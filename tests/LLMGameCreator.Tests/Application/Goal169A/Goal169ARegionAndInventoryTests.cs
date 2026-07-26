using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal167;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169ARegionAndInventoryTests
{
    [Fact]
    public void Behavioral_same_region_challenge_uses_exact_encounter_region()
    {
        var fixture = ChallengeFixture();
        var overlay = WithChallengeHome(fixture.Overlay,
            fixture.Seed.RegionId);

        var observed = Bind(fixture.Source, fixture.Package, overlay);
        var challenge = Challenge(observed);

        Assert.Equal(fixture.Seed.RegionId, challenge.RegionId);
        Assert.Equal(
            GeneratedCampaignRegionalEventTargetRegionDerivation
                .EXACT_CHALLENGE_ENCOUNTER_REGION,
            challenge.TargetRegionDerivation);
    }

    [Fact]
    public void Behavioral_cross_region_challenge_uses_encounter_not_home()
    {
        var fixture = ChallengeFixture();
        var differentHome = fixture.Source.RegeneratedPlan!.World.Regions
            .Select(item => item.RegionId)
            .First(item => item != fixture.Seed.RegionId);
        var overlay = WithChallengeHome(fixture.Overlay,
            differentHome);

        var challenge = Challenge(Bind(fixture.Source,
            fixture.Package, overlay));

        Assert.Equal(fixture.Seed.RegionId, challenge.RegionId);
        Assert.NotEqual(differentHome, challenge.RegionId);
    }

    [Fact]
    public void Behavioral_missing_provenance_uses_explicit_home_fallback()
    {
        var fixture = ChallengeFixture();

        var observed =
            new GeneratedCampaignRegionalEventBindingService().Bind(
                fixture.Package, fixture.Overlay);
        var challenge = Challenge(observed);

        Assert.Equal(
            GeneratedCampaignRegionalEventTargetRegionDerivation
                .RELATIONSHIP_HOME_FALLBACK,
            challenge.TargetRegionDerivation);
        Assert.Equal(fixture.Relationship.RegionId,
            challenge.RegionId);
    }

    [Fact]
    public void Behavioral_ambiguous_encounter_provenance_is_rejected()
    {
        var fixture = ChallengeFixture();
        var plan = fixture.Source.RegeneratedPlan!;
        var source = fixture.Source with
        {
            RegeneratedPlan = plan with
            {
                EncounterSeeds = plan.EncounterSeeds
                    .Concat([fixture.Seed with
                    {
                        RegionId = fixture.Relationship.RegionId
                    }]).ToList()
            }
        };

        var observed = Bind(source, fixture.Package,
            fixture.Overlay);

        Assert.False(observed.Passed);
        Assert.Contains(
            "generated_regional_event.challenge_provenance_ambiguous",
            observed.Diagnostics);
    }

    [Fact]
    public void Behavioral_declared_encounter_region_mismatch_is_rejected()
    {
        var fixture = ChallengeFixture();
        var package = Goal164TestKit.Clone(fixture.Package);
        var encounter = package.Game.Encounters.Single(item =>
            item.Id == fixture.Relationship.ChallengeEncounterId);
        encounter.Metadata["sourceRegionId"] =
            "region/goal169a-mismatch";

        var observed = Bind(fixture.Source, package,
            fixture.Overlay);

        Assert.False(observed.Passed);
        Assert.Contains(
            "generated_regional_event.challenge_region_mismatch",
            observed.Diagnostics);
    }

    [Fact]
    public void Behavioral_reordered_relationships_keep_region_derivation_deterministic()
    {
        var fixture = ChallengeFixture();
        var reversed = fixture.Overlay with
        {
            Bindings = fixture.Overlay.Bindings.Reverse().ToList()
        };

        var first = Bind(fixture.Source, fixture.Package,
            fixture.Overlay);
        var second = Bind(fixture.Source, fixture.Package, reversed);

        Assert.Equal(first.Bindings.Select(item =>
                GeneratedCampaignRegionalEventInventoryService.Create(
                    item).EventSemanticFingerprint),
            second.Bindings.Select(item =>
                GeneratedCampaignRegionalEventInventoryService.Create(
                    item).EventSemanticFingerprint));
    }

    [Fact]
    public void Behavioral_expanded_inventory_contains_exact_semantic_identity()
    {
        var events =
            Assert.IsType<GameProjectGeneratedCampaignRegionalEventSummary>(
                Goal168TestKit.Build.GeneratedCampaignRegionalEvents);

        Assert.All(events.EventInventory, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.RegionalEventId));
            Assert.False(string.IsNullOrWhiteSpace(row.RelationshipId));
            Assert.False(string.IsNullOrWhiteSpace(row.ActorSeedId));
            Assert.False(string.IsNullOrWhiteSpace(row.ActorEntityId));
            Assert.False(string.IsNullOrWhiteSpace(row.FactionId));
            Assert.False(string.IsNullOrWhiteSpace(row.RegionId));
            Assert.False(string.IsNullOrWhiteSpace(row.MapId));
            Assert.False(string.IsNullOrWhiteSpace(
                row.EntityPrototypeId));
            Assert.False(string.IsNullOrWhiteSpace(row.MapEntityId));
            Assert.False(string.IsNullOrWhiteSpace(row.InteractionId));
            Assert.False(string.IsNullOrWhiteSpace(row.DialogueId));
            Assert.False(string.IsNullOrWhiteSpace(
                row.ResolutionFlagId));
            Assert.False(string.IsNullOrWhiteSpace(
                row.PrerequisiteFingerprint));
            Assert.False(string.IsNullOrWhiteSpace(
                row.TargetRegionFingerprint));
            Assert.Equal(
                GeneratedCampaignRegionalEventInventoryService
                    .SemanticFingerprint(row),
                row.EventSemanticFingerprint);
            if (row.EventKind ==
                GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE)
            {
                Assert.False(string.IsNullOrWhiteSpace(
                    row.SourceQuestId));
                Assert.False(string.IsNullOrWhiteSpace(
                    row.RewardDerivationFingerprint));
            }
            if (row.EventKind ==
                GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH)
            {
                Assert.False(string.IsNullOrWhiteSpace(
                    row.ChallengeEncounterId));
                Assert.False(string.IsNullOrWhiteSpace(
                    row.ChallengeEncounterSourceId));
            }
        });
    }

    [Fact]
    public void Behavioral_inventory_order_is_canonical_and_unique()
    {
        var events =
            Assert.IsType<GameProjectGeneratedCampaignRegionalEventSummary>(
                Goal168TestKit.Build.GeneratedCampaignRegionalEvents);
        var ids = events.EventInventory.Select(item =>
            item.RegionalEventId).ToList();

        Assert.Equal(ids.Count,
            ids.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(events.Overlay!.Inventory,
            events.EventInventory);
    }

    private static GeneratedCampaignRegionalEventBindingResult Bind(
        SeededGeneratedProjectSourceValidationResult source,
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        GeneratedCampaignRelationshipOverlayDocument overlay)
    {
        var result =
            new GeneratedCampaignRegionalEventBindingService().Bind(
                source, package, overlay);
        return result;
    }

    private static GeneratedCampaignRegionalEventBinding Challenge(
        GeneratedCampaignRegionalEventBindingResult result)
    {
        Assert.True(result.Passed,
            string.Join(",", result.Diagnostics));
        return Assert.Single(result.Bindings, item =>
            item.EventKind ==
            GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH);
    }

    private static GeneratedCampaignRelationshipOverlayDocument
        WithChallengeHome(
            GeneratedCampaignRelationshipOverlayDocument overlay,
            string regionId) => overlay with
        {
            Bindings = overlay.Bindings.Select(item =>
                item.Branches.Contains(
                    GeneratedCampaignRelationshipBranch.CHALLENGE)
                    ? item with { RegionId = regionId }
                    : item).ToList()
        };

    private static RegionFixture ChallengeFixture()
    {
        var source = Goal168RelationshipFixture.Source;
        var package = Goal164TestKit.Clone(
            Goal168RelationshipFixture.Overlay
                .RelationshipOverlayPackage);
        var overlay = Goal168TestKit.RelationshipOverlay;
        var candidates = overlay.Bindings.Where(item =>
                item.Branches.Contains(
                    GeneratedCampaignRelationshipBranch.CHALLENGE))
            .Select(relationship =>
            {
                var encounter = package.Game.Encounters.Single(item =>
                    item.Id == relationship.ChallengeEncounterId);
                var sourceId =
                    encounter.Metadata["sourceEncounterSeedId"];
                var seed = source.RegeneratedPlan!.EncounterSeeds
                    .Single(item => Goal167TestKit.SourceIdMatches(
                        item.EncounterSeedId, sourceId));
                return (Relationship: relationship, Seed: seed);
            }).ToList();
        var selected = candidates[0];
        var relationship = selected.Relationship;
        overlay = overlay with { Bindings = [relationship] };
        return new RegionFixture(source, package, overlay,
            relationship, selected.Seed);
    }

    private sealed record RegionFixture(
        SeededGeneratedProjectSourceValidationResult Source,
        LLMGameCreator.GamePackage.GamePackageDefinition Package,
        GeneratedCampaignRelationshipOverlayDocument Overlay,
        GeneratedCampaignRelationshipBinding Relationship,
        ProceduralEncounterSeed Seed);
}
