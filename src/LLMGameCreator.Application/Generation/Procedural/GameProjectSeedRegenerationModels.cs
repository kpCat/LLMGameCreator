using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class GameProjectSeedRegenerationVocabulary
{
    public const string TransactionSchemaVersion = "seed_regeneration_transaction_v2";
    public const string LegacyTransactionSchemaVersion = "seed_regeneration_transaction_v1";
    public const string ResultSchemaVersion = "seed_regeneration_result_v1";
    public const string CandidateSealSchemaVersion = "seed_regeneration_candidate_seal_v1";
    public const string RegenerationRelativeRoot = ".llmgc/regeneration";
    public const string TransactionsRelativeRoot = RegenerationRelativeRoot + "/transactions";
    public const string LastSuccessfulRelativePath = RegenerationRelativeRoot + "/last-successful-regeneration.json";
    public const string CandidateSealRelativePath = ".llmgc/regeneration-candidate/seal.json";
}

public sealed record GameProjectSeedRegenerationRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public SeededGeneratedProjectGenerationRequest GenerationRequest { get; init; } = new();
    public string ExpectedSourceRecordSha256 { get; init; } = string.Empty;
    public string ExpectedQualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public int ExpectedAuthoringRevision { get; init; }
    public string ExpectedActivatedPackageSha256 { get; init; } = string.Empty;
    public string ExpectedCompositionPackageSha256 { get; init; } = string.Empty;
    public string ExpectedFinalStateHash { get; init; } = string.Empty;
    public string ExpectedProjectIdentityFingerprint { get; init; } = string.Empty;
    public string? ExpectedReleaseCandidateRecordSha256 { get; init; }
}

public sealed record GameProjectSeedRegenerationTruthTokens
{
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public int AuthoringRevision { get; init; }
    public string ActivatedPackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string ProjectIdentityFingerprint { get; init; } = string.Empty;
    public string? ReleaseCandidateRecordSha256 { get; init; }
}

public sealed record GameProjectSeedRegenerationCollectionCounts
{
    public int Regions { get; init; }
    public int Factions { get; init; }
    public int Actors { get; init; }
    public int ItemsAndResources { get; init; }
    public int Encounters { get; init; }
    public int QuestEvents { get; init; }
}

