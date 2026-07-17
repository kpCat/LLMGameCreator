using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_regenerated_project_fresh_reopen_is_travel_current()
    {
        var fixture = Goal159SuccessState.Value;
        var reopened = Goal157TestKit.OpenTravelWorkspace(fixture.Project.Path).Snapshot();

        Assert.Equal("CAMPAIGN_CURRENT", reopened.GeneratedWorld?.Status);
        Assert.True(reopened.GeneratedWorldActivation?.Passed);
        Assert.True(reopened.GeneratedRegionTravel?.Passed);
        Assert.True(reopened.AcceptedMechanics?.Passed);
    }

    [Fact]
    public void Behavioral_regenerated_build_with_old_rc_is_standalone_pending()
    {
        var snapshot = Goal159SuccessState.Value.Result.AuthoritativeSnapshot!;

        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("LAST_SUCCESS", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_apply_retains_old_rc_bytes_and_marks_last_success()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.OldReleaseCandidateBytes, fixture.NewReleaseCandidateBytes);
        Assert.Equal("LAST_SUCCESS", fixture.Record.PreviousReleaseCandidateStatus);
        Assert.Equal(Goal156TestKit.Hash(new GameProjectReleaseCandidateRecordService().RecordPath(fixture.Project.Path)),
            fixture.Record.PreviousReleaseCandidateRecordSha256);
    }

    [Fact]
    public void Behavioral_apply_retains_old_histories_and_adds_one_green_history()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.OldHistoryHashes.Count + 1, fixture.NewHistoryHashes.Count);
        Assert.All(fixture.OldHistoryHashes, pair => Assert.Equal(pair.Value, fixture.NewHistoryHashes[pair.Key]));
        Assert.Contains(fixture.Record.CandidateBuildHistoryFileName, fixture.NewHistoryHashes.Keys);
        Assert.Equal("GREEN", fixture.Record.Status);
    }

    [Fact]
    public void Behavioral_regeneration_record_and_source_are_current_after_reopen()
    {
        var fixture = Goal159SuccessState.Value;
        var record = fixture.Bundle.Record.Read(fixture.Project.Path);

        Assert.True(record.Passed, string.Join(Environment.NewLine, record.Diagnostics));
        Assert.Equal(Goal156TestKit.Hash(Goal159TestKit.SourcePath(fixture.Project.Path)),
            record.Record?.NewSourceRecordSha256);
        Assert.Equal(fixture.Source.Source?.PlanSha256, record.Record?.NewPlanSha256);
        Assert.Equal(fixture.Source.Source?.GeneratedBasePackageSha256,
            record.Record?.NewGeneratedBaseSha256);
    }

    [Fact]
    public void Behavioral_apply_deletes_candidate_and_leaves_committed_journal_evidence()
    {
        var fixture = Goal159SuccessState.Value;
        var journal = Directory.EnumerateFiles(Path.Combine(fixture.Project.Path,
                GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot), "journal.json",
            SearchOption.AllDirectories).Single(path => path.Contains(fixture.Record.AttemptId,
            StringComparison.Ordinal));

        Assert.False(Directory.Exists(fixture.CandidateRoot));
        Assert.Contains("\"state\": \"committed\"", File.ReadAllText(journal, Encoding.UTF8));
    }

    [Fact]
    public void Behavioral_portable_copy_restores_v2_regeneration_travel_and_accepted_without_execution()
    {
        var fixture = Goal159SuccessState.Value;
        using var portable = Goal156TestKit.Copy(fixture.Project, "goal159-portable-before-standalone");
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;

        var source = Goal156TestKit.SourceService.Validate(portable.Path);
        var snapshot = Goal157TestKit.OpenTravelWorkspace(portable.Path).Snapshot();
        var record = new GameProjectSeedRegenerationRecordService(
            Goal156TestKit.RepositoryRoot, Goal156TestKit.SourceService).Read(portable.Path);

        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceV2SchemaVersion, source.Source?.SchemaVersion);
        Assert.True(record.Passed, string.Join(Environment.NewLine, record.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_regeneration_never_invokes_standalone_or_unity_during_apply()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        Assert.Null(fixture.Result.AuthoritativeSnapshot?.LastStandaloneBuild);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING",
            fixture.Result.AuthoritativeSnapshot?.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_exactly_one_cached_hidden_standalone_smoke_after_regeneration()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL159_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase)) return;

        var fixture = Goal159SuccessState.Value;
        var caches = Goal157TestKit.CompleteHostCaches();
        Assert.NotEmpty(caches);
        var hostBefore = caches.ToDictionary(path => path, Goal157TestKit.TreeHashes,
            StringComparer.OrdinalIgnoreCase);
        var generationBefore = Goal157TestKit.TreeHashes(Path.Combine(fixture.Project.Path, ".llmgc", "generation"));
        var baselineBefore = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = Goal157TestKit.TreeHashes(goal148);

        var standalone = fixture.Bundle.Controller.BuildWindowsStandalone();
        var snapshot = fixture.Bundle.Controller.Snapshot();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var hostRoot = Goal157TestKit.HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(hostRoot, out var expectedHost));
        Assert.Equal(expectedHost, Goal157TestKit.TreeHashes(hostRoot));
        Assert.Equal(generationBefore, Goal157TestKit.TreeHashes(Path.Combine(fixture.Project.Path,
            ".llmgc", "generation")));
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
        Assert.Equal(snapshot.FinalStateHash, manifest.RootElement.GetProperty("finalStateHash").GetString());
        Assert.Contains("generated_travel", categories);
        Assert.Equal("generated_destination_interaction", categories[^1]);
        Assert.All(snapshot.GeneratedRegionTravel!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.All(snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(facts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);

        using var portable = Goal156TestKit.Copy(fixture.Project, "goal159-portable-after-standalone");
        var portableUnityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var portableSnapshot = Goal157TestKit.OpenTravelWorkspace(portable.Path).Snapshot();
        var portableSource = Goal156TestKit.SourceService.Validate(portable.Path);
        var portableRecord = fixture.Bundle.Record.Read(portable.Path);
        Assert.Equal(portableUnityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceV2SchemaVersion,
            portableSource.Source?.SchemaVersion);
        Assert.True(portableRecord.Passed);
        Assert.Equal("CAMPAIGN_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.True(portableSnapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(fixture, standalone, snapshot, portableSnapshot,
            expectedHost.SequenceEqual(Goal157TestKit.TreeHashes(hostRoot)),
            baselineBefore == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            goal148Before.SequenceEqual(Goal157TestKit.TreeHashes(goal148)), categories, facts);
    }

    private static void WriteCapture(
        Goal159SuccessFixture fixture,
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        UnifiedGameProjectWorkspaceSnapshot portable,
        bool hostFileSetHashUnchanged,
        bool goal142SourceByteIdentical,
        bool sourceGoal148ByteIdentical,
        IReadOnlyList<string> categories,
        IReadOnlyList<(string Label, string Value)> facts)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL159_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = "goal159_cached_hidden_standalone_smoke_v1",
            status = "GREEN",
            candidateStatus = fixture.Preview.Status,
            fixture.Record,
            fixture.Preview.Diff,
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            hostFileSetHashUnchanged,
            unityProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standaloneSelfChecksPassed = standalone.SelfCheckTotalCount > 0
                                         && standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            actualPayloadNewWorldFactsPassed = snapshot.GeneratedRegionTravel?.HumanFacts.All(expected =>
                facts.Contains((expected.Label, expected.Value))) == true
                && categories.Contains("generated_travel", StringComparer.Ordinal),
            actualPayloadAcceptedFactsPassed = snapshot.AcceptedMechanics?.HumanFacts.All(expected =>
                facts.Contains((expected.Label, expected.Value))) == true,
            actualPayloadHashesPassed = snapshot.FinalStateHash == snapshot.GeneratedRegionTravel?.FinalStateHash,
            releaseCandidateRecordCurrent = snapshot.ReleaseCandidateConfigurationStatus == "CURRENT",
            portableCopyCurrent = portable.GeneratedWorld?.Status == "CAMPAIGN_CURRENT"
                                  && portable.AcceptedMechanics?.Passed == true
                                  && portable.ReleaseCandidateConfigurationStatus == "CURRENT",
            goal142SourceByteIdentical,
            sourceGoal148ByteIdentical,
            generatedWorld = snapshot.GeneratedWorld,
            generatedWorldActivation = snapshot.GeneratedWorldActivation,
            generatedWorldTravelOverlay = snapshot.GeneratedWorldTravelOverlay,
            generatedRegionTravel = snapshot.GeneratedRegionTravel,
            acceptedMechanics = snapshot.AcceptedMechanics,
            releaseCandidate = snapshot.ReleaseCandidate
        }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }
}
