using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

/// <summary>Reads validated persisted last-success summaries without mutating project history.</summary>
public sealed class GameProjectBuildHistoryReader
{
    public const string SchemaVersionV2 = "unified_game_project_build_history_v2";
    public const string SchemaVersionV3 = "unified_game_project_build_history_v3";
    public const string SchemaVersionV4 = "unified_game_project_build_history_v4";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GameProjectBuildHistoryReadResult ReadLatestMatchingSocialSuccess(
        string projectFolder,
        FeatureModuleCompositionDocument document)
        => ReadLatestMatchingSocialSuccess(projectFolder, document, null);

    public GameProjectBuildHistoryReadResult ReadLatestMatchingSocialSuccess(
        string projectFolder,
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot? library)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        ArgumentNullException.ThrowIfNull(document);
        var historyRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        if (!Directory.Exists(historyRoot)) return new GameProjectBuildHistoryReadResult();

        var diagnostics = new List<string>();
        var candidates = new List<(GameProjectBuildHistoryEntry Entry, string FileName)>();
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json", SearchOption.TopDirectoryOnly))
        {
            GameProjectBuildHistoryEntry? historyEntry;
            try { historyEntry = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(File.ReadAllText(path), JsonOptions); }
            catch (JsonException) { diagnostics.Add("social.history.invalid_json:" + Path.GetFileName(path)); continue; }
            catch (IOException) { diagnostics.Add("social.history.unreadable:" + Path.GetFileName(path)); continue; }

            if (historyEntry is null
                || historyEntry.SchemaVersion is not SchemaVersionV2
                    and not SchemaVersionV3 and not SchemaVersionV4)
            {
                diagnostics.Add("social.history.unsupported_schema:" + Path.GetFileName(path));
                continue;
            }
            if (IsMatchingGreenSuccess(historyEntry, document)) candidates.Add((historyEntry, Path.GetFileName(path)));
        }

        var selected = candidates.OrderByDescending(candidate => candidate.Entry.CompletedAtUtc)
            .ThenByDescending(candidate => candidate.FileName, StringComparer.Ordinal).FirstOrDefault();
        if (selected.Entry is null)
        {
            if (HasPersistedSuccessIdentity(document)) diagnostics.Add("social.history.no_matching_green_social_success");
            return new GameProjectBuildHistoryReadResult { Diagnostics = diagnostics };
        }

