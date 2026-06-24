using LLMGameCreator.Application.Design.Semantics;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class ProceduralGameGenerationModes
{
    public const string AuthoredSmallWorld = "authored_small_world";
    public const string SemiProceduralRegions = "semi_procedural_regions";
    public const string FullySeededWorld = "fully_seeded_world";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        AuthoredSmallWorld,
        SemiProceduralRegions,
        FullySeededWorld
    };
}

public sealed record ProceduralGameKernelRequest
{
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = ProceduralGameGenerationModes.AuthoredSmallWorld;
    public SemanticCatalog? SemanticCatalog { get; init; }
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
}

public sealed record ProceduralGameKernelResult
{
    public ProceduralGeneratedGamePlan Plan { get; init; } = new();
    public string Json { get; init; } = string.Empty;
    public string Markdown { get; init; } = string.Empty;
    public IReadOnlyList<ProceduralGameDiagnostic> Diagnostics { get; init; } = Array.Empty<ProceduralGameDiagnostic>();
}

public sealed record ProceduralGameKernelWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string JsonPath { get; init; } = string.Empty;
    public string MarkdownPath { get; init; } = string.Empty;
}

public sealed record ProceduralGeneratedGamePlan
{
    public string SchemaVersion { get; init; } = "1";
    public string PlanId { get; init; } = "generated_game_plan/seeded_procedural_kernel_v1";
    public ProceduralGenerationMetadata Metadata { get; init; } = new();
    public ProceduralGenerationProfile Profile { get; init; } = new();
    public ProceduralWorldPlan World { get; init; } = new();
    public IReadOnlyList<ProceduralFactionSeed> Factions { get; init; } = Array.Empty<ProceduralFactionSeed>();
    public IReadOnlyList<ProceduralActorSeed> ActorSeeds { get; init; } = Array.Empty<ProceduralActorSeed>();
    public IReadOnlyList<ProceduralItemResourceSeed> ItemResourceSeeds { get; init; } = Array.Empty<ProceduralItemResourceSeed>();
    public IReadOnlyList<ProceduralEncounterSeed> EncounterSeeds { get; init; } = Array.Empty<ProceduralEncounterSeed>();
    public IReadOnlyList<ProceduralQuestEventSeed> QuestEventSeeds { get; init; } = Array.Empty<ProceduralQuestEventSeed>();
    public IReadOnlyList<ProceduralFormulaEffectActionPlaceholder> FormulaEffectActionPlaceholders { get; init; } = Array.Empty<ProceduralFormulaEffectActionPlaceholder>();
    public IReadOnlyList<ProceduralGameDiagnostic> Diagnostics { get; init; } = Array.Empty<ProceduralGameDiagnostic>();
    public string MarkdownSummary { get; init; } = string.Empty;
}

public sealed record ProceduralGenerationMetadata
{
    public string KernelVersion { get; init; } = "seeded_procedural_game_kernel_v1";
    public string Seed { get; init; } = string.Empty;
    public string Mode { get; init; } = ProceduralGameGenerationModes.AuthoredSmallWorld;
    public string DeterministicHash { get; init; } = string.Empty;
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record ProceduralGenerationProfile
{
    public IReadOnlyList<string> VariantIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> StyleHintIds { get; init; } = Array.Empty<string>();
}

public sealed record ProceduralWorldPlan
{
    public string WorldId { get; init; } = string.Empty;
    public string TopologyVariantId { get; init; } = "world_topology/region_graph";
    public IReadOnlyList<ProceduralRegionSeed> Regions { get; init; } = Array.Empty<ProceduralRegionSeed>();
    public IReadOnlyList<ProceduralRegionConnection> Connections { get; init; } = Array.Empty<ProceduralRegionConnection>();
}

public sealed record ProceduralRegionSeed
{
    public string RegionId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string MoodHintId { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed record ProceduralRegionConnection
{
    public string ConnectionId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string GateRequirementPlaceholderId { get; init; } = string.Empty;
}

public sealed record ProceduralFactionSeed
{
    public string FactionId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string HomeRegionId { get; init; } = string.Empty;
    public string MotiveHintId { get; init; } = string.Empty;
}

public sealed record ProceduralActorSeed
{
    public string ActorSeedId { get; init; } = string.Empty;
    public string ArchetypeId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string RoleHintId { get; init; } = string.Empty;
}

public sealed record ProceduralItemResourceSeed
{
    public string ItemSeedId { get; init; } = string.Empty;
    public string ResourceKindId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string AffordanceHintId { get; init; } = string.Empty;
}

public sealed record ProceduralEncounterSeed
{
    public string EncounterSeedId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> FactionIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActorSeedIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RewardItemSeedIds { get; init; } = Array.Empty<string>();
    public string ActionPlaceholderId { get; init; } = string.Empty;
}

public sealed record ProceduralQuestEventSeed
{
    public string QuestEventSeedId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string SourceFactionId { get; init; } = string.Empty;
    public string TargetEncounterSeedId { get; init; } = string.Empty;
    public string RequiredItemSeedId { get; init; } = string.Empty;
    public string RewardPlaceholderId { get; init; } = string.Empty;
}

public sealed record ProceduralFormulaEffectActionPlaceholder
{
    public string PlaceholderId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string RequiredNextSlice { get; init; } = "formula_effect_action_registry_foundation";
}

public sealed record ProceduralGameDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
