using System.Text.Json;
using LLMGameCreator.Tests.Devflow;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DevelopmentComplexityStabilizationProductSmokeTests
{
    [Fact]
    public void DevelopmentComplexityStabilizationProductSmoke()
    {
        var repoRoot = DevelopmentComplexityStabilizationArtifacts.FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);

        var written = DevelopmentComplexityStabilizationArtifacts.WriteArtifacts(repoRoot, projectRoot);

        Assert.True(File.Exists(Path.Combine(repoRoot, "docs", "DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md")));
        Assert.True(File.Exists(Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json")));
        Assert.True(File.Exists(Path.Combine(repoRoot, ".devflow", "scripts", "check-artifact-scope.ps1")));
        Assert.True(File.Exists(written.ReportJsonPath));
        Assert.True(File.Exists(written.ReportMarkdownPath));
        Assert.True(File.Exists(written.VerificationMarkdownPath));
        Assert.True(File.Exists(written.PolicyProofJsonPath));
        Assert.True(File.Exists(written.InventoryJsonPath));
        Assert.True(File.Exists(written.InvalidMatrixJsonPath));
        Assert.True(File.Exists(written.CheckAllIsolationProofJsonPath));

        using var report = JsonDocument.Parse(File.ReadAllText(written.ReportJsonPath));
        var root = report.RootElement;
        Assert.False(root.GetProperty("accepted").GetBoolean());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.FinalGate, root.GetProperty("finalStatus").GetString());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.FinalGate, root.GetProperty("manualGate").GetString());
        Assert.Equal(DevelopmentComplexityStabilizationArtifacts.PreviousAcceptedGate, root.GetProperty("previousAcceptedGate").GetString());
        Assert.True(root.GetProperty("scopeGuardImplemented").GetBoolean());
        Assert.True(root.GetProperty("checkAllArtifactIsolationImplemented").GetBoolean());
        Assert.True(root.GetProperty("legacyArtifactMutationGuarded").GetBoolean());
        Assert.True(root.GetProperty("trackedGeneratedArtifactInventoryWritten").GetBoolean());
        Assert.False(root.GetProperty("capabilitySelectionStarted").GetBoolean());
        Assert.False(root.GetProperty("publicGamePackageSchemaChanged").GetBoolean());
        Assert.False(root.GetProperty("projectFilesChanged").GetBoolean());
        Assert.False(root.GetProperty("generatorLibraryChanged").GetBoolean());
        Assert.False(root.GetProperty("unityBuildExecuted").GetBoolean());
        Assert.True(root.GetProperty("noExternalProviderLlmRagLuaMedia").GetBoolean());
        Assert.DoesNotContain(root.GetProperty("diagnostics").EnumerateArray(), item =>
            item.GetProperty("severity").GetString() == "error");

        using var invalidMatrix = JsonDocument.Parse(File.ReadAllText(written.InvalidMatrixJsonPath));
        Assert.True(invalidMatrix.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalidMatrix.RootElement.GetProperty("scenarioCount").GetInt32() >= 12);
        Assert.Equal(
            invalidMatrix.RootElement.GetProperty("scenarioCount").GetInt32(),
            invalidMatrix.RootElement.GetProperty("rejectedCount").GetInt32());

        using var inventory = JsonDocument.Parse(File.ReadAllText(written.InventoryJsonPath));
        Assert.True(inventory.RootElement.GetProperty("trackedFileCount").GetInt32() >= 0);
        Assert.Contains("Broad cleanup and untracking are deferred", inventory.RootElement.GetProperty("cleanupDeferredNote").GetString());
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }
}
