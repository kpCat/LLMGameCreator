using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionTests
{
    [Fact]
    public void RepositoryContinuationSelectionIsGreenAndRecommendsBaselineReview()
    {
        var result = new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            result.Dashboard.ManualGateStatus);
        Assert.True(result.Dashboard.HumanAccepted);
        Assert.Equal(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
            result.Dashboard.RecommendedNextLane);
        Assert.Equal(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId,
            result.Dashboard.RecommendedNextGoalId);
        Assert.Equal(1, result.Dashboard.ReadyLaneCount);
        Assert.Equal(3, result.Dashboard.CandidateLaneCount);
        Assert.Equal(3, result.Dashboard.BlockedLaneCount);
        Assert.True(result.Dashboard.DoNotStartAutomatically);
        Assert.Equal(7, result.Matrix.LaneCount);
        Assert.DoesNotContain(result.ProceduralFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ExportFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    [Fact]
    public void SelectionDoesNotRequireLocalManualResultToExist()
    {
        using var fixture = Goal117Fixture.Create();
        fixture.WriteAcceptedGoal116Evidence();
        fixture.WriteGreenGoal115Snapshot();

        var result = new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .Build(fixture.Root);

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.Dashboard.ManualInputNotCommitted);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, ".llmgc", "manual")));
    }

    [Fact]
    public void NonAcceptedGoal116EvidenceBlocksContinuationSelection()
    {
        using var fixture = Goal117Fixture.Create();
        fixture.WriteAcceptedGoal116Evidence();
        fixture.WriteGreenGoal115Snapshot();
        fixture.ReplaceGoal116RecordText("ACCEPTED_BY_HUMAN", "BLOCKED_SOURCE_EVIDENCE_INVALID");

        var result = new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .Build(fixture.Root);

        Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.QualityGateScan.Passed);
        Assert.Contains("goal117.goal116_manual_gate_not_accepted", result.Dashboard.Errors);
    }

    [Fact]
    public void NegativeProofRejectsForbiddenContinuationStarts()
    {
        var result = new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.AutomaticGoal118StartRejected);
        Assert.True(result.NegativeProof.Goal118TaskFilesNotCreated);
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("docs/agent-tasks/goal-118-", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal));
    }

    private sealed class Goal117Fixture : IDisposable
    {
        private Goal117Fixture(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static Goal117Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal117-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new Goal117Fixture(root);
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
                Goal116AcceptedQualityJson());
        }

        public void WriteGreenGoal115Snapshot()
        {
            var target = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .SourceDecisionSnapshotRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, Goal115GreenDecisionSnapshotJson());
        }

        public void ReplaceGoal116RecordText(string oldValue, string newValue)
        {
            var path = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar),
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName);
            File.WriteAllText(path, File.ReadAllText(path).Replace(oldValue, newValue));
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static string Goal116AcceptedRecordJson() =>
            $$"""
              {
                "goalId": "goal_116_offline_geoworld_alpha_manual_gate_acceptance_record",
                "manualGate": "offline_geoworld_alpha_manual_acceptance_verification",
                "manualGateStatus": "ACCEPTED_BY_HUMAN",
                "humanAccepted": true,
                "sourceDecisionStatus": "GREEN_ACCEPTABLE_CANDIDATE",
                "manualResultSha256": "{{OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExpectedManualResultSha256}}",
                "acceptedByCodex": false,
                "manualInputNotCommitted": true,
                "rawManualResultEmbeddedInArtifacts": false
              }
              """;

        private static string Goal116AcceptedDashboardJson() =>
            $$"""
              {
                "goalId": "goal_116_offline_geoworld_alpha_manual_gate_acceptance_record",
                "manualGate": "offline_geoworld_alpha_manual_acceptance_verification",
                "manualGateStatus": "ACCEPTED_BY_HUMAN",
                "humanAccepted": true,
                "sourceDecisionStatus": "GREEN_ACCEPTABLE_CANDIDATE",
                "manualResultSha256": "{{OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExpectedManualResultSha256}}",
                "acceptedByCodex": false,
                "manualInputNotCommitted": true,
                "rawManualResultEmbeddedInArtifacts": false,
                "recommendedNextDecision": "POST_ACCEPTANCE_CONTINUATION_SELECTION"
              }
              """;

        private static string Goal116AcceptedQualityJson() =>
            """
            {
              "goalId": "goal_116_offline_geoworld_alpha_manual_gate_acceptance_record",
              "implementationStatus": "GREEN",
              "accepted": false,
              "passed": true
            }
            """;

        private static string Goal115GreenDecisionSnapshotJson() =>
            $$"""
              {
                "decisionStatus": "GREEN_ACCEPTABLE_CANDIDATE",
                "manualResultSha256": "{{OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExpectedManualResultSha256}}",
                "acceptedByCodex": false,
                "stepSummary": {
                  "requiredStepCount": 12,
                  "passedCount": 12
                }
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
