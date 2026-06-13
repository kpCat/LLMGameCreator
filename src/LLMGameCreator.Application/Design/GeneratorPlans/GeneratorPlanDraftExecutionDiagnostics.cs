using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanDraftExecutionStatus
{
    public const string Draft = "draft";
    public const string Ready = "ready";
    public const string Blocked = "blocked";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanDraftExecutionStepState
{
    public const string Pending = "pending";
    public const string Blocked = "blocked";
    public const string Ready = "ready";
    public const string Skipped = "skipped";
}

public static class GeneratorPlanDraftExecutionValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanDraftExecutionDiagnosticCodes
{
    public const string MissingPlanId = "generator_plan_draft_execution.missing_plan_id";
    public const string NoSteps = "generator_plan_draft_execution.no_steps";
    public const string DuplicateStepId = "generator_plan_draft_execution.duplicate_step_id";
    public const string DuplicatePlannedArtifactId = "generator_plan_draft_execution.duplicate_planned_artifact_id";
    public const string StepMissingSourcePreviewStepId = "generator_plan_draft_execution.step_missing_source_preview_step_id";
    public const string StepMissingExpectedArtifactContract = "generator_plan_draft_execution.step_missing_expected_artifact_contract";
    public const string StepMissingValidationGates = "generator_plan_draft_execution.step_missing_validation_gates";
    public const string StepMissingProducerRole = "generator_plan_draft_execution.step_missing_producer_role";
    public const string PreviewDiagnostic = "generator_plan_draft_execution.preview_diagnostic";
}

public static class GeneratorPlanDraftExecutionPolicy
{
    public static string ToValidationState(GeneratorPlanDraftExecutionSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftExecutionValidationState.Invalid;
        }

        return summary.WarningCount > 0
            ? GeneratorPlanDraftExecutionValidationState.Warnings
            : GeneratorPlanDraftExecutionValidationState.Valid;
    }

    public static IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> SelectValidationDiagnostics(
        IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.PlanId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.PlanId ?? string.Empty, diagnostic.StepId ?? string.Empty, diagnostic.Target ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target ?? diagnostic.StepId ?? diagnostic.PlanId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    planId = diagnostic.PlanId,
                    stepId = diagnostic.StepId,
                    target = diagnostic.Target
                })))
            .ToList();
    }

    internal static int SeverityOrder(string severity)
    {
        return severity switch
        {
            GeneratorPlanPreviewDiagnosticSeverity.Error => 0,
            GeneratorPlanPreviewDiagnosticSeverity.Warning => 1,
            GeneratorPlanPreviewDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }

    internal static GeneratorPlanDraftExecutionSummary BuildSummary(
        GeneratorPlanDraftExecutionPlan plan,
        IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> diagnostics)
    {
        return new GeneratorPlanDraftExecutionSummary
        {
            StepCount = plan.Steps.Count,
            PendingStepCount = plan.Steps.Count(step => step.State == GeneratorPlanDraftExecutionStepState.Pending),
            BlockedStepCount = plan.Steps.Count(step => step.State == GeneratorPlanDraftExecutionStepState.Blocked),
            PlannedArtifactCount = plan.Steps.Count(step => !string.IsNullOrWhiteSpace(step.PlannedArtifactId)),
            RepairRequestCount = plan.Steps.Count(step => !string.IsNullOrWhiteSpace(step.RepairRequestId)),
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    internal static string BuildStatus(GeneratorPlanPreview preview, IReadOnlyList<GeneratorPlanDraftExecutionStep> steps)
    {
        if (preview.Summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftExecutionStatus.Invalid;
        }

        if (steps.Any(step => step.State == GeneratorPlanDraftExecutionStepState.Blocked))
        {
            return GeneratorPlanDraftExecutionStatus.Blocked;
        }

        return steps.Count > 0
            ? GeneratorPlanDraftExecutionStatus.Ready
            : GeneratorPlanDraftExecutionStatus.Draft;
    }

    internal static GeneratorPlanDraftExecutionDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? planId = null,
        string? stepId = null,
        string? target = null)
    {
        return new GeneratorPlanDraftExecutionDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            PlanId = planId,
            StepId = stepId,
            Target = target
        };
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
