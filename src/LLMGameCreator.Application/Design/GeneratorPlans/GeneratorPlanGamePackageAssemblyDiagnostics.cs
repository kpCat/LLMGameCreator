using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanGamePackageAssemblyDiagnosticCodes
{
    public const string MissingApprovedArtifactSet = "generator_plan_game_package_assembly.missing_approved_artifact_set";
    public const string NoApprovedArtifacts = "generator_plan_game_package_assembly.no_approved_artifacts";
    public const string ApprovedArtifactInvalidJson = "generator_plan_game_package_assembly.approved_artifact_invalid_json";
    public const string ApprovedArtifactMissingKind = "generator_plan_game_package_assembly.approved_artifact_missing_kind";
    public const string UnmappedArtifactKind = "generator_plan_game_package_assembly.unmapped_artifact_kind";
    public const string PackageValidationError = "generator_plan_game_package_assembly.package_validation_error";
    public const string PackageValidationWarning = "generator_plan_game_package_assembly.package_validation_warning";
    public const string PackageSerializationError = "generator_plan_game_package_assembly.package_serialization_error";
    public const string ExportPathMissing = "generator_plan_game_package_assembly.export_path_missing";
    public const string ExportFailed = "generator_plan_game_package_assembly.export_failed";
}

public static class GeneratorPlanGamePackageAssemblyPolicy
{
    public static GeneratorPlanGamePackageAssemblySummary BuildSummary(
        GeneratorPlanApprovedArtifactSet artifactSet,
        GamePackageDefinition package,
        IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics,
        IReadOnlyList<GeneratorPlanGamePackageAssemblyMapping> mappings,
        ValidationReport? validationReport)
    {
        ArgumentNullException.ThrowIfNull(artifactSet);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(mappings);

        return new GeneratorPlanGamePackageAssemblySummary
        {
            ApprovedArtifactCount = artifactSet.ApprovedArtifacts.Count,
            MappedArtifactCount = mappings.Count(mapping => mapping.Result == GeneratorPlanGamePackageAssemblyMappingResult.Mapped),
            UnmappedArtifactCount = mappings.Count(mapping => mapping.Result == GeneratorPlanGamePackageAssemblyMappingResult.Unmapped),
            MapCount = package.Game.Maps.Count,
            EntityPrototypeCount = package.Game.EntityPrototypes.Count,
            EntityInstanceCount = package.Game.Maps.Sum(map => map.Entities.Count),
            ItemCount = package.Game.Items.Count,
            QuestCount = package.Game.Quests.Count,
            ValidationErrorCount = validationReport?.Issues.Count(issue => issue.Severity is ValidationSeverity.Error or ValidationSeverity.Critical) ?? 0,
            ValidationWarningCount = validationReport?.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning) ?? 0,
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    public static string BuildStatus(GeneratorPlanGamePackageAssemblySummary summary, bool packageValidated)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return GeneratorPlanGamePackageAssemblyStatus.Invalid;
        }

        if (summary.ValidationErrorCount > 0)
        {
            return GeneratorPlanGamePackageAssemblyStatus.InvalidPackage;
        }

        if (packageValidated)
        {
            return GeneratorPlanGamePackageAssemblyStatus.ValidPackage;
        }

        return summary.ApprovedArtifactCount > 0
            ? GeneratorPlanGamePackageAssemblyStatus.Ready
            : GeneratorPlanGamePackageAssemblyStatus.Draft;
    }

    public static string ToValidationState(GeneratorPlanGamePackageAssemblySummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0 || summary.ValidationErrorCount > 0)
        {
            return GeneratorPlanGamePackageAssemblyValidationState.Invalid;
        }

        return summary.WarningCount > 0 || summary.ValidationWarningCount > 0
            ? GeneratorPlanGamePackageAssemblyValidationState.Warnings
            : GeneratorPlanGamePackageAssemblyValidationState.Valid;
    }

    public static IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> SelectValidationDiagnostics(
        IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactKind, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.ArtifactId ?? string.Empty, diagnostic.ArtifactKind ?? string.Empty, diagnostic.Target ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target ?? diagnostic.ArtifactId ?? artifactId,
                JsonSerializer.Serialize(new
                {
                    artifactId = diagnostic.ArtifactId,
                    artifactKind = diagnostic.ArtifactKind,
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

    internal static GeneratorPlanGamePackageAssemblyDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? artifactId = null,
        string? artifactKind = null,
        string? target = null)
    {
        return new GeneratorPlanGamePackageAssemblyDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            ArtifactId = artifactId,
            ArtifactKind = artifactKind,
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

    internal static GeneratorPlanGamePackageAssemblyDiagnostic PackageIssueDiagnostic(ValidationIssue issue)
    {
        var severity = issue.Severity is ValidationSeverity.Error or ValidationSeverity.Critical
            ? GeneratorPlanPreviewDiagnosticSeverity.Error
            : issue.Severity == ValidationSeverity.Warning
                ? GeneratorPlanPreviewDiagnosticSeverity.Warning
                : GeneratorPlanPreviewDiagnosticSeverity.Info;
        var code = severity == GeneratorPlanPreviewDiagnosticSeverity.Error
            ? GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageValidationError
            : GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageValidationWarning;

        return Diagnostic(
            severity,
            code,
            $"{issue.Code}: {issue.Message}",
            target: issue.TargetId ?? issue.TargetPath ?? issue.FilePath ?? issue.Category);
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public static class GeneratorPlanGamePackageAssemblyMappingResult
{
    public const string Mapped = "mapped";
    public const string Unmapped = "unmapped";
}
