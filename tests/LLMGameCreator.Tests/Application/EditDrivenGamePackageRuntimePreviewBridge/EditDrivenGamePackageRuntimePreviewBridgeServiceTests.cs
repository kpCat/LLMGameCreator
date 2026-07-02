using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenGamePackageRuntimePreviewBridge;

public sealed class EditDrivenGamePackageRuntimePreviewBridgeServiceTests
{
    [Fact]
    public async Task ServiceBuildsDiskBackedProjectedGamePackageAndRuntimePreviewProof()
    {
        var service = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal079AcceptedForContinuation);
        Assert.True(result.SourceArtifactManifest.Goal079ASourceFormatGuardPassedByHandoff);
        Assert.True(result.ProjectedPackageFileLedger.Passed);
        Assert.True(result.RuntimePreviewBridgeProof.Passed);
        Assert.True(result.RuntimePreviewNegativeProof.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(9, result.Report.RowCount);
        Assert.Equal(18, result.Report.TargetCount);
        Assert.Equal(57, result.Report.ActionCount);
        Assert.Equal(5, result.ProjectedPackageFileLedger.FileCount);

        foreach (var fileName in EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RequiredArtifactFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var entry in result.ProjectedPackageFileLedger.Files)
        {
            var path = Path.Combine(write.OutputDirectoryPath, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), entry.RelativePath);
            Assert.Equal(entry.Sha256, Sha256(File.ReadAllText(path)));
        }
    }

    [Fact]
    public void RuntimePreviewProofCoversEveryGoal077TargetAndGoal078TargetAction()
    {
        var result = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService().Build(ProjectRoot());
        var proof = result.RuntimePreviewBridgeProof;

        Assert.True(proof.ProjectedPackagePayloadRead);
        Assert.True(proof.GamePackageValidationPassed);
        Assert.True(proof.RuntimePreviewProjectionPassed);
        Assert.True(proof.InteractionCatalogProjectionPassed);
        Assert.True(proof.AllGoal077TargetsCovered);
        Assert.True(proof.AllGoal078ActionsCovered);
        Assert.Equal(3, proof.RuntimePreviewRegionCount);
        Assert.Equal(9, proof.RuntimePreviewNpcCount);
        Assert.Equal(18, proof.RuntimePreviewItemCount);
        Assert.Equal(9, proof.RuntimePreviewQuestCount);
        Assert.Equal(18, proof.RuntimePreviewMechanicCount);
        Assert.Empty(proof.RuntimePreviewWarnings);
        Assert.False(string.IsNullOrWhiteSpace(proof.Goal078ReplayFinalStateHash));
    }

    [Fact]
    public void NegativeProofRejectsMissingTamperedFakeAndLineageScenarios()
    {
        var result = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService().Build(ProjectRoot());

        Assert.True(result.RuntimePreviewNegativeProof.Passed);
        AssertScenarioRejected(result, "missing_projected_package_file");
        AssertScenarioRejected(result, "tampered_projected_package_file");
        AssertScenarioRejected(result, "projected_index_missing_target");
        AssertScenarioRejected(result, "fake_success_without_projected_package_read");
        AssertScenarioRejected(result, "source_lineage_hash_mismatch");
    }

    private static void AssertScenarioRejected(
        EditDrivenGamePackageRuntimePreviewBridgeBuildResult result,
        string scenarioId)
    {
        var scenario = Assert.Single(result.RuntimePreviewNegativeProof.Scenarios, item => item.ScenarioId == scenarioId);
        Assert.Equal("rejected", scenario.ActualStatus);
    }

    private static string Sha256(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
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
