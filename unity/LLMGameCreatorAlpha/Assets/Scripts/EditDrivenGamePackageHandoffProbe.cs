using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class EditDrivenGamePackageHandoffProbe : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/EditDrivenGoal082";

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

        [ContextMenu("Refresh Goal082 Handoff Probe")]
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
                "handoff-manifest.json",
                "projected-package-index.json",
                "playthrough-command-index.json",
                "playthrough-transcript-index.json",
                "expected-hashes.json",
                "README.md"
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

            var manifest = Get(payload, "handoff-manifest.json");
            var expected = Get(payload, "expected-hashes.json");
            var packageIndex = Get(payload, "projected-package-index.json");
            var commandIndex = Get(payload, "playthrough-command-index.json");
            var transcriptIndex = Get(payload, "playthrough-transcript-index.json");
            var expectedHashMatches = StringField(manifest, "expectedHashesHash") == Sha256(expected);
            var packageIndexHashMatches = StringField(expected, "projectedPackageIndexPayloadHash") == Sha256(packageIndex);
            var commandIndexHashMatches = StringField(expected, "playthroughCommandIndexPayloadHash") == Sha256(commandIndex);
            var transcriptIndexHashMatches = StringField(expected, "playthroughTranscriptIndexPayloadHash") == Sha256(transcriptIndex);
            var packageHash = StringField(expected, "projectedPackageHash");
            var packageHashMatches = packageHash.Length == 64
                                     && packageHash == StringField(manifest, "projectedPackageHash")
                                     && packageHash == StringField(packageIndex, "projectedPackageHash");
            var commandHashMatches = StringField(expected, "goal081CommandScriptHash")
                                     == StringField(commandIndex, "commandScriptHash");
            var transcriptHashMatches = StringField(expected, "goal081TranscriptHash")
                                        == StringField(transcriptIndex, "transcriptHash");
            var stateHashMatches = StringField(expected, "goal081StateHashChainHash")
                                   == StringField(transcriptIndex, "stateHashChainHash")
                                   && StringField(expected, "finalCoverageStateHash")
                                   == StringField(transcriptIndex, "finalCoverageStateHash")
                                   && StringField(expected, "replayFinalStateHash")
                                   == StringField(transcriptIndex, "replayFinalStateHash");
            var rowCount = IntField(manifest, "rowCount");
            var targetCount = IntField(manifest, "targetCount");
            var actionCount = IntField(manifest, "goal078ActionCount");
            var commandCount = IntField(manifest, "commandCount");
            var countsMatch = rowCount == IntField(expected, "rowCount")
                              && rowCount == IntField(packageIndex, "rowCount")
                              && rowCount == IntField(commandIndex, "rowCount")
                              && rowCount == IntField(transcriptIndex, "coveredRowCount")
                              && targetCount == IntField(expected, "targetCount")
                              && targetCount == IntField(packageIndex, "targetCount")
                              && targetCount == IntField(commandIndex, "targetCount")
                              && targetCount == IntField(transcriptIndex, "coveredTargetCount")
                              && actionCount == IntField(expected, "goal078ActionCount")
                              && actionCount == IntField(packageIndex, "actionCount")
                              && actionCount == IntField(commandIndex, "goal078ActionCount")
                              && actionCount == IntField(transcriptIndex, "coveredGoal078ActionCount")
                              && commandCount == IntField(expected, "commandCount")
                              && commandCount == IntField(commandIndex, "commandCount");

            AddIfFalse(diagnostics, expectedHashMatches, "expected-hashes");
            AddIfFalse(diagnostics, packageIndexHashMatches, "package-index-hash");
            AddIfFalse(diagnostics, commandIndexHashMatches, "command-index-hash");
            AddIfFalse(diagnostics, transcriptIndexHashMatches, "transcript-index-hash");
            AddIfFalse(diagnostics, packageHashMatches, "package-hash");
            AddIfFalse(diagnostics, commandHashMatches, "command-hash");
            AddIfFalse(diagnostics, transcriptHashMatches, "transcript-hash");
            AddIfFalse(diagnostics, stateHashMatches, "state-hash");
            AddIfFalse(diagnostics, countsMatch, "counts");

            return new ProbeResult
            {
                Passed = diagnostics.Count == 0,
                PayloadRoot = RelativeRoot,
                PayloadFileCount = payload.Count,
                RowCount = rowCount,
                TargetCount = targetCount,
                Goal078ActionCount = actionCount,
                CommandCount = commandCount,
                ProjectedPackageHash = packageHash,
                FinalCoverageStateHash = StringField(expected, "finalCoverageStateHash"),
                ReplayFinalStateHash = StringField(expected, "replayFinalStateHash"),
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
            public int RowCount;
            public int TargetCount;
            public int Goal078ActionCount;
            public int CommandCount;
            public string ProjectedPackageHash = string.Empty;
            public string FinalCoverageStateHash = string.Empty;
            public string ReplayFinalStateHash = string.Empty;
            public string[] Diagnostics = new string[0];

            public string ToStatusLine()
            {
                return "goal082_streamingassets_probe passed=" + Passed
                       + " files=" + PayloadFileCount
                       + " rows=" + RowCount
                       + " targets=" + TargetCount
                       + " actions=" + Goal078ActionCount
                       + " commands=" + CommandCount;
            }
        }
    }
}
