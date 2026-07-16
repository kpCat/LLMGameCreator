using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectSeedRegenerationTransaction
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GameProjectSeedRegenerationTransactionResult Apply(GameProjectSeedRegenerationTransactionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var project = Path.GetFullPath(request.ProjectFolder);
        var candidate = Path.GetFullPath(request.CandidateFolder);
        Require(Directory.Exists(project), "regeneration.project_missing");
        Require(Directory.Exists(candidate), "regeneration.candidate_missing");
        var transactionRoot = Confined(project, GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot
                                                + "/" + SafeSegment(request.AttemptId));
        var journalPath = Path.Combine(transactionRoot, "journal.json");
        var backupsRoot = Path.Combine(transactionRoot, "backups");
        var stagingRoot = Path.Combine(transactionRoot, "staging");
        var lockPath = Confined(project, GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot
                                         + "/regeneration.lock");
        Directory.CreateDirectory(transactionRoot);
        Directory.CreateDirectory(backupsRoot);
        Directory.CreateDirectory(stagingRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);

        using var projectLock = new FileStream(lockPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        var changed = new List<string>();
        SeedRegenerationTransactionJournal? journal = null;
        try
        {
            var generationRelative = SeededGeneratedProjectVocabulary.GenerationRelativeRoot;
            var authoringRelative = UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot;
            var identityRelative = IdentityRelative(project);
            var historyRelative = UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/"
                                  + request.CandidateBuildHistoryFileName;
            var recordRelative = GameProjectSeedRegenerationVocabulary.LastSuccessfulRelativePath;
            Require(!File.Exists(Confined(project, historyRelative)), "regeneration.history_collision");
            var supportRelative = ChangedSupportFiles(project, candidate);
            var roots = new[] { generationRelative, authoringRelative, "package.json", identityRelative }
                .Concat(supportRelative).Concat([historyRelative, recordRelative])
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
            var before = HashInventory(project, roots);
            var candidateHashes = CandidateInventory(candidate, roots, historyRelative,
                request.CandidateBuildHistoryFileName, request.RegenerationRecordJson);
            CopyBackups(project, backupsRoot, before);
            StageDirectory(candidate, generationRelative, Path.Combine(stagingRoot, "generation"));
            StageDirectory(candidate, authoringRelative, Path.Combine(stagingRoot, "authoring"));
            journal = new SeedRegenerationTransactionJournal
            {
                AttemptId = request.AttemptId,
                State = "prepared",
                AuthoritativeRelativePaths = roots,
                BeforeSha256 = before,
                CandidateSha256 = candidateHashes
            };
            WriteJournal(journalPath, journal);
            journal = journal with { State = "applying" };
            WriteJournal(journalPath, journal);

            SwapDirectory(project, generationRelative, Path.Combine(stagingRoot, "generation"),
                Path.Combine(transactionRoot, "original-generation"));
            changed.Add(generationRelative);
            journal = Step(journalPath, journal, "generation_swapped");
            Inject(request, GameProjectSeedRegenerationFailurePoint.AfterGenerationSwap);

            foreach (var relative in supportRelative)
            {
                ReplaceFileAtomic(Confined(candidate, relative), Confined(project, relative));
                changed.Add(relative);
            }
            journal = Step(journalPath, journal, "support_replaced");
            Inject(request, GameProjectSeedRegenerationFailurePoint.AfterSupportReplace);

            ReplaceFileAtomic(Confined(candidate, "package.json"), Confined(project, "package.json"));
            changed.Add("package.json");
            journal = Step(journalPath, journal, "package_replaced");
            Inject(request, GameProjectSeedRegenerationFailurePoint.AfterPackageReplace);

            SwapDirectory(project, authoringRelative, Path.Combine(stagingRoot, "authoring"),
                Path.Combine(transactionRoot, "original-authoring"));
            if (!string.IsNullOrWhiteSpace(identityRelative))
                ReplaceFileAtomic(Confined(candidate, identityRelative), Confined(project, identityRelative));
            changed.Add(authoringRelative);
            changed.Add(identityRelative);
            journal = Step(journalPath, journal, "authoring_identity_replaced");
            Inject(request, GameProjectSeedRegenerationFailurePoint.AfterAuthoringReplace);

            ReplaceFileAtomic(
                Confined(candidate, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/"
                                    + request.CandidateBuildHistoryFileName),
                Confined(project, historyRelative),
                overwrite: false);
            changed.Add(historyRelative);
            journal = Step(journalPath, journal, "history_added");
            Inject(request, GameProjectSeedRegenerationFailurePoint.AfterHistoryAdd);

            WriteTextAtomic(Confined(project, recordRelative), request.RegenerationRecordJson);
            changed.Add(recordRelative);
            journal = Step(journalPath, journal, "regeneration_record_written");
            Inject(request, GameProjectSeedRegenerationFailurePoint.BeforeFinalValidation);

            ValidateHashes(project, candidateHashes, "regeneration.apply_candidate_hash_mismatch");
            journal = journal with { State = "committed" };
            WriteJournal(journalPath, journal);
            CleanupTransactionWorkingFiles(transactionRoot);
            return new GameProjectSeedRegenerationTransactionResult
            {
                Passed = true,
                Applied = true,
                JournalStatus = "committed",
                BuildHistoryFileName = request.CandidateBuildHistoryFileName,
                ChangedRelativePaths = changed.Distinct(StringComparer.Ordinal).ToList()
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException)
        {
            if (journal is null) return new GameProjectSeedRegenerationTransactionResult
            {
                Diagnostics = [exception.Message]
            };
            try
            {
                journal = journal with { State = "rolling_back" };
                WriteJournal(journalPath, journal);
                RestoreFromBackups(project, backupsRoot, journal.BeforeSha256, journal.CandidateSha256);
                ValidateHashes(project, journal.BeforeSha256, "regeneration.rollback_hash_mismatch");
                journal = journal with { State = "rolled_back" };
                WriteJournal(journalPath, journal);
                return new GameProjectSeedRegenerationTransactionResult
                {
                    RollbackApplied = true,
                    JournalStatus = "rolled_back",
                    Diagnostics = [exception.Message],
                    ChangedRelativePaths = changed.Distinct(StringComparer.Ordinal).ToList()
                };
            }
            catch (Exception rollbackException) when (rollbackException is IOException
                                                       or UnauthorizedAccessException
                                                       or InvalidOperationException)
            {
                return new GameProjectSeedRegenerationTransactionResult
                {
                    JournalStatus = "rolling_back",
                    Diagnostics = [exception.Message, "regeneration.rollback_failed:" + rollbackException.Message],
                    ChangedRelativePaths = changed.Distinct(StringComparer.Ordinal).ToList()
                };
            }
        }
        finally
        {
            projectLock.Dispose();
            if (File.Exists(lockPath)) File.Delete(lockPath);
        }
    }

    public GameProjectSeedRegenerationTransactionResult Recover(string projectFolder)
    {
        var project = Path.GetFullPath(projectFolder);
        var root = Confined(project, GameProjectSeedRegenerationVocabulary.TransactionsRelativeRoot);
        if (!Directory.Exists(root)) return new GameProjectSeedRegenerationTransactionResult { Passed = true };
        foreach (var journalPath in Directory.EnumerateFiles(root, "journal.json", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var journal = JsonSerializer.Deserialize<SeedRegenerationTransactionJournal>(
                              File.ReadAllText(journalPath, Encoding.UTF8), JsonOptions)
                          ?? throw new InvalidOperationException("regeneration.recovery_required");
            if (journal.State is "rolled_back") continue;
            var transactionRoot = Path.GetDirectoryName(journalPath)!;
            var backupsRoot = Path.Combine(transactionRoot, "backups");
            if (journal.State == "committed")
            {
                ValidateHashes(project, journal.CandidateSha256, "regeneration.recovery_required");
                CleanupTransactionWorkingFiles(transactionRoot);
                continue;
            }
            if (journal.State is not "prepared" and not "applying" and not "rolling_back")
                return RecoveryRequired("regeneration.recovery_required");
            if (!BackupsComplete(backupsRoot, journal.BeforeSha256))
                return RecoveryRequired("regeneration.recovery_required");
            RestoreFromBackups(project, backupsRoot, journal.BeforeSha256, journal.CandidateSha256);
            ValidateHashes(project, journal.BeforeSha256, "regeneration.recovery_required");
            WriteJournal(journalPath, journal with { State = "rolled_back" });
        }
        return new GameProjectSeedRegenerationTransactionResult { Passed = true };
    }

    private static IReadOnlyList<string> ChangedSupportFiles(string project, string candidate)
    {
        var result = new List<string>();
        foreach (var relativeRoot in new[] { "assets", "scripts" })
        {
            var candidateRoot = Confined(candidate, relativeRoot);
            if (!Directory.Exists(candidateRoot)) continue;
            foreach (var path in Directory.EnumerateFiles(candidateRoot, "*", SearchOption.AllDirectories))
            {
                var relative = Relative(candidate, path);
                var authoritative = Confined(project, relative);
                if (!File.Exists(authoritative)
                    || !string.Equals(GameProjectSeedRegenerationRecordService.HashFile(path),
                        GameProjectSeedRegenerationRecordService.HashFile(authoritative), StringComparison.Ordinal))
                    result.Add(relative);
            }
        }
        return result.OrderBy(value => value, StringComparer.Ordinal).ToList();
    }

    private static SortedDictionary<string, string> CandidateInventory(
        string candidate,
        IReadOnlyList<string> roots,
        string historyRelative,
        string candidateHistoryFileName,
        string regenerationRecordJson)
    {
        var hashes = HashInventory(candidate, roots.Where(root => root != historyRelative
                                                                  && root != GameProjectSeedRegenerationVocabulary.LastSuccessfulRelativePath));
        var candidateHistory = Confined(candidate, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot
                                                   + "/" + candidateHistoryFileName);
        hashes[historyRelative] = GameProjectSeedRegenerationRecordService.HashFile(candidateHistory);
        hashes[GameProjectSeedRegenerationVocabulary.LastSuccessfulRelativePath] =
            GameProjectSeedRegenerationRecordService.HashText(regenerationRecordJson);
        return hashes;
    }

    private static SortedDictionary<string, string> HashInventory(string root, IEnumerable<string> paths)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var relative in paths)
        {
            var path = Confined(root, relative);
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    result[Relative(root, file)] = GameProjectSeedRegenerationRecordService.HashFile(file);
            }
            else result[relative] = File.Exists(path)
                ? GameProjectSeedRegenerationRecordService.HashFile(path)
                : string.Empty;
        }
        return result;
    }

    private static void CopyBackups(string project, string backupsRoot, IReadOnlyDictionary<string, string> before)
    {
        foreach (var item in before.Where(item => item.Value.Length > 0))
        {
            var source = Confined(project, item.Key);
            var target = Confined(backupsRoot, item.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: false);
            Require(string.Equals(GameProjectSeedRegenerationRecordService.HashFile(target), item.Value,
                StringComparison.Ordinal), "regeneration.backup_hash_mismatch");
        }
    }

    private static bool BackupsComplete(string backupsRoot, IReadOnlyDictionary<string, string> before) =>
        before.Where(item => item.Value.Length > 0).All(item =>
            File.Exists(Confined(backupsRoot, item.Key))
            && string.Equals(GameProjectSeedRegenerationRecordService.HashFile(Confined(backupsRoot, item.Key)),
                item.Value, StringComparison.Ordinal));

    private static void RestoreFromBackups(
        string project,
        string backupsRoot,
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> candidate)
    {
        if (!BackupsComplete(backupsRoot, before)) throw new InvalidOperationException("regeneration.backup_incomplete");
        foreach (var relative in candidate.Keys.Except(before.Keys, StringComparer.Ordinal)
                     .Concat(before.Where(item => item.Value.Length == 0).Select(item => item.Key))
                     .Distinct(StringComparer.Ordinal).OrderByDescending(value => value.Length))
        {
            var target = Confined(project, relative);
            if (File.Exists(target)) File.Delete(target);
        }
        foreach (var item in before.Where(item => item.Value.Length > 0))
            ReplaceFileAtomic(Confined(backupsRoot, item.Key), Confined(project, item.Key));
        RemoveEmptyDirectories(Confined(project, SeededGeneratedProjectVocabulary.GenerationRelativeRoot));
        RemoveEmptyDirectories(Confined(project, UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot));
    }

    private static void ValidateHashes(string root, IReadOnlyDictionary<string, string> expected, string diagnostic)
    {
        foreach (var item in expected)
        {
            var path = Confined(root, item.Key);
            var actual = File.Exists(path) ? GameProjectSeedRegenerationRecordService.HashFile(path) : string.Empty;
            if (!string.Equals(actual, item.Value, StringComparison.Ordinal))
                throw new InvalidOperationException(diagnostic + ":" + item.Key);
        }
    }

    private static void StageDirectory(string candidate, string relative, string target)
    {
        if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        CopyDirectory(Confined(candidate, relative), target);
    }

    private static void SwapDirectory(string project, string relative, string staged, string original)
    {
        var target = Confined(project, relative);
        if (Directory.Exists(original)) Directory.Delete(original, recursive: true);
        if (Directory.Exists(target)) Directory.Move(target, original);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        Directory.Move(staged, target);
    }

    private static void CopyDirectory(string source, string target)
    {
        Require(Directory.Exists(source), "regeneration.candidate_directory_missing");
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: false);
        }
    }

    private static void ReplaceFileAtomic(string source, string target, bool overwrite = true)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var temporary = target + ".regeneration-" + Guid.NewGuid().ToString("N");
        try
        {
            File.Copy(source, temporary, overwrite: false);
            File.Move(temporary, target, overwrite);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static void WriteTextAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, Utf8WithoutBom);
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static SeedRegenerationTransactionJournal Step(
        string path,
        SeedRegenerationTransactionJournal journal,
        string step)
    {
        var updated = journal with { AppliedStepIds = journal.AppliedStepIds.Append(step).ToList() };
        WriteJournal(path, updated);
        return updated;
    }

    private static void WriteJournal(string path, SeedRegenerationTransactionJournal journal) =>
        WriteTextAtomic(path, JsonSerializer.Serialize(journal, JsonOptions) + Environment.NewLine);

    private static void Inject(
        GameProjectSeedRegenerationTransactionRequest request,
        GameProjectSeedRegenerationFailurePoint point)
    {
        if (request.FailurePoint == point)
            throw new InvalidOperationException("regeneration.injected_failure:" + point);
    }

    private static string IdentityRelative(string project)
    {
        var path = new GameProjectIdentityStore().PathFor(project);
        return Relative(project, path);
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("regeneration.path_escape");
        return path;
    }

    private static string SafeSegment(string value)
    {
        var safe = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return safe.Length > 0 ? safe : throw new InvalidOperationException("regeneration.attempt_id_invalid");
    }

    private static void CleanupTransactionWorkingFiles(string transactionRoot)
    {
        foreach (var name in new[] { "backups", "staging", "original-generation", "original-authoring" })
        {
            var path = Path.Combine(transactionRoot, name);
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }

    private static void RemoveEmptyDirectories(string root)
    {
        if (!Directory.Exists(root)) return;
        foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(value => value.Length))
            if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
    }

    private static GameProjectSeedRegenerationTransactionResult RecoveryRequired(string diagnostic) => new()
    {
        Diagnostics = [diagnostic],
        JournalStatus = "recovery_required"
    };

    private static void Require(bool condition, string diagnostic)
    {
        if (!condition) throw new InvalidOperationException(diagnostic);
    }
}
