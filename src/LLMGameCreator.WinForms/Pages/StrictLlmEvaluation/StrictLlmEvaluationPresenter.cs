using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages.StrictLlmEvaluation;

public sealed class StrictLlmEvaluationPresenter
{
    private static readonly string[] DefaultContractIds =
    [
        "game_profile_v1",
        "scene_pack_v1",
        "quest_pack_v1",
        "mechanics_pack_v1"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public StrictLlmEvaluationViewState FromSettings(
        StrictLlmEvaluationViewState state,
        AppSettings settings,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactContractDefinition> contracts)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(contracts);

        var profiles = settings.LlmProfiles
            .Where(profile => !string.IsNullOrWhiteSpace(profile.Id))
            .Select(profile => new StrictLlmEvaluationProfileOption
            {
                Id = profile.Id,
                Title = profile.Title,
                Model = profile.Model
            })
            .OrderBy(profile => profile.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var contractOptions = contracts
            .Select(contract => new StrictLlmEvaluationContractOption
            {
                Id = contract.ContractId,
                Title = contract.ArtifactKind
            })
            .ToList();

        var selectedProfile = FirstExisting(state.SelectedProfileId, profiles.Select(profile => profile.Id))
            ?? FirstExisting(settings.DefaultLlmProfileId, profiles.Select(profile => profile.Id))
            ?? profiles.FirstOrDefault()?.Id
            ?? string.Empty;
        var selectedContracts = state.SelectedContractIds.Count > 0
            ? state.SelectedContractIds
            : DefaultContractIds.Where(id => contractOptions.Any(contract => string.Equals(contract.Id, id, StringComparison.OrdinalIgnoreCase))).ToList();

        return state with
        {
            Profiles = profiles,
            SelectedProfileId = selectedProfile,
            Contracts = contractOptions,
            SelectedContractIds = selectedContracts,
            Status = profiles.Count == 0 ? "No LLM profile configured." : $"Expected max LLM calls: {GeneratorPlanStrictLlmEvaluationService.ExpectedMaxLlmCalls(selectedContracts.Count, state.IterationsPerContract, state.EnableRepairAttempt, state.MaxRepairAttempts)}"
        };
    }

    public StrictLlmEvaluationViewState SetMode(StrictLlmEvaluationViewState state, bool latestAuditOnly)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state with
        {
            LatestAuditOnly = latestAuditOnly,
            Status = latestAuditOnly
                ? "Latest-audit mode selected. No LLM call will be made."
                : $"Batch mode selected. Expected max LLM calls: {state.ExpectedMaxLlmCalls}"
        };
    }

    public GeneratorPlanStrictLlmEvaluationRequest BuildRequest(StrictLlmEvaluationViewState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new GeneratorPlanStrictLlmEvaluationRequest
        {
            EvaluateLatestAuditOnly = state.LatestAuditOnly,
            LlmProfileId = state.SelectedProfileId,
            ContractIds = state.SelectedContractIds.Take(4).ToList(),
            IterationsPerContract = Math.Clamp(state.IterationsPerContract, 1, 10),
            EnableRepairAttempt = state.EnableRepairAttempt,
            MaxRepairAttempts = state.EnableRepairAttempt ? Math.Clamp(state.MaxRepairAttempts, 0, 2) : 0,
            StageValidArtifactsForReview = state.StageValidArtifactsForReview,
            MaxTokens = state.MaxTokens,
            Temperature = state.Temperature,
            ExtraUserBrief = state.ExtraBrief
        };
    }

    public StrictLlmEvaluationViewState FromLatestAudit(
        StrictLlmEvaluationViewState state,
        GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult latest)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(latest);

        if (!latest.Exists || latest.GenerationArtifact == null)
        {
            return state with
            {
                LatestAuditSummary = "No strict LLM generation audit found.",
                Status = "No strict LLM generation audit found."
            };
        }

