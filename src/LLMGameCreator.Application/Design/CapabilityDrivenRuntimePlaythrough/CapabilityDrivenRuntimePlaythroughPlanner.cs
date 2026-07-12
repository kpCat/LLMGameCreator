using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;

public sealed class CapabilityDrivenRuntimePlaythroughPlanner
{
    private readonly CapabilityDrivenRuntimePlaythroughValidator _validator;

    public CapabilityDrivenRuntimePlaythroughPlanner(CapabilityDrivenRuntimePlaythroughValidator? validator = null)
    {
        _validator = validator ?? new CapabilityDrivenRuntimePlaythroughValidator();
    }

    public CapabilityRuntimePlaythroughPlan Plan(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        GamePackageDefinition package)
    {
        var result = TryPlan(selectedModules, package);
        if (!result.Passed) throw new CapabilityDrivenRuntimePlaythroughException(result.Diagnostics);
        return result.Plan;
    }

    public CapabilityDrivenRuntimePlaythroughPlanningResult TryPlan(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        GamePackageDefinition package)
    {
        var validated = _validator.Validate(selectedModules, package);
        if (!validated.Passed) return validated;
        var remaining = validated.Plan.OrderedActions.ToDictionary(item => item.ActionId, StringComparer.Ordinal);
        var completed = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<CapabilityRuntimePlaythroughAction>();
        while (remaining.Count > 0)
        {
            var next = remaining.Values.Where(action => action.DependsOnActionIds.All(completed.Contains))
                .OrderBy(action => action.Order)
                .ThenBy(action => action.Phase, StringComparer.Ordinal)
                .ThenBy(action => action.ActionId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (next is null)
                return validated with { Diagnostics = [.. validated.Diagnostics, "action dependency cycle rejected"] };
            ordered.Add(next);
            completed.Add(next.ActionId);
            remaining.Remove(next.ActionId);
        }

        var signatureSource = string.Join("\n", ordered.Select(action =>
            action.ActionId + "|" + action.RuntimePrimitiveId + "|" + action.ResolvedTargetId + "|"
            + string.Join(",", action.Args.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "=" + pair.Value)) + "|"
            + string.Join(",", action.DependsOnActionIds) + "|" + action.PresentationOnly)) + "\n";
        var signature = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signatureSource))).ToLowerInvariant();
        var checkpoint = ordered.Where(action => action.CheckpointBoundaryAfter).LastOrDefault()?.ActionId
                         ?? ordered.Where(action => !action.PresentationOnly).Last().ActionId;
        var plan = new CapabilityRuntimePlaythroughPlan
        {
            PlanId = "capability-runtime-playthrough-" + signature[..16],
            SelectedModuleIds = selectedModules.Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList(),
            CapabilityIds = ordered.Select(action => action.CapabilityId).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToList(),
            OrderedActions = ordered,
            CheckpointBoundaryActionId = checkpoint,
            RuntimePrimitiveIds = ordered.Select(action => action.RuntimePrimitiveId).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToList(),
            ActionPlanSignature = signature
        };
        return new CapabilityDrivenRuntimePlaythroughPlanningResult { Passed = true, Plan = plan };
    }
}
