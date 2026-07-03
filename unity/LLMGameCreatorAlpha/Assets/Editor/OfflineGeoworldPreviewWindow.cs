#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPreviewWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Preview";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal101";
        private const string PreviewRootName = "__LLMGC_OfflineGeoworldEditorPreview__";

        private string status = "Not loaded";
        private string payloadPath = string.Empty;
        private int commandCount;
        private int commandKindCount;
        private int travelWindowStepCount;
        private int lastCreatedObjectCount;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldPreviewWindow>("Offline Geoworld Preview");
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
            EditorGUILayout.LabelField("Commands", commandCount.ToString());
            EditorGUILayout.LabelField("Command Kinds", commandKindCount.ToString());
            EditorGUILayout.LabelField("Travel Steps", travelWindowStepCount.ToString());
            EditorGUILayout.LabelField("Created Objects", lastCreatedObjectCount.ToString());
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload Status"))
            {
                RefreshPayloadStatus();
            }

            if (GUILayout.Button("Create Preview Objects"))
            {
                CreatePreviewObjects();
            }

            if (GUILayout.Button("Clear Preview Objects"))
            {
                ClearPreviewObjects();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayloadStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-preview-runner-manifest.json");
            var travel = ReadPayloadFile("offline-geoworld-preview-travel-window-script.json");
            commandCount = IntField(manifest, "commandCount");
            commandKindCount = IntField(manifest, "commandKindCount");
            travelWindowStepCount = IntField(travel, "stepCount");
            status = commandCount > 0 && travelWindowStepCount > 0
                ? "Goal101 payload ready"
                : "Goal101 payload missing or incomplete";
        }

        public void CreatePreviewObjects()
        {
            ClearPreviewObjects();
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var commandsJson = ReadPayloadFile("offline-geoworld-preview-feature-commands.json");
            var styleJson = ReadPayloadFile("offline-geoworld-preview-style-legend.json");
            var commands = ReadCommands(commandsJson);
            var root = new GameObject(PreviewRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Preview");
            foreach (var command in commands)
            {
                OfflineGeoworldPreviewPrimitiveFactory.CreatePlaceholder(
                    root.transform,
                    command,
                    styleJson);
            }

            lastCreatedObjectCount = commands.Count;
            commandCount = commands.Count;
            status = "Created " + lastCreatedObjectCount + " placeholder preview objects";
        }

        public void ClearPreviewObjects()
        {
            var root = GameObject.Find(PreviewRootName);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }

            lastCreatedObjectCount = 0;
            if (string.IsNullOrWhiteSpace(status))
            {
                status = "Preview objects cleared";
            }
        }

        private static List<OfflineGeoworldPreviewCommand> ReadCommands(string json)
        {
            var commands = new List<OfflineGeoworldPreviewCommand>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\"commandId\"[\\s\\S]*?\\}"))
            {
                var block = match.Value;
                commands.Add(new OfflineGeoworldPreviewCommand
                {
                    CommandId = StringField(block, "commandId"),
                    CommandKind = StringField(block, "commandKind"),
                    StyleKey = StringField(block, "styleKey"),
                    GridX = IntField(block, "gridX"),
                    GridZ = IntField(block, "gridZ"),
                    Elevation = IntField(block, "elevation")
                });
            }

            return commands;
        }

        private static string ReadPayloadFile(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, RelativeRoot, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
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
