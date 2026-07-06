namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GenericGamePackageFullPlaythroughProjectionVocabulary
{
    public const string GoalId =
        "goal_126_generic_gamepackage_full_playthrough_projection";
    public const string ScenarioId =
        "goal-126-generic-gamepackage-full-playthrough-projection";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke";
    public const string SamplePackagePath =
        GenericGamePackageProjectionVocabulary.SamplePackagePath;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-126-generic-gamepackage-full-playthrough-projection";
    public const string DocumentationPath =
        "docs/manual-acceptance/generic-gamepackage-full-playthrough-projection.md";
    public const string UnityBatchmodeLogFileName =
        "unity-batchmode-generic-gamepackage-full-playthrough.log";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/" + UnityBatchmodeLogFileName;
    public const string UnityBatchmodeExportLogRelativePath =
        ExportPackageDirectory + "/" + UnityBatchmodeLogFileName;

    public const string DashboardFileName =
        "generic-gamepackage-full-playthrough-dashboard.json";
    public const string ScriptInventoryFileName =
        "generic-gamepackage-full-playthrough-script-inventory.json";
    public const string SmokePlanFileName =
        "generic-gamepackage-full-playthrough-smoke-plan.json";
    public const string LogScanFileName =
        "generic-gamepackage-full-playthrough-log-scan.json";
    public const string ReportFileName =
        "generic-gamepackage-full-playthrough-report.md";
    public const string NegativeProofFileName =
        "generic-gamepackage-full-playthrough-negative-proof.json";
    public const string FileIndexFileName =
        "generic-gamepackage-full-playthrough-file-index.json";

    public const string UnityPlaythroughPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionPlaythrough.cs";

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

public sealed record GenericGamePackageFullPlaythroughProjectionBuildResult
{
    public GenericGamePackageFullPlaythroughProjectionDashboard Dashboard { get; init; } = new();
    public GenericGamePackageFullPlaythroughSampleSummary SamplePackage { get; init; } = new();
    public GenericGamePackageFullPlaythroughScriptInventory ScriptInventory { get; init; } = new();
    public GenericGamePackageFullPlaythroughSmokePlan SmokePlan { get; init; } = new();
    public GenericGamePackageFullPlaythroughLogScan LogScan { get; init; } = new();
    public GenericGamePackageFullPlaythroughNegativeProof NegativeProof { get; init; } = new();
    public GenericGamePackageFullPlaythroughFileIndex ProceduralFileIndex { get; init; } = new();
    public GenericGamePackageFullPlaythroughFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GenericGamePackageFullPlaythroughProjectionWriteResult
{
    public GenericGamePackageFullPlaythroughProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughProjectionDashboard
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public string FullPlaythroughStatus { get; init; } = "BLOCKED";
    public string SamplePackagePath { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public bool MapPathPreviewPresent { get; init; }
    public bool SignInteractionApplied { get; init; }
    public bool DialogueSummaryPresent { get; init; }
    public bool QuestObjectiveStatusPresent { get; init; }
    public bool InventorySummaryPresent { get; init; }
    public bool ResourceSummaryPresent { get; init; }
    public bool SystemsSummaryPresent { get; init; }
    public bool RecipeApplyPassed { get; init; }
    public bool HarvestApplyPassed { get; init; }
    public bool TransactionPreviewPresent { get; init; }
    public bool CombatRoundPreviewPresent { get; init; }
    public bool EventTranscriptPresent { get; init; }
    public string UnitySmokeStatus { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH";
    public bool CleanupScriptAvailable { get; init; }
    public bool Goal125StillGreen { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public string EvidencePath { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory;
    public string UnityBatchmodeExecuteMethod { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeLogRelativePath;
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughSampleSummary
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public string RelativePath { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath;
    public bool Exists { get; init; }
    public bool Parsed { get; init; }
    public bool ReadOnlySource { get; init; }
    public bool ExcludedFromExpectedChangedPaths { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public bool StartPositionPresent { get; init; }
    public bool PathTargetPresent { get; init; }
    public bool PathWalkable { get; init; }
    public bool SignInteractionPresent { get; init; }
    public bool OldGuardDialoguePresent { get; init; }
    public bool HelpHealerQuestIncomplete { get; init; }
    public bool PlayerInventoryPresent { get; init; }
    public bool ResourceDefaultsPresent { get; init; }
    public bool RecipeRequirementsMatchExpected { get; init; }
    public bool HarvestContractPresent { get; init; }
    public bool TransactionPresent { get; init; }
    public bool CombatRoundMatchesExpected { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughScriptInventory
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool WindowActionPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool StateClassTracksFullPlaythroughFields { get; init; }
    public bool PlaythroughRunsRequiredSequence { get; init; }
    public bool ControllerRendersFullPlaythroughMarkers { get; init; }
    public bool ModelsExposeFullPlaythroughSmokeFields { get; init; }
    public bool ExistingGoal125VerificationStillPresent { get; init; }
    public bool NoSourceWriteMarkers { get; init; }
    public IReadOnlyList<string> ForbiddenSourceMarkersFound { get; init; } = [];
    public IReadOnlyList<GenericGamePackageFullPlaythroughScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record GenericGamePackageFullPlaythroughSmokePlan
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<GenericGamePackageFullPlaythroughSmokePlanStep> Steps { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record GenericGamePackageFullPlaythroughLogScan
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughNegativeProof
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool NoForbiddenPathExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughFileIndex
{
    public string GoalId { get; init; } =
        GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GenericGamePackageFullPlaythroughFileIndexEntry> Files { get; init; } = [];
}

public sealed record GenericGamePackageFullPlaythroughFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
