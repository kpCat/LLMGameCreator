using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveMaterializationReadiness
{
    MaterializedPlayableContract,
    MaterializedMetadataOnly,
    MaterializedWithWarnings,
    Blocked,
    Invalid
}

public sealed record UnityArchiveMaterializationRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public GameDesignBrief DesignBrief { get; init; } = new();
    public UnityTargetProfile TargetProfile { get; init; } = new();
    public UnityGameArchiveManifest ArchiveManifest { get; init; } = new();
    public IReadOnlyList<UnityRuntimeModuleContract> RuntimeModules { get; init; }
        = Array.Empty<UnityRuntimeModuleContract>();
    public string CompositionReportMarkdown { get; init; } = string.Empty;
    public GamePackageDefinition? GamePackage { get; init; }
    public bool CreateZip { get; init; }
}

public sealed record UnityArchiveMaterializedFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
}

public sealed record UnityArchiveMaterializationDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelatedId { get; init; } = string.Empty;
}

public sealed record UnityArchiveMaterializationValidationReport
{
    public string SchemaVersion { get; init; } = "1";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveMaterializationReadiness Readiness { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveExportReadiness DryRunReadiness { get; init; }
    public IReadOnlyList<UnityArchiveMaterializationDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveMaterializationDiagnostic>();
    public IReadOnlyList<UnityArchiveMaterializedFile> MaterializedFiles { get; init; }
        = Array.Empty<UnityArchiveMaterializedFile>();
}

public sealed record UnityArchiveRuntimeModulesIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityRuntimeModuleContract> Modules { get; init; }
        = Array.Empty<UnityRuntimeModuleContract>();
}

public sealed record UnityArchiveUiLayoutsIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityUiLayoutContract> Layouts { get; init; }
        = Array.Empty<UnityUiLayoutContract>();
}

public sealed record UnityArchiveAssetRequestsIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityAssetGenerationRequest> Requests { get; init; }
        = Array.Empty<UnityAssetGenerationRequest>();
}

public sealed record UnityArchiveAudioRequestsIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityAudioGenerationRequest> Requests { get; init; }
        = Array.Empty<UnityAudioGenerationRequest>();
}

public sealed record UnityArchiveLocalizationIndex
{
    public string SchemaVersion { get; init; } = "1";
    public string ContentLanguage { get; init; } = string.Empty;
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveLuaModulesIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<string> ModuleIds { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveMaterializationResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ValidationReportPath { get; init; } = string.Empty;
    public string? ZipFilePath { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveMaterializationReadiness Readiness { get; init; }
    public IReadOnlyList<UnityArchiveMaterializedFile> MaterializedFiles { get; init; }
        = Array.Empty<UnityArchiveMaterializedFile>();
    public IReadOnlyList<UnityArchiveMaterializationDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveMaterializationDiagnostic>();
    public UnityArchiveExportDryRunResult DryRunResult { get; init; } = new();
}

public static class UnityArchiveMaterializationDiagnosticCodes
{
    public const string DryRunDiagnostic = "unity.materialization.dry_run_diagnostic";
    public const string FutureModulesMetadataOnly = "unity.materialization.future_modules_metadata_only";
    public const string MaterializationBlocked = "unity.materialization.blocked";
    public const string ZipNotImplemented = "unity.materialization.zip_not_implemented";
}
