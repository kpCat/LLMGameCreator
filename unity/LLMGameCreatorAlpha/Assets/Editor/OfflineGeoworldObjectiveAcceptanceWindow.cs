#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldObjectiveAcceptanceWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Objective Acceptance";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal107";
        private const string RigRootName = "__LLMGC_OfflineGeoworldObjectiveAcceptance__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int objectiveCount;
        private int completedObjectiveCount;
        private int replayStepCount;
        private int stateDeltaCount;
        private int checkpointStepIndex;
        private string finalStatus = string.Empty;
        private string finalStateHash = string.Empty;
        private string objectiveAcceptanceHash = string.Empty;
        private string acceptanceInstructionText = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldObjectiveAcceptanceWindow>("Offline Geoworld Objective Acceptance");
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
            EditorGUILayout.LabelField("Objectives", objectiveCount.ToString());
            EditorGUILayout.LabelField("Completed Objectives", completedObjectiveCount.ToString());
            EditorGUILayout.LabelField("Final Status", finalStatus);
            EditorGUILayout.LabelField("Replay Steps", replayStepCount.ToString());
            EditorGUILayout.LabelField("State Deltas", stateDeltaCount.ToString());
            EditorGUILayout.LabelField("Checkpoint Step", checkpointStepIndex.ToString());
            EditorGUILayout.LabelField("Final State Hash", finalStateHash);
            EditorGUILayout.LabelField("Objective Acceptance Hash", objectiveAcceptanceHash);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("acceptance instructions");
            EditorGUILayout.TextArea(acceptanceInstructionText, GUILayout.MinHeight(150));
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Objective Acceptance Rig"))
            {
                CreateObjectiveAcceptanceRig();
            }

            if (GUILayout.Button("Clear Objective Acceptance Rig"))
            {
                ClearObjectiveAcceptanceRig();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-objective-manifest.json");
            var completion = ReadPayloadFile("offline-geoworld-objective-completion-state.json");
            var readme = ReadPayloadFile("offline-geoworld-objective-readme.json");
            objectiveCount = IntField(manifest, "objectiveCount");
            replayStepCount = IntField(manifest, "replayStepCount");
            stateDeltaCount = IntField(manifest, "stateDeltaCount");
            checkpointStepIndex = IntField(manifest, "checkpointStepIndex");
            finalStateHash = StringField(manifest, "finalStateHash");
            objectiveAcceptanceHash = StringField(manifest, "objectiveAcceptanceHash");
            completedObjectiveCount = IntField(completion, "completedObjectiveCount");
            finalStatus = StringField(completion, "finalStatus");
            acceptanceInstructionText = ChecklistText(readme);
            status = objectiveCount >= 6
                     && completedObjectiveCount == objectiveCount
                     && replayStepCount >= 6
                     && stateDeltaCount >= 6
                ? "Goal107 objective acceptance payload ready"
                : "Goal107 objective acceptance payload missing or incomplete";
        }

        public void CreateObjectiveAcceptanceRig()
        {
            ClearObjectiveAcceptanceRig();
            var root = new GameObject(RigRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Objective Acceptance Rig");
            root.AddComponent<OfflineGeoworldStateDeltaLog>();
            root.AddComponent<OfflineGeoworldInteractionController>();
            root.AddComponent<OfflineGeoworldSessionSaveLoadController>();
            root.AddComponent<OfflineGeoworldSessionReplayController>();
            root.AddComponent<OfflineGeoworldObjectiveTracker>();
            var acceptance = root.AddComponent<OfflineGeoworldObjectiveAcceptanceController>();
            acceptance.RefreshPayloadStatus();
            status = "Created objective acceptance rig";
        }

        public void ClearObjectiveAcceptanceRig()
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
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\"manualAcceptanceInstructions\"\\s*:\\s*\\[([^\\]]*)\\]"))
            {
                foreach (Match item in Regex.Matches(match.Groups[1].Value, "\"([^\"]*)\""))
                {
                    lines.Add("- " + item.Groups[1].Value);
                }
            }

            return lines.Count == 0 ? "acceptance instructions missing" : string.Join("\n", lines.ToArray());
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
