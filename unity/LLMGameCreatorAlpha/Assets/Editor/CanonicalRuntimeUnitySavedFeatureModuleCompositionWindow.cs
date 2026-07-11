#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySavedFeatureModuleCompositionWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Accepted Alpha/Saved FeatureModule Composition";
        private CanonicalRuntimeUnitySavedFeatureModuleCompositionView view =
            new CanonicalRuntimeUnitySavedFeatureModuleCompositionView();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open() =>
            GetWindow<CanonicalRuntimeUnitySavedFeatureModuleCompositionWindow>("Saved FeatureModules");

        private void OnEnable() => Reload();

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity mode: read-only Goal147 evidence consumer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", view.Status);
            EditorGUILayout.LabelField("Composition", view.CompositionId);
            EditorGUILayout.LabelField("Revision", view.Revision.ToString());
            EditorGUILayout.LabelField("Catalog fingerprint", view.CatalogFingerprint);
            EditorGUILayout.LabelField("Package SHA", view.PackageSha256);
            EditorGUILayout.LabelField("Runtime final hash", view.FinalStateHash);
            EditorGUILayout.LabelField("Saved composition", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(view.CompositionText, GUILayout.MinHeight(140));
            EditorGUILayout.LabelField("Effective typed parameters", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(view.ParameterText, GUILayout.MinHeight(140));
            EditorGUILayout.LabelField("Runtime-owned semantic effects", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(view.RuntimeEffectsText, GUILayout.MinHeight(140));
            if (GUILayout.Button("Refresh read-only Goal147 evidence")) Reload();
            EditorGUILayout.HelpBox(
                "Unity cannot edit or persist compositions, apply parameters, build packages, or execute gameplay.",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            var project = Directory.GetParent(Application.dataPath);
            var unityRoot = project != null ? project.Parent : null;
            var repo = unityRoot != null ? unityRoot.Parent : null;
            var root = repo == null ? string.Empty : Path.Combine(repo.FullName, ".llmgc", "procedural",
                "goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification");
            view = CanonicalRuntimeUnitySavedFeatureModuleCompositionHarness.LoadView(root);
            Repaint();
        }
    }
}
#endif
