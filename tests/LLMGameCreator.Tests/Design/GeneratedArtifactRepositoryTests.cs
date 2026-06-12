using LLMGameCreator.Application.Design;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratedArtifactRepositoryTests
{
    [Fact]
    public async Task RepositorySavesAndListsGeneratedArtifactsByPlan()
    {
        using var temp = new TempDirectory();
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, "design.db"), CancellationToken.None);

        var artifact = new GeneratedArtifactRecord(
            "artifact/generator-plan-preview/plan/test",
            "generator_plan_preview",
            "design-db://generator-plans/plan/test/preview",
            "{\"kind\":\"generator_plan_preview\"}",
            "plan/test",
            "warning",
            "{}");

        await database.SaveGeneratedArtifactAsync(artifact, CancellationToken.None);

        var all = await database.ListGeneratedArtifactsAsync(CancellationToken.None);
        var byPlan = await database.ListGeneratedArtifactsByPlanAsync("plan/test", CancellationToken.None);
        var loaded = await database.GetGeneratedArtifactByIdAsync(artifact.Id, CancellationToken.None);

        Assert.Single(all);
        Assert.Single(byPlan);
        Assert.NotNull(loaded);
        Assert.Equal(artifact.Path, loaded.Path);
    }

    [Fact]
    public async Task RepositoryReplacesValidationResultsForOneArtifact()
    {
        using var temp = new TempDirectory();
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, "design.db"), CancellationToken.None);
        const string artifactId = "artifact/generator-plan-preview/plan/test";

        await database.SaveValidationResultsAsync(artifactId, new[]
        {
            Result("old/error", artifactId, "error", "preview.plan.validation_error"),
            Result("old/warning", artifactId, "warning", "preview.policy.no_execution")
        }, CancellationToken.None);
        await database.SaveValidationResultsAsync(artifactId, new[]
        {
            Result("new/warning", artifactId, "warning", "preview.policy.no_execution")
        }, CancellationToken.None);

        var results = await database.ListValidationResultsByArtifactAsync(artifactId, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal("new/warning", result.Id);
        Assert.Equal("preview.policy.no_execution", result.Code);
    }

    private static GeneratedArtifactValidationResultRecord Result(string id, string artifactId, string severity, string code)
    {
        return new GeneratedArtifactValidationResultRecord(id, artifactId, severity, code, code, artifactId, "{}");
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
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
