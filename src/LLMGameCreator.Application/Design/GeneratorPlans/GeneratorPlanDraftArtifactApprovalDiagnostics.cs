using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanDraftArtifactApprovalDiagnosticCodes
{
    public const string MissingSnapshotId = "generator_plan_draft_artifact_approval.missing_snapshot_id";
    public const string NoItems = "generator_plan_draft_artifact_approval.no_items";
    public const string DuplicateArtifactId = "generator_plan_draft_artifact_approval.duplicate_artifact_id";
    public const string ItemMissingArtifactId = "generator_plan_draft_artifact_approval.item_missing_artifact_id";
    public const string ItemMissingArtifactKind = "generator_plan_draft_artifact_approval.item_missing_artifact_kind";
    public const string ItemInvalidJson = "generator_plan_draft_artifact_approval.item_invalid_json";
    public const string ApprovedItemInvalidJson = "generator_plan_draft_artifact_approval.approved_item_invalid_json";
    public const string ApprovedItemMissingContract = "generator_plan_draft_artifact_approval.approved_item_missing_contract";
    public const string RejectedItemMissingReason = "generator_plan_draft_artifact_approval.rejected_item_missing_reason";
    public const string RepairRequestedMissingReason = "generator_plan_draft_artifact_approval.repair_requested_missing_reason";
    public const string BlockedItemWithoutRepairRequest = "generator_plan_draft_artifact_approval.blocked_item_without_repair_request";
    public const string ProductionDiagnostic = "generator_plan_draft_artifact_approval.production_diagnostic";
    public const string ReviewStagingArtifactMissing = "generator_plan_draft_artifact_review.staging_artifact_missing";
    public const string ReviewApproveInvalidArtifact = "generator_plan_draft_artifact_review.approve_invalid_artifact";
    public const string ReviewBlockedDecisionIgnored = "generator_plan_draft_artifact_review.blocked_decision_ignored";
    public const string ReviewUnknownArtifactDecision = "generator_plan_draft_artifact_review.unknown_artifact_decision";
}

public static class GeneratorPlanDraftArtifactApprovalPolicy
{
    public static GeneratorPlanDraftArtifactStagingSummary BuildSummary(
        GeneratorPlanDraftArtifactStagingSnapshot snapshot,
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(diagnostics);

        return new GeneratorPlanDraftArtifactStagingSummary
        {
            ItemCount = snapshot.Items.Count,
            PendingCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Pending),
            ApprovedCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved),
            RejectedCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected),
            RepairRequestedCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested),
            BlockedCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked),
            ReadyForPackageCount = snapshot.Items.Count(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved),
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    public static string BuildStatus(GeneratorPlanDraftArtifactStagingSnapshot snapshot, GeneratorPlanDraftArtifactStagingSummary summary)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftArtifactStagingStatus.Invalid;
        }

        if (summary.RepairRequestedCount > 0 || summary.BlockedCount > 0)
        {
            return GeneratorPlanDraftArtifactStagingStatus.NeedsRepair;
        }

        if (summary.PendingCount > 0)
        {
            return GeneratorPlanDraftArtifactStagingStatus.NeedsReview;
        }

        var nonRejected = snapshot.Items.Where(item => item.State != GeneratorPlanDraftArtifactApprovalItemState.Rejected).ToList();
        if (summary.ApprovedCount > 0 && nonRejected.All(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved))
        {
            return GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage;
        }

        return GeneratorPlanDraftArtifactStagingStatus.Draft;
    }

    public static string ToValidationState(GeneratorPlanDraftArtifactStagingSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftArtifactApprovalValidationState.Invalid;
        }

        return summary.WarningCount > 0
            ? GeneratorPlanDraftArtifactApprovalValidationState.Warnings
            : GeneratorPlanDraftArtifactApprovalValidationState.Valid;
    }

    public static IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> SelectValidationDiagnostics(
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.SnapshotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.SnapshotId ?? string.Empty, diagnostic.ArtifactId ?? string.Empty, diagnostic.Target ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target ?? diagnostic.ArtifactId ?? diagnostic.SnapshotId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    snapshotId = diagnostic.SnapshotId,
                    artifactId = diagnostic.ArtifactId,
                    target = diagnostic.Target
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

    internal static GeneratorPlanDraftArtifactApprovalDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? snapshotId = null,
        string? artifactId = null,
        string? target = null)
    {
        return new GeneratorPlanDraftArtifactApprovalDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            SnapshotId = snapshotId,
            ArtifactId = artifactId,
            Target = target
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
