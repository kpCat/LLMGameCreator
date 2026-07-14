using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public static class UnifiedGameProjectWorkspaceVocabulary
{
    public const string LegacyCompositionId = "goal147-custom-alchemy-combat-exploration";
    public const string AuthoringRelativeRoot = ".llmgc/authoring";
    public const string CertificationCacheRelativeRoot = ".llmgc/certification-cache";
    public const string BuildStagingRelativeRoot = ".llmgc/build-staging";
    public const string BuildHistoryRelativeRoot = ".llmgc/build-history";
    public const string ReleaseCandidateRelativeRoot = ".llmgc/release-candidate";
    public const string ReleaseCandidateRecordRelativePath = ".llmgc/release-candidate/accepted-mechanics-rc1.json";
    public const string ReleaseCandidateSchemaVersion = "accepted_mechanics_release_candidate_v1";
    public const string PrimaryActionText = "Собрать и проверить игру";
}

public sealed record GameProjectMechanicPresentation
{
    public string ModuleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public bool Required { get; init; }
    public bool Selected { get; init; }
    public IReadOnlyList<string> DependencyTitles { get; init; } = [];
    public IReadOnlyList<string> ConflictTitles { get; init; } = [];
}

public sealed record GameProjectParameterPresentation
{
    public string ModuleId { get; init; } = string.Empty;
    public string ModuleTitle { get; init; } = string.Empty;
    public string ParameterId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public JsonElement Value { get; init; }
    public decimal? Minimum { get; init; }
    public decimal? Maximum { get; init; }
    public decimal? Step { get; init; }
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
    public string Unit { get; init; } = string.Empty;
    public string ValidationError { get; init; } = string.Empty;
}

