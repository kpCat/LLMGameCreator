using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanPreviewDiagnosticSeverity
{
    public const string Info = "info";
    public const string Warning = "warning";
    public const string Error = "error";
}

public static class GeneratorPlanPreviewDiagnosticCodes
{
    public const string InvalidJson = "generator_plan_preview.invalid_json";
    public const string MissingExampleId = "generator_plan_preview.missing_example_id";
    public const string MissingTitle = "generator_plan_preview.missing_title";
    public const string MissingSourceProfile = "generator_plan_preview.missing_source_profile";
    public const string NoSteps = "generator_plan_preview.no_steps";
    public const string StepMissingId = "generator_plan_preview.step_missing_id";
    public const string StepDuplicateId = "generator_plan_preview.step_duplicate_id";
    public const string StepMissingExpectedArtifactContract = "generator_plan_preview.step_missing_expected_artifact_contract";
    public const string StepMissingValidationGates = "generator_plan_preview.step_missing_validation_gates";
    public const string StepOrderDuplicate = "generator_plan_preview.step_order_duplicate";
    public const string TargetArtifactsEmpty = "generator_plan_preview.target_artifacts_empty";
    public const string SelectedFeatureBundlesEmpty = "generator_plan_preview.selected_feature_bundles_empty";
    public const string Loaded = "generator_plan_preview.loaded";
}

public static class GeneratorPlanPreviewValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";

    public static string FromSummary(GeneratorPlanPreviewSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return Invalid;
        }

        return summary.WarningCount > 0 ? Warnings : Valid;
    }
}

public static class GeneratorPlanPreviewValidationPolicy
{
    public static IReadOnlyList<GeneratorPlanPreviewDiagnostic> SelectValidationDiagnostics(IReadOnlyList<GeneratorPlanPreviewDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.StepId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanPreviewDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.Path ?? string.Empty, diagnostic.StepId ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Path ?? diagnostic.StepId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    path = diagnostic.Path,
                    stepId = diagnostic.StepId
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

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
