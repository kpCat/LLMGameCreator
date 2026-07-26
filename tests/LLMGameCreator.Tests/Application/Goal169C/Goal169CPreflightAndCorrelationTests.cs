using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal161Q;
using LLMGameCreator.Tests.Application.Goal168;
using LLMGameCreator.Tests.Application.Goal169;
using LLMGameCreator.Tests.Application.Goal169B;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169C;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169CPreflightAndCorrelationTests
{
    [Fact]
    public void Behavioral_human_fact_begins_with_base64_prefix()
    {
        Assert.StartsWith("base64:", AuthorityFact().Value,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"")]
    [InlineData("\r")]
    [InlineData("\n")]
    public void Behavioral_human_fact_has_no_legacy_parser_delimiter(
        string delimiter)
    {
        Assert.DoesNotContain(delimiter, AuthorityFact().Value,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_human_fact_base64_decodes_as_strict_utf8()
    {
        var value = AuthorityFact().Value;
        var bytes = Convert.FromBase64String(value["base64:".Length..]);
        var json = new UTF8Encoding(false, true).GetString(bytes);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            GeneratedCampaignRegionalEventPayloadAuthority.CurrentSchema,
            document.RootElement.GetProperty("schemaVersion")
                .GetString());
    }

    [Fact]
    public void Behavioral_human_fact_schema_is_exact()
    {
        var authority =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(AuthorityFact().Value);

        Assert.Equal(
            GeneratedCampaignRegionalEventPayloadAuthority.CurrentSchema,
            authority.SchemaVersion);
        Assert.True(authority.Passed);
    }

    [Fact]
    public void Behavioral_authority_sha_roundtrips_and_recomputes()
    {
        var events = Goal169BTestKit.Events;
        var authority =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(AuthorityFact().Value);
        var validation =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .Validate(authority, events.EventInventory,
                    events.ReplaySignatures, events.RuntimeFrames);

        Assert.Equal(events.PayloadAuthority.AuthoritySha256,
            authority.AuthoritySha256);
        Assert.True(validation.Passed,
            string.Join(",", validation.Diagnostics));
    }

    [Fact]
    public void Behavioral_authority_has_exact_six_event_ids()
    {
        var authority = Goal169BTestKit.Events.PayloadAuthority;

        Assert.Equal(6, authority.RegionalEventIds.Count);
        Assert.Equal(authority.RegionalEventIds.Count,
            authority.RegionalEventIds.Distinct(
                StringComparer.Ordinal).Count());
        Assert.Equal(
            Goal169BTestKit.Events.EventInventory
                .Select(item => item.RegionalEventId)
                .OrderBy(item => item, StringComparer.Ordinal),
            authority.RegionalEventIds);
    }

    [Theory]
    [InlineData("signatures", 24)]
    [InlineData("frame-count-keys", 24)]
    [InlineData("nested-trace-keys", 24)]
    public void Behavioral_authority_has_exact_twenty_four_route_keys(
        string dimension,
        int expected)
    {
        var authority = Goal169BTestKit.Events.PayloadAuthority;
        var actual = dimension switch
        {
            "signatures" => authority.ReplaySignatures.Count,
            "frame-count-keys" => authority.FrameCounts.Count,
            "nested-trace-keys" =>
                authority.NestedCombatTraceSha256.Count,
            _ => throw new ArgumentOutOfRangeException(
                nameof(dimension))
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Behavioral_real_assembled_payload_self_check_is_green()
    {
        var result = CheckRealAssembledPayload();

        Assert.True(result.Passed,
            string.Join(",", result.FailedCheckCodes));
        Assert.Equal(13, result.PassedCount);
        Assert.Equal(13, result.TotalCount);
    }

    [Fact]
    public void Behavioral_real_assembled_payload_legacy_parser_is_green()
    {
        var result = CheckRealAssembledPayload();

        Assert.True(result.LegacyHostParserCompatibility.Passed);
        Assert.Empty(
            result.LegacyHostParserCompatibility.FailedCodes);
        Assert.Equal(
            result.LegacyHostParserCompatibility.StructuralFrameCount,
            result.LegacyHostParserCompatibility.LegacyFrameCount);
        Assert.Equal(
            result.LegacyHostParserCompatibility
                .StructuralHumanFactCount,
            result.LegacyHostParserCompatibility.LegacyHumanFactCount);
    }

    [Fact]
    public void Behavioral_every_frame_identity_roundtrips()
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
            Assert.Equal(frame.ReplayIndex, identity.ReplayIndex);
            Assert.Equal(frame.SequenceIndex,
                identity.SequenceIndex);
            Assert.Equal(frame.CommandSha256,
                identity.CommandIdentity);
        });
    }

    [Fact]
    public void Behavioral_frame_routes_are_exact()
    {
        var events = Goal169BTestKit.Events;
        Assert.All(events.EventInventory, row =>
        {
            var routes = events.RuntimeFrames.Where(item =>
                    item.RegionalEventId == row.RegionalEventId)
                .Select(item => item.RouteKind)
                .Distinct().OrderBy(item => item).ToList();
            Assert.Equal(Enum.GetValues<
                    GeneratedCampaignRegionalEventReplayRouteKind>()
                .OrderBy(item => item), routes);
        });
    }

    [Fact]
    public void Behavioral_each_event_route_has_two_replays()
    {
        var frames = Goal169BTestKit.Events.RuntimeFrames;
        foreach (var eventId in Goal169BTestKit.Events
                     .PayloadAuthority.RegionalEventIds)
        foreach (var route in Enum.GetValues<
                     GeneratedCampaignRegionalEventReplayRouteKind>())
        {
            var replays = frames.Where(item =>
                    item.RegionalEventId == eventId
                    && item.RouteKind == route)
                .Select(item => item.ReplayIndex)
                .Distinct().OrderBy(item => item).ToList();
            Assert.Equal(new[] { 1, 2 }, replays);
        }
    }

    [Fact]
    public void Behavioral_every_route_replay_sequence_is_contiguous()
    {
        Assert.All(Goal169BTestKit.Events.RuntimeFrames
            .GroupBy(item => new
            {
                item.RegionalEventId,
                item.RouteKind,
                item.ReplayIndex
            }), group =>
        {
            var sequence = group.OrderBy(item => item.SequenceIndex)
                .Select(item => item.SequenceIndex).ToList();
            Assert.Equal(Enumerable.Range(0, sequence.Count),
                sequence);
        });
    }

    [Fact]
    public void Behavioral_frame_command_identity_is_exact()
    {
        Assert.All(Goal169BTestKit.Events.RuntimeFrames, frame =>
        {
            Assert.False(string.IsNullOrWhiteSpace(
                frame.CommandSha256));
            Assert.True(
                GeneratedCampaignRegionalEventPayloadAuthorityService
                    .TryParseFrameCategory(
                        GeneratedCampaignRegionalEventPayloadAuthorityService
                            .FrameCategory(frame),
                        out var identity));
            Assert.Equal(frame.CommandSha256,
                identity.CommandIdentity);
        });
    }

    [Fact]
    public void Behavioral_nested_combat_is_represented()
    {
        var nested = Goal169BTestKit.Events.RuntimeFrames
            .Where(item => item.NestedCombat).ToList();

        Assert.NotEmpty(nested);
        Assert.True(nested.Select(item => item.RegionalEventId)
            .Distinct(StringComparer.Ordinal).Count() > 0);
        Assert.All(nested.Select(item => item.RegionalEventId)
            .Distinct(StringComparer.Ordinal), eventId =>
            Assert.Contains(eventId,
                Goal169BTestKit.Events.PayloadAuthority
                    .RegionalEventIds));
    }

    [Fact]
    public void Behavioral_nested_combat_identity_is_complete()
    {
        Assert.All(Goal169BTestKit.Events.RuntimeFrames.Where(item =>
            item.NestedCombat), frame =>
        {
            Assert.False(string.IsNullOrWhiteSpace(
                frame.NestedCombatCommandIdentity));
            Assert.False(string.IsNullOrWhiteSpace(
                frame.NestedCombatMapEventSequenceSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                frame.NestedCombatGameplayEventSequenceSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                frame.EncounterStateBeforeSha256));
            Assert.False(string.IsNullOrWhiteSpace(
                frame.EncounterStateAfterSha256));
        });
        Assert.Contains(Goal169BTestKit.Events.RuntimeFrames.Where(
            item => item.NestedCombat), frame =>
            frame.QualifiedDescriptorFingerprint.Length > 0
            && frame.ObservedEffectFingerprint.Length > 0
            && frame.CombatProgressObserved);
    }

    [Fact]
    public void Behavioral_signatures_recompute_from_typed_history_frames()
    {
        var events = Goal169BTestKit.Events;
        Assert.All(events.ReplaySignatures, signature =>
        {
            var frames = events.RuntimeFrames.Where(item =>
                    item.RegionalEventId ==
                    signature.RegionalEventId
                    && item.RouteKind == signature.RouteKind
                    && item.ReplayIndex == signature.ReplayIndex)
                .OrderBy(item => item.SequenceIndex).ToList();
            var recomputed =
                GeneratedCampaignRegionalEventReplayService
                    .CreateSignature(signature.RegionalEventId,
                        signature.RouteKind, signature.ReplayIndex,
                        frames);
            Assert.Equal(signature.SignatureSha256,
                recomputed.SignatureSha256);
            Assert.Equal(signature.NestedCombatTraceSha256,
                recomputed.NestedCombatTraceSha256);
            Assert.Equal(signature.FrameCount,
                recomputed.FrameCount);
        });
    }

    [Fact]
    public void Behavioral_exact_event_id_sets_are_equal()
    {
        var events = Goal169BTestKit.Events;
        var expected = events.Overlay!.Bindings
            .Select(item => item.RegionalEventId)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(expected.SetEquals(events.EventInventory.Select(
            item => item.RegionalEventId)));
        Assert.True(expected.SetEquals(
            events.EventQualifications.Select(
                item => item.RegionalEventId)));
        Assert.True(expected.SetEquals(events.ReplaySignatures.Select(
            item => item.RegionalEventId)));
        Assert.True(expected.SetEquals(events.RuntimeFrames.Select(
            item => item.RegionalEventId)));
    }

    [Fact]
    public void Behavioral_actual_package_authority_is_green()
    {
        var correlation = Goal169BTestKit.Correlate();

        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        Assert.Equal(Goal169BTestKit.PackageSha256(
                Goal169BTestKit.Package),
            Goal169BTestKit.Events.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_actual_definitions_are_exact()
    {
        Assert.All(Goal169BTestKit.Events.EventInventory, row =>
        {
            Assert.NotEmpty(row.DialogueDefinitionSha256);
            Assert.NotEmpty(row.InteractionDefinitionSha256);
            Assert.NotEmpty(row.EntityPrototypeDefinitionSha256);
            Assert.NotEmpty(row.MapEntityDefinitionSha256);
            Assert.NotEmpty(row.PositionSha256);
            Assert.NotEmpty(row.InteractableReferencesSha256);
            Assert.NotEmpty(row.ResolutionRequirementsSha256);
            Assert.NotEmpty(row.ResolutionEffectsSha256);
            Assert.NotEmpty(row.EventMetadataSha256);
        });
    }

    [Fact]
    public void Behavioral_strict_absent_profile_is_green()
    {
        var absent = Goal169BTestKit.Absent;
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService
                .Validate(absent.Package,
                    absent.Events.ExactPackageSha256,
                    absent.Events, absent.Relationships);

        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        Assert.False(absent.Events.Present);
        Assert.Equal("ABSENT", absent.Events.Status);
        Assert.Empty(absent.Events.EventInventory);
        Assert.Empty(absent.Events.RuntimeFrames);
    }

    [Fact]
    public void Behavioral_typed_migration_definitions_are_preserved()
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
        Assert.Equal("EXACT_PLACEMENT_REQUIRED",
            fact.PlacementPolicy);
    }

    [Fact]
    public void Behavioral_preflight_capture_records_real_authority()
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169C_PREFLIGHT_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;

        var events = Goal169BTestKit.Events;
        var fact = AuthorityFact();
        var decoded =
            GeneratedCampaignRegionalEventPayloadAuthorityService
                .DeserializeHumanFact(fact.Value);
        var selfCheck = CheckRealAssembledPayload();
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            factLabel = fact.Label,
            factPrefix = "base64:",
            factSingleLine = !fact.Value.Contains('\r')
                             && !fact.Value.Contains('\n'),
            factContainsQuote = fact.Value.Contains('"'),
            utf8Base64Decoded = true,
            schemaVersion = decoded.SchemaVersion,
            decoded.AuthoritySha256,
            authorityRoundtripExact =
                decoded.AuthoritySha256 ==
                events.PayloadAuthority.AuthoritySha256,
            eventCount = decoded.RegionalEventIds.Count,
            signatureCount = decoded.ReplaySignatures.Count,
            frameCountKeyCount = decoded.FrameCounts.Count,
            nestedTraceKeyCount =
                decoded.NestedCombatTraceSha256.Count,
            nestedCombatFrameCount = events.RuntimeFrames.Count(
                item => item.NestedCombat),
            frameCount = events.RuntimeFrames.Count,
            structuralSelfCheckPassed = selfCheck.Passed,
            selfCheckPassedCount = selfCheck.PassedCount,
            selfCheckTotalCount = selfCheck.TotalCount,
            legacyParserPassed =
                selfCheck.LegacyHostParserCompatibility.Passed,
            legacyFrameCount =
                selfCheck.LegacyHostParserCompatibility
                    .LegacyFrameCount,
            legacyHumanFactCount =
                selfCheck.LegacyHostParserCompatibility
                    .LegacyHumanFactCount,
            strictCorrelationPassed =
                Goal169BTestKit.Correlate().Passed,
            terminology =
                "immutable_payload_history_package_correlation"
        }, new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine, new UTF8Encoding(false));

        Assert.True(selfCheck.Passed);
        Assert.True(
            selfCheck.LegacyHostParserCompatibility.Passed);
    }

    private static GeneratedCampaignRegionalEventHumanFact
        AuthorityFact() =>
        Assert.Single(Goal169BTestKit.Events.HumanReviewFacts,
            item => item.Label ==
                    GeneratedCampaignRegionalEventPayloadAuthorityService
                        .HumanFactLabel);

    private static LLMGameCreator.Application.Design
        .ProjectStandaloneBuild.ProjectStandalonePayloadSelfCheckResult
        CheckRealAssembledPayload()
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
        return payload.Check();
    }
}
