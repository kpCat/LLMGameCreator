using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredGoal054Risks()
    {
        var matrix = MediaMaterializationReviewPackageTestFactory.BuildFromRepo().InvalidMatrix;
        var scenarios = matrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        AssertCase(scenarios, "missing_goal053_source", "goal054.source.goal053_missing", "blocked");
        AssertCase(scenarios, "fake_media_request_id", "goal054.request.fake_id", "rejected");
        AssertCase(scenarios, "fake_binding_id", "goal054.binding.fake_id", "rejected");
        AssertCase(scenarios, "missing_physical_media_file", "goal054.media.file_missing", "rejected");
        AssertCase(scenarios, "hash_mismatch", "goal054.media.hash_mismatch", "rejected");
        AssertCase(scenarios, "media_kind_mismatch", "goal054.media.kind_mismatch", "rejected");
        AssertCase(scenarios, "unknown_prohibited_license_promoted", "goal054.license.unknown_or_prohibited", "blocked");
        AssertCase(scenarios, "imported_provider_candidate_promoted", "goal054.provenance.import_or_provider_promoted", "blocked");
        AssertCase(scenarios, "cross_family_binding_leak", "goal054.binding.cross_family_leak", "rejected");
        AssertCase(scenarios, "absolute_path_leak", "goal054.path.absolute", "rejected");
        AssertCase(scenarios, "network_provider_llm_rag_call_claim", "goal054.boundary.provider_network_llm_rag", "blocked");
        AssertCase(scenarios, "gamepackage_schema_mutation_claim", "goal054.boundary.gamepackage_schema", "blocked");
        AssertCase(scenarios, "runtime_ui_unity_mutation_claim", "goal054.boundary.runtime_ui_unity", "blocked");
        AssertCase(scenarios, "nondeterministic_ordering", "goal054.order.nondeterministic", "rejected");
        AssertCase(scenarios, "malformed_png_header", "goal054.media.png_malformed", "rejected");
        AssertCase(scenarios, "malformed_wav_header", "goal054.media.wav_malformed", "rejected");
        AssertCase(scenarios, "missing_provenance", "goal054.provenance.missing", "rejected");
        AssertCase(scenarios, "missing_review_trace", "goal054.review.trace_missing", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, InvalidMediaMaterializationScenario> byId,
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
