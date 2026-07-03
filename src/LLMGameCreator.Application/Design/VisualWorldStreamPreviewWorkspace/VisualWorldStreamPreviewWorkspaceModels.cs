using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public static class VisualWorldStreamPreviewWorkspaceVocabulary
{
    public const string GoalId = "goal_096_unity_handoff_inspector_probe_readiness";
    public const string ProductSmokeRoute = "goal-096-unity-handoff-inspector-probe-readiness";
    public const string FinalGate = "unity_handoff_inspector_probe_readiness_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness";

    public const string CatalogSchemaVersion = "unity_handoff_inspector_catalog_v1";
    public const string ProofStatusSchemaVersion =
        "unity_handoff_inspector_proof_status_v1";
    public const string WinFormsBindingSchemaVersion =
        "unity_handoff_inspector_winforms_binding_inventory_v1";
    public const string QualityGateSchemaVersion =
        "unity_handoff_inspector_quality_gate_scan_v1";
    public const string SourceHealthSchemaVersion =
        "unity_handoff_inspector_source_health_scan_v1";
}

public static class VisualWorldPreviewServiceSplitSourceHealthVocabulary
{
    public const string GoalId = "goal_092a_visual_world_preview_service_split_source_health";
    public const string ProductSmokeRoute =
        "goal-092a-visual-world-preview-service-split-source-health";
    public const string FinalGate =
        "visual_world_preview_service_split_source_health_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health";

    public const string BeforeAfterSchemaVersion =
        "visual_world_preview_service_split_source_health_before_after_v1";
    public const string RefactorInventorySchemaVersion =
        "visual_world_preview_service_split_refactor_inventory_v1";
    public const string BehaviorEquivalenceSchemaVersion =
        "visual_world_preview_service_split_behavior_equivalence_v1";
    public const string QualityGateSchemaVersion =
        "visual_world_preview_service_split_quality_gate_scan_v1";
}

public enum VisualWorldPreviewArtifactStatus
{
    Unknown = 0,
    Passed,
    Failed
}

