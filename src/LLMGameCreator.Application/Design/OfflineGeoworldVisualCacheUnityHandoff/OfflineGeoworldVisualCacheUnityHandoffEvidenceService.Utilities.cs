using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;

public sealed partial class OfflineGeoworldVisualCacheUnityHandoffEvidenceService
{
    private static string TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static bool TryGetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(property.GetString(), out var value) && value,
            _ => false
        };
    }

    private static OfflineGeoworldNegativeScenario Scenario(
        string id,
        string mutation,
        string code,
        string target) =>
        new()
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ActualStatus = "rejected",
            Diagnostics =
            [
                OfflineGeoworldVisualCacheDiagnostic.Error(
                    code,
                    target,
                    "Goal100 negative proof rejected the mutated payload.")
            ]
        };

    private static IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldVisualCacheDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message,
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static bool ContainsAbsolutePath(string value) =>
        value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
        || value.Contains("C:/", StringComparison.OrdinalIgnoreCase)
        || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
        || value.Contains(@"\\?\", StringComparison.OrdinalIgnoreCase);

    private static bool IsBinaryOrRasterMedia(string path) =>
        BinaryOrRasterExtensions.Contains(Path.GetExtension(path));

    private static string PayloadRole(string fileName) =>
        fileName switch
        {
            var name when name == OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName =>
                "manifest",
            var name when name == OfflineGeoworldVisualCacheUnityHandoffVocabulary.PackageIndexFileName =>
                "package_index",
            var name when name == OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerFileName =>
                "feature_chunk_ledger",
            var name when name == OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamWindowIndexFileName =>
                "stream_window_index",
            var name when name == OfflineGeoworldVisualCacheUnityHandoffVocabulary.RuntimeReadmeFileName =>
                "runtime_readme",
            _ => "payload"
        };

    private static string Compact(string value) =>
        value.Replace('/', '_').Replace('\\', '_').Replace(':', '_');

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static string ReadOptionalText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Serialize<T>(T value) =>
        OfflineGeoworldVisualCacheUnityHandoffJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldVisualCacheUnityHandoffHash.Sha256Text(text);

    private static string HashFile(string path) =>
        OfflineGeoworldVisualCacheUnityHandoffHash.Sha256File(path);

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetFullPath(root),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, path);
        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void ResetDirectory(string root, string path)
    {
        EnsureContained(root, path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path must stay under the project root.");
        }
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        ICollection<OfflineGeoworldVisualCacheDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                code,
                target,
                "Offline geoworld visual cache Unity handoff gate did not pass."));
        }
    }
}
