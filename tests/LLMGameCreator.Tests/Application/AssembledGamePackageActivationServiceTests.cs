using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class AssembledGamePackageActivationServiceTests
{
    [Fact]
    public async Task ActivationFailsClearlyWhenAssembledPackageIsMissing()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        var assembly = await AssembleAsync(Path.Combine(temp.Path, "seed"));
        await repository.SaveAsync(temp.Path, assembly.Package, CancellationToken.None);
        await current.LoadAsync(temp.Path, CancellationToken.None);

        var result = await new AssembledGamePackageActivationService(repository, new GamePackageValidator(), current)
            .ActivateLatestAsync(CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Equal("assembled_package_not_found", result.Status);
        Assert.Contains(".llmgc", result.SourcePath);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public async Task ActivationLoadsValidatesAndSetsAssembledPackageWithoutChangingRootFile()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();
        var assemblyFolder = Path.Combine(temp.Path, ".llmgc", "package-assembly");
        var assembly = await AssembleAsync(assemblyFolder);
        await repository.SaveAsync(temp.Path, assembly.Package, CancellationToken.None);
        var rootPackagePath = Path.Combine(temp.Path, "package.json");
        var rootBefore = await File.ReadAllTextAsync(rootPackagePath);

        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(temp.Path, CancellationToken.None);
        current.ReplaceCurrent(new LLMGameCreator.GamePackage.GamePackageDefinition());

        var result = await new AssembledGamePackageActivationService(repository, new GamePackageValidator(), current)
            .ActivateLatestAsync(CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("activated", result.Status);
        Assert.Equal("Headless Smoke Baseline", current.CurrentPackage?.GeneratedContent.Profile.Title);
        Assert.NotEmpty(current.CurrentPackage!.GeneratedContent.Dialogues);
        Assert.Equal(temp.Path, current.CurrentFolder);
        Assert.Equal(rootBefore, await File.ReadAllTextAsync(rootPackagePath));
    }

    private static async Task<GeneratorPlanGamePackageAssemblyResult> AssembleAsync(string exportFolder)
    {
        return await new GeneratorPlanGamePackageAssemblyService(
                new GeneratorPlanGamePackageAssembler(),
                new GamePackageValidator(),
                new GeneratorPlanGamePackageAssemblyValidator(),
                new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
                new JsonGamePackageRepository())
            .AssembleFromApprovedArtifactSetAsync(
                ProductSmoke.ProductSmokeBaselineApprovedArtifacts.CreateExpandedApprovedArtifactSet(),
                new GeneratorPlanGamePackageAssemblyRequest
                {
                    AppliedAtUtc = ProductSmoke.ProductSmokeBaselineApprovedArtifacts.AppliedAtUtc,
                    ExportPackageJson = true,
                    ExportFolderPath = exportFolder
                },
                CancellationToken.None);
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
