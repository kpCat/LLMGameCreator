using System.Text.Json;
using LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CandidateWorldBiomeNoiseSmokeTests
{
    [Fact]
    public async Task CandidateWorldBiomeNoiseProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new WorldBiomeNoiseCandidateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<WorldBiomeNoiseCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.CandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.FastNoiseLiteDependencyAdopted);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}

