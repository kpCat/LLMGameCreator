namespace LLMGameCreator.Application.Composition;

public sealed class GeneratorCatalog
{
    private readonly IReadOnlyDictionary<string, GeneratorModuleManifest> _byId;

    public GeneratorCatalog(IEnumerable<GeneratorModuleManifest> manifests)
    {
        ArgumentNullException.ThrowIfNull(manifests);

        Manifests = manifests.ToList();
        DuplicateIds = Manifests
            .Where(manifest => !string.IsNullOrWhiteSpace(manifest.GeneratorId))
            .GroupBy(manifest => manifest.GeneratorId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
        _byId = Manifests
            .Where(manifest => !string.IsNullOrWhiteSpace(manifest.GeneratorId))
            .GroupBy(manifest => manifest.GeneratorId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<GeneratorModuleManifest> Manifests { get; }
    public IReadOnlyList<string> DuplicateIds { get; }
    public IReadOnlyList<GeneratorModuleManifest> Current => Manifests
        .Where(manifest => manifest.Maturity is GeneratorMaturity.Current or GeneratorMaturity.Preview)
        .OrderBy(manifest => manifest.GeneratorId, StringComparer.OrdinalIgnoreCase)
        .ToList();
    public IReadOnlyList<GeneratorModuleManifest> Planned => Manifests
        .Where(manifest => manifest.Maturity is GeneratorMaturity.Planned or GeneratorMaturity.UnsupportedYet)
        .OrderBy(manifest => manifest.GeneratorId, StringComparer.OrdinalIgnoreCase)
        .ToList();

    public bool TryGet(string? generatorId, out GeneratorModuleManifest manifest)
    {
        return _byId.TryGetValue(generatorId?.Trim() ?? string.Empty, out manifest!);
    }
}

public static class BuiltInGeneratorCatalog
{
    private static readonly string[] StrictContractIds =
    [
        "game_profile_v1",
        "region_pack_v1",
        "scene_pack_v1",
        "npc_pack_v1",
        "quest_pack_v1",
        "dialogue_pack_v1",
        "mechanics_pack_v1",
        "encounter_pack_v1",
        "item_pack_v1"
    ];

    public static IReadOnlyList<GeneratorModuleManifest> Manifests { get; } = BuildManifests();

    public static GeneratorCatalog Create()
    {
        return new GeneratorCatalog(Manifests);
    }

    private static IReadOnlyList<GeneratorModuleManifest> BuildManifests()
    {
        var manifests = StrictContractIds.Select(StrictLlm).ToList();
        manifests.AddRange(
        [
            Current(
                "generator.package.assembly_v1",
                "Package assembly",
                [.. StrictContractIds],
                ["package.assembled_game_package"],
                ["package.artifact_review"],
                ["package.assembly"],
                "Builds the reviewed assembled package through the existing Application service."),
            Current(
                "generator.package.activation_v1",
                "Package activation",
                ["package.assembled_game_package"],
                [],
                ["package.assembly"],
                ["package.activation"],
                "Activates an already assembled package without changing the root package file."),
            Current(
                "generator.runtime_preview.generated_map_markers_v1",
                "Generated map markers",
                ["package.assembled_game_package"],
                ["runtime_preview.generated_map_markers"],
                ["package.activation", "presentation.topdown_2d_runtime_preview", "content.generated_npcs", "content.generated_encounters"],
                ["map.generated_marker_placement"],
                "Deterministic offline Runtime Preview projection; it does not modify Runtime state."),
            Planned(
                "generator.semantic.world_model_seed_v1",
                "Semantic world model seed",
                [],
                ["semantic.world_model_seed"],
                ["world_source.imported_real_map"],
                [],
                "Contract placeholder only; no semantic world model is implemented."),
            Planned(
                "generator.procedural.quest_templates_v1",
                "Procedural quest templates",
                ["semantic.world_model_seed"],
                ["procedural.quest_templates"],
                ["content.generated_quests"],
                ["quest.procedural_templates"],
                "Contract placeholder only; no procedural quest engine is implemented."),
            Planned(
                "generator.procedural.dialogue_realizer_v1",
                "Procedural dialogue realizer",
                ["semantic.world_model_seed"],
                ["procedural.dialogue_realizer"],
                ["content.generated_dialogues"],
                ["dialogue.semantic_realizer"],
                "Contract placeholder only; no dialogue realizer is implemented."),
            Planned(
                "generator.world.lazy_region_cache_v1",
                "Lazy region cache",
                ["semantic.world_model_seed"],
                ["world.lazy_region_cache"],
                ["world_source.imported_real_map", "time.calendar"],
                [],
                "Contract placeholder only; no lazy world generation is implemented."),
            Planned(
                "generator.events.offscreen_scheduler_v1",
                "Offscreen event scheduler",
                ["semantic.world_model_seed"],
                ["events.offscreen_scheduler"],
                ["time.calendar"],
                ["event.offscreen_scheduler"],
                "Contract placeholder only; no offscreen scheduler is implemented."),
            Planned(
                "generator.imported_map.osm_like_classifier_v1",
                "OSM-like map classifier",
                [],
                ["imported_map.classified_map"],
                [],
                ["world_source.imported_real_map"],
                "Contract placeholder only; no imported map pipeline is implemented."),
            Planned(
                "generator.population.households_v1",
                "Population households",
                ["imported_map.classified_map"],
                ["population.households"],
                ["world_source.imported_real_map", "time.calendar"],
                ["population.households"],
                "Contract placeholder only; no population simulation is implemented."),
            Planned(
                "generator.schedule.daily_life_v1",
                "Daily life schedules",
                ["population.households"],
                ["schedule.daily_life"],
                ["time.calendar", "population.households"],
                ["schedule.daily_life"],
                "Contract placeholder only; no daily-life scheduler is implemented.")
        ]);
        return manifests;
    }

    private static GeneratorModuleManifest StrictLlm(string contractId)
    {
        var provided = new List<string> { "generation.strict_llm_artifacts" };
        if (contractId == "npc_pack_v1") provided.Add("content.generated_npcs");
        if (contractId == "quest_pack_v1") provided.Add("content.generated_quests");
        if (contractId == "dialogue_pack_v1") provided.Add("content.generated_dialogues");
        if (contractId == "encounter_pack_v1") provided.Add("content.generated_encounters");

        return new GeneratorModuleManifest
        {
            GeneratorId = $"generator.strict_llm.{contractId}",
            Title = $"Strict LLM {contractId}",
            Description = $"Produces the existing strict artifact contract '{contractId}'.",
            Maturity = GeneratorMaturity.Current,
            UsesLlm = true,
            Deterministic = false,
            CanRunOffline = false,
            CanRunAtRuntime = false,
            OutputContracts = [contractId],
            RequiresCapabilities = ["localization.content_language_policy"],
            ProvidesCapabilities = provided,
            SupportedWorldSources = [WorldSourceKind.ProceduralPackage],
            SupportedGenerationModes = [GenerationMode.OfflineReviewed],
            RuntimeCost = GeneratorRuntimeCost.None,
            ValidationRules = ["strict_exact_json", "bounded_repair", "artifact_review_required"],
            Notes = "Runs only after an explicit editor action and never from Runtime."
        };
    }

    private static GeneratorModuleManifest Current(
        string id,
        string title,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        IReadOnlyList<string> requires,
        IReadOnlyList<string> provides,
        string notes)
    {
        return new GeneratorModuleManifest
        {
            GeneratorId = id,
            Title = title,
            Description = title,
            Maturity = GeneratorMaturity.Current,
            UsesLlm = false,
            Deterministic = true,
            CanRunOffline = true,
            CanRunAtRuntime = false,
            InputContracts = inputs,
            OutputContracts = outputs,
            RequiresCapabilities = requires,
            ProvidesCapabilities = provides,
            SupportedWorldSources = [WorldSourceKind.ProceduralPackage],
            SupportedGenerationModes = [GenerationMode.OfflineReviewed],
            RuntimeCost = GeneratorRuntimeCost.None,
            ValidationRules = ["application_service_validation"],
            Notes = notes
        };
    }

    private static GeneratorModuleManifest Planned(
        string id,
        string title,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        IReadOnlyList<string> requires,
        IReadOnlyList<string> provides,
        string notes)
    {
        return new GeneratorModuleManifest
        {
            GeneratorId = id,
            Title = title,
            Description = title,
            Maturity = GeneratorMaturity.Planned,
            UsesLlm = false,
            Deterministic = true,
            CanRunOffline = true,
            CanRunAtRuntime = false,
            InputContracts = inputs,
            OutputContracts = outputs,
            RequiresCapabilities = requires,
            ProvidesCapabilities = provides,
            SupportedGenerationModes = [GenerationMode.OfflineReviewed, GenerationMode.HybridOfflinePlusLazy],
            RuntimeCost = GeneratorRuntimeCost.None,
            ValidationRules = ["contract_only_not_executable"],
            Notes = notes
        };
    }
}
