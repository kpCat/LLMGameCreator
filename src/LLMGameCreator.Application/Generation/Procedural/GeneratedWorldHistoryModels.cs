using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class GeneratedWorldHistoryVocabulary
{
    public const string SchemaVersion = "generated_world_history_v1";
    public const string RelativeRoot = ".llmgc/regeneration/world-history";
}

public static class GeneratedWorldHistoryOperationKinds
{
    public const string InitialCapture = "initial_capture";
    public const string RegenerationBefore = "regeneration_before";
    public const string RegenerationAfter = "regeneration_after";
    public const string HistoryRollbackBefore = "history_rollback_before";
    public const string HistoryRollbackAfter = "history_rollback_after";
}

public sealed record GeneratedWorldHistoryManifest
{
    public string SchemaVersion { get; init; } = GeneratedWorldHistoryVocabulary.SchemaVersion;
    public string WorldId { get; init; } = string.Empty;
    public string SourceSchemaVersion { get; init; } = string.Empty;
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string SourceRequestSha256 { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string OverlaySha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string PresetId { get; init; } = string.Empty;
    public IReadOnlyList<string> ResolvedStyleHintIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedVariantIds { get; init; } = [];
    public GeneratedProjectCounts Counts { get; init; } = new();
    public string StartRegionTitle { get; init; } = string.Empty;
    public string TravelDestinationTitle { get; init; } = string.Empty;
    public string GenerationTreeSha256 { get; init; } = string.Empty;
    public string CreatedByOperationKind { get; init; } = string.Empty;
}

public sealed record GeneratedWorldHistoryEntry
{
    public bool Passed { get; init; }
    public bool IsCurrent { get; init; }
    public string WorldId { get; init; } = string.Empty;
    public string EntryPath { get; init; } = string.Empty;
    public GeneratedWorldHistoryManifest? Manifest { get; init; }
    public SeededGeneratedProjectSourceValidationResult? SourceValidation { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedWorldHistoryReadResult
{
    public bool Passed { get; init; }
    public string CurrentWorldId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedWorldHistoryEntry> Entries { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedWorldHistoryStageResult
{
    public bool Passed { get; init; }
    public bool AlreadyPresent { get; init; }
    public string WorldId { get; init; } = string.Empty;
    public string StagedEntryPath { get; init; } = string.Empty;
    public GeneratedWorldHistoryManifest? Manifest { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectGeneratedWorldRollbackRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string TargetWorldId { get; init; } = string.Empty;
    public GameProjectSeedRegenerationTruthTokens ExpectedTruthTokens { get; init; } = new();
    public string ExpectedAuthoritativeInventorySha256 { get; init; } = string.Empty;
    public string ExpectedWorldHistoryManifestSha256 { get; init; } = string.Empty;
    public string ExpectedWorldHistoryTreeSha256 { get; init; } = string.Empty;
}

public sealed record GameProjectGeneratedWorldRollbackPreview
{
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = string.Empty;
    public string TargetWorldId { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public string CandidateRoot { get; init; } = string.Empty;
    public string CandidateBuildHistoryFileName { get; init; } = string.Empty;
    public GameProjectSeedRegenerationDiff? Diff { get; init; }
    public GameProjectBuildResult? CandidateBuild { get; init; }
    public UnifiedGameProjectWorkspaceSnapshot? CandidateSnapshot { get; init; }
    public GeneratedWorldHistoryManifest? TargetManifest { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectGeneratedWorldRollbackResult
{
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = string.Empty;
    public string TargetWorldId { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public GameProjectSeedRegenerationDiff? Diff { get; init; }
    public GameProjectBuildResult? CandidateBuild { get; init; }
    public UnifiedGameProjectWorkspaceSnapshot? AuthoritativeSnapshot { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Applied { get; init; }
    public bool RollbackApplied { get; init; }
    public string JournalStatus { get; init; } = string.Empty;
    public string TransactionState { get; init; } = string.Empty;
    public string BuildHistoryFileName { get; init; } = string.Empty;
    public bool CommittedWithPresentationDiagnostic { get; init; }
}

internal sealed record SealedGeneratedWorldRollbackCandidate
{
    public string CandidateRoot { get; init; } = string.Empty;
    public GameProjectSeedRegenerationCandidateSeal Seal { get; init; } = new();
    public GameProjectGeneratedWorldRollbackPreview PublicPreview { get; init; } = new();
    public GameProjectBuildResult CandidateBuild { get; init; } = new();
    public UnifiedGameProjectWorkspaceSnapshot CandidateSnapshot { get; init; } = new();
    public GameProjectSeedRegenerationDiff Diff { get; init; } = new();
    public GameProjectSeedRegenerationTruthTokens ExpectedTruthTokens { get; init; } = new();
    public string ExpectedAuthoritativeInventorySha256 { get; init; } = string.Empty;
    public GeneratedWorldHistoryManifest TargetManifest { get; init; } = new();
    public string ExpectedWorldHistoryManifestSha256 { get; init; } = string.Empty;
    public string ExpectedWorldHistoryTreeSha256 { get; init; } = string.Empty;
}