        var entry = selected.Entry;
        var fingerprint = library is null ? new FeatureModuleAuthoringFingerprintResult()
            : new FeatureModuleAuthoringFingerprintService().Calculate(document, library);
        var status = ResolveConfigurationStatus(entry.QualifiedAuthoringFingerprint, fingerprint);
        return new GameProjectBuildHistoryReadResult
        {
            LastSuccessfulBuild = new GameProjectBuildResult
            {
                Status = entry.Status, Passed = true, Diagnostics = entry.Diagnostics,
                SelectedMechanicCount = entry.SelectedMechanicCount, ConfiguredParameterCount = entry.ConfiguredParameterCount,
                PackageSha256 = entry.PackageSha256, CompositionPackageSha256 = entry.CompositionPackageSha256,
                ActivatedProjectPackageSha256 = entry.ActivatedProjectPackageSha256, FinalStateHash = entry.FinalStateHash,
                CheckpointReloadPassed = entry.CheckpointReloadPassed, FullReplayEquivalent = entry.FullReplayEquivalent,
                ActionBindingPassed = entry.ActionBindingPassed, AttemptId = entry.AttemptId, AttemptStatus = entry.AttemptStatus,
                AttemptedSelectedModuleIds = entry.AttemptedSelectedModuleIds, AttemptedCapabilityCount = entry.AttemptedCapabilityCount,
                AttemptedPlannedActionCount = entry.AttemptedPlannedActionCount, AttemptedCheckpointActionCount = entry.AttemptedCheckpointActionCount,
                AttemptedFinalReplayActionCount = entry.AttemptedFinalReplayActionCount, Social = entry.Social,
                QualifiedAuthoringFingerprint = entry.QualifiedAuthoringFingerprint,
                AcceptedMechanics = entry.AcceptedMechanics,
                GeneratedWorld = ProjectGeneratedWorld(entry),
                GeneratedWorldActivation = entry.GeneratedWorldActivation,
                GeneratedWorldTravelOverlay = IsGoal157(entry) ? null : entry.GeneratedWorldTravelOverlay,
                GeneratedRegionTravel = IsGoal157(entry) ? null : entry.GeneratedRegionTravel,
                GeneratedEncounterCombat = ProjectGeneratedCombat(entry),
                AcceptedMechanicsCompatibility = entry.AcceptedMechanicsCompatibility
            },
            Diagnostics = diagnostics.Concat(fingerprint.Diagnostics).ToList(),
            CurrentAuthoringFingerprint = fingerprint.Sha256,
            QualifiedAuthoringFingerprint = entry.QualifiedAuthoringFingerprint,
            SocialConfigurationStatus = status,
            MatchesCurrentConfiguration = status == "CURRENT"
        };
    }

    private static bool IsMatchingGreenSuccess(GameProjectBuildHistoryEntry entry, FeatureModuleCompositionDocument document) =>
        string.Equals(entry.Status, "GREEN", StringComparison.Ordinal)
        && string.Equals(entry.AttemptStatus, "GREEN", StringComparison.Ordinal)
        && (entry.GeneratedWorld is { Present: true }
            ? entry.GeneratedWorld is { Passed: true, PackageContentPreserved: true }
              && entry.GeneratedWorldActivation is
                  { Present: true, Passed: true, ReplayEquivalent: true, StateRoundtripPassed: true }
              && (entry.SchemaVersion == SchemaVersionV2
                  || entry.SchemaVersion == SchemaVersionV3 && TravelEligible(entry)
                  || entry.SchemaVersion == SchemaVersionV4
                  && TravelEligible(entry) && CombatEligible(entry))
            : entry.Social is { Present: true, Passed: true, CheckpointReplayPassed: true, FullReplayEquivalent: true })
        && string.Equals(entry.PackageSha256, document.LastActivatedProjectPackageSha256, StringComparison.Ordinal)
        && string.Equals(entry.CompositionPackageSha256, document.LastCompositionPackageSha256, StringComparison.Ordinal)
        && string.Equals(entry.FinalStateHash, document.LastQualifiedFinalStateHash, StringComparison.Ordinal)
        && entry.CheckpointReloadPassed && entry.FullReplayEquivalent && entry.ActionBindingPassed;

    private static bool TravelEligible(GameProjectBuildHistoryEntry entry) =>
        entry.GeneratedWorldTravelOverlay is
        {
            SchemaVersion: "generated_world_travel_overlay_v1",
            ControlledDeltaPassed: true,
            GatePlacementPassed: true,
            ConnectionCount: > 0,
            GateCount: > 0
        }
        && entry.GeneratedRegionTravel is
        {
            Present: true,
            Passed: true,
            TransitionCount: > 0,
            OriginInteractionObserved: true,
            TravelGateInteractionsPassed: true,
            DestinationInteractionObserved: true,
            ReplayEquivalent: true,
            StateRoundtripPassed: true
        } travel
        && travel.VisitedMapIds.Distinct(StringComparer.Ordinal).Count() >= 2
        && travel.VisitedRegionIds.Distinct(StringComparer.Ordinal).Count() >= 2
        && (entry.SchemaVersion == SchemaVersionV4
            || string.Equals(travel.FinalStateHash, entry.FinalStateHash, StringComparison.Ordinal));

    private static bool CombatEligible(GameProjectBuildHistoryEntry entry) =>
        entry.GeneratedEncounterCombat is
        {
            Present: true,
            Passed: true,
            Status: "CAMPAIGN_CURRENT",
            GeneratedEncounterCount: > 0,
            ExactPackageReferencePassed: true,
            PackageShaUnchangedDuringRuntime: true,
            BasicAttackPassed: true,
            PackageAbilityPassed: true,
            OpponentAiPassed: true,
            VictoryPassed: true,
            FleePassed: true,
            RewardPassed: true,
            GeneratedQuestReadyPassed: true,
            ManualTurnInPassed: true,
            CompleteQuestCommandCount: 1,
            AdvanceObjectiveCommandCount: 0,
            ConsequencePassed: true,
            ReplayPassed: true
        } combat
        && RouteEligible(combat)
        && combat.QualifiedEncounterCount == combat.GeneratedEncounterCount
        && string.Equals(combat.ExactPackageSha256, entry.PackageSha256, StringComparison.Ordinal)
        && string.Equals(combat.FinalStateHash, entry.FinalStateHash, StringComparison.Ordinal);

    private static bool RouteEligible(GameProjectGeneratedEncounterCombatSummary combat) => combat.RouteMode switch
    {
        GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY =>
            combat.PlayerRoutePassed && combat.BasicAttackRequired && !combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY =>
            combat.PlayerRoutePassed && !combat.BasicAttackRequired && combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        GeneratedEncounterCombatRouteMode.BOTH =>
            combat.PlayerRoutePassed && combat.BasicAttackRequired && combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        // Goal164 v4 rows predate route fields. Their two actual passed routes are
        // intentionally retained as current rather than rewritten as history.
        GeneratedEncounterCombatRouteMode.NONE =>
            !combat.PlayerRoutePassed && !combat.BasicAttackRequired && !combat.PackageAbilityRequired
            && combat.BasicAttackPassed && combat.PackageAbilityPassed,
        _ => false
    };

    private static GameProjectGeneratedWorldSummary? ProjectGeneratedWorld(GameProjectBuildHistoryEntry entry)
    {
        var generatedWorld = entry.GeneratedWorld;
        if (generatedWorld is null
            || entry.GeneratedWorldActivation is not { Present: true, Passed: true })
            return generatedWorld;

        if (IsGoal157(entry)) return generatedWorld with { Status = "START_CURRENT" };
        return entry.SchemaVersion == SchemaVersionV3
            ? generatedWorld with { Status = "TRAVEL_CURRENT" }
            : generatedWorld;
    }

    private static bool IsGoal157(GameProjectBuildHistoryEntry entry) =>
        string.Equals(entry.SchemaVersion, SchemaVersionV2, StringComparison.Ordinal);

    private static GameProjectGeneratedEncounterCombatSummary? ProjectGeneratedCombat(
        GameProjectBuildHistoryEntry entry)
    {
        if (string.Equals(entry.SchemaVersion, SchemaVersionV4, StringComparison.Ordinal))
            return entry.GeneratedEncounterCombat;
        if (string.Equals(entry.SchemaVersion, SchemaVersionV3, StringComparison.Ordinal)
            && entry.GeneratedWorld is { Present: true })
            return new GameProjectGeneratedEncounterCombatSummary
            {
                Present = true,
                Status = "COMBAT_PENDING",
                GeneratedEncounterCount = entry.GeneratedWorld.EncounterCount,
                Diagnostics = ["generated_combat.history_upgrade_required"]
            };
        if (string.Equals(entry.SchemaVersion, SchemaVersionV2, StringComparison.Ordinal)
            && entry.GeneratedWorld is { Present: true })
            return new GameProjectGeneratedEncounterCombatSummary { Status = "ABSENT" };
        return null;
    }

    private static bool HasPersistedSuccessIdentity(FeatureModuleCompositionDocument document) =>
        !string.IsNullOrWhiteSpace(document.LastActivatedProjectPackageSha256)
        || !string.IsNullOrWhiteSpace(document.LastCompositionPackageSha256)
        || !string.IsNullOrWhiteSpace(document.LastQualifiedFinalStateHash);

    private static string ResolveConfigurationStatus(string qualified, FeatureModuleAuthoringFingerprintResult current)
    {
        if (string.IsNullOrWhiteSpace(qualified)) return "UNKNOWN";
        if (!current.Passed || string.IsNullOrWhiteSpace(current.Sha256)) return "UNKNOWN";
        return string.Equals(qualified, current.Sha256, StringComparison.Ordinal) ? "CURRENT" : "LAST_SUCCESS";
    }
}

public sealed record GameProjectBuildHistoryReadResult
{
    public GameProjectBuildResult? LastSuccessfulBuild { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string CurrentAuthoringFingerprint { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public bool MatchesCurrentConfiguration { get; init; }
    public string SocialConfigurationStatus { get; init; } = "ABSENT";
}
