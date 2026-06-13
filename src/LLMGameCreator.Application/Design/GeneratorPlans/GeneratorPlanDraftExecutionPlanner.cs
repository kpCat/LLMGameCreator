namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionPlanner
{
    public GeneratorPlanDraftExecutionPlan CreateDraftPlan(
        GeneratorPlanPreview preview,
        GeneratorPlanDraftExecutionPlannerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(preview);

        options ??= new GeneratorPlanDraftExecutionPlannerOptions();

        var planId = string.IsNullOrWhiteSpace(options.PlanId)
            ? BuildPlanId(preview)
            : options.PlanId.Trim();
        var artifactPrefix = string.IsNullOrWhiteSpace(options.PlannedArtifactIdPrefix)
            ? "artifact/draft_execution"
            : options.PlannedArtifactIdPrefix.Trim().TrimEnd('/');

        var steps = preview.Steps
            .OrderBy(step => step.Order)
            .ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase)
            .Select((step, index) => BuildStep(planId, artifactPrefix, step, index + 1, options.RequireHumanApprovalByDefault))
            .ToList();

        var diagnostics = preview.Diagnostics
            .Select(diagnostic => MapPreviewDiagnostic(planId, diagnostic))
            .ToList();

        var plan = new GeneratorPlanDraftExecutionPlan
        {
            Id = planId,
            SourcePreviewExampleId = preview.ExampleId,
            SourcePath = preview.SourcePath,
            Title = string.IsNullOrWhiteSpace(preview.Title) ? "Draft Execution Plan" : preview.Title,
            Status = GeneratorPlanDraftExecutionPolicy.BuildStatus(preview, steps),
            Steps = steps,
            Diagnostics = diagnostics
        };

        return plan with
        {
            Summary = GeneratorPlanDraftExecutionPolicy.BuildSummary(plan, diagnostics)
        };
    }

    private static GeneratorPlanDraftExecutionStep BuildStep(
        string planId,
        string artifactPrefix,
        GeneratorPlanPreviewStep previewStep,
        int fallbackOrder,
        bool requiresHumanApproval)
    {
        var order = previewStep.Order > 0 ? previewStep.Order : fallbackOrder;
        var sourceStepId = previewStep.Id.Trim();
        var stepId = $"{planId}/step/{NormalizeSegment(string.IsNullOrWhiteSpace(sourceStepId) ? order.ToString() : sourceStepId)}";
        var expectedArtifactContract = previewStep.ExpectedArtifactContract.Trim();
        var plannedArtifactKind = string.IsNullOrWhiteSpace(expectedArtifactContract) ? "unknown" : expectedArtifactContract;
        var state = string.IsNullOrWhiteSpace(expectedArtifactContract) || previewStep.ValidationGates.Count == 0
            ? GeneratorPlanDraftExecutionStepState.Blocked
            : GeneratorPlanDraftExecutionStepState.Ready;

        return new GeneratorPlanDraftExecutionStep
        {
            Id = stepId,
            Order = order,
            Title = previewStep.Title,
            SourcePreviewStepId = sourceStepId,
            State = state,
            ProducerRole = previewStep.ProducerRole,
            ContextPackTemplate = previewStep.ContextPackTemplate,
            ExpectedArtifactContract = expectedArtifactContract,
            Inputs = previewStep.Inputs,
            ValidationGates = previewStep.ValidationGates,
            PlannedArtifactId = $"{artifactPrefix}/{planId}/step/{order}/{NormalizeSegment(plannedArtifactKind)}",
            PlannedArtifactKind = plannedArtifactKind,
            RepairRequestId = $"repair/draft_execution/{planId}/step/{order}",
            RequiresHumanApproval = requiresHumanApproval
        };
    }

    private static GeneratorPlanDraftExecutionDiagnostic MapPreviewDiagnostic(
        string planId,
        GeneratorPlanPreviewDiagnostic diagnostic)
    {
        return GeneratorPlanDraftExecutionPolicy.Diagnostic(
            diagnostic.Severity,
            GeneratorPlanDraftExecutionDiagnosticCodes.PreviewDiagnostic,
            $"Preview diagnostic {diagnostic.Code}: {diagnostic.Message}",
            planId,
            diagnostic.StepId,
            diagnostic.Path);
    }

    private static string BuildPlanId(GeneratorPlanPreview preview)
    {
        var source = string.IsNullOrWhiteSpace(preview.ExampleId)
            ? Path.GetFileNameWithoutExtension(preview.SourcePath)
            : preview.ExampleId;

        if (string.IsNullOrWhiteSpace(source))
        {
            source = "unknown";
        }

        return $"draft_execution/{NormalizePath(source)}";
    }

    private static string NormalizePath(string value)
    {
        return string.Join(
            "/",
            value.Replace('\\', '/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeSegment));
    }

    private static string NormalizeSegment(string value)
    {
        var chars = value.Trim()
            .Select(character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.'
                ? char.ToLowerInvariant(character)
                : '_')
            .ToArray();
        var normalized = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }
}
