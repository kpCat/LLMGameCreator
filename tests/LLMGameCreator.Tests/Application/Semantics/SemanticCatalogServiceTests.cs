using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticCatalogServiceTests
{
    [Fact]
    public void BuildsSeedCatalogDeterministically()
    {
        var service = new SemanticCatalogService();

        var first = service.Build(new GeneratorPlanApprovedArtifactSet());
        var second = service.Build(new GeneratorPlanApprovedArtifactSet());

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Contains(first.Terms, term => term.TermId == "theme/survival" && term.Status == SemanticTermStatuses.Known);
        Assert.Contains(first.Terms, term => term.TermId == "dialogue_intent/give_quest");
        Assert.Contains(first.Terms, term => term.TermId == "item_affordance/craft_material");
        Assert.Contains(first.Terms, term => term.TermId == "audio_mood_hint/mysterious");
        Assert.Empty(first.Diagnostics);
    }

    [Fact]
    public void MapsSemanticPackTermsAndRelations()
    {
        var artifactSet = Set("""
        {
          "semantic": {
            "terms": [
              { "id": "location/sky_lantern_outpost", "kind": "unknown", "label": "Sky Lantern Outpost", "aliases": ["lantern post"] },
              { "id": "theme/survival", "kind": "theme", "label": "Survival" }
            ],
            "relations": [
              { "source": "location/sky_lantern_outpost", "kind": "has_theme", "target": "theme/survival" }
            ]
          },
          "tones": ["mysterious"],
          "dialogueIntents": ["warn", "bargain"]
        }
        """);

        var catalog = new SemanticCatalogService().Build(artifactSet);

        var location = Assert.Single(catalog.Terms, term => term.TermId == "location/sky_lantern_outpost");
        Assert.Equal(SemanticTermStatuses.Candidate, location.Status);
        Assert.Contains("lantern post", location.Aliases);
        Assert.Contains("artifact/semantic", location.SourceArtifactIds);
        Assert.Contains(catalog.Terms, term => term.TermId == "tone/mysterious" && term.Status == SemanticTermStatuses.Candidate);
        Assert.Contains(catalog.Terms, term => term.TermId == "dialogue_intent/warn");
        var relation = Assert.Single(catalog.Relations);
        Assert.Equal("has_theme", relation.RelationKind);
        Assert.Equal("location/sky_lantern_outpost", relation.SourceTermId);
        Assert.Equal("theme/survival", relation.TargetTermId);
    }

    [Fact]
    public void UnknownSafeTermsBecomeCandidates()
    {
        var catalog = new SemanticCatalogService().Build(Set("""
        {
          "semantic_groups": [
            { "id": "semantic/core", "terms": ["survival", "glass storm memory"] }
          ],
          "terms": [
            { "id": "weather/mist", "kind": "weather_state", "label": "Mist" }
          ]
        }
        """));

        Assert.Contains(catalog.Terms, term => term.TermId == "theme/survival" && term.Status == SemanticTermStatuses.Known);
        Assert.Contains(catalog.Terms, term => term.TermId == "unknown/glass_storm_memory" && term.Status == SemanticTermStatuses.Candidate);
        Assert.Contains(catalog.Terms, term => term.TermId == "weather/mist" && term.Kind == SemanticTermKinds.Unknown);
        Assert.Contains(catalog.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogDiagnosticCodes.UnknownTermKind);
    }

    [Fact]
    public void InvalidSemanticIdsBecomeDiagnosticsAndAreSkipped()
    {
        var catalog = new SemanticCatalogService().Build(Set("""
        {
          "terms": [
            { "id": "/absolute", "kind": "theme", "label": "Absolute" },
            { "id": "theme/../escape", "kind": "theme", "label": "Escape" },
            { "id": "theme\\bad", "kind": "theme", "label": "Bad" }
          ],
          "relations": [
            { "source": "theme/survival", "kind": "has theme", "target": "C:/outside" }
          ]
        }
        """));

        Assert.DoesNotContain(catalog.Terms, term => term.Label is "Absolute" or "Escape" or "Bad");
        Assert.True(catalog.Diagnostics.Count(diagnostic => diagnostic.Code == SemanticCatalogDiagnosticCodes.InvalidTermId) >= 3);
        Assert.Contains(catalog.Diagnostics, diagnostic => diagnostic.Code == SemanticCatalogDiagnosticCodes.InvalidRelation);
    }

    [Fact]
    public void DoesNotRequireGamePackageSchemaChange()
    {
        var artifactSet = Set("""{ "themes": ["survival", "new horizon"] }""");

        var catalog = new SemanticCatalogService().Build(artifactSet);
        var assembly = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, DateTimeOffset.UnixEpoch);

        Assert.Contains(catalog.Terms, term => term.TermId == "theme/new_horizon");
        var mapping = Assert.Single(assembly.Mappings);
        Assert.Equal(GeneratorPlanGamePackageAssemblyMappingResult.Unmapped, mapping.Result);
        Assert.Equal("no_game_package_field", mapping.Target);
        Assert.Single(assembly.Package.GeneratedContent.PreservedArtifacts);
    }

    internal static GeneratorPlanApprovedArtifactSet Set(string contentJson) => new()
    {
        SchemaVersion = "1",
        SnapshotId = "snapshot/semantic-tests",
        ApprovedArtifacts =
        [
            new GeneratorPlanApprovedArtifact
            {
                ArtifactId = "artifact/semantic",
                ArtifactKind = "semantic_pack_v1",
                ExpectedArtifactContract = "semantic_pack_v1",
                ContentJson = contentJson
            }
        ]
    };
}
