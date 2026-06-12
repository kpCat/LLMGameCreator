using LLMGameCreator.Domain.Definitions;

namespace LLMGameCreator.Runtime;

internal static class RuntimeEffectMapper
{
    public static RequirementDefinition ToRequirement(ConditionDefinition condition)
    {
        var args = condition.Args;
        return new RequirementDefinition
        {
            Kind = condition.Type,
            Id = Get(args, "id") ?? Get(args, "itemId") ?? Get(args, "resourceId") ?? Get(args, "flagId") ?? Get(args, "statusId") ?? string.Empty,
            Amount = ParseDouble(Get(args, "amount")),
            Value = Get(args, "value"),
            Scope = Get(args, "scope") ?? Get(args, "inventoryId") ?? Get(args, "targetId"),
            Metadata = new Dictionary<string, string>(args)
        };
    }

    public static OutputDefinition ToOutput(EffectDefinition effect)
    {
        var args = effect.Args;
        return new OutputDefinition
        {
            Kind = effect.Type,
            Id = Get(args, "id") ?? Get(args, "itemId") ?? Get(args, "resourceId") ?? Get(args, "flagId") ?? Get(args, "statusId") ?? string.Empty,
            Amount = ParseDouble(Get(args, "amount")) ?? 1,
            Scope = Get(args, "scope") ?? Get(args, "inventoryId") ?? Get(args, "targetId"),
            Mode = Get(args, "mode") ?? Get(args, "value"),
            Metadata = new Dictionary<string, string>(args)
        };
    }

    private static string? Get(Dictionary<string, string> args, string key)
    {
        return args.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }

    private static double? ParseDouble(string? value)
    {
        return double.TryParse(value, out var parsed) ? parsed : null;
    }
}
