using System.Diagnostics;
using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed partial class OfflineGeoworldActualUnityEditorSourceReformatEvidenceService
{
    private static GitBytesResult RunGitBytes(string root, params string[] args)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return new GitBytesResult(false, [], ex.Message);
        }

        using var output = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(output);
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0
            ? new GitBytesResult(true, output.ToArray(), string.Empty)
            : new GitBytesResult(false, output.ToArray(), error.Trim());
    }

    private static string RunGitText(string root, params string[] args)
    {
        var result = RunGitBytes(root, args);
        return result.Succeeded ? Encoding.UTF8.GetString(result.OutputBytes) : string.Empty;
    }

    private static int RunGitExitCode(string root, params string[] args)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        try
        {
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch
        {
            return -1;
        }
    }

    private static string ParsePorcelainPath(string line)
    {
        if (line.Length <= 3)
        {
            return string.Empty;
        }

        var path = line[3..].Trim();
        var renameIndex = path.IndexOf(" -> ", StringComparison.Ordinal);
        return renameIndex >= 0 ? path[(renameIndex + 4)..].Trim() : path;
    }

    private static string NormalizeRelativePath(string path) =>
        path.Trim().Trim('"').Replace('\\', '/').TrimEnd('/');

    private static bool IsForbiddenChangedPath(string path)
    {
        path = NormalizeRelativePath(path);
        if (string.Equals(
            path,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            StringComparison.Ordinal))
        {
            return false;
        }

        if (string.Equals(
            path,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapPath,
            StringComparison.Ordinal))
        {
            return true;
        }

        var forbiddenPrefixes = new[]
        {
            "generator-library/",
            "src/LLMGameCreator.Runtime/",
            "src/LLMGameCreator.Runtime.Abstractions/",
            "src/LLMGameCreator.GamePackage/",
            "src/LLMGameCreator.Scripting/",
            "src/LLMGameCreator.AssetPipeline/",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/",
            "unity/LLMGameCreatorAlpha/ProjectSettings/",
            "unity/LLMGameCreatorAlpha/Packages/"
        };
        if (forbiddenPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal)))
        {
            return true;
        }

        var forbiddenExtensions = new[]
        {
            ".sln",
            ".csproj",
            ".props",
            ".targets",
            ".lock",
            ".unity",
            ".prefab",
            ".asmdef",
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
            ".bytes",
            ".osm",
            ".pbf",
            ".mbtiles",
            ".gpkg",
            ".geojson"
        };

        return forbiddenExtensions.Any(extension =>
            path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetFullPath(root),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root.");
        }

        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void ResetDirectory(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(path).StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root.");
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string Serialize<T>(T value) =>
        OfflineGeoworldUnityEditorPreviewJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldUnityEditorPreviewHash.Sha256Text(text);

    private sealed record GitBytesResult(
        bool Succeeded,
        byte[] OutputBytes,
        string Error);
}
