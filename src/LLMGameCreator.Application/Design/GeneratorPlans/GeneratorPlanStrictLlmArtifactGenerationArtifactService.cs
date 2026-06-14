using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactGenerationArtifactService
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

    public GeneratorPlanStrictLlmArtifactGenerationArtifactService(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanStrictLlmArtifactGenerationArtifactSaveResult> SaveAsync(
        GeneratorPlanStrictLlmArtifactGenerationResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var artifact = BuildArtifact(result);
        var validationResults = ToValidationResults(artifact.Id, result.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(artifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(artifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanStrictLlmArtifactGenerationArtifactSaveResult
        {
            GenerationArtifact = artifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildArtifact(GeneratorPlanStrictLlmArtifactGenerationResult result)
    {
        var json = JsonSerializer.Serialize(new
        {
            generatedAtUtc = result.GeneratedAtUtc,
            ok = result.Ok,
            status = result.Status,
            sourceCapabilitySelectionId = result.SourceCapabilitySelectionId,
            requestedContractIds = result.RequestedContractIds,
            generatedArtifacts = result.Artifacts.Select(artifact => new
            {
                artifact.ArtifactId,
                artifact.ArtifactKind,
                artifact.ExpectedArtifactContract,
                artifact.Valid,
                artifact.Repaired,
                artifact.RequiresHumanApproval
            }).ToList(),
            attempts = result.Attempts,
            diagnostics = result.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GenerationArtifactId,
            GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GenerationArtifactKind,
            GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GenerationArtifactPath,
            json,
            GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static string BuildMetadataJson(GeneratorPlanStrictLlmArtifactGenerationResult result)
    {
        return JsonSerializer.Serialize(new
        {
            result.GeneratedAtUtc,
            result.Status,
            result.SourceCapabilitySelectionId,
            RequestedContractCount = result.RequestedContractIds.Count,
            ArtifactCount = result.Artifacts.Count,
            ValidArtifactCount = result.Artifacts.Count(artifact => artifact.Valid),
            RepairedArtifactCount = result.Artifacts.Count(artifact => artifact.Repaired),
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
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
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

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            GeneratorPlanPreviewDiagnosticSeverity.Error => 0,
            GeneratorPlanPreviewDiagnosticSeverity.Warning => 1,
            GeneratorPlanPreviewDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
