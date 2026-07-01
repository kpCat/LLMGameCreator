namespace LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;

public static class ProgrammaticNarrativeVocabulary
{
    public const string GoalId = "goal_067_programmatic_narrative_quest_dialogue_event_matrix";
    public const string ProductSmokeRoute = "goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    public const string FinalGate = "programmatic_narrative_quest_dialogue_event_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal065RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string Goal066RelativeOutputDirectory = ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    public const string StagingRoot = "staging";
    public const string UnityNarrativeCommandPlanStagingRelativePath = "narrative/unity-narrative-command-plan.json";

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyList<string> SeedIds =
    [
        "seed_alpha",
        "seed_beta",
        "seed_gamma"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal066_source",
        "fake_package_row",
        "fake_npc_faction_ref",
        "fake_settlement_ref",
        "fake_interlocked_gameplay_ref",
        "duplicate_narrative_row_id",
        "missing_quest_stage_graph",
        "missing_dialogue_option_graph",
        "final_prose_leakage",
        "provider_llm_rag_claim",
        "yarn_ink_runtime_dependency_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "unsafe_unity_broad_mutation_claim",
        "nondeterministic_ordering",
        "missing_replay_trace",
        "event_consequence_without_state_delta",
        "localization_key_without_template_slots",
        "memory_rumor_without_source_actor_faction_context"
    ];

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string SeedOrderingKey(string seedId) =>
        seedId switch
        {
            "seed_alpha" => "001-seed-alpha",
            "seed_beta" => "002-seed-beta",
            "seed_gamma" => "003-seed-gamma",
            _ => "999-" + seedId
        };
}

public sealed record ProgrammaticNarrativeOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record ProgrammaticNarrativeDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ProgrammaticNarrativeDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static ProgrammaticNarrativeDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static ProgrammaticNarrativeDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record ProgrammaticNarrativeFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record ProgrammaticNarrativeSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ProgrammaticNarrativeSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string SourceInterlockedGameplayRowRef { get; init; } = string.Empty;
    public string SourceSettlementRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string SpatialVarianceMarker { get; init; } = string.Empty;
    public string GameplayAfterStateHash { get; init; } = string.Empty;
    public string LivingWorldRowHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedRowHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public string SettlementRowHash { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public string LivingWorldLinkageId { get; init; } = string.Empty;
    public string InterlockedDependencyId { get; init; } = string.Empty;
    public bool Goal060PackageValid { get; init; }
    public bool Goal061ReviewPackageRcExists { get; init; }
    public bool Goal062SpatialRowValid { get; init; }
    public bool Goal063GameplayRowValid { get; init; }
    public bool Goal064LivingWorldRowValid { get; init; }
    public bool Goal065InterlockedRowValid { get; init; }
    public bool Goal066SettlementRowValid { get; init; }
    public bool Goal066SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> GameplayDeltaIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldActorIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldFactionIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldEventIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldMemoryRumorIds { get; init; } = [];
    public IReadOnlyList<string> InterlockedDeltaIds { get; init; } = [];
    public IReadOnlyList<string> SettlementLedgerEntryIds { get; init; } = [];
}

public sealed record ProgrammaticNarrativeSourceBundle
{
    public bool Goal066AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal066UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ProgrammaticNarrativeGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record ProgrammaticNarrativeSourceManifest
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_source_manifest_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ProgrammaticNarrativeVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = ProgrammaticNarrativeVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal066AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal066UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record NarrativeTemplateProfile
{
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> QuestKinds { get; init; } = [];
    public IReadOnlyList<string> DialogueTemplateIds { get; init; } = [];
    public IReadOnlyList<string> EventKinds { get; init; } = [];
    public IReadOnlyList<string> SpeakerRoles { get; init; } = [];
    public IReadOnlyList<string> MemoryKinds { get; init; } = [];
}

public sealed record ProgrammaticNarrativeTemplateCatalog
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_template_catalog_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ProfileCount { get; init; }
    public IReadOnlyList<NarrativeTemplateProfile> Profiles { get; init; } = [];
}

