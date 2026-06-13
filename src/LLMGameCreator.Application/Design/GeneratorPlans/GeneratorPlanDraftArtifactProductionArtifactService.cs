using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactProductionArtifactService
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

    private readonly GeneratorPlanDraftArtifactProductionService _productionService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactProductionArtifactService(
        GeneratorPlanDraftArtifactProductionService productionService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _productionService = productionService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactProductionArtifactResult> CaptureAsync(
        GeneratorPlanDraftArtifactProductionArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PreviewRequest.SourcePath))
        {
            throw new ArgumentException("Preview source path is required.", nameof(request));
        }

        var productionResult = await _productionService
            .ProduceFromExampleAsync(request.PreviewRequest.SourcePath, request.ProductionRequest, cancellationToken)
            .ConfigureAwait(false);

        return await SaveAsync(
            productionResult,
            new GeneratorPlanDraftArtifactProductionArtifactSaveRequest
            {
                BatchArtifactId = request.BatchArtifactId,
                MarkdownArtifactId = request.MarkdownArtifactId,
                GeneratedBy = request.GeneratedBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorPlanDraftArtifactProductionArtifactResult> SaveAsync(
        GeneratorPlanDraftArtifactProductionResult productionResult,
        GeneratorPlanDraftArtifactProductionArtifactSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productionResult);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.BatchArtifactId))
        {
            throw new ArgumentException("Batch artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.GeneratedBy))
        {
            throw new ArgumentException("GeneratedBy is required.", nameof(request));
        }

        var batchArtifact = BuildBatchArtifact(request, productionResult);
        var markdownArtifact = BuildMarkdownArtifact(request, productionResult);
        var producedArtifacts = productionResult.Batch.Artifacts
            .Select(artifact => BuildProducedArtifact(request, productionResult.Batch, artifact))
            .ToList();
        var validationResults = new List<GeneratedArtifactValidationResultRecord>();
        var batchDiagnostics = productionResult.Diagnostics
            .Where(diagnostic => string.IsNullOrWhiteSpace(diagnostic.ArtifactId)
                || string.Equals(diagnostic.BatchId, productionResult.Batch.Id, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var batchValidationResults = GeneratorPlanDraftArtifactProductionPolicy.ToValidationResults(batchArtifact.Id, batchDiagnostics);

        validationResults.AddRange(batchValidationResults);

        await _artifactRepository.SaveGeneratedArtifactAsync(batchArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(batchArtifact.Id, batchValidationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        foreach (var producedArtifact in producedArtifacts)
        {
            var diagnostics = productionResult.Diagnostics
                .Where(diagnostic => string.Equals(diagnostic.ArtifactId, producedArtifact.Id, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var producedValidationResults = GeneratorPlanDraftArtifactProductionPolicy.ToValidationResults(producedArtifact.Id, diagnostics);

            await _artifactRepository.SaveGeneratedArtifactAsync(producedArtifact, cancellationToken).ConfigureAwait(false);
            await _artifactRepository.SaveValidationResultsAsync(producedArtifact.Id, producedValidationResults, cancellationToken).ConfigureAwait(false);
            validationResults.AddRange(producedValidationResults);
        }

        return new GeneratorPlanDraftArtifactProductionArtifactResult
        {
            ProductionResult = productionResult,
            BatchArtifact = batchArtifact,
            MarkdownArtifact = markdownArtifact,
            ProducedArtifacts = producedArtifacts,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildBatchArtifact(
        GeneratorPlanDraftArtifactProductionArtifactSaveRequest request,
        GeneratorPlanDraftArtifactProductionResult productionResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactProductionArtifactSnapshot
        {
            GeneratedAtUtc = productionResult.GeneratedAtUtc,
            Ok = productionResult.Ok,
            Status = productionResult.Status,
            QueueResult = productionResult.QueueResult,
            Batch = productionResult.Batch,
            Diagnostics = productionResult.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.BatchArtifactId.Trim(),
            GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactKind,
            GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactProductionPolicy.ToValidationState(productionResult.Batch.Summary),
            BuildBatchMetadataJson(productionResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanDraftArtifactProductionArtifactSaveRequest request,
        GeneratorPlanDraftArtifactProductionResult productionResult)
    {
        if (string.IsNullOrWhiteSpace(productionResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactProductionMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = productionResult.GeneratedAtUtc,
            Markdown = productionResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactKind,
            GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactProductionPolicy.ToValidationState(productionResult.Batch.Summary),
            BuildBatchMetadataJson(productionResult));
    }

    private static GeneratedArtifactRecord BuildProducedArtifact(
        GeneratorPlanDraftArtifactProductionArtifactSaveRequest request,
        GeneratorPlanDraftArtifactProductionBatch batch,
        GeneratorPlanProducedDraftArtifact artifact)
    {
        var state = BuildProducedValidationState(batch.Diagnostics, artifact.ArtifactId);

        return new GeneratedArtifactRecord(
            artifact.ArtifactId,
            artifact.ArtifactKind,
            $".llmgc/generated-artifacts/{GeneratorPlanDraftArtifactProductionPolicy.NormalizeSegment(artifact.ArtifactId)}.json",
            artifact.ContentJson,
            request.GeneratedBy.Trim(),
            state,
            JsonSerializer.Serialize(new
            {
                batchId = batch.Id,
                queueItemId = artifact.QueueItemId,
                sourceExecutionStepId = artifact.SourceExecutionStepId,
                expectedArtifactContract = artifact.ExpectedArtifactContract,
                requiresHumanApproval = artifact.RequiresHumanApproval,
                repairRequestId = artifact.RepairRequestId,
                state = artifact.State
            }, JsonOptions));
    }

    private static string BuildProducedValidationState(
        IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics,
        string artifactId)
    {
        var artifactDiagnostics = diagnostics
            .Where(diagnostic => string.Equals(diagnostic.ArtifactId, artifactId, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var errorCount = artifactDiagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        var warningCount = artifactDiagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);

        if (errorCount > 0)
        {
            return GeneratorPlanDraftArtifactProductionValidationState.Invalid;
        }

        return warningCount > 0
            ? GeneratorPlanDraftArtifactProductionValidationState.Warnings
            : GeneratorPlanDraftArtifactProductionValidationState.Valid;
    }

    private static string BuildBatchMetadataJson(GeneratorPlanDraftArtifactProductionResult productionResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = productionResult.GeneratedAtUtc,
            batchId = productionResult.Batch.Id,
            sourceQueueId = productionResult.Batch.SourceQueueId,
            sourceDraftExecutionPlanId = productionResult.Batch.SourceDraftExecutionPlanId,
            sourcePreviewExampleId = productionResult.Batch.SourcePreviewExampleId,
            sourcePath = productionResult.Batch.SourcePath,
            status = productionResult.Batch.Status,
            artifactCount = productionResult.Batch.Summary.ArtifactCount,
            readyForApprovalCount = productionResult.Batch.Summary.ReadyForApprovalCount,
            blockedArtifactCount = productionResult.Batch.Summary.BlockedArtifactCount,
            repairRequestCount = productionResult.Batch.Summary.RepairRequestCount,
            errorCount = productionResult.Batch.Summary.ErrorCount,
            warningCount = productionResult.Batch.Summary.WarningCount,
            producedArtifactIds = productionResult.Batch.Artifacts.Select(artifact => artifact.ArtifactId).ToArray()
        }, JsonOptions);
    }

    private sealed record GeneratorPlanDraftArtifactProductionArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanDraftArtifactQueueResult QueueResult { get; init; } = new();
        public GeneratorPlanDraftArtifactProductionBatch Batch { get; init; } = new();
        public IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactProductionDiagnostic>();
    }

    private sealed record GeneratorPlanDraftArtifactProductionMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
