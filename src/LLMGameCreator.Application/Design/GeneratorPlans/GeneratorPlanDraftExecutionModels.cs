namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftExecutionRequest
{
    public string? PlanId { get; init; }
    public bool RenderMarkdown { get; init; } = true;
    public bool RequireHumanApprovalByDefault { get; init; } = true;
}

public sealed record GeneratorPlanDraftExecutionResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanPreviewResult PreviewResult { get; init; } = new();
    public GeneratorPlanDraftExecutionPlan Plan { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftExecutionDiagnostic>();
}

public sealed record GeneratorPlanDraftExecutionPlan
{
    public string Id { get; init; } = string.Empty;
    public string SourcePreviewExampleId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = GeneratorPlanDraftExecutionStatus.Draft;
    public IReadOnlyList<GeneratorPlanDraftExecutionStep> Steps { get; init; } = Array.Empty<GeneratorPlanDraftExecutionStep>();
    public IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftExecutionDiagnostic>();
    public GeneratorPlanDraftExecutionSummary Summary { get; init; } = new();
}

public sealed record GeneratorPlanDraftExecutionStep
{
    public string Id { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string SourcePreviewStepId { get; init; } = string.Empty;
    public string State { get; init; } = GeneratorPlanDraftExecutionStepState.Pending;
    public string ProducerRole { get; init; } = string.Empty;
    public string ContextPackTemplate { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public IReadOnlyList<string> Inputs { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ValidationGates { get; init; } = Array.Empty<string>();
    public string PlannedArtifactId { get; init; } = string.Empty;
    public string PlannedArtifactKind { get; init; } = string.Empty;
    public string RepairRequestId { get; init; } = string.Empty;
    public bool RequiresHumanApproval { get; init; }
}

public sealed record GeneratorPlanDraftExecutionSummary
{
    public int StepCount { get; init; }
    public int PendingStepCount { get; init; }
    public int BlockedStepCount { get; init; }
    public int PlannedArtifactCount { get; init; }
    public int RepairRequestCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanDraftExecutionDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? PlanId { get; init; }
    public string? StepId { get; init; }
    public string? Target { get; init; }
}

public sealed record GeneratorPlanDraftExecutionPlannerOptions
{
    public string? PlanId { get; init; }
    public string PlannedArtifactIdPrefix { get; init; } = "artifact/draft_execution";
    public bool RequireHumanApprovalByDefault { get; init; } = true;
}
