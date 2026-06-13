namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewValidator
{
    public GeneratorPlanPreview Validate(GeneratorPlanPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        var diagnostics = new List<GeneratorPlanPreviewDiagnostic>(preview.Diagnostics);

        if (string.IsNullOrWhiteSpace(preview.ExampleId))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.MissingExampleId, "Example id is required.", preview.SourcePath);
        }

        if (string.IsNullOrWhiteSpace(preview.Title))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.MissingTitle, "Title is required.", preview.SourcePath);
        }

        if (string.IsNullOrWhiteSpace(preview.SourceProfileId))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.MissingSourceProfile, "Source profile id is required.", preview.SourcePath);
        }

        if (preview.TargetArtifacts.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanPreviewDiagnosticCodes.TargetArtifactsEmpty, "Target artifacts should not be empty.", preview.SourcePath);
        }

        if (preview.SelectedFeatureBundles.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanPreviewDiagnosticCodes.SelectedFeatureBundlesEmpty, "Selected feature bundles should not be empty.", preview.SourcePath);
        }

        if (preview.Steps.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.NoSteps, "Plan must contain at least one step.", preview.SourcePath);
        }

        var stepIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stepOrders = new HashSet<int>();

        foreach (var step in preview.Steps)
        {
            var stepTarget = string.IsNullOrWhiteSpace(step.Id) ? $"order/{step.Order}" : step.Id;

            if (string.IsNullOrWhiteSpace(step.Id))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.StepMissingId, "Step id is required.", preview.SourcePath, stepTarget);
            }
            else if (!stepIds.Add(step.Id))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanPreviewDiagnosticCodes.StepDuplicateId, $"Duplicate step id: {step.Id}", preview.SourcePath, step.Id);
            }

            if (!stepOrders.Add(step.Order))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanPreviewDiagnosticCodes.StepOrderDuplicate, $"Duplicate step order: {step.Order}", preview.SourcePath, stepTarget);
            }

            if (string.IsNullOrWhiteSpace(step.ExpectedArtifactContract))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanPreviewDiagnosticCodes.StepMissingExpectedArtifactContract, "Step expected artifact contract is required before execution.", preview.SourcePath, stepTarget);
            }

            if (step.ValidationGates.Count == 0)
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanPreviewDiagnosticCodes.StepMissingValidationGates, "Step validation gates are required before execution.", preview.SourcePath, stepTarget);
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(diagnostic => GeneratorPlanPreviewValidationPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return preview with
        {
            Diagnostics = orderedDiagnostics,
            Summary = GeneratorPlanPreviewLoader.BuildSummary(preview, orderedDiagnostics)
        };
    }

    private static void Add(
        ICollection<GeneratorPlanPreviewDiagnostic> diagnostics,
        string severity,
        string code,
        string message,
        string? path = null,
        string? stepId = null)
    {
        diagnostics.Add(GeneratorPlanPreviewLoader.CreateDiagnostic(severity, code, message, path, stepId));
    }
}
