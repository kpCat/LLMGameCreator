using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed partial class ExtensionRulePackValidator
{
    private const int MaxFormulaLength = 160;

    private static readonly IReadOnlySet<string> AllowedTriggerTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "trigger/runtime_interact_completed",
        "trigger/on_goal_completed",
        "trigger/on_reward_granted"
    };

    private static readonly IReadOnlySet<string> AllowedConditionTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "condition/has_inventory_item",
        "condition/flag_equals",
        "condition/quest_objective_completed"
    };

    private static readonly IReadOnlySet<string> AllowedActionTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "action/grant_item",
        "action/advance_objective",
        "action/set_flag"
    };

    private static readonly IReadOnlySet<string> AllowedRewardTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "reward/item"
    };

    private static readonly IReadOnlySet<string> AllowedObjectiveTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "objective/inventory_item",
        "objective/flag"
    };

    private static readonly IReadOnlySet<string> AllowedApiCalls = new HashSet<string>(StringComparer.Ordinal)
    {
        "runtime.rule_pack.apply_declared_actions"
    };

    private static readonly IReadOnlySet<string> AllowedMutationTargets = new HashSet<string>(StringComparer.Ordinal)
    {
        "runtime.inventory",
        "runtime.quest_objective",
        "runtime.flag"
    };

    public ExtensionRulePackValidationReport Validate(ExtensionRulePack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);

        var diagnostics = new List<ExtensionRulePackDiagnostic>();
        var triggerIds = pack.Triggers.Select(item => item.TriggerId).ToHashSet(StringComparer.Ordinal);
        var conditionIds = pack.Conditions.Select(item => item.ConditionId).ToHashSet(StringComparer.Ordinal);
        var formulaIds = pack.Formulas.Select(item => item.FormulaId).ToHashSet(StringComparer.Ordinal);
        var actionIds = pack.Actions.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);
        var rewardIds = pack.Rewards.Select(item => item.RewardId).ToHashSet(StringComparer.Ordinal);
        var objectiveIds = pack.QuestObjectives.Select(item => item.ObjectiveId).ToHashSet(StringComparer.Ordinal);

        ValidateId(pack.Metadata.RulePackId, "rulePack", diagnostics);
        CheckDuplicates("trigger", pack.Triggers.Select(item => item.TriggerId), diagnostics);
        CheckDuplicates("condition", pack.Conditions.Select(item => item.ConditionId), diagnostics);
        CheckDuplicates("formula", pack.Formulas.Select(item => item.FormulaId), diagnostics);
        CheckDuplicates("action", pack.Actions.Select(item => item.ActionId), diagnostics);
        CheckDuplicates("reward", pack.Rewards.Select(item => item.RewardId), diagnostics);
        CheckDuplicates("objective", pack.QuestObjectives.Select(item => item.ObjectiveId), diagnostics);
        CheckDuplicates("rule", pack.Rules.Select(item => item.RuleId), diagnostics);

        foreach (var trigger in pack.Triggers)
        {
            ValidateId(trigger.TriggerId, "trigger", diagnostics);
            ValidateAllowed(trigger.TriggerType, AllowedTriggerTypes, trigger.TriggerId, "trigger.type", diagnostics);
            ValidateApiCalls(trigger.ApiCalls, trigger.TriggerId, diagnostics);
        }

        foreach (var condition in pack.Conditions)
        {
            ValidateId(condition.ConditionId, "condition", diagnostics);
            ValidateAllowed(condition.ConditionType, AllowedConditionTypes, condition.ConditionId, "condition.type", diagnostics);
            ValidateOptionalRef(condition.FormulaId, formulaIds, condition.ConditionId, "rule_pack.unknown_formula_ref", diagnostics);
            ValidateSafeValue(condition.TargetRef, condition.ConditionId, "condition.target_ref", diagnostics);
            ValidateParameters(condition.Parameters, condition.ConditionId, diagnostics);
            ValidateApiCalls(condition.ApiCalls, condition.ConditionId, diagnostics);
        }

        foreach (var formula in pack.Formulas)
        {
            ValidateId(formula.FormulaId, "formula", diagnostics);
            ValidateFormula(formula, diagnostics);
            ValidateApiCalls(formula.ApiCalls, formula.FormulaId, diagnostics);
        }

        foreach (var action in pack.Actions)
        {
            ValidateId(action.ActionId, "action", diagnostics);
            ValidateAllowed(action.ActionType, AllowedActionTypes, action.ActionId, "action.type", diagnostics);
            ValidateOptionalRef(action.FormulaId, formulaIds, action.ActionId, "rule_pack.unknown_formula_ref", diagnostics);
            ValidateSafeValue(action.TargetRef, action.ActionId, "action.target_ref", diagnostics);
            ValidateParameters(action.Parameters, action.ActionId, diagnostics);
            ValidateApiCalls(action.ApiCalls, action.ActionId, diagnostics);
            ValidateMutations(action.MutationTargets, action.ActionId, diagnostics);
        }

        foreach (var reward in pack.Rewards)
        {
            ValidateId(reward.RewardId, "reward", diagnostics);
            ValidateAllowed(reward.RewardType, AllowedRewardTypes, reward.RewardId, "reward.type", diagnostics);
            ValidateSafeValue(reward.TargetRef, reward.RewardId, "reward.target_ref", diagnostics);
            ValidateParameters(reward.Parameters, reward.RewardId, diagnostics);
        }

        foreach (var objective in pack.QuestObjectives)
        {
            ValidateId(objective.ObjectiveId, "objective", diagnostics);
            ValidateAllowed(objective.ObjectiveType, AllowedObjectiveTypes, objective.ObjectiveId, "objective.type", diagnostics);
            ValidateSafeValue(objective.TargetRef, objective.ObjectiveId, "objective.target_ref", diagnostics);
            ValidateParameters(objective.Parameters, objective.ObjectiveId, diagnostics);
        }

        foreach (var rule in pack.Rules)
        {
            ValidateId(rule.RuleId, "rule", diagnostics);
            ValidateRequiredRef(rule.TriggerId, triggerIds, rule.RuleId, "rule_pack.unknown_trigger_ref", diagnostics);
            foreach (var conditionId in rule.ConditionIds)
            {
                ValidateRequiredRef(conditionId, conditionIds, rule.RuleId, "rule_pack.unknown_condition_ref", diagnostics);
            }

            foreach (var actionId in rule.ActionIds)
            {
                ValidateRequiredRef(actionId, actionIds, rule.RuleId, "rule_pack.unknown_action_ref", diagnostics);
            }

            foreach (var rewardId in rule.RewardIds)
            {
                ValidateRequiredRef(rewardId, rewardIds, rule.RuleId, "rule_pack.unknown_reward_ref", diagnostics);
            }

            foreach (var objectiveId in rule.ObjectiveIds)
            {
                ValidateRequiredRef(objectiveId, objectiveIds, rule.RuleId, "rule_pack.unknown_objective_ref", diagnostics);
            }

            if (rule.ActionIds.Count == 0 && rule.RewardIds.Count == 0 && rule.ObjectiveIds.Count == 0)
            {
                Add(diagnostics, "error", "rule_pack.empty_rule_effect", rule.RuleId, "Rule must reference at least one action, reward or objective declaration.");
            }
        }

        var sorted = SortDiagnostics(diagnostics);
        return new ExtensionRulePackValidationReport
        {
            SchemaVersion = ExtensionRulePackConstants.SchemaVersion,
            RulePackId = pack.Metadata.RulePackId,
            HasErrors = sorted.Any(item => item.Severity == "error"),
            DiagnosticCount = sorted.Count,
            Diagnostics = sorted
        };
    }

    private static void ValidateFormula(ExtensionFormulaDefinition formula, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(formula.Expression))
        {
            Add(diagnostics, "error", "extension_formula.expression.empty", formula.FormulaId, "Formula expression is required.");
            return;
        }

        if (formula.Expression.Length > MaxFormulaLength)
        {
            Add(diagnostics, "error", "extension_formula.expression.too_long", formula.FormulaId, $"Formula expression must not exceed {MaxFormulaLength} characters.");
        }

        if (LooksUnsafeFormula(formula.Expression))
        {
            Add(diagnostics, "error", "extension_formula.expression.unsafe", formula.FormulaId, "Formula expression contains unsafe or code-looking text.");
            return;
        }

        var variables = formula.DeclaredVariables.ToHashSet(StringComparer.Ordinal);
        foreach (var variable in variables)
        {
            if (!VariableNameRegex().IsMatch(variable))
            {
                Add(diagnostics, "error", "extension_formula.variable.unsafe", formula.FormulaId, $"Formula variable '{variable}' is unsafe.");
            }
        }

        foreach (var token in FormulaIdentifierRegex().Matches(formula.Expression).Select(match => match.Value).Distinct(StringComparer.Ordinal))
        {
            if (!variables.Contains(token))
            {
                Add(diagnostics, "error", "extension_formula.expression.unknown_variable", formula.FormulaId, $"Formula expression references undeclared variable '{token}'.");
            }
        }
    }

    private static bool LooksUnsafeFormula(string expression)
    {
        if (expression.Any(character => !(char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || character is '_' or '+' or '-' or '*' or '/' or '(' or ')' or '.')))
        {
            return true;
        }

        var codeLookingTokens = new[]
        {
            "using", "namespace", "class", "new", "return", "system", "reflection", "process", "file", "directory", "lua", "load", "io", "os", "debug"
        };

        return expression.Contains("../", StringComparison.Ordinal)
               || expression.Contains("..\\", StringComparison.Ordinal)
               || FormulaIdentifierRegex().Matches(expression)
                   .Select(match => match.Value)
                   .Any(token => codeLookingTokens.Contains(token, StringComparer.OrdinalIgnoreCase));
    }

    private static void ValidateParameters(IReadOnlyDictionary<string, string> parameters, string target, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        foreach (var pair in parameters.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (!ParameterKeyRegex().IsMatch(pair.Key))
            {
                Add(diagnostics, "error", "extension_rule_pack.unsafe_parameter_key", target, $"Parameter key '{pair.Key}' is unsafe.");
            }

            ValidateSafeValue(pair.Value, target, "parameter." + pair.Key, diagnostics);
        }
    }

    private static void ValidateApiCalls(IReadOnlyList<string> apiCalls, string target, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        foreach (var apiCall in apiCalls)
        {
            if (!AllowedApiCalls.Contains(apiCall))
            {
                Add(diagnostics, "error", "extension_rule_pack.unknown_api_call", target, $"API call '{apiCall}' is not allowed in declaration-level rule packs.");
            }
        }
    }

    private static void ValidateMutations(IReadOnlyList<string> mutationTargets, string target, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        foreach (var mutation in mutationTargets)
        {
            if (!AllowedMutationTargets.Contains(mutation))
            {
                Add(diagnostics, "error", "extension_rule_pack.unsupported_mutation", target, $"Mutation target '{mutation}' is not supported by extension rule pack v1.");
            }
        }
    }

    private static void ValidateSafeValue(string value, string target, string field, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Length > 180 ||
            value.Contains('\\') ||
            value.Contains(':') ||
            value.Contains("../", StringComparison.Ordinal) ||
            value.Contains("..\\", StringComparison.Ordinal))
        {
            Add(diagnostics, "error", "extension_rule_pack.unsafe_path", target, $"{field} contains unsafe path-like text.");
        }
    }

    private static void ValidateRequiredRef(string id, IReadOnlySet<string> knownIds, string target, string code, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !knownIds.Contains(id))
        {
            Add(diagnostics, "error", code, target, $"Referenced id '{id}' was not found.");
        }
    }

    private static void ValidateOptionalRef(string id, IReadOnlySet<string> knownIds, string target, string code, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (!string.IsNullOrWhiteSpace(id) && !knownIds.Contains(id))
        {
            Add(diagnostics, "error", code, target, $"Referenced id '{id}' was not found.");
        }
    }

    private static void ValidateAllowed(string value, IReadOnlySet<string> allowed, string target, string category, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (!allowed.Contains(value))
        {
            Add(diagnostics, "error", "extension_rule_pack.unsupported_" + category, target, $"Type '{value}' is not supported by extension rule pack v1.");
        }
    }

    private static void ValidateId(string id, string category, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        if (!IsSafeId(id))
        {
            Add(diagnostics, "error", "extension_rule_pack.unsafe_id", id, $"{category} id is empty, too long or contains unsafe characters.");
        }
    }

    private static bool IsSafeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 160 || id.Contains('\\') || id.Contains(':'))
        {
            return false;
        }

        if (id.StartsWith('/') || id.EndsWith('/'))
        {
            return false;
        }

        var segments = id.Split('/', StringSplitOptions.None);
        return segments.All(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not "..") &&
               id.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/');
    }

    private static void CheckDuplicates(string kind, IEnumerable<string> ids, ICollection<ExtensionRulePackDiagnostic> diagnostics)
    {
        foreach (var group in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            Add(diagnostics, "error", "extension_rule_pack.duplicate_id", group.Key, $"Duplicate {kind} id '{group.Key}' was found.");
        }
    }

    private static IReadOnlyList<ExtensionRulePackDiagnostic> SortDiagnostics(IEnumerable<ExtensionRulePackDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void Add(ICollection<ExtensionRulePackDiagnostic> diagnostics, string severity, string code, string target, string message) =>
        diagnostics.Add(new ExtensionRulePackDiagnostic
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        });

    [GeneratedRegex("[A-Za-z_][A-Za-z0-9_]*", RegexOptions.CultureInvariant)]
    private static partial Regex FormulaIdentifierRegex();

    [GeneratedRegex("^[a-z][a-z0-9_]{0,63}$", RegexOptions.CultureInvariant)]
    private static partial Regex VariableNameRegex();

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{0,63}$", RegexOptions.CultureInvariant)]
    private static partial Regex ParameterKeyRegex();
}

