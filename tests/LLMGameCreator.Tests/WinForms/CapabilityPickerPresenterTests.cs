using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.WinForms.Pages.CapabilityPicker;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class CapabilityPickerPresenterTests
{
    [Fact]
    public void PresenterBuildsVariantOptions()
    {
        var state = new CapabilityPickerPresenter().FromAtlas(new CapabilityPickerViewState(), Atlas());

        Assert.Equal("presentation_mode/first_person_grid_2d_textures", state.PresentationModeId);
        Assert.Single(state.PresentationModes);
        Assert.Single(state.FeatureBundles);
        Assert.Equal("headless", state.RuntimeTargetId);
    }

    [Fact]
    public void PresenterMapsDiagnostics()
    {
        var presenter = new CapabilityPickerPresenter();
        var state = presenter.FromSelectionResult(new CapabilityPickerViewState(), Result());

        Assert.Equal("ready_with_warnings", state.Status);
        Assert.Contains(state.Diagnostics, diagnostic => diagnostic.Code == "test.warning");
        Assert.Contains("warning", state.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PresenterBuildsRequestFromSelections()
    {
        var presenter = new CapabilityPickerPresenter();
        var state = new CapabilityPickerViewState
        {
            AtlasRootPath = "atlas",
            Title = "Test",
            Purpose = "Purpose",
            PresentationModeId = "presentation_mode/first_person_grid_2d_textures",
            WorldTopologyId = "world_topology/first_person_grid_dungeon",
            ActorModelId = "actor_model/party_blob",
            InventoryModelId = "inventory_model/grid_inventory",
            CombatModelId = "combat_model/blobber_party_turn_based",
            ProgressionModelId = "progression_model/level_xp",
            PathfindingProfileId = "pathfinding/first_person_grid_movement",
            NpcBehaviorModelId = "npc_behavior/static",
            RuntimeTargetId = "headless",
            SelectedFeatureBundleIds = ["feature_bundle/core_atlas_planning/v1"]
        };

        var request = presenter.BuildRequest(state);

        Assert.Equal("Test", request.Title);
        Assert.Equal("headless", Assert.Single(request.SelectedRuntimeTargetIds));
        Assert.Equal("feature_bundle/core_atlas_planning/v1", Assert.Single(request.SelectedFeatureBundleIds));
    }

    [Fact]
    public void PresenterMapsLatestSelection()
    {
        var presenter = new CapabilityPickerPresenter();
        var state = presenter.FromLatestSelection(new CapabilityPickerViewState(), Latest());

        Assert.Equal("Loaded", state.Title);
        Assert.Equal("presentation_mode/first_person_grid_2d_textures", state.PresentationModeId);
        Assert.Equal("feature_bundle/core_atlas_planning/v1", Assert.Single(state.SelectedFeatureBundleIds));
        Assert.Contains("latest", state.Summary, StringComparison.OrdinalIgnoreCase);
    }

    private static GeneratorPlanCapabilitySelectionAtlas Atlas()
    {
        return new GeneratorPlanCapabilitySelectionAtlas
        {
            AtlasRootPath = "atlas",
            PresentationModes = [Option("presentation_mode/first_person_grid_2d_textures", "First Person Grid")],
            WorldTopologies = [Option("world_topology/first_person_grid_dungeon", "Grid Dungeon")],
            ActorModels = [Option("actor_model/party_blob", "Party Blob")],
            InventoryModels = [Option("inventory_model/grid_inventory", "Grid Inventory")],
            CombatModels = [Option("combat_model/blobber_party_turn_based", "Blobber Combat")],
            ProgressionModels = [Option("progression_model/level_xp", "Level XP")],
            PathfindingProfiles = [Option("pathfinding/first_person_grid_movement", "Grid Movement")],
            NpcBehaviorModels = [Option("npc_behavior/static", "Static")],
            RuntimeTargets = [Option("headless", "Headless")],
            FeatureBundles =
            [
                new GeneratorPlanCapabilitySelectionFeatureBundle
                {
                    Id = "feature_bundle/core_atlas_planning/v1",
                    Title = "Core Atlas Planning",
                    Domain = "core",
                    Category = "bundle_category/core_planning/v1",
                    Purpose = "Core planning.",
                    ArtifactContracts = ["game_profile_v1"]
                }
            ]
        };
    }

    private static GeneratorPlanCapabilitySelectionResult Result()
    {
        return new GeneratorPlanCapabilitySelectionResult
        {
            Ok = true,
            Status = "ready_with_warnings",
            Selection = Selection() with
            {
                Warnings = ["warning"],
                ResolvedArtifactContracts = ["game_profile_v1"],
                ResolvedValidators = ["bundle.required_fields"],
                ResolvedRuntimeTargets = ["headless"],
                RequiredLuaModulesOrGaps = ["gap/example"]
            },
            Diagnostics =
            [
                new GeneratorPlanCapabilitySelectionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "test.warning",
                    Target = "target",
                    Message = "warning"
                }
            ]
        };
    }

    private static GeneratorPlanCapabilitySelectionArtifactReadResult Latest()
    {
        return new GeneratorPlanCapabilitySelectionArtifactReadResult
        {
            Exists = true,
            SelectionArtifact = new GeneratedArtifactRecord(
                GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactId,
                GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactKind,
                GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactPath,
                "{}",
                GeneratorPlanCapabilitySelectionArtifactIds.GeneratedBy,
                "warnings",
                "{}"),
            Selection = Selection()
        };
    }

    private static GeneratorPlanCapabilitySelection Selection()
    {
        return new GeneratorPlanCapabilitySelection
        {
            SelectionId = "selection/test",
            Title = "Loaded",
            Purpose = "Loaded purpose.",
            SelectedVariantIds = new GeneratorPlanCapabilitySelectedVariantIds
            {
                PresentationModeId = "presentation_mode/first_person_grid_2d_textures",
                WorldTopologyId = "world_topology/first_person_grid_dungeon",
                ActorModelId = "actor_model/party_blob",
                InventoryModelId = "inventory_model/grid_inventory",
                CombatModelId = "combat_model/blobber_party_turn_based",
                ProgressionModelId = "progression_model/level_xp",
                PathfindingProfileId = "pathfinding/first_person_grid_movement",
                NpcBehaviorModelId = "npc_behavior/static"
            },
            SelectedFeatureBundleIds = ["feature_bundle/core_atlas_planning/v1"],
            SelectedRuntimeTargets = ["headless"]
        };
    }

    private static GeneratorPlanCapabilitySelectionAtlasOption Option(string id, string title)
    {
        return new GeneratorPlanCapabilitySelectionAtlasOption
        {
            Id = id,
            Title = title,
            Purpose = title
        };
    }
}
