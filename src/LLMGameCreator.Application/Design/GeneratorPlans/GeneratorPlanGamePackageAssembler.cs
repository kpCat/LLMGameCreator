using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssembler
{
    public GeneratorPlanGamePackageAssemblerResult Assemble(GeneratorPlanApprovedArtifactSet artifactSet)
    {
        return Assemble(artifactSet, DateTimeOffset.UtcNow);
    }

    public GeneratorPlanGamePackageAssemblerResult Assemble(GeneratorPlanApprovedArtifactSet artifactSet, DateTimeOffset appliedAtUtc)
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
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "manifest"));
                    break;
                case "scene_pack_v1":
                    MapScenePack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.maps"));
                    break;
                case "region_pack_v1":
                    MapRegionPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.regions"));
                    break;
                case "npc_pack_v1":
                    MapNpcPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.npcs"));
                    break;
                case "item_pack_v1":
                    MapItemPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.items"));
                    break;
                case "resource_pack_v1":
                    MapResourcePack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.resources"));
                    break;
                case "recipe_pack_v1":
                    MapRecipePack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.recipes"));
                    break;
                case "loot_pack_v1":
                    MapLootPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.lootTables"));
                    break;
                case "transaction_pack_v1":
                case "vendor_pack_v1":
                    MapTransactionPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.transactions"));
                    break;
                case "inventory_pack_v1":
                    MapInventoryPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.inventories"));
                    break;
                case "equipment_pack_v1":
                    MapEquipmentPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.equipmentSlots"));
                    break;
                case "stat_pack_v1":
                    MapStatPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.stats"));
                    break;
                case "ability_pack_v1":
                    MapAbilityPack(package, document.RootElement, "ability_pack_v1");
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.abilities"));
                    break;
                case "status_pack_v1":
                    MapStatusPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.statuses"));
                    break;
                case "progression_pack_v1":
                    MapProgressionPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.progressions"));
                    break;
                case "dialogue_pack_v1":
                    MapDialoguePack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.dialogues"));
                    break;
                case "encounter_pack_v1":
                    MapEncounterPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.encounters"));
                    break;
                case "combat_pack_v1":
                    MapCombatPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.encounters"));
                    break;
                case "entity_pack_v1":
                    MapEntityPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.entity_prototypes"));
                    break;
                case "quest_pack_v1":
                    MapQuestPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.quests"));
                    break;
                case "mechanics_pack_v1":
                    MapMechanicsPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "game.abilities"));
                    break;
                case "semantic_pack_v1":
                    var semanticTermCount = CountSemanticTerms(document.RootElement);
                    diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Warning,
                        GeneratorPlanGamePackageAssemblyDiagnosticCodes.UnmappedArtifactKind,
                        semanticTermCount > 0
                            ? $"Semantic artifacts are acknowledged but do not have a GamePackage field in v1. Terms: {semanticTermCount}."
                            : "Semantic artifacts are acknowledged but do not have a GamePackage field in v1.",
                        artifact.ArtifactId,
                        artifact.ArtifactKind,
                        "semantic_pack_v1"));
                    PreserveArtifact(package, artifact, document.RootElement, "no_game_package_field");
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Unmapped);
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
                    PreserveArtifact(package, artifact, document.RootElement, "unknown_kind");
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Unmapped);
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
        package.GeneratedContent.Profile = new GeneratedGameProfileDefinition
        {
            Title = title.Trim(),
            Description = FirstNonEmpty(GetString(game, "description"), GetString(root, "purpose"), GetString(game, "core_idea")),
            Genre = genre.Trim(),
            Tone = GetString(game, "tone").Trim(),
            PresentationMode = GetString(game, "presentation_mode").Trim(),
            WorldTopology = GetString(game, "world_topology").Trim(),
            ActorModel = GetString(game, "actor_model").Trim(),
            CombatModel = GetString(game, "combat_model").Trim(),
            CoreLoop = ReadStringArray(game, "core_loop"),
            Pillars = ReadStringArray(root, "pillars"),
            SourceContextJson = GetRawJsonOrDefault(root, "source_context")
        };

        var description = FirstNonEmpty(GetString(game, "description"), GetString(root, "purpose"), GetString(game, "core_idea"));
        if (!string.IsNullOrWhiteSpace(description))
        {
            package.Manifest.Description = description.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(genre))
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
            var packageMapId = "map/start";
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
                packageMapId = id;
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

            UpsertGeneratedScene(package, new GeneratedSceneDefinition
            {
                SourceId = sourceId.Trim(),
                PackageMapId = packageMapId,
                Title = title.Trim(),
                Description = GetString(scene, "description").Trim(),
                Purpose = GetString(scene, "purpose").Trim()
            });
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

            TryMapEntityPlacement(package, entity, id, sourceId, "entity");
            index++;
        }
    }

    private static void MapRegionPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "regions", out var regions) || regions.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var region in regions.EnumerateArray())
        {
            var mapped = new GeneratedRegionDefinition
            {
                SourceId = GetString(region, "id").Trim(),
                Title = GetString(region, "title").Trim(),
                Description = GetString(region, "description").Trim(),
                SceneIds = ReadStringArray(region, "scene_ids")
            };
            package.GeneratedContent.Regions.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Regions.Add(mapped);
        }
    }

    private static void MapNpcPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "npcs", out var npcs) || npcs.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var npc in npcs.EnumerateArray())
        {
            var mapped = new GeneratedNpcDefinition
            {
                SourceId = GetString(npc, "id").Trim(),
                Name = GetString(npc, "name").Trim(),
                Description = GetString(npc, "description").Trim(),
                RegionId = GetString(npc, "region_id").Trim(),
                SceneId = GetString(npc, "scene_id").Trim()
            };
            package.GeneratedContent.Npcs.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Npcs.Add(mapped);

            var kind = FirstNonEmpty(GetString(npc, "kind"), "npc");
            var title = FirstNonEmpty(GetString(npc, "title"), mapped.Name);
            var prototypeId = NormalizeEntityPrototypeId(FirstNonEmpty(GetString(npc, "entity_id"), mapped.SourceId), kind, package.Game.EntityPrototypes.Count);
            if (HasPlacementFields(npc) && package.Game.EntityPrototypes.All(candidate => !string.Equals(candidate.Id, prototypeId, StringComparison.OrdinalIgnoreCase)))
            {
                package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
                {
                    Id = prototypeId,
                    Name = string.IsNullOrWhiteSpace(title) ? ToTitle(kind) : title.Trim(),
                    Components =
                    [
                        new ComponentDefinition { Type = NormalizeIdSegment(kind) }
                    ]
                });
            }

            TryMapEntityPlacement(package, npc, prototypeId, mapped.SourceId, "npc");
        }
    }

    private static void MapItemPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in items.EnumerateArray())
        {
            var mapped = new GeneratedItemDefinition
            {
                SourceId = GetString(item, "id").Trim(),
                Name = GetString(item, "name").Trim(),
                Description = GetString(item, "description").Trim()
            };
            package.GeneratedContent.Items.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Items.Add(mapped);

            var packageItemId = NormalizeItemId(FirstNonEmpty(GetString(item, "package_item_id"), GetString(item, "packageItemId"), mapped.SourceId));
            if (package.Game.Items.All(candidate => !string.Equals(candidate.Id, packageItemId, StringComparison.OrdinalIgnoreCase)))
            {
                package.Game.Items.Add(new ItemDefinition
                {
                    Id = packageItemId,
                    Name = FirstNonEmpty(mapped.Name, ToTitle(packageItemId)),
                    Description = mapped.Description,
                    Kind = FirstNonEmpty(GetString(item, "kind"), "generic"),
                    Rarity = NormalizeNullable(GetString(item, "rarity")),
                    MaxStack = GetNullableInt(item, "max_stack", "maxStack"),
                    Value = GetNullableDouble(item, "value"),
                    Weight = GetNullableDouble(item, "weight"),
                    QuestItem = GetNullableBool(item, "quest_item", "questItem"),
                    Unique = GetNullableBool(item, "unique"),
                    Tags = ReadStringArray(item, "tags"),
                    Metadata =
                    {
                        ["source"] = "item_pack_v1",
                        ["source_item_id"] = mapped.SourceId
                    }
                });
            }
        }
    }

    private static void MapResourcePack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "resources", out var resources) || resources.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var resource in resources.EnumerateArray())
        {
            var id = NormalizeResourceId(GetString(resource, "id"));
            if (package.Game.Resources.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Resources.Add(new ResourceDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(resource, "name"), ToTitle(id)),
                Kind = FirstNonEmpty(GetString(resource, "kind"), "abstract"),
                Description = GetString(resource, "description").Trim(),
                DefaultValue = GetNullableDouble(resource, "default_value", "defaultValue"),
                MinValue = GetNullableDouble(resource, "min_value", "minValue"),
                MaxValue = GetNullableDouble(resource, "max_value", "maxValue"),
                RegenPerTick = GetNullableDouble(resource, "regen_per_tick", "regenPerTick"),
                Tags = ReadStringArray(resource, "tags"),
                Metadata = { ["source"] = "resource_pack_v1" }
            });
        }
    }

    private static void MapRecipePack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "recipes", out var recipes) || recipes.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var recipe in recipes.EnumerateArray())
        {
            var id = NormalizeRecipeId(GetString(recipe, "id"));
            if (package.Game.Recipes.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Recipes.Add(new RecipeDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(recipe, "name"), GetString(recipe, "title"), ToTitle(id)),
                Category = FirstNonEmpty(GetString(recipe, "category"), "crafting"),
                StationId = NormalizeNullable(FirstNonEmpty(GetString(recipe, "station_id"), GetString(recipe, "stationId"))),
                Inputs = ReadCosts(recipe, "inputs"),
                Costs = ReadCosts(recipe, "costs"),
                Outputs = ReadOutputs(recipe, "outputs"),
                FailureOutputs = ReadOutputs(recipe, "failure_outputs", "failureOutputs"),
                Duration = GetNullableDouble(recipe, "duration"),
                Cooldown = GetNullableDouble(recipe, "cooldown"),
                SuccessChance = GetNullableDouble(recipe, "success_chance", "successChance"),
                Tags = ReadStringArray(recipe, "tags"),
                Metadata = { ["source"] = "recipe_pack_v1" }
            });
        }
    }

    private static void MapLootPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "loot_tables", out var lootTables) && !TryGetProperty(root, "lootTables", out lootTables))
        {
            return;
        }

        if (lootTables.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var loot in lootTables.EnumerateArray())
        {
            var id = NormalizeLootTableId(GetString(loot, "id"));
            if (package.Game.LootTables.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.LootTables.Add(new LootTableDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(loot, "name"), ToTitle(id)),
                Kind = FirstNonEmpty(GetString(loot, "kind"), "common"),
                Entries = ReadLootEntries(loot),
                Tags = ReadStringArray(loot, "tags"),
                Metadata = { ["source"] = "loot_pack_v1" }
            });
        }
    }

    private static void MapTransactionPack(GamePackageDefinition package, JsonElement root)
    {
        var property = TryGetProperty(root, "transactions", out var transactions) ? transactions :
            TryGetProperty(root, "vendors", out var vendors) ? vendors : default;
        if (property.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var transaction in property.EnumerateArray())
        {
            var id = NormalizeTransactionId(GetString(transaction, "id"));
            if (package.Game.Transactions.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Transactions.Add(new TransactionDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(transaction, "name"), GetString(transaction, "title"), ToTitle(id)),
                Kind = FirstNonEmpty(GetString(transaction, "kind"), "shop"),
                VendorId = NormalizeNullable(FirstNonEmpty(GetString(transaction, "vendor_id"), GetString(transaction, "vendorId"))),
                Costs = ReadCosts(transaction, "costs"),
                Outputs = ReadOutputs(transaction, "outputs"),
                StockLootTableId = NormalizeNullable(FirstNonEmpty(GetString(transaction, "stock_loot_table_id"), GetString(transaction, "stockLootTableId"))),
                RestockRule = NormalizeNullable(FirstNonEmpty(GetString(transaction, "restock_rule"), GetString(transaction, "restockRule"))),
                Tags = ReadStringArray(transaction, "tags"),
                Metadata = { ["source"] = "transaction_pack_v1" }
            });
        }
    }

    private static void MapInventoryPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "inventories", out var inventories) || inventories.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var inventory in inventories.EnumerateArray())
        {
            var id = NormalizeInventoryId(GetString(inventory, "id"));
            if (package.Game.Inventories.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Inventories.Add(new InventoryDefinition
            {
                Id = id,
                OwnerKind = FirstNonEmpty(GetString(inventory, "owner_kind"), GetString(inventory, "ownerKind"), "container"),
                OwnerId = NormalizeNullable(FirstNonEmpty(GetString(inventory, "owner_id"), GetString(inventory, "ownerId"))),
                Slots = Math.Max(0, GetInt(inventory, "slots", 0)),
                Stacks = ReadItemStacks(inventory),
                Tags = ReadStringArray(inventory, "tags"),
                Metadata = { ["source"] = "inventory_pack_v1" }
            });
        }
    }

    private static void MapEquipmentPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "equipment_slots", out var equipmentSlots) && !TryGetProperty(root, "equipmentSlots", out equipmentSlots))
        {
            return;
        }

        if (equipmentSlots.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var slot in equipmentSlots.EnumerateArray())
        {
            var id = NormalizeEquipmentSlotId(GetString(slot, "id"));
            if (package.Game.EquipmentSlots.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.EquipmentSlots.Add(new EquipmentSlotDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(slot, "name"), ToTitle(id)),
                AllowedTags = ReadStringArray(slot, "allowed_tags").Count > 0 ? ReadStringArray(slot, "allowed_tags") : ReadStringArray(slot, "allowedTags"),
                AllowedKinds = ReadStringArray(slot, "allowed_kinds").Count > 0 ? ReadStringArray(slot, "allowed_kinds") : ReadStringArray(slot, "allowedKinds"),
                Metadata = { ["source"] = "equipment_pack_v1" }
            });
        }
    }

    private static void MapStatPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "stats", out var stats) || stats.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var stat in stats.EnumerateArray())
        {
            var id = NormalizeStatId(GetString(stat, "id"));
            if (package.Game.Stats.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Stats.Add(new StatDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(stat, "name"), GetString(stat, "title"), ToTitle(id)),
                Kind = FirstNonEmpty(GetString(stat, "kind"), "attribute"),
                Description = GetString(stat, "description"),
                DefaultValue = GetNullableDouble(stat, "default_value", "defaultValue"),
                MinValue = GetNullableDouble(stat, "min_value", "minValue"),
                MaxValue = GetNullableDouble(stat, "max_value", "maxValue"),
                Tags = ReadStringArray(stat, "tags"),
                Metadata = { ["source"] = "stat_pack_v1" }
            });
        }
    }

    private static void MapAbilityPack(GamePackageDefinition package, JsonElement root, string source)
    {
        var abilities = TryGetProperty(root, "abilities", out var abilityArray) ? abilityArray :
            TryGetProperty(root, "mechanics", out var mechanics) ? mechanics : default;
        if (abilities.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var ability in abilities.EnumerateArray())
        {
            var sourceId = GetString(ability, "id");
            var id = NormalizeAbilityId(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
            if (package.Game.Abilities.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                index++;
                continue;
            }

            var title = FirstNonEmpty(GetString(ability, "name"), GetString(ability, "title"));
            package.Game.Abilities.Add(new AbilityDefinition
            {
                Id = id,
                Name = string.IsNullOrWhiteSpace(title) ? $"Draft Ability {index + 1}" : title.Trim(),
                Kind = FirstNonEmpty(GetString(ability, "kind"), "active"),
                Costs = ReadCosts(ability, "costs"),
                Cooldown = GetNullableInt(ability, "cooldown"),
                Targeting = NormalizeNullable(GetString(ability, "targeting")),
                Range = GetNullableDouble(ability, "range"),
                Power = GetNullableDouble(ability, "power"),
                ResourceId = NormalizeNullable(FirstNonEmpty(GetString(ability, "resource_id"), GetString(ability, "resourceId"))),
                Tags = ReadStringArray(ability, "tags"),
                Effects = ReadEffects(ability, "effects"),
                Metadata =
                {
                    ["source"] = source,
                    ["source_ability_id"] = sourceId.Trim(),
                    ["description"] = GetString(ability, "description").Trim()
                }
            });
            UpsertGeneratedMechanic(package, new GeneratedMechanicDefinition
            {
                SourceId = sourceId.Trim(),
                PackageAbilityId = id,
                Name = string.IsNullOrWhiteSpace(title) ? $"Draft Ability {index + 1}" : title.Trim(),
                Description = GetString(ability, "description").Trim(),
                Tags = ReadStringArray(ability, "tags")
            });
            index++;
        }
    }

    private static void MapStatusPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "statuses", out var statuses) || statuses.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var status in statuses.EnumerateArray())
        {
            var id = NormalizeStatusId(GetString(status, "id"));
            if (package.Game.Statuses.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Statuses.Add(new StatusDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(status, "name"), GetString(status, "title"), ToTitle(id)),
                Description = GetString(status, "description"),
                Kind = FirstNonEmpty(GetString(status, "kind"), "status"),
                DurationMode = NormalizeNullable(FirstNonEmpty(GetString(status, "duration_mode"), GetString(status, "durationMode"))),
                Effects = ReadEffects(status, "effects"),
                Tags = ReadStringArray(status, "tags"),
                Metadata = { ["source"] = "status_pack_v1" }
            });
        }
    }

    private static void MapProgressionPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "progressions", out var progressions) || progressions.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var progression in progressions.EnumerateArray())
        {
            var id = NormalizeProgressionId(GetString(progression, "id"));
            if (package.Game.Progressions.Any(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            package.Game.Progressions.Add(new ProgressionDefinition
            {
                Id = id,
                Name = FirstNonEmpty(GetString(progression, "name"), GetString(progression, "title"), ToTitle(id)),
                Kind = FirstNonEmpty(GetString(progression, "kind"), "xp_level"),
                Description = GetString(progression, "description"),
                Stages = ReadProgressionStages(progression),
                Tags = ReadStringArray(progression, "tags"),
                Metadata = { ["source"] = "progression_pack_v1" }
            });
        }
    }

    private static List<ProgressionStageDefinition> ReadProgressionStages(JsonElement progression)
    {
        var result = new List<ProgressionStageDefinition>();
        if (!TryGetProperty(progression, "stages", out var stages) || stages.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var stage in stages.EnumerateArray())
        {
            var source = FirstNonEmpty(GetString(stage, "id"), GetString(stage, "name"), index.ToString());
            result.Add(new ProgressionStageDefinition
            {
                Id = NormalizeStageId(source),
                Name = FirstNonEmpty(GetString(stage, "name"), GetString(stage, "title"), ToTitle(source)),
                RequiredAmount = GetDouble(stage, "required_amount", GetDouble(stage, "requiredAmount", index)),
                Outputs = ReadOutputs(stage, "outputs", "rewards"),
                Metadata = { ["source"] = "progression_pack_v1" }
            });
            index++;
        }

        return result;
    }

    private static void MapDialoguePack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "dialogues", out var dialogues) || dialogues.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var dialogue in dialogues.EnumerateArray())
        {
            var sourceId = GetString(dialogue, "id");
            var title = GetString(dialogue, "title");
            var dialogueId = NormalizeDialogueId(sourceId, index);
            var nodes = BuildDialogueNodes(dialogue);
            var startNodeId = FirstNonEmpty(GetString(dialogue, "start_node_id"), GetString(dialogue, "startNodeId"), nodes.FirstOrDefault()?.Id ?? string.Empty);
            if (nodes.Count == 0)
            {
                nodes.Add(new DialogueNodeDefinition
                {
                    Id = "start",
                    Text = FirstNonEmpty(GetString(dialogue, "description"), title, $"Dialogue {index + 1}"),
                    Choices = [new DialogueChoiceDefinition { Id = "close", Text = "Continue.", CloseDialogue = true }]
                });
                startNodeId = "start";
            }

            if (package.Game.Dialogues.All(candidate => !string.Equals(candidate.Id, dialogueId, StringComparison.OrdinalIgnoreCase)))
            {
                package.Game.Dialogues.Add(new DialogueDefinition
                {
                    Id = dialogueId,
                    Title = string.IsNullOrWhiteSpace(title) ? $"Draft Dialogue {index + 1}" : title.Trim(),
                    StartNodeId = string.IsNullOrWhiteSpace(startNodeId) ? nodes[0].Id : startNodeId.Trim(),
                    Nodes = nodes,
                    Metadata =
                    {
                        ["source"] = "game_package_assembly",
                        ["source_dialogue_id"] = sourceId.Trim()
                    }
                });
            }

            var mapped = new GeneratedDialogueDefinition
            {
                SourceId = sourceId.Trim(),
                Title = title.Trim(),
                Description = GetString(dialogue, "description").Trim(),
                NpcId = GetString(dialogue, "npc_id").Trim(),
                SceneId = GetString(dialogue, "scene_id").Trim(),
                Lines = ReadStringArray(dialogue, "lines")
            };
            package.GeneratedContent.Dialogues.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Dialogues.Add(mapped);
            index++;
        }
    }

    private static void MapEncounterPack(GamePackageDefinition package, JsonElement root)
    {
        if (!TryGetProperty(root, "encounters", out var encounters) || encounters.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var index = 0;
        foreach (var encounter in encounters.EnumerateArray())
        {
            var sourceId = GetString(encounter, "id");
            var id = NormalizeEncounterId(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
            if (package.Game.Encounters.All(candidate => !string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                package.Game.Encounters.Add(new EncounterDefinition
                {
                    Id = id,
                    Name = FirstNonEmpty(GetString(encounter, "name"), GetString(encounter, "title"), ToTitle(id)),
                    Kind = FirstNonEmpty(GetString(encounter, "kind"), "combat"),
                    Participants = ReadEncounterParticipants(encounter),
                    Actions = ReadEncounterActions(encounter),
                    Rewards = ReadOutputs(encounter, "rewards"),
                    Consequences = ReadOutputs(encounter, "consequences"),
                    LootTableId = NormalizeNullable(FirstNonEmpty(GetString(encounter, "loot_table_id"), GetString(encounter, "lootTableId"))),
                    DefaultSeed = GetNullableInt(encounter, "default_seed", "defaultSeed"),
                    Tags = ReadStringArray(encounter, "tags"),
                    Metadata = { ["source"] = "encounter_pack_v1", ["source_encounter_id"] = sourceId.Trim() }
                });
            }

            var mapped = new GeneratedEncounterDefinition
            {
                SourceId = sourceId.Trim(),
                Title = GetString(encounter, "title").Trim(),
                Description = GetString(encounter, "description").Trim(),
                RegionId = GetString(encounter, "region_id").Trim(),
                SceneId = GetString(encounter, "scene_id").Trim(),
                NpcIds = ReadStringArray(encounter, "npc_ids")
            };
            package.GeneratedContent.Encounters.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Encounters.Add(mapped);
            index++;
        }
    }

    private static void MapCombatPack(GamePackageDefinition package, JsonElement root)
    {
        MapEncounterPack(package, root);
        MapAbilityPack(package, root, "combat_pack_v1");
    }

    private static List<EncounterParticipantDefinition> ReadEncounterParticipants(JsonElement encounter)
    {
        var result = new List<EncounterParticipantDefinition>();
        if (!TryGetProperty(encounter, "participants", out var participants) || participants.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var participant in participants.EnumerateArray())
        {
            result.Add(new EncounterParticipantDefinition
            {
                Id = FirstNonEmpty(GetString(participant, "id"), $"participant/{index}"),
                Name = FirstNonEmpty(GetString(participant, "name"), GetString(participant, "title"), $"Participant {index + 1}"),
                Kind = FirstNonEmpty(GetString(participant, "kind"), "enemy"),
                EntityPrototypeId = NormalizeNullable(FirstNonEmpty(GetString(participant, "entity_prototype_id"), GetString(participant, "entityPrototypeId"))),
                FactionId = NormalizeNullable(FirstNonEmpty(GetString(participant, "faction_id"), GetString(participant, "factionId"))),
                Team = FirstNonEmpty(GetString(participant, "team"), "neutral"),
                Stats = ReadOutputs(participant, "stats"),
                Resources = ReadOutputs(participant, "resources"),
                Abilities = ReadStringArray(participant, "abilities"),
                InventoryId = NormalizeNullable(FirstNonEmpty(GetString(participant, "inventory_id"), GetString(participant, "inventoryId"))),
                EquipmentOwnerId = NormalizeNullable(FirstNonEmpty(GetString(participant, "equipment_owner_id"), GetString(participant, "equipmentOwnerId"))),
                Tags = ReadStringArray(participant, "tags")
            });
            index++;
        }

        return result;
    }

    private static List<EncounterActionDefinition> ReadEncounterActions(JsonElement encounter)
    {
        var result = new List<EncounterActionDefinition>();
        if (!TryGetProperty(encounter, "actions", out var actions) || actions.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var action in actions.EnumerateArray())
        {
            result.Add(new EncounterActionDefinition
            {
                Id = FirstNonEmpty(GetString(action, "id"), $"action/{index}"),
                Name = FirstNonEmpty(GetString(action, "name"), GetString(action, "title"), $"Action {index + 1}"),
                Kind = FirstNonEmpty(GetString(action, "kind"), "ability"),
                AbilityId = NormalizeNullable(FirstNonEmpty(GetString(action, "ability_id"), GetString(action, "abilityId"))),
                Costs = ReadCosts(action, "costs"),
                Outputs = ReadOutputs(action, "outputs"),
                Targeting = NormalizeNullable(GetString(action, "targeting")),
                Cooldown = GetNullableInt(action, "cooldown"),
                Tags = ReadStringArray(action, "tags")
            });
            index++;
        }

        return result;
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
                Stages = BuildQuestStages(quest),
                Metadata =
                {
                    ["source"] = "game_package_assembly"
                }
            });
            UpsertGeneratedQuest(package, new GeneratedQuestSeedDefinition
            {
                SourceId = sourceId.Trim(),
                PackageQuestId = id,
                Title = string.IsNullOrWhiteSpace(title) ? $"Draft Quest {index + 1}" : title.Trim(),
                Description = GetString(quest, "description").Trim(),
                Steps = ReadStringArray(quest, "steps"),
                Objectives = ReadStringArray(quest, "objectives")
            });
            index++;
        }
    }

    private static List<QuestObjectiveDefinition> BuildQuestObjectives(JsonElement quest)
    {
        var result = new List<QuestObjectiveDefinition>();
        var index = 0;
        if (TryGetProperty(quest, "objectives", out var objectives) && objectives.ValueKind == JsonValueKind.Array)
        {
            foreach (var objective in objectives.EnumerateArray())
            {
                result.Add(BuildQuestObjective(objective, index));
                index++;
            }
        }

        if (TryGetProperty(quest, "steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
        {
            foreach (var step in steps.EnumerateArray())
            {
                result.Add(BuildQuestObjective(step, index));
                index++;
            }
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

    private static QuestObjectiveDefinition BuildQuestObjective(JsonElement objective, int index)
    {
        var source = objective.ValueKind == JsonValueKind.String
            ? objective.GetString() ?? string.Empty
            : FirstNonEmpty(GetString(objective, "id"), GetString(objective, "title"), GetString(objective, "text"));
        var text = objective.ValueKind == JsonValueKind.String
            ? objective.GetString() ?? string.Empty
            : FirstNonEmpty(GetString(objective, "text"), GetString(objective, "title"), GetString(objective, "description"));
        var result = new QuestObjectiveDefinition
        {
            Id = "objective/" + NormalizeIdSegment(string.IsNullOrWhiteSpace(source) ? index.ToString() : source),
            Kind = objective.ValueKind == JsonValueKind.Object ? FirstNonEmpty(GetString(objective, "kind"), "custom_counter") : "custom_counter",
            TargetId = objective.ValueKind == JsonValueKind.Object ? FirstNonEmpty(GetString(objective, "target_id"), GetString(objective, "targetId")) : string.Empty,
            RequiredAmount = objective.ValueKind == JsonValueKind.Object ? Math.Max(1, GetInt(objective, "required_amount", GetInt(objective, "requiredAmount", 1))) : 1
        };

        if (!string.IsNullOrWhiteSpace(text))
        {
            result.Metadata["text"] = text.Trim();
        }

        return result;
    }

    private static List<QuestStageDefinition> BuildQuestStages(JsonElement quest)
    {
        var result = new List<QuestStageDefinition>();
        if (!TryGetProperty(quest, "stages", out var stages) || stages.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var stage in stages.EnumerateArray())
        {
            var source = FirstNonEmpty(GetString(stage, "id"), GetString(stage, "title"), index.ToString());
            var stageObjectives = new List<QuestObjectiveDefinition>();
            if (TryGetProperty(stage, "objectives", out var objectives) && objectives.ValueKind == JsonValueKind.Array)
            {
                var objectiveIndex = 0;
                foreach (var objective in objectives.EnumerateArray())
                {
                    stageObjectives.Add(BuildQuestObjective(objective, objectiveIndex));
                    objectiveIndex++;
                }
            }

            result.Add(new QuestStageDefinition
            {
                Id = "stage/" + NormalizeIdSegment(source),
                Text = FirstNonEmpty(GetString(stage, "text"), GetString(stage, "title"), $"Stage {index + 1}"),
                NextStageId = NormalizeNullableStageId(FirstNonEmpty(GetString(stage, "next_stage_id"), GetString(stage, "nextStageId"))),
                Objectives = stageObjectives
            });
            index++;
        }

        return result;
    }

    private static List<DialogueNodeDefinition> BuildDialogueNodes(JsonElement dialogue)
    {
        var result = new List<DialogueNodeDefinition>();
        if (!TryGetProperty(dialogue, "nodes", out var nodes) || nodes.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var node in nodes.EnumerateArray())
        {
            var nodeId = FirstNonEmpty(GetString(node, "id"), index == 0 ? "start" : $"node/{index}");
            result.Add(new DialogueNodeDefinition
            {
                Id = nodeId.Trim(),
                SpeakerId = FirstNonEmpty(GetString(node, "speaker_id"), GetString(node, "speakerId")),
                Expression = FirstNonEmpty(GetString(node, "expression"), "neutral"),
                Text = FirstNonEmpty(GetString(node, "text"), $"Dialogue node {index + 1}"),
                Choices = BuildDialogueChoices(node)
            });
            index++;
        }

        return result;
    }

    private static List<DialogueChoiceDefinition> BuildDialogueChoices(JsonElement node)
    {
        var result = new List<DialogueChoiceDefinition>();
        if (!TryGetProperty(node, "choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var choice in choices.EnumerateArray())
        {
            result.Add(new DialogueChoiceDefinition
            {
                Id = FirstNonEmpty(GetString(choice, "id"), $"choice/{index}"),
                Text = FirstNonEmpty(GetString(choice, "text"), "Continue."),
                TargetNodeId = NormalizeNullable(FirstNonEmpty(GetString(choice, "target_node_id"), GetString(choice, "targetNodeId"))),
                CloseDialogue = GetBool(choice, "close_dialogue", GetBool(choice, "closeDialogue", false)),
                StartQuestId = NormalizeNullable(FirstNonEmpty(GetString(choice, "start_quest_id"), GetString(choice, "startQuestId"))),
                AdvanceQuestId = NormalizeNullable(FirstNonEmpty(GetString(choice, "advance_quest_id"), GetString(choice, "advanceQuestId"))),
                SetQuestStageId = NormalizeNullableStageId(FirstNonEmpty(GetString(choice, "set_quest_stage_id"), GetString(choice, "setQuestStageId")))
            });
            index++;
        }

        return result;
    }

    private static List<CostDefinition> ReadCosts(JsonElement element, params string[] propertyNames)
    {
        var result = new List<CostDefinition>();
        if (!TryGetFirstArray(element, propertyNames, out var costs))
        {
            return result;
        }

        foreach (var cost in costs.EnumerateArray())
        {
            result.Add(new CostDefinition
            {
                Kind = FirstNonEmpty(GetString(cost, "kind"), "item"),
                Id = FirstNonEmpty(GetString(cost, "id"), GetString(cost, "item_id"), GetString(cost, "resource_id")),
                Amount = GetDouble(cost, "amount", 1)
            });
        }

        return result;
    }

    private static List<OutputDefinition> ReadOutputs(JsonElement element, params string[] propertyNames)
    {
        var result = new List<OutputDefinition>();
        if (!TryGetFirstArray(element, propertyNames, out var outputs))
        {
            return result;
        }

        foreach (var output in outputs.EnumerateArray())
        {
            result.Add(new OutputDefinition
            {
                Kind = FirstNonEmpty(GetString(output, "kind"), "item"),
                Id = FirstNonEmpty(GetString(output, "id"), GetString(output, "item_id"), GetString(output, "resource_id"), GetString(output, "loot_table_id")),
                Amount = GetDouble(output, "amount", 1)
            });
        }

        return result;
    }

    private static List<EffectDefinition> ReadEffects(JsonElement element, params string[] propertyNames)
    {
        var result = new List<EffectDefinition>();
        if (!TryGetFirstArray(element, propertyNames, out var effects))
        {
            return result;
        }

        foreach (var effect in effects.EnumerateArray())
        {
            var type = FirstNonEmpty(GetString(effect, "type"), GetString(effect, "kind"));
            if (string.IsNullOrWhiteSpace(type))
            {
                continue;
            }

            var args = new Dictionary<string, string>(StringComparer.Ordinal);
            AddEffectArg(args, "id", FirstNonEmpty(GetString(effect, "id"), GetString(effect, "resource_id"), GetString(effect, "resourceId"), GetString(effect, "status_id"), GetString(effect, "statusId"), GetString(effect, "item_id"), GetString(effect, "itemId")));
            AddEffectArg(args, "resourceId", FirstNonEmpty(GetString(effect, "resource_id"), GetString(effect, "resourceId")));
            AddEffectArg(args, "statusId", FirstNonEmpty(GetString(effect, "status_id"), GetString(effect, "statusId")));
            AddEffectArg(args, "itemId", FirstNonEmpty(GetString(effect, "item_id"), GetString(effect, "itemId")));
            if (TryGetProperty(effect, "amount", out var amount) && amount.ValueKind == JsonValueKind.Number)
            {
                args["amount"] = amount.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            result.Add(new EffectDefinition { Type = type.Trim(), Args = args });
        }

        return result;
    }

    private static void AddEffectArg(Dictionary<string, string> args, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            args[key] = value.Trim();
        }
    }

    private static List<LootEntryDefinition> ReadLootEntries(JsonElement loot)
    {
        var result = new List<LootEntryDefinition>();
        if (!TryGetProperty(loot, "entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var index = 0;
        foreach (var entry in entries.EnumerateArray())
        {
            var outputs = ReadOutputs(entry, "outputs", "output");
            result.Add(new LootEntryDefinition
            {
                Id = FirstNonEmpty(GetString(entry, "id"), $"entry/{index}"),
                Output = outputs.Count > 0 ? outputs[0] : new OutputDefinition(),
                Weight = GetNullableDouble(entry, "weight"),
                MinCount = GetNullableInt(entry, "min_count", "minCount"),
                MaxCount = GetNullableInt(entry, "max_count", "maxCount"),
                Rarity = NormalizeNullable(GetString(entry, "rarity")),
                Tags = ReadStringArray(entry, "tags")
            });
            index++;
        }

        return result;
    }

    private static List<ItemStackDefinition> ReadItemStacks(JsonElement inventory)
    {
        var result = new List<ItemStackDefinition>();
        if (!TryGetProperty(inventory, "stacks", out var stacks) || stacks.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var stack in stacks.EnumerateArray())
        {
            result.Add(new ItemStackDefinition
            {
                ItemId = FirstNonEmpty(GetString(stack, "item_id"), GetString(stack, "itemId")),
                Amount = GetDouble(stack, "amount", 1),
                UniqueInstanceId = NormalizeNullable(FirstNonEmpty(GetString(stack, "unique_instance_id"), GetString(stack, "uniqueInstanceId"))),
                QuestItem = GetNullableBool(stack, "quest_item", "questItem"),
                Durability = GetNullableDouble(stack, "durability"),
                Charge = GetNullableDouble(stack, "charge")
            });
        }

        return result;
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
            var title = FirstNonEmpty(GetString(mechanic, "title"), GetString(mechanic, "name"));
            var description = GetString(mechanic, "description");
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
                    ["source"] = "mechanics_pack_v1",
                    ["source_mechanic_id"] = sourceId.Trim(),
                    ["description"] = description.Trim()
                }
            });
            UpsertGeneratedMechanic(package, new GeneratedMechanicDefinition
            {
                SourceId = sourceId.Trim(),
                PackageAbilityId = id,
                Name = string.IsNullOrWhiteSpace(title) ? $"Draft Ability {index + 1}" : title.Trim(),
                Description = description.Trim(),
                Tags = ReadStringArray(mechanic, "tags")
            });
            index++;
        }
    }

    private static void RecordAppliedArtifact(
        GamePackageDefinition package,
        GeneratorPlanApprovedArtifact artifact,
        JsonElement root,
        DateTimeOffset appliedAtUtc,
        string mappingResult)
    {
        var contractId = FirstNonEmpty(artifact.ExpectedArtifactContract, artifact.ArtifactKind);
        var provenance = new GeneratedContentArtifactProvenance
        {
            ArtifactId = artifact.ArtifactId,
            ContractId = contractId,
            ArtifactKind = artifact.ArtifactKind,
            CapabilitySelectionId = ReadSourceContextString(root, "capability_selection_id"),
            GeneratedAt = FirstNonEmpty(GetString(root, "generated_at"), ReadSourceContextString(root, "generated_at")),
            AuditId = FirstNonEmpty(GetString(root, "audit_id"), ReadSourceContextString(root, "audit_id"), ReadSourceContextString(root, "evaluation_id")),
            AppliedAt = appliedAtUtc.UtcDateTime.ToString("O"),
            ContentHash = Sha256(root.GetRawText()),
            MappingResult = mappingResult
        };

        package.GeneratedContent.AppliedArtifacts.RemoveAll(candidate =>
            string.Equals(candidate.ArtifactId, provenance.ArtifactId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(candidate.ContractId, provenance.ContractId, StringComparison.OrdinalIgnoreCase));
        package.GeneratedContent.AppliedArtifacts.Add(provenance);
    }

    private static void PreserveArtifact(
        GamePackageDefinition package,
        GeneratorPlanApprovedArtifact artifact,
        JsonElement root,
        string reason)
    {
        var preserved = new PreservedGeneratedArtifactDefinition
        {
            ArtifactId = artifact.ArtifactId,
            ContractId = FirstNonEmpty(artifact.ExpectedArtifactContract, artifact.ArtifactKind),
            ArtifactKind = artifact.ArtifactKind,
            Reason = reason,
            RawJson = root.GetRawText()
        };

        package.GeneratedContent.PreservedArtifacts.RemoveAll(candidate =>
            string.Equals(candidate.ArtifactId, preserved.ArtifactId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(candidate.ContractId, preserved.ContractId, StringComparison.OrdinalIgnoreCase));
        package.GeneratedContent.PreservedArtifacts.Add(preserved);
    }

    private static void UpsertGeneratedScene(GamePackageDefinition package, GeneratedSceneDefinition scene)
    {
        package.GeneratedContent.Scenes.RemoveAll(candidate =>
            string.Equals(candidate.SourceId, scene.SourceId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.PackageMapId, scene.PackageMapId, StringComparison.OrdinalIgnoreCase));
        package.GeneratedContent.Scenes.Add(scene);
    }

    private static void UpsertGeneratedQuest(GamePackageDefinition package, GeneratedQuestSeedDefinition quest)
    {
        package.GeneratedContent.Quests.RemoveAll(candidate =>
            string.Equals(candidate.SourceId, quest.SourceId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.PackageQuestId, quest.PackageQuestId, StringComparison.OrdinalIgnoreCase));
        package.GeneratedContent.Quests.Add(quest);
    }

    private static void UpsertGeneratedMechanic(GamePackageDefinition package, GeneratedMechanicDefinition mechanic)
    {
        package.GeneratedContent.Mechanics.RemoveAll(candidate =>
            string.Equals(candidate.SourceId, mechanic.SourceId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.PackageAbilityId, mechanic.PackageAbilityId, StringComparison.OrdinalIgnoreCase));
        package.GeneratedContent.Mechanics.Add(mechanic);
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

    private static void TryMapEntityPlacement(
        GamePackageDefinition package,
        JsonElement record,
        string defaultPrototypeId,
        string sourceId,
        string fallbackKind)
    {
        if (!HasPlacementFields(record))
        {
            return;
        }

        var mapId = ResolvePackageMapId(package, record);
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        var map = package.Game.Maps.FirstOrDefault(candidate => string.Equals(candidate.Id, mapId, StringComparison.OrdinalIgnoreCase));
        if (map == null)
        {
            return;
        }

        if (!TryReadPosition(record, out var x, out var y))
        {
            return;
        }

        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
        {
            return;
        }

        var prototypeId = FirstNonEmpty(GetString(record, "prototype_id"), GetString(record, "prototypeId"), defaultPrototypeId);
        if (package.Game.EntityPrototypes.All(candidate => !string.Equals(candidate.Id, prototypeId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var instanceId = FirstNonEmpty(GetString(record, "instance_id"), GetString(record, "instanceId"));
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            instanceId = "entity/instance/" + NormalizeIdSegment(FirstNonEmpty(sourceId, fallbackKind, map.Entities.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        if (map.Entities.Any(candidate => string.Equals(candidate.Id, instanceId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        map.Entities.Add(new EntityInstanceDefinition
        {
            Id = instanceId,
            PrototypeId = prototypeId,
            Position = new Position2D(x, y)
        });
    }

    private static bool HasPlacementFields(JsonElement record) =>
        TryGetProperty(record, "position", out _)
        || TryGetProperty(record, "map_position", out _)
        || TryGetProperty(record, "x", out _)
        || TryGetProperty(record, "map_id", out _)
        || TryGetProperty(record, "package_map_id", out _)
        || TryGetProperty(record, "packageMapId", out _)
        || TryGetProperty(record, "scene_id", out _);

    private static string ResolvePackageMapId(GamePackageDefinition package, JsonElement record)
    {
        var mapId = FirstNonEmpty(GetString(record, "package_map_id"), GetString(record, "packageMapId"), GetString(record, "map_id"), GetString(record, "mapId"));
        if (!string.IsNullOrWhiteSpace(mapId))
        {
            return mapId.Trim();
        }

        var sceneId = FirstNonEmpty(GetString(record, "scene_id"), GetString(record, "sceneId"));
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            return string.Empty;
        }

        return package.GeneratedContent.Scenes
            .FirstOrDefault(scene => string.Equals(scene.SourceId, sceneId, StringComparison.OrdinalIgnoreCase))
            ?.PackageMapId ?? string.Empty;
    }

    private static bool TryReadPosition(JsonElement record, out int x, out int y)
    {
        if (TryGetProperty(record, "position", out var position) && TryReadPositionObject(position, out x, out y))
        {
            return true;
        }

        if (TryGetProperty(record, "map_position", out var mapPosition) && TryReadPositionObject(mapPosition, out x, out y))
        {
            return true;
        }

        x = TryGetProperty(record, "x", out var xElement) && xElement.ValueKind == JsonValueKind.Number ? xElement.GetInt32() : -1;
        y = TryGetProperty(record, "y", out var yElement) && yElement.ValueKind == JsonValueKind.Number ? yElement.GetInt32() : -1;
        return x >= 0 && y >= 0;
    }

    private static bool TryReadPositionObject(JsonElement position, out int x, out int y)
    {
        if (position.ValueKind == JsonValueKind.Object)
        {
            x = TryGetProperty(position, "x", out var xElement) && xElement.ValueKind == JsonValueKind.Number ? xElement.GetInt32() : -1;
            y = TryGetProperty(position, "y", out var yElement) && yElement.ValueKind == JsonValueKind.Number ? yElement.GetInt32() : -1;
            return x >= 0 && y >= 0;
        }

        if (position.ValueKind == JsonValueKind.Array && position.GetArrayLength() >= 2)
        {
            var values = position.EnumerateArray().Take(2).ToArray();
            x = values[0].ValueKind == JsonValueKind.Number ? values[0].GetInt32() : -1;
            y = values[1].ValueKind == JsonValueKind.Number ? values[1].GetInt32() : -1;
            return x >= 0 && y >= 0;
        }

        x = -1;
        y = -1;
        return false;
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
        var normalized = NormalizeIdSegment(value);
        return normalized.StartsWith("entity/", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : "entity/" + normalized;
    }

    private static string NormalizeDialogueId(string sourceId, int index)
    {
        var normalized = NormalizeIdSegment(string.IsNullOrWhiteSpace(sourceId) ? index.ToString() : sourceId);
        return normalized.StartsWith("dialogue/", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : "dialogue/" + normalized;
    }

    private static string NormalizeItemId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("item/", StringComparison.OrdinalIgnoreCase) ? normalized : "item/" + normalized;
    }

    private static string NormalizeResourceId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("resource/", StringComparison.OrdinalIgnoreCase) ? normalized : "resource/" + normalized;
    }

    private static string NormalizeRecipeId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("recipe/", StringComparison.OrdinalIgnoreCase) ? normalized : "recipe/" + normalized;
    }

    private static string NormalizeLootTableId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("loot/", StringComparison.OrdinalIgnoreCase) ? normalized : "loot/" + normalized;
    }

    private static string NormalizeTransactionId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("transaction/", StringComparison.OrdinalIgnoreCase) ? normalized : "transaction/" + normalized;
    }

    private static string NormalizeInventoryId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("inventory/", StringComparison.OrdinalIgnoreCase) ? normalized : "inventory/" + normalized;
    }

    private static string NormalizeEquipmentSlotId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("equipment/", StringComparison.OrdinalIgnoreCase) ? normalized : "equipment/" + normalized;
    }

    private static string NormalizeStatId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("stat/", StringComparison.OrdinalIgnoreCase) ? normalized : "stat/" + normalized;
    }

    private static string NormalizeAbilityId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("ability/", StringComparison.OrdinalIgnoreCase) ? normalized : "ability/" + normalized;
    }

    private static string NormalizeStatusId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("status/", StringComparison.OrdinalIgnoreCase) ? normalized : "status/" + normalized;
    }

    private static string NormalizeProgressionId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("progression/", StringComparison.OrdinalIgnoreCase) ? normalized : "progression/" + normalized;
    }

    private static string NormalizeEncounterId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("encounter/", StringComparison.OrdinalIgnoreCase) ? normalized : "encounter/" + normalized;
    }

    private static string NormalizeStageId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("stage/", StringComparison.OrdinalIgnoreCase) ? normalized : "stage/" + normalized;
    }

    private static string? NormalizeNullable(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeNullableStageId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = NormalizeIdSegment(value);
        return normalized.StartsWith("stage/", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : "stage/" + normalized;
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

    private static int GetInt(JsonElement element, string propertyName, int fallback)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : fallback;
    }

    private static int? GetNullableInt(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
        }

        return null;
    }

    private static double GetDouble(JsonElement element, string propertyName, double fallback) =>
        TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : fallback;

    private static double? GetNullableDouble(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
            {
                return property.GetDouble();
            }
        }

        return null;
    }

    private static bool GetBool(JsonElement element, string propertyName, bool fallback)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : fallback;
    }

    private static bool? GetNullableBool(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return property.GetBoolean();
            }
        }

        return null;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }

    private static List<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return new List<string>();
        }

        return array
            .EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();
    }

    private static bool TryGetFirstArray(JsonElement element, IEnumerable<string> propertyNames, out JsonElement array)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out array) && array.ValueKind == JsonValueKind.Array)
            {
                return true;
            }
        }

        array = default;
        return false;
    }

    private static string GetRawJsonOrDefault(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind is JsonValueKind.Object or JsonValueKind.Array
            ? property.GetRawText()
            : "{}";
    }

    private static string ReadSourceContextString(JsonElement root, string propertyName)
    {
        return TryGetProperty(root, "source_context", out var sourceContext) && sourceContext.ValueKind == JsonValueKind.Object
            ? GetString(sourceContext, propertyName)
            : string.Empty;
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static int CountSemanticTerms(JsonElement root)
    {
        if (!TryGetProperty(root, "semantic_groups", out var groups) || groups.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        var count = 0;
        foreach (var group in groups.EnumerateArray())
        {
            if (TryGetProperty(group, "terms", out var terms) && terms.ValueKind == JsonValueKind.Array)
            {
                count += terms.GetArrayLength();
            }
        }

        return count;
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
