namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed partial class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService
{
    private static string RenderReport(
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard,
        OfflineGeoworldAlphaPostAcceptanceContinuationMatrix matrix,
        OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan quality,
        OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 117 Offline Geoworld Alpha Post-Acceptance Continuation Selection",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + dashboard.ManualGate,
            "- manualGateStatus: " + dashboard.ManualGateStatus,
            "- humanAccepted: " + dashboard.HumanAccepted.ToString().ToLowerInvariant(),
            "- sourceDecisionStatus: " + dashboard.SourceDecisionStatus,
            "- manualResultSha256: " + dashboard.ManualResultSha256,
            "- acceptedByCodex: false",
            "- manualInputNotCommitted: " + dashboard.ManualInputNotCommitted
                .ToString().ToLowerInvariant(),
            "- rawManualResultEmbeddedInArtifacts: "
                + dashboard.RawManualResultEmbeddedInArtifacts.ToString().ToLowerInvariant(),
            "- recommendedNextLane: " + dashboard.RecommendedNextLane,
            "- recommendedNextGoalId: " + dashboard.RecommendedNextGoalId,
            "- readyLaneCount: " + dashboard.ReadyLaneCount,
            "- candidateLaneCount: " + dashboard.CandidateLaneCount,
            "- blockedLaneCount: " + dashboard.BlockedLaneCount,
            "- doNotStartAutomatically: "
                + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Goal116 Source Evidence",
            string.Empty,
            "- goal116AcceptanceRecordPresent: "
                + dashboard.Goal116AcceptanceRecordPresent.ToString().ToLowerInvariant(),
            "- goal116AcceptanceRecordValid: "
                + dashboard.Goal116AcceptanceRecordValid.ToString().ToLowerInvariant(),
            "- goal115DecisionSnapshotPresent: "
                + dashboard.Goal115DecisionSnapshotPresent.ToString().ToLowerInvariant(),
            "- goal115DecisionSnapshotGreen: "
                + dashboard.Goal115DecisionSnapshotGreen.ToString().ToLowerInvariant(),
            string.Empty,
            "## Continuation Matrix",
            string.Empty
        };
        lines.AddRange(matrix.Lanes.Select(lane =>
            "- " + lane.LaneId
            + ": status=" + lane.Status
            + ", recommended=" + lane.IsRecommended.ToString().ToLowerInvariant()
            + ", nextGoal=" + lane.RecommendedNextGoalId
            + ", explicitApproval="
            + lane.RequiresExplicitFutureApproval.ToString().ToLowerInvariant()));
        lines.AddRange(
        [
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- runtimeSchemaLuaGeneratorLibraryBlocked: "
            + quality.RuntimeSchemaLuaGeneratorLibraryBlocked.ToString().ToLowerInvariant(),
            "- liveGeodataProviderNetworkBlocked: "
            + quality.LiveGeodataProviderNetworkBlocked.ToString().ToLowerInvariant(),
            "- unityScenePrefabSettingsReleaseBlocked: "
            + quality.UnityScenePrefabSettingsReleaseBlocked.ToString().ToLowerInvariant(),
            "- finalRendererAtlasRequiresFutureDecision: "
            + quality.FinalRendererAtlasRequiresFutureDecision.ToString().ToLowerInvariant(),
            "- noGoal118TaskFilesCreated: "
            + quality.NoGoal118TaskFilesCreated.ToString().ToLowerInvariant(),
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- missingGoal116AcceptanceRejected: "
            + negative.MissingGoal116AcceptanceRejected.ToString().ToLowerInvariant(),
            "- nonAcceptedGoal116Rejected: "
            + negative.NonAcceptedGoal116Rejected.ToString().ToLowerInvariant(),
            "- codexAcceptanceRejected: "
            + negative.CodexAcceptanceRejected.ToString().ToLowerInvariant(),
            "- rawManualResultEmbeddingRejected: "
            + negative.RawManualResultEmbeddingRejected.ToString().ToLowerInvariant(),
            "- manualInputStagedOrCommittedRejected: "
            + negative.ManualInputStagedOrCommittedRejected.ToString().ToLowerInvariant(),
            "- automaticGoal118StartRejected: "
            + negative.AutomaticGoal118StartRejected.ToString().ToLowerInvariant(),
            "- forbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected: "
            + negative.ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected
                .ToString().ToLowerInvariant(),
            "- goal118TaskFilesNotCreated: "
            + negative.Goal118TaskFilesNotCreated.ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard,
        OfflineGeoworldAlphaPostAcceptanceContinuationMatrix matrix,
        OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Post-Acceptance Continuation Selection",
            string.Empty,
            "Goal117 records the first bounded continuation-selection surface after accepted Goal116 manual gate evidence.",
            string.Empty,
            "## Goal116 Source Evidence",
            string.Empty,
            "- manualGate: " + dashboard.ManualGate,
            "- manualGateStatus: " + dashboard.ManualGateStatus,
            "- humanAccepted: " + dashboard.HumanAccepted.ToString().ToLowerInvariant(),
            "- sourceDecisionStatus: " + dashboard.SourceDecisionStatus,
            "- manualResultSha256: " + dashboard.ManualResultSha256,
            "- acceptedByCodex: false",
            "- manualInputNotCommitted: "
                + dashboard.ManualInputNotCommitted.ToString().ToLowerInvariant(),
            "- rawManualResultEmbeddedInArtifacts: "
                + dashboard.RawManualResultEmbeddedInArtifacts.ToString().ToLowerInvariant(),
            string.Empty,
            "## Recommended Continuation",
            string.Empty,
            "- recommendedNextLane: " + dashboard.RecommendedNextLane,
            "- recommendedNextGoalId: " + dashboard.RecommendedNextGoalId,
            "- doNotStartAutomatically: "
                + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant(),
            "- readyLaneCount: " + dashboard.ReadyLaneCount,
            "- candidateLaneCount: " + dashboard.CandidateLaneCount,
            "- blockedLaneCount: " + dashboard.BlockedLaneCount,
            string.Empty,
            "## Matrix",
            string.Empty
        };
        lines.AddRange(matrix.Lanes.Select(lane =>
            "- `" + lane.LaneId + "`: " + lane.Status));
        lines.AddRange(
        [
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "No automatic live geodata, provider/network, Runtime, public schema, Lua, generator-library, final gameplay, final art, atlas, Unity scene/prefab/project-settings or release-packaging work is authorized by this selection surface.",
            string.Empty,
            "Goal117 does not create Goal118 task files. The next task must be explicitly selected from the matrix.",
            string.Empty,
            "## Quality",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderExportReadme(
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard) =>
        "# Goal 117 Offline Geoworld Alpha Post-Acceptance Continuation Selection"
        + Environment.NewLine
        + Environment.NewLine
        + "This export summarizes the post-acceptance continuation matrix. Current Goal116 "
        + "manual result hash evidence is included when present; raw manual JSON is never embedded."
        + Environment.NewLine
        + Environment.NewLine
        + "- manualGateStatus: " + dashboard.ManualGateStatus + Environment.NewLine
        + "- humanAccepted: " + dashboard.HumanAccepted.ToString().ToLowerInvariant()
        + Environment.NewLine
        + "- recommendedNextLane: " + dashboard.RecommendedNextLane + Environment.NewLine
        + "- recommendedNextGoalId: " + dashboard.RecommendedNextGoalId + Environment.NewLine
        + "- doNotStartAutomatically: "
        + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant()
        + Environment.NewLine;

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
