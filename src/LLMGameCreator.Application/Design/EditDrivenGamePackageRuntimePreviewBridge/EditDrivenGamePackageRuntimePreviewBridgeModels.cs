using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

public static class EditDrivenGamePackageRuntimePreviewBridgeVocabulary
{
    public const string GoalId = "goal_080_edit_driven_gamepackage_runtime_preview_bridge";
    public const string ProductSmokeRoute = "goal-080-edit-driven-gamepackage-runtime-preview-bridge";
    public const string FinalGate = "edit_driven_gamepackage_runtime_preview_bridge_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge";
    public const string ProjectedPackageDirectoryName = "projected-gamepackage";
    public const string Goal077RelativeOutputDirectory =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization";
    public const string Goal078RelativeOutputDirectory =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session";
    public const string Goal079RelativeOutputDirectory =
        ".llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation";
    public const string Goal079ARelativeOutputDirectory =
        ".llmgc/procedural/goal-079a-source-format-line-ending-guard";
    public const string Goal079AcceptedForContinuationText =
        "edit_driven_spine_quality_consolidation_verification accepted for continuation before Goal 080";
    public const string Goal079ASourceFormatHandoffText =
        "source_format_line_ending_guard_verification passed before Goal 080";

    public static readonly IReadOnlyList<string> RequiredArtifactFileNames =
    [
        "edit-driven-gamepackage-runtime-preview-bridge-report.md",
        "projected-gamepackage-manifest.json",
        "projected-gamepackage-file-ledger.json",
        "runtime-preview-bridge-proof.json",
        "runtime-preview-negative-proof.json",
        "winforms-binding-inventory.json",
        "quality-gate-scan.json",
        "source-artifact-manifest.json"
    ];

