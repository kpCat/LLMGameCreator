using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaHumanResultRevalidation;

public sealed class OfflineGeoworldAlphaHumanResultRevalidationTests
{
    [Fact]
    public void MissingManualResultIsBlockedPendingManualResult()
    {
        using var fixture = Goal115Fixture.Create();

        var result = new OfflineGeoworldAlphaHumanResultRevalidationService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending,
            result.DecisionSnapshot.DecisionStatus);
        Assert.False(result.DecisionSnapshot.AcceptableCandidate);
        Assert.True(result.DecisionSnapshot.HumanAcceptanceStillRequired);
        Assert.False(result.DecisionSnapshot.AcceptedByCodex);
    }

    [Fact]
    public void MalformedManualResultIsFailedInvalidResult()
    {
        using var fixture = Goal115Fixture.Create();
        fixture.WriteManualResult("{ invalid json");

        var result = new OfflineGeoworldAlphaHumanResultRevalidationService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusInvalid,
            result.DecisionSnapshot.DecisionStatus);
        Assert.False(result.DecisionSnapshot.AcceptableCandidate);
        Assert.Contains(result.DecisionSnapshot.Errors, item =>
            item.Contains("malformed", StringComparison.Ordinal));
    }

    [Fact]
    public void DraftTemplateLikeManualResultIsBlockedIncomplete()
    {
        using var fixture = Goal115Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(accepted: false));

        var result = new OfflineGeoworldAlphaHumanResultRevalidationService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusIncomplete,
            result.DecisionSnapshot.DecisionStatus);
        Assert.False(result.DecisionSnapshot.AcceptableCandidate);
        Assert.Equal(fixture.StepIds.Count, result.DecisionSnapshot.StepSummary.PassedCount);
    }

    [Fact]
    public void ValidHumanResultIsGreenCandidateButStillRequiresHumanDecision()
    {
        using var fixture = Goal115Fixture.Create();
        var json = fixture.BuildResultJson(accepted: true);
        fixture.WriteManualResult(json);

        var result = new OfflineGeoworldAlphaHumanResultRevalidationService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate,
            result.DecisionSnapshot.DecisionStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
            result.DecisionSnapshot.Goal111DecisionStatus);
        Assert.True(result.DecisionSnapshot.AcceptableCandidate);
        Assert.False(result.DecisionSnapshot.AcceptedByCodex);
        Assert.True(result.DecisionSnapshot.HumanAcceptanceStillRequired);
        Assert.True(result.DecisionSnapshot.ManualGateRemainsHumanDecision);
        Assert.Equal(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionReady,
            result.DecisionSnapshot.RecommendedHumanDecision);
        Assert.Equal(fixture.Sha256(json), result.DecisionSnapshot.ManualResultSha256);
        Assert.Equal(fixture.StepIds.Count, result.DecisionSnapshot.StepSummary.PassedCount);
    }

    [Fact]
    public void FileIndexesAndExpectedPathsNeverIncludeManualInput()
    {
        using var fixture = Goal115Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(accepted: true));

        var result = new OfflineGeoworldAlphaHumanResultRevalidationService().Build(fixture.Root);

        Assert.True(result.QualityGateScan.ManualInputNotCommitted);
        Assert.True(result.QualityGateScan.ManualInputExcludedFromFileIndex);
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ProceduralFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ExportFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    private sealed class Goal115Fixture : IDisposable
    {
        private Goal115Fixture(
            string root,
            string checklistHash,
            string resultSchema,
            IReadOnlyList<string> stepIds)
        {
            Root = root;
            ChecklistHash = checklistHash;
            ResultSchema = resultSchema;
            StepIds = stepIds;
        }

        public string Root { get; }
        public string ChecklistHash { get; }
        public string ResultSchema { get; }
        public IReadOnlyList<string> StepIds { get; }

        public static Goal115Fixture Create()
        {
            var sourceRoot = ProjectRoot();
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal115-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var exportRoot = Path.Combine(
                root,
                ".llmgc",
                "exports",
                "goal-110-offline-geoworld-alpha-acceptance");
            var proceduralRoot = Path.Combine(
                root,
                ".llmgc",
                "procedural",
                "goal-110-offline-geoworld-alpha-manual-acceptance-gate");
            var streamingRoot = Path.Combine(
                root,
                "unity",
                "LLMGameCreatorAlpha",
                "Assets",
                "StreamingAssets",
                "LLMGameCreator",
                "OfflineGeoworldGoal110");
            Directory.CreateDirectory(exportRoot);
            Directory.CreateDirectory(proceduralRoot);
            Directory.CreateDirectory(streamingRoot);
            foreach (var file in Directory.EnumerateFiles(Path.Combine(
                         sourceRoot,
                         ".llmgc",
                         "exports",
                         "goal-110-offline-geoworld-alpha-acceptance")))
            {
                File.Copy(file, Path.Combine(exportRoot, Path.GetFileName(file)));
            }

            var templatePath = Path.Combine(
                exportRoot,
                "offline-geoworld-alpha-acceptance-result-template.json");
            using var template = JsonDocument.Parse(File.ReadAllText(templatePath));
            var checklistHash = template.RootElement.GetProperty("checklistHash").GetString() ?? string.Empty;
            var resultSchema = template.RootElement.GetProperty("resultSchema").GetString() ?? string.Empty;
            var steps = template.RootElement.GetProperty("steps")
                .EnumerateArray()
                .Select(item => item.GetProperty("stepId").GetString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
            return new Goal115Fixture(root, checklistHash, resultSchema, steps);
        }

        public void WriteManualResult(string json)
        {
            var path = Path.Combine(
                Root,
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        public string BuildResultJson(bool accepted)
        {
            var steps = StepIds
                .Select(step => new
                {
                    stepId = step,
                    status = "passed",
                    notes = "fixture",
                    evidenceRef = step + "Evidence"
                })
                .ToArray();
            return JsonSerializer.Serialize(
                new
                {
                    goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
                    manualGate = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualGate,
                    resultSchema = ResultSchema,
                    accepted,
                    acceptedByCodex = false,
                    humanAcceptanceStillRequired = true,
                    manualAcceptancePending = false,
                    resultStatus = "manual_result_completed",
                    checklistHash = ChecklistHash,
                    steps
                },
                new JsonSerializerOptions { WriteIndented = true });
        }

        public string Sha256(string text) =>
            Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))
                .ToLowerInvariant();

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
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
}
