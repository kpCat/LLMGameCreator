using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredRisks()
    {
        var invalid = MediaAssetCampaignTestFactory.BuildFromRepo()
            .InvalidMatrix
            .Scenarios
            .ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        AssertCase(invalid, "duplicate_media_request_id", "goal053.request.duplicate_id", "rejected");
        AssertCase(invalid, "unknown_family_id", "goal053.family.unknown", "rejected");
        AssertCase(invalid, "unknown_generated_target_id", "goal053.target.unknown", "rejected");
        AssertCase(invalid, "unknown_media_slot_id", "goal053.slot.unknown", "rejected");
        AssertCase(invalid, "invalid_media_kind", "goal053.media_kind.invalid", "rejected");
        AssertCase(invalid, "missing_required_provenance", "goal053.provenance.missing", "rejected");
        AssertCase(invalid, "unknown_no_license_candidate_accepted_attempt", "goal053.license.unknown", "rejected");
        AssertCase(invalid, "cc_by_without_attribution", "goal053.license.attribution_missing", "rejected");
        AssertCase(invalid, "share_alike_gpl_risk_auto_promotion", "goal053.license.share_alike_or_gpl_risk", "blocked");
        AssertCase(invalid, "provider_candidate_without_model_license_run_metadata", "goal053.provider.metadata_missing", "blocked");
        AssertCase(invalid, "final_prose_or_final_artwork_claim", "goal053.boundary.final_claim", "blocked");
        AssertCase(invalid, "path_traversal_in_fixture_path", "goal053.fixture.path_traversal", "rejected");
        AssertCase(invalid, "external_absolute_path_in_artifact", "goal053.artifact.absolute_path", "rejected");
        AssertCase(invalid, "network_url_treated_as_downloaded_asset", "goal053.artifact.network_url", "rejected");
        AssertCase(invalid, "provider_llm_rag_call_claim", "goal053.boundary.provider_llm_rag", "blocked");
        AssertCase(invalid, "runtime_ui_unity_gamepackage_mutation_claim", "goal053.boundary.runtime_ui_unity_gamepackage", "blocked");
        AssertCase(invalid, "nondeterministic_ordering", "goal053.order.nondeterministic", "rejected");
        AssertCase(invalid, "fake_source_artifact_hash_or_path", "goal053.source.fake_hash_or_path", "rejected");
        AssertCase(invalid, "self_promotion_without_review_trace", "goal053.review.trace_missing", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, InvalidMediaScenario> byId,
        string scenarioId,
        string expectedCode,
        string expectedStatus)
    {
        Assert.True(byId.TryGetValue(scenarioId, out var scenario), "Missing invalid scenario: " + scenarioId);
        Assert.Equal(expectedStatus, scenario.ActualStatus);
        Assert.False(scenario.ActualValid);
        Assert.Contains(scenario.Diagnostics, item => item.Code == expectedCode);
    }
}
