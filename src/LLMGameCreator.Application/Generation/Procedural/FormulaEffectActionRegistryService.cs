using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class FormulaEffectActionRegistryService
{
    public const string RulePackJsonFileName = "formula-effect-action-rule-pack.json";
    public const string RulePackMarkdownFileName = "formula-effect-action-rule-pack.md";
    public const string ValidationReportJsonFileName = "formula-effect-action-validation-report.json";
    public const string ValidationReportMarkdownFileName = "formula-effect-action-validation-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly FormulaEffectActionRulePackValidator _validator;
    private readonly FormulaEffectActionRulePackMarkdownRenderer _markdownRenderer;

    public FormulaEffectActionRegistryService(
        FormulaEffectActionRulePackValidator? validator = null,
        FormulaEffectActionRulePackMarkdownRenderer? markdownRenderer = null)
    {
        _validator = validator ?? new FormulaEffectActionRulePackValidator();
        _markdownRenderer = markdownRenderer ?? new FormulaEffectActionRulePackMarkdownRenderer();
    }

    public FormulaEffectActionRegistryResult Generate(FormulaEffectActionRegistryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<FormulaEffectActionDiagnostic>();
        var sourcePlan = request.SourcePlan;
        if (sourcePlan is null)
        {
            diagnostics.Add(Diagnostic("warning", "rule_pack.source_plan_missing", "sourcePlan", "Source generated plan was not supplied; source refs will be empty."));
        }

        var selectedPlaceholderIds = SelectPlaceholderIds(request, diagnostics);
        var formulas = BuildFormulas(selectedPlaceholderIds);
        var requirements = BuildRequirements(sourcePlan, selectedPlaceholderIds);
        var effects = BuildEffects(sourcePlan, selectedPlaceholderIds);
        var actions = BuildActions(sourcePlan, selectedPlaceholderIds, effects);
        var eventRules = BuildEventRules(sourcePlan, selectedPlaceholderIds);

        diagnostics.Add(Diagnostic("info", "rule_pack.no_external_execution", "generation", "No LLM, provider, Lua, Unity, media or runtime execution was invoked."));

        var metadataWithoutHash = new FormulaEffectActionRulePackMetadata
        {
            SourcePlanId = sourcePlan?.PlanId ?? string.Empty,
            SourcePlanHash = sourcePlan?.Metadata.DeterministicHash ?? string.Empty,
            StableSummary = BuildStableSummary(formulas, requirements, effects, actions, eventRules)
        };

        var packWithoutHash = new FormulaEffectActionRulePack
        {
            Metadata = metadataWithoutHash,
            Formulas = formulas,
            Requirements = requirements,
            Effects = effects,
            Actions = actions,
            EventRules = eventRules
        };

        var deterministicHash = ComputeHash(JsonSerializer.Serialize(packWithoutHash, JsonOptions));
        var packForValidation = packWithoutHash with
        {
            Metadata = metadataWithoutHash with { DeterministicHash = deterministicHash }
        };

        diagnostics.AddRange(_validator.Validate(packForValidation, sourcePlan));
        var sortedDiagnostics = SortDiagnostics(diagnostics);
        var packWithDiagnostics = packForValidation with { Diagnostics = sortedDiagnostics };
        var markdown = _markdownRenderer.RenderRulePack(packWithDiagnostics);
        var rulePack = packWithDiagnostics with { MarkdownSummary = markdown };
        var json = JsonSerializer.Serialize(rulePack, JsonOptions);

        var validationReport = new FormulaEffectActionValidationReport
        {
            RulePackId = rulePack.Metadata.RulePackId,
            RulePackHash = rulePack.Metadata.DeterministicHash,
            DiagnosticCount = sortedDiagnostics.Count,
            HasErrors = sortedDiagnostics.Any(item => item.Severity == "error"),
            Diagnostics = sortedDiagnostics
        };
        var validationReportJson = JsonSerializer.Serialize(validationReport, JsonOptions);
        var validationReportMarkdown = _markdownRenderer.RenderValidationReport(validationReport);

        if (request.StrictMode && validationReport.HasErrors)
        {
            throw new InvalidOperationException("Formula/effect/action rule pack validation failed in strict mode.");
        }

        return new FormulaEffectActionRegistryResult
        {
            RulePack = rulePack,
            ValidationReport = validationReport,
            Json = json,
            Markdown = markdown,
            ValidationReportJson = validationReportJson,
            ValidationReportMarkdown = validationReportMarkdown,
            Diagnostics = sortedDiagnostics
        };
    }

    public async Task<FormulaEffectActionRulePackWriteResult> WriteAsync(
        string projectRootPath,
        FormulaEffectActionRegistryResult result,
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

        var rulePackJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, RulePackJsonFileName));
        var rulePackMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, RulePackMarkdownFileName));
        var validationReportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ValidationReportJsonFileName));
        var validationReportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ValidationReportMarkdownFileName));
        EnsureContained(outputDirectory, rulePackJsonPath);
        EnsureContained(outputDirectory, rulePackMarkdownPath);
        EnsureContained(outputDirectory, validationReportJsonPath);
        EnsureContained(outputDirectory, validationReportMarkdownPath);

        await File.WriteAllTextAsync(rulePackJsonPath, result.Json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(rulePackMarkdownPath, result.Markdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(validationReportJsonPath, result.ValidationReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(validationReportMarkdownPath, result.ValidationReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new FormulaEffectActionRulePackWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            RulePackJsonPath = rulePackJsonPath,
            RulePackMarkdownPath = rulePackMarkdownPath,
            ValidationReportJsonPath = validationReportJsonPath,
            ValidationReportMarkdownPath = validationReportMarkdownPath
        };
    }

    private static IReadOnlyList<string> SelectPlaceholderIds(
        FormulaEffectActionRegistryRequest request,
        ICollection<FormulaEffectActionDiagnostic> diagnostics)
    {
        var selected = request.SelectedPlaceholderIds.Count > 0
            ? request.SelectedPlaceholderIds
            : request.SourcePlan?.FormulaEffectActionPlaceholders.Select(item => item.PlaceholderId).ToList()
              ?? BuiltInPlaceholderIds;

        var ids = selected
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        foreach (var id in ids.Where(id => !BuiltInPlaceholderIds.Contains(id)))
        {
            diagnostics.Add(Diagnostic("warning", "rule_pack.unmapped_placeholder", id, $"Placeholder '{id}' has no built-in mapping in this foundation slice."));
        }

        return ids;
    }

    private static IReadOnlyList<FormulaDefinition> BuildFormulas(IReadOnlyList<string> selectedPlaceholderIds)
    {
        var formulas = new List<FormulaDefinition>();
        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementOpenRoute))
        {
            formulas.Add(new FormulaDefinition
            {
                FormulaId = "formula/route_access_score",
                Expression = "connection_open + route_safety",
                DeclaredVariables = ["connection_open", "route_safety"],
                MinimumValue = 0,
                MaximumValue = 2
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess))
        {
            formulas.Add(new FormulaDefinition
            {
                FormulaId = "formula/faction_access_score",
                Expression = "reputation_score + story_access",
                DeclaredVariables = ["reputation_score", "story_access"],
                MinimumValue = -100,
                MaximumValue = 200
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter))
        {
            formulas.Add(new FormulaDefinition
            {
                FormulaId = "formula/encounter_reward_count",
                Expression = "base_reward_count + encounter_bonus",
                DeclaredVariables = ["base_reward_count", "encounter_bonus"],
                MinimumValue = 0,
                MaximumValue = 12
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress))
        {
            formulas.Add(new FormulaDefinition
            {
                FormulaId = "formula/quest_reward_count",
                Expression = "base_reward_count + quest_stage_bonus",
                DeclaredVariables = ["base_reward_count", "quest_stage_bonus"],
                MinimumValue = 0,
                MaximumValue = 12
            });
        }

        return formulas.OrderBy(item => item.FormulaId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<RequirementDefinition> BuildRequirements(
        ProceduralGeneratedGamePlan? sourcePlan,
        IReadOnlyList<string> selectedPlaceholderIds)
    {
        var requirements = new List<RequirementDefinition>();
        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementOpenRoute))
        {
            requirements.Add(new RequirementDefinition
            {
                RequirementId = FormulaEffectActionRulePackConstants.RequirementOpenRoute,
                RequirementType = FormulaEffectActionRulePackConstants.RequirementOpenRoute,
                Purpose = "Check whether a region connection can be traversed.",
                FormulaId = "formula/route_access_score",
                PredicateSlot = "route_access_predicate",
                SourceRefs = sourcePlan?.World.Connections
                    .Select(item => Ref("connection", item.ConnectionId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess))
        {
            requirements.Add(new RequirementDefinition
            {
                RequirementId = FormulaEffectActionRulePackConstants.RequirementFactionAccess,
                RequirementType = FormulaEffectActionRulePackConstants.RequirementFactionAccess,
                Purpose = "Check whether a faction relationship, reputation or story access gate allows an action.",
                FormulaId = "formula/faction_access_score",
                PredicateSlot = "faction_access_predicate",
                SourceRefs = sourcePlan?.Factions
                    .Select(item => Ref("faction", item.FactionId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        return requirements.OrderBy(item => item.RequirementId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<EffectDefinition> BuildEffects(
        ProceduralGeneratedGamePlan? sourcePlan,
        IReadOnlyList<string> selectedPlaceholderIds)
    {
        var effects = new List<EffectDefinition>();
        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter))
        {
            foreach (var encounter in sourcePlan?.EncounterSeeds ?? [])
            {
                effects.Add(new EffectDefinition
                {
                    EffectId = "effect/set_flag/" + IdSegment(encounter.EncounterSeedId) + "_resolved",
                    EffectType = "effect/set_flag",
                    TargetRef = "flag/encounter_resolved/" + IdSegment(encounter.EncounterSeedId),
                    SourceRefs = [Ref("encounter", encounter.EncounterSeedId)],
                    Parameters = SortedParameters(("flagValue", "true"))
                });

                foreach (var itemSeedId in encounter.RewardItemSeedIds.OrderBy(id => id, StringComparer.Ordinal))
                {
                    effects.Add(new EffectDefinition
                    {
                        EffectId = "effect/grant_item/" + IdSegment(encounter.EncounterSeedId) + "/" + IdSegment(itemSeedId),
                        EffectType = "effect/grant_item",
                        TargetRef = itemSeedId,
                        FormulaId = "formula/encounter_reward_count",
                        SourceRefs = [Ref("encounter", encounter.EncounterSeedId), Ref("item", itemSeedId)],
                        Parameters = SortedParameters(("grantReason", "encounter_reward"))
                    });
                }
            }
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress))
        {
            foreach (var questEvent in sourcePlan?.QuestEventSeeds ?? [])
            {
                effects.Add(new EffectDefinition
                {
                    EffectId = "effect/advance_quest_event/" + IdSegment(questEvent.QuestEventSeedId),
                    EffectType = "effect/advance_quest_event",
                    TargetRef = questEvent.QuestEventSeedId,
                    SourceRefs = [Ref("quest_event", questEvent.QuestEventSeedId)],
                    Parameters = SortedParameters(("progressState", "advanced"))
                });

                effects.Add(new EffectDefinition
                {
                    EffectId = "effect/grant_item/" + IdSegment(questEvent.QuestEventSeedId) + "/" + IdSegment(questEvent.RequiredItemSeedId),
                    EffectType = "effect/grant_item",
                    TargetRef = questEvent.RequiredItemSeedId,
                    FormulaId = "formula/quest_reward_count",
                    SourceRefs = [Ref("quest_event", questEvent.QuestEventSeedId), Ref("item", questEvent.RequiredItemSeedId)],
                    Parameters = SortedParameters(("grantReason", "quest_progress_reward"))
                });

                effects.Add(new EffectDefinition
                {
                    EffectId = "effect/adjust_reputation/" + IdSegment(questEvent.QuestEventSeedId) + "/" + IdSegment(questEvent.SourceFactionId),
                    EffectType = "effect/adjust_reputation",
                    TargetRef = questEvent.SourceFactionId,
                    FormulaId = "formula/faction_access_score",
                    SourceRefs = [Ref("quest_event", questEvent.QuestEventSeedId), Ref("faction", questEvent.SourceFactionId)],
                    Parameters = SortedParameters(("deltaKind", "quest_progress"))
                });
            }
        }

        return effects.OrderBy(item => item.EffectId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<ActionDefinition> BuildActions(
        ProceduralGeneratedGamePlan? sourcePlan,
        IReadOnlyList<string> selectedPlaceholderIds,
        IReadOnlyList<EffectDefinition> effects)
    {
        var actions = new List<ActionDefinition>();
        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter))
        {
            actions.Add(new ActionDefinition
            {
                ActionId = FormulaEffectActionRulePackConstants.ActionResolveEncounter,
                ActionType = FormulaEffectActionRulePackConstants.ActionResolveEncounter,
                Purpose = "Resolve generated encounters into explicit state and reward effects.",
                RequirementIds = selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess)
                    ? [FormulaEffectActionRulePackConstants.RequirementFactionAccess]
                    : [],
                EffectIds = effects
                    .Where(item => item.SourceRefs.Any(sourceRef => sourceRef.Kind == "encounter"))
                    .Select(item => item.EffectId)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                SourceRefs = sourcePlan?.EncounterSeeds
                    .Select(item => Ref("encounter", item.EncounterSeedId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress))
        {
            actions.Add(new ActionDefinition
            {
                ActionId = FormulaEffectActionRulePackConstants.RewardQuestProgress,
                ActionType = "action/grant_quest_progress",
                Purpose = "Advance quest/event state and grant deterministic resource or item rewards.",
                RequirementIds = selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess)
                    ? [FormulaEffectActionRulePackConstants.RequirementFactionAccess]
                    : [],
                EffectIds = effects
                    .Where(item => item.SourceRefs.Any(sourceRef => sourceRef.Kind == "quest_event"))
                    .Select(item => item.EffectId)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                SourceRefs = sourcePlan?.QuestEventSeeds
                    .Select(item => Ref("quest_event", item.QuestEventSeedId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        return actions.OrderBy(item => item.ActionId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<EventRuleDefinition> BuildEventRules(
        ProceduralGeneratedGamePlan? sourcePlan,
        IReadOnlyList<string> selectedPlaceholderIds)
    {
        var eventRules = new List<EventRuleDefinition>
        {
            new()
            {
                EventRuleId = "event_rule/on_enter_region",
                EventRuleType = "event_rule/on_enter_region",
                TriggerId = "trigger/on_enter_region",
                RequirementIds = selectedPlaceholderIds
                    .Where(id => id is FormulaEffectActionRulePackConstants.RequirementOpenRoute or FormulaEffectActionRulePackConstants.RequirementFactionAccess)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                SourceRefs = sourcePlan?.World.Regions
                    .Select(item => Ref("region", item.RegionId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            }
        };

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter))
        {
            eventRules.Add(new EventRuleDefinition
            {
                EventRuleId = "event_rule/on_resolve_encounter",
                EventRuleType = "event_rule/on_resolve_encounter",
                TriggerId = "trigger/on_resolve_encounter",
                RequirementIds = selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess)
                    ? [FormulaEffectActionRulePackConstants.RequirementFactionAccess]
                    : [],
                ActionIds = [FormulaEffectActionRulePackConstants.ActionResolveEncounter],
                SourceRefs = sourcePlan?.EncounterSeeds
                    .Select(item => Ref("encounter", item.EncounterSeedId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        if (selectedPlaceholderIds.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress))
        {
            eventRules.Add(new EventRuleDefinition
            {
                EventRuleId = "event_rule/on_complete_quest_event",
                EventRuleType = "event_rule/on_complete_quest_event",
                TriggerId = "trigger/on_complete_quest_event",
                ActionIds = [FormulaEffectActionRulePackConstants.RewardQuestProgress],
                SourceRefs = sourcePlan?.QuestEventSeeds
                    .Select(item => Ref("quest_event", item.QuestEventSeedId))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToList() ?? []
            });
        }

        return eventRules.OrderBy(item => item.EventRuleId, StringComparer.Ordinal).ToList();
    }

    private static string BuildStableSummary(
        IReadOnlyList<FormulaDefinition> formulas,
        IReadOnlyList<RequirementDefinition> requirements,
        IReadOnlyList<EffectDefinition> effects,
        IReadOnlyList<ActionDefinition> actions,
        IReadOnlyList<EventRuleDefinition> eventRules) =>
        string.Join("; ", new[]
        {
            $"formulas={formulas.Count}",
            $"requirements={requirements.Count}",
            $"effects={effects.Count}",
            $"actions={actions.Count}",
            $"eventRules={eventRules.Count}"
        });

    private static string IdSegment(string id) => id.Replace('/', '_');

    private static GeneratedPlanReference Ref(string kind, string id) => new()
    {
        Kind = kind,
        Id = id
    };

    private static IReadOnlyDictionary<string, string> SortedParameters(params (string Key, string Value)[] values) =>
        new SortedDictionary<string, string>(values.ToDictionary(item => item.Key, item => item.Value), StringComparer.Ordinal);

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static IReadOnlyList<FormulaEffectActionDiagnostic> SortDiagnostics(IEnumerable<FormulaEffectActionDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static FormulaEffectActionDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Formula/effect/action output path must stay under the project root.");
        }
    }

    private static readonly IReadOnlyList<string> BuiltInPlaceholderIds =
    [
        FormulaEffectActionRulePackConstants.RequirementOpenRoute,
        FormulaEffectActionRulePackConstants.RequirementFactionAccess,
        FormulaEffectActionRulePackConstants.ActionResolveEncounter,
        FormulaEffectActionRulePackConstants.RewardQuestProgress
    ];
}
