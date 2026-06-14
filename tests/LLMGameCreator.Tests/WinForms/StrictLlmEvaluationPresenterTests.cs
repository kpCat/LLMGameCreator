using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.WinForms.Pages.StrictLlmEvaluation;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class StrictLlmEvaluationPresenterTests
{
    [Fact]
    public void PresenterComputesExpectedCallCount()
    {
        var state = new StrictLlmEvaluationViewState
        {
            LatestAuditOnly = false,
            SelectedContractIds = ["game_profile_v1", "scene_pack_v1"],
            IterationsPerContract = 3,
            EnableRepairAttempt = true,
            MaxRepairAttempts = 1
        };

        Assert.Equal(12, state.ExpectedMaxLlmCalls);
    }

    [Fact]
    public void PresenterBuildsBatchRequest()
    {
        var state = new StrictLlmEvaluationViewState
        {
            LatestAuditOnly = false,
            SelectedProfileId = "local",
            SelectedContractIds = ["game_profile_v1"],
            IterationsPerContract = 2,
            EnableRepairAttempt = true,
            MaxRepairAttempts = 1,
            StageValidArtifactsForReview = false,
            MaxTokens = 3000,
            Temperature = 0.15,
            ExtraBrief = "Keep it compact."
        };

        var request = new StrictLlmEvaluationPresenter().BuildRequest(state);

        Assert.False(request.EvaluateLatestAuditOnly);
        Assert.Equal("local", request.LlmProfileId);
        Assert.Equal("game_profile_v1", Assert.Single(request.ContractIds));
        Assert.Equal(2, request.IterationsPerContract);
        Assert.False(request.StageValidArtifactsForReview);
    }

    [Fact]
    public void PresenterMapsEvaluationResult()
    {
        var state = new StrictLlmEvaluationPresenter().FromEvaluationResult(new StrictLlmEvaluationViewState(), Result());

        Assert.Contains("Overall pass rate", state.SummaryText);
        Assert.Single(state.ContractRows);
        Assert.Single(state.DiagnosticRows);
        Assert.Single(state.SampleRows);
        Assert.Contains("Strict LLM Generation Evaluation", state.ReportMarkdown);
        Assert.Contains("strict_llm_evaluation/test", state.EvaluationJson);
    }

    [Fact]
    public void PresenterDisablesBatchWithoutProfileOrContracts()
    {
        var state = new StrictLlmEvaluationViewState
        {
            LatestAuditOnly = false,
            Profiles = [new StrictLlmEvaluationProfileOption { Id = "local" }],
            SelectedProfileId = "",
            SelectedContractIds = ["game_profile_v1"]
        };

        Assert.False(state.CanRunBatch);
    }

    [Fact]
    public void PresenterShowsLatestAuditOnlyMode()
    {
        var state = new StrictLlmEvaluationPresenter().SetMode(new StrictLlmEvaluationViewState(), true);

        Assert.True(state.LatestAuditOnly);
        Assert.Contains("No LLM call", state.Status);
    }

    [Fact]
    public void PresenterBuildsProfileAndContractOptions()
    {
        var state = new StrictLlmEvaluationPresenter().FromSettings(new StrictLlmEvaluationViewState(), Settings(), new GeneratorPlanStrictLlmArtifactContractCatalog().ListContracts());

        Assert.Equal("local", state.SelectedProfileId);
        Assert.Contains(state.Contracts, contract => contract.Id == "game_profile_v1");
    }

    private static GeneratorPlanStrictLlmEvaluationResult Result()
    {
        return new GeneratorPlanStrictLlmEvaluationResult
        {
            Ok = true,
            Status = GeneratorPlanStrictLlmEvaluationStatus.Evaluated,
            Mode = GeneratorPlanStrictLlmEvaluationMode.Batch,
            EvaluationId = "strict_llm_evaluation/test",
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
            ExpectedMaxLlmCalls = 2,
            Summary = new GeneratorPlanStrictLlmEvaluationSummary
            {
                TotalGenerationRuns = 1,
                TotalAttempts = 1,
                InitialPassCount = 1,
                ValidArtifactCount = 1,
                OverallPassRate = 1
            },
            ContractSummaries =
            [
                new GeneratorPlanStrictLlmEvaluationContractSummary
                {
                    ContractId = "game_profile_v1",
                    Runs = 1,
                    InitialPass = 1,
                    ValidArtifacts = 1,
                    AverageAttempts = 1
                }
            ],
            DiagnosticSummaries =
            [
                new GeneratorPlanStrictLlmEvaluationDiagnosticSummary
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "quality.warning",
                    ContractId = "game_profile_v1",
                    Target = "description",
                    Count = 1,
                    ExampleMessage = "warning"
                }
            ],
            Samples =
            [
                new GeneratorPlanStrictLlmEvaluationSample
                {
                    ContractId = "game_profile_v1",
                    ArtifactId = "artifact/strict_llm/game_profile_v1",
                    Valid = true,
                    ContentExcerpt = "{}"
                }
            ],
            MarkdownReport = "# Strict LLM Generation Evaluation"
        };
    }

    private static AppSettings Settings()
    {
        return new AppSettings
        {
            DefaultLlmProfileId = "local",
            LlmProfiles =
            [
                new LlmEndpointSettings
                {
                    Id = "local",
                    Title = "Local",
                    Endpoint = "http://localhost:1234/v1",
                    Model = "fake"
                }
            ]
        };
    }
}
