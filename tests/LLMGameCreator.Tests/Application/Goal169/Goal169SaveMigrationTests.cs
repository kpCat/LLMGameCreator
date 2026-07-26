using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169SaveMigrationTests
{
    [Fact]
    public void Behavioral_build_save_continuation_truth_is_not_evaluated()
    {
        var relationships = Goal168TestKit.Build
            .GeneratedCampaignRelationships!;

        Assert.False(relationships.SaveContinuationFactsPassed);
        Assert.Equal("NOT_EVALUATED_AT_BUILD",
            relationships.SaveContinuationFactsEvaluationStatus);
    }

    [Fact]
    public void Behavioral_available_event_exact_save_continues_available()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.AvailableLoaded.Passed,
            string.Join(",", state.AvailableLoaded.Diagnostics));
        Assert.Equal(GeneratedCampaignRegionalEventStatus.AVAILABLE,
            GameProjectGeneratedCampaignRegionalEventQualificationService
                .Status(state.Event, state.AvailableLoaded.Session!));
    }

    [Fact]
    public void Behavioral_available_event_save_has_no_resolution_flag()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.DoesNotContain(
            state.AvailableLoaded.Session!.GameplayState.Flags,
            item => item.Id == state.Event.ResolutionFlagId);
    }

    [Fact]
    public void Behavioral_resolved_event_exact_save_continues_resolved()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.ResolvedLoaded.Passed,
            string.Join(",", state.ResolvedLoaded.Diagnostics));
        Assert.Equal(GeneratedCampaignRegionalEventStatus.RESOLVED,
            GameProjectGeneratedCampaignRegionalEventQualificationService
                .Status(state.Event, state.ResolvedLoaded.Session!));
        Assert.Equal("RESOLVED", Flag(
            state.ResolvedLoaded.Session!, state.Event.ResolutionFlagId));
    }

    [Fact]
    public void Behavioral_resolved_event_roundtrip_is_exactly_once()
    {
        var state = Goal169SaveMigrationState.Value;
        var loaded = state.ResolvedLoaded.Session!;
        var before = Reputation(loaded, state.Event.FactionId);
        var opened = state.Build.Runtime.ExecuteGameplayCommand(
            state.Package, loaded,
            GameRuntimeCommand.OpenDialogue(state.Event.DialogueId));
        var resolvedChoiceId = state.Event.DialogueId + "/resolved";
        var observed = state.Build.Runtime.ExecuteGameplayCommand(
            state.Package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(resolvedChoiceId));

        Assert.True(observed.Success);
        Assert.Equal(before,
            Reputation(observed.Session, state.Event.FactionId));
        Assert.Equal("RESOLVED",
            Flag(observed.Session, state.Event.ResolutionFlagId));
    }

    [Fact]
    public void Behavioral_compatible_migration_preserves_resolution_flag()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.CompatibleApplied.Passed,
            string.Join(",", state.CompatibleApplied.Diagnostics));
        Assert.Equal("RESOLVED", Flag(
            state.CompatibleApplied.Session!,
            state.Event.ResolutionFlagId));
        Assert.True(state.CompatiblePreview.PreservedCountsByKind
            .GetValueOrDefault("regional_event_resolution") > 0);
    }

    [Fact]
    public void Behavioral_old_v6_save_requires_explicit_package_rebase()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.Equal(
            GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED,
            state.CompatiblePreview.SourceStatus);
        Assert.True(state.CompatiblePreview.Passed,
            string.Join(",", state.CompatiblePreview.Diagnostics));
        Assert.True(state.CompatibleApplied.Passed,
            string.Join(",", state.CompatibleApplied.Diagnostics));
    }

    [Fact]
    public void Behavioral_incompatible_event_migration_drops_resolution()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.IncompatibleApplied.Passed,
            string.Join(",", state.IncompatibleApplied.Diagnostics));
        Assert.DoesNotContain(
            state.IncompatibleApplied.Session!.GameplayState.Flags,
            item => item.Id == state.Event.ResolutionFlagId);
        Assert.Contains(state.IncompatiblePreview.DroppedReasons,
            item => item.Contains(
                "generated_regional_event_incompatible",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_incompatible_event_migration_has_no_ghost_event()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.NotEqual(GeneratedCampaignRegionalEventStatus.RESOLVED,
            GameProjectGeneratedCampaignRegionalEventQualificationService
                .Status(state.Event,
                    state.IncompatibleApplied.Session!));
    }

    [Fact]
    public void Behavioral_unknown_event_flag_is_not_promoted_to_ghost()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.GhostApplied.Passed,
            string.Join(",", state.GhostApplied.Diagnostics));
        Assert.DoesNotContain(
            state.GhostApplied.Session!.GameplayState.Flags,
            item => item.Id == state.GhostEventId);
        Assert.Contains(state.GhostPreview.DroppedReasons,
            item => item.Contains(
                "generated_regional_event_incompatible",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_save_proofs_remain_separate_from_build_truth()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.True(state.AvailableLoaded.Passed);
        Assert.True(state.ResolvedLoaded.Passed);
        Assert.False(Goal168TestKit.Build
            .GeneratedCampaignRelationships!
            .SaveContinuationFactsPassed);
    }

    private static string Flag(UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Flags.Single(item =>
            item.Id == id).Value;

    private static double Reputation(UnifiedRuntimeSession session,
        string factionId) =>
        session.GameplayState.Factions.Single(item =>
            item.FactionId == factionId).Reputation;
}

internal static class Goal169SaveMigrationState
{
    private static readonly Lazy<Goal169SaveMigrationFixture> Fixture =
        new(Create);

    internal static Goal169SaveMigrationFixture Value => Fixture.Value;

    private static Goal169SaveMigrationFixture Create()
    {
        var build = Goal168TestKit.Real;
        var package = Goal168TestKit.Package;
        var support = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var relationship = support.Relationship;
        var regionalEvents = Goal168TestKit.Build
            .GeneratedCampaignRegionalEvents!.Overlay!;
        var regionalEvent = regionalEvents.Bindings.Single(item =>
            item.RelationshipId == relationship.RelationshipId
            && item.RelationshipBranch ==
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var available = CompleteSupport(build, package,
            support.Session, relationship);
        Assert.Equal(GeneratedCampaignRegionalEventStatus.AVAILABLE,
            GameProjectGeneratedCampaignRegionalEventQualificationService
                .Status(regionalEvent, available));

        var availableSaved = build.Saves.Save.Save(
            build.Project.Path, "goal169-event-available", available);
        Assert.True(availableSaved.Passed,
            string.Join(",", availableSaved.Diagnostics));
        var availableLoaded = build.Saves.Save.Load(
            build.Project.Path, "goal169-event-available");

        var opened = build.Runtime.ExecuteGameplayCommand(package,
            available,
            GameRuntimeCommand.OpenDialogue(regionalEvent.DialogueId));
        var resolved = build.Runtime.ExecuteGameplayCommand(package,
            opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(
                regionalEvent.DialogueId + "/resolve"));
        Assert.True(resolved.Success);
        Assert.Equal(GeneratedCampaignRegionalEventStatus.RESOLVED,
            GameProjectGeneratedCampaignRegionalEventQualificationService
                .Status(regionalEvent, resolved.Session));
        var resolvedSaved = build.Saves.Save.Save(
            build.Project.Path, "goal169-event-resolved",
            resolved.Session);
        Assert.True(resolvedSaved.Passed,
            string.Join(",", resolvedSaved.Diagnostics));
        var resolvedLoaded = build.Saves.Save.Load(
            build.Project.Path, "goal169-event-resolved");

        var compatible = resolvedSaved.Revision! with
        {
            ParentRevisionSha256 = null,
            PackageSha256 = new string('a', 64),
            RevisionSha256 = string.Empty
        };
        Write(build, "goal169-event-compatible", compatible);
        var compatiblePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal169-event-compatible");
        var compatibleApplied = Apply(build,
            "goal169-event-compatible", compatiblePreview);

        var incompatibleFingerprints =
            resolvedSaved.Revision.DefinitionFingerprints.Select(item =>
                item.Kind == "dialogue"
                && item.Id == regionalEvent.DialogueId
                    ? item with
                    {
                        CanonicalSha256 = new string('b', 64)
                    }
                    : item).ToList();
        var incompatible = resolvedSaved.Revision with
        {
            ParentRevisionSha256 = null,
            WorldId = "world/goal169-incompatible-event",
            DefinitionFingerprints = incompatibleFingerprints,
            RevisionSha256 = string.Empty
        };
        Write(build, "goal169-event-incompatible", incompatible);
        var incompatiblePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal169-event-incompatible");
        var incompatibleApplied = Apply(build,
            "goal169-event-incompatible", incompatiblePreview);

        var ghostEventId = relationship.RelationshipId
                           + "/regional-event/ghost";
        var ghostSession = build.Saves.Serializer
            .DeserializeUnifiedSession(
                build.Saves.Serializer.Serialize(resolved.Session));
        ghostSession.GameplayState.Flags.Add(new RuntimeFlagState
        {
            Id = ghostEventId,
            Value = "RESOLVED"
        });
        var ghostSaved = build.Saves.Save.Save(build.Project.Path,
            "goal169-event-ghost", ghostSession);
        Assert.True(ghostSaved.Passed,
            string.Join(",", ghostSaved.Diagnostics));
        var ghostRevision = ghostSaved.Revision! with
        {
            ParentRevisionSha256 = null,
            WorldId = "world/goal169-ghost",
            RevisionSha256 = string.Empty
        };
        Write(build, "goal169-event-ghost-migration",
            ghostRevision);
        var ghostPreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal169-event-ghost-migration");
        var ghostApplied = Apply(build,
            "goal169-event-ghost-migration", ghostPreview);

        return new Goal169SaveMigrationFixture(
            build, package, regionalEvent, availableLoaded,
            resolvedLoaded, compatiblePreview, compatibleApplied,
            incompatiblePreview, incompatibleApplied,
            ghostEventId, ghostPreview, ghostApplied);
    }

    private static UnifiedRuntimeSession CompleteSupport(
        Goal164BuildFixture build,
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        UnifiedRuntimeSession initial,
        GeneratedCampaignRelationshipBinding relationship)
    {
        var session = initial;
        foreach (var step in relationship.QuestArc)
        {
            if (!string.IsNullOrWhiteSpace(step.TargetEncounterId))
            {
                var combat =
                    new GeneratedCampaignExactCombatRouteService()
                        .Execute(
                            new GeneratedCampaignExactCombatRouteRequest
                            {
                                FinalPackage = package,
                                EncounterId = step.TargetEncounterId,
                                CombatSummary =
                                    Goal168TestKit.Combat,
                                Runtime = build.Runtime,
                                InitialSession = session,
                                Goal =
                                    GeneratedCampaignExactCombatRouteGoal
                                        .VICTORY
                            });
                Assert.True(combat.Passed,
                    string.Join(",", combat.Diagnostics));
                session = combat.Session;
            }

            var completed = build.Runtime.ExecuteGameplayCommand(
                package, session,
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.CompleteQuest,
                    Id = step.QuestId
                });
            Assert.True(completed.Success);
            var reopened = build.Runtime.ExecuteGameplayCommand(
                package, completed.Session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId));
            var followUp = package.Game.Dialogues.Single(item =>
                    item.Id == relationship.DialogueId).Nodes
                .SelectMany(item => item.Choices)
                .Single(item => step.Order + 1 <
                                relationship.QuestArc.Count
                    ? item.StartQuestId ==
                      relationship.QuestArc[step.Order + 1].QuestId
                    : item.Metadata.GetValueOrDefault(
                        "generatedRelationshipPhase")
                      == "followup/completed");
            var observed = build.Runtime.ExecuteGameplayCommand(
                package, reopened.Session,
                GameRuntimeCommand.ChooseDialogueOption(followUp.Id));
            Assert.True(observed.Success);
            session = observed.Session;
        }

        return session;
    }

    private static void Write(
        Goal164BuildFixture build,
        string slot,
        GeneratedGameplaySaveRevision revision)
    {
        var written = build.Saves.Store.WriteRevision(
            build.Project.Path, slot, revision);
        Assert.True(written.Passed,
            string.Join(",", written.Diagnostics));
    }

    private static GeneratedGameplaySaveMigrationResult Apply(
        Goal164BuildFixture build,
        string slot,
        GeneratedGameplaySaveMigrationPreview preview) =>
        build.Saves.Migration.Apply(
            new GeneratedGameplaySaveMigrationApplyRequest
            {
                ProjectFolder = build.Project.Path,
                SlotName = slot,
                SourceRevisionSha256 =
                    preview.SourceRevisionSha256,
                CandidateSessionSha256 =
                    preview.CandidateSessionSha256
            });
}

internal sealed record Goal169SaveMigrationFixture(
    Goal164BuildFixture Build,
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    GeneratedCampaignRegionalEventBinding Event,
    GeneratedGameplaySaveResult AvailableLoaded,
    GeneratedGameplaySaveResult ResolvedLoaded,
    GeneratedGameplaySaveMigrationPreview CompatiblePreview,
    GeneratedGameplaySaveMigrationResult CompatibleApplied,
    GeneratedGameplaySaveMigrationPreview IncompatiblePreview,
    GeneratedGameplaySaveMigrationResult IncompatibleApplied,
    string GhostEventId,
    GeneratedGameplaySaveMigrationPreview GhostPreview,
    GeneratedGameplaySaveMigrationResult GhostApplied);
