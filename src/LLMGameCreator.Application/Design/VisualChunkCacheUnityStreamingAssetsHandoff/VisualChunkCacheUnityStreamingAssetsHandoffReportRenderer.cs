using System.Text;

namespace LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;

public static class VisualChunkCacheUnityStreamingAssetsHandoffReportRenderer
{
    public static string Render(VisualChunkCacheUnityHandoffReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Goal 095 Visual Chunk Cache Unity StreamingAssets Handoff");
        builder.AppendLine();
        builder.AppendLine("- gate: " + report.ManualGate + " required");
        builder.AppendLine("- accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- implementationStatus: " + report.ImplementationStatus);
        builder.AppendLine("- streamingAssetsRelativeRoot: " + report.StreamingAssetsRelativeRoot);
        builder.AppendLine("- packageCount: " + report.PackageCount);
        builder.AppendLine("- exportRecordCount: " + report.ExportRecordCount);
        builder.AppendLine("- streamWindowCount: " + report.StreamWindowCount);
        builder.AppendLine("- uniqueChunkKeyCount: " + report.UniqueChunkKeyCount);
        builder.AppendLine("- alphaRuntimeBootstrapBeforeHash: " + report.AlphaRuntimeBootstrapBeforeHash);
        builder.AppendLine("- alphaRuntimeBootstrapAfterHash: " + report.AlphaRuntimeBootstrapAfterHash);
        builder.AppendLine("- alphaRuntimeBootstrapBeforeLineCount: " + report.AlphaRuntimeBootstrapBeforeLineCount);
        builder.AppendLine("- alphaRuntimeBootstrapAfterLineCount: " + report.AlphaRuntimeBootstrapAfterLineCount);
        builder.AppendLine("- handoffManifestHash: " + report.HandoffManifestHash);
        builder.AppendLine("- packageIndexHash: " + report.PackageIndexHash);
        builder.AppendLine("- streamWindowIndexHash: " + report.StreamWindowIndexHash);
        builder.AppendLine("- chunkKeyLedgerHash: " + report.ChunkKeyLedgerHash);
        builder.AppendLine("- runtimeReadmeHash: " + report.RuntimeReadmeHash);
        builder.AppendLine("- streamingAssetsLedgerHash: " + report.StreamingAssetsLedgerHash);
        builder.AppendLine("- probeSourceInventoryHash: " + report.ProbeSourceInventoryHash);
        builder.AppendLine("- simulatedReadProofHash: " + report.SimulatedReadProofHash);
        builder.AppendLine("- negativeProofHash: " + report.NegativeProofHash);
        builder.AppendLine("- sourceLineageHash: " + report.SourceLineageHash);
        builder.AppendLine("- qualityGateScanHash: " + report.QualityGateScanHash);
        builder.AppendLine("- deterministicHash: " + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Disposition");
        builder.AppendLine();
        builder.AppendLine(
            "Goal095 mirrors a compact metadata-only subset of the real Goal093/094 visual chunk cache evidence into Unity Alpha StreamingAssets and validates it through an Application-side simulated Unity read plus a standalone Unity probe source inventory.");
        builder.AppendLine();
        builder.AppendLine(
            "This is Unity Alpha handoff/probe only. It does not implement Runtime consumption, live Unity gameplay rendering, final atlas generation, provider calls, LLM calls, or runtime streaming.");
        builder.AppendLine();
        builder.AppendLine("## Payload Files");
        foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames)
        {
            builder.AppendLine("- " + fileName);
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence Files");
        foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredEvidenceFileNames)
        {
            builder.AppendLine("- " + fileName);
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        if (report.Diagnostics.Count == 0)
        {
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
