using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class SmokeTests
{
    [Fact]
    public async Task MinimalGame_Loads_Validates_And_Starts_Runtime()
    {
        var root = FindRepositoryRoot();
        var projectFolder = Path.Combine(root, "samples", "minimal-map-game");
        var repository = new JsonGamePackageRepository();
        var package = await repository.LoadAsync(projectFolder, CancellationToken.None);

        var validator = new GamePackageValidator();
        var report = validator.Validate(package, projectFolder);
        Assert.True(report.IsValid, string.Join(Environment.NewLine, report.Issues.Select(i => i.ToString())));

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(package);
        Assert.True(start.Success);
        Assert.Equal("map/village", start.State.CurrentMapId);
    }

    [Fact]
    public async Task MinimalGame_FirstMove_Changes_Position()
    {
        var root = FindRepositoryRoot();
        var projectFolder = Path.Combine(root, "samples", "minimal-map-game");
        var repository = new JsonGamePackageRepository();
        var package = await repository.LoadAsync(projectFolder, CancellationToken.None);
        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(package);

        var result = runtime.Execute(package, start.State, PlayerCommand.Move(Direction2D.Right));

        Assert.True(result.Success);
        Assert.Equal(2, result.State.PlayerPosition.X);
        Assert.Equal(1, result.State.PlayerPosition.Y);
    }

    [Fact]
    public void ScriptCatalog_MissingPathOrEntryPoint_ProducesValidationErrors()
    {
        var package = CreateMinimalValidPackage();
        package.ScriptCatalog.Scripts.Add(new ScriptDefinition
        {
            Id = "script/generator/broken",
            Kind = LuaScriptKind.Generator,
            Path = "",
            EntryPoints = new List<string>(),
            Capabilities = new List<string> { "return_chunk_draft" }
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "script.path.empty" && issue.TargetId == "script/generator/broken");
        Assert.Contains(report.Issues, issue => issue.Code == "script.entry_points.empty" && issue.TargetId == "script/generator/broken");
    }

    [Fact]
    public void ScriptCatalog_GeneratorMissingOrNonGeneratorScript_ProducesValidationErrors()
    {
        var package = CreateMinimalValidPackage();
        package.ScriptCatalog.Scripts.Add(new ScriptDefinition
        {
            Id = "script/behavior/npc",
            Kind = LuaScriptKind.Behavior,
            Path = "scripts/behaviors/npc.lua",
            EntryPoints = new List<string> { "decide_action" },
            Capabilities = new List<string> { "return_action_draft" }
        });
        package.ScriptCatalog.Generators.Add(new GeneratorDefinition
        {
            Id = "generator/missing",
            ScriptId = "script/generator/missing",
            EntryPoint = "generate_chunk"
        });
        package.ScriptCatalog.Generators.Add(new GeneratorDefinition
        {
            Id = "generator/non_generator",
            ScriptId = "script/behavior/npc",
            EntryPoint = "decide_action"
        });
        package.ScriptCatalog.Scripts.Add(new ScriptDefinition
        {
            Id = "script/generator/chunk",
            Kind = LuaScriptKind.Generator,
            Path = "scripts/generators/chunk.lua",
            EntryPoints = new List<string> { "generate_chunk" },
            Capabilities = new List<string> { "return_chunk_draft" }
        });
        package.ScriptCatalog.Generators.Add(new GeneratorDefinition
        {
            Id = "generator/duplicate",
            Kind = "",
            ScriptId = "script/generator/chunk",
            EntryPoint = " generate_chunk "
        });
        package.ScriptCatalog.Generators.Add(new GeneratorDefinition
        {
            Id = "generator/duplicate",
            ScriptId = "script/generator/chunk",
            EntryPoint = "generate_chunk"
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "generator.script_id.missing" && issue.TargetId == "generator/missing");
        Assert.Contains(report.Issues, issue => issue.Code == "generator.script_kind.invalid" && issue.TargetId == "generator/non_generator");
        Assert.Contains(report.Issues, issue => issue.Code == "duplicate.generator" && issue.TargetId == "generator/duplicate");
        Assert.Contains(report.Issues, issue => issue.Code == "generator.kind.empty" && issue.TargetId == "generator/duplicate");
        Assert.DoesNotContain(report.Issues, issue => issue.Code == "generator.entry_point.missing" && issue.TargetId == "generator/duplicate");
    }

    [Fact]
    public void ScriptCatalog_PathTraversal_ProducesValidationError()
    {
        var package = CreateMinimalValidPackage();
        package.ScriptCatalog.Scripts.Add(new ScriptDefinition
        {
            Id = "script/generator/escape",
            Kind = LuaScriptKind.Generator,
            Path = "../escape.lua",
            EntryPoints = new List<string> { "generate_chunk" },
            Capabilities = new List<string> { "return_chunk_draft" }
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "script.path.traversal" && issue.TargetId == "script/generator/escape");
    }

    [Fact]
    public void AssetCatalog_BrokenFallbackContractAndPath_ProducesValidationIssues()
    {
        var package = CreateMinimalValidPackage();
        package.AssetCatalog.Assets.Add(new AssetDefinition
        {
            Id = "asset/broken",
            Type = "tile",
            Path = "../escape.png",
            FallbackAssetId = "asset/missing",
            ContractId = "contract/missing"
        });

        var report = new GamePackageValidator().Validate(package, FindRepositoryRoot());

        Assert.Contains(report.Issues, issue => issue.Code == "asset.path.traversal" && issue.TargetId == "asset/broken");
        Assert.Contains(report.Issues, issue => issue.Code == "asset.fallback.missing" && issue.TargetId == "asset/broken");
        Assert.Contains(report.Issues, issue => issue.Code == "asset.contract.missing" && issue.TargetId == "asset/broken");
    }

    [Fact]
    public void GameDefinitions_MissingTileAndEntityAssetReferences_ProduceValidationErrors()
    {
        var package = CreateMinimalValidPackage();
        package.Game.TilePrototypes[0].AssetId = "asset/missing/tile";
        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
        {
            Id = "prototype/npc/missing_asset",
            Name = "Missing Asset NPC",
            AssetId = "asset/missing/entity"
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "tile.asset.missing" && issue.TargetId == "tile/grass");
        Assert.Contains(report.Issues, issue => issue.Code == "entity.asset.missing" && issue.TargetId == "prototype/npc/missing_asset");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Не найден корень репозитория.");
        }

        return directory.FullName;
    }

    private static GamePackageDefinition CreateMinimalValidPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/test",
                StartMapId = "map/test"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition
                    {
                        Id = "tile/grass",
                        Name = "Grass"
                    }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/test",
                        Name = "Test Map",
                        Width = 1,
                        Height = 1,
                        DefaultTileId = "tile/grass"
                    }
                }
            }
        };
    }
}
