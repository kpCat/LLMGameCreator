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
        private string packageId = string.Empty;
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
        private int playerX = 1;
        private int playerY = 1;
        private int mapWidth = 7;
        private int mapHeight = 5;
        private int focusedTargetIndex;

        private void Start()
        {
            Application.targetFrameRate = 30;
            var arguments = Environment.GetCommandLineArgs();
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
            GUI.Box(new Rect(16, 16, 760, 560), "LLMGameCreator Alpha");
            var y = 48f;
            DrawLabel(ref y, "Package: " + Display(packageId));
            DrawLabel(ref y, "Style: " + Display(selectedStyleId));
            DrawLabel(ref y, "Thread: " + Display(selectedThreadId));
            DrawLabel(ref y, "Start map: " + Display(startMapId));
            DrawLabel(ref y, "Quest: " + Display(selectedQuestId));
            DrawLabel(ref y, "Dialogue: " + Display(selectedDialogueId));
            DrawLabel(ref y, "Item: " + Display(selectedItemId));
            DrawLabel(ref y, "Event: " + Display(selectedEventId));
            DrawLabel(ref y, "Command hints: " + commands.Count + "    Asset refs: " + assetRefCount);
            DrawLabel(ref y, "Player: (" + playerX + "," + playerY + ")    Focus: " + FocusName());
            DrawLabel(ref y, "Status: " + status);
            DrawLabel(ref y, "Quest phase: " + CurrentQuestPhase() + "    Reward: " + rewardGranted.ToString().ToLowerInvariant());
            DrawLabel(ref y, "Objectives: start=" + questStarted.ToString().ToLowerInvariant() +
                " dialogue=" + dialogueSeen.ToString().ToLowerInvariant() +
                " choice=" + dialogueChoiceSelected.ToString().ToLowerInvariant() +
                " item=" + itemObtained.ToString().ToLowerInvariant() +
                " event=" + eventApplied.ToString().ToLowerInvariant() +
                " complete=" + questCompletedCandidate.ToString().ToLowerInvariant());
            DrawLabel(ref y, "State: quest=" + questStarted.ToString().ToLowerInvariant() +
                " completeCandidate=" + questCompletedCandidate.ToString().ToLowerInvariant() +
                " dialogue=" + dialogueSeen.ToString().ToLowerInvariant() +
                " choice=" + dialogueChoiceSelected.ToString().ToLowerInvariant() +
                " item=" + itemObtained.ToString().ToLowerInvariant() +
                " inventory=" + inventoryItemCount +
                " event=" + eventApplied.ToString().ToLowerInvariant());
            DrawLabel(ref y, "Last command: " + Display(lastCommandId) + " / " + Display(lastCommandType) + " / " + Display(lastCommandTargetId));

            DrawMap(32, y + 8);

            if (GUI.Button(new Rect(340, y + 8, 160, 36), "Interact"))
            {
                InteractWithFocusedTarget();
            }

            if (GUI.Button(new Rect(512, y + 8, 120, 36), "Reset"))
            {
                ResetLoop();
            }

            GUI.Label(new Rect(340, y + 52, 420, 48), "WASD/arrows move. Tab focus. Space/Enter interact. R reset. Esc quit.");
            y += 174;

            GUI.Box(new Rect(32, y, 720, 230), "Play log");
            var logY = y + 28;
            var start = Math.Max(0, playLog.Count - 8);
            for (var index = start; index < playLog.Count; index++)
            {
                GUI.Label(new Rect(48, logY, 688, 22), playLog[index]);
                logY += 24;
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
                BuildSceneProjection();
                payloadLoaded = payloadRootExists && commands.Count > 0;
                ResetLoop();

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
    }
}
