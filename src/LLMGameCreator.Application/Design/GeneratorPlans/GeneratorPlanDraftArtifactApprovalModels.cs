namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactApprovalRequest
{
    public string? SnapshotId { get; init; }
    public bool RenderMarkdown { get; init; } = true;
    public bool AutoApproveValidArtifacts { get; init; }
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDecision> Decisions { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDecision>();
}

public sealed record GeneratorPlanDraftArtifactApprovalResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanDraftArtifactProductionResult ProductionResult { get; init; } = new();
    public GeneratorPlanDraftArtifactStagingSnapshot Snapshot { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDiagnostic>();
}

public sealed record GeneratorPlanDraftArtifactApprovalDecision
{
    public string ArtifactId { get; init; } = string.Empty;
    public string Decision { get; init; } = GeneratorPlanDraftArtifactApprovalDecisionKind.Pending;
    public string ReasonCode { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTimeOffset DecidedAtUtc { get; init; }
}

public sealed record GeneratorPlanDraftArtifactApprovalItem
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanDraftArtifactApprovalItemState.Pending;
    public string SourceProductionBatchId { get; init; } = string.Empty;
    public string QueueItemId { get; init; } = string.Empty;
    public string SourceExecutionStepId { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
    public bool RequiresHumanApproval { get; init; }
    public string RepairRequestId { get; init; } = string.Empty;
    public string DecisionReasonCode { get; init; } = string.Empty;
    public string DecisionComment { get; init; } = string.Empty;
    public DateTimeOffset DecidedAtUtc { get; init; }
    public IReadOnlyList<string> ValidationIssues { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanDraftArtifactStagingSnapshot
{
    public string Id { get; init; } = string.Empty;
    public string SourceProductionBatchId { get; init; } = string.Empty;
    public string SourcePreviewExampleId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Status { get; init; } = GeneratorPlanDraftArtifactStagingStatus.Draft;
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> Items { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalItem>();
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDiagnostic>();
    public GeneratorPlanDraftArtifactStagingSummary Summary { get; init; } = new();
}

public sealed record GeneratorPlanDraftArtifactStagingSummary
{
    public int ItemCount { get; init; }
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public int RepairRequestedCount { get; init; }
    public int BlockedCount { get; init; }
    public int ReadyForPackageCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanDraftArtifactApprovalDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? SnapshotId { get; init; }
    public string? ArtifactId { get; init; }
    public string? Target { get; init; }
}

public static class GeneratorPlanDraftArtifactApprovalDecisionKind
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string RepairRequested = "repair_requested";
}

public static class GeneratorPlanDraftArtifactApprovalItemState
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string RepairRequested = "repair_requested";
    public const string Blocked = "blocked";
}

public static class GeneratorPlanDraftArtifactStagingStatus
{
    public const string Draft = "draft";
    public const string ReadyForPackage = "ready_for_package";
    public const string NeedsReview = "needs_review";
    public const string NeedsRepair = "needs_repair";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanDraftArtifactApprovalValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";
}
