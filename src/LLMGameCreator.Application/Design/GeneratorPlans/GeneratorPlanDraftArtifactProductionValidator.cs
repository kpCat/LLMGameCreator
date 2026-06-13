using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactProductionValidator
{
    public GeneratorPlanDraftArtifactProductionBatch Validate(GeneratorPlanDraftArtifactProductionBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var diagnostics = new List<GeneratorPlanDraftArtifactProductionDiagnostic>(batch.Diagnostics);

        if (string.IsNullOrWhiteSpace(batch.Id))
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactProductionDiagnosticCodes.MissingBatchId, "Draft artifact production batch id is required.", batch.Id);
        }

        if (batch.Artifacts.Count == 0)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactProductionDiagnosticCodes.NoArtifacts, "Draft artifact production must contain at least one produced artifact.", batch.Id);
        }

        var artifactIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var artifact in batch.Artifacts)
        {
            if (!string.IsNullOrWhiteSpace(artifact.ArtifactId) && !artifactIds.Add(artifact.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactProductionDiagnosticCodes.DuplicateArtifactId, $"Duplicate produced artifact id: {artifact.ArtifactId}", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }

            if (string.IsNullOrWhiteSpace(artifact.QueueItemId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingQueueItemId, "Produced artifact should reference a source queue item id.", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }

            if (string.IsNullOrWhiteSpace(artifact.ArtifactId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingArtifactId, "Produced artifact id is required.", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }

            if (string.IsNullOrWhiteSpace(artifact.ArtifactKind))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingArtifactKind, "Produced artifact kind should be set.", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }

            if (string.IsNullOrWhiteSpace(artifact.ExpectedArtifactContract))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingExpectedArtifactContract, "Produced artifact expected contract should be set.", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }

            ValidateJson(diagnostics, batch.Id, artifact);

            if (artifact.State == GeneratorPlanProducedDraftArtifactState.Blocked && string.IsNullOrWhiteSpace(artifact.RepairRequestId))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.BlockedArtifactMissingRepairRequest, "Blocked produced artifact should reference a repair request.", batch.Id, artifact.ArtifactId, artifact.QueueItemId);
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(diagnostic => GeneratorPlanDraftArtifactProductionPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.BatchId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.QueueItemId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var validated = batch with { Diagnostics = orderedDiagnostics };
        var summary = GeneratorPlanDraftArtifactProductionPolicy.BuildSummary(validated, orderedDiagnostics);
        return validated with
        {
            Status = GeneratorPlanDraftArtifactProductionPolicy.BuildStatus(validated.Artifacts, summary.ErrorCount),
            Summary = summary
        };
    }

    private static void ValidateJson(
        ICollection<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics,
        string batchId,
        GeneratorPlanProducedDraftArtifact artifact)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(artifact.ContentJson) ? string.Empty : artifact.ContentJson);
        }
        catch (JsonException exception)
        {
            Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactInvalidJson, $"Produced artifact content_json must be valid JSON: {exception.Message}", batchId, artifact.ArtifactId, artifact.QueueItemId, "content_json");
            return;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("schema_version", out _))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingSchemaVersion, "Produced artifact JSON should include schema_version.", batchId, artifact.ArtifactId, artifact.QueueItemId, "schema_version");
            }

            if (!document.RootElement.TryGetProperty("artifact_id", out var artifactIdProperty) || string.IsNullOrWhiteSpace(artifactIdProperty.GetString()))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactContentMissingArtifactId, "Produced artifact JSON should include artifact_id.", batchId, artifact.ArtifactId, artifact.QueueItemId, "artifact_id");
                return;
            }

            var contentArtifactId = artifactIdProperty.GetString();
            if (!string.Equals(contentArtifactId, artifact.ArtifactId, StringComparison.Ordinal))
            {
                Add(diagnostics, GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactContentArtifactIdMismatch, $"Produced artifact JSON artifact_id '{contentArtifactId}' does not match '{artifact.ArtifactId}'.", batchId, artifact.ArtifactId, artifact.QueueItemId, "artifact_id");
            }
        }
    }

    private static void Add(
        ICollection<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics,
        string severity,
        string code,
        string message,
        string? batchId = null,
        string? artifactId = null,
        string? queueItemId = null,
        string? target = null)
    {
        diagnostics.Add(GeneratorPlanDraftArtifactProductionPolicy.Diagnostic(severity, code, message, batchId, artifactId, queueItemId, target));
    }
}
