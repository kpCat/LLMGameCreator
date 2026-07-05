using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaManualGateAcceptanceRecord;

public sealed class OfflineGeoworldAlphaManualGateAcceptanceRecordTests
{
    [Fact]
    public void RepositoryManualGateAcceptanceRecordIsGreenAndDoesNotEmbedManualInput()
    {
        var root = ProjectRoot();

        var result = new OfflineGeoworldAlphaManualGateAcceptanceRecordService().Build(root);

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.AcceptanceRecord.HumanAccepted);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            result.AcceptanceRecord.ManualGateStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.HumanDecisionStatement,
            result.AcceptanceRecord.HumanDecisionStatement);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256,
            result.AcceptanceRecord.ManualResultSha256);
        Assert.False(result.AcceptanceRecord.AcceptedByCodex);
        Assert.True(result.AcceptanceRecord.ManualInputNotCommitted);
        Assert.False(result.AcceptanceRecord.RawManualResultEmbeddedInArtifacts);
        Assert.Equal(12, result.AcceptanceRecord.RequiredStepCount);
        Assert.Equal(12, result.AcceptanceRecord.PassedStepCount);
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ProceduralFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.ExportFileIndex.Files, file =>
            file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingGoal115SnapshotBlocksAcceptance()
    {
        using var fixture = Goal116Fixture.Create(copyGoal115Snapshot: false);
        fixture.WriteManualResult("manual result fixture");

        var result = new OfflineGeoworldAlphaManualGateAcceptanceRecordService().Build(fixture.Root);

        Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.AcceptanceRecord.HumanAccepted);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusBlocked,
            result.AcceptanceRecord.ManualGateStatus);
        Assert.Contains("goal116.goal115_snapshot_missing", result.AcceptanceRecord.Errors);
    }

    [Fact]
    public void ManualHashMismatchBlocksAcceptance()
    {
        using var fixture = Goal116Fixture.Create(copyGoal115Snapshot: true);
        fixture.WriteManualResult("manual result fixture");

        var result = new OfflineGeoworldAlphaManualGateAcceptanceRecordService().Build(fixture.Root);

        Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.AcceptanceRecord.HumanAccepted);
        Assert.Contains("goal116.manual_result_sha256_mismatch", result.AcceptanceRecord.Errors);
    }

    [Fact]
    public void NonGreenGoal115SnapshotBlocksAcceptance()
    {
        using var fixture = Goal116Fixture.Create(copyGoal115Snapshot: true);
        fixture.ReplaceGoal115SnapshotText("GREEN_ACCEPTABLE_CANDIDATE", "FAILED_SOURCE_SNAPSHOT");
        fixture.WriteManualResult("manual result fixture");

        var result = new OfflineGeoworldAlphaManualGateAcceptanceRecordService().Build(fixture.Root);

        Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.AcceptanceRecord.HumanAccepted);
        Assert.Contains("goal116.goal115_decision_status_not_green", result.AcceptanceRecord.Errors);
    }

    [Fact]
    public void NegativeProofRejectsManualInputAndForbiddenPathSamples()
    {
        var result = new OfflineGeoworldAlphaManualGateAcceptanceRecordService().Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.ManualInputStagedOrCommittedRejected);
        Assert.True(result.NegativeProof
            .ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected);
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("unity/LLMGameCreatorAlpha/ProjectSettings/", StringComparison.Ordinal));
    }

    private sealed class Goal116Fixture : IDisposable
    {
        private Goal116Fixture(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static Goal116Fixture Create(bool copyGoal115Snapshot)
        {
            var root = Path.Combine(Path.GetTempPath(), "llmgc-goal116-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var fixture = new Goal116Fixture(root);
            if (copyGoal115Snapshot)
            {
                fixture.CopyGoal115Snapshot();
            }

            return fixture;
        }

        public void CopyGoal115Snapshot()
        {
            var source = Path.Combine(
                ProjectRoot(),
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .SourceDecisionSnapshotRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            var target = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .SourceDecisionSnapshotRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
        }

        public void ReplaceGoal115SnapshotText(string oldValue, string newValue)
        {
            var path = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .SourceDecisionSnapshotRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(path, File.ReadAllText(path).Replace(oldValue, newValue));
        }

        public void WriteManualResult(string text)
        {
            var path = Path.Combine(
                Root,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualResultRelativePath
                    .Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
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
