using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal161Q;
using LLMGameCreator.Tests.Application.Goal168;
using LLMGameCreator.Tests.Application.Goal169;
using System.Text.Json.Nodes;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169BMigrationAndPayloadTests
{
    [Fact]
    public void Behavioral_compatible_migration_preserves_all_definition_authorities()
    {
        var state = Goal169SaveMigrationState.Value;
        var fact = Assert.Single(
            state.CompatiblePreview.RegionalEventFacts,
            item => item.RegionalEventId ==
                    state.Event.RegionalEventId);

        Assert.True(fact.Compatible);
        Assert.True(fact.DefinitionCorrelationPassed);
        Assert.True(fact.MarkerDefinitionPreserved);
        Assert.True(fact.PrototypeDefinitionPreserved);
        Assert.True(fact.DialogueDefinitionPreserved);
        Assert.True(fact.InteractionDefinitionPreserved);
        Assert.False(fact.PlacementChanged);
        Assert.Equal("EXACT_PLACEMENT_REQUIRED",
            fact.PlacementPolicy);
    }

    [Theory]
    [InlineData("dialogue", "DialogueDefinitionPreserved")]
    [InlineData("interaction", "InteractionDefinitionPreserved")]
    [InlineData("entity_prototype", "PrototypeDefinitionPreserved")]
    [InlineData("map_entity", "MarkerDefinitionPreserved")]
    public void Behavioral_definition_change_drops_resolution_without_ghost(
        string kind,
        string expectedFalseFact)
    {
        var state = Goal169SaveMigrationState.Value;
        var revision = state.ResolvedLoaded.Revision!;
        var id = kind switch
        {
            "dialogue" => state.Event.DialogueId,
            "interaction" => state.Event.InteractionId,
            "entity_prototype" => state.Event.EntityPrototypeId,
            "map_entity" => state.Event.MapEntityId,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var fingerprints = revision.DefinitionFingerprints.Select(item =>
            item.Kind == kind && item.Id == id
                ? item with { CanonicalSha256 = new string('8', 64) }
                : item).ToList();
        var source = revision with
        {
            ParentRevisionSha256 = null,
            WorldId = "world/goal169b-" + kind,
            DefinitionFingerprints = fingerprints,
            RevisionSha256 = string.Empty
        };
        var slot = "goal169b-" + kind;
        var written = state.Build.Saves.Store.WriteRevision(
            state.Build.Project.Path, slot, source);
        Assert.True(written.Passed,
            string.Join(",", written.Diagnostics));
        var preview = state.Build.Saves.Migration.Preview(
            state.Build.Project.Path, slot);
        var fact = Assert.Single(preview.RegionalEventFacts,
            item => item.RegionalEventId ==
                    state.Event.RegionalEventId);

        Assert.True(preview.Passed,
            string.Join(",", preview.Diagnostics));
        Assert.False(fact.Compatible);
        Assert.False(fact.DefinitionCorrelationPassed);
        Assert.False(fact.ResolutionFlagPreserved);
        Assert.True(fact.StatusReset);
        Assert.Equal("event_definition_mismatch",
            fact.DroppedReason);
        Assert.False(expectedFalseFact switch
        {
            "DialogueDefinitionPreserved" =>
                fact.DialogueDefinitionPreserved,
            "InteractionDefinitionPreserved" =>
                fact.InteractionDefinitionPreserved,
            "PrototypeDefinitionPreserved" =>
                fact.PrototypeDefinitionPreserved,
            "MarkerDefinitionPreserved" =>
                fact.MarkerDefinitionPreserved,
            _ => true
        });
    }

    [Fact]
    public void Behavioral_migration_definition_inventory_has_prototype_and_map_entity()
    {
        var revision =
            Goal169SaveMigrationState.Value.ResolvedLoaded.Revision!;
        Assert.Contains(revision.DefinitionFingerprints, item =>
            item.Kind == "entity_prototype"
            && item.Id ==
            Goal169SaveMigrationState.Value.Event.EntityPrototypeId);
        Assert.Contains(revision.DefinitionFingerprints, item =>
            item.Kind == "map_entity"
            && item.Id ==
            Goal169SaveMigrationState.Value.Event.MapEntityId);
    }

    [Fact]
    public void Behavioral_payload_authority_has_twenty_four_signatures()
    {
        var authority = Goal169BTestKit.Events.PayloadAuthority;
        Assert.NotNull(authority);
        Assert.True(authority.Passed);
        Assert.Equal(6, authority.RegionalEventIds.Count);
        Assert.Equal(24, authority.ReplaySignatures.Count);
        Assert.Equal(24, authority.FrameCounts.Count);
        Assert.NotEmpty(authority.NestedCombatTraceSha256);
    }

    [Fact]
    public void Behavioral_payload_authority_is_recomputed_from_history_frames()
    {
        var events = Goal169BTestKit.Events;
        var observed =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(events.PayloadAuthority,
                    events.EventInventory,
                    events.ReplaySignatures,
                    events.RuntimeFrames);

        Assert.True(observed.Passed,
            string.Join(",", observed.Diagnostics));
    }

    [Fact]
    public void Behavioral_payload_frame_identity_is_self_contained()
    {
        Assert.All(Goal169BTestKit.Events.RuntimeFrames, frame =>
        {
            var encoded =
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .FrameCategory(frame);
            Assert.True(
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .TryParseFrameCategory(encoded,
                        out var identity));
            Assert.Equal(frame.RegionalEventId,
                identity.RegionalEventId);
            Assert.Equal(frame.RouteKind, identity.RouteKind);
            Assert.Equal(frame.ReplayIndex,
                identity.ReplayIndex);
            Assert.Equal(frame.SequenceIndex,
                identity.SequenceIndex);
            Assert.Equal(frame.CommandSha256,
                identity.CommandIdentity);
        });
    }

    [Fact]
    public void Behavioral_standalone_projection_preserves_frame_identity_in_title()
    {
        var build = Goal168TestKit.Build;
        Assert.NotEmpty(build.RuntimeFrames);
        Assert.All(build.RuntimeFrames, frame =>
        {
            Assert.Equal("generated-regional-event", frame.Category);
            Assert.True(
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .TryParseFrameCategory(frame.Title, out _));
        });
    }

    [Fact]
    public void Behavioral_locked_and_resolution_replay_one_are_unambiguous()
    {
        var eventId = Goal169BTestKit.Events.EventInventory[0]
            .RegionalEventId;
        var locked = Goal169BTestKit.Events.RuntimeFrames.First(item =>
            item.RegionalEventId == eventId
            && item.RouteKind ==
            GeneratedCampaignRegionalEventReplayRouteKind.LOCKED_PROBE
            && item.ReplayIndex == 1);
        var resolution = Goal169BTestKit.Events.RuntimeFrames.First(item =>
            item.RegionalEventId == eventId
            && item.RouteKind ==
            GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION
            && item.ReplayIndex == 1);

        Assert.NotEqual(
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .FrameCategory(locked),
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .FrameCategory(resolution));
    }

    [Theory]
    [InlineData("package")]
    [InlineData("final")]
    [InlineData("event_id")]
    [InlineData("signature")]
    [InlineData("component")]
    [InlineData("frame_count")]
    [InlineData("nested_trace")]
    public void Behavioral_payload_authority_tamper_is_rejected(
        string tamper)
    {
        var events = Goal169BTestKit.Events;
        var source = events.PayloadAuthority;
        var changed = tamper switch
        {
            "package" => source with
            {
                PackageSha256 = new string('1', 64)
            },
            "final" => source with
            {
                FinalStateHash = new string('2', 64)
            },
            "event_id" => source with
            {
                RegionalEventIds =
                    source.RegionalEventIds.Skip(1).ToList()
            },
            "signature" => source with
            {
                ReplaySignatures = source.ReplaySignatures.Select(
                    (item, index) => index == 0
                        ? item with
                        {
                            SignatureSha256 = new string('3', 64)
                        }
                        : item).ToList()
            },
            "component" => source with
            {
                ComponentSha256 =
                    new SortedDictionary<string, string>(
                        source.ComponentSha256.ToDictionary(
                            item => item.Key, item => item.Value,
                            StringComparer.Ordinal),
                        StringComparer.Ordinal)
                    {
                        ["frames"] = new string('4', 64)
                    }
            },
            "frame_count" => source with
            {
                FrameCounts =
                    new SortedDictionary<string, int>(
                        source.FrameCounts.ToDictionary(
                            item => item.Key, item => item.Value,
                            StringComparer.Ordinal),
                        StringComparer.Ordinal)
                    {
                        [source.FrameCounts.Keys.First()] =
                            source.FrameCounts.Values.First() + 1
                    }
            },
            "nested_trace" => source with
            {
                NestedCombatTraceSha256 =
                    new SortedDictionary<string, string>(
                        source.NestedCombatTraceSha256.ToDictionary(
                            item => item.Key, item => item.Value,
                            StringComparer.Ordinal),
                        StringComparer.Ordinal)
                    {
                        [source.NestedCombatTraceSha256.Keys.First()] =
                            new string('5', 64)
                    }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tamper))
        };

        var observed =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(changed, events.EventInventory,
                    events.ReplaySignatures, events.RuntimeFrames);
        Assert.False(observed.Passed);
    }

    [Fact]
    public void Behavioral_payload_human_fact_contains_strict_authority()
    {
        var events = Goal169BTestKit.Events;
        var fact = Assert.Single(events.HumanReviewFacts, item =>
            item.Label ==
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .HumanFactLabel);

        Assert.StartsWith("base64:", fact.Value,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\"", fact.Value,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\r", fact.Value,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\n", fact.Value,
            StringComparison.Ordinal);
        var decoded =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(fact.Value);
        Assert.Equal(events.PayloadAuthority.AuthoritySha256,
            decoded.AuthoritySha256);
        var validation =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(decoded, events.EventInventory,
                    events.ReplaySignatures, events.RuntimeFrames);
        Assert.True(validation.Passed,
            string.Join(",", validation.Diagnostics));
    }

    [Fact]
    public void Behavioral_post_smoke_payload_fix_passes_legacy_self_check()
    {
        var events = Goal169BTestKit.Events;
        using var payload = Goal161QPayloadFixture.Create();
        payload.EditModel(model =>
        {
            var facts = model["humanReviewFacts"]!.AsArray();
            facts.Clear();
            facts.Add(new JsonObject
            {
                ["label"] =
                    GeneratedCampaignRegionalEventPayloadAuthorityService
                        .HumanFactLabel,
                ["value"] =
                    GeneratedCampaignRegionalEventPayloadAuthorityService
                        .SerializeHumanFact(events.PayloadAuthority)
            });
        });
        payload.EditFrames(frames =>
        {
            for (var index = 0; index < frames.Count; index++)
                frames[index]!["title"] =
                    GeneratedCampaignRegionalEventPayloadAuthorityService
                        .FrameCategory(events.RuntimeFrames[index]);
        });

        var result = payload.Check();

        Assert.True(result.Passed,
            string.Join(",", result.FailedCheckCodes));
        Assert.True(result.LegacyHostParserCompatibility.Passed);
        Assert.Equal(result.TotalCount, result.PassedCount);
    }
}
