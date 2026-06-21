using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class GameDesignBriefTests
{
    [Fact]
    public void GameDesignBriefExpressesLoreRulesWishesAndGenerationPolicy()
    {
        Assert.True(new GameDesignBriefPresetProvider().TryGet(
            GameDesignBriefPresetProvider.TopDownGeneratedRpg,
            out var brief));

        Assert.Equal(GameLoreMode.OriginalFiction, brief.LoreMode);
        Assert.NotEmpty(brief.LoreFacts);
        Assert.NotEmpty(brief.WorldRules);
        Assert.Contains(brief.ViewModeWishes, wish => wish.ViewModeId == "top_down_character" && wish.Required);
        Assert.Contains(brief.InteractionWishes, wish => wish.InteractionId == "talk" && wish.Required);
        Assert.NotEmpty(brief.UiWishes);
        Assert.NotEmpty(brief.AssetStyleWishes);
        Assert.NotEmpty(brief.AudioStyleWishes);
        Assert.NotEmpty(brief.GenerationPolicy.LlmSeededAreas);
        Assert.NotEmpty(brief.GenerationPolicy.ProgramGeneratedAreas);
        Assert.NotEmpty(brief.GenerationPolicy.LuaDefinedAreas);
        Assert.NotEmpty(brief.GenerationPolicy.AssetGeneratedAreas);
        Assert.NotEmpty(brief.GenerationPolicy.HandAuthoredAreas);
        Assert.NotEmpty(brief.GenerationPolicy.RuntimeGeneratedLazyAreas);
        Assert.True(brief.ScalePolicy.SupportsLazyExpansion);
        Assert.True(brief.PerformancePolicy.UseAbstractOffscreenSimulation);
    }
}
