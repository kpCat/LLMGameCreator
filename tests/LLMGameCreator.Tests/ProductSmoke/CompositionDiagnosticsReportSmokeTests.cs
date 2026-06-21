using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CompositionDiagnosticsReportSmokeTests
{
    [Fact]
    public void CompositionDiagnosticsReportProductSmoke()
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var service = new GameCompositionDiagnosticsService(
            new GameBlueprintCompositionValidator(capabilities),
            new GeneratorCatalogValidator(capabilities),
            new GeneratorPlanResolver(capabilities, catalog),
            catalog);
        var presets = new GameBlueprintPresetProvider();
        var renderer = new GameCompositionDiagnosticsMarkdownRenderer();

        Assert.True(presets.TryGet(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview, out var baseline));
        var baselineReport = service.CreateReport(baseline);
        Assert.Contains(baselineReport.Readiness, new[]
        {
            GameCompositionReadiness.BuildableNow,
            GameCompositionReadiness.BuildableWithWarnings
        });
        Assert.NotEmpty(baselineReport.SelectedCurrentGeneratorIds);

        Assert.True(presets.TryGet(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture, out var future));
        var futureReport = service.CreateReport(future);
        Assert.Contains(futureReport.Readiness, new[]
        {
            GameCompositionReadiness.PlannedFuture,
            GameCompositionReadiness.MissingRequirements
        });

        var brokenReport = service.CreateReport(new GameBlueprint
        {
            BlueprintId = "smoke/intentionally-broken",
            Title = "Intentionally broken blueprint",
            RequestedCapabilityIds = ["runtime.preview_movement"]
        });
        Assert.Contains(brokenReport.Readiness, new[]
        {
            GameCompositionReadiness.MissingRequirements,
            GameCompositionReadiness.Conflict,
            GameCompositionReadiness.Invalid
        });

        var firstMarkdown = renderer.Render(futureReport);
        var secondMarkdown = renderer.Render(service.CreateReport(future));
        Assert.NotEmpty(firstMarkdown);
        Assert.Equal(firstMarkdown, secondMarkdown);
        Assert.DoesNotContain(catalog.Manifests, manifest => manifest.CanRunAtRuntime);
        Assert.DoesNotContain(catalog.Manifests, manifest =>
            manifest.GeneratorId.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }
}
