using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168SaveMigrationTests
{
    [Fact]
    public void Behavioral_exact_save_continues_relationship_flag_reputation_and_quest()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.True(state.Loaded.Passed,
            string.Join(",", state.Loaded.Diagnostics));
        Assert.Equal("SUPPORT", Flag(state.Loaded.Session!,
            state.Relationship.RelationshipId));
        Assert.Equal(QuestState(state.Support.Session,
                state.Relationship.QuestArc[0].QuestId),
            QuestState(state.Loaded.Session!,
                state.Relationship.QuestArc[0].QuestId));
        Assert.Equal(Reputation(state.Support.Session,
                state.Relationship.FactionId),
            Reputation(state.Loaded.Session!,
                state.Relationship.FactionId));
    }

    [Fact]
    public void Behavioral_exact_middle_arc_roundtrip_continues_second_quest()
    {
        var between = Goal168UiFixture.Multi(completedSteps: 1);
        var second = between.Relationship.QuestArc[1];
        var opened = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            between.Package, between.Session,
            GameRuntimeCommand.OpenDialogue(
                between.Relationship.RelationshipId));
        var choice = between.Package.Game.Dialogues.Single(item =>
                item.Id == between.Relationship.RelationshipId).Nodes
            .SelectMany(item => item.Choices)
            .Single(item => item.StartQuestId == second.QuestId);
        var started = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            between.Package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(choice.Id));
        var json = Goal168TestKit.Real.Saves.Serializer.Serialize(
            started.Session);
        var continued = Goal168TestKit.Real.Saves.Serializer
            .DeserializeUnifiedSession(json);

        Assert.Equal("completed", QuestState(continued,
            between.Relationship.QuestArc[0].QuestId));
        Assert.Equal("active", QuestState(continued, second.QuestId));
        Assert.Equal("SUPPORT", Flag(continued,
            between.Relationship.RelationshipId));
        var reopened = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            between.Package, continued,
            GameRuntimeCommand.OpenDialogue(
                between.Relationship.RelationshipId));
        Assert.True(reopened.Success);
    }

    [Fact]
    public void Behavioral_same_world_rebase_preserves_generated_arc_progress()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.True(state.SameWorldApplied.Passed,
            string.Join(",", state.SameWorldApplied.Diagnostics));
        Assert.Equal(QuestState(state.Support.Session,
                state.Relationship.QuestArc[0].QuestId),
            QuestState(state.SameWorldApplied.Session!,
                state.Relationship.QuestArc[0].QuestId));
        Assert.True(state.SameWorldPreview.PreservedCountsByKind
            .GetValueOrDefault("quest") > 0);
    }

    [Fact]
    public void Behavioral_same_world_rebase_preserves_relationship_truth()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.Equal("SUPPORT", Flag(state.SameWorldApplied.Session!,
            state.Relationship.RelationshipId));
        Assert.Equal(Reputation(state.Support.Session,
                state.Relationship.FactionId),
            Reputation(state.SameWorldApplied.Session!,
                state.Relationship.FactionId));
    }

    [Fact]
    public void Behavioral_world_migration_is_explicit_and_resets_map()
    {
        var preview = Goal168SaveMigrationState.Value.WorldPreview;

        Assert.Equal(GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED,
            preview.SourceStatus);
        Assert.True(preview.Passed,
            string.Join(",", preview.Diagnostics));
        Assert.True(preview.MapReset);
    }

    [Fact]
    public void Behavioral_world_migration_preserves_compatible_decision_reputation()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.True(state.WorldApplied.Passed,
            string.Join(",", state.WorldApplied.Diagnostics));
        Assert.Equal("SUPPORT", Flag(state.WorldApplied.Session!,
            state.Relationship.RelationshipId));
        Assert.Equal(Reputation(state.Support.Session,
                state.Relationship.FactionId),
            Reputation(state.WorldApplied.Session!,
                state.Relationship.FactionId));
        Assert.True(state.WorldPreview.PreservedCountsByKind
            .GetValueOrDefault("relationship_decision") > 0);
        Assert.True(state.WorldPreview.PreservedCountsByKind
            .GetValueOrDefault("relationship_reputation") > 0);
    }

    [Fact]
    public void Behavioral_world_migration_resets_generated_arc_progress()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.Equal("not_started", QuestState(
            state.WorldApplied.Session!,
            state.Relationship.QuestArc[0].QuestId));
        Assert.Contains(state.WorldPreview.DroppedReasons, item =>
            item.Contains("generated_relationship_arc_reset",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_incompatible_relationship_drops_decision_without_ghost()
    {
        var state = Goal168SaveMigrationState.Value;

        Assert.True(state.IncompatibleApplied.Passed,
            string.Join(",", state.IncompatibleApplied.Diagnostics));
        Assert.DoesNotContain(
            state.IncompatibleApplied.Session!.GameplayState.Flags,
            item => item.Id == state.Relationship.RelationshipId);
        Assert.Empty(new GeneratedCampaignDecisionJournalService().Project(
            Goal168TestKit.Package,
            state.IncompatibleApplied.Session).Decisions);
    }

    private static string Flag(UnifiedRuntimeSession session, string id) =>
        session.GameplayState.Flags.Single(item => item.Id == id).Value;

    private static string QuestState(UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Quests.SingleOrDefault(item =>
            item.QuestId == id)?.State
        ?? session.GameplayState.QuestStates.GetValueOrDefault(id)
        ?? "not_started";

    private static double Reputation(UnifiedRuntimeSession session,
        string factionId) =>
        session.GameplayState.Factions.Single(item =>
            item.FactionId == factionId).Reputation;
}

internal static class Goal168SaveMigrationState
{
    private static readonly Lazy<Goal168SaveMigrationFixture> Fixture =
        new(Create);
    internal static Goal168SaveMigrationFixture Value => Fixture.Value;

    private static Goal168SaveMigrationFixture Create()
    {
        var build = Goal168TestKit.Real;
        var support = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var relationship = support.Relationship;
        var saved = build.Saves.Save.Save(build.Project.Path,
            "goal168-relationship-current", support.Session);
        Assert.True(saved.Passed,
            string.Join(",", saved.Diagnostics));
        var loaded = build.Saves.Save.Load(build.Project.Path,
            "goal168-relationship-current");

        var sameWorld = saved.Revision! with
        {
            ParentRevisionSha256 = null,
            PackageSha256 = new string('a', 64),
            RevisionSha256 = string.Empty
        };
        Write(build, "goal168-same-world", sameWorld);
        var samePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal168-same-world");
        var sameApplied = Apply(build, "goal168-same-world",
            samePreview);

        var otherWorld = saved.Revision with
        {
            ParentRevisionSha256 = null,
            WorldId = "world/goal168-previous",
            RevisionSha256 = string.Empty
        };
        Write(build, "goal168-world", otherWorld);
        var worldPreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal168-world");
        var worldApplied = Apply(build, "goal168-world",
            worldPreview);

        var incompatibleFingerprints =
            saved.Revision.DefinitionFingerprints.Select(item =>
                item.Kind == "dialogue"
                && item.Id == relationship.RelationshipId
                    ? item with
                    {
                        SourceId = item.SourceId + "/incompatible",
                        CanonicalSha256 = new string('b', 64)
                    }
                    : item).ToList();
        var incompatible = saved.Revision with
        {
            ParentRevisionSha256 = null,
            WorldId = "world/goal168-incompatible",
            DefinitionFingerprints = incompatibleFingerprints,
            RevisionSha256 = string.Empty
        };
        Write(build, "goal168-incompatible", incompatible);
        var incompatiblePreview = build.Saves.Migration.Preview(
            build.Project.Path, "goal168-incompatible");
        var incompatibleApplied = Apply(build,
            "goal168-incompatible", incompatiblePreview);

        return new Goal168SaveMigrationFixture(
            support, relationship, saved, loaded,
            samePreview, sameApplied, worldPreview, worldApplied,
            incompatiblePreview, incompatibleApplied);
    }

    private static void Write(
        LLMGameCreator.Tests.Application.Goal164.Goal164BuildFixture build,
        string slot,
        GeneratedGameplaySaveRevision revision)
    {
        var written = build.Saves.Store.WriteRevision(
            build.Project.Path, slot, revision);
        Assert.True(written.Passed,
            string.Join(",", written.Diagnostics));
    }

    private static GeneratedGameplaySaveMigrationResult Apply(
        LLMGameCreator.Tests.Application.Goal164.Goal164BuildFixture build,
        string slot,
        GeneratedGameplaySaveMigrationPreview preview) =>
        build.Saves.Migration.Apply(
            new GeneratedGameplaySaveMigrationApplyRequest
            {
                ProjectFolder = build.Project.Path,
                SlotName = slot,
                SourceRevisionSha256 = preview.SourceRevisionSha256,
                CandidateSessionSha256 = preview.CandidateSessionSha256
            });
}

internal sealed record Goal168SaveMigrationFixture(
    Goal168UiFixture Support,
    GeneratedCampaignRelationshipBinding Relationship,
    GeneratedGameplaySaveResult Saved,
    GeneratedGameplaySaveResult Loaded,
    GeneratedGameplaySaveMigrationPreview SameWorldPreview,
    GeneratedGameplaySaveMigrationResult SameWorldApplied,
    GeneratedGameplaySaveMigrationPreview WorldPreview,
    GeneratedGameplaySaveMigrationResult WorldApplied,
    GeneratedGameplaySaveMigrationPreview IncompatiblePreview,
    GeneratedGameplaySaveMigrationResult IncompatibleApplied);
