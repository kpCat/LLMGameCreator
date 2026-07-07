using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunRuntimeBackedUnityPlayerLoopControlsUxPolishScriptTests
{
    [Fact]
    public void RunRuntimeBackedUnityPlayerLoopControlsUxPolishScriptExposesGoal140Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-unity-player-loop-controls-ux-polish.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-unity-player-loop-controls-ux-polish.cmd"));

        Assert.Contains("[string]$InteractiveControlsModelPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$InteractiveControlsResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$InteractiveControlsScriptPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal140 output root", script, StringComparison.Ordinal);
        Assert.Contains("RuntimeBackedUnityPlayerLoopControlsUxPolishScriptProof", script, StringComparison.Ordinal);
        Assert.Contains(
            "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_PASS",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_FAIL",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "RunBatchmodeRuntimeBackedUnityPlayerLoopControlsUxSmoke",
            script,
            StringComparison.Ordinal);
        Assert.Contains("BuildProfileContext", script, StringComparison.Ordinal);
        Assert.Contains("CreateOrLoad", script, StringComparison.Ordinal);
        Assert.Contains("unpaired NullReferenceException", script, StringComparison.Ordinal);
        Assert.Contains("knownUnityEditorBuildProfileNoise", script, StringComparison.Ordinal);
        Assert.Contains("runtime-backed-player-loop-controls-ux-model.json", script, StringComparison.Ordinal);
        Assert.Contains("runtime-backed-player-loop-controls-ux-script.json", script, StringComparison.Ordinal);
        Assert.Contains("unity-editor-noise-classification.json", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-runtime-backed-unity-player-loop-controls-ux-polish.ps1", cmd, StringComparison.Ordinal);
        Assert.Contains("-ApplyCleanup", cmd, StringComparison.Ordinal);
        Assert.Contains("%*", cmd, StringComparison.Ordinal);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
