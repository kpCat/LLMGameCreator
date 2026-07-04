namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

public static class OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary
{
    public const string GoalId = "goal_110_offline_geoworld_alpha_manual_acceptance_gate";
    public const string ProductSmokeRoute = "goal-110-offline-geoworld-alpha-manual-acceptance-gate";
    public const string FinalGate = "offline_geoworld_alpha_manual_acceptance_verification";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-110-offline-geoworld-alpha-acceptance";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110";
    public const string Goal109ExportPackageDirectory =
        ".llmgc/exports/goal-109-offline-geoworld-alpha-slice";
    public const string Goal109ProceduralOutputDirectory =
        ".llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestFileName = "offline-geoworld-alpha-acceptance-manifest.json";
    public const string ChecklistFileName = "offline-geoworld-alpha-acceptance-checklist.json";
    public const string ResultTemplateFileName =
        "offline-geoworld-alpha-acceptance-result-template.json";
    public const string DashboardFileName =
        "offline-geoworld-alpha-release-gate-dashboard.json";
    public const string ReadmeFileName = "offline-geoworld-alpha-acceptance-readme.md";
    public const string FileIndexFileName = "offline-geoworld-alpha-acceptance-file-index.json";
    public const string ChecksumsFileName = "offline-geoworld-alpha-acceptance-checksums.json";
    public const string ReportFileName = "offline-geoworld-alpha-acceptance-report.md";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-alpha-acceptance-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-alpha-acceptance-editor-window-inventory.json";
    public const string SimulatedProofFileName =
        "offline-geoworld-alpha-acceptance-simulated-proof.json";
    public const string SimulatedResultFileName =
        "offline-geoworld-alpha-acceptance-simulated-result.json";
    public const string NegativeProofFileName =
        "offline-geoworld-alpha-acceptance-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-alpha-acceptance-workspace-binding-inventory.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-acceptance-quality-gate-scan.json";

    public const string UnityResultScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs";
    public const string UnityResultStoreScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs";

    public static IReadOnlyList<string> RequiredPayloadFileNames =>
    [
        ManifestFileName,
        ChecklistFileName,
        ResultTemplateFileName,
        DashboardFileName,
        ReadmeFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        ManifestFileName,
        ChecklistFileName,
        ResultTemplateFileName,
        DashboardFileName,
        ReadmeFileName,
        FileIndexFileName,
        ChecksumsFileName
    ];

    public static IReadOnlyList<string> IndexedExportFileNames =>
    [
        ManifestFileName,
        ChecklistFileName,
        ResultTemplateFileName,
        DashboardFileName,
        ReadmeFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredEvidenceFileNames =>
    [
        ReportFileName,
        ManifestFileName,
        ChecklistFileName,
        ResultTemplateFileName,
        DashboardFileName,
        FileIndexFileName,
        ChecksumsFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        QualityGateScanFileName
    ];

    public static IReadOnlyList<string> RequiredChecklistStepIds =>
    [
        "open_unity_project",
        "open_alpha_slice_window",
        "setup_rig",
        "verify_package",
        "run_travel",
        "run_interaction",
        "save_snapshot",
        "load_snapshot",
        "replay",
        "complete_objectives",
        "run_package_verifier",
        "record_diagnostics"
    ];

    public static IReadOnlyList<string> RequiredNegativeScenarioIds =>
    [
        "missing_goal109_package",
        "missing_checklist_step",
        "accepted_true_without_manual_result",
        "fake_manual_result_without_file_read",
        "tampered_result_hash",
        "absolute_path_in_payload",
        "raw_geodata_leak",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency",
        "scene_prefab_settings_mutation_marker",
        "binary_raster_media_marker",
        "external_dependency_new_input_system_marker",
        "historical_goal101_to_109_artifact_rewrite"
    ];

    public static IReadOnlyList<string> SourceHealthFiles =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/OfflineGeoworldAlphaSliceManualAcceptanceGateModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService.Readme.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService.Quality.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService.Utilities.cs",
        UnityResultScriptPath,
        UnityResultStoreScriptPath,
        UnityEditorWindowScriptPath
    ];
}

