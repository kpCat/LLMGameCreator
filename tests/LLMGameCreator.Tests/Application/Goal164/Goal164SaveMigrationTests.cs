using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal162;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164SaveMigrationTests
{
    [Fact]
    public void Behavioral_combat_campaign_save_writes_current_revision()
    {
        var saved = Goal164MigrationState.Value.Saved;

        Assert.Equal("Сохранено", saved.SaveState.Status);
        Assert.Equal(1, saved.SaveState.RevisionCount);
    }

    [Fact]
    public void Behavioral_exact_continue_preserves_post_combat_state()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Equal(route.Saved.SessionSha256, route.Continued.SessionSha256);
        Assert.Equal(route.Saved.CurrentMapTitle, route.Continued.CurrentMapTitle);
        Assert.Equal(route.Saved.Quests.Select(item => (item.Title, item.StateTitle)),
            route.Continued.Quests.Select(item => (item.Title, item.StateTitle)));
    }

    [Fact]
    public void Behavioral_regeneration_stales_old_combat_session()
    {
        var state = Goal164MigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.STALE_PROJECT, state.Stale.Status);
        Assert.Empty(state.Stale.Actions);
    }

    [Fact]
    public void Behavioral_cross_world_continue_requires_explicit_migration()
    {
        var state = Goal164MigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED,
            state.MigrationRequired.Status);
        Assert.Empty(state.MigrationRequired.Actions);
    }

    [Fact]
    public void Behavioral_migration_preview_is_zero_write_and_deterministic()
    {
        var state = Goal164MigrationState.Value;

        Assert.True(state.Preview.Passed, string.Join(",", state.Preview.Diagnostics));
        Assert.Equal(state.SaveTreeBeforePreview, state.SaveTreeAfterPreview);
        Assert.False(string.IsNullOrWhiteSpace(state.Preview.CandidateSessionSha256));
    }

    [Fact]
    public void Behavioral_migration_continues_on_v4_campaign_current_package()
    {
        var state = Goal164MigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.Migrated.Status);
        Assert.Equal("CAMPAIGN_CURRENT",
            state.Build.Controller.Snapshot().GeneratedEncounterCombat?.Status);
    }

    [Fact]
    public void Behavioral_post_migration_generated_combat_executes_exact_package_actions()
    {
        var state = Goal164MigrationState.Value;

        Assert.Contains(GameRuntimeCommandType.StartEncounter, state.Runtime.GameplayCommands);
        Assert.Contains(state.Runtime.GameplayCommands,
            item => item is GameRuntimeCommandType.BasicAttack or GameRuntimeCommandType.UseAbility);
        Assert.True(state.AfterPostMigrationAction.Encounter is { Active: true }
                    || state.AfterPostMigrationAction.Consequences.Any(item =>
                        item.Kind == GeneratedCampaignConsequenceKind.EncounterWon));
    }
}

internal static class Goal164MigrationState
{
    private static readonly Lazy<Goal164MigrationFixture> Fixture = new(Create);
    public static Goal164MigrationFixture Value => Fixture.Value;

    private static Goal164MigrationFixture Create()
    {
        const string slot = "goal164-migration";
        var build = Goal164BuildFixture.Create(coreOnly: false);
        var runtime = new Goal162CountingRuntime(build.Runtime);
        var service = CampaignService(build, runtime);
        var started = service.StartNew();
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, started.Status);
        var saved = service.Save(slot);
        var request = build.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(build.Controller.Snapshot(), "goal164-migration-world"));
        var preview = build.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.Equal("GREEN", preview.Status);
        var applied = build.Controller.ApplyGeneratedWorldRegeneration(request, preview);
        Assert.True(applied.Applied, string.Join(Environment.NewLine, applied.Diagnostics));
        var stale = service.Refresh();
        var migrationRequired = service.Continue(slot);
        var beforePreview = Goal159TestKit.TreeHashes(build.Saves.Store.RootPath(build.Project.Path));
        var migrationPreview = service.PreviewMigration(slot);
        var afterPreview = Goal159TestKit.TreeHashes(build.Saves.Store.RootPath(build.Project.Path));
        var migrated = service.MigrateAndContinue(migrationPreview);
        var start = migrated.Actions.FirstOrDefault(item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.StartEncounter);
        Assert.NotNull(start);
        var encounter = service.Execute(start!.ActionId);
        var action = encounter.Actions.FirstOrDefault(item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.UseAbility)
                     ?? encounter.Actions.FirstOrDefault(item => item.Enabled
                         && item.Kind == GeneratedCampaignActionKind.BasicAttack);
        Assert.NotNull(action);
        var afterAction = service.Execute(action!.ActionId);
        return new Goal164MigrationFixture(build, runtime, saved, stale, migrationRequired,
            migrationPreview, beforePreview, afterPreview, migrated, afterAction);
    }

    private static GeneratedCampaignSessionService CampaignService(
        Goal164BuildFixture build,
        Goal162CountingRuntime runtime) => new(
        build.Current,
        new GeneratedCampaignSessionTruthService(
            build.Current, build.Saves.Validator, build.Saves.Coordinator),
        runtime,
        build.Saves.Save,
        build.Saves.Migration,
        new GeneratedCampaignActionPlanner(),
        new GeneratedCampaignProjectionService(),
        new GeneratedCampaignEventPresenter());
}

internal sealed record Goal164MigrationFixture(
    Goal164BuildFixture Build,
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot Saved,
    GeneratedCampaignSnapshot Stale,
    GeneratedCampaignSnapshot MigrationRequired,
    GeneratedGameplaySaveMigrationPreview Preview,
    IReadOnlyDictionary<string, string> SaveTreeBeforePreview,
    IReadOnlyDictionary<string, string> SaveTreeAfterPreview,
    GeneratedCampaignSnapshot Migrated,
    GeneratedCampaignSnapshot AfterPostMigrationAction);
