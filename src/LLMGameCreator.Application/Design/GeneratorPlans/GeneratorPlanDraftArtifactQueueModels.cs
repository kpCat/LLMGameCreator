namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactQueueRequest
{
    public string? QueueId { get; init; }
    public bool RenderMarkdown { get; init; } = true;
    public bool CreateRepairRequestsForBlockedItems { get; init; } = true;
    public GeneratorPlanDraftExecutionRequest DraftExecutionRequest { get; init; } = new();
}

public sealed record GeneratorPlanDraftArtifactQueueResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanDraftExecutionResult DraftExecutionResult { get; init; } = new();
    public GeneratorPlanDraftArtifactQueue Queue { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactQueueDiagnostic>();
}

public sealed record GeneratorPlanDraftArtifactQueue
{
    public string Id { get; init; } = string.Empty;
    public string SourceDraftExecutionPlanId { get; init; } = string.Empty;
    public string SourcePreviewExampleId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Status { get; init; } = GeneratorPlanDraftArtifactQueueStatus.Draft;
    public IReadOnlyList<GeneratorPlanDraftArtifactQueueItem> Items { get; init; } = Array.Empty<GeneratorPlanDraftArtifactQueueItem>();
    public IReadOnlyList<GeneratorPlanDraftArtifactRepairRequest> RepairRequests { get; init; } = Array.Empty<GeneratorPlanDraftArtifactRepairRequest>();
    public IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactQueueDiagnostic>();
    public GeneratorPlanDraftArtifactQueueSummary Summary { get; init; } = new();
}

public sealed record GeneratorPlanDraftArtifactQueueItem
{
    public string Id { get; init; } = string.Empty;
    public int Order { get; init; }
    public string SourceExecutionStepId { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanDraftArtifactQueueItemState.Pending;
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string ProducerRole { get; init; } = string.Empty;
    public string ContextPackTemplate { get; init; } = string.Empty;
    public IReadOnlyList<string> Inputs { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratorPlanDraftValidationGateTicket> ValidationGates { get; init; } = Array.Empty<GeneratorPlanDraftValidationGateTicket>();
    public bool RequiresHumanApproval { get; init; }
}

public sealed record GeneratorPlanDraftValidationGateTicket
{
    public string Id { get; init; } = string.Empty;
    public string GateId { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanDraftValidationGateState.Pending;
    public string ArtifactId { get; init; } = string.Empty;
    public string SourceExecutionStepId { get; init; } = string.Empty;
}

public sealed record GeneratorPlanDraftArtifactRepairRequest
{
    public string Id { get; init; } = string.Empty;
    public string SourceExecutionStepId { get; init; } = string.Empty;
    public string ArtifactId { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanDraftRepairRequestState.Draft;
}

public sealed record GeneratorPlanDraftArtifactQueueSummary
{
    public int ItemCount { get; init; }
    public int PendingItemCount { get; init; }
    public int BlockedItemCount { get; init; }
    public int ReadyItemCount { get; init; }
    public int ValidationGateCount { get; init; }
    public int RepairRequestCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanDraftArtifactQueueDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? QueueId { get; init; }
    public string? ItemId { get; init; }
    public string? ArtifactId { get; init; }
    public string? GateId { get; init; }
}

public sealed record GeneratorPlanDraftArtifactQueueBuilderOptions
{
    public string? QueueId { get; init; }
    public bool CreateRepairRequestsForBlockedItems { get; init; } = true;
}
