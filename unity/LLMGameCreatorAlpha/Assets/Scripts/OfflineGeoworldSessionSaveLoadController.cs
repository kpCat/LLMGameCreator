using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldSessionSaveLoadController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal106";
        private const string ManifestFileName = "offline-geoworld-session-manifest.json";
        private const string DeltaLogFileName = "offline-geoworld-session-delta-log.json";
        private const string ReplayScriptFileName = "offline-geoworld-session-replay-script.json";
        private const string SnapshotFileName = "offline-geoworld-goal106-session-snapshot.json";

        [SerializeField] private int replayStepCount;
        [SerializeField] private int stateDeltaCount;
        [SerializeField] private int checkpointStepIndex;
        [SerializeField] private string initialStateHash = string.Empty;
        [SerializeField] private string checkpointStateHash = string.Empty;
        [SerializeField] private string finalStateHash = string.Empty;
        [SerializeField] private string currentStateHash = string.Empty;
        [SerializeField] private string snapshotHash = string.Empty;
        [SerializeField] private string payloadRoot = RelativeRoot;
        [SerializeField] private string snapshotRelativePath = SnapshotFileName;
        [SerializeField] private string statusLine = string.Empty;

        private readonly List<OfflineGeoworldSessionSnapshotDelta> replayDeltas =
            new List<OfflineGeoworldSessionSnapshotDelta>();
        private OfflineGeoworldInteractionController interactionController;
        private OfflineGeoworldStateDeltaLog stateDeltaLog;
        private int appliedReplayStepCount;

        public int ReplayStepCount { get { return replayStepCount; } }
        public int StateDeltaCount { get { return stateDeltaCount; } }
        public int CheckpointStepIndex { get { return checkpointStepIndex; } }
        public string CurrentStateHash { get { return currentStateHash; } }
        public string FinalStateHash { get { return finalStateHash; } }
        public string LastStatus { get { return statusLine; } }

        private void Awake()
        {
            BindOptionalGoal105Components();
        }

        private void Start()
        {
            RefreshPayload();
        }

        [ContextMenu("Refresh Goal106 Session Payload")]
        public void RefreshPayload()
        {
            BindOptionalGoal105Components();
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifest = ReadFile(root, ManifestFileName, diagnostics);
            var deltaLog = ReadFile(root, DeltaLogFileName, diagnostics);
            var replayScript = ReadFile(root, ReplayScriptFileName, diagnostics);

            replayStepCount = IntField(manifest, "replayStepCount");
            stateDeltaCount = IntField(manifest, "stateDeltaCount");
            checkpointStepIndex = IntField(manifest, "checkpointStepIndex");
            initialStateHash = StringField(manifest, "initialStateHash");
            checkpointStateHash = StringField(manifest, "checkpointStateHash");
            finalStateHash = StringField(manifest, "finalStateHash");
            snapshotHash = StringField(replayScript, "snapshotHash");
            currentStateHash = initialStateHash;
            appliedReplayStepCount = 0;
            replayDeltas.Clear();

            foreach (var block in Blocks(deltaLog, "replayStepIndex"))
            {
                replayDeltas.Add(new OfflineGeoworldSessionSnapshotDelta
                {
                    ReplayStepIndex = IntField(block, "replayStepIndex"),
                    EventId = StringField(block, "eventId"),
                    TargetId = StringField(block, "targetId"),
                    ActionId = StringField(block, "actionId"),
                    ActionKind = StringField(block, "actionKind"),
                    StateHashBefore = StringField(block, "stateHashBefore"),
                    StateHashAfter = StringField(block, "stateHashAfter")
                });
            }

            if (stateDeltaLog != null)
            {
                stateDeltaLog.ClearLog(initialStateHash);
            }

            statusLine = "goal106_session_payload steps=" + replayStepCount
                         + " deltas=" + replayDeltas.Count
                         + " checkpoint=" + checkpointStepIndex
                         + " diagnostics=" + diagnostics.Count;
        }

        public bool ApplyReplayDelta(OfflineGeoworldSessionSnapshotDelta delta)
        {
            if (delta == null)
            {
                statusLine = "replay delta missing";
                return false;
            }

            if (delta.ReplayStepIndex <= appliedReplayStepCount)
            {
                statusLine = "duplicate replay step rejected step=" + delta.ReplayStepIndex;
                return false;
            }

            if (!string.Equals(delta.StateHashBefore, currentStateHash, StringComparison.Ordinal))
            {
                statusLine = "replay hash mismatch step=" + delta.ReplayStepIndex;
                return false;
            }

            currentStateHash = delta.StateHashAfter;
            appliedReplayStepCount = delta.ReplayStepIndex;
            if (stateDeltaLog != null)
            {
                stateDeltaLog.AppendDelta(new OfflineGeoworldStateDeltaLogEntry
                {
                    EventId = delta.EventId,
                    TargetId = delta.TargetId,
                    ActionId = delta.ActionId,
                    ActionKind = delta.ActionKind,
                    DeltaKind = "goal106_replay",
                    PreviousStateHash = delta.StateHashBefore,
                    DeterministicStateHash = delta.StateHashAfter
                });
            }

            statusLine = "replay applied step=" + appliedReplayStepCount + " hash=" + currentStateHash;
            return true;
        }

        [ContextMenu("Save Goal106 Session Snapshot")]
        public bool SaveSnapshot()
        {
            var snapshot = BuildSnapshot();
            var json = SnapshotToJson(snapshot);
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SnapshotPath(), json, Encoding.UTF8);
            statusLine = "snapshot saved step=" + snapshot.AppliedReplayStepCount
                         + " hash=" + snapshot.SnapshotHash;
            return true;
        }

        [ContextMenu("Load Goal106 Session Snapshot")]
        public bool LoadSnapshot()
        {
            var path = SnapshotPath();
            if (!File.Exists(path))
            {
                statusLine = "snapshot missing";
                return false;
            }

            var snapshot = SnapshotFromJson(File.ReadAllText(path, Encoding.UTF8));
            if (snapshot == null || !ValidateSnapshot(snapshot))
            {
                statusLine = "snapshot rejected";
                return false;
            }

            currentStateHash = snapshot.CurrentStateHash;
            appliedReplayStepCount = snapshot.AppliedReplayStepCount;
            if (stateDeltaLog != null)
            {
                stateDeltaLog.ClearLog(initialStateHash);
                foreach (var delta in snapshot.Deltas)
                {
                    stateDeltaLog.AppendDelta(new OfflineGeoworldStateDeltaLogEntry
                    {
                        EventId = delta.EventId,
                        TargetId = delta.TargetId,
                        ActionId = delta.ActionId,
                        ActionKind = delta.ActionKind,
                        DeltaKind = "goal106_loaded_snapshot",
                        PreviousStateHash = delta.StateHashBefore,
                        DeterministicStateHash = delta.StateHashAfter
                    });
                }
            }

            statusLine = "snapshot loaded step=" + appliedReplayStepCount + " hash=" + currentStateHash;
            return true;
        }

        [ContextMenu("Delete Goal106 Session Snapshot")]
        public void DeleteSnapshot()
        {
            var path = SnapshotPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            statusLine = "snapshot deleted";
        }

        public OfflineGeoworldSessionSnapshot BuildSnapshot()
        {
            var applied = replayDeltas.FindAll(delta => delta.ReplayStepIndex <= appliedReplayStepCount);
            var snapshot = new OfflineGeoworldSessionSnapshot
            {
                InitialStateHash = initialStateHash,
                CurrentStateHash = currentStateHash,
                FinalStateHash = finalStateHash,
                AppliedReplayStepCount = appliedReplayStepCount,
                CheckpointStepIndex = checkpointStepIndex,
                CheckpointStateHash = checkpointStateHash,
                Deltas = applied
            };
            snapshot.SnapshotHash = SnapshotHash(snapshot);
            return snapshot;
        }

        private static string SnapshotToJson(OfflineGeoworldSessionSnapshot snapshot)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            AppendJsonString(builder, "GoalId", snapshot.GoalId, true);
            AppendJsonString(builder, "InitialStateHash", snapshot.InitialStateHash, true);
            AppendJsonString(builder, "CurrentStateHash", snapshot.CurrentStateHash, true);
            AppendJsonString(builder, "FinalStateHash", snapshot.FinalStateHash, true);
            AppendJsonInt(builder, "AppliedReplayStepCount", snapshot.AppliedReplayStepCount, true);
            AppendJsonInt(builder, "CheckpointStepIndex", snapshot.CheckpointStepIndex, true);
            AppendJsonString(builder, "CheckpointStateHash", snapshot.CheckpointStateHash, true);
            AppendJsonString(builder, "SnapshotHash", snapshot.SnapshotHash, true);
            builder.AppendLine("  \"Deltas\": [");
            var deltas = snapshot.Deltas ?? new List<OfflineGeoworldSessionSnapshotDelta>();
            for (var i = 0; i < deltas.Count; i++)
            {
                var delta = deltas[i] ?? new OfflineGeoworldSessionSnapshotDelta();
                builder.AppendLine("    {");
                AppendJsonInt(builder, "ReplayStepIndex", delta.ReplayStepIndex, true, 6);
                AppendJsonString(builder, "EventId", delta.EventId, true, 6);
                AppendJsonString(builder, "TargetId", delta.TargetId, true, 6);
                AppendJsonString(builder, "ActionId", delta.ActionId, true, 6);
                AppendJsonString(builder, "ActionKind", delta.ActionKind, true, 6);
                AppendJsonString(builder, "StateHashBefore", delta.StateHashBefore, true, 6);
                AppendJsonString(builder, "StateHashAfter", delta.StateHashAfter, false, 6);
                builder.Append("    }");
                if (i + 1 < deltas.Count)
                {
                    builder.Append(',');
                }

                builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static OfflineGeoworldSessionSnapshot SnapshotFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var snapshot = new OfflineGeoworldSessionSnapshot
            {
                GoalId = StringField(json, "GoalId"),
                InitialStateHash = StringField(json, "InitialStateHash"),
                CurrentStateHash = StringField(json, "CurrentStateHash"),
                FinalStateHash = StringField(json, "FinalStateHash"),
                AppliedReplayStepCount = IntField(json, "AppliedReplayStepCount"),
                CheckpointStepIndex = IntField(json, "CheckpointStepIndex"),
                CheckpointStateHash = StringField(json, "CheckpointStateHash"),
                SnapshotHash = StringField(json, "SnapshotHash")
            };
            snapshot.Deltas.Clear();
            foreach (var block in Blocks(json, "ReplayStepIndex"))
            {
                snapshot.Deltas.Add(new OfflineGeoworldSessionSnapshotDelta
                {
                    ReplayStepIndex = IntField(block, "ReplayStepIndex"),
                    EventId = StringField(block, "EventId"),
                    TargetId = StringField(block, "TargetId"),
                    ActionId = StringField(block, "ActionId"),
                    ActionKind = StringField(block, "ActionKind"),
                    StateHashBefore = StringField(block, "StateHashBefore"),
                    StateHashAfter = StringField(block, "StateHashAfter")
                });
            }

            return snapshot;
        }

        private bool ValidateSnapshot(OfflineGeoworldSessionSnapshot snapshot)
        {
            return snapshot.AppliedReplayStepCount >= 0
                   && snapshot.AppliedReplayStepCount <= replayStepCount
                   && !string.IsNullOrWhiteSpace(snapshot.CurrentStateHash)
                   && string.Equals(snapshot.SnapshotHash, SnapshotHash(snapshot), StringComparison.Ordinal)
                   && (snapshot.AppliedReplayStepCount != checkpointStepIndex
                       || string.Equals(snapshot.CurrentStateHash, checkpointStateHash, StringComparison.Ordinal));
        }

        private void BindOptionalGoal105Components()
        {
            interactionController = GetComponent<OfflineGeoworldInteractionController>();
            stateDeltaLog = GetComponent<OfflineGeoworldStateDeltaLog>();
            if (stateDeltaLog == null)
            {
                stateDeltaLog = gameObject.AddComponent<OfflineGeoworldStateDeltaLog>();
            }
        }

        private string SnapshotPath()
        {
            return Path.Combine(Application.persistentDataPath, SnapshotFileName);
        }

        private static string SnapshotHash(OfflineGeoworldSessionSnapshot snapshot)
        {
            var seed = snapshot.InitialStateHash
                       + "|"
                       + snapshot.AppliedReplayStepCount
                       + "|"
                       + snapshot.CurrentStateHash
                       + "|"
                       + string.Join(",", snapshot.Deltas.ConvertAll(delta => delta.EventId).ToArray());
            return StableHash(seed);
        }

        private static string StableHash(string text)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var ch in text ?? string.Empty)
                {
                    hash ^= ch;
                    hash *= 16777619;
                }

                return hash.ToString("x8");
            }
        }

        private static void AppendJsonString(
            StringBuilder builder,
            string name,
            string value,
            bool trailingComma,
            int indent = 2)
        {
            builder.Append(new string(' ', indent));
            builder.Append('"').Append(name).Append("\": \"").Append(EscapeJson(value)).Append('"');
            if (trailingComma)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        private static void AppendJsonInt(
            StringBuilder builder,
            string name,
            int value,
            bool trailingComma,
            int indent = 2)
        {
            builder.Append(new string(' ', indent));
            builder.Append('"').Append(name).Append("\": ").Append(value);
            if (trailingComma)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        private static string EscapeJson(string value)
        {
            var builder = new StringBuilder();
            foreach (var ch in value ?? string.Empty)
            {
                switch (ch)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (ch < ' ')
                        {
                            builder.Append("\\u").Append(((int)ch).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(ch);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        private static string ReadFile(string root, string fileName, List<string> diagnostics)
        {
            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing:" + fileName);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static List<string> Blocks(string json, string anchorField)
        {
            var result = new List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\""
                                                                   + Regex.Escape(anchorField)
                                                                   + "\"[\\s\\S]*?\\}"))
            {
                result.Add(match.Value);
            }

            return result;
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }
    }
}
