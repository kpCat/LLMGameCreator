using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class StrictLlmArtifactsPresenterTests
{
    [Fact]
    public void PresenterBuildsProfileAndContractOptions()
    {
        var state = new StrictLlmArtifactsPresenter().FromSettings(new StrictLlmArtifactsViewState(), Settings(), new GeneratorPlanStrictLlmArtifactContractCatalog().ListContracts());

        Assert.Equal("local", state.SelectedProfileId);
        Assert.Contains(state.Contracts, contract => contract.Id == "game_profile_v1");
        Assert.Contains(state.Contracts, contract => contract.Id == "region_pack_v1" && contract.Title == "Region pack");
        Assert.Contains(state.Contracts, contract => contract.Id == "encounter_pack_v1" && contract.Title == "Encounter pack");
    }

    [Fact]
    public void PresenterBuildsRequest()
    {
        var state = new StrictLlmArtifactsViewState
        {
            SelectedProfileId = "local",
            SelectedContractIds = ["game_profile_v1"],
            MaxTokens = 3000,
            Temperature = 0.15,
            EnableRepairAttempt = true,
            StageForReview = true,
            ExtraBrief = "Keep it compact."
        };

        var request = new StrictLlmArtifactsPresenter().BuildRequest(state);

        Assert.Equal("local", request.LlmProfileId);
        Assert.Equal("game_profile_v1", Assert.Single(request.ContractIds));
        Assert.Equal(3000, request.MaxTokens);
        Assert.True(request.StageForReview);
    }

    [Fact]
    public void PresenterMapsGenerationResult()
    {
        var result = new GeneratorPlanStrictLlmArtifactGenerationResult
        {
            Ok = true,
            Status = "generated",
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Artifacts =
            [
                new GeneratorPlanStrictLlmGeneratedArtifact
                {
                    ArtifactId = "artifact/strict_llm/game_profile_v1",
                    ArtifactKind = "game_profile_v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    Valid = true,
                    Repaired = true
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanStrictLlmArtifactDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "test.warning",
                    ContractId = "game_profile_v1",
                    Message = "warning"
                }
            ]
        };

        var state = new StrictLlmArtifactsPresenter().FromGenerationResult(new StrictLlmArtifactsViewState(), result);

        Assert.Single(state.ArtifactRows);
        Assert.Single(state.DiagnosticRows);
        Assert.Contains("game_profile_v1", state.ResultJson);
    }

    [Fact]
    public void PresenterShowsMissingSelectionMessage()
    {
        var state = new StrictLlmArtifactsPresenter().FromLatestSelection(new StrictLlmArtifactsViewState(), new GeneratorPlanCapabilitySelectionArtifactReadResult());

        Assert.False(state.HasLatestSelection);
        Assert.Contains("Capability Picker", state.Status);
    }

    [Fact]
    public void PreviewPromptShownWithoutGenerationResult()
    {
        var state = new StrictLlmArtifactsPresenter().FromPreview(new StrictLlmArtifactsViewState(), new GeneratorPlanStrictLlmArtifactPromptPreviewResult
        {
            Ok = true,
            Status = "preview_ready",
            ContractId = "game_profile_v1",
            PromptText = "Return exactly one JSON object."
        });

        Assert.Contains("JSON object", state.PromptPreview);
        Assert.Empty(state.ResultJson);
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
