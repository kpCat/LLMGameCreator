using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityRegressionMatrixPlanTests
{
    [Fact]
    public void MatrixReviewPreviewAndUnityCommandPlanCoverEveryRow()
    {
        var result = FullGeneratorVariabilityRegressionMatrixTestFactory.BuildFromRepo();

        Assert.True(result.SeedProfileMatrix.Passed);
        Assert.False(result.SeedProfileMatrix.Accepted);
        Assert.Equal(9, result.SeedProfileMatrix.RowCount);
        Assert.True(result.VarianceMetrics.Passed);
        Assert.True(result.ReplayProof.Passed);
        Assert.True(result.ReviewPackageMatrixManifest.Passed);
        Assert.False(result.ReviewPackageMatrixManifest.Accepted);
        Assert.True(result.PreviewExportMatrixPayload.Passed);
        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains(result.StagingFiles, item => item.RelativePath == FullGeneratorVariabilityMatrixVocabulary.UnityMatrixCommandPlanStagingRelativePath);
        Assert.Contains("full_generator_matrix_loaded=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("full_generator_matrix_completed=true", result.UnityCommandPlan.ExpectedPlayerMarkers);

        foreach (var row in result.MatrixRowsByRowId.Values)
        {
            var rowRef = FullGeneratorVariabilityMatrixEvidenceService.RowFileName(row.FamilyId, row.SeedId);
            Assert.Contains(rowRef, result.ReviewPackageMatrixManifest.MatrixRowRefs);
            Assert.Contains(result.PreviewExportMatrixPayload.Rows, item => item.RowId == row.RowId && item.DerivedCampaignHash == row.DerivedCampaignHash);
            Assert.Contains(result.UnityCommandPlan.Rows, item => item.RowId == row.RowId && item.DerivedCampaignHash == row.DerivedCampaignHash);
            foreach (var marker in FullGeneratorVariabilityMatrixBuilder.ExpectedRowMarkers(row))
            {
                Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
            }
        }
    }
}
