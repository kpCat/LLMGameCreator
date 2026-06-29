using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-035-lua-module-manifest-registry";
    public const string RegistrySummaryJsonFileName = "lua-module-registry-summary.json";
    public const string HostApiSurfacePolicyJsonFileName = "lua-host-api-surface-policy.json";
    public const string FrontierSelectionJsonFileName = "lua-module-selection-frontier.json";
    public const string GothicSelectionJsonFileName = "lua-module-selection-gothic.json";
    public const string CaravanSelectionJsonFileName = "lua-module-selection-caravan.json";
    public const string MetamoduleSelectionJsonFileName = "lua-module-selection-metamodule-kingdoms.json";
    public const string DependencyPlanJsonFileName = "lua-module-dependency-plan.json";
    public const string InvalidMatrixJsonFileName = "invalid-lua-manifest-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "lua-module-manifest-registry-report.md";
    public const string FinalGate = "lua_module_manifest_registry_verification";
    public const string ProductSmokeRoute = "goal-035-lua-module-manifest-registry";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public LuaModuleManifestEvidenceResult Build()
    {
        var families = LuaModuleManifestRegistryCatalog.BuildFamilies();
        var policy = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy();
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var familyDiagnostics = LuaModuleManifestRegistryValidator.ValidateFamilies(families);
        var policyDiagnostics = LuaModuleManifestRegistryValidator.ValidateHostApiSurface(policy.Groups);
        var manifestDiagnostics = LuaModuleManifestRegistryValidator.ValidateManifests(families, policy.Groups, manifests);
        var invalidMatrix = LuaModuleManifestRegistryValidator.BuildInvalidMatrix();
        var planner = new LuaModuleManifestPlanner();
        var plans = planner.PlanDefaultScenarios();
        var dependencyPlan = planner.BuildDependencyPlan(plans);
        var planDiagnostics = plans
            .SelectMany(item => item.DeniedApiDiagnostics.Concat(item.CompatibilityDiagnostics))
            .ToList();
        var allDiagnostics = LuaModuleManifestRegistryValidator.SortDiagnostics(
            familyDiagnostics.Concat(policyDiagnostics).Concat(manifestDiagnostics).Concat(planDiagnostics));

        var registrySummary = new LuaModuleRegistrySummary
        {
            FamilyCount = families.Count,
            ManifestCount = manifests.Count,
            ReadyManifestCount = manifests.Count(item => item.LifecycleStatus == "ready"),
            OptionalManifestCount = manifests.Count(item => item.LifecycleStatus == "optional"),
            FutureRequiredManifestCount = manifests.Count(item => item.LifecycleStatus == "future_required"),
            QuarantinedManifestCount = manifests.Count(item => item.LifecycleStatus == "quarantined"),
            MetamoduleSpeciesArchetypeSlotManifestCount = manifests.Count(item => item.ModuleId.StartsWith("lua-module/metamodule/species-archetype-slot/", StringComparison.Ordinal)),
            Families = families,
            Manifests = manifests,
            Diagnostics = allDiagnostics
        };

        var artifactJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [RegistrySummaryJsonFileName] = Serialize(registrySummary),
            [HostApiSurfacePolicyJsonFileName] = Serialize(policy),
            [DependencyPlanJsonFileName] = Serialize(dependencyPlan),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        var selectionJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FrontierSelectionJsonFileName] = Serialize(plans.Single(item => item.ScenarioId == "frontier_survival")),
            [GothicSelectionJsonFileName] = Serialize(plans.Single(item => item.ScenarioId == "gothic_intrigue")),
            [CaravanSelectionJsonFileName] = Serialize(plans.Single(item => item.ScenarioId == "caravan_trade")),
            [MetamoduleSelectionJsonFileName] = Serialize(plans.Single(item => item.ScenarioId == "metamodule_kingdoms"))
        };
        var reportWithoutHash = new LuaModuleManifestRegistryReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            ProductSmokeRoute = ProductSmokeRoute,
            FamilyCount = families.Count,
            HostApiGroupCount = policy.GroupCount,
            ManifestCount = manifests.Count,
            SelectedScenarioCount = plans.Count,
            MetamoduleSpeciesArchetypeSlotManifestCount = registrySummary.MetamoduleSpeciesArchetypeSlotManifestCount,
            InvalidMatrixPassed = invalidMatrix.Passed,
            RegistrySummaryHash = ComputeHash(artifactJson[RegistrySummaryJsonFileName]),
            HostApiPolicyHash = ComputeHash(artifactJson[HostApiSurfacePolicyJsonFileName]),
            DependencyPlanHash = ComputeHash(artifactJson[DependencyPlanJsonFileName]),
            InvalidMatrixHash = ComputeHash(artifactJson[InvalidMatrixJsonFileName]),
            SelectionPlanHashes = selectionJson.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => ComputeHash(item.Value)).ToList(),
            Diagnostics = allDiagnostics
        };

        var distinctSelectionShapes = plans
            .Select(item => string.Join("|", item.SelectedManifests.Select(manifest => manifest.FamilyId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .Count();

        var report = reportWithoutHash with
        {
            ContractProofPassed = allDiagnostics.All(item => item.Severity != "error")
                && invalidMatrix.Passed
                && plans.Count == 4
                && plans.All(item => item.Summary.SelectedCount > 0)
                && distinctSelectionShapes >= 4
                && registrySummary.MetamoduleSpeciesArchetypeSlotManifestCount >= 100
                && policy.DeniedGroupIds.Contains("implicit_lua_execution", StringComparer.Ordinal)
                && policy.DeniedGroupIds.Contains("provider_llm_rag", StringComparer.Ordinal),
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new LuaModuleManifestEvidenceResult
        {
            RegistrySummary = registrySummary,
            HostApiSurfacePolicy = policy,
            DependencyPlan = dependencyPlan,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            SelectionJsonByFileName = selectionJson,
            ReportMarkdown = RenderReport(report, plans, invalidMatrix, policy)
        };
    }

    public async Task<LuaModuleManifestEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LuaModuleManifestEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        LuaModuleManifestEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var files = result.ArtifactJsonByFileName
            .Concat(result.SelectionJsonByFileName)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToList();
        var written = new List<string>();
        foreach (var file in files)
        {
            var path = Path.Combine(outputDirectory, file.Key);
            await File.WriteAllTextAsync(path, file.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);
        return new LuaModuleManifestEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        LuaModuleManifestRegistryReport report,
        IReadOnlyList<LuaModuleSelectionPlan> plans,
        LuaModuleManifestInvalidMatrix invalidMatrix,
        LuaHostApiSurfacePolicy policy)
    {
        var lines = new List<string>
        {
            "# Lua Module Manifest Registry Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- familyCount: {report.FamilyCount}",
            $"- hostApiGroupCount: {report.HostApiGroupCount}",
            $"- manifestCount: {report.ManifestCount}",
            $"- selectedScenarioCount: {report.SelectedScenarioCount}",
            $"- metamoduleSpeciesArchetypeSlotManifestCount: {report.MetamoduleSpeciesArchetypeSlotManifestCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- registrySummaryHash: {report.RegistrySummaryHash}",
            $"- hostApiPolicyHash: {report.HostApiPolicyHash}",
            $"- dependencyPlanHash: {report.DependencyPlanHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Future Lua/manual/import/LLM module output can only become selectable through deterministic manifest records, host API surface policy, dependency planning, provenance checks and invalid/fake/leak diagnostics before any executor is allowed.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };

        lines.AddRange(plans.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: selected={item.Summary.SelectedCount}, blocked={item.Summary.BlockedCount}, futureRequired={item.Summary.FutureRequiredCount}, missingDependencies={item.Summary.MissingDependencyCount}, summary={item.Summary.StableSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Host API denied groups");
        lines.Add(string.Empty);
        lines.AddRange(policy.DeniedGroupIds.Select(item => $"- {item}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedValid={item.ExpectedValid.ToString().ToLowerInvariant()}, actualValid={item.ActualValid.ToString().ToLowerInvariant()}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- noLuaExecutionOrParsing: {report.NoLuaExecutionOrParsing.ToString().ToLowerInvariant()}");
        lines.Add($"- noLuaSourceGenerated: {report.NoLuaSourceGenerated.ToString().ToLowerInvariant()}");
        lines.Add($"- noProviderLlmRagCallHappened: {report.NoProviderLlmRagCallHappened.ToString().ToLowerInvariant()}");
        lines.Add($"- noRuntimeUiUnityGamePackageMutation: {report.NoRuntimeUiUnityGamePackageMutation.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("No Lua execution or parsing happened. No Lua source was generated. No provider/LLM/RAG call happened. No Runtime/UI/Unity/GamePackage schema mutation happened.");
        lines.Add(string.Empty);
        lines.Add($"{FinalGate} required");
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
