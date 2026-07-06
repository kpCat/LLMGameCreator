namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GenericGamePackageProjectionVocabulary
{
    public const string GoalId =
        "goal_123_generic_gamepackage_playable_projection_adapter";
    public const string ScenarioId =
        "goal-123-generic-gamepackage-playable-projection-adapter";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageProjectionSmoke";
    public const string SamplePackagePath =
        "samples/minimal-map-game/package.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-123-generic-gamepackage-playable-projection-adapter";
    public const string DocumentationPath =
        "docs/manual-acceptance/generic-gamepackage-playable-projection-adapter.md";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-generic-gamepackage-projection.log";

    public const string DashboardFileName =
        "generic-gamepackage-projection-dashboard.json";
    public const string ScriptInventoryFileName =
        "generic-gamepackage-projection-script-inventory.json";
    public const string SmokePlanFileName =
        "generic-gamepackage-projection-smoke-plan.json";
    public const string LogScanFileName =
        "generic-gamepackage-projection-log-scan.json";
    public const string ReportFileName =
        "generic-gamepackage-projection-report.md";
    public const string NegativeProofFileName =
        "generic-gamepackage-projection-negative-proof.json";
    public const string FileIndexFileName =
        "generic-gamepackage-projection-file-index.json";

    public const string UnityAdapterPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs";
    public const string UnityModelsPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs";
    public const string UnityControllerPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs";

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        ScriptInventoryFileName,
        SmokePlanFileName,
        LogScanFileName,
        ReportFileName,
        NegativeProofFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;
}

public sealed record GenericGamePackageProjectionBuildResult
{
    public GenericGamePackageProjectionDashboard Dashboard { get; init; } = new();
    public GenericGamePackageProjectionSamplePackageSummary SamplePackage { get; init; } = new();
    public GenericGamePackageProjectionScriptInventory ScriptInventory { get; init; } = new();
    public GenericGamePackageProjectionSmokePlan SmokePlan { get; init; } = new();
    public GenericGamePackageProjectionLogScan LogScan { get; init; } = new();
    public GenericGamePackageProjectionNegativeProof NegativeProof { get; init; } = new();
    public GenericGamePackageProjectionFileIndex ProceduralFileIndex { get; init; } = new();
    public GenericGamePackageProjectionFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GenericGamePackageProjectionWriteResult
{
    public GenericGamePackageProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GenericGamePackageProjectionDashboard
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public string GenericProjectionStatus { get; init; } = "BLOCKED";
    public string SamplePackagePath { get; init; } =
        GenericGamePackageProjectionVocabulary.SamplePackagePath;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string MapSize { get; init; } = string.Empty;
    public int EntityCount { get; init; }
    public int ItemCount { get; init; }
    public string UnitySmokeStatus { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION";
    public bool Goal122StillGreen { get; init; }
    public bool CleanupScriptAvailable { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public string EvidencePath { get; init; } =
        GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GenericGamePackageProjectionVocabulary.ExportPackageDirectory;
    public string UnityBatchmodeExecuteMethod { get; init; } =
        GenericGamePackageProjectionVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        GenericGamePackageProjectionVocabulary.UnityBatchmodeLogRelativePath;
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageProjectionSamplePackageSummary
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public string RelativePath { get; init; } =
        GenericGamePackageProjectionVocabulary.SamplePackagePath;
    public bool Exists { get; init; }
    public bool Parsed { get; init; }
    public bool ReadOnlySource { get; init; }
    public bool ExcludedFromExpectedChangedPaths { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string MapName { get; init; } = string.Empty;
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }
    public int StartX { get; init; }
    public int StartY { get; init; }
    public int ExplicitTileCount { get; init; }
    public bool WallTilePresent { get; init; }
    public bool RoadTilePresent { get; init; }
    public int EntityCount { get; init; }
    public int InteractableEntityCount { get; init; }
    public int ItemCount { get; init; }
    public bool PackageIdentityPresent { get; init; }
    public bool MapDimensionsPresent { get; init; }
    public bool StartPositionPresent { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageProjectionScriptInventory
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool WindowActionPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool AdapterReadsSamplePackage { get; init; }
    public bool ControllerBuildsGenericSection { get; init; }
    public bool ControllerVerifiesRequiredMarkers { get; init; }
    public bool ModelsExposeSmokeFields { get; init; }
    public bool ExistingGoal122VerificationStillPresent { get; init; }
    public bool MarkerDescriptorCompatible { get; init; }
    public bool NoSourceWriteMarkers { get; init; }
    public IReadOnlyList<string> ForbiddenSourceMarkersFound { get; init; } = [];
    public IReadOnlyList<GenericGamePackageProjectionScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record GenericGamePackageProjectionScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record GenericGamePackageProjectionSmokePlan
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<GenericGamePackageProjectionSmokePlanStep> Steps { get; init; } = [];
}

public sealed record GenericGamePackageProjectionSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record GenericGamePackageProjectionLogScan
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GenericGamePackageProjectionNegativeProof
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool NoForbiddenPathExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GenericGamePackageProjectionFileIndex
{
    public string GoalId { get; init; } = GenericGamePackageProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GenericGamePackageProjectionFileIndexEntry> Files { get; init; } = [];
}

public sealed record GenericGamePackageProjectionFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
