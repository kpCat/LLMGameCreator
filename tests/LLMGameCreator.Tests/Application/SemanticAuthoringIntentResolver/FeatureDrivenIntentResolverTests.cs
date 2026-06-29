using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticAuthoringIntentResolver;

public sealed class FeatureDrivenIntentResolverTests
{
    [Fact]
    public void ResolverProducesDistinctComparableIntentPlansForAllScenarios()
    {
        var resolver = new FeatureDrivenIntentResolver();
        var resolutions = DynamicSemanticFeatureCatalog.BuildDefaultScenarios()
            .Select(resolver.ResolveScenario)
            .ToList();

        Assert.Equal(4, resolutions.Select(item => item.StableSummary).Distinct(StringComparer.Ordinal).Count());
        Assert.All(resolutions, resolution =>
        {
            Assert.NotEmpty(resolution.Intents);
            Assert.DoesNotContain(resolution.Diagnostics, item => item.Severity == "error");
            Assert.Equal(resolution.Intents.OrderBy(item => item.IntentId, StringComparer.Ordinal).Select(item => item.IntentId), resolution.Intents.Select(item => item.IntentId));
        });

        var families = resolutions.SelectMany(item => item.Intents).Select(item => item.IntentFamily).Distinct(StringComparer.Ordinal).ToList();
        foreach (var family in SemanticAuthoringIntentVocabulary.IntentFamilies)
        {
            Assert.Contains(family, families);
        }
    }

    [Fact]
    public void IntentsKeepTracesAndDoNotGenerateFinalContent()
    {
        var resolution = new FeatureDrivenIntentResolver().ResolveScenario(DynamicSemanticFeatureCatalog.GothicScenario());

        Assert.All(resolution.Intents, intent =>
        {
            Assert.NotEmpty(intent.SourceFeatureIds);
            Assert.DoesNotContain("final dialogue", intent.TemplateHint, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GamePackage", intent.TraceSummary, StringComparison.OrdinalIgnoreCase);
        });
    }
}
