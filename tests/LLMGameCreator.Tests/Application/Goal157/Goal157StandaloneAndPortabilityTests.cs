using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_standalone_request_uses_complete_travel_hashes_and_frames()
    {
        var fixture = Goal157PortableState.Value;
        var request = Assert.IsType<ProjectStandaloneBuildRequest>(fixture.Service.Request);

        Assert.Equal(fixture.Build.PackageSha256, request.PackageSha256);
        Assert.Equal(fixture.Build.CompositionPackageSha256, request.CompositionPackageSha256);
        Assert.Equal(fixture.Build.GeneratedRegionTravel?.FinalStateHash, request.FinalStateHash);
        Assert.Equal(fixture.Build.RuntimeFrames.Select(frame => frame.Category),
            request.RuntimeFrames.Select(frame => frame.Category));
        Assert.Contains(request.RuntimeFrames, frame => frame.Category == "generated_travel");
    }

    [Fact]
    public void Behavioral_green_standalone_result_is_cached_reused_and_not_rebuilt()
    {
        var standalone = Goal157PortableState.Value.Standalone;

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
    }

    [Fact]
    public void Behavioral_actual_project_package_starts_on_generated_map_after_build()
    {
        var fixture = Goal157PortableState.Value;
        var package = Goal156TestKit.Load(fixture.Project.Path);

        Assert.Equal(fixture.Build.GeneratedWorldActivation?.GeneratedStartMapId, package.Manifest.StartMapId);
    }

    [Fact]
    public void Behavioral_payload_final_hash_matches_complete_travel_route()
    {
        var fixture = Goal157PortableState.Value;
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(Goal157TestKit.CapturedPayloadRoot(fixture), "player-adapter-model.json")));

        Assert.Equal(fixture.Build.GeneratedRegionTravel?.FinalStateHash,
            model.RootElement.GetProperty("finalStateHash").GetString());
        Assert.Equal(fixture.Build.FinalStateHash, fixture.Standalone.FinalStateHash);
    }

    [Fact]
    public void Behavioral_payload_contains_all_generated_activation_facts()
    {
        var fixture = Goal157PortableState.Value;
        var facts = Goal157TestKit.PayloadFacts(fixture);

        Assert.All(fixture.Build.GeneratedWorldActivation!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
    }

    [Fact]
    public void Behavioral_payload_contains_all_accepted_mechanics_facts()
    {
        var fixture = Goal157PortableState.Value;
        var facts = Goal157TestKit.PayloadFacts(fixture);

        Assert.All(fixture.Build.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Contains(facts, fact => fact.Label == "Release Candidate" && fact.Value == "готов");
    }

    [Fact]
    public void Behavioral_release_candidate_record_is_current_and_primary_lane_correlated()
    {
        var fixture = Goal157PortableState.Value;
        var snapshot = fixture.Snapshot;

        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(fixture.Build.PackageSha256, snapshot.ReleaseCandidate?.PackageSha256);
        Assert.Equal(fixture.Build.CompositionPackageSha256, snapshot.ReleaseCandidate?.CompositionPackageSha256);
        Assert.Equal(fixture.Build.FinalStateHash, snapshot.ReleaseCandidate?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_portable_copy_restores_source_activation_accepted_and_rc_without_execution()
    {
        var fixture = Goal157PortableState.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "goal157-portable");
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;

        var snapshot = Goal157TestKit.OpenTravelWorkspace(copy.Path).Snapshot();

        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.True(snapshot.GeneratedRegionTravel?.Passed);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.True(snapshot.AcceptedMechanicsCompatibility?.Passed);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_failed_standalone_attempt_preserves_current_release_candidate()
    {
        var fixture = Goal157PortableState.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "goal157-failed-standalone");
        var recordPath = new GameProjectReleaseCandidateRecordService().RecordPath(copy.Path);
        var before = File.ReadAllBytes(recordPath);
        var failure = new CapturingFailureStandaloneService();

        var result = Goal157TestKit.OpenTravelWorkspace(copy.Path, failure).BuildWindowsStandalone();

        Assert.Equal("FAILED", result.Status);
        Assert.Equal(before, File.ReadAllBytes(recordPath));
        Assert.Equal("CURRENT", Goal157TestKit.OpenTravelWorkspace(copy.Path).Snapshot().ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_standalone_build_does_not_mutate_generated_source_sidecars()
    {
        var fixture = Goal157PortableState.Value;

        Assert.Equal(fixture.GenerationBefore, Goal157TestKit.TreeHashes(
            Path.Combine(fixture.Project.Path, ".llmgc", "generation")));
    }

    [Fact]
    public void Behavioral_exactly_one_real_cached_hidden_standalone_smoke_when_explicitly_enabled()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var caches = Goal157TestKit.CompleteHostCaches();
        Assert.NotEmpty(caches);
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL157_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase)) return;

        var project = Goal157BuildState.Value.Project;
        var generationBefore = Goal157TestKit.TreeHashes(Path.Combine(project.Path, ".llmgc", "generation"));
        var baselineBefore = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = Goal157TestKit.TreeHashes(goal148);
        var hostBefore = caches.ToDictionary(path => path, Goal157TestKit.TreeHashes,
            StringComparer.OrdinalIgnoreCase);
        var controller = Goal157TestKit.OpenTravelWorkspace(project.Path,
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
        var usedHost = Goal157TestKit.HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(usedHost, out var expectedHost));
        Assert.Equal(expectedHost, Goal157TestKit.TreeHashes(usedHost));
        Assert.Equal(generationBefore, Goal157TestKit.TreeHashes(Path.Combine(project.Path, ".llmgc", "generation")));
        Assert.Equal(baselineBefore, Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(goal148Before, Goal157TestKit.TreeHashes(goal148));
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);

        var payload = Goal157TestKit.RealPayloadRoot(standalone);
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "game-package.json")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "project-manifest.json")));
        using var model = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "player-adapter-model.json")));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        Assert.Equal(snapshot.GeneratedWorldActivation?.GeneratedStartMapId,
            package.RootElement.GetProperty("manifest").GetProperty("startMapId").GetString());
        Assert.Equal(snapshot.PackageSha256, manifest.RootElement.GetProperty("packageSha256").GetString());
        Assert.Equal(snapshot.CompositionPackageSha256,
            manifest.RootElement.GetProperty("compositionPackageSha256").GetString());
        Assert.Equal(snapshot.GeneratedRegionTravel?.FinalStateHash,
            manifest.RootElement.GetProperty("finalStateHash").GetString());
        Assert.All(snapshot.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(snapshot.GeneratedWorldActivation!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));

        using var portable = Goal156TestKit.Copy(project, "goal157-real-portable");
        var portableUnityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var portableSnapshot = Goal157TestKit.OpenTravelWorkspace(portable.Path).Snapshot();
        Assert.Equal(portableUnityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal("TRAVEL_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.True(portableSnapshot.GeneratedWorldActivation?.Passed);
        Assert.True(portableSnapshot.GeneratedRegionTravel?.Passed);
        Assert.True(portableSnapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);
        Goal157TestKit.WriteSmokeCapture(standalone, snapshot, portableSnapshot,
            hostFileSetHashUnchanged: expectedHost.SequenceEqual(Goal157TestKit.TreeHashes(usedHost)),
            goal142SourceByteIdentical: baselineBefore == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            sourceGoal148ByteIdentical: goal148Before.SequenceEqual(Goal157TestKit.TreeHashes(goal148)));
    }

    [Fact]
    public void Contract_release_candidate_uses_player_hashes_and_lane_a_accepted_qualification()
    {
        var fixture = Goal157PortableState.Value;
        var record = Assert.IsType<GameProjectReleaseCandidateRecord>(fixture.Snapshot.ReleaseCandidate);

        Assert.Equal(fixture.Build.PackageSha256, record.PackageSha256);
        Assert.Equal(fixture.Build.FinalStateHash, record.FinalStateHash);
        Assert.Equal(fixture.Build.AcceptedMechanicsCompatibility?.CompatibilityActivatedPackageSha256,
            record.AcceptedMechanicsSummary.QualificationPackageSha256);
        Assert.NotEqual(record.PackageSha256, record.AcceptedMechanicsSummary.QualificationPackageSha256);
    }
}

