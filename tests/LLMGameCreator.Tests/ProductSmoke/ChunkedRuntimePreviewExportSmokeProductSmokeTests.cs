using System.Text.Json;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ChunkedRuntimePreviewExportSmokeProductSmokeTests
{
    [Fact]
    public async Task ChunkedRuntimePreviewExportSmokeProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new ChunkedRuntimePreviewExportEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.CatalogSummaryJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.FrontierPayloadJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.GothicPayloadJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.CaravanPayloadJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.MetamodulePayloadJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.ExportManifestJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.MultiFamilyMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.InfiniteSmokeProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.RuntimePreviewConsumptionProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.PackageImmutabilityAuditJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.CatalogSummaryJsonFileName)));
        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.FrontierPayloadJsonFileName)));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.ExportManifestJsonFileName)));
        using var multiFamily = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.MultiFamilyMatrixJsonFileName)));
        using var infinite = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.InfiniteSmokeProofJsonFileName)));
        using var audit = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.PackageImmutabilityAuditJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(4, catalog.RootElement.GetProperty("payloadCount").GetInt32());
        Assert.True(catalog.RootElement.GetProperty("goal039AcceptedByUserHandoff").GetBoolean());
        Assert.False(catalog.RootElement.GetProperty("goal040GatePassed").GetBoolean());
        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(frontier.RootElement.GetProperty("sourceEvidence").GetProperty("consumesGoal039RuntimeDeltaCommands").GetBoolean());
        Assert.Equal(4, manifest.RootElement.GetProperty("payloads").GetArrayLength());
        Assert.True(multiFamily.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(infinite.RootElement.GetProperty("deterministic").GetBoolean());
        Assert.True(audit.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("chunked_runtime_preview_export_multifamily_smoke_verification required", report);
        Assert.Contains("accepted=false", report);
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
