using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public enum GeneratedCampaignBranchKind
{
    SUPPORT,
    CHALLENGE,
    REFUSE
}

public sealed record GeneratedCampaignChoiceBranch
{
    public GeneratedCampaignBranchKind Kind { get; init; }
    public string ChoiceId { get; init; } = string.Empty;
    public string FlagValue { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string? QuestId { get; init; }
    public string? EncounterId { get; init; }
    public double ReputationAmount { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignChoiceBinding
{
    public string ActorSeedId { get; init; } = string.Empty;
    public string ActorEntityId { get; init; } = string.Empty;
    public string InteractionId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignChoiceBranch> Branches { get; init; } = [];
    public string Status { get; init; } = "NO_BRANCH_RELATIONSHIP";
}

public sealed record GeneratedCampaignChoiceBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedCampaignChoiceBinding> Bindings { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignChoiceDialogueFingerprint
{
    public string DialogueId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignChoiceOverlayDocument
{
    public string SchemaVersion { get; init; } = "generated_campaign_choice_overlay_v1";
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string OutputPackageSha256 { get; init; } = string.Empty;
    public int GeneratedDialogueCount { get; init; }
    public int BranchableDialogueCount { get; init; }
    public int QualifiedBranchCount { get; init; }
    public IReadOnlyList<GeneratedCampaignChoiceBinding> Bindings { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignChoiceDialogueFingerprint> DialogueFingerprintsBefore { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignChoiceDialogueFingerprint> DialogueFingerprintsAfter { get; init; } = [];
    public IReadOnlyList<string> AllowedFieldPaths { get; init; } = [];
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsBefore { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsAfter { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignChoiceOverlayResult
{
    public bool Passed { get; init; }
    public GamePackageDefinition ChoiceOverlayPackage { get; init; } = new();
    public string ChoiceOverlayPackageJson { get; init; } = string.Empty;
    public GeneratedCampaignChoiceOverlayDocument Document { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignChoiceRuntimeFrame
{
    public string DialogueId { get; init; } = string.Empty;
    public GeneratedCampaignBranchKind BranchKind { get; init; }
    public string StateHash { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignChoiceHumanFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GameProjectGeneratedCampaignChoiceSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "ABSENT";
    public string OverlaySchemaVersion { get; init; } = string.Empty;
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string ChoiceOverlayPackageSha256 { get; init; } = string.Empty;
    public string FinalPackageSha256 { get; init; } = string.Empty;
    public int GeneratedDialogueCount { get; init; }
    public int BranchableDialogueCount { get; init; }
    public int QualifiedDialogueCount { get; init; }
    public int SupportBranchCount { get; init; }
    public int ChallengeBranchCount { get; init; }
    public int RefuseBranchCount { get; init; }
    public IReadOnlyList<string> BranchFlagIds { get; init; } = [];
    public string ChoiceOverlaySha256 { get; init; } = string.Empty;
    public bool RuntimeQualificationPassed { get; init; }
    public bool ExclusiveBranchingPassed { get; init; }
    public bool FollowUpPassed { get; init; }
    public bool AtomicRollbackPassed { get; init; }
    public bool ReplayPassed { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignChoiceRuntimeFrame> RuntimeFrames { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignChoiceHumanFact> HumanReviewFacts { get; init; } = [];
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public GeneratedCampaignChoiceOverlayDocument? Overlay { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal static class GeneratedCampaignChoiceCanonical
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);
    internal static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Serialize(value), JsonOptions)
        ?? throw new InvalidOperationException("generated_choice.clone_failed");
    internal static string Hash<T>(T value) => HashText(Serialize(value));
    internal static string HashText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
