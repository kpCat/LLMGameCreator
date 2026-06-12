using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanPipelineServiceTests
{
    [Fact]
    public async Task PreparePatchPipelineCreatesPreviewPatchAndDryRunForApprovedPlan()
    {
        var package = CreateMinimalPackage();
        var store = Store("approved", StepWithOperations(TileOperation("tile/stone", "Stone")));
        var service = CreatePipeline(store, new InMemoryCurrentGamePackageService(package, "C:\\temp\\project"));

        var result = await service.PreparePatchPipelineAsync(store.Plan.Id, CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.NotNull(result.PreviewArtifact);
        Assert.NotNull(result.PatchArtifact);
        Assert.NotNull(result.DryRunResult);
        Assert.Contains(result.DryRunResult.DiffLines, line => line.Target == "tile/stone");
        Assert.DoesNotContain(package.Game.TilePrototypes, tile => tile.Id == "tile/stone");
    }

    [Fact]
    public async Task PreparePatchPipelineRejectsNonApprovedPlan()
    {
        var store = Store("draft", StepWithOperations(TileOperation("tile/stone", "Stone")));
        var service = CreatePipeline(store, new InMemoryCurrentGamePackageService(CreateMinimalPackage(), "C:\\temp\\project"));

        var result = await service.PreparePatchPipelineAsync(store.Plan.Id, CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Null(result.PreviewArtifact);
        Assert.Contains(result.ValidationIssues, issue => issue.Code == "pipeline.plan.not_approved");
    }

    [Fact]
    public async Task PreparePatchPipelineReportsNoPackageOperations()
    {
        var store = Store("approved", Step("{}"));
        var service = CreatePipeline(store, new InMemoryCurrentGamePackageService(CreateMinimalPackage(), "C:\\temp\\project"));

        var result = await service.PreparePatchPipelineAsync(store.Plan.Id, CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.NotNull(result.PreviewArtifact);
        Assert.Null(result.PatchArtifact);
        Assert.Contains("no data-only package operations", result.Message);
    }

    [Fact]
    public async Task ExplicitApplyUsesPatchServiceRollbackPath()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();
        await repository.SaveAsync(temp.Path, CreateMinimalPackage(), CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(temp.Path, CancellationToken.None);
        var store = Store("approved", StepWithOperations(TileOperation("tile/stone", "Stone")));
        var service = CreatePipeline(store, current);
        var prepared = await service.PreparePatchPipelineAsync(store.Plan.Id, CancellationToken.None);

        var applied = await service.ApplyPreparedPatchAsync(prepared.PatchArtifact!.Id, CancellationToken.None);

        Assert.True(applied.Applied, applied.Message);
        Assert.NotNull(applied.BackupPath);
        Assert.True(File.Exists(applied.BackupPath));
        Assert.Contains(Path.Combine(temp.Path, ".llmgc", "backups"), applied.BackupPath);
    }

    private static IGeneratorPlanPipelineService CreatePipeline(InMemoryDesignStore store, ICurrentGamePackageService current)
    {
        var patchValidator = new GamePackagePatchOperationValidator();
        var planValidator = new GeneratorPlanValidator(patchValidator);
        var review = new GeneratorPlanReviewService(store, store, planValidator);
        var preview = new GeneratorPlanPreviewService(store, store, review, store);
        var patch = new GamePackagePatchService(store, current, new GamePackageValidator(), patchValidator);
        return new GeneratorPlanPipelineService(store, review, preview, patch);
    }

    private static InMemoryDesignStore Store(string status, GeneratorPlanStepRecord step)
    {
        return new InMemoryDesignStore(
            new[] { Module("core/base/v1") },
            new GeneratorPlanRecord("plan/test", "Plan", "Goal", status, "{}", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new[] { step });
    }

    private static GeneratorPlanStepRecord StepWithOperations(params string[] operations)
    {
        return Step($$"""
        {
          "package_operations": [
            {{string.Join(",", operations)}}
          ]
        }
        """);
    }

    private static GeneratorPlanStepRecord Step(string configJson)
    {
        return new GeneratorPlanStepRecord("step/1", "plan/test", 1, "core/base/v1", configJson, "[]", "pending");
    }

    private static GeneratorModuleRecord Module(string id)
    {
        return new GeneratorModuleRecord(id, "001", "lua/core/base.lua", "core", "[\"core.base\"]", "[]", "[]", "[]", "[]", "manifests/test.manifest.json", "{}", DateTimeOffset.UtcNow);
    }

    private static string TileOperation(string id, string name)
    {
        return $$"""
        {
          "op": "upsert_tile_prototype",
          "id": "{{id}}",
          "name": "{{name}}",
          "walkable": true,
          "movement_cost": 1.0
        }
        """;
    }

    private static GamePackageDefinition CreateMinimalPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/test",
                Title = "Test Game",
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = "map/start"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition
                    {
                        Id = "tile/grass",
                        Name = "Grass",
                        Walkable = true,
                        MovementCost = 1.0
                    }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/start",
                        Name = "Start",
                        Width = 5,
                        Height = 5,
                        DefaultTileId = "tile/grass",
                        StartPosition = new Position2D(2, 2)
                    }
                }
            }
        };
    }

    private sealed class InMemoryDesignStore : IGeneratorLibraryRegistry, IGeneratorPlanRepository, IGeneratedArtifactRepository
    {
        private readonly IReadOnlyList<GeneratorModuleRecord> _modules;
        private readonly IReadOnlyList<GeneratorPlanStepRecord> _steps;
        private readonly List<GeneratedArtifactRecord> _artifacts = new();
        private readonly List<GeneratedArtifactValidationResultRecord> _validationResults = new();

        public InMemoryDesignStore(IReadOnlyList<GeneratorModuleRecord> modules, GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps)
        {
            _modules = modules;
            Plan = plan;
            _steps = steps;
        }

        public GeneratorPlanRecord Plan { get; }

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
        public Task<bool> UpdateGeneratorPlanStatusAsync(string planId, string status, string? note, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task SaveGeneratedArtifactAsync(GeneratedArtifactRecord artifact, CancellationToken cancellationToken)
        {
            _artifacts.RemoveAll(existing => existing.Id == artifact.Id);
            _artifacts.Add(artifact);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(_artifacts);
        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsByPlanAsync(string planId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(_artifacts.Where(artifact => artifact.GeneratedBy == planId).ToList());
        public Task<GeneratedArtifactRecord?> GetGeneratedArtifactByIdAsync(string artifactId, CancellationToken cancellationToken) => Task.FromResult(_artifacts.FirstOrDefault(artifact => artifact.Id == artifactId));

        public Task SaveValidationResultsAsync(string artifactId, IReadOnlyList<GeneratedArtifactValidationResultRecord> results, CancellationToken cancellationToken)
        {
            _validationResults.RemoveAll(result => result.ArtifactId == artifactId);
            _validationResults.AddRange(results);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratedArtifactValidationResultRecord>> ListValidationResultsByArtifactAsync(string artifactId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<GeneratedArtifactValidationResultRecord>>(_validationResults.Where(result => result.ArtifactId == artifactId).ToList());
        }
    }

    private sealed class InMemoryCurrentGamePackageService : ICurrentGamePackageService
    {
        public InMemoryCurrentGamePackageService(GamePackageDefinition package, string? currentFolder)
        {
            CurrentPackage = package;
            CurrentFolder = currentFolder;
        }

        public string? CurrentFolder { get; private set; }
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;

        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken)
        {
            CurrentFolder = projectFolder;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task SaveAsync(CancellationToken cancellationToken)
        {
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public void ReplaceCurrent(GamePackageDefinition package)
        {
            CurrentPackage = package;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
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
