namespace LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

public static class EditDrivenPlayableReviewPackageMaterializationVocabulary
{
    public const string GoalId = "goal_077_edit_driven_review_package_materialization";
    public const string ProductSmokeRoute = "goal-077-edit-driven-review-package-materialization";
    public const string FinalGate = "edit_driven_review_package_materialization_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization";
    public const string Goal076RelativeOutputDirectory =
        ".llmgc/procedural/goal-076-edit-driven-playable-preview-refresh";
    public const string ReviewPackageDirectoryName = "review-package";
    public const string Goal076AcceptedHandoffText =
        "edit_driven_playable_preview_refresh_verification passed before Goal 077";

    public static readonly IReadOnlyList<string> RequiredSourceArtifactNames =
    [
        "edit-driven-playable-preview-refresh-report.md",
        "playable-preview-refresh-manifest.json",
        "gamepackage-refresh-plan.json",
        "unity-player-handoff-manifest.json",
        "state-transition-proof.json",
        "quality-gate-scan.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_package_target_file",
        "tampered_package_target_file",
        "player_index_missing_row_or_target"
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

public sealed record EditDrivenPlayableReviewPackageDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenPlayableReviewPackageDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenPlayableReviewPackageDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenReviewPackageSourceArtifactReference
{
    public string SourceGoal { get; init; } = "Goal076";
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record EditDrivenReviewPackageSourceArtifactManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_source_artifact_manifest_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenPlayableReviewPackageMaterializationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal076AcceptedByUserHandoff { get; init; }
    public bool Goal076ReportWasGreenProducedForReview { get; init; }
    public bool Goal076ArtifactAcceptedFalse { get; init; }
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076ManifestHash { get; init; } = string.Empty;
    public string SourceGoal076RefreshPlanHash { get; init; } = string.Empty;
    public string SourceGoal076HandoffManifestHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGoal076PreviewManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = string.Empty;
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public string PreviewRefreshHash { get; init; } = string.Empty;
    public string RefreshPlanHash { get; init; } = string.Empty;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public int ChangedRowCount { get; init; }
    public int PackageTargetCount { get; init; }
    public bool StateTransitionProofPassed { get; init; }
    public bool GamePackageRefreshPlanPassed { get; init; }
    public bool StagedHandoffManifestPassed { get; init; }
    public bool TamperNegativeProofPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
}

public sealed record EditDrivenGoal076RefreshPlan
{
    public string SchemaVersion { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool PublicGamePackageSchemaMutationRequired { get; init; }
    public string FullMaterializationDisposition { get; init; } = string.Empty;
    public string PreviewExportRefreshPayloadRef { get; init; } = string.Empty;
    public string PreviewExportRefreshPayloadHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenGoal076RefreshPlanRow> Rows { get; init; } = [];
}

public sealed record EditDrivenGoal076RefreshPlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string RefreshKey { get; init; } = string.Empty;
    public string SourceAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGoal076RefreshTarget> Targets { get; init; } = [];
}

public sealed record EditDrivenGoal076RefreshTarget
{
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string ValidationRequirement { get; init; } = string.Empty;
}

public sealed record EditDrivenGoal076HandoffManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManifestRelativePath { get; init; } = string.Empty;
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public string PreviewRefreshHash { get; init; } = string.Empty;
    public string RefreshPlanHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPackageLogicalTargets { get; init; } = [];
    public IReadOnlyList<string> PlayerFacingScenarioIds { get; init; } = [];
    public IReadOnlyList<EditDrivenGoal076HandoffRow> Rows { get; init; } = [];
}

public sealed record EditDrivenGoal076HandoffRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PreviewRefreshKey { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPackageLogicalTargets { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record EditDrivenGoal076StateTransitionProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int RollbackRestoredRowCount { get; init; }
    public int ReplayRestoredAfterRowCount { get; init; }
    public IReadOnlyList<EditDrivenGoal076StateRow> Rows { get; init; } = [];
}

public sealed record EditDrivenGoal076StateRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool RollbackRestored { get; init; }
    public bool ReplayRestoredAfter { get; init; }
    public string PreviewRefreshKey { get; init; } = string.Empty;
    public string PreviewAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGoal076AppliedChange> AppliedChanges { get; init; } = [];
    public IReadOnlyList<string> PackageLogicalTargets { get; init; } = [];
}

public sealed record EditDrivenGoal076AppliedChange
{
    public string CandidateId { get; init; } = string.Empty;
    public string CandidateKind { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string PackageLogicalTarget { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackageTargetFile
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_target_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public string TargetId { get; init; } = string.Empty;
    public string SourceRowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public string ValidationRequirement { get; init; } = string.Empty;
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076ManifestHash { get; init; } = string.Empty;
    public string SourceGoal076RefreshPlanHash { get; init; } = string.Empty;
    public string SourceGoal076HandoffManifestHash { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackageFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public string RowId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackageManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_manifest_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenPlayableReviewPackageMaterializationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string PackageRoot { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.ReviewPackageDirectoryName;
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076ManifestHash { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageFileEntry> Files { get; init; } = [];
}

public sealed record EditDrivenReviewPackageFileLedger
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_file_ledger_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageFileEntry> Files { get; init; } = [];
}

public sealed record EditDrivenReviewPackageIndex
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_index_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageIndexRow> Rows { get; init; } = [];
}

public sealed record EditDrivenReviewPackageIndexRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenReviewPackageIndexTarget> Targets { get; init; } = [];
}

public sealed record EditDrivenReviewPackageIndexTarget
{
    public string TargetId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record EditDrivenPlayerReadablePackageIndex
{
    public string SchemaVersion { get; init; } = "edit_driven_player_readable_package_index_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076HandoffManifestHash { get; init; } = string.Empty;
    public int ScenarioCount { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public bool AllScenarioIdsMapped { get; init; }
    public bool AllPlayerMarkersResolved { get; init; }
    public bool AllRowsRepresented { get; init; }
    public bool AllTargetsRepresented { get; init; }
    public IReadOnlyList<EditDrivenPlayerScenarioMapping> Scenarios { get; init; } = [];
}

public sealed record EditDrivenPlayerScenarioMapping
{
    public string ScenarioId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetIds { get; init; } = [];
    public IReadOnlyList<string> TargetFileRefs { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayerMarkerReference> PlayerMarkers { get; init; } = [];
}

public sealed record EditDrivenPlayerMarkerReference
{
    public string Marker { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetIds { get; init; } = [];
}

public sealed record EditDrivenPackageTargetCoverage
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_target_coverage_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenPackageTargetCoverageRow> Rows { get; init; } = [];
}

public sealed record EditDrivenPackageTargetCoverageRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int TargetCount { get; init; }
    public IReadOnlyList<string> TargetIds { get; init; } = [];
}

public sealed record EditDrivenReviewPackageStateLineageProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_state_lineage_proof_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageStateLineageRow> Rows { get; init; } = [];
}

public sealed record EditDrivenReviewPackageStateLineageRow
{
    public string RowId { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool RollbackRestored { get; init; }
    public bool ReplayRestoredAfter { get; init; }
}

public sealed record EditDrivenReviewPackageStagedReadProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_staged_read_proof_v1";
    public bool Passed { get; init; }
    public bool ManifestExists { get; init; }
    public bool PackageIndexExists { get; init; }
    public bool PlayerReadableIndexExists { get; init; }
    public bool AllLedgerFilesExist { get; init; }
    public bool AllFileHashesMatch { get; init; }
    public bool AllExpectedRowsPresent { get; init; }
    public bool AllExpectedTargetsPresent { get; init; }
    public bool SourceGoal076HashesMatch { get; init; }
    public bool StateLineageValid { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackageNegativeProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenReviewPackageNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackageWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPageReviewPackageTabDeclared { get; init; }
    public bool ParentPageReviewPackageEvidenceServiceLoaded { get; init; }
    public bool ParentPageReviewPackageControlBound { get; init; }
    public bool ParentPageActivationBindsGoal077Data { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackageWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal077Data { get; init; }
}

public sealed record EditDrivenReviewPackageQualityGateScan
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int ReviewPackageTargetFileCount { get; init; }
    public bool ParentUiBindingPassed { get; init; }
    public bool ReportOnlySmokeDetected { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public string AlphaRuntimeBootstrapNoChangeStatus { get; init; } = "recorded_read_only";
    public bool EvidenceContainsAbsoluteLocalPaths { get; init; }
    public bool EvidenceContainsTimestampLikeValues { get; init; }
    public bool EvidenceContainsHeavyLogs { get; init; }
    public bool EvidenceContainsScratchTamperFiles { get; init; }
    public IReadOnlyList<EditDrivenReviewPackageQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackageQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record EditDrivenReviewPackageMaterializationReport
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_materialization_report_v1";
    public string GoalId { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenPlayableReviewPackageMaterializationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal076AcceptedByUserHandoff { get; init; }
    public bool Goal076ImplementationGreen { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ReviewPackageFileCount { get; init; }
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076ManifestHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PlayerReadablePackageIndexHash { get; init; } = string.Empty;
    public string PackageTargetCoverageHash { get; init; } = string.Empty;
    public string StateLineageProofHash { get; init; } = string.Empty;
    public string StagedPackageReadProofHash { get; init; } = string.Empty;
    public string TamperNegativeProofHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenPlayableReviewPackageMaterializationBuildResult
{
    public EditDrivenReviewPackageSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenReviewPackageManifest ReviewPackageManifest { get; init; } = new();
    public EditDrivenReviewPackageFileLedger PackageFileLedger { get; init; } = new();
    public EditDrivenReviewPackageIndex PackageIndex { get; init; } = new();
    public EditDrivenPlayerReadablePackageIndex PlayerReadablePackageIndex { get; init; } = new();
    public EditDrivenPackageTargetCoverage PackageTargetCoverage { get; init; } = new();
    public EditDrivenReviewPackageStateLineageProof StateLineageProof { get; init; } = new();
    public EditDrivenReviewPackageStagedReadProof StagedPackageReadProof { get; init; } = new();
    public EditDrivenReviewPackageNegativeProof TamperNegativeProof { get; init; } = new();
    public EditDrivenReviewPackageWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenReviewPackageQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenReviewPackageMaterializationReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ReviewPackageFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenPlayableReviewPackageMaterializationWriteResult
{
    public EditDrivenPlayableReviewPackageMaterializationBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReviewPackageDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
