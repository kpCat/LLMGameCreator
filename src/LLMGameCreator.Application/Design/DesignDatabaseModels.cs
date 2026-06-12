namespace LLMGameCreator.Application.Design;

public sealed record DesignDatabaseInfo(
    string DatabasePath,
    int SchemaVersion,
    DateTimeOffset InitializedUtc);

public sealed record DesignKnowledgeItem(
    string Id,
    string Kind,
    string Title,
    string Body,
    string Source,
    double Confidence,
    string Status,
    string MetadataJson,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record DesignKnowledgeRelation(
    string Id,
    string FromId,
    string ToId,
    string RelationKind,
    string MetadataJson);

public sealed record DesignDecision(
    string Id,
    string Question,
    string ChosenAnswer,
    string AlternativesJson,
    string Reason,
    string Status,
    string MetadataJson,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record DesignConstraint(
    string Id,
    string Scope,
    string Rule,
    string Severity,
    string Status,
    string MetadataJson);

public sealed record CapabilityModuleRecord(
    string Id,
    string Category,
    string Title,
    string Purpose,
    string SourceManifestPath,
    string RuntimeTargetsJson,
    string TurnModesJson,
    string CombatModesJson,
    string UiModesJson,
    string WorldScalesJson,
    string MetadataJson,
    DateTimeOffset ImportedUtc);

public sealed record GeneratorModuleRecord(
    string Id,
    string BatchId,
    string Path,
    string Category,
    string CapabilitiesJson,
    string DependenciesJson,
    string RuntimeTargetsJson,
    string TurnModesJson,
    string CombatModesJson,
    string SourceManifestPath,
    string MetadataJson,
    DateTimeOffset ImportedUtc);

public sealed record GeneratorModuleFileRecord(
    string Id,
    string BatchId,
    string RelativePath,
    string FileKind,
    string SourceManifestPath);

public sealed record GeneratorLibraryImportReport(
    string ImportId,
    int ManifestCount,
    int ImportedManifestCount,
    int ModuleCount,
    int CapabilityCount,
    int FileCount,
    IReadOnlyList<GeneratorLibraryImportIssue> Issues);

public sealed record GeneratorLibraryImportIssue(
    string Id,
    string ImportId,
    string Severity,
    string Code,
    string Message,
    string Target,
    string MetadataJson);

public sealed record GeneratorLibraryImportData(
    IReadOnlyList<CapabilityModuleRecord> Capabilities,
    IReadOnlyList<GeneratorModuleRecord> Modules,
    IReadOnlyList<GeneratorModuleFileRecord> Files,
    IReadOnlyList<GeneratorLibraryImportIssue> Issues);

public sealed record GeneratorPlanRecord(
    string Id,
    string Title,
    string Goal,
    string Status,
    string MetadataJson,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record GeneratorPlanStepRecord(
    string Id,
    string PlanId,
    int StepOrder,
    string ModuleId,
    string ConfigJson,
    string DependsOnJson,
    string Status);

public sealed record GeneratedArtifactRecord(
    string Id,
    string Kind,
    string Path,
    string Json,
    string GeneratedBy,
    string ValidationState,
    string MetadataJson);

public sealed record GeneratorValidationResultRecord(
    string Id,
    string ArtifactId,
    string Severity,
    string Code,
    string Message,
    string Target,
    string MetadataJson);

public sealed record PromptContextPackRecord(
    string Id,
    string Purpose,
    string IncludedKnowledgeIdsJson,
    string IncludedModuleIdsJson,
    int TokenBudget,
    string MetadataJson);
