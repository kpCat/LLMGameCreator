using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.HybridDraftLuaExpansion;

public sealed class HybridDraftLuaExpansionEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion";
    public const string AdapterSelectionJsonFileName = "executor-adapter-selection.json";
    public const string PipelineSummaryJsonFileName = "hybrid-pipeline-summary.json";
    public const string DraftToLuaRequestMapJsonFileName = "draft-to-lua-request-map.json";
    public const string SandboxApprovedMatrixJsonFileName = "sandbox-approved-expansion-matrix.json";
    public const string FrontierOutputJsonFileName = "lua-expansion-output-frontier.json";
    public const string GothicOutputJsonFileName = "lua-expansion-output-gothic.json";
    public const string CaravanOutputJsonFileName = "lua-expansion-output-caravan.json";
    public const string MetamoduleOutputJsonFileName = "lua-expansion-output-metamodule-kingdoms.json";
    public const string PromotionDecisionMatrixJsonFileName = "promotion-decision-matrix.json";
    public const string InvalidMatrixJsonFileName = "invalid-hybrid-expansion-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "hybrid-llm-draft-lua-deterministic-expansion-report.md";
    public const string FinalGate = HybridDraftLuaExpansionVocabulary.FinalGate;
    public const string ProductSmokeRoute = HybridDraftLuaExpansionVocabulary.ProductSmokeRoute;

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<HybridDraftLuaExpansionEvidenceResult> BuildAsync(CancellationToken cancellationToken = default)
    {
        var adapterSelection = HybridDraftLuaExpansionCatalog.BuildAdapterSelection();
        var requests = HybridDraftLuaExpansionCatalog.BuildDefaultRequests();
        var fixtures = HybridDraftLuaExpansionCatalog.BuildFixtures(requests);
        var adapter = new HybridDraftLuaExecutorAdapter();
        var results = new List<HybridExecutorAdapterResult>();

        foreach (var request in requests.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            results.Add(await adapter.ExecuteAsync(request, fixtures[request.FixtureId], cancellationToken).ConfigureAwait(false));
        }

        var outputsByRequestId = results
            .Where(item => item.Output != null)
            .ToDictionary(item => item.ExecutionRequestId, item => item.Output!, StringComparer.Ordinal);
        var decisions = requests
            .Select(request => HybridDraftLuaExpansionCatalog.DecidePromotion(request, outputsByRequestId.GetValueOrDefault(request.ExecutionRequestId)))
            .OrderBy(item => item.DecisionId, StringComparer.Ordinal)
            .ToList();
        var promotionMatrix = new HybridPromotionDecisionMatrix
        {
            DecisionCount = decisions.Count,
            AcceptedCount = decisions.Count(item => item.PromotionStatus == "accepted"),
            RejectedCount = decisions.Count(item => item.PromotionStatus == "rejected"),
            RepairRequiredCount = decisions.Count(item => item.PromotionStatus == "repair_required"),
            BlockedCount = decisions.Count(item => item.PromotionStatus == "blocked"),
            Decisions = decisions
        };
        var invalidMatrix = HybridDraftLuaExpansionCatalog.BuildInvalidMatrix(requests, outputsByRequestId, adapterSelection);
        var draftMap = HybridDraftLuaExpansionCatalog.BuildDraftToLuaRequestMap(requests);
        var sandboxMatrix = HybridDraftLuaExpansionCatalog.BuildSandboxApprovedExpansionMatrix(requests);
        var scenarioOutputs = BuildScenarioOutputs(outputsByRequestId.Values);
        var coveredFamilies = outputsByRequestId.Values
            .Select(item => item.ProducedArtifactFamily)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var validPathDiagnostics = HybridDraftLuaExpansionCatalog.SortDiagnostics(
            adapterSelection.Diagnostics
                .Concat(results.SelectMany(item => item.Diagnostics)));

        var pipelineSummary = new HybridPipelineSummary
        {
            ScenarioCount = scenarioOutputs.Count,
            ExpansionRequestCount = requests.Count,
            ExecutedRequestCount = results.Count(item => item.LuaExecuted),
            OutputCount = outputsByRequestId.Count,
            MetamoduleSlotCount = outputsByRequestId.Values
                .Where(item => item.ScenarioId == "metamodule_kingdoms" && item.ProducedArtifactFamily == "metamodule_species_archetype_slot_expansion")
                .SelectMany(item => item.Slots)
                .Count(),
            RealBoundedExecutorPathProven = results.Any(item => item.LuaExecuted && item.Status == "accepted"),
            Steps = HybridDraftLuaExpansionCatalog.BuildPipelineSteps(),
            RequiredFamiliesCovered = coveredFamilies,
            Diagnostics = validPathDiagnostics
        };

        var artifactJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AdapterSelectionJsonFileName] = Serialize(adapterSelection),
            [PipelineSummaryJsonFileName] = Serialize(pipelineSummary),
            [DraftToLuaRequestMapJsonFileName] = Serialize(draftMap),
            [SandboxApprovedMatrixJsonFileName] = Serialize(sandboxMatrix),
            [PromotionDecisionMatrixJsonFileName] = Serialize(promotionMatrix),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        var scenarioJson = scenarioOutputs.ToDictionary(item => item.Key, item => Serialize(item.Value), StringComparer.Ordinal);
        var reportWithoutHash = new HybridDraftLuaExpansionReport
        {
            Accepted = false,
            ImplementationStatus = "GREEN",
            ProductSmokeRoute = ProductSmokeRoute,
            RealBoundedExecutorPathProven = pipelineSummary.RealBoundedExecutorPathProven,
            AdapterId = adapterSelection.AdapterId,
            PackageId = adapterSelection.PackageId,
            PackageVersion = adapterSelection.PackageVersion,
            ScenarioCount = pipelineSummary.ScenarioCount,
            ExpansionRequestCount = pipelineSummary.ExpansionRequestCount,
            ExecutedRequestCount = pipelineSummary.ExecutedRequestCount,
            OutputCount = pipelineSummary.OutputCount,
            MetamoduleSpeciesArchetypeSlotCount = pipelineSummary.MetamoduleSlotCount,
            InvalidScenarioCount = invalidMatrix.ScenarioCount,
            InvalidMatrixPassed = invalidMatrix.Passed,
            AdapterSelectionHash = Hash(artifactJson[AdapterSelectionJsonFileName]),
            PipelineSummaryHash = Hash(artifactJson[PipelineSummaryJsonFileName]),
            DraftRequestMapHash = Hash(artifactJson[DraftToLuaRequestMapJsonFileName]),
            SandboxMatrixHash = Hash(artifactJson[SandboxApprovedMatrixJsonFileName]),
            PromotionMatrixHash = Hash(artifactJson[PromotionDecisionMatrixJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            ScenarioOutputHashes = scenarioJson.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => Hash(item.Value)).ToList(),
            Diagnostics = validPathDiagnostics
        };
        var report = reportWithoutHash with
        {
            ContractProofPassed = adapterSelection.Status == "selected_real_bounded_executor"
                && adapterSelection.SafeApiIsolationProven
                && pipelineSummary.RealBoundedExecutorPathProven
                && pipelineSummary.ScenarioCount == 4
                && pipelineSummary.OutputCount == requests.Count
                && pipelineSummary.MetamoduleSlotCount >= 100
                && HybridDraftLuaExpansionVocabulary.ArtifactFamilies.All(family => pipelineSummary.RequiredFamiliesCovered.Contains(family, StringComparer.Ordinal))
                && promotionMatrix.AcceptedCount == requests.Count
                && invalidMatrix.Passed
                && sandboxMatrix.ApprovedCount == sandboxMatrix.RowCount
                && validPathDiagnostics.All(item => item.Severity != "error"),
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new HybridDraftLuaExpansionEvidenceResult
        {
            AdapterSelection = adapterSelection,
            PipelineSummary = pipelineSummary,
            DraftToLuaRequestMap = draftMap,
            SandboxApprovedExpansionMatrix = sandboxMatrix,
            ScenarioOutputsByFileName = scenarioOutputs,
            PromotionDecisionMatrix = promotionMatrix,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ScenarioOutputJsonByFileName = scenarioJson,
            ReportMarkdown = RenderReport(report, pipelineSummary, sandboxMatrix, promotionMatrix, invalidMatrix, adapterSelection)
        };
    }

    public async Task<HybridDraftLuaExpansionEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HybridDraftLuaExpansionEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        HybridDraftLuaExpansionEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var files = result.ArtifactJsonByFileName
            .Concat(result.ScenarioOutputJsonByFileName)
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

        return new HybridDraftLuaExpansionEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static IReadOnlyDictionary<string, HybridScenarioExpansionOutput> BuildScenarioOutputs(IEnumerable<HybridExpansionOutput> outputs)
    {
        var byScenario = outputs
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .GroupBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToDictionary(
                item => ScenarioFileName(item.Key),
                item =>
                {
                    var outputList = item.OrderBy(output => output.StableId, StringComparer.Ordinal).ToList();
                    return new HybridScenarioExpansionOutput
                    {
                        ScenarioId = item.Key,
                        OutputCount = outputList.Count,
                        SlotCount = outputList.SelectMany(output => output.Slots).Count(),
                        Outputs = outputList
                    };
                },
                StringComparer.Ordinal);

        return byScenario;
    }

    private static string ScenarioFileName(string scenarioId) =>
        scenarioId switch
        {
            "frontier_survival" => FrontierOutputJsonFileName,
            "gothic_intrigue" => GothicOutputJsonFileName,
            "caravan_trade" => CaravanOutputJsonFileName,
            "metamodule_kingdoms" => MetamoduleOutputJsonFileName,
            _ => $"lua-expansion-output-{scenarioId}.json"
        };

    private static string RenderReport(
        HybridDraftLuaExpansionReport report,
        HybridPipelineSummary pipeline,
        HybridSandboxApprovedExpansionMatrix sandboxMatrix,
        HybridPromotionDecisionMatrix promotionMatrix,
        HybridInvalidMatrix invalidMatrix,
        HybridExecutorAdapterSelection adapterSelection)
    {
        var lines = new List<string>
        {
            "# Hybrid LLM Draft Plus Lua Deterministic Expansion Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- realBoundedExecutorPathProven: {report.RealBoundedExecutorPathProven.ToString().ToLowerInvariant()}",
            $"- adapter: {report.AdapterId}",
            $"- package: {report.PackageId} {report.PackageVersion} ({adapterSelection.LicenseExpression})",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- expansionRequestCount: {report.ExpansionRequestCount}",
            $"- executedRequestCount: {report.ExecutedRequestCount}",
            $"- outputCount: {report.OutputCount}",
            $"- metamoduleSpeciesArchetypeSlotCount: {report.MetamoduleSpeciesArchetypeSlotCount}",
            $"- invalidScenarioCount: {report.InvalidScenarioCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- adapterSelectionHash: {report.AdapterSelectionHash}",
            $"- pipelineSummaryHash: {report.PipelineSummaryHash}",
            $"- draftRequestMapHash: {report.DraftRequestMapHash}",
            $"- sandboxMatrixHash: {report.SandboxMatrixHash}",
            $"- promotionMatrixHash: {report.PromotionMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 034 strict draft ids, Goal 035 manifest selections and Goal 036 sandbox decisions now flow through a real bounded LuaCSharp executor adapter for repo-owned deterministic expansion fixtures, then through C# validation and promotion decisions.",
            string.Empty,
            "## Pipeline",
            string.Empty
        };

        lines.AddRange(pipeline.Steps.Select(item => $"- {item.Ordinal}. {item.StepId}: {item.SourceGoal}; {item.Responsibility}"));
        lines.Add(string.Empty);
        lines.Add("## Adapter decision");
        lines.Add(string.Empty);
        lines.Add($"- packageId: {adapterSelection.PackageId}");
        lines.Add($"- packageVersion: {adapterSelection.PackageVersion}");
        lines.Add($"- license: {adapterSelection.LicenseExpression}");
        lines.Add($"- status: {adapterSelection.Status}");
        lines.Add($"- standardLibrariesOpened: {adapterSelection.CapabilityFlags.StandardLibrariesOpened.ToString().ToLowerInvariant()}");
        lines.Add($"- arbitraryUserLuaAllowed: {adapterSelection.CapabilityFlags.ArbitraryUserLuaAllowed.ToString().ToLowerInvariant()}");
        lines.Add($"- instructionCountHookSupported: {adapterSelection.CapabilityFlags.InstructionCountHookSupported.ToString().ToLowerInvariant()}");
        lines.Add($"- declarativeFixtureRestrictionRequired: {adapterSelection.CapabilityFlags.DeclarativeFixtureRestrictionRequired.ToString().ToLowerInvariant()}");
        lines.AddRange(adapterSelection.RiskNotes.Select(item => $"- riskNote: {item}"));
        lines.Add(string.Empty);
        lines.Add("## Sandbox approvals");
        lines.Add(string.Empty);
        lines.AddRange(sandboxMatrix.Rows.Select(item => $"- {item.ScenarioId}: goal036={item.Goal036DecisionStatus}, approvedForGoal037={item.ApprovedForRepoOwnedFixtureExecution.ToString().ToLowerInvariant()}, reason={item.ApprovalReason}"));
        lines.Add(string.Empty);
        lines.Add("## Promotion decisions");
        lines.Add(string.Empty);
        lines.AddRange(promotionMatrix.Decisions.Select(item => $"- {item.ScenarioId}: status={item.PromotionStatus}, promoted={item.Promoted.ToString().ToLowerInvariant()}, output={item.StableOutputId}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- noLiveLlmProviderRagCall: {report.NoLiveLlmProviderRagCall.ToString().ToLowerInvariant()}");
        lines.Add($"- noFinalProse: {report.NoFinalProse.ToString().ToLowerInvariant()}");
        lines.Add($"- noRuntimeUiUnityGamePackageMutation: {report.NoRuntimeUiUnityGamePackageMutation.ToString().ToLowerInvariant()}");
        lines.Add($"- noFilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop: {report.NoFilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("No live LLM/provider/RAG call happened. No final prose was generated. No Runtime/UI/Unity/GamePackage/provider/LLM/RAG path was touched. No filesystem/network/process/reflection/thread/time/random/native interop surface was exposed.");
        lines.Add(string.Empty);
        lines.Add($"{FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Hash(string text) => HybridDraftLuaExpansionCatalog.ComputeHash(text);

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
