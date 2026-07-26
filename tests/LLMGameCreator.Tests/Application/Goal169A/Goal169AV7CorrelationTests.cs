using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169AV7CorrelationTests
{
    [Theory]
    [InlineData("strict_schema")]
    [InlineData("arc_count")]
    [InlineData("relationship_matrix")]
    [InlineData("event_matrix")]
    [InlineData("package_hash")]
    [InlineData("overlay_package")]
    [InlineData("overlay_hash")]
    [InlineData("inventory_overlay")]
    [InlineData("inventory_hash")]
    [InlineData("overlay_inventory_hash")]
    [InlineData("event_count")]
    [InlineData("kind_count")]
    [InlineData("duplicate_inventory")]
    [InlineData("semantic_fingerprint")]
    [InlineData("binding_inventory")]
    [InlineData("branch_availability")]
    [InlineData("signature_missing")]
    [InlineData("runtime_start_count")]
    [InlineData("frame_command")]
    [InlineData("final_state")]
    public void Behavioral_v7_tamper_matrix_is_rejected(string tamper)
    {
        var events =
            Assert.IsType<GameProjectGeneratedCampaignRegionalEventSummary>(
                Goal168TestKit.Build.GeneratedCampaignRegionalEvents);
        var relationships =
            Assert.IsType<
                GameProjectGeneratedCampaignRelationshipSummary>(
                Goal168TestKit.Build.GeneratedCampaignRelationships);
        var packageSha256 = events.ExactPackageSha256;
        Assert.True(GeneratedCampaignRegionalEventCorrelationService
            .Validate(packageSha256, events, relationships).Passed);

        (events, relationships) = Tamper(events, relationships, tamper);
        var observed =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                packageSha256, events, relationships);

        Assert.False(observed.Passed);
        Assert.NotEmpty(observed.Diagnostics);
    }

    private static (
        GameProjectGeneratedCampaignRegionalEventSummary Events,
        GameProjectGeneratedCampaignRelationshipSummary Relationships)
        Tamper(
            GameProjectGeneratedCampaignRegionalEventSummary events,
            GameProjectGeneratedCampaignRelationshipSummary relationships,
            string tamper)
    {
        var overlay = events.Overlay!;
        var inventory = events.EventInventory.ToList();
        var qualifications = events.EventQualifications.ToList();
        var frames = events.RuntimeFrames.ToList();
        switch (tamper)
        {
            case "strict_schema":
                events = events with
                {
                    StrictProofSchemaVersion = "tampered"
                };
                break;
            case "arc_count":
                relationships = relationships with
                {
                    QualifiedArcQuestCount =
                        relationships.ArcQuestCount + 1
                };
                break;
            case "relationship_matrix":
                relationships = relationships with
                {
                    RelationshipBranchMatrixSha256 =
                        new string('1', 64)
                };
                break;
            case "event_matrix":
                events = events with
                {
                    RelationshipBranchMatrixSha256 =
                        new string('2', 64)
                };
                break;
            case "package_hash":
                events = events with
                {
                    ExactPackageSha256 = new string('3', 64)
                };
                break;
            case "overlay_package":
                events = events with
                {
                    Overlay = overlay with
                    {
                        OutputPackageSha256 = new string('4', 64)
                    }
                };
                break;
            case "overlay_hash":
                events = events with
                {
                    RegionalEventOverlaySha256 = new string('5', 64)
                };
                break;
            case "inventory_overlay":
                inventory[0] = inventory[0] with
                {
                    ActorSeedId = inventory[0].ActorSeedId + "/tampered"
                };
                events = events with { EventInventory = inventory };
                break;
            case "inventory_hash":
                events = events with
                {
                    RegionalEventInventorySha256 = new string('6', 64)
                };
                break;
            case "overlay_inventory_hash":
                events = events with
                {
                    Overlay = overlay with
                    {
                        InventorySha256 = new string('7', 64)
                    }
                };
                break;
            case "event_count":
                events = events with
                {
                    EventCount = events.EventCount + 1
                };
                break;
            case "kind_count":
                events = events with
                {
                    SupportGratitudeCount =
                        events.SupportGratitudeCount + 1
                };
                break;
            case "duplicate_inventory":
                inventory[1] = inventory[1] with
                {
                    RegionalEventId = inventory[0].RegionalEventId
                };
                events = events with { EventInventory = inventory };
                break;
            case "semantic_fingerprint":
                inventory[0] = inventory[0] with
                {
                    EventSemanticFingerprint = new string('8', 64)
                };
                events = events with { EventInventory = inventory };
                break;
            case "binding_inventory":
                var bindings = overlay.Bindings.ToList();
                bindings[0] = bindings[0] with
                {
                    MapEntityId = bindings[0].MapEntityId + "/tampered"
                };
                events = events with
                {
                    Overlay = overlay with { Bindings = bindings }
                };
                break;
            case "branch_availability":
                var branchFacts =
                    relationships.BranchQualifications.ToList();
                branchFacts[0] = branchFacts[0] with
                {
                    Available = !branchFacts[0].Available
                };
                relationships = relationships with
                {
                    BranchQualifications = branchFacts
                };
                break;
            case "signature_missing":
                events = events with
                {
                    ReplaySignatures =
                        events.ReplaySignatures.Skip(1).ToList()
                };
                break;
            case "runtime_start_count":
                qualifications[0] = qualifications[0] with
                {
                    RuntimeStartCount = 3
                };
                events = events with
                {
                    EventQualifications = qualifications
                };
                break;
            case "frame_command":
                frames[0] = frames[0] with
                {
                    CommandSha256 = new string('9', 64)
                };
                events = events with { RuntimeFrames = frames };
                break;
            case "final_state":
                events = events with
                {
                    FinalStateHash = new string('a', 64)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(tamper), tamper, null);
        }
        return (events, relationships);
    }
}
