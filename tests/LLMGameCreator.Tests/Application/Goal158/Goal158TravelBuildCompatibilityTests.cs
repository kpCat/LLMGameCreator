using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

[Collection(Goal156Collection.Name)]
public sealed class Goal158TravelBuildCompatibilityTests
{
    [Fact]
    public void Behavioral_all_selectable_generated_build_is_green_with_travel()
    {
        var build = Goal157BuildState.Value.First;

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.Equal("GREEN", build.Status);
        Assert.True(build.GeneratedWorldActivation?.Passed);
        Assert.True(build.GeneratedRegionTravel?.Passed);
        Assert.True(build.GeneratedWorldTravelOverlay?.ControlledDeltaPassed);
    }

    [Fact]
    public void Behavioral_lane_a_accepted_mechanics_and_social_remain_green()
    {
        var build = Goal157BuildState.Value.First;
        var compatibility = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(
            build.AcceptedMechanicsCompatibility);
        var accepted = Assert.IsType<GameProjectAcceptedMechanicsSummary>(build.AcceptedMechanics);

        Assert.True(compatibility.Passed);
        Assert.True(accepted.Passed, string.Join(Environment.NewLine, accepted.Diagnostics));
        Assert.True(compatibility.Social?.Passed);
        Assert.Equal(compatibility.CompatibilityFinalStateHash, accepted.QualificationFinalStateHash);
        Assert.NotEqual(build.FinalStateHash, accepted.QualificationFinalStateHash);
    }

    [Fact]
    public void Behavioral_primary_hashes_and_frames_belong_to_complete_travel_route()
    {
        var build = Goal157BuildState.Value.First;
        var travel = Assert.IsType<GameProjectGeneratedRegionTravelSummary>(build.GeneratedRegionTravel);

        Assert.Equal(travel.FinalStateHash, build.FinalStateHash);
        Assert.Equal(travel.RuntimeFrames, build.RuntimeFrames);
        Assert.Equal(build.PackageSha256, build.ActivatedProjectPackageSha256);
        Assert.Contains(build.RuntimeFrames, frame => frame.Category == "generated_start");
        Assert.Contains(build.RuntimeFrames, frame => frame.Category == "generated_travel");
        Assert.Contains(build.RuntimeFrames, frame => frame.Category == "generated_destination_interaction");
    }

    [Fact]
    public void Behavioral_repeat_generated_build_is_travel_deterministic()
    {
        var fixture = Goal157BuildState.Value;

        Assert.True(fixture.First.Passed && fixture.Repeat.Passed);
        Assert.Equal(fixture.First.PackageSha256, fixture.Repeat.PackageSha256);
        Assert.Equal(fixture.First.CompositionPackageSha256, fixture.Repeat.CompositionPackageSha256);
        Assert.Equal(fixture.First.FinalStateHash, fixture.Repeat.FinalStateHash);
        Assert.Equal(fixture.First.GeneratedWorldTravelOverlay?.TravelOverlaySha256,
            fixture.Repeat.GeneratedWorldTravelOverlay?.TravelOverlaySha256);
        Assert.Equal(fixture.First.GeneratedRegionTravel?.ConnectionIds,
            fixture.Repeat.GeneratedRegionTravel?.ConnectionIds);
    }

    [Fact]
    public void Behavioral_core_only_generated_build_is_green_with_travel_but_not_accepted()
    {
        var build = Goal157BuildState.Value.Core;

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.True(build.GeneratedWorldActivation?.Passed);
        Assert.True(build.GeneratedRegionTravel?.Passed);
        Assert.False(build.AcceptedMechanics?.Passed);
        Assert.NotEmpty(build.AcceptedMechanics?.MissingFactKinds ?? []);
    }

    [Fact]
    public void Behavioral_legacy_single_lane_hashes_and_behavior_remain_unchanged()
    {
        var build = Goal157BuildState.Value.Legacy;
        var compatibility = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(
            build.AcceptedMechanicsCompatibility);

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.Null(build.GeneratedWorldTravelOverlay);
        Assert.Null(build.GeneratedRegionTravel);
        Assert.Equal(compatibility.CompatibilityCompositionPackageSha256, build.CompositionPackageSha256);
        Assert.Equal(compatibility.CompatibilityActivatedPackageSha256, build.PackageSha256);
        Assert.Equal(compatibility.CompatibilityFinalStateHash, build.FinalStateHash);
    }

    [Fact]
    public void Behavioral_overlay_binding_gate_and_transition_counts_correlate()
    {
        var build = Goal157BuildState.Value.First;
        var overlay = Assert.IsType<GeneratedWorldTravelOverlayDocument>(
            build.GeneratedWorldTravelOverlay);
        var travel = Assert.IsType<GameProjectGeneratedRegionTravelSummary>(build.GeneratedRegionTravel);

        Assert.Equal(overlay.ConnectionCount, overlay.GateCount);
        Assert.Equal(travel.ConnectionIds.Count, travel.TransitionCount);
        Assert.True(overlay.RegionBindingCount >= travel.VisitedRegionIds.Distinct(StringComparer.Ordinal).Count());
    }
}
