using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public static class ProjectStandaloneBuildVocabulary
{
    public const string SettingsRelativePath = ".llmgc/standalone-build-settings.json";
    public const string HistoryRelativePath = ".llmgc/standalone-build-history.json";
    public const string HostCacheRootName = "LLMGameCreator/StandaloneHostCache";
    public const string HostExecutableName = "LLMGameCreatorProjectHost.exe";
    public const string HostDataDirectoryName = "LLMGameCreatorProjectHost_Data";
    public const string OutputLocationKind = "short_local_appdata";
    public const string ImmutableOutputLocationKind = "immutable_short_local_appdata_run";
    public const string OperationalExecutableName = "g.exe";
    public const string OperationalDataDirectoryName = "g_Data";
    public const string CurrentOutputDirectoryName = "current";
    public const int PlayerPathBudgetLimit = 240;
}

public sealed record ProjectStandaloneOutputLocation
{
    public string Root { get; init; } = string.Empty;
    public string ProjectToken { get; init; } = string.Empty;
    public string ProjectRoot { get; init; } = string.Empty;
    public string CurrentOutputFolder { get; init; } = string.Empty;
    public string StagingOutputFolder { get; init; } = string.Empty;
    public string BackupOutputFolder { get; init; } = string.Empty;
    public string RunsFolder { get; init; } = string.Empty;
    public string RunDirectoryName { get; init; } = string.Empty;
    public string RunOutputFolder { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
    public string ExecutableName { get; init; } = ProjectStandaloneBuildVocabulary.OperationalExecutableName;
    public string DataDirectoryName { get; init; } = ProjectStandaloneBuildVocabulary.OperationalDataDirectoryName;
}

public sealed record ProjectStandaloneRunLocation
{
    public string ProjectToken { get; init; } = string.Empty;
    public string ProjectRoot { get; init; } = string.Empty;
    public string RunsFolder { get; init; } = string.Empty;
    public string RunDirectoryName { get; init; } = string.Empty;
    public string RunOutputFolder { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
}

public sealed record ProjectStandaloneCurrentPointer
{
    public string SchemaVersion { get; init; } = "standalone_current_output_v1";
    public string ProjectToken { get; init; } = string.Empty;
    public string RunDirectoryName { get; init; } = string.Empty;
    public string ExecutableRelativePath { get; init; } = "g.exe";
    public string BuildManifestRelativePath { get; init; } = "build-manifest.json";
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string HostCacheKey { get; init; } = string.Empty;
    public string PayloadSelfCheckSha256 { get; init; } = string.Empty;
    public string SmokeMarkerSha256 { get; init; } = string.Empty;
    public string PlayerLogSha256 { get; init; } = string.Empty;
    public int SmokeExitCode { get; init; } = -1;
    public string PublishedAttemptId { get; init; } = string.Empty;
}

public sealed record ProjectStandaloneRunStatus
{
    public string SchemaVersion { get; init; } = "standalone_run_status_v1";
    public string Status { get; init; } = string.Empty;
    public string AttemptId { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public bool PayloadSelfCheckPassed { get; init; }
    public bool LegacyParserCompatibilityPassed { get; init; }
    public int MaximumPlayerPathLength { get; init; }
    public int PlayerPathBudgetLimit { get; init; }
    public int SmokeExitCode { get; init; } = -1;
    public bool SmokeMarkersPassed { get; init; }
    public bool PlayerLogPresent { get; init; }
    public string HostCacheKey { get; init; } = string.Empty;
    public bool HostReused { get; init; }
    public bool HostRebuilt { get; init; }
}

public sealed record ProjectStandalonePublicationResult
{
    public bool Passed { get; init; }
    public string Stage { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
    public string CurrentPointerSha256 { get; init; } = string.Empty;
    public bool PriorCurrentPreserved { get; init; }
}

public sealed record ProjectStandaloneCurrentOutputReadResult
{
    public bool Passed { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
    public ProjectStandaloneCurrentPointer? Pointer { get; init; }
    public string RunOutputFolder { get; init; } = string.Empty;
    public string ExecutablePath { get; init; } = string.Empty;
}

public sealed record ProjectStandaloneCurrentQualifiedResultReadResult
{
    public bool Passed { get; init; }
    public ProjectStandaloneBuildResult? Result { get; init; }
    public ProjectStandaloneCurrentPointer? Pointer { get; init; }
    public string Diagnostics { get; init; } = string.Empty;
}

public sealed record ProjectStandaloneOutputPathBudgetResult
{
    public int MaximumAbsolutePathLength { get; init; }
    public string LongestRelativePath { get; init; } = string.Empty;
    public int BudgetLimit { get; init; } = ProjectStandaloneBuildVocabulary.PlayerPathBudgetLimit;
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
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
    public bool PayloadSelfCheckPassed { get; init; }
    public bool LegacyHostParserCompatibilityPassed { get; init; }
    public IReadOnlyList<string> PayloadSelfCheckFailedCodes { get; init; } = [];
    public int SmokeExitCode { get; init; } = -1;
    public string SmokeMarkerText { get; init; } = string.Empty;
    public string SmokeMarkerPath { get; init; } = string.Empty;
    public string PlayerLogPath { get; init; } = string.Empty;
    public bool PlayerLogPresent { get; init; }
    public IReadOnlyList<string> PlayerLogRelevantLines { get; init; } = [];
    public string NamedSmokeFailure { get; init; } = string.Empty;
    public string BuildManifestPath { get; init; } = string.Empty;
    public string OutputLocationKind { get; init; } = string.Empty;
    public string OutputProjectToken { get; init; } = string.Empty;
    public int MaximumPlayerPathLength { get; init; }
    public int PlayerPathBudgetLimit { get; init; }
    public bool PlayerPathBudgetPassed { get; init; }
    public bool PriorSuccessfulOutputPreserved { get; init; }
    public string OutputRunDirectoryName { get; init; } = string.Empty;
    public string CurrentPointerPath { get; init; } = string.Empty;
    public string CurrentPointerSha256 { get; init; } = string.Empty;
    public string RunStatusPath { get; init; } = string.Empty;
    public string PublicationStage { get; init; } = string.Empty;
    public string PublicationDiagnostic { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
}

public sealed record ProjectStandalonePayloadCheckResult
{
    public int Number { get; init; }
    public string Code { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record LegacyHostParserCompatibility
{
    public bool Passed { get; init; }
    public int StructuralFrameCount { get; init; }
    public int LegacyFrameCount { get; init; }
    public int StructuralHumanFactCount { get; init; }
    public int LegacyHumanFactCount { get; init; }
    public IReadOnlyList<string> FailedCodes { get; init; } = [];
}

public sealed record ProjectStandalonePayloadSelfCheckResult
{
    public bool Passed { get; init; }
    public int PassedCount { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<ProjectStandalonePayloadCheckResult> Checks { get; init; } = [];
    public LegacyHostParserCompatibility LegacyHostParserCompatibility { get; init; } = new();
    public IReadOnlyList<string> FailedCheckCodes { get; init; } = [];
}

public sealed record ProjectStandaloneSmokeResult
{
    public bool Passed { get; init; }
    public bool ProcessStarted { get; init; }
    public int ExitCode { get; init; } = -1;
    public string SmokeMarkerText { get; init; } = string.Empty;
    public string SmokeMarkerPath { get; init; } = string.Empty;
    public string PlayerLogPath { get; init; } = string.Empty;
    public bool PlayerLogPresent { get; init; }
    public IReadOnlyList<string> PlayerLogRelevantLines { get; init; } = [];
    public string NamedFailure { get; init; } = string.Empty;
}

public interface IProjectStandaloneBuildService
{
    bool BuildRunning { get; }
    ProjectStandaloneBuildResult? LastResult { get; }
    ProjectStandaloneBuildSettings LoadSettings(string projectFolder);
    ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings);
    ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default);
    ProjectStandaloneCurrentOutputReadResult LoadCurrentOutput(string projectFolder, string packageId) => new()
    {
        Diagnostic = "Standalone current output is not available for this controller."
    };
    ProjectStandaloneCurrentQualifiedResultReadResult LoadCurrentQualifiedResult(string projectFolder, string packageId) => new()
    {
        Diagnostics = "standalone.current_history_missing"
    };
    void Cancel();
    void LaunchLastBuild();
    void OpenLastBuildFolder();
}
