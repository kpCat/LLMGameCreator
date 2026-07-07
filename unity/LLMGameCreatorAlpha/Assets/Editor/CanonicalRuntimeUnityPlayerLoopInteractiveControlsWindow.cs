#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow : EditorWindow
    {
        private const string MenuPath =
            "LLMGameCreator/Accepted Alpha/Runtime Player Loop Controls";
        private const string WindowTitle = "Runtime Player Loop Controls";

        private string modelPath = string.Empty;
        private string status = "Not loaded";
        private string lastControlAction = "none";
        private CanonicalRuntimeUnityPlayerLoopInteractiveControlsModelView model =
            new CanonicalRuntimeUnityPlayerLoopInteractiveControlsModelView();
        private int frameIndex;
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow>(WindowTitle);
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
            EditorGUILayout.LabelField("Unity mode: PlayerAdapter/HUD controls only", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Model", string.IsNullOrWhiteSpace(modelPath) ? "(none)" : modelPath);
            EditorGUILayout.LabelField("Status", status);
            EditorGUILayout.LabelField("Candidate", EmptyAsNone(model.CandidateId));
            EditorGUILayout.LabelField("Current Frame", HumanFrameLabel());
            EditorGUILayout.LabelField("Frame Index", frameIndex.ToString());
            EditorGUILayout.LabelField("Total Frames", Mathf.Max(model.Frames.Count, model.FrameCount).ToString());
            EditorGUILayout.LabelField("Last Control Action", lastControlAction);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Goal140 Controls UX Model"))
            {
                LoadDefaultModel();
                lastControlAction = "load_model";
            }

            if (GUILayout.Button("Reset/First"))
            {
                frameIndex = 0;
                lastControlAction = "first";
                status = "reset_to_first_frame";
            }

            if (GUILayout.Button("Previous"))
            {
                frameIndex = Mathf.Max(0, frameIndex - 1);
                lastControlAction = "previous";
                status = "moved_previous_frame";
            }

            if (GUILayout.Button("Next"))
            {
                frameIndex = Mathf.Min(Mathf.Max(model.Frames.Count - 1, 0), frameIndex + 1);
                lastControlAction = "next";
                status = "moved_next_frame";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Last"))
            {
                frameIndex = Mathf.Max(model.Frames.Count - 1, 0);
                lastControlAction = "last";
                status = "moved_last_frame";
            }

            if (GUILayout.Button("Step Once"))
            {
                frameIndex = Mathf.Min(Mathf.Max(model.Frames.Count - 1, 0), frameIndex + 1);
                lastControlAction = "step_once";
                status = "stepped_one_frame_tick";
            }

            if (GUILayout.Button("Play All To End"))
            {
                frameIndex = Mathf.Max(model.Frames.Count - 1, 0);
                lastControlAction = "play_all_to_end";
                status = "played_all_to_end";
            }

            if (GUILayout.Button("Copy Frame Summary"))
            {
                EditorGUIUtility.systemCopyBuffer = CurrentFrame().PlayerFacingSummary;
                lastControlAction = "copy_current_frame_summary";
                status = "copied_frame_summary";
            }
            EditorGUILayout.EndHorizontal();

            if (model.Frames.Count > 0)
            {
                var frame = CurrentFrame();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Frame Category", frame.FrameCategory);
                EditorGUILayout.LabelField("Title", frame.Title);
                EditorGUILayout.LabelField("Canonical State Hash", frame.CanonicalStateHash);
                EditorGUILayout.LabelField("HUD Lines", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(frame.Hud, GUILayout.MinHeight(140f));
                EditorGUILayout.LabelField("Frame Summary", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(frame.PlayerFacingSummary, GUILayout.MinHeight(80f));
            }
            else
            {
                EditorGUILayout.HelpBox("Goal139 interactive controls model is not loaded.", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        private void LoadDefaultModel()
        {
            modelPath = DefaultModelPath();
            model = CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.LoadModelView(modelPath);
            frameIndex = Mathf.Clamp(model.CurrentFrameIndex, 0, Mathf.Max(model.Frames.Count - 1, 0));
            status = File.Exists(modelPath)
                ? "Loaded Goal140 runtime-backed controls UX model."
                : "Goal140 runtime-backed controls UX model not found.";
        }

        private CanonicalRuntimeUnityPlayerLoopInteractiveControlsFrameView CurrentFrame()
        {
            if (model.Frames.Count == 0)
            {
                return new CanonicalRuntimeUnityPlayerLoopInteractiveControlsFrameView();
            }

            return model.Frames[Mathf.Clamp(frameIndex, 0, model.Frames.Count - 1)];
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
                "goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard",
                "runtime-backed-player-loop-controls-ux-model.json");
        }

        private string HumanFrameLabel()
        {
            var total = Mathf.Max(model.Frames.Count, model.FrameCount);
            if (total <= 0)
            {
                return "Current Frame: 0/0";
            }

            return "Current Frame: " + (frameIndex + 1) + "/" + total;
        }

        private static string EmptyAsNone(string value) =>
            string.IsNullOrWhiteSpace(value) ? "none" : value;
    }
}
#endif
