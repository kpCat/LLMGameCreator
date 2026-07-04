using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldUnityEditorPreviewTool;

public sealed class OfflineGeoworldUnityEditorPreviewToolTests
{
    [Fact]
    public async Task ServiceBuildsEditorToolReadinessEvidenceFromGoal101Payload()
    {
        await Task.CompletedTask;
        var result = new OfflineGeoworldUnityEditorPreviewToolEvidenceService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldUnityEditorPreviewToolVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.ToolInventory.Passed);
        Assert.True(result.SimulatedActionProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.Equal(18, result.SimulatedActionProof.CommandCount);
        Assert.Equal(10, result.SimulatedActionProof.CommandKindCount);
        Assert.True(result.SimulatedActionProof.TravelWindowStepCount >= 4);
        Assert.Equal(18, result.SimulatedActionProof.ExpectedObjectCount);
        Assert.Equal(18, result.SimulatedActionProof.ClearOperationRemovedObjectCount);
        Assert.True(result.ToolInventory.MenuItemMarkerPresent);
        Assert.True(result.ToolInventory.StreamingAssetsPathMarkerPresent);
        Assert.True(result.ToolInventory.Goal101PayloadPathMarkerPresent);
        Assert.True(result.ToolInventory.CreatePreviewObjectsMethodPresent);
        Assert.True(result.ToolInventory.ClearPreviewObjectsMethodPresent);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);

