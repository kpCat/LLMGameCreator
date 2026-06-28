using System.Text.Json;
using LLMGameCreator.Application.Design.ModularGeneratorKernel;
using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class ProductSmokeScenarioManifestTests
{
    [Fact]
    public void ProductSmokeScenarioManifestsParseAndValidate()
    {
        var repoRoot = FindRepoRoot();
        var manifestRoot = Path.Combine(repoRoot, ".devflow", "product-smoke-scenarios");
        var scenarioIds = new[]
        {
            "modular-generator-kernel-readiness",
            "package-assembly-world-entities",
            "package-assembly-dialogue-quests"
        };

        foreach (var scenarioId in scenarioIds)
        {
            var path = Path.Combine(manifestRoot, scenarioId + ".json");
            var json = File.ReadAllText(path);
            using var parsed = JsonDocument.Parse(json);
            var manifest = ModularGeneratorKernelManifestReader.ReadProductSmokeScenarioManifestFromJson(json);
            var diagnostics = ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(manifest);

            Assert.Equal("product_smoke_scenario_manifest_v1", parsed.RootElement.GetProperty("schemaVersion").GetString());
            Assert.Equal(scenarioId, manifest.ScenarioId);
            Assert.NotEmpty(manifest.TestFilter);
            Assert.NotEmpty(manifest.ExpectedReportPath);
            Assert.False(manifest.IsProductVerticalGate);
            Assert.True(manifest.AllowedForModuleOnlyVerification);
            Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        }
    }

    [Fact]
    public void ProductSmokeRunnerUsesManifestFirstAndKeepsFallbackRoutes()
    {
        var repoRoot = FindRepoRoot();
        var script = File.ReadAllText(Path.Combine(repoRoot, ".devflow", "scripts", "run-product-smoke.ps1"));

        Assert.Contains(".devflow\\product-smoke-scenarios\\$Scenario.json", script, StringComparison.Ordinal);
        Assert.Contains("ConvertFrom-Json", script, StringComparison.Ordinal);
        Assert.Contains("$TestFilter = \"$($ScenarioManifest.testFilter)\"", script, StringComparison.Ordinal);
        Assert.Contains("Product smoke scenario manifest expectedReportPath was not produced", script, StringComparison.Ordinal);
        Assert.Contains("$Scenario -eq \"package-assembly-combat-progression\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("$Scenario -eq \"modular-generator-kernel-readiness\"", script, StringComparison.Ordinal);
    }

    [Fact]
    public void ScenarioManifestValidatorRejectsMissingFilterAndProductVerticalModuleOnlyConflict()
    {
        var missingFilter = new ProductSmokeScenarioManifest
        {
            SchemaVersion = "product_smoke_scenario_manifest_v1",
            ScenarioId = "bad",
            ArtifactRoot = ".llmgc/procedural/bad",
            OwnedModuleId = "bad",
            ExpectedReportPath = ".llmgc/procedural/bad/report.json"
        };
        var verticalConflict = missingFilter with
        {
            TestFilter = "FullyQualifiedName~Bad",
            IsProductVerticalGate = true,
            AllowedForModuleOnlyVerification = true
        };

        Assert.Contains(
            ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(missingFilter),
            item => item.Code == "product_smoke_manifest.test_filter.missing");
        Assert.Contains(
            ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(verticalConflict),
            item => item.Code == "product_smoke_manifest.product_vertical.module_only_conflict");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
