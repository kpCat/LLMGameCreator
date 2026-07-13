using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153B;

public sealed class Goal153BArchitecturePolicyTests
{
    [Fact]
    public void Generic_production_services_contain_no_goal153_catalog_literals()
    {
        var root = FindRoot();
        var forbidden = new[]
        {
            "feature.magic.mana_spellcasting", "startingMana", "abilityManaCost",
            "feature.combat.active_ability_loadout", "feature.status.turn_effects",
            "ability/arcane_impulse", "status/arcane_burn", "goal153_target"
        };
        var production = Directory.GetFiles(Path.Combine(root, "src", "LLMGameCreator.Application"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("Goal153", StringComparison.OrdinalIgnoreCase)).ToList();
        var matches = production.SelectMany(path => forbidden.Where(id => File.ReadAllText(path).Contains(id, StringComparison.Ordinal))
            .Select(id => Path.GetRelativePath(root, path) + ":" + id)).ToList();
        Assert.Empty(matches);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
