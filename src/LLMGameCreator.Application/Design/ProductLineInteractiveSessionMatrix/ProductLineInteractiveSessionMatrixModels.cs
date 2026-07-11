using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public static class ProductLineInteractiveSessionMatrixVocabulary
{
    public const string GoalId = "goal_145_operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix";
    public const string Goal142Root = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string ProceduralRoot = ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix";
    public const string ExportRoot = ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix";
    public const string NormalCommand = ".devflow\\scripts\\run-product-line-interactive-session-matrix.cmd";
}

public sealed record ProductLineInteractiveSessionMatrixRequest
{
    public string Goal142Root { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.Goal142Root;
    public string OutputRoot { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot;
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string UnitySmokePath { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot + "/unity-product-line-interactive-session-matrix-smoke.json";
}

public sealed record ProductLineInteractiveSessionCandidate
{
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public bool RuntimeEvaluated { get; init; }
    public bool RuntimeMutated { get; init; }
    public bool ControlCandidate { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionCandidateCatalog
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_candidate_catalog_v1";
    public string GoalId { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.GoalId;
    public int CandidateCount { get; init; }
    public string DefaultSelectedCandidateId { get; init; } = string.Empty;
    public IReadOnlyList<ProductLineInteractiveSessionCandidate> Candidates { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionReplaySummary
{
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

public sealed record ProductLineInteractiveSessionState
{
    public string CandidateId { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public int CurrentActionIndex { get; init; }
    public int RuntimeCommandExecutionCount { get; init; }
    public int PresentationOnlyActionCount { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public bool Completed { get; init; }
}

public sealed record ProductLineInteractiveSessionActionCatalog
{
    public string CandidateId { get; init; } = string.Empty;
    public int ActionDescriptorCount { get; init; }
    public int RuntimeRoutedActionDescriptorCount { get; init; }
    public int PresentationOnlyActionDescriptorCount { get; init; }
    public IReadOnlyList<SelectedRuntimeVariantActionDescriptor> Actions { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionJournal
{
    public string CandidateId { get; init; } = string.Empty;
    public int ActionCount { get; init; }
    public IReadOnlyList<SelectedRuntimeVariantInteractiveJournalEntry> Actions { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionFocusEffectProof
{
    public string CandidateId { get; init; } = string.Empty;
    public string FocusKind { get; init; } = string.Empty;
    public string ComparedDimension { get; init; } = string.Empty;
    public string BaselineValue { get; init; } = string.Empty;
    public string CandidateValue { get; init; } = string.Empty;
    public bool FocusEffectObserved { get; init; }
}

public sealed record ProductLineInteractiveSessionCandidateResult
{
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public bool RuntimeEvaluated { get; init; }
    public bool RuntimeMutated { get; init; }
    public bool ControlCandidate { get; init; }
    public int ActionDescriptorCount { get; init; }
    public int RuntimeRoutedActionDescriptorCount { get; init; }
    public int PresentationOnlyActionDescriptorCount { get; init; }
    public int ExecutedRuntimeActionCount { get; init; }
    public bool InvalidActionStateUnchanged { get; init; }
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool CheckpointStateHashRestored { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public string FocusKind { get; init; } = string.Empty;
    public bool FocusEffectObserved { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionComparison
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_comparison_v1";
    public string BaselineCandidateId { get; init; } = string.Empty;
    public string BaselineFinalStateHash { get; init; } = string.Empty;
    public bool AllFocusEffectsObserved { get; init; }
    public IReadOnlyList<ProductLineInteractiveSessionFocusEffectProof> Comparisons { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionMatrixResult
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_matrix_result_v1";
    public string GoalId { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RuntimeEvaluatedCandidateCount { get; init; }
    public int RuntimeMutatedCandidateCount { get; init; }
    public int ControlCandidateCount { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool AllCandidatePackageHashesDistinct { get; init; }
    public bool AllCandidateCheckpointReloadsPassed { get; init; }
    public bool AllCandidateFullReplaysEquivalent { get; init; }
    public bool AllCandidateActionBindingsPassed { get; init; }
    public bool SameRuntimeServiceUsedForAllCandidates { get; init; }
    public bool SameCanonicalActionPlanUsedForAllCandidates { get; init; }
    public bool AllFocusEffectsObserved { get; init; }
    public IReadOnlyList<ProductLineInteractiveSessionCandidateResult> Candidates { get; init; } = [];
}

public sealed record ProductLineInteractiveSessionSelectionHandoff
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_selection_handoff_v1";
    public string SelectionId { get; init; } = "goal145-active-runtime-session-selection";
    public string SelectionMode { get; init; } = "human_operator";
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedRecipeId { get; init; } = string.Empty;
    public string SelectedVariantKind { get; init; } = string.Empty;
    public int SelectedScore { get; init; }
    public string SelectedPackagePath { get; init; } = string.Empty;
    public string SelectedPackageSha256 { get; init; } = string.Empty;
    public string SelectedFinalStateHash { get; init; } = string.Empty;
    public string SelectedCheckpointHash { get; init; } = string.Empty;
    public ProductLineInteractiveSessionFocusEffectProof SelectedComparisonToBaseline { get; init; } = new();
    public IReadOnlyList<string> AvailableCandidateIds { get; init; } = [];
    public string CandidateMatrixResultPath { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot + "/product-line-interactive-session-matrix-result.json";
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
}

public sealed record ProductLineInteractiveSessionNegativeProof
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_negative_proof_v1";
    public bool UnknownCandidateRejected { get; init; }
    public bool FailedCandidateSelectionRejected { get; init; }
    public bool CandidatePackageHashMismatchRejected { get; init; }
    public bool CandidateMetadataMismatchRejected { get; init; }
    public bool CandidatePathEscapeRejected { get; init; }
    public bool DuplicateCandidateIdRejected { get; init; }
    public bool DuplicatePackagePathRejected { get; init; }
    public bool CrossCandidateCheckpointRejected { get; init; }
    public bool BaselineFallbackRejected { get; init; }
    public bool Goal131FallbackRejected { get; init; }
    public bool SampleTemplateFallbackRejected { get; init; }
    public bool HardcodedExplorationOnlySelectionRejected { get; init; }
    public bool PrecomputedGoal142OutcomeCannotCountAsGoal145Execution { get; init; }
    public bool CandidateSpecificRuntimeImplementationAbsent { get; init; }
    public bool UnityDoesNotExecuteGameplay { get; init; }
    public bool WinFormsStartsNoCompilerOrTestProcess { get; init; }
    public bool PreviousArtifactsPreservedOnFailure { get; init; }
    public bool Passed { get; init; }
}

public sealed record ProductLineInteractiveSessionUnitySmoke
{
    public string SchemaVersion { get; init; } = "unity_product_line_interactive_session_matrix_smoke_v1";
    public string Status { get; init; } = "PENDING";
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool SelectedCandidateExists { get; init; }
    public bool SelectedCandidatePackageHashMatches { get; init; }
    public bool AllCandidateCheckpointReloadsPassed { get; init; }
    public bool AllCandidateFullReplaysEquivalent { get; init; }
    public bool AllCandidateActionBindingsPassed { get; init; }
    public bool AllFocusEffectsObserved { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool Passed { get; init; }
    public int UnityExitCode { get; init; } = -1;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record Goal144HumanAcceptanceRecord
{
    public string SchemaVersion { get; init; } = "goal144_human_acceptance_record_v1";
    public string GoalId { get; init; } = "goal_144_selected_runtime_variant_interactive_action_session_and_save_replay";
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
    public string Decision { get; init; } = "Я принимаю Goal144 selected_runtime_variant_interactive_action_session_and_save_replay_verification GREEN. selectedCandidate=minimal-map-game-exploration-resource-focus, actionDescriptorCount=14, runtimeRoutedActionDescriptorCount=11, presentationOnlyActionDescriptorCount=3, executedRuntimeActionCount=11, actionDescriptorExecutionBindingPassed=true, harvestTarget=node/apple_tree, basicAttackTarget=goblin, invalidActionStateUnchanged=true, checkpointReloadByReplayPassed=true, checkpointReplayedActionCount=8, finalReplayActionCount=13, replayEvidenceFrozenBeforeContinuation=true, fullReplayEquivalent=true, finalStateHashMatchesGoal142=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.";
}

public sealed record ProductLineInteractiveSessionDashboard
{
    public string SchemaVersion { get; init; } = "product_line_interactive_session_dashboard_v1";
    public string Status { get; init; } = "BLOCKED";
    public bool ProductLineInteractiveSessionMatrix { get; init; }
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RuntimeEvaluatedCandidateCount { get; init; }
    public int RuntimeMutatedCandidateCount { get; init; }
    public int ControlCandidateCount { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool AllCandidatePackageHashesDistinct { get; init; }
    public bool AllCandidateCheckpointReloadsPassed { get; init; }
    public bool AllCandidateFullReplaysEquivalent { get; init; }
    public bool AllCandidateActionBindingsPassed { get; init; }
    public bool SameRuntimeServiceUsedForAllCandidates { get; init; }
    public bool SameCanonicalActionPlanUsedForAllCandidates { get; init; }
    public bool AllFocusEffectsObserved { get; init; }
    public int OperatorSelectableCandidateCount { get; init; }
    public bool ActiveSelectionResolved { get; init; }
    public bool ActiveSelectedCandidateExists { get; init; }
    public string ActiveSelectedCandidateId { get; init; } = string.Empty;
    public bool CrossCandidateCheckpointRejected { get; init; }
    public bool NoHardcodedExplorationOnlyPath { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool NoGoal131Fallback { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool Goal144Accepted { get; init; } = true;
    public bool Goal145Accepted { get; init; }
    public bool Accepted { get; init; }
    public string NormalCommand { get; init; } = ProductLineInteractiveSessionMatrixVocabulary.NormalCommand;
}

public sealed record ProductLineInteractiveSessionArtifactSet
{
    public Goal144HumanAcceptanceRecord Acceptance { get; init; } = new();
    public ProductLineInteractiveSessionCandidateCatalog Catalog { get; init; } = new();
    public ProductLineInteractiveSessionMatrixResult Matrix { get; init; } = new();
    public ProductLineInteractiveSessionComparison Comparison { get; init; } = new();
    public ProductLineInteractiveSessionDashboard Dashboard { get; init; } = new();
    public ProductLineInteractiveSessionNegativeProof NegativeProof { get; init; } = new();
    public ProductLineInteractiveSessionSelectionHandoff Selection { get; init; } = new();
    public ProductLineInteractiveSessionUnitySmoke UnitySmoke { get; init; } = new();
}

public sealed record ProductLineInteractiveSessionCandidateArtifacts
{
    public ProductLineInteractiveSessionState State { get; init; } = new();
    public ProductLineInteractiveSessionActionCatalog Catalog { get; init; } = new();
    public ProductLineInteractiveSessionJournal Journal { get; init; } = new();
    public SelectedRuntimeVariantInteractiveCheckpoint Checkpoint { get; init; } = new();
    public ProductLineInteractiveSessionReplaySummary CheckpointReplay { get; init; } = new();
    public ProductLineInteractiveSessionReplaySummary FinalReplay { get; init; } = new();
    public ProductLineInteractiveSessionFocusEffectProof FocusProof { get; init; } = new();
}

public sealed record ProductLineInteractiveSessionMatrixWriteResult
{
    public ProductLineInteractiveSessionArtifactSet Artifacts { get; init; } = new();
    public IReadOnlyDictionary<string, ProductLineInteractiveSessionCandidateArtifacts> CandidateArtifacts { get; init; } = new Dictionary<string, ProductLineInteractiveSessionCandidateArtifacts>();
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
