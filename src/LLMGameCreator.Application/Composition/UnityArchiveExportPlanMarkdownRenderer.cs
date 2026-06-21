using System.Text;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveExportPlanMarkdownRenderer
{
    public string Render(UnityArchiveExportPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var builder = new StringBuilder();
        builder.AppendLine("# Unity Archive Export Dry Run");
        builder.AppendLine();
        builder.AppendLine("## Readiness");
        builder.AppendLine();
        builder.AppendLine($"**{plan.Readiness}**");
        builder.AppendLine();
        builder.AppendLine("## Design brief");
        builder.AppendLine();
        builder.AppendLine($"- ID: {Cell(plan.DesignBriefId)}");
        builder.AppendLine();
        builder.AppendLine("## Target profile");
        builder.AppendLine();
        builder.AppendLine($"- ID: {Cell(plan.TargetProfileId)}");
        builder.AppendLine($"- Archive game ID: {Cell(plan.ArchiveGameId)}");
        builder.AppendLine();

        AppendList(builder, "Runtime modules", plan.RuntimeModuleIds, "No runtime modules selected.");

        builder.AppendLine("## Planned files");
        builder.AppendLine();
        if (plan.PlannedFiles.Count == 0)
        {
            builder.AppendLine("_No safe files planned._");
        }
        else
        {
            builder.AppendLine("| Path | Kind | Source | ");
            builder.AppendLine("|---|---|---|");
            foreach (var file in plan.PlannedFiles)
            {
                builder.AppendLine($"| {Cell(file.RelativePath)} | {Cell(file.Kind)} | {Cell(file.SourceId)} |");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();
        if (plan.Diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics._");
        }
        else
        {
            builder.AppendLine("| Severity | Code | Target | Related | Message |");
            builder.AppendLine("|---|---|---|---|---|");
            foreach (var diagnostic in plan.Diagnostics)
            {
                builder.AppendLine($"| {diagnostic.Severity} | {Cell(diagnostic.Code)} | {Cell(diagnostic.TargetId)} | {Cell(diagnostic.RelatedId)} | {Cell(diagnostic.Message)} |");
            }
        }

        builder.AppendLine();
        var blockedModules = plan.Diagnostics
            .Where(diagnostic => diagnostic.Code == UnityArchiveExportDiagnosticCodes.FutureRuntimeModule)
            .Select(diagnostic => diagnostic.RelatedId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(id => id, StringComparer.Ordinal)
            .ToList();
        AppendList(builder, "Blocked/future modules", blockedModules, "No future modules block this dry run.");

        return builder.ToString();
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
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Replace("|", "\\|", StringComparison.Ordinal).Trim();
    }
}
