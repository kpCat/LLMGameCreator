namespace LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;

public sealed class IntegratedCampaignTimelineValidator
{
    public IReadOnlyList<TimelineDiagnostic> ValidateSourceManifest(TimelineSourceManifest manifest)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal070.gate.self_pass.forbidden", "source-manifest", "Goal 070 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal069AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "world_event_weather_daynight_crisis_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal070.preflight.goal069_handoff_missing", "source-manifest", "Goal 069 acceptance by user handoff is required before Goal 070."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == IntegratedCampaignTimelineVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal070.gate.required_missing", "source-manifest", "Goal 070 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal070.source.matrix_counts_invalid", "source-manifest", "Goal 070 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewPackageRcConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed
            || !manifest.Goal065InterlockedRowsConsumed
            || !manifest.Goal066SettlementRowsConsumed
            || !manifest.Goal067NarrativeRowsConsumed
            || !manifest.Goal068CombatMagicRowsConsumed
            || !manifest.Goal069WorldEventRowsConsumed)
        {
            diagnostics.Add(Error("goal070.source.chain_incomplete", "source-manifest", "Goal 070 must consume Goal 060/061/062/063/064/065/066/067/068/069 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateRows(TimelineMatrixSummary matrix, PreviewExportTimelinePayload previewPayload)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (!matrix.Passed
            || matrix.Accepted
            || matrix.RowCount != 9
            || matrix.StateChangingRowCount != 9
            || matrix.RowsWithSixOrMoreTicks != 9
            || matrix.RowsWithFiveOrMoreCategories != 9
            || matrix.RowsWithThreeOrMoreCascades != 9
            || matrix.RowsWithArbitration != 9
            || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal070.matrix.invalid", "timeline-matrix-summary", "Integrated campaign timeline matrix must contain 9 state-changing rows with required timeline/cascade/arbitration coverage."));
        }

        if (matrix.Rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal070.identity.duplicate_row_id", "rowId", "Timeline row ids must be unique."));
        }

        foreach (var familyId in IntegratedCampaignTimelineVocabulary.FamilyIds)
        {
            foreach (var seedId in IntegratedCampaignTimelineVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal070.matrix.row_missing", familyId + "/" + seedId, "Required timeline row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal070.preview.payload_invalid", "preview-export-timeline-payload", "Preview/export timeline payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateCascadeAndArbitration(CrossSystemCascadeLedger cascades, ConflictArbitrationLedger arbitrations)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (!cascades.Passed || cascades.RowCount != 9 || cascades.CascadeCount < 27)
        {
            diagnostics.Add(Error("goal070.cascade.ledger_invalid", "cross-system-cascade-ledger", "Every row must prove at least three cross-system cascades."));
        }

        if (!arbitrations.Passed || arbitrations.RowCount != 9 || arbitrations.ArbitrationCount != 9)
        {
            diagnostics.Add(Error("goal070.arbitration.ledger_invalid", "conflict-arbitration-ledger", "Every row must prove one conflict/arbitration decision."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateReplay(SaveLoadReplayAudit replay)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.StateChangingRowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal070.replay.audit_invalid", "save-load-replay-audit", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.StateChanging || row.InitialStateHash == row.FinalStateHash)
            {
                diagnostics.Add(Error("goal070.state.initial_final_equal", row.RowId, "Initial and final timeline state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SaveCheckpointHash != row.LoadedCheckpointHash)
            {
                diagnostics.Add(Error("goal070.save_load.mismatch", row.RowId, "Save/load checkpoint did not preserve timeline state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.ExpectedReplayHash != row.ReplayHash)
            {
                diagnostics.Add(Error("goal070.replay.mismatch", row.RowId, "Replay hashes must match for same timeline input."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateVariance(TimelineVarianceMetrics variance)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (!variance.Passed
            || variance.FamilyCount != 3
            || variance.SeedCount != 3
            || variance.DistinctRowHashCount != 9
            || variance.DistinctPhaseProfileCount != 3)
        {
            diagnostics.Add(Error("goal070.variance.invalid", "variance-metrics", "Variance must prove same-family seed differences and family-specific phase profiles beyond ids and hashes."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateUnityCommandPlan(TimelineUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal070.unity.command_plan_invalid", "unity-command-plan", "Unity command plan must cover all 9 timeline rows and stay accepted=false."));
        }

        foreach (var marker in IntegratedCampaignTimelineProjector.RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal070.unity.marker_missing", marker, "Unity command plan is missing a required global timeline marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.TickIds.Count < 6 || row.CascadeIds.Count < 3 || row.ArbitrationIds.Count < 1 || !row.StateChanged || !row.SaveLoadReplayPassed)
            {
                diagnostics.Add(Error("goal070.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include ticks, cascades, arbitration, state change and replay facts."));
            }

            foreach (var marker in IntegratedCampaignTimelineProjector.RowMarkers(row))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal070.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, tick, cascade, arbitration and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateUnityProof(TimelineUnityCommandPlan commandPlan, TimelineUnityProofSummary proof)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal070.unity.marker_missing", marker, "Unity player logs did not contain the required Goal 070 marker."));
            }
            else if (!proof.PlayerExecuted)
            {
                diagnostics.Add(TimelineDiagnostic.Warning("goal070.unity.marker_not_checked", marker, "Unity player was not executed, so the Goal 070 marker could not be checked."));
            }
        }

        if (proof.PlayerExecuted && (!proof.Passed || proof.ProvenRowCount != 9 || proof.MissingMarkers.Count != 0))
        {
            diagnostics.Add(Error("goal070.unity.proof_invalid", "unity-player-proof-summary", "Unity proof must match all Goal 070 markers and prove 9 rows."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<TimelineDiagnostic> ValidateInvalidMatrix(TimelineInvalidDiagnosticsMatrix invalid)
    {
        var diagnostics = new List<TimelineDiagnostic>();
        var ids = invalid.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in IntegratedCampaignTimelineVocabulary.RequiredInvalidScenarioIds)
        {
            if (!ids.Contains(required))
            {
                diagnostics.Add(Error("goal070.invalid.required_scenario_missing", required, "Invalid/fake/leak matrix is missing a required scenario."));
            }
        }

        foreach (var scenario in invalid.Scenarios)
        {
            if (scenario.ExpectedStatus != scenario.ActualStatus)
            {
                diagnostics.Add(Error("goal070.invalid.status_mismatch", scenario.ScenarioId, "Invalid scenario actual status must match expected status."));
            }

            if (!scenario.Diagnostics.Any(item => item.Code.StartsWith("goal070.", StringComparison.Ordinal)))
            {
                diagnostics.Add(Error("goal070.invalid.diagnostic_code_missing", scenario.ScenarioId, "Invalid scenario needs stable goal070 diagnostic codes."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<TimelineDiagnostic> Sort(IEnumerable<TimelineDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void ValidateRow(CampaignTimelineRow row, List<TimelineDiagnostic> diagnostics)
    {
        if (!row.StateChanging || row.InitialState.StateHash == row.SaveLoadReplayProof.FinalStateHash)
        {
            diagnostics.Add(Error("goal070.row.not_state_changing", row.RowId, "Every timeline row must change state."));
        }

        if (row.Ticks.Count < 6 || row.Ticks.Any(tick => !tick.StateChanging || tick.Deltas.Count == 0))
        {
            diagnostics.Add(Error("goal070.row.ticks_missing", row.RowId, "Every row must contain at least 6 ordered state-changing ticks."));
        }

        if (row.TouchedSystemCategories.Count < 5)
        {
            diagnostics.Add(Error("goal070.row.category_coverage_missing", row.RowId, "Every row must touch at least five system categories."));
        }

        if (row.Cascades.Count < 3 || row.Cascades.Any(cascade => !cascade.Passed))
        {
            diagnostics.Add(Error("goal070.row.cascade_missing", row.RowId, "Every row must include at least three valid cross-system cascades."));
        }

        if (!row.Arbitration.Passed)
        {
            diagnostics.Add(Error("goal070.row.arbitration_missing", row.RowId, "Every row must include a valid conflict/arbitration decision."));
        }

        if (!row.SettlementWorldNarrativeCombatCoupled)
        {
            diagnostics.Add(Error("goal070.row.coupling_missing", row.RowId, "Every row must couple settlement, world, narrative and combat systems."));
        }

        if (!row.SaveLoadReplayProof.SaveLoadRoundtripPassed || !row.SaveLoadReplayProof.ReplayDeterminismPassed)
        {
            diagnostics.Add(Error("goal070.row.replay_invalid", row.RowId, "Every row must pass save/load and replay proof."));
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static TimelineDiagnostic Error(string code, string target, string message) =>
        TimelineDiagnostic.Error(code, target, message);
}
