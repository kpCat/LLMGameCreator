using System.Text;

namespace LLMGameCreator.Application.Composition;

public sealed class GameCompositionDiagnosticsMarkdownRenderer
{
    public string Render(GameCompositionDiagnosticsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Game Composition Diagnostics");
        builder.AppendLine();
        builder.AppendLine("## Blueprint");
        builder.AppendLine();
        builder.AppendLine($"- ID: {Cell(report.BlueprintId)}");
        builder.AppendLine($"- Title: {Cell(report.Title)}");
        builder.AppendLine($"- Game kind: {report.GameKind}");
        builder.AppendLine($"- Requested capabilities: {report.RequestedCapabilityIds.Count}");
        builder.AppendLine();
        builder.AppendLine("## Readiness");
        builder.AppendLine();
        builder.AppendLine($"**{report.Readiness}**");
        builder.AppendLine();
        builder.AppendLine("## Content language");
        builder.AppendLine();
        builder.AppendLine(Cell(report.ContentLanguage));
        builder.AppendLine();

        AppendDiagnostics(builder, "Capability diagnostics", report.Diagnostics.Where(item => item.Source == "capability"));
        AppendDiagnostics(builder, "Generator catalog diagnostics", report.Diagnostics.Where(item => item.Source == "generator_catalog"));
        AppendDiagnostics(builder, "Generator planning diagnostics", report.Diagnostics.Where(item => item.Source == "generator_plan"));
        AppendList(builder, "Selected current generators", report.SelectedCurrentGeneratorIds, "No current generators selected.");
        AppendList(builder, "Related planned generators", report.RelatedPlannedGeneratorIds, "No related planned generators.");
        AppendList(builder, "Missing generator support", report.MissingGeneratorCapabilityIds, "No missing generator capability support.");

        builder.AppendLine("## Recommended actions");
        builder.AppendLine();
        foreach (var action in report.RecommendedActions)
        {
            builder.AppendLine($"- {Cell(action.Message)}");
        }

        return builder.ToString();
    }

    private static void AppendDiagnostics(
        StringBuilder builder,
        string heading,
        IEnumerable<GameCompositionDiagnosticItem> diagnostics)
    {
        var items = diagnostics.ToList();
        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        if (items.Count == 0)
        {
            builder.AppendLine("_No diagnostics._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Subject | Related | Message |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var item in items)
        {
            builder.AppendLine($"| {item.Severity} | {Cell(item.Code)} | {Cell(item.SubjectId)} | {Cell(item.RelatedId)} | {Cell(item.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendList(
        StringBuilder builder,
        string heading,
        IReadOnlyList<string> values,
        string emptyMessage)
    {
        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        if (values.Count == 0)
        {
            builder.AppendLine($"_{emptyMessage}_");
            builder.AppendLine();
            return;
        }

        foreach (var value in values)
        {
            builder.AppendLine($"- {Cell(value)}");
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
