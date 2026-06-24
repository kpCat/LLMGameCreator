using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class TinyGeneratedRuntimeLoopSmokeTests
{
    [Fact]
    public async Task TinyGeneratedRuntimeLoopProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var kernel = new ProceduralGameKernelService();
        var registry = new FormulaEffectActionRegistryService();
        var loop = new TinyGeneratedRuntimeLoopService();
        var request = new ProceduralGameKernelRequest
        {
            Seed = "product-smoke-tiny-generated-runtime-loop",
            Mode = ProceduralGameGenerationModes.FullySeededWorld,
            CompactStyleHintIds =
            [
                "theme/exploration",
                "theme/survival",
                "tone/mysterious",
                "quest_motif/recover_lost_resource",
                "item_affordance/quest_item"
            ],
            SelectedVariantIds =
            [
                "world_topology/infinite_chunks",
                "chunk_streaming/generated_on_demand",
                "actor_model/single_player_character",
                "combat_model/turn_based"
            ]
        };

        var plan = kernel.Generate(request);
        var rulePack = registry.Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan.Plan });
        var first = loop.Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = plan.Plan,
            RulePack = rulePack.RulePack,
            RulePackValidationReport = rulePack.ValidationReport
        });
        var second = loop.Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = plan.Plan,
            RulePack = rulePack.RulePack,
            RulePackValidationReport = rulePack.ValidationReport
        });

        var planWrite = await kernel.WriteAsync(projectRoot, plan);
        var rulePackWrite = await registry.WriteAsync(projectRoot, rulePack);
        var loopWrite = await loop.WriteAsync(projectRoot, first);

        Assert.Equal(first.StateJson, second.StateJson);
        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.True(File.Exists(planWrite.JsonPath));
        Assert.True(File.Exists(planWrite.MarkdownPath));
        Assert.True(File.Exists(rulePackWrite.RulePackJsonPath));
        Assert.True(File.Exists(rulePackWrite.RulePackMarkdownPath));
        Assert.True(File.Exists(rulePackWrite.ValidationReportJsonPath));
        Assert.True(File.Exists(rulePackWrite.ValidationReportMarkdownPath));
        Assert.True(File.Exists(loopWrite.StateJsonPath));
        Assert.True(File.Exists(loopWrite.ReportJsonPath));
        Assert.True(File.Exists(loopWrite.ReportMarkdownPath));
        Assert.Equal(first.StateJson, await File.ReadAllTextAsync(loopWrite.StateJsonPath));
        Assert.Equal(first.ReportJson, await File.ReadAllTextAsync(loopWrite.ReportJsonPath));
        Assert.Equal(first.ReportMarkdown, await File.ReadAllTextAsync(loopWrite.ReportMarkdownPath));
        Assert.Contains(".llmgc", loopWrite.StateJsonPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("procedural", loopWrite.ReportMarkdownPath, StringComparison.OrdinalIgnoreCase);
        Assert.False(first.Report.HasErrors);
        Assert.NotEmpty(first.State.AppliedEffectIds);
        Assert.NotEmpty(first.State.InventoryItemCounts);
        Assert.Contains(first.State.DeterministicHash, first.ReportMarkdown, StringComparison.Ordinal);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
