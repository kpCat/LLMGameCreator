using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168SupportArcTests
{
    [Fact]
    public void Behavioral_support_applies_exact_positive_reputation() =>
        Assert.True(Goal168TestKit.Relationships.SupportPassed);

    [Fact]
    public void Behavioral_first_quest_is_started_by_initial_decision()
    {
        Assert.All(Goal168TestKit.RelationshipOverlay.Bindings, relationship =>
        {
            var dialogue = Goal168TestKit.Package.Game.Dialogues.Single(item =>
                item.Id == relationship.DialogueId);
            var support = dialogue.Nodes.SelectMany(item => item.Choices)
                .Single(item =>
                    item.Metadata.GetValueOrDefault("generatedChoiceKind")
                    == "SUPPORT"
                    && item.Metadata.GetValueOrDefault("generatedChoicePhase")
                    == "initial");
            Assert.Equal(relationship.QuestArc[0].QuestId,
                support.StartQuestId);
        });
    }

    [Fact]
    public void Behavioral_later_quests_require_not_started_transition()
    {
        var multi = Goal168SupportFixture.Multi;
        var next = multi.Summary.Overlay!.Bindings.Single().QuestArc[1];
        var choice = multi.Package.Game.Dialogues.SelectMany(item => item.Nodes)
            .SelectMany(item => item.Choices)
            .Single(item => item.StartQuestId == next.QuestId);
        Assert.Contains(choice.Requirements, item =>
            item.Kind == "quest_state" && item.Id == next.QuestId
            && item.Value == "not_started");
    }

    [Fact]
    public void Behavioral_every_arc_step_uses_exact_combat_and_manual_turn_in()
    {
        var multi = Goal168SupportFixture.Multi;
        var supportFrames = multi.Summary.RuntimeFrames.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.SUPPORT
            && item.ReplayIndex == 1).ToList();
        Assert.Equal(multi.Summary.ArcQuestCount,
            supportFrames.Count(item =>
                item.CommandType == GameRuntimeCommandType.CompleteQuest.ToString()));
        Assert.DoesNotContain(supportFrames, item =>
            item.CommandType == GameRuntimeCommandType.AdvanceQuestObjective.ToString());
    }

    [Fact]
    public void Behavioral_next_quest_starts_through_dialogue()
    {
        var multi = Goal168SupportFixture.Multi;
        var second = multi.Summary.Overlay!.Bindings.Single().QuestArc[1];
        Assert.Contains(multi.Summary.RuntimeFrames, item =>
            item.Branch == GeneratedCampaignRelationshipBranch.SUPPORT
            && item.ArcStep == second.Order
            && item.QuestId == second.QuestId
            && item.CommandType ==
            GameRuntimeCommandType.ChooseDialogueOption.ToString());
    }

    [Fact]
    public void Behavioral_all_arc_steps_complete_in_data_order()
    {
        var multi = Goal168SupportFixture.Multi;
        Assert.True(multi.Summary.ArcProgressionPassed,
            string.Join(Environment.NewLine, multi.Summary.Diagnostics));
        Assert.Equal(2, multi.Summary.QualifiedArcQuestCount);
        Assert.Equal(2, multi.Summary.MaximumObservedArcLength);
        WriteMultiArcCapture(multi.Summary);
    }

    [Fact]
    public void Behavioral_final_relationship_completed_response_is_observed()
    {
        var multi = Goal168SupportFixture.Multi;
        Assert.True(multi.Summary.SupportPassed);
        Assert.Contains(multi.Package.Game.Dialogues.SelectMany(item => item.Nodes)
            .SelectMany(item => item.Choices), item =>
            item.Metadata.GetValueOrDefault("generatedRelationshipPhase")
            == "followup/completed"
            && item.StartQuestId is null);
    }

    [Fact]
    public void Behavioral_support_full_arc_replays_independently()
    {
        var summary = Goal168SupportFixture.Multi.Summary;
        Assert.True(summary.SupportReplayEquivalent);
        var support = summary.RuntimeFrames.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.SUPPORT);
        Assert.Equal(new[] { 1, 2 },
            support.Select(item => item.ReplayIndex).Distinct().OrderBy(item => item));
    }

    [Fact]
    public void Behavioral_support_full_arc_keeps_exact_package()
    {
        var multi = Goal168SupportFixture.Multi;
        Assert.Equal(Goal168TestKit.PackageSha(multi.Package),
            multi.Summary.ExactPackageSha256);
        Assert.True(multi.Summary.ExactCombatCatalogPassed);
    }

    private static void WriteMultiArcCapture(
        GameProjectGeneratedCampaignRelationshipSummary summary)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL168_MULTI_ARC_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(new
            {
                status = "GREEN",
                summary.RelationshipCount,
                summary.QualifiedRelationshipCount,
                summary.ArcQuestCount,
                summary.QualifiedArcQuestCount,
                summary.MaximumObservedArcLength,
                summary.ArcProgressionPassed,
                summary.SupportPassed,
                summary.SupportReplayEquivalent,
                summary.ExactCombatCatalogPassed,
                completeQuestCommandCount = summary.RuntimeFrames.Count(item =>
                    item.Branch
                    == GeneratedCampaignRelationshipBranch.SUPPORT
                    && item.ReplayIndex == 1
                    && item.CommandType
                    == GameRuntimeCommandType.CompleteQuest.ToString()),
                nextQuestDialogueCount = summary.RuntimeFrames.Count(item =>
                    item.Branch
                    == GeneratedCampaignRelationshipBranch.SUPPORT
                    && item.ReplayIndex == 1
                    && item.ArcStep > 0
                    && item.CommandType
                    == GameRuntimeCommandType.ChooseDialogueOption.ToString())
            },
                new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine,
            new UTF8Encoding(false));
    }
}

