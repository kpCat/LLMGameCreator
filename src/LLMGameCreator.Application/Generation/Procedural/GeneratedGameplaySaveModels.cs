using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class GeneratedGameplaySaveVocabulary
{
    public const string RootRelativePath = ".llmgc/gameplay-saves";
    public const string SlotSchemaVersion = "generated_gameplay_save_slot_v1";
    public const string RevisionSchemaVersion = "generated_gameplay_save_v1";
    public const string MigrationSchemaVersion = "generated_gameplay_save_migration_v1";
    public const string MigrationPolicyId = "generated_world_canonical_definition_migration_v1";
}

public enum GeneratedGameplaySaveStatus
{
    CURRENT,
    PACKAGE_REBASE_REQUIRED,
    WORLD_MIGRATION_REQUIRED,
    INVALID,
    LEGACY_RAW
}

public sealed record GeneratedGameplaySaveSlotManifest
{
    public string SchemaVersion { get; init; } = GeneratedGameplaySaveVocabulary.SlotSchemaVersion;
    public string SlotName { get; init; } = string.Empty;
    public string CurrentRevisionSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> RevisionSha256s { get; init; } = [];
}

public sealed record GeneratedGameplayDefinitionFingerprint
{
    public string Kind { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
    public bool Generated { get; init; }
    public string? SourceId { get; init; }
}

public sealed record GeneratedGameplaySessionReference
{
    public string Kind { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
}

public sealed record GeneratedGameplaySessionReferenceInventory
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedGameplayDefinitionFingerprint> Fingerprints { get; init; } = [];
    public IReadOnlyList<GeneratedGameplaySessionReference> References { get; init; } = [];
    public IReadOnlyList<GeneratedGameplaySessionReference> UnresolvedReferences { get; init; } = [];
    public IReadOnlyList<string> PortableFlagKeys { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GeneratedGameplaySaveMigration
{
    public string SchemaVersion { get; init; } = GeneratedGameplaySaveVocabulary.MigrationSchemaVersion;
    public string SourceRevisionSha256 { get; init; } = string.Empty;
    public string SourceWorldId { get; init; } = string.Empty;
    public string TargetWorldId { get; init; } = string.Empty;
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string TargetPackageSha256 { get; init; } = string.Empty;
    public string MigrationPolicyId { get; init; } = GeneratedGameplaySaveVocabulary.MigrationPolicyId;
    public bool MapReset { get; init; }
    public IReadOnlyDictionary<string, int> PreservedCounts { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DroppedCounts { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> PreservedDefinitionIds { get; init; } = [];
    public IReadOnlyList<string> DroppedDefinitionIds { get; init; } = [];
    public IReadOnlyList<string> DroppedReasons { get; init; } = [];
}

public sealed record GeneratedGameplaySaveRevision
{
    public string SchemaVersion { get; init; } = GeneratedGameplaySaveVocabulary.RevisionSchemaVersion;
    public string RevisionSha256 { get; init; } = string.Empty;
    public string? ParentRevisionSha256 { get; init; }
    public GeneratedGameplaySaveMigration? Migration { get; init; }
    public string ProjectPackageId { get; init; } = string.Empty;
    public string ProjectIdentityFingerprint { get; init; } = string.Empty;
    public string WorldId { get; init; } = string.Empty;
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string SourceRequestSha256 { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string OverlaySha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string SelectedBuildHistorySha256 { get; init; } = string.Empty;
    public string UnifiedRuntimeSessionJson { get; init; } = string.Empty;
    public string UnifiedRuntimeSessionSha256 { get; init; } = string.Empty;
    public string MapStateSha256 { get; init; } = string.Empty;
    public string GameplayStateSha256 { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public string CurrentRegionSourceId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedGameplayDefinitionFingerprint> DefinitionFingerprints { get; init; } = [];
    public IReadOnlyList<string> GeneratedReferenceIds { get; init; } = [];
    public IReadOnlyList<string> PortableFlagKeys { get; init; } = [];
    public IReadOnlyList<GeneratedGameplaySaveFact> SaveFacts { get; init; } = [];
}

public sealed record GeneratedGameplaySaveProjectTruth
{
    public string ProjectFolder { get; init; } = string.Empty;
    public GameProjectIdentityDocument Identity { get; init; } = new();
    public string IdentityFingerprint { get; init; } = string.Empty;
    public SeededGeneratedProjectSourceValidationResult StrictGeneratedSource { get; init; } = new();
    public string WorldId { get; init; } = string.Empty;
    [JsonIgnore]
    public GamePackageDefinition ActualPackage { get; init; } = new();
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string SelectedBuildHistorySha256 { get; init; } = string.Empty;
    public string GeneratedStartMapId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> GeneratedRegionMapBindings { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<GeneratedGameplayDefinitionFingerprint> DefinitionFingerprintInventory { get; init; } = [];
}

public sealed record GeneratedGameplaySaveProjectTruthResult
{
    public bool Passed { get; init; }
    public GeneratedGameplaySaveProjectTruth? Truth { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveValidationResult
{
    public bool Passed { get; init; }
    public GeneratedGameplaySaveStatus Status { get; init; } = GeneratedGameplaySaveStatus.INVALID;
    public UnifiedRuntimeSession? Session { get; init; }
    public GeneratedGameplaySessionReferenceInventory? References { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveStoreReadResult
{
    public bool Passed { get; init; }
    public string SlotName { get; init; } = string.Empty;
    public string SlotPath { get; init; } = string.Empty;
    public GeneratedGameplaySaveSlotManifest? Manifest { get; init; }
    public GeneratedGameplaySaveRevision? CurrentRevision { get; init; }
    public IReadOnlyList<GeneratedGameplaySaveRevision> Revisions { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveStoreWriteResult
{
    public bool Passed { get; init; }
    public bool Deduplicated { get; init; }
    public bool RevisionCreated { get; init; }
    public string SlotName { get; init; } = string.Empty;
    public string RevisionSha256 { get; init; } = string.Empty;
    public GeneratedGameplaySaveSlotManifest? Manifest { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveEntry
{
    public string SlotName { get; init; } = string.Empty;
    public GeneratedGameplaySaveStatus Status { get; init; } = GeneratedGameplaySaveStatus.INVALID;
    public string CurrentRevisionSha256 { get; init; } = string.Empty;
    public int RevisionCount { get; init; }
    public string SavedWorldId { get; init; } = string.Empty;
    public string CurrentWorldId { get; init; } = string.Empty;
    public string SavedWorldTitle { get; init; } = string.Empty;
    public string CurrentWorldTitle { get; init; } = string.Empty;
    public GeneratedGameplaySaveMigration? Migration { get; init; }
    public bool LegacyRaw { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveListResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedGameplaySaveEntry> Entries { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveResult
{
    public bool Passed { get; init; }
    public string SlotName { get; init; } = string.Empty;
    public string RevisionSha256 { get; init; } = string.Empty;
    public bool Deduplicated { get; init; }
    public GeneratedGameplaySaveStatus Status { get; init; } = GeneratedGameplaySaveStatus.INVALID;
    public GeneratedGameplaySaveRevision? Revision { get; init; }
    public UnifiedRuntimeSession? Session { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySaveMigrationPreview
{
    public string SlotName { get; init; } = string.Empty;
    public string SourceRevisionSha256 { get; init; } = string.Empty;
    public GeneratedGameplaySaveStatus SourceStatus { get; init; } = GeneratedGameplaySaveStatus.INVALID;
    public string SourceWorldId { get; init; } = string.Empty;
    public string TargetWorldId { get; init; } = string.Empty;
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string TargetPackageSha256 { get; init; } = string.Empty;
    public bool MapReset { get; init; }
    public IReadOnlyDictionary<string, int> PreservedCountsByKind { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DroppedCountsByKind { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> PreservedDefinitionIds { get; init; } = [];
    public IReadOnlyList<string> DroppedDefinitionIds { get; init; } = [];
    public IReadOnlyList<string> DroppedReasons { get; init; } = [];
    public string CandidateSessionSha256 { get; init; } = string.Empty;
    public string CandidateMapStateSha256 { get; init; } = string.Empty;
    public string CandidateGameplayStateSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record GeneratedGameplaySaveMigrationApplyRequest
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string SlotName { get; init; } = string.Empty;
    public string SourceRevisionSha256 { get; init; } = string.Empty;
    public string CandidateSessionSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedGameplaySaveMigrationResult
{
    public bool Passed { get; init; }
    public string SlotName { get; init; } = string.Empty;
    public string SourceRevisionSha256 { get; init; } = string.Empty;
    public string MigratedRevisionSha256 { get; init; } = string.Empty;
    public GeneratedGameplaySaveMigrationPreview? Preview { get; init; }
    public GeneratedGameplaySaveRevision? Revision { get; init; }
    public UnifiedRuntimeSession? Session { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameplaySavesSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public int SlotCount { get; init; }
    public int CurrentCount { get; init; }
    public int MigrationRequiredCount { get; init; }
    public int InvalidCount { get; init; }
    public int LegacyRawCount { get; init; }
    public GeneratedGameplaySaveMigration? LastMigration { get; init; }
    public IReadOnlyList<GeneratedGameplaySaveEntry> Entries { get; init; } = [];
    public IReadOnlyList<GeneratedGameplaySaveFact> HumanFacts { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal static class GeneratedGameplaySaveJson
{
    private static readonly JsonSerializerOptions CompactOptions = Create(writeIndented: false);
    private static readonly JsonSerializerOptions PrettyOptions = Create(writeIndented: true);

    public static string Canonical<T>(T value)
    {
        var node = JsonSerializer.SerializeToNode(value, CompactOptions);
        return CanonicalNode(node)?.ToJsonString(CompactOptions) ?? "null";
    }

    public static string Stored<T>(T value) =>
        (CanonicalNode(JsonSerializer.SerializeToNode(value, PrettyOptions))?.ToJsonString(PrettyOptions) ?? "null")
        + Environment.NewLine;

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, CompactOptions);

    public static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static string HashCanonical<T>(T value) => HashText(Canonical(value));

    public static string RevisionSha256(GeneratedGameplaySaveRevision revision) =>
        HashCanonical(revision with { RevisionSha256 = string.Empty });

    private static JsonSerializerOptions Create(bool writeIndented)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = writeIndented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static JsonNode? CanonicalNode(JsonNode? node) => node switch
    {
        JsonObject obj => new JsonObject(obj.OrderBy(property => property.Key, StringComparer.Ordinal)
            .Select(property => KeyValuePair.Create(property.Key, CanonicalNode(property.Value)))),
        JsonArray array => new JsonArray(array.Select(CanonicalNode).ToArray()),
        null => null,
        _ => node.DeepClone()
    };
}
