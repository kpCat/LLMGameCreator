using System.Text;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPackageExportRunMarkdownRenderer
{
    public string Render(GeneratorPlanPackageExportRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("# One-click Package Export Run");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(result.Status)}**");
        builder.AppendLine($"- Source example: {Cell(result.SourceExamplePath)}");
        builder.AppendLine($"- Export folder: {Cell(result.ExportFolderPath)}");
        builder.AppendLine($"- package.json path: {Cell(result.PackageJsonPath)}");
        builder.AppendLine($"- Approval status: {Cell(result.ApprovalArtifacts.ApprovalResult.Status)}");
        builder.AppendLine($"- Assembly status: {Cell(result.AssemblyResult.Status)}");
        builder.AppendLine($"- Package title/id: {Cell(result.AssemblyResult.Package.Manifest.Title)} / {Cell(result.AssemblyResult.Package.Manifest.PackageId)}");
        builder.AppendLine($"- Artifact IDs saved: {Cell(ArtifactIds(result))}");
        builder.AppendLine();

        AppendFiles(builder, result);
        AppendDiagnostics(builder, result.Diagnostics);
        builder.AppendLine("## Next step");
        builder.AppendLine();
        builder.AppendLine("Open/exported package folder and validate/run package preview.");
        return builder.ToString();
    }

    private static void AppendFiles(StringBuilder builder, GeneratorPlanPackageExportRunResult result)
    {
        builder.AppendLine("## Files");
        builder.AppendLine();
        builder.AppendLine("| Kind | Path | Exists |");
        builder.AppendLine("|---|---|---|");
        builder.AppendLine($"| package_json | {Cell(result.PackageJsonPath)} | {File.Exists(result.PackageJsonPath)} |");
        AppendArtifactFile(builder, "approval_staging", result.ApprovalArtifacts.StagingArtifact);
        AppendArtifactFile(builder, "approved_artifact_set", result.ApprovalArtifacts.ApprovedArtifactSetArtifact);
        AppendArtifactFile(builder, "approval_markdown", result.ApprovalArtifacts.MarkdownArtifact);
        AppendArtifactFile(builder, "assembly", result.AssemblyArtifacts?.AssemblyArtifact);
        AppendArtifactFile(builder, "package_draft", result.AssemblyArtifacts?.PackageDraftArtifact);
        AppendArtifactFile(builder, "assembly_markdown", result.AssemblyArtifacts?.MarkdownArtifact);
        builder.AppendLine();
    }

    private static void AppendArtifactFile(StringBuilder builder, string kind, GeneratedArtifactRecord? artifact)
    {
        builder.AppendLine($"| {Cell(kind)} | {Cell(artifact?.Path)} | {(!string.IsNullOrWhiteSpace(artifact?.Id)).ToString()} |");
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Target | Message |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.Target)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static string ArtifactIds(GeneratorPlanPackageExportRunResult result)
    {
        var ids = new[]
            {
                result.ApprovalArtifacts.StagingArtifact.Id,
                result.ApprovalArtifacts.MarkdownArtifact?.Id,
                result.ApprovalArtifacts.ApprovedArtifactSetArtifact.Id,
                result.AssemblyArtifacts?.AssemblyArtifact.Id,
                result.AssemblyArtifacts?.PackageDraftArtifact.Id,
                result.AssemblyArtifacts?.MarkdownArtifact?.Id,
                GeneratorPlanPackageExportRunArtifactIds.RunArtifactId,
                string.IsNullOrWhiteSpace(result.MarkdownReport) ? null : GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactId
            }
            .Where(id => !string.IsNullOrWhiteSpace(id));

        return string.Join(", ", ids);
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
