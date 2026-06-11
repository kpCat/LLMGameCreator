using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Projects;

public sealed class NewGamePackageFactory
{
    public GamePackageDefinition Create(CreateGameProjectRequest request)
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = request.PackageId.Trim(),
                Title = request.Title.Trim(),
                Version = request.Version.Trim(),
                FormatVersion = "0.1",
                StartMapId = "map/start",
                Description = "Minimal game package created by LLMGameCreator."
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
                        Name = "Start Map",
                        Width = 5,
                        Height = 5,
                        DefaultTileId = "tile/grass",
                        StartPosition = new Position2D
                        {
                            X = 2,
                            Y = 2
                        }
                    }
                }
            }
        };
    }
}
