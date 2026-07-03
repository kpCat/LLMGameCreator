using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldUnityPreviewRunner;

public sealed class OfflineGeoworldUnityPreviewRunnerTests
{
    [Fact]
    public async Task ServiceBuildsPreviewCommandsPayloadAndEvidenceFromGoal100Artifacts()
    {
        var write = await new OfflineGeoworldUnityPreviewRunnerEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldUnityPreviewRunnerVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.StreamingAssetsLedger.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.SimulatedCommandProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.Equal(18, result.Manifest.CommandCount);
        Assert.Equal(10, result.Manifest.CommandKindCount);
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.True(result.Manifest.TravelWindowStepCount >= 4);
        Assert.Equal(10, result.StyleLegend.StyleCount);
        Assert.Equal(18, result.CommandCatalog.CommandCount);
        Assert.Equal(18, result.CommandCatalog.ExpectedObjectCount);

        foreach (var kind in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds)
        {
            Assert.True(result.CommandCatalog.CommandCountByKind.ContainsKey(kind), kind);
            Assert.True(result.SimulatedCommandProof.CommandCountByKind.ContainsKey(kind), kind);
        }

        Assert.All(result.CommandCatalog.Commands, command =>
        {
            Assert.False(string.IsNullOrWhiteSpace(command.CommandId), command.SourceCacheRecordId);
            Assert.False(string.IsNullOrWhiteSpace(command.SourceCacheRecordId), command.CommandId);
            Assert.False(string.IsNullOrWhiteSpace(command.SourceFeatureId), command.CommandId);
            Assert.False(string.IsNullOrWhiteSpace(command.SourceChunkKey), command.CommandId);
            Assert.True(command.MetadataOnly, command.CommandId);
            Assert.False(command.RawGeodataIncluded, command.CommandId);
            Assert.Contains("safe_public_geoworld_fallback", command.SafeRatingMetadataStatus);
        });

        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedCommandProofAndNegativeProofCoverRequiredRisks()
    {
        var result = (await new OfflineGeoworldUnityPreviewRunnerEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedCommandProof.PayloadReadAttempted);
        Assert.True(result.SimulatedCommandProof.ManifestRead);
        Assert.True(result.SimulatedCommandProof.CommandFileRead);
        Assert.True(result.SimulatedCommandProof.StyleLegendRead);
        Assert.True(result.SimulatedCommandProof.TravelWindowScriptRead);
        Assert.True(result.SimulatedCommandProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedCommandProof.AllRequiredCommandKindsRepresented);
        Assert.True(result.SimulatedCommandProof.NoUnsupportedCommandKind);
        Assert.True(result.SimulatedCommandProof.NoAbsolutePaths);
        Assert.True(result.SimulatedCommandProof.NoRawGeodata);
        Assert.True(result.SimulatedCommandProof.NoBinaryOrRasterMedia);
        Assert.True(result.SimulatedCommandProof.NoProviderOrNetworkMarkers);
        Assert.Equal(18, result.SimulatedCommandProof.CommandCount);
        Assert.Equal(10, result.SimulatedCommandProof.CommandKindCount);
        Assert.True(result.SimulatedCommandProof.TravelWindowStepCount >= 4);

        Assert.Equal(
            OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        foreach (var scenarioId in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task UnityScriptInventoryAndWorkspaceGroupAreReal()
    {
        await new OfflineGeoworldUnityPreviewRunnerEvidenceService().BuildAndWriteAsync(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_unity_preview");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_unity_preview_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewGroupPresent);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityPreviewCommandCount);
        Assert.Equal(10, result.QualityGateScan.OfflineGeoworldUnityPreviewCommandKindCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewTravelWindowStepCount >= 4);
        Assert.Equal(5, result.QualityGateScan.OfflineGeoworldUnityPreviewUnityPayloadFileCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal101FilesDiscoveredByRelativePaths);
        Assert.Equal(18, summary.OfflineGeoworldUnityPreviewCommandCount);
        Assert.Contains("building_footprint_marker=1", summary.OfflineGeoworldUnityPreviewKindCoverageSummary);
        AssertProofPassed(result.ProofStatus.Proofs, "goal101.simulated_command");
        AssertProofPassed(result.ProofStatus.Proofs, "goal101.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal101.all_command_kinds_mapped");
        AssertProofPassed(result.ProofStatus.Proofs, "goal101.travel_window_demo");
        AssertProofPassed(result.ProofStatus.Proofs, "goal101.quality_gate");
    }

    [Fact]
    public async Task EvidenceIsDeterministicAndSourceHealthClean()
    {
        var service = new OfflineGeoworldUnityPreviewRunnerEvidenceService();
        var first = await service.BuildAndWriteAsync(ProjectRoot());
        var second = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal(first.Result.PayloadJsonByFileName, second.Result.PayloadJsonByFileName);
        Assert.Equal(first.Result.Report.DeterministicReportHash, second.Result.Report.DeterministicReportHash);
        Assert.True(second.Result.QualityGateScan.Passed);
        Assert.Equal(0, second.Result.QualityGateScan.FilesOver1000LogicalLinesCount);
        Assert.Equal(0, second.Result.QualityGateScan.FilesOver700LogicalLinesCount);
        Assert.True(second.Result.QualityGateScan.MaxLogicalLineCount <= 700);
    }

    private static void AssertProofPassed(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        string proofId)
    {
        var proof = Assert.Single(proofs, item => item.ProofId == proofId);
        Assert.True(proof.Passed, proof.DiagnosticSummary);
        Assert.False(Path.IsPathFullyQualified(proof.RelativePath), proof.RelativePath);
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
