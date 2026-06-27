using System.Text.Json;
using LLMGameCreator.Application.Design.RichPackageAssemblyCoverageAudit;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class RichPackageAssemblyCoverageAuditSmokeTests
{
    [Fact]
    public async Task RichPackageAssemblyCoverageAuditProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new RichPackageAssemblyCoverageAuditAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.CoverageMatrixJsonPath));
        Assert.True(File.Exists(write.GapReportJsonPath));
        Assert.True(File.Exists(write.NextSlicePlanJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<RichPackageAssemblyCoverageAuditReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal(RichPackageAssemblyCoverageAuditAcceptanceService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.True(report.Goal023EvidenceVerified);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.True(report.RequiredCoverageDomainsPresent);
        Assert.True(report.FutureRequiredAndBlockedGapsPreserved);
        Assert.True(report.CoverageMatrixWritten);
        Assert.True(report.GapReportWritten);
        Assert.True(report.NextSlicePlanWritten);
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
