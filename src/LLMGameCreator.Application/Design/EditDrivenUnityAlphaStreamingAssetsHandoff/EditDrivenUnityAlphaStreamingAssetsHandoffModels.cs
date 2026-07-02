namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

public static class EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary
{
    public const string GoalId = "goal_082_edit_driven_unity_alpha_streamingassets_handoff";
    public const string ProductSmokeRoute = "goal-082-edit-driven-unity-alpha-streamingassets-handoff";
    public const string FinalGate = "edit_driven_unity_alpha_streamingassets_handoff_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-082-edit-driven-unity-alpha-streamingassets-handoff";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/EditDrivenGoal082";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/EditDrivenGoal082";
    public const string UnityProbeScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string Goal080RelativeOutputDirectory =
        ".llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge";
    public const string Goal081RelativeOutputDirectory =
        ".llmgc/procedural/goal-081-edit-driven-gamepackage-runtime-preview-playthrough";
    public const string Goal081HandoffText =
        "edit_driven_gamepackage_runtime_preview_playthrough_verification passed before Goal 082";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";

    public static readonly IReadOnlyList<string> RequiredArtifactFileNames =
    [
        "edit-driven-unity-alpha-streamingassets-handoff-report.md",
        "unity-streamingassets-handoff-manifest.json",
        "unity-streamingassets-file-ledger.json",
        "unity-probe-read-proof.json",
        "unity-probe-negative-proof.json",
        "unity-probe-command-transcript-proof.json",
        "winforms-binding-inventory.json",
        "quality-gate-scan.json",
        "source-artifact-manifest.json"
    ];

    public static readonly IReadOnlyList<string> RequiredUnityPayloadFileNames =
    [
        "handoff-manifest.json",
        "projected-package-index.json",
        "playthrough-command-index.json",
        "playthrough-transcript-index.json",
        "expected-hashes.json",
        "README.md"
    ];

