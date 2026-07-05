using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaAcceptanceOperatorPack;

public sealed class OfflineGeoworldAlphaAcceptanceOperatorPackTests
{
    [Fact]
    public void RealCurrentRepositoryStateWithMissingManualResultIsOperatorReady()
    {
        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(ProjectRoot());

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun,
            result.Dashboard.OperatorStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            result.Dashboard.DecisionStatusFromGoal111);
        Assert.False(result.Dashboard.ManualResultPresent);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
    }

    [Fact]
    public void MissingGoal110PackageBlocksOperatorPack()
    {
        using var fixture = Goal112Fixture.Create(copyGoal110Package: false);
        fixture.WriteGoal111Decision(OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal110Missing,
            result.Dashboard.OperatorStatus);
        Assert.Contains(result.Dashboard.Errors, item => item.Contains("Goal110", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingGoal111DecisionBlocksOperatorPack()
    {
        using var fixture = Goal112Fixture.Create();

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal111DecisionMissing,
            result.Dashboard.OperatorStatus);
        Assert.False(result.Dashboard.AcceptedByCodex);
    }

    [Fact]
    public void FailedInvalidGoal111DecisionBlocksOperatorPack()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            manualResultPresent: true);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal111Invalid,
            result.Dashboard.OperatorStatus);
        Assert.False(result.Dashboard.AcceptedByCodex);
    }

    [Fact]
    public void GreenGoal111CandidateIsHumanReviewOnly()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
            manualResultPresent: true);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGreenManualResultAvailable,
            result.Dashboard.OperatorStatus);
        Assert.True(result.Dashboard.ManualResultAvailableForHumanReview);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
    }

    [Fact]
    public async Task PendingResultTemplateCopyIsNotManualAcceptance()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending);

        var write = await new OfflineGeoworldAlphaAcceptanceOperatorPackService()
            .BuildAndWriteAsync(fixture.Root);
        var templatePath = Path.Combine(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PendingResultTemplateCopyFileName);
        using var template = JsonDocument.Parse(File.ReadAllText(templatePath));

        Assert.True(template.RootElement.GetProperty("templateCopyOnly").GetBoolean());
        Assert.True(template.RootElement.GetProperty("pendingOnly").GetBoolean());
        Assert.True(template.RootElement.GetProperty("notRealHumanResult").GetBoolean());
        Assert.False(template.RootElement.GetProperty("accepted").GetBoolean());
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, ".llmgc", "manual")));
    }

    [Fact]
    public void GeneratedRunbookContainsManualResultPathAndGate()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);
        var runbook = result.ProceduralFiles[
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName];

        Assert.Contains(OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath, runbook);
        Assert.Contains(OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ManualGate, runbook);
    }

    [Fact]
    public void GeneratedRunbookDoesNotContainForbiddenPathOrSourceReferences()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);
        var runbook = result.ProceduralFiles[
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName];

        Assert.DoesNotContain("/mnt", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/home/oai", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sandbox:/", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LFZ", runbook, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Infection Free Zone", runbook, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NegativeProofConfirmsNoResultMeansNoAcceptance()
    {
        using var fixture = Goal112Fixture.Create();
        fixture.WriteGoal111Decision(OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending);

        var result = new OfflineGeoworldAlphaAcceptanceOperatorPackService().Build(fixture.Root);

        Assert.True(result.NegativeProof.Passed);
        Assert.False(result.NegativeProof.ManualResultPresent);
        Assert.False(result.NegativeProof.AcceptedByCodex);
        Assert.True(result.NegativeProof.HumanAcceptanceStillRequired);
    }

    private sealed class Goal112Fixture : IDisposable
    {
        private Goal112Fixture(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static Goal112Fixture Create(bool copyGoal110Package = true)
        {
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal112-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            if (copyGoal110Package)
            {
                CopyGoal110Package(ProjectRoot(), root);
            }

            return new Goal112Fixture(root);
        }

        public void WriteGoal111Decision(string decisionStatus, bool manualResultPresent = false)
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
                    sourceGoalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
                    manualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
                    decisionStatus,
                    acceptedByCodex = false,
                    humanAcceptanceStillRequired = true,
                    acceptableCandidate = decisionStatus
                                          == OfflineGeoworldAlphaManualResultIntakeVocabulary
                                              .DecisionStatusGreenCandidate,
                    resultFilePresent = manualResultPresent,
                    resultFilePath = manualResultPresent
                        ? OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath
                        : string.Empty,
                    candidateResultPaths =
                        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths,
                    checklistHashExpected = "fixture",
                    checklistHashActual = "fixture",
                    errors = Array.Empty<string>(),
                    warnings = Array.Empty<string>()
                },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(
                Path.Combine(
                    decisionRoot,
                    OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName),
                json);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void CopyGoal110Package(string sourceRoot, string targetRoot)
        {
            var source = Path.Combine(
                sourceRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            var export = Path.Combine(
                targetRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            var procedural = Path.Combine(
                targetRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar));
            var streaming = Path.Combine(
                targetRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(export);
            Directory.CreateDirectory(procedural);
            Directory.CreateDirectory(streaming);
            foreach (var file in Directory.EnumerateFiles(source))
            {
                File.Copy(file, Path.Combine(export, Path.GetFileName(file)));
            }
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
