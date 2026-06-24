namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticCatalogMarkdownRenderer
{
    public string Render(SemanticCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var lines = new List<string>
        {
            "# Project Semantic Catalog v1",
            string.Empty,
            $"- Catalog id: `{catalog.CatalogId}`",
            $"- Terms: `{catalog.Terms.Count}`",
            $"- Relations: `{catalog.Relations.Count}`",
            $"- Diagnostics: `{catalog.Diagnostics.Count}`",
            string.Empty,
            "## Terms",
            string.Empty
        };

        lines.AddRange(catalog.Terms.Count == 0
            ? ["- None"]
            : catalog.Terms.Select(term =>
                $"- `{term.TermId}`: kind=`{term.Kind}`; status=`{term.Status}`; label={term.Label}; aliases=`{string.Join(", ", term.Aliases)}`; sources=`{string.Join(", ", term.SourceArtifactIds)}`"));

        lines.AddRange([string.Empty, "## Relations", string.Empty]);
        lines.AddRange(catalog.Relations.Count == 0
            ? ["- None"]
            : catalog.Relations.Select(relation =>
                $"- `{relation.RelationId}`: `{relation.SourceTermId}` --`{relation.RelationKind}`--> `{relation.TargetTermId}`; status=`{relation.Status}`; sources=`{string.Join(", ", relation.SourceArtifactIds)}`"));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(catalog.Diagnostics.Count == 0
            ? ["- None"]
            : catalog.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` artifact=`{diagnostic.SourceArtifactId}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }
}
