using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ProceduralGameKernelSmokeTests
{
    [Fact]
    public async Task ProceduralGameKernelProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = new ProceduralGameKernelService();
        var request = new ProceduralGameKernelRequest
        {
            Seed = "product-smoke-procedural-kernel",
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

        var first = service.Generate(request);
        var second = service.Generate(request);
        var write = await service.WriteAsync(projectRoot, first);

        Assert.Equal(first.Json, second.Json);
        Assert.True(File.Exists(write.JsonPath));
        Assert.True(File.Exists(write.MarkdownPath));
        Assert.Equal(first.Json, await File.ReadAllTextAsync(write.JsonPath));
        Assert.Equal(first.Markdown, await File.ReadAllTextAsync(write.MarkdownPath));
        Assert.Contains(".llmgc", write.JsonPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("procedural", write.MarkdownPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("world_topology/infinite_chunks", first.Json, StringComparison.Ordinal);
        Assert.Contains("Formula/effect/action placeholders", first.Markdown, StringComparison.Ordinal);
        Assert.Contains(first.Plan.Metadata.DeterministicHash, first.Markdown, StringComparison.Ordinal);
        Assert.Contains(first.Diagnostics, item => item.Code == "procedural_kernel.no_external_execution");
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
