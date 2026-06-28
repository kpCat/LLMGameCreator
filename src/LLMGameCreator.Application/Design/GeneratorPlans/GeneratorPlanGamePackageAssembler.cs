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
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.items"));
                    break;
                case "dialogue_pack_v1":
                    MapDialoguePack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.dialogues"));
                    break;
                case "encounter_pack_v1":
                    MapEncounterPack(package, document.RootElement);
                    RecordAppliedArtifact(package, artifact, document.RootElement, appliedAtUtc, GeneratorPlanGamePackageAssemblyMappingResult.Mapped);
                    mappings.Add(Mapping(artifact, GeneratorPlanGamePackageAssemblyMappingResult.Mapped, "generatedContent.encounters"));
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
        }
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

        foreach (var encounter in encounters.EnumerateArray())
        {
            var mapped = new GeneratedEncounterDefinition
            {
                SourceId = GetString(encounter, "id").Trim(),
                Title = GetString(encounter, "title").Trim(),
                Description = GetString(encounter, "description").Trim(),
                RegionId = GetString(encounter, "region_id").Trim(),
                SceneId = GetString(encounter, "scene_id").Trim(),
                NpcIds = ReadStringArray(encounter, "npc_ids")
            };
            package.GeneratedContent.Encounters.RemoveAll(candidate => string.Equals(candidate.SourceId, mapped.SourceId, StringComparison.OrdinalIgnoreCase));
            package.GeneratedContent.Encounters.Add(mapped);
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

    private static bool GetBool(JsonElement element, string propertyName, bool fallback)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : fallback;
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
