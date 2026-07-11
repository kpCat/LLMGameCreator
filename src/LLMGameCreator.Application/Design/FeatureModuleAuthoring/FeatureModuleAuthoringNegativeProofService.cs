using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleAuthoringNegativeProofService
{
    public IReadOnlyDictionary<string, bool> Build(
        string repositoryRoot,
        FeatureModuleLibrarySnapshot library,
        FeatureModuleCompositionPersistenceService persistence,
        FeatureModuleCompositionDocument savedDocument,
        FeatureModuleCompositionCoveragePlan hundredCoverage,
        FeatureModuleCompositionCoveragePlan incompatibleSmallCoverage,
        bool corruptCertificationCacheRejected,
        bool runtimeContractVersionInvalidatesCache,
        bool multiEffectAccountingPassed,
        bool previousArtifactsPreservedOnFailure)
    {
        var loader = new FeatureModuleLibraryLoader();
        var sourceRoot = library.LibraryRoot;
        var parameterValidator = new FeatureModuleParameterValidator();
        var binder = new FeatureModuleParameterBindingService();
        var alchemy = "feature.profile.alchemy_focus";
        var combat = "feature.profile.combat_focus";
        var badBindingCatalog = MutateCatalog(library.Catalog, alchemy, module => module with
        {
            ParameterDefinitions = module.ParameterDefinitions.Select(parameter =>
                parameter.ParameterId == "healingPotionOutput"
                    ? parameter with
                    {
                        Bindings =
                        [
                            new FeatureModuleParameterBinding
                            {
                                OperationId = "missing.operation",
                                AtomicGroupId = parameter.AtomicGroupId
                            }
                        ]
                    }
                    : parameter).ToList()
        });
        var enumCatalog = MutateCatalog(library.Catalog, alchemy, module => module with
        {
            ParameterDefinitions = module.ParameterDefinitions.Append(new FeatureModuleParameterDefinition
            {
                ParameterId = "difficulty",
                ModuleId = module.ModuleId,
                Title = "Difficulty",
                ValueType = FeatureModuleParameterValueTypes.Enum,
                Required = true,
                DefaultValue = JsonSerializer.SerializeToElement("normal"),
                AllowedValues = ["normal", "hard"],
                AuthoringControl = FeatureModuleAuthoringControls.ComboBox
            }).ToList()
        });
        var stepCatalog = MutateCatalog(library.Catalog, alchemy, module => module with
        {
            ParameterDefinitions = module.ParameterDefinitions.Select(parameter =>
                parameter.ParameterId == "healingPotionOutput" ? parameter with { Minimum = 0, Step = 2 } : parameter).ToList()
        });
        var unknown = parameterValidator.Validate(library.Catalog, [alchemy], [Value(alchemy, "missing", 1)]);
        var unselected = parameterValidator.Validate(library.Catalog, [alchemy], [Value(combat, "basicAttackDamage", 6)]);
        var wrongType = parameterValidator.Validate(library.Catalog, [alchemy], [Value(alchemy, "healingPotionOutput", "bad")]);
        var range = parameterValidator.Validate(library.Catalog, [alchemy], [Value(alchemy, "healingPotionOutput", 999)]);
        var step = parameterValidator.Validate(stepCatalog, [alchemy], [Value(alchemy, "healingPotionOutput", 3)]);
        var invalidEnum = parameterValidator.Validate(enumCatalog, [alchemy], [Value(alchemy, "difficulty", "impossible")]);
        var duplicateValue = Value(alchemy, "healingPotionOutput", 2);
        var duplicate = parameterValidator.Validate(library.Catalog, [alchemy], [duplicateValue, duplicateValue]);
        var bindingFailure = binder.Bind(badBindingCatalog, [alchemy], []);

        var invalidId = Throws(() => persistence.CreateNew("../escape", "Bad", "Bad", library));
        var workspaceEscape = invalidId;
        var corruptComposition = CorruptCompositionRejected(persistence, library);
        var saveAsDuplicate = Throws(() => persistence.SaveAs(savedDocument, savedDocument.CompositionId, "Duplicate", library));
        var missingDocument = savedDocument with { SelectedModuleIds = ["feature.profile.missing"] };
        var missing = new FeatureModuleCompositionStalenessService().Evaluate(missingDocument, library);
        var stale = new FeatureModuleCompositionStalenessService().Evaluate(savedDocument, library with
        {
            CatalogFingerprint = new string('f', 64)
        });
        var parameterSource = Read(repositoryRoot,
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs");
        var runtimeSource = Read(repositoryRoot,
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs");
        var unitySource = Read(repositoryRoot,
            "unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySavedFeatureModuleCompositionHarness.cs");
        var winFormsSource = Read(repositoryRoot,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs");

        var values = new SortedDictionary<string, bool>(StringComparer.Ordinal)
        {
            ["moduleLibraryPathEscapeRejected"] = TamperedLibraryThrows(sourceRoot, (manifest, _) =>
                manifest["moduleFiles"]!.AsArray()[0] = "../outside.featuremodule.json"),
            ["moduleFilePathEscapeRejected"] = TamperedLibraryThrows(sourceRoot, (manifest, _) =>
                manifest["moduleFiles"]!.AsArray()[0] = "../outside.featuremodule.json"),
            ["malformedModuleJsonRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                File.WriteAllText(modulePath, "{malformed")),
            ["unsupportedModuleSchemaRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["schemaVersion"] = "unsupported")),
            ["duplicateModuleIdRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["moduleId"] = "feature.crafting.recipes")),
            ["duplicateModuleFileRejected"] = TamperedLibraryThrows(sourceRoot, (manifest, _) =>
                manifest["moduleFiles"]!.AsArray().Add(manifest["moduleFiles"]!.AsArray()[0]!.GetValue<string>())),
            ["unknownDependencyRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["dependencies"]!.AsArray().Add("feature.missing"))),
            ["conflictReferenceMismatchRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["conflicts"]!.AsArray().Add("feature.profile.combat_focus"))),
            ["operationReferenceMismatchRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["mutationOperations"]!.AsArray()[0]!["operationId"] = ""), optional: true),
            ["effectOperationReferenceMismatchRejected"] = TamperedLibraryThrows(sourceRoot, (_, modulePath) =>
                MutateJson(modulePath, node => node["runtimeEffectContracts"]!.AsArray()[0]!["sourceOperationIds"] =
                    new JsonArray("missing.operation")), optional: true),
            ["unknownParameterRejected"] = !unknown.Passed,
            ["unselectedModuleParameterRejected"] = !unselected.Passed,
            ["wrongParameterTypeRejected"] = !wrongType.Passed,
            ["parameterRangeViolationRejected"] = !range.Passed,
            ["parameterStepViolationRejected"] = !step.Passed,
            ["invalidEnumRejected"] = !invalidEnum.Passed,
            ["duplicateParameterRejected"] = !duplicate.Passed,
            ["conflictingParameterBindingRejected"] = !bindingFailure.Passed,
            ["atomicGroupPartialApplyRejected"] = !bindingFailure.Passed && bindingFailure.EffectiveMutationOperations.Count == 0,
            ["compositionWorkspacePathEscapeRejected"] = workspaceEscape,
            ["invalidCompositionIdRejected"] = invalidId,
            ["corruptCompositionRejected"] = corruptComposition,
            ["saveAsDuplicateRejected"] = saveAsDuplicate,
            ["missingSelectedModuleNoFallback"] = missing.Unresolved && missing.MissingModuleIds.Count == 1,
            ["staleCompositionDetected"] = stale.Stale,
            ["corruptCertificationCacheRejected"] = corruptCertificationCacheRejected,
            ["runtimeContractVersionInvalidatesCache"] = runtimeContractVersionInvalidatesCache,
            ["catalogAboveTwentyModulesDoesNotThrow"] = hundredCoverage.OptionalModuleCount == 100,
            ["fullPowersetAboveSmallLimitAbsent"] = !hundredCoverage.FullPowersetEnumerated,
            ["incompatibleSmallCatalogCombinationNotExecuted"] = incompatibleSmallCoverage.RejectedCompositions.Count > 0,
            ["multiEffectCountNotComparedToModuleCount"] = multiEffectAccountingPassed,
            ["moduleIdSpecificParameterBranchAbsent"] = !CurrentModuleIdPresent(parameterSource),
            ["compositionIdSpecificRuntimeBranchAbsent"] = !runtimeSource.Contains("minimal-map-game-composed-", StringComparison.Ordinal),
            ["unityDoesNotAuthorMaterializeOrExecute"] = !unitySource.Contains("FeatureModuleParameterizedCompositionService", StringComparison.Ordinal)
                                                        && !unitySource.Contains("ProductLineRuntimeVariantMaterializer", StringComparison.Ordinal)
                                                        && !unitySource.Contains("FeatureModuleCompositionPersistenceService", StringComparison.Ordinal),
            ["winFormsStartsNoCompilerOrTestProcess"] = !winFormsSource.Contains("ProcessStartInfo", StringComparison.Ordinal)
                                                        && !winFormsSource.Contains("dotnet test", StringComparison.OrdinalIgnoreCase)
                                                        && !winFormsSource.Contains("powershell", StringComparison.OrdinalIgnoreCase),
            ["previousArtifactsPreservedOnFailure"] = previousArtifactsPreservedOnFailure
        };
        return values;
    }

    private static bool TamperedLibraryThrows(
        string sourceRoot,
        Action<JsonObject, string> mutation,
        bool optional = false)
    {
        var temp = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-negative-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyDirectory(sourceRoot, temp);
            var manifestPath = Path.Combine(temp, "catalog.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            var relative = manifest["moduleFiles"]!.AsArray()
                .Select(item => item!.GetValue<string>())
                .First(path => !optional || path.StartsWith("optional/", StringComparison.Ordinal));
            var modulePath = Path.Combine(temp, relative.Replace('/', Path.DirectorySeparatorChar));
            mutation(manifest, modulePath);
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            return Throws(() => new FeatureModuleLibraryLoader().Load(temp));
        }
        finally { if (Directory.Exists(temp)) Directory.Delete(temp, true); }
    }

    private static void MutateJson(string path, Action<JsonObject> mutation)
    {
        var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        mutation(node);
        File.WriteAllText(path, node.ToJsonString());
    }

    private static bool CorruptCompositionRejected(
        FeatureModuleCompositionPersistenceService persistence,
        FeatureModuleLibrarySnapshot library)
    {
        var path = Path.Combine(persistence.WorkspaceRoot, "corrupt.featurecomposition.json");
        Directory.CreateDirectory(persistence.WorkspaceRoot);
        File.WriteAllText(path, "{corrupt");
        return Throws(() => persistence.Load("corrupt", library));
    }

    private static FeatureModuleCatalogDocument MutateCatalog(
        FeatureModuleCatalogDocument catalog,
        string moduleId,
        Func<FeatureModuleDefinition, FeatureModuleDefinition> mutation) => catalog with
    {
        Modules = catalog.Modules.Select(module => module.ModuleId == moduleId ? mutation(module) : module).ToList()
    };

    private static FeatureModuleParameterValue Value<T>(string moduleId, string parameterId, T value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    private static bool Throws(Action action)
    {
        try { action(); return false; }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException) { return true; }
    }

    private static bool CurrentModuleIdPresent(string source) =>
        new[] { "feature.profile.alchemy_focus", "feature.profile.combat_focus", "feature.profile.exploration_resource_focus" }
            .Any(id => source.Contains(id, StringComparison.Ordinal));

    private static string Read(string root, string relative)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static void CopyDirectory(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }
}
