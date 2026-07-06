namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal121WinFormsPageText(string projectRoot)
    {
        const string pageGoal121RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal121.cs";

        return ReadOptionalText(projectRoot, pageGoal121RelativePath);
    }

    private static bool PageBindsGoal121AcceptedAlphaInteractionDrilldown(string pageText) =>
        pageText.Contains("acceptedAlphaInteractionDrilldownFullVerificationStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownUnityMenuPath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownOneClickButtonPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownDrilldownFieldsPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownInteractionPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownCleanupScriptAvailable", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownMaterialWarningGuardPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownEvidencePath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaInteractionDrilldownExportPath", StringComparison.Ordinal);
}
