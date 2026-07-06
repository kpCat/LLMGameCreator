using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidatePipelineOperatorServiceTests
{
    [Fact]
    public async Task Goal132CandidatePipelineOperatorWritesGreenPanelEvidence()
    {
        var root = ProjectRoot();
        await new GamePackageCandidateRecipePipelineService()
            .BuildAndWriteAsync(root);

        var write = await new GamePackageCandidatePipelineOperatorService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN_READY", result.Dashboard.OperatorStatus);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.NormalCommand,
            result.Dashboard.NormalCommand);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand,
            result.Dashboard.DryRunCommand);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath,
            result.Dashboard.ResultPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Dashboard.SelectedCandidateId));
        Assert.True(result.Dashboard.SelectedCandidateScore > 0);
        Assert.Equal(4, result.Dashboard.CandidateCount);
        Assert.Equal(4, result.Dashboard.PassedCandidates);
        Assert.Equal(0, result.Dashboard.FailedCandidates);
        Assert.True(result.Dashboard.MatrixPassed);
        Assert.True(result.Dashboard.ManualUnityOptional);
        Assert.True(result.Dashboard.ProjectionOnly);
        Assert.True(result.Dashboard.SamplePackageReadOnly);
        Assert.True(result.Dashboard.OperatorResultPresent);

        Assert.True(result.ScriptScan.Passed);
        Assert.True(result.ScriptScan.SupportsDryRun);
        Assert.True(result.ScriptScan.SupportsApplyCleanup);
        Assert.True(result.ScriptScan.NormalCommandUsesCmdWrapper);
        Assert.True(result.ScriptScan.RejectsManualInputRoot);
        Assert.True(result.ScriptScan.NoBroadGitClean);
        Assert.True(result.ScriptScan.NoLlmProviderNetwork);

        Assert.True(result.WinFormsScan.Passed);
        Assert.True(result.WinFormsScan.WinFormsPanelPresent);
        Assert.True(result.WinFormsScan.RefreshButtonPresent);
        Assert.True(result.WinFormsScan.CopyCommandButtonPresent);
        Assert.True(result.WinFormsScan.DryRunButtonPresent);
        Assert.True(result.WinFormsScan.RunButtonPresent);
        Assert.True(result.WinFormsScan.AsyncRunPresent);
        Assert.True(result.WinFormsScan.MarshalUiUpdatesPresent);
        Assert.True(result.WinFormsScan.UsesApplicationOperatorService);
        Assert.True(result.WinFormsScan.ShowsOutputTail);
        Assert.True(result.NegativeProof.Passed);

        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == GamePackageCandidatePipelineOperatorVocabulary.DashboardRelativePath);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidatePipelineOperatorVocabulary.ResultFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidatePipelineOperatorVocabulary.DocumentationPath);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
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
