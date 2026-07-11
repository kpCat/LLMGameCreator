using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleCertification;

public sealed class FeatureModuleCertificationAndCoverageTests
{
    [Fact]
    public void Certification_executes_then_reuses_rejects_corruption_and_invalidates_contract_version()
    {
        var root = FindRoot();
        var library = Load(root);
        var cacheRoot = Temp("cache");
        var executionRoot = Temp("execution");
        try
        {
            var cache = new FeatureModuleCertificationCache(cacheRoot);
            var service = new FeatureModuleCertificationService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), cache, new FixedClock());
            var baseSha = BaselineSha(root);
            var first = service.Certify(root, library, baseSha, executionRoot);
            var second = service.Certify(root, library, baseSha, executionRoot);
            Assert.Equal("GREEN", first.Status);
            Assert.Equal(3, first.ExecutedCount);
            Assert.Equal(0, first.ReusedCount);
            Assert.Equal(0, second.ExecutedCount);
            Assert.Equal(3, second.ReusedCount);

            File.WriteAllText(cache.PathForModule(first.Entries[0].ModuleId), "{corrupt");
            var afterCorruption = service.Certify(root, library, baseSha, executionRoot);
            Assert.Equal(1, afterCorruption.ExecutedCount);
            Assert.Equal(2, afterCorruption.ReusedCount);
            Assert.Equal(1, afterCorruption.InvalidatedCount);
            Assert.True(afterCorruption.CorruptCacheRejected);

            var changedContract = service.Certify(root, library, baseSha, executionRoot, "product_line_runtime_qualifier_v2");
            Assert.Equal(3, changedContract.ExecutedCount);
            Assert.Equal(3, changedContract.InvalidatedCount);
            Assert.All(changedContract.Entries, entry => Assert.Equal("GREEN", entry.Status));
        }
        finally { Delete(cacheRoot); Delete(executionRoot); }
    }

    [Fact]
    public void Hundred_module_certification_plan_is_linear_and_interaction_rows_remain_bounded()
    {
        var source = Load(FindRoot());
        var modules = Enumerable.Range(0, 100).Select(index => new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.module_" + index.ToString("000"),
            Title = "Synthetic Module " + index.ToString("000"),
            Category = "synthetic",
            ModuleKind = "test",
            Selectable = true,
            ModuleVersion = "1.0.0"
        }).ToList();
        var catalog = source.Catalog with
        {
            OptionalProfileModuleCount = modules.Count,
            Modules = source.Catalog.Modules.Where(module => module.Required).Concat(modules).ToList()
        };
        var fingerprintService = new FeatureModuleLibraryFingerprintService();
        var fingerprints = catalog.Modules.ToDictionary(module => module.ModuleId,
            fingerprintService.ModuleFingerprint, StringComparer.Ordinal);
        var library = source with
        {
            Catalog = catalog,
            ModuleFingerprints = fingerprints,
            CatalogFingerprint = fingerprintService.CatalogFingerprint(fingerprints)
        };
        var certification = new FeatureModuleCertificationPlanner().Plan(
            library, new string('a', 64), FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
            string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan));
        var selected = new[] { modules[0].ModuleId, modules[^1].ModuleId };
        var coverage = new FeatureModuleCompositionCoveragePlanner().Plan(catalog, selected);

        Assert.Equal(100, certification.ModuleCount);
        Assert.Equal(100, certification.Modules.Select(item => item.ModuleId).Distinct(StringComparer.Ordinal).Count());
        Assert.False(coverage.FullPowersetEnumerated);
        Assert.True(coverage.GeneratedCompositionCount <= 24);
        Assert.True(coverage.SelectedCompositionIncluded);
        Assert.True(coverage.BaselineIncluded);
        Assert.Equal(0, coverage.SingletonCoverageCount);
        Assert.Equal("2^100", coverage.TheoreticalPowersetSize);
    }

    [Fact]
    public void Exhaustive_small_catalog_classifies_incompatible_rows_without_execution()
    {
        var source = Load(FindRoot());
        var first = new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.first",
            Title = "First",
            Category = "synthetic",
            ModuleKind = "test",
            Selectable = true,
            Conflicts = ["feature.synthetic.second"]
        };
        var second = new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.second",
            Title = "Second",
            Category = "synthetic",
            ModuleKind = "test",
            Selectable = true,
            Conflicts = ["feature.synthetic.first"]
        };
        var catalog = source.Catalog with
        {
            OptionalProfileModuleCount = 2,
            Modules = source.Catalog.Modules.Where(module => module.Required).Concat([first, second]).ToList()
        };
        var plan = new FeatureModuleCompositionCoveragePlanner().Plan(catalog, [first.ModuleId]);
        Assert.True(plan.FullPowersetEnumerated);
        Assert.Equal(3, plan.GeneratedCompositionCount);
        var rejected = Assert.Single(plan.RejectedCompositions);
        Assert.Equal(2, rejected.ModuleIds.Count);
        Assert.Contains(rejected.Diagnostics, item => item.Contains("declared conflict rejected", StringComparison.Ordinal));
    }

    private static FeatureModuleLibrarySnapshot Load(string root) =>
        new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));

    private static string BaselineSha(string root)
    {
        var path = Path.Combine(root, ".llmgc", "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "product-line-runtime-variant-matrix-result.json");
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var row = json.RootElement.GetProperty("candidates").EnumerateArray()
            .Single(item => item.GetProperty("candidateId").GetString() == "minimal-map-game-balanced-baseline");
        return row.GetProperty("packageSha256").GetString()!;
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

    private static string Temp(string name) => Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-" + name + "-" + Guid.NewGuid().ToString("N"));
    private static void Delete(string path) { if (Directory.Exists(path)) Directory.Delete(path, true); }

    private sealed class FixedClock : IFeatureModuleAuthoringClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 11, 10, 0, 0, TimeSpan.Zero);
    }
}
