using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class GeneratedQuestDialoguePreviewServiceTests
{
    [Fact]
    public void QuestDialoguePreviewSupportsNpcDialogueAndInMemoryQuestJournal()
    {
        var service = new GeneratedQuestDialoguePreviewService();
        service.StartSession(Package());

        var linked = Assert.Single(service.FindDialoguesLinkedToNpc("npc/guide"));
        Assert.Equal("dialogue/intro", linked.SourceId);

        var dialogue = service.PreviewDialogue("dialogue/intro");
        Assert.True(dialogue.Ok);
        Assert.Equal(new[] { "Hello.", "Welcome." }, dialogue.Lines);

        var initial = service.BuildJournal();
        Assert.Equal(1, initial.AvailableCount);
        var started = service.StartQuest("quest/intro");
        Assert.True(started.Ok);
        Assert.Equal(GeneratedQuestPreviewStatus.Active, started.QuestStatus);
        Assert.Equal(1, service.BuildJournal().ActiveCount);

        var firstStep = service.MarkNextStep("quest/intro");
        Assert.Equal(1, firstStep.CompletedStepCount);
        Assert.Equal("Return", Assert.Single(service.BuildJournal().Entries).CurrentStep);
        var completed = service.MarkNextStep("quest/intro");
        Assert.Equal(GeneratedQuestPreviewStatus.Completed, completed.QuestStatus);
        Assert.Equal(1, service.BuildJournal().CompletedCount);
    }

    private static GamePackageDefinition Package()
    {
        return new GamePackageDefinition
        {
            GeneratedContent = new GeneratedContentDefinition
            {
                Npcs =
                [
                    new GeneratedNpcDefinition { SourceId = "npc/guide", Name = "Guide" }
                ],
                Dialogues =
                [
                    new GeneratedDialogueDefinition
                    {
                        SourceId = "dialogue/intro",
                        Title = "Introduction",
                        NpcId = "npc/guide",
                        Lines = ["Hello.", "Welcome."]
                    }
                ],
                Quests =
                [
                    new GeneratedQuestSeedDefinition
                    {
                        SourceId = "quest/intro",
                        Title = "Introduction Quest",
                        Steps = ["Talk", "Return"]
                    }
                ]
            }
        };
    }
}
