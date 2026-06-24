using LLMGameCreator.Application.Design.Semantics;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticGenerationContextPreviewTests
{
    [Fact]
    public void BuildsCompactPreviewWithoutLlm()
    {
        var catalog = new SemanticCatalogService().Build(SemanticCatalogServiceTests.Set("""
        {
          "themes": ["survival", "new frontier"],
          "tones": ["hopeful"],
          "questMotifs": ["lost expedition"],
          "assetStyleHints": ["hand_painted"],
          "audioMoodHints": ["mysterious"]
        }
        """));

        var first = new SemanticGenerationContextPreviewService().Build(catalog);
        var second = new SemanticGenerationContextPreviewService().Build(catalog);

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Equal(80, first.LlmPolicy.MaxRecommendedPromptTerms);
        Assert.Contains("semantic catalog merge", first.LlmPolicy.DeterministicSteps);
        Assert.Contains("new quest/dialogue prose", first.LlmPolicy.LlmRequiredFor);
        Assert.Contains(first.Sections, section => section.SectionId == "themes" && section.TermIds.Contains("theme/survival"));
        Assert.Contains("theme/new_frontier", first.CandidateTerms);
        Assert.DoesNotContain(first.LlmPolicy.DeterministicSteps, step => step.Contains("provider execution", StringComparison.OrdinalIgnoreCase));
    }
}
