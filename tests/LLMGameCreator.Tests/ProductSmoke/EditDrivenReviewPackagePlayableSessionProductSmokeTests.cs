using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenReviewPackagePlayableSessionProductSmokeTests
{
    [Fact]
    public async Task Goal078EditDrivenReviewPackagePlayableSessionReadsArtifactsAndReplays()
    {
        var service = new EditDrivenReviewPackagePlayableSessionEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.PackageReadProof.Passed);
        Assert.True(result.ReplayProof.Passed);
        Assert.True(result.TamperNegativeProof.Passed);
        Assert.Equal(EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate, result.Report.ManualGate);

        var readProof = ReadArtifact<EditDrivenReviewPackagePlayableSessionPackageReadProof>(
            write.OutputDirectoryPath,
            "package-read-proof.json");
        var actionLog = ReadArtifact<EditDrivenReviewPackagePlayableSessionActionLog>(
            write.OutputDirectoryPath,
            "playable-session-action-log.json");
        var stateChain = ReadArtifact<EditDrivenReviewPackagePlayableSessionStateChain>(
            write.OutputDirectoryPath,
            "playable-session-state-chain.json");
        var replayProof = ReadArtifact<EditDrivenReviewPackagePlayableSessionReplayProof>(
            write.OutputDirectoryPath,
            "playable-session-replay-proof.json");

        Assert.True(readProof.Passed);
        Assert.True(readProof.AllLedgerFilesExist);
        Assert.True(readProof.AllLedgerFileHashesMatch);
        Assert.True(replayProof.ReplayFinalHashMatchesOriginal);
        Assert.NotEqual(stateChain.InitialStateHash, stateChain.FinalStateHash);

        var inspectActions = actionLog.Actions.Where(action => action.ActionType == "inspect_target").ToList();
        Assert.Equal(18, inspectActions.Count);
        Assert.All(inspectActions, action =>
        {
            Assert.True(action.TargetPayloadRead);
            Assert.False(string.IsNullOrWhiteSpace(action.TargetFileHash));
            Assert.False(string.IsNullOrWhiteSpace(action.TargetPayloadHash));
        });
    }

    private static T ReadArtifact<T>(string outputRoot, string fileName)
    {
        var json = File.ReadAllText(Path.Combine(outputRoot, fileName));
        var value = EditDrivenReviewPackagePlayableSessionHash.Deserialize<T>(json);
        Assert.NotNull(value);
        return value!;
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
