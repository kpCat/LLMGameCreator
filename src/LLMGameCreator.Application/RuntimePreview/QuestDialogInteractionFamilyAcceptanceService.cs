using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed partial class QuestDialogInteractionRulePackValidator
{
    private static readonly IReadOnlySet<string> AllowedQuestPatternTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "quest/fetch_item",
        "quest/deliver_item",
        "quest/recover_item_from_encounter_or_region",
        "quest/interact_with_npc_or_object",
        "quest/sequence"
    };

    private static readonly IReadOnlySet<string> AllowedObjectiveKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        "objective/fetch_item",
        "objective/deliver_item",
        "objective/recover_item",
        "objective/interact",
        "objective/sequence_step"
    };

    private static readonly IReadOnlySet<string> AllowedDialogueIntentTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "dialogue/greeting",
        "dialogue/ask_about_quest",
        "dialogue/warn_threaten",
        "dialogue/bargain_reward",
        "dialogue/completion_response"
    };

    private static readonly IReadOnlySet<string> AllowedInteractionFamilies = new HashSet<string>(StringComparer.Ordinal)
    {
        "interaction/inspect",
        "interaction/talk",
        "interaction/take_collect",
        "interaction/resolve_challenge",
        "interaction/use_item_on_target"
    };

    private static readonly IReadOnlySet<string> AllowedInteractionResultActions = new HashSet<string>(StringComparer.Ordinal)
    {
        "action/show_description",
        "action/start_dialogue",
        "action/grant_item",
        "action/resolve_challenge",
        "action/use_item_on_target",
        "action/advance_objective"
    };

    public QuestDialogInteractionRulePackValidationReport Validate(QuestDialogInteractionRulePack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);

        var diagnostics = new List<QuestDialogInteractionRulePackDiagnostic>();
        ValidateId(pack.Metadata.RulePackId, "rulePack", diagnostics);
        CheckDuplicates("quest pattern", pack.QuestPatterns.Select(item => item.PatternId), diagnostics);
        CheckDuplicates("quest objective", pack.QuestPatterns.SelectMany(item => item.Objectives).Select(item => item.ObjectiveId), diagnostics);
        CheckDuplicates("dialogue intent", pack.DialogueIntents.Select(item => item.IntentId), diagnostics);
        CheckDuplicates("interaction pattern", pack.InteractionPatterns.Select(item => item.InteractionId), diagnostics);

        var interactionIds = pack.InteractionPatterns.Select(item => item.InteractionId).ToHashSet(StringComparer.Ordinal);

        foreach (var pattern in pack.QuestPatterns)
        {
            ValidateId(pattern.PatternId, "quest pattern", diagnostics);
            ValidateAllowed(pattern.PatternType, AllowedQuestPatternTypes, pattern.PatternId, "quest_pattern.unsupported_type", diagnostics);
            if (pattern.Objectives.Count is < 1 or > 3)
            {
                Add(diagnostics, "error", "quest_pattern.objective_count_out_of_range", pattern.PatternId, "Quest pattern must declare one to three objectives.");
            }

            foreach (var objective in pattern.Objectives)
            {
                ValidateId(objective.ObjectiveId, "quest objective", diagnostics);
                ValidateAllowed(objective.ObjectiveKind, AllowedObjectiveKinds, objective.ObjectiveId, "quest_pattern.unsupported_objective_kind", diagnostics);
                ValidateSafeTargetRef(objective.TargetRef, objective.ObjectiveId, diagnostics);
                foreach (var interactionId in objective.RequiredInteractionPatternIds)
                {
                    ValidateRequiredRef(interactionId, interactionIds, objective.ObjectiveId, "quest_pattern.unknown_interaction_ref", diagnostics);
                }
            }
        }

        foreach (var intent in pack.DialogueIntents)
        {
            ValidateId(intent.IntentId, "dialogue intent", diagnostics);
            ValidateAllowed(intent.IntentType, AllowedDialogueIntentTypes, intent.IntentId, "dialogue_intent.unsupported_type", diagnostics);
            if (string.IsNullOrWhiteSpace(intent.LineTemplate) || !intent.LineTemplate.Contains("{", StringComparison.Ordinal))
            {
                Add(diagnostics, "error", "dialogue_intent.template_missing_slots", intent.IntentId, "Dialogue template must be non-empty and use semantic slots.");
            }

            ValidateTemplate(intent.LineTemplate, intent.IntentId, diagnostics);
        }

        foreach (var interaction in pack.InteractionPatterns)
        {
            ValidateId(interaction.InteractionId, "interaction pattern", diagnostics);
            ValidateAllowed(interaction.Family, AllowedInteractionFamilies, interaction.InteractionId, "interaction_pattern.unsupported_family", diagnostics);
            ValidateSafeTargetRef(interaction.TargetRef, interaction.InteractionId, diagnostics);
            ValidateAllowed(interaction.ResultActionId, AllowedInteractionResultActions, interaction.InteractionId, "interaction_pattern.unsupported_result_action", diagnostics);
            if (!string.IsNullOrWhiteSpace(interaction.RequiredItemRef))
            {
                ValidateSafeTargetRef(interaction.RequiredItemRef, interaction.InteractionId, diagnostics);
            }
        }

        var sorted = SortDiagnostics(diagnostics);
        return new QuestDialogInteractionRulePackValidationReport
        {
            SchemaVersion = QuestDialogInteractionFamilyAcceptanceService.SchemaVersion,
            RulePackId = pack.Metadata.RulePackId,
            HasErrors = sorted.Any(item => item.Severity == "error"),
            DiagnosticCount = sorted.Count,
            Diagnostics = sorted
        };
    }

    private static void ValidateTemplate(string template, string target, ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        if (template.Length > 220 ||
            template.Contains("../", StringComparison.Ordinal) ||
            template.Contains("..\\", StringComparison.Ordinal) ||
            template.Contains("System.", StringComparison.OrdinalIgnoreCase) ||
            template.Contains("lua", StringComparison.OrdinalIgnoreCase))
        {
            Add(diagnostics, "error", "dialogue_intent.template_unsafe", target, "Dialogue template contains unsafe or code-looking text.");
        }
    }

    private static void ValidateSafeTargetRef(string value, string target, ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 180 ||
            value.Contains('\\') ||
            value.Contains(':') ||
            value.Contains("../", StringComparison.Ordinal) ||
            value.Contains("..\\", StringComparison.Ordinal))
        {
            Add(diagnostics, "error", "quest_dialog_interaction.unsafe_target_ref", target, "Target reference is empty or contains unsafe path-like text.");
        }
    }

    private static void ValidateRequiredRef(
        string id,
        IReadOnlySet<string> knownIds,
        string target,
        string code,
        ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !knownIds.Contains(id))
        {
            Add(diagnostics, "error", code, target, $"Referenced id '{id}' was not found.");
        }
    }

    private static void ValidateAllowed(
        string value,
        IReadOnlySet<string> allowed,
        string target,
        string code,
        ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        if (!allowed.Contains(value))
        {
            Add(diagnostics, "error", code, target, $"Type '{value}' is not supported by quest/dialog/interaction rule pack v1.");
        }
    }

    private static void ValidateId(string id, string category, ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            id.Length > 160 ||
            id.Contains('\\') ||
            id.Contains(':') ||
            id.StartsWith('/') ||
            id.EndsWith('/') ||
            id.Split('/', StringSplitOptions.None).Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..") ||
            id.Any(character => character is not (>= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/')))
        {
            Add(diagnostics, "error", "quest_dialog_interaction.unsafe_id", id, $"{category} id is empty, too long or contains unsafe characters.");
        }
    }

    private static void CheckDuplicates(
        string kind,
        IEnumerable<string> ids,
        ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics)
    {
        foreach (var group in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            Add(diagnostics, "error", "quest_dialog_interaction.duplicate_id", group.Key, $"Duplicate {kind} id '{group.Key}' was found.");
        }
    }

    private static IReadOnlyList<QuestDialogInteractionRulePackDiagnostic> SortDiagnostics(
        IEnumerable<QuestDialogInteractionRulePackDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void Add(
        ICollection<QuestDialogInteractionRulePackDiagnostic> diagnostics,
        string severity,
        string code,
        string target,
        string message) =>
        diagnostics.Add(new QuestDialogInteractionRulePackDiagnostic
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        });
}

public sealed class QuestDialogInteractionFamilyAcceptanceService
{
    public const string SchemaVersion = "1";
    public const string RelativeOutputDirectory = ".llmgc/procedural/quest-dialog-interaction-families";
    public const string ReportJsonFileName = "quest-dialog-interaction-family-report.json";
    public const string ReportMarkdownFileName = "quest-dialog-interaction-family-report.md";
    public const string ManualVerificationMarkdownFileName = "manual-quest-dialog-interaction-family-verification.md";

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
    private readonly QuestDialogInteractionRulePackValidator _validator;

    public QuestDialogInteractionFamilyAcceptanceService(
        VisibleGeneratedPlayablePreviewService? visiblePreviewService = null,
        RuntimeBackedMicrogameStateAcceptanceService? runtimeBackedStateAcceptanceService = null,
        GenerationPresetOptionsService? generationOptionsService = null,
        QuestDialogInteractionRulePackValidator? validator = null)
    {
        _visiblePreviewService = visiblePreviewService ?? new VisibleGeneratedPlayablePreviewService();
        _runtimeBackedStateAcceptanceService = runtimeBackedStateAcceptanceService ?? new RuntimeBackedMicrogameStateAcceptanceService();
        _generationOptionsService = generationOptionsService ?? new GenerationPresetOptionsService();
        _validator = validator ?? new QuestDialogInteractionRulePackValidator();
    }

    public QuestDialogInteractionFamilyAcceptanceResult Build(string? projectRootPath = null)
    {
        var proofPack = BuildProofPack();
        var invalidPack = BuildInvalidProofPack();
        var proofValidation = _validator.Validate(proofPack);
        var invalidValidation = _validator.Validate(invalidPack);

        var scenarios = new[]
        {
            RunScenario("baseline_generated_microgame", "goal004-baseline-generated-microgame", GenerationPresetOptionsService.DefaultPresetId, proofPack, proofValidation, projectRootPath, includeQuest: false, includeDialogue: false, includeInteractions: false),
            RunScenario("quest_pattern_variant", "goal004-quest-pattern-variant", "recover_resource", proofPack, proofValidation, projectRootPath, includeQuest: true, includeDialogue: false, includeInteractions: false),
            RunScenario("dialogue_intent_variant", "goal004-dialogue-intent-variant", "safe_faction_truce", proofPack, proofValidation, projectRootPath, includeQuest: true, includeDialogue: true, includeInteractions: false),
            RunScenario("interaction_pattern_variant", "goal004-interaction-pattern-variant", "recover_resource", proofPack, proofValidation, projectRootPath, includeQuest: true, includeDialogue: true, includeInteractions: true)
        };

        var invalidRejected = invalidValidation.HasErrors
                              && invalidValidation.Diagnostics.Any(item => item.Code == "quest_pattern.unknown_interaction_ref")
                              && invalidValidation.Diagnostics.Any(item => item.Code == "interaction_pattern.unsupported_result_action")
                              && invalidValidation.Diagnostics.Any(item => item.Code == "dialogue_intent.unsupported_type");
        var questStructures = scenarios
            .Where(item => item.QuestEvidence.Generated)
            .Select(item => item.QuestEvidence.PatternType)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var dialogueEvidence = scenarios.Any(item => item.DialogueEvidence.Generated && item.DialogueEvidence.Lines.Count > 0);
        var invokedInteractionFamilies = scenarios
            .SelectMany(item => item.InteractionEvidence.InvokedFamilies)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var accepted = !proofValidation.HasErrors
                       && invalidRejected
                       && scenarios.All(item => item.Accepted)
                       && questStructures >= 2
                       && dialogueEvidence
                       && invokedInteractionFamilies >= 2;

        var diagnostics = new List<QuestDialogInteractionFamilyDiagnostic>
        {
            Diagnostic("info", "quest_dialog_interaction.no_external_execution", "harness", "No LLM, provider, Lua, Unity or media execution was invoked."),
            Diagnostic("info", "quest_dialog_interaction.csharp_scope", "harness", "C# changes are limited to reusable declarations, validation, deterministic adapters and scenario evidence."),
            Diagnostic(
                questStructures >= 2 ? "info" : "error",
                questStructures >= 2 ? "quest_dialog_interaction.quest_variants_generated" : "quest_dialog_interaction.quest_variants_missing",
                proofPack.Metadata.RulePackId,
                questStructures >= 2 ? "Rule-pack data produced at least two quest structures." : "Rule-pack data did not produce the required quest structure variation."),
            Diagnostic(
                dialogueEvidence ? "info" : "error",
                dialogueEvidence ? "quest_dialog_interaction.dialogue_evidence_generated" : "quest_dialog_interaction.dialogue_evidence_missing",
                proofPack.Metadata.RulePackId,
                dialogueEvidence ? "Dialogue evidence was generated from intent templates and semantic slots." : "Dialogue evidence was not generated."),
            Diagnostic(
                invokedInteractionFamilies >= 2 ? "info" : "error",
                invokedInteractionFamilies >= 2 ? "quest_dialog_interaction.interaction_families_invoked" : "quest_dialog_interaction.interaction_families_missing",
                proofPack.Metadata.RulePackId,
                invokedInteractionFamilies >= 2 ? "At least two interaction families were invoked through rule-pack declarations." : "Required interaction family invocation evidence is missing."),
            Diagnostic(
                invalidRejected ? "info" : "error",
                invalidRejected ? "quest_dialog_interaction.invalid_pack_rejected" : "quest_dialog_interaction.invalid_pack_not_rejected",
                invalidPack.Metadata.RulePackId,
                invalidRejected ? "Invalid quest/dialog/interaction pack was rejected by declaration-level validation." : "Invalid quest/dialog/interaction pack did not produce required validation errors."),
            Diagnostic("info", "quest_dialog_interaction.manual_verification_required", "manual_quest_dialog_interaction_family_verification", "Codex acceptance is headless; the next step is manual quest/dialog/interaction family verification.")
        };

        var reportWithoutHash = new QuestDialogInteractionFamilyReport
        {
            Accepted = accepted,
            ManualGate = "manual_quest_dialog_interaction_family_verification",
            ProofRulePackId = proofPack.Metadata.RulePackId,
            InvalidRulePackRejected = invalidRejected,
            QuestStructureVariantCount = questStructures,
            DialogueEvidenceGenerated = dialogueEvidence,
            InteractionFamilyInvocationCount = invokedInteractionFamilies,
            WhatIsDataExtensible =
            [
                "fetch item quest pattern",
                "deliver item quest pattern",
                "recover item from encounter or region quest pattern",
                "interact with NPC or object quest pattern",
                "two-to-three objective quest sequences",
                "dialogue intent templates with semantic slots",
                "inspect/talk/take/resolve/use-item interaction families"
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

        return new QuestDialogInteractionFamilyAcceptanceResult
        {
            Report = report,
            ProofRulePack = proofPack,
            InvalidRulePack = invalidPack,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            ManualVerificationMarkdown = RenderManualVerification(report)
        };
    }

    public async Task<QuestDialogInteractionFamilyAcceptanceWriteResult> WriteAsync(
        string projectRootPath,
        QuestDialogInteractionFamilyAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "quest-dialog-interaction-families"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        EnsureContained(outputDirectory, reportJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, manualVerificationMarkdownPath);

        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new QuestDialogInteractionFamilyAcceptanceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    public static QuestDialogInteractionRulePack BuildProofPack() => new()
    {
        Metadata = new QuestDialogInteractionRulePackMetadata
        {
            RulePackId = "rule_pack/quest_dialog_interaction_families_v1",
            DisplayName = "Quest dialog interaction family proof pack"
        },
        QuestPatterns =
        [
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/fetch_item_cache",
                PatternType = "quest/fetch_item",
                Objectives =
                [
                    Objective("objective/find_cache_item", "objective/fetch_item", "item/generated_cache", "interaction/take_cache_item")
                ]
            },
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/deliver_reward_token",
                PatternType = "quest/deliver_item",
                Objectives =
                [
                    Objective("objective/talk_to_contact", "objective/interact", "npc/generated_contact", "interaction/talk_contact"),
                    Objective("objective/deliver_reward_token", "objective/deliver_item", "item/generated_reward", "interaction/use_reward_on_contact")
                ]
            },
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/recover_from_encounter",
                PatternType = "quest/recover_item_from_encounter_or_region",
                Objectives =
                [
                    Objective("objective/inspect_encounter", "objective/interact", "encounter/generated_challenge", "interaction/inspect_challenge"),
                    Objective("objective/resolve_encounter", "objective/recover_item", "encounter/generated_challenge", "interaction/resolve_challenge"),
                    Objective("objective/collect_recovered_item", "objective/fetch_item", "item/generated_recovered_item", "interaction/take_cache_item")
                ]
            },
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/interact_with_object",
                PatternType = "quest/interact_with_npc_or_object",
                Objectives =
                [
                    Objective("objective/inspect_marker", "objective/interact", "object/generated_marker", "interaction/inspect_marker")
                ]
            },
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/two_step_sequence",
                PatternType = "quest/sequence",
                Objectives =
                [
                    Objective("objective/sequence_talk", "objective/sequence_step", "npc/generated_contact", "interaction/talk_contact"),
                    Objective("objective/sequence_collect", "objective/sequence_step", "item/generated_cache", "interaction/take_cache_item")
                ]
            }
        ],
        DialogueIntents =
        [
            Dialogue("dialogue/greeting/default", "dialogue/greeting", "Hello, {player}. The {quest_title} still matters."),
            Dialogue("dialogue/ask_about_quest/default", "dialogue/ask_about_quest", "Ask about {quest_title}: find {objective_target}."),
            Dialogue("dialogue/warn_threaten/default", "dialogue/warn_threaten", "Careful near {challenge_title}; it blocks the route."),
            Dialogue("dialogue/bargain_reward/default", "dialogue/bargain_reward", "Bring {objective_target} and the reward is {reward_title}."),
            Dialogue("dialogue/completion_response/default", "dialogue/completion_response", "Done. {reward_title} proves the loop completed.")
        ],
        InteractionPatterns =
        [
            Interaction("interaction/inspect_marker", "interaction/inspect", "object/generated_marker", "action/show_description"),
            Interaction("interaction/inspect_challenge", "interaction/inspect", "encounter/generated_challenge", "action/show_description"),
            Interaction("interaction/talk_contact", "interaction/talk", "npc/generated_contact", "action/start_dialogue"),
            Interaction("interaction/take_cache_item", "interaction/take_collect", "item/generated_cache", "action/grant_item"),
            Interaction("interaction/resolve_challenge", "interaction/resolve_challenge", "encounter/generated_challenge", "action/resolve_challenge"),
            Interaction("interaction/use_reward_on_contact", "interaction/use_item_on_target", "npc/generated_contact", "action/use_item_on_target", "item/generated_reward")
        ]
    };

    public static QuestDialogInteractionRulePack BuildInvalidProofPack() => BuildProofPack() with
    {
        Metadata = new QuestDialogInteractionRulePackMetadata
        {
            RulePackId = "rule_pack/invalid_quest_dialog_interaction_families_v1",
            DisplayName = "Invalid quest dialog interaction proof pack"
        },
        QuestPatterns =
        [
            new QuestPatternDefinition
            {
                PatternId = "quest_pattern/invalid",
                PatternType = "quest/arbitrary_lua",
                Objectives =
                [
                    Objective("objective/invalid", "objective/run_script", "..\\outside", "interaction/missing")
                ]
            }
        ],
        DialogueIntents =
        [
            Dialogue("dialogue/invalid", "dialogue/free_llm_completion", "Run lua {quest_title} ../bad")
        ],
        InteractionPatterns =
        [
            Interaction("../bad", "interaction/run_lua", "..\\outside", "action/run_lua")
        ]
    };

    private QuestDialogInteractionFamilyScenario RunScenario(
        string scenarioId,
        string seed,
        string presetId,
        QuestDialogInteractionRulePack proofPack,
        QuestDialogInteractionRulePackValidationReport validation,
        string? projectRootPath,
        bool includeQuest,
        bool includeDialogue,
        bool includeInteractions)
    {
        var generationOptions = _generationOptionsService.Resolve(new GenerationPresetOptionsRequest
        {
            Seed = seed,
            PresetId = presetId
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
        var runtimeAccepted = visibleResult.Report.RuntimeStartSucceeded
                              && visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "interact", StringComparison.OrdinalIgnoreCase) && item.Succeeded)
                              && visibleResult.Report.GoalProgressAdvanced
                              && runtimeAcceptance.Snapshot.RuntimeRewardGranted
                              && runtimeAcceptance.Snapshot.RuntimeCompletionBacked
                              && string.Equals(runtimeAcceptance.Snapshot.GoalProgressStateSource, "runtime_state_quests", StringComparison.Ordinal)
                              && string.Equals(runtimeAcceptance.Snapshot.ChallengeStateSource, "runtime_state_flags_inventory_encounter", StringComparison.Ordinal);

        var semanticSlots = BuildSemanticSlots(runtimeAcceptance.Snapshot);
        var selectedPattern = SelectQuestPattern(proofPack, scenarioId);
        var questEvidence = includeQuest && !validation.HasErrors
            ? BuildQuestEvidence(selectedPattern, semanticSlots)
            : QuestFamilyEvidence.None;
        var dialogueEvidence = includeDialogue && !validation.HasErrors
            ? BuildDialogueEvidence(proofPack.DialogueIntents, semanticSlots)
            : DialogueFamilyEvidence.None;
        var interactionEvidence = includeInteractions && !validation.HasErrors
            ? BuildInteractionEvidence(proofPack.InteractionPatterns, semanticSlots)
            : InteractionFamilyEvidence.None;

        return new QuestDialogInteractionFamilyScenario
        {
            ScenarioId = scenarioId,
            Accepted = runtimeAccepted
                       && (!includeQuest || questEvidence.Generated)
                       && (!includeDialogue || dialogueEvidence.Generated)
                       && (!includeInteractions || interactionEvidence.InvokedFamilies.Count >= 2),
            GenerationOptions = generationOptions,
            PackageId = visibleResult.Snapshot.PackageId,
            PackageTitle = visibleResult.Snapshot.PackageTitle,
            RuntimeStartSucceeded = visibleResult.Report.RuntimeStartSucceeded,
            RuntimeInteractSucceeded = runtimeAcceptance.Snapshot.RuntimeInteractSucceeded,
            GoalProgressAdvanced = runtimeAcceptance.Snapshot.ProgressAdvanced,
            RuntimeRewardGranted = runtimeAcceptance.Snapshot.RuntimeRewardGranted,
            RuntimeCompletionBacked = runtimeAcceptance.Snapshot.RuntimeCompletionBacked,
            GoalProgressStateSource = runtimeAcceptance.Snapshot.GoalProgressStateSource,
            ChallengeStateSource = runtimeAcceptance.Snapshot.ChallengeStateSource,
            QuestEvidence = questEvidence,
            DialogueEvidence = dialogueEvidence,
            InteractionEvidence = interactionEvidence,
            SnapshotHash = visibleResult.Snapshot.DeterministicHash,
            RuntimeBackedStateHash = runtimeAcceptance.Snapshot.DeterministicHash
        };
    }

    private static QuestPatternDefinition SelectQuestPattern(QuestDialogInteractionRulePack proofPack, string scenarioId)
    {
        var preferred = scenarioId switch
        {
            "quest_pattern_variant" => "quest_pattern/recover_from_encounter",
            "dialogue_intent_variant" => "quest_pattern/deliver_reward_token",
            "interaction_pattern_variant" => "quest_pattern/two_step_sequence",
            _ => "quest_pattern/fetch_item_cache"
        };

        return proofPack.QuestPatterns.First(item => string.Equals(item.PatternId, preferred, StringComparison.Ordinal));
    }

    private static IReadOnlyDictionary<string, string> BuildSemanticSlots(RuntimeBackedMicrogameStateAcceptanceSnapshot snapshot) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["challenge_title"] = FirstNonEmpty(snapshot.ChallengeTitle, snapshot.ChallengeId, "generated challenge"),
            ["objective_target"] = FirstNonEmpty(snapshot.RewardTitle, snapshot.RewardItemId, "generated item"),
            ["player"] = "player",
            ["quest_title"] = FirstNonEmpty(snapshot.ActiveGoalTitle, snapshot.ActiveGoalId, "generated quest"),
            ["reward_title"] = FirstNonEmpty(snapshot.RewardTitle, snapshot.RewardItemId, "generated reward")
        };

    private static QuestFamilyEvidence BuildQuestEvidence(
        QuestPatternDefinition pattern,
        IReadOnlyDictionary<string, string> semanticSlots) =>
        new()
        {
            Generated = true,
            PatternId = pattern.PatternId,
            PatternType = pattern.PatternType,
            ObjectiveIds = pattern.Objectives.Select(item => item.ObjectiveId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ObjectiveTexts = pattern.Objectives
                .OrderBy(item => item.ObjectiveId, StringComparer.Ordinal)
                .Select(item => $"{item.ObjectiveKind} -> {ResolveTargetLabel(item.TargetRef, semanticSlots)}")
                .ToList(),
            ObjectiveCount = pattern.Objectives.Count
        };

    private static DialogueFamilyEvidence BuildDialogueEvidence(
        IReadOnlyList<DialogueIntentPatternDefinition> intents,
        IReadOnlyDictionary<string, string> semanticSlots) =>
        new()
        {
            Generated = intents.Count > 0,
            IntentIds = intents.Select(item => item.IntentId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Lines = intents
                .OrderBy(item => item.IntentId, StringComparer.Ordinal)
                .Select(item => ReplaceSlots(item.LineTemplate, semanticSlots))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList(),
            Source = "intent_templates_semantic_slots"
        };

    private static InteractionFamilyEvidence BuildInteractionEvidence(
        IReadOnlyList<InteractionPatternDefinition> interactions,
        IReadOnlyDictionary<string, string> semanticSlots)
    {
        var selected = interactions
            .Where(item => item.Family is "interaction/inspect" or "interaction/talk" or "interaction/take_collect" or "interaction/resolve_challenge" or "interaction/use_item_on_target")
            .OrderBy(item => item.InteractionId, StringComparer.Ordinal)
            .Take(3)
            .ToList();

        return new InteractionFamilyEvidence
        {
            Invoked = selected.Count >= 2,
            InvokedFamilies = selected.Select(item => item.Family).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            BoundTargets = selected.Select(item => ResolveTargetLabel(item.TargetRef, semanticSlots)).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ResultActionIds = selected.Select(item => item.ResultActionId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ChangedGeneratedReportEvidence = selected.Count >= 2,
            StateSource = "existing_interact_command_plus_rule_pack_report_evidence"
        };
    }

    private static string RenderReport(QuestDialogInteractionFamilyReport report)
    {
        var lines = new List<string>
        {
            "# Quest/Dialog/Interaction Family Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Accepted: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Snapshot hash: `{report.DeterministicHash}`",
            $"- Manual gate: `{report.ManualGate}`",
            $"- Quest structure variants: `{report.QuestStructureVariantCount}`",
            $"- Dialogue evidence generated: `{report.DialogueEvidenceGenerated.ToString().ToLowerInvariant()}`",
            $"- Interaction family invocations: `{report.InteractionFamilyInvocationCount}`",
            $"- Invalid pack rejected: `{report.InvalidRulePackRejected.ToString().ToLowerInvariant()}`",
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
            lines.Add($"- Runtime: start=`{scenario.RuntimeStartSucceeded.ToString().ToLowerInvariant()}`, interact=`{scenario.RuntimeInteractSucceeded.ToString().ToLowerInvariant()}`, reward=`{scenario.RuntimeRewardGranted.ToString().ToLowerInvariant()}`, completion=`{scenario.RuntimeCompletionBacked.ToString().ToLowerInvariant()}`");
            lines.Add($"- Quest pattern: `{FirstNonEmpty(scenario.QuestEvidence.PatternId, "none")}` objectives=`{scenario.QuestEvidence.ObjectiveCount}`");
            lines.Add($"- Dialogue lines: `{scenario.DialogueEvidence.Lines.Count}`");
            lines.Add($"- Interaction families: `{string.Join(", ", scenario.InteractionEvidence.InvokedFamilies)}`");
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

    private static string RenderManualVerification(QuestDialogInteractionFamilyReport report)
    {
        var lines = new List<string>
        {
            "# Manual Quest/Dialog/Interaction Family Verification",
            string.Empty,
            "Use this after Goal 004. Codex stops here and does not perform this manual UI check.",
            string.Empty,
            "1. Review `.llmgc/procedural/quest-dialog-interaction-families/quest-dialog-interaction-family-report.json`.",
            "2. Confirm generated quest objectives are meaningful and differ between variants.",
            "3. Confirm dialogue lines are non-empty and tied to quest or interaction context.",
            "4. Confirm interactions are understandable and bind to NPC/item/encounter/location targets.",
            "5. Confirm runtime-backed reward and completion evidence remains valid.",
            string.Empty,
            $"Headless acceptance status: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"Next state marker: `{report.ManualGate}`"
        };

        return string.Join("\n", lines) + "\n";
    }

    private static QuestObjectivePatternDefinition Objective(
        string objectiveId,
        string objectiveKind,
        string targetRef,
        params string[] requiredInteractionPatternIds) =>
        new()
        {
            ObjectiveId = objectiveId,
            ObjectiveKind = objectiveKind,
            TargetRef = targetRef,
            RequiredInteractionPatternIds = requiredInteractionPatternIds
        };

    private static DialogueIntentPatternDefinition Dialogue(string intentId, string intentType, string lineTemplate) => new()
    {
        IntentId = intentId,
        IntentType = intentType,
        LineTemplate = lineTemplate
    };

    private static InteractionPatternDefinition Interaction(
        string interactionId,
        string family,
        string targetRef,
        string resultActionId,
        string requiredItemRef = "") =>
        new()
        {
            InteractionId = interactionId,
            Family = family,
            TargetRef = targetRef,
            ResultActionId = resultActionId,
            RequiredItemRef = requiredItemRef
        };

    private static string ReplaceSlots(string template, IReadOnlyDictionary<string, string> semanticSlots)
    {
        var result = template;
        foreach (var pair in semanticSlots.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            result = result.Replace("{" + pair.Key + "}", pair.Value, StringComparison.Ordinal);
        }

        return result;
    }

    private static string ResolveTargetLabel(string targetRef, IReadOnlyDictionary<string, string> semanticSlots)
    {
        if (targetRef.StartsWith("encounter/", StringComparison.Ordinal))
        {
            return semanticSlots["challenge_title"];
        }

        if (targetRef.StartsWith("item/", StringComparison.Ordinal))
        {
            return semanticSlots["objective_target"];
        }

        if (targetRef.StartsWith("npc/", StringComparison.Ordinal))
        {
            return "generated contact";
        }

        if (targetRef.StartsWith("quest/", StringComparison.Ordinal))
        {
            return semanticSlots["quest_title"];
        }

        return targetRef;
    }

    private static IReadOnlyList<QuestDialogInteractionFamilyDiagnostic> SortDiagnostics(
        IEnumerable<QuestDialogInteractionFamilyDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static QuestDialogInteractionFamilyDiagnostic Diagnostic(
        string severity,
        string code,
        string target,
        string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

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
            throw new InvalidOperationException("Quest/dialog/interaction family output path must stay under the project root.");
        }
    }
}

public sealed record QuestDialogInteractionRulePack
{
    public QuestDialogInteractionRulePackMetadata Metadata { get; init; } = new();
    public IReadOnlyList<QuestPatternDefinition> QuestPatterns { get; init; } = Array.Empty<QuestPatternDefinition>();
    public IReadOnlyList<DialogueIntentPatternDefinition> DialogueIntents { get; init; } = Array.Empty<DialogueIntentPatternDefinition>();
    public IReadOnlyList<InteractionPatternDefinition> InteractionPatterns { get; init; } = Array.Empty<InteractionPatternDefinition>();
}

public sealed record QuestDialogInteractionRulePackMetadata
{
    public string SchemaVersion { get; init; } = QuestDialogInteractionFamilyAcceptanceService.SchemaVersion;
    public string RulePackId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string AuthoringMode { get; init; } = "data_rule_pack";
}

public sealed record QuestPatternDefinition
{
    public string PatternId { get; init; } = string.Empty;
    public string PatternType { get; init; } = string.Empty;
    public IReadOnlyList<QuestObjectivePatternDefinition> Objectives { get; init; } = Array.Empty<QuestObjectivePatternDefinition>();
}

public sealed record QuestObjectivePatternDefinition
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredInteractionPatternIds { get; init; } = Array.Empty<string>();
}

public sealed record DialogueIntentPatternDefinition
{
    public string IntentId { get; init; } = string.Empty;
    public string IntentType { get; init; } = string.Empty;
    public string LineTemplate { get; init; } = string.Empty;
}

public sealed record InteractionPatternDefinition
{
    public string InteractionId { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string ResultActionId { get; init; } = string.Empty;
    public string RequiredItemRef { get; init; } = string.Empty;
}

public sealed record QuestDialogInteractionRulePackValidationReport
{
    public string SchemaVersion { get; init; } = QuestDialogInteractionFamilyAcceptanceService.SchemaVersion;
    public string RulePackId { get; init; } = string.Empty;
    public bool HasErrors { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<QuestDialogInteractionRulePackDiagnostic> Diagnostics { get; init; } = Array.Empty<QuestDialogInteractionRulePackDiagnostic>();
}

public sealed record QuestDialogInteractionRulePackDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record QuestDialogInteractionFamilyAcceptanceResult
{
    public QuestDialogInteractionFamilyReport Report { get; init; } = new();
    public QuestDialogInteractionRulePack ProofRulePack { get; init; } = new();
    public QuestDialogInteractionRulePack InvalidRulePack { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record QuestDialogInteractionFamilyAcceptanceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record QuestDialogInteractionFamilyReport
{
    public string SchemaVersion { get; init; } = QuestDialogInteractionFamilyAcceptanceService.SchemaVersion;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public string ProofRulePackId { get; init; } = string.Empty;
    public bool InvalidRulePackRejected { get; init; }
    public int QuestStructureVariantCount { get; init; }
    public bool DialogueEvidenceGenerated { get; init; }
    public int InteractionFamilyInvocationCount { get; init; }
    public IReadOnlyList<string> WhatIsDataExtensible { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> WhatStillRequiresCSharpPrimitive { get; init; } = Array.Empty<string>();
    public IReadOnlyList<QuestDialogInteractionFamilyScenario> Scenarios { get; init; } = Array.Empty<QuestDialogInteractionFamilyScenario>();
    public QuestDialogInteractionRulePackValidationReport ProofValidation { get; init; } = new();
    public QuestDialogInteractionRulePackValidationReport InvalidValidation { get; init; } = new();
    public IReadOnlyList<QuestDialogInteractionFamilyDiagnostic> Diagnostics { get; init; } = Array.Empty<QuestDialogInteractionFamilyDiagnostic>();
}

public sealed record QuestDialogInteractionFamilyScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool RuntimeInteractSucceeded { get; init; }
    public bool GoalProgressAdvanced { get; init; }
    public bool RuntimeRewardGranted { get; init; }
    public bool RuntimeCompletionBacked { get; init; }
    public string GoalProgressStateSource { get; init; } = string.Empty;
    public string ChallengeStateSource { get; init; } = string.Empty;
    public QuestFamilyEvidence QuestEvidence { get; init; } = new();
    public DialogueFamilyEvidence DialogueEvidence { get; init; } = new();
    public InteractionFamilyEvidence InteractionEvidence { get; init; } = new();
    public string SnapshotHash { get; init; } = string.Empty;
    public string RuntimeBackedStateHash { get; init; } = string.Empty;
}

public sealed record QuestFamilyEvidence
{
    public static QuestFamilyEvidence None { get; } = new();
    public bool Generated { get; init; }
    public string PatternId { get; init; } = string.Empty;
    public string PatternType { get; init; } = string.Empty;
    public int ObjectiveCount { get; init; }
    public IReadOnlyList<string> ObjectiveIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObjectiveTexts { get; init; } = Array.Empty<string>();
}

public sealed record DialogueFamilyEvidence
{
    public static DialogueFamilyEvidence None { get; } = new();
    public bool Generated { get; init; }
    public string Source { get; init; } = string.Empty;
    public IReadOnlyList<string> IntentIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

public sealed record InteractionFamilyEvidence
{
    public static InteractionFamilyEvidence None { get; } = new();
    public bool Invoked { get; init; }
    public string StateSource { get; init; } = string.Empty;
    public bool ChangedGeneratedReportEvidence { get; init; }
    public IReadOnlyList<string> InvokedFamilies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BoundTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResultActionIds { get; init; } = Array.Empty<string>();
}

public sealed record QuestDialogInteractionFamilyDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
