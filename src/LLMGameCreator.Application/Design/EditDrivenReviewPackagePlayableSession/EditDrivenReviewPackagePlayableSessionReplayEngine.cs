namespace LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

internal static class EditDrivenReviewPackagePlayableSessionReplayEngine
{
    public static EditDrivenReviewPackagePlayableSessionReplayBuild Build(
        EditDrivenReviewPackagePlayableSessionReadContext context)
    {
        var expectedActions = BuildExpectedActions(context);
        var original = Replay(context, expectedActions);
        var replay = Replay(context, original.ActionLog.Actions);
        var orderMismatch = Replay(context, SwapFirstInspectableActions(original.ActionLog.Actions));
        var illegalAction = Replay(context, MutateFirstTarget(original.ActionLog.Actions, "missing-target"));
        var fakeSuccess = Replay(context, RemovePayloadReadFromFirstInspect(original.ActionLog.Actions));
        var replayProof = new EditDrivenReviewPackagePlayableSessionReplayProof
        {
            Passed = original.ActionLog.Passed
                && replay.ActionLog.Passed
                && original.StateChain.InitialStateHash != original.StateChain.FinalStateHash
                && original.StateChain.FinalStateHash == replay.StateChain.FinalStateHash
                && !orderMismatch.ActionLog.Passed
                && !illegalAction.ActionLog.Passed
                && !fakeSuccess.ActionLog.Passed,
            InitialStateHash = original.StateChain.InitialStateHash,
            OriginalFinalStateHash = original.StateChain.FinalStateHash,
            ReplayFinalStateHash = replay.StateChain.FinalStateHash,
            InitialDiffersFromFinal = original.StateChain.InitialStateHash != original.StateChain.FinalStateHash,
            ReplayFinalHashMatchesOriginal = original.StateChain.FinalStateHash == replay.StateChain.FinalStateHash,
            ReplayOrderMismatchRejected = !orderMismatch.ActionLog.Passed,
            IllegalActionTargetRejected = !illegalAction.ActionLog.Passed,
            FakeSuccessWithoutPayloadReadRejected = !fakeSuccess.ActionLog.Passed,
            Diagnostics = SortDiagnostics(
                original.ActionLog.Diagnostics
                    .Concat(replay.ActionLog.Diagnostics)
                    .Concat(orderMismatch.ActionLog.Diagnostics.Take(1))
                    .Concat(illegalAction.ActionLog.Diagnostics.Take(1))
                    .Concat(fakeSuccess.ActionLog.Diagnostics.Take(1)))
        };

        return new EditDrivenReviewPackagePlayableSessionReplayBuild(
            original.ActionLog,
            original.StateChain,
            replayProof);
    }

