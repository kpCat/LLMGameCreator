namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal137WinFormsPageText(string projectRoot)
    {
        const string pageGoal137RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal137.cs";
        return ReadOptionalText(projectRoot, pageGoal137RelativePath);
    }

    private static string ReadGoal120Through137WinFormsPageText(string projectRoot) =>
        ReadGoal120Through136WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal137WinFormsPageText(projectRoot);

    private static bool PageBindsGoal137CanonicalRuntimeUnityPlayerLoopPlayback(
        string pageText) =>
        pageText.Contains("Goal137 Playback", StringComparison.Ordinal)
        && pageText.Contains("BindGoal137UnityPlayerLoopPlayback", StringComparison.Ordinal)
        && pageText.Contains("playbackFrameCount", StringComparison.Ordinal)
        && pageText.Contains("requiredFrameCategoriesPresent", StringComparison.Ordinal)
        && pageText.Contains("unityPlayerLoopPlaybackPassed", StringComparison.Ordinal)
        && pageText.Contains("runtimeSnapshotSource", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateExecutedByRuntime", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal137CanonicalRuntimeUnityPlayerLoopPlaybackBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal137CanonicalRuntimeUnityPlayerLoopPlayback(pageText);
        AddIfFalse(
            binds,
            "goal137.winforms.canonical_runtime_unity_player_loop_playback_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
