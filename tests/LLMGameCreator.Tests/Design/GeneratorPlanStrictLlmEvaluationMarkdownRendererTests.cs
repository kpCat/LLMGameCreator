using LLMGameCreator.Application.Design.GeneratorPlans;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictLlmEvaluationMarkdownRendererTests
{
    [Fact]
    public void MarkdownRendererIncludesSummaryContractDiagnosticsSamplesAndRecommendations()
    {
        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(Result());

        Assert.Contains("## Summary", markdown);
        Assert.Contains("## Per-contract summary", markdown);
        Assert.Contains("## Diagnostic hot spots", markdown);
        Assert.Contains("## Samples", markdown);
        Assert.Contains("## Recommendations", markdown);
    }

    [Fact]
    public void MarkdownRendererRecommendsTightenPromptWhenJsonFailuresHigh()
    {
        var result = Result() with
        {
            Summary = Result().Summary with
            {
                TotalAttempts = 4,
                JsonInvalidCount = 2
            }
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("Tighten the strict JSON prompt", markdown);
    }

    [Fact]
    public void MarkdownRendererRecommendsContractStableWhenPassRateHigh()
    {
        var result = Result() with
        {
            Summary = Result().Summary with
            {
                OverallPassRate = 1,
                FailedCount = 0
            }
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("Contract looks stable", markdown);
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
            SourceCapabilitySelectionId = "selection/test",
            RequestedContractIds = ["game_profile_v1"],
            IterationsPerContract = 1,
            RepairEnabled = true,
            MaxRepairAttempts = 1,
            ExpectedMaxLlmCalls = 2,
            Summary = new GeneratorPlanStrictLlmEvaluationSummary
            {
                TotalContractsRequested = 1,
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
                    Code = GeneratorPlanStrictLlmEvaluationDiagnosticCodes.ShortDescriptionWarning,
                    ContractId = "game_profile_v1",
                    Target = "game.description",
                    Count = 1,
                    ExampleMessage = "Description is short."
                }
            ],
            Samples =
            [
                new GeneratorPlanStrictLlmEvaluationSample
                {
                    ContractId = "game_profile_v1",
                    ArtifactId = "artifact/strict_llm/game_profile_v1",
                    Valid = true,
                    ContentExcerpt = "{\"artifact_kind\":\"game_profile_v1\"}"
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanStrictLlmArtifactDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = GeneratorPlanStrictLlmEvaluationDiagnosticCodes.ShortDescriptionWarning,
                    ContractId = "game_profile_v1",
                    Target = "game.description",
                    Message = "Description is short."
                }
            ]
        };
    }
}
