using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Procedural;

public sealed class FormulaEffectActionRegistryServiceTests
{
    [Fact]
    public void SameGeneratedPlanProducesByteIdenticalRulePackAndValidationReport()
    {
        var plan = CreatePlan();
        var service = new FormulaEffectActionRegistryService();
        var request = new FormulaEffectActionRegistryRequest { SourcePlan = plan };

        var first = service.Generate(request);
        var second = service.Generate(request);

        Assert.Equal(first.Json, second.Json);
        Assert.Equal(first.Markdown, second.Markdown);
        Assert.Equal(first.ValidationReportJson, second.ValidationReportJson);
        Assert.Equal(first.ValidationReportMarkdown, second.ValidationReportMarkdown);
        Assert.Equal(plan.PlanId, first.RulePack.Metadata.SourcePlanId);
        Assert.Equal(plan.Metadata.DeterministicHash, first.RulePack.Metadata.SourcePlanHash);
        Assert.Equal(4, first.RulePack.Formulas.Count);
        Assert.Equal(2, first.RulePack.Requirements.Count);
        Assert.NotEmpty(first.RulePack.Effects);
        Assert.Equal(2, first.RulePack.Actions.Count);
        Assert.Equal(3, first.RulePack.EventRules.Count);
        Assert.False(first.ValidationReport.HasErrors);
        Assert.Contains(first.Diagnostics, item => item.Code == "rule_pack.no_external_execution");
        Assert.Contains("Formula/Effect/Action Rule Pack v1", first.Markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedRuleReferencesPointToKnownRulesAndSourcePlanIds()
    {
        var plan = CreatePlan();
        var result = new FormulaEffectActionRegistryService().Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan });
        var formulaIds = result.RulePack.Formulas.Select(item => item.FormulaId).ToHashSet(StringComparer.Ordinal);
        var requirementIds = result.RulePack.Requirements.Select(item => item.RequirementId).ToHashSet(StringComparer.Ordinal);
        var effectIds = result.RulePack.Effects.Select(item => item.EffectId).ToHashSet(StringComparer.Ordinal);
        var actionIds = result.RulePack.Actions.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);
        var sourceIds = plan.World.Regions.Select(item => item.RegionId)
            .Concat(plan.World.Connections.Select(item => item.ConnectionId))
            .Concat(plan.Factions.Select(item => item.FactionId))
            .Concat(plan.ActorSeeds.Select(item => item.ActorSeedId))
            .Concat(plan.ItemResourceSeeds.Select(item => item.ItemSeedId))
            .Concat(plan.EncounterSeeds.Select(item => item.EncounterSeedId))
            .Concat(plan.QuestEventSeeds.Select(item => item.QuestEventSeedId))
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(result.RulePack.Requirements, item => Assert.Contains(item.FormulaId, formulaIds));
        Assert.All(result.RulePack.Effects.Where(item => !string.IsNullOrWhiteSpace(item.FormulaId)), item => Assert.Contains(item.FormulaId, formulaIds));
        Assert.All(result.RulePack.Actions, item =>
        {
            Assert.NotEmpty(item.EffectIds);
            Assert.All(item.RequirementIds, id => Assert.Contains(id, requirementIds));
            Assert.All(item.EffectIds, id => Assert.Contains(id, effectIds));
        });
        Assert.All(result.RulePack.EventRules, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.TriggerId));
            Assert.All(item.RequirementIds, id => Assert.Contains(id, requirementIds));
            Assert.All(item.ActionIds, id => Assert.Contains(id, actionIds));
        });
        Assert.All(result.RulePack.Requirements.SelectMany(item => item.SourceRefs)
            .Concat(result.RulePack.Effects.SelectMany(item => item.SourceRefs))
            .Concat(result.RulePack.Actions.SelectMany(item => item.SourceRefs))
            .Concat(result.RulePack.EventRules.SelectMany(item => item.SourceRefs)),
            item => Assert.Contains(item.Id, sourceIds));
    }

    [Fact]
    public void ValidatorRejectsDuplicateUnsafeIdsInvalidFormulaAndMissingRefs()
    {
        var validator = new FormulaEffectActionRulePackValidator();
        var invalid = new FormulaEffectActionRulePack
        {
            Formulas =
            [
                new FormulaDefinition
                {
                    FormulaId = "formula/bad",
                    Expression = "known_value + missing_value",
                    DeclaredVariables = ["known_value"]
                },
                new FormulaDefinition
                {
                    FormulaId = "formula/bad",
                    Expression = "System.IO.File",
                    DeclaredVariables = ["system"]
                }
            ],
            Requirements =
            [
                new RequirementDefinition
                {
                    RequirementId = "../bad",
                    RequirementType = FormulaEffectActionRulePackConstants.RequirementOpenRoute,
                    FormulaId = "formula/missing"
                }
            ],
            Actions =
            [
                new ActionDefinition
                {
                    ActionId = "action/bad",
                    ActionType = FormulaEffectActionRulePackConstants.ActionResolveEncounter,
                    RequirementIds = ["requirement/missing"],
                    EffectIds = []
                }
            ],
            EventRules =
            [
                new EventRuleDefinition
                {
                    EventRuleId = "event_rule/bad",
                    EventRuleType = "event_rule/on_resolve_encounter",
                    TriggerId = string.Empty,
                    ActionIds = ["action/missing"]
                }
            ]
        };

        var diagnostics = validator.Validate(invalid);

        Assert.Contains(diagnostics, item => item.Code == "rule_pack.duplicate_id");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.unsafe_id");
        Assert.Contains(diagnostics, item => item.Code == "formula.expression.unknown_variable");
        Assert.Contains(diagnostics, item => item.Code == "formula.expression.unsafe");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.unknown_formula_ref");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.unknown_requirement_ref");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.unknown_action_ref");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.empty_action_effects");
        Assert.Contains(diagnostics, item => item.Code == "rule_pack.empty_event_trigger");
    }

    [Fact]
    public void MissingSourcePlanRefsProduceDiagnosticsInsteadOfThrowing()
    {
        var plan = CreatePlan();
        var result = new FormulaEffectActionRegistryService().Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan });
        var tampered = result.RulePack with
        {
            Effects =
            [
                result.RulePack.Effects[0] with
                {
                    SourceRefs = [new GeneratedPlanReference { Kind = "item", Id = "item_seed/missing" }]
                }
            ]
        };

        var diagnostics = new FormulaEffectActionRulePackValidator().Validate(tampered, plan);

        Assert.Contains(diagnostics, item =>
            item.Code == "rule_pack.missing_source_plan_ref" &&
            item.Severity == "warning");
    }

    private static ProceduralGeneratedGamePlan CreatePlan()
    {
        var kernel = new ProceduralGameKernelService();
        return kernel.Generate(new ProceduralGameKernelRequest
        {
            Seed = "formula-effect-action-registry-tests",
            Mode = ProceduralGameGenerationModes.SemiProceduralRegions,
            CompactStyleHintIds =
            [
                "theme/survival",
                "tone/mysterious",
                "quest_motif/faction_truce",
                "item_affordance/tradable"
            ],
            SelectedVariantIds =
            [
                "world_topology/region_graph",
                "actor_model/single_player_character",
                "combat_model/turn_based",
                "inventory_model/list_inventory"
            ]
        }).Plan;
    }
}
