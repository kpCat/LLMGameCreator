using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class TinyGeneratedRuntimeLoopService
{
    public const string StateJsonFileName = "tiny-runtime-loop-state.json";
    public const string ReportJsonFileName = "tiny-runtime-loop-report.json";
    public const string ReportMarkdownFileName = "tiny-runtime-loop-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly IReadOnlySet<string> SupportedActionTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        FormulaEffectActionRulePackConstants.ActionResolveEncounter,
        "action/grant_quest_progress"
    };

    private static readonly IReadOnlySet<string> SupportedEffectTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "effect/set_flag",
        "effect/grant_item",
        "effect/adjust_reputation",
        "effect/advance_quest_event"
    };

    private readonly TinyGeneratedRuntimeLoopMarkdownRenderer _markdownRenderer;

    public TinyGeneratedRuntimeLoopService(TinyGeneratedRuntimeLoopMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new TinyGeneratedRuntimeLoopMarkdownRenderer();
    }

    public TinyGeneratedRuntimeLoopResult Run(TinyGeneratedRuntimeLoopRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<TinyGeneratedRuntimeDiagnostic>();
        var steps = new List<TinyGeneratedRuntimeStep>();
        var visitedRegions = new SortedSet<string>(StringComparer.Ordinal);
        var appliedActions = new SortedSet<string>(StringComparer.Ordinal);
        var appliedEffects = new SortedSet<string>(StringComparer.Ordinal);
        var inventory = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var flags = new SortedDictionary<string, bool>(StringComparer.Ordinal);
        var reputation = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var questStates = new SortedDictionary<string, string>(StringComparer.Ordinal);

        var plan = request.SourcePlan;
        var rulePack = request.RulePack;
        AddValidationReportDiagnostics(request.RulePackValidationReport, diagnostics);

        if (plan is null)
        {
            diagnostics.Add(Diagnostic("error", "tiny_runtime_loop.source_plan_missing", "sourcePlan", "Generated plan was not supplied."));
        }

        if (rulePack is null)
        {
            diagnostics.Add(Diagnostic("error", "tiny_runtime_loop.rule_pack_missing", "rulePack", "Formula/effect/action rule pack was not supplied."));
        }

        var source = new TinyGeneratedRuntimeSourceMetadata
        {
            PlanId = plan?.PlanId ?? string.Empty,
            PlanHash = plan?.Metadata.DeterministicHash ?? string.Empty,
            RulePackId = rulePack?.Metadata.RulePackId ?? string.Empty,
            RulePackHash = rulePack?.Metadata.DeterministicHash ?? string.Empty,
            Seed = plan?.Metadata.Seed ?? string.Empty,
            Mode = plan?.Metadata.Mode ?? string.Empty
        };

        var startingRegionId = SelectStartingRegion(plan, diagnostics);
        if (!string.IsNullOrWhiteSpace(startingRegionId))
        {
            visitedRegions.Add(startingRegionId);
            flags["flag/entered_region/" + IdSegment(startingRegionId)] = true;
            steps.Add(Step("01_enter_region", "enter_region", startingRegionId, "Entered the deterministic starting region."));
        }

        var movementRegionId = SelectMovementRegion(plan, startingRegionId, diagnostics);
        if (!string.IsNullOrWhiteSpace(movementRegionId))
        {
            visitedRegions.Add(movementRegionId);
            flags["flag/visited_region/" + IdSegment(movementRegionId)] = true;
            steps.Add(Step("02_explore_region", "explore_region", movementRegionId, "Recorded one generated movement/exploration transition."));
        }

        var resolvedEncounterId = SelectEncounter(plan, visitedRegions, diagnostics);
        if (!string.IsNullOrWhiteSpace(resolvedEncounterId))
        {
            flags["flag/encounter_selected/" + IdSegment(resolvedEncounterId)] = true;
            steps.Add(Step("03_select_encounter", "select_encounter", resolvedEncounterId, "Selected one generated encounter seed for resolution."));
        }

        var advancedQuestEventId = SelectQuestEvent(plan, resolvedEncounterId, visitedRegions, diagnostics);
        if (!string.IsNullOrWhiteSpace(advancedQuestEventId))
        {
            steps.Add(Step("04_select_quest_event", "select_quest_event", advancedQuestEventId, "Selected one generated quest/event seed for advancement."));
        }

        if (rulePack is not null)
        {
            ApplyAction(
                rulePack,
                FormulaEffectActionRulePackConstants.ActionResolveEncounter,
                "encounter",
                resolvedEncounterId,
                diagnostics,
                steps,
                appliedActions,
                appliedEffects,
                inventory,
                flags,
                reputation,
                questStates);
            ApplyAction(
                rulePack,
                FormulaEffectActionRulePackConstants.RewardQuestProgress,
                "quest_event",
                advancedQuestEventId,
                diagnostics,
                steps,
                appliedActions,
                appliedEffects,
                inventory,
                flags,
                reputation,
                questStates);
            DiagnoseUnsupportedRules(rulePack, diagnostics);
            DiagnoseMissingRulePackSourceRefs(rulePack, plan, diagnostics);
        }

        diagnostics.Add(Diagnostic("info", "tiny_runtime_loop.no_external_execution", "simulation", "No LLM, provider, Lua, Unity, media or broad runtime execution was invoked."));

        var sortedDiagnostics = SortDiagnostics(diagnostics);
        var stateWithoutHash = new TinyGeneratedRuntimeState
        {
            Source = source,
            StartingRegionId = startingRegionId,
            VisitedRegionIds = visitedRegions.ToList(),
            ResolvedEncounterId = resolvedEncounterId,
            AdvancedQuestEventId = advancedQuestEventId,
            AppliedActionIds = appliedActions.ToList(),
            AppliedEffectIds = appliedEffects.ToList(),
            InventoryItemCounts = inventory,
            Flags = flags,
            FactionReputationDeltas = reputation,
            QuestEventStates = questStates,
            Diagnostics = sortedDiagnostics
        };
        var stateHash = ComputeHash(JsonSerializer.Serialize(stateWithoutHash, JsonOptions));
        var state = stateWithoutHash with { DeterministicHash = stateHash };
        var stateJson = JsonSerializer.Serialize(state, JsonOptions);

        var report = new TinyGeneratedRuntimeLoopReport
        {
            Source = source,
            StateHash = stateHash,
            StableSummary = BuildStableSummary(state),
            HasErrors = sortedDiagnostics.Any(item => item.Severity == "error"),
            DiagnosticCount = sortedDiagnostics.Count,
            Steps = steps.OrderBy(item => item.StepId, StringComparer.Ordinal).ToList(),
            Diagnostics = sortedDiagnostics
        };
        var reportJson = JsonSerializer.Serialize(report, JsonOptions);
        var reportMarkdown = _markdownRenderer.Render(report, state);

        return new TinyGeneratedRuntimeLoopResult
        {
            State = state,
            Report = report,
            StateJson = stateJson,
            ReportJson = reportJson,
            ReportMarkdown = reportMarkdown,
            Diagnostics = sortedDiagnostics
        };
    }

    public async Task<TinyGeneratedRuntimeLoopWriteResult> WriteAsync(
        string projectRootPath,
        TinyGeneratedRuntimeLoopResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var stateJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, StateJsonFileName));
        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        EnsureContained(outputDirectory, stateJsonPath);
        EnsureContained(outputDirectory, reportJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);

        await File.WriteAllTextAsync(stateJsonPath, result.StateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new TinyGeneratedRuntimeLoopWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StateJsonPath = stateJsonPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    private static void ApplyAction(
        FormulaEffectActionRulePack rulePack,
        string actionId,
        string sourceKind,
        string sourceId,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics,
        ICollection<TinyGeneratedRuntimeStep> steps,
        ISet<string> appliedActions,
        ISet<string> appliedEffects,
        IDictionary<string, int> inventory,
        IDictionary<string, bool> flags,
        IDictionary<string, int> reputation,
        IDictionary<string, string> questStates)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.action_source_missing", actionId, $"Action '{actionId}' could not run because selected source id is missing."));
            return;
        }

        var action = rulePack.Actions.FirstOrDefault(item => item.ActionId == actionId);
        if (action is null)
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.action_missing", actionId, $"Action '{actionId}' was not found in the rule pack."));
            return;
        }

        if (!SupportedActionTypes.Contains(action.ActionType))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.unsupported_action_type", action.ActionId, $"Action type '{action.ActionType}' is not supported by the tiny loop."));
            return;
        }

        var effectsById = rulePack.Effects.ToDictionary(item => item.EffectId, StringComparer.Ordinal);
        var matchingEffectIds = action.EffectIds
            .Where(effectId => effectsById.TryGetValue(effectId, out var effect) && HasSourceRef(effect, sourceKind, sourceId))
            .OrderBy(effectId => effectId, StringComparer.Ordinal)
            .ToList();

        if (matchingEffectIds.Count == 0)
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.no_applicable_effects", action.ActionId, $"Action '{action.ActionId}' had no effects for '{sourceKind}:{sourceId}'."));
            return;
        }

        appliedActions.Add(action.ActionId);
        steps.Add(Step("05_apply_action_" + IdSegment(action.ActionId), "apply_action", action.ActionId, $"Applied generated action for '{sourceKind}:{sourceId}'."));

        foreach (var effectId in matchingEffectIds)
        {
            if (!effectsById.TryGetValue(effectId, out var effect))
            {
                diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.effect_missing", effectId, $"Effect '{effectId}' referenced by action '{action.ActionId}' was not found."));
                continue;
            }

            if (!SupportedEffectTypes.Contains(effect.EffectType))
            {
                diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.unsupported_effect_type", effect.EffectId, $"Effect type '{effect.EffectType}' is not supported by the tiny loop."));
                continue;
            }

            appliedEffects.Add(effect.EffectId);
            ApplyEffect(effect, inventory, flags, reputation, questStates);
        }
    }

    private static void ApplyEffect(
        EffectDefinition effect,
        IDictionary<string, int> inventory,
        IDictionary<string, bool> flags,
        IDictionary<string, int> reputation,
        IDictionary<string, string> questStates)
    {
        switch (effect.EffectType)
        {
            case "effect/set_flag":
                flags[effect.TargetRef] = !effect.Parameters.TryGetValue("flagValue", out var value) ||
                                          bool.TryParse(value, out var parsed) && parsed;
                break;
            case "effect/grant_item":
                inventory[effect.TargetRef] = inventory.TryGetValue(effect.TargetRef, out var existingCount) ? existingCount + 1 : 1;
                break;
            case "effect/adjust_reputation":
                reputation[effect.TargetRef] = reputation.TryGetValue(effect.TargetRef, out var existingDelta) ? existingDelta + 1 : 1;
                break;
            case "effect/advance_quest_event":
                questStates[effect.TargetRef] = effect.Parameters.TryGetValue("progressState", out var state) ? state : "advanced";
                break;
        }
    }

    private static string SelectStartingRegion(ProceduralGeneratedGamePlan? plan, ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        var regionId = plan?.World.Regions
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .Select(item => item.RegionId)
            .FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(regionId))
        {
            diagnostics.Add(Diagnostic("error", "tiny_runtime_loop.no_starting_region", "sourcePlan.world.regions", "Generated plan has no starting region."));
        }

        return regionId;
    }

    private static string SelectMovementRegion(
        ProceduralGeneratedGamePlan? plan,
        string startingRegionId,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        if (plan is null || string.IsNullOrWhiteSpace(startingRegionId))
        {
            return string.Empty;
        }

        var regionId = plan.World.Connections
            .Where(item => item.FromRegionId == startingRegionId || item.ToRegionId == startingRegionId)
            .OrderBy(item => item.ConnectionId, StringComparer.Ordinal)
            .Select(item => item.FromRegionId == startingRegionId ? item.ToRegionId : item.FromRegionId)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(regionId))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.no_movement_region", startingRegionId, "Generated plan has no traversable connection from the starting region."));
        }

        return regionId;
    }

    private static string SelectEncounter(
        ProceduralGeneratedGamePlan? plan,
        IReadOnlySet<string> visitedRegionIds,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        var encounterId = plan?.EncounterSeeds
            .Where(item => visitedRegionIds.Contains(item.RegionId))
            .OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal)
            .Select(item => item.EncounterSeedId)
            .FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(encounterId))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.no_resolvable_encounter", "sourcePlan.encounterSeeds", "No generated encounter was resolvable from the visited regions."));
        }

        return encounterId;
    }

    private static string SelectQuestEvent(
        ProceduralGeneratedGamePlan? plan,
        string resolvedEncounterId,
        IReadOnlySet<string> visitedRegionIds,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        var questEventId = plan?.QuestEventSeeds
            .Where(item => item.TargetEncounterSeedId == resolvedEncounterId || visitedRegionIds.Contains(item.RegionId))
            .OrderBy(item => item.QuestEventSeedId, StringComparer.Ordinal)
            .Select(item => item.QuestEventSeedId)
            .FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(questEventId))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.no_advanceable_quest_event", "sourcePlan.questEventSeeds", "No generated quest/event was advanceable from the selected encounter or visited regions."));
        }

        return questEventId;
    }

    private static void AddValidationReportDiagnostics(
        FormulaEffectActionValidationReport? report,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        if (report is null)
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.rule_pack_validation_report_missing", "rulePackValidationReport", "Rule pack validation report was not supplied."));
            return;
        }

        if (report.HasErrors)
        {
            diagnostics.Add(Diagnostic("error", "tiny_runtime_loop.rule_pack_validation_failed", report.RulePackId, "Rule pack validation report contains errors."));
        }

        foreach (var diagnostic in report.Diagnostics.Where(item => item.Severity == "error").OrderBy(item => item.Code, StringComparer.Ordinal).ThenBy(item => item.Target, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "tiny_runtime_loop.rule_pack_validation_error", diagnostic.Target, diagnostic.Message));
        }
    }

    private static void DiagnoseUnsupportedRules(
        FormulaEffectActionRulePack rulePack,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        foreach (var action in rulePack.Actions.Where(item => !SupportedActionTypes.Contains(item.ActionType)).OrderBy(item => item.ActionId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.unsupported_action_type", action.ActionId, $"Action type '{action.ActionType}' is not supported by the tiny loop."));
        }

        foreach (var effect in rulePack.Effects.Where(item => !SupportedEffectTypes.Contains(item.EffectType)).OrderBy(item => item.EffectId, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.unsupported_effect_type", effect.EffectId, $"Effect type '{effect.EffectType}' is not supported by the tiny loop."));
        }
    }

    private static void DiagnoseMissingRulePackSourceRefs(
        FormulaEffectActionRulePack rulePack,
        ProceduralGeneratedGamePlan? plan,
        ICollection<TinyGeneratedRuntimeDiagnostic> diagnostics)
    {
        if (plan is null)
        {
            return;
        }

        foreach (var reference in rulePack.Requirements.SelectMany(item => item.SourceRefs)
                     .Concat(rulePack.Effects.SelectMany(item => item.SourceRefs))
                     .Concat(rulePack.Actions.SelectMany(item => item.SourceRefs))
                     .Concat(rulePack.EventRules.SelectMany(item => item.SourceRefs))
                     .OrderBy(item => item.Kind, StringComparer.Ordinal)
                     .ThenBy(item => item.Id, StringComparer.Ordinal))
        {
            if (!SourcePlanContains(plan, reference.Kind, reference.Id))
            {
                diagnostics.Add(Diagnostic("warning", "tiny_runtime_loop.missing_source_plan_ref", reference.Kind + ":" + reference.Id, "Rule pack source reference was not found in the generated plan."));
            }
        }
    }

    private static bool HasSourceRef(EffectDefinition effect, string kind, string id) =>
        effect.SourceRefs.Any(item => item.Kind == kind && item.Id == id);

    private static bool SourcePlanContains(ProceduralGeneratedGamePlan sourcePlan, string kind, string id) => kind switch
    {
        "region" => sourcePlan.World.Regions.Any(item => item.RegionId == id),
        "connection" => sourcePlan.World.Connections.Any(item => item.ConnectionId == id),
        "faction" => sourcePlan.Factions.Any(item => item.FactionId == id),
        "actor" => sourcePlan.ActorSeeds.Any(item => item.ActorSeedId == id),
        "item" => sourcePlan.ItemResourceSeeds.Any(item => item.ItemSeedId == id),
        "encounter" => sourcePlan.EncounterSeeds.Any(item => item.EncounterSeedId == id),
        "quest_event" => sourcePlan.QuestEventSeeds.Any(item => item.QuestEventSeedId == id),
        _ => false
    };

    private static string BuildStableSummary(TinyGeneratedRuntimeState state) =>
        string.Join("; ", new[]
        {
            $"visitedRegions={state.VisitedRegionIds.Count}",
            $"resolvedEncounter={FormatSummaryValue(state.ResolvedEncounterId)}",
            $"advancedQuestEvent={FormatSummaryValue(state.AdvancedQuestEventId)}",
            $"appliedActions={state.AppliedActionIds.Count}",
            $"appliedEffects={state.AppliedEffectIds.Count}",
            $"inventoryItems={state.InventoryItemCounts.Count}",
            $"flags={state.Flags.Count}",
            $"factionDeltas={state.FactionReputationDeltas.Count}"
        });

    private static string FormatSummaryValue(string value) =>
        string.IsNullOrWhiteSpace(value) ? "none" : value;

    private static TinyGeneratedRuntimeStep Step(string stepId, string stepType, string targetId, string summary) => new()
    {
        StepId = stepId,
        StepType = stepType,
        TargetId = targetId,
        Summary = summary
    };

    private static TinyGeneratedRuntimeDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static IReadOnlyList<TinyGeneratedRuntimeDiagnostic> SortDiagnostics(IEnumerable<TinyGeneratedRuntimeDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static string IdSegment(string id) => id.Replace('/', '_');

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
            throw new InvalidOperationException("Tiny generated runtime loop output path must stay under the project root.");
        }
    }
}
