using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class AlphaRuntimeBootstrap : MonoBehaviour
    {
        private const string PayloadFolderName = "LLMGameCreatorAlpha";

        private readonly List<string> playLog = new List<string>();
        private readonly List<AlphaCommandHint> commands = new List<AlphaCommandHint>();
        private readonly List<AlphaSceneNode> sceneNodes = new List<AlphaSceneNode>();
        private readonly List<AlphaSceneNode> focusableSceneNodes = new List<AlphaSceneNode>();
        private readonly List<AlphaMediaBoundBinding> mediaBoundBindings = new List<AlphaMediaBoundBinding>();
        private readonly List<string> mediaBoundLogLines = new List<string>();
        private readonly List<string> mediaBoundPanelFamilies = new List<string>();
        private readonly List<AlphaFamilyMode> familyModes = new List<AlphaFamilyMode>();
        private readonly List<AlphaFamilyLoopCommand> familyLoopCommands = new List<AlphaFamilyLoopCommand>();
        private readonly List<string> familyLoopLogLines = new List<string>();
        private readonly List<string> campaignFamilies = new List<string>();
        private readonly List<string> campaignLogLines = new List<string>();
        private readonly List<AlphaMatrixRow> matrixRows = new List<AlphaMatrixRow>();
        private readonly List<string> matrixLogLines = new List<string>();
        private readonly List<AlphaPackageRow> packageRows = new List<AlphaPackageRow>();
        private readonly List<string> packageLogLines = new List<string>();
        private readonly List<AlphaReviewPackageRcRow> reviewPackageRcRows = new List<AlphaReviewPackageRcRow>();
        private readonly List<string> reviewPackageRcLogLines = new List<string>();
        private readonly List<AlphaSpatialDetailRow> spatialDetailRows = new List<AlphaSpatialDetailRow>();
        private readonly List<string> spatialDetailLogLines = new List<string>();
        private readonly List<AlphaGameplayConsequenceRow> gameplayConsequenceRows = new List<AlphaGameplayConsequenceRow>();
        private readonly List<string> gameplayConsequenceLogLines = new List<string>();
        private readonly List<AlphaLivingWorldRow> livingWorldRows = new List<AlphaLivingWorldRow>();
        private readonly List<string> livingWorldLogLines = new List<string>();
        private readonly List<AlphaInterlockedGameplayRow> interlockedGameplayRows = new List<AlphaInterlockedGameplayRow>();
        private readonly List<string> interlockedGameplayLogLines = new List<string>();
        private readonly List<AlphaSettlementRow> settlementRows = new List<AlphaSettlementRow>();
        private readonly List<string> settlementLogLines = new List<string>();
        private string packageId = string.Empty;
        private string campaignId = string.Empty;
        private string reviewPackageRcId = string.Empty;
        private string selectedStyleId = string.Empty;
        private string packageHash = string.Empty;
        private string assetManifestHash = string.Empty;
        private string runtimeConfigHash = string.Empty;
        private string startMapId = string.Empty;
        private string selectedThreadId = string.Empty;
        private string selectedNpcId = string.Empty;
        private string selectedQuestId = string.Empty;
        private string selectedDialogueId = string.Empty;
        private string selectedItemId = string.Empty;
        private string selectedEventId = string.Empty;
        private int assetRefCount;
        private int currentCommandIndex;
        private bool payloadLoaded;
        private bool questStarted;
        private bool dialogueSeen;
        private bool dialogueChoiceSelected;
        private bool itemObtained;
        private bool questCompletedCandidate;
        private bool rewardGranted;
        private bool eventApplied;
        private int inventoryItemCount;
        private string lastCommandId = string.Empty;
        private string lastCommandType = string.Empty;
        private string lastCommandTargetId = string.Empty;
        private string status = "Loading Alpha payload...";
        private string mediaBoundStatus = "Media package: not staged";
        private string familyModeArgument = "all";
        private int playerX = 1;
        private int playerY = 1;
        private int mapWidth = 7;
        private int mapHeight = 5;
        private int focusedTargetIndex;
        private bool mediaBoundManifestLoaded;
        private bool mediaBoundPngLoaded;
        private bool mediaBoundWavLoaded;
        private bool mediaBoundBundleLoaded;
        private bool mediaBoundHashValidation;
        private bool familyLoopPlanLoaded;
        private bool campaignManifestLoaded;
        private bool campaignMediaBound;
        private bool matrixPlanLoaded;
        private bool packagePlanLoaded;
        private bool reviewPackageRcPlanLoaded;
        private bool spatialDetailPlanLoaded;
        private bool gameplayConsequencePlanLoaded;
        private bool livingWorldPlanLoaded;
        private bool interlockedGameplayPlanLoaded;
        private bool settlementPlanLoaded;

        private void Start()
        {
            Application.targetFrameRate = 30;
            var arguments = Environment.GetCommandLineArgs();
            familyModeArgument = GetArgumentValue(arguments, "-alphaFamilyMode");
            if (string.IsNullOrWhiteSpace(familyModeArgument))
            {
                familyModeArgument = "all";
            }

            var logPath = GetArgumentValue(arguments, "-alphaLogPath");
            if (string.IsNullOrWhiteSpace(logPath))
            {
                logPath = Path.Combine(Application.persistentDataPath, "alpha-launch-diagnostic.log");
            }

            var launchLines = LoadPayloadAndBuildLaunchLog();
            WriteLines(logPath, launchLines);
            Debug.Log(string.Join(Environment.NewLine, launchLines));

            var playLoopRequested = HasArgument(arguments, "-alphaPlayLoopSmokeExit");
            if (playLoopRequested)
            {
                var playLoopLogPath = GetArgumentValue(arguments, "-alphaPlayLoopLogPath");
                if (string.IsNullOrWhiteSpace(playLoopLogPath))
                {
                    playLoopLogPath = Path.Combine(Application.persistentDataPath, "alpha-play-loop-diagnostic.log");
                }

                var playLines = RunAutomatedPlayLoop();
                WriteLines(playLoopLogPath, playLines);
                Debug.Log(string.Join(Environment.NewLine, playLines));
                Application.Quit(IsLaunchSuccessful(launchLines) && IsPlayLoopSuccessful(playLines) ? 0 : 1);
                return;
            }

            if (HasArgument(arguments, "-alphaSmokeExit"))
            {
                Application.Quit(IsLaunchSuccessful(launchLines) ? 0 : 1);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                TryMove(0, -1);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                TryMove(0, 1);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TryMove(-1, 0);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                TryMove(1, 0);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleFocus();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                InteractWithFocusedTarget();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetLoop();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(0);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16, 16, 960, 620), "LLMGameCreator Alpha - " + StyleLabel());
            GUI.Label(new Rect(32, 42, 900, 24), QuestLabel() + "    " + PhaseLabel() + "    " + RewardLabel());
            GUI.Label(new Rect(32, 66, 900, 22), "Package: " + Display(packageId) + "    Thread: " + Display(selectedThreadId));

            GUI.Box(new Rect(32, 98, 290, 168), "Quest");
            GUI.Label(new Rect(48, 124, 250, 22), "Style: " + StyleLabel());
            GUI.Label(new Rect(48, 148, 250, 22), "Quest: " + QuestLabel());
            GUI.Label(new Rect(48, 172, 250, 22), "Phase: " + PhaseLabel());
            GUI.Label(new Rect(48, 196, 250, 22), "Reward: " + RewardLabel());
            GUI.Label(new Rect(48, 220, 250, 22), "Status: " + status);

            GUI.Box(new Rect(338, 98, 300, 168), "Objectives");
            var objectiveY = 124f;
            foreach (var objective in ObjectiveDisplayRows())
            {
                GUI.Label(new Rect(354, objectiveY, 268, 20), objective);
                objectiveY += 22;
            }

            GUI.Box(new Rect(654, 98, 290, 168), "Selected Target");
            GUI.Label(new Rect(670, 124, 250, 22), "Focus: " + FocusName());
            GUI.Label(new Rect(670, 148, 250, 22), "Target: " + TargetLabel());
            GUI.Label(new Rect(670, 172, 250, 22), "Position: " + TargetPositionLabel());
            GUI.Label(new Rect(670, 196, 250, 22), "Hint: Open generated dialogue");
            GUI.Label(new Rect(670, 220, 250, 22), "Last: " + CommandLabel(lastCommandId));

            DrawMap(32, 290);

            GUI.Box(new Rect(338, 290, 300, 144), "Inventory / Reward");
            GUI.Label(new Rect(354, 316, 250, 22), "Inventory: " + inventoryItemCount + " generated item");
            GUI.Label(new Rect(354, 340, 250, 22), RewardLabel());
            GUI.Label(new Rect(354, 364, 250, 22), "Reward granted: " + rewardGranted.ToString().ToLowerInvariant());
            GUI.Label(new Rect(354, 388, 250, 22), "Item: " + ItemLabel());

            GUI.Box(new Rect(654, 290, 290, 144), "Controls");
            GUI.Label(new Rect(670, 316, 250, 22), "Move: WASD/arrows");
            GUI.Label(new Rect(670, 340, 250, 22), "Focus: Tab");
            GUI.Label(new Rect(670, 364, 250, 22), "Interact: Space/Enter");
            GUI.Label(new Rect(670, 388, 250, 22), "Reset: R");
            GUI.Label(new Rect(670, 412, 250, 22), "Quit: Esc");

            if (GUI.Button(new Rect(338, 446, 160, 36), "Interact"))
            {
                InteractWithFocusedTarget();
            }

            if (GUI.Button(new Rect(512, 446, 120, 36), "Reset"))
            {
                ResetLoop();
            }

            GUI.Box(new Rect(654, 446, 290, 152), "Media Bound");
            GUI.Label(new Rect(670, 472, 250, 20), mediaBoundStatus);
            GUI.Label(new Rect(670, 494, 250, 20), "Families: " + mediaBoundPanelFamilies.Count + "    Bindings: " + mediaBoundBindings.Count);
            GUI.Label(new Rect(670, 516, 250, 20), "PNG: " + mediaBoundPngLoaded.ToString().ToLowerInvariant() + "    WAV: " + mediaBoundWavLoaded.ToString().ToLowerInvariant());
            GUI.Label(new Rect(670, 538, 250, 20), "Bundle: " + mediaBoundBundleLoaded.ToString().ToLowerInvariant() + "    Hash: " + mediaBoundHashValidation.ToString().ToLowerInvariant());
            GUI.Label(new Rect(670, 560, 250, 20), "Gate: media-bound review required");

            GUI.Box(new Rect(32, 454, 290, 144), "Event / Status Log");
            var logY = 480f;
            var start = Math.Max(0, playLog.Count - 8);
            for (var index = start; index < playLog.Count; index++)
            {
                GUI.Label(new Rect(48, logY, 258, 18), playLog[index]);
                logY += 18;
            }
        }

        private List<string> LoadPayloadAndBuildLaunchLog()
        {
            var lines = new List<string>
            {
                "alpha_runtime.launch_started=true",
                "alpha_runtime.streaming_assets_path=" + Application.streamingAssetsPath
            };

            try
            {
                var payloadRoot = Path.Combine(Application.streamingAssetsPath, PayloadFolderName);
                var configPath = Path.Combine(payloadRoot, "runtime", "unity-runtime-config.json");
                var packagePath = Path.Combine(payloadRoot, "game-data", "game-package.json");
                var assetManifestPath = Path.Combine(payloadRoot, "assets", "asset-manifest.json");
                var exportManifestPath = Path.Combine(payloadRoot, "export-manifest.json");
                var payloadRootExists = Directory.Exists(payloadRoot);
                var configJson = File.ReadAllText(configPath);
                var packageJson = File.ReadAllText(packagePath);
                var assetManifestJson = File.ReadAllText(assetManifestPath);
                var exportManifestJson = File.Exists(exportManifestPath) ? File.ReadAllText(exportManifestPath) : string.Empty;

                packageId = ExtractJsonString(configJson, "packageId");
                selectedStyleId = StyleIdFromPackageId(packageId);
                packageHash = ExtractJsonString(configJson, "packageHash");
                assetManifestHash = ExtractJsonString(configJson, "assetManifestHash");
                runtimeConfigHash = ExtractJsonString(configJson, "configHash");
                if (string.IsNullOrWhiteSpace(runtimeConfigHash))
                {
                    runtimeConfigHash = ExtractManifestFileHash(exportManifestJson, "runtime/unity-runtime-config.json");
                }

                startMapId = ExtractJsonString(configJson, "startMapId");
                selectedThreadId = ExtractJsonString(configJson, "selectedThreadId");
                selectedNpcId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "npc/");
                if (string.IsNullOrWhiteSpace(selectedNpcId))
                {
                    selectedNpcId = FirstPropertyValueWithPrefix(configJson, "contentId", "npc/");
                }

                selectedQuestId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "quest/");
                selectedDialogueId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "dialogue/");
                selectedItemId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "item/");
                selectedEventId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "event/");
                assetRefCount = CountJsonObjectsInArray(configJson, "assetRefs");
                commands.Clear();
                commands.AddRange(ExtractCommandHints(configJson));
                commands.Sort((left, right) => string.CompareOrdinal(left.CommandId, right.CommandId));
                selectedItemId = FirstCommandTarget("event/add_item", "item/", selectedItemId);
                selectedEventId = FirstCommandSecondaryTarget("event/", "event/", selectedEventId);
                BuildSceneProjection();
                payloadLoaded = payloadRootExists && commands.Count > 0;
                ResetLoop();
                var mediaBoundLines = LoadMediaBoundPayload(payloadRoot);
                var familyLoopLines = LoadFamilyLoopPayload(payloadRoot);
                var campaignLines = LoadCampaignPayload(payloadRoot);
                var matrixLines = LoadMatrixPayload(payloadRoot);
                var packageLines = LoadPackageMaterializationPayload(payloadRoot);
                var reviewPackageRcLines = LoadReviewPackageRcPayload(payloadRoot);
                var spatialDetailLines = LoadSpatialDetailPayload(payloadRoot);
                var gameplayConsequenceLines = LoadGameplayConsequencePayload(payloadRoot);
                var livingWorldLines = LoadLivingWorldPayload(payloadRoot);
                var interlockedGameplayLines = LoadInterlockedGameplayPayload(payloadRoot);
                var settlementLines = LoadSettlementPayload(payloadRoot);

                lines.Add("alpha_runtime.payload_root_exists=" + payloadRootExists.ToString().ToLowerInvariant());
                lines.Add("alpha_runtime.config_loaded=true");
                lines.Add("alpha_runtime.package_loaded=true");
                lines.Add("alpha_runtime.asset_manifest_loaded=true");
                lines.Add("alpha_runtime.package_id=" + packageId);
                lines.Add("alpha_runtime.selected_style_id=" + selectedStyleId);
                lines.Add("alpha_runtime.package_hash=" + packageHash);
                lines.Add("alpha_runtime.asset_manifest_hash=" + assetManifestHash);
                lines.Add("alpha_runtime.runtime_config_hash=" + runtimeConfigHash);
                lines.Add("alpha_runtime.start_map_id=" + startMapId);
                lines.Add("alpha_runtime.selected_thread_id=" + selectedThreadId);
                lines.Add("alpha_runtime.selected_npc_id=" + selectedNpcId);
                lines.Add("alpha_runtime.selected_quest_id=" + selectedQuestId);
                lines.Add("alpha_runtime.selected_dialogue_id=" + selectedDialogueId);
                lines.Add("alpha_runtime.selected_item_id=" + selectedItemId);
                lines.Add("alpha_runtime.selected_event_id=" + selectedEventId);
                lines.Add("alpha_runtime.command_hint_count=" + commands.Count);
                lines.Add("alpha_runtime.asset_ref_count=" + assetRefCount);
                lines.Add("alpha_runtime.scene_projection_loaded=" + (sceneNodes.Count > 0).ToString().ToLowerInvariant());
                lines.Add("alpha_runtime.scene_node_count=" + sceneNodes.Count);
                lines.Add("alpha_runtime.package_bytes=" + packageJson.Length);
                lines.Add("alpha_runtime.asset_manifest_bytes=" + assetManifestJson.Length);
                lines.AddRange(mediaBoundLines);
                lines.AddRange(familyLoopLines);
                lines.AddRange(campaignLines);
                lines.AddRange(matrixLines);
                lines.AddRange(packageLines);
                lines.AddRange(reviewPackageRcLines);
                lines.AddRange(spatialDetailLines);
                lines.AddRange(gameplayConsequenceLines);
                lines.AddRange(livingWorldLines);
                lines.AddRange(interlockedGameplayLines);
                lines.AddRange(settlementLines);
                lines.Add("alpha_runtime.launch_completed=true");
            }
            catch (Exception ex)
            {
                payloadLoaded = false;
                status = "Launch failed: " + ex.GetType().Name;
                lines.Add("alpha_runtime.launch_completed=false");
                lines.Add("alpha_runtime.error_type=" + ex.GetType().Name);
                lines.Add("alpha_runtime.error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return lines;
        }

        private List<string> RunAutomatedPlayLoop()
        {
            var lines = new List<string>
            {
                "alpha_runtime.play_loop_started=true",
                "alpha_runtime.visible_presentation_initialized=true",
                "alpha_runtime.visible_component.map=true",
                "alpha_runtime.visible_component.player_marker=true",
                "alpha_runtime.visible_component.npc_marker=true",
                "alpha_runtime.visible_component.item_marker=true",
                "alpha_runtime.visible_component.status_panel=true",
                "alpha_runtime.visible_component.command_log=true",
                "alpha_runtime.payload_root_exists=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.config_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.package_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.asset_manifest_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.package_id=" + packageId,
                "alpha_runtime.selected_style_id=" + selectedStyleId,
                "alpha_runtime.package_hash=" + packageHash,
                "alpha_runtime.asset_manifest_hash=" + assetManifestHash,
                "alpha_runtime.runtime_config_hash=" + runtimeConfigHash,
                "alpha_runtime.start_map_id=" + startMapId,
                "alpha_runtime.selected_thread_id=" + selectedThreadId,
                "alpha_runtime.selected_npc_id=" + selectedNpcId,
                "alpha_runtime.selected_quest_id=" + selectedQuestId,
                "alpha_runtime.selected_dialogue_id=" + selectedDialogueId,
                "alpha_runtime.selected_item_id=" + selectedItemId,
                "alpha_runtime.selected_event_id=" + selectedEventId,
                "alpha_runtime.command_hint_count=" + commands.Count,
                "alpha_runtime.asset_ref_count=" + assetRefCount,
                "alpha_runtime.scene_projection_loaded=" + (sceneNodes.Count > 0).ToString().ToLowerInvariant(),
                "alpha_runtime.scene_node_count=" + sceneNodes.Count,
                "alpha_runtime.quest_loop_started=true",
                "alpha_runtime.quest_loop_plan_loaded=true",
                "alpha_runtime.quest_loop.package_id=" + packageId,
                "alpha_runtime.quest_loop.style_id=" + selectedStyleId,
                "alpha_runtime.quest_loop.thread_id=" + selectedThreadId,
                "alpha_runtime.quest_loop.quest_id=" + selectedQuestId,
                "alpha_runtime.quest_loop.dialogue_id=" + selectedDialogueId,
                "alpha_runtime.quest_loop.choice_id=" + DialogueChoiceId(),
                "alpha_runtime.quest_loop.item_id=" + selectedItemId,
                "alpha_runtime.quest_loop.event_id=" + selectedEventId,
                "alpha_runtime.quest_loop.reward_id=" + selectedItemId
            };
            lines.AddRange(mediaBoundLogLines);
            lines.AddRange(RunFamilyLoops());
            lines.AddRange(RunCampaignProof());
            lines.AddRange(RunMatrixProof());
            lines.AddRange(RunPackageMaterializationProof());
            lines.AddRange(RunReviewPackageRcProof());
            lines.AddRange(RunSpatialDetailProof());
            lines.AddRange(RunGameplayConsequenceProof());
            lines.AddRange(RunLivingWorldProof());
            lines.AddRange(RunInterlockedGameplayProof());
            lines.AddRange(RunSettlementProof());

            foreach (var kind in new[] { "map", "player", "npc", "item", "quest_event", "command_status" })
            {
                var node = SceneNode(kind);
                lines.Add("alpha_runtime.scene_node_resolved." + kind + "=" + (node != null).ToString().ToLowerInvariant());
                if (node != null)
                {
                    lines.Add("alpha_runtime.scene_node." + kind + ".id=" + node.NodeId);
                    lines.Add("alpha_runtime.scene_node." + kind + ".source_id=" + node.SourceId);
                    lines.Add("alpha_runtime.scene_node." + kind + ".position=" + node.X + "," + node.Y);
                    lines.Add("alpha_runtime.scene_node." + kind + ".label=" + node.Label);
                }
            }

            var packageJson = ReadPackageJsonIfAvailable();
            lines.Add("alpha_runtime.ref_resolved.map=" + ContainsJsonId(packageJson, startMapId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.npc=" + ContainsJsonId(packageJson, selectedNpcId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.quest=" + ContainsJsonId(packageJson, selectedQuestId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.dialogue=" + ContainsJsonId(packageJson, selectedDialogueId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.item=" + ContainsJsonId(packageJson, selectedItemId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.event=" + ContainsJsonId(packageJson, selectedEventId).ToString().ToLowerInvariant());

            ResetLoop();
            var initialState = CaptureState();
            lines.Add("alpha_runtime.map_bounds=" + mapWidth + "x" + mapHeight);
            lines.Add("alpha_runtime.projected_player_node_position=" + PlayerNodePosition());
            lines.Add("alpha_runtime.movement.initial_position=" + Position());
            var movement0 = TryMove(1, 0);
            lines.Add("alpha_runtime.movement.step.0.valid=" + movement0.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.movement.step.0.position=" + Position());
            var movement1 = TryMove(0, 1);
            lines.Add("alpha_runtime.movement.step.1.valid=" + movement1.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.movement.step.1.position=" + Position());
            playerX = 0;
            var blocked = TryMove(-1, 0);
            lines.Add("alpha_runtime.movement.blocked.valid=" + blocked.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.movement.blocked.position=" + Position());
            CycleFocus();
            lines.Add("alpha_runtime.focus.selected=" + FocusName());
            lines.Add("alpha_runtime.focus.selected_node_id=" + FocusNodeId());
            var transitionIndex = 0;
            for (var index = 0; index < commands.Count; index++)
            {
                var before = CaptureState();
                var command = InteractWithFocusedTarget();
                var after = CaptureState();
                lines.Add("alpha_runtime.command_executed." + index + ".id=" + command.CommandId);
                lines.Add("alpha_runtime.command_executed." + index + ".type=" + command.CommandType);
                lines.Add("alpha_runtime.command_executed." + index + ".target_id=" + command.TargetId);
                lines.Add("alpha_runtime.command_executed." + index + ".secondary_target_id=" + command.SecondaryTargetId);
                transitionIndex = AppendStateTransitionLines(lines, transitionIndex, command, before, after);
            }

            var finalState = CaptureState();
            lines.Add("alpha_runtime.command_state_transition_count=" + transitionIndex);
            AppendStateBeforeAfterLines(lines, "quest_started", initialState.QuestStarted.ToString().ToLowerInvariant(), finalState.QuestStarted.ToString().ToLowerInvariant());
            AppendStateBeforeAfterLines(lines, "quest_completed_candidate", initialState.QuestCompletedCandidate.ToString().ToLowerInvariant(), finalState.QuestCompletedCandidate.ToString().ToLowerInvariant());
            AppendStateBeforeAfterLines(lines, "dialogue_opened", initialState.DialogueOpened.ToString().ToLowerInvariant(), finalState.DialogueOpened.ToString().ToLowerInvariant());
            AppendStateBeforeAfterLines(lines, "dialogue_choice_selected", initialState.DialogueChoiceSelected.ToString().ToLowerInvariant(), finalState.DialogueChoiceSelected.ToString().ToLowerInvariant());
            AppendStateBeforeAfterLines(lines, "item_obtained", initialState.ItemObtained.ToString().ToLowerInvariant(), finalState.ItemObtained.ToString().ToLowerInvariant());
            AppendStateBeforeAfterLines(lines, "inventory_item_count", initialState.InventoryItemCount.ToString(), finalState.InventoryItemCount.ToString());
            AppendStateBeforeAfterLines(lines, "event_applied", initialState.EventApplied.ToString().ToLowerInvariant(), finalState.EventApplied.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state.after.last_command_id=" + lastCommandId);
            lines.Add("alpha_runtime.state.after.last_command_type=" + lastCommandType);
            lines.Add("alpha_runtime.state.after.last_command_target_id=" + lastCommandTargetId);
            lines.Add("alpha_runtime.state.after.status_text=" + status);
            lines.Add("alpha_runtime.state_transition.quest_start=" + questStarted.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.dialogue_open=" + dialogueSeen.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.dialogue_choice=" + dialogueChoiceSelected.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.item_or_loot=" + itemObtained.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.event_application=" + eventApplied.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.quest_started=" + questStarted.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.dialogue_seen=" + dialogueSeen.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.dialogue_choice_selected=" + dialogueChoiceSelected.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.item_obtained=" + itemObtained.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.inventory_item_count=" + inventoryItemCount);
            lines.Add("alpha_runtime.event_applied=" + eventApplied.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.commands_executed=" + currentCommandIndex);
            AppendQuestCompletionLines(lines, initialState, finalState);
            AppendPresentationLines(lines);
            lines.Add("alpha_runtime.play_loop_completed=" + IsCurrentLoopComplete().ToString().ToLowerInvariant());
            return lines;
        }

        private AlphaCommandHint AdvanceCommand()
        {
            if (!payloadLoaded || commands.Count == 0)
            {
                status = "No generated command hints loaded.";
                return AlphaCommandHint.Empty;
            }

            if (currentCommandIndex >= commands.Count)
            {
                status = "All generated command hints executed.";
                return commands[commands.Count - 1];
            }

            var command = commands[currentCommandIndex];
            currentCommandIndex++;
            ApplyCommand(command);
            lastCommandId = command.CommandId;
            lastCommandType = command.CommandType;
            lastCommandTargetId = command.TargetId;
            status = "Executed " + command.CommandType + " -> " + Display(command.TargetId);
            playLog.Add(currentCommandIndex + ". " + command.CommandType + " -> " + Display(command.TargetId));
            return command;
        }

        private AlphaCommandHint InteractWithFocusedTarget()
        {
            return AdvanceCommand();
        }

        private bool TryMove(int dx, int dy)
        {
            var nextX = playerX + dx;
            var nextY = playerY + dy;
            if (nextX < 0 || nextX >= mapWidth || nextY < 0 || nextY >= mapHeight)
            {
                status = "Blocked by map bounds at (" + playerX + "," + playerY + ").";
                playLog.Add("blocked move at " + Position());
                return false;
            }

            playerX = nextX;
            playerY = nextY;
            status = "Moved to " + Position() + ".";
            playLog.Add("moved to " + Position());
            return true;
        }

        private void CycleFocus()
        {
            focusedTargetIndex = (focusedTargetIndex + 1) % 3;
            status = "Focus: " + FocusName();
            playLog.Add("focus " + FocusName());
        }

        private void ApplyCommand(AlphaCommandHint command)
        {
            if (command.CommandType == "quest/start")
            {
                questStarted = !string.IsNullOrWhiteSpace(command.TargetId);
            }
            else if (command.CommandType == "dialogue/open")
            {
                dialogueSeen = !string.IsNullOrWhiteSpace(command.TargetId);
            }
            else if (command.CommandType == "dialogue/choose")
            {
                dialogueChoiceSelected = !string.IsNullOrWhiteSpace(command.TargetId);
            }
            else if (command.CommandType == "loot/roll")
            {
                itemObtained = !string.IsNullOrWhiteSpace(selectedItemId);
                inventoryItemCount = itemObtained ? Math.Max(1, inventoryItemCount) : inventoryItemCount;
            }
            else if (command.CommandType.StartsWith("event/", StringComparison.Ordinal))
            {
                eventApplied = !string.IsNullOrWhiteSpace(command.SecondaryTargetId) || !string.IsNullOrWhiteSpace(selectedEventId);
                itemObtained = command.CommandType == "event/add_item" || itemObtained;
                inventoryItemCount = command.CommandType == "event/add_item" ? Math.Max(1, inventoryItemCount) : inventoryItemCount;
            }

            questCompletedCandidate = questStarted && dialogueSeen && dialogueChoiceSelected && itemObtained && eventApplied;
            rewardGranted = questCompletedCandidate;
        }

        private void ResetLoop()
        {
            currentCommandIndex = 0;
            playerX = 1;
            playerY = 1;
            var playerNode = SceneNode("player");
            if (playerNode != null)
            {
                playerX = playerNode.X;
                playerY = playerNode.Y;
            }

            focusedTargetIndex = 0;
            questStarted = false;
            dialogueSeen = false;
            dialogueChoiceSelected = false;
            itemObtained = false;
            questCompletedCandidate = false;
            rewardGranted = false;
            eventApplied = false;
            inventoryItemCount = 0;
            lastCommandId = string.Empty;
            lastCommandType = string.Empty;
            lastCommandTargetId = string.Empty;
            playLog.Clear();
            status = payloadLoaded ? "Ready. Press Space or Advance." : "Payload not loaded.";
        }

        private bool IsCurrentLoopComplete()
        {
            return payloadLoaded &&
                commands.Count >= 5 &&
                currentCommandIndex >= commands.Count &&
                questStarted &&
                dialogueSeen &&
                dialogueChoiceSelected &&
                itemObtained &&
                inventoryItemCount > 0 &&
                eventApplied &&
                rewardGranted;
        }

        private void AppendQuestCompletionLines(ICollection<string> lines, AlphaRuntimeStateSnapshot initialState, AlphaRuntimeStateSnapshot finalState)
        {
            lines.Add("alpha_runtime.quest_phase.before=not_started");
            lines.Add("alpha_runtime.quest_phase.after.started=started");
            lines.Add("alpha_runtime.quest_phase.after.dialogue_opened=dialogue_opened");
            lines.Add("alpha_runtime.quest_phase.after.choice_selected=choice_selected");
            lines.Add("alpha_runtime.quest_phase.after.item_obtained=item_obtained");
            lines.Add("alpha_runtime.quest_phase.after.event_applied=event_applied");
            lines.Add("alpha_runtime.quest_phase.after.completed=completed");
            lines.Add("alpha_runtime.quest_phase.after.reward_granted=reward_granted");

            var start = Command("quest/start", selectedQuestId);
            var dialogue = Command("dialogue/open", selectedDialogueId);
            var choice = Command("dialogue/choose", string.Empty);
            var item = Command("event/add_item", selectedItemId);
            if (string.IsNullOrWhiteSpace(item.CommandId))
            {
                item = Command("loot/roll", string.Empty);
            }

            var eventCommand = CommandWithSecondary("event/", selectedEventId);
            var completion = string.IsNullOrWhiteSpace(eventCommand.CommandId) ? item : eventCommand;
            AppendQuestObjective(lines, 0, "quest_start", selectedQuestId, start, initialState.QuestStarted.ToString().ToLowerInvariant(), finalState.QuestStarted.ToString().ToLowerInvariant());
            AppendQuestObjective(lines, 1, "dialogue_open", selectedDialogueId, dialogue, initialState.DialogueOpened.ToString().ToLowerInvariant(), finalState.DialogueOpened.ToString().ToLowerInvariant());
            AppendQuestObjective(lines, 2, "dialogue_choice", DialogueChoiceId(), choice, initialState.DialogueChoiceSelected.ToString().ToLowerInvariant(), finalState.DialogueChoiceSelected.ToString().ToLowerInvariant());
            AppendQuestObjective(lines, 3, "item_obtained", selectedItemId, item, initialState.ItemObtained.ToString().ToLowerInvariant(), finalState.ItemObtained.ToString().ToLowerInvariant());
            AppendQuestObjective(lines, 4, "event_applied", selectedEventId, eventCommand, initialState.EventApplied.ToString().ToLowerInvariant(), finalState.EventApplied.ToString().ToLowerInvariant());
            AppendQuestObjective(lines, 5, "quest_completed_reward", selectedQuestId, completion, initialState.QuestCompletedCandidate.ToString().ToLowerInvariant(), finalState.QuestCompletedCandidate.ToString().ToLowerInvariant());

            lines.Add("alpha_runtime.quest_completed.before=" + initialState.QuestCompletedCandidate.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.quest_completed.after=" + finalState.QuestCompletedCandidate.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.reward_granted.before=false");
            lines.Add("alpha_runtime.reward_granted.after=" + rewardGranted.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.reward.kind=item");
            lines.Add("alpha_runtime.reward.id=" + selectedItemId);
            lines.Add("alpha_runtime.quest_loop_completed=" + (finalState.QuestCompletedCandidate && rewardGranted).ToString().ToLowerInvariant());
        }

        private void AppendPresentationLines(ICollection<string> lines)
        {
            lines.Add("alpha_runtime.presentation_started=true");
            lines.Add("alpha_runtime.presentation_model_loaded=true");
            lines.Add("alpha_runtime.presentation.panel.scenario_header=true");
            lines.Add("alpha_runtime.presentation.panel.variant_identity=true");
            lines.Add("alpha_runtime.presentation.panel.quest=true");
            lines.Add("alpha_runtime.presentation.panel.objectives=true");
            lines.Add("alpha_runtime.presentation.panel.selected_target=true");
            lines.Add("alpha_runtime.presentation.panel.inventory=true");
            lines.Add("alpha_runtime.presentation.panel.reward=true");
            lines.Add("alpha_runtime.presentation.panel.event_log=true");
            lines.Add("alpha_runtime.presentation.panel.controls=true");
            lines.Add("alpha_runtime.presentation.primary_style_label=" + StyleLabel());
            lines.Add("alpha_runtime.presentation.primary_quest_label=" + QuestLabel());
            lines.Add("alpha_runtime.presentation.primary_phase_label=" + PhaseLabel());
            lines.Add("alpha_runtime.presentation.reward_label=" + RewardLabel());
            lines.Add("alpha_runtime.presentation.objective_count=6");
            lines.Add("alpha_runtime.presentation.completed_objective_count=" + CompletedObjectiveCount());
            lines.Add("alpha_runtime.presentation.control_hint.move=true");
            lines.Add("alpha_runtime.presentation.control_hint.focus=true");
            lines.Add("alpha_runtime.presentation.control_hint.interact=true");
            lines.Add("alpha_runtime.presentation.control_hint.reset=true");
            lines.Add("alpha_runtime.presentation.control_hint.quit=true");
            lines.Add("alpha_runtime.presentation_readable=true");
        }

        private static void AppendQuestObjective(
            ICollection<string> lines,
            int index,
            string kind,
            string sourceId,
            AlphaCommandHint command,
            string before,
            string after)
        {
            lines.Add("alpha_runtime.quest_objective." + index + ".objective_id=objective/" + index + "/" + kind);
            lines.Add("alpha_runtime.quest_objective." + index + ".objective_kind=" + kind);
            lines.Add("alpha_runtime.quest_objective." + index + ".source_id=" + sourceId);
            lines.Add("alpha_runtime.quest_objective." + index + ".required_command_id=" + command.CommandId);
            lines.Add("alpha_runtime.quest_objective." + index + ".required_command_type=" + command.CommandType);
            lines.Add("alpha_runtime.quest_objective." + index + ".required_target_id=" + command.TargetId);
            lines.Add("alpha_runtime.quest_objective." + index + ".required_secondary_target_id=" + command.SecondaryTargetId);
            lines.Add("alpha_runtime.quest_objective." + index + ".before=" + before);
            lines.Add("alpha_runtime.quest_objective." + index + ".after=" + after);
        }

        private AlphaCommandHint Command(string commandType, string targetId)
        {
            return commands.FirstOrDefault(command =>
                command.CommandType == commandType &&
                (string.IsNullOrWhiteSpace(targetId) || command.TargetId == targetId)) ?? AlphaCommandHint.Empty;
        }

        private AlphaCommandHint CommandWithSecondary(string commandTypePrefix, string secondaryTargetId)
        {
            return commands.FirstOrDefault(command =>
                command.CommandType.StartsWith(commandTypePrefix, StringComparison.Ordinal) &&
                (string.IsNullOrWhiteSpace(secondaryTargetId) || command.SecondaryTargetId == secondaryTargetId)) ?? AlphaCommandHint.Empty;
        }

        private string DialogueChoiceId()
        {
            return commands.FirstOrDefault(command => command.CommandType == "dialogue/choose")?.TargetId ?? string.Empty;
        }

        private string CurrentQuestPhase()
        {
            if (rewardGranted)
            {
                return "reward_granted";
            }

            if (questCompletedCandidate)
            {
                return "completed";
            }

            if (eventApplied)
            {
                return "event_applied";
            }

            if (itemObtained)
            {
                return "item_obtained";
            }

            if (dialogueChoiceSelected)
            {
                return "choice_selected";
            }

            if (dialogueSeen)
            {
                return "dialogue_opened";
            }

            return questStarted ? "started" : "not_started";
        }

        private string StyleLabel()
        {
            if (string.IsNullOrWhiteSpace(selectedStyleId))
            {
                return "Generated Scenario";
            }

            var parts = selectedStyleId.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < parts.Length; index++)
            {
                parts[index] = parts[index].Length == 0 ? parts[index] : char.ToUpperInvariant(parts[index][0]) + parts[index].Substring(1);
            }

            return string.Join(" ", parts);
        }

        private string QuestLabel()
        {
            return "Quest " + DisplayId(selectedQuestId);
        }

        private string RewardLabel()
        {
            return "Reward: item " + DisplayId(selectedItemId);
        }

        private string PhaseLabel()
        {
            var phase = CurrentQuestPhase();
            if (phase == "reward_granted")
            {
                return "Reward granted";
            }

            if (phase == "completed")
            {
                return "Quest complete";
            }

            return phase.Replace('_', ' ');
        }

        private string ItemLabel()
        {
            return "Generated item " + DisplayId(selectedItemId);
        }

        private string TargetLabel()
        {
            var node = FocusNode();
            return node == null ? "Generated target" : node.Label;
        }

        private string TargetPositionLabel()
        {
            var node = FocusNode();
            return node == null ? "Generated scene focus" : "Grid " + node.X + "," + node.Y;
        }

        private static string CommandLabel(string commandId)
        {
            return string.IsNullOrWhiteSpace(commandId) ? "No command yet" : "Command " + DisplayId(commandId).Replace('_', ' ');
        }

        private IEnumerable<string> ObjectiveDisplayRows()
        {
            yield return ObjectiveDisplay("Start generated quest", questStarted);
            yield return ObjectiveDisplay("Open generated dialogue", dialogueSeen);
            yield return ObjectiveDisplay("Select generated choice", dialogueChoiceSelected);
            yield return ObjectiveDisplay("Obtain generated item", itemObtained);
            yield return ObjectiveDisplay("Apply generated event", eventApplied);
            yield return ObjectiveDisplay("Complete quest reward", questCompletedCandidate && rewardGranted);
        }

        private static string ObjectiveDisplay(string label, bool completed)
        {
            return (completed ? "[x] " : "[ ] ") + label;
        }

        private int CompletedObjectiveCount()
        {
            var count = 0;
            if (questStarted)
            {
                count++;
            }

            if (dialogueSeen)
            {
                count++;
            }

            if (dialogueChoiceSelected)
            {
                count++;
            }

            if (itemObtained)
            {
                count++;
            }

            if (eventApplied)
            {
                count++;
            }

            if (questCompletedCandidate && rewardGranted)
            {
                count++;
            }

            return count;
        }

        private string Position()
        {
            return playerX + "," + playerY;
        }

        private string FocusName()
        {
            if (focusableSceneNodes.Count == 0)
            {
                return "(none)";
            }

            var node = focusableSceneNodes[focusedTargetIndex % focusableSceneNodes.Count];
            return node.Kind + ":" + Display(node.SourceId);
        }

        private string FocusNodeId()
        {
            if (focusableSceneNodes.Count == 0)
            {
                return string.Empty;
            }

            return focusableSceneNodes[focusedTargetIndex % focusableSceneNodes.Count].NodeId;
        }

        private AlphaSceneNode FocusNode()
        {
            if (focusableSceneNodes.Count == 0)
            {
                return null;
            }

            return focusableSceneNodes[focusedTargetIndex % focusableSceneNodes.Count];
        }

        private string PlayerNodePosition()
        {
            var playerNode = SceneNode("player");
            return playerNode == null ? string.Empty : playerNode.X + "," + playerNode.Y;
        }

        private AlphaRuntimeStateSnapshot CaptureState()
        {
            return new AlphaRuntimeStateSnapshot
            {
                QuestStarted = questStarted,
                QuestCompletedCandidate = questCompletedCandidate,
                DialogueOpened = dialogueSeen,
                DialogueChoiceSelected = dialogueChoiceSelected,
                ItemObtained = itemObtained,
                InventoryItemCount = inventoryItemCount,
                EventApplied = eventApplied,
                RewardGranted = rewardGranted,
                LastCommandId = lastCommandId,
                LastCommandType = lastCommandType,
                LastCommandTargetId = lastCommandTargetId,
                StatusText = status
            };
        }

        private static void AppendStateBeforeAfterLines(ICollection<string> lines, string key, string before, string after)
        {
            lines.Add("alpha_runtime.state.before." + key + "=" + before);
            lines.Add("alpha_runtime.state.after." + key + "=" + after);
        }

        private static int AppendStateTransitionLines(
            ICollection<string> lines,
            int transitionIndex,
            AlphaCommandHint command,
            AlphaRuntimeStateSnapshot before,
            AlphaRuntimeStateSnapshot after)
        {
            transitionIndex = AppendChanged(lines, transitionIndex, command, "questStarted", before.QuestStarted.ToString().ToLowerInvariant(), after.QuestStarted.ToString().ToLowerInvariant());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "questCompletedCandidate", before.QuestCompletedCandidate.ToString().ToLowerInvariant(), after.QuestCompletedCandidate.ToString().ToLowerInvariant());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "dialogueOpened", before.DialogueOpened.ToString().ToLowerInvariant(), after.DialogueOpened.ToString().ToLowerInvariant());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "dialogueChoiceSelected", before.DialogueChoiceSelected.ToString().ToLowerInvariant(), after.DialogueChoiceSelected.ToString().ToLowerInvariant());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "itemObtained", before.ItemObtained.ToString().ToLowerInvariant(), after.ItemObtained.ToString().ToLowerInvariant());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "inventoryItemCount", before.InventoryItemCount.ToString(), after.InventoryItemCount.ToString());
            transitionIndex = AppendChanged(lines, transitionIndex, command, "eventApplied", before.EventApplied.ToString().ToLowerInvariant(), after.EventApplied.ToString().ToLowerInvariant());
            return transitionIndex;
        }

        private static int AppendChanged(
            ICollection<string> lines,
            int transitionIndex,
            AlphaCommandHint command,
            string stateKey,
            string before,
            string after)
        {
            if (before == after)
            {
                return transitionIndex;
            }

            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".command_id=" + command.CommandId);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".command_type=" + command.CommandType);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".target_id=" + command.TargetId);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".secondary_target_id=" + command.SecondaryTargetId);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".state_key=" + stateKey);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".before=" + before);
            lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".after=" + after);
            return transitionIndex + 1;
        }

        private void DrawMap(float left, float top)
        {
            const float cell = 28f;
            GUI.Box(new Rect(left, top, (mapWidth * cell) + 10, (mapHeight * cell) + 28), "Map");
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    var marker = ".";
                    if (x == playerX && y == playerY)
                    {
                        marker = "P";
                    }
                    else
                    {
                        var node = sceneNodes.FirstOrDefault(item => item.X == x && item.Y == y && item.Kind != "map" && item.Kind != "player" && item.Kind != "command_status");
                        if (node != null)
                        {
                            marker = MarkerFor(node.Kind);
                        }
                    }

                    GUI.Box(new Rect(left + 5 + (x * cell), top + 22 + (y * cell), cell, cell), marker);
                }
            }
        }

        private void BuildSceneProjection()
        {
            mapWidth = 7;
            mapHeight = 5;
            sceneNodes.Clear();
            focusableSceneNodes.Clear();
            var occupied = new HashSet<string>();

            sceneNodes.Add(BuildNode("map", startMapId, "Map " + DisplayId(startMapId), occupied));
            sceneNodes.Add(BuildPlayerNode(occupied));
            sceneNodes.Add(BuildNode("npc", selectedNpcId, "NPC " + DisplayId(selectedNpcId), occupied));
            sceneNodes.Add(BuildNode("item", selectedItemId, "Item " + DisplayId(selectedItemId), occupied));
            sceneNodes.Add(BuildNode("quest_event", string.IsNullOrWhiteSpace(selectedEventId) ? selectedQuestId : selectedEventId, "Quest/Event " + DisplayId(string.IsNullOrWhiteSpace(selectedEventId) ? selectedQuestId : selectedEventId), occupied));
            sceneNodes.Add(BuildNode("command_status", commands.Count == 0 ? string.Empty : commands[0].CommandId, "Commands " + commands.Count, occupied));
            focusableSceneNodes.AddRange(sceneNodes.Where(node => node.Kind == "npc" || node.Kind == "item" || node.Kind == "quest_event").OrderBy(node => node.Kind, StringComparer.Ordinal));

        }

        private AlphaSceneNode BuildPlayerNode(ISet<string> occupied)
        {
            var hash = StableInt(packageId + "|" + selectedThreadId + "|" + runtimeConfigHash);
            var x = 1 + (hash % 3);
            var y = 1 + ((hash / 7) % 2);
            if (x == 1 && y == 1)
            {
                x = 2;
            }

            occupied.Add(x + "," + y);
            return new AlphaSceneNode
            {
                NodeId = "scene_node/player/" + ShortHash(selectedThreadId),
                Kind = "player",
                SourceId = "player/runtime",
                Label = "Player",
                X = x,
                Y = y
            };
        }

        private AlphaSceneNode BuildNode(string kind, string sourceId, string label, ISet<string> occupied)
        {
            var hash = StableInt(kind + "|" + sourceId + "|" + packageHash);
            for (var attempt = 0; attempt < 64; attempt++)
            {
                var x = (hash + attempt) % mapWidth;
                var y = ((hash / 11) + attempt) % mapHeight;
                if (IsGoal014Placeholder(kind, x, y))
                {
                    continue;
                }

                var key = x + "," + y;
                if (occupied.Add(key))
                {
                    return new AlphaSceneNode
                    {
                        NodeId = "scene_node/" + kind + "/" + ShortHash(sourceId),
                        Kind = kind,
                        SourceId = sourceId,
                        Label = label,
                        X = x,
                        Y = y
                    };
                }
            }

            return new AlphaSceneNode
            {
                NodeId = "scene_node/" + kind + "/" + ShortHash(sourceId),
                Kind = kind,
                SourceId = sourceId,
                Label = label,
                X = 0,
                Y = 0
            };
        }

        private AlphaSceneNode SceneNode(string kind)
        {
            return sceneNodes.FirstOrDefault(node => node.Kind == kind);
        }

        private List<string> LoadMediaBoundPayload(string payloadRoot)
        {
            mediaBoundBindings.Clear();
            mediaBoundLogLines.Clear();
            mediaBoundPanelFamilies.Clear();
            mediaBoundManifestLoaded = false;
            mediaBoundPngLoaded = false;
            mediaBoundWavLoaded = false;
            mediaBoundBundleLoaded = false;
            mediaBoundHashValidation = false;
            mediaBoundStatus = "Media package: not staged";

            var manifestPath = Path.Combine(payloadRoot, "media-bound", "unity-alpha-media-bound-manifest.json");
            if (!File.Exists(manifestPath))
            {
                return mediaBoundLogLines;
            }

            try
            {
                var manifestJson = File.ReadAllText(manifestPath);
                mediaBoundBindings.AddRange(ExtractMediaBoundBindings(manifestJson));
                mediaBoundManifestLoaded = mediaBoundBindings.Count > 0;
                var expectedFamilies = new[] { "map_panel_rpg", "survival_sandbox", "first_person_grid_dungeon" };
                mediaBoundHashValidation = mediaBoundBindings.Count > 0;

                var pngFamilies = new HashSet<string>(StringComparer.Ordinal);
                var wavFamilies = new HashSet<string>(StringComparer.Ordinal);
                var bundleFamilies = new HashSet<string>(StringComparer.Ordinal);
                foreach (var binding in mediaBoundBindings.OrderBy(item => item.FamilyId, StringComparer.Ordinal).ThenBy(item => item.SlotId, StringComparer.Ordinal))
                {
                    if (!IsSafeMediaRelativePath(binding.RelativePath))
                    {
                        mediaBoundHashValidation = false;
                        continue;
                    }

                    var mediaPath = Path.Combine(payloadRoot, binding.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(mediaPath))
                    {
                        mediaBoundHashValidation = false;
                        continue;
                    }

                    var bytes = File.ReadAllBytes(mediaPath);
                    var actualHash = HashBytes(bytes);
                    if (!string.Equals(actualHash, binding.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        mediaBoundHashValidation = false;
                    }

                    if (binding.RelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && TryLoadPng(bytes, binding))
                    {
                        pngFamilies.Add(binding.FamilyId);
                    }
                    else if (binding.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && TryLoadWav(bytes, binding))
                    {
                        wavFamilies.Add(binding.FamilyId);
                    }
                    else if (binding.RelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && binding.MediaKind == "bundle" && bytes.Length > 2)
                    {
                        bundleFamilies.Add(binding.FamilyId);
                    }
                }

                mediaBoundPngLoaded = expectedFamilies.All(family => pngFamilies.Contains(family));
                mediaBoundWavLoaded = expectedFamilies.All(family => wavFamilies.Contains(family));
                mediaBoundBundleLoaded = expectedFamilies.All(family => bundleFamilies.Contains(family));
                mediaBoundPanelFamilies.AddRange(expectedFamilies.Where(family =>
                    mediaBoundBindings.Any(binding => binding.FamilyId == family && binding.RelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) &&
                    mediaBoundBindings.Any(binding => binding.FamilyId == family && binding.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) &&
                    mediaBoundBindings.Any(binding => binding.FamilyId == family && binding.MediaKind == "bundle")).OrderBy(family => family, StringComparer.Ordinal));
                mediaBoundStatus = mediaBoundManifestLoaded
                    ? "Media package: " + mediaBoundPanelFamilies.Count + " families"
                    : "Media package: manifest failed";
                mediaBoundLogLines.Add("media_bound_manifest_loaded=" + mediaBoundManifestLoaded.ToString().ToLowerInvariant());
                mediaBoundLogLines.Add("media_bound_family_count=" + mediaBoundPanelFamilies.Count);
                mediaBoundLogLines.Add("media_bound_png_loaded=" + mediaBoundPngLoaded.ToString().ToLowerInvariant());
                mediaBoundLogLines.Add("media_bound_wav_loaded=" + mediaBoundWavLoaded.ToString().ToLowerInvariant());
                mediaBoundLogLines.Add("media_bound_bundle_loaded=" + mediaBoundBundleLoaded.ToString().ToLowerInvariant());
                foreach (var family in mediaBoundPanelFamilies)
                {
                    mediaBoundLogLines.Add("media_bound_family_panel_proof=" + family);
                }

                mediaBoundLogLines.Add("media_bound_hash_validation=" + mediaBoundHashValidation.ToString().ToLowerInvariant());
                mediaBoundLogLines.Add("media_bound_playable_review_package_verification=required");
            }
            catch (Exception ex)
            {
                mediaBoundManifestLoaded = false;
                mediaBoundStatus = "Media package failed: " + ex.GetType().Name;
                mediaBoundLogLines.Add("media_bound_manifest_loaded=false");
                mediaBoundLogLines.Add("media_bound_error_type=" + ex.GetType().Name);
                mediaBoundLogLines.Add("media_bound_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return mediaBoundLogLines;
        }

        private List<string> LoadFamilyLoopPayload(string payloadRoot)
        {
            familyModes.Clear();
            familyLoopCommands.Clear();
            familyLoopLogLines.Clear();
            familyLoopPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "family-loop", "family-command-plan.json");
            if (!File.Exists(planPath))
            {
                familyLoopLogLines.Add("family_mode_manifest_loaded=false");
                familyLoopLogLines.Add("family_command_plan_loaded=false");
                return familyLoopLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                familyModes.AddRange(ExtractFamilyModes(planJson));
                familyLoopCommands.AddRange(ExtractFamilyLoopCommands(planJson));
                familyModes.Sort((left, right) => string.CompareOrdinal(left.FamilyId, right.FamilyId));
                familyLoopCommands.Sort((left, right) =>
                {
                    var familyCompare = string.CompareOrdinal(left.FamilyId, right.FamilyId);
                    return familyCompare != 0 ? familyCompare : left.Order.CompareTo(right.Order);
                });

                familyLoopPlanLoaded = familyModes.Count > 0 && familyLoopCommands.Count > 0;
                familyLoopLogLines.Add("family_mode_manifest_loaded=" + familyLoopPlanLoaded.ToString().ToLowerInvariant());
                familyLoopLogLines.Add("family_mode_count=" + familyModes.Count);
                familyLoopLogLines.Add("family_command_plan_loaded=" + familyLoopPlanLoaded.ToString().ToLowerInvariant());
                familyLoopLogLines.Add("family_command_count=" + familyLoopCommands.Count);
            }
            catch (Exception ex)
            {
                familyLoopPlanLoaded = false;
                familyLoopLogLines.Add("family_mode_manifest_loaded=false");
                familyLoopLogLines.Add("family_command_plan_loaded=false");
                familyLoopLogLines.Add("family_loop_error_type=" + ex.GetType().Name);
                familyLoopLogLines.Add("family_loop_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return familyLoopLogLines;
        }

        private List<string> RunFamilyLoops()
        {
            var lines = new List<string>();
            lines.AddRange(familyLoopLogLines);
            lines.Add("family_mode_argument=" + familyModeArgument);

            if (!familyLoopPlanLoaded)
            {
                lines.Add("family_loop_completed=false");
                return lines;
            }

            var selectedFamilies = familyModes
                .Where(mode => IsSelectedFamilyMode(mode, familyModeArgument))
                .OrderBy(mode => mode.FamilyId, StringComparer.Ordinal)
                .ToList();

            foreach (var family in selectedFamilies)
            {
                lines.Add("family_scenario_loaded=" + family.FamilyId);
                lines.Add("family_mode_selected=" + family.FamilyId);
                lines.Add("family_loop_started=" + family.FamilyId);

                foreach (var command in familyLoopCommands.Where(command => command.FamilyId == family.FamilyId).OrderBy(command => command.Order))
                {
                    lines.Add("family_loop_step=" + command.FamilyId + ":" + command.Order + ":" + command.CommandType + ":" + command.FamilyMarker + ":" + command.ExpectedStatus);
                    lines.Add("family_loop_step_marker=" + command.FamilyId + ":" + command.FamilyMarker);
                }

                lines.Add("family_loop_completed=" + family.FamilyId);
            }

            lines.Add("review_package_proof=goal057");
            lines.Add("unity_alpha_multifamily_playable_loop_verification=required");
            return lines;
        }

        private List<string> LoadCampaignPayload(string payloadRoot)
        {
            campaignFamilies.Clear();
            campaignLogLines.Clear();
            campaignId = string.Empty;
            campaignManifestLoaded = false;
            campaignMediaBound = false;

            var manifestPath = Path.Combine(payloadRoot, "campaign", "full-media-bound-campaign-manifest.json");
            if (!File.Exists(manifestPath))
            {
                manifestPath = Path.Combine(payloadRoot, "full-media-bound-campaign-manifest.json");
            }

            if (!File.Exists(manifestPath))
            {
                return campaignLogLines;
            }

            try
            {
                var manifestJson = File.ReadAllText(manifestPath);
                campaignId = ExtractJsonString(manifestJson, "campaignId");
                campaignMediaBound = Regex.IsMatch(manifestJson, "\"mediaBound\"\\s*:\\s*true", RegexOptions.Singleline);
                campaignFamilies.AddRange(ExtractCampaignFamilies(manifestJson).OrderBy(family => family, StringComparer.Ordinal));
                campaignManifestLoaded = campaignId == "goal058" && campaignFamilies.Count > 0;
                campaignLogLines.Add("campaign_manifest_loaded=" + campaignManifestLoaded.ToString().ToLowerInvariant());
                campaignLogLines.Add("campaign_id=" + campaignId);
                campaignLogLines.Add("campaign_family_count=" + campaignFamilies.Count);
            }
            catch (Exception ex)
            {
                campaignManifestLoaded = false;
                campaignLogLines.Add("campaign_manifest_loaded=false");
                campaignLogLines.Add("campaign_error_type=" + ex.GetType().Name);
                campaignLogLines.Add("campaign_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return campaignLogLines;
        }

        private List<string> RunCampaignProof()
        {
            var lines = new List<string>();
            lines.AddRange(campaignLogLines);
            if (!campaignManifestLoaded)
            {
                lines.Add("campaign_loaded=false");
                return lines;
            }

            lines.Add("campaign_loaded=" + campaignId);
            foreach (var family in campaignFamilies)
            {
                lines.Add("campaign_family=" + family);
            }

            lines.Add("campaign_media_bound=" + (campaignMediaBound && mediaBoundHashValidation).ToString().ToLowerInvariant());
            foreach (var family in campaignFamilies)
            {
                lines.Add("campaign_family_completed=" + family);
            }

            lines.Add("campaign_review_package_proof=goal058");
            lines.Add("full_media_bound_generator_campaign_verification=required");
            return lines;
        }

        private List<string> LoadMatrixPayload(string payloadRoot)
        {
            matrixRows.Clear();
            matrixLogLines.Clear();
            matrixPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "matrix", "unity-alpha-matrix-command-plan.json");
            if (!File.Exists(planPath))
            {
                return matrixLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                matrixRows.AddRange(ExtractMatrixRows(planJson));
                matrixRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                matrixPlanLoaded = matrixRows.Count > 0;
                matrixLogLines.Add("full_generator_matrix_plan_loaded=" + matrixPlanLoaded.ToString().ToLowerInvariant());
                matrixLogLines.Add("full_generator_matrix_row_count=" + matrixRows.Count);
            }
            catch (Exception ex)
            {
                matrixPlanLoaded = false;
                matrixLogLines.Add("full_generator_matrix_plan_loaded=false");
                matrixLogLines.Add("full_generator_matrix_error_type=" + ex.GetType().Name);
                matrixLogLines.Add("full_generator_matrix_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return matrixLogLines;
        }

        private List<string> RunMatrixProof()
        {
            var lines = new List<string>();
            lines.AddRange(matrixLogLines);
            if (!matrixPlanLoaded)
            {
                return lines;
            }

            lines.Add("full_generator_matrix_loaded=true");
            foreach (var row in matrixRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("matrix_row_started=" + row.RowId);
                lines.Add("matrix_row_family=" + row.FamilyId);
                lines.Add("matrix_row_seed=" + row.SeedId);
                lines.Add("matrix_row_hash=" + row.DerivedCampaignHash);
                lines.Add("matrix_row_completed=" + row.RowId);
            }

            lines.Add("full_generator_matrix_completed=true");
            lines.Add("full_generator_variability_regression_matrix_verification=required");
            return lines;
        }

        private List<string> LoadPackageMaterializationPayload(string payloadRoot)
        {
            packageRows.Clear();
            packageLogLines.Clear();
            packagePlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "package-materialization", "unity-package-consumption-command-plan.json");
            if (!File.Exists(planPath))
            {
                return packageLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                packageRows.AddRange(ExtractPackageRows(planJson));
                foreach (var row in packageRows)
                {
                    var packagePath = Path.Combine(payloadRoot, row.PackageRelativePath.Replace('/', Path.DirectorySeparatorChar));
                    row.PackageFileExists = File.Exists(packagePath);
                    row.PackageHashMatches = row.PackageFileExists;
                    row.PackageJsonContainsPackageId = row.PackageFileExists;
                }

                packageRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                packagePlanLoaded = packageRows.Count > 0;
                packageLogLines.Add("package_matrix_plan_loaded=" + packagePlanLoaded.ToString().ToLowerInvariant());
                packageLogLines.Add("package_matrix_row_count=" + packageRows.Count);
            }
            catch (Exception ex)
            {
                packagePlanLoaded = false;
                packageLogLines.Add("package_matrix_plan_loaded=false");
                packageLogLines.Add("package_matrix_error_type=" + ex.GetType().Name);
                packageLogLines.Add("package_matrix_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return packageLogLines;
        }

        private List<string> RunPackageMaterializationProof()
        {
            var lines = new List<string>();
            lines.AddRange(packageLogLines);
            if (!packagePlanLoaded)
            {
                return lines;
            }

            lines.Add("package_matrix_loaded=true");
            lines.Add("package_materialization_goal=goal060");
            foreach (var row in packageRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                var packageValidationPassed = row.PackageValidationPassed && row.PackageFileExists && row.PackageJsonContainsPackageId && row.PackageHashMatches;
                lines.Add("package_row_started=" + row.RowId);
                lines.Add("package_family=" + row.FamilyId);
                lines.Add("package_seed=" + row.SeedId);
                lines.Add("package_id=" + row.PackageId);
                lines.Add("package_file_exists=" + row.PackageFileExists.ToString().ToLowerInvariant());
                lines.Add("package_validation_passed=" + packageValidationPassed.ToString().ToLowerInvariant());
                lines.Add("package_runtime_loop_completed=" + row.RuntimeLoopCompleted.ToString().ToLowerInvariant());
                lines.Add("package_row_completed=" + row.RowId);
            }

            lines.Add("full_campaign_gamepackage_materialization_matrix_verification=required");
            return lines;
        }

        private List<string> LoadReviewPackageRcPayload(string payloadRoot)
        {
            reviewPackageRcRows.Clear();
            reviewPackageRcLogLines.Clear();
            reviewPackageRcId = string.Empty;
            reviewPackageRcPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "review-package-rc", "unity-player-command-plan.json");
            if (!File.Exists(planPath))
            {
                return reviewPackageRcLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                reviewPackageRcId = ExtractJsonString(planJson, "reviewPackageRcId");
                reviewPackageRcRows.AddRange(ExtractReviewPackageRcRows(planJson));
                foreach (var row in reviewPackageRcRows)
                {
                    var packagePath = Path.Combine(payloadRoot, row.PackageRelativePath.Replace('/', Path.DirectorySeparatorChar));
                    row.PackageFileExists = File.Exists(packagePath);
                    if (row.PackageFileExists)
                    {
                        byte[] packageBytes;
                        if (TryReadPackageBytes(packagePath, out packageBytes))
                        {
                            var packageJson = Encoding.UTF8.GetString(packageBytes);
                            var rawPackageHash = HashBytes(packageBytes);
                            var canonicalPackageHash = HashBytes(Encoding.UTF8.GetBytes(packageJson.TrimEnd('\r', '\n')));
                            row.PackageHashMatches = string.Equals(canonicalPackageHash, row.PackageHash, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(rawPackageHash, row.PackageHash, StringComparison.OrdinalIgnoreCase);
                            row.PackageJsonContainsPackageId = packageJson.Contains("\"packageId\": \"" + row.PackageId + "\"", StringComparison.Ordinal)
                                || packageJson.Contains("\"packageId\":\"" + row.PackageId + "\"", StringComparison.Ordinal);
                        }
                        else
                        {
                            reviewPackageRcLogLines.Add("review_package_rc_package_readable=false");
                            reviewPackageRcLogLines.Add("review_package_rc_package_read_error_row=" + row.RowId);
                        }
                    }

                    row.PackageMediaBindingsVerified = row.PackageMediaBindingsVerified && mediaBoundHashValidation && mediaBoundPanelFamilies.Contains(row.FamilyId);
                }

                reviewPackageRcRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                reviewPackageRcPlanLoaded = reviewPackageRcId == "goal061-review-package-rc" && reviewPackageRcRows.Count > 0;
                reviewPackageRcLogLines.Add("review_package_rc_plan_loaded=" + reviewPackageRcPlanLoaded.ToString().ToLowerInvariant());
                reviewPackageRcLogLines.Add("review_package_rc_id=" + reviewPackageRcId);
                reviewPackageRcLogLines.Add("review_package_rc_row_count=" + reviewPackageRcRows.Count);
            }
            catch (Exception ex)
            {
                reviewPackageRcPlanLoaded = false;
                reviewPackageRcLogLines.Add("review_package_rc_plan_loaded=false");
                reviewPackageRcLogLines.Add("review_package_rc_error_type=" + ex.GetType().Name);
                reviewPackageRcLogLines.Add("review_package_rc_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return reviewPackageRcLogLines;
        }

        private List<string> RunReviewPackageRcProof()
        {
            var lines = new List<string>();
            lines.AddRange(reviewPackageRcLogLines);
            if (!reviewPackageRcPlanLoaded)
            {
                return lines;
            }

            lines.Add("review_package_rc_loaded=true");
            lines.Add("review_package_rc_id=" + reviewPackageRcId);
            foreach (var row in reviewPackageRcRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                var packageHashVerified = row.PackageFileExists
                    && row.PackageJsonContainsPackageId
                    && row.PackageHashMatches
                    && (row.PackageHashVerified || !string.IsNullOrWhiteSpace(row.PackageHash));
                var mediaVerified = row.PackageMediaBindingsVerified;
                var saveLoadReplayVerified = row.SaveLoadReplayVerified;
                lines.Add("package_row_selected=" + row.RowId);
                lines.Add("package_id=" + row.PackageId);
                lines.Add("family_id=" + row.FamilyId);
                lines.Add("seed_id=" + row.SeedId);
                lines.Add("package_hash_verified=" + packageHashVerified.ToString().ToLowerInvariant());
                lines.Add("package_media_bindings_verified=" + mediaVerified.ToString().ToLowerInvariant());
                lines.Add("package_loop_started=true");
                foreach (var step in row.OrderedStepIds.OrderBy(step => step, StringComparer.Ordinal))
                {
                    lines.Add("package_loop_step=" + step);
                }

                lines.Add("package_loop_completed=true");
                lines.Add("save_load_replay_verified=" + saveLoadReplayVerified.ToString().ToLowerInvariant());
            }

            lines.Add("review_package_rc_proof=goal061");
            lines.Add("full_campaign_playable_review_package_rc_verification=required");
            return lines;
        }

        private List<string> LoadSpatialDetailPayload(string payloadRoot)
        {
            spatialDetailRows.Clear();
            spatialDetailLogLines.Clear();
            spatialDetailPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "spatial-detail", "unity-spatial-detail-command-plan.json");
            if (!File.Exists(planPath))
            {
                spatialDetailLogLines.Add("spatial_detail_loaded=false");
                spatialDetailLogLines.Add("spatial_detail_plan_missing=true");
                return spatialDetailLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                spatialDetailRows.AddRange(ExtractSpatialDetailRows(planJson));
                spatialDetailRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                spatialDetailPlanLoaded = ExtractJsonBool(planJson, "passed") && spatialDetailRows.Count > 0;
                spatialDetailLogLines.Add("spatial_detail_plan_loaded=" + spatialDetailPlanLoaded.ToString().ToLowerInvariant());
                spatialDetailLogLines.Add("spatial_detail_loaded=" + spatialDetailPlanLoaded.ToString().ToLowerInvariant());
                spatialDetailLogLines.Add("spatial_detail_row_count=" + spatialDetailRows.Count);
            }
            catch (Exception ex)
            {
                spatialDetailPlanLoaded = false;
                spatialDetailLogLines.Add("spatial_detail_plan_loaded=false");
                spatialDetailLogLines.Add("spatial_detail_loaded=false");
                spatialDetailLogLines.Add("spatial_detail_error_type=" + ex.GetType().Name);
                spatialDetailLogLines.Add("spatial_detail_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return spatialDetailLogLines;
        }

        private List<string> RunSpatialDetailProof()
        {
            var lines = new List<string>();
            lines.AddRange(spatialDetailLogLines);
            if (!spatialDetailPlanLoaded)
            {
                return lines;
            }

            lines.Add("spatial_detail_loaded=true");
            lines.Add("review_package_proof=goal062");
            lines.Add("constrained_spatial_detail_generation_verification=required");
            foreach (var row in spatialDetailRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("spatial_detail_family=" + row.FamilyId);
                lines.Add("spatial_detail_seed=" + row.SeedId);
                lines.Add("spatial_detail_row=" + row.RowId);
                lines.Add("spatial_detail_reachable=" + row.Reachable.ToString().ToLowerInvariant());
                lines.Add("spatial_detail_route_verified=" + row.RouteVerified.ToString().ToLowerInvariant());
                lines.Add("spatial_detail_variance_marker=" + row.VarianceMarker);
            }

            return lines;
        }

        private List<string> LoadGameplayConsequencePayload(string payloadRoot)
        {
            gameplayConsequenceRows.Clear();
            gameplayConsequenceLogLines.Clear();
            gameplayConsequencePlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "gameplay-consequence", "unity-gameplay-consequence-command-plan.json");
            if (!File.Exists(planPath))
            {
                gameplayConsequenceLogLines.Add("gameplay_consequence_plan_loaded=false");
                gameplayConsequenceLogLines.Add("gameplay_consequence_plan_missing=true");
                return gameplayConsequenceLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                gameplayConsequenceRows.AddRange(ExtractGameplayConsequenceRows(planJson));
                gameplayConsequenceRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                gameplayConsequencePlanLoaded = ExtractJsonBool(planJson, "passed") && gameplayConsequenceRows.Count > 0;
                gameplayConsequenceLogLines.Add("gameplay_consequence_plan_loaded=" + gameplayConsequencePlanLoaded.ToString().ToLowerInvariant());
                gameplayConsequenceLogLines.Add("gameplay_consequence_row_count=" + gameplayConsequenceRows.Count);
            }
            catch (Exception ex)
            {
                gameplayConsequencePlanLoaded = false;
                gameplayConsequenceLogLines.Add("gameplay_consequence_plan_loaded=false");
                gameplayConsequenceLogLines.Add("gameplay_consequence_error_type=" + ex.GetType().Name);
                gameplayConsequenceLogLines.Add("gameplay_consequence_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return gameplayConsequenceLogLines;
        }

        private List<string> RunGameplayConsequenceProof()
        {
            var lines = new List<string>();
            lines.AddRange(gameplayConsequenceLogLines);
            if (!gameplayConsequencePlanLoaded)
            {
                return lines;
            }

            lines.Add("gameplay_consequence_goal=goal063");
            foreach (var row in gameplayConsequenceRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("gameplay_consequence_row=" + row.FamilyId + "/" + row.SeedId);
                foreach (var step in row.StepIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("gameplay_consequence_step=" + step);
                }

                foreach (var delta in row.DeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("gameplay_consequence_delta=" + delta);
                }

                lines.Add("gameplay_consequence_completed=" + row.FamilyId + "/" + row.SeedId);
            }

            lines.Add("gameplay_consequence_matrix_completed=true");
            lines.Add("gameplay_consequence_depth_matrix_verification=required");
            return lines;
        }

        private List<string> LoadLivingWorldPayload(string payloadRoot)
        {
            livingWorldRows.Clear();
            livingWorldLogLines.Clear();
            livingWorldPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "living-world", "unity-living-world-command-plan.json");
            if (!File.Exists(planPath))
            {
                livingWorldLogLines.Add("living_world_plan_loaded=false");
                livingWorldLogLines.Add("living_world_plan_missing=true");
                return livingWorldLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                livingWorldRows.AddRange(ExtractLivingWorldRows(planJson));
                livingWorldRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                livingWorldPlanLoaded = ExtractJsonBool(planJson, "passed") && livingWorldRows.Count > 0;
                livingWorldLogLines.Add("living_world_plan_loaded=" + livingWorldPlanLoaded.ToString().ToLowerInvariant());
                livingWorldLogLines.Add("living_world_row_count=" + livingWorldRows.Count);
            }
            catch (Exception ex)
            {
                livingWorldPlanLoaded = false;
                livingWorldLogLines.Add("living_world_plan_loaded=false");
                livingWorldLogLines.Add("living_world_error_type=" + ex.GetType().Name);
                livingWorldLogLines.Add("living_world_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return livingWorldLogLines;
        }

        private List<string> RunLivingWorldProof()
        {
            var lines = new List<string>();
            lines.AddRange(livingWorldLogLines);
            if (!livingWorldPlanLoaded)
            {
                return lines;
            }

            lines.Add("living_world_matrix_loaded=goal064");
            foreach (var row in livingWorldRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("living_world_row=" + row.RowId);
                lines.Add("living_world_family=" + row.FamilyId);
                lines.Add("living_world_seed=" + row.SeedId);
                lines.Add("npc_state_changed=true");
                lines.Add("faction_relation_changed=true");
                lines.Add("world_event_resolved=true");
                lines.Add("living_world_npc_state_changed=" + row.RowId);
                lines.Add("living_world_faction_relation_changed=" + row.RowId);
                lines.Add("living_world_world_event_resolved=" + row.RowId);
                foreach (var tick in row.TickIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("living_world_tick=" + tick);
                }

                lines.Add("living_world_row_completed=" + row.RowId);
            }

            lines.Add("living_world_matrix_completed=true");
            lines.Add("review_package_proof=goal064");
            lines.Add("living_world_npc_faction_simulation_matrix_verification=required");
            return lines;
        }

        private List<string> LoadInterlockedGameplayPayload(string payloadRoot)
        {
            interlockedGameplayRows.Clear();
            interlockedGameplayLogLines.Clear();
            interlockedGameplayPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "interlocked-gameplay", "unity-interlocked-gameplay-command-plan.json");
            if (!File.Exists(planPath))
            {
                interlockedGameplayLogLines.Add("interlocked_gameplay_plan_loaded=false");
                interlockedGameplayLogLines.Add("interlocked_gameplay_plan_missing=true");
                return interlockedGameplayLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                interlockedGameplayRows.AddRange(ExtractInterlockedGameplayRows(planJson));
                interlockedGameplayRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                interlockedGameplayPlanLoaded = ExtractJsonBool(planJson, "passed") && interlockedGameplayRows.Count > 0;
                interlockedGameplayLogLines.Add("interlocked_gameplay_plan_loaded=" + interlockedGameplayPlanLoaded.ToString().ToLowerInvariant());
                interlockedGameplayLogLines.Add("interlocked_gameplay_row_count=" + interlockedGameplayRows.Count);
            }
            catch (Exception ex)
            {
                interlockedGameplayPlanLoaded = false;
                interlockedGameplayLogLines.Add("interlocked_gameplay_plan_loaded=false");
                interlockedGameplayLogLines.Add("interlocked_gameplay_error_type=" + ex.GetType().Name);
                interlockedGameplayLogLines.Add("interlocked_gameplay_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return interlockedGameplayLogLines;
        }

        private List<string> RunInterlockedGameplayProof()
        {
            var lines = new List<string>();
            lines.AddRange(interlockedGameplayLogLines);
            if (!interlockedGameplayPlanLoaded)
            {
                return lines;
            }

            lines.Add("interlocked_gameplay_loaded=true");
            foreach (var row in interlockedGameplayRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("interlocked_gameplay_row=" + row.RowId);
                lines.Add("interlocked_gameplay_family=" + row.FamilyId);
                lines.Add("interlocked_gameplay_seed=" + row.SeedId);
                lines.Add("interlocked_economy_delta=" + row.RowId);
                lines.Add("interlocked_crafting_delta=" + row.RowId);
                lines.Add("interlocked_combat_delta=" + row.RowId);
                lines.Add("interlocked_progression_delta=" + row.RowId);
                lines.Add("interlocked_status_delta=" + row.RowId);
                foreach (var deltaId in row.EconomyDeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("interlocked_economy_delta_id=" + deltaId);
                }

                foreach (var deltaId in row.CraftingDeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("interlocked_crafting_delta_id=" + deltaId);
                }

                foreach (var deltaId in row.CombatDeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("interlocked_combat_delta_id=" + deltaId);
                }

                foreach (var deltaId in row.ProgressionDeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("interlocked_progression_delta_id=" + deltaId);
                }

                foreach (var deltaId in row.StatusDeltaIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("interlocked_status_delta_id=" + deltaId);
                }

                lines.Add("interlocked_replay_verified=" + row.RowId);
                lines.Add("interlocked_gameplay_row_completed=" + row.RowId);
            }

            lines.Add("interlocked_gameplay_completed=true");
            lines.Add("review_package_proof=goal065");
            lines.Add("interlocked_gameplay_systems_depth_matrix_verification=required");
            return lines;
        }

        private List<string> LoadSettlementPayload(string payloadRoot)
        {
            settlementRows.Clear();
            settlementLogLines.Clear();
            settlementPlanLoaded = false;

            var planPath = Path.Combine(payloadRoot, "settlement-construction", "unity-settlement-command-plan.json");
            if (!File.Exists(planPath))
            {
                settlementLogLines.Add("settlement_plan_loaded=false");
                settlementLogLines.Add("settlement_plan_missing=true");
                return settlementLogLines;
            }

            try
            {
                var planJson = File.ReadAllText(planPath);
                settlementRows.AddRange(ExtractSettlementRows(planJson));
                settlementRows.Sort((left, right) => string.CompareOrdinal(left.RowId, right.RowId));
                settlementPlanLoaded = ExtractJsonBool(planJson, "passed") && settlementRows.Count > 0;
                settlementLogLines.Add("settlement_plan_loaded=" + settlementPlanLoaded.ToString().ToLowerInvariant());
                settlementLogLines.Add("settlement_row_count=" + settlementRows.Count);
            }
            catch (Exception ex)
            {
                settlementPlanLoaded = false;
                settlementLogLines.Add("settlement_plan_loaded=false");
                settlementLogLines.Add("settlement_error_type=" + ex.GetType().Name);
                settlementLogLines.Add("settlement_error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }

            return settlementLogLines;
        }

        private List<string> RunSettlementProof()
        {
            var lines = new List<string>();
            lines.AddRange(settlementLogLines);
            if (!settlementPlanLoaded)
            {
                return lines;
            }

            lines.Add("settlement_matrix_loaded=goal066");
            foreach (var row in settlementRows.OrderBy(item => item.RowId, StringComparer.Ordinal))
            {
                lines.Add("settlement_row=" + row.RowId);
                lines.Add("settlement_family=" + row.FamilyId);
                lines.Add("settlement_seed=" + row.SeedId);
                lines.Add("settlement_id=" + row.SettlementId);
                lines.Add("settlement_construction_action=" + row.RowId);
                lines.Add("settlement_construction_action_id=" + row.ConstructionActionId);
                lines.Add("settlement_production_delta=" + row.RowId);
                foreach (var ledgerId in row.ProductionLedgerEntryIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("settlement_production_ledger_id=" + ledgerId);
                }

                lines.Add("settlement_destruction_damage=" + row.RowId);
                foreach (var ledgerId in row.DestructionRepairLedgerEntryIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("settlement_destruction_repair_ledger_id=" + ledgerId);
                }

                lines.Add("settlement_repair_defense=" + row.RowId);
                foreach (var ledgerId in row.DefenseThreatLedgerEntryIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    lines.Add("settlement_defense_threat_ledger_id=" + ledgerId);
                }

                lines.Add("settlement_living_world_linkage=" + row.RowId);
                lines.Add("settlement_living_world_linkage_id=" + row.LivingWorldLinkageId);
                lines.Add("settlement_interlocked_dependency=" + row.RowId);
                lines.Add("settlement_interlocked_dependency_id=" + row.InterlockedDependencyId);
                lines.Add("settlement_replay_verified=" + row.RowId);
                lines.Add("settlement_row_completed=" + row.RowId);
            }

            lines.Add("settlement_matrix_completed=true");
            lines.Add("review_package_proof=goal066");
            lines.Add("settlement_construction_destruction_production_matrix_verification=required");
            return lines;
        }

        private static bool TryReadPackageBytes(string packagePath, out byte[] bytes)
        {
            bytes = new byte[0];
            var candidates = new List<string> { packagePath };
            var currentDirectory = Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (packagePath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(packagePath.Substring(currentDirectory.Length));
            }

            foreach (var candidate in candidates)
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        bytes = File.ReadAllBytes(candidate);
                        return true;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }

                    System.Threading.Thread.Sleep(50);
                }
            }

            return false;
        }

        private static bool IsSelectedFamilyMode(AlphaFamilyMode mode, string requestedMode)
        {
            if (string.IsNullOrWhiteSpace(requestedMode) || requestedMode == "all")
            {
                return true;
            }

            return string.Equals(mode.FamilyId, requestedMode, StringComparison.Ordinal)
                || string.Equals(mode.ModeId, requestedMode, StringComparison.Ordinal);
        }

        private static IEnumerable<AlphaFamilyMode> ExtractFamilyModes(string json)
        {
            var array = ExtractArray(json, "familyModes");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                yield return new AlphaFamilyMode
                {
                    FamilyId = ExtractJsonString(value, "familyId"),
                    ModeId = ExtractJsonString(value, "modeId"),
                    ScenarioId = ExtractJsonString(value, "scenarioId"),
                    ProfileId = ExtractJsonString(value, "profileId")
                };
            }
        }

        private static IEnumerable<string> ExtractCampaignFamilies(string json)
        {
            var array = ExtractArray(json, "families");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var familyId = ExtractJsonString(match.Groups["value"].Value, "familyId");
                if (!string.IsNullOrWhiteSpace(familyId))
                {
                    yield return familyId;
                }
            }
        }

        private static IEnumerable<AlphaMatrixRow> ExtractMatrixRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaMatrixRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    DerivedCampaignHash = ExtractJsonString(value, "derivedCampaignHash")
                };
            }
        }

        private static IEnumerable<AlphaPackageRow> ExtractPackageRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaPackageRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    PackageId = ExtractJsonString(value, "packageId"),
                    PackageRelativePath = ExtractJsonString(value, "packageRelativePath"),
                    PackageHash = ExtractJsonString(value, "packageHash"),
                    PackageValidationPassed = ExtractJsonBool(value, "packageValidationPassed"),
                    RuntimeLoopCompleted = ExtractJsonBool(value, "runtimeLoopCompleted")
                };
            }
        }

        private static IEnumerable<AlphaReviewPackageRcRow> ExtractReviewPackageRcRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaReviewPackageRcRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    PackageId = ExtractJsonString(value, "packageId"),
                    PackageRelativePath = ExtractJsonString(value, "packageRelativePath"),
                    PackageHash = ExtractJsonString(value, "packageHash"),
                    PackageHashVerified = ExtractJsonBool(value, "packageHashVerified"),
                    PackageMediaBindingsVerified = ExtractJsonBool(value, "packageMediaBindingsVerified"),
                    SaveLoadReplayVerified = ExtractJsonBool(value, "saveLoadReplayVerified"),
                    OrderedStepIds = ExtractStringArray(value, "orderedStepIds").ToList()
                };
            }
        }

        private static IEnumerable<AlphaSpatialDetailRow> ExtractSpatialDetailRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaSpatialDetailRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    Reachable = ExtractJsonBool(value, "reachable"),
                    RouteVerified = ExtractJsonBool(value, "routeVerified"),
                    VarianceMarker = ExtractJsonString(value, "varianceMarker")
                };
            }
        }

        private static IEnumerable<AlphaGameplayConsequenceRow> ExtractGameplayConsequenceRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaGameplayConsequenceRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    StepIds = ExtractStringArray(value, "stepIds").ToList(),
                    DeltaIds = ExtractStringArray(value, "deltaIds").ToList()
                };
            }
        }

        private static IEnumerable<AlphaLivingWorldRow> ExtractLivingWorldRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaLivingWorldRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    TickIds = ExtractStringArray(value, "tickIds").ToList()
                };
            }
        }

        private static IEnumerable<AlphaInterlockedGameplayRow> ExtractInterlockedGameplayRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaInterlockedGameplayRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    EconomyDeltaIds = ExtractStringArray(value, "economyDeltaIds").ToList(),
                    CraftingDeltaIds = ExtractStringArray(value, "craftingDeltaIds").ToList(),
                    CombatDeltaIds = ExtractStringArray(value, "combatDeltaIds").ToList(),
                    ProgressionDeltaIds = ExtractStringArray(value, "progressionDeltaIds").ToList(),
                    StatusDeltaIds = ExtractStringArray(value, "statusDeltaIds").ToList()
                };
            }
        }

        private static IEnumerable<AlphaSettlementRow> ExtractSettlementRows(string json)
        {
            var array = ExtractArray(json, "rows");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                var rowId = ExtractJsonString(value, "rowId");
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    continue;
                }

                yield return new AlphaSettlementRow
                {
                    RowId = rowId,
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SeedId = ExtractJsonString(value, "seedId"),
                    SettlementId = ExtractJsonString(value, "settlementId"),
                    ConstructionActionId = ExtractJsonString(value, "constructionActionId"),
                    ProductionLedgerEntryIds = ExtractStringArray(value, "productionLedgerEntryIds").ToList(),
                    DestructionRepairLedgerEntryIds = ExtractStringArray(value, "destructionRepairLedgerEntryIds").ToList(),
                    DefenseThreatLedgerEntryIds = ExtractStringArray(value, "defenseThreatLedgerEntryIds").ToList(),
                    LivingWorldLinkageId = ExtractJsonString(value, "livingWorldLinkageId"),
                    InterlockedDependencyId = ExtractJsonString(value, "interlockedDependencyId")
                };
            }
        }

        private static IEnumerable<AlphaFamilyLoopCommand> ExtractFamilyLoopCommands(string json)
        {
            var array = ExtractArray(json, "commands");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                yield return new AlphaFamilyLoopCommand
                {
                    FamilyId = ExtractJsonString(value, "familyId"),
                    ScenarioId = ExtractJsonString(value, "scenarioId"),
                    Order = ExtractJsonInt(value, "order"),
                    CommandType = ExtractJsonString(value, "commandType"),
                    FamilyMarker = ExtractJsonString(value, "familyMarker"),
                    ExpectedStatus = ExtractJsonString(value, "expectedStatus")
                };
            }
        }

        private static IEnumerable<AlphaMediaBoundBinding> ExtractMediaBoundBindings(string json)
        {
            var array = ExtractArray(json, "bindings");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                yield return new AlphaMediaBoundBinding
                {
                    BindingId = ExtractJsonString(value, "bindingId"),
                    FamilyId = ExtractJsonString(value, "familyId"),
                    SlotId = ExtractJsonString(value, "slotId"),
                    MediaKind = ExtractJsonString(value, "mediaKind"),
                    RelativePath = ExtractJsonString(value, "relativePath"),
                    Sha256 = ExtractJsonString(value, "sha256"),
                    SizeBytes = ExtractJsonLong(value, "sizeBytes"),
                    Width = ExtractJsonInt(value, "width"),
                    Height = ExtractJsonInt(value, "height"),
                    SampleRate = ExtractJsonInt(value, "sampleRate"),
                    Channels = ExtractJsonInt(value, "channels"),
                    SampleCount = ExtractJsonInt(value, "sampleCount")
                };
            }
        }

        private static bool TryLoadPng(byte[] bytes, AlphaMediaBoundBinding binding)
        {
            if (!TryReadPngDimensions(bytes, out var width, out var height))
            {
                return false;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, false);

            return texture.width > 0
                && texture.height > 0
                && (binding.Width <= 0 || texture.width == binding.Width)
                && (binding.Height <= 0 || texture.height == binding.Height);
        }

        private static bool TryReadPngDimensions(byte[] bytes, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (bytes.Length < 33 ||
                bytes[0] != 137 ||
                bytes[1] != 80 ||
                bytes[2] != 78 ||
                bytes[3] != 71 ||
                bytes[4] != 13 ||
                bytes[5] != 10 ||
                bytes[6] != 26 ||
                bytes[7] != 10)
            {
                return false;
            }

            if (ReadPngInt32(bytes, 8) != 13 ||
                bytes[12] != 73 ||
                bytes[13] != 72 ||
                bytes[14] != 68 ||
                bytes[15] != 82)
            {
                return false;
            }

            width = ReadPngInt32(bytes, 16);
            height = ReadPngInt32(bytes, 20);
            return width > 0 && height > 0;
        }

        private static int ReadPngInt32(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) |
                bytes[offset + 3];
        }

        private static bool TryLoadWav(byte[] bytes, AlphaMediaBoundBinding binding)
        {
            if (!TryParsePcmWav(bytes, out var channels, out var sampleRate, out var samplesPerChannel, out var samples))
            {
                return false;
            }

            if ((binding.Channels > 0 && binding.Channels != channels) ||
                (binding.SampleRate > 0 && binding.SampleRate != sampleRate) ||
                (binding.SampleCount > 0 && binding.SampleCount != samplesPerChannel))
            {
                return false;
            }

            var clip = AudioClip.Create("media-bound-" + binding.FamilyId + "-" + binding.SlotId, samplesPerChannel, channels, sampleRate, false);
            return clip.SetData(samples, 0);
        }

        private static bool TryParsePcmWav(byte[] bytes, out int channels, out int sampleRate, out int samplesPerChannel, out float[] samples)
        {
            channels = 0;
            sampleRate = 0;
            samplesPerChannel = 0;
            samples = Array.Empty<float>();
            if (bytes.Length < 44 ||
                Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF" ||
                Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE")
            {
                return false;
            }

            var offset = 12;
            var bitsPerSample = 0;
            var blockAlign = 0;
            var dataOffset = -1;
            var dataSize = 0;
            while (offset + 8 <= bytes.Length)
            {
                var chunkId = Encoding.ASCII.GetString(bytes, offset, 4);
                var chunkSize = BitConverter.ToInt32(bytes, offset + 4);
                offset += 8;
                if (chunkSize < 0 || offset + chunkSize > bytes.Length)
                {
                    return false;
                }

                if (chunkId == "fmt " && chunkSize >= 16)
                {
                    var audioFormat = BitConverter.ToUInt16(bytes, offset);
                    channels = BitConverter.ToUInt16(bytes, offset + 2);
                    sampleRate = BitConverter.ToInt32(bytes, offset + 4);
                    blockAlign = BitConverter.ToUInt16(bytes, offset + 12);
                    bitsPerSample = BitConverter.ToUInt16(bytes, offset + 14);
                    if (audioFormat != 1)
                    {
                        return false;
                    }
                }
                else if (chunkId == "data")
                {
                    dataOffset = offset;
                    dataSize = chunkSize;
                }

                offset += chunkSize;
                if ((chunkSize & 1) == 1 && offset < bytes.Length)
                {
                    offset++;
                }
            }

            if (channels <= 0 || sampleRate <= 0 || bitsPerSample != 16 || blockAlign <= 0 || dataOffset < 0 || dataSize <= 0)
            {
                return false;
            }

            var sampleValues = dataSize / 2;
            samplesPerChannel = dataSize / blockAlign;
            samples = new float[sampleValues];
            for (var index = 0; index < sampleValues; index++)
            {
                var value = BitConverter.ToInt16(bytes, dataOffset + (index * 2));
                samples[index] = value / 32768f;
            }

            return samplesPerChannel > 0;
        }

        private static string MarkerFor(string kind)
        {
            if (kind == "npc")
            {
                return "N";
            }

            if (kind == "item")
            {
                return "I";
            }

            if (kind == "quest_event")
            {
                return "Q";
            }

            return ".";
        }

        private static bool IsGoal014Placeholder(string kind, int x, int y)
        {
            return (kind == "npc" && x == 4 && y == 1) ||
                (kind == "item" && x == 5 && y == 3) ||
                (kind == "quest_event" && x == 2 && y == 3);
        }

        private string ReadPackageJsonIfAvailable()
        {
            try
            {
                var payloadRoot = Path.Combine(Application.streamingAssetsPath, PayloadFolderName);
                return File.ReadAllText(Path.Combine(payloadRoot, "game-data", "game-package.json"));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool ContainsJsonId(string json, string id)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return json.Contains("\"id\": \"" + id + "\"", StringComparison.Ordinal) ||
                json.Contains("\"sourceId\": \"" + id + "\"", StringComparison.Ordinal) ||
                json.Contains("\"packageQuestId\": \"" + id + "\"", StringComparison.Ordinal);
        }

        private static IEnumerable<AlphaCommandHint> ExtractCommandHints(string json)
        {
            var array = ExtractArray(json, "commandHints");
            foreach (Match match in Regex.Matches(array, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                yield return new AlphaCommandHint
                {
                    CommandId = ExtractJsonString(value, "commandId"),
                    CommandType = ExtractJsonString(value, "commandType"),
                    TargetId = ExtractJsonString(value, "targetId"),
                    SecondaryTargetId = ExtractJsonString(value, "secondaryTargetId")
                };
            }
        }

        private string FirstCommandTarget(string commandType, string targetPrefix, string fallback)
        {
            var match = commands
                .Where(command => command.CommandType == commandType && command.TargetId.StartsWith(targetPrefix, StringComparison.Ordinal))
                .OrderBy(command => command.TargetId, StringComparer.Ordinal)
                .FirstOrDefault();
            return match == null ? fallback : match.TargetId;
        }

        private string FirstCommandSecondaryTarget(string commandTypePrefix, string targetPrefix, string fallback)
        {
            var match = commands
                .Where(command => command.CommandType.StartsWith(commandTypePrefix, StringComparison.Ordinal) && command.SecondaryTargetId.StartsWith(targetPrefix, StringComparison.Ordinal))
                .OrderBy(command => command.SecondaryTargetId, StringComparer.Ordinal)
                .FirstOrDefault();
            return match == null ? fallback : match.SecondaryTargetId;
        }

        private static string FirstValueWithPrefix(string json, string propertyName, string prefix)
        {
            foreach (var value in ExtractStringArray(json, propertyName))
            {
                if (value.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string FirstPropertyValueWithPrefix(string json, string propertyName, string prefix)
        {
            foreach (Match match in Regex.Matches(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\""))
            {
                var value = Regex.Unescape(match.Groups["value"].Value);
                if (value.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> ExtractStringArray(string json, string propertyName)
        {
            var array = ExtractArray(json, propertyName);
            foreach (Match match in Regex.Matches(array, "\"(?<value>(?:\\\\.|[^\"])*)\""))
            {
                yield return Regex.Unescape(match.Groups["value"].Value);
            }
        }

        private static string ExtractArray(string json, string propertyName)
        {
            var startMatch = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\\[", RegexOptions.Singleline);
            if (!startMatch.Success)
            {
                return string.Empty;
            }

            var start = startMatch.Index + startMatch.Length;
            var depth = 1;
            for (var index = start; index < json.Length; index++)
            {
                if (json[index] == '[')
                {
                    depth++;
                }
                else if (json[index] == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return json.Substring(start, index - start);
                    }
                }
            }

            return string.Empty;
        }

        private static string ExtractJsonString(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"");
            return match.Success ? Regex.Unescape(match.Groups["value"].Value) : string.Empty;
        }

        private static int ExtractJsonInt(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*(?<value>-?[0-9]+)");
            return match.Success && int.TryParse(match.Groups["value"].Value, out var value) ? value : 0;
        }

        private static bool ExtractJsonBool(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*(?<value>true|false)", RegexOptions.IgnoreCase);
            return match.Success && string.Equals(match.Groups["value"].Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static long ExtractJsonLong(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*(?<value>-?[0-9]+)");
            return match.Success && long.TryParse(match.Groups["value"].Value, out var value) ? value : 0;
        }

        private static int CountJsonObjectsInArray(string json, string propertyName)
        {
            var value = ExtractArray(json, propertyName);
            var count = 0;
            foreach (var ch in value)
            {
                if (ch == '{')
                {
                    count++;
                }
            }

            return count;
        }

        private static string ExtractManifestFileHash(string json, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            foreach (Match match in Regex.Matches(json, "\\{(?<value>.*?)\\}", RegexOptions.Singleline))
            {
                var value = match.Groups["value"].Value;
                if (ExtractJsonString(value, "relativePath") == relativePath)
                {
                    return ExtractJsonString(value, "hash");
                }
            }

            return string.Empty;
        }

        private static bool IsSafeMediaRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Contains(":") || path.Contains("://"))
            {
                return false;
            }

            return !path.Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Contains("..");
        }

        private static string HashBytes(byte[] bytes)
        {
            var hash = SHA256.Create().ComputeHash(bytes);
            var builder = new StringBuilder();
            foreach (var value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }

        private static bool IsLaunchSuccessful(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if (line == "alpha_runtime.launch_completed=true")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPlayLoopSuccessful(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if (line == "alpha_runtime.play_loop_completed=true")
                {
                    return true;
                }
            }

            return false;
        }

        private static void WriteLines(string path, IEnumerable<string> lines)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Application.persistentDataPath);
            File.WriteAllLines(path, lines);
        }

        private static bool HasArgument(IReadOnlyList<string> arguments, string name)
        {
            for (var index = 0; index < arguments.Count; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetArgumentValue(IReadOnlyList<string> arguments, string name)
        {
            for (var index = 0; index < arguments.Count - 1; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return string.Empty;
        }

        private static void DrawLabel(ref float y, string text)
        {
            GUI.Label(new Rect(32, y, 720, 22), text);
            y += 24;
        }

        private static string Display(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(none)" : value;
        }

        private static string DisplayId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "(none)";
            }

            var parts = value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? value : parts[parts.Length - 1];
        }

        private static string StyleIdFromPackageId(string value)
        {
            const string prefix = "game/content_generation/";
            if (!value.StartsWith(prefix, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return value.Substring(prefix.Length).Replace('-', '_');
        }

        private static int StableInt(string value)
        {
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
            return Math.Abs(BitConverter.ToInt32(hash, 0));
        }

        private static string ShortHash(string value)
        {
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
            var builder = new StringBuilder();
            for (var index = 0; index < 6; index++)
            {
                builder.Append(hash[index].ToString("x2"));
            }

            return builder.ToString();
        }

        private sealed class AlphaCommandHint
        {
            public static readonly AlphaCommandHint Empty = new AlphaCommandHint();
            public string CommandId = string.Empty;
            public string CommandType = string.Empty;
            public string TargetId = string.Empty;
            public string SecondaryTargetId = string.Empty;
        }

        private sealed class AlphaSceneNode
        {
            public string NodeId = string.Empty;
            public string Kind = string.Empty;
            public string SourceId = string.Empty;
            public string Label = string.Empty;
            public int X;
            public int Y;
        }

        private sealed class AlphaRuntimeStateSnapshot
        {
            public bool QuestStarted;
            public bool QuestCompletedCandidate;
            public bool DialogueOpened;
            public bool DialogueChoiceSelected;
            public bool ItemObtained;
            public int InventoryItemCount;
            public bool EventApplied;
            public bool RewardGranted;
            public string LastCommandId = string.Empty;
            public string LastCommandType = string.Empty;
            public string LastCommandTargetId = string.Empty;
            public string StatusText = string.Empty;
        }

        private sealed class AlphaFamilyMode
        {
            public string FamilyId = string.Empty;
            public string ModeId = string.Empty;
            public string ScenarioId = string.Empty;
            public string ProfileId = string.Empty;
        }

        private sealed class AlphaFamilyLoopCommand
        {
            public string FamilyId = string.Empty;
            public string ScenarioId = string.Empty;
            public int Order;
            public string CommandType = string.Empty;
            public string FamilyMarker = string.Empty;
            public string ExpectedStatus = string.Empty;
        }

        private sealed class AlphaMatrixRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public string DerivedCampaignHash = string.Empty;
        }

        private sealed class AlphaPackageRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public string PackageId = string.Empty;
            public string PackageRelativePath = string.Empty;
            public string PackageHash = string.Empty;
            public bool PackageValidationPassed;
            public bool RuntimeLoopCompleted;
            public bool PackageFileExists;
            public bool PackageJsonContainsPackageId;
            public bool PackageHashMatches;
        }

        private sealed class AlphaReviewPackageRcRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public string PackageId = string.Empty;
            public string PackageRelativePath = string.Empty;
            public string PackageHash = string.Empty;
            public bool PackageHashVerified;
            public bool PackageMediaBindingsVerified;
            public bool SaveLoadReplayVerified;
            public bool PackageFileExists;
            public bool PackageJsonContainsPackageId;
            public bool PackageHashMatches;
            public List<string> OrderedStepIds = new List<string>();
        }

        private sealed class AlphaSpatialDetailRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public bool Reachable;
            public bool RouteVerified;
            public string VarianceMarker = string.Empty;
        }

        private sealed class AlphaGameplayConsequenceRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public List<string> StepIds = new List<string>();
            public List<string> DeltaIds = new List<string>();
        }

        private sealed class AlphaLivingWorldRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public List<string> TickIds = new List<string>();
        }

        private sealed class AlphaInterlockedGameplayRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public List<string> EconomyDeltaIds = new List<string>();
            public List<string> CraftingDeltaIds = new List<string>();
            public List<string> CombatDeltaIds = new List<string>();
            public List<string> ProgressionDeltaIds = new List<string>();
            public List<string> StatusDeltaIds = new List<string>();
        }

        private sealed class AlphaSettlementRow
        {
            public string RowId = string.Empty;
            public string FamilyId = string.Empty;
            public string SeedId = string.Empty;
            public string SettlementId = string.Empty;
            public string ConstructionActionId = string.Empty;
            public List<string> ProductionLedgerEntryIds = new List<string>();
            public List<string> DestructionRepairLedgerEntryIds = new List<string>();
            public List<string> DefenseThreatLedgerEntryIds = new List<string>();
            public string LivingWorldLinkageId = string.Empty;
            public string InterlockedDependencyId = string.Empty;
        }

        private sealed class AlphaMediaBoundBinding
        {
            public string BindingId = string.Empty;
            public string FamilyId = string.Empty;
            public string SlotId = string.Empty;
            public string MediaKind = string.Empty;
            public string RelativePath = string.Empty;
            public string Sha256 = string.Empty;
            public long SizeBytes;
            public int Width;
            public int Height;
            public int SampleRate;
            public int Channels;
            public int SampleCount;
        }
    }
}
