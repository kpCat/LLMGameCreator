using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal161;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignSaveMigrationTests
{
    [Fact]
    public void Behavioral_save_writes_current_generated_gameplay_revision()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.FirstSave.Status);
        Assert.Equal("Сохранено", state.FirstSave.SaveState.Status);
        Assert.Equal(1, state.FirstSave.SaveState.RevisionCount);
        Assert.False(state.FirstSave.SaveState.Deduplicated);
    }

    [Fact]
    public void Behavioral_repeat_unchanged_save_deduplicates_without_revision_growth()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.True(state.SecondSave.SaveState.Deduplicated);
        Assert.Equal(state.FirstSave.SaveState.RevisionCount, state.SecondSave.SaveState.RevisionCount);
        Assert.Contains("новая ревизия не потребовалась", state.SecondSave.SaveState.LastResult,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_save_list_projects_current_status_without_raw_world_ids()
    {
        var state = Goal162SaveMigrationState.Value;
        var entry = Assert.Single(state.SaveList.Entries,
            entry => entry.SlotName == state.SlotName);
        var projection = Assert.Single(GeneratedCampaignProjectionService.ProjectSaves([entry]));

        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT, entry.Status);
        Assert.True(projection.CanContinue);
        Assert.False(projection.CanMigrate);
        Assert.DoesNotContain(entry.SavedWorldId, projection.SavedWorldTitle, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_clear_and_exact_continue_restore_map_and_player_position()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.READY, state.Cleared.Status);
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.Continued.Status);
        Assert.Equal(state.BeforeSave.CurrentMapTitle, state.Continued.CurrentMapTitle);
        Assert.Equal(Goal162TestKit.PlayerPosition(state.BeforeSave), Goal162TestKit.PlayerPosition(state.Continued));
    }

    [Fact]
    public void Behavioral_continue_does_not_invoke_runtime_start_again()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(1, state.SameWorldService.RuntimeStartInvocationCount);
        Assert.Contains(state.Continued.RecentEvents,
            message => message.Contains("продолжена", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_cross_world_continue_requires_explicit_migration()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED,
            state.MigrationRequired.Status);
        Assert.Empty(state.MigrationRequired.Actions);
        Assert.Equal(0, state.MigrationService.RuntimeStartInvocationCount);
    }

    [Fact]
    public void Behavioral_migration_preview_is_deterministic_and_zero_write()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.True(state.Preview.Passed, string.Join(Environment.NewLine, state.Preview.Diagnostics));
        Assert.Equal(state.SaveTreeBeforePreview, state.SaveTreeAfterPreview);
        Assert.NotEqual(state.Preview.SourceWorldId, state.Preview.TargetWorldId);
        Assert.False(string.IsNullOrWhiteSpace(state.Preview.CandidateSessionSha256));
    }

    [Fact]
    public void Behavioral_explicit_migration_apply_continues_without_runtime_start()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.Migrated.Status);
        Assert.Equal(0, state.MigrationService.RuntimeStartInvocationCount);
        Assert.Contains(state.Migrated.RecentEvents,
            message => message.Contains("перенесено", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(state.MigrationService.ListSaves().Entries,
            entry => entry.SlotName == "campaign" && entry.Status == GeneratedGameplaySaveStatus.CURRENT);
    }

    [Fact]
    public void Behavioral_migration_preview_reports_preserved_and_dropped_facts()
    {
        var preview = Goal162SaveMigrationState.Value.Preview;

        Assert.True(preview.PreservedCountsByKind.Values.Sum() > 0);
        Assert.True(preview.DroppedCountsByKind.Values.Sum() >= 0);
        Assert.True(preview.MapReset);
    }

    [Fact]
    public void Behavioral_core_only_matrix_starts_travels_interacts_and_saves_without_false_rc()
    {
        var state = Goal162SaveMigrationState.Value.Core;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.Started.Status);
        Assert.NotEqual(state.Started.CurrentMapTitle, state.AfterTravel.CurrentMapTitle);
        Assert.NotEqual(state.BeforeInteraction.SessionSha256, state.AfterInteraction.SessionSha256);
        Assert.True(state.Saved.SaveState.Status == "Сохранено",
            $"status={state.Saved.Status}; save={state.Saved.SaveState.Status}; " +
            $"diagnostics={string.Join(",", state.Saved.Diagnostics)}");
        Assert.Contains(state.Bundle.Saves.Save.List(state.Project.Path).Entries,
            entry => entry.SlotName == "core-campaign" && entry.Status == GeneratedGameplaySaveStatus.CURRENT);
        Assert.NotEqual("CURRENT", state.Bundle.Controller.Snapshot().ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_real_regeneration_stales_old_session_and_new_game_uses_new_truth()
    {
        var project = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "goal162-regeneration-route");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var runtime = new Goal162CountingRuntime(bundle.Saves.Runtime);
        var service = Goal162TestKit.Service(bundle, runtime);
        var started = service.StartNew();
        var oldSessionSha256 = started.SessionSha256;
        var oldWorldSeed = started.WorldSeed;
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal162-new-world"));
        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.Equal("GREEN", preview.Status);
        var applied = bundle.Controller.ApplyGeneratedWorldRegeneration(request, preview);
        Assert.True(applied.Applied, string.Join(Environment.NewLine, applied.Diagnostics));

        var stale = service.Refresh();
        Assert.Equal(GeneratedCampaignSessionStatus.STALE_PROJECT, stale.Status);
        Assert.Empty(stale.Actions);
        Assert.Equal(oldSessionSha256, stale.SessionSha256);
        Assert.Equal(oldWorldSeed, stale.WorldSeed);

        var restarted = service.StartNew();
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, restarted.Status);
        Assert.NotEqual(oldSessionSha256, restarted.SessionSha256);
        Assert.NotEqual(oldWorldSeed, restarted.WorldSeed);
        Assert.Equal(2, runtime.StartCount);
    }
}

