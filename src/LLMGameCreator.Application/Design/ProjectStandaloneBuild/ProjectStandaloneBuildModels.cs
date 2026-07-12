using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public static class ProjectStandaloneBuildVocabulary
{
    public const string SettingsRelativePath = ".llmgc/standalone-build-settings.json";
    public const string HistoryRelativePath = ".llmgc/standalone-build-history.json";
    public const string HostCacheRootName = "LLMGameCreator/StandaloneHostCache";
    public const string HostExecutableName = "LLMGameCreatorProjectHost.exe";
    public const string HostDataDirectoryName = "LLMGameCreatorProjectHost_Data";
}

public sealed record ProjectStandaloneBuildSettings
{
    public string UnityEditorPath { get; init; } = string.Empty;
    public bool DevelopmentBuild { get; init; }
    public bool AllowDebugging { get; init; }
    public bool ConnectProfiler { get; init; }
}

public sealed record ProjectStandaloneBuildRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ProjectTitle { get; init; } = string.Empty;
    public string ProjectPackageId { get; init; } = string.Empty;
    public string ProjectVersion { get; init; } = string.Empty;
    public string CompositionId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedModuleIds { get; init; } = [];
    public IReadOnlyList<StandaloneParameterValue> Parameters { get; init; } = [];
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string RuntimePlanId { get; init; } = string.Empty;
    public int CapabilityCount { get; init; }
    public int RequiredMechanicCount { get; init; }
    public int SelectedOptionalMechanicCount { get; init; }
    public int ActiveMechanicCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public int PlannedActionCount { get; init; }
    public int CheckpointActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public string EquipmentSummary { get; init; } = string.Empty;
    public string AttributesSummary { get; init; } = string.Empty;
    public string ProgressionSummary { get; init; } = string.Empty;
    public int EquipmentDamageBonus { get; init; }
    public decimal StatDamageBonus { get; init; }
    public decimal TotalAdditionalDamage { get; init; }
    public IReadOnlyList<StandaloneHumanReviewFact> HumanReviewFacts { get; init; } = [];
    public IReadOnlyList<StandaloneRuntimeFrame> RuntimeFrames { get; init; } = [];
}

public sealed record StandaloneHumanReviewFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record StandaloneRuntimeFrame
{
    public int Index { get; init; }
    public string ActionId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string StateHash { get; init; } = string.Empty;
}

public sealed record StandaloneParameterValue
{
    public string ModuleId { get; init; } = string.Empty;
    public string ParameterId { get; init; } = string.Empty;
    public JsonElement Value { get; init; }
}

public sealed record ProjectStandaloneBuildResult
{
    public string AttemptId { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string ProjectFolder { get; init; } = string.Empty;
    public string OutputFolder { get; init; } = string.Empty;
    public string ExecutablePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int SelectedModuleCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public string RuntimePlanId { get; init; } = string.Empty;
    public int CapabilityCount { get; init; }
    public int FrameCount { get; init; }
    public int SelfCheckPassedCount { get; init; }
    public int SelfCheckTotalCount { get; init; }
    public string UnityEditorPath { get; init; } = string.Empty;
    public string UnityVersion { get; init; } = string.Empty;
    public string HostCacheKey { get; init; } = string.Empty;
    public bool HostRebuilt { get; init; }
    public bool HostReused { get; init; }
    public bool LaunchSmokePassed { get; init; }
    public string BuildManifestPath { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
}

public interface IProjectStandaloneBuildService
{
    bool BuildRunning { get; }
    ProjectStandaloneBuildResult? LastResult { get; }
    ProjectStandaloneBuildSettings LoadSettings(string projectFolder);
    ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings);
    ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default);
    void Cancel();
    void LaunchLastBuild();
    void OpenLastBuildFolder();
}
