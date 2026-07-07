using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimePlayerLoopReadinessSmokeResult
    {
        public bool Passed;
        public string PlanPath = string.Empty;
        public string StateSummaryPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public static class CanonicalRuntimePlayerLoopReadinessAdapter
    {
        public const string PassMarker = "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_PASS";
        public const string FailMarker = "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_FAIL";

        private static readonly string[] RequiredStepCategories =
        {
            "load_package",
            "show_start_state",
            "show_map_position",
            "show_interaction_result",
            "show_dialogue",
            "show_quest_state",
            "show_inventory_state",
            "show_crafting_result",
            "show_harvest_result",
            "show_transaction_result",
            "show_encounter_state",
            "show_combat_round",
            "show_final_state"
        };

        public static CanonicalRuntimePlayerLoopReadinessSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var planPath = ReadArgument(args, "-llmgcCanonicalRuntimePlayerLoopPlanPath");
            var stateSummaryPath = ReadArgument(args, "-llmgcCanonicalRuntimeStateSummaryPath");
            return Consume(planPath, stateSummaryPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeCanonicalRuntimePlayerLoopReadinessSmoke()
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

        public static CanonicalRuntimePlayerLoopReadinessSmokeResult Consume(
            string planPath,
            string stateSummaryPath)
        {
            var result = new CanonicalRuntimePlayerLoopReadinessSmokeResult
            {
                PlanPath = planPath ?? string.Empty,
                StateSummaryPath = stateSummaryPath ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(planPath) || !File.Exists(planPath))
            {
                result.Diagnostics = "player loop plan path is missing";
                return result;
            }

            if (string.IsNullOrWhiteSpace(stateSummaryPath) || !File.Exists(stateSummaryPath))
            {
                result.Diagnostics = "canonical runtime state summary path is missing";
                return result;
            }

            var plan = File.ReadAllText(planPath);
            var stateSummary = File.ReadAllText(stateSummaryPath);
            var requiredStepCategoriesPresent = true;
            foreach (var category in RequiredStepCategories)
            {
                requiredStepCategoriesPresent = requiredStepCategoriesPresent
                                                && ContainsJsonPair(plan, "category", category);
            }

            var canonicalAuthorityMarkersPresent =
                ContainsJsonBool(plan, "canonicalRuntimeSource", true)
                && ContainsJsonBool(plan, "unityGameplayTruth", false)
                && ContainsJsonBool(plan, "projectionOnly", false)
                && ContainsJsonBool(plan, "canonicalRuntimeAuthority", true);
            var stateSummaryReady =
                ContainsJsonPair(stateSummary, "candidateId", "minimal-map-game-balanced-baseline")
                && stateSummary.Contains("\"finalStateHash\"")
                && stateSummary.Contains("\"stateHashChain\"");

            result.Passed = requiredStepCategoriesPresent
                            && canonicalAuthorityMarkersPresent
                            && stateSummaryReady;
            result.Diagnostics =
                "requiredStepCategoriesPresent="
                + requiredStepCategoriesPresent
                + "; canonicalAuthorityMarkersPresent="
                + canonicalAuthorityMarkersPresent
                + "; stateSummaryReady="
                + stateSummaryReady
                + "; planPath="
                + planPath
                + "; stateSummaryPath="
                + stateSummaryPath;
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
