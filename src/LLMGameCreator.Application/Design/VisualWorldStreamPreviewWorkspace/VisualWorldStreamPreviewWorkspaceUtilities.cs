using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyDictionary<string, string> LoadLedger(
        string projectRoot,
        string sourceRoot,
        string ledgerFileName)
    {
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        using var doc = TryReadJson(
            projectRoot,
            sourceRoot + "/" + ledgerFileName,
            []);
        if (doc is null || !TryGetArray(doc.RootElement, "files", out var files))
        {
            return ledger;
        }

        foreach (var file in files)
        {
            var relativePath = NormalizeLedgerPath(sourceRoot, TryGetString(file, "relativePath"));
            var sha = TryGetString(file, "sha256");
            if (!string.IsNullOrWhiteSpace(relativePath) && !string.IsNullOrWhiteSpace(sha))
            {
                ledger[relativePath] = sha;
            }
        }

        return ledger;
    }

    private static JsonDocument? TryReadJson(
        string projectRoot,
        string relativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.json.missing",
                relativePath,
                "Required JSON artifact was not found."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static IEnumerable<string> EnumerateExistingFiles(
        string projectRoot,
        string relativeDirectory,
        string pattern)
    {
        var fullDirectory = Resolve(projectRoot, relativeDirectory);
        if (!Directory.Exists(fullDirectory))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullDirectory, pattern, SearchOption.TopDirectoryOnly);
    }

    private static string BuildProofSummary(JsonElement root, string booleanProperty, bool passed)
    {
        var fragments = new List<string>
        {
            booleanProperty + "=" + passed.ToString().ToLowerInvariant()
        };
        foreach (var property in new[]
        {
            "seamCount",
            "cacheRecordCount",
            "reusedChunkKeyCount",
            "infiniteOverlapReusedChunkKeyCount",
            "portalOrTransitionLinkCount",
            "scenarioCount",
            "rejectedCount",
            "packageCount",
            "exportRecordCount",
            "recordCount",
            "payloadFileCount",
            "ruleCount",
            "streamWindowCount",
            "uniqueChunkKeyCount",
            "sourceGoal091ReusedChunkKeyCount",
            "exportReusedChunkKeyCount"
        })
        {
            if (TryGetInt(root, property, out var value))
            {
                fragments.Add(property + "=" + value);
            }
        }

        return string.Join("; ", fragments);
    }

    private static string HashFor(
        string projectRoot,
        string relativePath,
        IReadOnlyDictionary<string, string> ledger)
    {
        var normalized = NormalizePath(relativePath);
        if (ledger.TryGetValue(normalized, out var declaredHash))
        {
            return declaredHash;
        }

        var fullPath = Resolve(projectRoot, normalized);
        return File.Exists(fullPath) ? Sha256File(fullPath) : string.Empty;
    }

    private static string NormalizeLedgerPath(string sourceRoot, string relativePath)
    {
        var normalized = NormalizePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.StartsWith(".llmgc/", StringComparison.Ordinal))
        {
            return normalized;
        }

        return NormalizePath(sourceRoot + "/" + normalized);
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out List<JsonElement> values)
    {
        values = [];
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        values = property.EnumerateArray().ToList();
        return true;
    }

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

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static string TryGetInt(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value.ToString() : string.Empty;

    private static bool IsSafeSvg(string text) =>
        text.Contains("<svg", StringComparison.OrdinalIgnoreCase)
        && text.Contains("viewBox=", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("<script", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("https://", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("base64", StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathFullyQualified(path)
        && !path.StartsWith("/", StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Contains(":\\", StringComparison.Ordinal);

    private static bool IsBinaryOrRasterMedia(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".asset", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bytes", StringComparison.OrdinalIgnoreCase);
    }

    private static string TruncatePreview(string text) =>
        text.Length <= MaxPreviewCharacters
            ? text
            : text[..MaxPreviewCharacters] + Environment.NewLine + "... truncated ...";

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                code,
                target,
                "Visual world stream preview workspace quality gate did not pass."));
        }
    }

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        return File.Exists(fullPath) ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(
            Path.GetFullPath(projectRoot),
            NormalizePath(relativePath).Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string projectRoot, string path) =>
        Path.GetRelativePath(projectRoot, path).Replace('\\', '/');

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim().TrimStart('/');

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

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Sha256Text(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
