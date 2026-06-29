using LLMGameCreator.Application.Design.DynamicSemanticFeatures;

namespace LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

public sealed class FeatureDrivenIntentResolver
{
    public SemanticAuthoringIntentResolution ResolveScenario(DynamicSemanticScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        var state = new DynamicSemanticFeatureResolver().ResolveScenario(scenario, definitions);
        var intents = new List<SemanticContentIntentRecord>();

        foreach (var target in state.TargetStates.OrderBy(item => item.TargetId, StringComparer.Ordinal))
        {
            intents.AddRange(BuildTargetIntents(scenario, target));
        }

        intents.AddRange(BuildScenarioWideIntents(scenario, state));

        var sorted = intents
            .GroupBy(item => item.IntentId, StringComparer.Ordinal)
            .Select(item => item.First())
            .OrderBy(item => item.IntentId, StringComparer.Ordinal)
            .ToList();
        var resolution = new SemanticAuthoringIntentResolution
        {
            ScenarioId = scenario.ScenarioId,
            ProfileId = scenario.ProfileId,
            Intents = sorted,
            StableSummary = $"{scenario.ScenarioId}|intents={sorted.Count}|families={sorted.Select(item => item.IntentFamily).Distinct(StringComparer.Ordinal).Count()}|features={sorted.SelectMany(item => item.SourceFeatureIds).Distinct(StringComparer.Ordinal).Count()}"
        };
        var diagnostics = SemanticAuthoringIntentValidator.ValidateIntentResolution(resolution);
        return resolution with { Diagnostics = diagnostics };
    }

    private static IEnumerable<SemanticContentIntentRecord> BuildTargetIntents(
        DynamicSemanticScenario scenario,
        DynamicSemanticResolvedTargetState target)
    {
        var features = target.Features.ToDictionary(item => item.FeatureId, StringComparer.Ordinal);
        var domain = ToDomain(target.TargetScope);

        if (target.TargetScope == "npc")
        {
            yield return Intent(scenario, target, "npc_role", ["world.theme", "npc.mood", "npc.trust", "npc.hunger"], 100, "intent.template.npc_role");
            yield return Intent(scenario, target, "relationship_pressure", ["npc.faction_relation", "npc.trust", "relationship.tension"], 110, "intent.template.relationship_pressure");
            if (!features.ContainsKey("npc.faction_relation"))
            {
                yield return Gap(scenario, target, "lore_gap", "npc.faction_relation", "optional_absent_npc_faction", 900);
            }
        }

        if (target.TargetScope == "quest")
        {
            yield return Intent(scenario, target, "quest_motive", ["quest.motive", "region.route_pressure", "kingdom.pressure", "world.theme"], 200, "intent.template.quest_motive");
        }

        if (target.TargetScope == "dialogue")
        {
            yield return Intent(scenario, target, "dialogue_act", ["dialogue.intent", "dialogue.text_key_hint", "faction.reputation_axis", "quest.motive"], 300, "intent.template.dialogue_act");
        }

        if (target.TargetScope == "event")
        {
            yield return Intent(scenario, target, "event_intent", ["event.intent", "event.is_volatile", "kingdom.pressure", "world.theme"], 400, "intent.template.event_intent");
        }

        if (target.TargetScope == "settlement")
        {
            yield return Intent(scenario, target, "economy_pressure", ["settlement.market_need", "resource.scarcity", "region.route_pressure"], 500, "intent.template.economy_pressure");
            yield return Intent(scenario, target, "settlement_need", ["settlement.market_need", "biome.hazard", "world.theme"], 510, "intent.template.settlement_need");
        }

        if (target.TargetScope == "species" || target.TargetScope == "archetype")
        {
            yield return Intent(scenario, target, "lore_gap", ["species.mana_resonance", "species.module_capacity", "archetype.forbidden_affinity"], 600, "intent.template.species_archetype_gap");
        }

        if (domain == "combat")
        {
            yield return Intent(scenario, target, "combat_pressure", ["combat.threat_level", "kingdom.pressure", "region.route_pressure"], 700, "intent.template.combat_pressure");
        }
    }

    private static IEnumerable<SemanticContentIntentRecord> BuildScenarioWideIntents(
        DynamicSemanticScenario scenario,
        DynamicSemanticResolvedScenarioState state)
    {
        foreach (var target in scenario.Targets.OrderBy(item => item.TargetId, StringComparer.Ordinal))
        {
            if (target.TargetScope == "faction")
            {
                yield return new SemanticContentIntentRecord
                {
                    IntentId = $"{scenario.ScenarioId}:faction_reaction:{SafeTail(target.TargetId)}",
                    IntentFamily = "faction_reaction",
                    TargetId = target.TargetId,
                    TargetDomain = "faction",
                    SourceFeatureIds = ["faction.reputation_axis", "world.theme"],
                    ResolvedFeatureValueSummary = FeatureSummary(state, ["faction.reputation_axis", "world.theme"]),
                    Priority = 250,
                    Weight = 0.65,
                    TemplateHint = "intent.template.faction_reaction",
                    LocalizationKeyHint = $"intent.{scenario.ScenarioId}.faction_reaction",
                    ProvenanceSummary = "semantic_pack+dynamic_feature_trace",
                    TraceSummary = "goal_031_pack_hint->goal_032_resolved_feature_state"
                };
            }

            if (target.TargetScope == "combat")
            {
                yield return new SemanticContentIntentRecord
                {
                    IntentId = $"{scenario.ScenarioId}:combat_pressure:{SafeTail(target.TargetId)}",
                    IntentFamily = "combat_pressure",
                    TargetId = target.TargetId,
                    TargetDomain = "combat",
                    SourceFeatureIds = ["combat.threat_level", "kingdom.pressure", "region.route_pressure"],
                    ResolvedFeatureValueSummary = FeatureSummary(state, ["combat.threat_level", "kingdom.pressure", "region.route_pressure"]),
                    Priority = 700,
                    Weight = 0.7,
                    TemplateHint = "intent.template.combat_pressure",
                    LocalizationKeyHint = $"intent.{scenario.ScenarioId}.combat_pressure",
                    BlockersOrGaps = FeatureSummary(state, ["combat.threat_level"]).Length == 0 ? ["combat_target_not_resolved_in_goal032_state"] : [],
                    ProvenanceSummary = "programmatic+dynamic_feature_trace",
                    TraceSummary = "scenario_target+goal_032_feature_assignments"
                };
            }
        }
    }

