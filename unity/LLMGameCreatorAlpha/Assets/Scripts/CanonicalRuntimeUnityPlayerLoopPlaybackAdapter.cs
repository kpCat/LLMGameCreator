using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerLoopPlaybackSmokeResult
    {
        public bool Passed;
        public string FramesPath = string.Empty;
        public string ResultPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public static class CanonicalRuntimeUnityPlayerLoopPlaybackAdapter
    {
        public const string PassMarker = "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_PASS";
        public const string FailMarker = "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_FAIL";

        private static readonly string[] RequiredFrameCategories =
        {
            "hud",
            "player_position",
            "interaction",
            "dialogue",
            "quest",
            "inventory",
            "crafting",
            "harvest",
            "transaction",
            "encounter",
            "combat",
            "final_state"
        };

        public static CanonicalRuntimeUnityPlayerLoopPlaybackSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var framesPath = ReadArgument(args, "-llmgcCanonicalRuntimePlaybackFramesPath");
            var resultPath = ReadArgument(args, "-llmgcCanonicalRuntimePlaybackResultPath");
            return Consume(framesPath, resultPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeCanonicalRuntimeUnityPlayerLoopPlaybackSmoke()
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

        public static CanonicalRuntimeUnityPlayerLoopPlaybackSmokeResult Consume(
            string framesPath,
            string resultPath)
        {
            var result = new CanonicalRuntimeUnityPlayerLoopPlaybackSmokeResult
            {
                FramesPath = framesPath ?? string.Empty,
                ResultPath = resultPath ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(framesPath) || !File.Exists(framesPath))
            {
                result.Diagnostics = "playback frames path is missing";
                return result;
            }

            if (string.IsNullOrWhiteSpace(resultPath) || !File.Exists(resultPath))
            {
                result.Diagnostics = "playback result path is missing";
                return result;
            }

            var frames = File.ReadAllText(framesPath);
            var playback = File.ReadAllText(resultPath);
            var frameCount = CountJsonProperty(frames, "frameIndex");
            var frameCountPassed = frameCount >= 13
                                   || playback.Contains("\"playbackFrameCount\": 13");
            var requiredFrameCategoriesPresent = true;
            foreach (var category in RequiredFrameCategories)
            {
                requiredFrameCategoriesPresent = requiredFrameCategoriesPresent
                                                 && ContainsJsonPair(frames, "category", category);
            }

            var runtimeAuthorityMarkersPresent =
                ContainsJsonBool(playback, "runtimeSnapshotSource", true)
                && ContainsJsonBool(playback, "unityConsumesRuntimeSnapshots", true)
                && ContainsJsonBool(playback, "selectedCandidateExecutedByRuntime", true)
                && ContainsJsonBool(playback, "unityGameplayTruth", false)
                && ContainsJsonBool(playback, "projectionOnly", false)
                && frames.Contains("\"runtimeSnapshotSource\": true")
                && frames.Contains("\"canonicalRuntimeAuthority\": true");
            result.Passed = frameCountPassed
                            && requiredFrameCategoriesPresent
                            && runtimeAuthorityMarkersPresent;
            result.Diagnostics =
                "frameCount="
                + frameCount
                + "; frameCountPassed="
                + frameCountPassed
                + "; requiredFrameCategoriesPresent="
                + requiredFrameCategoriesPresent
                + "; runtimeAuthorityMarkersPresent="
                + runtimeAuthorityMarkersPresent
                + "; framesPath="
                + framesPath
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

        private static int CountJsonProperty(string json, string name)
        {
            var needle = "\"" + name + "\"";
            var count = 0;
            var index = 0;
            while (index >= 0)
            {
                index = json.IndexOf(needle, index, StringComparison.Ordinal);
                if (index >= 0)
                {
                    count++;
                    index += needle.Length;
                }
            }

            return count;
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
