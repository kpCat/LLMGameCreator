using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public static class GenericGamePackageProjectionAdapter
    {
        public const string SamplePackageRelativePath = "samples/minimal-map-game/package.json";

        public static GenericGamePackageProjectionModel LoadSamplePackageProjection(List<string> diagnostics)
        {
            var model = new GenericGamePackageProjectionModel();
            var repoRoot = AcceptedAlphaPlayableProjectionDiagnostics.ResolveRepositoryRoot(diagnostics);
            if (string.IsNullOrWhiteSpace(repoRoot))
            {
                model.Diagnostics.AddRange(diagnostics);
                return model;
            }

            var packagePath = Path.GetFullPath(Path.Combine(
                repoRoot,
                SamplePackageRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsUnderRoot(repoRoot, packagePath))
            {
                diagnostics.Add("goal123_sample_package_outside_repository:" + packagePath);
                model.Diagnostics.AddRange(diagnostics);
                return model;
            }

            if (!File.Exists(packagePath))
            {
                diagnostics.Add("goal123_sample_package_missing:" + SamplePackageRelativePath);
                model.Diagnostics.AddRange(diagnostics);
                return model;
            }

            var json = File.ReadAllText(packagePath, Encoding.UTF8);
            var manifestJson = ObjectField(json, "manifest");
            var gameJson = ObjectField(json, "game");
            if (string.IsNullOrWhiteSpace(manifestJson) || string.IsNullOrWhiteSpace(gameJson))
            {
                diagnostics.Add("goal123_sample_package_empty_document");
                model.Diagnostics.AddRange(diagnostics);
                return model;
            }

            model.PackageId = StringField(manifestJson, "packageId");
            model.PackageTitle = StringField(manifestJson, "title");
            model.StartMapId = StringField(manifestJson, "startMapId");

            var mapJson = SelectMapJson(gameJson, model.StartMapId);
            if (string.IsNullOrWhiteSpace(mapJson))
            {
                diagnostics.Add("goal123_sample_package_map_missing");
                model.Diagnostics.AddRange(diagnostics);
                return model;
            }

            model.MapId = StringField(mapJson, "id");
            model.MapName = StringField(mapJson, "name");
            model.MapWidth = Mathf.Max(0, IntField(mapJson, "width"));
            model.MapHeight = Mathf.Max(0, IntField(mapJson, "height"));
            var startPositionJson = ObjectField(mapJson, "startPosition");
            model.StartX = IntField(startPositionJson, "x");
            model.StartY = IntField(startPositionJson, "y");

            var tilePrototypes = BuildTilePrototypeMap(gameJson);
            var explicitTiles = BuildExplicitTileMap(mapJson);
            var defaultTileId = StringField(mapJson, "defaultTileId");
            for (var y = 0; y < model.MapHeight; y++)
            {
                for (var x = 0; x < model.MapWidth; x++)
                {
                    GenericGamePackageTile explicitTile;
                    var key = CoordinateKey(x, y);
                    var explicitPresent = explicitTiles.TryGetValue(key, out explicitTile);
                    var tileId = explicitPresent ? explicitTile.tileId : defaultTileId;
                    GenericGamePackageTilePrototype prototype;
                    tilePrototypes.TryGetValue(tileId ?? string.Empty, out prototype);
                    model.Tiles.Add(new GenericGamePackageProjectionTile
                    {
                        X = x,
                        Y = y,
                        TileId = tileId ?? string.Empty,
                        TileName = prototype == null ? tileId ?? string.Empty : prototype.name ?? string.Empty,
                        TileKind = ClassifyTile(tileId),
                        Explicit = explicitPresent,
                        Walkable = prototype == null || prototype.walkable
                    });
                }
            }

            var entityPrototypes = BuildEntityPrototypeMap(gameJson);
            foreach (var entityJson in TopLevelObjectBlocks(ArrayField(mapJson, "entities")))
            {
                GenericGamePackageEntityPrototype prototype;
                var prototypeId = StringField(entityJson, "prototypeId");
                entityPrototypes.TryGetValue(prototypeId, out prototype);
                model.Entities.Add(BuildEntityProjection(entityJson, prototype));
            }

            foreach (var itemJson in TopLevelObjectBlocks(ArrayField(gameJson, "items")))
            {
                model.Items.Add(new GenericGamePackageProjectionItem
                {
                    ItemId = StringField(itemJson, "id"),
                    Name = StringField(itemJson, "name"),
                    Kind = StringField(itemJson, "kind")
                });
            }

            var itemNames = BuildItemNameMap(model.Items);
            foreach (var resourceJson in TopLevelObjectBlocks(ArrayField(gameJson, "resources")))
            {
                model.Resources.Add(new GenericGamePackageProjectionResource
                {
                    ResourceId = StringField(resourceJson, "id"),
                    Name = StringField(resourceJson, "name"),
                    Kind = StringField(resourceJson, "kind"),
                    DefaultValue = IntField(resourceJson, "defaultValue"),
                    MinValue = IntField(resourceJson, "minValue"),
                    MaxValue = IntField(resourceJson, "maxValue")
                });
            }

            foreach (var inventoryJson in TopLevelObjectBlocks(ArrayField(gameJson, "inventories")))
            {
                model.Inventories.Add(BuildInventoryProjection(inventoryJson, itemNames));
            }

            foreach (var questJson in TopLevelObjectBlocks(ArrayField(gameJson, "quests")))
            {
                model.Quests.Add(BuildQuestProjection(questJson, itemNames));
            }

            foreach (var dialogueJson in TopLevelObjectBlocks(ArrayField(gameJson, "dialogues")))
            {
                model.Dialogues.Add(BuildDialogueProjection(dialogueJson));
            }

            foreach (var interactionJson in TopLevelObjectBlocks(ArrayField(gameJson, "interactions")))
            {
                model.Interactions.Add(BuildInteractionProjection(interactionJson));
            }

            model.Diagnostics.AddRange(diagnostics);
            return model;
        }

        private static string SelectMapJson(string gameJson, string startMapId)
        {
            var maps = TopLevelObjectBlocks(ArrayField(gameJson, "maps"));
            foreach (var mapJson in maps)
            {
                if (string.Equals(StringField(mapJson, "id"), startMapId, StringComparison.Ordinal))
                {
                    return mapJson;
                }
            }

            return maps.Count == 0 ? string.Empty : maps[0];
        }

        private static Dictionary<string, GenericGamePackageTilePrototype> BuildTilePrototypeMap(
            string gameJson)
        {
            var map = new Dictionary<string, GenericGamePackageTilePrototype>(StringComparer.Ordinal);
            foreach (var prototypeJson in TopLevelObjectBlocks(ArrayField(gameJson, "tilePrototypes")))
            {
                var prototype = new GenericGamePackageTilePrototype
                {
                    id = StringField(prototypeJson, "id"),
                    name = StringField(prototypeJson, "name"),
                    walkable = BoolField(prototypeJson, "walkable", true)
                };
                if (!string.IsNullOrWhiteSpace(prototype.id) && !map.ContainsKey(prototype.id))
                {
                    map.Add(prototype.id, prototype);
                }
            }

            return map;
        }

        private static Dictionary<string, GenericGamePackageEntityPrototype> BuildEntityPrototypeMap(
            string gameJson)
        {
            var map = new Dictionary<string, GenericGamePackageEntityPrototype>(StringComparer.Ordinal);
            foreach (var prototypeJson in TopLevelObjectBlocks(ArrayField(gameJson, "entityPrototypes")))
            {
                var prototype = new GenericGamePackageEntityPrototype
                {
                    id = StringField(prototypeJson, "id"),
                    name = StringField(prototypeJson, "name"),
                    components = BuildComponents(ArrayField(prototypeJson, "components")).ToArray()
                };
                if (!string.IsNullOrWhiteSpace(prototype.id) && !map.ContainsKey(prototype.id))
                {
                    map.Add(prototype.id, prototype);
                }
            }

            return map;
        }

        private static Dictionary<string, GenericGamePackageTile> BuildExplicitTileMap(string mapJson)
        {
            var result = new Dictionary<string, GenericGamePackageTile>(StringComparer.Ordinal);
            foreach (var tileJson in TopLevelObjectBlocks(ArrayField(mapJson, "tiles")))
            {
                var tile = new GenericGamePackageTile
                {
                    x = IntField(tileJson, "x"),
                    y = IntField(tileJson, "y"),
                    tileId = StringField(tileJson, "tileId")
                };
                var key = CoordinateKey(tile.x, tile.y);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, tile);
                }
            }

            return result;
        }

        private static Dictionary<string, string> BuildItemNameMap(
            List<GenericGamePackageProjectionItem> items)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.ItemId) && !map.ContainsKey(item.ItemId))
                {
                    map.Add(item.ItemId, item.Name);
                }
            }

            return map;
        }

        private static GenericGamePackageProjectionEntity BuildEntityProjection(
            string entityJson,
            GenericGamePackageEntityPrototype prototype)
        {
            var positionJson = ObjectField(entityJson, "position");
            var projection = new GenericGamePackageProjectionEntity
            {
                EntityId = StringField(entityJson, "id"),
                PrototypeId = StringField(entityJson, "prototypeId"),
                PrototypeName = prototype == null
                    ? StringField(entityJson, "prototypeId")
                    : prototype.name ?? string.Empty,
                X = IntField(positionJson, "x"),
                Y = IntField(positionJson, "y")
            };

            ApplyInteraction(projection, prototype == null ? Array.Empty<GenericGamePackageComponent>() : prototype.components);
            ApplyInteraction(projection, BuildComponents(ArrayField(entityJson, "components")).ToArray());
            return projection;
        }

        private static GenericGamePackageProjectionInventory BuildInventoryProjection(
            string inventoryJson,
            Dictionary<string, string> itemNames)
        {
            var inventory = new GenericGamePackageProjectionInventory
            {
                InventoryId = StringField(inventoryJson, "id"),
                OwnerKind = StringField(inventoryJson, "ownerKind"),
                OwnerId = StringField(inventoryJson, "ownerId"),
                Slots = IntField(inventoryJson, "slots")
            };
            foreach (var stackJson in TopLevelObjectBlocks(ArrayField(inventoryJson, "stacks")))
            {
                var itemId = StringField(stackJson, "itemId");
                string itemName;
                itemNames.TryGetValue(itemId, out itemName);
                inventory.Stacks.Add(new GenericGamePackageProjectionInventoryStack
                {
                    ItemId = itemId,
                    ItemName = itemName ?? string.Empty,
                    Amount = IntField(stackJson, "amount"),
                    Durability = IntField(stackJson, "durability")
                });
            }

            return inventory;
        }

        private static GenericGamePackageProjectionQuest BuildQuestProjection(
            string questJson,
            Dictionary<string, string> itemNames)
        {
            var quest = new GenericGamePackageProjectionQuest
            {
                QuestId = StringField(questJson, "id"),
                Title = StringField(questJson, "title"),
                Description = StringField(questJson, "description")
            };
            foreach (var objectiveJson in TopLevelObjectBlocks(ArrayField(questJson, "objectives")))
            {
                var targetId = StringField(objectiveJson, "targetId");
                string targetName;
                itemNames.TryGetValue(targetId, out targetName);
                quest.Objectives.Add(new GenericGamePackageProjectionQuestObjective
                {
                    ObjectiveId = StringField(objectiveJson, "id"),
                    Kind = StringField(objectiveJson, "kind"),
                    TargetId = targetId,
                    TargetName = targetName ?? string.Empty,
                    RequiredAmount = IntField(objectiveJson, "requiredAmount")
                });
            }

            return quest;
        }

        private static GenericGamePackageProjectionDialogue BuildDialogueProjection(string dialogueJson)
        {
            var dialogue = new GenericGamePackageProjectionDialogue
            {
                DialogueId = StringField(dialogueJson, "id"),
                Title = StringField(dialogueJson, "title"),
                StartNodeId = StringField(dialogueJson, "startNodeId")
            };
            foreach (var nodeJson in TopLevelObjectBlocks(ArrayField(dialogueJson, "nodes")))
            {
                if (!string.Equals(StringField(nodeJson, "id"), dialogue.StartNodeId, StringComparison.Ordinal))
                {
                    continue;
                }

                dialogue.StartSpeakerId = StringField(nodeJson, "speakerId");
                dialogue.StartText = StringField(nodeJson, "text");
                break;
            }

            return dialogue;
        }

        private static GenericGamePackageProjectionInteraction BuildInteractionProjection(
            string interactionJson)
        {
            var interaction = new GenericGamePackageProjectionInteraction
            {
                InteractionId = StringField(interactionJson, "id"),
                Kind = StringField(interactionJson, "kind")
            };
            foreach (var effectJson in TopLevelObjectBlocks(ArrayField(interactionJson, "effects")))
            {
                var argsJson = ObjectField(effectJson, "args");
                interaction.Effects.Add(new GenericGamePackageProjectionEffect
                {
                    Type = StringField(effectJson, "type"),
                    Id = StringField(argsJson, "id"),
                    Value = StringField(argsJson, "value"),
                    Message = StringField(argsJson, "message")
                });
            }

            return interaction;
        }

        private static List<GenericGamePackageComponent> BuildComponents(string componentsJson)
        {
            var components = new List<GenericGamePackageComponent>();
            foreach (var componentJson in TopLevelObjectBlocks(componentsJson))
            {
                var argsJson = ObjectField(componentJson, "args");
                components.Add(new GenericGamePackageComponent
                {
                    type = StringField(componentJson, "type"),
                    args = string.IsNullOrWhiteSpace(argsJson)
                        ? null
                        : new GenericGamePackageComponentArgs
                        {
                            interactionId = StringField(argsJson, "interactionId"),
                            dialogueId = StringField(argsJson, "dialogueId"),
                            text = StringField(argsJson, "text")
                        }
                });
            }

            return components;
        }

        private static void ApplyInteraction(
            GenericGamePackageProjectionEntity projection,
            GenericGamePackageComponent[] components)
        {
            foreach (var component in Safe(components))
            {
                if (!string.Equals(component.type, "interactable", StringComparison.Ordinal))
                {
                    continue;
                }

                projection.Interactable = true;
                if (component.args == null)
                {
                    continue;
                }

                projection.InteractionId = FirstNonEmpty(projection.InteractionId, component.args.interactionId);
                projection.DialogueId = FirstNonEmpty(projection.DialogueId, component.args.dialogueId);
                projection.InteractionText = FirstNonEmpty(projection.InteractionText, component.args.text);
            }
        }

        private static string ClassifyTile(string tileId)
        {
            if ((tileId ?? string.Empty).IndexOf("wall", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "wall";
            }

            if ((tileId ?? string.Empty).IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "road";
            }

            return "tile";
        }

        private static string FirstNonEmpty(string current, string candidate)
        {
            return string.IsNullOrWhiteSpace(current) ? candidate ?? string.Empty : current;
        }

        private static string CoordinateKey(int x, int y)
        {
            return x + ":" + y;
        }

        private static T[] Safe<T>(T[] values)
        {
            return values ?? Array.Empty<T>();
        }

        private static string ObjectField(string json, string field)
        {
            var start = FindValueStart(json, field, '{');
            return start < 0 ? string.Empty : ExtractBalanced(json, start, '{', '}');
        }

        private static string ArrayField(string json, string field)
        {
            var start = FindValueStart(json, field, '[');
            return start < 0 ? string.Empty : ExtractBalanced(json, start, '[', ']');
        }

        private static int FindValueStart(string json, string field, char expected)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return -1;
            }

            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:");
            if (!match.Success)
            {
                return -1;
            }

            var index = match.Index + match.Length;
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }

            return index < json.Length && json[index] == expected ? index : -1;
        }

        private static List<string> TopLevelObjectBlocks(string arrayJson)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(arrayJson))
            {
                return result;
            }

            for (var index = 0; index < arrayJson.Length; index++)
            {
                if (arrayJson[index] != '{')
                {
                    continue;
                }

                var block = ExtractBalanced(arrayJson, index, '{', '}');
                if (!string.IsNullOrWhiteSpace(block))
                {
                    result.Add(block);
                    index += block.Length - 1;
                }
            }

            return result;
        }

        private static string ExtractBalanced(string text, int start, char open, char close)
        {
            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var index = start; index < text.Length; index++)
            {
                var current = text[index];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (current == '\\')
                    {
                        escaped = true;
                    }
                    else if (current == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    continue;
                }

                if (current == open)
                {
                    depth++;
                }
                else if (current == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, index - start + 1);
                    }
                }
            }

            return string.Empty;
        }

        private static string StringField(string json, string field)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                json,
                "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"",
                RegexOptions.Singleline);
            return match.Success ? UnescapeJsonString(match.Groups["value"].Value) : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return 0;
            }

            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+)");
            return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : 0;
        }

        private static bool BoolField(string json, string field, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return fallback;
            }

            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            return match.Success && bool.TryParse(match.Groups[1].Value, out var value) ? value : fallback;
        }

        private static string UnescapeJsonString(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        private static bool IsUnderRoot(string root, string path)
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}
