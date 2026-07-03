using LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualChunkCacheUnityStreamingAssetsHandoff;

public sealed class VisualChunkCacheUnityStreamingAssetsHandoffEvidenceTests
{
    [Fact]
    public async Task ServiceBuildsCompactStreamingAssetsPayloadFromGoal093AndGoal094()
    {
        var service = new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService();
        var first = await service.BuildAndWriteAsync(ProjectRoot());
        var second = await service.BuildAndWriteAsync(ProjectRoot());
        var result = first.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.StreamingAssetsLedger.Passed);
        Assert.True(result.SimulatedReadProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.ProbeSourceInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(4, result.Report.PackageCount);
        Assert.Equal(93, result.Report.ExportRecordCount);
        Assert.Equal(5, result.Report.StreamWindowCount);
        Assert.Equal(93, result.Report.UniqueChunkKeyCount);
        Assert.Equal(result.Report.DeterministicHash, second.Result.Report.DeterministicHash);

        foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(first.OutputDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(first.StreamingAssetsDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(first.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedUnityReadProofReadsMirroredPayloadAndMatchesSourceCounts()
    {
        var result = (await new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedReadProof.PayloadReadAttempted);
        Assert.True(result.SimulatedReadProof.ManifestRead);
        Assert.True(result.SimulatedReadProof.RequiredPayloadFilesPresent);
        Assert.True(result.SimulatedReadProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedReadProof.PackageCountMatchesGoal093AndGoal094);
        Assert.True(result.SimulatedReadProof.StreamWindowsRepresented);
        Assert.True(result.SimulatedReadProof.ChunkKeysRepresented);
        Assert.True(result.SimulatedReadProof.RuntimeHandoffSidecarMetadataOnly);
        Assert.True(result.SimulatedReadProof.NoRawFullWorldDump);
        Assert.True(result.SimulatedReadProof.NoAbsolutePaths);
        Assert.True(result.SimulatedReadProof.NoBinaryOrRasterMedia);
        Assert.Equal(result.Report.PackageCount, result.SimulatedReadProof.PackageCount);
        Assert.Equal(result.Report.ExportRecordCount, result.SimulatedReadProof.ExportRecordCount);
        Assert.Equal(result.Report.StreamWindowCount, result.SimulatedReadProof.StreamWindowCount);
        Assert.Equal(result.Report.UniqueChunkKeyCount, result.SimulatedReadProof.UniqueChunkKeyCount);
    }

    [Fact]
    public async Task NegativeProofRejectsMissingTamperedUnsafeAndFakeReadScenarios()
    {
        var result = (await new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.NegativeProof.Passed);
        foreach (var scenarioId in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredNegativeScenarioIds)
        {
            AssertScenarioRejected(result, scenarioId);
        }
    }

    [Fact]
    public async Task UnityProbeSourceAndAlphaBootstrapBaselineAreGuarded()
    {
        var result = (await new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;
        var probePath = Path.Combine(
            ProjectRoot(),
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "Scripts",
            "VisualChunkCacheHandoffProbe.cs");
        var probe = await File.ReadAllTextAsync(probePath);

        Assert.True(result.ProbeSourceInventory.UsesApplicationStreamingAssetsPath);
        Assert.True(result.ProbeSourceInventory.UsesExpectedPayloadRoot);
        Assert.True(result.ProbeSourceInventory.ExposesInspectorResultFields);
        Assert.True(result.ProbeSourceInventory.DoesNotReferenceAlphaRuntimeBootstrap);
        Assert.True(result.ProbeSourceInventory.HasNoProviderLlmNetworkMarkers);
        Assert.Contains("Application.streamingAssetsPath", probe);
        Assert.Contains("LLMGameCreator/VisualChunkCacheGoal095", probe);
        Assert.DoesNotContain("AlphaRuntimeBootstrap", probe);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(
            VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
            result.QualityGateScan.AlphaRuntimeBootstrapAfterHash);
    }

    [Fact]
    public async Task PayloadPathsStayRelativeCompactAndMetadataOnly()
    {
        var result = (await new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.HandoffManifest.NoAbsolutePaths);
        Assert.True(result.HandoffManifest.NoRawFullWorldDump);
        Assert.True(result.HandoffManifest.NoBinaryOrRasterMedia);
        Assert.True(result.HandoffManifest.NoPromptDumps);
        Assert.True(result.HandoffManifest.RuntimeHandoffSidecarMetadataOnly);
        Assert.False(result.HandoffManifest.ContainsRuntimeExecution);
        Assert.False(result.HandoffManifest.ContainsProviderCalls);
        Assert.False(result.HandoffManifest.ContainsUnityGameplayImplementation);
        Assert.True(result.ChunkKeyLedger.CompactMetadataOnly);
        Assert.Equal(0, result.QualityGateScan.FilesOver700LogicalLinesCount);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LogicalLinesCount);
    }

    private static void AssertScenarioRejected(
        VisualChunkCacheUnityBuildResult result,
        string scenarioId)
    {
        var scenario = Assert.Single(result.NegativeProof.Scenarios, item => item.ScenarioId == scenarioId);
        Assert.Equal("rejected", scenario.ActualStatus);
        Assert.NotEmpty(scenario.Diagnostics);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
