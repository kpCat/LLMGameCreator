using LLMGameCreator.Application.Design.SemanticCatalog;
using Xunit;

namespace LLMGameCreator.Tests.Application.CandidateSemanticCatalog;

public sealed class SemanticCatalogQualityAnalyzerTests
{
    [Fact]
    public void ValidCompactCatalogProducesDeterministicMetricsAndAcceptedReport()
    {
        var analyzer = new SemanticCatalogQualityAnalyzer();
        var catalog = ValidCatalog();

        var first = analyzer.Analyze(catalog);
        var second = analyzer.Analyze(catalog);

        Assert.True(first.Accepted, Join(first.Diagnostics));
        Assert.Empty(first.Diagnostics);
        Assert.Equal(first.Metrics, second.Metrics);
        AssertMetric(first, "concept_count", "total", 6);
        AssertMetric(first, "concept_count_by_kind", SemanticCatalogConceptKinds.Archetype, 2);
        AssertMetric(first, "broader_relation_count", "total", 1);
        AssertMetric(first, "narrower_relation_count", "total", 1);
        AssertMetric(first, "related_relation_count", "total", 2);
        AssertMetric(first, "orphan_concept_count", "total", 0);
        AssertMetric(first, "top_tag_count", "frontier", 2);
        AssertMetric(first, "top_facet_key_count", "role", 3);
    }

