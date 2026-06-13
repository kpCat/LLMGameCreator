using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public static class AtlasRegistryValidationPolicy
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";

    public static string ToValidationState(AtlasRegistrySummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ErrorCount > 0)
        {
            return Invalid;
        }

        return summary.WarningCount > 0 ? Warnings : Valid;
    }

    public static IReadOnlyList<AtlasDiagnostic> SelectValidationDiagnostics(IReadOnlyList<AtlasDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .Where(diagnostic => diagnostic.Severity is AtlasDiagnosticSeverity.Error or AtlasDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<AtlasDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            throw new ArgumentException("Artifact id is required.", nameof(artifactId));
        }

        return SelectValidationDiagnostics(diagnostics)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.Path ?? string.Empty, diagnostic.Id ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Path ?? diagnostic.Id ?? artifactId,
                BuildDiagnosticMetadataJson(diagnostic)))
            .ToList();
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            AtlasDiagnosticSeverity.Error => 0,
            AtlasDiagnosticSeverity.Warning => 1,
            AtlasDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }

    private static string BuildDiagnosticMetadataJson(AtlasDiagnostic diagnostic)
    {
        return JsonSerializer.Serialize(new
        {
            path = diagnostic.Path,
            id = diagnostic.Id
        });
    }

    private static string StableId(params string[] parts)
    {
        var text = string.Join("|", parts);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
