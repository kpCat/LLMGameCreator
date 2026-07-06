namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal123ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Generic GamePackage Projection",
            string.Empty,
            $"- genericProjectionStatus: {report.GenericProjectionStatus}",
            $"- genericProjectionSamplePackagePath: {report.GenericProjectionSamplePackagePath}",
            $"- genericProjectionPackageId: {report.GenericProjectionPackageId}",
            $"- genericProjectionPackageTitle: {report.GenericProjectionPackageTitle}",
            $"- genericProjectionMapId: {report.GenericProjectionMapId}",
            $"- genericProjectionMapSize: {report.GenericProjectionMapSize}",
            $"- genericProjectionEntityCount: {report.GenericProjectionEntityCount}",
            $"- genericProjectionItemCount: {report.GenericProjectionItemCount}",
            $"- genericProjectionUnitySmokeStatus: {report.GenericProjectionUnitySmokeStatus}",
            $"- genericProjectionGoal122StillGreen: {report.GenericProjectionGoal122StillGreen.ToString().ToLowerInvariant()}",
            $"- genericProjectionCleanupScriptAvailable: {report.GenericProjectionCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- genericGamePackageProjectionQualityGatePassed: {report.GenericGamePackageProjectionQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal123FilesDiscoveredByRelativePaths: {report.Goal123FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal123QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal123 Quality",
            string.Empty,
            $"- genericGamePackageProjectionGroupPresent: {qualityGate.GenericGamePackageProjectionGroupPresent.ToString().ToLowerInvariant()}",
            $"- genericProjectionStatus: {qualityGate.GenericProjectionStatus}",
            $"- genericProjectionSamplePackagePath: {qualityGate.GenericProjectionSamplePackagePath}",
            $"- genericProjectionPackageId: {qualityGate.GenericProjectionPackageId}",
            $"- genericProjectionPackageTitle: {qualityGate.GenericProjectionPackageTitle}",
            $"- genericProjectionMapId: {qualityGate.GenericProjectionMapId}",
            $"- genericProjectionMapSize: {qualityGate.GenericProjectionMapSize}",
            $"- genericProjectionEntityCount: {qualityGate.GenericProjectionEntityCount}",
            $"- genericProjectionItemCount: {qualityGate.GenericProjectionItemCount}",
            $"- genericProjectionUnitySmokeStatus: {qualityGate.GenericProjectionUnitySmokeStatus}",
            $"- genericProjectionGoal122StillGreen: {qualityGate.GenericProjectionGoal122StillGreen.ToString().ToLowerInvariant()}",
            $"- genericProjectionCleanupScriptAvailable: {qualityGate.GenericProjectionCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- genericGamePackageProjectionQualityGatePassed: {qualityGate.GenericGamePackageProjectionQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal123FilesDiscoveredByRelativePaths: {qualityGate.Goal123FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGenericGamePackageProjectionBindingReal: {qualityGate.WinFormsGenericGamePackageProjectionBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
