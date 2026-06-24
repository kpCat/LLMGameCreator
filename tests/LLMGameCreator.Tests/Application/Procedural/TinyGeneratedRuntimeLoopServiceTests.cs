using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Procedural;

public sealed class TinyGeneratedRuntimeLoopServiceTests
{
    [Fact]
    public void SamePlanAndRulePackProduceByteIdenticalStateAndReports()
    {
        var pipeline = CreatePipeline();
        var service = new TinyGeneratedRuntimeLoopService();
        var request = new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = pipeline.Plan,
            RulePack = pipeline.Registry.RulePack,
            RulePackValidationReport = pipeline.Registry.ValidationReport
        };

        var first = service.Run(request);
        var second = service.Run(request);

        Assert.Equal(first.StateJson, second.StateJson);
        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.Equal(first.State.DeterministicHash, second.State.DeterministicHash);
        Assert.False(first.Report.HasErrors);
        Assert.Contains(first.Diagnostics, item => item.Code == "tiny_runtime_loop.no_external_execution");
    }

    [Fact]
    public void SuccessfulLoopAppliesEffectsAndMutatesTinyRuntimeState()
    {
        var pipeline = CreatePipeline();

        var result = new TinyGeneratedRuntimeLoopService().Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = pipeline.Plan,
            RulePack = pipeline.Registry.RulePack,
            RulePackValidationReport = pipeline.Registry.ValidationReport
        });

        Assert.False(string.IsNullOrWhiteSpace(result.State.StartingRegionId));
        Assert.NotEmpty(result.State.VisitedRegionIds);
        Assert.False(string.IsNullOrWhiteSpace(result.State.ResolvedEncounterId));
        Assert.False(string.IsNullOrWhiteSpace(result.State.AdvancedQuestEventId));
        Assert.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter, result.State.AppliedActionIds);
        Assert.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress, result.State.AppliedActionIds);
        Assert.Contains(result.State.AppliedEffectIds, id => id.StartsWith("effect/grant_item/", StringComparison.Ordinal));
        Assert.NotEmpty(result.State.InventoryItemCounts);
        Assert.NotEmpty(result.State.Flags);
        Assert.NotEmpty(result.State.QuestEventStates);
        Assert.NotEmpty(result.State.FactionReputationDeltas);
        Assert.Contains(result.State.ResolvedEncounterId, result.ReportMarkdown, StringComparison.Ordinal);
        Assert.Contains(result.State.AdvancedQuestEventId, result.ReportMarkdown, StringComparison.Ordinal);
        Assert.Contains("Diagnostics", result.ReportMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingRefsAndUnsupportedTypesProduceDiagnosticsInsteadOfThrowing()
    {
        var pipeline = CreatePipeline();
        var unsupportedEffect = pipeline.Registry.RulePack.Effects[0] with
        {
            EffectId = "effect/unsupported/test",
            EffectType = "effect/teleport_player",
            SourceRefs = [new GeneratedPlanReference { Kind = "encounter", Id = pipeline.Plan.EncounterSeeds[0].EncounterSeedId }]
        };
        var unsupportedAction = new ActionDefinition
        {
            ActionId = "action/unsupported/test",
            ActionType = "action/teleport_player",
            EffectIds = [unsupportedEffect.EffectId],
            SourceRefs = [new GeneratedPlanReference { Kind = "encounter", Id = pipeline.Plan.EncounterSeeds[0].EncounterSeedId }]
        };
        var tampered = pipeline.Registry.RulePack with
        {
            Effects = pipeline.Registry.RulePack.Effects
                .Append(unsupportedEffect)
                .Append(pipeline.Registry.RulePack.Effects[0] with
                {
                    EffectId = "effect/set_flag/missing_ref",
                    SourceRefs = [new GeneratedPlanReference { Kind = "item", Id = "item_seed/missing" }]
                })
                .OrderBy(item => item.EffectId, StringComparer.Ordinal)
                .ToList(),
            Actions = pipeline.Registry.RulePack.Actions
                .Append(unsupportedAction)
                .OrderBy(item => item.ActionId, StringComparer.Ordinal)
                .ToList()
        };

        var result = new TinyGeneratedRuntimeLoopService().Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = pipeline.Plan,
            RulePack = tampered,
            RulePackValidationReport = pipeline.Registry.ValidationReport
        });

        Assert.Contains(result.Diagnostics, item => item.Code == "tiny_runtime_loop.unsupported_action_type");
        Assert.Contains(result.Diagnostics, item => item.Code == "tiny_runtime_loop.unsupported_effect_type");
        Assert.Contains(result.Diagnostics, item => item.Code == "tiny_runtime_loop.missing_source_plan_ref");
        Assert.Contains("tiny_runtime_loop.unsupported_effect_type", result.ReportMarkdown, StringComparison.Ordinal);
    }

    private static (ProceduralGeneratedGamePlan Plan, FormulaEffectActionRegistryResult Registry) CreatePipeline()
    {
        var kernel = new ProceduralGameKernelService();
        var plan = kernel.Generate(new ProceduralGameKernelRequest
        {
            Seed = "tiny-generated-runtime-loop-tests",
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
        var registry = new FormulaEffectActionRegistryService().Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan });

        return (plan, registry);
    }
}
