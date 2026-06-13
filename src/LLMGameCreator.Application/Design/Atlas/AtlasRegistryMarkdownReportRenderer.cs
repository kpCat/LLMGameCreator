using System.Text;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryMarkdownReportRenderer
{
    public string Render(AtlasRegistryImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();

        builder.AppendLine("# Atlas Registry Import Report");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{(result.Ok ? "OK" : "FAILED")}**");
        builder.AppendLine($"- Atlas root: `{InlineCode(result.AtlasRoot)}`");
        builder.AppendLine();

        AppendSummary(builder, result.Summary);
        AppendDocuments(builder, result.Documents);
        AppendExamples(builder, result.Examples);
        AppendDiagnostics(builder, result.Diagnostics);

        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, AtlasRegistrySummary summary)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| Documents | {summary.DocumentCount} |");
        builder.AppendLine($"| Loaded documents | {summary.LoadedDocumentCount} |");
        builder.AppendLine($"| Examples | {summary.ExampleCount} |");
        builder.AppendLine($"| Unique IDs | {summary.UniqueIdCount} |");
        builder.AppendLine($"| Errors | {summary.ErrorCount} |");
        builder.AppendLine($"| Warnings | {summary.WarningCount} |");
        builder.AppendLine();
    }

    private static void AppendDocuments(StringBuilder builder, IReadOnlyList<AtlasDocumentSummary> documents)
    {
        builder.AppendLine("## Documents");
        builder.AppendLine();

        if (documents.Count == 0)
        {
            builder.AppendLine("_No atlas documents were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Path | ID | Title | Loaded | Top-level IDs | References |");
        builder.AppendLine("|---|---|---|---:|---:|---:|");

        foreach (var document in documents.OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine(
                $"| {Cell(document.Path)} | {Cell(document.Id)} | {Cell(document.Title)} | {Bool(document.Loaded)} | {document.TopLevelIds.Count} | {document.ReferencedIds.Count} |");
        }

        builder.AppendLine();
    }

    private static void AppendExamples(StringBuilder builder, IReadOnlyList<AtlasExampleSummary> examples)
    {
        builder.AppendLine("## Examples");
        builder.AppendLine();

        if (examples.Count == 0)
        {
            builder.AppendLine("_No atlas examples were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Path | Example ID | Title | Source profile | Bundles | Target artifacts | Steps |");
        builder.AppendLine("|---|---|---|---|---:|---:|---:|");

        foreach (var example in examples.OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine(
                $"| {Cell(example.Path)} | {Cell(example.ExampleId)} | {Cell(example.Title)} | {Cell(example.SourceProfileId)} | {example.SelectedFeatureBundles.Count} | {example.TargetArtifacts.Count} | {example.StepCount} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<AtlasDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Path | ID | Message |");
        builder.AppendLine("|---|---|---|---|---|");

        foreach (var diagnostic in diagnostics
                     .OrderBy(item => SeverityOrder(item.Severity))
                     .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine(
                $"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.Path)} | {Cell(diagnostic.Id)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            AtlasDiagnosticSeverity.Error => 0,
            AtlasDiagnosticSeverity.Warning => 1,
            AtlasDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }

    private static string Bool(bool value)
    {
        return value ? "yes" : "no";
    }

    private static string Cell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal)
            .Trim();
    }

    private static string InlineCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value.Replace("`", "\\`", StringComparison.Ordinal);
    }
}
