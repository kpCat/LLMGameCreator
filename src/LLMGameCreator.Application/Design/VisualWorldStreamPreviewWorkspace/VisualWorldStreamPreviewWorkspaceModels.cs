using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public static class VisualWorldStreamPreviewWorkspaceVocabulary
{
    public const string GoalId = "goal_092_visual_world_stream_preview_workspace";
    public const string ProductSmokeRoute = "goal-092-visual-world-stream-preview-workspace";
    public const string FinalGate = "visual_world_stream_preview_workspace_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-092-visual-world-stream-preview-workspace";

    public const string CatalogSchemaVersion = "visual_world_stream_preview_catalog_v1";
    public const string ProofStatusSchemaVersion = "visual_world_stream_preview_proof_status_v1";
    public const string WinFormsBindingSchemaVersion =
        "visual_world_stream_preview_winforms_binding_inventory_v1";
    public const string QualityGateSchemaVersion =
        "visual_world_stream_preview_quality_gate_scan_v1";
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
    public bool RequiredArtifactGroupsPresent { get; init; }
    public bool Goal091StreamWindowsVisible { get; init; }
    public bool ProofStatusPassed { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMediaAdded { get; init; }
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool WinFormsBindingReal { get; init; }
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
    public bool ProofStatusPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
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
    public VisualWorldStreamPreviewWorkspaceReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string ProofStatusJson { get; init; } = string.Empty;
    public string WinFormsBindingInventoryJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
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
    public VisualWorldStreamPreviewWorkspaceResult Result { get; init; } = new();
}
