using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests
{
    [Fact]
    public async Task Goal080ProjectedGamePackageReadsValidatesAndProjectsIntoRuntimePreview()
    {
        var service = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.ProjectedPackageFileLedger.Passed);
        Assert.True(result.RuntimePreviewBridgeProof.Passed);
        Assert.True(result.RuntimePreviewNegativeProof.Passed);

        var packagePath = Path.Combine(write.ProjectedPackageDirectoryPath, "package.json");
        Assert.True(File.Exists(packagePath));
        var package = ReadPackage(packagePath);
        var validation = new GamePackageValidator().Validate(package);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));

        var start = new DefaultGameRuntime().Start(package);
        Assert.True(start.Success);
        Assert.Equal(package.Manifest.StartMapId, start.State.CurrentMapId);

        var preview = new GeneratedPackageRuntimePreviewService().Build(package, start.State);
        var interactions = new GeneratedContentInteractionPreviewService().Build(preview);

        Assert.NotNull(preview.CurrentScene);
        Assert.Empty(preview.Warnings);
        Assert.Equal(18, preview.Items.Count);
        Assert.Equal(9, preview.Quests.Count);
        Assert.Equal(18, preview.Mechanics.Count);
        Assert.True(interactions.Categories.Sum(category => category.Entries.Count) >= 27);

        var proof = ReadArtifact<EditDrivenGamePackageRuntimePreviewBridgeProof>(
            write.OutputDirectoryPath,
            "runtime-preview-bridge-proof.json");
        var negative = ReadArtifact<EditDrivenGamePackageRuntimePreviewBridgeNegativeProof>(
            write.OutputDirectoryPath,
            "runtime-preview-negative-proof.json");
        Assert.True(proof.Passed);
        Assert.True(negative.Passed);
    }

    private static GamePackageDefinition ReadPackage(string packagePath)
    {
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(packagePath), JsonOptions());
        Assert.NotNull(package);
        return package!;
    }

    private static T ReadArtifact<T>(string outputRoot, string fileName)
    {
        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(outputRoot, fileName)), JsonOptions());
        Assert.NotNull(value);
        return value!;
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
