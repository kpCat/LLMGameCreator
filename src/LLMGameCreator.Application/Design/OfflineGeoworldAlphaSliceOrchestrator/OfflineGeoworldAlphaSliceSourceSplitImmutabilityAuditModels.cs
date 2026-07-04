namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public static class OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary
{
    public const string GoalId = "goal_108a_alpha_slice_source_split_immutability_audit";
    public const string ProductSmokeRoute = "goal-108a-alpha-slice-source-split-immutability-audit";
    public const string ParentCommit = "14ad9f38";
    public const string Goal108Commit = "989a79ab";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit";

    public const string ReportMarkdownFileName = "alpha-slice-source-split-immutability-audit-report.md";
    public const string SourceHealthBeforeAfterFileName = "alpha-slice-source-health-before-after.json";
    public const string HistoricalArtifactDiffAuditFileName = "alpha-slice-historical-artifact-diff-audit.json";
    public const string ImmutabilityTrustAuditFileName = "alpha-slice-immutability-trust-audit.json";
    public const string SourceSplitQualityGateFileName = "alpha-slice-source-split-quality-gate.json";
    public const string NegativeProofFileName = "alpha-slice-negative-proof.json";

    public static IReadOnlyList<string> RequiredEvidenceFileNames =>
    [
        SourceHealthBeforeAfterFileName,
        HistoricalArtifactDiffAuditFileName,
        ImmutabilityTrustAuditFileName,
        SourceSplitQualityGateFileName,
        NegativeProofFileName
    ];

    public static IReadOnlyList<string> HistoricalArtifactPathspecs =>
    [
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner",
        ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool",
        ".llmgc/procedural/goal-102a-unity-editor-source-format-guard",
        ".llmgc/procedural/goal-102b-actual-unity-editor-source-reformat",
        ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview",
        ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview",
        ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe",
        ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay",
        ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run",
        ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal102",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108"
    ];

    public static IReadOnlyList<string> RequiredNegativeScenarioIds =>
    [
        "service_file_still_over_700_lines",
        "actual_git_diff_not_read",
        "fake_historical_unchanged_claim_without_parent_head_comparison",
        "goal108a_historical_artifact_mutation_attempt",
        "alpha_runtime_bootstrap_changed",
        "runtime_schema_provider_unity_scene_settings_changed"
    ];
}

public sealed record OfflineGeoworldAlphaSliceSourceSplitAuditBuildResult
{
    public OfflineGeoworldAlphaSliceSourceHealthBeforeAfter SourceHealthBeforeAfter { get; init; } = new();
    public OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit HistoricalArtifactDiffAudit { get; init; } = new();
    public OfflineGeoworldAlphaSliceImmutabilityTrustAudit ImmutabilityTrustAudit { get; init; } = new();
    public OfflineGeoworldAlphaSliceAuditNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaSliceSourceSplitQualityGate QualityGate { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceSourceSplitAuditWriteResult
{
    public OfflineGeoworldAlphaSliceSourceSplitAuditBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceSourceHealthBeforeAfter
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string BeforeSource { get; init; } = "git_commit_989a79ab";
    public string AfterSource { get; init; } = "working_tree_after_source_split";
    public bool BeforeScanReadActualGitBlob { get; init; }
    public bool SourceSplitCompleted { get; init; }
    public bool BeforeHadFileOver700Lines { get; init; }
    public bool AllAfterFilesBelow700Lines { get; init; }
    public OfflineGeoworldAlphaSliceSourceHealthScan Before { get; init; } = new();
    public OfflineGeoworldAlphaSliceSourceHealthScan After { get; init; } = new();
}

public sealed record OfflineGeoworldAlphaSliceSourceHealthScan
{
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public int MaxPhysicalLineCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceSourceHealthFile> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceSourceHealthFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public int PhysicalLineCount { get; init; }
    public int LogicalLineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceHistoricalArtifactDiffAudit
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string ParentCommit { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.ParentCommit;
    public string Goal108Commit { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.Goal108Commit;
    public bool GitDiffRead { get; init; }
    public string GitDiffCommand { get; init; } = string.Empty;
    public int ChangedPathCount { get; init; }
    public int Goal108ChangedPathCount { get; init; }
    public int Goal101To107ChangedPathCount { get; init; }
    public bool Goal101To107ArtifactsModified { get; init; }
    public IReadOnlyList<string> Goal101To107ChangedPaths { get; init; } = [];
    public IReadOnlyList<string> Goal108ChangedPaths { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldAlphaSliceHistoricalArtifactDiffRecord> ChangedPaths { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceHistoricalArtifactDiffRecord
{
    public string Status { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public int Additions { get; init; }
    public int Deletions { get; init; }
    public string OldBlobSha { get; init; } = string.Empty;
    public string NewBlobSha { get; init; } = string.Empty;
    public int GoalNumber { get; init; }
    public bool IsGoal101To107Artifact { get; init; }
    public bool IsGoal108Artifact { get; init; }
}

public sealed record OfflineGeoworldAlphaSliceImmutabilityTrustAudit
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal108ClaimRead { get; init; }
    public bool Goal108HistoricalArtifactsUnchangedClaim { get; init; }
    public bool ActualGitDiffRead { get; init; }
    public bool ActualGoal101To107ArtifactsUnchanged { get; init; }
    public bool Goal108ClaimMatchesActualGitDiff { get; init; }
    public bool EvidenceTrustDebtRecorded { get; init; }
    public string EvidenceTrustDebtReason { get; init; } = string.Empty;
    public string HistoricalScope { get; init; } =
        "Goal101-107 evidence and Unity StreamingAssets payload roots";
    public bool Goal108AdditionsClassifiedAsCurrentGoalOutput { get; init; }
}

public sealed record OfflineGeoworldAlphaSliceAuditNegativeProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceAuditNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceAuditNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceSourceSplitQualityGate
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.GoalId;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool SourceSplitCompleted { get; init; }
    public bool LargestGoal108OrchestratorFileBelow700Lines { get; init; }
    public int MaxPhysicalLineCountAfterSplit { get; init; }
    public int MaxLogicalLineCountAfterSplit { get; init; }
    public bool ActualGitDiffAuditPerformed { get; init; }
    public bool Goal101To107ArtifactsModified { get; init; }
    public bool Goal108ClaimMatchesActualGitDiff { get; init; }
    public bool EvidenceTrustDebtStatusHonest { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoForbiddenAreasChanged { get; init; }
    public IReadOnlyList<string> CurrentChangedPaths { get; init; } = [];
    public IReadOnlyList<string> ForbiddenChangedPaths { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
