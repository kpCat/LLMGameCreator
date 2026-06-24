using LLMGameCreator.Application.Composition;

namespace LLMGameCreator.WinForms.Pages.UnityArchiveReview;

public enum UnityArchiveManualImportSlotFilter
{
    All,
    Missing,
    Available,
    Invalid,
    ManualImportProvider,
    FutureProviders
}

public sealed record UnityArchiveReviewViewState
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ArchiveRoot { get; init; } = string.Empty;
    public string Status { get; init; } = "No current project is loaded.";
    public string CurrentReviewReadiness { get; init; } = "Unavailable";
    public string ComparisonReadiness { get; init; } = "Unavailable";
    public int HistorySnapshotCount { get; init; }
    public string SelectedSnapshotId { get; init; } = string.Empty;
    public string SelectedSnapshotJson { get; init; } = string.Empty;
    public string SelectedSnapshotStatus { get; init; } = "Unavailable";
    public string SelectedSnapshotRelativePath { get; init; } = string.Empty;
    public int SelectedSnapshotSequence { get; init; }
    public IReadOnlyList<UnityArchiveReviewSnapshotOption> HistorySnapshots { get; init; }
        = Array.Empty<UnityArchiveReviewSnapshotOption>();
    public string CurrentReviewMarkdown { get; init; } = string.Empty;
    public string ComparisonMarkdown { get; init; } = string.Empty;
    public string CurrentReviewJson { get; init; } = string.Empty;
    public string ComparisonJson { get; init; } = string.Empty;
    public string HistoryIndexJson { get; init; } = string.Empty;
    public string ManualImportReportMarkdown { get; init; } = string.Empty;
    public string ManualImportReportJson { get; init; } = string.Empty;
    public string ManualImportReportStatus { get; init; } = "No manual import report yet.";
    public string ManualImportWorkspaceStatus { get; init; } = "No manual import workspace loaded.";
    public IReadOnlyList<UnityArchiveManualImportWorkspaceSlot> ManualImportSlots { get; init; }
        = Array.Empty<UnityArchiveManualImportWorkspaceSlot>();
    public IReadOnlyList<UnityArchiveManualImportWorkspaceSlot> VisibleManualImportSlots { get; init; }
        = Array.Empty<UnityArchiveManualImportWorkspaceSlot>();
    public UnityArchiveManualImportSlotFilter ManualImportSlotFilter { get; init; }
    public string SelectedManualImportSlotId { get; init; } = string.Empty;
    public string SelectedManualImportSlotDetail { get; init; } = string.Empty;
    public bool CanRefresh { get; init; }
    public bool CanOpenArchiveFolder { get; init; }
    public bool CanCreateManualImportTemplate { get; init; }
    public bool CanRunManualImport { get; init; }
    public bool CanOpenManualImportFolder { get; init; }
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
