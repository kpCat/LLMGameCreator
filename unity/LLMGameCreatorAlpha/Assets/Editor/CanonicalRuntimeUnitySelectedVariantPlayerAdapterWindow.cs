#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySelectedVariantPlayerAdapterWindow : EditorWindow
    {
        private const string MenuPath =
            "LLMGameCreator/Accepted Alpha/Selected Runtime Variant PlayerAdapter";
        private const string WindowTitle = "Selected Runtime Variant PlayerAdapter";

        private string modelPath = string.Empty;
        private string framesPath = string.Empty;
        private string handoffPath = string.Empty;
        private int frameIndex;
        private Vector2 scroll;
        private CanonicalRuntimeUnitySelectedVariantPlayerAdapterModelView model =
            new CanonicalRuntimeUnitySelectedVariantPlayerAdapterModelView();

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<CanonicalRuntimeUnitySelectedVariantPlayerAdapterWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            Reload();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Unity mode: Selected PlayerAdapter consumer only",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", EmptyAsNone(model.Status));
            EditorGUILayout.LabelField("Candidate", EmptyAsNone(model.CandidateId));
            EditorGUILayout.LabelField("Variant", EmptyAsNone(model.VariantKind));
            EditorGUILayout.LabelField("Score", model.Score.ToString());
            EditorGUILayout.LabelField(
                "Package SHA-256 status",
                model.PackageHashMatch ? "MATCH" : "MISMATCH");
            EditorGUILayout.LabelField(
                "Final state hash status",
                model.FinalStateHashMatch ? "MATCH" : "MISMATCH");
            EditorGUILayout.LabelField(
                "Frame",
                model.FrameCount > 0
                    ? (model.CurrentFrameIndex + 1) + "/" + model.FrameCount
                    : "0/0");
            EditorGUILayout.LabelField(
                "Control intent / route",
                EmptyAsNone(model.ControlIntent) + " / " + EmptyAsNone(model.Route));
            EditorGUILayout.LabelField("Canonical step", EmptyAsNone(model.CanonicalStep));
            EditorGUILayout.LabelField("Inventory summary", EmptyAsNone(model.InventorySummary));
            EditorGUILayout.LabelField("Quest summary", EmptyAsNone(model.QuestSummary));
            EditorGUILayout.LabelField("Combat summary", EmptyAsNone(model.CombatSummary));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                frameIndex = Mathf.Max(0, frameIndex - 1);
                LoadFrame();
            }

            if (GUILayout.Button("Next"))
            {
                frameIndex = Mathf.Min(Mathf.Max(0, model.FrameCount - 1), frameIndex + 1);
                LoadFrame();
            }

            if (GUILayout.Button("Reload"))
            {
                Reload();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "Read-only Goal143 consumer. Runtime owns gameplay execution and state mutation.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            var root = RepoRoot();
            var artifactRoot = Path.Combine(
                root,
                ".llmgc",
                "procedural",
                "goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff");
            modelPath = Path.Combine(
                artifactRoot,
                "selected-runtime-variant-playeradapter-model.json");
            framesPath = Path.Combine(
                artifactRoot,
                "selected-runtime-variant-playeradapter-frames.json");
            handoffPath = Path.Combine(
                artifactRoot,
                "selected-runtime-variant-playeradapter-handoff.json");
            frameIndex = 0;
            LoadFrame();
        }

        private void LoadFrame()
        {
            model = CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.LoadModelView(
                modelPath,
                framesPath,
                handoffPath,
                frameIndex);
            frameIndex = model.CurrentFrameIndex;
            Repaint();
        }

        private static string RepoRoot()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project != null ? project.Parent : null;
            var repoRoot = unityRoot != null ? unityRoot.Parent : null;
            return repoRoot != null ? repoRoot.FullName : string.Empty;
        }

        private static string EmptyAsNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
#endif
