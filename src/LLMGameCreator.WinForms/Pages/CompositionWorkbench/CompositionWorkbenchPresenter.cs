using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;

namespace LLMGameCreator.WinForms.Pages.CompositionWorkbench;

public sealed class CompositionWorkbenchPresenter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly GameBlueprintPresetProvider _presetProvider;
    private readonly GameCompositionDiagnosticsService _diagnosticsService;
    private readonly GameCompositionDiagnosticsMarkdownRenderer _markdownRenderer;
    private readonly GameCompositionDiagnosticsExportService _exportService;

    public CompositionWorkbenchPresenter(
        GameBlueprintPresetProvider presetProvider,
        GameCompositionDiagnosticsService diagnosticsService,
        GameCompositionDiagnosticsMarkdownRenderer markdownRenderer,
        GameCompositionDiagnosticsExportService exportService)
    {
        _presetProvider = presetProvider ?? throw new ArgumentNullException(nameof(presetProvider));
        _diagnosticsService = diagnosticsService ?? throw new ArgumentNullException(nameof(diagnosticsService));
        _markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    }

    public CompositionWorkbenchViewState Initialize(string? projectRootPath)
    {
        var presets = _presetProvider.List()
            .Select(preset => new CompositionWorkbenchPresetOption
            {
                Id = preset.BlueprintId,
                Title = preset.Title
            })
            .ToList();
        var root = NormalizeProjectRoot(projectRootPath);

        return new CompositionWorkbenchViewState
        {
            Presets = presets,
            SelectedPresetId = presets.FirstOrDefault()?.Id ?? string.Empty,
            ProjectRootPath = root,
            Status = string.IsNullOrWhiteSpace(root)
                ? "No current project is loaded. In-memory preview is available; export and saved reports require a project."
                : $"Current project: {root}"
        };
    }

    public CompositionWorkbenchViewState BuildPreview(CompositionWorkbenchViewState state, string? presetId = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        var selectedId = string.IsNullOrWhiteSpace(presetId) ? state.SelectedPresetId : presetId.Trim();
        if (!_presetProvider.TryGet(selectedId, out var blueprint))
        {
            return state with { Status = $"Blueprint preset '{selectedId}' was not found." };
        }

        var report = _diagnosticsService.CreateReport(blueprint);
        return state with
        {
            SelectedPresetId = blueprint.BlueprintId,
            Readiness = report.Readiness.ToString(),
            Summary = BuildSummary(report),
            Markdown = _markdownRenderer.Render(report),
            Status = string.IsNullOrWhiteSpace(state.ProjectRootPath)
                ? "Preview report built in memory. Load a project to export it."
                : "Preview report built in memory."
        };
    }

    public async Task<CompositionWorkbenchViewState> ExportAsync(
        CompositionWorkbenchViewState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (string.IsNullOrWhiteSpace(state.ProjectRootPath))
        {
            return state with { Status = "No current project is loaded. Export is unavailable." };
        }

        if (!_presetProvider.TryGet(state.SelectedPresetId, out var blueprint))
        {
            return state with { Status = $"Blueprint preset '{state.SelectedPresetId}' was not found." };
        }

        var report = _diagnosticsService.CreateReport(blueprint);
        var result = await _exportService.ExportAsync(new GameCompositionDiagnosticsExportRequest
        {
            ProjectRootPath = state.ProjectRootPath,
            Report = report
        }, cancellationToken).ConfigureAwait(false);
        var refreshed = await RefreshSavedReportsAsync(
            state with
            {
                Readiness = report.Readiness.ToString(),
                Summary = BuildSummary(report),
                Markdown = _markdownRenderer.Render(report),
                SelectedReportFileName = result.IndexEntry.ReportFileName
            },
            cancellationToken).ConfigureAwait(false);

        return refreshed with
        {
            SelectedReportFileName = result.IndexEntry.ReportFileName,
            Status = $"Report exported: {result.MarkdownPath}"
        };
    }

    public async Task<CompositionWorkbenchViewState> RefreshSavedReportsAsync(
        CompositionWorkbenchViewState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (string.IsNullOrWhiteSpace(state.ProjectRootPath))
        {
            return state with
            {
                SavedReports = Array.Empty<CompositionWorkbenchSavedReportOption>(),
                SelectedReportFileName = string.Empty,
                Status = "No current project is loaded. Saved reports are unavailable."
            };
        }

        var indexPath = GetIndexPath(state.ProjectRootPath);
        if (!File.Exists(indexPath))
        {
            return state with
            {
                SavedReports = Array.Empty<CompositionWorkbenchSavedReportOption>(),
                SelectedReportFileName = string.Empty,
                Status = "No saved composition reports were found for the current project."
            };
        }

        try
        {
            await using var stream = File.OpenRead(indexPath);
            var index = await JsonSerializer.DeserializeAsync<GameCompositionDiagnosticsExportIndex>(
                stream,
                JsonOptions,
                cancellationToken).ConfigureAwait(false) ?? new GameCompositionDiagnosticsExportIndex();
            var savedReports = index.Entries
                .Where(entry => IsSafeReportFileName(entry.ReportFileName))
                .Select(entry => new CompositionWorkbenchSavedReportOption
                {
                    BlueprintId = entry.BlueprintId,
                    Title = entry.Title,
                    Readiness = entry.Readiness,
                    ContentLanguage = entry.ContentLanguage,
                    ReportFileName = entry.ReportFileName
                })
                .OrderBy(entry => entry.BlueprintId, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var selected = savedReports.FirstOrDefault(entry =>
                               string.Equals(entry.ReportFileName, state.SelectedReportFileName, StringComparison.OrdinalIgnoreCase))
                           ?? savedReports.FirstOrDefault();
            var refreshed = state with
            {
                SavedReports = savedReports,
                SelectedReportFileName = selected?.ReportFileName ?? string.Empty,
                Status = savedReports.Count == 0
                    ? "The composition report index contains no safe report entries."
                    : $"Saved reports refreshed: {savedReports.Count}."
            };

            return selected is null
                ? refreshed
                : await LoadSavedReportAsync(refreshed, selected.ReportFileName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return state with { Status = $"Saved reports could not be read: {ex.Message}" };
        }
    }

    public async Task<CompositionWorkbenchViewState> LoadSavedReportAsync(
        CompositionWorkbenchViewState state,
        string? reportFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        var selected = state.SavedReports.FirstOrDefault(entry =>
            string.Equals(entry.ReportFileName, reportFileName, StringComparison.OrdinalIgnoreCase));
        if (selected is null || string.IsNullOrWhiteSpace(state.ProjectRootPath))
        {
            return state with { Status = "The selected saved report is unavailable." };
        }

        var reportPath = GetReportPath(state.ProjectRootPath, selected.ReportFileName);
        if (!File.Exists(reportPath))
        {
            return state with { Status = $"Saved report file was not found: {selected.ReportFileName}" };
        }

        try
        {
            var markdown = await File.ReadAllTextAsync(reportPath, cancellationToken).ConfigureAwait(false);
            return state with
            {
                SelectedReportFileName = selected.ReportFileName,
                SelectedPresetId = selected.BlueprintId,
                Readiness = selected.Readiness.ToString(),
                Markdown = markdown,
                Status = $"Saved report loaded: {selected.ReportFileName}"
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return state with { Status = $"Saved report could not be read: {ex.Message}" };
        }
    }

    private static string BuildSummary(GameCompositionDiagnosticsReport report)
    {
        var lines = new List<string>
        {
            $"Blueprint: {report.Title} ({report.BlueprintId})",
            $"Readiness: {report.Readiness}",
            $"Diagnostics: {report.Diagnostics.Count}",
            $"Current generators: {report.SelectedCurrentGeneratorIds.Count}",
            $"Planned generators: {report.RelatedPlannedGeneratorIds.Count}",
            "Recommended actions:"
        };
        lines.AddRange(report.RecommendedActions.Select(action => $"- {action.Message}"));
        return string.Join(Environment.NewLine, lines);
    }

    private static string NormalizeProjectRoot(string? projectRootPath)
    {
        return string.IsNullOrWhiteSpace(projectRootPath) ? string.Empty : Path.GetFullPath(projectRootPath);
    }

    private static string GetIndexPath(string projectRootPath)
    {
        return Path.Combine(
            Path.GetFullPath(projectRootPath),
            ".llmgc",
            "composition-diagnostics",
            GameCompositionDiagnosticsExportService.IndexFileName);
    }

    private static string GetReportPath(string projectRootPath, string reportFileName)
    {
        if (!IsSafeReportFileName(reportFileName))
        {
            throw new InvalidOperationException("Saved report file name is unsafe.");
        }

        return Path.Combine(Path.GetDirectoryName(GetIndexPath(projectRootPath))!, reportFileName);
    }

    private static bool IsSafeReportFileName(string? reportFileName)
    {
        return !string.IsNullOrWhiteSpace(reportFileName)
               && string.Equals(Path.GetFileName(reportFileName), reportFileName, StringComparison.Ordinal)
               && reportFileName.EndsWith(".composition-report.md", StringComparison.OrdinalIgnoreCase);
    }
}
