using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaManualResultWorkbench;

public sealed class OfflineGeoworldAlphaManualResultWorkbenchTests
{
    [Fact]
    public void MissingRealResultIsWorkbenchReadyPendingHumanResult()
    {
        using var fixture = Goal113Fixture.Create();

        var result = new OfflineGeoworldAlphaManualResultWorkbenchService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult,
            result.Dashboard.WorkbenchStatus);
        Assert.False(result.Dashboard.ManualResultPresent);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
    }

    [Fact]
    public async Task DraftTemplateIsWrittenOnlyUnderGoal113Paths()
    {
        using var fixture = Goal113Fixture.Create();

        var write = await new OfflineGeoworldAlphaManualResultWorkbenchService()
            .BuildAndWriteAsync(fixture.Root);
        var draftPath = Path.Combine(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName);

        Assert.True(File.Exists(draftPath));
        Assert.All(write.WrittenFiles, path =>
            Assert.False(path.StartsWith(".llmgc/manual/", StringComparison.Ordinal), path));
        Assert.DoesNotContain(
            write.WrittenFiles,
            path => path.StartsWith("unity/", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath
                .Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void MalformedResultIsInvalidAndNotAccepted()
    {
        using var fixture = Goal113Fixture.Create();
        fixture.WriteCandidate("{ invalid json");

        var result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(fixture.Root, [fixture.CandidatePath]);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid,
            result.Dashboard.WorkbenchStatus);
        Assert.False(result.Dashboard.Validation.ReadyForHumanReview);
        Assert.False(result.Dashboard.AcceptedByCodex);
    }

    [Fact]
    public void ChecklistHashMismatchIsInvalidAndNotAccepted()
    {
        using var fixture = Goal113Fixture.Create();
        fixture.WriteCandidate(fixture.BuildResultJson(accepted: true, checklistHash: "wrong"));

        var result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(fixture.Root, [fixture.CandidatePath]);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid,
            result.Dashboard.WorkbenchStatus);
        Assert.Contains(result.Dashboard.Validation.Errors, item => item.Contains("checklistHash", StringComparison.Ordinal));
        Assert.False(result.Dashboard.AcceptedByCodex);
    }

    [Fact]
    public void DuplicateMissingAndUnknownStepsAreInvalid()
    {
        using var duplicate = Goal113Fixture.Create();
        duplicate.WriteCandidate(duplicate.BuildResultJson(
            accepted: true,
            duplicateStepId: duplicate.StepIds[0]));
        var duplicateResult = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(duplicate.Root, [duplicate.CandidatePath]);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid,
            duplicateResult.Dashboard.WorkbenchStatus);
        Assert.Equal(1, duplicateResult.Dashboard.Validation.StepSummary.DuplicateCount);

        using var missing = Goal113Fixture.Create();
        missing.WriteCandidate(missing.BuildResultJson(
            accepted: true,
            omittedStepId: missing.StepIds[0]));
        var missingResult = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(missing.Root, [missing.CandidatePath]);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid,
            missingResult.Dashboard.WorkbenchStatus);
        Assert.Equal(1, missingResult.Dashboard.Validation.StepSummary.MissingCount);

        using var unknown = Goal113Fixture.Create();
        unknown.WriteCandidate(unknown.BuildResultJson(
            accepted: true,
            unknownStepId: "unknown_extra_step"));
        var unknownResult = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(unknown.Root, [unknown.CandidatePath]);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid,
            unknownResult.Dashboard.WorkbenchStatus);
        Assert.Equal(1, unknownResult.Dashboard.Validation.StepSummary.UnknownCount);
    }

    [Fact]
    public void AllRequiredStepsPassedIsHumanReviewCandidateOnly()
    {
        using var fixture = Goal113Fixture.Create();
        fixture.WriteCandidate(fixture.BuildResultJson(accepted: true));

        var result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(fixture.Root, [fixture.CandidatePath]);

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultReadyForHumanReview,
            result.Dashboard.WorkbenchStatus);
        Assert.True(result.Dashboard.Validation.ReadyForHumanReview);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
    }

    [Fact]
    public void MissingSourceArtifactsProduceBlockedStatuses()
    {
        using var missingGoal110 = Goal113Fixture.Create(copyGoal110Package: false);
        missingGoal110.WriteGoal111Decision();
        missingGoal110.WriteGoal112Artifacts();
        var missingGoal110Result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(missingGoal110.Root);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal110,
            missingGoal110Result.Dashboard.WorkbenchStatus);

        using var missingGoal111 = Goal113Fixture.Create(writeGoal111: false);
        var missingGoal111Result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(missingGoal111.Root);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal111,
            missingGoal111Result.Dashboard.WorkbenchStatus);

        using var missingGoal112 = Goal113Fixture.Create(writeGoal112: false);
        var missingGoal112Result = new OfflineGeoworldAlphaManualResultWorkbenchService()
            .Build(missingGoal112.Root);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal112,
            missingGoal112Result.Dashboard.WorkbenchStatus);
    }

    private sealed class Goal113Fixture : IDisposable
    {
        private Goal113Fixture(string root, string checklistHash, string resultSchema, IReadOnlyList<string> stepIds)
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
        public string CandidatePath =>
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
            + "/input/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultFileName;

        public static Goal113Fixture Create(
            bool copyGoal110Package = true,
            bool writeGoal111 = true,
            bool writeGoal112 = true)
        {
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal113-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string checklistHash = string.Empty;
            string resultSchema = OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultSchema;
            IReadOnlyList<string> steps = [];
            if (copyGoal110Package)
            {
                (checklistHash, resultSchema, steps) = CopyGoal110Package(ProjectRoot(), root);
            }

            var fixture = new Goal113Fixture(root, checklistHash, resultSchema, steps);
            if (writeGoal111)
            {
                fixture.WriteGoal111Decision();
            }

            if (writeGoal112)
            {
                fixture.WriteGoal112Artifacts();
            }

            return fixture;
        }

        public void WriteCandidate(string json) => WriteRelative(CandidatePath, json);

        public void WriteGoal111Decision()
        {
            var decisionRoot = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(decisionRoot);
            var json = JsonSerializer.Serialize(
                new
                {
                    goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
                    manualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
                    decisionStatus = OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
                    acceptedByCodex = false,
                    humanAcceptanceStillRequired = true,
                    resultFilePresent = false,
                    candidateResultPaths = new[] { CandidatePath }
                },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(
                Path.Combine(
                    decisionRoot,
                    OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName),
                json);
        }

        public void WriteGoal112Artifacts()
        {
            var root = Path.Combine(
                Root,
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(root);
            var dashboard = JsonSerializer.Serialize(
                new
                {
                    goalId = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
                    manualGate = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ManualGate,
                    operatorStatus = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
                        .OperatorStatusReadyPendingHumanRun,
                    preferredManualResultPath =
                        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath,
                    candidateManualResultPaths = new[] { CandidatePath },
                    acceptedByCodex = false,
                    humanAcceptanceStillRequired = true
                },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(
                Path.Combine(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName),
                dashboard);
            File.WriteAllText(
                Path.Combine(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ResultPathMapFileName),
                dashboard);
            File.WriteAllText(
                Path.Combine(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName),
                "Goal112 fixture runbook");
        }

        public string BuildResultJson(
            bool accepted,
            string? checklistHash = null,
            string? omittedStepId = null,
            string? duplicateStepId = null,
            string? unknownStepId = null)
        {
            var steps = StepIds
                .Where(step => !string.Equals(step, omittedStepId, StringComparison.Ordinal))
                .Select(step => new
                {
                    stepId = step,
                    status = "passed",
                    notes = "real fixture",
                    evidenceRef = step + "Evidence"
                })
                .ToList();
            if (!string.IsNullOrWhiteSpace(duplicateStepId))
            {
                steps.Add(new
                {
                    stepId = duplicateStepId,
                    status = "passed",
                    notes = "duplicate fixture",
                    evidenceRef = duplicateStepId + "Evidence"
                });
            }

            if (!string.IsNullOrWhiteSpace(unknownStepId))
            {
                steps.Add(new
                {
                    stepId = unknownStepId,
                    status = "passed",
                    notes = "unknown fixture",
                    evidenceRef = unknownStepId + "Evidence"
                });
            }

            return JsonSerializer.Serialize(
                new
                {
                    goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
                    manualGate = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ManualGate,
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

        private void WriteRelative(string relativePath, string json)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        private static (string ChecklistHash, string ResultSchema, IReadOnlyList<string> StepIds)
            CopyGoal110Package(string sourceRoot, string targetRoot)
        {
            var exportRoot = Path.Combine(
                targetRoot,
                ".llmgc",
                "exports",
                "goal-110-offline-geoworld-alpha-acceptance");
            Directory.CreateDirectory(exportRoot);
            Directory.CreateDirectory(Path.Combine(
                targetRoot,
                ".llmgc",
                "procedural",
                "goal-110-offline-geoworld-alpha-manual-acceptance-gate"));
            Directory.CreateDirectory(Path.Combine(
                targetRoot,
                "unity",
                "LLMGameCreatorAlpha",
                "Assets",
                "StreamingAssets",
                "LLMGameCreator",
                "OfflineGeoworldGoal110"));

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
            return (checklistHash, resultSchema, steps);
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