    [Fact]
    public void MissingDescriptionAlternateLabelsTagsAndFacetsProduceWarnings()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("archetype/ranger", SemanticCatalogConceptKinds.Archetype, "Ranger")));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.All(report.Diagnostics, diagnostic => Assert.Equal(SemanticCatalogDiagnosticSeverity.Warning, diagnostic.Severity));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.MissingDescription);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.MissingAlternateLabels);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.MissingTags);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.MissingFacets);
    }

    [Fact]
    public void InvalidFacetSyntaxProducesDiagnostic()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("tag/frontier", SemanticCatalogConceptKinds.Tag, "Frontier", tags: ["frontier"], facets: ["role:"])));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.InvalidFacetSyntax);
    }

    [Fact]
    public void OrphanConceptProducesDiagnostic()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("tag/lonely", SemanticCatalogConceptKinds.Tag, "Lonely")));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.OrphanConcept);
    }

    [Fact]
    public void AsymmetricRelatedIdsProducesDiagnostic()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("archetype/ranger", SemanticCatalogConceptKinds.Archetype, "Ranger", related: ["theme/frontier"], description: "Tracks frontier paths.", alternate: ["Guide"], tags: ["frontier"], facets: ["role:navigator"]),
            Concept("theme/frontier", SemanticCatalogConceptKinds.Theme, "Frontier", description: "Frontier theme.", tags: ["frontier"], facets: ["theme:frontier"])));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.AsymmetricRelatedIds);
    }

    [Fact]
    public void RelatedVsDirectBroaderConflictProducesDiagnostic()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("archetype/ranger", SemanticCatalogConceptKinds.Archetype, "Ranger", broader: ["archetype/survival_actor"], related: ["archetype/survival_actor"], description: "Tracks frontier paths.", alternate: ["Guide"], tags: ["frontier"], facets: ["role:navigator"]),
            Concept("archetype/survival_actor", SemanticCatalogConceptKinds.Archetype, "Survival Actor", narrower: ["archetype/ranger"], related: ["archetype/ranger"], description: "Survival actor group.", alternate: ["Explorer"], tags: ["survival"], facets: ["role:actor"])));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.RelatedDuplicatesDirectHierarchy);
    }

    [Fact]
    public void RelatedVsTransitiveHierarchyConflictProducesDiagnostic()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("archetype/grandchild", SemanticCatalogConceptKinds.Archetype, "Grandchild", broader: ["archetype/child"], related: ["archetype/root"], description: "Specific actor.", alternate: ["Specific"], tags: ["frontier"], facets: ["role:specific"]),
            Concept("archetype/child", SemanticCatalogConceptKinds.Archetype, "Child", broader: ["archetype/root"], narrower: ["archetype/grandchild"], description: "Middle actor.", alternate: ["Middle"], tags: ["frontier"], facets: ["role:middle"]),
            Concept("archetype/root", SemanticCatalogConceptKinds.Archetype, "Root", narrower: ["archetype/child"], related: ["archetype/grandchild"], description: "Root actor.", alternate: ["Base"], tags: ["frontier"], facets: ["role:root"])));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.RelatedConflictsWithTransitiveHierarchy);
    }

    [Fact]
    public void RequiredKindMissingProducesErrorAndAcceptedFalse()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(
            Catalog(Concept("archetype/ranger", SemanticCatalogConceptKinds.Archetype, "Ranger", description: "Tracks paths.", alternate: ["Guide"], tags: ["frontier"], facets: ["role:navigator"])),
            new SemanticCatalogQualityProfile { RequiredKinds = [SemanticCatalogConceptKinds.QuestMotif] });

        Assert.False(report.Accepted);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.RequiredKindMissing && diagnostic.Severity == SemanticCatalogDiagnosticSeverity.Error);
    }

    [Fact]
    public void MinimumConceptCountByKindProducesErrorAndAcceptedFalse()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(
            Catalog(Concept("archetype/ranger", SemanticCatalogConceptKinds.Archetype, "Ranger", description: "Tracks paths.", alternate: ["Guide"], tags: ["frontier"], facets: ["role:navigator"])),
            new SemanticCatalogQualityProfile { MinimumConceptsByKind = new Dictionary<string, int>(StringComparer.Ordinal) { [SemanticCatalogConceptKinds.Archetype] = 2 } });

        Assert.False(report.Accepted);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.MinimumConceptCountByKindNotSatisfied && diagnostic.Severity == SemanticCatalogDiagnosticSeverity.Error);
    }

    [Fact]
    public void LookupSmokeExpectationPassesUsingSemanticCatalogLookupIndex()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(
            ValidCatalog(),
            new SemanticCatalogQualityProfile
            {
                LookupSmokeExpectations =
                [
                    new SemanticCatalogLookupSmokeExpectation
                    {
                        Name = "ranger_synonym",
                        Query = new SemanticCatalogLookupQuery { QueryText = "Woodsman" },
                        ExpectedConceptIds = ["archetype/ranger"],
                        RequireAllExpectedConceptIds = true
                    }
                ]
            });

        Assert.True(report.Accepted, Join(report.Diagnostics));
        var smoke = Assert.Single(report.LookupSmokeResults);
        Assert.True(smoke.Passed);
        Assert.Equal(["archetype/ranger"], smoke.ActualConceptIds);
    }

    [Fact]
    public void LookupSmokeExpectationFailureProducesErrorAndAcceptedFalse()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(
            ValidCatalog(),
            new SemanticCatalogQualityProfile
            {
                LookupSmokeExpectations =
                [
                    new SemanticCatalogLookupSmokeExpectation
                    {
                        Name = "missing_query",
                        Query = new SemanticCatalogLookupQuery { QueryText = "No Such Term" },
                        ExpectedConceptIds = ["archetype/ranger"],
                        RequireAllExpectedConceptIds = true
                    }
                ]
            });

        Assert.False(report.Accepted);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogQualityDiagnosticCodes.LookupSmokeExpectationFailed && diagnostic.Severity == SemanticCatalogDiagnosticSeverity.Error);
        Assert.Equal(["archetype/ranger"], Assert.Single(report.LookupSmokeResults).MissingExpectedConceptIds);
    }

    [Fact]
    public void DiagnosticsAndMetricsOrderingIsStable()
    {
        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(
            Concept("theme/b", SemanticCatalogConceptKinds.Theme, "B"),
            Concept("archetype/a", SemanticCatalogConceptKinds.Archetype, "A", facets: ["bad"])));

        Assert.Equal(
            report.Diagnostics
                .OrderBy(diagnostic => diagnostic.Severity == SemanticCatalogDiagnosticSeverity.Error ? 0 : 1)
                .ThenBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.Target, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
                .ToArray(),
            report.Diagnostics);
        Assert.Equal(
            report.Metrics
                .OrderBy(metric => metric.Name, StringComparer.Ordinal)
                .ThenBy(metric => metric.Key, StringComparer.Ordinal)
                .ToArray(),
            report.Metrics);
    }

    [Fact]
    public void AnalyzerHandlesMoreThanOneHundredGeneratedConcepts()
    {
        var concepts = Enumerable.Range(0, 120)
            .Select(index => Concept(
                $"archetype/generated_actor_{index:000}",
                SemanticCatalogConceptKinds.Archetype,
                $"Generated Actor {index:000}",
                description: $"Generated fixture actor {index:000}.",
                alternate: [$"Generated Role {index:000}"],
                tags: ["generated"],
                facets: [$"slot:{index:000}"]))
            .ToArray();

        var report = new SemanticCatalogQualityAnalyzer().Analyze(Catalog(concepts));

        Assert.True(report.Accepted, Join(report.Diagnostics));
        AssertMetric(report, "concept_count", "total", 120);
        Assert.Empty(report.Diagnostics);
    }

    private static CompiledSemanticCatalog ValidCatalog() => Catalog(
        Concept(
            "archetype/ranger",
            SemanticCatalogConceptKinds.Archetype,
            "Ranger",
            description: "A frontier survival actor who tracks paths.",
            alternate: ["Guide", "Woodsman"],
            broader: ["archetype/survival_actor"],
            related: ["theme/frontier_survival"],
            tags: ["frontier", "survival"],
            facets: ["role:navigator"]),
        Concept(
            "archetype/survival_actor",
            SemanticCatalogConceptKinds.Archetype,
            "Survival Actor",
            description: "A broad survival actor family.",
            alternate: ["Explorer"],
            narrower: ["archetype/ranger"],
            tags: ["survival"],
            facets: ["role:actor"]),
        Concept(
            "theme/frontier_survival",
            SemanticCatalogConceptKinds.Theme,
            "Frontier Survival",
            description: "Cold weather frontier survival theme.",
            related: ["archetype/ranger"],
            tags: ["frontier"],
            facets: ["theme:survival"]),
        Concept(
            "dialogue_intent/ask_help",
            SemanticCatalogConceptKinds.DialogueIntent,
            "Ask Help",
            description: "Request help from a nearby actor.",
            alternate: ["Request Aid"],
            tags: ["dialogue"],
            facets: ["intent:request"]),
        Concept(
            "quest_motif/rescue",
            SemanticCatalogConceptKinds.QuestMotif,
            "Rescue",
            description: "Recover a missing actor.",
            alternate: ["Save Ally"],
            tags: ["quest"],
            facets: ["motif:rescue"]),
        Concept(
            "npc_archetype/mentor",
            SemanticCatalogConceptKinds.NpcArchetype,
            "Mentor",
            description: "Guides the player through early risk.",
            alternate: ["Teacher"],
            tags: ["npc"],
            facets: ["role:mentor"]));

    private static CompiledSemanticCatalog Catalog(params CompiledSemanticCatalogConcept[] concepts) => new()
    {
        CatalogId = "candidate_semantic_catalog_quality_v1/test",
        Version = "0.1.0",
        Language = "en",
        Concepts = concepts
    };

    private static CompiledSemanticCatalogConcept Concept(
        string id,
        string kind,
        string label,
        string description = "",
        IReadOnlyList<string>? alternate = null,
        IReadOnlyList<string>? broader = null,
        IReadOnlyList<string>? narrower = null,
        IReadOnlyList<string>? related = null,
        IReadOnlyList<string>? tags = null,
        IReadOnlyList<string>? facets = null) => new()
        {
            Id = id,
            Kind = kind,
            PreferredLabel = label,
            Description = description,
            AlternateLabels = alternate ?? Array.Empty<string>(),
            BroaderIds = broader ?? Array.Empty<string>(),
            NarrowerIds = narrower ?? Array.Empty<string>(),
            RelatedIds = related ?? Array.Empty<string>(),
            Tags = tags ?? Array.Empty<string>(),
            Facets = facets ?? Array.Empty<string>()
        };

    private static void AssertMetric(SemanticCatalogQualityReport report, string name, string key, int value)
    {
        Assert.Contains(report.Metrics, metric => metric.Name == name && metric.Key == key && metric.Value == value);
    }

    private static string Join(IEnumerable<SemanticCatalogQualityDiagnostic> diagnostics) =>
        string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}:{item.Message}"));
}
