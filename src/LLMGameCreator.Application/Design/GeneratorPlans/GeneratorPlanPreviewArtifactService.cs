using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewArtifactService
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

    private readonly GeneratorPlanPreviewService _previewService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanPreviewArtifactService(
        GeneratorPlanPreviewService previewService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _previewService = previewService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanPreviewArtifactResult> CaptureAsync(
        GeneratorPlanPreviewArtifactRequest request,
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

        var previewResult = await _previewService.PreviewAsync(request.PreviewRequest, cancellationToken).ConfigureAwait(false);
        var resultArtifact = BuildResultArtifact(request, previewResult);
        var markdownArtifact = BuildMarkdownArtifact(request, previewResult);
        var validationResults = GeneratorPlanPreviewValidationPolicy.ToValidationResults(resultArtifact.Id, previewResult.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(resultArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(resultArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanPreviewArtifactResult
        {
            PreviewResult = previewResult,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildResultArtifact(
        GeneratorPlanPreviewArtifactRequest request,
        GeneratorPlanPreviewResult previewResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanPreviewArtifactSnapshot
        {
            GeneratedAtUtc = previewResult.GeneratedAtUtc,
            Ok = previewResult.Ok,
            Status = previewResult.Status,
            Preview = previewResult.Preview,
            Diagnostics = previewResult.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.ResultArtifactId.Trim(),
            GeneratorPlanPreviewArtifactIds.ResultArtifactKind,
            GeneratorPlanPreviewArtifactIds.ResultArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            previewResult.Status,
            BuildMetadataJson(previewResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanPreviewArtifactRequest request,
        GeneratorPlanPreviewResult previewResult)
    {
        if (string.IsNullOrWhiteSpace(previewResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanPreviewArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        var json = JsonSerializer.Serialize(new GeneratorPlanPreviewMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = previewResult.GeneratedAtUtc,
            Markdown = previewResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanPreviewArtifactIds.MarkdownArtifactKind,
            GeneratorPlanPreviewArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            previewResult.Status,
            BuildMetadataJson(previewResult));
    }

    private static string BuildMetadataJson(GeneratorPlanPreviewResult previewResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = previewResult.GeneratedAtUtc,
            sourcePath = previewResult.Preview.SourcePath,
            exampleId = previewResult.Preview.ExampleId,
            title = previewResult.Preview.Title,
            stepCount = previewResult.Preview.Summary.StepCount,
            targetArtifactCount = previewResult.Preview.Summary.TargetArtifactCount,
            featureBundleCount = previewResult.Preview.Summary.FeatureBundleCount,
            errorCount = previewResult.Preview.Summary.ErrorCount,
            warningCount = previewResult.Preview.Summary.WarningCount
        }, JsonOptions);
    }

    private sealed record GeneratorPlanPreviewArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanPreview Preview { get; init; } = new();
        public IReadOnlyList<GeneratorPlanPreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPreviewDiagnostic>();
    }

    private sealed record GeneratorPlanPreviewMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
