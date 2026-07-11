using System;
using System.IO;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnityProductLineInteractiveSessionMatrixView
    {
        public string Status = string.Empty;
        public string SelectedCandidate = string.Empty;
        public string SelectedVariant = string.Empty;
        public int CandidateCount;
        public int PassedCandidateCount;
        public int DistinctFinalStateHashCount;
        public bool AllFocusEffectsObserved;
        public bool AllCheckpointReloadsPassed;
        public bool AllFullReplaysEquivalent;
        public bool AllActionBindingsPassed;
        public string MatrixText = string.Empty;
    }

    public static class CanonicalRuntimeUnityProductLineInteractiveSessionMatrixHarness
    {
        public const string PassMarker = "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_PASS";
        public const string FailMarker = "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_FAIL";

        public static void RunBatchmodeProductLineInteractiveSessionMatrixSmoke()
        {
            var exit = 0;
            try
            {
                var root = ReadArgument(Environment.GetCommandLineArgs(), "-llmgcGoal145ArtifactRoot");
                var diagnostics = Validate(root);
                var passed = diagnostics.StartsWith("candidateCount=4", StringComparison.Ordinal)
                             && diagnostics.Contains("passedCandidateCount=4")
                             && diagnostics.Contains("distinctFinalStateHashCount=4")
                             && diagnostics.Contains("selectedCandidateExists=True")
                             && diagnostics.Contains("selectedCandidatePackageHashMatches=True")
                             && diagnostics.Contains("allCandidateCheckpointReloadsPassed=True")
                             && diagnostics.Contains("allCandidateFullReplaysEquivalent=True")
                             && diagnostics.Contains("allCandidateActionBindingsPassed=True")
                             && diagnostics.Contains("allFocusEffectsObserved=True")
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
            var matrixPath = Path.Combine(root, "product-line-interactive-session-matrix-result.json");
            var dashboardPath = Path.Combine(root, "product-line-interactive-session-dashboard.json");
            var comparisonPath = Path.Combine(root, "product-line-interactive-session-comparison.json");
            var selectionPath = Path.Combine(root, "product-line-interactive-session-selection-handoff.json");
            if (!File.Exists(matrixPath) || !File.Exists(dashboardPath)
                || !File.Exists(comparisonPath) || !File.Exists(selectionPath))
                return "candidateCount=0; artifactsExist=False";
            var matrix = File.ReadAllText(matrixPath);
            var dashboard = File.ReadAllText(dashboardPath);
            var comparison = File.ReadAllText(comparisonPath);
            var selection = File.ReadAllText(selectionPath);
            var selectedId = ExtractString(selection, "selectedCandidateId");
            var selectedPath = ExtractString(selection, "selectedPackagePath");
            var selectedHash = ExtractString(selection, "selectedPackageSha256");
            var repoRoot = RootFromArtifact(root);
            var packagePath = Path.GetFullPath(Path.Combine(repoRoot, selectedPath.Replace('/', Path.DirectorySeparatorChar)));
            var selectedExists = selectedId.Length > 0
                                 && matrix.Contains("\"candidateId\": \"" + selectedId + "\"", StringComparison.Ordinal)
                                 && File.Exists(packagePath);
            var hashMatches = selectedExists && HashFile(packagePath) == selectedHash;
            return "candidateCount=" + ExtractInt(matrix, "candidateCount")
                   + "; passedCandidateCount=" + ExtractInt(matrix, "passedCandidateCount")
                   + "; distinctFinalStateHashCount=" + ExtractInt(matrix, "distinctFinalStateHashCount")
                   + "; selectedCandidateExists=" + selectedExists
                   + "; selectedCandidatePackageHashMatches=" + hashMatches
                   + "; allCandidateCheckpointReloadsPassed=" + Bool(matrix, "allCandidateCheckpointReloadsPassed", true)
                   + "; allCandidateFullReplaysEquivalent=" + Bool(matrix, "allCandidateFullReplaysEquivalent", true)
                   + "; allCandidateActionBindingsPassed=" + Bool(matrix, "allCandidateActionBindingsPassed", true)
                   + "; allFocusEffectsObserved=" + (Bool(matrix, "allFocusEffectsObserved", true)
                                                       && Bool(comparison, "allFocusEffectsObserved", true))
                   + "; runtimeAuthority=" + Bool(dashboard, "runtimeAuthority", true)
                   + "; unityGameplayTruth=" + Bool(dashboard, "unityGameplayTruth", true);
        }

        public static CanonicalRuntimeUnityProductLineInteractiveSessionMatrixView LoadView(string root)
        {
            var matrix = Read(Path.Combine(root, "product-line-interactive-session-matrix-result.json"));
            var dashboard = Read(Path.Combine(root, "product-line-interactive-session-dashboard.json"));
            var selection = Read(Path.Combine(root, "product-line-interactive-session-selection-handoff.json"));
            return new CanonicalRuntimeUnityProductLineInteractiveSessionMatrixView
            {
                Status = ExtractString(dashboard, "status"),
                SelectedCandidate = ExtractString(selection, "selectedCandidateId"),
                SelectedVariant = ExtractString(selection, "selectedVariantKind"),
                CandidateCount = ExtractInt(matrix, "candidateCount"),
                PassedCandidateCount = ExtractInt(matrix, "passedCandidateCount"),
                DistinctFinalStateHashCount = ExtractInt(matrix, "distinctFinalStateHashCount"),
                AllFocusEffectsObserved = Bool(matrix, "allFocusEffectsObserved", true),
                AllCheckpointReloadsPassed = Bool(matrix, "allCandidateCheckpointReloadsPassed", true),
                AllFullReplaysEquivalent = Bool(matrix, "allCandidateFullReplaysEquivalent", true),
                AllActionBindingsPassed = Bool(matrix, "allCandidateActionBindingsPassed", true),
                MatrixText = matrix
            };
        }

        private static string RootFromArtifact(string artifactRoot)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(artifactRoot));
            for (var index = 0; index < 3 && directory.Parent != null; index++) directory = directory.Parent;
            return directory.FullName;
        }

        private static string HashFile(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var index = 0; index < args.Length - 1; index++)
                if (args[index] == name) return args[index + 1];
            return string.Empty;
        }

        private static string Read(string path) => File.Exists(path) ? File.ReadAllText(path) : string.Empty;
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
