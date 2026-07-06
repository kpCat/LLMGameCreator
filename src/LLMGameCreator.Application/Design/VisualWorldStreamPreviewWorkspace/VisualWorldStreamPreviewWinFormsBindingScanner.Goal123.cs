namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal123WinFormsPageText(string projectRoot)
    {
        const string pageGoal123RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal123.cs";

        return ReadOptionalText(projectRoot, pageGoal123RelativePath);
    }

    private static string ReadGoal120Through123WinFormsPageText(string projectRoot) =>
        ReadGoal120WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal121WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal122WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal123WinFormsPageText(projectRoot);

    private static bool PageBindsGoal123GenericGamePackageProjection(string pageText) =>
        pageText.Contains("genericProjectionStatus", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionSamplePackagePath", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionPackageId", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionPackageTitle", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionMapId", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionMapSize", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionEntityCount", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionItemCount", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionUnitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionGoal122StillGreen", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionCleanupScriptAvailable", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionDoNotStartAutomatically", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionEvidencePath", StringComparison.Ordinal)
        && pageText.Contains("genericProjectionExportPath", StringComparison.Ordinal);
}
