using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GamePackagePatchServiceTests
{
    [Fact]
    public async Task PatchParserAcceptsValidGamePackagePatchV1()
    {
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water"))));
        var current = new InMemoryCurrentGamePackageService(CreateMinimalPackage(), "C:\\temp\\project");
        var service = CreateService(store, current);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.Contains(result.PatchValidationResults, item => item.Code == "patch.schema.valid");
    }

    [Fact]
    public async Task PatchParserRejectsUnknownOperationType()
    {
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson("""{"op":"upsert_unknown","id":"x"}""")));
        var service = CreateService(store);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Contains(result.PatchValidationResults, item => item.Code == "patch.operation.op.unknown");
    }

    [Fact]
    public async Task PatchParserRejectsDeleteOperation()
    {
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson("""{"op":"delete_tile_prototype","id":"tile/grass"}""")));
        var service = CreateService(store);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Contains(result.PatchValidationResults, item => item.Code == "patch.operation.delete.unsupported");
    }

    [Fact]
    public async Task PatchParserRejectsDuplicateOperationTarget()
    {
        var json = ValidPatchJson(TileOperation("tile/water", "Water"), TileOperation("tile/water", "Water 2"));
        var store = new InMemoryArtifactStore(PatchArtifact(json));
        var service = CreateService(store);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Contains(result.PatchValidationResults, item => item.Code == "patch.operation.duplicate_target");
    }

    [Fact]
    public async Task CreatePatchArtifactFromPreviewExtractsExplicitPackageOperations()
    {
        var preview = PreviewArtifact(PreviewJson(TileOperation("tile/water", "Water")));
        var store = new InMemoryArtifactStore(preview);
        var service = CreateService(store);

        var result = await service.CreatePatchArtifactFromPreviewAsync(preview.Id, CancellationToken.None);

        Assert.True(result.Saved, result.Message);
        Assert.NotNull(result.PatchArtifact);
        Assert.Equal(GamePackagePatchArtifactKinds.PatchV1, result.PatchArtifact.Kind);
        Assert.Contains("package_operations", preview.Json);
        Assert.Contains("upsert_tile_prototype", result.PatchArtifact.Json);
    }

    [Fact]
    public async Task CreatePatchArtifactFromPreviewRejectsPreviewWithoutPackageOperations()
    {
        var preview = PreviewArtifact("""
        {
          "kind": "generator_plan_preview",
          "schema_version": 1,
          "plan": { "id": "plan/test" },
          "steps": [{ "config": { "seed": 7 } }]
        }
        """);
        var store = new InMemoryArtifactStore(preview);
        var service = CreateService(store);

        var result = await service.CreatePatchArtifactFromPreviewAsync(preview.Id, CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Null(result.PatchArtifact);
        Assert.Contains(result.ValidationResults, item => item.Code == "patch.preview.package_operations.empty");
    }

    [Fact]
    public async Task CreatePatchArtifactFromPreviewRejectsNonPreviewArtifact()
    {
        var artifact = new GeneratedArtifactRecord("artifact/other", "other_kind", "design-db://other", "{}", "plan/test", "valid", "{}");
        var store = new InMemoryArtifactStore(artifact);
        var service = CreateService(store);

        var result = await service.CreatePatchArtifactFromPreviewAsync(artifact.Id, CancellationToken.None);

        Assert.False(result.Saved);
        Assert.Contains(result.ValidationResults, item => item.Code == "patch.preview.kind.invalid");
    }

    [Fact]
    public async Task DryRunAddTilePrototypeProducesAddDiffAndDoesNotMutateCurrentPackage()
    {
        var package = CreateMinimalPackage();
        var current = new InMemoryCurrentGamePackageService(package, "C:\\temp\\project");
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water")))), current);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.Contains(result.DiffLines, line => line.ChangeKind == "add" && line.Target == "tile/water");
        Assert.DoesNotContain(package.Game.TilePrototypes, tile => tile.Id == "tile/water");
    }

    [Fact]
    public async Task DryRunUpdateTilePrototypeProducesUpdateDiffAndDoesNotMutateCurrentPackage()
    {
        var package = CreateMinimalPackage();
        var current = new InMemoryCurrentGamePackageService(package, "C:\\temp\\project");
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/grass", "Grass Updated")))), current);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.Contains(result.DiffLines, line => line.ChangeKind == "update" && line.Target == "tile/grass");
        Assert.Equal("Grass", package.Game.TilePrototypes[0].Name);
    }

    [Fact]
    public async Task DryRunAddMapAllowsDefaultTileFromEarlierPatchOperation()
    {
        var json = ValidPatchJson(
            TileOperation("tile/water", "Water"),
            MapOperation("map/water", "tile/water", 1, 1));
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(json)));

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.Contains(result.DiffLines, line => line.ChangeKind == "add" && line.Target == "map/water");
    }

    [Fact]
    public async Task DryRunAddRecipeProducesReadableDiffAndDoesNotMutateCurrentPackage()
    {
        var package = CreateMinimalPackage();
        var json = ValidPatchJson(
            ItemOperation("item/red_herb", "Red Herb"),
            ResourceOperation("resource/mana", "Mana"),
            RecipeOperation("recipe/healing_potion"));
        var current = new InMemoryCurrentGamePackageService(package, "C:\\temp\\project");
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(json)), current);

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.CanApply, result.Message);
        Assert.Contains(result.DiffLines, line => line.ChangeKind == "add" && line.Target == "recipe/healing_potion" && line.Message == "Add recipe recipe/healing_potion.");
        Assert.Empty(package.Game.Recipes);
    }

    [Fact]
    public async Task DryRunRejectsMapStartPositionOutsideBounds()
    {
        var json = ValidPatchJson(MapOperation("map/bad", "tile/grass", 9, 0));
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(json)));

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Contains(result.PatchValidationResults, item => item.Code == "patch.map.start_position.out_of_bounds");
    }

    [Fact]
    public async Task DryRunRejectsManifestStartMapIdThatDoesNotExistAfterPatch()
    {
        var json = ValidPatchJson("""{"op":"update_manifest","start_map_id":"map/missing"}""");
        var service = CreateService(new InMemoryArtifactStore(PatchArtifact(json)));

        var result = await service.DryRunPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.CanApply);
        Assert.Contains(result.ValidationIssues, item => item.Code == "patch.manifest.start_map.missing");
    }

    [Fact]
    public async Task ApplyCreatesRollbackSnapshotBeforeSave()
    {
        using var temp = new TempDirectory();
        var service = await CreateFileBackedServiceAsync(temp.Path, new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water")))));

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));
        Assert.Contains(Path.Combine(temp.Path, ".llmgc", "backups"), result.BackupPath);
    }

    [Fact]
    public async Task ApplySavesPackageOnlyWhenDryRunAndPostValidationPass()
    {
        using var temp = new TempDirectory();
        var service = await CreateFileBackedServiceAsync(temp.Path, new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water")))));

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        var reloaded = await new JsonGamePackageRepository().LoadAsync(temp.Path, CancellationToken.None);
        Assert.True(result.Applied, result.Message);
        Assert.Contains(reloaded.Game.TilePrototypes, tile => tile.Id == "tile/water");
    }

    [Fact]
    public async Task ApplyRestoresInMemoryPackageIfPostValidationFails()
    {
        using var temp = new TempDirectory();
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water"))));
        var repository = new JsonGamePackageRepository();
        await repository.SaveAsync(temp.Path, CreateMinimalPackage(), CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(temp.Path, CancellationToken.None);
        var validator = new FailOnSecondValidationValidator();
        var service = new GamePackagePatchService(store, current, validator, new GamePackagePatchOperationValidator());

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.False(result.Applied);
        Assert.DoesNotContain(current.CurrentPackage!.Game.TilePrototypes, tile => tile.Id == "tile/water");
    }

    [Fact]
    public async Task ApplyCreatesAuditArtifactAndResultRows()
    {
        using var temp = new TempDirectory();
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water"))));
        var service = await CreateFileBackedServiceAsync(temp.Path, store);

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        Assert.NotNull(result.AuditArtifact);
        Assert.Contains(store.SavedArtifacts, artifact => artifact.Kind == GamePackagePatchArtifactKinds.ApplyResultV1);
        Assert.Contains(store.SavedValidationResults, row => row.Code == "patch.apply.backup_created");
    }

    [Fact]
    public async Task ApplyDoesNotExecuteLua()
    {
        using var temp = new TempDirectory();
        var markerPath = Path.Combine(temp.Path, "lua-executed.txt");
        await File.WriteAllTextAsync(Path.Combine(temp.Path, "danger.lua"), $"write {markerPath}", CancellationToken.None);
        var service = await CreateFileBackedServiceAsync(temp.Path, new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water")))));

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        Assert.False(File.Exists(markerPath));
    }

    [Fact]
    public async Task ApplyDoesNotCallLlm()
    {
        using var temp = new TempDirectory();
        var store = new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water"))));
        var service = await CreateFileBackedServiceAsync(temp.Path, store);

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        Assert.DoesNotContain(store.SavedArtifacts, artifact => artifact.Kind.Contains("llm", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(store.SavedValidationResults, row => row.Code.Contains("llm", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApplyDoesNotExecuteGeneratorModules()
    {
        using var temp = new TempDirectory();
        var markerPath = Path.Combine(temp.Path, "module-executed.txt");
        var preview = PreviewArtifact(PreviewJson(TileOperation("tile/water", "Water")).Replace("\"module_id\": \"core/test/v1\"", $"\"module_id\": \"{markerPath.Replace("\\", "\\\\")}\""));
        var store = new InMemoryArtifactStore(preview);
        var createService = CreateService(store);
        var createResult = await createService.CreatePatchArtifactFromPreviewAsync(preview.Id, CancellationToken.None);
        var service = await CreateFileBackedServiceAsync(temp.Path, store);

        var result = await service.ApplyPatchArtifactAsync(createResult.PatchArtifact!.Id, CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        Assert.False(File.Exists(markerPath));
    }

    [Fact]
    public async Task ApplyDoesNotWriteFilesOutsidePackageJsonAndBackups()
    {
        using var temp = new TempDirectory();
        var service = await CreateFileBackedServiceAsync(temp.Path, new InMemoryArtifactStore(PatchArtifact(ValidPatchJson(TileOperation("tile/water", "Water")))));

        var result = await service.ApplyPatchArtifactAsync("artifact/patch/test", CancellationToken.None);

        Assert.True(result.Applied, result.Message);
        var files = Directory.GetFiles(temp.Path, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(temp.Path, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        Assert.Contains("package.json", files);
        Assert.Contains(files, path => path.StartsWith(".llmgc/backups/package-", StringComparison.Ordinal));
        Assert.All(files, path => Assert.True(path == "package.json" || path.StartsWith(".llmgc/backups/package-", StringComparison.Ordinal), path));
    }

    private static IGamePackagePatchService CreateService(InMemoryArtifactStore store)
    {
        return CreateService(store, new InMemoryCurrentGamePackageService(CreateMinimalPackage(), "C:\\temp\\project"));
    }

    private static IGamePackagePatchService CreateService(InMemoryArtifactStore store, ICurrentGamePackageService current)
    {
        return new GamePackagePatchService(store, current, new GamePackageValidator(), new GamePackagePatchOperationValidator());
    }

    private static async Task<IGamePackagePatchService> CreateFileBackedServiceAsync(string projectFolder, InMemoryArtifactStore store)
    {
        var repository = new JsonGamePackageRepository();
        await repository.SaveAsync(projectFolder, CreateMinimalPackage(), CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(projectFolder, CancellationToken.None);
        return new GamePackagePatchService(store, current, new GamePackageValidator(), new GamePackagePatchOperationValidator());
    }

    private static GeneratedArtifactRecord PatchArtifact(string json)
    {
        return new GeneratedArtifactRecord("artifact/patch/test", GamePackagePatchArtifactKinds.PatchV1, "design-db://patch", json, "plan/test", "valid", "{}");
    }

    private static GeneratedArtifactRecord PreviewArtifact(string json)
    {
        return new GeneratedArtifactRecord("artifact/generator-plan-preview/plan/test", "generator_plan_preview", "design-db://preview", json, "plan/test", "valid", "{}");
    }

    private static string PreviewJson(params string[] operations)
    {
        return $$"""
        {
          "kind": "generator_plan_preview",
          "schema_version": 1,
          "plan": { "id": "plan/test" },
          "steps": [
            {
              "module_id": "core/test/v1",
              "config": {
                "package_operations": [
                  {{string.Join(",", operations)}}
                ]
              }
            }
          ]
        }
        """;
    }

    private static string ValidPatchJson(params string[] operations)
    {
        return $$"""
        {
          "kind": "game_package_patch_v1",
          "schema_version": 1,
          "source": {
            "plan_id": "plan/test",
            "preview_artifact_id": "artifact/generator-plan-preview/plan/test"
          },
          "operations": [
            {{string.Join(",", operations)}}
          ]
        }
        """;
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

    private static string MapOperation(string id, string defaultTileId, int startX, int startY)
    {
        return $$"""
        {
          "op": "upsert_map",
          "id": "{{id}}",
          "name": "Map",
          "width": 3,
          "height": 3,
          "default_tile_id": "{{defaultTileId}}",
          "start_x": {{startX}},
          "start_y": {{startY}}
        }
        """;
    }

    private static string ItemOperation(string id, string name)
    {
        return $$"""
        {
          "op": "upsert_item_prototype",
          "id": "{{id}}",
          "name": "{{name}}",
          "kind": "material",
          "max_stack": 20
        }
        """;
    }

    private static string ResourceOperation(string id, string name)
    {
        return $$"""
        {
          "op": "upsert_resource",
          "id": "{{id}}",
          "name": "{{name}}",
          "kind": "magic",
          "min_value": 0,
          "max_value": 100
        }
        """;
    }

    private static string RecipeOperation(string id)
    {
        return $$"""
        {
          "op": "upsert_recipe",
          "id": "{{id}}",
          "name": "Healing Potion",
          "category": "alchemy",
          "inputs": [
            { "kind": "item", "id": "item/red_herb", "amount": 2 }
          ],
          "costs": [
            { "kind": "resource", "id": "resource/mana", "amount": 5 }
          ],
          "outputs": [
            { "kind": "item", "id": "item/red_herb", "amount": 1 }
          ]
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

    private sealed class InMemoryArtifactStore : IGeneratedArtifactRepository
    {
        public InMemoryArtifactStore(params GeneratedArtifactRecord[] artifacts)
        {
            SavedArtifacts.AddRange(artifacts);
        }

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

    private sealed class FailOnSecondValidationValidator : IGamePackageValidator
    {
        private int _count;

        public ValidationReport Validate(GamePackageDefinition package) => Validate(package, null);

        public ValidationReport Validate(GamePackageDefinition package, string? projectFolder)
        {
            _count++;
            var report = new GamePackageValidator().Validate(package, projectFolder);
            if (_count >= 2)
            {
                report.Issues.Add(new ValidationIssue
                {
                    Code = "test.post_validation.failure",
                    Severity = ValidationSeverity.Error,
                    Message = "Forced post-validation failure.",
                    TargetId = "test"
                });
            }

            return report;
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