        foreach (var fileName in OfflineGeoworldUnityEditorPreviewToolVocabulary.RequiredEvidenceFileNames)
        {
            if (fileName == OfflineGeoworldUnityEditorPreviewToolVocabulary.ReportMarkdownFileName)
            {
                Assert.False(string.IsNullOrWhiteSpace(result.ReportMarkdown));
                continue;
            }

            Assert.True(result.EvidenceJsonByFileName.ContainsKey(fileName), fileName);
        }
    }

    [Fact]
    public async Task SimulatedActionProofAndNegativeProofCoverRequiredRisks()
    {
        await Task.CompletedTask;
        var result = new OfflineGeoworldUnityEditorPreviewToolEvidenceService()
            .Build(ProjectRoot());

        Assert.True(result.SimulatedActionProof.PayloadReadAttempted);
        Assert.True(result.SimulatedActionProof.ManifestRead);
        Assert.True(result.SimulatedActionProof.CommandCatalogRead);
        Assert.True(result.SimulatedActionProof.TravelWindowScriptRead);
        Assert.True(result.SimulatedActionProof.EditorWindowScriptRead);
        Assert.True(result.SimulatedActionProof.PayloadCountsMatchGoal101);
        Assert.True(result.SimulatedActionProof.AllRequiredCommandKindsRepresented);
        Assert.True(result.SimulatedActionProof.NoUnsupportedCommandKind);
        Assert.True(result.SimulatedActionProof.PreviewObjectPlanBuilt);
        Assert.True(result.SimulatedActionProof.CreateOperationModelPassed);
        Assert.True(result.SimulatedActionProof.ClearOperationModelPassed);
        Assert.True(result.SimulatedActionProof.NoAbsolutePaths);
        Assert.True(result.SimulatedActionProof.NoRawGeodata);
        Assert.True(result.SimulatedActionProof.NoBinaryOrRasterMedia);
        Assert.True(result.SimulatedActionProof.NoProviderOrNetworkMarkers);
        Assert.True(result.SimulatedActionProof.NoScenePrefabSettingsChangeMarkers);
        Assert.All(result.SimulatedActionProof.PreviewObjects, item =>
        {
            Assert.True(item.MetadataOnly, item.CommandId);
            Assert.Equal(1, item.ExpectedObjectCount);
            Assert.StartsWith("editor_preview_", item.ObjectName, StringComparison.Ordinal);
        });

        Assert.Equal(
            OfflineGeoworldUnityEditorPreviewToolVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        foreach (var scenarioId in OfflineGeoworldUnityEditorPreviewToolVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesEditorToolReadiness()
    {
        await Task.CompletedTask;
        new OfflineGeoworldUnityEditorPreviewToolEvidenceService()
            .Build(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_unity_editor_preview");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_unity_editor_preview_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewGroupPresent);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewCommandCount);
        Assert.Equal(10, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewCommandKindCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount >= 4);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewExpectedObjectCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewToolInventoryPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal102FilesDiscoveredByRelativePaths);
        Assert.Equal(18, summary.OfflineGeoworldUnityEditorPreviewCommandCount);
        Assert.Contains(
            "LLMGameCreator/Offline Geoworld Preview",
            summary.OfflineGeoworldUnityEditorPreviewMenuItemMarker,
            StringComparison.Ordinal);
        Assert.Contains(
            "OfflineGeoworldGoal101",
            summary.OfflineGeoworldUnityEditorPreviewPayloadPath,
            StringComparison.Ordinal);
        AssertProofPassed(result.ProofStatus.Proofs, "goal102.tool_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal102.simulated_action");
        AssertProofPassed(result.ProofStatus.Proofs, "goal102.clear_operation");
        AssertProofPassed(result.ProofStatus.Proofs, "goal102.quality_gate");
    }

    [Fact]
    public async Task EvidenceIsDeterministicAndSourceHealthClean()
    {
        var service = new OfflineGeoworldUnityEditorPreviewToolEvidenceService();
        await Task.CompletedTask;
        var first = service.Build(ProjectRoot());
        var second = service.Build(ProjectRoot());

        Assert.Equal(first.EvidenceJsonByFileName, second.EvidenceJsonByFileName);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.True(second.QualityGateScan.Passed);
        Assert.Equal(0, second.QualityGateScan.FilesOver1000LogicalLinesCount);
        Assert.Equal(0, second.QualityGateScan.FilesOver700LogicalLinesCount);
        Assert.True(second.QualityGateScan.MaxLogicalLineCount <= 700);
    }

    [Fact]
    public void SourceFormatGuardRejectsSyntheticOneLineCSharp()
    {
        var bytes = Encoding.UTF8.GetBytes(
            "using System; using UnityEngine; namespace Broken; public sealed class BrokenWindow { public void A() { } public void B() { } }");
        var scan = OfflineGeoworldUnityEditorSourceFormatGuardScanner.AnalyzeSourceBytes(
            "synthetic/BrokenWindow.cs",
            bytes);

        Assert.True(scan.ZeroLfSource);
        Assert.True(scan.OnePhysicalLineMultiStatementSource);
        Assert.True(scan.MinifiedSourceCandidate);
        Assert.False(scan.Passed);
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(bytes));
    }

    [Fact]
    public void SourceFormatGuardRejectsZeroLfCrOnlyAndExtremeLine()
    {
        var zeroLf = Encoding.UTF8.GetBytes("public sealed class BrokenZeroLf { public void Run() { } }");
        var crOnly = Encoding.UTF8.GetBytes("public sealed class BrokenCrOnly\r{\r    public void Run() { }\r}\r");
        var extreme = Encoding.UTF8.GetBytes(
            "namespace Broken;\npublic sealed class ExtremeLine\n{\n    private const string Value = \""
            + new string('x', 520)
            + "\";\n}\n");

        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner
            .AnalyzeSourceBytes("synthetic/ZeroLf.cs", zeroLf)
            .ZeroLfSource);
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner
            .AnalyzeSourceBytes("synthetic/CrOnly.cs", crOnly)
            .CrOnlySource);
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner
            .AnalyzeSourceBytes("synthetic/Extreme.cs", extreme)
            .RawPhysicalLinesOver500Count > 0);
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(zeroLf));
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(crOnly));
        Assert.True(OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(extreme));
    }

    [Fact]
    public void SourceFormatGuardAcceptsRepairedEditorWindow()
    {
        var root = ProjectRoot();
        var scan = OfflineGeoworldUnityEditorSourceFormatGuardScanner.AnalyzeSourceFile(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath);

        Assert.True(scan.Passed);
        Assert.True(scan.LfByteCount > 0);
        Assert.True(scan.RawPhysicalLineCount > 1);
        Assert.True(scan.RawPhysicalMaxLineLength <= OfflineGeoworldUnityEditorSourceFormatGuardScanner.MaxAllowedPhysicalLineLength);
        Assert.False(scan.ZeroLfSource);
        Assert.False(scan.CrOnlySource);
        Assert.False(scan.OnePhysicalLineMultiStatementSource);
        Assert.False(scan.MinifiedSourceCandidate);
    }

    [Fact]
    public async Task SourceFormatGuardEvidenceIsDeterministicAndKeepsAlphaRuntimeBootstrapUnchanged()
    {
        var service = new OfflineGeoworldUnityEditorSourceFormatGuardEvidenceService();
        var first = await service.BuildAndWriteAsync(ProjectRoot());
        var second = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal(first.Result.ScanBeforeAfterJson, second.Result.ScanBeforeAfterJson);
        Assert.Equal(first.Result.NegativeProofJson, second.Result.NegativeProofJson);
        Assert.Equal(first.Result.QualityGateJson, second.Result.QualityGateJson);
        Assert.Equal(first.Result.Report.DeterministicReportHash, second.Result.Report.DeterministicReportHash);
        Assert.Equal("GREEN", second.Result.Report.ImplementationStatus);
        Assert.False(second.Result.Report.Accepted);
        Assert.Equal(
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.FinalGate,
            second.Result.Report.ManualGate);
        Assert.True(second.Result.BeforeAfter.BeforeEditorWindowMalformedDetected);
        Assert.True(second.Result.BeforeAfter.AfterEditorWindowRepaired);
        Assert.True(second.Result.BeforeAfter.AlphaRuntimeBootstrap.Unchanged);
        Assert.True(second.Result.NegativeProof.Passed);
        Assert.True(second.Result.QualityGate.Passed);
        Assert.Equal(0, second.Result.QualityGate.ZeroLfSourceFileCount);
        Assert.Equal(0, second.Result.QualityGate.CrOnlySourceFileCount);
        Assert.Equal(0, second.Result.QualityGate.RawPhysicalOneLineSourceFileCount);
        Assert.Equal(0, second.Result.QualityGate.MinifiedSourceFileCount);

        foreach (var fileName in OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(second.OutputDirectoryPath, fileName)), fileName);
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
