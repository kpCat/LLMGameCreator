namespace LLMGameCreator.Application.Design.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureResolver
{
    public DynamicSemanticResolvedScenarioState ResolveScenario(DynamicSemanticScenario scenario, IReadOnlyList<DynamicSemanticFeatureDefinition>? definitions = null)
    {
        var request = new DynamicSemanticResolveRequest
        {
            FeatureDefinitions = definitions ?? DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions(),
            Assignments = scenario.Assignments,
            InfluenceRules = scenario.InfluenceRules,
            Targets = scenario.Targets,
            TargetIds = scenario.ResolveTargetIds,
            ProfileId = scenario.ProfileId,
            Seed = scenario.Seed
        };

        var state = Resolve(request);
        return state with
        {
            ScenarioId = scenario.ScenarioId
        };
    }

    public DynamicSemanticResolvedScenarioState Resolve(DynamicSemanticResolveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestDiagnostics = DynamicSemanticFeatureValidator.ValidateRequest(request).ToList();
        var targetsById = request.Targets
            .GroupBy(item => item.TargetId, StringComparer.Ordinal)
            .Where(item => item.Count() == 1)
            .ToDictionary(item => item.Key, item => item.Single(), StringComparer.Ordinal);
        var targetIds = request.TargetIds.Count == 0
            ? request.Targets.Select(item => item.TargetId).Order(StringComparer.Ordinal).ToList()
            : request.TargetIds.Order(StringComparer.Ordinal).ToList();
        var states = new List<DynamicSemanticResolvedTargetState>();

        foreach (var targetId in targetIds)
        {
            if (!targetsById.TryGetValue(targetId, out var target))
            {
                requestDiagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.target.unknown", targetId, "Resolve target id is not present in the context."));
                continue;
            }

            states.Add(ResolveTarget(target, request, targetsById));
        }

        var diagnostics = DynamicSemanticFeatureValidator.SortDiagnostics(requestDiagnostics.Concat(states.SelectMany(item => item.Diagnostics)));
        return new DynamicSemanticResolvedScenarioState
        {
            ProfileId = request.ProfileId,
            Seed = request.Seed,
            TargetStates = states.OrderBy(item => item.TargetId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics,
            AuthoringSuggestions = states.SelectMany(item => item.AuthoringSuggestions).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            StableSummary = $"{request.ProfileId}|seed={request.Seed}|targets={states.Count}|features={states.Sum(item => item.Features.Count)}|diagnostics={diagnostics.Count(item => item.Severity == "error")}"
        };
    }

    private static DynamicSemanticResolvedTargetState ResolveTarget(
        DynamicSemanticTargetNode target,
        DynamicSemanticResolveRequest request,
        IReadOnlyDictionary<string, DynamicSemanticTargetNode> targetsById)
    {
        var diagnostics = new List<DynamicSemanticDiagnostic>();
        var traces = new List<DynamicSemanticResolutionTrace>();
        var influenceTraces = new List<DynamicSemanticInfluenceTrace>();
        var suggestions = new List<string>();
        var features = new Dictionary<string, DynamicSemanticResolvedFeature>(StringComparer.Ordinal);
        var ancestorIds = Ancestors(target, targetsById).Select(item => item.TargetId).ToList();
        var sourceTargetIds = ancestorIds.Append(target.TargetId).ToHashSet(StringComparer.Ordinal);
        var assignments = request.Assignments
            .Where(item => sourceTargetIds.Contains(item.TargetId))
            .OrderBy(item => ancestorIds.IndexOf(item.TargetId) < 0 ? ancestorIds.Count : ancestorIds.IndexOf(item.TargetId))
            .ThenBy(item => item.Priority)
            .ThenBy(item => item.FeatureId, StringComparer.Ordinal)
            .ThenBy(item => item.TargetId, StringComparer.Ordinal)
            .ToList();
        var definitionsById = request.FeatureDefinitions
            .GroupBy(item => item.FeatureId, StringComparer.Ordinal)
            .Where(item => item.Count() == 1)
            .ToDictionary(item => item.Key, item => item.Single(), StringComparer.Ordinal);

        foreach (var definition in request.FeatureDefinitions
                     .OrderBy(item => item.ApplicabilityConditions.Count)
                     .ThenBy(item => item.TargetScope, StringComparer.Ordinal)
                     .ThenBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            var matchingAssignments = assignments
                .Where(item => item.FeatureId == definition.FeatureId)
                .Where(item => item.TargetScope == definition.TargetScope || definition.InheritedSourceScopes.Contains(item.TargetScope, StringComparer.Ordinal))
                .ToList();
            var appliesToTarget = definition.TargetScope == target.TargetScope;
            var applicable = appliesToTarget && DynamicSemanticFeatureValidator.IsFeatureApplicable(definition, target, features);

            if (matchingAssignments.Count > 0)
            {
                var selected = matchingAssignments.Last();
                var inherited = selected.TargetId != target.TargetId;
                features[definition.FeatureId] = new DynamicSemanticResolvedFeature
                {
                    FeatureId = definition.FeatureId,
                    ValueKind = definition.ValueKind,
                    Value = selected.Value,
                    ResolutionSource = inherited ? "inherited" : selected.SourceLayer,
                    SourceTargetId = selected.TargetId,
                    SourceLayer = selected.SourceLayer,
                    Inherited = inherited,
                    Defaulted = false,
                    Manual = selected.SourceLayer == "manual_override" || selected.SourceLayer == "instance",
                    Generated = selected.SourceLayer == "generated_default"
                };
                traces.Add(Trace(definition.FeatureId, inherited ? "inherited_value" : "assigned_value", selected.TargetId, selected.Value.StableValueKey()));

                if (matchingAssignments.Count > 1)
                {
                    var overridden = matchingAssignments.Take(matchingAssignments.Count - 1).Select(item => item.TargetId).Distinct(StringComparer.Ordinal);
                    traces.Add(Trace(definition.FeatureId, "override", selected.TargetId, string.Join(",", overridden)));
                }

                continue;
            }

            if (applicable && definition.DefaultValue != null && definition.DefaultStrategy != "none")
            {
                features[definition.FeatureId] = new DynamicSemanticResolvedFeature
                {
                    FeatureId = definition.FeatureId,
                    ValueKind = definition.ValueKind,
                    Value = definition.DefaultValue,
                    ResolutionSource = "default",
                    SourceTargetId = target.TargetId,
                    SourceLayer = "generated_default",
                    Defaulted = true,
                    Generated = true
                };
                traces.Add(Trace(definition.FeatureId, "default_value", target.TargetId, definition.DefaultStrategy));
                continue;
            }

            if (applicable && definition.DefaultStrategy == "first_allowed" && definition.AllowedValues.Count > 0)
            {
                var value = DynamicSemanticFeatureCatalog.Enum(definition.AllowedValues.Order(StringComparer.Ordinal).First());
                features[definition.FeatureId] = new DynamicSemanticResolvedFeature
                {
                    FeatureId = definition.FeatureId,
                    ValueKind = definition.ValueKind,
                    Value = value,
                    ResolutionSource = "default",
                    SourceTargetId = target.TargetId,
                    SourceLayer = "generated_default",
                    Defaulted = true,
                    Generated = true
                };
                traces.Add(Trace(definition.FeatureId, "default_value", target.TargetId, definition.DefaultStrategy));
                continue;
            }

            if (appliesToTarget && definition.RequiredMode == "required" && applicable)
            {
                diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.required_feature.missing", $"{target.TargetId}:{definition.FeatureId}", "Required feature is applicable but missing."));
                traces.Add(Trace(definition.FeatureId, "missing_required", target.TargetId, "diagnostic"));
            }
            else if (appliesToTarget)
            {
                traces.Add(Trace(definition.FeatureId, applicable ? "absent_optional" : "inapplicable_optional", target.TargetId, definition.RequiredMode));
            }
        }

        ApplyInfluenceRules(target, request, definitionsById, features, influenceTraces, diagnostics, suggestions);
        ValidateResolvedConstraints(target, request.FeatureDefinitions, features, diagnostics);

        var resolved = features.Values
            .OrderBy(item => item.FeatureId, StringComparer.Ordinal)
            .ToList();
        return new DynamicSemanticResolvedTargetState
        {
            TargetId = target.TargetId,
            TargetScope = target.TargetScope,
            Features = resolved,
            Traces = traces.OrderBy(item => item.FeatureId, StringComparer.Ordinal).ThenBy(item => item.TraceKind, StringComparer.Ordinal).ThenBy(item => item.SourceTargetId, StringComparer.Ordinal).ToList(),
            InfluenceEffects = influenceTraces.OrderBy(item => item.RuleId, StringComparer.Ordinal).ThenBy(item => item.EffectKind, StringComparer.Ordinal).ThenBy(item => item.FeatureId, StringComparer.Ordinal).ToList(),
            AuthoringSuggestions = suggestions.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = DynamicSemanticFeatureValidator.SortDiagnostics(diagnostics),
            StableSummary = $"{target.TargetId}|scope={target.TargetScope}|features={resolved.Count}|effects={influenceTraces.Count}|diagnostics={diagnostics.Count(item => item.Severity == "error")}"
        };
    }

    private static void ApplyInfluenceRules(
        DynamicSemanticTargetNode target,
        DynamicSemanticResolveRequest request,
        IReadOnlyDictionary<string, DynamicSemanticFeatureDefinition> definitionsById,
        Dictionary<string, DynamicSemanticResolvedFeature> features,
        ICollection<DynamicSemanticInfluenceTrace> influenceTraces,
        ICollection<DynamicSemanticDiagnostic> diagnostics,
        ICollection<string> suggestions)
    {
        foreach (var rule in request.InfluenceRules
                     .Where(item => item.TargetScope == target.TargetScope || target.FamilyIds.Contains(item.TargetFamily, StringComparer.Ordinal))
                     .OrderBy(item => item.Priority)
                     .ThenBy(item => item.TieBreaker, StringComparer.Ordinal)
                     .ThenBy(item => item.RuleId, StringComparer.Ordinal))
        {
            if (rule.Conditions.Any(condition => !DynamicSemanticFeatureValidator.EvaluateCondition(condition, target, features)))
            {
                continue;
            }

            foreach (var effect in rule.Effects.OrderBy(item => item.EffectKind, StringComparer.Ordinal).ThenBy(item => item.FeatureId, StringComparer.Ordinal))
            {
                ApplyEffect(target, rule, effect, definitionsById, features, influenceTraces, diagnostics, suggestions);
            }
        }
    }

    private static void ApplyEffect(
        DynamicSemanticTargetNode target,
        DynamicSemanticInfluenceRule rule,
        DynamicSemanticInfluenceEffect effect,
        IReadOnlyDictionary<string, DynamicSemanticFeatureDefinition> definitionsById,
        Dictionary<string, DynamicSemanticResolvedFeature> features,
        ICollection<DynamicSemanticInfluenceTrace> influenceTraces,
        ICollection<DynamicSemanticDiagnostic> diagnostics,
        ICollection<string> suggestions)
    {
        switch (effect.EffectKind)
        {
            case "set_feature":
                if (effect.Value == null || !definitionsById.TryGetValue(effect.FeatureId, out var setDefinition))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.influence.target.unknown", $"{rule.RuleId}:{effect.FeatureId}", "Influence set_feature target must be known."));
                    return;
                }

                DynamicSemanticFeatureValidator.ValidateValueShape(setDefinition, effect.Value, diagnostics, $"{rule.RuleId}:{effect.FeatureId}");
                features[effect.FeatureId] = Resolved(effect.FeatureId, setDefinition.ValueKind, effect.Value, "influence", target.TargetId);
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, effect.Value.StableValueKey()));
                break;
            case "adjust_number":
                if (!definitionsById.TryGetValue(effect.FeatureId, out var numberDefinition))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.influence.target.unknown", $"{rule.RuleId}:{effect.FeatureId}", "Influence adjust_number target must be known."));
                    return;
                }

                var oldNumber = features.TryGetValue(effect.FeatureId, out var existingNumber) ? existingNumber.Value?.NumberValue ?? 0 : numberDefinition.DefaultValue?.NumberValue ?? 0;
                var adjusted = Math.Clamp(oldNumber + effect.NumberDelta, numberDefinition.MinValue ?? double.MinValue, numberDefinition.MaxValue ?? double.MaxValue);
                var adjustedValue = DynamicSemanticFeatureCatalog.Number(adjusted);
                features[effect.FeatureId] = Resolved(effect.FeatureId, numberDefinition.ValueKind, adjustedValue, "influence", target.TargetId);
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, $"{oldNumber:0.###}->{adjusted:0.###}"));
                break;
            case "add_weighted_tag":
                if (!definitionsById.TryGetValue(effect.FeatureId, out var tagDefinition))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.influence.target.unknown", $"{rule.RuleId}:{effect.FeatureId}", "Influence weighted tag target must be known."));
                    return;
                }

                var existingTags = features.TryGetValue(effect.FeatureId, out var tagFeature)
                    ? tagFeature.Value?.WeightedTags ?? []
                    : [];
                var mergedTags = existingTags.Concat(effect.Value?.WeightedTags ?? [])
                    .GroupBy(item => item.Tag, StringComparer.Ordinal)
                    .Select(group => new DynamicSemanticWeightedTag { Tag = group.Key, Weight = group.Sum(item => item.Weight) })
                    .OrderBy(item => item.Tag, StringComparer.Ordinal)
                    .ToList();
                var weightedValue = new DynamicSemanticFeatureValue { ValueKind = "weighted_tag", WeightedTags = mergedTags };
                features[effect.FeatureId] = Resolved(effect.FeatureId, tagDefinition.ValueKind, weightedValue, "influence", target.TargetId);
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, weightedValue.StableValueKey()));
                break;
            case "add_relation":
                if (effect.Value?.RelationValue == null || !definitionsById.TryGetValue(effect.FeatureId, out var relationDefinition))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.influence.target.unknown", $"{rule.RuleId}:{effect.FeatureId}", "Influence relation target must be known."));
                    return;
                }

                features[effect.FeatureId] = Resolved(effect.FeatureId, relationDefinition.ValueKind, effect.Value, "influence", target.TargetId);
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, effect.Value.StableValueKey()));
                break;
            case "add_intent":
                var intentFeatureId = string.IsNullOrWhiteSpace(effect.FeatureId) ? $"{target.TargetScope}.intent" : effect.FeatureId;
                var existing = features.TryGetValue(intentFeatureId, out var intentFeature)
                    ? intentFeature.Value?.ListValues ?? []
                    : [];
                var newIntents = effect.Value?.ListValues.Count > 0 ? effect.Value.ListValues : [effect.IntentId];
                var intentValue = DynamicSemanticFeatureCatalog.List([.. existing.Concat(newIntents).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)]);
                features[intentFeatureId] = Resolved(intentFeatureId, "list", intentValue, "influence", target.TargetId);
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, intentFeatureId, intentValue.StableValueKey()));
                break;
            case "block_feature":
                if (features.TryGetValue(effect.FeatureId, out var blocked))
                {
                    features[effect.FeatureId] = blocked with { Blocked = true, ResolutionSource = "blocked" };
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.output.overconstrained", $"{target.TargetId}:{effect.FeatureId}", "Influence blocked a feature that already had a value."));
                }

                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, "blocked"));
                break;
            case "raise_diagnostic":
                diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("warning", string.IsNullOrWhiteSpace(effect.DiagnosticCode) ? "dynamic_semantic.influence.diagnostic" : effect.DiagnosticCode, target.TargetId, effect.DiagnosticMessage));
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, effect.DiagnosticCode));
                break;
            case "suggest_feature":
                suggestions.Add($"{target.TargetId}:{effect.FeatureId}:{effect.Suggestion}");
                influenceTraces.Add(Influence(rule.RuleId, effect.EffectKind, effect.FeatureId, effect.Suggestion));
                break;
        }
    }

    private static void ValidateResolvedConstraints(
        DynamicSemanticTargetNode target,
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        IReadOnlyDictionary<string, DynamicSemanticResolvedFeature> features,
        ICollection<DynamicSemanticDiagnostic> diagnostics)
    {
        foreach (var definition in definitions.Where(item => item.TargetScope == target.TargetScope).OrderBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            if (!features.ContainsKey(definition.FeatureId))
            {
                continue;
            }

            foreach (var required in definition.Requires.Order(StringComparer.Ordinal))
            {
                if (!features.ContainsKey(required))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.feature.requires.missing", $"{target.TargetId}:{definition.FeatureId}->{required}", "Resolved feature requires another feature that is absent."));
                }
            }

            foreach (var conflict in definition.Conflicts.Order(StringComparer.Ordinal))
            {
                if (features.ContainsKey(conflict))
                {
                    diagnostics.Add(DynamicSemanticFeatureValidator.Diagnostic("error", "dynamic_semantic.feature.conflict", $"{target.TargetId}:{definition.FeatureId}->{conflict}", "Resolved features conflict."));
                }
            }
        }
    }

    private static IReadOnlyList<DynamicSemanticTargetNode> Ancestors(
        DynamicSemanticTargetNode target,
        IReadOnlyDictionary<string, DynamicSemanticTargetNode> targetsById)
    {
        var result = new List<DynamicSemanticTargetNode>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        Visit(target);
        return result
            .DistinctBy(item => item.TargetId)
            .Reverse<DynamicSemanticTargetNode>()
            .ToList();

        void Visit(DynamicSemanticTargetNode node)
        {
            foreach (var parentId in node.ParentTargetIds.Order(StringComparer.Ordinal))
            {
                if (visited.Add(parentId) && targetsById.TryGetValue(parentId, out var parent))
                {
                    result.Add(parent);
                    Visit(parent);
                }
            }
        }
    }

    private static DynamicSemanticResolvedFeature Resolved(
        string featureId,
        string valueKind,
        DynamicSemanticFeatureValue value,
        string source,
        string targetId) =>
        new()
        {
            FeatureId = featureId,
            ValueKind = valueKind,
            Value = value,
            ResolutionSource = source,
            SourceTargetId = targetId,
            SourceLayer = source,
            Generated = source == "influence"
        };

    private static DynamicSemanticResolutionTrace Trace(string featureId, string kind, string source, string detail) =>
        new()
        {
            FeatureId = featureId,
            TraceKind = kind,
            SourceTargetId = source,
            Detail = detail
        };

    private static DynamicSemanticInfluenceTrace Influence(string ruleId, string kind, string featureId, string detail) =>
        new()
        {
            RuleId = ruleId,
            EffectKind = kind,
            FeatureId = featureId,
            Detail = detail
        };
}

