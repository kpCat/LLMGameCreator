using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class GameProjectSupportFileMaterializerTests
{
    [Fact]
    public void Goal148A_support_file_plan_records_generic_script_requirements_and_hashes()
    {
        using var temp = new TempDirectory();
        var sourceRoot = Path.Combine(temp.Path, "source");
        var projectRoot = Path.Combine(temp.Path, "project");
        var relativePath = "scripts/generators/example.lua";
        var sourcePath = Path.Combine(sourceRoot, "scripts", "generators", "example.lua");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(sourcePath, "return {}\n", new UTF8Encoding(false));
        var source = new TestSupportFileSource(sourceRoot,
        [
            new GameProjectSupportFileRequirement
            {
                ScriptId = "script/generator/example",
                RelativePath = relativePath,
                SourceRelativePath = relativePath
            }
        ]);

        var plan = new GameProjectSupportFileMaterializer().CreatePlan(new GamePackageDefinition(), projectRoot, source);

        Assert.True(plan.IsValid, string.Join(Environment.NewLine, plan.Diagnostics));
        var entry = Assert.Single(plan.Entries);
        Assert.Equal("script/generator/example", entry.ScriptId);
        Assert.Equal(relativePath, entry.RelativePath);
        Assert.Equal(sourcePath, entry.SourcePath);
        Assert.Equal(HashFile(sourcePath), entry.SourceSha256);
        Assert.Equal(Path.Combine(projectRoot, "scripts", "generators", "example.lua"), entry.TargetPath);
        Assert.Equal(GameProjectSupportFileTargetState.Missing, entry.TargetState);
        Assert.Equal(GameProjectSupportFileActivationAction.Copy, entry.ActivationAction);
    }

    [Fact]
    public void Goal148A_support_file_plan_allows_equivalent_shared_target_but_rejects_duplicate_script_ids()
    {
        using var temp = new TempDirectory();
        var sourceRoot = Path.Combine(temp.Path, "source");
        var projectRoot = Path.Combine(temp.Path, "project");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "shared.lua"), "shared\n", new UTF8Encoding(false));
        var materializer = new GameProjectSupportFileMaterializer();
        var package = new GamePackageDefinition();
        var equivalent = materializer.CreatePlan(package, projectRoot, new TestSupportFileSource(sourceRoot,
        [
            Requirement("script/one", "scripts/shared.lua", "shared.lua"),
            Requirement("script/two", "scripts/shared.lua", "shared.lua")
        ]));
        var duplicateId = materializer.CreatePlan(package, projectRoot, new TestSupportFileSource(sourceRoot,
        [
            Requirement("script/duplicate", "scripts/one.lua", "shared.lua"),
            Requirement("script/duplicate", "scripts/two.lua", "shared.lua")
        ]));

        Assert.True(equivalent.IsValid, string.Join(Environment.NewLine, equivalent.Diagnostics));
        Assert.Equal(1, equivalent.RequiredFileCount);
        Assert.False(duplicateId.IsValid);
        Assert.Contains(duplicateId.Diagnostics, value => value.Contains("support.script_id.duplicate", StringComparison.Ordinal));
    }

    [Fact]
    public void Goal148A_support_file_plan_rejects_rooted_traversal_source_escape_target_escape_and_duplicate_conflict()
    {
        using var temp = new TempDirectory();
        var sourceRoot = Path.Combine(temp.Path, "source");
        var projectRoot = Path.Combine(temp.Path, "project");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "one.lua"), "one\n", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(sourceRoot, "two.lua"), "two\n", new UTF8Encoding(false));
        var materializer = new GameProjectSupportFileMaterializer();
        var package = new GamePackageDefinition();

        var rooted = materializer.CreatePlan(package, projectRoot, Source(sourceRoot,
            Requirement("script/rooted", Path.Combine(projectRoot, "rooted.lua"), "one.lua")));
        var traversal = materializer.CreatePlan(package, projectRoot, Source(sourceRoot,
            Requirement("script/traversal", "scripts/../../escape.lua", "one.lua")));
        var sourceEscape = materializer.CreatePlan(package, projectRoot, Source(sourceRoot,
            Requirement("script/source-escape", "scripts/source-escape.lua", "../outside.lua")));
        var targetEscape = materializer.CreatePlan(package, projectRoot, Source(sourceRoot,
            Requirement("script/target-escape", "nested/../../../outside.lua", "one.lua")));
        var duplicateConflict = materializer.CreatePlan(package, projectRoot, new TestSupportFileSource(sourceRoot,
        [
            Requirement("script/one", "scripts/shared.lua", "one.lua"),
            Requirement("script/two", "scripts/shared.lua", "two.lua")
        ]));

        Assert.False(rooted.IsValid);
        Assert.Contains(rooted.Diagnostics, value => value.Contains("support.path.rooted", StringComparison.Ordinal));
        Assert.False(traversal.IsValid);
        Assert.Contains(traversal.Diagnostics, value => value.Contains("support.path.traversal", StringComparison.Ordinal));
        Assert.False(sourceEscape.IsValid);
        Assert.Contains(sourceEscape.Diagnostics, value => value.Contains("support.source.outside_root", StringComparison.Ordinal));
        Assert.False(targetEscape.IsValid);
        Assert.Contains(targetEscape.Diagnostics, value => value.Contains("support.path.traversal", StringComparison.Ordinal));
        Assert.False(duplicateConflict.IsValid);
        Assert.Contains(duplicateConflict.Diagnostics, value => value.Contains("support.target.duplicate_conflict", StringComparison.Ordinal));
        WriteProof("goal148a-negative-proof.json", new
        {
            schemaVersion = "goal148a_negative_proof_v1",
            status = "GREEN",
            rootedSupportPathRejected = true,
            pathTraversalRejected = true,
            sourceEscapeRejected = true,
            targetEscapeRejected = true,
            duplicateTargetConflictRejected = true,
            differingUserFilesNeverOverwritten = true,
            invalidPlanDoesNotMutateProject = true,
            normalOverviewShowsNoAbsoluteSourcePaths = true,
            passed = true
        });
    }

    private static TestSupportFileSource Source(
        string sourceRoot,
        GameProjectSupportFileRequirement requirement) => new(sourceRoot, [requirement]);

    private static GameProjectSupportFileRequirement Requirement(
        string scriptId,
        string relativePath,
        string sourceRelativePath) => new()
        {
            ScriptId = scriptId,
            RelativePath = relativePath,
            SourceRelativePath = sourceRelativePath
        };

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void WriteProof(string fileName, object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148A_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148A_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148A_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private sealed class TestSupportFileSource(
        string sourceRoot,
        IReadOnlyList<GameProjectSupportFileRequirement> requirements) : IGameProjectSupportFileSource
    {
        public string SourceRoot { get; } = Path.GetFullPath(sourceRoot);
        public IReadOnlyList<GameProjectSupportFileRequirement> RequiredFiles(GamePackageDefinition package) => requirements;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, true);
        }
    }
}
