using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class VisualChunkCacheHandoffProbe : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/VisualChunkCacheGoal095";
        private const string ManifestSchema = "visual_chunk_cache_unity_handoff_manifest_v1";

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

        [ContextMenu("Refresh Goal095 Visual Chunk Cache Handoff Probe")]
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
                "visual-chunk-cache-unity-handoff-manifest.json",
                "visual-chunk-cache-package-index.json",
                "visual-chunk-cache-stream-window-index.json",
                "visual-chunk-cache-chunk-key-ledger.json",
                "visual-chunk-cache-runtime-readme.json"
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

            var manifest = Get(payload, "visual-chunk-cache-unity-handoff-manifest.json");
            var packageIndex = Get(payload, "visual-chunk-cache-package-index.json");
            var streamIndex = Get(payload, "visual-chunk-cache-stream-window-index.json");
            var chunkLedger = Get(payload, "visual-chunk-cache-chunk-key-ledger.json");
            var runtimeReadme = Get(payload, "visual-chunk-cache-runtime-readme.json");
            var schemaMatches = StringField(manifest, "schemaVersion") == ManifestSchema;
            var requiredFilesPresent = payload.Count == files.Length;
            var packageIndexHashMatches = StringField(manifest, "packageIndexHash") == Sha256(packageIndex);
            var streamIndexHashMatches = StringField(manifest, "streamWindowIndexHash") == Sha256(streamIndex);
            var chunkLedgerHashMatches = StringField(manifest, "chunkKeyLedgerHash") == Sha256(chunkLedger);
            var readmeHashMatches = StringField(manifest, "runtimeReadmeHash") == Sha256(runtimeReadme);
            var packageCount = IntField(manifest, "packageCount");
            var exportRecordCount = IntField(manifest, "exportRecordCount");
            var streamWindowCount = IntField(manifest, "streamWindowCount");
            var chunkKeyCount = IntField(manifest, "uniqueChunkKeyCount");
            var countsMatch = packageCount == 4
                              && packageCount == IntField(packageIndex, "packageCount")
                              && exportRecordCount == 93
                              && exportRecordCount == IntField(packageIndex, "exportRecordCount")
                              && exportRecordCount == IntField(chunkLedger, "exportRecordCount")
                              && streamWindowCount == 5
                              && streamWindowCount == IntField(streamIndex, "streamWindowCount")
                              && chunkKeyCount == 93
                              && chunkKeyCount == IntField(chunkLedger, "uniqueChunkKeyCount");
            var metadataOnly = BoolField(manifest, "runtimeHandoffSidecarMetadataOnly")
                               && !BoolField(manifest, "containsRuntimeExecution")
                               && !BoolField(manifest, "containsProviderCalls")
                               && !BoolField(manifest, "containsUnityGameplayImplementation");
            var safeFlags = BoolField(manifest, "noRawFullWorldDump")
                            && BoolField(chunkLedger, "noRawFullWorldDump")
                            && BoolField(manifest, "noAbsolutePaths")
                            && BoolField(manifest, "noBinaryOrRasterMedia")
                            && BoolField(manifest, "noPromptDumps");
            var noRuntimeImplementation = !BoolField(runtimeReadme, "implementsRuntimeConsumption")
                                          && !BoolField(runtimeReadme, "implementsLiveUnityRendering")
                                          && !BoolField(runtimeReadme, "implementsFinalAtlas")
                                          && !BoolField(runtimeReadme, "implementsRuntimeStreaming");

            AddIfFalse(diagnostics, schemaMatches, "schema");
            AddIfFalse(diagnostics, requiredFilesPresent, "files");
            AddIfFalse(diagnostics, packageIndexHashMatches, "package-index-hash");
            AddIfFalse(diagnostics, streamIndexHashMatches, "stream-window-index-hash");
            AddIfFalse(diagnostics, chunkLedgerHashMatches, "chunk-key-ledger-hash");
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
                ExportRecordCount = exportRecordCount,
                StreamWindowCount = streamWindowCount,
                UniqueChunkKeyCount = chunkKeyCount,
                RuntimeHandoffMetadataOnly = metadataOnly,
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
            public int ExportRecordCount;
            public int StreamWindowCount;
            public int UniqueChunkKeyCount;
            public bool RuntimeHandoffMetadataOnly;
            public string[] Diagnostics = new string[0];

            public string ToStatusLine()
            {
                return "goal095_visual_chunk_cache_probe passed=" + Passed
                       + " files=" + PayloadFileCount
                       + " packages=" + PackageCount
                       + " records=" + ExportRecordCount
                       + " windows=" + StreamWindowCount
                       + " chunkKeys=" + UniqueChunkKeyCount;
            }
        }
    }
}
