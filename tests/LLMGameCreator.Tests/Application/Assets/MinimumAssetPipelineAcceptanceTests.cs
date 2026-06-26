using System.Text.Json;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.Assets;

public sealed class MinimumAssetPipelineAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedMinimumAssetPipelineArtifacts()
    {
        using var temp = new TempDirectory();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), temp.Path);
        var service = MinimumAssetPipelineAcceptanceTestFactory.CreateService();

        var first = service.BuildFromContentGeneration(temp.Path, ResolveAssetPackDirectory(), content);
        var second = service.BuildFromContentGeneration(temp.Path, ResolveAssetPackDirectory(), content);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("minimum_asset_pipeline_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal010GateRecorded);
        Assert.Equal(["S092", "S093", "S094", "S095", "S096", "S097", "S098"], first.Report.CompletedSlices);
        Assert.Equal(3, first.Report.GeneratedInputCount);
        Assert.Equal(3, first.Report.SourcePackCount);
        Assert.True(first.Report.TotalResolvedAssetSlots >= 90);
        Assert.True(first.Report.ValidMatrixPassed);
        Assert.True(first.Report.InvalidMatrixPassed);
        Assert.True(first.Report.PackageContentBindingPassed);
        Assert.True(first.Report.AssetValidationPassed);
        Assert.True(first.Report.ReplayEvidence.Passed);
        Assert.True(first.Report.VariationEvidence.Passed);
        Assert.Equal("minimum-asset-pipeline", first.Report.ProductSmokeRoute);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<MinimumAssetPipelineReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.True(roundTrip!.Accepted);
    }

    [Fact]
    public void ValidMatrixCoversRequiredCategoriesImportsFallbacksAndActualFiles()
    {
        using var temp = new TempDirectory();
        var report = BuildReport(temp.Path);

        Assert.True(report.CategoryCounts["tile_region_graphic"] >= 12);
        Assert.True(report.CategoryCounts["npc_portrait"] >= 12);
        Assert.True(report.CategoryCounts["item_icon_ui_graphic"] >= 12);
        Assert.True(report.CategoryCounts["sound_effect"] >= 12);
        Assert.True(report.CategoryCounts["music_ambience"] >= 3);
        Assert.True(report.ImportCountsByCategory.Values.Sum() > 0);
        Assert.True(report.FallbackCountsByCategory.Values.Sum() > 0);
        Assert.True(report.TotalByteCount > 0);
        Assert.NotEmpty(report.ManifestHash);
        Assert.All(report.Runs, run =>
        {
            Assert.True(run.Accepted);
            Assert.Equal(30, run.RequestCount);
            Assert.Equal(run.RequestCount, run.ResolvedAssetCount);
            Assert.True(run.PackageBindingAudit.Passed);
            Assert.True(run.PackageBindingAudit.PackageValidatorClean);
            Assert.Equal(run.ResolvedAssetCount, run.PackageBindingAudit.BoundAssetCount);
            Assert.True(run.AssetValidation.Passed);
            Assert.Equal(run.ResolvedAssetCount, run.AssetValidation.FilesChecked);
            Assert.True(run.ResolverEvidence.ResolverAvailable);
            Assert.All(run.ResolvedAssets, asset =>
            {
                Assert.False(Path.IsPathRooted(asset.RelativePath));
                Assert.DoesNotContain("..", asset.RelativePath, StringComparison.Ordinal);
                Assert.True(File.Exists(Path.Combine(temp.Path, asset.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
                Assert.NotEmpty(asset.Hash);
                Assert.True(asset.ByteCount > 0);
                Assert.Contains(asset.ResolutionKind, new[] { "import", "fallback" });
            });
        });
    }

    [Fact]
    public void InvalidFakeLeakAndExpectationOnlyScenariosAreRejectedCausally()
    {
        using var temp = new TempDirectory();
        var report = BuildReport(temp.Path);
        var invalid = report.InvalidMatrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(report.InvalidMatrix.Passed);
        Assert.True(report.InvalidMatrix.ScenarioCount >= 14);
        Assert.Contains(invalid["unknown_source_kind"].Diagnostics, item => item.Code == "asset_pipeline.source.kind");
        Assert.Contains(invalid["unsupported_media_type"].Diagnostics, item => item.Code == "asset_pipeline.source.media_type");
        Assert.Contains(invalid["missing_fixture_without_fallback_permission"].Diagnostics, item => item.Code == "asset_pipeline.fixture.missing");
        Assert.Contains(invalid["wrong_media_type_or_corrupt_fixture"].Diagnostics, item => item.Code == "asset_pipeline.fixture.media_type_mismatch");
        Assert.Contains(invalid["path_traversal_source"].Diagnostics, item => item.Code == "asset_pipeline.source.path_traversal");
        Assert.Contains(invalid["absolute_path_source"].Diagnostics, item => item.Code == "asset_pipeline.source.absolute_path");
        Assert.Contains(invalid["executable_script_provider_payload_injection"].Diagnostics, item => item.Code == "asset_pipeline.source.executable_payload" || item.Code == "asset_pipeline.source.command_payload");
        Assert.Contains(invalid["duplicate_slot_ids"].Diagnostics, item => item.Code == "asset_pipeline.request.duplicate_slot_id");
        Assert.Contains(invalid["unresolved_content_id"].Diagnostics, item => item.Code == "asset_pipeline.binding.unresolved_content_id");
        Assert.Contains(invalid["mismatched_file_hash"].Diagnostics, item => item.Code == "asset_pipeline.validation.hash_mismatch");
        Assert.Contains(invalid["tampered_package_content_hash"].Diagnostics, item => item.Code == "asset_pipeline.validation.package_content_hash_mismatch");
        Assert.Contains(invalid["over_budget_request"].Diagnostics, item => item.Code == "asset_pipeline.request.over_budget");
        Assert.Contains(invalid["cross_pack_asset_leakage"].Diagnostics, item => item.Code == "asset_pipeline.validation.cross_pack_asset_leakage");
        Assert.Contains(invalid["copied_expectation_report_without_files"].Diagnostics, item => item.Code == "asset_pipeline.invalid.expectation_only_mutation_present");
        Assert.Contains(invalid["unavailable_default_resolver"].Diagnostics, item => item.Code == "asset_pipeline.resolver_unavailable");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void DefaultUnavailableResolverCannotSatisfyAcceptance()
    {
        using var temp = new TempDirectory();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), temp.Path);

        var result = new MinimumAssetPipelineAcceptanceService()
            .BuildFromContentGeneration(temp.Path, ResolveAssetPackDirectory(), content);

        Assert.False(result.Report.Accepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "asset_pipeline.resolver_unavailable");
    }

    [Fact]
    public void RemovingExpectationOnlyMutationMakesExpectedInvalidMatrixFail()
    {
        using var temp = new TempDirectory();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), temp.Path);

        var result = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(
                temp.Path,
                ResolveAssetPackDirectory(),
                content,
                new MinimumAssetPipelineAcceptanceOptions { IncludeExpectationOnlyInvalidMutation = false });

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.InvalidMatrixPassed);
        var scenario = result.Report.InvalidMatrix.Scenarios.Single(item => item.ScenarioId == "copied_expectation_report_without_files");
        Assert.True(scenario.ActualValid);
        Assert.DoesNotContain(scenario.Diagnostics, item => item.Severity == "error");
    }

    private static MinimumAssetPipelineReport BuildReport(string projectRoot)
    {
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), projectRoot);
        return MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, ResolveAssetPackDirectory(), content)
            .Report;
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
