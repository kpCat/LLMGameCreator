using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAcceptedAlphaBaselineReview;

public sealed class OfflineGeoworldAcceptedAlphaBaselineReviewTests
{
    [Fact]
    public void RepositoryBaselineReviewIsGreenAndReady()
    {
        var result = new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
            result.Dashboard.BaselineId);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            result.Dashboard.ManualGateStatus);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExpectedManualResultSha256,
            result.Dashboard.ManualResultSha256);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision,
            result.Dashboard.RecommendedNextDecision);
        Assert.True(result.Dashboard.NotFinalReleaseOrRuntimeBuild);
        Assert.True(result.Dashboard.NoRuntimeProviderOrNetworkChanges);
        Assert.True(result.Dashboard.NoUnityFileChangesRequired);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalIds.Count,
            result.SourceIndex.IncludedSourceGoalCount);
        Assert.DoesNotContain(result.ProceduralFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ExportFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    [Fact]
    public void BaselineReviewDoesNotRequireLocalManualResultToExist()
    {
        using var fixture = Goal118Fixture.Create();
        fixture.WriteAllSourceRoots();
        fixture.WriteAcceptedGoal116Evidence();
        fixture.WriteGoal117ContinuationEvidence();
        fixture.WriteGoal114Evidence();
        fixture.WriteGoal109Evidence();
        fixture.WriteGoal108Evidence();

        var result = new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .Build(fixture.Root);

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.Dashboard.AcceptedBaselineReady);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, ".llmgc", "manual")));
    }

    [Fact]
    public void MissingGoal116AcceptedEvidenceBlocksBaselineReview()
    {
        using var fixture = Goal118Fixture.Create();
        fixture.WriteAllSourceRoots();
        fixture.WriteGoal117ContinuationEvidence();
        fixture.WriteGoal114Evidence();
        fixture.WriteGoal109Evidence();
        fixture.WriteGoal108Evidence();

        var result = new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .Build(fixture.Root);

        Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.QualityGateScan.Passed);
        Assert.Contains("goal118.goal116_record_missing", result.Dashboard.Errors);
    }

    [Fact]
    public void NegativeProofRejectsForbiddenBaselineExpansion()
    {
        var result = new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.ManualInputStagedOrEmbeddedRejected);
        Assert.True(result.NegativeProof.LiveGeodataProviderNetworkStartRejected);
        Assert.True(result.NegativeProof.RuntimeSchemaLuaGeneratorLibraryChangesRejected);
        Assert.True(result.NegativeProof.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected);
        Assert.True(result.NegativeProof.FinalReleasePackagingRejected);
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.Contains("StreamingAssets", StringComparison.Ordinal));
    }

    private sealed class Goal118Fixture : IDisposable
    {
        private Goal118Fixture(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static Goal118Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal118-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new Goal118Fixture(root);
        }

        public void WriteAllSourceRoots()
        {
            foreach (var root in OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRoots)
            {
                Directory.CreateDirectory(Path.Combine(Root, root.Replace('/', Path.DirectorySeparatorChar)));
            }
        }

        public void WriteAcceptedGoal116Evidence()
        {
            var targetRoot = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(targetRoot);
            File.WriteAllText(
                Path.Combine(
                    targetRoot,
                    OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName),
                Goal116AcceptedRecordJson());
            File.WriteAllText(
                Path.Combine(targetRoot, OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName),
                Goal116AcceptedDashboardJson());
            File.WriteAllText(
                Path.Combine(
                    targetRoot,
                    OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName),
                """
                {
                  "passed": true
                }
                """);
        }

        public void WriteGoal117ContinuationEvidence()
        {
            var targetRoot = Path.Combine(
                Root,
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(targetRoot);
            File.WriteAllText(
                Path.Combine(
                    targetRoot,
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName),
                Goal117DashboardJson());
            File.WriteAllText(
                Path.Combine(
                    targetRoot,
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName),
                Goal117MatrixJson());
            File.WriteAllText(
                Path.Combine(
                    targetRoot,
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.QualityGateScanFileName),
                """
                {
                  "passed": true
                }
                """);
        }

        public void WriteGoal114Evidence() =>
            WriteJson(
                ".llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix/unity-safe-mode-compile-hotfix-dashboard.json",
                """
                {
                  "implementationStatus": "GREEN",
                  "sourceScanPassed": true,
                  "negativeProofPassed": true
                }
                """);

        public void WriteGoal109Evidence() =>
            WriteJson(
                ".llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/offline-geoworld-alpha-export-manifest.json",
                """
                {
                  "implementationStatus": "GREEN",
                  "exportPackageRoot": ".llmgc/exports/goal-109-offline-geoworld-alpha-slice",
                  "cleanImportProofPassed": true,
                  "alphaRuntimeBootstrapUnchanged": true
                }
                """);

        public void WriteGoal108Evidence() =>
            WriteJson(
                ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/offline-geoworld-alpha-slice-manifest.json",
                """
                {
                  "implementationStatus": "GREEN",
                  "alphaRuntimeBootstrapUnchanged": true,
                  "containsProviderCalls": false,
                  "containsRuntimeExecution": false
                }
                """);

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private void WriteJson(string relativePath, string json)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        private static string Goal116AcceptedRecordJson() =>
            $$"""
              {
                "manualGate": "offline_geoworld_alpha_manual_acceptance_verification",
                "manualGateStatus": "ACCEPTED_BY_HUMAN",
                "humanAccepted": true,
                "manualResultSha256": "{{OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExpectedManualResultSha256}}",
                "acceptedByCodex": false,
                "manualInputNotCommitted": true,
                "rawManualResultEmbeddedInArtifacts": false
              }
              """;

        private static string Goal116AcceptedDashboardJson() =>
            """
            {
              "recommendedNextDecision": "POST_ACCEPTANCE_CONTINUATION_SELECTION"
            }
            """;

        private static string Goal117DashboardJson() =>
            $$"""
              {
                "recommendedNextLane": "accepted_alpha_baseline_review",
                "recommendedNextGoalId": "{{OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId}}",
                "readyLaneCount": 1,
                "candidateLaneCount": 3,
                "blockedLaneCount": 3,
                "doNotStartAutomatically": true
              }
              """;

        private static string Goal117MatrixJson() =>
            """
            {
              "lanes": [
                { "status": "READY" },
                { "status": "CANDIDATE_REQUIRES_EXPLICIT_APPROVAL" },
                { "status": "CANDIDATE_REQUIRES_EXPLICIT_APPROVAL" },
                { "status": "CANDIDATE_REQUIRES_RENDERER_DECISION" },
                { "status": "BLOCKED_REQUIRES_EXPLICIT_SCHEMA_RUNTIME_TASK" },
                { "status": "BLOCKED_BY_POLICY" },
                { "status": "BLOCKED_NOT_RELEASE_READY" }
              ]
            }
            """;
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
