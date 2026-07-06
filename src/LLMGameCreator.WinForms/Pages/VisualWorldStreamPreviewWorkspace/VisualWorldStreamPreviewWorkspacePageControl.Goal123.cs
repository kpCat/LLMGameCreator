using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGenericGamePackageProjectionDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "genericProjectionStatus="
            + result.Report.GenericProjectionStatus,
        "genericProjectionSamplePackagePath="
            + result.Report.GenericProjectionSamplePackagePath,
        "genericProjectionPackageId="
            + result.Report.GenericProjectionPackageId,
        "genericProjectionPackageTitle="
            + result.Report.GenericProjectionPackageTitle,
        "genericProjectionMapId="
            + result.Report.GenericProjectionMapId,
        "genericProjectionMapSize="
            + result.Report.GenericProjectionMapSize,
        "genericProjectionEntityCount="
            + result.Report.GenericProjectionEntityCount,
        "genericProjectionItemCount="
            + result.Report.GenericProjectionItemCount,
        "genericProjectionUnitySmokeStatus="
            + result.Report.GenericProjectionUnitySmokeStatus,
        "genericProjectionGoal122StillGreen="
            + result.Report.GenericProjectionGoal122StillGreen.ToString().ToLowerInvariant(),
        "genericProjectionCleanupScriptAvailable="
            + result.Report.GenericProjectionCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "genericGamePackageProjectionQualityGatePassed="
            + result.Report.GenericGamePackageProjectionQualityGatePassed.ToString().ToLowerInvariant(),
        "goal123FilesDiscoveredByRelativePaths="
            + result.Report.Goal123FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGenericGamePackageProjectionEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "genericProjectionStatus: "
            + entry.GenericProjectionStatus,
        "genericProjectionSamplePackagePath: "
            + entry.GenericProjectionSamplePackagePath,
        "genericProjectionPackageId: "
            + entry.GenericProjectionPackageId,
        "genericProjectionPackageTitle: "
            + entry.GenericProjectionPackageTitle,
        "genericProjectionMapId: "
            + entry.GenericProjectionMapId,
        "genericProjectionMapSize: "
            + entry.GenericProjectionMapSize,
        "genericProjectionEntityCount: "
            + entry.GenericProjectionEntityCount,
        "genericProjectionItemCount: "
            + entry.GenericProjectionItemCount,
        "genericProjectionUnitySmokeStatus: "
            + entry.GenericProjectionUnitySmokeStatus,
        "genericProjectionGoal122StillGreen: "
            + entry.GenericProjectionGoal122StillGreen.ToString().ToLowerInvariant(),
        "genericProjectionCleanupScriptAvailable: "
            + entry.GenericProjectionCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "genericProjectionDoNotStartAutomatically: "
            + entry.GenericProjectionDoNotStartAutomatically.ToString().ToLowerInvariant(),
        "genericProjectionEvidencePath: "
            + entry.GenericProjectionEvidencePath,
        "genericProjectionExportPath: "
            + entry.GenericProjectionExportPath
    ];
}
