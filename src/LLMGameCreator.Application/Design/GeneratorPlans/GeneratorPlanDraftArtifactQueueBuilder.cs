namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueBuilder
{
    public GeneratorPlanDraftArtifactQueue BuildQueue(
        GeneratorPlanDraftExecutionPlan executionPlan,
        GeneratorPlanDraftArtifactQueueBuilderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(executionPlan);

        options ??= new GeneratorPlanDraftArtifactQueueBuilderOptions();

        var queueId = string.IsNullOrWhiteSpace(options.QueueId)
            ? $"draft_artifact_queue/{executionPlan.Id}"
            : options.QueueId.Trim();
        var items = executionPlan.Steps
            .OrderBy(step => step.Order)
            .ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase)
            .Select(step => BuildItem(queueId, step))
            .ToList();
        var repairRequests = options.CreateRepairRequestsForBlockedItems
            ? items.Where(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked)
                .Select(item => BuildRepairRequest(queueId, item))
                .ToList()
            : [];
        var diagnostics = executionPlan.Diagnostics
            .Select(diagnostic => MapExecutionDiagnostic(queueId, diagnostic))
            .ToList();

        var queue = new GeneratorPlanDraftArtifactQueue
        {
            Id = queueId,
            SourceDraftExecutionPlanId = executionPlan.Id,
            SourcePreviewExampleId = executionPlan.SourcePreviewExampleId,
            SourcePath = executionPlan.SourcePath,
            Status = GeneratorPlanDraftArtifactQueuePolicy.BuildStatus(items, diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error)),
            Items = items,
            RepairRequests = repairRequests,
            Diagnostics = diagnostics
        };

        return queue with
        {
            Summary = GeneratorPlanDraftArtifactQueuePolicy.BuildSummary(queue, diagnostics)
        };
    }

    private static GeneratorPlanDraftArtifactQueueItem BuildItem(string queueId, GeneratorPlanDraftExecutionStep step)
    {
        var order = step.Order > 0 ? step.Order : 0;
        var sourceStepId = step.Id.Trim();
        var itemId = $"{queueId}/item/{GeneratorPlanDraftArtifactQueuePolicy.NormalizeSegment(order > 0 ? order.ToString() : sourceStepId)}";
        var hasProductionContract = !string.IsNullOrWhiteSpace(step.PlannedArtifactId)
            && !string.IsNullOrWhiteSpace(step.PlannedArtifactKind)
            && !string.IsNullOrWhiteSpace(step.ExpectedArtifactContract)
            && step.ValidationGates.Count > 0;
        var state = step.State == GeneratorPlanDraftExecutionStepState.Ready && hasProductionContract
            ? GeneratorPlanDraftArtifactQueueItemState.Ready
            : GeneratorPlanDraftArtifactQueueItemState.Blocked;
        var gateState = state == GeneratorPlanDraftArtifactQueueItemState.Blocked
            ? GeneratorPlanDraftValidationGateState.Blocked
            : GeneratorPlanDraftValidationGateState.Pending;
        var gates = step.ValidationGates
            .Select(gate => new GeneratorPlanDraftValidationGateTicket
            {
                Id = $"{itemId}/gate/{GeneratorPlanDraftArtifactQueuePolicy.NormalizeSegment(gate)}",
                GateId = gate.Trim(),
                State = gateState,
                ArtifactId = step.PlannedArtifactId,
                SourceExecutionStepId = sourceStepId
            })
            .ToList();

        return new GeneratorPlanDraftArtifactQueueItem
        {
            Id = itemId,
            Order = order,
            SourceExecutionStepId = sourceStepId,
            State = state,
            ArtifactId = step.PlannedArtifactId,
            ArtifactKind = step.PlannedArtifactKind,
            ExpectedArtifactContract = step.ExpectedArtifactContract,
            ProducerRole = step.ProducerRole,
            ContextPackTemplate = step.ContextPackTemplate,
            Inputs = step.Inputs,
            ValidationGates = gates,
            RequiresHumanApproval = step.RequiresHumanApproval
        };
    }

    private static GeneratorPlanDraftArtifactRepairRequest BuildRepairRequest(
        string queueId,
        GeneratorPlanDraftArtifactQueueItem item)
    {
        return new GeneratorPlanDraftArtifactRepairRequest
        {
            Id = $"{queueId}/repair/{GeneratorPlanDraftArtifactQueuePolicy.NormalizeSegment(item.SourceExecutionStepId)}",
            SourceExecutionStepId = item.SourceExecutionStepId,
            ArtifactId = item.ArtifactId,
            ReasonCode = "blocked_item",
            Message = $"Queue item {item.Id} is blocked before artifact production.",
            State = GeneratorPlanDraftRepairRequestState.Draft
        };
    }

    private static GeneratorPlanDraftArtifactQueueDiagnostic MapExecutionDiagnostic(
        string queueId,
        GeneratorPlanDraftExecutionDiagnostic diagnostic)
    {
        return GeneratorPlanDraftArtifactQueuePolicy.Diagnostic(
            diagnostic.Severity,
            GeneratorPlanDraftArtifactQueueDiagnosticCodes.ExecutionDiagnostic,
            $"Draft execution diagnostic {diagnostic.Code}: {diagnostic.Message}",
            queueId,
            diagnostic.StepId,
            diagnostic.Target);
    }
}
