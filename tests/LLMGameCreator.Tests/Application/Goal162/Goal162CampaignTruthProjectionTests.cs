using System.Text.RegularExpressions;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal161;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignTruthProjectionTests
{
    [Fact]
    public void Behavioral_real_generated_project_captures_complete_current_truth()
    {
        var capture = Goal162TestKit.TruthService().Capture();

        Assert.Equal(GeneratedCampaignSessionStatus.READY, capture.Status);
        var truth = Assert.IsType<GeneratedCampaignProjectTruth>(capture.Truth);
        Assert.All(new[]
        {
            truth.ProjectIdentityFingerprint, truth.WorldId, truth.GenerationSeed,
            truth.SourceRecordSha256, truth.SourceRequestSha256, truth.PlanSha256,
            truth.GeneratedBasePackageSha256, truth.PackageSha256,
            truth.CompositionPackageSha256, truth.FinalStateHash,
            truth.SelectedBuildHistorySha256,
            truth.QualifiedAuthoringFingerprint, truth.SelectedBuildHistoryFileName,
            truth.GeneratedStartMapId
        }, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.NotEmpty(truth.RegionMapBindings);
        Assert.Empty(capture.Diagnostics);
    }

    [Fact]
    public void Behavioral_refresh_before_start_reports_ready_without_starting_runtime()
    {
        var service = Goal162TestKit.Service();

        var snapshot = service.Refresh();

        Assert.Equal(GeneratedCampaignSessionStatus.READY, snapshot.Status);
        Assert.Equal(0, service.RuntimeStartInvocationCount);
        Assert.Null(snapshot.Map);
    }

    [Fact]
    public void Behavioral_new_campaign_invokes_runtime_start_exactly_once()
    {
        var service = Goal162TestKit.Service();

        var started = service.StartNew();
        var refreshed = service.Refresh();

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, started.Status);
        Assert.Equal(started.SessionSha256, refreshed.SessionSha256);
        Assert.Equal(1, service.RuntimeStartInvocationCount);
    }

    [Fact]
    public void Behavioral_start_projects_generated_map_region_player_and_seed()
    {
        var snapshot = Goal162TestKit.Service().StartNew();

        Assert.NotNull(snapshot.Map);
        Assert.Contains(snapshot.Map!.Cells, cell => cell.PlayerPresent);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.WorldTitle));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.WorldSeed));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.CurrentRegionTitle));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.CurrentMapTitle));
    }

    [Fact]
    public void Behavioral_projected_primary_surface_contains_no_raw_ids_hashes_or_paths()
    {
        var snapshot = Goal162TestKit.Service().StartNew();
        var text = Goal162TestKit.PrimaryText(snapshot);

        Assert.DoesNotContain("generated/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("entity/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".llmgc", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch("[A-Fa-f0-9]{48,}", text);
        Assert.DoesNotMatch("[A-Za-z]:\\\\", text);
    }

    [Fact]
    public void Behavioral_projected_valid_move_changes_player_position()
    {
        var service = Goal162TestKit.Service();
        var before = service.StartNew();
        var action = Assert.Single(before.Actions.Where(action => action.Enabled
            && action.Kind is GeneratedCampaignActionKind.MoveUp
                or GeneratedCampaignActionKind.MoveDown
                or GeneratedCampaignActionKind.MoveLeft
                or GeneratedCampaignActionKind.MoveRight).Take(1));

        var after = service.Execute(action.ActionId);

        Assert.NotEqual(Goal162TestKit.PlayerPosition(before), Goal162TestKit.PlayerPosition(after));
        Assert.Contains(after.RecentEvents, message => message.Contains("перемест", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_blocked_moves_are_visible_but_causally_disabled()
    {
        var service = Goal162TestKit.Service();
        var snapshot = service.StartNew();
        for (var index = 0; index < 32 && !snapshot.Actions.Any(action =>
                 action.Kind is GeneratedCampaignActionKind.MoveUp
                     or GeneratedCampaignActionKind.MoveDown
                     or GeneratedCampaignActionKind.MoveLeft
                     or GeneratedCampaignActionKind.MoveRight && !action.Enabled); index++)
        {
            var move = snapshot.Actions.First(action => action.Enabled
                && action.Kind is GeneratedCampaignActionKind.MoveUp
                    or GeneratedCampaignActionKind.MoveDown
                    or GeneratedCampaignActionKind.MoveLeft
                    or GeneratedCampaignActionKind.MoveRight);
            snapshot = service.Execute(move.ActionId);
        }
        var blocked = snapshot.Actions.Where(action => action.Kind is GeneratedCampaignActionKind.MoveUp
            or GeneratedCampaignActionKind.MoveDown
            or GeneratedCampaignActionKind.MoveLeft
            or GeneratedCampaignActionKind.MoveRight).Where(action => !action.Enabled).ToList();

        Assert.NotEmpty(blocked);
        Assert.All(blocked, action => Assert.False(string.IsNullOrWhiteSpace(action.DisabledReason)));
    }

    [Fact]
    public void Behavioral_generated_auto_start_quests_are_real_active_runtime_state()
    {
        var snapshot = Goal162TestKit.Service().StartNew();
        var expected = Goal162TestKit.Package.Game.Quests.Count(quest => quest.AutoStart);

        Assert.True(expected > 0);
        Assert.Equal(expected, snapshot.Quests.Count);
        Assert.All(snapshot.Quests, quest => Assert.Equal("Активно", quest.StateTitle));
        Assert.All(snapshot.Quests, quest => Assert.NotEmpty(quest.Objectives));
    }

    [Fact]
    public void Behavioral_action_ids_are_opaque_and_stable_for_unchanged_state()
    {
        var service = Goal162TestKit.Service();
        var started = service.StartNew();
        var refreshed = service.Refresh();

        Assert.Equal(started.Actions.Select(action => action.ActionId),
            refreshed.Actions.Select(action => action.ActionId));
        Assert.All(started.Actions, action => Assert.Matches("^a-[a-f0-9]{20}$", action.ActionId));
    }

    [Fact]
    public void Behavioral_technical_truth_is_separate_from_human_primary_projection()
    {
        var snapshot = Goal162TestKit.Service().StartNew();

        Assert.Contains("projectFolder", snapshot.TechnicalDetails.Keys);
        Assert.Contains("packageSha256", snapshot.TechnicalDetails.Keys);
        Assert.DoesNotContain(snapshot.TechnicalDetails.Values,
            value => Goal162TestKit.PrimaryText(snapshot).Contains(value, StringComparison.Ordinal));
    }
}

internal static class Goal162TestKit
{
    public static Goal161MigrationFixture Migration => Goal161MigrationState.Value;
    public static Goal161WorldBundle Bundle => Migration.Bundle;
    public static GamePackageDefinition Package => Bundle.Current.CurrentPackage
        ?? throw new InvalidOperationException("Goal162 fixture package is not loaded.");

    public static GeneratedCampaignSessionTruthService TruthService(Goal161WorldBundle? bundle = null)
    {
        bundle ??= Bundle;
        return new GeneratedCampaignSessionTruthService(
            bundle.Current,
            bundle.Saves.Validator,
            bundle.Saves.Coordinator);
    }

    public static GeneratedCampaignSessionService Service(
        Goal161WorldBundle? bundle = null,
        IUnifiedGameRuntimeService? runtime = null)
    {
        bundle ??= Bundle;
        return new GeneratedCampaignSessionService(
            bundle.Current,
            TruthService(bundle),
            runtime ?? bundle.Saves.Runtime,
            bundle.Saves.Save,
            bundle.Saves.Migration,
            new GeneratedCampaignActionPlanner(),
            new GeneratedCampaignProjectionService(),
            new GeneratedCampaignEventPresenter());
    }

    public static string PrimaryText(GeneratedCampaignSnapshot snapshot)
    {
        var values = new List<string>
        {
            snapshot.StatusTitle, snapshot.StatusDescription, snapshot.ProjectTitle,
            snapshot.WorldTitle, snapshot.WorldSeed, snapshot.CurrentRegionTitle,
            snapshot.CurrentMapTitle
        };
        values.AddRange(snapshot.Map?.Cells.Select(cell => cell.PrimaryTitle) ?? []);
        values.AddRange(snapshot.Map?.Entities.Select(entity => entity.Title) ?? []);
        values.AddRange(snapshot.Nearby.SelectMany(item => new[] { item.Title, item.Description }));
        values.AddRange(snapshot.Actions.SelectMany(action => new[]
        {
            action.Title, action.Description, action.DisabledReason, action.TargetTitle
        }));
        values.AddRange(snapshot.Resources.Concat(snapshot.Stats).Concat(snapshot.Progressions)
            .Concat(snapshot.Inventory).Concat(snapshot.Equipment).Concat(snapshot.Factions)
            .SelectMany(row => new[] { row.Title, row.Value }));
        values.AddRange(snapshot.Quests.SelectMany(quest => new[] { quest.Title, quest.StateTitle }
            .Concat(quest.Objectives.SelectMany(objective => new[] { objective.Title, objective.Progress }))));
        if (snapshot.Dialogue is not null)
            values.AddRange(new[] { snapshot.Dialogue.Title, snapshot.Dialogue.Speaker, snapshot.Dialogue.Text }
                .Concat(snapshot.Dialogue.Choices.SelectMany(choice => new[] { choice.Title, choice.Description })));
        if (snapshot.Encounter is not null)
            values.AddRange(new[] { snapshot.Encounter.Title, snapshot.Encounter.CurrentTurnTitle }
                .Concat(snapshot.Encounter.Participants.SelectMany(participant =>
                    new[] { participant.Title, participant.TeamTitle })));
        values.AddRange(snapshot.RecentEvents);
        if (snapshot.LastActionOutcome is not null)
            values.AddRange(new[]
            {
                snapshot.LastActionOutcome.ActionTitle,
                snapshot.LastActionOutcome.Summary
            });
        values.AddRange(snapshot.Consequences.SelectMany(item => new[]
        {
            item.Title, item.BeforeValue, item.AfterValue, item.Delta, item.Description
        }));
        return string.Join(Environment.NewLine, values.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    public static (int X, int Y) PlayerPosition(GeneratedCampaignSnapshot snapshot)
    {
        var player = Assert.Single(Assert.IsType<GeneratedCampaignMapProjection>(snapshot.Map).Cells,
            cell => cell.PlayerPresent);
        return (player.X, player.Y);
    }

    public static GeneratedCampaignSnapshot MoveAdjacentTo(
        GeneratedCampaignSessionService service,
        string targetTitle)
    {
        var snapshot = service.Refresh();
        var map = Assert.IsType<GeneratedCampaignMapProjection>(snapshot.Map);
        var target = Assert.Single(map.Entities, entity => entity.Title == targetTitle);
        var start = PlayerPosition(snapshot);
        var passable = map.Cells.Where(cell => cell.Walkable)
            .Select(cell => (cell.X, cell.Y)).ToHashSet();
        passable.Add(start);
        var goals = passable.Where(cell => Math.Abs(cell.X - target.X) + Math.Abs(cell.Y - target.Y) == 1)
            .ToHashSet();
        Assert.NotEmpty(goals);
        var queue = new Queue<(int X, int Y)>();
        var previous = new Dictionary<(int X, int Y), (int X, int Y)>();
        queue.Enqueue(start);
        previous[start] = start;
        (int X, int Y)? goal = null;
        while (queue.Count > 0 && goal is null)
        {
            var current = queue.Dequeue();
            if (goals.Contains(current))
            {
                goal = current;
                break;
            }

            foreach (var next in Neighbors(current).Where(passable.Contains))
            {
                if (previous.ContainsKey(next)) continue;
                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        Assert.NotNull(goal);
        var path = new List<(int X, int Y)>();
        var cursor = goal!.Value;
        while (cursor != start)
        {
            path.Add(cursor);
            cursor = previous[cursor];
        }
        path.Reverse();
        var position = start;
        foreach (var next in path)
        {
            var kind = next.X > position.X
                ? GeneratedCampaignActionKind.MoveRight
                : next.X < position.X
                    ? GeneratedCampaignActionKind.MoveLeft
                    : next.Y > position.Y
                        ? GeneratedCampaignActionKind.MoveDown
                        : GeneratedCampaignActionKind.MoveUp;
            var action = Assert.Single(snapshot.Actions,
                action => action.Kind == kind && action.Enabled);
            snapshot = service.Execute(action.ActionId);
            position = next;
        }

        Assert.Equal(goal.Value, PlayerPosition(snapshot));
        return snapshot;
    }

    public static GeneratedCampaignSnapshot Interact(
        GeneratedCampaignSessionService service,
        string targetTitle)
    {
        var snapshot = MoveAdjacentTo(service, targetTitle);
        var action = Assert.Single(snapshot.Actions,
            action => action.Kind == GeneratedCampaignActionKind.Interact
                      && action.TargetTitle == targetTitle && action.Enabled);
        return service.Execute(action.ActionId);
    }

    public static GeneratedCampaignSnapshot TravelTo(
        GeneratedCampaignSessionService service,
        string destinationMapTitle)
    {
        var target = "Переход в " + destinationMapTitle;
        return Interact(service, target);
    }

    public static IReadOnlyList<GeneratedCampaignSnapshot> Fight(
        GeneratedCampaignSessionService service,
        string encounterTitle,
        Goal162CountingRuntime? runtime = null)
    {
        var snapshots = new List<GeneratedCampaignSnapshot>();
        var before = service.Refresh();
        var start = Assert.Single(before.Actions,
            action => action.Kind == GeneratedCampaignActionKind.StartEncounter
                      && action.TargetTitle == encounterTitle);
        var snapshot = service.Execute(start.ActionId);
        snapshots.Add(snapshot);
        for (var index = 0; index < 80 && snapshot.Encounter is { Active: true }; index++)
        {
            var action = snapshot.Actions.FirstOrDefault(item => item.Enabled
                && item.Kind == GeneratedCampaignActionKind.BasicAttack)
                         ?? snapshot.Actions.FirstOrDefault(item => item.Enabled
                             && item.Kind == GeneratedCampaignActionKind.EndTurn);
            Assert.NotNull(action);
            snapshot = service.Execute(action.ActionId);
            snapshots.Add(snapshot);
        }

        Assert.NotNull(snapshot.Encounter);
        Assert.False(snapshot.Encounter!.Active,
            $"Encounter remained active. Status={snapshot.Status}; diagnostics={string.Join(",", snapshot.Diagnostics)}; " +
            $"dispatch={service.LastRuntimeDispatch?.CommandKind}:passed={service.LastRuntimeDispatch?.Passed}:" +
            $"runtimeSuccess={service.LastRuntimeDispatch?.UnifiedRuntimeResult.Success}:" +
            $"dispatchDiagnostics={string.Join(",", service.LastRuntimeDispatch?.Diagnostics ?? [])}:" +
            $"runtimeDiagnostics={string.Join(",", service.LastRuntimeDispatch?.UnifiedRuntimeResult.Diagnostics.Select(item => item.Code) ?? [])}; " +
            $"commands={string.Join(",", runtime?.GameplayCommands.GroupBy(item => item).Select(group => group.Key + "=" + group.Count()) ?? [])}; " +
            $"definition={CombatDefinitionSummary(encounterTitle)}; " +
            $"turn={snapshot.Encounter.CurrentTurnTitle}; participants=" +
            string.Join(" | ", snapshot.Encounter.Participants.Select(item =>
                $"{item.Title}:{item.TeamTitle}:resources={string.Join("/", item.Resources.Select(row => row.Value))}:alive={item.Alive}")));
        return snapshots;
    }

    private static string CombatDefinitionSummary(string encounterTitle)
    {
        var encounter = Package.Game.Encounters.Single(item => item.Name == encounterTitle);
        var player = encounter.Participants.Single(item => item.Team == "player");
        var abilities = player.Abilities.Select(id => Package.Game.Abilities.Single(item => item.Id == id))
            .Select(item => $"{item.Id}:kind={item.Kind}:resource={item.ResourceId}:power={item.Power}:" +
                            $"tags={string.Join("/", item.Tags)}:effects={string.Join("/", item.Effects.Select(effect => effect.Type + "(" + string.Join(",", effect.Args.Select(pair => pair.Key + "=" + pair.Value)) + ")"))}");
        return $"metadata={string.Join(",", encounter.Metadata.Select(item => item.Key + "=" + item.Value))};" +
               $"playerResources={string.Join(",", player.Resources.Select(item => item.Id))};" +
               $"abilities={string.Join("|", abilities)}";
    }

    public static GeneratedCampaignSnapshot InteractWithFirstLocalObject(
        GeneratedCampaignSessionService service)
    {
        var snapshot = service.Refresh();
        var target = Assert.IsType<GeneratedCampaignMapProjection>(snapshot.Map).Entities
            .First(entity => entity.Interactable
                             && !entity.Title.StartsWith("Переход в ", StringComparison.Ordinal));
        return Interact(service, target.Title);
    }

    public static (GeneratedProject Project, Goal161WorldBundle Bundle) CoreBundle()
    {
        var project = Goal156TestKit.Copy(Goal157BuildState.Value.CoreProject, "goal162-core-only");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var build = bundle.Controller.BuildAndQualify();
        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        return (project, bundle);
    }

    private static IEnumerable<(int X, int Y)> Neighbors((int X, int Y) cell)
    {
        yield return (cell.X, cell.Y - 1);
        yield return (cell.X, cell.Y + 1);
        yield return (cell.X - 1, cell.Y);
        yield return (cell.X + 1, cell.Y);
    }
}
