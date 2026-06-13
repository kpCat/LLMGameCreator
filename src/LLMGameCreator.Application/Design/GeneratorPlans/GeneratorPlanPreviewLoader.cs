using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewLoader
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public async Task<GeneratorPlanPreview> LoadAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(sourcePath));
        }

        var fullPath = Path.GetFullPath(sourcePath);
        try
        {
            var rawJson = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(rawJson, DocumentOptions);
            var root = document.RootElement;
            var steps = ReadSteps(root);

            var preview = new GeneratorPlanPreview
            {
                SourcePath = fullPath,
                ExampleId = ReadString(root, "example_id") ?? string.Empty,
                Title = ReadString(root, "title") ?? string.Empty,
                Purpose = ReadString(root, "purpose") ?? string.Empty,
                SourceProfileId = ReadNestedString(root, "source_profile", "id") ?? string.Empty,
                SelectedFeatureBundles = ReadStringArray(root, "selected_feature_bundles"),
                TargetArtifacts = ReadStringArray(root, "target_artifacts"),
                Steps = steps,
                Diagnostics =
                [
                    CreateDiagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Info,
                        GeneratorPlanPreviewDiagnosticCodes.Loaded,
                        "Generator plan example was loaded.",
                        fullPath)
                ]
            };

            return preview with { Summary = BuildSummary(preview, preview.Diagnostics) };
        }
        catch (JsonException ex)
        {
            var diagnostics = new[]
            {
                CreateDiagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanPreviewDiagnosticCodes.InvalidJson,
                    ex.Message,
                    fullPath)
            };

            return new GeneratorPlanPreview
            {
                SourcePath = fullPath,
                Diagnostics = diagnostics,
                Summary = BuildSummary(new GeneratorPlanPreview { SourcePath = fullPath }, diagnostics)
            };
        }
        catch (IOException ex)
        {
            var diagnostics = new[]
            {
                CreateDiagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanPreviewDiagnosticCodes.InvalidJson,
                    ex.Message,
                    fullPath)
            };

            return new GeneratorPlanPreview
            {
                SourcePath = fullPath,
                Diagnostics = diagnostics,
                Summary = BuildSummary(new GeneratorPlanPreview { SourcePath = fullPath }, diagnostics)
            };
        }
    }

    internal static GeneratorPlanPreviewSummary BuildSummary(
        GeneratorPlanPreview preview,
        IReadOnlyList<GeneratorPlanPreviewDiagnostic> diagnostics)
    {
        return new GeneratorPlanPreviewSummary
        {
            StepCount = preview.Steps.Count,
            TargetArtifactCount = preview.TargetArtifacts.Count,
            FeatureBundleCount = preview.SelectedFeatureBundles.Count,
            ErrorCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            WarningCount = diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        };
    }

    internal static GeneratorPlanPreviewDiagnostic CreateDiagnostic(
        string severity,
        string code,
        string message,
        string? path = null,
        string? stepId = null)
    {
        return new GeneratorPlanPreviewDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Path = path,
            StepId = stepId
        };
    }

    private static IReadOnlyList<GeneratorPlanPreviewStep> ReadSteps(JsonElement root)
    {
        if (!root.TryGetProperty("steps", out var stepsElement) ||
            stepsElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<GeneratorPlanPreviewStep>();
        }

        return stepsElement
            .EnumerateArray()
            .Where(step => step.ValueKind == JsonValueKind.Object)
            .Select(step => new GeneratorPlanPreviewStep
            {
                Id = ReadString(step, "id") ?? string.Empty,
                Order = ReadInt32(step, "order"),
                Title = ReadString(step, "title") ?? string.Empty,
                ProducerRole = ReadString(step, "producer_role") ?? string.Empty,
                ContextPackTemplate = ReadString(step, "context_pack_template") ?? string.Empty,
                ExpectedArtifactContract = ReadString(step, "expected_artifact_contract") ?? string.Empty,
                Inputs = ReadStringArray(step, "inputs"),
                ValidationGates = ReadStringArray(step, "validation_gates"),
                OnSuccess = ReadString(step, "on_success") ?? string.Empty,
                OnFailure = ReadString(step, "on_failure") ?? string.Empty
            })
            .OrderBy(step => step.Order)
            .ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ReadNestedString(JsonElement root, string objectPropertyName, string nestedPropertyName)
    {
        if (!root.TryGetProperty(objectPropertyName, out var nested) ||
            nested.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadString(nested, nestedPropertyName);
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToList();
    }

    private static int ReadInt32(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var value))
        {
            return 0;
        }

        return value;
    }
}
