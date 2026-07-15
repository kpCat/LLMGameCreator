using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[Collection(Goal156Collection.Name)]
public sealed class Goal156DeterminismAndOverlayTests
{
    [Fact]
    public void Behavioral_same_seed_and_options_produce_identical_generation_sidecars()
    {
        var first = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path).Source!;
        var second = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectableRepeat.Path).Source!;

        Assert.Equal(first.SidecarSha256, second.SidecarSha256);
        Assert.Equal(first.PlanSha256, second.PlanSha256);
        Assert.Equal(first.GeneratedBasePackageSha256, second.GeneratedBasePackageSha256);
    }

    [Fact]
    public void Behavioral_different_seed_has_visible_plan_and_generated_base_variation()
    {
        var first = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path).Source!;
        var varied = Goal156TestKit.SourceService.Validate(Goal156TestKit.DifferentSeed.Path).Source!;

        Assert.NotEqual(first.PlanSha256, varied.PlanSha256);
        Assert.NotEqual(first.GeneratedMvpPackageSha256, varied.GeneratedMvpPackageSha256);
        Assert.NotEqual(first.GeneratedBasePackageSha256, varied.GeneratedBasePackageSha256);
        var firstMaps = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path)
            .GeneratedMvpPackage!.Game.Maps.Select(map => map.Name).OrderBy(value => value, StringComparer.Ordinal);
        var variedMaps = Goal156TestKit.SourceService.Validate(Goal156TestKit.DifferentSeed.Path)
            .GeneratedMvpPackage!.Game.Maps.Select(map => map.Name).OrderBy(value => value, StringComparer.Ordinal);
        Assert.NotEqual(string.Join("|", firstMaps), string.Join("|", variedMaps));
    }

    [Theory]
    [InlineData(ProceduralGameGenerationModes.AuthoredSmallWorld)]
    [InlineData(ProceduralGameGenerationModes.SemiProceduralRegions)]
    [InlineData(ProceduralGameGenerationModes.FullySeededWorld)]
    public async Task Behavioral_each_supported_generation_mode_creates_a_valid_source(string mode)
    {
        using var scope = Goal156TestKit.Scope("mode");
        var folder = "generated-" + mode.Replace('_', '-');
        var request = Goal156TestKit.GeneratedRequest(scope.Root, folder, "mode-seed-" + mode,
            mode: mode);

        var result = await scope.Service.CreateAsync(request, CancellationToken.None);
        var validation = Goal156TestKit.SourceService.Validate(result.FolderPath);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics));
        Assert.Equal(mode, validation.Source!.Mode);
        Assert.True(validation.Source.TinyLoop.Passed);
    }

    [Fact]
    public void Behavioral_overlay_is_anchored_to_the_immutable_goal142_baseline_hash()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);

        Assert.Equal(Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            validation.Source!.Goal142BaselinePackageSha256);
        Assert.Equal(validation.Source.Goal142BaselinePackageSha256,
            validation.Overlay!.Goal142BaselinePackageSha256);
        Assert.True(validation.Overlay.BaselineDefinitionsPreserved);
    }

    [Fact]
    public void Behavioral_generated_overlay_is_additive_and_namespaced()
    {
        var overlay = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path).Overlay!;

        Assert.True(overlay.GeneratedRecordsAdditive);
        Assert.True(overlay.AdditiveRecordCount > 0);
        Assert.Equal(overlay.GeneratedRecordCount, overlay.AdditiveRecordCount + overlay.DeduplicatedRecordCount);
        Assert.Contains(overlay.GeneratedRecords, item =>
            item.RecordId.StartsWith("generated/", StringComparison.Ordinal)
            || item.RecordId.StartsWith("seeded_generated_project/", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_generated_base_preserves_baseline_start_map_and_contains_generated_start_map()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);

        Assert.Equal(validation.Overlay!.BaselineStartMapId, validation.GeneratedBasePackage!.Manifest.StartMapId);
        Assert.Contains(validation.GeneratedBasePackage.Game.Maps,
            map => map.Id == validation.Overlay.GeneratedStartMapId);
        Assert.NotEqual(validation.Overlay.BaselineStartMapId, validation.Overlay.GeneratedStartMapId);
    }

    [Fact]
    public void Behavioral_every_baseline_and_generated_record_fingerprint_survives_the_overlay()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        var service = new GeneratedProjectOverlayService();

        var generatedBaseJson = File.ReadAllText(Path.Combine(Goal156TestKit.AllSelectable.Path, ".llmgc", "generation",
            SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName));
        var diagnostics = service.ValidatePackageRecords(generatedBaseJson, validation.Overlay!, includeBaseline: true);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Behavioral_differing_definition_id_collision_fails_instead_of_overwriting_baseline()
    {
        var generationRoot = Path.Combine(Goal156TestKit.AllSelectable.Path, ".llmgc", "generation");
        var baselineJson = File.ReadAllText(Goal156TestKit.Goal142BaselinePath);
        var generatedPath = Path.Combine(generationRoot, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName);
        var generated = JsonNode.Parse(File.ReadAllText(generatedPath))!.AsObject();
        var baseline = JsonNode.Parse(baselineJson)!.AsObject();
        var baselineId = baseline["game"]!["tilePrototypes"]!.AsArray()[0]!["id"]!.GetValue<string>();
        generated["game"]!["tilePrototypes"]!.AsArray()[0]!["id"] = baselineId;
        var plan = JsonSerializer.Deserialize<ProceduralGeneratedGamePlan>(
            File.ReadAllText(Path.Combine(generationRoot, SeededGeneratedProjectVocabulary.PlanJsonFileName)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var error = Assert.Throws<InvalidOperationException>(() => new GeneratedProjectOverlayService().Build(
            baselineJson,
            Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            generated.ToJsonString(),
            plan));

        Assert.Contains("generated_overlay.id_collision", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_tampered_sidecar_is_rejected_with_a_causal_hash_diagnostic()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "tampered-sidecar");
        var path = Path.Combine(copy.Path, ".llmgc", "generation",
            SeededGeneratedProjectVocabulary.PlanMarkdownFileName);
        File.AppendAllText(path, "tamper");

        var validation = Goal156TestKit.SourceService.Validate(copy.Path);

        Assert.True(validation.Present);
        Assert.False(validation.Passed);
        Assert.Equal("INVALID", validation.Status);
        Assert.Contains(validation.Diagnostics, item => item.Contains("sidecar_hash_mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_source_json_contains_only_the_declared_typed_contract_properties()
    {
        var path = Path.Combine(Goal156TestKit.AllSelectable.Path,
            SeededGeneratedProjectVocabulary.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var actual = document.RootElement.EnumerateObject().Select(property => property.Name)
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        var expected = typeof(SeededGeneratedProjectSourceRecord).GetProperties()
            .Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name))
            .OrderBy(value => value, StringComparer.Ordinal).ToList();

        Assert.Equal(expected, actual);
    }
}
