using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySelectedVariantLiveSessionView
    {
        public string Status = string.Empty;
        public string Candidate = string.Empty;
        public string Variant = string.Empty;
        public string SessionId = string.Empty;
        public string StateHash = string.Empty;
        public int ActionIndex;
        public int ActionCount;
        public string LastAction = string.Empty;
        public string MapSummary = string.Empty;
        public string InventorySummary = string.Empty;
        public string QuestSummary = string.Empty;
        public string CombatSummary = string.Empty;
        public bool CheckpointReloadPassed;
        public bool FullReplayEquivalent;
    }

    public static class CanonicalRuntimeUnitySelectedVariantLiveSessionHarness
    {
        public const string PassMarker = "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_PASS";
        public const string FailMarker = "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_FAIL";
        private const string Candidate = "minimal-map-game-exploration-resource-focus";

        public static void RunBatchmodeSelectedRuntimeVariantLiveSessionSmoke()
        {
            var exit = 0;
            try
            {
                var root = ReadArgument(Environment.GetCommandLineArgs(), "-llmgcGoal144ArtifactRoot");
                var diagnostics = Validate(root);
                var passed = diagnostics.StartsWith("sessionArtifactsExist=True", StringComparison.Ordinal)
                             && diagnostics.Contains("selectedCandidateMatches=True")
                             && diagnostics.Contains("packageHashMatches=True")
                             && diagnostics.Contains("checkpointReloadPassed=True")
                             && diagnostics.Contains("fullReplayEquivalent=True")
                             && diagnostics.Contains("finalHashMatchesGoal142=True")
                             && diagnostics.Contains("selectedVariantEffectVisible=True")
                             && diagnostics.Contains("noFallback=True")
                             && diagnostics.Contains("runtimeAuthority=True")
                             && diagnostics.Contains("unityGameplayTruth=False");
#if UNITY_EDITOR
                if (passed) Debug.Log(PassMarker + "\n" + diagnostics);
                else { exit = 1; Debug.LogError(FailMarker + "\n" + diagnostics); }
#endif
            }
            catch (Exception ex)
            {
                exit = 1;
#if UNITY_EDITOR
                Debug.LogError(FailMarker + "\n" + ex);
#endif
            }
#if UNITY_EDITOR
            finally
            {
                if (Application.isBatchMode) EditorApplication.Exit(exit);
            }
#endif
        }

        public static string Validate(string root)
        {
            var dashboardPath = Path.Combine(root, "selected-runtime-variant-live-session-dashboard.json");
            var statePath = Path.Combine(root, "selected-runtime-variant-live-session-state.json");
            var journalPath = Path.Combine(root, "selected-runtime-variant-live-session-journal.json");
            var checkpointPath = Path.Combine(root, "selected-runtime-variant-live-session-checkpoint.json");
            var reloadPath = Path.Combine(root, "selected-runtime-variant-live-session-checkpoint-reload-result.json");
            var replayPath = Path.Combine(root, "selected-runtime-variant-live-session-final-replay-result.json");
            var paths = new[] { dashboardPath, statePath, journalPath, checkpointPath, reloadPath, replayPath };
            var exist = Array.TrueForAll(paths, File.Exists);
            if (!exist) return "sessionArtifactsExist=False";
            var dashboard = File.ReadAllText(dashboardPath);
            var state = File.ReadAllText(statePath);
            var reload = File.ReadAllText(reloadPath);
            var replay = File.ReadAllText(replayPath);
            var selected = Pair(dashboard, "selectedCandidateId", Candidate);
            var package = Bool(dashboard, "selectedPackageSha256Matches", true);
            var checkpoint = Bool(dashboard, "checkpointReloadByReplayPassed", true)
                             && Bool(reload, "passed", true);
            var fullReplay = Bool(dashboard, "fullReplayEquivalent", true)
                             && Bool(replay, "passed", true);
            var finalHash = Bool(dashboard, "finalStateHashMatchesGoal142", true);
            var effect = Bool(dashboard, "selectedVariantEffectVisible", true);
            var noFallback = Bool(dashboard, "noBalancedBaselineFallback", true)
                             && Bool(dashboard, "noGoal131Fallback", true)
                             && !dashboard.Contains("minimal-map-game-balanced-baseline", StringComparison.Ordinal);
            var authority = Bool(dashboard, "runtimeAuthority", true);
            var unityTruth = Bool(dashboard, "unityGameplayTruth", true);
            return "sessionArtifactsExist=" + exist
                   + "; selectedCandidateMatches=" + selected
                   + "; packageHashMatches=" + package
                   + "; checkpointReloadPassed=" + checkpoint
                   + "; fullReplayEquivalent=" + fullReplay
                   + "; finalHashMatchesGoal142=" + finalHash
                   + "; selectedVariantEffectVisible=" + effect
                   + "; noFallback=" + noFallback
                   + "; runtimeAuthority=" + authority
                   + "; unityGameplayTruth=" + unityTruth
                   + "; statePresent=" + (state.Length > 0);
        }

        public static CanonicalRuntimeUnitySelectedVariantLiveSessionView LoadView(string root)
        {
            var dashboard = Read(Path.Combine(root, "selected-runtime-variant-live-session-dashboard.json"));
            var state = Read(Path.Combine(root, "selected-runtime-variant-live-session-state.json"));
            var journal = Read(Path.Combine(root, "selected-runtime-variant-live-session-journal.json"));
            return new CanonicalRuntimeUnitySelectedVariantLiveSessionView
            {
                Status = ExtractString(dashboard, "status"),
                Candidate = ExtractString(dashboard, "selectedCandidateId"),
                Variant = ExtractString(dashboard, "selectedVariantKind"),
                SessionId = ExtractString(state, "sessionId"),
                StateHash = ExtractString(state, "currentStateHash"),
                ActionIndex = ExtractInt(state, "currentActionIndex"),
                ActionCount = ExtractInt(journal, "actionCount"),
                LastAction = ExtractLastString(journal, "actionId"),
                MapSummary = ExtractString(state, "mapSummary"),
                InventorySummary = ExtractString(state, "inventorySummary"),
                QuestSummary = ExtractString(state, "questSummary"),
                CombatSummary = ExtractString(state, "combatSummary"),
                CheckpointReloadPassed = Bool(dashboard, "checkpointReloadByReplayPassed", true),
                FullReplayEquivalent = Bool(dashboard, "fullReplayEquivalent", true)
            };
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return string.Empty;
        }

        private static string Read(string path) => File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        private static bool Pair(string text, string property, string value) =>
            text.Contains("\"" + property + "\": \"" + value + "\"", StringComparison.Ordinal);
        private static bool Bool(string text, string property, bool value) =>
            text.Contains("\"" + property + "\": " + value.ToString().ToLowerInvariant(), StringComparison.Ordinal);

        private static string ExtractString(string text, string property)
        {
            var marker = "\"" + property + "\": \"";
            var start = text.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            start += marker.Length;
            var end = text.IndexOf('"', start);
            return end < 0 ? string.Empty : text.Substring(start, end - start);
        }

        private static string ExtractLastString(string text, string property)
        {
            var marker = "\"" + property + "\": \"";
            var start = text.LastIndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            start += marker.Length;
            var end = text.IndexOf('"', start);
            return end < 0 ? string.Empty : text.Substring(start, end - start);
        }

        private static int ExtractInt(string text, string property)
        {
            var marker = "\"" + property + "\":";
            var start = text.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return 0;
            start += marker.Length;
            while (start < text.Length && char.IsWhiteSpace(text[start])) start++;
            var end = start;
            while (end < text.Length && char.IsDigit(text[end])) end++;
            int value;
            return int.TryParse(text.Substring(start, end - start), out value) ? value : 0;
        }
    }
}
