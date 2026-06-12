using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorBatchManifest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("batch")]
    public string? Batch { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    [JsonPropertyName("files")]
    public List<string>? Files { get; set; }

    [JsonPropertyName("modules")]
    public List<GeneratorManifestModule>? Modules { get; set; }

    [JsonPropertyName("runtime_targets")]
    public List<string>? RuntimeTargets { get; set; }

    [JsonPropertyName("supported_runtime_targets")]
    public List<string>? SupportedRuntimeTargets { get; set; }

    [JsonPropertyName("architecture_notes")]
    public GeneratorManifestArchitectureNotes? ArchitectureNotes { get; set; }

    [JsonPropertyName("supported_time_modes")]
    public List<string>? SupportedTimeModes { get; set; }

    [JsonPropertyName("supported_combat_modes")]
    public List<string>? SupportedCombatModes { get; set; }

    [JsonPropertyName("contracts")]
    public JsonElement? Contracts { get; set; }

    [JsonPropertyName("contracts_introduced")]
    public JsonElement? ContractsIntroduced { get; set; }

    [JsonPropertyName("deterministic")]
    public bool? Deterministic { get; set; }

    [JsonPropertyName("unsafe_features")]
    public List<string>? UnsafeFeatures { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class GeneratorManifestArchitectureNotes
{
    [JsonPropertyName("turn_modes")]
    public List<string>? TurnModes { get; set; }

    [JsonPropertyName("combat_modes")]
    public List<string>? CombatModes { get; set; }

    [JsonPropertyName("ui_modes")]
    public List<string>? UiModes { get; set; }

    [JsonPropertyName("world_scales")]
    public List<string>? WorldScales { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class GeneratorManifestModule
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("capabilities")]
    public List<string>? Capabilities { get; set; }

    [JsonPropertyName("dependencies")]
    public List<string>? Dependencies { get; set; }

    [JsonPropertyName("depends_on")]
    public List<string>? DependsOn { get; set; }

    [JsonPropertyName("supported_turn_modes")]
    public List<string>? SupportedTurnModes { get; set; }

    [JsonPropertyName("supported_combat_modes")]
    public List<string>? SupportedCombatModes { get; set; }

    [JsonPropertyName("runtime_targets")]
    public List<string>? RuntimeTargets { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
