using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public static class FeatureModuleCompositionDocumentVocabulary
{
    public const string SchemaVersion = "featuremodule_composition_document_v1";
    public const string FileExtension = ".featurecomposition.json";
    public const string DefaultWorkspaceRelativeRoot = ".llmgc/workspace/featuremodule-compositions";
}

public sealed record FeatureModuleCompositionDocument
{
    public string SchemaVersion { get; init; } = FeatureModuleCompositionDocumentVocabulary.SchemaVersion;
    public string CompositionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BaseCandidateId { get; init; } = FeatureModuleCompositionVocabulary.BaselineCandidateId;
    public IReadOnlyList<string> SelectedModuleIds { get; init; } = [];
    public IReadOnlyList<FeatureModuleParameterValue> ParameterValues { get; init; } = [];
    public string CatalogFingerprint { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> ModuleFingerprints { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public string LastMaterializedPackageSha256 { get; init; } = string.Empty;
    public string LastCompositionPackageSha256 { get; init; } = string.Empty;
    public string LastActivatedProjectPackageSha256 { get; init; } = string.Empty;
    public string LastQualifiedFinalStateHash { get; init; } = string.Empty;
    public string LastQualificationStatus { get; init; } = "NOT_RUN";
    public string PreviousMaterializedPackageSha256 { get; init; } = string.Empty;
    public string PreviousQualifiedFinalStateHash { get; init; } = string.Empty;
    public string PreviousQualificationStatus { get; init; } = string.Empty;
    public int Revision { get; init; }
}

public sealed record FeatureModuleCompositionDocumentValidation
{
    public bool Passed { get; init; }
    public bool SchemaVersionSupported { get; init; }
    public bool CompositionIdValid { get; init; }
    public bool SelectedModulesResolved { get; init; }
    public bool ParameterValuesValid { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCompositionStaleness
{
    public string Status { get; init; } = "UNRESOLVED";
    public bool Stale { get; init; }
    public bool Unresolved { get; init; }
    public bool CatalogFingerprintChanged { get; init; }
    public bool AdditiveCompatible { get; init; }
    public IReadOnlyList<string> ChangedRequiredCoreModuleIds { get; init; } = [];
    public IReadOnlyList<string> ChangedModuleIds { get; init; } = [];
    public IReadOnlyList<string> MissingModuleIds { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCompositionWorkspaceEntry
{
    public string CompositionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public int Revision { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool Corrupt { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleCompositionWorkspaceIndex
{
    public string SchemaVersion { get; init; } = "featuremodule_composition_workspace_index_v1";
    public int CompositionCount { get; init; }
    public int CorruptDocumentCount { get; init; }
    public IReadOnlyList<FeatureModuleCompositionWorkspaceEntry> Compositions { get; init; } = [];
}

public interface IFeatureModuleAuthoringClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemFeatureModuleAuthoringClock : IFeatureModuleAuthoringClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
