namespace LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

public static class EditDrivenSpineQualityConsolidationVocabulary
{
    public const string GoalId = "goal_079_edit_driven_spine_quality_consolidation";
    public const string ProductSmokeRoute = "goal-079-edit-driven-spine-quality-consolidation";
    public const string FinalGate = "edit_driven_spine_quality_consolidation_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation";
    public const string Goal078AcceptedHandoffText =
        "edit_driven_review_package_playable_session_verification passed before Goal 079";
    public const string Goal072BlockedText = "implementationStatus=BLOCKED";
    public const string AdaptiveDocsDebtCommit = "c8343e8";

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_target_file",
        "tampered_target_payload",
        "replay_order_mismatch",
        "illegal_action_target",
        "fake_success_without_target_payload_read"
    ];
}

public sealed record EditDrivenSpineQualityConsolidationBuildOptions
{
    public IReadOnlyDictionary<string, string?> ArtifactTextOverridesByRelativePath { get; init; } =
        new Dictionary<string, string?>(StringComparer.Ordinal);
}

public sealed record EditDrivenSpineQualityConsolidationDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenSpineQualityConsolidationDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenSpineQualityConsolidationDiagnostic Warning(
        string code,
        string target,
        string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenSpineQualityConsolidationReportFields
{
    public string ImplementationStatus { get; init; } = string.Empty;
    public string Accepted { get; init; } = string.Empty;
    public string Gate { get; init; } = string.Empty;
    public string DeclaredHash { get; init; } = string.Empty;
}

public sealed record EditDrivenSpineQualityConsolidationSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string ArtifactHash { get; init; } = string.Empty;
}

