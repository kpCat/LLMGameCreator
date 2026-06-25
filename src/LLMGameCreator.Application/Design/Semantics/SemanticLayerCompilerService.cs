using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticLayerCompilerService
{
    public const string RelativeOutputDirectory = ".llmgc/semantic";
    public const string CompiledJsonFileName = "compiled-semantic-pack.json";
    public const string CompiledMarkdownFileName = "compiled-semantic-pack-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly IReadOnlyDictionary<string, int> Precedence = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [SemanticLayerKinds.Core] = 1,
        [SemanticLayerKinds.Genre] = 2,
        [SemanticLayerKinds.Project] = 3
    };

    private static readonly IReadOnlySet<string> CandidateLayerKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        SemanticLayerKinds.ImportedCandidate,
        SemanticLayerKinds.LlmCandidate
    };

    private static readonly IReadOnlySet<string> ActiveStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        SemanticTermStatuses.Known
    };

    private static readonly IReadOnlySet<string> QuarantinedStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        SemanticTermStatuses.Candidate,
        SemanticTermStatuses.Deprecated,
        SemanticTermStatuses.Conflict,
        SemanticTermStatuses.Invalid
    };

    public SemanticLayerCompilerResult Compile(
        IEnumerable<SemanticLayerPack> layers,
        SemanticLayerCompilerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(layers);
        options ??= new SemanticLayerCompilerOptions();

        var orderedLayers = layers
            .OrderBy(item => LayerSortRank(item.LayerKind), Comparer<int>.Default)
            .ThenBy(item => item.LayerId, StringComparer.Ordinal)
            .ToList();
        var diagnostics = new List<SemanticCatalogDiagnostic>();
        var activeTerms = new Dictionary<string, CandidateTerm>(StringComparer.Ordinal);
        var quarantinedTerms = new Dictionary<string, SemanticCatalogTerm>(StringComparer.Ordinal);
        var activeRelations = new Dictionary<string, SemanticCatalogRelation>(StringComparer.Ordinal);
        var quarantinedRelations = new Dictionary<string, SemanticCatalogRelation>(StringComparer.Ordinal);
        var conflictedTermIds = new HashSet<string>(StringComparer.Ordinal);
        var conflictedRelationIds = new HashSet<string>(StringComparer.Ordinal);
        var layerSummaries = new List<SemanticLayerSummary>();

        foreach (var layer in orderedLayers)
        {
            if (!ValidateLayer(layer, diagnostics))
            {
                layerSummaries.Add(Summarize(layer, accepted: false));
                continue;
            }

            layerSummaries.Add(Summarize(layer, accepted: true));
            foreach (var declaration in layer.Terms)
            {
                if (!TryBuildTerm(layer, declaration, diagnostics, out var term))
                {
                    continue;
                }

                if (!IsActiveLayer(layer) || !ActiveStatuses.Contains(term.Status))
                {
                    quarantinedTerms[term.TermId] = MergeTerm(
                        quarantinedTerms.TryGetValue(term.TermId, out var quarantinedExisting) ? quarantinedExisting : null,
                        term);
                    continue;
                }

                if (conflictedTermIds.Contains(term.TermId))
                {
                    diagnostics.Add(Diagnostic(
                        SemanticDiagnosticSeverity.Error,
                        "semantic_layer.term_previously_conflicted",
                        layer.LayerId,
                        term.TermId,
                        "A term id already marked conflicted cannot re-enter the active compiled catalog."));
                    quarantinedTerms[term.TermId] = MergeTerm(
                        quarantinedTerms.TryGetValue(term.TermId, out var existingConflict) ? existingConflict : null,
                        term with { Status = SemanticTermStatuses.Conflict });
                    continue;
                }

                if (activeTerms.TryGetValue(term.TermId, out var activeExisting))
                {
                    if (IsIdenticalDeclaration(activeExisting.Term, term))
                    {
                        activeTerms[term.TermId] = activeExisting with
                        {
                            Term = MergeTerm(activeExisting.Term, term)
                        };
                        continue;
                    }

                    if (Precedence[layer.LayerKind] < activeExisting.Precedence)
                    {
                        diagnostics.Add(Diagnostic(
                            SemanticDiagnosticSeverity.Warning,
                            "semantic_layer.lower_precedence_override_ignored",
                            layer.LayerId,
                            term.TermId,
                            "A lower-precedence semantic declaration was ignored."));
                        continue;
                    }

                    if (Precedence[layer.LayerKind] == activeExisting.Precedence &&
                        !IsIdenticalDeclaration(activeExisting.Term, term))
                    {
                        diagnostics.Add(Diagnostic(
                            SemanticDiagnosticSeverity.Error,
                            "semantic_layer.term_conflict",
                            layer.LayerId,
                            term.TermId,
                            "Two same-precedence active declarations disagree."));
                        conflictedTermIds.Add(term.TermId);
                        quarantinedTerms[term.TermId] = MergeTerm(
                            activeExisting.Term with { Status = SemanticTermStatuses.Conflict },
                            term with { Status = SemanticTermStatuses.Conflict });
                        activeTerms.Remove(term.TermId);
                        continue;
                    }
                }

                activeTerms[term.TermId] = new CandidateTerm(term, Precedence[layer.LayerKind]);
            }
        }

        var knownIds = activeTerms.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var layer in orderedLayers.Where(layer => ValidateLayer(layer, diagnostics: null)))
        {
            foreach (var declaration in layer.Relations)
            {
                if (!TryBuildRelation(layer, declaration, diagnostics, out var relation))
                {
                    continue;
                }

                if (!IsActiveLayer(layer) || !ActiveStatuses.Contains(relation.Status))
                {
                    quarantinedRelations[relation.RelationId] = relation;
                    continue;
                }

                if (conflictedRelationIds.Contains(relation.RelationId))
                {
                    diagnostics.Add(Diagnostic(
                        SemanticDiagnosticSeverity.Error,
                        "semantic_layer.relation_previously_conflicted",
                        layer.LayerId,
                        relation.RelationId,
                        "A relation id already marked conflicted cannot re-enter the active compiled catalog."));
                    quarantinedRelations[relation.RelationId] = relation with { Status = SemanticTermStatuses.Conflict };
                    continue;
                }

                if (!ValidateRelationEndpoints(relation, knownIds, options.AcceptedExternalTargetIds, diagnostics))
                {
                    continue;
                }

                if (activeRelations.TryGetValue(relation.RelationId, out var existing))
                {
                    if (existing.SourceTermId == relation.SourceTermId &&
                        existing.RelationKind == relation.RelationKind &&
                        existing.TargetTermId == relation.TargetTermId)
                    {
                        activeRelations[relation.RelationId] = existing with
                        {
                            LayerIds = MergeStrings(existing.LayerIds, relation.LayerIds),
                            SourceArtifactIds = MergeStrings(existing.SourceArtifactIds, relation.SourceArtifactIds),
                            Tags = MergeStrings(existing.Tags, relation.Tags)
                        };
                    }
                    else
                    {
                        diagnostics.Add(Diagnostic(
                            SemanticDiagnosticSeverity.Error,
                            "semantic_layer.relation_conflict",
                            layer.LayerId,
                            relation.RelationId,
                            "Two active semantic relations share an id but disagree."));
                        conflictedRelationIds.Add(relation.RelationId);
                        quarantinedRelations[relation.RelationId] = relation with { Status = SemanticTermStatuses.Conflict };
                        activeRelations.Remove(relation.RelationId);
                    }

                    continue;
                }

                activeRelations[relation.RelationId] = relation;
            }
        }

        var compiledCatalog = new SemanticCatalog
        {
            SchemaVersion = "1",
            CatalogId = "compiled-semantic-pack",
            Terms = activeTerms.Values
                .Select(item => item.Term)
                .OrderBy(item => item.TermId, StringComparer.Ordinal)
                .ToList(),
            Relations = activeRelations.Values
                .OrderBy(item => item.RelationId, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
                .Distinct()
                .OrderBy(item => item.Severity, StringComparer.Ordinal)
                .ThenBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.SourceArtifactId, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ThenBy(item => item.Message, StringComparer.Ordinal)
                .ToList()
        };
        var resultWithoutHash = new SemanticLayerCompilerResult
        {
            Accepted = !compiledCatalog.Diagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error),
            Catalog = compiledCatalog,
            LayerSummaries = layerSummaries
                .OrderBy(item => item.LayerKind, StringComparer.Ordinal)
                .ThenBy(item => item.LayerId, StringComparer.Ordinal)
                .ToList(),
            QuarantinedTerms = quarantinedTerms.Values.OrderBy(item => item.TermId, StringComparer.Ordinal).ToList(),
            QuarantinedRelations = quarantinedRelations.Values.OrderBy(item => item.RelationId, StringComparer.Ordinal).ToList(),
            ActiveTermCount = compiledCatalog.Terms.Count,
            ActiveRelationCount = compiledCatalog.Relations.Count,
            QuarantinedTermCount = quarantinedTerms.Count,
            QuarantinedRelationCount = quarantinedRelations.Count
        };
        var hash = ComputeHash(JsonSerializer.Serialize(resultWithoutHash, JsonOptions));

        return resultWithoutHash with
        {
            CompiledCatalogHash = hash
        };
    }

    public async Task<SemanticLayerCompilerWriteResult> WriteAsync(
        string projectRootPath,
        SemanticLayerCompilerResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "semantic"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, CompiledJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, CompiledMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);

        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(result, JsonOptions), Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, RenderMarkdown(result), Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new SemanticLayerCompilerWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            CompiledJsonPath = jsonPath,
            CompiledMarkdownPath = markdownPath
        };
    }

    public SemanticLayerPackLoadResult LoadPacksFromDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return new SemanticLayerPackLoadResult();
        }

        var packs = new List<SemanticLayerPack>();
        var diagnostics = new List<SemanticCatalogDiagnostic>();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item, StringComparer.Ordinal))
        {
            var relativePath = Path.GetRelativePath(directoryPath, filePath).Replace('\\', '/');
            try
            {
                var pack = JsonSerializer.Deserialize<SemanticLayerPack>(File.ReadAllText(filePath), JsonOptions);
                if (pack == null)
                {
                    diagnostics.Add(Diagnostic(
                        SemanticDiagnosticSeverity.Error,
                        "semantic_layer.pack_json_empty",
                        relativePath,
                        relativePath,
                        "Semantic pack JSON did not contain an object."));
                    continue;
                }

                packs.Add(pack);
            }
            catch (JsonException)
            {
                diagnostics.Add(Diagnostic(
                    SemanticDiagnosticSeverity.Error,
                    "semantic_layer.pack_json_malformed",
                    relativePath,
                    relativePath,
                    "Semantic pack JSON is malformed."));
            }
        }

        return new SemanticLayerPackLoadResult
        {
            Packs = packs,
            Diagnostics = diagnostics
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.SourceArtifactId, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    public IReadOnlyList<SemanticLayerPack> ReadPacksFromDirectory(string directoryPath)
    {
        var result = LoadPacksFromDirectory(directoryPath);
        if (result.Diagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error))
        {
            throw new InvalidDataException("Semantic pack directory contains malformed pack files.");
        }

        return result.Packs;
    }

    private static bool ValidateLayer(SemanticLayerPack layer, ICollection<SemanticCatalogDiagnostic>? diagnostics)
    {
        var valid = true;
        if (!string.Equals(layer.SchemaVersion, "semantic_pack_contract_v1", StringComparison.Ordinal))
        {
            diagnostics?.Add(Diagnostic(
                SemanticDiagnosticSeverity.Error,
                "semantic_layer.invalid_schema_version",
                layer.LayerId,
                "schemaVersion",
                "Layer schemaVersion must be semantic_pack_contract_v1."));
            valid = false;
        }

        if (!TryNormalizeId(layer.LayerId, out _) || layer.LayerId.Split('/').Length != 2)
        {
            diagnostics?.Add(Diagnostic(
                SemanticDiagnosticSeverity.Error,
                "semantic_layer.invalid_layer_id",
                layer.LayerId,
                "layerId",
                "Layer id must be a safe two-segment id such as core/base."));
            valid = false;
        }
        else
        {
            var expectedKind = layer.LayerId[..layer.LayerId.IndexOf('/')];
            if (!string.Equals(expectedKind, layer.LayerKind, StringComparison.Ordinal))
            {
                diagnostics?.Add(Diagnostic(
                    SemanticDiagnosticSeverity.Error,
                    "semantic_layer.layer_kind_prefix_mismatch",
                    layer.LayerId,
                    "layerKind",
                    "Layer id prefix must match layer kind."));
                valid = false;
            }
        }

        if (!SemanticLayerKinds.Supported.Contains(layer.LayerKind))
        {
            diagnostics?.Add(Diagnostic(
                SemanticDiagnosticSeverity.Error,
                "semantic_layer.invalid_layer_kind",
                layer.LayerId,
                "layerKind",
                "Layer kind is not supported by semantic_pack_contract_v1."));
            valid = false;
        }

        if (!IsSafeSource(layer.Source))
        {
            diagnostics?.Add(Diagnostic(
                SemanticDiagnosticSeverity.Error,
                "semantic_layer.unsafe_source",
                layer.LayerId,
                "source",
                "Layer source/provenance must be relative descriptive text, not a rooted or traversal path."));
            valid = false;
        }

        return valid;
    }

    private static bool TryBuildTerm(
        SemanticLayerPack layer,
        SemanticLayerTermDeclaration declaration,
        ICollection<SemanticCatalogDiagnostic> diagnostics,
        out SemanticCatalogTerm term)
    {
        term = new SemanticCatalogTerm();
        if (!TryNormalizeId(declaration.TermId, out var termId))
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Error, "semantic_layer.invalid_term_id", layer.LayerId, declaration.TermId, "Semantic term id is unsafe."));
            return false;
        }

        var kind = FirstNonEmpty(declaration.Kind, InferKindFromId(termId));
        if (!TryNormalizeSegment(kind, out kind) || !SemanticTermKinds.Supported.Contains(kind))
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Error, "semantic_layer.invalid_term_kind", layer.LayerId, termId, "Semantic term kind is unsupported."));
            return false;
        }

        var status = NormalizeStatus(declaration.Status, layer, termId, diagnostics);
        term = new SemanticCatalogTerm
        {
            TermId = termId,
            Kind = kind,
            Label = FirstNonEmpty(declaration.Label, LabelFromId(termId)),
            Status = status,
            Aliases = NormalizeStringList(declaration.Aliases),
            Tags = NormalizeStringList(declaration.Tags),
            GenerationHints = NormalizeStringList(declaration.GenerationHints),
            Constraints = NormalizeStringList(declaration.Constraints),
            LayerIds = [layer.LayerId],
            SourceArtifactIds = [layer.LayerId],
            Notes = declaration.Notes.Trim()
        };

        return true;
    }

    private static bool TryBuildRelation(
        SemanticLayerPack layer,
        SemanticLayerRelationDeclaration declaration,
        ICollection<SemanticCatalogDiagnostic> diagnostics,
        out SemanticCatalogRelation relation)
    {
        relation = new SemanticCatalogRelation();
        if (!TryNormalizeId(declaration.SourceTermId, out var source) ||
            !TryNormalizeId(declaration.TargetTermId, out var target) ||
            !TryNormalizeSegment(declaration.RelationKind, out var kind) ||
            !SemanticRelationKinds.Supported.Contains(kind))
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Error, "semantic_layer.invalid_relation", layer.LayerId, declaration.RelationId, "Semantic relation source, kind or target is unsafe or unsupported."));
            return false;
        }

        var generatedId = $"relation/{source}/{kind}/{target}";
        if (!TryNormalizeId(FirstNonEmpty(declaration.RelationId, generatedId), out var relationId))
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Error, "semantic_layer.invalid_relation_id", layer.LayerId, declaration.RelationId, "Semantic relation id is unsafe."));
            return false;
        }

        relation = new SemanticCatalogRelation
        {
            RelationId = relationId,
            SourceTermId = source,
            RelationKind = kind,
            TargetTermId = target,
            Status = NormalizeStatus(declaration.Status, layer, relationId, diagnostics),
            Tags = NormalizeStringList(declaration.Tags),
            LayerIds = [layer.LayerId],
            SourceArtifactIds = [layer.LayerId]
        };
        return true;
    }

    private static bool ValidateRelationEndpoints(
        SemanticCatalogRelation relation,
        IReadOnlySet<string> knownIds,
        IReadOnlySet<string> acceptedExternalIds,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        var sourceKnown = knownIds.Contains(relation.SourceTermId);
        var targetKnown = knownIds.Contains(relation.TargetTermId) || acceptedExternalIds.Contains(relation.TargetTermId);
        if (sourceKnown && targetKnown)
        {
            return true;
        }

        diagnostics.Add(Diagnostic(
            SemanticDiagnosticSeverity.Error,
            "semantic_layer.unknown_relation_endpoint",
            string.Join(",", relation.LayerIds),
            relation.RelationId,
            "Semantic relation references an unknown source or target."));
        return false;
    }

    private static string NormalizeStatus(
        string rawStatus,
        SemanticLayerPack layer,
        string target,
        ICollection<SemanticCatalogDiagnostic> diagnostics)
    {
        if (!TryNormalizeSegment(rawStatus, out var status) || !SemanticTermStatuses.Supported.Contains(status))
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Error, "semantic_layer.invalid_status", layer.LayerId, target, "Semantic declaration status is unsupported."));
            return SemanticTermStatuses.Invalid;
        }

        if (CandidateLayerKinds.Contains(layer.LayerKind) && status == SemanticTermStatuses.Known)
        {
            diagnostics.Add(Diagnostic(SemanticDiagnosticSeverity.Warning, "semantic_layer.candidate_known_quarantined", layer.LayerId, target, "Candidate layers cannot make active known terms."));
            return SemanticTermStatuses.Candidate;
        }

        return status;
    }

    private static bool IsActiveLayer(SemanticLayerPack layer) => Precedence.ContainsKey(layer.LayerKind);

    private static bool IsIdenticalDeclaration(SemanticCatalogTerm first, SemanticCatalogTerm second) =>
        first.Kind == second.Kind &&
        first.Label == second.Label &&
        first.Status == second.Status &&
        SequenceEqual(first.Aliases, second.Aliases) &&
        SequenceEqual(first.Tags, second.Tags) &&
        SequenceEqual(first.GenerationHints, second.GenerationHints) &&
        SequenceEqual(first.Constraints, second.Constraints);

    private static SemanticCatalogTerm MergeTerm(SemanticCatalogTerm? existing, SemanticCatalogTerm incoming)
    {
        if (existing == null)
        {
            return incoming with
            {
                Aliases = NormalizeStringList(incoming.Aliases),
                Tags = NormalizeStringList(incoming.Tags),
                GenerationHints = NormalizeStringList(incoming.GenerationHints),
                Constraints = NormalizeStringList(incoming.Constraints),
                LayerIds = NormalizeStringList(incoming.LayerIds),
                SourceArtifactIds = NormalizeStringList(incoming.SourceArtifactIds)
            };
        }

        return existing with
        {
            Aliases = MergeStrings(existing.Aliases, incoming.Aliases),
            Tags = MergeStrings(existing.Tags, incoming.Tags),
            GenerationHints = MergeStrings(existing.GenerationHints, incoming.GenerationHints),
            Constraints = MergeStrings(existing.Constraints, incoming.Constraints),
            LayerIds = MergeStrings(existing.LayerIds, incoming.LayerIds),
            SourceArtifactIds = MergeStrings(existing.SourceArtifactIds, incoming.SourceArtifactIds),
            Notes = FirstNonEmpty(existing.Notes, incoming.Notes)
        };
    }

    private static SemanticLayerSummary Summarize(SemanticLayerPack layer, bool accepted) => new()
    {
        LayerId = layer.LayerId,
        LayerKind = layer.LayerKind,
        Source = layer.Source,
        Accepted = accepted,
        TermCount = layer.Terms.Count,
        RelationCount = layer.Relations.Count
    };

    private static int LayerSortRank(string layerKind) =>
        Precedence.TryGetValue(layerKind, out var rank) ? rank : CandidateLayerKinds.Contains(layerKind) ? 90 : 100;

    private static bool IsSafeSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 160)
        {
            return false;
        }

        return !Path.IsPathRooted(value) &&
               !value.Contains("../", StringComparison.Ordinal) &&
               !value.Contains("..\\", StringComparison.Ordinal) &&
               !value.Contains(':');
    }

    internal static bool TryNormalizeId(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 180 || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate.StartsWith('/') || candidate.EndsWith('/'))
        {
            return false;
        }

        var segments = candidate.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..") ||
            candidate.Any(character => character is not (>= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '/')))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    internal static bool TryNormalizeSegment(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 80 || value.Contains('/') || value.Contains('\\') || value.Contains(':'))
        {
            return false;
        }

        var candidate = NormalizeSpaces(value.Trim().ToLowerInvariant());
        if (candidate.Length == 0 || candidate is "." or ".." ||
            candidate.Any(character => character is not (>= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-')))
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

    private static IReadOnlyList<string> NormalizeStringList(IEnumerable<string> values) =>
        values
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> MergeStrings(IEnumerable<string> first, IEnumerable<string> second) =>
        NormalizeStringList(first.Concat(second));

    private static bool SequenceEqual(IReadOnlyList<string> first, IReadOnlyList<string> second) =>
        first.SequenceEqual(second, StringComparer.Ordinal);

    private static string InferKindFromId(string termId)
    {
        var separator = termId.IndexOf('/');
        return separator > 0 ? termId[..separator] : SemanticTermKinds.Unknown;
    }

    private static string LabelFromId(string termId)
    {
        var value = termId[(termId.LastIndexOf('/') + 1)..].Replace('_', ' ').Replace('-', ' ');
        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static SemanticCatalogDiagnostic Diagnostic(
        string severity,
        string code,
        string layerId,
        string target,
        string message) => new()
        {
            Severity = severity,
            Code = code,
            SourceArtifactId = layerId,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string RenderMarkdown(SemanticLayerCompilerResult result)
    {
        var lines = new List<string>
        {
            "# Compiled Semantic Pack",
            string.Empty,
            $"- Accepted: `{result.Accepted.ToString().ToLowerInvariant()}`",
            $"- Compiled catalog hash: `{result.CompiledCatalogHash}`",
            $"- Active terms: `{result.ActiveTermCount}`",
            $"- Active relations: `{result.ActiveRelationCount}`",
            $"- Quarantined terms: `{result.QuarantinedTermCount}`",
            $"- Quarantined relations: `{result.QuarantinedRelationCount}`",
            string.Empty,
            "## Layers",
            string.Empty
        };

        lines.AddRange(result.LayerSummaries.Select(item =>
            $"- `{item.LayerId}` kind=`{item.LayerKind}` accepted=`{item.Accepted.ToString().ToLowerInvariant()}` terms=`{item.TermCount}` relations=`{item.RelationCount}` source=`{item.Source}`"));
        lines.AddRange([string.Empty, "## Active Terms", string.Empty]);
        lines.AddRange(result.Catalog.Terms.Select(item =>
            $"- `{item.TermId}` kind=`{item.Kind}` layers=`{string.Join(", ", item.LayerIds)}` tags=`{string.Join(", ", item.Tags)}`"));
        lines.AddRange([string.Empty, "## Active Relations", string.Empty]);
        lines.AddRange(result.Catalog.Relations.Select(item =>
            $"- `{item.SourceTermId}` --`{item.RelationKind}`--> `{item.TargetTermId}` layers=`{string.Join(", ", item.LayerIds)}`"));
        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(result.Catalog.Diagnostics.Count == 0
            ? ["- None"]
            : result.Catalog.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` source=`{item.SourceArtifactId}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Compiled semantic output path must stay under the project root.");
        }
    }

    private sealed record CandidateTerm(SemanticCatalogTerm Term, int Precedence);
}

public sealed record SemanticLayerPack
{
    public string SchemaVersion { get; init; } = "semantic_pack_contract_v1";
    public string LayerId { get; init; } = string.Empty;
    public string LayerKind { get; init; } = SemanticLayerKinds.Project;
    public string Source { get; init; } = string.Empty;
    public IReadOnlyList<SemanticLayerTermDeclaration> Terms { get; init; } = Array.Empty<SemanticLayerTermDeclaration>();
    public IReadOnlyList<SemanticLayerRelationDeclaration> Relations { get; init; } = Array.Empty<SemanticLayerRelationDeclaration>();
}

public sealed record SemanticLayerTermDeclaration
{
    public string TermId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Status { get; init; } = SemanticTermStatuses.Known;
    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> GenerationHints { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = string.Empty;
}

public sealed record SemanticLayerRelationDeclaration
{
    public string RelationId { get; init; } = string.Empty;
    public string SourceTermId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string TargetTermId { get; init; } = string.Empty;
    public string Status { get; init; } = SemanticTermStatuses.Known;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed record SemanticLayerCompilerOptions
{
    public IReadOnlySet<string> AcceptedExternalTargetIds { get; init; } = new HashSet<string>(StringComparer.Ordinal);
}

public sealed record SemanticLayerCompilerResult
{
    public bool Accepted { get; init; }
    public string CompiledCatalogHash { get; init; } = string.Empty;
    public SemanticCatalog Catalog { get; init; } = new();
    public IReadOnlyList<SemanticLayerSummary> LayerSummaries { get; init; } = Array.Empty<SemanticLayerSummary>();
    public IReadOnlyList<SemanticCatalogTerm> QuarantinedTerms { get; init; } = Array.Empty<SemanticCatalogTerm>();
    public IReadOnlyList<SemanticCatalogRelation> QuarantinedRelations { get; init; } = Array.Empty<SemanticCatalogRelation>();
    public int ActiveTermCount { get; init; }
    public int ActiveRelationCount { get; init; }
    public int QuarantinedTermCount { get; init; }
    public int QuarantinedRelationCount { get; init; }
}

public sealed record SemanticLayerCompilerWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string CompiledJsonPath { get; init; } = string.Empty;
    public string CompiledMarkdownPath { get; init; } = string.Empty;
}

public sealed record SemanticLayerPackLoadResult
{
    public IReadOnlyList<SemanticLayerPack> Packs { get; init; } = Array.Empty<SemanticLayerPack>();
    public IReadOnlyList<SemanticCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticCatalogDiagnostic>();
}

public sealed record SemanticLayerSummary
{
    public string LayerId { get; init; } = string.Empty;
    public string LayerKind { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public int TermCount { get; init; }
    public int RelationCount { get; init; }
}

public static class SemanticLayerKinds
{
    public const string Core = "core";
    public const string Genre = "genre";
    public const string Project = "project";
    public const string ImportedCandidate = "imported_candidate";
    public const string LlmCandidate = "llm_candidate";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        Core,
        Genre,
        Project,
        ImportedCandidate,
        LlmCandidate
    };
}

public static class SemanticRelationKinds
{
    public const string Requires = "requires";
    public const string Excludes = "excludes";
    public const string Implies = "implies";
    public const string CompatibleWith = "compatible_with";
    public const string PreferredInTone = "preferred_in_tone";
    public const string ForbiddenInTone = "forbidden_in_tone";
    public const string PrefersQuestPattern = "prefers_quest_pattern";
    public const string PrefersDialogueIntent = "prefers_dialogue_intent";
    public const string PrefersInteractionFamily = "prefers_interaction_family";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        Requires,
        Excludes,
        Implies,
        CompatibleWith,
        PreferredInTone,
        ForbiddenInTone,
        PrefersQuestPattern,
        PrefersDialogueIntent,
        PrefersInteractionFamily
    };
}
