using System.Drawing;
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
        Assert.Equal("Псевдо-3D сетка от первого лица", state.PresentationModes[0].Help.DisplayNameRu);
        Assert.Contains("\u041e\u0431\u044f\u0437\u0430\u0442\u0435\u043b\u044c\u043d\u0430\u044f \u0442\u0435\u0445\u043d\u0438\u0447\u0435\u0441\u043a\u0430\u044f \u0431\u0430\u0437\u0430", state.FeatureBundles[0].Help.DisplayNameRu);
        Assert.Equal("feature_bundle/core_atlas_planning/v1", Assert.Single(state.SelectedFeatureBundleIds));
        Assert.True(state.FeatureBundles[0].IsRequiredTechnicalBase);
        Assert.Contains(state.AvailableModules, module => module.Id == "module/progression/perk_tree");
        Assert.Contains(state.AvailableModifiers, modifier => modifier.Id == "modifier/combat/hybrid_realtime_turn_toggle");
        Assert.Contains(state.AvailableConstraints, constraint => constraint.Id == "constraint/balance/no_player_rubberbanding");
        Assert.Contains(state.AvailableRuntimeRequirements, requirement => requirement.Id == "runtime_requirement/requires_party_state");
    }

    [Fact]
    public void PageControlCanBeConstructedBeforeLayout()
    {
        using var control = new LLMGameCreator.WinForms.Pages.CapabilityPickerPageControl();

        control.Size = new Size(900, 600);
        control.PerformLayout();

        Assert.Equal("capability_picker", control.Id);
    }

    [Fact]
    public void PresenterBuildsUsefulAtlasFallbackHelpForUnknownVisibleOption()
    {
        var state = new CapabilityPickerPresenter().FromAtlas(new CapabilityPickerViewState(), new GeneratorPlanCapabilitySelectionAtlas
        {
            AtlasRootPath = "atlas",
            PresentationModes = [Option("presentation_mode/custom_visible", "Custom Visible", "Atlas purpose for a visible option.")],
            WorldTopologies = [Option("world_topology/custom", "Custom World")],
            ActorModels = [Option("actor_model/custom", "Custom Actor")],
            InventoryModels = [Option("inventory_model/custom", "Custom Inventory")],
            CombatModels = [Option("combat_model/custom", "Custom Combat")],
            ProgressionModels = [Option("progression_model/custom", "Custom Progression")],
            PathfindingProfiles = [Option("pathfinding/custom", "Custom Path")],
            NpcBehaviorModels = [Option("npc_behavior/custom", "Custom NPC")],
            RuntimeTargets = [Option("custom_runtime", "Custom Runtime")]
        });

        var help = state.PresentationModes[0].Help;

        Assert.Equal("atlas_fallback", help.ImplementationStatus);
        Assert.Contains("Custom Visible", state.PresentationModes[0].DisplayName);
        Assert.Contains("presentation_mode/custom_visible", state.PresentationModes[0].DisplayName);
        Assert.Contains("Atlas purpose for a visible option.", help.ShortDescriptionRu);
        Assert.Contains("\u041f\u043e\u0434\u0440\u043e\u0431\u043d\u0430\u044f \u0440\u0443\u0441\u0441\u043a\u0430\u044f \u0441\u043f\u0440\u0430\u0432\u043a\u0430 \u043f\u043e\u043a\u0430 \u043d\u0435 \u043d\u0430\u043f\u0438\u0441\u0430\u043d\u0430", help.DetailsRu);
    }

    [Fact]
    public void PresenterMapsDiagnostics()
    {
        var presenter = new CapabilityPickerPresenter();
        var state = presenter.FromSelectionResult(new CapabilityPickerViewState(), Result());

        Assert.Equal("ready_with_warnings", state.Status);
        Assert.Contains(state.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended);
        Assert.Contains(state.Diagnostics, diagnostic => diagnostic.Category == GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky);
        Assert.Contains("\u0440\u0438\u0441\u043a", state.Diagnostics[0].CategoryDisplayName, StringComparison.OrdinalIgnoreCase);
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
            SelectedFeatureBundleIds = ["feature_bundle/core_atlas_planning/v1"],
            SelectedModuleIds = ["module/progression/perk_tree", "module/progression/skill_xp"],
            SelectedModifierIds = ["modifier/combat/hybrid_realtime_turn_toggle"],
            SelectedConstraintIds = ["constraint/balance/no_player_rubberbanding"],
            RuntimeRequirementIds = ["runtime_requirement/requires_party_state"]
        };

        var request = presenter.BuildRequest(state);

        Assert.Equal("Test", request.Title);
        Assert.Equal("headless", Assert.Single(request.SelectedRuntimeTargetIds));
        Assert.Equal("feature_bundle/core_atlas_planning/v1", Assert.Single(request.SelectedFeatureBundleIds));
        Assert.Equal(new[] { "module/progression/perk_tree", "module/progression/skill_xp" }, request.SelectedModuleIds);
        Assert.Equal("modifier/combat/hybrid_realtime_turn_toggle", Assert.Single(request.SelectedModifierIds));
        Assert.Equal("constraint/balance/no_player_rubberbanding", Assert.Single(request.SelectedConstraintIds));
        Assert.Equal("runtime_requirement/requires_party_state", Assert.Single(request.RuntimeRequirementIds));
    }

    [Fact]
    public void PresenterMapsLatestSelection()
    {
        var presenter = new CapabilityPickerPresenter();
        var state = presenter.FromLatestSelection(new CapabilityPickerViewState(), Latest());

        Assert.Equal("Loaded", state.Title);
        Assert.Equal("presentation_mode/first_person_grid_2d_textures", state.PresentationModeId);
        Assert.Equal("feature_bundle/core_atlas_planning/v1", Assert.Single(state.SelectedFeatureBundleIds));
        Assert.Equal("module/progression/perk_tree", Assert.Single(state.SelectedModuleIds));
        Assert.Equal("modifier/combat/hybrid_realtime_turn_toggle", Assert.Single(state.SelectedModifierIds));
        Assert.Equal("constraint/balance/no_player_rubberbanding", Assert.Single(state.SelectedConstraintIds));
        Assert.Equal("runtime_requirement/requires_party_state", Assert.Single(state.RuntimeRequirementIds));
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
                    Code = GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended,
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
            SelectedModuleIds = ["module/progression/perk_tree"],
            SelectedModifierIds = ["modifier/combat/hybrid_realtime_turn_toggle"],
            SelectedConstraintIds = ["constraint/balance/no_player_rubberbanding"],
            RuntimeRequirementIds = ["runtime_requirement/requires_party_state"],
            SelectedRuntimeTargets = ["headless"]
        };
    }

    private static GeneratorPlanCapabilitySelectionAtlasOption Option(string id, string title)
    {
        return Option(id, title, title);
    }

    private static GeneratorPlanCapabilitySelectionAtlasOption Option(string id, string title, string purpose)
    {
        return new GeneratorPlanCapabilitySelectionAtlasOption
        {
            Id = id,
            Title = title,
            Purpose = purpose
        };
    }
}
