using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveRequestProviderKind
{
    manual_import,
    comfyui_future,
    suno_future,
    local_audio_future,
    procedural_future,
    none
}

public enum UnityArchiveAssetKind
{
    portrait,
    icon,
    scene_illustration,
    background,
    tile_texture,
    ui_widget,
    ui_theme
}

public enum UnityArchiveAudioKind
{
    ui_sfx,
    footstep,
    ability,
    scene_ambience,
    music
}

public enum UnityArchiveLuaModuleKind
{
    inventory,
    quest_journal,
    dialogue,
    combat,
    crafting,
    stats,
    world_map,
    factions,
    transport_future,
    police_future,
    army_battle_future
}

public enum UnityArchiveRequestReadiness
{
    Ready,
    ReadyWithWarnings,
    BlockedByErrors
}

public sealed record UnityArchiveRequestSourceRef
{
    public string SourceId { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
}

public sealed record UnityArchiveRequestDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record UnityArchiveAssetRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public UnityArchiveAssetKind AssetKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; } = UnityArchiveRequestProviderKind.manual_import;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
    public string PromptOrInstruction { get; init; } = string.Empty;
    public IReadOnlyList<string> StyleTags { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityArchiveAudioRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string AudioId { get; init; } = string.Empty;
    public UnityArchiveAudioKind AudioKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; } = UnityArchiveRequestProviderKind.local_audio_future;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
    public string PromptOrInstruction { get; init; } = string.Empty;
    public bool Loop { get; init; }
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityArchiveLuaModuleRequest
{
    public string ModuleId { get; init; } = string.Empty;
    public UnityArchiveLuaModuleKind ModuleKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; } = UnityArchiveRequestProviderKind.none;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
    public string PromptOrInstruction { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityArchiveRequestPipelineRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public GameDesignBrief DesignBrief { get; init; } = new();
    public UnityTargetProfile TargetProfile { get; init; } = new();
    public UnityGameArchiveManifest ArchiveManifest { get; init; } = new();
    public IReadOnlyList<UnityRuntimeModuleContract> RuntimeModules { get; init; }
        = Array.Empty<UnityRuntimeModuleContract>();
    public GamePackageDefinition? Package { get; init; }
}

public sealed record UnityArchiveRequestPipelineResult
{
    public IReadOnlyList<UnityArchiveAssetRequest> AssetRequests { get; init; }
        = Array.Empty<UnityArchiveAssetRequest>();
    public IReadOnlyList<UnityArchiveAudioRequest> AudioRequests { get; init; }
        = Array.Empty<UnityArchiveAudioRequest>();
    public IReadOnlyList<UnityArchiveLuaModuleRequest> LuaModuleRequests { get; init; }
        = Array.Empty<UnityArchiveLuaModuleRequest>();
    public IReadOnlyList<UnityArchiveRequestDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveRequestDiagnostic>();
    public UnityArchiveRequestReadiness Readiness { get; init; } = UnityArchiveRequestReadiness.Ready;
}
