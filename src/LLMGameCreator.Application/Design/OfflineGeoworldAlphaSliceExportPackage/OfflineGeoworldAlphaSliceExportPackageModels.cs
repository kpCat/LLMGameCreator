namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

public static class OfflineGeoworldAlphaSliceExportPackageVocabulary
{
    public const string GoalId = "goal_109_offline_geoworld_alpha_slice_export_package";
    public const string ProductSmokeRoute = "goal-109-offline-geoworld-alpha-slice-export-package";
    public const string FinalGate = "offline_geoworld_alpha_slice_export_package_verification";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-109-offline-geoworld-alpha-slice";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal109";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestFileName = "offline-geoworld-alpha-export-manifest.json";
    public const string FileIndexFileName = "offline-geoworld-alpha-export-file-index.json";
    public const string ChecksumsFileName = "offline-geoworld-alpha-export-checksums.json";
    public const string RunbookFileName = "offline-geoworld-alpha-export-runbook.md";
    public const string AcceptanceGateFileName = "offline-geoworld-alpha-export-acceptance-gate.json";
    public const string ReadmeFileName = "offline-geoworld-alpha-export-readme.md";
    public const string ReportFileName = "offline-geoworld-alpha-export-report.md";
    public const string CleanImportProofFileName = "offline-geoworld-alpha-export-clean-import-proof.json";
    public const string NegativeProofFileName = "offline-geoworld-alpha-export-negative-proof.json";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-alpha-export-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-alpha-export-editor-window-inventory.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-alpha-export-workspace-binding-inventory.json";
    public const string SourceLineageFileName = "offline-geoworld-alpha-export-source-lineage.json";
    public const string QualityGateScanFileName = "offline-geoworld-alpha-export-quality-gate-scan.json";

    public const string UnityVerifierScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSlicePackageVerifier.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSlicePackageWindow.cs";

    public static IReadOnlyList<string> RequiredPackageFileNames =>
    [
        ManifestFileName,
        FileIndexFileName,
        ChecksumsFileName,
        RunbookFileName,
        AcceptanceGateFileName,
        ReadmeFileName
    ];

    public static IReadOnlyList<string> IndexedPackageFileNames =>
    [
        ManifestFileName,
        FileIndexFileName,
        RunbookFileName,
        AcceptanceGateFileName,
        ReadmeFileName
    ];

    public static IReadOnlyList<string> RequiredEvidenceFileNames =>
    [
        ReportFileName,
        CleanImportProofFileName,
        NegativeProofFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static IReadOnlyList<string> RequiredManualGates =>
    [
        "offline_geoworld_unity_preview_runner_verification",
        "offline_geoworld_unity_editor_preview_tool_verification",
        "offline_geoworld_playmode_travel_preview_verification",
        "offline_geoworld_interactive_travel_preview_verification",
        "offline_geoworld_interaction_playable_probe_verification",
        "offline_geoworld_session_persistence_replay_verification",
        "offline_geoworld_objective_acceptance_run_verification",
        "offline_geoworld_alpha_slice_orchestrator_verification",
        FinalGate
    ];

    public static IReadOnlyList<string> RequiredNegativeScenarioIds =>
    [
        "missing_goal108_manifest",
        "missing_goal108a_audit",
        "missing_export_manifest",
        "missing_indexed_file",
        "checksum_mismatch",
        "absolute_path",
        "raw_geodata_leak",
        "binary_raster_media_marker",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency_marker",
        "unity_scene_settings_mutation_marker",
        "accepted_true_fake_promotion",
        "missing_manual_gate",
        "missing_not_final_warning",
        "historical_artifact_rewrite_attempt",
        "fake_clean_import_without_reading_files"
    ];

    public static IReadOnlyList<string> SourceHealthFiles =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/OfflineGeoworldAlphaSliceExportPackageModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/OfflineGeoworldAlphaSliceExportPackageEvidenceService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/OfflineGeoworldAlphaSliceExportPackageEvidenceService.Utilities.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/OfflineGeoworldAlphaSliceExportPackageEvidenceService.Quality.cs",
        UnityVerifierScriptPath,
        UnityEditorWindowScriptPath
    ];
}

