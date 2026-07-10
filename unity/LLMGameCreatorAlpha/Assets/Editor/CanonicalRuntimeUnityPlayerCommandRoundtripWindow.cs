#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerCommandRoundtripWindow : EditorWindow
    {
        private const string MenuPath =
            "LLMGameCreator/Accepted Alpha/Runtime Player Command Roundtrip";
        private const string WindowTitle = "Runtime Player Command Roundtrip";

        private string modelPath = string.Empty;
        private string resultPath = string.Empty;
        private string status = "Not loaded";
        private CanonicalRuntimeUnityPlayerCommandRoundtripModelView model =
            new CanonicalRuntimeUnityPlayerCommandRoundtripModelView();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CanonicalRuntimeUnityPlayerCommandRoundtripWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            modelPath = DefaultModelPath();
            resultPath = DefaultResultPath();
            LoadDefaultModel();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity mode: PlayerAdapter command request/response only", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Model", string.IsNullOrWhiteSpace(modelPath) ? "(none)" : modelPath);
            EditorGUILayout.LabelField("Result", string.IsNullOrWhiteSpace(resultPath) ? "(none)" : resultPath);
            EditorGUILayout.LabelField("Status", status);
            EditorGUILayout.LabelField("Candidate", EmptyAsNone(model.CandidateId));
            EditorGUILayout.LabelField("Request Count", model.RoundtripRequestCount.ToString());
            EditorGUILayout.LabelField("Executed Request Count", model.RuntimeExecutedRequestCount.ToString());
            EditorGUILayout.LabelField("Snapshot Count", model.RoundtripSnapshotCount.ToString());
            EditorGUILayout.LabelField("Current Request", EmptyAsNone(model.CurrentRequest));
            EditorGUILayout.LabelField("Current Response Snapshot", EmptyAsNone(model.CurrentResponseSnapshot));

            if (GUILayout.Button("Reload Goal141 Roundtrip Model"))
            {
                LoadDefaultModel();
            }

            EditorGUILayout.HelpBox(
                "Read-only Goal141 view. Runtime remains gameplay truth; Unity only consumes command request/response artifacts.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void LoadDefaultModel()
        {
            modelPath = DefaultModelPath();
            resultPath = DefaultResultPath();
            model = CanonicalRuntimeUnityPlayerCommandRoundtripHarness.LoadModelView(modelPath);
            status = File.Exists(modelPath) && File.Exists(resultPath)
                ? "Loaded Goal141 runtime-backed command roundtrip model."
                : "Goal141 runtime-backed command roundtrip artifacts not found.";
        }

        private static string DefaultModelPath() =>
            Path.Combine(RepoRoot(), ".llmgc", "procedural",
                "goal-141-runtime-backed-unity-player-command-roundtrip-bridge",
                "runtime-backed-player-command-roundtrip-model.json");

        private static string DefaultResultPath() =>
            Path.Combine(RepoRoot(), ".llmgc", "procedural",
                "goal-141-runtime-backed-unity-player-command-roundtrip-bridge",
                "runtime-backed-player-command-roundtrip-result.json");

        private static string RepoRoot()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project?.Parent;
            var repoRoot = unityRoot?.Parent;
            return repoRoot?.FullName ?? string.Empty;
        }

        private static string EmptyAsNone(string value) =>
            string.IsNullOrWhiteSpace(value) ? "none" : value;
    }
}
#endif
