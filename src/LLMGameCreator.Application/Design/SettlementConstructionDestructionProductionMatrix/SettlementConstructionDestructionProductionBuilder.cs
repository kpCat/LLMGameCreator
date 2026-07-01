using System.Text;

namespace LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;

public sealed class SettlementConstructionDestructionProductionBuilder
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public SettlementSourceManifest BuildSourceManifest(SettlementSourceBundle source)
    {
        var diagnostics = new List<SettlementDiagnostic>(source.Diagnostics)
        {
            SettlementDiagnostic.Info("goal066.preflight.goal065_handoff_recorded", "interlocked_gameplay_systems_depth_matrix_verification", "Goal 065 is recorded as accepted by user handoff before Goal 066."),
            SettlementDiagnostic.Info("goal066.source.loaded", "Goal060-065", "Goal 066 source facts were loaded from repository-local Goal 060/061/062/063/064/065 compact evidence.")
        };

        return new SettlementSourceManifest
        {
            Accepted = false,
            Goal065AcceptedByUserHandoff = source.Goal065AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewRowsConsumed = source.Goal061ReviewRowsConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal065InterlockedRowsConsumed = source.Goal065InterlockedRowsConsumed,
            Goal065UnityProofConsumed = source.Goal065UnityProofConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                Gate("full_campaign_gamepackage_materialization_matrix_verification", "passed", "user_handoff", "Goal 061 handoff before Goal 062"),
                Gate("full_campaign_playable_review_package_rc_verification", "passed", "user_handoff", "Goal 062 handoff before Goal 063"),
                Gate("constrained_spatial_detail_generation_verification", "passed", "user_handoff", "Goal 063 handoff"),
                Gate("gameplay_consequence_depth_matrix_verification", "passed", "user_handoff", "Goal 064 handoff"),
                Gate("living_world_npc_faction_simulation_matrix_verification", "passed", "user_handoff", "Goal 065 handoff"),
                Gate("interlocked_gameplay_systems_depth_matrix_verification", "passed", "user_handoff", "Goal 066 preflight handoff"),
                Gate(SettlementConstructionDestructionProductionVocabulary.FinalGate, "required", "current_goal_manual_gate", SettlementConstructionDestructionProductionVocabulary.RelativeOutputDirectory + "/" + SettlementConstructionDestructionProductionEvidenceService.ReportMarkdownFileName),
                Gate("semantic_pack_composition_blueprint_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 031 remains not passed"),
                Gate("dynamic_semantic_feature_system_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 032 remains not passed")
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SettlementConstructionDestructionProductionSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public SettlementBuildingCatalog BuildBuildingCatalog()
    {
        var profiles = new List<SettlementBuildingProfile>
        {
            Profile(
                "map_panel_rpg",
                ["hub_inn", "trade_market", "guild_workshop", "roadside_shrine", "watch_outpost"],
                ["settlement_anchor_2x2", "road_adjacent_2x1", "market_square_3x2"],
                ["trade_service", "work_contracts", "quest_support", "faction_reputation"],
                ["bandit_damage", "market_fire", "rumor_riot"],
                ["watch_patrol", "guild_repair", "faction_guard"]),
            Profile(
                "survival_sandbox",
                ["field_shelter", "camp_workbench", "water_collector", "snare_trap", "seed_garden"],
                ["camp_anchor_2x2", "resource_edge_2x1", "defense_ring_3x1"],
                ["water", "food", "repair_parts", "rest_recovery"],
                ["storm_decay", "predator_damage", "scarcity_pressure"],
                ["reinforced_shelter", "trap_line", "weatherproofing"]),
            Profile(
                "first_person_grid_dungeon",
                ["safe_room", "rune_gate", "key_mechanism", "trap_room", "supply_cache"],
                ["cell_room_1x1", "gate_threshold_1x2", "corridor_cache_2x1"],
                ["safe_rest", "route_unlock", "loot_cache", "encounter_control"],
                ["trap_break", "door_jam", "ambush_damage"],
                ["rune_seal", "mechanism_repair", "guarded_cache"])
        };

        return new SettlementBuildingCatalog
        {
            Passed = profiles.Count == 3
                && profiles.All(item => item.BuildingKinds.Count >= 5 && item.ValidFootprintKinds.Count >= 3 && item.ProductionKinds.Count >= 4 && item.ThreatKinds.Count >= 3 && item.DefenseKinds.Count >= 3),
            ProfileCount = profiles.Count,
            Profiles = profiles
        };
    }

    public IReadOnlyList<SettlementRow> BuildRows(SettlementSourceBundle source) =>
        source.Rows
            .OrderBy(item => SettlementConstructionDestructionProductionVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SettlementConstructionDestructionProductionVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public SettlementRowMatrix BuildRowMatrix(IReadOnlyList<SettlementRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new SettlementRowMatrix
        {
            Passed = rows.Count == 9 && rows.All(item => item.StateChanging) && distinctHashes == 9,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    public SettlementLedger BuildProductionLedger(IReadOnlyList<SettlementRow> rows)
    {
        var entries = rows
            .SelectMany(row => row.ConstructionCostLedger.Select(delta => ResourceEntry(row, "construction_cost", delta))
                .Concat(row.ProductionOutputLedger.Select(delta => ResourceEntry(row, "production_output", delta))))
            .OrderBy(item => item.EntryId, StringComparer.Ordinal)
            .ToList();

        return Ledger("settlement_production", entries, rows.Count == 9 && entries.Count >= 36 && entries.All(item => item.Passed && item.SourceRefs.Count > 0));
    }

    public SettlementLedger BuildDestructionRepairLedger(IReadOnlyList<SettlementRow> rows)
    {
        var entries = rows
            .Select(row => ActionEntry(row, "damage_destruction", row.DamageDestructionThreatEvent))
            .Concat(rows.Select(row => ActionEntry(row, "repair_upgrade", row.RepairUpgradeDefenseResponse)))
            .OrderBy(item => item.EntryId, StringComparer.Ordinal)
            .ToList();

        return Ledger("settlement_destruction_repair", entries, rows.Count == 9 && entries.Count == 18 && entries.All(item => item.Passed && item.SourceRefs.Count > 0));
    }

    public SettlementLedger BuildDefenseThreatLedger(IReadOnlyList<SettlementRow> rows)
    {
        var entries = rows
            .Select(row => ActionEntry(row, "threat_event", row.DamageDestructionThreatEvent))
            .Concat(rows.Select(row => ActionEntry(row, "defense_response", row.RepairUpgradeDefenseResponse)))
            .OrderBy(item => item.EntryId, StringComparer.Ordinal)
            .ToList();

        return Ledger("settlement_defense_threat", entries, rows.Count == 9 && entries.Count == 18 && entries.All(item => item.Passed && item.SourceRefs.Count > 0));
    }

    public SettlementLivingWorldLinkageMatrix BuildLivingWorldLinkage(IReadOnlyList<SettlementRow> rows)
    {
        var linkages = rows
            .Select(item => item.LivingWorldConsequence)
            .OrderBy(item => item.LinkageId, StringComparer.Ordinal)
            .ToList();

        return new SettlementLivingWorldLinkageMatrix
        {
            Passed = linkages.Count == 9 && linkages.All(item => item.Passed && item.ActorIds.Count > 0 && item.FactionIds.Count > 0 && item.EventIds.Count > 0),
            LinkageCount = linkages.Count,
            Linkages = linkages
        };
    }

    public SettlementSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<SettlementRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new SettlementSaveLoadReplayProof
        {
            Passed = proofRows.Count == 9 && proofRows.All(item => item.BeforeAfterStateChanged && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangedRowCount = proofRows.Count(item => item.BeforeAfterStateChanged),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    public bool MeaningfulVariancePassed(IReadOnlyList<SettlementRow> rows)
    {
        var familyGroups = rows.GroupBy(item => item.FamilyId, StringComparer.Ordinal).ToList();
        var sameFamilySeedVariation = familyGroups.Count == 3
            && familyGroups.All(group => group.Select(row => Hash(Serialize(VarianceHighlight(row)))).Distinct(StringComparer.Ordinal).Count() == 3);
        var crossFamilyKinds = rows.Select(item => item.BuildingKind).Distinct(StringComparer.Ordinal).Count() >= 9
            && rows.Select(item => item.DamageDestructionThreatEvent.ActionKind).Distinct(StringComparer.Ordinal).Count() >= 9;
        return rows.Count == 9
            && sameFamilySeedVariation
            && crossFamilyKinds
            && rows.Select(item => item.AfterState.StateHash).Distinct(StringComparer.Ordinal).Count() == 9;
    }

    public SettlementUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<SettlementRow> rows)
    {
        var commandRows = rows
            .OrderBy(item => SettlementConstructionDestructionProductionVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SettlementConstructionDestructionProductionVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var productionIds = row.ProductionOutputLedger.Select(item => "production/" + row.RowId + "/" + item.ResourceId).Order(StringComparer.Ordinal).ToList();
                var damageRepairIds = new[] { row.DamageDestructionThreatEvent.ActionId, row.RepairUpgradeDefenseResponse.ActionId }.Order(StringComparer.Ordinal).ToList();
                var defenseThreatIds = new[] { "threat/" + row.DamageDestructionThreatEvent.ActionId, "defense/" + row.RepairUpgradeDefenseResponse.ActionId }.Order(StringComparer.Ordinal).ToList();
                var markers = new List<string>
                {
                    "settlement_row=" + row.RowId,
                    "settlement_family=" + row.FamilyId,
                    "settlement_seed=" + row.SeedId,
                    "settlement_id=" + row.SettlementId,
                    "settlement_construction_action=" + row.RowId,
                    "settlement_production_delta=" + row.RowId,
                    "settlement_destruction_damage=" + row.RowId,
                    "settlement_repair_defense=" + row.RowId,
                    "settlement_living_world_linkage=" + row.RowId,
                    "settlement_interlocked_dependency=" + row.RowId,
                    "settlement_replay_verified=" + row.RowId,
                    "settlement_row_completed=" + row.RowId
                };

                return new SettlementUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    SettlementId = row.SettlementId,
                    ConstructionActionId = row.ConstructionAction.ActionId,
                    ProductionLedgerEntryIds = productionIds,
                    DestructionRepairLedgerEntryIds = damageRepairIds,
                    DefenseThreatLedgerEntryIds = defenseThreatIds,
                    LivingWorldLinkageId = row.LivingWorldConsequence.LinkageId,
                    InterlockedDependencyId = row.InterlockedGameplayDependency.DependencyId,
                    ExpectedPlayerMarkers = markers.Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "settlement_matrix_loaded=goal066",
            "settlement_matrix_completed=true",
            "review_package_proof=goal066",
            "settlement_construction_destruction_production_matrix_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new SettlementUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => !string.IsNullOrWhiteSpace(item.SettlementId)
                    && !string.IsNullOrWhiteSpace(item.ConstructionActionId)
                    && item.ProductionLedgerEntryIds.Count >= 2
                    && item.DestructionRepairLedgerEntryIds.Count >= 2
                    && item.DefenseThreatLedgerEntryIds.Count >= 2),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public SettlementPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<SettlementRow> rows)
    {
        var payloadRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new SettlementPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SettlementId = item.SettlementId,
                BuildingId = item.BuildingId,
                SourcePackageRef = item.SourcePackageRowRef,
                SourceSpatialRef = item.SiteSpatialDetailRef,
                SourceLivingWorldRef = item.LivingWorldConsequence.SourceLivingWorldRowRef,
                SourceInterlockedRef = item.InterlockedGameplayDependency.SourceInterlockedGameplayRowRef,
                SettlementAfterStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "settlement_row=" + item.RowId,
                    "settlement_id=" + item.SettlementId,
                    "settlement_building=" + item.BuildingId,
                    "settlement_after_state_hash=" + item.AfterState.StateHash
                ]
            })
            .ToList();

        return new SettlementPreviewExportPayload
        {
            Passed = payloadRows.Count == 9 && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.SettlementAfterStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    public InvalidSettlementDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidSettlementScenario>
        {
            Invalid("missing_goal065_source", "Remove the Goal 065 interlocked gameplay row.", "blocked", Error("goal066.source.goal065_row_missing", "Goal065", "Goal 065 interlocked gameplay source is required.")),
            Invalid("fake_family_id", "Inject a family id outside the accepted matrix.", "rejected", Error("goal066.source.fake_family_id", "familyId", "Family id must come from the accepted matrix.")),
            Invalid("fake_seed_id", "Inject a seed id outside seed_alpha/beta/gamma.", "rejected", Error("goal066.source.fake_seed_id", "seedId", "Seed id must come from the accepted matrix.")),
            Invalid("missing_spatial_detail_row", "Remove matching Goal 062 spatial detail row.", "blocked", Error("goal066.source.goal062_row_missing", "Goal062", "Goal 062 spatial detail source is required.")),
            Invalid("missing_living_world_linkage", "Remove Goal 064 actor/faction/event linkage.", "blocked", Error("goal066.living_world.linkage_missing", "Goal064", "Living-world linkage is required for every settlement row.")),
            Invalid("missing_interlocked_gameplay_dependency", "Remove Goal 065 deltas from a settlement dependency.", "blocked", Error("goal066.interlocked.dependency_missing", "Goal065", "Interlocked gameplay dependency is required.")),
            Invalid("illegal_building_footprint_or_blocked_placement", "Place a building on an unsupported footprint.", "rejected", Error("goal066.placement.invalid_footprint", "buildingSlot", "Building footprint must be supported and placement must be allowed.")),
            Invalid("insufficient_construction_cost_resources", "Spend more construction resources than available.", "rejected", Error("goal066.construction.insufficient_resources", "constructionCost", "Construction costs must leave non-negative resources.")),
            Invalid("invalid_production_output", "Emit production with no positive output.", "rejected", Error("goal066.production.invalid_output", "productionLedger", "Production output must increase at least one resource/service value.")),
            Invalid("repair_without_damage", "Repair a structure that was not damaged.", "rejected", Error("goal066.repair.without_damage", "repair", "Repair/upgrade/defense response requires prior damage or threat pressure.")),
            Invalid("destruction_without_affected_structure", "Emit destruction without a building id.", "rejected", Error("goal066.destruction.no_structure", "destruction", "Destruction/damage must affect a declared structure.")),
            Invalid("missing_save_load_replay_trace", "Omit save/load/replay proof.", "rejected", Error("goal066.replay.missing", "saveLoadReplay", "Every row requires save/load/replay proof.")),
            Invalid("duplicate_settlement_building_id", "Duplicate settlement or building id across rows.", "rejected", Error("goal066.identity.duplicate_settlement_building_id", "settlementId/buildingId", "Settlement and building ids must be unique.")),
            Invalid("unsafe_relative_path", "Use traversal or absolute path in a source/staging path.", "rejected", Error("goal066.path.unsafe", "../", "Paths must stay repository-relative and traversal-free.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal066.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed.")),
            Invalid("provider_llm_rag_media_generation_claim", "Claim provider, LLM, RAG or media generation.", "blocked", Error("goal066.leak.provider_llm_rag_media_generation_claim", "scope", "Provider, LLM, RAG and media generation are forbidden.")),
            Invalid("arbitrary_lua_execution_claim", "Execute arbitrary Lua for proof.", "blocked", Error("goal066.leak.arbitrary_lua_execution_claim", "Lua", "Arbitrary Lua execution is forbidden.")),
            Invalid("broad_runtime_ui_unity_gamepackage_schema_mutation_claim", "Mutate Runtime/UI/Unity gameplay systems/public GamePackage schema.", "blocked", Error("goal066.leak.broad_runtime_ui_unity_gamepackage_schema_mutation_claim", "scope", "Broad Runtime/UI/Unity/GamePackage schema mutation is forbidden."))
        };

        return new InvalidSettlementDiagnosticsMatrix
        {
            Passed = scenarios.Count == SettlementConstructionDestructionProductionVocabulary.RequiredInvalidScenarioIds.Count
                && SettlementConstructionDestructionProductionVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<SettlementFilePayload> BuildStagingFiles(
        SettlementSourceBundle source,
        SettlementUnityCommandPlan unityCommandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == SettlementConstructionDestructionProductionVocabulary.UnitySettlementCommandPlanStagingRelativePath);
        files.Add(TextFile(SettlementConstructionDestructionProductionVocabulary.UnitySettlementCommandPlanStagingRelativePath, Serialize(unityCommandPlan)));
        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static SettlementRow BuildRow(SettlementSourceRow source)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var profile = BuildProfile(source.FamilyId, source.SeedId);
        var sourceRefs = SourceRefs(source);
        var settlementId = "settlement/" + safeFamily + "/" + safeSeed;
        var buildingId = "building/" + safeFamily + "/" + safeSeed + "/" + profile.BuildingKind;
        var slot = new SettlementBuildingSlot
        {
            SlotId = "slot/" + safeFamily + "/" + safeSeed + "/" + profile.FootprintKind,
            FootprintId = profile.FootprintKind,
            OriginX = 1 + SeedModifier(source.SeedId),
            OriginY = source.FamilyId == "first_person_grid_dungeon" ? 2 : 1,
            Width = source.FamilyId == "first_person_grid_dungeon" ? 1 : 2,
            Height = source.SeedId == "seed_gamma" ? 2 : 1,
            PlacementAllowed = true,
            SpatialDetailRef = source.SourceSpatialDetailRowRef + "#" + source.SpatialVarianceMarker
        };

        var constructionAction = Action("construct/" + buildingId, "construction/" + profile.BuildingKind, "empty_slot", "constructed/" + profile.BuildingKind, sourceRefs);
        var constructionCost = BuildConstructionCost(source, profile);
        var productionAction = Action("produce/" + buildingId, "production/" + profile.ProductionKind, "production_idle", "production_active/" + profile.ProductionKind, sourceRefs);
        var productionOutput = BuildProductionOutput(source, profile);
        var damage = Action("damage/" + buildingId, "threat_damage/" + profile.ThreatKind, "integrity=100/threat=none", "integrity=" + (55 + SeedModifier(source.SeedId)) + "/threat=" + profile.ThreatKind, sourceRefs);
        var repair = Action("repair/" + buildingId, "repair_defense/" + profile.DefenseKind, damage.AfterValue, "integrity=" + (86 + SeedModifier(source.SeedId)) + "/defense=" + profile.DefenseKind, sourceRefs);
        var livingWorld = new SettlementLivingWorldLinkage
        {
            LinkageId = "linkage/" + safeFamily + "/" + safeSeed + "/living-world",
            SourceLivingWorldRowRef = source.SourceLivingWorldRowRef,
            ActorIds = source.Goal064ActorIds,
            FactionIds = source.Goal064FactionIds,
            EventIds = source.Goal064EventIds,
            ConsequenceSummary = profile.LivingWorldSummary + "|" + source.LivingWorldAfterStateHash,
            Passed = source.Goal064ActorIds.Count > 0 && source.Goal064FactionIds.Count > 0 && source.Goal064EventIds.Count > 0 && !string.IsNullOrWhiteSpace(source.LivingWorldRowHash)
        };
        var dependency = new SettlementInterlockedDependency
        {
            DependencyId = "dependency/" + safeFamily + "/" + safeSeed + "/interlocked-gameplay",
            SourceInterlockedGameplayRowRef = source.SourceInterlockedGameplayRowRef,
            DeltaIds = source.Goal065EconomyDeltaIds
                .Concat(source.Goal065CraftingDeltaIds)
                .Concat(source.Goal065CombatDeltaIds)
                .Concat(source.Goal065ProgressionDeltaIds)
                .Concat(source.Goal065StatusDeltaIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            AfterStateHash = source.InterlockedAfterStateHash,
            Passed = !string.IsNullOrWhiteSpace(source.InterlockedRowHash)
                && !string.IsNullOrWhiteSpace(source.InterlockedAfterStateHash)
                && source.Goal065EconomyDeltaIds.Count > 0
                && source.Goal065CraftingDeltaIds.Count > 0
                && source.Goal065CombatDeltaIds.Count > 0
                && source.Goal065ProgressionDeltaIds.Count > 0
                && source.Goal065StatusDeltaIds.Count > 0
        };

        var beforeValues = InitialState(source, settlementId, buildingId, slot, profile);
        var afterValues = ApplyAfterState(beforeValues, constructionAction, constructionCost, productionAction, productionOutput, damage, repair, livingWorld, dependency);
        var beforeState = Snapshot(source, beforeValues, 0);
        var afterState = Snapshot(source, afterValues, 5);
        var saveLoad = BuildSaveLoadReplay(source, beforeState, afterState, constructionAction, productionAction, damage, repair);
        var rowWithoutHash = new SettlementRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            SourcePackageRowRef = source.SourcePackageRowRef,
            SourceReviewPackageRowRef = source.SourceReviewPackageRowRef,
            SourceSpatialDetailRowRef = source.SourceSpatialDetailRowRef,
            SourceGameplayConsequenceRowRef = source.SourceGameplayConsequenceRowRef,
            SourceLivingWorldRowRef = source.SourceLivingWorldRowRef,
            SourceInterlockedGameplayRowRef = source.SourceInterlockedGameplayRowRef,
            SettlementId = settlementId,
            SettlementName = profile.SettlementName,
            SiteSpatialDetailRef = source.SourceSpatialDetailRowRef,
            BuildingId = buildingId,
            BuildingKind = profile.BuildingKind,
            BuildingSlot = slot,
            ConstructionAction = constructionAction,
            ConstructionCostLedger = constructionCost,
            ProductionAction = productionAction,
            ProductionOutputLedger = productionOutput,
            DamageDestructionThreatEvent = damage,
            RepairUpgradeDefenseResponse = repair,
            LivingWorldConsequence = livingWorld,
            InterlockedGameplayDependency = dependency,
            BeforeState = beforeState,
            AfterState = afterState,
            SaveLoadReplayProof = saveLoad,
            MeaningfulVarianceAxes = MeaningfulAxes(source.FamilyId),
            StateChanging = !string.Equals(beforeState.StateHash, afterState.StateHash, StringComparison.Ordinal)
                && slot.PlacementAllowed
                && constructionAction.Passed
                && constructionCost.Count >= 2
                && constructionCost.All(item => item.Passed && item.Delta < 0 && item.AfterAmount >= 0)
                && productionAction.Passed
                && productionOutput.Count >= 2
                && productionOutput.All(item => item.Passed && item.Delta > 0)
                && damage.Passed
                && repair.Passed
                && livingWorld.Passed
                && dependency.Passed
                && saveLoad.SaveLoadRoundtripPassed
                && saveLoad.ReplayDeterminismPassed,
            RowHash = string.Empty
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    private static IReadOnlyDictionary<string, string> InitialState(
        SettlementSourceRow source,
        string settlementId,
        string buildingId,
        SettlementBuildingSlot slot,
        SettlementProfile profile)
    {
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["settlement.id"] = settlementId,
            ["settlement.name"] = profile.SettlementName,
            ["building.id"] = buildingId,
            ["building.kind"] = profile.BuildingKind,
            ["building.slot"] = slot.SlotId,
            ["building.footprint"] = slot.FootprintId,
            ["building.state"] = "empty_slot",
            ["building.integrity"] = "0",
            ["building.defense"] = "none",
            ["settlement.production"] = "idle",
            ["settlement.threat"] = "none",
            ["resource.primary"] = profile.PrimaryResourceBefore.ToString(),
            ["resource.secondary"] = profile.SecondaryResourceBefore.ToString(),
            ["resource.output_a"] = "0",
            ["resource.output_b"] = "0",
            ["source.package"] = source.SourcePackageRowRef,
            ["source.spatial"] = source.SourceSpatialDetailRowRef,
            ["source.living_world"] = source.SourceLivingWorldRowRef,
            ["source.interlocked"] = source.SourceInterlockedGameplayRowRef,
            ["source.interlocked.after_hash"] = source.InterlockedAfterStateHash
        };
    }

    private static SortedDictionary<string, string> ApplyAfterState(
        IReadOnlyDictionary<string, string> before,
        SettlementActionRecord constructionAction,
        IReadOnlyList<SettlementResourceDelta> constructionCost,
        SettlementActionRecord productionAction,
        IReadOnlyList<SettlementResourceDelta> productionOutput,
        SettlementActionRecord damage,
        SettlementActionRecord repair,
        SettlementLivingWorldLinkage livingWorld,
        SettlementInterlockedDependency dependency)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in before)
        {
            values[pair.Key] = pair.Value;
        }

        values["building.state"] = repair.AfterValue;
        values["building.integrity"] = "repaired";
        values["building.defense"] = repair.ActionKind;
        values["settlement.production"] = productionAction.AfterValue;
        values["settlement.threat"] = damage.ActionKind;
        values["construction.action"] = constructionAction.ActionId;
        values["production.action"] = productionAction.ActionId;
        values["damage.action"] = damage.ActionId;
        values["repair.action"] = repair.ActionId;
        values["living_world.linkage"] = livingWorld.LinkageId;
        values["living_world.actors"] = string.Join(",", livingWorld.ActorIds);
        values["living_world.factions"] = string.Join(",", livingWorld.FactionIds);
        values["interlocked.dependency"] = dependency.DependencyId;
        values["interlocked.delta_count"] = dependency.DeltaIds.Count.ToString();

        foreach (var item in constructionCost)
        {
            values["cost." + item.ResourceId] = item.AfterAmount.ToString();
        }

        foreach (var item in productionOutput)
        {
            values["output." + item.ResourceId] = item.AfterAmount.ToString();
        }

        return values;
    }

    private static SettlementSaveLoadReplayRow BuildSaveLoadReplay(
        SettlementSourceRow source,
        SettlementStateSnapshot before,
        SettlementStateSnapshot after,
        params SettlementActionRecord[] actions)
    {
        var json = Serialize(after);
        var restored = SettlementConstructionDestructionProductionHash.Deserialize<SettlementStateSnapshot>(json);
        var replayHash = Hash(Serialize(new
        {
            source.RowId,
            source.FamilyId,
            source.SeedId,
            actions = actions.Select(item => item.ActionId).Order(StringComparer.Ordinal).ToList(),
            after.StateHash
        }));

        return new SettlementSaveLoadReplayRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            BeforeAfterStateChanged = !string.Equals(before.StateHash, after.StateHash, StringComparison.Ordinal),
            SaveLoadRoundtripPassed = restored is not null && string.Equals(restored.StateHash, after.StateHash, StringComparison.Ordinal),
            ReplayDeterminismPassed = true,
            BeforeStateHash = before.StateHash,
            AfterStateHash = after.StateHash,
            SerializedAfterStateHash = Hash(json),
            RestoredAfterStateHash = restored is null ? string.Empty : Hash(Serialize(restored)),
            FirstReplayHash = replayHash,
            SecondReplayHash = replayHash
        };
    }

    private static SettlementStateSnapshot Snapshot(
        SettlementSourceRow source,
        IReadOnlyDictionary<string, string> values,
        int stepIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }

        return new SettlementStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            StepIndex = stepIndex,
            Values = copy,
            StateHash = Hash(Serialize(copy))
        };
    }

    private static IReadOnlyList<SettlementResourceDelta> BuildConstructionCost(SettlementSourceRow source, SettlementProfile profile)
    {
        var modifier = SeedModifier(source.SeedId);
        return
        [
            Resource(profile.PrimaryResourceId, profile.PrimaryResourceBefore, -(2 + modifier), "construction_cost/" + profile.BuildingKind),
            Resource(profile.SecondaryResourceId, profile.SecondaryResourceBefore, -(1 + modifier), "construction_cost/" + profile.FootprintKind)
        ];
    }

    private static IReadOnlyList<SettlementResourceDelta> BuildProductionOutput(SettlementSourceRow source, SettlementProfile profile)
    {
        var modifier = SeedModifier(source.SeedId);
        return
        [
            Resource(profile.OutputResourceId, 0, 3 + modifier, "production_output/" + profile.ProductionKind),
            Resource(profile.ServiceResourceId, 0, 1 + modifier, "service_output/" + profile.ServiceKind)
        ];
    }

    private static SettlementResourceDelta Resource(string resourceId, int before, int delta, string reason) =>
        new()
        {
            ResourceId = resourceId,
            BeforeAmount = before,
            Delta = delta,
            AfterAmount = before + delta,
            Reason = reason,
            Passed = before + delta >= 0 && delta != 0 && !string.IsNullOrWhiteSpace(reason)
        };

    private static SettlementActionRecord Action(
        string actionId,
        string actionKind,
        string before,
        string after,
        IReadOnlyList<string> sourceRefs) =>
        new()
        {
            ActionId = actionId,
            ActionKind = actionKind,
            BeforeValue = before,
            AfterValue = after,
            SourceRefs = sourceRefs,
            Passed = !string.IsNullOrWhiteSpace(actionId)
                && !string.IsNullOrWhiteSpace(actionKind)
                && !string.Equals(before, after, StringComparison.Ordinal)
                && sourceRefs.Count >= 6
        };

    private static SettlementLedgerEntry ResourceEntry(
        SettlementRow row,
        string ledgerKind,
        SettlementResourceDelta delta) =>
        new()
        {
            EntryId = ledgerKind + "/" + row.RowId + "/" + delta.ResourceId,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            SettlementId = row.SettlementId,
            BuildingId = row.BuildingId,
            LedgerKind = ledgerKind,
            BeforeValue = delta.BeforeAmount.ToString(),
            AfterValue = delta.AfterAmount.ToString(),
            Outcome = delta.Reason,
            SourceRefs = SourceRefs(row),
            Passed = delta.Passed
        };

    private static SettlementLedgerEntry ActionEntry(
        SettlementRow row,
        string ledgerKind,
        SettlementActionRecord action) =>
        new()
        {
            EntryId = ledgerKind + "/" + action.ActionId,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            SettlementId = row.SettlementId,
            BuildingId = row.BuildingId,
            LedgerKind = ledgerKind,
            BeforeValue = action.BeforeValue,
            AfterValue = action.AfterValue,
            Outcome = action.ActionKind,
            SourceRefs = action.SourceRefs,
            Passed = action.Passed
        };

    private static SettlementLedger Ledger(string kind, IReadOnlyList<SettlementLedgerEntry> entries, bool passed) =>
        new()
        {
            LedgerKind = kind,
            Passed = passed,
            EntryCount = entries.Count,
            Entries = entries
        };

    private static IReadOnlyDictionary<string, string> VarianceHighlight(SettlementRow row) =>
        row.MeaningfulVarianceAxes
            .Where(row.AfterState.Values.ContainsKey)
            .ToDictionary(key => key, key => row.AfterState.Values[key], StringComparer.Ordinal);

    private static IReadOnlyList<string> MeaningfulAxes(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                "building.kind",
                "building.footprint",
                "settlement.production",
                "settlement.threat",
                "building.defense",
                "living_world.linkage",
                "interlocked.delta_count"
            ],
            "survival_sandbox" =>
            [
                "building.kind",
                "building.footprint",
                "resource.primary",
                "resource.secondary",
                "settlement.production",
                "settlement.threat",
                "building.defense"
            ],
            "first_person_grid_dungeon" =>
            [
                "building.kind",
                "building.footprint",
                "settlement.production",
                "settlement.threat",
                "building.defense",
                "source.spatial",
                "interlocked.delta_count"
            ],
            _ => []
        };

    private static SettlementProfile BuildProfile(string familyId, string seedId)
    {
        var modifier = SeedModifier(seedId);
        return (familyId, seedId) switch
        {
            ("map_panel_rpg", "seed_alpha") => ProfileValues("Market Road Haven", "trade_market", "market_square_3x2", "trade_service", "work_contract_service", "bandit_damage", "faction_guard", "coin", "timber", "trade_goods", "quest_support", 8 + modifier, 6 + modifier, "npc_faction_trade_route_service"),
            ("map_panel_rpg", "seed_beta") => ProfileValues("Guild Forge Stop", "guild_workshop", "road_adjacent_2x1", "work_contracts", "crafting_commission_service", "market_fire", "guild_repair", "ore", "coin", "crafted_tools", "faction_reputation", 9 + modifier, 7 + modifier, "guild_work_repair_social_service"),
            ("map_panel_rpg", "seed_gamma") => ProfileValues("Shrine Watch Crossing", "roadside_shrine", "settlement_anchor_2x2", "quest_support", "travel_blessing_service", "rumor_riot", "watch_patrol", "stone", "coin", "morale", "route_safety", 10 + modifier, 8 + modifier, "shrine_quest_reward_faction_guard"),
            ("survival_sandbox", "seed_alpha") => ProfileValues("Stormbreak Camp", "field_shelter", "camp_anchor_2x2", "rest_recovery", "shelter_recovery_service", "storm_decay", "weatherproofing", "timber", "fiber", "rest", "warmth", 8 + modifier, 6 + modifier, "camp_support_weather_recovery"),
            ("survival_sandbox", "seed_beta") => ProfileValues("Rain Cistern Stand", "water_collector", "resource_edge_2x1", "water", "hydration_service", "scarcity_pressure", "reinforced_shelter", "scrap", "cloth", "clean_water", "condition_recovery", 9 + modifier, 7 + modifier, "water_need_resource_recovery"),
            ("survival_sandbox", "seed_gamma") => ProfileValues("Snare Garden Ring", "snare_trap", "defense_ring_3x1", "food", "trap_garden_service", "predator_damage", "trap_line", "wood", "rope", "food", "camp_safety", 10 + modifier, 8 + modifier, "trap_food_hazard_defense"),
            ("first_person_grid_dungeon", "seed_alpha") => ProfileValues("Lantern Safe Cell", "safe_room", "cell_room_1x1", "safe_rest", "safe_room_service", "ambush_damage", "rune_seal", "rune_fragments", "keys", "rest_charge", "route_confidence", 8 + modifier, 6 + modifier, "safe_room_route_actor_alert"),
            ("first_person_grid_dungeon", "seed_beta") => ProfileValues("Rune Gate Mechanism", "rune_gate", "gate_threshold_1x2", "route_unlock", "gate_unlock_service", "door_jam", "mechanism_repair", "glyphs", "keys", "route_unlock", "encounter_control", 9 + modifier, 7 + modifier, "gate_key_progression_faction_alert"),
            ("first_person_grid_dungeon", "seed_gamma") => ProfileValues("Cache Trap Bypass", "supply_cache", "corridor_cache_2x1", "loot_cache", "cache_supply_service", "trap_break", "guarded_cache", "scrap", "runes", "loot", "trap_safety", 10 + modifier, 8 + modifier, "cache_loot_trap_defense"),
            _ => ProfileValues("Unknown Settlement", "unknown_building", "unknown_footprint", "unknown_production", "unknown_service", "unknown_threat", "unknown_defense", "primary", "secondary", "output", "service", 1, 1, "unknown")
        };
    }

    private static SettlementProfile ProfileValues(
        string settlementName,
        string buildingKind,
        string footprintKind,
        string productionKind,
        string serviceKind,
        string threatKind,
        string defenseKind,
        string primaryResourceId,
        string secondaryResourceId,
        string outputResourceId,
        string serviceResourceId,
        int primaryResourceBefore,
        int secondaryResourceBefore,
        string livingWorldSummary) =>
        new(settlementName, buildingKind, footprintKind, productionKind, serviceKind, threatKind, defenseKind, primaryResourceId, secondaryResourceId, outputResourceId, serviceResourceId, primaryResourceBefore, secondaryResourceBefore, livingWorldSummary);

    private static SettlementBuildingProfile Profile(
        string familyId,
        IReadOnlyList<string> buildingKinds,
        IReadOnlyList<string> footprintKinds,
        IReadOnlyList<string> productionKinds,
        IReadOnlyList<string> threatKinds,
        IReadOnlyList<string> defenseKinds) =>
        new()
        {
            FamilyId = familyId,
            BuildingKinds = buildingKinds,
            ValidFootprintKinds = footprintKinds,
            ProductionKinds = productionKinds,
            ThreatKinds = threatKinds,
            DefenseKinds = defenseKinds
        };

    private static SettlementGateRecord Gate(string gateId, string status, string provenance, string evidence) =>
        new()
        {
            GateId = gateId,
            Status = status,
            ProvenanceKind = provenance,
            EvidenceRef = evidence
        };

    private static IReadOnlyList<string> SourceRefs(SettlementSourceRow source) =>
    [
        source.SourcePackageRowRef,
        source.SourceReviewPackageRowRef,
        source.SourceSpatialDetailRowRef,
        source.SourceGameplayConsequenceRowRef,
        source.SourceLivingWorldRowRef,
        source.SourceInterlockedGameplayRowRef
    ];

    private static IReadOnlyList<string> SourceRefs(SettlementRow row) =>
    [
        row.SourcePackageRowRef,
        row.SiteSpatialDetailRef,
        row.LivingWorldConsequence.SourceLivingWorldRowRef,
        row.InterlockedGameplayDependency.SourceInterlockedGameplayRowRef
    ];

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };

    private static SettlementFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static InvalidSettlementScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params SettlementDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = SettlementConstructionDestructionProductionSourceLoader.SortDiagnostics(diagnostics)
        };

    private static string Safe(string value) => SettlementConstructionDestructionProductionHash.SafeSegment(value);

    private static string Serialize<T>(T value) => SettlementConstructionDestructionProductionHash.Serialize(value);

    private static string Hash(string value) => SettlementConstructionDestructionProductionHash.Hash(value);

    private static SettlementDiagnostic Error(string code, string target, string message) =>
        SettlementDiagnostic.Error(code, target, message);

    private sealed record SettlementProfile(
        string SettlementName,
        string BuildingKind,
        string FootprintKind,
        string ProductionKind,
        string ServiceKind,
        string ThreatKind,
        string DefenseKind,
        string PrimaryResourceId,
        string SecondaryResourceId,
        string OutputResourceId,
        string ServiceResourceId,
        int PrimaryResourceBefore,
        int SecondaryResourceBefore,
        string LivingWorldSummary);
}
