using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal169;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169B;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169BEvidenceCaptureTests
{
    [Fact]
    public void Behavioral_evidence_capture_records_package_bound_truth()
    {
        var events = Goal169BTestKit.Events;
        var absent = Goal169BTestKit.Absent;
        var correlation = Goal169BTestKit.Correlate();
        Assert.True(correlation.Passed,
            string.Join(",", correlation.Diagnostics));
        var migration = Goal169SaveMigrationState.Value;
        var migrationFact = Assert.Single(
            migration.CompatiblePreview.RegionalEventFacts,
            item => item.RegionalEventId ==
                    migration.Event.RegionalEventId);
        var nested = events.RuntimeFrames.Where(item =>
            item.NestedCombat).ToList();
        Assert.NotEmpty(nested);
        Assert.Equal(6, events.EventInventory.Count);
        Assert.Equal(24, events.ReplaySignatures.Count);
        Assert.True(migrationFact.DefinitionCorrelationPassed);
        Assert.False(absent.Events.Present);

        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169B_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            eventCount = events.EventCount,
            qualifiedEventCount = events.QualifiedEventCount,
            exactPackageSha256 = events.ExactPackageSha256,
            inventorySha256 =
                events.RegionalEventInventorySha256,
            finalStateHash = events.FinalStateHash,
            exactIdSetPassed = correlation.Passed,
            bindingIds = events.Overlay!.Bindings.Select(item =>
                item.RegionalEventId).OrderBy(item => item,
                StringComparer.Ordinal).ToList(),
            inventoryIds = events.EventInventory.Select(item =>
                item.RegionalEventId).OrderBy(item => item,
                StringComparer.Ordinal).ToList(),
            qualificationIds = events.EventQualifications.Select(
                item => item.RegionalEventId).OrderBy(item => item,
                StringComparer.Ordinal).ToList(),
            replaySignatureCount = events.ReplaySignatures.Count,
            runtimeFrameCount = events.RuntimeFrames.Count,
            nestedCombatFrameCount = nested.Count,
            nestedCombatEventCount = nested.Select(item =>
                item.RegionalEventId).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatCommandCount = nested.Select(item =>
                item.NestedCombatCommandIdentity).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatMapEventHashCount = nested.Select(item =>
                item.NestedCombatMapEventSequenceSha256).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatGameplayEventHashCount = nested.Select(item =>
                item.NestedCombatGameplayEventSequenceSha256).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatDescriptorCount = nested.Select(item =>
                item.QualifiedDescriptorFingerprint).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatEffectCount = nested.Select(item =>
                item.ObservedEffectFingerprint).Distinct(
                StringComparer.Ordinal).Count(),
            nestedCombatEncounterStateCount = nested.SelectMany(item =>
                new[]
                {
                    item.EncounterStateBeforeSha256,
                    item.EncounterStateAfterSha256
                }).Distinct(StringComparer.Ordinal).Count(),
            sixDefinitionHashesPassed = events.EventInventory.All(item =>
                item.DialogueDefinitionSha256.Length > 0
                && item.InteractionDefinitionSha256.Length > 0
                && item.EntityPrototypeDefinitionSha256.Length > 0
                && item.MapEntityDefinitionSha256.Length > 0
                && (item.SourceQuestId.Length == 0
                    || item.SourceQuestDefinitionSha256.Length > 0)
                && (item.ChallengeEncounterId.Length == 0
                    || item.ChallengeEncounterDefinitionSha256.Length > 0)),
            placementReferencesAndSemanticsPassed =
                events.EventInventory.All(item =>
                    item.PositionSha256.Length > 0
                    && item.InteractableReferencesSha256.Length > 0
                    && item.ResolutionRequirementsSha256.Length > 0
                    && item.ResolutionEffectsSha256.Length > 0
                    && item.EventMetadataSha256.Length > 0),
            absentPresent = absent.Events.Present,
            absentStatus = absent.Events.Status,
            absentEventCount = absent.Events.EventCount,
            absentBindingCount =
                absent.Events.Overlay?.Bindings.Count ?? -1,
            absentInventoryCount =
                absent.Events.EventInventory.Count,
            absentQualificationCount =
                absent.Events.EventQualifications.Count,
            absentSignatureCount =
                absent.Events.ReplaySignatures.Count,
            absentFrameCount = absent.Events.RuntimeFrames.Count,
            absentPolicy = absent.Events.EmptyOverlayPolicy,
            migrationFact.Compatible,
            migrationFact.DefinitionCorrelationPassed,
            migrationFact.MarkerDefinitionPreserved,
            migrationFact.PrototypeDefinitionPreserved,
            migrationFact.DialogueDefinitionPreserved,
            migrationFact.InteractionDefinitionPreserved,
            migrationFact.PlacementChanged,
            migrationFact.PlacementPolicy,
            payloadSchema =
                events.PayloadAuthority.SchemaVersion,
            payloadAuthoritySha256 =
                events.PayloadAuthority.AuthoritySha256,
            payloadEventIdCount =
                events.PayloadAuthority.RegionalEventIds.Count,
            payloadSignatureCount =
                events.PayloadAuthority.ReplaySignatures.Count,
            payloadFrameCountEntryCount =
                events.PayloadAuthority.FrameCounts.Count,
            payloadNestedHashCount =
                events.PayloadAuthority.NestedCombatTraceSha256.Count
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
    }
}