    private static SemanticContentIntentRecord Intent(
        DynamicSemanticScenario scenario,
        DynamicSemanticResolvedTargetState target,
        string family,
        IReadOnlyList<string> featureIds,
        int priority,
        string templateHint)
    {
        var present = target.Features
            .Where(item => featureIds.Contains(item.FeatureId, StringComparer.Ordinal))
            .OrderBy(item => item.FeatureId, StringComparer.Ordinal)
            .ToList();
        var sourceFeatures = present.Select(item => item.FeatureId).DefaultIfEmpty(featureIds[0]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var summary = string.Join(";", present.Select(item => $"{item.FeatureId}={item.Value?.StableValueKey() ?? "unset"}"));
        var gaps = featureIds.Except(sourceFeatures, StringComparer.Ordinal).Select(item => $"missing_or_unresolved:{item}").Order(StringComparer.Ordinal).ToList();
        return new SemanticContentIntentRecord
        {
            IntentId = $"{scenario.ScenarioId}:{family}:{SafeTail(target.TargetId)}",
            IntentFamily = family,
            TargetId = target.TargetId,
            TargetDomain = ToDomain(target.TargetScope),
            SourceFeatureIds = sourceFeatures,
            ResolvedFeatureValueSummary = summary,
            Priority = priority,
            Weight = ComputeWeight(present.Count, gaps.Count, priority),
            TemplateHint = templateHint,
            LocalizationKeyHint = $"intent.{scenario.ScenarioId}.{family}",
            BlockersOrGaps = gaps,
            ProvenanceSummary = ProvenanceSummary(present),
            TraceSummary = TraceSummary(target, sourceFeatures)
        };
    }

    private static SemanticContentIntentRecord Gap(
        DynamicSemanticScenario scenario,
        DynamicSemanticResolvedTargetState target,
        string family,
        string featureId,
        string gap,
        int priority) =>
        new()
        {
            IntentId = $"{scenario.ScenarioId}:{family}:{SafeTail(target.TargetId)}:{featureId.Replace(".", "_", StringComparison.Ordinal)}",
            IntentFamily = family,
            TargetId = target.TargetId,
            TargetDomain = ToDomain(target.TargetScope),
            SourceFeatureIds = [featureId],
            ResolvedFeatureValueSummary = "unset",
            Priority = priority,
            Weight = 0.1,
            TemplateHint = "intent.template.authoring_gap",
            LocalizationKeyHint = $"intent.{scenario.ScenarioId}.authoring_gap",
            BlockersOrGaps = [gap],
            ProvenanceSummary = "unset",
            TraceSummary = "optional_absence_trace"
        };

    private static string FeatureSummary(DynamicSemanticResolvedScenarioState state, IReadOnlyList<string> featureIds)
    {
        var values = state.TargetStates
            .SelectMany(item => item.Features)
            .Where(item => featureIds.Contains(item.FeatureId, StringComparer.Ordinal))
            .GroupBy(item => item.FeatureId, StringComparer.Ordinal)
            .Select(item => $"{item.Key}={item.First().Value?.StableValueKey() ?? "unset"}")
            .Order(StringComparer.Ordinal)
            .ToList();
        return string.Join(";", values);
    }

    private static string ProvenanceSummary(IReadOnlyList<DynamicSemanticResolvedFeature> features)
    {
        if (features.Count == 0)
        {
            return "unset";
        }

        return string.Join(
            "+",
            features
                .Select(item => item.Inherited ? "inherited" : item.Defaulted ? "programmatic" : item.Generated ? "programmatic" : item.Manual ? "user" : "semantic_pack")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
    }

    private static string TraceSummary(DynamicSemanticResolvedTargetState target, IReadOnlyList<string> featureIds)
    {
        var traceKinds = target.Traces
            .Where(item => featureIds.Contains(item.FeatureId, StringComparer.Ordinal))
            .Select(item => item.TraceKind)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var influenceKinds = target.InfluenceEffects
            .Where(item => featureIds.Contains(item.FeatureId, StringComparer.Ordinal))
            .Select(item => item.EffectKind)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        return string.Join("+", traceKinds.Concat(influenceKinds).DefaultIfEmpty("scenario_feature_trace"));
    }

    private static double ComputeWeight(int presentCount, int gapCount, int priority)
    {
        var raw = 0.25 + presentCount * 0.2 - gapCount * 0.05 + Math.Max(0, 800 - priority) / 4000.0;
        return Math.Round(Math.Clamp(raw, 0.05, 1.0), 3);
    }

    private static string ToDomain(string scope) =>
        scope switch
        {
            "resource" or "item" => "economy",
            "biome" => "region",
            "magic" or "relationship" => "world",
            _ => SemanticAuthoringIntentVocabulary.DomainGroups.Contains(scope, StringComparer.Ordinal) ? scope : "world"
        };

    private static string SafeTail(string id)
    {
        var index = id.LastIndexOf('/');
        return (index >= 0 ? id[(index + 1)..] : id).Replace(".", "_", StringComparison.Ordinal);
    }
}
