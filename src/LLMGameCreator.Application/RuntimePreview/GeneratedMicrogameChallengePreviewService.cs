using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

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

        var runtimeEvidence = BuildRuntimeEvidence(package, selected, goal);
        var completionBacked = runtimeEvidence.ChallengeResolved
                               && runtimeEvidence.RewardGranted
                               && string.Equals(goal.ProgressStateSource, "runtime_state_quests", StringComparison.OrdinalIgnoreCase);

        return selected with
        {
            Resolved = runtimeEvidence.ChallengeResolved,
            RewardVisible = runtimeEvidence.RewardGranted,
            CompletionVisible = completionBacked,
            CompletedStepCount = completionBacked ? Math.Max(goal.StepCount, goal.CompletedStepCount) : goal.CompletedStepCount,
            StepCount = goal.StepCount,
            CompletionStatus = completionBacked ? "completed" : goal.ProgressStatus,
            ResolveAction = "interact: resolve generated challenge and grant generated reward",
            StateSource = runtimeEvidence.StateSource,
            RuntimeState = runtimeEvidence.RuntimeState,
            RuntimeChallengeResolved = runtimeEvidence.ChallengeResolved,
            RuntimeRewardGranted = runtimeEvidence.RewardGranted,
            RuntimeCompletionBacked = completionBacked,
            RuntimeEncounterId = runtimeEvidence.EncounterId,
            RuntimeRewardItemId = runtimeEvidence.RewardItemId,
            RuntimeRewardAmount = runtimeEvidence.RewardAmount,
            RuntimeChallengeFlagId = runtimeEvidence.ChallengeFlagId,
            FallbackPreviewProjectionUsed = runtimeEvidence.FallbackPreviewProjectionUsed,
            Diagnostics = selected.Diagnostics.Concat(new[]
            {
                Diagnostic("info", "generated_microgame_challenge.runtime_state_evidence", selected.EncounterId, "Challenge resolution, reward and completion evidence are stored in existing serializable GameRuntimeState fields.")
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

    private static GeneratedMicrogameChallengeRuntimeEvidence BuildRuntimeEvidence(
        GamePackageDefinition package,
        GeneratedMicrogameChallengePreviewModel selected,
        GeneratedMicrogameGoalPreviewModel goal)
    {
        if (!string.Equals(goal.ProgressStateSource, "runtime_state_quests", StringComparison.OrdinalIgnoreCase))
        {
            return new GeneratedMicrogameChallengeRuntimeEvidence
            {
                StateSource = "preview_projection_fallback",
                FallbackPreviewProjectionUsed = true
            };
        }

        var runtimeState = CloneRuntimeState(goal.RuntimeState);
        if (string.IsNullOrWhiteSpace(runtimeState.PackageId))
        {
            runtimeState.PackageId = package.Manifest.PackageId;
        }

        var encounterId = FirstNonEmpty(selected.EncounterId, goal.Related.EncounterId);
        var rewardItemId = FirstNonEmpty(selected.RewardItemId, goal.Related.ItemId);
        var flagId = "generated_microgame/challenge_resolved/" + IdSegment(encounterId);
        runtimeState.Metadata["generated_microgame_challenge.state_source"] = "runtime_state_flags_inventory_encounter";
        runtimeState.Metadata["generated_microgame_challenge.encounter_id"] = encounterId;
        runtimeState.Metadata["generated_microgame_challenge.reward_item_id"] = rewardItemId;
        SetFlag(runtimeState, flagId, "true");
        runtimeState.ActiveEncounter = new EncounterRuntimeState
        {
            EncounterId = encounterId,
            Kind = "generated_microgame_challenge",
            Active = false,
            ActionHistory = ["interact: resolve generated challenge and grant generated reward"],
            Metadata = new Dictionary<string, string>
            {
                ["generated_microgame_challenge.state_source"] = "runtime_state_flags_inventory_encounter",
                ["resolved"] = "true"
            }
        };

        var rewardGranted = !string.IsNullOrWhiteSpace(rewardItemId);
        if (rewardGranted)
        {
            var inventory = EnsurePlayerInventory(runtimeState);
            var item = package.Game.Items.FirstOrDefault(candidate => string.Equals(candidate.Id, rewardItemId, StringComparison.OrdinalIgnoreCase));
            inventory.Stacks.RemoveAll(stack => string.Equals(stack.ItemId, rewardItemId, StringComparison.OrdinalIgnoreCase));
            inventory.Stacks.Add(new ItemStackState
            {
                ItemId = rewardItemId,
                Amount = 1,
                QuestItem = item?.QuestItem == true,
                Metadata = new Dictionary<string, string>
                {
                    ["generated_microgame_challenge.reward_source"] = encounterId
                }
            });
        }

        return new GeneratedMicrogameChallengeRuntimeEvidence
        {
            StateSource = "runtime_state_flags_inventory_encounter",
            RuntimeState = runtimeState,
            ChallengeResolved = true,
            RewardGranted = rewardGranted,
            EncounterId = encounterId,
            RewardItemId = rewardItemId,
            RewardAmount = rewardGranted ? 1 : 0,
            ChallengeFlagId = flagId
        };
    }

    private static GameRuntimeState CloneRuntimeState(GameRuntimeState source) => new()
    {
        PackageId = source.PackageId,
        CurrentMapId = source.CurrentMapId,
        PlayerEntityId = source.PlayerEntityId,
        Tick = source.Tick,
        Inventories = source.Inventories.Select(CloneInventory).ToList(),
        Equipment = source.Equipment.ToList(),
        Resources = source.Resources.ToList(),
        Progressions = source.Progressions.ToList(),
        Flags = source.Flags.Select(flag => new RuntimeFlagState { Id = flag.Id, Value = flag.Value }).ToList(),
        Statuses = source.Statuses.ToList(),
        ActiveEncounter = source.ActiveEncounter,
        QuestStates = new Dictionary<string, string>(source.QuestStates, StringComparer.Ordinal),
        Quests = source.Quests.Select(CloneQuest).ToList(),
        ActiveDialogue = source.ActiveDialogue,
        Factions = source.Factions.ToList(),
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static InventoryState CloneInventory(InventoryState source) => new()
    {
        Id = source.Id,
        OwnerKind = source.OwnerKind,
        OwnerId = source.OwnerId,
        Stacks = source.Stacks.Select(stack => new ItemStackState
        {
            ItemId = stack.ItemId,
            Amount = stack.Amount,
            UniqueInstanceId = stack.UniqueInstanceId,
            QuestItem = stack.QuestItem,
            Durability = stack.Durability,
            Charge = stack.Charge,
            Metadata = new Dictionary<string, string>(stack.Metadata, StringComparer.Ordinal)
        }).ToList(),
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static QuestRuntimeState CloneQuest(QuestRuntimeState source) => new()
    {
        QuestId = source.QuestId,
        State = source.State,
        CurrentStageId = source.CurrentStageId,
        Objectives = source.Objectives.Select(objective => new QuestObjectiveRuntimeState
        {
            ObjectiveId = objective.ObjectiveId,
            Kind = objective.Kind,
            TargetId = objective.TargetId,
            CurrentAmount = objective.CurrentAmount,
            RequiredAmount = objective.RequiredAmount,
            Completed = objective.Completed,
            Metadata = new Dictionary<string, string>(objective.Metadata, StringComparer.Ordinal)
        }).ToList(),
        StartedTick = source.StartedTick,
        CompletedTick = source.CompletedTick,
        Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal)
    };

    private static InventoryState EnsurePlayerInventory(GameRuntimeState state)
    {
        var inventory = state.Inventories.FirstOrDefault(item => string.Equals(item.Id, "inventory/player", StringComparison.OrdinalIgnoreCase))
                        ?? state.Inventories.FirstOrDefault(item => string.Equals(item.OwnerKind, "player", StringComparison.OrdinalIgnoreCase));
        if (inventory != null)
        {
            return inventory;
        }

        inventory = new InventoryState
        {
            Id = "inventory/player",
            OwnerKind = "player",
            OwnerId = state.PlayerEntityId
        };
        state.Inventories.Add(inventory);
        return inventory;
    }

    private static void SetFlag(GameRuntimeState state, string id, string value)
    {
        var flag = state.Flags.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        if (flag == null)
        {
            state.Flags.Add(new RuntimeFlagState { Id = id, Value = value });
            return;
        }

        flag.Value = value;
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
    public string StateSource { get; init; } = string.Empty;
    public GameRuntimeState RuntimeState { get; init; } = new();
    public bool RuntimeChallengeResolved { get; init; }
    public bool RuntimeRewardGranted { get; init; }
    public bool RuntimeCompletionBacked { get; init; }
    public string RuntimeEncounterId { get; init; } = string.Empty;
    public string RuntimeRewardItemId { get; init; } = string.Empty;
    public double RuntimeRewardAmount { get; init; }
    public string RuntimeChallengeFlagId { get; init; } = string.Empty;
    public bool FallbackPreviewProjectionUsed { get; init; }
    public IReadOnlyList<GeneratedMicrogameChallengeDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedMicrogameChallengeDiagnostic>();
}

public sealed record GeneratedMicrogameChallengeRuntimeEvidence
{
    public string StateSource { get; init; } = string.Empty;
    public GameRuntimeState RuntimeState { get; init; } = new();
    public bool ChallengeResolved { get; init; }
    public bool RewardGranted { get; init; }
    public string EncounterId { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public double RewardAmount { get; init; }
    public string ChallengeFlagId { get; init; } = string.Empty;
    public bool FallbackPreviewProjectionUsed { get; init; }
}

public sealed record GeneratedMicrogameChallengeDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
