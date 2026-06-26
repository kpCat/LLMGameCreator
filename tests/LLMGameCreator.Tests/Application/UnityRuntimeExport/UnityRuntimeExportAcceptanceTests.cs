using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnityRuntimeExport;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityRuntimeExport;

public sealed class UnityRuntimeExportAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedUnityRuntimeExportArtifacts()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var service = new UnityRuntimeExportAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets);
        var second = service.BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal("unity_runtime_export_vertical_slice_artifact_verification", first.Report.ManualGate);
        Assert.Equal(["S099", "S100", "S101", "S102", "S103", "S104", "S105"], first.Report.CompletedSlices);
        Assert.True(first.Report.Goal011GateRecorded);
        Assert.Equal("unity-runtime-export", first.Report.ProductSmokeRoute);
        Assert.NotEmpty(first.Report.SelectedInput.PackageId);
        Assert.NotEmpty(first.Report.SelectedInput.SourcePackageHash);
        Assert.NotEmpty(first.Report.SelectedInput.AssetManifestHash);
        Assert.True(first.Report.ExportFileCount >= 6);
        Assert.True(first.Report.ExportByteCount > 0);
        Assert.True(first.Report.ValidMatrixPassed);
        Assert.True(first.Report.InvalidMatrixPassed);
        Assert.True(first.Report.PackageValidationPassed);
        Assert.True(first.Report.AssetManifestValidationPassed);
        Assert.True(first.Report.ExportManifestValidationPassed);
        Assert.True(first.Report.SelectedLoopResolutionPassed);
        Assert.True(first.Report.ReplayEvidence.Passed);
        Assert.True(first.Report.VariationEvidence.Passed);
        Assert.False(first.Report.WindowsExecutableProduced);
        Assert.False(first.Report.UnityEditorExecuted);
        Assert.False(first.Report.RuntimePreviewDependency);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.False(first.Report.ExternalExecution.AnyExecuted());
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "game-data", "game-package.json")));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "assets", "asset-manifest.json")));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "runtime", "unity-runtime-config.json")));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "runtime", "launch-metadata.json")));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "export-manifest.json")));

        var roundTrip = JsonSerializer.Deserialize<UnityRuntimeExportReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.True(roundTrip!.Accepted);
    }

    [Fact]
    public void ContractCoversSelectedLoopAndAssetRefsOutsideRuntimePreview()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var report = new UnityRuntimeExportAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets)
            .Report;

        Assert.Equal("generic_unity_runtime", report.ContractValidation.Diagnostics.All(item => item.Code != "unity_runtime_export.contract.runtime_preview_dependency")
            ? "generic_unity_runtime"
            : "winforms_runtime_preview");
        Assert.NotEmpty(report.SelectedInput.SelectedThreadId);
        Assert.NotEmpty(report.SelectedInput.SelectedGeneratedIds);
        Assert.NotEmpty(report.SelectedInput.SelectedRuntimeCommands);
        foreach (var category in new[] { "tile_region_graphic", "npc_portrait", "item_icon_ui_graphic", "sound_effect", "music_ambience" })
        {
            var asset = Assert.Single(report.SelectedInput.SelectedAssetRefs, item => item.Category == category);
            Assert.False(Path.IsPathRooted(asset.ExportRelativePath));
            Assert.DoesNotContain("..", asset.ExportRelativePath, StringComparison.Ordinal);
            Assert.NotEmpty(asset.Hash);
            Assert.True(asset.ByteCount > 0);
        }

        Assert.True(report.ContractValidation.RuntimePreviewDependencyFree);
        Assert.True(report.ContractValidation.ExternalExecutionFlagsFalse);
        Assert.False(report.ContractValidation.WindowsExecutableProduced);
        Assert.False(report.ContractValidation.UnityEditorExecuted);
    }

    [Fact]
    public void InvalidFakeLeakAndExpectationOnlyScenariosAreRejectedCausally()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var report = new UnityRuntimeExportAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets)
            .Report;
        var invalid = report.InvalidMatrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(report.InvalidMatrix.Passed);
        Assert.True(report.InvalidMatrix.ScenarioCount >= 14);
        Assert.Contains(invalid["missing_prior_package_evidence"].Diagnostics, item => item.Code == "unity_runtime_export.contract.package_hash_mismatch");
        Assert.Contains(invalid["missing_prior_asset_manifest_evidence"].Diagnostics, item => item.Code == "unity_runtime_export.contract.asset_manifest_hash_mismatch");
        Assert.Contains(invalid["package_hash_mismatch"].Diagnostics, item => item.Code == "unity_runtime_export.contract.package_hash_mismatch");
        Assert.Contains(invalid["asset_manifest_hash_mismatch"].Diagnostics, item => item.Code == "unity_runtime_export.contract.asset_manifest_hash_mismatch");
        Assert.Contains(invalid["unresolved_package_id"].Diagnostics, item => item.Code == "unity_runtime_export.contract.start_map_unresolved");
        Assert.Contains(invalid["unresolved_asset_id"].Diagnostics, item => item.Code == "unity_runtime_export.contract.asset_ref_unresolved");
        Assert.Contains(invalid["missing_exported_file"].Diagnostics, item => item.Code == "unity_runtime_export.contract.exported_asset_file_missing");
        Assert.Contains(invalid["mismatched_exported_file_hash"].Diagnostics, item => item.Code == "unity_runtime_export.contract.exported_asset_hash_mismatch");
        Assert.Contains(invalid["path_traversal_export_path"].Diagnostics, item => item.Code == "unity_runtime_export.contract.unsafe_export_path");
        Assert.Contains(invalid["absolute_export_path"].Diagnostics, item => item.Code == "unity_runtime_export.contract.unsafe_export_path");
        Assert.Contains(invalid["executable_script_provider_payload_injection"].Diagnostics, item => item.Code == "unity_runtime_export.contract.executable_payload_injection");
        Assert.Contains(invalid["copied_expectation_report_without_files"].Diagnostics, item => item.Code == "unity_runtime_export.contract.exported_asset_file_missing");
        Assert.Contains(invalid["runtime_preview_only_dependency"].Diagnostics, item => item.Code == "unity_runtime_export.contract.runtime_preview_dependency");
        Assert.Contains(invalid["unity_editor_build_claim_without_artifact"].Diagnostics, item => item.Code == "unity_runtime_export.contract.unity_editor_claim_without_artifact");
        Assert.Contains(invalid["cross_pack_or_cross_asset_leakage"].Diagnostics, item => item.Code == "unity_runtime_export.contract.asset_ref_unresolved");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void RemovingExpectationOnlyMutationMakesExpectedInvalidMatrixFail()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);

        var report = new UnityRuntimeExportAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                evidence.Content,
                evidence.Assets,
                new UnityRuntimeExportOptions { IncludeExpectationOnlyInvalidMutation = false })
            .Report;

        Assert.False(report.Accepted);
        Assert.False(report.InvalidMatrixPassed);
        var scenario = report.InvalidMatrix.Scenarios.Single(item => item.ScenarioId == "copied_expectation_report_without_files");
        Assert.True(scenario.ActualValid);
        Assert.DoesNotContain(scenario.Diagnostics, item => item.Severity == "error");
    }

    private static (ContentGenerationScaleAcceptanceResult Content, MinimumAssetPipelineAcceptanceResult Assets) BuildPriorEvidence(string projectRoot)
    {
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, ResolveAssetPackDirectory(), content);
        return (content, assets);
    }

    private static string ResolveContentPackDirectory() =>
        Path.Combine(FindRepoRoot(), "samples", "content-generation-packs");

    private static string ResolveAssetPackDirectory() =>
        Path.Combine(FindRepoRoot(), "samples", "minimum-asset-pipeline");

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
