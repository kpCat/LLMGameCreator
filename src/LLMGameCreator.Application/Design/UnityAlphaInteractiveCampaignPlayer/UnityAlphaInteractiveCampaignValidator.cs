namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignValidator
{
    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateSourceManifest(InteractiveCampaignSourceManifest manifest)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal071.gate.self_pass.forbidden", "source-manifest", "Goal 071 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal070AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "integrated_campaign_timeline_simulation_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal071.preflight.goal070_handoff_missing", "source-manifest", "Goal 070 acceptance by user handoff is required before Goal 071."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == UnityAlphaInteractiveCampaignVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal071.gate.required_missing", "source-manifest", "Goal 071 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal071.source.matrix_counts_invalid", "source-manifest", "Goal 071 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal070TimelineEvidenceConsumed || !manifest.Goal070UnityProofConsumed)
        {
            diagnostics.Add(Error("goal071.source.goal070_chain_incomplete", "source-manifest", "Goal 071 must consume Goal 070 timeline and Unity proof evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateMatrixAndSelector(InteractiveCampaignMatrix matrix, FamilySeedSelectorModel selector)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (!matrix.Passed
            || matrix.Accepted
            || matrix.RowCount != 9
            || matrix.FamilyCount != 3
            || matrix.SeedCount != 3
            || matrix.StateChangingRowCount != 9
            || matrix.ActionCount < 18)
        {
            diagnostics.Add(Error("goal071.matrix.invalid", "interactive-campaign-row-matrix", "Interactive campaign matrix must contain 9 state-changing rows and scripted actions."));
        }

        if (matrix.Rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal071.identity.duplicate_row_id", "rowId", "Interactive campaign row ids must be unique."));
        }

        foreach (var familyId in UnityAlphaInteractiveCampaignVocabulary.FamilyIds)
        {
            foreach (var seedId in UnityAlphaInteractiveCampaignVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal071.matrix.row_missing", familyId + "/" + seedId, "Required interactive campaign row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!selector.Passed || selector.Families.Count != 3 || selector.Families.Any(item => item.SeedIds.Count != 3 || item.RowIds.Count != 3))
        {
            diagnostics.Add(Error("goal071.selector.invalid", "family-seed-selector", "Family/seed selector must expose three family rows and three seed rows per family."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateActionsAndTransitions(InputActionScript script, StateTransitionLedger ledger)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (!script.Passed || script.ActionCount < 18)
        {
            diagnostics.Add(Error("goal071.actions.script_invalid", "interactive-campaign-input-script", "Input/action script must contain deterministic state-changing actions."));
        }

        if (!ledger.Passed || ledger.RowCount != 9 || ledger.TransitionCount != script.ActionCount)
        {
            diagnostics.Add(Error("goal071.transitions.ledger_invalid", "interactive-campaign-state-transition-ledger", "State transition ledger must align with scripted actions."));
        }

        foreach (var action in script.Actions)
        {
            if (!action.DeltaApplied || action.StateBeforeHash == action.StateAfterHash)
            {
                diagnostics.Add(Error("goal071.actions.unchanged_state", action.ActionId, "Interactive action must apply a state-changing delta."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateReplay(InteractiveCampaignSaveLoadReplayProof replay)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal071.replay.invalid", "interactive-campaign-save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.SaveLoadRoundtripPassed || row.SaveCheckpointHash != row.LoadedCheckpointHash)
            {
                diagnostics.Add(Error("goal071.save_load.mismatch", row.RowId, "Save/load checkpoint did not preserve interactive campaign state."));
            }

            if (!row.ReplayDeterminismPassed || row.ExpectedReplayHash != row.ReplayHash)
            {
                diagnostics.Add(Error("goal071.replay.mismatch", row.RowId, "Replay hashes must match for the same interactive script."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateHudAndPreview(InteractiveCampaignHudContract hud, InteractiveCampaignPreviewExportPayload preview)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (!hud.Passed || hud.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal071.hud.contract_invalid", "interactive-campaign-hud-contract", "HUD contract must cover all interactive rows."));
        }

        foreach (var required in new[] { "familyId", "seedId", "rowId", "actionId", "stepId", "stateBeforeHash", "stateAfterHash", "deltaSummary" })
        {
            if (!hud.RequiredFields.Contains(required, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal071.hud.required_field_missing", required, "HUD contract is missing a required review field."));
            }
        }

        if (!preview.Passed || preview.RowCount != 9)
        {
            diagnostics.Add(Error("goal071.preview.payload_invalid", "interactive-campaign-preview-export-payload", "Preview/export payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateUnityCommandPlan(InteractiveCampaignUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal071.unity.command_plan_invalid", "interactive-campaign-command-plan", "Unity command plan must cover all 9 interactive campaign rows and stay accepted=false."));
        }

        foreach (var marker in UnityAlphaInteractiveCampaignBuilder.RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal071.unity.marker_missing", marker, "Unity command plan is missing a required global interactive marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.StepIds.Count < 2 || row.InputIds.Count != row.StepIds.Count || row.StateBeforeHashes.Count != row.StepIds.Count || row.StateAfterHashes.Count != row.StepIds.Count)
            {
                diagnostics.Add(Error("goal071.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include aligned input, step and state hash arrays."));
            }

            foreach (var marker in UnityAlphaInteractiveCampaignBuilder.RowMarkers(row))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal071.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs family, seed, row, input, step, state, HUD and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateUnityProof(InteractiveCampaignUnityCommandPlan commandPlan, InteractiveCampaignUnityProofSummary proof)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal071.unity.marker_missing", marker, "Unity player logs did not contain the required Goal 071 marker."));
            }
            else if (!proof.PlayerExecuted)
            {
                diagnostics.Add(InteractiveCampaignDiagnostic.Warning("goal071.unity.marker_not_checked", marker, "Unity player was not executed, so the Goal 071 marker could not be checked."));
            }
        }

        if (proof.PlayerExecuted && (!proof.Passed || proof.ProvenRowCount != 9 || proof.MissingMarkers.Count != 0))
        {
            diagnostics.Add(Error("goal071.unity.proof_invalid", "interactive-campaign-player-proof-summary", "Unity proof must match all Goal 071 markers and prove 9 rows."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<InteractiveCampaignDiagnostic> ValidateInvalidMatrix(InteractiveCampaignInvalidDiagnosticsMatrix invalid)
    {
        var diagnostics = new List<InteractiveCampaignDiagnostic>();
        var ids = invalid.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in UnityAlphaInteractiveCampaignVocabulary.RequiredInvalidScenarioIds)
        {
            if (!ids.Contains(required))
            {
                diagnostics.Add(Error("goal071.invalid.required_scenario_missing", required, "Invalid/fake/leak matrix is missing a required scenario."));
            }
        }

        foreach (var scenario in invalid.Scenarios)
        {
            if (scenario.ExpectedStatus != scenario.ActualStatus)
            {
                diagnostics.Add(Error("goal071.invalid.status_mismatch", scenario.ScenarioId, "Invalid scenario actual status must match expected status."));
            }

            if (!scenario.Diagnostics.Any(item => item.Code.StartsWith("goal071.", StringComparison.Ordinal)))
            {
                diagnostics.Add(Error("goal071.invalid.diagnostic_code_missing", scenario.ScenarioId, "Invalid scenario needs stable goal071 diagnostic codes."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<InteractiveCampaignDiagnostic> Sort(IEnumerable<InteractiveCampaignDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void ValidateRow(InteractiveCampaignRow row, List<InteractiveCampaignDiagnostic> diagnostics)
    {
        if (!row.StateChanging || row.InitialStateHash == row.FinalStateHash)
        {
            diagnostics.Add(Error("goal071.row.not_state_changing", row.RowId, "Every interactive campaign row must change state across the source timeline."));
        }

        if (!row.HudRenderable || string.IsNullOrWhiteSpace(row.SelectedActionId) || string.IsNullOrWhiteSpace(row.SelectedStepId))
        {
            diagnostics.Add(Error("goal071.row.hud_missing", row.RowId, "Every row must expose action, step and state hashes to the HUD contract."));
        }

        if (row.Actions.Count < 2 || row.Actions.Any(action => !action.DeltaApplied || action.StateBeforeHash == action.StateAfterHash))
        {
            diagnostics.Add(Error("goal071.row.actions_invalid", row.RowId, "Every row must expose multiple deterministic state-changing actions."));
        }

        if (!row.SaveLoadReplayPassed)
        {
            diagnostics.Add(Error("goal071.row.replay_invalid", row.RowId, "Every row must preserve Goal 070 save/load/replay compatibility."));
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static InteractiveCampaignDiagnostic Error(string code, string target, string message) =>
        InteractiveCampaignDiagnostic.Error(code, target, message);
}
