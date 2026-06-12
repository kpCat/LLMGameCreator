using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RequirementEvaluator : IRequirementEvaluator
{
    public RequirementEvaluationResult Evaluate(GamePackageDefinition package, GameRuntimeState state, IEnumerable<RequirementDefinition> requirements, string? inventoryId = null)
    {
        var result = new RequirementEvaluationResult();
        foreach (var requirement in requirements)
        {
            EvaluateOne(package, state, requirement, inventoryId, result);
        }

        return result;
    }

    private static void EvaluateOne(GamePackageDefinition package, GameRuntimeState state, RequirementDefinition requirement, string? inventoryId, RequirementEvaluationResult result)
    {
        var kind = requirement.Kind.Trim();
        if (RuntimeStateHelpers.KindEquals(kind, "always"))
        {
            return;
        }

        if (RuntimeStateHelpers.KindEquals(kind, "has_item") || RuntimeStateHelpers.KindEquals(kind, "inventory_has"))
        {
            var inventory = RuntimeStateHelpers.FindInventory(state, requirement.Scope ?? inventoryId);
            var required = requirement.Amount ?? 1;
            var has = RuntimeStateHelpers.GetItemAmount(inventory, requirement.Id);
            if (has < required)
            {
                AddFailure(result, "requirement.item_missing", kind, requirement.Id, $"Missing item {requirement.Id} x{Format(required)}");
            }

            return;
        }

        if (RuntimeStateHelpers.KindEquals(kind, "resource_at_least") || RuntimeStateHelpers.KindEquals(kind, "network_resource_at_least"))
        {
            var required = requirement.Amount ?? 1;
            var has = RuntimeStateHelpers.GetResourceAmount(state, requirement.Id, requirement.Scope);
            if (has < required)
            {
                AddFailure(result, "requirement.resource_too_low", kind, requirement.Id, $"Resource {requirement.Id} requires {Format(required)}, has {Format(has)}");
            }

            return;
        }

        if (RuntimeStateHelpers.KindEquals(kind, "flag_equals"))
        {
            var expected = requirement.Value ?? "true";
            var actual = RuntimeStateHelpers.GetFlagValue(state, requirement.Id);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                AddFailure(result, "requirement.flag_mismatch", kind, requirement.Id, $"Flag {requirement.Id} is not {expected}");
            }

            return;
        }

        if (RuntimeStateHelpers.KindEquals(kind, "status_present") || RuntimeStateHelpers.KindEquals(kind, "status_active"))
        {
            var targetId = requirement.Scope;
            if (!RuntimeStateHelpers.HasStatus(state, requirement.Id, targetId))
            {
                AddFailure(result, "requirement.status_missing", kind, requirement.Id, $"Status {requirement.Id} is not present");
            }

            return;
        }

        if (RuntimeStateHelpers.KindEquals(kind, "time_available"))
        {
            var required = requirement.Amount ?? 0;
            if (state.Tick < required)
            {
                AddFailure(result, "requirement.time_unavailable", kind, requirement.Id, $"Time requires tick {Format(required)}, current tick is {state.Tick}");
            }

            return;
        }

        var diagnostic = RuntimeStateHelpers.Diagnostic("requirement.kind.unknown", $"Unknown requirement kind: {kind}", requirement.Id);
        result.Diagnostics.Add(diagnostic);
        AddFailure(result, diagnostic.Code, kind, requirement.Id, diagnostic.Message);
    }

    private static void AddFailure(RequirementEvaluationResult result, string code, string kind, string targetId, string message)
    {
        result.Failures.Add(new RequirementFailure
        {
            Code = code,
            RequirementKind = kind,
            TargetId = targetId,
            Message = message
        });
    }

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
