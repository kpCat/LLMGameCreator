using System.Text.Json;
using LLMGameCreator.Application.Design.ModularGeneratorKernel;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ModularGeneratorKernelReadinessSmokeTests
{
    [Fact]
    public async Task ModularGeneratorKernelReadinessProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new ModularGeneratorKernelReadinessService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ModuleContractManifestProofJsonPath));
        Assert.True(File.Exists(write.ProductSmokeScenarioManifestProofJsonPath));
        Assert.True(File.Exists(write.PackageAssemblyModuleRegistryReportJsonPath));
        Assert.True(File.Exists(write.ModuleCompatibilityMatrixJsonPath));
        Assert.True(File.Exists(write.ModuleAbsenceBehaviorReportJsonPath));
        Assert.True(File.Exists(write.ParallelCandidatePolicyProofJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ScopeReportJsonPath));
        Assert.True(File.Exists(write.ScopeReportMarkdownPath));

        var report = JsonSerializer.Deserialize<ModularGeneratorKernelReadinessReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(ModularGeneratorKernelReadinessService.FinalGate, report.FinalStatus);
        Assert.Equal(ModularGeneratorKernelReadinessService.FinalGate, report.ManualGate);
        Assert.Equal(ModularGeneratorKernelReadinessService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.True(report.Goal028EvidenceVerified);
        Assert.True(report.ModuleManifestContractWritten);
        Assert.True(report.SmokeScenarioManifestContractWritten);
        Assert.True(report.ParallelCandidatePolicyWritten);
        Assert.True(report.ModuleRegistryWritten);
        Assert.True(report.ModuleCompatibilityMatrixWritten);
        Assert.True(report.OptionalModuleAbsenceHandled);
        Assert.True(report.RequiredModuleMissingRejected);
        Assert.True(report.ManifestSmokeScenarioExecuted);
        Assert.True(report.RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario);
        Assert.True(report.ModuleOnlyVerificationTierDefined);
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
