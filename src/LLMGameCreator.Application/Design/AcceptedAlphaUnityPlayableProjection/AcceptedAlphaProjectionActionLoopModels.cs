namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class AcceptedAlphaProjectionActionLoopVocabulary
{
    public const string GoalId =
        "goal_122_accepted_alpha_projection_action_loop_and_window_polish";
    public const string ScenarioId =
        "goal-122-accepted-alpha-projection-action-loop-and-window-polish";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionActionLoopSmoke";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-122-accepted-alpha-projection-action-loop-and-window-polish";
    public const string DocumentationPath =
        "docs/manual-acceptance/accepted-alpha-projection-action-loop-and-window-polish.md";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-action-loop-smoke.log";

    public const string DashboardFileName =
        "accepted-alpha-projection-action-loop-dashboard.json";
    public const string ScriptInventoryFileName =
        "accepted-alpha-projection-action-loop-script-inventory.json";
    public const string SmokePlanFileName =
        "accepted-alpha-projection-action-loop-smoke-plan.json";
    public const string LogScanFileName =
        "accepted-alpha-projection-action-loop-log-scan.json";
    public const string ReportFileName =
        "accepted-alpha-projection-action-loop-report.md";
    public const string NegativeProofFileName =
        "accepted-alpha-projection-action-loop-negative-proof.json";
    public const string FileIndexFileName =
        "accepted-alpha-projection-action-loop-file-index.json";

    public const string UnityStatePath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionState.cs";

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

public sealed record AcceptedAlphaProjectionActionLoopBuildResult
{
    public AcceptedAlphaProjectionActionLoopDashboard Dashboard { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopScriptInventory ScriptInventory { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopSmokePlan SmokePlan { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopLogScan LogScan { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopNegativeProof NegativeProof { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopFileIndex ProceduralFileIndex { get; init; } = new();
    public AcceptedAlphaProjectionActionLoopFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionActionLoopWriteResult
{
    public AcceptedAlphaProjectionActionLoopBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopDashboard
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public string ActionLoopStatus { get; init; } = "BLOCKED";
    public string WindowPolishStatus { get; init; } = "BLOCKED";
    public string UnityMenuPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath;
    public bool OneClickVerificationStillPresent { get; init; }
    public bool Goal121StillGreen { get; init; }
    public bool ProjectionActionPreviewPresent { get; init; }
    public bool ProjectionActionApplyPresent { get; init; }
    public bool ProjectionStateResetPresent { get; init; }
    public bool WindowLayoutPolishPresent { get; init; }
    public bool CleanupScriptAvailable { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public string UnityBatchmodeExecuteMethod { get; init; } =
        AcceptedAlphaProjectionActionLoopVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        AcceptedAlphaProjectionActionLoopVocabulary.UnityBatchmodeLogRelativePath;
    public string UnitySmokeStatus { get; init; } = "PENDING_UNITY_BATCHMODE_ACTION_LOOP_SMOKE";
    public bool DoNotStartAutomatically { get; init; } = true;
    public bool ProjectionOnlyState { get; init; } = true;
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public string EvidencePath { get; init; } =
        AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopScriptInventory
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool OneClickVerificationStillPresent { get; init; }
    public bool BatchmodeActionLoopMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool ActionLoopControlsPresent { get; init; }
    public bool ProjectionStateModelPresent { get; init; }
    public bool ProjectionActionPreviewPresent { get; init; }
    public bool ProjectionActionApplyPresent { get; init; }
    public bool ProjectionStateResetPresent { get; init; }
    public bool WindowLayoutPolishPresent { get; init; }
    public bool ManualCleanupHintPresent { get; init; }
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public bool MaterialWarningSourceClean { get; init; }
    public IReadOnlyList<AcceptedAlphaProjectionActionLoopScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionActionLoopSmokePlan
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<AcceptedAlphaProjectionActionLoopSmokePlanStep> Steps { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionActionLoopLogScan
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool MaterialInstantiationWarningAbsent { get; init; } = true;
    public bool RendererGetMaterialStackAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE_ACTION_LOOP_SMOKE";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopNegativeProof
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool LiveGeodataProviderNetworkRejected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopFileIndex
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<AcceptedAlphaProjectionActionLoopFileIndexEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionActionLoopFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
