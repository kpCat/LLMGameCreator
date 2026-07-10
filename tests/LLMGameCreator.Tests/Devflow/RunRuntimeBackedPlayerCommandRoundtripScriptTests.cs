using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunRuntimeBackedPlayerCommandRoundtripScriptTests
{
    [Fact]
    public void RunRuntimeBackedPlayerCommandRoundtripScriptExposesGoal141Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-player-command-roundtrip.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-runtime-backed-player-command-roundtrip.cmd"));

        Assert.Contains("[string]$SelectedCandidatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$SelectedCandidateHandoffPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$ControlsUxModelPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$ControlsUxResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$ControlsUxScriptPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CommandLoopSnapshotsPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CommandLoopResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under repository root", script, StringComparison.Ordinal);
        Assert.Contains("Goal141 refuses .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal141 output root", script, StringComparison.Ordinal);
        Assert.Contains("RuntimeBackedPlayerCommandRoundtripScriptProof", script, StringComparison.Ordinal);
        Assert.Contains(
            "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_PASS",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_FAIL",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "RunBatchmodeRuntimeBackedPlayerCommandRoundtripSmoke",
            script,
            StringComparison.Ordinal);
        Assert.Contains("runtime-backed-player-command-roundtrip-model.json", script, StringComparison.Ordinal);
        Assert.Contains("runtime-backed-player-command-roundtrip-result.json", script, StringComparison.Ordinal);
        Assert.Contains("unity-player-command-roundtrip-smoke.json", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-runtime-backed-player-command-roundtrip.ps1", cmd, StringComparison.Ordinal);
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
