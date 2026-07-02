using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayableReviewPackageMaterialization;

public sealed class EditDrivenPlayableReviewPackageMaterializationServiceTests
{
    [Fact]
    public async Task ServiceBuildsFromGoal076ArtifactsAndWritesRequiredArtifacts()
    {
        var service = new EditDrivenPlayableReviewPackageMaterializationEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal076AcceptedByUserHandoff);
        Assert.True(result.SourceArtifactManifest.Goal076ReportWasGreenProducedForReview);
        Assert.True(result.SourceArtifactManifest.Goal076ArtifactAcceptedFalse);
        Assert.True(result.ReviewPackageManifest.TargetCount == 18);
        Assert.True(result.PackageFileLedger.Passed);
        Assert.True(result.PlayerReadablePackageIndex.Passed);
        Assert.True(result.PackageTargetCoverage.Passed);
        Assert.True(result.StateLineageProof.Passed);
        Assert.True(result.StagedPackageReadProof.Passed);
        Assert.True(result.TamperNegativeProof.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate, result.Report.ManualGate);
        Assert.Equal(9, result.PackageIndex.RowCount);
        Assert.Equal(18, result.PackageIndex.TargetCount);
        Assert.Equal(21, result.PackageFileLedger.FileCount);
        Assert.Equal(18, result.PackageFileLedger.Files.Count(file => file.Role == "target"));

