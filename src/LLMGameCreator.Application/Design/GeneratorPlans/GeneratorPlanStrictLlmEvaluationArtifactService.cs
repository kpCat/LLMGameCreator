using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmEvaluationArtifactService
{
    internal static readonly GeneratedArtifactRecord EmptyArtifact = new(
        string.Empty,
        string.Empty,
        string.Empty,
        "{}",
        string.Empty,
        string.Empty,
        "{}");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanStrictLlmEvaluationArtifactService(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanStrictLlmEvaluationArtifactSaveResult> SaveAsync(
        GeneratorPlanStrictLlmEvaluationResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var evaluationArtifact = BuildEvaluationArtifact(result);
        var markdownArtifact = BuildMarkdownArtifact(result);
        var validationResults = ToValidationResults(evaluationArtifact.Id, result.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(evaluationArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(evaluationArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanStrictLlmEvaluationArtifactSaveResult
        {
            EvaluationArtifact = evaluationArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildEvaluationArtifact(GeneratorPlanStrictLlmEvaluationResult result)
    {
        var json = JsonSerializer.Serialize(result, JsonOptions);
        return new GeneratedArtifactRecord(
            GeneratorPlanStrictLlmEvaluationArtifactIds.EvaluationArtifactId,
            GeneratorPlanStrictLlmEvaluationArtifactIds.EvaluationArtifactKind,
            GeneratorPlanStrictLlmEvaluationArtifactIds.EvaluationArtifactPath,
            json,
            GeneratorPlanStrictLlmEvaluationArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(GeneratorPlanStrictLlmEvaluationResult result)
    {
        if (string.IsNullOrWhiteSpace(result.MarkdownReport))
        {
            return null;
        }

        var json = JsonSerializer.Serialize(new
        {
            result.EvaluatedAtUtc,
            Markdown = result.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            GeneratorPlanStrictLlmEvaluationArtifactIds.MarkdownArtifactId,
            GeneratorPlanStrictLlmEvaluationArtifactIds.MarkdownArtifactKind,
            GeneratorPlanStrictLlmEvaluationArtifactIds.MarkdownArtifactPath,
            json,
            GeneratorPlanStrictLlmEvaluationArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static string BuildMetadataJson(GeneratorPlanStrictLlmEvaluationResult result)
    {
        return JsonSerializer.Serialize(new
        {
            result.EvaluationId,
            result.EvaluatedAtUtc,
            result.Status,
            result.Mode,
            result.SourceCapabilitySelectionId,
            RequestedContractCount = result.RequestedContractIds.Count,
            result.Summary.TotalGenerationRuns,
            result.Summary.ValidArtifactCount,
            result.Summary.FailedCount,
            result.Summary.OverallPassRate,
            ErrorCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        }, JsonOptions);
    }

    internal static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => GeneratorPlanStrictLlmEvaluationMarkdownRenderer.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ContractId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.ContractId, diagnostic.Target, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                string.IsNullOrWhiteSpace(diagnostic.Target) ? diagnostic.ContractId : diagnostic.Target,
                JsonSerializer.Serialize(new
                {
                    diagnostic.ContractId,
                    diagnostic.Target
                }, JsonOptions)))
            .ToList();
    }

    internal static string ToValidationState(IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error))
        {
            return GeneratorPlanDraftArtifactApprovalValidationState.Invalid;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            ? GeneratorPlanDraftArtifactApprovalValidationState.Warnings
            : GeneratorPlanDraftArtifactApprovalValidationState.Valid;
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
