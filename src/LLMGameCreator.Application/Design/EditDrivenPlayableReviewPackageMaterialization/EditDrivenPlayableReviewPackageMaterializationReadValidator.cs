namespace LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

internal static class EditDrivenPlayableReviewPackageMaterializationReadValidator
{
    private const string ReviewManifestPath = "review-package/manifest.json";
    private const string PackageIndexPath = "review-package/package-index.json";
    private const string PlayerIndexPath = "review-package/player-readable-index.json";

    public static EditDrivenReviewPackageStagedReadProof ValidateReviewPackagePayloads(
        IReadOnlyDictionary<string, string> payloads,
        EditDrivenReviewPackageFileLedger ledger,
        string expectedSourceReportHash,
        string expectedSourceManifestHash)
    {
        var diagnostics = new List<EditDrivenPlayableReviewPackageDiagnostic>();
        var manifest = ReadPayload<EditDrivenReviewPackageManifest>(payloads, ReviewManifestPath, diagnostics);
        var packageIndex = ReadPayload<EditDrivenReviewPackageIndex>(payloads, PackageIndexPath, diagnostics);
        var playerIndex = ReadPayload<EditDrivenPlayerReadablePackageIndex>(payloads, PlayerIndexPath, diagnostics);
        var allLedgerFilesExist = true;
        var allFileHashesMatch = true;

        foreach (var entry in ledger.Files)
        {
            if (!payloads.TryGetValue(entry.RelativePath, out var payload))
            {
                allLedgerFilesExist = false;
                diagnostics.Add(Error("goal077.read.ledger_file_missing", entry.RelativePath, "Ledger file is missing."));
                continue;
            }

            var actualHash = Hash(payload);
            if (!string.Equals(actualHash, entry.Sha256, StringComparison.Ordinal))
            {
                allFileHashesMatch = false;
                diagnostics.Add(Error("goal077.read.ledger_hash_mismatch", entry.RelativePath, "Ledger hash does not match file payload."));
            }
        }

        var targetFiles = payloads
            .Where(item => item.Key.StartsWith("review-package/targets/", StringComparison.Ordinal))
            .Select(item => Deserialize<EditDrivenReviewPackageTargetFile>(item.Value))
            .Where(item => item is not null)
            .Cast<EditDrivenReviewPackageTargetFile>()
            .ToList();
        var expectedRowsPresent = packageIndex?.Rows.Count == 9
            && targetFiles.Select(item => item.SourceRowId).Distinct(StringComparer.Ordinal).Count() == 9;
        var expectedTargetsPresent = packageIndex?.TargetCount == 18 && targetFiles.Count == 18;
        var sourceHashesMatch = manifest?.SourceGoal076ReportHash == expectedSourceReportHash
            && manifest.SourceGoal076ManifestHash == expectedSourceManifestHash
            && playerIndex?.SourceGoal076ReportHash == expectedSourceReportHash
            && targetFiles.All(item => item.SourceGoal076ReportHash == expectedSourceReportHash);
        var lineageValid = targetFiles.Count > 0
            && targetFiles.All(item => item.BeforeHash == item.RollbackHash && item.AfterHash == item.ReplayHash);

        if (!ValidatePlayerIndexReferences(playerIndex, packageIndex, out var playerDiagnostics))
        {
            diagnostics.AddRange(playerDiagnostics);
        }

        return BuildReadProof(
            manifestExists: manifest is not null,
            packageIndexExists: packageIndex is not null,
            playerIndexExists: playerIndex is not null,
            allLedgerFilesExist,
            allFileHashesMatch,
            expectedRowsPresent,
            expectedTargetsPresent,
            sourceHashesMatch,
            lineageValid,
            rowCount: packageIndex?.RowCount ?? 0,
            targetCount: packageIndex?.TargetCount ?? 0,
            diagnostics: diagnostics);
    }

