using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanPreviewServiceTests
{
    [Fact]
    public async Task PreviewServiceRejectsMissingPlan()
    {
        var store = new InMemoryDesignStore(Array.Empty<GeneratorModuleRecord>(), null, Array.Empty<GeneratorPlanStepRecord>());
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest("plan/missing"), CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(result.Artifact);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "preview.plan.not_found");
        Assert.Empty(store.SavedArtifacts);
    }

    [Theory]
    [InlineData("draft")]
    [InlineData("rejected")]
    [InlineData("archived")]
    public async Task PreviewServiceRejectsNonApprovedPlans(string status)
    {
        var store = Store(status, new[] { Module("core/base/v1") }, Step("core/base/v1", 1));
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(store.Plan!.Id), CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(result.Artifact);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "preview.plan.not_approved");
        Assert.Empty(store.SavedArtifacts);
    }

    [Fact]
    public async Task PreviewServiceRejectsApprovedPlanWhenCurrentRegistryValidationHasErrors()
    {
        var store = Store("approved", Array.Empty<GeneratorModuleRecord>(), Step("core/missing/v1", 1));
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(store.Plan!.Id), CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(result.Artifact);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "plan.module_id.unknown");
        Assert.Contains(result.ValidationResults, issue => issue.Code == "preview.plan.validation_error");
        Assert.Empty(store.SavedArtifacts);
    }

    [Fact]
    public async Task PreviewServiceCreatesArtifactForApprovedValidPlan()
    {
        var store = Store(
            "approved",
            new[] { Module("core/base/v1"), Module("world/map/v1", dependenciesJson: "[\"core/base/v1\"]", path: "generator-library/lua/world/map.lua") },
            Step("core/base/v1", 1, configJson: "{\"seed\":7}"),
            Step("world/map/v1", 2, dependsOnJson: "[\"core/base/v1\"]"));
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(store.Plan!.Id), CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        Assert.NotNull(result.Artifact);
        Assert.Equal("generator_plan_preview", result.Artifact.Kind);
        Assert.Equal(store.Plan.Id, result.Artifact.GeneratedBy);
        Assert.Single(store.SavedArtifacts);
        Assert.Contains(store.SavedValidationResults, item => item.Code == "preview.policy.no_execution");
    }

    [Fact]
    public async Task PreviewArtifactJsonIncludesModuleMetadataAndNoExecutionPolicy()
    {
        var store = Store(
            "approved",
            new[] { Module("core/base/v1", path: "generator-library/lua/core/base.lua") },
            Step("core/base/v1", 1, configJson: "{\"difficulty\":\"easy\"}"));
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(store.Plan!.Id), CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        var root = JsonNode.Parse(result.Artifact!.Json)!.AsObject();
        Assert.Equal(store.Plan.Id, root["plan"]!["id"]!.GetValue<string>());
        Assert.Equal("core/base/v1", root["steps"]![0]!["module_id"]!.GetValue<string>());
        Assert.Equal("generator-library/lua/core/base.lua", root["steps"]![0]!["module_path"]!.GetValue<string>());
        Assert.Equal("easy", root["steps"]![0]!["config"]!["difficulty"]!.GetValue<string>());
        Assert.False(root["execution_policy"]!["lua_execution"]!.GetValue<bool>());
        Assert.False(root["execution_policy"]!["module_execution"]!.GetValue<bool>());
        Assert.False(root["execution_policy"]!["game_package_mutation"]!.GetValue<bool>());
        Assert.False(root["execution_policy"]!["codegen_execution"]!.GetValue<bool>());
    }

    [Fact]
    public async Task PreviewServiceDoesNotCallLlmExecuteLuaOrTouchPackageFiles()
    {
        using var temp = new TempDirectory();
        var packagePath = Path.Combine(temp.Path, "package.json");
        await File.WriteAllTextAsync(packagePath, "{\"unchanged\":true}", CancellationToken.None);
        var markerPath = Path.Combine(temp.Path, "executed.txt");
        var luaSource = $"os.execute('echo executed > {markerPath}')";
        var luaPath = Path.Combine(temp.Path, "danger.lua");
        await File.WriteAllTextAsync(luaPath, luaSource, CancellationToken.None);
        var store = Store("approved", new[] { Module("core/danger/v1", path: luaPath) }, Step("core/danger/v1", 1));
        var service = CreateService(store);

        var result = await service.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(store.Plan!.Id), CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        Assert.False(File.Exists(markerPath));
        Assert.Equal("{\"unchanged\":true}", await File.ReadAllTextAsync(packagePath, CancellationToken.None));
        Assert.DoesNotContain("os.execute", result.Artifact!.Json);
        Assert.DoesNotContain(luaSource, result.Artifact.Json);
        Assert.Equal(0, store.LlmCallCount);
    }

    private static GeneratorPlanPreviewService CreateService(InMemoryDesignStore store)
    {
        return new GeneratorPlanPreviewService(
            store,
            store,
            new GeneratorPlanReviewService(store, store, new GeneratorPlanValidator()),
            store);
    }

    private static InMemoryDesignStore Store(string status, IReadOnlyList<GeneratorModuleRecord> modules, params GeneratorPlanStepRecord[] steps)
    {
        var now = DateTimeOffset.UtcNow;
        return new InMemoryDesignStore(modules, new GeneratorPlanRecord("plan/test", "Plan", "Goal", status, "{}", now, now), steps);
    }

    private static GeneratorModuleRecord Module(string id, string dependenciesJson = "[]", string path = "generator-library/lua/core/base.lua")
    {
        return new GeneratorModuleRecord(id, "001", path, "core", "[\"core.base\"]", dependenciesJson, "[]", "[]", "[]", "manifests/test.manifest.json", "{}", DateTimeOffset.UtcNow);
    }

    private static GeneratorPlanStepRecord Step(string moduleId, int order, string configJson = "{}", string dependsOnJson = "[]")
    {
        return new GeneratorPlanStepRecord($"step/{order}", "plan/test", order, moduleId, configJson, dependsOnJson, "pending");
    }

    private sealed class InMemoryDesignStore : IGeneratorLibraryRegistry, IGeneratorPlanRepository, IGeneratedArtifactRepository
    {
        private readonly IReadOnlyList<GeneratorModuleRecord> _modules;
        private readonly IReadOnlyList<GeneratorPlanStepRecord> _steps;

        public InMemoryDesignStore(IReadOnlyList<GeneratorModuleRecord> modules, GeneratorPlanRecord? plan, IReadOnlyList<GeneratorPlanStepRecord> steps)
        {
            _modules = modules;
            Plan = plan;
            _steps = steps;
        }

        public GeneratorPlanRecord? Plan { get; }
        public List<GeneratedArtifactRecord> SavedArtifacts { get; } = new();
        public List<GeneratedArtifactValidationResultRecord> SavedValidationResults { get; } = new();
        public int LlmCallCount { get; }

        public Task SaveImportedLibraryAsync(GeneratorLibraryImportData data, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<CapabilityModuleRecord>> ListCapabilitiesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CapabilityModuleRecord>>(Array.Empty<CapabilityModuleRecord>());
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesAsync(CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<GeneratorModuleRecord?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken) => Task.FromResult(_modules.FirstOrDefault(module => module.Id == moduleId));
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesByCapabilityAsync(string capabilityId, CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<IReadOnlyList<GeneratorLibraryImportIssue>> ListImportIssuesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratorLibraryImportIssue>>(Array.Empty<GeneratorLibraryImportIssue>());
        public Task SaveGeneratorPlanAsync(GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps, PromptContextPackRecord? contextPack, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<GeneratorPlanRecord>> ListGeneratorPlansAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratorPlanRecord>>(Plan == null ? Array.Empty<GeneratorPlanRecord>() : new[] { Plan });
        public Task<GeneratorPlanRecord?> GetGeneratorPlanByIdAsync(string planId, CancellationToken cancellationToken) => Task.FromResult(Plan?.Id == planId ? Plan : null);
        public Task<IReadOnlyList<GeneratorPlanStepRecord>> GetGeneratorPlanStepsAsync(string planId, CancellationToken cancellationToken) => Task.FromResult(_steps);
        public Task<bool> UpdateGeneratorPlanStatusAsync(string planId, string status, string? note, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task SaveGeneratedArtifactAsync(GeneratedArtifactRecord artifact, CancellationToken cancellationToken)
        {
            SavedArtifacts.RemoveAll(existing => existing.Id == artifact.Id);
            SavedArtifacts.Add(artifact);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(SavedArtifacts);
        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsByPlanAsync(string planId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(SavedArtifacts.Where(artifact => artifact.GeneratedBy == planId).ToList());
        public Task<GeneratedArtifactRecord?> GetGeneratedArtifactByIdAsync(string artifactId, CancellationToken cancellationToken) => Task.FromResult(SavedArtifacts.FirstOrDefault(artifact => artifact.Id == artifactId));

        public Task SaveValidationResultsAsync(string artifactId, IReadOnlyList<GeneratedArtifactValidationResultRecord> results, CancellationToken cancellationToken)
        {
            SavedValidationResults.RemoveAll(result => result.ArtifactId == artifactId);
            SavedValidationResults.AddRange(results);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratedArtifactValidationResultRecord>> ListValidationResultsByArtifactAsync(string artifactId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<GeneratedArtifactValidationResultRecord>>(SavedValidationResults.Where(result => result.ArtifactId == artifactId).ToList());
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
