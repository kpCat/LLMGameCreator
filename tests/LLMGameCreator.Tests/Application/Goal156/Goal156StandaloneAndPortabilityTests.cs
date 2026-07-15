using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[Collection(Goal156Collection.Name)]
public sealed class Goal156StandaloneAndPortabilityTests
{
    private static readonly Lazy<Goal156PortableFixture> Portable = new(Goal156PortableFixture.Create);

    [Fact]
    public void Behavioral_standalone_request_contains_generated_world_and_accepted_mechanics_facts()
    {
        var request = Portable.Value.StandaloneService.Request!;

        Assert.Contains(request.HumanReviewFacts, fact => fact.Label == "Seed"
            && fact.Value == Goal156TestKit.AllSelectable.Request.GenerationSeed);
        Assert.Contains(request.HumanReviewFacts, fact => fact.Label == "Регионы");
        Assert.Contains(request.HumanReviewFacts, fact => fact.Label == "Сгенерированный цикл");
        Assert.Contains(request.HumanReviewFacts, fact => fact.Label == "Release Candidate" && fact.Value == "готов");
        Assert.True(request.HumanReviewFacts.Count > Portable.Value.Build.AcceptedMechanics!.HumanFacts.Count);
    }

    [Fact]
    public void Behavioral_green_correlated_payload_writes_a_current_release_candidate_record()
    {
        var fixture = Portable.Value;

        Assert.Equal("GREEN", fixture.Standalone.Status);
        Assert.True(fixture.Standalone.HostReused);
        Assert.False(fixture.Standalone.HostRebuilt);
        Assert.Equal("CURRENT", fixture.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", fixture.Snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.NotNull(fixture.Snapshot.ReleaseCandidate);
        Assert.True(File.Exists(fixture.Snapshot.ReleaseCandidateRecordPath));
    }

    [Fact]
    public void Behavioral_complete_portable_copy_restores_generated_accepted_and_rc_current_without_execution()
    {
        var fixture = Portable.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "portable-complete");
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;

        var snapshot = Goal156TestKit.OpenWorkspace(copy.Path).Snapshot();

        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal("BUILD_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_authoring_change_marks_generated_and_rc_history_last_success_without_rewriting_record()
    {
        var fixture = Portable.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "portable-stale");
        var recordPath = new GameProjectReleaseCandidateRecordService().RecordPath(copy.Path);
        var before = File.ReadAllBytes(recordPath);
        var controller = Goal156TestKit.OpenWorkspace(copy.Path);
        var authoring = Goal156TestKit.Authoring(copy.Path);
        var selectedIds = authoring.Document.SelectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var selected = authoring.Library.Catalog.Modules
            .Where(module => selectedIds.Contains(module.ModuleId))
            .First(module => !authoring.Library.Catalog.Modules.Any(other =>
                selectedIds.Contains(other.ModuleId) && other.Dependencies.Contains(module.ModuleId, StringComparer.Ordinal)));

        controller.SetModuleSelected(selected.ModuleId, false);
        controller.SaveAuthoring();
        var snapshot = controller.Snapshot();

        Assert.Equal("LAST_SUCCESS", snapshot.GeneratedWorld?.Status);
        Assert.Equal("LAST_SUCCESS", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("LAST_SUCCESS", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(before, File.ReadAllBytes(recordPath));
    }

    [Fact]
    public void Behavioral_failed_standalone_attempt_preserves_the_last_current_rc_record()
    {
        var fixture = Portable.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "portable-failed-standalone");
        var recordPath = new GameProjectReleaseCandidateRecordService().RecordPath(copy.Path);
        var before = File.ReadAllBytes(recordPath);
        var failure = new CapturingFailureStandaloneService();
        var controller = Goal156TestKit.OpenWorkspace(copy.Path, failure);

        var result = controller.BuildWindowsStandalone();

        Assert.Equal("FAILED", result.Status);
        Assert.Equal(1, failure.InvocationCount);
        Assert.Equal(before, File.ReadAllBytes(recordPath));
        Assert.Equal("CURRENT", Goal156TestKit.OpenWorkspace(copy.Path).Snapshot().ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_exactly_one_real_cached_hidden_standalone_smoke_when_explicitly_enabled()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL156_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEmpty(CompleteHostCaches());
            return;
        }

        var hostBefore = CompleteHostCaches().ToDictionary(path => path, HashSet, StringComparer.OrdinalIgnoreCase);
        Assert.NotEmpty(hostBefore);
        var project = Goal156TestKit.AllSelectable;
        var generationRoot = Path.Combine(project.Path, ".llmgc", "generation");
        var sidecarsBefore = HashSet(generationRoot);
        var goal142Before = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var sourceGoal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var sourceGoal148Before = HashSet(sourceGoal148);
        var firstController = Goal156TestKit.OpenWorkspace(project.Path);
        var first = firstController.BuildAndQualify();
        var second = Goal156TestKit.OpenWorkspace(project.Path).BuildAndQualify();
        var reopened = Goal156TestKit.OpenWorkspace(project.Path).Snapshot();
        Assert.True(first.Passed && second.Passed);
        Assert.Equal(first.PackageSha256, second.PackageSha256);
        Assert.Equal(first.FinalStateHash, second.FinalStateHash);
        Assert.Equal("BUILD_CURRENT", reopened.GeneratedWorld?.Status);

        var controller = Goal156TestKit.OpenWorkspace(project.Path,
            new ProjectStandaloneBuildService(Goal156TestKit.RepositoryRoot));

        var standalone = controller.BuildWindowsStandalone();
        var snapshot = controller.Snapshot();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.True(standalone.SelfCheckTotalCount > 0);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var usedHostRoot = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(usedHostRoot, out var usedHostBefore),
            "Standalone selected a host cache that was not complete before the smoke.");
        Assert.Equal(usedHostBefore, HashSet(usedHostRoot));
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.True(snapshot.GeneratedWorld?.Passed);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.Equal(sidecarsBefore, HashSet(generationRoot));
        Assert.Equal(goal142Before, Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(sourceGoal148Before, HashSet(sourceGoal148));

        var payloadRoot = Path.Combine(standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data", "StreamingAssets",
            "LLMGameCreatorProject");
        using var payloadModel = JsonDocument.Parse(File.ReadAllText(Path.Combine(payloadRoot, "player-adapter-model.json")));
        var payloadFacts = payloadModel.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        Assert.All(snapshot.GeneratedWorld!.HumanFacts, expected => Assert.Contains(payloadFacts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(payloadFacts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));

        using var portable = Goal156TestKit.Copy(project, "real-smoke-portable");
        var portableSnapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        Assert.Equal("BUILD_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.True(portableSnapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);
        WriteCapture(standalone, snapshot, first, second, reopened, portableSnapshot,
            hostFileSetHashUnchanged: usedHostBefore == HashSet(usedHostRoot),
            sourceGoal148ByteIdentical: sourceGoal148Before == HashSet(sourceGoal148),
            goal142SourceByteIdentical: goal142Before == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            sidecarsUnchanged: sidecarsBefore == HashSet(generationRoot),
            actualPayloadGeneratedFactsPassed: true,
            actualPayloadAcceptedFactsPassed: true);
    }

    private static void WriteCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectBuildResult first,
        GameProjectBuildResult second,
        UnifiedGameProjectWorkspaceSnapshot reopened,
        UnifiedGameProjectWorkspaceSnapshot portable,
        bool hostFileSetHashUnchanged,
        bool sourceGoal148ByteIdentical,
        bool goal142SourceByteIdentical,
        bool sidecarsUnchanged,
        bool actualPayloadGeneratedFactsPassed,
        bool actualPayloadAcceptedFactsPassed)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL156_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            unityProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standaloneSelfChecksPassed = standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            standalone.SelfCheckPassedCount,
            standalone.SelfCheckTotalCount,
            standalone.OutputFolder,
            standalone.ExecutablePath,
            hostFileSetHashUnchanged,
            sourceGoal148ByteIdentical,
            goal142SourceByteIdentical,
            sidecarsUnchanged,
            allSelectableBuildPassed = first.Passed,
            allSelectableRepeatBuildDeterministic = second.Passed
                                                    && first.PackageSha256 == second.PackageSha256
                                                    && first.FinalStateHash == second.FinalStateHash,
            allSelectableFreshReopenCurrent = reopened.GeneratedWorld?.Status == "BUILD_CURRENT",
            allSelectableSelectedMechanicCount = first.SelectedMechanicCount,
            allSelectableExplicitParameterCount = first.ConfiguredParameterCount,
            acceptedMechanicsPassed = snapshot.AcceptedMechanics?.Passed,
            generatedSummaryPassed = snapshot.GeneratedWorld?.Passed,
            actualPayloadGeneratedFactsPassed,
            actualPayloadAcceptedFactsPassed,
            rcConfigurationStatus = snapshot.ReleaseCandidateConfigurationStatus,
            rcRecordConfigurationStatus = snapshot.ReleaseCandidateRecordConfigurationStatus,
            portableCopyCurrent = portable.GeneratedWorld?.Status == "BUILD_CURRENT"
                                  && portable.AcceptedMechanics?.Passed == true
                                  && portable.ReleaseCandidateConfigurationStatus == "CURRENT",
            generatedWorld = snapshot.GeneratedWorld,
            acceptedMechanics = snapshot.AcceptedMechanics,
            releaseCandidate = snapshot.ReleaseCandidate
        }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }

    private static IReadOnlyList<string> CompleteHostCaches()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        if (!Directory.Exists(root)) return [];
        return Directory.EnumerateDirectories(root).Select(path => Path.Combine(path, "host"))
            .Where(path => File.Exists(Path.Combine(path, ProjectStandaloneBuildVocabulary.HostExecutableName))
                           && Directory.Exists(Path.Combine(path, ProjectStandaloneBuildVocabulary.HostDataDirectoryName))
                           && File.Exists(Path.Combine(path, "UnityPlayer.dll"))
                           && Directory.Exists(Path.Combine(path, "MonoBleedingEdge"))
                           && File.Exists(Path.Combine(path, "host-cache-manifest.json")))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string HostRoot(string key) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProjectStandaloneBuildVocabulary.HostCacheRootName, key, "host");

