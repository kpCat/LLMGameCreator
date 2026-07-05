namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

public sealed partial class OfflineGeoworldAlphaManualGateAcceptanceRecordService
{
    private static object BuildExportRecord(
        OfflineGeoworldAlphaManualGateAcceptanceRecord record,
        OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan quality) =>
        new
        {
            record.GoalId,
            record.SourceGoalIds,
            record.ManualGate,
            record.ManualGateStatus,
            record.HumanAccepted,
            record.HumanDecisionStatement,
            record.SourceDecisionStatus,
            record.ManualResultSha256,
            record.ManualInputNotCommitted,
            record.RawManualResultEmbeddedInArtifacts,
            record.AcceptedByCodex,
            record.NotFinalReleaseOrRuntimeBuild,
            record.NoRuntimeProviderOrNetworkChanges,
            record.NoUnityFileChangesRequired,
            record.RecommendedNextDecision,
            qualityGatePassed = quality.Passed
        };

    private static string RenderReport(
        OfflineGeoworldAlphaManualGateAcceptanceRecord record,
        OfflineGeoworldAlphaManualGateAcceptanceDashboard dashboard,
        OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan quality,
        OfflineGeoworldAlphaManualGateAcceptanceNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 116 Offline Geoworld Alpha Manual Gate Acceptance Record",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: " + quality.Accepted.ToString().ToLowerInvariant(),
            "- manualGate: " + record.ManualGate,
            "- manualGateStatus: " + record.ManualGateStatus,
            "- humanAccepted: " + record.HumanAccepted.ToString().ToLowerInvariant(),
            "- humanDecisionStatement: " + record.HumanDecisionStatement,
            "- sourceDecisionStatus: " + record.SourceDecisionStatus,
            "- manualResultSha256: " + record.ManualResultSha256,
            "- acceptedByCodex: false",
            "- manualInputNotCommitted: true",
            "- rawManualResultEmbeddedInArtifacts: false",
            "- recommendedNextDecision: " + record.RecommendedNextDecision,
            "- notFinalReleaseOrRuntimeBuild: true",
            "- noRuntimeProviderOrNetworkChanges: true",
            "- noUnityFileChangesRequired: true",
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Source Evidence",
            string.Empty,
            "- goal115SnapshotPresent: " + record.Goal115SnapshotPresent.ToString().ToLowerInvariant(),
            "- goal115SnapshotValid: " + record.Goal115SnapshotValid.ToString().ToLowerInvariant(),
            "- requiredStepCount: " + record.RequiredStepCount,
            "- passedStepCount: " + record.PassedStepCount,
            "- goal115ErrorsEmpty: " + record.Goal115ErrorsEmpty.ToString().ToLowerInvariant(),
            "- goal115WarningsEmpty: " + record.Goal115WarningsEmpty.ToString().ToLowerInvariant(),
            string.Empty,
            "## Evidence",
            string.Empty
        };
        lines.AddRange(dashboard.EvidenceArtifactPaths.Select(path => "- " + path));
        lines.Add(string.Empty);
        lines.Add("## Export");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.ExportArtifactPaths.Select(path => "- " + path));
        lines.AddRange(
        [
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- missingGoal115SnapshotRejected: "
            + negative.MissingGoal115SnapshotRejected.ToString().ToLowerInvariant(),
            "- nonGreenGoal115DecisionRejected: "
            + negative.NonGreenGoal115DecisionRejected.ToString().ToLowerInvariant(),
            "- manualHashMismatchRejected: "
            + negative.ManualHashMismatchRejected.ToString().ToLowerInvariant(),
            "- rawManualResultEmbeddingRejected: "
            + negative.RawManualResultEmbeddingRejected.ToString().ToLowerInvariant(),
            "- manualInputStagedOrCommittedRejected: "
            + negative.ManualInputStagedOrCommittedRejected.ToString().ToLowerInvariant(),
            "- forbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected: "
            + negative.ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected
                .ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Errors", record.Errors);
        AddDiagnostics(lines, "Warnings", record.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(OfflineGeoworldAlphaManualGateAcceptanceRecord record)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Manual Gate Acceptance Record",
            string.Empty,
            "Goal116 records the repository owner's explicit human decision for `offline_geoworld_alpha_manual_acceptance_verification` from the Goal115 GREEN candidate.",
            string.Empty,
            "## Acceptance Record",
            string.Empty,
            "- manualGate: " + record.ManualGate,
            "- manualGateStatus: " + record.ManualGateStatus,
            "- humanAccepted: " + record.HumanAccepted.ToString().ToLowerInvariant(),
            "- humanDecisionStatement: " + record.HumanDecisionStatement,
            "- sourceDecisionStatus: " + record.SourceDecisionStatus,
            "- manualResultSha256: " + record.ManualResultSha256,
            "- acceptedByCodex: false",
            "- manualInputNotCommitted: true",
            "- rawManualResultEmbeddedInArtifacts: false",
            "- recommendedNextDecision: " + record.RecommendedNextDecision,
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "This record is not final release approval, not Runtime approval, not live geodata/provider approval, not schema/Lua/generator-library approval, not final art/atlas approval, and not Unity scene/prefab/project-settings/release-packaging approval.",
            string.Empty,
            "The local `.llmgc/manual/**` result remains human input and must not be committed.",
            string.Empty,
            "## Next Safe Step",
            string.Empty,
            "Select the post-acceptance continuation explicitly. Do not automatically start live geodata ingestion, Runtime consumer work, providers, schema, Lua, generator-library, final art, atlas, Unity scene/prefab/project settings or release packaging.",
            string.Empty
        };
        AddDiagnostics(lines, "Errors", record.Errors);
        AddDiagnostics(lines, "Warnings", record.Warnings);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderExportReadme(
        OfflineGeoworldAlphaManualGateAcceptanceRecord record) =>
        "# Goal 116 Offline Geoworld Alpha Manual Gate Acceptance Record" + Environment.NewLine
        + Environment.NewLine
        + "This export summarizes the explicit human acceptance decision for the Goal115 GREEN candidate. "
        + "It contains the manual result hash and decision statement, not the raw manual JSON." + Environment.NewLine
        + Environment.NewLine
        + "- manualGate: " + record.ManualGate + Environment.NewLine
        + "- manualGateStatus: " + record.ManualGateStatus + Environment.NewLine
        + "- humanAccepted: " + record.HumanAccepted.ToString().ToLowerInvariant() + Environment.NewLine
        + "- sourceDecisionStatus: " + record.SourceDecisionStatus + Environment.NewLine
        + "- acceptedByCodex: false" + Environment.NewLine
        + "- recommendedNextDecision: " + record.RecommendedNextDecision + Environment.NewLine;

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
