using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedQuestDialoguePreviewService
{
    private readonly Dictionary<string, GeneratedQuestPreviewProgress> _questProgress = new(StringComparer.OrdinalIgnoreCase);
    private GamePackageDefinition? _package;

    public void StartSession(GamePackageDefinition package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        _questProgress.Clear();
    }

    public IReadOnlyList<GeneratedDialogueDefinition> FindDialoguesLinkedToNpc(string npcId)
    {
        var package = RequirePackage();
        return package.GeneratedContent.Dialogues
            .Where(dialogue => string.Equals(dialogue.NpcId, npcId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public GeneratedDialoguePreviewResult PreviewDialogue(string dialogueId)
    {
        var dialogue = RequirePackage().GeneratedContent.Dialogues.FirstOrDefault(item =>
            string.Equals(item.SourceId, dialogueId, StringComparison.OrdinalIgnoreCase));
        if (dialogue == null)
        {
            return new GeneratedDialoguePreviewResult { Status = "dialogue_not_found", DialogueId = dialogueId };
        }

        return new GeneratedDialoguePreviewResult
        {
            Ok = true,
            Status = "previewed",
            DialogueId = dialogue.SourceId,
            Title = dialogue.Title,
            Lines = dialogue.Lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList()
        };
    }

    public GeneratedQuestPreviewActionResult StartQuest(string questId)
    {
        var quest = FindQuest(questId);
        if (quest == null)
        {
            return new GeneratedQuestPreviewActionResult { Status = "quest_not_found", QuestId = questId };
        }

        var id = QuestId(quest);
        if (!_questProgress.ContainsKey(id))
        {
            _questProgress[id] = new GeneratedQuestPreviewProgress
            {
                QuestId = id,
                Title = quest.Title,
                StepCount = quest.Steps.Count,
                CompletedStepCount = 0,
                Status = quest.Steps.Count == 0 ? GeneratedQuestPreviewStatus.Completed : GeneratedQuestPreviewStatus.Active
            };
        }

        return ActionResult(_questProgress[id], "started");
    }

    public GeneratedQuestPreviewActionResult MarkNextStep(string questId)
    {
        var quest = FindQuest(questId);
        if (quest == null)
        {
            return new GeneratedQuestPreviewActionResult { Status = "quest_not_found", QuestId = questId };
        }

        var id = QuestId(quest);
        if (!_questProgress.TryGetValue(id, out var progress))
        {
            return new GeneratedQuestPreviewActionResult { Status = "quest_not_started", QuestId = id };
        }

        if (progress.Status == GeneratedQuestPreviewStatus.Completed)
        {
            return ActionResult(progress, "already_completed");
        }

        var completed = Math.Min(progress.CompletedStepCount + 1, progress.StepCount);
        progress = progress with
        {
            CompletedStepCount = completed,
            Status = completed >= progress.StepCount ? GeneratedQuestPreviewStatus.Completed : GeneratedQuestPreviewStatus.Active
        };
        _questProgress[id] = progress;
        return ActionResult(progress, "advanced");
    }

    public GeneratedQuestPreviewJournal BuildJournal()
    {
        var package = RequirePackage();
        var entries = package.GeneratedContent.Quests.Select(quest =>
        {
            var id = QuestId(quest);
            _questProgress.TryGetValue(id, out var progress);
            var completedSteps = progress?.CompletedStepCount ?? 0;
            var status = progress?.Status ?? GeneratedQuestPreviewStatus.Available;
            return new GeneratedQuestPreviewJournalEntry
            {
                QuestId = id,
                Title = quest.Title,
                Status = status,
                CompletedStepCount = completedSteps,
                StepCount = quest.Steps.Count,
                CurrentStep = status == GeneratedQuestPreviewStatus.Completed || completedSteps >= quest.Steps.Count
                    ? string.Empty
                    : quest.Steps[completedSteps]
            };
        }).ToList();

        return new GeneratedQuestPreviewJournal { Entries = entries };
    }

    private GamePackageDefinition RequirePackage() =>
        _package ?? throw new InvalidOperationException("Generated content preview session has not been started.");

    private GeneratedQuestSeedDefinition? FindQuest(string questId) =>
        RequirePackage().GeneratedContent.Quests.FirstOrDefault(quest =>
            string.Equals(quest.SourceId, questId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(quest.PackageQuestId, questId, StringComparison.OrdinalIgnoreCase));

    private static string QuestId(GeneratedQuestSeedDefinition quest) =>
        string.IsNullOrWhiteSpace(quest.SourceId) ? quest.PackageQuestId : quest.SourceId;

    private static GeneratedQuestPreviewActionResult ActionResult(GeneratedQuestPreviewProgress progress, string status) =>
        new()
        {
            Ok = true,
            Status = status,
            QuestId = progress.QuestId,
            QuestStatus = progress.Status,
            CompletedStepCount = progress.CompletedStepCount,
            StepCount = progress.StepCount
        };
}

public enum GeneratedQuestPreviewStatus
{
    Available,
    Active,
    Completed
}

public sealed record GeneratedQuestPreviewProgress
{
    public string QuestId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public GeneratedQuestPreviewStatus Status { get; init; }
    public int CompletedStepCount { get; init; }
    public int StepCount { get; init; }
}

public sealed record GeneratedDialoguePreviewResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedQuestPreviewActionResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public GeneratedQuestPreviewStatus QuestStatus { get; init; }
    public int CompletedStepCount { get; init; }
    public int StepCount { get; init; }
}

public sealed record GeneratedQuestPreviewJournal
{
    public IReadOnlyList<GeneratedQuestPreviewJournalEntry> Entries { get; init; } = Array.Empty<GeneratedQuestPreviewJournalEntry>();
    public int AvailableCount => Entries.Count(entry => entry.Status == GeneratedQuestPreviewStatus.Available);
    public int ActiveCount => Entries.Count(entry => entry.Status == GeneratedQuestPreviewStatus.Active);
    public int CompletedCount => Entries.Count(entry => entry.Status == GeneratedQuestPreviewStatus.Completed);
}

public sealed record GeneratedQuestPreviewJournalEntry
{
    public string QuestId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public GeneratedQuestPreviewStatus Status { get; init; }
    public int CompletedStepCount { get; init; }
    public int StepCount { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
}
