using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryPreviewService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly AtlasRegistryImportService _importService;
    private readonly AtlasRegistryMarkdownReportRenderer _markdownRenderer;

    public AtlasRegistryPreviewService()
        : this(new AtlasRegistryImportService(), new AtlasRegistryMarkdownReportRenderer())
    {
    }

    public AtlasRegistryPreviewService(
        AtlasRegistryImportService importService,
        AtlasRegistryMarkdownReportRenderer markdownRenderer)
    {
        _importService = importService;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<AtlasRegistryPreviewResult> PreviewAsync(
        AtlasRegistryPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RepositoryRootOrAtlasRoot))
        {
            throw new ArgumentException("Repository root or atlas root is required.", nameof(request));
        }

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var importResult = await _importService
            .ImportAtlasRegistryAsync(request.RepositoryRootOrAtlasRoot, cancellationToken)
            .ConfigureAwait(false);

        var markdown = request.RenderMarkdown
            ? _markdownRenderer.Render(importResult)
            : string.Empty;

        var writtenFiles = request.WriteReportFiles
            ? await WriteReportFilesAsync(request, importResult, markdown, generatedAtUtc, cancellationToken).ConfigureAwait(false)
            : Array.Empty<string>();

        return new AtlasRegistryPreviewResult
        {
            Ok = importResult.Ok,
            GeneratedAtUtc = generatedAtUtc,
            ImportResult = importResult,
            MarkdownReport = markdown,
            WrittenFiles = writtenFiles
        };
    }

    private static async Task<IReadOnlyList<string>> WriteReportFilesAsync(
        AtlasRegistryPreviewRequest request,
        AtlasRegistryImportResult importResult,
        string markdown,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var outputRoot = ResolveReportOutputRoot(request, importResult);
        Directory.CreateDirectory(outputRoot);

        var markdownPath = Path.Combine(outputRoot, "atlas_registry_import_report.md");
        var jsonPath = Path.Combine(outputRoot, "atlas_registry_import_result.json");

        if (!string.IsNullOrWhiteSpace(markdown))
        {
            await File.WriteAllTextAsync(markdownPath, markdown, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await File.WriteAllTextAsync(markdownPath, "# Atlas Registry Import Report\n\nMarkdown rendering was disabled.\n", cancellationToken).ConfigureAwait(false);
        }

        var jsonSnapshot = new AtlasRegistryPreviewSnapshot
        {
            GeneratedAtUtc = generatedAtUtc,
            Ok = importResult.Ok,
            AtlasRoot = importResult.AtlasRoot,
            Summary = importResult.Summary,
            Documents = importResult.Documents,
            Examples = importResult.Examples,
            Diagnostics = importResult.Diagnostics
        };

        var json = JsonSerializer.Serialize(jsonSnapshot, JsonOptions);
        await File.WriteAllTextAsync(jsonPath, json, cancellationToken).ConfigureAwait(false);

        return new[] { markdownPath, jsonPath };
    }

    private static string ResolveReportOutputRoot(
        AtlasRegistryPreviewRequest request,
        AtlasRegistryImportResult importResult)
    {
        if (!string.IsNullOrWhiteSpace(request.ReportOutputRoot))
        {
            return Path.GetFullPath(request.ReportOutputRoot);
        }

        var repositoryRoot = TryResolveRepositoryRootFromAtlasRoot(importResult.AtlasRoot)
                             ?? TryResolveRepositoryRootFromInput(request.RepositoryRootOrAtlasRoot)
                             ?? Path.GetFullPath(request.RepositoryRootOrAtlasRoot);

        return Path.Combine(repositoryRoot, ".llmgc", "atlas");
    }

    private static string? TryResolveRepositoryRootFromAtlasRoot(string atlasRoot)
    {
        if (string.IsNullOrWhiteSpace(atlasRoot))
        {
            return null;
        }

        var directory = new DirectoryInfo(Path.GetFullPath(atlasRoot));
        if (!directory.Exists ||
            !directory.Name.Equals("atlas", StringComparison.OrdinalIgnoreCase) ||
            directory.Parent == null ||
            !directory.Parent.Name.Equals("generator-library", StringComparison.OrdinalIgnoreCase) ||
            directory.Parent.Parent == null)
        {
            return null;
        }

        return directory.Parent.Parent.FullName;
    }

    private static string? TryResolveRepositoryRootFromInput(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return null;
        }

        var directory = new DirectoryInfo(Path.GetFullPath(inputPath));
        if (!directory.Exists)
        {
            return null;
        }

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "generator-library", "atlas")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private sealed record AtlasRegistryPreviewSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string AtlasRoot { get; init; } = string.Empty;
        public AtlasRegistrySummary Summary { get; init; } = new();
        public IReadOnlyList<AtlasDocumentSummary> Documents { get; init; } = Array.Empty<AtlasDocumentSummary>();
        public IReadOnlyList<AtlasExampleSummary> Examples { get; init; } = Array.Empty<AtlasExampleSummary>();
        public IReadOnlyList<AtlasDiagnostic> Diagnostics { get; init; } = Array.Empty<AtlasDiagnostic>();
    }
}
