using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public enum GeneratedEncounterCombatRouteMode
{
    NONE,
    BASIC_ATTACK_ONLY,
    PACKAGE_ABILITY_ONLY,
    BOTH
}

public enum GeneratedEncounterCombatQualifiedActionKind
{
    BASIC_ATTACK,
    PACKAGE_ABILITY
}

public sealed record GeneratedEncounterCombatObservedEffect
{
    public string EffectClass { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetResourceIds { get; init; } = [];
    public IReadOnlyList<string> TargetStatIds { get; init; } = [];
    public IReadOnlyList<string> TargetStatusIds { get; init; } = [];
}

public sealed record GeneratedEncounterCombatQualifiedAction
{
    public GeneratedEncounterCombatQualifiedActionKind ActionKind { get; init; }
    public string AbilityId { get; init; } = string.Empty;
    public string AbilityDefinitionSha256 { get; init; } = string.Empty;
    public string SourceParticipantRoleFingerprint { get; init; } = string.Empty;
    public GeneratedEncounterCombatObservedEffect ObservedEffect { get; init; } = new();
    public IReadOnlyList<string> TargetResourceIds { get; init; } = [];
    public IReadOnlyList<string> TargetStatIds { get; init; } = [];
    public IReadOnlyList<string> TargetStatusIds { get; init; } = [];
    public GameRuntimeCommandType RuntimeCommandType { get; init; }
    public bool RuntimeQualificationPassed { get; init; }
}

public sealed record GeneratedEncounterCombatDefinitionFingerprint
{
    public string CollectionPath { get; init; } = string.Empty;
    public string DefinitionId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedEncounterCombatRoleContract
{
    public string SourceEncounterId { get; init; } = string.Empty;
    public string SourceParticipantId { get; init; } = string.Empty;
    public string RoleFingerprint { get; init; } = string.Empty;
    public IReadOnlyList<OutputDefinition> Resources { get; init; } = [];
    public IReadOnlyList<OutputDefinition> Stats { get; init; } = [];
    public IReadOnlyList<string> Abilities { get; init; } = [];
    public string? InventoryId { get; init; }
    public IReadOnlyDictionary<string, string> CombatMetadata { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GeneratedEncounterCombatContractQualificationSummary
{
    public bool StartEncounterPassed { get; init; }
    public GeneratedEncounterCombatRouteMode RouteMode { get; init; }
    public bool BasicAttackAvailable { get; init; }
    public bool BasicAttackRequired { get; init; }
    public bool PackageAbilityRequired { get; init; }
    public bool PackageAbilityAvailable { get; init; }
    public bool BasicAttackPassed { get; init; }
    public bool PackageAbilityPassed { get; init; }
    public bool PlayerRoutePassed { get; init; }
    public bool OpponentAiPassed { get; init; }
    public bool OpponentEffectObserved { get; init; }
    public bool ControlReturnedOrEncounterTerminated { get; init; }
    public bool ExactPackageReferencePassed { get; init; }
    public bool PackageShaUnchanged { get; init; }
    public int QualifiedActionCount { get; init; }
    public int QualifiedBasicAttackCount { get; init; }
    public int QualifiedPackageAbilityCount { get; init; }
    public string QualifiedActionsSha256 { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedEncounterCombatQualifiedAction> QualifiedActions { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedEncounterCombatContract
{
    public string ContractId { get; init; } = string.Empty;
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string SourceEncounterId { get; init; } = string.Empty;
    public string PlayerRoleFingerprint { get; init; } = string.Empty;
    public string OpponentRoleFingerprint { get; init; } = string.Empty;
    public GeneratedEncounterCombatRoleContract PlayerRole { get; init; } = new();
    public GeneratedEncounterCombatRoleContract OpponentRole { get; init; } = new();
    public GeneratedEncounterCombatRouteMode RouteMode { get; init; }
    public bool BasicAttackAvailable { get; init; }
    public bool BasicAttackRequired { get; init; }
    public bool BasicAttackPassed { get; init; }
    public bool PackageAbilityAvailable { get; init; }
    public bool PackageAbilityRequired { get; init; }
    public bool PackageAbilityPassed { get; init; }
    public bool PlayerRoutePassed { get; init; }
    public int QualifiedActionCount { get; init; }
    public int QualifiedBasicAttackCount { get; init; }
    public int QualifiedPackageAbilityCount { get; init; }
    public string QualifiedActionsSha256 { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedEncounterCombatQualifiedAction> QualifiedActions { get; init; } = [];
    public IReadOnlyList<GeneratedEncounterCombatDefinitionFingerprint> ExactDefinitionFingerprints { get; init; } = [];
    public GeneratedEncounterCombatContractQualificationSummary QualificationSummary { get; init; } = new();
}

public sealed record GeneratedEncounterCombatContractResult
{
    public bool Passed { get; init; }
    public GeneratedEncounterCombatContract? Contract { get; init; }
    public int CandidateEncounterCount { get; init; }
    public int CandidateRolePairCount { get; init; }
    public int QualifiedRolePairCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedEncounterCombatBinding
{
    public string EncounterSeedId { get; init; } = string.Empty;
    public string GeneratedContentSourceId { get; init; } = string.Empty;
    public string PackageEncounterId { get; init; } = string.Empty;
    public string BeforeEncounterSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedEncounterCombatBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedEncounterCombatBinding> Bindings { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedWorldEncounterCombatFingerprint
{
    public string EncounterId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedWorldEncounterCombatOverlayDocument
{
    public string SchemaVersion { get; init; } = "generated_encounter_combat_overlay_v1";
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string OutputPackageSha256 { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public int GeneratedEncounterCount { get; init; }
    public int BoundEncounterCount { get; init; }
    public int GeneratedParticipantCount { get; init; }
    public IReadOnlyList<GeneratedWorldEncounterCombatFingerprint> EncounterFingerprintsBefore { get; init; } = [];
    public IReadOnlyList<GeneratedWorldEncounterCombatFingerprint> EncounterFingerprintsAfter { get; init; } = [];
    public IReadOnlyList<string> AllowedFieldPaths { get; init; } = [];
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsBefore { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsAfter { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record GeneratedWorldEncounterCombatOverlayResult
{
    public bool Passed { get; init; }
    public GamePackageDefinition CombatOverlayPackage { get; init; } = new();
    public string CombatOverlayPackageJson { get; init; } = string.Empty;
    public GeneratedWorldEncounterCombatOverlayDocument Document { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectGeneratedEncounterCombatSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "ABSENT";
    public string ContractId { get; init; } = string.Empty;
    public string ContractSourcePackageSha256 { get; init; } = string.Empty;
    public int GeneratedEncounterCount { get; init; }
    public int QualifiedEncounterCount { get; init; }
    public string ExactPackageSha256 { get; init; } = string.Empty;
    public bool ExactPackageReferencePassed { get; init; }
    public bool PackageShaUnchangedDuringRuntime { get; init; }
    public GeneratedEncounterCombatRouteMode RouteMode { get; init; }
    public bool BasicAttackAvailable { get; init; }
    public bool BasicAttackRequired { get; init; }
    public bool PackageAbilityRequired { get; init; }
    public bool PackageAbilityAvailable { get; init; }
    public bool BasicAttackPassed { get; init; }
    public bool PackageAbilityPassed { get; init; }
    public bool PlayerRoutePassed { get; init; }
    public int QualifiedActionCount { get; init; }
    public int QualifiedBasicAttackCount { get; init; }
    public int QualifiedPackageAbilityCount { get; init; }
    public string QualifiedActionsSha256 { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedEncounterCombatQualifiedAction> QualifiedActions { get; init; } = [];
    public bool OpponentAiPassed { get; init; }
    public bool VictoryPassed { get; init; }
    public bool FleePassed { get; init; }
    public bool RewardPassed { get; init; }
    public bool GeneratedQuestReadyPassed { get; init; }
    public bool ManualTurnInPassed { get; init; }
    public int CompleteQuestCommandCount { get; init; }
    public int AdvanceObjectiveCommandCount { get; init; }
    public bool ConsequencePassed { get; init; }
    public bool ReplayPassed { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedEncounterCombatRuntimeFrame> RuntimeFrames { get; init; } = [];
    public IReadOnlyList<GeneratedEncounterCombatHumanFact> HumanReviewFacts { get; init; } = [];
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public GeneratedWorldEncounterCombatOverlayDocument? Overlay { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedEncounterCombatRuntimeFrame
{
    public int Index { get; init; }
    public string ActionKind { get; init; } = string.Empty;
    public string StateHash { get; init; } = string.Empty;
}

public sealed record GeneratedEncounterCombatHumanFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

internal static class GeneratedEncounterCombatCanonical
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    internal static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    internal static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions)
        ?? throw new InvalidOperationException("generated_combat.clone_failed");

    internal static string Hash<T>(T value) => HashText(Serialize(value));

    internal static string HashText(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
