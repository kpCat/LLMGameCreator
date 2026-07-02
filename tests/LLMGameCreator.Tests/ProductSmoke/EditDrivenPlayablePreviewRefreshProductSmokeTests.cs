using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenPlayablePreviewRefreshProductSmokeTests
{
    [Fact]
    public async Task Goal076EditDrivenPlayablePreviewRefreshEvidenceIsProducedForReview()
    {
        var service = new EditDrivenPlayablePreviewRefreshEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal("GREEN", write.Result.Report.ImplementationStatus);
        Assert.False(write.Result.Report.Accepted);
        Assert.True(write.Result.StateTransitionProof.Passed);
        Assert.True(write.Result.GamePackageRefreshPlan.Passed);
        Assert.True(write.Result.StagedHandoffProof.Passed);
        Assert.True(write.Result.TamperNegativeProof.Passed);
        Assert.True(write.Result.WinFormsBindingInventory.Passed);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.Equal(EditDrivenPlayablePreviewRefreshVocabulary.FinalGate, write.Result.Report.ManualGate);
        Assert.Equal(9, write.Result.StateTransitionProof.RowCount);
        Assert.Equal(18, write.Result.GamePackageRefreshPlan.TargetCount);
        Assert.NotEqual(write.Result.Report.BeforeStateHash, write.Result.Report.AfterStateHash);
        Assert.Equal(write.Result.Report.BeforeStateHash, write.Result.Report.RollbackStateHash);
        Assert.Equal(write.Result.Report.AfterStateHash, write.Result.Report.ReplayStateHash);

        foreach (var fileName in EditDrivenPlayablePreviewRefreshEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        var manifestPath = Path.Combine(write.OutputDirectoryPath, EditDrivenPlayablePreviewRefreshEvidenceService.UnityPlayerHandoffManifestFileName);
        var proof = service.ReadStagedPlayerHandoffManifest(
            manifestPath,
            write.Result.Report.HandoffManifestHash,
            write.Result.Report.SourceGoal075ReportHash,
            write.Result.Report.PreviewRefreshHash);
        Assert.True(proof.Passed);

        var missing = service.ReadStagedPlayerHandoffManifest(
            Path.Combine(write.OutputDirectoryPath, "missing-handoff-manifest.json"),
            write.Result.Report.HandoffManifestHash,
            write.Result.Report.SourceGoal075ReportHash,
            write.Result.Report.PreviewRefreshHash);
        Assert.False(missing.Passed);

        var tamperedPath = Path.Combine(Path.GetTempPath(), "llmgc-goal076-product-smoke-tampered-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(tamperedPath, File.ReadAllText(manifestPath).Replace(
                write.Result.Report.SourceGoal075ReportHash,
                new string('f', write.Result.Report.SourceGoal075ReportHash.Length),
                StringComparison.Ordinal));
            var tampered = service.ReadStagedPlayerHandoffManifest(
                tamperedPath,
                write.Result.Report.HandoffManifestHash,
                write.Result.Report.SourceGoal075ReportHash,
                write.Result.Report.PreviewRefreshHash);
            Assert.False(tampered.Passed);
        }
        finally
        {
            File.Delete(tamperedPath);
        }
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
