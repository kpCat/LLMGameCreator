using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public sealed class GameBlueprintPresetProvider
{
    public const string BaselineGeneratedRpgPreview = "baseline_generated_rpg_preview";
    public const string RealisticCitySurvivalImportedMapFuture = "realistic_city_survival_imported_map_future";
    public const string ZombieCitySurvivalImportedMapFuture = "zombie_city_survival_imported_map_future";

    private static readonly IReadOnlyList<GameBlueprint> Presets =
    [
        new()
        {
            BlueprintId = BaselineGeneratedRpgPreview,
            Title = "Baseline generated RPG preview",
            GameKind = GameKind.MapPanelRpg,
            WorldSources = [WorldSourceKind.ProceduralPackage],
            Presentations = [PresentationKind.TopDown2D],
            GenerationModes = [GenerationMode.OfflineReviewed],
            ContentLanguage = ContentLanguageCodes.Russian,
            RequestedCapabilityIds =
            [
                "localization.content_language_policy",
                "generation.strict_llm_artifacts",
                "package.artifact_review",
                "package.assembly",
                "package.activation",
                "world_source.procedural_package",
                "presentation.topdown_2d_runtime_preview",
                "runtime.preview_movement",
                "dialogue.preview_lines",
                "quest.preview_journal",
                "map.generated_marker_placement",
                "content.generated_npcs",
                "content.generated_quests",
                "content.generated_dialogues",
                "content.generated_encounters"
            ],
            Notes = "Current reviewed-generation and Runtime Preview product spine."
        },
        FutureImportedMap(
            RealisticCitySurvivalImportedMapFuture,
            "Realistic city survival imported-map future",
            GameKind.RealisticCitySurvival),
        FutureImportedMap(
            ZombieCitySurvivalImportedMapFuture,
            "Zombie city survival imported-map future",
            GameKind.ZombieCitySurvival)
    ];

    public IReadOnlyList<GameBlueprint> List()
    {
        return Presets;
    }

    public bool TryGet(string? presetId, out GameBlueprint blueprint)
    {
        blueprint = Presets.FirstOrDefault(item =>
            string.Equals(item.BlueprintId, presetId?.Trim(), StringComparison.OrdinalIgnoreCase))!;
        return blueprint is not null;
    }

    private static GameBlueprint FutureImportedMap(string id, string title, GameKind gameKind)
    {
        return new GameBlueprint
        {
            BlueprintId = id,
            Title = title,
            GameKind = gameKind,
            WorldSources = [WorldSourceKind.ImportedRealMap],
            Presentations = [PresentationKind.StrategyMap],
            GenerationModes = [GenerationMode.OfflineReviewed],
            ContentLanguage = ContentLanguageCodes.Russian,
            RequestedCapabilityIds =
            [
                "localization.content_language_policy",
                "world_source.imported_real_map",
                "time.calendar",
                "population.households",
                "schedule.daily_life",
                "event.offscreen_scheduler",
                "quest.procedural_templates",
                "dialogue.semantic_realizer"
            ],
            Notes = "Design intent only. Imported maps and semantic/procedural systems are planned capabilities."
        };
    }
}
