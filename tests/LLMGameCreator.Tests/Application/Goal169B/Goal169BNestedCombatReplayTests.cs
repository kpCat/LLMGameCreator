using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169BNestedCombatReplayTests
{
    [Fact]
    public void Behavioral_nested_combat_is_expanded_into_real_frames()
    {
        var nested = Goal169BTestKit.Events.RuntimeFrames.Where(item =>
            item.NestedCombat).ToList();

        Assert.NotEmpty(nested);
        Assert.All(nested, item =>
        {
            Assert.StartsWith("Prerequisite.ExactCombat.",
                item.CommandType);
            Assert.False(string.IsNullOrWhiteSpace(
                item.NestedCombatCommandIdentity));
            Assert.False(string.IsNullOrWhiteSpace(
                item.EncounterStateBeforeSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                item.EncounterStateAfterSha256));
        });
    }

    [Fact]
    public void Behavioral_each_resolution_replay_has_a_contiguous_combat_chain()
    {
        var groups = Goal169BTestKit.Events.RuntimeFrames
            .Where(item => item.NestedCombat)
            .GroupBy(item => (item.RegionalEventId, item.RouteKind,
                item.ReplayIndex)).ToList();

        Assert.NotEmpty(groups);
        Assert.All(groups, group =>
        {
            var ordered = group.OrderBy(item =>
                item.NestedCombatSequenceIndex).ToList();
            Assert.Equal(Enumerable.Range(0, ordered.Count),
                ordered.Select(item =>
                    item.NestedCombatSequenceIndex));
            Assert.All(ordered.Zip(ordered.Skip(1)),
                pair => Assert.Equal(pair.First.AfterStateHash,
                    pair.Second.BeforeStateHash));
        });
    }

    [Fact]
    public void Behavioral_nested_command_and_event_lists_match_route_service()
    {
        var route =
            LLMGameCreator.Tests.Application.Goal168.Goal168TestKit
                .RealRoute();
        Assert.True(route.Passed,
            string.Join(",", route.Diagnostics));
        Assert.True(route.TracePassed);
        Assert.Equal(route.Commands.Count, route.Trace.Count);
        Assert.Equal(route.Events,
            route.Trace.SelectMany(item => item.GameplayEvents));
        Assert.Equal(
            Goal169BTestKit.CombatHash(route.Trace),
            route.TraceSha256);
    }

    [Fact]
    public void Behavioral_baseline_replays_have_exact_nested_hashes()
    {
        var resolution = Goal169BTestKit.Events.ReplaySignatures
            .Where(item => item.RouteKind ==
                           GeneratedCampaignRegionalEventReplayRouteKind
                               .RESOLUTION
                           && item.NestedCombatFrameCount > 0)
            .ToList();

        Assert.Equal(8, resolution.Count);
        Assert.All(resolution, item =>
        {
            Assert.True(item.Passed);
            Assert.True(item.NestedCombatFrameCount > 0);
            Assert.False(string.IsNullOrWhiteSpace(
                item.NestedCombatTraceSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                item.NestedCombatEffectSequenceSha256));
        });
    }

    [Theory]
    [InlineData("command_type")]
    [InlineData("command_identity")]
    [InlineData("descriptor")]
    [InlineData("ability")]
    [InlineData("effect_class")]
    [InlineData("effect_fingerprint")]
    [InlineData("map_event")]
    [InlineData("gameplay_event")]
    [InlineData("encounter_state")]
    [InlineData("turn_chain")]
    public void Behavioral_same_final_state_with_changed_combat_route_is_rejected(
        string tamper)
    {
        var source = Goal169BTestKit.Events;
        var target = source.RuntimeFrames.First(item =>
            item.NestedCombat
            && item.RouteKind ==
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION
            && item.ReplayIndex == 1);
        var changed = Goal169BTestKit.RebuildRoute(source,
            target.RegionalEventId, target.RouteKind,
            target.ReplayIndex, frames => frames.Select(item =>
                item.SequenceIndex == target.SequenceIndex
                    ? Tamper(item, tamper)
                    : item).ToList());

        Assert.Equal(source.EventQualifications.Single(item =>
                item.RegionalEventId == target.RegionalEventId)
            .FinalStateHash, changed.EventQualifications.Single(item =>
                item.RegionalEventId == target.RegionalEventId)
            .FinalStateHash);
        Assert.False(Goal169BTestKit.Correlate(changed).Passed);
    }

    private static GeneratedCampaignRegionalEventRuntimeFrame Tamper(
        GeneratedCampaignRegionalEventRuntimeFrame frame,
        string tamper) => tamper switch
        {
            "command_type" => frame with
            {
                CommandType = frame.CommandType + ".changed"
            },
            "command_identity" => frame with
            {
                CommandSha256 = new string('1', 64),
                NestedCombatCommandIdentity = new string('1', 64)
            },
            "descriptor" => frame with
            {
                QualifiedDescriptorFingerprint = new string('2', 64)
            },
            "ability" => frame with
            {
                AbilityDefinitionSha256 = new string('3', 64)
            },
            "effect_class" => frame with
            {
                ObservedEffectClass = frame.ObservedEffectClass + ".changed"
            },
            "effect_fingerprint" => frame with
            {
                ObservedEffectFingerprint = new string('4', 64)
            },
            "map_event" => frame with
            {
                MapEventSha256 = new string('5', 64),
                NestedCombatMapEventSequenceSha256 =
                    new string('5', 64)
            },
            "gameplay_event" => frame with
            {
                GameplayEventSha256 = new string('6', 64),
                NestedCombatGameplayEventSequenceSha256 =
                    new string('6', 64)
            },
            "encounter_state" => frame with
            {
                EncounterStateAfterSha256 = new string('7', 64)
            },
            "turn_chain" => frame with
            {
                TurnAfter = frame.TurnAfter + 1
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tamper))
        };
}
