#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityFeatureModuleCompositionMatrixWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Accepted Alpha/FeatureModule Composition Matrix";
        private CanonicalRuntimeUnityFeatureModuleCompositionMatrixView view =
            new CanonicalRuntimeUnityFeatureModuleCompositionMatrixView();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open() =>
            GetWindow<CanonicalRuntimeUnityFeatureModuleCompositionMatrixWindow>("FeatureModule Matrix");

        private void OnEnable() => Reload();

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity mode: read-only Goal146 artifact consumer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", view.Status);
            EditorGUILayout.LabelField("Selected composition", view.SelectedComposition);
            EditorGUILayout.LabelField("Compositions", view.PassedCompositionCount + "/" + view.CompositionCount);
            EditorGUILayout.LabelField("Selected optional modules", view.SelectedModuleCount.ToString());
            EditorGUILayout.LabelField("Selected combined effects", view.SelectedCombinedEffectCount.ToString());
            EditorGUILayout.LabelField("Module lineage", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(view.CatalogText, GUILayout.MinHeight(160));
            EditorGUILayout.LabelField("Qualification rows", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(view.MatrixText, GUILayout.MinHeight(220));
            if (GUILayout.Button("Refresh read-only artifacts")) Reload();
            EditorGUILayout.HelpBox(
                "Unity reads Goal146 evidence only. It cannot edit module selection or execute Runtime gameplay.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project != null ? project.Parent : null;
            var repo = unityRoot != null ? unityRoot.Parent : null;
            var root = repo == null ? string.Empty : Path.Combine(repo.FullName, ".llmgc", "procedural",
                "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix");
            view = CanonicalRuntimeUnityFeatureModuleCompositionMatrixHarness.LoadView(root);
            Repaint();
        }
    }
}
#endif
