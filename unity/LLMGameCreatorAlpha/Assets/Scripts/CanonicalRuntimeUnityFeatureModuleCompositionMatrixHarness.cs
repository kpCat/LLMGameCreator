using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityFeatureModuleCompositionMatrixView
    {
        public string Status = string.Empty;
        public string SelectedComposition = string.Empty;
        public int CompositionCount;
        public int PassedCompositionCount;
        public int SelectedModuleCount;
        public int SelectedCombinedEffectCount;
        public string CatalogText = string.Empty;
        public string MatrixText = string.Empty;
    }

    public static class CanonicalRuntimeUnityFeatureModuleCompositionMatrixHarness
    {
        public const string PassMarker = "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_PASS";
        public const string FailMarker = "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_FAIL";

        public static void RunBatchmodeFeatureModuleCompositionMatrixSmoke()
        {
            var exitCode = 0;
            try
            {
                var root = ReadArgument(Environment.GetCommandLineArgs(), "-llmgcGoal146ArtifactRoot");
                var diagnostics = Validate(root);
                var passed = diagnostics.Contains("compositionCount=8")
                             && diagnostics.Contains("passedCompositionCount=8")
                             && diagnostics.Contains("distinctPackageSha256Count=8")
                             && diagnostics.Contains("distinctFinalStateHashCount=8")
                             && diagnostics.Contains("multiModuleCompositionCount=4")
                             && diagnostics.Contains("selectedCompositionExists=True")
                             && diagnostics.Contains("selectedModuleEffectCountMatches=True")
                             && diagnostics.Contains("selectedPackageDistinctFromGoal142Candidates=True")
                             && diagnostics.Contains("allOrderIndependenceProofsPassed=True")
                             && diagnostics.Contains("allCheckpointReloadsPassed=True")
                             && diagnostics.Contains("allFullReplaysEquivalent=True")
                             && diagnostics.Contains("allActionBindingsPassed=True")
                             && diagnostics.Contains("runtimeAuthority=True")
                             && diagnostics.Contains("unityGameplayTruth=False");
#if UNITY_EDITOR
                if (passed) Debug.Log(PassMarker + "\n" + diagnostics);
                else { exitCode = 1; Debug.LogError(FailMarker + "\n" + diagnostics); }
#endif
            }
            catch (Exception ex)
            {
                exitCode = 1;
#if UNITY_EDITOR
                Debug.LogError(FailMarker + "\n" + ex);
#endif
            }
#if UNITY_EDITOR
            finally
            {
                if (Application.isBatchMode) EditorApplication.Exit(exitCode);
            }
#endif
        }

        public static string Validate(string root)
        {
            var matrix = File.ReadAllText(Path.Combine(root, "featuremodule-composition-matrix-result.json"));
            var dashboard = File.ReadAllText(Path.Combine(root, "featuremodule-composition-dashboard.json"));
            var selection = File.ReadAllText(Path.Combine(root, "featuremodule-composition-selection-handoff.json"));
            var selectedId = ExtractString(selection, "compositionId");
            var selectedModuleCount = ExtractStringArrayCount(selection, "selectedOptionalModuleIds");
            var selectedCombinedEffectCount = ExtractInt(dashboard, "selectedCombinedEffectCount");
            return "compositionCount=" + ExtractInt(matrix, "compositionCount")
                   + "; passedCompositionCount=" + ExtractInt(matrix, "passedCompositionCount")
                   + "; distinctPackageSha256Count=" + ExtractInt(matrix, "distinctPackageSha256Count")
                   + "; distinctFinalStateHashCount=" + ExtractInt(matrix, "distinctFinalStateHashCount")
                   + "; multiModuleCompositionCount=" + ExtractInt(matrix, "multiModuleCompositionCount")
                   + "; selectedCompositionExists=" + (selectedId.Length > 0 && matrix.Contains("\"compositionId\": \"" + selectedId + "\""))
                   + "; selectedCompositionModuleCount=" + selectedModuleCount
                   + "; selectedPackageDistinctFromGoal142Candidates=" + Bool(selection, "packageDistinctFromGoal142Candidates", true)
                   + "; selectedCombinedEffectCount=" + selectedCombinedEffectCount
                   + "; selectedModuleEffectCountMatches=" + (selectedModuleCount == selectedCombinedEffectCount)
                   + "; allOrderIndependenceProofsPassed=" + Bool(matrix, "allOrderIndependenceProofsPassed", true)
                   + "; allCheckpointReloadsPassed=" + Bool(matrix, "allCheckpointReloadsPassed", true)
                   + "; allFullReplaysEquivalent=" + Bool(matrix, "allFullReplaysEquivalent", true)
                   + "; allActionBindingsPassed=" + Bool(matrix, "allActionBindingsPassed", true)
                   + "; runtimeAuthority=" + Bool(dashboard, "runtimeAuthority", true)
                   + "; unityGameplayTruth=" + Bool(dashboard, "unityGameplayTruth", true);
        }

        public static CanonicalRuntimeUnityFeatureModuleCompositionMatrixView LoadView(string root)
        {
            var matrix = Read(Path.Combine(root, "featuremodule-composition-matrix-result.json"));
            var dashboard = Read(Path.Combine(root, "featuremodule-composition-dashboard.json"));
            var selection = Read(Path.Combine(root, "featuremodule-composition-selection-handoff.json"));
            return new CanonicalRuntimeUnityFeatureModuleCompositionMatrixView
            {
                Status = ExtractString(dashboard, "status"),
                SelectedComposition = ExtractString(selection, "compositionId"),
                CompositionCount = ExtractInt(matrix, "compositionCount"),
                PassedCompositionCount = ExtractInt(matrix, "passedCompositionCount"),
                SelectedModuleCount = ExtractStringArrayCount(selection, "selectedOptionalModuleIds"),
                SelectedCombinedEffectCount = ExtractInt(dashboard, "selectedCombinedEffectCount"),
                CatalogText = Read(Path.Combine(root, "featuremodule-catalog.json")),
                MatrixText = matrix
            };
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var index = 0; index < args.Length - 1; index++) if (args[index] == name) return args[index + 1];
            return string.Empty;
        }

        private static string Read(string path) => File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        private static bool Bool(string text, string property, bool value) =>
            text.Contains("\"" + property + "\": " + value.ToString().ToLowerInvariant());

        private static string ExtractString(string text, string property)
        {
            var marker = "\"" + property + "\": \"";
            var start = text.IndexOf(marker, StringComparison.Ordinal);
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

        private static int ExtractStringArrayCount(string text, string property)
        {
            var marker = "\"" + property + "\":";
            var start = text.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return 0;
            start = text.IndexOf('[', start);
            var end = text.IndexOf(']', start);
            if (start < 0 || end < 0) return 0;
            var count = 0;
            for (var index = start; index < end; index++) if (text[index] == '"') count++;
            return count / 2;
        }
    }
}
