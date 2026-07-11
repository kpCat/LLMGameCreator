#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityProductLineInteractiveSessionMatrixWindow : EditorWindow
    {
        private const string MenuPath =
            "LLMGameCreator/Accepted Alpha/Product-Line Runtime Session Matrix";
        private CanonicalRuntimeUnityProductLineInteractiveSessionMatrixView view =
            new CanonicalRuntimeUnityProductLineInteractiveSessionMatrixView();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open() =>
            GetWindow<CanonicalRuntimeUnityProductLineInteractiveSessionMatrixWindow>("Runtime Session Matrix");

        private void OnEnable() => Reload();

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity mode: read-only Goal145 matrix consumer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", view.Status);
            EditorGUILayout.LabelField("Selected candidate", view.SelectedCandidate);
            EditorGUILayout.LabelField("Selected variant", view.SelectedVariant);
            EditorGUILayout.LabelField("Candidates", view.PassedCandidateCount + "/" + view.CandidateCount);
            EditorGUILayout.LabelField("Distinct final hashes", view.DistinctFinalStateHashCount.ToString());
            EditorGUILayout.LabelField("Checkpoint reloads", view.AllCheckpointReloadsPassed ? "PASS" : "FAIL");
            EditorGUILayout.LabelField("Full replays", view.AllFullReplaysEquivalent ? "PASS" : "FAIL");
            EditorGUILayout.LabelField("Action bindings", view.AllActionBindingsPassed ? "PASS" : "FAIL");
            EditorGUILayout.LabelField("Focus effects", view.AllFocusEffectsObserved ? "PASS" : "FAIL");
            EditorGUILayout.TextArea(view.MatrixText, GUILayout.MinHeight(220));
            if (GUILayout.Button("Refresh read-only artifacts")) Reload();
            EditorGUILayout.HelpBox(
                "Unity reads Goal145 artifacts only. It does not execute gameplay, choose a winner, mutate selection, or load a GamePackage as gameplay truth.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project != null ? project.Parent : null;
            var repo = unityRoot != null ? unityRoot.Parent : null;
            var root = repo == null ? string.Empty : Path.Combine(
                repo.FullName,
                ".llmgc",
                "procedural",
                "goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix");
            view = CanonicalRuntimeUnityProductLineInteractiveSessionMatrixHarness.LoadView(root);
            Repaint();
        }
    }
}
#endif
