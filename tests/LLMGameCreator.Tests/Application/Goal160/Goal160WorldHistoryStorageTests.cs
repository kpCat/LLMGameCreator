using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal159;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160WorldHistoryStorageTests
{
    [Fact]
    public void Behavioral_regeneration_archives_current_and_candidate_worlds_atomically()
    {
        var history = History();
        Assert.True(history.Passed, string.Join(Environment.NewLine, history.Diagnostics));
        Assert.Equal(2, history.Entries.Count);
        Assert.Single(history.Entries, entry => entry.IsCurrent);
    }

    [Fact]
    public void Behavioral_world_id_is_deterministic_from_source_and_generated_base()
    {
        var fixture = Goal159SuccessState.Value;
        var service = new GeneratedWorldHistoryService(fixture.Bundle.Source);
        var source = fixture.Bundle.Source.Validate(fixture.Project.Path);
        Assert.Equal(service.WorldId(fixture.Project.Path, source), service.WorldId(fixture.Project.Path, source));
        Assert.Equal(64, service.WorldId(fixture.Project.Path, source).Length);
    }

    [Fact]
    public void Behavioral_duplicate_exact_archive_is_a_no_op()
    {
        var fixture = Goal159SuccessState.Value;
        var service = new GeneratedWorldHistoryService(fixture.Bundle.Source);
        var history = service.ReadAll(fixture.Project.Path);
        using var staging = OperationProject.Create();
        var staged = service.Stage(fixture.Project.Path, fixture.Project.Path, staging.Path,
            GeneratedWorldHistoryOperationKinds.RegenerationAfter);
        Assert.True(staged.Passed);
        Assert.True(staged.AlreadyPresent);
        Assert.Equal(history.CurrentWorldId, staged.WorldId);
    }

    [Fact]
    public void Behavioral_manifest_tree_and_strict_source_validation_pass_for_every_entry()
    {
        var history = History();
        Assert.All(history.Entries, entry =>
        {
            Assert.True(entry.Passed, string.Join(Environment.NewLine, entry.Diagnostics));
            Assert.NotNull(entry.SourceValidation);
            Assert.True(entry.SourceValidation!.Passed);
            Assert.Equal(entry.WorldId, entry.Manifest?.WorldId);
        });
    }

    [Fact]
    public void Behavioral_tampered_history_sidecar_is_rejected()
    {
        var copy = CopyEntry();
        using var scope = copy.Scope;
        var service = Service();
        File.AppendAllText(Directory.EnumerateFiles(Path.Combine(copy.EntryPath, "generation"), "*.json")
            .First(path => !path.EndsWith("seeded-project-source.json", StringComparison.Ordinal)),
            "tamper", Encoding.UTF8);
        var result = service.ValidateEntry(copy.EntryPath);
        Assert.Contains("world_history.tree_hash_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_same_id_unequal_content_is_rejected_as_collision()
    {
        var fixture = Goal159SuccessState.Value;
        var service = Service();
        var current = History().Entries.Single(entry => entry.IsCurrent);
        var manifestPath = Path.Combine(current.EntryPath, "manifest.json");
        var before = File.ReadAllBytes(manifestPath);
        var node = JsonNode.Parse(File.ReadAllText(manifestPath, Encoding.UTF8))!.AsObject();
        node["seed"] = "tampered-seed";
        File.WriteAllText(manifestPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(false));
        try
        {
            using var staging = OperationProject.Create();
            var result = service.Stage(fixture.Project.Path, fixture.Project.Path, staging.Path,
                GeneratedWorldHistoryOperationKinds.RegenerationAfter);
            Assert.Contains("world_history.identity_collision", result.Diagnostics);
        }
        finally
        {
            File.WriteAllBytes(manifestPath, before);
        }
    }

    [Fact]
    public void Behavioral_path_escape_world_id_is_rejected()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            Service().Read(Goal159SuccessState.Value.Project.Path, "../escape"));
        Assert.Equal("world_history.path_escape", error.Message);
    }

    [Fact]
    public void Behavioral_history_contains_generation_only_not_current_truth()
    {
        foreach (var entry in History().Entries)
        {
            var files = Directory.EnumerateFiles(entry.EntryPath, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(entry.EntryPath, path).Replace('\\', '/')).ToList();
            Assert.Contains("manifest.json", files);
            Assert.DoesNotContain(files, path => path.Equals("package.json", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("authoring", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("identity", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(files, path => path.Contains("release-candidate", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Behavioral_plain_open_does_not_create_additional_history()
    {
        var fixture = Goal159SuccessState.Value;
        var before = History().Entries.Select(entry => entry.WorldId).ToList();
        fixture.Bundle.Controller.OpenProject(fixture.Project.Path);
        var after = History().Entries.Select(entry => entry.WorldId).ToList();
        Assert.Equal(before, after);
    }

    private static GeneratedWorldHistoryService Service() =>
        new(Goal159SuccessState.Value.Bundle.Source);

    private static GeneratedWorldHistoryReadResult History() =>
        Service().ReadAll(Goal159SuccessState.Value.Project.Path);

    private static (OperationProject Scope, string EntryPath) CopyEntry()
    {
        var entry = History().Entries.First();
        var scope = OperationProject.Create();
        var copy = Path.Combine(scope.Path, entry.WorldId);
        Directory.CreateDirectory(copy);
        CopyDirectory(entry.EntryPath, copy);
        return (scope, copy);
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }
}
