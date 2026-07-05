namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class AcceptedAlphaUnityMaterialWarningHotfixVocabulary
{
    public const string GoalId = "goal_119a_accepted_alpha_unity_material_warning_hotfix";
    public const string ScenarioId = "goal-119a-accepted-alpha-unity-material-warning-hotfix";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-119a-accepted-alpha-unity-material-warning-hotfix";
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/unity-batchmode-projection-smoke.log";

    public const string DashboardFileName =
        "accepted-alpha-unity-material-warning-hotfix-dashboard.json";
    public const string LogScanFileName =
        "accepted-alpha-unity-material-warning-hotfix-log-scan.json";
    public const string ScriptScanFileName =
        "accepted-alpha-unity-material-warning-hotfix-script-scan.json";
    public const string ReportFileName =
        "accepted-alpha-unity-material-warning-hotfix-report.md";
    public const string NegativeProofFileName =
        "accepted-alpha-unity-material-warning-hotfix-negative-proof.json";
    public const string FileIndexFileName =
        "accepted-alpha-unity-material-warning-hotfix-file-index.json";
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixBuildResult
{
    public AcceptedAlphaUnityMaterialWarningHotfixDashboard Dashboard { get; init; } = new();
    public AcceptedAlphaUnityMaterialWarningHotfixLogScan LogScan { get; init; } = new();
    public AcceptedAlphaUnityMaterialWarningHotfixScriptScan ScriptScan { get; init; } = new();
    public AcceptedAlphaUnityMaterialWarningHotfixNegativeProof NegativeProof { get; init; } = new();
    public AcceptedAlphaUnityMaterialWarningHotfixFileIndex ProceduralFileIndex { get; init; } = new();
    public AcceptedAlphaUnityMaterialWarningHotfixFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixWriteResult
{
    public AcceptedAlphaUnityMaterialWarningHotfixBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixDashboard
{
    public string GoalId { get; init; } = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.GoalId;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public string UnitySmokeStatus { get; init; } = "BLOCKED_PENDING_UNITY_BATCHMODE_SMOKE";
    public string UnityBatchmodeExecuteMethod { get; init; } =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionSmoke";
    public string UnityBatchmodeLogPath { get; init; } =
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.UnityBatchmodeLogRelativePath;
    public bool UnityLogExists { get; init; }
    public bool UnityLogContainsPassMarker { get; init; }
    public bool MaterialWarningAbsent { get; init; }
    public bool RendererMaterialSourceAccessAbsent { get; init; }
    public bool MaterialAssignmentSourceAccessAbsent { get; init; }
    public bool MaterialPropertyBlockUsed { get; init; }
    public bool ColorAndBaseColorPropertyBlocksSet { get; init; }
    public bool NoPerMarkerMaterialInstantiation { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderNetworkSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; } = true;
    public string EvidencePath { get; init; } =
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixLogScan
{
    public string GoalId { get; init; } = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.GoalId;
    public string RelativePath { get; init; } =
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.UnityBatchmodeLogRelativePath;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; }
    public bool MaterialInstantiationWarningAbsent { get; init; }
    public bool RendererGetMaterialStackAbsent { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "BLOCKED_PENDING_UNITY_BATCHMODE_SMOKE";
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixScriptScan
{
    public string GoalId { get; init; } = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public bool RendererMaterialAccessAbsent { get; init; }
    public bool MaterialAssignmentAbsent { get; init; }
    public bool MaterialPropertyBlockUsed { get; init; }
    public bool ColorPropertySet { get; init; }
    public bool BaseColorPropertySet { get; init; }
    public bool NoNewMaterialInPrimitiveFactory { get; init; }
    public IReadOnlyList<AcceptedAlphaUnityMaterialWarningHotfixScriptScanEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixScriptScanEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool ContainsRendererMaterialAccess { get; init; }
    public bool ContainsMaterialAssignment { get; init; }
    public bool ContainsNewMaterial { get; init; }
    public bool ContainsMaterialPropertyBlock { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixNegativeProof
{
    public string GoalId { get; init; } = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool LiveGeodataProviderNetworkRejected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixFileIndex
{
    public string GoalId { get; init; } = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<AcceptedAlphaUnityMaterialWarningHotfixFileIndexEntry> Files { get; init; } = [];
}

public sealed record AcceptedAlphaUnityMaterialWarningHotfixFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
