using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class SelectedRuntimeVariantInteractiveSessionVocabulary
{
    public const string GoalId =
        "goal_144_selected_runtime_variant_interactive_action_session_and_save_replay";
    public const string CandidateId = "minimal-map-game-exploration-resource-focus";
    public const string VariantKind = "exploration_resource_focus";
    public const string ExpectedPackageSha256 =
        "27b426b087eb6dfd4567facbf76b1463a7ab1a46ff0e834ba849c95aa1858565";
    public const string ExpectedFinalStateHash =
        "d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54";
    public const string SourceGoal142Root =
        ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string SelectedHandoffPath = SourceGoal142Root
        + "/selected-runtime-variant/selected-runtime-variant-handoff.json";
    public const string SelectedPackagePath = SourceGoal142Root
        + "/selected-runtime-variant/package.json";
    public const string SelectedOutcomePath = SourceGoal142Root
        + "/selected-runtime-variant/runtime-outcome-summary.json";
    public const string Goal143HandoffPath =
        ".llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-handoff.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay";
    public const string HotfixGoalId =
        "goal_144a_live_session_action_target_binding_and_replay_evidence_hotfix";
    public const string HotfixProceduralOutputDirectory =
        ".llmgc/procedural/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix";
    public const string HotfixExportPackageDirectory =
        ".llmgc/exports/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix";
    public const string NormalCommand =
        ".devflow\\scripts\\run-selected-runtime-variant-live-session.cmd";
    public const string AcceptanceFileName = "goal143-human-acceptance-record.json";
    public const string CatalogFileName = "selected-runtime-variant-live-session-action-catalog.json";
    public const string StateFileName = "selected-runtime-variant-live-session-state.json";
    public const string JournalFileName = "selected-runtime-variant-live-session-journal.json";
    public const string CheckpointFileName = "selected-runtime-variant-live-session-checkpoint.json";
    public const string ReloadFileName = "selected-runtime-variant-live-session-checkpoint-reload-result.json";
    public const string ReplayFileName = "selected-runtime-variant-live-session-final-replay-result.json";
    public const string DashboardFileName = "selected-runtime-variant-live-session-dashboard.json";
    public const string NegativeProofFileName = "selected-runtime-variant-live-session-negative-proof.json";
    public const string FileIndexFileName = "selected-runtime-variant-live-session-file-index.json";
    public const string UnitySmokeFileName = "unity-selected-runtime-variant-live-session-smoke.json";
    public const string ReportJsonFileName = "one-click-selected-runtime-variant-live-session-report.json";
    public const string ReportMarkdownFileName = "one-click-selected-runtime-variant-live-session-report.md";
    public const string ActionExecutionBindingProofFileName = "action-execution-binding-proof.json";
    public const string ReplayEvidenceFreezeProofFileName = "replay-evidence-freeze-proof.json";
    public const string HotfixDashboardFileName = "goal144a-correctness-dashboard.json";
    public const string HotfixReportFileName = "goal144a-correctness-report.md";
    public const string HotfixFileIndexFileName = "goal144a-file-index.json";
    public const string UnitySmokeRelativePath = ProceduralOutputDirectory + "/" + UnitySmokeFileName;
}