    public static readonly IReadOnlyList<string> RequiredSourceArtifactRelativePaths =
    [
        Goal080RelativeOutputDirectory + "/edit-driven-gamepackage-runtime-preview-bridge-report.md",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/package.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/projected-package-index.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage/validation-report.json",
        Goal080RelativeOutputDirectory + "/runtime-preview-bridge-proof.json",
        Goal080RelativeOutputDirectory + "/projected-gamepackage-file-ledger.json",
        Goal080RelativeOutputDirectory + "/source-artifact-manifest.json",
        Goal081RelativeOutputDirectory + "/edit-driven-gamepackage-runtime-preview-playthrough-report.md",
        Goal081RelativeOutputDirectory + "/package-read-proof.json",
        Goal081RelativeOutputDirectory + "/playthrough-command-script.json",
        Goal081RelativeOutputDirectory + "/playthrough-transcript.json",
        Goal081RelativeOutputDirectory + "/playthrough-state-hash-chain.json",
        Goal081RelativeOutputDirectory + "/playthrough-coverage-ledger.json",
        Goal081RelativeOutputDirectory + "/playthrough-negative-proof.json",
        Goal081RelativeOutputDirectory + "/quality-gate-scan.json",
        Goal081RelativeOutputDirectory + "/source-artifact-manifest.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_handoff_manifest",
        "missing_expected_hashes",
        "missing_command_index",
        "tampered_projected_package_index",
        "tampered_expected_hashes",
        "fake_success_without_payload_read"
    ];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Warning(
        string code,
        string target,
        string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
    public bool Exists { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_source_artifact_manifest_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal081AcceptedByHandoff { get; init; }
    public bool Goal080ReportWasGreenProducedForReview { get; init; }
    public bool Goal080ArtifactAcceptedFalse { get; init; }
    public bool Goal081ReportWasGreenProducedForReview { get; init; }
    public bool Goal081ArtifactAcceptedFalse { get; init; }
    public string Goal080ReportHash { get; init; } = string.Empty;
    public string Goal080ProjectedPackageHash { get; init; } = string.Empty;
    public string Goal080ProjectedPackageIndexHash { get; init; } = string.Empty;
    public string Goal080ValidationReportHash { get; init; } = string.Empty;
    public string Goal080RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string Goal081ReportHash { get; init; } = string.Empty;
    public string Goal081PackageReadProofHash { get; init; } = string.Empty;
    public string Goal081CommandScriptHash { get; init; } = string.Empty;
    public string Goal081TranscriptHash { get; init; } = string.Empty;
    public string Goal081StateHashChainHash { get; init; } = string.Empty;
    public string Goal081CoverageLedgerHash { get; init; } = string.Empty;
    public string Goal081NegativeProofHash { get; init; } = string.Empty;
    public string Goal081QualityGateScanHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffExpectedHashes
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_expected_hashes_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string Goal080ReportHash { get; init; } = string.Empty;
    public string Goal080RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public string Goal081PackageReadProofHash { get; init; } = string.Empty;
    public string Goal081CommandScriptHash { get; init; } = string.Empty;
    public string Goal081TranscriptHash { get; init; } = string.Empty;
    public string Goal081StateHashChainHash { get; init; } = string.Empty;
    public string Goal081CoverageLedgerHash { get; init; } = string.Empty;
    public string Goal081NegativeProofHash { get; init; } = string.Empty;
    public string ProjectedPackageIndexPayloadHash { get; init; } = string.Empty;
    public string PlaythroughCommandIndexPayloadHash { get; init; } = string.Empty;
    public string PlaythroughTranscriptIndexPayloadHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_manifest_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public string StreamingAssetsRelativeRoot { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public int PayloadFileCount { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string ExpectedHashesHash { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFiles { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames;
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffProjectedPackageIndexPayload
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_projected_package_index_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string SourceGoal { get; init; } = "Goal080";
    public string ProjectedPackageSourceRelativePath { get; init; } =
        ".llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/package.json";
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public long ProjectedPackageByteCount { get; init; }
    public string ProjectedPackageIndexHash { get; init; } = string.Empty;
    public string ValidationReportHash { get; init; } = string.Empty;
    public string RuntimePreviewBridgeProofHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffCommandIndexPayload
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_playthrough_command_index_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string SourceGoal { get; init; } = "Goal081";
    public string CommandScriptHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffCommandTypeCount> CommandTypeCounts { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffCommandTypeCount
{
    public string CommandType { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffTranscriptIndexPayload
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_playthrough_transcript_index_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string SourceGoal { get; init; } = "Goal081";
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string CoverageLedgerHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public bool ReplayFinalHashMatchesOriginal { get; init; }
    public int CoveredRowCount { get; init; }
    public int CoveredTargetCount { get; init; }
    public int CoveredGoal078ActionCount { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_file_ledger_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public int FileCount { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffFileEntry> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_probe_read_proof_v1";
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool HandoffManifestRead { get; init; }
    public bool ExpectedHashesRead { get; init; }
    public bool ProjectedPackageIndexRead { get; init; }
    public bool PlaythroughCommandIndexRead { get; init; }
    public bool PlaythroughTranscriptIndexRead { get; init; }
    public bool RequiredPayloadFilesPresent { get; init; }
    public bool PayloadFileHashesMatchExpected { get; init; }
    public bool PackageHashMatchesGoal080 { get; init; }
    public bool CommandHashMatchesGoal081 { get; init; }
    public bool TranscriptHashMatchesGoal081 { get; init; }
    public bool StateHashMatchesGoal081 { get; init; }
    public bool CountsMatchExpected { get; init; }
    public bool UnityProbeSourceReferencesStreamingAssetsRoot { get; init; }
    public bool UnityProbeSourceDoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public int PayloadFileCount { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string CommandScriptHash { get; init; } = string.Empty;
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_command_transcript_proof_v1";
    public bool Passed { get; init; }
    public bool CommandScriptRead { get; init; }
    public bool TranscriptRead { get; init; }
    public bool StateHashChainRead { get; init; }
    public bool CoverageLedgerRead { get; init; }
    public bool CommandCountMatchesTranscript { get; init; }
    public bool CoverageCountsMatch { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string CommandScriptHash { get; init; } = string.Empty;
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string CoverageLedgerHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPageHandoffTabDeclared { get; init; }
    public bool ParentPageHandoffServiceLoaded { get; init; }
    public bool ParentPageHandoffControlBound { get; init; }
    public bool ParentPageActivationBindsGoal082Data { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal082Data { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int RawByteScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int RawPhysicalOneLineSourceCount { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int ZeroLfSourceCount { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int FilesWithTooFewLinesForSizeCount { get; init; }
    public bool UnityProbeIncludedInRawScan { get; init; }
    public bool WinFormsParentIncludedInRawScan { get; init; }
    public bool Goal082ApplicationFilesIncludedInRawScan { get; init; }
    public bool SyntheticCrOnlySourceRejected { get; init; }
    public bool SyntheticZeroLfOneLineSourceRejected { get; init; }
    public bool SyntheticZeroLfOnePhysicalLineRejected { get; init; }
    public int ParentWorkspaceLineCount { get; init; }
    public int AlphaRuntimeBootstrapBaselineLineCount { get; init; }
    public string AlphaRuntimeBootstrapBaselineHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public int UnityProbeLineCount { get; init; }
    public bool UnityProbeBelow300Lines { get; init; }
    public bool UnityProbeUsesStreamingAssetsPath { get; init; }
    public bool UnityProbeNoRuntimeProviderLlmMediaDependency { get; init; }
    public bool ParentUiBindingPassed { get; init; }
    public bool EvidenceContainsAbsoluteLocalPaths { get; init; }
    public bool EvidenceContainsTimestampLikeValues { get; init; }
    public bool EvidenceContainsHeavyLogs { get; init; }
    public bool EvidenceContainsScratchTamperFiles { get; init; }
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int LogicalLineCount { get; init; }
    public long ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int LfByteCount { get; init; }
    public int CrByteCount { get; init; }
    public int RawPhysicalLineCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public bool RawPhysicalOneLineSource { get; init; }
    public bool ZeroLfSource { get; init; }
    public bool CrOnlySource { get; init; }
    public bool ContainsCrOnlyLineEndings { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
    public bool TooFewLinesForSizeSourceCandidate { get; init; }
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffReport
{
    public string SchemaVersion { get; init; } =
        "edit_driven_unity_alpha_streamingassets_handoff_report_v1";
    public string GoalId { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal081AcceptedByHandoff { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public int PayloadFileCount { get; init; }
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string CommandScriptHash { get; init; } = string.Empty;
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public string FileLedgerHash { get; init; } = string.Empty;
    public string ProbeReadProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string CommandTranscriptProofHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string SourceArtifactManifestHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult
{
    public EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest HandoffManifest { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger FileLedger { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof ProbeReadProof { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof NegativeProof { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof CommandTranscriptProof { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenUnityAlphaStreamingAssetsHandoffReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenUnityAlphaStreamingAssetsHandoffWriteResult
{
    public EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal sealed record Goal082SourceContext
{
    public string RootPath { get; init; } = string.Empty;
    public EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public string Goal080ReportMarkdown { get; init; } = string.Empty;
    public string Goal081ReportMarkdown { get; init; } = string.Empty;
    public string Goal080PackageJson { get; init; } = string.Empty;
    public string Goal080ProjectedIndexJson { get; init; } = string.Empty;
    public string Goal080ValidationReportJson { get; init; } = string.Empty;
    public string Goal080RuntimePreviewBridgeProofJson { get; init; } = string.Empty;
    public string Goal081PackageReadProofJson { get; init; } = string.Empty;
    public string Goal081CommandScriptJson { get; init; } = string.Empty;
    public string Goal081TranscriptJson { get; init; } = string.Empty;
    public string Goal081StateHashChainJson { get; init; } = string.Empty;
    public string Goal081CoverageLedgerJson { get; init; } = string.Empty;
    public string Goal081NegativeProofJson { get; init; } = string.Empty;
    public string Goal081QualityGateJson { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public int Goal078ActionCount { get; init; }
    public int CommandCount { get; init; }
    public string ProjectedPackageHash { get; init; } = string.Empty;
    public string CommandScriptHash { get; init; } = string.Empty;
    public string TranscriptHash { get; init; } = string.Empty;
    public string StateHashChainHash { get; init; } = string.Empty;
    public string CoverageLedgerHash { get; init; } = string.Empty;
    public string FinalCoverageStateHash { get; init; } = string.Empty;
    public string ReplayFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffCommandTypeCount> CommandTypeCounts { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics { get; init; } = [];
}
