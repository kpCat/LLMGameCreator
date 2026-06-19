using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanCapabilitySelectionServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    [Fact]
    public async Task BuildSelectionAcceptsKnownFirstPersonGridPartyBlobShape()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path), CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Equal("presentation_mode/first_person_grid_2d_textures", result.Selection.SelectedVariantIds.PresentationModeId);
        Assert.Contains("feature_bundle/core_atlas_planning/v1", result.Selection.SelectedFeatureBundleIds);
    }

    [Fact]
    public async Task BuildSelectionRejectsUnknownPresentationMode()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path) with
        {
            PresentationModeId = "presentation_mode/missing"
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownVariantId);
    }

    [Fact]
    public async Task BuildSelectionRejectsIncompatiblePresentationWorldCombination()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path) with
        {
            WorldTopologyId = "world_topology/single_map"
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.IncompatiblePresentationWorld);
    }

    [Fact]
    public async Task BuildSelectionWarnsWhenActorOrCombatIsNotRecommended()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path) with
        {
            PresentationModeId = "presentation_mode/top_down_2d",
            WorldTopologyId = "world_topology/single_map",
            ActorModelId = "actor_model/party_blob",
            CombatModelId = "combat_model/blobber_party_turn_based"
        }, CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended);
        Assert.Equal(GeneratorPlanCapabilitySelectionStatus.ReadyWithWarnings, result.Status);
    }

    [Fact]
    public async Task BuildSelectionResolvesFeatureBundleContractsValidatorsAndRuntimeTargets()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path), CancellationToken.None);

        Assert.Contains("game_profile_v1", result.Selection.ResolvedArtifactContracts);
        Assert.Contains("bundle.required_fields", result.Selection.ResolvedValidators);
        Assert.Contains("headless", result.Selection.ResolvedRuntimeTargets);
        Assert.Contains("strict_single_json_artifact", result.Selection.ResolvedPromptContextTemplates);
    }

    [Fact]
    public async Task BuildSelectionWarnsForMissingFutureContractsButDoesNotFail()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path) with
        {
            SelectedFeatureBundleIds = ["feature_bundle/future_contract/v1"]
        }, CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingArtifactContract);
        Assert.Contains("future_contract_v1", result.Selection.ResolvedArtifactContracts);
    }

    [Fact]
    public async Task BuildSelectionPreservesComposableSelectionsAndKeepsHybridCombatNonFatal()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var service = CreateService();

        var result = await service.BuildSelectionAsync(KnownBlobRequest(temp.Path) with
        {
            SelectedModuleIds =
            [
                "module/progression/perk_tree",
                "module/progression/skill_xp",
                "module/economy/trading",
                "module/future/custom"
            ],
            SelectedModifierIds = ["modifier/combat/hybrid_realtime_turn_toggle"],
            SelectedConstraintIds = ["constraint/balance/no_player_rubberbanding"],
            RuntimeRequirementIds = ["runtime_requirement/requires_party_state"]
        }, CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Contains("module/progression/perk_tree", result.Selection.SelectedModuleIds);
        Assert.Contains("module/progression/skill_xp", result.Selection.SelectedModuleIds);
        Assert.Contains("modifier/combat/hybrid_realtime_turn_toggle", result.Selection.SelectedModifierIds);
        Assert.Contains("constraint/balance/no_player_rubberbanding", result.Selection.SelectedConstraintIds);
        Assert.Contains("runtime_requirement/requires_party_state", result.Selection.RuntimeRequirementIds);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.ComposableCapabilityInfo && diagnostic.Target == "modifier/combat/hybrid_realtime_turn_toggle");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.ComposableCapabilityUnsupportedYet && diagnostic.Target == "module/economy/trading");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownComposableCapabilityId && diagnostic.Target == "module/future/custom");
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error && diagnostic.Target.Contains("progression", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SaveAndReadLatestSelectionArtifact()
    {
        using var temp = new TempDirectory();
        WriteMinimalAtlas(temp.Path);
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), CancellationToken.None);
        var result = await CreateService().BuildSelectionAsync(KnownBlobRequest(temp.Path), CancellationToken.None);
        var artifactService = new GeneratorPlanCapabilitySelectionArtifactService(database);
        var reader = new GeneratorPlanCapabilitySelectionArtifactReader(database);

        var saved = await artifactService.SaveAsync(result, CancellationToken.None);
        var loaded = await reader.ReadLatestAsync(CancellationToken.None);

        Assert.Equal(GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactId, saved.SelectionArtifact.Id);
        Assert.True(loaded.Exists);
        Assert.Equal(result.Selection.SelectionId, loaded.Selection.SelectionId);
        Assert.Equal(result.Selection.SelectedVariantIds.PresentationModeId, loaded.Selection.SelectedVariantIds.PresentationModeId);
    }

    [Fact]
    public void OldSelectionJsonDeserializesWithEmptyComposableArrays()
    {
        var selection = JsonSerializer.Deserialize<GeneratorPlanCapabilitySelection>("""
        {
          "schema_version": "0.1",
          "selection_id": "selection/old",
          "selected_variant_ids": {
            "presentation_mode_id": "presentation_mode/map_and_panel_rpg"
          },
          "selected_feature_bundle_ids": ["feature_bundle/core_atlas_planning/v1"]
        }
        """, JsonOptions);

        Assert.NotNull(selection);
        Assert.Empty(selection!.SelectedModuleIds);
        Assert.Empty(selection.SelectedModifierIds);
        Assert.Empty(selection.SelectedConstraintIds);
        Assert.Empty(selection.RuntimeRequirementIds);
    }

    [Fact]
    public void NewSelectionJsonRoundTripsComposableArrays()
    {
        var selection = new GeneratorPlanCapabilitySelection
        {
            SelectionId = "selection/new",
            SelectedModuleIds = ["module/progression/perk_tree"],
            SelectedModifierIds = ["modifier/combat/hybrid_realtime_turn_toggle"],
            SelectedConstraintIds = ["constraint/balance/no_player_rubberbanding"],
            RuntimeRequirementIds = ["runtime_requirement/requires_party_state"]
        };

        var json = JsonSerializer.Serialize(selection, JsonOptions);
        var loaded = JsonSerializer.Deserialize<GeneratorPlanCapabilitySelection>(json, JsonOptions);

        Assert.Equal("module/progression/perk_tree", Assert.Single(loaded!.SelectedModuleIds));
        Assert.Equal("modifier/combat/hybrid_realtime_turn_toggle", Assert.Single(loaded.SelectedModifierIds));
        Assert.Equal("constraint/balance/no_player_rubberbanding", Assert.Single(loaded.SelectedConstraintIds));
        Assert.Equal("runtime_requirement/requires_party_state", Assert.Single(loaded.RuntimeRequirementIds));
    }

    [Fact]
    public void HelpCatalogReturnsRussianMetadataAndSafeFallback()
    {
        var known = GeneratorPlanCapabilityHelpCatalog.Get("presentation_mode/map_and_panel_rpg");
        var unknown = GeneratorPlanCapabilityHelpCatalog.Get("feature_bundle/unknown/v1");

        Assert.Equal("Карта + панельная RPG", known.DisplayNameRu);
        Assert.Contains("регион", known.ShortDescriptionRu, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("feature_bundle/unknown/v1", unknown.DisplayNameRu);
        Assert.Equal("metadata_missing", unknown.ImplementationStatus);
    }

    [Fact]
    public void CoreAtlasPlanningHelpExplainsRequiredTechnicalGenerationBase()
    {
        var help = GeneratorPlanCapabilityHelpCatalog.Get("feature_bundle/core_atlas_planning/v1");

        Assert.Contains("\u041e\u0431\u044f\u0437\u0430\u0442\u0435\u043b\u044c\u043d\u0430\u044f \u0442\u0435\u0445\u043d\u0438\u0447\u0435\u0441\u043a\u0430\u044f \u0431\u0430\u0437\u0430", help.DisplayNameRu);
        Assert.Contains("\u042d\u0442\u043e \u043d\u0435 \u0438\u0433\u0440\u043e\u0432\u0430\u044f \u043c\u0435\u0445\u0430\u043d\u0438\u043a\u0430", help.ShortDescriptionRu);
        Assert.Contains("\u041e\u0431\u044b\u0447\u043d\u043e \u043e\u0441\u0442\u0430\u0432\u043b\u044f\u0439 \u0432\u043a\u043b\u044e\u0447\u0451\u043d\u043d\u044b\u043c", help.DetailsRu);
        Assert.DoesNotContain("M4 flow", help.DetailsRu, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCodes.IncompatiblePresentationWorld, GeneratorPlanCapabilitySelectionDiagnosticCategories.Impossible)]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingArtifactContract, GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet)]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCodes.CapabilityGap, GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet)]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended, GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky)]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCodes.Loaded, GeneratorPlanCapabilitySelectionDiagnosticCategories.Info)]
    public void DiagnosticCategoryMappingUsesUserFacingCategories(string code, string expectedCategory)
    {
        Assert.Equal(expectedCategory, GeneratorPlanCapabilityHelpCatalog.MapDiagnosticCategory(code));
    }

    [Theory]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCategories.Impossible, "\u041d\u0435\u043b\u044c\u0437\u044f \u0441\u043e\u0432\u043c\u0435\u0441\u0442\u0438\u0442\u044c")]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet, "\u0418\u0434\u0435\u044f \u0432\u043e\u0437\u043c\u043e\u0436\u043d\u0430")]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky, "\u0435\u0441\u0442\u044c \u0440\u0438\u0441\u043a")]
    [InlineData(GeneratorPlanCapabilitySelectionDiagnosticCategories.Info, "\u0418\u043d\u0444\u043e\u0440\u043c\u0430\u0446\u0438\u044f")]
    public void DiagnosticCategoryDisplayNameUsesRussianMeaning(string category, string expectedText)
    {
        Assert.Contains(expectedText, GeneratorPlanCapabilityHelpCatalog.MapDiagnosticCategoryDisplayName(category));
    }

    [Fact]
    public void StrictPromptIncludesComposableArraysOnlyWhenPresent()
    {
        var contract = new GeneratorPlanStrictLlmArtifactContractCatalog().ListContracts().First(contract => contract.ContractId == "game_profile_v1");
        var builder = new GeneratorPlanStrictLlmArtifactPromptBuilder();
        var emptyPrompt = builder.Build(contract, new GeneratorPlanCapabilitySelection(), new GeneratorPlanStrictLlmArtifactGenerationRequest());
        var composedPrompt = builder.Build(contract, new GeneratorPlanCapabilitySelection
        {
            SelectedModuleIds = ["module/progression/perk_tree"],
            SelectedModifierIds = ["modifier/combat/hybrid_realtime_turn_toggle"],
            SelectedConstraintIds = ["constraint/balance/no_player_rubberbanding"],
            RuntimeRequirementIds = ["runtime_requirement/requires_party_state"]
        }, new GeneratorPlanStrictLlmArtifactGenerationRequest());

        Assert.DoesNotContain("selected_module_ids:", emptyPrompt.UserPrompt);
        Assert.Contains("selected_module_ids:", composedPrompt.UserPrompt);
        Assert.Contains("selected_modifier_ids:", composedPrompt.UserPrompt);
        Assert.Contains("selected_constraint_ids:", composedPrompt.UserPrompt);
        Assert.Contains("runtime_requirement_ids:", composedPrompt.UserPrompt);
    }

    [Fact]
    public async Task AtlasReaderParsesCurrentAtlasFiles()
    {
        var atlasRoot = FindCurrentAtlasRoot();
        var atlas = await new GeneratorPlanCapabilitySelectionAtlasReader().LoadAsync(atlasRoot, CancellationToken.None);

        Assert.DoesNotContain(atlas.Diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(atlas.PresentationModes, option => option.Id == "presentation_mode/first_person_grid_2d_textures");
        Assert.Contains(atlas.WorldTopologies, option => option.Id == "world_topology/first_person_grid_dungeon");
        Assert.Contains(atlas.FeatureBundles, bundle => bundle.Id == "feature_bundle/core_atlas_planning/v1");
        Assert.Contains(atlas.RuntimeTargets, target => target.Id == "headless");
    }

    private static GeneratorPlanCapabilitySelectionService CreateService()
    {
        return new GeneratorPlanCapabilitySelectionService(new GeneratorPlanCapabilitySelectionAtlasReader());
    }

    private static GeneratorPlanCapabilitySelectionRequest KnownBlobRequest(string atlasRoot)
    {
        return new GeneratorPlanCapabilitySelectionRequest
        {
            AtlasRootPath = atlasRoot,
            Title = "Blob Test",
            Purpose = "Validate first-person party shape.",
            PresentationModeId = "presentation_mode/first_person_grid_2d_textures",
            WorldTopologyId = "world_topology/first_person_grid_dungeon",
            ActorModelId = "actor_model/party_blob",
            InventoryModelId = "inventory_model/grid_inventory",
            CombatModelId = "combat_model/blobber_party_turn_based",
            ProgressionModelId = "progression_model/level_xp",
            PathfindingProfileId = "pathfinding/first_person_grid_movement",
            NpcBehaviorModelId = "npc_behavior/static",
            SelectedFeatureBundleIds = ["feature_bundle/core_atlas_planning/v1"],
            SelectedRuntimeTargetIds = ["headless"]
        };
    }

    private static void WriteMinimalAtlas(string root)
    {
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "game_form_factor_taxonomy.json"), """
        {
          "schema_version": "0.1",
          "presentation_modes": [
            {
              "id": "presentation_mode/first_person_grid_2d_textures",
              "title": "First-Person Grid",
              "purpose": "Grid view.",
              "allowed_world_topologies": ["world_topology/first_person_grid_dungeon"],
              "recommended_actor_models": ["actor_model/party_blob"],
              "recommended_combat_models": ["combat_model/blobber_party_turn_based"],
              "required_artifact_contracts": ["world_profile_v1", "map_pack_v1"],
              "required_validators": ["world.first_person_grid_refs_valid"]
            },
            {
              "id": "presentation_mode/top_down_2d",
              "title": "Top Down",
              "purpose": "Top-down view.",
              "allowed_world_topologies": ["world_topology/single_map"],
              "recommended_actor_models": ["actor_model/single_player_character"],
              "recommended_combat_models": ["combat_model/turn_based"],
              "required_artifact_contracts": ["map_pack_v1"],
              "required_validators": ["map.bounds_valid"]
            }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(root, "game_system_variant_taxonomy.json"), """
        {
          "schema_version": "0.1",
          "world_topologies": [
            { "id": "world_topology/first_person_grid_dungeon", "title": "First Person Grid Dungeon", "purpose": "Grid dungeon.", "required_artifact_contracts": ["map_pack_v1"], "required_validators": ["world.grid_reachability"], "compatible_with": ["presentation_mode/first_person_grid_2d_textures"], "incompatible_with": [] },
            { "id": "world_topology/single_map", "title": "Single Map", "purpose": "Single map.", "required_artifact_contracts": ["map_pack_v1"], "required_validators": ["map.bounds_valid"], "compatible_with": [], "incompatible_with": [] }
          ],
          "actor_models": [
            { "id": "actor_model/party_blob", "title": "Party Blob", "purpose": "Party as one actor.", "required_artifact_contracts": ["party_roster_v1"], "required_validators": ["party.roster_valid"], "compatible_with": ["presentation_mode/first_person_grid_2d_textures"], "incompatible_with": [] },
            { "id": "actor_model/single_player_character", "title": "Single Player", "purpose": "Single actor.", "required_artifact_contracts": ["player_character_card_v1"], "required_validators": ["character.card_refs_valid"], "compatible_with": [], "incompatible_with": [] }
          ],
          "inventory_models": [
            { "id": "inventory_model/grid_inventory", "title": "Grid Inventory", "purpose": "Grid.", "required_artifact_contracts": ["inventory_pack_v1"], "required_validators": ["inventory.grid_no_overlap"], "compatible_with": [], "incompatible_with": [] }
          ],
          "combat_models": [
            { "id": "combat_model/blobber_party_turn_based", "title": "Blobber Combat", "purpose": "Party turn combat.", "required_artifact_contracts": ["combat_pack_v1"], "required_validators": ["combat.frontline_valid"], "compatible_with": ["actor_model/party_blob"], "incompatible_with": [] },
            { "id": "combat_model/turn_based", "title": "Turn Based", "purpose": "Turns.", "required_artifact_contracts": ["combat_pack_v1"], "required_validators": ["combat.turn_order_valid"], "compatible_with": [], "incompatible_with": [] }
          ],
          "progression_models": [
            { "id": "progression_model/level_xp", "title": "Level XP", "purpose": "XP.", "required_artifact_contracts": ["progression_pack_v1"], "required_validators": ["progression.level_bounds_valid"], "compatible_with": [], "incompatible_with": [] }
          ],
          "pathfinding_profiles": [
            { "id": "pathfinding/first_person_grid_movement", "title": "First Person Grid Movement", "purpose": "Step movement.", "required_artifact_contracts": ["path_network_v1"], "required_validators": ["path.facing_valid"], "compatible_with": ["presentation_mode/first_person_grid_2d_textures"], "incompatible_with": [] }
          ],
          "npc_behavior_models": [
            { "id": "npc_behavior/static", "title": "Static", "purpose": "Static NPC.", "required_artifact_contracts": ["npc_card_v1"], "required_validators": ["npc.refs_valid"], "compatible_with": [], "incompatible_with": [] }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(root, "feature_bundle_map.json"), """
        {
          "schema_version": "0.1",
          "feature_bundles": [
            {
              "id": "feature_bundle/core_atlas_planning/v1",
              "title": "Core Atlas Planning",
              "domain": "core",
              "category": "bundle_category/core_planning/v1",
              "purpose": "Core selection.",
              "requires": ["core.ids/v1"],
              "provides": ["feature_bundle_selection_v1"],
              "artifact_contracts": ["game_profile_v1", "generator_plan_v1"],
              "validators": ["bundle.required_fields"],
              "runtime_targets": ["debug", "headless"],
              "prompt_context_templates": ["strict_single_json_artifact"]
            },
            {
              "id": "feature_bundle/future_contract/v1",
              "title": "Future Contract",
              "domain": "future",
              "category": "bundle_category/core_planning/v1",
              "purpose": "Future warning.",
              "requires": [],
              "provides": [],
              "artifact_contracts": ["future_contract_v1"],
              "validators": ["future.validator"],
              "runtime_targets": ["headless"],
              "prompt_context_templates": []
            }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(root, "capability_atlas.json"), """
        {
          "atlas_version": "0.1",
          "runtime_targets": [
            { "id": "debug", "title": "Debug", "purpose": "Debug." },
            { "id": "headless", "title": "Headless", "purpose": "Headless." }
          ],
          "domains": [
            {
              "id": "core",
              "title": "Core",
              "capabilities": [
                {
                  "id": "core.ids/v1",
                  "title": "IDs",
                  "provides": ["id_rules"],
                  "depends_on": [],
                  "output_contracts": ["diagnostics_v1"],
                  "validators": ["id_format"],
                  "runtime_targets": ["debug", "headless"]
                }
              ]
            }
          ],
          "artifact_contracts": [
            { "id": "diagnostics_v1", "title": "Diagnostics", "purpose": "Diagnostics." }
          ],
          "feature_bundles": []
        }
        """);
        File.WriteAllText(Path.Combine(root, "artifact_contracts.json"), """
        {
          "schema_version": "0.1",
          "contracts": [
            { "id": "game_profile_v1", "title": "Game Profile", "purpose": "Profile.", "validation_levels": ["validation.level_0_json_shape"] },
            { "id": "generator_plan_v1", "title": "Generator Plan", "purpose": "Plan.", "validation_levels": ["validation.level_0_json_shape"] },
            { "id": "diagnostics_v1", "title": "Diagnostics", "purpose": "Diagnostics.", "validation_levels": ["validation.level_0_json_shape"] }
          ]
        }
        """);
    }

    private static string FindCurrentAtlasRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "generator-library", "atlas");
            if (File.Exists(Path.Combine(candidate, "capability_atlas.json")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("generator-library/atlas was not found.");
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
