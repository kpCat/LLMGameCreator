using System.Text;

namespace LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;

public sealed class CombatMagicAbilityBossEncounterProjector
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public CombatMagicSourceManifest BuildSourceManifest(CombatMagicSourceBundle source)
    {
        var diagnostics = new List<CombatMagicDiagnostic>(source.Diagnostics)
        {
            Info("goal068.preflight.goal067_handoff_recorded", "programmatic_narrative_quest_dialogue_event_matrix_verification", "Goal 067 is recorded as accepted by user handoff before Goal 068."),
            Info("goal068.source.loaded", "Goal060-067", "Goal 068 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066/067 compact evidence.")
        };

        return new CombatMagicSourceManifest
        {
            Accepted = false,
            Goal067AcceptedByUserHandoff = source.Goal067AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewPackageRcConsumed = source.Goal061ReviewPackageRcConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal065InterlockedRowsConsumed = source.Goal065InterlockedRowsConsumed,
            Goal066SettlementRowsConsumed = source.Goal066SettlementRowsConsumed,
            Goal067NarrativeRowsConsumed = source.Goal067NarrativeRowsConsumed,
            Goal067UnityProofConsumed = source.Goal067UnityProofConsumed,
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
                Gate("interlocked_gameplay_systems_depth_matrix_verification", "passed", "user_handoff", "Goal 066 handoff"),
                Gate("settlement_construction_destruction_production_matrix_verification", "passed", "user_handoff", "Goal 067 handoff"),
                Gate("programmatic_narrative_quest_dialogue_event_matrix_verification", "passed", "user_handoff", "Goal 068 preflight handoff"),
                Gate(CombatMagicAbilityBossEncounterVocabulary.FinalGate, "required", "current_goal_manual_gate", CombatMagicAbilityBossEncounterVocabulary.RelativeOutputDirectory + "/" + CombatMagicAbilityBossEncounterEvidenceService.ReportMarkdownFileName),
                Gate("semantic_pack_composition_blueprint_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 031 remains not passed"),
                Gate("dynamic_semantic_feature_system_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 032 remains not passed")
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = CombatMagicAbilityBossEncounterSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public CombatMagicAbilityTraitCatalog BuildAbilityTraitCatalog()
    {
        var abilities = CombatProfiles.AllFamilyProfiles()
            .SelectMany(profile => profile.AbilityKinds.Select((abilityKind, index) => new ActiveAbilityDefinition
            {
                AbilityId = "ability/" + profile.SafeFamily + "/" + abilityKind,
                FamilyId = profile.FamilyId,
                AbilityKind = abilityKind,
                ResourceKind = profile.ResourceKind,
                BaseCost = profile.CostPaid + index,
                BaseCooldown = 1 + index,
                Tags = profile.AbilityTags.OrderBy(item => item, StringComparer.Ordinal).ToList()
            }))
            .OrderBy(item => item.AbilityId, StringComparer.Ordinal)
            .ToList();

        var traits = CombatProfiles.AllFamilyProfiles()
            .Select(profile => new PassiveTraitDefinition
            {
                TraitId = "trait/" + profile.SafeFamily + "/" + profile.PassiveTraitKind,
                FamilyId = profile.FamilyId,
                TraitKind = profile.PassiveTraitKind,
                ResistanceRefs = ["resistance/" + profile.SafeFamily + "/" + profile.ResistanceKind],
                WeaknessRefs = ["weakness/" + profile.SafeFamily + "/" + profile.WeaknessKind]
            })
            .OrderBy(item => item.TraitId, StringComparer.Ordinal)
            .ToList();

        return new CombatMagicAbilityTraitCatalog
        {
            Passed = abilities.Count >= 9
                && traits.Count == 3
                && abilities.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() == 3
                && abilities.All(item => item.BaseCost > 0 && item.BaseCooldown > 0 && item.Tags.Count > 0),
            ActiveAbilityCount = abilities.Count,
            PassiveTraitCount = traits.Count,
            ActiveAbilities = abilities,
            PassiveTraits = traits
        };
    }

    public CombatMagicStatusEffectCatalog BuildStatusEffectCatalog()
    {
        var statuses = CombatProfiles.AllFamilyProfiles()
            .SelectMany(profile => profile.StatusKinds.Select(statusKind => new StatusEffectDefinition
            {
                StatusEffectId = "status/" + profile.SafeFamily + "/" + statusKind,
                FamilyId = profile.FamilyId,
                EffectKind = statusKind,
                StackPolicy = profile.StatusStackPolicy,
                MaxStacks = profile.MaxStacks,
                DeltaCategories = profile.StatusDeltaCategories
            }))
            .OrderBy(item => item.StatusEffectId, StringComparer.Ordinal)
            .ToList();

        return new CombatMagicStatusEffectCatalog
        {
            Passed = statuses.Count >= 9
                && statuses.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() == 3
                && statuses.All(item => item.MaxStacks > 0 && item.DeltaCategories.Count >= 2),
            StatusEffectCount = statuses.Count,
            StatusEffects = statuses
        };
    }

    public CombatMagicBossEncounterPhaseCatalog BuildBossPhaseCatalog()
    {
        var phases = CombatProfiles.AllFamilyProfiles()
            .SelectMany(profile => profile.BossPhaseKinds.Select((phaseKind, index) => new BossEncounterPhaseDefinition
            {
                PhaseId = "boss-phase/" + profile.SafeFamily + "/" + phaseKind,
                FamilyId = profile.FamilyId,
                PhaseKind = phaseKind,
                Trigger = index == 0 ? "health_threshold_70" : "counterplay_or_position_shift",
                TransitionMarkers =
                [
                    "phase_entered/" + phaseKind,
                    "phase_resolved/" + phaseKind
                ]
            }))
            .OrderBy(item => item.PhaseId, StringComparer.Ordinal)
            .ToList();

        return new CombatMagicBossEncounterPhaseCatalog
        {
            Passed = phases.Count >= 6
                && phases.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() == 3
                && phases.All(item => item.TransitionMarkers.Count >= 2),
            PhaseCount = phases.Count,
            Phases = phases
        };
    }

    public IReadOnlyList<CombatMagicRow> BuildRows(CombatMagicSourceBundle source) =>
        source.Rows
            .OrderBy(item => CombatMagicAbilityBossEncounterVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => CombatMagicAbilityBossEncounterVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public CombatMagicRowMatrix BuildRowMatrix(IReadOnlyList<CombatMagicRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        var sameFamilySeedVariance = SameFamilySeedVariancePassed(rows);
        var familyFlavorVariance = FamilyCombatFlavorVariancePassed(rows);
        var bossRows = rows.Count(item => item.BossOrElitePhaseRow);
        var magicRows = rows.Count(item => item.MagicStatusHeavyRow);
        var resourceRows = rows.Count(item => item.ResourceGearCraftingLinkedRow);

        return new CombatMagicRowMatrix
        {
            Passed = rows.Count == 9
                && rows.All(item => item.StateChanging && item.ChangedCategories.Count >= 3 && item.NoFinalProse)
                && bossRows >= 3
                && magicRows >= 3
                && resourceRows >= 3
                && distinctHashes == 9
                && sameFamilySeedVariance
                && familyFlavorVariance,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            BossEliteRowCount = bossRows,
            MagicStatusRowCount = magicRows,
            ResourceGearCraftingRowCount = resourceRows,
            DistinctRowHashCount = distinctHashes,
            SameFamilySeedVariancePassed = sameFamilySeedVariance,
            FamilyCombatFlavorVariancePassed = familyFlavorVariance,
            Rows = rows
        };
    }

    public CombatMagicSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<CombatMagicRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new CombatMagicSaveLoadReplayProof
        {
            Passed = proofRows.Count == 9 && proofRows.All(item => item.BeforeAfterStateChanged && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangedRowCount = proofRows.Count(item => item.BeforeAfterStateChanged),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    public CombatMagicLedger BuildProgressionLootLedger(IReadOnlyList<CombatMagicRow> rows) =>
        Ledger(
            "progression_loot",
            rows.SelectMany(row => row.LootProgressionRecords.Select(item => new CombatMagicLedgerEntry
            {
                EntryId = "progression_loot/" + item.LootProgressionId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "progression_loot",
                SubjectId = item.ProgressionId,
                BeforeValue = item.BeforeValue,
                AfterValue = item.AfterValue,
                Outcome = item.LootId,
                SourceRefs = item.SourceRefs,
                StateDeltaRefs = row.StateDeltas.Where(delta => delta.Category is "loot" or "progression").Select(delta => delta.DeltaId).ToList(),
                Passed = item.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.LootProgressionRecords.Count >= 1));

    public CombatMagicLedger BuildCounterplayLedger(IReadOnlyList<CombatMagicRow> rows) =>
        Ledger(
            "counterplay",
            rows.SelectMany(row => row.CounterplayRecords.Select(item => new CombatMagicLedgerEntry
            {
                EntryId = "counterplay/" + item.CounterplayId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "counterplay",
                SubjectId = item.CounterplayId,
                BeforeValue = "incoming/" + item.MitigatedPacketId,
                AfterValue = item.Result,
                Outcome = item.CounterplayKind,
                SourceRefs = item.SourceRefs,
                StateDeltaRefs = row.StateDeltas.Where(delta => delta.Category is "health" or "armor" or "status").Select(delta => delta.DeltaId).ToList(),
                Passed = item.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.CounterplayRecords.Count >= 1));

    public CombatMagicPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<CombatMagicRow> rows)
    {
        var payloadRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new CombatMagicPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                EncounterId = item.EncounterId,
                PackageRef = item.SourcePackageRowRef,
                SpatialRef = item.SourceSpatialDetailRowRef,
                NarrativeRef = item.SourceNarrativeRowRef,
                CombatMagicAfterStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "combat_magic_row_loaded=" + item.RowId,
                    "combat_magic_after_state_hash=" + item.AfterState.StateHash,
                    "combat_magic_round_count=" + item.RoundPhaseResults.Count,
                    "combat_magic_status_count=" + item.StatusEffects.Count,
                    "combat_magic_progression_count=" + item.LootProgressionRecords.Count
                ]
            })
            .ToList();

        return new CombatMagicPreviewExportPayload
        {
            Passed = payloadRows.Count == 9 && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.CombatMagicAfterStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    public CombatMagicUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<CombatMagicRow> rows)
    {
        var commandRows = rows
            .OrderBy(item => CombatMagicAbilityBossEncounterVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => CombatMagicAbilityBossEncounterVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var firstAbility = row.ActiveAbilities.First();
                var firstStatus = row.StatusEffects.First();
                var firstProgression = row.LootProgressionRecords.First();
                var roundSteps = row.RoundPhaseResults.Select(item => item.RoundId).OrderBy(item => item, StringComparer.Ordinal).ToList();
                var markers = new List<string>
                {
                    "combat_magic_row_loaded=" + row.RowId,
                    "combat_magic_family=" + row.FamilyId,
                    "combat_magic_seed=" + row.SeedId,
                    "combat_magic_ability_resolved=" + firstAbility.AbilityUseId,
                    "combat_magic_status_delta=" + firstStatus.StatusApplicationId,
                    "combat_magic_progression_delta=" + firstProgression.LootProgressionId,
                    "combat_magic_row_completed=" + row.RowId
                };
                markers.AddRange(roundSteps.Select(step => "combat_magic_round_step=" + step));

                return new CombatMagicUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    EncounterId = row.EncounterId,
                    AbilityUseId = firstAbility.AbilityUseId,
                    StatusApplicationId = firstStatus.StatusApplicationId,
                    ProgressionId = firstProgression.LootProgressionId,
                    RoundStepIds = roundSteps,
                    MarkerCommands = markers.Select((marker, index) => new CombatMagicUnityMarkerCommandRecord
                    {
                        CommandId = "goal068/" + Safe(row.FamilyId) + "/" + Safe(row.SeedId) + "/marker/" + (index + 1).ToString("00"),
                        MarkerKind = marker.Split('=')[0],
                        MarkerValue = marker,
                        Order = index + 1
                    }).ToList(),
                    ExpectedPlayerMarkers = markers.OrderBy(item => item, StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "combat_magic_matrix_loaded=goal068",
            "combat_magic_matrix_completed=true",
            "review_package_proof=goal068",
            "combat_magic_ability_boss_encounter_matrix_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new CombatMagicUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => !string.IsNullOrWhiteSpace(item.AbilityUseId)
                    && !string.IsNullOrWhiteSpace(item.StatusApplicationId)
                    && !string.IsNullOrWhiteSpace(item.ProgressionId)
                    && item.RoundStepIds.Count >= 2),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public InvalidCombatMagicDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidCombatMagicScenario>
        {
            Invalid("missing_goal067_source", "Remove the Goal 067 narrative row source.", "blocked", Error("goal068.source.goal067_row_missing", "Goal067", "Goal 067 narrative source is required.")),
            Invalid("fake_family_seed", "Use a family/seed outside the proven 3 x 3 matrix.", "rejected", Error("goal068.source.fake_family_seed", "familySeed", "Only the proven family/seed matrix may be consumed.")),
            Invalid("duplicate_row_id", "Duplicate a row id across two combat/magic rows.", "rejected", Error("goal068.identity.duplicate_row_id", "rowId", "Combat/magic row ids must be unique.")),
            Invalid("missing_active_ability", "Remove the active ability from a row.", "rejected", Error("goal068.ability.missing", "activeAbility", "Every row requires at least one active ability.")),
            Invalid("missing_state_delta", "Produce a row with unchanged before/after state.", "rejected", Error("goal068.state.non_state_changing_row", "state", "Every row must change combat/magic state.")),
            Invalid("fake_ability_id", "Reference an ability absent from the ability catalog.", "rejected", Error("goal068.ability.fake_id", "abilityId", "Ability ids must resolve to the catalog.")),
            Invalid("illegal_status_effect_shape", "Use a status effect without stack/duration transition.", "rejected", Error("goal068.status.shape_invalid", "statusEffect", "Status effects require legal stack or duration deltas.")),
            Invalid("cooldown_cost_underflow", "Spend more resource than the combatant has.", "rejected", Error("goal068.cost.underflow", "cooldownCost", "Cooldown/cost records must not underflow resources.")),
            Invalid("nondeterministic_ordering", "Shuffle round or marker order nondeterministically.", "rejected", Error("goal068.order.nondeterministic", "ordering", "Rows, rounds and markers must be stable sorted.")),
            Invalid("save_load_mismatch", "Alter the restored after-state hash.", "rejected", Error("goal068.save_load.mismatch", "saveLoad", "Save/load must preserve the combat/magic after-state.")),
            Invalid("replay_mismatch", "Alter the replay hash for identical inputs.", "rejected", Error("goal068.replay.mismatch", "replay", "Replay hashes must match for identical row input.")),
            Invalid("final_prose_leakage", "Add final prose text or generated dialogue text fields.", "rejected", Error("goal068.prose.final_leakage", "text", "Goal 068 must not emit final prose.")),
            Invalid("llm_provider_rag_claim", "Claim live LLM/provider/RAG execution.", "rejected", Error("goal068.external.llm_provider_rag_claim", "external", "Goal 068 is BCL-only and repository-local.")),
            Invalid("arbitrary_lua_or_generated_lua_claim", "Claim arbitrary Lua execution or generated Lua source.", "rejected", Error("goal068.lua.execution_claim", "lua", "Goal 068 must not execute or generate Lua.")),
            Invalid("runtime_ui_unity_broad_mutation_claim", "Mutate Runtime/UI or broad Unity assets.", "rejected", Error("goal068.scope.broad_mutation_claim", "scope", "Only the narrow AlphaRuntimeBootstrap marker loader may change.")),
            Invalid("public_gamepackage_schema_mutation_claim", "Change public GamePackage schema.", "rejected", Error("goal068.scope.public_schema_mutation", "schema", "Public GamePackage schema changes are forbidden.")),
            Invalid("unsafe_path", "Write an absolute or parent-traversal artifact path.", "rejected", Error("goal068.path.unsafe", "path", "Artifact paths must be repository-relative and safe.")),
            Invalid("missing_unity_marker_proof", "Omit required combat_magic player markers.", "blocked", Error("goal068.unity.marker_missing", "unity", "Unity proof must match all required markers.")),
            Invalid("boss_phase_without_transition", "Declare a boss phase without a state transition.", "rejected", Error("goal068.boss.phase_without_transition", "bossPhase", "Boss/elite phases require transitions.")),
            Invalid("impossible_overpowered_encounter", "Create an encounter that cannot resolve from available resources or counterplay.", "rejected", Error("goal068.encounter.impossible", "encounter", "Encounter must remain bounded and resolvable."))
        };

        return new InvalidCombatMagicDiagnosticsMatrix
        {
            Passed = scenarios.Count == CombatMagicAbilityBossEncounterVocabulary.RequiredInvalidScenarioIds.Count
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<CombatMagicFilePayload> BuildStagingFiles(CombatMagicSourceBundle source, CombatMagicUnityCommandPlan unityCommandPlan)
    {
        var files = new List<CombatMagicFilePayload>(source.BaseStagingFiles)
        {
            TextFile(CombatMagicAbilityBossEncounterVocabulary.UnityCombatMagicCommandPlanStagingRelativePath, Serialize(unityCommandPlan))
        };

        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static CombatMagicRow BuildRow(CombatMagicSourceRow source)
    {
        var profile = CombatProfiles.For(source.FamilyId, source.SeedId);
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var encounterId = "encounter/" + safeFamily + "/" + safeSeed + "/" + profile.EncounterKind;
        var playerId = "combatant/" + safeFamily + "/" + safeSeed + "/player";
        var enemyId = "combatant/" + safeFamily + "/" + safeSeed + "/" + profile.EnemyKind;
        var abilityId = "ability/" + safeFamily + "/" + profile.PrimaryAbilityKind;
        var traitId = "trait/" + safeFamily + "/" + profile.PassiveTraitKind;
        var statusId = "status/" + safeFamily + "/" + profile.PrimaryStatusKind;
        var phaseId = "boss-phase/" + safeFamily + "/" + profile.PrimaryBossPhaseKind;
        var variant = profile.SeedModifier;

        var initialPlayer = Combatant(
            playerId,
            "player_party",
            new AttributeResourceSnapshot
            {
                Health = profile.PlayerHealth,
                Armor = profile.PlayerArmor,
                Mana = profile.PlayerMana,
                Energy = profile.PlayerEnergy,
                Stamina = profile.PlayerStamina,
                Threat = 0
            },
            [abilityId],
            [traitId],
            []);
        var initialEnemy = Combatant(
            enemyId,
            profile.BossOrElite ? "boss_or_elite" : "encounter_opponent",
            new AttributeResourceSnapshot
            {
                Health = profile.EnemyHealth,
                Armor = profile.EnemyArmor,
                Mana = profile.EnemyMana,
                Energy = profile.EnemyEnergy,
                Stamina = profile.EnemyStamina,
                Threat = 2 + variant
            },
            ["ability/" + safeFamily + "/" + profile.EnemyAbilityKind],
            ["trait/" + safeFamily + "/" + profile.EnemyTraitKind],
            profile.BossOrElite ? ["status/" + safeFamily + "/" + profile.BossStatusKind] : []);

        var abilityUse = new ActiveAbilityUse
        {
            AbilityUseId = "goal068/" + safeFamily + "/" + safeSeed + "/ability/" + profile.PrimaryAbilityKind,
            AbilityId = abilityId,
            CasterCombatantId = playerId,
            TargetCombatantId = enemyId,
            AbilityKind = profile.PrimaryAbilityKind,
            Resolution = "resolved/" + profile.AbilityResolution
        };
        var passive = new PassiveTraitUse
        {
            TraitUseId = "goal068/" + safeFamily + "/" + safeSeed + "/trait/" + profile.PassiveTraitKind,
            TraitId = traitId,
            CombatantId = playerId,
            TriggeredBy = abilityUse.AbilityUseId
        };
        var status = new StatusEffectApplication
        {
            StatusApplicationId = "goal068/" + safeFamily + "/" + safeSeed + "/status/" + profile.PrimaryStatusKind,
            StatusEffectId = statusId,
            TargetCombatantId = enemyId,
            BeforeStacks = profile.StatusBeforeStacks,
            AfterStacks = profile.StatusAfterStacks,
            DurationChange = "duration/" + profile.StatusDuration
        };
        var packet = new DamageEffectPacket
        {
            PacketId = "goal068/" + safeFamily + "/" + safeSeed + "/packet/" + profile.DamageKind,
            DamageKind = profile.DamageKind,
            AmountBeforeMitigation = profile.DamageBeforeMitigation + variant,
            AmountAfterMitigation = profile.DamageAfterMitigation + variant,
            AppliedEffectIds = [status.StatusApplicationId]
        };
        var cost = new CooldownCostRecord
        {
            CooldownCostId = "goal068/" + safeFamily + "/" + safeSeed + "/cost/" + profile.ResourceKind,
            AbilityId = abilityId,
            ResourceKind = profile.ResourceKind,
            CostPaid = profile.CostPaid + variant,
            ResourceBefore = profile.ResourceBefore,
            ResourceAfter = profile.ResourceBefore - profile.CostPaid - variant,
            CooldownBefore = 0,
            CooldownAfter = profile.CooldownAfter + variant
        };
        var resistance = new ResistanceWeaknessRecord
        {
            ResistanceWeaknessId = "goal068/" + safeFamily + "/" + safeSeed + "/resistance-weakness",
            CombatantId = enemyId,
            ResistanceKind = profile.ResistanceKind,
            WeaknessKind = profile.WeaknessKind,
            MitigationAmount = profile.MitigationAmount + variant
        };
        var phase = new BossPhaseRecord
        {
            PhaseId = phaseId + "/" + safeSeed,
            PhaseKind = profile.PrimaryBossPhaseKind,
            Trigger = profile.BossOrElite ? "health_threshold_" + (75 - variant) : "elite_pressure_check",
            BeforePhaseState = profile.BossOrElite ? "phase/guarded" : "phase/standard",
            AfterPhaseState = profile.BossOrElite ? "phase/exposed_" + safeSeed : "phase/resolved_" + safeSeed,
            TransitionApplied = true
        };
        var counterplay = new CounterplayRecord
        {
            CounterplayId = "goal068/" + safeFamily + "/" + safeSeed + "/counterplay/" + profile.CounterplayKind,
            CounterplayKind = profile.CounterplayKind,
            PlayerOption = profile.CounterplayOption,
            MitigatedPacketId = packet.PacketId,
            Result = "mitigated/" + profile.CounterplayResult,
            SourceRefs = SourceRefs(source),
            Passed = true
        };
        var loot = new LootProgressionRecord
        {
            LootProgressionId = "goal068/" + safeFamily + "/" + safeSeed + "/loot-progression/" + profile.ProgressionKind,
            LootId = "loot/" + safeFamily + "/" + safeSeed + "/" + profile.LootKind,
            ProgressionId = "progression/" + safeFamily + "/" + safeSeed + "/" + profile.ProgressionKind,
            BeforeValue = "locked",
            AfterValue = "unlocked/" + profile.ProgressionKind,
            SourceRefs = [source.SourcePackageRowRef, source.SourceInterlockedGameplayRowRef, source.SourceNarrativeRowRef],
            Passed = true
        };
        var nonCombat = new NonCombatConsequenceRecord
        {
            ConsequenceId = "goal068/" + safeFamily + "/" + safeSeed + "/non-combat/" + profile.NonCombatConsequenceKind,
            ConsequenceKind = profile.NonCombatConsequenceKind,
            SubjectId = profile.NonCombatSubject(source),
            BeforeValue = "pending",
            AfterValue = "applied/" + profile.NonCombatConsequenceKind,
            SourceRefs = [source.SourceLivingWorldRowRef, source.SourceSettlementRowRef, source.SourceNarrativeRowRef],
            Passed = true
        };

        var deltas = BuildDeltas(source, profile, packet, cost, status, loot, nonCombat);
        var before = Snapshot(source, InitialState(source, profile, encounterId, initialPlayer, initialEnemy), 0);
        var afterValues = ApplyDeltas(before.Values, source, profile, deltas);
        var after = Snapshot(source, afterValues, 6);
        var replay = BuildSaveLoadReplay(source, before, after, [abilityUse], [status], [loot], [counterplay]);
        var finalPlayer = initialPlayer with
        {
            Attributes = initialPlayer.Attributes with
            {
                Health = Math.Max(1, initialPlayer.Attributes.Health - profile.PlayerHealthLoss),
                Mana = profile.ResourceKind == "mana" ? cost.ResourceAfter : initialPlayer.Attributes.Mana,
                Energy = profile.ResourceKind == "energy" ? cost.ResourceAfter : initialPlayer.Attributes.Energy,
                Stamina = profile.ResourceKind == "stamina" ? cost.ResourceAfter : initialPlayer.Attributes.Stamina,
                Threat = 1 + variant
            },
            StatusEffectIds = profile.PlayerFinalStatusIds(safeFamily)
        };
        var finalEnemy = initialEnemy with
        {
            Attributes = initialEnemy.Attributes with
            {
                Health = Math.Max(0, initialEnemy.Attributes.Health - packet.AmountAfterMitigation),
                Armor = Math.Max(0, initialEnemy.Attributes.Armor - profile.ArmorShred),
                Threat = Math.Max(0, initialEnemy.Attributes.Threat - 1)
            },
            StatusEffectIds = initialEnemy.StatusEffectIds.Concat([status.StatusEffectId]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
        var rounds = BuildRounds(source, profile, phase, abilityUse, packet, status, cost, deltas);
        var rowWithoutHash = new CombatMagicRow
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
            SourceSettlementRowRef = source.SourceSettlementRowRef,
            SourceNarrativeRowRef = source.SourceNarrativeRowRef,
            EncounterId = encounterId,
            EncounterKind = profile.EncounterKind,
            InitialCombatants = [initialPlayer, initialEnemy],
            FinalCombatants = [finalPlayer, finalEnemy],
            ActiveAbilities = [abilityUse],
            PassiveTraits = [passive],
            StatusEffects = [status],
            DamageEffectPackets = [packet],
            CooldownCosts = [cost],
            ResistanceWeaknesses = [resistance],
            BossPhases = [phase],
            RoundPhaseResults = rounds,
            CounterplayRecords = [counterplay],
            LootProgressionRecords = [loot],
            NonCombatConsequences = [nonCombat],
            StateDeltas = deltas,
            ChangedCategories = deltas.Select(item => item.Category).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            MeaningfulVarianceAxes = profile.MeaningfulAxes,
            BeforeState = before,
            AfterState = after,
            SaveLoadReplayProof = replay,
            StateChanging = before.StateHash != after.StateHash && deltas.Count(item => item.Passed) >= 5,
            BossOrElitePhaseRow = profile.BossOrElite,
            MagicStatusHeavyRow = profile.MagicStatusHeavy,
            ResourceGearCraftingLinkedRow = profile.ResourceGearCraftingLinked,
            NoFinalProse = true
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    private static IReadOnlyList<RoundPhaseResult> BuildRounds(
        CombatMagicSourceRow source,
        CombatProfile profile,
        BossPhaseRecord phase,
        ActiveAbilityUse ability,
        DamageEffectPacket packet,
        StatusEffectApplication status,
        CooldownCostRecord cost,
        IReadOnlyList<CombatMagicStateDelta> deltas)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        return
        [
            new RoundPhaseResult
            {
                RoundId = "goal068/" + safeFamily + "/" + safeSeed + "/round/01-" + profile.OpenerKind,
                Order = 1,
                PhaseRef = phase.PhaseId,
                ActiveAbilities = [ability],
                DamageEffectPackets = [packet],
                StatusApplications = [status],
                CooldownCosts = [cost],
                StateDeltaRefs = deltas.Where(item => item.Category is "health" or "resource" or "cooldown" or "status").Select(item => item.DeltaId).ToList(),
                Passed = true
            },
            new RoundPhaseResult
            {
                RoundId = "goal068/" + safeFamily + "/" + safeSeed + "/round/02-" + profile.FinisherKind,
                Order = 2,
                PhaseRef = phase.PhaseId,
                ActiveAbilities = [ability with { AbilityUseId = ability.AbilityUseId + "/followup", Resolution = "resolved/" + profile.FinisherKind }],
                DamageEffectPackets = [packet with { PacketId = packet.PacketId + "/followup", AmountAfterMitigation = Math.Max(1, packet.AmountAfterMitigation / 2) }],
                StatusApplications = [status with { StatusApplicationId = status.StatusApplicationId + "/followup", BeforeStacks = status.AfterStacks, AfterStacks = Math.Min(profile.MaxStacks, status.AfterStacks + 1) }],
                CooldownCosts = [cost with { CooldownCostId = cost.CooldownCostId + "/followup", CooldownBefore = cost.CooldownAfter, CooldownAfter = cost.CooldownAfter + 1 }],
                StateDeltaRefs = deltas.Where(item => item.Category is "loot" or "progression" or "non_combat").Select(item => item.DeltaId).ToList(),
                Passed = true
            }
        ];
    }

    private static IReadOnlyList<CombatMagicStateDelta> BuildDeltas(
        CombatMagicSourceRow source,
        CombatProfile profile,
        DamageEffectPacket packet,
        CooldownCostRecord cost,
        StatusEffectApplication status,
        LootProgressionRecord loot,
        NonCombatConsequenceRecord nonCombat)
    {
        var prefix = "goal068/" + Safe(source.FamilyId) + "/" + Safe(source.SeedId);
        return
        [
            Delta(prefix + "/01-health/delta/enemy_health", "health", "enemy.health", profile.EnemyHealth.ToString(), Math.Max(0, profile.EnemyHealth - packet.AmountAfterMitigation).ToString(), source.SourceGameplayConsequenceRowRef),
            Delta(prefix + "/02-resource/delta/" + profile.ResourceKind, "resource", "player." + profile.ResourceKind, cost.ResourceBefore.ToString(), cost.ResourceAfter.ToString(), source.SourceInterlockedGameplayRowRef),
            Delta(prefix + "/03-cooldown/delta/" + profile.PrimaryAbilityKind, "cooldown", "ability.cooldown." + profile.PrimaryAbilityKind, cost.CooldownBefore.ToString(), cost.CooldownAfter.ToString(), source.SourceInterlockedGameplayRowRef),
            Delta(prefix + "/04-status/delta/" + profile.PrimaryStatusKind, "status", "status." + profile.PrimaryStatusKind, status.BeforeStacks.ToString(), status.AfterStacks.ToString(), source.SourceInterlockedGameplayRowRef),
            Delta(prefix + "/05-loot/delta/" + profile.LootKind, "loot", "loot." + profile.LootKind, "absent", loot.LootId, source.SourcePackageRowRef),
            Delta(prefix + "/06-progression/delta/" + profile.ProgressionKind, "progression", "progression." + profile.ProgressionKind, loot.BeforeValue, loot.AfterValue, source.SourceNarrativeRowRef),
            Delta(prefix + "/07-non-combat/delta/" + profile.NonCombatConsequenceKind, "non_combat", "non_combat." + profile.NonCombatConsequenceKind, nonCombat.BeforeValue, nonCombat.AfterValue, source.SourceLivingWorldRowRef)
        ];
    }

    private static IReadOnlyDictionary<string, string> InitialState(
        CombatMagicSourceRow source,
        CombatProfile profile,
        string encounterId,
        CombatantSnapshot player,
        CombatantSnapshot enemy) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.package"] = source.SourcePackageRowRef,
            ["source.review_package"] = source.SourceReviewPackageRowRef,
            ["source.spatial"] = source.SourceSpatialDetailRowRef,
            ["source.gameplay"] = source.SourceGameplayConsequenceRowRef,
            ["source.living_world"] = source.SourceLivingWorldRowRef,
            ["source.interlocked"] = source.SourceInterlockedGameplayRowRef,
            ["source.settlement"] = source.SourceSettlementRowRef,
            ["source.narrative"] = source.SourceNarrativeRowRef,
            ["source.goal067.after_hash"] = source.NarrativeAfterStateHash,
            ["encounter.id"] = encounterId,
            ["encounter.kind"] = profile.EncounterKind,
            ["player.health"] = player.Attributes.Health.ToString(),
            ["player." + profile.ResourceKind] = profile.ResourceBefore.ToString(),
            ["enemy.health"] = enemy.Attributes.Health.ToString(),
            ["ability.cooldown." + profile.PrimaryAbilityKind] = "0",
            ["status." + profile.PrimaryStatusKind] = profile.StatusBeforeStacks.ToString(),
            ["loot." + profile.LootKind] = "absent",
            ["progression." + profile.ProgressionKind] = "locked",
            ["non_combat." + profile.NonCombatConsequenceKind] = "pending"
        };

    private static SortedDictionary<string, string> ApplyDeltas(
        IReadOnlyDictionary<string, string> before,
        CombatMagicSourceRow source,
        CombatProfile profile,
        IReadOnlyList<CombatMagicStateDelta> deltas)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in before)
        {
            values[pair.Key] = pair.Value;
        }

        foreach (var delta in deltas)
        {
            values[delta.Key] = delta.AfterValue;
        }

        values["combat.flavor"] = profile.EncounterKind;
        values["combat.ability"] = profile.PrimaryAbilityKind;
        values["combat.status_profile"] = profile.PrimaryStatusKind;
        values["combat.boss_or_elite"] = profile.BossOrElite.ToString().ToLowerInvariant();
        values["combat.resource_link"] = profile.ResourceGearCraftingLinked.ToString().ToLowerInvariant();
        values["combat.magic_status_heavy"] = profile.MagicStatusHeavy.ToString().ToLowerInvariant();
        values["goal067.quest_arc"] = source.QuestArcId;
        values["goal067.dialogue_graph"] = source.DialogueGraphId;
        values["settlement.id"] = source.SettlementId;
        values["settlement.building"] = source.BuildingId;
        values["living_world.actor"] = source.LivingWorldActorIds.FirstOrDefault() ?? string.Empty;
        values["living_world.faction"] = source.LivingWorldFactionIds.FirstOrDefault() ?? string.Empty;
        values["interlocked.combat_ledger_count"] = source.InterlockedCombatProgressionLedgerEntryIds.Count.ToString();
        values["interlocked.status_ledger_count"] = source.InterlockedStatusLedgerEntryIds.Count.ToString();
        return values;
    }

    private static CombatMagicSaveLoadReplayRecord BuildSaveLoadReplay(
        CombatMagicSourceRow source,
        CombatMagicStateSnapshot before,
        CombatMagicStateSnapshot after,
        IReadOnlyList<ActiveAbilityUse> abilities,
        IReadOnlyList<StatusEffectApplication> statuses,
        IReadOnlyList<LootProgressionRecord> progression,
        IReadOnlyList<CounterplayRecord> counterplay)
    {
        var json = Serialize(after);
        var restored = CombatMagicAbilityBossEncounterHash.Deserialize<CombatMagicStateSnapshot>(json);
        var replayHash = Hash(Serialize(new
        {
            source.RowId,
            abilityIds = abilities.Select(item => item.AbilityUseId).ToList(),
            statusIds = statuses.Select(item => item.StatusApplicationId).ToList(),
            progressionIds = progression.Select(item => item.LootProgressionId).ToList(),
            counterplayIds = counterplay.Select(item => item.CounterplayId).ToList(),
            after.StateHash
        }));

        return new CombatMagicSaveLoadReplayRecord
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

    private static CombatMagicStateSnapshot Snapshot(
        CombatMagicSourceRow source,
        IReadOnlyDictionary<string, string> values,
        int stepIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }

        return new CombatMagicStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            StepIndex = stepIndex,
            Values = copy,
            StateHash = Hash(Serialize(copy))
        };
    }

    private static CombatMagicStateDelta Delta(string deltaId, string category, string key, string before, string after, string sourceRef) =>
        new()
        {
            DeltaId = deltaId,
            Category = category,
            Key = key,
            BeforeValue = before,
            AfterValue = after,
            SourceRef = sourceRef,
            Passed = !string.IsNullOrWhiteSpace(deltaId)
                && !string.IsNullOrWhiteSpace(category)
                && !string.IsNullOrWhiteSpace(key)
                && !string.Equals(before, after, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(sourceRef)
        };

    private static CombatMagicLedger Ledger(string kind, IReadOnlyList<CombatMagicLedgerEntry> entries, bool coveragePassed) =>
        new()
        {
            LedgerKind = kind,
            Passed = coveragePassed && entries.Count > 0 && entries.All(item => item.Passed && item.StateDeltaRefs.Count > 0),
            EntryCount = entries.Count,
            Entries = entries
        };

    private static bool SameFamilySeedVariancePassed(IReadOnlyList<CombatMagicRow> rows)
    {
        var familyGroups = rows.GroupBy(item => item.FamilyId, StringComparer.Ordinal).ToList();
        return familyGroups.Count == 3
            && familyGroups.All(group => group.Select(row => Hash(Serialize(VarianceHighlight(row)))).Distinct(StringComparer.Ordinal).Count() == 3);
    }

    private static bool FamilyCombatFlavorVariancePassed(IReadOnlyList<CombatMagicRow> rows) =>
        rows.GroupBy(item => item.FamilyId, StringComparer.Ordinal).Count() == 3
        && rows.Select(item => item.EncounterKind).Distinct(StringComparer.Ordinal).Count() >= 3
        && rows.SelectMany(item => item.ActiveAbilities.Select(ability => ability.AbilityKind)).Distinct(StringComparer.Ordinal).Count() >= 9
        && rows.SelectMany(item => item.StatusEffects.Select(status => status.StatusEffectId)).Distinct(StringComparer.Ordinal).Count() >= 9;

    private static IReadOnlyDictionary<string, string> VarianceHighlight(CombatMagicRow row) =>
        row.MeaningfulVarianceAxes
            .Where(row.AfterState.Values.ContainsKey)
            .ToDictionary(key => key, key => row.AfterState.Values[key], StringComparer.Ordinal);

    private static CombatantSnapshot Combatant(
        string id,
        string role,
        AttributeResourceSnapshot attributes,
        IReadOnlyList<string> abilityIds,
        IReadOnlyList<string> traitIds,
        IReadOnlyList<string> statusIds) =>
        new()
        {
            CombatantId = id,
            Role = role,
            Attributes = attributes,
            ActiveAbilityIds = abilityIds,
            PassiveTraitIds = traitIds,
            StatusEffectIds = statusIds
        };

    private static IReadOnlyList<string> SourceRefs(CombatMagicSourceRow source) =>
    [
        source.SourcePackageRowRef,
        source.SourceReviewPackageRowRef,
        source.SourceSpatialDetailRowRef,
        source.SourceGameplayConsequenceRowRef,
        source.SourceLivingWorldRowRef,
        source.SourceInterlockedGameplayRowRef,
        source.SourceSettlementRowRef,
        source.SourceNarrativeRowRef
    ];

    private static CombatMagicGateRecord Gate(string gateId, string status, string provenance, string evidence) =>
        new()
        {
            GateId = gateId,
            Status = status,
            ProvenanceKind = provenance,
            EvidenceRef = evidence
        };

    private static InvalidCombatMagicScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params CombatMagicDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = CombatMagicAbilityBossEncounterSourceLoader.SortDiagnostics(diagnostics)
        };

    private static CombatMagicFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static string Safe(string value) => CombatMagicAbilityBossEncounterHash.SafeSegment(value);

    private static string Serialize<T>(T value) => CombatMagicAbilityBossEncounterHash.Serialize(value);

    private static string Hash(string value) => CombatMagicAbilityBossEncounterHash.Hash(value);

    private static CombatMagicDiagnostic Error(string code, string target, string message) =>
        CombatMagicDiagnostic.Error(code, target, message);

    private static CombatMagicDiagnostic Info(string code, string target, string message) =>
        CombatMagicDiagnostic.Info(code, target, message);

    private sealed record CombatProfile(
        string FamilyId,
        string EncounterKind,
        string EnemyKind,
        string PrimaryAbilityKind,
        string EnemyAbilityKind,
        string PassiveTraitKind,
        string EnemyTraitKind,
        string PrimaryStatusKind,
        string BossStatusKind,
        string PrimaryBossPhaseKind,
        string OpenerKind,
        string FinisherKind,
        string AbilityResolution,
        string DamageKind,
        string ResourceKind,
        string ResistanceKind,
        string WeaknessKind,
        string CounterplayKind,
        string CounterplayOption,
        string CounterplayResult,
        string LootKind,
        string ProgressionKind,
        string NonCombatConsequenceKind,
        int SeedModifier,
        int PlayerHealth,
        int PlayerArmor,
        int PlayerMana,
        int PlayerEnergy,
        int PlayerStamina,
        int EnemyHealth,
        int EnemyArmor,
        int EnemyMana,
        int EnemyEnergy,
        int EnemyStamina,
        int PlayerHealthLoss,
        int DamageBeforeMitigation,
        int DamageAfterMitigation,
        int ResourceBefore,
        int CostPaid,
        int CooldownAfter,
        int StatusBeforeStacks,
        int StatusAfterStacks,
        int MaxStacks,
        int ArmorShred,
        int MitigationAmount,
        bool BossOrElite,
        bool MagicStatusHeavy,
        bool ResourceGearCraftingLinked,
        IReadOnlyList<string> AbilityKinds,
        IReadOnlyList<string> AbilityTags,
        IReadOnlyList<string> StatusKinds,
        IReadOnlyList<string> StatusDeltaCategories,
        IReadOnlyList<string> BossPhaseKinds,
        IReadOnlyList<string> MeaningfulAxes)
    {
        public string SafeFamily => Safe(FamilyId);
        public string StatusStackPolicy => "bounded_stack";
        public string StatusDuration => "rounds_" + (2 + SeedModifier);

        public IReadOnlyList<string> PlayerFinalStatusIds(string safeFamily) =>
            MagicStatusHeavy ? ["status/" + safeFamily + "/" + PrimaryStatusKind + "/resisted"] : [];

        public string NonCombatSubject(CombatMagicSourceRow source) =>
            NonCombatConsequenceKind switch
            {
                "faction_narrative_pressure" => source.LivingWorldFactionIds.FirstOrDefault() ?? source.SourceLivingWorldRowRef,
                "survival_camp_recovery" => source.SettlementId,
                "dungeon_route_memory" => source.QuestArcId,
                _ => source.SourceNarrativeRowRef
            };
    }

    private static class CombatProfiles
    {
        public static IReadOnlyList<CombatProfile> AllFamilyProfiles() =>
        [
            FamilyProfile(
                "map_panel_rpg",
                "tactical_faction_duel",
                "faction_champion",
                "sigil_break",
                "shield_rally",
                "council_oath",
                "duelist_guard",
                "marked_vulnerable",
                "faction_guarded",
                "elite_guard_break",
                "read_the_guard",
                "route_duel_finish",
                "guard broken by linked route pressure",
                "arcane_slash",
                "mana",
                "shield",
                "ritual_mark",
                "stance_switch",
                "brace_and_flank",
                "incoming strike reduced",
                "council_badge",
                "route_access",
                "faction_narrative_pressure",
                true,
                true,
                false,
                ["sigil_break", "banner_interdict", "oath_counter"],
                ["magic", "tactical", "faction"],
                ["marked_vulnerable", "inspired_guard", "oath_burn"],
                ["health", "status", "faction"],
                ["elite_guard_break", "oath_reversal"]),
            FamilyProfile(
                "survival_sandbox",
                "hostile_hazard_creature_raid",
                "hazard_alpha",
                "ember_trap",
                "maul_pressure",
                "scarred_hide",
                "pack_hunger",
                "fatigue_burn",
                "enraged_hazard",
                "raid_hazard_escalation",
                "set_the_trap",
                "recover_and_salvage",
                "trap consumes crafted fuel and controls hazard",
                "fire_trap",
                "stamina",
                "hide",
                "cold_exposure",
                "crafted_countermeasure",
                "consume_repair_kit",
                "injury pressure reduced",
                "reinforced_tool",
                "camp_recovery",
                "survival_camp_recovery",
                true,
                true,
                true,
                ["ember_trap", "bandage_surge", "snare_pull"],
                ["hazard", "crafting", "survival"],
                ["fatigue_burn", "injury_stabilized", "warmth_buffer"],
                ["health", "status", "survival"],
                ["raid_hazard_escalation", "resource_starvation"]),
            FamilyProfile(
                "first_person_grid_dungeon",
                "orientation_boss_trap_gate",
                "grid_warden",
                "rune_lance",
                "trap_pulse",
                "glyph_focus",
                "stone_skin",
                "rune_shock",
                "boss_channeling",
                "boss_trap_turn",
                "align_the_corridor",
                "open_the_glyph_gate",
                "rune lance interrupts a traversal-aware boss phase",
                "rune_damage",
                "energy",
                "ward",
                "rune_instability",
                "position_counter",
                "sidestep_and_interrupt",
                "trap pulse redirected",
                "glyph_key",
                "gate_unlock",
                "dungeon_route_memory",
                true,
                true,
                false,
                ["rune_lance", "ward_shatter", "glyph_guard"],
                ["magic", "boss", "grid"],
                ["rune_shock", "orientation_focus", "trap_slow"],
                ["health", "status", "position"],
                ["boss_trap_turn", "glyph_gate_shift"])
        ];

        public static CombatProfile For(string familyId, string seedId)
        {
            var baseProfile = AllFamilyProfiles().First(item => item.FamilyId == familyId);
            var modifier = SeedModifier(seedId);
            return baseProfile with
            {
                SeedModifier = modifier,
                PrimaryAbilityKind = baseProfile.AbilityKinds[Math.Min(modifier - 1, baseProfile.AbilityKinds.Count - 1)],
                PrimaryStatusKind = baseProfile.StatusKinds[Math.Min(modifier - 1, baseProfile.StatusKinds.Count - 1)],
                PrimaryBossPhaseKind = baseProfile.BossPhaseKinds[Math.Min((modifier - 1) % baseProfile.BossPhaseKinds.Count, baseProfile.BossPhaseKinds.Count - 1)],
                BossOrElite = baseProfile.BossOrElite || familyId == "first_person_grid_dungeon",
                ResourceGearCraftingLinked = baseProfile.ResourceGearCraftingLinked || (familyId == "map_panel_rpg" && seedId == "seed_beta"),
                PlayerHealth = baseProfile.PlayerHealth + modifier,
                EnemyHealth = baseProfile.EnemyHealth + (modifier * 3),
                DamageBeforeMitigation = baseProfile.DamageBeforeMitigation + modifier,
                DamageAfterMitigation = baseProfile.DamageAfterMitigation + modifier,
                CostPaid = baseProfile.CostPaid + modifier,
                StatusAfterStacks = Math.Min(baseProfile.MaxStacks, baseProfile.StatusAfterStacks + modifier - 1)
            };
        }

        private static CombatProfile FamilyProfile(
            string familyId,
            string encounterKind,
            string enemyKind,
            string primaryAbilityKind,
            string enemyAbilityKind,
            string passiveTraitKind,
            string enemyTraitKind,
            string primaryStatusKind,
            string bossStatusKind,
            string primaryBossPhaseKind,
            string openerKind,
            string finisherKind,
            string abilityResolution,
            string damageKind,
            string resourceKind,
            string resistanceKind,
            string weaknessKind,
            string counterplayKind,
            string counterplayOption,
            string counterplayResult,
            string lootKind,
            string progressionKind,
            string nonCombatConsequenceKind,
            bool bossOrElite,
            bool magicStatusHeavy,
            bool resourceGearCraftingLinked,
            IReadOnlyList<string> abilityKinds,
            IReadOnlyList<string> abilityTags,
            IReadOnlyList<string> statusKinds,
            IReadOnlyList<string> statusDeltaCategories,
            IReadOnlyList<string> bossPhaseKinds) =>
            new(
                familyId,
                encounterKind,
                enemyKind,
                primaryAbilityKind,
                enemyAbilityKind,
                passiveTraitKind,
                enemyTraitKind,
                primaryStatusKind,
                bossStatusKind,
                primaryBossPhaseKind,
                openerKind,
                finisherKind,
                abilityResolution,
                damageKind,
                resourceKind,
                resistanceKind,
                weaknessKind,
                counterplayKind,
                counterplayOption,
                counterplayResult,
                lootKind,
                progressionKind,
                nonCombatConsequenceKind,
                1,
                PlayerHealth: 100,
                PlayerArmor: 20,
                PlayerMana: 45,
                PlayerEnergy: 40,
                PlayerStamina: 50,
                EnemyHealth: 90,
                EnemyArmor: 16,
                EnemyMana: 20,
                EnemyEnergy: 20,
                EnemyStamina: 25,
                PlayerHealthLoss: 7,
                DamageBeforeMitigation: 24,
                DamageAfterMitigation: 18,
                ResourceBefore: 42,
                CostPaid: 6,
                CooldownAfter: 2,
                StatusBeforeStacks: 0,
                StatusAfterStacks: 1,
                MaxStacks: 4,
                ArmorShred: 3,
                MitigationAmount: 5,
                bossOrElite,
                magicStatusHeavy,
                resourceGearCraftingLinked,
                abilityKinds,
                abilityTags,
                statusKinds,
                statusDeltaCategories,
                bossPhaseKinds,
                MeaningfulAxes(familyId));

        private static IReadOnlyList<string> MeaningfulAxes(string familyId) =>
            familyId switch
            {
                "map_panel_rpg" =>
                [
                    "combat.flavor",
                    "combat.ability",
                    "combat.status_profile",
                    "loot.council_badge",
                    "progression.route_access",
                    "non_combat.faction_narrative_pressure"
                ],
                "survival_sandbox" =>
                [
                    "combat.flavor",
                    "combat.ability",
                    "combat.resource_link",
                    "loot.reinforced_tool",
                    "progression.camp_recovery",
                    "non_combat.survival_camp_recovery"
                ],
                "first_person_grid_dungeon" =>
                [
                    "combat.flavor",
                    "combat.ability",
                    "combat.boss_or_elite",
                    "loot.glyph_key",
                    "progression.gate_unlock",
                    "non_combat.dungeon_route_memory"
                ],
                _ => []
            };

        private static int SeedModifier(string seedId) =>
            seedId switch
            {
                "seed_alpha" => 1,
                "seed_beta" => 2,
                "seed_gamma" => 3,
                _ => 1
            };
    }
}
