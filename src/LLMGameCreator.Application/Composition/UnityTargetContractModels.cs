namespace LLMGameCreator.Application.Composition;

public enum UnityContractMaturity
{
    Current,
    Preview,
    PlannedFuture
}

public enum UnityPlayerMode
{
    GenericArchivePlayer
}

public enum UnityRenderingMode
{
    TwoDimensional,
    TwoPointFiveDimensional,
    ThreeDimensional
}

public enum UnityViewMode
{
    TopDown,
    Isometric,
    FirstPerson,
    ThirdPerson,
    WorldMap
}

public enum UnityRuntimePerformanceClass
{
    Low,
    Medium,
    High
}

public enum UnityAssetRequestSource
{
    Manual,
    ComfyUiFuture,
    Imported
}

public enum UnityAudioRequestSource
{
    Manual,
    Recorded,
    SunoLikeFuture
}

public enum UnityWorldScale
{
    Small,
    Medium,
    Large,
    Infinite
}

public enum UnityBackgroundSimulationMode
{
    None,
    NearbyZones,
    AbstractRegions
}

public enum UnityMaterializationPolicy
{
    AuthoredOnly,
    LazyOnDemand,
    AuthoredImportantAndLazyGenerated
}

public enum UnityTargetContractDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record UnityTargetProfile
{
    public string TargetProfileId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public UnityContractMaturity Maturity { get; init; } = UnityContractMaturity.Current;
    public UnityPlayerMode PlayerMode { get; init; } = UnityPlayerMode.GenericArchivePlayer;
    public IReadOnlyList<UnityRenderingMode> RenderingModes { get; init; } = Array.Empty<UnityRenderingMode>();
    public IReadOnlyList<UnityViewMode> ViewModes { get; init; } = Array.Empty<UnityViewMode>();
    public string InputProfile { get; init; } = string.Empty;
    public string PerformanceBudget { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredRuntimeModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OptionalRuntimeModuleIds { get; init; } = Array.Empty<string>();
    public string AssetPipelineProfile { get; init; } = string.Empty;
    public string AudioPipelineProfile { get; init; } = string.Empty;
}

public sealed record UnityGameArchiveManifest
{
    public string ArchiveSchemaVersion { get; init; } = "0.1";
    public string GameId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ContentLanguage { get; init; } = string.Empty;
    public string TargetProfileId { get; init; } = string.Empty;
    public string DesignBriefId { get; init; } = string.Empty;
    public IReadOnlyList<string> DataPackages { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> LuaModuleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<UnityUiLayoutContract> UiLayouts { get; init; } = Array.Empty<UnityUiLayoutContract>();
    public IReadOnlyList<UnityAssetGenerationRequest> AssetRequests { get; init; } = Array.Empty<UnityAssetGenerationRequest>();
    public IReadOnlyList<UnityAudioGenerationRequest> AudioRequests { get; init; } = Array.Empty<UnityAudioGenerationRequest>();
    public IReadOnlyList<string> LocalizationFiles { get; init; } = Array.Empty<string>();
    public UnityWorldStreamingPolicy WorldStreamingPolicy { get; init; } = new();
    public string SavePolicy { get; init; } = string.Empty;
    public string BuildExportPolicy { get; init; } = string.Empty;
}

public sealed record UnityRuntimeModuleContract
{
    public string ModuleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public UnityContractMaturity Maturity { get; init; } = UnityContractMaturity.Current;
    public IReadOnlyList<string> RequiresCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ProvidesCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> InputDataContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OutputEvents { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ListenedEvents { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UiBindings { get; init; } = Array.Empty<string>();
    public string SaveSchemaId { get; init; } = string.Empty;
    public bool CanRunOffline { get; init; } = true;
    public bool CanRunAtRuntime { get; init; } = true;
    public UnityRuntimePerformanceClass PerformanceClass { get; init; } = UnityRuntimePerformanceClass.Low;
    public string ImplementationStatus { get; init; } = string.Empty;
}

public sealed record UnityUiLayoutContract
{
    public string LayoutId { get; init; } = string.Empty;
    public string ThemeId { get; init; } = string.Empty;
    public IReadOnlyList<UnityUiPanelContract> Panels { get; init; } = Array.Empty<UnityUiPanelContract>();
    public IReadOnlyList<UnityUiBindingContract> Bindings { get; init; } = Array.Empty<UnityUiBindingContract>();
    public IReadOnlyList<string> InputActions { get; init; } = Array.Empty<string>();
    public bool DraggablePanels { get; init; }
    public IReadOnlyList<string> VisibilityRules { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> StyleTokens { get; init; } = new Dictionary<string, string>();
    public IReadOnlyList<string> AssetRefs { get; init; } = Array.Empty<string>();
}

public sealed record UnityUiPanelContract
{
    public string PanelId { get; init; } = string.Empty;
    public string PanelKind { get; init; } = string.Empty;
    public string Dock { get; init; } = string.Empty;
    public IReadOnlyList<UnityUiWidgetContract> Widgets { get; init; } = Array.Empty<UnityUiWidgetContract>();
}

public sealed record UnityUiWidgetContract
{
    public string WidgetId { get; init; } = string.Empty;
    public string WidgetKind { get; init; } = string.Empty;
    public string BindingId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityUiBindingContract
{
    public string BindingId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public string UpdateMode { get; init; } = "read_only";
}

public sealed record UnityAssetGenerationRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public string AssetKind { get; init; } = string.Empty;
    public UnityAssetRequestSource Source { get; init; } = UnityAssetRequestSource.Manual;
    public string PromptOrInstruction { get; init; } = string.Empty;
    public IReadOnlyList<string> StyleTags { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityAudioGenerationRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string AudioId { get; init; } = string.Empty;
    public string AudioKind { get; init; } = string.Empty;
    public UnityAudioRequestSource Source { get; init; } = UnityAudioRequestSource.Manual;
    public string PromptOrInstruction { get; init; } = string.Empty;
    public bool Loop { get; init; }
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed record UnityWorldStreamingPolicy
{
    public UnityWorldScale WorldScale { get; init; } = UnityWorldScale.Small;
    public int ChunkSize { get; init; } = 64;
    public int ActiveRadius { get; init; } = 1;
    public UnityBackgroundSimulationMode BackgroundSimulationMode { get; init; } = UnityBackgroundSimulationMode.None;
    public UnityMaterializationPolicy PersistentEntityPolicy { get; init; } = UnityMaterializationPolicy.AuthoredOnly;
    public int GeneratedEntityBudget { get; init; } = 128;
    public int ActiveNpcBudget { get; init; } = 32;
    public UnityMaterializationPolicy NpcMaterializationPolicy { get; init; } = UnityMaterializationPolicy.AuthoredOnly;
    public UnityMaterializationPolicy QuestMaterializationPolicy { get; init; } = UnityMaterializationPolicy.AuthoredOnly;
    public bool StoreSeedRulesAndTemplates { get; init; }
    public bool MaterializeActiveChunksOnly { get; init; }
    public bool PersistDirtyDeltas { get; init; }
    public bool GenerateNpcsLazily { get; init; }
    public bool GenerateQuestsLazily { get; init; }
    public bool SeparateAuthoredAndGeneratedPopulation { get; init; }
}

public sealed record UnityTargetContractDiagnostic
{
    public UnityTargetContractDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelatedId { get; init; } = string.Empty;
}

public sealed record UnityTargetContractValidationResult
{
    public bool Ok { get; init; }
    public IReadOnlyList<UnityTargetContractDiagnostic> Diagnostics { get; init; } = Array.Empty<UnityTargetContractDiagnostic>();

    public IReadOnlyList<UnityTargetContractDiagnostic> Errors => Diagnostics
        .Where(diagnostic => diagnostic.Severity == UnityTargetContractDiagnosticSeverity.Error)
        .ToList();

    public IReadOnlyList<UnityTargetContractDiagnostic> Warnings => Diagnostics
        .Where(diagnostic => diagnostic.Severity == UnityTargetContractDiagnosticSeverity.Warning)
        .ToList();
}

public static class UnityTargetContractDiagnosticCodes
{
    public const string BlankId = "unity.target.blank_id";
    public const string DuplicateRuntimeModuleId = "unity.target.duplicate_runtime_module_id";
    public const string UnknownTargetProfile = "unity.target.unknown_target_profile";
    public const string UnknownRuntimeModule = "unity.target.unknown_runtime_module";
    public const string BlankUiBindingPath = "unity.target.blank_ui_binding_path";
    public const string DuplicateAssetRequestId = "unity.target.duplicate_asset_request_id";
    public const string DuplicateAudioRequestId = "unity.target.duplicate_audio_request_id";
    public const string UnsafeArchiveId = "unity.target.unsafe_archive_id";
    public const string FutureRuntimeModule = "unity.target.future_runtime_module";
    public const string InconsistentLargeWorldStreaming = "unity.target.inconsistent_large_world_streaming";
}
