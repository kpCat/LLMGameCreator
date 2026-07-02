namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

internal sealed class EditDrivenGamePackageRuntimePreviewPlaythroughReplayEngine
{
    private readonly EditDrivenGamePackageRuntimePreviewPlaythroughCommandBuilder _commandBuilder = new();

    public EditDrivenGamePackageRuntimePreviewPlaythroughReplayResult Replay(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript script)
    {
        var original = ReplayOnce(context, script.Commands, requireExpectedOrder: true);
        var rerun = ReplayOnce(context, script.Commands, requireExpectedOrder: true);
        var transcript = original.Transcript with
        {
            ReplayFinalStateHash = rerun.Transcript.FinalStateHash,
            ReplayFinalHashMatchesOriginal = original.Transcript.FinalStateHash == rerun.Transcript.FinalStateHash,
            Passed = original.Transcript.Passed
                     && rerun.Transcript.Passed
                     && original.Transcript.FinalStateHash == rerun.Transcript.FinalStateHash
        };
        var coverage = EditDrivenGamePackageRuntimePreviewPlaythroughCommandBuilder.BuildCoverageLedger(
            context,
            transcript);
        var stateHashChain = BuildStateHashChain(context, script, transcript, coverage);

        return new EditDrivenGamePackageRuntimePreviewPlaythroughReplayResult(
            transcript,
            stateHashChain,
            coverage);
    }

    public EditDrivenGamePackageRuntimePreviewPlaythroughTranscript ReplayMutated(
        Goal081SourceContext context,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> commands) =>
        ReplayOnce(context, commands, requireExpectedOrder: true).Transcript;

    public EditDrivenGamePackageRuntimePreviewPlaythroughTranscript ReplayCustomCommandScript(
        Goal081SourceContext context,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> commands) =>
        ReplayOnce(context, commands, requireExpectedOrder: false).Transcript;

    private ReplayRun ReplayOnce(
        Goal081SourceContext context,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> requestedCommands,
        bool requireExpectedOrder)
    {
        var expectedCommands = requireExpectedOrder
            ? _commandBuilder.Build(context).Commands
            : requestedCommands;
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var state = new MutablePlaythroughState(context.PackageReadProof);
        var entries = new List<EditDrivenGamePackageRuntimePreviewPlaythroughTranscriptEntry>();

        if (requestedCommands.Count != expectedCommands.Count)
        {
            diagnostics.Add(Error(
                "goal081.replay.command_count_mismatch",
                "playthrough-command-script",
                "Replay command count must match the deterministic package-derived command list."));
        }

        var commandCount = Math.Min(requestedCommands.Count, expectedCommands.Count);
        for (var index = 0; index < commandCount; index++)
        {
            var requested = requestedCommands[index];
            var expected = expectedCommands[index];
            if (requireExpectedOrder && !MatchesExpected(requested, expected))
            {
                diagnostics.Add(Error(
                    "goal081.replay.command_order_or_identity_mismatch",
                    requested.CommandId,
                    "Replay command does not match the deterministic package-derived command at this position."));
                break;
            }

            if (!ApplyCommand(context, state, requested, diagnostics))
            {
                break;
            }

            entries.Add(new EditDrivenGamePackageRuntimePreviewPlaythroughTranscriptEntry
            {
                CommandIndex = index + 1,
                CommandType = requested.CommandType,
                CommandId = requested.CommandId,
                RowId = requested.RowId,
                TargetId = requested.TargetId,
                VisitedRowCount = state.VisitedRows.Count,
                VisitedTargetCount = state.VisitedTargets.Count,
                CollectedTargetCount = state.CollectedTargets.Count,
                CoveredGoal078ActionCount = state.CoveredGoal078Actions.Count,
                StateHash = state.Hash()
            });
        }

        var expectedActionCount = context.Goal078ActionLog.ActionCount;
        var passed = diagnostics.Count == 0
                     && entries.Count == expectedCommands.Count
                     && state.PackageLoaded
                     && state.PackagePayloadRead
                     && state.MapStarted
                     && state.CompletedRows.Count == context.ProjectedIndex.RowCount
                     && state.CollectedTargets.Count == context.SourceTargets.TargetCount
                     && state.CoveredGoal078Actions.Count == expectedActionCount
                     && state.FinalCoverageAsserted;
        var transcript = new EditDrivenGamePackageRuntimePreviewPlaythroughTranscript
        {
            Passed = passed,
            CommandCount = entries.Count,
            InitialStateHash = state.InitialHash,
            FinalStateHash = state.Hash(),
            Entries = entries,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return new ReplayRun(transcript);
    }

    private static bool ApplyCommand(
        Goal081SourceContext context,
        MutablePlaythroughState state,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        state.CommandCount++;
        state.Cover(command.CoveredGoal078ActionIds);
        switch (command.CommandType)
        {
            case "load_package":
                if (!context.PackageReadProof.ProjectedPackagePayloadRead
                    || command.ProjectedPackageHash != context.PackageReadProof.ProjectedPackageHash)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.package_payload_not_read",
                        command.CommandId,
                        "load_package requires the projected GamePackage payload read from disk."));
                    return false;
                }

