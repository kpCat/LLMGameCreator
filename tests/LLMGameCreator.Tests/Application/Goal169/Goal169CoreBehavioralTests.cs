using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169CoreBehavioralTests
{
    public static TheoryData<int> Contracts => new()
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,
        18, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44,
        45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
        60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
        75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86
    };

    [Theory]
    [MemberData(nameof(Contracts))]
    public void Behavioral_goal169_contract(int contract)
    {
        var build = Goal168TestKit.Build;
        Assert.True(build.Passed,
            string.Join(Environment.NewLine, build.Diagnostics));
        var profiles = Goal169ProfileFixture.All;
        var relationships = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            build.GeneratedCampaignRelationships);
        var regionalEvents = Assert.IsType<
            GameProjectGeneratedCampaignRegionalEventSummary>(
            build.GeneratedCampaignRegionalEvents);
        var overlay = Assert.IsType<
            GeneratedCampaignRegionalEventOverlayDocument>(
            regionalEvents.Overlay);
        var fixture = Goal169RegionalEventFixture.Value;

        switch (contract)
        {
            case 1:
                Assert.True(profiles["all-branches"].Summary.Passed);
                break;
            case 2:
                Assert.True(profiles["challenge-only-zero-arc"].Summary.Passed);
                Assert.Equal(0,
                    profiles["challenge-only-zero-arc"].Summary.ArcQuestCount);
                break;
            case 3:
                Assert.True(profiles["support-refuse"].Summary.Passed);
                break;
            case 4:
                Assert.True(profiles["support-only"].Summary.Passed);
                break;
            case 5:
                Assert.True(profiles["refuse-only"].Summary.Passed);
                break;
            case 6:
                Assert.True(profiles["no-branches"].Summary.Passed);
                Assert.Equal(0, profiles["no-branches"].RuntimeStartCount);
                break;
            case 7:
                Assert.Equal(0, profiles.Values.SelectMany(item =>
                        item.Summary.BranchQualifications)
                    .Where(item => !item.Available
                                   && item.Branch ==
                                   GeneratedCampaignRelationshipBranch.SUPPORT)
                    .Sum(item => item.RuntimeStartCount));
                break;
            case 8:
                Assert.Equal(0, profiles.Values.SelectMany(item =>
                        item.Summary.BranchQualifications)
                    .Where(item => !item.Available
                                   && item.Branch ==
                                   GeneratedCampaignRelationshipBranch.CHALLENGE)
                    .Sum(item => item.RuntimeStartCount));
                break;
            case 9:
                Assert.Equal(0, profiles.Values.SelectMany(item =>
                        item.Summary.BranchQualifications)
                    .Where(item => !item.Available
                                   && item.Branch ==
                                   GeneratedCampaignRelationshipBranch.REFUSE)
                    .Sum(item => item.RuntimeStartCount));
                break;
            case 10:
                Assert.False(Goal169ProfileFixture.InvalidSupport.Passed);
                break;
            case 11:
                Assert.All(profiles.Values.SelectMany(item =>
                        item.Summary.BranchQualifications)
                    .Where(item => item.Available
                                   && item.Branch ==
                                   GeneratedCampaignRelationshipBranch.SUPPORT),
                    item => Assert.True(item.ArcLength > 0));
                break;
            case 12:
                Assert.All(profiles.Values.SelectMany(item =>
                    item.Summary.BranchQualifications), item =>
                {
                    Assert.Equal(item.Available, item.Required);
                    Assert.True(item.Passed);
                    Assert.True(item.ReplayEquivalent);
                });
                break;
            case 13:
                Assert.All(profiles.Values, item =>
                    Assert.Equal(64,
                        item.Summary.RelationshipBranchMatrixSha256
                            .Length));
                break;
            case 14:
                Assert.True(Goal169HistoryCompatibilityFixture
                    .LegacyAllBranchCompatible);
                break;
            case 15:
                Assert.True(Goal169HistoryCompatibilityFixture
                    .LegacyPartialRejected);
                break;
            case 16:
                Assert.True(profiles["challenge-only-zero-arc"].Summary
                    .MaximumObservedArcLength == 0);
                break;
            case 17:
                Assert.All(profiles.Values.SelectMany(item =>
                        item.Summary.BranchQualifications)
                    .Where(item => item.Branch ==
                                   GeneratedCampaignRelationshipBranch.SUPPORT
                                   && item.Available),
                    item => Assert.True(item.ArcLength > 0));
                break;
            case 18:
                Assert.All(profiles.Values, item =>
                    Assert.True(item.Summary.AtomicRollbackPassed));
                break;
            case 31:
                Assert.Contains(overlay.Bindings, item =>
                    item.EventKind ==
                    GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE
                    && item.Prerequisite.CompletedQuestIds.Count > 0);
                break;
            case 32:
                Assert.Contains(overlay.Bindings, item =>
                    item.EventKind ==
                    GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH);
                break;
            case 33:
                Assert.Contains(overlay.Bindings, item =>
                    item.EventKind ==
                    GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT);
                break;
            case 34:
                Assert.Equal(relationships.BranchQualifications.Count(item =>
                        item.Available),
                    overlay.EventCount);
                break;
            case 35:
                Assert.Empty(profiles["no-branches"].Events.Bindings);
                break;
            case 36:
                Assert.All(overlay.Bindings, item =>
                    Assert.Equal(item.RegionalEventId, item.DialogueId));
                break;
            case 37:
                Assert.All(overlay.Bindings, item =>
                    Assert.Equal(item.DialogueId, item.ResolutionFlagId));
                break;
            case 38:
                Assert.Equal(fixture.Binding.Bindings.Select(EventKey),
                    fixture.Rebound.Bindings.Select(EventKey));
                break;
            case 39:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE),
                    item =>
                    {
                        Assert.False(string.IsNullOrWhiteSpace(
                            item.SourceQuestId));
                        Assert.False(string.IsNullOrWhiteSpace(item.RegionId));
                    });
                break;
            case 40:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH),
                    item => Assert.False(string.IsNullOrWhiteSpace(
                        item.Prerequisite.ChallengeEncounterId)));
                break;
            case 41:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT),
                    item => Assert.False(string.IsNullOrWhiteSpace(
                        item.RelationshipId)));
                break;
            case 42:
                Assert.All(overlay.Bindings, item =>
                    Assert.InRange(item.Placement.X, 0,
                        build.GeneratedCampaignRegionalEvents!.Overlay!
                            .Bindings.Single(value =>
                                value.RegionalEventId ==
                                item.RegionalEventId).Placement.X + 1000));
                break;
            case 43:
                Assert.Equal(overlay.EventCount, overlay.Bindings.Count);
                Assert.True(overlay.EventCount > 1);
                break;
            case 44:
                Assert.All(overlay.Bindings,
                    item => Assert.True(item.Placement.Walkable));
                break;
            case 45:
                Assert.All(overlay.Bindings,
                    item => Assert.True(item.Placement.Reachable));
                break;
            case 46:
                Assert.All(overlay.Bindings, item =>
                    Assert.True(item.Placement.Safe));
                break;
            case 47:
                Assert.Equal(overlay.EventCount, overlay.Bindings
                    .Select(item =>
                        (item.MapId, item.Placement.X, item.Placement.Y))
                    .Distinct().Count());
                break;
            case 48:
                Assert.Equal(fixture.Overlay.RegionalEventOverlayPackageJson,
                    fixture.Reordered.RegionalEventOverlayPackageJson);
                break;
            case 49:
                Assert.False(Goal169RegionalEventFixture
                    .InsufficientPlacement.Passed);
                break;
            case 50:
                Assert.Equal(overlay.EventCount,
                    overlay.Inventory.Count);
                Assert.False(string.IsNullOrWhiteSpace(
                    overlay.InventorySha256));
                break;
            case 51:
                Assert.True(overlay.ControlledDeltaPassed);
                break;
            case 52:
                Assert.True(fixture.Overlay.Document
                    .ControlledDeltaPassed);
                break;
            case 53:
                Assert.Equal(
                    Goal164TestKit.Canonical(
                        fixture.Source.GeneratedContent),
                    Goal164TestKit.Canonical(fixture.Overlay
                        .RegionalEventOverlayPackage.GeneratedContent));
                Assert.Equal(Goal164TestKit.Canonical(
                        fixture.Source.Manifest),
                    Goal164TestKit.Canonical(fixture.Overlay
                        .RegionalEventOverlayPackage.Manifest));
                break;
            case 54:
                Assert.True(fixture.Overlay.Document
                    .ControlledDeltaPassed);
                Assert.True(fixture
                    .RelationshipTravelRecordsPreserved);
                break;
            case 55:
                Assert.True(fixture.EventReferencesResolve);
                break;
            case 56:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE),
                    item => Assert.Equal(64,
                        item.SourceQuestRewardFingerprint.Length));
                break;
            case 57:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind !=
                        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE),
                    item => Assert.Equal(0,
                        item.ResolutionReputationDelta));
                break;
            case 58:
                Assert.Equal(fixture.Overlay.Document.OutputPackageSha256,
                    fixture.Rebuilt.Document.OutputPackageSha256);
                break;
            case 59:
                Assert.False(fixture.ForbiddenDelta.Passed);
                break;
            case 60:
                Assert.True(regionalEvents.LockedStatePassed);
                break;
            case 61:
                Assert.True(regionalEvents.AvailableStatePassed);
                break;
            case 62:
                Assert.True(regionalEvents.ResolvedStatePassed);
                Assert.Contains(regionalEvents.RuntimeFrames, item =>
                    item.CommandType.Contains("Interact",
                        StringComparison.Ordinal));
                break;
            case 63:
                Assert.True(regionalEvents.ExactlyOncePassed);
                Assert.Contains(overlay.Bindings, item =>
                    item.EventKind ==
                    GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE
                    && item.ResolutionReputationDelta > 0);
                break;
            case 64:
                Assert.True(relationships.ChallengeFleePassed);
                Assert.True(regionalEvents.LockedStatePassed);
                break;
            case 65:
                Assert.All(regionalEvents.EventQualifications.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH),
                    item => Assert.True(item.AvailableStatePassed));
                break;
            case 66:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH),
                    item => Assert.Equal(0,
                        item.ResolutionReputationDelta));
                break;
            case 67:
                Assert.All(regionalEvents.EventQualifications.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT),
                    item => Assert.True(item.AvailableStatePassed));
                break;
            case 68:
                Assert.All(overlay.Bindings.Where(item =>
                        item.EventKind ==
                        GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT),
                    item => Assert.Equal(0,
                        item.ResolutionReputationDelta));
                break;
            case 69:
                Assert.All(overlay.Inventory, item =>
                    Assert.Equal(item.DialogueId, item.ResolutionFlagId));
                break;
            case 70:
                Assert.All(regionalEvents.EventQualifications,
                    item => Assert.True(item.ExactlyOncePassed));
                break;
            case 71:
                Assert.All(regionalEvents.EventQualifications,
                    item => Assert.True(item.LockedStatePassed));
                break;
            case 72:
                Assert.True(regionalEvents.RuntimeQualificationPassed);
                break;
            case 73:
                Assert.All(regionalEvents.EventQualifications,
                    item => Assert.True(item.ReplayPassed));
                break;
            case 74:
                Assert.Contains(regionalEvents.RuntimeFrames, item =>
                    item.CommandType.StartsWith("Move.",
                        StringComparison.Ordinal));
                Assert.Contains(regionalEvents.RuntimeFrames, item =>
                    item.CommandType.Contains("Interact",
                        StringComparison.Ordinal));
                break;
            case 75:
                Assert.All(regionalEvents.RuntimeFrames,
                    item => Assert.True(item.Passed));
                break;
            case 76:
                Assert.True(regionalEvents.ExactlyOncePassed);
                break;
            case 77:
            case 78:
            case 79:
                Assert.True(Goal169RegionalEventFixture
                    .ProjectionStatuses.Contains(
                        (GeneratedCampaignRegionalEventStatus)
                        (contract - 77)));
                break;
            case 80:
                Assert.True(Goal169RegionalEventFixture.HumanMarkerPassed);
                break;
            case 81:
                Assert.True(Goal169RegionalEventFixture.OtherRegionHuman);
                break;
            case 82:
                Assert.True(Goal169RegionalEventFixture.EventsTabPresent);
                break;
            case 83:
                Assert.True(Goal169RegionalEventFixture.NoRawIds);
                break;
            case 84:
                Assert.True(Goal169RegionalEventFixture.LayoutFits);
                break;
            case 85:
                Assert.True(relationships.Passed);
                Assert.True(regionalEvents.Passed);
                break;
            case 86:
                Assert.Empty(profiles["no-branches"].Events.Bindings);
                break;
            default:
                throw new InvalidOperationException(
                    "goal169.unknown_contract");
        }
    }

    private static string EventKey(
        GeneratedCampaignRegionalEventBinding value) =>
        JsonSerializer.Serialize(new
        {
            value.RegionalEventId,
            value.EventKind,
            value.RegionId,
            value.MapId,
            value.Placement.X,
            value.Placement.Y
        });
}

