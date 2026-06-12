using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class TransactionRuntimeService : ITransactionRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly ICostConsumer _costConsumer;
    private readonly IOutputApplier _outputApplier;

    public TransactionRuntimeService(IRequirementEvaluator requirementEvaluator, ICostConsumer costConsumer, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _costConsumer = costConsumer;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult ExecuteTransaction(GamePackageDefinition package, GameRuntimeState state, string transactionId, string? inventoryId = null)
    {
        var transaction = package.Game.Transactions.FirstOrDefault(t => RuntimeStateHelpers.IdEquals(t.Id, transactionId));
        if (transaction == null)
        {
            return Failure(state, "transaction.missing", $"Transaction not found: {transactionId}", transactionId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };

        var requirements = _requirementEvaluator.Evaluate(package, working, transaction.Requirements, inventoryId);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var costResult = _costConsumer.Consume(package, working, transaction.Costs, inventoryId);
        result.Events.AddRange(costResult.Events);
        result.Diagnostics.AddRange(costResult.Diagnostics);
        if (!costResult.Success)
        {
            result.Success = false;
            result.Message = $"Transaction failed: {transaction.Id}";
            return result;
        }

        var outputResult = _outputApplier.Apply(package, working, transaction.Outputs, inventoryId);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Transaction failed: {transaction.Id}";
            return result;
        }

        if (!string.IsNullOrWhiteSpace(transaction.StockLootTableId))
        {
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(
                "transaction.stock.preview_only",
                "Stock loot tables are not restocked by transaction runtime v1.",
                transaction.Id,
                "warning"));
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.Success = true;
        result.State = state;
        result.Message = $"Transaction executed: {transaction.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.TransactionExecuted, $"Transaction executed: {transaction.Name}", transaction.Id));
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
}
