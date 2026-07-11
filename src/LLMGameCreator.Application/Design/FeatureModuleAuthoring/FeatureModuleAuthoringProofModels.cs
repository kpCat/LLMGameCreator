using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public static class FeatureModuleAuthoringVocabulary
{
    public const string ScenarioId = "goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification";
    public const string GoalId = "goal_147_persistent_featuremodule_registry_typed_parameter_authoring_saved_compositions_and_incremental_certification";
    public const string ProceduralRoot = ".llmgc/procedural/" + ScenarioId;
    public const string ExportRoot = ".llmgc/exports/" + ScenarioId;
    public const string DefaultCompositionId = "goal147-custom-alchemy-combat-exploration";
    public const string NormalCommand = ".devflow\\scripts\\run-featuremodule-authoring-persistence-and-certification.cmd";
}

public sealed record FeatureModuleAuthoringRunRequest
{
    public string CatalogRoot { get; init; } = FeatureModuleLibraryVocabulary.DefaultRelativeRoot;
    public string WorkspaceRoot { get; init; } = string.Empty;
    public string CertificationCacheRoot { get; init; } = string.Empty;
    public string CompositionId { get; init; } = FeatureModuleAuthoringVocabulary.DefaultCompositionId;
    public string OutputRoot { get; init; } = FeatureModuleAuthoringVocabulary.ProceduralRoot;
    public string UnitySmokePath { get; init; } = FeatureModuleAuthoringVocabulary.ProceduralRoot
                                                 + "/unity-saved-featuremodule-composition-smoke.json";
}

public sealed record FeatureModuleAuthoringUnitySmoke
{
    public string Status { get; init; } = "PENDING";
    public bool SavedCompositionLoaded { get; init; }
    public bool CatalogFingerprintMatches { get; init; }
    public bool SelectedModuleFingerprintsMatch { get; init; }
    public bool ParameterValuesLoaded { get; init; }
    public bool PackageShaMatches { get; init; }
    public bool RuntimeQualificationPassed { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public int UnityExitCode { get; init; } = -1;
    public bool Passed { get; init; }
}

public sealed record FeatureModuleAuthoringDashboard
{
    public string Status { get; init; } = "BLOCKED";
    public bool PersistentFeatureModuleLibrary { get; init; }
    public bool ModuleLibraryFileBased { get; init; }
    public bool ModuleLibrarySourceOfTruth { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public int RequiredCoreModuleCount { get; init; }
    public int OptionalModuleCount { get; init; }
    public bool ModuleFingerprintingPassed { get; init; }
    public bool CatalogFingerprintingPassed { get; init; }
    public bool AddingModuleFileRequiresNoComposerCodeChange { get; init; }
    public bool TypedParameterAuthoring { get; init; }
    public int ParameterDefinitionCount { get; init; }
    public bool GenericParameterBinding { get; init; }
    public bool AtomicParameterGroupsPassed { get; init; }
    public bool DefaultParametersPreserveGoal146Hashes { get; init; }
    public bool SavedCompositionPersistence { get; init; }
    public bool SavedCompositionRoundtripPassed { get; init; }
    public bool SavedCompositionAtomicWritePassed { get; init; }
    public bool SavedCompositionStalenessDetectionPassed { get; init; }
    public bool IncrementalModuleCertification { get; init; }
    public bool AllOptionalModulesCertified { get; init; }
    public bool UnchangedCertificationCacheReusePassed { get; init; }
    public bool ChangedModuleSelectiveInvalidationPassed { get; init; }
    public bool InteractionCoverageIndependentFromSingletonCertification { get; init; }
    public bool HundredModuleCatalogAccepted { get; init; }
    public int HundredModuleInteractionRowCount { get; init; }
    public bool HundredModulePowersetEnumerated { get; init; }
    public bool SelectedCompositionAlwaysIncluded { get; init; }
    public bool SmallCatalogCompatibleExhaustiveCoveragePassed { get; init; }
    public bool SmallCatalogInvalidCombinationsClassified { get; init; }
    public bool MultiEffectModuleAccountingPassed { get; init; }
    public bool CustomParameterizedCompositionPassed { get; init; }
    public bool CustomPackageDistinctFromDefault { get; init; }
    public bool CustomRuntimeQualificationPassed { get; init; }
    public bool OperatorUsesInProcessService { get; init; } = true;
    public bool UnitySmokePassed { get; init; }
    public bool Goal146Accepted { get; init; }
    public bool Goal147Accepted { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Accepted { get; init; }
}

public sealed record FeatureModuleAuthoringProofResult
{
    public FeatureModuleLibrarySnapshot Library { get; init; } = new();
    public object ParameterSchema { get; init; } = new();
    public object DefaultHashCompatibilityProof { get; init; } = new();
    public object SavedCompositionRoundtripProof { get; init; } = new();
    public object ParameterizedCompositionMaterializationProof { get; init; } = new();
    public FeatureModuleCertificationLedger CertificationLedger { get; init; } = new();
    public object CertificationCacheProof { get; init; } = new();
    public FeatureModuleCompositionCoveragePlan BoundedInteractionCoverageProof { get; init; } = new();
    public object HundredModuleScalabilityProof { get; init; } = new();
    public object MultiEffectModuleProof { get; init; } = new();
    public IReadOnlyDictionary<string, bool> NegativeProof { get; init; } = new Dictionary<string, bool>();
    public FeatureModuleCompositionDocument SelectedComposition { get; init; } = new();
    public FeatureModuleParameterizedCompositionResult SelectedMaterialization { get; init; } = new();
    public FeatureModuleAuthoringDashboard Dashboard { get; init; } = new();
    public FeatureModuleAuthoringUnitySmoke UnitySmoke { get; init; } = new();
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
