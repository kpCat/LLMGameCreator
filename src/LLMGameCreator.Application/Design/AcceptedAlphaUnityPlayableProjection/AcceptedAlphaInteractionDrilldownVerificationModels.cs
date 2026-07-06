namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class AcceptedAlphaInteractionDrilldownVerificationVocabulary
{
    public const string GoalId =
        "goal_121_accepted_alpha_interaction_drilldown_and_one_click_verification";
    public const string ScenarioId =
        "goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification";
    public const string FullVerificationStatus = "GREEN";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionFullVerification";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification";
    public const string DocumentationPath =
        "docs/manual-acceptance/accepted-alpha-interaction-drilldown-and-one-click-verification.md";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-full-projection-verification.log";

    public const string DashboardFileName =
        "accepted-alpha-interaction-drilldown-dashboard.json";
    public const string ScriptInventoryFileName =
        "accepted-alpha-interaction-drilldown-script-inventory.json";
    public const string SmokePlanFileName =
        "accepted-alpha-interaction-drilldown-smoke-plan.json";
    public const string LogScanFileName =
        "accepted-alpha-interaction-drilldown-log-scan.json";
    public const string ReportFileName =
        "accepted-alpha-interaction-drilldown-report.md";
    public const string NegativeProofFileName =
        "accepted-alpha-interaction-drilldown-negative-proof.json";
    public const string FileIndexFileName =
        "accepted-alpha-interaction-drilldown-file-index.json";

    public const string UnityDrilldownPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs";
    public const string UnityActionPreviewPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs";

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

public sealed record AcceptedAlphaInteractionDrilldownVerificationBuildResult
{
    public AcceptedAlphaInteractionDrilldownDashboard Dashboard { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownScriptInventory ScriptInventory { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownSmokePlan SmokePlan { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownLogScan LogScan { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownNegativeProof NegativeProof { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownFileIndex ProceduralFileIndex { get; init; } = new();
    public AcceptedAlphaInteractionDrilldownFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaInteractionDrilldownVerificationWriteResult
{
    public AcceptedAlphaInteractionDrilldownVerificationBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownDashboard
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public string FullVerificationStatus { get; init; } = "BLOCKED";
    public string UnityMenuPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath;
    public bool OneClickButtonPresent { get; init; }
    public bool DrilldownFieldsPresent { get; init; }
    public bool InteractionPreviewPresent { get; init; }
    public bool ObjectiveReplayDetailsPresent { get; init; }
    public string BatchmodeFullVerificationMarker { get; init; } = string.Empty;
    public bool CleanupScriptAvailable { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public bool HumanManualStepsReducedToOneButton { get; init; }
    public string UnityBatchmodeExecuteMethod { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityBatchmodeLogRelativePath;
    public string UnityBatchmodeLogStatus { get; init; } = "PENDING_UNITY_BATCHMODE_FULL_VERIFICATION";
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderNetworkSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public string EvidencePath { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownScriptInventory
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool OneClickButtonPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool BatchmodeFailMarkerPresent { get; init; }
    public bool DrilldownFieldsPresent { get; init; }
    public bool InteractionPreviewFieldsPresent { get; init; }
    public bool ObjectiveReplayDetailsFieldsPresent { get; init; }
    public bool VerificationEventLogPresent { get; init; }
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public bool MaterialWarningSourceClean { get; init; }
    public IReadOnlyList<AcceptedAlphaInteractionDrilldownScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaInteractionDrilldownSmokePlan
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public bool OneClickManualPath { get; init; } = true;
    public bool RefreshBaselineStep { get; init; }
    public bool BuildProjectionStep { get; init; }
    public bool PlayerProxySelectionStep { get; init; }
    public bool InteractionTargetSelectionStep { get; init; }
    public bool InteractionPreviewStep { get; init; }
    public bool ObjectiveSelectionStep { get; init; }
    public bool ObjectiveReplayDetailsStep { get; init; }
    public bool DiagnosticsMarkerStep { get; init; }
    public bool LegendStep { get; init; }
    public bool LocalSmokeStep { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<AcceptedAlphaInteractionDrilldownSmokePlanStep> Steps { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaInteractionDrilldownLogScan
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; }
    public bool MaterialInstantiationWarningAbsent { get; init; } = true;
    public bool RendererGetMaterialStackAbsent { get; init; } = true;
    public bool SmokeRequiredFieldsPresent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE_FULL_VERIFICATION";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownNegativeProof
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool LiveGeodataProviderNetworkRejected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownFileIndex
{
    public string GoalId { get; init; } =
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<AcceptedAlphaInteractionDrilldownFileIndexEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaInteractionDrilldownFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
