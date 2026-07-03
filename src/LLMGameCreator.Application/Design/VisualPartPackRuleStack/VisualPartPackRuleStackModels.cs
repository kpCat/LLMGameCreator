namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public static class VisualPartPackRuleStackVocabulary
{
    public const string GoalId = "goal_085_deepsearch_backed_visual_part_pack_rule_stack";
    public const string ProductSmokeRoute = "goal-085-deepsearch-backed-visual-part-pack-rule-stack";
    public const string FinalGate = "visual_part_pack_rule_stack_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack";

    public const string ManifestSchemaVersion = "visual_part_pack_rule_stack_manifest_v1";
    public const string CatalogSchemaVersion = "visual_part_pack_catalog_v1";
    public const string ValidationMatrixSchemaVersion = "visual_part_pack_validation_matrix_v1";
    public const string NegativeProofSchemaVersion = "visual_part_pack_negative_proof_v1";
    public const string DeepsearchLineageSchemaVersion = "deepsearch_lineage_inventory_v1";
    public const string Goal084BindingSchemaVersion = "goal084_contract_binding_matrix_v1";
    public const string WaterBiomeCoverageSchemaVersion = "water_biome_coverage_matrix_v1";
    public const string QualityGateSchemaVersion = "visual_part_pack_quality_gate_scan_v1";
}

public enum VisualPartPackKind
{
    Unknown = 0,
    TileTerrain,
    WaterBiome,
    SettlementFacade,
    CreatureBodyPlanEquipment,
    UiThemeEffect,
    AdultRatingExtension
}

public enum VisualContentRating
{
    Unspecified = 0,
    Safe,
    SuggestiveMetadata,
    AdultMetadataOnly
}

public enum VisualPartExportPolicy
{
    Unspecified = 0,
    PublicSafe,
    MatureOptional,
    AdultBuildOnly,
    PrivateLocalOnly,
    Blocked
}

public enum VisualPartReviewStatus
{
    Unspecified = 0,
    CandidateQuarantined,
    ApprovedMetadata,
    Rejected
}

public enum VisualPartProviderState
{
    None = 0,
    MetadataOnly,
    CandidateQuarantine,
    ApprovedAsset,
    Rejected
}

public sealed record VisualPartPackManifest
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.ManifestSchemaVersion;
    public string ManifestId { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public bool StrictReferenceValidation { get; init; } = true;
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
    public IReadOnlyList<VisualPartPackDefinition> PartPacks { get; init; } = [];
    public IReadOnlyList<VisualPartPackRecipe> Recipes { get; init; } = [];
}

public sealed record VisualPartPackDefinition
{
    public string PackId { get; init; } = string.Empty;
    public VisualPartPackKind Kind { get; init; } = VisualPartPackKind.Unknown;
    public VisualContentRating Rating { get; init; } = VisualContentRating.Safe;
    public VisualPartExportPolicy ExportPolicy { get; init; } = VisualPartExportPolicy.PublicSafe;
    public VisualPartReviewStatus ReviewStatus { get; init; } = VisualPartReviewStatus.ApprovedMetadata;
    public VisualPartProviderState ProviderState { get; init; } = VisualPartProviderState.MetadataOnly;
    public bool IsAdultRatingExtension { get; init; }
    public string SafeFallbackPackId { get; init; } = string.Empty;
    public string MetadataRelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string ProvenanceRef { get; init; } = string.Empty;
    public bool PromptTextIsSourceOfTruth { get; init; }
    public IReadOnlyList<string> FeatureTags { get; init; } = [];
    public IReadOnlyList<VisualPartDefinition> Parts { get; init; } = [];
    public IReadOnlyList<VisualPartLayer> Layers { get; init; } = [];
    public IReadOnlyList<VisualMaskDefinition> Masks { get; init; } = [];
    public IReadOnlyList<VisualSocketDefinition> Sockets { get; init; } = [];
    public IReadOnlyList<VisualAnchorDefinition> Anchors { get; init; } = [];
    public IReadOnlyList<VisualPaletteProfile> PaletteProfiles { get; init; } = [];
    public IReadOnlyList<VisualPaletteSwapRule> PaletteSwapRules { get; init; } = [];
    public IReadOnlyList<VisualOverlayRule> OverlayRules { get; init; } = [];
    public IReadOnlyList<VisualBiomeProfile> BiomeProfiles { get; init; } = [];
    public IReadOnlyList<VisualWaterProfile> WaterProfiles { get; init; } = [];
    public IReadOnlyList<VisualTerrainTransitionRule> TerrainTransitionRules { get; init; } = [];
    public IReadOnlyList<VisualAutoTileRule> AutoTileRules { get; init; } = [];
    public IReadOnlyList<VisualObjectPlacementRule> ObjectPlacementRules { get; init; } = [];
    public IReadOnlyList<VisualCreatureBodyPlanProfile> CreatureBodyPlanProfiles { get; init; } = [];
    public IReadOnlyList<VisualEquipmentOverlayProfile> EquipmentOverlayProfiles { get; init; } = [];
    public IReadOnlyList<VisualUiThemeProfile> UiThemeProfiles { get; init; } = [];
    public IReadOnlyList<VisualEffectProfile> EffectProfiles { get; init; } = [];
    public int BodyPlanGrammarCapacity { get; init; }
    public int HandAuthoredSpeciesAssetCount { get; init; }
}

