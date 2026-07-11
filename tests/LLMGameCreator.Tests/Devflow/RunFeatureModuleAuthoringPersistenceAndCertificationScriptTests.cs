using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunFeatureModuleAuthoringPersistenceAndCertificationScriptTests
{
    [Fact]
    public void Script_exposes_required_parameters_guards_and_transactional_rollback()
    {
        var root = FindRoot();
        var path = Path.Combine(root, ".devflow", "scripts", "run-featuremodule-authoring-persistence-and-certification.ps1");
        var source = File.ReadAllText(path);
        foreach (var parameter in new[]
                 {
                     "$CatalogRoot", "$WorkspaceRoot", "$CertificationCacheRoot", "$CompositionId",
                     "$UnityPath", "$DryRun", "$ApplyCleanup"
                 })
            Assert.Contains(parameter, source, StringComparison.Ordinal);
        Assert.Contains("Goal147 refuses .llmgc/manual", source, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal147Directory", source, StringComparison.Ordinal);
        Assert.Contains("FeatureModuleAuthoringScriptProof", source, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeSavedFeatureModuleCompositionSmoke", source, StringComparison.Ordinal);
        Assert.Contains("GOAL147_FEATUREMODULE_AUTHORING_GREEN", source, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
