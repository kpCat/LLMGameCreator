using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldSessionReplayController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal106";
        private const string ReplayScriptFileName = "offline-geoworld-session-replay-script.json";
        private const string DeltaLogFileName = "offline-geoworld-session-delta-log.json";

        [SerializeField] private int replayStepCount;
        [SerializeField] private int currentStepIndex;
        [SerializeField] private int checkpointStepIndex;
        [SerializeField] private string initialStateHash = string.Empty;
        [SerializeField] private string checkpointStateHash = string.Empty;
        [SerializeField] private string finalStateHash = string.Empty;
        [SerializeField] private string currentStateHash = string.Empty;
        [SerializeField] private string statusLine = string.Empty;

        private readonly List<OfflineGeoworldSessionSnapshotDelta> replaySteps =
            new List<OfflineGeoworldSessionSnapshotDelta>();
        private OfflineGeoworldSessionSaveLoadController saveLoadController;

        public int ReplayStepCount { get { return replayStepCount; } }
        public int CurrentStepIndex { get { return currentStepIndex; } }
        public string CurrentStateHash { get { return currentStateHash; } }
        public string FinalStateHash { get { return finalStateHash; } }
        public string LastStatus { get { return statusLine; } }

        private void Awake()
        {
            BindSaveLoadController();
        }

        private void Start()
        {
            RefreshReplayPayload();
        }

        [ContextMenu("Refresh Goal106 Replay Payload")]
        public void RefreshReplayPayload()
        {
            BindSaveLoadController();
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var replayJson = ReadFile(root, ReplayScriptFileName, diagnostics);
            var deltaJson = ReadFile(root, DeltaLogFileName, diagnostics);
            replayStepCount = IntField(replayJson, "replayStepCount");
            checkpointStepIndex = IntField(replayJson, "stepIndex");
            initialStateHash = StringField(replayJson, "initialStateHash");
            checkpointStateHash = StringField(replayJson, "stateHash");
            finalStateHash = StringField(replayJson, "finalStateHash");
            currentStateHash = initialStateHash;
            currentStepIndex = 0;
            replaySteps.Clear();

            foreach (var block in Blocks(deltaJson, "replayStepIndex"))
            {
                replaySteps.Add(new OfflineGeoworldSessionSnapshotDelta
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

            if (saveLoadController != null)
            {
                saveLoadController.RefreshPayload();
            }

            statusLine = "goal106_replay_payload steps=" + replaySteps.Count
                         + " checkpoint=" + checkpointStepIndex
                         + " diagnostics=" + diagnostics.Count;
        }

        [ContextMenu("Replay Goal106 Next Step")]
        public bool ReplayNextStep()
        {
            BindSaveLoadController();
            if (currentStepIndex >= replaySteps.Count)
            {
                statusLine = "replay complete hash=" + currentStateHash;
                return false;
            }

            var step = replaySteps[currentStepIndex];
            if (saveLoadController == null || !saveLoadController.ApplyReplayDelta(step))
            {
                statusLine = "replay step rejected step=" + step.ReplayStepIndex;
                return false;
            }

            currentStepIndex = step.ReplayStepIndex;
            currentStateHash = step.StateHashAfter;
            statusLine = "replayed step=" + currentStepIndex + " hash=" + currentStateHash;
            return true;
        }

        [ContextMenu("Replay Goal106 Remaining Steps")]
        public void ReplayAllRemaining()
        {
            while (currentStepIndex < replaySteps.Count)
            {
                if (!ReplayNextStep())
                {
                    break;
                }
            }

            statusLine = currentStateHash == finalStateHash
                ? "replay final hash matched"
                : "replay final hash mismatch";
        }

        [ContextMenu("Reset Goal106 Replay")]
        public void ResetReplay()
        {
            currentStepIndex = 0;
            currentStateHash = initialStateHash;
            if (saveLoadController != null)
            {
                saveLoadController.RefreshPayload();
            }

            statusLine = "replay reset";
        }

        public void SaveCheckpointSnapshot()
        {
            while (currentStepIndex < checkpointStepIndex)
            {
                if (!ReplayNextStep())
                {
                    return;
                }
            }

            if (saveLoadController != null)
            {
                saveLoadController.SaveSnapshot();
            }
        }

        public bool LoadCheckpointSnapshot()
        {
            var loaded = saveLoadController != null && saveLoadController.LoadSnapshot();
            if (loaded)
            {
                currentStepIndex = checkpointStepIndex;
                currentStateHash = checkpointStateHash;
            }

            statusLine = loaded ? "checkpoint snapshot loaded" : "checkpoint snapshot rejected";
            return loaded;
        }

        private void BindSaveLoadController()
        {
            saveLoadController = GetComponent<OfflineGeoworldSessionSaveLoadController>();
            if (saveLoadController == null)
            {
                saveLoadController = gameObject.AddComponent<OfflineGeoworldSessionSaveLoadController>();
            }
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