public sealed record VisualPartDefinition
{
    public string PartId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool RequiresLayeredComposition { get; init; }
    public string PaletteProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public IReadOnlyList<string> MaskIds { get; init; } = [];
    public IReadOnlyList<string> SocketIds { get; init; } = [];
    public IReadOnlyList<string> AnchorIds { get; init; } = [];
    public IReadOnlyList<string> CompatibleTags { get; init; } = [];
}

public sealed record VisualPartLayer
{
    public string LayerId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Role { get; init; } = string.Empty;
}

public sealed record VisualMaskDefinition
{
    public string MaskId { get; init; } = string.Empty;
    public string MaskKind { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
}

public sealed record VisualSocketDefinition
{
    public string SocketId { get; init; } = string.Empty;
    public string SocketKind { get; init; } = string.Empty;
    public IReadOnlyList<string> CompatibleRoles { get; init; } = [];
}

public sealed record VisualAnchorDefinition
{
    public string AnchorId { get; init; } = string.Empty;
    public string AnchorKind { get; init; } = string.Empty;
    public double NormalizedX { get; init; }
    public double NormalizedY { get; init; }
}

public sealed record VisualPaletteProfile
{
    public string PaletteProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> ColorSlots { get; init; } = [];
}

public sealed record VisualPaletteSwapRule
{
    public string RuleId { get; init; } = string.Empty;
    public string PaletteProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedTargetTags { get; init; } = [];
}

public sealed record VisualOverlayRule
{
    public string RuleId { get; init; } = string.Empty;
    public string OverlayKind { get; init; } = string.Empty;
    public IReadOnlyList<string> CompatibleLayerIds { get; init; } = [];
}

public sealed record VisualBiomeProfile
{
    public string BiomeProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> BiomeKinds { get; init; } = [];
}

public sealed record VisualWaterProfile
{
    public string WaterProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> WaterKinds { get; init; } = [];
    public bool CoastAware { get; init; }
    public bool RiverAware { get; init; }
    public bool LakeAware { get; init; }
    public bool MarshAware { get; init; }
}

public sealed record VisualTerrainTransitionRule
{
    public string RuleId { get; init; } = string.Empty;
    public string FromTerrain { get; init; } = string.Empty;
    public string ToTerrain { get; init; } = string.Empty;
    public string MaskId { get; init; } = string.Empty;
}

public sealed record VisualAutoTileRule
{
    public string RuleId { get; init; } = string.Empty;
    public IReadOnlyList<string> TerrainKinds { get; init; } = [];
    public string EdgeMaskId { get; init; } = string.Empty;
}

public sealed record VisualObjectPlacementRule
{
    public string RuleId { get; init; } = string.Empty;
    public string ObjectKind { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTags { get; init; } = [];
}

public sealed record VisualCreatureBodyPlanProfile
{
    public string BodyPlanProfileId { get; init; } = string.Empty;
    public string BodyPlanKind { get; init; } = string.Empty;
    public bool AdultEligible { get; init; }
    public bool AgeKnownAdult { get; init; }
    public bool Sapient { get; init; }
    public bool HumanoidCompatible { get; init; }
    public bool AgeAmbiguous { get; init; }
    public bool NonSapient { get; init; }
    public IReadOnlyList<string> CompatibleSocketIds { get; init; } = [];
}

public sealed record VisualEquipmentOverlayProfile
{
    public string EquipmentOverlayProfileId { get; init; } = string.Empty;
    public string OverlayKind { get; init; } = string.Empty;
    public IReadOnlyList<string> CompatibleSocketIds { get; init; } = [];
    public IReadOnlyList<string> CompatibleBodyPlanProfileIds { get; init; } = [];
}

public sealed record VisualUiThemeProfile
{
    public string UiThemeProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> UiElementKinds { get; init; } = [];
    public string SafeFallbackThemeId { get; init; } = string.Empty;
}

public sealed record VisualEffectProfile
{
    public string EffectProfileId { get; init; } = string.Empty;
    public string EffectKind { get; init; } = string.Empty;
    public bool HasSafeFallback { get; init; }
}

public sealed record VisualPartPackRecipe
{
    public string RecipeId { get; init; } = string.Empty;
    public string PackId { get; init; } = string.Empty;
    public string PaletteProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> PartIds { get; init; } = [];
    public IReadOnlyList<string> DependsOnRecipeIds { get; init; } = [];
    public string SafeFallbackRecipeId { get; init; } = string.Empty;
    public string Goal084SlotId { get; init; } = string.Empty;
}

public sealed record VisualRuleStackValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualRuleStackDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualRuleStackDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualRuleStackDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static VisualRuleStackDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record VisualPartPackCatalog
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualPartPackRuleStackVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixturePackCount { get; init; }
    public IReadOnlyList<string> FixturePackIds { get; init; } = [];
    public VisualPartPackManifest Manifest { get; init; } = new();
}

public sealed record VisualPartPackValidationMatrix
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.ValidationMatrixSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FixturePackCount { get; init; }
    public IReadOnlyList<VisualPartPackValidationRow> Rows { get; init; } = [];
}