internal sealed record Goal169ProfileResult(
    GamePackageDefinition Package,
    GameProjectGeneratedCampaignRelationshipSummary Summary,
    GeneratedCampaignRegionalEventBindingResult Events)
{
    internal int RuntimeStartCount => Summary.BranchQualifications
        .Sum(item => item.RuntimeStartCount);

    internal GeneratedCampaignRelationshipBranchQualification Unavailable(
        GeneratedCampaignRelationshipBranch branch) =>
        Summary.BranchQualifications.Single(item =>
            item.Branch == branch && !item.Available);
}

internal static class Goal169ProfileFixture
{
    private static readonly Lazy<IReadOnlyDictionary<string,
        Goal169ProfileResult>> Values = new(Create);

    internal static IReadOnlyDictionary<string, Goal169ProfileResult> All =>
        Values.Value;

    internal static GeneratedCampaignRelationshipOverlayResult
        InvalidSupport => InvalidSupportLazy.Value;

    private static readonly Lazy<
        GeneratedCampaignRelationshipOverlayResult> InvalidSupportLazy =
        new(() => Build(
            [GeneratedCampaignRelationshipBranch.SUPPORT],
            keepArc: false).Overlay);

    private static IReadOnlyDictionary<string, Goal169ProfileResult> Create()
    {
        return new Dictionary<string, Goal169ProfileResult>(
            StringComparer.Ordinal)
        {
            ["all-branches"] = Qualify(
                Enum.GetValues<GeneratedCampaignRelationshipBranch>(),
                true),
            ["challenge-only-zero-arc"] = Qualify(
                [GeneratedCampaignRelationshipBranch.CHALLENGE],
                false),
            ["support-refuse"] = Qualify(
                [
                    GeneratedCampaignRelationshipBranch.SUPPORT,
                    GeneratedCampaignRelationshipBranch.REFUSE
                ], true),
            ["support-only"] = Qualify(
                [GeneratedCampaignRelationshipBranch.SUPPORT], true),
            ["refuse-only"] = Qualify(
                [GeneratedCampaignRelationshipBranch.REFUSE], false),
            ["no-branches"] = Qualify([], false)
        };
    }

