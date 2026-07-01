namespace LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;

public sealed class LivingWorldNpcFactionSimulationValidator
{
    public IReadOnlyList<LivingWorldDiagnostic> ValidateSourceManifest(LivingWorldSourceManifest manifest)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal064.gate.self_pass.forbidden", "source-manifest", "Goal 064 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal063AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "gameplay_consequence_depth_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal064.preflight.goal063_handoff_missing", "source-manifest", "Goal 063 acceptance by user handoff is required before Goal 064."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == LivingWorldNpcFactionSimulationVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal064.gate.required_missing", "source-manifest", "Goal 064 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal064.source.matrix_counts_invalid", "source-manifest", "Goal 064 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed || !manifest.Goal061ReviewRowsConsumed || !manifest.Goal062SpatialRowsConsumed || !manifest.Goal063GameplayRowsConsumed)
        {
            diagnostics.Add(Error("goal064.source.chain_incomplete", "source-manifest", "Goal 064 must consume Goal 060 package rows, Goal 061 review rows, Goal 062 spatial rows and Goal 063 gameplay rows."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<LivingWorldDiagnostic> ValidateSimulation(
        LivingWorldActorFactionCatalogSummary catalog,
        LivingWorldSimulationMatrixPlan matrix,
        LivingWorldPreviewExportPayload previewPayload)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        if (!catalog.Passed || catalog.ActorCount < 18 || catalog.FactionCount < 18 || catalog.RuleFamilies.Count != 3)
        {
            diagnostics.Add(Error("goal064.catalog.invalid", "actor-faction-catalog-summary", "Catalog must contain unique actor/faction records for all 9 rows and 3 rule families."));
        }

        if (!matrix.Passed || matrix.Accepted || matrix.RowCount != 9 || matrix.StateChangingRowCount != 9 || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal064.matrix.invalid", "simulation-matrix-plan", "Simulation matrix must contain 9 produced-for-review state-changing rows with distinct hashes."));
        }

        foreach (var familyId in LivingWorldNpcFactionSimulationVocabulary.FamilyIds)
        {
            foreach (var seedId in LivingWorldNpcFactionSimulationVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal064.matrix.row_missing", familyId + "/" + seedId, "Required living-world simulation row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal064.preview.payload_invalid", "preview-export-living-world-payload", "Preview/export living-world payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<LivingWorldDiagnostic> ValidateReplayAndVariance(
        LivingWorldSaveLoadReplayProof saveLoadReplay,
        LivingWorldVarianceMetrics variance)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        if (!saveLoadReplay.Passed
            || saveLoadReplay.RowCount != 9
            || saveLoadReplay.StateChangedRowCount != 9
            || saveLoadReplay.SaveLoadPassedRowCount != 9
            || saveLoadReplay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal064.replay.audit_invalid", "save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in saveLoadReplay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal064.state.before_after_equal", row.RowId, "Before and after state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed)
            {
                diagnostics.Add(Error("goal064.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal064.replay.mismatch", row.RowId, "Replay hashes must match for same input."));
            }
        }

        if (!variance.Passed
            || !variance.HashOnlyVarianceRejected
            || !variance.SameFamilySeedVariationPassed
            || !variance.CrossFamilyRuleVariationPassed
            || variance.DistinctAfterStateHashCount != 9
            || variance.DistinctRuleProfileCount != 3)
        {
            diagnostics.Add(Error("goal064.variance.invalid", "variance-metrics", "Variance must prove same-family seed variation and cross-family rule differences beyond ID/hash noise."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<LivingWorldDiagnostic> ValidateUnityCommandPlan(LivingWorldUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal064.unity.command_plan_invalid", "unity-command-plan", "Unity command plan must cover all 9 living-world rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal064.unity.marker_missing", marker, "Unity command plan is missing a required global marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.TickIds.Count < 3)
            {
                diagnostics.Add(Error("goal064.unity.row_tick_plan_shallow", row.RowId, "Every Unity row marker plan must include at least three ticks."));
            }

            foreach (var marker in new[]
            {
                "living_world_row=" + row.RowId,
                "npc_state_changed=true",
                "faction_relation_changed=true",
                "world_event_resolved=true",
                "living_world_row_completed=" + row.RowId
            })
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal064.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, state-change and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<LivingWorldDiagnostic> ValidateUnityProof(
        LivingWorldUnityCommandPlan commandPlan,
        LivingWorldUnityProofSummary proof)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal064.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 064 marker."));
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
                diagnostics.Add(Error("goal064.unity.proof_inconsistent", "unity-player-proof-summary", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal064.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<LivingWorldDiagnostic> ValidateInvalidMatrix(InvalidLivingWorldDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<LivingWorldDiagnostic>();
        foreach (var scenarioId in LivingWorldNpcFactionSimulationVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal064.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal064.invalid.matrix_failed", "invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<LivingWorldDiagnostic> Sort(IEnumerable<LivingWorldDiagnostic> diagnostics) =>
        LivingWorldNpcFactionSimulationSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateRow(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics)
    {
        if (row.ActorRecords.Count < 2 || row.FactionRecords.Count < 2)
        {
            diagnostics.Add(Error("goal064.row.catalog_shallow", row.RowId, "Every row requires actor and faction/group records."));
        }

        if (row.ActorRecords.Select(item => item.ActorId).Distinct(StringComparer.Ordinal).Count() != row.ActorRecords.Count)
        {
            diagnostics.Add(Error("goal064.catalog.duplicate_actor_id", row.RowId, "Actor ids must be unique per row."));
        }

        if (row.FactionRecords.Select(item => item.FactionId).Distinct(StringComparer.Ordinal).Count() != row.FactionRecords.Count)
        {
            diagnostics.Add(Error("goal064.catalog.duplicate_faction_id", row.RowId, "Faction ids must be unique per row."));
        }

        var knownRelationTargets = row.ActorRecords.Select(item => item.ActorId)
            .Concat(row.FactionRecords.Select(item => item.FactionId))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var relation in row.RelationshipRecords)
        {
            if (!knownRelationTargets.Contains(relation.SourceActorOrFactionId) || !knownRelationTargets.Contains(relation.TargetActorOrFactionId))
            {
                diagnostics.Add(Error("goal064.relation.invalid_target", relation.RelationshipId, "Relationship endpoints must resolve to declared actor/faction ids."));
            }
        }

        if (row.ScheduleAvailabilityRecords.Count < 2 || row.ScheduleAvailabilityRecords.Any(item => !item.AvailabilityChanged))
        {
            diagnostics.Add(Error("goal064.schedule.no_availability_change", row.RowId, "Every row requires schedule/availability changes."));
        }

        if (row.WorldEventRecords.Count == 0 || row.WorldEventRecords.Any(item => !item.Resolved || item.BeforeState == item.AfterState))
        {
            diagnostics.Add(Error("goal064.event.not_resolved", row.RowId, "Every row requires at least one resolved world event with before/after state."));
        }

        if (row.MemoryRumorConsequenceTraceRecords.Count < 2
            || row.MemoryRumorConsequenceTraceRecords.Any(item => string.IsNullOrWhiteSpace(item.SourceGameplayConsequenceRowRef) || string.IsNullOrWhiteSpace(item.SourceSpatialDetailRowRef)))
        {
            diagnostics.Add(Error("goal064.memory.trace_missing_source", row.RowId, "Memory/rumor traces must link Goal 063 and Goal 062 source refs."));
        }

        if (row.OrderedTickPlan.Count < 3 || row.OrderedTickPlan.Any(item => item.BeforeStateHash == item.AfterStateHash))
        {
            diagnostics.Add(Error("goal064.tick.plan_invalid", row.RowId, "Every row requires ordered state-changing ticks."));
        }

        if (row.BeforeState.StateHash == row.AfterState.StateHash || row.StateDeltaSummary.Count < 8 || row.StateDeltaSummary.Any(item => !item.Passed))
        {
            diagnostics.Add(Error("goal064.state.non_state_changing_row", row.RowId, "Every row requires passed state deltas and different before/after hashes."));
        }

        ValidateFamilyDepth(row, diagnostics);
    }

    private static void ValidateFamilyDepth(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics)
    {
        var changedKeys = row.StateDeltaSummary.Select(item => item.Key).ToList();
        switch (row.FamilyId)
        {
            case "map_panel_rpg":
                RequireAny(row, diagnostics, changedKeys, ".availability", "goal064.family.map_panel.availability_missing");
                RequireAny(row, diagnostics, changedKeys, ".reputation", "goal064.family.map_panel.reputation_missing");
                RequireTrace(row, diagnostics, "quest_rumor", "goal064.family.map_panel.rumor_missing");
                RequireEvent(row, diagnostics, "quest_rumor_pressure", "goal064.family.map_panel.event_missing");
                RequireSourceDelta(row, diagnostics, "reward", "goal064.family.map_panel.reward_link_missing");
                break;
            case "survival_sandbox":
                RequireAny(row, diagnostics, changedKeys, ".availability", "goal064.family.survival.availability_missing");
                RequireAny(row, diagnostics, changedKeys, ".trust_or_aggression", "goal064.family.survival.trust_missing");
                RequireEvent(row, diagnostics, "weather_hunger_shelter_danger_recovery", "goal064.family.survival.event_memory_missing");
                RequirePressure(row, diagnostics, "scarcity_resource_shelter_pressure", "goal064.family.survival.pressure_missing");
                break;
            case "first_person_grid_dungeon":
                RequireAny(row, diagnostics, changedKeys, ".status", "goal064.family.dungeon.actor_pressure_missing");
                RequireAny(row, diagnostics, changedKeys, ".trust_or_aggression", "goal064.family.dungeon.aggression_missing");
                RequireEvent(row, diagnostics, "alert_loot_progression_spatial_relation", "goal064.family.dungeon.loot_progression_missing");
                RequirePressure(row, diagnostics, "alert_loot_spatial_pressure", "goal064.family.dungeon.spatial_relation_missing");
                break;
            default:
                diagnostics.Add(Error("goal064.family.unknown", row.RowId, "Unsupported family id."));
                break;
        }
    }

    private static void RequireAny(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics, IReadOnlyList<string> changedKeys, string keyFragment, string code)
    {
        if (!changedKeys.Any(key => key.Contains(keyFragment, StringComparison.Ordinal)))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family-specific changed key is missing: " + keyFragment));
        }
    }

    private static void RequireTrace(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics, string traceKindFragment, string code)
    {
        if (!row.MemoryRumorConsequenceTraceRecords.Any(item => item.TraceKind.Contains(traceKindFragment, StringComparison.Ordinal)))
        {
            diagnostics.Add(Error(code, row.RowId, "Required memory/rumor trace is missing."));
        }
    }

    private static void RequireEvent(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics, string eventKind, string code)
    {
        if (!row.WorldEventRecords.Any(item => item.EventKind == eventKind && item.Resolved))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family-specific world event is missing."));
        }
    }

    private static void RequireSourceDelta(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics, string deltaFragment, string code)
    {
        if (!row.WorldEventRecords.Any(item => item.SourceGameplayDeltaId.Contains(deltaFragment, StringComparison.OrdinalIgnoreCase))
            && !row.MemoryRumorConsequenceTraceRecords.Any(item => item.SourceDeltaId.Contains(deltaFragment, StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Error(code, row.RowId, "Required source gameplay consequence link is missing."));
        }
    }

    private static void RequirePressure(LivingWorldSimulationRow row, List<LivingWorldDiagnostic> diagnostics, string expectedProfile, string code)
    {
        if (!row.OrderedTickPlan.Any(item => item.TickKind == expectedProfile))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family pressure tick is missing."));
        }
    }

    private static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "living_world_matrix_loaded=goal064",
        "living_world_matrix_completed=true",
        "review_package_proof=goal064",
        "living_world_npc_faction_simulation_matrix_verification=required"
    ];

    private static LivingWorldDiagnostic Error(string code, string target, string message) =>
        LivingWorldDiagnostic.Error(code, target, message);
}
