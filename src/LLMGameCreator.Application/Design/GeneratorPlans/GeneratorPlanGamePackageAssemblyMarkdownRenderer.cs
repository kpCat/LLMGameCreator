using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssemblyMarkdownRenderer
{
    private const int JsonPreviewMaxLength = 3000;

    public string Render(GeneratorPlanGamePackageAssemblyResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("# GamePackage Assembly");
        builder.AppendLine();
        builder.AppendLine($"- Status: **{Cell(result.Status)}**");
        builder.AppendLine($"- Package ID: {Cell(result.Package.Manifest.PackageId)}");
        builder.AppendLine($"- Title: {Cell(result.Package.Manifest.Title)}");
        builder.AppendLine($"- Approved artifacts: {result.Summary.ApprovedArtifactCount}");
        builder.AppendLine($"- Mapped artifacts: {result.Summary.MappedArtifactCount}");
        builder.AppendLine($"- Unmapped artifacts: {result.Summary.UnmappedArtifactCount}");
        builder.AppendLine($"- Maps: {result.Summary.MapCount}");
        builder.AppendLine($"- Entity prototypes: {result.Summary.EntityPrototypeCount}");
        builder.AppendLine($"- Items: {result.Summary.ItemCount}");
        builder.AppendLine($"- Quests: {result.Summary.QuestCount}");
        builder.AppendLine($"- Validation errors: {result.Summary.ValidationErrorCount}");
        builder.AppendLine($"- Validation warnings: {result.Summary.ValidationWarningCount}");
        builder.AppendLine();

        AppendArtifactMapping(builder, result.Mappings);
        AppendPackageSummary(builder, result.Summary);
        AppendValidation(builder, result.Diagnostics);
        AppendPackageJsonPreview(builder, result.PackageJson);
        return builder.ToString();
    }

    private static void AppendArtifactMapping(StringBuilder builder, IReadOnlyList<GeneratorPlanGamePackageAssemblyMapping> mappings)
    {
        builder.AppendLine("## Artifact Mapping");
        builder.AppendLine();

        if (mappings.Count == 0)
        {
            builder.AppendLine("_No artifact mappings were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Artifact ID | Kind | Contract | Result | Target |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var mapping in mappings.OrderBy(mapping => mapping.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(mapping.ArtifactId)} | {Cell(mapping.ArtifactKind)} | {Cell(mapping.ExpectedArtifactContract)} | {Cell(mapping.Result)} | {Cell(mapping.Target)} |");
        }

        builder.AppendLine();
    }

    private static void AppendPackageSummary(StringBuilder builder, GeneratorPlanGamePackageAssemblySummary summary)
    {
        builder.AppendLine("## Package Summary");
        builder.AppendLine();
        builder.AppendLine("| Area | Count |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| Maps | {summary.MapCount} |");
        builder.AppendLine($"| Entity prototypes | {summary.EntityPrototypeCount} |");
        builder.AppendLine($"| Entity instances | {summary.EntityInstanceCount} |");
        builder.AppendLine($"| Items | {summary.ItemCount} |");
        builder.AppendLine($"| Quests | {summary.QuestCount} |");
        builder.AppendLine();
    }

    private static void AppendValidation(StringBuilder builder, IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        builder.AppendLine("## Validation");
        builder.AppendLine();

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("_No diagnostics were reported._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Code | Target | Message |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var diagnostic in diagnostics
                     .OrderBy(diagnostic => GeneratorPlanGamePackageAssemblyPolicy.SeverityOrder(diagnostic.Severity))
                     .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.ArtifactId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {Cell(diagnostic.Severity)} | {Cell(diagnostic.Code)} | {Cell(diagnostic.Target ?? diagnostic.ArtifactId)} | {Cell(diagnostic.Message)} |");
        }

        builder.AppendLine();
    }

    private static void AppendPackageJsonPreview(StringBuilder builder, string packageJson)
    {
        builder.AppendLine("## Package JSON Preview");
        builder.AppendLine();

        if (string.IsNullOrWhiteSpace(packageJson))
        {
            builder.AppendLine("_No package JSON was serialized._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("```json");
        builder.AppendLine(Truncate(packageJson));
        builder.AppendLine("```");
        builder.AppendLine();
    }

    private static string Truncate(string value)
    {
        if (value.Length <= JsonPreviewMaxLength)
        {
            return value;
        }

        return value[..JsonPreviewMaxLength] + "\n...";
    }

    private static string Cell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal)
            .Trim();
    }
}
