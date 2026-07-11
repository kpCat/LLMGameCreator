using System;
using System.IO;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeUnitySavedFeatureModuleCompositionView
    {
        public string Status = string.Empty;
        public string CompositionId = string.Empty;
        public int Revision;
        public string CatalogFingerprint = string.Empty;
        public string PackageSha256 = string.Empty;
        public string FinalStateHash = string.Empty;
        public string CompositionText = string.Empty;
        public string ParameterText = string.Empty;
        public string RuntimeEffectsText = string.Empty;
    }

    public static class CanonicalRuntimeUnitySavedFeatureModuleCompositionHarness
    {
        public const string PassMarker = "GOAL147_SAVED_FEATUREMODULE_COMPOSITION_PASS";
        public const string FailMarker = "GOAL147_SAVED_FEATUREMODULE_COMPOSITION_FAIL";

        public static void RunBatchmodeSavedFeatureModuleCompositionSmoke()
        {
            var exitCode = 0;
            try
            {
                var root = ReadArgument(Environment.GetCommandLineArgs(), "-llmgcGoal147ArtifactRoot");
                var diagnostics = Validate(root);
                var passed = diagnostics.Contains("savedCompositionLoaded=True")
                             && diagnostics.Contains("catalogFingerprintMatches=True")
                             && diagnostics.Contains("selectedModuleFingerprintsMatch=True")
                             && diagnostics.Contains("parameterValuesLoaded=True")
                             && diagnostics.Contains("packageShaMatches=True")
                             && diagnostics.Contains("runtimeQualificationPassed=True")
                             && diagnostics.Contains("checkpointReloadPassed=True")
                             && diagnostics.Contains("fullReplayEquivalent=True")
                             && diagnostics.Contains("actionBindingPassed=True")
                             && diagnostics.Contains("runtimeAuthority=True")
                             && diagnostics.Contains("unityGameplayTruth=False");
#if UNITY_EDITOR
                if (passed) Debug.Log(PassMarker + "\n" + diagnostics);
                else { exitCode = 1; Debug.LogError(FailMarker + "\n" + diagnostics); }
#endif
            }
            catch (Exception exception)
            {
                exitCode = 1;
#if UNITY_EDITOR
                Debug.LogError(FailMarker + "\n" + exception);
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
            var composition = Read(Path.Combine(root, "selected-composition", "composition.json"));
            var parameters = Read(Path.Combine(root, "selected-composition", "effective-parameter-values.json"));
            var proof = Read(Path.Combine(root, "parameterized-composition-materialization-proof.json"));
            var library = Read(Path.Combine(root, "featuremodule-library-index.json"));
            var packagePath = Path.Combine(root, "selected-composition", "package.json");
            var packageSha = File.Exists(packagePath) ? Hash(packagePath) : string.Empty;
            var expectedSha = ExtractString(proof, "packageSha256");
            var catalogFingerprint = ExtractString(composition, "catalogFingerprint");
            var selectedFingerprints = ExtractObjectValues(composition, "moduleFingerprints");
            return "savedCompositionLoaded=" + (ExtractString(composition, "compositionId").Length > 0)
                   + "; catalogFingerprintMatches=" + (catalogFingerprint.Length == 64 && library.Contains(catalogFingerprint))
                   + "; selectedModuleFingerprintsMatch=" + (selectedFingerprints > 0 && selectedFingerprints == ExtractArrayCount(composition, "selectedModuleIds") && EveryObjectValueOccurs(composition, "moduleFingerprints", library))
                   + "; parameterValuesLoaded=" + (CountOccurrences(parameters, "\"parameterId\":") >= 8)
                   + "; packageShaMatches=" + (packageSha.Length == 64 && packageSha == expectedSha)
                   + "; runtimeQualificationPassed=" + Bool(proof, "passed", true)
                   + "; checkpointReloadPassed=" + Bool(proof, "checkpointReloadPassed", true)
                   + "; fullReplayEquivalent=" + Bool(proof, "fullReplayEquivalent", true)
                   + "; actionBindingPassed=" + Bool(proof, "actionBindingPassed", true)
                   + "; runtimeAuthority=True; unityGameplayTruth=False";
        }

        public static CanonicalRuntimeUnitySavedFeatureModuleCompositionView LoadView(string root)
        {
            var composition = Read(Path.Combine(root, "selected-composition", "composition.json"));
            var parameters = Read(Path.Combine(root, "selected-composition", "effective-parameter-values.json"));
            var effects = Read(Path.Combine(root, "selected-composition", "runtime-effect-observations.json"));
            var dashboard = Read(Path.Combine(root, "featuremodule-authoring-dashboard.json"));
            return new CanonicalRuntimeUnitySavedFeatureModuleCompositionView
            {
                Status = ExtractString(dashboard, "status"),
                CompositionId = ExtractString(composition, "compositionId"),
                Revision = ExtractInt(composition, "revision"),
                CatalogFingerprint = ExtractString(composition, "catalogFingerprint"),
                PackageSha256 = ExtractString(composition, "lastMaterializedPackageSha256"),
                FinalStateHash = ExtractString(composition, "lastQualifiedFinalStateHash"),
                CompositionText = composition,
                ParameterText = parameters,
                RuntimeEffectsText = effects
            };
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var index = 0; index < args.Length - 1; index++) if (args[index] == name) return args[index + 1];
            return string.Empty;
        }

        private static string Read(string path) => File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        private static bool Bool(string text, string property, bool value) => text.Contains("\"" + property + "\": " + value.ToString().ToLowerInvariant());

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

        private static int ExtractArrayCount(string text, string property = "")
        {
            var start = property.Length == 0 ? text.IndexOf('[') : text.IndexOf('[', text.IndexOf("\"" + property + "\"", StringComparison.Ordinal));
            var end = start < 0 ? -1 : text.IndexOf(']', start);
            if (start < 0 || end < 0) return 0;
            var count = 0;
            for (var index = start; index < end; index++) if (text[index] == '{') count++;
            if (count > 0) return count;
            for (var index = start; index < end; index++) if (text[index] == '"') count++;
            return count / 2;
        }

        private static int ExtractObjectValues(string text, string property)
        {
            var start = text.IndexOf('{', text.IndexOf("\"" + property + "\"", StringComparison.Ordinal));
            var end = start < 0 ? -1 : text.IndexOf('}', start);
            if (start < 0 || end < 0) return 0;
            var count = 0;
            for (var index = start; index < end; index++) if (text[index] == ':') count++;
            return count;
        }

        private static int CountOccurrences(string text, string marker)
        {
            var count = 0;
            var cursor = 0;
            while ((cursor = text.IndexOf(marker, cursor, StringComparison.Ordinal)) >= 0)
            {
                count++;
                cursor += marker.Length;
            }
            return count;
        }

        private static bool EveryObjectValueOccurs(string text, string property, string target)
        {
            var start = text.IndexOf('{', text.IndexOf("\"" + property + "\"", StringComparison.Ordinal));
            var end = start < 0 ? -1 : text.IndexOf('}', start);
            if (start < 0 || end < 0) return false;
            var block = text.Substring(start, end - start + 1);
            var cursor = 0;
            while ((cursor = block.IndexOf(": \"", cursor, StringComparison.Ordinal)) >= 0)
            {
                cursor += 3;
                var valueEnd = block.IndexOf('"', cursor);
                if (valueEnd < 0 || !target.Contains(block.Substring(cursor, valueEnd - cursor))) return false;
                cursor = valueEnd + 1;
            }
            return true;
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
