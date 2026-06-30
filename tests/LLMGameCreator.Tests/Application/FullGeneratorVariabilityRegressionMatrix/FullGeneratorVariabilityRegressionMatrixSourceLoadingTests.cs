using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityRegressionMatrixSourceLoadingTests
{
    [Fact]
    public void Goal059ConsumesGoal058HandoffAndRequiredSourceChain()
    {
        var result = FullGeneratorVariabilityRegressionMatrixTestFactory.BuildFromRepo();
        var manifest = result.SourceManifest;

        Assert.False(manifest.Accepted);
        Assert.True(manifest.Goal058AcceptedByUserHandoff);
        Assert.True(manifest.Goal058ReportWasGreenProducedForReview);
        Assert.True(manifest.Goal058UnityProofPassed);
        Assert.Equal(3, manifest.FamilyCount);
        Assert.True(manifest.SourceArtifactCount >= 12);
        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            Assert.Contains(familyId, manifest.SelectedFamilyIds);
        }

        foreach (var artifactFamily in new[]
        {
            "campaign_source_manifest",
            "campaign_plan",
            "family_run",
            "review_package_manifest",
            "preview_export_payload",
            "unity_command_plan",
            "unity_player_proof",
            "staging_family_command_plan",
            "staging_media_manifest"
        })
        {
            Assert.Contains(manifest.SourceArtifactRefs, item => item.ArtifactFamily == artifactFamily && item.Exists && item.HashMatches);
        }

        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "full_media_bound_generator_campaign_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == FullGeneratorVariabilityMatrixVocabulary.FinalGate
            && item.Status == "required");
    }
}
