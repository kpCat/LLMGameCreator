using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimePlayerCommandLoopSmokeResult
    {
        public bool Passed;
        public string SnapshotsPath = string.Empty;
        public string ResultPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public static class CanonicalRuntimePlayerCommandLoopAdapter
    {
        public const string PassMarker = "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_PASS";
        public const string FailMarker = "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_FAIL";

        private static readonly string[] RequiredCategories =
        {
            "load_package",
            "start_runtime",
            "move",
            "interact",
            "show_dialogue",
            "start_or_update_quest",
            "show_inventory",
            "craft",
            "harvest",
            "transaction",
            "encounter",
            "combat_round",
            "final_state"
        };

        public static CanonicalRuntimePlayerCommandLoopSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var snapshotsPath = ReadArgument(args, "-llmgcCanonicalRuntimePlayerCommandLoopSnapshotsPath");
            var resultPath = ReadArgument(args, "-llmgcCanonicalRuntimePlayerCommandLoopResultPath");
            return Consume(snapshotsPath, resultPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeCanonicalRuntimePlayerCommandLoopSmoke()
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

        public static CanonicalRuntimePlayerCommandLoopSmokeResult Consume(
            string snapshotsPath,
            string resultPath)
        {
            var result = new CanonicalRuntimePlayerCommandLoopSmokeResult
            {
                SnapshotsPath = snapshotsPath ?? string.Empty,
                ResultPath = resultPath ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(snapshotsPath) || !File.Exists(snapshotsPath))
            {
                result.Diagnostics = "command-loop snapshots path is missing";
                return result;
            }

            if (string.IsNullOrWhiteSpace(resultPath) || !File.Exists(resultPath))
            {
                result.Diagnostics = "command-loop result path is missing";
                return result;
            }

            var snapshots = File.ReadAllText(snapshotsPath);
            var commandLoop = File.ReadAllText(resultPath);
            var categoriesPresent = true;
            foreach (var category in RequiredCategories)
            {
                categoriesPresent = categoriesPresent
                                    && ContainsJsonPair(snapshots, "category", category);
            }

            var snapshotContractPresent =
                ContainsJsonPair(commandLoop, "candidateId", "minimal-map-game-balanced-baseline")
                && ContainsJsonBool(commandLoop, "playerCommandLoopPassed", true)
                && ContainsJsonBool(commandLoop, "projectionOnly", false)
                && ContainsJsonBool(commandLoop, "unityGameplayTruth", false)
                && ContainsJsonBool(commandLoop, "selectedCandidateExecutedByRuntime", true)
                && commandLoop.Contains("\"playerCommandCount\": 13")
                && commandLoop.Contains("\"playerSnapshotCount\": 13")
                && snapshots.Contains("\"stateHashBefore\"")
                && snapshots.Contains("\"stateHashAfter\"")
                && snapshots.Contains("\"runtimeEvents\"");
            result.Passed = categoriesPresent && snapshotContractPresent;
            result.Diagnostics =
                "requiredCategoriesPresent="
                + categoriesPresent
                + "; snapshotContractPresent="
                + snapshotContractPresent
                + "; snapshotsPath="
                + snapshotsPath
                + "; resultPath="
                + resultPath;
            return result;
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }

        private static bool ContainsJsonPair(string json, string name, string value)
        {
            return json.Contains("\"" + name + "\": \"" + value + "\"")
                   || json.Contains("\"" + name + "\":\"" + value + "\"");
        }

        private static bool ContainsJsonBool(string json, string name, bool value)
        {
            var text = value ? "true" : "false";
            return json.Contains("\"" + name + "\": " + text)
                   || json.Contains("\"" + name + "\":" + text);
        }
    }
}