public sealed record VisualPartPackValidationRow
{
    public string PackId { get; init; } = string.Empty;
    public VisualPartPackKind Kind { get; init; }
    public bool Passed { get; init; }
    public int PartCount { get; init; }
    public int RecipeCount { get; init; }
    public bool HasSafeFallback { get; init; }
    public IReadOnlyList<VisualRuleStackDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualPartPackNegativeProof
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualPartPackNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualPartPackNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualRuleStackDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record DeepsearchLineageInventory
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.DeepsearchLineageSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool IndexedInContextIndex { get; init; }
    public bool RoutedInFullGeneratorGoalQueue { get; init; }
    public int DocumentCount { get; init; }
    public IReadOnlyList<DeepsearchLineageRecord> Records { get; init; } = [];
}

public sealed record DeepsearchLineageRecord
{
    public string Path { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record Goal084ContractBindingMatrix
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.Goal084BindingSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal084ArtifactExists { get; init; }
    public bool Goal084AcceptedFalse { get; init; }
    public string Goal084CatalogHash { get; init; } = string.Empty;
    public IReadOnlyList<Goal084ContractBindingRow> Rows { get; init; } = [];
}

public sealed record Goal084ContractBindingRow
{
    public string PackId { get; init; } = string.Empty;
    public string Goal084SlotId { get; init; } = string.Empty;
    public bool SlotExistsInGoal084Catalog { get; init; }
    public string BindingKind { get; init; } = "metadata_contract_lineage";
}

public sealed record WaterBiomeCoverageMatrix
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.WaterBiomeCoverageSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public bool Passed { get; init; }
    public string PackId { get; init; } = string.Empty;
    public bool SeaCovered { get; init; }
    public bool LakeCovered { get; init; }
    public bool RiverCovered { get; init; }
    public bool CoastCovered { get; init; }
    public bool MarshCovered { get; init; }
    public bool BridgeCovered { get; init; }
    public bool DockCovered { get; init; }
    public bool WaterObjectCovered { get; init; }
}

public sealed record VisualPartPackQualityGateScan
{
    public string SchemaVersion { get; init; } = VisualPartPackRuleStackVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualPartPackRuleStackVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool AllDeepsearchDocsConsumed { get; init; }
    public bool Goal084ContractLineagePassed { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WaterBiomeCoveragePassed { get; init; }
    public bool CreatureBodyPlanEquipmentCoveragePassed { get; init; }
    public bool UiEffectWeatherDayNightCoveragePassed { get; init; }
    public bool AdultMetadataOnlyFallbackBound { get; init; }
    public bool NoForbiddenFilesChanged { get; init; } = true;
    public bool NoExternalDependenciesAdded { get; init; } = true;
    public bool NoImagesMediaBinaryAssetsAdded { get; init; } = true;
    public bool NoProviderIntegrationAdded { get; init; } = true;
    public bool NoRuntimeOrUnityChanged { get; init; } = true;
    public bool NoPublicGamePackageSchemaChanged { get; init; } = true;
    public bool NoProjectFilesChanged { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualRuleStackDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualPartPackRuleStackReport
{
    public string GoalId { get; init; } = VisualPartPackRuleStackVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualPartPackRuleStackVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool ContractModelsImplemented { get; init; }
    public bool ValidatorImplemented { get; init; }
    public bool FixturesImplemented { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool DeepsearchLineagePassed { get; init; }
    public bool Goal084BindingPassed { get; init; }
    public bool WaterBiomeCoveragePassed { get; init; }
    public int FixturePackCount { get; init; }
    public int NegativeScenarioCount { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string ValidationMatrixHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string DeepsearchLineageHash { get; init; } = string.Empty;
    public string Goal084BindingHash { get; init; } = string.Empty;
    public string WaterBiomeCoverageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualRuleStackEvidenceResult
{
    public VisualPartPackCatalog Catalog { get; init; } = new();
    public VisualPartPackValidationMatrix ValidationMatrix { get; init; } = new();
    public VisualPartPackNegativeProof NegativeProof { get; init; } = new();
    public DeepsearchLineageInventory DeepsearchLineageInventory { get; init; } = new();
    public Goal084ContractBindingMatrix Goal084ContractBindingMatrix { get; init; } = new();
    public WaterBiomeCoverageMatrix WaterBiomeCoverageMatrix { get; init; } = new();
    public VisualPartPackQualityGateScan QualityGateScan { get; init; } = new();
    public VisualPartPackRuleStackReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string ValidationMatrixJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string DeepsearchLineageJson { get; init; } = string.Empty;
    public string Goal084BindingMatrixJson { get; init; } = string.Empty;
    public string WaterBiomeCoverageMatrixJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record VisualPartPackRuleStackWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string ValidationMatrixJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string DeepsearchLineageJsonPath { get; init; } = string.Empty;
    public string Goal084BindingMatrixJsonPath { get; init; } = string.Empty;
    public string WaterBiomeCoverageMatrixJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
}
