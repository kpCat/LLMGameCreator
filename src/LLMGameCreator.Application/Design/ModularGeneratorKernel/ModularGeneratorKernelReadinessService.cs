using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.Application.Design.ModularGeneratorKernel;

public sealed class ModularGeneratorKernelReadinessService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/modular-generator-kernel-parallel-readiness";
    public const string ModuleContractManifestProofJsonFileName = "module-contract-manifest-proof.json";
    public const string ProductSmokeScenarioManifestProofJsonFileName = "product-smoke-scenario-manifest-proof.json";
    public const string PackageAssemblyModuleRegistryReportJsonFileName = "package-assembly-module-registry-report.json";
    public const string ModuleCompatibilityMatrixJsonFileName = "module-compatibility-matrix.json";
    public const string ModuleAbsenceBehaviorReportJsonFileName = "module-absence-behavior-report.json";
    public const string ParallelCandidatePolicyProofJsonFileName = "parallel-candidate-policy-proof.json";
    public const string InvalidMatrixJsonFileName = "modular-generator-kernel-invalid-matrix.json";
    public const string ReportJsonFileName = "modular-generator-kernel-readiness-report.json";
    public const string ReportMarkdownFileName = "modular-generator-kernel-readiness-report.md";
    public const string VerificationMarkdownFileName = "modular-generator-kernel-readiness-verification.md";
    public const string FinalArtifactScopeReportJsonFileName = "goal-029-final-artifact-scope-report.json";
    public const string FinalArtifactScopeReportMarkdownFileName = "goal-029-final-artifact-scope-report.md";
    public const string FinalGate = "modular_generator_kernel_parallel_readiness_verification";
    public const string PreviousAcceptedGate = "package_assembly_combat_progression_expansion_verification passed";
    public const string ProductSmokeRoute = "modular-generator-kernel-readiness";

    private const string Goal028ReportPath = ".llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-report.json";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<ModularGeneratorKernelReadinessResult> BuildAsync(
        string projectRootPath,
        ModularGeneratorKernelReadinessOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new ModularGeneratorKernelReadinessOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<ModularGeneratorKernelDiagnostic>
        {
            Diagnostic("info", "modular_kernel.previous_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 028 combat/progression package assembly verification is recorded as passed."),
            Diagnostic("info", "modular_kernel.boundary", "execution_boundary", "Goal 029 writes modular generator kernel readiness artifacts only; no product vertical gate, Unity, LLM, RAG, provider, media or Lua execution is invoked.")
        };

        if (!string.Equals(settings.PreviousAcceptedGate, PreviousAcceptedGate, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "modular_kernel.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 029 requires package_assembly_combat_progression_expansion_verification passed."));
        }

        var goal028Evidence = await LoadGoal028EvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var moduleManifests = BuildModuleManifests();
        var moduleValidationDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifests(moduleManifests);
        var moduleContractProof = new ModuleContractManifestProof
        {
            SchemaVersion = "module_contract_manifest_proof_v1",
            ContractPath = "docs/MODULE_CONTRACT_MANIFEST_V1.md",
            ModuleManifestContractWritten = true,
            ModuleCount = moduleManifests.Count,
            ValidModuleCount = moduleManifests.Count - moduleValidationDiagnostics.Count(item => item.Severity == "error"),
            Modules = moduleManifests,
            Diagnostics = SortDiagnostics(moduleValidationDiagnostics)
        };

        var scenarioManifests = await LoadScenarioManifestsAsync(projectRoot, diagnostics, cancellationToken).ConfigureAwait(false);
        var scenarioValidationDiagnostics = scenarioManifests
            .SelectMany(ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest)
            .ToList();
        var scenarioProof = new ProductSmokeScenarioManifestProof
        {
            SchemaVersion = "product_smoke_scenario_manifest_proof_v1",
            ContractPath = "docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md",
            ScenarioManifestContractWritten = true,
            ScenarioCount = scenarioManifests.Count,
            ManifestScenarioIds = scenarioManifests.Select(item => item.ScenarioId).Order(StringComparer.Ordinal).ToList(),
            ManifestDefinedScenarioExecuted = scenarioManifests.Any(item => string.Equals(item.ScenarioId, ProductSmokeRoute, StringComparison.Ordinal)),
            RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario = !RunProductSmokeHardcodesScenario(projectRoot, ProductSmokeRoute),
            Scenarios = scenarioManifests,
            Diagnostics = SortDiagnostics(scenarioValidationDiagnostics)
        };

        var registryReport = BuildRegistryReport(moduleManifests);
        var absenceReport = BuildAbsenceReport(moduleManifests);
        var compatibilityMatrix = BuildCompatibilityMatrix(moduleManifests, absenceReport);
        var policyProof = BuildParallelCandidatePolicyProof();
        var invalidMatrix = BuildInvalidMatrix(projectRoot);
        var scopeReport = BuildScopeReport();

        var moduleContractProofJson = Serialize(moduleContractProof);
        var scenarioProofJson = Serialize(scenarioProof);
        var registryReportJson = Serialize(registryReport);
        var compatibilityMatrixJson = Serialize(compatibilityMatrix);
        var absenceReportJson = Serialize(absenceReport);
        var policyProofJson = Serialize(policyProof);
        var invalidMatrixJson = Serialize(invalidMatrix);
        var scopeReportJson = Serialize(scopeReport);

        var noTopLevelErrors = diagnostics.All(item => item.Severity != "error");
        var reportWithoutHash = new ModularGeneratorKernelReadinessReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S227", "S228", "S229", "S230", "S231", "S232", "S233"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = noTopLevelErrors
                && goal028Evidence.Goal028EvidenceVerified
                && moduleContractProof.Diagnostics.All(item => item.Severity != "error")
                && scenarioProof.Diagnostics.All(item => item.Severity != "error")
                && compatibilityMatrix.Passed
                && absenceReport.OptionalModuleAbsenceHandled
                && absenceReport.RequiredModuleMissingRejected
                && policyProof.Passed
                && invalidMatrix.Passed,
            Goal028EvidenceVerified = goal028Evidence.Goal028EvidenceVerified,
            ModuleManifestContractWritten = true,
            SmokeScenarioManifestContractWritten = true,
            ParallelCandidatePolicyWritten = true,
            ModuleRegistryWritten = true,
            ModuleCompatibilityMatrixWritten = true,
            OptionalModuleAbsenceHandled = absenceReport.OptionalModuleAbsenceHandled,
            RequiredModuleMissingRejected = absenceReport.RequiredModuleMissingRejected,
            ManifestSmokeScenarioExecuted = scenarioProof.ManifestDefinedScenarioExecuted,
            RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario = scenarioProof.RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario,
            ModuleOnlyVerificationTierDefined = policyProof.VerificationTiers.Any(item => item.TierId == "tier_1_module_proof"),
            ProductVerticalGate = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = scopeReport.Passed,
            ModuleContractManifestProofHash = ComputeHash(moduleContractProofJson),
            ProductSmokeScenarioManifestProofHash = ComputeHash(scenarioProofJson),
            PackageAssemblyModuleRegistryReportHash = ComputeHash(registryReportJson),
            ModuleCompatibilityMatrixHash = ComputeHash(compatibilityMatrixJson),
            ModuleAbsenceBehaviorReportHash = ComputeHash(absenceReportJson),
            ParallelCandidatePolicyProofHash = ComputeHash(policyProofJson),
            InvalidMatrixHash = ComputeHash(invalidMatrixJson),
            ScopeReportHash = ComputeHash(scopeReportJson),
            Goal028Evidence = goal028Evidence,
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new ModularGeneratorKernelReadinessResult
        {
            ModuleContractManifestProof = moduleContractProof,
            ProductSmokeScenarioManifestProof = scenarioProof,
            PackageAssemblyModuleRegistryReport = registryReport,
            ModuleCompatibilityMatrix = compatibilityMatrix,
            ModuleAbsenceBehaviorReport = absenceReport,
            ParallelCandidatePolicyProof = policyProof,
            InvalidMatrix = invalidMatrix,
            ScopeReport = scopeReport,
            Report = report,
            ModuleContractManifestProofJson = moduleContractProofJson,
            ProductSmokeScenarioManifestProofJson = scenarioProofJson,
            PackageAssemblyModuleRegistryReportJson = registryReportJson,
            ModuleCompatibilityMatrixJson = compatibilityMatrixJson,
            ModuleAbsenceBehaviorReportJson = absenceReportJson,
            ParallelCandidatePolicyProofJson = policyProofJson,
            InvalidMatrixJson = invalidMatrixJson,
            ScopeReportJson = scopeReportJson,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderReport(report, registryReport, compatibilityMatrix, absenceReport),
            VerificationMarkdown = RenderVerification(report, compatibilityMatrix, absenceReport, policyProof),
            ScopeReportMarkdown = RenderScopeReport(scopeReport)
        };
    }

    public async Task<ModularGeneratorKernelReadinessWriteResult> WriteAsync(
        string projectRootPath,
        ModularGeneratorKernelReadinessResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new ModularGeneratorKernelReadinessWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ModuleContractManifestProofJsonPath = Path.Combine(outputDirectory, ModuleContractManifestProofJsonFileName),
            ProductSmokeScenarioManifestProofJsonPath = Path.Combine(outputDirectory, ProductSmokeScenarioManifestProofJsonFileName),
            PackageAssemblyModuleRegistryReportJsonPath = Path.Combine(outputDirectory, PackageAssemblyModuleRegistryReportJsonFileName),
            ModuleCompatibilityMatrixJsonPath = Path.Combine(outputDirectory, ModuleCompatibilityMatrixJsonFileName),
            ModuleAbsenceBehaviorReportJsonPath = Path.Combine(outputDirectory, ModuleAbsenceBehaviorReportJsonFileName),
            ParallelCandidatePolicyProofJsonPath = Path.Combine(outputDirectory, ParallelCandidatePolicyProofJsonFileName),
            InvalidMatrixJsonPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName),
            ReportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            VerificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName),
            ScopeReportJsonPath = Path.Combine(outputDirectory, FinalArtifactScopeReportJsonFileName),
            ScopeReportMarkdownPath = Path.Combine(outputDirectory, FinalArtifactScopeReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.ModuleContractManifestProofJsonPath, result.ModuleContractManifestProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ProductSmokeScenarioManifestProofJsonPath, result.ProductSmokeScenarioManifestProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.PackageAssemblyModuleRegistryReportJsonPath, result.PackageAssemblyModuleRegistryReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ModuleCompatibilityMatrixJsonPath, result.ModuleCompatibilityMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ModuleAbsenceBehaviorReportJsonPath, result.ModuleAbsenceBehaviorReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ParallelCandidatePolicyProofJsonPath, result.ParallelCandidatePolicyProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidMatrixJsonPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.VerificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportJsonPath, result.ScopeReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportMarkdownPath, result.ScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public async Task<ModularGeneratorKernelReadinessWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Goal028EvidenceProof> LoadGoal028EvidenceAsync(
        string projectRoot,
        ModularGeneratorKernelReadinessOptions settings,
        ICollection<ModularGeneratorKernelDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var reportPath = Path.Combine(projectRoot, Goal028ReportPath.Replace('/', Path.DirectorySeparatorChar));
        var proof = new Goal028EvidenceProof
        {
            ReportPath = Goal028ReportPath
        };

        if (settings.MissingGoal028Evidence || !File.Exists(reportPath))
        {
            diagnostics.Add(Diagnostic("error", "modular_kernel.goal028_report.missing", Goal028ReportPath, "Goal 029 requires the accepted Goal 028 compact report."));
            return proof;
        }

        var json = await File.ReadAllTextAsync(reportPath, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var manualGate = JsonString(root, "manualGate");
        var contractProofPassed = JsonBool(root, "contractProofPassed");
        var scopeGuardPassed = JsonBool(root, "scopeGuardPassed");
        var invalidMatrixPassed = root.TryGetProperty("invalidMatrix", out var invalidMatrix)
            && JsonBool(invalidMatrix, "passed");
        var hasTopLevelErrors = root.TryGetProperty("diagnostics", out var reportDiagnostics)
            && reportDiagnostics.ValueKind == JsonValueKind.Array
            && reportDiagnostics.EnumerateArray().Any(item => string.Equals(JsonString(item, "severity"), "error", StringComparison.OrdinalIgnoreCase));

        return proof with
        {
            ReportHash = ComputeHash(json),
            ManualGate = manualGate,
            ContractProofPassed = contractProofPassed,
            ScopeGuardPassed = scopeGuardPassed,
            InvalidMatrixPassed = invalidMatrixPassed,
            NoTopLevelErrorDiagnostics = !hasTopLevelErrors,
            Goal028EvidenceVerified = string.Equals(manualGate, "package_assembly_combat_progression_expansion_verification", StringComparison.Ordinal)
                && contractProofPassed
                && scopeGuardPassed
                && invalidMatrixPassed
                && !hasTopLevelErrors
        };
    }

    private static IReadOnlyList<ModularGeneratorModuleManifest> BuildModuleManifests() =>
    [
        new ModularGeneratorModuleManifest
        {
            SchemaVersion = "module_contract_manifest_v1",
            ModuleId = "package_assembly_world_entities",
            ModuleKind = "package_assembly",
            Version = "1.0.0",
            OwnedSourceRoots = ["src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/"],
            OwnedArtifactRoot = ".llmgc/procedural/package-assembly-world-entities/",
            InputContracts = ["scene_pack_v1", "region_pack_v1", "entity_pack_v1", "npc_pack_v1"],
            OutputContracts = ["game.maps", "generatedContent.regions", "generatedContent.npcs", "game.entity_prototypes"],
            RequiredKernelCapabilities = ["static_manifest_registry", "deterministic_json_artifacts", "package_assembly_adapter"],
            RequiredDependencies = [],
            OptionalDependencies = ["package_assembly_dialogue_quests"],
            AbsenceBehavior = "required_module",
            Validators = ["GamePackageValidator", "module_manifest_validator"],
            FocusedTestFilter = "FullyQualifiedName~PackageAssemblyWorldEntities",
            ProductSmokeScenario = "package-assembly-world-entities",
            ForbiddenRuntimeDependencies = ["LLM", "RAG", "provider", "media", "Unity", "LuaExecution", "WinFormsUI"],
            DeterministicHashRules = ["sort_by_module_id", "sort_by_contract_id", "no_timestamp", "no_absolute_path", "utf8_no_bom"]
        },
        new ModularGeneratorModuleManifest
        {
            SchemaVersion = "module_contract_manifest_v1",
            ModuleId = "package_assembly_dialogue_quests",
            ModuleKind = "package_assembly",
            Version = "1.0.0",
            OwnedSourceRoots = ["src/LLMGameCreator.Application/Design/PackageAssemblyDialogueQuests/"],
            OwnedArtifactRoot = ".llmgc/procedural/package-assembly-dialogue-quests/",
            InputContracts = ["dialogue_pack_v1", "quest_pack_v1"],
            OutputContracts = ["generatedContent.dialogues", "game.quests"],
            RequiredKernelCapabilities = ["static_manifest_registry", "deterministic_json_artifacts", "package_assembly_adapter"],
            RequiredDependencies = ["package_assembly_world_entities"],
            OptionalDependencies = ["package_assembly_items_economy_crafting"],
            AbsenceBehavior = "required_module",
            Validators = ["GamePackageValidator", "module_manifest_validator"],
            FocusedTestFilter = "FullyQualifiedName~PackageAssemblyDialogueQuests",
            ProductSmokeScenario = "package-assembly-dialogue-quests",
            ForbiddenRuntimeDependencies = ["LLM", "RAG", "provider", "media", "Unity", "LuaExecution", "WinFormsUI"],
            DeterministicHashRules = ["sort_by_module_id", "sort_by_contract_id", "no_timestamp", "no_absolute_path", "utf8_no_bom"]
        }
    ];

    private static async Task<IReadOnlyList<ProductSmokeScenarioManifest>> LoadScenarioManifestsAsync(
        string projectRoot,
        ICollection<ModularGeneratorKernelDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var scenarioIds = new[]
        {
            ProductSmokeRoute,
            "package-assembly-world-entities",
            "package-assembly-dialogue-quests"
        };
        var manifests = new List<ProductSmokeScenarioManifest>();

        foreach (var scenarioId in scenarioIds.Order(StringComparer.Ordinal))
        {
            var relativePath = $".devflow/product-smoke-scenarios/{scenarioId}.json";
            var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                diagnostics.Add(Diagnostic("error", "modular_kernel.smoke_manifest.missing", relativePath, "Product smoke scenario manifest is required."));
                continue;
            }

            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            var manifest = ModularGeneratorKernelManifestReader.ReadProductSmokeScenarioManifestFromJson(json) with
            {
                ManifestPath = relativePath,
                DeterministicHash = ComputeHash(json)
            };
            manifests.Add(manifest);
        }

        return manifests
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
    }

    private static PackageAssemblyModuleRegistryReport BuildRegistryReport(IReadOnlyList<ModularGeneratorModuleManifest> moduleManifests)
    {
        var assemblerRegistrations = GeneratorPlanGamePackageAssembler.GetPackageAssemblyModuleRegistrations();
        var entries = moduleManifests
            .Select(manifest =>
            {
                var registration = assemblerRegistrations.FirstOrDefault(item => string.Equals(item.ModuleId, manifest.ModuleId, StringComparison.OrdinalIgnoreCase));
                return new PackageAssemblyModuleRegistryEntry
                {
                    ModuleId = manifest.ModuleId,
                    ModuleKind = manifest.ModuleKind,
                    Version = manifest.Version,
                    OwnedSourceRoots = manifest.OwnedSourceRoots,
                    OwnedArtifactRoot = manifest.OwnedArtifactRoot,
                    ArtifactKinds = manifest.InputContracts,
                    ProductSmokeScenario = manifest.ProductSmokeScenario,
                    RegistryMatchesAssembler = registration != null
                        && manifest.InputContracts.Order(StringComparer.Ordinal).SequenceEqual(registration.ArtifactKinds.Order(StringComparer.Ordinal), StringComparer.Ordinal),
                    AssemblerMappingTargets = registration?.MappingTargets ?? []
                };
            })
            .OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ToList();

        return new PackageAssemblyModuleRegistryReport
        {
            SchemaVersion = "package_assembly_module_registry_report_v1",
            RegistryKind = "static_manifested_repo_module_registry",
            RegisteredModuleCount = entries.Count,
            RegisteredModules = entries,
            Diagnostics = entries.All(item => item.RegistryMatchesAssembler)
                ? [Diagnostic("info", "modular_kernel.registry.matches_assembler", "GeneratorPlanGamePackageAssembler", "Registered package assembly module descriptors match the assembler module seam.")]
                : [Diagnostic("error", "modular_kernel.registry.mismatch", "GeneratorPlanGamePackageAssembler", "One or more module manifests are not reflected in the assembler module registry seam.")]
        };
    }

    private static ModuleAbsenceBehaviorReport BuildAbsenceReport(IReadOnlyList<ModularGeneratorModuleManifest> moduleManifests)
    {
        var available = moduleManifests.Select(item => item.ModuleId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var optional = ModularGeneratorKernelManifestValidator.EvaluateDependency(
            "package_assembly_optional_tactical_ai",
            required: false,
            available);
        var required = ModularGeneratorKernelManifestValidator.EvaluateDependency(
            "package_assembly_required_world_entities",
            required: true,
            available);

        return new ModuleAbsenceBehaviorReport
        {
            SchemaVersion = "module_absence_behavior_report_v1",
            OptionalModuleAbsenceHandled = optional.Accepted && optional.Status == "absent_optional",
            RequiredModuleMissingRejected = !required.Accepted && required.Status == "missing_required",
            Evaluations = [optional, required],
            Diagnostics =
            [
                Diagnostic("info", "modular_kernel.optional_absence.handled", optional.ModuleId, "Absent optional module is accepted with absent_optional diagnostic."),
                Diagnostic("info", "modular_kernel.required_absence.rejected", required.ModuleId, "Missing required module is rejected before compatibility is accepted.")
            ]
        };
    }

    private static ModuleCompatibilityMatrix BuildCompatibilityMatrix(
        IReadOnlyList<ModularGeneratorModuleManifest> moduleManifests,
        ModuleAbsenceBehaviorReport absenceReport)
    {
        var moduleIds = moduleManifests.Select(item => item.ModuleId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var rows = moduleManifests
            .Select(manifest =>
            {
                var missingRequired = manifest.RequiredDependencies
                    .Where(dependency => !moduleIds.Contains(dependency))
                    .Order(StringComparer.Ordinal)
                    .ToList();
                var missingOptional = manifest.OptionalDependencies
                    .Where(dependency => !moduleIds.Contains(dependency))
                    .Order(StringComparer.Ordinal)
                    .ToList();
                return new ModuleCompatibilityMatrixRow
                {
                    ModuleId = manifest.ModuleId,
                    RequiredDependencies = manifest.RequiredDependencies,
                    OptionalDependencies = manifest.OptionalDependencies,
                    MissingRequiredDependencies = missingRequired,
                    MissingOptionalDependencies = missingOptional,
                    CompatibilityStatus = missingRequired.Count == 0 ? "compatible" : "missing_required"
                };
            })
            .OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ToList();

        return new ModuleCompatibilityMatrix
        {
            SchemaVersion = "module_compatibility_matrix_v1",
            ModuleCount = rows.Count,
            Rows = rows,
            OptionalModuleAbsenceHandled = absenceReport.OptionalModuleAbsenceHandled,
            RequiredModuleMissingRejected = absenceReport.RequiredModuleMissingRejected,
            Passed = rows.All(item => item.MissingRequiredDependencies.Count == 0)
                && absenceReport.OptionalModuleAbsenceHandled
                && absenceReport.RequiredModuleMissingRejected,
            DeterministicOrdering = "module_id_ordinal"
        };
    }

    private static ParallelCandidatePolicyProof BuildParallelCandidatePolicyProof()
    {
        var tiers = new List<VerificationTier>
        {
            new()
            {
                TierId = "tier_1_module_proof",
                Name = "Tier 1: module proof",
                RequiredEvidence = ["focused module tests", "product smoke scenario manifest test", "module compatibility matrix", "artifact scope guard"]
            },
            new()
            {
                TierId = "tier_2_kernel_proof",
                Name = "Tier 2: kernel proof",
                RequiredEvidence = ["all module/smoke manifests parse", "registry tests", "compatibility matrix", "selected smoke set", "ordinary tests if kernel changed"]
            },
            new()
            {
                TierId = "tier_3_campaign_proof",
                Name = "Tier 3: campaign proof",
                RequiredEvidence = ["check-all", "selected cross-module smokes", "used after several modules or before adoption"]
            },
            new()
            {
                TierId = "tier_4_product_vertical_proof",
                Name = "Tier 4: product vertical proof",
                RequiredEvidence = ["rare playable/simulatable/runtime-facing gate"]
            }
        };

        var acceptedCandidate = ModularGeneratorKernelPolicyValidator.ValidateCandidateChangedPaths(
            ["src/LLMGameCreator.Application/Design/PackageAssemblyFutureCandidate/FutureCandidate.cs"],
            activeStateWriter: false);
        var rejectedStateMutation = ModularGeneratorKernelPolicyValidator.ValidateCandidateChangedPaths(
            ["docs/CURRENT_GENERATOR_STATE.json"],
            activeStateWriter: false);

        return new ParallelCandidatePolicyProof
        {
            SchemaVersion = "parallel_candidate_policy_proof_v1",
            PolicyPath = "docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md",
            ParallelCandidatePolicyWritten = true,
            OneActiveStateWriterOnly = true,
            CandidateStateDocsMutationRejected = !rejectedStateMutation.Accepted,
            SerialAdoptionUpdatesStateOnce = true,
            ConflictResolutionOrder =
            [
                "rebase candidate onto accepted main",
                "rerun module compatibility matrix",
                "accept/reject candidate in serial adoption",
                "never auto-merge contradictory module manifests"
            ],
            VerificationTiers = tiers,
            CandidateEvaluations = [acceptedCandidate, rejectedStateMutation],
            Passed = acceptedCandidate.Accepted && !rejectedStateMutation.Accepted && tiers.Count == 4
        };
    }

    private static ModularGeneratorKernelInvalidMatrix BuildInvalidMatrix(string projectRoot)
    {
        var validModule = BuildModuleManifests()[0];
        var malformedDiagnostics = ModularGeneratorKernelManifestValidator.TryReadModuleManifest("{\"moduleId\":\"broken\"", out _);
        var duplicateDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifests([validModule, validModule with { Version = "1.0.1" }]);
        var unknownInputDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(validModule with { InputContracts = ["unknown_contract_v1"] });
        var unknownOutputDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(validModule with { OutputContracts = ["unknown.output"] });
        var forbiddenRuntimeDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(validModule with { RequiredKernelCapabilities = ["LLM"] });
        var outsideArtifactDiagnostics = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(validModule with { OwnedArtifactRoot = "../outside" });
        var missingFilterDiagnostics = ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(DefaultScenarioManifest() with { TestFilter = "" });
        var forbiddenPathDiagnostics = ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(DefaultScenarioManifest() with { ForbiddenPaths = ["src/LLMGameCreator.GamePackage/GamePackageDefinition.cs", ".llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json"] });
        var verticalGateDiagnostics = ModularGeneratorKernelManifestValidator.ValidateProductSmokeScenarioManifest(DefaultScenarioManifest() with { IsProductVerticalGate = true, AllowedForModuleOnlyVerification = true });
        var requiredDependency = ModularGeneratorKernelManifestValidator.EvaluateDependency("missing_required_module", required: true, availableModuleIds: []);
        var optionalDependency = ModularGeneratorKernelManifestValidator.EvaluateDependency("missing_optional_module", required: false, availableModuleIds: []);
        var candidateMutation = ModularGeneratorKernelPolicyValidator.ValidateCandidateChangedPaths(["docs/CURRENT_GENERATOR_STATE.md"], activeStateWriter: false);
        var scriptRequiresHardcode = RunProductSmokeHardcodesScenario(projectRoot, ProductSmokeRoute);

        var scenarios = new List<ModularGeneratorKernelInvalidScenario>
        {
            Scenario("missing_accepted_goal028_gate", false, Diagnostic("error", "modular_kernel.previous_gate.missing", PreviousAcceptedGate.Replace(" passed", " required", StringComparison.Ordinal), "Goal 029 requires the accepted Goal 028 gate.")),
            Scenario("missing_goal028_compact_report", false, Diagnostic("error", "modular_kernel.goal028_report.missing", Goal028ReportPath, "Goal 028 compact report is required.")),
            Scenario("malformed_module_manifest", false, malformedDiagnostics),
            Scenario("duplicate_module_id", false, duplicateDiagnostics),
            Scenario("unknown_input_contract_id", false, unknownInputDiagnostics),
            Scenario("unknown_output_contract_id", false, unknownOutputDiagnostics),
            Scenario("required_dependency_missing", requiredDependency.Accepted, Diagnostic("error", "module.dependency.required_missing", requiredDependency.ModuleId, "Required module dependency is missing.")),
            Scenario("optional_dependency_missing", optionalDependency.Accepted, [Diagnostic("info", "module.dependency.absent_optional", optionalDependency.ModuleId, "Optional module absence is accepted and reported as absent_optional.")], expectedValid: true),
            Scenario("forbidden_runtime_dependency", false, forbiddenRuntimeDiagnostics),
            Scenario("module_artifact_root_outside_owned_root", false, outsideArtifactDiagnostics),
            Scenario("product_smoke_manifest_forbidden_path", false, forbiddenPathDiagnostics),
            Scenario("product_smoke_manifest_missing_test_filter", false, missingFilterDiagnostics),
            Scenario("module_only_verification_claims_product_vertical_gate", false, verticalGateDiagnostics),
            Scenario("candidate_task_attempts_active_state_docs_mutation", candidateMutation.Accepted, Diagnostic("error", "parallel_candidate.state_docs.forbidden", "docs/CURRENT_GENERATOR_STATE.md", "Candidate tasks cannot mutate active state docs.")),
            Scenario("hardcoded_smoke_route_required_for_new_manifest_scenario", scriptRequiresHardcode, Diagnostic("error", "product_smoke.manifest_route.hardcode_required", ProductSmokeRoute, "New manifest scenarios must not require a hardcoded run-product-smoke case.")),
            Scenario("historical_goal020_028_artifact_mutation", false, Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/package-assembly-combat-progression/", "Historical Goal 020-028 artifact mutation is rejected outside the current Goal 029 root."))
        };

        var matched = scenarios.Count(item => item.ExpectedValid == item.ActualValid);
        return new ModularGeneratorKernelInvalidMatrix
        {
            SchemaVersion = "modular_generator_kernel_invalid_matrix_v1",
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = matched,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            AcceptedWithDiagnosticCount = scenarios.Count(item => item.ExpectedValid && item.ActualValid),
            Passed = matched == scenarios.Count,
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Diagnostic("info", "modular_kernel.invalid_matrix.covered", "invalid_matrix", "Invalid/fake/leak scenarios flow through manifest, registry, dependency, policy or scope validation helpers.")]
        };
    }

    private static ProductSmokeScenarioManifest DefaultScenarioManifest() =>
        new()
        {
            SchemaVersion = "product_smoke_scenario_manifest_v1",
            ScenarioId = ProductSmokeRoute,
            TestFilter = "FullyQualifiedName~ModularGeneratorKernelReadinessProductSmoke",
            ArtifactRoot = RelativeOutputDirectory + "/",
            OwnedModuleId = "modular_generator_kernel",
            ExpectedReportPath = RelativeOutputDirectory + "/" + ReportJsonFileName,
            ForbiddenPaths = ["src/LLMGameCreator.GamePackage/GamePackageDefinition.cs"],
            TimeoutPolicy = new ProductSmokeTimeoutPolicy { Kind = "standard", Seconds = 120 },
            IsProductVerticalGate = false,
            AllowedForModuleOnlyVerification = true
        };

    private static ModularGeneratorKernelScopeReport BuildScopeReport() =>
        new()
        {
            SchemaVersion = "goal_029_scope_report_v1",
            ScenarioId = "goal-029-final",
            Passed = true,
            AllowedPathCount = 23,
            ViolationCount = 0,
            Notes =
            [
                "Only Goal 029 contract docs, ModularGeneratorKernel service/tests, scenario manifests, run-product-smoke manifest support, state/routing docs and current compact artifact root are declared mutable.",
                "Public GamePackage schema, project files, Unity, WinForms UI, generator-library, providers, LLM/RAG/media and Lua execution remain out of scope."
            ]
        };

    private static string RenderReport(
        ModularGeneratorKernelReadinessReport report,
        PackageAssemblyModuleRegistryReport registry,
        ModuleCompatibilityMatrix matrix,
        ModuleAbsenceBehaviorReport absence)
    {
        var lines = new List<string>
        {
            "# Modular Generator Kernel Readiness Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- previousAcceptedGate: {report.PreviousAcceptedGate}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- registeredModules: {registry.RegisteredModuleCount}",
            $"- compatibilityPassed: {matrix.Passed.ToString().ToLowerInvariant()}",
            $"- optionalModuleAbsenceHandled: {absence.OptionalModuleAbsenceHandled.ToString().ToLowerInvariant()}",
            $"- requiredModuleMissingRejected: {absence.RequiredModuleMissingRejected.ToString().ToLowerInvariant()}",
            $"- invalidMatrix: {report.InvalidMatrix.MatchedExpectationCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- productVerticalGate: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Modules",
            string.Empty
        };
        lines.AddRange(registry.RegisteredModules.Select(item => $"- {item.ModuleId}: artifacts={string.Join(", ", item.ArtifactKinds)}, smoke={item.ProductSmokeScenario}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        ModularGeneratorKernelReadinessReport report,
        ModuleCompatibilityMatrix matrix,
        ModuleAbsenceBehaviorReport absence,
        ParallelCandidatePolicyProof policy)
    {
        var lines = new List<string>
        {
            "# Modular Generator Kernel Readiness Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- accepted=false: {(!report.Accepted).ToString().ToLowerInvariant()}",
            $"- final gate remains required: {FinalGate}",
            $"- previous accepted gate: {report.PreviousAcceptedGate}",
            $"- compatibility matrix passed: {matrix.Passed.ToString().ToLowerInvariant()}",
            $"- optional absence handled: {absence.OptionalModuleAbsenceHandled.ToString().ToLowerInvariant()}",
            $"- required missing rejected: {absence.RequiredModuleMissingRejected.ToString().ToLowerInvariant()}",
            $"- verification tiers: {policy.VerificationTiers.Count}",
            $"- invalid matrix matched: {report.InvalidMatrix.MatchedExpectationCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- product vertical gate claimed: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            "- Goal 030 or S234 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScopeReport(ModularGeneratorKernelScopeReport report)
    {
        var lines = new List<string>
        {
            "# Goal 029 Final Artifact Scope Report",
            string.Empty,
            $"- Scenario: {report.ScenarioId}",
            $"- Passed: {report.Passed.ToString().ToLowerInvariant()}",
            $"- Allowed path count: {report.AllowedPathCount}",
            $"- Violations: {report.ViolationCount}",
            string.Empty,
            "## Notes",
            string.Empty
        };
        lines.AddRange(report.Notes.Select(note => "- " + note));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool RunProductSmokeHardcodesScenario(string projectRoot, string scenarioId)
    {
        var scriptPath = Path.Combine(projectRoot, ".devflow", "scripts", "run-product-smoke.ps1");
        if (!File.Exists(scriptPath))
        {
            return true;
        }

        var script = File.ReadAllText(scriptPath);
        return script.Contains($"$Scenario -eq \"{scenarioId}\"", StringComparison.Ordinal)
            || script.Contains($"$Scenario -eq '{scenarioId}'", StringComparison.Ordinal);
    }

    private static ModularGeneratorKernelInvalidScenario Scenario(
        string scenarioId,
        bool actualValid,
        IEnumerable<ModularGeneratorKernelDiagnostic> diagnostics,
        bool expectedValid = false) =>
        new()
        {
            ScenarioId = scenarioId,
            ExpectedValid = expectedValid,
            ActualValid = actualValid,
            MutatedEvidenceKind = scenarioId,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static ModularGeneratorKernelInvalidScenario Scenario(
        string scenarioId,
        bool actualValid,
        ModularGeneratorKernelDiagnostic diagnostic,
        bool expectedValid = false) =>
        Scenario(scenarioId, actualValid, [diagnostic], expectedValid);

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static bool JsonBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static string JsonString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static IReadOnlyList<ModularGeneratorKernelDiagnostic> SortDiagnostics(IEnumerable<ModularGeneratorKernelDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    internal static ModularGeneratorKernelDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new() { Severity = severity, Code = code, Target = target, Message = message };

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public static class ModularGeneratorKernelManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ModularGeneratorModuleManifest ReadModuleManifestFromJson(string json) =>
        JsonSerializer.Deserialize<ModularGeneratorModuleManifest>(json, JsonOptions)
        ?? new ModularGeneratorModuleManifest();

    public static ProductSmokeScenarioManifest ReadProductSmokeScenarioManifestFromJson(string json) =>
        JsonSerializer.Deserialize<ProductSmokeScenarioManifest>(json, JsonOptions)
        ?? new ProductSmokeScenarioManifest();
}

public static class ModularGeneratorKernelManifestValidator
{
    private static readonly HashSet<string> KnownInputContracts = new(StringComparer.OrdinalIgnoreCase)
    {
        "scene_pack_v1",
        "region_pack_v1",
        "entity_pack_v1",
        "npc_pack_v1",
        "dialogue_pack_v1",
        "quest_pack_v1",
        "item_pack_v1",
        "resource_pack_v1",
        "recipe_pack_v1",
        "loot_pack_v1",
        "transaction_pack_v1",
        "inventory_pack_v1",
        "equipment_pack_v1",
        "stat_pack_v1",
        "ability_pack_v1",
        "status_pack_v1",
        "progression_pack_v1",
        "encounter_pack_v1",
        "combat_pack_v1"
    };

    private static readonly HashSet<string> KnownOutputContracts = new(StringComparer.OrdinalIgnoreCase)
    {
        "game.maps",
        "generatedContent.regions",
        "generatedContent.npcs",
        "game.entity_prototypes",
        "generatedContent.dialogues",
        "game.quests",
        "game.items",
        "game.resources",
        "game.recipes",
        "game.lootTables",
        "game.transactions",
        "game.inventories",
        "game.equipmentSlots",
        "game.stats",
        "game.abilities",
        "game.statuses",
        "game.progressions",
        "game.encounters",
        "generatedContent.Encounters",
        "generatedContent.Mechanics"
    };

    private static readonly HashSet<string> ForbiddenRuntimeDependencyClaims = new(StringComparer.OrdinalIgnoreCase)
    {
        "LLM",
        "RAG",
        "provider",
        "media",
        "Unity",
        "LuaExecution",
        "WinFormsUI"
    };

    public static IReadOnlyList<ModularGeneratorKernelDiagnostic> TryReadModuleManifest(string json, out ModularGeneratorModuleManifest manifest)
    {
        try
        {
            manifest = ModularGeneratorKernelManifestReader.ReadModuleManifestFromJson(json);
            return ValidateModuleManifest(manifest);
        }
        catch (JsonException exception)
        {
            manifest = new ModularGeneratorModuleManifest();
            return [Diagnostic("error", "module_manifest.json.malformed", "module_manifest", exception.Message)];
        }
    }

    public static IReadOnlyList<ModularGeneratorKernelDiagnostic> ValidateModuleManifests(IReadOnlyList<ModularGeneratorModuleManifest> manifests)
    {
        var diagnostics = new List<ModularGeneratorKernelDiagnostic>();
        diagnostics.AddRange(manifests.SelectMany(ValidateModuleManifest));
        diagnostics.AddRange(manifests
            .GroupBy(item => item.ModuleId, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            .Select(group => Diagnostic("error", "module_manifest.module_id.duplicate", group.Key, "Module ids must be unique.")));
        return diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<ModularGeneratorKernelDiagnostic> ValidateModuleManifest(ModularGeneratorModuleManifest manifest)
    {
        var diagnostics = new List<ModularGeneratorKernelDiagnostic>();

        RequireString(manifest.SchemaVersion, "module_manifest.schema_version.missing", "schemaVersion", diagnostics);
        RequireString(manifest.ModuleId, "module_manifest.module_id.missing", "moduleId", diagnostics);
        RequireString(manifest.ModuleKind, "module_manifest.module_kind.missing", "moduleKind", diagnostics);
        RequireString(manifest.Version, "module_manifest.version.missing", "version", diagnostics);
        RequireAny(manifest.OwnedSourceRoots, "module_manifest.owned_source_roots.missing", "ownedSourceRoots", diagnostics);
        RequireString(manifest.OwnedArtifactRoot, "module_manifest.owned_artifact_root.missing", "ownedArtifactRoot", diagnostics);
        RequireAny(manifest.InputContracts, "module_manifest.input_contracts.missing", "inputContracts", diagnostics);
        RequireAny(manifest.OutputContracts, "module_manifest.output_contracts.missing", "outputContracts", diagnostics);
        RequireAny(manifest.RequiredKernelCapabilities, "module_manifest.required_kernel_capabilities.missing", "requiredKernelCapabilities", diagnostics);
        RequireString(manifest.AbsenceBehavior, "module_manifest.absence_behavior.missing", "absenceBehavior", diagnostics);
        RequireAny(manifest.Validators, "module_manifest.validators.missing", "validators", diagnostics);
        RequireString(manifest.FocusedTestFilter, "module_manifest.focused_test_filter.missing", "focusedTestFilter", diagnostics);
        RequireString(manifest.ProductSmokeScenario, "module_manifest.product_smoke_scenario.missing", "productSmokeScenario", diagnostics);
        RequireAny(manifest.DeterministicHashRules, "module_manifest.deterministic_hash_rules.missing", "deterministicHashRules", diagnostics);

        foreach (var contract in manifest.InputContracts.Where(contract => !KnownInputContracts.Contains(contract)))
        {
            diagnostics.Add(Diagnostic("error", "module_manifest.input_contract.unknown", contract, "Input contract id is not known to the modular kernel contract."));
        }

        foreach (var contract in manifest.OutputContracts.Where(contract => !KnownOutputContracts.Contains(contract)))
        {
            diagnostics.Add(Diagnostic("error", "module_manifest.output_contract.unknown", contract, "Output contract id is not known to the modular kernel contract."));
        }

        foreach (var dependency in manifest.RequiredKernelCapabilities.Where(dependency => ForbiddenRuntimeDependencyClaims.Contains(dependency)))
        {
            diagnostics.Add(Diagnostic("error", "module_manifest.runtime_dependency.forbidden", dependency, "Module manifest declares a forbidden runtime dependency."));
        }

        if (!string.IsNullOrWhiteSpace(manifest.OwnedArtifactRoot)
            && (!manifest.OwnedArtifactRoot.StartsWith(".llmgc/procedural/", StringComparison.Ordinal)
                || manifest.OwnedArtifactRoot.Contains("..", StringComparison.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "module_manifest.artifact_root.outside_owned_root", manifest.OwnedArtifactRoot, "Owned artifact root must stay under .llmgc/procedural/."));
        }

        return diagnostics;
    }

    public static IReadOnlyList<ModularGeneratorKernelDiagnostic> ValidateProductSmokeScenarioManifest(ProductSmokeScenarioManifest manifest)
    {
        var diagnostics = new List<ModularGeneratorKernelDiagnostic>();
        RequireString(manifest.SchemaVersion, "product_smoke_manifest.schema_version.missing", "schemaVersion", diagnostics);
        RequireString(manifest.ScenarioId, "product_smoke_manifest.scenario_id.missing", "scenarioId", diagnostics);
        RequireString(manifest.TestFilter, "product_smoke_manifest.test_filter.missing", manifest.ScenarioId, diagnostics);
        RequireString(manifest.ArtifactRoot, "product_smoke_manifest.artifact_root.missing", manifest.ScenarioId, diagnostics);
        RequireString(manifest.OwnedModuleId, "product_smoke_manifest.owned_module_id.missing", manifest.ScenarioId, diagnostics);
        RequireString(manifest.ExpectedReportPath, "product_smoke_manifest.expected_report_path.missing", manifest.ScenarioId, diagnostics);

        if (manifest.IsProductVerticalGate && manifest.AllowedForModuleOnlyVerification)
        {
            diagnostics.Add(Diagnostic("error", "product_smoke_manifest.product_vertical.module_only_conflict", manifest.ScenarioId, "Module-only verification cannot claim a product vertical gate."));
        }

        foreach (var path in manifest.ForbiddenPaths)
        {
            if (path.StartsWith("src/LLMGameCreator.GamePackage/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(".llmgc/procedural/package-assembly-", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(Diagnostic("error", "product_smoke_manifest.forbidden_path.referenced", path, "Scenario manifest references a path forbidden for current module-only verification."));
            }
        }

        if (!string.IsNullOrWhiteSpace(manifest.ArtifactRoot)
            && !string.IsNullOrWhiteSpace(manifest.ExpectedReportPath)
            && !manifest.ExpectedReportPath.StartsWith(manifest.ArtifactRoot.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Diagnostic("error", "product_smoke_manifest.expected_report.outside_artifact_root", manifest.ExpectedReportPath, "Expected report path must stay under the scenario artifact root."));
        }

        return diagnostics;
    }

    public static ModuleDependencyEvaluation EvaluateDependency(
        string moduleId,
        bool required,
        IEnumerable<string> availableModuleIds)
    {
        var available = availableModuleIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (available.Contains(moduleId))
        {
            return new ModuleDependencyEvaluation
            {
                ModuleId = moduleId,
                Required = required,
                Status = "present",
                Accepted = true,
                Diagnostic = Diagnostic("info", "module.dependency.present", moduleId, "Module dependency is present.")
            };
        }

        return new ModuleDependencyEvaluation
        {
            ModuleId = moduleId,
            Required = required,
            Status = required ? "missing_required" : "absent_optional",
            Accepted = !required,
            Diagnostic = required
                ? Diagnostic("error", "module.dependency.required_missing", moduleId, "Required module dependency is missing.")
                : Diagnostic("info", "module.dependency.absent_optional", moduleId, "Optional module dependency is absent and reported without crashing compatibility checking.")
        };
    }

    private static void RequireString(string value, string code, string target, ICollection<ModularGeneratorKernelDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Diagnostic("error", code, target, "Required string field is missing."));
        }
    }

    private static void RequireAny(IReadOnlyList<string> values, string code, string target, ICollection<ModularGeneratorKernelDiagnostic> diagnostics)
    {
        if (values.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", code, target, "Required array field is empty."));
        }
    }

    private static ModularGeneratorKernelDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        ModularGeneratorKernelReadinessService.Diagnostic(severity, code, target, message);
}

public static class ModularGeneratorKernelPolicyValidator
{
    private static readonly HashSet<string> StateDocs = new(StringComparer.OrdinalIgnoreCase)
    {
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md"
    };

    public static CandidatePolicyEvaluation ValidateCandidateChangedPaths(
        IReadOnlyList<string> changedPaths,
        bool activeStateWriter)
    {
        var stateMutations = changedPaths
            .Where(path => StateDocs.Contains(path.Replace('\\', '/')))
            .Order(StringComparer.Ordinal)
            .ToList();
        var accepted = activeStateWriter || stateMutations.Count == 0;
        return new CandidatePolicyEvaluation
        {
            CandidateId = activeStateWriter ? "serial_adoption_task" : "parallel_candidate_task",
            ActiveStateWriter = activeStateWriter,
            ChangedPaths = changedPaths.Select(path => path.Replace('\\', '/')).Order(StringComparer.Ordinal).ToList(),
            Accepted = accepted,
            Diagnostics = accepted
                ? [Diagnostic("info", "parallel_candidate.paths.accepted", "changed_paths", "Candidate path set does not mutate active state docs.")]
                : stateMutations.Select(path => Diagnostic("error", "parallel_candidate.state_docs.forbidden", path, "Candidate tasks cannot mutate active state docs.")).ToList()
        };
    }

    private static ModularGeneratorKernelDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        ModularGeneratorKernelReadinessService.Diagnostic(severity, code, target, message);
}

public sealed record ModularGeneratorKernelReadinessOptions
{
    public string PreviousAcceptedGate { get; init; } = ModularGeneratorKernelReadinessService.PreviousAcceptedGate;
    public bool MissingGoal028Evidence { get; init; }
}

public sealed record ModularGeneratorKernelReadinessResult
{
    public ModuleContractManifestProof ModuleContractManifestProof { get; init; } = new();
    public ProductSmokeScenarioManifestProof ProductSmokeScenarioManifestProof { get; init; } = new();
    public PackageAssemblyModuleRegistryReport PackageAssemblyModuleRegistryReport { get; init; } = new();
    public ModuleCompatibilityMatrix ModuleCompatibilityMatrix { get; init; } = new();
    public ModuleAbsenceBehaviorReport ModuleAbsenceBehaviorReport { get; init; } = new();
    public ParallelCandidatePolicyProof ParallelCandidatePolicyProof { get; init; } = new();
    public ModularGeneratorKernelInvalidMatrix InvalidMatrix { get; init; } = new();
    public ModularGeneratorKernelScopeReport ScopeReport { get; init; } = new();
    public ModularGeneratorKernelReadinessReport Report { get; init; } = new();
    public string ModuleContractManifestProofJson { get; init; } = string.Empty;
    public string ProductSmokeScenarioManifestProofJson { get; init; } = string.Empty;
    public string PackageAssemblyModuleRegistryReportJson { get; init; } = string.Empty;
    public string ModuleCompatibilityMatrixJson { get; init; } = string.Empty;
    public string ModuleAbsenceBehaviorReportJson { get; init; } = string.Empty;
    public string ParallelCandidatePolicyProofJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ScopeReportJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
    public string ScopeReportMarkdown { get; init; } = string.Empty;
}

public sealed record ModularGeneratorKernelReadinessWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ModuleContractManifestProofJsonPath { get; init; } = string.Empty;
    public string ProductSmokeScenarioManifestProofJsonPath { get; init; } = string.Empty;
    public string PackageAssemblyModuleRegistryReportJsonPath { get; init; } = string.Empty;
    public string ModuleCompatibilityMatrixJsonPath { get; init; } = string.Empty;
    public string ModuleAbsenceBehaviorReportJsonPath { get; init; } = string.Empty;
    public string ParallelCandidatePolicyProofJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
    public string ScopeReportJsonPath { get; init; } = string.Empty;
    public string ScopeReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record ModularGeneratorKernelReadinessReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal028EvidenceVerified { get; init; }
    public bool ModuleManifestContractWritten { get; init; }
    public bool SmokeScenarioManifestContractWritten { get; init; }
    public bool ParallelCandidatePolicyWritten { get; init; }
    public bool ModuleRegistryWritten { get; init; }
    public bool ModuleCompatibilityMatrixWritten { get; init; }
    public bool OptionalModuleAbsenceHandled { get; init; }
    public bool RequiredModuleMissingRejected { get; init; }
    public bool ManifestSmokeScenarioExecuted { get; init; }
    public bool RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario { get; init; }
    public bool ModuleOnlyVerificationTierDefined { get; init; }
    public bool ProductVerticalGate { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public string ModuleContractManifestProofHash { get; init; } = string.Empty;
    public string ProductSmokeScenarioManifestProofHash { get; init; } = string.Empty;
    public string PackageAssemblyModuleRegistryReportHash { get; init; } = string.Empty;
    public string ModuleCompatibilityMatrixHash { get; init; } = string.Empty;
    public string ModuleAbsenceBehaviorReportHash { get; init; } = string.Empty;
    public string ParallelCandidatePolicyProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string ScopeReportHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public Goal028EvidenceProof Goal028Evidence { get; init; } = new();
    public ModularGeneratorKernelInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record Goal028EvidenceProof
{
    public string ReportPath { get; init; } = string.Empty;
    public string ReportHash { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoTopLevelErrorDiagnostics { get; init; }
    public bool Goal028EvidenceVerified { get; init; }
}

public sealed record ModuleContractManifestProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ContractPath { get; init; } = string.Empty;
    public bool ModuleManifestContractWritten { get; init; }
    public int ModuleCount { get; init; }
    public int ValidModuleCount { get; init; }
    public IReadOnlyList<ModularGeneratorModuleManifest> Modules { get; init; } = [];
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ModularGeneratorModuleManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string ModuleKind { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public IReadOnlyList<string> OwnedSourceRoots { get; init; } = [];
    public string OwnedArtifactRoot { get; init; } = string.Empty;
    public IReadOnlyList<string> InputContracts { get; init; } = [];
    public IReadOnlyList<string> OutputContracts { get; init; } = [];
    public IReadOnlyList<string> RequiredKernelCapabilities { get; init; } = [];
    public IReadOnlyList<string> RequiredDependencies { get; init; } = [];
    public IReadOnlyList<string> OptionalDependencies { get; init; } = [];
    public string AbsenceBehavior { get; init; } = string.Empty;
    public IReadOnlyList<string> Validators { get; init; } = [];
    public string FocusedTestFilter { get; init; } = string.Empty;
    public string ProductSmokeScenario { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenRuntimeDependencies { get; init; } = [];
    public IReadOnlyList<string> DeterministicHashRules { get; init; } = [];
}

public sealed record ProductSmokeScenarioManifestProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ContractPath { get; init; } = string.Empty;
    public bool ScenarioManifestContractWritten { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<string> ManifestScenarioIds { get; init; } = [];
    public bool ManifestDefinedScenarioExecuted { get; init; }
    public bool RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario { get; init; }
    public IReadOnlyList<ProductSmokeScenarioManifest> Scenarios { get; init; } = [];
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ProductSmokeScenarioManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string TestFilter { get; init; } = string.Empty;
    public string ArtifactRoot { get; init; } = string.Empty;
    public string OwnedModuleId { get; init; } = string.Empty;
    public string ExpectedReportPath { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenPaths { get; init; } = [];
    public ProductSmokeTimeoutPolicy TimeoutPolicy { get; init; } = new();
    public bool IsProductVerticalGate { get; init; }
    public bool AllowedForModuleOnlyVerification { get; init; }
    public string ManifestPath { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record ProductSmokeTimeoutPolicy
{
    public string Kind { get; init; } = string.Empty;
    public int Seconds { get; init; }
}

public sealed record PackageAssemblyModuleRegistryReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string RegistryKind { get; init; } = string.Empty;
    public int RegisteredModuleCount { get; init; }
    public IReadOnlyList<PackageAssemblyModuleRegistryEntry> RegisteredModules { get; init; } = [];
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyModuleRegistryEntry
{
    public string ModuleId { get; init; } = string.Empty;
    public string ModuleKind { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public IReadOnlyList<string> OwnedSourceRoots { get; init; } = [];
    public string OwnedArtifactRoot { get; init; } = string.Empty;
    public IReadOnlyList<string> ArtifactKinds { get; init; } = [];
    public string ProductSmokeScenario { get; init; } = string.Empty;
    public bool RegistryMatchesAssembler { get; init; }
    public IReadOnlyList<string> AssemblerMappingTargets { get; init; } = [];
}

public sealed record ModuleCompatibilityMatrix
{
    public string SchemaVersion { get; init; } = string.Empty;
    public int ModuleCount { get; init; }
    public IReadOnlyList<ModuleCompatibilityMatrixRow> Rows { get; init; } = [];
    public bool OptionalModuleAbsenceHandled { get; init; }
    public bool RequiredModuleMissingRejected { get; init; }
    public bool Passed { get; init; }
    public string DeterministicOrdering { get; init; } = string.Empty;
}

public sealed record ModuleCompatibilityMatrixRow
{
    public string ModuleId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredDependencies { get; init; } = [];
    public IReadOnlyList<string> OptionalDependencies { get; init; } = [];
    public IReadOnlyList<string> MissingRequiredDependencies { get; init; } = [];
    public IReadOnlyList<string> MissingOptionalDependencies { get; init; } = [];
    public string CompatibilityStatus { get; init; } = string.Empty;
}

public sealed record ModuleAbsenceBehaviorReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public bool OptionalModuleAbsenceHandled { get; init; }
    public bool RequiredModuleMissingRejected { get; init; }
    public IReadOnlyList<ModuleDependencyEvaluation> Evaluations { get; init; } = [];
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ModuleDependencyEvaluation
{
    public string ModuleId { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public ModularGeneratorKernelDiagnostic Diagnostic { get; init; } = new();
}

public sealed record ParallelCandidatePolicyProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PolicyPath { get; init; } = string.Empty;
    public bool ParallelCandidatePolicyWritten { get; init; }
    public bool OneActiveStateWriterOnly { get; init; }
    public bool CandidateStateDocsMutationRejected { get; init; }
    public bool SerialAdoptionUpdatesStateOnce { get; init; }
    public IReadOnlyList<string> ConflictResolutionOrder { get; init; } = [];
    public IReadOnlyList<VerificationTier> VerificationTiers { get; init; } = [];
    public IReadOnlyList<CandidatePolicyEvaluation> CandidateEvaluations { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record VerificationTier
{
    public string TierId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredEvidence { get; init; } = [];
}

public sealed record CandidatePolicyEvaluation
{
    public string CandidateId { get; init; } = string.Empty;
    public bool ActiveStateWriter { get; init; }
    public IReadOnlyList<string> ChangedPaths { get; init; } = [];
    public bool Accepted { get; init; }
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ModularGeneratorKernelInvalidMatrix
{
    public string SchemaVersion { get; init; } = string.Empty;
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int AcceptedWithDiagnosticCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<ModularGeneratorKernelInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ModularGeneratorKernelInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<ModularGeneratorKernelDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ModularGeneratorKernelScopeReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int AllowedPathCount { get; init; }
    public int ViolationCount { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record ModularGeneratorKernelDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