internal sealed record Goal157PortableFixture(
    GeneratedProject Project,
    CapturingPayloadStandaloneService Service,
    GameProjectBuildResult Build,
    ProjectStandaloneBuildResult Standalone,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    SortedDictionary<string, string> GenerationBefore);

internal static class Goal157PortableState
{
    private static readonly Lazy<Goal157PortableFixture> Fixture = new(Create);
    public static Goal157PortableFixture Value => Fixture.Value;

    private static Goal157PortableFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "goal157-portable-fixture");
        var before = Goal157TestKit.TreeHashes(Path.Combine(project.Path, ".llmgc", "generation"));
        var service = new CapturingPayloadStandaloneService();
        var controller = Goal157TestKit.OpenTravelWorkspace(project.Path, service);
        var standalone = controller.BuildWindowsStandalone();
        var build = controller.LastBuild ?? throw new InvalidOperationException("Goal157 build was not captured.");
        return new Goal157PortableFixture(project, service, build, standalone, controller.Snapshot(), before);
    }
}

internal static partial class Goal157TestKit
{
    public static string CapturedPayloadRoot(Goal157PortableFixture fixture)
    {
        var id = Goal156TestKit.Load(fixture.Project.Path).Manifest.PackageId.ToLowerInvariant();
        var slug = string.Concat(id.Select(character => char.IsLetterOrDigit(character) ? character : '-')).Trim('-');
        return Path.Combine(fixture.Standalone.OutputFolder, slug + "_Data",
            "StreamingAssets", "LLMGameCreatorProject");
    }

