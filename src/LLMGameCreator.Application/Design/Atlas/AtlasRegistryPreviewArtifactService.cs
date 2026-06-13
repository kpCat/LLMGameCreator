using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryPreviewArtifactService
{
    internal static readonly GeneratedArtifactRecord EmptyArtifact = new(
        string.Empty,
        string.Empty,
        string.Empty,
        "{}",
        string.Empty,
        string.Empty,
        "{}");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly AtlasRegistryPreviewService _previewService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public AtlasRegistryPreviewArtifactService(
        AtlasRegistryPreviewService previewService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _previewService = previewService;
        _artifactRepository = artifactRepository;
    }

    public async Task<AtlasRegistryPreviewArtifactResult> CaptureAsync(
        AtlasRegistryPreviewArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ResultArtifactId))
        {
            throw new ArgumentException("Result artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.GeneratedBy))
        {
            throw new ArgumentException("GeneratedBy is required.", nameof(request));
        }

        var previewResult = await _previewService
            .PreviewAsync(request.PreviewRequest, cancellationToken)
            .ConfigureAwait(false);

        var resultArtifact = BuildResultArtifact(request, previewResult);
        var markdownArtifact = BuildMarkdownArtifact(request, previewResult);
        var validationResults = BuildValidationResults(resultArtifact.Id, previewResult.ImportResult.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(resultArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(resultArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new AtlasRegistryPreviewArtifactResult
        {
            PreviewResult = previewResult,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildResultArtifact(
        AtlasRegistryPreviewArtifactRequest request,
        AtlasRegistryPreviewResult previewResult)
    {
        var json = JsonSerializer.Serialize(new AtlasRegistryPreviewArtifactSnapshot
        {
            GeneratedAtUtc = previewResult.GeneratedAtUtc,
            Ok = previewResult.Ok,
            AtlasRoot = previewResult.ImportResult.AtlasRoot,
            Summary = previewResult.ImportResult.Summary,
            Documents = previewResult.ImportResult.Documents,
            Examples = previewResult.ImportResult.Examples,
            Diagnostics = previewResult.ImportResult.Diagnostics,
            WrittenFiles = previewResult.WrittenFiles
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.ResultArtifactId.Trim(),
            AtlasRegistryPreviewArtifactIds.ResultArtifactKind,
            AtlasRegistryPreviewArtifactIds.ResultArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            ToValidationState(previewResult.ImportResult.Summary),
            BuildMetadataJson(previewResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        AtlasRegistryPreviewArtifactRequest request,
        AtlasRegistryPreviewResult previewResult)
    {
        if (string.IsNullOrWhiteSpace(previewResult.MarkdownReport))
        {
            return null;
        }

        var json = JsonSerializer.Serialize(new AtlasRegistryMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = previewResult.GeneratedAtUtc,
            Markdown = previewResult.MarkdownReport
        }, JsonOptions);

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? AtlasRegistryPreviewArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        return new GeneratedArtifactRecord(
            id,
            AtlasRegistryPreviewArtifactIds.MarkdownArtifactKind,
            AtlasRegistryPreviewArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            ToValidationState(previewResult.ImportResult.Summary),
            BuildMetadataJson(previewResult));
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> BuildValidationResults(
        string artifactId,
        IReadOnlyList<AtlasDiagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic => diagnostic.Severity is AtlasDiagnosticSeverity.Error or AtlasDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Id, StringComparer.OrdinalIgnoreCase)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.Path ?? string.Empty, diagnostic.Id ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Path ?? diagnostic.Id ?? artifactId,
                BuildDiagnosticMetadataJson(diagnostic)))
            .ToList();
    }

    private static string ToValidationState(AtlasRegistrySummary summary)
    {
        if (summary.ErrorCount > 0)
        {
            return "invalid";
        }

        return summary.WarningCount > 0 ? "warnings" : "valid";
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            AtlasDiagnosticSeverity.Error => 0,
            AtlasDiagnosticSeverity.Warning => 1,
            AtlasDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }

    private static string BuildMetadataJson(AtlasRegistryPreviewResult previewResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = previewResult.GeneratedAtUtc,
            atlasRoot = previewResult.ImportResult.AtlasRoot,
            documentCount = previewResult.ImportResult.Summary.DocumentCount,
            loadedDocumentCount = previewResult.ImportResult.Summary.LoadedDocumentCount,
            exampleCount = previewResult.ImportResult.Summary.ExampleCount,
            uniqueIdCount = previewResult.ImportResult.Summary.UniqueIdCount,
            errorCount = previewResult.ImportResult.Summary.ErrorCount,
            warningCount = previewResult.ImportResult.Summary.WarningCount,
            writtenFiles = previewResult.WrittenFiles
        }, JsonOptions);
    }

    private static string BuildDiagnosticMetadataJson(AtlasDiagnostic diagnostic)
    {
        return JsonSerializer.Serialize(new
        {
            path = diagnostic.Path,
            id = diagnostic.Id
        }, JsonOptions);
    }

    private static string StableId(params string[] parts)
    {
        var text = string.Join("|", parts);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record AtlasRegistryPreviewArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string AtlasRoot { get; init; } = string.Empty;
        public AtlasRegistrySummary Summary { get; init; } = new();
        public IReadOnlyList<AtlasDocumentSummary> Documents { get; init; } = Array.Empty<AtlasDocumentSummary>();
        public IReadOnlyList<AtlasExampleSummary> Examples { get; init; } = Array.Empty<AtlasExampleSummary>();
        public IReadOnlyList<AtlasDiagnostic> Diagnostics { get; init; } = Array.Empty<AtlasDiagnostic>();
        public IReadOnlyList<string> WrittenFiles { get; init; } = Array.Empty<string>();
    }

    private sealed record AtlasRegistryMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
