using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[Collection(Goal156Collection.Name)]
public sealed class Goal156CustomBaseCompositionTests
{
    [Fact]
    public void Behavioral_explicit_generated_base_is_the_actual_parameterized_composition_input()
    {
        var project = Goal156TestKit.CoreOnly;
        var state = Goal156TestKit.Authoring(project.Path);
        using var output = Goal156TestKit.Scope("custom-base-valid");
        var source = GeneratedBase(project.Path);
        var copied = Path.Combine(output.Root, "generated-base-package.json");
        File.Copy(source, copied);
        var before = Goal156TestKit.Hash(source);
        var descriptor = Descriptor(copied);

        var result = Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot,
            state.Library,
            state.Document,
            output.Root,
            useCapabilityDrivenRuntimePlaythrough: true,
            descriptor);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(descriptor.PackageSha256, result.Plan.CompositionPlan.BasePackageSha256);
        Assert.Equal(before, Goal156TestKit.Hash(source));
        Assert.Contains("seeded_generated_base", result.PackageJson, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_default_composition_overload_keeps_the_goal142_baseline_hash()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);
        using var output = Goal156TestKit.Scope("default-base-valid");
        var expected = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);

        var result = Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot,
            state.Library,
            state.Document,
            output.Root,
            useCapabilityDrivenRuntimePlaythrough: true);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(expected, result.Plan.CompositionPlan.BasePackageSha256);
        Assert.DoesNotContain("seeded_generated_base", result.PackageJson, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_explicit_base_hash_mismatch_is_rejected_before_composition()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);
        using var output = Goal156TestKit.Scope("custom-base-hash");
        var copied = CopyGeneratedBase(output.Root);
        var descriptor = Descriptor(copied) with { PackageSha256 = new string('0', 64) };

        var error = Assert.Throws<InvalidOperationException>(() => Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot, state.Library, state.Document, output.Root, true, descriptor));

        Assert.Contains("hash mismatch", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_incomplete_explicit_base_descriptor_is_rejected_causally()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);
        using var output = Goal156TestKit.Scope("custom-base-incomplete");

        var error = Assert.Throws<InvalidOperationException>(() => Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot, state.Library, state.Document, output.Root, true,
            new FeatureModuleCompositionBasePackage()));

        Assert.Contains("incomplete", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_unsupported_explicit_base_source_kind_is_rejected()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);
        using var output = Goal156TestKit.Scope("custom-base-kind");
        var descriptor = Descriptor(CopyGeneratedBase(output.Root)) with { SourceKind = "external-provider" };

        var error = Assert.Throws<InvalidOperationException>(() => Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot, state.Library, state.Document, output.Root, true, descriptor));

        Assert.Contains("source kind", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_explicit_base_path_outside_repository_and_output_is_rejected()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);
        using var output = Goal156TestKit.Scope("custom-base-escape-output");
        using var outside = Goal156TestKit.Scope("custom-base-escape-source");
        var path = CopyGeneratedBase(outside.Root);

        var error = Assert.Throws<InvalidOperationException>(() => Service().MaterializeAndQualify(
            Goal156TestKit.RepositoryRoot, state.Library, state.Document, output.Root, true, Descriptor(path)));

        Assert.Contains("path escape", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static FeatureModuleParameterizedCompositionService Service() => new(
        SelectedRuntimeVariantInteractiveSessionService.CreateDefault());

    private static FeatureModuleCompositionBasePackage Descriptor(string path) => new()
    {
        PackagePath = path,
        PackageSha256 = Goal156TestKit.Hash(path),
        SourceKind = FeatureModuleCompositionBasePackageSourceKinds.SeededGeneratedBase,
        SourceIdentity = "goal156-test-generated-base"
    };

    private static string CopyGeneratedBase(string outputRoot)
    {
        var target = Path.Combine(outputRoot, "generated-base-package.json");
        File.Copy(GeneratedBase(Goal156TestKit.CoreOnly.Path), target, overwrite: true);
        return target;
    }

    private static string GeneratedBase(string project) => Path.Combine(project, ".llmgc", "generation",
        SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
}
