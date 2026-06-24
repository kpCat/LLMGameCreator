using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Procedural;

public sealed class GeneratedPackageMvpServiceTests
{
    [Fact]
    public void SamePipelineInputProducesByteIdenticalPackageAndReports()
    {
        var pipeline = CreatePipeline();
        var service = new GeneratedPackageMvpService();
        var request = CreateRequest(pipeline);

        var first = service.Generate(request);
        var second = service.Generate(request);

        Assert.Equal(first.PackageJson, second.PackageJson);
        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.Equal(first.RuntimeBootstrapReportJson, second.RuntimeBootstrapReportJson);
        Assert.Equal(first.RuntimeBootstrapReportMarkdown, second.RuntimeBootstrapReportMarkdown);
        Assert.False(first.Report.HasErrors);
        Assert.DoesNotContain(first.Report.ValidationIssues, issue => issue.Severity is "Error" or "Critical");
        Assert.Contains(first.Diagnostics, item => item.Code == "generated_package_mvp.no_external_execution");
    }

    [Fact]
    public void PackageContainsDeterministicMetadataAndMappedGameplayRecords()
    {
        var pipeline = CreatePipeline();
        var result = new GeneratedPackageMvpService().Generate(CreateRequest(pipeline));

        Assert.Equal(pipeline.Plan.PlanId, result.Report.Source.PlanId);
        Assert.Equal(pipeline.Plan.Metadata.DeterministicHash, result.Report.Source.PlanHash);
        Assert.Equal(pipeline.Registry.RulePack.Metadata.RulePackId, result.Report.Source.RulePackId);
        Assert.Equal(pipeline.Registry.RulePack.Metadata.DeterministicHash, result.Report.Source.RulePackHash);
        Assert.Equal(pipeline.Loop.State.DeterministicHash, result.Report.Source.TinyLoopStateHash);
        Assert.Contains(pipeline.Plan.Metadata.Seed, result.PackageJson, StringComparison.Ordinal);
        Assert.StartsWith("game/generated_mvp_", result.Package.Manifest.PackageId, StringComparison.Ordinal);
        Assert.True(result.Package.Game.Maps.Count >= 2);
        Assert.NotEmpty(result.Package.Game.Items);
        Assert.NotEmpty(result.Package.Game.Resources);
        Assert.NotEmpty(result.Package.Game.Encounters);
        Assert.NotEmpty(result.Package.Game.Quests);
        Assert.NotEmpty(result.Package.Game.Factions);
        Assert.NotEmpty(result.Package.GeneratedContent.AppliedArtifacts);
        Assert.Contains(result.Report.MappedRecords, item => item.PackageKind == "map");
        Assert.Contains(result.Report.MappedRecords, item => item.PackageKind == "item");
        Assert.Contains(result.Report.MappedRecords, item => item.PackageKind == "encounter");
        Assert.Contains(result.Report.MappedRecords, item => item.PackageKind == "quest");
    }

    [Fact]
    public void RuntimeBootstrapAndValidationEvidenceAreDeterministic()
    {
        var pipeline = CreatePipeline();
        var result = new GeneratedPackageMvpService().Generate(CreateRequest(pipeline));

        Assert.True(result.RuntimeBootstrapReport.ValidationPassed);
        Assert.True(result.RuntimeBootstrapReport.InitialStateCreated);
        Assert.True(result.RuntimeBootstrapReport.MapRuntimeStarted);
        Assert.True(result.RuntimeBootstrapReport.MoveCommandSucceeded);
        Assert.True(result.RuntimeBootstrapReport.InteractCommandObserved);
        Assert.Contains("InteractionTriggered", result.RuntimeBootstrapReport.EventTypes);
        Assert.Contains("Runtime Bootstrap", result.ReportMarkdown, StringComparison.Ordinal);
        Assert.Contains(result.Report.PackageHash, result.ReportMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingInputsProduceDiagnosticsInsteadOfUnhandledExceptions()
    {
        var result = new GeneratedPackageMvpService().Generate(new GeneratedPackageMvpRequest());

        Assert.NotNull(result.Package);
        Assert.NotEmpty(result.PackageJson);
        Assert.Contains(result.Diagnostics, item => item.Code == "generated_package_mvp.source_plan_missing");
        Assert.Contains(result.Diagnostics, item => item.Code == "generated_package_mvp.rule_pack_missing");
        Assert.True(result.Report.HasErrors);
        Assert.Contains("source_plan_missing", result.ReportMarkdown, StringComparison.Ordinal);
    }

    private static GeneratedPackageMvpRequest CreateRequest(
        (ProceduralGeneratedGamePlan Plan, FormulaEffectActionRegistryResult Registry, TinyGeneratedRuntimeLoopResult Loop) pipeline) => new()
    {
        SourcePlan = pipeline.Plan,
        RulePack = pipeline.Registry.RulePack,
        RulePackValidationReport = pipeline.Registry.ValidationReport,
        TinyLoopResult = pipeline.Loop
    };

    private static (ProceduralGeneratedGamePlan Plan, FormulaEffectActionRegistryResult Registry, TinyGeneratedRuntimeLoopResult Loop) CreatePipeline()
    {
        var kernel = new ProceduralGameKernelService();
        var plan = kernel.Generate(new ProceduralGameKernelRequest
        {
            Seed = "generated-package-mvp-tests",
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
        var loop = new TinyGeneratedRuntimeLoopService().Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = plan,
            RulePack = registry.RulePack,
            RulePackValidationReport = registry.ValidationReport
        });

        return (plan, registry, loop);
    }
}
