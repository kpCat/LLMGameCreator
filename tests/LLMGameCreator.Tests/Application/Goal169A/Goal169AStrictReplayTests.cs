using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169AStrictReplayTests
{
    private static GameProjectGeneratedCampaignRegionalEventSummary Events =>
        Assert.IsType<GameProjectGeneratedCampaignRegionalEventSummary>(
            Goal168TestKit.Build.GeneratedCampaignRegionalEvents);

    [Fact]
    public void Behavioral_each_event_has_two_locked_and_two_resolution_replays()
    {
        foreach (var qualification in Events.EventQualifications)
        {
            Assert.Equal(4, qualification.ReplaySignatures.Count);
            Assert.Equal(2, qualification.ReplaySignatures.Count(item =>
                item.RouteKind ==
                GeneratedCampaignRegionalEventReplayRouteKind.LOCKED_PROBE));
            Assert.Equal(2, qualification.ReplaySignatures.Count(item =>
                item.RouteKind ==
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION));
        }
    }

    [Fact]
    public void Behavioral_locked_replays_have_exact_matching_signatures()
    {
        Assert.All(Events.EventQualifications, qualification =>
        {
            var pair = qualification.ReplaySignatures.Where(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind.LOCKED_PROBE)
                .OrderBy(item => item.ReplayIndex).ToList();
            Assert.True(GeneratedCampaignRegionalEventReplayService
                .Compare(pair[0], pair[1]).Passed);
        });
    }

    [Fact]
    public void Behavioral_resolution_replays_have_exact_matching_signatures()
    {
        Assert.All(Events.EventQualifications, qualification =>
        {
            var pair = qualification.ReplaySignatures.Where(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION)
                .OrderBy(item => item.ReplayIndex).ToList();
            Assert.True(GeneratedCampaignRegionalEventReplayService
                .Compare(pair[0], pair[1]).Passed);
        });
    }

    [Fact]
    public void Behavioral_runtime_frames_contain_explicit_move_commands()
    {
        Assert.Contains(Events.RuntimeFrames, item =>
            item.CommandType.StartsWith("Move.", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_runtime_frames_do_not_reinterpret_bare_directions()
    {
        var bareDirections = new HashSet<string>(StringComparer.Ordinal)
        {
            "Up", "Down", "Left", "Right"
        };
        Assert.DoesNotContain(Events.RuntimeFrames,
            item => bareDirections.Contains(item.CommandType));
    }

    [Fact]
    public void Behavioral_same_final_hash_is_not_replay_equivalence()
    {
        var pair = ResolutionPair();
        var tampered = pair.Second with
        {
            CommandSequenceSha256 = new string('f', 64)
        };

        Assert.Equal(pair.First.FinalStateHash, tampered.FinalStateHash);
        var comparison =
            GeneratedCampaignRegionalEventReplayService.Compare(
                pair.First, tampered);
        Assert.False(comparison.Passed);
        Assert.Contains(
            "generated_regional_event.replay_mismatch.command_sequence",
            comparison.Diagnostics);
    }

    [Theory]
    [InlineData("command")]
    [InlineData("map_event")]
    [InlineData("gameplay_event")]
    [InlineData("status")]
    [InlineData("state_chain")]
    [InlineData("choices")]
    [InlineData("reputation")]
    [InlineData("resolution_flag")]
    [InlineData("relationship_flags")]
    [InlineData("quests")]
    [InlineData("encounter")]
    [InlineData("missing_frame")]
    public void Behavioral_adversarial_frame_mismatch_is_rejected(
        string dimension)
    {
        var pair = ResolutionPair();
        var frames = Events.RuntimeFrames.Where(item =>
                item.RegionalEventId == pair.First.RegionalEventId
                && item.RouteKind == pair.First.RouteKind
                && item.ReplayIndex == 1)
            .OrderBy(item => item.SequenceIndex)
            .Select(item => item with { ReplayIndex = 2 })
            .ToList();
        Assert.NotEmpty(frames);

        if (dimension == "missing_frame")
        {
            frames.RemoveAt(Math.Min(1, frames.Count - 1));
            frames = frames.Select((item, index) =>
                item with { SequenceIndex = index }).ToList();
        }
        else
        {
            frames[0] = dimension switch
            {
                "command" => frames[0] with
                {
                    CommandSha256 = new string('1', 64)
                },
                "map_event" => frames[0] with
                {
                    MapEventSha256 = new string('2', 64)
                },
                "gameplay_event" => frames[0] with
                {
                    GameplayEventSha256 = new string('3', 64)
                },
                "status" => frames[0] with
                {
                    StatusAfter =
                        GeneratedCampaignRegionalEventStatus.RESOLVED
                },
                "state_chain" => frames[0] with
                {
                    BeforeStateHash = new string('4', 64)
                },
                "choices" => frames[0] with
                {
                    AvailableChoiceIdsSha256 = new string('5', 64)
                },
                "reputation" => frames[0] with
                {
                    ObservedReputation =
                        frames[0].ObservedReputation + 1
                },
                "resolution_flag" => frames[0] with
                {
                    ObservedResolutionFlag = "TAMPERED"
                },
                "relationship_flags" => frames[0] with
                {
                    RelationshipFlagsSha256 = new string('6', 64)
                },
                "quests" => frames[0] with
                {
                    QuestStatesSha256 = new string('7', 64)
                },
                "encounter" => frames[0] with
                {
                    EncounterStateSha256 = new string('8', 64)
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(dimension), dimension, null)
            };
        }

        var adversarial =
            GeneratedCampaignRegionalEventReplayService.CreateSignature(
                pair.First.RegionalEventId, pair.First.RouteKind, 2,
                frames);
        Assert.Equal(pair.First.FinalStateHash,
            adversarial.FinalStateHash);
        Assert.False(GeneratedCampaignRegionalEventReplayService
            .Compare(pair.First, adversarial).Passed);
    }

    private static (GeneratedCampaignRegionalEventReplaySignature First,
        GeneratedCampaignRegionalEventReplaySignature Second)
        ResolutionPair()
    {
        var pair = Events.EventQualifications[0].ReplaySignatures
            .Where(item => item.RouteKind ==
                           GeneratedCampaignRegionalEventReplayRouteKind
                               .RESOLUTION)
            .OrderBy(item => item.ReplayIndex).ToList();
        return (pair[0], pair[1]);
    }
}