    private static string HashSet(string root) => string.Join("\n", Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + Goal156TestKit.Hash(path)));
}

internal sealed record Goal156PortableFixture(
    GeneratedProject Project,
    CapturingPayloadStandaloneService StandaloneService,
    GameProjectBuildResult Build,
    ProjectStandaloneBuildResult Standalone,
    UnifiedGameProjectWorkspaceSnapshot Snapshot)
{
    public static Goal156PortableFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "portable-fixture");
        var service = new CapturingPayloadStandaloneService();
        var controller = Goal156TestKit.OpenWorkspace(project.Path, service);
        var standalone = controller.BuildWindowsStandalone();
        var build = controller.LastBuild ?? throw new InvalidOperationException("build was not captured");
        return new Goal156PortableFixture(project, service, build, standalone, controller.Snapshot());
    }
}

internal sealed class CapturingPayloadStandaloneService : IProjectStandaloneBuildService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public bool BuildRunning => false;
    public ProjectStandaloneBuildResult? LastResult { get; private set; }
    public ProjectStandaloneBuildRequest? Request { get; private set; }
    public ProjectStandaloneBuildSettings LoadSettings(string projectFolder) => new();
    public ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings) => settings;

    public ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default)
    {
        Request = request;
        var slug = SafeSlug(request.ProjectPackageId);
        var payload = Path.Combine(request.ProjectFolder, "Builds", "Windows", slug,
            slug + "_Data", "StreamingAssets", "LLMGameCreatorProject");
        Directory.CreateDirectory(payload);
        File.WriteAllText(Path.Combine(payload, "project-manifest.json"), JsonSerializer.Serialize(new
        {
            request.PackageSha256,
            request.CompositionPackageSha256,
            request.FinalStateHash
        }, JsonOptions), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(payload, "player-adapter-model.json"), JsonSerializer.Serialize(new
        {
            request.FinalStateHash,
            humanReviewFacts = request.HumanReviewFacts
        }, JsonOptions), new UTF8Encoding(false));
        LastResult = new ProjectStandaloneBuildResult
        {
            AttemptId = Guid.NewGuid().ToString("N"),
            Status = "GREEN",
            Stage = "captured_payload",
            ProjectFolder = request.ProjectFolder,
            OutputFolder = Path.GetFullPath(Path.Combine(payload, "..", "..", "..")),
            PackageSha256 = request.PackageSha256,
            FinalStateHash = request.FinalStateHash,
            HostCacheKey = "goal156-captured-cache",
            HostReused = true,
            HostRebuilt = false,
            LaunchSmokePassed = true,
            SelfCheckPassedCount = 3,
            SelfCheckTotalCount = 3
        };
        return LastResult;
    }

    public void Cancel() { }
    public void LaunchLastBuild() => throw new InvalidOperationException("capturing service does not launch");
    public void OpenLastBuildFolder() => throw new InvalidOperationException("capturing service does not open folders");

    private static string SafeSlug(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        return builder.ToString().Trim('-');
    }
}

internal sealed class CapturingFailureStandaloneService : IProjectStandaloneBuildService
{
    public bool BuildRunning => false;
    public ProjectStandaloneBuildResult? LastResult { get; private set; }
    public int InvocationCount { get; private set; }
    public ProjectStandaloneBuildSettings LoadSettings(string projectFolder) => new();
    public ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings) => settings;
    public ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default)
    {
        InvocationCount++;
        return LastResult = new ProjectStandaloneBuildResult
        {
            Status = "FAILED",
            Stage = "captured_failure",
            ProjectFolder = request.ProjectFolder,
            PackageSha256 = request.PackageSha256,
            FinalStateHash = request.FinalStateHash
        };
    }
    public void Cancel() { }
    public void LaunchLastBuild() => throw new InvalidOperationException();
    public void OpenLastBuildFolder() => throw new InvalidOperationException();
}
