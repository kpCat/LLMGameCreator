using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class SelectedRuntimeVariantPlayerAdapterVocabulary
{
    public const string GoalId =
        "goal_143_selected_runtime_variant_end_to_end_playeradapter_handoff";
    public const string ScenarioId =
        "goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff";
    public const string CandidateId = "minimal-map-game-exploration-resource-focus";
    public const string RecipeId = "exploration_resource_focus";
    public const string VariantKind = "exploration_resource_focus";
    public const int Score = 100;

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff";
    public const string SourceGoal142Root =
        ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string SourceSelectedHandoffPath =
        SourceGoal142Root + "/selected-runtime-variant/selected-runtime-variant-handoff.json";
    public const string SourcePackagePath =
        SourceGoal142Root + "/selected-runtime-variant/package.json";
    public const string SourceOutcomePath =
        SourceGoal142Root + "/selected-runtime-variant/runtime-outcome-summary.json";
    public const string SourceRoundtripResultPath =
        SourceGoal142Root + "/matrix/" + CandidateId + "/roundtrip-result.json";
    public const string NormalCommand =
        ".devflow\\scripts\\run-selected-runtime-variant-playeradapter-handoff.cmd";
    public const string ScriptPath =
        ".devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.cmd";
    public const string DocumentationPath =
        "docs/manual-acceptance/selected-runtime-variant-end-to-end-playeradapter-handoff.md";

    public const string AcceptanceFileName = "goal142-human-acceptance-record.json";
    public const string HandoffFileName = "selected-runtime-variant-playeradapter-handoff.json";
    public const string ModelFileName = "selected-runtime-variant-playeradapter-model.json";
    public const string FramesFileName = "selected-runtime-variant-playeradapter-frames.json";
    public const string ResultFileName = "selected-runtime-variant-playeradapter-result.json";
    public const string DashboardFileName = "selected-runtime-variant-playeradapter-dashboard.json";
    public const string NegativeProofFileName =
        "selected-runtime-variant-playeradapter-negative-proof.json";
    public const string FileIndexFileName = "selected-runtime-variant-playeradapter-file-index.json";
    public const string UnitySmokeFileName = "unity-selected-runtime-variant-playeradapter-smoke.json";
    public const string OneClickReportJsonFileName =
        "one-click-selected-runtime-variant-playeradapter-report.json";
    public const string OneClickReportMarkdownFileName =
        "one-click-selected-runtime-variant-playeradapter-report.md";

    public const string HandoffRelativePath = ProceduralOutputDirectory + "/" + HandoffFileName;
    public const string ModelRelativePath = ProceduralOutputDirectory + "/" + ModelFileName;
    public const string FramesRelativePath = ProceduralOutputDirectory + "/" + FramesFileName;
    public const string ResultRelativePath = ProceduralOutputDirectory + "/" + ResultFileName;
    public const string DashboardRelativePath = ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string UnitySmokeRelativePath = ProceduralOutputDirectory + "/" + UnitySmokeFileName;

    public const string HumanDecision =
        "Я принимаю Goal142 "
        + "runtime_significant_product_line_variant_matrix_and_selection_handoff_verification "
        + "GREEN. candidateCount=4, passedCandidateCount=4, "
        + "runtimeSignificantCandidateCount=4, distinctFinalStateHashCount=4, "
        + "selectedCandidate=minimal-map-game-exploration-resource-focus, "
        + "selectedScore=100, sourceTemplateUnmodified=true, "
        + "operatorUsesInProcessService=true, operatorExitCode=0, "
        + "projectionOnly=false, runtimeAuthority=true.";
}

