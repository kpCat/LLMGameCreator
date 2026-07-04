#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldInteractionProbeWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Interaction Probe";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal105";
        private const string RigRootName = "__LLMGC_OfflineGeoworldInteractionProbe__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int targetCount;
        private int actionKindCount;
        private int scriptedEventCount;
        private int stateDeltaCount;
        private string finalStateHash = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldInteractionProbeWindow>("Offline Geoworld Interaction Probe");
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
            EditorGUILayout.LabelField("Targets", targetCount.ToString());
            EditorGUILayout.LabelField("Action Kinds", actionKindCount.ToString());
            EditorGUILayout.LabelField("Scripted Events", scriptedEventCount.ToString());
            EditorGUILayout.LabelField("State Deltas", stateDeltaCount.ToString());
            EditorGUILayout.LabelField("Final State Hash", finalStateHash);
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Interaction Probe Rig"))
            {
                CreateInteractionProbeRig();
            }

            if (GUILayout.Button("Clear Interaction Probe Rig"))
            {
                ClearInteractionProbeRig();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-interaction-manifest.json");
            targetCount = IntField(manifest, "targetCount");
            actionKindCount = IntField(manifest, "actionKindCount");
            scriptedEventCount = IntField(manifest, "scriptedEventCount");
            stateDeltaCount = IntField(manifest, "stateDeltaCount");
            finalStateHash = StringField(manifest, "finalStateHash");
            status = targetCount >= 8 && actionKindCount >= 5 && scriptedEventCount >= 6
                ? "Goal105 interaction payload ready"
                : "Goal105 interaction payload missing or incomplete";
        }

        public void CreateInteractionProbeRig()
        {
            ClearInteractionProbeRig();
            var root = new GameObject(RigRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Interaction Probe Rig");
            root.AddComponent<OfflineGeoworldStateDeltaLog>();
            var controller = root.AddComponent<OfflineGeoworldInteractionController>();
            controller.RefreshPayload();
            status = "Created interaction probe rig";
        }

        public void ClearInteractionProbeRig()
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
