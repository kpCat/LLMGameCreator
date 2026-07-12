using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleParameterValueTypes
{
    public const string Integer = "integer";
    public const string Number = "number";
    public const string Boolean = "boolean";
    public const string Enum = "enum";
}

public static class FeatureModuleAuthoringControls
{
    public const string NumericUpDown = "numeric_up_down";
    public const string CheckBox = "check_box";
    public const string ComboBox = "combo_box";
}

public static class FeatureModuleEffectiveValueBindingTargetKinds
{
    public const string MutationOperationField = "mutation_operation_field";
    public const string RuntimeEffectExpectedValue = "runtime_effect_expected_value";
    public const string RuntimePlaythroughArg = "runtime_playthrough_arg";

    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        MutationOperationField,
        RuntimeEffectExpectedValue,
        RuntimePlaythroughArg
    };
}

public sealed record FeatureModuleEffectiveValueBinding
{
    public string BindingId { get; init; } = string.Empty;
    public string TargetKind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string TargetField { get; init; } = string.Empty;
    public string ValueExpression { get; init; } = string.Empty;
}

public sealed record FeatureModuleParameterBinding
{
    public string OperationId { get; init; } = string.Empty;
    public string OperationField { get; init; } = "newValue";
    public string TransformKind { get; init; } = "identity";
    public string AtomicGroupId { get; init; } = string.Empty;
}

public sealed record FeatureModuleParameterDefinition
{
    public string ParameterId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public bool Required { get; init; }
    public JsonElement DefaultValue { get; init; }
    public decimal? Minimum { get; init; }
    public decimal? Maximum { get; init; }
    public decimal? Step { get; init; }
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
    public string Unit { get; init; } = string.Empty;
    public string AuthoringControl { get; init; } = string.Empty;
    public IReadOnlyList<FeatureModuleParameterBinding> Bindings { get; init; } = [];
    public IReadOnlyList<string> ValidationRules { get; init; } = [];
    public IReadOnlyList<string> RuntimeEffectIds { get; init; } = [];
    public string AtomicGroupId { get; init; } = string.Empty;
}

public sealed record FeatureModuleParameterValue
{
    public string ModuleId { get; init; } = string.Empty;
    public string ParameterId { get; init; } = string.Empty;
    public JsonElement Value { get; init; }
}

public sealed record FeatureModuleResolvedParameterValue
{
    public string ModuleId { get; init; } = string.Empty;
    public string ParameterId { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public JsonElement Value { get; init; }
    public bool UsedDefault { get; init; }
    public IReadOnlyList<string> BoundOperationIds { get; init; } = [];
    public string AtomicGroupId { get; init; } = string.Empty;
}

public sealed record FeatureModuleParameterValidationResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<FeatureModuleResolvedParameterValue> EffectiveValues { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleParameterBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<ProductLineRuntimeVariantMutationOperation> EffectiveMutationOperations { get; init; } = [];
    public IReadOnlyList<FeatureModuleResolvedParameterValue> EffectiveParameterValues { get; init; } = [];
    public IReadOnlyList<string> AppliedAtomicGroupIds { get; init; } = [];
    public IReadOnlyList<string> AppliedEffectiveValueBindingIds { get; init; } = [];
    public FeatureModuleCatalogDocument EffectiveCatalog { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
