using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.ArtifactReview;

public static class ArtifactReviewFilter
{
    public const string All = "All";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string RepairRequested = "Repair requested";
    public const string Blocked = "Blocked";
}

public sealed record ArtifactReviewViewState
{
    public bool Exists { get; init; }
    public string Message { get; init; } = string.Empty;
    public string SnapshotId { get; init; } = string.Empty;
    public string SourceExampleId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public int RepairRequestedCount { get; init; }
    public int BlockedCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public string Filter { get; init; } = ArtifactReviewFilter.All;
    public string SelectedArtifactId { get; init; } = string.Empty;
    public IReadOnlyList<ArtifactReviewRowViewModel> Rows { get; init; } = Array.Empty<ArtifactReviewRowViewModel>();
    public IReadOnlyList<ArtifactReviewRowViewModel> FilteredRows { get; init; } = Array.Empty<ArtifactReviewRowViewModel>();
    public ArtifactReviewDetailViewModel Detail { get; init; } = new();
}

public sealed record ArtifactReviewRowViewModel
{
    public string ArtifactId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Contract { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public bool RequiresApproval { get; init; }
    public string Issues { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
    public string Decision { get; init; } = string.Empty;
    public bool IsChanged { get; init; }
    public bool JsonIsValid { get; init; } = true;
    public bool CanApprove => State == GeneratorPlanDraftArtifactApprovalItemState.Pending && JsonIsValid && string.IsNullOrWhiteSpace(Issues);

    public static ArtifactReviewRowViewModel FromItem(GeneratorPlanDraftArtifactApprovalItem item)
    {
        return new ArtifactReviewRowViewModel
        {
            ArtifactId = item.ArtifactId,
            Kind = item.ArtifactKind,
            Contract = item.ExpectedArtifactContract,
            State = item.State,
            RequiresApproval = item.RequiresHumanApproval,
            Issues = string.Join(Environment.NewLine, item.ValidationIssues),
            ReasonCode = item.DecisionReasonCode,
            Comment = item.DecisionComment,
            ContentJson = item.ContentJson,
            Decision = item.State,
            JsonIsValid = IsValidJson(item.ContentJson)
        };
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? string.Empty : json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed record ArtifactReviewDetailViewModel
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ContentJson { get; init; } = string.Empty;
    public string ValidationIssues { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
}
