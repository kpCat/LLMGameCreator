using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160WorldRollbackApplyTests
{
    [Fact]
    public void Behavioral_sealed_history_rollback_apply_commits()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal("GREEN", state.Result.Status);
        Assert.True(state.Result.Applied);
        Assert.Equal("committed", state.Result.TransactionState);
    }

    [Fact]
    public void Behavioral_apply_activates_target_source_package_and_build()
    {
        var state = Goal160RollbackState.Value;
        var source = state.Bundle.Source.Validate(state.Project.Path);
        Assert.True(source.Passed, string.Join(Environment.NewLine, source.Diagnostics));
        Assert.Equal(state.TargetWorldId, state.FinalHistory.CurrentWorldId);
        Assert.Equal(state.Preview.CandidateBuild?.PackageSha256,
            Goal156TestKit.Hash(Path.Combine(state.Project.Path, "package.json")));
        Assert.Equal(state.Result.BuildHistoryFileName, state.WorldChangeRecord.SelectedBuildHistoryFileName);
    }

    [Fact]
    public void Behavioral_old_world_histories_are_retained()
    {
        var state = Goal160RollbackState.Value;
        var oldIds = state.InitialHistory.Entries.Select(entry => entry.WorldId).OrderBy(id => id).ToList();
        var newIds = state.FinalHistory.Entries.Select(entry => entry.WorldId).OrderBy(id => id).ToList();
        Assert.Equal(oldIds, newIds);
    }

    [Fact]
    public void Behavioral_exactly_one_new_green_build_history_is_added()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal(state.InitialBuildHistoryCount + 1, state.FinalBuildHistoryCount);
        Assert.Equal("GREEN", state.Preview.CandidateBuild?.Status);
    }

    [Fact]
    public void Behavioral_last_world_change_truthfully_says_history_rollback()
    {
        var record = Goal160RollbackState.Value.WorldChangeRecord;
        Assert.Equal("history_rollback", record.OperationKind);
        Assert.Equal(Goal160RollbackState.Value.TargetWorldId, record.ToWorldId);
        Assert.Equal("committed", record.TransactionState);
    }

    [Fact]
    public void Behavioral_old_release_candidate_bytes_are_retained_and_last_success()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal(state.OldReleaseCandidateBytes, state.NewReleaseCandidateBytes);
        Assert.Equal("LAST_SUCCESS", state.WorldChangeRecord.PreviousReleaseCandidateStatus);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING",
            state.Result.AuthoritativeSnapshot?.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_authoring_and_identity_remain_current_truth()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal(state.OriginalSelectedModuleIds, state.CandidateSelectedModuleIds);
        Assert.Equal(state.OriginalParameterJson, state.CandidateParameterJson);
        Assert.Equal(state.OriginalIdentityJson, state.CandidateIdentityJson);
    }

    [Fact]
    public void Behavioral_fresh_reopen_after_rollback_is_travel_current()
    {
        var snapshot = Goal160RollbackState.Value.Bundle.Controller.OpenProject(
            Goal160RollbackState.Value.Project.Path);
        Assert.Equal("TRAVEL_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.True(snapshot.GeneratedRegionTravel?.Passed);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
    }

    [Fact]
    public void Behavioral_rollback_does_not_copy_historical_package_authoring_identity_or_rc()
    {
        var state = Goal160RollbackState.Value;
        Assert.All(state.FinalHistory.Entries, entry =>
        {
            var files = Directory.EnumerateFiles(entry.EntryPath, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(entry.EntryPath, path).Replace('\\', '/'));
            Assert.DoesNotContain(files, path => path.Equals("package.json", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("authoring", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("identity", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("release-candidate", StringComparison.OrdinalIgnoreCase));
        });
    }
}
