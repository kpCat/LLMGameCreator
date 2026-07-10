using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class SelectedRuntimeVariantInteractiveSessionService :
    ISelectedRuntimeVariantInteractiveSessionService
{
    private const string RuntimeRoute = "runtime_session";
    private const string PresentationRoute = "presentation_only";
    private readonly ICanonicalRuntimePlayerCommandLoopService _commandLoop;

    public SelectedRuntimeVariantInteractiveSessionService(
        ICanonicalRuntimePlayerCommandLoopService commandLoop)
    {
        _commandLoop = commandLoop ?? throw new ArgumentNullException(nameof(commandLoop));
    }

    public static SelectedRuntimeVariantInteractiveSessionService CreateDefault() =>
        new(CanonicalRuntimePlayerCommandLoopService.CreateDefault());

    public SelectedRuntimeVariantInteractiveSession StartSession(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.SessionId)
            || string.IsNullOrWhiteSpace(request.CandidateId)
            || string.IsNullOrWhiteSpace(request.VariantKind)
            || string.IsNullOrWhiteSpace(request.PackageSha256))
        {
            throw new InvalidOperationException("Goal144 session identity and package hash are required.");
        }

        var canonical = _commandLoop.BeginSession(package, new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath
        });
        var session = new SelectedRuntimeVariantInteractiveSession
        {
            SessionId = request.SessionId,
            CandidateId = request.CandidateId,
            VariantKind = request.VariantKind,
            PackagePath = request.PackagePath,
            PackageSha256 = request.PackageSha256,
            CanonicalSession = canonical,
            CurrentStateHash = canonical.CurrentStateHash
        };
        Refresh(session, package);
        return session;
    }

    public SelectedRuntimeVariantInteractiveActionResult ExecuteAction(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantInteractiveActionRequest request)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (session is null) throw new ArgumentNullException(nameof(session));
        if (request is null) throw new ArgumentNullException(nameof(request));
        var before = session.CurrentStateHash;
        if (request.SessionId != session.SessionId
            || request.ActionIndex != session.CurrentActionIndex
            || string.IsNullOrWhiteSpace(request.ActionRequestId))
        {
            return Rejected(request, before, "goal144.action_correlation_rejected");
        }

        var descriptor = session.AvailableActions.FirstOrDefault(action =>
            action.ActionId == request.ActionId);
        if (descriptor is null)
        {
            return Rejected(request, before, "goal144.action_unknown");
        }

        if (!descriptor.Available)
        {
            return Rejected(
                request,
                before,
                "goal144.action_unavailable:" + descriptor.UnavailableReason,
                descriptor);
        }

        var runtimeExecuted = false;
        var runtimeMutation = false;
        var eventCount = 0;
        var diagnostics = new List<string>();
        if (descriptor.Route == RuntimeRoute)
        {
            var range = CanonicalRange(descriptor.ActionId);
            var execution = _commandLoop.ExecuteRange(
                package,
                session.CanonicalSession,
                new CanonicalRuntimePlayerCommandLoopExecutionRequest
                {
                    RequestedOperation = descriptor.ActionId,
                    RuntimeCommandStartIndex = range.Start,
                    RuntimeCommandEndIndex = range.End
                });
            diagnostics.AddRange(execution.Diagnostics);
            if (!execution.Success)
            {
                return Rejected(
                    request,
                    before,
                    "goal144.runtime_execution_failed",
                    descriptor,
                    diagnostics);
            }

            runtimeExecuted = execution.RuntimeExecuted;
            runtimeMutation = execution.RuntimeMutation;
            eventCount = execution.EventCount;
            session.RuntimeCommandExecutionCount += execution.ExecutedCommandCount;
            session.RuntimeStarted = session.CanonicalSession.RuntimeStarted;
            if (execution.Snapshots.Count > 0)
            {
                session.LatestSnapshot = execution.Snapshots[^1];
            }
        }
        else
        {
            session.PresentationOnlyActionCount++;
            if (descriptor.ActionId == "show_final_state")
            {
                session.CanonicalSession.CurrentCommandIndex++;
                session.Completed = true;
            }
        }

        UpdateSummaries(session);
        session.CurrentStateHash = session.CanonicalSession.CurrentStateHash;
        var result = new SelectedRuntimeVariantInteractiveActionResult
        {
            ActionRequestId = request.ActionRequestId,
            SessionId = request.SessionId,
            ActionIndex = request.ActionIndex,
            ActionId = descriptor.ActionId,
            Category = descriptor.Category,
            Route = descriptor.Route,
            TargetId = descriptor.TargetId,
            StateHashBefore = before,
            StateHashAfter = session.CurrentStateHash,
            RuntimeExecuted = runtimeExecuted,
            RuntimeMutation = runtimeMutation,
            RuntimeEventCount = eventCount,
            CorrelationPassed = true,
            Status = "EXECUTED",
            Diagnostics = diagnostics
        };
        session.ActionJournal.Add(ToJournal(result));
        session.CurrentActionIndex++;
        Refresh(session, package);
        return result;
    }

    public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
        SelectedRuntimeVariantInteractiveSession session,
        string checkpointId,
        string createdAtUtc)
    {
        if (session is null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(checkpointId) || string.IsNullOrWhiteSpace(createdAtUtc))
        {
            throw new InvalidOperationException("Goal144 checkpoint id and UTC marker are required.");
        }

        return new SelectedRuntimeVariantInteractiveCheckpoint
        {
            CheckpointId = checkpointId,
            SessionId = session.SessionId,
            CandidateId = session.CandidateId,
            VariantKind = session.VariantKind,
            PackageSha256 = session.PackageSha256,
            ActionJournal = session.ActionJournal.Select(Clone).ToList(),
            RuntimeCommandExecutionCount = session.RuntimeCommandExecutionCount,
            ExpectedStateHash = session.CurrentStateHash,
            ExpectedActionIndex = session.CurrentActionIndex,
            MapSummary = session.LatestMapSummary,
            InventorySummary = session.LatestInventorySummary,
            QuestSummary = session.LatestQuestSummary,
            CombatSummary = session.LatestCombatSummary,
            CreatedAtUtc = createdAtUtc
        };
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (checkpoint is null) throw new ArgumentNullException(nameof(checkpoint));
        var diagnostics = new List<string>();
        var packageHashValid = request.PackageSha256 == checkpoint.PackageSha256;
        var candidateValid = request.CandidateId == checkpoint.CandidateId
                             && request.VariantKind == checkpoint.VariantKind
                             && request.SessionId == checkpoint.SessionId;
        if (!packageHashValid || !candidateValid)
        {
            diagnostics.Add(packageHashValid
                ? "goal144.checkpoint_candidate_mismatch"
                : "goal144.checkpoint_package_hash_mismatch");
            return FailedReplay(packageHashValid, candidateValid, checkpoint, diagnostics);
        }

        var fresh = StartSession(package, request);
        var correlation = true;
        var continuity = true;
        foreach (var entry in checkpoint.ActionJournal)
        {
            if (entry.ActionIndex != fresh.CurrentActionIndex
                || entry.SessionId != fresh.SessionId
                || string.IsNullOrWhiteSpace(entry.ActionRequestId))
            {
                correlation = false;
                diagnostics.Add("goal144.checkpoint_journal_index_or_identity_tamper");
                break;
            }

            var replay = ExecuteAction(package, fresh, new SelectedRuntimeVariantInteractiveActionRequest
            {
                ActionRequestId = entry.ActionRequestId,
                SessionId = entry.SessionId,
                ActionIndex = entry.ActionIndex,
                ActionId = entry.ActionId
            });
            correlation &= replay.CorrelationPassed
                           && replay.Status == "EXECUTED"
                           && replay.Category == entry.Category
                           && replay.Route == entry.Route
                           && replay.TargetId == entry.TargetId;
            continuity &= replay.StateHashBefore == entry.StateHashBefore
                          && replay.StateHashAfter == entry.StateHashAfter;
            if (!correlation || !continuity)
            {
                diagnostics.Add("goal144.checkpoint_journal_tamper_or_hash_discontinuity");
                break;
            }
        }

        var expectedHashMatched = fresh.CurrentStateHash == checkpoint.ExpectedStateHash
                                  && fresh.CurrentActionIndex == checkpoint.ExpectedActionIndex
                                  && fresh.RuntimeCommandExecutionCount ==
                                  checkpoint.RuntimeCommandExecutionCount;
        if (!expectedHashMatched)
        {
            diagnostics.Add("goal144.checkpoint_expected_hash_or_count_mismatch");
        }

        return new SelectedRuntimeVariantInteractiveReplayResult
        {
            Passed = packageHashValid && candidateValid && correlation && continuity
                     && expectedHashMatched,
            PackageHashValidated = packageHashValid,
            CandidateValidated = candidateValid,
            JournalCorrelationPassed = correlation,
            StateHashContinuityPassed = continuity,
            ExpectedStateHashMatched = expectedHashMatched,
            ExpectedStateHash = checkpoint.ExpectedStateHash,
            ActualStateHash = fresh.CurrentStateHash,
            Session = fresh,
            Diagnostics = diagnostics
        };
    }

    private static SelectedRuntimeVariantInteractiveReplayResult FailedReplay(
        bool packageHashValid,
        bool candidateValid,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint,
        IReadOnlyList<string> diagnostics) =>
        new()
        {
            PackageHashValidated = packageHashValid,
            CandidateValidated = candidateValid,
            ExpectedStateHash = checkpoint.ExpectedStateHash,
            Diagnostics = diagnostics
        };

    private static SelectedRuntimeVariantInteractiveActionResult Rejected(
        SelectedRuntimeVariantInteractiveActionRequest request,
        string stateHash,
        string diagnostic,
        SelectedRuntimeVariantActionDescriptor? descriptor = null,
        IReadOnlyList<string>? diagnostics = null)
    {
        var items = new List<string> { diagnostic };
        if (diagnostics is not null)
        {
            items.AddRange(diagnostics);
        }

        return new SelectedRuntimeVariantInteractiveActionResult
        {
            ActionRequestId = request.ActionRequestId,
            SessionId = request.SessionId,
            ActionIndex = request.ActionIndex,
            ActionId = request.ActionId,
            Category = descriptor?.Category ?? string.Empty,
            Route = descriptor?.Route ?? string.Empty,
            TargetId = descriptor?.TargetId ?? string.Empty,
            StateHashBefore = stateHash,
            StateHashAfter = stateHash,
            CorrelationPassed = request.SessionId.Length > 0,
            Status = "REJECTED",
            Diagnostics = items
        };
    }

    private static void Refresh(
        SelectedRuntimeVariantInteractiveSession session,
        GamePackageDefinition package)
    {
        session.CurrentStateHash = session.CanonicalSession.CurrentStateHash;
        session.RuntimeStarted = session.CanonicalSession.RuntimeStarted;
        session.Completed = session.CanonicalSession.CurrentCommandIndex >= 13;
        session.AvailableActions = BuildCatalog(package, session);
        UpdateSummaries(session);
    }

    private static IReadOnlyList<SelectedRuntimeVariantActionDescriptor> BuildCatalog(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session)
    {
        var game = package.Game;
        var sign = game.Maps.SelectMany(map => map.Entities)
            .FirstOrDefault(entity => entity.Id.Contains("sign", StringComparison.Ordinal))?.Id
            ?? string.Empty;
        var interaction = game.Interactions.FirstOrDefault()?.Id ?? string.Empty;
        var dialogue = game.Dialogues.FirstOrDefault()?.Id ?? string.Empty;
        var quest = game.Quests.FirstOrDefault()?.Id ?? string.Empty;
        var inventory = game.Inventories.FirstOrDefault(item => item.OwnerKind == "player")?.Id
                        ?? game.Inventories.FirstOrDefault()?.Id
                        ?? string.Empty;
        var recipe = game.Recipes.FirstOrDefault()?.Id ?? string.Empty;
        var node = game.ResourceNodes.FirstOrDefault()?.Id ?? string.Empty;
        var transaction = game.Transactions.FirstOrDefault()?.Id ?? string.Empty;
        var encounter = game.Encounters.FirstOrDefault()?.Id ?? string.Empty;
        var attack = game.Abilities.FirstOrDefault(item =>
            item.Id.Contains("basic_attack", StringComparison.Ordinal))?.Id ?? string.Empty;
        var cursor = session.CanonicalSession.CurrentCommandIndex;
        var actions = new List<SelectedRuntimeVariantActionDescriptor>
        {
            Runtime("start_runtime", "start_runtime", "Initialize", package.Manifest.StartMapId, cursor, 0),
            Runtime("move", "move", "Move", sign, cursor, 2),
            Runtime("interact", "interact", "Interact", interaction, cursor, 3),
            Runtime("open_dialogue", "open_dialogue", "OpenDialogue", dialogue, cursor, 4),
            Runtime("start_or_update_quest", "start_or_update_quest", "StartQuest", quest, cursor, 5),
            Runtime("show_inventory", "show_inventory", "AddItem", inventory, cursor, 6),
            Runtime("craft", "craft", "CraftRecipe", recipe, cursor, 7),
            Runtime("harvest", "harvest", "HarvestResourceNode", node, cursor, 8),
            Runtime("transaction", "transaction", "ExecuteTransaction", transaction, cursor, 9),
            Runtime("begin_encounter", "begin_encounter", "StartEncounter", encounter, cursor, 10),
            Runtime("basic_attack", "basic_attack", "BasicAttack", attack, cursor, 11),
            Presentation("show_final_state", "show_final_state", package.Manifest.PackageId, cursor == 12,
                cursor < 12 ? "complete Runtime actions first" : cursor > 12 ? "session already completed" : string.Empty),
            Presentation("inspect_inventory", "show_inventory", inventory, session.RuntimeStarted && !session.Completed,
                session.RuntimeStarted ? "session completed" : "runtime not started"),
            Presentation("inspect_status", "show_status", package.Manifest.PackageId, session.RuntimeStarted,
                session.RuntimeStarted ? string.Empty : "runtime not started")
        };
        return actions;
    }

    private static SelectedRuntimeVariantActionDescriptor Runtime(
        string actionId,
        string category,
        string commandKind,
        string targetId,
        int cursor,
        int expectedCursor)
    {
        var targetExists = !string.IsNullOrWhiteSpace(targetId);
        var available = targetExists && cursor == expectedCursor;
        return new SelectedRuntimeVariantActionDescriptor
        {
            ActionId = actionId,
            Category = category,
            Route = RuntimeRoute,
            CommandKind = commandKind,
            TargetId = targetId,
            Prerequisites = expectedCursor == 0
                ? new List<string> { "selected package hash validated" }
                : new List<string> { "previous canonical action completed" },
            MayMutateState = true,
            Available = available,
            UnavailableReason = !targetExists
                ? "target is absent from selected package"
                : cursor < expectedCursor
                    ? "previous canonical action required"
                    : "canonical action already completed"
        };
    }

    private static SelectedRuntimeVariantActionDescriptor Presentation(
        string actionId,
        string category,
        string targetId,
        bool available,
        string unavailableReason) =>
        new()
        {
            ActionId = actionId,
            Category = category,
            Route = PresentationRoute,
            CommandKind = "read_state_summary",
            TargetId = targetId,
            Prerequisites = new List<string> { "runtime state exists" },
            MayMutateState = false,
            Available = available,
            UnavailableReason = available ? string.Empty : unavailableReason
        };

    private static (int Start, int End) CanonicalRange(string actionId) => actionId switch
    {
        "start_runtime" => (0, 1),
        "move" => (2, 2),
        "interact" => (3, 3),
        "open_dialogue" => (4, 4),
        "start_or_update_quest" => (5, 5),
        "show_inventory" => (6, 6),
        "craft" => (7, 7),
        "harvest" => (8, 8),
        "transaction" => (9, 9),
        "begin_encounter" => (10, 10),
        "basic_attack" => (11, 11),
        _ => throw new InvalidOperationException("Unknown Goal144 Runtime action: " + actionId)
    };

    private static void UpdateSummaries(SelectedRuntimeVariantInteractiveSession session)
    {
        var snapshot = session.LatestSnapshot;
        session.LatestMapSummary = snapshot.MapSummary;
        session.LatestInventorySummary = snapshot.InventorySummary;
        session.LatestQuestSummary = snapshot.QuestSummary;
        session.LatestCombatSummary = snapshot.CombatSummary;
    }

    private static SelectedRuntimeVariantInteractiveJournalEntry ToJournal(
        SelectedRuntimeVariantInteractiveActionResult result) =>
        new()
        {
            ActionRequestId = result.ActionRequestId,
            SessionId = result.SessionId,
            ActionIndex = result.ActionIndex,
            ActionId = result.ActionId,
            Category = result.Category,
            Route = result.Route,
            TargetId = result.TargetId,
            StateHashBefore = result.StateHashBefore,
            StateHashAfter = result.StateHashAfter,
            RuntimeExecuted = result.RuntimeExecuted,
            RuntimeMutation = result.RuntimeMutation,
            RuntimeEventCount = result.RuntimeEventCount
        };

    private static SelectedRuntimeVariantInteractiveJournalEntry Clone(
        SelectedRuntimeVariantInteractiveJournalEntry entry) =>
        new()
        {
            ActionRequestId = entry.ActionRequestId,
            SessionId = entry.SessionId,
            ActionIndex = entry.ActionIndex,
            ActionId = entry.ActionId,
            Category = entry.Category,
            Route = entry.Route,
            TargetId = entry.TargetId,
            StateHashBefore = entry.StateHashBefore,
            StateHashAfter = entry.StateHashAfter,
            RuntimeExecuted = entry.RuntimeExecuted,
            RuntimeMutation = entry.RuntimeMutation,
            RuntimeEventCount = entry.RuntimeEventCount
        };
}
