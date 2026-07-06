using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunGamePackageCandidateFactoryScriptTests
{
    [Fact]
    public void RunGamePackageCandidateFactoryScriptExposesFactoryAndMatrixContract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-candidate-factory.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-candidate-factory.cmd"));

        Assert.Contains("[string]$TemplatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-FactoryTemplatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-FactoryOutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("TemplatePackagePath must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("TemplatePackagePath must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal130 output root", script, StringComparison.Ordinal);
        Assert.Contains("Refusing to write outside allowed Goal130 root", script, StringComparison.Ordinal);
        Assert.Contains("New-FactoryCandidatePackage", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-baseline", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-alchemy-route", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-combat-route", script, StringComparison.Ordinal);
        Assert.Contains("run-gamepackage-projection-matrix.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-CandidateIndexPath", script, StringComparison.Ordinal);
        Assert.Contains("-OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("-ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-candidate-factory-result.json", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-candidate-factory-dashboard.json", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-candidate-factory-log-scan.json", script, StringComparison.Ordinal);
        Assert.Contains("gamepackage-candidate-factory-negative-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("sourceTemplateSha256 = $templateHashBefore", script, StringComparison.Ordinal);
        Assert.Contains("samplePackageUnmodified", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-gamepackage-candidate-factory.ps1", cmd, StringComparison.Ordinal);
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
