using LLMGameCreator.Application.Design.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticCatalogMarkdownRendererTests
{
    [Fact]
    public void RendersTermsRelationsDiagnostics()
    {
        var catalog = new SemanticCatalogService().Build(SemanticCatalogServiceTests.Set("""
        {
          "terms": [{ "id": "location/harbor", "kind": "unknown", "label": "Harbor" }],
          "relations": [{ "source": "location/harbor", "kind": "has_theme", "target": "theme/survival" }],
          "themes": ["new frontier"],
          "tones": [{ "id": "/invalid", "kind": "tone", "label": "Invalid" }]
        }
        """));

        var markdown = new SemanticCatalogMarkdownRenderer().Render(catalog);

        Assert.Contains("## Terms", markdown, StringComparison.Ordinal);
        Assert.Contains("theme/new_frontier", markdown, StringComparison.Ordinal);
        Assert.Contains("## Relations", markdown, StringComparison.Ordinal);
        Assert.Contains("has_theme", markdown, StringComparison.Ordinal);
        Assert.Contains("semantic_catalog.invalid_term_id", markdown, StringComparison.Ordinal);
    }
}