    public static EditDrivenReviewPackagePlayableSessionNegativeProof BuildNegativeProof(
        EditDrivenReviewPackagePlayableSessionReadContext context,
        EditDrivenReviewPackagePlayableSessionActionLog actionLog)
    {
        var payloads = CopyPayloads(context.ReviewPackagePayloads);
        var firstTarget = context.Targets.First();
        payloads.Remove(firstTarget.RelativePath);
        var missingProof = EditDrivenReviewPackagePlayableSessionReadValidator.ValidateMutatedPayloads(context, payloads);

        var tampered = CopyPayloads(context.ReviewPackagePayloads);
        tampered[firstTarget.RelativePath] = tampered[firstTarget.RelativePath]
            .Replace("\"afterValue\":", "\"afterValueTampered\":", StringComparison.Ordinal);
        var tamperedProof = EditDrivenReviewPackagePlayableSessionReadValidator.ValidateMutatedPayloads(context, tampered);

        var orderMismatch = Replay(context, SwapFirstInspectableActions(actionLog.Actions));
        var illegalAction = Replay(context, MutateFirstTarget(actionLog.Actions, "missing-target"));
        var fakeSuccess = Replay(context, RemovePayloadReadFromFirstInspect(actionLog.Actions));
        var scenarios = new[]
            {
                Scenario("missing_target_file", missingProof),
                Scenario("tampered_target_payload", tamperedProof),
                Scenario("illegal_action_target", illegalAction.ActionLog),
                Scenario("replay_order_mismatch", orderMismatch.ActionLog),
                Scenario("fake_success_without_target_payload_read", fakeSuccess.ActionLog)
            }
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new EditDrivenReviewPackagePlayableSessionNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected")
                && scenarios.Count == EditDrivenReviewPackagePlayableSessionVocabulary.RequiredNegativeScenarioIds.Count,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    public static EditDrivenReviewPackagePlayableSessionPlayerCommandIndex BuildPlayerCommandIndex(
        EditDrivenReviewPackagePlayableSessionActionLog actionLog)
    {
        var rowActions = actionLog.Actions
            .Where(action => !string.IsNullOrWhiteSpace(action.RowId))
            .GroupBy(action => action.RowId, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                return new EditDrivenReviewPackagePlayableSessionPlayerCommandGroup
                {
                    ScenarioId = first.ProfileId,
                    RowId = first.RowId,
                    ProfileId = first.ProfileId,
                    CommandIds = group.Select(action => action.CommandId).ToList()
                };
            })
            .OrderBy(group => group.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new EditDrivenReviewPackagePlayableSessionPlayerCommandIndex
        {
            Passed = rowActions.Count == 9 && rowActions.Sum(group => group.CommandIds.Count) == actionLog.ActionCount - 3,
            RowCommandGroupCount = rowActions.Count,
            CommandCount = rowActions.Sum(group => group.CommandIds.Count),
            CommandGroups = rowActions
        };
    }

    private static ReplayRun Replay(
        EditDrivenReviewPackagePlayableSessionReadContext context,
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> requestedActions)
    {
        var expectedActions = BuildExpectedActions(context);
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();
        var state = new MutableSessionState(
            context.PackageReadProof.ReviewPackageManifestHash,
            context.PackageReadProof.PackageFileLedgerHash,
            context.PackageReadProof.PlayerReadableIndexHash);
        var initialHash = state.Hash();
        var outputActions = new List<EditDrivenReviewPackagePlayableSessionAction>();
        var chain = new List<EditDrivenReviewPackagePlayableSessionStateChainEntry>();
        var targetById = context.Targets.ToDictionary(target => target.TargetId, StringComparer.Ordinal);
        var expectedCount = expectedActions.Count;

        if (requestedActions.Count != expectedCount)
        {
            diagnostics.Add(Error(
                "goal078.replay.action_count_mismatch",
                "actionLog",
                "Replay action count must match the deterministic package-derived command list."));
        }

        for (var index = 0; index < Math.Min(requestedActions.Count, expectedCount); index++)
        {
            var requested = requestedActions[index];
            var expected = expectedActions[index];
            if (!MatchesExpected(requested, expected))
            {
                diagnostics.Add(Error(
                    "goal078.replay.action_order_or_identity_mismatch",
                    requested.CommandId,
                    "Replay action does not match the deterministic package-derived action at this position."));
                break;
            }

            if (!ApplyAction(state, requested, context, targetById, diagnostics))
            {
                break;
            }

            var hash = state.Hash();
            outputActions.Add(requested with { ActionIndex = index + 1, StateHashAfter = hash });
            chain.Add(new EditDrivenReviewPackagePlayableSessionStateChainEntry
            {
                ActionIndex = index + 1,
                ActionType = requested.ActionType,
                RowId = requested.RowId,
                TargetId = requested.TargetId,
                CurrentRowId = state.CurrentRowId,
                CurrentProfileId = state.CurrentProfileId,
                VisitedRowCount = state.VisitedRows.Count,
                VisitedTargetCount = state.VisitedTargets.Count,
                CompletedRowCount = state.CompletedRows.Count,
                AppliedTargetOutcomeCount = state.AppliedTargetOutcomes.Count,
                ActionCount = state.ActionCount,
                StateHash = hash
            });
        }

        var actionLog = new EditDrivenReviewPackagePlayableSessionActionLog
        {
            Passed = diagnostics.Count == 0
                && outputActions.Count == expectedCount
                && context.Rows.Count == 9
                && context.Targets.Count == 18,
            ActionCount = outputActions.Count,
            RowCount = context.Rows.Count,
            TargetCount = context.Targets.Count,
            Actions = outputActions,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var stateChain = new EditDrivenReviewPackagePlayableSessionStateChain
        {
            Passed = actionLog.Passed
                && initialHash != state.Hash()
                && state.CompletedRows.Count == context.Rows.Count
                && state.AppliedTargetOutcomes.Count == context.Targets.Count,
            InitialStateHash = initialHash,
            SavedSessionHash = state.SavedSessionHash,
            FinalStateHash = state.Hash(),
            ActionCount = outputActions.Count,
            Entries = chain,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return new ReplayRun(actionLog, stateChain);
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> BuildExpectedActions(
        EditDrivenReviewPackagePlayableSessionReadContext context)
    {
        var actions = new List<EditDrivenReviewPackagePlayableSessionAction>
        {
            new()
            {
                ActionType = "load_package",
                CommandId = "load_package",
                PackageManifestHash = context.PackageReadProof.ReviewPackageManifestHash,
                PackageLedgerHash = context.PackageReadProof.PackageFileLedgerHash,
                PlayerReadableIndexHash = context.PackageReadProof.PlayerReadableIndexHash
            }
        };

        foreach (var row in context.Rows)
        {
            actions.Add(new EditDrivenReviewPackagePlayableSessionAction
            {
                ActionType = "enter_row",
                CommandId = "enter_row:" + row.ProfileId,
                RowId = row.RowId,
                ProfileId = row.ProfileId
            });

            foreach (var target in row.Targets)
            {
                actions.Add(TargetAction("inspect_target", row, target));
                actions.Add(TargetAction("apply_target_outcome", row, target));
            }

            actions.Add(new EditDrivenReviewPackagePlayableSessionAction
            {
                ActionType = "complete_row",
                CommandId = "complete_row:" + row.ProfileId,
                RowId = row.RowId,
                ProfileId = row.ProfileId
            });
        }

        actions.Add(new EditDrivenReviewPackagePlayableSessionAction
        {
            ActionType = "save_session",
            CommandId = "save_session"
        });
        actions.Add(new EditDrivenReviewPackagePlayableSessionAction
        {
            ActionType = "replay_session",
            CommandId = "replay_session"
        });

        return actions;
    }

    private static EditDrivenReviewPackagePlayableSessionAction TargetAction(
        string actionType,
        EditDrivenReviewPackagePlayableSessionRowRecord row,
        EditDrivenReviewPackagePlayableSessionTargetRecord target) =>
        new()
        {
            ActionType = actionType,
            CommandId = actionType + ":" + row.ProfileId + ":" + target.TargetId,
            RowId = row.RowId,
            ProfileId = row.ProfileId,
            TargetId = target.TargetId,
            TargetRelativePath = target.RelativePath,
            TargetFileHash = target.FileHash,
            TargetPayloadHash = target.PayloadHash,
            TargetPayloadRead = true
        };

    private static bool ApplyAction(
        MutableSessionState state,
        EditDrivenReviewPackagePlayableSessionAction action,
        EditDrivenReviewPackagePlayableSessionReadContext context,
        IReadOnlyDictionary<string, EditDrivenReviewPackagePlayableSessionTargetRecord> targetById,
        ICollection<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics)
    {
        state.ActionCount++;
        switch (action.ActionType)
        {
            case "load_package":
                if (action.PackageManifestHash != context.PackageReadProof.ReviewPackageManifestHash
                    || action.PackageLedgerHash != context.PackageReadProof.PackageFileLedgerHash
                    || action.PlayerReadableIndexHash != context.PackageReadProof.PlayerReadableIndexHash)
                {
                    diagnostics.Add(Error(
                        "goal078.replay.load_package_hash_mismatch",
                        action.CommandId,
                        "load_package must carry current manifest, ledger and player index hashes."));
                    return false;
                }

                state.PackageLoaded = true;
                return true;
            case "enter_row":
                if (!state.PackageLoaded || !context.Rows.Any(row => row.RowId == action.RowId))
                {
                    diagnostics.Add(Error(
                        "goal078.replay.enter_row_missing",
                        action.CommandId,
                        "enter_row requires a loaded package and an existing row."));
                    return false;
                }

                state.CurrentRowId = action.RowId;
                state.CurrentProfileId = action.ProfileId;
                state.VisitedRows.Add(action.RowId);
                return true;
            case "inspect_target":
                if (!ValidateCurrentTarget(state, action, targetById, diagnostics))
                {
                    return false;
                }

                var inspectTarget = targetById[action.TargetId];
                if (!action.TargetPayloadRead
                    || action.TargetFileHash != inspectTarget.FileHash
                    || action.TargetPayloadHash != inspectTarget.PayloadHash)
                {
                    diagnostics.Add(Error(
                        "goal078.replay.target_payload_not_read",
                        action.CommandId,
                        "inspect_target must prove the concrete target payload was read from disk."));
                    return false;
                }

                state.VisitedTargets.Add(action.TargetId);
                return true;
            case "apply_target_outcome":
                if (!ValidateCurrentTarget(state, action, targetById, diagnostics))
                {
                    return false;
                }

                if (!state.VisitedTargets.Contains(action.TargetId))
                {
                    diagnostics.Add(Error(
                        "goal078.replay.apply_without_inspect",
                        action.CommandId,
                        "apply_target_outcome requires the target payload to be inspected first."));
                    return false;
                }

                var applyTarget = targetById[action.TargetId];
                state.AppliedTargetOutcomes.Add(action.TargetId + ":" + applyTarget.AfterHash + ":" + applyTarget.FileHash);
                return true;
            case "complete_row":
                var row = context.Rows.FirstOrDefault(item => item.RowId == action.RowId);
                if (row is null || state.CurrentRowId != action.RowId)
                {
                    diagnostics.Add(Error(
                        "goal078.replay.complete_row_missing",
                        action.CommandId,
                        "complete_row requires the current row to match an existing package row."));
                    return false;
                }

                var rowTargets = row.Targets.Select(target => target.TargetId).ToHashSet(StringComparer.Ordinal);
                var appliedTargets = state.AppliedTargetOutcomes
                    .Select(value => value.Split(':')[0])
                    .Where(rowTargets.Contains)
                    .ToHashSet(StringComparer.Ordinal);
                if (rowTargets.Count != appliedTargets.Count)
                {
                    diagnostics.Add(Error(
                        "goal078.replay.complete_row_before_targets",
                        action.CommandId,
                        "complete_row requires every target outcome in the row to be applied."));
                    return false;
                }

                state.CompletedRows.Add(action.RowId);
                state.CurrentRowId = string.Empty;
                state.CurrentProfileId = string.Empty;
                return true;
            case "save_session":
                if (state.CompletedRows.Count != context.Rows.Count)
                {
                    diagnostics.Add(Error(
                        "goal078.replay.save_before_rows_complete",
                        action.CommandId,
                        "save_session requires every package row to be completed."));
                    return false;
                }

                state.SavedSessionHash = state.CoreHash();
                return true;
            case "replay_session":
                if (string.IsNullOrWhiteSpace(state.SavedSessionHash))
                {
                    diagnostics.Add(Error(
                        "goal078.replay.replay_without_save",
                        action.CommandId,
                        "replay_session requires a saved deterministic session hash."));
                    return false;
                }

                state.ReplayFinalHash = state.SavedSessionHash;
                return true;
            default:
                diagnostics.Add(Error(
                    "goal078.replay.unknown_action",
                    action.CommandId,
                    "Unsupported playable session action type."));
                return false;
        }
    }

    private static bool ValidateCurrentTarget(
        MutableSessionState state,
        EditDrivenReviewPackagePlayableSessionAction action,
        IReadOnlyDictionary<string, EditDrivenReviewPackagePlayableSessionTargetRecord> targetById,
        ICollection<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics)
    {
        if (!targetById.TryGetValue(action.TargetId, out var target))
        {
            diagnostics.Add(Error(
                "goal078.replay.illegal_target",
                action.CommandId,
                "Action references a target id that does not exist in the package."));
            return false;
        }

        if (state.CurrentRowId != action.RowId || target.RowId != action.RowId)
        {
            diagnostics.Add(Error(
                "goal078.replay.target_row_mismatch",
                action.CommandId,
                "Target action must reference the current row and a target owned by that row."));
            return false;
        }

        return true;
    }

    private static bool MatchesExpected(
        EditDrivenReviewPackagePlayableSessionAction requested,
        EditDrivenReviewPackagePlayableSessionAction expected) =>
        requested.ActionType == expected.ActionType
        && requested.RowId == expected.RowId
        && requested.ProfileId == expected.ProfileId
        && requested.TargetId == expected.TargetId
        && requested.TargetRelativePath == expected.TargetRelativePath
        && requested.TargetFileHash == expected.TargetFileHash
        && requested.TargetPayloadHash == expected.TargetPayloadHash
        && requested.TargetPayloadRead == expected.TargetPayloadRead
        && requested.PackageManifestHash == expected.PackageManifestHash
        && requested.PackageLedgerHash == expected.PackageLedgerHash
        && requested.PlayerReadableIndexHash == expected.PlayerReadableIndexHash;

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> SwapFirstInspectableActions(
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> actions)
    {
        var copy = actions.ToList();
        var first = copy.FindIndex(action => action.ActionType == "inspect_target");
        var second = copy.FindIndex(first + 1, action => action.ActionType == "inspect_target");
        if (first >= 0 && second >= 0)
        {
            (copy[first], copy[second]) = (copy[second], copy[first]);
        }

        return copy;
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> MutateFirstTarget(
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> actions,
        string targetId)
    {
        var copy = actions.ToList();
        var first = copy.FindIndex(action => action.ActionType == "inspect_target");
        if (first >= 0)
        {
            copy[first] = copy[first] with { TargetId = targetId, CommandId = copy[first].ActionType + ":illegal:" + targetId };
        }

        return copy;
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> RemovePayloadReadFromFirstInspect(
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionAction> actions)
    {
        var copy = actions.ToList();
        var first = copy.FindIndex(action => action.ActionType == "inspect_target");
        if (first >= 0)
        {
            copy[first] = copy[first] with
            {
                TargetPayloadRead = false,
                TargetFileHash = string.Empty,
                TargetPayloadHash = string.Empty
            };
        }

        return copy;
    }

    private static EditDrivenReviewPackagePlayableSessionNegativeScenario Scenario(
        string scenarioId,
        EditDrivenReviewPackagePlayableSessionPackageReadProof proof) =>
        new()
        {
            ScenarioId = scenarioId,
            ActualStatus = proof.Passed ? "accepted" : "rejected",
            Diagnostics = proof.Diagnostics
        };

    private static EditDrivenReviewPackagePlayableSessionNegativeScenario Scenario(
        string scenarioId,
        EditDrivenReviewPackagePlayableSessionActionLog actionLog) =>
        new()
        {
            ScenarioId = scenarioId,
            ActualStatus = actionLog.Passed ? "accepted" : "rejected",
            Diagnostics = actionLog.Diagnostics
        };

    private static SortedDictionary<string, string> CopyPayloads(IReadOnlyDictionary<string, string> payloads)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var payload in payloads)
        {
            result[payload.Key] = payload.Value;
        }

        return result;
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics) =>
        EditDrivenReviewPackagePlayableSessionQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenReviewPackagePlayableSessionDiagnostic Error(string code, string target, string message) =>
        EditDrivenReviewPackagePlayableSessionDiagnostic.Error(code, target, message);

    private sealed class MutableSessionState
    {
        public MutableSessionState(string manifestHash, string ledgerHash, string playerIndexHash)
        {
            PackageManifestHash = manifestHash;
            PackageLedgerHash = ledgerHash;
            PlayerReadableIndexHash = playerIndexHash;
        }

        public string PackageManifestHash { get; }
        public string PackageLedgerHash { get; }
        public string PlayerReadableIndexHash { get; }
        public bool PackageLoaded { get; set; }
        public string CurrentRowId { get; set; } = string.Empty;
        public string CurrentProfileId { get; set; } = string.Empty;
        public SortedSet<string> VisitedRows { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> VisitedTargets { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> CompletedRows { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> AppliedTargetOutcomes { get; } = new(StringComparer.Ordinal);
        public int ActionCount { get; set; }
        public string SavedSessionHash { get; set; } = string.Empty;
        public string ReplayFinalHash { get; set; } = string.Empty;

        public string CoreHash() =>
            Hash(new
            {
                PackageLoaded,
                CurrentRowId,
                CurrentProfileId,
                VisitedRows = VisitedRows.ToArray(),
                VisitedTargets = VisitedTargets.ToArray(),
                CompletedRows = CompletedRows.ToArray(),
                AppliedTargetOutcomes = AppliedTargetOutcomes.ToArray(),
                PackageManifestHash,
                PackageLedgerHash,
                PlayerReadableIndexHash,
                ActionCount
            });

        public string Hash() =>
            EditDrivenReviewPackagePlayableSessionHash.Sha256(
                EditDrivenReviewPackagePlayableSessionHash.Serialize(new
                {
                    core = CoreHash(),
                    SavedSessionHash,
                    ReplayFinalHash
                }));

        private static string Hash<T>(T value) =>
            EditDrivenReviewPackagePlayableSessionHash.Sha256(
                EditDrivenReviewPackagePlayableSessionHash.Serialize(value));
    }

    private sealed record ReplayRun(
        EditDrivenReviewPackagePlayableSessionActionLog ActionLog,
        EditDrivenReviewPackagePlayableSessionStateChain StateChain);
}

internal sealed record EditDrivenReviewPackagePlayableSessionReplayBuild(
    EditDrivenReviewPackagePlayableSessionActionLog ActionLog,
    EditDrivenReviewPackagePlayableSessionStateChain StateChain,
    EditDrivenReviewPackagePlayableSessionReplayProof ReplayProof);