internal static class Goal162SaveMigrationState
{
    private static readonly Lazy<Goal162SaveMigrationFixture> Fixture = new(Create);
    public static Goal162SaveMigrationFixture Value => Fixture.Value;

    private static Goal162SaveMigrationFixture Create()
    {
        const string slot = "goal162-save-resume";
        var service = Goal162TestKit.Service();
        var started = service.StartNew();
        var move = started.Actions.First(action => action.Enabled
            && action.Kind is GeneratedCampaignActionKind.MoveUp
                or GeneratedCampaignActionKind.MoveDown
                or GeneratedCampaignActionKind.MoveLeft
                or GeneratedCampaignActionKind.MoveRight);
        var beforeSave = service.Execute(move.ActionId);
        var firstSave = service.Save(slot);
        var secondSave = service.Save(slot);
        var list = service.ListSaves();
        var cleared = service.ClearSession();
        var continued = service.Continue(slot);

        var migrationService = Goal162TestKit.Service();
        var migrationRequired = migrationService.Continue("campaign");
        var beforePreview = Goal159TestKit.TreeHashes(
            Goal162TestKit.Bundle.Saves.Store.RootPath(Goal162TestKit.Migration.Project.Path));
        var preview = migrationService.PreviewMigration("campaign");
        var afterPreview = Goal159TestKit.TreeHashes(
            Goal162TestKit.Bundle.Saves.Store.RootPath(Goal162TestKit.Migration.Project.Path));
        var migrated = migrationService.MigrateAndContinue(preview);

        var corePair = Goal162TestKit.CoreBundle();
        var coreService = Goal162TestKit.Service(corePair.Bundle);
        var coreStarted = coreService.StartNew();
        var destination = corePair.Bundle.Current.CurrentPackage!.Game.Maps.First(map =>
            map.Name != coreStarted.CurrentMapTitle
            && corePair.Bundle.Current.CurrentPackage.GeneratedContent.Scenes.Any(scene =>
                scene.PackageMapId == map.Id));
        var afterTravel = Goal162TestKit.TravelTo(coreService, destination.Name);
        var target = afterTravel.Map!.Entities.First(entity => entity.Interactable
            && !entity.Title.StartsWith("Переход в ", StringComparison.Ordinal));
        var beforeInteraction = Goal162TestKit.MoveAdjacentTo(coreService, target.Title);
        var interaction = Assert.Single(beforeInteraction.Actions,
            action => action.Kind == GeneratedCampaignActionKind.Interact
                      && action.TargetTitle == target.Title);
        var afterInteraction = coreService.Execute(interaction.ActionId);
        var coreSaved = coreService.Save("core-campaign");
        var core = new Goal162CoreCampaignFixture(corePair.Project, corePair.Bundle, coreStarted,
            afterTravel, beforeInteraction, afterInteraction, coreSaved);
        return new Goal162SaveMigrationFixture(slot, service, beforeSave, firstSave, secondSave, list,
            cleared, continued, migrationService, migrationRequired, preview, beforePreview, afterPreview,
            migrated, core);
    }
}

internal sealed record Goal162SaveMigrationFixture(
    string SlotName,
    GeneratedCampaignSessionService SameWorldService,
    GeneratedCampaignSnapshot BeforeSave,
    GeneratedCampaignSnapshot FirstSave,
    GeneratedCampaignSnapshot SecondSave,
    GeneratedGameplaySaveListResult SaveList,
    GeneratedCampaignSnapshot Cleared,
    GeneratedCampaignSnapshot Continued,
    GeneratedCampaignSessionService MigrationService,
    GeneratedCampaignSnapshot MigrationRequired,
    GeneratedGameplaySaveMigrationPreview Preview,
    SortedDictionary<string, string> SaveTreeBeforePreview,
    SortedDictionary<string, string> SaveTreeAfterPreview,
    GeneratedCampaignSnapshot Migrated,
    Goal162CoreCampaignFixture Core);

internal sealed record Goal162CoreCampaignFixture(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    GeneratedCampaignSnapshot Started,
    GeneratedCampaignSnapshot AfterTravel,
    GeneratedCampaignSnapshot BeforeInteraction,
    GeneratedCampaignSnapshot AfterInteraction,
    GeneratedCampaignSnapshot Saved);