public sealed record OfflineGeoworldAlphaSliceExportPackageBuildResult
{
    public OfflineGeoworldAlphaSliceExportManifest Manifest { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportFileIndex FileIndex { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportChecksums Checksums { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportAcceptanceGate AcceptanceGate { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportCleanImportProof CleanImportProof { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaSliceExportReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> PackageFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceExportPackageWriteResult
{
    public OfflineGeoworldAlphaSliceExportPackageBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportManifest
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool MetadataOnly { get; init; } = true;
    public bool AlphaToolingOnly { get; init; } = true;
    public bool PortableDirectoryPackage { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public int PackageFileCount { get; init; }
    public int IndexedFileCount { get; init; }
    public int SourceComponentCount { get; init; }
    public int ReadySourceComponentCount { get; init; }
    public int ManualGateCount { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalObjectiveStatus { get; init; } = string.Empty;
    public string Goal107FinalAcceptanceHash { get; init; } = string.Empty;
    public string Goal108ComponentAggregateHash { get; init; } = string.Empty;
    public bool Goal108AcceptedFalse { get; init; } = true;
    public bool Goal108AImmutabilityAuditIncluded { get; init; }
    public bool Goal101To107HistoricalArtifactsUnchanged { get; init; }
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoNetworkProviderMarkers { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string ExportPackageRoot { get; init; } =
        OfflineGeoworldAlphaSliceExportPackageVocabulary.ExportPackageDirectory;
    public string StreamingAssetsRelativeRoot { get; init; } =
        "LLMGameCreator/OfflineGeoworldGoal109";
    public IReadOnlyList<string> ManualGates { get; init; } = [];
    public IReadOnlyList<string> NotFinalWarnings { get; init; } =
    [
        "Manual gate offline_geoworld_alpha_slice_export_package_verification remains required.",
        "Goal109 package is portable Alpha review/export tooling, not final Runtime or release build.",
        "Real geodata, providers, final art, scene/prefab changes and public schema changes remain separate future gates."
    ];
}

public sealed record OfflineGeoworldAlphaSliceExportFileIndex
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int IndexedFileCount { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaSliceExportFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public bool PackageRelativePath { get; init; } = true;
}

public sealed record OfflineGeoworldAlphaSliceExportChecksums
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public string Algorithm { get; init; } = "SHA-256";
    public bool ChecksumsFileSelfExcluded { get; init; } = true;
    public int HashedFileCount { get; init; }
    public IReadOnlyDictionary<string, string> Sha256ByRelativePath { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceExportAcceptanceGate
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool PackageReadyForManualReview { get; init; }
    public bool CleanImportRequired { get; init; } = true;
    public bool UnityVerifierRequired { get; init; } = true;
    public bool WorkspaceInspectionRequired { get; init; } = true;
    public bool NotFinalRelease { get; init; } = true;
    public IReadOnlyList<string> RequiredManualGates { get; init; } = [];
    public IReadOnlyList<string> ManualChecklist { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportSourceLineage
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Goal108ManifestRead { get; init; }
    public bool Goal108ComponentsRead { get; init; }
    public bool Goal108SimulatedProofRead { get; init; }
    public bool Goal108NegativeProofRead { get; init; }
    public bool Goal108AImmutabilityAuditRead { get; init; }
    public bool Goal108ASourceSplitReportRead { get; init; }
    public bool Goal108AHistoricalDiffAuditRead { get; init; }
    public bool Goal101To107ArtifactsUnchanged { get; init; }
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public int SourceHashCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceExportSourceComponent> Components { get; init; } = [];
    public IReadOnlyDictionary<string, string> SourceArtifactHashes { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceExportSourceComponent
{
    public string ComponentId { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string SourceArtifactRoot { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public bool Ready { get; init; }
    public int SourceHashCount { get; init; }
}

public sealed record OfflineGeoworldAlphaSliceExportCleanImportProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool PackageRootReadAttempted { get; init; }
    public bool ManifestPresent { get; init; }
    public bool FileIndexPresent { get; init; }
    public bool ChecksumsPresent { get; init; }
    public bool AllRequiredFilesPresent { get; init; }
    public bool AllIndexedFilesPresent { get; init; }
    public bool ChecksumsMatch { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoNetworkProviderMarkers { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool ManualGatesListed { get; init; }
    public bool Goal107FinalObjectiveAcceptanceIncluded { get; init; }
    public bool Goal108ASourceSplitImmutabilityAuditIncluded { get; init; }
    public int IndexedFileCount { get; init; }
    public int ReadFileCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceExportNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceExportUnityScriptInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool VerifierExists { get; init; }
    public string VerifierRelativePath { get; init; } = string.Empty;
    public bool ReadsApplicationStreamingAssetsPath { get; init; }
    public bool ReadsGoal109Root { get; init; }
    public bool ExposesStatusFields { get; init; }
    public bool VerifyPackageMethodPresent { get; init; }
    public bool ChecksumVerificationMarkerPresent { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int LineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceExportEditorWindowInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool EditorWindowExists { get; init; }
    public string EditorWindowRelativePath { get; init; } = string.Empty;
    public bool MenuItemMarkerPresent { get; init; }
    public bool ShowsPackageReadiness { get; init; }
    public bool VerifyButtonPresent { get; init; }
    public bool ShowsRunbookAndAcceptanceSummary { get; init; }
    public bool DoesNotMutateScenesAutomatically { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int LineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool WorkspaceGroupPresent { get; init; }
    public bool ProofStatusPresent { get; init; }
    public bool PageBindDisplaysExportPackage { get; init; }
    public string PageRelativePath { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceExportQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool ManifestPassed { get; init; }
    public bool FileIndexPassed { get; init; }
    public bool ChecksumsPassed { get; init; }
    public bool CleanImportProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoNetworkProviderMarkers { get; init; }
    public bool NoScenePrefabSettingsProjectPackageMutation { get; init; }
    public bool SourceHealthLimitsPassed { get; init; }
    public int PackageFileCount { get; init; }
    public int IndexedFileCount { get; init; }
    public int SourceComponentCount { get; init; }
    public int ReadySourceComponentCount { get; init; }
    public int NegativeRejectedCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public string PackageAggregateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceExportReport
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PackageFileCount { get; init; }
    public int IndexedFileCount { get; init; }
    public int SourceComponentCount { get; init; }
    public int ReadySourceComponentCount { get; init; }
    public bool CleanImportProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public int NegativeRejectedCount { get; init; }
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string Goal107FinalAcceptanceHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}