public sealed record EditDrivenSpineQualityConsolidationGoalEvidence
{
    public string GoalId { get; init; } = string.Empty;
    public int GoalNumber { get; init; }
    public string ReportRelativePath { get; init; } = string.Empty;
    public string QualityGateRelativePath { get; init; } = string.Empty;
    public bool ReportExists { get; init; }
    public bool QualityGateExists { get; init; }
    public string ReportHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeclaredReportHash { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public string Accepted { get; init; } = string.Empty;
    public string Gate { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationSourceArtifactManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_source_artifact_manifest_v1";
    public string GoalId { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenSpineQualityConsolidationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal078AcceptedByUserHandoff { get; init; }
    public bool Goal078ArtifactGreenAcceptedFalse { get; init; }
    public bool Goal072PreservedAsHistoricalBlocked { get; init; }
    public bool AdaptiveDocsDebtStillP3 { get; init; }
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationGoalEvidence> GoalEvidence { get; init; } = [];
    public IReadOnlyList<EditDrivenSpineQualityConsolidationSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationChainItem
{
    public string GoalId { get; init; } = string.Empty;
    public int GoalNumber { get; init; }
    public string ReportHash { get; init; } = string.Empty;
    public string DeclaredReportHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public string Accepted { get; init; } = string.Empty;
}

public sealed record EditDrivenSpineQualityConsolidationChainManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_chain_manifest_v1";
    public string GoalId { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.GoalId;
    public int ChainItemCount { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationChainItem> ChainItems { get; init; } = [];
    public string Goal078PackageReadProofHash { get; init; } = string.Empty;
    public string Goal078ReplayProofHash { get; init; } = string.Empty;
    public string Goal078NegativeProofHash { get; init; } = string.Empty;
}

public sealed record EditDrivenSpineQualityConsolidationReadinessDashboard
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_acceptance_readiness_v1";
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.FinalGate;
    public bool Goal078AcceptedByUserHandoff { get; init; }
    public bool Goal078ArtifactStillAcceptedFalse { get; init; }
    public bool PackageReadProofPassed { get; init; }
    public bool ReplayProofPassed { get; init; }
    public bool ReplayFinalHashMatchesOriginal { get; init; }
    public bool NegativeProofPassed { get; init; }
    public int ChainItemCount { get; init; }
    public int P0Count { get; init; }
    public int P1Count { get; init; }
    public int P2Count { get; init; }
    public int P3Count { get; init; }
}

public sealed record EditDrivenSpineQualityConsolidationNegativeProofIndex
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_negative_proof_index_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationNegativeProofScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationNegativeProofScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public int DiagnosticCount { get; init; }
}

public sealed record EditDrivenSpineQualityConsolidationWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_workspace_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPageDashboardTabDeclared { get; init; }
    public bool ParentPageDashboardEvidenceServiceLoaded { get; init; }
    public bool ParentPageDashboardControlBound { get; init; }
    public bool ParentPageActivationBindsGoal079Data { get; init; }
    public bool AllFiveChildSurfacesBound { get; init; }
    public bool AllChildSurfacesSeparateUserControls { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationWorkspaceSurface> Surfaces { get; init; } = [];
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationWorkspaceSurface
{
    public string SurfaceId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool TabDeclared { get; init; }
    public bool ServiceBuiltDuringActivation { get; init; }
    public bool BoundByParent { get; init; }
    public bool SeparateUserControl { get; init; }
}

public sealed record EditDrivenSpineQualityConsolidationSourceHealthScan
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_source_health_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int ParentWorkspaceLineCount { get; init; }
    public bool ParentWorkspaceWithinLimit { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationSourceFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationSourceFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LogicalLineCount { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int LfByteCount { get; init; }
    public int CrByteCount { get; init; }
    public int RawPhysicalLineCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public bool ZeroLfWithCr { get; init; }
    public bool ContainsCrOnlyLineEndings { get; init; }
    public bool RawPhysicalOneLineSourceCandidate { get; init; }
    public bool FileOver1000Lines { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record EditDrivenSpineQualityConsolidationDebtClassification
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_debt_classification_v1";
    public int P0Count { get; init; }
    public int P1Count { get; init; }
    public int P2Count { get; init; }
    public int P3Count { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDebtItem> Debts { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationDebtItem
{
    public string FindingId { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Area { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
    public string Disposition { get; init; } = string.Empty;
}

public sealed record EditDrivenSpineQualityConsolidationArtifactHygieneScan
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_artifact_hygiene_scan_v1";
    public bool Passed { get; init; }
    public int ArtifactCount { get; init; }
    public bool ContainsAbsoluteLocalPaths { get; init; }
    public bool ContainsTimestampLikeValues { get; init; }
    public bool ContainsHeavyLogs { get; init; }
    public bool ContainsScratchTamperFiles { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationQualityGateScan
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public bool RequiredArtifactsPresent { get; init; }
    public bool Goal078HandoffRecordedBeforeGoal079 { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceHealthPassed { get; init; }
    public bool ArtifactHygienePassed { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int P0Count { get; init; }
    public int P1Count { get; init; }
    public int P2Count { get; init; }
    public int P3Count { get; init; }
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationReport
{
    public string SchemaVersion { get; init; } = "edit_driven_spine_quality_report_v1";
    public string GoalId { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        EditDrivenSpineQualityConsolidationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenSpineQualityConsolidationVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal078AcceptedByUserHandoff { get; init; }
    public int ChainItemCount { get; init; }
    public int P0Count { get; init; }
    public int P1Count { get; init; }
    public int P2Count { get; init; }
    public int P3Count { get; init; }
    public int BlockerCount { get; init; }
    public int ParentWorkspaceLineCount { get; init; }
    public int MaxCSharpLineLength { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public string SourceArtifactManifestHash { get; init; } = string.Empty;
    public string SpineChainManifestHash { get; init; } = string.Empty;
    public string AcceptanceReadinessDashboardHash { get; init; } = string.Empty;
    public string NegativeProofIndexHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceHealthScanHash { get; init; } = string.Empty;
    public string QualityDebtClassificationHash { get; init; } = string.Empty;
    public string ArtifactHygieneScanHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenSpineQualityConsolidationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenSpineQualityConsolidationBuildResult
{
    public EditDrivenSpineQualityConsolidationSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenSpineQualityConsolidationChainManifest SpineChainManifest { get; init; } = new();
    public EditDrivenSpineQualityConsolidationReadinessDashboard AcceptanceReadinessDashboard { get; init; } = new();
    public EditDrivenSpineQualityConsolidationNegativeProofIndex NegativeProofIndex { get; init; } = new();
    public EditDrivenSpineQualityConsolidationWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public EditDrivenSpineQualityConsolidationSourceHealthScan SourceHealthScan { get; init; } = new();
    public EditDrivenSpineQualityConsolidationDebtClassification QualityDebtClassification { get; init; } = new();
    public EditDrivenSpineQualityConsolidationArtifactHygieneScan ArtifactHygieneScan { get; init; } = new();
    public EditDrivenSpineQualityConsolidationQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenSpineQualityConsolidationReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record EditDrivenSpineQualityConsolidationWriteResult
{
    public EditDrivenSpineQualityConsolidationBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
