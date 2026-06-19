using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.WinForms.Pages.ArtifactReview;
using System.Drawing;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class ArtifactReviewPresenterTests
{
    [Fact]
    public void PresenterBuildsSummaryAndRows()
    {
        var state = new ArtifactReviewPresenter().FromLoadResult(LoadResult());

        Assert.True(state.Exists);
        Assert.Equal("snapshot/test", state.SnapshotId);
        Assert.Equal(3, state.ItemCount);
        Assert.Equal(1, state.PendingCount);
        Assert.Equal(1, state.ApprovedCount);
        Assert.Equal(1, state.RepairRequestedCount);
        Assert.Equal("artifact/approved", state.Rows[0].ArtifactId);
    }

    [Fact]
    public void PresenterFiltersRowsByState()
    {
        var presenter = new ArtifactReviewPresenter();
        var state = presenter.FromLoadResult(LoadResult());

        var filtered = presenter.ApplyFilter(state, ArtifactReviewFilter.Pending);

        Assert.Single(filtered.FilteredRows);
        Assert.Equal("artifact/pending", filtered.FilteredRows[0].ArtifactId);
    }

    [Fact]
    public void PresenterBuildsDecisionRequestFromChangedRows()
    {
        var presenter = new ArtifactReviewPresenter();
        var state = presenter.FromLoadResult(LoadResult());
        state = presenter.SetDecision(state, "artifact/pending", GeneratorPlanDraftArtifactApprovalDecisionKind.Approved, "ok", "Looks good.");

        var request = presenter.BuildDecisionRequest(state);

        var decision = Assert.Single(request.Decisions);
        Assert.Equal("artifact/pending", decision.ArtifactId);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalDecisionKind.Approved, decision.Decision);
        Assert.Equal("ok", decision.ReasonCode);
        Assert.Equal("Looks good.", decision.Comment);
    }

    [Fact]
    public void PageControlCanBeConstructedBeforeValidSplitterWidth()
    {
        using var control = new LLMGameCreator.WinForms.Pages.ArtifactReviewPageControl();

        control.Size = new Size(260, 480);
        control.PerformLayout();
        control.OnActivated();

        Assert.Equal("artifact_review", control.Id);
    }

    [Fact]
    public void PresenterBuildsApproveAllDecisionRequestForPendingRows()
    {
        var presenter = new ArtifactReviewPresenter();
        var state = presenter.FromLoadResult(LoadResultWithValidPending());

        state = presenter.ApproveAllValidPending(state, "bulk_approved", "Valid pending artifacts.");
        var request = presenter.BuildDecisionRequest(state);

        var decision = Assert.Single(request.Decisions);
        Assert.Equal("artifact/pending-valid", decision.ArtifactId);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalDecisionKind.Approved, decision.Decision);
        Assert.Equal("bulk_approved", decision.ReasonCode);
        Assert.Equal("Valid pending artifacts.", decision.Comment);
    }

    [Fact]
    public void PresenterShowsMissingSnapshotMessage()
    {
        var state = new ArtifactReviewPresenter().FromLoadResult(new GeneratorPlanDraftArtifactReviewLoadResult());

        Assert.False(state.Exists);
        Assert.Equal("not_found", state.Status);
        Assert.Contains("No draft artifact staging snapshot", state.Message);
    }

    [Fact]
    public void PresenterFormatsValidationIssuesAndJson()
    {
        var state = new ArtifactReviewPresenter().FromLoadResult(LoadResult(), ArtifactReviewFilter.Pending);

        Assert.Equal("artifact/pending", state.Detail.ArtifactId);
        Assert.Contains("\"artifact_id\":\"artifact/pending\"", state.Detail.ContentJson);
        Assert.Contains("needs_review", state.Detail.ValidationIssues);
    }

    private static GeneratorPlanDraftArtifactReviewLoadResult LoadResult()
    {
        var snapshot = new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = "snapshot/test",
            SourceProductionBatchId = "batch/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Items =
            [
                Item("artifact/pending") with { ValidationIssues = ["needs_review"] },
                Item("artifact/approved") with { State = GeneratorPlanDraftArtifactApprovalItemState.Approved },
                Item("artifact/repair") with { State = GeneratorPlanDraftArtifactApprovalItemState.RepairRequested, DecisionReasonCode = "repair" }
            ]
        };
        snapshot = new GeneratorPlanDraftArtifactApprovalValidator().Validate(snapshot);

        return new GeneratorPlanDraftArtifactReviewLoadResult
        {
            Exists = true,
            Message = "loaded",
            Snapshot = snapshot
        };
    }

    private static GeneratorPlanDraftArtifactReviewLoadResult LoadResultWithValidPending()
    {
        var snapshot = new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = "snapshot/valid-pending",
            SourceProductionBatchId = "batch/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Items = [Item("artifact/pending-valid")]
        };
        snapshot = new GeneratorPlanDraftArtifactApprovalValidator().Validate(snapshot);

        return new GeneratorPlanDraftArtifactReviewLoadResult
        {
            Exists = true,
            Message = "loaded",
            Snapshot = snapshot
        };
    }

    private static GeneratorPlanDraftArtifactApprovalItem Item(string artifactId)
    {
        return new GeneratorPlanDraftArtifactApprovalItem
        {
            ArtifactId = artifactId,
            ArtifactKind = "game_profile_v1",
            State = GeneratorPlanDraftArtifactApprovalItemState.Pending,
            SourceProductionBatchId = "batch/test",
            QueueItemId = "queue/test/item/1",
            SourceExecutionStepId = "step/test",
            ExpectedArtifactContract = "game_profile_v1",
            ContentJson = $$"""{"schema_version":"0.1","artifact_id":"{{artifactId}}"}""",
            RequiresHumanApproval = true
        };
    }
}
