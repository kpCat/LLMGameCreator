using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public sealed partial class OfflineGeoworldAlphaSliceOrchestratorEvidenceService
{
    private static IReadOnlyDictionary<string, string> SnapshotHistoricalArtifacts(string root)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var directory in ComponentDefinitions()
                     .SelectMany(item => new[] { item.ArtifactRoot, item.StreamingAssetsRoot })
                     .Where(item => item.Length > 0)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            var full = Resolve(root, directory);
            if (!Directory.Exists(full))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                result[Relative(root, path)] = HashFile(path);
            }
        }

        return result;
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static JsonDocument? TryReadJson(string path) =>
        File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8)) : null;

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int ReadInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static string ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static void ResetDirectory(string root, string path)
    {
        if (!IsSubPath(root, path)
            || !path.Replace('\\', '/').Contains("goal-108", StringComparison.OrdinalIgnoreCase)
            && !path.Replace('\\', '/').Contains("OfflineGeoworldGoal108", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to reset a non-Goal108 directory: " + path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static bool IsSubPath(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathFullyQualified(path)
        && !path.Contains("..", StringComparison.Ordinal)
        && !path.Contains('\\', StringComparison.Ordinal);

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static bool ContainsAny(string text, params string[] markers) =>
        markers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static bool IsBinaryOrRasterMedia(string path)
    {
        var extension = Path.GetExtension(path);
        return OfflineGeoworldAlphaSliceVocabulary.ForbiddenBinaryOrRasterExtensions
            .Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
