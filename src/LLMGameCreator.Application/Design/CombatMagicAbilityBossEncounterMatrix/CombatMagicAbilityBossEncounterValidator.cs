namespace LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;

public sealed class CombatMagicAbilityBossEncounterValidator
{
    public IReadOnlyList<CombatMagicDiagnostic> ValidateSourceManifest(CombatMagicSourceManifest manifest)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal068.gate.self_pass.forbidden", "source-manifest", "Goal 068 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal067AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "programmatic_narrative_quest_dialogue_event_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal068.preflight.goal067_handoff_missing", "source-manifest", "Goal 067 acceptance by user handoff is required before Goal 068."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == CombatMagicAbilityBossEncounterVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal068.gate.required_missing", "source-manifest", "Goal 068 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal068.source.matrix_counts_invalid", "source-manifest", "Goal 068 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewPackageRcConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed
            || !manifest.Goal065InterlockedRowsConsumed
            || !manifest.Goal066SettlementRowsConsumed
            || !manifest.Goal067NarrativeRowsConsumed)
        {
            diagnostics.Add(Error("goal068.source.chain_incomplete", "source-manifest", "Goal 068 must consume Goal 060/061/062/063/064/065/066/067 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateCatalogs(
        CombatMagicAbilityTraitCatalog abilityTraitCatalog,
        CombatMagicStatusEffectCatalog statusEffectCatalog,
        CombatMagicBossEncounterPhaseCatalog bossPhaseCatalog)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (!abilityTraitCatalog.Passed
            || abilityTraitCatalog.ActiveAbilityCount < 9
            || abilityTraitCatalog.PassiveTraitCount < 3)
        {
            diagnostics.Add(Error("goal068.catalog.ability_trait_invalid", "ability-trait-catalog", "Ability/trait catalog must cover active abilities and passive traits for all families."));
        }

        if (!statusEffectCatalog.Passed || statusEffectCatalog.StatusEffectCount < 9)
        {
            diagnostics.Add(Error("goal068.catalog.status_effect_invalid", "status-effect-catalog", "Status catalog must cover status/effect stack behavior across all families."));
        }

        if (!bossPhaseCatalog.Passed || bossPhaseCatalog.PhaseCount < 6)
        {
            diagnostics.Add(Error("goal068.catalog.boss_phase_invalid", "boss-encounter-phase-catalog", "Boss/elite phase catalog must include transition markers."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateRows(
        CombatMagicAbilityTraitCatalog abilityTraitCatalog,
        CombatMagicStatusEffectCatalog statusEffectCatalog,
        CombatMagicBossEncounterPhaseCatalog bossPhaseCatalog,
        CombatMagicRowMatrix matrix,
        CombatMagicPreviewExportPayload previewPayload)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (!matrix.Passed
            || matrix.Accepted
            || matrix.RowCount != 9
            || matrix.StateChangingRowCount != 9
            || matrix.BossEliteRowCount < 3
            || matrix.MagicStatusRowCount < 3
            || matrix.ResourceGearCraftingRowCount < 3
            || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal068.matrix.invalid", "combat-magic-row-matrix", "Combat/magic row matrix must contain 9 produced-for-review state-changing rows with required coverage."));
        }

        if (matrix.Rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal068.identity.duplicate_row_id", "rowId", "Combat/magic row ids must be unique."));
        }

        foreach (var familyId in CombatMagicAbilityBossEncounterVocabulary.FamilyIds)
        {
            foreach (var seedId in CombatMagicAbilityBossEncounterVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal068.matrix.row_missing", familyId + "/" + seedId, "Required combat/magic row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, abilityTraitCatalog, statusEffectCatalog, bossPhaseCatalog, diagnostics);
        }

        if (!matrix.SameFamilySeedVariancePassed || !matrix.FamilyCombatFlavorVariancePassed)
        {
            diagnostics.Add(Error("goal068.variance.invalid", "combat-magic-row-matrix", "Variance must prove same-family seed and cross-family combat flavor differences beyond ids/hashes."));
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal068.preview.payload_invalid", "combat-magic-preview-export-payload", "Preview/export combat payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateLedgers(
        CombatMagicLedger progressionLootLedger,
        CombatMagicLedger counterplayLedger)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (!progressionLootLedger.Passed || progressionLootLedger.Entries.Count(item => item.LedgerKind == "progression_loot") < 9)
        {
            diagnostics.Add(Error("goal068.ledger.progression_loot_invalid", "combat-magic-progression-loot-ledger", "Progression/loot ledger must include every row."));
        }

        if (!counterplayLedger.Passed || counterplayLedger.Entries.Count(item => item.LedgerKind == "counterplay") < 9)
        {
            diagnostics.Add(Error("goal068.ledger.counterplay_invalid", "combat-magic-counterplay-ledger", "Counterplay ledger must include mitigation for every row."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateReplay(CombatMagicSaveLoadReplayProof replay)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.StateChangedRowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal068.replay.audit_invalid", "combat-magic-save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal068.state.before_after_equal", row.RowId, "Before and after combat/magic state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SerializedAfterStateHash != row.RestoredAfterStateHash)
            {
                diagnostics.Add(Error("goal068.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve combat/magic after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal068.replay.mismatch", row.RowId, "Replay hashes must match for same combat/magic input."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateUnityCommandPlan(CombatMagicUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal068.unity.command_plan_invalid", "combat-magic-unity-command-plan", "Unity command plan must cover all 9 combat/magic rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal068.unity.marker_missing", marker, "Unity command plan is missing a required global combat/magic marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.AbilityUseId)
                || string.IsNullOrWhiteSpace(row.StatusApplicationId)
                || string.IsNullOrWhiteSpace(row.ProgressionId)
                || row.RoundStepIds.Count < 2)
            {
                diagnostics.Add(Error("goal068.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include row, rounds, ability, status and progression ids."));
            }

            foreach (var marker in RowMarkers(row))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal068.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, round, ability, status, progression and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateUnityProof(
        CombatMagicUnityCommandPlan commandPlan,
        CombatMagicUnityProofSummary proof)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal068.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 068 marker."));
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
                diagnostics.Add(Error("goal068.unity.proof_inconsistent", "combat-magic-unity-player-proof-summary", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal068.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<CombatMagicDiagnostic> ValidateInvalidMatrix(InvalidCombatMagicDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<CombatMagicDiagnostic>();
        foreach (var scenarioId in CombatMagicAbilityBossEncounterVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal068.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal068.invalid.matrix_failed", "combat-magic-invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "combat_magic_matrix_loaded=goal068",
        "combat_magic_matrix_completed=true",
        "review_package_proof=goal068",
        "combat_magic_ability_boss_encounter_matrix_verification=required"
    ];

    public static IReadOnlyList<CombatMagicDiagnostic> Sort(IEnumerable<CombatMagicDiagnostic> diagnostics) =>
        CombatMagicAbilityBossEncounterSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateRow(
        CombatMagicRow row,
        CombatMagicAbilityTraitCatalog abilityTraitCatalog,
        CombatMagicStatusEffectCatalog statusEffectCatalog,
        CombatMagicBossEncounterPhaseCatalog bossPhaseCatalog,
        List<CombatMagicDiagnostic> diagnostics)
    {
        if (!row.StateChanging || row.BeforeState.StateHash == row.AfterState.StateHash)
        {
            diagnostics.Add(Error("goal068.state.non_state_changing_row", row.RowId, "Every combat/magic row must change before/after state."));
        }

        if (row.ChangedCategories.Count < 3 || row.StateDeltas.Count(item => item.Passed) < 5)
        {
            diagnostics.Add(Error("goal068.state_delta.too_shallow", row.RowId, "Every row must change at least three categories and carry multiple state deltas."));
        }

        if (row.InitialCombatants.Count < 2 || row.FinalCombatants.Count < 2)
        {
            diagnostics.Add(Error("goal068.combatants.missing", row.RowId, "Every row needs before/after combatant snapshots."));
        }

        if (row.ActiveAbilities.Count == 0 || row.ActiveAbilities.Any(item => !abilityTraitCatalog.ActiveAbilities.Any(definition => definition.AbilityId == item.AbilityId)))
        {
            diagnostics.Add(Error("goal068.ability.missing_or_fake", row.RowId, "Every active ability must resolve to the ability catalog."));
        }

        if (row.PassiveTraits.Count == 0 || row.PassiveTraits.Any(item => !abilityTraitCatalog.PassiveTraits.Any(definition => definition.TraitId == item.TraitId)))
        {
            diagnostics.Add(Error("goal068.passive_trait.missing_or_fake", row.RowId, "Every row needs a passive trait resolved from the catalog."));
        }

        if (row.StatusEffects.Count == 0
            || row.StatusEffects.Any(item => !statusEffectCatalog.StatusEffects.Any(definition => definition.StatusEffectId == item.StatusEffectId)
                || item.AfterStacks <= item.BeforeStacks
                || string.IsNullOrWhiteSpace(item.DurationChange)))
        {
            diagnostics.Add(Error("goal068.status.shape_invalid", row.RowId, "Every row needs a catalog-backed status/effect stack or duration delta."));
        }

        if (row.DamageEffectPackets.Count == 0 || row.DamageEffectPackets.Any(item => item.AmountAfterMitigation <= 0 || item.AmountBeforeMitigation < item.AmountAfterMitigation))
        {
            diagnostics.Add(Error("goal068.damage.packet_invalid", row.RowId, "Damage/effect packet must have positive mitigated damage and valid mitigation."));
        }

        if (row.CooldownCosts.Count == 0 || row.CooldownCosts.Any(item => item.CostPaid <= 0 || item.ResourceAfter < 0 || item.CooldownAfter <= item.CooldownBefore))
        {
            diagnostics.Add(Error("goal068.cost.cooldown_invalid", row.RowId, "Cooldown/cost must pay a positive resource and increase cooldown without underflow."));
        }

        if (row.ResistanceWeaknesses.Count == 0 || row.ResistanceWeaknesses.Any(item => string.IsNullOrWhiteSpace(item.ResistanceKind) || string.IsNullOrWhiteSpace(item.WeaknessKind)))
        {
            diagnostics.Add(Error("goal068.resistance_weakness.missing", row.RowId, "Every row needs resistance/weakness evidence."));
        }

        if (row.BossPhases.Count == 0
            || row.BossPhases.Any(item => !item.TransitionApplied
                || item.BeforePhaseState == item.AfterPhaseState
                || !bossPhaseCatalog.Phases.Any(definition => item.PhaseId.StartsWith(definition.PhaseId, StringComparison.Ordinal))))
        {
            diagnostics.Add(Error("goal068.boss.phase_without_transition", row.RowId, "Boss/elite phase records require catalog-backed transitions."));
        }

        if (row.RoundPhaseResults.Count < 2 || !IsStrictlyOrdered(row.RoundPhaseResults.Select(item => item.Order)))
        {
            diagnostics.Add(Error("goal068.rounds.missing", row.RowId, "Every row needs at least two ordered rounds or phases."));
        }

        if (row.CounterplayRecords.Count == 0 || row.CounterplayRecords.Any(item => !item.Passed || string.IsNullOrWhiteSpace(item.MitigatedPacketId)))
        {
            diagnostics.Add(Error("goal068.counterplay.missing", row.RowId, "Every row needs counterplay or mitigation evidence."));
        }

        if (row.LootProgressionRecords.Count == 0 || row.LootProgressionRecords.Any(item => !item.Passed || item.BeforeValue == item.AfterValue))
        {
            diagnostics.Add(Error("goal068.loot_progression.missing", row.RowId, "Every row needs loot/progression state delta evidence."));
        }

        if (row.NonCombatConsequences.Count == 0 || row.NonCombatConsequences.Any(item => !item.Passed || item.BeforeValue == item.AfterValue))
        {
            diagnostics.Add(Error("goal068.non_combat_consequence.missing", row.RowId, "Every row needs faction/narrative/living-world/settlement/survival consequence evidence."));
        }

        if (string.IsNullOrWhiteSpace(row.SourceNarrativeRowRef)
            || string.IsNullOrWhiteSpace(row.SourceInterlockedGameplayRowRef)
            || string.IsNullOrWhiteSpace(row.SourceSettlementRowRef)
            || string.IsNullOrWhiteSpace(row.SourcePackageRowRef)
            || string.IsNullOrWhiteSpace(row.SourceSpatialDetailRowRef))
        {
            diagnostics.Add(Error("goal068.source.ref_missing", row.RowId, "Combat/magic rows must link narrative, interlocked, settlement, package and spatial rows."));
        }

        ValidateNoFinalProse(row, diagnostics);
    }

    private static void ValidateNoFinalProse(CombatMagicRow row, List<CombatMagicDiagnostic> diagnostics)
    {
        var serialized = CombatMagicAbilityBossEncounterHash.Serialize(row);
        var forbidden = new[]
        {
            "lineText",
            "finalDialogue",
            "finalText",
            "generatedProse",
            "proseText",
            "providerCall",
            "llmCall",
            "ragCall",
            "generatedLua",
            "luaSource"
        };
        foreach (var token in forbidden)
        {
            if (serialized.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("goal068.prose.final_leakage", row.RowId + "#" + token, "Goal 068 may not emit final prose or external runtime/generator dependency claims."));
            }
        }
    }

    private static bool IsStrictlyOrdered(IEnumerable<int> orders)
    {
        var previous = 0;
        foreach (var order in orders)
        {
            if (order <= previous)
            {
                return false;
            }

            previous = order;
        }

        return true;
    }

    private static IReadOnlyList<string> RowMarkers(CombatMagicUnityCommandRow row)
    {
        var markers = new List<string>
        {
            "combat_magic_row_loaded=" + row.RowId,
            "combat_magic_family=" + row.FamilyId,
            "combat_magic_seed=" + row.SeedId,
            "combat_magic_ability_resolved=" + row.AbilityUseId,
            "combat_magic_status_delta=" + row.StatusApplicationId,
            "combat_magic_progression_delta=" + row.ProgressionId,
            "combat_magic_row_completed=" + row.RowId
        };
        markers.AddRange(row.RoundStepIds.Select(step => "combat_magic_round_step=" + step));
        return markers;
    }

    private static CombatMagicDiagnostic Error(string code, string target, string message) =>
        CombatMagicDiagnostic.Error(code, target, message);
}
