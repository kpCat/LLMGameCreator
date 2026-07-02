using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

public static class EditDrivenGamePackageRuntimePreviewBridgeReportRenderer
{
    public static string Render(EditDrivenGamePackageRuntimePreviewBridgeReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Goal 080 Edit-Driven GamePackage Runtime Preview Bridge");
        builder.AppendLine();
        builder.AppendLine($"- gate: {report.ManualGate} required");
        builder.AppendLine($"- accepted: {report.Accepted.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- implementationStatus: {report.ImplementationStatus}");
        builder.AppendLine($"- goal079AcceptedForContinuation: {report.Goal079AcceptedForContinuation}");
        builder.AppendLine($"- goal079ASourceFormatGuardPassedByHandoff: {report.Goal079ASourceFormatGuardPassedByHandoff}");
        builder.AppendLine($"- rowCount: {report.RowCount}");
        builder.AppendLine($"- targetCount: {report.TargetCount}");
        builder.AppendLine($"- actionCount: {report.ActionCount}");
        builder.AppendLine($"- projectedPackageFileCount: {report.ProjectedPackageFileCount}");
        builder.AppendLine($"- sourceGoal077ReportHash: {report.SourceGoal077ReportHash}");
        builder.AppendLine($"- sourceGoal078ReportHash: {report.SourceGoal078ReportHash}");
        builder.AppendLine($"- sourceGoal079ReportHash: {report.SourceGoal079ReportHash}");
        builder.AppendLine($"- sourceGoal079AReportHash: {report.SourceGoal079AReportHash}");
        builder.AppendLine($"- projectedPackageHash: {report.ProjectedPackageHash}");
        builder.AppendLine($"- projectedPackageManifestHash: {report.ProjectedPackageManifestHash}");
        builder.AppendLine($"- projectedPackageFileLedgerHash: {report.ProjectedPackageFileLedgerHash}");
        builder.AppendLine($"- runtimePreviewBridgeProofHash: {report.RuntimePreviewBridgeProofHash}");
        builder.AppendLine($"- runtimePreviewNegativeProofHash: {report.RuntimePreviewNegativeProofHash}");
        builder.AppendLine($"- winFormsBindingInventoryHash: {report.WinFormsBindingInventoryHash}");
        builder.AppendLine($"- qualityGateScanHash: {report.QualityGateScanHash}");
        builder.AppendLine($"- deterministicHash: {report.DeterministicHash}");
        builder.AppendLine();
        builder.AppendLine("## Disposition");
        builder.AppendLine();
        builder.AppendLine(
            "Goal080 projects the Goal077 edit targets and Goal078 playable-session coverage into a disk-backed public GamePackage, reads the projected package back through the existing validator/runtime-preview path, and leaves the manual gate required for review.");
        builder.AppendLine();
        builder.AppendLine("## Required Artifacts");
        foreach (var artifact in EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RequiredArtifactFileNames)
        {
            builder.AppendLine($"- {artifact}");
        }

        builder.AppendLine("- projected-gamepackage/package.json");
        builder.AppendLine("- projected-gamepackage/projected-package-index.json");
        builder.AppendLine("- projected-gamepackage/player-readable-bridge-index.json");
        builder.AppendLine("- projected-gamepackage/source-targets.json");
        builder.AppendLine("- projected-gamepackage/validation-report.json");
        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        if (report.Diagnostics.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var diagnostic in report.Diagnostics)
            {
                builder.AppendLine($"- {diagnostic.Severity} {diagnostic.Code}: {diagnostic.Target} - {diagnostic.Message}");
            }
        }

        builder.AppendLine();
        builder.AppendLine($"{report.ManualGate} required");
        return builder.ToString();
    }
}
