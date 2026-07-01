namespace LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;

public sealed class WorldEventWeatherDayNightCrisisValidator
{
    public IReadOnlyList<WorldEventDiagnostic> ValidateSourceManifest(WorldEventSourceManifest manifest)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal069.gate.self_pass.forbidden", "source-manifest", "Goal 069 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal068AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "combat_magic_ability_boss_encounter_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal069.preflight.goal068_handoff_missing", "source-manifest", "Goal 068 acceptance by user handoff is required before Goal 069."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == WorldEventWeatherDayNightCrisisVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal069.gate.required_missing", "source-manifest", "Goal 069 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal069.source.matrix_counts_invalid", "source-manifest", "Goal 069 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewPackageRcConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed
            || !manifest.Goal065InterlockedRowsConsumed
            || !manifest.Goal066SettlementRowsConsumed
            || !manifest.Goal067NarrativeRowsConsumed
            || !manifest.Goal068CombatMagicRowsConsumed)
        {
            diagnostics.Add(Error("goal069.source.chain_incomplete", "source-manifest", "Goal 069 must consume Goal 060/061/062/063/064/065/066/067/068 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateCatalogs(
        WorldClockCalendarPolicy clockPolicy,
        WeatherHazardCatalog weatherCatalog,
        CrisisEventCatalog crisisCatalog)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (!clockPolicy.Passed || clockPolicy.Phases.Count < 4)
        {
            diagnostics.Add(Error("goal069.clock.policy_invalid", "world-clock-calendar-policy", "World clock policy must define deterministic day/night phases."));
        }

        if (!weatherCatalog.Passed || weatherCatalog.WeatherHazards.Count != 9)
        {
            diagnostics.Add(Error("goal069.weather.catalog_invalid", "weather-hazard-catalog", "Weather/hazard catalog must cover every family/seed row."));
        }

        if (!crisisCatalog.Passed || crisisCatalog.CrisisEvents.Count != 9)
        {
            diagnostics.Add(Error("goal069.crisis.catalog_invalid", "crisis-event-catalog", "Crisis catalog must cover every family/seed row."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateRows(WorldEventRowMatrix matrix, WorldEventPreviewExportPayload previewPayload)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (!matrix.Passed
            || matrix.Accepted
            || matrix.RowCount != 9
            || matrix.StateChangingRowCount != 9
            || matrix.DayNightEffectRowCount != 9
            || matrix.WeatherHazardRowCount != 9
            || matrix.CrisisConsequenceRowCount != 9
            || matrix.CrossSystemDeltaRowCount != 9
            || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal069.matrix.invalid", "world-event-weather-daynight-row-matrix", "World-event/weather/day-night/crisis matrix must contain 9 state-changing rows with required coverage."));
        }

        if (matrix.Rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal069.identity.duplicate_row_id", "rowId", "World-event row ids must be unique."));
        }

        foreach (var familyId in WorldEventWeatherDayNightCrisisVocabulary.FamilyIds)
        {
            foreach (var seedId in WorldEventWeatherDayNightCrisisVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal069.matrix.row_missing", familyId + "/" + seedId, "Required world-event row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal069.preview.payload_invalid", "preview-export-payload", "Preview/export world-event payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateReplay(WorldEventSaveLoadReplayProof replay)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.StateChangedRowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal069.replay.audit_invalid", "save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal069.state.before_after_equal", row.RowId, "Before and after world-event state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SerializedAfterStateHash != row.RestoredAfterStateHash)
            {
                diagnostics.Add(Error("goal069.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve world-event after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal069.replay.mismatch", row.RowId, "Replay hashes must match for same world-event input."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateVariance(WorldEventVarianceMetrics variance)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (!variance.Passed
            || variance.FamilyCount != 3
            || variance.SeedCount != 3
            || variance.DistinctWeatherCount < 9
            || variance.DistinctCrisisCount < 9
            || variance.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal069.variance.invalid", "variance-metrics", "Variance must prove distinct family/seed weather, crisis, phase and state hashes."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateUnityCommandPlan(WorldEventUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal069.unity.command_plan_invalid", "unity-command-plan", "Unity command plan must cover all 9 world-event rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal069.unity.marker_missing", marker, "Unity command plan is missing a required global world-event marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.ClockPhase)
                || string.IsNullOrWhiteSpace(row.WeatherId)
                || string.IsNullOrWhiteSpace(row.CrisisId)
                || !row.StateChanged
                || !row.SaveLoadReplayPassed)
            {
                diagnostics.Add(Error("goal069.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include clock phase, weather, crisis, state change and replay facts."));
            }

            foreach (var marker in RowMarkers(row))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal069.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, family, seed, clock, weather, crisis, state, replay and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateUnityProof(WorldEventUnityCommandPlan commandPlan, WorldEventUnityProofSummary proof)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal069.unity.marker_missing", marker, "Unity player logs did not contain the required Goal 069 marker."));
            }
            else if (!proof.PlayerExecuted)
            {
                diagnostics.Add(WorldEventDiagnostic.Warning("goal069.unity.marker_not_checked", marker, "Unity player was not executed, so the Goal 069 marker could not be checked."));
            }
        }

        if (proof.PlayerExecuted && (!proof.Passed || proof.ProvenRowCount != 9 || proof.MissingMarkers.Count != 0))
        {
            diagnostics.Add(Error("goal069.unity.proof_invalid", "unity-proof-summary", "Unity proof must match all Goal 069 markers and prove 9 rows."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<WorldEventDiagnostic> ValidateInvalidMatrix(WorldEventInvalidDiagnosticsMatrix invalid)
    {
        var diagnostics = new List<WorldEventDiagnostic>();
        var ids = invalid.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in WorldEventWeatherDayNightCrisisVocabulary.RequiredInvalidScenarioIds)
        {
            if (!ids.Contains(required))
            {
                diagnostics.Add(Error("goal069.invalid.required_scenario_missing", required, "Invalid/fake/leak matrix is missing a required scenario."));
            }
        }

        foreach (var scenario in invalid.Scenarios)
        {
            if (scenario.ExpectedStatus != scenario.ActualStatus)
            {
                diagnostics.Add(Error("goal069.invalid.status_mismatch", scenario.ScenarioId, "Invalid scenario actual status must match expected status."));
            }

            if (!scenario.Diagnostics.Any(item => item.Code.StartsWith("goal069.", StringComparison.Ordinal)))
            {
                diagnostics.Add(Error("goal069.invalid.diagnostic_code_missing", scenario.ScenarioId, "Invalid scenario needs stable goal069 diagnostic codes."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "world_event_matrix_loaded=true",
        "world_event_matrix_completed=true",
        "world_event_weather_daynight_crisis_matrix_verification=required",
        "review_package_proof=goal069"
    ];

    public static IReadOnlyList<string> RowMarkers(WorldEventUnityCommandPlanRow row) =>
    [
        "world_event_row=" + row.RowId,
        "world_event_family=" + row.FamilyId,
        "world_event_seed=" + row.SeedId,
        "world_event_clock_phase=" + row.ClockPhase,
        "world_event_weather=" + row.WeatherId,
        "world_event_crisis=" + row.CrisisId,
        "world_event_state_changed=true",
        "world_event_save_load_replay=true",
        "world_event_row_completed=" + row.RowId
    ];

    public static IReadOnlyList<WorldEventDiagnostic> Sort(IEnumerable<WorldEventDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void ValidateRow(WorldEventRow row, List<WorldEventDiagnostic> diagnostics)
    {
        if (!row.StateChanging || row.BeforeState.StateHash == row.AfterState.StateHash)
        {
            diagnostics.Add(Error("goal069.row.not_state_changing", row.RowId, "Every world-event row must change state."));
        }

        if (!row.DayNightEffect.Passed || row.DayNightEffect.BeforePhase == row.DayNightEffect.AfterPhase)
        {
            diagnostics.Add(Error("goal069.row.day_night_missing", row.RowId, "Every row must have a stateful day/night phase effect."));
        }

        if (!row.WeatherHazard.Passed || row.WeatherHazard.StateDeltaRefs.Count == 0)
        {
            diagnostics.Add(Error("goal069.row.weather_hazard_missing", row.RowId, "Every row must have weather/hazard state deltas."));
        }

        if (!row.CrisisEvent.Passed || row.CrisisEvent.StateDeltaRefs.Count == 0)
        {
            diagnostics.Add(Error("goal069.row.crisis_consequence_missing", row.RowId, "Every crisis event must have consequences."));
        }

        if (row.CrossSystemDeltas.Select(item => item.Category).Distinct(StringComparer.Ordinal).Count() < 2
            || row.CrossSystemDeltas.Count < 5
            || row.CrossSystemDeltas.Any(item => !item.Passed || item.BeforeValue == item.AfterValue))
        {
            diagnostics.Add(Error("goal069.row.cross_system_delta_missing", row.RowId, "Every row must change at least two cross-system categories."));
        }

        if (!row.SaveLoadReplayProof.SaveLoadRoundtripPassed || !row.SaveLoadReplayProof.ReplayDeterminismPassed)
        {
            diagnostics.Add(Error("goal069.row.replay_invalid", row.RowId, "Every row must pass save/load and replay proof."));
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static WorldEventDiagnostic Error(string code, string target, string message) =>
        WorldEventDiagnostic.Error(code, target, message);
}
