using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SchemaDrivenCampaignAuthoringReviewWorkspaceProductSmokeTests
{
    [Fact]
    public async Task Goal074SchemaDrivenCampaignAuthoringReviewWorkspaceEvidenceIsProducedForReview()
    {
        var service = new SchemaDrivenCampaignWorkspaceEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal("GREEN", write.Result.Report.ImplementationStatus);
        Assert.False(write.Result.Report.Accepted);
        Assert.True(write.Result.SourceManifest.Goal073AcceptedByUserHandoff);
        Assert.True(write.Result.SourceManifest.Goal072RemainsHistoricalBlocked);
        Assert.True(write.Result.SourceManifest.Goal031And032RemainProducedForReview);
        Assert.True(write.Result.RowSelector.Passed);
        Assert.True(write.Result.DynamicSchema.Passed);
        Assert.True(write.Result.UiBindingContract.Passed);
        Assert.True(write.Result.ProvenanceLedger.Passed);
        Assert.True(write.Result.ActionPlan.Passed);
        Assert.True(write.Result.ValidationDashboard.Passed);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.True(write.Result.WinFormsControlInventory.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.Equal(9, write.Result.RowSelector.RowCount);
        Assert.Equal(13, write.Result.DynamicSchema.Groups.Count);
        Assert.Equal(write.Result.DynamicSchema.Groups.Count, write.Result.UiBindingContract.GroupBindings.Count);
        Assert.Contains(
            write.Result.ProvenanceLedger.Entries,
            entry => entry.SourceGoal == "Goal072" && entry.Category == "quarantined");

        using var page = new CampaignAuthoringReviewWorkspacePageControl(service);
        page.Bind(write.Result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(Path.Combine(
            write.OutputDirectoryPath,
            SchemaDrivenCampaignWorkspaceEvidenceService.ArtifactScopeReportFileName)));
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
