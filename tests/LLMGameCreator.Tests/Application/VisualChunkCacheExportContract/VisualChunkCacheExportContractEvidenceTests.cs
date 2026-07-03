using System.Text.Json;
using LLMGameCreator.Application.Design.VisualChunkCacheExportContract;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualChunkCacheExportContract;

public sealed class VisualChunkCacheExportContractEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new VisualChunkCacheExportEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.ManifestJson, second.ManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.RuntimeHandoffSidecarJson, second.RuntimeHandoffSidecarJson);
        Assert.Equal(first.InvalidationMatrixJson, second.InvalidationMatrixJson);
        Assert.Equal(first.ReadbackProofJson, second.ReadbackProofJson);
        Assert.Equal(first.OverlapReuseProofJson, second.OverlapReuseProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.Equal(4, first.Manifest.PackageCount);
        Assert.True(first.Manifest.ExportRecordCount > 0);
        Assert.False(first.Manifest.Accepted);
        Assert.Equal(VisualChunkCacheExportContractVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("visual_chunk_cache_export_contract_verification required", first.ReportMarkdown);
        Assert.True(first.ReadbackProof.Passed);
        Assert.True(first.OverlapReuseProof.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.Passed);
    }

    [Fact]
    public void RequiredExportPackagesHaveRelativeMetadataOnlyRecords()
    {
        var evidence = new VisualChunkCacheExportEvidenceService().Build(FindRepoRoot());

        AssertPackage(evidence, VisualChunkCacheExportContractVocabulary.FinitePackageId);
        AssertPackage(evidence, VisualChunkCacheExportContractVocabulary.HugeSparsePackageId);
        AssertPackage(evidence, VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId);
        AssertPackage(evidence, VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId);

        Assert.All(evidence.Manifest.Packages.SelectMany(item => item.ArtifactRefs), artifact =>
        {
            Assert.False(Path.IsPathFullyQualified(artifact.RelativePath), artifact.RelativePath);
            Assert.False(artifact.IsBinaryOrRaster, artifact.RelativePath);
            Assert.False(artifact.IsPromptDump, artifact.RelativePath);
            Assert.Equal(64, artifact.Sha256.Length);
        });

        Assert.All(evidence.Manifest.Packages.SelectMany(item => item.Records), record =>
        {
            Assert.False(string.IsNullOrWhiteSpace(record.CacheKey.ChunkKey));
            Assert.Equal(64, record.CacheKey.ChunkKey.Length);
            Assert.False(string.IsNullOrWhiteSpace(record.ChunkHash));
            Assert.Equal(64, record.ChunkHash.Length);
            Assert.NotEmpty(record.StreamWindowIds);
            Assert.False(record.ContainsRawFullWorldCellDump);
            Assert.False(record.PromptTextIsSourceOfTruth);
        });

        var huge = evidence.Manifest.Packages.Single(item => item.PackageId == VisualChunkCacheExportContractVocabulary.HugeSparsePackageId);
        Assert.True(huge.EstimatedFullWorldChunkCapacity > huge.ExportedRecordCount);
        Assert.True(huge.OnlyMaterializedChunksExported);

        var sidecar = evidence.RuntimeHandoffSidecar;
        Assert.True(sidecar.MetadataOnly);
        Assert.False(sidecar.ContainsRuntimeExecution);
        Assert.False(sidecar.ContainsProviderCalls);
        Assert.False(sidecar.ContainsUnityImplementation);
        Assert.Equal(VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId, sidecar.PackageId);
    }

    [Fact]
    public void OverlapReuseAndNegativeMatrixProtectExportContract()
    {
        var evidence = new VisualChunkCacheExportEvidenceService().Build(FindRepoRoot());

        Assert.True(evidence.OverlapReuseProof.Passed);
        Assert.Equal(24, evidence.OverlapReuseProof.SourceGoal091ReusedChunkKeyCount);
        Assert.Equal(evidence.OverlapReuseProof.SourceGoal091ReusedChunkKeyCount, evidence.OverlapReuseProof.ExportReusedChunkKeyCount);
        Assert.All(evidence.OverlapReuseProof.Rows, row => Assert.True(row.StreamWindowIds.Count > 1));

        var proof = evidence.NegativeProof;
        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        AssertScenarioHasCode(proof, "unknown_source_chunk_key", "visual_chunk_cache.chunk_key.unknown_source");
        AssertScenarioHasCode(proof, "absolute_artifact_path", "visual_chunk_cache.artifact_ref.absolute_path");
        AssertScenarioHasCode(proof, "missing_chunk_hash", "visual_chunk_cache.chunk_hash.missing");
        AssertScenarioHasCode(proof, "duplicate_chunk_key_conflicting_hash", "visual_chunk_cache.chunk_key.conflicting_hash");
        AssertScenarioHasCode(proof, "stream_window_membership_mismatch", "visual_chunk_cache.stream_window_membership.mismatch");
        AssertScenarioHasCode(proof, "raw_full_world_dump", "visual_chunk_cache.raw_full_world_dump.forbidden");
        AssertScenarioHasCode(proof, "missing_goal090_lineage", "visual_chunk_cache.source_lineage.goal090.missing");
        AssertScenarioHasCode(proof, "stale_generator_version", "visual_chunk_cache.generator_version.stale");
        AssertScenarioHasCode(proof, "unknown_invalidation_key", "visual_chunk_cache.invalidation_key.unknown");
        AssertScenarioHasCode(proof, "runtime_handoff_provider_call", "visual_chunk_cache.sidecar.provider_call.forbidden");
        AssertScenarioHasCode(proof, "prompt_text_source_of_truth", "visual_chunk_cache.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "rating_metadata_without_safe_fallback", "visual_chunk_cache.rating.safe_fallback_missing");
        AssertScenarioHasCode(proof, "binary_raster_artifact_ref", "visual_chunk_cache.artifact_ref.binary_raster");
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndReadBack()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new VisualChunkCacheExportEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.RuntimeHandoffSidecarJsonPath));
        Assert.True(File.Exists(write.InvalidationMatrixJsonPath));
        Assert.True(File.Exists(write.ReadbackProofJsonPath));
        Assert.True(File.Exists(write.OverlapReuseProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.ManifestJsonPath));
        using var sidecar = JsonDocument.Parse(await File.ReadAllTextAsync(write.RuntimeHandoffSidecarJsonPath));
        using var readback = JsonDocument.Parse(await File.ReadAllTextAsync(write.ReadbackProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(4, manifest.RootElement.GetProperty("packageCount").GetInt32());
        Assert.True(sidecar.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(readback.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
    }

    private static void AssertPackage(VisualChunkCacheEvidenceResult evidence, string packageId)
    {
        var package = evidence.Manifest.Packages.Single(item => item.PackageId == packageId);
        Assert.NotEmpty(package.Records);
        Assert.NotEmpty(package.StreamWindows);
        Assert.True(package.NoRawFullWorldDump);
        Assert.True(package.OnlyMaterializedChunksExported);
        Assert.True(package.MetadataOnly);
    }

    private static void AssertScenarioHasCode(VisualChunkCacheNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
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
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
