namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueValidator
{
    public GeneratorPlanDraftArtifactQueue Validate(GeneratorPlanDraftArtifactQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var diagnostics = new List<GeneratorPlanDraftArtifactQueueDiagnostic>(queue.Diagnostics);

        if (string.IsNullOrWhiteSpace(queue.Id))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactQueueDiagnosticCodes.MissingQueueId, "Draft artifact queue id is required.", queue.Id);
        }

        if (queue.Items.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactQueueDiagnosticCodes.NoItems, "Draft artifact queue must contain at least one item.", queue.Id);
        }

        var itemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var artifactIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in queue.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Id) && !itemIds.Add(item.Id))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactQueueDiagnosticCodes.DuplicateItemId, $"Duplicate queue item id: {item.Id}", queue.Id, item.Id, item.ArtifactId);
            }

            if (!string.IsNullOrWhiteSpace(item.ArtifactId) && !artifactIds.Add(item.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactQueueDiagnosticCodes.DuplicateArtifactId, $"Duplicate artifact id: {item.ArtifactId}", queue.Id, item.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.SourceExecutionStepId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingSourceExecutionStepId, "Queue item should reference a source draft execution step id.", queue.Id, item.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingArtifactId, "Queue item artifact id is required.", queue.Id, item.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.ArtifactKind))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingArtifactKind, "Queue item artifact kind should be set before production.", queue.Id, item.Id, item.ArtifactId);
            }

            if (string.IsNullOrWhiteSpace(item.ExpectedArtifactContract))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingExpectedArtifactContract, "Queue item expected artifact contract should be set before production.", queue.Id, item.Id, item.ArtifactId);
            }

            if (item.ValidationGates.Count == 0)
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingValidationGates, "Queue item should include validation gates before production.", queue.Id, item.Id, item.ArtifactId);
            }

            foreach (var gate in item.ValidationGates)
            {
                if (string.IsNullOrWhiteSpace(gate.GateId))
                {
                    Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.GateMissingId, "Validation gate ticket should include a gate id.", queue.Id, item.Id, item.ArtifactId, gate.Id);
                }
            }
        }

        foreach (var repairRequest in queue.RepairRequests)
        {
            if (string.IsNullOrWhiteSpace(repairRequest.ReasonCode) || string.IsNullOrWhiteSpace(repairRequest.Message))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactQueueDiagnosticCodes.RepairRequestMissingReason, "Repair request should include a reason code and message.", queue.Id, repairRequest.SourceExecutionStepId, repairRequest.ArtifactId, repairRequest.Id);
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(diagnostic => GeneratorPlanDraftArtifactQueuePolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.QueueId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ItemId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.GateId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var validated = queue with { Diagnostics = orderedDiagnostics };
        var summary = GeneratorPlanDraftArtifactQueuePolicy.BuildSummary(validated, orderedDiagnostics);
        return validated with
        {
            Status = GeneratorPlanDraftArtifactQueuePolicy.BuildStatus(validated.Items, summary.ErrorCount),
            Summary = summary
        };
    }

    private static void Add(
        ICollection<GeneratorPlanDraftArtifactQueueDiagnostic> diagnostics,
        string severity,
        string code,
        string message,
        string? queueId = null,
        string? itemId = null,
        string? artifactId = null,
        string? gateId = null)
    {
        diagnostics.Add(GeneratorPlanDraftArtifactQueuePolicy.Diagnostic(severity, code, message, queueId, itemId, artifactId, gateId));
    }
}
