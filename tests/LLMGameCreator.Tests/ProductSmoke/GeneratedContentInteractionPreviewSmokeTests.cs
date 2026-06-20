using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratedContentInteractionPreviewSmokeTests
{
    [Fact]
    public async Task GeneratedContentInteractionPreviewProductSmoke()
    {
        using var temp = new TempDirectory();
        var exportFolder = ResolveExportFolder(temp.Path);
        var artifactSet = ProductSmokeBaselineApprovedArtifacts.CreateExpandedApprovedArtifactSet();
        var assemblyService = new GeneratorPlanGamePackageAssemblyService(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            new JsonGamePackageRepository());

        var assembly = await assemblyService.AssembleFromApprovedArtifactSetAsync(
            artifactSet,
            new GeneratorPlanGamePackageAssemblyRequest
            {
                AppliedAtUtc = ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                ExportPackageJson = true,
                ExportFolderPath = exportFolder
            },
            CancellationToken.None);

        Assert.True(assembly.Ok, string.Join(Environment.NewLine, assembly.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.True(File.Exists(Path.Combine(exportFolder, "package.json")));

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(assembly.Package);
        Assert.True(start.Success);

        var preview = new GeneratedPackageRuntimePreviewService().Build(assembly.Package, start.State);
        var catalog = new GeneratedContentInteractionPreviewService().Build(preview);
        var expectedCategoryIds = new[]
        {
            "current_scene", "regions", "npcs", "items", "dialogues",
            "quests", "mechanics", "encounters", "applied_artifacts", "warnings"
        };

        Assert.Equal(expectedCategoryIds, catalog.Categories.Select(category => category.Id));

        var region = SingleEntry(catalog, "regions");
        var npc = SingleEntry(catalog, "npcs");
        var item = SingleEntry(catalog, "items");
        var dialogue = SingleEntry(catalog, "dialogues");
        var quest = SingleEntry(catalog, "quests");
        var mechanic = SingleEntry(catalog, "mechanics");
        var encounter = SingleEntry(catalog, "encounters");
        var artifact = catalog.Categories.Single(category => category.Id == "applied_artifacts").Entries
            .Single(entry => entry.Title == "dialogue_pack_v1");

        Assert.All(new[] { region, npc, item, dialogue, quest, mechanic, encounter, artifact },
            entry => Assert.False(string.IsNullOrWhiteSpace(entry.DetailsText)));
        Assert.Contains("region/smoke-harbor", npc.DetailsText);
        Assert.Contains("scene/smoke-start", npc.DetailsText);
        Assert.Contains("Welcome to Smoke Harbor.", dialogue.DetailsText);
        Assert.Contains("Inspect the package", quest.DetailsText);
        Assert.Contains("package_json_exists", quest.DetailsText);
        Assert.Contains("region/smoke-harbor", encounter.DetailsText);
        Assert.Contains("npc/smoke-guide", encounter.DetailsText);
        Assert.Contains("Contract: dialogue_pack_v1", artifact.DetailsText);
        Assert.Contains("Mapping: mapped", artifact.DetailsText);
        Assert.Contains("Content hash:", artifact.DetailsText);

        var movement = runtime.Execute(assembly.Package, start.State, PlayerCommand.Move(Direction2D.Right));
        var afterMoveCatalog = new GeneratedContentInteractionPreviewService().Build(
            new GeneratedPackageRuntimePreviewService().Build(assembly.Package, movement.State));

        Assert.True(movement.Success);
        Assert.Equal(2, movement.State.PlayerPosition.X);
        Assert.Single(afterMoveCatalog.Categories.Single(category => category.Id == "npcs").Entries);
        Assert.All(artifactSet.ApprovedArtifacts, approvedArtifact =>
        {
            Assert.DoesNotContain("provider", approvedArtifact.ContentJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lm_studio", approvedArtifact.ContentJson, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static GeneratedContentInteractionEntry SingleEntry(
        GeneratedContentInteractionCatalog catalog,
        string categoryId)
    {
        return Assert.Single(catalog.Categories.Single(category => category.Id == categoryId).Entries);
    }

    private static string ResolveExportFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR");
        var exportFolder = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(tempPath, "package-output")
            : configured;

        Directory.CreateDirectory(exportFolder);
        return exportFolder;
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
