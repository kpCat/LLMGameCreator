using System.Text.Json;
using LLMGameCreator.Application.Design.PackageAssemblyWorldEntities;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class PackageAssemblyWorldEntitiesSmokeTests
{
    [Fact]
    public async Task PackageAssemblyWorldEntitiesProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new PackageAssemblyWorldEntitiesAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.MappingContractProofJsonPath));
        Assert.True(File.Exists(write.InputFixturesJsonPath));
        Assert.True(File.Exists(write.AssemblyReportJsonPath));
        Assert.True(File.Exists(write.PackageSummaryJsonPath));
        Assert.True(File.Exists(write.AntiOverfitFixturesJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ScopeReportJsonPath));
        Assert.True(File.Exists(write.ScopeReportMarkdownPath));

        var report = JsonSerializer.Deserialize<PackageAssemblyWorldEntitiesReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.True(report.Goal024EvidenceVerified);
        Assert.True(report.Goal023EvidenceVerified);
        Assert.True(report.RealConsumerPassed);
        Assert.True(report.SyntheticConsumerPassed);
        Assert.True(report.AntiOverfitProofPassed);
        Assert.True(report.WorldEntityMappingWritten);
        Assert.True(report.PackageSummaryWritten);
        Assert.True(report.PackageAssemblyExecuted);
        Assert.False(report.ProductVerticalGate);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.UnityBuildExecuted);
        Assert.False(report.LlmRagProviderMediaLuaExecuted);
        Assert.True(report.ScopeGuardPassed);
        Assert.True(report.InvalidMatrix.Passed);
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
