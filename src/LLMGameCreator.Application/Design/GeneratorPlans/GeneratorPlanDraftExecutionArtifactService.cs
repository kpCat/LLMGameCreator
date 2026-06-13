using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionArtifactService
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
    private readonly GeneratorPlanDraftExecutionService _draftExecutionService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftExecutionArtifactService(
        GeneratorPlanPreviewService previewService,
        GeneratorPlanDraftExecutionService draftExecutionService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _previewService = previewService;
        _draftExecutionService = draftExecutionService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftExecutionArtifactResult> CaptureAsync(
        GeneratorPlanDraftExecutionArtifactRequest request,
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
        var draftResult = await _draftExecutionService.CreateDraftAsync(previewResult, request.DraftRequest, cancellationToken).ConfigureAwait(false);
        var resultArtifact = BuildResultArtifact(request, draftResult);
        var markdownArtifact = BuildMarkdownArtifact(request, draftResult);
        var validationResults = GeneratorPlanDraftExecutionPolicy.ToValidationResults(resultArtifact.Id, draftResult.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(resultArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(resultArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanDraftExecutionArtifactResult
        {
            DraftResult = draftResult,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildResultArtifact(
        GeneratorPlanDraftExecutionArtifactRequest request,
        GeneratorPlanDraftExecutionResult draftResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanDraftExecutionArtifactSnapshot
        {
            GeneratedAtUtc = draftResult.GeneratedAtUtc,
            Ok = draftResult.Ok,
            Status = draftResult.Status,
            Plan = draftResult.Plan,
            Diagnostics = draftResult.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.ResultArtifactId.Trim(),
            GeneratorPlanDraftExecutionArtifactIds.ResultArtifactKind,
            GeneratorPlanDraftExecutionArtifactIds.ResultArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftExecutionPolicy.ToValidationState(draftResult.Plan.Summary),
            BuildMetadataJson(draftResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanDraftExecutionArtifactRequest request,
        GeneratorPlanDraftExecutionResult draftResult)
    {
        if (string.IsNullOrWhiteSpace(draftResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanDraftExecutionArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        var json = JsonSerializer.Serialize(new GeneratorPlanDraftExecutionMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = draftResult.GeneratedAtUtc,
            Markdown = draftResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanDraftExecutionArtifactIds.MarkdownArtifactKind,
            GeneratorPlanDraftExecutionArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftExecutionPolicy.ToValidationState(draftResult.Plan.Summary),
            BuildMetadataJson(draftResult));
    }

    private static string BuildMetadataJson(GeneratorPlanDraftExecutionResult draftResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = draftResult.GeneratedAtUtc,
            planId = draftResult.Plan.Id,
            sourcePreviewExampleId = draftResult.Plan.SourcePreviewExampleId,
            sourcePath = draftResult.Plan.SourcePath,
            status = draftResult.Plan.Status,
            stepCount = draftResult.Plan.Summary.StepCount,
            pendingStepCount = draftResult.Plan.Summary.PendingStepCount,
            blockedStepCount = draftResult.Plan.Summary.BlockedStepCount,
            plannedArtifactCount = draftResult.Plan.Summary.PlannedArtifactCount,
            repairRequestCount = draftResult.Plan.Summary.RepairRequestCount,
            errorCount = draftResult.Plan.Summary.ErrorCount,
            warningCount = draftResult.Plan.Summary.WarningCount
        }, JsonOptions);
    }

    private sealed record GeneratorPlanDraftExecutionArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanDraftExecutionPlan Plan { get; init; } = new();
        public IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftExecutionDiagnostic>();
    }

    private sealed record GeneratorPlanDraftExecutionMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
