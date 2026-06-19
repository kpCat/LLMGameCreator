using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratedPackageRuntimePreviewSmokeTests
{
    [Fact]
    public async Task GeneratedPackageRuntimePreviewSmoke()
    {
        using var temp = new TempDirectory();
        var exportFolder = ResolveExportFolder(temp.Path);
        var artifactSet = ProductSmokeBaselineApprovedArtifacts.CreateApprovedArtifactSet();
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
        Assert.All(artifactSet.ApprovedArtifacts, artifact =>
        {
            Assert.DoesNotContain("provider", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lm_studio", artifact.ContentJson, StringComparison.OrdinalIgnoreCase);
        });

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(assembly.Package);
        var projection = new GeneratedPackageRuntimePreviewService().Build(assembly.Package, start.State);

        Assert.True(start.Success);
        Assert.Equal("map/start", start.State.CurrentMapId);
        Assert.NotNull(projection.CurrentScene);
        Assert.Equal("Smoke Start", projection.CurrentScene.Title);
        Assert.Equal("A compact start scene for package assembly smoke.", projection.CurrentScene.Description);
        Assert.Equal("Prove scene mapping and generatedContent.scenes.", projection.CurrentScene.Purpose);
        Assert.Equal("Headless Smoke Baseline", projection.Profile.Title);
        Assert.Equal("A deterministic package assembly smoke baseline.", projection.Profile.Description);
        Assert.Contains("complete_quest", projection.Profile.CoreLoop);
        Assert.Contains(projection.Quests, quest => quest.Title == "Run the Smoke" && quest.Description.Contains("inspectable quest", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(projection.Mechanics, mechanic => mechanic.Name == "Smoke Check" && mechanic.Description.Contains("headless product smoke", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(projection.Provenance, provenance => provenance.ContractId == "game_profile_v1" && provenance.MappingResult == "mapped");
        Assert.Contains(projection.Provenance, provenance => provenance.ContractId == "scene_pack_v1" && provenance.MappingResult == "mapped");
        Assert.Contains(projection.Provenance, provenance => provenance.ContractId == "quest_pack_v1" && provenance.MappingResult == "mapped");
        Assert.Contains(projection.Provenance, provenance => provenance.ContractId == "mechanics_pack_v1" && provenance.MappingResult == "mapped");

        var movement = runtime.Execute(assembly.Package, start.State, PlayerCommand.Move(Direction2D.Right));
        var afterMoveProjection = new GeneratedPackageRuntimePreviewService().Build(assembly.Package, movement.State);

        Assert.True(movement.Success);
        Assert.Equal(2, movement.State.PlayerPosition.X);
        Assert.Equal(1, movement.State.PlayerPosition.Y);
        Assert.Equal("Smoke Start", afterMoveProjection.CurrentScene?.Title);
        Assert.Empty(afterMoveProjection.Warnings);
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
