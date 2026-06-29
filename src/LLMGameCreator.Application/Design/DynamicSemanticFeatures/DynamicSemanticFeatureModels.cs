namespace LLMGameCreator.Application.Design.DynamicSemanticFeatures;

public static class DynamicSemanticFeatureVocabulary
{
    public static readonly IReadOnlySet<string> ValidScopes = new HashSet<string>(
        [
            "world",
            "kingdom",
            "region",
            "biome",
            "settlement",
            "faction",
            "species",
            "archetype",
            "npc",
            "item",
            "resource",
            "quest",
            "dialogue",
            "event",
            "magic",
            "combat",
            "relationship"
        ],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ValidValueKinds = new HashSet<string>(
        ["flag", "number", "enum", "weighted_tag", "relation", "text_key", "list"],
        StringComparer.Ordinal);
}

public sealed record DynamicSemanticFeatureDefinition
{
    public string FeatureId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public string ValueKind { get; init; } = string.Empty;
    public string Cardinality { get; init; } = "single";
    public string RequiredMode { get; init; } = "optional";
    public string DefaultStrategy { get; init; } = "none";
    public DynamicSemanticFeatureValue? DefaultValue { get; init; }
    public string InheritanceMode { get; init; } = "none";
    public IReadOnlyList<string> InheritedSourceScopes { get; init; } = [];
    public IReadOnlyList<DynamicSemanticConditionClause> ApplicabilityConditions { get; init; } = [];
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<string> Conflicts { get; init; } = [];
    public IReadOnlyList<string> Requires { get; init; } = [];
    public string AuthoringGroup { get; init; } = string.Empty;
    public string Provenance { get; init; } = string.Empty;
    public string Status { get; init; } = "ready";
    public string Notes { get; init; } = string.Empty;
}

public sealed record DynamicSemanticFeatureAssignment
{
    public string TargetId { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public DynamicSemanticFeatureValue Value { get; init; } = new();
    public string SourceLayer { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string OverrideMode { get; init; } = "set";
    public double Weight { get; init; } = 1;
    public int Priority { get; init; }
    public string Provenance { get; init; } = string.Empty;
    public string Status { get; init; } = "ready";
}

public sealed record DynamicSemanticFeatureValue
{
    public string ValueKind { get; init; } = string.Empty;
    public bool? FlagValue { get; init; }
    public double? NumberValue { get; init; }
    public string? EnumValue { get; init; }
    public IReadOnlyList<DynamicSemanticWeightedTag> WeightedTags { get; init; } = [];
    public DynamicSemanticRelationValue? RelationValue { get; init; }
    public string? TextKeyValue { get; init; }
    public IReadOnlyList<string> ListValues { get; init; } = [];

    public string StableValueKey() =>
        ValueKind switch
        {
            "flag" => FlagValue == true ? "true" : "false",
            "number" => (NumberValue ?? 0).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            "enum" => EnumValue ?? string.Empty,
            "weighted_tag" => string.Join(",", WeightedTags.OrderBy(item => item.Tag, StringComparer.Ordinal).Select(item => $"{item.Tag}:{item.Weight:0.###}")),
            "relation" => RelationValue == null ? string.Empty : $"{RelationValue.RelationKind}:{RelationValue.TargetScope}:{RelationValue.TargetId}:{RelationValue.Strength:0.###}",
            "text_key" => TextKeyValue ?? string.Empty,
            "list" => string.Join(",", ListValues.Order(StringComparer.Ordinal)),
            _ => string.Empty
        };
}

public sealed record DynamicSemanticWeightedTag
{
    public string Tag { get; init; } = string.Empty;
    public double Weight { get; init; } = 1;
}

public sealed record DynamicSemanticRelationValue
{
    public string RelationKind { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public double Strength { get; init; } = 1;
}

public sealed record DynamicSemanticTargetNode
{
    public string TargetId { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public IReadOnlyList<string> ParentTargetIds { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
}

public sealed record DynamicSemanticInfluenceRule
{
    public string RuleId { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public string TargetFamily { get; init; } = string.Empty;
    public IReadOnlyList<DynamicSemanticConditionClause> Conditions { get; init; } = [];
    public IReadOnlyList<DynamicSemanticInfluenceEffect> Effects { get; init; } = [];
    public double Weight { get; init; } = 1;
    public int Priority { get; init; }
    public string TieBreaker { get; init; } = string.Empty;
    public string Status { get; init; } = "ready";
    public string Provenance { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
}

public sealed record DynamicSemanticConditionClause
{
    public string Operator { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public double? NumberValue { get; init; }
    public string Tag { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
}

public sealed record DynamicSemanticInfluenceEffect
{
    public string EffectKind { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public DynamicSemanticFeatureValue? Value { get; init; }
    public double NumberDelta { get; init; }
    public string IntentId { get; init; } = string.Empty;
    public string Suggestion { get; init; } = string.Empty;
    public string DiagnosticCode { get; init; } = string.Empty;
    public string DiagnosticMessage { get; init; } = string.Empty;
}

public sealed record DynamicSemanticResolveRequest
{
    public IReadOnlyList<DynamicSemanticFeatureDefinition> FeatureDefinitions { get; init; } = [];
    public IReadOnlyList<DynamicSemanticFeatureAssignment> Assignments { get; init; } = [];
    public IReadOnlyList<DynamicSemanticInfluenceRule> InfluenceRules { get; init; } = [];
    public IReadOnlyList<DynamicSemanticTargetNode> Targets { get; init; } = [];
    public IReadOnlyList<string> TargetIds { get; init; } = [];
    public string ProfileId { get; init; } = string.Empty;
    public int Seed { get; init; }
}

public sealed record DynamicSemanticScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public int Seed { get; init; }
    public IReadOnlyList<DynamicSemanticTargetNode> Targets { get; init; } = [];
    public IReadOnlyList<DynamicSemanticFeatureAssignment> Assignments { get; init; } = [];
    public IReadOnlyList<DynamicSemanticInfluenceRule> InfluenceRules { get; init; } = [];
    public IReadOnlyList<string> ResolveTargetIds { get; init; } = [];
}

public sealed record DynamicSemanticResolvedScenarioState
{
    public string SchemaVersion { get; init; } = "dynamic_semantic_resolved_state_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public int Seed { get; init; }
    public IReadOnlyList<DynamicSemanticResolvedTargetState> TargetStates { get; init; } = [];
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
    public IReadOnlyList<string> AuthoringSuggestions { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record DynamicSemanticResolvedTargetState
{
    public string TargetId { get; init; } = string.Empty;
    public string TargetScope { get; init; } = string.Empty;
    public IReadOnlyList<DynamicSemanticResolvedFeature> Features { get; init; } = [];
    public IReadOnlyList<DynamicSemanticResolutionTrace> Traces { get; init; } = [];
    public IReadOnlyList<DynamicSemanticInfluenceTrace> InfluenceEffects { get; init; } = [];
    public IReadOnlyList<string> AuthoringSuggestions { get; init; } = [];
    public IReadOnlyList<DynamicSemanticDiagnostic> Diagnostics { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record DynamicSemanticResolvedFeature
{
    public string FeatureId { get; init; } = string.Empty;
    public string ValueKind { get; init; } = string.Empty;
    public DynamicSemanticFeatureValue? Value { get; init; }
    public string ResolutionSource { get; init; } = string.Empty;
    public string SourceTargetId { get; init; } = string.Empty;
    public string SourceLayer { get; init; } = string.Empty;
    public bool Inherited { get; init; }
    public bool Defaulted { get; init; }
    public bool Manual { get; init; }
    public bool Generated { get; init; }
    public bool Blocked { get; init; }
}

public sealed record DynamicSemanticResolutionTrace
{
    public string FeatureId { get; init; } = string.Empty;
    public string TraceKind { get; init; } = string.Empty;
    public string SourceTargetId { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public sealed record DynamicSemanticInfluenceTrace
{
    public string RuleId { get; init; } = string.Empty;
    public string EffectKind { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}

public sealed record DynamicSemanticAuthoringField
{
    public string FeatureGroup { get; init; } = string.Empty;
    public string FieldKind { get; init; } = string.Empty;
    public string LabelKey { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public IReadOnlyList<string> OptionList { get; init; } = [];
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public string RequirementStatus { get; init; } = string.Empty;
    public bool Applicable { get; init; }
    public DynamicSemanticFeatureValue? InheritedValue { get; init; }
    public bool CanOverride { get; init; }
    public DynamicSemanticFeatureValue? SuggestedDefault { get; init; }
    public IReadOnlyList<string> DiagnosticLinks { get; init; } = [];
    public IReadOnlyList<string> SafeEditorHints { get; init; } = [];
}

public sealed record DynamicSemanticAuthoringSchemaMatrix
{
    public string SchemaVersion { get; init; } = "dynamic_semantic_authoring_schema_matrix_v1";
    public IReadOnlyList<DynamicSemanticAuthoringField> Fields { get; init; } = [];
}

public sealed record DynamicSemanticDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
