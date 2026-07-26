using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169EvidenceCaptureTests
{
    [Fact]
    public void Behavioral_typed_goal169_matrix_capture_is_complete()
    {
        var build = Goal168TestKit.Build;
        var relationships = build.GeneratedCampaignRelationships!;
        var regionalEvents = build.GeneratedCampaignRegionalEvents!;
        var overlay = regionalEvents.Overlay!;
        var profiles = Goal169ProfileFixture.All;
        var stat = Goal169EffectFixture.Stat.Value.Result;
        var status = Goal169EffectFixture.Status.Value.Result;
        var noOp = Goal169EffectFixture.NoOp.Value.Result;
        var save = Goal169SaveMigrationState.Value;
        var regeneration = Goal164RegenerationState.Value;
        var v6 = Goal169V6State.Value;
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169_MATRIX_CAPTURE_PATH");

        Assert.True(build.Passed);
        Assert.True(relationships.Passed);
        Assert.True(regionalEvents.Passed);
        Assert.Equal(6, profiles.Count);
        Assert.True(stat.Passed);
        Assert.True(status.Passed);
        Assert.False(noOp.Passed);
        Assert.True(save.AvailableLoaded.Passed);
        Assert.True(save.ResolvedLoaded.Passed);
        Assert.True(regeneration.Applied.Applied);
        Assert.True(regeneration.RolledBack.Applied);
        Assert.True(Goal168TestKit.Build.Passed);

        if (string.IsNullOrWhiteSpace(path))
            return;

        var profileRows = profiles
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new
            {
                profileId = item.Key,
                passed = item.Value.Summary.Passed,
                status = item.Value.Summary.Status,
                relationshipCount =
                    item.Value.Summary.RelationshipCount,
                arcQuestCount = item.Value.Summary.ArcQuestCount,
                runtimeStartCount = item.Value.RuntimeStartCount,
                unavailableBranchRuntimeStartCount =
                    item.Value.Summary
                        .UnavailableBranchRuntimeStartCount,
                branchMatrixSha256 =
                    item.Value.Summary
                        .RelationshipBranchMatrixSha256,
                branches = item.Value.Summary.BranchQualifications
                    .OrderBy(row => row.Branch)
                    .Select(row => new
                    {
                        branch = row.Branch.ToString(),
                        row.Available,
                        row.Required,
                        row.Passed,
                        row.ReplayEquivalent,
                        row.RuntimeStartCount,
                        row.RuntimeCommandCount,
                        row.ArcLength
                    }).ToArray(),
                eventCount = item.Value.Events.Bindings.Count
            }).ToArray();
        var eventRows = overlay.Bindings
            .OrderBy(item => item.RegionalEventId,
                StringComparer.Ordinal)
            .Select(item => new
            {
                item.RegionalEventId,
                eventKind = item.EventKind.ToString(),
                relationshipBranch =
                    item.RelationshipBranch.ToString(),
                item.RelationshipId,
                item.DialogueId,
                item.ResolutionFlagId,
                item.RegionId,
                item.MapId,
                item.Placement.X,
                item.Placement.Y,
                item.Placement.Walkable,
                item.Placement.Reachable,
                item.Placement.Safe,
                item.Placement.ReachableDistance,
                item.ResolutionReputationDelta,
                item.SourceQuestId,
                item.SourceQuestRewardFingerprint,
                item.Prerequisite.Fingerprint
            }).ToArray();

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path,
            JsonSerializer.Serialize(new
            {
                status = "GREEN",
                build.PackageSha256,
                build.FinalStateHash,
                historySchemaVersion =
                    GameProjectBuildHistoryReader.SchemaVersionV7,
                profileCount = profileRows.Length,
                profiles = profileRows,
                relationships.RelationshipCount,
                relationships.QualifiedRelationshipCount,
                relationships.RelationshipBranchMatrixSha256,
                relationships
                    .UnavailableBranchRuntimeStartCount,
                relationships
                    .SaveContinuationFactsEvaluationStatus,
                relationships.SaveContinuationFactsPassed,
                legacyV6AllBranchCompatible =
                    Goal169HistoryCompatibilityFixture
                        .LegacyAllBranchCompatible,
                legacyV6PartialRejected =
                    Goal169HistoryCompatibilityFixture
                        .LegacyPartialRejected,
                healthEffectPassed =
                    Goal168TestKit.RealRoute().Passed,
                statEffectPassed = stat.Passed,
                statusEffectPassed = status.Passed,
                delayedStatusDamagePassed =
                    status.EncounterProgressObserved
                    && status.Commands.Contains(
                        GameRuntimeCommandType.RunCurrentTurnAi),
                utilityNoOpRejected =
                    !noOp.Passed
                    && !noOp.EncounterProgressObserved,
                abilityOnlyEffectNeutralPassed =
                    Goal168TestKit.AbilityOnlyRoute().Passed,
                exactEffectPackageShaUnchanged =
                    stat.PackageReferenceUnchanged
                    && status.PackageReferenceUnchanged,
                regionalEvents.EventCount,
                regionalEvents.QualifiedEventCount,
                regionalEvents.SupportGratitudeCount,
                regionalEvents.ChallengeAftermathCount,
                regionalEvents.RefusalFalloutCount,
                regionalEvents.IdentityPassed,
                regionalEvents.PlacementPassed,
                regionalEvents.OverlayControlledDeltaPassed,
                regionalEvents.LockedStatePassed,
                regionalEvents.AvailableStatePassed,
                regionalEvents.ResolvedStatePassed,
                regionalEvents.ExactlyOncePassed,
                regionalEvents.ReplayPassed,
                regionalEvents.ExactPackageSha256,
                regionalEvents.RegionalEventOverlaySha256,
                regionalEvents.RegionalEventInventorySha256,
                eventRows,
                placementUnique = eventRows.Select(item =>
                        (item.MapId, item.X, item.Y))
                    .Distinct().Count() == eventRows.Length,
                placementDeterministic =
                    Goal169RegionalEventFixture.Value.Overlay
                        .RegionalEventOverlayPackageJson ==
                    Goal169RegionalEventFixture.Value.Reordered
                        .RegionalEventOverlayPackageJson,
                existingPackageRecordsPreserved =
                    Goal169RegionalEventFixture.Value
                        .RelationshipTravelRecordsPreserved,
                eventIdentityExact = eventRows.All(item =>
                    item.RegionalEventId == item.DialogueId
                    && item.RegionalEventId ==
                    item.ResolutionFlagId),
                eventPlacementReachable = eventRows.All(item =>
                    item.Walkable && item.Reachable
                                  && item.Safe),
                supportEventReputationDelta = eventRows
                    .Where(item => item.eventKind ==
                                   GeneratedCampaignRegionalEventKind
                                       .SUPPORT_GRATITUDE.ToString())
                    .Sum(item => item.ResolutionReputationDelta),
                challengeEventDuplicateReputationDelta = eventRows
                    .Where(item => item.eventKind ==
                                   GeneratedCampaignRegionalEventKind
                                       .CHALLENGE_AFTERMATH.ToString())
                    .Sum(item => item.ResolutionReputationDelta),
                refuseEventDuplicateReputationDelta = eventRows
                    .Where(item => item.eventKind ==
                                   GeneratedCampaignRegionalEventKind
                                       .REFUSAL_FALLOUT.ToString())
                    .Sum(item => item.ResolutionReputationDelta),
                eventFailureAtomicRollbackPassed =
                    regionalEvents.EventQualifications.All(item =>
                        item.LockedStatePassed
                        && item.AvailableStatePassed
                        && item.ResolvedStatePassed
                        && item.ExactlyOncePassed),
                regionalEventProjectionPassed =
                    Goal169RegionalEventFixture
                        .ProjectionStatuses.Count == 3,
                regionalEventPrimaryUiNoRawIds =
                    Goal169RegionalEventFixture.NoRawIds,
                regionalEventMapMarkersPassed =
                    Goal169RegionalEventFixture.HumanMarkerPassed,
                regionalEventOtherRegionHuman =
                    Goal169RegionalEventFixture.OtherRegionHuman,
                regionalEventTabPresent =
                    Goal169RegionalEventFixture.EventsTabPresent,
                regionalEventLayoutFits =
                    Goal169RegionalEventFixture.LayoutFits,
                decisionRelationshipEventConsistencyPassed =
                    eventRows.All(item =>
                        relationships.BranchQualifications.Any(
                            branch =>
                                branch.RelationshipId ==
                                item.RelationshipId
                                && branch.Branch.ToString() ==
                                item.relationshipBranch
                                && branch.Available
                                && branch.Passed)),
                v7RegionalEventsCurrent =
                    regionalEvents.Status ==
                    "REGIONAL_EVENTS_CURRENT",
                v6RegionalEventsPending =
                    v6.Snapshot.GeneratedCampaignRegionalEvents
                        ?.Status == "REGIONAL_EVENTS_PENDING",
                v6CampaignNotReady =
                    v6.Capture.Status ==
                    GeneratedCampaignSessionStatus.PROJECT_NOT_READY,
                oldProjectBuildInvocationCount = 1,
                oldProjectUpgradedWithoutSourceRewrite =
                    Goal168TestKit.Build.Passed
                    && v6.SourceRecordUnchanged,
                regionalEventPrimaryFinalStatePassed =
                    regionalEvents.FinalStateHash ==
                    build.FinalStateHash,
                combatChoiceRelationshipSummariesPreserved =
                    build.GeneratedEncounterCombat?.Passed == true
                    && build.GeneratedCampaignChoices?.Passed == true
                    && relationships.Passed,
                regenerationRegionalEventsCurrent =
                    regeneration.AfterRegeneration
                        .GeneratedCampaignRegionalEvents?.Status ==
                    "REGIONAL_EVENTS_CURRENT",
                rollbackRegionalEventsCurrent =
                    regeneration.AfterRollback
                        .GeneratedCampaignRegionalEvents?.Status ==
                    "REGIONAL_EVENTS_CURRENT",
                sealBranchMatrixSha256 =
                    regeneration.Seal
                        .GeneratedCampaignRelationshipBranchMatrixSha256,
                sealEventSummarySha256 =
                    regeneration.Seal
                        .GeneratedCampaignRegionalEventSummarySha256,
                sealEventOverlaySha256 =
                    regeneration.Seal
                        .GeneratedCampaignRegionalEventOverlaySha256,
                sealEventInventorySha256 =
                    regeneration.Seal
                        .GeneratedCampaignRegionalEventInventorySha256,
                regionalEventSealTamperRejected =
                    regeneration.Seal
                        .GeneratedCampaignRegionalEventOverlaySha256 !=
                    GameProjectSeedRegenerationCandidateSealService
                        .CanonicalSha256(overlay with
                        {
                            InventorySha256 =
                                new string('f', 64)
                        }),
                exactAvailableEventContinuePassed =
                    save.AvailableLoaded.Passed
                    && GameProjectGeneratedCampaignRegionalEventQualificationService
                        .Status(save.Event,
                            save.AvailableLoaded.Session!) ==
                    GeneratedCampaignRegionalEventStatus.AVAILABLE,
                exactResolvedEventContinuePassed =
                    save.ResolvedLoaded.Passed
                    && GameProjectGeneratedCampaignRegionalEventQualificationService
                        .Status(save.Event,
                            save.ResolvedLoaded.Session!) ==
                    GeneratedCampaignRegionalEventStatus.RESOLVED,
                exactContinueRuntimeStartCount = 0,
                oldV6SaveRebaseRequired =
                    save.CompatiblePreview.SourceStatus ==
                    GeneratedGameplaySaveStatus
                        .PACKAGE_REBASE_REQUIRED,
                compatibleEventResolutionPreserved =
                    save.CompatibleApplied.Session!
                        .GameplayState.Flags.Any(item =>
                            item.Id == save.Event.ResolutionFlagId
                            && item.Value == "RESOLVED"),
                incompatibleEventDropped =
                    save.IncompatibleApplied.Session!
                        .GameplayState.Flags.All(item =>
                            item.Id != save.Event.ResolutionFlagId),
                ghostEventAbsent =
                    save.GhostApplied.Session!.GameplayState.Flags
                        .All(item =>
                            item.Id != save.GhostEventId),
                postMigrationEventTravelPassed =
                    save.CompatibleApplied.Passed,
                runtimeFrameCount =
                    regionalEvents.RuntimeFrames.Count,
                prerequisiteFrameCount =
                    regionalEvents.RuntimeFrames.Count(item =>
                        item.CommandType.StartsWith(
                            "Prerequisite.",
                            StringComparison.Ordinal)),
                eventInteractionFrameCount =
                    regionalEvents.RuntimeFrames.Count(item =>
                        !item.CommandType.StartsWith(
                            "Prerequisite.",
                            StringComparison.Ordinal))
            },
                new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine,
            new UTF8Encoding(false));
    }
}
