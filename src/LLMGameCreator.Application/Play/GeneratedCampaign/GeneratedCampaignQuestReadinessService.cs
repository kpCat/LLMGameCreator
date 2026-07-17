using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed record GeneratedCampaignQuestObjectiveReadiness
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public double CurrentAmount { get; init; }
    public double RequiredAmount { get; init; }
    public bool Optional { get; init; }
    public bool Satisfied { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignQuestReadiness
{
    public string QuestId { get; init; } = string.Empty;
    public bool Generated { get; init; }
    public bool MappingExact { get; init; }
    public bool Active { get; init; }
    public bool Completed { get; init; }
    public bool Ready { get; init; }
    public IReadOnlyList<GeneratedCampaignQuestObjectiveReadiness> Objectives { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedCampaignQuestReadinessService
{
    public IReadOnlyList<GeneratedCampaignQuestReadiness> EvaluateAll(
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        return session.GameplayState.Quests
            .Select(runtime => Evaluate(package, session, runtime.QuestId))
            .ToList();
    }

    public GeneratedCampaignQuestReadiness Evaluate(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        string questId)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        var definitions = package.Game.Quests.Where(item => IdEquals(item.Id, questId)).ToList();
        var runtimeStates = session.GameplayState.Quests.Where(item => IdEquals(item.QuestId, questId)).ToList();
        if (definitions.Count != 1 || runtimeStates.Count != 1)
            return new GeneratedCampaignQuestReadiness
            {
                QuestId = questId,
                Diagnostics = ["campaign.quest_definition_or_state_ambiguous"]
            };
        var definition = definitions[0];
        var runtime = runtimeStates[0];
        var generatedMarker = KindEquals(definition.Kind, "generated_quest")
                              || definition.Tags.Any(tag => KindEquals(tag, "generated"));
        var mappingCount = package.GeneratedContent.Quests.Count(item =>
            IdEquals(item.PackageQuestId, definition.Id));
        if (!generatedMarker)
            return new GeneratedCampaignQuestReadiness
            {
                QuestId = definition.Id,
                Active = KindEquals(runtime.State, "active"),
                Completed = KindEquals(runtime.State, "completed")
            };
        if (mappingCount != 1)
            return new GeneratedCampaignQuestReadiness
            {
                QuestId = definition.Id,
                Generated = true,
                Active = KindEquals(runtime.State, "active"),
                Completed = KindEquals(runtime.State, "completed"),
                Diagnostics = ["campaign.generated_quest_mapping_invalid"]
            };

        var definitionsById = CurrentObjectives(definition, runtime).ToDictionary(
            item => item.Id, item => item, StringComparer.OrdinalIgnoreCase);
        var objectives = new List<GeneratedCampaignQuestObjectiveReadiness>();
        var diagnostics = new List<string>();
        foreach (var runtimeObjective in runtime.Objectives)
        {
            if (!definitionsById.TryGetValue(runtimeObjective.ObjectiveId, out var objective))
            {
                var missing = "campaign.quest_objective_definition_missing";
                diagnostics.Add(missing);
                objectives.Add(new GeneratedCampaignQuestObjectiveReadiness
                {
                    ObjectiveId = runtimeObjective.ObjectiveId,
                    Kind = runtimeObjective.Kind,
                    RequiredAmount = runtimeObjective.RequiredAmount,
                    Optional = false,
                    Diagnostics = [missing]
                });
                continue;
            }

            var required = objective.RequiredAmount <= 0 ? 1 : objective.RequiredAmount;
            var current = CurrentAmount(package, session.GameplayState, objective, required);
            var supported = KindEquals(objective.Kind, "complete_encounter")
                            || KindEquals(objective.Kind, "has_item");
            var objectiveDiagnostics = supported
                ? Array.Empty<string>()
                : ["campaign.quest_objective_kind_unsupported:" + objective.Kind];
            diagnostics.AddRange(objectiveDiagnostics);
            objectives.Add(new GeneratedCampaignQuestObjectiveReadiness
            {
                ObjectiveId = objective.Id,
                Kind = objective.Kind,
                TargetId = objective.TargetId ?? string.Empty,
                CurrentAmount = current,
                RequiredAmount = required,
                Optional = objective.Optional,
                Satisfied = supported && current >= required,
                Diagnostics = objectiveDiagnostics
            });
        }

        var requiredObjectives = objectives.Where(item => !item.Optional).ToList();
        var completed = KindEquals(runtime.State, "completed");
        return new GeneratedCampaignQuestReadiness
        {
            QuestId = definition.Id,
            Generated = true,
            MappingExact = true,
            Active = KindEquals(runtime.State, "active"),
            Completed = completed,
            Ready = !completed && KindEquals(runtime.State, "active")
                                && requiredObjectives.Count > 0
                                && requiredObjectives.All(item => item.Satisfied),
            Objectives = objectives,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    public bool IsGeneratedQuest(GamePackageDefinition package, string questId)
    {
        var definitions = package.Game.Quests.Where(item => IdEquals(item.Id, questId)).ToList();
        if (definitions.Count != 1) return false;
        var definition = definitions[0];
        return KindEquals(definition.Kind, "generated_quest")
               || definition.Tags.Any(tag => KindEquals(tag, "generated"));
    }

    private static IReadOnlyList<QuestObjectiveDefinition> CurrentObjectives(
        QuestDefinition definition,
        QuestRuntimeState runtime)
    {
        var stage = definition.Stages.SingleOrDefault(item => IdEquals(item.Id, runtime.CurrentStageId));
        return stage is { Objectives.Count: > 0 } ? stage.Objectives : definition.Objectives;
    }

    private static double CurrentAmount(
        GamePackageDefinition package,
        GameRuntimeState state,
        QuestObjectiveDefinition objective,
        double required)
    {
        if (KindEquals(objective.Kind, "complete_encounter"))
        {
            var encounter = state.ActiveEncounter;
            if (encounter is null || !IdEquals(encounter.EncounterId, objective.TargetId)
                || encounter.Active
                || encounter.ActionHistory.Any(item => KindEquals(item, "flee")))
                return 0;
            var players = encounter.Participants.Where(item => KindEquals(item.Team, "player")).ToList();
            var opponents = encounter.Participants.Where(item => !KindEquals(item.Team, "player")).ToList();
            return players.Any(item => item.Alive) && opponents.Count > 0
                   && opponents.All(item => !item.Alive)
                ? required
                : 0;
        }
        if (KindEquals(objective.Kind, "has_item"))
        {
            return state.Inventories.Where(inventory => PlayerOwned(state, inventory))
                .SelectMany(inventory => inventory.Stacks)
                .Where(stack => IdEquals(stack.ItemId, objective.TargetId))
                .Sum(stack => stack.Amount);
        }
        return 0;
    }

    private static bool PlayerOwned(GameRuntimeState state, InventoryState inventory) =>
        KindEquals(inventory.OwnerKind, "player")
        && (string.IsNullOrWhiteSpace(inventory.OwnerId)
            || IdEquals(inventory.OwnerId, state.PlayerEntityId));

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
