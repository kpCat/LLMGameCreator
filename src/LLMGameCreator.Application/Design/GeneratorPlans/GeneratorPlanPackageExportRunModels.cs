using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanPackageExportRunRequest
{
    public string SourceExamplePath { get; init; } = string.Empty;
    public string ExportFolderPath { get; init; } = string.Empty;
    public bool AutoApproveValidArtifacts { get; init; } = true;
    public bool RenderMarkdown { get; init; } = true;
    public bool SaveArtifacts { get; init; } = true;
    public GeneratorPlanDraftArtifactApprovalRequest ApprovalRequest { get; init; } = new() { AutoApproveValidArtifacts = true };
    public GeneratorPlanGamePackageAssemblyRequest AssemblyRequest { get; init; } = new() { ExportPackageJson = true, SerializePackageJson = true };
}

public sealed record GeneratorPlanPackageExportRunResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public string SourceExamplePath { get; init; } = string.Empty;
    public string ExportFolderPath { get; init; } = string.Empty;
    public string PackageJsonPath { get; init; } = string.Empty;
    public GeneratorPlanDraftArtifactApprovalArtifactResult ApprovalArtifacts { get; init; } = new();
    public GeneratorPlanGamePackageAssemblyResult AssemblyResult { get; init; } = new();
    public GeneratorPlanGamePackageAssemblyArtifactResult? AssemblyArtifacts { get; init; }
    public IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPackageExportRunDiagnostic>();
    public string MarkdownReport { get; init; } = string.Empty;
}

public sealed record GeneratorPlanPackageExportRunDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Target { get; init; }
}

public sealed record GeneratorPlanPackageExportRunArtifactSaveResult
{
    public GeneratedArtifactRecord RunArtifact { get; init; } = GeneratorPlanPackageExportRunArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanPackageExportRunArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? RunArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanPackageExportRunStatus
{
    public const string Succeeded = "succeeded";
    public const string SucceededWithWarnings = "succeeded_with_warnings";
    public const string Failed = "failed";
}

public static class GeneratorPlanPackageExportRunDiagnosticCodes
{
    public const string MissingSourceExamplePath = "generator_plan_package_export_run.missing_source_example_path";
    public const string MissingExportFolderPath = "generator_plan_package_export_run.missing_export_folder_path";
    public const string SourceExampleNotFound = "generator_plan_package_export_run.source_example_not_found";
    public const string ApprovalFailed = "generator_plan_package_export_run.approval_failed";
    public const string AssemblyFailed = "generator_plan_package_export_run.assembly_failed";
    public const string PackageJsonMissingAfterExport = "generator_plan_package_export_run.package_json_missing_after_export";
    public const string ApprovalDiagnostic = "generator_plan_package_export_run.approval_diagnostic";
    public const string AssemblyDiagnostic = "generator_plan_package_export_run.assembly_diagnostic";
}

public static class GeneratorPlanPackageExportRunArtifactIds
{
    public const string GeneratedBy = "generator_plan_package_export_run";
    public const string RunArtifactId = "artifact/generator_plan_package_export_run/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_package_export_run_markdown/latest";
    public const string RunArtifactKind = "generator_plan.package_export_run";
    public const string MarkdownArtifactKind = "generator_plan.package_export_run_markdown_report";
    public const string RunArtifactPath = ".llmgc/generator-plans/generator_plan_package_export_run_snapshot.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_package_export_run_report.md";
}
