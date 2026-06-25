using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedMicrogameGoalPreviewService
{
    public GeneratedMicrogameGoalPreviewModel BuildFromRuntimeAttempt(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        VisibleGeneratedPlayableRuntimeAttempt runtimeAttempt)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(runtimeAttempt);

        var tracker = new GeneratedQuestDialoguePreviewService();
        tracker.StartSession(package);
        var model = EnsureActiveGoal(package, preview, tracker);
        return runtimeAttempt.CommandAttempts.Any(IsSuccessfulInteraction)
            ? AdvanceAfterInteraction(package, preview, tracker, runtimeAttempt.CommandAttempts.First(IsSuccessfulInteraction))
            : model;
    }

    public GeneratedMicrogameGoalPreviewModel EnsureActiveGoal(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedQuestDialoguePreviewService tracker)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(tracker);

        var quest = SelectActiveQuest(preview);
        if (quest == null)
        {
            return Empty("generated_microgame_goal.no_generated_quest", "Generated package has no quest to mark as the active goal.");
        }

        var questId = QuestId(quest);
        var started = tracker.StartQuest(questId);
        var runtimeProgress = CreateRuntimeProgress(package, preview, quest, progressAction: string.Empty, advance: false);
        return BuildModel(package, preview, tracker, quest, started, runtimeProgress, progressAction: string.Empty, progressAdvanced: false);
    }

    public GeneratedMicrogameGoalPreviewModel AdvanceAfterInteraction(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedQuestDialoguePreviewService tracker,
        VisibleGeneratedPlayableRuntimeCommandAttempt interactionAttempt)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(tracker);
        ArgumentNullException.ThrowIfNull(interactionAttempt);

        var quest = SelectActiveQuest(preview);
        if (quest == null)
        {
            return Empty("generated_microgame_goal.no_generated_quest", "Generated package has no quest to advance after interaction.");
        }

        tracker.StartQuest(QuestId(quest));
        var advanced = tracker.MarkNextStep(QuestId(quest));
        var action = BuildProgressAction(interactionAttempt);
        var runtimeProgress = CreateRuntimeProgress(package, preview, quest, action, advance: interactionAttempt.Succeeded);
        return BuildModel(package, preview, tracker, quest, advanced, runtimeProgress, action, progressAdvanced: runtimeProgress.ProgressAdvanced);
    }

    public GeneratedMicrogameGoalPreviewModel AdvanceAfterInteraction(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedQuestDialoguePreviewService tracker,
        CommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var attempt = new VisibleGeneratedPlayableRuntimeCommandAttempt
        {
            CommandId = "ui_interact",
            CommandType = "interact",
            Succeeded = result.Success,
            CurrentMapId = result.State.CurrentMapId,
            PlayerPosition = new VisibleGeneratedPlayablePosition
            {
                X = result.State.PlayerPosition.X,
                Y = result.State.PlayerPosition.Y
            },
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventTargets = result.Events.Select(item => item.TargetId ?? string.Empty).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventMessages = result.Events.Select(item => item.Message).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };

        return AdvanceAfterInteraction(package, preview, tracker, attempt);
    }

    private static GeneratedMicrogameGoalPreviewModel BuildModel(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedQuestDialoguePreviewService tracker,
        GeneratedPackageRuntimePreviewQuest quest,
        GeneratedQuestPreviewActionResult actionResult,
        GeneratedMicrogameRuntimeGoalProgress runtimeProgress,
        string progressAction,
        bool progressAdvanced)
    {
        var journal = tracker.BuildJournal();
        var entry = journal.Entries.FirstOrDefault(item => string.Equals(item.QuestId, QuestId(quest), StringComparison.OrdinalIgnoreCase));
        var related = BuildRelatedContent(package, preview, quest, progressAction);
        var diagnostics = new List<GeneratedMicrogameGoalDiagnostic>
        {
            Diagnostic("info", "generated_microgame_goal.runtime_state_progress", runtimeProgress.RuntimeQuestId, "Quest progress is stored in existing serializable GameRuntimeState.Quests and Runtime Preview reads that runtime-owned state.")
        };

        if (runtimeProgress.FallbackPreviewJournalUsed)
        {
            diagnostics.Add(Diagnostic("warning", "generated_microgame_goal.preview_journal_fallback", QuestId(quest), "Runtime-owned quest progress was unavailable; existing Runtime Preview journal progress was used as compatibility fallback."));
        }

        if (!actionResult.Ok)
        {
            diagnostics.Add(Diagnostic("warning", "generated_microgame_goal.progress_action_not_applied", QuestId(quest), actionResult.Status));
        }

        var completedStepCount = runtimeProgress.ObjectiveCurrentAmount > 0
            ? (int)Math.Floor(runtimeProgress.ObjectiveCurrentAmount)
            : entry?.CompletedStepCount ?? actionResult.CompletedStepCount;
        var stepCount = runtimeProgress.ObjectiveRequiredAmount > 0
            ? (int)Math.Ceiling(runtimeProgress.ObjectiveRequiredAmount)
            : entry?.StepCount ?? actionResult.StepCount;
        var progressStatus = runtimeProgress.ObjectiveCompleted
            ? "completed"
            : runtimeProgress.RuntimeQuestState;

        return new GeneratedMicrogameGoalPreviewModel
        {
            ActiveGoalSelected = actionResult.Ok || entry != null,
            ActiveQuestId = QuestId(quest),
            ActiveQuestTitle = FirstNonEmpty(quest.Title, quest.PackageQuestId, quest.SourceId),
            CurrentObjectiveText = FirstNonEmpty(entry?.CurrentStep, quest.Steps.FirstOrDefault(), quest.Objectives.FirstOrDefault()),
            ProgressStatus = progressStatus,
            CompletedStepCount = completedStepCount,
            StepCount = stepCount,
            ProgressAdvancedByInteraction = progressAdvanced || runtimeProgress.ProgressAdvanced,
            LastProgressAction = progressAction,
            Related = related,
            AvailableQuestCount = journal.AvailableCount,
            ActiveQuestCount = journal.ActiveCount,
            CompletedQuestCount = journal.CompletedCount,
            ProgressStateSource = runtimeProgress.StateSource,
            RuntimeQuestId = runtimeProgress.RuntimeQuestId,
            RuntimeObjectiveId = runtimeProgress.RuntimeObjectiveId,
            RuntimeObjectiveCurrentAmount = runtimeProgress.ObjectiveCurrentAmount,
            RuntimeObjectiveRequiredAmount = runtimeProgress.ObjectiveRequiredAmount,
            RuntimeObjectiveCompleted = runtimeProgress.ObjectiveCompleted,
            RuntimeState = runtimeProgress.RuntimeState,
            FallbackPreviewJournalUsed = runtimeProgress.FallbackPreviewJournalUsed,
            Diagnostics = diagnostics
        };
    }

    private static GeneratedMicrogameRuntimeGoalProgress CreateRuntimeProgress(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedPackageRuntimePreviewQuest quest,
        string progressAction,
        bool advance)
    {
        var packageQuestId = FirstNonEmpty(quest.PackageQuestId, QuestId(quest));
        var packageQuest = package.Game.Quests
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .FirstOrDefault(item => string.Equals(item.Id, packageQuestId, StringComparison.OrdinalIgnoreCase))
            ?? package.Game.Quests.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault();

        if (packageQuest == null)
        {
            return new GeneratedMicrogameRuntimeGoalProgress
            {
                StateSource = "preview_journal_fallback",
                FallbackPreviewJournalUsed = true
            };
        }

        var runtimeState = new GameRuntimeState
        {
            PackageId = package.Manifest.PackageId,
            CurrentMapId = preview.CurrentMapId,
            Metadata = new Dictionary<string, string>
            {
                ["generated_microgame_goal.progress_source"] = "runtime_state_quests",
                ["generated_microgame_goal.source_quest_id"] = QuestId(quest)
            }
        };
        var objective = BuildRuntimeObjective(packageQuest, quest, progressAction);
        var runtimeQuest = new QuestRuntimeState
        {
            QuestId = packageQuest.Id,
            State = "active",
            CurrentStageId = packageQuest.Stages.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id,
            StartedTick = runtimeState.Tick,
            Metadata = new Dictionary<string, string>(packageQuest.Metadata)
            {
                ["generated_microgame_goal.progress_source"] = "runtime_state_quests",
                ["generated_microgame_goal.source_quest_id"] = QuestId(quest)
            },
            Objectives = [objective]
        };

        runtimeState.Quests.Add(runtimeQuest);
        runtimeState.QuestStates[runtimeQuest.QuestId] = runtimeQuest.State;

        if (advance)
        {
            objective.CurrentAmount = Math.Min(objective.RequiredAmount, objective.CurrentAmount + 1);
            objective.Completed = objective.CurrentAmount >= objective.RequiredAmount;
            runtimeState.Metadata["generated_microgame_goal.last_progress_action"] = progressAction;
            runtimeQuest.Metadata["generated_microgame_goal.last_progress_action"] = progressAction;
            if (objective.Completed)
            {
                runtimeQuest.State = "completed";
                runtimeQuest.CompletedTick = runtimeState.Tick;
                runtimeState.QuestStates[runtimeQuest.QuestId] = runtimeQuest.State;
            }
        }

        return new GeneratedMicrogameRuntimeGoalProgress
        {
            StateSource = "runtime_state_quests",
            RuntimeQuestId = runtimeQuest.QuestId,
            RuntimeObjectiveId = objective.ObjectiveId,
            RuntimeQuestState = runtimeQuest.State,
            ObjectiveCurrentAmount = objective.CurrentAmount,
            ObjectiveRequiredAmount = objective.RequiredAmount,
            ObjectiveCompleted = objective.Completed,
            RuntimeState = runtimeState,
            ProgressAdvanced = advance && objective.CurrentAmount > 0
        };
    }

    private static QuestObjectiveRuntimeState BuildRuntimeObjective(
        LLMGameCreator.Domain.Definitions.QuestDefinition packageQuest,
        GeneratedPackageRuntimePreviewQuest generatedQuest,
        string progressAction)
    {
        var packageObjective = packageQuest.Objectives.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()
                               ?? packageQuest.Stages.SelectMany(item => item.Objectives).OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault();
        var requiredAmount = Math.Max(1, Math.Max(generatedQuest.Steps.Count, generatedQuest.Objectives.Count));
        return new QuestObjectiveRuntimeState
        {
            ObjectiveId = packageObjective?.Id ?? "objective/generated_microgame_progress",
            Kind = "generated_microgame_progress",
            TargetId = FirstNonEmpty(ExtractEventTargets(progressAction).FirstOrDefault(), packageObjective?.TargetId, generatedQuest.Objectives.FirstOrDefault()),
            CurrentAmount = 0,
            RequiredAmount = requiredAmount,
            Completed = false,
            Metadata = new Dictionary<string, string>
            {
                ["generated_microgame_goal.progress_source"] = "runtime_state_quests"
            }
        };
    }

    private static GeneratedMicrogameGoalRelatedContent BuildRelatedContent(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedPackageRuntimePreviewQuest quest,
        string progressAction)
    {
        var objectiveIds = quest.Objectives.Where(NotBlank).ToList();
        var encounter = package.Game.Encounters
            .Where(item => objectiveIds.Contains(item.Id, StringComparer.OrdinalIgnoreCase))
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        var item = package.Game.Items
            .Where(candidate => objectiveIds.Contains(candidate.Id, StringComparer.OrdinalIgnoreCase))
            .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        var eventTargets = ExtractEventTargets(progressAction);
        var npc = ResolveNpcFromEventTargets(preview, eventTargets)
                  ?? preview.Npcs.OrderBy(candidate => candidate.SourceId, StringComparer.Ordinal).FirstOrDefault();

        return new GeneratedMicrogameGoalRelatedContent
        {
            NpcId = npc?.SourceId ?? string.Empty,
            NpcTitle = FirstNonEmpty(npc?.Title, npc?.SourceId),
            ItemId = item?.Id ?? objectiveIds.FirstOrDefault(id => id.StartsWith("item/", StringComparison.OrdinalIgnoreCase)) ?? string.Empty,
            ItemTitle = FirstNonEmpty(item?.Name, item?.Id),
            EncounterId = encounter?.Id ?? objectiveIds.FirstOrDefault(id => id.StartsWith("encounter/", StringComparison.OrdinalIgnoreCase)) ?? string.Empty,
            EncounterTitle = FirstNonEmpty(encounter?.Name, encounter?.Id),
            ObjectiveIds = objectiveIds
        };
    }

    private static GeneratedPackageRuntimePreviewContentItem? ResolveNpcFromEventTargets(
        GeneratedPackageRuntimePreviewModel preview,
        IReadOnlyList<string> eventTargets)
    {
        foreach (var target in eventTargets)
        {
            var segment = IdSegment(target);
            var match = preview.Npcs.FirstOrDefault(npc => string.Equals(IdSegment(npc.SourceId), segment, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ExtractEventTargets(string progressAction)
    {
        const string marker = "target=";
        var index = progressAction.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            return Array.Empty<string>();
        }

        var value = progressAction[(index + marker.Length)..].Trim();
        return value.Length == 0 ? Array.Empty<string>() : [value];
    }

    private static string BuildProgressAction(VisibleGeneratedPlayableRuntimeCommandAttempt attempt)
    {
        var target = attempt.EventTargets.FirstOrDefault(NotBlank);
        return string.IsNullOrWhiteSpace(target)
            ? $"{attempt.CommandType}: {string.Join(", ", attempt.EventTypes)}"
            : $"{attempt.CommandType}: {string.Join(", ", attempt.EventTypes)}; target={target}";
    }

    private static GeneratedPackageRuntimePreviewQuest? SelectActiveQuest(GeneratedPackageRuntimePreviewModel preview) =>
        preview.Quests
            .OrderBy(item => QuestId(item), StringComparer.Ordinal)
            .FirstOrDefault();

    private static string QuestId(GeneratedPackageRuntimePreviewQuest quest) => FirstNonEmpty(quest.SourceId, quest.PackageQuestId);

    private static GeneratedMicrogameGoalPreviewModel Empty(string code, string message) => new()
    {
        Diagnostics = [Diagnostic("warning", code, "generatedContent.quests", message)]
    };

    private static GeneratedMicrogameGoalDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static bool IsSuccessfulInteraction(VisibleGeneratedPlayableRuntimeCommandAttempt attempt) =>
        attempt.Succeeded
        && string.Equals(attempt.CommandType, "interact", StringComparison.OrdinalIgnoreCase)
        && attempt.EventTypes.Any(item => string.Equals(item, RuntimeEventType.InteractionTriggered.ToString(), StringComparison.Ordinal));

    private static bool NotBlank(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static string IdSegment(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        var trimmed = id.Trim();
        var slash = trimmed.LastIndexOf('/');
        return slash >= 0 && slash + 1 < trimmed.Length ? trimmed[(slash + 1)..] : trimmed;
    }
}

public sealed record GeneratedMicrogameGoalPreviewModel
{
    public bool ActiveGoalSelected { get; init; }
    public string ActiveQuestId { get; init; } = string.Empty;
    public string ActiveQuestTitle { get; init; } = string.Empty;
    public string CurrentObjectiveText { get; init; } = string.Empty;
    public string ProgressStatus { get; init; } = string.Empty;
    public int CompletedStepCount { get; init; }
    public int StepCount { get; init; }
    public bool ProgressAdvancedByInteraction { get; init; }
    public string LastProgressAction { get; init; } = string.Empty;
    public GeneratedMicrogameGoalRelatedContent Related { get; init; } = new();
    public int AvailableQuestCount { get; init; }
    public int ActiveQuestCount { get; init; }
    public int CompletedQuestCount { get; init; }
    public string ProgressStateSource { get; init; } = string.Empty;
    public string RuntimeQuestId { get; init; } = string.Empty;
    public string RuntimeObjectiveId { get; init; } = string.Empty;
    public double RuntimeObjectiveCurrentAmount { get; init; }
    public double RuntimeObjectiveRequiredAmount { get; init; }
    public bool RuntimeObjectiveCompleted { get; init; }
    public GameRuntimeState RuntimeState { get; init; } = new();
    public bool FallbackPreviewJournalUsed { get; init; }
    public IReadOnlyList<GeneratedMicrogameGoalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedMicrogameGoalDiagnostic>();
}

public sealed record GeneratedMicrogameRuntimeGoalProgress
{
    public string StateSource { get; init; } = string.Empty;
    public string RuntimeQuestId { get; init; } = string.Empty;
    public string RuntimeObjectiveId { get; init; } = string.Empty;
    public string RuntimeQuestState { get; init; } = string.Empty;
    public double ObjectiveCurrentAmount { get; init; }
    public double ObjectiveRequiredAmount { get; init; }
    public bool ObjectiveCompleted { get; init; }
    public bool ProgressAdvanced { get; init; }
    public bool FallbackPreviewJournalUsed { get; init; }
    public GameRuntimeState RuntimeState { get; init; } = new();
}

public sealed record GeneratedMicrogameGoalRelatedContent
{
    public string NpcId { get; init; } = string.Empty;
    public string NpcTitle { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public string ItemTitle { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string EncounterTitle { get; init; } = string.Empty;
    public IReadOnlyList<string> ObjectiveIds { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedMicrogameGoalDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
