using System.Reflection;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal167;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168RelationshipUiTests
{
    [Fact]
    public void Behavioral_undecided_relationship_row_is_projected()
    {
        var fixture = Goal168UiFixture.Start();
        var row = Assert.Single(fixture.Rows, item =>
            item.Actor == fixture.ActorTitle);
        Assert.Equal(GeneratedCampaignRelationshipStatus.UNDECIDED,
            row.Status);
    }

    [Fact]
    public void Behavioral_supported_relationship_row_is_projected()
    {
        var fixture = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var row = fixture.Row();
        Assert.Equal(GeneratedCampaignRelationshipStatus.QUEST_ACTIVE,
            row.Status);
        Assert.Equal("Поддержка", row.Branch);
    }

    [Fact]
    public void Behavioral_active_quest_and_next_action_are_human_readable()
    {
        var row = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT).Row();
        Assert.False(string.IsNullOrWhiteSpace(row.CurrentQuest));
        Assert.False(string.IsNullOrWhiteSpace(row.NextAction));
    }

    [Fact]
    public void Behavioral_between_quests_projects_start_next_action()
    {
        var fixture = Goal168UiFixture.Multi(completedSteps: 1);
        var row = fixture.Row();
        Assert.Equal(GeneratedCampaignRelationshipStatus.SUPPORTED,
            row.Status);
        Assert.Contains(fixture.Relationship.QuestArc[1].QuestId,
            fixture.AvailableNextQuestIds);
        Assert.Contains("Вернуться", row.NextAction);
    }

    [Fact]
    public void Behavioral_completed_relationship_row_is_projected()
    {
        var fixture = Goal168UiFixture.Multi(completedSteps: 2);
        Assert.Equal(GeneratedCampaignRelationshipStatus.COMPLETED,
            fixture.Row().Status);
        Assert.Equal(2, fixture.Row().CompletedQuestCount);
    }

    [Fact]
    public void Behavioral_challenged_and_resolved_rows_follow_runtime_state()
    {
        var challenged = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.CHALLENGE);
        Assert.Equal(GeneratedCampaignRelationshipStatus.CHALLENGED,
            challenged.Row().Status);
        var resolved = challenged.ResolveChallenge();
        Assert.Equal(GeneratedCampaignRelationshipStatus.CHALLENGE_RESOLVED,
            resolved.Row().Status);
    }

    [Fact]
    public void Behavioral_refused_relationship_row_is_projected()
    {
        var fixture = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.REFUSE);
        Assert.Equal(GeneratedCampaignRelationshipStatus.REFUSED,
            fixture.Row().Status);
        Assert.Equal("Отказ", fixture.Row().Branch);
    }

    [Fact]
    public void Behavioral_relationship_counts_are_data_derived()
    {
        var fixture = Goal168UiFixture.Start();
        Assert.Equal(Goal168TestKit.RelationshipOverlay.RelationshipCount,
            fixture.Rows.Count);
        Assert.Equal(Goal168TestKit.RelationshipOverlay.ArcQuestCount,
            fixture.Rows.Sum(item => item.TotalQuestCount));
    }

    [Fact]
    public void Behavioral_decision_journal_and_relationship_use_same_flag()
    {
        var fixture = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var journal = new GeneratedCampaignDecisionJournalService().Project(
            fixture.Package, fixture.Session);
        var decision = Assert.Single(journal.Decisions, item =>
            item.ActorTitle == fixture.ActorTitle);
        Assert.Contains("Поддерж", decision.ChosenBranch,
            StringComparison.Ordinal);
        Assert.Equal("SUPPORT", fixture.Session.GameplayState.Flags.Single(item =>
            item.Id == fixture.Relationship.RelationshipId).Value);
    }

    [Fact]
    public void Behavioral_primary_relationship_ui_contains_no_technical_ids()
    {
        var fixture = Goal168UiFixture.Choose(
            GeneratedCampaignRelationshipBranch.SUPPORT);
        var text = BindPage(fixture.Snapshot());
        Assert.DoesNotContain(fixture.Relationship.RelationshipId, text,
            StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Relationship.ActorSeedId, text,
            StringComparison.Ordinal);
        Assert.Contains(fixture.ActorTitle, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Contract_relationships_fit_1100x720_campaign_surface()
    {
        RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl
            {
                Size = new System.Drawing.Size(1100, 720)
            };
            Assert.True(page.Width >= 1100);
            Assert.True(page.Height >= 720);
        });
    }

    [Fact]
    public void Behavioral_relationship_consequence_kind_is_state_backed()
    {
        var outcome = ExecutedFixture.Create(
            GeneratedCampaignBranchKind.SUPPORT).Outcome;
        Assert.Contains(outcome.Consequences, item =>
            item.Kind == GeneratedCampaignConsequenceKind.RelationshipStarted);
    }

    private static string BindPage(GeneratedCampaignSnapshot snapshot)
    {
        var result = string.Empty;
        RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            typeof(GeneratedCampaignPageControl).GetMethod("Bind",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, [snapshot]);
            result = (string)typeof(GeneratedCampaignPageControl)
                .GetProperty("RelationshipText",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(page)!;
        });
        return result;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
    }
}

