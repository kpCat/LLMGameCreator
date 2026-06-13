using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanDraftArtifactQueueStatus
{
    public const string Draft = "draft";
    public const string Ready = "ready";
    public const string Blocked = "blocked";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanDraftArtifactQueueItemState
{
    public const string Pending = "pending";
    public const string Ready = "ready";
    public const string Blocked = "blocked";
}

public static class GeneratorPlanDraftValidationGateState
{
    public const string Pending = "pending";
    public const string Blocked = "blocked";
}

public static class GeneratorPlanDraftRepairRequestState
{
    public const string Draft = "draft";
    public const string Resolved = "resolved";
}

public static class GeneratorPlanDraftArtifactQueueValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanDraftArtifactQueueDiagnosticCodes
{
    public const string MissingQueueId = "generator_plan_draft_artifact_queue.missing_queue_id";
    public const string NoItems = "generator_plan_draft_artifact_queue.no_items";
    public const string DuplicateItemId = "generator_plan_draft_artifact_queue.duplicate_item_id";
    public const string DuplicateArtifactId = "generator_plan_draft_artifact_queue.duplicate_artifact_id";
    public const string ItemMissingSourceExecutionStepId = "generator_plan_draft_artifact_queue.item_missing_source_execution_step_id";
    public const string ItemMissingArtifactId = "generator_plan_draft_artifact_queue.item_missing_artifact_id";
    public const string ItemMissingArtifactKind = "generator_plan_draft_artifact_queue.item_missing_artifact_kind";
    public const string ItemMissingExpectedArtifactContract = "generator_plan_draft_artifact_queue.item_missing_expected_artifact_contract";
    public const string ItemMissingValidationGates = "generator_plan_draft_artifact_queue.item_missing_validation_gates";
    public const string GateMissingId = "generator_plan_draft_artifact_queue.gate_missing_id";
    public const string RepairRequestMissingReason = "generator_plan_draft_artifact_queue.repair_request_missing_reason";
    public const string ExecutionDiagnostic = "generator_plan_draft_artifact_queue.execution_diagnostic";
}

public static class GeneratorPlanDraftArtifactQueuePolicy
{
    public static string ToValidationState(GeneratorPlanDraftArtifactQueueSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftArtifactQueueValidationState.Invalid;
        }

        return summary.WarningCount > 0
            ? GeneratorPlanDraftArtifactQueueValidationState.Warnings
            : GeneratorPlanDraftArtifactQueueValidationState.Valid;
    }

    public static IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> SelectValidationDiagnostics(
        IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.QueueId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ItemId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.GateId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.QueueId ?? string.Empty, diagnostic.ItemId ?? string.Empty, diagnostic.ArtifactId ?? string.Empty, diagnostic.GateId ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.GateId ?? diagnostic.ArtifactId ?? diagnostic.ItemId ?? diagnostic.QueueId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    queueId = diagnostic.QueueId,
                    itemId = diagnostic.ItemId,
                    artifactId = diagnostic.ArtifactId,
                    gateId = diagnostic.GateId
                })))
            .ToList();
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

    internal static GeneratorPlanDraftArtifactQueueSummary BuildSummary(
        GeneratorPlanDraftArtifactQueue queue,
        IReadOnlyList<GeneratorPlanDraftArtifactQueueDiagnostic> diagnostics)
    {
        return new GeneratorPlanDraftArtifactQueueSummary
        {
            ItemCount = queue.Items.Count,
            PendingItemCount = queue.Items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Pending),
            BlockedItemCount = queue.Items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked),
            ReadyItemCount = queue.Items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Ready),
            ValidationGateCount = queue.Items.Sum(item => item.ValidationGates.Count),
            RepairRequestCount = queue.RepairRequests.Count,
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    internal static string BuildStatus(IReadOnlyList<GeneratorPlanDraftArtifactQueueItem> items, int errorCount)
    {
        if (errorCount > 0)
        {
            return GeneratorPlanDraftArtifactQueueStatus.Invalid;
        }

        if (items.Any(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked))
        {
            return GeneratorPlanDraftArtifactQueueStatus.Blocked;
        }

        return items.Count > 0
            ? GeneratorPlanDraftArtifactQueueStatus.Ready
            : GeneratorPlanDraftArtifactQueueStatus.Draft;
    }

    internal static GeneratorPlanDraftArtifactQueueDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? queueId = null,
        string? itemId = null,
        string? artifactId = null,
        string? gateId = null)
    {
        return new GeneratorPlanDraftArtifactQueueDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            QueueId = queueId,
            ItemId = itemId,
            ArtifactId = artifactId,
            GateId = gateId
        };
    }

    internal static string NormalizeSegment(string value)
    {
        var chars = value.Trim()
            .Select(character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.'
                ? char.ToLowerInvariant(character)
                : '_')
            .ToArray();
        var normalized = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
