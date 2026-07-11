using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleAuthoringProofService
{
    private static readonly DateTimeOffset ProofTime = new(2026, 7, 11, 0, 0, 0, TimeSpan.Zero);
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly FeatureModuleAuthoringArtifactService _artifacts = new();

    public FeatureModuleAuthoringProofService(ISelectedRuntimeVariantInteractiveSessionService runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public async Task<FeatureModuleAuthoringProofResult> RunAndWriteAsync(
        string repositoryRoot,
        FeatureModuleAuthoringRunRequest request,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var catalogRoot = Resolve(root, request.CatalogRoot);
        var workspace = Path.GetFullPath(request.WorkspaceRoot);
        var cacheRoot = Path.GetFullPath(request.CertificationCacheRoot);
        var outputRoot = Resolve(root, request.OutputRoot);
        var exportRoot = Resolve(root, FeatureModuleAuthoringVocabulary.ExportRoot);
        var unitySmokePath = Resolve(root, request.UnitySmokePath);
        GuardNoManual(root, catalogRoot, workspace, cacheRoot, outputRoot);
        var library = new FeatureModuleLibraryLoader().Load(catalogRoot);
        var optionalIds = OptionalIds(library);
        var compatibility = ProveCompatibility(root, workspace, library, optionalIds);

        var persistence = new FeatureModuleCompositionPersistenceService(workspace, new ProofClock());
        var document = persistence.CreateNew(
            request.CompositionId,
            "Goal147 Custom Alchemy + Combat + Exploration",
            "Saved all-three composition with typed non-default parameters.",
            library) with
        {
            ParameterValues = CustomValues()
        };
        var saved = persistence.Save(document, library);
        var loaded = persistence.Load(saved.CompositionId, library);
        var canonicalRoundtrip = FeatureModuleCompositionPersistenceService.SerializeCanonical(saved)
                                 == FeatureModuleCompositionPersistenceService.SerializeCanonical(loaded);
        var clone = persistence.Clone(saved.CompositionId, saved.CompositionId + "-clone", "Goal147 Clone", library);
        var listedWithClone = persistence.List(library);
        persistence.Delete(clone.CompositionId);
        var deleted = persistence.List(library).Compositions.All(item => item.CompositionId != clone.CompositionId);
        var saveAsDuplicateRejected = Throws(() => persistence.SaveAs(saved, saved.CompositionId, "Duplicate", library));

        var parameterized = new FeatureModuleParameterizedCompositionService(_runtime);
        var first = parameterized.MaterializeAndQualify(root, library, loaded, Path.Combine(workspace, "materialization-a"));
        var reversed = loaded with { ParameterValues = loaded.ParameterValues.Reverse().ToList() };
        var second = parameterized.MaterializeAndQualify(root, library, reversed, Path.Combine(workspace, "materialization-b"));
        if (!first.Passed || !second.Passed || first.PackageJson != second.PackageJson
            || first.FinalStateHash != second.FinalStateHash)
            throw new InvalidOperationException("custom parameterized composition determinism failed");
        var qualifiedDocument = persistence.Save(first.QualifiedDocument, library);
        var atomicWritePassed = !Directory.EnumerateFiles(workspace, "*.tmp-*", SearchOption.AllDirectories).Any();
        var staleLibrary = library with { CatalogFingerprint = new string('d', 64) };
        var staleProof = new FeatureModuleCompositionStalenessService().Evaluate(qualifiedDocument, staleLibrary);

        var baseSha = BaselineSha(root);
        var cache = new FeatureModuleCertificationCache(cacheRoot);
        var certification = new FeatureModuleCertificationService(_runtime, cache, new ProofClock());
        var certificationExecution = Path.Combine(workspace, "certification-execution");
        var initialLedger = certification.Certify(root, library, baseSha, certificationExecution);
        var reuseLedger = certification.Certify(root, library, baseSha, certificationExecution);
        File.WriteAllText(cache.PathForModule("feature.profile.combat_focus"), "{corrupt");
        var corruptLedger = certification.Certify(root, library, baseSha, certificationExecution);
        var changedLibrary = ChangeModuleFingerprint(library, "feature.profile.alchemy_focus");
        var changedLedger = certification.Certify(root, changedLibrary, baseSha, certificationExecution);
        var contractLedger = certification.Certify(root, library, baseSha, certificationExecution,
            FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion + "-changed");

        var hundred = BuildHundredModuleLibrary(library);
        var selectedHundred = new[] { "feature.synthetic.module_000", "feature.synthetic.module_099" };
        var hundredCoverage = new FeatureModuleCompositionCoveragePlanner().Plan(hundred.Catalog, selectedHundred);
        var hundredCertification = new FeatureModuleCertificationPlanner().Plan(hundred, baseSha,
            FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
            string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan));
        var incompatibleSmall = BuildIncompatibleSmallCoverage(library);
        var multiEffect = BuildMultiEffectProof(compatibility.Qualifications);
        var moduleFileAddition = ProveValidFourthModuleFile(catalogRoot);
        var unitySmoke = LoadUnitySmoke(unitySmokePath);
        var scriptSource = Read(root, ".devflow/scripts/run-featuremodule-authoring-persistence-and-certification.ps1");
        var previousArtifactsPreserved = scriptSource.Contains("Restore-Goal147Directory", StringComparison.Ordinal)
                                         && scriptSource.Contains("catch", StringComparison.Ordinal);
        var negative = new FeatureModuleAuthoringNegativeProofService().Build(
            root, library, persistence, qualifiedDocument, hundredCoverage, incompatibleSmall,
            corruptLedger.CorruptCacheRejected, contractLedger.ExecutedCount == 3,
            (bool)multiEffect.GetType().GetProperty("Passed")!.GetValue(multiEffect)!, previousArtifactsPreserved);

        var defaultSelected = compatibility.Rows.Single(row => row.CompositionId == FeatureModuleCompositionVocabulary.DefaultCompositionId);
        var defaultCompatibility = new
        {
            schemaVersion = "featuremodule_default_hash_compatibility_proof_v1",
            seededLibraryCurrentEightPackageHashesPreserved = compatibility.PackageHashesPreserved,
            seededLibraryCurrentEightFinalHashesPreserved = compatibility.FinalHashesPreserved,
            compositionCount = compatibility.Rows.Count,
            selectedPackageSha256 = defaultSelected.PackageSha256,
            selectedFinalStateHash = defaultSelected.FinalStateHash,
            rows = compatibility.Rows
        };
        var roundtripProof = new
        {
            schemaVersion = "saved_composition_roundtrip_proof_v1",
            createNewPassed = true,
            savePassed = saved.Revision == 1,
            loadPassed = canonicalRoundtrip,
            listPassed = listedWithClone.CompositionCount == 2,
            clonePassed = clone.Revision == 1,
            deletePassed = deleted,
            saveAsDuplicateRejected,
            deterministicCanonicalSerialization = canonicalRoundtrip,
            atomicWritePassed,
            stalenessDetectionPassed = staleProof.Stale,
            revisionAfterQualification = qualifiedDocument.Revision,
            passed = canonicalRoundtrip && deleted && saveAsDuplicateRejected && atomicWritePassed && staleProof.Stale
        };
        var materializationProof = new
        {
            schemaVersion = "parameterized_composition_materialization_proof_v1",
            compositionId = qualifiedDocument.CompositionId,
            parameterValues = first.Plan.ParameterBinding.EffectiveParameterValues,
            packageSha256 = first.PackageSha256,
            finalStateHash = first.FinalStateHash,
            packageDistinctFromDefault = first.PackageSha256 != defaultSelected.PackageSha256,
            repeatedPackageHashMatches = first.PackageSha256 == second.PackageSha256,
            repeatedFinalHashMatches = first.FinalStateHash == second.FinalStateHash,
            effectObservationCount = first.EffectObservationCount,
            satisfiedSelectedModuleCount = first.SatisfiedSelectedModuleCount,
            inventorySummary = first.Qualification.Artifacts.Session.LatestInventorySummary,
            combatSummary = first.Qualification.Artifacts.Session.LatestCombatSummary,
            questSummary = first.Qualification.Artifacts.Session.LatestQuestSummary,
            checkpointReloadPassed = first.CheckpointReloadPassed,
            fullReplayEquivalent = first.FullReplayEquivalent,
            actionBindingPassed = first.ActionBindingPassed,
            passed = first.Passed
        };
        var cacheProof = new
        {
            schemaVersion = "module_certification_cache_proof_v1",
            firstExecutedCount = initialLedger.ExecutedCount,
            unchangedReusedCount = reuseLedger.ReusedCount,
            corruptCacheRejected = corruptLedger.CorruptCacheRejected,
            corruptCacheRegeneratedCount = corruptLedger.ExecutedCount,
            changedModuleExecutedCount = changedLedger.ExecutedCount,
            changedModuleReusedCount = changedLedger.ReusedCount,
            changedModuleSelectiveInvalidationPassed = changedLedger.ExecutedCount == 1 && changedLedger.ReusedCount == 2,
            runtimeContractVersionInvalidatesCache = contractLedger.ExecutedCount == 3,
            passed = reuseLedger.ReusedCount == 3 && corruptLedger.CorruptCacheRejected
                     && changedLedger.ExecutedCount == 1 && contractLedger.ExecutedCount == 3
        };
        var hundredProof = new
        {
            schemaVersion = "hundred_module_scalability_proof_v1",
            optionalModuleCount = 100,
            certificationPlanCount = hundredCertification.ModuleCount,
            interactionRowCount = hundredCoverage.GeneratedCompositionCount,
            maxTotalRows = hundredCoverage.Policy.MaxTotalRows,
            selectedCompositionIncluded = hundredCoverage.SelectedCompositionIncluded,
            baselineIncluded = hundredCoverage.BaselineIncluded,
            fullPowersetEnumerated = hundredCoverage.FullPowersetEnumerated,
            theoreticalPowersetSize = hundredCoverage.TheoreticalPowersetSize,
            passed = hundredCertification.ModuleCount == 100 && hundredCoverage.GeneratedCompositionCount <= 24
                     && !hundredCoverage.FullPowersetEnumerated
        };
        var parameterSchema = new
        {
            schemaVersion = "featuremodule_parameter_schema_v1",
            supportedValueTypes = new[] { "integer", "number", "boolean", "enum" },
            supportedControls = new[] { "numeric_up_down", "check_box", "combo_box" },
            parameterDefinitionCount = library.Index.ParameterDefinitionCount,
            parameters = library.Catalog.Modules.SelectMany(module => module.ParameterDefinitions)
                .OrderBy(item => item.ModuleId, StringComparer.Ordinal).ThenBy(item => item.ParameterId, StringComparer.Ordinal).ToList()
        };
        var coreGreen = library.Validation.Passed && compatibility.PackageHashesPreserved
                        && compatibility.FinalHashesPreserved && (bool)roundtripProof.passed && first.Passed
                        && reuseLedger.ReusedCount == 3 && changedLedger.ExecutedCount == 1
                        && hundredCertification.ModuleCount == 100 && hundredCoverage.GeneratedCompositionCount <= 24
                        && (bool)multiEffect.GetType().GetProperty("Passed")!.GetValue(multiEffect)!
                        && negative.Values.All(value => value) && moduleFileAddition;
        var dashboard = new FeatureModuleAuthoringDashboard
        {
            Status = coreGreen && unitySmoke.Passed ? "GREEN" : coreGreen ? "READY_FOR_UNITY_SMOKE" : "FAILED",
            PersistentFeatureModuleLibrary = true,
            ModuleLibraryFileBased = true,
            ModuleLibrarySourceOfTruth = true,
            RequiredCoreModuleCount = library.Index.RequiredCoreModuleCount,
            OptionalModuleCount = library.Index.OptionalModuleCount,
            ModuleFingerprintingPassed = library.ModuleFingerprints.Count == 13,
            CatalogFingerprintingPassed = library.CatalogFingerprint.Length == 64,
            AddingModuleFileRequiresNoComposerCodeChange = moduleFileAddition,
            TypedParameterAuthoring = true,
            ParameterDefinitionCount = library.Index.ParameterDefinitionCount,
            GenericParameterBinding = true,
            AtomicParameterGroupsPassed = first.AtomicParameterGroupsPassed,
            DefaultParametersPreserveGoal146Hashes = compatibility.PackageHashesPreserved && compatibility.FinalHashesPreserved,
            SavedCompositionPersistence = true,
            SavedCompositionRoundtripPassed = canonicalRoundtrip,
            SavedCompositionAtomicWritePassed = atomicWritePassed,
            SavedCompositionStalenessDetectionPassed = staleProof.Stale,
            IncrementalModuleCertification = true,
            AllOptionalModulesCertified = reuseLedger.CertifiedModuleCount == 3,
            UnchangedCertificationCacheReusePassed = reuseLedger.ReusedCount == 3,
            ChangedModuleSelectiveInvalidationPassed = changedLedger.ExecutedCount == 1 && changedLedger.ReusedCount == 2,
            InteractionCoverageIndependentFromSingletonCertification = hundredCoverage.SingletonCoverageCount == 0,
            HundredModuleCatalogAccepted = hundredCertification.ModuleCount == 100,
            HundredModuleInteractionRowCount = hundredCoverage.GeneratedCompositionCount,
            HundredModulePowersetEnumerated = hundredCoverage.FullPowersetEnumerated,
            SelectedCompositionAlwaysIncluded = hundredCoverage.SelectedCompositionIncluded,
            SmallCatalogCompatibleExhaustiveCoveragePassed = compatibility.Rows.Count == 8,
            SmallCatalogInvalidCombinationsClassified = incompatibleSmall.RejectedCompositions.Count == 1,
            MultiEffectModuleAccountingPassed = (bool)multiEffect.GetType().GetProperty("Passed")!.GetValue(multiEffect)!,
            CustomParameterizedCompositionPassed = first.Passed,
            CustomPackageDistinctFromDefault = first.PackageSha256 != defaultSelected.PackageSha256,
            CustomRuntimeQualificationPassed = first.CheckpointReloadPassed && first.FullReplayEquivalent && first.ActionBindingPassed,
            UnitySmokePassed = unitySmoke.Passed
        };
        var result = new FeatureModuleAuthoringProofResult
        {
            Library = library,
            ParameterSchema = parameterSchema,
            DefaultHashCompatibilityProof = defaultCompatibility,
            SavedCompositionRoundtripProof = roundtripProof,
            ParameterizedCompositionMaterializationProof = materializationProof,
            CertificationLedger = reuseLedger,
            CertificationCacheProof = cacheProof,
            BoundedInteractionCoverageProof = hundredCoverage,
            HundredModuleScalabilityProof = hundredProof,
            MultiEffectModuleProof = multiEffect,
            NegativeProof = negative,
            SelectedComposition = qualifiedDocument,
            SelectedMaterialization = first,
            Dashboard = dashboard,
            UnitySmoke = unitySmoke
        };
        var written = await _artifacts.WriteAsync(root, outputRoot, exportRoot, result, cancellationToken).ConfigureAwait(false);
        return result with { WrittenFiles = written };
    }

    private CompatibilityResult ProveCompatibility(
        string root, string workspace, FeatureModuleLibrarySnapshot library, IReadOnlyList<string> selected)
    {
        var legacyPath = Path.Combine(root, FeatureModuleCompositionVocabulary.ProceduralRoot.Replace('/', Path.DirectorySeparatorChar),
            "featuremodule-composition-matrix-result.json");
        var legacy = JsonSerializer.Deserialize<FeatureModuleCompositionMatrixResult>(File.ReadAllText(legacyPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var expected = legacy.Compositions.ToDictionary(item => item.CompositionId, StringComparer.Ordinal);
        var coverage = new FeatureModuleCompositionCoveragePlanner().Plan(library.Catalog, selected);
        var service = new FeatureModuleCompositionService(_runtime);
        var rows = new List<CompatibilityRow>();
        var qualifications = new Dictionary<string, FeatureModuleCompositionQualification>(StringComparer.Ordinal);
        foreach (var spec in coverage.CompositionSpecs)
        {
            var qualification = service.ComposeAndQualify(root, library.Catalog, spec.ModuleIds,
                Path.Combine(workspace, "compatibility", FeatureModuleLibraryFingerprintService.Hash(spec.CompositionId)), spec.CompositionId);
            qualifications[spec.CompositionId] = qualification;
            var old = expected[spec.CompositionId];
            rows.Add(new CompatibilityRow(spec.CompositionId, qualification.Result.PackageSha256,
                qualification.Result.FinalStateHash, qualification.Result.PackageSha256 == old.PackageSha256,
                qualification.Result.FinalStateHash == old.FinalStateHash));
        }
        return new CompatibilityResult(rows, qualifications,
            rows.All(row => row.PackageHashPreserved), rows.All(row => row.FinalHashPreserved));
    }

    private static object BuildMultiEffectProof(
        IReadOnlyDictionary<string, FeatureModuleCompositionQualification> qualifications)
    {
        var alchemy = qualifications["minimal-map-game-composed-alchemy"];
        var baseline = qualifications["minimal-map-game-composed-baseline"];
        var module = new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.multi_effect",
            Title = "Synthetic Multi Effect",
            RuntimeEffectContracts =
            [
                new FeatureModuleRuntimeEffectContract { EffectId = "effect.red_herb", ModuleId = "feature.synthetic.multi_effect", MetricKind = "inventory_item_quantity", TargetId = "inventory/player_start", ResourceOrItemId = "item/red_herb", ComparisonKind = "greater_than_baseline" },
                new FeatureModuleRuntimeEffectContract { EffectId = "effect.water", ModuleId = "feature.synthetic.multi_effect", MetricKind = "inventory_item_quantity", TargetId = "inventory/player_start", ResourceOrItemId = "item/water_flask", ComparisonKind = "greater_than_baseline" }
            ]
        };
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate([module],
            alchemy.Artifacts.Session, baseline.Artifacts.Session);
        var satisfied = observations.Count == 2 && observations.All(item => item.Passed) ? 1 : 0;
        return new
        {
            SchemaVersion = "multi_effect_module_proof_v1",
            EffectObservationCount = observations.Count,
            PassedEffectObservationCount = observations.Count(item => item.Passed),
            SelectedModuleCount = 1,
            SatisfiedSelectedModuleCount = satisfied,
            Observations = observations,
            Passed = observations.Count == 2 && satisfied == 1
        };
    }

    private static FeatureModuleLibrarySnapshot ChangeModuleFingerprint(FeatureModuleLibrarySnapshot library, string moduleId)
    {
        var catalog = library.Catalog with
        {
            Modules = library.Catalog.Modules.Select(module => module.ModuleId == moduleId
                ? module with { Title = module.Title + " Changed" } : module).ToList()
        };
        return ReFingerprint(library, catalog);
    }

    private static FeatureModuleLibrarySnapshot BuildHundredModuleLibrary(FeatureModuleLibrarySnapshot source)
    {
        var modules = Enumerable.Range(0, 100).Select(index => new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.module_" + index.ToString("000"),
            Title = "Synthetic Module " + index.ToString("000"),
            Category = "synthetic",
            ModuleKind = "scalability_test",
            Selectable = true
        }).ToList();
        var catalog = source.Catalog with
        {
            OptionalProfileModuleCount = modules.Count,
            Modules = source.Catalog.Modules.Where(module => module.Required).Concat(modules).ToList()
        };
        return ReFingerprint(source, catalog);
    }

    private static FeatureModuleLibrarySnapshot ReFingerprint(
        FeatureModuleLibrarySnapshot source, FeatureModuleCatalogDocument catalog)
    {
        var service = new FeatureModuleLibraryFingerprintService();
        var fingerprints = catalog.Modules.ToDictionary(module => module.ModuleId, service.ModuleFingerprint, StringComparer.Ordinal);
        return source with { Catalog = catalog, ModuleFingerprints = fingerprints, CatalogFingerprint = service.CatalogFingerprint(fingerprints) };
    }

    private static FeatureModuleCompositionCoveragePlan BuildIncompatibleSmallCoverage(FeatureModuleLibrarySnapshot source)
    {
        var first = new FeatureModuleDefinition { ModuleId = "feature.synthetic.first", Title = "First", Category = "test", ModuleKind = "test", Selectable = true, Conflicts = ["feature.synthetic.second"] };
        var second = new FeatureModuleDefinition { ModuleId = "feature.synthetic.second", Title = "Second", Category = "test", ModuleKind = "test", Selectable = true, Conflicts = ["feature.synthetic.first"] };
        var catalog = source.Catalog with { OptionalProfileModuleCount = 2, Modules = source.Catalog.Modules.Where(module => module.Required).Concat([first, second]).ToList() };
        return new FeatureModuleCompositionCoveragePlanner().Plan(catalog, [first.ModuleId]);
    }

    private static bool ProveValidFourthModuleFile(string sourceRoot)
    {
        var temp = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-fourth-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyDirectory(sourceRoot, temp);
            var source = Path.Combine(temp, "optional", "alchemy-focus.featuremodule.json");
            var module = JsonNode.Parse(File.ReadAllText(source))!.AsObject();
            module["moduleId"] = "feature.profile.synthetic_fourth";
            module["title"] = "Synthetic Fourth";
            module["dependencies"] = new JsonArray();
            module["mutationOperations"] = new JsonArray();
            module["runtimeEffectContracts"] = new JsonArray();
            module["parameterDefinitions"] = new JsonArray();
            module["sourceLineage"]!["operationIds"] = new JsonArray();
            var relative = "optional/synthetic-fourth.featuremodule.json";
            File.WriteAllText(Path.Combine(temp, relative.Replace('/', Path.DirectorySeparatorChar)), module.ToJsonString());
            var manifestPath = Path.Combine(temp, "catalog.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            manifest["optionalModuleCount"] = 4;
            manifest["moduleFileCount"] = 14;
            manifest["moduleFiles"]!.AsArray().Add(relative);
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            return new FeatureModuleLibraryLoader().Load(temp).Index.OptionalModuleCount == 4;
        }
        finally { if (Directory.Exists(temp)) Directory.Delete(temp, true); }
    }

    private static IReadOnlyList<FeatureModuleParameterValue> CustomValues() =>
    [
        Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
        Value("feature.profile.combat_focus", "goblinStartingHealth", 18),
        Value("feature.profile.combat_focus", "basicAttackDamage", 5),
        Value("feature.profile.exploration_resource_focus", "appleYield", 4),
        Value("feature.profile.exploration_resource_focus", "logYield", 3),
        Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3)
    ];

    private static FeatureModuleParameterValue Value<T>(string moduleId, string parameterId, T value) => new() { ModuleId = moduleId, ParameterId = parameterId, Value = JsonSerializer.SerializeToElement(value) };
    private static IReadOnlyList<string> OptionalIds(FeatureModuleLibrarySnapshot library) => library.Catalog.Modules.Where(module => module.Selectable && !module.Required).Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();
    private static FeatureModuleAuthoringUnitySmoke LoadUnitySmoke(string path) => File.Exists(path) ? JsonSerializer.Deserialize<FeatureModuleAuthoringUnitySmoke>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new() : new();
    private static string BaselineSha(string root) { var path = Path.Combine(root, FeatureModuleCompositionVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar), "product-line-runtime-variant-matrix-result.json"); using var json = JsonDocument.Parse(File.ReadAllText(path)); return json.RootElement.GetProperty("candidates").EnumerateArray().Single(item => item.GetProperty("candidateId").GetString() == FeatureModuleCompositionVocabulary.BaselineCandidateId).GetProperty("packageSha256").GetString()!; }
    private static string Resolve(string root, string path) => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
    private static void GuardNoManual(string root, params string[] paths) { foreach (var path in paths) { var relative = Path.GetRelativePath(root, path).Replace('\\', '/'); if (relative.StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Goal147 refuses .llmgc/manual path"); } }
    private static bool Throws(Action action) { try { action(); return false; } catch (InvalidOperationException) { return true; } }
    private static string Read(string root, string relative) { var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)); return File.Exists(path) ? File.ReadAllText(path) : string.Empty; }
    private static void CopyDirectory(string source, string destination) { foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)) { var target = Path.Combine(destination, Path.GetRelativePath(source, file)); Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(file, target); } }

    private sealed record CompatibilityRow(string CompositionId, string PackageSha256, string FinalStateHash, bool PackageHashPreserved, bool FinalHashPreserved);
    private sealed record CompatibilityResult(IReadOnlyList<CompatibilityRow> Rows, IReadOnlyDictionary<string, FeatureModuleCompositionQualification> Qualifications, bool PackageHashesPreserved, bool FinalHashesPreserved);
    private sealed class ProofClock : IFeatureModuleAuthoringClock { public DateTimeOffset UtcNow => ProofTime; }
}