public sealed record OfflineGeoworldAlphaSliceManualAcceptanceGateBuildResult
{
    public OfflineGeoworldAlphaAcceptanceManifest Manifest { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceChecklist Checklist { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceResultTemplate ResultTemplate { get; init; } = new();
    public OfflineGeoworldAlphaReleaseGateDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceFileIndex FileIndex { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceChecksums Checksums { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceSimulatedProof SimulatedProof { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> PayloadFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceManualAcceptanceGateWriteResult
{
    public OfflineGeoworldAlphaSliceManualAcceptanceGateBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceManifest
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool MetadataOnly { get; init; } = true;
    public bool ManualAcceptanceRunner { get; init; } = true;
    public bool ReleaseGateDashboard { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; }
    public int PayloadFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int ChecklistStepCount { get; init; }
    public int Goal109PackageFileCount { get; init; }
    public int Goal109IndexedFileCount { get; init; }
    public int Goal109SourceComponentCount { get; init; }
    public bool Goal109AcceptedFalse { get; init; }
    public bool Goal109CleanImportProofPassed { get; init; }
    public bool Goal109NegativeProofPassed { get; init; }
    public bool Goal109UnityVerifierReady { get; init; }
    public bool UnityAcceptanceRunnerReady { get; init; }
    public string ResultTemplateRelativePath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot
        + "/"
        + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName;
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public IReadOnlyList<string> ReleaseRiskLinks { get; init; } = [];
    public IReadOnlyList<string> MilestoneGateLinks { get; init; } = [];
    public IReadOnlyList<string> NotFinalWarnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceChecklist
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaAcceptanceChecklistStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceChecklistStep
{
    public string StepId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
    public string EvidenceField { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
}

public sealed record OfflineGeoworldAlphaAcceptanceResultTemplate
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; } = true;
    public string ResultSchema { get; init; } = "offline_geoworld_alpha_acceptance_result_v1";
    public string ResultStatus { get; init; } = "manual_result_required";
    public string OperatorNotes { get; init; } = string.Empty;
    public string ChecklistHash { get; init; } = string.Empty;
    public string ResultHashField { get; init; } = "resultHash";
    public IReadOnlyList<OfflineGeoworldAlphaAcceptanceResultStepTemplate> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceResultStepTemplate
{
    public string StepId { get; init; } = string.Empty;
    public string Status { get; init; } = "pending";
    public string Notes { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaReleaseGateDashboard
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ReleaseReadinessStatus { get; init; } = "manual_acceptance_pending";
    public bool PackageReady { get; init; }
    public bool CleanImportProofPassed { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; }
    public bool UnityRunnerReady { get; init; }
    public bool ResultTemplateReady { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string ResultTemplateRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> ReleaseRiskLinks { get; init; } = [];
    public IReadOnlyList<string> MilestoneGateLinks { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public IReadOnlyList<string> ManualInstructions { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceFileIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int IndexedFileCount { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaAcceptanceFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public bool PackageRelativePath { get; init; } = true;
}

public sealed record OfflineGeoworldAlphaAcceptanceChecksums
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public string Algorithm { get; init; } = "SHA-256";
    public bool ChecksumsFileSelfExcluded { get; init; } = true;
    public int HashedFileCount { get; init; }
    public IReadOnlyDictionary<string, string> Sha256ByRelativePath { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaAcceptanceSourceLineage
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Goal109PackageManifestRead { get; init; }
    public bool Goal109FileIndexRead { get; init; }
    public bool Goal109ChecksumsRead { get; init; }
    public bool Goal109CleanImportProofRead { get; init; }
    public bool Goal109NegativeProofRead { get; init; }
    public bool Goal109QualityGateRead { get; init; }
    public bool Goal109AcceptedFalse { get; init; }
    public bool Goal109CleanImportProofPassed { get; init; }
    public bool Goal109NegativeProofPassed { get; init; }
    public bool Goal109QualityGatePassed { get; init; }
    public bool Goal109UnityVerifierReady { get; init; }
    public int Goal109PackageFileCount { get; init; }
    public int Goal109IndexedFileCount { get; init; }
    public int Goal109SourceComponentCount { get; init; }
    public int Goal109SourceHashCount { get; init; }
    public IReadOnlyDictionary<string, string> Goal109SourceHashes { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaAcceptanceUnityScriptInventory
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool ResultModelExists { get; init; }
    public bool ResultStoreExists { get; init; }
    public bool ReadsApplicationPersistentDataPath { get; init; }
    public bool SavesJsonResult { get; init; }
    public bool LoadsJsonResult { get; init; }
    public bool ClearsJsonResult { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int ScriptCount { get; init; }
    public int TotalLineCount { get; init; }
    public IReadOnlyDictionary<string, string> Sha256ByRelativePath { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaAcceptanceEditorWindowInventory
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool EditorWindowExists { get; init; }
    public string EditorWindowRelativePath { get; init; } = string.Empty;
    public bool MenuItemMarkerPresent { get; init; }
    public bool ReadsApplicationStreamingAssetsPath { get; init; }
    public bool ReadsGoal110Root { get; init; }
    public bool ShowsChecklistStatusFields { get; init; }
    public bool ShowsPackagePaths { get; init; }
    public bool CreateRunnerButtonPresent { get; init; }
    public bool ClearRunnerButtonPresent { get; init; }
    public bool SaveLoadResultButtonsPresent { get; init; }
    public bool DoesNotAutoMutateScenesOnImport { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int LineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaAcceptanceSimulatedProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal109PackageRead { get; init; }
    public bool ChecklistRead { get; init; }
    public bool ResultTemplateRead { get; init; }
    public bool SyntheticResultWritten { get; init; }
    public bool SyntheticResultLoaded { get; init; }
    public bool EveryChecklistStepWalked { get; init; }
    public bool ResultHashValidationPassed { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; }
    public int WalkedStepCount { get; init; }
    public string ChecklistHash { get; init; } = string.Empty;
    public string ResultTemplateHash { get; init; } = string.Empty;
    public string SyntheticResultHash { get; init; } = string.Empty;
    public string LoadedResultHash { get; init; } = string.Empty;
    public string SyntheticResultRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> WalkedStepIds { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceNegativeProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaAcceptanceNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool WorkspaceGroupPresent { get; init; }
    public bool ProofStatusPresent { get; init; }
    public bool PageBindDisplaysManualAcceptance { get; init; }
    public string PageRelativePath { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaAcceptanceQualityGateScan
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool ManifestPassed { get; init; }
    public bool ChecklistPassed { get; init; }
    public bool ResultTemplatePassed { get; init; }
    public bool DashboardPassed { get; init; }
    public bool FileIndexPassed { get; init; }
    public bool ChecksumsPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool SimulatedProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoNetworkProviderMarkers { get; init; }
    public bool NoScenePrefabSettingsProjectPackageMutation { get; init; }
    public bool SourceHealthLimitsPassed { get; init; }
    public int PayloadFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int IndexedFileCount { get; init; }
    public int ChecklistStepCount { get; init; }
    public int NegativeRejectedCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public string ExportAggregateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool ManualAcceptancePending { get; init; } = true;
    public bool AutomatedGatePassed { get; init; }
    public int PayloadFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int ChecklistStepCount { get; init; }
    public bool Goal109CleanImportProofPassed { get; init; }
    public bool SimulatedProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string SimulatedResultHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}
