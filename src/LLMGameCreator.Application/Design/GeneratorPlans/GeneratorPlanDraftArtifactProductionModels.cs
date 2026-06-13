namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactProductionRequest
{
    public string? BatchId { get; init; }
    public bool RenderMarkdown { get; init; } = true;
    public bool ProduceBlockedItems { get; init; }
    public bool RequireHumanApprovalByDefault { get; init; } = true;
}

public sealed record GeneratorPlanDraftArtifactProductionResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanDraftArtifactQueueResult QueueResult { get; init; } = new();
    public GeneratorPlanDraftArtifactProductionBatch Batch { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactProductionDiagnostic>();
}

public sealed record GeneratorPlanDraftArtifactProductionBatch
{
    public string Id { get; init; } = string.Empty;
    public string SourceQueueId { get; init; } = string.Empty;
    public string SourceDraftExecutionPlanId { get; init; } = string.Empty;
    public string SourcePreviewExampleId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Status { get; init; } = GeneratorPlanDraftArtifactProductionStatus.Draft;
    public IReadOnlyList<GeneratorPlanProducedDraftArtifact> Artifacts { get; init; } = Array.Empty<GeneratorPlanProducedDraftArtifact>();
    public IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactProductionDiagnostic>();
    public GeneratorPlanDraftArtifactProductionSummary Summary { get; init; } = new();
}

public sealed record GeneratorPlanProducedDraftArtifact
{
    public string Id { get; init; } = string.Empty;
    public string QueueItemId { get; init; } = string.Empty;
    public string SourceExecutionStepId { get; init; } = string.Empty;
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanProducedDraftArtifactState.Draft;
    public string ContentJson { get; init; } = "{}";
    public IReadOnlyList<string> ValidationGates { get; init; } = Array.Empty<string>();
    public string RepairRequestId { get; init; } = string.Empty;
    public bool RequiresHumanApproval { get; init; }
}

public sealed record GeneratorPlanDraftArtifactProductionSummary
{
    public int ArtifactCount { get; init; }
    public int DraftArtifactCount { get; init; }
    public int BlockedArtifactCount { get; init; }
    public int ReadyForApprovalCount { get; init; }
    public int RepairRequestCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanDraftArtifactProductionDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? BatchId { get; init; }
    public string? ArtifactId { get; init; }
    public string? QueueItemId { get; init; }
    public string? Target { get; init; }
}

public static class GeneratorPlanDraftArtifactProductionStatus
{
    public const string Draft = "draft";
    public const string ReadyForApproval = "ready_for_approval";
    public const string Blocked = "blocked";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanProducedDraftArtifactState
{
    public const string Draft = "draft";
    public const string ReadyForApproval = "ready_for_approval";
    public const string Blocked = "blocked";
}

public static class GeneratorPlanDraftArtifactProductionValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";
}