public sealed record SelectedRuntimeVariantInteractiveSessionRequest
{
    public string SelectedHandoffPath { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedHandoffPath;
    public string SelectedPackagePath { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedPackagePath;
    public string SelectedOutcomePath { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedOutcomePath;
    public string Goal143HandoffPath { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.Goal143HandoffPath;
    public string OutputRoot { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.ProceduralOutputDirectory;
    public string UnitySmokePath { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.UnitySmokeRelativePath;
}

public sealed record Goal143HumanAcceptanceRecord
{
    public string SchemaVersion { get; init; } = "goal143_human_acceptance_record_v1";
    public string GoalId { get; init; } =
        "goal_143_selected_runtime_variant_end_to_end_playeradapter_handoff";
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
    public string SelectedCandidate { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.CandidateId;
    public string SelectedVariant { get; init; } =
        SelectedRuntimeVariantInteractiveSessionVocabulary.VariantKind;
    public int SelectedScore { get; init; } = 100;
    public bool PackageHashMatch { get; init; } = true;
    public bool FinalStateHashMatch { get; init; } = true;
    public int RequestCount { get; init; } = 6;
    public int SnapshotCount { get; init; } = 15;
    public int FrameCount { get; init; } = 15;
    public bool SelectedVariantEffectVisible { get; init; } = true;
    public bool NoBalancedBaselineFallback { get; init; } = true;
    public bool OperatorUsesInProcessService { get; init; } = true;
    public string OperatorStatus { get; init; } = "GREEN";
    public string UnitySmoke { get; init; } = "GREEN";
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
}

public sealed record SelectedRuntimeVariantLiveSessionCatalog
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_action_catalog_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string CandidateId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.CandidateId;
    public int ActionDescriptorCount { get; init; }
    public int RuntimeRoutedActionDescriptorCount { get; init; }
    public int PresentationOnlyActionDescriptorCount { get; init; }
    public IReadOnlyList<SelectedRuntimeVariantActionDescriptor> Actions { get; init; } = [];
}

public sealed record SelectedRuntimeVariantLiveSessionState
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_state_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string SessionId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public int CurrentActionIndex { get; init; }
    public int RuntimeCommandExecutionCount { get; init; }
    public int PresentationOnlyActionCount { get; init; }
    public string CurrentStateHash { get; init; } = string.Empty;
    public bool RuntimeStarted { get; init; }
    public bool Completed { get; init; }
    public string MapSummary { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
}

public sealed record SelectedRuntimeVariantLiveSessionJournal
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_journal_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string SessionId { get; init; } = string.Empty;
    public int ActionCount { get; init; }
    public IReadOnlyList<SelectedRuntimeVariantInteractiveJournalEntry> ActionJournal { get; init; } = [];
}

public sealed record SelectedRuntimeVariantLiveSessionReplaySummary
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_replay_result_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string ReplayKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool PackageHashValidated { get; init; }
    public bool CandidateValidated { get; init; }
    public bool JournalCorrelationPassed { get; init; }
    public bool StateHashContinuityPassed { get; init; }
    public bool ExpectedStateHashMatched { get; init; }
    public string ExpectedStateHash { get; init; } = string.Empty;
    public string ActualStateHash { get; init; } = string.Empty;
    public int ReplayedActionCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SelectedRuntimeVariantLiveSessionNegativeProof
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_negative_proof_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public bool InvalidActionRejectedWithoutMutation { get; init; }
    public bool PresentationOnlyActionsDoNotExecuteRuntime { get; init; }
    public bool CheckpointPackageHashMismatchRejected { get; init; }
    public bool CheckpointCandidateMismatchRejected { get; init; }
    public bool CheckpointJournalTamperRejected { get; init; }
    public bool HarvestDescriptorTargetTamperRejected { get; init; }
    public bool BasicAttackDescriptorTargetTamperRejected { get; init; }
    public bool CanonicalStepIdTamperRejected { get; init; }
    public bool RuntimeRangeTamperRejected { get; init; }
    public bool JournalTargetTamperRejected { get; init; }
    public bool CheckpointExpectedHashMismatchRejected { get; init; }
    public bool BalancedBaselineFallbackRejected { get; init; }
    public bool Goal131FallbackRejected { get; init; }
    public bool SampleTemplateFallbackRejected { get; init; }
    public bool UnityDoesNotExecuteGameplay { get; init; }
    public bool WinFormsStartsNoCompilerOrTestProcess { get; init; }
    public bool PreviousArtifactsPreservedOnFailure { get; init; }
    public bool Passed { get; init; }
}

public sealed record SelectedRuntimeVariantLiveSessionUnitySmoke
{
    public string SchemaVersion { get; init; } = "unity_selected_runtime_variant_live_session_smoke_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string Status { get; init; } = "PENDING";
    public bool SessionArtifactsExist { get; init; }
    public bool SelectedCandidateMatches { get; init; }
    public bool PackageHashMatches { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool FinalHashMatchesGoal142 { get; init; }
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoFallback { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public bool HarvestTargetMatches { get; init; }
    public bool BasicAttackTargetMatches { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool ReplayEvidenceFrozenBeforeContinuation { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool Passed { get; init; }
    public int UnityExitCode { get; init; } = -1;
    public string DashboardSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SelectedRuntimeVariantLiveSessionDashboard
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_live_session_dashboard_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool SelectedRuntimeVariantInteractiveSession { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedVariantKind { get; init; } = string.Empty;
    public string SelectedPackageSha256 { get; init; } = string.Empty;
    public bool SelectedPackageSha256Matches { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public int ActionDescriptorCount { get; init; }
    public int RuntimeRoutedActionDescriptorCount { get; init; }
    public int PresentationOnlyActionDescriptorCount { get; init; }
    public int ExecutedRuntimeActionCount { get; init; }
    public int RejectedInvalidActionCount { get; init; }
    public bool InvalidActionStateUnchanged { get; init; }
    public bool CheckpointSavePassed { get; init; }
    public bool CheckpointReloadByReplayPassed { get; init; }
    public bool CheckpointStateHashRestored { get; init; }
    public bool JournalCorrelationPassed { get; init; }
    public bool StateHashContinuityPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool FinalStateHashMatchesGoal142 { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public bool SelectedVariantEffectVisible { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool NoGoal131Fallback { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool Goal144ActionExecutionBindingCorrected { get; init; }
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public bool AllRuntimeActionTargetsMatchExecutedSteps { get; init; }
    public bool AllRuntimeActionCommandKindsMatchExecutedSteps { get; init; }
    public string HarvestActionTargetId { get; init; } = string.Empty;
    public string HarvestExecutedTargetId { get; init; } = string.Empty;
    public string BasicAttackActionTargetId { get; init; } = string.Empty;
    public string BasicAttackExecutedTargetId { get; init; } = string.Empty;
    public bool NoFirstResourceNodeFallback { get; init; }
    public bool NoIndependentCanonicalRangeLookup { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool ReplayEvidenceFrozenBeforeContinuation { get; init; }
    public bool Goal144Accepted { get; init; }
    public bool Accepted { get; init; }
    public string NormalCommand { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.NormalCommand;
}

public sealed record SelectedRuntimeVariantLiveSessionArtifactSet
{
    public Goal143HumanAcceptanceRecord Acceptance { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionCatalog Catalog { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionState State { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionJournal Journal { get; init; } = new();
    public SelectedRuntimeVariantInteractiveCheckpoint Checkpoint { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionReplaySummary CheckpointReload { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionReplaySummary FinalReplay { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionDashboard Dashboard { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionNegativeProof NegativeProof { get; init; } = new();
    public SelectedRuntimeVariantLiveSessionUnitySmoke UnitySmoke { get; init; } = new();
    public Goal144AActionExecutionBindingProof ActionExecutionBindingProof { get; init; } = new();
    public Goal144AReplayEvidenceFreezeProof ReplayEvidenceFreezeProof { get; init; } = new();
    public Goal144ACorrectnessDashboard HotfixDashboard { get; init; } = new();
}

public sealed record Goal144AActionExecutionBindingProof
{
    public string SchemaVersion { get; init; } = "goal144a_action_execution_binding_proof_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.HotfixGoalId;
    public bool Passed { get; init; }
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public bool AllRuntimeActionTargetsMatchExecutedSteps { get; init; }
    public bool AllRuntimeActionCommandKindsMatchExecutedSteps { get; init; }
    public string HarvestActionTargetId { get; init; } = string.Empty;
    public string HarvestExecutedTargetId { get; init; } = string.Empty;
    public string BasicAttackActionTargetId { get; init; } = string.Empty;
    public string BasicAttackExecutedTargetId { get; init; } = string.Empty;
    public bool NoFirstResourceNodeFallback { get; init; }
    public bool NoIndependentCanonicalRangeLookup { get; init; }
    public bool HarvestDescriptorTargetTamperRejected { get; init; }
    public bool BasicAttackDescriptorTargetTamperRejected { get; init; }
    public bool CanonicalStepIdTamperRejected { get; init; }
    public bool RuntimeRangeTamperRejected { get; init; }
    public bool JournalTargetTamperRejected { get; init; }
}

public sealed record Goal144AReplayEvidenceFreezeProof
{
    public string SchemaVersion { get; init; } = "goal144a_replay_evidence_freeze_proof_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.HotfixGoalId;
    public bool Passed { get; init; }
    public int CheckpointJournalActionCount { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool ReplayEvidenceFrozenBeforeContinuation { get; init; }
    public bool CheckpointStateHashRestored { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public string CheckpointExpectedStateHash { get; init; } = string.Empty;
    public string CheckpointActualStateHash { get; init; } = string.Empty;
    public string FinalExpectedStateHash { get; init; } = string.Empty;
    public string FinalActualStateHash { get; init; } = string.Empty;
}

public sealed record Goal144ACorrectnessDashboard
{
    public string SchemaVersion { get; init; } = "goal144a_correctness_dashboard_v1";
    public string GoalId { get; init; } = SelectedRuntimeVariantInteractiveSessionVocabulary.HotfixGoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public bool AllRuntimeActionTargetsMatchExecutedSteps { get; init; }
    public bool AllRuntimeActionCommandKindsMatchExecutedSteps { get; init; }
    public string HarvestActionTargetId { get; init; } = string.Empty;
    public string HarvestExecutedTargetId { get; init; } = string.Empty;
    public string BasicAttackActionTargetId { get; init; } = string.Empty;
    public string BasicAttackExecutedTargetId { get; init; } = string.Empty;
    public bool NoFirstResourceNodeFallback { get; init; }
    public bool NoIndependentCanonicalRangeLookup { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool ReplayEvidenceFrozenBeforeContinuation { get; init; }
    public bool CheckpointStateHashRestored { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool FinalStateHashMatchesGoal142 { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Goal144Accepted { get; init; }
    public bool Accepted { get; init; }
}

public sealed record SelectedRuntimeVariantLiveSessionWriteResult
{
    public SelectedRuntimeVariantLiveSessionArtifactSet Artifacts { get; init; } = new();
    public IReadOnlyList<SelectedRuntimeVariantInteractiveActionResult> ActionResults { get; init; } = [];
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
