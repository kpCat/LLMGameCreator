using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169D;

[Collection(LLMGameCreator.Tests.Application.Goal160
    .Goal160Collection.Name)]
public sealed class Goal169DPortableAndRetainedTests
{
    [Fact]
    public void Behavioral_portable_copy_is_physical_and_has_no_builds()
    {
        var state = Goal169DTestKit.State;

        Assert.NotEqual(
            Path.GetFullPath(state.Qualified.Project.Path),
            Path.GetFullPath(state.Portable.Path));
        Assert.True(Directory.Exists(state.Portable.Path));
        Assert.False(Directory.Exists(
            Path.Combine(state.Portable.Path, "Builds")));
    }

    [Fact]
    public void Behavioral_portable_package_hash_equals_qualified_source()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.QualifiedPackageSha256,
            state.PortableAfterOpen.PackageSha256);
        Assert.Equal(
            state.Qualified.Build.PackageSha256,
            state.PortableHistory.PackageSha256);
    }

    [Fact]
    public void Behavioral_portable_selected_history_is_byte_identical_v7()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            Goal169DTestKit.FileSha(
                state.Qualified.Build.BuildHistoryPath),
            state.PortableAfterOpen.SelectedHistorySha256);
        Assert.Equal(
            GameProjectBuildHistoryReader.SchemaVersionV7,
            state.PortableHistory.SchemaVersion);
    }

    [Fact]
    public void Behavioral_portable_authoring_is_byte_identical()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.QualifiedAuthoringSha256,
            state.PortableAfterOpen.AuthoringSha256);
        Assert.Equal(
            state.PortableBeforeOpen.AuthoringSha256,
            state.PortableAfterOpen.AuthoringSha256);
    }

    [Fact]
    public void Behavioral_portable_generation_sidecars_are_byte_identical()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.QualifiedGenerationSha256,
            state.PortableAfterOpen.GenerationSha256);
        Assert.Equal(
            state.PortableBeforeOpen.GenerationSha256,
            state.PortableAfterOpen.GenerationSha256);
    }

    [Fact]
    public void Behavioral_portable_reopen_does_not_rewrite_package_or_history()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.PortableBeforeOpen.PackageSha256,
            state.PortableAfterOpen.PackageSha256);
        Assert.Equal(
            state.PortableBeforeOpen.SelectedHistorySha256,
            state.PortableAfterOpen.SelectedHistorySha256);
    }

    [Fact]
    public void Behavioral_portable_world_restores_campaign_current()
    {
        Assert.Equal(
            "CAMPAIGN_CURRENT",
            Goal169DTestKit.State.PortableSnapshot
                .GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_portable_relationship_truth_equals_source()
    {
        var state = Goal169DTestKit.State;
        var source = state.Relationships;
        var portable = state.PortableSnapshot
            .GeneratedCampaignRelationships;

        Assert.NotNull(portable);
        Assert.Equal(source.Status, portable!.Status);
        Assert.Equal(
            source.RelationshipBranchMatrixSha256,
            portable.RelationshipBranchMatrixSha256);
        Assert.Equal(
            JsonSerializer.Serialize(source.BranchQualifications),
            JsonSerializer.Serialize(portable.BranchQualifications));
    }

    [Fact]
    public void Behavioral_portable_event_truth_equals_source()
    {
        var state = Goal169DTestKit.State;
        var source = state.Events;
        var portable = state.PortableSnapshot
            .GeneratedCampaignRegionalEvents;

        Assert.NotNull(portable);
        Assert.Equal(source.Status, portable!.Status);
        Assert.Equal(source.EventCount, portable.EventCount);
        Assert.Equal(
            source.RegionalEventInventorySha256,
            portable.RegionalEventInventorySha256);
        Assert.Equal(source.FinalStateHash,
            portable.FinalStateHash);
    }

    [Fact]
    public void Behavioral_portable_event_graph_correlates_with_package()
    {
        var correlation =
            Goal169DTestKit.State.PortableCorrelation;

        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
    }

    [Fact]
    public void Behavioral_portable_has_no_operational_pointer()
    {
        var pointer = Goal169DTestKit.State.PortablePointer;

        Assert.False(pointer.Passed);
        Assert.Equal(
            "standalone.current_pointer_missing",
            pointer.Diagnostic);
    }

    [Fact]
    public void Behavioral_portable_campaign_current_is_not_rc_current()
    {
        var snapshot =
            Goal169DTestKit.State.PortableSnapshot;

        Assert.Equal("CAMPAIGN_CURRENT",
            snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_retained_goal169c_pointer_and_status_are_exact()
    {
        var state = Goal169DTestKit.State;
        var before = state.RetainedBefore;
        var after = state.RetainedAfter;

        Assert.Equal(
            before.ExpectedCurrentPointerSha256,
            before.CurrentPointerSha256);
        Assert.Equal(
            before.ExpectedRunStatusSha256,
            before.RunStatusSha256);
        Assert.Equal(before.CurrentPointerSha256,
            after.CurrentPointerSha256);
        Assert.Equal(before.RunStatusSha256,
            after.RunStatusSha256);
        Assert.Equal(before.Pointer.PublishedAttemptId,
            before.RunStatus.AttemptId);
    }

    [Fact]
    public void Behavioral_retained_goal169c_run_payload_history_rc_and_package_are_exact()
    {
        var state = Goal169DTestKit.State;
        var before = state.RetainedBefore;
        var after = state.RetainedAfter;

        Assert.Equal(before.RunTreeSha256,
            after.RunTreeSha256);
        Assert.Equal(before.PayloadTreeSha256,
            after.PayloadTreeSha256);
        Assert.Equal(before.StandaloneHistorySha256,
            after.StandaloneHistorySha256);
        Assert.Equal(before.SelectedHistorySha256,
            after.SelectedHistorySha256);
        Assert.Equal(before.ReleaseCandidateSha256,
            after.ReleaseCandidateSha256);
        Assert.Equal(before.PackageSha256,
            after.PackageSha256);
        Assert.Equal(
            before.ExpectedSelectedHistorySha256,
            before.SelectedHistorySha256);
        Assert.Equal(
            before.ExpectedReleaseCandidateSha256,
            before.ReleaseCandidateSha256);
        Assert.Equal(before.ExpectedPackageSha256,
            before.PackageSha256);
    }

    [Fact]
    public void Behavioral_retained_goal169c_package_history_and_final_state_correlate()
    {
        var retained =
            Goal169DTestKit.State.RetainedAfter;

        Assert.Equal(retained.Pointer.PackageSha256,
            retained.SelectedHistory.PackageSha256);
        Assert.Equal(retained.Pointer.PackageSha256,
            retained.ExpectedPackageSha256);
        Assert.Equal(retained.ExpectedFinalStateHash,
            retained.Pointer.FinalStateHash);
        Assert.Equal(retained.ExpectedFinalStateHash,
            retained.SelectedHistory.FinalStateHash);
        Assert.Equal("GREEN", retained.RunStatus.Status);
    }

    [Fact]
    public void Behavioral_goal169d_invokes_no_player_unity_or_host_build()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(0,
            state.RealPlayerSmokeInvocationCount);
        Assert.Equal(0,
            state.UnityEditorProcessStartCount);
        Assert.Equal(0, state.UnityHostBuildCount);
        Assert.Equal(0, state.CachedHostMutationCount);
        Assert.Equal(state.HostBeforeSha256,
            state.HostAfterSha256);
    }
}
