using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerCommandRoundtripSmokeResult
    {
        public bool Passed;
        public string ModelPath = string.Empty;
        public string ResultPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public sealed class CanonicalRuntimeUnityPlayerCommandRoundtripModelView
    {
        public string CandidateId = string.Empty;
        public int RoundtripRequestCount;
        public int RuntimeExecutedRequestCount;
        public int RoundtripSnapshotCount;
        public string CurrentRequest = string.Empty;
        public string CurrentResponseSnapshot = string.Empty;
        public string Status = string.Empty;
    }

    public static class CanonicalRuntimeUnityPlayerCommandRoundtripHarness
    {
        public const string PassMarker =
            "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_PASS";
        public const string FailMarker =
            "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_FAIL";

        private static readonly string[] RequiredControlIntents =
        {
            "load_model",
            "reset_first",
            "step_once",
            "next_frame",
            "play_all_to_end",
            "copy_frame_summary"
        };

        private static readonly string[] RequiredRuntimeCommandCoverage =
        {
            "load_package_or_session",
            "show_or_select_start_state",
            "advance_to_interaction",
            "advance_to_dialogue_or_quest",
            "advance_to_inventory_or_crafting",
            "advance_to_combat_or_final_state"
        };

        public static CanonicalRuntimeUnityPlayerCommandRoundtripSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var modelPath = ReadArgument(
                args,
                "-llmgcRuntimeBackedPlayerCommandRoundtripModelPath");
            var resultPath = ReadArgument(
                args,
                "-llmgcRuntimeBackedPlayerCommandRoundtripResultPath");
            return Consume(modelPath, resultPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeRuntimeBackedPlayerCommandRoundtripSmoke()
        {
            var exitCode = 0;
            try
            {
                var result = RunFromCommandLine();
                if (result.Passed)
                {
                    Debug.Log(PassMarker + "\n" + result.Diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError(FailMarker + "\n" + result.Diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError(FailMarker + "\n" + ex);
            }
            finally
            {
                if (Application.isBatchMode) { EditorApplication.Exit(exitCode); }
            }
        }
#endif

        public static CanonicalRuntimeUnityPlayerCommandRoundtripSmokeResult Consume(
            string modelPath,
            string resultPath)
        {
            var result = new CanonicalRuntimeUnityPlayerCommandRoundtripSmokeResult
            {
                ModelPath = modelPath ?? string.Empty,
                ResultPath = resultPath ?? string.Empty
            };
            var modelPathExists = !string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath);
            var resultPathExists = !string.IsNullOrWhiteSpace(resultPath) && File.Exists(resultPath);
            if (!modelPathExists || !resultPathExists)
            {
                result.Diagnostics =
                    "modelPathExists="
                    + modelPathExists
                    + "; resultPathExists="
                    + resultPathExists
                    + "; roundtripRequestCountPassed=False"
                    + "; runtimeSnapshotResponsePresent=False"
                    + "; runtimeAuthorityMarkersPresent=False"
                    + "; unityConsumesRoundtripResult=False"
                    + "; unityGameplayTruth=False"
                    + "; passMarkerPresent=False"
                    + "; failMarkerPresent=False";
                return result;
            }

            var model = File.ReadAllText(modelPath);
            var roundtrip = File.ReadAllText(resultPath);
            var roundtripRequestCountPassed =
                ContainsJsonNumberAtLeast(model, "roundtripRequestCount", 6)
                && ContainsJsonNumberAtLeast(roundtrip, "roundtripRequestCount", 6);
            foreach (var control in RequiredControlIntents)
            {
                roundtripRequestCountPassed = roundtripRequestCountPassed
                                              && ContainsJsonPair(roundtrip, "controlIntent", control);
            }

            foreach (var coverage in RequiredRuntimeCommandCoverage)
            {
                roundtripRequestCountPassed = roundtripRequestCountPassed
                                              && ContainsJsonPair(roundtrip, "runtimeCommandCoverage", coverage);
            }

            var runtimeSnapshotResponsePresent =
                ContainsJsonNumberAtLeast(roundtrip, "runtimeExecutedRequestCount", 6)
                && ContainsJsonNumberAtLeast(roundtrip, "roundtripSnapshotCount", 6)
                && ContainsJsonBool(roundtrip, "runtimeExecuted", true)
                && roundtrip.Contains("\"responses\"", StringComparison.Ordinal)
                && roundtrip.Contains("\"snapshots\"", StringComparison.Ordinal)
                && roundtrip.Contains("\"stateHashAfter\"", StringComparison.Ordinal);
            var runtimeAuthorityMarkersPresent =
                ContainsJsonBool(model, "runtimeAuthority", true)
                && ContainsJsonBool(roundtrip, "runtimeAuthority", true)
                && ContainsJsonBool(model, "projectionOnly", false)
                && ContainsJsonBool(roundtrip, "projectionOnly", false)
                && ContainsJsonBool(model, "unityGameplayTruth", false)
                && ContainsJsonBool(roundtrip, "unityGameplayTruth", false)
                && ContainsJsonBool(roundtrip, "stateHashChainPresent", true)
                && ContainsJsonBool(roundtrip, "controlRequestBridgePresent", true);
            var unityConsumesRoundtripResult =
                ContainsJsonBool(model, "unityConsumesRoundtripResult", true)
                && ContainsJsonBool(roundtrip, "unityConsumesRoundtripResult", true);
            var unityGameplayTruth =
                ContainsJsonBool(model, "unityGameplayTruth", true)
                || ContainsJsonBool(roundtrip, "unityGameplayTruth", true);
            result.Passed = modelPathExists
                            && resultPathExists
                            && roundtripRequestCountPassed
                            && runtimeSnapshotResponsePresent
                            && runtimeAuthorityMarkersPresent
                            && unityConsumesRoundtripResult
                            && !unityGameplayTruth;
            result.Diagnostics =
                "modelPathExists="
                + modelPathExists
                + "; roundtripRequestCountPassed="
                + roundtripRequestCountPassed
                + "; runtimeSnapshotResponsePresent="
                + runtimeSnapshotResponsePresent
                + "; runtimeAuthorityMarkersPresent="
                + runtimeAuthorityMarkersPresent
                + "; unityConsumesRoundtripResult="
                + unityConsumesRoundtripResult
                + "; unityGameplayTruth="
                + unityGameplayTruth
                + "; passMarkerPresent="
                + result.Passed
                + "; failMarkerPresent=False"
                + "; modelPath="
                + modelPath
                + "; resultPath="
                + resultPath;
            return result;
        }

        public static CanonicalRuntimeUnityPlayerCommandRoundtripModelView LoadModelView(
            string modelPath)
        {
            var view = new CanonicalRuntimeUnityPlayerCommandRoundtripModelView();
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                view.Status = "Goal141 command roundtrip model not found.";
                return view;
            }

            var text = File.ReadAllText(modelPath);
            view.CandidateId = ExtractString(text, "candidateId");
            view.RoundtripRequestCount = ExtractInt(text, "roundtripRequestCount");
            view.RuntimeExecutedRequestCount = ExtractInt(text, "runtimeExecutedRequestCount");
            view.RoundtripSnapshotCount = ExtractInt(text, "roundtripSnapshotCount");
            view.CurrentRequest =
                ExtractString(text, "controlIntent")
                + " -> "
                + ExtractString(text, "runtimeCommandCoverage");
            view.CurrentResponseSnapshot = ExtractString(text, "stateHashAfter");
            view.Status = ExtractString(text, "status");
            return view;
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.Ordinal))
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }

        private static bool ContainsJsonPair(string text, string property, string value) =>
            text.Contains("\"" + property + "\": \"" + value + "\"", StringComparison.Ordinal);

        private static bool ContainsJsonBool(string text, string property, bool value) =>
            text.Contains("\"" + property + "\": " + value.ToString().ToLowerInvariant(), StringComparison.Ordinal);

        private static bool ContainsJsonNumberAtLeast(string text, string property, int minimum) =>
            ExtractInt(text, property) >= minimum;

        private static int ExtractInt(string text, string property)
        {
            var marker = "\"" + property + "\":";
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
            {
                return 0;
            }

            index += marker.Length;
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            var start = index;
            while (index < text.Length && char.IsDigit(text[index]))
            {
                index++;
            }

            if (start == index)
            {
                return 0;
            }

            return int.TryParse(text.Substring(start, index - start), out var value) ? value : 0;
        }

        private static string ExtractString(string text, string property)
        {
            var marker = "\"" + property + "\": \"";
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
            {
                return string.Empty;
            }

            index += marker.Length;
            var end = text.IndexOf('"', index);
            return end > index ? text.Substring(index, end - index) : string.Empty;
        }
    }
}
