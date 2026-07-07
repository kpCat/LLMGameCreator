using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityPlayerLoopInteractiveControlsSmokeResult
    {
        public bool Passed;
        public string ModelPath = string.Empty;
        public string ControlScriptPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public sealed class CanonicalRuntimeUnityPlayerLoopInteractiveControlsModelView
    {
        public string CandidateId = string.Empty;
        public int FrameCount;
        public int CurrentFrameIndex;
        public readonly List<string> Controls = new();
        public readonly List<CanonicalRuntimeUnityPlayerLoopInteractiveControlsFrameView> Frames = new();
    }

    public sealed class CanonicalRuntimeUnityPlayerLoopInteractiveControlsFrameView
    {
        public int FrameIndex;
        public string FrameCategory = string.Empty;
        public string Title = string.Empty;
        public string PlayerFacingSummary = string.Empty;
        public string CanonicalStateHash = string.Empty;
        public string Hud = string.Empty;
    }

    public static class CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness
    {
        public const string PassMarker =
            "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_PASS";
        public const string FailMarker =
            "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_FAIL";

        private static readonly string[] RequiredControls =
        {
            "load_model",
            "first",
            "previous",
            "next",
            "last",
            "autoplay_tick",
            "autoplay_all",
            "copy_current_frame_summary",
            "show_runtime_hash",
            "show_hud_lines"
        };

        private static readonly string[] RequiredScriptActions =
        {
            "load_model",
            "assert_frame_count",
            "first",
            "next",
            "next",
            "previous",
            "last",
            "first",
            "autoplay_tick",
            "autoplay_tick",
            "autoplay_all",
            "copy_current_frame_summary",
            "assert_final_frame_reachable",
            "assert_runtime_authority_markers"
        };

        public static CanonicalRuntimeUnityPlayerLoopInteractiveControlsSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var modelPath = ReadArgument(
                args,
                "-llmgcRuntimeBackedUnityPlayerLoopInteractiveControlsModelPath");
            var controlScriptPath = ReadArgument(
                args,
                "-llmgcRuntimeBackedUnityPlayerLoopInteractiveControlsScriptPath");
            return Consume(modelPath, controlScriptPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeRuntimeBackedUnityPlayerLoopInteractiveControlsSmoke()
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

        public static CanonicalRuntimeUnityPlayerLoopInteractiveControlsSmokeResult Consume(
            string modelPath,
            string controlScriptPath)
        {
            var result = new CanonicalRuntimeUnityPlayerLoopInteractiveControlsSmokeResult
            {
                ModelPath = modelPath ?? string.Empty,
                ControlScriptPath = controlScriptPath ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                result.Diagnostics = "interactive controls model path is missing";
                return result;
            }

            if (string.IsNullOrWhiteSpace(controlScriptPath) || !File.Exists(controlScriptPath))
            {
                result.Diagnostics = "interactive controls script path is missing";
                return result;
            }

            var model = File.ReadAllText(modelPath);
            var script = File.ReadAllText(controlScriptPath);
            var frameCount = CountJsonProperty(model, "frameIndex");
            var frameCountPassed = frameCount >= 13
                                   || ContainsJsonNumber(model, "frameCount", 13);
            var requiredControlsPresent = true;
            foreach (var control in RequiredControls)
            {
                requiredControlsPresent = requiredControlsPresent
                                          && ContainsJsonPair(model, "id", control);
            }

            var controlScriptPassed = ContainsJsonNumber(script, "expectedFrameCount", 13);
            foreach (var action in RequiredScriptActions)
            {
                controlScriptPassed = controlScriptPassed
                                      && ContainsJsonPair(script, "action", action);
            }

            var runtimeAuthorityMarkersPresent =
                ContainsJsonBool(model, "runtimeAuthority", true)
                && ContainsJsonBool(model, "unityGameplayTruth", false)
                && ContainsJsonBool(model, "projectionOnly", false)
                && model.Contains("\"gameplayTruth\": \"Runtime\"", StringComparison.Ordinal)
                && model.Contains(
                    "\"unityMode\": \"PlayerAdapter/HUD controls only\"",
                    StringComparison.Ordinal)
                && model.Contains("\"canonicalStateHash\"", StringComparison.Ordinal)
                && model.Contains("\"hudLines\"", StringComparison.Ordinal);
            result.Passed = frameCountPassed
                            && requiredControlsPresent
                            && controlScriptPassed
                            && runtimeAuthorityMarkersPresent;
            result.Diagnostics =
                "frameCount="
                + frameCount
                + "; frameCountPassed="
                + frameCountPassed
                + "; requiredControlsPresent="
                + requiredControlsPresent
                + "; controlScriptPassed="
                + controlScriptPassed
                + "; runtimeAuthorityMarkersPresent="
                + runtimeAuthorityMarkersPresent
                + "; modelPath="
                + modelPath
                + "; controlScriptPath="
                + controlScriptPath;
            return result;
        }

        public static CanonicalRuntimeUnityPlayerLoopInteractiveControlsModelView LoadModelView(
            string modelPath)
        {
            var view = new CanonicalRuntimeUnityPlayerLoopInteractiveControlsModelView();
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                return view;
            }

            var json = File.ReadAllText(modelPath);
            view.CandidateId = ExtractString(json, "candidateId");
            view.FrameCount = ExtractInt(json, "frameCount");
            view.CurrentFrameIndex = ExtractInt(json, "currentFrameIndex");
            ExtractControls(json, view.Controls);
            var index = 0;
            while (index >= 0)
            {
                index = json.IndexOf("\"frameIndex\"", index, StringComparison.Ordinal);
                if (index < 0)
                {
                    break;
                }

                var start = json.LastIndexOf('{', index);
                var end = json.IndexOf('}', index);
                if (start >= 0 && end > start)
                {
                    var block = json.Substring(start, end - start + 1);
                    view.Frames.Add(new CanonicalRuntimeUnityPlayerLoopInteractiveControlsFrameView
                    {
                        FrameIndex = ExtractInt(block, "frameIndex"),
                        FrameCategory = ExtractString(block, "frameCategory"),
                        Title = ExtractString(block, "title"),
                        PlayerFacingSummary = ExtractString(block, "playerFacingSummary"),
                        CanonicalStateHash = ExtractString(block, "canonicalStateHash"),
                        Hud = ExtractHud(block)
                    });
                }

                index += "\"frameIndex\"".Length;
            }

            return view;
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
            return json.Contains("\"" + name + "\": \"" + value + "\"", StringComparison.Ordinal)
                   || json.Contains("\"" + name + "\":\"" + value + "\"", StringComparison.Ordinal);
        }

        private static bool ContainsJsonBool(string json, string name, bool value)
        {
            var text = value ? "true" : "false";
            return json.Contains("\"" + name + "\": " + text, StringComparison.Ordinal)
                   || json.Contains("\"" + name + "\":" + text, StringComparison.Ordinal);
        }

        private static bool ContainsJsonNumber(string json, string name, int value)
        {
            var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return json.Contains("\"" + name + "\": " + text, StringComparison.Ordinal)
                   || json.Contains("\"" + name + "\":" + text, StringComparison.Ordinal);
        }

        private static void ExtractControls(string json, List<string> controls)
        {
            foreach (var control in RequiredControls)
            {
                if (ContainsJsonPair(json, "id", control))
                {
                    controls.Add(control);
                }
            }
        }

        private static string ExtractString(string json, string name)
        {
            var needle = "\"" + name + "\"";
            var index = json.IndexOf(needle, StringComparison.Ordinal);
            if (index < 0)
            {
                return string.Empty;
            }

            var colon = json.IndexOf(':', index);
            var firstQuote = json.IndexOf('"', colon + 1);
            var secondQuote = firstQuote < 0 ? -1 : json.IndexOf('"', firstQuote + 1);
            return firstQuote >= 0 && secondQuote > firstQuote
                ? json.Substring(firstQuote + 1, secondQuote - firstQuote - 1)
                : string.Empty;
        }

        private static int ExtractInt(string json, string name)
        {
            var needle = "\"" + name + "\"";
            var index = json.IndexOf(needle, StringComparison.Ordinal);
            if (index < 0)
            {
                return 0;
            }

            var colon = json.IndexOf(':', index);
            if (colon < 0)
            {
                return 0;
            }

            var end = colon + 1;
            while (end < json.Length && char.IsWhiteSpace(json[end]))
            {
                end++;
            }

            var start = end;
            while (end < json.Length && char.IsDigit(json[end]))
            {
                end++;
            }

            return int.TryParse(
                json.Substring(start, end - start),
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var value)
                ? value
                : 0;
        }

        private static string ExtractHud(string json)
        {
            var index = json.IndexOf("\"hudLines\"", StringComparison.Ordinal);
            if (index < 0)
            {
                return string.Empty;
            }

            var start = json.IndexOf('[', index);
            var end = json.IndexOf(']', index);
            if (start < 0 || end <= start)
            {
                return string.Empty;
            }

            return json.Substring(start + 1, end - start - 1)
                .Replace("\"", string.Empty, StringComparison.Ordinal)
                .Replace(",", "\n", StringComparison.Ordinal)
                .Trim();
        }
    }
}
