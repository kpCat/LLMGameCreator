using System.Text.Json;
using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityRegressionMatrixEvidenceTests
{
    [Fact]
    public void MatrixRowsProveFamilySeedCoverageVarianceAndReplayDeterminism()
    {
        var result = FullGeneratorVariabilityRegressionMatrixTestFactory.BuildFromRepo();

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.Report.Goal058AcceptedByUserHandoff);
        Assert.True(result.Report.SourceFactsConsumed);
        Assert.True(result.Report.MatrixRowsPassed);
        Assert.True(result.Report.VarianceMetricsPassed);
        Assert.True(result.Report.ReplayDeterminismPassed);
        Assert.True(result.Report.ReviewPackageMatrixManifestPassed);
        Assert.True(result.Report.PreviewExportMatrixPayloadPassed);
        Assert.True(result.Report.InvalidMatrixPassed);
        Assert.False(result.Report.UnityEditorOrPlayerExecuted);
        Assert.False(result.Report.AllMatrixMarkersMatched);
        Assert.Equal(9, result.MatrixRowsByRowId.Count);
        Assert.Equal(9, result.VarianceMetrics.DistinctDerivedCampaignHashCount);
        Assert.Equal(0, result.VarianceMetrics.OverfitWarningCount);
        Assert.Equal(9, result.ReplayProof.MatchedRowCount);

        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in FullGeneratorVariabilityMatrixVocabulary.SeedIds)
            {
                Assert.Contains(result.MatrixRowsByRowId.Values, item => item.FamilyId == familyId && item.SeedId == seedId);
            }
        }

        foreach (var row in result.MatrixRowsByRowId.Values)
        {
            Assert.NotEmpty(row.DerivedCampaignHash);
            Assert.NotEmpty(row.SelectedMediaRefs);
            Assert.NotEmpty(row.SelectedFamilyLoopRefs);
            Assert.NotEmpty(row.SelectedPreviewExportRefs);
            Assert.True(row.VariationDimensions.Count >= 4);
            Assert.Contains("matrix_row_started=" + row.RowId, row.DeterministicMarkerPlan);
            Assert.Contains("matrix_row_completed=" + row.RowId, row.DeterministicMarkerPlan);
        }
    }

    [Fact]
    public void EvidenceJsonAndReportRemainParseableAndGateRequired()
    {
        var result = FullGeneratorVariabilityRegressionMatrixTestFactory.BuildFromRepo();

        Assert.Contains("implementationStatus=BLOCKED", result.ReportMarkdown);
        Assert.Contains("accepted=false", result.ReportMarkdown);
        Assert.Contains("manualGate=full_generator_variability_regression_matrix_verification", result.ReportMarkdown);
        Assert.Contains("full_generator_variability_regression_matrix_verification required", result.ReportMarkdown);
        Assert.Contains("goal058AcceptedByUserHandoff=true", result.ReportMarkdown);
        Assert.Contains("matrixRowsPassed=true", result.ReportMarkdown);
        Assert.Contains("varianceMetricsPassed=true", result.ReportMarkdown);
        Assert.Contains("replayDeterminismPassed=true", result.ReportMarkdown);

        foreach (var fileName in RequiredJsonFileNames())
        {
            Assert.Contains(fileName, result.ArtifactJsonByFileName.Keys);
            using var _ = JsonDocument.Parse(result.ArtifactJsonByFileName[fileName]);
        }
    }

    private static IEnumerable<string> RequiredJsonFileNames()
    {
        yield return FullGeneratorVariabilityMatrixEvidenceService.SourceManifestJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.SeedProfileMatrixJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.VarianceMetricsJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.ReplayProofJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.ReviewPackageMatrixManifestJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.PreviewExportMatrixPayloadJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.UnityCommandPlanJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.UnityPlayerProofJsonFileName;
        yield return FullGeneratorVariabilityMatrixEvidenceService.InvalidMatrixJsonFileName;
        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in FullGeneratorVariabilityMatrixVocabulary.SeedIds)
            {
                yield return FullGeneratorVariabilityMatrixEvidenceService.RowFileName(familyId, seedId);
            }
        }
    }
}
