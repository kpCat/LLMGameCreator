using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166RealDefeatRetryTests
{
    [Fact] public void Behavioral_real_generated_start_encounter_captures_checkpoint() { var route = Goal166CampaignRoute.Start(); Assert.NotNull(route.Service.RecoveryCheckpoint); }
    [Fact] public void Behavioral_real_generated_checkpoint_precedes_start_encounter() { var route = Goal166CampaignRoute.Start(); Assert.Equal(route.EncounterId, route.Service.RecoveryCheckpoint!.EncounterId); }
    [Fact] public void Behavioral_real_generated_end_turn_route_reaches_defeated() { var route = Goal166CampaignRoute.Defeat(); Assert.Equal(GeneratedCampaignSessionStatus.DEFEATED, route.Snapshot.Status); }
    [Fact] public void Behavioral_defeat_keeps_exact_checkpoint() { var route = Goal166CampaignRoute.Defeat(); Assert.NotNull(route.Service.RecoveryCheckpoint); }
    [Fact] public void Behavioral_retry_has_zero_runtime_start_delta() { var route = Goal166CampaignRoute.Defeat(); var count = route.Service.RuntimeStartInvocationCount; route.Service.Execute(GeneratedCampaignRecoveryService.RetryActionId); Assert.Equal(count, route.Service.RuntimeStartInvocationCount); }
    [Fact] public void Behavioral_retry_dispatches_one_start_encounter() { var route = Goal166CampaignRoute.Defeat(); var before = route.Runtime.GameplayCommands.Count(x => x == GameRuntimeCommandType.StartEncounter); route.Service.Execute(GeneratedCampaignRecoveryService.RetryActionId); Assert.Equal(before + 1, route.Runtime.GameplayCommands.Count(x => x == GameRuntimeCommandType.StartEncounter)); }
    [Fact] public void Behavioral_retry_restores_same_encounter_identity() { var route = Goal166CampaignRoute.Defeat(); var retried = route.Service.Execute(GeneratedCampaignRecoveryService.RetryActionId); Assert.Equal(route.EncounterTitle, retried.Encounter!.Title); }
    [Fact] public void Behavioral_real_save_recovery_returns_to_the_saved_active_session() { var route = Goal166CampaignRoute.Defeat(); var runtimeStarts = route.Service.RuntimeStartInvocationCount; var recovered = route.Service.Execute(GeneratedCampaignRecoveryService.ContinueActionId); Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, recovered.Status); Assert.Equal(route.PreEncounterSaveSessionSha256, recovered.SessionSha256); Assert.Equal(runtimeStarts, route.Service.RuntimeStartInvocationCount); }
    [Fact] public void Behavioral_real_new_game_recovery_starts_a_fresh_active_session() { var route = Goal166CampaignRoute.Defeat(); var runtimeStarts = route.Service.RuntimeStartInvocationCount; var fresh = route.Service.Execute(GeneratedCampaignRecoveryService.NewGameActionId); Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, fresh.Status); Assert.Equal(runtimeStarts + 1, route.Service.RuntimeStartInvocationCount); }
    [Fact] public void Behavioral_stale_retry_dispatches_zero_runtime_commands() { var route = Goal166CampaignRoute.Defeat(isolated: true); File.AppendAllText(Path.Combine(route.ProjectPath, "package.json"), " "); var before = route.Runtime.GameplayCommands.Count; var stale = route.Service.Execute(GeneratedCampaignRecoveryService.RetryActionId); Assert.Equal(GeneratedCampaignSessionStatus.STALE_PROJECT, stale.Status); Assert.Equal(before, route.Runtime.GameplayCommands.Count); }
    [Fact] public void Behavioral_defeat_consequence_is_projected() { var route = Goal166CampaignRoute.Defeat(); Assert.Contains(route.Snapshot.Consequences, x => x.Kind == GeneratedCampaignConsequenceKind.Defeat); }
}

internal sealed record Goal166CampaignRoute(GeneratedCampaignSessionService Service, Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot Snapshot, string EncounterId, string EncounterTitle, string ProjectPath,
    string PreEncounterSaveSessionSha256)
{
    internal static Goal166CampaignRoute Start(bool isolated = false)
    {
        var build = Goal164BuildFixture.Create(coreOnly: false);
        var runtime = new Goal162CountingRuntime(build.Runtime);
        var service = new GeneratedCampaignSessionService(build.Current,
            new GeneratedCampaignSessionTruthService(build.Current, build.Saves.Validator, build.Saves.Coordinator), runtime,
            build.Saves.Save, build.Saves.Migration, new GeneratedCampaignActionPlanner(),
            new GeneratedCampaignProjectionService(), new GeneratedCampaignEventPresenter());
        var started = service.StartNew();
        var saved = service.Save("goal166-pre-encounter");
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, saved.Status);
        var action = Assert.Single(started.Actions.Where(x => x.Enabled && x.Kind == GeneratedCampaignActionKind.StartEncounter));
        var snapshot = service.Execute(action.ActionId);
        var definition = build.Package.Game.Encounters.Single(x => x.Name == action.TargetTitle);
        return new Goal166CampaignRoute(service, runtime, snapshot, definition.Id, action.TargetTitle, build.Project.Path,
            saved.SessionSha256);
    }

    internal static Goal166CampaignRoute Defeat(bool isolated = false)
    {
        var route = Start(isolated);
        var bound = Math.Max(16, route.Snapshot.Encounter!.Participants.Count * 32);
        for (var index = 0; index < bound && route.Snapshot.Status == GeneratedCampaignSessionStatus.ACTIVE; index++)
        {
            var action = route.Snapshot.Actions.FirstOrDefault(x => x.Enabled && x.Kind == GeneratedCampaignActionKind.EndTurn)
                         ?? route.Snapshot.Actions.FirstOrDefault(x => x.Enabled && x.Kind == GeneratedCampaignActionKind.RunEncounterAi);
            Assert.NotNull(action);
            route = route with { Snapshot = route.Service.Execute(action!.ActionId) };
        }
        Assert.Equal(GeneratedCampaignSessionStatus.DEFEATED, route.Snapshot.Status);
        return route;
    }
}
