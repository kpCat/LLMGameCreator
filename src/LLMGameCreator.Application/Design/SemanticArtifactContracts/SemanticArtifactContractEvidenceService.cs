using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.SemanticArtifactContracts;

public sealed class SemanticArtifactContractEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-030-semantic-artifact-contract-registry";
    public const string RegistrySummaryJsonFileName = "registry-summary.json";
    public const string CompatibilityMatrixJsonFileName = "compatibility-matrix.json";
    public const string FrontierPlanJsonFileName = "semantic-expansion-plan-frontier.json";
    public const string GothicPlanJsonFileName = "semantic-expansion-plan-gothic.json";
    public const string CaravanPlanJsonFileName = "semantic-expansion-plan-caravan.json";
    public const string ReportMarkdownFileName = "semantic-artifact-contract-registry-report.md";
    public const string FinalGate = "semantic_artifact_contract_registry_verification";
    public const string PreviousAcceptedGate = "modular_generator_kernel_parallel_readiness_verification passed";
    public const string ProductSmokeRoute = "goal-030-semantic-artifact-contract-registry";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public SemanticArtifactEvidenceResult Build()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var packs = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks();
        var diagnostics = SemanticArtifactContractValidator.ValidateContracts(contracts);
        var registrySummary = BuildRegistrySummary(contracts, diagnostics);
        var planner = new SemanticArtifactCompatibilityPlanner(contracts);
        var frontier = planner.BuildPlan(Request("frontier_survival", packs, "semantic_pack/frontier_survival"));
        var gothic = planner.BuildPlan(Request("gothic_intrigue", packs, "semantic_pack/gothic_intrigue"));
        var caravan = planner.BuildPlan(Request("caravan_trade", packs, "semantic_pack/caravan_trade"));
        var plans = new[] { frontier, gothic, caravan };
        var matrix = BuildMatrix(plans);
        var invalidMatrix = BuildInvalidMatrix();

        var registryJson = Serialize(registrySummary);
        var matrixJson = Serialize(matrix);
        var frontierJson = Serialize(frontier);
        var gothicJson = Serialize(gothic);
        var caravanJson = Serialize(caravan);
        var planHashes = new[] { frontierJson, gothicJson, caravanJson }.Select(ComputeHash).Order(StringComparer.Ordinal).ToList();

        var reportWithoutHash = new SemanticArtifactContractRegistryReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = PreviousAcceptedGate,
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = diagnostics.All(item => item.Severity != "error")
                && plans.All(plan => plan.Diagnostics.All(item => item.Severity != "error"))
                && matrix.PlannerSharedByAllScenarios
                && matrix.ScenariosAreMeaningfullyDifferent
                && invalidMatrix.Passed,
            ContractCount = contracts.Count,
            ScenarioCount = plans.Length,
            RegistryValidated = diagnostics.All(item => item.Severity != "error"),
            CompatibilityPlannerShared = matrix.PlannerSharedByAllScenarios,
            SemanticExpansionSlotsWritten = plans.All(plan => plan.SemanticExpansionSlots.Count >= 8),
            InvalidMatrixPassed = invalidMatrix.Passed,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            RuntimeBehaviorChanged = false,
            RegistrySummaryHash = ComputeHash(registryJson),
            CompatibilityMatrixHash = ComputeHash(matrixJson),
            PlanHashes = planHashes,
            InvalidMatrix = invalidMatrix,
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new SemanticArtifactEvidenceResult
        {
            RegistrySummary = registrySummary,
            CompatibilityMatrix = matrix,
            FrontierPlan = frontier,
            GothicPlan = gothic,
            CaravanPlan = caravan,
            Report = report,
            RegistrySummaryJson = registryJson,
            CompatibilityMatrixJson = matrixJson,
            FrontierPlanJson = frontierJson,
            GothicPlanJson = gothicJson,
            CaravanPlanJson = caravanJson,
            ReportMarkdown = RenderReport(report, matrix)
        };
    }

    public async Task<SemanticArtifactEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        SemanticArtifactEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new SemanticArtifactEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            RegistrySummaryJsonPath = Path.Combine(outputDirectory, RegistrySummaryJsonFileName),
            CompatibilityMatrixJsonPath = Path.Combine(outputDirectory, CompatibilityMatrixJsonFileName),
            FrontierPlanJsonPath = Path.Combine(outputDirectory, FrontierPlanJsonFileName),
            GothicPlanJsonPath = Path.Combine(outputDirectory, GothicPlanJsonFileName),
            CaravanPlanJsonPath = Path.Combine(outputDirectory, CaravanPlanJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.RegistrySummaryJsonPath, result.RegistrySummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CompatibilityMatrixJsonPath, result.CompatibilityMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FrontierPlanJsonPath, result.FrontierPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.GothicPlanJsonPath, result.GothicPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CaravanPlanJsonPath, result.CaravanPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public async Task<SemanticArtifactEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static SemanticArtifactInvalidMatrix BuildInvalidMatrix()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var scenarios = new List<SemanticArtifactInvalidScenario>
        {
            Invalid("duplicate_contract_id", "duplicate id mutation", SemanticArtifactContractValidator.ValidateContracts([.. contracts, contracts[0]])),
            Invalid("unknown_dependency", "unknown dependency mutation", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "quest_graph_objective_reward_pattern_v1", item => item with { Dependencies = [.. item.Dependencies, "fake_dependency_v1"] }))),
            Invalid("dependency_cycle", "dependency cycle mutation", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "game_profile_v1", item => item with { Dependencies = ["quest_graph_objective_reward_pattern_v1"] }))),
            Invalid("missing_semantic_scope", "missing semantic scope mutation", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "semantic_pack_v1", item => item with { RequiredSemanticScopes = [] }))),
            Invalid("incompatible_tag_declaration", "incompatible tag mutation", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "semantic_pack_v1", item => item with { CompatibilityTags = ["bcl_only", "provider_required"] }))),
            Invalid("future_required_marked_ready", "future required contract incorrectly treated as ready", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "settlement_building_landmark_v1", item => item with { LifecycleStatus = "ready", CapabilityTags = [.. item.CapabilityTags, "future_required"] }))),
            Invalid("leakage_attempt", "runtime/provider/LLM/Lua/UI/GamePackage-schema leakage attempt", SemanticArtifactContractValidator.ValidateContracts(Mutate(contracts, "semantic_pack_v1", item => item with { Notes = "Goal 030 should call LLM and mutate GamePackage schema." }))),
            PlannerInvalid("module_absent_mutation", "module absent mutation", new SemanticArtifactCompatibilityPlanner(contracts).BuildPlan(new SemanticCompatibilityRequest
            {
                ProfileId = "frontier_survival",
                SelectedSemanticPacks = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks().Where(pack => pack.PackId is "semantic_pack/core_generator_spine" or "semantic_pack/frontier_survival").ToList(),
                AvailableModuleIds = new HashSet<string>(SemanticArtifactContractRegistry.DefaultAvailableModuleIds.Where(id => id != "package_assembly_dialogue_quests"), StringComparer.Ordinal)
            })),
            PlannerInvalid("fake_contract_id", "fake contract id accepted by planner", new SemanticArtifactCompatibilityPlanner(contracts).BuildPlan(new SemanticCompatibilityRequest
            {
                ProfileId = "frontier_survival",
                RequestedContractIds = ["fake_contract_v1"],
                SelectedSemanticPacks = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks().Where(pack => pack.PackId is "semantic_pack/core_generator_spine" or "semantic_pack/frontier_survival").ToList()
            }))
        };

        return new SemanticArtifactInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static SemanticCompatibilityRequest Request(string profileId, IReadOnlyList<SemanticPackDescriptor> packs, string profilePackId) =>
        new()
        {
            ProfileId = profileId,
            SelectedSemanticPacks = packs
                .Where(pack => pack.PackId is "semantic_pack/core_generator_spine" || pack.PackId == profilePackId)
                .ToList()
        };

    private static SemanticArtifactRegistrySummary BuildRegistrySummary(
        IReadOnlyList<SemanticArtifactContractDescriptor> contracts,
        IReadOnlyList<SemanticArtifactDiagnostic> diagnostics) =>
        new()
        {
            ContractCount = contracts.Count,
            ContractIds = contracts.Select(contract => contract.ContractId).Order(StringComparer.Ordinal).ToList(),
            ReadyContractIds = contracts.Where(contract => contract.LifecycleStatus == "ready").Select(contract => contract.ContractId).Order(StringComparer.Ordinal).ToList(),
            OptionalContractIds = contracts.Where(contract => contract.LifecycleStatus == "optional").Select(contract => contract.ContractId).Order(StringComparer.Ordinal).ToList(),
            FutureRequiredContractIds = contracts.Where(contract => contract.LifecycleStatus == "future_required").Select(contract => contract.ContractId).Order(StringComparer.Ordinal).ToList(),
            BlockedContractIds = contracts.Where(contract => contract.LifecycleStatus == "blocked").Select(contract => contract.ContractId).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };

    private static SemanticCompatibilityMatrix BuildMatrix(IReadOnlyList<SemanticCompatibilityPlan> plans)
    {
        var rows = plans
            .OrderBy(plan => plan.ProfileId, StringComparer.Ordinal)
            .Select(plan => new SemanticCompatibilityMatrixRow
            {
                ProfileId = plan.ProfileId,
                PackIds = plan.SelectedSemanticPackIds,
                SelectedContractCount = plan.SelectedContractIds.Count,
                ExpansionSlotCount = plan.SemanticExpansionSlots.Count,
                SlotFamilies = plan.SemanticExpansionSlots.Select(slot => slot.SlotFamily).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                BlockedOrFutureRequiredContractIds = plan.BlockedOrFutureRequiredItems.Select(item => item.ContractId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                StableSummary = plan.StableSummary
            })
            .ToList();
        return new SemanticCompatibilityMatrix
        {
            Rows = rows,
            PlannerSharedByAllScenarios = rows.Count == 3 && rows.All(row => row.SelectedContractCount > 0),
            ScenariosAreMeaningfullyDifferent = rows.Select(row => string.Join("|", row.PackIds.Concat(row.SlotFamilies))).Distinct(StringComparer.Ordinal).Count() == rows.Count
        };
    }

    private static SemanticArtifactInvalidScenario Invalid(string id, string kind, IReadOnlyList<SemanticArtifactDiagnostic> diagnostics)
    {
        var sorted = SemanticArtifactContractValidator.SortDiagnostics(diagnostics);
        return new SemanticArtifactInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = sorted
        };
    }

    private static SemanticArtifactInvalidScenario PlannerInvalid(string id, string kind, SemanticCompatibilityPlan plan)
    {
        var sorted = SemanticArtifactContractValidator.SortDiagnostics(plan.Diagnostics);
        return new SemanticArtifactInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = kind,
            Diagnostics = sorted
        };
    }

    private static IReadOnlyList<SemanticArtifactContractDescriptor> Mutate(
        IReadOnlyList<SemanticArtifactContractDescriptor> contracts,
        string contractId,
        Func<SemanticArtifactContractDescriptor, SemanticArtifactContractDescriptor> mutate) =>
        contracts
            .Select(contract => contract.ContractId == contractId ? mutate(contract) : contract)
            .ToList();

    private static string RenderReport(SemanticArtifactContractRegistryReport report, SemanticCompatibilityMatrix matrix)
    {
        var lines = new List<string>
        {
            "# Semantic Artifact Contract Registry Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- previousAcceptedGate: {report.PreviousAcceptedGate}",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- contractCount: {report.ContractCount}",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- registrySummaryHash: {report.RegistrySummaryHash}",
            $"- compatibilityMatrixHash: {report.CompatibilityMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Future generator modules can now ask one deterministic registry which artifact contracts and semantic expansion slots are valid for a selected profile/semantic-pack set, instead of hardcoding isolated vertical paths.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };
        lines.AddRange(matrix.Rows.Select(row => $"- {row.ProfileId}: packs={string.Join(",", row.PackIds)}, contracts={row.SelectedContractCount}, slots={row.ExpansionSlotCount}, blocked={string.Join(",", row.BlockedOrFutureRequiredContractIds)}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(report.InvalidMatrix.Scenarios.Select(scenario => $"- {scenario.ScenarioId}: rejected={(!scenario.ActualValid).ToString().ToLowerInvariant()}, codes={string.Join(",", scenario.Diagnostics.Select(item => item.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeBehaviorChanged: {report.RuntimeBehaviorChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- unityBuildExecuted: {report.UnityBuildExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- llmRagProviderMediaLuaExecuted: {report.LlmRagProviderMediaLuaExecuted.ToString().ToLowerInvariant()}");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ComputeHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}

public sealed record SemanticArtifactEvidenceResult
{
    public SemanticArtifactRegistrySummary RegistrySummary { get; init; } = new();
    public SemanticCompatibilityMatrix CompatibilityMatrix { get; init; } = new();
    public SemanticCompatibilityPlan FrontierPlan { get; init; } = new();
    public SemanticCompatibilityPlan GothicPlan { get; init; } = new();
    public SemanticCompatibilityPlan CaravanPlan { get; init; } = new();
    public SemanticArtifactContractRegistryReport Report { get; init; } = new();
    public string RegistrySummaryJson { get; init; } = string.Empty;
    public string CompatibilityMatrixJson { get; init; } = string.Empty;
    public string FrontierPlanJson { get; init; } = string.Empty;
    public string GothicPlanJson { get; init; } = string.Empty;
    public string CaravanPlanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record SemanticArtifactEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string RegistrySummaryJsonPath { get; init; } = string.Empty;
    public string CompatibilityMatrixJsonPath { get; init; } = string.Empty;
    public string FrontierPlanJsonPath { get; init; } = string.Empty;
    public string GothicPlanJsonPath { get; init; } = string.Empty;
    public string CaravanPlanJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
