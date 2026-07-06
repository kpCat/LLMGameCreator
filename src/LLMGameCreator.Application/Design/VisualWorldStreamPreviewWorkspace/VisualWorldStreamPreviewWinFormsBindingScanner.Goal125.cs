namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal125WinFormsPageText(string projectRoot)
    {
        const string pageGoal125RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal125.cs";

        return ReadOptionalText(projectRoot, pageGoal125RelativePath);
    }

    private static string ReadGoal120Through125WinFormsPageText(string projectRoot) =>
        ReadGoal120Through124WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal125WinFormsPageText(projectRoot);

    private static bool PageBindsGoal125GenericGamePackageSystems(string pageText) =>
        pageText.Contains("genericSystemsStatus", StringComparison.Ordinal)
        && pageText.Contains("samplePackagePath", StringComparison.Ordinal)
        && pageText.Contains("packageId", StringComparison.Ordinal)
        && pageText.Contains("recipePreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("recipeApplyPassed", StringComparison.Ordinal)
        && pageText.Contains("harvestPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("harvestApplyPassed", StringComparison.Ordinal)
        && pageText.Contains("transactionPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("encounterPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("combatRoundPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("inventorySummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("resourceSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("systemsEventLogPresent", StringComparison.Ordinal)
        && pageText.Contains("unitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("cleanupCommand", StringComparison.Ordinal)
        && pageText.Contains("goal124StillGreen", StringComparison.Ordinal)
        && pageText.Contains("samplePackageReadOnly", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);
}
