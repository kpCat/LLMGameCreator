using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159SourceV2Tests
{
    [Fact]
    public void Behavioral_new_generated_creation_writes_exact_v2_source()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Goal159TestKit.SourcePath(
            Goal159TestKit.FreshV2.Path), Encoding.UTF8));
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(SeededGeneratedProjectVocabulary.SourceV2SchemaVersion,
            document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.Equal(Goal159TestKit.ExactV2Properties, names);
        Assert.False(document.RootElement.TryGetProperty("seed", out _));
    }

    [Fact]
    public void Behavioral_same_effective_options_preserve_gameplay_artifacts()
    {
        var first = Goal156TestKit.SourceService.Validate(Goal159TestKit.FreshV2.Path);
        var repeat = Goal156TestKit.SourceService.Validate(Goal159TestKit.FreshV2Repeat.Path);

        Assert.True(first.Passed, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.True(repeat.Passed, string.Join(Environment.NewLine, repeat.Diagnostics));
        Assert.Equal(first.Source?.PlanSha256, repeat.Source?.PlanSha256);
        Assert.Equal(first.Source?.GeneratedBasePackageSha256, repeat.Source?.GeneratedBasePackageSha256);
    }

    [Fact]
    public void Behavioral_v2_request_resolves_exact_options()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal159TestKit.FreshV2.Path);
        var resolved = new GenerationPresetOptionsService().Resolve(source.GenerationRequest!);

        Assert.True(source.Passed, string.Join(Environment.NewLine, source.Diagnostics));
        Assert.Equal(source.ResolvedGenerationOptions, resolved);
    }

    [Fact]
    public void Behavioral_preset_definition_hash_is_validated()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal159TestKit.FreshV2.Path);
        var expected = new GenerationPresetOptionsService().PresetDefinitionSha256(
            source.GenerationRequest!.PresetId);

        Assert.Equal(expected, source.ResolvedGenerationOptions?.PresetDefinitionSha256);
    }

    [Fact]
    public void Behavioral_no_style_override_uses_preset_defaults_truthfully()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal159TestKit.FreshV2.Path);
        var preset = new GenerationPresetOptionsService().GetPresets().Single(item =>
            item.PresetId == source.GenerationRequest!.PresetId);

        Assert.Empty(source.GenerationRequest!.CompactStyleHintIds);
        Assert.False(source.ResolvedGenerationOptions!.StyleOverridesApplied);
        Assert.Equal(preset.CompactStyleHintIds.OrderBy(value => value, StringComparer.Ordinal),
            source.ResolvedGenerationOptions.CompactStyleHintIds.OrderBy(value => value, StringComparer.Ordinal));
    }

    [Fact]
    public void Behavioral_explicit_style_override_is_represented_truthfully()
    {
        using var artifacts = Goal159TestKit.CreateArtifacts(new SeededGeneratedProjectGenerationRequest
        {
            Seed = "goal159-style",
            Mode = GenerationPresetOptionsService.DefaultMode,
            PresetId = GenerationPresetOptionsService.DefaultPresetId,
            CompactStyleHintIds = ["tone/mysterious", "theme/trade"]
        });

        Assert.True(artifacts.Validation.Passed, string.Join(Environment.NewLine, artifacts.Validation.Diagnostics));
        Assert.True(artifacts.Validation.ResolvedGenerationOptions?.StyleOverridesApplied);
        Assert.Equal(["theme/trade", "tone/mysterious"],
            artifacts.Validation.GenerationRequest?.CompactStyleHintIds);
    }

    [Fact]
    public void Behavioral_explicit_variant_override_is_represented_truthfully()
    {
        using var artifacts = Goal159TestKit.CreateArtifacts(new SeededGeneratedProjectGenerationRequest
        {
            Seed = "goal159-variant",
            Mode = GenerationPresetOptionsService.DefaultMode,
            PresetId = GenerationPresetOptionsService.DefaultPresetId,
            SelectedVariantIds = ["inventory_model/list_inventory", "combat_model/turn_based"]
        });

        Assert.True(artifacts.Validation.Passed, string.Join(Environment.NewLine, artifacts.Validation.Diagnostics));
        Assert.True(artifacts.Validation.ResolvedGenerationOptions?.VariantOverridesApplied);
        Assert.Equal(["combat_model/turn_based", "inventory_model/list_inventory"],
            artifacts.Validation.GenerationRequest?.SelectedVariantIds);
    }

    [Fact]
    public void Behavioral_v2_request_resolution_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal159TestKit.FreshV2, "goal159-v2-mismatch");
        Goal159TestKit.MutateSource(copy.Path, root =>
            root["resolvedGenerationOptions"]!["seed"] = "not-the-request-seed");

        var validation = Goal156TestKit.SourceService.Validate(copy.Path);

        Assert.False(validation.Passed);
        Assert.Contains("generated_source.v2_request_resolution_mismatch", validation.Diagnostics);
    }

    [Fact]
    public void Behavioral_preset_definition_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal159TestKit.FreshV2, "goal159-preset-hash");
        Goal159TestKit.MutateSource(copy.Path, root =>
            root["resolvedGenerationOptions"]!["presetDefinitionSha256"] = new string('0', 64));

        var validation = Goal156TestKit.SourceService.Validate(copy.Path);

        Assert.False(validation.Passed);
        Assert.Contains("generated_source.preset_definition_mismatch", validation.Diagnostics);
    }

    [Fact]
    public void Behavioral_valid_v1_source_remains_readable_without_rewrite()
    {
        using var project = Goal159TestKit.CreateV1Project("v1-readable");
        var path = Goal159TestKit.SourcePath(project.Path);
        var before = File.ReadAllBytes(path);

        var validation = Goal156TestKit.SourceService.Validate(project.Path);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics));
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceSchemaVersion, validation.Source?.SchemaVersion);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Fact]
    public void Behavioral_v1_inferred_request_is_exposed()
    {
        using var project = Goal159TestKit.CreateV1Project("v1-inferred");

        var validation = Goal156TestKit.SourceService.Validate(project.Path);

        Assert.Equal(SeededGeneratedProjectRequestOrigins.LegacyV1EffectiveOptions,
            validation.RequestOrigin);
        Assert.Equal(validation.Source?.Seed, validation.GenerationRequest?.Seed);
        Assert.NotEmpty(validation.GenerationRequest!.CompactStyleHintIds);
    }

    [Fact]
    public void Behavioral_v1_successful_regeneration_writes_v2()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(SeededGeneratedProjectVocabulary.SourceSchemaVersion, fixture.OldSourceSchema);
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceV2SchemaVersion,
            fixture.Source.Source?.SchemaVersion);
        Assert.Equal(SeededGeneratedProjectRequestOrigins.ExplicitV2Request,
            fixture.Source.RequestOrigin);
    }

    [Fact]
    public async Task Behavioral_template_lane_remains_byte_compatible()
    {
        using var scope = Goal156TestKit.Scope("goal159-template");
        var request = Goal156TestKit.TemplateRequest(scope.Root, "template");
        var expected = new NewGamePackageFactory().Create(request);

        var summary = await scope.Service.CreateAsync(request, CancellationToken.None);
        var actual = Goal156TestKit.Load(summary.FolderPath);

        Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(actual));
        Assert.False(File.Exists(Goal159TestKit.SourcePath(summary.FolderPath)));
    }
}

