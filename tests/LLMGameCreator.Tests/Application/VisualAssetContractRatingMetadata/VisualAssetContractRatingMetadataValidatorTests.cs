using LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualAssetContractRatingMetadata;

public sealed class VisualAssetContractRatingMetadataValidatorTests
{
    [Fact]
    public void DefaultMetadataOnlyContractPassesRequiredFixtureCoverage()
    {
        var contract = VisualAssetContractRatingMetadataFixtures.BuildDefaultContract();

        var result = VisualAssetContractRatingMetadataValidator.Validate(contract);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal(VisualAssetContractRatingMetadataFixtures.RequiredFixtureIds.Count, contract.Slots.Count);
        Assert.All(VisualAssetContractRatingMetadataFixtures.RequiredFixtureIds, id => Assert.Contains(contract.Slots, slot => slot.AssetSlot == id));

        var adultCapable = contract.Slots.Single(slot => slot.AssetSlot == "humanoid_paperdoll_adult_capable_metadata_only");
        Assert.True(adultCapable.AdultEnabled);
        Assert.True(adultCapable.SafeFallbackRequired);
        Assert.True(VisualAssetContractRatingMetadataValidator.HasDeterministicSafeFallback(adultCapable));
        Assert.Null(adultCapable.ApprovedAssetRef);
        Assert.Equal(VisualReviewStatus.CandidateQuarantined, adultCapable.ReviewStatus);
    }

    [Fact]
    public void NegativeProofRejectsUnsafeFakeAndMissingCases()
    {
        var contract = VisualAssetContractRatingMetadataFixtures.BuildDefaultContract();
        var proof = VisualAssetContractRatingMetadataEvidenceService.BuildNegativeProof(contract);

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(proof.ScenarioCount >= 16);
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "empty_invalid_ids", "visual_contract.contract_id.invalid");
        AssertScenarioHasCode(proof, "absolute_path_rejected", "visual_contract.approved_ref.path.invalid");
        AssertScenarioHasCode(proof, "prompt_text_as_source_of_truth", "visual_contract.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "public_export_without_safe_ref_or_fallback", "visual_contract.public_export.safe_ref_or_fallback_missing");
        AssertScenarioHasCode(proof, "adult_enabled_missing_rating_policy", "visual_contract.adult.policy_missing");
        AssertScenarioHasCode(proof, "adult_public_export_without_fallback", "visual_contract.adult.public_export_fallback_missing");
        AssertScenarioHasCode(proof, "provider_candidate_treated_as_approved", "visual_contract.provider_candidate.treated_as_approved");
        AssertScenarioHasCode(proof, "unreviewed_rejected_promotion", "visual_contract.promotion.unreviewed_or_rejected");
        AssertScenarioHasCode(proof, "approved_ref_missing_hash_path_provenance", "visual_contract.approved_ref.sha256.missing");
        AssertScenarioHasCode(proof, "missing_fallback_when_required", "visual_contract.fallback.required_missing");
        AssertScenarioHasCode(proof, "rating_export_contradiction", "visual_contract.rating_export.adult_public_contradiction");
        AssertScenarioHasCode(proof, "age_ambiguous_adult_metadata", "visual_contract.adult.age_ambiguous_or_not_adult");
        AssertScenarioHasCode(proof, "non_sapient_adult_metadata", "visual_contract.adult.non_sapient");
        AssertScenarioHasCode(proof, "non_eligible_body_plan_adult_metadata", "visual_contract.adult.non_humanoid");
        AssertScenarioHasCode(proof, "duplicate_slot_ids", "visual_contract.slot_id.duplicate");
        AssertScenarioHasCode(proof, "strict_unknown_recipe_ref", "visual_contract.recipe_ref.unknown");
        AssertScenarioHasCode(proof, "strict_unknown_part_pack_ref", "visual_contract.part_pack_ref.unknown");
    }

    private static void AssertScenarioHasCode(VisualContractNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }
}
