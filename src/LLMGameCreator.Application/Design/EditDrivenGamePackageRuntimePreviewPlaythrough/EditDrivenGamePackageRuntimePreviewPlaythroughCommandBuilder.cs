using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

internal sealed class EditDrivenGamePackageRuntimePreviewPlaythroughCommandBuilder
{
    public EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript Build(Goal081SourceContext context)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var commands = new List<EditDrivenGamePackageRuntimePreviewPlaythroughCommand>();
        var package = context.Package;
        var startMapId = package?.Manifest.StartMapId ?? string.Empty;
        var targetById = context.SourceTargets.Targets
            .ToDictionary(target => target.TargetId, StringComparer.Ordinal);
        var scenarioByRow = context.PlayerIndex.Scenarios
            .ToDictionary(scenario => scenario.RowId, StringComparer.Ordinal);
        var actionIdsByTarget = context.Goal078ActionLog.Actions
            .Where(action => !string.IsNullOrWhiteSpace(action.TargetId))
            .GroupBy(action => action.TargetId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(action => action.CommandId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);
        var actionIdsByRowType = context.Goal078ActionLog.Actions
            .Where(action => !string.IsNullOrWhiteSpace(action.RowId)
                             && string.IsNullOrWhiteSpace(action.TargetId))
            .GroupBy(action => action.RowId + "|" + action.ActionType, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(action => action.CommandId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);
        var globalActionIdsByType = context.Goal078ActionLog.Actions
            .Where(action => string.IsNullOrWhiteSpace(action.RowId)
                             && string.IsNullOrWhiteSpace(action.TargetId))
            .GroupBy(action => action.ActionType, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(action => action.CommandId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        Add(commands, new EditDrivenGamePackageRuntimePreviewPlaythroughCommand
        {
            CommandType = "load_package",
            CommandId = "load_package:" + context.PackageReadProof.ProjectedPackageHash,
            ProjectedPackageHash = context.PackageReadProof.ProjectedPackageHash,
            CoveredGoal078ActionIds = ActionIds(globalActionIdsByType, "load_package")
        });
        Add(commands, new EditDrivenGamePackageRuntimePreviewPlaythroughCommand
        {
            CommandType = "start_at_map",
            CommandId = "start_at_map:" + startMapId,
            StartMapId = startMapId,
            ProjectedPackageHash = context.PackageReadProof.ProjectedPackageHash
        });
        Add(commands, new EditDrivenGamePackageRuntimePreviewPlaythroughCommand
        {
            CommandType = "inspect_runtime_preview_map",
            CommandId = "inspect_runtime_preview_map:" + startMapId,
            StartMapId = startMapId
        });

        foreach (var familyId in context.ProjectedIndex.Rows
                     .Select(row => row.FamilyId)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FamilyOrderingKey, StringComparer.Ordinal))
        {
            Add(commands, new EditDrivenGamePackageRuntimePreviewPlaythroughCommand
            {
                CommandType = "inspect_runtime_preview_region",
                CommandId = "inspect_runtime_preview_region:" + familyId,
                FamilyId = familyId
            });
        }

        foreach (var row in OrderedRows(context.ProjectedIndex.Rows))
        {
            if (!scenarioByRow.TryGetValue(row.RowId, out var scenario))
            {
                diagnostics.Add(Error(
                    "goal081.command.missing_player_scenario",
                    row.RowId,
                    "Projected package row is missing from the player-readable bridge index."));
                continue;
            }

            Add(commands, RowCommand("enter_scenario", row, scenario, ActionIds(actionIdsByRowType, row.RowId + "|enter_row")));
            Add(commands, RowCommand("inspect_linked_npc", row, scenario));
            Add(commands, RowCommand("inspect_linked_dialogue", row, scenario));
            Add(commands, RowCommand("inspect_linked_quest", row, scenario));

            foreach (var targetId in row.TargetIds.Order(StringComparer.Ordinal))
            {
                var playerTarget = scenario.ProjectedTargets.FirstOrDefault(target =>
                    string.Equals(target.TargetId, targetId, StringComparison.Ordinal));
                if (playerTarget is null)
                {
                    diagnostics.Add(Error(
                        "goal081.command.missing_player_target",
                        targetId,
                        "Projected target is missing from the player-readable bridge index."));
                    continue;
                }

                if (!targetById.TryGetValue(targetId, out var sourceTarget))
                {
                    diagnostics.Add(Error(
                        "goal081.command.missing_source_target",
                        targetId,
                        "Projected target is missing from source-targets.json."));
                    continue;
                }

                var actionIds = actionIdsByTarget.TryGetValue(targetId, out var ids) ? ids : [];
                Add(commands, TargetCommand("inspect_projected_target", row, playerTarget, sourceTarget, actionIds));
                Add(commands, TargetCommand("collect_projected_target", row, playerTarget, sourceTarget, actionIds));
                Add(commands, TargetCommand("inspect_linked_mechanic", row, playerTarget, sourceTarget, actionIds));
                Add(commands, TargetCommand("cover_goal078_actions", row, playerTarget, sourceTarget, actionIds));
            }

            Add(commands, RowCommand(
                "complete_scenario",
                row,
                scenario,
                ActionIds(actionIdsByRowType, row.RowId + "|complete_row")));
        }

        Add(commands, new EditDrivenGamePackageRuntimePreviewPlaythroughCommand
        {
            CommandType = "assert_final_coverage",
            CommandId = "assert_final_coverage:rows-9:targets-18:actions-57",
            ProjectedPackageHash = context.PackageReadProof.ProjectedPackageHash,
            CoveredGoal078ActionIds = ActionIds(globalActionIdsByType, "save_session")
                .Concat(ActionIds(globalActionIdsByType, "replay_session"))
                .Order(StringComparer.Ordinal)
                .ToList()
        });

        var coveredTargetIds = commands
            .Where(command => command.CommandType == "collect_projected_target")
            .Select(command => command.TargetId)
            .ToHashSet(StringComparer.Ordinal);
        var coveredActionIds = commands
            .SelectMany(command => command.CoveredGoal078ActionIds)
            .ToHashSet(StringComparer.Ordinal);
        var expectedActionIds = context.Goal078ActionLog.Actions
            .Select(action => action.CommandId)
            .ToHashSet(StringComparer.Ordinal);

        if (coveredTargetIds.Count != context.SourceTargets.TargetCount)
        {
            diagnostics.Add(Error(
                "goal081.command.target_coverage_gap",
                "playthrough-command-script",
                "Command script does not collect every projected Goal077 target."));
        }

        if (!expectedActionIds.SetEquals(coveredActionIds))
        {
            diagnostics.Add(Error(
                "goal081.command.action_coverage_gap",
                "playthrough-command-script",
                "Command script does not cover every Goal078 action id through projected target linkage."));
        }

        return new EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript
        {
            Passed = diagnostics.Count == 0
                     && context.PackageReadProof.Passed
                     && context.ProjectedIndex.RowCount == 9
                     && context.SourceTargets.TargetCount == 18
                     && context.Goal078ActionLog.ActionCount == 57
                     && commands.Count > 0,
            ProjectedPackageHash = context.PackageReadProof.ProjectedPackageHash,
            StartMapId = startMapId,
            RowCount = context.ProjectedIndex.RowCount,
            TargetCount = context.SourceTargets.TargetCount,
            Goal078ActionCount = context.Goal078ActionLog.ActionCount,
            CommandCount = commands.Count,
            Commands = commands.Select((command, index) => command with { CommandIndex = index + 1 }).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger BuildCoverageLedger(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughTranscript transcript)
    {
        var coveredRows = transcript.Entries
            .Where(entry => entry.CommandType == "complete_scenario")
            .Select(entry => entry.RowId)
            .ToHashSet(StringComparer.Ordinal);
        var coveredTargets = transcript.Entries
            .Where(entry => entry.CommandType == "collect_projected_target")
            .Select(entry => entry.TargetId)
            .ToHashSet(StringComparer.Ordinal);
        var coveredActionCount = transcript.Entries.Count == 0
            ? 0
            : transcript.Entries.Max(entry => entry.CoveredGoal078ActionCount);
        var rows = OrderedRows(context.ProjectedIndex.Rows)
            .Select(row =>
            {
                var rowTargets = row.TargetIds.Order(StringComparer.Ordinal).ToList();
                var rowActionCount = context.Goal078ActionLog.Actions
                    .Count(action => rowTargets.Contains(action.TargetId, StringComparer.Ordinal)
                                     || string.Equals(action.RowId, row.RowId, StringComparison.Ordinal)
                                     || string.IsNullOrWhiteSpace(action.TargetId)
                                        && (action.CommandId == "load_package"
                                            || action.CommandId == "save_session"
                                            || action.CommandId == "replay_session"));
                return new EditDrivenGamePackageRuntimePreviewPlaythroughCoverageRow
                {
                    RowId = row.RowId,
                    ScenarioId = row.ProfileId,
                    TargetCount = rowTargets.Count,
                    CoveredTargetCount = rowTargets.Count(coveredTargets.Contains),
                    CoveredGoal078ActionCount = rowActionCount,
                    TargetIds = rowTargets
                };
            })
            .ToList();
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        if (coveredRows.Count != context.ProjectedIndex.RowCount)
        {
            diagnostics.Add(Error(
                "goal081.coverage.row_gap",
                "playthrough-transcript",
                "Playthrough did not complete every projected package row."));
        }

        if (coveredTargets.Count != context.SourceTargets.TargetCount)
        {
            diagnostics.Add(Error(
                "goal081.coverage.target_gap",
                "playthrough-transcript",
                "Playthrough did not collect every projected target."));
        }

        if (coveredActionCount != context.Goal078ActionLog.ActionCount)
        {
            diagnostics.Add(Error(
                "goal081.coverage.action_gap",
                "playthrough-transcript",
                "Playthrough did not cover every Goal078 action."));
        }

        return new EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger
        {
            Passed = diagnostics.Count == 0
                     && transcript.Passed
                     && context.PackageReadProof.ProjectedPackagePayloadRead,
            RowCount = context.ProjectedIndex.RowCount,
            TargetCount = context.SourceTargets.TargetCount,
            Goal078ActionCount = context.Goal078ActionLog.ActionCount,
            CoveredRowCount = coveredRows.Count,
            CoveredTargetCount = coveredTargets.Count,
            CoveredGoal078ActionCount = coveredActionCount,
            AllGoal077TargetsCovered = coveredTargets.Count == context.SourceTargets.TargetCount,
            AllGoal078ActionsCovered = coveredActionCount == context.Goal078ActionLog.ActionCount,
            PackageReadRequired = context.PackageReadProof.ProjectedPackagePayloadRead,
            Rows = rows,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughCommand RowCommand(
        string commandType,
        Goal081ProjectedPackageIndexRow row,
        Goal081PlayerReadableBridgeScenario scenario,
        IReadOnlyList<string>? coveredGoal078ActionIds = null) =>
        new()
        {
            CommandType = commandType,
            CommandId = commandType + ":" + row.ProfileId,
            ScenarioId = scenario.ScenarioId,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            PackageQuestId = scenario.PlayerFacingQuest,
            PackageDialogueId = scenario.PlayerFacingDialogue,
            CoveredGoal078ActionIds = coveredGoal078ActionIds ?? []
        };

    private static EditDrivenGamePackageRuntimePreviewPlaythroughCommand TargetCommand(
        string commandType,
        Goal081ProjectedPackageIndexRow row,
        Goal081PlayerReadableBridgeTarget playerTarget,
        Goal081SourceTargetRecord sourceTarget,
        IReadOnlyList<string> goal078ActionIds) =>
        new()
        {
            CommandType = commandType,
            CommandId = commandType + ":" + row.ProfileId + ":" + playerTarget.TargetId,
            ScenarioId = row.ProfileId,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            TargetId = playerTarget.TargetId,
            LogicalPackagePath = playerTarget.LogicalPackagePath,
            PackageItemId = playerTarget.ProjectedItem,
            PackageInteractionId = playerTarget.ProjectedInteraction,
            PackageMechanicId = "ability/goal080/" + playerTarget.TargetId,
            SourceTargetPayloadHash = sourceTarget.PayloadHash,
            CoveredGoal078ActionIds = goal078ActionIds
        };

    private static void Add(
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughCommand> commands,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommand command) =>
        commands.Add(command);

    private static IReadOnlyList<string> ActionIds(
        IReadOnlyDictionary<string, List<string>> actionIdsByKey,
        string key) =>
        actionIdsByKey.TryGetValue(key, out var values) ? values : [];

    private static IReadOnlyList<Goal081ProjectedPackageIndexRow> OrderedRows(
        IEnumerable<Goal081ProjectedPackageIndexRow> rows) =>
        rows.OrderBy(row => EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .ThenBy(row => row.RowId, StringComparer.Ordinal)
            .ToList();

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
}
