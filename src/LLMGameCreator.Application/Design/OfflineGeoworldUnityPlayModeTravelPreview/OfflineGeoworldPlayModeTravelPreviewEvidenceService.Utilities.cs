using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

public sealed partial class OfflineGeoworldPlayModeTravelPreviewEvidenceService
{
    private static readonly HashSet<string> ProviderNetworkMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "HttpClient",
        "UnityWebRequest",
        "WebRequest",
        "TcpClient",
        "NetworkStream",
        "Socket(",
        "http://",
        "https://",
        "ProviderCallRequested",
        "LLMProvider",
        "ComfyUI",
        "Fooocus"
    };

    private static readonly HashSet<string> ScenePrefabSettingsMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "EditorSceneManager.SaveScene",
        "EditorSceneManager.MarkSceneDirty",
        "PrefabUtility",
        "EditorBuildSettings",
        "ProjectSettings/",
        "Packages/manifest.json",
        ".unity",
        ".prefab"
    };

    private static readonly HashSet<string> BinaryOrRasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
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

    private static readonly HashSet<string> SupportedStepActions = new(StringComparer.Ordinal)
    {
        "load_manifest",
        "spawn_commands",
        "shift_window",
        "reset_and_replay"
    };

    private static IReadOnlyDictionary<string, string> ReadPayloadFiles(string root)
    {
        var directory = Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.StreamingAssetsRelativeRoot);
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                result[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> BuildBoundaryPrefetchChunks(
        string centerChunkKey,
        IReadOnlyList<string> activeChunkKeys)
    {
        var chunks = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var chunk in activeChunkKeys.Append(centerChunkKey).Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (!TryParseChunk(chunk, out var zoom, out var x, out var y))
            {
                continue;
            }

            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    chunks.Add("z" + zoom + "/x" + (x + dx) + "/y" + (y + dy));
                }
            }
        }

        chunks.ExceptWith(activeChunkKeys);
        return chunks.Order(StringComparer.Ordinal).ToList();
    }

    private static bool TryParseChunk(string chunk, out int zoom, out int x, out int y)
    {
        var match = Regex.Match(chunk, "^z(?<z>\\d+)/x(?<x>-?\\d+)/y(?<y>-?\\d+)$");
        if (match.Success
            && int.TryParse(match.Groups["z"].Value, out zoom)
            && int.TryParse(match.Groups["x"].Value, out x)
            && int.TryParse(match.Groups["y"].Value, out y))
        {
            return true;
        }

        zoom = 0;
        x = 0;
        y = 0;
        return false;
    }

    private static string BuildStepHashSeed(OfflineGeoworldPlayModeTravelStep step) =>
        string.Join(
            "|",
            step.PreviousStateHash,
            step.StepIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
            step.SourceGoal101StepId,
            step.Action,
            step.CenterChunkKey,
            string.Join(",", step.ActiveChunkKeys),
            string.Join(",", step.BoundaryPrefetchChunkKeys),
            string.Join(",", step.VisibleObjectIds),
            string.Join(",", step.NewlyVisibleObjectIds),
            string.Join(",", step.NewlyHiddenObjectIds));

    private static OfflineGeoworldPlayModeNegativeScenario Scenario(
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
                OfflineGeoworldPlayModeDiagnostic.Error(
                    code,
                    target,
                    "Goal103 negative proof rejected the mutated play-mode travel input.")
            ]
        };

    private static JsonDocument? ReadJson(
        string root,
        string relativePath,
        List<OfflineGeoworldPlayModeDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                "goal103.read.missing",
                relativePath,
                "Required Goal103 source artifact is missing."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                "goal103.read.invalid_json",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static string ReadOptionalText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static T? Deserialize<T>(string json) =>
        OfflineGeoworldPlayModeJson.Deserialize<T>(json);

    private static string Serialize<T>(T value) =>
        OfflineGeoworldPlayModeJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldPlayModeHash.Sha256Text(text);

    private static string HashFile(string path) =>
        OfflineGeoworldPlayModeHash.Sha256File(path);

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            root,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, path);
        return path;
    }

    private static string Relative(string root, string fullPath)
    {
        var relative = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
        return relative == "." ? string.Empty : relative;
    }

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
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + path);
        }
    }

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out value);
    }

    private static int TryGetNestedInt(JsonElement element, string propertyName, string nestedName) =>
        element.TryGetProperty(propertyName, out var property)
        && TryGetInt(property, nestedName, out var value)
            ? value
            : 0;

    private static bool TryGetNestedBool(JsonElement element, string propertyName, string nestedName) =>
        element.TryGetProperty(propertyName, out var property)
        && TryGetBool(property, nestedName);

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<OfflineGeoworldPlayModeDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                code,
                target,
                "Goal103 offline geoworld play-mode travel preview gate did not pass."));
        }
    }

    private static IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldPlayModeDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static bool ContainsAbsolutePath(string value) =>
        Regex.IsMatch(value, "[A-Za-z]:\\\\")
        || value.Contains("\\\\", StringComparison.Ordinal)
        || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
        || value.Contains("/home/", StringComparison.OrdinalIgnoreCase);

    private static bool IsBinaryOrRasterMedia(string pathOrText) =>
        BinaryOrRasterExtensions.Any(ext => pathOrText.Contains(ext, StringComparison.OrdinalIgnoreCase));

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static string Compact(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
        }

        return builder.ToString();
    }
}
