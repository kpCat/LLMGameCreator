using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

public sealed partial class OfflineGeoworldSessionPersistenceReplayEvidenceService
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

    private static readonly HashSet<string> ExternalDependencyMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "UnityEngine.InputSystem",
        "InputAction",
        "PackageManager",
        "com.unity.inputsystem",
        "Newtonsoft.Json",
        "DllImport",
        "Process.Start"
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

    private static IReadOnlyDictionary<string, string> ReadPayloadFiles(string root)
    {
        var directory = Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.StreamingAssetsRelativeRoot);
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                result[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return result;
    }

    private static string BuildSnapshotHash(
        string initialStateHash,
        int checkpointStep,
        string checkpointStateHash,
        IEnumerable<string> eventIds) =>
        Hash("goal106|snapshot|"
             + initialStateHash
             + "|"
             + checkpointStep.ToString(System.Globalization.CultureInfo.InvariantCulture)
             + "|"
             + checkpointStateHash
             + "|"
             + string.Join(",", eventIds));

    private static OfflineGeoworldSessionNegativeScenario Scenario(
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
                OfflineGeoworldSessionDiagnostic.Error(
                    code,
                    target,
                    "Goal106 negative proof rejected the mutated save/load/replay input.")
            ]
        };

    private static T? ReadSource<T>(
        string root,
        string relativePath,
        List<OfflineGeoworldSessionDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                "goal106.read.missing",
                relativePath,
                "Required Goal106 source artifact is missing."));
            return default;
        }

        try
        {
            return Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                "goal106.read.invalid_json",
                relativePath,
                ex.Message));
            return default;
        }
    }

    private static string ReadOptionalText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static T? Deserialize<T>(string json) => OfflineGeoworldSessionJson.Deserialize<T>(json);

    private static string Serialize<T>(T value) => OfflineGeoworldSessionJson.Serialize(value);

    private static string Hash(string text) => OfflineGeoworldSessionHash.Sha256Text(text);

    private static string HashFile(string path) => OfflineGeoworldSessionHash.Sha256File(path);

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
        var rootFull = Path.GetFullPath(root).TrimEnd(
                           Path.DirectorySeparatorChar,
                           Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + path);
        }
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<OfflineGeoworldSessionDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                code,
                target,
                "Goal106 offline geoworld session persistence replay gate did not pass."));
        }
    }

    private static IReadOnlyList<OfflineGeoworldSessionDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldSessionDiagnostic> diagnostics) =>
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

    private static int CountLines(string text) => string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static int MaxLineLength(string text) =>
        string.IsNullOrEmpty(text)
            ? 0
            : text.Split('\n').Max(line => line.TrimEnd('\r').Length);

    private static bool IsMinified(string text) =>
        CountLines(text) <= 2 && text.Count(ch => ch == ';') > 12;
}