public sealed class ExtensionSpineScenarioHarnessService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/extension-spine";
    public const string ScenarioReportJsonFileName = "extension-spine-scenario-report.json";
    public const string ScenarioReportMarkdownFileName = "extension-spine-scenario-report.md";
    public const string ProofPackJsonFileName = "extension-proof-rule-pack.json";
    public const string ProofValidationJsonFileName = "extension-proof-validation-report.json";
    public const string InvalidValidationJsonFileName = "invalid-extension-validation-report.json";
    public const string ManualVerificationMarkdownFileName = "manual-extension-spine-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly VisibleGeneratedPlayablePreviewService _visiblePreviewService;
    private readonly RuntimeBackedMicrogameStateAcceptanceService _runtimeBackedStateAcceptanceService;
    private readonly GenerationPresetOptionsService _generationOptionsService;
    private readonly ExtensionRulePackValidator _validator;

    public ExtensionSpineScenarioHarnessService(
        VisibleGeneratedPlayablePreviewService? visiblePreviewService = null,
        RuntimeBackedMicrogameStateAcceptanceService? runtimeBackedStateAcceptanceService = null,
        GenerationPresetOptionsService? generationOptionsService = null,
        ExtensionRulePackValidator? validator = null)
    {
        _visiblePreviewService = visiblePreviewService ?? new VisibleGeneratedPlayablePreviewService();
        _runtimeBackedStateAcceptanceService = runtimeBackedStateAcceptanceService ?? new RuntimeBackedMicrogameStateAcceptanceService();
        _generationOptionsService = generationOptionsService ?? new GenerationPresetOptionsService();
        _validator = validator ?? new ExtensionRulePackValidator();
    }

    public ExtensionSpineScenarioHarnessResult Run(string? projectRootPath = null)
    {
        var proofPack = BuildProofPack();
        var invalidPack = BuildInvalidProofPack();
        var proofValidation = _validator.Validate(proofPack);
        var invalidValidation = _validator.Validate(invalidPack);
        var baseScenario = RunScenario(
            "base",
            new MicrogameVariationAcceptanceRequest
            {
                Seed = "goal003-base-runtime-backed-loop",
                PresetId = GenerationPresetOptionsService.DefaultPresetId
            },
            null,
            proofValidation,
            projectRootPath);
        var extensionScenario = RunScenario(
            "extension_inventory_objective",
            new MicrogameVariationAcceptanceRequest
            {
                Seed = "goal003-extension-inventory-objective",
                PresetId = "recover_resource"
            },
            proofPack,
            proofValidation,
            projectRootPath);
        var scenarios = new[] { baseScenario, extensionScenario };
        var extensionChangedBehavior = extensionScenario.ExtensionEvidence.Consumed
                                       && extensionScenario.ExtensionEvidence.InventoryObjectiveCompleted
                                       && extensionScenario.ExtensionEvidence.AdditionalRewardGranted
                                       && !string.Equals(
                                           baseScenario.ExtensionEvidence.AddedRewardItemId,
                                           extensionScenario.ExtensionEvidence.AddedRewardItemId,
                                           StringComparison.Ordinal);
        var invalidRejected = invalidValidation.HasErrors
                              && invalidValidation.Diagnostics.Any(item => item.Code == "extension_rule_pack.unknown_api_call")
                              && invalidValidation.Diagnostics.Any(item => item.Code == "extension_rule_pack.unsupported_mutation")
                              && invalidValidation.Diagnostics.Any(item => item.Code == "extension_formula.expression.unsafe");
        var accepted = scenarios.All(item => item.Accepted)
                       && !proofValidation.HasErrors
                       && extensionChangedBehavior
                       && invalidRejected;
        var diagnostics = new List<ExtensionSpineDiagnostic>
        {
            Diagnostic("info", "extension_spine.no_external_execution", "harness", "No LLM, provider, Lua, Unity or media execution was invoked."),
            Diagnostic("info", "extension_spine.csharp_scope", "harness", "C# changes are limited to declaration validation, deterministic harnessing and generic runtime-state action primitives."),
            Diagnostic(
                extensionChangedBehavior ? "info" : "error",
                extensionChangedBehavior ? "extension_spine.extension_changed_behavior" : "extension_spine.extension_did_not_change_behavior",
                extensionScenario.ScenarioId,
                extensionChangedBehavior ? "Extension rule pack changed runtime-backed inventory objective and reward evidence." : "Extension rule pack did not produce distinct accepted runtime-backed evidence."),
            Diagnostic(
                invalidRejected ? "info" : "error",
                invalidRejected ? "extension_spine.invalid_extension_rejected" : "extension_spine.invalid_extension_not_rejected",
                invalidPack.Metadata.RulePackId,
                invalidRejected ? "Invalid extension rule pack was rejected by declaration-level validation." : "Invalid extension rule pack did not produce required validation errors."),
            Diagnostic("info", "extension_spine.manual_verification_required", "manual_extension_spine_verification", "Codex acceptance is headless; the next and only manual gate for this goal is manual extension spine verification.")
        };

        var reportWithoutHash = new ExtensionSpineScenarioReport
        {
            Accepted = accepted,
            ManualGate = "manual_extension_spine_verification",
            ProofRulePackId = proofPack.Metadata.RulePackId,
            InvalidRulePackRejected = invalidRejected,
            ExtensionChangedBehavior = extensionChangedBehavior,
            WhatIsDataExtensible =
            [
                "triggers",
                "conditions",
                "formulas",
                "actions",
                "rewards",
                "quest objectives",
                "inventory-objective reward variation"
            ],
            WhatStillRequiresCSharpPrimitive =
            [
                "new runtime command families",
                "new mutable runtime state containers",
                "new formula evaluator semantics",
                "new rendering or UI interaction modes",
                "new external providers or Lua execution"
            ],
            Scenarios = scenarios,
            ProofValidation = proofValidation,
            InvalidValidation = invalidValidation,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var hash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions));
        var report = reportWithoutHash with { DeterministicHash = hash };

        return new ExtensionSpineScenarioHarnessResult
        {
            Report = report,
            ProofRulePack = proofPack,
            InvalidRulePack = invalidPack,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            ProofRulePackJson = JsonSerializer.Serialize(proofPack, JsonOptions),
            ProofValidationJson = JsonSerializer.Serialize(proofValidation, JsonOptions),
            InvalidValidationJson = JsonSerializer.Serialize(invalidValidation, JsonOptions),
            ManualVerificationMarkdown = RenderManualVerification(report)
        };
    }

    public async Task<ExtensionSpineScenarioHarnessWriteResult> WriteAsync(
        string projectRootPath,
        ExtensionSpineScenarioHarnessResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "extension-spine"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ScenarioReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ScenarioReportMarkdownFileName));
        var proofRulePackJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ProofPackJsonFileName));
        var proofValidationJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ProofValidationJsonFileName));
        var invalidValidationJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, InvalidValidationJsonFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        foreach (var path in new[]
                 {
                     reportJsonPath,
                     reportMarkdownPath,
                     proofRulePackJsonPath,
                     proofValidationJsonPath,
                     invalidValidationJsonPath,
                     manualVerificationMarkdownPath
                 })
        {
            EnsureContained(outputDirectory, path);
        }

        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(proofRulePackJsonPath, result.ProofRulePackJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(proofValidationJsonPath, result.ProofValidationJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(invalidValidationJsonPath, result.InvalidValidationJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new ExtensionSpineScenarioHarnessWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ProofRulePackJsonPath = proofRulePackJsonPath,
            ProofValidationJsonPath = proofValidationJsonPath,
            InvalidValidationJsonPath = invalidValidationJsonPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    public static ExtensionRulePack BuildProofPack() => new()
    {
        Metadata = new ExtensionRulePackMetadata
        {
            RulePackId = "rule_pack/extension_spine_inventory_objective_v1",
            DisplayName = "Inventory objective proof pack",
            AuthoringMode = "data_rule_pack"
        },
        Triggers =
        [
            new ExtensionTriggerDefinition
            {
                TriggerId = "trigger/runtime_interact_completed",
                TriggerType = "trigger/runtime_interact_completed"
            }
        ],
        Conditions =
        [
            new ExtensionConditionDefinition
            {
                ConditionId = "condition/reward_item_visible",
                ConditionType = "condition/has_inventory_item",
                TargetRef = "reward/runtime_reward_item",
                Parameters = SortedParameters(("minimumAmount", "1"))
            }
        ],
        Formulas =
        [
            new ExtensionFormulaDefinition
            {
                FormulaId = "formula/extension_bonus_count",
                Expression = "base_reward_count + extension_bonus",
                DeclaredVariables = ["base_reward_count", "extension_bonus"],
                MinimumValue = 1,
                MaximumValue = 3
            }
        ],
        Actions =
        [
            new ExtensionActionDefinition
            {
                ActionId = "action/grant_extension_badge",
                ActionType = "action/grant_item",
                TargetRef = "item/extension_spine_badge",
                FormulaId = "formula/extension_bonus_count",
                Parameters = SortedParameters(("amount", "1"), ("questItem", "true")),
                MutationTargets = ["runtime.inventory"]
            },
            new ExtensionActionDefinition
            {
                ActionId = "action/complete_inventory_objective",
                ActionType = "action/advance_objective",
                TargetRef = "objective/collect_extension_badge",
                Parameters = SortedParameters(("amount", "1"), ("questId", "quest/extension_spine_inventory_objective")),
                MutationTargets = ["runtime.quest_objective"]
            },
            new ExtensionActionDefinition
            {
                ActionId = "action/mark_extension_rule_applied",
                ActionType = "action/set_flag",
                TargetRef = "flag/extension_spine_rule_applied",
                Parameters = SortedParameters(("value", "true")),
                MutationTargets = ["runtime.flag"]
            }
        ],
        Rewards =
        [
            new ExtensionRewardDefinition
            {
                RewardId = "reward/extension_spine_badge",
                RewardType = "reward/item",
                TargetRef = "item/extension_spine_badge",
                Parameters = SortedParameters(("amount", "1"))
            }
        ],
        QuestObjectives =
        [
            new ExtensionQuestObjectiveDefinition
            {
                ObjectiveId = "objective/collect_extension_badge",
                ObjectiveType = "objective/inventory_item",
                TargetRef = "item/extension_spine_badge",
                RequiredAmount = 1,
                Parameters = SortedParameters(("questId", "quest/extension_spine_inventory_objective"))
            }
        ],
        Rules =
        [
            new ExtensionRuleDefinition
            {
                RuleId = "rule/extension_inventory_objective_on_interact",
                TriggerId = "trigger/runtime_interact_completed",
                ConditionIds = ["condition/reward_item_visible"],
                ActionIds =
                [
                    "action/grant_extension_badge",
                    "action/complete_inventory_objective",
                    "action/mark_extension_rule_applied"
                ],
                RewardIds = ["reward/extension_spine_badge"],
                ObjectiveIds = ["objective/collect_extension_badge"]
            }
        ]
    };

    public static ExtensionRulePack BuildInvalidProofPack() => BuildProofPack() with
    {
        Metadata = new ExtensionRulePackMetadata
        {
            RulePackId = "rule_pack/invalid_extension_spine_v1",
            DisplayName = "Invalid extension proof pack",
            AuthoringMode = "data_rule_pack"
        },
        Formulas =
        [
            new ExtensionFormulaDefinition
            {
                FormulaId = "formula/invalid",
                Expression = "System.IO.File + missing_value",
                DeclaredVariables = ["known_value"],
                ApiCalls = ["runtime.exec_arbitrary_lua"]
            }
        ],
        Actions =
        [
            new ExtensionActionDefinition
            {
                ActionId = "../bad",
                ActionType = "action/run_lua",
                TargetRef = "..\\outside",
                FormulaId = "formula/missing",
                Parameters = SortedParameters(("outputPath", "..\\outside\\package.json")),
                ApiCalls = ["runtime.exec_arbitrary_lua"],
                MutationTargets = ["runtime.direct_game_state"]
            }
        ],
        Rules =
        [
            new ExtensionRuleDefinition
            {
                RuleId = "rule/invalid",
                TriggerId = "trigger/missing",
                ActionIds = ["../bad"]
            }
        ]
    };

    private ExtensionSpineScenario RunScenario(
        string scenarioId,
        MicrogameVariationAcceptanceRequest request,
        ExtensionRulePack? extensionRulePack,
        ExtensionRulePackValidationReport proofValidation,
        string? projectRootPath)
    {
        var generationOptions = _generationOptionsService.Resolve(new GenerationPresetOptionsRequest
        {
            Seed = request.Seed,
            Mode = request.Mode,
            PresetId = request.PresetId,
            CompactStyleHintIds = request.CompactStyleHintIds,
            SelectedVariantIds = request.SelectedVariantIds
        });
        var visibleResult = _visiblePreviewService.Generate(new VisibleGeneratedPlayablePreviewRequest
        {
            Seed = generationOptions.Seed,
            Mode = generationOptions.Mode,
            PresetId = generationOptions.PresetId,
            CompactStyleHintIds = generationOptions.CompactStyleHintIds,
            SelectedVariantIds = generationOptions.SelectedVariantIds
        });
        var runtimeAcceptance = _runtimeBackedStateAcceptanceService.Build(visibleResult, projectRootPath);
        var baseAccepted = visibleResult.Report.RuntimeStartSucceeded
                           && visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "move/right", StringComparison.OrdinalIgnoreCase) && item.Succeeded)
                           && visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "interact", StringComparison.OrdinalIgnoreCase) && item.Succeeded)
                           && visibleResult.Report.ActiveGoalSelected
                           && visibleResult.Report.GoalProgressAdvanced
                           && visibleResult.Report.RewardVisible
                           && visibleResult.Report.CompletionVisible
                           && string.Equals(runtimeAcceptance.Snapshot.GoalProgressStateSource, "runtime_state_quests", StringComparison.Ordinal)
                           && string.Equals(runtimeAcceptance.Snapshot.ChallengeStateSource, "runtime_state_flags_inventory_encounter", StringComparison.Ordinal)
                           && runtimeAcceptance.Snapshot.RuntimeRewardGranted
                           && runtimeAcceptance.Snapshot.RuntimeCompletionBacked;
        var extensionEvidence = extensionRulePack is null
            ? ExtensionRuntimeEvidence.None
            : ApplyExtensionRulePack(extensionRulePack, proofValidation, visibleResult);

        return new ExtensionSpineScenario
        {
            ScenarioId = scenarioId,
            Accepted = baseAccepted && (extensionRulePack is null || extensionEvidence.Consumed),
            GenerationOptions = generationOptions,
            PackageId = visibleResult.Snapshot.PackageId,
            PackageTitle = visibleResult.Snapshot.PackageTitle,
            RuntimeStartSucceeded = visibleResult.Report.RuntimeStartSucceeded,
            RuntimeMoveSucceeded = runtimeAcceptance.Snapshot.RuntimeMoveSucceeded,
            RuntimeInteractSucceeded = runtimeAcceptance.Snapshot.RuntimeInteractSucceeded,
            GoalProgressAdvanced = runtimeAcceptance.Snapshot.ProgressAdvanced,
            RuntimeRewardGranted = runtimeAcceptance.Snapshot.RuntimeRewardGranted,
            RuntimeCompletionBacked = runtimeAcceptance.Snapshot.RuntimeCompletionBacked,
            GoalProgressStateSource = runtimeAcceptance.Snapshot.GoalProgressStateSource,
            ChallengeStateSource = runtimeAcceptance.Snapshot.ChallengeStateSource,
            BaseRewardItemId = runtimeAcceptance.Snapshot.RuntimeRewardItemId,
            ExtensionEvidence = extensionEvidence,
            SnapshotHash = visibleResult.Snapshot.DeterministicHash,
            RuntimeBackedStateHash = runtimeAcceptance.Snapshot.DeterministicHash
        };
    }

    private static ExtensionRuntimeEvidence ApplyExtensionRulePack(
        ExtensionRulePack rulePack,
        ExtensionRulePackValidationReport validation,
        VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        if (validation.HasErrors)
        {
            return ExtensionRuntimeEvidence.None with
            {
                RulePackId = rulePack.Metadata.RulePackId,
                Consumed = false,
                StateSource = "validation_failed"
            };
        }

        var state = CloneRuntimeState(visibleResult.Snapshot.MicrogameChallenge.RuntimeState);
        if (string.IsNullOrWhiteSpace(state.PackageId))
        {
            state.PackageId = visibleResult.Snapshot.PackageId;
        }

        var quest = EnsureQuest(state, "quest/extension_spine_inventory_objective");
        var objective = EnsureObjective(quest, "objective/collect_extension_badge", "item/extension_spine_badge");
        var inventory = EnsurePlayerInventory(state);
        var appliedActionIds = new SortedSet<string>(StringComparer.Ordinal);
        var addedRewardItemId = string.Empty;
        var addedRewardAmount = 0d;
        var flagId = string.Empty;

        foreach (var rule in rulePack.Rules.OrderBy(item => item.RuleId, StringComparer.Ordinal))
        {
            foreach (var actionId in rule.ActionIds.OrderBy(id => id, StringComparer.Ordinal))
            {
                var action = rulePack.Actions.FirstOrDefault(item => item.ActionId == actionId);
                if (action is null)
                {
                    continue;
                }

                switch (action.ActionType)
                {
                    case "action/grant_item":
                        var amount = ParseDouble(action.Parameters, "amount", 1);
                        AddItem(inventory, action.TargetRef, amount);
                        addedRewardItemId = action.TargetRef;
                        addedRewardAmount += amount;
                        appliedActionIds.Add(action.ActionId);
                        break;
                    case "action/advance_objective":
                        objective.CurrentAmount = Math.Min(objective.RequiredAmount, objective.CurrentAmount + ParseDouble(action.Parameters, "amount", 1));
                        objective.Completed = objective.CurrentAmount >= objective.RequiredAmount;
                        appliedActionIds.Add(action.ActionId);
                        break;
                    case "action/set_flag":
                        flagId = action.TargetRef;
                        SetFlag(state, flagId, action.Parameters.TryGetValue("value", out var value) ? value : "true");
                        appliedActionIds.Add(action.ActionId);
                        break;
                }
            }
        }

        quest.State = objective.Completed ? "completed" : "active";
        quest.CompletedTick = objective.Completed ? state.Tick : null;
        state.Metadata["extension_spine.rule_pack_id"] = rulePack.Metadata.RulePackId;
        state.Metadata["extension_spine.consumed"] = "true";

        return new ExtensionRuntimeEvidence
        {
            Consumed = appliedActionIds.Count > 0,
            RulePackId = rulePack.Metadata.RulePackId,
            StateSource = "validated_rule_pack_existing_runtime_state",
            AppliedActionIds = appliedActionIds.ToList(),
            AddedRewardItemId = addedRewardItemId,
            AddedRewardAmount = addedRewardAmount,
            AddedObjectiveId = objective.ObjectiveId,
            AddedQuestId = quest.QuestId,
            AddedFlagId = flagId,
            InventoryObjectiveCompleted = objective.Completed,
            AdditionalRewardGranted = addedRewardAmount > 0,
            RuntimeStateMetadata = new SortedDictionary<string, string>(state.Metadata, StringComparer.Ordinal)
        };
    }

    private static string RenderReport(ExtensionSpineScenarioReport report)
    {
        var lines = new List<string>
        {
            "# Extension Spine Scenario Report",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Accepted: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Snapshot hash: `{report.DeterministicHash}`",
            $"- Manual gate: `{report.ManualGate}`",
            $"- Proof rule pack: `{report.ProofRulePackId}`",
            $"- Extension changed behavior: `{report.ExtensionChangedBehavior.ToString().ToLowerInvariant()}`",
            $"- Invalid extension rejected: `{report.InvalidRulePackRejected.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Scenarios",
            string.Empty
        };

        foreach (var scenario in report.Scenarios)
        {
            lines.Add($"### {scenario.ScenarioId}");
            lines.Add(string.Empty);
            lines.Add($"- Accepted: `{scenario.Accepted.ToString().ToLowerInvariant()}`");
            lines.Add($"- Seed/preset: `{scenario.GenerationOptions.Seed}` / `{scenario.GenerationOptions.PresetId}`");
            lines.Add($"- Package: `{scenario.PackageTitle}` / `{scenario.PackageId}`");
            lines.Add($"- Runtime path: start=`{scenario.RuntimeStartSucceeded.ToString().ToLowerInvariant()}`, move=`{scenario.RuntimeMoveSucceeded.ToString().ToLowerInvariant()}`, interact=`{scenario.RuntimeInteractSucceeded.ToString().ToLowerInvariant()}`");
            lines.Add($"- Goal/reward/completion: progress=`{scenario.GoalProgressAdvanced.ToString().ToLowerInvariant()}`, reward=`{scenario.RuntimeRewardGranted.ToString().ToLowerInvariant()}`, completion=`{scenario.RuntimeCompletionBacked.ToString().ToLowerInvariant()}`");
            lines.Add($"- Extension consumed: `{scenario.ExtensionEvidence.Consumed.ToString().ToLowerInvariant()}`");
            if (scenario.ExtensionEvidence.Consumed)
            {
                lines.Add($"- Extension reward/objective: `{scenario.ExtensionEvidence.AddedRewardItemId}` / `{scenario.ExtensionEvidence.AddedObjectiveId}`");
            }

            lines.Add(string.Empty);
        }

        lines.Add("## Data Extensible");
        lines.Add(string.Empty);
        lines.AddRange(report.WhatIsDataExtensible.Select(item => "- `" + item + "`"));
        lines.Add(string.Empty);
        lines.Add("## Requires C# Primitive");
        lines.Add(string.Empty);
        lines.AddRange(report.WhatStillRequiresCSharpPrimitive.Select(item => "- `" + item + "`"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderManualVerification(ExtensionSpineScenarioReport report)
    {
        var lines = new List<string>
        {
            "# Manual Extension Spine Verification",
            string.Empty,
            "Use this after Goal 003. Codex stops here and does not perform this manual UI check.",
            string.Empty,
            "1. Review `.llmgc/procedural/extension-spine/extension-spine-scenario-report.json`.",
            "2. Confirm the base scenario and extension scenario are both accepted.",
            "3. Confirm the extension scenario records `validated_rule_pack_existing_runtime_state`.",
            "4. Confirm invalid extension validation contains errors.",
            "5. If desired, run Runtime Preview with the extension scenario seed/preset and compare generated labels with the report.",
            string.Empty,
            $"Headless acceptance status: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"Next state marker: `{report.ManualGate}`"
        };

        return string.Join("\n", lines) + "\n";
    }

    private static InventoryState EnsurePlayerInventory(GameRuntimeState state)
    {
        var inventory = state.Inventories.FirstOrDefault(item => string.Equals(item.Id, "inventory/player", StringComparison.OrdinalIgnoreCase))
                        ?? state.Inventories.FirstOrDefault(item => string.Equals(item.OwnerKind, "player", StringComparison.OrdinalIgnoreCase));
        if (inventory != null)
        {
            return inventory;
        }

        inventory = new InventoryState
        {
            Id = "inventory/player",
            OwnerKind = "player",
            OwnerId = state.PlayerEntityId
        };
        state.Inventories.Add(inventory);
        return inventory;
    }

    private static QuestRuntimeState EnsureQuest(GameRuntimeState state, string questId)
    {
        var quest = state.Quests.FirstOrDefault(item => string.Equals(item.QuestId, questId, StringComparison.Ordinal));
        if (quest != null)
        {
            return quest;
        }

        quest = new QuestRuntimeState
        {
            QuestId = questId,
            State = "active",
            StartedTick = state.Tick,
            Metadata = new Dictionary<string, string>
            {
                ["extension_spine.declared_by_rule_pack"] = "true"
            }
        };
        state.Quests.Add(quest);
        return quest;
    }

    private static QuestObjectiveRuntimeState EnsureObjective(QuestRuntimeState quest, string objectiveId, string targetId)
    {
        var objective = quest.Objectives.FirstOrDefault(item => string.Equals(item.ObjectiveId, objectiveId, StringComparison.Ordinal));
        if (objective != null)
        {
            return objective;
        }

        objective = new QuestObjectiveRuntimeState
        {
            ObjectiveId = objectiveId,
            Kind = "inventory_item",
            TargetId = targetId,
            RequiredAmount = 1,
            Metadata = new Dictionary<string, string>
            {
                ["extension_spine.declared_by_rule_pack"] = "true"
            }
        };
        quest.Objectives.Add(objective);
        return objective;
    }

    private static void AddItem(InventoryState inventory, string itemId, double amount)
    {
        var stack = inventory.Stacks.FirstOrDefault(item => string.Equals(item.ItemId, itemId, StringComparison.Ordinal));
        if (stack == null)
        {
            inventory.Stacks.Add(new ItemStackState
            {
                ItemId = itemId,
                Amount = amount,
                QuestItem = true,
                Metadata = new Dictionary<string, string>
                {
                    ["extension_spine.reward_source"] = "validated_rule_pack"
                }
            });
            return;
        }

        stack.Amount += amount;
    }

    private static void SetFlag(GameRuntimeState state, string id, string value)
    {
        var flag = state.Flags.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (flag == null)
        {
            state.Flags.Add(new RuntimeFlagState { Id = id, Value = value });
            return;
        }

        flag.Value = value;
    }

    private static GameRuntimeState CloneRuntimeState(GameRuntimeState source) => new()
    {
        PackageId = source.PackageId,
        CurrentMapId = source.CurrentMapId,
        PlayerEntityId = source.PlayerEntityId,
        Tick = source.Tick,
        Inventories = source.Inventories.Select(CloneInventory).ToList(),
        Equipment = source.Equipment.ToList(),
        Resources = source.Resources.ToList(),
        Progressions = source.Progressions.ToList(),
        Flags = source.Flags.Select(flag => new RuntimeFlagState { Id = flag.Id, Value = flag.Value }).ToList(),
        Statuses = source.Statuses.ToList(),
        ActiveEncounter = source.ActiveEncounter,
        QuestStates = new Dictionary<string, string>(source.QuestStates, StringComparer.Ordinal),
        Quests = source.Quests.Select(CloneQuest).ToList(),
        ActiveDialogue = source.ActiveDialogue,
        Factions = source.Factions.ToList(),
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static InventoryState CloneInventory(InventoryState source) => new()
    {
        Id = source.Id,
        OwnerKind = source.OwnerKind,
        OwnerId = source.OwnerId,
        Stacks = source.Stacks.Select(stack => new ItemStackState
        {
            ItemId = stack.ItemId,
            Amount = stack.Amount,
            UniqueInstanceId = stack.UniqueInstanceId,
            QuestItem = stack.QuestItem,
            Durability = stack.Durability,
            Charge = stack.Charge,
            Metadata = new Dictionary<string, string>(stack.Metadata, StringComparer.Ordinal)
        }).ToList(),
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static QuestRuntimeState CloneQuest(QuestRuntimeState source) => new()
    {
        QuestId = source.QuestId,
        State = source.State,
        CurrentStageId = source.CurrentStageId,
        Objectives = source.Objectives.Select(objective => new QuestObjectiveRuntimeState
        {
            ObjectiveId = objective.ObjectiveId,
            Kind = objective.Kind,
            TargetId = objective.TargetId,
            CurrentAmount = objective.CurrentAmount,
            RequiredAmount = objective.RequiredAmount,
            Completed = objective.Completed,
            Metadata = new Dictionary<string, string>(objective.Metadata, StringComparer.Ordinal)
        }).ToList(),
        StartedTick = source.StartedTick,
        CompletedTick = source.CompletedTick,
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static double ParseDouble(IReadOnlyDictionary<string, string> parameters, string key, double fallback) =>
        parameters.TryGetValue(key, out var value) && double.TryParse(value, out var parsed) ? parsed : fallback;

    private static IReadOnlyDictionary<string, string> SortedParameters(params (string Key, string Value)[] values) =>
        new SortedDictionary<string, string>(values.ToDictionary(item => item.Key, item => item.Value), StringComparer.Ordinal);

    private static IReadOnlyList<ExtensionSpineDiagnostic> SortDiagnostics(IEnumerable<ExtensionSpineDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static ExtensionSpineDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Extension spine output path must stay under the project root.");
        }
    }
}

public static class ExtensionRulePackConstants
{
    public const string SchemaVersion = "1";
}

public sealed record ExtensionRulePack
{
    public ExtensionRulePackMetadata Metadata { get; init; } = new();
    public IReadOnlyList<ExtensionTriggerDefinition> Triggers { get; init; } = Array.Empty<ExtensionTriggerDefinition>();
    public IReadOnlyList<ExtensionConditionDefinition> Conditions { get; init; } = Array.Empty<ExtensionConditionDefinition>();
    public IReadOnlyList<ExtensionFormulaDefinition> Formulas { get; init; } = Array.Empty<ExtensionFormulaDefinition>();
    public IReadOnlyList<ExtensionActionDefinition> Actions { get; init; } = Array.Empty<ExtensionActionDefinition>();
    public IReadOnlyList<ExtensionRewardDefinition> Rewards { get; init; } = Array.Empty<ExtensionRewardDefinition>();
    public IReadOnlyList<ExtensionQuestObjectiveDefinition> QuestObjectives { get; init; } = Array.Empty<ExtensionQuestObjectiveDefinition>();
    public IReadOnlyList<ExtensionRuleDefinition> Rules { get; init; } = Array.Empty<ExtensionRuleDefinition>();
}

public sealed record ExtensionRulePackMetadata
{
    public string SchemaVersion { get; init; } = ExtensionRulePackConstants.SchemaVersion;
    public string RulePackId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string AuthoringMode { get; init; } = "data_rule_pack";
}

public sealed record ExtensionTriggerDefinition
{
    public string TriggerId { get; init; } = string.Empty;
    public string TriggerType { get; init; } = string.Empty;
    public IReadOnlyList<string> ApiCalls { get; init; } = Array.Empty<string>();
}

public sealed record ExtensionConditionDefinition
{
    public string ConditionId { get; init; } = string.Empty;
    public string ConditionType { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string FormulaId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> ApiCalls { get; init; } = Array.Empty<string>();
}

public sealed record ExtensionFormulaDefinition
{
    public string FormulaId { get; init; } = string.Empty;
    public string Expression { get; init; } = string.Empty;
    public IReadOnlyList<string> DeclaredVariables { get; init; } = Array.Empty<string>();
    public string ResultType { get; init; } = "number";
    public decimal? MinimumValue { get; init; }
    public decimal? MaximumValue { get; init; }
    public IReadOnlyList<string> ApiCalls { get; init; } = Array.Empty<string>();
}

public sealed record ExtensionActionDefinition
{
    public string ActionId { get; init; } = string.Empty;
    public string ActionType { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string FormulaId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> ApiCalls { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MutationTargets { get; init; } = Array.Empty<string>();
}

public sealed record ExtensionRewardDefinition
{
    public string RewardId { get; init; } = string.Empty;
    public string RewardType { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record ExtensionQuestObjectiveDefinition
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveType { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public double RequiredAmount { get; init; } = 1;
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record ExtensionRuleDefinition
{
    public string RuleId { get; init; } = string.Empty;
    public string TriggerId { get; init; } = string.Empty;
    public IReadOnlyList<string> ConditionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RewardIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObjectiveIds { get; init; } = Array.Empty<string>();
}

public sealed record ExtensionRulePackValidationReport
{
    public string SchemaVersion { get; init; } = ExtensionRulePackConstants.SchemaVersion;
    public string RulePackId { get; init; } = string.Empty;
    public bool HasErrors { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<ExtensionRulePackDiagnostic> Diagnostics { get; init; } = Array.Empty<ExtensionRulePackDiagnostic>();
}

public sealed record ExtensionRulePackDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record ExtensionSpineScenarioHarnessResult
{
    public ExtensionSpineScenarioReport Report { get; init; } = new();
    public ExtensionRulePack ProofRulePack { get; init; } = new();
    public ExtensionRulePack InvalidRulePack { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ProofRulePackJson { get; init; } = string.Empty;
    public string ProofValidationJson { get; init; } = string.Empty;
    public string InvalidValidationJson { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record ExtensionSpineScenarioHarnessWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ProofRulePackJsonPath { get; init; } = string.Empty;
    public string ProofValidationJsonPath { get; init; } = string.Empty;
    public string InvalidValidationJsonPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record ExtensionSpineScenarioReport
{
    public string SchemaVersion { get; init; } = ExtensionRulePackConstants.SchemaVersion;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public string ProofRulePackId { get; init; } = string.Empty;
    public bool InvalidRulePackRejected { get; init; }
    public bool ExtensionChangedBehavior { get; init; }
    public IReadOnlyList<string> WhatIsDataExtensible { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> WhatStillRequiresCSharpPrimitive { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ExtensionSpineScenario> Scenarios { get; init; } = Array.Empty<ExtensionSpineScenario>();
    public ExtensionRulePackValidationReport ProofValidation { get; init; } = new();
    public ExtensionRulePackValidationReport InvalidValidation { get; init; } = new();
    public IReadOnlyList<ExtensionSpineDiagnostic> Diagnostics { get; init; } = Array.Empty<ExtensionSpineDiagnostic>();
}

public sealed record ExtensionSpineScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool RuntimeMoveSucceeded { get; init; }
    public bool RuntimeInteractSucceeded { get; init; }
    public bool GoalProgressAdvanced { get; init; }
    public bool RuntimeRewardGranted { get; init; }
    public bool RuntimeCompletionBacked { get; init; }
    public string GoalProgressStateSource { get; init; } = string.Empty;
    public string ChallengeStateSource { get; init; } = string.Empty;
    public string BaseRewardItemId { get; init; } = string.Empty;
    public ExtensionRuntimeEvidence ExtensionEvidence { get; init; } = new();
    public string SnapshotHash { get; init; } = string.Empty;
    public string RuntimeBackedStateHash { get; init; } = string.Empty;
}

public sealed record ExtensionRuntimeEvidence
{
    public static ExtensionRuntimeEvidence None { get; } = new();

    public bool Consumed { get; init; }
    public string RulePackId { get; init; } = string.Empty;
    public string StateSource { get; init; } = "none";
    public IReadOnlyList<string> AppliedActionIds { get; init; } = Array.Empty<string>();
    public string AddedRewardItemId { get; init; } = string.Empty;
    public double AddedRewardAmount { get; init; }
    public string AddedObjectiveId { get; init; } = string.Empty;
    public string AddedQuestId { get; init; } = string.Empty;
    public string AddedFlagId { get; init; } = string.Empty;
    public bool InventoryObjectiveCompleted { get; init; }
    public bool AdditionalRewardGranted { get; init; }
    public IReadOnlyDictionary<string, string> RuntimeStateMetadata { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record ExtensionSpineDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
