using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public enum GameRealismMode
{
    AbstractGamey,
    SemiRealistic,
    RealisticWithFictionalAdditions,
    HardRealistic,
    Fantasy,
    Custom
}

public enum GameLoreMode
{
    None,
    OriginalFiction,
    HistoricalInspired,
    RealWorldWithFictionalAdditions,
    Custom
}

public sealed record GameDesignWish
{
    public string WishId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = "optional";
}

public sealed record GameViewModeWish
{
    public string ViewModeId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Required { get; init; }
}

public sealed record GameInteractionWish
{
    public string InteractionId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Required { get; init; }
}

public sealed record GameGenerationPolicy
{
    public IReadOnlyList<string> LlmSeededAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ProgramGeneratedAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> LuaDefinedAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AssetGeneratedAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> HandAuthoredAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeGeneratedLazyAreas { get; init; } = Array.Empty<string>();
}

public sealed record GameScalePolicy
{
    public UnityWorldScale WorldScale { get; init; } = UnityWorldScale.Small;
    public int ImportantNpcBudget { get; init; }
    public int GeneratedPopulationBudget { get; init; }
    public int RegionBudget { get; init; }
    public bool SupportsLazyExpansion { get; init; }
}

public sealed record GamePerformancePolicy
{
    public int TargetFramesPerSecond { get; init; } = 60;
    public int ActiveNpcBudget { get; init; } = 64;
    public int ActiveChunkBudget { get; init; } = 9;
    public bool UseAbstractOffscreenSimulation { get; init; } = true;
}

public sealed record GameDesignBrief
{
    public string BriefId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ShortPitch { get; init; } = string.Empty;
    public string ContentLanguage { get; init; } = ContentLanguageCodes.Russian;
    public string Tone { get; init; } = string.Empty;
    public GameRealismMode RealismMode { get; init; } = GameRealismMode.Custom;
    public GameLoreMode LoreMode { get; init; } = GameLoreMode.Custom;
    public IReadOnlyList<string> LoreFacts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> WorldRules { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GameDesignWish> GameplayWishes { get; init; } = Array.Empty<GameDesignWish>();
    public IReadOnlyList<GameInteractionWish> InteractionWishes { get; init; } = Array.Empty<GameInteractionWish>();
    public IReadOnlyList<GameViewModeWish> ViewModeWishes { get; init; } = Array.Empty<GameViewModeWish>();
    public IReadOnlyList<GameDesignWish> UiWishes { get; init; } = Array.Empty<GameDesignWish>();
    public IReadOnlyList<GameDesignWish> AssetStyleWishes { get; init; } = Array.Empty<GameDesignWish>();
    public IReadOnlyList<GameDesignWish> AudioStyleWishes { get; init; } = Array.Empty<GameDesignWish>();
    public IReadOnlyList<string> ExpectedUnityRuntimeModuleIds { get; init; } = Array.Empty<string>();
    public GameGenerationPolicy GenerationPolicy { get; init; } = new();
    public GameScalePolicy ScalePolicy { get; init; } = new();
    public GamePerformancePolicy PerformancePolicy { get; init; } = new();
}
