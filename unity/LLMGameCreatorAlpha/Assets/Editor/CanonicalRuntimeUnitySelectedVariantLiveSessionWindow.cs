#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySelectedVariantLiveSessionWindow : EditorWindow
    {
        private const string MenuPath =
            "LLMGameCreator/Accepted Alpha/Selected Runtime Variant Live Session";
        private CanonicalRuntimeUnitySelectedVariantLiveSessionView view =
            new CanonicalRuntimeUnitySelectedVariantLiveSessionView();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        public static void Open() =>
            GetWindow<CanonicalRuntimeUnitySelectedVariantLiveSessionWindow>("Selected Variant Live Session");

        private void OnEnable() => Reload();

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Gameplay truth: Runtime", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity mode: read-only live-session consumer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", view.Status);
            EditorGUILayout.LabelField("Candidate / variant", view.Candidate + " / " + view.Variant);
            EditorGUILayout.LabelField("Session ID", view.SessionId);
            EditorGUILayout.LabelField("Current state hash", view.StateHash);
            EditorGUILayout.LabelField("Action progress", view.ActionIndex + "/" + view.ActionCount);
            EditorGUILayout.LabelField("Last action", view.LastAction);
            EditorGUILayout.LabelField("Map", view.MapSummary);
            EditorGUILayout.LabelField("Inventory", view.InventorySummary);
            EditorGUILayout.LabelField("Quest", view.QuestSummary);
            EditorGUILayout.LabelField("Combat", view.CombatSummary);
            EditorGUILayout.LabelField("Checkpoint replay", view.CheckpointReloadPassed ? "PASS" : "FAIL");
            EditorGUILayout.LabelField("Full replay", view.FullReplayEquivalent ? "PASS" : "FAIL");
            if (GUILayout.Button("Refresh read-only artifacts")) Reload();
            EditorGUILayout.HelpBox("Unity reads Goal144 artifacts only and never executes gameplay.", MessageType.Info);
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
                "goal-144-selected-runtime-variant-interactive-action-session-and-save-replay");
            view = CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.LoadView(root);
            Repaint();
        }
    }
}
#endif
