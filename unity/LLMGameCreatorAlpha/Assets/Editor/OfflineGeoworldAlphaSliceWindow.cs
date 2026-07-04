#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaSliceWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Alpha Slice";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal108";
        private const string RigRootName = "__LLMGC_OfflineGeoworldAlphaSlice__";

        private string payloadPath = string.Empty;
        private string status = "Not loaded";
        private int componentCount;
        private int readyComponentCount;
        private int objectiveCount;
        private int completedObjectiveCount;
        private string finalStatus = string.Empty;
        private string finalAcceptanceHash = string.Empty;
        private string componentSummary = string.Empty;
        private string runbookSummary = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldAlphaSliceWindow>("Offline Geoworld Alpha Slice");
        }

        private void OnEnable()
        {
            RefreshManifestStatus();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Payload", payloadPath);
            EditorGUILayout.LabelField("Status", status);
            EditorGUILayout.LabelField("Components", readyComponentCount + "/" + componentCount);
            EditorGUILayout.LabelField("Objectives", completedObjectiveCount + "/" + objectiveCount);
            EditorGUILayout.LabelField("Final Status", finalStatus);
            EditorGUILayout.LabelField("Final Acceptance Hash", finalAcceptanceHash);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Component readiness");
            EditorGUILayout.TextArea(componentSummary, GUILayout.MinHeight(150));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Acceptance runbook");
            EditorGUILayout.TextArea(runbookSummary, GUILayout.MinHeight(150));
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Manifest Status"))
            {
                RefreshManifestStatus();
            }

            if (GUILayout.Button("Create Alpha Slice Rig"))
            {
                CreateAlphaSliceRig();
            }

            if (GUILayout.Button("Verify Alpha Slice"))
            {
                VerifyAlphaSlice();
            }

            if (GUILayout.Button("Clear Alpha Slice Rig"))
            {
                ClearAlphaSliceRig();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshManifestStatus()
        {
            payloadPath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile("offline-geoworld-alpha-slice-manifest.json");
            var components = ReadPayloadFile("offline-geoworld-alpha-slice-components.json");
            var runbook = ReadPayloadFile("offline-geoworld-alpha-slice-acceptance-runbook.json");
            componentCount = IntField(manifest, "componentCount");
            readyComponentCount = IntField(manifest, "readyComponentCount");
            objectiveCount = IntField(manifest, "objectiveCount");
            completedObjectiveCount = IntField(manifest, "completedObjectiveCount");
            finalStatus = StringField(manifest, "finalStatus");
            finalAcceptanceHash = StringField(manifest, "finalAcceptanceHash");
            componentSummary = ComponentSummary(components);
            runbookSummary = RunbookSummary(runbook);
            status = componentCount == 7
                     && readyComponentCount == componentCount
                     && objectiveCount >= 5
                     && completedObjectiveCount == objectiveCount
                     && finalStatus == "completed"
                ? "Goal108 Alpha Slice manifest ready"
                : "Goal108 Alpha Slice manifest missing or incomplete";
        }

        public void CreateAlphaSliceRig()
        {
            RefreshManifestStatus();
            if (componentCount != 7 || readyComponentCount != componentCount)
            {
                status = "Create rejected: manifest components are incomplete";
                return;
            }

            ClearAlphaSliceRig();
            var root = new GameObject(RigRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Alpha Slice Rig");
            root.AddComponent<OfflineGeoworldPreviewRunner>();
            root.AddComponent<OfflineGeoworldPlayModeTravelController>();
            root.AddComponent<OfflineGeoworldInteractiveTravelController>();
            root.AddComponent<OfflineGeoworldInteractionController>();
            root.AddComponent<OfflineGeoworldSessionSaveLoadController>();
            root.AddComponent<OfflineGeoworldSessionReplayController>();
            root.AddComponent<OfflineGeoworldObjectiveAcceptanceController>();
            var coordinator = root.AddComponent<OfflineGeoworldAlphaSliceCoordinator>();
            coordinator.RefreshStatus();
            status = "Created Alpha Slice rig";
        }

        public bool VerifyAlphaSlice()
        {
            RefreshManifestStatus();
            var root = GameObject.Find(RigRootName);
            if (root == null)
            {
                status = "Verify rejected: Alpha Slice rig missing";
                return false;
            }

            var coordinator = root.GetComponent<OfflineGeoworldAlphaSliceCoordinator>();
            if (coordinator == null)
            {
                coordinator = root.AddComponent<OfflineGeoworldAlphaSliceCoordinator>();
            }

            var verified = coordinator.VerifySlice();
            status = verified ? "Alpha Slice verified" : "Alpha Slice verification incomplete";
            return verified;
        }

        public void ClearAlphaSliceRig()
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
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        private static string ComponentSummary(string json)
        {
            var lines = new List<string>();
            foreach (Match block in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\"componentId\"[\\s\\S]*?\\}"))
            {
                lines.Add("- " + StringField(block.Value, "componentId")
                          + ": ready=" + BoolField(block.Value, "ready")
                          + ", gate=" + StringField(block.Value, "manualGate"));
            }

            return lines.Count == 0 ? "component readiness missing" : string.Join("\n", lines.ToArray());
        }

        private static string RunbookSummary(string json)
        {
            var lines = new List<string>();
            foreach (Match item in Regex.Matches(json ?? string.Empty, "\"([^\"]*)\""))
            {
                var value = item.Groups[1].Value;
                if (value.Contains("Unity", System.StringComparison.Ordinal)
                    || value.Contains("Verify", System.StringComparison.Ordinal)
                    || value.Contains("Clear", System.StringComparison.Ordinal)
                    || value.Contains("offline_geoworld_alpha_slice", System.StringComparison.Ordinal))
                {
                    lines.Add("- " + value);
                }
            }

            return lines.Count == 0 ? "acceptance runbook missing" : string.Join("\n", lines.ToArray());
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

        private static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            bool value;
            return match.Success && bool.TryParse(match.Groups[1].Value, out value) && value;
        }
    }
}
#endif
