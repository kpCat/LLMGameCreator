namespace LLMGameCreator.Application.Design.FeatureModuleCertification;

public static class FeatureModuleCertificationVocabulary
{
    public const string RuntimeQualifierContractVersion = "product_line_runtime_qualifier_v1";
    public const string DefaultCacheRelativeRoot = ".llmgc/workspace/featuremodule-certification-cache";
}

public sealed record FeatureModuleCertificationPlanItem
{
    public string ModuleId { get; init; } = string.Empty;
    public string ModuleFingerprint { get; init; } = string.Empty;
    public string DependencyFingerprint { get; init; } = string.Empty;
    public string BasePackageSha256 { get; init; } = string.Empty;
    public string RuntimeQualifierContractVersion { get; init; } = string.Empty;
    public string ActionPlanSignature { get; init; } = string.Empty;
    public string ParameterDefaultsFingerprint { get; init; } = string.Empty;
    public string CacheKey { get; init; } = string.Empty;
}

public sealed record FeatureModuleCertificationPlan
{
    public string SchemaVersion { get; init; } = "featuremodule_certification_plan_v1";
    public int ModuleCount { get; init; }
    public IReadOnlyList<FeatureModuleCertificationPlanItem> Modules { get; init; } = [];
}

public sealed record FeatureModuleCertificationEntry
{
    public string ModuleId { get; init; } = string.Empty;
    public string ModuleFingerprint { get; init; } = string.Empty;
    public string DependencyFingerprint { get; init; } = string.Empty;
    public string BasePackageSha256 { get; init; } = string.Empty;
    public string RuntimeQualifierContractVersion { get; init; } = string.Empty;
    public string ActionPlanSignature { get; init; } = string.Empty;
    public string ParameterDefaultsFingerprint { get; init; } = string.Empty;
    public string Status { get; init; } = "FAILED";
    public bool StructuralValidationPassed { get; init; }
    public bool DefaultParameterValidationPassed { get; init; }
    public bool MaterializationPassed { get; init; }
    public bool PackageValidationPassed { get; init; }
    public bool RuntimeQualificationPassed { get; init; }
    public bool RuntimeEffectsPassed { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public DateTimeOffset CertifiedAtUtc { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCertificationLedger
{
    public string SchemaVersion { get; init; } = "featuremodule_certification_ledger_v1";
    public string Status { get; init; } = "FAILED";
    public int PlannedModuleCount { get; init; }
    public int CertifiedModuleCount { get; init; }
    public int ExecutedCount { get; init; }
    public int ReusedCount { get; init; }
    public int InvalidatedCount { get; init; }
    public bool CorruptCacheRejected { get; init; }
    public bool CacheIsOptimizationOnly { get; init; } = true;
    public IReadOnlyList<FeatureModuleCertificationEntry> Entries { get; init; } = [];
}

internal sealed record FeatureModuleCertificationCacheEnvelope
{
    public string SchemaVersion { get; init; } = "featuremodule_certification_cache_entry_v1";
    public string CacheKey { get; init; } = string.Empty;
    public string EntrySha256 { get; init; } = string.Empty;
    public FeatureModuleCertificationEntry Entry { get; init; } = new();
}

public enum FeatureModuleCertificationCacheReadState
{
    Missing,
    Reused,
    Invalidated,
    Corrupt
}
