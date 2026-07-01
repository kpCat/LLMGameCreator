namespace LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;

public static class CombatMagicAbilityBossEncounterVocabulary
{
    public const string GoalId = "goal_068_combat_magic_ability_boss_encounter_matrix";
    public const string ProductSmokeRoute = "goal-068-combat-magic-ability-boss-encounter-matrix";
    public const string FinalGate = "combat_magic_ability_boss_encounter_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal065RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string Goal066RelativeOutputDirectory = ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    public const string Goal067RelativeOutputDirectory = ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    public const string StagingRoot = "staging";
    public const string UnityCombatMagicCommandPlanStagingRelativePath = "combat-magic/unity-combat-magic-command-plan.json";

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
        "missing_goal067_source",
        "fake_family_seed",
        "duplicate_row_id",
        "missing_active_ability",
        "missing_state_delta",
        "fake_ability_id",
        "illegal_status_effect_shape",
        "cooldown_cost_underflow",
        "nondeterministic_ordering",
        "save_load_mismatch",
        "replay_mismatch",
        "final_prose_leakage",
        "llm_provider_rag_claim",
        "arbitrary_lua_or_generated_lua_claim",
        "runtime_ui_unity_broad_mutation_claim",
        "public_gamepackage_schema_mutation_claim",
        "unsafe_path",
        "missing_unity_marker_proof",
        "boss_phase_without_transition",
        "impossible_overpowered_encounter"
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

public sealed record CombatMagicAbilityBossEncounterOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record CombatMagicDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static CombatMagicDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static CombatMagicDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static CombatMagicDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record CombatMagicFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record CombatMagicSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CombatMagicSourceRow
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
    public string SourceNarrativeRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string GameplayAfterStateHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public string NarrativeAfterStateHash { get; init; } = string.Empty;
    public string QuestArcId { get; init; } = string.Empty;
    public string DialogueGraphId { get; init; } = string.Empty;
    public string EventChainId { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public bool Goal060PackageValid { get; init; }
    public bool Goal061ReviewPackageRcExists { get; init; }
    public bool Goal062SpatialRowValid { get; init; }
    public bool Goal063GameplayRowValid { get; init; }
    public bool Goal064LivingWorldRowValid { get; init; }
    public bool Goal065InterlockedRowValid { get; init; }
    public bool Goal066SettlementRowValid { get; init; }
    public bool Goal067NarrativeRowValid { get; init; }
    public bool Goal067SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> GameplayDeltaIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldActorIds { get; init; } = [];
    public IReadOnlyList<string> LivingWorldFactionIds { get; init; } = [];
    public IReadOnlyList<string> InterlockedDeltaIds { get; init; } = [];
    public IReadOnlyList<string> InterlockedCombatProgressionLedgerEntryIds { get; init; } = [];
    public IReadOnlyList<string> InterlockedStatusLedgerEntryIds { get; init; } = [];
    public IReadOnlyList<string> SettlementLedgerEntryIds { get; init; } = [];
    public IReadOnlyList<string> NarrativeDeltaIds { get; init; } = [];
}

public sealed record CombatMagicSourceBundle
{
    public bool Goal067AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal067UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<CombatMagicSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<CombatMagicFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<CombatMagicSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CombatMagicGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record CombatMagicSourceManifest
{
    public string SchemaVersion { get; init; } = "combat_magic_source_manifest_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = CombatMagicAbilityBossEncounterVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = CombatMagicAbilityBossEncounterVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal067AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal067UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<CombatMagicGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<CombatMagicSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ActiveAbilityDefinition
{
    public string AbilityId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string AbilityKind { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public int BaseCost { get; init; }
    public int BaseCooldown { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record PassiveTraitDefinition
{
    public string TraitId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string TraitKind { get; init; } = string.Empty;
    public IReadOnlyList<string> ResistanceRefs { get; init; } = [];
    public IReadOnlyList<string> WeaknessRefs { get; init; } = [];
}

public sealed record CombatMagicAbilityTraitCatalog
{
    public string SchemaVersion { get; init; } = "combat_magic_ability_trait_catalog_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ActiveAbilityCount { get; init; }
    public int PassiveTraitCount { get; init; }
    public IReadOnlyList<ActiveAbilityDefinition> ActiveAbilities { get; init; } = [];
    public IReadOnlyList<PassiveTraitDefinition> PassiveTraits { get; init; } = [];
}

public sealed record StatusEffectDefinition
{
    public string StatusEffectId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string EffectKind { get; init; } = string.Empty;
    public string StackPolicy { get; init; } = string.Empty;
    public int MaxStacks { get; init; }
    public IReadOnlyList<string> DeltaCategories { get; init; } = [];
}

public sealed record CombatMagicStatusEffectCatalog
{
    public string SchemaVersion { get; init; } = "combat_magic_status_effect_catalog_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int StatusEffectCount { get; init; }
    public IReadOnlyList<StatusEffectDefinition> StatusEffects { get; init; } = [];
}

public sealed record BossEncounterPhaseDefinition
{
    public string PhaseId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PhaseKind { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public IReadOnlyList<string> TransitionMarkers { get; init; } = [];
}

public sealed record CombatMagicBossEncounterPhaseCatalog
{
    public string SchemaVersion { get; init; } = "combat_magic_boss_encounter_phase_catalog_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PhaseCount { get; init; }
    public IReadOnlyList<BossEncounterPhaseDefinition> Phases { get; init; } = [];
}

public sealed record AttributeResourceSnapshot
{
    public int Health { get; init; }
    public int Armor { get; init; }
    public int Mana { get; init; }
    public int Energy { get; init; }
    public int Stamina { get; init; }
    public int Threat { get; init; }
}

public sealed record CombatantSnapshot
{
    public string CombatantId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public AttributeResourceSnapshot Attributes { get; init; } = new();
    public IReadOnlyList<string> ActiveAbilityIds { get; init; } = [];
    public IReadOnlyList<string> PassiveTraitIds { get; init; } = [];
    public IReadOnlyList<string> StatusEffectIds { get; init; } = [];
}

public sealed record ActiveAbilityUse
{
    public string AbilityUseId { get; init; } = string.Empty;
    public string AbilityId { get; init; } = string.Empty;
    public string CasterCombatantId { get; init; } = string.Empty;
    public string TargetCombatantId { get; init; } = string.Empty;
    public string AbilityKind { get; init; } = string.Empty;
    public string Resolution { get; init; } = string.Empty;
}

public sealed record PassiveTraitUse
{
    public string TraitUseId { get; init; } = string.Empty;
    public string TraitId { get; init; } = string.Empty;
    public string CombatantId { get; init; } = string.Empty;
    public string TriggeredBy { get; init; } = string.Empty;
}

public sealed record StatusEffectApplication
{
    public string StatusApplicationId { get; init; } = string.Empty;
    public string StatusEffectId { get; init; } = string.Empty;
    public string TargetCombatantId { get; init; } = string.Empty;
    public int BeforeStacks { get; init; }
    public int AfterStacks { get; init; }
    public string DurationChange { get; init; } = string.Empty;
}

public sealed record DamageEffectPacket
{
    public string PacketId { get; init; } = string.Empty;
    public string DamageKind { get; init; } = string.Empty;
    public int AmountBeforeMitigation { get; init; }
    public int AmountAfterMitigation { get; init; }
    public IReadOnlyList<string> AppliedEffectIds { get; init; } = [];
}

public sealed record CooldownCostRecord
{
    public string CooldownCostId { get; init; } = string.Empty;
    public string AbilityId { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public int CostPaid { get; init; }
    public int ResourceBefore { get; init; }
    public int ResourceAfter { get; init; }
    public int CooldownBefore { get; init; }
    public int CooldownAfter { get; init; }
}

public sealed record ResistanceWeaknessRecord
{
    public string ResistanceWeaknessId { get; init; } = string.Empty;
    public string CombatantId { get; init; } = string.Empty;
    public string ResistanceKind { get; init; } = string.Empty;
    public string WeaknessKind { get; init; } = string.Empty;
    public int MitigationAmount { get; init; }
}

public sealed record BossPhaseRecord
{
    public string PhaseId { get; init; } = string.Empty;
    public string PhaseKind { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public string BeforePhaseState { get; init; } = string.Empty;
    public string AfterPhaseState { get; init; } = string.Empty;
    public bool TransitionApplied { get; init; }
}

public sealed record RoundPhaseResult
{
    public string RoundId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string PhaseRef { get; init; } = string.Empty;
    public IReadOnlyList<ActiveAbilityUse> ActiveAbilities { get; init; } = [];
    public IReadOnlyList<DamageEffectPacket> DamageEffectPackets { get; init; } = [];
    public IReadOnlyList<StatusEffectApplication> StatusApplications { get; init; } = [];
    public IReadOnlyList<CooldownCostRecord> CooldownCosts { get; init; } = [];
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record CounterplayRecord
{
    public string CounterplayId { get; init; } = string.Empty;
    public string CounterplayKind { get; init; } = string.Empty;
    public string PlayerOption { get; init; } = string.Empty;
    public string MitigatedPacketId { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record LootProgressionRecord
{
    public string LootProgressionId { get; init; } = string.Empty;
    public string LootId { get; init; } = string.Empty;
    public string ProgressionId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record NonCombatConsequenceRecord
{
    public string ConsequenceId { get; init; } = string.Empty;
    public string ConsequenceKind { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record CombatMagicStateDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record CombatMagicStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record CombatMagicSaveLoadReplayRecord
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

public sealed record CombatMagicUnityMarkerCommandRecord
{
    public string CommandId { get; init; } = string.Empty;
    public string MarkerKind { get; init; } = string.Empty;
    public string MarkerValue { get; init; } = string.Empty;
    public int Order { get; init; }
}

public sealed record CombatMagicRow
{
    public string SchemaVersion { get; init; } = "combat_magic_ability_boss_encounter_row_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
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
    public string SourceNarrativeRowRef { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string EncounterKind { get; init; } = string.Empty;
    public IReadOnlyList<CombatantSnapshot> InitialCombatants { get; init; } = [];
    public IReadOnlyList<CombatantSnapshot> FinalCombatants { get; init; } = [];
    public IReadOnlyList<ActiveAbilityUse> ActiveAbilities { get; init; } = [];
    public IReadOnlyList<PassiveTraitUse> PassiveTraits { get; init; } = [];
    public IReadOnlyList<StatusEffectApplication> StatusEffects { get; init; } = [];
    public IReadOnlyList<DamageEffectPacket> DamageEffectPackets { get; init; } = [];
    public IReadOnlyList<CooldownCostRecord> CooldownCosts { get; init; } = [];
    public IReadOnlyList<ResistanceWeaknessRecord> ResistanceWeaknesses { get; init; } = [];
    public IReadOnlyList<BossPhaseRecord> BossPhases { get; init; } = [];
    public IReadOnlyList<RoundPhaseResult> RoundPhaseResults { get; init; } = [];
    public IReadOnlyList<CounterplayRecord> CounterplayRecords { get; init; } = [];
    public IReadOnlyList<LootProgressionRecord> LootProgressionRecords { get; init; } = [];
    public IReadOnlyList<NonCombatConsequenceRecord> NonCombatConsequences { get; init; } = [];
    public IReadOnlyList<CombatMagicStateDelta> StateDeltas { get; init; } = [];
    public IReadOnlyList<string> ChangedCategories { get; init; } = [];
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public CombatMagicStateSnapshot BeforeState { get; init; } = new();
    public CombatMagicStateSnapshot AfterState { get; init; } = new();
    public CombatMagicSaveLoadReplayRecord SaveLoadReplayProof { get; init; } = new();
    public bool StateChanging { get; init; }
    public bool BossOrElitePhaseRow { get; init; }
    public bool MagicStatusHeavyRow { get; init; }
    public bool ResourceGearCraftingLinkedRow { get; init; }
    public bool NoFinalProse { get; init; } = true;
    public string RowHash { get; init; } = string.Empty;
}

public sealed record CombatMagicRowMatrix
{
    public string SchemaVersion { get; init; } = "combat_magic_row_matrix_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int BossEliteRowCount { get; init; }
    public int MagicStatusRowCount { get; init; }
    public int ResourceGearCraftingRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public bool SameFamilySeedVariancePassed { get; init; }
    public bool FamilyCombatFlavorVariancePassed { get; init; }
    public IReadOnlyList<CombatMagicRow> Rows { get; init; } = [];
}

public sealed record CombatMagicSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = "combat_magic_save_load_replay_proof_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<CombatMagicSaveLoadReplayRecord> Rows { get; init; } = [];
}

public sealed record CombatMagicLedgerEntry
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

public sealed record CombatMagicLedger
{
    public string SchemaVersion { get; init; } = "combat_magic_ledger_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public string LedgerKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<CombatMagicLedgerEntry> Entries { get; init; } = [];
}

public sealed record CombatMagicPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string PackageRef { get; init; } = string.Empty;
    public string SpatialRef { get; init; } = string.Empty;
    public string NarrativeRef { get; init; } = string.Empty;
    public string CombatMagicAfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record CombatMagicPreviewExportPayload
{
    public string SchemaVersion { get; init; } = "combat_magic_preview_export_payload_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<CombatMagicPreviewExportRow> Rows { get; init; } = [];
}

public sealed record CombatMagicUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string AbilityUseId { get; init; } = string.Empty;
    public string StatusApplicationId { get; init; } = string.Empty;
    public string ProgressionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RoundStepIds { get; init; } = [];
    public IReadOnlyList<CombatMagicUnityMarkerCommandRecord> MarkerCommands { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record CombatMagicUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "combat_magic_unity_command_plan_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = CombatMagicAbilityBossEncounterVocabulary.FinalGate;
    public IReadOnlyList<CombatMagicUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record CombatMagicUnityProofSummary
{
    public string SchemaVersion { get; init; } = "combat_magic_unity_proof_summary_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
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
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CombatMagicUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public CombatMagicUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidCombatMagicScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidCombatMagicDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "combat_magic_invalid_diagnostics_matrix_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidCombatMagicScenario> Scenarios { get; init; } = [];
}

public sealed record CombatMagicReport
{
    public string SchemaVersion { get; init; } = "combat_magic_ability_boss_encounter_matrix_report_v1";
    public string GoalId { get; init; } = CombatMagicAbilityBossEncounterVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = CombatMagicAbilityBossEncounterVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = CombatMagicAbilityBossEncounterVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal067AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool AbilityTraitCatalogPassed { get; init; }
    public bool StatusEffectCatalogPassed { get; init; }
    public bool BossPhaseCatalogPassed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool ProgressionLootLedgerPassed { get; init; }
    public bool CounterplayLedgerPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllCombatMagicMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoFinalProseLeakage { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int BossEliteRowCount { get; init; }
    public int MagicStatusRowCount { get; init; }
    public int ResourceGearCraftingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string AbilityTraitCatalogHash { get; init; } = string.Empty;
    public string StatusEffectCatalogHash { get; init; } = string.Empty;
    public string BossPhaseCatalogHash { get; init; } = string.Empty;
    public string RowMatrixHash { get; init; } = string.Empty;
    public string ProgressionLootLedgerHash { get; init; } = string.Empty;
    public string CounterplayLedgerHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<CombatMagicDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CombatMagicBuildResult
{
    public CombatMagicSourceManifest SourceManifest { get; init; } = new();
    public CombatMagicAbilityTraitCatalog AbilityTraitCatalog { get; init; } = new();
    public CombatMagicStatusEffectCatalog StatusEffectCatalog { get; init; } = new();
    public CombatMagicBossEncounterPhaseCatalog BossPhaseCatalog { get; init; } = new();
    public CombatMagicRowMatrix RowMatrix { get; init; } = new();
    public CombatMagicSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public CombatMagicLedger ProgressionLootLedger { get; init; } = new();
    public CombatMagicLedger CounterplayLedger { get; init; } = new();
    public CombatMagicPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public CombatMagicUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public CombatMagicUnityProofSummary UnityProofSummary { get; init; } = new();
    public InvalidCombatMagicDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public CombatMagicReport Report { get; init; } = new();
    public IReadOnlyList<CombatMagicRow> Rows { get; init; } = [];
    public IReadOnlyList<CombatMagicFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record CombatMagicWriteResult
{
    public CombatMagicBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
