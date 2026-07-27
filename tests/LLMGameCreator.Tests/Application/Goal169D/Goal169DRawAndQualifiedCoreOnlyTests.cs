using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169D;

[Collection(LLMGameCreator.Tests.Application.Goal160
    .Goal160Collection.Name)]
public sealed class Goal169DRawAndQualifiedCoreOnlyTests
{
    [Fact]
    public void Behavioral_raw_creation_only_package_source_and_authoring_are_valid()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.True(Goal156TestKit.Validator.Validate(
            raw.Package, raw.Project.Path).IsValid);
        Assert.True(raw.Source.Present);
        Assert.True(raw.Source.Passed,
            string.Join(",", raw.Source.Diagnostics));
        Assert.NotEmpty(raw.Authoring.Library.Catalog.Modules);
    }

    [Fact]
    public void Behavioral_raw_creation_only_profile_has_no_optional_selection()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.Empty(raw.Authoring.Document.SelectedModuleIds);
        Assert.Empty(raw.Authoring.Document.ParameterValues);
        Assert.Contains(raw.Authoring.Library.Catalog.Modules,
            item => item.Required);
    }

    [Fact]
    public void Behavioral_raw_creation_only_has_zero_build_invocations()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.Equal(0, raw.BuildInvocationCount);
        Assert.Empty(Goal169DTestKit.BuildHistoryFiles(
            raw.Project.Path));
    }

    [Fact]
    public void Behavioral_raw_creation_only_has_no_selected_v7_success()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.Empty(Goal169DTestKit.BuildHistoryFiles(
            raw.Project.Path));
        Assert.NotEqual(
            GameProjectBuildHistoryReader.SchemaVersionV7,
            raw.Snapshot.GeneratedCampaignRegionalEvents?.Status);
    }

    [Fact]
    public void Behavioral_raw_creation_only_cannot_claim_campaign_current()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.NotEqual("CAMPAIGN_CURRENT",
            raw.Snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("REGIONAL_EVENTS_CURRENT",
            raw.Snapshot.GeneratedCampaignRegionalEvents?.Status);
    }

    [Fact]
    public void Behavioral_raw_creation_only_cannot_claim_rc_current()
    {
        var raw = Goal169DTestKit.Raw;

        Assert.NotEqual("CURRENT",
            raw.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT",
            raw.Snapshot
                .ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_raw_creation_only_status_is_explicit()
    {
        Assert.Equal(
            "CREATION_ONLY_NOT_QUALIFIED",
            Goal169DTestKit.Raw.Status);
    }

    [Fact]
    public void Behavioral_projects_primary_action_remains_collect_and_play()
    {
        Assert.Equal(
            "Собрать и играть",
            UnifiedGameProjectWorkspaceVocabulary.PrimaryActionText);
    }

    [Fact]
    public void Behavioral_qualified_core_only_invokes_one_build()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(1, state.QualifiedBuildInvocationCount);
        Assert.Single(state.QualifiedHistoryFiles);
    }

    [Fact]
    public void Behavioral_qualified_core_only_build_is_green()
    {
        var build = Goal169DTestKit.State.Qualified.Build;

        Assert.True(build.Passed,
            string.Join(",", build.Diagnostics));
        Assert.Equal("GREEN", build.Status);
        Assert.Equal("GREEN", build.AttemptStatus);
    }

    [Fact]
    public void Behavioral_qualified_core_only_selects_v7_history()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            GameProjectBuildHistoryReader.SchemaVersionV7,
            state.QualifiedHistory.SchemaVersion);
        Assert.Equal("GREEN", state.QualifiedHistory.Status);
        Assert.Equal("GREEN",
            state.QualifiedHistory.AttemptStatus);
    }

    [Fact]
    public void Behavioral_qualified_core_only_package_sha_is_exact()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.Qualified.Build.PackageSha256,
            state.QualifiedPackageSha256);
        Assert.Equal(
            state.Qualified.Build.ActivatedProjectPackageSha256,
            state.QualifiedPackageSha256);
    }

    [Fact]
    public void Behavioral_qualified_core_only_package_validates()
    {
        var state = Goal169DTestKit.State;
        var validation = Goal156TestKit.Validator.Validate(
            state.Qualified.Package,
            state.Qualified.Project.Path);

        Assert.True(validation.IsValid);
    }

    [Fact]
    public void Behavioral_qualified_v7_history_correlates_package_and_final_state()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.QualifiedPackageSha256,
            state.QualifiedHistory.PackageSha256);
        Assert.Equal(
            state.Qualified.Build.FinalStateHash,
            state.QualifiedHistory.FinalStateHash);
        Assert.Equal(
            state.Qualified.Build.AttemptId,
            state.QualifiedHistory.AttemptId);
    }

    [Fact]
    public void Behavioral_qualified_build_preserves_generation_sidecars()
    {
        var state = Goal169DTestKit.State;
        var generationRoot = Goal169DTestKit.GenerationRoot(
            state.Qualified.Project.Path);

        Assert.All(
            state.Qualified.GenerationSidecarHashesBefore,
            item => Assert.Equal(
                item.Value,
                Goal169DTestKit.FileSha(
                    Path.Combine(generationRoot, item.Key))));
        Assert.Equal(
            state.QualifiedGenerationSha256,
            Goal169DTestKit.TreeSha(generationRoot));
    }

    [Fact]
    public void Behavioral_qualified_build_preserves_raw_creation_source()
    {
        var state = Goal169DTestKit.State;

        Assert.Equal(
            state.Raw.SourceSha256,
            Goal169DTestKit.FileSha(Path.Combine(
                state.Raw.Project.Path,
                SeededGeneratedProjectVocabulary.SourceRelativePath
                    .Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(
            state.Raw.GenerationSha256,
            Goal169DTestKit.TreeSha(
                Goal169DTestKit.GenerationRoot(
                    state.Raw.Project.Path)));
    }

    [Fact]
    public void Behavioral_qualified_core_only_world_is_campaign_current()
    {
        Assert.Equal(
            "CAMPAIGN_CURRENT",
            Goal169DTestKit.State.Qualified.Snapshot
                .GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_qualified_relationship_truth_matches_available_branches()
    {
        var state = Goal169DTestKit.State;
        var relationships = state.Relationships;

        Assert.True(relationships.Passed,
            string.Join(",", relationships.Diagnostics));
        Assert.Equal(
            state.AvailableBranchCount,
            relationships.BranchQualifications.Count(
                item => item.Available));
        Assert.True(
            state.AvailableBranchCount == 0
                ? relationships.Status is
                    "ABSENT" or "RELATIONSHIPS_CURRENT"
                : relationships.Status ==
                  "RELATIONSHIPS_CURRENT");
    }

    [Fact]
    public void Behavioral_qualified_event_truth_matches_available_branches()
    {
        var state = Goal169DTestKit.State;
        var events = state.Events;

        Assert.True(events.Passed,
            string.Join(",", events.Diagnostics));
        Assert.Equal(state.AvailableBranchCount,
            events.EventCount);
        Assert.Equal(events.EventCount,
            events.QualifiedEventCount);
        Assert.Equal(
            state.AvailableBranchCount > 0,
            events.Present);
        Assert.Equal(
            state.AvailableBranchCount == 0
                ? "ABSENT"
                : "REGIONAL_EVENTS_CURRENT",
            events.Status);
    }

    [Fact]
    public void Behavioral_qualified_event_graph_correlates_with_actual_package()
    {
        var state = Goal169DTestKit.State;

        Assert.True(state.QualifiedCorrelation.Passed,
            string.Join(",",
                state.QualifiedCorrelation.Diagnostics));
        Assert.Equal(
            state.QualifiedPackageSha256,
            state.Events.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_qualified_empty_or_event_bearing_policy_is_exact()
    {
        var state = Goal169DTestKit.State;
        var events = state.Events;

        if (state.AvailableBranchCount == 0)
        {
            Assert.Equal(
                "EXACT_EMPTY_EVENT_GRAPH_V1",
                events.EmptyOverlayPolicy);
            Assert.NotNull(events.Overlay);
            Assert.Empty(events.EventInventory);
            Assert.Empty(events.EventQualifications);
            Assert.Empty(events.RuntimeFrames);
            Assert.Empty(events.ReplaySignatures);
            Assert.Empty(events.Overlay!.Bindings);
        }
        else
        {
            Assert.NotNull(events.Overlay);
            Assert.Equal(
                state.AvailableBranchCount,
                events.Overlay!.Bindings.Count);
            Assert.Equal(
                state.AvailableBranchCount,
                events.EventInventory.Count);
            Assert.Equal(
                state.AvailableBranchCount,
                events.EventQualifications.Count);
        }
    }

    [Fact]
    public void Behavioral_qualified_core_only_accepted_mechanics_remain_incomplete()
    {
        var accepted =
            Goal169DTestKit.State.Qualified.Build.AcceptedMechanics;

        Assert.NotNull(accepted);
        Assert.True(accepted!.Present);
        Assert.False(accepted.Passed);
        Assert.NotEmpty(accepted.MissingFactKinds);
    }

    [Fact]
    public void Behavioral_qualified_core_only_never_projects_false_rc_current()
    {
        var snapshot =
            Goal169DTestKit.State.Qualified.Snapshot;

        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);
    }
}
