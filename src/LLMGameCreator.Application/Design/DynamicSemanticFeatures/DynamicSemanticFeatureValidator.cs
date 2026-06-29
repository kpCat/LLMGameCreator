using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.DynamicSemanticFeatures;

public static partial class DynamicSemanticFeatureValidator
{
    private static readonly IReadOnlySet<string> ValidConditionOperators = new HashSet<string>(
        ["feature_exists", "feature_missing", "enum_equals", "number_at_least", "number_at_most", "tag_contains", "relation_exists", "scope_is", "target_has_tag"],
        StringComparer.Ordinal);

    private static readonly IReadOnlySet<string> ValidEffectKinds = new HashSet<string>(
        ["set_feature", "adjust_number", "add_weighted_tag", "add_relation", "add_intent", "block_feature", "raise_diagnostic", "suggest_feature"],
        StringComparer.Ordinal);

    private static readonly string[] LeakageNeedles =
    [
        "runtime",
        "winforms",
        "winforms_ui",
        "ui mutation",
        "unity",
        "provider",
        "llm",
        "rag",
        "lua",
        "gamepackage schema",
        "gamepackage_schema"
    ];

    public static IReadOnlyList<DynamicSemanticDiagnostic> ValidateCatalog(
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        IReadOnlyList<DynamicSemanticInfluenceRule> rules)
    {
        var diagnostics = new List<DynamicSemanticDiagnostic>();
        var definitionsById = definitions
            .Where(item => !string.IsNullOrWhiteSpace(item.FeatureId))
            .GroupBy(item => item.FeatureId, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.ToList(), StringComparer.Ordinal);

        foreach (var definition in definitions.OrderBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            ValidateDefinition(definition, definitionsById.Keys.ToHashSet(StringComparer.Ordinal), diagnostics);
        }

        foreach (var duplicate in definitionsById.Where(item => item.Value.Count > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_id.duplicate", duplicate.Key, "Feature ids must be unique."));
        }

        foreach (var rule in rules.OrderBy(item => item.RuleId, StringComparer.Ordinal))
        {
            ValidateRule(rule, definitionsById.Keys.ToHashSet(StringComparer.Ordinal), diagnostics);
        }

        diagnostics.AddRange(DetectInfluenceCycles(rules));
        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<DynamicSemanticDiagnostic> ValidateRequest(DynamicSemanticResolveRequest request)
    {
        var diagnostics = ValidateCatalog(request.FeatureDefinitions, request.InfluenceRules).ToList();
        var definitionsById = request.FeatureDefinitions
            .GroupBy(item => item.FeatureId, StringComparer.Ordinal)
            .Where(item => item.Count() == 1)
            .ToDictionary(item => item.Key, item => item.Single(), StringComparer.Ordinal);
        var targetById = request.Targets
            .GroupBy(item => item.TargetId, StringComparer.Ordinal)
            .Where(item => !string.IsNullOrWhiteSpace(item.Key) && item.Count() == 1)
            .ToDictionary(item => item.Key, item => item.Single(), StringComparer.Ordinal);

        foreach (var target in request.Targets.OrderBy(item => item.TargetId, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(target.TargetId) || !StableIdPattern().IsMatch(target.TargetId))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.target_id.invalid", target.TargetId, "Target id must be stable."));
            }

            if (!DynamicSemanticFeatureVocabulary.ValidScopes.Contains(target.TargetScope))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", target.TargetId, "Target scope is not part of the Goal 032 scope vocabulary."));
            }

            foreach (var parent in target.ParentTargetIds.Order(StringComparer.Ordinal))
            {
                if (!targetById.ContainsKey(parent))
                {
                    diagnostics.Add(Diagnostic("error", "dynamic_semantic.inheritance.source.unknown", $"{target.TargetId}->{parent}", "Hierarchy parent must reference a known target."));
                }
            }
        }

        foreach (var assignment in request.Assignments.OrderBy(item => item.TargetId, StringComparer.Ordinal).ThenBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            ValidateAssignment(assignment, definitionsById, targetById, diagnostics);
        }

        diagnostics.AddRange(DetectTargetCycles(request.Targets));
        return SortDiagnostics(diagnostics);
    }

