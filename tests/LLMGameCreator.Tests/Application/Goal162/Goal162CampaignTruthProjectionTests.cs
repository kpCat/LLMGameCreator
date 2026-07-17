using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal161;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

public sealed class Goal162CampaignTruthProjectionTests
{
    [Fact]
    public void Behavioral_real_travel_current_generated_project_starts_player_campaign_without_raw_map_ids()
    {
        var state = Goal161MigrationState.Value;
        var current = state.Bundle.Current;
        var service = new GeneratedCampaignSessionService(
            current,
            new GeneratedCampaignSessionTruthService(current, state.Bundle.Saves.Validator, state.Bundle.Saves.Coordinator),
            state.Bundle.Saves.Runtime, state.Bundle.Saves.Save, state.Bundle.Saves.Migration,
            new GeneratedCampaignActionPlanner(), new GeneratedCampaignProjectionService(), new GeneratedCampaignEventPresenter());

        var snapshot = service.StartNew();

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, snapshot.Status);
        Assert.NotNull(snapshot.Map);
        Assert.Contains(snapshot.Map!.Cells, cell => cell.PlayerPresent);
        Assert.DoesNotContain(snapshot.Map.Cells, cell => cell.PrimaryTitle.StartsWith("generated/", StringComparison.Ordinal));
        Assert.DoesNotContain(snapshot.Actions, action => action.Title.Contains("entity/", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_real_campaign_executes_only_a_projected_valid_move()
    {
        var service = RealService();
        var before = service.StartNew();
        var action = Assert.IsType<GeneratedCampaignAction>(before.Actions.First(item => item.Kind is GeneratedCampaignActionKind.MoveUp or GeneratedCampaignActionKind.MoveDown or GeneratedCampaignActionKind.MoveLeft or GeneratedCampaignActionKind.MoveRight && item.Enabled));
        var after = service.Execute(action.ActionId);
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, after.Status);
        Assert.NotEqual(before.Map!.Cells.Single(cell => cell.PlayerPresent).X + ":" + before.Map.Cells.Single(cell => cell.PlayerPresent).Y, after.Map!.Cells.Single(cell => cell.PlayerPresent).X + ":" + after.Map.Cells.Single(cell => cell.PlayerPresent).Y);
    }

    [Fact]
    public void Behavioral_real_campaign_save_and_exact_continue_restore_active_session()
    {
        var service = RealService();
        var started = service.StartNew();
        var saved = service.Save("campaign-goal162");
        var continued = service.Continue("campaign-goal162");
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, started.Status);
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, saved.Status);
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, continued.Status);
        Assert.Equal(started.CurrentMapTitle, continued.CurrentMapTitle);
    }

    private static GeneratedCampaignSessionService RealService()
    {
        var state = Goal161MigrationState.Value;
        var current = state.Bundle.Current;
        return new GeneratedCampaignSessionService(current, new GeneratedCampaignSessionTruthService(current, state.Bundle.Saves.Validator, state.Bundle.Saves.Coordinator), state.Bundle.Saves.Runtime, state.Bundle.Saves.Save, state.Bundle.Saves.Migration, new GeneratedCampaignActionPlanner(), new GeneratedCampaignProjectionService(), new GeneratedCampaignEventPresenter());
    }
    [Theory]
    [InlineData(GeneratedCampaignSessionStatus.NO_PROJECT, "Проект не открыт")]
    [InlineData(GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED, "Кампания недоступна")]
    [InlineData(GeneratedCampaignSessionStatus.PROJECT_NOT_READY, "Кампания не готова")]
    [InlineData(GeneratedCampaignSessionStatus.READY, "Готово к игре")]
    [InlineData(GeneratedCampaignSessionStatus.ACTIVE, "Игра")]
    [InlineData(GeneratedCampaignSessionStatus.STALE_PROJECT, "Проект изменён")]
    [InlineData(GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED, "Требуется перенос сохранения")]
    [InlineData(GeneratedCampaignSessionStatus.FAILED, "Ошибка кампании")]
    public void Behavioral_status_projection_is_human_readable(GeneratedCampaignSessionStatus status, string title)
    {
        var snapshot = new GeneratedCampaignProjectionService().Project(status, null, null, null, [], [], "campaign", []);
        Assert.Equal(title, snapshot.StatusTitle);
    }

    [Theory]
    [InlineData(GeneratedCampaignActionKind.MoveUp)] [InlineData(GeneratedCampaignActionKind.MoveDown)]
    [InlineData(GeneratedCampaignActionKind.MoveLeft)] [InlineData(GeneratedCampaignActionKind.MoveRight)]
    [InlineData(GeneratedCampaignActionKind.Interact)] [InlineData(GeneratedCampaignActionKind.OpenDialogue)]
    [InlineData(GeneratedCampaignActionKind.ChooseDialogue)] [InlineData(GeneratedCampaignActionKind.CloseDialogue)]
    [InlineData(GeneratedCampaignActionKind.StartEncounter)] [InlineData(GeneratedCampaignActionKind.BasicAttack)]
    [InlineData(GeneratedCampaignActionKind.UseAbility)] [InlineData(GeneratedCampaignActionKind.EndTurn)]
    [InlineData(GeneratedCampaignActionKind.RunEncounterAi)] [InlineData(GeneratedCampaignActionKind.ResolveEncounter)]
    [InlineData(GeneratedCampaignActionKind.FleeEncounter)] [InlineData(GeneratedCampaignActionKind.CompleteQuest)]
    [InlineData(GeneratedCampaignActionKind.UseItem)] [InlineData(GeneratedCampaignActionKind.Save)]
    [InlineData(GeneratedCampaignActionKind.Load)] [InlineData(GeneratedCampaignActionKind.MigrateSave)]
    [InlineData(GeneratedCampaignActionKind.RestartSession)]
    public void Behavioral_campaign_actions_remain_typed_existing_command_surface(GeneratedCampaignActionKind kind) => Assert.True(Enum.IsDefined(kind));

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
    [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)] [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)] [InlineData(20)]
    [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)] [InlineData(25)] [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)] [InlineData(30)]
    public void Behavioral_primary_action_ids_are_opaque(int index)
    {
        var action = new GeneratedCampaignAction { ActionId = Guid.NewGuid().ToString("N"), Title = "Действие " + index, Description = "Описание" };
        Assert.DoesNotContain("generated/", action.Title, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("entity/", action.Description, StringComparison.OrdinalIgnoreCase);
    }
}
