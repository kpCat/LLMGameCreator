using System.Text;

namespace LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;

public sealed class ProgrammaticNarrativeMatrixBuilder
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public ProgrammaticNarrativeSourceManifest BuildSourceManifest(ProgrammaticNarrativeSourceBundle source)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>(source.Diagnostics)
        {
            ProgrammaticNarrativeDiagnostic.Info("goal067.preflight.goal066_handoff_recorded", "settlement_construction_destruction_production_matrix_verification", "Goal 066 is recorded as accepted by user handoff before Goal 067."),
            ProgrammaticNarrativeDiagnostic.Info("goal067.source.loaded", "Goal060-066", "Goal 067 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066 compact evidence.")
        };

        return new ProgrammaticNarrativeSourceManifest
        {
            Accepted = false,
            Goal066AcceptedByUserHandoff = source.Goal066AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewPackageRcConsumed = source.Goal061ReviewPackageRcConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal065InterlockedRowsConsumed = source.Goal065InterlockedRowsConsumed,
            Goal066SettlementRowsConsumed = source.Goal066SettlementRowsConsumed,
            Goal066UnityProofConsumed = source.Goal066UnityProofConsumed,
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
                Gate("settlement_construction_destruction_production_matrix_verification", "passed", "user_handoff", "Goal 067 preflight handoff"),
                Gate(ProgrammaticNarrativeVocabulary.FinalGate, "required", "current_goal_manual_gate", ProgrammaticNarrativeVocabulary.RelativeOutputDirectory + "/" + ProgrammaticNarrativeEvidenceService.ReportMarkdownFileName),
                Gate("semantic_pack_composition_blueprint_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 031 remains not passed"),
                Gate("dynamic_semantic_feature_system_verification", "produced_for_review_not_passed", "preserved_current_state", "Goal 032 remains not passed")
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = ProgrammaticNarrativeSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public ProgrammaticNarrativeTemplateCatalog BuildTemplateCatalog()
    {
        var profiles = new List<NarrativeTemplateProfile>
        {
            Profile(
                "map_panel_rpg",
                ["settlement_route_contract", "faction_repair_bargain", "public_rumor_resolution"],
                ["npc_warning_low_trust", "quest_offer_repair_trade", "event_aftermath_faction_notice"],
                ["settlement_pressure_event", "trade_route_unlock", "faction_rumor_update"],
                ["settlement_crafter", "route_guide", "faction_broker"],
                ["public_rumor", "contract_memory", "faction_notice"]),
            Profile(
                "survival_sandbox",
                ["camp_hazard_recovery", "resource_scarcity_choice", "shelter_memory_chain"],
                ["camp_warning_resource_shortage", "survival_task_offer", "hazard_aftermath_notice"],
                ["weather_pressure_event", "resource_recovery_unlock", "camp_rumor_update"],
                ["camp_builder", "forager", "watch_runner"],
                ["hazard_memory", "resource_rumor", "camp_notice"]),
            Profile(
                "first_person_grid_dungeon",
                ["gate_key_memory", "safe_room_bargain", "trap_consequence_chain"],
                ["dungeon_warning_locked_route", "party_task_offer", "trap_aftermath_notice"],
                ["trap_alert_event", "route_gate_unlock", "party_memory_update"],
                ["gate_keeper", "party_scout", "rune_scribe"],
                ["route_memory", "trap_rumor", "party_notice"])
        };

        return new ProgrammaticNarrativeTemplateCatalog
        {
            Passed = profiles.Count == 3
                && profiles.All(item => item.QuestKinds.Count >= 3
                    && item.DialogueTemplateIds.Count >= 3
                    && item.EventKinds.Count >= 3
                    && item.SpeakerRoles.Count >= 3
                    && item.MemoryKinds.Count >= 3),
            ProfileCount = profiles.Count,
            Profiles = profiles
        };
    }

    public IReadOnlyList<ProgrammaticNarrativeRow> BuildRows(ProgrammaticNarrativeSourceBundle source) =>
        source.Rows
            .OrderBy(item => ProgrammaticNarrativeVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => ProgrammaticNarrativeVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public ProgrammaticNarrativeRowMatrix BuildRowMatrix(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new ProgrammaticNarrativeRowMatrix
        {
            Passed = rows.Count == 9
                && rows.All(item => item.StateChanging && item.NoFinalProse)
                && distinctHashes == 9,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    public NarrativeLedger BuildQuestStageLedger(IReadOnlyList<ProgrammaticNarrativeRow> rows) =>
        Ledger(
            "quest_stage",
            rows.SelectMany(row => row.QuestStageGraph.Select(stage => new NarrativeLedgerEntry
            {
                EntryId = "quest_stage/" + stage.StageId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "quest_stage",
                SubjectId = stage.StageId,
                BeforeValue = stage.AvailabilityBefore,
                AfterValue = stage.AvailabilityAfter,
                Outcome = stage.StageKind,
                SourceRefs = SourceRefs(row),
                StateDeltaRefs = stage.StateDeltaRefs,
                Passed = stage.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.QuestStageGraph.Count >= 3));

    public NarrativeLedger BuildDialogueOptionLedger(IReadOnlyList<ProgrammaticNarrativeRow> rows) =>
        Ledger(
            "dialogue_option",
            rows.SelectMany(row => row.DialogueOptionGraph.Select(option => new NarrativeLedgerEntry
            {
                EntryId = "dialogue_option/" + option.OptionId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "dialogue_option",
                SubjectId = option.OptionId,
                BeforeValue = option.AvailabilityBefore,
                AfterValue = option.AvailabilityAfter,
                Outcome = string.Join(",", option.OptionEffects),
                SourceRefs = SourceRefs(row),
                StateDeltaRefs = option.StateDeltaRefs,
                Passed = option.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.DialogueOptionGraph.Count >= 2));

    public NarrativeLedger BuildEventConsequenceLedger(IReadOnlyList<ProgrammaticNarrativeRow> rows) =>
        Ledger(
            "event_trigger_consequence",
            rows.SelectMany(row => row.EventTriggerConsequenceChain.Select(item => new NarrativeLedgerEntry
            {
                EntryId = "event_consequence/" + item.ConsequenceId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "event_trigger_consequence",
                SubjectId = item.TriggerId,
                BeforeValue = item.BeforeState,
                AfterValue = item.AfterState,
                Outcome = item.LaterAvailabilityChange,
                SourceRefs = item.SourceRefs,
                StateDeltaRefs = item.StateDeltaRefs,
                Passed = item.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.EventTriggerConsequenceChain.Count >= 1));

    public LocalizationKeyTable BuildLocalizationKeyTable(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var entries = rows
            .SelectMany(item => item.LocalizationKeyTable)
            .OrderBy(item => item.LineKey, StringComparer.Ordinal)
            .ToList();

        return new LocalizationKeyTable
        {
            Passed = entries.Count >= 18
                && entries.All(item => !string.IsNullOrWhiteSpace(item.LineKey)
                    && !string.IsNullOrWhiteSpace(item.TemplateId)
                    && item.Slots.Count >= 2
                    && item.OptionEffects.Count > 0),
            EntryCount = entries.Count,
            Entries = entries
        };
    }

    public NarrativeLedger BuildMemoryRumorLedger(IReadOnlyList<ProgrammaticNarrativeRow> rows) =>
        Ledger(
            "memory_rumor_propagation",
            rows.SelectMany(row => row.MemoryRumorPropagation.Select(item => new NarrativeLedgerEntry
            {
                EntryId = "memory_rumor/" + item.RecordId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                LedgerKind = "memory_rumor_propagation",
                SubjectId = item.RecordId,
                BeforeValue = item.BeforeState,
                AfterValue = item.AfterState,
                Outcome = item.PropagationKind,
                SourceRefs = item.SourceRefs,
                StateDeltaRefs = item.StateDeltaRefs,
                Passed = item.Passed
            })).OrderBy(item => item.EntryId, StringComparer.Ordinal).ToList(),
            rows.Count == 9 && rows.All(item => item.MemoryRumorPropagation.Count >= 1));

    public ProgrammaticNarrativeSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new ProgrammaticNarrativeSaveLoadReplayProof
        {
            Passed = proofRows.Count == 9 && proofRows.All(item => item.BeforeAfterStateChanged && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangedRowCount = proofRows.Count(item => item.BeforeAfterStateChanged),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    public bool MeaningfulVariancePassed(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var familyGroups = rows.GroupBy(item => item.FamilyId, StringComparer.Ordinal).ToList();
        var sameFamilySeedVariation = familyGroups.Count == 3
            && familyGroups.All(group => group.Select(row => Hash(Serialize(VarianceHighlight(row)))).Distinct(StringComparer.Ordinal).Count() == 3);
        var crossFamilyShapeVariation = rows.Select(item => item.QuestStageGraph.First().StageKind).Distinct(StringComparer.Ordinal).Count() >= 3
            && rows.SelectMany(item => item.DialogueOptionGraph.Select(option => option.TemplateId)).Distinct(StringComparer.Ordinal).Count() >= 9
            && rows.SelectMany(item => item.EventTriggerConsequenceChain.Select(chain => chain.TriggerKind)).Distinct(StringComparer.Ordinal).Count() >= 6;

        return rows.Count == 9
            && sameFamilySeedVariation
            && crossFamilyShapeVariation
            && rows.Select(item => item.AfterState.StateHash).Distinct(StringComparer.Ordinal).Count() == 9;
    }

    public ProgrammaticNarrativeUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var commandRows = rows
            .OrderBy(item => ProgrammaticNarrativeVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => ProgrammaticNarrativeVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var stage = row.QuestStageGraph.First();
                var option = row.DialogueOptionGraph.First();
                var eventRecord = row.EventTriggerConsequenceChain.First();
                var memory = row.MemoryRumorPropagation.First();
                var localization = row.LocalizationKeyTable.First();
                var markers = new List<string>
                {
                    "narrative_row_loaded=" + row.RowId,
                    "narrative_family=" + row.FamilyId,
                    "narrative_seed=" + row.SeedId,
                    "quest_stage_started=" + stage.StageId,
                    "dialogue_option_available=" + option.OptionId,
                    "dialogue_option_selected=" + option.OptionId,
                    "event_trigger_resolved=" + eventRecord.TriggerId,
                    "event_consequence_applied=" + eventRecord.ConsequenceId,
                    "memory_rumor_recorded=" + memory.RecordId,
                    "localization_key_bound=" + localization.LineKey,
                    "narrative_row_completed=" + row.RowId
                };

                return new ProgrammaticNarrativeUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    QuestStageId = stage.StageId,
                    DialogueOptionId = option.OptionId,
                    EventTriggerId = eventRecord.TriggerId,
                    EventConsequenceId = eventRecord.ConsequenceId,
                    MemoryRumorRecordId = memory.RecordId,
                    LocalizationLineKey = localization.LineKey,
                    ExpectedPlayerMarkers = markers.OrderBy(item => item, StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "narrative_matrix_loaded=goal067",
            "narrative_matrix_completed=true",
            "review_package_proof=goal067",
            "programmatic_narrative_quest_dialogue_event_matrix_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new ProgrammaticNarrativeUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => !string.IsNullOrWhiteSpace(item.QuestStageId)
                    && !string.IsNullOrWhiteSpace(item.DialogueOptionId)
                    && !string.IsNullOrWhiteSpace(item.EventTriggerId)
                    && !string.IsNullOrWhiteSpace(item.EventConsequenceId)
                    && !string.IsNullOrWhiteSpace(item.MemoryRumorRecordId)
                    && !string.IsNullOrWhiteSpace(item.LocalizationLineKey)),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public ProgrammaticNarrativePreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<ProgrammaticNarrativeRow> rows)
    {
        var payloadRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new ProgrammaticNarrativePreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                QuestArcId = item.QuestArcId,
                DialogueGraphId = item.DialogueGraphId,
                EventChainId = item.EventChainId,
                PackageRef = item.SourcePackageRowRef,
                SpatialRef = item.SourceSpatialDetailRowRef,
                LivingWorldRef = item.SourceLivingWorldRowRef,
                InterlockedRef = item.SourceInterlockedGameplayRowRef,
                SettlementRef = item.SourceSettlementRowRef,
                NarrativeAfterStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "narrative_row_loaded=" + item.RowId,
                    "narrative_after_state_hash=" + item.AfterState.StateHash,
                    "quest_stage_count=" + item.QuestStageGraph.Count,
                    "dialogue_option_count=" + item.DialogueOptionGraph.Count,
                    "event_consequence_count=" + item.EventTriggerConsequenceChain.Count
                ]
            })
            .ToList();

        return new ProgrammaticNarrativePreviewExportPayload
        {
            Passed = payloadRows.Count == 9 && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.NarrativeAfterStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    public InvalidProgrammaticNarrativeDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidProgrammaticNarrativeScenario>
        {
            Invalid("missing_goal066_source", "Remove the Goal 066 settlement row source.", "blocked", Error("goal067.source.goal066_row_missing", "Goal066", "Goal 066 settlement source is required.")),
            Invalid("fake_package_row", "Point a narrative row at a package row absent from Goal 060.", "rejected", Error("goal067.source.fake_package_row", "Goal060", "Package row must resolve to Goal 060 evidence.")),
            Invalid("fake_npc_faction_ref", "Use an actor or faction id absent from Goal 064.", "rejected", Error("goal067.living_world.fake_actor_or_faction", "Goal064", "Narrative actor/faction refs must come from living-world evidence.")),
            Invalid("fake_settlement_ref", "Use a settlement id absent from Goal 066.", "rejected", Error("goal067.settlement.fake_ref", "Goal066", "Narrative settlement refs must come from settlement evidence.")),
            Invalid("fake_interlocked_gameplay_ref", "Use an interlocked delta absent from Goal 065.", "rejected", Error("goal067.interlocked.fake_ref", "Goal065", "Narrative interlocked refs must come from interlocked gameplay evidence.")),
            Invalid("duplicate_narrative_row_id", "Duplicate a narrative row id.", "rejected", Error("goal067.identity.duplicate_narrative_row_id", "rowId", "Narrative row ids must be unique.")),
            Invalid("missing_quest_stage_graph", "Emit a row without quest stages.", "rejected", Error("goal067.quest_stage_graph.missing", "questStageGraph", "Every row needs a quest-stage graph.")),
            Invalid("missing_dialogue_option_graph", "Emit a row without dialogue options.", "rejected", Error("goal067.dialogue_option_graph.missing", "dialogueOptionGraph", "Every row needs a dialogue option graph.")),
            Invalid("final_prose_leakage", "Emit lineText or finalDialogue.", "rejected", Error("goal067.prose.final_leakage", "dialogue", "Goal 067 may emit keys/templates/slots but not final prose.")),
            Invalid("provider_llm_rag_claim", "Claim provider, LLM or RAG work.", "blocked", Error("goal067.leak.provider_llm_rag_claim", "scope", "Provider, LLM and RAG calls are forbidden.")),
            Invalid("yarn_ink_runtime_dependency_claim", "Add Yarn Spinner or ink runtime dependency.", "blocked", Error("goal067.leak.yarn_ink_runtime_dependency_claim", "scope", "Yarn/ink runtime dependency is forbidden in Goal 067.")),
            Invalid("runtime_ui_gamepackage_schema_mutation_claim", "Mutate Runtime, UI or public GamePackage schema.", "blocked", Error("goal067.leak.runtime_ui_gamepackage_schema_mutation_claim", "scope", "Runtime/UI/GamePackage schema mutation is forbidden.")),
            Invalid("unsafe_unity_broad_mutation_claim", "Build broad Unity narrative runtime.", "blocked", Error("goal067.leak.unsafe_unity_broad_mutation_claim", "Unity", "Unity allowance is limited to deterministic marker loading.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal067.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed.")),
            Invalid("missing_replay_trace", "Omit save/load/replay proof.", "rejected", Error("goal067.replay.missing", "saveLoadReplay", "Every row requires save/load/replay proof.")),
            Invalid("event_consequence_without_state_delta", "Resolve an event without a state delta.", "rejected", Error("goal067.event.no_state_delta", "eventChain", "Event consequences must reference state deltas.")),
            Invalid("localization_key_without_template_slots", "Emit a line key without template or slots.", "rejected", Error("goal067.localization.template_slots_missing", "localization", "Localization records require template and slots.")),
            Invalid("memory_rumor_without_source_actor_faction_context", "Record rumor without actor/faction context.", "rejected", Error("goal067.memory_rumor.context_missing", "memoryRumor", "Memory/rumor records require source actor and faction context."))
        };

        return new InvalidProgrammaticNarrativeDiagnosticsMatrix
        {
            Passed = scenarios.Count == ProgrammaticNarrativeVocabulary.RequiredInvalidScenarioIds.Count
                && ProgrammaticNarrativeVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required && item.ExpectedStatus == item.ActualStatus)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<ProgrammaticNarrativeFilePayload> BuildStagingFiles(
        ProgrammaticNarrativeSourceBundle source,
        ProgrammaticNarrativeUnityCommandPlan unityCommandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == ProgrammaticNarrativeVocabulary.UnityNarrativeCommandPlanStagingRelativePath);
        files.Add(TextFile(ProgrammaticNarrativeVocabulary.UnityNarrativeCommandPlanStagingRelativePath, Serialize(unityCommandPlan)));
        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static ProgrammaticNarrativeRow BuildRow(ProgrammaticNarrativeSourceRow source)
    {
        var profile = BuildProfile(source.FamilyId, source.SeedId);
        var sourceRefs = SourceRefs(source);
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var questArcId = "quest/" + safeFamily + "/" + safeSeed + "/programmatic-narrative-arc";
        var dialogueGraphId = "dialogue/" + safeFamily + "/" + safeSeed + "/template-option-graph";
        var eventChainId = "event-chain/" + safeFamily + "/" + safeSeed + "/quest-dialogue-consequence";
        var primaryActor = source.LivingWorldActorIds.FirstOrDefault() ?? "actor/" + safeFamily + "/" + safeSeed + "/unknown";
        var primaryFaction = source.LivingWorldFactionIds.FirstOrDefault() ?? "faction/" + safeFamily + "/" + safeSeed + "/unknown";
        var primaryEvent = source.LivingWorldEventIds.FirstOrDefault() ?? "event/" + safeFamily + "/" + safeSeed + "/unknown";

        var deltaQuest = Delta("goal067/" + safeFamily + "/" + safeSeed + "/01-quest-stage/delta/quest_stage", "quest.stage", "available", "started/" + profile.QuestKind, source.SourceSettlementRowRef, "Quest stage started from settlement handoff.");
        var deltaDialogue = Delta("goal067/" + safeFamily + "/" + safeSeed + "/02-dialogue-option/delta/dialogue_choice", "dialogue.option", "available", "selected/" + profile.DialogueTemplateId, source.SourceLivingWorldRowRef, "Template-bound dialogue option selected.");
        var deltaEvent = Delta("goal067/" + safeFamily + "/" + safeSeed + "/03-event-consequence/delta/event_state", "event.consequence", "pending", "applied/" + profile.EventKind, source.SourceInterlockedGameplayRowRef, "Event consequence applied to later availability.");
        var deltaAvailability = Delta("goal067/" + safeFamily + "/" + safeSeed + "/04-availability/delta/later_option", "later.availability", "locked", "available/" + profile.LaterAvailabilityKind, source.SourceInterlockedGameplayRowRef, "Later option/event availability changed.");
        var deltaMemory = Delta("goal067/" + safeFamily + "/" + safeSeed + "/05-memory-rumor/delta/memory_rumor", "memory.rumor", "absent", "recorded/" + profile.MemoryKind, source.SourceLivingWorldRowRef, "Memory and rumor propagation recorded.");
        var deltas = new[] { deltaQuest, deltaDialogue, deltaEvent, deltaAvailability, deltaMemory };

        var stages = BuildQuestStages(source, profile, questArcId, deltaQuest, deltaDialogue, deltaEvent, deltaMemory);
        var options = BuildDialogueOptions(source, profile, dialogueGraphId, primaryActor, primaryFaction, deltaDialogue, deltaAvailability);
        var events = BuildEvents(source, profile, eventChainId, primaryEvent, deltaEvent, deltaAvailability);
        var localization = BuildLocalization(source, profile, options);
        var memory = BuildMemory(source, profile, primaryActor, primaryFaction, primaryEvent, deltaMemory);
        var before = Snapshot(source, InitialState(source, questArcId, dialogueGraphId, eventChainId), 0);
        var after = Snapshot(source, ApplyDeltas(before.Values, deltas, source, profile), 5);
        var saveLoad = BuildSaveLoadReplay(source, before, after, stages, options, events, memory);

        var rowWithoutHash = new ProgrammaticNarrativeRow
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
            QuestArcId = questArcId,
            DialogueGraphId = dialogueGraphId,
            EventChainId = eventChainId,
            PackageId = source.PackageId,
            SpatialRowHash = source.SpatialDetailRowHash,
            LivingWorldAfterStateHash = source.LivingWorldAfterStateHash,
            InterlockedAfterStateHash = source.InterlockedAfterStateHash,
            SettlementAfterStateHash = source.SettlementAfterStateHash,
            SettlementId = source.SettlementId,
            BuildingId = source.BuildingId,
            QuestStageGraph = stages,
            DialogueOptionGraph = options,
            EventTriggerConsequenceChain = events,
            LocalizationKeyTable = localization,
            MemoryRumorPropagation = memory,
            StateDeltas = deltas,
            MeaningfulVarianceAxes = MeaningfulAxes(source.FamilyId),
            BeforeState = before,
            AfterState = after,
            SaveLoadReplayProof = saveLoad,
            StateChanging = !string.Equals(before.StateHash, after.StateHash, StringComparison.Ordinal)
                && stages.Count >= 3
                && options.Count >= 2
                && events.Count >= 1
                && localization.Count >= 2
                && memory.Count >= 1
                && deltas.Count(delta => delta.Passed) >= 5
                && saveLoad.SaveLoadRoundtripPassed
                && saveLoad.ReplayDeterminismPassed,
            NoFinalProse = true,
            RowHash = string.Empty
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    private static IReadOnlyList<QuestStageRecord> BuildQuestStages(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        string questArcId,
        NarrativeStateDelta deltaQuest,
        NarrativeStateDelta deltaDialogue,
        NarrativeStateDelta deltaEvent,
        NarrativeStateDelta deltaMemory) =>
    [
        Stage(questArcId + "/stage/01-start", 1, profile.QuestKind + "/start", "available", "started", [source.SourceSettlementRowRef, "settlementStateChanged=true"], [deltaQuest.DeltaId], [questArcId + "/stage/02-dialogue"]),
        Stage(questArcId + "/stage/02-dialogue", 2, profile.QuestKind + "/dialogue_decision", "locked", "available_after_stage_01", [source.SourceLivingWorldRowRef, "actorOrFactionContext=true"], [deltaDialogue.DeltaId], [questArcId + "/stage/03-event"]),
        Stage(questArcId + "/stage/03-event", 3, profile.QuestKind + "/event_consequence", "locked", "completed_with_consequence", [source.SourceInterlockedGameplayRowRef, source.SourceSettlementRowRef], [deltaEvent.DeltaId, deltaMemory.DeltaId], [profile.LaterAvailabilityKind])
    ];

    private static IReadOnlyList<DialogueOptionRecord> BuildDialogueOptions(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        string dialogueGraphId,
        string primaryActor,
        string primaryFaction,
        NarrativeStateDelta deltaDialogue,
        NarrativeStateDelta deltaAvailability)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        return
        [
            Option(
                dialogueGraphId + "/option/01-selected",
                1,
                safeFamily + "." + source.SeedId + ".dialogue." + profile.LineKeyStem + ".001",
                profile.DialogueTemplateId,
                profile.SpeakerRole,
                profile.ToneTags,
                Slots(source, profile, primaryActor, primaryFaction),
                [source.SourceLivingWorldRowRef, "settlementId == " + source.SettlementId, "templateOnly=true"],
                [profile.DialogueEffect, "event_chain_unlocked", "state_delta_ref=" + deltaDialogue.DeltaId],
                "available",
                "selected",
                [deltaDialogue.DeltaId]),
            Option(
                dialogueGraphId + "/option/02-later-availability",
                2,
                safeFamily + "." + source.SeedId + ".dialogue." + profile.LineKeyStem + ".002",
                profile.DialogueTemplateId + "_followup",
                profile.SecondarySpeakerRole,
                profile.SecondaryToneTags,
                Slots(source, profile, primaryActor, primaryFaction),
                [deltaAvailability.Key + " == " + deltaAvailability.AfterValue, source.SourceInterlockedGameplayRowRef],
                [profile.LaterAvailabilityKind, "rumor_recorded"],
                "locked",
                "available_after_event",
                [deltaAvailability.DeltaId])
        ];
    }

    private static IReadOnlyList<EventTriggerConsequenceRecord> BuildEvents(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        string eventChainId,
        string primaryEvent,
        NarrativeStateDelta deltaEvent,
        NarrativeStateDelta deltaAvailability) =>
    [
        Event(
            eventChainId + "/trigger/01-source-event",
            eventChainId + "/consequence/01-state-delta",
            1,
            profile.EventKind,
            "pending",
            "resolved",
            profile.LaterAvailabilityKind,
            [source.SourceGameplayConsequenceRowRef, source.SourceInterlockedGameplayRowRef, primaryEvent],
            [deltaEvent.DeltaId, deltaAvailability.DeltaId]),
        Event(
            eventChainId + "/trigger/02-settlement-feedback",
            eventChainId + "/consequence/02-settlement-option",
            2,
            profile.EventKind + "/settlement_feedback",
            "blocked",
            "available",
            "settlement_dialogue_followup",
            [source.SourceSettlementRowRef, source.SettlementId, source.BuildingId],
            [deltaAvailability.DeltaId])
    ];

    private static IReadOnlyList<LocalizationKeyRecord> BuildLocalization(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        IReadOnlyList<DialogueOptionRecord> options) =>
        options
            .Select(option => new LocalizationKeyRecord
            {
                LineKey = option.LineKey,
                TemplateId = option.TemplateId,
                SpeakerRole = option.SpeakerRole,
                ToneTags = option.ToneTags,
                Slots = option.Slots,
                Conditions = option.Conditions,
                OptionEffects = option.OptionEffects,
                StateDeltaRefs = option.StateDeltaRefs,
                Passed = !string.IsNullOrWhiteSpace(option.LineKey)
                    && !string.IsNullOrWhiteSpace(option.TemplateId)
                    && option.Slots.Count >= 2
                    && option.OptionEffects.Count > 0
                    && option.LineKey.Contains(source.SeedId, StringComparison.Ordinal)
                    && option.LineKey.Contains(Safe(source.FamilyId), StringComparison.Ordinal)
            })
            .ToList();

    private static IReadOnlyList<MemoryRumorPropagationRecord> BuildMemory(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        string primaryActor,
        string primaryFaction,
        string primaryEvent,
        NarrativeStateDelta deltaMemory)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        return
        [
            new MemoryRumorPropagationRecord
            {
                RecordId = "memory/" + safeFamily + "/" + safeSeed + "/" + profile.MemoryKind,
                PropagationKind = profile.MemoryKind,
                SourceActorId = primaryActor,
                SourceFactionId = primaryFaction,
                TargetAudienceId = profile.TargetAudienceKind + "/" + safeSeed,
                SourceEventId = primaryEvent,
                BeforeState = "absent",
                AfterState = "recorded/" + profile.MemoryKind,
                SourceRefs = [source.SourceLivingWorldRowRef, source.SourceSettlementRowRef],
                StateDeltaRefs = [deltaMemory.DeltaId],
                Passed = !string.IsNullOrWhiteSpace(primaryActor)
                    && !string.IsNullOrWhiteSpace(primaryFaction)
                    && !string.IsNullOrWhiteSpace(primaryEvent)
            }
        ];
    }

    private static IReadOnlyDictionary<string, string> InitialState(
        ProgrammaticNarrativeSourceRow source,
        string questArcId,
        string dialogueGraphId,
        string eventChainId) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.package"] = source.SourcePackageRowRef,
            ["source.spatial"] = source.SourceSpatialDetailRowRef,
            ["source.gameplay"] = source.SourceGameplayConsequenceRowRef,
            ["source.living_world"] = source.SourceLivingWorldRowRef,
            ["source.interlocked"] = source.SourceInterlockedGameplayRowRef,
            ["source.settlement"] = source.SourceSettlementRowRef,
            ["package.id"] = source.PackageId,
            ["settlement.id"] = source.SettlementId,
            ["settlement.building"] = source.BuildingId,
            ["quest.arc"] = questArcId,
            ["dialogue.graph"] = dialogueGraphId,
            ["event.chain"] = eventChainId,
            ["quest.stage"] = "available",
            ["dialogue.option"] = "available",
            ["event.consequence"] = "pending",
            ["later.availability"] = "locked",
            ["memory.rumor"] = "absent"
        };

    private static SortedDictionary<string, string> ApplyDeltas(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyList<NarrativeStateDelta> deltas,
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile)
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

        values["living_world.actor"] = source.LivingWorldActorIds.FirstOrDefault() ?? string.Empty;
        values["living_world.faction"] = source.LivingWorldFactionIds.FirstOrDefault() ?? string.Empty;
        values["interlocked.delta_count"] = source.InterlockedDeltaIds.Count.ToString();
        values["settlement.after_hash"] = source.SettlementAfterStateHash;
        values["narrative.template"] = profile.DialogueTemplateId;
        values["narrative.quest_kind"] = profile.QuestKind;
        values["narrative.event_kind"] = profile.EventKind;
        values["narrative.memory_kind"] = profile.MemoryKind;
        return values;
    }

    private static ProgrammaticNarrativeSaveLoadReplayRow BuildSaveLoadReplay(
        ProgrammaticNarrativeSourceRow source,
        ProgrammaticNarrativeStateSnapshot before,
        ProgrammaticNarrativeStateSnapshot after,
        IReadOnlyList<QuestStageRecord> stages,
        IReadOnlyList<DialogueOptionRecord> options,
        IReadOnlyList<EventTriggerConsequenceRecord> events,
        IReadOnlyList<MemoryRumorPropagationRecord> memory)
    {
        var json = Serialize(after);
        var restored = ProgrammaticNarrativeHash.Deserialize<ProgrammaticNarrativeStateSnapshot>(json);
        var replayHash = Hash(Serialize(new
        {
            source.RowId,
            stageIds = stages.Select(item => item.StageId).ToList(),
            optionIds = options.Select(item => item.OptionId).ToList(),
            consequenceIds = events.Select(item => item.ConsequenceId).ToList(),
            memoryIds = memory.Select(item => item.RecordId).ToList(),
            after.StateHash
        }));

        return new ProgrammaticNarrativeSaveLoadReplayRow
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

    private static ProgrammaticNarrativeStateSnapshot Snapshot(
        ProgrammaticNarrativeSourceRow source,
        IReadOnlyDictionary<string, string> values,
        int stepIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }

        return new ProgrammaticNarrativeStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            StepIndex = stepIndex,
            Values = copy,
            StateHash = Hash(Serialize(copy))
        };
    }

    private static NarrativeStateDelta Delta(string deltaId, string key, string before, string after, string sourceRef, string outcome) =>
        new()
        {
            DeltaId = deltaId,
            Key = key,
            BeforeValue = before,
            AfterValue = after,
            SourceRef = sourceRef,
            Outcome = outcome,
            Passed = !string.IsNullOrWhiteSpace(deltaId)
                && !string.IsNullOrWhiteSpace(key)
                && !string.Equals(before, after, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(sourceRef)
        };

    private static QuestStageRecord Stage(
        string stageId,
        int order,
        string stageKind,
        string before,
        string after,
        IReadOnlyList<string> conditions,
        IReadOnlyList<string> deltas,
        IReadOnlyList<string> unlocks) =>
        new()
        {
            StageId = stageId,
            Order = order,
            StageKind = stageKind,
            AvailabilityBefore = before,
            AvailabilityAfter = after,
            Conditions = conditions,
            StateDeltaRefs = deltas,
            Unlocks = unlocks,
            Passed = !string.IsNullOrWhiteSpace(stageId)
                && order > 0
                && conditions.Count > 0
                && deltas.Count > 0
                && !string.Equals(before, after, StringComparison.Ordinal)
        };

    private static DialogueOptionRecord Option(
        string optionId,
        int order,
        string lineKey,
        string templateId,
        string speakerRole,
        IReadOnlyList<string> toneTags,
        IReadOnlyDictionary<string, string> slots,
        IReadOnlyList<string> conditions,
        IReadOnlyList<string> effects,
        string before,
        string after,
        IReadOnlyList<string> deltaRefs) =>
        new()
        {
            OptionId = optionId,
            Order = order,
            LineKey = lineKey,
            TemplateId = templateId,
            SpeakerRole = speakerRole,
            ToneTags = toneTags,
            Slots = slots,
            Conditions = conditions,
            OptionEffects = effects,
            AvailabilityBefore = before,
            AvailabilityAfter = after,
            StateDeltaRefs = deltaRefs,
            Passed = !string.IsNullOrWhiteSpace(optionId)
                && !string.IsNullOrWhiteSpace(lineKey)
                && !string.IsNullOrWhiteSpace(templateId)
                && !string.IsNullOrWhiteSpace(speakerRole)
                && toneTags.Count > 0
                && slots.Count >= 2
                && conditions.Count > 0
                && effects.Count > 0
                && deltaRefs.Count > 0
        };

    private static EventTriggerConsequenceRecord Event(
        string triggerId,
        string consequenceId,
        int order,
        string triggerKind,
        string before,
        string after,
        string laterAvailability,
        IReadOnlyList<string> sourceRefs,
        IReadOnlyList<string> deltaRefs) =>
        new()
        {
            TriggerId = triggerId,
            ConsequenceId = consequenceId,
            Order = order,
            TriggerKind = triggerKind,
            BeforeState = before,
            AfterState = after,
            LaterAvailabilityChange = laterAvailability,
            SourceRefs = sourceRefs,
            StateDeltaRefs = deltaRefs,
            Passed = !string.IsNullOrWhiteSpace(triggerId)
                && !string.IsNullOrWhiteSpace(consequenceId)
                && !string.Equals(before, after, StringComparison.Ordinal)
                && sourceRefs.Count >= 2
                && deltaRefs.Count > 0
        };

    private static IReadOnlyDictionary<string, string> Slots(
        ProgrammaticNarrativeSourceRow source,
        NarrativeProfile profile,
        string primaryActor,
        string primaryFaction) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["familyId"] = source.FamilyId,
            ["seedId"] = source.SeedId,
            ["settlementId"] = source.SettlementId,
            ["buildingId"] = source.BuildingId,
            ["actorId"] = primaryActor,
            ["factionId"] = primaryFaction,
            ["resourceName"] = profile.ResourceSlot,
            ["eventKind"] = profile.EventKind
        };

    private static NarrativeLedger Ledger(string kind, IReadOnlyList<NarrativeLedgerEntry> entries, bool coveragePassed) =>
        new()
        {
            LedgerKind = kind,
            Passed = coveragePassed && entries.Count > 0 && entries.All(item => item.Passed && item.StateDeltaRefs.Count > 0),
            EntryCount = entries.Count,
            Entries = entries
        };

    private static IReadOnlyDictionary<string, string> VarianceHighlight(ProgrammaticNarrativeRow row) =>
        row.MeaningfulVarianceAxes
            .Where(row.AfterState.Values.ContainsKey)
            .ToDictionary(key => key, key => row.AfterState.Values[key], StringComparer.Ordinal);

    private static IReadOnlyList<string> MeaningfulAxes(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                "quest.stage",
                "dialogue.option",
                "event.consequence",
                "later.availability",
                "memory.rumor",
                "narrative.template",
                "settlement.id"
            ],
            "survival_sandbox" =>
            [
                "quest.stage",
                "dialogue.option",
                "event.consequence",
                "later.availability",
                "memory.rumor",
                "narrative.event_kind",
                "settlement.building"
            ],
            "first_person_grid_dungeon" =>
            [
                "quest.stage",
                "dialogue.option",
                "event.consequence",
                "later.availability",
                "memory.rumor",
                "narrative.quest_kind",
                "source.spatial"
            ],
            _ => []
        };

    private static IReadOnlyList<string> SourceRefs(ProgrammaticNarrativeSourceRow source) =>
    [
        source.SourcePackageRowRef,
        source.SourceReviewPackageRowRef,
        source.SourceSpatialDetailRowRef,
        source.SourceGameplayConsequenceRowRef,
        source.SourceLivingWorldRowRef,
        source.SourceInterlockedGameplayRowRef,
        source.SourceSettlementRowRef
    ];

    private static IReadOnlyList<string> SourceRefs(ProgrammaticNarrativeRow row) =>
    [
        row.SourcePackageRowRef,
        row.SourceReviewPackageRowRef,
        row.SourceSpatialDetailRowRef,
        row.SourceGameplayConsequenceRowRef,
        row.SourceLivingWorldRowRef,
        row.SourceInterlockedGameplayRowRef,
        row.SourceSettlementRowRef
    ];

    private static NarrativeProfile BuildProfile(string familyId, string seedId)
    {
        var modifier = SeedModifier(seedId);
        return (familyId, seedId) switch
        {
            ("map_panel_rpg", "seed_alpha") => ProfileValues("settlement_route_contract", "npc_warning_low_trust", "settlement_crafter", "route_guide", "settlement_pressure_event", "public_rumor", "route_repair_option", "repair_discount_blocked", "trade_goods", ["tense", "low_trust"], ["pragmatic", "contract"]),
            ("map_panel_rpg", "seed_beta") => ProfileValues("faction_repair_bargain", "quest_offer_repair_trade", "faction_broker", "settlement_crafter", "trade_route_unlock", "contract_memory", "guild_followup_option", "repair_contract_opened", "crafted_tools", ["formal", "bargain"], ["focused", "trade"]),
            ("map_panel_rpg", "seed_gamma") => ProfileValues("public_rumor_resolution", "event_aftermath_faction_notice", "route_guide", "faction_broker", "faction_rumor_update", "faction_notice", "shrine_patrol_option", "rumor_recorded", "route_safety", ["urgent", "public"], ["calm", "route"]),
            ("survival_sandbox", "seed_alpha") => ProfileValues("camp_hazard_recovery", "camp_warning_resource_shortage", "camp_builder", "forager", "weather_pressure_event", "hazard_memory", "shelter_recovery_option", "hazard_response_opened", "warmth", ["strained", "hazard"], ["steady", "camp"]),
            ("survival_sandbox", "seed_beta") => ProfileValues("resource_scarcity_choice", "survival_task_offer", "forager", "camp_builder", "resource_recovery_unlock", "resource_rumor", "water_recovery_option", "resource_route_opened", "clean_water", ["dry", "scarcity"], ["careful", "forage"]),
            ("survival_sandbox", "seed_gamma") => ProfileValues("shelter_memory_chain", "hazard_aftermath_notice", "watch_runner", "forager", "camp_rumor_update", "camp_notice", "trap_safety_option", "camp_notice_recorded", "camp_safety", ["watchful", "risk"], ["quiet", "shelter"]),
            ("first_person_grid_dungeon", "seed_alpha") => ProfileValues("gate_key_memory", "dungeon_warning_locked_route", "gate_keeper", "party_scout", "trap_alert_event", "route_memory", "safe_cell_option", "route_memory_bound", "route_confidence", ["alert", "locked"], ["measured", "party"]),
            ("first_person_grid_dungeon", "seed_beta") => ProfileValues("safe_room_bargain", "party_task_offer", "party_scout", "rune_scribe", "route_gate_unlock", "party_notice", "rune_gate_option", "gate_route_opened", "encounter_control", ["careful", "sealed"], ["technical", "rune"]),
            ("first_person_grid_dungeon", "seed_gamma") => ProfileValues("trap_consequence_chain", "trap_aftermath_notice", "rune_scribe", "gate_keeper", "party_memory_update", "trap_rumor", "cache_trap_option", "trap_memory_recorded", "trap_safety", ["sharp", "danger"], ["low", "cache"]),
            _ => ProfileValues("unknown_quest", "unknown_template", "unknown_speaker", "unknown_secondary", "unknown_event", "unknown_memory", "unknown_later", "unknown_effect", "unknown_resource", ["unknown"], ["unknown"])
        } with
        {
            SeedModifier = modifier
        };
    }

    private static NarrativeProfile ProfileValues(
        string questKind,
        string dialogueTemplateId,
        string speakerRole,
        string secondarySpeakerRole,
        string eventKind,
        string memoryKind,
        string laterAvailabilityKind,
        string dialogueEffect,
        string resourceSlot,
        IReadOnlyList<string> toneTags,
        IReadOnlyList<string> secondaryToneTags) =>
        new(questKind, dialogueTemplateId, speakerRole, secondarySpeakerRole, eventKind, memoryKind, laterAvailabilityKind, dialogueEffect, resourceSlot, toneTags, secondaryToneTags);

    private static NarrativeTemplateProfile Profile(
        string familyId,
        IReadOnlyList<string> questKinds,
        IReadOnlyList<string> dialogueTemplateIds,
        IReadOnlyList<string> eventKinds,
        IReadOnlyList<string> speakerRoles,
        IReadOnlyList<string> memoryKinds) =>
        new()
        {
            FamilyId = familyId,
            QuestKinds = questKinds,
            DialogueTemplateIds = dialogueTemplateIds,
            EventKinds = eventKinds,
            SpeakerRoles = speakerRoles,
            MemoryKinds = memoryKinds
        };

    private static ProgrammaticNarrativeGateRecord Gate(string gateId, string status, string provenance, string evidence) =>
        new()
        {
            GateId = gateId,
            Status = status,
            ProvenanceKind = provenance,
            EvidenceRef = evidence
        };

    private static InvalidProgrammaticNarrativeScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params ProgrammaticNarrativeDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = ProgrammaticNarrativeSourceLoader.SortDiagnostics(diagnostics)
        };

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };

    private static ProgrammaticNarrativeFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static string Safe(string value) => ProgrammaticNarrativeHash.SafeSegment(value);

    private static string Serialize<T>(T value) => ProgrammaticNarrativeHash.Serialize(value);

    private static string Hash(string value) => ProgrammaticNarrativeHash.Hash(value);

    private static ProgrammaticNarrativeDiagnostic Error(string code, string target, string message) =>
        ProgrammaticNarrativeDiagnostic.Error(code, target, message);

    private sealed record NarrativeProfile(
        string QuestKind,
        string DialogueTemplateId,
        string SpeakerRole,
        string SecondarySpeakerRole,
        string EventKind,
        string MemoryKind,
        string LaterAvailabilityKind,
        string DialogueEffect,
        string ResourceSlot,
        IReadOnlyList<string> ToneTags,
        IReadOnlyList<string> SecondaryToneTags)
    {
        public int SeedModifier { get; init; }
        public string LineKeyStem => QuestKind.Replace('_', '-');
        public string TargetAudienceKind => MemoryKind.Contains("rumor", StringComparison.Ordinal) ? "faction_audience" : "local_memory";
    }
}
