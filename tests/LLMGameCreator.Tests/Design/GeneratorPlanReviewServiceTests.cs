using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanReviewServiceTests
{
    [Fact]
    public async Task ReviewServiceRevalidatesSavedValidPlan()
    {
        var store = Store(
            new[] { Module("core/base/v1"), Module("world/map/v1", "[\"core/base/v1\"]") },
            Steps(Step("core/base/v1", 1), Step("world/map/v1", 2, "[\"core/base/v1\"]")));
        var service = CreateService(store);

        var result = await service.RevalidatePlanAsync(store.Plan.Id, CancellationToken.None);

        Assert.True(result.CanApprove, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.DoesNotContain(result.ValidationIssues, issue => issue.Severity == "error");
        Assert.Equal(2, result.Steps.Count);
    }

    [Fact]
    public async Task ReviewServiceRejectsApprovalWhenRegistryModuleIsMissing()
    {
        var store = Store(Array.Empty<GeneratorModuleRecord>(), Steps(Step("core/base/v1", 1)));
        var service = CreateService(store);

        var result = await service.ApprovePlanAsync(store.Plan.Id, "reviewed", CancellationToken.None);

        Assert.False(result.Updated);
        Assert.Equal("draft", store.Plan.Status);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "plan.module_id.unknown");
    }

    [Fact]
    public async Task ReviewServiceRejectsApprovalWhenRequiredDependencyIsMissing()
    {
        var store = Store(
            new[] { Module("core/base/v1"), Module("world/map/v1", "[\"core/base/v1\"]") },
            Steps(Step("world/map/v1", 1)));
        var service = CreateService(store);

        var result = await service.ApprovePlanAsync(store.Plan.Id, "reviewed", CancellationToken.None);

        Assert.False(result.Updated);
        Assert.Equal("draft", store.Plan.Status);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "plan.dependency.missing");
    }

    [Fact]
    public async Task ReviewServiceRejectsApprovalWhenDependencyOrderIsInvalid()
    {
        var store = Store(
            new[] { Module("core/base/v1"), Module("world/map/v1", "[\"core/base/v1\"]") },
            Steps(Step("world/map/v1", 1), Step("core/base/v1", 2)));
        var service = CreateService(store);

        var result = await service.ApprovePlanAsync(store.Plan.Id, "reviewed", CancellationToken.None);

        Assert.False(result.Updated);
        Assert.Equal("draft", store.Plan.Status);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "plan.dependency.order");
    }

    [Fact]
    public async Task ReviewServiceAllowsApprovalWhenValidationHasNoErrors()
    {
        var store = Store(
            new[] { Module("core/base/v1"), Module("world/map/v1", "[\"core/base/v1\"]") },
            Steps(Step("core/base/v1", 1), Step("world/map/v1", 2)));
        var service = CreateService(store);

        var result = await service.ApprovePlanAsync(store.Plan.Id, "reviewed", CancellationToken.None);

        Assert.True(result.Updated);
        Assert.Equal("approved", store.Plan.Status);
    }

    [Fact]
    public async Task ReviewServiceRejectActionSetsRejectedWithoutLlm()
    {
        var store = Store(new[] { Module("core/base/v1") }, Steps(Step("core/base/v1", 1)));
        var service = CreateService(store);

        var result = await service.RejectPlanAsync(store.Plan.Id, "not useful", CancellationToken.None);

        Assert.True(result.Updated);
        Assert.Equal("rejected", store.Plan.Status);
        Assert.Equal(0, store.LlmCallCount);
    }

    [Fact]
    public async Task ReviewServiceArchiveActionSetsArchivedWithoutLlm()
    {
        var store = Store(new[] { Module("core/base/v1") }, Steps(Step("core/base/v1", 1)));
        var service = CreateService(store);

        var result = await service.ArchivePlanAsync(store.Plan.Id, "old", CancellationToken.None);

        Assert.True(result.Updated);
        Assert.Equal("archived", store.Plan.Status);
        Assert.Equal(0, store.LlmCallCount);
    }

    [Fact]
    public async Task RevalidationDoesNotExecuteLuaOrTouchPackageFiles()
    {
        using var temp = new TempDirectory();
        var packagePath = Path.Combine(temp.Path, "package.json");
        await File.WriteAllTextAsync(packagePath, "{\"unchanged\":true}", CancellationToken.None);
        var markerPath = Path.Combine(temp.Path, "executed.txt");
        var luaPath = Path.Combine(temp.Path, "danger.lua");
        await File.WriteAllTextAsync(luaPath, $"os.execute('echo executed > {markerPath}')", CancellationToken.None);
        var store = Store(new[] { Module("core/danger/v1", path: luaPath) }, Steps(Step("core/danger/v1", 1)));
        var service = CreateService(store);

        var result = await service.RevalidatePlanAsync(store.Plan.Id, CancellationToken.None);

        Assert.True(result.CanApprove, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.False(File.Exists(markerPath));
        Assert.Equal("{\"unchanged\":true}", await File.ReadAllTextAsync(packagePath, CancellationToken.None));
    }

    private static GeneratorPlanReviewService CreateService(InMemoryDesignStore store)
    {
        return new GeneratorPlanReviewService(store, store, new GeneratorPlanValidator());
    }

    private static InMemoryDesignStore Store(IReadOnlyList<GeneratorModuleRecord> modules, IReadOnlyList<GeneratorPlanStepRecord> steps)
    {
        return new InMemoryDesignStore(modules, new GeneratorPlanRecord("plan/test", "Plan", "Goal", "draft", "{}", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow), steps);
    }

    private static IReadOnlyList<GeneratorPlanStepRecord> Steps(params GeneratorPlanStepRecord[] steps)
    {
        return steps;
    }

    private static GeneratorPlanStepRecord Step(string moduleId, int order, string dependsOnJson = "[]")
    {
        return new GeneratorPlanStepRecord(
            $"step/{order}",
            "plan/test",
            order,
            moduleId,
            "{}",
            string.IsNullOrWhiteSpace(dependsOnJson) ? "[]" : dependsOnJson,
            "pending");
    }

    private static GeneratorModuleRecord Module(string id, string dependenciesJson = "[]", string path = "lua/core/base.lua")
    {
        return new GeneratorModuleRecord(id, "001", path, "core", "[\"core.base\"]", dependenciesJson, "[]", "[]", "[]", "manifests/test.manifest.json", "{}", DateTimeOffset.UtcNow);
    }

    private sealed class InMemoryDesignStore : IGeneratorLibraryRegistry, IGeneratorPlanRepository
    {
        private readonly IReadOnlyList<GeneratorModuleRecord> _modules;
        private readonly IReadOnlyList<GeneratorPlanStepRecord> _steps;

        public InMemoryDesignStore(IReadOnlyList<GeneratorModuleRecord> modules, GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps)
        {
            _modules = modules;
            Plan = plan;
            _steps = steps;
        }

        public GeneratorPlanRecord Plan { get; private set; }
        public int LlmCallCount { get; private set; }

        public Task SaveImportedLibraryAsync(GeneratorLibraryImportData data, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<CapabilityModuleRecord>> ListCapabilitiesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CapabilityModuleRecord>>(Array.Empty<CapabilityModuleRecord>());
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesAsync(CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<GeneratorModuleRecord?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken) => Task.FromResult(_modules.FirstOrDefault(module => module.Id == moduleId));
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesByCapabilityAsync(string capabilityId, CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<IReadOnlyList<GeneratorLibraryImportIssue>> ListImportIssuesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratorLibraryImportIssue>>(Array.Empty<GeneratorLibraryImportIssue>());
        public Task SaveGeneratorPlanAsync(GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps, PromptContextPackRecord? contextPack, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<GeneratorPlanRecord>> ListGeneratorPlansAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratorPlanRecord>>(new[] { Plan });
        public Task<GeneratorPlanRecord?> GetGeneratorPlanByIdAsync(string planId, CancellationToken cancellationToken) => Task.FromResult(Plan.Id == planId ? Plan : null);
        public Task<IReadOnlyList<GeneratorPlanStepRecord>> GetGeneratorPlanStepsAsync(string planId, CancellationToken cancellationToken) => Task.FromResult(_steps);

        public Task<bool> UpdateGeneratorPlanStatusAsync(string planId, string status, string? note, CancellationToken cancellationToken)
        {
            if (Plan.Id != planId)
            {
                return Task.FromResult(false);
            }

            Plan = Plan with { Status = status, UpdatedUtc = DateTimeOffset.UtcNow };
            return Task.FromResult(true);
        }
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
