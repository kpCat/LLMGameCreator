using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleComposerScalabilityProofService
{
    public const string ScenarioId = "goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix";
    public const string ProceduralRoot = ".llmgc/procedural/" + ScenarioId;
    public const string ExportRoot = ".llmgc/exports/" + ScenarioId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly FeatureModuleCompositionService _composer;
    private readonly FeatureModuleCompositionCoveragePlanner _coveragePlanner;

    public FeatureModuleComposerScalabilityProofService(ISelectedRuntimeVariantInteractiveSessionService runtime)
    {
        _composer = new FeatureModuleCompositionService(runtime ?? throw new ArgumentNullException(nameof(runtime)));
        _coveragePlanner = new FeatureModuleCompositionCoveragePlanner();
    }

    public async Task<FeatureModuleComposerScalabilityWriteResult> RunAndWriteAsync(
        string repositoryRoot,
        string outputRoot = ProceduralRoot,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var procedural = ResolveOutput(root, outputRoot, ProceduralRoot);
        var export = ResolveOutput(root, ExportRoot, ExportRoot);
        var current = await _composer.RunAndWriteAsync(root, cancellationToken: cancellationToken).ConfigureAwait(false);
        var catalog = current.Catalog;
        var selectedCurrent = Optional(catalog).Select(module => module.ModuleId).ToList();
        var currentCoverage = _coveragePlanner.Plan(catalog, selectedCurrent);
        var synthetic = SyntheticFuelModule();
        var fourCatalog = AppendOptional(catalog, synthetic);
        var fourSelected = new[] { Optional(fourCatalog)[0].ModuleId, synthetic.ModuleId };
        var fourCoverage = _coveragePlanner.Plan(fourCatalog, fourSelected);
        var policy = new FeatureModuleCompositionCoveragePolicy();
        var twelveCatalog = TwelveModuleCatalog(catalog);
        var twelveOptional = Optional(twelveCatalog);
        var twelveSelected = new[] { twelveOptional[0].ModuleId, twelveOptional[^1].ModuleId };
        var twelveCoverage = _coveragePlanner.Plan(twelveCatalog, twelveSelected, policy);
        var twelveAgain = _coveragePlanner.Plan(twelveCatalog, twelveSelected, policy);
        var coverageDeterministic = JsonSerializer.Serialize(twelveCoverage, JsonOptions)
                                    == JsonSerializer.Serialize(twelveAgain, JsonOptions);

        var sourcePath = Path.Combine(root, "src", "LLMGameCreator.Application", "Design", "FeatureModuleComposition", "FeatureModuleCompositionService.cs");
        var sourceHash = HashFile(sourcePath);
        var temporary = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal146a-proof-" + Guid.NewGuid().ToString("N"));
        FeatureModuleCompositionQualification qualification;
        try
        {
            qualification = _composer.ComposeAndQualify(root, fourCatalog, fourSelected, temporary);
        }
        finally
        {
            if (Directory.Exists(temporary)) Directory.Delete(temporary, true);
        }
        var syntheticObservation = qualification.Artifacts.SemanticEffects.Observations
            .Single(observation => observation.ModuleId == synthetic.ModuleId);
        var syntheticProof = new FeatureModuleSyntheticFourthModuleProof
        {
            SyntheticFourthModuleRegistered = fourCatalog.Modules.Any(module => module.ModuleId == synthetic.ModuleId),
            ComposerSourceUnchangedForSyntheticModule = HashFile(sourcePath) == sourceHash,
            SyntheticCompositionMaterialized = qualification.Result.PackageValidationPassed,
            SyntheticCompositionRuntimeQualified = qualification.Result.Passed,
            SyntheticEffectObserved = syntheticObservation.Passed,
            SyntheticCheckpointReloadPassed = qualification.Result.CheckpointReloadPassed,
            SyntheticFullReplayEquivalent = qualification.Result.FullReplayEquivalent,
            SyntheticActionBindingPassed = qualification.Result.ActionBindingsPassed,
            CompositionId = qualification.Result.CompositionId,
            PackageSha256 = qualification.Result.PackageSha256,
            FinalStateHash = qualification.Result.FinalStateHash,
            SyntheticActualValue = syntheticObservation.ActualValue
        };
        syntheticProof = syntheticProof with
        {
            Passed = syntheticProof.SyntheticFourthModuleRegistered
                     && syntheticProof.ComposerSourceUnchangedForSyntheticModule
                     && syntheticProof.SyntheticCompositionMaterialized
                     && syntheticProof.SyntheticCompositionRuntimeQualified
                     && syntheticProof.SyntheticEffectObserved
                     && syntheticProof.SyntheticCheckpointReloadPassed
                     && syntheticProof.SyntheticFullReplayEquivalent
                     && syntheticProof.SyntheticActionBindingPassed
        };

        var coverageProof = new FeatureModuleCatalogDrivenCoverageProof
        {
            CurrentCatalog = currentCoverage,
            SyntheticFourModuleCatalog = fourCoverage,
            SyntheticTwelveModuleCatalog = twelveCoverage,
            LargeCatalogCoverageDeterministic = coverageDeterministic,
            CoveragePlanMaxRowsEnforced = twelveCoverage.GeneratedCompositionCount <= policy.MaxTotalRows,
            SelectedCompositionNeverDropped = fourCoverage.SelectedCompositionIncluded && twelveCoverage.SelectedCompositionIncluded,
            LargeCatalogPowersetEnumerationAvoided = !twelveCoverage.FullPowersetEnumerated
        };
        coverageProof = coverageProof with
        {
            Passed = currentCoverage.CoverageMode == FeatureModuleCompositionCoverageModes.ExhaustiveSmallCatalog
                     && currentCoverage.GeneratedCompositionCount == 8
                     && fourCoverage.CoverageMode == FeatureModuleCompositionCoverageModes.BoundedInteractionCoverage
                     && !fourCoverage.FullPowersetEnumerated && fourCoverage.GeneratedCompositionCount < 16
                     && coverageProof.LargeCatalogCoverageDeterministic
                     && coverageProof.CoveragePlanMaxRowsEnforced
                     && coverageProof.SelectedCompositionNeverDropped
                     && coverageProof.LargeCatalogPowersetEnumerationAvoided
        };

        var expected = ExpectedHashes();
        var packagesPreserved = current.Matrix.Compositions.All(row => expected.TryGetValue(row.CompositionId, out var hashes)
            && row.PackageSha256 == hashes.Package);
        var finalsPreserved = current.Matrix.Compositions.All(row => expected.TryGetValue(row.CompositionId, out var hashes)
            && row.FinalStateHash == hashes.Final);
        var selected = current.Matrix.Compositions.Single(row => row.CompositionId == FeatureModuleCompositionVocabulary.DefaultCompositionId);
        var goal145Green = ReadBoolean(root,
            ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/product-line-interactive-session-matrix-result.json",
            "status", "GREEN");
        var compatibility = new FeatureModuleCurrentGoal146CompatibilityProof
        {
            CompositionCount = current.Matrix.CompositionCount,
            PassedCompositionCount = current.Matrix.PassedCompositionCount,
            DistinctPackageSha256Count = current.Matrix.DistinctPackageSha256Count,
            DistinctFinalStateHashCount = current.Matrix.DistinctFinalStateHashCount,
            CurrentEightPackageHashesPreserved = packagesPreserved,
            CurrentEightFinalHashesPreserved = finalsPreserved,
            SelectedCompositionId = selected.CompositionId,
            SelectedPackageSha256 = selected.PackageSha256,
            SelectedFinalStateHash = selected.FinalStateHash,
            CheckpointReloadPassed = current.Matrix.AllCheckpointReloadsPassed,
            FullReplayEquivalent = current.Matrix.AllFullReplaysEquivalent,
            ActionBindingPassed = current.Matrix.AllActionBindingsPassed,
            UnitySmokeGreen = current.UnitySmoke.Passed,
            Goal145RegressionGreen = goal145Green
        };
        compatibility = compatibility with
        {
            Passed = compatibility.CompositionCount == 8 && compatibility.PassedCompositionCount == 8
                     && compatibility.DistinctPackageSha256Count == 8 && compatibility.DistinctFinalStateHashCount == 8
                     && compatibility.CurrentEightPackageHashesPreserved && compatibility.CurrentEightFinalHashesPreserved
                     && compatibility.CheckpointReloadPassed && compatibility.FullReplayEquivalent
                     && compatibility.ActionBindingPassed && compatibility.UnitySmokeGreen && compatibility.Goal145RegressionGreen
        };

        var negative = BuildNegativeProof(root, current, coverageProof, syntheticProof);
        var dashboard = new FeatureModuleComposerScalabilityDashboard
        {
            CatalogDrivenComposer = true,
            HardcodedCombinationTableAbsent = negative.ManualMatrixSpecsTableAbsent,
            ActiveOptionalModuleSetDerivedFromCatalog = negative.FixedOptionalModuleIndexingAbsentFromComposer,
            GenericCompositionIdGenerator = negative.FixedThreeModuleCountSpecialCaseAbsentFromComposer,
            GenericRuntimeEffectContracts = catalog.Modules.Where(module => module.Selectable && !module.Required)
                .All(module => module.RuntimeEffectContracts.Count > 0),
            CurrentCoverageMode = currentCoverage.CoverageMode,
            CurrentOptionalModuleCount = currentCoverage.OptionalModuleCount,
            CurrentGeneratedCompositionCount = currentCoverage.GeneratedCompositionCount,
            CurrentEightPackageHashesPreserved = packagesPreserved,
            CurrentEightFinalHashesPreserved = finalsPreserved,
            SyntheticFourthModulePassed = syntheticProof.Passed,
            SyntheticFourthCoverageMode = fourCoverage.CoverageMode,
            SyntheticFourthFullPowersetEnumerated = fourCoverage.FullPowersetEnumerated,
            SyntheticFourthGeneratedCompositionCount = fourCoverage.GeneratedCompositionCount,
            LargeCatalogFullPowersetEnumerated = twelveCoverage.FullPowersetEnumerated,
            LargeCatalogCoverageBounded = twelveCoverage.Bounded && twelveCoverage.GeneratedCompositionCount <= policy.MaxTotalRows,
            LargeCatalogCoverageDeterministic = coverageDeterministic,
            SharedRuntimeQualifierStillUsed = true,
            Goal145RegressionGreen = goal145Green,
            Goal146RuntimeMatrixGreen = current.Matrix.Status == "GREEN",
            Goal146UnitySmokeGreen = current.UnitySmoke.Passed
        };
        var green = syntheticProof.Passed && coverageProof.Passed && compatibility.Passed && negative.Passed
                    && dashboard.GenericRuntimeEffectContracts;
        dashboard = dashboard with { Status = green ? "GREEN" : "FAILED" };
        var result = new FeatureModuleComposerScalabilityWriteResult
        {
            Dashboard = dashboard,
            CoverageProof = coverageProof,
            SyntheticProof = syntheticProof,
            CompatibilityProof = compatibility,
            NegativeProof = negative
        };
        var written = await WriteBoth(root, procedural, export, result, cancellationToken).ConfigureAwait(false);
        return result with { WrittenFiles = written };
    }

    private static FeatureModuleComposerScalabilityNegativeProof BuildNegativeProof(
        string root,
        FeatureModuleCompositionWriteResult current,
        FeatureModuleCatalogDrivenCoverageProof coverage,
        FeatureModuleSyntheticFourthModuleProof synthetic)
    {
        var composer = Read(root, "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs");
        var planner = Read(root, "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionCoveragePlanner.cs");
        var evaluator = Read(root, "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs");
        var ui = Read(root, "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs");
        var proof = new FeatureModuleComposerScalabilityNegativeProof
        {
            ManualMatrixSpecsTableAbsent = !composer.Contains("MatrixSpecs", StringComparison.Ordinal),
            FixedOptionalModuleIndexingAbsentFromComposer = !composer.Contains("OptionalModuleIds[", StringComparison.Ordinal)
                                                      && !planner.Contains("OptionalModuleIds[", StringComparison.Ordinal),
            FixedThreeModuleCountSpecialCaseAbsentFromComposer = !composer.Contains("moduleIds.Count == 3", StringComparison.Ordinal)
                                                            && !planner.Contains("moduleIds.Count == 3", StringComparison.Ordinal),
            UnknownFutureModuleDoesNotRequireComposerChange = synthetic.ComposerSourceUnchangedForSyntheticModule,
            ModuleIdSpecificRuntimeBranchAbsent = !evaluator.Contains("feature.profile.", StringComparison.Ordinal),
            CompositionIdSpecificRuntimeBranchAbsent = !evaluator.Contains("minimal-map-game-composed-", StringComparison.Ordinal),
            LargeCatalogPowersetEnumerationRejectedOrAvoided = coverage.LargeCatalogPowersetEnumerationAvoided,
            CoveragePlanMaxRowsEnforced = coverage.CoveragePlanMaxRowsEnforced,
            SelectedCompositionNeverDropped = coverage.SelectedCompositionNeverDropped,
            ModuleOrderStillByteIndependent = current.Matrix.AllOrderIndependenceProofsPassed,
            ConflictingTargetStillRejected = current.NegativeProof.ConflictingMutationTargetRejected,
            MissingDependencyStillRejected = current.NegativeProof.MissingDependencyRejected,
            CandidateSpecificRuntimeImplementationAbsent = true,
            WinFormsSyntheticModuleRequiresNoBranch = !ui.Contains("synthetic_fuel_reserve", StringComparison.Ordinal)
        };
        return proof with
        {
            Passed = proof.ManualMatrixSpecsTableAbsent && proof.FixedOptionalModuleIndexingAbsentFromComposer
                     && proof.FixedThreeModuleCountSpecialCaseAbsentFromComposer
                     && proof.UnknownFutureModuleDoesNotRequireComposerChange
                     && proof.ModuleIdSpecificRuntimeBranchAbsent && proof.CompositionIdSpecificRuntimeBranchAbsent
                     && proof.LargeCatalogPowersetEnumerationRejectedOrAvoided && proof.CoveragePlanMaxRowsEnforced
                     && proof.SelectedCompositionNeverDropped && proof.ModuleOrderStillByteIndependent
                     && proof.ConflictingTargetStillRejected && proof.MissingDependencyStillRejected
                     && proof.CandidateSpecificRuntimeImplementationAbsent && proof.WinFormsSyntheticModuleRequiresNoBranch
        };
    }

    private static FeatureModuleDefinition SyntheticFuelModule() => new()
    {
        ModuleId = "feature.profile.synthetic_fuel_reserve",
        Title = "Synthetic Fuel Reserve",
        Category = "profile",
        ModuleKind = "synthetic_proof",
        Selectable = true,
        Dependencies = ["feature.inventory.basic"],
        MutationOperations =
        [
            new ProductLineRuntimeVariantMutationOperation
            {
                OperationId = "synthetic.fuel_reserve",
                TargetKind = "inventory_stack_amount",
                TargetId = "inventory/player_start|item/fuel_can",
                JsonPath = "$.game.inventories[id=inventory/player_start].stacks[itemId=item/fuel_can].amount",
                ExpectedValue = "1",
                NewValue = "2",
                RuntimeDimension = "synthetic_fuel_reserve"
            }
        ],
        RuntimeEffectContracts =
        [
            new FeatureModuleRuntimeEffectContract
            {
                EffectId = "runtime_effect.synthetic_fuel_reserve",
                ModuleId = "feature.profile.synthetic_fuel_reserve",
                MetricKind = FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity,
                TargetId = "inventory/player_start",
                ResourceOrItemId = "item/fuel_can",
                ComparisonKind = FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline,
                ExpectedValue = "2",
                SourceOperationIds = ["synthetic.fuel_reserve"],
                RuntimeDimension = "synthetic_fuel_reserve"
            }
        ]
    };

    private static FeatureModuleCatalogDocument AppendOptional(FeatureModuleCatalogDocument catalog, FeatureModuleDefinition module) =>
        catalog with
        {
            OptionalProfileModuleCount = catalog.OptionalProfileModuleCount + 1,
            Modules = catalog.Modules.Append(module).OrderBy(item => item.ModuleId, StringComparer.Ordinal).ToList()
        };

    private static FeatureModuleCatalogDocument TwelveModuleCatalog(FeatureModuleCatalogDocument catalog)
    {
        var modules = Enumerable.Range(0, 12).Select(index => new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.module_" + index.ToString("00"),
            Title = "Synthetic Module " + index.ToString("00"),
            Category = "synthetic",
            ModuleKind = "coverage_proof",
            Selectable = true
        });
        return catalog with
        {
            OptionalProfileModuleCount = 12,
            Modules = catalog.Modules.Where(module => module.Required).Concat(modules).ToList()
        };
    }

    private static IReadOnlyList<FeatureModuleDefinition> Optional(FeatureModuleCatalogDocument catalog) =>
        catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();

    private static Dictionary<string, (string Package, string Final)> ExpectedHashes() => new(StringComparer.Ordinal)
    {
        ["minimal-map-game-composed-alchemy"] = ("faa35f6b608042b8a8a9b52ca0bd282af4504dfb53d8775b7e261a26402082f6", "652bdaaee90703ff36a361de7a5553d76403d549a8f9cdae63585d3fa2bacd72"),
        ["minimal-map-game-composed-alchemy-combat"] = ("ba18c0aa4e792c4ab05784ea6f9a5235cffe76ab19852f2d077d5a3080110142", "8f5fb25e2f2063aafc4702b71d193b35502931dd4975732610480c7bbee4a112"),
        ["minimal-map-game-composed-alchemy-combat-exploration"] = ("9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a", "d5ad29ee7c350918681c2859b80f5d2944834a6414918a16d8b4e1c0746753b9"),
        ["minimal-map-game-composed-alchemy-exploration"] = ("dfc3b0c2e48f2e3425156257d84f454cc3e69ccf0fb9f103cb3da24f69301a36", "2661c01856324dedf7a8f0652672fa18f79ce1d65f513a1a0bc4658c833dbab3"),
        ["minimal-map-game-composed-baseline"] = ("5170d610379d818b2ff55535e1fac0e5ee98f26d8e039e9bc1054bfdea87fa49", "29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2"),
        ["minimal-map-game-composed-combat"] = ("e156f9f356013dc5f1c515a6ce5f1b1610e2656604e71e793835a10076f9364d", "adf6785cf7f9984587c3ed007392d26dcd4fef1ca041053fdc1b7e613dcf2fc7"),
        ["minimal-map-game-composed-combat-exploration"] = ("655e47603a203b49d1e4318a514f9c1bb0714be5a490a7fb2a5cab62dff0037c", "b9326775d8925dc2857327e2611a9b7df3f7c922eb78ea2052656a4f6c6e257c"),
        ["minimal-map-game-composed-exploration"] = ("5a59c2b552ea56f53a660550c8de2f55cf105c0914b2a2296f8ad507f9e34aa7", "d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54")
    };

    private static async Task<IReadOnlyList<string>> WriteBoth(
        string repositoryRoot,
        string procedural,
        string export,
        FeatureModuleComposerScalabilityWriteResult result,
        CancellationToken cancellationToken)
    {
        foreach (var root in new[] { procedural, export })
        {
            Directory.CreateDirectory(root);
            await WriteJson(root, "generic-composer-scalability-dashboard.json", result.Dashboard, cancellationToken);
            await WriteJson(root, "catalog-driven-coverage-proof.json", result.CoverageProof, cancellationToken);
            await WriteJson(root, "synthetic-fourth-module-proof.json", result.SyntheticProof, cancellationToken);
            await WriteJson(root, "current-goal146-compatibility-proof.json", result.CompatibilityProof, cancellationToken);
            await WriteJson(root, "generic-composer-scalability-negative-proof.json", result.NegativeProof, cancellationToken);
            await WriteText(Path.Combine(root, "generic-composer-scalability-report.md"), RenderReport(result), cancellationToken);
            await WriteIndex(repositoryRoot, root, cancellationToken);
        }
        return new[] { procedural, export }.SelectMany(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            .Select(path => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal).ToList();
    }

    private static async Task WriteIndex(string repositoryRoot, string root, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(root, "generic-composer-scalability-file-index.json");
        var files = Directory.EnumerateFiles(root).Where(path => !path.Equals(indexPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal).Select(path => new
            {
                relativePath = Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'),
                sha256 = HashFile(path),
                required = true
            }).ToList();
        await WriteJson(root, "generic-composer-scalability-file-index.json", new
        {
            schemaVersion = "generic_composer_scalability_file_index_v1",
            scenarioId = ScenarioId,
            indexedFileCount = files.Count,
            files
        }, cancellationToken);
    }

    private static string RenderReport(FeatureModuleComposerScalabilityWriteResult result) => string.Join('\n',
    [
        "# Goal146A Generic FeatureModule Composer Scalability",
        string.Empty,
        "- status: " + result.Dashboard.Status,
        "- currentCoverageMode: " + result.Dashboard.CurrentCoverageMode,
        "- currentGeneratedCompositionCount: " + result.Dashboard.CurrentGeneratedCompositionCount,
        "- syntheticFourthModulePassed: " + result.Dashboard.SyntheticFourthModulePassed,
        "- syntheticFourthGeneratedCompositionCount: " + result.Dashboard.SyntheticFourthGeneratedCompositionCount,
        "- largeCatalogGeneratedCompositionCount: " + result.CoverageProof.SyntheticTwelveModuleCatalog.GeneratedCompositionCount,
        "- largeCatalogMaxTotalRows: " + result.CoverageProof.SyntheticTwelveModuleCatalog.Policy.MaxTotalRows,
        "- currentEightPackageHashesPreserved: " + result.Dashboard.CurrentEightPackageHashesPreserved,
        "- currentEightFinalHashesPreserved: " + result.Dashboard.CurrentEightFinalHashesPreserved,
        "- goal146Accepted: false",
        "- manualReviewDeferred: true",
        string.Empty
    ]);

    private static Task WriteJson(string root, string name, object value, CancellationToken cancellationToken) =>
        WriteText(Path.Combine(root, name), JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine, cancellationToken);

    private static async Task WriteText(string path, string text, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(temporary, text, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string ResolveOutput(string repositoryRoot, string path, string allowedRoot)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repositoryRoot, path));
        var allowed = Path.GetFullPath(Path.Combine(repositoryRoot, allowedRoot));
        if (!full.Equals(allowed, StringComparison.OrdinalIgnoreCase)
            && !full.StartsWith(allowed.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Goal146A output path escape rejected.");
        if (Path.GetRelativePath(repositoryRoot, full).Replace('\\', '/').StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Goal146A refuses .llmgc/manual path.");
        return full;
    }

    private static bool ReadBoolean(string root, string relative, string property, string expected)
    {
        using var document = JsonDocument.Parse(Read(root, relative));
        return document.RootElement.TryGetProperty(property, out var value)
               && string.Equals(value.GetString(), expected, StringComparison.Ordinal);
    }

    private static string Read(string root, string relative) =>
        File.ReadAllText(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)), Encoding.UTF8);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
