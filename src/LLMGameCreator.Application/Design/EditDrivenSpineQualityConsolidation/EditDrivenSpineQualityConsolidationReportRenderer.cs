namespace LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

internal static class EditDrivenSpineQualityConsolidationReportRenderer
{
    public static string Render(
        EditDrivenSpineQualityConsolidationReport report,
        EditDrivenSpineQualityConsolidationChainManifest chain,
        EditDrivenSpineQualityConsolidationReadinessDashboard dashboard,
        EditDrivenSpineQualityConsolidationDebtClassification debt,
        EditDrivenSpineQualityConsolidationSourceHealthScan sourceHealth)
    {
        var lines = new List<string>
        {
            "# Goal 079 Edit-Driven Spine Quality Consolidation",
            string.Empty,
            "- gate: " + EditDrivenSpineQualityConsolidationVocabulary.FinalGate + " required",
            "- accepted: false",
            "- implementationStatus: " + report.ImplementationStatus,
            "- goal078HandoffRecordedBeforeGoal079: " + report.Goal078AcceptedByUserHandoff,
            "- chainItemCount: " + report.ChainItemCount,
            "- blockerCount: " + report.BlockerCount,
            "- p0Count: " + report.P0Count,
            "- p1Count: " + report.P1Count,
            "- p2Count: " + report.P2Count,
            "- p3Count: " + report.P3Count,
            "- parentWorkspaceLineCount: " + report.ParentWorkspaceLineCount,
            "- maxCSharpLineLength: " + report.MaxCSharpLineLength,
            "- logicalMaxLineLength: " + report.LogicalMaxLineLength,
            "- zeroLfSourceFileCount: " + report.ZeroLfSourceFileCount,
            "- crOnlySourceFileCount: " + report.CrOnlySourceFileCount,
            "- rawPhysicalMaxLineLength: " + report.RawPhysicalMaxLineLength,
            "- rawPhysicalOneLineSourceFileCount: " + report.RawPhysicalOneLineSourceFileCount,
            "- minifiedSourceFileCount: " + report.MinifiedSourceFileCount,
            "- filesOver1000LinesCount: " + report.FilesOver1000LinesCount,
            "- alphaRuntimeBootstrapLineCount: " + report.AlphaRuntimeBootstrapLineCount,
            "- alphaRuntimeBootstrapHash: " + report.AlphaRuntimeBootstrapHash,
            "- sourceArtifactManifestHash: " + report.SourceArtifactManifestHash,
            "- spineChainManifestHash: " + report.SpineChainManifestHash,
            "- acceptanceReadinessDashboardHash: " + report.AcceptanceReadinessDashboardHash,
            "- negativeProofIndexHash: " + report.NegativeProofIndexHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceHealthScanHash: " + report.SourceHealthScanHash,
            "- qualityDebtClassificationHash: " + report.QualityDebtClassificationHash,
            "- artifactHygieneScanHash: " + report.ArtifactHygieneScanHash,
            "- qualityGateScanHash: " + report.QualityGateScanHash,
            "- reportHash: " + report.DeterministicHash,
            string.Empty,
            "## Consumed Report Hashes"
        };
        lines.AddRange(chain.ChainItems.Select(item =>
            "- Goal " + item.GoalNumber + ": " + item.ReportHash
            + " declared=" + item.DeclaredReportHash));
        lines.AddRange(
        [
            string.Empty,
            "## Readiness",
            "- packageReadProofPassed: " + dashboard.PackageReadProofPassed,
            "- replayProofPassed: " + dashboard.ReplayProofPassed,
            "- replayFinalHashMatchesOriginal: " + dashboard.ReplayFinalHashMatchesOriginal,
            "- negativeProofPassed: " + dashboard.NegativeProofPassed,
            string.Empty,
            "## Source Health",
            "- scannedFileCount: " + sourceHealth.ScannedFileCount,
            "- zeroLfSourceFileCount: " + sourceHealth.ZeroLfSourceFileCount,
            "- crOnlySourceFileCount: " + sourceHealth.CrOnlySourceFileCount,
            "- rawPhysicalMaxLineLength: " + sourceHealth.RawPhysicalMaxLineLength,
            "- rawPhysicalOneLineSourceFileCount: " + sourceHealth.RawPhysicalOneLineSourceFileCount,
            "- logicalMaxLineLength: " + sourceHealth.LogicalMaxLineLength,
            "- parentWorkspaceWithinLimit: " + sourceHealth.ParentWorkspaceWithinLimit,
            "- alphaRuntimeBootstrapUnchanged: " + sourceHealth.AlphaRuntimeBootstrapUnchanged,
            string.Empty,
            "## Remaining Debt"
        ]);
        lines.AddRange(debt.Debts.Select(item =>
            "- " + item.Severity + " " + item.FindingId + ": " + item.Evidence));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- none"]
            : report.Diagnostics.Select(item => "- " + item.Severity + ": " + item.Code + " [" + item.Target + "]"));
        lines.Add(string.Empty);
        lines.Add(EditDrivenSpineQualityConsolidationVocabulary.FinalGate + " required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