public sealed record SelectedRuntimeVariantPlayerAdapterRequest
{
    public string SelectedHandoffPath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.SourceSelectedHandoffPath;
    public string SelectedPackagePath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.SourcePackagePath;
    public string SelectedOutcomePath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.SourceOutcomePath;
    public string SelectedRoundtripResultPath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.SourceRoundtripResultPath;
    public string OutputRoot { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory;
    public string UnitySmokePath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.UnitySmokeRelativePath;
}

public sealed record Goal142HumanAcceptanceRecord
{
    public string GoalId { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
    public string Decision { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.HumanDecision;
    public int CandidateCount { get; init; } = 4;
    public int PassedCandidateCount { get; init; } = 4;
    public int RuntimeSignificantCandidateCount { get; init; } = 4;
    public int DistinctFinalStateHashCount { get; init; } = 4;
    public string SelectedCandidate { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId;
    public int SelectedScore { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.Score;
    public bool SourceTemplateUnmodified { get; init; } = true;
    public bool OperatorUsesInProcessService { get; init; } = true;
    public int OperatorExitCode { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
}

public sealed record SelectedRuntimeVariantPlayerAdapterFrame
{
    public int FrameIndex { get; init; }
    public int HumanFrameNumber { get; init; }
    public string RequestId { get; init; } = string.Empty;
    public int RequestIndex { get; init; }
    public string ControlIntent { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string RequestedOperation { get; init; } = string.Empty;
    public int CanonicalStepIndex { get; init; }
    public string CanonicalStepId { get; init; } = string.Empty;
    public string StateHashBefore { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
    public string MapSummary { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public bool RuntimeExecuted { get; init; }
    public bool RuntimeMutation { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterFrames
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_frames_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string CandidateId { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId;
    public int FrameCount { get; init; }
    public IReadOnlyList<SelectedRuntimeVariantPlayerAdapterFrame> Frames { get; init; } = [];
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterModel
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_model_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int RequestCount { get; init; }
    public int SnapshotCount { get; init; }
    public int RuntimeRoutedRequestCount { get; init; }
    public int PresentationOnlyRequestCount { get; init; }
    public int PresentationOnlyRuntimeExecutionCount { get; init; }
    public bool RequestResponseCorrelationPassed { get; init; }
    public bool SequentialCursorContinuityPassed { get; init; }
    public bool StateHashContinuityPassed { get; init; }
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SourceGoal142Handoff { get; init; } = true;
}

public sealed record SelectedRuntimeVariantPlayerAdapterResult
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_result_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string CandidateId { get; init; } = string.Empty;
    public bool SelectedPackageSha256MatchesHandoff { get; init; }
    public bool SelectedFinalStateHashMatches { get; init; }
    public bool CorrectedRoundtripSemanticsPassed { get; init; }
    public int FrameCount { get; init; }
    public int RequestCount { get; init; }
    public int SnapshotCount { get; init; }
    public int RuntimeRoutedRequestCount { get; init; }
    public int PresentationOnlyRequestCount { get; init; }
    public int PresentationOnlyRuntimeExecutionCount { get; init; }
    public bool RequestResponseCorrelationPassed { get; init; }
    public bool SequentialCursorContinuityPassed { get; init; }
    public bool StateHashContinuityPassed { get; init; }
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool CorePassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SelectedRuntimeVariantPlayerAdapterHandoff
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_handoff_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public string SourceSelectedHandoffPath { get; init; } = string.Empty;
    public string SourcePackagePath { get; init; } = string.Empty;
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string SourceRoundtripResultPath { get; init; } = string.Empty;
    public string SourceOutcomePath { get; init; } = string.Empty;
    public string PlayerAdapterModelPath { get; init; } = string.Empty;
    public string PlayerAdapterFramesPath { get; init; } = string.Empty;
    public string PlayerAdapterResultPath { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public bool SelectedPackageSha256MatchesHandoff { get; init; }
    public bool SelectedFinalStateHashMatches { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterDashboard
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_dashboard_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedVariantKind { get; init; } = string.Empty;
    public int SelectedScore { get; init; }
    public bool PackageHashMatch { get; init; }
    public bool FinalStateHashMatch { get; init; }
    public int FrameCount { get; init; }
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public string NormalCommand { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.NormalCommand;
    public string HandoffPath { get; init; } =
        SelectedRuntimeVariantPlayerAdapterVocabulary.HandoffRelativePath;
    public bool Accepted { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterNegativeProof
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_negative_proof_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public bool NoBalancedBaselineFallback { get; init; }
    public bool NoGoal131SelectedCandidateFallback { get; init; }
    public bool NoSampleTemplateFallback { get; init; }
    public bool SelectedCandidateMatchesGoal142Handoff { get; init; }
    public bool SelectedPackageHashMismatchRejected { get; init; }
    public bool SelectedFinalStateHashMismatchRejected { get; init; }
    public bool PresentationOnlyControlsStillDoNotExecuteRuntime { get; init; }
    public bool UnityDoesNotExecuteGameplay { get; init; }
    public bool WinFormsStartsNoCompilerOrTestProcess { get; init; }
    public bool PreviousArtifactsPreservedOnFailure { get; init; }
    public bool Passed { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterUnitySmoke
{
    public string SchemaVersion { get; init; } =
        "unity_selected_runtime_variant_playeradapter_smoke_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string Status { get; init; } = "PENDING";
    public bool UnityAvailable { get; init; }
    public bool ModelPathExists { get; init; }
    public bool FramesPathExists { get; init; }
    public bool CandidateIsGoal142Selection { get; init; }
    public bool SelectedPackageSha256MatchesHandoff { get; init; }
    public bool SelectedFinalStateHashMatches { get; init; }
    public bool FrameCountPassed { get; init; }
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool UnityConsumesSelectedVariantPlayerAdapter { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool Passed { get; init; }
    public int UnityExitCode { get; init; } = -1;
    public string ModelPath { get; init; } = string.Empty;
    public string FramesPath { get; init; } = string.Empty;
    public string ModelSha256 { get; init; } = string.Empty;
    public string FramesSha256 { get; init; } = string.Empty;
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SelectedRuntimeVariantPlayerAdapterFileIndex
{
    public string SchemaVersion { get; init; } =
        "selected_runtime_variant_playeradapter_file_index_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<SelectedRuntimeVariantPlayerAdapterFileIndexEntry> Files { get; init; } = [];
}

public sealed record SelectedRuntimeVariantPlayerAdapterFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record SelectedRuntimeVariantPlayerAdapterArtifactSet
{
    public Goal142HumanAcceptanceRecord Acceptance { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterHandoff Handoff { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterModel Model { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterFrames Frames { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterResult Result { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterDashboard Dashboard { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterNegativeProof NegativeProof { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterUnitySmoke UnitySmoke { get; init; } = new();
    public RuntimeBackedPlayerCommandRoundtripResult RuntimeRoundtrip { get; init; } = new();
}

public sealed record SelectedRuntimeVariantPlayerAdapterWriteResult
{
    public SelectedRuntimeVariantPlayerAdapterDashboard Dashboard { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterHandoff Handoff { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterModel Model { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterFrames Frames { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterResult Result { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterNegativeProof NegativeProof { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterUnitySmoke UnitySmoke { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
