using System.Text.Json;
using LLMGameCreator.Application.Design.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticLayerCompilerTests
{
    [Fact]
    public void CompilesLayerPrecedenceAndCandidateQuarantineDeterministically()
    {
        var service = new SemanticLayerCompilerService();
        var layers = new[]
        {
            Layer("core/base", SemanticLayerKinds.Core, Term("tone/tense", "tone", "Tense")),
            Layer("genre/frontier", SemanticLayerKinds.Genre, Term("tone/tense", "tone", "Tense frontier", tags: ["genre"])),
            Layer("project/outpost", SemanticLayerKinds.Project, Term("tone/tense", "tone", "Tense project", tags: ["project"])),
            Layer("imported_candidate/rumors", SemanticLayerKinds.ImportedCandidate, Term("tone/prophetic", "tone", "Prophetic", SemanticTermStatuses.Candidate))
        };

        var first = service.Compile(layers);
        var second = service.Compile(layers);

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.True(first.Accepted);
        var tense = Assert.Single(first.Catalog.Terms, item => item.TermId == "tone/tense");
        Assert.Equal("Tense project", tense.Label);
        Assert.Contains("project/outpost", tense.LayerIds);
        Assert.DoesNotContain(first.Catalog.Terms, item => item.TermId == "tone/prophetic");
        Assert.Contains(first.QuarantinedTerms, item => item.TermId == "tone/prophetic");
    }

    [Fact]
    public void RejectsUnsafeLayerAndUnknownRelationEndpoint()
    {
        var service = new SemanticLayerCompilerService();
        var result = service.Compile(
        [
            new SemanticLayerPack
            {
                LayerId = "../bad",
                LayerKind = SemanticLayerKinds.Project,
                Source = "C:/outside",
                Terms = [Term("tone/valid", "tone", "Valid")]
            },
            new SemanticLayerPack
            {
                LayerId = "core/base",
                LayerKind = SemanticLayerKinds.Core,
                Source = "unit-test",
                Terms = [Term("tone/valid", "tone", "Valid")],
                Relations =
                [
                    new SemanticLayerRelationDeclaration
                    {
                        SourceTermId = "tone/valid",
                        RelationKind = SemanticRelationKinds.Requires,
                        TargetTermId = "tone/missing",
                        Status = SemanticTermStatuses.Known
                    }
                ]
            }
        ]);

        Assert.False(result.Accepted);
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.invalid_layer_id");
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.unknown_relation_endpoint");
    }

    [Fact]
    public void RejectsWrongSchemaVersionAndLayerKindPrefixMismatch()
    {
        var service = new SemanticLayerCompilerService();

        var result = service.Compile(
        [
            new SemanticLayerPack
            {
                SchemaVersion = "semantic_pack_v0",
                LayerId = "core/base",
                LayerKind = SemanticLayerKinds.Core,
                Source = "unit-test",
                Terms = [Term("tone/valid", "tone", "Valid")]
            },
            new SemanticLayerPack
            {
                LayerId = "genre/mismatch",
                LayerKind = SemanticLayerKinds.Project,
                Source = "unit-test",
                Terms = [Term("tone/other", "tone", "Other")]
            }
        ]);

        Assert.False(result.Accepted);
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.invalid_schema_version");
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.layer_kind_prefix_mismatch");
    }

    [Fact]
    public void ThreeSamePrecedenceConflictingTermDeclarationsCannotReactivateTerm()
    {
        var service = new SemanticLayerCompilerService();

        var result = service.Compile(
        [
            Layer("genre/one", SemanticLayerKinds.Genre, Term("tone/shared", "tone", "Shared one")),
            Layer("genre/three", SemanticLayerKinds.Genre, Term("tone/shared", "tone", "Shared three")),
            Layer("genre/two", SemanticLayerKinds.Genre, Term("tone/shared", "tone", "Shared two"))
        ]);

        Assert.False(result.Accepted);
        Assert.DoesNotContain(result.Catalog.Terms, item => item.TermId == "tone/shared");
        Assert.Contains(result.QuarantinedTerms, item => item.TermId == "tone/shared" && item.Status == SemanticTermStatuses.Conflict);
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.term_conflict");
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.term_previously_conflicted");
    }

    [Fact]
    public void ConflictingRelationIdCannotRemainActive()
    {
        var service = new SemanticLayerCompilerService();

        var result = service.Compile(
        [
            new SemanticLayerPack
            {
                LayerId = "core/base",
                LayerKind = SemanticLayerKinds.Core,
                Source = "unit-test",
                Terms =
                [
                    Term("tone/source", "tone", "Source"),
                    Term("tone/target_a", "tone", "Target A"),
                    Term("tone/target_b", "tone", "Target B")
                ]
            },
            new SemanticLayerPack
            {
                LayerId = "genre/a",
                LayerKind = SemanticLayerKinds.Genre,
                Source = "unit-test",
                Relations =
                [
                    Relation("relation/shared", "tone/source", SemanticRelationKinds.CompatibleWith, "tone/target_a")
                ]
            },
            new SemanticLayerPack
            {
                LayerId = "genre/b",
                LayerKind = SemanticLayerKinds.Genre,
                Source = "unit-test",
                Relations =
                [
                    Relation("relation/shared", "tone/source", SemanticRelationKinds.CompatibleWith, "tone/target_b")
                ]
            },
            new SemanticLayerPack
            {
                LayerId = "genre/c",
                LayerKind = SemanticLayerKinds.Genre,
                Source = "unit-test",
                Relations =
                [
                    Relation("relation/shared", "tone/source", SemanticRelationKinds.CompatibleWith, "tone/target_a")
                ]
            }
        ]);

        Assert.False(result.Accepted);
        Assert.DoesNotContain(result.Catalog.Relations, item => item.RelationId == "relation/shared");
        Assert.Contains(result.QuarantinedRelations, item => item.RelationId == "relation/shared" && item.Status == SemanticTermStatuses.Conflict);
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.relation_conflict");
        Assert.Contains(result.Catalog.Diagnostics, item => item.Code == "semantic_layer.relation_previously_conflicted");
    }

    [Fact]
    public void MalformedJsonProducesDeterministicDiagnosticsAndKeepsValidNeighbor()
    {
        using var temp = new TempDirectory();
        File.WriteAllText(Path.Combine(temp.Path, "valid.json"), JsonSerializer.Serialize(Layer("core/base", SemanticLayerKinds.Core, Term("tone/valid", "tone", "Valid"))));
        File.WriteAllText(Path.Combine(temp.Path, "bad.json"), "{ not json");
        var service = new SemanticLayerCompilerService();

        var result = service.LoadPacksFromDirectory(temp.Path);

        Assert.Single(result.Packs);
        Assert.Contains(result.Packs, item => item.LayerId == "core/base");
        var diagnostic = Assert.Single(result.Diagnostics, item => item.Code == "semantic_layer.pack_json_malformed");
        Assert.Equal("bad.json", diagnostic.Target);
        Assert.DoesNotContain(temp.Path, diagnostic.Target);
        Assert.DoesNotContain(temp.Path, diagnostic.SourceArtifactId);
    }

    [Fact]
    public async Task WriteCreatesCompiledSemanticPackArtifacts()
    {
        using var temp = new TempDirectory();
        var service = new SemanticLayerCompilerService();
        var result = service.Compile([Layer("core/base", SemanticLayerKinds.Core, Term("tone/tense", "tone", "Tense"))]);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.CompiledJsonPath));
        Assert.True(File.Exists(write.CompiledMarkdownPath));
        Assert.Contains("compiled-semantic-pack", await File.ReadAllTextAsync(write.CompiledJsonPath));
        Assert.Contains("Compiled Semantic Pack", await File.ReadAllTextAsync(write.CompiledMarkdownPath));
    }

    private static SemanticLayerPack Layer(string id, string kind, params SemanticLayerTermDeclaration[] terms) => new()
    {
        LayerId = id,
        LayerKind = kind,
        Source = "unit-test",
        Terms = terms
    };

    private static SemanticLayerTermDeclaration Term(
        string id,
        string kind,
        string label,
        string status = SemanticTermStatuses.Known,
        IReadOnlyList<string>? tags = null) => new()
        {
            TermId = id,
            Kind = kind,
            Label = label,
            Status = status,
            Tags = tags ?? Array.Empty<string>()
        };

    private static SemanticLayerRelationDeclaration Relation(
        string id,
        string source,
        string kind,
        string target) => new()
        {
            RelationId = id,
            SourceTermId = source,
            RelationKind = kind,
            TargetTermId = target,
            Status = SemanticTermStatuses.Known
        };

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
