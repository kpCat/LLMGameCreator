using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedMicrogameChallengePreviewService
{
    public GeneratedMicrogameChallengePreviewModel BuildFromRuntimeAttempt(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedMicrogameGoalPreviewModel goal,
        VisibleGeneratedPlayableRuntimeAttempt runtimeAttempt)
    {
        ArgumentNullException.ThrowIfNull(runtimeAttempt);
        var selected = SelectChallenge(package, preview, goal);
        return runtimeAttempt.CommandAttempts.Any(IsSuccessfulInteraction)
            ? ResolveAfterInteraction(package, preview, goal, selected)
            : selected;
    }

    public GeneratedMicrogameChallengePreviewModel ResolveAfterInteraction(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedMicrogameGoalPreviewModel goal,
        GeneratedMicrogameChallengePreviewModel? selected = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(goal);

        selected ??= SelectChallenge(package, preview, goal);
        if (!selected.ChallengeSelected)
        {
            return selected;
        }

        return selected with
        {
            Resolved = true,
            RewardVisible = true,
            CompletionVisible = true,
            CompletedStepCount = Math.Max(goal.StepCount, goal.CompletedStepCount),
            StepCount = goal.StepCount,
            CompletionStatus = "completed",
            ResolveAction = "interact: resolve generated challenge and grant generated reward",
            Diagnostics = selected.Diagnostics.Concat(new[]
            {
                Diagnostic("info", "generated_microgame_challenge.preview_level_resolution", selected.EncounterId, "Challenge resolution, reward and completion are deterministic Runtime Preview projections; package/runtime contracts were not redesigned.")
            }).ToList()
        };
    }

    public GeneratedMicrogameChallengePreviewModel SelectChallenge(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel preview,
        GeneratedMicrogameGoalPreviewModel goal)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(goal);

        if (!goal.ActiveGoalSelected)
        {
            return Empty("generated_microgame_challenge.no_active_goal", "No active generated goal was available for challenge selection.");
        }

        var encounterId = FirstNonEmpty(goal.Related.EncounterId, preview.Encounters.OrderBy(item => item.SourceId, StringComparer.Ordinal).FirstOrDefault()?.SourceId);
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            return Empty("generated_microgame_challenge.no_encounter", "Generated package has no encounter/challenge to resolve.");
        }

        var encounter = package.Game.Encounters.FirstOrDefault(item => string.Equals(item.Id, encounterId, StringComparison.OrdinalIgnoreCase));
        var projectedEncounter = preview.Encounters.FirstOrDefault(item =>
                                     string.Equals(item.SourceId, encounterId, StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(IdSegment(item.SourceId), IdSegment(encounterId), StringComparison.OrdinalIgnoreCase))
                                 ?? preview.Encounters.OrderBy(item => item.SourceId, StringComparer.Ordinal).FirstOrDefault();
        var rewardItemId = FirstNonEmpty(goal.Related.ItemId, encounter?.Rewards.FirstOrDefault(item => IsItemReward(item.Kind))?.Id);
        var rewardItem = package.Game.Items.FirstOrDefault(item => string.Equals(item.Id, rewardItemId, StringComparison.OrdinalIgnoreCase));

        return new GeneratedMicrogameChallengePreviewModel
        {
            ChallengeSelected = true,
            EncounterId = encounterId,
            EncounterTitle = FirstNonEmpty(goal.Related.EncounterTitle, encounter?.Name, projectedEncounter?.Title, encounterId),
            QuestId = goal.ActiveQuestId,
            QuestTitle = goal.ActiveQuestTitle,
            RewardItemId = rewardItemId,
            RewardTitle = FirstNonEmpty(goal.Related.ItemTitle, rewardItem?.Name, rewardItemId),
            RelatedNpcId = goal.Related.NpcId,
            RelatedNpcTitle = goal.Related.NpcTitle,
            CompletedStepCount = goal.CompletedStepCount,
            StepCount = goal.StepCount,
            CompletionStatus = goal.ProgressStatus,
            Diagnostics =
            [
                Diagnostic("info", "generated_microgame_challenge.no_external_execution", encounterId, "No LLM, provider, Lua, Unity or media execution was invoked.")
            ]
        };
    }

    private static GeneratedMicrogameChallengePreviewModel Empty(string code, string message) => new()
    {
        Diagnostics = [Diagnostic("warning", code, "generatedContent.encounters", message)]
    };

    private static GeneratedMicrogameChallengeDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static bool IsSuccessfulInteraction(VisibleGeneratedPlayableRuntimeCommandAttempt attempt) =>
        attempt.Succeeded
        && string.Equals(attempt.CommandType, "interact", StringComparison.OrdinalIgnoreCase)
        && attempt.EventTypes.Any(item => string.Equals(item, "InteractionTriggered", StringComparison.Ordinal));

    private static bool IsItemReward(string kind) =>
        string.Equals(kind, "item", StringComparison.OrdinalIgnoreCase)
        || string.Equals(kind, "add_item", StringComparison.OrdinalIgnoreCase);

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

public sealed record GeneratedMicrogameChallengePreviewModel
{
    public bool ChallengeSelected { get; init; }
    public string EncounterId { get; init; } = string.Empty;
    public string EncounterTitle { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string QuestTitle { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RewardTitle { get; init; } = string.Empty;
    public string RelatedNpcId { get; init; } = string.Empty;
    public string RelatedNpcTitle { get; init; } = string.Empty;
    public bool Resolved { get; init; }
    public bool RewardVisible { get; init; }
    public bool CompletionVisible { get; init; }
    public int CompletedStepCount { get; init; }
    public int StepCount { get; init; }
    public string CompletionStatus { get; init; } = string.Empty;
    public string ResolveAction { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedMicrogameChallengeDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedMicrogameChallengeDiagnostic>();
}

public sealed record GeneratedMicrogameChallengeDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
