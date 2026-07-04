using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

public sealed partial class OfflineGeoworldAlphaSliceExportPackageEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static JsonDocument? TryReadJson(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8)) : null;
    }

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

    private static bool ContainsAny(string text, params string[] markers) =>
        markers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));

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

    private static bool IsBinaryOrRasterMedia(string path)
    {
        var extension = Path.GetExtension(path);
        return ForbiddenBinaryOrRasterExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSubPath(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
    }

    private static void ResetCurrentGoalDirectory(string root, string path)
    {
        var normalized = path.Replace('\\', '/');
        if (!IsSubPath(root, path)
            || !normalized.Contains("goal-109", StringComparison.OrdinalIgnoreCase)
            && !normalized.Contains("OfflineGeoworldGoal109", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to reset a non-Goal109 directory: " + path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static IReadOnlyDictionary<string, string> SnapshotHashes(string root, IEnumerable<string> relativePaths)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var relativePath in relativePaths.OrderBy(path => path, StringComparer.Ordinal))
        {
            var fullPath = Resolve(root, relativePath);
            if (File.Exists(fullPath))
            {
                result[relativePath] = HashFile(fullPath);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> EnumerateFilesIfExists(string root, string relativeDirectory)
    {
        var directory = Resolve(root, relativeDirectory);
        return Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Select(path => Relative(root, path))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList()
            : [];
    }

    private static readonly HashSet<string> ForbiddenBinaryOrRasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };
}
