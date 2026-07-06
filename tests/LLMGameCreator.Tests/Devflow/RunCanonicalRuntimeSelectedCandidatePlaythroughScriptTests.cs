using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunCanonicalRuntimeSelectedCandidatePlaythroughScriptTests
{
    [Fact]
    public void RunCanonicalRuntimeSelectedCandidatePlaythroughScriptExposesGoal134Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-selected-candidate-playthrough.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-canonical-runtime-selected-candidate-playthrough.cmd"));

        Assert.Contains("[string]$SelectedCandidateHandoffPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$SelectedCandidatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal134 output root", script, StringComparison.Ordinal);
        Assert.Contains("CanonicalRuntimeSelectedCandidatePlaythroughScriptRuntimeProof", script, StringComparison.Ordinal);
        Assert.Contains("GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeCanonicalRuntimeSelectedCandidateTranscriptSmoke", script, StringComparison.Ordinal);
        Assert.Contains("unity-player-canonical-transcript-smoke.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-transcript.json", script, StringComparison.Ordinal);
        Assert.Contains("canonical-runtime-state-summary.json", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL134_UNITY_SMOKE_PATH", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-canonical-runtime-selected-candidate-playthrough.ps1", cmd, StringComparison.Ordinal);
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
