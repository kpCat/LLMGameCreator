using LLMGameCreator.Application.Composition;
using LLMGameCreator.Application.Projects;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class CompositionDiagnosticsTests
{
    [Fact]
    public void BaselineReportIsBuildableWithDeterministicCurrentGenerators()
    {
        var report = CreateService().CreateReport(GetPreset(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview));

        Assert.Contains(report.Readiness, new[]
        {
            GameCompositionReadiness.BuildableNow,
            GameCompositionReadiness.BuildableWithWarnings
        });
        Assert.NotEmpty(report.SelectedCurrentGeneratorIds);
        Assert.Empty(report.RelatedPlannedGeneratorIds);
        Assert.Empty(report.MissingGeneratorCapabilityIds);
        Assert.Equal(ContentLanguageCodes.Russian, report.ContentLanguage);
        Assert.Equal(
            report.SelectedCurrentGeneratorIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase),
            report.SelectedCurrentGeneratorIds);
    }

    [Fact]
    public void ExplicitContentLanguagePolicyIsReflectedInReport()
    {
        var report = CreateService().CreateReport(
            GetPreset(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview),
            new ContentLanguagePolicy { ContentLanguage = ContentLanguageCodes.Ukrainian });

        Assert.Equal(ContentLanguageCodes.Ukrainian, report.ContentLanguage);
    }

    [Fact]
    public void FutureImportedMapReportReturnsPlannedAndMissingDiagnostics()
    {
        var report = CreateService().CreateReport(GetPreset(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture));

        Assert.Contains(report.Readiness, new[]
        {
            GameCompositionReadiness.PlannedFuture,
            GameCompositionReadiness.MissingRequirements
        });
        Assert.NotEmpty(report.RelatedPlannedGeneratorIds);
        Assert.Contains("time.calendar", report.MissingGeneratorCapabilityIds);
        Assert.Contains(report.RecommendedActions, action =>
            action.Message == "Add generator support for capability 'time.calendar'.");
        Assert.Contains(report.RecommendedActions, action =>
            action.Code == "composition.action.implement_planned_generator");
    }

    [Fact]
    public void BrokenBlueprintReturnsErrorReadinessAndCapabilityActions()
    {
        var report = CreateService().CreateReport(new GameBlueprint
        {
            BlueprintId = "test/broken",
            Title = "Broken blueprint",
            RequestedCapabilityIds = ["runtime.preview_movement"]
        });

        Assert.Contains(report.Readiness, new[]
        {
            GameCompositionReadiness.MissingRequirements,
            GameCompositionReadiness.Conflict,
            GameCompositionReadiness.Invalid
        });
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Severity == GameCompositionDiagnosticSeverity.Error);
        Assert.Contains(report.RecommendedActions, action =>
            action.Message == "Add or request capability 'package.activation'.");
    }

    [Fact]
    public void ReportActionsAndMarkdownAreDeterministic()
    {
        var service = CreateService();
        var blueprint = GetPreset(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture);
        var first = service.CreateReport(blueprint);
        var second = service.CreateReport(blueprint);
        var renderer = new GameCompositionDiagnosticsMarkdownRenderer();
        var firstMarkdown = renderer.Render(first);
        var secondMarkdown = renderer.Render(second);

        Assert.Equal(first.SelectedCurrentGeneratorIds, second.SelectedCurrentGeneratorIds);
        Assert.Equal(first.RecommendedActions, second.RecommendedActions);
        Assert.Equal(firstMarkdown, secondMarkdown);
        Assert.NotEmpty(firstMarkdown);
        Assert.Contains("# Game Composition Diagnostics", firstMarkdown);
        Assert.Contains("## Recommended actions", firstMarkdown);
    }

    private static GameCompositionDiagnosticsService CreateService()
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        return new GameCompositionDiagnosticsService(
            new GameBlueprintCompositionValidator(capabilities),
            new GeneratorCatalogValidator(capabilities),
            new GeneratorPlanResolver(capabilities, catalog),
            catalog);
    }

    private static GameBlueprint GetPreset(string presetId)
    {
        Assert.True(new GameBlueprintPresetProvider().TryGet(presetId, out var blueprint));
        return blueprint;
    }
}
