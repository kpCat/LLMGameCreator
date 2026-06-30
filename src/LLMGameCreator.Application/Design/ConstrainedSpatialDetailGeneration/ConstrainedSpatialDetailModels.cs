namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public static class ConstrainedSpatialDetailVocabulary
{
    public const string GoalId = "goal_062_constrained_spatial_detail_generation";
    public const string ProductSmokeRoute = "goal-062-constrained-spatial-detail-generation";
    public const string FinalGate = "constrained_spatial_detail_generation_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal059RelativeOutputDirectory = ".llmgc/procedural/goal-059-full-generator-variability-regression-matrix";
    public const string StagingRoot = "staging";
    public const string UnitySpatialDetailCommandPlanStagingRelativePath = "spatial-detail/unity-spatial-detail-command-plan.json";

    public const string SourceManifestSchemaVersion = "constrained_spatial_detail_source_manifest_v1";
    public const string PaletteCatalogSchemaVersion = "constrained_spatial_palette_catalog_v1";
    public const string RewriteRuleCatalogSchemaVersion = "constrained_spatial_rewrite_rule_catalog_v1";
    public const string ConstraintRuleCatalogSchemaVersion = "constrained_spatial_constraint_rule_catalog_v1";
    public const string SpatialDetailMatrixSchemaVersion = "constrained_spatial_detail_matrix_v1";
    public const string SpatialDetailRowSchemaVersion = "constrained_spatial_detail_row_v1";
    public const string ReachabilityMatrixSchemaVersion = "constrained_spatial_reachability_matrix_v1";
    public const string RepairFallbackMatrixSchemaVersion = "constrained_spatial_repair_fallback_matrix_v1";
    public const string UnityCommandPlanSchemaVersion = "constrained_spatial_unity_command_plan_v1";
    public const string UnityProofSummarySchemaVersion = "constrained_spatial_unity_proof_summary_v1";
    public const string PreviewExportPayloadSchemaVersion = "constrained_spatial_preview_export_payload_v1";
    public const string InvalidDiagnosticsMatrixSchemaVersion = "invalid_constrained_spatial_detail_diagnostics_matrix_v1";
    public const string ReportSchemaVersion = "constrained_spatial_detail_generation_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyList<string> SeedIds =
    [
        "seed_alpha",
        "seed_beta",
        "seed_gamma"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal061_source",
        "fake_package_row_id",
        "fake_family",
        "fake_seed",
        "invalid_tile_id",
        "missing_entry",
        "missing_exit",
        "unreachable_objective",
        "contradiction_no_tile_candidate",
        "unsafe_path_traversal",
        "external_asset_provenance_leak",
        "copied_mxgmn_sample_asset_claim",
        "provider_network_llm_rag_claim",
        "lua_execution_claim",
        "public_gamepackage_mutation_claim",
        "runtime_ui_broad_mutation_claim",
        "nondeterministic_ordering",
        "missing_unity_proof_trace"
    ];

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string SeedOrderingKey(string seedId) =>
        seedId switch
        {
            "seed_alpha" => "001-seed-alpha",
            "seed_beta" => "002-seed-beta",
            "seed_gamma" => "003-seed-gamma",
            _ => "999-" + seedId
        };
}

public sealed record ConstrainedSpatialDetailOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record ConstrainedSpatialDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static ConstrainedSpatialDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static ConstrainedSpatialDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record ConstrainedSpatialSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialPackageRowSource
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string Goal060PackageRelativePath { get; init; } = string.Empty;
    public string Goal059DerivedCampaignHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ReviewPackageCommandSteps { get; init; } = [];
}

