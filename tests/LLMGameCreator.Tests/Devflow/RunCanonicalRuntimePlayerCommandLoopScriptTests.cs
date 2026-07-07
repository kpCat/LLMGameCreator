using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunCanonicalRuntimePlayerCommandLoopScriptTests
{
    [Fact]
    public void RunCanonicalRuntimePlayerCommandLoopScriptExposesGoal136Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-player-command-loop.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-player-command-loop.cmd"));

        Assert.Contains("[string]$SelectedCandidateHandoffPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$SelectedCandidatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$Goal134TranscriptPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$Goal134StateSummaryPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$Goal135PlayerLoopPlanPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$Goal135PlayerAdapterContractPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal136 output root", script, StringComparison.Ordinal);
        Assert.Contains("CanonicalRuntimePlayerCommandLoopScriptRuntimeProof", script, StringComparison.Ordinal);
        Assert.Contains("GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeCanonicalRuntimePlayerCommandLoopSmoke", script, StringComparison.Ordinal);
        Assert.Contains("unity-player-command-loop-smoke.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-player-command-loop-snapshots.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-player-command-loop-result.json", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL136_UNITY_SMOKE_PATH", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-canonical-runtime-player-command-loop.ps1", cmd, StringComparison.Ordinal);
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