public sealed record GameProjectSocialHumanFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GameProjectSocialSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string FactionId { get; init; } = string.Empty;
    public string FactionTitle { get; init; } = string.Empty;
    public decimal ReputationBefore { get; init; }
    public decimal ReputationAfter { get; init; }
    public string QuestId { get; init; } = string.Empty;
    public string QuestTitle { get; init; } = string.Empty;
    public string QuestState { get; init; } = string.Empty;
    public string ChoiceId { get; init; } = string.Empty;
    public string ChoiceText { get; init; } = string.Empty;
    public IReadOnlyList<string> ChoiceVisibilitySequence { get; init; } = [];
    public decimal GoldBefore { get; init; }
    public decimal GoldAfterQuest { get; init; }
    public decimal GoldAfterClaim { get; init; }
    public decimal TrustedRewardDelta { get; init; }
    public string ClaimFlagId { get; init; } = string.Empty;
    public bool RewardClaimed { get; init; }
    public bool RepeatRewardAvailable { get; init; }
    public string SocialOutcome { get; init; } = string.Empty;
    public bool CheckpointReplayPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public IReadOnlyList<GameProjectSocialHumanFact> HumanFacts { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GameProjectAcceptedMechanicsSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public int SelectedMechanicCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public decimal EquipmentDamageBonus { get; init; }
    public decimal StatDamageBonus { get; init; }
    public decimal TotalAdditionalDamage { get; init; }
    public decimal AbilityDirectDamage { get; init; }
    public decimal ManaBefore { get; init; }
    public decimal ManaSpent { get; init; }
    public decimal ManaRemaining { get; init; }
    public decimal StatusTickDamage { get; init; }
    public bool StatusExpired { get; init; }
    public GameProjectSocialSummary? Social { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public IReadOnlyList<GameProjectSocialHumanFact> HumanFacts { get; init; } = [];
    public IReadOnlyList<string> MissingFactKinds { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record UnifiedGameProjectWorkspaceSnapshot
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ProjectTitle { get; init; } = string.Empty;
    public string ProjectPackageId { get; init; } = string.Empty;
    public string ProjectVersion { get; init; } = string.Empty;
    public string ProjectFormatVersion { get; init; } = string.Empty;
    public string ProjectDescription { get; init; } = string.Empty;
    public string ProjectScopedCompositionId { get; init; } = string.Empty;
    public string IdentitySource { get; init; } = string.Empty;
    public IReadOnlyList<string> IdentityRecoveryDiagnostics { get; init; } = [];
    public string PackageStatus { get; init; } = "Проверка ещё не запускалась";
    public string AuthoringStatus { get; init; } = "Готово";
    public int SelectedMechanicCount { get; init; }
    public string LastSuccessfulBuild { get; init; } = "Проверка ещё не запускалась";
    public string LastRuntimeQualification { get; init; } = "Проверка ещё не запускалась";
    public bool Dirty { get; init; }
    public int Revision { get; init; }
    public string CatalogFingerprint { get; init; } = string.Empty;
    public IReadOnlyList<GameProjectMechanicPresentation> Mechanics { get; init; } = [];
    public IReadOnlyList<GameProjectParameterPresentation> Parameters { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string ActivatedProjectPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int LastCertificationExecutedCount { get; init; }
    public int LastCertificationReusedCount { get; init; }
    public string RuntimePlaythroughPlanId { get; init; } = string.Empty;
    public int CapabilityCount { get; init; }
    public int PlannedActionCount { get; init; }
    public int CheckpointActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public string PlaythroughSignature { get; init; } = string.Empty;
    public string EquipmentSlotSummary { get; init; } = string.Empty;
    public string AttributesSummary { get; init; } = string.Empty;
    public string ProgressionSummary { get; init; } = string.Empty;
    public decimal StatDamageBonus { get; init; }
    public decimal EquipmentDamageBonus { get; init; }
    public decimal TotalAdditionalDamage { get; init; }
    public string AbilitySummary { get; init; } = string.Empty;
    public string ManaSummary { get; init; } = string.Empty;
    public string StatusSummary { get; init; } = string.Empty;
    public decimal AbilityDirectDamage { get; init; }
    public decimal ManaBefore { get; init; }
    public decimal ManaSpent { get; init; }
    public decimal ManaRemaining { get; init; }
    public decimal StatusTickDamage { get; init; }
    public int StatusRemainingTicks { get; init; }
    public bool StatusExpired { get; init; }
    public string LastBuildAttemptId { get; init; } = string.Empty;
    public string LastBuildAttemptStatus { get; init; } = "NOT_RUN";
    public string LastBuildFailureStage { get; init; } = string.Empty;
    public IReadOnlyList<string> LastBuildAttemptedSelectedModuleIds { get; init; } = [];
    public int LastBuildAttemptedConfiguredParameterCount { get; init; }
    public int LastBuildAttemptedCapabilityCount { get; init; }
    public int LastBuildAttemptedPlannedActionCount { get; init; }
    public int LastBuildAttemptedCheckpointActionCount { get; init; }
    public int LastBuildAttemptedFinalReplayActionCount { get; init; }
    public string LastBuildAttemptedCompositionPackageSha256 { get; init; } = string.Empty;
    public string LastBuildAttemptedFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> LastBuildAttemptDiagnostics { get; init; } = [];
    public string ExecutablePath { get; init; } = string.Empty;
    public string ExecutableSha256 { get; init; } = string.Empty;
    public string ExecutableFileVersion { get; init; } = string.Empty;
    public string ExecutableInformationalVersion { get; init; } = string.Empty;
    public ProjectStandaloneBuildResult? LastStandaloneBuild { get; init; }
    public string StandaloneUnityEditorPath { get; init; } = string.Empty;
    public GameProjectSocialSummary? Social { get; init; }
    public bool SocialMatchesCurrentConfiguration { get; init; }
    public string SocialConfigurationStatus { get; init; } = "ABSENT";
    public GameProjectAcceptedMechanicsSummary? AcceptedMechanics { get; init; }
    public GameProjectReleaseCandidateRecord? ReleaseCandidate { get; init; }
    public string ReleaseCandidateRecordConfigurationStatus { get; init; } = "ABSENT";
    public string ReleaseCandidateConfigurationStatus { get; init; } = "ABSENT";
    public string ReleaseCandidateRecordPath { get; init; } = string.Empty;
}

public sealed record GameProjectBuildResult
{
    public string Status { get; init; } = "FAILED";
    public bool Passed { get; init; }
    public string HumanSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public int SelectedMechanicCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string ActivatedProjectPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public bool PackageActivated { get; init; }
    public bool PackageActivationTransactional { get; init; }
    public bool RollbackApplied { get; init; }
    public int CertificationExecutedCount { get; init; }
    public int CertificationReusedCount { get; init; }
    public string BuildHistoryPath { get; init; } = string.Empty;
    public int RequiredSupportFileCount { get; init; }
    public int CopiedSupportFileCount { get; init; }
    public int ReusedSupportFileCount { get; init; }
    public bool SupportFilesPrepared { get; init; }
    public IReadOnlyList<string> SupportFileDiagnostics { get; init; } = [];
    public bool StagedProjectValidationPassed { get; init; }
    public bool RealProjectValidationPassed { get; init; }
    public string RuntimePlaythroughPlanId { get; init; } = string.Empty;
    public int CapabilityCount { get; init; }
    public int PlannedActionCount { get; init; }
    public int CheckpointActionCount { get; init; }
    public int FinalReplayActionCount { get; init; }
    public string PlaythroughSignature { get; init; } = string.Empty;
    public string EquipmentSlotSummary { get; init; } = string.Empty;
    public int WeaponDamageBonus { get; init; }
    public int CombatDamageDelta { get; init; }
    public string AttributesSummary { get; init; } = string.Empty;
    public string ProgressionSummary { get; init; } = string.Empty;
    public decimal StatDamageBonus { get; init; }
    public decimal TotalAdditionalDamage { get; init; }
    public string AbilitySummary { get; init; } = string.Empty;
    public string ManaSummary { get; init; } = string.Empty;
    public string StatusSummary { get; init; } = string.Empty;
    public decimal AbilityDirectDamage { get; init; }
    public decimal ManaBefore { get; init; }
    public decimal ManaSpent { get; init; }
    public decimal ManaRemaining { get; init; }
    public decimal StatusTickDamage { get; init; }
    public int StatusRemainingTicks { get; init; }
    public bool StatusExpired { get; init; }
    public string AttemptId { get; init; } = string.Empty;
    public string AttemptStatus { get; init; } = "NOT_RUN";
    public string FailureStage { get; init; } = string.Empty;
    public IReadOnlyList<string> AttemptedSelectedModuleIds { get; init; } = [];
    public int AttemptedConfiguredParameterCount { get; init; }
    public int AttemptedCapabilityCount { get; init; }
    public int AttemptedPlannedActionCount { get; init; }
    public int AttemptedCheckpointActionCount { get; init; }
    public int AttemptedFinalReplayActionCount { get; init; }
    public string AttemptedCompositionPackageSha256 { get; init; } = string.Empty;
    public string AttemptedFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GameProjectRuntimeFrame> RuntimeFrames { get; init; } = [];
    public GameProjectSocialSummary? Social { get; init; }
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public GameProjectAcceptedMechanicsSummary? AcceptedMechanics { get; init; }
}

public sealed record GameProjectRuntimeFrame
{
    public int Index { get; init; }
    public string ActionId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string StateHash { get; init; } = string.Empty;
}

public sealed record GameProjectBuildHistoryEntry
{
    public string SchemaVersion { get; init; } = "unified_game_project_build_history_v2";
    public DateTimeOffset CompletedAtUtc { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string ActivatedProjectPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int SelectedMechanicCount { get; init; }
    public int ConfiguredParameterCount { get; init; }
    public int CertificationExecutedCount { get; init; }
    public int CertificationReusedCount { get; init; }
    public bool CheckpointReloadPassed { get; init; }
    public bool FullReplayEquivalent { get; init; }
    public bool ActionBindingPassed { get; init; }
    public string AttemptId { get; init; } = string.Empty;
    public string AttemptStatus { get; init; } = string.Empty;
    public string FailureStage { get; init; } = string.Empty;
    public IReadOnlyList<string> AttemptedSelectedModuleIds { get; init; } = [];
    public int AttemptedCapabilityCount { get; init; }
    public int AttemptedPlannedActionCount { get; init; }
    public int AttemptedCheckpointActionCount { get; init; }
    public int AttemptedFinalReplayActionCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public GameProjectSocialSummary? Social { get; init; }
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public GameProjectAcceptedMechanicsSummary? AcceptedMechanics { get; init; }
}

public sealed record GameProjectAuthoringState
{
    public string ProjectFolder { get; init; } = string.Empty;
    public FeatureModuleLibrarySnapshot Library { get; init; } = new();
    public FeatureModuleCompositionDocument Document { get; init; } = new();
    public GameProjectIdentityDocument Identity { get; init; } = new();
    public bool Dirty { get; init; }
    public int DirtyTransitionCount { get; init; }
}
