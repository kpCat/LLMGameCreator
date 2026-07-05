using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaManualResultIntake;

public sealed class OfflineGeoworldAlphaManualResultIntakeTests
{
    [Fact]
    public void MissingActualResultIsBlockedPendingManualResult()
    {
        using var fixture = Goal110Fixture.Create();
        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            result.Decision.DecisionStatus);
        Assert.False(result.Decision.AcceptableCandidate);
        Assert.True(result.Decision.HumanAcceptanceStillRequired);
        Assert.False(result.Decision.AcceptedByCodex);
    }

    [Fact]
    public void MalformedJsonIsFailedInvalidResult()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult("{ invalid json");

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            result.Decision.DecisionStatus);
        Assert.Contains(result.Decision.Errors, item => item.Contains("malformed", StringComparison.Ordinal));
    }

    [Fact]
    public void WrongChecklistHashIsFailedInvalidResult()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(accepted: true, checklistHash: "wrong"));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            result.Decision.DecisionStatus);
        Assert.Contains(result.Decision.Errors, item => item.Contains("checklistHash", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingRequiredStepBlocksIncompleteResult()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(
            accepted: true,
            omittedStepId: fixture.StepIds[0]));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusIncomplete,
            result.Decision.DecisionStatus);
        Assert.Equal(1, result.Decision.StepSummary.MissingCount);
    }

    [Fact]
    public void DuplicateRequiredStepIsFailedInvalidResult()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(
            accepted: true,
            duplicateStepId: fixture.StepIds[0]));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            result.Decision.DecisionStatus);
        Assert.Equal(1, result.Decision.StepSummary.DuplicateCount);
    }

    [Fact]
    public void FailedRequiredStepBlocksIncompleteResult()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(
            accepted: true,
            statusOverrides: new Dictionary<string, string>
            {
                [fixture.StepIds[0]] = "failed"
            }));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusIncomplete,
            result.Decision.DecisionStatus);
        Assert.Equal(1, result.Decision.StepSummary.FailedCount);
    }

    [Fact]
    public void AllStepsPassedWithAcceptedFalseBlocksAcceptedFalse()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(accepted: false));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusAcceptedFalse,
            result.Decision.DecisionStatus);
        Assert.False(result.Decision.AcceptableCandidate);
    }

    [Fact]
    public void AllStepsPassedWithAcceptedTrueIsGreenCandidateButNotAcceptedByCodex()
    {
        using var fixture = Goal110Fixture.Create();
        fixture.WriteManualResult(fixture.BuildResultJson(accepted: true));

        var result = new OfflineGeoworldAlphaManualResultIntakeService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
            result.Decision.DecisionStatus);
        Assert.True(result.Decision.AcceptableCandidate);
        Assert.False(result.Decision.AcceptedByCodex);
        Assert.True(result.Decision.HumanAcceptanceStillRequired);
    }

    [Fact]
    public void MultipleDifferingResultFilesAreInvalidNotRandomWinner()
    {
        using var fixture = Goal110Fixture.Create();
        var first = ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/"
                    + OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultFileName;
        var second = OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                     + "/input/"
                     + OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultFileName;
        fixture.WriteRelative(first, fixture.BuildResultJson(accepted: true));
        fixture.WriteRelative(second, fixture.BuildResultJson(accepted: false));

        var result = new OfflineGeoworldAlphaManualResultIntakeService()
            .Build(fixture.Root, [first, second]);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            result.Decision.DecisionStatus);
        Assert.Contains(result.Decision.Errors, item => item.Contains("multiple differing", StringComparison.Ordinal));
    }

    private sealed class Goal110Fixture : IDisposable
    {
        private Goal110Fixture(string root, string checklistHash, string resultSchema, IReadOnlyList<string> stepIds)
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

        public static Goal110Fixture Create()
        {
            var sourceRoot = ProjectRoot();
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal111-" + Guid.NewGuid().ToString("N"));
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
            return new Goal110Fixture(root, checklistHash, resultSchema, steps);
        }

        public void WriteManualResult(string json) =>
            WriteRelative(
                ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/"
                + OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultFileName,
                json);

        public void WriteRelative(string relativePath, string json)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        public string BuildResultJson(
            bool accepted,
            string? checklistHash = null,
            string? omittedStepId = null,
            string? duplicateStepId = null,
            IReadOnlyDictionary<string, string>? statusOverrides = null)
        {
            var steps = StepIds
                .Where(step => !string.Equals(step, omittedStepId, StringComparison.Ordinal))
                .Select(step => new
                {
                    stepId = step,
                    status = statusOverrides is not null && statusOverrides.TryGetValue(step, out var status)
                        ? status
                        : "passed",
                    notes = "",
                    evidenceRef = step + "Evidence"
                })
                .ToList();
            if (!string.IsNullOrWhiteSpace(duplicateStepId))
            {
                steps.Add(new
                {
                    stepId = duplicateStepId,
                    status = "passed",
                    notes = "",
                    evidenceRef = duplicateStepId + "Evidence"
                });
            }

            return JsonSerializer.Serialize(
                new
                {
                    goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
                    manualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
                    resultSchema = ResultSchema,
                    accepted,
                    checklistHash = checklistHash ?? ChecklistHash,
                    steps
                },
                new JsonSerializerOptions { WriteIndented = true });
        }

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
