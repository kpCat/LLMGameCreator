using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ExpandedContractBatchSmokeTests
{
    [Fact]
    public async Task ExpandedContractBatchSmoke()
    {
        using var temp = new TempDirectory();
        var exportFolder = ResolveExportFolder(temp.Path);
        var artifactSet = ProductSmokeBaselineApprovedArtifacts.CreateExpandedApprovedArtifactSet();
        var service = new GeneratorPlanGamePackageAssemblyService(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            new JsonGamePackageRepository());

        var result = await service.AssembleFromApprovedArtifactSetAsync(
            artifactSet,
            new GeneratorPlanGamePackageAssemblyRequest
            {
                AppliedAtUtc = ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                ExportPackageJson = true,
                ExportFolderPath = exportFolder
            },
            CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        var packagePath = Path.Combine(exportFolder, "package.json");
        Assert.True(File.Exists(packagePath));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(packagePath));
        var generatedContent = document.RootElement.GetProperty("generatedContent");
        foreach (var propertyName in new[] { "regions", "npcs", "items", "dialogues", "encounters" })
        {
            Assert.True(generatedContent.GetProperty(propertyName).GetArrayLength() > 0, propertyName);
        }

        var expectedContracts = new[]
        {
            "game_profile_v1", "region_pack_v1", "scene_pack_v1", "npc_pack_v1", "quest_pack_v1",
            "dialogue_pack_v1", "mechanics_pack_v1", "encounter_pack_v1", "item_pack_v1"
        };
        Assert.All(expectedContracts, contractId =>
            Assert.Contains(result.Package.GeneratedContent.AppliedArtifacts, provenance =>
                provenance.ContractId == contractId && provenance.MappingResult == GeneratorPlanGamePackageAssemblyMappingResult.Mapped));

        var projection = new GeneratedPackageRuntimePreviewService().Build(result.Package, null);
        Assert.Single(projection.Regions);
        Assert.Single(projection.Npcs);
        Assert.Single(projection.Items);
        Assert.Single(projection.Dialogues);
        Assert.Single(projection.Encounters);
        Assert.Equal("Smoke Harbor", projection.Regions[0].Title);
        Assert.Equal("Smoke Guide", projection.Npcs[0].Title);
        Assert.Contains("npc/smoke-guide", projection.Dialogues[0].References);
        Assert.Contains("region/smoke-harbor", projection.Encounters[0].References);
        Assert.All(artifactSet.ApprovedArtifacts, artifact =>
        {
            Assert.DoesNotContain("provider", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lm_studio", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
        });
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
