namespace LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

public sealed partial class OfflineGeoworldAcceptedAlphaBaselineReviewService
{
    private static string RenderReport(
        OfflineGeoworldAcceptedAlphaBaselineDashboard dashboard,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex,
        OfflineGeoworldAcceptedAlphaBaselineQualityGateScan quality,
        OfflineGeoworldAcceptedAlphaBaselineNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 118 Offline Geoworld Accepted Alpha Baseline Review",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- acceptedBaselineReady: " + dashboard.AcceptedBaselineReady.ToString().ToLowerInvariant(),
            "- baselineId: " + dashboard.BaselineId,
            "- baselineHash: " + dashboard.BaselineHash,
            "- manualGate: " + dashboard.ManualGate,
            "- manualGateStatus: " + dashboard.ManualGateStatus,
            "- acceptedByCodex: false",
            "- manualResultSha256: " + dashboard.ManualResultSha256,
            "- sourceGoalRange: " + dashboard.SourceGoalRange,
            "- includedSourceGoalCount: " + dashboard.IncludedSourceGoalCount,
            "- acceptedEvidenceRootCount: " + dashboard.AcceptedEvidenceRootCount,
            "- producedOnlyRootCount: " + dashboard.ProducedOnlyRootCount,
            "- blockedOrSupersededNoteCount: " + dashboard.BlockedOrSupersededNoteCount,
            "- notFinalReleaseOrRuntimeBuild: "
            + dashboard.NotFinalReleaseOrRuntimeBuild.ToString().ToLowerInvariant(),
            "- noRuntimeProviderOrNetworkChanges: "
            + dashboard.NoRuntimeProviderOrNetworkChanges.ToString().ToLowerInvariant(),
            "- noUnityFileChangesRequired: "
            + dashboard.NoUnityFileChangesRequired.ToString().ToLowerInvariant(),
            "- recommendedNextDecision: " + dashboard.RecommendedNextDecision,
            "- doNotStartAutomatically: "
            + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Accepted Evidence Roots",
            string.Empty
        };
        lines.AddRange(dashboard.AcceptedEvidenceRoots.Select(root => "- " + root));
        lines.AddRange(
        [
            string.Empty,
            "## Produced-Only Historical Roots",
            string.Empty
        ]);
        lines.AddRange(dashboard.ProducedOnlyHistoricalRoots.Select(root => "- " + root));
        lines.AddRange(
        [
            string.Empty,
            "## Source Index",
            string.Empty
        ]);
        lines.AddRange(sourceIndex.Entries.Select(entry =>
            "- " + entry.SourceGoalId
            + ": present=" + entry.Present.ToString().ToLowerInvariant()
            + ", classification=" + entry.Classification
            + ", root=" + entry.RelativeRoot));
        lines.AddRange(
        [
            string.Empty,
            "## Blocked Or Superseded Notes",
            string.Empty
        ]);
        lines.AddRange(dashboard.BlockedOrSupersededNotes.Select(note => "- " + note));
        lines.AddRange(
        [
            string.Empty,
            "## Quality",
            string.Empty,
            "- goal116AcceptedEvidenceValid: "
            + quality.Goal116AcceptedEvidenceValid.ToString().ToLowerInvariant(),
            "- goal117ContinuationEvidenceValid: "
            + quality.Goal117ContinuationEvidenceValid.ToString().ToLowerInvariant(),
            "- goal114UnitySafeModeEvidenceExists: "
            + quality.Goal114UnitySafeModeEvidenceExists.ToString().ToLowerInvariant(),
            "- goal109PortableExportEvidenceExists: "
            + quality.Goal109PortableExportEvidenceExists.ToString().ToLowerInvariant(),
            "- goal108AlphaSliceEvidenceExists: "
            + quality.Goal108AlphaSliceEvidenceExists.ToString().ToLowerInvariant(),
            "- sourceGoalRangeIncluded: "
            + quality.SourceGoalRangeIncluded.ToString().ToLowerInvariant(),
            "- manualInputExcluded: "
            + quality.ManualInputExcluded.ToString().ToLowerInvariant(),
            "- negativeProofPassed: "
            + quality.NegativeProofPassed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- missingGoal116AcceptedEvidenceRejected: "
            + negative.MissingGoal116AcceptedEvidenceRejected.ToString().ToLowerInvariant(),
            "- missingGoal117PostAcceptanceRoutingRejected: "
            + negative.MissingGoal117PostAcceptanceRoutingRejected.ToString().ToLowerInvariant(),
            "- manualInputStagedOrEmbeddedRejected: "
            + negative.ManualInputStagedOrEmbeddedRejected.ToString().ToLowerInvariant(),
            "- liveGeodataProviderNetworkStartRejected: "
            + negative.LiveGeodataProviderNetworkStartRejected.ToString().ToLowerInvariant(),
            "- runtimeSchemaLuaGeneratorLibraryChangesRejected: "
            + negative.RuntimeSchemaLuaGeneratorLibraryChangesRejected.ToString().ToLowerInvariant(),
            "- unityScenesPrefabsSettingsPackagesStreamingAssetsRejected: "
            + negative.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected
                .ToString().ToLowerInvariant(),
            "- finalReleasePackagingRejected: "
            + negative.FinalReleasePackagingRejected.ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(
        OfflineGeoworldAcceptedAlphaBaselineDashboard dashboard,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex,
        OfflineGeoworldAcceptedAlphaBaselineQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Accepted Alpha Baseline Review",
            string.Empty,
            "Goal118 records a deterministic accepted Alpha baseline review package after Goal116 human acceptance.",
            string.Empty,
            "## Baseline",
            string.Empty,
            "- baselineId: " + dashboard.BaselineId,
            "- baselineHash: " + dashboard.BaselineHash,
            "- acceptedBaselineReady: " + dashboard.AcceptedBaselineReady.ToString().ToLowerInvariant(),
            "- manualGateStatus: " + dashboard.ManualGateStatus,
            "- manualResultSha256: " + dashboard.ManualResultSha256,
            "- sourceGoalRange: " + dashboard.SourceGoalRange,
            "- includedSourceGoalCount: " + dashboard.IncludedSourceGoalCount,
            string.Empty,
            "## Boundaries",
            string.Empty,
            "This baseline review is not final release, not a Runtime build, not live geodata/provider/network approval, not public schema/Lua/generator-library approval, not final art/atlas approval and not Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.",
            string.Empty,
            "## Source Roots",
            string.Empty
        };
        lines.AddRange(sourceIndex.Entries.Select(entry =>
            "- `" + entry.SourceGoalId + "`: " + entry.Classification));
        lines.AddRange(
        [
            string.Empty,
            "## Next Decision",
            string.Empty,
            "- recommendedNextDecision: " + dashboard.RecommendedNextDecision,
            "- doNotStartAutomatically: "
            + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant(),
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void AddDiagnostics(
        List<string> lines,
        string title,
        IReadOnlyList<string> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return;
        }

        lines.Add(string.Empty);
        lines.Add("## " + title);
        lines.Add(string.Empty);
        lines.AddRange(diagnostics.Select(item => "- " + item));
    }
}