internal static partial class Goal159TestKit
{
    private static readonly Lazy<GeneratedProject> FreshV2Lazy = new(() => CreateFreshV2("fresh-v2"));
    private static readonly Lazy<GeneratedProject> FreshV2RepeatLazy = new(() => CreateFreshV2("fresh-v2-repeat"));

    public static GeneratedProject FreshV2 => FreshV2Lazy.Value;
    public static GeneratedProject FreshV2Repeat => FreshV2RepeatLazy.Value;
    public static readonly IReadOnlySet<string> ExactV2Properties = new HashSet<string>(StringComparer.Ordinal)
    {
        "schemaVersion", "creationKind", "generationRequest", "resolvedGenerationOptions",
        "mechanicsProfileId", "planId", "planSha256", "rulePackId", "rulePackSha256",
        "tinyLoopStateSha256", "generatedMvpPackageSha256", "generatedOverlaySha256",
        "generatedBasePackageSha256", "goal142BaselinePackageSha256", "generatedStartMapId",
        "counts", "tinyLoop", "sidecarSha256"
    };

    public static string SourcePath(string project) => Path.Combine(project,
        SeededGeneratedProjectVocabulary.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar));

    public static GeneratedProject CreateV1Project(string suffix) =>
        CreateV1Project(Goal156TestKit.AllSelectable, suffix);

    public static GeneratedProject CreateV1Project(GeneratedProject source, string suffix)
    {
        var copy = Goal156TestKit.Copy(source, "goal159-" + suffix);
        var sourcePath = SourcePath(copy.Path);
        var v2 = JsonNode.Parse(File.ReadAllText(sourcePath, Encoding.UTF8))!.AsObject();
        if (v2["schemaVersion"]?.GetValue<string>() == SeededGeneratedProjectVocabulary.SourceSchemaVersion)
            return copy;
        var resolved = v2["resolvedGenerationOptions"]!.AsObject();
        var v1 = new JsonObject
        {
            ["schemaVersion"] = SeededGeneratedProjectVocabulary.SourceSchemaVersion,
            ["creationKind"] = v2["creationKind"]!.DeepClone(),
            ["seed"] = resolved["seed"]!.DeepClone(),
            ["mode"] = resolved["mode"]!.DeepClone(),
            ["presetId"] = resolved["presetId"]!.DeepClone(),
            ["styleHintIds"] = resolved["compactStyleHintIds"]!.DeepClone(),
            ["variantIds"] = resolved["selectedVariantIds"]!.DeepClone()
        };
        foreach (var name in new[]
                 {
                     "mechanicsProfileId", "planId", "planSha256", "rulePackId", "rulePackSha256",
                     "tinyLoopStateSha256", "generatedMvpPackageSha256", "generatedOverlaySha256",
                     "generatedBasePackageSha256", "goal142BaselinePackageSha256", "generatedStartMapId",
                     "counts", "tinyLoop", "sidecarSha256"
                 })
            v1[name] = v2[name]!.DeepClone();
        File.WriteAllText(sourcePath, v1.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(false));
        return copy;
    }

    private static GeneratedProject CreateFreshV2(string folder)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal159V2",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var request = Goal156TestKit.GeneratedRequest(root, folder, "goal159-v2-stable-seed");
        var service = new GameProjectService(
            Goal156TestKit.Repository,
            Goal156TestKit.Validator,
            new NewGamePackageFactory(),
            Goal156TestKit.Creation);
        var summary = service.CreateAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        return new GeneratedProject(root, summary.FolderPath, request, service, DeleteOnDispose: false);
    }

    public static void MutateSource(string project, Action<JsonObject> mutation)
    {
        var path = SourcePath(project);
        var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
        mutation(root);
        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(false));
    }

    public static ArtifactScope CreateArtifacts(SeededGeneratedProjectGenerationRequest request)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal159Artifacts",
            Guid.NewGuid().ToString("N"));
        var generation = Path.Combine(root, ".llmgc", "generation");
        var baseline = new Goal142GeneratedProjectBaselineProvider(Goal156TestKit.RepositoryRoot);
        var source = new SeededGeneratedProjectSourceService(
            Goal156TestKit.Validator,
            baselineProvider: baseline);
        var result = new SeededGeneratedProjectArtifactFactory(baseline, Goal156TestKit.Validator)
            .Create(new SeededGeneratedProjectArtifactFactoryRequest
            {
                GenerationRequest = request,
                MechanicsProfileId = GeneratedProjectMechanicsProfiles.AllSelectableDefaults,
                OutputDirectory = generation
            });
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        return new ArtifactScope(root, source.Validate(root));
    }
}

internal sealed record ArtifactScope(string Root, SeededGeneratedProjectSourceValidationResult Validation) : IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}
