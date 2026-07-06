using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunUnityProjectionVerificationScriptTests
{
    [Fact]
    public void RunUnityProjectionVerificationScriptExposesBatchmodeRunnerContract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-unity-projection-verification.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-unity-projection-verification.cmd"));

        Assert.Contains("[ValidateSet(\"GenericFullPlaythrough\")]", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$PackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$EvidenceRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$ResultPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$LogPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains(
            "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke",
            script,
            StringComparison.Ordinal);
        Assert.Contains("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS", script, StringComparison.Ordinal);
        Assert.Contains("GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL", script, StringComparison.Ordinal);
        Assert.Contains("samples/minimal-map-game/package.json", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-RunnerPackagePath", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-RunnerOutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-RunnerOutputFile", script, StringComparison.Ordinal);
        Assert.Contains("Test-RunnerPathUnderRoot", script, StringComparison.Ordinal);
        Assert.Contains(".llmgc/manual/", script, StringComparison.Ordinal);
        Assert.Contains("-llmgcPackagePath", script, StringComparison.Ordinal);
        Assert.Contains(
            "Instantiating material due to calling renderer.material during edit mode",
            script,
            StringComparison.Ordinal);
        Assert.Contains("UnityEngine.Renderer:get_material()", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-Apply", script, StringComparison.Ordinal);
        Assert.Contains("parameterized-gamepackage-runner-result.json", script, StringComparison.Ordinal);
        Assert.Contains("unity-batchmode-parameterized-gamepackage-full-playthrough.log", script, StringComparison.Ordinal);
        Assert.Contains("mode", script, StringComparison.Ordinal);
        Assert.Contains("packagePath", script, StringComparison.Ordinal);
        Assert.Contains("packagePathRelative", script, StringComparison.Ordinal);
        Assert.Contains("unityPath", script, StringComparison.Ordinal);
        Assert.Contains("unityExitCode", script, StringComparison.Ordinal);
        Assert.Contains("passMarkerPresent", script, StringComparison.Ordinal);
        Assert.Contains("failMarkerAbsent", script, StringComparison.Ordinal);
        Assert.Contains("materialWarningAbsent", script, StringComparison.Ordinal);
        Assert.Contains("cleanupApplied", script, StringComparison.Ordinal);
        Assert.Contains("cleanupExitCode", script, StringComparison.Ordinal);
        Assert.Contains("passed", script, StringComparison.Ordinal);
        Assert.Contains("logPath", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSettings", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generator-library", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("run-unity-projection-verification.ps1", cmd, StringComparison.Ordinal);
        Assert.Contains("-Mode GenericFullPlaythrough", cmd, StringComparison.Ordinal);
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