public sealed record NarrativeStateDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record QuestStageRecord
{
    public string StageId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string StageKind { get; init; } = string.Empty;
    public string AvailabilityBefore { get; init; } = string.Empty;
    public string AvailabilityAfter { get; init; } = string.Empty;
    public IReadOnlyList<string> Conditions { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public IReadOnlyList<string> Unlocks { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record DialogueOptionRecord
{
    public string OptionId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string LineKey { get; init; } = string.Empty;
    public string TemplateId { get; init; } = string.Empty;
    public string SpeakerRole { get; init; } = string.Empty;
    public IReadOnlyList<string> ToneTags { get; init; } = [];
    public IReadOnlyDictionary<string, string> Slots { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> Conditions { get; init; } = [];
    public IReadOnlyList<string> OptionEffects { get; init; } = [];
    public string AvailabilityBefore { get; init; } = string.Empty;
    public string AvailabilityAfter { get; init; } = string.Empty;
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record EventTriggerConsequenceRecord
{
    public string TriggerId { get; init; } = string.Empty;
    public string ConsequenceId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string TriggerKind { get; init; } = string.Empty;
    public string BeforeState { get; init; } = string.Empty;
    public string AfterState { get; init; } = string.Empty;
    public string LaterAvailabilityChange { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record LocalizationKeyRecord
{
    public string LineKey { get; init; } = string.Empty;
    public string TemplateId { get; init; } = string.Empty;
    public string SpeakerRole { get; init; } = string.Empty;
    public IReadOnlyList<string> ToneTags { get; init; } = [];
    public IReadOnlyDictionary<string, string> Slots { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> Conditions { get; init; } = [];
    public IReadOnlyList<string> OptionEffects { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record MemoryRumorPropagationRecord
{
    public string RecordId { get; init; } = string.Empty;
    public string PropagationKind { get; init; } = string.Empty;
    public string SourceActorId { get; init; } = string.Empty;
    public string SourceFactionId { get; init; } = string.Empty;
    public string TargetAudienceId { get; init; } = string.Empty;
    public string SourceEventId { get; init; } = string.Empty;
    public string BeforeState { get; init; } = string.Empty;
    public string AfterState { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record ProgrammaticNarrativeStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record ProgrammaticNarrativeSaveLoadReplayRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public bool BeforeAfterStateChanged { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string SerializedAfterStateHash { get; init; } = string.Empty;
    public string RestoredAfterStateHash { get; init; } = string.Empty;
    public string FirstReplayHash { get; init; } = string.Empty;
    public string SecondReplayHash { get; init; } = string.Empty;
}

public sealed record ProgrammaticNarrativeRow
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_row_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string SourceInterlockedGameplayRowRef { get; init; } = string.Empty;
    public string SourceSettlementRowRef { get; init; } = string.Empty;
    public string QuestArcId { get; init; } = string.Empty;
    public string DialogueGraphId { get; init; } = string.Empty;
    public string EventChainId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string SpatialRowHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public IReadOnlyList<QuestStageRecord> QuestStageGraph { get; init; } = [];
    public IReadOnlyList<DialogueOptionRecord> DialogueOptionGraph { get; init; } = [];
    public IReadOnlyList<EventTriggerConsequenceRecord> EventTriggerConsequenceChain { get; init; } = [];
    public IReadOnlyList<LocalizationKeyRecord> LocalizationKeyTable { get; init; } = [];
    public IReadOnlyList<MemoryRumorPropagationRecord> MemoryRumorPropagation { get; init; } = [];
    public IReadOnlyList<NarrativeStateDelta> StateDeltas { get; init; } = [];
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public ProgrammaticNarrativeStateSnapshot BeforeState { get; init; } = new();
    public ProgrammaticNarrativeStateSnapshot AfterState { get; init; } = new();
    public ProgrammaticNarrativeSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public bool StateChanging { get; init; }
    public bool NoFinalProse { get; init; } = true;
    public string RowHash { get; init; } = string.Empty;
}

public sealed record ProgrammaticNarrativeRowMatrix
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_row_matrix_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<ProgrammaticNarrativeRow> Rows { get; init; } = [];
}

public sealed record NarrativeLedgerEntry
{
    public string EntryId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string LedgerKind { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record NarrativeLedger
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_ledger_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public string LedgerKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<NarrativeLedgerEntry> Entries { get; init; } = [];
}

public sealed record LocalizationKeyTable
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_localization_key_table_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<LocalizationKeyRecord> Entries { get; init; } = [];
}

public sealed record ProgrammaticNarrativeSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_save_load_replay_proof_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<ProgrammaticNarrativeSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record ProgrammaticNarrativePreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string QuestArcId { get; init; } = string.Empty;
    public string DialogueGraphId { get; init; } = string.Empty;
    public string EventChainId { get; init; } = string.Empty;
    public string PackageRef { get; init; } = string.Empty;
    public string SpatialRef { get; init; } = string.Empty;
    public string LivingWorldRef { get; init; } = string.Empty;
    public string InterlockedRef { get; init; } = string.Empty;
    public string SettlementRef { get; init; } = string.Empty;
    public string NarrativeAfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record ProgrammaticNarrativePreviewExportPayload
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_preview_export_payload_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<ProgrammaticNarrativePreviewExportRow> Rows { get; init; } = [];
}

public sealed record ProgrammaticNarrativeUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string QuestStageId { get; init; } = string.Empty;
    public string DialogueOptionId { get; init; } = string.Empty;
    public string EventTriggerId { get; init; } = string.Empty;
    public string EventConsequenceId { get; init; } = string.Empty;
    public string MemoryRumorRecordId { get; init; } = string.Empty;
    public string LocalizationLineKey { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record ProgrammaticNarrativeUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_unity_command_plan_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = ProgrammaticNarrativeVocabulary.FinalGate;
    public IReadOnlyList<ProgrammaticNarrativeUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record ProgrammaticNarrativeUnityProofSummary
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_unity_proof_summary_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool PlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public string UnityBuildLogRelativePath { get; init; } = string.Empty;
    public string LaunchLogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    public int ProvenRowCount { get; init; }
    public IReadOnlyList<string> MatchedMarkers { get; init; } = [];
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ProgrammaticNarrativeUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public ProgrammaticNarrativeUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidProgrammaticNarrativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidProgrammaticNarrativeDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_invalid_diagnostics_matrix_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidProgrammaticNarrativeScenario> Scenarios { get; init; } = [];
}

public sealed record ProgrammaticNarrativeReport
{
    public string SchemaVersion { get; init; } = "programmatic_narrative_quest_dialogue_event_matrix_report_v1";
    public string GoalId { get; init; } = ProgrammaticNarrativeVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ProgrammaticNarrativeVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = ProgrammaticNarrativeVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal066AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool TemplateCatalogPassed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool QuestStageLedgerPassed { get; init; }
    public bool DialogueOptionLedgerPassed { get; init; }
    public bool EventConsequenceLedgerPassed { get; init; }
    public bool LocalizationKeyTablePassed { get; init; }
    public bool MemoryRumorLedgerPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllNarrativeMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoFinalProseLeakage { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string TemplateCatalogHash { get; init; } = string.Empty;
    public string RowMatrixHash { get; init; } = string.Empty;
    public string QuestStageLedgerHash { get; init; } = string.Empty;
    public string DialogueOptionLedgerHash { get; init; } = string.Empty;
    public string EventConsequenceLedgerHash { get; init; } = string.Empty;
    public string LocalizationKeyTableHash { get; init; } = string.Empty;
    public string MemoryRumorLedgerHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<ProgrammaticNarrativeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ProgrammaticNarrativeBuildResult
{
    public ProgrammaticNarrativeSourceManifest SourceManifest { get; init; } = new();
    public ProgrammaticNarrativeTemplateCatalog TemplateCatalog { get; init; } = new();
    public ProgrammaticNarrativeRowMatrix RowMatrix { get; init; } = new();
    public NarrativeLedger QuestStageLedger { get; init; } = new();
    public NarrativeLedger DialogueOptionLedger { get; init; } = new();
    public NarrativeLedger EventConsequenceLedger { get; init; } = new();
    public LocalizationKeyTable LocalizationKeyTable { get; init; } = new();
    public NarrativeLedger MemoryRumorLedger { get; init; } = new();
    public ProgrammaticNarrativeSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public ProgrammaticNarrativeUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public ProgrammaticNarrativeUnityProofSummary UnityProofSummary { get; init; } = new();
    public ProgrammaticNarrativePreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidProgrammaticNarrativeDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public ProgrammaticNarrativeReport Report { get; init; } = new();
    public IReadOnlyList<ProgrammaticNarrativeRow> Rows { get; init; } = [];
    public IReadOnlyList<ProgrammaticNarrativeFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record ProgrammaticNarrativeWriteResult
{
    public ProgrammaticNarrativeBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
