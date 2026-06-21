using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class GameBlueprintCapabilityTests
{
    [Fact]
    public void BuiltInCapabilityIdsAreUniqueAndResolvable()
    {
        var definitions = BuiltInCapabilityRegistry.Definitions;
        var registry = BuiltInCapabilityRegistry.Create();

        Assert.Equal(
            definitions.Count,
            definitions.Select(definition => definition.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Empty(registry.DuplicateIds);
        Assert.All(definitions, definition =>
        {
            Assert.True(registry.TryGet(definition.Id, out var resolved));
            Assert.Equal(definition, resolved);
        });
    }

    [Fact]
    public void BaselineGeneratedRpgBlueprintValidatesCompatible()
    {
        var blueprint = GetPreset(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);
        var result = CreateValidator().Validate(blueprint);

        Assert.True(result.Ok, JoinDiagnostics(result));
        Assert.Equal(CompositionCompatibilityStatus.Compatible, result.Status);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void UnknownCapabilityProducesErrorDiagnostic()
    {
        var result = CreateValidator().Validate(Blueprint("future.unknown_capability"));

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.False(result.Ok);
        Assert.Equal(CompositionDiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(CompositionDiagnosticCodes.UnknownCapability, diagnostic.Code);
        Assert.Equal("future.unknown_capability", diagnostic.CapabilityId);
    }

    [Fact]
    public void MissingRequiredCapabilityProducesError()
    {
        var result = CreateValidator().Validate(Blueprint("runtime.preview_movement"));

        Assert.False(result.Ok);
        Assert.Equal(CompositionCompatibilityStatus.MissingRequirement, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement &&
            diagnostic.RelatedCapabilityId == "package.activation");
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement &&
            diagnostic.RelatedCapabilityId == "presentation.topdown_2d_runtime_preview");
    }

    [Fact]
    public void DirectConflictProducesError()
    {
        var result = CreateValidator().Validate(Blueprint(
            "world_source.procedural_package",
            "world_source.imported_real_map"));

        Assert.False(result.Ok);
        Assert.Equal(CompositionCompatibilityStatus.Conflict, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Severity == CompositionDiagnosticSeverity.Error &&
            diagnostic.Code == CompositionDiagnosticCodes.DirectConflict);
    }

    [Fact]
    public void OptionalMissingCapabilityProducesWarning()
    {
        var registry = new CapabilityRegistry(
        [
            new CapabilityDefinition
            {
                Id = "feature.primary",
                Title = "Primary feature",
                OptionalRequires = ["feature.adapter"],
                Provides = ["feature.primary"]
            }
        ]);
        var result = new GameBlueprintCompositionValidator(registry).Validate(Blueprint("feature.primary"));

        Assert.True(result.Ok, JoinDiagnostics(result));
        Assert.Equal(CompositionCompatibilityStatus.DegradedButUsable, result.Status);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(CompositionDiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal(CompositionDiagnosticCodes.OptionalRequirementMissing, diagnostic.Code);
    }

    [Fact]
    public void DuplicateRegistryCapabilityProducesDeterministicError()
    {
        var registry = new CapabilityRegistry(
        [
            new CapabilityDefinition { Id = "duplicate.capability", Title = "First" },
            new CapabilityDefinition { Id = "DUPLICATE.CAPABILITY", Title = "Second" }
        ]);
        var result = new GameBlueprintCompositionValidator(registry).Validate(Blueprint("duplicate.capability"));

        Assert.False(result.Ok);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(CompositionDiagnosticCodes.DuplicateRegistryId, diagnostic.Code);
        Assert.Equal(CompositionCompatibilityStatus.Conflict, result.Status);
    }

    [Fact]
    public void FutureImportedMapBlueprintsReportPlannedAndMissingCapabilitiesWithoutThrowing()
    {
        foreach (var presetId in new[]
                 {
                     GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture,
                     GameBlueprintPresetProvider.ZombieCitySurvivalImportedMapFuture
                 })
        {
            var result = CreateValidator().Validate(GetPreset(presetId));

            Assert.False(result.Ok);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == CompositionDiagnosticCodes.UnsupportedYet);
            Assert.Contains(result.Diagnostics, diagnostic =>
                diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement &&
                diagnostic.RelatedCapabilityId == "content.generated_quests");
            Assert.Contains(result.Diagnostics, diagnostic =>
                diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement &&
                diagnostic.RelatedCapabilityId == "content.generated_dialogues");
            Assert.Equal(
                result.Diagnostics.OrderBy(diagnostic => diagnostic.Severity == CompositionDiagnosticSeverity.Error ? 0 : 1)
                    .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(diagnostic => diagnostic.CapabilityId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(diagnostic => diagnostic.RelatedCapabilityId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase),
                result.Diagnostics);
        }
    }

    private static GameBlueprintCompositionValidator CreateValidator()
    {
        return new GameBlueprintCompositionValidator(BuiltInCapabilityRegistry.Create());
    }

    private static GameBlueprint GetPreset(string presetId)
    {
        Assert.True(new GameBlueprintPresetProvider().TryGet(presetId, out var blueprint));
        return blueprint;
    }

    private static GameBlueprint Blueprint(params string[] capabilityIds)
    {
        return new GameBlueprint
        {
            BlueprintId = "test/blueprint",
            Title = "Test blueprint",
            RequestedCapabilityIds = capabilityIds
        };
    }

    private static string JoinDiagnostics(CompositionValidationResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
