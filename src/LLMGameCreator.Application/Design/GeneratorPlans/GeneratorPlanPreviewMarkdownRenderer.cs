using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewMarkdownRenderer
{
    public string Render(GeneratorPlanPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        var builder = new StringBuilder();
        builder.AppendLine("# Generator Plan Preview");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Status(preview.Summary)}**");
        builder.AppendLine($"- Example ID: {Cell(preview.ExampleId)}");
        builder.AppendLine($"- Title: {Cell(preview.Title)}");
        builder.AppendLine($"- Source profile: {Cell(preview.SourceProfileId)}");
        builder.AppendLine($"- Feature bundles: {preview.SelectedFeatureBundles.Count}");
        builder.AppendLine($"- Target artifacts: {preview.TargetArtifacts.Count}");
        builder.AppendLine();

        AppendSummary(builder, preview.Summary);
        AppendSteps(builder, preview.Steps);
        AppendDiagnostics(builder, preview.Diagnostics);
        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, GeneratorPlanPreviewSummary summary)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| Steps | {summary.StepCount} |");
        builder.AppendLine($"| Target artifacts | {summary.TargetArtifactCount} |");
        builder.AppendLine($"| Feature bundles | {summary.FeatureBundleCount} |");
        builder.AppendLine($"| Errors | {summary.ErrorCount} |");
        builder.AppendLine($"| Warnings | {summary.WarningCount} |");
        builder.AppendLine();
    }

    private static void AppendSteps(StringBuilder builder, IReadOnlyList<GeneratorPlanPreviewStep> steps)
    {
        builder.AppendLine("## Steps");
        builder.AppendLine();

        if (steps.Count == 0)
        {
            builder.AppendLine("_No generator plan steps were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Order | Step ID | Title | Producer | Expected artifact | Gates |");
        builder.AppendLine("|---:|---|---|---|---|---:|");
        foreach (var step in steps.OrderBy(step => step.Order).ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {step.Order} | {Cell(step.Id)} | {Cell(step.Title)} | {Cell(step.ProducerRole)} | {Cell(step.ExpectedArtifactContract)} | {step.ValidationGates.Count} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanPreviewDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Path | Step | Message |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanPreviewValidationPolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Path, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.Path)} | {Cell(diagnostic.StepId)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static string Status(GeneratorPlanPreviewSummary summary)
    {
        if (summary.ErrorCount > 0)
        {
            return "FAILED";
        }

        return summary.WarningCount > 0 ? "WARNINGS" : "OK";
    }

    private static string Cell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal)
            .Trim();
    }
}
