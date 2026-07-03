using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldHandoffProbe : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal100";
        private const string ManifestSchema = "offline_geoworld_unity_handoff_manifest_v1";

        [SerializeField] private bool runOnStart = true;
        [SerializeField] private string lastStatus = string.Empty;

        public ProbeResult LastResult { get; private set; } = new ProbeResult();
        public string LastStatus { get { return lastStatus; } }

        private void Start()
        {
            if (runOnStart)
            {
                Refresh();
            }
        }

        [ContextMenu("Refresh Goal100 Offline Geoworld Handoff Probe")]
        public void Refresh()
        {
            LastResult = RunProbe();
            lastStatus = LastResult.ToStatusLine();
            Debug.Log(lastStatus);
        }

        public static ProbeResult RunProbe()
        {
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var files = new[]
            {
                "offline-geoworld-unity-handoff-manifest.json",
                "offline-geoworld-package-index.json",
                "offline-geoworld-feature-chunk-ledger.json",
                "offline-geoworld-stream-window-index.json",
                "offline-geoworld-runtime-readme.json"
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

            var manifest = Get(payload, "offline-geoworld-unity-handoff-manifest.json");
            var packageIndex = Get(payload, "offline-geoworld-package-index.json");
            var ledger = Get(payload, "offline-geoworld-feature-chunk-ledger.json");
            var streamIndex = Get(payload, "offline-geoworld-stream-window-index.json");
            var readme = Get(payload, "offline-geoworld-runtime-readme.json");
            var schemaMatches = StringField(manifest, "schemaVersion") == ManifestSchema;
            var requiredFilesPresent = payload.Count == files.Length;
            var packageIndexHashMatches = StringField(manifest, "packageIndexHash") == Sha256(packageIndex);
            var ledgerHashMatches = StringField(manifest, "featureChunkLedgerHash") == Sha256(ledger);
            var streamIndexHashMatches = StringField(manifest, "streamWindowIndexHash") == Sha256(streamIndex);
            var readmeHashMatches = StringField(manifest, "runtimeReadmeHash") == Sha256(readme);
            var packageCount = IntField(manifest, "packageCount");
            var featureCount = IntField(manifest, "featureCount");
            var kindCount = IntField(manifest, "featureKindCount");
            var recordCount = IntField(manifest, "visualCacheRecordCount");
            var sourceChunkCount = IntField(manifest, "sourceChunkCount");
            var streamWindowCount = IntField(manifest, "streamWindowChunkCount");
            var countsMatch = packageCount == 3
                              && packageCount == IntField(packageIndex, "packageCount")
                              && featureCount == 10
                              && featureCount == IntField(packageIndex, "featureCount")
                              && featureCount == IntField(ledger, "featureCount")
                              && kindCount == 10
                              && recordCount == 18
                              && recordCount == IntField(packageIndex, "visualCacheRecordCount")
                              && recordCount == IntField(ledger, "visualCacheRecordCount")
                              && sourceChunkCount == 5
                              && sourceChunkCount == IntField(ledger, "sourceChunkCount")
                              && streamWindowCount == 9
                              && streamWindowCount == IntField(streamIndex, "requiredChunkCount");
            var metadataOnly = BoolField(manifest, "metadataOnly")
                               && !BoolField(manifest, "containsRuntimeExecution")
                               && !BoolField(manifest, "containsProviderCalls")
                               && !BoolField(manifest, "containsUnityGameplayImplementation");
            var safeFlags = BoolField(manifest, "noRawGeodata")
                            && BoolField(manifest, "noRawFullWorldDump")
                            && BoolField(manifest, "noAbsolutePaths")
                            && BoolField(manifest, "noBinaryOrRasterMedia")
                            && BoolField(manifest, "noProviderOrNetworkMarkers");
            var noRuntimeImplementation = !BoolField(readme, "implementsRuntimeConsumption")
                                          && !BoolField(readme, "implementsLiveUnityRendering")
                                          && !BoolField(readme, "implementsNetworkProvider")
                                          && !BoolField(readme, "implementsRawGeodataImport");

            AddIfFalse(diagnostics, schemaMatches, "schema");
            AddIfFalse(diagnostics, requiredFilesPresent, "files");
            AddIfFalse(diagnostics, packageIndexHashMatches, "package-index-hash");
            AddIfFalse(diagnostics, ledgerHashMatches, "feature-chunk-ledger-hash");
            AddIfFalse(diagnostics, streamIndexHashMatches, "stream-window-index-hash");
            AddIfFalse(diagnostics, readmeHashMatches, "runtime-readme-hash");
            AddIfFalse(diagnostics, countsMatch, "counts");
            AddIfFalse(diagnostics, metadataOnly, "metadata-only");
            AddIfFalse(diagnostics, safeFlags, "safe-flags");
            AddIfFalse(diagnostics, noRuntimeImplementation, "runtime-nongoals");

            return new ProbeResult
            {
                Passed = diagnostics.Count == 0,
                PayloadRoot = RelativeRoot,
                PayloadFileCount = payload.Count,
                PackageCount = packageCount,
                FeatureCount = featureCount,
                FeatureKindCount = kindCount,
                VisualCacheRecordCount = recordCount,
                SourceChunkCount = sourceChunkCount,
                StreamWindowChunkCount = streamWindowCount,
                MetadataOnly = metadataOnly,
                Diagnostics = diagnostics.ToArray()
            };
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
            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        private static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
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
        public sealed class ProbeResult
        {
            public bool Passed;
            public string PayloadRoot = string.Empty;
            public int PayloadFileCount;
            public int PackageCount;
            public int FeatureCount;
            public int FeatureKindCount;
            public int VisualCacheRecordCount;
            public int SourceChunkCount;
            public int StreamWindowChunkCount;
            public bool MetadataOnly;
            public string[] Diagnostics = new string[0];

            public string ToStatusLine()
            {
                return "goal100_offline_geoworld_probe passed=" + Passed
                       + " files=" + PayloadFileCount
                       + " packages=" + PackageCount
                       + " features=" + FeatureCount
                       + " records=" + VisualCacheRecordCount
                       + " chunks=" + SourceChunkCount
                       + " windows=" + StreamWindowChunkCount;
            }
        }
    }
}
