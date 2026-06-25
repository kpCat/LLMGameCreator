using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GenerationPresetOptionsService
{
    public const string DefaultSeed = "one-click-generated-preview-workflow";
    public const string DefaultPresetId = "survival_exploration";
    public const string DefaultMode = ProceduralGameGenerationModes.SemiProceduralRegions;

    private static readonly IReadOnlyList<GenerationPresetDefinition> Presets =
    [
        new GenerationPresetDefinition
        {
            PresetId = DefaultPresetId,
            Title = "Survival exploration",
            CompactStyleHintIds =
            [
                "theme/exploration",
                "theme/survival",
                "tone/mysterious",
                "quest_motif/faction_truce",
                "item_affordance/quest_item"
            ]
        },
        new GenerationPresetDefinition
        {
            PresetId = "recover_resource",
            Title = "Recover resource",
            CompactStyleHintIds =
            [
                "theme/survival",
                "theme/trade",
                "tone/mysterious",
                "quest_motif/recover_lost_resource",
                "item_affordance/consumable",
                "location_mood/dangerous"
            ]
        },
        new GenerationPresetDefinition
        {
            PresetId = "safe_faction_truce",
            Title = "Safe faction truce",
            CompactStyleHintIds =
            [
                "theme/exploration",
                "theme/trade",
                "tone/mysterious",
                "quest_motif/faction_truce",
                "item_affordance/tradable",
                "location_mood/safe"
            ]
        }
    ];

    private static readonly IReadOnlyList<string> DefaultVariantIds =
    [
        "world_topology/region_graph",
        "actor_model/single_player_character",
        "combat_model/turn_based",
        "inventory_model/list_inventory"
    ];

    public IReadOnlyList<GenerationPresetDefinition> GetPresets() => Presets;

    public GenerationPresetOptions Resolve(GenerationPresetOptionsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var seed = FirstNonEmpty(request.Seed, DefaultSeed);
        var mode = ProceduralGameGenerationModes.Supported.Contains(request.Mode)
            ? request.Mode
            : DefaultMode;
        var preset = Presets.FirstOrDefault(item => string.Equals(item.PresetId, request.PresetId, StringComparison.OrdinalIgnoreCase))
            ?? Presets.First(item => item.PresetId == DefaultPresetId);
        var styleHintIds = request.CompactStyleHintIds.Count > 0
            ? NormalizeIds(request.CompactStyleHintIds)
            : preset.CompactStyleHintIds;
        var selectedVariantIds = request.SelectedVariantIds.Count > 0
            ? NormalizeIds(request.SelectedVariantIds)
            : DefaultVariantIds;

        return new GenerationPresetOptions
        {
            Seed = seed,
            Mode = mode,
            PresetId = preset.PresetId,
            PresetTitle = preset.Title,
            CompactStyleHintIds = styleHintIds,
            SelectedVariantIds = selectedVariantIds,
            StableSummary = BuildStableSummary(seed, mode, preset.PresetId, styleHintIds, selectedVariantIds)
        };
    }

    public GenerationPresetOptions ResolveDefault() => Resolve(new GenerationPresetOptionsRequest());

    private static IReadOnlyList<string> NormalizeIds(IReadOnlyList<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

    private static string BuildStableSummary(
        string seed,
        string mode,
        string presetId,
        IReadOnlyList<string> compactStyleHintIds,
        IReadOnlyList<string> selectedVariantIds) =>
        string.Join("; ", new[]
        {
            $"seed={seed}",
            $"mode={mode}",
            $"preset={presetId}",
            $"styleHints={string.Join("|", compactStyleHintIds)}",
            $"variants={string.Join("|", selectedVariantIds)}"
        });

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}

public sealed record GenerationPresetOptionsRequest
{
    public string Seed { get; init; } = GenerationPresetOptionsService.DefaultSeed;
    public string Mode { get; init; } = GenerationPresetOptionsService.DefaultMode;
    public string PresetId { get; init; } = GenerationPresetOptionsService.DefaultPresetId;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
}

public sealed record GenerationPresetOptions
{
    public string Seed { get; init; } = GenerationPresetOptionsService.DefaultSeed;
    public string Mode { get; init; } = GenerationPresetOptionsService.DefaultMode;
    public string PresetId { get; init; } = GenerationPresetOptionsService.DefaultPresetId;
    public string PresetTitle { get; init; } = string.Empty;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record GenerationPresetDefinition
{
    public string PresetId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();

    public override string ToString() => Title;
}
