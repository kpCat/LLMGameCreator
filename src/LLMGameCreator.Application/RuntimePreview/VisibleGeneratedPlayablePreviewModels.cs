using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed record VisibleGeneratedPlayablePreviewRequest
{
    public string Seed { get; init; } = GenerationPresetOptionsService.DefaultSeed;
    public string Mode { get; init; } = ProceduralGameGenerationModes.SemiProceduralRegions;
    public string PresetId { get; init; } = GenerationPresetOptionsService.DefaultPresetId;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
}

public sealed record VisibleGeneratedPlayablePreviewResult
{
    public ProceduralGameKernelResult PlanResult { get; init; } = new();
    public FormulaEffectActionRegistryResult RulePackResult { get; init; } = new();
    public TinyGeneratedRuntimeLoopResult TinyLoopResult { get; init; } = new();
    public GeneratedPackageMvpResult PackageMvpResult { get; init; } = new();
    public VisibleGeneratedPlayablePreviewSnapshot Snapshot { get; init; } = new();
    public VisibleGeneratedPlayablePreviewReport Report { get; init; } = new();
    public string SnapshotJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record VisibleGeneratedPlayablePreviewWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string SnapshotJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record VisibleGeneratedPlayablePreviewSnapshot
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
    public VisibleGeneratedPlayablePreviewSourceHashes SourceHashes { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public VisibleGeneratedPlayableRuntimeAttempt RuntimeAttempt { get; init; } = new();
    public GeneratedPackageRuntimePreviewModel Projection { get; init; } = new();
    public GeneratedMicrogameGoalPreviewModel MicrogameGoal { get; init; } = new();
    public GeneratedMicrogameChallengePreviewModel MicrogameChallenge { get; init; } = new();
    public VisibleGeneratedPlayablePreviewCounts Counts { get; init; } = new();
    public VisibleGeneratedPlayablePreviewRepresentativeIds RepresentativeGeneratedIds { get; init; } = new();
    public IReadOnlyList<VisibleGeneratedPlayablePreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<VisibleGeneratedPlayablePreviewDiagnostic>();
}

public sealed record VisibleGeneratedPlayablePreviewReport
{
    public string SchemaVersion { get; init; } = "1";
    public string SnapshotHash { get; init; } = string.Empty;
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
    public string StableSummary { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool RuntimeCommandAttempted { get; init; }
    public bool RuntimeCommandSucceeded { get; init; }
    public bool ActiveGoalSelected { get; init; }
    public bool GoalProgressAdvanced { get; init; }
    public bool ChallengeResolved { get; init; }
    public bool RewardVisible { get; init; }
    public bool CompletionVisible { get; init; }
    public int DiagnosticCount { get; init; }
    public VisibleGeneratedPlayablePreviewSourceHashes SourceHashes { get; init; } = new();
    public IReadOnlyList<VisibleGeneratedPlayablePreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<VisibleGeneratedPlayablePreviewDiagnostic>();
}

public sealed record VisibleGeneratedPlayablePreviewSourceHashes
{
    public string PlanHash { get; init; } = string.Empty;
    public string RulePackHash { get; init; } = string.Empty;
    public string TinyLoopStateHash { get; init; } = string.Empty;
    public string GeneratedPackageFinalHash { get; init; } = string.Empty;
}

public sealed record VisibleGeneratedPlayablePreviewCounts
{
    public int PackageMaps { get; init; }
    public int PackageItems { get; init; }
    public int PackageEncounters { get; init; }
    public int PackageQuests { get; init; }
    public int Regions { get; init; }
    public int Npcs { get; init; }
    public int Items { get; init; }
    public int Encounters { get; init; }
    public int Quests { get; init; }
    public int ActiveGoals { get; init; }
    public int ActiveGoalCompletedSteps { get; init; }
    public int ActiveGoalTotalSteps { get; init; }
    public int ResolvedChallenges { get; init; }
    public int VisibleRewards { get; init; }
    public int VisibleCompletions { get; init; }
    public int Mechanics { get; init; }
    public int ProvenanceRecords { get; init; }
}

public sealed record VisibleGeneratedPlayablePreviewRepresentativeIds
{
    public IReadOnlyList<string> RegionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> NpcIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ItemIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EncounterIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> QuestIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MechanicIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ProvenanceArtifactIds { get; init; } = Array.Empty<string>();
}

public sealed record VisibleGeneratedPlayableRuntimeAttempt
{
    public bool RuntimeStartAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public string StartMapId { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public VisibleGeneratedPlayablePosition PlayerStartPosition { get; init; } = new();
    public VisibleGeneratedPlayablePosition PlayerCurrentPosition { get; init; } = new();
    public IReadOnlyList<VisibleGeneratedPlayableRuntimeCommandAttempt> CommandAttempts { get; init; } = Array.Empty<VisibleGeneratedPlayableRuntimeCommandAttempt>();
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<VisibleGeneratedPlayablePreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<VisibleGeneratedPlayablePreviewDiagnostic>();
}

public sealed record VisibleGeneratedPlayableRuntimeCommandAttempt
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string CurrentMapId { get; init; } = string.Empty;
    public VisibleGeneratedPlayablePosition PlayerPosition { get; init; } = new();
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EventTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EventMessages { get; init; } = Array.Empty<string>();
}

public sealed record VisibleGeneratedPlayablePosition
{
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record VisibleGeneratedPlayablePreviewDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public interface IVisibleGeneratedPlayableRuntimeAdapter
{
    VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package);
}
