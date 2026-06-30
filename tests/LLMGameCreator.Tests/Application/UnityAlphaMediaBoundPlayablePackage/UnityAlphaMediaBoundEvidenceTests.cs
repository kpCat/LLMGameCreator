using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundEvidenceTests
{
    [Fact]
    public void EvidenceIsJsonParseableAndBlockedWhenUnityProofIsNotExecuted()
    {
        var result = UnityAlphaMediaBoundTestFactory.BuildFromRepo();

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.Report.Goal055AcceptedByUserHandoff);
        Assert.True(result.Report.StreamingAssetsPayloadStaged);
        Assert.Equal(15, result.Report.PhysicalMediaFileCount);
        Assert.True(result.Report.InvalidMatrixPassed);
        Assert.False(result.Report.UnityEditorOrPlayerExecuted);
        Assert.False(result.Report.UnityMediaLoadContractPassed);
        Assert.Contains("implementationStatus=BLOCKED", result.ReportMarkdown);
        Assert.Contains("accepted=false", result.ReportMarkdown);
        Assert.Contains("unity_alpha_media_bound_playable_package_verification required", result.ReportMarkdown);

        foreach (var pair in result.ArtifactJsonByFileName)
        {
            using var _ = JsonDocument.Parse(pair.Value);
            Assert.False(string.IsNullOrWhiteSpace(pair.Key));
        }
    }
}
