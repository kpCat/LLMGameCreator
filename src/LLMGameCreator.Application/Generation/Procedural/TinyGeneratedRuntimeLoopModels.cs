namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record TinyGeneratedRuntimeLoopRequest
{
    public ProceduralGeneratedGamePlan? SourcePlan { get; init; }
    public FormulaEffectActionRulePack? RulePack { get; init; }
    public FormulaEffectActionValidationReport? RulePackValidationReport { get; init; }
}

public sealed record TinyGeneratedRuntimeLoopResult
{
    public TinyGeneratedRuntimeState State { get; init; } = new();
    public TinyGeneratedRuntimeLoopReport Report { get; init; } = new();
    public string StateJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<TinyGeneratedRuntimeDiagnostic> Diagnostics { get; init; } = Array.Empty<TinyGeneratedRuntimeDiagnostic>();
}

public sealed record TinyGeneratedRuntimeLoopWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StateJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record TinyGeneratedRuntimeState
{
    public string SchemaVersion { get; init; } = "1";
    public TinyGeneratedRuntimeSourceMetadata Source { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public string StartingRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> VisitedRegionIds { get; init; } = Array.Empty<string>();
    public string ResolvedEncounterId { get; init; } = string.Empty;
    public string AdvancedQuestEventId { get; init; } = string.Empty;
    public IReadOnlyList<string> AppliedActionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AppliedEffectIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, int> InventoryItemCounts { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, bool> Flags { get; init; } = new SortedDictionary<string, bool>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> FactionReputationDeltas { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> QuestEventStates { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<TinyGeneratedRuntimeDiagnostic> Diagnostics { get; init; } = Array.Empty<TinyGeneratedRuntimeDiagnostic>();
}

public sealed record TinyGeneratedRuntimeSourceMetadata
{
    public string PlanId { get; init; } = string.Empty;
    public string PlanHash { get; init; } = string.Empty;
    public string RulePackId { get; init; } = string.Empty;
    public string RulePackHash { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
}

public sealed record TinyGeneratedRuntimeLoopReport
{
    public string SchemaVersion { get; init; } = "1";
    public TinyGeneratedRuntimeSourceMetadata Source { get; init; } = new();
    public string StateHash { get; init; } = string.Empty;
    public string StableSummary { get; init; } = string.Empty;
    public bool HasErrors { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<TinyGeneratedRuntimeStep> Steps { get; init; } = Array.Empty<TinyGeneratedRuntimeStep>();
    public IReadOnlyList<TinyGeneratedRuntimeDiagnostic> Diagnostics { get; init; } = Array.Empty<TinyGeneratedRuntimeDiagnostic>();
}

public sealed record TinyGeneratedRuntimeStep
{
    public string StepId { get; init; } = string.Empty;
    public string StepType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record TinyGeneratedRuntimeDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