    public static readonly IReadOnlyList<string> RequiredSourceArtifactRelativePaths =
    [
        Goal077RelativeOutputDirectory + "/edit-driven-review-package-materialization-report.md",
        Goal077RelativeOutputDirectory + "/package-file-ledger.json",
        Goal077RelativeOutputDirectory + "/review-package/manifest.json",
        Goal077RelativeOutputDirectory + "/review-package/package-index.json",
        Goal077RelativeOutputDirectory + "/review-package/player-readable-index.json",
        Goal078RelativeOutputDirectory + "/edit-driven-review-package-playable-session-report.md",
        Goal078RelativeOutputDirectory + "/package-read-proof.json",
        Goal078RelativeOutputDirectory + "/playable-session-action-log.json",
        Goal078RelativeOutputDirectory + "/playable-session-replay-proof.json",
        Goal078RelativeOutputDirectory + "/tamper-negative-proof.json",
        Goal078RelativeOutputDirectory + "/player-command-index.json",
        Goal079RelativeOutputDirectory + "/edit-driven-spine-quality-consolidation-report.md",
        Goal079RelativeOutputDirectory + "/quality-gate-scan.json",
        Goal079ARelativeOutputDirectory + "/source-format-line-ending-guard-report.md",
        Goal079ARelativeOutputDirectory + "/source-format-line-ending-guard-scan.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_projected_package_file",
        "tampered_projected_package_file",
        "projected_index_missing_target",
        "fake_success_without_projected_package_read",
        "source_lineage_hash_mismatch"
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

internal static class EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths
{
    public const string PackageJsonRelativePath = "projected-gamepackage/package.json";
    public const string ProjectedIndexRelativePath = "projected-gamepackage/projected-package-index.json";
    public const string PlayerIndexRelativePath = "projected-gamepackage/player-readable-bridge-index.json";
    public const string ValidationReportRelativePath = "projected-gamepackage/validation-report.json";
    public const string SourceTargetsRelativePath = "projected-gamepackage/source-targets.json";
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenGamePackageRuntimePreviewBridgeDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenGamePackageRuntimePreviewBridgeDiagnostic Warning(
        string code,
        string target,
        string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_source_artifact_manifest_v1";
    public string GoalId { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenGamePackageRuntimePreviewBridgeVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal079AcceptedForContinuation { get; init; }
    public bool Goal079ASourceFormatGuardPassedByHandoff { get; init; }
    public bool Goal079ReportWasGreenProducedForReview { get; init; }
    public bool Goal079ArtifactAcceptedFalse { get; init; }
    public string Goal077ReportHash { get; init; } = string.Empty;
    public string Goal078ReportHash { get; init; } = string.Empty;
    public string Goal079ReportHash { get; init; } = string.Empty;
    public string Goal079AReportHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeTargetRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public string ValidationRequirement { get; init; } = string.Empty;
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeRowRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeTargetRecord> Targets { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeProjectedFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public string RowId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_projected_manifest_v1";
    public string GoalId { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenGamePackageRuntimePreviewBridgeVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public string ProjectedPackageRoot { get; init; } =
        EditDrivenGamePackageRuntimePreviewBridgeVocabulary.ProjectedPackageDirectoryName;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public int ProjectedPackageFileCount { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string SourceGoal078ReportHash { get; init; } = string.Empty;
    public string SourceGoal079ReportHash { get; init; } = string.Empty;
    public string SourceGoal079AReportHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string ProjectedPackageFileLedgerHash { get; init; } = string.Empty;
    public string RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string RuntimePreviewNegativeProofHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public bool ProjectedPackageReadProofPassed { get; init; }
    public bool RuntimePreviewBridgeProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_projected_file_ledger_v1";
    public string GoalId { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeProjectedFileEntry> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_proof_v1";
    public bool Passed { get; init; }
    public bool ProjectedPackagePayloadRead { get; init; }
    public bool ProjectedPackageDeserialized { get; init; }
    public bool GamePackageValidationPassed { get; init; }
    public bool RuntimePreviewProjectionPassed { get; init; }
    public bool InteractionCatalogProjectionPassed { get; init; }
    public bool AllGoal077TargetsCovered { get; init; }
    public bool AllGoal078ActionsCovered { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public int RuntimePreviewRegionCount { get; init; }
    public int RuntimePreviewNpcCount { get; init; }
    public int RuntimePreviewItemCount { get; init; }
    public int RuntimePreviewDialogueCount { get; init; }
    public int RuntimePreviewQuestCount { get; init; }
    public int RuntimePreviewMechanicCount { get; init; }
    public int InteractionCategoryCount { get; init; }
    public int InteractionEntryCount { get; init; }
    public string InitialProjectionStateHash { get; init; } = string.Empty;
    public string PostPackageReadStateHash { get; init; } = string.Empty;
    public string PostRuntimePreviewStateHash { get; init; } = string.Empty;
    public string ActionCoverageStateHash { get; init; } = string.Empty;
    public string Goal078ReplayFinalStateHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string ProjectedPackageFileLedgerHash { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimePreviewWarnings { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeNegativeProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPageRuntimePreviewBridgeTabDeclared { get; init; }
    public bool ParentPageRuntimePreviewBridgeServiceLoaded { get; init; }
    public bool ParentPageRuntimePreviewBridgeControlBound { get; init; }
    public bool ParentPageActivationBindsGoal080Data { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal080Data { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int RawPhysicalOneLineSourceCount { get; init; }
    public int ZeroLfSourceCount { get; init; }
    public int CrOnlySourceCount { get; init; }
    public bool SyntheticCrOnlySourceRejected { get; init; }
    public bool SyntheticZeroLfOneLineSourceRejected { get; init; }
    public bool ParentUiBindingPassed { get; init; }
    public bool ReportOnlySmokeDetected { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public bool AlphaRuntimeBootstrapRecordedReadOnly { get; init; }
    public bool EvidenceContainsAbsoluteLocalPaths { get; init; }
    public bool EvidenceContainsTimestampLikeValues { get; init; }
    public bool EvidenceContainsHeavyLogs { get; init; }
    public bool EvidenceContainsScratchTamperFiles { get; init; }
    public bool ForbiddenAreaEvidenceDetected { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool RawPhysicalOneLineSource { get; init; }
    public bool ZeroLfSource { get; init; }
    public bool CrOnlySource { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeReport
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_bridge_report_v1";
    public string GoalId { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenGamePackageRuntimePreviewBridgeVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal079AcceptedForContinuation { get; init; }
    public bool Goal079ASourceFormatGuardPassedByHandoff { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public int ProjectedPackageFileCount { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string SourceGoal078ReportHash { get; init; } = string.Empty;
    public string SourceGoal079ReportHash { get; init; } = string.Empty;
    public string SourceGoal079AReportHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string ProjectedPackageManifestHash { get; init; } = string.Empty;
    public string ProjectedPackageFileLedgerHash { get; init; } = string.Empty;
    public string RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string RuntimePreviewNegativeProofHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeBuildResult
{
    public EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageManifest ProjectedPackageManifest { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger ProjectedPackageFileLedger { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeProof RuntimePreviewBridgeProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeNegativeProof RuntimePreviewNegativeProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ProjectedPackageFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenGamePackageRuntimePreviewBridgeWriteResult
{
    public EditDrivenGamePackageRuntimePreviewBridgeBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ProjectedPackageDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal sealed record Goal080SourceContext
{
    public string RootPath { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeRowRecord> Rows { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeTargetRecord> Targets { get; init; } = [];
    public EditDrivenReviewPackagePlayableSessionActionLog ActionLog { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionReplayProof ReplayProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public string Goal077ReportHash { get; init; } = string.Empty;
    public string Goal078ReportHash { get; init; } = string.Empty;
    public string Goal079ReportHash { get; init; } = string.Empty;
    public string Goal079AReportHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> Diagnostics { get; init; } = [];
}
