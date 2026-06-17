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
        Assert.DoesNotContain("M5", markdown);
        Assert.DoesNotContain("M6", markdown);
    }

    [Fact]
    public void MarkdownRendererWarnsHighJsonInvalidCount()
    {
        var result = Result() with
        {
            Summary = Result().Summary with
            {
                TotalAttempts = 4,
                JsonInvalidCount = 1
            }
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("Tighten the strict JSON prompt", markdown);
    }

    [Fact]
    public void MarkdownRendererShowsWarningsWithHighPassRate()
    {
        var result = new GeneratorPlanStrictLlmEvaluationResult
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
                OverallPassRate = 1,
                FailedCount = 0
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
            DiagnosticSummaries = [],
            Samples = [],
            Diagnostics =
            [
                new GeneratorPlanStrictLlmArtifactDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = GeneratorPlanStrictLlmEvaluationDiagnosticCodes.GenericTextWarning,
                    ContractId = "game_profile_v1",
                    Target = "game.title",
                    Message = "Generic placeholder text found."
                }
            ]
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("## Recommendations", markdown);
        Assert.Contains("Tighten prompt guidance", markdown);
        Assert.Contains("## Quality warnings", markdown);
    }

    [Fact]
    public void MarkdownRendererDeterministicForSameResult()
    {
        var result = Result();
        var renderer = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer();

        var markdown1 = renderer.Render(result);
        var markdown2 = renderer.Render(result);

        Assert.Equal(markdown1, markdown2);
    }

    [Fact]
    public void MarkdownRendererHandlesEmptyDiagnosticsAndSamples()
    {
        var result = new GeneratorPlanStrictLlmEvaluationResult
        {
            Ok = true,
            Status = GeneratorPlanStrictLlmEvaluationStatus.Evaluated,
            Mode = GeneratorPlanStrictLlmEvaluationMode.Batch,
            EvaluationId = "strict_llm_evaluation/test",
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
            SourceCapabilitySelectionId = "selection/test",
            RequestedContractIds = ["game_profile_v1"],
            IterationsPerContract = 1,
            RepairEnabled = false,
            MaxRepairAttempts = 0,
            ExpectedMaxLlmCalls = 1,
            Summary = new GeneratorPlanStrictLlmEvaluationSummary
            {
                TotalContractsRequested = 1,
                TotalGenerationRuns = 1,
                TotalAttempts = 1,
                InitialPassCount = 0,
                FailedCount = 1,
                OverallPassRate = 0
            },
            ContractSummaries = [],
            DiagnosticSummaries = [],
            Samples = [],
            Diagnostics = []
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("## Summary", markdown);
        Assert.Contains("## Per-contract summary", markdown);
        Assert.Contains("## Diagnostic hot spots", markdown);
        Assert.Contains("## Samples", markdown);
        Assert.Contains("## Recommendations", markdown);
        Assert.Contains("No contract summaries", markdown);
        Assert.Contains("No diagnostics were reported", markdown);
        Assert.Contains("No samples were captured", markdown);
    }

    [Fact]
    public void MarkdownRendererGoldenRecommendationsMatchFixture()
    {
        var result = new GeneratorPlanStrictLlmEvaluationResult
        {
            Ok = true,
            Status = GeneratorPlanStrictLlmEvaluationStatus.Evaluated,
            Mode = GeneratorPlanStrictLlmEvaluationMode.Batch,
            EvaluationId = "strict_llm_evaluation/example",
            EvaluatedAtUtc = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            SourceCapabilitySelectionId = "selection/example",
            RequestedContractIds = ["game_profile_v1"],
            IterationsPerContract = 1,
            RepairEnabled = true,
            MaxRepairAttempts = 1,
            ExpectedMaxLlmCalls = 4,
            Summary = new GeneratorPlanStrictLlmEvaluationSummary
            {
                TotalContractsRequested = 1,
                TotalGenerationRuns = 4,
                TotalAttempts = 4,
                InitialPassCount = 1,
                RepairPassCount = 2,
                FailedCount = 1,
                ValidArtifactCount = 3,
                StagedForReviewCount = 0,
                MarkdownFenceErrorCount = 1,
                JsonWrapperErrorCount = 0,
                JsonInvalidCount = 1,
                WrongArtifactKindCount = 0,
                ForbiddenFieldCount = 0,
                InvalidIdCount = 0,
                MissingFieldCount = 1,
                ExpectedMaxLlmCalls = 4,
                OverallPassRate = 0.75,
                RepairRecoveryRate = 1
            },
            ContractSummaries =
            [
                new GeneratorPlanStrictLlmEvaluationContractSummary
                {
                    ContractId = "game_profile_v1",
                    Runs = 4,
                    InitialPass = 1,
                    RepairPass = 2,
                    Failed = 1,
                    ValidArtifacts = 3,
                    AverageAttempts = 1,
                    TopDiagnosticCodes = ["JSON_INVALID", "MISSING_FIELD"]
                }
            ],
            DiagnosticSummaries = [],
            Samples = [],
            Diagnostics = []
        };

        var markdown = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer().Render(result);

        Assert.Contains("## Summary", markdown);
        Assert.Contains("## Recommendations", markdown);
        Assert.Contains("Tighten the strict JSON prompt", markdown);
        Assert.Contains("Add or refine a repair rule", markdown);
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
