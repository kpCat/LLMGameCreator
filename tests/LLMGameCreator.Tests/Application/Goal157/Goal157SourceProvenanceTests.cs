using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157SourceProvenanceTests
{
    [Fact]
    public void Behavioral_exact_valid_goal156_source_reproduces_full_chain()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics));
        Assert.NotNull(validation.Source);
        Assert.NotNull(validation.GeneratedMvpPackage);
        Assert.NotNull(validation.Overlay);
        Assert.NotNull(validation.GeneratedBasePackage);
    }

    [Fact]
    public void Behavioral_editing_only_source_seed_is_a_causal_failure()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "source-seed");
        Goal157TestKit.EditSource(copy.Path, root => root["seed"] = "goal157-altered-seed");

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_source_mode_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "source-mode");
        Goal157TestKit.EditSource(copy.Path, root => root["mode"] = ProceduralGameGenerationModes.AuthoredSmallWorld);

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_source_style_hints_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "source-style");
        Goal157TestKit.EditSource(copy.Path, root => root["styleHintIds"] = new JsonArray("style.dark-fantasy"));

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_source_variant_ids_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "source-variants");
        Goal157TestKit.EditSource(copy.Path, root => root["variantIds"] = new JsonArray("world.invalid-goal157"));

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_plan_from_another_seed_fails_even_when_sidecar_hashes_are_rewritten()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "other-plan");
        Goal157TestKit.ReplaceSidecar(copy.Path, Goal156TestKit.DifferentSeed.Path,
            SeededGeneratedProjectVocabulary.PlanJsonFileName, "planSha256");
        Goal157TestKit.ReplaceSidecar(copy.Path, Goal156TestKit.DifferentSeed.Path,
            SeededGeneratedProjectVocabulary.PlanMarkdownFileName);

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_stored_plan_metadata_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "plan-metadata");
        Goal157TestKit.EditSidecarJson(copy.Path, SeededGeneratedProjectVocabulary.PlanJsonFileName,
            root => root["metadata"]!["seed"] = "goal157-metadata-seed", "planSha256");

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.plan_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_altered_rule_pack_fails_regeneration()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "rule-pack");
        Goal157TestKit.AppendAndRehash(copy.Path, SeededGeneratedProjectVocabulary.RulePackJsonFileName, "rulePackSha256");

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.rule_pack_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_altered_tiny_loop_fails_regeneration()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "tiny-loop");
        Goal157TestKit.AppendAndRehash(copy.Path, SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName,
            "tinyLoopStateSha256");

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.tiny_loop_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_altered_mvp_fails_regeneration()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "mvp");
        Goal157TestKit.AppendAndRehash(copy.Path, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName,
            "generatedMvpPackageSha256");

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.mvp_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_self_consistent_altered_overlay_and_base_fail_canonical_rebuild()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "forged-overlay");
        Goal157TestKit.ReplaceSidecar(copy.Path, Goal156TestKit.DifferentSeed.Path,
            SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName, "generatedOverlaySha256");
        Goal157TestKit.ReplaceSidecar(copy.Path, Goal156TestKit.DifferentSeed.Path,
            SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName, "generatedBasePackageSha256");

        var validation = Goal156TestKit.SourceService.Validate(copy.Path);

        Assert.False(validation.Passed);
        Assert.Contains(validation.Diagnostics, item => item is "generated_source.overlay_regeneration_mismatch"
            or "generated_source.base_regeneration_mismatch");
    }

    [Fact]
    public void Behavioral_source_baseline_hash_mismatch_fails()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "baseline-hash");
        Goal157TestKit.EditSource(copy.Path, root => root["goal142BaselinePackageSha256"] = new string('0', 64));

        Goal157TestKit.AssertSourceFails(copy.Path, "generated_source.baseline_hash_mismatch");
    }

    [Fact]
    public void Behavioral_existing_valid_v1_source_remains_valid()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.CoreOnly.Path);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics));
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceSchemaVersion, validation.Source?.SchemaVersion);
    }

    [Fact]
    public void Behavioral_failed_validation_does_not_change_any_project_file()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "read-only-validation");
        Goal157TestKit.EditSource(copy.Path, root => root["seed"] = "goal157-read-only");
        var before = Goal157TestKit.TreeHashes(copy.Path);

        var validation = Goal156TestKit.SourceService.Validate(copy.Path);
        var after = Goal157TestKit.TreeHashes(copy.Path);

        Assert.False(validation.Passed);
        Assert.Equal(before, after);
    }

    [Fact]
    public void Contract_source_schema_remains_exact_v1_without_goal157_fields()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Goal157TestKit.SourcePath(Goal156TestKit.AllSelectable.Path)));

        Assert.Equal("seeded_generated_project_source_v1", document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.False(document.RootElement.TryGetProperty("activation", out _));
        Assert.False(document.RootElement.TryGetProperty("lane", out _));
    }
}

