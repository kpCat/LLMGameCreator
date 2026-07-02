using LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenUnityAlphaStreamingAssetsHandoff;

[Collection(EditDrivenUnityAlphaStreamingAssetsHandoffTestCollection.Name)]
public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffServiceTests
{
    [Fact]
    public async Task ServiceBuildsDeterministicHandoffArtifactsFromGoal080AndGoal081Inputs()
    {
        var service = new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService();
        var first = await service.BuildAndWriteAsync(ProjectRoot());
        var second = await service.BuildAndWriteAsync(ProjectRoot());
        var result = first.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal081AcceptedByHandoff);
        Assert.True(result.ProbeReadProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.CommandTranscriptProof.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(9, result.Report.RowCount);
        Assert.Equal(18, result.Report.TargetCount);
        Assert.Equal(57, result.Report.Goal078ActionCount);
        Assert.Equal(124, result.Report.CommandCount);
        Assert.Equal(result.Report.DeterministicHash, second.Result.Report.DeterministicHash);

        foreach (var fileName in EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(first.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(first.StreamingAssetsDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task MirroredStreamingAssetsPayloadCanBeReadAndValidated()
    {
        var result = (await new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.ProbeReadProof.PayloadReadAttempted);
        Assert.True(result.ProbeReadProof.RequiredPayloadFilesPresent);
        Assert.True(result.ProbeReadProof.PayloadFileHashesMatchExpected);
        Assert.True(result.ProbeReadProof.PackageHashMatchesGoal080);
        Assert.True(result.ProbeReadProof.CommandHashMatchesGoal081);
        Assert.True(result.ProbeReadProof.TranscriptHashMatchesGoal081);
        Assert.True(result.ProbeReadProof.StateHashMatchesGoal081);
        Assert.True(result.ProbeReadProof.CountsMatchExpected);
        Assert.Equal(result.Report.ProjectedPackageHash, result.ProbeReadProof.ProjectedPackageHash);
        Assert.Equal(result.Report.CommandScriptHash, result.ProbeReadProof.CommandScriptHash);
        Assert.Equal(result.Report.TranscriptHash, result.ProbeReadProof.TranscriptHash);
    }

    [Fact]
    public async Task NegativeProofRejectsMissingTamperedAndFakeReadScenarios()
    {
        var result = (await new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.NegativeProof.Passed);
        AssertScenarioRejected(result, "missing_handoff_manifest");
        AssertScenarioRejected(result, "missing_expected_hashes");
        AssertScenarioRejected(result, "missing_command_index");
        AssertScenarioRejected(result, "tampered_projected_package_index");
        AssertScenarioRejected(result, "tampered_expected_hashes");
        AssertScenarioRejected(result, "fake_success_without_payload_read");
    }

    [Fact]
    public async Task UnityProbeSourceReferencesStreamingAssetsRootOnly()
    {
        var result = (await new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;
        var probePath = Path.Combine(
            ProjectRoot(),
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "Scripts",
            "EditDrivenGamePackageHandoffProbe.cs");
        var probe = File.ReadAllText(probePath);

        Assert.True(result.ProbeReadProof.UnityProbeSourceReferencesStreamingAssetsRoot);
        Assert.True(result.ProbeReadProof.UnityProbeSourceDoesNotReferenceAlphaRuntimeBootstrap);
        Assert.Contains("Application.streamingAssetsPath", probe);
        Assert.Contains("LLMGameCreator/EditDrivenGoal082", probe);
        Assert.DoesNotContain("AlphaRuntimeBootstrap", probe);
    }

    private static void AssertScenarioRejected(
        EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result,
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