internal sealed record Goal168UiFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    UnifiedRuntimeSession Session,
    GeneratedCampaignRelationshipOverlayDocument Overlay,
    GeneratedCampaignRelationshipBinding Relationship,
    IReadOnlyList<GeneratedCampaignRelationshipRow> Rows,
    IReadOnlyList<string> AvailableNextQuestIds)
{
    internal string ActorTitle => Package.Game.Dialogues.Single(item =>
        item.Id == Relationship.DialogueId).Title;

    internal static Goal168UiFixture Start()
    {
        var package = Goal168TestKit.Package;
        var started = Goal168TestKit.Real.Runtime.Start(package);
        Assert.True(started.Success);
        return Create(package, started.Session,
            Goal168TestKit.RelationshipOverlay,
            Goal168TestKit.RelationshipOverlay.Bindings[0]);
    }

    internal static Goal168UiFixture Choose(
        GeneratedCampaignRelationshipBranch branch)
    {
        var initial = Start();
        var relationship = branch == GeneratedCampaignRelationshipBranch.CHALLENGE
            ? initial.Overlay.Bindings.First(item =>
                !string.IsNullOrWhiteSpace(item.ChallengeEncounterId))
            : initial.Relationship;
        var opened = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            initial.Package, initial.Session,
            GameRuntimeCommand.OpenDialogue(relationship.DialogueId));
        var choice = initial.Package.Game.Dialogues.Single(item =>
                item.Id == relationship.DialogueId).Nodes
            .SelectMany(item => item.Choices).Single(item =>
                item.Metadata.GetValueOrDefault("generatedChoiceKind")
                == branch.ToString()
                && item.Metadata.GetValueOrDefault("generatedChoicePhase")
                == "initial");
        var chosen = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            initial.Package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(choice.Id));
        Assert.True(chosen.Success);
        return Create(initial.Package, chosen.Session, initial.Overlay,
            relationship);
    }

    internal static Goal168UiFixture Multi(int completedSteps)
    {
        var multi = Goal168SupportFixture.Multi;
        var relationship = multi.Summary.Overlay!.Bindings.Single();
        var started = Goal168TestKit.Real.Runtime.Start(multi.Package);
        var opened = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            multi.Package, started.Session,
            GameRuntimeCommand.OpenDialogue(relationship.DialogueId));
        var support = multi.Package.Game.Dialogues.Single(item =>
                item.Id == relationship.DialogueId).Nodes
            .SelectMany(item => item.Choices).Single(item =>
                item.Metadata.GetValueOrDefault("generatedChoiceKind")
                == "SUPPORT"
                && item.Metadata.GetValueOrDefault("generatedChoicePhase")
                == "initial");
        var chosen = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
            multi.Package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(support.Id));
        var session = chosen.Session;
        var combat = Goal168TestKit.SummaryFor(multi.Package,
            Goal168TestKit.Combat.QualifiedActions);
        for (var index = 0; index < completedSteps; index++)
        {
            var step = relationship.QuestArc[index];
            var won = new GeneratedCampaignExactCombatRouteService().Execute(
                new GeneratedCampaignExactCombatRouteRequest
                {
                    FinalPackage = multi.Package,
                    EncounterId = step.TargetEncounterId,
                    CombatSummary = combat,
                    Runtime = Goal168TestKit.Real.Runtime,
                    InitialSession = session,
                    Goal = GeneratedCampaignExactCombatRouteGoal.VICTORY
                });
            Assert.True(won.Passed,
                string.Join(Environment.NewLine, won.Diagnostics));
            var completed = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
                multi.Package, won.Session,
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.CompleteQuest,
                    Id = step.QuestId
                });
            Assert.True(completed.Success);
            session = completed.Session;
            if (index + 1 < completedSteps)
            {
                var next = relationship.QuestArc[index + 1];
                var reopened = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
                    multi.Package, session,
                    GameRuntimeCommand.OpenDialogue(relationship.DialogueId));
                var nextChoice = multi.Package.Game.Dialogues.Single(item =>
                        item.Id == relationship.DialogueId).Nodes
                    .SelectMany(item => item.Choices).Single(item =>
                        item.StartQuestId == next.QuestId);
                session = Goal168TestKit.Real.Runtime.ExecuteGameplayCommand(
                    multi.Package, reopened.Session,
                    GameRuntimeCommand.ChooseDialogueOption(nextChoice.Id))
                    .Session;
            }
        }
        return Create(multi.Package, session, multi.Summary.Overlay,
            relationship);
    }

    internal Goal168UiFixture ResolveChallenge()
    {
        var won = new GeneratedCampaignExactCombatRouteService().Execute(
            new GeneratedCampaignExactCombatRouteRequest
            {
                FinalPackage = Package,
                EncounterId = Relationship.ChallengeEncounterId,
                CombatSummary = Goal168TestKit.Combat,
                Runtime = Goal168TestKit.Real.Runtime,
                InitialSession = Session,
                Goal = GeneratedCampaignExactCombatRouteGoal.VICTORY
            });
        Assert.True(won.Passed,
            string.Join(Environment.NewLine, won.Diagnostics));
        return Create(Package, won.Session, Overlay, Relationship);
    }

    internal GeneratedCampaignRelationshipRow Row() => Rows.Single(item =>
        item.Actor == ActorTitle);

    internal GeneratedCampaignSnapshot Snapshot() => new()
    {
        Status = GeneratedCampaignSessionStatus.ACTIVE,
        ProjectTitle = Package.Manifest.Title,
        Relationships = Rows
    };

    private static Goal168UiFixture Create(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedCampaignRelationshipOverlayDocument overlay,
        GeneratedCampaignRelationshipBinding relationship)
    {
        var readiness = new GeneratedCampaignQuestReadinessService()
            .EvaluateAll(package, session);
        var rows = new GeneratedCampaignRelationshipProjectionService()
            .Project(package, session, overlay, readiness).Rows;
        var available = package.Game.Dialogues.Single(item =>
                item.Id == relationship.DialogueId).Nodes
            .SelectMany(item => item.Choices)
            .Where(item => item.StartQuestId is not null)
            .Select(item => item.StartQuestId!)
            .ToList();
        return new Goal168UiFixture(package, session, overlay, relationship,
            rows, available);
    }
}