internal static partial class Goal157TestKit
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    public static string SourcePath(string project) => Path.Combine(project, ".llmgc", "generation",
        SeededGeneratedProjectVocabulary.SourceJsonFileName);

    public static string SidecarPath(string project, string fileName) =>
        Path.Combine(project, ".llmgc", "generation", fileName);

    public static void EditSource(string project, Action<JsonObject> edit)
    {
        var path = SourcePath(project);
        var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
        edit(root);
        File.WriteAllText(path, root.ToJsonString(WriteOptions), new UTF8Encoding(false));
    }

    public static void EditSidecarJson(string project, string fileName, Action<JsonObject> edit,
        string? directHashProperty = null)
    {
        var path = SidecarPath(project, fileName);
        var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
        edit(root);
        File.WriteAllText(path, root.ToJsonString(WriteOptions), new UTF8Encoding(false));
        Rehash(project, fileName, directHashProperty);
    }

    public static void AppendAndRehash(string project, string fileName, string directHashProperty)
    {
        File.AppendAllText(SidecarPath(project, fileName), " ", Encoding.UTF8);
        Rehash(project, fileName, directHashProperty);
    }

    public static void ReplaceSidecar(string project, string sourceProject, string fileName,
        string? directHashProperty = null)
    {
        File.Copy(SidecarPath(sourceProject, fileName), SidecarPath(project, fileName), overwrite: true);
        Rehash(project, fileName, directHashProperty);
    }

    public static void AssertSourceFails(string project, string diagnostic)
    {
        var validation = Goal156TestKit.SourceService.Validate(project);
        Assert.False(validation.Passed);
        Assert.Contains(diagnostic, validation.Diagnostics);
    }

    public static SortedDictionary<string, string> TreeHashes(string root) => new(
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(root, path).Replace('\\', '/'), Goal156TestKit.Hash,
                StringComparer.Ordinal), StringComparer.Ordinal);

    public static GameProjectGeneratedWorldActivationResult Activate(
        GeneratedProject project,
        IGameRuntime? runtime = null,
        IRuntimeStateSerializer? serializer = null,
        Action<GamePackageDefinition>? mutate = null)
    {
        var package = Goal156TestKit.Load(project.Path);
        mutate?.Invoke(package);
        Goal156TestKit.Repository.SaveAsync(project.Path, package, CancellationToken.None).GetAwaiter().GetResult();
        var source = Goal156TestKit.SourceService.Validate(project.Path);
        var manifest = package.Manifest;
        return new GameProjectGeneratedWorldActivationService(
            runtime ?? new DefaultGameRuntime(),
            serializer ?? new RuntimeStateSerializer(),
            Goal156TestKit.Validator).Activate(new GameProjectGeneratedWorldActivationRequest
            {
                CompatibilityPackagePath = Path.Combine(project.Path, "package.json"),
                CompatibilityPackage = package,
                GeneratedSource = source,
                ProjectIdentity = new GameProjectIdentityDocument
                {
                    PackageId = manifest.PackageId,
                    Title = manifest.Title,
                    Version = manifest.Version,
                    FormatVersion = manifest.FormatVersion,
                    Description = manifest.Description ?? string.Empty,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    UpdatedAtUtc = DateTimeOffset.UtcNow,
                    Source = GameProjectIdentityVocabulary.CreatedProjectPackageSource
                },
                OutputRoot = Path.Combine(project.Path, ".llmgc", "build", "goal157-direct-" + Guid.NewGuid().ToString("N"))
            });
    }

    public static string GeneratedStartMapId(GeneratedProject project) =>
        Goal156TestKit.SourceService.Validate(project.Path).Source!.GeneratedStartMapId;

    private static void Rehash(string project, string fileName, string? directHashProperty)
    {
        var hash = Goal156TestKit.Hash(SidecarPath(project, fileName));
        EditSource(project, root =>
        {
            root["sidecarSha256"]![fileName] = hash;
            if (directHashProperty is not null) root[directHashProperty] = hash;
        });
    }
}