public sealed class DynamicSemanticAuthoringSchemaPlanner
{
    public DynamicSemanticAuthoringSchemaMatrix Build(
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions,
        IReadOnlyList<DynamicSemanticResolvedScenarioState> states)
    {
        var fields = new List<DynamicSemanticAuthoringField>();
        foreach (var state in states.OrderBy(item => item.ProfileId, StringComparer.Ordinal))
        {
            foreach (var target in state.TargetStates.OrderBy(item => item.TargetId, StringComparer.Ordinal))
            {
                var featureById = target.Features.ToDictionary(item => item.FeatureId, StringComparer.Ordinal);
                foreach (var definition in definitions
                             .Where(item => item.TargetScope == target.TargetScope || featureById.TryGetValue(item.FeatureId, out var inherited) && inherited.Inherited)
                             .OrderBy(item => item.AuthoringGroup, StringComparer.Ordinal)
                             .ThenBy(item => item.FeatureId, StringComparer.Ordinal))
                {
                    featureById.TryGetValue(definition.FeatureId, out var resolved);
                    var fieldDiagnostics = target.Diagnostics
                        .Where(item => item.Target.Contains(definition.FeatureId, StringComparison.Ordinal))
                        .Select(item => item.Code)
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToList();
                    fields.Add(new DynamicSemanticAuthoringField
                    {
                        FeatureGroup = definition.AuthoringGroup,
                        FieldKind = definition.ValueKind,
                        LabelKey = $"{target.TargetScope}.{definition.FeatureId}",
                        FeatureId = definition.FeatureId,
                        OptionList = definition.AllowedValues.Order(StringComparer.Ordinal).ToList(),
                        MinValue = definition.MinValue,
                        MaxValue = definition.MaxValue,
                        RequirementStatus = definition.RequiredMode,
                        Applicable = resolved != null || !fieldDiagnostics.Contains("dynamic_semantic.required_feature.missing"),
                        InheritedValue = resolved?.Inherited == true ? resolved.Value : null,
                        CanOverride = definition.InheritanceMode != "locked",
                        SuggestedDefault = definition.DefaultValue,
                        DiagnosticLinks = fieldDiagnostics,
                        SafeEditorHints = SafeHints(definition)
                    });
                }
            }
        }

        return new DynamicSemanticAuthoringSchemaMatrix
        {
            Fields = fields
                .OrderBy(item => item.FeatureGroup, StringComparer.Ordinal)
                .ThenBy(item => item.LabelKey, StringComparer.Ordinal)
                .ThenBy(item => item.FeatureId, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static IReadOnlyList<string> SafeHints(DynamicSemanticFeatureDefinition definition)
    {
        var hints = new List<string> { "application_layer_contract_only", "no_expression_evaluation" };
        if (definition.AllowedValues.Count > 0)
        {
            hints.Add("use_declared_options");
        }

        if (definition.MinValue.HasValue || definition.MaxValue.HasValue)
        {
            hints.Add("respect_numeric_bounds");
        }

        if (definition.InheritanceMode != "none")
        {
            hints.Add("show_inherited_value_before_override");
        }

        return hints.Order(StringComparer.Ordinal).ToList();
    }
}
