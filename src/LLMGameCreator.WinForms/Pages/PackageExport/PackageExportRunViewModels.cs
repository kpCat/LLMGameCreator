using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.PackageExport;

public sealed record PackageExportRunViewState
{
    public bool Exists { get; init; } = true;
    public string Summary { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string SourceExamplePath { get; init; } = string.Empty;
    public string ExportFolderPath { get; init; } = string.Empty;
    public string PackageJsonPath { get; init; } = string.Empty;
    public string ApprovalStatus { get; init; } = string.Empty;
    public string AssemblyStatus { get; init; } = string.Empty;
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public string RunArtifactId { get; init; } = string.Empty;
    public bool MarkdownExists { get; init; }
    public int ValidationResultCount { get; init; }
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<PackageExportDiagnosticRow> Diagnostics { get; init; } = Array.Empty<PackageExportDiagnosticRow>();

    public bool CanOpenExportFolder => Directory.Exists(ExportFolderPath);
    public bool CanOpenPackageJson => File.Exists(PackageJsonPath);
    public bool CanCopyPackagePath => !string.IsNullOrWhiteSpace(PackageJsonPath);
    public bool CanCopyMarkdownReport => !string.IsNullOrWhiteSpace(MarkdownReport);
}

public sealed record PackageExportDiagnosticRow
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static PackageExportDiagnosticRow FromRunDiagnostic(GeneratorPlanPackageExportRunDiagnostic diagnostic)
    {
        return new PackageExportDiagnosticRow
        {
            Severity = diagnostic.Severity,
            Code = diagnostic.Code,
            Target = diagnostic.Target ?? string.Empty,
            Message = diagnostic.Message
        };
    }

    public static PackageExportDiagnosticRow FromValidationResult(GeneratedArtifactValidationResultRecord result)
    {
        return new PackageExportDiagnosticRow
        {
            Severity = result.Severity,
            Code = result.Code,
            Target = result.Target,
            Message = result.Message
        };
    }
}

public sealed record PackageExportTemplateViewModel
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
