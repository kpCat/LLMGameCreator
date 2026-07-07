using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunRuntimeBackedUnityPlayerLoopStepperScriptTests
{
    [Fact]
    public void RunRuntimeBackedUnityPlayerLoopStepperScriptExposesGoal138Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-unity-player-loop-stepper.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-unity-player-loop-stepper.cmd"));

        Assert.Contains("[string]$PlaybackFramesPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$PlaybackResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CommandLoopSnapshotsPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CommandLoopResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$PlayerAdapterContractPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal138 output root", script, StringComparison.Ordinal);
        Assert.Contains("RuntimeBackedUnityPlayerLoopStepperScriptProof", script, StringComparison.Ordinal);
        Assert.Contains("GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeRuntimeBackedUnityPlayerLoopStepperSmoke", script, StringComparison.Ordinal);
        Assert.Contains("runtime-backed-player-loop-stepper-model.json", script, StringComparison.Ordinal);
        Assert.Contains("unity-player-loop-stepper-smoke.json", script, StringComparison.Ordinal);
        Assert.Contains("CanonicalRuntimeUnityPlayerLoopStepperWindow.cs", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL138_UNITY_SMOKE_PATH", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-runtime-backed-unity-player-loop-stepper.ps1", cmd, StringComparison.Ordinal);
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
