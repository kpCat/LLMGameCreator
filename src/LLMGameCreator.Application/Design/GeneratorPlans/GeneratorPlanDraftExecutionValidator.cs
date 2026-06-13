namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionValidator
{
    public GeneratorPlanDraftExecutionPlan Validate(GeneratorPlanDraftExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var diagnostics = new List<GeneratorPlanDraftExecutionDiagnostic>(plan.Diagnostics);

        if (string.IsNullOrWhiteSpace(plan.Id))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftExecutionDiagnosticCodes.MissingPlanId, "Draft execution plan id is required.", plan.Id);
        }

        if (plan.Steps.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftExecutionDiagnosticCodes.NoSteps, "Draft execution plan must contain at least one step.", plan.Id);
        }

        var stepIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var artifactIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var step in plan.Steps)
        {
            if (!string.IsNullOrWhiteSpace(step.Id) && !stepIds.Add(step.Id))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftExecutionDiagnosticCodes.DuplicateStepId, $"Duplicate draft execution step id: {step.Id}", plan.Id, step.Id, step.Id);
            }

            if (!string.IsNullOrWhiteSpace(step.PlannedArtifactId) && !artifactIds.Add(step.PlannedArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftExecutionDiagnosticCodes.DuplicatePlannedArtifactId, $"Duplicate planned artifact id: {step.PlannedArtifactId}", plan.Id, step.Id, step.PlannedArtifactId);
            }

            if (string.IsNullOrWhiteSpace(step.SourcePreviewStepId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingSourcePreviewStepId, "Step should reference a source preview step id.", plan.Id, step.Id, step.Id);
            }

            if (string.IsNullOrWhiteSpace(step.ExpectedArtifactContract))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingExpectedArtifactContract, "Step expected artifact contract is required before execution.", plan.Id, step.Id, step.Id);
            }

            if (step.ValidationGates.Count == 0)
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingValidationGates, "Step validation gates are required before execution.", plan.Id, step.Id, step.Id);
            }

            if (string.IsNullOrWhiteSpace(step.ProducerRole))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingProducerRole, "Step producer role should be set before execution.", plan.Id, step.Id, step.Id);
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(diagnostic => GeneratorPlanDraftExecutionPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.PlanId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var validated = plan with { Diagnostics = orderedDiagnostics };
        return validated with
        {
            Summary = GeneratorPlanDraftExecutionPolicy.BuildSummary(validated, orderedDiagnostics)
        };
    }

    private static void Add(
        ICollection<GeneratorPlanDraftExecutionDiagnostic> diagnostics,
        string severity,
        string code,
        string message,
        string? planId = null,
        string? stepId = null,
        string? target = null)
    {
        diagnostics.Add(GeneratorPlanDraftExecutionPolicy.Diagnostic(severity, code, message, planId, stepId, target));
    }
}
