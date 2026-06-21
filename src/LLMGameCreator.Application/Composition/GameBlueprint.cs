using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public enum GameKind
{
    TextRpg,
    MapPanelRpg,
    FantasyOpenWorldRpg,
    RealisticCitySurvival,
    ZombieCitySurvival,
    CrimeSandbox,
    Custom
}

public enum WorldSourceKind
{
    ProceduralPackage,
    ImportedRealMap,
    HandAuthoredMap,
    HybridImportedPlusGenerated,
    LazyInfiniteWorld,
    Custom
}

public enum PresentationKind
{
    Text,
    TopDown2D,
    Isometric2D,
    StrategyMap,
    FirstPerson3D,
    ThirdPerson3D,
    Custom
}

public enum GenerationMode
{
    OfflineReviewed,
    LazyRuntime,
    HybridOfflinePlusLazy,
    HandAuthored
}

public sealed record GameBlueprint
{
    public string BlueprintId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public GameKind GameKind { get; init; } = GameKind.Custom;
    public IReadOnlyList<WorldSourceKind> WorldSources { get; init; } = Array.Empty<WorldSourceKind>();
    public IReadOnlyList<PresentationKind> Presentations { get; init; } = Array.Empty<PresentationKind>();
    public IReadOnlyList<GenerationMode> GenerationModes { get; init; } = Array.Empty<GenerationMode>();
    public IReadOnlyList<string> RequestedCapabilityIds { get; init; } = Array.Empty<string>();
    public string ContentLanguage { get; init; } = ContentLanguageCodes.Russian;
    public string Notes { get; init; } = string.Empty;
}
