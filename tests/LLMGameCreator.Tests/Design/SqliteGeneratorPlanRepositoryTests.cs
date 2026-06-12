using LLMGameCreator.Application.Design;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class SqliteGeneratorPlanRepositoryTests
{
    [Fact]
    public async Task SqliteRepositorySavesAndLoadsPlanWithOrderedSteps()
    {
        using var temp = new TempDirectory();
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, "design.db"), CancellationToken.None);
        var now = DateTimeOffset.UtcNow;

        await database.SaveGeneratorPlanAsync(
            new GeneratorPlanRecord("plan/test", "Plan", "Goal", "draft", "{}", now, now),
            new[]
            {
                new GeneratorPlanStepRecord("step/2", "plan/test", 2, "world/map/v1", "{}", "[]", "pending"),
                new GeneratorPlanStepRecord("step/1", "plan/test", 1, "core/base/v1", "{}", "[]", "pending")
            },
            new PromptContextPackRecord("context/test", "generator-plan-draft", "[]", "[\"core/base/v1\"]", 1000, "{}"),
            CancellationToken.None);

        var plan = Assert.Single(await database.ListGeneratorPlansAsync(CancellationToken.None));
        var steps = await database.GetGeneratorPlanStepsAsync(plan.Id, CancellationToken.None);

        Assert.Equal("draft", plan.Status);
        Assert.Collection(
            steps,
            step => Assert.Equal(1, step.StepOrder),
            step => Assert.Equal(2, step.StepOrder));
    }

    [Fact]
    public async Task SqliteRepositoryUpdatesPlanStatusAndUpdatedUtcWithoutDeletingSteps()
    {
        using var temp = new TempDirectory();
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, "design.db"), CancellationToken.None);
        var created = DateTimeOffset.UtcNow.AddMinutes(-10);

        await database.SaveGeneratorPlanAsync(
            new GeneratorPlanRecord("plan/status", "Plan", "Goal", "draft", "{}", created, created),
            new[]
            {
                new GeneratorPlanStepRecord("step/1", "plan/status", 1, "core/base/v1", "{}", "[]", "pending"),
                new GeneratorPlanStepRecord("step/2", "plan/status", 2, "world/map/v1", "{}", "[\"core/base/v1\"]", "pending")
            },
            null,
            CancellationToken.None);

        var updated = await database.UpdateGeneratorPlanStatusAsync("plan/status", "approved", "reviewed", CancellationToken.None);
        var plan = await database.GetGeneratorPlanByIdAsync("plan/status", CancellationToken.None);
        var steps = await database.GetGeneratorPlanStepsAsync("plan/status", CancellationToken.None);

        Assert.True(updated);
        Assert.NotNull(plan);
        Assert.Equal("approved", plan.Status);
        Assert.True(plan.UpdatedUtc > created);
        Assert.Contains("reviewed", plan.MetadataJson);
        Assert.Equal(2, steps.Count);
    }

    [Fact]
    public async Task SqliteRepositoryStatusUpdateReturnsFalseForUnknownPlan()
    {
        using var temp = new TempDirectory();
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, "design.db"), CancellationToken.None);

        var updated = await database.UpdateGeneratorPlanStatusAsync("plan/missing", "approved", null, CancellationToken.None);

        Assert.False(updated);
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
