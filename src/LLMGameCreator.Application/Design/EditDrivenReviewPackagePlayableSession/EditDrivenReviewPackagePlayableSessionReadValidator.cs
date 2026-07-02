using System.Text;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

namespace LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

internal static class EditDrivenReviewPackagePlayableSessionReadValidator
{
    private const string ReportPath =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/edit-driven-review-package-materialization-report.md";
    private const string LedgerPath =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/package-file-ledger.json";
    private const string ManifestPath =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/manifest.json";
    private const string PackageIndexPath =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/package-index.json";
    private const string PlayerIndexPath =
        ".llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/player-readable-index.json";

    public static EditDrivenReviewPackagePlayableSessionReadContext LoadFromDisk(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();
        var reportText = ReadOptional(root, ReportPath);
        var ledgerText = ReadOptional(root, LedgerPath);
        var manifestText = ReadOptional(root, ManifestPath);
        var packageIndexText = ReadOptional(root, PackageIndexPath);
        var playerIndexText = ReadOptional(root, PlayerIndexPath);
        var reportFields = ParseReportFields(reportText);
        var ledger = Deserialize<EditDrivenReviewPackageFileLedger>(ledgerText);
        var manifest = Deserialize<EditDrivenReviewPackageManifest>(manifestText);
        var packageIndex = Deserialize<EditDrivenReviewPackageIndex>(packageIndexText);
        var playerIndex = Deserialize<EditDrivenPlayerReadablePackageIndex>(playerIndexText);
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal);

        if (ledger is not null)
        {
            foreach (var file in ledger.Files.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
            {
                var relative = EditDrivenReviewPackagePlayableSessionVocabulary.Goal077RelativeOutputDirectory
                    + "/" + file.RelativePath;
                var payload = ReadOptional(root, relative);
                if (!string.IsNullOrEmpty(payload))
                {
                    payloads[file.RelativePath] = payload;
                }
            }
        }

        return Validate(
            reportText,
            ledgerText,
            manifestText,
            packageIndexText,
            playerIndexText,
            reportFields,
            ledger,
            manifest,
            packageIndex,
            playerIndex,
            payloads,
            diagnostics);
    }

    public static EditDrivenReviewPackagePlayableSessionPackageReadProof ValidateMutatedPayloads(
        EditDrivenReviewPackagePlayableSessionReadContext source,
        IReadOnlyDictionary<string, string> payloads)
    {
        var ledgerText = Serialize(source.PackageFileLedger);
        var manifestText = payloads.TryGetValue("review-package/manifest.json", out var manifest)
            ? manifest
            : string.Empty;
        var packageIndexText = payloads.TryGetValue("review-package/package-index.json", out var packageIndex)
            ? packageIndex
            : string.Empty;
        var playerIndexText = payloads.TryGetValue("review-package/player-readable-index.json", out var playerIndex)
            ? playerIndex
            : string.Empty;
        var context = Validate(
            source.ReportMarkdown,
            ledgerText,
            manifestText,
            packageIndexText,
            playerIndexText,
            source.ReportFields,
            source.PackageFileLedger,
            Deserialize<EditDrivenReviewPackageManifest>(manifestText),
            Deserialize<EditDrivenReviewPackageIndex>(packageIndexText),
            Deserialize<EditDrivenPlayerReadablePackageIndex>(playerIndexText),
            payloads,
            []);

        return context.PackageReadProof;
    }

