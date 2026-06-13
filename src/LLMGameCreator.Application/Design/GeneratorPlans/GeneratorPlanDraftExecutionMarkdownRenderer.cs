using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionMarkdownRenderer
{
    public string Render(GeneratorPlanDraftExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var builder = new StringBuilder();
        builder.AppendLine("# Generator Plan Draft Execution");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(plan.Status)}**");
        builder.AppendLine($"- Plan ID: {Cell(plan.Id)}");
        builder.AppendLine($"- Source example: {Cell(plan.SourcePreviewExampleId)}");
        builder.AppendLine($"- Source path: {Cell(plan.SourcePath)}");
        builder.AppendLine($"- Step count: {plan.Summary.StepCount}");
        builder.AppendLine($"- Planned artifacts: {plan.Summary.PlannedArtifactCount}");
        builder.AppendLine($"- Repair requests: {plan.Summary.RepairRequestCount}");
        builder.AppendLine();

        AppendSteps(builder, plan.Steps);
        AppendDiagnostics(builder, plan.Diagnostics);
        return builder.ToString();
    }

    private static void AppendSteps(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftExecutionStep> steps)
    {
        builder.AppendLine("## Steps");
        builder.AppendLine();

        if (steps.Count == 0)
        {
            builder.AppendLine("_No draft execution steps were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Order | State | Step ID | Preview Step | Producer | Expected Artifact | Planned Artifact | Gates | Approval |");
        builder.AppendLine("|---:|---|---|---|---|---|---|---:|---|");
        foreach (var step in steps.OrderBy(step => step.Order).ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase))
        {
            var approval = step.RequiresHumanApproval ? "required" : "not required";
            builder.AppendLine($"| {step.Order} | {Cell(step.State)} | {Cell(step.Id)} | {Cell(step.SourcePreviewStepId)} | {Cell(step.ProducerRole)} | {Cell(step.ExpectedArtifactContract)} | {Cell(step.PlannedArtifactId)} | {step.ValidationGates.Count} | {approval} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftExecutionDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Step | Target | Message |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanDraftExecutionPolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.StepId)} | {Cell(diagnostic.Target)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
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
