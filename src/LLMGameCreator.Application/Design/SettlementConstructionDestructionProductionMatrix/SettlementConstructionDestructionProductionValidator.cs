namespace LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;

public sealed class SettlementConstructionDestructionProductionValidator
{
    public IReadOnlyList<SettlementDiagnostic> ValidateSourceManifest(SettlementSourceManifest manifest)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal066.gate.self_pass.forbidden", "source-manifest", "Goal 066 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal065AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "interlocked_gameplay_systems_depth_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal066.preflight.goal065_handoff_missing", "source-manifest", "Goal 065 acceptance by user handoff is required before Goal 066."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == SettlementConstructionDestructionProductionVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal066.gate.required_missing", "source-manifest", "Goal 066 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal066.source.matrix_counts_invalid", "source-manifest", "Goal 066 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewRowsConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed
            || !manifest.Goal065InterlockedRowsConsumed)
        {
            diagnostics.Add(Error("goal066.source.chain_incomplete", "source-manifest", "Goal 066 must consume Goal 060/061/062/063/064/065 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateRows(
        SettlementBuildingCatalog catalog,
        SettlementRowMatrix matrix,
        SettlementPreviewExportPayload previewPayload,
        bool meaningfulVariancePassed)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        if (!catalog.Passed || catalog.ProfileCount != 3)
        {
            diagnostics.Add(Error("goal066.catalog.invalid", "settlement-building-catalog", "Building catalog must define all three settlement family profiles."));
        }

        if (!matrix.Passed || matrix.Accepted || matrix.RowCount != 9 || matrix.StateChangingRowCount != 9 || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal066.matrix.invalid", "settlement-construction-row-matrix", "Settlement row matrix must contain 9 produced-for-review state-changing rows with distinct hashes."));
        }

        if (matrix.Rows.Select(item => item.SettlementId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count
            || matrix.Rows.Select(item => item.BuildingId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal066.identity.duplicate_settlement_building_id", "settlementId/buildingId", "Settlement and building ids must be unique across rows."));
        }

        foreach (var familyId in SettlementConstructionDestructionProductionVocabulary.FamilyIds)
        {
            foreach (var seedId in SettlementConstructionDestructionProductionVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal066.matrix.row_missing", familyId + "/" + seedId, "Required settlement row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, catalog, diagnostics);
        }

        if (!meaningfulVariancePassed)
        {
            diagnostics.Add(Error("goal066.variance.invalid", "settlement-construction-row-matrix", "Variance must prove same-family seed and cross-family settlement differences beyond ids/hashes."));
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal066.preview.payload_invalid", "settlement-preview-export-payload", "Preview/export settlement payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateLedgers(
        SettlementLedger production,
        SettlementLedger destructionRepair,
        SettlementLedger defenseThreat,
        SettlementLivingWorldLinkageMatrix livingWorld)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        if (!production.Passed
            || production.Entries.Count(item => item.LedgerKind == "construction_cost") < 18
            || production.Entries.Count(item => item.LedgerKind == "production_output") < 18)
        {
            diagnostics.Add(Error("goal066.ledger.production_invalid", "settlement-production-ledger", "Construction cost and production output ledgers must cover every row."));
        }

        if (!destructionRepair.Passed
            || destructionRepair.Entries.Count(item => item.LedgerKind == "damage_destruction") != 9
            || destructionRepair.Entries.Count(item => item.LedgerKind == "repair_upgrade") != 9)
        {
            diagnostics.Add(Error("goal066.ledger.destruction_repair_invalid", "settlement-destruction-repair-ledger", "Damage/destruction and repair/upgrade ledgers must cover every row."));
        }

        if (!defenseThreat.Passed
            || defenseThreat.Entries.Count(item => item.LedgerKind == "threat_event") != 9
            || defenseThreat.Entries.Count(item => item.LedgerKind == "defense_response") != 9)
        {
            diagnostics.Add(Error("goal066.ledger.defense_threat_invalid", "settlement-defense-threat-ledger", "Threat and defense ledgers must cover every row."));
        }

        if (!livingWorld.Passed || livingWorld.LinkageCount != 9)
        {
            diagnostics.Add(Error("goal066.living_world.linkage_missing", "settlement-living-world-linkage", "Living-world linkage must cover all 9 settlement rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateReplay(SettlementSaveLoadReplayProof replay)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.StateChangedRowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal066.replay.audit_invalid", "settlement-save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal066.state.before_after_equal", row.RowId, "Before and after settlement state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SerializedAfterStateHash != row.RestoredAfterStateHash)
            {
                diagnostics.Add(Error("goal066.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve settlement after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal066.replay.mismatch", row.RowId, "Replay hashes must match for same settlement input."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateUnityCommandPlan(SettlementUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal066.unity.command_plan_invalid", "settlement-unity-command-plan", "Unity command plan must cover all 9 settlement rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal066.unity.marker_missing", marker, "Unity command plan is missing a required global settlement marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.SettlementId)
                || string.IsNullOrWhiteSpace(row.ConstructionActionId)
                || row.ProductionLedgerEntryIds.Count == 0
                || row.DestructionRepairLedgerEntryIds.Count == 0
                || row.DefenseThreatLedgerEntryIds.Count == 0
                || string.IsNullOrWhiteSpace(row.LivingWorldLinkageId)
                || string.IsNullOrWhiteSpace(row.InterlockedDependencyId))
            {
                diagnostics.Add(Error("goal066.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include settlement, construction, production, destruction/repair, defense, living-world and interlocked ids."));
            }

            foreach (var marker in RowMarkers(row.RowId, row.FamilyId, row.SeedId, row.SettlementId))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal066.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, family, seed, settlement, construction, production, damage, repair/defense, linkage, replay and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateUnityProof(
        SettlementUnityCommandPlan commandPlan,
        SettlementUnityProofSummary proof)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal066.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 066 marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorExecuted
                || !proof.PlayerExecuted
                || proof.UnityExitCode != 0
                || proof.PlayerExitCode != 0
                || proof.ProvenRowCount != 9
                || proof.MissingMarkers.Count != 0)
            {
                diagnostics.Add(Error("goal066.unity.proof_inconsistent", "settlement-unity-player-proof-summary", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal066.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<SettlementDiagnostic> ValidateInvalidMatrix(InvalidSettlementDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<SettlementDiagnostic>();
        foreach (var scenarioId in SettlementConstructionDestructionProductionVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal066.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal066.invalid.matrix_failed", "settlement-invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "settlement_matrix_loaded=goal066",
        "settlement_matrix_completed=true",
        "review_package_proof=goal066",
        "settlement_construction_destruction_production_matrix_verification=required"
    ];

    public static IReadOnlyList<SettlementDiagnostic> Sort(IEnumerable<SettlementDiagnostic> diagnostics) =>
        SettlementConstructionDestructionProductionSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateRow(
        SettlementRow row,
        SettlementBuildingCatalog catalog,
        List<SettlementDiagnostic> diagnostics)
    {
        var profile = catalog.Profiles.FirstOrDefault(item => item.FamilyId == row.FamilyId);
        if (profile is null)
        {
            diagnostics.Add(Error("goal066.family.unknown", row.RowId, "Unsupported family id."));
            return;
        }

        if (!profile.BuildingKinds.Contains(row.BuildingKind, StringComparer.Ordinal)
            || !profile.ValidFootprintKinds.Contains(row.BuildingSlot.FootprintId, StringComparer.Ordinal)
            || !row.BuildingSlot.PlacementAllowed)
        {
            diagnostics.Add(Error("goal066.placement.invalid_footprint", row.RowId, "Building footprint must be family-supported and placement must be allowed."));
        }

        if (!row.StateChanging || row.BeforeState.StateHash == row.AfterState.StateHash)
        {
            diagnostics.Add(Error("goal066.state.non_state_changing_row", row.RowId, "Every settlement row must change before/after state."));
        }

        if (!row.ConstructionAction.Passed || row.ConstructionCostLedger.Count < 2 || row.ConstructionCostLedger.Any(item => !item.Passed || item.Delta >= 0 || item.AfterAmount < 0))
        {
            diagnostics.Add(Error("goal066.construction.insufficient_resources", row.RowId, "Construction must spend available resources and leave non-negative amounts."));
        }

        if (!row.ProductionAction.Passed || row.ProductionOutputLedger.Count < 2 || row.ProductionOutputLedger.All(item => item.Delta <= 0))
        {
            diagnostics.Add(Error("goal066.production.invalid_output", row.RowId, "Production/service output must produce positive deltas."));
        }

        if (!row.DamageDestructionThreatEvent.Passed || string.IsNullOrWhiteSpace(row.BuildingId) || !row.DamageDestructionThreatEvent.AfterValue.Contains("threat=", StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal066.destruction.no_structure", row.RowId, "Damage/destruction must affect a declared structure and threat."));
        }

        if (!row.RepairUpgradeDefenseResponse.Passed
            || !row.RepairUpgradeDefenseResponse.BeforeValue.Contains("threat=", StringComparison.Ordinal)
            || !row.RepairUpgradeDefenseResponse.AfterValue.Contains("defense=", StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal066.repair.without_damage", row.RowId, "Repair/upgrade/defense must follow a damage or threat event."));
        }

        if (!row.LivingWorldConsequence.Passed)
        {
            diagnostics.Add(Error("goal066.living_world.linkage_missing", row.RowId, "Living-world actor/faction/event linkage is required."));
        }

        if (!row.InterlockedGameplayDependency.Passed)
        {
            diagnostics.Add(Error("goal066.interlocked.dependency_missing", row.RowId, "Interlocked gameplay dependency with Goal 065 deltas is required."));
        }

        ValidateFamilyDepth(row, diagnostics);
    }

    private static void ValidateFamilyDepth(SettlementRow row, List<SettlementDiagnostic> diagnostics)
    {
        var haystack = string.Join("|", row.BuildingKind, row.ProductionAction.ActionKind, row.DamageDestructionThreatEvent.ActionKind, row.RepairUpgradeDefenseResponse.ActionKind, row.LivingWorldConsequence.ConsequenceSummary);
        switch (row.FamilyId)
        {
            case "map_panel_rpg":
                RequireAny(haystack, row, diagnostics, ["trade", "guild", "shrine", "quest"], "goal066.family.map_panel.service_missing");
                RequireAny(haystack, row, diagnostics, ["faction", "guild", "guard", "patrol", "reputation"], "goal066.family.map_panel.faction_missing");
                break;
            case "survival_sandbox":
                RequireAny(haystack, row, diagnostics, ["shelter", "water", "trap", "food"], "goal066.family.survival.resource_or_recovery_missing");
                RequireAny(haystack, row, diagnostics, ["storm", "scarcity", "predator", "hazard"], "goal066.family.survival.hazard_missing");
                break;
            case "first_person_grid_dungeon":
                RequireAny(haystack, row, diagnostics, ["route", "gate", "safe", "cache"], "goal066.family.dungeon.route_missing");
                RequireAny(haystack, row, diagnostics, ["trap", "ambush", "door", "rune", "mechanism"], "goal066.family.dungeon.trap_or_lock_missing");
                break;
            default:
                diagnostics.Add(Error("goal066.family.unknown", row.RowId, "Unsupported family id."));
                break;
        }
    }

    private static void Require(string haystack, SettlementRow row, List<SettlementDiagnostic> diagnostics, string fragment, string code)
    {
        if (!haystack.Contains(fragment, StringComparison.Ordinal))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family-specific settlement outcome is missing: " + fragment));
        }
    }

    private static void RequireAny(string haystack, SettlementRow row, List<SettlementDiagnostic> diagnostics, IReadOnlyList<string> fragments, string code)
    {
        if (!fragments.Any(fragment => haystack.Contains(fragment, StringComparison.Ordinal)))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family-specific settlement outcome is missing one of: " + string.Join(",", fragments)));
        }
    }

    private static IReadOnlyList<string> RowMarkers(string rowId, string familyId, string seedId, string settlementId) =>
    [
        "settlement_row=" + rowId,
        "settlement_family=" + familyId,
        "settlement_seed=" + seedId,
        "settlement_id=" + settlementId,
        "settlement_construction_action=" + rowId,
        "settlement_production_delta=" + rowId,
        "settlement_destruction_damage=" + rowId,
        "settlement_repair_defense=" + rowId,
        "settlement_living_world_linkage=" + rowId,
        "settlement_interlocked_dependency=" + rowId,
        "settlement_replay_verified=" + rowId,
        "settlement_row_completed=" + rowId
    ];

    private static SettlementDiagnostic Error(string code, string target, string message) =>
        SettlementDiagnostic.Error(code, target, message);
}
