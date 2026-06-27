using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CapabilityBundlePipelineInputsSmokeTests
{
    [Fact]
    public async Task CapabilityBundlePipelineInputsProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var profileDirectory = Path.Combine(repoRoot, "samples", "game-profiles");
        var service = new CapabilityBundlePipelineInputsAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot, profileDirectory);

        Assert.True(File.Exists(write.ProfileRequestsJsonPath));
        Assert.True(File.Exists(write.SelectionJsonPath));
        Assert.True(File.Exists(write.GeneratorInputsJsonPath));
        Assert.True(File.Exists(write.GapReportJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<CapabilityBundlePipelineInputsReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.Equal(3, report.ValidProfileCount);
        Assert.Equal(3, report.PipelineInputCount);
        Assert.True(report.CapabilitySelectionStarted);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.True(report.FutureRequiredCapabilitiesPreserved);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.False(report.PackageAssemblyExecuted);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.UnityBuildExecuted);
        Assert.False(report.LlmRagProviderMediaLuaExecuted);
        Assert.True(report.ScopeGuardPassed);
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
