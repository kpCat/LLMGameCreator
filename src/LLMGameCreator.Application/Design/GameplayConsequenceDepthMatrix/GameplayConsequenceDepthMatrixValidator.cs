namespace LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;

public sealed class GameplayConsequenceDepthMatrixValidator
{
    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateSourceManifest(GameplayConsequenceSourceManifest manifest)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal063.gate.self_pass.forbidden", "source-manifest", "Goal 063 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal062AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "constrained_spatial_detail_generation_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal063.preflight.goal062_handoff_missing", "source-manifest", "Goal 062 acceptance by user handoff is required before Goal 063."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == GameplayConsequenceDepthMatrixVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal063.gate.required_missing", "source-manifest", "Goal 063 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal063.source.matrix_counts_invalid", "source-manifest", "Goal 063 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed || !manifest.Goal061ReviewRowsConsumed || !manifest.Goal062SpatialRowsConsumed)
        {
            diagnostics.Add(Error("goal063.source.chain_incomplete", "source-manifest", "Goal 063 must consume Goal 060 package rows, Goal 061 review rows and Goal 062 spatial rows."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateCommandPlan(
        GameplayConsequenceCatalog catalog,
        GameplayConsequenceCommandPlanMatrix commandPlan)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        if (!catalog.Passed || catalog.FamilyTemplateCount != 3)
        {
            diagnostics.Add(Error("goal063.catalog.invalid", "gameplay-consequence-catalog", "Catalog must define all three family consequence templates."));
        }

        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.RowCount != 9)
        {
            diagnostics.Add(Error("goal063.plan.invalid", "gameplay-command-plan-matrix", "Command plan matrix must contain 9 produced-for-review rows."));
        }

        foreach (var familyId in GameplayConsequenceDepthMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in GameplayConsequenceDepthMatrixVocabulary.SeedIds)
            {
                if (!commandPlan.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal063.plan.row_missing", familyId + "/" + seedId, "Required gameplay consequence row is missing."));
                }
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.Commands.Count < 3 || row.StateChangingStepCount < 3)
            {
                diagnostics.Add(Error("goal063.plan.too_shallow", row.RowId, "Every row requires at least three state-changing command steps."));
            }

            var commandIds = row.Commands.Select(item => item.CommandId).ToList();
            if (commandIds.Count != commandIds.Distinct(StringComparer.Ordinal).Count())
            {
                diagnostics.Add(Error("goal063.plan.duplicate_command_id", row.RowId, "Command ids must be unique per row."));
            }

            if (row.Commands.Any(item => item.ExpectedChanges.Count == 0))
            {
                diagnostics.Add(Error("goal063.plan.command_without_delta", row.RowId, "Every command must carry expected state changes."));
            }

            ValidateFamilyShape(row, diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateStateProofs(
        GameplayConsequenceRuntimeStateDeltaMatrix matrix,
        GameplayConsequenceSaveLoadReplayAudit replayAudit,
        GameplayConsequenceFamilySummary familySummary,
        GameplayConsequencePreviewExportPayload previewPayload)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        if (!matrix.Passed || matrix.RowCount != 9 || matrix.StateChangingRowCount != 9)
        {
            diagnostics.Add(Error("goal063.state.matrix_invalid", "runtime-state-delta-matrix", "Runtime/state projection must prove all 9 rows changed state."));
        }

        foreach (var row in matrix.Rows)
        {
            if (row.StateChangingStepCount < 3 || !row.StateTransitionProofPassed)
            {
                diagnostics.Add(Error("goal063.state.row_not_deep", row.RowId, "Every row needs at least three passed state-changing transitions."));
            }

            if (row.BeforeState.StateHash == row.AfterState.StateHash)
            {
                diagnostics.Add(Error("goal063.state.before_after_equal", row.RowId, "Before and after state hashes must differ."));
            }

            foreach (var transition in row.Transitions)
            {
                if (!transition.StateChanged || !transition.ExpectedVsActualPassed || transition.Deltas.Count == 0)
                {
                    diagnostics.Add(Error("goal063.state.transition_invalid", transition.CommandId, "Every transition must have before/after deltas and expected-vs-actual proof."));
                }
            }
        }

        if (!replayAudit.Passed || replayAudit.RowCount != 9 || replayAudit.SaveLoadPassedRowCount != 9 || replayAudit.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal063.replay.audit_invalid", "save-load-replay-audit", "Save/load and same-seed replay must pass for all 9 rows."));
        }

        if (!familySummary.Passed || !familySummary.MeaningfulVariancePassed || familySummary.FamilyCount != 3 || familySummary.SeedCount != 3)
        {
            diagnostics.Add(Error("goal063.variance.summary_invalid", "family-consequence-summary", "Meaningful variance must pass across all three families and seeds."));
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal063.preview.payload_invalid", "preview-export-gameplay-payload", "Preview/export gameplay payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateUnityCommandPlan(GameplayConsequenceUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal063.unity.command_plan_invalid", "unity-command-plan", "Unity command plan must cover all 9 gameplay rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal063.unity.marker_missing", marker, "Unity command plan is missing a required global marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.StepIds.Count < 3 || row.DeltaIds.Count < 3)
            {
                diagnostics.Add(Error("goal063.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include at least three steps and deltas."));
            }

            if (!row.ExpectedPlayerMarkers.Contains("gameplay_consequence_row=" + row.FamilyId + "/" + row.SeedId, StringComparer.Ordinal)
                || !row.ExpectedPlayerMarkers.Contains("gameplay_consequence_completed=" + row.FamilyId + "/" + row.SeedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal063.unity.row_marker_missing", row.RowId, "Every Unity row marker plan needs row and completion markers."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateUnityProof(
        GameplayConsequenceUnityCommandPlan commandPlan,
        GameplayConsequenceUnityProofSummary proof)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal063.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 063 marker."));
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
                diagnostics.Add(Error("goal063.unity.proof_inconsistent", "unity-player-proof-summary", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal063.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<GameplayConsequenceDiagnostic> ValidateInvalidMatrix(InvalidGameplayConsequenceDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        foreach (var scenarioId in GameplayConsequenceDepthMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal063.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal063.invalid.matrix_failed", "invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public InvalidGameplayConsequenceDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidGameplayConsequenceScenario>
        {
            Invalid("missing_goal060_package_row", "Remove a Goal 060 materialized package row.", "blocked", Error("goal063.source.goal060_row_missing", "Goal060", "Goal 060 package row is required.")),
            Invalid("missing_goal061_review_package_row", "Remove the matching Goal 061 review row.", "blocked", Error("goal063.source.goal061_row_missing", "Goal061", "Goal 061 review row is required.")),
            Invalid("missing_goal062_spatial_detail_row", "Remove the matching Goal 062 spatial row.", "blocked", Error("goal063.source.goal062_row_missing", "Goal062", "Goal 062 spatial row is required.")),
            Invalid("fake_family", "Inject a family outside the accepted matrix.", "rejected", Error("goal063.source.fake_family", "familyId", "Family must be one of the accepted matrix families.")),
            Invalid("fake_seed", "Inject a seed outside the accepted matrix.", "rejected", Error("goal063.source.fake_seed", "seedId", "Seed must be one of seed_alpha, seed_beta or seed_gamma.")),
            Invalid("fake_package_id", "Swap the package id with an unproven id.", "rejected", Error("goal063.source.fake_package_id", "packageId", "Package id must come from Goal 060.")),
            Invalid("fake_command_id", "Reference a command id not present in the row plan.", "rejected", Error("goal063.plan.fake_command_id", "commandId", "Command id must be generated by the row plan.")),
            Invalid("duplicate_command_id", "Duplicate a command id in a row.", "rejected", Error("goal063.plan.duplicate_command_id", "commandId", "Command ids must be unique.")),
            Invalid("command_without_state_delta", "Emit a command with no expected state changes.", "rejected", Error("goal063.plan.command_without_delta", "command", "A command without state delta is not gameplay proof.")),
            Invalid("delta_without_before_after_values", "Drop before or after values from a delta.", "rejected", Error("goal063.state.delta_before_after_missing", "delta", "State deltas require before and after values.")),
            Invalid("replay_mismatch", "Re-run same seed and produce a different state hash.", "rejected", Error("goal063.replay.mismatch", "replay", "Same seed replay must be deterministic.")),
            Invalid("save_load_mismatch", "Deserialize a different after-state hash.", "rejected", Error("goal063.save_load.mismatch", "serializer", "Serializer roundtrip must preserve after state.")),
            Invalid("row_hash_collision", "Produce two rows with the same proof hash.", "rejected", Error("goal063.matrix.row_hash_collision", "rowHash", "Row proof hashes must be distinct.")),
            Invalid("no_meaningful_variance", "Only change ids or hashes without gameplay axes.", "rejected", Error("goal063.variance.not_meaningful", "variance", "Variance must include gameplay state axes, not only ids.")),
            Invalid("unsafe_path", "Reference a source or staging path outside the goal root.", "rejected", Error("goal063.path.unsafe", "../", "Paths must stay repo-relative and traversal-free.")),
            Invalid("final_prose_treated_as_gameplay_consequence", "Use final report prose as the consequence proof.", "rejected", Error("goal063.proof.prose_only", "report", "Gameplay consequence proof must be structured state deltas.")),
            Invalid("provider_llm_rag_media_generation_claim", "Claim provider, LLM, RAG or media generation.", "blocked", Error("goal063.leak.provider_llm_rag_media_generation_claim", "scope", "Provider, LLM, RAG and media generation are forbidden.")),
            Invalid("runtime_ui_unity_broad_mutation_claim", "Claim broad Runtime/UI/Unity mutation as proof.", "blocked", Error("goal063.leak.runtime_ui_unity_broad_mutation_claim", "scope", "Broad Runtime/UI/Unity mutation is forbidden.")),
            Invalid("gamepackage_schema_mutation_claim", "Mutate public GamePackage schema for gameplay proof.", "blocked", Error("goal063.leak.gamepackage_schema_mutation_claim", "GamePackage", "Public GamePackage schema mutation is forbidden.")),
            Invalid("lua_arbitrary_execution_or_source_claim", "Generate or execute arbitrary Lua for the proof.", "blocked", Error("goal063.leak.lua_arbitrary_execution_or_source_claim", "Lua", "Arbitrary Lua execution/source generation is forbidden.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal063.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed order."))
        };

        return new InvalidGameplayConsequenceDiagnosticsMatrix
        {
            Passed = scenarios.Count == GameplayConsequenceDepthMatrixVocabulary.RequiredInvalidScenarioIds.Count
                && GameplayConsequenceDepthMatrixVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<GameplayConsequenceDiagnostic> Sort(IEnumerable<GameplayConsequenceDiagnostic> diagnostics) =>
        GameplayConsequenceDepthMatrixSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateFamilyShape(GameplayConsequenceCommandPlanRow row, List<GameplayConsequenceDiagnostic> diagnostics)
    {
        var types = row.Commands.Select(item => item.CommandType).ToHashSet(StringComparer.Ordinal);
        switch (row.FamilyId)
        {
            case "map_panel_rpg":
                Require(row, diagnostics, types, "travel/detail");
                Require(row, diagnostics, types, "quest/npc_event");
                Require(row, diagnostics, types, "inventory/reward");
                Require(row, diagnostics, types, "faction/social");
                break;
            case "survival_sandbox":
                Require(row, diagnostics, types, "survival/hazard_pressure");
                Require(row, diagnostics, types, "survival/resource_collect");
                Require(row, diagnostics, types, "survival/craft_mitigation");
                Require(row, diagnostics, types, "survival/recover");
                break;
            case "first_person_grid_dungeon":
                Require(row, diagnostics, types, "grid/traverse");
                Require(row, diagnostics, types, "grid/blocked_move");
                Require(row, diagnostics, types, "encounter/pressure");
                Require(row, diagnostics, types, "progression/unlock");
                break;
            default:
                diagnostics.Add(Error("goal063.plan.family_unknown", row.FamilyId, "Unsupported family id."));
                break;
        }
    }

    private static void Require(
        GameplayConsequenceCommandPlanRow row,
        List<GameplayConsequenceDiagnostic> diagnostics,
        HashSet<string> commandTypes,
        string commandType)
    {
        if (!commandTypes.Contains(commandType))
        {
            diagnostics.Add(Error("goal063.plan.family_shape_missing", row.RowId + "#" + commandType, "Family command shape is missing."));
        }
    }

    private static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "gameplay_consequence_goal=goal063",
        "gameplay_consequence_matrix_completed=true",
        "gameplay_consequence_depth_matrix_verification=required"
    ];

    private static InvalidGameplayConsequenceScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params GameplayConsequenceDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = Sort(diagnostics)
        };

    private static GameplayConsequenceDiagnostic Error(string code, string target, string message) =>
        GameplayConsequenceDiagnostic.Error(code, target, message);
}
