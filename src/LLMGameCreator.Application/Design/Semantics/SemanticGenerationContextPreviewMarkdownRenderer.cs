namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticGenerationContextPreviewMarkdownRenderer
{
    public string Render(SemanticGenerationContextPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        var lines = new List<string>
        {
            "# Semantic Generation Context Preview v1",
            string.Empty,
            "This is a deterministic preview. It does not call an LLM, provider, generator, Lua, Unity, or Runtime gameplay.",
            string.Empty,
            $"- Maximum recommended prompt terms: `{preview.LlmPolicy.MaxRecommendedPromptTerms}`",
            string.Empty,
            "## LLM required for",
            string.Empty
        };
        lines.AddRange(preview.LlmPolicy.LlmRequiredFor.Select(item => $"- {item}"));

        lines.AddRange([string.Empty, "## Deterministic without LLM", string.Empty]);
        lines.AddRange(preview.LlmPolicy.DeterministicSteps.Select(item => $"- {item}"));

        lines.AddRange([string.Empty, "## Compact semantic sections", string.Empty]);
        if (preview.Sections.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            foreach (var section in preview.Sections)
            {
                lines.Add($"### {section.Title}");
                lines.Add(string.Empty);
                lines.AddRange(section.TermIds.Select(termId => $"- `{termId}`"));
                lines.Add(string.Empty);
            }
        }

        lines.AddRange(["## Candidate terms needing approval", string.Empty]);
        lines.AddRange(preview.CandidateTerms.Count == 0
            ? ["- None"]
            : preview.CandidateTerms.Select(termId => $"- `{termId}`"));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(preview.Diagnostics.Count == 0
            ? ["- None"]
            : preview.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }
}