public sealed record VisualWorldPreviewDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualWorldPreviewDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static VisualWorldPreviewDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record VisualWorldPreviewArtifactGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string SourceRootRelativePath { get; init; } = string.Empty;
    public VisualWorldPreviewArtifactStatus Status { get; init; }
    public int EntryCount { get; init; }
    public int SvgEntryCount { get; init; }
    public IReadOnlyList<VisualWorldPreviewArtifactEntry> Entries { get; init; } = [];
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldPreviewArtifactEntry
{
    public string Id { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public VisualWorldPreviewArtifactStatus Status { get; init; }
    public string DiagnosticSummary { get; init; } = string.Empty;
    public string TextSvgPreviewPath { get; init; } = string.Empty;
    public string SafeRatingMetadataSummary { get; init; } = string.Empty;
    public string ExportTargetKind { get; init; } = string.Empty;
    public int CacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowCount { get; init; }
    public bool RuntimeHandoffMetadataOnly { get; init; }
    public bool InvalidationMatrixPassed { get; init; }
    public bool ReadbackProofPassed { get; init; }
    public bool OverlapReuseProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public int PayloadFileCount { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public bool SimulatedUnityReadProofPassed { get; init; }
    public bool ProbeSourceInventoryPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool ForbiddenUnityAreasUnchanged { get; init; }
    public bool MetadataOnly { get; init; }
    public bool PayloadHashesMatchGoal095Ledger { get; init; }
    public bool NoUnityFilesChangedByGoal096 { get; init; }
    public IReadOnlyList<string> ChunkKeys { get; init; } = [];

    [JsonIgnore]
    public string TextPreview { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewSvgEntry
{
    public string EntryId { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
    public bool SafeToDisplayAsText { get; init; }
    public string SafetySummary { get; init; } = string.Empty;

    [JsonIgnore]
    public string PreviewText { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewProofStatus
{
    public string ProofId { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public VisualWorldPreviewArtifactStatus Status { get; init; }
    public bool Passed { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string DiagnosticSummary { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewSelection
{
    public string GroupId { get; init; } = string.Empty;
    public string EntryId { get; init; } = string.Empty;
    public string SvgRelativePath { get; init; } = string.Empty;
    public string DetailsText { get; init; } = string.Empty;
    public string SvgTextPreview { get; init; } = string.Empty;
    public string ProofSummary { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } =
        VisualWorldStreamPreviewWorkspaceVocabulary.WinFormsBindingSchemaVersion;

    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PageControlExists { get; init; }
    public bool DesignerExists { get; init; }
    public bool CompositionRootRegistersService { get; init; }
    public bool CompositionRootRegistersPage { get; init; }
    public bool EditorRegistryIncludesPage { get; init; }
    public bool PageActivationLoadsApplicationResult { get; init; }
    public bool PageBindDisplaysGroupsEntriesProofs { get; init; }
    public bool PageBindDisplaysCacheExports { get; init; }
    public bool PageBindDisplaysUnityHandoff { get; init; }
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldPreviewWorkspaceQualityGate
{
    public string SchemaVersion { get; init; } =
        VisualWorldStreamPreviewWorkspaceVocabulary.QualityGateSchemaVersion;

    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int GroupCount { get; init; }
    public int EntryCount { get; init; }
    public int SvgTextPreviewCount { get; init; }
    public int Goal091StreamWindowEntryCount { get; init; }
    public bool CacheExportGroupPresent { get; init; }
    public int CacheExportPackageCount { get; init; }
    public int CacheExportRecordCount { get; init; }
    public int CacheExportSourceChunkCount { get; init; }
    public int CacheExportStreamWindowCount { get; init; }
    public bool RuntimeHandoffSidecarVisible { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool CacheReadbackProofPassed { get; init; }
    public bool CacheOverlapReuseProofPassed { get; init; }
    public bool CacheNegativeProofPassed { get; init; }
    public bool CacheInvalidationMatrixPassed { get; init; }
    public bool CacheNoRawFullWorldDump { get; init; }
    public bool Goal093FilesDiscoveredByRelativePaths { get; init; }
    public bool UnityHandoffGroupPresent { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public int UnityPackageCount { get; init; }
    public int UnityExportRecordCount { get; init; }
    public int UnityStreamWindowCount { get; init; }
    public int UnityUniqueChunkKeyCount { get; init; }
    public bool UnityProbeSourceInventoryVisible { get; init; }
    public bool UnityProbeSourceInventoryPassed { get; init; }
    public bool UnitySimulatedReadProofPassed { get; init; }
    public bool UnityNegativeProofPassed { get; init; }
    public bool UnityAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool UnityForbiddenAreasUnchanged { get; init; }
    public bool UnityHandoffMetadataOnly { get; init; }
    public bool UnityPayloadHashesMatchGoal095Ledger { get; init; }
    public bool Goal095FilesDiscoveredByRelativePaths { get; init; }
    public bool NoUnityFilesChangedByGoal096 { get; init; }
    public bool RequiredArtifactGroupsPresent { get; init; }
    public bool Goal091StreamWindowsVisible { get; init; }
    public bool ProofStatusPassed { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMediaAdded { get; init; }
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool WinFormsBindingReal { get; init; }
    public bool WinFormsCacheExportBindingReal { get; init; }
    public bool WinFormsUnityHandoffBindingReal { get; init; }
    public bool SourceHealthPassed { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int MaxPhysicalLineLength { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public int FilesOver700LogicalLinesInGoal092NamespaceCount { get; init; }
    public int ZeroLfSourceCount { get; init; }
    public int CrOnlySourceCount { get; init; }
    public int RawPhysicalOneLineSourceCount { get; init; }
    public int MinifiedSourceCount { get; init; }
    public int WorkspaceServiceLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldStreamPreviewCatalog
{
    public string SchemaVersion { get; init; } =
        VisualWorldStreamPreviewWorkspaceVocabulary.CatalogSchemaVersion;

    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int GroupCount { get; init; }
    public int EntryCount { get; init; }
    public int SvgTextPreviewCount { get; init; }
    public IReadOnlyList<VisualWorldPreviewArtifactGroup> Groups { get; init; } = [];
    public IReadOnlyList<VisualWorldPreviewSvgEntry> SvgEntries { get; init; } = [];
}

public sealed record VisualWorldStreamPreviewProofStatusDocument
{
    public string SchemaVersion { get; init; } =
        VisualWorldStreamPreviewWorkspaceVocabulary.ProofStatusSchemaVersion;

    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ProofCount { get; init; }
    public IReadOnlyList<VisualWorldPreviewProofStatus> Proofs { get; init; } = [];
}

public sealed record VisualWorldStreamPreviewWorkspaceReport
{
    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int GroupCount { get; init; }
    public int EntryCount { get; init; }
    public int SvgTextPreviewCount { get; init; }
    public int Goal091StreamWindowEntryCount { get; init; }
    public int CacheExportPackageCount { get; init; }
    public int CacheExportRecordCount { get; init; }
    public int CacheExportSourceChunkCount { get; init; }
    public int CacheExportStreamWindowCount { get; init; }
    public bool RuntimeHandoffSidecarVisible { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool CacheReadbackProofPassed { get; init; }
    public bool CacheOverlapReuseProofPassed { get; init; }
    public bool CacheNegativeProofPassed { get; init; }
    public bool CacheInvalidationMatrixPassed { get; init; }
    public bool CacheNoRawFullWorldDump { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public int UnityPackageCount { get; init; }
    public int UnityExportRecordCount { get; init; }
    public int UnityStreamWindowCount { get; init; }
    public int UnityUniqueChunkKeyCount { get; init; }
    public bool UnityProbeSourceInventoryVisible { get; init; }
    public bool UnityProbeSourceInventoryPassed { get; init; }
    public bool UnitySimulatedReadProofPassed { get; init; }
    public bool UnityNegativeProofPassed { get; init; }
    public bool UnityAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool UnityForbiddenAreasUnchanged { get; init; }
    public bool UnityHandoffMetadataOnly { get; init; }
    public bool UnityPayloadHashesMatchGoal095Ledger { get; init; }
    public bool Goal095FilesDiscoveredByRelativePaths { get; init; }
    public bool NoUnityFilesChangedByGoal096 { get; init; }
    public bool ProofStatusPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool SourceHealthPassed { get; init; }
    public int WorkspaceServiceLogicalLineCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public int FilesOver700LogicalLinesInGoal092NamespaceCount { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string ProofStatusHash { get; init; } = string.Empty;
    public string WinFormsBindingInventoryHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualWorldStreamPreviewWorkspaceResult
{
    public VisualWorldStreamPreviewCatalog Catalog { get; init; } = new();
    public VisualWorldStreamPreviewProofStatusDocument ProofStatus { get; init; } = new();
    public VisualWorldPreviewWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public VisualWorldPreviewWorkspaceQualityGate QualityGateScan { get; init; } = new();
    public VisualWorldStreamPreviewSourceHealthScan SourceHealthScan { get; init; } = new();
    public VisualWorldStreamPreviewWorkspaceReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string ProofStatusJson { get; init; } = string.Empty;
    public string WinFormsBindingInventoryJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string SourceHealthScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldStreamPreviewWorkspaceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string ProofStatusJsonPath { get; init; } = string.Empty;
    public string WinFormsBindingInventoryJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public string SourceHealthScanJsonPath { get; init; } = string.Empty;
    public VisualWorldStreamPreviewWorkspaceResult Result { get; init; } = new();
}

public sealed record VisualWorldStreamPreviewSourceFileHealth
{
    public string RelativePath { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public int LogicalLineCount { get; init; }
    public int LogicalMaxLineLength { get; init; }
    public int LfByteCount { get; init; }
    public int CrByteCount { get; init; }
    public int RawPhysicalLineCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public bool ZeroLfSource { get; init; }
    public bool CrOnlySource { get; init; }
    public bool ContainsCrOnlyLineEndings { get; init; }
    public bool RawPhysicalOneLineSource { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
    public bool FileOver1000LogicalLines { get; init; }
    public bool FileOver700LogicalLines { get; init; }
}

public sealed record VisualWorldStreamPreviewSourceHealthScan
{
    public string SchemaVersion { get; init; } =
        VisualWorldStreamPreviewWorkspaceVocabulary.SourceHealthSchemaVersion;

    public string GoalId { get; init; } = VisualWorldStreamPreviewWorkspaceVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int MaxPhysicalLineLength { get; init; }
    public int RawPhysicalLinesOver500Count { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public int FilesOver700LogicalLinesInGoal092NamespaceCount { get; init; }
    public int ZeroLfSourceCount { get; init; }
    public int CrOnlySourceCount { get; init; }
    public int RawPhysicalOneLineSourceCount { get; init; }
    public int MinifiedSourceCount { get; init; }
    public int WorkspaceServiceLogicalLineCount { get; init; }
    public int WorkspaceServiceMaxPhysicalLineLength { get; init; }
    public IReadOnlyList<VisualWorldStreamPreviewSourceFileHealth> Files { get; init; } = [];
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldPreviewServiceSplitSourceHealthSnapshot
{
    public string Source { get; init; } = string.Empty;
    public string WorkspaceServiceRelativePath { get; init; } = string.Empty;
    public int ScannedCSharpFileCount { get; init; }
    public int WorkspaceServiceLogicalLineCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int MaxPhysicalLineLength { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public int FilesOver700LogicalLinesInGoal092NamespaceCount { get; init; }
    public int ZeroLfSourceCount { get; init; }
    public int CrOnlySourceCount { get; init; }
    public int RawPhysicalOneLineSourceCount { get; init; }
    public int MinifiedSourceCount { get; init; }
    public bool OversizedWorkspaceServiceDetected { get; init; }
}

public sealed record VisualWorldPreviewServiceSplitSourceHealthBeforeAfter
{
    public string SchemaVersion { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.BeforeAfterSchemaVersion;

    public string GoalId { get; init; } = VisualWorldPreviewServiceSplitSourceHealthVocabulary.GoalId;
    public string ManualGate { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public VisualWorldPreviewServiceSplitSourceHealthSnapshot Before { get; init; } = new();
    public VisualWorldStreamPreviewSourceHealthScan After { get; init; } = new();
}

public sealed record VisualWorldPreviewRefactorInventoryFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Responsibility { get; init; } = string.Empty;
    public int LogicalLineCount { get; init; }
    public int MaxPhysicalLineLength { get; init; }
}

public sealed record VisualWorldPreviewRefactorFileInventory
{
    public string SchemaVersion { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.RefactorInventorySchemaVersion;

    public string GoalId { get; init; } = VisualWorldPreviewServiceSplitSourceHealthVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int WorkspaceServiceLogicalLineCount { get; init; }
    public IReadOnlyList<VisualWorldPreviewRefactorInventoryFile> Files { get; init; } = [];
}

public sealed record VisualWorldPreviewBehaviorEquivalenceProof
{
    public string SchemaVersion { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.BehaviorEquivalenceSchemaVersion;

    public string GoalId { get; init; } = VisualWorldPreviewServiceSplitSourceHealthVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ArtifactGroupCount { get; init; }
    public int EntryCount { get; init; }
    public int SvgTextPreviewCount { get; init; }
    public int Goal091StreamWindowEntryCount { get; init; }
    public int ProofStatusCount { get; init; }
    public bool RequiredArtifactGroupsPresent { get; init; }
    public bool Goal091StreamWindowsVisible { get; init; }
    public bool ProofStatusPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMediaAdded { get; init; }
}

public sealed record VisualWorldPreviewServiceSplitQualityGateScan
{
    public string SchemaVersion { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.QualityGateSchemaVersion;

    public string GoalId { get; init; } = VisualWorldPreviewServiceSplitSourceHealthVocabulary.GoalId;
    public string ManualGate { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool BeforeOversizedServiceDetected { get; init; }
    public bool AfterNoFilesOver1000LogicalLines { get; init; }
    public bool AfterNoFilesOver700LogicalLines { get; init; }
    public bool WorkspaceServiceBelow700Lines { get; init; }
    public bool Goal092QualityGateCarriesSourceHealthMetrics { get; init; }
    public bool BehaviorEquivalencePassed { get; init; }
    public bool RefactorInventoryPassed { get; init; }
    public bool NoForbiddenAreasRequired { get; init; } = true;
    public bool NoBinaryMediaArtifacts { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCountAfterRepair { get; init; }
    public int WorkspaceServiceLogicalLineCountBeforeRepair { get; init; }
    public int WorkspaceServiceLogicalLineCountAfterRepair { get; init; }
    public IReadOnlyList<VisualWorldPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldPreviewServiceSplitReport
{
    public string GoalId { get; init; } = VisualWorldPreviewServiceSplitSourceHealthVocabulary.GoalId;
    public string ManualGate { get; init; } =
        VisualWorldPreviewServiceSplitSourceHealthVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool BehaviorEquivalencePassed { get; init; }
    public bool SourceHealthPassed { get; init; }
    public int WorkspaceServiceLogicalLineCountBeforeRepair { get; init; }
    public int WorkspaceServiceLogicalLineCountAfterRepair { get; init; }
    public int MaxLogicalLineCountAfterRepair { get; init; }
    public string SourceHealthBeforeAfterHash { get; init; } = string.Empty;
    public string RefactorInventoryHash { get; init; } = string.Empty;
    public string BehaviorEquivalenceProofHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewServiceSplitSourceHealthBuildResult
{
    public VisualWorldPreviewServiceSplitSourceHealthBeforeAfter SourceHealthBeforeAfter { get; init; } = new();
    public VisualWorldPreviewRefactorFileInventory RefactorFileInventory { get; init; } = new();
    public VisualWorldPreviewBehaviorEquivalenceProof BehaviorEquivalenceProof { get; init; } = new();
    public VisualWorldPreviewServiceSplitQualityGateScan QualityGateScan { get; init; } = new();
    public VisualWorldPreviewServiceSplitReport Report { get; init; } = new();
    public string SourceHealthBeforeAfterJson { get; init; } = string.Empty;
    public string RefactorFileInventoryJson { get; init; } = string.Empty;
    public string BehaviorEquivalenceProofJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record VisualWorldPreviewServiceSplitSourceHealthWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string SourceHealthBeforeAfterJsonPath { get; init; } = string.Empty;
    public string RefactorFileInventoryJsonPath { get; init; } = string.Empty;
    public string BehaviorEquivalenceProofJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public VisualWorldPreviewServiceSplitSourceHealthBuildResult Result { get; init; } = new();
}
