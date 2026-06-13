using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssembler
{
    public GeneratorPlanGamePackageAssemblerResult Assemble(GeneratorPlanApprovedArtifactSet artifactSet)
    {
        ArgumentNullException.ThrowIfNull(artifactSet);

        var package = CreateBaselinePackage();
        var diagnostics = new List<GeneratorPlanGamePackageAssemblyDiagnostic>();
        var mappings = new List<GeneratorPlanGamePackageAssemblyMapping>();

        EnsureBaseline(package);

        foreach (var artifact in artifactSet.ApprovedArtifacts.OrderBy(artifact => artifact.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(artifact.ArtifactKind))
            {
                diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    GeneratorPlanGamePackageAssemblyDiagnosticCodes.ApprovedArtifactMissingKind,
                    "Approved artifact kind should be set.",
                    artifact.ArtifactId,
                    artifact.ArtifactKind,
                    "artifact_kind"));
            }

            using var document = TryParseArtifactContent(artifact, diagnostics);
            if (document == null)
            {
                mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Unmapped, "invalid_json"));
                continue;
            }

            switch (artifact.ArtifactKind)
            {
                case "game_profile_v1":
                    MapGameProfile(package, document.RootElement);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "manifest"));
                    break;
                case "scene_pack_v1":
                    MapScenePack(package, document.RootElement);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.maps"));
                    break;
                case "entity_pack_v1":
                    MapEntityPack(package, document.RootElement);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.entity_prototypes"));
                    break;
                case "quest_pack_v1":
                    MapQuestPack(package, document.RootElement);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.quests"));
                    break;
                case "mechanics_pack_v1":
                    MapMechanicsPack(package, document.RootElement);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.abilities"));
                    break;
                case "semantic_pack_v1":
                    diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Warning,
                        GeneratorPlanGamePackageAssemblyDiagnosticCodes.UnmappedArtifactKind,
                        "Semantic artifacts are acknowledged but do not have a GamePackage field in v1.",
                        artifact.ArtifactId,
                        artifact.ArtifactKind,
                        "semantic_pack_v1"));
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Unmapped, "no_game_package_field"));
                    break;
                default:
                    diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Warning,
                        GeneratorPlanGamePackageAssemblyDiagnosticCodes.UnmappedArtifactKind,
                        $"Approved artifact kind is not mapped by GamePackage Assembly v1: {artifact.ArtifactKind}",
                        artifact.ArtifactId,
                        artifact.ArtifactKind,
                        "artifact_kind"));
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Unmapped, "unknown_kind"));
                    break;
            }
        }

        EnsureBaseline(package);
        return new GeneratorPlanGamePackageAssemblerResult
        {
            Package = package,
            Diagnostics = diagnostics,
            Mappings = mappings
        };
    }

    private static GamePackageDefinition CreateBaselinePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/generated-draft",
                Title = "Generated Draft Game",
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = "map/start",
                Description = "Generated draft GamePackage assembled from approved artifacts."
            },
            Game = new GameDefinition()
        };
    }

    private static void EnsureBaseline(GamePackageDefinition package)
    {
        if (package.Game.TilePrototypes.All(tile => !string.Equals(tile.Id, "tile/grass", StringComparison.OrdinalIgnoreCase)))
        {
            package.Game.TilePrototypes.Add(new TilePrototypeDefinition
            {
                Id = "tile/grass",
                Name = "Grass",
                Walkable = true,
                MovementCost = 1.0
            });
        }

        if (package.Game.EntityPrototypes.All(entity => !string.Equals(entity.Id, "entity/player", StringComparison.OrdinalIgnoreCase)))
        {
            package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
            {
                Id = "entity/player",
                Name = "Player",
                Components =
                [
                    new ComponentDefinition { Type = "player" }
                ]
            });
        }

        var startMap = package.Game.Maps.FirstOrDefault(map => string.Equals(map.Id, "map/start", StringComparison.OrdinalIgnoreCase));
        if (startMap == null)
        {
            startMap = new MapDefinition
            {
                Id = "map/start",
                Name = "Start Map",
                Width = 8,
                Height = 8,
                DefaultTileId = "tile/grass",
                StartPosition = new Position2D(1, 1)
            };
            package.Game.Maps.Insert(0, startMap);
        }

        if (string.IsNullOrWhiteSpace(startMap.DefaultTileId))
        {
            startMap.DefaultTileId = "tile/grass";
        }

        if (startMap.Width <= 0)
        {
            startMap.Width = 8;
        }

        if (startMap.Height <= 0)
        {
            startMap.Height = 8;
        }

        if (startMap.StartPosition.X < 0 || startMap.StartPosition.X >= startMap.Width)
        {
            startMap.StartPosition.X = Math.Min(1, startMap.Width - 1);
        }

        if (startMap.StartPosition.Y < 0 || startMap.StartPosition.Y >= startMap.Height)
        {
            startMap.StartPosition.Y = Math.Min(1, startMap.Height - 1);
        }

        if (startMap.Entities.All(entity => !string.Equals(entity.Id, "entity/player/start", StringComparison.OrdinalIgnoreCase)))
        {
            startMap.Entities.Add(new EntityInstanceDefinition
            {
                Id = "entity/player/start",
                PrototypeId = "entity/player",
                Position = new Position2D(startMap.StartPosition.X, startMap.StartPosition.Y)
            });
        }
    }

    private static JsonDocument? TryParseArtifactContent(
        GeneratorPlanApprovedArtifact artifact,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(artifact.ContentJson) ? string.Empty : artifact.ContentJson);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.ApprovedArtifactInvalidJson,
                $"Approved artifact content_json must be valid JSON: {exception.Message}",
                artifact.ArtifactId,
                artifact.ArtifactKind,
                "content_json"));
            return null;
        }
    }

    private static void MapGameProfile(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "game", out var game) || game.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var title = GetString(game, "title");
        if (!string.IsNullOrWhiteSpace(title))
        {
            package.Manifest.Title = title.Trim();
            package.Manifest.PackageId = "game/" + NormalizeIdSegment(title);
        }

        var genre = GetString(game, "genre");
        if (!string.IsNullOrWhiteSpace(genre))
        {
            package.Manifest.Description = genre.Trim();
        }
    }

    private static void MapScenePack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "scenes", out var scenes) || scenes.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var scene in scenes.EnumerateArray())
        {
            var title = GetString(scene, "title");
            var sourceId = GetString(scene, "id");
            if (index == 0)
            {
                var start = package.Game.Maps.FirstOrDefault(map => map.Id == "map/start");
                if (start != null && !string.IsNullOrWhiteSpace(title))
                {
                    start.Name = title.Trim();
                }
            }
            else
            {
                var id = "map/draft/" + NormalizeIdSegment(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
                if (package.Game.Maps.All(map => !string.Equals(map.Id, id, StringComparison.OrdinalIgnoreCase)))
                {
                    package.Game.Maps.Add(new MapDefinition
                    {
                        Id = id,
                        Name = string.IsNullOrWhiteSpace(title) ? $"Draft Scene {index + 1}" : title.Trim(),
                        Width = 8,
                        Height = 8,
                        DefaultTileId = "tile/grass",
                        StartPosition = new Position2D(1, 1)
                    });
                }
            }

            index++;
        }
    }

    private static void MapEntityPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "entities", out var entities) || entities.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var entity in entities.EnumerateArray())
        {
            var sourceId = GetString(entity, "id");
            var kind = GetString(entity, "kind");
            var title = GetString(entity, "title");
            var id = NormalizeEntityPrototypeId(sourceId, kind, index);
            if (package.Game.EntityPrototypes.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                index++;
                continue;
            }

            package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
            {
                Id = id,
                Name = !string.IsNullOrWhiteSpace(title)
                    ? title.Trim()
                    : !string.IsNullOrWhiteSpace(kind)
                        ? ToTitle(kind)
                        : $"Entity {index + 1}",
                Components =
                [
                    new ComponentDefinition
                    {
                        Type = string.IsNullOrWhiteSpace(kind) ? "entity" : NormalizeIdSegment(kind)
                    }
                ]
            });
            index++;
        }
    }

    private static void MapQuestPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "quests", out var quests) || quests.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var quest in quests.EnumerateArray())
        {
            var sourceId = GetString(quest, "id");
            var title = GetString(quest, "title");
            var id = "quest/" + NormalizeIdSegment(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
            if (package.Game.Quests.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                index++;
                continue;
            }

            package.Game.Quests.Add(new QuestDefinition
            {
                Id = id,
                Title = string.IsNullOrWhiteSpace(title) ? $"Draft Quest {index + 1}" : title.Trim(),
                Description = GetString(quest, "description"),
                Kind = "quest",
                AutoStart = index == 0,
                Objectives = BuildQuestObjectives(quest),
                Metadata =
                {
                    ["source"] = "game_package_assembly"
                }
            });
            index++;
        }
    }

    private static List<QuestObjectiveDefinition> BuildQuestObjectives(JsonElement quest)
    {
        if (!TryGetProperty(quest, "objectives", out var objectives) || objectives.ValueKind != JsonValueKind.Array)
        {
            return
            [
                new QuestObjectiveDefinition
                {
                    Id = "objective/first",
                    Kind = "custom_counter",
                    RequiredAmount = 1
                }
            ];
        }

        var result = new List<QuestObjectiveDefinition>();
        var index = 0;
        foreach (var objective in objectives.EnumerateArray())
        {
            var id = objective.ValueKind == JsonValueKind.String
                ? objective.GetString()
                : GetString(objective, "id");
            result.Add(new QuestObjectiveDefinition
            {
                Id = "objective/" + NormalizeIdSegment(string.IsNullOrWhiteSpace(id) ? index.ToString() : id),
                Kind = "custom_counter",
                RequiredAmount = 1
            });
            index++;
        }

        return result.Count == 0
            ?
            [
                new QuestObjectiveDefinition
                {
                    Id = "objective/first",
                    Kind = "custom_counter",
                    RequiredAmount = 1
                }
            ]
            : result;
    }

    private static void MapMechanicsPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "mechanics", out var mechanics) || mechanics.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var mechanic in mechanics.EnumerateArray())
        {
            var sourceId = GetString(mechanic, "id");
            var title = GetString(mechanic, "title");
            var id = "ability/" + NormalizeIdSegment(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
            if (package.Game.Abilities.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                index++;
                continue;
            }

            package.Game.Abilities.Add(new AbilityDefinition
            {
                Id = id,
                Name = string.IsNullOrWhiteSpace(title) ? $"Draft Ability {index + 1}" : title.Trim(),
                Kind = "active",
                Metadata =
                {
                    ["source"] = "mechanics_pack_v1"
                }
            });
            index++;
        }
    }

    private static GeneratorPlanGamePackageAssemblyMapping Mapping(
        GeneratorPlanApprovedArtifact artifact,
        string result,
        string target)
    {
        return new GeneratorPlanGamePackageAssemblyMapping
        {
            ArtifactId = artifact.ArtifactId,
            ArtifactKind = artifact.ArtifactKind,
            ExpectedArtifactContract = artifact.ExpectedArtifactContract,
            Result = result,
            Target = target
        };
    }

    private static string NormalizeEntityPrototypeId(string sourceId, string kind, int index)
    {
        if (string.Equals(kind, "player", StringComparison.OrdinalIgnoreCase))
        {
            return "entity/player";
        }

        var value = !string.IsNullOrWhiteSpace(sourceId)
            ? sourceId
            : !string.IsNullOrWhiteSpace(kind)
                ? kind
                : index.ToString();
        return "entity/" + NormalizeIdSegment(value);
    }

    private static string NormalizeIdSegment(string value)
    {
        var normalized = GeneratorPlanGamePackageAssemblyPolicy.NormalizeSegment(value).Replace('_', '/');
        return string.IsNullOrWhiteSpace(normalized) ? "generated" : normalized;
    }

    private static string ToTitle(string value)
    {
        return string.Join(' ', value.Replace('_', ' ').Replace('-', ' ').Split('/', StringSplitOptions.RemoveEmptyEntries)).Trim();
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        return element.TryGetProperty(propertyName, out value)
            || element.TryGetProperty(char.ToLowerInvariant(propertyName[0]) + propertyName[1..], out value);
    }
}

public sealed record GeneratorPlanGamePackageAssemblerResult
{
    public GamePackageDefinition Package { get; init; } = new();
    public IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyDiagnostic>();
    public IReadOnlyList<GeneratorPlanGamePackageAssemblyMapping> Mappings { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyMapping>();
}
