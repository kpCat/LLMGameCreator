using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactProductionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly GeneratorPlanDraftArtifactQueueService _queueService;
    private readonly IGeneratorPlanDraftArtifactProducer _producer;
    private readonly GeneratorPlanDraftArtifactProductionValidator _validator;
    private readonly GeneratorPlanDraftArtifactProductionMarkdownRenderer _markdownRenderer;

    public GeneratorPlanDraftArtifactProductionService()
        : this(
            new GeneratorPlanDraftArtifactQueueService(),
            new DeterministicGeneratorPlanDraftArtifactProducer(),
            new GeneratorPlanDraftArtifactProductionValidator(),
            new GeneratorPlanDraftArtifactProductionMarkdownRenderer())
    {
    }

    public GeneratorPlanDraftArtifactProductionService(
        GeneratorPlanDraftArtifactQueueService queueService,
        IGeneratorPlanDraftArtifactProducer producer,
        GeneratorPlanDraftArtifactProductionValidator validator,
        GeneratorPlanDraftArtifactProductionMarkdownRenderer markdownRenderer)
    {
        _queueService = queueService;
        _producer = producer;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<GeneratorPlanDraftArtifactProductionResult> ProduceAsync(
        GeneratorPlanDraftArtifactQueueResult queueResult,
        GeneratorPlanDraftArtifactProductionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queueResult);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var batchId = string.IsNullOrWhiteSpace(request.BatchId)
            ? $"draft_artifact_production/{queueResult.Queue.Id}"
            : request.BatchId.Trim();
        var diagnostics = MapQueueDiagnostics(batchId, queueResult).ToList();
        var produced = new List<GeneratorPlanProducedDraftArtifact>();

        if (!queueResult.Ok || queueResult.Status == GeneratorPlanDraftArtifactQueueStatus.Invalid)
        {
            diagnostics.Add(GeneratorPlanDraftArtifactProductionPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanDraftArtifactProductionDiagnosticCodes.QueueInvalid,
                "Draft artifact queue must be valid before artifact production.",
                batchId,
                target: queueResult.Queue.Id));
        }
        else
        {
            foreach (var item in queueResult.Queue.Items.OrderBy(item => item.Order).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var repairRequest = queueResult.Queue.RepairRequests.FirstOrDefault(candidate =>
                    string.Equals(candidate.SourceExecutionStepId, item.SourceExecutionStepId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.ArtifactId, item.ArtifactId, StringComparison.OrdinalIgnoreCase));

                if (item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked && !request.ProduceBlockedItems)
                {
                    produced.Add(BuildBlockedArtifact(batchId, item, repairRequest?.Id ?? string.Empty, request));
                    continue;
                }

                var artifact = await _producer.ProduceAsync(item, request with { BatchId = batchId }, cancellationToken).ConfigureAwait(false);
                if (item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked)
                {
                    artifact = artifact with
                    {
                        State = GeneratorPlanProducedDraftArtifactState.Blocked,
                        RepairRequestId = repairRequest?.Id ?? artifact.RepairRequestId
                    };
                }
                else
                {
                    artifact = artifact with { State = GeneratorPlanProducedDraftArtifactState.ReadyForApproval };
                }

                produced.Add(artifact);
            }
        }

        var batch = new GeneratorPlanDraftArtifactProductionBatch
        {
            Id = batchId,
            SourceQueueId = queueResult.Queue.Id,
            SourceDraftExecutionPlanId = queueResult.Queue.SourceDraftExecutionPlanId,
            SourcePreviewExampleId = queueResult.Queue.SourcePreviewExampleId,
            SourcePath = queueResult.Queue.SourcePath,
            Artifacts = produced,
            Diagnostics = diagnostics
        };

        batch = _validator.Validate(batch);
        var status = batch.Status;
        var markdown = request.RenderMarkdown ? _markdownRenderer.Render(batch) : string.Empty;

        return new GeneratorPlanDraftArtifactProductionResult
        {
            Ok = status != GeneratorPlanDraftArtifactProductionStatus.Invalid,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            QueueResult = queueResult,
            Batch = batch,
            MarkdownReport = markdown,
            Diagnostics = batch.Diagnostics
        };
    }

    public async Task<GeneratorPlanDraftArtifactProductionResult> ProduceFromExampleAsync(
        string examplePath,
        GeneratorPlanDraftArtifactProductionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(examplePath))
        {
            throw new ArgumentException("Example path is required.", nameof(examplePath));
        }

        ArgumentNullException.ThrowIfNull(request);

        var queueResult = await _queueService
            .CreateQueueFromExampleAsync(examplePath, new GeneratorPlanDraftArtifactQueueRequest { RenderMarkdown = false }, cancellationToken)
            .ConfigureAwait(false);

        return await ProduceAsync(queueResult, request, cancellationToken).ConfigureAwait(false);
    }

    private static IEnumerable<GeneratorPlanDraftArtifactProductionDiagnostic> MapQueueDiagnostics(
        string batchId,
        GeneratorPlanDraftArtifactQueueResult queueResult)
    {
        return queueResult.Diagnostics.Select(diagnostic => GeneratorPlanDraftArtifactProductionPolicy.Diagnostic(
            diagnostic.Severity,
            GeneratorPlanDraftArtifactProductionDiagnosticCodes.QueueDiagnostic,
            $"Draft artifact queue diagnostic {diagnostic.Code}: {diagnostic.Message}",
            batchId,
            diagnostic.ArtifactId,
            diagnostic.ItemId,
            diagnostic.GateId ?? diagnostic.QueueId));
    }

    private static GeneratorPlanProducedDraftArtifact BuildBlockedArtifact(
        string batchId,
        GeneratorPlanDraftArtifactQueueItem item,
        string repairRequestId,
        GeneratorPlanDraftArtifactProductionRequest request)
    {
        var content = new JsonObject
        {
            ["schema_version"] = "0.1",
            ["artifact_id"] = item.ArtifactId,
            ["artifact_kind"] = item.ArtifactKind,
            ["expected_artifact_contract"] = item.ExpectedArtifactContract,
            ["title"] = "Blocked draft artifact",
            ["purpose"] = "Production was blocked by queue validation or missing source planning data.",
            ["source"] = new JsonObject
            {
                ["queue_item_id"] = item.Id,
                ["execution_step_id"] = item.SourceExecutionStepId
            },
            ["draft"] = true,
            ["blocked"] = true,
            ["repair_request_id"] = repairRequestId,
            ["reason"] = "blocked_queue_item"
        };

        return new GeneratorPlanProducedDraftArtifact
        {
            Id = $"{batchId}/produced/{GeneratorPlanDraftArtifactProductionPolicy.NormalizeSegment(string.IsNullOrWhiteSpace(item.ArtifactId) ? item.Id : item.ArtifactId)}",
            QueueItemId = item.Id,
            SourceExecutionStepId = item.SourceExecutionStepId,
            ArtifactId = item.ArtifactId,
            ArtifactKind = item.ArtifactKind,
            ExpectedArtifactContract = item.ExpectedArtifactContract,
            State = GeneratorPlanProducedDraftArtifactState.Blocked,
            ContentJson = content.ToJsonString(JsonOptions),
            ValidationGates = item.ValidationGates.Select(gate => gate.GateId).ToList(),
            RepairRequestId = repairRequestId,
            RequiresHumanApproval = request.RequireHumanApprovalByDefault || item.RequiresHumanApproval
        };
    }
}
