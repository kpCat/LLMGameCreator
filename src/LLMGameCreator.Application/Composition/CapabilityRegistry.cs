namespace LLMGameCreator.Application.Composition;

public sealed class CapabilityRegistry
{
    private readonly IReadOnlyDictionary<string, CapabilityDefinition> _byId;

    public CapabilityRegistry(IEnumerable<CapabilityDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        Definitions = definitions.ToList();
        DuplicateIds = Definitions
            .Where(definition => !string.IsNullOrWhiteSpace(definition.Id))
            .GroupBy(definition => definition.Id.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _byId = Definitions
            .Where(definition => !string.IsNullOrWhiteSpace(definition.Id))
            .GroupBy(definition => definition.Id.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<CapabilityDefinition> Definitions { get; }
    public IReadOnlyList<string> DuplicateIds { get; }

    public bool TryGet(string? capabilityId, out CapabilityDefinition definition)
    {
        return _byId.TryGetValue(capabilityId?.Trim() ?? string.Empty, out definition!);
    }
}

public static class BuiltInCapabilityRegistry
{
    public static IReadOnlyList<CapabilityDefinition> Definitions { get; } = BuildDefinitions();

    public static CapabilityRegistry Create()
    {
        return new CapabilityRegistry(Definitions);
    }

    private static IReadOnlyList<CapabilityDefinition> BuildDefinitions()
    {
        return
        [
            Current("localization.content_language_policy", "Content language policy", "localization"),
            Current("generation.strict_llm_artifacts", "Strict LLM artifacts", "generation",
                requires: ["localization.content_language_policy"]),
            Current("package.artifact_review", "Artifact review", "package",
                requires: ["generation.strict_llm_artifacts"]),
            Current("package.assembly", "Package assembly", "package",
                requires: ["package.artifact_review"]),
            Current("package.activation", "Package activation", "package",
                requires: ["package.assembly"]),
            Current("world_source.procedural_package", "Procedural package world", "world_source",
                conflicts: ["world_source.imported_real_map"],
                worldSources: [WorldSourceKind.ProceduralPackage]),
            Current("presentation.topdown_2d_runtime_preview", "Top-down 2D runtime preview", "presentation",
                requires: ["world_source.procedural_package", "package.activation"],
                worldSources: [WorldSourceKind.ProceduralPackage],
                presentations: [PresentationKind.TopDown2D]),
            Current("runtime.preview_movement", "Runtime preview movement", "runtime",
                requires: ["presentation.topdown_2d_runtime_preview", "package.activation"],
                worldSources: [WorldSourceKind.ProceduralPackage],
                presentations: [PresentationKind.TopDown2D]),
            Current("dialogue.preview_lines", "Dialogue line preview", "dialogue",
                requires: ["package.activation", "content.generated_dialogues"]),
            Current("quest.preview_journal", "Quest preview journal", "quest",
                requires: ["package.activation", "content.generated_quests"]),
            Current("map.generated_marker_placement", "Generated marker placement", "map",
                requires: ["presentation.topdown_2d_runtime_preview", "content.generated_npcs", "content.generated_encounters"],
                worldSources: [WorldSourceKind.ProceduralPackage],
                presentations: [PresentationKind.TopDown2D]),
            Current("content.generated_npcs", "Generated NPC content", "content",
                requires: ["package.assembly"]),
            Current("content.generated_quests", "Generated quest content", "content",
                requires: ["package.assembly"]),
            Current("content.generated_dialogues", "Generated dialogue content", "content",
                requires: ["package.assembly"]),
            Current("content.generated_encounters", "Generated encounter content", "content",
                requires: ["package.assembly"]),
            Planned("world_source.imported_real_map", "Imported real map", "world_source",
                conflicts: ["world_source.procedural_package"],
                worldSources: [WorldSourceKind.ImportedRealMap, WorldSourceKind.HybridImportedPlusGenerated]),
            Planned("time.calendar", "Calendar time", "time"),
            Planned("population.households", "Population households", "population",
                requires: ["world_source.imported_real_map", "time.calendar"]),
            Planned("schedule.daily_life", "Daily life schedules", "schedule",
                requires: ["time.calendar", "population.households"]),
            Planned("event.offscreen_scheduler", "Offscreen event scheduler", "event",
                requires: ["time.calendar"]),
            Planned("quest.procedural_templates", "Procedural quest templates", "quest",
                requires: ["content.generated_quests"],
                optionalRequires: ["dialogue.semantic_realizer"]),
            Planned("dialogue.semantic_realizer", "Semantic dialogue realizer", "dialogue",
                requires: ["content.generated_dialogues"])
        ];
    }

    private static CapabilityDefinition Current(
        string id,
        string title,
        string category,
        IReadOnlyList<string>? requires = null,
        IReadOnlyList<string>? optionalRequires = null,
        IReadOnlyList<string>? conflicts = null,
        IReadOnlyList<WorldSourceKind>? worldSources = null,
        IReadOnlyList<PresentationKind>? presentations = null)
    {
        return Definition(id, title, category, CapabilityMaturity.Current, requires, optionalRequires, conflicts, worldSources, presentations);
    }

    private static CapabilityDefinition Planned(
        string id,
        string title,
        string category,
        IReadOnlyList<string>? requires = null,
        IReadOnlyList<string>? optionalRequires = null,
        IReadOnlyList<string>? conflicts = null,
        IReadOnlyList<WorldSourceKind>? worldSources = null,
        IReadOnlyList<PresentationKind>? presentations = null)
    {
        return Definition(id, title, category, CapabilityMaturity.Planned, requires, optionalRequires, conflicts, worldSources, presentations);
    }

    private static CapabilityDefinition Definition(
        string id,
        string title,
        string category,
        CapabilityMaturity maturity,
        IReadOnlyList<string>? requires,
        IReadOnlyList<string>? optionalRequires,
        IReadOnlyList<string>? conflicts,
        IReadOnlyList<WorldSourceKind>? worldSources,
        IReadOnlyList<PresentationKind>? presentations)
    {
        return new CapabilityDefinition
        {
            Id = id,
            Title = title,
            Description = title,
            Category = category,
            Requires = requires ?? Array.Empty<string>(),
            OptionalRequires = optionalRequires ?? Array.Empty<string>(),
            Provides = [id],
            Conflicts = conflicts ?? Array.Empty<string>(),
            SupportedWorldSources = worldSources ?? Array.Empty<WorldSourceKind>(),
            SupportedPresentations = presentations ?? Array.Empty<PresentationKind>(),
            GenerationModes = [GenerationMode.OfflineReviewed],
            RuntimeCost = category == "runtime" ? CapabilityRuntimeCost.Low : CapabilityRuntimeCost.None,
            Maturity = maturity
        };
    }
}
