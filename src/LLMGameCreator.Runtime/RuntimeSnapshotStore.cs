using System.Text.RegularExpressions;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RuntimeSnapshotStore : IRuntimeSnapshotStore
{
    private static readonly Regex SlotNameRegex = new Regex("^[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant);

    private readonly IRuntimeStateSerializer _serializer;

    public RuntimeSnapshotStore(IRuntimeStateSerializer serializer)
    {
        _serializer = serializer;
    }

    public RuntimeSnapshotResult SaveSnapshot(string projectFolder, string slotName, UnifiedRuntimeSession session)
    {
        var pathResult = ResolveSnapshotPath(projectFolder, slotName, mustExist: false);
        if (!pathResult.Success || pathResult.Path == null || pathResult.SlotName == null)
        {
            return pathResult;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pathResult.Path)!);
            File.WriteAllText(pathResult.Path, _serializer.Serialize(session));
            pathResult.Message = $"Runtime snapshot saved: {pathResult.SlotName}";
            pathResult.Session = session;
            return pathResult;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Failure("snapshot.save_failed", ex.Message, slotName);
        }
    }

    public RuntimeSnapshotResult LoadSnapshot(string projectFolder, string slotName)
    {
        var pathResult = ResolveSnapshotPath(projectFolder, slotName, mustExist: true);
        if (!pathResult.Success || pathResult.Path == null)
        {
            return pathResult;
        }

        try
        {
            var json = File.ReadAllText(pathResult.Path);
            pathResult.Session = _serializer.DeserializeUnifiedSession(json);
            pathResult.Message = $"Runtime snapshot loaded: {pathResult.SlotName}";
            return pathResult;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            return Failure("snapshot.load_failed", ex.Message, slotName);
        }
    }

    public RuntimeSnapshotListResult ListSnapshots(string projectFolder)
    {
        try
        {
            var root = ResolveProjectRoot(projectFolder);
            if (root == null)
            {
                return ListFailure("snapshot.project_folder.invalid", "Project folder is required.", projectFolder);
            }

            var directory = SnapshotDirectory(root);
            var result = new RuntimeSnapshotListResult { Success = true, Message = "Runtime snapshots listed." };
            if (!Directory.Exists(directory))
            {
                return result;
            }

            result.SlotNames.AddRange(Directory.GetFiles(directory, "*.runtime.json")
                .Select(path => Path.GetFileName(path))
                .Where(name => name.EndsWith(".runtime.json", StringComparison.OrdinalIgnoreCase))
                .Select(name => name.Substring(0, name.Length - ".runtime.json".Length))
                .OrderBy(name => name, StringComparer.Ordinal));
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ListFailure("snapshot.list_failed", ex.Message, projectFolder);
        }
    }

    private static RuntimeSnapshotResult ResolveSnapshotPath(string projectFolder, string slotName, bool mustExist)
    {
        var root = ResolveProjectRoot(projectFolder);
        if (root == null)
        {
            return Failure("snapshot.project_folder.invalid", "Project folder is required.", projectFolder);
        }

        var sanitized = SanitizeSlotName(slotName);
        if (sanitized == null)
        {
            return Failure("snapshot.slot_name.invalid", "Snapshot slot name must contain only letters, digits, dot, dash or underscore.", slotName);
        }

        var directory = SnapshotDirectory(root);
        var path = Path.GetFullPath(Path.Combine(directory, $"{sanitized}.runtime.json"));
        if (!path.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
        {
            return Failure("snapshot.path.invalid", "Resolved snapshot path escaped runtime-saves directory.", slotName);
        }

        if (mustExist && !File.Exists(path))
        {
            return Failure("snapshot.not_found", $"Runtime snapshot not found: {sanitized}", sanitized);
        }

        return new RuntimeSnapshotResult
        {
            Success = true,
            SlotName = sanitized,
            Path = path,
            Message = "Runtime snapshot path resolved."
        };
    }

    private static string? ResolveProjectRoot(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return null;
        }

        return Path.GetFullPath(projectFolder);
    }

    private static string SnapshotDirectory(string projectRoot)
    {
        return Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "runtime-saves"));
    }

    private static string? SanitizeSlotName(string slotName)
    {
        var text = slotName.Trim();
        if (string.IsNullOrWhiteSpace(text)
            || text.Contains("..", StringComparison.Ordinal)
            || text.Contains(Path.DirectorySeparatorChar)
            || text.Contains(Path.AltDirectorySeparatorChar)
            || !SlotNameRegex.IsMatch(text))
        {
            return null;
        }

        return text;
    }

    private static RuntimeSnapshotResult Failure(string code, string message, string? targetId)
    {
        return new RuntimeSnapshotResult
        {
            Success = false,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) }
        };
    }

    private static RuntimeSnapshotListResult ListFailure(string code, string message, string? targetId)
    {
        return new RuntimeSnapshotListResult
        {
            Success = false,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) }
        };
    }
}
