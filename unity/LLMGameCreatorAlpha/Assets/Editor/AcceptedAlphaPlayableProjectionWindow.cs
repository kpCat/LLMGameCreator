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
        private string selectedMarkerDetails = string.Empty;
        private string interactionPreview = string.Empty;
        private string objectiveReplayDetails = string.Empty;
        private string verificationEventLog = string.Empty;
        private string baselineStatus = "unknown";
        private string fullVerificationStatus = "not run";
        private string selectedMarkerStatus = "none";
        private string projectionStateStatus = "not initialized";
        private string genericProjectionStatus = "not run";
        private string genericSystemsStatus = "not run";
        private string genericFullPlaythroughStatus = "not run";
        private bool diagnosticsExpanded;
        private bool smokeExpanded = true;
        private bool selectedMarkerExpanded = true;
        private bool interactionPreviewExpanded = true;
        private bool objectiveReplayExpanded;
        private bool verificationEventLogExpanded;
        private int interactionSelectionIndex;
        private int objectiveSelectionIndex;

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

        public static void RunBatchmodeProjectionUsabilitySmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureController();
                controller.RefreshAcceptedBaseline();
                controller.BuildOrRefreshProjection();
                SelectAndFrame(controller.FindPlayerProxy());
                SelectAndFrame(controller.FindNextMarkerByKind("interaction", 0));
                SelectAndFrame(controller.FindNextMarkerByKind("objective", 0));
                SelectAndFrame(controller.FindDiagnosticsMarker());
                var passed = controller.RunLocalProjectionSmoke();
                var diagnostics = controller.LastDiagnostics + "\n" + controller.LastSmokeDiagnostics;
                if (passed)
                {
                    Debug.Log("GOAL120_PROJECTION_USABILITY_SMOKE_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL120_PROJECTION_USABILITY_SMOKE_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL120_PROJECTION_USABILITY_SMOKE_FAIL\n" + ex);
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
                    Debug.LogError("GOAL120_PROJECTION_USABILITY_SMOKE_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeProjectionFullVerification()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureController();
                var passed = controller.RunFullProjectionVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.InteractionPreview
                                  + "\n" + controller.ObjectiveReplayDetails
                                  + "\n" + controller.VerificationEventLog;
                if (passed)
                {
                    Debug.Log("GOAL121_FULL_PROJECTION_VERIFICATION_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL\n" + ex);
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
                    Debug.LogError("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeProjectionActionLoopSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureController();
                var passed = controller.RunFullProjectionVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.InteractionPreview
                                  + "\n" + controller.ObjectiveReplayDetails
                                  + "\n" + controller.VerificationEventLog
                                  + "\nprojectionStateStatus=" + controller.ProjectionStateStatus;
                if (passed)
                {
                    Debug.Log("GOAL122_ACTION_LOOP_SMOKE_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL122_ACTION_LOOP_SMOKE_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL122_ACTION_LOOP_SMOKE_FAIL\n" + ex);
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
                    Debug.LogError("GOAL122_ACTION_LOOP_SMOKE_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeGenericGamePackageProjectionSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureGenericController();
                var passed = controller.RunGenericPackageProjectionVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.VerificationEventLog;
                if (passed)
                {
                    Debug.Log("GOAL123_GENERIC_PACKAGE_PROJECTION_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL\n" + ex);
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
                    Debug.LogError("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeGenericGamePackageLoopSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureGenericController();
                var passed = controller.RunGenericPackageGameplayLoopVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.InteractionEffectPreview
                                  + "\n" + controller.QuestObjectiveSummary
                                  + "\n" + controller.InventorySummary
                                  + "\n" + controller.ResourceSummary
                                  + "\n" + controller.VerificationEventLog;
                if (passed)
                {
                    Debug.Log("GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL\n" + ex);
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
                    Debug.LogError("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeGenericGamePackageSystemsSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureGenericController();
                var passed = controller.RunGenericPackageSystemsLoopVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.InventorySummary
                                  + "\n" + controller.ResourceSummary
                                  + "\n" + controller.RecipePreview
                                  + "\n" + controller.RecipeApplyResult
                                  + "\n" + controller.HarvestPreview
                                  + "\n" + controller.HarvestApplyResult
                                  + "\n" + controller.TransactionPreview
                                  + "\n" + controller.EncounterPreview
                                  + "\n" + controller.CombatRoundPreview
                                  + "\n" + controller.SystemsEventLog;
                if (passed)
                {
                    Debug.Log("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL\n" + ex);
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
                    Debug.LogError("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeGenericGamePackageFullPlaythroughSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureGenericController();
                var passed = controller.RunGenericPackageFullPlaythroughVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\n" + controller.SelectedMarkerDetails
                                  + "\n" + controller.MovementPathSummary
                                  + "\n" + controller.SignInteractionResult
                                  + "\n" + controller.DialogueSummary
                                  + "\n" + controller.QuestObjectiveStatus
                                  + "\n" + controller.InventoryResourceFinalSummary
                                  + "\n" + controller.SystemsSummary
                                  + "\n" + controller.CombatSummary
                                  + "\n" + controller.EventTranscriptSummary
                                  + "\n" + controller.FinalStateSummary
                                  + "\n" + controller.VerificationEventLog;
                if (passed)
                {
                    Debug.Log("GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError("GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\n" + ex);
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
                    Debug.LogError(
                        "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        public static void RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke()
        {
            var exitCode = 0;
            try
            {
                var controller = EnsureGenericController();
                var passed = controller.RunParameterizedGamePackageFullPlaythroughVerification();
                var diagnostics = controller.LastDiagnostics
                                  + "\n" + controller.LastSmokeDiagnostics
                                  + "\npackagePath=" + controller.PackagePathFull
                                  + "\npackagePathRelative=" + controller.PackagePathRelative
                                  + "\npackagePathResolved=" + controller.PackagePathResolved
                                  + "\npackagePathUnderRepo=" + controller.PackagePathUnderRepo
                                  + "\n" + string.Join("\n", controller.SelectedMarkerDetails, controller.MovementPathSummary, controller.SignInteractionResult, controller.DialogueSummary, controller.QuestObjectiveStatus, controller.InventoryResourceFinalSummary, controller.SystemsSummary, controller.CombatSummary, controller.EventTranscriptSummary, controller.FinalStateSummary, controller.VerificationEventLog);
                if (passed)
                {
                    Debug.Log("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS\n" + diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError(
                        "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\n" + diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\n" + ex);
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
                    Debug.LogError("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL\ncleanup_failed\n" + ex);
                }

                if (Application.isBatchMode) { EditorApplication.Exit(exitCode); }
            }
        }

        private void OnEnable()
        {
            RefreshAcceptedBaseline();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawStatusArea();

            if (GUILayout.Button("Run Full Projection Verification", GUILayout.Height(32)))
            {
                RunFullProjectionVerification();
            }

            EditorGUILayout.HelpBox(
                "Manual check path: Run Full Projection Verification, then .devflow\\scripts\\clean-unity-editor-noise.cmd.",
                MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Generic GamePackage Projection", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", genericProjectionStatus);
            if (GUILayout.Button("Run Generic Package Full Playthrough Verification", GUILayout.Height(36)))
            {
                RunGenericPackageFullPlaythroughVerification();
            }
            if (GUILayout.Button("Run Generic Package Projection Verification", GUILayout.Height(28)))
            {
                RunGenericPackageProjectionVerification();
            }
            if (GUILayout.Button("Run Generic Package Gameplay Loop Verification", GUILayout.Height(32)))
            {
                RunGenericPackageGameplayLoopVerification();
            }
            if (GUILayout.Button("Run Generic Package Systems Loop Verification", GUILayout.Height(32)))
            {
                RunGenericPackageSystemsLoopVerification();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Projection Action Loop", EditorStyles.boldLabel);
            if (GUILayout.Button("Select Next Interaction Target"))
            {
                SelectNextInteractionTarget();
            }

            if (GUILayout.Button("Preview Selected Action"))
            {
                PreviewSelectedAction();
            }

            if (GUILayout.Button("Apply Preview Action To Projection State"))
            {
                ApplyPreviewActionToProjectionState();
            }

            if (GUILayout.Button("Reset Projection State"))
            {
                ResetProjectionState();
            }
            EditorGUILayout.EndVertical();

            DrawFoldoutTextPanel(ref smokeExpanded, "Smoke", smokeDiagnostics, 96f);
            DrawFoldoutTextPanel(ref selectedMarkerExpanded, "Selected Marker Details", selectedMarkerDetails, 96f);
            DrawFoldoutTextPanel(ref interactionPreviewExpanded, "Interaction Preview", interactionPreview, 96f);
            DrawFoldoutTextPanel(ref objectiveReplayExpanded, "Objective / Replay Details", objectiveReplayDetails, 96f);
            DrawFoldoutTextPanel(ref verificationEventLogExpanded, "Verification Event Log", verificationEventLog, 120f);
            DrawFoldoutTextPanel(ref diagnosticsExpanded, "Diagnostics", diagnostics, 120f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Debug / Optional Inspection", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Accepted Baseline"))
            {
                RefreshAcceptedBaseline();
            }

            if (GUILayout.Button("Build/Refresh Playable Projection"))
            {
                BuildOrRefreshPlayableProjection();
            }

            if (GUILayout.Button("Focus Projection Camera"))
            {
                FocusProjectionCamera();
            }

            if (GUILayout.Button("Select Player Proxy"))
            {
                SelectPlayerProxy();
            }

            if (GUILayout.Button("Select Next Objective"))
            {
                SelectNextObjective();
            }

            if (GUILayout.Button("Select Diagnostics Marker"))
            {
                SelectDiagnosticsMarker();
            }

            if (GUILayout.Button("Toggle/Refresh Legend"))
            {
                ToggleOrRefreshLegend();
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
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusArea()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Menu Path", MenuPath);
            EditorGUILayout.LabelField("Generated Root", AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            EditorGUILayout.LabelField("Status", statusLine);
            EditorGUILayout.LabelField("Baseline", baselineStatus);
            EditorGUILayout.LabelField("Full Verification", fullVerificationStatus);
            EditorGUILayout.LabelField("Selected Marker", selectedMarkerStatus);
            EditorGUILayout.LabelField("Projection State", projectionStateStatus);
            EditorGUILayout.LabelField("Generic Package Projection", genericProjectionStatus);
            EditorGUILayout.LabelField("Generic Package Systems", genericSystemsStatus);
            EditorGUILayout.LabelField("Generic Package Full Playthrough", genericFullPlaythroughStatus);
            EditorGUILayout.EndVertical();
        }

        private static void DrawFoldoutTextPanel(ref bool expanded, string title, string text, float maxHeight)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            expanded = EditorGUILayout.Foldout(expanded, title, true);
            if (expanded)
            {
                EditorGUILayout.TextArea(text ?? string.Empty, GUILayout.MinHeight(48f), GUILayout.MaxHeight(maxHeight));
            }
            EditorGUILayout.EndVertical();
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
            interactionSelectionIndex = 0;
            objectiveSelectionIndex = 0;
            SceneView.FrameLastActiveSceneView();
            Capture(controller);
        }

        private void FocusProjectionCamera()
        {
            var controller = EnsureController();
            SelectAndFrame(controller.gameObject);
            statusLine = "Focused projection camera on generated root.";
            Capture(controller);
        }

        private void SelectPlayerProxy()
        {
            var controller = EnsureController();
            SelectAndFrame(controller.FindPlayerProxy());
            statusLine = "Selected player proxy.";
            Capture(controller);
        }

        private void SelectNextInteractionTarget()
        {
            var controller = EnsureController();
            var marker = controller.FindNextMarkerByKind("interaction", interactionSelectionIndex);
            SelectAndFrame(marker);
            controller.SelectProjectionActionTarget(marker);
            interactionSelectionIndex++;
            statusLine = "Selected next interaction target.";
            Capture(controller);
        }

        private void PreviewSelectedAction()
        {
            var controller = EnsureController();
            controller.PreviewSelectedAction();
            Capture(controller);
        }

        private void ApplyPreviewActionToProjectionState()
        {
            var controller = EnsureController();
            controller.ApplyPreviewActionToProjectionState();
            Capture(controller);
        }

        private void ResetProjectionState()
        {
            var controller = EnsureController();
            controller.ResetProjectionState();
            Capture(controller);
        }

        private void SelectNextObjective()
        {
            var controller = EnsureController();
            SelectAndFrame(controller.FindNextMarkerByKind("objective", objectiveSelectionIndex));
            objectiveSelectionIndex++;
            statusLine = "Selected next objective.";
            Capture(controller);
        }

        private void SelectDiagnosticsMarker()
        {
            var controller = EnsureController();
            SelectAndFrame(controller.FindDiagnosticsMarker());
            statusLine = "Selected diagnostics marker.";
            Capture(controller);
        }

        private void ToggleOrRefreshLegend()
        {
            var controller = EnsureController();
            controller.ToggleOrRefreshLegend();
            SelectAndFrame(controller.gameObject);
            Capture(controller);
        }

        private void RunLocalProjectionSmoke()
        {
            var controller = EnsureController();
            controller.RunLocalProjectionSmoke();
            Capture(controller);
        }

        private void RunFullProjectionVerification()
        {
            var controller = EnsureController();
            controller.RunFullProjectionVerification();
            SelectAndFrame(controller.FindDiagnosticsMarker());
            interactionSelectionIndex = 1;
            objectiveSelectionIndex = 1;
            Capture(controller);
        }

        private void RunGenericPackageProjectionVerification()
        {
            var controller = EnsureGenericController();
            controller.RunGenericPackageProjectionVerification();
            SelectAndFrame(controller.FindFirstGenericEntityMarker() ?? controller.FindGenericProjectionRoot());
            CaptureGeneric(controller);
        }

        private void RunGenericPackageGameplayLoopVerification()
        {
            var controller = EnsureGenericController();
            controller.RunGenericPackageGameplayLoopVerification();
            SelectAndFrame(controller.FindSignEntityMarker()
                           ?? controller.FindFirstGenericEntityMarker()
                           ?? controller.FindGenericProjectionRoot());
            CaptureGeneric(controller);
        }

        private void RunGenericPackageSystemsLoopVerification()
        {
            var controller = EnsureGenericController();
            controller.RunGenericPackageSystemsLoopVerification();
            SelectAndFrame(controller.FindGenericProjectionRoot());
            CaptureGeneric(controller);
        }

        private void RunGenericPackageFullPlaythroughVerification()
        {
            var controller = EnsureGenericController();
            controller.RunGenericPackageFullPlaythroughVerification();
            SelectAndFrame(controller.FindGenericProjectionRoot());
            CaptureGeneric(controller);
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
            selectedMarkerDetails = string.Empty;
            interactionPreview = string.Empty;
            objectiveReplayDetails = string.Empty;
            verificationEventLog = string.Empty;
            baselineStatus = "unknown";
            fullVerificationStatus = "not run";
            selectedMarkerStatus = "none";
            projectionStateStatus = "not initialized";
            genericProjectionStatus = "not run";
            genericSystemsStatus = "not run";
            genericFullPlaythroughStatus = "not run";
        }

        private static void ClearProjectionRootImmediate()
        {
            var root = GameObject.Find(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            if (root != null)
            {
                DestroyImmediate(root);
            }
        }

        private static void SelectAndFrame(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            Selection.activeGameObject = obj;
            if (!Application.isBatchMode)
            {
                SceneView.FrameLastActiveSceneView();
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

        private static GenericGamePackageProjectionController EnsureGenericController()
        {
            var root = GameObject.Find(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
            if (root == null)
            {
                root = new GameObject(AcceptedAlphaPlayableProjectionController.GeneratedRootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Accepted Alpha Playable Projection");
            }

            var acceptedController = root.GetComponent<AcceptedAlphaPlayableProjectionController>();
            if (acceptedController == null)
            {
                root.AddComponent<AcceptedAlphaPlayableProjectionController>();
            }

            var controller = root.GetComponent<GenericGamePackageProjectionController>();
            if (controller == null)
            {
                controller = root.AddComponent<GenericGamePackageProjectionController>();
            }

            return controller;
        }

        private void Capture(AcceptedAlphaPlayableProjectionController controller)
        {
            statusLine = controller.StatusLine;
            diagnostics = controller.LastDiagnostics;
            smokeDiagnostics = controller.LastSmokeDiagnostics;
            selectedMarkerDetails = controller.SelectedMarkerDetails;
            interactionPreview = controller.InteractionPreview;
            objectiveReplayDetails = controller.ObjectiveReplayDetails;
            verificationEventLog = controller.VerificationEventLog;
            baselineStatus = "loaded=" + controller.AcceptedBaselineReady
                             + "; baselineId=" + controller.BaselineId
                             + "; manualGate=" + controller.ManualGateStatus;
            fullVerificationStatus = controller.LastFullVerificationPassed ? "passed" : "not passed";
            selectedMarkerStatus = "id=" + EmptyAsNone(controller.SelectedMarkerId)
                                   + "; kind=" + EmptyAsNone(controller.SelectedMarkerKind);
            projectionStateStatus = controller.ProjectionStateStatus;
        }

        private void CaptureGeneric(GenericGamePackageProjectionController controller)
        {
            statusLine = controller.StatusLine;
            diagnostics = controller.LastDiagnostics;
            smokeDiagnostics = controller.LastSmokeDiagnostics;
            selectedMarkerDetails = controller.SelectedMarkerDetails;
            interactionPreview = string.IsNullOrWhiteSpace(controller.InteractionEffectPreview)
                ? "Generic package projection is read-only; no Runtime action is executed."
                : controller.InteractionEffectPreview;
            objectiveReplayDetails = "samplePackagePath="
                                     + controller.PackagePathRelative
                                     + "\npackagePath="
                                     + controller.PackagePathFull
                                     + "\npackagePathResolved="
                                     + controller.PackagePathResolved
                                     + "\npackagePathUnderRepo="
                                     + controller.PackagePathUnderRepo
                                     + "\npackageId="
                                     + controller.PackageId
                                     + "\npackageTitle="
                                     + controller.PackageTitle
                                     + "\nmapId="
                                     + controller.MapId
                                     + "\nmapSize="
                                     + controller.MapWidth
                                     + "x"
                                     + controller.MapHeight
                                     + "\nentityCount="
                                     + controller.EntityCount
                                     + "\nitemCount="
                                     + controller.ItemCount
                                     + "\nselectedDialogueId="
                                     + EmptyAsNone(controller.SelectedDialogueId)
                                     + "\nselectedQuestId="
                                     + EmptyAsNone(controller.SelectedQuestId)
                                     + "\nquestObjectiveSummary="
                                     + EmptyAsNone(controller.QuestObjectiveSummary)
                                     + "\ninventorySummary="
                                     + EmptyAsNone(controller.InventorySummary)
                                     + "\nresourceSummary="
                                     + EmptyAsNone(controller.ResourceSummary)
                                     + "\nappliedInteractionCount="
                                     + controller.AppliedInteractionCount
                                     + "\nstartedQuestCount="
                                     + controller.StartedQuestCount
                                     + "\nrecipePreview="
                                     + EmptyAsNone(controller.RecipePreview)
                                     + "\nrecipeApplyResult="
                                     + EmptyAsNone(controller.RecipeApplyResult)
                                     + "\nharvestPreview="
                                     + EmptyAsNone(controller.HarvestPreview)
                                     + "\nharvestApplyResult="
                                     + EmptyAsNone(controller.HarvestApplyResult)
                                     + "\ntransactionPreview="
                                     + EmptyAsNone(controller.TransactionPreview)
                                     + "\nencounterPreview="
                                     + EmptyAsNone(controller.EncounterPreview)
                                     + "\ncombatRoundPreview="
                                     + EmptyAsNone(controller.CombatRoundPreview)
                                     + "\nfullPlaythroughStatus="
                                     + EmptyAsNone(controller.FullPlaythroughStatus)
                                     + "\nmovementPathSummary="
                                     + EmptyAsNone(controller.MovementPathSummary)
                                     + "\nsignInteractionResult="
                                     + EmptyAsNone(controller.SignInteractionResult)
                                     + "\ndialogueSummary="
                                     + EmptyAsNone(controller.DialogueSummary)
                                     + "\nquestObjectiveStatus="
                                     + EmptyAsNone(controller.QuestObjectiveStatus)
                                     + "\ninventoryResourceFinalSummary="
                                     + EmptyAsNone(controller.InventoryResourceFinalSummary)
                                     + "\nsystemsSummary="
                                     + EmptyAsNone(controller.SystemsSummary)
                                     + "\ncombatSummary="
                                     + EmptyAsNone(controller.CombatSummary)
                                     + "\neventTranscriptSummary="
                                     + EmptyAsNone(controller.EventTranscriptSummary)
                                     + "\nfinalStateSummary="
                                     + EmptyAsNone(controller.FinalStateSummary);
            verificationEventLog = controller.VerificationEventLog;
            genericProjectionStatus =
                controller.LastGenericFullPlaythroughVerificationPassed
                    ? "full playthrough passed"
                    : controller.LastGenericSystemsVerificationPassed
                    ? "systems passed"
                    : controller.LastGenericLoopVerificationPassed
                    ? "loop passed"
                    : controller.LastVerificationPassed ? "projection passed" : "not passed";
            genericSystemsStatus =
                controller.LastGenericSystemsVerificationPassed ? "systems passed" : "not passed";
            genericFullPlaythroughStatus =
                controller.LastGenericFullPlaythroughVerificationPassed ? "passed" : "not passed";
            fullVerificationStatus = "genericPackageProjection="
                                     + (controller.LastVerificationPassed ? "passed" : "not passed")
                                     + "; genericPackageLoop="
                                     + (controller.LastGenericLoopVerificationPassed ? "passed" : "not passed")
                                     + "; genericPackageSystems="
                                     + (controller.LastGenericSystemsVerificationPassed ? "passed" : "not passed")
                                     + "; genericPackageFullPlaythrough="
                                     + (controller.LastGenericFullPlaythroughVerificationPassed
                                         ? "passed"
                                         : "not passed");
            selectedMarkerStatus = "id=" + EmptyAsNone(controller.SelectedMarkerId)
                                   + "; kind=" + EmptyAsNone(controller.SelectedMarkerKind);
        }

        private static string EmptyAsNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
#endif