                state.PackageLoaded = true;
                state.PackagePayloadRead = true;
                return true;
            case "start_at_map":
                if (!state.PackageLoaded || command.StartMapId != context.PackageReadProof.StartMapId)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.start_map_mismatch",
                        command.CommandId,
                        "start_at_map requires a loaded package and the projected package startMapId."));
                    return false;
                }

                if (context.Package?.Game.Maps.All(map => map.Id != command.StartMapId) != false)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.start_map_missing",
                        command.StartMapId,
                        "startMapId is not present in the projected GamePackage maps."));
                    return false;
                }

                state.MapStarted = true;
                state.CurrentMapId = command.StartMapId;
                return true;
            case "inspect_runtime_preview_map":
                return RequireMapStarted(state, command, diagnostics);
            case "inspect_runtime_preview_region":
                if (!RequireMapStarted(state, command, diagnostics))
                {
                    return false;
                }

                var expectedRegionId = "region/goal080/" + command.FamilyId;
                if (context.Package?.GeneratedContent.Regions.All(region => region.SourceId != expectedRegionId) != false)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.region_missing",
                        command.CommandId,
                        "Runtime-preview region is missing from projected generated content."));
                    return false;
                }

                state.VisitedRegions.Add(command.FamilyId);
                return true;
            case "enter_scenario":
                if (!RequireMapStarted(state, command, diagnostics))
                {
                    return false;
                }

                if (!context.ProjectedIndex.Rows.Any(row => row.RowId == command.RowId))
                {
                    diagnostics.Add(Error(
                        "goal081.replay.row_missing",
                        command.CommandId,
                        "Command references a row outside projected-package-index.json."));
                    return false;
                }

                state.CurrentRowId = command.RowId;
                state.VisitedRows.Add(command.RowId);
                return true;
            case "inspect_linked_npc":
                return ValidateCurrentRow(context, state, command, diagnostics)
                       && ValidateGeneratedNpc(context, command, diagnostics);
            case "inspect_linked_dialogue":
                return ValidateCurrentRow(context, state, command, diagnostics)
                       && ValidateDialogue(context, command, diagnostics);
            case "inspect_linked_quest":
                return ValidateCurrentRow(context, state, command, diagnostics)
                       && ValidateQuest(context, command, diagnostics);
            case "inspect_projected_target":
                if (!ValidateTarget(context, state, command, diagnostics))
                {
                    return false;
                }

                state.VisitedTargets.Add(command.TargetId);
                return true;
            case "collect_projected_target":
                if (!ValidateTarget(context, state, command, diagnostics))
                {
                    return false;
                }

                if (!state.VisitedTargets.Contains(command.TargetId))
                {
                    diagnostics.Add(Error(
                        "goal081.replay.collect_without_inspect",
                        command.CommandId,
                        "collect_projected_target requires the target to be inspected first."));
                    return false;
                }

                state.CollectedTargets.Add(command.TargetId);
                return true;
            case "inspect_linked_mechanic":
                if (!ValidateTarget(context, state, command, diagnostics))
                {
                    return false;
                }

                if (context.Package?.Game.Abilities.All(ability => ability.Id != command.PackageMechanicId) != false)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.mechanic_missing",
                        command.CommandId,
                        "Linked package mechanic is missing from projected GamePackage abilities."));
                    return false;
                }

                state.VisitedMechanics.Add(command.PackageMechanicId);
                return true;
            case "cover_goal078_actions":
                if (!ValidateTarget(context, state, command, diagnostics))
                {
                    return false;
                }

                if (!state.CollectedTargets.Contains(command.TargetId))
                {
                    diagnostics.Add(Error(
                        "goal081.replay.cover_without_collect",
                        command.CommandId,
                        "Goal078 action coverage requires the projected target to be collected first."));
                    return false;
                }

                return ValidateCoveredActionIds(context, command, diagnostics);
            case "complete_scenario":
                if (!ValidateCurrentRow(context, state, command, diagnostics))
                {
                    return false;
                }

                var row = context.ProjectedIndex.Rows.First(item => item.RowId == command.RowId);
                if (row.TargetIds.Any(targetId => !state.CollectedTargets.Contains(targetId)))
                {
                    diagnostics.Add(Error(
                        "goal081.replay.complete_before_targets",
                        command.CommandId,
                        "complete_scenario requires every projected target in the row to be collected."));
                    return false;
                }

                state.CompletedRows.Add(command.RowId);
                state.CurrentRowId = string.Empty;
                return true;
            case "assert_final_coverage":
                if (state.CompletedRows.Count != context.ProjectedIndex.RowCount
                    || state.CollectedTargets.Count != context.SourceTargets.TargetCount
                    || state.CoveredGoal078Actions.Count != context.Goal078ActionLog.ActionCount)
                {
                    diagnostics.Add(Error(
                        "goal081.replay.final_coverage_gap",
                        command.CommandId,
                        "Final coverage command requires all rows, targets and Goal078 actions to be covered."));
                    return false;
                }

                state.FinalCoverageAsserted = true;
                return true;
            default:
                diagnostics.Add(Error(
                    "goal081.replay.unknown_command",
                    command.CommandId,
                    "Unsupported playthrough command type."));
                return false;
        }
    }

    private static bool RequireMapStarted(
        MutablePlaythroughState state,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (state.PackageLoaded && state.MapStarted)
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.map_not_started",
            command.CommandId,
            "Command requires package load and start_at_map first."));
        return false;
    }

    private static bool ValidateCurrentRow(
        Goal081SourceContext context,
        MutablePlaythroughState state,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (state.CurrentRowId == command.RowId
            && context.ProjectedIndex.Rows.Any(row => row.RowId == command.RowId))
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.row_context_mismatch",
            command.CommandId,
            "Command must reference the current projected package row."));
        return false;
    }

    private static bool ValidateTarget(
        Goal081SourceContext context,
        MutablePlaythroughState state,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (!ValidateCurrentRow(context, state, command, diagnostics))
        {
            return false;
        }

        var sourceTarget = context.SourceTargets.Targets.FirstOrDefault(target => target.TargetId == command.TargetId);
        if (sourceTarget is null)
        {
            diagnostics.Add(Error(
                "goal081.replay.target_missing",
                command.CommandId,
                "Command references a target outside source-targets.json."));
            return false;
        }

        if (sourceTarget.RowId != command.RowId
            || sourceTarget.PayloadHash != command.SourceTargetPayloadHash
            || sourceTarget.LogicalPackagePath != command.LogicalPackagePath)
        {
            diagnostics.Add(Error(
                "goal081.replay.target_linkage_mismatch",
                command.CommandId,
                "Command target linkage does not match source-targets.json."));
            return false;
        }

        var itemMatches = context.PackageItemByTargetId.TryGetValue(command.TargetId, out var itemId)
                          && itemId == command.PackageItemId;
        var interactionMatches = context.PackageInteractionByTargetId.TryGetValue(command.TargetId, out var interactionId)
                                 && interactionId == command.PackageInteractionId;
        if (itemMatches && interactionMatches)
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.package_target_projection_missing",
            command.CommandId,
            "Command target is not backed by projected package item and interaction metadata."));
        return false;
    }

    private static bool ValidateGeneratedNpc(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        var npcId = "entity/goal080/" + command.ScenarioId + "/review-node";
        if (context.Package?.GeneratedContent.Npcs.Any(npc => npc.SourceId == npcId) == true)
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.npc_missing",
            command.CommandId,
            "Linked generated NPC is missing from projected package generated content."));
        return false;
    }

    private static bool ValidateDialogue(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (context.Package?.Game.Dialogues.Any(dialogue => dialogue.Id == command.PackageDialogueId) == true)
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.dialogue_missing",
            command.CommandId,
            "Linked dialogue is missing from projected GamePackage."));
        return false;
    }

    private static bool ValidateQuest(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (context.Package?.Game.Quests.Any(quest => quest.Id == command.PackageQuestId) == true)
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.quest_missing",
            command.CommandId,
            "Linked quest is missing from projected GamePackage."));
        return false;
    }

    private static bool ValidateCoveredActionIds(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        var knownActionIds = context.Goal078ActionLog.Actions
            .Select(action => action.CommandId)
            .ToHashSet(StringComparer.Ordinal);
        if (command.CoveredGoal078ActionIds.Count > 0
            && command.CoveredGoal078ActionIds.All(knownActionIds.Contains))
        {
            return true;
        }

        diagnostics.Add(Error(
            "goal081.replay.goal078_action_linkage_missing",
            command.CommandId,
            "Goal078 action coverage command must link to known Goal078 command ids."));
        return false;
    }

    private static bool MatchesExpected(
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand requested,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand expected) =>
        requested.CommandType == expected.CommandType
        && requested.CommandId == expected.CommandId
        && requested.ScenarioId == expected.ScenarioId
        && requested.RowId == expected.RowId
        && requested.FamilyId == expected.FamilyId
        && requested.SeedId == expected.SeedId
        && requested.TargetId == expected.TargetId
        && requested.LogicalPackagePath == expected.LogicalPackagePath
        && requested.PackageItemId == expected.PackageItemId
        && requested.PackageInteractionId == expected.PackageInteractionId
        && requested.PackageQuestId == expected.PackageQuestId
        && requested.PackageDialogueId == expected.PackageDialogueId
        && requested.PackageMechanicId == expected.PackageMechanicId
        && requested.SourceTargetPayloadHash == expected.SourceTargetPayloadHash
        && requested.ProjectedPackageHash == expected.ProjectedPackageHash
        && requested.StartMapId == expected.StartMapId
        && requested.CoveredGoal078ActionIds.SequenceEqual(expected.CoveredGoal078ActionIds);

    public static IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> SwapFirstTargetCommands(
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> commands)
    {
        var copy = commands.ToList();
        var first = copy.FindIndex(command => command.CommandType == "inspect_projected_target");
        var second = copy.FindIndex(first + 1, command => command.CommandType == "inspect_projected_target");
        if (first >= 0 && second >= 0)
        {
            (copy[first], copy[second]) = (copy[second], copy[first]);
        }

        return copy;
    }

    public static IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> MutateFirstTarget(
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> commands,
        string targetId)
    {
        var copy = commands.ToList();
        var first = copy.FindIndex(command => command.CommandType == "inspect_projected_target");
        if (first >= 0)
        {
            copy[first] = copy[first] with
            {
                TargetId = targetId,
                CommandId = "inspect_projected_target:illegal:" + targetId
            };
        }

        return copy;
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain BuildStateHashChain(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript script,
        EditDrivenGamePackageRuntimePreviewPlaythroughTranscript transcript,
        EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger coverage)
    {
        var entries = new[]
        {
            Entry("initial_package_read_state", Hash(new
            {
                context.PackageReadProof.ProjectedPackageHash,
                context.PackageReadProof.ProjectedPackagePayloadRead,
                context.PackageReadProof.ProjectedPackageDeserialized,
                context.PackageReadProof.GamePackageValidationPassed
            })),
            Entry("command_script_state", Hash(script)),
            Entry("replay_transcript_state", Hash(transcript)),
            Entry("final_coverage_state", Hash(coverage))
        };
        var diagnostics = transcript.Diagnostics.Concat(coverage.Diagnostics).ToList();

        return new EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain
        {
            Passed = transcript.Passed
                     && coverage.Passed
                     && transcript.ReplayFinalHashMatchesOriginal
                     && diagnostics.All(diagnostic => diagnostic.Severity != "error"),
            InitialPackageReadStateHash = entries[0].Hash,
            CommandScriptStateHash = entries[1].Hash,
            ReplayTranscriptStateHash = entries[2].Hash,
            FinalCoverageStateHash = entries[3].Hash,
            ReplayRerunFinalStateHash = transcript.ReplayFinalStateHash,
            ReplayRerunFinalHashMatchesFirstRun = transcript.ReplayFinalHashMatchesOriginal,
            Entries = entries,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChainEntry Entry(
        string stageId,
        string hash) =>
        new() { StageId = stageId, Hash = hash };

    private static string Hash<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughHash.HashJson(value);

    private static IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic.Error(code, target, message);

    private sealed class MutablePlaythroughState
    {
        public MutablePlaythroughState(EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof readProof)
        {
            ProjectedPackageHash = readProof.ProjectedPackageHash;
            ProjectedIndexHash = readProof.ProjectedIndexHash;
            PlayerIndexHash = readProof.PlayerReadableBridgeIndexHash;
            InitialHash = Hash();
        }

        public string ProjectedPackageHash { get; }
        public string ProjectedIndexHash { get; }
        public string PlayerIndexHash { get; }
        public string InitialHash { get; }
        public bool PackageLoaded { get; set; }
        public bool PackagePayloadRead { get; set; }
        public bool MapStarted { get; set; }
        public bool FinalCoverageAsserted { get; set; }
        public string CurrentMapId { get; set; } = string.Empty;
        public string CurrentRowId { get; set; } = string.Empty;
        public int CommandCount { get; set; }
        public SortedSet<string> VisitedRegions { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> VisitedRows { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> VisitedTargets { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> CollectedTargets { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> VisitedMechanics { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> CompletedRows { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> CoveredGoal078Actions { get; } = new(StringComparer.Ordinal);

        public void Cover(IEnumerable<string> actionIds)
        {
            foreach (var actionId in actionIds)
            {
                CoveredGoal078Actions.Add(actionId);
            }
        }

        public string Hash() =>
            EditDrivenGamePackageRuntimePreviewPlaythroughHash.HashJson(new
            {
                ProjectedPackageHash,
                ProjectedIndexHash,
                PlayerIndexHash,
                PackageLoaded,
                PackagePayloadRead,
                MapStarted,
                FinalCoverageAsserted,
                CurrentMapId,
                CurrentRowId,
                CommandCount,
                VisitedRegions = VisitedRegions.ToArray(),
                VisitedRows = VisitedRows.ToArray(),
                VisitedTargets = VisitedTargets.ToArray(),
                CollectedTargets = CollectedTargets.ToArray(),
                VisitedMechanics = VisitedMechanics.ToArray(),
                CompletedRows = CompletedRows.ToArray(),
                CoveredGoal078Actions = CoveredGoal078Actions.ToArray()
            });
    }

    private sealed record ReplayRun(EditDrivenGamePackageRuntimePreviewPlaythroughTranscript Transcript);
}

internal sealed record EditDrivenGamePackageRuntimePreviewPlaythroughReplayResult(
    EditDrivenGamePackageRuntimePreviewPlaythroughTranscript Transcript,
    EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain StateHashChain,
    EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger CoverageLedger);
