using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanCapabilitySelectionRequest
{
    public string AtlasRootPath { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string PresentationModeId { get; init; } = string.Empty;
    public string WorldTopologyId { get; init; } = string.Empty;
    public string ActorModelId { get; init; } = string.Empty;
    public string InventoryModelId { get; init; } = string.Empty;
    public string CombatModelId { get; init; } = string.Empty;
    public string ProgressionModelId { get; init; } = string.Empty;
    public string PathfindingProfileId { get; init; } = string.Empty;
    public string NpcBehaviorModelId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedModifierIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedConstraintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeRequirementIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedRuntimeTargetIds { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanCapabilitySelectionResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanCapabilitySelection Selection { get; init; } = new();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionDiagnostic>();
}

public sealed record GeneratorPlanCapabilitySelection
{
    public string SchemaVersion { get; init; } = "0.1";
    public string SelectionId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public GeneratorPlanCapabilitySelectedVariantIds SelectedVariantIds { get; init; } = new();
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedModifierIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedConstraintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeRequirementIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedRuntimeTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedCapabilityIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedArtifactContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedValidators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedPromptContextTemplates { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedRuntimeTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredLuaModulesOrGaps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public DateTimeOffset GeneratedAtUtc { get; init; }
}

public sealed record GeneratorPlanCapabilitySelectedVariantIds
{
    public string PresentationModeId { get; init; } = string.Empty;
    public string WorldTopologyId { get; init; } = string.Empty;
    public string ActorModelId { get; init; } = string.Empty;
    public string InventoryModelId { get; init; } = string.Empty;
    public string CombatModelId { get; init; } = string.Empty;
    public string ProgressionModelId { get; init; } = string.Empty;
    public string PathfindingProfileId { get; init; } = string.Empty;
    public string NpcBehaviorModelId { get; init; } = string.Empty;
}

public sealed record GeneratorPlanCapabilitySelectionDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record GeneratorPlanCapabilityHelpMetadata
{
    public string Id { get; init; } = string.Empty;
    public string DisplayNameRu { get; init; } = string.Empty;
    public string DisplayNameEn { get; init; } = string.Empty;
    public string ShortDescriptionRu { get; init; } = string.Empty;
    public string DetailsRu { get; init; } = string.Empty;
    public string ExamplesRu { get; init; } = string.Empty;
    public string BestForRu { get; init; } = string.Empty;
    public string WarningsRu { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public string DiagnosticCategoryHint { get; init; } = GeneratorPlanCapabilitySelectionDiagnosticCategories.Info;

    public static GeneratorPlanCapabilityHelpMetadata Fallback(string id)
    {
        return new GeneratorPlanCapabilityHelpMetadata
        {
            Id = id,
            DisplayNameRu = string.IsNullOrWhiteSpace(id) ? "Не выбрано" : id,
            DisplayNameEn = id,
            ShortDescriptionRu = "Подробная справка для этого идентификатора пока не добавлена.",
            DetailsRu = "Машинный идентификатор сохранён для технической точности.",
            ImplementationStatus = "metadata_missing",
            DiagnosticCategoryHint = GeneratorPlanCapabilitySelectionDiagnosticCategories.Info
        };
    }
}

public sealed record GeneratorPlanCapabilityCompositionSeed
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string DisplayNameRu { get; init; } = string.Empty;
    public string ShortDescriptionRu { get; init; } = string.Empty;
}

public sealed record GeneratorPlanCapabilitySelectionArtifactSaveResult
{
    public GeneratedArtifactRecord SelectionArtifact { get; init; } = GeneratorPlanCapabilitySelectionArtifactService.EmptyArtifact;
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanCapabilitySelectionArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? SelectionArtifact { get; init; }
    public GeneratorPlanCapabilitySelection Selection { get; init; } = new();
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanCapabilitySelectionAtlas
{
    public string AtlasRootPath { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> PresentationModes { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> WorldTopologies { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> ActorModels { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> InventoryModels { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> CombatModels { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> ProgressionModels { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> PathfindingProfiles { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> NpcBehaviorModels { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionFeatureBundle> FeatureBundles { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionFeatureBundle>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> RuntimeTargets { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionAtlasOption>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionCapability> Capabilities { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionCapability>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionArtifactContract> ArtifactContracts { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionArtifactContract>();
    public IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanCapabilitySelectionDiagnostic>();
}

public sealed record GeneratorPlanCapabilitySelectionAtlasOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredArtifactContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredValidators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CompatibleWith { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> IncompatibleWith { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AllowedWorldTopologies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RecommendedActorModels { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RecommendedCombatModels { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanCapabilitySelectionFeatureBundle
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public IReadOnlyList<string> Requires { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Provides { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ArtifactContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Validators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PromptContextTemplates { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> FutureModuleGaps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> IncompatibleWith { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RecommendedWith { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanCapabilitySelectionCapability
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> Provides { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DependsOn { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OutputContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Validators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeTargets { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanCapabilitySelectionArtifactContract
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredValidators { get; init; } = Array.Empty<string>();
}

public static class GeneratorPlanCapabilitySelectionStatus
{
    public const string Ready = "ready";
    public const string ReadyWithWarnings = "ready_with_warnings";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanCapabilitySelectionDiagnosticCodes
{
    public const string AtlasRootMissing = "capability_selection.atlas_root_missing";
    public const string AtlasFileMissing = "capability_selection.atlas_file_missing";
    public const string AtlasInvalidJson = "capability_selection.atlas_invalid_json";
    public const string MissingVariantId = "capability_selection.missing_variant_id";
    public const string UnknownVariantId = "capability_selection.unknown_variant_id";
    public const string UnknownFeatureBundleId = "capability_selection.unknown_feature_bundle_id";
    public const string UnknownRuntimeTargetId = "capability_selection.unknown_runtime_target_id";
    public const string IncompatiblePresentationWorld = "capability_selection.incompatible_presentation_world";
    public const string VariantExplicitlyIncompatible = "capability_selection.variant_explicitly_incompatible";
    public const string VariantNotRecommended = "capability_selection.variant_not_recommended";
    public const string NoFeatureBundlesSelected = "capability_selection.no_feature_bundles_selected";
    public const string MissingArtifactContract = "capability_selection.missing_artifact_contract";
    public const string MissingValidator = "capability_selection.missing_validator";
    public const string CapabilityGap = "capability_selection.capability_gap";
    public const string Loaded = "capability_selection.loaded";
}

public static class GeneratorPlanCapabilitySelectionDiagnosticCategories
{
    public const string Impossible = "impossible";
    public const string UnsupportedYet = "unsupported_yet";
    public const string Risky = "risky";
    public const string Info = "info";
}

public static class GeneratorPlanCapabilitySelectionArtifactIds
{
    public const string GeneratedBy = "generator_plan_capability_selection";
    public const string SelectionArtifactId = "artifact/generator_plan_capability_selection/latest";
    public const string SelectionArtifactKind = "generator_plan.capability_selection";
    public const string SelectionArtifactPath = ".llmgc/generator-plans/generator_plan_capability_selection.json";
}
