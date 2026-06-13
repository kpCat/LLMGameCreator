using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueMarkdownRenderer
{
    public string Render(GeneratorPlanDraftArtifactQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var builder = new StringBuilder();
        builder.AppendLine("# Draft Artifact Production Queue");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(queue.Status)}**");
        builder.AppendLine($"- Queue ID: {Cell(queue.Id)}");
        builder.AppendLine($"- Source execution plan: {Cell(queue.SourceDraftExecutionPlanId)}");
        builder.AppendLine($"- Source example: {Cell(queue.SourcePreviewExampleId)}");
        builder.AppendLine($"- Source path: {Cell(queue.SourcePath)}");
        builder.AppendLine($"- Items: {queue.Summary.ItemCount}");
        builder.AppendLine($"- Ready items: {queue.Summary.ReadyItemCount}");
        builder.AppendLine($"- Blocked items: {queue.Summary.BlockedItemCount}");
        builder.AppendLine($"- Validation gates: {queue.Summary.ValidationGateCount}");
        builder.AppendLine($"- Repair requests: {queue.Summary.RepairRequestCount}");
        builder.AppendLine();

        AppendItems(builder, queue.Items);
        AppendValidationGates(builder, queue.Items);
        AppendRepairRequests(builder, queue.RepairRequests);
        AppendDiagnostics(builder, queue.Diagnostics);
        return builder.ToString();
    }

    private static void AppendItems(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactQueueItem> items)
    {
        builder.AppendLine("## Queue Items");
        builder.AppendLine();

        if (items.Count == 0)
        {
            builder.AppendLine("_No draft artifact queue items were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Order | State | Item ID | Step | Artifact | Kind | Contract | Gates | Approval |");
        builder.AppendLine("|---:|---|---|---|---|---|---|---:|---|");
        foreach (var item in items.OrderBy(item => item.Order).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
        {
            var approval = item.RequiresHumanApproval ? "required" : "not required";
            builder.AppendLine($"| {item.Order} | {Cell(item.State)} | {Cell(item.Id)} | {Cell(item.SourceExecutionStepId)} | {Cell(item.ArtifactId)} | {Cell(item.ArtifactKind)} | {Cell(item.ExpectedArtifactContract)} | {item.ValidationGates.Count} | {approval} |");
        }

        builder.AppendLine();
    }

    private static void AppendValidationGates(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactQueueItem> items)
    {
        builder.AppendLine("## Validation Gates");
        builder.AppendLine();

        var gates = items.SelectMany(item => item.ValidationGates).ToList();
        if (gates.Count == 0)
        {
            builder.AppendLine("_No validation gate tickets were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Gate ID | State | Artifact | Step |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var gate in gates.OrderBy(gate => gate.SourceExecutionStepId, StringComparer.OrdinalIgnoreCase).ThenBy(gate => gate.GateId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(gate.GateId)} | {Cell(gate.State)} | {Cell(gate.ArtifactId)} | {Cell(gate.SourceExecutionStepId)} |");
        }

        builder.AppendLine();
    }

    private static void AppendRepairRequests(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactRepairRequest> repairRequests)
    {
        builder.AppendLine("## Repair Requests");
        builder.AppendLine();

        if (repairRequests.Count == 0)
        {
            builder.AppendLine("_No repair request drafts were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Request ID | State | Step | Artifact | Reason | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var request in repairRequests.OrderBy(request => request.SourceExecutionStepId, StringComparer.OrdinalIgnoreCase).ThenBy(request => request.Id, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(request.Id)} | {Cell(request.State)} | {Cell(request.SourceExecutionStepId)} | {Cell(request.ArtifactId)} | {Cell(request.ReasonCode)} | {Cell(request.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Item | Artifact | Gate | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanDraftArtifactQueuePolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.ItemId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.GateId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.ItemId)} | {Cell(diagnostic.ArtifactId)} | {Cell(diagnostic.GateId)} | {Cell(diagnostic.Message)} |");
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
