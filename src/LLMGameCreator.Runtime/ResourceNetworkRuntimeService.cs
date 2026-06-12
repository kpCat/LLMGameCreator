using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class ResourceNetworkRuntimeService : IResourceNetworkRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly ICostConsumer _costConsumer;
    private readonly IOutputApplier _outputApplier;

    public ResourceNetworkRuntimeService(IRequirementEvaluator requirementEvaluator, ICostConsumer costConsumer, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _costConsumer = costConsumer;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult TickResourceNodes(GamePackageDefinition package, GameRuntimeState state, int ticks = 1)
    {
        var count = Math.Max(1, ticks);
        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state, Success = true };

        for (var i = 0; i < count; i++)
        {
            working.Tick++;
            foreach (var resource in package.Game.Resources.Where(r => r.RegenPerTick.HasValue && r.RegenPerTick.Value != 0))
            {
                RuntimeStateHelpers.ChangeResource(working, resource, resource.RegenPerTick!.Value);
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceChanged, $"Resource regenerated: {resource.Id}", resource.Id));
            }

            foreach (var node in package.Game.ResourceNodes)
            {
                TickNode(package, working, node, result);
            }
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Message = $"Resource nodes ticked: {count}";
        return result;
    }

    private void TickNode(GamePackageDefinition package, GameRuntimeState state, ResourceNodeDefinition node, GameRuntimeResult result)
    {
        var requirements = _requirementEvaluator.Evaluate(package, state, node.Requirements);
        if (!requirements.Success)
        {
            foreach (var failure in requirements.Failures)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(failure.Code, failure.Message, node.Id, "warning"));
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.RequirementFailed, failure.Message, node.Id));
            }

            return;
        }

        var nodeWorking = RuntimeStateHelpers.CloneState(state);
        var costs = node.Consumption.Concat(node.ConversionInputs).ToList();
        var costResult = _costConsumer.Consume(package, nodeWorking, costs);
        if (!costResult.Success)
        {
            foreach (var diagnostic in costResult.Diagnostics)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(diagnostic.Code, diagnostic.Message, node.Id, "warning"));
            }

            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceNodeTicked, $"Resource node skipped: {node.Name}", node.Id));
            return;
        }

        var outputs = node.Production.Concat(node.Storage).Concat(node.ConversionOutputs).ToList();
        var outputResult = _outputApplier.Apply(package, nodeWorking, outputs);
        if (!outputResult.Success)
        {
            foreach (var diagnostic in outputResult.Diagnostics)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(diagnostic.Code, diagnostic.Message, node.Id, diagnostic.Severity));
            }

            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, $"Resource node output failed: {node.Name}", node.Id));
            return;
        }

        RuntimeStateHelpers.CopyState(nodeWorking, state);
        result.Events.AddRange(costResult.Events);
        result.Events.AddRange(outputResult.Events);
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceNodeTicked, $"Resource node ticked: {node.Name}", node.Id));
    }
}
