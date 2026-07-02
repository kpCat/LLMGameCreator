using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

public static class EditDrivenUnityAlphaStreamingAssetsHandoffReportRenderer
{
    public static string Render(EditDrivenUnityAlphaStreamingAssetsHandoffReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Goal 082 Edit-Driven Unity Alpha StreamingAssets Handoff");
        builder.AppendLine();
        builder.AppendLine("- gate: " + report.ManualGate + " required");
        builder.AppendLine("- accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- goal081AcceptedByHandoff: " + report.Goal081AcceptedByHandoff);
        builder.AppendLine("- streamingAssetsRelativeRoot: " + report.StreamingAssetsRelativeRoot);
        builder.AppendLine("- payloadFileCount: " + report.PayloadFileCount);
        builder.AppendLine("- rowCount: " + report.RowCount);
        builder.AppendLine("- targetCount: " + report.TargetCount);
        builder.AppendLine("- goal078ActionCount: " + report.Goal078ActionCount);
        builder.AppendLine("- commandCount: " + report.CommandCount);
        builder.AppendLine("- projectedPackageHash: " + report.ProjectedPackageHash);
        builder.AppendLine("- commandScriptHash: " + report.CommandScriptHash);
        builder.AppendLine("- transcriptHash: " + report.TranscriptHash);
        builder.AppendLine("- stateHashChainHash: " + report.StateHashChainHash);
        builder.AppendLine("- finalCoverageStateHash: " + report.FinalCoverageStateHash);
        builder.AppendLine("- replayFinalStateHash: " + report.ReplayFinalStateHash);
        builder.AppendLine("- handoffManifestHash: " + report.HandoffManifestHash);
        builder.AppendLine("- fileLedgerHash: " + report.FileLedgerHash);
        builder.AppendLine("- probeReadProofHash: " + report.ProbeReadProofHash);
        builder.AppendLine("- negativeProofHash: " + report.NegativeProofHash);
        builder.AppendLine("- commandTranscriptProofHash: " + report.CommandTranscriptProofHash);
        builder.AppendLine("- winFormsBindingInventoryHash: " + report.WinFormsBindingInventoryHash);
        builder.AppendLine("- qualityGateScanHash: " + report.QualityGateScanHash);
        builder.AppendLine("- sourceArtifactManifestHash: " + report.SourceArtifactManifestHash);
        builder.AppendLine("- deterministicHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Disposition");
        builder.AppendLine();
        builder.AppendLine(
            "Goal082 consumes the real Goal080 projected GamePackage and Goal081 runtime-preview playthrough artifacts, mirrors a compact player-facing payload into Unity StreamingAssets, validates the mirrored payload through an Application-side probe simulation, and leaves the manual gate required for review.");
        builder.AppendLine();
        builder.AppendLine("## Required Artifacts");
        foreach (var artifact in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredArtifactFileNames)
        {
            builder.AppendLine("- " + artifact);
        }

        builder.AppendLine();
        builder.AppendLine("## Unity Payload Files");
        foreach (var artifact in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames)
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
