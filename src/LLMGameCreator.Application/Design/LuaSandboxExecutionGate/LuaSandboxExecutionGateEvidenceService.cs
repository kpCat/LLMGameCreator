using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public sealed class LuaSandboxExecutionGateEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-036-lua-sandbox-execution-gate";
    public const string PolicySummaryJsonFileName = "lua-sandbox-policy-summary.json";
    public const string HostBindingMatrixJsonFileName = "lua-host-binding-matrix.json";
    public const string ExecutionRequestsJsonFileName = "lua-sandbox-execution-requests.json";
    public const string FrontierDecisionJsonFileName = "lua-sandbox-decision-frontier.json";
    public const string GothicDecisionJsonFileName = "lua-sandbox-decision-gothic.json";
    public const string CaravanDecisionJsonFileName = "lua-sandbox-decision-caravan.json";
    public const string MetamoduleDecisionJsonFileName = "lua-sandbox-decision-metamodule.json";
    public const string DryRunTraceMatrixJsonFileName = "lua-sandbox-dry-run-trace-matrix.json";
    public const string RepairPlanMatrixJsonFileName = "lua-sandbox-repair-plan-matrix.json";
    public const string InvalidMatrixJsonFileName = "invalid-lua-sandbox-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "lua-sandbox-execution-gate-report.md";
    public const string FinalGate = "lua_sandbox_execution_gate_verification";
    public const string ProductSmokeRoute = "goal-036-lua-sandbox-execution-gate";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public LuaSandboxExecutionGateEvidenceResult Build()
    {
        var policySummary = LuaSandboxExecutionGateCatalog.BuildPolicySummary();
        var policy = policySummary.Policy;
        var hostBindingMatrix = LuaSandboxExecutionGateCatalog.BuildHostBindingMatrix();
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var requests = LuaSandboxExecutionGateCatalog.BuildDefaultRequests()
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
        var decisions = requests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, manifests, policy, hostBindingMatrix))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
        var requestMatrix = new LuaSandboxExecutionRequestMatrix
        {
            RequestCount = requests.Count,
            SelectedManifestCount = requests.SelectMany(item => item.SelectedManifestIds).Distinct(StringComparer.Ordinal).Count(),
            MetamoduleSpeciesArchetypeSlotManifestCount = requests
                .Single(item => item.ScenarioId == "metamodule_kingdoms")
                .SelectedManifestIds
                .Count(item => item.StartsWith("lua-module/metamodule/species-archetype-slot/", StringComparison.Ordinal)),
            Requests = requests
        };
        var dryRunTraceMatrix = new LuaSandboxDryRunTraceBuilder().BuildMatrix(requests, decisions);
        var invalidMatrix = LuaSandboxExecutionGateValidator.BuildInvalidMatrix();
        var invalidCases = LuaSandboxExecutionGateValidator.BuildInvalidRequestCases();
        var invalidRequests = invalidCases.Select(item => item.Request).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var invalidDecisions = invalidRequests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, manifests, policy, hostBindingMatrix))
            .OrderBy(item => item.RequestId, StringComparer.Ordinal)
            .ToList();
        var repairPlanMatrix = new LuaSandboxRepairPlanner().BuildRepairPlanMatrix(
            requests.Concat(invalidRequests).ToList(),
            decisions.Concat(invalidDecisions).ToList());

        var decisionsByFileName = new Dictionary<string, LuaSandboxExecutionDecision>(StringComparer.Ordinal)
        {
            [FrontierDecisionJsonFileName] = decisions.Single(item => item.ScenarioId == "frontier_survival"),
            [GothicDecisionJsonFileName] = decisions.Single(item => item.ScenarioId == "gothic_intrigue"),
            [CaravanDecisionJsonFileName] = decisions.Single(item => item.ScenarioId == "caravan_trade"),
            [MetamoduleDecisionJsonFileName] = decisions.Single(item => item.ScenarioId == "metamodule_kingdoms")
        };
        var artifactJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PolicySummaryJsonFileName] = Serialize(policySummary),
            [HostBindingMatrixJsonFileName] = Serialize(hostBindingMatrix),
            [ExecutionRequestsJsonFileName] = Serialize(requestMatrix),
            [DryRunTraceMatrixJsonFileName] = Serialize(dryRunTraceMatrix),
            [RepairPlanMatrixJsonFileName] = Serialize(repairPlanMatrix),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        var decisionJson = decisionsByFileName.ToDictionary(item => item.Key, item => Serialize(item.Value), StringComparer.Ordinal);
        var allDiagnostics = LuaSandboxExecutionGateValidator.SortDiagnostics(
            policySummary.Diagnostics
                .Concat(decisions.SelectMany(item => item.Diagnostics))
                .Concat(invalidMatrix.Scenarios.SelectMany(item => item.Diagnostics)));
        var reportWithoutHash = new LuaSandboxExecutionGateReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            ProductSmokeRoute = ProductSmokeRoute,
            RequestCount = requestMatrix.RequestCount,
            DecisionCount = decisions.Count,
            TraceCount = dryRunTraceMatrix.TraceCount,
            RepairPlanCount = repairPlanMatrix.RepairPlanCount,
            InvalidScenarioCount = invalidMatrix.ScenarioCount,
            MetamoduleSpeciesArchetypeSlotManifestCount = requestMatrix.MetamoduleSpeciesArchetypeSlotManifestCount,
            LuaExecuted = false,
            LuaParserUsed = false,
            LuaSourceGenerated = false,
            ExternalDependencyAdded = false,
            RuntimeUiUnityGamePackageProviderLlmRagTouched = false,
            PolicySummaryHash = ComputeHash(artifactJson[PolicySummaryJsonFileName]),
            HostBindingMatrixHash = ComputeHash(artifactJson[HostBindingMatrixJsonFileName]),
            RequestMatrixHash = ComputeHash(artifactJson[ExecutionRequestsJsonFileName]),
            DryRunTraceMatrixHash = ComputeHash(artifactJson[DryRunTraceMatrixJsonFileName]),
            RepairPlanMatrixHash = ComputeHash(artifactJson[RepairPlanMatrixJsonFileName]),
            InvalidMatrixHash = ComputeHash(artifactJson[InvalidMatrixJsonFileName]),
            DecisionHashes = decisionJson.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => ComputeHash(item.Value)).ToList(),
            Diagnostics = allDiagnostics
        };

        var requiredDeniedPresent = policy.DeniedBoundaryGroups.All(item => hostBindingMatrix.DeniedGroupIds.Contains(item, StringComparer.Ordinal) || hostBindingMatrix.BoundaryBlockedGroupIds.Contains(item, StringComparer.Ordinal));
        var defaultStatusesValid = decisions.All(item => item.DecisionStatus is "dry_run_only" or "ready_for_future_executor" or "blocked_no_executor");
        var report = reportWithoutHash with
        {
            ContractProofPassed = policySummary.Diagnostics.All(item => item.Severity != "error")
                && requiredDeniedPresent
                && requestMatrix.RequestCount == 4
                && requestMatrix.MetamoduleSpeciesArchetypeSlotManifestCount >= 100
                && defaultStatusesValid
                && decisions.Any(item => item.DecisionStatus == "dry_run_only")
                && decisions.Any(item => item.DecisionStatus == "ready_for_future_executor")
                && decisions.Any(item => item.DecisionStatus == "blocked_no_executor")
                && dryRunTraceMatrix.LuaExecuted == false
                && repairPlanMatrix.MutatesAcceptedManifests == false
                && invalidMatrix.Passed
                && invalidMatrix.NeedsRepairCount > 0
                && invalidMatrix.RejectedCount > 0,
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new LuaSandboxExecutionGateEvidenceResult
        {
            PolicySummary = policySummary,
            HostBindingMatrix = hostBindingMatrix,
            RequestMatrix = requestMatrix,
            DecisionsByFileName = decisionsByFileName,
            DryRunTraceMatrix = dryRunTraceMatrix,
            RepairPlanMatrix = repairPlanMatrix,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            DecisionJsonByFileName = decisionJson,
            ReportMarkdown = RenderReport(report, decisions, invalidMatrix, repairPlanMatrix)
        };
    }

    public async Task<LuaSandboxExecutionGateEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LuaSandboxExecutionGateEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        LuaSandboxExecutionGateEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var files = result.ArtifactJsonByFileName
            .Concat(result.DecisionJsonByFileName)
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
        return new LuaSandboxExecutionGateEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        LuaSandboxExecutionGateReport report,
        IReadOnlyList<LuaSandboxExecutionDecision> decisions,
        LuaSandboxInvalidMatrix invalidMatrix,
        LuaSandboxRepairPlanMatrix repairPlanMatrix)
    {
        var lines = new List<string>
        {
            "# Lua Sandbox Execution Gate Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            "- luaExecuted=false",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- requestCount: {report.RequestCount}",
            $"- decisionCount: {report.DecisionCount}",
            $"- traceCount: {report.TraceCount}",
            $"- repairPlanCount: {report.RepairPlanCount}",
            $"- invalidScenarioCount: {report.InvalidScenarioCount}",
            $"- metamoduleSpeciesArchetypeSlotManifestCount: {report.MetamoduleSpeciesArchetypeSlotManifestCount}",
            $"- policySummaryHash: {report.PolicySummaryHash}",
            $"- hostBindingMatrixHash: {report.HostBindingMatrixHash}",
            $"- requestMatrixHash: {report.RequestMatrixHash}",
            $"- dryRunTraceMatrixHash: {report.DryRunTraceMatrixHash}",
            $"- repairPlanMatrixHash: {report.RepairPlanMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 035 manifest selections now pass through an Application-layer deny-first sandbox execution gate with budget, determinism, host binding, dry-run trace and repair evidence before any future executor adapter can be considered.",
            string.Empty,
            "## Scenario decisions",
            string.Empty
        };

        lines.AddRange(decisions.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: status={item.DecisionStatus}, selected={item.SelectedManifestCount}, metamoduleSlots={item.MetamoduleSpeciesArchetypeSlotManifestCount}, luaExecuted={item.LuaExecuted.ToString().ToLowerInvariant()}, summary={item.StableSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Repair plans");
        lines.Add(string.Empty);
        lines.AddRange(repairPlanMatrix.RepairPlans.Select(item => $"- {item.RepairPlanId}: status={item.Status}, actions={string.Join(",", item.Actions.Select(action => action.ActionKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}, mutatesAcceptedManifests={item.MutatesAcceptedManifests.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- luaExecuted: {report.LuaExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- luaParserUsed: {report.LuaParserUsed.ToString().ToLowerInvariant()}");
        lines.Add($"- luaSourceGenerated: {report.LuaSourceGenerated.ToString().ToLowerInvariant()}");
        lines.Add($"- externalDependencyAdded: {report.ExternalDependencyAdded.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeUiUnityGamePackageProviderLlmRagTouched: {report.RuntimeUiUnityGamePackageProviderLlmRagTouched.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("No real Lua execution happened. No Lua parser was used. No Lua source was generated. No external dependency was added. No Runtime/UI/Unity/GamePackage/provider/LLM/RAG path was touched.");
        lines.Add(string.Empty);
        lines.Add($"{FinalGate} required");
        lines.Add("luaExecuted=false");
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
