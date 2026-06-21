using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratedMapPlacementPreviewSmokeTests
{
    [Fact]
    public async Task GeneratedMapPlacementPreviewProductSmoke()
    {
        using var temp = new TempDirectory();
        var exportFolder = ResolveExportFolder(temp.Path);
        var artifacts = ProductSmokeBaselineApprovedArtifacts.CreateExpandedApprovedArtifactSet();
        var assembly = await new GeneratorPlanGamePackageAssemblyService(
                new GeneratorPlanGamePackageAssembler(),
                new GamePackageValidator(),
                new GeneratorPlanGamePackageAssemblyValidator(),
                new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
                new JsonGamePackageRepository())
            .AssembleFromApprovedArtifactSetAsync(
                artifacts,
                new GeneratorPlanGamePackageAssemblyRequest
                {
                    AppliedAtUtc = ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                    ExportPackageJson = true,
                    ExportFolderPath = exportFolder
                },
                CancellationToken.None);

        Assert.True(assembly.Ok, string.Join(Environment.NewLine, assembly.Diagnostics.Select(item => item.Message)));
        Assert.True(File.Exists(Path.Combine(exportFolder, "package.json")));

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(assembly.Package);
        Assert.True(start.Success);
        var preview = new GeneratedPackageRuntimePreviewService().Build(assembly.Package, start.State);
        var placementService = new GeneratedMapPlacementPreviewService();
        var first = placementService.Build(assembly.Package, start.State, preview);
        var second = placementService.Build(assembly.Package, start.State, preview);

        Assert.Equal(
            assembly.Package.GeneratedContent.Npcs.Count,
            first.Markers.Count(marker => marker.Type == GeneratedRuntimeMapMarkerType.Npc));
        Assert.Equal(
            assembly.Package.GeneratedContent.Encounters.Count,
            first.Markers.Count(marker => marker.Type == GeneratedRuntimeMapMarkerType.Encounter));
        Assert.All(first.Markers, marker =>
        {
            var map = Assert.Single(assembly.Package.Game.Maps, candidate => candidate.Id == marker.MapId);
            Assert.InRange(marker.Position.X, 0, map.Width - 1);
            Assert.InRange(marker.Position.Y, 0, map.Height - 1);
        });
        Assert.Equal(
            first.Markers.Select(marker => (marker.MarkerId, marker.MapId, marker.Position.X, marker.Position.Y)),
            second.Markers.Select(marker => (marker.MarkerId, marker.MapId, marker.Position.X, marker.Position.Y)));

        var catalog = new GeneratedContentInteractionPreviewService().Build(preview);
        Assert.Equal(assembly.Package.GeneratedContent.Npcs.Count, catalog.Categories.Single(category => category.Id == "npcs").Entries.Count);
        Assert.Equal(assembly.Package.GeneratedContent.Encounters.Count, catalog.Categories.Single(category => category.Id == "encounters").Entries.Count);

        var movement = runtime.Execute(assembly.Package, start.State, PlayerCommand.Move(Direction2D.Right));
        Assert.True(movement.Success);
        Assert.All(artifacts.ApprovedArtifacts, artifact =>
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
