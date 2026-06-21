using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public sealed class GameDesignBriefPresetProvider
{
    public const string TopDownGeneratedRpg = "topdown_generated_rpg";

    private static readonly IReadOnlyList<GameDesignBrief> Presets =
    [
        new()
        {
            BriefId = TopDownGeneratedRpg,
            Title = "Top-down generated RPG",
            ShortPitch = "A reviewed, data-driven RPG assembled for a generic Unity player.",
            ContentLanguage = ContentLanguageCodes.Russian,
            Tone = "adventure",
            RealismMode = GameRealismMode.SemiRealistic,
            LoreMode = GameLoreMode.OriginalFiction,
            LoreFacts = ["The frontier settlements depend on old trade roads."],
            WorldRules = ["Important authored characters keep stable identities."],
            GameplayWishes = [new GameDesignWish { WishId = "questing", Description = "Reviewed quests and encounters", Priority = "required" }],
            InteractionWishes = [new GameInteractionWish { InteractionId = "talk", Description = "Talk to persistent characters", Required = true }],
            ViewModeWishes = [new GameViewModeWish { ViewModeId = "top_down_character", Description = "Top-down character view", Required = true }],
            UiWishes = [new GameDesignWish { WishId = "quest_journal", Description = "Data-bound quest journal" }],
            AssetStyleWishes = [new GameDesignWish { WishId = "painted_2_5d", Description = "Painted 2.5D sprites and environments" }],
            AudioStyleWishes = [new GameDesignWish { WishId = "short_sfx", Description = "Short interaction sound effects" }],
            ExpectedUnityRuntimeModuleIds =
            [
                "unity.core.archive_loader",
                "unity.ui.dynamic_layout",
                "unity.ui.data_binding",
                "unity.world.topdown_map",
                "unity.gameplay.dialogue",
                "unity.gameplay.quest_journal"
            ],
            GenerationPolicy = new GameGenerationPolicy
            {
                LlmSeededAreas = ["world_lore", "rare_quest_arcs"],
                ProgramGeneratedAreas = ["routine_encounters", "loot_placement"],
                LuaDefinedAreas = ["quest_templates", "item_families"],
                AssetGeneratedAreas = ["character_portraits"],
                HandAuthoredAreas = ["main_story_gate"],
                RuntimeGeneratedLazyAreas = ["ambient_population"]
            },
            ScalePolicy = new GameScalePolicy
            {
                WorldScale = UnityWorldScale.Medium,
                ImportantNpcBudget = 32,
                GeneratedPopulationBudget = 500,
                RegionBudget = 12,
                SupportsLazyExpansion = true
            },
            PerformancePolicy = new GamePerformancePolicy
            {
                TargetFramesPerSecond = 60,
                ActiveNpcBudget = 48,
                ActiveChunkBudget = 9,
                UseAbstractOffscreenSimulation = true
            }
        }
    ];

    public IReadOnlyList<GameDesignBrief> List()
    {
        return Presets;
    }

    public bool TryGet(string? presetId, out GameDesignBrief brief)
    {
        brief = Presets.FirstOrDefault(item =>
            string.Equals(item.BriefId, presetId?.Trim(), StringComparison.OrdinalIgnoreCase))!;
        return brief is not null;
    }
}
