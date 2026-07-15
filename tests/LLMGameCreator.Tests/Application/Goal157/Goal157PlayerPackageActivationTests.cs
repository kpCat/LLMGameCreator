using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157PlayerPackageActivationTests
{
    [Fact]
    public void Behavioral_compatibility_package_retains_canonical_baseline_start()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "lane-a-start");
        var validation = Goal156TestKit.SourceService.Validate(copy.Path);
        var package = Goal156TestKit.Load(copy.Path);

        Assert.Equal(validation.Overlay?.BaselineStartMapId, package.Manifest.StartMapId);
        Assert.NotEqual(validation.Source?.GeneratedStartMapId, package.Manifest.StartMapId);
    }

    [Fact]
    public void Behavioral_player_composition_package_uses_generated_start()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "lane-b-start");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(Goal157TestKit.GeneratedStartMapId(copy), result.PlayerCompositionPackage.Manifest.StartMapId);
    }

    [Fact]
    public void Behavioral_all_gameplay_collections_are_canonical_equal_between_lanes()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "lane-gameplay");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Empty(result.CanonicalGameplayRecordDiff);
        Assert.Equal(["manifest.startMapId"], result.ManifestDiff);
    }

    [Fact]
    public void Behavioral_generated_start_map_exists_in_player_package_and_is_source_generated()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "generated-map");
        var source = Goal156TestKit.SourceService.Validate(copy.Path);

        var result = Goal157TestKit.Activate(copy);

        var map = Assert.Single(result.PlayerCompositionPackage.Game.Maps,
            item => item.Id == source.Source?.GeneratedStartMapId);
        Assert.Contains(source.GeneratedMvpPackage!.Game.Maps, item => item.Id == map.Id);
    }

    [Fact]
    public void Behavioral_identity_overlay_preserves_generated_start()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "identity-start");
        var original = Goal156TestKit.Load(copy.Path).Manifest;

        var result = Goal157TestKit.Activate(copy);

        Assert.Equal(result.PlayerCompositionPackage.Manifest.StartMapId,
            result.ActivatedProjectPackage.Manifest.StartMapId);
        Assert.Equal(original.PackageId, result.ActivatedProjectPackage.Manifest.PackageId);
        Assert.Equal(original.Title, result.ActivatedProjectPackage.Manifest.Title);
        Assert.Equal(original.Version, result.ActivatedProjectPackage.Manifest.Version);
    }

    [Fact]
    public void Behavioral_missing_generated_start_map_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "missing-start");
        var generatedMapId = Goal157TestKit.GeneratedStartMapId(copy);

        var result = Goal157TestKit.Activate(copy, mutate: package =>
            package.Game.Maps.RemoveAll(map => map.Id == generatedMapId));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.generated_start_map_missing", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_invalid_generated_start_position_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "invalid-start");
        var generatedMapId = Goal157TestKit.GeneratedStartMapId(copy);

        var result = Goal157TestKit.Activate(copy, mutate: package =>
            package.Game.Maps.Single(map => map.Id == generatedMapId).StartPosition = new Position2D(-1, -1));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.start_position_invalid", result.Diagnostics);
    }

    [Fact]
    public void Contract_lane_b_writes_two_explicit_package_artifacts()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "activation-artifacts");

        var result = Goal157TestKit.Activate(copy);

        Assert.EndsWith("player-composition/package.json", result.PlayerCompositionPackagePath.Replace('\\', '/'));
        Assert.EndsWith("identity-overlaid/package.json", result.ActivatedProjectPackagePath.Replace('\\', '/'));
        Assert.True(File.Exists(result.PlayerCompositionPackagePath));
        Assert.True(File.Exists(result.ActivatedProjectPackagePath));
    }
}
