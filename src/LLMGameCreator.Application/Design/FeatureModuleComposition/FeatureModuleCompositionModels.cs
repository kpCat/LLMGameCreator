using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleCompositionVocabulary
{
    public const string ScenarioId = "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix";
    public const string GoalId = "goal_146_featuremodule_composition_workbench_and_novel_gamepackage_runtime_qualification_matrix";
    public const string Goal142Root = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string ProceduralRoot = ".llmgc/procedural/" + ScenarioId;
    public const string ExportRoot = ".llmgc/exports/" + ScenarioId;
    public const string BaselineCandidateId = "minimal-map-game-balanced-baseline";
    public const string DefaultCompositionId = "minimal-map-game-composed-alchemy-combat-exploration";
    public const string NormalCommand = ".devflow\\scripts\\run-featuremodule-composition-runtime-matrix.cmd";

}

public static class FeatureModuleCompositionBasePackageSourceKinds
{
    public const string Goal142BalancedBaseline = "goal142_balanced_baseline";
    public const string SeededGeneratedBase = "seeded_generated_base";
}

public sealed record FeatureModuleCompositionBasePackage
{
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string SourceKind { get; init; } = FeatureModuleCompositionBasePackageSourceKinds.Goal142BalancedBaseline;
    public string SourceIdentity { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
}

public sealed record FeatureModuleSourceLineage
{
    public string GoalId { get; init; } = string.Empty;
    public string CatalogPath { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public IReadOnlyList<string> OperationIds { get; init; } = [];
}

public sealed record FeatureModuleDefinition
{
    public string SchemaVersion { get; init; } = "featuremodule_definition_v1";
    public string ModuleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string ModuleKind { get; init; } = string.Empty;
    public bool Required { get; init; }
    public bool Selectable { get; init; }
    public bool DefaultSelected { get; init; }
    public string ModuleVersion { get; init; } = "1.0.0";
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public IReadOnlyList<string> Conflicts { get; init; } = [];
    public IReadOnlyList<string> RequiredSchemaSections { get; init; } = [];
    public IReadOnlyList<string> RequiredRuntimePrimitives { get; init; } = [];
    public IReadOnlyList<string> RequiredValidationRules { get; init; } = [];
    public IReadOnlyList<string> RequiredSaveLoadPolicy { get; init; } = [];
    public IReadOnlyList<string> RequiredPlayerAdapterSurface { get; init; } = [];
    public IReadOnlyList<string> GeneratorInputs { get; init; } = [];
    public IReadOnlyList<string> AuthoringControls { get; init; } = [];
    public IReadOnlyList<string> GoldenPackages { get; init; } = [];
    public IReadOnlyList<string> SmokePlaythroughs { get; init; } = [];
    public IReadOnlyList<string> KnownLimitations { get; init; } = [];
    public IReadOnlyList<string> FutureExpansionNotes { get; init; } = [];
    public IReadOnlyList<ProductLineRuntimeVariantMutationOperation> MutationOperations { get; init; } = [];
    public IReadOnlyList<FeatureModuleRuntimeEffectContract> RuntimeEffectContracts { get; init; } = [];
    public IReadOnlyList<FeatureModuleParameterDefinition> ParameterDefinitions { get; init; } = [];
    public IReadOnlyList<FeatureModuleParameterConstraint> ParameterConstraints { get; init; } = [];
    public IReadOnlyList<FeatureModuleEffectiveValueBinding> EffectiveValueBindings { get; init; } = [];
    public IReadOnlyList<FeatureModuleRuntimePlaythroughContract> RuntimePlaythroughContracts { get; init; } = [];
    public FeatureModuleSourceLineage SourceLineage { get; init; } = new();
}

public sealed record FeatureModuleCatalogDocument
{
    public string SchemaVersion { get; init; } = "featuremodule_catalog_v1";
    public string GoalId { get; init; } = FeatureModuleCompositionVocabulary.GoalId;
    public string ImmutableBaseCandidateId { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
    public int RequiredCoreModuleCount { get; init; }
    public int OptionalProfileModuleCount { get; init; }
    public IReadOnlyList<FeatureModuleDefinition> Modules { get; init; } = [];
}

public sealed record FeatureModuleCompositionRequest
{
    public string CompositionId { get; init; } = FeatureModuleCompositionVocabulary.DefaultCompositionId;
    public string DisplayName { get; init; } = "Alchemy + Combat + Exploration Composition";
    public string BaseCandidateId { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
    public IReadOnlyList<string> SelectedModuleIds { get; init; } = [];
    public IReadOnlyDictionary<string, string> ParameterOverrides { get; init; } = new Dictionary<string, string>();
    public string SelectionMode { get; init; } = "human_operator";
}

public sealed record FeatureModuleCompositionRunRequest
{
    public string Goal142Root { get; init; } = FeatureModuleCompositionVocabulary.Goal142Root;
    public string OutputRoot { get; init; } = FeatureModuleCompositionVocabulary.ProceduralRoot;
    public IReadOnlyList<string>? SelectedModuleIds { get; init; }
    public string CompositionId { get; init; } = string.Empty;
    public string UnitySmokePath { get; init; } = FeatureModuleCompositionVocabulary.ProceduralRoot + "/unity-featuremodule-composition-matrix-smoke.json";
}

public sealed record FeatureModuleCompositionValidation
{
    public bool AllModuleIdsExist { get; init; }
    public bool RequiredModulesSelected { get; init; }
    public bool DependenciesSatisfied { get; init; }
    public bool ConflictsAbsent { get; init; }
    public bool ModuleIdsUnique { get; init; }
    public bool OperationIdsUnique { get; init; }
    public bool MutationTargetsUniqueOrIdentical { get; init; }
    public bool ParameterOverridesSupported { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCompositionPlan
{
    public string SchemaVersion { get; init; } = "featuremodule_composition_plan_v1";
    public string CompositionId { get; init; } = string.Empty;
    public string BaseCandidateId { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
    public string BasePackagePath { get; init; } = string.Empty;
    public string BasePackageSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredModuleIds { get; init; } = [];
    public IReadOnlyList<string> SelectedOptionalModuleIds { get; init; } = [];
    public IReadOnlyList<string> OrderedModuleIds { get; init; } = [];
    public IReadOnlyList<ProductLineRuntimeVariantMutationOperation> OrderedMutationOperations { get; init; } = [];
    public int DeduplicatedOperationCount { get; init; }
    public int ConflictCount { get; init; }
    public bool DependencyValidationPassed { get; init; }
    public bool OrderIndependencePassed { get; init; }
    public bool SourceTemplateUnmodified { get; init; }
    public FeatureModuleCompositionValidation Validation { get; init; } = new();
}

public sealed record FeatureModulePackageValidation
{
    public bool CandidateFileExists { get; init; }
    public bool ValidJson { get; init; }
    public bool ExistingPackageValidatorPassed { get; init; }
    public bool RequiredAnchorsPresent { get; init; }
    public bool CompositionMetadataMatches { get; init; }
    public bool PackageUnderGoal146Root { get; init; }
    public bool SourceTemplateUnmodified { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleOrderIndependenceProof
{
    public string CompositionId { get; init; } = string.Empty;
    public IReadOnlyList<string> ForwardModuleIds { get; init; } = [];
    public IReadOnlyList<string> ReverseModuleIds { get; init; } = [];
    public string ForwardPackageSha256 { get; init; } = string.Empty;
    public string ReversePackageSha256 { get; init; } = string.Empty;
    public bool PackageBytesIdentical { get; init; }
    public bool Passed { get; init; }
}

public sealed record FeatureModuleSemanticEffectProof
{
    public string CompositionId { get; init; } = string.Empty;
    public bool AlchemyEffectObserved { get; init; }
    public bool CombatEffectObserved { get; init; }
    public bool ExplorationResourceEffectObserved { get; init; }
    public int CombinedEffectCount { get; init; }
    public int EffectObservationCount { get; init; }
    public int PassedEffectObservationCount { get; init; }
    public int SelectedModuleCount { get; init; }
    public int SatisfiedSelectedModuleCount { get; init; }
    public int HealingPotionQuantity { get; init; }
    public int AppleQuantity { get; init; }
    public int LogQuantity { get; init; }
    public int GoblinHealthAfterAttack { get; init; }
    public int RetainedRedHerbQuantity { get; init; }
    public int RetainedWaterFlaskQuantity { get; init; }
    public string QuestState { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public IReadOnlyList<FeatureModuleRuntimeEffectObservation> Observations { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record FeatureModuleCompositionResult
{
    public string CompositionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedOptionalModuleIds { get; init; } = [];
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string CheckpointHash { get; init; } = string.Empty;
    public bool PackageValidationPassed { get; init; }
    public bool MutationAuditPassed { get; init; }
    public bool DependencyValidationPassed { get; init; }
    public bool ConflictValidationPassed { get; init; }
    public bool OrderIndependencePassed { get; init; }
    public bool InvalidActionStateUnchanged { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingsPassed { get; init; }
    public int CheckpointReplayedActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public bool PackageDistinctFromGoal142Candidates { get; init; }
    public FeatureModuleSemanticEffectProof SemanticEffects { get; init; } = new();
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCompositionMatrixResult
{
    public string SchemaVersion { get; init; } = "featuremodule_composition_matrix_result_v1";
    public string Status { get; init; } = "BLOCKED";
    public int CompositionCount { get; init; }
    public int PassedCompositionCount { get; init; }
    public int FailedCompositionCount { get; init; }
    public int BaselineOnlyCompositionCount { get; init; }
    public int SingleOptionalModuleCompositionCount { get; init; }
    public int MultiModuleCompositionCount { get; init; }
    public int DistinctPackageSha256Count { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool AllPackageValidationsPassed { get; init; }
    public bool AllMutationAuditsPassed { get; init; }
    public bool AllDependencyValidationsPassed { get; init; }
    public bool AllConflictValidationsPassed { get; init; }
    public bool AllOrderIndependenceProofsPassed { get; init; }
    public bool AllCheckpointReloadsPassed { get; init; }
    public bool AllFullReplaysEquivalent { get; init; }
    public bool AllActionBindingsPassed { get; init; }
    public bool SameMutationEngineUsedForAllCompositions { get; init; }
    public bool SameRuntimeQualifierUsedForGoal145AndGoal146 { get; init; }
    public bool SameCanonicalActionPlanUsedForAllCompositions { get; init; }
    public bool MultiModulePackagesDistinctFromAllGoal142Candidates { get; init; }
    public FeatureModuleCompositionCoveragePlan CoveragePlan { get; init; } = new();
    public IReadOnlyList<FeatureModuleCompositionResult> Compositions { get; init; } = [];
}

public sealed record FeatureModuleCompositionComparison
{
    public string BaselineCompositionId { get; init; } = string.Empty;
    public string DefaultCompositionId { get; init; } = FeatureModuleCompositionVocabulary.DefaultCompositionId;
    public bool AllFreshDimensionsObserved { get; init; }
    public IReadOnlyList<FeatureModuleSemanticEffectProof> SemanticEffects { get; init; } = [];
}

public sealed record FeatureModuleCompositionSelectionHandoff
{
    public string SelectionId { get; init; } = "goal146-human-operator-feature-modules";
    public string SelectionMode { get; init; } = "human_operator_feature_modules";
    public string CompositionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string BaseCandidateId { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
    public IReadOnlyList<string> RequiredModuleIds { get; init; } = [];
    public IReadOnlyList<string> SelectedOptionalModuleIds { get; init; } = [];
    public IReadOnlyList<string> OrderedModuleIds { get; init; } = [];
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public bool PackageDistinctFromGoal142Candidates { get; init; }
    public string RuntimeQualificationResultPath { get; init; } = string.Empty;
    public string CheckpointHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SemanticEffects { get; init; } = [];
    public IReadOnlyList<string> AvailableOptionalModuleIds { get; init; } = [];
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
}

public sealed record FeatureModuleCompositionNegativeProof
{
    public bool UnknownModuleRejected { get; init; }
    public bool RequiredModuleDeselectionRejected { get; init; }
    public bool MissingDependencyRejected { get; init; }
    public bool DeclaredConflictRejected { get; init; }
    public bool DuplicateModuleRejected { get; init; }
    public bool ConflictingMutationTargetRejected { get; init; }
    public bool MismatchedExpectedOldValueRejected { get; init; }
    public bool UnsupportedParameterOverrideRejected { get; init; }
    public bool BasePackageHashMismatchRejected { get; init; }
    public bool CompositionPathEscapeRejected { get; init; }
    public bool ModuleOrderChangesPackageBytes { get; init; }
    public bool Goal142PackageCopyCannotCountAsComposition { get; init; }
    public bool SingleGoal142CandidateAliasCannotCountAsNovelComposition { get; init; }
    public bool Goal131ProjectionRecipeCannotBecomeSourceOfTruth { get; init; }
    public bool PrecomputedGoal145OutcomeCannotCountAsGoal146Execution { get; init; }
    public bool CandidateSpecificRuntimeImplementationAbsent { get; init; }
    public bool DuplicateRuntimeActionPlanAbsent { get; init; }
    public bool UnityDoesNotMaterializeOrExecuteGameplay { get; init; }
    public bool WinFormsStartsNoCompilerOrTestProcess { get; init; }
    public bool PreviousArtifactsPreservedOnFailure { get; init; }
    public bool Passed { get; init; }
}

public sealed record FeatureModuleCompositionUnitySmoke
{
    public string Status { get; init; } = "PENDING";
    public int CompositionCount { get; init; }
    public int PassedCompositionCount { get; init; }
    public int DistinctPackageSha256Count { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public int MultiModuleCompositionCount { get; init; }
    public bool SelectedCompositionExists { get; init; }
    public int SelectedCompositionModuleCount { get; init; }
    public bool SelectedPackageDistinctFromGoal142Candidates { get; init; }
    public int SelectedCombinedEffectCount { get; init; }
    public bool AllOrderIndependenceProofsPassed { get; init; }
    public bool AllCheckpointReloadsPassed { get; init; }
    public bool AllFullReplaysEquivalent { get; init; }
    public bool AllActionBindingsPassed { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public int UnityExitCode { get; init; } = -1;
    public bool Passed { get; init; }
}

public sealed record FeatureModuleCompositionDashboard
{
    public string Status { get; init; } = "BLOCKED";
    public bool FeatureModuleComposition { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public int RequiredCoreModuleCount { get; init; }
    public int OptionalProfileModuleCount { get; init; }
    public int CompositionCount { get; init; }
    public int PassedCompositionCount { get; init; }
    public int FailedCompositionCount { get; init; }
    public int MultiModuleCompositionCount { get; init; }
    public int DistinctPackageSha256Count { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool AllPackageValidationsPassed { get; init; }
    public bool AllMutationAuditsPassed { get; init; }
    public bool AllDependencyValidationsPassed { get; init; }
    public bool AllConflictValidationsPassed { get; init; }
    public bool AllOrderIndependenceProofsPassed { get; init; }
    public bool AllCheckpointReloadsPassed { get; init; }
    public bool AllFullReplaysEquivalent { get; init; }
    public bool AllActionBindingsPassed { get; init; }
    public bool SameMutationEngineUsedForAllCompositions { get; init; }
    public bool SameRuntimeQualifierUsedForGoal145AndGoal146 { get; init; }
    public bool SameCanonicalActionPlanUsedForAllCompositions { get; init; }
    public bool MultiModulePackagesDistinctFromAllGoal142Candidates { get; init; }
    public string SelectedCompositionId { get; init; } = string.Empty;
    public int SelectedCompositionModuleCount { get; init; }
    public bool SelectedPackageDistinctFromGoal142Candidates { get; init; }
    public int SelectedCombinedEffectCount { get; init; }
    public bool OperatorUsesInProcessService { get; init; } = true;
    public bool OperatorStartsCompilerProcess { get; init; }
    public bool OperatorStartsDotnetTestProcess { get; init; }
    public bool OperatorStartsPowerShellProcess { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool Goal145Accepted { get; init; } = true;
    public bool Goal146Accepted { get; init; }
    public bool ManualReviewDeferred { get; init; } = true;
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
}

public sealed record Goal145HumanAcceptanceRecord
{
    public string SchemaVersion { get; init; } = "goal145_human_acceptance_record_v1";
    public string GoalId { get; init; } = "goal_145_operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix";
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
    public string Decision { get; init; } = "Я принимаю Goal145 operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix_verification GREEN. candidateCount=4, passedCandidateCount=4, distinctFinalStateHashCount=4, defaultSelection=minimal-map-game-exploration-resource-focus, combatSelectionStable=true, combatPackageSha256=4528af180259dd0d3dd11c97de4048ed4ee43ea2c77209cf5b311061ea702497, programmaticBindInvokesSelectionCount=0, programmaticRestoreInvokesSelectionCount=0, operatorCommitInvokesSelectionCount=1, maximumSelectionCallbackDepth=1, allCandidateCheckpointReloadsPassed=true, allCandidateFullReplaysEquivalent=true, allCandidateActionBindingsPassed=true, allFocusEffectsObserved=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.";
}

public sealed record FeatureModuleCompositionArtifacts
{
    public string PackageJson { get; init; } = string.Empty;
    public FeatureModuleCompositionPlan Plan { get; init; } = new();
    public ProductLineRuntimeVariantMutationAudit MutationAudit { get; init; } = new();
    public FeatureModulePackageValidation PackageValidation { get; init; } = new();
    public RuntimeInteractiveSession Session { get; init; } = new();
    public IReadOnlyList<SelectedRuntimeVariantActionDescriptor> ActionCatalog { get; init; } = [];
    public IReadOnlyList<SelectedRuntimeVariantInteractiveJournalEntry> Journal { get; init; } = [];
    public SelectedRuntimeVariantInteractiveCheckpoint Checkpoint { get; init; } = new();
    public ProductLineRuntimeQualificationReplayEvidence CheckpointReplay { get; init; } = new();
    public ProductLineRuntimeQualificationReplayEvidence FinalReplay { get; init; } = new();
    public FeatureModuleSemanticEffectProof SemanticEffects { get; init; } = new();
    public FeatureModuleOrderIndependenceProof OrderIndependence { get; init; } = new();
}

public sealed record FeatureModuleCompositionQualification
{
    public FeatureModuleCompositionResult Result { get; init; } = new();
    public FeatureModuleCompositionArtifacts Artifacts { get; init; } = new();
}

public sealed record FeatureModuleCompositionWriteResult
{
    public FeatureModuleCatalogDocument Catalog { get; init; } = new();
    public FeatureModuleCompositionRequest Request { get; init; } = new();
    public FeatureModuleCompositionPlan SelectedPlan { get; init; } = new();
    public FeatureModuleCompositionMatrixResult Matrix { get; init; } = new();
    public FeatureModuleCompositionComparison Comparison { get; init; } = new();
    public FeatureModuleCompositionSelectionHandoff Selection { get; init; } = new();
    public FeatureModuleCompositionNegativeProof NegativeProof { get; init; } = new();
    public FeatureModuleCompositionDashboard Dashboard { get; init; } = new();
    public FeatureModuleCompositionUnitySmoke UnitySmoke { get; init; } = new();
    public IReadOnlyDictionary<string, FeatureModuleCompositionArtifacts> CompositionArtifacts { get; init; } = new Dictionary<string, FeatureModuleCompositionArtifacts>();
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
