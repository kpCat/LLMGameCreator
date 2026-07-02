using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

public static class EditDrivenGamePackageRuntimePreviewPlaythroughReportRenderer
{
    public static string Render(EditDrivenGamePackageRuntimePreviewPlaythroughReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Goal 081 Edit-Driven GamePackage Runtime Preview Playthrough");
        builder.AppendLine();
        builder.AppendLine("- gate: " + report.ManualGate + " required");
        builder.AppendLine("- accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- goal080AcceptedByHandoff: " + report.Goal080AcceptedByHandoff);
        builder.AppendLine("- rowCount: " + report.RowCount);
        builder.AppendLine("- targetCount: " + report.TargetCount);
        builder.AppendLine("- goal078ActionCount: " + report.Goal078ActionCount);
        builder.AppendLine("- commandCount: " + report.CommandCount);
        builder.AppendLine("- goal080ReportHash: " + report.Goal080ReportHash);
        builder.AppendLine("- projectedPackageHash: " + report.ProjectedPackageHash);
        builder.AppendLine("- initialPackageReadStateHash: " + report.InitialPackageReadStateHash);
        builder.AppendLine("- finalCoverageStateHash: " + report.FinalCoverageStateHash);
        builder.AppendLine("- replayFinalStateHash: " + report.ReplayFinalStateHash);
        builder.AppendLine("- packageReadProofHash: " + report.PackageReadProofHash);
        builder.AppendLine("- commandScriptHash: " + report.CommandScriptHash);
        builder.AppendLine("- transcriptHash: " + report.TranscriptHash);
        builder.AppendLine("- stateHashChainHash: " + report.StateHashChainHash);
        builder.AppendLine("- coverageLedgerHash: " + report.CoverageLedgerHash);
        builder.AppendLine("- negativeProofHash: " + report.NegativeProofHash);
        builder.AppendLine("- winFormsBindingInventoryHash: " + report.WinFormsBindingInventoryHash);
        builder.AppendLine("- qualityGateScanHash: " + report.QualityGateScanHash);
        builder.AppendLine("- sourceArtifactManifestHash: " + report.SourceArtifactManifestHash);
        builder.AppendLine("- deterministicHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Disposition");
        builder.AppendLine();
        builder.AppendLine(
            "Goal081 consumes the real Goal080 disk-backed projected GamePackage, builds a deterministic package-driven runtime-preview playthrough script, replays it with a state-hash chain, proves all Goal077 targets and Goal078 actions are covered, and leaves the manual gate required for review.");
        builder.AppendLine();
        builder.AppendLine("## Required Artifacts");
        foreach (var artifact in EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.RequiredArtifactFileNames)
        {
            builder.AppendLine("- " + artifact);
        }

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
                builder.AppendLine(
                    "- " + diagnostic.Severity + " " + diagnostic.Code + ": " + diagnostic.Target + " - " + diagnostic.Message);
            }
        }

        builder.AppendLine();
        builder.AppendLine(report.ManualGate + " required");
        return builder.ToString();
    }
}