public sealed record ConstrainedSpatialSourceBundle
{
    public bool Goal061AcceptedByUserHandoff { get; init; }
    public bool Goal061ReviewPackageRcManifestPassed { get; init; }
    public bool Goal061UnityProofPassed { get; init; }
    public bool Goal060PackageInventoryConsumed { get; init; }
    public bool Goal059VarianceConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialPackageRowSource> PackageRows { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialSourceManifest
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ConstrainedSpatialDetailVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = ConstrainedSpatialDetailVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal061AcceptedByUserHandoff { get; init; }
    public bool Goal061ReviewPackageRcManifestPassed { get; init; }
    public bool Goal061UnityProofPassed { get; init; }
    public bool Goal060PackageInventoryConsumed { get; init; }
    public bool Goal059VarianceConsumed { get; init; }
    public int PackageRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialTileDefinition
{
    public string TileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
    public IReadOnlyList<string> FamilyApplicability { get; init; } = [];
    public bool Passable { get; init; }
    public bool Hazard { get; init; }
    public bool Resource { get; init; }
    public bool Objective { get; init; }
    public bool Door { get; init; }
    public bool Corridor { get; init; }
    public bool Settlement { get; init; }
    public bool Biome { get; init; }
    public IReadOnlyList<string> AdjacencyTags { get; init; } = [];
    public string RenderMarker { get; init; } = ".";
    public string ThumbnailColor { get; init; } = "#808080";
    public string Provenance { get; init; } = "in_house_fixture";
}

public sealed record ConstrainedSpatialPaletteCatalog
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.PaletteCatalogSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int TileCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialTileDefinition> Tiles { get; init; } = [];
}

public sealed record ConstrainedSpatialRewriteRule
{
    public string RuleId { get; init; } = string.Empty;
    public IReadOnlyList<string> FamilyApplicability { get; init; } = [];
    public int Priority { get; init; }
    public string MatchDescription { get; init; } = string.Empty;
    public string EffectDescription { get; init; } = string.Empty;
    public int DeterministicApplicationOrder { get; init; }
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialRewriteRuleCatalog
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.RewriteRuleCatalogSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RuleCount { get; init; }
    public IReadOnlyList<ConstrainedSpatialRewriteRule> Rules { get; init; } = [];
}

public sealed record ConstrainedSpatialConstraintRule
{
    public string RuleId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string TileId { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedNeighborTags { get; init; } = [];
    public bool ContradictionDetected { get; init; }
    public int RetryBudget { get; init; }
    public int FallbackBudget { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialConstraintRuleCatalog
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.ConstraintRuleCatalogSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RuleCount { get; init; }
    public IReadOnlyList<ConstrainedSpatialConstraintRule> Rules { get; init; } = [];
}

public sealed record ConstrainedSpatialCell
{
    public string CellId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string TileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
}

public sealed record ConstrainedSpatialAnchor
{
    public string AnchorId { get; init; } = string.Empty;
    public string Semantic { get; init; } = string.Empty;
    public string CellId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string TileId { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialRoute
{
    public string RouteId { get; init; } = string.Empty;
    public string FromAnchorId { get; init; } = string.Empty;
    public string ToAnchorId { get; init; } = string.Empty;
    public bool RouteVerified { get; init; }
    public IReadOnlyList<string> RouteCellIds { get; init; } = [];
}

public sealed record ConstrainedSpatialReachabilityProof
{
    public string RowId { get; init; } = string.Empty;
    public bool Reachable { get; init; }
    public bool RouteVerified { get; init; }
    public ConstrainedSpatialRoute EntryToObjective { get; init; } = new();
    public ConstrainedSpatialRoute ObjectiveToExit { get; init; } = new();
    public ConstrainedSpatialRoute FamilySpecificRoute { get; init; } = new();
    public int BlockedCellCount { get; init; }
    public int PassableCellCount { get; init; }
    public IReadOnlyList<string> SemanticAnchorsFound { get; init; } = [];
    public IReadOnlyList<string> RouteCellIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialVarianceMetrics
{
    public string RowId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, int> TileHistogram { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> AnchorPositions { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public int PathLength { get; init; }
    public int HazardCount { get; init; }
    public int ResourceCount { get; init; }
    public int EncounterCount { get; init; }
    public IReadOnlyDictionary<string, int> FamilySpecificSemanticCounts { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> MeaningfulMetricKeys { get; init; } = [];
    public string VarianceMarker { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialRepairFallbackRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int RetryBudget { get; init; }
    public int FallbackBudget { get; init; }
    public int ContradictionCount { get; init; }
    public bool FallbackApplied { get; init; }
    public IReadOnlyList<string> AppliedRepairRuleIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialDetailRow
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.SpatialDetailRowSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageRowId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string ReviewPackageRef { get; init; } = string.Empty;
    public string Goal059DerivedCampaignHash { get; init; } = string.Empty;
    public string DeterministicSeed { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public IReadOnlyList<string> TileDataCompact { get; init; } = [];
    public IReadOnlyDictionary<string, string> TileIdByMarker { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<ConstrainedSpatialCell> Cells { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialAnchor> Anchors { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialRoute> Paths { get; init; } = [];
    public IReadOnlyList<string> AppliedRewriteRuleIds { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> ConstraintDiagnostics { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> RepairDiagnostics { get; init; } = [];
    public ConstrainedSpatialReachabilityProof ReachabilityProof { get; init; } = new();
    public ConstrainedSpatialVarianceMetrics VarianceMetrics { get; init; } = new();
    public ConstrainedSpatialRepairFallbackRecord RepairFallback { get; init; } = new();
    public string PreviewExportRef { get; init; } = string.Empty;
    public string ThumbnailRef { get; init; } = string.Empty;
    public string ThumbnailDecision { get; init; } = "skipped_no_existing_bcl_png_helper_required_for_goal";
    public string Provenance { get; init; } = "in_house_fixture";
    public string RowHash { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialDetailMatrixRowSummary
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string RowHash { get; init; } = string.Empty;
    public string VarianceMarker { get; init; } = string.Empty;
    public bool Reachable { get; init; }
    public bool RouteVerified { get; init; }
    public int PathLength { get; init; }
    public IReadOnlyDictionary<string, int> TileHistogram { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
}

public sealed record ConstrainedSpatialDetailMatrix
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.SpatialDetailMatrixSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public bool SameFamilyRowsDifferByTwoMetrics { get; init; }
    public bool FamiliesDifferByPaletteAndRuleSet { get; init; }
    public IReadOnlyList<ConstrainedSpatialDetailMatrixRowSummary> Rows { get; init; } = [];
}

public sealed record ConstrainedSpatialReachabilityProofMatrix
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.ReachabilityMatrixSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int ReachableRowCount { get; init; }
    public int RouteVerifiedRowCount { get; init; }
    public IReadOnlyList<ConstrainedSpatialReachabilityProof> Rows { get; init; } = [];
}

public sealed record ConstrainedSpatialRepairFallbackMatrix
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.RepairFallbackMatrixSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int ContradictionScenarioCount { get; init; }
    public IReadOnlyList<ConstrainedSpatialRepairFallbackRecord> Rows { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> ContradictionDiagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string SpatialDetailRowRef { get; init; } = string.Empty;
    public string RowHash { get; init; } = string.Empty;
    public bool Reachable { get; init; }
    public bool RouteVerified { get; init; }
    public string VarianceMarker { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record ConstrainedSpatialUnityCommandPlan
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = ConstrainedSpatialDetailVocabulary.FinalGate;
    public IReadOnlyList<ConstrainedSpatialUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record ConstrainedSpatialUnityProofSummary
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.UnityProofSummarySchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool PlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public string UnityBuildLogRelativePath { get; init; } = string.Empty;
    public string LaunchLogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    public int ProvenRowCount { get; init; }
    public IReadOnlyList<string> MatchedMarkers { get; init; } = [];
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public ConstrainedSpatialUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public IReadOnlyList<string> TileDataCompact { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialAnchor> Anchors { get; init; } = [];
    public IReadOnlyList<ConstrainedSpatialRoute> Paths { get; init; } = [];
    public string PackageRowRef { get; init; } = string.Empty;
    public string ReviewPackageRef { get; init; } = string.Empty;
    public string RowHash { get; init; } = string.Empty;
    public string ThumbnailRef { get; init; } = string.Empty;
    public string Provenance { get; init; } = "in_house_fixture";
}

public sealed record ConstrainedSpatialPreviewExportPayload
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<ConstrainedSpatialPreviewExportRow> Rows { get; init; } = [];
}

public sealed record InvalidConstrainedSpatialDetailScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidConstrainedSpatialDetailDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.InvalidDiagnosticsMatrixSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidConstrainedSpatialDetailScenario> Scenarios { get; init; } = [];
}

public sealed record ConstrainedSpatialDetailGenerationReport
{
    public string SchemaVersion { get; init; } = ConstrainedSpatialDetailVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = ConstrainedSpatialDetailVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ConstrainedSpatialDetailVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = ConstrainedSpatialDetailVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal061AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool PaletteCatalogPassed { get; init; }
    public bool RewriteRuleCatalogPassed { get; init; }
    public bool ConstraintRuleCatalogPassed { get; init; }
    public bool SpatialDetailMatrixPassed { get; init; }
    public bool ReachabilityProofPassed { get; init; }
    public bool RepairFallbackMatrixPassed { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllUnitySpatialMarkersMatched { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public int UnityProvenRowCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string PaletteCatalogHash { get; init; } = string.Empty;
    public string RewriteRuleCatalogHash { get; init; } = string.Empty;
    public string ConstraintRuleCatalogHash { get; init; } = string.Empty;
    public string SpatialDetailMatrixHash { get; init; } = string.Empty;
    public string ReachabilityProofMatrixHash { get; init; } = string.Empty;
    public string RepairFallbackMatrixHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<ConstrainedSpatialDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ConstrainedSpatialDetailEvidenceResult
{
    public ConstrainedSpatialSourceManifest SourceManifest { get; init; } = new();
    public ConstrainedSpatialPaletteCatalog PaletteCatalog { get; init; } = new();
    public ConstrainedSpatialRewriteRuleCatalog RewriteRuleCatalog { get; init; } = new();
    public ConstrainedSpatialConstraintRuleCatalog ConstraintRuleCatalog { get; init; } = new();
    public ConstrainedSpatialDetailMatrix SpatialDetailMatrix { get; init; } = new();
    public IReadOnlyList<ConstrainedSpatialDetailRow> SpatialDetailRows { get; init; } = [];
    public ConstrainedSpatialReachabilityProofMatrix ReachabilityProofMatrix { get; init; } = new();
    public ConstrainedSpatialRepairFallbackMatrix RepairFallbackMatrix { get; init; } = new();
    public ConstrainedSpatialUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public ConstrainedSpatialUnityProofSummary UnityProofSummary { get; init; } = new();
    public ConstrainedSpatialPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidConstrainedSpatialDetailDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public ConstrainedSpatialDetailGenerationReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RowJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<ConstrainedSpatialFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record ConstrainedSpatialFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record ConstrainedSpatialDetailWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public ConstrainedSpatialDetailEvidenceResult Result { get; init; } = new();
}
