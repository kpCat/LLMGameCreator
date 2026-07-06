using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidateFactoryProjectionServiceTests
{
    [Fact]
    public async Task Goal130GamePackageCandidateFactorySurfacesGeneratedMatrixArtifacts()
    {
        var root = ProjectRoot();
        var write = await new GamePackageCandidateFactoryProjectionService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.True(result.ScriptScan.Passed);
        Assert.True(result.ScriptScan.InvokesGoal129MatrixRunner);
        Assert.True(result.ScriptScan.RefusesWritesOutsideGoal130Root);
        Assert.True(result.ScriptScan.RejectsManualInputRoot);
        Assert.True(result.ScriptScan.NoBroadGitClean);
        Assert.True(result.ScriptScan.NoLlmProviderNetwork);
        Assert.True(result.CandidateIndexScan.Passed);
        Assert.Equal(3, result.CandidateIndexScan.CandidateCount);
        Assert.True(result.CandidateIndexScan.RequiredCandidateIdsPresent);
        Assert.True(result.CandidateIndexScan.CandidatePackagesUnderGoal130Roots);
        Assert.True(result.CandidateIndexScan.CandidatePackageHashesDiffer);
        Assert.True(result.CandidateIndexScan.RequiredCompatibilityIdsPreserved);
        Assert.True(result.CandidateIndexScan.SourceTemplateHashMatchesSample);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateFactoryProjectionVocabulary.BaselineCandidateId);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateFactoryProjectionVocabulary.AlchemyCandidateId);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateFactoryProjectionVocabulary.CombatCandidateId);
        Assert.True(result.FactoryResultScan.ResultExists);
        Assert.True(result.FactoryResultScan.Passed);
        Assert.True(result.MatrixResultScan.ResultExists);
        Assert.True(result.MatrixResultScan.Passed);
        Assert.True(result.LogScan.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.Equal("GREEN", result.Dashboard.CandidateFactoryStatus);
        Assert.Equal(3, result.Dashboard.CandidateCount);
        Assert.Equal(3, result.Dashboard.PassedCandidates);
        Assert.Equal(0, result.Dashboard.FailedCandidates);
        Assert.True(result.Dashboard.MatrixPassed);
        Assert.True(result.Dashboard.ManualUnityOptional);
        Assert.True(result.Dashboard.SamplePackageUnmodified);
        Assert.True(result.Dashboard.ProjectionOnly);

        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.FactoryResultFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.MatrixResultFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.DocumentationPath);
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
