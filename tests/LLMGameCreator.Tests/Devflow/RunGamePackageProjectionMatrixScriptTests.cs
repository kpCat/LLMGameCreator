using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunGamePackageProjectionMatrixScriptTests
{
    [Fact]
    public void RunGamePackageProjectionMatrixScriptExposesCandidateMatrixContract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-projection-matrix.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-projection-matrix.cmd"));

        Assert.Contains("[string]$CandidateIndexPath", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-candidate-index.json", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("Read-CandidateIndex", script, StringComparison.Ordinal);
        Assert.Contains("Test-MatrixPathUnderRoot", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-MatrixInputPath", script, StringComparison.Ordinal);
        Assert.Contains(".llmgc/manual/", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("run-unity-projection-verification.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-Mode", script, StringComparison.Ordinal);
        Assert.Contains("GenericFullPlaythrough", script, StringComparison.Ordinal);
        Assert.Contains("-PackagePath", script, StringComparison.Ordinal);
        Assert.Contains("-EvidenceRoot", script, StringComparison.Ordinal);
        Assert.Contains("-ResultPath", script, StringComparison.Ordinal);
        Assert.Contains("-LogPath", script, StringComparison.Ordinal);
        Assert.Contains("-ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("runner-result.json", script, StringComparison.Ordinal);
        Assert.Contains("log-scan.json", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-projection-matrix-result.json", script, StringComparison.Ordinal);
        Assert.Contains("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("Instantiating material due to calling renderer.material during edit mode", script, StringComparison.Ordinal);
        Assert.Contains("UnityEngine.Renderer:get_material()", script, StringComparison.Ordinal);
        Assert.Contains("passed = [bool]$allPassed", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSettings", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Packages/manifest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StreamingAssets", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generator-library", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-gamepackage-projection-matrix.ps1", cmd, StringComparison.Ordinal);
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
