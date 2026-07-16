using LLMGameCreator.GamePackage;
using LLMGameCreator.Application.RuntimePreview;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class SeededGeneratedProjectVocabulary
{
    public const string SourceSchemaVersion = "seeded_generated_project_source_v1";
    public const string SourceV2SchemaVersion = "seeded_generated_project_source_v2";
    public const string OverlaySchemaVersion = "generated_project_overlay_v1";
    public const string GenerationRelativeRoot = ".llmgc/generation";
    public const string PlanJsonFileName = "generated-game-plan.json";
    public const string PlanMarkdownFileName = "generated-game-plan.md";
    public const string RulePackJsonFileName = "formula-effect-action-rule-pack.json";
    public const string TinyLoopStateJsonFileName = "tiny-runtime-loop-state.json";
    public const string TinyLoopReportMarkdownFileName = "tiny-runtime-loop-report.md";
    public const string GeneratedMvpPackageJsonFileName = "generated-package-mvp.json";
    public const string GeneratedOverlayJsonFileName = "generated-project-overlay.json";
    public const string GeneratedBasePackageJsonFileName = "generated-base-package.json";
    public const string SourceJsonFileName = "seeded-project-source.json";
    public const string SourceRelativePath = GenerationRelativeRoot + "/" + SourceJsonFileName;

    public static readonly IReadOnlyList<string> RequiredSidecarFileNames =
    [
        PlanJsonFileName,
        PlanMarkdownFileName,
        RulePackJsonFileName,
        TinyLoopStateJsonFileName,
        TinyLoopReportMarkdownFileName,
        GeneratedMvpPackageJsonFileName,
        GeneratedOverlayJsonFileName,
        GeneratedBasePackageJsonFileName
    ];
}

public static class SeededGeneratedProjectRequestOrigins
{
    public const string LegacyV1EffectiveOptions = "legacy_v1_effective_options";
    public const string ExplicitV2Request = "explicit_v2_request";
}

public sealed record SeededGeneratedProjectGenerationRequest
{
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string PresetId { get; init; } = string.Empty;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = [];
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = [];
}

public sealed record SeededGeneratedProjectResolvedOptions
{
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string PresetId { get; init; } = string.Empty;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = [];
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
    public string PresetDefinitionSha256 { get; init; } = string.Empty;
    public bool StyleOverridesApplied { get; init; }
    public bool VariantOverridesApplied { get; init; }
}

public sealed record GeneratedProjectCounts
{
    public int Regions { get; init; }
    public int Factions { get; init; }
    public int Actors { get; init; }
    public int ItemsAndResources { get; init; }
    public int Encounters { get; init; }
    public int QuestEvents { get; init; }
}

public sealed record GeneratedProjectTinyLoopFacts
{
    public bool Passed { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int StepCount { get; init; }
    public bool RewardOrCostObserved { get; init; }
    public bool StateChangeObserved { get; init; }
}

public sealed record SeededGeneratedProjectSourceRecord
{
    public string SchemaVersion { get; init; } = SeededGeneratedProjectVocabulary.SourceSchemaVersion;
    public string CreationKind { get; init; } = LLMGameCreator.Application.Projects.GameProjectCreationKinds.SeededGenerated;
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string PresetId { get; init; } = string.Empty;
    public IReadOnlyList<string> StyleHintIds { get; init; } = [];
    public IReadOnlyList<string> VariantIds { get; init; } = [];
    internal SeededGeneratedProjectGenerationRequest GenerationRequest { get; init; } = new();
    internal SeededGeneratedProjectResolvedOptions ResolvedGenerationOptions { get; init; } = new();
    internal string RequestOrigin { get; init; } = SeededGeneratedProjectRequestOrigins.LegacyV1EffectiveOptions;
    public string MechanicsProfileId { get; init; } = string.Empty;
    public string PlanId { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string RulePackId { get; init; } = string.Empty;
    public string RulePackSha256 { get; init; } = string.Empty;
    public string TinyLoopStateSha256 { get; init; } = string.Empty;
    public string GeneratedMvpPackageSha256 { get; init; } = string.Empty;
    public string GeneratedOverlaySha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string Goal142BaselinePackageSha256 { get; init; } = string.Empty;
    public string GeneratedStartMapId { get; init; } = string.Empty;
    public GeneratedProjectCounts Counts { get; init; } = new();
    public GeneratedProjectTinyLoopFacts TinyLoop { get; init; } = new();
    public IReadOnlyDictionary<string, string> SidecarSha256 { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GeneratedProjectRecordFingerprint
{
    public string CollectionPath { get; init; } = string.Empty;
    public string RecordId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedProjectOverlayDocument
{
    public string SchemaVersion { get; init; } = SeededGeneratedProjectVocabulary.OverlaySchemaVersion;
    public string Goal142BaselinePackageSha256 { get; init; } = string.Empty;
    public string GeneratedMvpPackageSha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string BaselineManifestSha256 { get; init; } = string.Empty;
    public string BaselineStartMapId { get; init; } = string.Empty;
    public string GeneratedStartMapId { get; init; } = string.Empty;
    public int BaselineRecordCount { get; init; }
    public int GeneratedRecordCount { get; init; }
    public int AdditiveRecordCount { get; init; }
    public int DeduplicatedRecordCount { get; init; }
    public IReadOnlyList<GeneratedProjectRecordFingerprint> BaselineRecords { get; init; } = [];
    public IReadOnlyList<GeneratedProjectRecordFingerprint> GeneratedRecords { get; init; } = [];
    public IReadOnlyList<string> DeduplicatedRecordKeys { get; init; } = [];
    public bool BaselineDefinitionsPreserved { get; init; }
    public bool GeneratedRecordsAdditive { get; init; }
    public bool GeneratedReferencesValid { get; init; }
}

public sealed record GeneratedProjectOverlayResult
{
    public GeneratedProjectOverlayDocument Document { get; init; } = new();
    public GamePackageDefinition GeneratedBasePackage { get; init; } = new();
    public string OverlayJson { get; init; } = string.Empty;
    public string GeneratedBasePackageJson { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record SeededGeneratedProjectSourceValidationResult
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "ABSENT";
    public string SourcePath { get; init; } = string.Empty;
    public SeededGeneratedProjectSourceRecord? Source { get; init; }
    public GeneratedProjectOverlayDocument? Overlay { get; init; }
    public GamePackageDefinition? GeneratedBasePackage { get; init; }
    public GamePackageDefinition? GeneratedMvpPackage { get; init; }
    public ProceduralGeneratedGamePlan? RegeneratedPlan { get; init; }
    public string RegeneratedPlanJson { get; init; } = string.Empty;
    public SeededGeneratedProjectGenerationRequest? GenerationRequest { get; init; }
    public SeededGeneratedProjectResolvedOptions? ResolvedGenerationOptions { get; init; }
    public string RequestOrigin { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SeededGeneratedProjectArtifactFactoryRequest
{
    public SeededGeneratedProjectGenerationRequest GenerationRequest { get; init; } = new();
    public string MechanicsProfileId { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
}

public sealed record SeededGeneratedProjectArtifactFactoryResult
{
    public SeededGeneratedProjectResolvedOptions ResolvedOptions { get; init; } = new();
    public VisibleGeneratedPlayablePreviewResult Generated { get; init; } = new();
    public GeneratedProjectOverlayResult Overlay { get; init; } = new();
    public SeededGeneratedProjectSourceRecord Source { get; init; } = new();
    public string SourceJson { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> SidecarBytes { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> SidecarSha256 { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}
