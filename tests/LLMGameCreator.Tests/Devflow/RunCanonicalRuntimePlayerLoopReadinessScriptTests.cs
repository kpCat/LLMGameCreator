using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunCanonicalRuntimePlayerLoopReadinessScriptTests
{
    [Fact]
    public void RunCanonicalRuntimePlayerLoopReadinessScriptExposesGoal135Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-player-loop-readiness.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-player-loop-readiness.cmd"));

        Assert.Contains("[string]$CanonicalRuntimeTranscriptPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CanonicalRuntimeStateSummaryPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$CanonicalRuntimeDashboardPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal135 output root", script, StringComparison.Ordinal);
        Assert.Contains("CanonicalRuntimePlayerLoopReadinessScriptRuntimeProof", script, StringComparison.Ordinal);
        Assert.Contains("GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeCanonicalRuntimePlayerLoopReadinessSmoke", script, StringComparison.Ordinal);
        Assert.Contains("unity-player-loop-readiness-smoke.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-player-loop-plan.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-state-summary.json", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL135_UNITY_SMOKE_PATH", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-canonical-runtime-player-loop-readiness.ps1", cmd, StringComparison.Ordinal);
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
