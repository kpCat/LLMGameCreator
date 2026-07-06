namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GenericGamePackageLoopProjectionVocabulary
{
    public const string GoalId =
        "goal_124_generic_gamepackage_quest_dialogue_interaction_loop";
    public const string ScenarioId =
        "goal-124-generic-gamepackage-quest-dialogue-interaction-loop";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageLoopSmoke";
    public const string SamplePackagePath =
        GenericGamePackageProjectionVocabulary.SamplePackagePath;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-124-generic-gamepackage-quest-dialogue-interaction-loop";
    public const string DocumentationPath =
        "docs/manual-acceptance/generic-gamepackage-quest-dialogue-interaction-loop.md";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-generic-gamepackage-loop.log";

    public const string DashboardFileName =
        "generic-gamepackage-loop-dashboard.json";
    public const string ScriptInventoryFileName =
        "generic-gamepackage-loop-script-inventory.json";
    public const string SmokePlanFileName =
        "generic-gamepackage-loop-smoke-plan.json";
    public const string LogScanFileName =
        "generic-gamepackage-loop-log-scan.json";
    public const string ReportFileName =
        "generic-gamepackage-loop-report.md";
    public const string NegativeProofFileName =
        "generic-gamepackage-loop-negative-proof.json";
    public const string FileIndexFileName =
        "generic-gamepackage-loop-file-index.json";

    public const string UnityStatePath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs";
    public const string UnityLoopPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs";

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

public sealed record GenericGamePackageLoopProjectionBuildResult
{
    public GenericGamePackageLoopProjectionDashboard Dashboard { get; init; } = new();
    public GenericGamePackageLoopSampleSummary SamplePackage { get; init; } = new();
    public GenericGamePackageLoopScriptInventory ScriptInventory { get; init; } = new();
    public GenericGamePackageLoopSmokePlan SmokePlan { get; init; } = new();
    public GenericGamePackageLoopLogScan LogScan { get; init; } = new();
    public GenericGamePackageLoopNegativeProof NegativeProof { get; init; } = new();
    public GenericGamePackageLoopFileIndex ProceduralFileIndex { get; init; } = new();
    public GenericGamePackageLoopFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GenericGamePackageLoopProjectionWriteResult
{
    public GenericGamePackageLoopProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GenericGamePackageLoopProjectionDashboard
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public string GenericLoopStatus { get; init; } = "BLOCKED";
    public string SamplePackagePath { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.SamplePackagePath;
    public string PackageId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public bool InteractionPreviewPresent { get; init; }
    public bool InteractionApplyPassed { get; init; }
    public bool DialogueSummaryPresent { get; init; }
    public bool QuestObjectiveSummaryPresent { get; init; }
    public bool InventorySummaryPresent { get; init; }
    public bool ResourceSummaryPresent { get; init; }
    public string UnitySmokeStatus { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP";
    public bool CleanupScriptAvailable { get; init; }
    public bool Goal123StillGreen { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageReadOnly { get; init; }
    public int AppliedInteractionCount { get; init; }
    public int StartedQuestCount { get; init; }
    public string EvidencePath { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory;
    public string UnityBatchmodeExecuteMethod { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.UnityBatchmodeLogRelativePath;
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageLoopSampleSummary
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public string RelativePath { get; init; } =
        GenericGamePackageLoopProjectionVocabulary.SamplePackagePath;
    public bool Exists { get; init; }
    public bool Parsed { get; init; }
    public bool ReadOnlySource { get; init; }
    public bool ExcludedFromExpectedChangedPaths { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public bool SignEntityPresent { get; init; }
    public bool SignInspectInteractionPresent { get; init; }
    public bool SignInspectSetFlagEffectPresent { get; init; }
    public bool SignInspectLogEffectPresent { get; init; }
    public bool OldGuardEntityPresent { get; init; }
    public bool OldGuardDialoguePresent { get; init; }
    public bool HelpHealerQuestPresent { get; init; }
    public int RequiredRedHerbAmount { get; init; }
    public int PlayerRedHerbAmount { get; init; }
    public bool HelpHealerIncomplete { get; init; }
    public int InventoryStackCount { get; init; }
    public int ResourceCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GenericGamePackageLoopScriptInventory
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool WindowActionPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool StateClassTracksRequiredFields { get; init; }
    public bool LoopRunsRequiredSequence { get; init; }
    public bool ControllerRendersLoopMarkers { get; init; }
    public bool AdapterParsesLoopData { get; init; }
    public bool ModelsExposeLoopSmokeFields { get; init; }
    public bool ExistingGoal123VerificationStillPresent { get; init; }
    public bool NoSourceWriteMarkers { get; init; }
    public IReadOnlyList<string> ForbiddenSourceMarkersFound { get; init; } = [];
    public IReadOnlyList<GenericGamePackageLoopScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record GenericGamePackageLoopScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record GenericGamePackageLoopSmokePlan
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<GenericGamePackageLoopSmokePlanStep> Steps { get; init; } = [];
}

public sealed record GenericGamePackageLoopSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record GenericGamePackageLoopLogScan
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } =
        "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GenericGamePackageLoopNegativeProof
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool NoForbiddenPathExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GenericGamePackageLoopFileIndex
{
    public string GoalId { get; init; } = GenericGamePackageLoopProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GenericGamePackageLoopFileIndexEntry> Files { get; init; } = [];
}

public sealed record GenericGamePackageLoopFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
