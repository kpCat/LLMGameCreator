using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public static class OfflineGeoworldUnityEditorPreviewToolVocabulary
{
    public const string GoalId = "goal_102_offline_geoworld_unity_editor_preview_tool";
    public const string ProductSmokeRoute =
        "goal-102-offline-geoworld-unity-editor-preview-tool";
    public const string FinalGate =
        "offline_geoworld_unity_editor_preview_tool_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool";

    public const string Goal101SourceRoot =
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner";
    public const string Goal101StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101";
    public const string UnityStreamingAssetsProbeRoot =
        "LLMGameCreator/OfflineGeoworldGoal101";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ToolInventorySchemaVersion =
        "offline_geoworld_unity_editor_tool_inventory_v1";
    public const string SimulatedActionProofSchemaVersion =
        "offline_geoworld_unity_editor_simulated_action_proof_v1";
    public const string NegativeProofSchemaVersion =
        "offline_geoworld_unity_editor_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_unity_editor_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion =
        "offline_geoworld_unity_editor_source_lineage_v1";
    public const string QualityGateSchemaVersion =
        "offline_geoworld_unity_editor_quality_gate_scan_v1";

    public const string ReportMarkdownFileName =
        "offline-geoworld-unity-editor-preview-tool-report.md";
    public const string ToolInventoryFileName =
        "offline-geoworld-unity-editor-tool-inventory.json";
    public const string SimulatedActionProofFileName =
        "offline-geoworld-unity-editor-simulated-action-proof.json";
    public const string NegativeProofFileName =
        "offline-geoworld-unity-editor-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-unity-editor-workspace-binding-inventory.json";
    public const string SourceLineageFileName =
        "offline-geoworld-unity-editor-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-unity-editor-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ToolInventoryFileName,
        SimulatedActionProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal101_payload",
        "missing_editor_window_script",
        "missing_menu_marker",
        "missing_clear_method",
        "unsupported_command_kind",
        "network_provider_marker_in_editor_script",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_project_settings_change_marker",
        "fake_success_without_payload_read",
        "absolute_path_in_payload",
        "raw_geodata_leaked_into_command",
        "binary_raster_media_marker",
        "missing_create_method"
    ];
}

public sealed record OfflineGeoworldUnityEditorPreviewDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldUnityEditorPreviewDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldUnityEditorSourceFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
}

public sealed record OfflineGeoworldUnityEditorToolInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal101PayloadPathMarkerPresent { get; init; }
    public bool CreatePreviewObjectsMethodPresent { get; init; }
    public bool ClearPreviewObjectsMethodPresent { get; init; }
    public bool PayloadStatusUiPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldUnityEditorSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorPreviewObjectPlan
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandKind { get; init; } = string.Empty;
    public string ObjectKind { get; init; } = string.Empty;
    public string ObjectName { get; init; } = string.Empty;
    public int ExpectedObjectCount { get; init; }
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldUnityEditorSimulatedActionProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool CommandCatalogRead { get; init; }
    public bool TravelWindowScriptRead { get; init; }
    public bool EditorWindowScriptRead { get; init; }
    public bool PayloadCountsMatchGoal101 { get; init; }
    public bool AllRequiredCommandKindsRepresented { get; init; }
    public bool NoUnsupportedCommandKind { get; init; }
    public bool PreviewObjectPlanBuilt { get; init; }
    public bool CreateOperationModelPassed { get; init; }
    public bool ClearOperationModelPassed { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public bool NoScenePrefabSettingsChangeMarkers { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public int ClearOperationRemovedObjectCount { get; init; }
    public IReadOnlyDictionary<string, int> CommandCountByKind { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewObjectPlan> PreviewObjects { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesEditorPreviewGroup { get; init; }
    public bool WorkspaceReadsGoal102EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysEditorPreviewFields { get; init; }
    public bool ShowsEditorWindowScriptPath { get; init; }
    public bool ShowsMenuItemMarker { get; init; }
    public bool ShowsGoal101PayloadPath { get; init; }
    public bool ShowsPreviewCommandCount { get; init; }
    public bool ShowsTravelWindowSteps { get; init; }
    public bool ShowsSimulatedEditorActionProof { get; init; }
    public bool ShowsClearCleanupProof { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public bool ShowsManualLaunchInstructions { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldUnityEditorSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal101AcceptedFalsePreserved { get; init; }
    public bool Goal101PayloadConsumed { get; init; }
    public bool Goal101SimulatedCommandProofPassed { get; init; }
    public bool Goal101NegativeProofPassed { get; init; }
    public bool Goal101AlphaRuntimeBootstrapUnchanged { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal101Consumed { get; init; }
    public bool EditorWindowScriptReady { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool Goal101PayloadPathMarkerPresent { get; init; }
    public bool CreatePreviewObjectsMethodPresent { get; init; }
    public bool ClearPreviewObjectsMethodPresent { get; init; }
    public bool SimulatedActionProofPassed { get; init; }
    public bool ClearOperationProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoRawGeodataDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoScenePrefabSettingsChanges { get; init; }
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public string EditorWindowScriptPath { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath;
    public string Goal101PayloadPath { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityStreamingAssetsProbeRoot;
    public string MenuItemMarker { get; init; } = "LLMGameCreator/Offline Geoworld Preview";
    public string ManualInstructions { get; init; } =
        "Open Unity Editor, use LLMGameCreator/Offline Geoworld Preview, then Refresh, Create Preview Objects and Clear Preview Objects.";
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityEditorPreviewToolVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public bool EditorWindowScriptReady { get; init; }
    public bool SimulatedActionProofPassed { get; init; }
    public bool ClearOperationProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string ToolInventoryHash { get; init; } = string.Empty;
    public string SimulatedActionProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldUnityEditorPreviewBuildResult
{
    public OfflineGeoworldUnityEditorToolInventory ToolInventory { get; init; } = new();
    public OfflineGeoworldUnityEditorSimulatedActionProof SimulatedActionProof { get; init; } = new();
    public OfflineGeoworldUnityEditorNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldUnityEditorWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldUnityEditorQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldUnityEditorReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldUnityEditorPreviewWriteResult
{
    public OfflineGeoworldUnityEditorPreviewBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldUnityEditorPreviewJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class OfflineGeoworldUnityEditorPreviewHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
