using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class InteractionRuntimeService : IInteractionRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;
    private readonly IRecipeRuntimeService _recipeRuntimeService;
    private readonly ITransactionRuntimeService _transactionRuntimeService;
    private readonly IContainerRuntimeService _containerRuntimeService;
    private readonly IHarvestRuntimeService _harvestRuntimeService;
    private readonly IUseItemRuntimeService _useItemRuntimeService;
    private readonly IDialogueRuntimeService? _dialogueRuntimeService;
    private readonly IQuestRuntimeService? _questRuntimeService;
    private readonly IEncounterRuntimeService? _encounterRuntimeService;

    public InteractionRuntimeService(
        IRequirementEvaluator requirementEvaluator,
        IOutputApplier outputApplier,
        IRecipeRuntimeService recipeRuntimeService,
        ITransactionRuntimeService transactionRuntimeService,
        IContainerRuntimeService? containerRuntimeService = null,
        IHarvestRuntimeService? harvestRuntimeService = null,
        IUseItemRuntimeService? useItemRuntimeService = null,
        IDialogueRuntimeService? dialogueRuntimeService = null,
        IQuestRuntimeService? questRuntimeService = null,
        IEncounterRuntimeService? encounterRuntimeService = null)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
        _recipeRuntimeService = recipeRuntimeService;
        _transactionRuntimeService = transactionRuntimeService;
        _containerRuntimeService = containerRuntimeService ?? new ContainerRuntimeService();
        _harvestRuntimeService = harvestRuntimeService ?? new HarvestRuntimeService(new RequirementEvaluator(), new CostConsumer(), new OutputApplier());
        _useItemRuntimeService = useItemRuntimeService ?? new UseItemRuntimeService(new RequirementEvaluator(), new OutputApplier());
        _dialogueRuntimeService = dialogueRuntimeService;
        _questRuntimeService = questRuntimeService;
        _encounterRuntimeService = encounterRuntimeService;
    }

    public GameRuntimeResult ExecuteInteraction(GamePackageDefinition package, GameRuntimeState state, string interactionId, string? targetId = null, string? inventoryId = null)
    {
        var interaction = package.Game.Interactions.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, interactionId));
        if (interaction == null)
        {
            return Failure(state, "interaction.missing", $"Interaction not found: {interactionId}", interactionId);
        }

        var routed = TryRouteMetadataInteraction(package, state, interaction, targetId, inventoryId);
        if (routed != null)
        {
            return routed;
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "craft")
            && interaction.Effects.Count == 0
            && interaction.Conditions.Count == 0
            && TryGetMetadataTarget(interactionId, "recipe/", out var recipeId))
        {
            return _recipeRuntimeService.CraftRecipe(package, state, recipeId, inventoryId);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "trade")
            && interaction.Effects.Count == 0
            && interaction.Conditions.Count == 0
            && TryGetMetadataTarget(interactionId, "transaction/", out var transactionId))
        {
            return _transactionRuntimeService.ExecuteTransaction(package, state, transactionId, inventoryId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(
            package,
            working,
            interaction.Conditions.Select(RuntimeEffectMapper.ToRequirement),
            inventoryId);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var outputs = interaction.Effects.Select(effect =>
        {
            var output = RuntimeEffectMapper.ToOutput(effect);
            if (!string.IsNullOrWhiteSpace(targetId) && string.IsNullOrWhiteSpace(output.Scope))
            {
                output.Scope = targetId;
            }

            return output;
        }).ToList();

        var outputResult = _outputApplier.Apply(package, working, outputs, inventoryId);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Interaction failed: {interaction.Id}";
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.Success = true;
        result.State = state;
        result.Message = $"Interaction executed: {interaction.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(
            GameRuntimeEventType.InteractionTriggered,
            $"Interaction triggered: {interaction.Kind}",
            interaction.Id,
            new Dictionary<string, string> { ["kind"] = interaction.Kind, ["targetId"] = targetId ?? string.Empty }));
        return result;
    }

    private GameRuntimeResult? TryRouteMetadataInteraction(GamePackageDefinition package, GameRuntimeState state, Domain.Definitions.InteractionDefinition interaction, string? targetId, string? inventoryId)
    {
        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "open_container"))
        {
            return TryGetMetadata(interaction, "container_id", out var containerId)
                ? _containerRuntimeService.OpenContainer(package, state, containerId)
                : Failure(state, "interaction.container_metadata_missing", "open_container interaction requires metadata.container_id.", interaction.Id);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "talk"))
        {
            if (_dialogueRuntimeService == null)
            {
                return Failure(state, "interaction.dialogue_service_missing", "talk interaction requires dialogue runtime service.", interaction.Id);
            }

            return TryGetMetadata(interaction, "dialogue_id", out var dialogueId)
                ? _dialogueRuntimeService.OpenDialogue(package, state, dialogueId)
                : Failure(state, "interaction.dialogue_metadata_missing", "talk interaction requires metadata.dialogue_id.", interaction.Id);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "quest") || RuntimeStateHelpers.KindEquals(interaction.Kind, "start_quest"))
        {
            if (_questRuntimeService == null)
            {
                return Failure(state, "interaction.quest_service_missing", "quest interaction requires quest runtime service.", interaction.Id);
            }

            return TryGetMetadata(interaction, "quest_id", out var questId)
                ? _questRuntimeService.StartQuest(package, state, questId)
                : Failure(state, "interaction.quest_metadata_missing", "quest interaction requires metadata.quest_id.", interaction.Id);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "complete_quest"))
        {
            if (_questRuntimeService == null)
            {
                return Failure(state, "interaction.quest_service_missing", "complete_quest interaction requires quest runtime service.", interaction.Id);
            }

            return TryGetMetadata(interaction, "quest_id", out var questId)
                ? _questRuntimeService.CompleteQuest(package, state, questId)
                : Failure(state, "interaction.quest_metadata_missing", "complete_quest interaction requires metadata.quest_id.", interaction.Id);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "harvest_resource"))
        {
            if (!TryGetMetadata(interaction, "resource_node_id", out var nodeId))
            {
                return Failure(state, "interaction.resource_node_metadata_missing", "harvest_resource interaction requires metadata.resource_node_id.", interaction.Id);
            }

            TryGetMetadata(interaction, "tool_item_id", out var toolItemId);
            return _harvestRuntimeService.HarvestResourceNode(package, state, nodeId, inventoryId, toolItemId, ReadSeed(interaction));
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "use_item_on_target"))
        {
            return TryGetMetadata(interaction, "item_id", out var itemId)
                ? _useItemRuntimeService.UseItem(package, state, itemId, inventoryId, targetId)
                : Failure(state, "interaction.item_metadata_missing", "use_item_on_target interaction requires metadata.item_id.", interaction.Id);
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "craft"))
        {
            if (TryGetMetadata(interaction, "recipe_id", out var recipeId))
            {
                return _recipeRuntimeService.CraftRecipe(package, state, recipeId, inventoryId);
            }
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "trade"))
        {
            if (TryGetMetadata(interaction, "transaction_id", out var transactionId))
            {
                return _transactionRuntimeService.ExecuteTransaction(package, state, transactionId, inventoryId);
            }
        }

        if (RuntimeStateHelpers.KindEquals(interaction.Kind, "fight"))
        {
            if (_encounterRuntimeService == null)
            {
                return Failure(state, "interaction.encounter_service_missing", "fight interaction requires encounter runtime service.", interaction.Id);
            }

            return TryGetMetadata(interaction, "encounter_id", out var encounterId)
                ? _encounterRuntimeService.StartEncounter(package, state, encounterId, ReadSeed(interaction))
                : Failure(state, "interaction.encounter_metadata_missing", "fight interaction requires metadata.encounter_id.", interaction.Id);
        }

        return null;
    }

    private static bool TryGetMetadata(Domain.Definitions.InteractionDefinition interaction, string key, out string value)
    {
        return interaction.Metadata.TryGetValue(key, out value!) && !string.IsNullOrWhiteSpace(value);
    }

    private static int? ReadSeed(Domain.Definitions.InteractionDefinition interaction)
    {
        return interaction.Metadata.TryGetValue("seed", out var value) && int.TryParse(value, out var seed) ? seed : null;
    }

    private static bool TryGetMetadataTarget(string interactionId, string prefix, out string targetId)
    {
        targetId = string.Empty;
        var index = interactionId.IndexOf(prefix, StringComparison.Ordinal);
        if (index < 0)
        {
            return false;
        }

        targetId = interactionId.Substring(index);
        return !string.IsNullOrWhiteSpace(targetId);
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string targetId)
    {
        return new GameRuntimeResult
        {
            Success = false,
            State = state,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) },
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId) }
        };
    }
}
