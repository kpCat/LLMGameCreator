using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

public static class OfflineGeoworldObjectiveAcceptanceRunVocabulary
{
    public const string GoalId = "goal_107_offline_geoworld_objective_acceptance_run";
    public const string ProductSmokeRoute = "goal-107-offline-geoworld-objective-acceptance-run";
    public const string FinalGate = "offline_geoworld_objective_acceptance_run_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/OfflineGeoworldGoal107";
    public const string SourceGoal106Root =
        ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay";

    public const string UnityObjectiveStateScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveState.cs";
    public const string UnityObjectiveTrackerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveTracker.cs";
    public const string UnityObjectiveAcceptanceControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveAcceptanceController.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldObjectiveAcceptanceWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ReportMarkdownFileName = "offline-geoworld-objective-report.md";
    public const string ManifestFileName = "offline-geoworld-objective-manifest.json";
    public const string ObjectivesFileName = "offline-geoworld-objectives.json";
    public const string AcceptanceRunFileName =
        "offline-geoworld-objective-acceptance-run.json";
    public const string CompletionStateFileName =
        "offline-geoworld-objective-completion-state.json";
    public const string ReplayAcceptanceProofFileName =
        "offline-geoworld-objective-replay-acceptance-proof.json";
    public const string ReadmeFileName = "offline-geoworld-objective-readme.json";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-objective-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-objective-editor-window-inventory.json";
    public const string SimulatedAcceptanceProofFileName =
        "offline-geoworld-objective-simulated-acceptance-proof.json";
    public const string NegativeProofFileName = "offline-geoworld-objective-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-objective-workspace-binding-inventory.json";
    public const string SourceLineageFileName = "offline-geoworld-objective-source-lineage.json";
    public const string AlphaQualityConsolidationFileName =
        "offline-geoworld-objective-alpha-quality-consolidation.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-objective-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        ManifestFileName,
        ObjectivesFileName,
        AcceptanceRunFileName,
        CompletionStateFileName,
        ReplayAcceptanceProofFileName,
        ReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ManifestFileName,
        ObjectivesFileName,
        AcceptanceRunFileName,
        CompletionStateFileName,
        ReplayAcceptanceProofFileName,
        ReadmeFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedAcceptanceProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        AlphaQualityConsolidationFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal106_payload",
        "unknown_action_ref",
        "unknown_target_ref",
        "unknown_delta_ref",
        "prerequisite_bypass",
        "completion_without_required_state_delta",
        "save_load_without_checkpoint",
        "replay_mismatch",
        "fake_success_without_file_reads",
        "absolute_path",
        "raw_geodata_leak",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_settings_mutation_marker",
        "binary_raster_media_marker",
        "external_dependency_new_input_system_marker"
    ];
}

public sealed record OfflineGeoworldObjectiveDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldObjectiveDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldObjectiveManifest
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public int SourceGoal106ReplayStepCount { get; init; }
    public int SourceGoal106StateDeltaCount { get; init; }
    public int SourceGoal106CheckpointStepIndex { get; init; }
    public string SourceGoal106InitialStateHash { get; init; } = string.Empty;
    public string SourceGoal106CheckpointStateHash { get; init; } = string.Empty;
    public string SourceGoal106FinalStateHash { get; init; } = string.Empty;
    public string ObjectiveAcceptanceHash { get; init; } = string.Empty;
    public string CompletionStateHash { get; init; } = string.Empty;
    public string FinalStatus { get; init; } = "completed";
    public string StreamingAssetsRelativeRoot { get; init; } =
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityStreamingAssetsProbeRoot;
    public bool MetadataOnly { get; init; } = true;
    public bool AlphaToolingOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalGameplay { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string ObjectivesHash { get; init; } = string.Empty;
    public string AcceptanceRunHash { get; init; } = string.Empty;
    public string CompletionStateFileHash { get; init; } = string.Empty;
    public string ReplayAcceptanceProofHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldObjectiveDocument
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ObjectiveCount { get; init; }
    public IReadOnlyList<OfflineGeoworldObjectiveDefinition> Objectives { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveDefinition
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int SequenceIndex { get; init; }
    public IReadOnlyList<string> PrerequisiteObjectiveIds { get; init; } = [];
    public IReadOnlyList<string> LinkedActionIds { get; init; } = [];
    public IReadOnlyList<string> LinkedTargetIds { get; init; } = [];
    public IReadOnlyList<string> LinkedEventIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedStateDeltaKeys { get; init; } = [];
    public IReadOnlyList<string> ExpectedStateDeltaKinds { get; init; } = [];
    public IReadOnlyList<string> VisibleDiagnostics { get; init; } = [];
    public string CompletionCondition { get; init; } = string.Empty;
    public string CompletionState { get; init; } = "completed";
    public bool RequiresCheckpoint { get; init; }
    public int RequiredCheckpointStepIndex { get; init; }
    public string ExpectedStateHashAfter { get; init; } = string.Empty;
    public string DeterministicHashContribution { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldObjectiveAcceptanceRun
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Accepted { get; init; }
    public string SourceGoalId { get; init; } = "goal_106_offline_geoworld_session_persistence_replay";
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public bool SaveLoadResumeRequired { get; init; } = true;
    public string InitialStateHash { get; init; } = string.Empty;
    public string CheckpointStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalObjectiveId { get; init; } = string.Empty;
    public IReadOnlyList<OfflineGeoworldObjectiveRunStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveRunStep
{
    public int StepIndex { get; init; }
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public IReadOnlyList<string> AppliedReplayEventIds { get; init; } = [];
    public IReadOnlyList<string> AppliedActionIds { get; init; } = [];
    public string StateHashBefore { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
    public bool CheckpointLoadedBeforeCompletion { get; init; }
    public string CompletionHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldObjectiveCompletionState
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Completed { get; init; }
    public string FinalStatus { get; init; } = "completed";
    public int CompletedObjectiveCount { get; init; }
    public string CurrentObjectiveId { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedObjectiveIds { get; init; } = [];
    public IReadOnlyList<string> ObjectiveHashChain { get; init; } = [];
    public string FinalObjectiveAcceptanceHash { get; init; } = string.Empty;
    public bool ReplayLinked { get; init; }
    public bool SaveLoadResumeLinked { get; init; }
}

public sealed record OfflineGeoworldObjectiveReplayAcceptanceProof
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool ObjectivesRead { get; init; }
    public bool AcceptanceRunRead { get; init; }
    public bool CompletionStateRead { get; init; }
    public bool SourceGoal106PayloadRead { get; init; }
    public bool SourceGoal106ReplayProofRead { get; init; }
    public bool SourceGoal106ReplayHashChainPassed { get; init; }
    public bool CheckpointResumeApplied { get; init; }
    public bool ObjectivePrerequisitesPassed { get; init; }
    public bool CompletionTransitionsPassed { get; init; }
    public bool StateDeltaLinkagePassed { get; init; }
    public bool DeterministicHashChainPassed { get; init; }
    public bool FailedPrerequisiteRejected { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string FinalObjectiveAcceptanceHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ObjectiveHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveReadme
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool AlphaToolingOnly { get; init; } = true;
    public bool ImplementsFinalRuntimeQuestSystem { get; init; }
    public bool UsesRealGeodata { get; init; }
    public bool UsesProviderOrNetworkCalls { get; init; }
    public string ScopeSummary { get; init; } =
        "Unity Alpha objective tracker payload over Goal106 offline geoworld replay/save-load metadata.";
}

public sealed record OfflineGeoworldObjectiveSourceFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int MaxLineLength { get; init; }
    public bool NotMinified { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
}

public sealed record OfflineGeoworldObjectiveUnityScriptInventory
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScannedUnitySourceFileCount { get; init; }
    public bool ObjectiveStateExists { get; init; }
    public bool ObjectiveTrackerExists { get; init; }
    public bool ObjectiveAcceptanceControllerExists { get; init; }
    public bool ReadsApplicationStreamingAssetsPath { get; init; }
    public bool ReadsGoal107Root { get; init; }
    public bool IntegratesGoal105InteractionController { get; init; }
    public bool IntegratesGoal106ReplayAndSaveLoadControllers { get; init; }
    public bool SupportsManualAdvanceAndReplayChecks { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldObjectiveSourceFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveEditorWindowInventory
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal107PayloadPathMarkerPresent { get; init; }
    public bool CreateRigMethodPresent { get; init; }
    public bool ClearRigMethodPresent { get; init; }
    public bool AcceptanceInstructionsPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldObjectiveSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldObjectiveNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveWorkspaceBindingInventory
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesObjectiveAcceptanceGroup { get; init; }
    public bool WorkspaceReadsGoal107EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysObjectiveFields { get; init; }
    public bool ShowsObjectiveCount { get; init; }
    public bool ShowsCompletedObjectiveCount { get; init; }
    public bool ShowsFinalStatus { get; init; }
    public bool ShowsReplaySaveLoadLinkage { get; init; }
    public bool ShowsUnityScriptReadiness { get; init; }
    public bool ShowsEditorHelperReadiness { get; init; }
    public bool ShowsAlphaQualityConsolidationStatus { get; init; }
    public bool ShowsManualChecklistSummary { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldObjectiveSourceLineage
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal106AcceptedFalsePreserved { get; init; }
    public bool Goal106PayloadConsumed { get; init; }
    public bool Goal106UnityEvidenceConsumed { get; init; }
    public IReadOnlyList<OfflineGeoworldObjectiveSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveAlphaQualityConsolidation
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool TravelPreviewReady { get; init; }
    public bool EditorPreviewReady { get; init; }
    public bool PlayModeTravelReady { get; init; }
    public bool InteractiveTravelReady { get; init; }
    public bool InteractionProbeReady { get; init; }
    public bool SessionReplayReady { get; init; }
    public bool ObjectiveAcceptanceRunReady { get; init; }
    public bool ManualAcceptanceChecklistReady { get; init; }
    public bool SourceReadableNotMinified { get; init; }
    public bool NoNetworkProviderLlmMarkers { get; init; }
    public bool NoAlphaRuntimeBootstrapDependency { get; init; }
    public bool NoExternalPackageOrNewInputSystemMarkers { get; init; }
    public bool NoScenePrefabSettingsBuildPackageMutation { get; init; }
    public bool NoBinaryRasterMedia { get; init; }
    public int ScannedUnitySourceFileCount { get; init; }
    public int MaxUnitySourceLineCount { get; init; }
    public IReadOnlyList<string> RemainingNotFinalWarnings { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal106Consumed { get; init; }
    public bool ObjectivePayloadCreated { get; init; }
    public bool ReplayAcceptanceProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool AlphaQualityConsolidationPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoRawGeodataDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoScenePrefabSettingsChanges { get; init; }
    public bool NoExternalDependenciesOrNewInputSystem { get; init; }
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldObjectiveReport
{
    public string GoalId { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldObjectiveAcceptanceRunVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool ReplayAcceptanceProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaQualityConsolidationPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldObjectiveBuildResult
{
    public OfflineGeoworldObjectiveManifest Manifest { get; init; } = new();
    public OfflineGeoworldObjectiveDocument Objectives { get; init; } = new();
    public OfflineGeoworldObjectiveAcceptanceRun AcceptanceRun { get; init; } = new();
    public OfflineGeoworldObjectiveCompletionState CompletionState { get; init; } = new();
    public OfflineGeoworldObjectiveReplayAcceptanceProof ReplayAcceptanceProof { get; init; } = new();
    public OfflineGeoworldObjectiveReadme Readme { get; init; } = new();
    public OfflineGeoworldObjectiveUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldObjectiveEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldObjectiveReplayAcceptanceProof SimulatedAcceptanceProof { get; init; } = new();
    public OfflineGeoworldObjectiveNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldObjectiveWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldObjectiveSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldObjectiveAlphaQualityConsolidation AlphaQualityConsolidation { get; init; } = new();
    public OfflineGeoworldObjectiveQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldObjectiveReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldObjectiveWriteResult
{
    public OfflineGeoworldObjectiveBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldObjectiveJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class OfflineGeoworldObjectiveHash
{
    public static string Sha256Text(string text) => Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) => Sha256Bytes(File.ReadAllBytes(path));
}
