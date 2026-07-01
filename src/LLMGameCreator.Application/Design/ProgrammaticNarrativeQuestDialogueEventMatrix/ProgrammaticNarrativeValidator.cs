namespace LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;

public sealed class ProgrammaticNarrativeValidator
{
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateSourceManifest(ProgrammaticNarrativeSourceManifest manifest)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal067.gate.self_pass.forbidden", "source-manifest", "Goal 067 must not mark its own manual gate passed."));
        }

        if (!manifest.Goal066AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "settlement_construction_destruction_production_matrix_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal067.preflight.goal066_handoff_missing", "source-manifest", "Goal 066 acceptance by user handoff is required before Goal 067."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == ProgrammaticNarrativeVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal067.gate.required_missing", "source-manifest", "Goal 067 gate must remain required."));
        }

        if (manifest.RowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal067.source.matrix_counts_invalid", "source-manifest", "Goal 067 requires 9 rows across 3 families and 3 seeds."));
        }

        if (!manifest.Goal060PackageRowsConsumed
            || !manifest.Goal061ReviewPackageRcConsumed
            || !manifest.Goal062SpatialRowsConsumed
            || !manifest.Goal063GameplayRowsConsumed
            || !manifest.Goal064LivingWorldRowsConsumed
            || !manifest.Goal065InterlockedRowsConsumed
            || !manifest.Goal066SettlementRowsConsumed)
        {
            diagnostics.Add(Error("goal067.source.chain_incomplete", "source-manifest", "Goal 067 must consume Goal 060/061/062/063/064/065/066 evidence."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateRows(
        ProgrammaticNarrativeTemplateCatalog catalog,
        ProgrammaticNarrativeRowMatrix matrix,
        ProgrammaticNarrativePreviewExportPayload previewPayload,
        bool meaningfulVariancePassed)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        if (!catalog.Passed || catalog.ProfileCount != 3)
        {
            diagnostics.Add(Error("goal067.catalog.invalid", "template-catalog", "Narrative template catalog must define all three family profiles."));
        }

        if (!matrix.Passed || matrix.Accepted || matrix.RowCount != 9 || matrix.StateChangingRowCount != 9 || matrix.DistinctRowHashCount != 9)
        {
            diagnostics.Add(Error("goal067.matrix.invalid", "narrative-row-matrix", "Narrative row matrix must contain 9 produced-for-review state-changing rows with distinct hashes."));
        }

        if (matrix.Rows.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal067.identity.duplicate_narrative_row_id", "rowId", "Narrative row ids must be unique."));
        }

        foreach (var familyId in ProgrammaticNarrativeVocabulary.FamilyIds)
        {
            foreach (var seedId in ProgrammaticNarrativeVocabulary.SeedIds)
            {
                if (!matrix.Rows.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal067.matrix.row_missing", familyId + "/" + seedId, "Required narrative row is missing."));
                }
            }
        }

        foreach (var row in matrix.Rows)
        {
            ValidateRow(row, diagnostics);
        }

        if (!meaningfulVariancePassed)
        {
            diagnostics.Add(Error("goal067.variance.invalid", "narrative-row-matrix", "Variance must prove same-family seed and cross-family narrative differences beyond ids/hashes."));
        }

        if (!previewPayload.Passed || previewPayload.RowCount != 9)
        {
            diagnostics.Add(Error("goal067.preview.payload_invalid", "narrative-preview-export-payload", "Preview/export narrative payload must cover all 9 rows."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateLedgers(
        NarrativeLedger questStage,
        NarrativeLedger dialogueOption,
        NarrativeLedger eventConsequence,
        LocalizationKeyTable localizationKeyTable,
        NarrativeLedger memoryRumor)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        if (!questStage.Passed || questStage.Entries.Count(item => item.LedgerKind == "quest_stage") < 27)
        {
            diagnostics.Add(Error("goal067.ledger.quest_stage_invalid", "quest-stage-ledger", "Quest stage ledger must include at least 3 ordered stages for every row."));
        }

        if (!dialogueOption.Passed || dialogueOption.Entries.Count(item => item.LedgerKind == "dialogue_option") < 18)
        {
            diagnostics.Add(Error("goal067.ledger.dialogue_option_invalid", "dialogue-option-ledger", "Dialogue option ledger must include template-bound options for every row."));
        }

        if (!eventConsequence.Passed || eventConsequence.Entries.Count(item => item.LedgerKind == "event_trigger_consequence") < 18)
        {
            diagnostics.Add(Error("goal067.ledger.event_consequence_invalid", "event-trigger-consequence-ledger", "Event consequence ledger must include state-changing consequences for every row."));
        }

        if (!localizationKeyTable.Passed || localizationKeyTable.EntryCount < 18)
        {
            diagnostics.Add(Error("goal067.localization.table_invalid", "localization-key-table", "Localization key table must include line keys, templates and slots."));
        }

        if (!memoryRumor.Passed || memoryRumor.EntryCount < 9)
        {
            diagnostics.Add(Error("goal067.memory_rumor.ledger_invalid", "memory-rumor-propagation-ledger", "Memory/rumor ledger must include actor/faction context for every row."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateReplay(ProgrammaticNarrativeSaveLoadReplayProof replay)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        if (!replay.Passed
            || replay.RowCount != 9
            || replay.StateChangedRowCount != 9
            || replay.SaveLoadPassedRowCount != 9
            || replay.ReplayPassedRowCount != 9)
        {
            diagnostics.Add(Error("goal067.replay.audit_invalid", "narrative-save-load-replay-proof", "Save/load and replay proof must pass for all 9 rows."));
        }

        foreach (var row in replay.Rows)
        {
            if (!row.BeforeAfterStateChanged || row.BeforeStateHash == row.AfterStateHash)
            {
                diagnostics.Add(Error("goal067.state.before_after_equal", row.RowId, "Before and after narrative state hashes must differ."));
            }

            if (!row.SaveLoadRoundtripPassed || row.SerializedAfterStateHash != row.RestoredAfterStateHash)
            {
                diagnostics.Add(Error("goal067.save_load.mismatch", row.RowId, "Save/load roundtrip did not preserve narrative after-state hash."));
            }

            if (!row.ReplayDeterminismPassed || row.FirstReplayHash != row.SecondReplayHash)
            {
                diagnostics.Add(Error("goal067.replay.mismatch", row.RowId, "Replay hashes must match for same narrative input."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateUnityCommandPlan(ProgrammaticNarrativeUnityCommandPlan commandPlan)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal067.unity.command_plan_invalid", "narrative-unity-command-plan", "Unity command plan must cover all 9 narrative rows and stay accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal067.unity.marker_missing", marker, "Unity command plan is missing a required global narrative marker."));
            }
        }

        foreach (var row in commandPlan.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.QuestStageId)
                || string.IsNullOrWhiteSpace(row.DialogueOptionId)
                || string.IsNullOrWhiteSpace(row.EventTriggerId)
                || string.IsNullOrWhiteSpace(row.EventConsequenceId)
                || string.IsNullOrWhiteSpace(row.MemoryRumorRecordId)
                || string.IsNullOrWhiteSpace(row.LocalizationLineKey))
            {
                diagnostics.Add(Error("goal067.unity.row_marker_plan_shallow", row.RowId, "Every Unity row marker plan must include quest, dialogue, event, memory/rumor and localization ids."));
            }

            foreach (var marker in RowMarkers(row))
            {
                if (!row.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
                {
                    diagnostics.Add(Error("goal067.unity.row_marker_missing", row.RowId + "#" + marker, "Every Unity row marker plan needs row, quest, dialogue, event, memory, localization and completion markers."));
                }
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateUnityProof(
        ProgrammaticNarrativeUnityCommandPlan commandPlan,
        ProgrammaticNarrativeUnityProofSummary proof)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerExecuted && !proof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal067.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 067 marker."));
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
                diagnostics.Add(Error("goal067.unity.proof_inconsistent", "narrative-unity-player-proof-summary", "Passed Unity proof must have zero exit codes and all 9 rows."));
            }
        }
        else if (proof.Diagnostics.Count == 0)
        {
            diagnostics.Add(Error("goal067.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry exact diagnostics."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> ValidateInvalidMatrix(InvalidProgrammaticNarrativeDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        foreach (var scenarioId in ProgrammaticNarrativeVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal067.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal067.invalid.matrix_failed", "narrative-invalid-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "narrative_matrix_loaded=goal067",
        "narrative_matrix_completed=true",
        "review_package_proof=goal067",
        "programmatic_narrative_quest_dialogue_event_matrix_verification=required"
    ];

    public static IReadOnlyList<ProgrammaticNarrativeDiagnostic> Sort(IEnumerable<ProgrammaticNarrativeDiagnostic> diagnostics) =>
        ProgrammaticNarrativeSourceLoader.SortDiagnostics(diagnostics);

    private static void ValidateRow(ProgrammaticNarrativeRow row, List<ProgrammaticNarrativeDiagnostic> diagnostics)
    {
        if (!row.StateChanging || row.BeforeState.StateHash == row.AfterState.StateHash)
        {
            diagnostics.Add(Error("goal067.state.non_state_changing_row", row.RowId, "Every narrative row must change before/after state."));
        }

        if (row.QuestStageGraph.Count < 3 || !IsStrictlyOrdered(row.QuestStageGraph.Select(item => item.Order)))
        {
            diagnostics.Add(Error("goal067.quest_stage_graph.missing", row.RowId, "Every row needs at least 3 ordered quest stages."));
        }

        if (row.DialogueOptionGraph.Count < 2 || !row.DialogueOptionGraph.All(item => item.Passed && item.StateDeltaRefs.Count > 0))
        {
            diagnostics.Add(Error("goal067.dialogue_option_graph.missing", row.RowId, "Every row needs template-bound dialogue options with state deltas."));
        }

        if (row.EventTriggerConsequenceChain.Count == 0 || row.EventTriggerConsequenceChain.Any(item => !item.Passed || item.StateDeltaRefs.Count == 0 || string.IsNullOrWhiteSpace(item.LaterAvailabilityChange)))
        {
            diagnostics.Add(Error("goal067.event.no_state_delta", row.RowId, "Every event consequence must change state and later availability."));
        }

        if (row.LocalizationKeyTable.Count == 0 || row.LocalizationKeyTable.Any(item => !item.Passed || item.Slots.Count == 0 || string.IsNullOrWhiteSpace(item.TemplateId)))
        {
            diagnostics.Add(Error("goal067.localization.template_slots_missing", row.RowId, "Localization records require lineKey, templateId and slots."));
        }

        if (row.MemoryRumorPropagation.Count == 0 || row.MemoryRumorPropagation.Any(item => !item.Passed || string.IsNullOrWhiteSpace(item.SourceActorId) || string.IsNullOrWhiteSpace(item.SourceFactionId)))
        {
            diagnostics.Add(Error("goal067.memory_rumor.context_missing", row.RowId, "Memory/rumor records require source actor and faction context."));
        }

        if (row.StateDeltas.Count < 2 || row.StateDeltas.Any(item => !item.Passed))
        {
            diagnostics.Add(Error("goal067.state_delta.missing", row.RowId, "Every row must include at least two meaningful state deltas."));
        }

        if (string.IsNullOrWhiteSpace(row.SourceLivingWorldRowRef)
            || string.IsNullOrWhiteSpace(row.SourceInterlockedGameplayRowRef)
            || string.IsNullOrWhiteSpace(row.SourceSettlementRowRef)
            || string.IsNullOrWhiteSpace(row.SourcePackageRowRef)
            || string.IsNullOrWhiteSpace(row.SourceSpatialDetailRowRef))
        {
            diagnostics.Add(Error("goal067.source.ref_missing", row.RowId, "Narrative rows must link living-world, interlocked, settlement, package and spatial rows."));
        }

        ValidateNoFinalProse(row, diagnostics);
    }

    private static void ValidateNoFinalProse(ProgrammaticNarrativeRow row, List<ProgrammaticNarrativeDiagnostic> diagnostics)
    {
        var serialized = ProgrammaticNarrativeHash.Serialize(row);
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
            "YarnRuntime",
            "InkRuntime"
        };
        foreach (var token in forbidden)
        {
            if (serialized.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("goal067.prose.final_leakage", row.RowId + "#" + token, "Goal 067 may not emit final prose or external narrative/runtime dependency claims."));
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

    private static IReadOnlyList<string> RowMarkers(ProgrammaticNarrativeUnityCommandRow row) =>
    [
        "narrative_row_loaded=" + row.RowId,
        "narrative_family=" + row.FamilyId,
        "narrative_seed=" + row.SeedId,
        "quest_stage_started=" + row.QuestStageId,
        "dialogue_option_available=" + row.DialogueOptionId,
        "dialogue_option_selected=" + row.DialogueOptionId,
        "event_trigger_resolved=" + row.EventTriggerId,
        "event_consequence_applied=" + row.EventConsequenceId,
        "memory_rumor_recorded=" + row.MemoryRumorRecordId,
        "localization_key_bound=" + row.LocalizationLineKey,
        "narrative_row_completed=" + row.RowId
    ];

    private static ProgrammaticNarrativeDiagnostic Error(string code, string target, string message) =>
        ProgrammaticNarrativeDiagnostic.Error(code, target, message);
}
