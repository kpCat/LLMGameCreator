namespace LLMGameCreator.WinForms.Pages.UnityArchiveReview;

public sealed record UnityArchiveReviewViewState
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ArchiveRoot { get; init; } = string.Empty;
    public string Status { get; init; } = "No current project is loaded.";
    public string CurrentReviewReadiness { get; init; } = "Unavailable";
    public string ComparisonReadiness { get; init; } = "Unavailable";
    public int HistorySnapshotCount { get; init; }
    public string SelectedSnapshotId { get; init; } = string.Empty;
    public IReadOnlyList<UnityArchiveReviewSnapshotOption> HistorySnapshots { get; init; }
        = Array.Empty<UnityArchiveReviewSnapshotOption>();
    public string CurrentReviewMarkdown { get; init; } = string.Empty;
    public string ComparisonMarkdown { get; init; } = string.Empty;
    public string CurrentReviewJson { get; init; } = string.Empty;
    public string ComparisonJson { get; init; } = string.Empty;
    public string HistoryIndexJson { get; init; } = string.Empty;
    public bool CanRefresh { get; init; }
    public bool CanOpenArchiveFolder { get; init; }
}

public sealed record UnityArchiveReviewSnapshotOption
{
    public int Sequence { get; init; }
    public string SnapshotId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool FileExists { get; init; }
    public string DisplayName => Sequence > 0
        ? $"{Sequence}: {SnapshotId}"
        : SnapshotId;
}
