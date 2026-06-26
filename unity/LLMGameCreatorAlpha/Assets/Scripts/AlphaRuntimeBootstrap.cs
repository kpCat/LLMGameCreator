using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class AlphaRuntimeBootstrap : MonoBehaviour
    {
        private const string PayloadFolderName = "LLMGameCreatorAlpha";

        private readonly List<string> playLog = new List<string>();
        private readonly List<AlphaCommandHint> commands = new List<AlphaCommandHint>();
        private string packageId = string.Empty;
        private string packageHash = string.Empty;
        private string assetManifestHash = string.Empty;
        private string startMapId = string.Empty;
        private string selectedThreadId = string.Empty;
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
        private bool eventApplied;
        private string status = "Loading Alpha payload...";

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AdvanceCommand();
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
            DrawLabel(ref y, "Thread: " + Display(selectedThreadId));
            DrawLabel(ref y, "Start map: " + Display(startMapId));
            DrawLabel(ref y, "Quest: " + Display(selectedQuestId));
            DrawLabel(ref y, "Dialogue: " + Display(selectedDialogueId));
            DrawLabel(ref y, "Item: " + Display(selectedItemId));
            DrawLabel(ref y, "Event: " + Display(selectedEventId));
            DrawLabel(ref y, "Command hints: " + commands.Count + "    Asset refs: " + assetRefCount);
            DrawLabel(ref y, "Status: " + status);
            DrawLabel(ref y, "State: quest=" + questStarted.ToString().ToLowerInvariant() +
                " dialogue=" + dialogueSeen.ToString().ToLowerInvariant() +
                " item=" + itemObtained.ToString().ToLowerInvariant() +
                " event=" + eventApplied.ToString().ToLowerInvariant());

            if (GUI.Button(new Rect(32, y + 8, 160, 36), "Advance"))
            {
                AdvanceCommand();
            }

            if (GUI.Button(new Rect(204, y + 8, 120, 36), "Reset"))
            {
                ResetLoop();
            }

            GUI.Label(new Rect(340, y + 14, 420, 24), "Space advances, R resets, Esc quits.");
            y += 60;

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
                var payloadRootExists = Directory.Exists(payloadRoot);
                var configJson = File.ReadAllText(configPath);
                var packageJson = File.ReadAllText(packagePath);
                var assetManifestJson = File.ReadAllText(assetManifestPath);

                packageId = ExtractJsonString(configJson, "packageId");
                packageHash = ExtractJsonString(configJson, "packageHash");
                assetManifestHash = ExtractJsonString(configJson, "assetManifestHash");
                startMapId = ExtractJsonString(configJson, "startMapId");
                selectedThreadId = ExtractJsonString(configJson, "selectedThreadId");
                selectedQuestId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "quest/");
                selectedDialogueId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "dialogue/");
                selectedItemId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "item/");
                selectedEventId = FirstValueWithPrefix(configJson, "selectedGeneratedIds", "event/");
                assetRefCount = CountJsonObjectsInArray(configJson, "assetRefs");
                commands.Clear();
                commands.AddRange(ExtractCommandHints(configJson));
                payloadLoaded = payloadRootExists && commands.Count > 0;
                ResetLoop();

                lines.Add("alpha_runtime.payload_root_exists=" + payloadRootExists.ToString().ToLowerInvariant());
                lines.Add("alpha_runtime.config_loaded=true");
                lines.Add("alpha_runtime.package_loaded=true");
                lines.Add("alpha_runtime.asset_manifest_loaded=true");
                lines.Add("alpha_runtime.package_id=" + packageId);
                lines.Add("alpha_runtime.package_hash=" + packageHash);
                lines.Add("alpha_runtime.asset_manifest_hash=" + assetManifestHash);
                lines.Add("alpha_runtime.start_map_id=" + startMapId);
                lines.Add("alpha_runtime.selected_thread_id=" + selectedThreadId);
                lines.Add("alpha_runtime.selected_quest_id=" + selectedQuestId);
                lines.Add("alpha_runtime.selected_dialogue_id=" + selectedDialogueId);
                lines.Add("alpha_runtime.selected_item_id=" + selectedItemId);
                lines.Add("alpha_runtime.selected_event_id=" + selectedEventId);
                lines.Add("alpha_runtime.command_hint_count=" + commands.Count);
                lines.Add("alpha_runtime.asset_ref_count=" + assetRefCount);
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
                "alpha_runtime.payload_root_exists=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.config_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.package_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.asset_manifest_loaded=" + payloadLoaded.ToString().ToLowerInvariant(),
                "alpha_runtime.package_id=" + packageId,
                "alpha_runtime.package_hash=" + packageHash,
                "alpha_runtime.asset_manifest_hash=" + assetManifestHash,
                "alpha_runtime.start_map_id=" + startMapId,
                "alpha_runtime.selected_thread_id=" + selectedThreadId,
                "alpha_runtime.selected_quest_id=" + selectedQuestId,
                "alpha_runtime.selected_dialogue_id=" + selectedDialogueId,
                "alpha_runtime.selected_item_id=" + selectedItemId,
                "alpha_runtime.selected_event_id=" + selectedEventId,
                "alpha_runtime.command_hint_count=" + commands.Count,
                "alpha_runtime.asset_ref_count=" + assetRefCount
            };

            var packageJson = ReadPackageJsonIfAvailable();
            lines.Add("alpha_runtime.ref_resolved.map=" + ContainsJsonId(packageJson, startMapId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.quest=" + ContainsJsonId(packageJson, selectedQuestId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.dialogue=" + ContainsJsonId(packageJson, selectedDialogueId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.item=" + ContainsJsonId(packageJson, selectedItemId).ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.ref_resolved.event=" + ContainsJsonId(packageJson, selectedEventId).ToString().ToLowerInvariant());

            ResetLoop();
            for (var index = 0; index < commands.Count; index++)
            {
                var command = AdvanceCommand();
                lines.Add("alpha_runtime.command_executed." + index + ".id=" + command.CommandId);
                lines.Add("alpha_runtime.command_executed." + index + ".type=" + command.CommandType);
                lines.Add("alpha_runtime.command_executed." + index + ".target_id=" + command.TargetId);
                lines.Add("alpha_runtime.command_executed." + index + ".secondary_target_id=" + command.SecondaryTargetId);
            }

            lines.Add("alpha_runtime.state_transition.quest_start=" + questStarted.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.dialogue_open=" + dialogueSeen.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.dialogue_choice=" + dialogueChoiceSelected.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.item_or_loot=" + itemObtained.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.state_transition.event_application=" + eventApplied.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.quest_started=" + questStarted.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.dialogue_seen=" + dialogueSeen.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.item_obtained=" + itemObtained.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.event_applied=" + eventApplied.ToString().ToLowerInvariant());
            lines.Add("alpha_runtime.commands_executed=" + currentCommandIndex);
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
            status = "Executed " + command.CommandType + " -> " + Display(command.TargetId);
            playLog.Add(currentCommandIndex + ". " + command.CommandType + " -> " + Display(command.TargetId));
            return command;
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
                dialogueSeen = dialogueChoiceSelected || dialogueSeen;
                questStarted = !string.IsNullOrWhiteSpace(command.SecondaryTargetId) || questStarted;
            }
            else if (command.CommandType == "loot/roll")
            {
                itemObtained = !string.IsNullOrWhiteSpace(selectedItemId);
            }
            else if (command.CommandType.StartsWith("event/", StringComparison.Ordinal))
            {
                eventApplied = !string.IsNullOrWhiteSpace(command.SecondaryTargetId) || !string.IsNullOrWhiteSpace(selectedEventId);
                itemObtained = command.CommandType == "event/add_item" || itemObtained;
            }
        }

        private void ResetLoop()
        {
            currentCommandIndex = 0;
            questStarted = false;
            dialogueSeen = false;
            dialogueChoiceSelected = false;
            itemObtained = false;
            eventApplied = false;
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
                eventApplied;
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

        private sealed class AlphaCommandHint
        {
            public static readonly AlphaCommandHint Empty = new AlphaCommandHint();
            public string CommandId = string.Empty;
            public string CommandType = string.Empty;
            public string TargetId = string.Empty;
            public string SecondaryTargetId = string.Empty;
        }
    }
}
