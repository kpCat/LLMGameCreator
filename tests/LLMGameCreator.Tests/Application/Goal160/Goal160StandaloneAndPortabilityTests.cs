using System.Text;
using System.Text.Json;
using System.Reflection;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_rollback_is_standalone_pending_before_ordinary_build()
    {
        var snapshot = Goal160RollbackState.Value.Result.AuthoritativeSnapshot!;
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("LAST_SUCCESS", snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_rollback_apply_starts_no_unity_and_no_standalone()
    {
        var state = Goal160RollbackState.Value;
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        Assert.Null(state.Result.AuthoritativeSnapshot?.LastStandaloneBuild);
    }

    [Fact]
    public void Behavioral_portable_copy_before_standalone_restores_history_world_change_and_travel()
    {
        var state = Goal160RollbackState.Value;
        using var portable = Goal156TestKit.Copy(state.Project, "goal160-portable-before-standalone");
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var bundle = Goal160RollbackFixture.CreateBundle(portable.Path);
        var snapshot = bundle.Controller.Snapshot();
        var history = bundle.Controller.ReadGeneratedWorldHistory();
        var change = bundle.WorldChange.Read(portable.Path);
        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.True(history.Passed);
        Assert.Equal(state.TargetWorldId, history.CurrentWorldId);
        Assert.True(change.Passed);
        Assert.Equal("history_rollback", change.Record?.OperationKind);
        Assert.Equal("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_world_change_record_correlates_rollback_build_hashes()
    {
        var state = Goal160RollbackState.Value;
        var record = state.WorldChangeRecord;
        Assert.Equal(state.Preview.CandidateBuild?.PackageSha256, record.NewPackageSha256);
        Assert.Equal(state.Preview.CandidateBuild?.CompositionPackageSha256,
            record.NewCompositionPackageSha256);
        Assert.Equal(state.Preview.CandidateBuild?.FinalStateHash, record.NewFinalStateHash);
    }

    [Fact]
    public void Behavioral_exactly_one_cached_hidden_standalone_smoke_after_rollback()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL160_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase)) return;
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var state = Goal160RollbackState.Value;
        var caches = Goal157TestKit.CompleteHostCaches();
        Assert.NotEmpty(caches);
        var hostBefore = caches.ToDictionary(path => path, Goal157TestKit.TreeHashes,
            StringComparer.OrdinalIgnoreCase);
        var baselineBefore = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = Goal157TestKit.TreeHashes(goal148);
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;

        var standalone = state.Bundle.Controller.BuildWindowsStandalone();
        var snapshot = state.Bundle.Controller.Snapshot();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        var hostRoot = Goal157TestKit.HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(hostRoot, out var expectedHost));
        Assert.Equal(expectedHost, Goal157TestKit.TreeHashes(hostRoot));
        Assert.Equal(baselineBefore, Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(goal148Before, Goal157TestKit.TreeHashes(goal148));

        var payloadRoot = Goal157TestKit.RealPayloadRoot(standalone);
        using var manifest = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "project-manifest.json"), Encoding.UTF8));
        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-model.json"), Encoding.UTF8));
        using var frames = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-frames.json"), Encoding.UTF8));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        var categories = frames.RootElement.EnumerateArray()
            .Select(item => item.GetProperty("category").GetString() ?? string.Empty).ToList();
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
        var historyCard = (string)typeof(ProjectsPageControl).GetMethod("FormatWorldHistoryCard",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [snapshot])!;
        Assert.Contains("Windows standalone    подтверждён", historyCard, StringComparison.Ordinal);

        using var portable = Goal156TestKit.Copy(state.Project, "goal160-portable-after-standalone");
        var portableUnityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var portableBundle = Goal160RollbackFixture.CreateBundle(portable.Path);
        var portableSnapshot = portableBundle.Controller.Snapshot();
        var portableHistory = portableBundle.Controller.ReadGeneratedWorldHistory();
        var portableChange = portableBundle.WorldChange.Read(portable.Path);
        Assert.Equal(portableUnityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.True(portableHistory.Passed);
        Assert.Equal(state.TargetWorldId, portableHistory.CurrentWorldId);
        Assert.True(portableChange.Passed);
        Assert.Equal("TRAVEL_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.True(portableSnapshot.AcceptedMechanics?.Passed);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(state, standalone, snapshot, portableSnapshot, portableHistory, portableChange,
            expectedHost.SequenceEqual(Goal157TestKit.TreeHashes(hostRoot)),
            baselineBefore == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            goal148Before.SequenceEqual(Goal157TestKit.TreeHashes(goal148)),
            historyCard.Contains("Windows standalone    подтверждён", StringComparison.Ordinal), categories, facts);
    }

    private static void WriteCapture(
        Goal160RollbackFixture state,
        ProjectStandaloneBuildResult standalone,
        LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace.UnifiedGameProjectWorkspaceSnapshot snapshot,
        LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace.UnifiedGameProjectWorkspaceSnapshot portable,
        GeneratedWorldHistoryReadResult portableHistory,
        GameProjectGeneratedWorldChangeReadResult portableChange,
        bool hostFileSetHashUnchanged,
        bool goal142SourceByteIdentical,
        bool sourceGoal148ByteIdentical,
        bool historyUiStandaloneConfirmed,
        IReadOnlyList<string> categories,
        IReadOnlyList<(string Label, string Value)> facts)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL160_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = "goal160_cached_hidden_standalone_smoke_v1",
            status = "GREEN",
            state.TargetWorldId,
            state.Preview.Diff,
            state.WorldChangeRecord,
            state.Preview.AttemptId,
            state.Preview.CandidateSealSha256,
            rollbackCandidateBuildPassed = state.Preview.CandidateBuild?.Status == "GREEN",
            rollbackCandidateRepeatDeterministic = state.Preview.Status == "GREEN"
                                                   && state.Preview.CandidateBuild?.Passed == true,
            rollbackCandidateFreshReopenTravelCurrent = state.Preview.CandidateSnapshot?.GeneratedWorld?.Status == "TRAVEL_CURRENT",
            rollbackAuthoringPreserved = state.OriginalSelectedModuleIds.SequenceEqual(state.CandidateSelectedModuleIds)
                                         && state.OriginalParameterJson == state.CandidateParameterJson,
            rollbackIdentityPreserved = state.OriginalIdentityJson == state.CandidateIdentityJson,
            rollbackDiffPassed = state.Preview.Diff?.GameplayChanged == true,
            rollbackAtomicApplyPassed = state.Result.Applied && state.Result.TransactionState == "committed",
            rollbackOneNewHistoryAdded = state.FinalBuildHistoryCount == state.InitialBuildHistoryCount + 1,
            rollbackOldHistoryPreserved = state.InitialHistory.Entries.All(oldEntry =>
                state.FinalHistory.Entries.Any(newEntry => newEntry.WorldId == oldEntry.WorldId)),
            rollbackOldRcBytesRetained = state.OldReleaseCandidateBytes.SequenceEqual(state.NewReleaseCandidateBytes),
            rollbackOldRcLastSuccess = state.Result.AuthoritativeSnapshot?.ReleaseCandidateRecordConfigurationStatus == "LAST_SUCCESS",
            standalonePendingAfterRollback = state.Result.AuthoritativeSnapshot?.ReleaseCandidateConfigurationStatus
                                             == "BUILD_GREEN_STANDALONE_PENDING",
            initialWorldHistoryEntryCount = state.InitialHistory.Entries.Count,
            worldHistoryEntryCount = state.FinalHistory.Entries.Count,
            currentWorldId = state.FinalHistory.CurrentWorldId,
            worldHistoryEntries = state.FinalHistory.Entries.Select(entry => new
            {
                entry.WorldId,
                entry.IsCurrent,
                entry.Manifest?.Seed,
                entry.Manifest?.Mode,
                entry.Manifest?.PresetId,
                entry.Manifest?.GenerationTreeSha256
            }).ToList(),
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            hostFileSetHashUnchanged,
            goal142SourceByteIdentical,
            sourceGoal148ByteIdentical,
            historyUiStandaloneConfirmed,
            unityProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standaloneSelfChecksPassed = standalone.SelfCheckTotalCount > 0
                                         && standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            actualPayloadRollbackWorldFactsPassed = snapshot.GeneratedRegionTravel?.HumanFacts.All(expected =>
                facts.Contains((expected.Label, expected.Value))) == true
                && categories.Contains("generated_travel", StringComparer.Ordinal),
            actualPayloadAcceptedFactsPassed = snapshot.AcceptedMechanics?.HumanFacts.All(expected =>
                facts.Contains((expected.Label, expected.Value))) == true,
            actualPayloadHashesPassed = snapshot.FinalStateHash == snapshot.GeneratedRegionTravel?.FinalStateHash,
            releaseCandidateRecordCurrent = snapshot.ReleaseCandidateConfigurationStatus == "CURRENT",
            portableCopyCurrent = portableHistory.Passed
                                  && portableHistory.CurrentWorldId == state.TargetWorldId
                                  && portableChange.Passed
                                  && portable.GeneratedWorld?.Status == "TRAVEL_CURRENT"
                                  && portable.AcceptedMechanics?.Passed == true
                                  && portable.ReleaseCandidateConfigurationStatus == "CURRENT",
            generatedWorld = snapshot.GeneratedWorld,
            generatedWorldActivation = snapshot.GeneratedWorldActivation,
            generatedRegionTravel = snapshot.GeneratedRegionTravel,
            acceptedMechanics = snapshot.AcceptedMechanics,
            releaseCandidate = snapshot.ReleaseCandidate
        }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }
}
