using System.Text.Json;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssemblyValidator
{
    public IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> Validate(
        GeneratorPlanApprovedArtifactSet artifactSet,
        GeneratorPlanGamePackageAssemblyRequest request,
        string packageJson,
        ValidationReport? packageValidationReport,
        IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(artifactSet);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var result = new List<GeneratorPlanGamePackageAssemblyDiagnostic>(diagnostics);

        if (string.IsNullOrWhiteSpace(artifactSet.SnapshotId))
        {
            result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.MissingApprovedArtifactSet,
                "Approved artifact set snapshot id is required.",
                target: "snapshot_id"));
        }

        if (artifactSet.ApprovedArtifacts.Count == 0)
        {
            result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.NoApprovedArtifacts,
                "Approved artifact set must contain at least one approved artifact.",
                target: "approved_artifacts"));
        }

        foreach (var artifact in artifactSet.ApprovedArtifacts)
        {
            if (string.IsNullOrWhiteSpace(artifact.ArtifactKind))
            {
                result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.ApprovedArtifactMissingKind,
                    "Approved artifact kind should be set.",
                    artifact.ArtifactId,
                    artifact.ArtifactKind,
                    "artifact_kind"));
            }

            try
            {
                using var _ = JsonDocument.Parse(string.IsNullOrWhiteSpace(artifact.ContentJson) ? string.Empty : artifact.ContentJson);
            }
            catch (JsonException exception)
            {
                result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.ApprovedArtifactInvalidJson,
                    $"Approved artifact content_json must be valid JSON: {exception.Message}",
                    artifact.ArtifactId,
                    artifact.ArtifactKind,
                    "content_json"));
            }
        }

        if (request.SerializePackageJson && string.IsNullOrWhiteSpace(packageJson))
        {
            result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageSerializationError,
                "Package JSON was requested but is empty.",
                target: "package_json"));
        }

        if (request.ExportPackageJson && string.IsNullOrWhiteSpace(request.ExportFolderPath))
        {
            result.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.ExportPathMissing,
                "Export folder path is required when package export is requested.",
                target: "export_folder_path"));
        }

        if (packageValidationReport != null)
        {
            result.AddRange(packageValidationReport.Issues
                .Where(issue => issue.Severity is ValidationSeverity.Error or ValidationSeverity.Critical or ValidationSeverity.Warning)
                .Select(GeneratorPlanGamePackageAssemblyPolicy.PackageIssueDiagnostic));
        }

        return result
            .OrderBy(diagnostic => GeneratorPlanGamePackageAssemblyPolicy.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
