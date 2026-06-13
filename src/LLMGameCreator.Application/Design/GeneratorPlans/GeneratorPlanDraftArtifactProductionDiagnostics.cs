using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanDraftArtifactProductionDiagnosticCodes
{
    public const string MissingBatchId = "generator_plan_draft_artifact_production.missing_batch_id";
    public const string NoArtifacts = "generator_plan_draft_artifact_production.no_artifacts";
    public const string DuplicateArtifactId = "generator_plan_draft_artifact_production.duplicate_artifact_id";
    public const string ArtifactMissingQueueItemId = "generator_plan_draft_artifact_production.artifact_missing_queue_item_id";
    public const string ArtifactMissingArtifactId = "generator_plan_draft_artifact_production.artifact_missing_artifact_id";
    public const string ArtifactMissingArtifactKind = "generator_plan_draft_artifact_production.artifact_missing_artifact_kind";
    public const string ArtifactMissingExpectedArtifactContract = "generator_plan_draft_artifact_production.artifact_missing_expected_artifact_contract";
    public const string ArtifactInvalidJson = "generator_plan_draft_artifact_production.artifact_invalid_json";
    public const string ArtifactMissingSchemaVersion = "generator_plan_draft_artifact_production.artifact_missing_schema_version";
    public const string ArtifactContentMissingArtifactId = "generator_plan_draft_artifact_production.artifact_content_missing_artifact_id";
    public const string ArtifactContentArtifactIdMismatch = "generator_plan_draft_artifact_production.artifact_content_artifact_id_mismatch";
    public const string BlockedArtifactMissingRepairRequest = "generator_plan_draft_artifact_production.blocked_artifact_missing_repair_request";
    public const string QueueInvalid = "generator_plan_draft_artifact_production.queue_invalid";
    public const string QueueDiagnostic = "generator_plan_draft_artifact_production.queue_diagnostic";
}

public static class GeneratorPlanDraftArtifactProductionPolicy
{
    public static string ToValidationState(GeneratorPlanDraftArtifactProductionSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanDraftArtifactProductionValidationState.Invalid;
        }

        return summary.WarningCount > 0
            ? GeneratorPlanDraftArtifactProductionValidationState.Warnings
            : GeneratorPlanDraftArtifactProductionValidationState.Valid;
    }

    public static IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> SelectValidationDiagnostics(
        IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.BatchId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.QueueItemId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.BatchId ?? string.Empty, diagnostic.ArtifactId ?? string.Empty, diagnostic.QueueItemId ?? string.Empty, diagnostic.Target ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target ?? diagnostic.ArtifactId ?? diagnostic.QueueItemId ?? diagnostic.BatchId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    batchId = diagnostic.BatchId,
                    artifactId = diagnostic.ArtifactId,
                    queueItemId = diagnostic.QueueItemId,
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

    internal static GeneratorPlanDraftArtifactProductionSummary BuildSummary(
        GeneratorPlanDraftArtifactProductionBatch batch,
        IReadOnlyList<GeneratorPlanDraftArtifactProductionDiagnostic> diagnostics)
    {
        return new GeneratorPlanDraftArtifactProductionSummary
        {
            ArtifactCount = batch.Artifacts.Count,
            DraftArtifactCount = batch.Artifacts.Count(artifact => artifact.State == GeneratorPlanProducedDraftArtifactState.Draft),
            BlockedArtifactCount = batch.Artifacts.Count(artifact => artifact.State == GeneratorPlanProducedDraftArtifactState.Blocked),
            ReadyForApprovalCount = batch.Artifacts.Count(artifact => artifact.State == GeneratorPlanProducedDraftArtifactState.ReadyForApproval),
            RepairRequestCount = batch.Artifacts.Count(artifact => !string.IsNullOrWhiteSpace(artifact.RepairRequestId)),
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    internal static string BuildStatus(IReadOnlyList<GeneratorPlanProducedDraftArtifact> artifacts, int errorCount)
    {
        if (errorCount > 0)
        {
            return GeneratorPlanDraftArtifactProductionStatus.Invalid;
        }

        if (artifacts.Any(artifact => artifact.State == GeneratorPlanProducedDraftArtifactState.Blocked))
        {
            return GeneratorPlanDraftArtifactProductionStatus.Blocked;
        }

        return artifacts.Count > 0
            ? GeneratorPlanDraftArtifactProductionStatus.ReadyForApproval
            : GeneratorPlanDraftArtifactProductionStatus.Draft;
    }

    internal static GeneratorPlanDraftArtifactProductionDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? batchId = null,
        string? artifactId = null,
        string? queueItemId = null,
        string? target = null)
    {
        return new GeneratorPlanDraftArtifactProductionDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            BatchId = batchId,
            ArtifactId = artifactId,
            QueueItemId = queueItemId,
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