    public static DynamicSemanticDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<DynamicSemanticDiagnostic> SortDiagnostics(IEnumerable<DynamicSemanticDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static bool IsFeatureApplicable(
        DynamicSemanticFeatureDefinition definition,
        DynamicSemanticTargetNode target,
        IReadOnlyDictionary<string, DynamicSemanticResolvedFeature> existingFeatures)
    {
        if (definition.TargetScope != target.TargetScope)
        {
            return false;
        }

        foreach (var condition in definition.ApplicabilityConditions)
        {
            if (!EvaluateCondition(condition, target, existingFeatures))
            {
                return false;
            }
        }

        return true;
    }

    public static bool EvaluateCondition(
        DynamicSemanticConditionClause condition,
        DynamicSemanticTargetNode target,
        IReadOnlyDictionary<string, DynamicSemanticResolvedFeature> features)
    {
        return condition.Operator switch
        {
            "feature_exists" => features.TryGetValue(condition.FeatureId, out var exists) && exists.Value != null && !exists.Blocked,
            "feature_missing" => !features.TryGetValue(condition.FeatureId, out var missing) || missing.Value == null || missing.Blocked,
            "enum_equals" => features.TryGetValue(condition.FeatureId, out var enumFeature)
                && string.Equals(enumFeature.Value?.EnumValue, condition.ExpectedValue, StringComparison.Ordinal),
            "number_at_least" => features.TryGetValue(condition.FeatureId, out var minFeature)
                && minFeature.Value?.NumberValue >= condition.NumberValue,
            "number_at_most" => features.TryGetValue(condition.FeatureId, out var maxFeature)
                && maxFeature.Value?.NumberValue <= condition.NumberValue,
            "tag_contains" => features.TryGetValue(condition.FeatureId, out var tagFeature)
                && tagFeature.Value?.WeightedTags.Any(item => item.Tag == condition.Tag) == true,
            "relation_exists" => features.TryGetValue(condition.FeatureId, out var relationFeature)
                && relationFeature.Value?.RelationValue != null
                && (string.IsNullOrWhiteSpace(condition.RelationKind) || relationFeature.Value.RelationValue.RelationKind == condition.RelationKind),
            "scope_is" => target.TargetScope == condition.Scope,
            "target_has_tag" => target.Tags.Contains(condition.Tag, StringComparer.Ordinal)
                || target.FamilyIds.Contains(condition.Tag, StringComparer.Ordinal),
            _ => false
        };
    }

    public static bool ValidateValueShape(
        DynamicSemanticFeatureDefinition definition,
        DynamicSemanticFeatureValue value,
        ICollection<DynamicSemanticDiagnostic> diagnostics,
        string target)
    {
        var valid = true;
        if (value.ValueKind != definition.ValueKind)
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.value_shape.invalid", target, "Value kind must match the feature definition."));
            return false;
        }

        switch (definition.ValueKind)
        {
            case "flag":
                valid = value.FlagValue.HasValue;
                break;
            case "number":
                valid = value.NumberValue.HasValue
                    && (!definition.MinValue.HasValue || value.NumberValue.Value >= definition.MinValue.Value)
                    && (!definition.MaxValue.HasValue || value.NumberValue.Value <= definition.MaxValue.Value);
                break;
            case "enum":
                valid = !string.IsNullOrWhiteSpace(value.EnumValue)
                    && (definition.AllowedValues.Count == 0 || definition.AllowedValues.Contains(value.EnumValue, StringComparer.Ordinal));
                break;
            case "weighted_tag":
                valid = value.WeightedTags.Count > 0 && value.WeightedTags.All(item => !string.IsNullOrWhiteSpace(item.Tag));
                break;
            case "relation":
                valid = value.RelationValue != null
                    && !string.IsNullOrWhiteSpace(value.RelationValue.RelationKind)
                    && DynamicSemanticFeatureVocabulary.ValidScopes.Contains(value.RelationValue.TargetScope)
                    && !string.IsNullOrWhiteSpace(value.RelationValue.TargetId);
                break;
            case "text_key":
                valid = !string.IsNullOrWhiteSpace(value.TextKeyValue);
                break;
            case "list":
                valid = value.ListValues.Count > 0 && value.ListValues.All(item => !string.IsNullOrWhiteSpace(item));
                break;
        }

        if (!valid)
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.value_shape.invalid", target, "Feature value shape is invalid for the declared kind or bounds."));
        }

