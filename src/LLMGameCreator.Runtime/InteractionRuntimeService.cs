using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class InteractionRuntimeService : IInteractionRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;
    private readonly IRecipeRuntimeService _recipeRuntimeService;
    private readonly ITransactionRuntimeService _transactionRuntimeService;

    public InteractionRuntimeService(
        IRequirementEvaluator requirementEvaluator,
        IOutputApplier outputApplier,
        IRecipeRuntimeService recipeRuntimeService,
        ITransactionRuntimeService transactionRuntimeService)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
        _recipeRuntimeService = recipeRuntimeService;
        _transactionRuntimeService = transactionRuntimeService;
    }

    public GameRuntimeResult ExecuteInteraction(GamePackageDefinition package, GameRuntimeState state, string interactionId, string? targetId = null, string? inventoryId = null)
    {
        var interaction = package.Game.Interactions.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, interactionId));
        if (interaction == null)
        {
            return Failure(state, "interaction.missing", $"Interaction not found: {interactionId}", interactionId);
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
