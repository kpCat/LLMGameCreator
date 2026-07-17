using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignSessionService
{
    private readonly ICurrentGamePackageService _currentProject;
    private readonly GeneratedCampaignSessionTruthService _truths;
    private readonly IUnifiedGameRuntimeService _runtime;
    private readonly GeneratedGameplaySaveService _saves;
    private readonly GeneratedGameplaySaveMigrationService _migration;
    private readonly GeneratedCampaignActionPlanner _planner;
    private readonly GeneratedCampaignProjectionService _projection;
    private readonly GeneratedCampaignEventPresenter _events;
    private GeneratedCampaignSession? _session;
    private GeneratedCampaignSessionStatus _status;
    private List<string> _diagnostics = [];
    private List<string> _recentEvents = [];
    private GeneratedCampaignSaveState _saveState = new();

    public GeneratedCampaignSessionService(
        ICurrentGamePackageService currentProject,
        GeneratedCampaignSessionTruthService truths,
        IUnifiedGameRuntimeService runtime,
        GeneratedGameplaySaveService saves,
        GeneratedGameplaySaveMigrationService migration,
        GeneratedCampaignActionPlanner planner,
        GeneratedCampaignProjectionService projection,
        GeneratedCampaignEventPresenter events)
    {
        _currentProject = currentProject;
        _truths = truths;
        _runtime = runtime;
        _saves = saves;
        _migration = migration;
        _planner = planner;
        _projection = projection;
        _events = events;
    }

    public int RuntimeStartInvocationCount { get; private set; }

    public GeneratedCampaignSnapshot Refresh()
    {
        var captured = _truths.Capture();
        if (_session is not null
            && (captured.Truth is null
                || !GeneratedCampaignSessionTruthService.Same(_session.Truth, captured.Truth)))
        {
            _status = GeneratedCampaignSessionStatus.STALE_PROJECT;
            _diagnostics = ["campaign.project_truth_changed"];
        }
        else if (_session is null)
        {
            _status = captured.Status;
            _diagnostics = captured.Diagnostics.ToList();
        }

        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot StartNew()
    {
        var captured = _truths.Capture();
        var package = _currentProject.CurrentPackage;
        if (captured.Status != GeneratedCampaignSessionStatus.READY
            || captured.Truth is null
            || package is null)
        {
            _session = null;
            _status = captured.Status;
            _diagnostics = captured.Diagnostics.ToList();
            return Snapshot(captured.Truth);
        }

        RuntimeStartInvocationCount++;
        var start = _runtime.Start(package);
        _recentEvents = _events.Present(start).ToList();
        if (!start.Success)
        {
            _session = null;
            _status = GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = ["campaign.runtime_start_failed"];
            return Snapshot(captured.Truth);
        }

        var runtimeSession = start.Session;
        foreach (var quest in package.Game.Quests.Where(quest => quest.AutoStart))
        {
            if (runtimeSession.GameplayState.Quests.Any(item => IdEquals(item.QuestId, quest.Id))) continue;
            var questStart = _runtime.ExecuteGameplayCommand(package, runtimeSession,
                GameRuntimeCommand.StartQuest(quest.Id));
            runtimeSession = questStart.Session;
            _recentEvents.AddRange(_events.Present(questStart));
            if (!questStart.Success)
            {
                _status = GeneratedCampaignSessionStatus.FAILED;
                _diagnostics = ["campaign.auto_quest_start_failed"];
                _session = null;
                return Snapshot(captured.Truth);
            }
        }

        _session = new GeneratedCampaignSession(captured.Truth, package, runtimeSession, "campaign");
        _status = GeneratedCampaignSessionStatus.ACTIVE;
        _diagnostics = [];
        _saveState = new GeneratedCampaignSaveState { Slot = "campaign" };
        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot Execute(string actionId)
    {
        var captured = _truths.Capture();
        if (_session is null)
        {
            _status = captured.Status == GeneratedCampaignSessionStatus.READY
                ? GeneratedCampaignSessionStatus.READY
                : captured.Status;
            _diagnostics = ["campaign.session_not_started"];
            return Snapshot(captured.Truth);
        }

        if (captured.Truth is null
            || !GeneratedCampaignSessionTruthService.Same(_session.Truth, captured.Truth))
        {
            _status = GeneratedCampaignSessionStatus.STALE_PROJECT;
            _diagnostics = ["campaign.project_truth_changed"];
            return Snapshot(captured.Truth);
        }

        var planned = _planner.Plan(_session.Package, _session.RuntimeSession)
            .SingleOrDefault(item => item.Action.ActionId == actionId);
        if (planned is null)
        {
            _diagnostics = ["campaign.action_unknown"];
            return Snapshot(captured.Truth);
        }

        if (!planned.Action.Enabled)
        {
            _diagnostics = ["campaign.action_disabled"];
            return Snapshot(captured.Truth);
        }

        var result = ExecuteRuntimeAction(planned);
        if (result.Success && result.MapEvents.Any(item => item.Type == RuntimeEventType.MapChanged))
        {
            result.Session.GameplayState.CurrentMapId = result.Session.MapState.CurrentMapId;
        }
        _session = _session with { RuntimeSession = result.Session };
        _recentEvents = _events.Present(result).ToList();
        _diagnostics = result.Success
            ? []
            : ["campaign.runtime_command_failed", .. result.Diagnostics.Select(item => item.Code)];
        if (!result.Success) return Snapshot(captured.Truth);

        if (planned.Action.Kind == GeneratedCampaignActionKind.Interact)
        {
            OpenProjectedDialogue(planned);
            if (_diagnostics.Count > 0) return Snapshot(captured.Truth);
        }

        if (planned.Action.Kind is GeneratedCampaignActionKind.StartEncounter
            or GeneratedCampaignActionKind.BasicAttack
            or GeneratedCampaignActionKind.UseAbility
            or GeneratedCampaignActionKind.EndTurn
            or GeneratedCampaignActionKind.RunEncounterAi)
        {
            RunBoundedEncounterAi();
        }

        if (_diagnostics.Count == 0 && IsQuestCausal(planned.Action.Kind))
        {
            RefreshQuestObjectivesOnce();
        }

        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot Save(string slotName)
    {
        var captured = _truths.Capture();
        if (_session is null)
        {
            _status = captured.Status;
            _diagnostics = ["campaign.session_not_started"];
            return Snapshot(captured.Truth);
        }

        if (captured.Truth is null
            || !GeneratedCampaignSessionTruthService.Same(_session.Truth, captured.Truth))
        {
            _status = GeneratedCampaignSessionStatus.STALE_PROJECT;
            _diagnostics = ["campaign.project_truth_changed"];
            return Snapshot(captured.Truth);
        }

        var result = _saves.Save(_session.Truth.ProjectFolder, slotName, _session.RuntimeSession);
        _diagnostics = result.Passed ? [] : result.Diagnostics.ToList();
        _recentEvents = [result.Passed ? "Игра сохранена." : "Сохранение не создано."];
        if (result.Passed)
        {
            _session = _session with { SlotName = slotName };
            var entry = _saves.List(_session.Truth.ProjectFolder).Entries
                .SingleOrDefault(item => string.Equals(item.SlotName, slotName, StringComparison.Ordinal));
            _saveState = new GeneratedCampaignSaveState
            {
                Slot = slotName,
                Status = "Сохранено",
                RevisionCount = entry?.RevisionCount ?? 1,
                Deduplicated = result.Deduplicated,
                LastResult = result.Deduplicated
                    ? "Состояние уже сохранено; новая ревизия не потребовалась."
                    : "Создана новая ревизия сохранения."
            };
        }

        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot Continue(string slotName)
    {
        var captured = _truths.Capture();
        var package = _currentProject.CurrentPackage;
        if (captured.Status != GeneratedCampaignSessionStatus.READY
            || captured.Truth is null
            || package is null)
        {
            _status = captured.Status;
            _diagnostics = captured.Diagnostics.ToList();
            return Snapshot(captured.Truth);
        }

        var result = _saves.Load(captured.Truth.ProjectFolder, slotName);
        if (!result.Passed || result.Session is null)
        {
            _status = result.Status is GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
                or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED
                ? GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED
                : GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = result.Diagnostics.ToList();
            return Snapshot(captured.Truth);
        }

        _session = new GeneratedCampaignSession(captured.Truth, package, result.Session, slotName);
        _status = GeneratedCampaignSessionStatus.ACTIVE;
        _diagnostics = [];
        _recentEvents = ["Сохранённая игра продолжена."];
        UpdateSaveState(slotName, "Загружено");
        return Snapshot(captured.Truth);
    }

    public GeneratedGameplaySaveListResult ListSaves() =>
        string.IsNullOrWhiteSpace(_currentProject.CurrentFolder)
            ? new GeneratedGameplaySaveListResult
            {
                Passed = false,
                Diagnostics = ["campaign.no_project"]
            }
            : _saves.List(_currentProject.CurrentFolder);

    public GeneratedGameplaySaveMigrationPreview PreviewMigration(string slotName) =>
        string.IsNullOrWhiteSpace(_currentProject.CurrentFolder)
            ? new GeneratedGameplaySaveMigrationPreview
            {
                SlotName = slotName,
                Diagnostics = ["campaign.no_project"]
            }
            : _migration.Preview(_currentProject.CurrentFolder, slotName);

    public GeneratedCampaignSnapshot MigrateAndContinue(GeneratedGameplaySaveMigrationPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        var projectFolder = _currentProject.CurrentFolder ?? string.Empty;
        var result = _migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = projectFolder,
            SlotName = preview.SlotName,
            SourceRevisionSha256 = preview.SourceRevisionSha256,
            CandidateSessionSha256 = preview.CandidateSessionSha256
        });
        if (!result.Passed || result.Session is null)
        {
            _status = GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = result.Diagnostics.ToList();
            return Snapshot(_truths.Capture().Truth);
        }

        var captured = _truths.Capture();
        var package = _currentProject.CurrentPackage;
        if (captured.Status != GeneratedCampaignSessionStatus.READY
            || captured.Truth is null
            || package is null)
        {
            _status = captured.Status;
            _diagnostics = captured.Diagnostics.ToList();
            return Snapshot(captured.Truth);
        }

        _session = new GeneratedCampaignSession(captured.Truth, package, result.Session, preview.SlotName);
        _status = GeneratedCampaignSessionStatus.ACTIVE;
        _diagnostics = [];
        _recentEvents = ["Сохранение перенесено в текущий мир и продолжено."];
        UpdateSaveState(preview.SlotName, "Перенесено и загружено");
        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot ClearSession()
    {
        _session = null;
        _recentEvents = [];
        _saveState = new GeneratedCampaignSaveState();
        return Refresh();
    }

    private void RunBoundedEncounterAi()
    {
        if (_session is null) return;
        var encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
        var limit = Math.Max(1, (encounter?.Participants.Count ?? 1) * 2);
        for (var index = 0; index < limit; index++)
        {
            encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
            if (encounter is not { Active: true } || encounter.Participants.Count == 0) return;
            var turn = Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1);
            if (IdEquals(encounter.Participants[turn].Team, "player")) return;
            var ai = _runtime.ExecuteGameplayCommand(_session.Package, _session.RuntimeSession,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            _session = _session with { RuntimeSession = ai.Session };
            _recentEvents.AddRange(_events.Present(ai));
            if (ai.Success) continue;
            _diagnostics = ["campaign.encounter_ai_failed"];
            return;
        }

        encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
        if (encounter is { Active: true } && encounter.Participants.Count > 0)
        {
            var turn = Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1);
            if (!IdEquals(encounter.Participants[turn].Team, "player"))
                _diagnostics = ["campaign.encounter_ai_bound_reached"];
        }
    }

    private UnifiedRuntimeResult ExecuteRuntimeAction(GeneratedCampaignPlannedAction planned)
    {
        if (_session is null) throw new InvalidOperationException("Campaign session is not active.");
        if (planned.PlayerCommand is not null)
            return _runtime.ExecutePlayerCommand(_session.Package, _session.RuntimeSession, planned.PlayerCommand);

        var command = planned.RuntimeCommand!;
        if (planned.Action.Kind != GeneratedCampaignActionKind.BasicAttack
            || string.IsNullOrWhiteSpace(command.TargetId)
            || !command.Args.TryGetValue("sourceParticipantId", out var sourceParticipantId))
        {
            return _runtime.ExecuteGameplayCommand(_session.Package, _session.RuntimeSession, command);
        }

        var target = _session.RuntimeSession.GameplayState.ActiveEncounter?.Participants
            .FirstOrDefault(item => IdEquals(item.Id, command.TargetId));
        var targetResourceId = target?.Resources.FirstOrDefault()?.ResourceId;
        if (string.IsNullOrWhiteSpace(targetResourceId))
            return _runtime.ExecuteGameplayCommand(_session.Package, _session.RuntimeSession, command);

        var runtimePackage = ClonePackage(_session.Package);
        var targetResource = runtimePackage.Game.Resources
            .FirstOrDefault(item => IdEquals(item.Id, targetResourceId));
        if (targetResource is null)
        {
            runtimePackage.Game.Resources.Add(new ResourceDefinition
            {
                Id = targetResourceId,
                Name = "Здоровье",
                Kind = "health",
                Tags = ["health"]
            });
        }
        else
        {
            targetResource.Kind = "health";
            if (!targetResource.Tags.Any(item => IdEquals(item, "health")))
                targetResource.Tags.Add("health");
        }

        const string abilityId = "campaign/session-compatible-attack";
        runtimePackage.Game.Abilities.RemoveAll(item => IdEquals(item.Id, abilityId));
        runtimePackage.Game.Abilities.Add(new AbilityDefinition
        {
            Id = abilityId,
            Name = "Обычная атака",
            Kind = "attack",
            Targeting = "enemy",
            Power = 3,
            ResourceId = targetResourceId
        });
        return _runtime.ExecuteGameplayCommand(runtimePackage, _session.RuntimeSession,
            GameRuntimeCommand.UseAbility(abilityId, sourceParticipantId, command.TargetId));
    }

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(JsonSerializer.Serialize(package))
        ?? throw new InvalidOperationException("Campaign runtime package could not be cloned.");

    private void OpenProjectedDialogue(GeneratedCampaignPlannedAction planned)
    {
        if (_session is null
            || _session.RuntimeSession.GameplayState.ActiveDialogue is { Open: true }
            || string.IsNullOrWhiteSpace(planned.PlayerCommand?.TargetId))
        {
            return;
        }

        var entity = _session.Package.Game.Maps
            .SelectMany(map => map.Entities)
            .FirstOrDefault(item => IdEquals(item.Id, planned.PlayerCommand.TargetId));
        var interaction = entity is null
            ? null
            : GeneratedCampaignActionPlanner.Components(_session.Package, entity)
                .FirstOrDefault(item => IdEquals(item.Type, "interactable"));
        if (interaction?.Args.TryGetValue("dialogueId", out var dialogueId) != true
            || string.IsNullOrWhiteSpace(dialogueId)
            || !_session.Package.Game.Dialogues.Any(item => IdEquals(item.Id, dialogueId)))
        {
            return;
        }

        var opened = _runtime.ExecuteGameplayCommand(
            _session.Package,
            _session.RuntimeSession,
            GameRuntimeCommand.OpenDialogue(dialogueId!));
        _session = _session with { RuntimeSession = opened.Session };
        _recentEvents.AddRange(_events.Present(opened));
        if (!opened.Success)
        {
            _diagnostics = ["campaign.dialogue_open_failed", .. opened.Diagnostics.Select(item => item.Code)];
        }
    }

    private void RefreshQuestObjectivesOnce()
    {
        if (_session is null) return;
        var refresh = _runtime.ExecuteGameplayCommand(_session.Package, _session.RuntimeSession,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives });
        _session = _session with { RuntimeSession = refresh.Session };
        _recentEvents.AddRange(_events.Present(refresh));
        if (!refresh.Success) _diagnostics = ["campaign.quest_refresh_failed"];
    }

    private void UpdateSaveState(string slotName, string status)
    {
        if (_session is null) return;
        var entry = _saves.List(_session.Truth.ProjectFolder).Entries
            .SingleOrDefault(item => string.Equals(item.SlotName, slotName, StringComparison.Ordinal));
        _saveState = new GeneratedCampaignSaveState
        {
            Slot = slotName,
            Status = status,
            RevisionCount = entry?.RevisionCount ?? 0,
            LastResult = status
        };
    }

    private GeneratedCampaignSnapshot Snapshot(GeneratedCampaignProjectTruth? capturedTruth)
    {
        var truth = _session?.Truth ?? capturedTruth;
        var package = _session?.Package ?? _currentProject.CurrentPackage;
        var runtimeSession = _session?.RuntimeSession;
        var actions = _status == GeneratedCampaignSessionStatus.ACTIVE
                      && package is not null
                      && runtimeSession is not null
            ? _planner.Plan(package, runtimeSession).Select(item => item.Action).ToList()
            : [];
        return _projection.Project(
            _status,
            truth,
            package,
            runtimeSession,
            actions,
            _recentEvents.TakeLast(16).ToList(),
            _session?.SlotName ?? _saveState.Slot,
            _diagnostics,
            _saveState);
    }

    private static bool IsQuestCausal(GeneratedCampaignActionKind kind) => kind is
        GeneratedCampaignActionKind.Interact
        or GeneratedCampaignActionKind.StartEncounter
        or GeneratedCampaignActionKind.BasicAttack
        or GeneratedCampaignActionKind.UseAbility
        or GeneratedCampaignActionKind.EndTurn
        or GeneratedCampaignActionKind.RunEncounterAi
        or GeneratedCampaignActionKind.ResolveEncounter;

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
