using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactApprovalValidator
{
    public GeneratorPlanDraftArtifactStagingSnapshot Validate(GeneratorPlanDraftArtifactStagingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var diagnostics = new List<GeneratorPlanDraftArtifactApprovalDiagnostic>(snapshot.Diagnostics);

        if (string.IsNullOrWhiteSpace(snapshot.Id))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.MissingSnapshotId, "Draft artifact staging snapshot id is required.", snapshot.Id);
        }

        if (snapshot.Items.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.NoItems, "Draft artifact staging snapshot must contain at least one approval item.", snapshot.Id);
        }

        var artifactIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in snapshot.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.ArtifactId) && !artifactIds.Add(item.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.DuplicateArtifactId, $"Duplicate approval artifact id: {item.ArtifactId}", snapshot.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ItemMissingArtifactId, "Approval item artifact id is required.", snapshot.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.ArtifactKind))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ItemMissingArtifactKind, "Approval item artifact kind should be set.", snapshot.Id, item.ArtifactId);
            }

            var jsonIsValid = IsValidJson(item.ContentJson, out var jsonMessage);
            if (!jsonIsValid)
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ItemInvalidJson, $"Approval item content_json must be valid JSON: {jsonMessage}", snapshot.Id, item.ArtifactId, "content_json");
            }

            if (item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved)
            {
                if (!jsonIsValid)
                {
                    Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ApprovedItemInvalidJson, "Approved approval item must contain valid JSON.", snapshot.Id, item.ArtifactId, "content_json");
                }

                if (string.IsNullOrWhiteSpace(item.ExpectedArtifactContract))
                {
                    Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ApprovedItemMissingContract, "Approved approval item must include expected artifact contract.", snapshot.Id, item.ArtifactId, "expected_artifact_contract");
                }
            }

            if (item.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected && string.IsNullOrWhiteSpace(item.DecisionReasonCode) && string.IsNullOrWhiteSpace(item.DecisionComment))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.RejectedItemMissingReason, "Rejected approval item should include a reason code or comment.", snapshot.Id, item.ArtifactId, "decision");
            }

            if (item.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested && string.IsNullOrWhiteSpace(item.DecisionReasonCode) && string.IsNullOrWhiteSpace(item.DecisionComment))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.RepairRequestedMissingReason, "Repair-requested approval item should include a reason code or comment.", snapshot.Id, item.ArtifactId, "decision");
            }

            if (item.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked && string.IsNullOrWhiteSpace(item.RepairRequestId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactApprovalDiagnosticCodes.BlockedItemWithoutRepairRequest, "Blocked approval item should reference a repair request.", snapshot.Id, item.ArtifactId, "repair_request_id");
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(diagnostic => GeneratorPlanDraftArtifactApprovalPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.SnapshotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var validated = snapshot with { Diagnostics = orderedDiagnostics };
        var summary = GeneratorPlanDraftArtifactApprovalPolicy.BuildSummary(validated, orderedDiagnostics);
        return validated with
        {
            Status = GeneratorPlanDraftArtifactApprovalPolicy.BuildStatus(validated, summary),
            Summary = summary
        };
    }

    private static bool IsValidJson(string contentJson, out string message)
    {
        try
        {
            using var _ = JsonDocument.Parse(string.IsNullOrWhiteSpace(contentJson) ? string.Empty : contentJson);
            message = string.Empty;
            return true;
        }
        catch (JsonException exception)
        {
            message = exception.Message;
            return false;
        }
    }

    private static void Add(
        ICollection<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics,
        string severity,
        string code,
        string message,
        string? snapshotId = null,
        string? artifactId = null,
        string? target = null)
    {
        diagnostics.Add(GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(severity, code, message, snapshotId, artifactId, target));
    }
}