        foreach (var fileName in EditDrivenPlayableReviewPackageMaterializationEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var file in result.ReviewPackageFiles.Keys)
        {
            var path = Path.Combine(write.OutputDirectoryPath, file.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), file);
        }
    }

    [Fact]
    public async Task PackageLedgerHashesMatchFilesOnDisk()
    {
        var service = new EditDrivenPlayableReviewPackageMaterializationEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        var readProof = service.ReadStagedReviewPackage(
            write.OutputDirectoryPath,
            result.SourceArtifactManifest.SourceGoal076ReportHash,
            result.SourceArtifactManifest.SourceGoal076ManifestHash);

        Assert.True(readProof.Passed);
        Assert.True(readProof.AllLedgerFilesExist);
        Assert.True(readProof.AllFileHashesMatch);
        Assert.True(readProof.AllExpectedRowsPresent);
        Assert.True(readProof.AllExpectedTargetsPresent);
        Assert.True(readProof.SourceGoal076HashesMatch);

        foreach (var entry in result.PackageFileLedger.Files)
        {
            var path = Path.Combine(write.OutputDirectoryPath, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            var payload = File.ReadAllText(path).TrimEnd('\r', '\n');
            Assert.Equal(entry.Sha256, EditDrivenPlayableReviewPackageMaterializationHash.Sha256(payload));
        }
    }

    [Fact]
    public void PlayerIndexMapsScenariosAndMarkersToExistingPackageTargets()
    {
        var result = new EditDrivenPlayableReviewPackageMaterializationEvidenceService().Build(ProjectRoot());

        var rowIds = result.PackageIndex.Rows.Select(row => row.RowId).ToHashSet(StringComparer.Ordinal);
        var targetIds = result.PackageIndex.Rows
            .SelectMany(row => row.Targets)
            .Select(target => target.TargetId)
            .ToHashSet(StringComparer.Ordinal);
        var targetRefs = result.PackageFileLedger.Files
            .Where(file => file.Role == "target")
            .Select(file => file.RelativePath)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(9, result.PlayerReadablePackageIndex.ScenarioCount);
        Assert.True(result.PlayerReadablePackageIndex.AllScenarioIdsMapped);
        Assert.True(result.PlayerReadablePackageIndex.AllPlayerMarkersResolved);
        Assert.True(result.PlayerReadablePackageIndex.AllRowsRepresented);
        Assert.True(result.PlayerReadablePackageIndex.AllTargetsRepresented);

        foreach (var scenario in result.PlayerReadablePackageIndex.Scenarios)
        {
            Assert.Contains(scenario.RowId, rowIds);
            Assert.NotEmpty(scenario.TargetIds);
            Assert.All(scenario.TargetIds, targetId => Assert.Contains(targetId, targetIds));
            Assert.All(scenario.TargetFileRefs, targetRef => Assert.Contains(targetRef, targetRefs));
            Assert.All(
                scenario.PlayerMarkers,
                marker =>
                {
                    Assert.Contains(marker.RowId, rowIds);
                    Assert.All(marker.TargetIds, targetId => Assert.Contains(targetId, targetIds));
                });
        }
    }

    [Fact]
    public void BeforeAfterRollbackReplayLineageIsPreservedInEveryTargetFile()
    {
        var result = new EditDrivenPlayableReviewPackageMaterializationEvidenceService().Build(ProjectRoot());

        Assert.Equal(9, result.StateLineageProof.RowCount);
        Assert.Equal(18, result.StateLineageProof.TargetCount);
        Assert.All(result.StateLineageProof.Rows, row =>
        {
            Assert.True(row.StateChanged);
            Assert.Equal(row.BeforeHash, row.RollbackHash);
            Assert.Equal(row.AfterHash, row.ReplayHash);
            Assert.NotEqual(row.BeforeHash, row.AfterHash);
        });

        var targetPayloads = result.ReviewPackageFiles
            .Where(file => file.Key.StartsWith("review-package/targets/", StringComparison.Ordinal))
            .Select(file => EditDrivenPlayableReviewPackageMaterializationHash
                .Deserialize<EditDrivenReviewPackageTargetFile>(file.Value))
            .OfType<EditDrivenReviewPackageTargetFile>()
            .ToList();

        Assert.Equal(18, targetPayloads.Count);
        Assert.All(targetPayloads, targetFile =>
        {
            Assert.Equal(targetFile.BeforeHash, targetFile.RollbackHash);
            Assert.Equal(targetFile.AfterHash, targetFile.ReplayHash);
            Assert.NotEqual(targetFile.BeforeValue, targetFile.AfterValue);
            Assert.Equal(
                result.SourceArtifactManifest.SourceGoal076ReportHash,
                targetFile.SourceGoal076ReportHash);
        });
    }

    [Fact]
    public async Task StagedReadRejectsMissingTamperedAndBrokenPlayerIndexFiles()
    {
        var service = new EditDrivenPlayableReviewPackageMaterializationEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;
        var targetEntry = Assert.Single(
            result.PackageFileLedger.Files,
            file => file.Role == "target" && file.TargetId == "target-001");

        var missingRoot = CopyOutputToTemp(write.OutputDirectoryPath);
        var tamperedRoot = CopyOutputToTemp(write.OutputDirectoryPath);
        var brokenPlayerRoot = CopyOutputToTemp(write.OutputDirectoryPath);
        try
        {
            File.Delete(Path.Combine(missingRoot, targetEntry.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            var missing = service.ReadStagedReviewPackage(
                missingRoot,
                result.SourceArtifactManifest.SourceGoal076ReportHash,
                result.SourceArtifactManifest.SourceGoal076ManifestHash);
            Assert.False(missing.Passed);
            Assert.Contains(missing.Diagnostics, diagnostic => diagnostic.Code == "goal077.read.ledger_file_missing");

            var tamperedTargetPath = Path.Combine(tamperedRoot, targetEntry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(
                tamperedTargetPath,
                File.ReadAllText(tamperedTargetPath).Replace("\"afterValue\":", "\"afterValueTampered\":", StringComparison.Ordinal));
            var tampered = service.ReadStagedReviewPackage(
                tamperedRoot,
                result.SourceArtifactManifest.SourceGoal076ReportHash,
                result.SourceArtifactManifest.SourceGoal076ManifestHash);
            Assert.False(tampered.Passed);
            Assert.Contains(tampered.Diagnostics, diagnostic => diagnostic.Code == "goal077.read.ledger_hash_mismatch");

            var brokenPlayerIndex = result.PlayerReadablePackageIndex with
            {
                Scenarios =
                [
                    result.PlayerReadablePackageIndex.Scenarios[0] with
                    {
                        RowId = "missing-row",
                        TargetIds = ["missing-target"],
                        TargetFileRefs = ["review-package/targets/missing-target.json"],
                        PlayerMarkers =
                        [
                            new EditDrivenPlayerMarkerReference
                            {
                                Marker = "goal077_missing_target=true",
                                RowId = "missing-row",
                                TargetIds = ["missing-target"]
                            }
                        ]
                    },
                    .. result.PlayerReadablePackageIndex.Scenarios.Skip(1)
                ]
            };
            var playerIndexPath = Path.Combine(
                brokenPlayerRoot,
                "review-package/player-readable-index.json".Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(
                playerIndexPath,
                EditDrivenPlayableReviewPackageMaterializationHash.Serialize(brokenPlayerIndex));
            var brokenPlayer = service.ReadStagedReviewPackage(
                brokenPlayerRoot,
                result.SourceArtifactManifest.SourceGoal076ReportHash,
                result.SourceArtifactManifest.SourceGoal076ManifestHash);
            Assert.False(brokenPlayer.Passed);
            Assert.Contains(brokenPlayer.Diagnostics, diagnostic => diagnostic.Code == "goal077.read.player_index_missing_row");
            Assert.Contains(brokenPlayer.Diagnostics, diagnostic => diagnostic.Code == "goal077.read.player_index_missing_target");
        }
        finally
        {
            DeleteDirectory(missingRoot);
            DeleteDirectory(tamperedRoot);
            DeleteDirectory(brokenPlayerRoot);
        }
    }

    private static string CopyOutputToTemp(string sourceRoot)
    {
        var destination = Path.Combine(
            Path.GetTempPath(),
            "llmgc-goal077-staged-read-" + Guid.NewGuid().ToString("N"));
        foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, sourceFile);
            var destinationFile = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(sourceFile, destinationFile);
        }

        return destination;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
