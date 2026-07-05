namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class AcceptedAlphaProjectionUsabilityVocabulary
{
    public const string GoalId = "goal_120_accepted_alpha_projection_usability_and_cleanup";
    public const string ScenarioId = "goal-120-accepted-alpha-projection-usability-and-cleanup";
    public const string UsabilityStatus = "GREEN";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionUsabilitySmoke";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-120-accepted-alpha-projection-usability-and-cleanup";
    public const string DocumentationPath =
        "docs/manual-acceptance/accepted-alpha-projection-usability-and-cleanup.md";
    public const string CleanupScriptPath = ".devflow/scripts/clean-unity-editor-noise.ps1";
    public const string CleanupScriptCmdPath = ".devflow/scripts/clean-unity-editor-noise.cmd";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-projection-usability-smoke.log";

    public const string DashboardFileName =
        "accepted-alpha-projection-usability-dashboard.json";
    public const string ScriptInventoryFileName =
        "accepted-alpha-projection-usability-script-inventory.json";
    public const string SmokePlanFileName =
        "accepted-alpha-projection-usability-smoke-plan.json";
    public const string CleanupScriptScanFileName =
        "accepted-alpha-projection-cleanup-script-scan.json";
    public const string ReportFileName =
        "accepted-alpha-projection-usability-report.md";
    public const string NegativeProofFileName =
        "accepted-alpha-projection-usability-negative-proof.json";
    public const string FileIndexFileName =
        "accepted-alpha-projection-usability-file-index.json";

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        ScriptInventoryFileName,
        SmokePlanFileName,
        CleanupScriptScanFileName,
        ReportFileName,
        NegativeProofFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;
}

public sealed record AcceptedAlphaProjectionUsabilityBuildResult
{
    public AcceptedAlphaProjectionUsabilityDashboard Dashboard { get; init; } = new();
    public AcceptedAlphaProjectionUsabilityScriptInventory ScriptInventory { get; init; } = new();
    public AcceptedAlphaProjectionUsabilitySmokePlan SmokePlan { get; init; } = new();
    public AcceptedAlphaProjectionCleanupScriptScan CleanupScriptScan { get; init; } = new();
    public AcceptedAlphaProjectionUsabilityNegativeProof NegativeProof { get; init; } = new();
    public AcceptedAlphaProjectionUsabilityFileIndex ProceduralFileIndex { get; init; } = new();
    public AcceptedAlphaProjectionUsabilityFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionUsabilityWriteResult
{
    public AcceptedAlphaProjectionUsabilityBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilityDashboard
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public string UsabilityStatus { get; init; } = "BLOCKED";
    public string UnityMenuPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath;
    public string CleanupScriptPath { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath;
    public string CleanupScriptCmdPath { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath;
    public bool Goal119ARemainsGreen { get; init; }
    public bool LegendPresent { get; init; }
    public bool MarkerDescriptorPresent { get; init; }
    public bool SelectionControlsPresent { get; init; }
    public bool FocusCameraControlPresent { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public bool CleanupScriptContractPassed { get; init; }
    public string UnitySmokeStatus { get; init; } = "PENDING_UNITY_BATCHMODE_SMOKE";
    public string UnityBatchmodeExecuteMethod { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.UnityBatchmodeExecuteMethod;
    public string UnityBatchmodeLogPath { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.UnityBatchmodeLogRelativePath;
    public bool DoNotStartAutomatically { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderNetworkSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public string EvidencePath { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilityScriptInventory
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScriptCount { get; init; }
    public bool UnityMenuPathPresent { get; init; }
    public bool BatchmodeMethodPresent { get; init; }
    public bool BatchmodePassMarkerPresent { get; init; }
    public bool LegendPresent { get; init; }
    public bool MarkerDescriptorPresent { get; init; }
    public bool SelectionControlsPresent { get; init; }
    public bool FocusCameraControlPresent { get; init; }
    public bool MaterialWarningGuardPresent { get; init; }
    public bool MaterialWarningSourceClean { get; init; }
    public IReadOnlyList<AcceptedAlphaProjectionUsabilityScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilityScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionUsabilitySmokePlan
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public bool RootPresentCheck { get; init; } = true;
    public bool PlayerProxySelectionCheck { get; init; }
    public bool LegendCheck { get; init; }
    public bool MarkerDescriptorCheck { get; init; }
    public bool InteractionSelectionCheck { get; init; }
    public bool ObjectiveSelectionCheck { get; init; }
    public bool DiagnosticsMarkerSelectionCheck { get; init; }
    public bool MaterialWarningGuardCheck { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<AcceptedAlphaProjectionUsabilitySmokePlanStep> Steps { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilitySmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionCleanupScriptScan
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PowerShellScriptExists { get; init; }
    public bool CmdWrapperExists { get; init; }
    public bool DryRunDefaultPresent { get; init; }
    public bool ApplySwitchPresent { get; init; }
    public bool AllowStagedSwitchPresent { get; init; }
    public bool GitStatusPorcelainAllPresent { get; init; }
    public bool RefusesStagedByDefault { get; init; }
    public bool RemovesOnlyAllowedUnityNoise { get; init; }
    public bool RestoresOnlyProjectVersion { get; init; }
    public bool NeverRemoveSourceOrPayloadExtensions { get; init; }
    public bool NoBroadGitClean { get; init; }
    public string PowerShellSha256 { get; init; } = string.Empty;
    public string CmdSha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaProjectionUsabilityNegativeProof
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool LiveGeodataProviderNetworkRejected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilityFileIndex
{
    public string GoalId { get; init; } = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<AcceptedAlphaProjectionUsabilityFileIndexEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaProjectionUsabilityFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
