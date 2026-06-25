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
