using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class RunGamePackageCandidateRecipePipelineScriptTests
{
    [Fact]
    public void RunGamePackageCandidateRecipePipelineScriptExposesCatalogScoringAndPromotionContract()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-candidate-recipe-pipeline.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "run-gamepackage-candidate-recipe-pipeline.cmd"));

        Assert.Contains("[string]$TemplatePackagePath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$RecipeCatalogPath", script, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-RecipeInputPath", script, StringComparison.Ordinal);
        Assert.Contains("Resolve-RecipeOutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("must stay under the repository root", script, StringComparison.Ordinal);
        Assert.Contains("must not point under .llmgc/manual", script, StringComparison.Ordinal);
        Assert.Contains("OutputRoot must stay under the Goal131 output root", script, StringComparison.Ordinal);
        Assert.Contains("Refusing to write outside allowed Goal131 root", script, StringComparison.Ordinal);
        Assert.Contains("candidate-recipe-catalog.json", script, StringComparison.Ordinal);
        Assert.Contains("balanced_baseline", script, StringComparison.Ordinal);
        Assert.Contains("alchemy_focus", script, StringComparison.Ordinal);
        Assert.Contains("combat_focus", script, StringComparison.Ordinal);
        Assert.Contains("exploration_focus", script, StringComparison.Ordinal);
        Assert.Contains("preservesFullPlaythroughIdentity", script, StringComparison.Ordinal);
        Assert.DoesNotContain("inventoryAdjustments", script, StringComparison.Ordinal);
        Assert.DoesNotContain("resourceAdjustments", script, StringComparison.Ordinal);
        Assert.DoesNotContain("questTuning", script, StringComparison.Ordinal);
        Assert.DoesNotContain("encounterTuning", script, StringComparison.Ordinal);
        Assert.Contains("run-gamepackage-projection-matrix.ps1", script, StringComparison.Ordinal);
        Assert.Contains("-CandidateIndexPath", script, StringComparison.Ordinal);
        Assert.Contains("-OutputRoot", script, StringComparison.Ordinal);
        Assert.Contains("-ApplyCleanup", script, StringComparison.Ordinal);
        Assert.Contains("Build-ScoringComponents", script, StringComparison.Ordinal);
        Assert.Contains("candidate-scoring-result.json", script, StringComparison.Ordinal);
        Assert.Contains("selected-candidate", script, StringComparison.Ordinal);
        Assert.Contains("selected-candidate-handoff.json", script, StringComparison.Ordinal);
        Assert.Contains("SelectedCandidatePackageFileName = \"package.json\"", script, StringComparison.Ordinal);
        Assert.Contains("sourceTemplateSha256 = $templateHashBefore", script, StringComparison.Ordinal);
        Assert.Contains("samplePackageUnmodified", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-WebRequest", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ComfyUI", script, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("run-gamepackage-candidate-recipe-pipeline.ps1", cmd, StringComparison.Ordinal);
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
