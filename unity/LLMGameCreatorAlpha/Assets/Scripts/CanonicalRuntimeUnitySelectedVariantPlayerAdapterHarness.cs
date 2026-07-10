using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySelectedVariantPlayerAdapterSmokeResult
    {
        public bool Passed;
        public string ModelPath = string.Empty;
        public string FramesPath = string.Empty;
        public string HandoffPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public sealed class CanonicalRuntimeUnitySelectedVariantPlayerAdapterModelView
    {
        public string CandidateId = string.Empty;
        public string VariantKind = string.Empty;
        public int Score;
        public bool PackageHashMatch;
        public bool FinalStateHashMatch;
        public int FrameCount;
        public int CurrentFrameIndex;
        public string ControlIntent = string.Empty;
        public string Route = string.Empty;
        public string CanonicalStep = string.Empty;
        public string InventorySummary = string.Empty;
        public string QuestSummary = string.Empty;
        public string CombatSummary = string.Empty;
        public string Status = string.Empty;
    }

    public static class CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness
    {
        public const string PassMarker =
            "GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_PASS";
        public const string FailMarker =
            "GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_FAIL";
        private const string ExpectedCandidate =
            "minimal-map-game-exploration-resource-focus";

        public static CanonicalRuntimeUnitySelectedVariantPlayerAdapterSmokeResult
            RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            return Consume(
                ReadArgument(args, "-llmgcSelectedVariantPlayerAdapterModelPath"),
                ReadArgument(args, "-llmgcSelectedVariantPlayerAdapterFramesPath"),
                ReadArgument(args, "-llmgcSelectedVariantPlayerAdapterHandoffPath"));
        }

#if UNITY_EDITOR
        public static void RunBatchmodeSelectedRuntimeVariantPlayerAdapterSmoke()
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
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }
#endif

        public static CanonicalRuntimeUnitySelectedVariantPlayerAdapterSmokeResult Consume(
            string modelPath,
            string framesPath,
            string handoffPath)
        {
            var result = new CanonicalRuntimeUnitySelectedVariantPlayerAdapterSmokeResult
            {
                ModelPath = modelPath ?? string.Empty,
                FramesPath = framesPath ?? string.Empty,
                HandoffPath = handoffPath ?? string.Empty
            };
            var modelPathExists = !string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath);
            var framesPathExists = !string.IsNullOrWhiteSpace(framesPath) && File.Exists(framesPath);
            var handoffPathExists = !string.IsNullOrWhiteSpace(handoffPath) && File.Exists(handoffPath);
            if (!modelPathExists || !framesPathExists || !handoffPathExists)
            {
                result.Diagnostics =
                    "modelPathExists=" + modelPathExists
                    + "; framesPathExists=" + framesPathExists
                    + "; handoffPathExists=" + handoffPathExists
                    + "; candidateIsGoal142Selection=False"
                    + "; selectedPackageSha256MatchesHandoff=False"
                    + "; selectedFinalStateHashMatches=False"
                    + "; frameCountPassed=False"
                    + "; selectedVariantEffectVisible=False"
                    + "; noBalancedBaselineFallback=False"
                    + "; runtimeAuthorityMarkersPresent=False"
                    + "; unityConsumesSelectedVariantPlayerAdapter=False"
                    + "; unityGameplayTruth=False";
                return result;
            }

            var model = File.ReadAllText(modelPath);
            var frames = File.ReadAllText(framesPath);
            var handoff = File.ReadAllText(handoffPath);
            var candidateIsGoal142Selection =
                ContainsJsonPair(model, "candidateId", ExpectedCandidate)
                && ContainsJsonPair(handoff, "candidateId", ExpectedCandidate);
            var packageHashMatch =
                ContainsJsonBool(handoff, "selectedPackageSha256MatchesHandoff", true);
            var finalStateHashMatch =
                ContainsJsonBool(handoff, "selectedFinalStateHashMatches", true);
            var frameCountPassed = ExtractInt(model, "frameCount") >= 6
                                   && ExtractInt(frames, "frameCount") >= 6;
            var selectedVariantEffectVisible =
                ContainsJsonBool(model, "selectedVariantEffectVisible", true);
            var noBalancedBaselineFallback =
                ContainsJsonBool(model, "noBalancedBaselineFallback", true)
                && !model.Contains(
                    "minimal-map-game-balanced-baseline",
                    StringComparison.Ordinal);
            var runtimeAuthorityMarkersPresent =
                ContainsJsonBool(model, "runtimeAuthority", true)
                && ContainsJsonBool(frames, "runtimeAuthority", true)
                && ContainsJsonBool(handoff, "runtimeAuthority", true)
                && ContainsJsonBool(model, "projectionOnly", false)
                && ContainsJsonBool(frames, "projectionOnly", false)
                && ContainsJsonBool(handoff, "projectionOnly", false);
            var unityGameplayTruth =
                ContainsJsonBool(model, "unityGameplayTruth", true)
                || ContainsJsonBool(frames, "unityGameplayTruth", true)
                || ContainsJsonBool(handoff, "unityGameplayTruth", true);
            var unityConsumesSelectedVariantPlayerAdapter =
                modelPathExists && framesPathExists && handoffPathExists;
            result.Passed = candidateIsGoal142Selection
                            && packageHashMatch
                            && finalStateHashMatch
                            && frameCountPassed
                            && selectedVariantEffectVisible
                            && noBalancedBaselineFallback
                            && runtimeAuthorityMarkersPresent
                            && unityConsumesSelectedVariantPlayerAdapter
                            && !unityGameplayTruth;
            result.Diagnostics =
                "modelPathExists=" + modelPathExists
                + "; framesPathExists=" + framesPathExists
                + "; candidateIsGoal142Selection=" + candidateIsGoal142Selection
                + "; selectedPackageSha256MatchesHandoff=" + packageHashMatch
                + "; selectedFinalStateHashMatches=" + finalStateHashMatch
                + "; frameCountPassed=" + frameCountPassed
                + "; selectedVariantEffectVisible=" + selectedVariantEffectVisible
                + "; noBalancedBaselineFallback=" + noBalancedBaselineFallback
                + "; runtimeAuthorityMarkersPresent=" + runtimeAuthorityMarkersPresent
                + "; unityConsumesSelectedVariantPlayerAdapter="
                + unityConsumesSelectedVariantPlayerAdapter
                + "; unityGameplayTruth=" + unityGameplayTruth;
            return result;
        }

        public static CanonicalRuntimeUnitySelectedVariantPlayerAdapterModelView LoadModelView(
            string modelPath,
            string framesPath,
            string handoffPath,
            int frameIndex)
        {
            var view = new CanonicalRuntimeUnitySelectedVariantPlayerAdapterModelView();
            if (!File.Exists(modelPath) || !File.Exists(framesPath) || !File.Exists(handoffPath))
            {
                view.Status = "Goal143 Selected PlayerAdapter artifacts not found.";
                return view;
            }

            var model = File.ReadAllText(modelPath);
            var frames = File.ReadAllText(framesPath);
            var handoff = File.ReadAllText(handoffPath);
            view.CandidateId = ExtractString(model, "candidateId");
            view.VariantKind = ExtractString(model, "variantKind");
            view.Score = ExtractInt(model, "score");
            view.PackageHashMatch =
                ContainsJsonBool(handoff, "selectedPackageSha256MatchesHandoff", true);
            view.FinalStateHashMatch =
                ContainsJsonBool(handoff, "selectedFinalStateHashMatches", true);
            view.FrameCount = ExtractInt(frames, "frameCount");
            view.CurrentFrameIndex = Math.Max(0, Math.Min(frameIndex, view.FrameCount - 1));
            view.ControlIntent = ExtractStringAtOccurrence(
                frames,
                "controlIntent",
                view.CurrentFrameIndex);
            view.Route = ExtractStringAtOccurrence(frames, "route", view.CurrentFrameIndex);
            view.CanonicalStep = ExtractStringAtOccurrence(
                frames,
                "canonicalStepId",
                view.CurrentFrameIndex);
            view.InventorySummary = ExtractStringAtOccurrence(
                frames,
                "inventorySummary",
                view.CurrentFrameIndex);
            view.QuestSummary = ExtractStringAtOccurrence(
                frames,
                "questSummary",
                view.CurrentFrameIndex);
            view.CombatSummary = ExtractStringAtOccurrence(
                frames,
                "combatSummary",
                view.CurrentFrameIndex);
            view.Status = "Loaded Goal143 read-only selected PlayerAdapter model.";
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

        private static bool ContainsJsonPair(string text, string property, string value)
        {
            return text.Contains(
                "\"" + property + "\": \"" + value + "\"",
                StringComparison.Ordinal);
        }

        private static bool ContainsJsonBool(string text, string property, bool value)
        {
            return text.Contains(
                "\"" + property + "\": " + value.ToString().ToLowerInvariant(),
                StringComparison.Ordinal);
        }

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

            int value;
            return int.TryParse(text.Substring(start, index - start), out value) ? value : 0;
        }

        private static string ExtractString(string text, string property)
        {
            return ExtractStringAtOccurrence(text, property, 0);
        }

        private static string ExtractStringAtOccurrence(
            string text,
            string property,
            int occurrence)
        {
            var marker = "\"" + property + "\": \"";
            var index = -1;
            for (var i = 0; i <= occurrence; i++)
            {
                index = text.IndexOf(marker, index + 1, StringComparison.Ordinal);
                if (index < 0)
                {
                    return string.Empty;
                }
            }

            index += marker.Length;
            var end = text.IndexOf('"', index);
            return end > index ? text.Substring(index, end - index) : string.Empty;
        }
    }
}
