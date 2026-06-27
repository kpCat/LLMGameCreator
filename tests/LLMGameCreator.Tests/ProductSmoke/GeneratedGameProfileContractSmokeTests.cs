using System.Text.Json;
using LLMGameCreator.Application.Design.GameProfiles;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratedGameProfileContractSmokeTests
{
    [Fact]
    public async Task GeneratedGameProfileContractProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var profileDirectory = Path.Combine(repoRoot, "samples", "game-profiles");
        var service = new GeneratedGameProfileContractAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot, profileDirectory);

        Assert.True(File.Exists(write.ProfilesJsonPath));
        Assert.True(File.Exists(write.PipelinePlanJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<GeneratedGameProfileContractReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.Equal("generated-game-profile-contract", report.ProductSmokeRoute);
        Assert.Equal(3, report.ValidProfileCount);
        Assert.Equal(3, report.PipelinePlanCount);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.True(report.InvalidMatrix.Passed);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.UnityBuildExecuted);
        Assert.True(report.NoExternalProviderLlmRagLuaMedia);
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
