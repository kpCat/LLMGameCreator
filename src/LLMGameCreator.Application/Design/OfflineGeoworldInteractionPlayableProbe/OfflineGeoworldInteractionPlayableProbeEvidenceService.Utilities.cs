using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

public sealed partial class OfflineGeoworldInteractionPlayableProbeEvidenceService
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
        var directory = Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.StreamingAssetsRelativeRoot);
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                result[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return result;
    }

    private static string BuildInitialStateHash(
        IReadOnlyList<OfflineGeoworldInteractionTargetRecord> targets,
        IReadOnlyList<OfflineGeoworldInteractionActionRecord> actions) =>
        Hash("goal105|initial|"
             + string.Join(",", targets.Select(item => item.TargetId).Order(StringComparer.Ordinal))
             + "|"
             + string.Join(",", actions.Select(item => item.ActionId).Order(StringComparer.Ordinal)));

    private static string BuildDeltaHashSeed(OfflineGeoworldInteractionStateDelta delta) =>
        string.Join(
            "|",
            delta.PreviousStateHash,
            delta.DeltaIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
            delta.EventId,
            delta.TargetId,
            delta.ActionId,
            delta.ActionKind,
            delta.DeltaKind,
            delta.StateKey,
            delta.StateValue);

    private static string ActionId(string targetId, string actionKind) =>
        targetId.Replace("interaction_target/", "interaction_action/", StringComparison.Ordinal)
        + "/"
        + actionKind;

    private static double Distance(
        int playerGridX,
        int playerGridZ,
        OfflineGeoworldInteractionTargetRecord target)
    {
        var dx = playerGridX - target.GridX;
        var dz = playerGridZ - target.GridZ;
        return Math.Round(Math.Sqrt(dx * dx + dz * dz), 3, MidpointRounding.AwayFromZero);
    }

    private static OfflineGeoworldInteractionNegativeScenario Scenario(
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
                OfflineGeoworldInteractionDiagnostic.Error(
                    code,
                    target,
                    "Goal105 negative proof rejected the mutated interaction probe input.")
            ]
        };

    private static T? ReadSource<T>(
        string root,
        string relativePath,
        List<OfflineGeoworldInteractionDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                "goal105.read.missing",
                relativePath,
                "Required Goal105 source artifact is missing."));
            return default;
        }

        try
        {
            return Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                "goal105.read.invalid_json",
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

    private static T? Deserialize<T>(string json) =>
        OfflineGeoworldInteractionJson.Deserialize<T>(json);

    private static string Serialize<T>(T value) =>
        OfflineGeoworldInteractionJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldInteractionHash.Sha256Text(text);

    private static string HashFile(string path) =>
        OfflineGeoworldInteractionHash.Sha256File(path);

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

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<OfflineGeoworldInteractionDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                code,
                target,
                "Goal105 offline geoworld interaction playable probe gate did not pass."));
        }
    }

    private static IReadOnlyList<OfflineGeoworldInteractionDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldInteractionDiagnostic> diagnostics) =>
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

    private static int MaxLineLength(string text) =>
        string.IsNullOrEmpty(text)
            ? 0
            : text.Split('\n').Max(line => line.TrimEnd('\r').Length);

    private static bool IsMinified(string text) =>
        CountLines(text) <= 2 && text.Count(ch => ch == ';') > 12;

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