    private static Goal169ProfileResult Qualify(
        IReadOnlyList<GeneratedCampaignRelationshipBranch> branches,
        bool keepArc)
    {
        var built = Build(branches, keepArc);
        Assert.True(built.Overlay.Passed,
            string.Join(Environment.NewLine, built.Overlay.Diagnostics));
        var combat = branches.Contains(
                         GeneratedCampaignRelationshipBranch.CHALLENGE)
                     || keepArc
            ? Goal168TestKit.SummaryFor(
                built.Overlay.RelationshipOverlayPackage,
                Goal168TestKit.Combat.QualifiedActions)
            : null;
        var summary =
            new GameProjectGeneratedCampaignRelationshipQualificationService()
                .Qualify(
                    built.Overlay.RelationshipOverlayPackage,
                    built.Overlay.Document, combat,
                    Goal168TestKit.Real.Runtime);
        Assert.True(summary.Passed,
            string.Join(Environment.NewLine, summary.Diagnostics));
        var events = new GeneratedCampaignRegionalEventBindingService()
            .Bind(built.Overlay.RelationshipOverlayPackage,
                built.Overlay.Document);
        Assert.True(events.Passed,
            string.Join(Environment.NewLine, events.Diagnostics));
        return new Goal169ProfileResult(
            built.Overlay.RelationshipOverlayPackage, summary, events);
    }

    private static (GeneratedCampaignRelationshipOverlayResult Overlay,
        GeneratedCampaignRelationshipBinding Binding) Build(
        IReadOnlyList<GeneratedCampaignRelationshipBranch> branches,
        bool keepArc)
    {
        var relationship = Goal168RelationshipFixture.Binding.Bindings
            .First(item => item.QuestArc.Count > 0
                           && !string.IsNullOrWhiteSpace(
                               item.ChallengeEncounterId));
        relationship = relationship with
        {
            Branches = branches.OrderBy(item => item).ToList(),
            QuestArc = keepArc ? relationship.QuestArc : []
        };
        var binding = new GeneratedCampaignRelationshipBindingResult
        {
            Passed = true,
            Bindings = [relationship]
        };
        var overlay = new GeneratedCampaignRelationshipOverlayService()
            .Build(Goal164TestKit.Clone(
                    Goal168TestKit.Package),
                binding);
        return (overlay, relationship);
    }
}
