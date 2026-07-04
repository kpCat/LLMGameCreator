#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldInteractiveTravelWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Interactive Travel";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal104";
        private const string RigRootName = "__LLMGC_OfflineGeoworldInteractiveTravel__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int movementSampleCount;
        private int boundaryCrossingCount;
        private int objectCount;
        private int maxActiveChunkCount;
        private int maxBoundaryPrefetchChunkCount;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldInteractiveTravelWindow>("Offline Geoworld Interactive Travel");
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
            EditorGUILayout.LabelField("Movement Samples", movementSampleCount.ToString());
            EditorGUILayout.LabelField("Boundary Crossings", boundaryCrossingCount.ToString());
            EditorGUILayout.LabelField("Objects", objectCount.ToString());
            EditorGUILayout.LabelField("Max Active Chunks", maxActiveChunkCount.ToString());
            EditorGUILayout.LabelField("Max Boundary Prefetch", maxBoundaryPrefetchChunkCount.ToString());
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Preview Rig"))
            {
                CreatePreviewRig();
            }

            if (GUILayout.Button("Clear Preview Rig"))
            {
                ClearPreviewRig();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-interactive-travel-manifest.json");
            movementSampleCount = IntField(manifest, "movementSampleCount");
            boundaryCrossingCount = IntField(manifest, "boundaryCrossingCount");
            objectCount = IntField(manifest, "objectCount");
            maxActiveChunkCount = IntField(manifest, "maxActiveChunkCount");
            maxBoundaryPrefetchChunkCount = IntField(manifest, "maxBoundaryPrefetchChunkCount");
            status = movementSampleCount >= 6 && boundaryCrossingCount >= 2 && objectCount > 0
                ? "Goal104 interactive travel payload ready"
                : "Goal104 interactive travel payload missing or incomplete";
        }

        public void CreatePreviewRig()
        {
            ClearPreviewRig();
            var root = new GameObject(RigRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Interactive Travel Rig");
            root.AddComponent<OfflineGeoworldPreviewPlayerMotor>();
            root.AddComponent<OfflineGeoworldBoundaryPrefetchState>();
            var controller = root.AddComponent<OfflineGeoworldInteractiveTravelController>();
            controller.RefreshPayload();
            status = "Created interactive travel preview rig";
        }

        public void ClearPreviewRig()
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
    }
}
#endif
