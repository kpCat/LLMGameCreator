using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GamePackagePatchOperationValidatorTests
{
    private readonly GamePackagePatchOperationValidator _validator = new();

    [Fact]
    public void PackageOperationsValidatorAcceptsValidOperations()
    {
        var result = _validator.ValidatePackageOperationsJson($$"""
        [
          {{TileOperation("tile/stone", "Stone")}},
          {{MapOperation("map/start")}},
          { "op": "update_manifest", "title": "Stone Game", "start_map_id": "map/start" }
        ]
        """, "test");

        Assert.DoesNotContain(result.ValidationResults, item => item.Severity == "error");
        Assert.Equal(3, result.Operations.Count);
    }

    [Fact]
    public void PackageOperationsValidatorRejectsUnknownOperation()
    {
        var result = _validator.ValidatePackageOperationsJson("""[{ "op": "merge_anything", "path": "/game" }]""", "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.op.unknown");
    }

    [Fact]
    public void PackageOperationsValidatorRejectsDeleteOperation()
    {
        var result = _validator.ValidatePackageOperationsJson("""[{ "op": "delete_map", "id": "map/start" }]""", "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.delete.unsupported");
    }

    [Fact]
    public void PackageOperationsValidatorRejectsDuplicateTarget()
    {
        var result = _validator.ValidatePackageOperationsJson($$"""
        [
          {{TileOperation("tile/stone", "Stone")}},
          {{TileOperation("tile/stone", "Stone 2")}}
        ]
        """, "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.duplicate_target");
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

    private static string MapOperation(string id)
    {
        return $$"""
        {
          "op": "upsert_map",
          "id": "{{id}}",
          "name": "Start",
          "width": 8,
          "height": 8,
          "default_tile_id": "tile/stone",
          "start_x": 1,
          "start_y": 1
        }
        """;
    }
}
