using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanCapabilitySelectionArtifactService
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

    public GeneratorPlanCapabilitySelectionArtifactService(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanCapabilitySelectionArtifactSaveResult> SaveAsync(
        GeneratorPlanCapabilitySelectionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var artifact = BuildArtifact(result);
        var validationResults = ToValidationResults(artifact.Id, result.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(artifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(artifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanCapabilitySelectionArtifactSaveResult
        {
            SelectionArtifact = artifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildArtifact(GeneratorPlanCapabilitySelectionResult result)
    {
        var json = JsonSerializer.Serialize(result.Selection, JsonOptions);
        return new GeneratedArtifactRecord(
            GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactId,
            GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactKind,
            GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactPath,
            json,
            GeneratorPlanCapabilitySelectionArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static string BuildMetadataJson(GeneratorPlanCapabilitySelectionResult result)
    {
        return JsonSerializer.Serialize(new
        {
            status = result.Status,
            selectionId = result.Selection.SelectionId,
            title = result.Selection.Title,
            selectedVariantIds = result.Selection.SelectedVariantIds,
            selectedFeatureBundleIds = result.Selection.SelectedFeatureBundleIds,
            selectedRuntimeTargets = result.Selection.SelectedRuntimeTargets,
            resolvedCapabilityCount = result.Selection.ResolvedCapabilityIds.Count,
            resolvedArtifactContractCount = result.Selection.ResolvedArtifactContracts.Count,
            resolvedValidatorCount = result.Selection.ResolvedValidators.Count,
            resolvedRuntimeTargetCount = result.Selection.ResolvedRuntimeTargets.Count,
            errorCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            warningCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        }, JsonOptions);
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => GeneratorPlanPreviewValidationPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                string.IsNullOrWhiteSpace(diagnostic.Target) ? artifactId : diagnostic.Target,
                JsonSerializer.Serialize(new { diagnostic.Target }, JsonOptions)))
            .ToList();
    }

    private static string ToValidationState(IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error))
        {
            return GeneratorPlanGamePackageAssemblyValidationState.Invalid;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            ? GeneratorPlanGamePackageAssemblyValidationState.Warnings
            : GeneratorPlanGamePackageAssemblyValidationState.Valid;
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
