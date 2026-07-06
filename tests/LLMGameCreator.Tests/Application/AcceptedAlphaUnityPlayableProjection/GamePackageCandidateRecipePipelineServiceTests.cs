using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidateRecipePipelineServiceTests
{
    [Fact]
    public async Task Goal131GamePackageCandidateRecipePipelineSurfacesScoredSelectedCandidate()
    {
        var root = ProjectRoot();
        var write = await new GamePackageCandidateRecipePipelineService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.True(result.ScriptScan.Passed);
        Assert.True(result.ScriptScan.InvokesGoal129MatrixRunner);
        Assert.True(result.ScriptScan.ScoresCandidatesAfterMatrix);
        Assert.True(result.ScriptScan.SelectsAndWritesHandoff);
        Assert.True(result.ScriptScan.MetadataOnlyRecipeMutation);
        Assert.True(result.ScriptScan.RefusesWritesOutsideGoal131Root);
        Assert.True(result.ScriptScan.RejectsManualInputRoot);
        Assert.True(result.ScriptScan.NoBroadGitClean);
        Assert.True(result.ScriptScan.NoLlmProviderNetwork);

        Assert.True(result.RecipeCatalogScan.Passed);
        Assert.Equal(4, result.RecipeCatalogScan.RecipeCount);
        Assert.True(result.RecipeCatalogScan.RequiredRecipeIdsPresent);
        Assert.True(result.RecipeCatalogScan.RequiredCandidateIdsPresent);
        Assert.True(result.RecipeCatalogScan.MetadataOnlySafeTuning);
        Assert.True(result.RecipeCatalogScan.RequiredAnchorsPresent);

        Assert.True(result.CandidateIndexScan.Passed);
        Assert.Equal(4, result.CandidateIndexScan.CandidateCount);
        Assert.True(result.CandidateIndexScan.RequiredCandidateIdsPresent);
        Assert.True(result.CandidateIndexScan.CandidatePackagesUnderGoal131Roots);
        Assert.True(result.CandidateIndexScan.CandidatePackageHashesDiffer);
        Assert.True(result.CandidateIndexScan.RequiredCompatibilityIdsPreserved);
        Assert.True(result.CandidateIndexScan.SourceTemplateHashMatchesSample);
        Assert.True(result.CandidateIndexScan.ManifestTitlePreserved);
        Assert.True(result.CandidateIndexScan.CandidateMetadataPreservesFullPlaythrough);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateRecipePipelineVocabulary.BalancedBaselineCandidateId);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateRecipePipelineVocabulary.AlchemyFocusCandidateId);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateRecipePipelineVocabulary.CombatFocusCandidateId);
        Assert.Contains(result.CandidateIndexScan.Candidates, candidate =>
            candidate.CandidateId
            == GamePackageCandidateRecipePipelineVocabulary.ExplorationFocusCandidateId);

        Assert.True(result.PipelineResultScan.ResultExists);
        Assert.True(result.PipelineResultScan.Passed);
        Assert.True(result.ScoringResultScan.ResultExists);
        Assert.True(result.ScoringResultScan.Passed);
        Assert.True(result.MatrixResultScan.ResultExists);
        Assert.True(result.MatrixResultScan.Passed);
        Assert.True(result.SelectedHandoffScan.Passed);
        Assert.True(result.LogScan.Passed);
        Assert.True(result.NegativeProof.Passed);

        Assert.Equal("GREEN", result.Dashboard.RecipePipelineStatus);
        Assert.Equal(4, result.Dashboard.RecipeCount);
        Assert.Equal(4, result.Dashboard.CandidateCount);
        Assert.Equal(4, result.Dashboard.PassedCandidates);
        Assert.Equal(0, result.Dashboard.FailedCandidates);
        Assert.True(result.Dashboard.MatrixPassed);
        Assert.Equal(
            result.PipelineResultScan.SelectedCandidateId,
            result.Dashboard.SelectedCandidateId);
        Assert.True(result.Dashboard.SelectedCandidateScore > 0);
        Assert.True(result.Dashboard.ManualUnityOptional);
        Assert.True(result.Dashboard.SamplePackageUnmodified);
        Assert.True(result.Dashboard.ProjectionOnly);
        Assert.True(result.Dashboard.MetadataOnlyRecipeMutation);

        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == GamePackageCandidateRecipePipelineVocabulary.CandidateIndexRelativePath);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateRecipePipelineVocabulary.PipelineResultFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateRecipePipelineVocabulary.ScoringResultFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == GamePackageCandidateRecipePipelineVocabulary.DocumentationPath);
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
