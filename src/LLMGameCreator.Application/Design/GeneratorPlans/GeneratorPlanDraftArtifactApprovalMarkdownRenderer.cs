using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactApprovalMarkdownRenderer
{
    private const int JsonPreviewMaxLength = 1500;

    public string Render(GeneratorPlanDraftArtifactStagingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var builder = new StringBuilder();
        builder.AppendLine("# Draft Artifact Approval/Staging");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(snapshot.Status)}**");
        builder.AppendLine($"- Snapshot ID: {Cell(snapshot.Id)}");
        builder.AppendLine($"- Source production batch: {Cell(snapshot.SourceProductionBatchId)}");
        builder.AppendLine($"- Source example: {Cell(snapshot.SourcePreviewExampleId)}");
        builder.AppendLine($"- Items: {snapshot.Summary.ItemCount}");
        builder.AppendLine($"- Approved: {snapshot.Summary.ApprovedCount}");
        builder.AppendLine($"- Pending: {snapshot.Summary.PendingCount}");
        builder.AppendLine($"- Rejected: {snapshot.Summary.RejectedCount}");
        builder.AppendLine($"- Repair requested: {snapshot.Summary.RepairRequestedCount}");
        builder.AppendLine($"- Blocked: {snapshot.Summary.BlockedCount}");
        builder.AppendLine($"- Ready for package: {snapshot.Summary.ReadyForPackageCount}");
        builder.AppendLine();

        AppendWorklist(builder, snapshot.Items);
        AppendApprovedSet(builder, snapshot.Items);
        AppendRejectedRepair(builder, snapshot.Items);
        AppendDiagnostics(builder, snapshot.Diagnostics);
        AppendApprovedJsonPreview(builder, snapshot.Items);
        return builder.ToString();
    }

    private static void AppendWorklist(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> items)
    {
        builder.AppendLine("## Approval Worklist");
        builder.AppendLine();

        if (items.Count == 0)
        {
            builder.AppendLine("_No approval items were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| State | Artifact ID | Kind | Contract | Requires approval | Repair request | Reason |");
        builder.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var item in items.OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(item.State)} | {Cell(item.ArtifactId)} | {Cell(item.ArtifactKind)} | {Cell(item.ExpectedArtifactContract)} | {Cell(item.RequiresHumanApproval ? "yes" : "no")} | {Cell(item.RepairRequestId)} | {Cell(item.DecisionReasonCode)} |");
        }

        builder.AppendLine();
    }

    private static void AppendApprovedSet(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> items)
    {
        builder.AppendLine("## Approved Artifact Set");
        builder.AppendLine();

        var approved = items.Where(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved).OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase).ToList();
        if (approved.Count == 0)
        {
            builder.AppendLine("_No artifacts are approved for package staging._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Artifact ID | Kind | Contract |");
        builder.AppendLine("|---|---|---|");
        foreach (var item in approved)
        {
            builder.AppendLine($"| {Cell(item.ArtifactId)} | {Cell(item.ArtifactKind)} | {Cell(item.ExpectedArtifactContract)} |");
        }

        builder.AppendLine();
    }

    private static void AppendRejectedRepair(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> items)
    {
        builder.AppendLine("## Rejected / Repair");
        builder.AppendLine();

        var reviewItems = items
            .Where(item => item.State is GeneratorPlanDraftArtifactApprovalItemState.Rejected or GeneratorPlanDraftArtifactApprovalItemState.RepairRequested or GeneratorPlanDraftArtifactApprovalItemState.Blocked)
            .OrderBy(item => item.State, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (reviewItems.Count == 0)
        {
            builder.AppendLine("_No rejected, repair-requested, or blocked artifacts were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| State | Artifact ID | Reason | Comment |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var item in reviewItems)
        {
            builder.AppendLine($"| {Cell(item.State)} | {Cell(item.ArtifactId)} | {Cell(item.DecisionReasonCode)} | {Cell(item.DecisionComment)} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Artifact | Target | Message |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanDraftArtifactApprovalPolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.ArtifactId)} | {Cell(diagnostic.Target)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendApprovedJsonPreview(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> items)
    {
        builder.AppendLine("## Approved JSON Preview");
        builder.AppendLine();

        var approved = items.Where(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved).OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase).ToList();
        if (approved.Count == 0)
        {
            builder.AppendLine("_No approved JSON is available._");
            builder.AppendLine();
            return;
        }

        foreach (var item in approved)
        {
            builder.AppendLine($"### {item.ArtifactId}");
            builder.AppendLine();
            builder.AppendLine("```json");
            builder.AppendLine(Truncate(item.ContentJson));
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
