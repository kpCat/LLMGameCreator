#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerLoopStepperWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Accepted Alpha/Runtime Player Loop Stepper";
        private const string WindowTitle = "Runtime Player Loop Stepper";

        private string modelPath = string.Empty;
        private string status = "Not loaded";
        private CanonicalRuntimeUnityPlayerLoopStepperModelView model =
            new CanonicalRuntimeUnityPlayerLoopStepperModelView();
        private int frameIndex;
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CanonicalRuntimeUnityPlayerLoopStepperWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            modelPath = DefaultModelPath();
            LoadDefaultModel();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Model", string.IsNullOrWhiteSpace(modelPath) ? "(none)" : modelPath);
            EditorGUILayout.LabelField("Status", status);
            EditorGUILayout.LabelField("Candidate", EmptyAsNone(model.CandidateId));
            EditorGUILayout.LabelField("Frame", (frameIndex + 1) + "/" + Mathf.Max(model.Frames.Count, 1));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Default Goal138 Stepper Model"))
            {
                LoadDefaultModel();
            }

            if (GUILayout.Button("Previous"))
            {
                frameIndex = Mathf.Max(0, frameIndex - 1);
            }

            if (GUILayout.Button("Next"))
            {
                frameIndex = Mathf.Min(Mathf.Max(model.Frames.Count - 1, 0), frameIndex + 1);
            }
            EditorGUILayout.EndHorizontal();

            if (model.Frames.Count > 0)
            {
                var frame = model.Frames[Mathf.Clamp(frameIndex, 0, model.Frames.Count - 1)];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Frame Category", frame.FrameCategory);
                EditorGUILayout.LabelField("Title", frame.Title);
                EditorGUILayout.LabelField("Canonical State Hash", frame.CanonicalStateHash);
                EditorGUILayout.LabelField("HUD Lines", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(frame.Hud, GUILayout.MinHeight(140f));
            }
            else
            {
                EditorGUILayout.HelpBox("Goal138 stepper model is not loaded.", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        private void LoadDefaultModel()
        {
            modelPath = DefaultModelPath();
            model = CanonicalRuntimeUnityPlayerLoopStepperHarness.LoadModelView(modelPath);
            frameIndex = Mathf.Clamp(model.CurrentFrameIndex, 0, Mathf.Max(model.Frames.Count - 1, 0));
            status = File.Exists(modelPath)
                ? "Loaded Goal138 runtime-backed stepper model."
                : "Goal138 runtime-backed stepper model not found.";
        }

        private static string DefaultModelPath()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project?.Parent;
            var repoRoot = unityRoot?.Parent;
            return Path.Combine(
                repoRoot?.FullName ?? string.Empty,
                ".llmgc",
                "procedural",
                "goal-138-runtime-backed-unity-player-loop-stepper-hud-harness",
                "runtime-backed-player-loop-stepper-model.json");
        }

        private static string EmptyAsNone(string value) =>
            string.IsNullOrWhiteSpace(value) ? "none" : value;
    }
}
#endif
