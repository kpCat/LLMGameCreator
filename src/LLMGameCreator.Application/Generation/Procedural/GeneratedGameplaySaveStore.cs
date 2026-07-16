using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplaySaveStore
{
    private static readonly Regex SlotNameRegex = new("^[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant);
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public GeneratedGameplaySaveStoreReadResult ReadSlot(string projectFolder, string slotName)
    {
        try
        {
            var slot = ResolveSlot(projectFolder, slotName);
            var manifestPath = Path.Combine(slot.Path, "slot.json");
            if (!File.Exists(manifestPath)) return Failed(slot.Name, slot.Path, "generated_save.slot_missing");
            var manifest = GeneratedGameplaySaveJson.Deserialize<GeneratedGameplaySaveSlotManifest>(
                File.ReadAllText(manifestPath, Encoding.UTF8));
            if (manifest is null
                || manifest.SchemaVersion != GeneratedGameplaySaveVocabulary.SlotSchemaVersion
                || !string.Equals(manifest.SlotName, slot.Name, StringComparison.Ordinal)
                || manifest.RevisionSha256s.Count == 0
                || manifest.RevisionSha256s.Distinct(StringComparer.Ordinal).Count()
                   != manifest.RevisionSha256s.Count
                || !manifest.RevisionSha256s.Contains(manifest.CurrentRevisionSha256, StringComparer.Ordinal))
                return Failed(slot.Name, slot.Path, "generated_save.slot_manifest_invalid");

            var revisions = new List<GeneratedGameplaySaveRevision>();
            foreach (var revisionSha256 in manifest.RevisionSha256s)
            {
                var revision = ReadRevision(projectFolder, slot.Name, revisionSha256);
                if (!revision.Passed || revision.CurrentRevision is null)
                    return Failed(slot.Name, slot.Path,
                        revision.Diagnostics.FirstOrDefault() ?? "generated_save.revision_invalid");
                revisions.Add(revision.CurrentRevision);
            }

            var current = revisions.Single(revision =>
                string.Equals(revision.RevisionSha256, manifest.CurrentRevisionSha256, StringComparison.Ordinal));
            return new GeneratedGameplaySaveStoreReadResult
            {
                Passed = true,
                SlotName = slot.Name,
                SlotPath = slot.Path,
                Manifest = manifest,
                CurrentRevision = current,
                Revisions = revisions
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            return Failed(slotName, string.Empty, Diagnostic(exception));
        }
    }

    public GeneratedGameplaySaveStoreReadResult ReadRevision(
        string projectFolder,
        string slotName,
        string revisionSha256)
    {
        try
        {
            var slot = ResolveSlot(projectFolder, slotName);
            RequireSha256(revisionSha256);
            var path = Path.Combine(slot.Path, "revisions", revisionSha256 + ".json");
            if (!File.Exists(path)) return Failed(slot.Name, slot.Path, "generated_save.revision_missing");
            var revision = GeneratedGameplaySaveJson.Deserialize<GeneratedGameplaySaveRevision>(
                File.ReadAllText(path, Encoding.UTF8));
            if (revision is null
                || revision.SchemaVersion != GeneratedGameplaySaveVocabulary.RevisionSchemaVersion
                || !string.Equals(revision.RevisionSha256, revisionSha256, StringComparison.Ordinal)
                || !string.Equals(GeneratedGameplaySaveJson.RevisionSha256(revision), revisionSha256,
                    StringComparison.Ordinal)
                || !string.Equals(GeneratedGameplaySaveJson.HashText(revision.UnifiedRuntimeSessionJson),
                    revision.UnifiedRuntimeSessionSha256, StringComparison.Ordinal))
                return Failed(slot.Name, slot.Path, "generated_save.revision_hash_mismatch");
            return new GeneratedGameplaySaveStoreReadResult
            {
                Passed = true,
                SlotName = slot.Name,
                SlotPath = slot.Path,
                CurrentRevision = revision,
                Revisions = [revision]
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            return Failed(slotName, string.Empty, Diagnostic(exception));
        }
    }

    public GeneratedGameplaySaveStoreWriteResult WriteRevision(
        string projectFolder,
        string slotName,
        GeneratedGameplaySaveRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        try
        {
            var slot = ResolveSlot(projectFolder, slotName);
            var calculated = GeneratedGameplaySaveJson.RevisionSha256(revision);
            if (!string.IsNullOrWhiteSpace(revision.RevisionSha256)
                && !string.Equals(revision.RevisionSha256, calculated, StringComparison.Ordinal))
                return WriteFailed(slot.Name, calculated, "generated_save.revision_hash_mismatch");
            revision = revision with { RevisionSha256 = calculated };
            var bytes = Utf8WithoutBom.GetBytes(GeneratedGameplaySaveJson.Stored(revision));
            var revisionsRoot = Path.Combine(slot.Path, "revisions");
            var revisionPath = Path.Combine(revisionsRoot, calculated + ".json");
            var manifestPath = Path.Combine(slot.Path, "slot.json");
            var beforeManifest = File.Exists(manifestPath) ? File.ReadAllBytes(manifestPath) : null;
            var revisionCreated = false;
            try
            {
                Directory.CreateDirectory(revisionsRoot);
                if (File.Exists(revisionPath))
                {
                    if (!File.ReadAllBytes(revisionPath).SequenceEqual(bytes))
                        return WriteFailed(slot.Name, calculated, "generated_save.revision_collision");
                }
                else
                {
                    WriteNew(revisionPath, bytes);
                    revisionCreated = true;
                }

                GeneratedGameplaySaveSlotManifest manifest;
                if (beforeManifest is null)
                {
                    manifest = new GeneratedGameplaySaveSlotManifest
                    {
                        SlotName = slot.Name,
                        CurrentRevisionSha256 = calculated,
                        RevisionSha256s = [calculated]
                    };
                }
                else
                {
                    var previous = GeneratedGameplaySaveJson.Deserialize<GeneratedGameplaySaveSlotManifest>(
                        Utf8WithoutBom.GetString(beforeManifest));
                    if (previous is null
                        || previous.SchemaVersion != GeneratedGameplaySaveVocabulary.SlotSchemaVersion
                        || !string.Equals(previous.SlotName, slot.Name, StringComparison.Ordinal)
                        || previous.RevisionSha256s.Distinct(StringComparer.Ordinal).Count()
                           != previous.RevisionSha256s.Count
                        || !previous.RevisionSha256s.Contains(previous.CurrentRevisionSha256,
                            StringComparer.Ordinal))
                        return RollbackWrite(slot.Name, calculated, revisionPath, revisionCreated,
                            "generated_save.slot_manifest_invalid");
                    manifest = previous with
                    {
                        CurrentRevisionSha256 = calculated,
                        RevisionSha256s = previous.RevisionSha256s.Contains(calculated, StringComparer.Ordinal)
                            ? previous.RevisionSha256s
                            : previous.RevisionSha256s.Append(calculated).ToList()
                    };
                }

                WriteAtomic(manifestPath, Utf8WithoutBom.GetBytes(GeneratedGameplaySaveJson.Stored(manifest)));
                return new GeneratedGameplaySaveStoreWriteResult
                {
                    Passed = true,
                    Deduplicated = !revisionCreated,
                    RevisionCreated = revisionCreated,
                    SlotName = slot.Name,
                    RevisionSha256 = calculated,
                    Manifest = manifest
                };
            }
            catch
            {
                if (revisionCreated && File.Exists(revisionPath)) File.Delete(revisionPath);
                if (beforeManifest is not null) WriteAtomic(manifestPath, beforeManifest);
                else if (File.Exists(manifestPath)) File.Delete(manifestPath);
                throw;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException)
        {
            return WriteFailed(slotName, string.Empty, Diagnostic(exception));
        }
    }

    public IReadOnlyList<string> ListSlotNames(string projectFolder)
    {
        var root = Root(projectFolder);
        if (!Directory.Exists(root)) return [];
        return Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    public string RootPath(string projectFolder) => Root(projectFolder);

    private static GeneratedGameplaySaveStoreWriteResult RollbackWrite(
        string slotName,
        string revisionSha256,
        string revisionPath,
        bool revisionCreated,
        string diagnostic)
    {
        if (revisionCreated && File.Exists(revisionPath)) File.Delete(revisionPath);
        return WriteFailed(slotName, revisionSha256, diagnostic);
    }

    private static (string Name, string Path) ResolveSlot(string projectFolder, string slotName)
    {
        var name = slotName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name)
            || name.Contains("..", StringComparison.Ordinal)
            || name.Contains(Path.DirectorySeparatorChar)
            || name.Contains(Path.AltDirectorySeparatorChar)
            || !SlotNameRegex.IsMatch(name))
            throw new InvalidOperationException("generated_save.slot_invalid");
        var root = Root(projectFolder);
        var path = Path.GetFullPath(Path.Combine(root, name));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, PathComparison))
            throw new InvalidOperationException("generated_save.path_escape");
        return (name, path);
    }

    private static string Root(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
            throw new InvalidOperationException("generated_save.project_not_ready");
        return Path.GetFullPath(Path.Combine(Path.GetFullPath(projectFolder),
            GeneratedGameplaySaveVocabulary.RootRelativePath.Replace('/', Path.DirectorySeparatorChar)))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static void RequireSha256(string value)
    {
        if (value.Length != 64 || value.Any(character => !char.IsAsciiHexDigit(character) || char.IsUpper(character)))
            throw new InvalidOperationException("generated_save.revision_hash_mismatch");
    }

    private static void WriteNew(string path, byte[] bytes)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            4096, FileOptions.WriteThrough);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private static void WriteAtomic(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            WriteNew(temporary, bytes);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static GeneratedGameplaySaveStoreReadResult Failed(
        string slotName,
        string slotPath,
        string diagnostic) => new()
    {
        SlotName = slotName,
        SlotPath = slotPath,
        Diagnostics = [diagnostic]
    };

    private static GeneratedGameplaySaveStoreWriteResult WriteFailed(
        string slotName,
        string revisionSha256,
        string diagnostic) => new()
    {
        SlotName = slotName,
        RevisionSha256 = revisionSha256,
        Diagnostics = [diagnostic]
    };

    private static string Diagnostic(Exception exception) => exception.Message.StartsWith("generated_save.",
        StringComparison.Ordinal) ? exception.Message : "generated_save.store_failed:" + exception.Message;

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