    private static EditDrivenReviewPackagePlayableSessionReadContext Validate(
        string reportText,
        string ledgerText,
        string manifestText,
        string packageIndexText,
        string playerIndexText,
        EditDrivenReviewPackagePlayableSessionReportFields reportFields,
        EditDrivenReviewPackageFileLedger? ledger,
        EditDrivenReviewPackageManifest? manifest,
        EditDrivenReviewPackageIndex? packageIndex,
        EditDrivenPlayerReadablePackageIndex? playerIndex,
        IReadOnlyDictionary<string, string> payloads,
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> initialDiagnostics)
    {
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>(initialDiagnostics);
        var reportExists = !string.IsNullOrWhiteSpace(reportText);
        var reportHash = Hash(reportText);
        var manifestHash = Hash(manifestText);
        var ledgerHash = Hash(ledgerText);
        var packageIndexHash = Hash(packageIndexText);
        var playerIndexHash = Hash(playerIndexText);
        var reportFieldsPresent = RequiredReportFieldsPresent(reportFields);
        var reportHashesMatch = reportFieldsPresent
            && reportFields.ReviewPackageManifestHash == manifestHash
            && reportFields.PackageFileLedgerHash == ledgerHash
            && reportFields.PlayerReadablePackageIndexHash == playerIndexHash;

        if (!reportExists)
        {
            diagnostics.Add(Error("goal078.read.goal077_report_missing", ReportPath, "Goal 077 report is required."));
        }

        if (!reportFieldsPresent)
        {
            diagnostics.Add(Error(
                "goal078.read.goal077_report_hash_fields_missing",
                ReportPath,
                "Goal 077 report must expose source and package hash fields."));
        }

        if (reportFieldsPresent && !reportHashesMatch)
        {
            diagnostics.Add(Error(
                "goal078.read.goal077_report_hash_mismatch",
                ReportPath,
                "Goal 077 report hash fields must match current package files."));
        }

        if (reportFields.ImplementationStatus != "GREEN")
        {
            diagnostics.Add(Error(
                "goal078.read.goal077_not_green",
                ReportPath,
                "Goal 077 report must be GREEN before a playable session is produced."));
        }

        if (reportFields.Accepted != "false")
        {
            diagnostics.Add(Error(
                "goal078.read.goal077_artifact_acceptance_mutated",
                ReportPath,
                "Goal 077 artifact must remain accepted=false; only state docs record handoff acceptance."));
        }

        var allLedgerFilesExist = true;
        var allLedgerHashesMatch = true;
        var ledgerFiles = ledger?.Files ?? [];
        foreach (var entry in ledgerFiles)
        {
            if (!payloads.TryGetValue(entry.RelativePath, out var payload))
            {
                allLedgerFilesExist = false;
                diagnostics.Add(Error(
                    "goal078.read.ledger_file_missing",
                    entry.RelativePath,
                    "Ledger-listed review package file is missing."));
                continue;
            }

            var actualHash = Hash(payload);
            if (actualHash != entry.Sha256)
            {
                allLedgerHashesMatch = false;
                diagnostics.Add(Error(
                    "goal078.read.ledger_hash_mismatch",
                    entry.RelativePath,
                    "Ledger-listed review package file hash does not match current bytes."));
            }
        }

        var targetRecords = BuildTargetRecords(payloads, ledgerFiles, diagnostics);
        var rows = BuildRows(packageIndex, targetRecords);
        var packageTargetsInLedger = ValidatePackageIndexTargets(packageIndex, ledgerFiles, diagnostics);
        var playerTargetsInLedger = ValidatePlayerIndexTargets(playerIndex, packageIndex, ledgerFiles, diagnostics);
        if (rows.Count != 9)
        {
            diagnostics.Add(Error(
                "goal078.read.unexpected_row_count",
                "review-package/package-index.json",
                "Expected 9 rows from current Goal 077 evidence but found " + rows.Count + "."));
        }

        if (targetRecords.Count != 18)
        {
            diagnostics.Add(Error(
                "goal078.read.unexpected_target_count",
                "review-package/targets",
                "Expected 18 target files from current Goal 077 evidence but found " + targetRecords.Count + "."));
        }

        var proof = new EditDrivenReviewPackagePlayableSessionPackageReadProof
        {
            Passed = diagnostics.Count == 0
                && reportExists
                && reportFieldsPresent
                && reportHashesMatch
                && manifest is not null
                && ledger is not null
                && packageIndex is not null
                && playerIndex is not null
                && allLedgerFilesExist
                && allLedgerHashesMatch
                && packageTargetsInLedger
                && playerTargetsInLedger
                && rows.Count == 9
                && targetRecords.Count == 18,
            Goal077ReportExists = reportExists,
            Goal077ReportHashFieldsPresent = reportFieldsPresent,
            Goal077ReportHashesMatchCurrentFiles = reportHashesMatch,
            ReviewPackageManifestExists = manifest is not null,
            PackageLedgerExists = ledger is not null,
            PackageIndexExists = packageIndex is not null,
            PlayerReadableIndexExists = playerIndex is not null,
            AllLedgerFilesExist = allLedgerFilesExist,
            AllLedgerFileHashesMatch = allLedgerHashesMatch,
            AllPackageIndexTargetsInLedger = packageTargetsInLedger,
            AllPlayerIndexTargetsInLedger = playerTargetsInLedger,
            RowCount = rows.Count,
            TargetCount = targetRecords.Count,
            LedgerFileCount = ledgerFiles.Count,
            SourceGoal077ReportHash = reportHash,
            ReviewPackageManifestHash = manifestHash,
            PackageFileLedgerHash = ledgerHash,
            PackageIndexHash = packageIndexHash,
            PlayerReadableIndexHash = playerIndexHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return new EditDrivenReviewPackagePlayableSessionReadContext(
            proof,
            reportText,
            reportFields,
            ledger ?? new EditDrivenReviewPackageFileLedger(),
            manifest ?? new EditDrivenReviewPackageManifest(),
            packageIndex ?? new EditDrivenReviewPackageIndex(),
            playerIndex ?? new EditDrivenPlayerReadablePackageIndex(),
            rows,
            targetRecords,
            payloads);
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionTargetRecord> BuildTargetRecords(
        IReadOnlyDictionary<string, string> payloads,
        IReadOnlyList<EditDrivenReviewPackageFileEntry> ledgerFiles,
        ICollection<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics)
    {
        var records = new List<EditDrivenReviewPackagePlayableSessionTargetRecord>();
        foreach (var entry in ledgerFiles.Where(item => item.Role == "target").OrderBy(item => item.TargetId, StringComparer.Ordinal))
        {
            if (!payloads.TryGetValue(entry.RelativePath, out var payload))
            {
                continue;
            }

            var target = Deserialize<EditDrivenReviewPackageTargetFile>(payload);
            if (target is null)
            {
                diagnostics.Add(Error(
                    "goal078.read.target_payload_invalid_json",
                    entry.RelativePath,
                    "Ledger-listed target payload could not be deserialized."));
                continue;
            }

            records.Add(new EditDrivenReviewPackagePlayableSessionTargetRecord
            {
                RowId = target.SourceRowId,
                FamilyId = target.FamilyId,
                SeedId = target.SeedId,
                TargetId = target.TargetId,
                RelativePath = entry.RelativePath,
                LogicalPackagePath = target.LogicalPackagePath,
                FileHash = entry.Sha256,
                PayloadHash = Hash(payload),
                PayloadJson = payload,
                BeforeValue = target.BeforeValue,
                AfterValue = target.AfterValue,
                BeforeHash = target.BeforeHash,
                AfterHash = target.AfterHash,
                ValidationRequirement = target.ValidationRequirement
            });
        }

        return records
            .OrderBy(item => EditDrivenReviewPackagePlayableSessionVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => EditDrivenReviewPackagePlayableSessionVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ThenBy(item => item.TargetId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionRowRecord> BuildRows(
        EditDrivenReviewPackageIndex? packageIndex,
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionTargetRecord> targets)
    {
        if (packageIndex is null)
        {
            return [];
        }

        var targetsByRow = targets
            .GroupBy(target => target.RowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        return packageIndex.Rows
            .OrderBy(row => EditDrivenReviewPackagePlayableSessionVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenReviewPackagePlayableSessionVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                targetsByRow.TryGetValue(row.RowId, out var rowTargets);
                return new EditDrivenReviewPackagePlayableSessionRowRecord
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    ProfileId = row.FamilyId + "/" + row.SeedId,
                    Targets = (rowTargets ?? [])
                        .OrderBy(target => target.TargetId, StringComparer.Ordinal)
                        .ToList()
                };
            })
            .ToList();
    }

    private static bool ValidatePackageIndexTargets(
        EditDrivenReviewPackageIndex? packageIndex,
        IReadOnlyList<EditDrivenReviewPackageFileEntry> ledgerFiles,
        ICollection<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics)
    {
        if (packageIndex is null)
        {
            return false;
        }

        var ledgerByPath = ledgerFiles.ToDictionary(item => item.RelativePath, StringComparer.Ordinal);
        var valid = true;
        foreach (var target in packageIndex.Rows.SelectMany(row => row.Targets))
        {
            if (!ledgerByPath.TryGetValue(target.RelativePath, out var ledgerEntry))
            {
                valid = false;
                diagnostics.Add(Error(
                    "goal078.read.package_index_target_not_in_ledger",
                    target.RelativePath,
                    "Package index references a target file outside the package ledger."));
                continue;
            }

            if (ledgerEntry.TargetId != target.TargetId || ledgerEntry.Sha256 != target.Sha256)
            {
                valid = false;
                diagnostics.Add(Error(
                    "goal078.read.package_index_target_ledger_mismatch",
                    target.RelativePath,
                    "Package index target id/hash must match the package ledger."));
            }
        }

        return valid;
    }

    private static bool ValidatePlayerIndexTargets(
        EditDrivenPlayerReadablePackageIndex? playerIndex,
        EditDrivenReviewPackageIndex? packageIndex,
        IReadOnlyList<EditDrivenReviewPackageFileEntry> ledgerFiles,
        ICollection<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics)
    {
        if (playerIndex is null || packageIndex is null)
        {
            return false;
        }

        var ledgerPaths = ledgerFiles.Select(item => item.RelativePath).ToHashSet(StringComparer.Ordinal);
        var rowIds = packageIndex.Rows.Select(row => row.RowId).ToHashSet(StringComparer.Ordinal);
        var targetIds = packageIndex.Rows
            .SelectMany(row => row.Targets)
            .Select(target => target.TargetId)
            .ToHashSet(StringComparer.Ordinal);
        var valid = true;
        foreach (var scenario in playerIndex.Scenarios)
        {
            if (!rowIds.Contains(scenario.RowId))
            {
                valid = false;
                diagnostics.Add(Error(
                    "goal078.read.player_index_missing_row",
                    scenario.ScenarioId,
                    "Player-readable index references a row outside the package index."));
            }

            foreach (var targetId in scenario.TargetIds)
            {
                if (!targetIds.Contains(targetId))
                {
                    valid = false;
                    diagnostics.Add(Error(
                        "goal078.read.player_index_missing_target",
                        scenario.ScenarioId,
                        "Player-readable index references a target outside the package index."));
                }
            }

            foreach (var targetRef in scenario.TargetFileRefs)
            {
                if (!ledgerPaths.Contains(targetRef))
                {
                    valid = false;
                    diagnostics.Add(Error(
                        "goal078.read.player_index_target_ref_not_in_ledger",
                        targetRef,
                        "Player-readable target file ref must exist in the package ledger."));
                }
            }
        }

        return valid;
    }

    private static bool RequiredReportFieldsPresent(EditDrivenReviewPackagePlayableSessionReportFields fields) =>
        !string.IsNullOrWhiteSpace(fields.ImplementationStatus)
        && !string.IsNullOrWhiteSpace(fields.Accepted)
        && !string.IsNullOrWhiteSpace(fields.SourceGoal076ReportHash)
        && !string.IsNullOrWhiteSpace(fields.SourceGoal076ManifestHash)
        && !string.IsNullOrWhiteSpace(fields.ReviewPackageManifestHash)
        && !string.IsNullOrWhiteSpace(fields.PackageFileLedgerHash)
        && !string.IsNullOrWhiteSpace(fields.PlayerReadablePackageIndexHash)
        && !string.IsNullOrWhiteSpace(fields.ReportHash);

    private static EditDrivenReviewPackagePlayableSessionReportFields ParseReportFields(string report)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in report.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var key = line.Substring(2, separator - 2).Trim();
            var value = line[(separator + 1)..].Trim();
            values[key] = value;
        }

        return new EditDrivenReviewPackagePlayableSessionReportFields
        {
            ImplementationStatus = values.GetValueOrDefault("implementationStatus", string.Empty),
            Accepted = values.GetValueOrDefault("accepted", string.Empty),
            SourceGoal076ReportHash = values.GetValueOrDefault("sourceGoal076ReportHash", string.Empty),
            SourceGoal076ManifestHash = values.GetValueOrDefault("sourceGoal076ManifestHash", string.Empty),
            ReviewPackageManifestHash = values.GetValueOrDefault("reviewPackageManifestHash", string.Empty),
            PackageFileLedgerHash = values.GetValueOrDefault("packageFileLedgerHash", string.Empty),
            PlayerReadablePackageIndexHash = values.GetValueOrDefault("playerReadablePackageIndexHash", string.Empty),
            ReportHash = values.GetValueOrDefault("reportHash", string.Empty)
        };
    }

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n') : string.Empty;
    }

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, path);
        return path;
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static string Serialize<T>(T value) =>
        EditDrivenReviewPackagePlayableSessionHash.Serialize(value);

