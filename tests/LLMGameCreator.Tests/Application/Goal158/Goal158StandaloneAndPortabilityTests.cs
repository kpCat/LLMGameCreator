using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

[Collection(Goal156Collection.Name)]
public sealed class Goal158StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_standalone_request_uses_primary_travel_hashes_frames_and_facts()
    {
        var fixture = Goal157PortableState.Value;
        var request = Assert.IsType<ProjectStandaloneBuildRequest>(fixture.Service.Request);

        Assert.Equal(fixture.Build.PackageSha256, request.PackageSha256);
        Assert.Equal(fixture.Build.CompositionPackageSha256, request.CompositionPackageSha256);
        Assert.Equal(fixture.Build.GeneratedRegionTravel?.FinalStateHash, request.FinalStateHash);
        Assert.Equal(fixture.Build.RuntimeFrames.Select(item => item.Category),
            request.RuntimeFrames.Select(item => item.Category));
        Assert.Contains(request.RuntimeFrames, item => item.Category == "generated_travel");
        Assert.Contains(request.RuntimeFrames, item => item.Category == "generated_destination_interaction");
        Assert.All(fixture.Build.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(
            request.HumanReviewFacts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
    }

    [Fact]
    public void Behavioral_payload_contains_route_destination_accepted_facts_and_primary_hash()
    {
        var fixture = Goal157PortableState.Value;
        var facts = Goal157TestKit.PayloadFacts(fixture);
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(Goal157TestKit.CapturedPayloadRoot(fixture), "player-adapter-model.json")));

        Assert.Equal(fixture.Build.GeneratedRegionTravel?.FinalStateHash,
            model.RootElement.GetProperty("finalStateHash").GetString());
        Assert.All(fixture.Build.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(fixture.Build.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Contains(facts, item => item.Label == "Регион назначения"
                                       && item.Value == fixture.Build.GeneratedRegionTravel.DestinationRegionTitle);
        Assert.Contains(facts, item => item.Label == "Release Candidate" && item.Value == "готов");
    }

    [Fact]
    public void Behavioral_release_candidate_uses_primary_travel_hashes_and_is_current()
    {
        var fixture = Goal157PortableState.Value;
        var record = Assert.IsType<GameProjectReleaseCandidateRecord>(fixture.Snapshot.ReleaseCandidate);

        Assert.Equal("CURRENT", fixture.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", fixture.Snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(fixture.Build.PackageSha256, record.PackageSha256);
        Assert.Equal(fixture.Build.CompositionPackageSha256, record.CompositionPackageSha256);
        Assert.Equal(fixture.Build.GeneratedRegionTravel?.FinalStateHash, record.FinalStateHash);
    }

    [Fact]
    public void Behavioral_portable_copy_restores_travel_accepted_and_rc_without_execution()
    {
        var fixture = Goal157PortableState.Value;
        using var copy = Goal156TestKit.Copy(fixture.Project, "goal158-portable");
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
        Assert.Equal(snapshot.GeneratedWorldActivation?.GeneratedStartMapId,
            Goal156TestKit.Load(copy.Path).Manifest.StartMapId);
    }

    [Fact]
    public void Behavioral_exactly_one_cached_hidden_standalone_smoke_when_explicitly_enabled()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var caches = Goal157TestKit.CompleteHostCaches();
        Assert.NotEmpty(caches);
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL158_RUN_SMOKE"), "true",
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

        var payload = Goal157TestKit.RealPayloadRoot(standalone);
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "game-package.json")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "project-manifest.json")));
        using var model = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "player-adapter-model.json")));
        using var frames = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "player-adapter-frames.json")));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        var categories = frames.RootElement.EnumerateArray()
            .Select(item => item.GetProperty("category").GetString() ?? string.Empty).ToList();
        Assert.Equal(snapshot.GeneratedWorldActivation?.GeneratedStartMapId,
            package.RootElement.GetProperty("manifest").GetProperty("startMapId").GetString());
        Assert.Equal(snapshot.PackageSha256, manifest.RootElement.GetProperty("packageSha256").GetString());
        Assert.Equal(snapshot.CompositionPackageSha256,
            manifest.RootElement.GetProperty("compositionPackageSha256").GetString());
        Assert.Equal(snapshot.GeneratedRegionTravel?.FinalStateHash,
            manifest.RootElement.GetProperty("finalStateHash").GetString());
        Assert.Contains("generated_travel", categories);
        Assert.Equal("generated_destination_interaction", categories[^1]);
        Assert.All(snapshot.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Contains(facts, item => item.Label == "Release Candidate" && item.Value == "готов");

        using var portable = Goal156TestKit.Copy(project, "goal158-real-portable");
        var portableUnityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var portableSnapshot = Goal157TestKit.OpenTravelWorkspace(portable.Path).Snapshot();
        Assert.Equal(portableUnityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal("TRAVEL_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.True(portableSnapshot.GeneratedWorldActivation?.Passed);
        Assert.True(portableSnapshot.GeneratedRegionTravel?.Passed);
        Assert.True(portableSnapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(
            standalone,
            snapshot,
            portableSnapshot,
            expectedHost.SequenceEqual(Goal157TestKit.TreeHashes(usedHost)),
            baselineBefore == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            goal148Before.SequenceEqual(Goal157TestKit.TreeHashes(goal148)),
            categories,
            facts);
    }

    private static void WriteCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        UnifiedGameProjectWorkspaceSnapshot portable,
        bool hostFileSetHashUnchanged,
        bool goal142SourceByteIdentical,
        bool sourceGoal148ByteIdentical,
        IReadOnlyList<string> categories,
        IReadOnlyList<(string Label, string Value)> facts)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL158_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = "goal158_cached_hidden_standalone_smoke_v1",
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
            actualPayloadGeneratedStartMapPassed = true,
            actualPayloadMapChangedRepresentationPassed = categories.Contains("generated_travel", StringComparer.Ordinal),
            actualPayloadDestinationInteractionPassed = categories.LastOrDefault() == "generated_destination_interaction",
            actualPayloadTravelFinalHashPassed = snapshot.GeneratedRegionTravel?.FinalStateHash == snapshot.FinalStateHash,
            actualPayloadTravelFactsPassed = snapshot.GeneratedRegionTravel?.HumanFacts.All(expected => facts.Contains(
                (expected.Label, expected.Value))) == true,
            actualPayloadAcceptedFactsPassed = snapshot.AcceptedMechanics?.HumanFacts.All(expected => facts.Contains(
                (expected.Label, expected.Value))) == true,
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
            releaseCandidate = snapshot.ReleaseCandidate
        }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }
}
