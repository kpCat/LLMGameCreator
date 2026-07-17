using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
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
    private readonly GeneratedCampaignRuntimeDispatchService _dispatch;
    private readonly GeneratedCampaignQuestReadinessService _questReadiness;
    private readonly GeneratedCampaignConsequenceProjector _consequenceProjector;
    private readonly List<GeneratedCampaignConsequence> _consequenceTimeline = [];
    private GeneratedCampaignSession? _session;
    private GeneratedCampaignSessionStatus _status;
    private List<string> _diagnostics = [];
    private List<string> _recentEvents = [];
    private GeneratedCampaignSaveState _saveState = new();
    private GeneratedCampaignActionOutcome? _lastActionOutcome;

    public GeneratedCampaignSessionService(
        ICurrentGamePackageService currentProject,
        GeneratedCampaignSessionTruthService truths,
        IUnifiedGameRuntimeService runtime,
        GeneratedGameplaySaveService saves,
        GeneratedGameplaySaveMigrationService migration,
        GeneratedCampaignActionPlanner planner,
        GeneratedCampaignProjectionService projection,
        GeneratedCampaignEventPresenter events)
        : this(currentProject, truths, runtime, saves, migration, planner, projection, events,
            new GeneratedCampaignRuntimeDispatchService(runtime),
            new GeneratedCampaignQuestReadinessService(),
            new GeneratedCampaignConsequenceProjector())
    {
    }

    public GeneratedCampaignSessionService(
        ICurrentGamePackageService currentProject,
        GeneratedCampaignSessionTruthService truths,
        IUnifiedGameRuntimeService runtime,
        GeneratedGameplaySaveService saves,
        GeneratedGameplaySaveMigrationService migration,
        GeneratedCampaignActionPlanner planner,
        GeneratedCampaignProjectionService projection,
        GeneratedCampaignEventPresenter events,
        GeneratedCampaignRuntimeDispatchService dispatch,
        GeneratedCampaignQuestReadinessService questReadiness,
        GeneratedCampaignConsequenceProjector consequenceProjector)
    {
        _currentProject = currentProject;
        _truths = truths;
        _runtime = runtime;
        _saves = saves;
        _migration = migration;
        _planner = planner;
        _projection = projection;
        _events = events;
        _dispatch = dispatch;
        _questReadiness = questReadiness;
        _consequenceProjector = consequenceProjector;
    }

    public int RuntimeStartInvocationCount { get; private set; }
    public GeneratedCampaignRuntimeDispatchResult? LastRuntimeDispatch { get; private set; }

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

        var packageBefore = GeneratedCampaignRuntimeDispatchService.PackageSha256(package);
        RuntimeStartInvocationCount++;
        var start = _runtime.Start(package);
        var packageAfter = GeneratedCampaignRuntimeDispatchService.PackageSha256(package);
        _recentEvents = _events.Present(start).ToList();
        if (!start.Success || !string.Equals(packageBefore, packageAfter, StringComparison.Ordinal))
        {
            _session = null;
            _status = GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = !string.Equals(packageBefore, packageAfter, StringComparison.Ordinal)
                ? ["campaign.package_mutated_during_dispatch"]
                : ["campaign.runtime_start_failed"];
            return Snapshot(captured.Truth);
        }

        var runtimeSession = start.Session;
        foreach (var quest in package.Game.Quests.Where(quest => quest.AutoStart))
        {
            if (runtimeSession.GameplayState.Quests.Any(item => IdEquals(item.QuestId, quest.Id))) continue;
            var questStart = _dispatch.DispatchGameplay(package, runtimeSession,
                GameRuntimeCommand.StartQuest(quest.Id));
            runtimeSession = questStart.UnifiedRuntimeResult.Session;
            _recentEvents.AddRange(_events.Present(questStart.UnifiedRuntimeResult));
            if (questStart.Passed) continue;
            _status = GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = questStart.Diagnostics.Contains("campaign.package_mutated_during_dispatch",
                    StringComparer.Ordinal)
                ? ["campaign.package_mutated_during_dispatch"]
                : ["campaign.auto_quest_start_failed", .. questStart.Diagnostics];
            _session = null;
            return Snapshot(captured.Truth);
        }

        _session = new GeneratedCampaignSession(captured.Truth, package, runtimeSession, "campaign");
        _status = GeneratedCampaignSessionStatus.ACTIVE;
        _diagnostics = [];
        _saveState = new GeneratedCampaignSaveState { Slot = "campaign" };
        _lastActionOutcome = null;
        _consequenceTimeline.Clear();
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
            RecordOutcome(_consequenceProjector.ProjectFailure(
                "Неизвестное действие", _session.RuntimeSession, _diagnostics));
            return Snapshot(captured.Truth);
        }
        if (!planned.Action.Enabled)
        {
            _diagnostics = ["campaign.action_disabled"];
            RecordOutcome(_consequenceProjector.ProjectFailure(
                planned.Action.Title, _session.RuntimeSession, _diagnostics));
            return Snapshot(captured.Truth);
        }
        if (planned.Action.Kind == GeneratedCampaignActionKind.CompleteQuest
            && planned.RuntimeCommand is { Id: { Length: > 0 } questId })
        {
            var readiness = _questReadiness.Evaluate(_session.Package, _session.RuntimeSession, questId);
            if (readiness.Generated && (!readiness.MappingExact || !readiness.Ready))
            {
                _diagnostics = ["campaign.quest_not_ready"];
                RecordOutcome(_consequenceProjector.ProjectFailure(
                    planned.Action.Title, _session.RuntimeSession, _diagnostics));
                return Snapshot(captured.Truth);
            }
        }

        var before = CopySession(_session.RuntimeSession);
        var readinessBefore = _questReadiness.EvaluateAll(_session.Package, before);
        var dispatch = _dispatch.Dispatch(_session.Package, _session.RuntimeSession, planned);
        LastRuntimeDispatch = dispatch;
        var results = new List<GeneratedCampaignRuntimeDispatchResult> { dispatch };
        ApplyRuntimeSession(dispatch.UnifiedRuntimeResult);
        if (dispatch.Passed
            && dispatch.UnifiedRuntimeResult.MapEvents.Any(item => item.Type == RuntimeEventType.MapChanged))
            dispatch.UnifiedRuntimeResult.Session.GameplayState.CurrentMapId =
                dispatch.UnifiedRuntimeResult.Session.MapState.CurrentMapId;
        _recentEvents = _events.Present(dispatch.UnifiedRuntimeResult).ToList();
        _diagnostics = DispatchDiagnostics(dispatch, "campaign.runtime_command_failed");

        if (dispatch.Passed && planned.Action.Kind == GeneratedCampaignActionKind.Interact)
        {
            var dialogue = OpenProjectedDialogue(planned);
            if (dialogue is not null)
            {
                results.Add(dialogue);
                _recentEvents.AddRange(_events.Present(dialogue.UnifiedRuntimeResult));
                if (!dialogue.Passed)
                    _diagnostics = DispatchDiagnostics(dialogue, "campaign.dialogue_open_failed");
            }
        }

        if (_diagnostics.Count == 0 && planned.Action.Kind is GeneratedCampaignActionKind.StartEncounter
            or GeneratedCampaignActionKind.BasicAttack
            or GeneratedCampaignActionKind.UseAbility
            or GeneratedCampaignActionKind.EndTurn
            or GeneratedCampaignActionKind.RunEncounterAi)
        {
            foreach (var ai in RunBoundedEncounterAi())
            {
                results.Add(ai);
                _recentEvents.AddRange(_events.Present(ai.UnifiedRuntimeResult));
                if (ai.Passed) continue;
                _diagnostics = DispatchDiagnostics(ai, "campaign.encounter_ai_failed");
                break;
            }
        }

        if (_diagnostics.Count == 0 && IsQuestCausal(planned.Action.Kind)
            && CanRefreshOnlyNonGeneratedQuests())
        {
            var refresh = RefreshQuestObjectivesOnce();
            results.Add(refresh);
            _recentEvents.AddRange(_events.Present(refresh.UnifiedRuntimeResult));
            if (!refresh.Passed)
                _diagnostics = DispatchDiagnostics(refresh, "campaign.quest_refresh_failed");
        }

        if (results.Any(item => item.Diagnostics.Contains(
                "campaign.package_mutated_during_dispatch", StringComparer.Ordinal)))
        {
            _status = GeneratedCampaignSessionStatus.FAILED;
            _diagnostics = ["campaign.package_mutated_during_dispatch"];
        }

        var after = _session.RuntimeSession;
        var readinessAfter = _questReadiness.EvaluateAll(_session.Package, after);
        var mapEvents = results.SelectMany(item => item.UnifiedRuntimeResult.MapEvents).ToList();
        var gameplayEvents = results.SelectMany(item => item.UnifiedRuntimeResult.GameplayEvents).ToList();
        var success = _diagnostics.Count == 0 && results.All(item => item.Passed);
        RecordOutcome(_consequenceProjector.ProjectAction(
            _session.Package, before, after, mapEvents, gameplayEvents, planned.Action,
            readinessBefore, readinessAfter, success, _diagnostics));
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
        RecordOutcome(_consequenceProjector.ProjectSave(_session.RuntimeSession, result));
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
        _consequenceTimeline.Clear();
        _consequenceTimeline.AddRange(_consequenceProjector.RebuildFromPersistedEvents(package, result.Session));
        RecordOutcome(_consequenceProjector.ProjectLoad(result.Session, result));
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
        _consequenceTimeline.Clear();
        RecordOutcome(_consequenceProjector.ProjectMigration(result.Session, result));
        return Snapshot(captured.Truth);
    }

    public GeneratedCampaignSnapshot ClearSession()
    {
        _session = null;
        _recentEvents = [];
        _saveState = new GeneratedCampaignSaveState();
        _lastActionOutcome = null;
        _consequenceTimeline.Clear();
        return Refresh();
    }

    private IReadOnlyList<GeneratedCampaignRuntimeDispatchResult> RunBoundedEncounterAi()
    {
        var results = new List<GeneratedCampaignRuntimeDispatchResult>();
        if (_session is null) return results;
        var encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
        var limit = Math.Max(1, (encounter?.Participants.Count ?? 1) * 2);
        for (var index = 0; index < limit; index++)
        {
            encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
            if (encounter is not { Active: true } || encounter.Participants.Count == 0) return results;
            var turn = Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1);
            if (IdEquals(encounter.Participants[turn].Team, "player")) return results;
            var ai = _dispatch.DispatchGameplay(_session.Package, _session.RuntimeSession,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            results.Add(ai);
            ApplyRuntimeSession(ai.UnifiedRuntimeResult);
            if (ai.Passed) continue;
            return results;
        }

        encounter = _session.RuntimeSession.GameplayState.ActiveEncounter;
        if (encounter is { Active: true } && encounter.Participants.Count > 0)
        {
            var turn = Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1);
            if (!IdEquals(encounter.Participants[turn].Team, "player"))
                _diagnostics = ["campaign.encounter_ai_bound_reached"];
        }
        return results;
    }

    private GeneratedCampaignRuntimeDispatchResult? OpenProjectedDialogue(
        GeneratedCampaignPlannedAction planned)
    {
        if (_session is null
            || _session.RuntimeSession.GameplayState.ActiveDialogue is { Open: true }
            || string.IsNullOrWhiteSpace(planned.PlayerCommand?.TargetId))
            return null;

        var entity = _session.Package.Game.Maps.SelectMany(map => map.Entities)
            .FirstOrDefault(item => IdEquals(item.Id, planned.PlayerCommand.TargetId));
        var interaction = entity is null
            ? null
            : GeneratedCampaignActionPlanner.Components(_session.Package, entity)
                .FirstOrDefault(item => IdEquals(item.Type, "interactable"));
        if (interaction?.Args.TryGetValue("dialogueId", out var dialogueId) != true
            || string.IsNullOrWhiteSpace(dialogueId)
            || _session.Package.Game.Dialogues.Count(item => IdEquals(item.Id, dialogueId)) != 1)
            return null;

        var opened = _dispatch.DispatchGameplay(_session.Package, _session.RuntimeSession,
            GameRuntimeCommand.OpenDialogue(dialogueId));
        ApplyRuntimeSession(opened.UnifiedRuntimeResult);
        return opened;
    }

    private GeneratedCampaignRuntimeDispatchResult RefreshQuestObjectivesOnce()
    {
        if (_session is null) throw new InvalidOperationException("Campaign session is not active.");
        var refresh = _dispatch.DispatchGameplay(_session.Package, _session.RuntimeSession,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives });
        ApplyRuntimeSession(refresh.UnifiedRuntimeResult);
        return refresh;
    }

    private bool CanRefreshOnlyNonGeneratedQuests()
    {
        if (_session is null) return false;
        var active = _session.RuntimeSession.GameplayState.Quests
            .Where(item => !IdEquals(item.State, "completed"))
            .ToList();
        return active.Count > 0 && active.All(item =>
            !_questReadiness.IsGeneratedQuest(_session.Package, item.QuestId));
    }

    private void ApplyRuntimeSession(UnifiedRuntimeResult result)
    {
        if (_session is not null) _session = _session with { RuntimeSession = result.Session };
    }

    private static List<string> DispatchDiagnostics(
        GeneratedCampaignRuntimeDispatchResult result,
        string fallback)
    {
        if (result.Passed) return [];
        return new[] { fallback }
            .Concat(result.Diagnostics)
            .Concat(result.UnifiedRuntimeResult.Diagnostics.Select(item => item.Code))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private void RecordOutcome(GeneratedCampaignActionOutcome outcome)
    {
        _lastActionOutcome = outcome;
        _consequenceTimeline.AddRange(outcome.Consequences);
        var overflow = _consequenceTimeline.Count
                       - GeneratedCampaignConsequenceTimeline.DefaultMaximumEntries;
        if (overflow > 0) _consequenceTimeline.RemoveRange(0, overflow);
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
        IReadOnlyList<GeneratedCampaignQuestReadiness> readiness = package is null || runtimeSession is null
            ? []
            : _questReadiness.EvaluateAll(package, runtimeSession);
        return _projection.Project(
            _status,
            truth,
            package,
            runtimeSession,
            actions,
            _recentEvents.TakeLast(16).ToList(),
            _session?.SlotName ?? _saveState.Slot,
            _diagnostics,
            _saveState,
            readiness,
            _lastActionOutcome,
            _consequenceTimeline.ToList());
    }

    private static UnifiedRuntimeSession CopySession(UnifiedRuntimeSession session) =>
        JsonSerializer.Deserialize<UnifiedRuntimeSession>(JsonSerializer.Serialize(session))
        ?? throw new InvalidOperationException("Campaign session snapshot could not be copied.");

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
