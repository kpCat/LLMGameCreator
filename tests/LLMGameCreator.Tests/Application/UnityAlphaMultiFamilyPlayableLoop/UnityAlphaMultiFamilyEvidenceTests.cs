using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyEvidenceTests
{
    [Fact]
    public void EvidenceIsJsonParseableAndBlockedWhenUnityProofIsNotExecuted()
    {
        var result = UnityAlphaMultiFamilyTestFactory.BuildFromRepo();

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.Report.Goal056AcceptedByUserHandoff);
        Assert.True(result.Report.SourceFactsConsumed);
        Assert.True(result.Report.UnityStagingExists);
        Assert.True(result.Report.AllFamilyModesPresent);
        Assert.True(result.Report.MediaBindingValidationPassed);
        Assert.True(result.Report.InvalidMatrixPassed);
        Assert.False(result.Report.UnityEditorOrPlayerExecuted);
        Assert.False(result.Report.AllFamilyLoopsVerified);
        Assert.Contains("implementationStatus=BLOCKED", result.ReportMarkdown);
        Assert.Contains("accepted=false", result.ReportMarkdown);
        Assert.Contains("manualGate=unity_alpha_multifamily_playable_loop_verification", result.ReportMarkdown);
        Assert.Contains("unity_alpha_multifamily_playable_loop_verification required", result.ReportMarkdown);

        Assert.Contains(UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("map_panel_rpg"), result.ArtifactJsonByFileName.Keys);
        Assert.Contains(UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("survival_sandbox"), result.ArtifactJsonByFileName.Keys);
        Assert.Contains(UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("first_person_grid_dungeon"), result.ArtifactJsonByFileName.Keys);
        foreach (var pair in result.ArtifactJsonByFileName)
        {
            using var _ = JsonDocument.Parse(pair.Value);
            Assert.False(string.IsNullOrWhiteSpace(pair.Key));
        }
    }
}
