namespace LLMGameCreator.Application.Design.Semantics;

public sealed record SemanticCatalog
{
    public string SchemaVersion { get; init; } = "1";
    public string CatalogId { get; init; } = "project-semantic-catalog";
    public IReadOnlyList<SemanticCatalogTerm> Terms { get; init; } = Array.Empty<SemanticCatalogTerm>();
    public IReadOnlyList<SemanticCatalogRelation> Relations { get; init; } = Array.Empty<SemanticCatalogRelation>();
    public IReadOnlyList<SemanticCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticCatalogDiagnostic>();
}

public sealed record SemanticCatalogTerm
{
    public string TermId { get; init; } = string.Empty;
    public string Kind { get; init; } = SemanticTermKinds.Unknown;
    public string Label { get; init; } = string.Empty;
    public string Status { get; init; } = SemanticTermStatuses.Candidate;
    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SourceArtifactIds { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = string.Empty;
}

public sealed record SemanticCatalogRelation
{
    public string RelationId { get; init; } = string.Empty;
    public string SourceTermId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string TargetTermId { get; init; } = string.Empty;
    public string Status { get; init; } = SemanticTermStatuses.Candidate;
    public IReadOnlyList<string> SourceArtifactIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticCatalogDiagnostic
{
    public string Severity { get; init; } = SemanticDiagnosticSeverity.Warning;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string SourceArtifactId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record SemanticCatalogWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string CatalogMarkdownPath { get; init; } = string.Empty;
}

public static class SemanticTermKinds
{
    public const string Theme = "theme";
    public const string Tone = "tone";
    public const string Biome = "biome";
    public const string Faction = "faction";
    public const string FactionRelation = "faction_relation";
    public const string NpcArchetype = "npc_archetype";
    public const string DialogueIntent = "dialogue_intent";
    public const string QuestMotif = "quest_motif";
    public const string ItemAffordance = "item_affordance";
    public const string LocationMood = "location_mood";
    public const string AssetStyleHint = "asset_style_hint";
    public const string AudioMoodHint = "audio_mood_hint";
    public const string EntityRole = "entity_role";
    public const string Unknown = "unknown";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        Theme,
        Tone,
        Biome,
        Faction,
        FactionRelation,
        NpcArchetype,
        DialogueIntent,
        QuestMotif,
        ItemAffordance,
        LocationMood,
        AssetStyleHint,
        AudioMoodHint,
        EntityRole,
        Unknown
    };
}

public static class SemanticTermStatuses
{
    public const string Known = "known";
    public const string Candidate = "candidate";
    public const string Deprecated = "deprecated";
    public const string Conflict = "conflict";
    public const string Invalid = "invalid";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        Known,
        Candidate,
        Deprecated,
        Conflict,
        Invalid
    };
}

public static class SemanticDiagnosticSeverity
{
    public const string Warning = "warning";
    public const string Error = "error";
}

public static class SemanticCatalogDiagnosticCodes
{
    public const string InvalidArtifactJson = "semantic_catalog.invalid_artifact_json";
    public const string InvalidTermId = "semantic_catalog.invalid_term_id";
    public const string UnknownTermKind = "semantic_catalog.unknown_term_kind";
    public const string InvalidRelation = "semantic_catalog.invalid_relation";
    public const string ConflictingTerm = "semantic_catalog.conflicting_term";
}
