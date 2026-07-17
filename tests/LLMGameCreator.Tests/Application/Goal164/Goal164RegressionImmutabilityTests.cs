using System.Text.RegularExpressions;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164RegressionImmutabilityTests
{
    [Fact]
    public void Behavioral_runtime_qualification_keeps_exact_package_bytes_stable()
    {
        var combat = Goal164TestKit.AllSelectable.Build.GeneratedEncounterCombat!;

        Assert.True(combat.PackageShaUnchangedDuringRuntime);
        Assert.True(combat.ExactPackageReferencePassed);
        Assert.Equal(combat.ExactPackageSha256, combat.Overlay?.OutputPackageSha256);
    }

    [Fact]
    public void Behavioral_goal142_baseline_is_exact_source_authority()
    {
        var fixture = Goal164TestKit.AllSelectable;

        Assert.Equal(fixture.Source.Source?.Goal142BaselinePackageSha256,
            Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
    }

    [Fact]
    public void Behavioral_goal148_manual_tree_is_byte_identical_after_goal164_build()
    {
        var state = Goal164ExternalArtifactState.Value;

        Assert.NotEmpty(state.Goal148Before);
        Assert.Equal(state.Goal148Before, state.Goal148After);
    }

    [Fact]
    public void Behavioral_all_generation_sidecars_are_byte_identical_after_upgrade_build()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var root = Path.Combine(fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot.Replace('/', Path.DirectorySeparatorChar));

        Assert.All(fixture.GenerationSidecarHashesBefore,
            pair => Assert.Equal(pair.Value, Goal164TestKit.FileSha(Path.Combine(root, pair.Key))));
    }

    [Fact]
    public void Behavioral_combat_overlay_preserves_generated_content_catalog_exactly()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var contract = fixture.Contract.Contract!;
        var binding = new GeneratedEncounterCombatBindingService().Bind(fixture.Source, fixture.LaneAPackage, contract);
        var overlay = new GeneratedWorldEncounterCombatOverlayService().Build(
            fixture.LaneAPackage, contract, binding);

        Assert.Equal(Goal164TestKit.Canonical(fixture.LaneAPackage.GeneratedContent),
            Goal164TestKit.Canonical(overlay.CombatOverlayPackage.GeneratedContent));
    }

    [Fact]
    public void Behavioral_game_package_schema_is_not_extended_for_binding()
    {
        var properties = typeof(LLMGameCreator.GamePackage.GeneratedEncounterDefinition).GetProperties()
            .Select(item => item.Name).ToList();

        Assert.DoesNotContain("PackageEncounterId", properties);
        Assert.Contains("SourceId", properties);
    }

    [Fact]
    public void Behavioral_combat_services_create_no_runtime_definitions()
    {
        foreach (var file in NewCombatSources())
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("new AbilityDefinition", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new ResourceDefinition", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Game.Abilities.Add", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Game.Resources.Add", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Behavioral_changed_goal164_sources_have_no_mojibake_or_escaped_cyrillic()
    {
        var mojibake = new[] { "Рџ", "Рќ", "Рћ", "Р•", "РЎ", "Р›", "Р¤", "Рњ", "РЈ", "Рљ", "�" };
        var escaped = new Regex(@"\\u0[45][0-9A-Fa-f]{2}|&#[xX]0[45][0-9A-Fa-f]{2};",
            RegexOptions.CultureInvariant);
        foreach (var file in NewCombatSources())
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain('\0', source);
            Assert.DoesNotContain(mojibake, marker => source.Contains(marker, StringComparison.Ordinal));
            Assert.False(escaped.IsMatch(source), file);
        }
    }

    private static IReadOnlyList<string> NewCombatSources()
    {
        var root = Path.Combine(Goal164TestKit.RepositoryRoot,
            "src", "LLMGameCreator.Application", "Generation", "Procedural");
        return
        [
            Path.Combine(root, "GeneratedEncounterCombatContractModels.cs"),
            Path.Combine(root, "GeneratedEncounterCombatContractService.cs"),
            Path.Combine(root, "GeneratedEncounterCombatBindingService.cs"),
            Path.Combine(root, "GeneratedWorldEncounterCombatOverlayService.cs"),
            Path.Combine(root, "GameProjectGeneratedEncounterCombatQualificationService.cs")
        ];
    }
}

internal static class Goal164ExternalArtifactState
{
    private static readonly Lazy<Goal164ExternalArtifactFixture> Fixture = new(Create);
    public static Goal164ExternalArtifactFixture Value => Fixture.Value;

    private static Goal164ExternalArtifactFixture Create()
    {
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        Assert.True(Directory.Exists(goal148));
        var before = Goal159TestKit.TreeHashes(goal148);
        _ = Goal164TestKit.AllSelectable;
        var after = Goal159TestKit.TreeHashes(goal148);
        return new Goal164ExternalArtifactFixture(before, after);
    }
}

internal sealed record Goal164ExternalArtifactFixture(
    IReadOnlyDictionary<string, string> Goal148Before,
    IReadOnlyDictionary<string, string> Goal148After);
