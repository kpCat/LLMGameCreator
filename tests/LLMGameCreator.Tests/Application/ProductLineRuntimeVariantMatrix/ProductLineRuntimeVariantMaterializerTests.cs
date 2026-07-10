using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantMaterializerTests
{
    [Fact]
    public void MaterializerAppliesStructuredRuntimeMutations()
    {
        var root = ProjectRoot();
        var template = File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json"));
        var recipe = ProductLineRuntimeVariantCatalog.CreateDefault()
            .Variants
            .Single(item => item.RecipeId == "alchemy_focus");

        var result = new ProductLineRuntimeVariantMaterializer().Materialize(template, recipe);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
            result.PackageJson,
            options);

        Assert.NotNull(package);
        Assert.True(result.MutationAudit.Passed);
        Assert.Equal(3, result.MutationAudit.OperationCount);
        Assert.All(result.MutationAudit.Operations, operation => Assert.True(operation.Passed));
        Assert.Equal("minimal-map-game-alchemy-focus", result.MutationAudit.CandidateId);
        Assert.Contains(
            "\"candidateId\": \"minimal-map-game-alchemy-focus\"",
            package!.GeneratedContent.Profile.SourceContextJson,
            StringComparison.Ordinal);
        Assert.Equal(
            4,
            package.Game.Inventories
                .Single(item => item.Id == "inventory/player_start")
                .Stacks
                .Single(item => item.ItemId == "item/red_herb")
                .Amount);
        Assert.Equal(
            2,
            package.Game.Recipes
                .Single(item => item.Id == "recipe/healing_potion")
                .Outputs
                .Single(item => item.Id == "item/healing_potion")
                .Amount);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
