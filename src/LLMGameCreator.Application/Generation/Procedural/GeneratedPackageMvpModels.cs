using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedPackageMvpRequest
{
    public ProceduralGeneratedGamePlan? SourcePlan { get; init; }
    public FormulaEffectActionRulePack? RulePack { get; init; }
    public FormulaEffectActionValidationReport? RulePackValidationReport { get; init; }
    public TinyGeneratedRuntimeLoopResult? TinyLoopResult { get; init; }
}

public sealed record GeneratedPackageMvpResult
{
    public GamePackageDefinition Package { get; init; } = new();
    public GeneratedPackageMvpReport Report { get; init; } = new();
    public GeneratedPackageRuntimeBootstrapReport RuntimeBootstrapReport { get; init; } = new();
    public string PackageJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string RuntimeBootstrapReportJson { get; init; } = string.Empty;
    public string RuntimeBootstrapReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedPackageMvpDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedPackageMvpDiagnostic>();
}

public sealed record GeneratedPackageMvpWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PackageJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string RuntimeBootstrapReportJsonPath { get; init; } = string.Empty;
    public string RuntimeBootstrapReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record GeneratedPackageMvpReport
{
    public string SchemaVersion { get; init; } = "1";
    public GeneratedPackageMvpSourceMetadata Source { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PreProvenancePackageHash { get; init; } = string.Empty;
    public string ProvenanceContentHashMeaning { get; init; } = string.Empty;
    public string StableSummary { get; init; } = string.Empty;
    public bool HasErrors { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<GeneratedPackageMappedRecord> MappedRecords { get; init; } = Array.Empty<GeneratedPackageMappedRecord>();
    public IReadOnlyList<GeneratedPackageMvpValidationIssue> ValidationIssues { get; init; } = Array.Empty<GeneratedPackageMvpValidationIssue>();
    public GeneratedPackageRuntimeBootstrapReport RuntimeBootstrap { get; init; } = new();
    public IReadOnlyList<GeneratedPackageMvpDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedPackageMvpDiagnostic>();
}

public sealed record GeneratedPackageMvpSourceMetadata
{
    public string PlanId { get; init; } = string.Empty;
    public string PlanHash { get; init; } = string.Empty;
    public string RulePackId { get; init; } = string.Empty;
    public string RulePackHash { get; init; } = string.Empty;
    public string TinyLoopStateHash { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
}

public sealed record GeneratedPackageMappedRecord
{
    public string SourceKind { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string PackageKind { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string MappingNote { get; init; } = string.Empty;
}

public sealed record GeneratedPackageMvpValidationIssue
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}

public sealed record GeneratedPackageRuntimeBootstrapReport
{
    public string SchemaVersion { get; init; } = "1";
    public bool ValidationPassed { get; init; }
    public bool InitialStateCreated { get; init; }
    public bool MapRuntimeStarted { get; init; }
    public bool MoveCommandSucceeded { get; init; }
    public bool InteractCommandObserved { get; init; }
    public string StartMapId { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public string PlayerEntityId { get; init; } = string.Empty;
    public string RuntimeSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratedPackageMvpDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedPackageMvpDiagnostic>();
}

public sealed record GeneratedPackageMvpDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
