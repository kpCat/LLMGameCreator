using System.Text.Json;
using LLMGameCreator.Application.Design.ModularGeneratorKernel;
using Xunit;

namespace LLMGameCreator.Tests.Application.ModularGeneratorKernel;

public sealed class ModularGeneratorKernelReadinessTests
{
    [Fact]
    public async Task BuildsDeterministicReadinessArtifacts()
    {
        using var temp = new TempDirectory();
        var service = new ModularGeneratorKernelReadinessService();
        var repoRoot = FindRepoRoot();

        var first = await service.BuildAsync(repoRoot);
        var second = await service.BuildAsync(repoRoot);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(ModularGeneratorKernelReadinessService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(ModularGeneratorKernelReadinessService.FinalGate, first.Report.ManualGate);
        Assert.Equal(ModularGeneratorKernelReadinessService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.True(first.Report.Goal028EvidenceVerified);
        Assert.True(first.Report.ModuleManifestContractWritten);
        Assert.True(first.Report.SmokeScenarioManifestContractWritten);
        Assert.True(first.Report.ModuleRegistryWritten);
        Assert.True(first.Report.ModuleCompatibilityMatrixWritten);
        Assert.True(first.Report.OptionalModuleAbsenceHandled);
        Assert.True(first.Report.RequiredModuleMissingRejected);
        Assert.True(first.Report.ManifestSmokeScenarioExecuted);
        Assert.True(first.Report.RunProductSmokeHardcodedRouteNotRequiredForNewManifestScenario);
        Assert.True(first.Report.ModuleOnlyVerificationTierDefined);
        Assert.False(first.Report.ProductVerticalGate);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(first.Report.ModuleCompatibilityMatrixHash, second.Report.ModuleCompatibilityMatrixHash);
        Assert.True(File.Exists(write.ModuleContractManifestProofJsonPath));
        Assert.True(File.Exists(write.ProductSmokeScenarioManifestProofJsonPath));
        Assert.True(File.Exists(write.PackageAssemblyModuleRegistryReportJsonPath));
        Assert.True(File.Exists(write.ModuleCompatibilityMatrixJsonPath));
        Assert.True(File.Exists(write.ModuleAbsenceBehaviorReportJsonPath));
        Assert.True(File.Exists(write.ParallelCandidatePolicyProofJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ScopeReportJsonPath));
    }

    [Fact]
    public void ModuleManifestParserValidatorRejectsMalformedDuplicateAndUnknownContracts()
    {
        var malformed = ModularGeneratorKernelManifestValidator.TryReadModuleManifest("{\"moduleId\":\"broken\"", out _);
        var valid = ValidModuleManifest();
        var duplicate = ModularGeneratorKernelManifestValidator.ValidateModuleManifests([valid, valid with { Version = "1.0.1" }]);
        var unknownInput = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(valid with { InputContracts = ["unknown_contract_v1"] });
        var unknownOutput = ModularGeneratorKernelManifestValidator.ValidateModuleManifest(valid with { OutputContracts = ["unknown.output"] });

        Assert.Contains(malformed, item => item.Code == "module_manifest.json.malformed");
        Assert.Contains(duplicate, item => item.Code == "module_manifest.module_id.duplicate");
        Assert.Contains(unknownInput, item => item.Code == "module_manifest.input_contract.unknown");
        Assert.Contains(unknownOutput, item => item.Code == "module_manifest.output_contract.unknown");
    }

    [Fact]
    public async Task CompatibilityMatrixAndAbsenceBehaviorAreDeterministic()
    {
        var result = await new ModularGeneratorKernelReadinessService().BuildAsync(FindRepoRoot());

        Assert.True(result.ModuleCompatibilityMatrix.Passed);
        Assert.Contains(result.ModuleCompatibilityMatrix.Rows, row => row.ModuleId == "package_assembly_world_entities" && row.CompatibilityStatus == "compatible");
        Assert.Contains(result.ModuleCompatibilityMatrix.Rows, row => row.ModuleId == "package_assembly_dialogue_quests" && row.MissingRequiredDependencies.Count == 0);
        Assert.True(result.ModuleAbsenceBehaviorReport.OptionalModuleAbsenceHandled);
        Assert.True(result.ModuleAbsenceBehaviorReport.RequiredModuleMissingRejected);
        Assert.Contains(result.ModuleAbsenceBehaviorReport.Evaluations, item => item.Status == "absent_optional");
        Assert.Contains(result.ModuleAbsenceBehaviorReport.Evaluations, item => item.Status == "missing_required" && !item.Accepted);
    }

    [Fact]
    public async Task InvalidMatrixCoversRequiredScenariosAndFinalReportHasNoTopLevelErrors()
    {
        var result = await new ModularGeneratorKernelReadinessService().BuildAsync(FindRepoRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(16, result.InvalidMatrix.ScenarioCount);
        Assert.Equal(16, result.InvalidMatrix.MatchedExpectationCount);
        Assert.Equal(1, result.InvalidMatrix.AcceptedWithDiagnosticCount);
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_goal028_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "duplicate_module_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "optional_dependency_missing" && scenario.ActualValid);
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "required_dependency_missing" && !scenario.ActualValid);
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "hardcoded_smoke_route_required_for_new_manifest_scenario" && !scenario.ActualValid);
        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGate()
    {
        using var temp = new TempDirectory();
        var service = new ModularGeneratorKernelReadinessService();
        var result = await service.BuildAsync(FindRepoRoot());
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<ModularGeneratorKernelReadinessReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(ModularGeneratorKernelReadinessService.FinalGate, report.ManualGate);
        Assert.Equal(ModularGeneratorKernelReadinessService.PreviousAcceptedGate, report.PreviousAcceptedGate);
        Assert.True(report.ContractProofPassed);
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateRecordsGoal028AcceptedBeforeGoal029()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal("goal_030_semantic_artifact_contract_registry", root.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Equal("semantic_artifact_contract_registry_verification", root.GetProperty("gate_status").GetString());
        Assert.Equal("passed_by_user_prompt_before_goal_029", root.GetProperty("package_assembly_combat_progression_expansion_verification").GetProperty("status").GetString());
        Assert.Equal("passed_by_user_handoff_before_goal_030", root.GetProperty(ModularGeneratorKernelReadinessService.FinalGate).GetProperty("status").GetString());
    }

    private static ModularGeneratorModuleManifest ValidModuleManifest() =>
        new()
        {
            SchemaVersion = "module_contract_manifest_v1",
            ModuleId = "package_assembly_world_entities",
            ModuleKind = "package_assembly",
            Version = "1.0.0",
            OwnedSourceRoots = ["src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/"],
            OwnedArtifactRoot = ".llmgc/procedural/package-assembly-world-entities/",
            InputContracts = ["scene_pack_v1"],
            OutputContracts = ["game.maps"],
            RequiredKernelCapabilities = ["static_manifest_registry"],
            AbsenceBehavior = "required_module",
            Validators = ["module_manifest_validator"],
            FocusedTestFilter = "FullyQualifiedName~PackageAssemblyWorldEntities",
            ProductSmokeScenario = "package-assembly-world-entities",
            DeterministicHashRules = ["sort_by_module_id"]
        };

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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
