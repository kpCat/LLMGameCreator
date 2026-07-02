using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

public static class EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary
{
    public const string GoalId = "goal_081_edit_driven_gamepackage_runtime_preview_playthrough";
    public const string ProductSmokeRoute = "goal-081-edit-driven-gamepackage-runtime-preview-playthrough";
    public const string FinalGate = "edit_driven_gamepackage_runtime_preview_playthrough_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-081-edit-driven-gamepackage-runtime-preview-playthrough";
    public const string Goal080RelativeOutputDirectory =
        ".llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge";
    public const string Goal078RelativeOutputDirectory =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session";
    public const string Goal080HandoffText =
        "edit_driven_gamepackage_runtime_preview_bridge_verification passed before Goal 081";

    public static readonly IReadOnlyList<string> RequiredArtifactFileNames =
    [
        "edit-driven-gamepackage-runtime-preview-playthrough-report.md",
        "playthrough-command-script.json",
        "playthrough-transcript.json",
        "playthrough-state-hash-chain.json",
        "playthrough-coverage-ledger.json",
        "package-read-proof.json",
        "playthrough-negative-proof.json",
        "winforms-binding-inventory.json",
        "quality-gate-scan.json",
        "source-artifact-manifest.json"
    ];

    public static readonly IReadOnlyList<string> RequiredSourceArtifactRelativePaths =
    [
        Goal080RelativeOutputDirectory + "/edit-driven-gamepackage-runtime-preview-bridge-report.md",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/package.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/projected-package-index.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/player-readable-bridge-index.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/source-targets.json",
        Goal080RelativeOutputDirectory + "/runtime-preview-bridge-proof.json",
        Goal080RelativeOutputDirectory + "/runtime-preview-negative-proof.json",
        Goal080RelativeOutputDirectory + "/quality-gate-scan.json",
        Goal080RelativeOutputDirectory + "/source-artifact-manifest.json",
        Goal078RelativeOutputDirectory + "/playable-session-action-log.json",
        Goal078RelativeOutputDirectory + "/playable-session-replay-proof.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_projected_gamepackage_payload",
        "tampered_projected_gamepackage_payload",
        "missing_player_readable_bridge_index",
        "command_script_nonexistent_target",
        "replay_order_mismatch",
        "fake_success_without_package_read",
        "source_goal080_lineage_hash_mismatch"
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

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic Warning(
        string code,
        string target,
        string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_source_artifact_manifest_v1";
    public string GoalId { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal080AcceptedByHandoff { get; init; }
    public bool Goal080ReportWasGreenProducedForReview { get; init; }
    public bool Goal080ArtifactAcceptedFalse { get; init; }
    public string Goal080ReportHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string RuntimePreviewNegativeProofHash { get; init; } = string.Empty;
    public string Goal078ActionLogHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_package_read_proof_v1";
    public bool Passed { get; init; }
    public bool Goal080HandoffRecorded { get; init; }
    public bool Goal080ReportGreen { get; init; }
    public bool Goal080ArtifactAcceptedFalse { get; init; }
    public bool ProjectedPackagePayloadRead { get; init; }
    public bool ProjectedPackageDeserialized { get; init; }
    public bool GamePackageValidationPassed { get; init; }
    public bool ProjectedIndexRead { get; init; }
    public bool PlayerReadableBridgeIndexRead { get; init; }
    public bool SourceTargetsRead { get; init; }
    public bool RuntimePreviewBridgeProofRead { get; init; }
    public bool RuntimePreviewBridgeProofPassed { get; init; }
    public bool RuntimePreviewNegativeProofPassed { get; init; }
    public bool Goal080QualityGatePassed { get; init; }
    public bool PackageHashMatchesGoal080Report { get; init; }
    public bool PackageHashMatchesProjectedIndex { get; init; }
    public bool PackageHashMatchesBridgeProof { get; init; }
    public bool RuntimePreviewProjectionPassed { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public string StartMapId { get; init; } = string.Empty;
    public string Goal080ReportHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string ProjectedIndexHash { get; init; } = string.Empty;
    public string PlayerReadableBridgeIndexHash { get; init; } = string.Empty;
    public string SourceTargetsHash { get; init; } = string.Empty;
    public string RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string Goal078ActionLogHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_command_script_v1";
    public string GoalId { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> Commands { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughCommand
{
    public int CommandIndex { get; init; }
    public string CommandType { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string PackageItemId { get; init; } = string.Empty;
    public string PackageInteractionId { get; init; } = string.Empty;
    public string PackageQuestId { get; init; } = string.Empty;
    public string PackageDialogueId { get; init; } = string.Empty;
    public string PackageMechanicId { get; init; } = string.Empty;
    public string SourceTargetPayloadHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public IReadOnlyList<string> CoveredGoal078ActionIds { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughTranscript
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_transcript_v1";
    public bool Passed { get; init; }
    public int CommandCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public bool ReplayFinalHashMatchesOriginal { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughTranscriptEntry> Entries { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughTranscriptEntry
{
    public int CommandIndex { get; init; }
    public string CommandType { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public int VisitedRowCount { get; init; }
    public int VisitedTargetCount { get; init; }
    public int CollectedTargetCount { get; init; }
    public int CoveredGoal078ActionCount { get; init; }
    public string StateHash { get; init; } = string.Empty;
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_state_hash_chain_v1";
    public bool Passed { get; init; }
    public string InitialPackageReadStateHash { get; init; } = string.Empty;
    public string CommandScriptStateHash { get; init; } = string.Empty;
    public string ReplayTranscriptStateHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayRerunFinalStateHash { get; init; } = string.Empty;
    public bool ReplayRerunFinalHashMatchesFirstRun { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChainEntry> Entries { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChainEntry
{
    public string StageId { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_coverage_ledger_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CoveredRowCount { get; init; }
    public int CoveredTargetCount { get; init; }
    public int CoveredGoal078ActionCount { get; init; }
    public bool AllGoal077TargetsCovered { get; init; }
    public bool AllGoal078ActionsCovered { get; init; }
    public bool PackageReadRequired { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCoverageRow> Rows { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughCoverageRow
{
    public string RowId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public int TargetCount { get; init; }
    public int CoveredTargetCount { get; init; }
    public int CoveredGoal078ActionCount { get; init; }
    public IReadOnlyList<string> TargetIds { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPagePlaythroughTabDeclared { get; init; }
    public bool ParentPagePlaythroughServiceLoaded { get; init; }
    public bool ParentPagePlaythroughControlBound { get; init; }
    public bool ParentPageActivationBindsGoal081Data { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal081Data { get; init; }
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_quality_gate_scan_v1";
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
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool EvidenceContainsAbsoluteLocalPaths { get; init; }
    public bool EvidenceContainsTimestampLikeValues { get; init; }
    public bool EvidenceContainsHeavyLogs { get; init; }
    public bool EvidenceContainsScratchTamperFiles { get; init; }
    public bool ForbiddenAreaEvidenceDetected { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughQualityFileScan
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

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughReport
{
    public string SchemaVersion { get; init; } =
        "edit_driven_gamepackage_runtime_preview_playthrough_report_v1";
    public string GoalId { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal080AcceptedByHandoff { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string Goal080ReportHash { get; init; } = string.Empty;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string InitialPackageReadStateHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public string PackageReadProofHash { get; init; } = string.Empty;
    public string CommandScriptHash { get; init; } = string.Empty;
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string CoverageLedgerHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string SourceArtifactManifestHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult
{
    public EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof PackageReadProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript CommandScript { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughTranscript Transcript { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain StateHashChain { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger CoverageLedger { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof NegativeProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenGamePackageRuntimePreviewPlaythroughWriteResult
{
    public EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal sealed record Goal081ProjectedPackageIndex
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string PackageFile { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string SourceGoal077ReportHash { get; init; } = string.Empty;
    public string SourceGoal078ReportHash { get; init; } = string.Empty;
    public string SourceGoal079ReportHash { get; init; } = string.Empty;
    public string SourceGoal079AReportHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<Goal081ProjectedPackageIndexRow> Rows { get; init; } = [];
}

internal sealed record Goal081ProjectedPackageIndexRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetIds { get; init; } = [];
}

internal sealed record Goal081PlayerReadableBridgeIndex
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int CommandCount { get; init; }
    public IReadOnlyList<Goal081PlayerReadableBridgeScenario> Scenarios { get; init; } = [];
}

internal sealed record Goal081PlayerReadableBridgeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PlayerFacingQuest { get; init; } = string.Empty;
    public string PlayerFacingDialogue { get; init; } = string.Empty;
    public IReadOnlyList<Goal081PlayerReadableBridgeTarget> ProjectedTargets { get; init; } = [];
}

internal sealed record Goal081PlayerReadableBridgeTarget
{
    public string TargetId { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string ProjectedItem { get; init; } = string.Empty;
    public string ProjectedInteraction { get; init; } = string.Empty;
}

internal sealed record Goal081SourceTargetsDocument
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public int TargetCount { get; init; }
    public IReadOnlyList<Goal081SourceTargetRecord> Targets { get; init; } = [];
}

internal sealed record Goal081SourceTargetRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
}

internal sealed record Goal081SourceContext
{
    public string RootPath { get; init; } = string.Empty;
    public GamePackageDefinition? Package { get; init; }
    public Goal081ProjectedPackageIndex ProjectedIndex { get; init; } = new();
    public Goal081PlayerReadableBridgeIndex PlayerIndex { get; init; } = new();
    public Goal081SourceTargetsDocument SourceTargets { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeProof BridgeProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeNegativeProof BridgeNegativeProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan BridgeQualityGate { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest BridgeSourceManifest { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionActionLog Goal078ActionLog { get; init; } = new();
    public EditDrivenReviewPackagePlayableSessionReplayProof Goal078ReplayProof { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof PackageReadProof { get; init; } = new();
    public IReadOnlyDictionary<string, string> PackageItemByTargetId { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> PackageInteractionByTargetId { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics { get; init; } = [];
}
