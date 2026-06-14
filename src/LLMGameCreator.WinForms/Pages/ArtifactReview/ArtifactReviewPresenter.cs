using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.ArtifactReview;

public sealed class ArtifactReviewPresenter
{
    public ArtifactReviewViewState FromLoadResult(
        GeneratorPlanDraftArtifactReviewLoadResult result,
        string filter = ArtifactReviewFilter.All)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Exists)
        {
            return new ArtifactReviewViewState
            {
                Exists = false,
                Message = string.IsNullOrWhiteSpace(result.Message) ? "No draft artifact staging snapshot found." : result.Message,
                Status = "not_found",
                Filter = filter
            };
        }

        var rows = result.Snapshot.Items
            .OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .Select(ArtifactReviewRowViewModel.FromItem)
            .ToList();

        return BuildState(
            true,
            string.IsNullOrWhiteSpace(result.Message) ? "Latest draft artifact staging snapshot loaded." : result.Message,
            result.Snapshot,
            rows,
            filter,
            rows.FirstOrDefault()?.ArtifactId ?? string.Empty);
    }

    public ArtifactReviewViewState FromDecisionResult(
        GeneratorPlanDraftArtifactReviewDecisionResult result,
        string filter = ArtifactReviewFilter.All)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rows = result.Snapshot.Items
            .OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .Select(ArtifactReviewRowViewModel.FromItem)
            .ToList();

        return BuildState(
            result.Snapshot.Items.Count > 0,
            result.Ok ? "Artifact review decisions saved." : "Artifact review decisions saved with validation errors.",
            result.Snapshot,
            rows,
            filter,
            rows.FirstOrDefault()?.ArtifactId ?? string.Empty);
    }

    public ArtifactReviewViewState ApplyFilter(ArtifactReviewViewState state, string filter)
    {
        ArgumentNullException.ThrowIfNull(state);
        return BuildStateFromRows(state, state.Rows, filter, state.SelectedArtifactId);
    }

    public ArtifactReviewViewState SelectArtifact(ArtifactReviewViewState state, string artifactId)
    {
        ArgumentNullException.ThrowIfNull(state);
        return BuildStateFromRows(state, state.Rows, state.Filter, artifactId);
    }

    public ArtifactReviewViewState SetDecision(
        ArtifactReviewViewState state,
        string artifactId,
        string decision,
        string reasonCode,
        string comment)
    {
        ArgumentNullException.ThrowIfNull(state);

        var normalized = NormalizeDecision(decision);
        var rows = state.Rows
            .Select(row => string.Equals(row.ArtifactId, artifactId, StringComparison.OrdinalIgnoreCase)
                ? row with
                {
                    Decision = normalized,
                    ReasonCode = reasonCode.Trim(),
                    Comment = comment.Trim(),
                    IsChanged = !string.Equals(row.State, normalized, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(row.ReasonCode, reasonCode.Trim(), StringComparison.Ordinal)
                        || !string.Equals(row.Comment, comment.Trim(), StringComparison.Ordinal)
                }
                : row)
            .ToList();

        return BuildStateFromRows(state, rows, state.Filter, artifactId);
    }

    public ArtifactReviewViewState ApproveAllValidPending(
        ArtifactReviewViewState state,
        string reasonCode,
        string comment)
    {
        ArgumentNullException.ThrowIfNull(state);

        var rows = state.Rows
            .Select(row => row.CanApprove
                ? row with
                {
                    Decision = GeneratorPlanDraftArtifactApprovalDecisionKind.Approved,
                    ReasonCode = reasonCode.Trim(),
                    Comment = comment.Trim(),
                    IsChanged = true
                }
                : row)
            .ToList();

        return BuildStateFromRows(state, rows, state.Filter, state.SelectedArtifactId);
    }

    public GeneratorPlanDraftArtifactReviewDecisionRequest BuildDecisionRequest(
        ArtifactReviewViewState state,
        bool renderMarkdown = true)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new GeneratorPlanDraftArtifactReviewDecisionRequest
        {
            RenderMarkdown = renderMarkdown,
            Decisions = state.Rows
                .Where(row => row.IsChanged)
                .Select(row => new GeneratorPlanDraftArtifactApprovalDecision
                {
                    ArtifactId = row.ArtifactId,
                    Decision = NormalizeDecision(row.Decision),
                    ReasonCode = row.ReasonCode,
                    Comment = row.Comment,
                    DecidedAtUtc = DateTimeOffset.UtcNow
                })
                .ToList()
        };
    }

    private static ArtifactReviewViewState BuildState(
        bool exists,
        string message,
        GeneratorPlanDraftArtifactStagingSnapshot snapshot,
        IReadOnlyList<ArtifactReviewRowViewModel> rows,
        string filter,
        string selectedArtifactId)
    {
        var filteredRows = FilterRows(rows, filter).ToList();
        var selected = filteredRows.FirstOrDefault(row => string.Equals(row.ArtifactId, selectedArtifactId, StringComparison.OrdinalIgnoreCase))
            ?? filteredRows.FirstOrDefault()
            ?? rows.FirstOrDefault(row => string.Equals(row.ArtifactId, selectedArtifactId, StringComparison.OrdinalIgnoreCase))
            ?? rows.FirstOrDefault();

        return new ArtifactReviewViewState
        {
            Exists = exists,
            Message = message,
            SnapshotId = snapshot.Id,
            SourceExampleId = snapshot.SourcePreviewExampleId,
            SourcePath = snapshot.SourcePath,
            Status = snapshot.Status,
            ItemCount = rows.Count,
            PendingCount = rows.Count(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Pending),
            ApprovedCount = rows.Count(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Approved),
            RejectedCount = rows.Count(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected),
            RepairRequestedCount = rows.Count(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested),
            BlockedCount = rows.Count(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked),
            ErrorCount = snapshot.Summary.ErrorCount,
            WarningCount = snapshot.Summary.WarningCount,
            Filter = string.IsNullOrWhiteSpace(filter) ? ArtifactReviewFilter.All : filter,
            SelectedArtifactId = selected?.ArtifactId ?? string.Empty,
            Rows = rows,
            FilteredRows = filteredRows,
            Detail = selected == null ? new ArtifactReviewDetailViewModel() : new ArtifactReviewDetailViewModel
            {
                ArtifactId = selected.ArtifactId,
                ContentJson = selected.ContentJson,
                ValidationIssues = selected.Issues,
                ReasonCode = selected.ReasonCode,
                Comment = selected.Comment
            }
        };
    }

    private static ArtifactReviewViewState BuildStateFromRows(
        ArtifactReviewViewState state,
        IReadOnlyList<ArtifactReviewRowViewModel> rows,
        string filter,
        string selectedArtifactId)
    {
        var snapshot = new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = state.SnapshotId,
            SourcePreviewExampleId = state.SourceExampleId,
            SourcePath = state.SourcePath,
            Status = state.Status,
            Summary = new GeneratorPlanDraftArtifactStagingSummary
            {
                ErrorCount = state.ErrorCount,
                WarningCount = state.WarningCount
            }
        };

        return BuildState(state.Exists, state.Message, snapshot, rows, filter, selectedArtifactId);
    }

    private static IEnumerable<ArtifactReviewRowViewModel> FilterRows(
        IReadOnlyList<ArtifactReviewRowViewModel> rows,
        string filter)
    {
        return filter switch
        {
            ArtifactReviewFilter.Pending => rows.Where(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Pending),
            ArtifactReviewFilter.Approved => rows.Where(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Approved),
            ArtifactReviewFilter.Rejected => rows.Where(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected),
            ArtifactReviewFilter.RepairRequested => rows.Where(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested),
            ArtifactReviewFilter.Blocked => rows.Where(row => row.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked),
            _ => rows
        };
    }

    private static string NormalizeDecision(string decision)
    {
        return decision.Trim().ToLowerInvariant() switch
        {
            GeneratorPlanDraftArtifactApprovalDecisionKind.Approved => GeneratorPlanDraftArtifactApprovalDecisionKind.Approved,
            GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected => GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected,
            GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested => GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested,
            GeneratorPlanDraftArtifactApprovalDecisionKind.Pending => GeneratorPlanDraftArtifactApprovalDecisionKind.Pending,
            _ => GeneratorPlanDraftArtifactApprovalDecisionKind.Pending
        };
    }
}
