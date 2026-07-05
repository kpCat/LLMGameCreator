#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class AcceptedAlphaPlayableProjectionWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection";
        private const string WindowTitle = "Accepted Alpha Projection";

        private Vector2 scrollPosition;
        private string statusLine = "Not loaded";
        private string diagnostics = string.Empty;
        private string smokeDiagnostics = string.Empty;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<AcceptedAlphaPlayableProjectionWindow>(WindowTitle);
        }

        public static void RunBatchmodeProjectionSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureController();
                controller.RefreshAcceptedBaseline();
                controller.BuildOrRefreshProjection();
                var passed = controller.RunLocalProjectionSmoke();
                var diagnostics = controller.LastDiagnostics + "\n" + controller.LastSmokeDiagnostics;
                if (passed)
                {
                    Debug.Log("GOAL119A_PROJECTION_SMOKE_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL119A_PROJECTION_SMOKE_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL119A_PROJECTION_SMOKE_FAIL\n" + ex);
            }
            finally
            {
                try
                {
                    ClearProjectionRootImmediate();
                }
                catch (Exception ex)
                {
                    exitCode = 1;
                    Debug.LogError("GOAL119A_PROJECTION_SMOKE_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        private void OnEnable()
        {
            RefreshAcceptedBaseline();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Menu Path", MenuPath);
            EditorGUILayout.LabelField("Generated Root", AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            EditorGUILayout.LabelField("Status", statusLine);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Diagnostics");
            EditorGUILayout.TextArea(diagnostics, GUILayout.MinHeight(120));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Smoke");
            EditorGUILayout.TextArea(smokeDiagnostics, GUILayout.MinHeight(120));
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh Accepted Baseline"))
            {
                RefreshAcceptedBaseline();
            }

            if (GUILayout.Button("Build/Refresh Playable Projection"))
            {
                BuildOrRefreshPlayableProjection();
            }

            if (GUILayout.Button("Run Local Projection Smoke"))
            {
                RunLocalProjectionSmoke();
            }

            if (GUILayout.Button("Clear Projection"))
            {
                ClearProjection();
            }

            if (GUILayout.Button("Copy Diagnostics"))
            {
                EditorGUIUtility.systemCopyBuffer = diagnostics + "\n\n" + smokeDiagnostics;
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshAcceptedBaseline()
        {
            var controller = EnsureController();
            controller.RefreshAcceptedBaseline();
            Capture(controller);
        }

        private void BuildOrRefreshPlayableProjection()
        {
            var controller = EnsureController();
            controller.BuildOrRefreshProjection();
            Selection.activeGameObject = controller.gameObject;
            Capture(controller);
        }

        private void RunLocalProjectionSmoke()
        {
            var controller = EnsureController();
            controller.RunLocalProjectionSmoke();
            Capture(controller);
        }

        private void ClearProjection()
        {
            var root = GameObject.Find(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }

            statusLine = "Projection root cleared.";
            diagnostics = "Removed only " + AcceptedAlphaPlayableProjectionController.GeneratedRootName + ".";
            smokeDiagnostics = string.Empty;
        }

        private static void ClearProjectionRootImmediate()
        {
            var root = GameObject.Find(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            if (root != null)
            {
                DestroyImmediate(root);
            }
        }

        private static AcceptedAlphaPlayableProjectionController EnsureController()
        {
            var root = GameObject.Find(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            if (root == null)
            {
                root = new GameObject(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Accepted Alpha Playable Projection");
            }

            var controller = root.GetComponent<AcceptedAlphaPlayableProjectionController>();
            if (controller == null)
            {
                controller = root.AddComponent<AcceptedAlphaPlayableProjectionController>();
            }

            return controller;
        }

        private void Capture(AcceptedAlphaPlayableProjectionController controller)
        {
            statusLine = controller.StatusLine;
            diagnostics = controller.LastDiagnostics;
            smokeDiagnostics = controller.LastSmokeDiagnostics;
        }
    }
}
#endif
