using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmEvaluationMarkdownRenderer
{
    public string Render(GeneratorPlanStrictLlmEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("# Strict LLM Generation Evaluation");
        builder.AppendLine();
        builder.AppendLine($"- Evaluation id: {Cell(result.EvaluationId)}");
        builder.AppendLine($"- Evaluated at: {Cell(result.EvaluatedAtUtc.ToString("O"))}");
        builder.AppendLine($"- Source capability selection id: {Cell(result.SourceCapabilitySelectionId)}");
        builder.AppendLine($"- Mode: {Cell(result.Mode)}");
        builder.AppendLine($"- Requested contracts: {Cell(string.Join(", ", result.RequestedContractIds))}");
        builder.AppendLine($"- Iterations: {result.IterationsPerContract}");
        builder.AppendLine($"- Repair enabled: {result.RepairEnabled}");
        builder.AppendLine($"- Stage for review: {result.StageValidArtifactsForReview}");
        builder.AppendLine($"- Expected max LLM calls: {result.ExpectedMaxLlmCalls}");
        builder.AppendLine();

        AppendSummary(builder, result.Summary);
        AppendContracts(builder, result.ContractSummaries);
        AppendDiagnostics(builder, result.DiagnosticSummaries);
        AppendQualityWarnings(builder, result.Diagnostics);
        AppendSamples(builder, result.Samples);
        AppendRecommendations(builder, result);

        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, GeneratorPlanStrictLlmEvaluationSummary summary)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| total_contracts_requested | {summary.TotalContractsRequested} |");
        builder.AppendLine($"| total_generation_runs | {summary.TotalGenerationRuns} |");
        builder.AppendLine($"| total_attempts | {summary.TotalAttempts} |");
        builder.AppendLine($"| initial_pass_count | {summary.InitialPassCount} |");
        builder.AppendLine($"| repair_pass_count | {summary.RepairPassCount} |");
        builder.AppendLine($"| failed_count | {summary.FailedCount} |");
        builder.AppendLine($"| valid_artifact_count | {summary.ValidArtifactCount} |");
        builder.AppendLine($"| staged_for_review_count | {summary.StagedForReviewCount} |");
        builder.AppendLine($"| markdown_fence_error_count | {summary.MarkdownFenceErrorCount} |");
        builder.AppendLine($"| json_wrapper_error_count | {summary.JsonWrapperErrorCount} |");
        builder.AppendLine($"| json_invalid_count | {summary.JsonInvalidCount} |");
        builder.AppendLine($"| wrong_artifact_kind_count | {summary.WrongArtifactKindCount} |");
        builder.AppendLine($"| forbidden_field_count | {summary.ForbiddenFieldCount} |");
        builder.AppendLine($"| invalid_id_count | {summary.InvalidIdCount} |");
        builder.AppendLine($"| missing_field_count | {summary.MissingFieldCount} |");
        builder.AppendLine($"| expected_max_llm_calls | {summary.ExpectedMaxLlmCalls} |");
        builder.AppendLine($"| overall_pass_rate | {summary.OverallPassRate:P1} |");
        builder.AppendLine($"| repair_recovery_rate | {summary.RepairRecoveryRate:P1} |");
        builder.AppendLine();
    }

    private static void AppendContracts(StringBuilder builder, IReadOnlyList<GeneratorPlanStrictLlmEvaluationContractSummary> contracts)
    {
        builder.AppendLine("## Per-contract summary");
        builder.AppendLine();
        if (contracts.Count == 0)
        {
            builder.AppendLine("_No contract summaries._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Contract | Runs | Initial pass | Repair pass | Failed | Valid artifacts | Average attempts | Top diagnostics |");
        builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
        foreach (var contract in contracts)
        {
            builder.AppendLine($"| {Cell(contract.ContractId)} | {contract.Runs} | {contract.InitialPass} | {contract.RepairPass} | {contract.Failed} | {contract.ValidArtifacts} | {contract.AverageAttempts:0.##} | {Cell(string.Join(", ", contract.TopDiagnosticCodes))} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanStrictLlmEvaluationDiagnosticSummary> diagnostics)
    {
        builder.AppendLine("## Diagnostic hot spots");
        builder.AppendLine();
        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Contract | Target | Count | Example |");
        builder.AppendLine("|---|---|---|---|---:|---|");
        foreach (var diagnostic in diagnostics)
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.ContractId)} | {Cell(diagnostic.Target)} | {diagnostic.Count} | {Cell(diagnostic.ExampleMessage)} |");
        }

        builder.AppendLine();
    }

    private static void AppendQualityWarnings(StringBuilder builder, IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        builder.AppendLine("## Quality warnings");
        builder.AppendLine();
        var warnings = diagnostics
            .Where(diagnostic => diagnostic.Code.StartsWith("strict_llm_evaluation.", StringComparison.Ordinal)
                && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .ToList();

        if (warnings.Count == 0)
        {
            builder.AppendLine("_No quality warnings were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Code | Contract | Target | Message |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var warning in warnings)
        {
            builder.AppendLine($"| {Cell(warning.Code)} | {Cell(warning.ContractId)} | {Cell(warning.Target)} | {Cell(warning.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendSamples(StringBuilder builder, IReadOnlyList<GeneratorPlanStrictLlmEvaluationSample> samples)
    {
        builder.AppendLine("## Samples");
        builder.AppendLine();
        if (samples.Count == 0)
        {
            builder.AppendLine("_No samples were captured._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Contract | Artifact | Valid | Repaired | Content excerpt | Diagnostic excerpt |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var sample in samples)
        {
            builder.AppendLine($"| {Cell(sample.ContractId)} | {Cell(sample.ArtifactId)} | {sample.Valid} | {sample.Repaired} | {Cell(sample.ContentExcerpt)} | {Cell(sample.DiagnosticExcerpt)} |");
        }

        builder.AppendLine();
    }

    private static void AppendRecommendations(StringBuilder builder, GeneratorPlanStrictLlmEvaluationResult result)
    {
        builder.AppendLine("## Recommendations");
        builder.AppendLine();
        foreach (var recommendation in BuildRecommendations(result))
        {
            builder.AppendLine("- " + recommendation);
        }
    }

    private static IReadOnlyList<string> BuildRecommendations(GeneratorPlanStrictLlmEvaluationResult result)
    {
        var recommendations = new List<string>();
        var summary = result.Summary;
        var jsonFailures = summary.MarkdownFenceErrorCount + summary.JsonWrapperErrorCount + summary.JsonInvalidCount;
        if (summary.TotalGenerationRuns == 0)
        {
            recommendations.Add("Run a small explicit batch before expanding contracts.");
            return recommendations;
        }

        if ((double)jsonFailures / Math.Max(1, summary.TotalAttempts) >= 0.25)
        {
            recommendations.Add("Tighten the strict JSON prompt and output rules.");
        }

        if (summary.MissingFieldCount + summary.InvalidIdCount + summary.WrongArtifactKindCount > 0)
        {
            recommendations.Add("Add or refine a repair rule for repeated contract failures.");
        }

        if (summary.ForbiddenFieldCount > 0)
        {
            recommendations.Add("Expand validator and prompt language around forbidden code or script fields.");
        }

        if (summary.OverallPassRate >= 0.9 && summary.FailedCount == 0)
        {
            recommendations.Add("Contract looks stable for the sampled profile and contracts.");
        }

        if (summary.OverallPassRate < 0.75)
        {
            recommendations.Add("Avoid expanding artifact contracts until pass rate improves.");
        }

        if (result.Diagnostics.Any(diagnostic => diagnostic.Code == GeneratorPlanStrictLlmEvaluationDiagnosticCodes.GenericTextWarning
            || diagnostic.Code == GeneratorPlanStrictLlmEvaluationDiagnosticCodes.ShortDescriptionWarning))
        {
            recommendations.Add("Tighten prompt guidance for concrete names and useful descriptions.");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Continue collecting batch samples before changing contracts.");
        }

        return recommendations;
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

    internal static string Cell(string? value)
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
