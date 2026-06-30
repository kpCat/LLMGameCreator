namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class FamilySimulatableLoopRunner
{
    public FamilySimulatableLoopProof Run(FamilyLifecyclePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var first = Execute(plan);
        var second = Execute(plan);
        return first with
        {
            ReplayedDeterminismHash = second.ReplayDeterminismHash,
            Diagnostics = first.ReplayDeterminismHash == second.ReplayDeterminismHash
                ? first.Diagnostics
                : first.Diagnostics
                    .Concat(
                    [
                        MultiFamilyGeneratedTemplateDiagnostic.Error(
                            "goal043.loop.replay_nondeterministic",
                            plan.FamilyId,
                            "Same plan replay produced a different loop proof hash.")
                    ])
                    .ToList()
        };
    }

    private static FamilySimulatableLoopProof Execute(FamilyLifecyclePlan plan)
    {
        var initialState = BuildInitialState(plan);
        var state = new SortedDictionary<string, string>(
            initialState.Values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            StringComparer.Ordinal);
        var events = new List<FamilyLoopEvent>();
        var changedMarkers = new SortedSet<string>(StringComparer.Ordinal);
        BlockedInvalidAction? blocked = null;

        foreach (var command in plan.LoopCommands.OrderBy(item => item.Order))
        {
            var before = new SortedDictionary<string, string>(state, StringComparer.Ordinal);
            var blockReason = TryApply(command, state);
            if (!string.IsNullOrWhiteSpace(blockReason))
            {
                blocked ??= new BlockedInvalidAction
                {
                    Blocked = true,
                    CommandId = command.CommandId,
                    ReasonCode = blockReason,
                    Reason = "Command was rejected by the Application-owned family loop runner."
                };
                events.Add(new FamilyLoopEvent
                {
                    Order = command.Order,
                    EventId = "evt/" + command.CommandId.Replace("cmd/", "", StringComparison.Ordinal) + "/blocked",
                    EventType = "blocked_invalid_action",
                    TargetId = command.TargetId,
                    StateKey = "blockedInvalidAction",
                    BeforeValue = "",
                    AfterValue = blockReason
                });
                continue;
            }

            var changedKeys = state
                .Where(pair => !before.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .Order(StringComparer.Ordinal)
                .ToList();
            foreach (var key in changedKeys)
            {
                events.Add(new FamilyLoopEvent
                {
                    Order = command.Order,
                    EventId = "evt/" + command.CommandId.Replace("cmd/", "", StringComparison.Ordinal) + "/" + key.Replace('/', '_'),
                    EventType = command.CommandType + "_applied",
                    TargetId = command.TargetId,
                    StateKey = key,
                    BeforeValue = before.TryGetValue(key, out var beforeValue) ? beforeValue : string.Empty,
                    AfterValue = state[key]
                });
            }

            if (changedKeys.Count > 0 && !string.IsNullOrWhiteSpace(command.FamilyMarker))
            {
                changedMarkers.Add(command.FamilyMarker);
            }
        }

        var afterState = new FamilyLoopState { Values = state };
        var stateChanged = initialState.Values.Any(pair => !state.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
            || state.Count != initialState.Values.Count;
        var requiredMarkers = MultiFamilyGeneratedTemplateCatalog.RequiredFamilyMarkers(plan.FamilyId);
        var minimumsPassed = requiredMarkers.All(changedMarkers.Contains);
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (!stateChanged)
        {
            diagnostics.Add(MultiFamilyGeneratedTemplateDiagnostic.Error("goal043.loop.state_transition_missing", plan.FamilyId, "Loop proof must change state."));
        }

        if (!minimumsPassed)
        {
            diagnostics.Add(MultiFamilyGeneratedTemplateDiagnostic.Error("goal043.loop.family_minimum_missing", plan.FamilyId, "Loop proof does not cover the family-specific required markers."));
        }

        if (blocked == null)
        {
            diagnostics.Add(MultiFamilyGeneratedTemplateDiagnostic.Error("goal043.loop.invalid_action_not_blocked", plan.FamilyId, "Loop proof must block at least one invalid action for this family."));
        }

        var proofWithoutHash = new FamilySimulatableLoopProof
        {
            FamilyId = plan.FamilyId,
            ScenarioId = plan.ScenarioId,
            InitialState = initialState,
            OrderedCommands = plan.LoopCommands.OrderBy(item => item.Order).ToList(),
            Events = events.OrderBy(item => item.Order).ThenBy(item => item.EventId, StringComparer.Ordinal).ToList(),
            AfterState = afterState,
            ChangedMarkers = changedMarkers.ToList(),
            BlockedInvalidAction = blocked ?? new BlockedInvalidAction(),
            StateChanged = stateChanged,
            FamilySpecificMinimumsPassed = minimumsPassed,
            Diagnostics = diagnostics
        };
        var hashInput = new
        {
            proofWithoutHash.FamilyId,
            proofWithoutHash.ScenarioId,
            proofWithoutHash.InitialState,
            proofWithoutHash.OrderedCommands,
            proofWithoutHash.Events,
            proofWithoutHash.AfterState,
            proofWithoutHash.ChangedMarkers,
            proofWithoutHash.BlockedInvalidAction,
            proofWithoutHash.StateChanged,
            proofWithoutHash.FamilySpecificMinimumsPassed
        };
        var hash = MultiFamilyGeneratedTemplateHash.Hash(MultiFamilyGeneratedTemplateHash.Serialize(hashInput));

        return proofWithoutHash with
        {
            ReplayDeterminismHash = hash,
            ReplayedDeterminismHash = hash
        };
    }

    private static FamilyLoopState BuildInitialState(FamilyLifecyclePlan plan)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["familyId"] = plan.FamilyId,
            ["scenarioId"] = plan.ScenarioId,
            ["currentRegionId"] = Value(plan, "startRegionId"),
            ["currentChunkId"] = Value(plan, "firstChunkId"),
            ["focusTargetId"] = "",
            ["inventoryItemId"] = "",
            ["questProgress"] = "0",
            ["observedHazardId"] = "",
            ["resourceCount"] = "0",
            ["craftedItemId"] = "",
            ["survivalState"] = "unstable",
            ["partyFacing"] = "north",
            ["partyPosition"] = Value(plan, "firstChunkId"),
            ["lockedRouteState"] = "locked",
            ["dungeonKeyState"] = "missing",
            ["encounterPressure"] = "inactive"
        };
        return new FamilyLoopState { Values = values };
    }

    private static string TryApply(
        FamilyLoopCommand command,
        SortedDictionary<string, string> state)
    {
        switch (command.CommandType)
        {
            case "move_to_region":
                state["currentRegionId"] = command.TargetId;
                state["currentChunkId"] = command.SecondaryTargetId;
                state["movementMarker"] = command.FamilyMarker;
                return string.Empty;

            case "claim_quest_reward":
                return state.TryGetValue("questProgress", out var progress) && progress == "1"
                    ? string.Empty
                    : "goal043.loop.quest_reward_without_progress";

            case "focus_target":
                state["focusTargetId"] = command.TargetId;
                return string.Empty;

            case "obtain_item":
                state["inventoryItemId"] = command.TargetId;
                return string.Empty;

            case "progress_quest":
                if (!state.TryGetValue("focusTargetId", out var focus) || string.IsNullOrWhiteSpace(focus))
                {
                    return "goal043.loop.quest_progress_without_focus";
                }

                state["questProgress"] = "1";
                state["questEventId"] = command.TargetId;
                return string.Empty;

            case "observe_hazard":
                state["observedHazardId"] = command.TargetId;
                state["currentChunkId"] = command.SecondaryTargetId;
                return string.Empty;

            case "collect_resource":
                state["resourceCount"] = AddInt(state["resourceCount"], command.Value);
                state["resourceSourceChunkId"] = command.SecondaryTargetId;
                return string.Empty;

            case "consume_resource":
                var consumeAmount = ParseInt(command.Value);
                var available = ParseInt(state["resourceCount"]);
                if (available < consumeAmount)
                {
                    return "goal043.loop.resource_underflow_blocked";
                }

                state["resourceCount"] = (available - consumeAmount).ToString("0");
                state["survivalState"] = "stabilized";
                return string.Empty;

            case "craft_item":
                var craftCost = ParseInt(command.Value);
                var resources = ParseInt(state["resourceCount"]);
                if (resources < craftCost)
                {
                    return "goal043.loop.craft_without_resource";
                }

                state["resourceCount"] = (resources - craftCost).ToString("0");
                state["craftedItemId"] = command.TargetId;
                state["survivalState"] = "crafted";
                return string.Empty;

            case "orient_party":
                state["partyFacing"] = command.Value;
                return string.Empty;

            case "move_corridor":
                state["currentRegionId"] = command.TargetId;
                state["currentChunkId"] = command.SecondaryTargetId;
                state["partyPosition"] = command.SecondaryTargetId;
                return string.Empty;

            case "encounter_pressure":
                state["encounterPressure"] = command.TargetId;
                return string.Empty;

            case "acquire_key":
                state["dungeonKeyState"] = command.TargetId;
                return string.Empty;

            case "enter_locked_route":
                if (!state.TryGetValue("dungeonKeyState", out var key) || !key.StartsWith("key/", StringComparison.Ordinal))
                {
                    return "goal043.loop.locked_route_without_key";
                }

                state["lockedRouteState"] = "unlocked:" + command.TargetId;
                return string.Empty;

            default:
                return "goal043.loop.unknown_command";
        }
    }

    private static string AddInt(string left, string right) =>
        (ParseInt(left) + ParseInt(right)).ToString("0");

    private static int ParseInt(string value) =>
        int.TryParse(value, out var parsed) ? parsed : 0;

    private static string Value(FamilyLifecyclePlan plan, string key) =>
        plan.FamilyExtension.Values.TryGetValue(key, out var value) ? value : string.Empty;
}
