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
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        var state = new StrictLlmArtifactsPresenter().FromSettings(
            new StrictLlmArtifactsViewState(),
            Settings(),
            catalog.ListContracts(),
            catalog.ListBatchPresets());

        Assert.Equal("local", state.SelectedProfileId);
        Assert.Contains(state.Contracts, contract => contract.Id == "game_profile_v1");
        Assert.Contains(state.Contracts, contract => contract.Id == "region_pack_v1" && contract.Title == "Region pack");
        Assert.Contains(state.Contracts, contract => contract.Id == "encounter_pack_v1" && contract.Title == "Encounter pack");
        Assert.Equal(string.Empty, state.SelectedBatchPresetId);
        Assert.Equal("Manual/custom", state.BatchPresets[0].DisplayName);
        Assert.Contains(state.BatchPresets, preset => preset.Id == "full_small_rpg_seed");
    }

    [Fact]
    public void PresenterAppliesBaselineAndFullBatchPresetsInContractListOrder()
    {
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        var presenter = new StrictLlmArtifactsPresenter();
        var state = presenter.FromSettings(
            new StrictLlmArtifactsViewState(),
            Settings(),
            catalog.ListContracts(),
            catalog.ListBatchPresets());

        var baseline = presenter.ApplyBatchPreset(state, "baseline_game_seed", catalog);
        var full = presenter.ApplyBatchPreset(state, "full_small_rpg_seed", catalog);

        Assert.Equal(
            ["game_profile_v1", "mechanics_pack_v1", "quest_pack_v1", "scene_pack_v1"],
            baseline.SelectedContractIds);
        Assert.Equal(state.Contracts.Select(contract => contract.Id), full.SelectedContractIds);
        Assert.Equal(9, full.SelectedContractIds.Count);
    }

    [Fact]
    public void UnknownBatchPresetPreservesCurrentContractSelection()
    {
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        var state = new StrictLlmArtifactsViewState
        {
            SelectedBatchPresetId = "baseline_game_seed",
            SelectedContractIds = ["game_profile_v1", "quest_pack_v1"]
        };

        var result = new StrictLlmArtifactsPresenter().ApplyBatchPreset(state, "missing_preset", catalog);

        Assert.Equal(state.SelectedBatchPresetId, result.SelectedBatchPresetId);
        Assert.Equal(state.SelectedContractIds, result.SelectedContractIds);
        Assert.Contains("not found", result.Status);
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