public sealed record GameProjectSeedRegenerationDiff
{
    public string OldSeed { get; init; } = string.Empty;
    public string NewSeed { get; init; } = string.Empty;
    public string OldMode { get; init; } = string.Empty;
    public string NewMode { get; init; } = string.Empty;
    public string OldPresetId { get; init; } = string.Empty;
    public string NewPresetId { get; init; } = string.Empty;
    public string OldSourceRequestSha256 { get; init; } = string.Empty;
    public string NewSourceRequestSha256 { get; init; } = string.Empty;
    public string OldPlanSha256 { get; init; } = string.Empty;
    public string NewPlanSha256 { get; init; } = string.Empty;
    public string OldOverlaySha256 { get; init; } = string.Empty;
    public string NewOverlaySha256 { get; init; } = string.Empty;
    public string OldGeneratedBaseSha256 { get; init; } = string.Empty;
    public string NewGeneratedBaseSha256 { get; init; } = string.Empty;
    public GameProjectSeedRegenerationCollectionCounts OldCounts { get; init; } = new();
    public GameProjectSeedRegenerationCollectionCounts NewCounts { get; init; } = new();
    public int AddedRecordCount { get; init; }
    public int RemovedRecordCount { get; init; }
    public int ChangedRecordCount { get; init; }
    public int UnchangedRecordCount { get; init; }
    public IReadOnlyDictionary<string, int> AddedByCollection { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> RemovedByCollection { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> ChangedByCollection { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public string OldStartRegionTitle { get; init; } = string.Empty;
    public string NewStartRegionTitle { get; init; } = string.Empty;
    public string OldTravelDestinationTitle { get; init; } = string.Empty;
    public string NewTravelDestinationTitle { get; init; } = string.Empty;
    public bool GameplayChanged { get; init; }
    public bool AuthoringPreserved { get; init; }
    public bool ProjectIdentityPreserved { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectSeedRegenerationPreview
{
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = string.Empty;
    public string CurrentSourceSummary { get; init; } = string.Empty;
    public string CandidateSourceSummary { get; init; } = string.Empty;
    public GameProjectSeedRegenerationDiff? Diff { get; init; }
    public GameProjectBuildResult? CandidateBuild { get; init; }
    public UnifiedGameProjectWorkspaceSnapshot? CandidateSnapshot { get; init; }
    public GameProjectSeedRegenerationTruthTokens ExpectedTruthTokens { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Applied { get; init; }
    public bool RollbackApplied { get; init; }
    public IReadOnlyList<string> AuthoritativeFilesChanged { get; init; } = [];
    public string CandidateRoot { get; init; } = string.Empty;
    public string CandidateBuildHistoryFileName { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public string TransactionState { get; init; } = string.Empty;
}

public sealed record GameProjectSeedRegenerationResult
{
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = string.Empty;
    public GameProjectSeedRegenerationDiff? Diff { get; init; }
    public GameProjectBuildResult? CandidateBuild { get; init; }
    public UnifiedGameProjectWorkspaceSnapshot? AuthoritativeSnapshot { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Applied { get; init; }
    public bool RollbackApplied { get; init; }
    public IReadOnlyList<string> AuthoritativeFilesChanged { get; init; } = [];
    public string JournalStatus { get; init; } = string.Empty;
    public string BuildHistoryFileName { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public string TransactionState { get; init; } = string.Empty;
    public bool CommittedWithPresentationDiagnostic { get; init; }
}

public sealed record SeedRegenerationTransactionJournal
{
    public string SchemaVersion { get; init; } = GameProjectSeedRegenerationVocabulary.TransactionSchemaVersion;
    public string AttemptId { get; init; } = string.Empty;
    public string State { get; init; } = "prepared";
    public IReadOnlyList<string> AuthoritativeRelativePaths { get; init; } = [];
    public IReadOnlyDictionary<string, string> BeforeSha256 { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> CandidateSha256 { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> AppliedStepIds { get; init; } = [];
    public string ExpectedAuthoritativeInventorySha256 { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
}

public enum GameProjectSeedRegenerationFailurePoint
{
    None,
    AfterGenerationSwap,
    AfterSupportReplace,
    AfterPackageReplace,
    AfterAuthoringReplace,
    AfterHistoryAdd,
    BeforeFinalValidation
    ,DuringSemanticValidation
}

public sealed record GameProjectSeedRegenerationTransactionRequest
{
    public string AttemptId { get; init; } = string.Empty;
    public string ProjectFolder { get; init; } = string.Empty;
    public string CandidateFolder { get; init; } = string.Empty;
    public string CandidateBuildHistoryFileName { get; init; } = string.Empty;
    public string RegenerationRecordJson { get; init; } = string.Empty;
    public GameProjectSeedRegenerationFailurePoint FailurePoint { get; init; }
    public GameProjectSeedRegenerationTruthTokens ExpectedTruthTokens { get; init; } = new();
    public string ExpectedAuthoritativeInventorySha256 { get; init; } = string.Empty;
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public GameProjectOperationLease? OperationLease { get; init; }
    public IGameProjectSeedRegenerationTruthReader? TruthReader { get; init; }
    public IGameProjectSeedRegenerationCommitValidator? CommitValidator { get; init; }
    public GameProjectSeedRegenerationCommitValidationRequest? CommitValidationRequest { get; init; }
    public GeneratedWorldHistoryService? WorldHistoryService { get; init; }
    public string BeforeWorldHistoryOperationKind { get; init; } = string.Empty;
    public string AfterWorldHistoryOperationKind { get; init; } = string.Empty;
    public string WorldChangeRecordRelativePath { get; init; } = string.Empty;
    public string WorldChangeRecordJson { get; init; } = string.Empty;
}

public sealed record GameProjectSeedRegenerationTransactionResult
{
    public bool Passed { get; init; }
    public bool Applied { get; init; }
    public bool RollbackApplied { get; init; }
    public string JournalStatus { get; init; } = string.Empty;
    public string BuildHistoryFileName { get; init; } = string.Empty;
    public IReadOnlyList<string> ChangedRelativePaths { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string TransactionState { get; init; } = string.Empty;
    public string FromWorldId { get; init; } = string.Empty;
    public string ToWorldId { get; init; } = string.Empty;
}

public sealed record GameProjectSeedRegenerationCandidateSeal
{
    public string SchemaVersion { get; init; } = GameProjectSeedRegenerationVocabulary.CandidateSealSchemaVersion;
    public string AttemptId { get; init; } = string.Empty;
    public string CandidateRootIdentity { get; init; } = string.Empty;
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string GenerationTreeSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string AuthoringTreeSha256 { get; init; } = string.Empty;
    public string IdentitySha256 { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string SelectedBuildHistorySha256 { get; init; } = string.Empty;
    public string SupportTreeSha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string SelectedModuleIdsSha256 { get; init; } = string.Empty;
    public string ParameterValuesSha256 { get; init; } = string.Empty;
    public string CandidatePackageSha256 { get; init; } = string.Empty;
    public string CandidateCompositionSha256 { get; init; } = string.Empty;
    public string CandidateFinalStateHash { get; init; } = string.Empty;
    public string CandidateSourceRequestSha256 { get; init; } = string.Empty;
    public string CandidatePlanSha256 { get; init; } = string.Empty;
    public string CandidateOverlaySha256 { get; init; } = string.Empty;
    public string CandidateGeneratedBaseSha256 { get; init; } = string.Empty;
    public string CandidateSnapshotStatus { get; init; } = string.Empty;
    public string DiffSha256 { get; init; } = string.Empty;
    public string SealSha256 { get; init; } = string.Empty;
}

internal sealed record SealedRegenerationCandidate
{
    public string CandidateRoot { get; init; } = string.Empty;
    public GameProjectSeedRegenerationCandidateSeal Seal { get; init; } = new();
    public GameProjectSeedRegenerationPreview PublicPreview { get; init; } = new();
    public GameProjectBuildResult CandidateBuild { get; init; } = new();
    public UnifiedGameProjectWorkspaceSnapshot CandidateSnapshot { get; init; } = new();
    public GameProjectSeedRegenerationDiff Diff { get; init; } = new();
    public GameProjectSeedRegenerationTruthTokens ExpectedTruthTokens { get; init; } = new();
    public string ExpectedAuthoritativeInventorySha256 { get; init; } = string.Empty;
}

public interface IGameProjectSeedRegenerationTruthReader
{
    GameProjectSeedRegenerationTruthTokens CaptureTruthTokens(
        string projectFolder,
        GameProjectOperationLease operationLease);
    string CaptureAuthoritativeInventorySha256(string projectFolder);
}

public sealed record GameProjectSeedRegenerationCommitValidationRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string OperationKind { get; init; } = "regeneration";
    public GameProjectSeedRegenerationCandidateSeal CandidateSeal { get; init; } = new();
    public string ExpectedProjectIdentityFingerprint { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string? PreviousReleaseCandidateRecordSha256 { get; init; }
    public string ExpectedWorldChangeRecordSha256 { get; init; } = string.Empty;
}

public sealed record GameProjectSeedRegenerationCommitValidationResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public interface IGameProjectSeedRegenerationCommitValidator
{
    GameProjectSeedRegenerationCommitValidationResult Validate(
        GameProjectSeedRegenerationCommitValidationRequest request,
        GameProjectOperationLease operationLease);
}

public sealed record GameProjectSeedRegenerationRecord
{
    public string SchemaVersion { get; init; } = GameProjectSeedRegenerationVocabulary.ResultSchemaVersion;
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "GREEN";
    public string OldSourceRecordSha256 { get; init; } = string.Empty;
    public string NewSourceRecordSha256 { get; init; } = string.Empty;
    public string OldRequestSha256 { get; init; } = string.Empty;
    public string NewRequestSha256 { get; init; } = string.Empty;
    public string OldPlanSha256 { get; init; } = string.Empty;
    public string NewPlanSha256 { get; init; } = string.Empty;
    public string OldOverlaySha256 { get; init; } = string.Empty;
    public string NewOverlaySha256 { get; init; } = string.Empty;
    public string OldGeneratedBaseSha256 { get; init; } = string.Empty;
    public string NewGeneratedBaseSha256 { get; init; } = string.Empty;
    public string OldPackageSha256 { get; init; } = string.Empty;
    public string NewPackageSha256 { get; init; } = string.Empty;
    public string NewCompositionPackageSha256 { get; init; } = string.Empty;
    public string NewFinalStateHash { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public int SelectedModuleCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public GameProjectSeedRegenerationDiff Diff { get; init; } = new();
    public string CandidateBuildHistoryFileName { get; init; } = string.Empty;
    public string? PreviousReleaseCandidateRecordSha256 { get; init; }
    public string PreviousReleaseCandidateStatus { get; init; } = "ABSENT";
}

public sealed record GameProjectSeedRegenerationRecordReadResult
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public GameProjectSeedRegenerationRecord? Record { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
