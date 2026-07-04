#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaAcceptanceRunnerWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Alpha Acceptance Runner";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal110";
        private const string RunnerObjectName = "__LLMGC_OfflineGeoworldAlphaAcceptanceRunner__";
        private const string ChecklistFileName = "offline-geoworld-alpha-acceptance-checklist.json";
        private const string ManifestFileName = "offline-geoworld-alpha-acceptance-manifest.json";
        private const string ResultTemplateFileName =
            "offline-geoworld-alpha-acceptance-result-template.json";

        private string packagePath = string.Empty;
        private string statusLine = "Not loaded";
        private string checklistStatus = string.Empty;
        private string resultStatus = string.Empty;
        private string checklistHash = string.Empty;
        private string resultTemplateHash = string.Empty;
        private Vector2 scrollPosition;
        private readonly List<string> stepIds = new List<string>();

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldAlphaAcceptanceRunnerWindow>("Geoworld Acceptance");
        }

        private void OnEnable()
        {
            RefreshPayload();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Payload Path", packagePath);
            EditorGUILayout.LabelField("Status", statusLine);
            EditorGUILayout.LabelField("Checklist Steps", stepIds.Count.ToString());
            EditorGUILayout.LabelField("Result Template Hash", resultTemplateHash);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Checklist Status");
            EditorGUILayout.TextArea(checklistStatus, GUILayout.MinHeight(180));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Local Result");
            EditorGUILayout.TextArea(resultStatus, GUILayout.MinHeight(120));
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Payload"))
            {
                RefreshPayload();
            }

            if (GUILayout.Button("Create Runner Object"))
            {
                CreateRunnerObject();
            }

            if (GUILayout.Button("Save Pending Result"))
            {
                SavePendingResult();
            }

            if (GUILayout.Button("Load Result"))
            {
                LoadResult();
            }

            if (GUILayout.Button("Clear Result"))
            {
                ClearResult();
            }

            if (GUILayout.Button("Clear Runner Object"))
            {
                ClearRunnerObject();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPayload()
        {
            packagePath = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var manifest = ReadPayloadFile(ManifestFileName);
            var checklist = ReadPayloadFile(ChecklistFileName);
            var template = ReadPayloadFile(ResultTemplateFileName);
            stepIds.Clear();
            foreach (Match match in Regex.Matches(checklist, "\"stepId\"\\s*:\\s*\"([^\"]+)\""))
            {
                stepIds.Add(match.Groups[1].Value);
            }

            checklistHash = Sha256(checklist);
            resultTemplateHash = Sha256(template);
            checklistStatus = BuildChecklistSummary(checklist);
            statusLine = manifest.Contains("manualAcceptancePending", System.StringComparison.Ordinal)
                         && stepIds.Count >= 12
                ? "Goal110 manual acceptance payload ready"
                : "Goal110 manual acceptance payload missing or incomplete";
        }

        public void CreateRunnerObject()
        {
            RefreshPayload();
            var root = GameObject.Find(RunnerObjectName);
            if (root == null)
            {
                root = new GameObject(RunnerObjectName);
                Undo.RegisterCreatedObjectUndo(root, "Create Offline Geoworld Alpha Acceptance Runner");
            }

            if (root.GetComponent<OfflineGeoworldAlphaAcceptanceResultStore>() == null)
            {
                root.AddComponent<OfflineGeoworldAlphaAcceptanceResultStore>();
            }

            statusLine = "Runner object ready.";
        }

        public void SavePendingResult()
        {
            var store = RequireStore();
            var result = store.CreatePendingResult(stepIds, packagePath, checklistHash, resultTemplateHash);
            store.SaveResult(result);
            resultStatus = store.LastStatus;
        }

        public void LoadResult()
        {
            var store = RequireStore();
            var result = store.LoadResult();
            resultStatus = result == null
                ? store.LastStatus
                : store.LastStatus + "\nstatus=" + result.resultStatus + "\nsteps=" + result.steps.Count;
        }

        public void ClearResult()
        {
            var store = RequireStore();
            store.ClearResult();
            resultStatus = store.LastStatus;
        }

        public void ClearRunnerObject()
        {
            var root = GameObject.Find(RunnerObjectName);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }

            statusLine = "Runner object cleared.";
        }

        private OfflineGeoworldAlphaAcceptanceResultStore RequireStore()
        {
            var root = GameObject.Find(RunnerObjectName);
            if (root == null)
            {
                CreateRunnerObject();
                root = GameObject.Find(RunnerObjectName);
            }

            return root.GetComponent<OfflineGeoworldAlphaAcceptanceResultStore>();
        }

        private static string ReadPayloadFile(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, RelativeRoot, fileName);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        private static string BuildChecklistSummary(string json)
        {
            var lines = new List<string>();
            foreach (Match block in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\"stepId\"[\\s\\S]*?\\}"))
            {
                lines.Add("- " + StringField(block.Value, "stepId")
                          + ": " + StringField(block.Value, "title"));
            }

            return lines.Count == 0 ? "checklist missing" : string.Join("\n", lines.ToArray());
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string Sha256(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
#endif
