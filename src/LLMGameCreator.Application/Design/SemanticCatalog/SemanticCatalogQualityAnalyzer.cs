namespace LLMGameCreator.Application.Design.SemanticCatalog;

public sealed class SemanticCatalogQualityAnalyzer
{
    private static readonly IReadOnlySet<string> ImportantCoverageKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        SemanticCatalogConceptKinds.Archetype,
        SemanticCatalogConceptKinds.NpcArchetype,
        SemanticCatalogConceptKinds.DialogueIntent,
        SemanticCatalogConceptKinds.QuestMotif,
        SemanticCatalogConceptKinds.Theme
    };

    private static readonly IReadOnlySet<string> ImportantAlternateLabelKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        SemanticCatalogConceptKinds.Archetype,
        SemanticCatalogConceptKinds.NpcArchetype,
        SemanticCatalogConceptKinds.DialogueIntent,
        SemanticCatalogConceptKinds.QuestMotif
    };

    public SemanticCatalogQualityReport Analyze(CompiledSemanticCatalog? catalog, SemanticCatalogQualityProfile? profile = null)
    {
        if (catalog == null)
        {
            var missingCatalogDiagnostics = new[]
            {
                Diagnostic(
                    SemanticCatalogDiagnosticSeverity.Error,
                    SemanticCatalogQualityDiagnosticCodes.CatalogMissing,
                    "catalog",
                    "Compiled semantic catalog is required.")
            };

            return new SemanticCatalogQualityReport
            {
                Accepted = false,
                Diagnostics = missingCatalogDiagnostics,
                Metrics = Array.Empty<SemanticCatalogQualityMetric>(),
                LookupSmokeResults = Array.Empty<SemanticCatalogLookupSmokeResult>()
            };
        }

        profile ??= new SemanticCatalogQualityProfile();
        var concepts = catalog.Concepts
            .Where(concept => !string.IsNullOrWhiteSpace(concept.Id))
            .OrderBy(concept => concept.Id, StringComparer.Ordinal)
            .ThenBy(concept => concept.PreferredLabel, StringComparer.Ordinal)
            .ToList();
        var conceptsById = concepts
            .GroupBy(concept => concept.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var diagnostics = new List<SemanticCatalogQualityDiagnostic>();
        AddCoverageDiagnostics(concepts, diagnostics);
        AddFacetDiagnostics(concepts, diagnostics);
        AddOrphanDiagnostics(concepts, diagnostics);
        AddRelatedIntegrityDiagnostics(concepts, conceptsById, diagnostics);
        AddProfileDiagnostics(profile, concepts, diagnostics);
        var smokeResults = RunLookupSmokeExpectations(catalog, profile, diagnostics);
        var sortedDiagnostics = SortDiagnostics(diagnostics);

        return new SemanticCatalogQualityReport
        {
            Accepted = sortedDiagnostics.All(diagnostic => diagnostic.Severity != SemanticCatalogDiagnosticSeverity.Error),
            Diagnostics = sortedDiagnostics,
            Metrics = BuildMetrics(concepts),
            LookupSmokeResults = smokeResults
        };
    }

    private static IReadOnlyList<SemanticCatalogQualityMetric> BuildMetrics(IReadOnlyList<CompiledSemanticCatalogConcept> concepts)
    {
        var metrics = new List<SemanticCatalogQualityMetric>
        {
            Metric("broader_relation_count", "total", concepts.Sum(concept => concept.BroaderIds.Count)),
            Metric("concept_count", "total", concepts.Count),
            Metric("concept_count_with_alternate_labels", "total", concepts.Count(concept => concept.AlternateLabels.Count > 0)),
            Metric("concept_count_with_description", "total", concepts.Count(concept => !string.IsNullOrWhiteSpace(concept.Description))),
            Metric("concept_count_with_facets", "total", concepts.Count(concept => concept.Facets.Count > 0)),
            Metric("concept_count_with_tags", "total", concepts.Count(concept => concept.Tags.Count > 0)),
            Metric("narrower_relation_count", "total", concepts.Sum(concept => concept.NarrowerIds.Count)),
            Metric("orphan_concept_count", "total", concepts.Count(IsOrphan)),
            Metric("related_relation_count", "total", concepts.Sum(concept => concept.RelatedIds.Count))
        };

        metrics.AddRange(concepts
            .GroupBy(concept => concept.Kind, StringComparer.Ordinal)
            .Select(group => Metric("concept_count_by_kind", group.Key, group.Count())));

        metrics.AddRange(concepts
            .SelectMany(concept => concept.Tags)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.Ordinal)
            .Select(group => Metric("top_tag_count", group.Key, group.Count())));

        metrics.AddRange(concepts
            .SelectMany(concept => concept.Facets)
            .Select(FacetKey)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.Ordinal)
            .Select(group => Metric("top_facet_key_count", group.Key, group.Count())));

        return metrics
            .OrderBy(metric => metric.Name, StringComparer.Ordinal)
            .ThenBy(metric => metric.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static void AddCoverageDiagnostics(
        IReadOnlyList<CompiledSemanticCatalogConcept> concepts,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        foreach (var concept in concepts)
        {
            if (ImportantCoverageKinds.Contains(concept.Kind) && string.IsNullOrWhiteSpace(concept.Description))
            {
                diagnostics.Add(Warning(
                    SemanticCatalogQualityDiagnosticCodes.MissingDescription,
                    concept.Id,
                    "Important semantic catalog concept should include a description."));
            }

            if (ImportantAlternateLabelKinds.Contains(concept.Kind) && concept.AlternateLabels.Count == 0)
            {
                diagnostics.Add(Warning(
                    SemanticCatalogQualityDiagnosticCodes.MissingAlternateLabels,
                    concept.Id,
                    "Important semantic catalog concept should include alternateLabels for semantic variation."));
            }

            if (ImportantCoverageKinds.Contains(concept.Kind) && concept.Tags.Count == 0)
            {
                diagnostics.Add(Warning(
                    SemanticCatalogQualityDiagnosticCodes.MissingTags,
                    concept.Id,
                    "Important semantic catalog concept should include tags."));
            }

            if (ImportantCoverageKinds.Contains(concept.Kind) && concept.Facets.Count == 0)
            {
                diagnostics.Add(Warning(
                    SemanticCatalogQualityDiagnosticCodes.MissingFacets,
                    concept.Id,
                    "Important semantic catalog concept should include facets."));
            }
        }
    }

    private static void AddFacetDiagnostics(
        IReadOnlyList<CompiledSemanticCatalogConcept> concepts,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        foreach (var concept in concepts)
        {
            foreach (var facet in concept.Facets.OrderBy(value => value, StringComparer.Ordinal))
            {
                if (IsValidFacet(facet))
                {
                    continue;
                }

                diagnostics.Add(Warning(
                    SemanticCatalogQualityDiagnosticCodes.InvalidFacetSyntax,
                    concept.Id,
                    $"Facet '{facet}' should use non-empty key:value syntax."));
            }
        }
    }

    private static void AddOrphanDiagnostics(
        IReadOnlyList<CompiledSemanticCatalogConcept> concepts,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        foreach (var concept in concepts.Where(IsOrphan))
        {
            diagnostics.Add(Warning(
                SemanticCatalogQualityDiagnosticCodes.OrphanConcept,
                concept.Id,
                "Concept has no relations, tags or facets."));
        }
    }

    private static void AddRelatedIntegrityDiagnostics(
        IReadOnlyList<CompiledSemanticCatalogConcept> concepts,
        IReadOnlyDictionary<string, CompiledSemanticCatalogConcept> conceptsById,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        foreach (var concept in concepts)
        {
            foreach (var relatedId in concept.RelatedIds.OrderBy(id => id, StringComparer.Ordinal))
            {
                if (!conceptsById.TryGetValue(relatedId, out var related))
                {
                    continue;
                }

                var target = $"{concept.Id}->{relatedId}";
                if (!related.RelatedIds.Contains(concept.Id, StringComparer.Ordinal))
                {
                    diagnostics.Add(Warning(
                        SemanticCatalogQualityDiagnosticCodes.AsymmetricRelatedIds,
                        target,
                        "relatedIds should be symmetric between catalog concepts."));
                }

                if (HasDirectHierarchyRelation(concept, related))
                {
                    diagnostics.Add(Warning(
                        SemanticCatalogQualityDiagnosticCodes.RelatedDuplicatesDirectHierarchy,
                        target,
                        "relatedIds should not duplicate a direct broader/narrower relation."));
                    continue;
                }

                if (HasTransitiveHierarchyRelation(concept.Id, relatedId, conceptsById))
                {
                    diagnostics.Add(Warning(
                        SemanticCatalogQualityDiagnosticCodes.RelatedConflictsWithTransitiveHierarchy,
                        target,
                        "relatedIds should not conflict with transitive broader/narrower hierarchy."));
                }
            }
        }
    }

    private static void AddProfileDiagnostics(
        SemanticCatalogQualityProfile profile,
        IReadOnlyList<CompiledSemanticCatalogConcept> concepts,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        var kindCounts = concepts
            .GroupBy(concept => concept.Kind, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        foreach (var requiredKind in NormalizeKinds(profile.RequiredKinds))
        {
            if (!kindCounts.ContainsKey(requiredKind))
            {
                diagnostics.Add(Diagnostic(
                    SemanticCatalogDiagnosticSeverity.Error,
                    SemanticCatalogQualityDiagnosticCodes.RequiredKindMissing,
                    requiredKind,
                    "Required semantic catalog concept kind is missing."));
            }
        }

        foreach (var minimum in NormalizeMinimums(profile.MinimumConceptsByKind))
        {
            var actual = kindCounts.TryGetValue(minimum.Key, out var count) ? count : 0;
            if (actual < minimum.Value)
            {
                diagnostics.Add(Diagnostic(
                    SemanticCatalogDiagnosticSeverity.Error,
                    SemanticCatalogQualityDiagnosticCodes.MinimumConceptCountByKindNotSatisfied,
                    minimum.Key,
                    $"Concept kind '{minimum.Key}' requires at least {minimum.Value} concepts but has {actual}."));
            }
        }
    }

    private static IReadOnlyList<SemanticCatalogLookupSmokeResult> RunLookupSmokeExpectations(
        CompiledSemanticCatalog catalog,
        SemanticCatalogQualityProfile profile,
        ICollection<SemanticCatalogQualityDiagnostic> diagnostics)
    {
        if (profile.LookupSmokeExpectations.Count == 0)
        {
            return Array.Empty<SemanticCatalogLookupSmokeResult>();
        }

        var index = new SemanticCatalogLookupIndex(catalog);
        var results = new List<SemanticCatalogLookupSmokeResult>();
        foreach (var expectation in profile.LookupSmokeExpectations.OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            var expectedIds = expectation.ExpectedConceptIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            var actualIds = index.Lookup(expectation.Query)
                .Select(result => result.ConceptId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            var actualSet = actualIds.ToHashSet(StringComparer.Ordinal);
            var missingIds = expectedIds
                .Where(id => !actualSet.Contains(id))
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            var passed = expectation.RequireAllExpectedConceptIds
                ? expectedIds.Count > 0 && missingIds.Count == 0
                : expectedIds.Any(actualSet.Contains);

            var result = new SemanticCatalogLookupSmokeResult
            {
                Name = expectation.Name,
                QueryText = expectation.Query.QueryText,
                ExpectedConceptIds = expectedIds,
                ActualConceptIds = actualIds,
                MissingExpectedConceptIds = missingIds,
                Passed = passed
            };
            results.Add(result);

            if (!passed)
            {
                diagnostics.Add(Diagnostic(
                    SemanticCatalogDiagnosticSeverity.Error,
                    SemanticCatalogQualityDiagnosticCodes.LookupSmokeExpectationFailed,
                    expectation.Name,
                    $"Lookup smoke expectation '{expectation.Name}' did not return the expected concept ids."));
            }
        }

        return results
            .OrderBy(result => result.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static bool HasDirectHierarchyRelation(CompiledSemanticCatalogConcept concept, CompiledSemanticCatalogConcept related) =>
        concept.BroaderIds.Contains(related.Id, StringComparer.Ordinal) ||
        concept.NarrowerIds.Contains(related.Id, StringComparer.Ordinal) ||
        related.BroaderIds.Contains(concept.Id, StringComparer.Ordinal) ||
        related.NarrowerIds.Contains(concept.Id, StringComparer.Ordinal);

    private static bool HasTransitiveHierarchyRelation(
        string conceptId,
        string relatedId,
        IReadOnlyDictionary<string, CompiledSemanticCatalogConcept> conceptsById) =>
        ReachableThroughBroader(conceptId, relatedId, conceptsById) ||
        ReachableThroughBroader(relatedId, conceptId, conceptsById);

    private static bool ReachableThroughBroader(
        string startId,
        string targetId,
        IReadOnlyDictionary<string, CompiledSemanticCatalogConcept> conceptsById)
    {
        if (!conceptsById.TryGetValue(startId, out var start))
        {
            return false;
        }

        var frontier = new Queue<string>(start.BroaderIds.OrderBy(id => id, StringComparer.Ordinal));
        var visited = new HashSet<string>(StringComparer.Ordinal) { startId };
        while (frontier.Count > 0)
        {
            var currentId = frontier.Dequeue();
            if (!visited.Add(currentId))
            {
                continue;
            }

            if (string.Equals(currentId, targetId, StringComparison.Ordinal))
            {
                return true;
            }

            if (!conceptsById.TryGetValue(currentId, out var current))
            {
                continue;
            }

            foreach (var broaderId in current.BroaderIds.OrderBy(id => id, StringComparer.Ordinal))
            {
                frontier.Enqueue(broaderId);
            }
        }

        return false;
    }

    private static IReadOnlyList<SemanticCatalogQualityDiagnostic> SortDiagnostics(IEnumerable<SemanticCatalogQualityDiagnostic> diagnostics) =>
        diagnostics
            .Distinct()
            .OrderBy(diagnostic => SeverityRank(diagnostic.Severity), Comparer<int>.Default)
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> NormalizeKinds(IEnumerable<string> kinds) =>
        kinds
            .Select(NormalizeKind)
            .Where(kind => !string.IsNullOrWhiteSpace(kind))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(kind => kind, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyDictionary<string, int> NormalizeMinimums(IReadOnlyDictionary<string, int> minimums) =>
        minimums
            .Select(pair => new KeyValuePair<string, int>(NormalizeKind(pair.Key), pair.Value))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Max(pair => pair.Value), StringComparer.Ordinal);

    private static string NormalizeKind(string kind) =>
        TryNormalizeSegment(kind, out var normalized) ? normalized : string.Empty;

    private static bool TryNormalizeSegment(string value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var builder = new System.Text.StringBuilder();
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if ((character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9'))
            {
                builder.Append(character);
            }
            else if (character is '_' or '-' or ' ' or '.')
            {
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }
            }
            else
            {
                return false;
            }
        }

        normalized = builder.ToString().Trim('_');
        return !string.IsNullOrWhiteSpace(normalized);
    }

    private static string FacetKey(string facet)
    {
        var separatorIndex = facet.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex <= 0 ? string.Empty : facet[..separatorIndex].Trim();
    }

    private static bool IsValidFacet(string facet)
    {
        var separatorIndex = facet.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex > 0 &&
               separatorIndex < facet.Length - 1 &&
               !string.IsNullOrWhiteSpace(facet[..separatorIndex]) &&
               !string.IsNullOrWhiteSpace(facet[(separatorIndex + 1)..]);
    }

    private static bool IsOrphan(CompiledSemanticCatalogConcept concept) =>
        concept.BroaderIds.Count == 0 &&
        concept.NarrowerIds.Count == 0 &&
        concept.RelatedIds.Count == 0 &&
        concept.Tags.Count == 0 &&
        concept.Facets.Count == 0;

    private static SemanticCatalogQualityMetric Metric(string name, string key, int value) => new()
    {
        Name = name,
        Key = key,
        Value = value
    };

    private static SemanticCatalogQualityDiagnostic Warning(string code, string target, string message) =>
        Diagnostic(SemanticCatalogDiagnosticSeverity.Warning, code, target, message);

    private static SemanticCatalogQualityDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static int SeverityRank(string severity) => severity switch
    {
        SemanticCatalogDiagnosticSeverity.Error => 0,
        SemanticCatalogDiagnosticSeverity.Warning => 1,
        _ => 2
    };
}

public sealed record SemanticCatalogQualityProfile
{
    public IReadOnlyList<string> RequiredKinds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, int> MinimumConceptsByKind { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<SemanticCatalogLookupSmokeExpectation> LookupSmokeExpectations { get; init; } = Array.Empty<SemanticCatalogLookupSmokeExpectation>();
}

public sealed record SemanticCatalogQualityReport
{
    public bool Accepted { get; init; }
    public IReadOnlyList<SemanticCatalogQualityDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticCatalogQualityDiagnostic>();
    public IReadOnlyList<SemanticCatalogQualityMetric> Metrics { get; init; } = Array.Empty<SemanticCatalogQualityMetric>();
    public IReadOnlyList<SemanticCatalogLookupSmokeResult> LookupSmokeResults { get; init; } = Array.Empty<SemanticCatalogLookupSmokeResult>();
}

public sealed record SemanticCatalogQualityMetric
{
    public string Name { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int Value { get; init; }
}

public sealed record SemanticCatalogQualityDiagnostic
{
    public string Severity { get; init; } = SemanticCatalogDiagnosticSeverity.Warning;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record SemanticCatalogLookupSmokeExpectation
{
    public string Name { get; init; } = string.Empty;
    public SemanticCatalogLookupQuery Query { get; init; } = new();
    public IReadOnlyList<string> ExpectedConceptIds { get; init; } = Array.Empty<string>();
    public bool RequireAllExpectedConceptIds { get; init; } = true;
}

public sealed record SemanticCatalogLookupSmokeResult
{
    public string Name { get; init; } = string.Empty;
    public string QueryText { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedConceptIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActualConceptIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingExpectedConceptIds { get; init; } = Array.Empty<string>();
    public bool Passed { get; init; }
}

public static class SemanticCatalogQualityDiagnosticCodes
{
    public const string CatalogMissing = "semantic_catalog_quality.catalog.missing";
    public const string MissingDescription = "semantic_catalog_quality.coverage.description_missing";
    public const string MissingAlternateLabels = "semantic_catalog_quality.coverage.alternate_labels_missing";
    public const string MissingTags = "semantic_catalog_quality.coverage.tags_missing";
    public const string MissingFacets = "semantic_catalog_quality.coverage.facets_missing";
    public const string InvalidFacetSyntax = "semantic_catalog_quality.facet.syntax_invalid";
    public const string OrphanConcept = "semantic_catalog_quality.concept.orphan";
    public const string AsymmetricRelatedIds = "semantic_catalog_quality.related.asymmetric";
    public const string RelatedDuplicatesDirectHierarchy = "semantic_catalog_quality.related.duplicates_direct_hierarchy";
    public const string RelatedConflictsWithTransitiveHierarchy = "semantic_catalog_quality.related.conflicts_transitive_hierarchy";
    public const string RequiredKindMissing = "semantic_catalog_quality.profile.required_kind_missing";
    public const string MinimumConceptCountByKindNotSatisfied = "semantic_catalog_quality.profile.minimum_concept_count_by_kind_not_satisfied";
    public const string LookupSmokeExpectationFailed = "semantic_catalog_quality.lookup_smoke.failed";
}

public sealed class SemanticCatalogLookupIndex
{
    private readonly IReadOnlyList<CompiledSemanticCatalogConcept> _concepts;

    public SemanticCatalogLookupIndex(CompiledSemanticCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _concepts = catalog.Concepts
            .OrderBy(concept => concept.Id, StringComparer.Ordinal)
            .ToList();
    }

    public IReadOnlyList<SemanticCatalogLookupResult> Lookup(SemanticCatalogLookupQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var normalizedQuery = NormalizeLookupText(query.QueryText);
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Array.Empty<SemanticCatalogLookupResult>();
        }

        return _concepts
            .Select(concept => new
            {
                Concept = concept,
                Score = Score(concept, normalizedQuery)
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Concept.Id, StringComparer.Ordinal)
            .Take(query.Limit <= 0 ? 10 : query.Limit)
            .Select(item => new SemanticCatalogLookupResult
            {
                ConceptId = item.Concept.Id,
                Score = item.Score
            })
            .ToList();
    }

    private static int Score(CompiledSemanticCatalogConcept concept, string normalizedQuery)
    {
        if (NormalizeLookupText(concept.Id) == normalizedQuery ||
            NormalizeLookupText(concept.PreferredLabel) == normalizedQuery)
        {
            return 100;
        }

        if (concept.AlternateLabels.Any(label => NormalizeLookupText(label) == normalizedQuery))
        {
            return 90;
        }

        if (concept.Tags.Any(tag => NormalizeLookupText(tag) == normalizedQuery) ||
            concept.Facets.Any(facet => NormalizeLookupText(facet).Contains(normalizedQuery, StringComparison.Ordinal)))
        {
            return 50;
        }

        if (NormalizeLookupText(concept.PreferredLabel).Contains(normalizedQuery, StringComparison.Ordinal) ||
            concept.AlternateLabels.Any(label => NormalizeLookupText(label).Contains(normalizedQuery, StringComparison.Ordinal)))
        {
            return 25;
        }

        return 0;
    }

    private static string NormalizeLookupText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder();
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != ' ')
            {
                builder.Append(' ');
            }
        }

        return builder.ToString().Trim();
    }
}

public sealed record SemanticCatalogLookupQuery
{
    public string QueryText { get; init; } = string.Empty;
    public int Limit { get; init; } = 10;
}

public sealed record SemanticCatalogLookupResult
{
    public string ConceptId { get; init; } = string.Empty;
    public int Score { get; init; }
}

public sealed record CompiledSemanticCatalog
{
    public string CatalogId { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public IReadOnlyList<CompiledSemanticCatalogConcept> Concepts { get; init; } = Array.Empty<CompiledSemanticCatalogConcept>();
}

public sealed record CompiledSemanticCatalogConcept
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string PreferredLabel { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> AlternateLabels { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BroaderIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> NarrowerIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RelatedIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Facets { get; init; } = Array.Empty<string>();
}

public static class SemanticCatalogConceptKinds
{
    public const string Archetype = "archetype";
    public const string NpcArchetype = "npc_archetype";
    public const string DialogueIntent = "dialogue_intent";
    public const string QuestMotif = "quest_motif";
    public const string Theme = "theme";
    public const string Tag = "tag";
}

public static class SemanticCatalogDiagnosticSeverity
{
    public const string Error = "error";
    public const string Warning = "warning";
    public const string Info = "info";
}
