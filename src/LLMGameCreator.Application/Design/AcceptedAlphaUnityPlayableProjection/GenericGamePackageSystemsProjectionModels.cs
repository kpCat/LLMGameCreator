namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GenericGamePackageSystemsProjectionVocabulary
{
    public const string GoalId =
        "goal_125_generic_gamepackage_systems_loop_projection";
    public const string ScenarioId =
        "goal-125-generic-gamepackage-systems-loop-projection";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageSystemsSmoke";
    public const string SamplePackagePath =
        GenericGamePackageProjectionVocabulary.SamplePackagePath;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-125-generic-gamepackage-systems-loop-projection";
    public const string DocumentationPath =
        "docs/manual-acceptance/generic-gamepackage-systems-loop-projection.md";
    public const string UnityBatchmodeLogFileName =
        "unity-batchmode-generic-gamepackage-systems-loop.log";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/" + UnityBatchmodeLogFileName;
    public const string UnityBatchmodeExportLogRelativePath =
        ExportPackageDirectory + "/" + UnityBatchmodeLogFileName;

    public const string DashboardFileName =
        "generic-gamepackage-systems-loop-dashboard.json";
    public const string ScriptInventoryFileName =
        "generic-gamepackage-systems-loop-script-inventory.json";
    public const string SmokePlanFileName =
        "generic-gamepackage-systems-loop-smoke-plan.json";
    public const string LogScanFileName =
        "generic-gamepackage-systems-loop-log-scan.json";
    public const string ReportFileName =
        "generic-gamepackage-systems-loop-report.md";
    public const string NegativeProofFileName =
        "generic-gamepackage-systems-loop-negative-proof.json";
    public const string FileIndexFileName =
        "generic-gamepackage-systems-loop-file-index.json";

    public const string UnitySystemsPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionSystems.cs";

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

public sealed record GenericGamePackageSystemsProjectionBuildResult
{
    public GenericGamePackageSystemsProjectionDashboard Dashboard { get; init; } = new();
    public GenericGamePackageSystemsSampleSummary SamplePackage { get; init; } = new();
    public GenericGamePackageSystemsScriptInventory ScriptInventory { get; init; } = new();
    public GenericGamePackageSystemsSmokePlan SmokePlan { get; init; } = new();
    public GenericGamePackageSystemsLogScan LogScan { get; init; } = new();
    public GenericGamePackageSystemsNegativeProof NegativeProof { get; init; } = new();
    public GenericGamePackageSystemsFileIndex ProceduralFileIndex { get; init; } = new();
    public GenericGamePackageSystemsFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GenericGamePackageSystemsProjectionWriteResult
{
    public GenericGamePackageSystemsProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GenericGamePackageSystemsProjectionDashboard
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public string GenericSystemsStatus { get; init; } = "BLOCKED";
    public string SamplePackagePath { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath;
    public string PackageId { get; init; } = string.Empty;
    public bool RecipePreviewPresent { get; init; }
    public bool RecipeApplyPassed { get; init; }
    public bool HarvestPreviewPresent { get; init; }
    public bool HarvestApplyPassed { get; init; }
    public bool TransactionPreviewPresent { get; init; }
    public bool EncounterPreviewPresent { get; init; }
    public bool CombatRoundPreviewPresent { get; init; }
    public bool InventorySummaryPresent { get; init; }
    public bool ResourceSummaryPresent { get; init; }
    public bool SystemsEventLogPresent { get; init; }
    public string UnitySmokeStatus { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS";
    public bool CleanupScriptAvailable { get; init; }
    public bool Goal124StillGreen { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public string EvidencePath { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory;
    public string UnityBatchmodeExecuteMethod { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeLogRelativePath;
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageSystemsSampleSummary
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public string RelativePath { get; init; } =
        GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath;
    public bool Exists { get; init; }
    public bool Parsed { get; init; }
    public bool ReadOnlySource { get; init; }
    public bool ExcludedFromExpectedChangedPaths { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public bool PlayerInventoryPresent { get; init; }
    public bool ResourceDefaultsPresent { get; init; }
    public bool RecipeHealingPotionPresent { get; init; }
    public bool RecipeRequirementsMatchExpected { get; init; }
    public bool HarvestNodePresent { get; init; }
    public bool HarvestLootPresent { get; init; }
    public bool TransactionPresent { get; init; }
    public bool EncounterPresent { get; init; }
    public bool CombatRoundMatchesExpected { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageSystemsScriptInventory
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool WindowActionPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool StateClassTracksRequiredFields { get; init; }
    public bool SystemsLoopRunsRequiredSequence { get; init; }
    public bool ControllerRendersSystemsMarkers { get; init; }
    public bool AdapterParsesSystemsData { get; init; }
    public bool ModelsExposeSystemsSmokeFields { get; init; }
    public bool ExistingGoal124VerificationStillPresent { get; init; }
    public bool NoSourceWriteMarkers { get; init; }
    public IReadOnlyList<string> ForbiddenSourceMarkersFound { get; init; } = [];
    public IReadOnlyList<GenericGamePackageSystemsScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record GenericGamePackageSystemsScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record GenericGamePackageSystemsSmokePlan
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<GenericGamePackageSystemsSmokePlanStep> Steps { get; init; } = [];
}

public sealed record GenericGamePackageSystemsSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record GenericGamePackageSystemsLogScan
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GenericGamePackageSystemsNegativeProof
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool NoForbiddenPathExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GenericGamePackageSystemsFileIndex
{
    public string GoalId { get; init; } = GenericGamePackageSystemsProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GenericGamePackageSystemsFileIndexEntry> Files { get; init; } = [];
}

public sealed record GenericGamePackageSystemsFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
