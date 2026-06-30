using System.Text.Json;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewExportEvidenceTests
{
    [Fact]
    public async Task EvidenceWriterCreatesRequiredFilesAndReport()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();

        var write = await ChunkedRuntimePreviewExportTestFactory.CreateService().BuildAndWriteAsync(temp.Path);
        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            [
                "chunked-consumer-catalog-summary.json",
                "chunked-export-manifest.json",
                "chunked-preview-payload-caravan.json",
                "chunked-preview-payload-frontier.json",
                "chunked-preview-payload-gothic.json",
                "chunked-preview-payload-metamodule.json",
                "chunked-runtime-preview-export-multifamily-smoke-report.md",
                "infinite-chunked-world-smoke-proof.json",
                "invalid-chunked-consumer-diagnostics-matrix.json",
                "multi-family-world-scale-regression-matrix.json",
                "package-immutability-audit.json",
                "runtime-preview-consumption-proof.json"
            ],
            names);

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.CatalogSummaryJsonFileName)));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.ExportManifestJsonFileName)));
        using var audit = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.PackageImmutabilityAuditJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, ChunkedRuntimePreviewExportEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(4, catalog.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(catalog.RootElement.GetProperty("sourceGoal039RuntimeDeltasConsumed").GetBoolean());
        Assert.Equal(4, manifest.RootElement.GetProperty("payloads").GetArrayLength());
        Assert.True(audit.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("chunked_runtime_preview_export_multifamily_smoke_verification required", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("goal039AcceptedGate: runtime_chunk_delta_traversal_smoke_verification passed", report);
        Assert.Contains("No GamePackage schema/source definition, Runtime source contract, WinForms/UI, Unity entrypoint, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change", report);
    }

    [Fact]
    public async Task PackageImmutabilityAuditPasses()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();

        var audit = ChunkedRuntimePreviewExportTestFactory.CreateService().Build(temp.Path).PackageImmutabilityAudit;

        Assert.True(audit.Passed);
        Assert.False(audit.GamePackageDefinitionsMutated);
        Assert.False(audit.PublicPackageSchemaMutated);
        Assert.False(audit.RuntimeStateSourceContractsMutated);
        Assert.False(audit.UnityEntrypointsMutated);
        Assert.False(audit.WinFormsUiMutated);
        Assert.False(audit.ProviderLlmRagTouched);
        Assert.False(audit.LuaExecutionTouched);
        Assert.False(audit.GeneratorLibraryTouched);
    }
}
