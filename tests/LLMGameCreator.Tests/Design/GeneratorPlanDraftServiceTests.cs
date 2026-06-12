using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftServiceTests
{
    [Fact]
    public async Task DraftServiceSavesValidatedDraftWithoutRealLlm()
    {
        var store = new InMemoryDesignStore(new[]
        {
            Module("core/base/v1"),
            Module("world/map/v1", dependenciesJson: "[\"core/base/v1\"]")
        });
        var llm = new FakeLlmChatClient("""
        {
          "title": "World plan",
          "goal": "Create a world slice",
          "steps": [
            { "order": 1, "module_id": "core/base/v1", "config": {}, "depends_on": [] },
            { "order": 2, "module_id": "world/map/v1", "config": { "size": "small" }, "depends_on": ["core/base/v1"] }
          ]
        }
        """);
        var service = CreateService(store, llm);

        var result = await service.CreateDraftPlanAsync(new GeneratorPlanDraftRequest("World", "Create a world slice", "Small village", RuntimeTarget: "debug"), CancellationToken.None);

        Assert.True(result.Saved, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.NotNull(store.SavedPlan);
        Assert.Equal("draft", store.SavedPlan.Status);
        Assert.Equal(2, store.SavedSteps.Count);
        Assert.Equal(1, llm.CallCount);
    }

    [Fact]
    public async Task DraftServiceDoesNotSaveInvalidDraft()
    {
        var store = new InMemoryDesignStore(new[] { Module("core/base/v1") });
        var service = CreateService(store, new FakeLlmChatClient("""
        { "title": "Bad", "goal": "Bad", "steps": [{ "order": 1, "module_id": "missing/v1", "config": {}, "depends_on": [] }] }
        """));

        var result = await service.CreateDraftPlanAsync(new GeneratorPlanDraftRequest("Bad", "Bad", "Brief"), CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(store.SavedPlan);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "plan.module_id.unknown");
    }

    [Fact]
    public async Task DraftServiceDoesNotExecuteLuaOrTouchPackageFilesAndContextStaysCompact()
    {
        using var temp = new TempDirectory();
        var packagePath = Path.Combine(temp.Path, "package.json");
        await File.WriteAllTextAsync(packagePath, "{\"unchanged\":true}", CancellationToken.None);
        var markerPath = Path.Combine(temp.Path, "executed.txt");
        await File.WriteAllTextAsync(Path.Combine(temp.Path, "danger.lua"), $"os.execute('echo executed > {markerPath}')", CancellationToken.None);
        var store = new InMemoryDesignStore(new[] { Module("core/danger/v1", path: Path.Combine(temp.Path, "danger.lua")) });
        var llm = new FakeLlmChatClient("""
        { "title": "Safe", "goal": "Safe", "steps": [{ "order": 1, "module_id": "core/danger/v1", "config": {}, "depends_on": [] }] }
        """);
        var service = CreateService(store, llm);

        var result = await service.CreateDraftPlanAsync(new GeneratorPlanDraftRequest("Safe", "Safe", "Brief"), CancellationToken.None);

        Assert.True(result.Saved, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.False(File.Exists(markerPath));
        Assert.Equal("{\"unchanged\":true}", await File.ReadAllTextAsync(packagePath, CancellationToken.None));
        Assert.DoesNotContain("os.execute", llm.LastRequest.UserPrompt);
        Assert.Contains("core/danger/v1", llm.LastRequest.UserPrompt);
    }

    [Fact]
    public async Task DraftServiceIncludesPackageOperationsContractInDataPatchMode()
    {
        var store = new InMemoryDesignStore(new[] { Module("core/base/v1") });
        var llm = new FakeLlmChatClient("""
        { "title": "Patch", "goal": "Patch", "steps": [{ "order": 1, "module_id": "core/base/v1", "config": {}, "depends_on": [] }] }
        """);
        var service = CreateService(store, llm);

        var result = await service.CreateDraftPlanAsync(new GeneratorPlanDraftRequest("Patch", "Patch", "Brief", OutputMode: "data_patch_plan"), CancellationToken.None);

        Assert.True(result.Saved, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.Contains("config.package_operations", llm.LastRequest.SystemPrompt);
        Assert.Contains("game_package_patch_v1", llm.LastRequest.SystemPrompt);
        Assert.Contains("upsert_tile_prototype", llm.LastRequest.UserPrompt);
    }

    [Fact]
    public async Task DraftServiceKeepsGenericPlanModeWithoutPatchContract()
    {
        var store = new InMemoryDesignStore(new[] { Module("core/base/v1") });
        var llm = new FakeLlmChatClient("""
        { "title": "Generic", "goal": "Generic", "steps": [{ "order": 1, "module_id": "core/base/v1", "config": {}, "depends_on": [] }] }
        """);
        var service = CreateService(store, llm);

        var result = await service.CreateDraftPlanAsync(new GeneratorPlanDraftRequest("Generic", "Generic", "Brief"), CancellationToken.None);

        Assert.True(result.Saved, string.Join(Environment.NewLine, result.ValidationIssues.Select(issue => issue.Message)));
        Assert.DoesNotContain("config.package_operations", llm.LastRequest.SystemPrompt);
        Assert.DoesNotContain("Patch-capable plan contract", llm.LastRequest.UserPrompt);
    }

    private static GeneratorPlanDraftService CreateService(InMemoryDesignStore store, FakeLlmChatClient llm)
    {
        return new GeneratorPlanDraftService(store, store, store, new InMemorySettingsRepository(), llm, new GeneratorPlanValidator());
    }

    private static GeneratorModuleRecord Module(string id, string dependenciesJson = "[]", string path = "lua/core/base.lua")
    {
        return new GeneratorModuleRecord(id, "001", path, "core", "[\"core.base\"]", dependenciesJson, "[\"debug\"]", "[]", "[]", "manifests/test.manifest.json", "{}", DateTimeOffset.UtcNow);
    }

    private sealed class FakeLlmChatClient : ILlmChatClient
    {
        private readonly string _response;

        public FakeLlmChatClient(string response)
        {
            _response = response;
        }

        public int CallCount { get; private set; }
        public LlmChatRequest LastRequest { get; private set; } = new();

        public Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(new LlmChatResponse { Content = _response, Endpoint = profile.Endpoint, Model = profile.Model });
        }
    }

    private sealed class InMemorySettingsRepository : IAppSettingsRepository
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new AppSettings
            {
                DefaultLlmProfileId = "test",
                LlmProfiles = new List<LlmEndpointSettings>
                {
                    new LlmEndpointSettings { Id = "test", Title = "Test", Endpoint = "http://127.0.0.1:1234/v1", Model = "fake" }
                }
            });
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryDesignStore : IGeneratorLibraryRegistry, IDesignKnowledgeRepository, IGeneratorPlanRepository
    {
        private readonly IReadOnlyList<GeneratorModuleRecord> _modules;

        public InMemoryDesignStore(IReadOnlyList<GeneratorModuleRecord> modules)
        {
            _modules = modules;
        }

        public GeneratorPlanRecord? SavedPlan { get; private set; }
        public IReadOnlyList<GeneratorPlanStepRecord> SavedSteps { get; private set; } = Array.Empty<GeneratorPlanStepRecord>();

        public Task SaveImportedLibraryAsync(GeneratorLibraryImportData data, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<CapabilityModuleRecord>> ListCapabilitiesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CapabilityModuleRecord>>(Array.Empty<CapabilityModuleRecord>());
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesAsync(CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<GeneratorModuleRecord?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken) => Task.FromResult(_modules.FirstOrDefault(module => module.Id == moduleId));
        public Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesByCapabilityAsync(string capabilityId, CancellationToken cancellationToken) => Task.FromResult(_modules);
        public Task<IReadOnlyList<GeneratorLibraryImportIssue>> ListImportIssuesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratorLibraryImportIssue>>(Array.Empty<GeneratorLibraryImportIssue>());
        public Task UpsertKnowledgeItemAsync(DesignKnowledgeItem item, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<DesignKnowledgeItem>> ListKnowledgeItemsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DesignKnowledgeItem>>(Array.Empty<DesignKnowledgeItem>());
        public Task UpsertDecisionAsync(DesignDecision decision, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<DesignDecision>> ListDecisionsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DesignDecision>>(Array.Empty<DesignDecision>());
        public Task UpsertConstraintAsync(DesignConstraint constraint, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<DesignConstraint>> ListConstraintsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DesignConstraint>>(Array.Empty<DesignConstraint>());

        public Task SaveGeneratorPlanAsync(GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps, PromptContextPackRecord? contextPack, CancellationToken cancellationToken)
        {
            SavedPlan = plan;
            SavedSteps = steps;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratorPlanRecord>> ListGeneratorPlansAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<GeneratorPlanRecord>>(SavedPlan == null ? Array.Empty<GeneratorPlanRecord>() : new[] { SavedPlan });
        }

        public Task<GeneratorPlanRecord?> GetGeneratorPlanByIdAsync(string planId, CancellationToken cancellationToken)
        {
            return Task.FromResult(SavedPlan?.Id == planId ? SavedPlan : null);
        }

        public Task<IReadOnlyList<GeneratorPlanStepRecord>> GetGeneratorPlanStepsAsync(string planId, CancellationToken cancellationToken)
        {
            return Task.FromResult(SavedSteps);
        }

        public Task<bool> UpdateGeneratorPlanStatusAsync(string planId, string status, string? note, CancellationToken cancellationToken)
        {
            if (SavedPlan == null || SavedPlan.Id != planId)
            {
                return Task.FromResult(false);
            }

            SavedPlan = SavedPlan with { Status = status, UpdatedUtc = DateTimeOffset.UtcNow };
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
