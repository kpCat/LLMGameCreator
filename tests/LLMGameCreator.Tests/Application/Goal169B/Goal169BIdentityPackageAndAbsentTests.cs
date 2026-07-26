using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169BIdentityPackageAndAbsentTests
{
    [Fact]
    public void Behavioral_baseline_has_exact_six_event_id_sets()
    {
        var events = Goal169BTestKit.Events;
        var expected = events.Overlay!.Bindings.Select(item =>
                item.RegionalEventId).OrderBy(item => item,
                StringComparer.Ordinal).ToList();

        Assert.Equal(6, expected.Count);
        Assert.Equal(expected, events.Overlay.Inventory.Select(item =>
            item.RegionalEventId).OrderBy(item => item,
            StringComparer.Ordinal));
        Assert.Equal(expected, events.EventInventory.Select(item =>
            item.RegionalEventId).OrderBy(item => item,
            StringComparer.Ordinal));
        Assert.Equal(expected, events.EventQualifications.Select(item =>
            item.RegionalEventId).OrderBy(item => item,
            StringComparer.Ordinal));
        Assert.Equal(expected, events.ReplaySignatures.Select(item =>
                item.RegionalEventId).Distinct().OrderBy(item => item,
                StringComparer.Ordinal));
        Assert.Equal(expected, events.RuntimeFrames.Select(item =>
                item.RegionalEventId).Distinct().OrderBy(item => item,
                StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("qualification_id")]
    [InlineData("qualification_kind")]
    [InlineData("qualification_relationship")]
    [InlineData("qualification_branch")]
    [InlineData("signature_event")]
    [InlineData("duplicate_signature_key")]
    [InlineData("frame_event")]
    [InlineData("frame_sequence_gap")]
    [InlineData("duplicate_frame_key")]
    [InlineData("runtime_count")]
    [InlineData("ghost_qualification")]
    [InlineData("coordinated_rename")]
    public void Behavioral_identity_set_tamper_is_rejected(string tamper)
    {
        var source = Goal169BTestKit.Events;
        var changed = TamperIdentity(source, tamper);

        var observed = Goal169BTestKit.Correlate(changed);

        Assert.False(observed.Passed);
        Assert.NotEmpty(observed.Diagnostics);
    }

    [Fact]
    public void Behavioral_each_inventory_row_has_one_binding_and_four_signatures()
    {
        var events = Goal169BTestKit.Events;
        Assert.All(events.EventInventory, row =>
        {
            Assert.Single(events.Overlay!.Bindings, item =>
                item.RegionalEventId == row.RegionalEventId);
            Assert.Single(events.EventQualifications, item =>
                item.RegionalEventId == row.RegionalEventId);
            Assert.Equal(4, events.ReplaySignatures.Count(item =>
                item.RegionalEventId == row.RegionalEventId));
        });
    }

    [Theory]
    [InlineData("dialogue_title")]
    [InlineData("dialogue_metadata")]
    [InlineData("choice_id")]
    [InlineData("choice_requirement")]
    [InlineData("choice_effect")]
    [InlineData("interaction_kind")]
    [InlineData("interaction_metadata")]
    [InlineData("prototype_name")]
    [InlineData("prototype_component")]
    [InlineData("map_entity_prototype")]
    [InlineData("map_entity_position")]
    [InlineData("map_entity_reference")]
    [InlineData("quest_definition")]
    [InlineData("encounter_definition")]
    public void Behavioral_actual_package_definition_tamper_is_rejected(
        string tamper)
    {
        var package = Goal169BTestKit.ClonePackage();
        MutateDefinition(package, tamper);
        var packageSha = Goal169BTestKit.PackageSha256(package);
        var events = RewritePackageIdentity(Goal169BTestKit.Events,
            packageSha);

        var observed = Goal169BTestKit.Correlate(events, package,
            packageSha);

        Assert.False(observed.Passed);
        Assert.Contains(observed.Diagnostics, item =>
            item.Contains("actual_package",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_event_semantic_fingerprint_contains_definition_hashes()
    {
        Assert.All(Goal169BTestKit.Events.EventInventory, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(
                row.DialogueDefinitionSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                row.InteractionDefinitionSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                row.EntityPrototypeDefinitionSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                row.MapEntityDefinitionSha256));
            Assert.Equal(
                GeneratedCampaignRegionalEventInventoryService
                    .SemanticFingerprint(row),
                row.EventSemanticFingerprint);
        });
    }

    [Theory]
    [InlineData("present")]
    [InlineData("count")]
    [InlineData("ghost_binding")]
    [InlineData("ghost_inventory")]
    [InlineData("ghost_frame")]
    [InlineData("final_hash")]
    public void Behavioral_absent_profile_rejects_nonempty_graph(
        string tamper)
    {
        var fixture = Goal169BTestKit.Absent;
        var source = fixture.Events;
        Assert.False(source.Present);
        Assert.True(source.Passed);
        Assert.Equal("EXACT_EMPTY_EVENT_GRAPH_V1",
            source.Overlay!.EmptyOverlayPolicy);
        var changed = tamper switch
        {
            "present" => source with { Present = true },
            "count" => source with { EventCount = 1 },
            "ghost_binding" => source with
            {
                Overlay = source.Overlay with
                {
                    Bindings =
                    [
                        new GeneratedCampaignRegionalEventBinding
                        {
                            RegionalEventId = "ghost"
                        }
                    ]
                }
            },
            "ghost_inventory" => source with
            {
                EventInventory =
                [
                    new GeneratedCampaignRegionalEventInventoryRow
                    {
                        RegionalEventId = "ghost"
                    }
                ]
            },
            "ghost_frame" => source with
            {
                RuntimeFrames =
                [
                    new GeneratedCampaignRegionalEventRuntimeFrame
                    {
                        RegionalEventId = "ghost"
                    }
                ]
            },
            "final_hash" => source with
            {
                FinalStateHash = new string('9', 64)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tamper))
        };

        var observed =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                fixture.Package, source.ExactPackageSha256, changed,
                fixture.Relationships);
        Assert.False(observed.Passed);
    }

    [Fact]
    public void Behavioral_absent_profile_rejects_actual_package_ghost()
    {
        var fixture = Goal169BTestKit.Absent;
        var source = fixture.Events;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Dialogues.Add(new DialogueDefinition
        {
            Id = "ghost/regional-event",
            Metadata =
            {
                ["generatedRegionalEventId"] =
                    "ghost/regional-event"
            }
        });
        var sha = Goal169BTestKit.PackageSha256(package);
        var events = RewritePackageIdentity(source, sha);

        Assert.False(
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                package, sha, events,
                fixture.Relationships).Passed);
    }

    private static GameProjectGeneratedCampaignRegionalEventSummary
        TamperIdentity(
            GameProjectGeneratedCampaignRegionalEventSummary source,
            string tamper)
    {
        var qualifications = source.EventQualifications.ToList();
        var signatures = source.ReplaySignatures.ToList();
        var frames = source.RuntimeFrames.ToList();
        var first = qualifications[0];
        switch (tamper)
        {
            case "qualification_id":
                qualifications[0] = first with
                {
                    RegionalEventId = "ghost"
                };
                break;
            case "qualification_kind":
                qualifications[0] = first with
                {
                    EventKind = first.EventKind ==
                                GeneratedCampaignRegionalEventKind
                                    .SUPPORT_GRATITUDE
                        ? GeneratedCampaignRegionalEventKind
                            .CHALLENGE_AFTERMATH
                        : GeneratedCampaignRegionalEventKind
                            .SUPPORT_GRATITUDE
                };
                break;
            case "qualification_relationship":
                qualifications[0] = first with
                {
                    RelationshipId = "ghost"
                };
                break;
            case "qualification_branch":
                qualifications[0] = first with
                {
                    RelationshipBranch =
                        GeneratedCampaignRelationshipBranch.SUPPORT
                };
                break;
            case "signature_event":
                signatures[0] = signatures[0] with
                {
                    RegionalEventId = "ghost"
                };
                break;
            case "duplicate_signature_key":
                signatures.Add(signatures[0]);
                break;
            case "frame_event":
                frames[0] = frames[0] with
                {
                    RegionalEventId = "ghost"
                };
                break;
            case "frame_sequence_gap":
                frames[0] = frames[0] with
                {
                    SequenceIndex = frames[0].SequenceIndex + 10
                };
                break;
            case "duplicate_frame_key":
                frames.Add(frames[0]);
                break;
            case "runtime_count":
                qualifications[0] = first with
                {
                    RuntimeCommandCount =
                        first.RuntimeCommandCount + 1
                };
                break;
            case "ghost_qualification":
                qualifications.Add(new
                    GeneratedCampaignRegionalEventQualification
                    {
                        RegionalEventId = "ghost"
                    });
                break;
            case "coordinated_rename":
                return CoordinatedRename(source,
                    first.RegionalEventId, "ghost/renamed");
            default:
                throw new ArgumentOutOfRangeException(nameof(tamper));
        }

        return source with
        {
            EventQualifications = qualifications,
            ReplaySignatures = signatures,
            RuntimeFrames = frames
        };
    }

    private static GameProjectGeneratedCampaignRegionalEventSummary
        CoordinatedRename(
            GameProjectGeneratedCampaignRegionalEventSummary source,
            string oldId,
            string newId)
    {
        var frames = source.RuntimeFrames.Select(item =>
            item.RegionalEventId == oldId
                ? item with { RegionalEventId = newId }
                : item).ToList();
        var qualifications = source.EventQualifications.Select(item =>
        {
            if (item.RegionalEventId != oldId)
                return item;
            var renamed = item.ReplaySignatures.Select(signature =>
            {
                var owned = frames.Where(frame =>
                    frame.RegionalEventId == newId
                    && frame.RouteKind == signature.RouteKind
                    && frame.ReplayIndex ==
                    signature.ReplayIndex).ToList();
                return GeneratedCampaignRegionalEventReplayService
                    .CreateSignature(newId, signature.RouteKind,
                        signature.ReplayIndex, owned);
            }).ToList();
            return item with
            {
                RegionalEventId = newId,
                ReplaySignatures = renamed,
                FinalStateHash = renamed.Single(signature =>
                        signature.RouteKind ==
                        GeneratedCampaignRegionalEventReplayRouteKind
                            .RESOLUTION
                        && signature.ReplayIndex == 1)
                    .FinalStateHash
            };
        }).ToList();
        var signatures = qualifications.SelectMany(item =>
            item.ReplaySignatures).ToList();
        var finalHash = Goal169BTestKit.Hash(
            qualifications.Select(item => new
            {
                item.RegionalEventId,
                item.FinalStateHash,
                ResolutionSignature = item.ReplaySignatures.Single(
                    signature => signature.RouteKind ==
                                 GeneratedCampaignRegionalEventReplayRouteKind
                                     .RESOLUTION
                                 && signature.ReplayIndex == 1)
                    .SignatureSha256
            }).ToList());
        return source with
        {
            FinalStateHash = finalHash,
            EventQualifications = qualifications,
            ReplaySignatures = signatures,
            RuntimeFrames = frames,
            PayloadAuthority =
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .Create(source.ExactPackageSha256, finalHash,
                        source.RegionalEventInventorySha256,
                        source.EventInventory, signatures, frames)
        };
    }

    private static void MutateDefinition(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        string tamper)
    {
        var events = Goal169BTestKit.Events;
        var binding = events.Overlay!.Bindings[0];
        var dialogue = package.Game.Dialogues.Single(item =>
            item.Id == binding.DialogueId);
        var choice = dialogue.Nodes.SelectMany(item => item.Choices)
            .Single(item => item.Id == binding.ResolutionChoiceId);
        var interaction = package.Game.Interactions.Single(item =>
            item.Id == binding.InteractionId);
        var prototype = package.Game.EntityPrototypes.Single(item =>
            item.Id == binding.EntityPrototypeId);
        var mapEntity = package.Game.Maps.Single(item =>
                item.Id == binding.MapId).Entities.Single(item =>
                item.Id == binding.MapEntityId);
        switch (tamper)
        {
            case "dialogue_title":
                dialogue.Title += " changed";
                break;
            case "dialogue_metadata":
                dialogue.Metadata["tamper"] = "changed";
                break;
            case "choice_id":
                choice.Id += "/changed";
                break;
            case "choice_requirement":
                choice.Requirements[0].Kind += ".changed";
                break;
            case "choice_effect":
                choice.Effects[0].Type += ".changed";
                break;
            case "interaction_kind":
                interaction.Kind += ".changed";
                break;
            case "interaction_metadata":
                interaction.Metadata["tamper"] = "changed";
                break;
            case "prototype_name":
                prototype.Name += " changed";
                break;
            case "prototype_component":
                prototype.Components[0].Type += ".changed";
                break;
            case "map_entity_prototype":
                mapEntity.PrototypeId += "/changed";
                break;
            case "map_entity_position":
                mapEntity.Position.X++;
                break;
            case "map_entity_reference":
                mapEntity.Components[0].Args["tamper"] = "changed";
                break;
            case "quest_definition":
            {
                var questBinding = events.Overlay.Bindings.First(item =>
                    !string.IsNullOrWhiteSpace(item.SourceQuestId));
                package.Game.Quests.Single(item =>
                    item.Id == questBinding.SourceQuestId).Title +=
                    " changed";
                break;
            }
            case "encounter_definition":
            {
                var encounterBinding = events.Overlay.Bindings.First(item =>
                    !string.IsNullOrWhiteSpace(
                        item.ChallengeEncounterId));
                package.Game.Encounters.Single(item =>
                    item.Id ==
                    encounterBinding.ChallengeEncounterId).Name +=
                    " changed";
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(tamper));
        }
    }

    private static GameProjectGeneratedCampaignRegionalEventSummary
        RewritePackageIdentity(
            GameProjectGeneratedCampaignRegionalEventSummary source,
            string packageSha)
    {
        var overlay = source.Overlay! with
        {
            OutputPackageSha256 = packageSha
        };
        var payload =
            GeneratedCampaignRegionalEventPayloadAuthorityService.Create(
                packageSha, source.FinalStateHash,
                source.RegionalEventInventorySha256,
                source.EventInventory, source.ReplaySignatures,
                source.RuntimeFrames);
        return source with
        {
            ExactPackageSha256 = packageSha,
            RegionalEventOverlaySha256 =
                Goal169BTestKit.Hash(overlay),
            Overlay = overlay,
            PayloadAuthority = payload
        };
    }
}
