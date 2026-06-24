namespace LLMGameCreator.Application.Generation.Procedural;

public static class FormulaEffectActionRulePackConstants
{
    public const string SchemaVersion = "1";
    public const string RulePackId = "rule_pack/formula_effect_action_registry_v1";

    public const string RequirementOpenRoute = "requirement/open_route";
    public const string RequirementFactionAccess = "requirement/faction_access";
    public const string ActionResolveEncounter = "action/resolve_encounter";
    public const string RewardQuestProgress = "reward/quest_progress";
}

public sealed record FormulaEffectActionRegistryRequest
{
    public ProceduralGeneratedGamePlan? SourcePlan { get; init; }
    public IReadOnlyList<string> SelectedPlaceholderIds { get; init; } = Array.Empty<string>();
    public bool StrictMode { get; init; }
    public string? ProjectFolderPath { get; init; }
}

public sealed record FormulaEffectActionRegistryResult
{
    public FormulaEffectActionRulePack RulePack { get; init; } = new();
    public FormulaEffectActionValidationReport ValidationReport { get; init; } = new();
    public string Json { get; init; } = string.Empty;
    public string Markdown { get; init; } = string.Empty;
    public string ValidationReportJson { get; init; } = string.Empty;
    public string ValidationReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<FormulaEffectActionDiagnostic> Diagnostics { get; init; } = Array.Empty<FormulaEffectActionDiagnostic>();
}

public sealed record FormulaEffectActionRulePackWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string RulePackJsonPath { get; init; } = string.Empty;
    public string RulePackMarkdownPath { get; init; } = string.Empty;
    public string ValidationReportJsonPath { get; init; } = string.Empty;
    public string ValidationReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record FormulaEffectActionRulePack
{
    public FormulaEffectActionRulePackMetadata Metadata { get; init; } = new();
    public IReadOnlyList<FormulaDefinition> Formulas { get; init; } = Array.Empty<FormulaDefinition>();
    public IReadOnlyList<RequirementDefinition> Requirements { get; init; } = Array.Empty<RequirementDefinition>();
    public IReadOnlyList<EffectDefinition> Effects { get; init; } = Array.Empty<EffectDefinition>();
    public IReadOnlyList<ActionDefinition> Actions { get; init; } = Array.Empty<ActionDefinition>();
    public IReadOnlyList<EventRuleDefinition> EventRules { get; init; } = Array.Empty<EventRuleDefinition>();
    public IReadOnlyList<FormulaEffectActionDiagnostic> Diagnostics { get; init; } = Array.Empty<FormulaEffectActionDiagnostic>();
    public string MarkdownSummary { get; init; } = string.Empty;
}

public sealed record FormulaEffectActionRulePackMetadata
{
    public string SchemaVersion { get; init; } = FormulaEffectActionRulePackConstants.SchemaVersion;
    public string RulePackId { get; init; } = FormulaEffectActionRulePackConstants.RulePackId;
    public string SourcePlanId { get; init; } = string.Empty;
    public string SourcePlanHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record FormulaDefinition
{
    public string FormulaId { get; init; } = string.Empty;
    public string Expression { get; init; } = string.Empty;
    public IReadOnlyList<string> DeclaredVariables { get; init; } = Array.Empty<string>();
    public string ResultType { get; init; } = "number";
    public decimal? MinimumValue { get; init; }
    public decimal? MaximumValue { get; init; }
}

public sealed record RequirementDefinition
{
    public string RequirementId { get; init; } = string.Empty;
    public string RequirementType { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string FormulaId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedPlanReference> SourceRefs { get; init; } = Array.Empty<GeneratedPlanReference>();
    public string PredicateSlot { get; init; } = string.Empty;
}

public sealed record EffectDefinition
{
    public string EffectId { get; init; } = string.Empty;
    public string EffectType { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string FormulaId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedPlanReference> SourceRefs { get; init; } = Array.Empty<GeneratedPlanReference>();
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record ActionDefinition
{
    public string ActionId { get; init; } = string.Empty;
    public string ActionType { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public IReadOnlyList<string> RequirementIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EffectIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratedPlanReference> SourceRefs { get; init; } = Array.Empty<GeneratedPlanReference>();
}

public sealed record EventRuleDefinition
{
    public string EventRuleId { get; init; } = string.Empty;
    public string EventRuleType { get; init; } = string.Empty;
    public string TriggerId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequirementIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratedPlanReference> SourceRefs { get; init; } = Array.Empty<GeneratedPlanReference>();
}

public sealed record GeneratedPlanReference
{
    public string Kind { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
}

public sealed record FormulaEffectActionDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record FormulaEffectActionValidationReport
{
    public string SchemaVersion { get; init; } = FormulaEffectActionRulePackConstants.SchemaVersion;
    public string RulePackId { get; init; } = string.Empty;
    public string RulePackHash { get; init; } = string.Empty;
    public int DiagnosticCount { get; init; }
    public bool HasErrors { get; init; }
    public IReadOnlyList<FormulaEffectActionDiagnostic> Diagnostics { get; init; } = Array.Empty<FormulaEffectActionDiagnostic>();
}
