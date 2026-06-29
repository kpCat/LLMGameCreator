using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftArtifactLoopEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-034-strict-llm-draft-artifact-loop";
    public const string ContractSummaryJsonFileName = "draft-loop-contract-summary.json";
    public const string RequestMatrixJsonFileName = "draft-request-matrix.json";
    public const string CandidateMatrixJsonFileName = "candidate-quarantine-matrix.json";
    public const string RepairMatrixJsonFileName = "repair-request-matrix.json";
    public const string PromotionMatrixJsonFileName = "promotion-decision-matrix.json";
    public const string FrontierPlanJsonFileName = "strict-draft-plan-frontier.json";
    public const string GothicPlanJsonFileName = "strict-draft-plan-gothic.json";
    public const string CaravanPlanJsonFileName = "strict-draft-plan-caravan.json";
    public const string MetamodulePlanJsonFileName = "strict-draft-plan-metamodule-kingdoms.json";
    public const string InvalidMatrixJsonFileName = "invalid-draft-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "strict-llm-draft-artifact-loop-report.md";
    public const string FinalGate = "strict_llm_draft_artifact_loop_verification";
    public const string ProductSmokeRoute = "goal-034-strict-llm-draft-artifact-loop";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public StrictLlmDraftArtifactLoopEvidenceResult Build()
    {
        var families = StrictLlmDraftArtifactLoopCatalog.BuildDraftFamilies();
        var familyDiagnostics = StrictLlmDraftArtifactLoopValidator.ValidateFamilies(families);
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var requestDiagnostics = StrictLlmDraftArtifactLoopValidator.ValidateRequests(requests, families);
        var candidates = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets);
        var candidateDiagnostics = StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, candidates);
        var fixableCandidate = candidates.First() with { CandidateId = "candidate/fixable/missing-required-field", PayloadFields = candidates.First().PayloadFields.Skip(1).ToList() };
        var nonFixableCandidate = candidates.Skip(1).First() with
        {
            CandidateId = "candidate/rejected/final-prose-leak",
            PayloadFields = candidates.Skip(1).First().PayloadFields.Concat([new StrictLlmDraftPayloadField { Name = "dialogue_line", ValueKind = "text", Value = "final dialogue line", FinalProse = true }]).ToList()
        };
        var decisionCandidates = candidates.Take(12).Concat([fixableCandidate, nonFixableCandidate]).OrderBy(item => item.CandidateId, StringComparer.Ordinal).ToList();
        var decisionDiagnostics = StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, decisionCandidates);
        var repairRequests = new StrictLlmDraftRepairPlanner().PlanRepairRequests(requests, [fixableCandidate], StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, [fixableCandidate]));
        var repairDiagnostics = StrictLlmDraftArtifactLoopValidator.ValidateRepairRequests([fixableCandidate], repairRequests);
        var decisions = new StrictLlmDraftPromotionDecisionEngine().Decide(requests, decisionCandidates);
        var invalidMatrix = StrictLlmDraftArtifactLoopValidator.BuildInvalidMatrix();

        var requestMatrix = new StrictLlmDraftRequestMatrix
        {
            RequestCount = requests.Count,
            ScenarioRequestSets = requestSets
        };
        var candidateMatrix = new StrictLlmDraftCandidateQuarantineMatrix
        {
            CandidateCount = candidates.Count,
            QuarantinedCount = candidates.Count(item => item.Status == "quarantined"),
            Candidates = candidates
        };
        var repairMatrix = new StrictLlmDraftRepairRequestMatrix
        {
            RepairRequestCount = repairRequests.Count,
            RepairRequests = repairRequests
        };
        var promotionMatrix = new StrictLlmDraftPromotionDecisionMatrix
        {
            DecisionCount = decisions.Count,
            PromotedCount = decisions.Count(item => item.Promoted),
            RepairRequiredCount = decisions.Count(item => item.Status == "repair_required"),
            RejectedCount = decisions.Count(item => item.Status == "rejected"),
            Decisions = decisions
        };
        var allDiagnostics = StrictLlmDraftArtifactLoopValidator.SortDiagnostics(
            familyDiagnostics
                .Concat(requestDiagnostics)
                .Concat(candidateDiagnostics)
                .Concat(repairDiagnostics)
                .Concat(decisionDiagnostics.Where(item => item.Severity == "warning")));
        var contractSummary = new StrictLlmDraftContractSummary
        {
            FamilyCount = families.Count,
            RequestCount = requests.Count,
            CandidateCount = candidates.Count,
            RepairRequestCount = repairRequests.Count,
            PromotionDecisionCount = decisions.Count,
            Families = families,
            Diagnostics = allDiagnostics
        };

        var artifactJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ContractSummaryJsonFileName] = Serialize(contractSummary),
            [RequestMatrixJsonFileName] = Serialize(requestMatrix),
            [CandidateMatrixJsonFileName] = Serialize(candidateMatrix),
            [RepairMatrixJsonFileName] = Serialize(repairMatrix),
            [PromotionMatrixJsonFileName] = Serialize(promotionMatrix),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        var scenarioJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FrontierPlanJsonFileName] = Serialize(requestSets.Single(item => item.ScenarioId == "frontier_survival")),
            [GothicPlanJsonFileName] = Serialize(requestSets.Single(item => item.ScenarioId == "gothic_intrigue")),
            [CaravanPlanJsonFileName] = Serialize(requestSets.Single(item => item.ScenarioId == "caravan_trade")),
            [MetamodulePlanJsonFileName] = Serialize(requestSets.Single(item => item.ScenarioId == "metamodule_kingdoms"))
        };

        var reportWithoutHash = new StrictLlmDraftArtifactLoopReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            ProductSmokeRoute = ProductSmokeRoute,
            FamilyCount = families.Count,
            RequestCount = requests.Count,
            CandidateCount = candidates.Count,
            RepairRequestCount = repairRequests.Count,
            PromotionDecisionCount = decisions.Count,
            MetamoduleSpeciesArchetypeRequestCount = requestSets.Single(item => item.ScenarioId == "metamodule_kingdoms").SpeciesArchetypeSlotRequestCount,
            InvalidMatrixPassed = invalidMatrix.Passed,
            ProviderLlmRagCalled = false,
            FinalProseGeneratedOrPromoted = false,
            GamePackageMaterialized = false,
            RuntimeUiUnityLuaGeneratorLibraryTouched = false,
            ContractSummaryHash = ComputeHash(artifactJson[ContractSummaryJsonFileName]),
            RequestMatrixHash = ComputeHash(artifactJson[RequestMatrixJsonFileName]),
            CandidateMatrixHash = ComputeHash(artifactJson[CandidateMatrixJsonFileName]),
            RepairMatrixHash = ComputeHash(artifactJson[RepairMatrixJsonFileName]),
            PromotionMatrixHash = ComputeHash(artifactJson[PromotionMatrixJsonFileName]),
            InvalidMatrixHash = ComputeHash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = allDiagnostics
        };
        var report = reportWithoutHash with
        {
            ContractProofPassed = allDiagnostics.All(item => item.Severity != "error")
                && invalidMatrix.Passed
                && repairRequests.Count > 0
                && promotionMatrix.PromotedCount > 0
                && promotionMatrix.RepairRequiredCount > 0
                && promotionMatrix.RejectedCount > 0
                && reportWithoutHash.MetamoduleSpeciesArchetypeRequestCount >= 100,
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new StrictLlmDraftArtifactLoopEvidenceResult
        {
            ContractSummary = contractSummary,
            RequestMatrix = requestMatrix,
            CandidateMatrix = candidateMatrix,
            RepairRequestMatrix = repairMatrix,
            PromotionDecisionMatrix = promotionMatrix,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ScenarioPlanJsonByFileName = scenarioJson,
            ReportMarkdown = RenderReport(report, requestSets, invalidMatrix, promotionMatrix)
        };
    }

    public async Task<StrictLlmDraftArtifactLoopEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StrictLlmDraftArtifactLoopEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        StrictLlmDraftArtifactLoopEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var files = result.ArtifactJsonByFileName
            .Concat(result.ScenarioPlanJsonByFileName)
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
        return new StrictLlmDraftArtifactLoopEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        StrictLlmDraftArtifactLoopReport report,
        IReadOnlyList<StrictLlmDraftRequestSet> requestSets,
        StrictLlmDraftInvalidMatrix invalidMatrix,
        StrictLlmDraftPromotionDecisionMatrix promotionMatrix)
    {
        var lines = new List<string>
        {
            "# Strict LLM Draft Artifact Loop Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- accepted=false",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- familyCount: {report.FamilyCount}",
            $"- requestCount: {report.RequestCount}",
            $"- candidateCount: {report.CandidateCount}",
            $"- repairRequestCount: {report.RepairRequestCount}",
            $"- promotionDecisionCount: {report.PromotionDecisionCount}",
            $"- promotedDecisions: {promotionMatrix.PromotedCount}",
            $"- repairRequiredDecisions: {promotionMatrix.RepairRequiredCount}",
            $"- rejectedDecisions: {promotionMatrix.RejectedCount}",
            $"- metamoduleSpeciesArchetypeRequestCount: {report.MetamoduleSpeciesArchetypeRequestCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- contractSummaryHash: {report.ContractSummaryHash}",
            $"- requestMatrixHash: {report.RequestMatrixHash}",
            $"- candidateMatrixHash: {report.CandidateMatrixHash}",
            $"- repairMatrixHash: {report.RepairMatrixHash}",
            $"- promotionMatrixHash: {report.PromotionMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Future LLM/manual/import output can only enter the generator as quarantined contract-bound draft candidates, and the program deterministically validates, repairs or rejects them before any promotion.",
            string.Empty,
            "## Scenarios",
            string.Empty
        };
        lines.AddRange(requestSets.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: requests={item.Requests.Count}, speciesArchetypeSlotRequests={item.SpeciesArchetypeSlotRequestCount}, summary={item.StableSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedValid={item.ExpectedValid.ToString().ToLowerInvariant()}, actualValid={item.ActualValid.ToString().ToLowerInvariant()}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add($"- providerLlmRagCalled: {report.ProviderLlmRagCalled.ToString().ToLowerInvariant()}");
        lines.Add($"- finalProseGeneratedOrPromoted: {report.FinalProseGeneratedOrPromoted.ToString().ToLowerInvariant()}");
        lines.Add($"- gamePackageMaterialized: {report.GamePackageMaterialized.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeUiUnityLuaGeneratorLibraryTouched: {report.RuntimeUiUnityLuaGeneratorLibraryTouched.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("No provider/LLM/RAG call happened. No final prose was generated or promoted. No GamePackage materialization happened.");
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
