using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GameBlueprintCapabilityCompatibilitySmokeTests
{
    [Fact]
    public void GameBlueprintCapabilityCompatibilityProductSmoke()
    {
        var registry = BuiltInCapabilityRegistry.Create();
        var presets = new GameBlueprintPresetProvider();
        var validator = new GameBlueprintCompositionValidator(registry);

        Assert.Empty(registry.DuplicateIds);
        Assert.Equal(
            registry.Definitions.Count,
            registry.Definitions.Select(definition => definition.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.True(presets.TryGet(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview, out var baseline));
        var baselineResult = validator.Validate(baseline);
        Assert.True(baselineResult.Ok, JoinDiagnostics(baselineResult));
        Assert.Equal(CompositionCompatibilityStatus.Compatible, baselineResult.Status);

        Assert.True(presets.TryGet(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture, out var future));
        var futureResult = validator.Validate(future);
        Assert.NotEmpty(futureResult.Diagnostics);
        Assert.Contains(futureResult.Diagnostics, diagnostic => diagnostic.Code == CompositionDiagnosticCodes.UnsupportedYet);
        Assert.Contains(futureResult.Diagnostics, diagnostic => diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement);

        var broken = new GameBlueprint
        {
            BlueprintId = "smoke/intentionally-broken",
            Title = "Intentionally broken blueprint",
            RequestedCapabilityIds = ["runtime.preview_movement"]
        };
        var brokenResult = validator.Validate(broken);
        Assert.False(brokenResult.Ok);
        Assert.Contains(brokenResult.Diagnostics, diagnostic =>
            diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement);

        Assert.DoesNotContain(
            registry.Definitions,
            definition => definition.Id.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    private static string JoinDiagnostics(CompositionValidationResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
