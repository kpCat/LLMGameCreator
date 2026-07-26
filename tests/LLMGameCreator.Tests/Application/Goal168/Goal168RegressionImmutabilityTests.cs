using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168RegressionImmutabilityTests
{
    [Fact]
    public void Behavioral_all_selectable_source_sidecars_stay_byte_identical() =>
        AssertSidecars(Goal168TestKit.Real);

    [Fact]
    public void Behavioral_core_only_source_sidecars_stay_byte_identical() =>
        AssertSidecars(Goal164TestKit.CoreOnly);

    [Fact]
    public void Behavioral_regeneration_rollback_restores_source_sidecars()
    {
        var state = Goal164RegenerationState.Value;
        var generationRoot = Path.Combine(state.Build.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));

        Assert.All(state.Build.GenerationSidecarHashesBefore, pair =>
            Assert.Equal(pair.Value, Goal164TestKit.FileSha(
                Path.Combine(generationRoot, pair.Key))));
    }

    [Fact]
    public void Contract_goal142_procedural_and_export_twins_are_byte_identical()
    {
        Assert.Equal(
            FileHash(Path.Combine(".llmgc", "procedural",
                "goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff",
                "goal142-human-acceptance-record.json")),
            FileHash(Path.Combine(".llmgc", "exports",
                "goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff",
                "goal142-human-acceptance-record.json")));
    }

    [Fact]
    public void Contract_goal148_procedural_and_export_trees_are_byte_identical()
    {
        foreach (var name in new[]
                 {
                     "goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation",
                     "goal-148a-new-project-required-support-files-and-transactional-activation-hotfix",
                     "goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix",
                     "goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix"
                 })
        {
            Assert.Equal(TreeHash(Path.Combine(".llmgc", "procedural", name)),
                TreeHash(Path.Combine(".llmgc", "exports", name)));
        }
    }

    private static void AssertSidecars(Goal164BuildFixture build)
    {
        var generationRoot = Path.Combine(build.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        Assert.All(build.GenerationSidecarHashesBefore, pair =>
            Assert.Equal(pair.Value, Goal164TestKit.FileSha(
                Path.Combine(generationRoot, pair.Key))));
    }

    private static string FileHash(string path)
    {
        using var stream = File.OpenRead(Path.Combine(RepositoryRoot(),
            path));
        return Convert.ToHexString(SHA256.HashData(stream))
            .ToLowerInvariant();
    }

    private static string TreeHash(string root)
    {
        var full = Path.Combine(RepositoryRoot(), root);
        var stable = string.Join("\n",
            Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => Path.GetRelativePath(full, path)
                                    .Replace('\\', '/')
                                + "|" + FileHash(path)));
        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(stable))).ToLowerInvariant();
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName,
                    "LLMGameCreator.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException(
            "LLMGameCreator.sln was not found.");
    }
}
