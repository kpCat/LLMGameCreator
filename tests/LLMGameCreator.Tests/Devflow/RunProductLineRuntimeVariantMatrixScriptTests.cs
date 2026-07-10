using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunProductLineRuntimeVariantMatrixScriptTests
{
    [Fact]
    public void RunProductLineRuntimeVariantMatrixScriptExposesGoal142Contract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-product-line-runtime-variant-matrix.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-product-line-runtime-variant-matrix.cmd"));

        Assert.Contains("[string]$TemplatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$VariantCatalogPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("must stay under repository root", script, StringComparison.Ordinal);
        Assert.Contains("Goal142 refuses .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the Goal142 output root", script, StringComparison.Ordinal);
        Assert.Contains("ProductLineRuntimeVariantMatrixScriptProof", script, StringComparison.Ordinal);
        Assert.Contains("Get-FileHash -Algorithm SHA256", script, StringComparison.Ordinal);
        Assert.Contains("goal142-script-", script, StringComparison.Ordinal);
        Assert.Contains("transaction backup must stay outside the repository", script, StringComparison.Ordinal);
        Assert.Contains("Copy-Goal142Directory -Source $ResolvedOutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("Copy-Goal142Directory -Source $ResolvedExportRoot", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal142Directory -Destination $ResolvedOutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal142Directory -Destination $ResolvedExportRoot", script, StringComparison.Ordinal);
        Assert.Contains("catch {", script, StringComparison.Ordinal);
        Assert.True(
            script.IndexOf("Copy-Goal142Directory -Source $ResolvedOutputRoot", StringComparison.Ordinal)
            < script.IndexOf("Remove-Goal142Directory -Path $ResolvedOutputRoot", StringComparison.Ordinal));
        Assert.True(
            script.IndexOf("Copy-Goal142Directory -Source $ResolvedExportRoot", StringComparison.Ordinal)
            < script.IndexOf("Remove-Goal142Directory -Path $ResolvedExportRoot", StringComparison.Ordinal));
        Assert.Contains("GOAL142_PRODUCT_LINE_RUNTIME_VARIANT_MATRIX_GREEN", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-balanced-baseline", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-alchemy-focus", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-combat-focus", script, StringComparison.Ordinal);
        Assert.Contains("minimal-map-game-exploration-resource-focus", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-product-line-runtime-variant-matrix.ps1", cmd, StringComparison.Ordinal);
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
