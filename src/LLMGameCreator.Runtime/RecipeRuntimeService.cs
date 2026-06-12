using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RecipeRuntimeService : IRecipeRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly ICostConsumer _costConsumer;
    private readonly IOutputApplier _outputApplier;

    public RecipeRuntimeService(IRequirementEvaluator requirementEvaluator, ICostConsumer costConsumer, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _costConsumer = costConsumer;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult CraftRecipe(GamePackageDefinition package, GameRuntimeState state, string recipeId, string? inventoryId = null)
    {
        var recipe = package.Game.Recipes.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, recipeId));
        if (recipe == null)
        {
            return Failure(state, "recipe.missing", $"Recipe not found: {recipeId}", recipeId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };

        var requirements = _requirementEvaluator.Evaluate(package, working, recipe.Requirements, inventoryId);
        AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var allCosts = recipe.Inputs.Concat(recipe.Costs).ToList();
        var costResult = _costConsumer.Consume(package, working, allCosts, inventoryId);
        result.Events.AddRange(costResult.Events);
        result.Diagnostics.AddRange(costResult.Diagnostics);
        if (!costResult.Success)
        {
            result.Success = false;
            result.Message = $"Recipe failed: {recipe.Id}";
            return result;
        }

        var outputResult = _outputApplier.Apply(package, working, recipe.Outputs, inventoryId);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Recipe failed: {recipe.Id}";
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.Success = true;
        result.State = state;
        result.Message = $"Recipe crafted: {recipe.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.RecipeCrafted, $"Recipe crafted: {recipe.Name}", recipe.Id));
        return result;
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

    internal static void AddRequirementFailures(GameRuntimeResult result, RequirementEvaluationResult requirements)
    {
        foreach (var failure in requirements.Failures)
        {
            result.Success = false;
            result.Message = failure.Message;
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(failure.Code, failure.Message, failure.TargetId));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.RequirementFailed, failure.Message, failure.TargetId));
        }

        result.Diagnostics.AddRange(requirements.Diagnostics);
    }
}
