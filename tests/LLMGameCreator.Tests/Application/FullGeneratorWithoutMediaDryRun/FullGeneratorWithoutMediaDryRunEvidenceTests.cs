using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorWithoutMediaDryRunEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsFinalGateRequired()
    {
        var repoRoot = FindRepoRoot();
        var service = new FullGeneratorWithoutMediaDryRunEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var artifactNames = first.ArtifactJsonByFileName.Keys.OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(first.OneClickSummary.DeterministicHash, second.OneClickSummary.DeterministicHash);
        Assert.Equal(
            [
                "dry-run-source-manifest.json",
                "export-profile-selection-matrix.json",
                "family-first-person-grid-dungeon-dry-run.json",
                "family-map-panel-rpg-dry-run.json",
                "family-survival-sandbox-dry-run.json",
                "invalid-fake-leak-matrix.json",
                "one-click-dry-run-summary.json",
                "package-compatibility-or-materialization-summary.json",
                "repair-diagnostics-matrix.json",
                "review-promotion-ledger.json",
                "runtime-preview-validation-matrix.json"
            ],
            artifactNames);

        Assert.Equal("GREEN", first.Report.ImplementationStatus);
        Assert.False(first.Report.Accepted);
        Assert.False(first.SourceManifest.Accepted);
        Assert.Contains("accepted=false", first.ReportMarkdown);
        Assert.Contains("manualGate=full_generator_without_media_verification", first.ReportMarkdown);
        Assert.Contains("full_generator_without_media_verification required", first.ReportMarkdown);
        Assert.DoesNotContain(first.Report.Diagnostics, item => item.Severity == "error" || item.Severity == "critical");
    }

    [Fact]
    public void SourceManifestRecordsGoal043HandoffAndPreservesGoal031AndGoal032()
    {
        var manifest = new FullGeneratorWithoutMediaDryRunEvidenceService()
            .Build(FindRepoRoot())
            .SourceManifest;

        Assert.Equal("without_media", manifest.MediaPolicy);
        Assert.Equal(3, manifest.SelectedFamilyIds.Count);
        Assert.Contains(manifest.AcceptedPreflightGates, item =>
            item.GateId == "multi_family_generated_template_vertical_slice_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.AcceptedPreflightGates, item =>
            item.GateId == "semantic_pack_composition_blueprint_verification"
            && item.Status == "produced_for_review_not_passed");
        Assert.Contains(manifest.AcceptedPreflightGates, item =>
            item.GateId == "dynamic_semantic_feature_system_verification"
            && item.Status == "produced_for_review_not_passed");
        Assert.Contains(manifest.AcceptedPreflightGates, item =>
            item.GateId == "full_generator_without_media_verification"
            && item.Status == "required");
    }

    [Fact]
    public void RuntimeExportPackageAndInvalidMatricesCoverRequiredGoal047Risks()
    {
        var result = new FullGeneratorWithoutMediaDryRunEvidenceService().Build(FindRepoRoot());

        Assert.True(result.ReviewPromotionLedger.Passed);
        Assert.Equal(12, result.ReviewPromotionLedger.TransitionCount);
        Assert.True(result.RepairDiagnosticsMatrix.Passed);
        Assert.Equal(14, result.RepairDiagnosticsMatrix.DiagnosticCount);
        Assert.Equal(3, result.FamilyDryRuns.Count);
        Assert.All(result.FamilyDryRuns, item =>
        {
            Assert.True(item.StateChangingLoopProof);
            Assert.True(item.ReplayHashProof.Passed);
            Assert.Equal(10, item.GeneratedSystemCoverage.Count);
            Assert.True(item.RuntimePreviewPayloadSummary.SourceHashesMatch);
            Assert.True(item.ExportCandidatePayloadSummary.WithoutMedia);
        });
        Assert.True(result.RuntimePreviewValidationMatrix.Passed);
        Assert.True(result.ExportProfileSelectionMatrix.Passed);
        Assert.False(result.PackageCompatibilitySummary.PackageMaterializationAttempted);
        Assert.True(result.PackageCompatibilitySummary.CompatibilityProofPassed);
        Assert.Equal(30, result.PackageCompatibilitySummary.Rows.Count);
        Assert.True(result.InvalidMatrix.Passed);
        Assert.True(result.InvalidMatrix.ScenarioCount >= 17);

        var invalidById = result.InvalidMatrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        AssertCase(invalidById, "missing_goal043_source", "goal047.source.goal043_missing", "rejected");
        AssertCase(invalidById, "wrong_accepted_gate", "goal047.preflight.goal043_handoff_missing", "rejected");
        AssertCase(invalidById, "fake_family_id", "goal047.family.fake", "rejected");
        AssertCase(invalidById, "duplicate_promotion_transition_id", "goal047.review.transition_id.duplicate", "rejected");
        AssertCase(invalidById, "missing_repair_action", "goal047.repair.action_missing", "rejected");
        AssertCase(invalidById, "provider_llm_rag_call_claim", "goal047.boundary.provider_llm_rag", "blocked");
        AssertCase(invalidById, "media_generated_claim", "goal047.boundary.media", "blocked");
        AssertCase(invalidById, "runtime_source_changed_claim", "goal047.boundary.runtime_source", "blocked");
        AssertCase(invalidById, "unity_executed_claim", "goal047.boundary.unity", "blocked");
        AssertCase(invalidById, "gamepackage_schema_mutation_claim", "goal047.boundary.gamepackage_schema", "blocked");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, FullGeneratorInvalidScenario> byId,
        string scenarioId,
        string expectedCode,
        string expectedStatus)
    {
        Assert.True(byId.TryGetValue(scenarioId, out var scenario), "Missing invalid scenario: " + scenarioId);
        Assert.Equal(expectedStatus, scenario.ActualStatus);
        Assert.False(scenario.ActualValid);
        Assert.Contains(scenario.Diagnostics, item => item.Code == expectedCode);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
