namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal122ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Accepted Alpha Projection Action Loop",
            string.Empty,
            $"- acceptedAlphaProjectionActionLoopStatus: {report.AcceptedAlphaProjectionActionLoopStatus}",
            $"- acceptedAlphaProjectionActionLoopWindowPolishStatus: {report.AcceptedAlphaProjectionActionLoopWindowPolishStatus}",
            $"- acceptedAlphaProjectionActionLoopUnityMenuPath: {report.AcceptedAlphaProjectionActionLoopUnityMenuPath}",
            $"- acceptedAlphaProjectionActionLoopOneClickVerificationStillPresent: {report.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionActionPreviewPresent: {report.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionActionApplyPresent: {report.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionStateResetPresent: {report.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopWindowLayoutPolishPresent: {report.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopUnitySmokeStatus: {report.AcceptedAlphaProjectionActionLoopUnitySmokeStatus}",
            $"- acceptedAlphaProjectionActionLoopCleanupScriptAvailable: {report.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopQualityGatePassed: {report.AcceptedAlphaProjectionActionLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal122FilesDiscoveredByRelativePaths: {report.Goal122FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal122QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal122 Quality",
            string.Empty,
            $"- acceptedAlphaProjectionActionLoopGroupPresent: {qualityGate.AcceptedAlphaProjectionActionLoopGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopStatus: {qualityGate.AcceptedAlphaProjectionActionLoopStatus}",
            $"- acceptedAlphaProjectionActionLoopWindowPolishStatus: {qualityGate.AcceptedAlphaProjectionActionLoopWindowPolishStatus}",
            $"- acceptedAlphaProjectionActionLoopUnityMenuPath: {qualityGate.AcceptedAlphaProjectionActionLoopUnityMenuPath}",
            $"- acceptedAlphaProjectionActionLoopOneClickVerificationStillPresent: {qualityGate.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionActionPreviewPresent: {qualityGate.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionActionApplyPresent: {qualityGate.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopProjectionStateResetPresent: {qualityGate.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopWindowLayoutPolishPresent: {qualityGate.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopUnitySmokeStatus: {qualityGate.AcceptedAlphaProjectionActionLoopUnitySmokeStatus}",
            $"- acceptedAlphaProjectionActionLoopCleanupScriptAvailable: {qualityGate.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionActionLoopQualityGatePassed: {qualityGate.AcceptedAlphaProjectionActionLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal122FilesDiscoveredByRelativePaths: {qualityGate.Goal122FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsAcceptedAlphaProjectionActionLoopBindingReal: {qualityGate.WinFormsAcceptedAlphaProjectionActionLoopBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