    public static EditDrivenReviewPackageNegativeProof BuildNegativeProof(
        IReadOnlyDictionary<string, string> reviewPackageFiles,
        EditDrivenReviewPackageFileLedger ledger,
        EditDrivenReviewPackageSourceArtifactManifest source,
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets,
        EditDrivenPlayerReadablePackageIndex playerIndex)
    {
        var firstTargetPath = TargetRelativePath(targets.First());
        var missing = Copy(reviewPackageFiles);
        missing.Remove(firstTargetPath);
        var missingProof = ValidateReviewPackagePayloads(
            missing,
            ledger,
            source.SourceGoal076ReportHash,
            source.SourceGoal076ManifestHash);

        var tampered = Copy(reviewPackageFiles);
        tampered[firstTargetPath] = tampered[firstTargetPath].Replace("\"afterValue\":", "\"afterValueTampered\":", StringComparison.Ordinal);
        var tamperedProof = ValidateReviewPackagePayloads(
            tampered,
            ledger,
            source.SourceGoal076ReportHash,
            source.SourceGoal076ManifestHash);

        var brokenPlayer = Copy(reviewPackageFiles);
        var brokenScenario = playerIndex.Scenarios.First() with
        {
            TargetIds = ["missing-target"],
            TargetFileRefs = ["review-package/targets/missing-target.json"],
            PlayerMarkers =
            [
                new EditDrivenPlayerMarkerReference
                {
                    Marker = "edit_driven_review_package_missing_target=true",
                    RowId = "missing-row",
                    TargetIds = ["missing-target"]
                }
            ]
        };
        var brokenIndex = playerIndex with { Scenarios = [brokenScenario, .. playerIndex.Scenarios.Skip(1)] };
        brokenPlayer[PlayerIndexPath] = Serialize(brokenIndex);
        var brokenPlayerProof = ValidateReviewPackagePayloads(
            brokenPlayer,
            ledger,
            source.SourceGoal076ReportHash,
            source.SourceGoal076ManifestHash);

        var scenarios = new[]
            {
                Scenario("missing_package_target_file", missingProof),
                Scenario("tampered_package_target_file", tamperedProof),
                Scenario("player_index_missing_row_or_target", brokenPlayerProof)
            }
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new EditDrivenReviewPackageNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected")
                && scenarios.Count == EditDrivenPlayableReviewPackageMaterializationVocabulary.RequiredNegativeScenarioIds.Count,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    public static EditDrivenReviewPackageStagedReadProof BuildReadProof(
        bool manifestExists,
        bool packageIndexExists,
        bool playerIndexExists,
        bool allLedgerFilesExist,
        bool allFileHashesMatch,
        bool allExpectedRowsPresent,
        bool allExpectedTargetsPresent,
        bool sourceGoal076HashesMatch,
        bool stateLineageValid,
        int rowCount,
        int targetCount,
        IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> diagnostics) =>
        new()
        {
            Passed = manifestExists
                && packageIndexExists
                && playerIndexExists
                && allLedgerFilesExist
                && allFileHashesMatch
                && allExpectedRowsPresent
                && allExpectedTargetsPresent
                && sourceGoal076HashesMatch
                && stateLineageValid
                && diagnostics.Count == 0,
            ManifestExists = manifestExists,
            PackageIndexExists = packageIndexExists,
            PlayerReadableIndexExists = playerIndexExists,
            AllLedgerFilesExist = allLedgerFilesExist,
            AllFileHashesMatch = allFileHashesMatch,
            AllExpectedRowsPresent = allExpectedRowsPresent,
            AllExpectedTargetsPresent = allExpectedTargetsPresent,
            SourceGoal076HashesMatch = sourceGoal076HashesMatch,
            StateLineageValid = stateLineageValid,
            RowCount = rowCount,
            TargetCount = targetCount,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static bool ValidatePlayerIndexReferences(
        EditDrivenPlayerReadablePackageIndex? playerIndex,
        EditDrivenReviewPackageIndex? packageIndex,
        out IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> diagnostics)
    {
        var list = new List<EditDrivenPlayableReviewPackageDiagnostic>();
        if (playerIndex is null || packageIndex is null)
        {
            diagnostics = list;
            return false;
        }

        var rowIds = packageIndex.Rows.Select(row => row.RowId).ToHashSet(StringComparer.Ordinal);
        var targetIds = packageIndex.Rows.SelectMany(row => row.Targets).Select(target => target.TargetId).ToHashSet(StringComparer.Ordinal);
        foreach (var scenario in playerIndex.Scenarios)
        {
            if (!rowIds.Contains(scenario.RowId))
            {
                list.Add(Error("goal077.read.player_index_missing_row", scenario.ScenarioId, "Player index references a missing row."));
            }

            foreach (var targetId in scenario.TargetIds)
            {
                if (!targetIds.Contains(targetId))
                {
                    list.Add(Error("goal077.read.player_index_missing_target", scenario.ScenarioId, "Player index references a missing target."));
                }
            }

            foreach (var marker in scenario.PlayerMarkers)
            {
                if (!rowIds.Contains(marker.RowId) || marker.TargetIds.Any(id => !targetIds.Contains(id)))
                {
                    list.Add(Error("goal077.read.player_marker_missing_target", marker.Marker, "Player marker does not resolve to an existing row/target."));
                }
            }
        }

        diagnostics = list;
        return list.Count == 0;
    }

    private static T? ReadPayload<T>(
        IReadOnlyDictionary<string, string> payloads,
        string relativePath,
        ICollection<EditDrivenPlayableReviewPackageDiagnostic> diagnostics)
    {
        if (!payloads.TryGetValue(relativePath, out var json))
        {
            diagnostics.Add(Error("goal077.read.required_file_missing", relativePath, "Required review package file is missing."));
            return default;
        }

        try
        {
            return Deserialize<T>(json);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Text.Json.JsonException)
        {
            diagnostics.Add(Error("goal077.read.invalid_json", relativePath, ex.Message));
            return default;
        }
    }

    private static EditDrivenReviewPackageNegativeScenario Scenario(
        string scenarioId,
        EditDrivenReviewPackageStagedReadProof proof) =>
        new()
        {
            ScenarioId = scenarioId,
            ActualStatus = proof.Passed ? "accepted" : "rejected",
            Diagnostics = proof.Diagnostics
        };

    private static SortedDictionary<string, string> Copy(IReadOnlyDictionary<string, string> values)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            result[value.Key] = value.Value;
        }

        return result;
    }

    private static string TargetRelativePath(EditDrivenReviewPackageTargetFile target) =>
        "review-package/targets/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.FamilyId) + "/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.SourceRowId) + "/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.TargetId + "-" + target.DomainId + "-" + target.FieldId)
        + ".json";

    private static string Serialize<T>(T value) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Serialize(value);

    private static T? Deserialize<T>(string json) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Deserialize<T>(json);

    private static string Hash(string text) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Sha256(text);

    private static IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenPlayableReviewPackageDiagnostic> diagnostics) =>
        EditDrivenPlayableReviewPackageMaterializationQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenPlayableReviewPackageDiagnostic Error(string code, string target, string message) =>
        EditDrivenPlayableReviewPackageDiagnostic.Error(code, target, message);
}
