#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldSessionReplayWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Session Replay";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal106";
        private const string RigRootName = "__LLMGC_OfflineGeoworldSessionReplay__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int replayStepCount;
        private int stateDeltaCount;
        private int checkpointStepIndex;
        private int acceptanceChecklistStepCount;
        private string checkpointStateHash = string.Empty;
        private string finalStateHash = string.Empty;
        private string acceptanceChecklistText = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldSessionReplayWindow>("Offline Geoworld Session Replay");
        }

        private void OnEnable()
        {
            RefreshPayloadStatus();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Payload", payloadPath);
            EditorGUILayout.LabelField("Status", status);
            EditorGUILayout.LabelField("Replay Steps", replayStepCount.ToString());
            EditorGUILayout.LabelField("State Deltas", stateDeltaCount.ToString());
            EditorGUILayout.LabelField("Checkpoint Step", checkpointStepIndex.ToString());
            EditorGUILayout.LabelField("Checkpoint Hash", checkpointStateHash);
            EditorGUILayout.LabelField("Final State Hash", finalStateHash);
            EditorGUILayout.LabelField("Acceptance Checklist Steps", acceptanceChecklistStepCount.ToString());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manual acceptance checklist");
            EditorGUILayout.TextArea(acceptanceChecklistText, GUILayout.MinHeight(140));
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Session Replay Rig"))
            {
                CreateSessionReplayRig();
            }

            if (GUILayout.Button("Clear Session Replay Rig"))
            {
                ClearSessionReplayRig();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-session-manifest.json");
            var checklist = ReadPayloadFile("offline-geoworld-session-acceptance-checklist.json");
            replayStepCount = IntField(manifest, "replayStepCount");
            stateDeltaCount = IntField(manifest, "stateDeltaCount");
            checkpointStepIndex = IntField(manifest, "checkpointStepIndex");
            checkpointStateHash = StringField(manifest, "checkpointStateHash");
            finalStateHash = StringField(manifest, "finalStateHash");
            acceptanceChecklistStepCount = IntField(checklist, "stepCount");
            acceptanceChecklistText = ChecklistText(checklist);
            status = replayStepCount >= 6 && stateDeltaCount >= 6 && checkpointStepIndex >= 3
                ? "Goal106 session replay payload ready"
                : "Goal106 session replay payload missing or incomplete";
        }

        public void CreateSessionReplayRig()
        {
            ClearSessionReplayRig();
            var root = new GameObject(RigRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Session Replay Rig");
            root.AddComponent<OfflineGeoworldStateDeltaLog>();
            root.AddComponent<OfflineGeoworldInteractionController>();
            var saveLoad = root.AddComponent<OfflineGeoworldSessionSaveLoadController>();
            var replay = root.AddComponent<OfflineGeoworldSessionReplayController>();
            saveLoad.RefreshPayload();
            replay.RefreshReplayPayload();
            status = "Created session replay rig";
        }

        public void ClearSessionReplayRig()
        {
            var root = GameObject.Find(RigRootName);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }

        private static string ReadPayloadFile(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, RelativeRoot, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string ChecklistText(string json)
        {
            var lines = new System.Collections.Generic.List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\"instruction\"\\s*:\\s*\"([^\"]*)\""))
            {
                lines.Add("- " + match.Groups[1].Value);
            }

            return lines.Count == 0 ? "acceptance checklist missing" : string.Join("\n", lines.ToArray());
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
#endif
