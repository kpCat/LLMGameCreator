namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal139WinFormsPageText(string projectRoot)
    {
        const string pageGoal139RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal139.cs";
        return ReadOptionalText(projectRoot, pageGoal139RelativePath);
    }

    private static string ReadGoal120Through139WinFormsPageText(string projectRoot) =>
        ReadGoal120Through138WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal139WinFormsPageText(projectRoot);

    private static string ReadGoal140WinFormsPageText(string projectRoot)
    {
        const string pageGoal140RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal140.cs";
        return ReadOptionalText(projectRoot, pageGoal140RelativePath);
    }

    private static string ReadGoal120Through140WinFormsPageText(string projectRoot) =>
        ReadGoal120Through139WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal140WinFormsPageText(projectRoot);

    private static string ReadGoal141WinFormsPageText(string projectRoot)
    {
        const string pageGoal141RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal141.cs";
        return ReadOptionalText(projectRoot, pageGoal141RelativePath);
    }

    private static string ReadGoal120Through141WinFormsPageText(string projectRoot) =>
        ReadGoal120Through140WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal141WinFormsPageText(projectRoot);

    private static bool PageBindsGoal139RuntimeBackedUnityPlayerLoopInteractiveControls(
        string pageText) =>
        pageText.Contains("Goal139 Controls", StringComparison.Ordinal)
        && pageText.Contains("BindGoal139RuntimeBackedUnityPlayerLoopInteractiveControls", StringComparison.Ordinal)
        && pageText.Contains("acceptedGoal138", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("frameCount", StringComparison.Ordinal)
        && pageText.Contains("requiredControlsPresent", StringComparison.Ordinal)
        && pageText.Contains("controlScriptPassed", StringComparison.Ordinal)
        && pageText.Contains("interactiveControlsWindowPresent", StringComparison.Ordinal)
        && pageText.Contains("unityInteractiveControlsSmokePassed", StringComparison.Ordinal)
        && pageText.Contains("runtimeAuthority", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal139RuntimeBackedUnityPlayerLoopInteractiveControls(pageText);
        AddIfFalse(
            binds,
            "goal139.winforms.runtime_backed_unity_player_loop_interactive_controls_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }

    private static bool PageBindsGoal140RuntimeBackedUnityPlayerLoopControlsUx(
        string pageText) =>
        pageText.Contains("Goal140 Controls UX", StringComparison.Ordinal)
        && pageText.Contains("BindGoal140RuntimeBackedUnityPlayerLoopControlsUx", StringComparison.Ordinal)
        && pageText.Contains("acceptedGoal139", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateId", StringComparison.Ordinal)
        && pageText.Contains("frameCount", StringComparison.Ordinal)
        && pageText.Contains("humanReadableFrameNumbering", StringComparison.Ordinal)
        && pageText.Contains("stepOnceSemanticsClear", StringComparison.Ordinal)
        && pageText.Contains("playAllToEndSemanticsClear", StringComparison.Ordinal)
        && pageText.Contains("knownUnityEditorNoiseClassified", StringComparison.Ordinal)
        && pageText.Contains("blockingUnityErrorCount", StringComparison.Ordinal)
        && pageText.Contains("unclassifiedUnityErrorCount", StringComparison.Ordinal)
        && pageText.Contains("unityControlsUxSmokePassed", StringComparison.Ordinal)
        && pageText.Contains("runtimeAuthority", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal);

    private static bool ScanGoal140RuntimeBackedUnityPlayerLoopControlsUxBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal140RuntimeBackedUnityPlayerLoopControlsUx(pageText);
        AddIfFalse(
            binds,
            "goal140.winforms.runtime_backed_unity_player_loop_controls_ux_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }

    private static bool PageBindsGoal141RuntimeBackedPlayerCommandRoundtrip(
        string pageText) =>
        pageText.Contains("Goal141 Command Roundtrip", StringComparison.Ordinal)
        && pageText.Contains("BindGoal141RuntimeBackedPlayerCommandRoundtrip", StringComparison.Ordinal)
        && pageText.Contains("goal140Accepted", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("roundtripRequestCount", StringComparison.Ordinal)
        && pageText.Contains("runtimeExecutedRequestCount", StringComparison.Ordinal)
        && pageText.Contains("roundtripSnapshotCount", StringComparison.Ordinal)
        && pageText.Contains("controlRequestBridgePresent", StringComparison.Ordinal)
        && pageText.Contains("stateHashChainPresent", StringComparison.Ordinal)
        && pageText.Contains("runtimeAuthority", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("unityConsumesRoundtripResult", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("accepted", StringComparison.Ordinal);

    private static bool ScanGoal141RuntimeBackedPlayerCommandRoundtripBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal141RuntimeBackedPlayerCommandRoundtrip(pageText);
        AddIfFalse(
            binds,
            "goal141.winforms.runtime_backed_player_command_roundtrip_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }

    private static (
        bool Stepper,
        bool InteractiveControls,
        bool ControlsUx,
        bool CommandRoundtrip) ScanGoal138Through141RuntimeBackedBindings(
            string pageText,
            string pageRelativePath,
            List<VisualWorldPreviewDiagnostic> diagnostics) =>
        (
            ScanGoal138RuntimeBackedUnityPlayerLoopStepperBinding(pageText, pageRelativePath, diagnostics),
            ScanGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsBinding(
                pageText,
                pageRelativePath,
                diagnostics),
            ScanGoal140RuntimeBackedUnityPlayerLoopControlsUxBinding(pageText, pageRelativePath, diagnostics),
            ScanGoal141RuntimeBackedPlayerCommandRoundtripBinding(pageText, pageRelativePath, diagnostics));
}