        return valid;
    }

    public static bool ContainsForbiddenLeakage(params string[] values)
    {
        var text = string.Join(" ", values).ToLowerInvariant();
        return LeakageNeedles.Any(text.Contains);
    }

    private static void ValidateDefinition(
        DynamicSemanticFeatureDefinition definition,
        IReadOnlySet<string> featureIds,
        ICollection<DynamicSemanticDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(definition.FeatureId) || !StableIdPattern().IsMatch(definition.FeatureId))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_id.invalid", definition.FeatureId, "Feature id must be stable and lowercase."));
        }

        if (!DynamicSemanticFeatureVocabulary.ValidScopes.Contains(definition.TargetScope))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", definition.FeatureId, "Feature target scope is unknown."));
        }

        if (!DynamicSemanticFeatureVocabulary.ValidValueKinds.Contains(definition.ValueKind))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.value_kind.unknown", definition.FeatureId, "Feature value kind is unknown."));
        }

        foreach (var sourceScope in definition.InheritedSourceScopes.Order(StringComparer.Ordinal))
        {
            if (!DynamicSemanticFeatureVocabulary.ValidScopes.Contains(sourceScope))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", $"{definition.FeatureId}:{sourceScope}", "Inherited source scope is unknown."));
            }
        }

        foreach (var required in definition.Requires.Order(StringComparer.Ordinal))
        {
            if (!featureIds.Contains(required))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_ref.unknown", $"{definition.FeatureId}->{required}", "Required feature reference must be known."));
            }
        }

        foreach (var conflict in definition.Conflicts.Order(StringComparer.Ordinal))
        {
            if (!featureIds.Contains(conflict))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_ref.unknown", $"{definition.FeatureId}->{conflict}", "Conflict feature reference must be known."));
            }
        }

        foreach (var condition in definition.ApplicabilityConditions)
        {
            ValidateCondition(condition, featureIds, definition.FeatureId, diagnostics);
        }

        if (definition.DefaultValue != null)
        {
            ValidateValueShape(definition, definition.DefaultValue, diagnostics, $"{definition.FeatureId}:default");
        }

        if (ContainsForbiddenLeakage(definition.Tags.ToArray()) || ContainsForbiddenLeakage(definition.Notes, definition.Provenance))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.boundary.leakage", definition.FeatureId, "Feature definition must not imply Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage schema work."));
        }
    }

    private static void ValidateAssignment(
        DynamicSemanticFeatureAssignment assignment,
        IReadOnlyDictionary<string, DynamicSemanticFeatureDefinition> definitionsById,
        IReadOnlyDictionary<string, DynamicSemanticTargetNode> targetById,
        ICollection<DynamicSemanticDiagnostic> diagnostics)
    {
        if (!targetById.TryGetValue(assignment.TargetId, out var target))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.target.unknown", assignment.TargetId, "Assignment target must exist in the hierarchy/context."));
        }

        if (!DynamicSemanticFeatureVocabulary.ValidScopes.Contains(assignment.TargetScope))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", $"{assignment.TargetId}:{assignment.TargetScope}", "Assignment scope is unknown."));
        }

        if (!definitionsById.TryGetValue(assignment.FeatureId, out var definition))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_ref.unknown", assignment.FeatureId, "Assignment feature id must reference a known feature definition."));
            return;
        }

        if (target != null && assignment.TargetScope != target.TargetScope)
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.assignment.scope_mismatch", $"{assignment.TargetId}:{assignment.FeatureId}", "Assignment scope must match the target node scope."));
        }

        var allowedScope = assignment.TargetScope == definition.TargetScope
            || definition.InheritedSourceScopes.Contains(assignment.TargetScope, StringComparer.Ordinal);
        if (!allowedScope)
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.assignment.scope_illegal", $"{assignment.TargetId}:{assignment.FeatureId}", "Feature assignment is illegal for the target scope."));
        }

        ValidateValueShape(definition, assignment.Value, diagnostics, $"{assignment.TargetId}:{assignment.FeatureId}");

        if (ContainsForbiddenLeakage(assignment.Provenance, assignment.SourceId, assignment.SourceLayer))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.boundary.leakage", $"{assignment.TargetId}:{assignment.FeatureId}", "Assignment must not imply forbidden boundary work."));
        }
    }

    private static void ValidateRule(
        DynamicSemanticInfluenceRule rule,
        IReadOnlySet<string> featureIds,
        ICollection<DynamicSemanticDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(rule.RuleId) || !StableIdPattern().IsMatch(rule.RuleId))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.influence.rule_id.invalid", rule.RuleId, "Influence rule id must be stable."));
        }

        if (!DynamicSemanticFeatureVocabulary.ValidScopes.Contains(rule.TargetScope))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", rule.RuleId, "Influence rule target scope is unknown."));
        }

        foreach (var condition in rule.Conditions)
        {
            ValidateCondition(condition, featureIds, rule.RuleId, diagnostics);
        }

        foreach (var effect in rule.Effects)
        {
            if (!ValidEffectKinds.Contains(effect.EffectKind))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.influence.effect.unknown", $"{rule.RuleId}:{effect.EffectKind}", "Influence effect kind is unknown."));
            }

            if (!string.IsNullOrWhiteSpace(effect.FeatureId) && !featureIds.Contains(effect.FeatureId))
            {
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.influence.target.unknown", $"{rule.RuleId}:{effect.FeatureId}", "Influence effect feature target must be known."));
            }
        }

        var conditionFeatures = rule.Conditions.Select(item => item.FeatureId).Where(item => !string.IsNullOrWhiteSpace(item)).ToHashSet(StringComparer.Ordinal);
        var effectFeatures = rule.Effects.Select(item => item.FeatureId).Where(item => !string.IsNullOrWhiteSpace(item)).ToHashSet(StringComparer.Ordinal);
        if (conditionFeatures.Overlaps(effectFeatures))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.influence.self_feeding", rule.RuleId, "Influence rule must not repeatedly feed the same feature it depends on."));
        }

        if (ContainsForbiddenLeakage(rule.Provenance, rule.Explanation, rule.TargetFamily))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.boundary.leakage", rule.RuleId, "Influence rule must not imply forbidden boundary work."));
        }
    }

    private static void ValidateCondition(
        DynamicSemanticConditionClause condition,
        IReadOnlySet<string> featureIds,
        string target,
        ICollection<DynamicSemanticDiagnostic> diagnostics)
    {
        if (!ValidConditionOperators.Contains(condition.Operator))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.condition.operator.unknown", $"{target}:{condition.Operator}", "Condition operator is unknown."));
        }

        if (!string.IsNullOrWhiteSpace(condition.FeatureId) && !featureIds.Contains(condition.FeatureId))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.feature_ref.unknown", $"{target}:{condition.FeatureId}", "Condition feature reference must be known."));
        }

        if (!string.IsNullOrWhiteSpace(condition.Scope) && !DynamicSemanticFeatureVocabulary.ValidScopes.Contains(condition.Scope))
        {
            diagnostics.Add(Diagnostic("error", "dynamic_semantic.scope.unknown", $"{target}:{condition.Scope}", "Condition scope is unknown."));
        }
    }

    private static IEnumerable<DynamicSemanticDiagnostic> DetectTargetCycles(IReadOnlyList<DynamicSemanticTargetNode> targets)
    {
        var diagnostics = new List<DynamicSemanticDiagnostic>();
        var byId = targets
            .GroupBy(item => item.TargetId, StringComparer.Ordinal)
            .Where(item => !string.IsNullOrWhiteSpace(item.Key) && item.Count() == 1)
            .ToDictionary(item => item.Key, item => item.Single(), StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var targetId in byId.Keys.Order(StringComparer.Ordinal))
        {
            Visit(targetId, []);
        }

        return diagnostics;

        void Visit(string targetId, IReadOnlyList<string> path)
        {
            if (visited.Contains(targetId))
            {
                return;
            }

            if (!visiting.Add(targetId))
            {
                var cycleStart = path.ToList().IndexOf(targetId);
                var cycle = cycleStart >= 0 ? path.Skip(cycleStart).Append(targetId) : path.Append(targetId);
                diagnostics.Add(Diagnostic("error", "dynamic_semantic.inheritance.circular", string.Join("->", cycle), "Target inheritance graph must be acyclic."));
                return;
            }

            foreach (var parent in byId[targetId].ParentTargetIds.Order(StringComparer.Ordinal))
            {
                if (byId.ContainsKey(parent))
                {
                    Visit(parent, path.Append(targetId).ToList());
                }
            }

            visiting.Remove(targetId);
            visited.Add(targetId);
        }
    }

    private static IEnumerable<DynamicSemanticDiagnostic> DetectInfluenceCycles(IReadOnlyList<DynamicSemanticInfluenceRule> rules)
    {
        var dependencies = rules
            .Select(rule => new
            {
                Rule = rule,
                Inputs = rule.Conditions.Select(item => item.FeatureId).Where(item => !string.IsNullOrWhiteSpace(item)).ToHashSet(StringComparer.Ordinal),
                Outputs = rule.Effects.Select(item => item.FeatureId).Where(item => !string.IsNullOrWhiteSpace(item)).ToHashSet(StringComparer.Ordinal)
            })
            .ToList();

        foreach (var first in dependencies.OrderBy(item => item.Rule.RuleId, StringComparer.Ordinal))
        {
            foreach (var second in dependencies.Where(item => string.CompareOrdinal(first.Rule.RuleId, item.Rule.RuleId) < 0))
            {
                if (first.Outputs.Overlaps(second.Inputs) && second.Outputs.Overlaps(first.Inputs))
                {
                    yield return Diagnostic("error", "dynamic_semantic.influence.circular", $"{first.Rule.RuleId}<->{second.Rule.RuleId}", "Influence rule feature dependencies must not form a circular feedback pair.");
                }
            }
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();
}
