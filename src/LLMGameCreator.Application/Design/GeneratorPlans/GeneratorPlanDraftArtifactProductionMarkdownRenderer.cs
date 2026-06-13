using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactProductionMarkdownRenderer
{
    private const int JsonPreviewMaxLength = 1500;

    public string Render(GeneratorPlanDraftArtifactProductionBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var builder = new StringBuilder();
        builder.AppendLine("# Draft Artifact Production");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(batch.Status)}**");
        builder.AppendLine($"- Batch ID: {Cell(batch.Id)}");
        builder.AppendLine($"- Source queue: {Cell(batch.SourceQueueId)}");
        builder.AppendLine($"- Source execution plan: {Cell(batch.SourceDraftExecutionPlanId)}");
        builder.AppendLine($"- Source example: {Cell(batch.SourcePreviewExampleId)}");
        builder.AppendLine($"- Produced artifacts: {batch.Summary.ArtifactCount}");
        builder.AppendLine($"- Ready for approval: {batch.Summary.ReadyForApprovalCount}");
        builder.AppendLine($"- Blocked: {batch.Summary.BlockedArtifactCount}");
        builder.AppendLine($"- Repair requests: {batch.Summary.RepairRequestCount}");
        builder.AppendLine();

        AppendArtifacts(builder, batch.Artifacts);
        AppendDiagnostics(builder, batch.Diagnostics);
        AppendJsonPreview(builder, batch.Artifacts);
        return builder.ToString();
    }

    private static void AppendArtifacts(StringBuilder builder, IReadOnlyList<GeneratorPlanProducedDraftArtifact> artifacts)
    {
        builder.AppendLine("## Produced Artifacts");
        builder.AppendLine();

        if (artifacts.Count == 0)
        {
            builder.AppendLine("_No draft artifacts were produced._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| State | Artifact ID | Kind | Contract | Queue Item | Gates | Approval |");
        builder.AppendLine("|---|---|---|---|---|---:|---|");
        foreach (var artifact in artifacts.OrderBy(artifact => artifact.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            var approval = artifact.RequiresHumanApproval ? "required" : "not required";
            builder.AppendLine($"| {Cell(artifact.State)} | {Cell(artifact.ArtifactId)} | {Cell(artifact.ArtifactKind)} | {Cell(artifact.ExpectedArtifactContract)} | {Cell(artifact.QueueItemId)} | {artifact.ValidationGates.Count} | {approval} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Artifact | Queue Item | Target | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanDraftArtifactProductionPolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.QueueItemId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.ArtifactId)} | {Cell(diagnostic.QueueItemId)} | {Cell(diagnostic.Target)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendJsonPreview(StringBuilder builder, IReadOnlyList<GeneratorPlanProducedDraftArtifact> artifacts)
    {
        builder.AppendLine("## Artifact JSON Preview");
        builder.AppendLine();

        if (artifacts.Count == 0)
        {
            builder.AppendLine("_No artifact JSON is available._");
            builder.AppendLine();
            return;
        }

        foreach (var artifact in artifacts.OrderBy(artifact => artifact.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"### {artifact.ArtifactId}");
            builder.AppendLine();
            builder.AppendLine("```json");
            builder.AppendLine(Truncate(artifact.ContentJson));
            builder.AppendLine("```");
            builder.AppendLine();
        }
    }

    private static string Truncate(string value)
    {
        if (value.Length <= JsonPreviewMaxLength)
        {
            return value;
        }

        return value[..JsonPreviewMaxLength] + "\n...";
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
