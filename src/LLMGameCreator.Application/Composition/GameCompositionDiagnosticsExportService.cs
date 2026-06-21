using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class GameCompositionDiagnosticsExportService
{
    public const string RelativeOutputDirectory = ".llmgc/composition-diagnostics";
    public const string IndexFileName = "index.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly GameCompositionDiagnosticsMarkdownRenderer _renderer;

    public GameCompositionDiagnosticsExportService(GameCompositionDiagnosticsMarkdownRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public async Task<GameCompositionDiagnosticsExportResult> ExportAsync(
        GameCompositionDiagnosticsExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Report);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var projectRoot = Path.GetFullPath(request.ProjectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "composition-diagnostics"));
        EnsureContained(projectRoot, outputDirectory, "Composition diagnostics output directory");
        Directory.CreateDirectory(outputDirectory);

        var safeBlueprintId = SanitizeBlueprintId(request.Report.BlueprintId);
        var reportFileName = $"{safeBlueprintId}.composition-report.md";
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, reportFileName));
        var indexPath = Path.GetFullPath(Path.Combine(outputDirectory, IndexFileName));
        EnsureContained(outputDirectory, markdownPath, "Composition diagnostics markdown path");
        EnsureContained(outputDirectory, indexPath, "Composition diagnostics index path");

        var markdown = _renderer.Render(request.Report);
        await File.WriteAllTextAsync(markdownPath, markdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        var entry = new GameCompositionDiagnosticsExportIndexEntry
        {
            BlueprintId = request.Report.BlueprintId.Trim(),
            Title = request.Report.Title.Trim(),
            Readiness = request.Report.Readiness,
            ContentLanguage = request.Report.ContentLanguage.Trim(),
            ReportFileName = reportFileName
        };
        var index = await ReadIndexAsync(indexPath, cancellationToken).ConfigureAwait(false);
        var entries = index.Entries
            .Where(existing => !string.Equals(existing.BlueprintId, entry.BlueprintId, StringComparison.OrdinalIgnoreCase))
            .Append(entry)
            .OrderBy(existing => existing.BlueprintId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(existing => existing.BlueprintId, StringComparer.Ordinal)
            .ToList();
        var updatedIndex = index with { Entries = entries };
        var indexJson = JsonSerializer.Serialize(updatedIndex, JsonOptions);
        await File.WriteAllTextAsync(indexPath, indexJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new GameCompositionDiagnosticsExportResult
        {
            OutputDirectoryPath = outputDirectory,
            MarkdownPath = markdownPath,
            IndexPath = indexPath,
            IndexEntry = entry
        };
    }

    private static async Task<GameCompositionDiagnosticsExportIndex> ReadIndexAsync(
        string indexPath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(indexPath))
        {
            return new GameCompositionDiagnosticsExportIndex();
        }

        await using var stream = File.OpenRead(indexPath);
        return await JsonSerializer.DeserializeAsync<GameCompositionDiagnosticsExportIndex>(stream, JsonOptions, cancellationToken)
                   .ConfigureAwait(false)
               ?? new GameCompositionDiagnosticsExportIndex();
    }

    private static string SanitizeBlueprintId(string? blueprintId)
    {
        var source = blueprintId?.Trim() ?? string.Empty;
        var builder = new StringBuilder(source.Length);
        var previousWasReplacement = false;
        foreach (var character in source)
        {
            var isSafe = character is >= 'a' and <= 'z'
                or >= 'A' and <= 'Z'
                or >= '0' and <= '9'
                or '.' or '_' or '-';
            if (isSafe)
            {
                builder.Append(character);
                previousWasReplacement = false;
            }
            else if (!previousWasReplacement)
            {
                builder.Append('-');
                previousWasReplacement = true;
            }
        }

        var safe = builder.ToString().Trim('.', '-', '_');
        return string.IsNullOrWhiteSpace(safe) ? "blueprint" : safe;
    }

    private static void EnsureContained(string rootPath, string candidatePath, string pathLabel)
    {
        var normalizedRoot = Path.GetFullPath(rootPath);
        var normalizedCandidate = Path.GetFullPath(candidatePath);
        var relativePath = Path.GetRelativePath(normalizedRoot, normalizedCandidate);
        if (Path.IsPathRooted(relativePath) ||
            string.Equals(relativePath, "..", StringComparison.Ordinal) ||
            relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{pathLabel} must stay under '{normalizedRoot}'.");
        }
    }
}
