using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshHandoffTests
{
    [Fact]
    public async Task StagedHandoffManifestIsReadAndTamperProofRejectsMissingOrChangedData()
    {
        var service = new EditDrivenPlayablePreviewRefreshEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var manifestPath = Path.Combine(write.OutputDirectoryPath, EditDrivenPlayablePreviewRefreshEvidenceService.UnityPlayerHandoffManifestFileName);

        var proof = service.ReadStagedPlayerHandoffManifest(
            manifestPath,
            write.Result.Report.HandoffManifestHash,
            write.Result.Report.SourceGoal075ReportHash,
            write.Result.Report.PreviewRefreshHash);

        Assert.True(proof.Passed);
        Assert.True(proof.ManifestLoaded);
        Assert.True(proof.HashMatched);
        Assert.True(proof.SourceHashMatched);
        Assert.True(proof.PackageTargetsPresent);
        Assert.Equal(9, proof.RowCount);
        Assert.True(write.Result.TamperNegativeProof.Passed);
        Assert.Contains(write.Result.TamperNegativeProof.Scenarios, item => item.ScenarioId == "missing_staged_handoff_manifest" && item.ActualStatus == "rejected");
        Assert.Contains(write.Result.TamperNegativeProof.Scenarios, item => item.ScenarioId == "tampered_staged_handoff_manifest" && item.ActualStatus == "rejected");

        var missingProof = service.ReadStagedPlayerHandoffManifest(
            Path.Combine(write.OutputDirectoryPath, "missing-unity-player-handoff-manifest.json"),
            write.Result.Report.HandoffManifestHash,
            write.Result.Report.SourceGoal075ReportHash,
            write.Result.Report.PreviewRefreshHash);
        Assert.False(missingProof.Passed);
        Assert.Contains(missingProof.Diagnostics, diagnostic => diagnostic.Code == "goal076.handoff.manifest_missing");

        var tamperedPath = Path.Combine(Path.GetTempPath(), "llmgc-goal076-tampered-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var tampered = File.ReadAllText(manifestPath).Replace(
                write.Result.Report.SourceGoal075ReportHash,
                new string('0', write.Result.Report.SourceGoal075ReportHash.Length),
                StringComparison.Ordinal);
            File.WriteAllText(tamperedPath, tampered);
            var tamperedProof = service.ReadStagedPlayerHandoffManifest(
                tamperedPath,
                write.Result.Report.HandoffManifestHash,
                write.Result.Report.SourceGoal075ReportHash,
                write.Result.Report.PreviewRefreshHash);
            Assert.False(tamperedProof.Passed);
            Assert.Contains(tamperedProof.Diagnostics, diagnostic => diagnostic.Code == "goal076.handoff.manifest_hash_mismatch");
            Assert.Contains(tamperedProof.Diagnostics, diagnostic => diagnostic.Code == "goal076.handoff.source_hash_mismatch");
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
