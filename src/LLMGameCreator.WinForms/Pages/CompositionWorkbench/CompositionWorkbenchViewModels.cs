using LLMGameCreator.Application.Composition;

namespace LLMGameCreator.WinForms.Pages.CompositionWorkbench;

public sealed record CompositionWorkbenchViewState
{
    public IReadOnlyList<CompositionWorkbenchPresetOption> Presets { get; init; }
        = Array.Empty<CompositionWorkbenchPresetOption>();
    public string SelectedPresetId { get; init; } = string.Empty;
    public IReadOnlyList<CompositionWorkbenchSavedReportOption> SavedReports { get; init; }
        = Array.Empty<CompositionWorkbenchSavedReportOption>();
    public string SelectedReportFileName { get; init; } = string.Empty;
    public string ProjectRootPath { get; init; } = string.Empty;
    public string Readiness { get; init; } = "Not built";
    public string Summary { get; init; } = "Select a blueprint preset and build a preview report.";
    public string Markdown { get; init; } = string.Empty;
    public string Status { get; init; } = "Not loaded.";
    public bool CanExport => !string.IsNullOrWhiteSpace(ProjectRootPath) && !string.IsNullOrWhiteSpace(SelectedPresetId);
}

public sealed record CompositionWorkbenchPresetOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Id : $"{Title} ({Id})";
}

public sealed record CompositionWorkbenchSavedReportOption
{
    public string BlueprintId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public GameCompositionReadiness Readiness { get; init; }
    public string ContentLanguage { get; init; } = string.Empty;
    public string ReportFileName { get; init; } = string.Empty;
    public string DisplayName => $"{Title} | {Readiness} | {ContentLanguage}";
}
