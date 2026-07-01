using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SchemaDrivenCampaignEditValidateApplyLoopProductSmokeTests
{
    [Fact]
    public async Task Goal075SchemaDrivenCampaignEditValidateApplyLoopEvidenceIsProducedForReview()
    {
        var service = new SchemaDrivenCampaignEditEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal("GREEN", write.Result.Report.ImplementationStatus);
        Assert.False(write.Result.Report.Accepted);
        Assert.True(write.Result.SourceManifest.Goal074AcceptedByUserHandoff);
        Assert.True(write.Result.ValidationMatrix.Passed);
        Assert.True(write.Result.ApplyRollbackLedger.Passed);
        Assert.True(write.Result.DiffMatrix.Passed);
        Assert.True(write.Result.PreviewExportRefreshPayload.Passed);
        Assert.True(write.Result.WinFormsBindingInventory.Passed);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.Equal(9, write.Result.Report.RowCount);
        Assert.Equal(6, write.Result.Report.EditableFieldCount);
        Assert.Equal(18, write.Result.Report.CandidateCount);
        Assert.Equal(18, write.Result.Report.AppliedChangeCount);
        Assert.Equal(SchemaDrivenCampaignEditVocabulary.FinalGate, write.Result.Report.ManualGate);

        using var editLoop = new CampaignEditValidateApplyLoopControl();
        editLoop.Bind(write.Result);

        foreach (var fileName in SchemaDrivenCampaignEditEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
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
