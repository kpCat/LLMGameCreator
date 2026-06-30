using System.Text.Json;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class MultiFamilyGeneratedTemplateVerticalSliceProductSmokeTests
{
    [Fact]
    public async Task MultiFamilyGeneratedTemplateVerticalSliceProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new MultiFamilyGeneratedTemplateEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.CatalogJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SharedLifecycleContractJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.MapPanelPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SurvivalPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.GridDungeonPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.MapPanelProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SurvivalProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.GridDungeonProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.RegressionMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.PreviewExportConsumptionMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.CatalogJsonFileName)));
        using var shared = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SharedLifecycleContractJsonFileName)));
        using var regression = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.RegressionMatrixJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(3, catalog.RootElement.GetProperty("familyCount").GetInt32());
        Assert.True(catalog.RootElement.GetProperty("goal040AcceptedByUserHandoff").GetBoolean());
        Assert.True(shared.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(regression.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("multi_family_generated_template_vertical_slice_verification required", report);
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
