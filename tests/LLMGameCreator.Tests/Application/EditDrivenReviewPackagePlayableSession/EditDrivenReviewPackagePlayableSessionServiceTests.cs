using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenReviewPackagePlayableSession;

public sealed class EditDrivenReviewPackagePlayableSessionServiceTests
{
    [Fact]
    public async Task ServiceBuildsGoal078ArtifactsFromRealGoal077Package()
    {
        var service = new EditDrivenReviewPackagePlayableSessionEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal077AcceptedByUserHandoff);
        Assert.True(result.SourceArtifactManifest.Goal077ReportWasGreenProducedForReview);
        Assert.True(result.SourceArtifactManifest.Goal077ArtifactAcceptedFalse);
        Assert.True(result.PackageReadProof.Passed);
        Assert.True(result.ActionLog.Passed);
        Assert.True(result.StateChain.Passed);
        Assert.True(result.ReplayProof.Passed);
        Assert.True(result.TamperNegativeProof.Passed);
        Assert.True(result.PlayerCommandIndex.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(9, result.Report.RowCount);
        Assert.Equal(18, result.Report.TargetCount);
        Assert.Equal(57, result.Report.ActionCount);

        foreach (var fileName in EditDrivenReviewPackagePlayableSessionEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public void ActionLogReadsEveryPackageTargetPayloadAndRecordsDiskHashes()
    {
        var root = ProjectRoot();
        var result = new EditDrivenReviewPackagePlayableSessionEvidenceService().Build(root);
        var inspectActions = result.ActionLog.Actions
            .Where(action => action.ActionType == "inspect_target")
            .ToList();

        Assert.Equal(18, inspectActions.Count);
        Assert.All(inspectActions, action =>
        {
            Assert.True(action.TargetPayloadRead);
            Assert.False(string.IsNullOrWhiteSpace(action.TargetFileHash));
            Assert.False(string.IsNullOrWhiteSpace(action.TargetPayloadHash));
            var path = Path.Combine(
                root,
                EditDrivenReviewPackagePlayableSessionVocabulary.Goal077RelativeOutputDirectory,
                action.TargetRelativePath.Replace('/', Path.DirectorySeparatorChar));
            var payload = File.ReadAllText(path).TrimEnd('\r', '\n');
            Assert.Equal(action.TargetFileHash, EditDrivenReviewPackagePlayableSessionHash.Sha256(payload));
        });

        Assert.Equal(
            result.ActionLog.Actions.Select(action => action.TargetId)
                .Where(targetId => !string.IsNullOrWhiteSpace(targetId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal),
            inspectActions.Select(action => action.TargetId).OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public void StateHashChainAndReplayProofAreDeterministic()
    {
        var result = new EditDrivenReviewPackagePlayableSessionEvidenceService().Build(ProjectRoot());

        Assert.NotEqual(result.StateChain.InitialStateHash, result.StateChain.FinalStateHash);
        Assert.Equal(result.StateChain.FinalStateHash, result.ReplayProof.OriginalFinalStateHash);
        Assert.Equal(result.StateChain.FinalStateHash, result.ReplayProof.ReplayFinalStateHash);
        Assert.True(result.ReplayProof.InitialDiffersFromFinal);
        Assert.True(result.ReplayProof.ReplayFinalHashMatchesOriginal);
        Assert.Equal(result.ActionLog.ActionCount, result.StateChain.Entries.Count);
        Assert.All(result.StateChain.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.StateHash)));
    }

    [Fact]
    public void NegativeProofRejectsMissingTamperedIllegalOrderAndFakeReadScenarios()
    {
        var result = new EditDrivenReviewPackagePlayableSessionEvidenceService().Build(ProjectRoot());

        Assert.True(result.TamperNegativeProof.Passed);
        AssertScenarioRejected(result, "missing_target_file", "goal078.read.ledger_file_missing");
        AssertScenarioRejected(result, "tampered_target_payload", "goal078.read.ledger_hash_mismatch");
        AssertScenarioRejected(result, "illegal_action_target", "goal078.replay.action_order_or_identity_mismatch");
        AssertScenarioRejected(result, "replay_order_mismatch", "goal078.replay.action_order_or_identity_mismatch");
        AssertScenarioRejected(result, "fake_success_without_target_payload_read", "goal078.replay.action_order_or_identity_mismatch");
    }

    private static void AssertScenarioRejected(
        EditDrivenReviewPackagePlayableSessionBuildResult result,
        string scenarioId,
        string diagnosticCode)
    {
        var scenario = Assert.Single(result.TamperNegativeProof.Scenarios, item => item.ScenarioId == scenarioId);
        Assert.Equal("rejected", scenario.ActualStatus);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == diagnosticCode);
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
