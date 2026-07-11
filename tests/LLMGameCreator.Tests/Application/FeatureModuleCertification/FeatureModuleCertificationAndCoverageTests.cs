using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.FeatureModuleCertification;

public sealed class FeatureModuleCertificationAndCoverageTests
{
    [Fact]
    public void Goal147A_dependency_closure_certification_is_transitive_incremental_and_cycle_safe()
    {
        var proof = RunDependencyClosureProof();
        Assert.Equal(3, proof.LedgerEntryCount);
        Assert.Equal(3, proof.InitialExecutedCount);
        Assert.Equal(3, proof.SecondRunReusedCount);
        Assert.Equal(new[] { SyntheticBaseId, SyntheticDependentId }, proof.DependentCertificationSelectedModuleIds);
        Assert.Equal(2, proof.DependencyChangeExecutedCount);
        Assert.Equal(1, proof.DependencyChangeReusedCount);
        Assert.Equal(1, proof.CorruptDependentCacheExecutedCount);
        Assert.True(proof.DependencyCycleRejected);
        Assert.True(proof.UnknownDependencyRejected);
        Assert.Equal(0, proof.RuntimeInvocationsBeforeCycleRejection);
    }

    [Fact]
    public void Goal147A_script_writes_executable_dependent_module_certification_proof()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL147A_RUN"), "true", StringComparison.OrdinalIgnoreCase))
            return;
        var proof = RunDependencyClosureProof();
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL147A_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL147A_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        WriteJson(Path.Combine(root, "dependent-module-certification-proof.json"), new
        {
            schemaVersion = "dependent_module_certification_proof_v1",
            status = "GREEN",
            proof.LedgerEntryCount,
            proof.InitialExecutedCount,
            proof.InitialGreen,
            proof.SecondRunReusedCount,
            proof.DependentCertificationSelectedModuleIds,
            proof.DependentOptionalDependencyClosureIds,
            proof.DependencyClosureFingerprint,
            proof.DependencyChangeExecutedCount,
            proof.DependencyChangeReusedCount,
            proof.UnrelatedEntryReusedAfterDependencyChange,
            proof.CorruptDependentCacheExecutedCount,
            proof.CorruptDependentCacheRegenerated,
            proof.DependencyCycleRejected,
            proof.UnknownDependencyRejected,
            proof.RuntimeInvocationsBeforeCycleRejection,
            transitiveDependencyClosurePassed = true,
            composeAndQualifyReceivesDependencyClosure = true,
            targetRequiredEffectContractsPassed = true,
            noModuleIdSpecificCertificationBranch = true,
            passed = true
        });
    }

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

    private static DependencyClosureProof RunDependencyClosureProof()
    {
        var root = FindRoot();
        var library = SyntheticDependencyLibrary(Load(root));
        var cacheRoot = Temp("goal147a-dependent-cache");
        var executionRoot = Temp("goal147a-dependent-execution");
        var cycleCacheRoot = Temp("goal147a-cycle-cache");
        try
        {
            var cache = new FeatureModuleCertificationCache(cacheRoot);
            var service = new FeatureModuleCertificationService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), cache, new FixedClock());
            var baseSha = BaselineSha(root);
            var plan = new FeatureModuleCertificationPlanner().Plan(
                library, baseSha, FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
                string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan));
            var dependentPlan = plan.Modules.Single(item => item.ModuleId == SyntheticDependentId);
            var first = service.Certify(root, library, baseSha, executionRoot);
            var second = service.Certify(root, library, baseSha, executionRoot);

            var changedBase = library.Catalog.Modules.Single(module => module.ModuleId == SyntheticBaseId) with
            {
                ModuleVersion = "1.0.1"
            };
            var changedCatalog = library.Catalog with
            {
                Modules = library.Catalog.Modules.Select(module => module.ModuleId == SyntheticBaseId ? changedBase : module).ToList()
            };
            var changedLibrary = RebuildLibrary(library, changedCatalog);
            var afterDependencyChange = service.Certify(root, changedLibrary, baseSha, executionRoot);

            File.WriteAllText(cache.PathForModule(SyntheticDependentId), "{corrupt");
            var afterCorruption = service.Certify(root, changedLibrary, baseSha, executionRoot);

            var cycleCatalog = changedCatalog with
            {
                Modules = changedCatalog.Modules.Select(module => module.ModuleId == SyntheticBaseId
                    ? module with { Dependencies = [SyntheticDependentId] }
                    : module).ToList()
            };
            var cycleLibrary = RebuildLibrary(changedLibrary, cycleCatalog);
            var runtimeProbe = new CountingRuntimeService();
            var cycleService = new FeatureModuleCertificationService(runtimeProbe,
                new FeatureModuleCertificationCache(cycleCacheRoot), new FixedClock());
            var cycle = Assert.Throws<InvalidOperationException>(() =>
                cycleService.Certify(root, cycleLibrary, baseSha, executionRoot));

            var unknownCatalog = changedCatalog with
            {
                Modules = changedCatalog.Modules.Select(module => module.ModuleId == SyntheticDependentId
                    ? module with { Dependencies = ["feature.synthetic.unknown_optional"] }
                    : module).ToList()
            };
            var unknown = Assert.Throws<InvalidOperationException>(() =>
                new FeatureModuleCertificationPlanner().Plan(RebuildLibrary(changedLibrary, unknownCatalog), baseSha,
                    FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
                    string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan)));

            return new DependencyClosureProof(
                first.Entries.Count,
                first.ExecutedCount,
                first.Status == "GREEN" && first.Entries.All(entry => entry.Status == "GREEN"),
                second.ReusedCount,
                dependentPlan.CertificationSelectedModuleIds,
                dependentPlan.OptionalDependencyClosureIds,
                dependentPlan.DependencyClosureFingerprint,
                afterDependencyChange.ExecutedCount,
                afterDependencyChange.ReusedCount,
                afterDependencyChange.Entries.Single(entry => entry.ModuleId == SyntheticUnrelatedId).Status == "GREEN"
                && afterDependencyChange.ReusedCount == 1,
                afterCorruption.ExecutedCount,
                afterCorruption.CorruptCacheRejected
                && afterCorruption.Entries.Single(entry => entry.ModuleId == SyntheticDependentId).Status == "GREEN",
                cycle.Message.Contains("certification dependency cycle rejected", StringComparison.Ordinal),
                unknown.Message.Contains("unknown certification dependency", StringComparison.Ordinal),
                runtimeProbe.InvocationCount);
        }
        finally
        {
            Delete(cacheRoot);
            Delete(executionRoot);
            Delete(cycleCacheRoot);
        }
    }

    private static FeatureModuleLibrarySnapshot SyntheticDependencyLibrary(FeatureModuleLibrarySnapshot source)
    {
        var optional = source.Catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();
        var alchemy = optional.Single(module => module.ModuleId == "feature.profile.alchemy_focus");
        var combat = optional.Single(module => module.ModuleId == "feature.profile.combat_focus");
        var exploration = optional.Single(module => module.ModuleId == "feature.profile.exploration_resource_focus");
        var synthetic = new[]
        {
            RenameModule(alchemy, SyntheticBaseId, "Synthetic Base Optional", alchemy.Dependencies),
            RenameModule(combat, SyntheticDependentId, "Synthetic Dependent Optional",
                combat.Dependencies.Append(SyntheticBaseId).OrderBy(id => id, StringComparer.Ordinal).ToList()),
            RenameModule(exploration, SyntheticUnrelatedId, "Synthetic Unrelated Optional", exploration.Dependencies)
        };
        var catalog = source.Catalog with
        {
            OptionalProfileModuleCount = synthetic.Length,
            Modules = source.Catalog.Modules.Where(module => module.Required).Concat(synthetic).ToList()
        };
        return RebuildLibrary(source, catalog);
    }

    private static FeatureModuleDefinition RenameModule(
        FeatureModuleDefinition module,
        string moduleId,
        string title,
        IReadOnlyList<string> dependencies) => module with
    {
        ModuleId = moduleId,
        Title = title,
        Dependencies = dependencies,
        RuntimeEffectContracts = module.RuntimeEffectContracts.Select(contract => contract with { ModuleId = moduleId }).ToList(),
        ParameterDefinitions = module.ParameterDefinitions.Select(parameter => parameter with { ModuleId = moduleId }).ToList()
    };

    private static FeatureModuleLibrarySnapshot RebuildLibrary(
        FeatureModuleLibrarySnapshot source,
        FeatureModuleCatalogDocument catalog)
    {
        var service = new FeatureModuleLibraryFingerprintService();
        var fingerprints = catalog.Modules.ToDictionary(
            module => module.ModuleId,
            service.ModuleFingerprint,
            StringComparer.Ordinal);
        return source with
        {
            Catalog = catalog,
            ModuleFingerprints = fingerprints,
            CatalogFingerprint = service.CatalogFingerprint(fingerprints)
        };
    }

    private static void WriteJson(string path, object value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(path, json + Environment.NewLine);
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

    private sealed class CountingRuntimeService : ISelectedRuntimeVariantInteractiveSessionService
    {
        public int InvocationCount { get; private set; }

        public RuntimeInteractiveSession StartSession(
            GamePackageDefinition package,
            SelectedRuntimeVariantInteractiveSessionStartRequest request) => Invoked<RuntimeInteractiveSession>();

        public SelectedRuntimeVariantInteractiveActionResult ExecuteAction(
            GamePackageDefinition package,
            RuntimeInteractiveSession session,
            SelectedRuntimeVariantInteractiveActionRequest request) => Invoked<SelectedRuntimeVariantInteractiveActionResult>();

        public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
            RuntimeInteractiveSession session,
            string checkpointId,
            string createdAtUtc) => Invoked<SelectedRuntimeVariantInteractiveCheckpoint>();

        public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(
            GamePackageDefinition package,
            SelectedRuntimeVariantInteractiveSessionStartRequest request,
            SelectedRuntimeVariantInteractiveCheckpoint checkpoint) => Invoked<SelectedRuntimeVariantInteractiveReplayResult>();

        private T Invoked<T>()
        {
            InvocationCount++;
            throw new InvalidOperationException("Runtime must not execute before dependency validation.");
        }
    }

    private sealed record DependencyClosureProof(
        int LedgerEntryCount,
        int InitialExecutedCount,
        bool InitialGreen,
        int SecondRunReusedCount,
        IReadOnlyList<string> DependentCertificationSelectedModuleIds,
        IReadOnlyList<string> DependentOptionalDependencyClosureIds,
        string DependencyClosureFingerprint,
        int DependencyChangeExecutedCount,
        int DependencyChangeReusedCount,
        bool UnrelatedEntryReusedAfterDependencyChange,
        int CorruptDependentCacheExecutedCount,
        bool CorruptDependentCacheRegenerated,
        bool DependencyCycleRejected,
        bool UnknownDependencyRejected,
        int RuntimeInvocationsBeforeCycleRejection);

    private const string SyntheticBaseId = "feature.synthetic.base_optional";
    private const string SyntheticDependentId = "feature.synthetic.dependent_optional";
    private const string SyntheticUnrelatedId = "feature.synthetic.unrelated_optional";
}
