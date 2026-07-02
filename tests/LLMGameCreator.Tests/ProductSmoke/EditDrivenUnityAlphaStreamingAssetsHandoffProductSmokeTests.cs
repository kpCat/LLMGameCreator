using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests
{
    [Fact]
    public async Task Goal082ReadsMirroredStreamingAssetsPayloadAndRejectsFakeHandoffSuccess()
    {
        var service = new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.ProbeReadProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.CommandTranscriptProof.Passed);

        var manifest = ReadPayload<EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest>(
            write.StreamingAssetsDirectoryPath,
            "handoff-manifest.json");
        var expected = ReadPayload<EditDrivenUnityAlphaStreamingAssetsHandoffExpectedHashes>(
            write.StreamingAssetsDirectoryPath,
            "expected-hashes.json");
        var packageIndex = ReadPayload<EditDrivenUnityAlphaStreamingAssetsHandoffProjectedPackageIndexPayload>(
            write.StreamingAssetsDirectoryPath,
            "projected-package-index.json");
        var commandIndex = ReadPayload<EditDrivenUnityAlphaStreamingAssetsHandoffCommandIndexPayload>(
            write.StreamingAssetsDirectoryPath,
            "playthrough-command-index.json");
        var transcriptIndex = ReadPayload<EditDrivenUnityAlphaStreamingAssetsHandoffTranscriptIndexPayload>(
            write.StreamingAssetsDirectoryPath,
            "playthrough-transcript-index.json");

        Assert.Equal(6, manifest.PayloadFileCount);
        Assert.Equal(result.Report.ProjectedPackageHash, manifest.ProjectedPackageHash);
        Assert.Equal(result.Report.ProjectedPackageHash, expected.ProjectedPackageHash);
        Assert.Equal(result.Report.ProjectedPackageHash, packageIndex.ProjectedPackageHash);
        Assert.Equal(result.Report.CommandScriptHash, expected.Goal081CommandScriptHash);
        Assert.Equal(result.Report.CommandScriptHash, commandIndex.CommandScriptHash);
        Assert.Equal(result.Report.TranscriptHash, expected.Goal081TranscriptHash);
        Assert.Equal(result.Report.TranscriptHash, transcriptIndex.TranscriptHash);
        Assert.Equal(result.Report.StateHashChainHash, expected.Goal081StateHashChainHash);
        Assert.Equal(result.Report.StateHashChainHash, transcriptIndex.StateHashChainHash);
        Assert.Equal(9, manifest.RowCount);
        Assert.Equal(18, manifest.TargetCount);
        Assert.Equal(57, manifest.Goal078ActionCount);
        Assert.Equal(124, manifest.CommandCount);
        Assert.Equal(manifest.RowCount, commandIndex.RowCount);
        Assert.Equal(manifest.TargetCount, transcriptIndex.CoveredTargetCount);
        Assert.Equal(manifest.Goal078ActionCount, transcriptIndex.CoveredGoal078ActionCount);

        Assert.Equal(
            expected.ProjectedPackageIndexPayloadHash,
            HashFile(Path.Combine(write.StreamingAssetsDirectoryPath, "projected-package-index.json")));
        Assert.Equal(
            expected.PlaythroughCommandIndexPayloadHash,
            HashFile(Path.Combine(write.StreamingAssetsDirectoryPath, "playthrough-command-index.json")));
        Assert.Equal(
            expected.PlaythroughTranscriptIndexPayloadHash,
            HashFile(Path.Combine(write.StreamingAssetsDirectoryPath, "playthrough-transcript-index.json")));
        AssertRejected(result, "missing_handoff_manifest");
        AssertRejected(result, "tampered_projected_package_index");
        AssertRejected(result, "tampered_expected_hashes");
        AssertRejected(result, "fake_success_without_payload_read");
    }

    private static void AssertRejected(
        EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result,
        string scenarioId)
    {
        Assert.Contains(
            result.NegativeProof.Scenarios,
            scenario => scenario.ScenarioId == scenarioId
                        && scenario.ActualStatus == "rejected"
                        && scenario.Diagnostics.Count > 0);
    }

    private static T ReadPayload<T>(string root, string fileName)
    {
        var value = JsonSerializer.Deserialize<T>(
            File.ReadAllText(Path.Combine(root, fileName)),
            JsonOptions());
        Assert.NotNull(value);
        return value!;
    }

    private static string HashFile(string path)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(File.ReadAllText(path)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
