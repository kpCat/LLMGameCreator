using LLMGameCreator.Application.Design;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.Scripting;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class PrototypeLuaPatchArtifactServiceTests
{
    [Fact]
    public async Task CreatesGamePackagePatchArtifactFromValidPrototypeLua()
    {
        var store = new InMemoryArtifactStore();
        var service = CreateService(store, new FakePatchService());

        var result = await service.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
        {
            ScriptId = "script/prototype/test",
            Source = """
            data:extend({
              { type = "tile", id = "tile/water", name = "Water", walkable = false, movement_cost = 2.0 }
            })
            """
        }, CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        Assert.NotNull(result.PatchArtifact);
        Assert.Equal(GamePackagePatchArtifactKinds.PatchV1, result.PatchArtifact.Kind);
        Assert.Contains("upsert_tile_prototype", result.PatchArtifact.Json);
        Assert.Contains(store.SavedArtifacts, artifact => artifact.Id == result.PatchArtifact.Id);
    }

    [Fact]
    public async Task DoesNotCreateArtifactWhenExecutionHasErrors()
    {
        var store = new InMemoryArtifactStore();
        var service = CreateService(store, new FakePatchService());

        var result = await service.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
        {
            ScriptId = "script/prototype/bad",
            Source = "io.open('danger')"
        }, CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(result.PatchArtifact);
        Assert.Empty(store.SavedArtifacts);
        Assert.Contains(result.ValidationResults, item => item.Code == "lua.prototype.forbidden_api");
    }

    [Fact]
    public async Task RejectsInvalidDeclarationIdThroughPatchValidator()
    {
        var store = new InMemoryArtifactStore();
        var service = CreateService(store, new FakePatchService());

        var result = await service.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
        {
            ScriptId = "script/prototype/bad-id",
            Source = """
            data:extend({
              { type = "tile", id = "BadId", name = "Bad", walkable = true, movement_cost = 1.0 }
            })
            """
        }, CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.id.invalid");
    }

    [Fact]
    public async Task RejectsInvalidMapShapeThroughPatchValidator()
    {
        var store = new InMemoryArtifactStore();
        var service = CreateService(store, new FakePatchService());

        var result = await service.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
        {
            ScriptId = "script/prototype/bad-map",
            Source = """
            data:extend({
              { type = "map", id = "map/bad", name = "Bad", width = 2, height = 2, default_tile_id = "tile/grass", start_x = 9, start_y = 0 }
            })
            """
        }, CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Contains(result.ValidationResults, item => item.Code == "patch.map.start_position.out_of_bounds");
    }

    [Fact]
    public async Task OptionalDryRunUsesExistingPatchServiceWithoutApplying()
    {
        var store = new InMemoryArtifactStore();
        var patchService = new FakePatchService();
        var service = CreateService(store, patchService);

        var result = await service.CreatePatchArtifactFromPrototypeLuaAsync(new PrototypeLuaPatchArtifactRequest
        {
            ScriptId = "script/prototype/test",
            DryRun = true,
            Source = """
            data:extend({
              { type = "manifest_update", title = "My Game", start_map_id = "map/start" }
            })
            """
        }, CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        Assert.NotNull(result.DryRunResult);
        Assert.Equal(1, patchService.DryRunCallCount);
        Assert.Equal(0, patchService.ApplyCallCount);
    }

    private static PrototypeLuaPatchArtifactService CreateService(InMemoryArtifactStore store, IGamePackagePatchService patchService)
    {
        return new PrototypeLuaPatchArtifactService(
            new PrototypeLuaExecutor(new PrototypeLuaStaticAnalyzer()),
            new PrototypeLuaDeclarationMapper(),
            new GamePackagePatchOperationValidator(),
            store,
            patchService);
    }

    private sealed class InMemoryArtifactStore : IGeneratedArtifactRepository
    {
        public List<GeneratedArtifactRecord> SavedArtifacts { get; } = new();
        public List<GeneratedArtifactValidationResultRecord> SavedValidationResults { get; } = new();

        public Task SaveGeneratedArtifactAsync(GeneratedArtifactRecord artifact, CancellationToken cancellationToken)
        {
            SavedArtifacts.RemoveAll(existing => existing.Id == artifact.Id);
            SavedArtifacts.Add(artifact);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(SavedArtifacts);
        }

        public Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsByPlanAsync(string planId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<GeneratedArtifactRecord>>(SavedArtifacts.Where(artifact => artifact.GeneratedBy == planId).ToList());
        }

        public Task<GeneratedArtifactRecord?> GetGeneratedArtifactByIdAsync(string artifactId, CancellationToken cancellationToken)
        {
            return Task.FromResult(SavedArtifacts.FirstOrDefault(artifact => artifact.Id == artifactId));
        }

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

    private sealed class FakePatchService : IGamePackagePatchService
    {
        public int DryRunCallCount { get; private set; }
        public int ApplyCallCount { get; private set; }

        public Task<GamePackagePatchCreateResult> CreatePatchArtifactFromPreviewAsync(string previewArtifactId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<GamePackagePatchDryRunResult> DryRunPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken)
        {
            DryRunCallCount++;
            return Task.FromResult(new GamePackagePatchDryRunResult(
                null,
                true,
                Array.Empty<GamePackagePatchDiffLine>(),
                Array.Empty<ValidationIssue>(),
                Array.Empty<GeneratedArtifactValidationResultRecord>(),
                "Dry-run can be applied."));
        }

        public Task<GamePackagePatchApplyResult> ApplyPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken)
        {
            ApplyCallCount++;
            throw new NotSupportedException();
        }
    }
}

