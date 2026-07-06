using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class AcceptedAlphaUnityPlayableProjectionVocabulary
{
    public const string GoalId = "goal_119_accepted_alpha_unity_playable_projection";
    public const string ProductSmokeRoute = "goal-119-accepted-alpha-unity-playable-projection";
    public const string ProjectionStatus = "GREEN";
    public const string UnityMenuPath =
        "LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection";
    public const string BaselineId = "offline_geoworld_alpha_accepted_baseline_v1";
    public const string ManualGateStatusAccepted = "ACCEPTED_BY_HUMAN";
    public const string GeneratedRootName = "__LLMGC_AcceptedAlphaPlayableProjection__";

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-119-accepted-alpha-unity-playable-projection";
    public const string DocumentationPath =
        "docs/manual-acceptance/accepted-alpha-unity-playable-projection.md";

    public const string DashboardFileName =
        "accepted-alpha-unity-playable-projection-dashboard.json";
    public const string ScriptInventoryFileName =
        "accepted-alpha-unity-playable-projection-script-inventory.json";
    public const string SmokePlanFileName =
        "accepted-alpha-unity-playable-projection-smoke-plan.json";
    public const string ReportFileName =
        "accepted-alpha-unity-playable-projection-report.md";
    public const string QualityGateScanFileName =
        "accepted-alpha-unity-playable-projection-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "accepted-alpha-unity-playable-projection-negative-proof.json";
    public const string FileIndexFileName =
        "accepted-alpha-unity-playable-projection-file-index.json";

    public const string Goal118DashboardPath =
        ".llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/"
        + "offline-geoworld-accepted-alpha-baseline-dashboard.json";
    public const string Goal116AcceptanceRecordPath =
        ".llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/"
        + "offline-geoworld-alpha-manual-gate-acceptance-record.json";

    public const string UnityEditorWindowPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs";
    public const string UnityControllerPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs";
    public const string UnityDiagnosticsPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs";
    public const string UnityModelsPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs";
    public const string UnityPrimitiveFactoryPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs";
    public const string UnityDrilldownPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs";
    public const string UnityActionPreviewPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs";

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        ScriptInventoryFileName,
        SmokePlanFileName,
        ReportFileName,
        QualityGateScanFileName,
        NegativeProofFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;

    public static IReadOnlyList<string> UnityScriptPaths =>
    [
        UnityEditorWindowPath,
        UnityControllerPath,
        UnityDiagnosticsPath,
        UnityModelsPath,
        UnityPrimitiveFactoryPath,
        UnityDrilldownPath,
        UnityActionPreviewPath
    ];
}

public sealed record AcceptedAlphaUnityPlayableProjectionBuildResult
{
    public AcceptedAlphaUnityPlayableProjectionDashboard Dashboard { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionScriptInventory ScriptInventory { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionSmokePlan SmokePlan { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionQualityGateScan QualityGateScan { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionNegativeProof NegativeProof { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionFileIndex ProceduralFileIndex { get; init; } = new();
    public AcceptedAlphaUnityPlayableProjectionFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityPlayableProjectionWriteResult
{
    public AcceptedAlphaUnityPlayableProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionDashboard
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public string ProjectionStatus { get; init; } = "BLOCKED";
    public string UnityMenuPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath;
    public string BaselineId { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.BaselineId;
    public bool AcceptedBaselineReady { get; init; }
    public string ManualGateStatus { get; init; } = string.Empty;
    public string ExpectedGeneratedRootName { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName;
    public int ScriptInventoryCount { get; init; }
    public int SmokePlanStepCount { get; init; }
    public int PreviewCommandCount { get; init; }
    public int ChunkWindowStepCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public int InteractionTargetCount { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public int ReplayStepCount { get; init; }
    public bool ForbiddenUnitySurfaceClean { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderNetworkSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public string EvidencePath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory;
    public string HandsOnVerification { get; init; } =
        "Open Unity and select LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection.";
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionScriptInventory
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public bool MenuPathExistsExactly { get; init; }
    public int ScriptCount { get; init; }
    public bool AllScriptsPresent { get; init; }
    public bool NoForbiddenUnityPathsExpected { get; init; }
    public IReadOnlyList<AcceptedAlphaUnityPlayableProjectionScriptInventoryEntry> Scripts { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionScriptInventoryEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool Required { get; init; } = true;
    public bool ContainsRequiredMarker { get; init; }
    public string RequiredMarker { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityPlayableProjectionSmokePlan
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public string UnityMenuPath { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath;
    public string ExpectedGeneratedRootName { get; init; } =
        AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName;
    public bool BaselineLoaded { get; init; }
    public bool HasPlayerProxyStep { get; init; }
    public bool HasChunkWindowStep { get; init; }
    public bool HasInteractionOrObjectiveStep { get; init; }
    public bool HasDiagnosticsStatusStep { get; init; }
    public bool ZeroFatalErrorsExpected { get; init; } = true;
    public int StepCount { get; init; }
    public IReadOnlyList<AcceptedAlphaUnityPlayableProjectionSmokePlanStep> Steps { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionSmokePlanStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityPlayableProjectionQualityGateScan
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Passed { get; init; }
    public bool MenuPathExistsExactly { get; init; }
    public bool NewUnityScriptsPresent { get; init; }
    public bool ProjectionRootNamePresent { get; init; }
    public bool BaselineFromGoal118Ready { get; init; }
    public bool Goal116ManualGateAccepted { get; init; }
    public bool SmokePlanCoversRequiredChecks { get; init; }
    public bool ForbiddenUnitySurfaceClean { get; init; }
    public bool NoProjectSettingsPackagesStreamingAssetsExpected { get; init; }
    public bool NoRuntimeSchemaProviderLuaGeneratorLibraryExpected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public bool NotFinalReleaseOrRuntimeBuild { get; init; }
    public bool NegativeProofPassed { get; init; }
    public int ExpectedChangedPathCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPaths { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionNegativeProof
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool LiveGeodataProviderNetworkRejected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityPlayableProjectionFileIndex
{
    public string GoalId { get; init; } = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<AcceptedAlphaUnityPlayableProjectionFileIndexEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaUnityPlayableProjectionFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}

internal sealed record AcceptedAlphaUnityPlayableProjectionPayloadSummary(
    int PreviewCommandCount,
    int ChunkWindowStepCount,
    int BoundaryCrossingCount,
    int InteractionTargetCount,
    int ObjectiveCount,
    int CompletedObjectiveCount,
    int ReplayStepCount);

internal sealed record AcceptedAlphaUnityPlayableProjectionBaselineSummary(
    string BaselineId,
    bool AcceptedBaselineReady,
    string ManualGateStatus,
    bool Goal116Accepted);