        return state with
        {
            LatestAuditSummary = string.Join(Environment.NewLine, new[]
            {
                $"Audit artifact id: {latest.GenerationArtifact.Id}",
                $"Status: {latest.Result.Status}",
                $"Source selection: {latest.Result.SourceCapabilitySelectionId}",
                $"Contracts: {string.Join(", ", latest.Result.RequestedContractIds)}",
                $"Artifacts: {latest.Result.Artifacts.Count}",
                $"Attempts: {latest.Result.Attempts.Count}",
                $"Diagnostics: {latest.Result.Diagnostics.Count}"
            }),
            Status = "Latest strict LLM generation audit loaded."
        };
    }

    public StrictLlmEvaluationViewState FromEvaluationResult(
        StrictLlmEvaluationViewState state,
        GeneratorPlanStrictLlmEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(result);

        return state with
        {
            Status = result.Ok ? result.Status : BuildFailureStatus(result),
            SummaryText = BuildSummaryText(result),
            ContractRows = result.ContractSummaries.Select(contract => new StrictLlmEvaluationContractRow
            {
                ContractId = contract.ContractId,
                Runs = contract.Runs,
                InitialPass = contract.InitialPass,
                RepairPass = contract.RepairPass,
                Failed = contract.Failed,
                ValidArtifacts = contract.ValidArtifacts,
                AverageAttempts = contract.AverageAttempts,
                TopDiagnosticCodes = string.Join(", ", contract.TopDiagnosticCodes)
            }).ToList(),
            DiagnosticRows = result.DiagnosticSummaries.Select(diagnostic => new StrictLlmEvaluationDiagnosticRow
            {
                Severity = diagnostic.Severity,
                Code = diagnostic.Code,
                ContractId = diagnostic.ContractId,
                Target = diagnostic.Target,
                Count = diagnostic.Count,
                ExampleMessage = diagnostic.ExampleMessage
            }).ToList(),
            SampleRows = result.Samples.Select(sample => new StrictLlmEvaluationSampleRow
            {
                ContractId = sample.ContractId,
                ArtifactId = sample.ArtifactId,
                Valid = sample.Valid,
                Repaired = sample.Repaired,
                ContentExcerpt = sample.ContentExcerpt,
                DiagnosticExcerpt = sample.DiagnosticExcerpt
            }).ToList(),
            ReportMarkdown = result.MarkdownReport,
            EvaluationJson = JsonSerializer.Serialize(result, JsonOptions)
        };
    }

    public StrictLlmEvaluationViewState FromLatestEvaluation(
        StrictLlmEvaluationViewState state,
        GeneratorPlanStrictLlmEvaluationArtifactReadResult latest)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(latest);

        if (!latest.Exists || latest.EvaluationArtifact == null)
        {
            return state with
            {
                Status = "No strict LLM evaluation artifact found.",
                SummaryText = "No strict LLM evaluation artifact found.",
                EvaluationJson = string.Empty,
                ReportMarkdown = string.Empty,
                ContractRows = Array.Empty<StrictLlmEvaluationContractRow>(),
                DiagnosticRows = Array.Empty<StrictLlmEvaluationDiagnosticRow>(),
                SampleRows = Array.Empty<StrictLlmEvaluationSampleRow>()
            };
        }

        return FromEvaluationResult(state, latest.Result with
        {
            MarkdownReport = string.IsNullOrWhiteSpace(latest.MarkdownReport) ? latest.Result.MarkdownReport : latest.MarkdownReport
        }) with
        {
            Status = "Latest strict LLM evaluation loaded.",
            EvaluationJson = latest.EvaluationArtifact.Json
        };
    }

    private static string BuildSummaryText(GeneratorPlanStrictLlmEvaluationResult result)
    {
        var summary = result.Summary;
        return string.Join(Environment.NewLine, new[]
        {
            $"Evaluation: {result.EvaluationId}",
            $"Mode: {result.Mode}",
            $"Status: {result.Status}",
            $"Expected max LLM calls: {result.ExpectedMaxLlmCalls}",
            $"Runs: {summary.TotalGenerationRuns}",
            $"Attempts: {summary.TotalAttempts}",
            $"Initial pass: {summary.InitialPassCount}",
            $"Repair pass: {summary.RepairPassCount}",
            $"Failed: {summary.FailedCount}",
            $"Valid artifacts: {summary.ValidArtifactCount}",
            $"Staged for review: {summary.StagedForReviewCount}",
            $"Overall pass rate: {summary.OverallPassRate:P1}",
            $"Repair recovery rate: {summary.RepairRecoveryRate:P1}",
            $"Diagnostics: {result.DiagnosticSummaries.Sum(diagnostic => diagnostic.Count)}"
        });
    }

    private static string BuildFailureStatus(GeneratorPlanStrictLlmEvaluationResult result)
    {
        return result.Diagnostics.Count == 0
            ? result.Status
            : string.Join("; ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
    }

    private static string? FirstExisting(string selectedId, IEnumerable<string> ids)
    {
        return ids.FirstOrDefault(id => string.Equals(id, selectedId, StringComparison.OrdinalIgnoreCase));
    }
}
