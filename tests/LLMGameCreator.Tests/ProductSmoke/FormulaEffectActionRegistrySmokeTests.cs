using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class FormulaEffectActionRegistrySmokeTests
{
    [Fact]
    public async Task FormulaEffectActionRegistryProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var kernel = new ProceduralGameKernelService();
        var registry = new FormulaEffectActionRegistryService();
        var request = new ProceduralGameKernelRequest
        {
            Seed = "product-smoke-formula-effect-action-registry",
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
        var first = registry.Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan.Plan });
        var second = registry.Generate(new FormulaEffectActionRegistryRequest { SourcePlan = plan.Plan });
        var write = await registry.WriteAsync(projectRoot, first);

        Assert.Equal(first.Json, second.Json);
        Assert.Equal(first.Markdown, second.Markdown);
        Assert.Equal(first.ValidationReportJson, second.ValidationReportJson);
        Assert.Equal(first.ValidationReportMarkdown, second.ValidationReportMarkdown);
        Assert.True(File.Exists(write.RulePackJsonPath));
        Assert.True(File.Exists(write.RulePackMarkdownPath));
        Assert.True(File.Exists(write.ValidationReportJsonPath));
        Assert.True(File.Exists(write.ValidationReportMarkdownPath));
        Assert.Equal(first.Json, await File.ReadAllTextAsync(write.RulePackJsonPath));
        Assert.Equal(first.Markdown, await File.ReadAllTextAsync(write.RulePackMarkdownPath));
        Assert.Equal(first.ValidationReportJson, await File.ReadAllTextAsync(write.ValidationReportJsonPath));
        Assert.Equal(first.ValidationReportMarkdown, await File.ReadAllTextAsync(write.ValidationReportMarkdownPath));
        Assert.Contains(".llmgc", write.RulePackJsonPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("procedural", write.ValidationReportMarkdownPath, StringComparison.OrdinalIgnoreCase);
        Assert.False(first.ValidationReport.HasErrors);
        Assert.Contains(FormulaEffectActionRulePackConstants.RequirementOpenRoute, first.Json, StringComparison.Ordinal);
        Assert.Contains(FormulaEffectActionRulePackConstants.RequirementFactionAccess, first.Json, StringComparison.Ordinal);
        Assert.Contains(FormulaEffectActionRulePackConstants.ActionResolveEncounter, first.Json, StringComparison.Ordinal);
        Assert.Contains(FormulaEffectActionRulePackConstants.RewardQuestProgress, first.Json, StringComparison.Ordinal);
        Assert.Contains(first.RulePack.Metadata.DeterministicHash, first.Markdown, StringComparison.Ordinal);
        Assert.Contains(first.Diagnostics, item => item.Code == "rule_pack.no_external_execution");
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