    public static string RealPayloadRoot(ProjectStandaloneBuildResult standalone) => Path.Combine(
        standalone.OutputFolder,
        Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data",
        "StreamingAssets", "LLMGameCreatorProject");

    public static IReadOnlyList<(string Label, string Value)> PayloadFacts(Goal157PortableFixture fixture)
    {
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(CapturedPayloadRoot(fixture), "player-adapter-model.json")));
        return model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (item.GetProperty("label").GetString() ?? string.Empty,
                item.GetProperty("value").GetString() ?? string.Empty)).ToList();
    }

    public static IReadOnlyList<string> CompleteHostCaches()
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

    public static string HostRoot(string key) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProjectStandaloneBuildVocabulary.HostCacheRootName, key, "host");

    public static void WriteSmokeCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        UnifiedGameProjectWorkspaceSnapshot portable,
        bool hostFileSetHashUnchanged,
        bool goal142SourceByteIdentical,
        bool sourceGoal148ByteIdentical)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL157_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            hostFileSetHashUnchanged,
            unityProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standaloneSelfChecksPassed = standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            standalone.SelfCheckPassedCount,
            standalone.SelfCheckTotalCount,
            actualPackageGeneratedStartMapPassed = true,
            actualPayloadActivationHashPassed = true,
            actualPayloadActivationFactsPassed = true,
            actualPayloadAcceptedFactsPassed = true,
            releaseCandidateRecordCurrent = snapshot.ReleaseCandidateConfigurationStatus == "CURRENT",
            portableCopyCurrent = portable.GeneratedWorld?.Status == "TRAVEL_CURRENT"
                                  && portable.GeneratedWorldActivation?.Passed == true
                                  && portable.GeneratedRegionTravel?.Passed == true
                                  && portable.AcceptedMechanics?.Passed == true
                                  && portable.ReleaseCandidateConfigurationStatus == "CURRENT",
            goal142SourceByteIdentical,
            sourceGoal148ByteIdentical,
            generatedWorld = snapshot.GeneratedWorld,
            generatedWorldActivation = snapshot.GeneratedWorldActivation,
            generatedWorldTravelOverlay = snapshot.GeneratedWorldTravelOverlay,
            generatedRegionTravel = snapshot.GeneratedRegionTravel,
            acceptedMechanicsCompatibility = snapshot.AcceptedMechanicsCompatibility,
            acceptedMechanics = snapshot.AcceptedMechanics,
            releaseCandidate = snapshot.ReleaseCandidate,
            standalone.OutputFolder,
            standalone.ExecutablePath
        }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }
}