    private static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : EditDrivenReviewPackagePlayableSessionHash.Deserialize<T>(json);

    private static string Hash(string text) =>
        string.IsNullOrEmpty(text) ? string.Empty : EditDrivenReviewPackagePlayableSessionHash.Sha256(text);

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics) =>
        EditDrivenReviewPackagePlayableSessionQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenReviewPackagePlayableSessionDiagnostic Error(string code, string target, string message) =>
        EditDrivenReviewPackagePlayableSessionDiagnostic.Error(code, target, message);
}

internal sealed record EditDrivenReviewPackagePlayableSessionReadContext(
    EditDrivenReviewPackagePlayableSessionPackageReadProof PackageReadProof,
    string ReportMarkdown,
    EditDrivenReviewPackagePlayableSessionReportFields ReportFields,
    EditDrivenReviewPackageFileLedger PackageFileLedger,
    EditDrivenReviewPackageManifest ReviewPackageManifest,
    EditDrivenReviewPackageIndex PackageIndex,
    EditDrivenPlayerReadablePackageIndex PlayerReadableIndex,
    IReadOnlyList<EditDrivenReviewPackagePlayableSessionRowRecord> Rows,
    IReadOnlyList<EditDrivenReviewPackagePlayableSessionTargetRecord> Targets,
    IReadOnlyDictionary<string, string> ReviewPackagePayloads);
