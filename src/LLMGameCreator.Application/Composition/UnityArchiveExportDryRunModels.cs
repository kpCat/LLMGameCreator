using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveExportReadiness
{
    ExportableNow,
    ExportableWithWarnings,
    BlockedByFutureModules,
    MissingRequirements,
    Invalid
}

public enum UnityArchiveExportDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record UnityArchiveExportDryRunRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public GameDesignBrief DesignBrief { get; init; } = new();
    public UnityTargetProfile TargetProfile { get; init; } = new();
    public UnityGameArchiveManifest ArchiveManifest { get; init; } = new();
    public IReadOnlyList<UnityRuntimeModuleContract> RuntimeModules { get; init; }
        = Array.Empty<UnityRuntimeModuleContract>();
}

public sealed record UnityArchivePlannedFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
}

public sealed record UnityArchiveExportDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelatedId { get; init; } = string.Empty;
}

public sealed record UnityArchiveExportPlan
{
    public string SchemaVersion { get; init; } = "1";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveExportReadiness Readiness { get; init; }
    public string DesignBriefId { get; init; } = string.Empty;
    public string TargetProfileId { get; init; } = string.Empty;
    public string ArchiveGameId { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<UnityArchivePlannedFile> PlannedFiles { get; init; } = Array.Empty<UnityArchivePlannedFile>();
    public IReadOnlyList<UnityArchiveExportDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveExportDiagnostic>();
}

public sealed record UnityArchiveExportValidationReport
{
    public string SchemaVersion { get; init; } = "1";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveExportReadiness Readiness { get; init; }
    public IReadOnlyList<UnityArchiveExportDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveExportDiagnostic>();
}

public sealed record UnityArchiveExportDryRunResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PlanJsonPath { get; init; } = string.Empty;
    public string PlanMarkdownPath { get; init; } = string.Empty;
    public string ArchiveManifestJsonPath { get; init; } = string.Empty;
    public string ValidationReportJsonPath { get; init; } = string.Empty;
    public UnityArchiveExportPlan Plan { get; init; } = new();
}

public static class UnityArchiveExportDiagnosticCodes
{
    public const string ContractDiagnostic = "unity.export.contract_diagnostic";
    public const string MissingRequiredRuntimeModule = "unity.export.missing_required_runtime_module";
    public const string MissingBriefRuntimeModule = "unity.export.missing_brief_runtime_module";
    public const string FutureRuntimeModule = "unity.export.future_runtime_module";
    public const string UnsafePlannedPath = "unity.export.unsafe_planned_path";
}
