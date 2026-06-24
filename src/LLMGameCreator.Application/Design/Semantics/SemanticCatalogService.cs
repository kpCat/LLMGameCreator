using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticCatalogService
{
    public const string RelativeOutputDirectory = ".llmgc/semantic";
    public const string CatalogJsonFileName = "semantic-catalog.json";
    public const string CatalogMarkdownFileName = "semantic-catalog-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly IReadOnlyList<SemanticCatalogTerm> SeedTerms = BuildSeedTerms();
    private readonly SemanticCatalogMarkdownRenderer _markdownRenderer;

    public SemanticCatalogService(SemanticCatalogMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new SemanticCatalogMarkdownRenderer();
    }

    public SemanticCatalog Build(GeneratorPlanApprovedArtifactSet artifactSet)
    {
        ArgumentNullException.ThrowIfNull(artifactSet);

        var terms = SeedTerms.ToDictionary(term => term.TermId, StringComparer.OrdinalIgnoreCase);
        var relations = new Dictionary<string, SemanticCatalogRelation>(StringComparer.OrdinalIgnoreCase);
        var diagnostics = new List<SemanticCatalogDiagnostic>();

        foreach (var artifact in artifactSet.ApprovedArtifacts
                     .Where(IsSemanticPack)
                     .OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.ArtifactId, StringComparer.Ordinal))
        {
            ParseArtifact(artifact, terms, relations, diagnostics);
        }

        return new SemanticCatalog
        {
            Terms = terms.Values
                .OrderBy(term => term.TermId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(term => term.TermId, StringComparer.Ordinal)
                .ToList(),
            Relations = relations.Values
                .OrderBy(relation => relation.RelationId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(relation => relation.RelationId, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
                .Distinct()
                .OrderBy(diagnostic => diagnostic.Severity, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.SourceArtifactId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
                .ToList()
        };
    }

    public async Task<SemanticCatalogWriteResult> WriteAsync(
        string projectRootPath,
        SemanticCatalog catalog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "semantic"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, CatalogJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, CatalogMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);

        var json = JsonSerializer.Serialize(catalog, JsonOptions);
        await File.WriteAllTextAsync(jsonPath, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            markdownPath,
            _markdownRenderer.Render(catalog),
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return new SemanticCatalogWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            CatalogJsonPath = jsonPath,
            CatalogMarkdownPath = markdownPath
        };
    }

    private static bool IsSemanticPack(GeneratorPlanApprovedArtifact artifact) =>
        string.Equals(artifact.ArtifactKind, "semantic_pack_v1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(artifact.ExpectedArtifactContract, "semantic_pack_v1", StringComparison.OrdinalIgnoreCase);

    private static void ParseArtifact(
        GeneratorPlanApprovedArtifact artifact,
        IDictionary<string, SemanticCatalogTerm> terms,
        IDictionary<string, SemanticCatalogRelation> relations,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        try
        {
            using var document = JsonDocument.Parse(artifact.ContentJson);
            ParseContainer(document.RootElement, artifact.ArtifactId, terms, relations, diagnostics);
            if (TryGetProperty(document.RootElement, "semantic", out var semantic) &&
                semantic.ValueKind == JsonValueKind.Object)
            {
                ParseContainer(semantic, artifact.ArtifactId, terms, relations, diagnostics);
            }
        }
        catch (JsonException)
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Error,
                SemanticCatalogDiagnosticCodes.InvalidArtifactJson,
                "semantic_pack_v1 content is not valid JSON.",
                artifact.ArtifactId,
                "content_json"));
        }
    }

    private static void ParseContainer(
        JsonElement container,
        string artifactId,
        IDictionary<string, SemanticCatalogTerm> terms,
        IDictionary<string, SemanticCatalogRelation> relations,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (TryGetProperty(container, "terms", out var termArray) && termArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var term in termArray.EnumerateArray())
            {
                ParseTerm(term, null, artifactId, terms, diagnostics);
            }
        }

        if (TryGetProperty(container, "semantic_groups", out var groups) && groups.ValueKind == JsonValueKind.Array)
        {
            foreach (var group in groups.EnumerateArray())
            {
                if (group.ValueKind == JsonValueKind.Object &&
                    TryGetProperty(group, "terms", out var groupTerms) &&
                    groupTerms.ValueKind == JsonValueKind.Array)
                {
                    foreach (var term in groupTerms.EnumerateArray())
                    {
                        ParseTerm(term, null, artifactId, terms, diagnostics);
                    }
                }
            }
        }

        foreach (var pattern in PatternProperties)
        {
            if (!TryGetProperty(container, pattern.PropertyName, out var values) || values.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var value in values.EnumerateArray())
            {
                ParseTerm(value, pattern.Kind, artifactId, terms, diagnostics);
            }
        }

        if (TryGetProperty(container, "relations", out var relationArray) && relationArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var relation in relationArray.EnumerateArray())
            {
                ParseRelation(relation, artifactId, terms, relations, diagnostics);
            }
        }
    }

    private static void ParseTerm(
        JsonElement element,
        string? suggestedKind,
        string artifactId,
        IDictionary<string, SemanticCatalogTerm> terms,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var stringLabel = element.GetString() ?? string.Empty;
            AddStringTerm(stringLabel, suggestedKind, artifactId, terms, diagnostics);
            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidTermId,
                "Semantic term must be a string or object.",
                artifactId,
                element.GetRawText()));
            return;
        }

        var rawId = FirstNonEmpty(GetString(element, "id"), GetString(element, "termId"));
        var label = FirstNonEmpty(GetString(element, "label"), GetString(element, "title"));
        var rawKind = FirstNonEmpty(GetString(element, "kind"), suggestedKind ?? string.Empty, InferKindFromRawId(rawId));
        var kind = NormalizeKind(rawKind, artifactId, rawId, diagnostics);

        string termId;
        if (string.IsNullOrWhiteSpace(rawId))
        {
            if (!TryBuildTermId(kind, label, out termId))
            {
                diagnostics.Add(Diagnostic(
                    SemanticDiagnosticSeverity.Warning,
                    SemanticCatalogDiagnosticCodes.InvalidTermId,
                    "Semantic term requires a safe id or label.",
                    artifactId,
                    label));
                return;
            }
        }
        else if (!TryNormalizeId(rawId, out termId))
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidTermId,
                "Semantic term id is unsafe and was skipped.",
                artifactId,
                rawId));
            return;
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            label = LabelFromId(termId);
        }

        var status = NormalizeStatus(GetString(element, "status"));
        var aliases = ReadStringArray(element, "aliases");
        var notes = GetString(element, "notes").Trim();
        AddOrMergeTerm(terms, new SemanticCatalogTerm
        {
            TermId = termId,
            Kind = kind,
            Label = label.Trim(),
            Status = status,
            Aliases = aliases,
            SourceArtifactIds = SourceIds(artifactId),
            Notes = notes
        }, artifactId, diagnostics);
    }

    private static void AddStringTerm(
        string label,
        string? suggestedKind,
        string artifactId,
        IDictionary<string, SemanticCatalogTerm> terms,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        var kind = string.IsNullOrWhiteSpace(suggestedKind)
            ? SemanticTermKinds.Unknown
            : NormalizeKind(suggestedKind, artifactId, label, diagnostics);
        var knownId = FindKnownSeedId(label, kind);
        if (!string.IsNullOrWhiteSpace(knownId))
        {
            var seed = SeedTerms.First(term => string.Equals(term.TermId, knownId, StringComparison.OrdinalIgnoreCase));
            AddOrMergeTerm(terms, seed with { SourceArtifactIds = SourceIds(artifactId) }, artifactId, diagnostics);
            return;
        }

        if (!TryBuildTermId(kind, label, out var termId))
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidTermId,
                "Semantic string could not be normalized to a safe term id.",
                artifactId,
                label));
            return;
        }

        AddOrMergeTerm(terms, new SemanticCatalogTerm
        {
            TermId = termId,
            Kind = kind,
            Label = label.Trim(),
            Status = SemanticTermStatuses.Candidate,
            SourceArtifactIds = SourceIds(artifactId)
        }, artifactId, diagnostics);
    }

    private static void ParseRelation(
        JsonElement element,
        string artifactId,
        IDictionary<string, SemanticCatalogTerm> terms,
        IDictionary<string, SemanticCatalogRelation> relations,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidRelation,
                "Semantic relation must be an object.",
                artifactId,
                element.GetRawText()));
            return;
        }

        var rawSource = FirstNonEmpty(GetString(element, "source"), GetString(element, "sourceTermId"));
        var rawTarget = FirstNonEmpty(GetString(element, "target"), GetString(element, "targetTermId"));
        var rawKind = FirstNonEmpty(GetString(element, "kind"), GetString(element, "relationKind"));
        if (!TryNormalizeId(rawSource, out var source) ||
            !TryNormalizeId(rawTarget, out var target) ||
            !TryNormalizeSegment(rawKind, out var relationKind))
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidRelation,
                "Semantic relation source, kind, or target is unsafe and was skipped.",
                artifactId,
                FirstNonEmpty(rawSource, rawKind, rawTarget)));
            return;
        }

        EnsureRelationEndpoint(source, artifactId, terms, diagnostics);
        EnsureRelationEndpoint(target, artifactId, terms, diagnostics);

        var rawRelationId = FirstNonEmpty(GetString(element, "id"), GetString(element, "relationId"));
        var generatedId = $"relation/{source}/{relationKind}/{target}";
        if (!TryNormalizeId(string.IsNullOrWhiteSpace(rawRelationId) ? generatedId : rawRelationId, out var relationId))
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.InvalidRelation,
                "Semantic relation id is unsafe and was skipped.",
                artifactId,
                rawRelationId));
            return;
        }

        var incoming = new SemanticCatalogRelation
        {
            RelationId = relationId,
            SourceTermId = source,
            RelationKind = relationKind,
            TargetTermId = target,
            Status = NormalizeStatus(GetString(element, "status")),
            SourceArtifactIds = SourceIds(artifactId)
        };

        if (relations.TryGetValue(relationId, out var existing))
        {
            relations[relationId] = existing with
            {
                SourceArtifactIds = MergeStrings(existing.SourceArtifactIds, incoming.SourceArtifactIds)
            };
        }
        else
        {
            relations.Add(relationId, incoming);
        }
    }

    private static void EnsureRelationEndpoint(
        string termId,
        string artifactId,
        IDictionary<string, SemanticCatalogTerm> terms,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (terms.ContainsKey(termId))
        {
            return;
        }

        var kind = InferKindFromRawId(termId);
        kind = SemanticTermKinds.Supported.Contains(kind) ? kind : SemanticTermKinds.Unknown;
        AddOrMergeTerm(terms, new SemanticCatalogTerm
        {
            TermId = termId,
            Kind = kind,
            Label = LabelFromId(termId),
            Status = SemanticTermStatuses.Candidate,
            SourceArtifactIds = SourceIds(artifactId),
            Notes = "Introduced by semantic relation endpoint."
        }, artifactId, diagnostics);
    }

    private static void AddOrMergeTerm(
        IDictionary<string, SemanticCatalogTerm> terms,
        SemanticCatalogTerm incoming,
        string artifactId,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (!terms.TryGetValue(incoming.TermId, out var existing))
        {
            terms.Add(incoming.TermId, incoming with
            {
                Aliases = MergeStrings(incoming.Aliases, Array.Empty<string>()),
                SourceArtifactIds = MergeStrings(incoming.SourceArtifactIds, Array.Empty<string>())
            });
            return;
        }

        var conflictingKind = existing.Kind != incoming.Kind &&
                              existing.Kind != SemanticTermKinds.Unknown &&
                              incoming.Kind != SemanticTermKinds.Unknown;
        if (conflictingKind)
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.ConflictingTerm,
                $"Semantic term '{incoming.TermId}' has conflicting kinds '{existing.Kind}' and '{incoming.Kind}'.",
                artifactId,
                incoming.TermId));
        }

        terms[incoming.TermId] = existing with
        {
            Kind = existing.Kind == SemanticTermKinds.Unknown ? incoming.Kind : existing.Kind,
            Label = string.IsNullOrWhiteSpace(existing.Label) ? incoming.Label : existing.Label,
            Status = conflictingKind
                ? SemanticTermStatuses.Conflict
                : existing.Status == SemanticTermStatuses.Known
                    ? SemanticTermStatuses.Known
                    : incoming.Status,
            Aliases = MergeStrings(existing.Aliases, incoming.Aliases),
            SourceArtifactIds = MergeStrings(existing.SourceArtifactIds, incoming.SourceArtifactIds),
            Notes = FirstNonEmpty(existing.Notes, incoming.Notes)
        };
    }

    private static string NormalizeKind(
        string rawKind,
        string artifactId,
        string target,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (!TryNormalizeSegment(rawKind, out var normalized) || !SemanticTermKinds.Supported.Contains(normalized))
        {
            diagnostics.Add(Diagnostic(
                SemanticDiagnosticSeverity.Warning,
                SemanticCatalogDiagnosticCodes.UnknownTermKind,
                $"Unknown semantic term kind '{rawKind}' was normalized to 'unknown'.",
                artifactId,
                target));
            return SemanticTermKinds.Unknown;
        }

        return normalized;
    }

    private static string NormalizeStatus(string rawStatus)
    {
        return TryNormalizeSegment(rawStatus, out var normalized) && SemanticTermStatuses.Supported.Contains(normalized)
            ? normalized
            : SemanticTermStatuses.Candidate;
    }

    private static bool TryBuildTermId(string kind, string label, out string termId)
    {
        termId = string.Empty;
        if (!TryNormalizeSegment(label, out var labelSegment))
        {
            return false;
        }

        return TryNormalizeId($"{kind}/{labelSegment}", out termId);
    }

    internal static bool TryNormalizeId(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate.Length > 128 || candidate.StartsWith('/') || candidate.EndsWith('/'))
        {
            return false;
        }

        var segments = candidate.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..") ||
            candidate.Any(character => !IsSafeIdCharacter(character)))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeSegment(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 80 || value.Contains('/') || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate is "." or ".." ||
            candidate.Any(character => !IsSafeSegmentCharacter(character)))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static string NormalizeSpaces(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousUnderscore = false;
        foreach (var character in value)
        {
            var normalized = char.IsWhiteSpace(character) ? '_' : character;
            if (normalized == '_' && previousUnderscore)
            {
                continue;
            }

            builder.Append(normalized);
            previousUnderscore = normalized == '_';
        }

        return builder.ToString().Trim('_');
    }

    private static bool IsSafeIdCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/';

    private static bool IsSafeSegmentCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-';

    private static string FindKnownSeedId(string label, string kind)
    {
        if (!TryNormalizeSegment(label, out var segment))
        {
            return string.Empty;
        }

        var candidates = SeedTerms.Where(term =>
                string.Equals(term.TermId[(term.TermId.LastIndexOf('/') + 1)..], segment, StringComparison.OrdinalIgnoreCase) &&
                (kind == SemanticTermKinds.Unknown || string.Equals(term.Kind, kind, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        return candidates.Count == 1 ? candidates[0].TermId : string.Empty;
    }

    private static string InferKindFromRawId(string value)
    {
        var separator = value.IndexOf('/');
        return separator > 0 ? value[..separator].Trim().ToLowerInvariant() : string.Empty;
    }

    private static string LabelFromId(string termId)
    {
        var value = termId[(termId.LastIndexOf('/') + 1)..].Replace('_', ' ').Replace('-', ' ');
        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()?.Trim() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        var expected = propertyName.Replace("_", string.Empty, StringComparison.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            var actual = property.Name.Replace("_", string.Empty, StringComparison.Ordinal);
            if (string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static IReadOnlyList<string> MergeStrings(IEnumerable<string> first, IEnumerable<string> second) =>
        first.Concat(second)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> SourceIds(string artifactId) =>
        string.IsNullOrWhiteSpace(artifactId) ? Array.Empty<string>() : [artifactId.Trim()];

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static SemanticCatalogDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string artifactId,
        string target) => new()
        {
            Severity = severity,
            Code = code,
            Message = message,
            SourceArtifactId = artifactId,
            Target = target
        };

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Semantic output path must stay under the project root.");
        }
    }

    private static IReadOnlyList<SemanticCatalogTerm> BuildSeedTerms()
    {
        var ids = new[]
        {
            "theme/survival", "theme/exploration", "theme/mystery", "theme/political_intrigue",
            "theme/occult", "theme/trade", "theme/combat", "theme/crafting",
            "dialogue_intent/greet", "dialogue_intent/warn", "dialogue_intent/threaten",
            "dialogue_intent/bargain", "dialogue_intent/comfort", "dialogue_intent/reveal_secret",
            "dialogue_intent/ask_for_help", "dialogue_intent/give_quest",
            "item_affordance/edible", "item_affordance/tradable", "item_affordance/craft_material",
            "item_affordance/quest_item", "item_affordance/weapon", "item_affordance/tool",
            "item_affordance/consumable",
            "location_mood/safe", "location_mood/dangerous", "location_mood/isolated",
            "location_mood/sacred", "location_mood/ruined", "location_mood/busy",
            "asset_style_hint/portrait", "asset_style_hint/tile", "asset_style_hint/icon",
            "asset_style_hint/hand_painted", "asset_style_hint/low_poly",
            "audio_mood_hint/calm", "audio_mood_hint/tense", "audio_mood_hint/mysterious",
            "audio_mood_hint/combat"
        };

        return ids.Select(id => new SemanticCatalogTerm
            {
                TermId = id,
                Kind = id[..id.IndexOf('/')],
                Label = LabelFromId(id),
                Status = SemanticTermStatuses.Known
            })
            .OrderBy(term => term.TermId, StringComparer.Ordinal)
            .ToList();
    }

    private static readonly IReadOnlyList<(string PropertyName, string Kind)> PatternProperties =
    [
        ("themes", SemanticTermKinds.Theme),
        ("tones", SemanticTermKinds.Tone),
        ("biomes", SemanticTermKinds.Biome),
        ("factions", SemanticTermKinds.Faction),
        ("factionRelations", SemanticTermKinds.FactionRelation),
        ("npcArchetypes", SemanticTermKinds.NpcArchetype),
        ("dialogueIntents", SemanticTermKinds.DialogueIntent),
        ("questMotifs", SemanticTermKinds.QuestMotif),
        ("itemAffordances", SemanticTermKinds.ItemAffordance),
        ("locationMoods", SemanticTermKinds.LocationMood),
        ("assetStyleHints", SemanticTermKinds.AssetStyleHint),
        ("audioMoodHints", SemanticTermKinds.AudioMoodHint),
        ("entityRoles", SemanticTermKinds.EntityRole)
    ];
}
