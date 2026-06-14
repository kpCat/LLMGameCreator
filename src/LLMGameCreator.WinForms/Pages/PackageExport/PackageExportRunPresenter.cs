using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.PackageExport;

public sealed class PackageExportRunPresenter
{
    public PackageExportRunViewState FromRunResult(GeneratorPlanPackageExportRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var diagnostics = result.Diagnostics
            .Select(PackageExportDiagnosticRow.FromRunDiagnostic)
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new PackageExportRunViewState
        {
            Summary = BuildSummary("Run completed.", result.Status, result.PackageJsonPath, result.MarkdownReport, diagnostics.Count),
            Status = result.Status,
            SourceExamplePath = result.SourceExamplePath,
            ExportFolderPath = result.ExportFolderPath,
            PackageJsonPath = result.PackageJsonPath,
            ApprovalStatus = result.ApprovalArtifacts.ApprovalResult.Status,
            AssemblyStatus = result.AssemblyResult.Status,
            ErrorCount = diagnostics.Count(IsError),
            WarningCount = diagnostics.Count(IsWarning),
            MarkdownExists = !string.IsNullOrWhiteSpace(result.MarkdownReport),
            MarkdownReport = result.MarkdownReport,
            Diagnostics = diagnostics
        };
    }

    public PackageExportRunViewState FromLatestRun(GeneratorPlanPackageExportRunArtifactReadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Exists || result.RunArtifact == null)
        {
            return new PackageExportRunViewState
            {
                Exists = false,
                Summary = "No package export run found.",
                Status = "not_found"
            };
        }

        var snapshot = ReadSnapshot(result.RunArtifact.Json);
        var diagnostics = snapshot.Diagnostics.Count > 0
            ? snapshot.Diagnostics
                .Select(PackageExportDiagnosticRow.FromRunDiagnostic)
                .ToList()
            : result.ValidationResults
                .Select(PackageExportDiagnosticRow.FromValidationResult)
                .ToList();

        diagnostics = diagnostics
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var markdown = ReadMarkdown(result.MarkdownArtifact?.Json);
        var status = FirstNonEmpty(snapshot.Status, ReadMetadataValue(result.RunArtifact.MetadataJson, "status"), result.RunArtifact.ValidationState);
        var packageJsonPath = FirstNonEmpty(snapshot.PackageJsonPath, ReadMetadataValue(result.RunArtifact.MetadataJson, "packageJsonPath"));
        var exportFolderPath = FirstNonEmpty(snapshot.ExportFolderPath, ReadMetadataValue(result.RunArtifact.MetadataJson, "exportFolderPath"));

        return new PackageExportRunViewState
        {
            Summary = BuildLatestSummary(result.RunArtifact.Id, status, result.MarkdownArtifact != null, result.ValidationResults.Count),
            Status = status,
            SourceExamplePath = FirstNonEmpty(snapshot.SourceExamplePath, ReadMetadataValue(result.RunArtifact.MetadataJson, "sourceExamplePath")),
            ExportFolderPath = exportFolderPath,
            PackageJsonPath = packageJsonPath,
            ApprovalStatus = FirstNonEmpty(snapshot.ApprovalStatus, ReadMetadataValue(result.RunArtifact.MetadataJson, "approvalStatus")),
            AssemblyStatus = FirstNonEmpty(snapshot.AssemblyStatus, ReadMetadataValue(result.RunArtifact.MetadataJson, "assemblyStatus")),
            ErrorCount = diagnostics.Count(IsError),
            WarningCount = diagnostics.Count(IsWarning),
            RunArtifactId = result.RunArtifact.Id,
            MarkdownExists = result.MarkdownArtifact != null,
            ValidationResultCount = result.ValidationResults.Count,
            MarkdownReport = markdown,
            Diagnostics = diagnostics
        };
    }

    private static PackageExportRunSnapshot ReadSnapshot(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PackageExportRunSnapshot();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var diagnostics = new List<GeneratorPlanPackageExportRunDiagnostic>();
            if (TryGetProperty(root, "Diagnostics", out var diagnosticArray) && diagnosticArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var diagnostic in diagnosticArray.EnumerateArray())
                {
                    diagnostics.Add(new GeneratorPlanPackageExportRunDiagnostic
                    {
                        Severity = GetString(diagnostic, "Severity"),
                        Code = GetString(diagnostic, "Code"),
                        Target = GetString(diagnostic, "Target"),
                        Message = GetString(diagnostic, "Message")
                    });
                }
            }

            return new PackageExportRunSnapshot
            {
                Status = GetString(root, "Status"),
                SourceExamplePath = GetString(root, "SourceExamplePath"),
                ExportFolderPath = GetString(root, "ExportFolderPath"),
                PackageJsonPath = GetString(root, "PackageJsonPath"),
                ApprovalStatus = GetString(root, "ApprovalStatus"),
                AssemblyStatus = GetString(root, "AssemblyStatus"),
                Diagnostics = diagnostics
            };
        }
        catch (JsonException)
        {
            return new PackageExportRunSnapshot();
        }
    }

    private static string ReadMarkdown(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return GetString(document.RootElement, "Markdown");
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string ReadMetadataValue(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return GetString(document.RootElement, propertyName);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string BuildSummary(string prefix, string status, string packageJsonPath, string markdownReport, int diagnosticCount)
    {
        return string.Join(Environment.NewLine, new[]
        {
            prefix,
            $"Status: {status}",
            $"Package JSON: {packageJsonPath}",
            $"Diagnostics: {diagnosticCount}",
            $"Markdown report: {(!string.IsNullOrWhiteSpace(markdownReport) ? "yes" : "no")}"
        });
    }

    private static string BuildLatestSummary(string artifactId, string status, bool markdownExists, int validationCount)
    {
        return string.Join(Environment.NewLine, new[]
        {
            "Latest package export run loaded.",
            $"Run artifact id: {artifactId}",
            $"Status: {status}",
            $"Markdown exists: {markdownExists}",
            $"Validation results: {validationCount}"
        });
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

        var camel = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        return element.TryGetProperty(camel, out value);
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
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

    private static bool IsError(PackageExportDiagnosticRow row)
    {
        return string.Equals(row.Severity, GeneratorPlanPreviewDiagnosticSeverity.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWarning(PackageExportDiagnosticRow row)
    {
        return string.Equals(row.Severity, GeneratorPlanPreviewDiagnosticSeverity.Warning, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PackageExportRunSnapshot
    {
        public string Status { get; init; } = string.Empty;
        public string SourceExamplePath { get; init; } = string.Empty;
        public string ExportFolderPath { get; init; } = string.Empty;
        public string PackageJsonPath { get; init; } = string.Empty;
        public string ApprovalStatus { get; init; } = string.Empty;
        public string AssemblyStatus { get; init; } = string.Empty;
        public IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPackageExportRunDiagnostic>();
    }
}
