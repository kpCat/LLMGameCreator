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
        return BuildModel(package, preview, tracker, quest, started, progressAction: string.Empty, progressAdvanced: false);
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
        return BuildModel(package, preview, tracker, quest, advanced, action, progressAdvanced: advanced.Ok);
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
        string progressAction,
        bool progressAdvanced)
    {
        var journal = tracker.BuildJournal();
        var entry = journal.Entries.FirstOrDefault(item => string.Equals(item.QuestId, QuestId(quest), StringComparison.OrdinalIgnoreCase));
        var related = BuildRelatedContent(package, preview, quest, progressAction);
        var diagnostics = new List<GeneratedMicrogameGoalDiagnostic>
        {
            Diagnostic("info", "generated_microgame_goal.preview_level_progress", QuestId(quest), "Quest progress is tracked by the existing Runtime Preview quest journal without changing runtime or package contracts.")
        };

        if (!actionResult.Ok)
        {
            diagnostics.Add(Diagnostic("warning", "generated_microgame_goal.progress_action_not_applied", QuestId(quest), actionResult.Status));
        }

        return new GeneratedMicrogameGoalPreviewModel
        {
            ActiveGoalSelected = actionResult.Ok || entry != null,
            ActiveQuestId = QuestId(quest),
            ActiveQuestTitle = FirstNonEmpty(quest.Title, quest.PackageQuestId, quest.SourceId),
            CurrentObjectiveText = FirstNonEmpty(entry?.CurrentStep, quest.Steps.FirstOrDefault(), quest.Objectives.FirstOrDefault()),
            ProgressStatus = entry?.Status.ToString() ?? actionResult.QuestStatus.ToString(),
            CompletedStepCount = entry?.CompletedStepCount ?? actionResult.CompletedStepCount,
            StepCount = entry?.StepCount ?? actionResult.StepCount,
            ProgressAdvancedByInteraction = progressAdvanced,
            LastProgressAction = progressAction,
            Related = related,
            AvailableQuestCount = journal.AvailableCount,
            ActiveQuestCount = journal.ActiveCount,
            CompletedQuestCount = journal.CompletedCount,
            Diagnostics = diagnostics
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
    public IReadOnlyList<GeneratedMicrogameGoalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedMicrogameGoalDiagnostic>();
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
