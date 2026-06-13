using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueArtifactService
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

    private readonly GeneratorPlanDraftArtifactQueueService _queueService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactQueueArtifactService(
        GeneratorPlanDraftArtifactQueueService queueService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _queueService = queueService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactQueueArtifactResult> CaptureAsync(
        GeneratorPlanDraftArtifactQueueArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PreviewRequest.SourcePath))
        {
            throw new ArgumentException("Preview source path is required.", nameof(request));
        }

        var queueResult = await _queueService
            .CreateQueueFromExampleAsync(request.PreviewRequest.SourcePath, request.QueueRequest, cancellationToken)
            .ConfigureAwait(false);

        return await SaveAsync(
            queueResult,
            new GeneratorPlanDraftArtifactQueueArtifactSaveRequest
            {
                ResultArtifactId = request.ResultArtifactId,
                MarkdownArtifactId = request.MarkdownArtifactId,
                GeneratedBy = request.GeneratedBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorPlanDraftArtifactQueueArtifactResult> SaveAsync(
        GeneratorPlanDraftArtifactQueueResult queueResult,
        GeneratorPlanDraftArtifactQueueArtifactSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queueResult);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ResultArtifactId))
        {
            throw new ArgumentException("Result artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.GeneratedBy))
        {
            throw new ArgumentException("GeneratedBy is required.", nameof(request));
        }

        var resultArtifact = BuildResultArtifact(request, queueResult);
        var markdownArtifact = BuildMarkdownArtifact(request, queueResult);
        var validationResults = GeneratorPlanDraftArtifactQueuePolicy.ToValidationResults(resultArtifact.Id, queueResult.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(resultArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(resultArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanDraftArtifactQueueArtifactResult
        {
            QueueResult = queueResult,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildResultArtifact(
        GeneratorPlanDraftArtifactQueueArtifactSaveRequest request,
        GeneratorPlanDraftArtifactQueueResult queueResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactQueueArtifactSnapshot
        {
            GeneratedAtUtc = queueResult.GeneratedAtUtc,
            Ok = queueResult.Ok,
            Status = queueResult.Status,
            DraftExecutionResult = queueResult.DraftExecutionResult,
            Queue = queueResult.Queue,
            Diagnostics = queueResult.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.ResultArtifactId.Trim(),
            GeneratorPlanDraftArtifactQueueArtifactIds.ResultArtifactKind,
            GeneratorPlanDraftArtifactQueueArtifactIds.ResultArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactQueuePolicy.ToValidationState(queueResult.Queue.Summary),
            BuildMetadataJson(queueResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanDraftArtifactQueueArtifactSaveRequest request,
        GeneratorPlanDraftArtifactQueueResult queueResult)
    {
        if (string.IsNullOrWhiteSpace(queueResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactQueueMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = queueResult.GeneratedAtUtc,
            Markdown = queueResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactKind,
            GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactQueuePolicy.ToValidationState(queueResult.Queue.Summary),
            BuildMetadataJson(queueResult));
    }

    private static string BuildMetadataJson(GeneratorPlanDraftArtifactQueueResult queueResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = queueResult.GeneratedAtUtc,
            queueId = queueResult.Queue.Id,
            sourceDraftExecutionPlanId = queueResult.Queue.SourceDraftExecutionPlanId,
            sourcePreviewExampleId = queueResult.Queue.SourcePreviewExampleId,
            sourcePath = queueResult.Queue.SourcePath,
            status = queueResult.Queue.Status,
            itemCount = queueResult.Queue.Summary.ItemCount,
            readyItemCount = queueResult.Queue.Summary.ReadyItemCount,
            blockedItemCount = queueResult.Queue.Summary.BlockedItemCount,
            validationGateCount = queueResult.Queue.Summary.ValidationGateCount,
            repairRequestCount = queueResult.Queue.Summary.RepairRequestCount,
            errorCount = queueResult.Queue.Summary.ErrorCount,
            warningCount = queueResult.Queue.Summary.WarningCount
        }, JsonOptions);
    }

    private sealed record GeneratorPlanDraftArtifactQueueArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanDraftExecutionResult DraftExecutionResult { get; init; } = new();
        public GeneratorPlanDraftArtifactQueue Queue { get; init; } = new();
        public IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactQueueDiagnostic>();
    }

    private sealed record GeneratorPlanDraftArtifactQueueMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