internal static class Goal168SupportFixture
{
    private static readonly Lazy<QualifiedMultiArcFixture> MultiLazy =
        new(CreateMulti);
    internal static QualifiedMultiArcFixture Multi => MultiLazy.Value;

    private static QualifiedMultiArcFixture CreateMulti()
    {
        var package = Goal164TestKit.Clone(Goal168TestKit.Package);
        var relationship = Goal168TestKit.RelationshipOverlay.Bindings[0];
        var first = relationship.QuestArc[0];
        var sourceQuest = package.Game.Quests.Single(item =>
            item.Id == first.QuestId);
        var copy = Goal164TestKit.Clone(sourceQuest);
        copy.Id += "/goal168-runtime-second";
        copy.Title += " II";
        copy.Metadata["sourceQuestEventSeedId"] += "/goal168-runtime-second";
        package.Game.Quests.Add(copy);
        var second = first with
        {
            Order = 1,
            QuestId = copy.Id,
            QuestSourceId = copy.Metadata["sourceQuestEventSeedId"]
        };
        var binding = new GeneratedCampaignRelationshipBindingResult
        {
            Passed = true,
            Bindings =
            [
                relationship with
                {
                    QuestArc = [first with { Order = 0 }, second]
                }
            ]
        };
        var overlay = new GeneratedCampaignRelationshipOverlayService().Build(
            package, binding);
        Assert.True(overlay.Passed,
            string.Join(Environment.NewLine, overlay.Diagnostics));
        var combat = Goal168TestKit.SummaryFor(
            overlay.RelationshipOverlayPackage,
            Goal168TestKit.Combat.QualifiedActions);
        var summary =
            new GameProjectGeneratedCampaignRelationshipQualificationService()
                .Qualify(overlay.RelationshipOverlayPackage,
                    overlay.Document, combat, Goal168TestKit.Real.Runtime);
        Assert.True(summary.Passed,
            string.Join(Environment.NewLine, summary.Diagnostics));
        return new QualifiedMultiArcFixture(
            overlay.RelationshipOverlayPackage, summary);
    }
}

internal sealed record QualifiedMultiArcFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    GameProjectGeneratedCampaignRelationshipSummary Summary);
