using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaSliceExportPackage;

public sealed class OfflineGeoworldAlphaSliceExportPackageTests
{
    [Fact]
    public async Task BuildAndWriteCreatesPortablePackageEvidenceAndWorkspaceGroup()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAlphaSliceExportPackageEvidenceService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.CleanImportProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Goal108ManifestRead);
        Assert.True(result.SourceLineage.Goal108AImmutabilityAuditRead);
        Assert.True(result.SourceLineage.Goal108AHistoricalDiffAuditRead);
        Assert.Equal(6, result.Manifest.PackageFileCount);
        Assert.Equal(5, result.FileIndex.IndexedFileCount);
        Assert.Equal(5, result.Checksums.HashedFileCount);
        Assert.Equal(7, result.Manifest.SourceComponentCount);
        Assert.Equal(7, result.Manifest.ReadySourceComponentCount);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.False(result.AcceptanceGate.Accepted);
        Assert.True(result.AcceptanceGate.PackageReadyForManualReview);

        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);
        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredEvidenceFileNames);
        AssertFilesExist(write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);
        AssertFilesExist(write.StreamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_export_package");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_export_package_workspace_summary");

        Assert.True(
            workspace.QualityGateScan.Passed,
            string.Join(Environment.NewLine, workspace.QualityGateScan.Diagnostics.Select(item =>
                item.Code + " [" + item.Target + "] " + item.Message)));
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportPackageGroupPresent);
        Assert.Equal(6, workspace.QualityGateScan.OfflineGeoworldAlphaExportPackageFileCount);
        Assert.Equal(5, workspace.QualityGateScan.OfflineGeoworldAlphaExportIndexedFileCount);
        Assert.Equal("matched", workspace.QualityGateScan.OfflineGeoworldAlphaExportChecksumStatus);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportCleanImportProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportNegativeProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportUnityVerifierReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportEditorWindowReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportWorkspaceBindingPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportSourceLineagePassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal109FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.WinFormsOfflineGeoworldAlphaExportPackageBindingReal);
        Assert.Equal(6, summary.OfflineGeoworldAlphaExportPackageFileCount);
        Assert.True(summary.OfflineGeoworldAlphaExportUnityVerifierReady);
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal109.alpha_export.clean_import");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal109.alpha_export.unity_verifier");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal109.alpha_export.quality_gate");
    }

    [Fact]
    public async Task CleanImportVerifierRejectsTamperedAndMissingPackageFiles()
    {
        var root = ProjectRoot();
        var service = new OfflineGeoworldAlphaSliceExportPackageEvidenceService();
        var write = await service.BuildAndWriteAsync(root);

        var tamperedRoot = CopyPackage(write.ExportPackageDirectoryPath);
        try
        {
            File.AppendAllText(
                Path.Combine(tamperedRoot, OfflineGeoworldAlphaSliceExportPackageVocabulary.ReadmeFileName),
                Environment.NewLine + "tampered");

            var tampered = service.VerifyPackage(tamperedRoot);

            Assert.False(tampered.Passed);
            Assert.False(tampered.ChecksumsMatch);
            Assert.Contains(tampered.Diagnostics, item => item.Contains("checksum-mismatch", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(tamperedRoot, recursive: true);
        }

        var missingRoot = CopyPackage(write.ExportPackageDirectoryPath);
        try
        {
            File.Delete(Path.Combine(
                missingRoot,
                OfflineGeoworldAlphaSliceExportPackageVocabulary.ReadmeFileName));

            var missing = service.VerifyPackage(missingRoot);

            Assert.False(missing.Passed);
            Assert.False(missing.AllRequiredFilesPresent);
            Assert.False(missing.AllIndexedFilesPresent);
            Assert.Contains(missing.Diagnostics, item => item.Contains("missing-indexed-file", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(missingRoot, recursive: true);
        }
    }

    [Fact]
    public void NegativeProofCoversRequiredPackageFailureClasses()
    {
        var result = new OfflineGeoworldAlphaSliceExportPackageEvidenceService()
            .Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        Assert.Equal(
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostic.Length > 0);
        }
    }

    private static string CopyPackage(string source)
    {
        var destination = Path.Combine(
            Path.GetTempPath(),
            "llmgc-goal109-package-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(destination);
        foreach (var fileName in OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames)
        {
            File.Copy(Path.Combine(source, fileName), Path.Combine(destination, fileName));
        }

        return destination;
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
    }

    private static void AssertProofPassed(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        string proofId)
    {
        var proof = Assert.Single(proofs, item => item.ProofId == proofId);
        Assert.True(proof.Passed, proof.DiagnosticSummary);
        Assert.False(Path.IsPathFullyQualified(proof.RelativePath), proof.RelativePath);
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
