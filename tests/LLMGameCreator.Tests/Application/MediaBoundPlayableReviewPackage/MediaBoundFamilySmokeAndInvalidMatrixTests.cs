using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundFamilySmokeAndInvalidMatrixTests
{
    [Fact]
    public void FamilySmokeMatrixCoversAllFamiliesWithPhysicalMediaAndProof()
    {
        var matrix = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo().FamilySmokeMatrix;

        Assert.True(matrix.Passed);
        Assert.Equal(3, matrix.FamilyCount);
        Assert.All(matrix.Families, family =>
        {
            Assert.True(family.Passed);
            Assert.Equal(5, family.StagedFileCount);
            Assert.Equal(3, family.PngFileCount);
            Assert.Equal(1, family.WavFileCount);
            Assert.Equal(1, family.BundleJsonFileCount);
            Assert.True(family.ManifestBound);
            Assert.True(family.PreviewPayloadBound);
            Assert.True(family.UnityProofBound);
        });
    }

    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredGoal055Risks()
    {
        var matrix = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo().InvalidMatrix;
        var scenarios = matrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Equal(MediaBoundPlayableReviewPackageVocabulary.RequiredInvalidScenarioIds.Count, matrix.ScenarioCount);
        AssertCase(scenarios, "missing_goal054_source", "goal055.source.goal054_missing", "blocked");
        AssertCase(scenarios, "missing_staged_file", "goal055.stage.file_missing", "rejected");
        AssertCase(scenarios, "stale_hash", "goal055.stage.hash_mismatch", "rejected");
        AssertCase(scenarios, "malformed_png", "goal055.media.png_malformed", "rejected");
        AssertCase(scenarios, "malformed_wav", "goal055.media.wav_malformed", "rejected");
        AssertCase(scenarios, "unsafe_relative_path", "goal055.path.unsafe", "rejected");
        AssertCase(scenarios, "duplicate_binding_id", "goal055.binding.duplicate_id", "rejected");
        AssertCase(scenarios, "fake_family_id", "goal055.family.fake_id", "rejected");
        AssertCase(scenarios, "fake_slot_id", "goal055.slot.fake_id", "rejected");
        AssertCase(scenarios, "license_provenance_blocked_promoted", "goal055.license.blocked_promoted", "blocked");
        AssertCase(scenarios, "provider_network_llm_rag_claim", "goal055.boundary.provider_network_llm_rag", "blocked");
        AssertCase(scenarios, "lua_execution_claim", "goal055.boundary.lua_execution", "blocked");
        AssertCase(scenarios, "runtime_ui_gamepackage_schema_mutation_claim", "goal055.boundary.runtime_ui_gamepackage", "blocked");
        AssertCase(scenarios, "unity_broad_mutation_claim", "goal055.boundary.unity_broad_mutation", "blocked");
        AssertCase(scenarios, "nondeterministic_ordering", "goal055.order.nondeterministic", "rejected");
        AssertCase(scenarios, "missing_review_trace", "goal055.review.trace_missing", "rejected");
        AssertCase(scenarios, "fake_unity_proof_line", "goal055.unity.fake_proof_line", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, InvalidMediaBoundPackageScenario> byId,
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
