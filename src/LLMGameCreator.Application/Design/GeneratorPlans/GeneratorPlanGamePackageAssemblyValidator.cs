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

            JsonDocument? artifactDocument = null;
            try
            {
                artifactDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(artifact.ContentJson) ? string.Empty : artifact.ContentJson);
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

            using (artifactDocument)
            {
                if (artifactDocument != null)
                {
                    ValidateArtifactKind(artifact, artifactDocument.RootElement, result);
                    ValidateGeneratedIds(artifact, artifactDocument.RootElement, result);
                }
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
        else if (!string.IsNullOrWhiteSpace(packageJson))
        {
            ValidateGeneratedPackageJson(artifactSet, packageJson, result);
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

    private static void ValidateArtifactKind(
        GeneratorPlanApprovedArtifact artifact,
        JsonElement root,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        if (!TryGetProperty(root, "artifact_kind", out var artifactKind)
            || artifactKind.ValueKind != JsonValueKind.String)
        {
            return;
        }

        var actual = artifactKind.GetString() ?? string.Empty;
        var expected = string.IsNullOrWhiteSpace(artifact.ExpectedArtifactContract)
            ? artifact.ArtifactKind
            : artifact.ExpectedArtifactContract;
        if (!string.IsNullOrWhiteSpace(expected) && !string.Equals(actual, expected, StringComparison.Ordinal))
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.ArtifactKindMismatch,
                $"Artifact content artifact_kind '{actual}' must match expected contract '{expected}'.",
                artifact.ArtifactId,
                artifact.ArtifactKind,
                "artifact_kind"));
        }
    }

    private static void ValidateGeneratedIds(
        GeneratorPlanApprovedArtifact artifact,
        JsonElement root,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        switch (artifact.ArtifactKind)
        {
            case "scene_pack_v1":
                ValidateUniqueArrayIds(artifact, root, "scenes", GeneratorPlanGamePackageAssemblyDiagnosticCodes.DuplicateGeneratedSceneId, diagnostics);
                break;
            case "quest_pack_v1":
                ValidateUniqueArrayIds(artifact, root, "quests", GeneratorPlanGamePackageAssemblyDiagnosticCodes.DuplicateGeneratedQuestId, diagnostics);
                break;
            case "mechanics_pack_v1":
                ValidateUniqueArrayIds(artifact, root, "mechanics", GeneratorPlanGamePackageAssemblyDiagnosticCodes.DuplicateGeneratedMechanicId, diagnostics);
                break;
        }
    }

    private static void ValidateUniqueArrayIds(
        GeneratorPlanApprovedArtifact artifact,
        JsonElement root,
        string arrayPropertyName,
        string diagnosticCode,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        if (!TryGetProperty(root, arrayPropertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in array.EnumerateArray())
        {
            var id = GetString(item, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (!seen.Add(id.Trim()))
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    diagnosticCode,
                    $"Duplicate generated id '{id}' in {arrayPropertyName}.",
                    artifact.ArtifactId,
                    artifact.ArtifactKind,
                    arrayPropertyName));
            }
        }
    }

    private static void ValidateGeneratedPackageJson(
        GeneratorPlanApprovedArtifactSet artifactSet,
        string packageJson,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(packageJson);
        }
        catch (JsonException)
        {
            return;
        }

        using (document)
        {
            var root = document.RootElement;
            var hasProfile = artifactSet.ApprovedArtifacts.Any(artifact => string.Equals(artifact.ArtifactKind, "game_profile_v1", StringComparison.OrdinalIgnoreCase));
            if (hasProfile
                && (!TryGetProperty(root, "manifest", out var manifest)
                    || string.IsNullOrWhiteSpace(GetString(manifest, "title"))))
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.GeneratedPackageTitleMissing,
                    "Package title is required after applying game_profile_v1.",
                    target: "manifest.title"));
            }

            if (!TryGetProperty(root, "generatedContent", out var generatedContent)
                || generatedContent.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.GeneratedProvenanceMissing,
                    "generatedContent section with provenance is required for package assembly.",
                    target: "generatedContent"));
                return;
            }

            ValidateProvenance(artifactSet, generatedContent, diagnostics);
            ValidatePreservedRawJson(generatedContent, diagnostics);
        }
    }

    private static void ValidateProvenance(
        GeneratorPlanApprovedArtifactSet artifactSet,
        JsonElement generatedContent,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        if (!TryGetProperty(generatedContent, "appliedArtifacts", out var appliedArtifacts)
            || appliedArtifacts.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.GeneratedProvenanceMissing,
                "generatedContent.appliedArtifacts is required.",
                target: "generatedContent.appliedArtifacts"));
            return;
        }

        var provenanceByArtifactId = appliedArtifacts.EnumerateArray()
            .Select(item => GetString(item, "artifactId"))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var artifact in artifactSet.ApprovedArtifacts)
        {
            if (!provenanceByArtifactId.Contains(artifact.ArtifactId))
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.GeneratedProvenanceMissing,
                    "Applied artifact provenance is missing.",
                    artifact.ArtifactId,
                    artifact.ArtifactKind,
                    "generatedContent.appliedArtifacts"));
            }
        }
    }

    private static void ValidatePreservedRawJson(
        JsonElement generatedContent,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        if (!TryGetProperty(generatedContent, "preservedArtifacts", out var preservedArtifacts)
            || preservedArtifacts.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var preserved in preservedArtifacts.EnumerateArray())
        {
            var rawJson = GetString(preserved, "rawJson");
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                continue;
            }

            try
            {
                using var _ = JsonDocument.Parse(rawJson);
            }
            catch (JsonException exception)
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.PreservedArtifactJsonInvalid,
                    $"Preserved raw JSON is invalid: {exception.Message}",
                    GetString(preserved, "artifactId"),
                    GetString(preserved, "artifactKind"),
                    "generatedContent.preservedArtifacts.rawJson"));
            }
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        var pascal = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
        return element.TryGetProperty(pascal, out value);
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }
}
