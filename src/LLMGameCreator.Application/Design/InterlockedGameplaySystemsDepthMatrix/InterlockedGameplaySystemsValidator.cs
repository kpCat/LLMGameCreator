namespace LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;

public sealed class InterlockedGameplaySystemsValidator
{
    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateSourceManifest(InterlockedGameplaySourceManifest manifest)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal065.gate.self_pass.forbidden", "source-manifest", "Goal 065 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal064AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "living_world_npc_faction_simulation_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal065.preflight.goal064_handoff_missing", "source-manifest", "Goal 064 acceptance by user handoff is required before Goal 065."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal065.gate.required_missing", "source-manifest", "Goal 065 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal065.source.matrix_counts_invalid", "source-manifest", "Goal 065 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewRowsConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed)
        {
            diagnostics.Add(Error("goal065.source.chain_incomplete", "source-manifest", "Goal 065 must consume Goal 060/061/062/063/064 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateRows(
        InterlockedGameplayRuleCatalog catalog,
        InterlockedGameplayRowPlanMatrix matrix,
        InterlockedPreviewExportPayload previewPayload)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        if (!catalog.Passed || catalog.RuleProfileCount != 3)
        {
            diagnostics.Add(Error("goal065.catalog.invalid", "system-rule-catalog", "Rule catalog must define all three family interlock profiles."));
        }

        if (!matrix.Passed || matrix.Accepted || matrix.RowCount != 9 || matrix.StateChangingRowCount != 9 || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal065.matrix.invalid", "row-plan-matrix", "Row matrix must contain 9 produced-for-review state-changing rows with distinct hashes."));
        }

        foreach (var familyId in InterlockedGameplaySystemsDepthMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in InterlockedGameplaySystemsDepthMatrixVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal065.matrix.row_missing", familyId + "/" + seedId, "Required interlocked gameplay row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal065.preview.payload_invalid", "preview-export-gameplay-payload", "Preview/export gameplay payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateLedgers(
        InterlockedGameplayLedger economyCrafting,
        InterlockedGameplayLedger combatProgression,
        InterlockedGameplayLedger statusEffect)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        if (!economyCrafting.Passed || economyCrafting.Entries.Count(item => item.Category == "economy") < 9 || economyCrafting.Entries.Count(item => item.Category == "crafting") < 9)
        {
            diagnostics.Add(Error("goal065.ledger.economy_crafting_invalid", "economy-crafting-ledger", "Economy and crafting ledgers must cover every row."));
        }

        if (!combatProgression.Passed || combatProgression.Entries.Count(item => item.Category == "combat") < 9 || combatProgression.Entries.Count(item => item.Category == "progression") < 9 || combatProgression.Entries.Count(item => item.Category == "inventory") < 9)
        {
            diagnostics.Add(Error("goal065.ledger.combat_progression_invalid", "combat-progression-ledger", "Combat, progression and inventory ledgers must cover every row."));
        }

        if (!statusEffect.Passed || statusEffect.Entries.Count(item => item.Category == "status") < 9)
        {
            diagnostics.Add(Error("goal065.ledger.status_invalid", "status-effect-ledger", "Status/effect ledger must cover every row."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateReplayAndVariance(
        InterlockedSaveLoadReplayProof saveLoadReplay,
        InterlockedVarianceMetrics variance)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        if (!saveLoadReplay.Passed
            || saveLoadReplay.RowCount != 9
            || saveLoadReplay.StateChangedRowCount != 9
            || saveLoadReplay.SaveLoadPassedRowCount != 9
            || saveLoadReplay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal065.replay.audit_invalid", "save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in saveLoadReplay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal065.state.before_after_equal", row.RowId, "Before and after state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SerializedAfterStateHash != row.RestoredAfterStateHash)
            {
                diagnostics.Add(Error("goal065.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal065.replay.mismatch", row.RowId, "Replay hashes must match for same input."));
            }
        }

        if (!variance.Passed
            || !variance.HashOnlyVarianceRejected
            || !variance.SameFamilySeedVariationPassed
            || !variance.CrossFamilyRuleVariationPassed
            || variance.DistinctAfterStateHashCount != 9
            || variance.DistinctRuleSetCount != 3)
        {
            diagnostics.Add(Error("goal065.variance.invalid", "variance-metrics", "Variance must prove same-family seed and cross-family rule differences beyond IDs/hashes."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateUnityCommandPlan(InterlockedUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal065.unity.command_plan_invalid", "unity-command-plan", "Unity command plan must cover all 9 interlocked gameplay rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal065.unity.marker_missing", marker, "Unity command plan is missing a required global marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (row.EconomyDeltaIds.Count == 0 || row.CraftingDeltaIds.Count == 0 || row.CombatDeltaIds.Count == 0 || row.ProgressionDeltaIds.Count == 0 || row.StatusDeltaIds.Count == 0)
            {
                diagnostics.Add(Error("goal065.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include economy, crafting, combat, progression and status deltas."));
            }

            foreach (var marker in new[]
            {
                "interlocked_gameplay_row=" + row.RowId,
                "interlocked_economy_delta=" + row.RowId,
                "interlocked_crafting_delta=" + row.RowId,
                "interlocked_combat_delta=" + row.RowId,
                "interlocked_progression_delta=" + row.RowId,
                "interlocked_status_delta=" + row.RowId,
                "interlocked_replay_verified=" + row.RowId,
                "interlocked_gameplay_row_completed=" + row.RowId
            })
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal065.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, system-delta, replay and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateUnityProof(
        InterlockedUnityCommandPlan commandPlan,
        InterlockedUnityProofSummary proof)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal065.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 065 marker."));
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
                diagnostics.Add(Error("goal065.unity.proof_inconsistent", "unity-player-proof", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal065.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<InterlockedGameplayDiagnostic> ValidateInvalidMatrix(InvalidInterlockedGameplayDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>();
        foreach (var scenarioId in InterlockedGameplaySystemsDepthMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal065.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal065.invalid.matrix_failed", "invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public InvalidInterlockedGameplayDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidInterlockedGameplayScenario>
        {
            Invalid("missing_goal060_source", "Remove the Goal 060 package row.", "blocked", Error("goal065.source.goal060_row_missing", "Goal060", "Goal 060 source is required.")),
            Invalid("missing_goal061_source", "Remove the Goal 061 review row.", "blocked", Error("goal065.source.goal061_row_missing", "Goal061", "Goal 061 source is required.")),
            Invalid("missing_goal062_source", "Remove the Goal 062 spatial row.", "blocked", Error("goal065.source.goal062_row_missing", "Goal062", "Goal 062 source is required.")),
            Invalid("missing_goal063_source", "Remove the Goal 063 gameplay row.", "blocked", Error("goal065.source.goal063_row_missing", "Goal063", "Goal 063 source is required.")),
            Invalid("missing_goal064_source", "Remove the Goal 064 living-world row.", "blocked", Error("goal065.source.goal064_row_missing", "Goal064", "Goal 064 source is required.")),
            Invalid("fake_family_id", "Inject a family id outside the accepted matrix.", "rejected", Error("goal065.source.fake_family_id", "familyId", "Family id must come from the accepted matrix.")),
            Invalid("fake_seed_id", "Inject a seed id outside seed_alpha/beta/gamma.", "rejected", Error("goal065.source.fake_seed_id", "seedId", "Seed id must come from the accepted matrix.")),
            Invalid("duplicate_row_id", "Duplicate a matrix row id.", "rejected", Error("goal065.matrix.duplicate_row_id", "rowId", "Row ids must be unique.")),
            Invalid("non_state_changing_row", "Emit a row whose before and after hashes are equal.", "rejected", Error("goal065.state.non_state_changing_row", "row", "Every row must change state.")),
            Invalid("economy_delta_without_source_trace", "Emit an economy delta without Goal source refs.", "rejected", Error("goal065.economy.source_trace_missing", "economy", "Economy deltas must carry source traces.")),
            Invalid("crafting_delta_without_resource_input_output", "Emit a crafting delta without input/output values.", "rejected", Error("goal065.crafting.input_output_missing", "crafting", "Crafting deltas must include resource input and output.")),
            Invalid("combat_delta_without_outcome", "Emit a combat delta without an outcome.", "rejected", Error("goal065.combat.outcome_missing", "combat", "Combat deltas require an outcome.")),
            Invalid("progression_delta_without_causal_trace", "Emit a progression delta without source refs.", "rejected", Error("goal065.progression.causal_trace_missing", "progression", "Progression deltas require causal source refs.")),
            Invalid("replay_mismatch", "Replay the same row input with a different hash.", "rejected", Error("goal065.replay.mismatch", "replay", "Replay must be deterministic.")),
            Invalid("save_load_mismatch", "Deserialize a different after-state hash.", "rejected", Error("goal065.save_load.mismatch", "serializer", "Save/load must preserve after-state hash.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal065.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed.")),
            Invalid("unsafe_path", "Use traversal or absolute path in a source/staging path.", "rejected", Error("goal065.path.unsafe", "../", "Paths must stay repository-relative and traversal-free.")),
            Invalid("provider_llm_rag_media_generation_claim", "Claim provider, LLM, RAG or media generation.", "blocked", Error("goal065.leak.provider_llm_rag_media_generation_claim", "scope", "Provider, LLM, RAG and media generation are forbidden.")),
            Invalid("runtime_ui_gamepackage_schema_mutation_claim", "Mutate Runtime/UI/public GamePackage schema.", "blocked", Error("goal065.leak.runtime_ui_gamepackage_schema_mutation_claim", "scope", "Runtime/UI/GamePackage schema mutation is forbidden.")),
            Invalid("unity_broad_mutation_claim", "Claim broad Unity gameplay systems.", "blocked", Error("goal065.leak.unity_broad_mutation_claim", "Unity", "Only narrow Alpha marker loading is allowed.")),
            Invalid("arbitrary_lua_execution_claim", "Execute arbitrary Lua for proof.", "blocked", Error("goal065.leak.arbitrary_lua_execution_claim", "Lua", "Arbitrary Lua execution is forbidden."))
        };

        return new InvalidInterlockedGameplayDiagnosticsMatrix
        {
            Passed = scenarios.Count == InterlockedGameplaySystemsDepthMatrixVocabulary.RequiredInvalidScenarioIds.Count
                && InterlockedGameplaySystemsDepthMatrixVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<InterlockedGameplayDiagnostic> Sort(IEnumerable<InterlockedGameplayDiagnostic> diagnostics) =>
        InterlockedGameplaySystemsSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateRow(InterlockedGameplayRow row, List<InterlockedGameplayDiagnostic> diagnostics)
    {
        var categories = row.Deltas.Select(item => item.Category).ToHashSet(StringComparer.Ordinal);
        foreach (var required in InterlockedGameplaySystemsRuleCatalogBuilder.RequiredCategories())
        {
            if (!categories.Contains(required))
            {
                diagnostics.Add(Error("goal065.row.category_missing", row.RowId + "#" + required, "Every row must include all required interlocked delta categories."));
            }
        }

        if (!row.StateChanging || row.BeforeState.StateHash == row.AfterState.StateHash || row.Deltas.Count < 7)
        {
            diagnostics.Add(Error("goal065.state.non_state_changing_row", row.RowId, "Every row requires passed interlocked state deltas and different before/after hashes."));
        }

        if (row.Deltas.Any(item => item.SourceRefs.Count == 0 || string.IsNullOrWhiteSpace(item.CausalTrace)))
        {
            diagnostics.Add(Error("goal065.delta.source_trace_missing", row.RowId, "Every delta must carry causal source references."));
        }

        if (!row.Deltas.Any(item => item.Category == "crafting" && item.BeforeValue != item.AfterValue && !string.IsNullOrWhiteSpace(item.Outcome)))
        {
            diagnostics.Add(Error("goal065.crafting.input_output_missing", row.RowId, "Crafting deltas must include input/output change and an outcome."));
        }

        if (!row.Deltas.Any(item => item.Category == "combat" && !string.IsNullOrWhiteSpace(item.Outcome)))
        {
            diagnostics.Add(Error("goal065.combat.outcome_missing", row.RowId, "Combat delta must include an outcome."));
        }

        ValidateFamilyDepth(row, diagnostics);
    }

    private static void ValidateFamilyDepth(InterlockedGameplayRow row, List<InterlockedGameplayDiagnostic> diagnostics)
    {
        var outcomes = string.Join("|", row.Deltas.Select(item => item.Outcome));
        switch (row.FamilyId)
        {
            case "map_panel_rpg":
                Require(outcomes, row, diagnostics, "trade/work", "goal065.family.map_panel.trade_work_missing");
                Require(outcomes, row, diagnostics, "conflict", "goal065.family.map_panel.conflict_missing");
                Require(outcomes, row, diagnostics, "social", "goal065.family.map_panel.social_missing");
                break;
            case "survival_sandbox":
                Require(outcomes, row, diagnostics, "hazard", "goal065.family.survival.hazard_missing");
                Require(outcomes, row, diagnostics, "resource", "goal065.family.survival.resource_missing");
                Require(outcomes, row, diagnostics, "condition", "goal065.family.survival.status_missing");
                break;
            case "first_person_grid_dungeon":
                Require(outcomes, row, diagnostics, "encounter", "goal065.family.dungeon.encounter_missing");
                Require(outcomes, row, diagnostics, "key", "goal065.family.dungeon.key_missing");
                Require(outcomes, row, diagnostics, "blocked/valid movement", "goal065.family.dungeon.movement_missing");
                break;
            default:
                diagnostics.Add(Error("goal065.family.unknown", row.RowId, "Unsupported family id."));
                break;
        }
    }

    private static void Require(string haystack, InterlockedGameplayRow row, List<InterlockedGameplayDiagnostic> diagnostics, string fragment, string code)
    {
        if (!haystack.Contains(fragment, StringComparison.Ordinal))
        {
            diagnostics.Add(Error(code, row.RowId, "Required family-specific outcome is missing: " + fragment));
        }
    }

    private static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "interlocked_gameplay_loaded=true",
        "interlocked_gameplay_completed=true",
        "review_package_proof=goal065",
        "interlocked_gameplay_systems_depth_matrix_verification=required"
    ];

    private static InvalidInterlockedGameplayScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params InterlockedGameplayDiagnostic[] diagnostics) =>
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

    private static InterlockedGameplayDiagnostic Error(string code, string target, string message) =>
        InterlockedGameplayDiagnostic.Error(code, target, message);
}
