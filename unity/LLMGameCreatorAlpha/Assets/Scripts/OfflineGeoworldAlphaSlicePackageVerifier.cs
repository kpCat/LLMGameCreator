using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaSlicePackageVerifier
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal109";
        private const string ManifestFileName = "offline-geoworld-alpha-export-manifest.json";
        private const string FileIndexFileName = "offline-geoworld-alpha-export-file-index.json";
        private const string ChecksumsFileName = "offline-geoworld-alpha-export-checksums.json";
        private const string RunbookFileName = "offline-geoworld-alpha-export-runbook.md";
        private const string AcceptanceGateFileName = "offline-geoworld-alpha-export-acceptance-gate.json";
        private const string ReadmeFileName = "offline-geoworld-alpha-export-readme.md";
        private const string FinalGate = "offline_geoworld_alpha_slice_export_package_verification";
        private static readonly string[] ForbiddenPackageMarkers =
        {
            "rawGeoJson",
            "rawGeoJsonPath",
            ".geojson",
            ".shp",
            ".png",
            ".jpg",
            ".jpeg",
            ".fbx",
            ".wav",
            "Unity" + "WebRequest",
            "Http" + "Client",
            "http" + "://",
            "https" + "://",
            "C:" + "\\",
            "C:" + "/",
            "\\\\"
        };

        private bool packageReady;
        private int packageFileCount;
        private int indexedFileCount;
        private int verifiedChecksumCount;
        private string statusLine = "Not verified";
        private string packageRoot = string.Empty;

        public bool PackageReady { get { return packageReady; } }
        public int PackageFileCount { get { return packageFileCount; } }
        public int IndexedFileCount { get { return indexedFileCount; } }
        public int VerifiedChecksumCount { get { return verifiedChecksumCount; } }
        public string StatusLine { get { return statusLine; } }
        public string PackageRoot { get { return packageRoot; } }

        public bool VerifyPackage()
        {
            return VerifyPackageRoot(Path.Combine(Application.streamingAssetsPath, RelativeRoot));
        }

        public bool VerifyPackageRoot(string root)
        {
            packageRoot = root ?? string.Empty;
            var diagnostics = new List<string>();
            var manifest = ReadFile(root, ManifestFileName, diagnostics);
            var index = ReadFile(root, FileIndexFileName, diagnostics);
            var checksums = ReadFile(root, ChecksumsFileName, diagnostics);
            var runbook = ReadFile(root, RunbookFileName, diagnostics);
            var acceptance = ReadFile(root, AcceptanceGateFileName, diagnostics);
            var readme = ReadFile(root, ReadmeFileName, diagnostics);

            packageFileCount = 6 - diagnostics.Count;
            var indexedFiles = IndexedFiles(index);
            var checksumMap = ChecksumMap(checksums);
            indexedFileCount = indexedFiles.Count;
            verifiedChecksumCount = 0;

            foreach (var relativePath in indexedFiles)
            {
                if (!IsSafeRelativePath(relativePath))
                {
                    diagnostics.Add("unsafe-index-path:" + relativePath);
                    continue;
                }

                var path = Path.Combine(root, relativePath);
                if (!File.Exists(path))
                {
                    diagnostics.Add("missing-indexed-file:" + relativePath);
                    continue;
                }

                string expected;
                if (!checksumMap.TryGetValue(relativePath, out expected))
                {
                    diagnostics.Add("missing-checksum:" + relativePath);
                    continue;
                }

                if (!string.Equals(expected, Sha256(path), System.StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add("checksum-mismatch:" + relativePath);
                    continue;
                }

                verifiedChecksumCount++;
            }

            var combined = manifest + "\n" + index + "\n" + checksums + "\n" + runbook + "\n" + acceptance + "\n" + readme;
            var acceptedFalse = BoolField(acceptance, "accepted") == false
                                && !Regex.IsMatch(acceptance, "\"accepted\"\\s*:\\s*true");
            var gateRequired = acceptance.Contains(FinalGate, System.StringComparison.Ordinal)
                               && acceptance.Contains("required", System.StringComparison.Ordinal);
            var notFinalWarnings = combined.Contains("not final", System.StringComparison.OrdinalIgnoreCase)
                                   || combined.Contains("manual gate", System.StringComparison.OrdinalIgnoreCase);
            var noForbiddenMarkers = !ContainsAny(combined, ForbiddenPackageMarkers);

            packageReady = diagnostics.Count == 0
                           && packageFileCount == 6
                           && indexedFileCount == 5
                           && verifiedChecksumCount == indexedFileCount
                           && acceptedFalse
                           && gateRequired
                           && notFinalWarnings
                           && noForbiddenMarkers;
            statusLine = "goal109 packageReady=" + packageReady
                         + " files=" + packageFileCount
                         + " indexed=" + indexedFileCount
                         + " checksums=" + verifiedChecksumCount
                         + " diagnostics=" + diagnostics.Count;
            return packageReady;
        }

        private static string ReadFile(string root, string fileName, List<string> diagnostics)
        {
            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing:" + fileName);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static List<string> IndexedFiles(string json)
        {
            var files = new List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\"relativePath\"\\s*:\\s*\"([^\"]+)\""))
            {
                var value = match.Groups[1].Value;
                if (!files.Contains(value))
                {
                    files.Add(value);
                }
            }

            files.Sort(System.StringComparer.Ordinal);
            return files;
        }

        private static Dictionary<string, string> ChecksumMap(string json)
        {
            var map = new Dictionary<string, string>(System.StringComparer.Ordinal);
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\"([^\"]+)\"\\s*:\\s*\"([0-9a-fA-F]{64})\""))
            {
                map[match.Groups[1].Value] = match.Groups[2].Value;
            }

            return map;
        }

        private static bool? BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            if (!match.Success)
            {
                return null;
            }

            bool value;
            return bool.TryParse(match.Groups[1].Value, out value) ? value : (bool?)null;
        }

        private static bool IsSafeRelativePath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                   && !Path.IsPathRooted(path)
                   && !path.Contains("..")
                   && !path.Contains("\\");
        }

        private static bool ContainsAny(string text, params string[] markers)
        {
            foreach (var marker in markers)
            {
                if ((text ?? string.Empty).Contains(marker, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Sha256(string path)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(File.ReadAllBytes(path));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
