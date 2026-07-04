#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPlayModeTravelWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Play Mode Travel";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal103";
        private const string ControllerRootName = "__LLMGC_OfflineGeoworldPlayModeTravel__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int stepCount;
        private int objectCount;
        private int maxActiveChunkCount;
        private int maxBoundaryPrefetchChunkCount;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldPlayModeTravelWindow>("Offline Geoworld Play Mode Travel");
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
            EditorGUILayout.LabelField("Travel Steps", stepCount.ToString());
            EditorGUILayout.LabelField("Objects", objectCount.ToString());
            EditorGUILayout.LabelField("Max Active Chunks", maxActiveChunkCount.ToString());
            EditorGUILayout.LabelField("Max Boundary Prefetch", maxBoundaryPrefetchChunkCount.ToString());
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Controller"))
            {
                CreateController();
            }

            if (GUILayout.Button("Clear Controller"))
            {
                ClearController();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-playmode-travel-manifest.json");
            stepCount = IntField(manifest, "stepCount");
            objectCount = IntField(manifest, "objectCount");
            maxActiveChunkCount = IntField(manifest, "maxActiveChunkCount");
            maxBoundaryPrefetchChunkCount = IntField(manifest, "maxBoundaryPrefetchChunkCount");
            status = stepCount >= 4 && objectCount > 0
                ? "Goal103 play-mode travel payload ready"
                : "Goal103 play-mode travel payload missing or incomplete";
        }

        public void CreateController()
        {
            ClearController();
            var root = new GameObject(ControllerRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Play Mode Travel Controller");
            var controller = root.AddComponent<OfflineGeoworldPlayModeTravelController>();
            controller.RefreshPayload();
            status = "Created play-mode travel controller";
        }

        public void ClearController()
        {
            var root = GameObject.Find(ControllerRootName);
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
    }
}
#endif
