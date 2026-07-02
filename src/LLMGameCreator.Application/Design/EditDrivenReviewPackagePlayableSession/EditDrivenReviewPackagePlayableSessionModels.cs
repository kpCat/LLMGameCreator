namespace LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

public static class EditDrivenReviewPackagePlayableSessionVocabulary
{
    public const string GoalId = "goal_078_edit_driven_review_package_playable_session";
    public const string ProductSmokeRoute = "goal-078-edit-driven-review-package-playable-session";
    public const string FinalGate = "edit_driven_review_package_playable_session_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session";
    public const string Goal077RelativeOutputDirectory =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization";
    public const string Goal077AcceptedHandoffText =
        "edit_driven_review_package_materialization_verification passed before Goal 078";

    public static readonly IReadOnlyList<string> RequiredSourceArtifactRelativePaths =
    [
        Goal077RelativeOutputDirectory + "/edit-driven-review-package-materialization-report.md",
        Goal077RelativeOutputDirectory + "/package-file-ledger.json",
        Goal077RelativeOutputDirectory + "/review-package/manifest.json",
        Goal077RelativeOutputDirectory + "/review-package/package-index.json",
        Goal077RelativeOutputDirectory + "/review-package/player-readable-index.json",
        Goal077RelativeOutputDirectory + "/tamper-negative-proof.json",
        Goal077RelativeOutputDirectory + "/quality-gate-scan.json",
        Goal077RelativeOutputDirectory + "/source-artifact-manifest.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_target_file",
        "tampered_target_payload",
        "illegal_action_target",
        "replay_order_mismatch",
        "fake_success_without_target_payload_read"
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

public sealed record EditDrivenReviewPackagePlayableSessionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenReviewPackagePlayableSessionDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenReviewPackagePlayableSessionDiagnostic Warning(
        string code,
        string target,
        string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenReviewPackagePlayableSessionSourceArtifactReference
{
    public string SourceGoal { get; init; } = "Goal077";
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record EditDrivenReviewPackagePlayableSessionSourceArtifactManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_review_package_playable_session_source_artifact_manifest_v1";
    public string GoalId { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenReviewPackagePlayableSessionVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal077AcceptedByUserHandoff { get; init; }
    public bool Goal077ReportWasGreenProducedForReview { get; init; }
    public bool Goal077ArtifactAcceptedFalse { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string SourceGoal077ReportDeclaredHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string PlayerReadableIndexHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionReportFields
{
    public string ImplementationStatus { get; init; } = string.Empty;
    public string Accepted { get; init; } = string.Empty;
    public string SourceGoal076ReportHash { get; init; } = string.Empty;
    public string SourceGoal076ManifestHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PlayerReadablePackageIndexHash { get; init; } = string.Empty;
    public string ReportHash { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackagePlayableSessionLedgerFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public string RowId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackagePlayableSessionTargetRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string ValidationRequirement { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackagePlayableSessionRowRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionTargetRecord> Targets { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionPackageReadProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_read_proof_v1";
    public bool Passed { get; init; }
    public bool Goal077ReportExists { get; init; }
    public bool Goal077ReportHashFieldsPresent { get; init; }
    public bool Goal077ReportHashesMatchCurrentFiles { get; init; }
    public bool ReviewPackageManifestExists { get; init; }
    public bool PackageLedgerExists { get; init; }
    public bool PackageIndexExists { get; init; }
    public bool PlayerReadableIndexExists { get; init; }
    public bool AllLedgerFilesExist { get; init; }
    public bool AllLedgerFileHashesMatch { get; init; }
    public bool AllPackageIndexTargetsInLedger { get; init; }
    public bool AllPlayerIndexTargetsInLedger { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int LedgerFileCount { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string PlayerReadableIndexHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionAction
{
    public int ActionIndex { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string TargetRelativePath { get; init; } = string.Empty;
    public string TargetFileHash { get; init; } = string.Empty;
    public string TargetPayloadHash { get; init; } = string.Empty;
    public bool TargetPayloadRead { get; init; }
    public string PackageManifestHash { get; init; } = string.Empty;
    public string PackageLedgerHash { get; init; } = string.Empty;
    public string PlayerReadableIndexHash { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackagePlayableSessionActionLog
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_action_log_v1";
    public string GoalId { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ActionCount { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> Actions { get; init; } = [];
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionStateChainEntry
{
    public int ActionIndex { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string CurrentRowId { get; init; } = string.Empty;
    public string CurrentProfileId { get; init; } = string.Empty;
    public int VisitedRowCount { get; init; }
    public int VisitedTargetCount { get; init; }
    public int CompletedRowCount { get; init; }
    public int AppliedTargetOutcomeCount { get; init; }
    public int ActionCount { get; init; }
    public string StateHash { get; init; } = string.Empty;
}

public sealed record EditDrivenReviewPackagePlayableSessionStateChain
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_state_chain_v1";
    public bool Passed { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string SavedSessionHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int ActionCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionStateChainEntry> Entries { get; init; } = [];
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionReplayProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_replay_proof_v1";
    public bool Passed { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string OriginalFinalStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public bool InitialDiffersFromFinal { get; init; }
    public bool ReplayFinalHashMatchesOriginal { get; init; }
    public bool ReplayOrderMismatchRejected { get; init; }
    public bool IllegalActionTargetRejected { get; init; }
    public bool FakeSuccessWithoutPayloadReadRejected { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionNegativeProof
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionPlayerCommandIndex
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_player_command_index_v1";
    public bool Passed { get; init; }
    public int RowCommandGroupCount { get; init; }
    public int CommandCount { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionPlayerCommandGroup> CommandGroups { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionPlayerCommandGroup
{
    public string ScenarioId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> CommandIds { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPagePlaySessionTabDeclared { get; init; }
    public bool ParentPagePlaySessionEvidenceServiceLoaded { get; init; }
    public bool ParentPagePlaySessionControlBound { get; init; }
    public bool ParentPageActivationBindsGoal078Data { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal078Data { get; init; }
}

public sealed record EditDrivenReviewPackagePlayableSessionQualityGateScan
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public bool ParentUiBindingPassed { get; init; }
    public bool ReportOnlySmokeDetected { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public string AlphaRuntimeBootstrapExpectedHash { get; init; } = string.Empty;
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool EvidenceContainsAbsoluteLocalPaths { get; init; }
    public bool EvidenceContainsTimestampLikeValues { get; init; }
    public bool EvidenceContainsHeavyLogs { get; init; }
    public bool EvidenceContainsScratchTamperFiles { get; init; }
    public bool ForbiddenAreaEvidenceDetected { get; init; }
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record EditDrivenReviewPackagePlayableSessionManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_manifest_v1";
    public string GoalId { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenReviewPackagePlayableSessionVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string PackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string PlayerReadableIndexHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string SavedSessionHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public bool PackageReadProofPassed { get; init; }
    public bool ReplayProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
}

public sealed record EditDrivenReviewPackagePlayableSessionReport
{
    public string SchemaVersion { get; init; } = "edit_driven_review_package_playable_session_report_v1";
    public string GoalId { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenReviewPackagePlayableSessionVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal077AcceptedByUserHandoff { get; init; }
    public bool Goal077ImplementationGreen { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string PackageManifestHash { get; init; } = string.Empty;
    public string PackageFileLedgerHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string PlayerReadableIndexHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string SavedSessionHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public string PackageReadProofHash { get; init; } = string.Empty;
    public string ActionLogHash { get; init; } = string.Empty;
    public string StateChainHash { get; init; } = string.Empty;
    public string ReplayProofHash { get; init; } = string.Empty;
    public string TamperNegativeProofHash { get; init; } = string.Empty;
    public string PlayerCommandIndexHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenReviewPackagePlayableSessionBuildResult
{
    public EditDrivenReviewPackagePlayableSessionSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionPackageReadProof PackageReadProof { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionManifest Manifest { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionActionLog ActionLog { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionStateChain StateChain { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionReplayProof ReplayProof { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionNegativeProof TamperNegativeProof { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionPlayerCommandIndex PlayerCommandIndex { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenReviewPackagePlayableSessionWriteResult
{
    public EditDrivenReviewPackagePlayableSessionBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
