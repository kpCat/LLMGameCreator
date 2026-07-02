using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenPlayableReviewPackageMaterializationProductSmokeTests
{
    [Fact]
    public async Task Goal077EditDrivenPlayableReviewPackageMaterializationReadsDiskBackedPackage()
    {
        var service = new EditDrivenPlayableReviewPackageMaterializationEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal076AcceptedByUserHandoff);
        Assert.True(result.StagedPackageReadProof.Passed);
        Assert.True(result.TamperNegativeProof.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate, result.Report.ManualGate);
        Assert.Equal(9, result.Report.RowCount);
        Assert.Equal(18, result.Report.TargetCount);
        Assert.Equal(21, result.Report.ReviewPackageFileCount);

        foreach (var fileName in EditDrivenPlayableReviewPackageMaterializationEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        var stagedRead = service.ReadStagedReviewPackage(
            write.OutputDirectoryPath,
            result.SourceArtifactManifest.SourceGoal076ReportHash,
            result.SourceArtifactManifest.SourceGoal076ManifestHash);
        Assert.True(stagedRead.Passed);
        Assert.True(stagedRead.AllLedgerFilesExist);
        Assert.True(stagedRead.AllFileHashesMatch);
        Assert.True(stagedRead.AllExpectedRowsPresent);
        Assert.True(stagedRead.AllExpectedTargetsPresent);
        Assert.True(stagedRead.SourceGoal076HashesMatch);
        Assert.True(stagedRead.StateLineageValid);

        var targetEntries = result.PackageFileLedger.Files.Where(file => file.Role == "target").ToList();
        Assert.Equal(18, targetEntries.Count);
        foreach (var entry in result.PackageFileLedger.Files)
        {
            var path = Path.Combine(write.OutputDirectoryPath, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), entry.RelativePath);
            var payload = File.ReadAllText(path).TrimEnd('\r', '\n');
            Assert.Equal(entry.Sha256, EditDrivenPlayableReviewPackageMaterializationHash.Sha256(payload));
        }

        var firstTargetFile = ReadTargetFile(write.OutputDirectoryPath, targetEntries[0].RelativePath);
        Assert.False(string.IsNullOrWhiteSpace(firstTargetFile.SourceRowId));
        Assert.False(string.IsNullOrWhiteSpace(firstTargetFile.LogicalPackagePath));
        Assert.Equal(firstTargetFile.BeforeHash, firstTargetFile.RollbackHash);
        Assert.Equal(firstTargetFile.AfterHash, firstTargetFile.ReplayHash);
        Assert.Equal(
            result.SourceArtifactManifest.SourceGoal076ReportHash,
            firstTargetFile.SourceGoal076ReportHash);

        var missingRoot = CopyOutputToTemp(write.OutputDirectoryPath);
        try
        {
            File.Delete(Path.Combine(missingRoot, targetEntries[0].RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            var missing = service.ReadStagedReviewPackage(
                missingRoot,
                result.SourceArtifactManifest.SourceGoal076ReportHash,
                result.SourceArtifactManifest.SourceGoal076ManifestHash);
            Assert.False(missing.Passed);
        }
        finally
        {
            if (Directory.Exists(missingRoot))
            {
                Directory.Delete(missingRoot, recursive: true);
            }
        }
    }

    private static EditDrivenReviewPackageTargetFile ReadTargetFile(string outputRoot, string relativePath)
    {
        var path = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var payload = File.ReadAllText(path);
        var targetFile = EditDrivenPlayableReviewPackageMaterializationHash
            .Deserialize<EditDrivenReviewPackageTargetFile>(payload);
        Assert.NotNull(targetFile);
        return targetFile!;
    }

    private static string CopyOutputToTemp(string sourceRoot)
    {
        var destination = Path.Combine(
            Path.GetTempPath(),
            "llmgc-goal077-product-smoke-" + Guid.NewGuid().ToString("N"));
        foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, sourceFile);
            var destinationFile = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(sourceFile, destinationFile);
        }

        return destination;
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
