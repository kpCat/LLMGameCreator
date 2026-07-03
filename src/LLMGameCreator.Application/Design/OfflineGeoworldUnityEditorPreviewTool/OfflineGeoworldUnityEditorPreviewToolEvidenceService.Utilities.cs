using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed partial class OfflineGeoworldUnityEditorPreviewToolEvidenceService
{
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

    private static IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldUnityEditorPreviewDiagnostic> diagnostics) =>
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

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static string ReadOptionalText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

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
        ICollection<OfflineGeoworldUnityEditorPreviewDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                code,
                target,
                "Offline geoworld Unity editor preview tool gate did not pass."));
        }
    }

    private static JsonDocument? ReadJson(
        string root,
        string relativePath,
        List<OfflineGeoworldUnityEditorPreviewDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102.json.missing",
                relativePath,
                "Required source JSON file is missing."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static string Serialize<T>(T value) =>
        OfflineGeoworldUnityEditorPreviewJson.Serialize(value);

    private static T? Deserialize<T>(string json) =>
        OfflineGeoworldUnityEditorPreviewJson.Deserialize<T>(json);

    private static string Hash(string text) =>
        OfflineGeoworldUnityEditorPreviewHash.Sha256Text(text);

    private static string HashFile(string path) =>
        OfflineGeoworldUnityEditorPreviewHash.Sha256File(path);
}
