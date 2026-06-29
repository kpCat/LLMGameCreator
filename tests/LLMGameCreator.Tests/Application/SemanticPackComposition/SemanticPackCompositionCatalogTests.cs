using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using LLMGameCreator.Application.Design.SemanticPackComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticPackComposition;

public sealed class SemanticPackCompositionCatalogTests
{
    [Fact]
    public void SeedCatalogValidatesAndCoversRequiredProfilesAndMixins()
    {
        var packs = SemanticPackCompositionCatalog.BuildDefaultPacks();
        var diagnostics = SemanticPackCompositionValidator.ValidateCatalog(packs, SemanticArtifactContractRegistry.BuildDefaultContracts());

        Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/frontier_survival");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/gothic_intrigue");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/caravan_trade");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/ruins_and_relics");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/winter_hazards");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/merchant_guilds");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/border_conflict");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/folk_magic");
        Assert.Contains(packs, pack => pack.PackId == "semantic_pack/scarcity_economy");
    }

    [Fact]
    public void SeedCatalogCoversGoal031FactDomainVocabulary()
    {
        var domains = SemanticPackCompositionCatalog.BuildDefaultPacks()
            .SelectMany(pack => pack.Facts)
            .Select(fact => fact.Domain)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var required in SemanticPackCompositionCatalog.ValidFactDomains)
        {
            Assert.Contains(required, domains);
        }
    }
}
