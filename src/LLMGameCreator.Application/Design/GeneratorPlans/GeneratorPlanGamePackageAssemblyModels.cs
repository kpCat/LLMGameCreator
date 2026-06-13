using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanApprovedArtifactSet
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SnapshotId { get; init; } = string.Empty;
    public string SourceProductionBatchId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanApprovedArtifact> ApprovedArtifacts { get; init; } = Array.Empty<GeneratorPlanApprovedArtifact>();
}

public sealed record GeneratorPlanApprovedArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
}

public sealed record GeneratorPlanGamePackageAssemblyRequest
{
    public bool RenderMarkdown { get; init; } = true;
    public bool SerializePackageJson { get; init; } = true;
    public string? ExportFolderPath { get; init; }
    public bool ExportPackageJson { get; init; }
}

public sealed record GeneratorPlanGamePackageAssemblyResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanApprovedArtifactSet ApprovedArtifactSet { get; init; } = new();
    public GamePackageDefinition Package { get; init; } = new();
    public string PackageJson { get; init; } = string.Empty;
    public string MarkdownReport { get; init; } = string.Empty;
    public ValidationReport? PackageValidationReport { get; init; }
    public IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyDiagnostic>();
    public GeneratorPlanGamePackageAssemblySummary Summary { get; init; } = new();
    public IReadOnlyList<GeneratorPlanGamePackageAssemblyMapping> Mappings { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyMapping>();
    public string? ExportFolderPath { get; init; }
}

public sealed record GeneratorPlanGamePackageAssemblySummary
{
    public int ApprovedArtifactCount { get; init; }
    public int MappedArtifactCount { get; init; }
    public int UnmappedArtifactCount { get; init; }
    public int MapCount { get; init; }
    public int EntityPrototypeCount { get; init; }
    public int EntityInstanceCount { get; init; }
    public int ItemCount { get; init; }
    public int QuestCount { get; init; }
    public int ValidationErrorCount { get; init; }
    public int ValidationWarningCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanGamePackageAssemblyDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ArtifactId { get; init; }
    public string? ArtifactKind { get; init; }
    public string? Target { get; init; }
}

public sealed record GeneratorPlanGamePackageAssemblyMapping
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public static class GeneratorPlanGamePackageAssemblyStatus
{
    public const string Draft = "draft";
    public const string Ready = "ready";
    public const string ValidPackage = "valid_package";
    public const string InvalidPackage = "invalid_package";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanGamePackageAssemblyValidationState
{
    public const string Valid = "valid";
    public const string Warnings = "warnings";
    public const string Invalid = "invalid";
}
