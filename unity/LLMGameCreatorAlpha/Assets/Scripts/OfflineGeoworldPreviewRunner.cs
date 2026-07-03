using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPreviewRunner : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal101";
        private const string ManifestSchema = "offline_geoworld_preview_runner_manifest_v1";

        [SerializeField] private bool runOnStart = true;
        [SerializeField] private string lastStatus = string.Empty;
        [SerializeField] private int lastCommandCount;
        [SerializeField] private int lastTravelWindowStepCount;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private string travelWindowJson = string.Empty;

        public PreviewResult LastResult { get; private set; } = new PreviewResult();
        public string LastStatus { get { return lastStatus; } }
        public int LastCommandCount { get { return lastCommandCount; } }
        public int LastTravelWindowStepCount { get { return lastTravelWindowStepCount; } }

        private void Start()
        {
            if (runOnStart)
            {
                Refresh();
            }
        }

        [ContextMenu("Refresh Goal101 Offline Geoworld Preview")]
        public void Refresh()
        {
            LastResult = RunPreview();
            lastStatus = LastResult.ToStatusLine();
            lastCommandCount = LastResult.CommandCount;
            lastTravelWindowStepCount = LastResult.TravelWindowStepCount;
            Debug.Log(lastStatus);
        }

        public PreviewResult RunPreview()
        {
            ClearSpawned();
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var payload = ReadPayload(root, diagnostics);
            var manifest = Get(payload, "offline-geoworld-preview-runner-manifest.json");
            var commandsJson = Get(payload, "offline-geoworld-preview-feature-commands.json");
            travelWindowJson = Get(payload, "offline-geoworld-preview-travel-window-script.json");
            var styleJson = Get(payload, "offline-geoworld-preview-style-legend.json");
            var readmeJson = Get(payload, "offline-geoworld-preview-readme.json");

            var commands = ReadCommands(commandsJson);
            foreach (var command in commands)
            {
                spawnedObjects.Add(
                    OfflineGeoworldPreviewPrimitiveFactory.CreatePlaceholder(transform, command, styleJson));
            }

            var travel = GetComponent<OfflineGeoworldPreviewTravelWindow>();
            if (travel == null)
            {
                travel = gameObject.AddComponent<OfflineGeoworldPreviewTravelWindow>();
            }

            travel.LoadScript(travelWindowJson);
            travel.ApplyStep(0, travelWindowJson, spawnedObjects);

            var schemaMatches = StringField(manifest, "schemaVersion") == ManifestSchema;
            var hashesMatch = StringField(manifest, "featureCommandsHash") == Sha256(commandsJson)
                              && StringField(manifest, "travelWindowScriptHash") == Sha256(travelWindowJson)
                              && StringField(manifest, "styleLegendHash") == Sha256(styleJson)
                              && StringField(manifest, "readmeHash") == Sha256(readmeJson);
            var commandCount = IntField(manifest, "commandCount");
            var commandKindCount = IntField(manifest, "commandKindCount");
            var travelSteps = IntField(manifest, "travelWindowStepCount");
            var styleCount = IntField(manifest, "styleCount");
            var countsMatch = commandCount == commands.Count
                              && commandCount == 18
                              && commandKindCount == 10
                              && travelSteps == travel.StepCount
                              && styleCount == 10;
            var metadataOnly = BoolField(manifest, "metadataOnly")
                               && !BoolField(manifest, "containsRuntimeExecution")
                               && !BoolField(manifest, "containsProviderCalls")
                               && !BoolField(manifest, "containsFinalArt")
                               && !BoolField(readmeJson, "implementsRuntimeConsumption")
                               && !BoolField(readmeJson, "implementsFinalArt")
                               && !BoolField(readmeJson, "implementsSceneOrPrefabProduction");
            var safeFlags = BoolField(manifest, "noRawGeodata")
                            && BoolField(manifest, "noAbsolutePaths")
                            && BoolField(manifest, "noBinaryOrRasterMedia")
                            && BoolField(manifest, "noProviderOrNetworkMarkers");

            AddIfFalse(diagnostics, schemaMatches, "schema");
            AddIfFalse(diagnostics, payload.Count == 5, "files");
            AddIfFalse(diagnostics, hashesMatch, "hashes");
            AddIfFalse(diagnostics, countsMatch, "counts");
            AddIfFalse(diagnostics, metadataOnly, "metadata-only");
            AddIfFalse(diagnostics, safeFlags, "safe-flags");

            return new PreviewResult
            {
                Passed = diagnostics.Count == 0,
                PayloadRoot = RelativeRoot,
                PayloadFileCount = payload.Count,
                CommandCount = commandCount,
                CommandKindCount = commandKindCount,
                TravelWindowStepCount = travelSteps,
                SpawnedObjectCount = spawnedObjects.Count,
                MetadataOnly = metadataOnly,
                Diagnostics = diagnostics.ToArray()
            };
        }

        public void NextTravelStep()
        {
            var travel = GetComponent<OfflineGeoworldPreviewTravelWindow>();
            if (travel != null)
            {
                travel.Next(travelWindowJson, spawnedObjects);
                lastTravelWindowStepCount = travel.StepCount;
            }
        }

        private void ClearSpawned()
        {
            for (var i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedObjects[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(spawnedObjects[i]);
                    }
                    else
                    {
                        DestroyImmediate(spawnedObjects[i]);
                    }
                }
            }

            spawnedObjects.Clear();
        }

        private static Dictionary<string, string> ReadPayload(string root, List<string> diagnostics)
        {
            var files = new[]
            {
                "offline-geoworld-preview-runner-manifest.json",
                "offline-geoworld-preview-feature-commands.json",
                "offline-geoworld-preview-travel-window-script.json",
                "offline-geoworld-preview-style-legend.json",
                "offline-geoworld-preview-readme.json"
            };
            var payload = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in files)
            {
                var path = Path.Combine(root, file);
                if (!File.Exists(path))
                {
                    diagnostics.Add("missing:" + file);
                    continue;
                }

                payload[file] = File.ReadAllText(path, Encoding.UTF8);
            }

            return payload;
        }

        private static List<OfflineGeoworldPreviewCommand> ReadCommands(string json)
        {
            var commands = new List<OfflineGeoworldPreviewCommand>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\"commandId\"[\\s\\S]*?\\}"))
            {
                var block = match.Value;
                commands.Add(new OfflineGeoworldPreviewCommand
                {
                    CommandId = StringField(block, "commandId"),
                    CommandKind = StringField(block, "commandKind"),
                    StyleKey = StringField(block, "styleKey"),
                    GridX = IntField(block, "gridX"),
                    GridZ = IntField(block, "gridZ"),
                    Elevation = IntField(block, "elevation")
                });
            }

            return commands;
        }

        private static string Get(Dictionary<string, string> payload, string key)
        {
            string value;
            return payload.TryGetValue(key, out value) ? value : string.Empty;
        }

        private static void AddIfFalse(List<string> diagnostics, bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add("mismatch:" + code);
            }
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        private static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            bool value;
            return match.Success && bool.TryParse(match.Groups[1].Value, out value) && value;
        }

        private static string Sha256(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        [Serializable]
        public sealed class PreviewResult
        {
            public bool Passed;
            public string PayloadRoot = string.Empty;
            public int PayloadFileCount;
            public int CommandCount;
            public int CommandKindCount;
            public int TravelWindowStepCount;
            public int SpawnedObjectCount;
            public bool MetadataOnly;
            public string[] Diagnostics = new string[0];

            public string ToStatusLine()
            {
                return "goal101_offline_geoworld_preview_runner passed=" + Passed
                       + " files=" + PayloadFileCount
                       + " commands=" + CommandCount
                       + " kinds=" + CommandKindCount
                       + " travelSteps=" + TravelWindowStepCount
                       + " spawned=" + SpawnedObjectCount;
            }
        }
    }
}
