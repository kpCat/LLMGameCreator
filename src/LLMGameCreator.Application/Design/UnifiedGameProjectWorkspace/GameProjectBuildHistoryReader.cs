using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

/// <summary>Reads validated persisted last-success summaries without mutating project history.</summary>
public sealed class GameProjectBuildHistoryReader
{
    public const string SchemaVersionV2 = "unified_game_project_build_history_v2";
    public const string SchemaVersionV3 = "unified_game_project_build_history_v3";
    public const string SchemaVersionV4 = "unified_game_project_build_history_v4";
    public const string SchemaVersionV5 = "unified_game_project_build_history_v5";
    public const string SchemaVersionV6 = "unified_game_project_build_history_v6";
    public const string SchemaVersionV7 = "unified_game_project_build_history_v7";
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
        var (actualPackage, actualPackageSha256) =
            TryLoadActualPackage(projectFolder);
        var candidates = new List<(GameProjectBuildHistoryEntry Entry, string FileName)>();
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json", SearchOption.TopDirectoryOnly))
        {
            GameProjectBuildHistoryEntry? historyEntry;
            try { historyEntry = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(File.ReadAllText(path), JsonOptions); }
            catch (JsonException) { diagnostics.Add("social.history.invalid_json:" + Path.GetFileName(path)); continue; }
            catch (IOException) { diagnostics.Add("social.history.unreadable:" + Path.GetFileName(path)); continue; }

            if (historyEntry is null
                || historyEntry.SchemaVersion is not SchemaVersionV2
                    and not SchemaVersionV3 and not SchemaVersionV4
                    and not SchemaVersionV5 and not SchemaVersionV6
                    and not SchemaVersionV7)
            {
                diagnostics.Add("social.history.unsupported_schema:" + Path.GetFileName(path));
                continue;
            }
            if (IsMatchingGreenSuccess(historyEntry, document,
                    actualPackage, actualPackageSha256))
                candidates.Add((historyEntry, Path.GetFileName(path)));
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
                GeneratedCampaignChoices = ProjectGeneratedChoices(entry),
                GeneratedCampaignRelationships = ProjectGeneratedRelationships(entry),
                GeneratedCampaignRegionalEvents =
                    ProjectGeneratedRegionalEvents(entry),
                AcceptedMechanicsCompatibility = entry.AcceptedMechanicsCompatibility
            },
            Diagnostics = diagnostics.Concat(fingerprint.Diagnostics).ToList(),
            CurrentAuthoringFingerprint = fingerprint.Sha256,
            QualifiedAuthoringFingerprint = entry.QualifiedAuthoringFingerprint,
            SocialConfigurationStatus = status,
            MatchesCurrentConfiguration = status == "CURRENT"
        };
    }

    private static bool IsMatchingGreenSuccess(
        GameProjectBuildHistoryEntry entry,
        FeatureModuleCompositionDocument document,
        GamePackageDefinition? actualPackage,
        string actualPackageSha256) =>
        string.Equals(entry.Status, "GREEN", StringComparison.Ordinal)
        && string.Equals(entry.AttemptStatus, "GREEN", StringComparison.Ordinal)
        && (entry.GeneratedWorld is { Present: true }
            ? entry.GeneratedWorld is { Passed: true, PackageContentPreserved: true }
              && entry.GeneratedWorldActivation is
                  { Present: true, Passed: true, ReplayEquivalent: true, StateRoundtripPassed: true }
              && (entry.SchemaVersion == SchemaVersionV2
                  || entry.SchemaVersion == SchemaVersionV3 && TravelEligible(entry)
                  || entry.SchemaVersion == SchemaVersionV4
                  && TravelEligible(entry) && CombatEligible(entry)
                  || entry.SchemaVersion == SchemaVersionV5
                  && TravelEligible(entry) && CombatEligible(entry) && ChoiceEligible(entry)
                  || entry.SchemaVersion == SchemaVersionV6
                  && TravelEligible(entry) && CombatEligible(entry) && ChoiceEligible(entry)
                  && RelationshipEligible(entry)
                  || entry.SchemaVersion == SchemaVersionV7
                  && TravelEligible(entry) && CombatEligible(entry)
                  && ChoiceEligible(entry) && RelationshipEligible(entry)
                  && RegionalEventEligible(entry, actualPackage,
                      actualPackageSha256))
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
        && (entry.SchemaVersion is SchemaVersionV4 or SchemaVersionV5
            or SchemaVersionV6 or SchemaVersionV7
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
        && QualifiedActionCatalogEligible(combat)
        && combat.QualifiedEncounterCount == combat.GeneratedEncounterCount
        && string.Equals(combat.ExactPackageSha256, entry.PackageSha256, StringComparison.Ordinal)
        && (entry.SchemaVersion is SchemaVersionV5 or SchemaVersionV6
            or SchemaVersionV7
            || string.Equals(combat.FinalStateHash, entry.FinalStateHash, StringComparison.Ordinal));

    private static bool ChoiceEligible(GameProjectBuildHistoryEntry entry) => entry.GeneratedCampaignChoices is
        {
            Present: true,
            Passed: true,
            Status: "CHOICE_CURRENT",
            RuntimeQualificationPassed: true,
            ExclusiveBranchingPassed: true,
            FollowUpPassed: true,
            ChallengeFleeFollowUpPassed: true,
            ChallengeVictoryFollowUpPassed: true,
            AtomicRollbackPassed: true,
            ReplayPassed: true
        } choices
        && choices.BranchableDialogueCount == choices.QualifiedDialogueCount
        && choices.RuntimeFrames.Count == (choices.SupportBranchCount + choices.ChallengeBranchCount
                                           + choices.RefuseBranchCount) * 2
        && choices.RuntimeFrames.GroupBy(item => (item.DialogueId, item.BranchKind))
            .All(group => group.Select(item => item.ReplayIndex).OrderBy(item => item).SequenceEqual([1, 2]))
        && choices.BranchFlagIds.Distinct(StringComparer.Ordinal).Count() == choices.BranchFlagIds.Count
        && choices.BranchFlagIds.Count == choices.BranchableDialogueCount
        && !string.IsNullOrWhiteSpace(choices.BranchFlagInventorySha256)
        && string.Equals(choices.FinalPackageSha256, entry.PackageSha256, StringComparison.Ordinal)
        && (entry.SchemaVersion is SchemaVersionV6 or SchemaVersionV7
            || string.Equals(choices.FinalStateHash, entry.FinalStateHash, StringComparison.Ordinal));

    private static bool RelationshipEligible(GameProjectBuildHistoryEntry entry)
    {
        var relationships = entry.GeneratedCampaignRelationships;
        if (relationships is
            {
                Present: false,
                Passed: true,
                Status: "ABSENT",
                RelationshipCount: 0,
                ArcQuestCount: 0
            })
            return true;
        if (relationships is not
            {
                Present: true,
                Passed: true,
                Status: "RELATIONSHIPS_CURRENT",
                AssignmentUnique: true,
                ArcOrderingDeterministic: true,
                OverlayControlledDeltaPassed: true,
                RuntimeQualificationPassed: true,
                ExclusiveBranchingPassed: true,
                ArcProgressionPassed: true,
                ExactCombatCatalogPassed: true,
                SupportPassed: true,
                SupportReplayEquivalent: true,
                ChallengeFleePassed: true,
                ChallengeVictoryPassed: true,
                ChallengeRecoveryPassed: true,
                RefusePassed: true,
                AtomicRollbackPassed: true
            })
            return false;
        var common =
            relationships.RelationshipCount ==
            relationships.QualifiedRelationshipCount
            && relationships.RelationshipInventory.Count ==
            relationships.RelationshipCount
            && relationships.RelationshipInventory
                .SelectMany(item => item.OrderedQuestSourceIds)
                .Distinct(StringComparer.Ordinal).Count() ==
            relationships.ArcQuestCount
            && relationships.RuntimeFrames.All(item => item.Passed)
            && !string.IsNullOrWhiteSpace(
                relationships.RelationshipOverlaySha256)
            && !string.IsNullOrWhiteSpace(
                relationships.RelationshipInventorySha256)
            && string.Equals(relationships.ExactPackageSha256,
                entry.PackageSha256, StringComparison.Ordinal);
        if (!common)
            return false;

        if (entry.SchemaVersion == SchemaVersionV6)
            return relationships.ArcQuestCount ==
                   relationships.QualifiedArcQuestCount
                   && relationships.MaximumObservedArcLength > 0
                   && relationships.RuntimeFrames.Count > 0
                   && relationships.SaveContinuationFactsPassed
                   && relationships.RelationshipInventory.All(item =>
                       item.BranchKinds.OrderBy(value => value)
                           .SequenceEqual(Enum.GetValues<
                                   GeneratedCampaignRelationshipBranch>()
                               .OrderBy(value => value)))
                   && string.Equals(relationships.FinalStateHash,
                       entry.FinalStateHash,
                       StringComparison.Ordinal)
                   && entry.GeneratedEncounterCombat is { } legacyCombat
                   && string.Equals(
                       relationships.QualifiedActionsSha256,
                       legacyCombat.QualifiedActionsSha256,
                       StringComparison.Ordinal);

        if (entry.SchemaVersion != SchemaVersionV7)
            return false;
        var matrix = relationships.BranchQualifications;
        if (relationships.ArcQuestCount !=
            relationships.QualifiedArcQuestCount
            || matrix.Count != relationships.RelationshipCount * 3
            || string.IsNullOrWhiteSpace(
                relationships.RelationshipBranchMatrixSha256)
            || !string.Equals(
                relationships.RelationshipBranchMatrixSha256,
                GeneratedCampaignChoiceCanonical.Hash(matrix),
                StringComparison.Ordinal)
            || relationships.SaveContinuationFactsPassed
            || relationships.SaveContinuationFactsEvaluationStatus !=
            "NOT_EVALUATED_AT_BUILD")
            return false;
        foreach (var inventory in relationships.RelationshipInventory)
        {
            var facts = matrix.Where(item =>
                    item.RelationshipId == inventory.RelationshipId)
                .OrderBy(item => item.Branch).ToList();
            if (facts.Count != 3
                || !facts.Select(item => item.Branch)
                    .SequenceEqual(Enum.GetValues<
                            GeneratedCampaignRelationshipBranch>()
                        .OrderBy(value => value)))
                return false;
            foreach (var fact in facts)
            {
                var available = inventory.BranchKinds.Contains(
                    fact.Branch);
                if (fact.Available != available
                    || fact.Required != available
                    || !fact.Passed || !fact.ReplayEquivalent)
                    return false;
                if (!available && (fact.RuntimeStartCount != 0
                                   || fact.RuntimeCommandCount != 0))
                    return false;
                if (available
                    && fact.Branch ==
                    GeneratedCampaignRelationshipBranch.SUPPORT
                    && (fact.ArcLength <= 0
                        || fact.ArcLength !=
                        inventory.OrderedQuestSourceIds.Count))
                    return false;
            }
        }
        var support = matrix.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.SUPPORT)
            .ToList();
        var challenge = matrix.Where(item =>
            item.Branch ==
            GeneratedCampaignRelationshipBranch.CHALLENGE).ToList();
        var refuse = matrix.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.REFUSE)
            .ToList();
        var combatExact = string.IsNullOrWhiteSpace(
            relationships.QualifiedActionsSha256)
            ? relationships.ExactCombatCatalogPassed
            : entry.GeneratedEncounterCombat is { } combat
              && string.Equals(
                  relationships.QualifiedActionsSha256,
                  combat.QualifiedActionsSha256,
                  StringComparison.Ordinal);
        return CountsEligible(support,
                   relationships.SupportAvailableCount,
                   relationships.SupportRequiredCount,
                   relationships.SupportQualifiedCount)
               && CountsEligible(challenge,
                   relationships.ChallengeAvailableCount,
                   relationships.ChallengeRequiredCount,
                   relationships.ChallengeQualifiedCount)
               && CountsEligible(refuse,
                   relationships.RefuseAvailableCount,
                   relationships.RefuseRequiredCount,
                   relationships.RefuseQualifiedCount)
               && relationships.UnavailableBranchRuntimeStartCount == 0
               && combatExact;
    }

    private static bool CountsEligible(
        IReadOnlyList<GeneratedCampaignRelationshipBranchQualification>
            facts,
        int available,
        int required,
        int qualified) =>
        available == facts.Count(item => item.Available)
        && required == facts.Count(item => item.Required)
        && qualified == facts.Count(item =>
            item.Required && item.Passed);

    private static bool RegionalEventEligible(
        GameProjectBuildHistoryEntry entry,
        GamePackageDefinition? actualPackage,
        string actualPackageSha256)
    {
        var events = entry.GeneratedCampaignRegionalEvents;
        var relationships = entry.GeneratedCampaignRelationships;
        if (events is null || relationships is null)
            return false;
        if (actualPackage is null
            || actualPackageSha256 != entry.PackageSha256)
            return false;
        var expectedEventCount = relationships.BranchQualifications
            .Count(item => item.Available);
        var common = events.EventCount == expectedEventCount
                     && events.QualifiedEventCount ==
                     events.EventCount
                     && events.IdentityPassed
                     && events.PlacementPassed
                     && events.OverlayControlledDeltaPassed
                     && events.RuntimeQualificationPassed
                     && events.LockedStatePassed
                     && events.AvailableStatePassed
                     && events.ResolvedStatePassed
                     && events.ExactlyOncePassed
                     && events.ReplayPassed
                     && events.FinalStateHash == entry.FinalStateHash;
        if (!common)
            return false;
        return GeneratedCampaignRegionalEventCorrelationService
            .Validate(actualPackage, actualPackageSha256, events,
                relationships)
            .Passed;
    }

    private static (GamePackageDefinition? Package, string Sha256)
        TryLoadActualPackage(string projectFolder)
    {
        try
        {
            var path = GameProjectFeatureModuleAuthoringService
                .ConfinedPath(projectFolder, "package.json");
            if (!File.Exists(path))
                return (null, string.Empty);
            var text = File.ReadAllText(path, Encoding.UTF8);
            var package = JsonSerializer.Deserialize<
                GamePackageDefinition>(text,
                GeneratedEncounterCombatCanonical.JsonOptions);
            if (package is null)
                return (null, string.Empty);
            using var stream = File.OpenRead(path);
            return (package,
                Convert.ToHexString(SHA256.HashData(stream))
                    .ToLowerInvariant());
        }
        catch (Exception exception) when (exception is IOException
                                           or UnauthorizedAccessException
                                           or JsonException
                                           or InvalidOperationException)
        {
            return (null, string.Empty);
        }
    }

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

    private static bool QualifiedActionCatalogEligible(GameProjectGeneratedEncounterCombatSummary combat)
    {
        var catalogDeclared = combat.QualifiedActionCount > 0
                              || !string.IsNullOrWhiteSpace(combat.QualifiedActionsSha256)
                              || combat.QualifiedActions.Count > 0;
        if (!catalogDeclared)
            return true; // Goal164/165 v4 rows precede the exact catalog and retain route truth only.
        var actions = combat.QualifiedActions.OrderBy(item => item.ActionKind)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal)
            .ThenBy(item => item.ObservedEffect.Fingerprint, StringComparer.Ordinal).ToList();
        var basic = actions.Count(item => item.ActionKind
            == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK);
        var abilities = actions.Count(item => item.ActionKind
            == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
        var mode = (basic > 0, abilities > 0) switch
        {
            (true, true) => GeneratedEncounterCombatRouteMode.BOTH,
            (true, false) => GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY,
            (false, true) => GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY,
            _ => GeneratedEncounterCombatRouteMode.NONE
        };
        return actions.Count > 0
               && combat.QualifiedActionCount == actions.Count
               && combat.QualifiedBasicAttackCount == basic
               && combat.QualifiedPackageAbilityCount == abilities
               && string.Equals(combat.QualifiedActionsSha256, GeneratedEncounterCombatCanonical.Hash(actions),
                   StringComparison.Ordinal)
               && combat.RouteMode == mode
               && actions.All(item => item.RuntimeQualificationPassed
                   && (item.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                       ? string.IsNullOrWhiteSpace(item.AbilityId)
                       : !string.IsNullOrWhiteSpace(item.AbilityId)
                         && !string.IsNullOrWhiteSpace(item.AbilityDefinitionSha256)));
    }

    private static GameProjectGeneratedWorldSummary? ProjectGeneratedWorld(GameProjectBuildHistoryEntry entry)
    {
        var generatedWorld = entry.GeneratedWorld;
        if (generatedWorld is null
            || entry.GeneratedWorldActivation is not { Present: true, Passed: true })
            return generatedWorld;

        if (IsGoal157(entry)) return generatedWorld with { Status = "START_CURRENT" };
        if (entry.SchemaVersion == SchemaVersionV5
            && entry.GeneratedCampaignChoices is
                { Present: true, Passed: true, Status: "CHOICE_CURRENT" })
            return generatedWorld with { Status = "RELATIONSHIPS_PENDING" };
        if (entry.SchemaVersion == SchemaVersionV6
            && entry.GeneratedCampaignRelationships is
            {
                Passed: true,
                Status: "RELATIONSHIPS_CURRENT"
            })
            return generatedWorld with
            {
                Status = "REGIONAL_EVENTS_PENDING"
            };
        return entry.SchemaVersion == SchemaVersionV3
            ? generatedWorld with { Status = "TRAVEL_CURRENT" }
            : generatedWorld;
    }

    private static bool IsGoal157(GameProjectBuildHistoryEntry entry) =>
        string.Equals(entry.SchemaVersion, SchemaVersionV2, StringComparison.Ordinal);

    private static GameProjectGeneratedEncounterCombatSummary? ProjectGeneratedCombat(
        GameProjectBuildHistoryEntry entry)
    {
        if (string.Equals(entry.SchemaVersion, SchemaVersionV4, StringComparison.Ordinal)
            || string.Equals(entry.SchemaVersion, SchemaVersionV5, StringComparison.Ordinal)
            || string.Equals(entry.SchemaVersion, SchemaVersionV6, StringComparison.Ordinal)
            || string.Equals(entry.SchemaVersion, SchemaVersionV7, StringComparison.Ordinal))
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

    private static GameProjectGeneratedCampaignChoiceSummary? ProjectGeneratedChoices(
        GameProjectBuildHistoryEntry entry)
    {
        if (string.Equals(entry.SchemaVersion, SchemaVersionV5, StringComparison.Ordinal)
            || string.Equals(entry.SchemaVersion, SchemaVersionV6, StringComparison.Ordinal)
            || string.Equals(entry.SchemaVersion, SchemaVersionV7, StringComparison.Ordinal))
            return entry.GeneratedCampaignChoices;
        if (string.Equals(entry.SchemaVersion, SchemaVersionV4, StringComparison.Ordinal)
            && entry.GeneratedEncounterCombat is { Present: true, Status: "CAMPAIGN_CURRENT" })
            return new GameProjectGeneratedCampaignChoiceSummary
            {
                Present = true,
                Status = "CHOICES_PENDING",
                Diagnostics = ["campaign.generated_choices_not_current"]
            };
        return entry.SchemaVersion == SchemaVersionV3 && entry.GeneratedWorld is { Present: true }
            ? new GameProjectGeneratedCampaignChoiceSummary { Status = "ABSENT" }
            : null;
    }

    private static GameProjectGeneratedCampaignRelationshipSummary? ProjectGeneratedRelationships(
        GameProjectBuildHistoryEntry entry)
    {
        if (string.Equals(entry.SchemaVersion, SchemaVersionV7,
                StringComparison.Ordinal))
            return entry.GeneratedCampaignRelationships;
        if (string.Equals(entry.SchemaVersion, SchemaVersionV6,
                StringComparison.Ordinal)
            && entry.GeneratedCampaignRelationships is { } legacy)
        {
            if (!legacy.Present)
                return legacy with
                {
                    SaveContinuationFactsPassed = false,
                    SaveContinuationFactsEvaluationStatus =
                        "NOT_EVALUATED_AT_BUILD"
                };
            var matrix = legacy.RelationshipInventory
                .SelectMany(inventory =>
                    Enum.GetValues<
                            GeneratedCampaignRelationshipBranch>()
                        .OrderBy(branch => branch)
                        .Select(branch => new
                            GeneratedCampaignRelationshipBranchQualification
                            {
                                RelationshipId =
                                    inventory.RelationshipId,
                                Branch = branch,
                                Available = true,
                                Required = true,
                                Passed = true,
                                ReplayEquivalent = true,
                                RuntimeStartCount = 1,
                                RuntimeCommandCount = 1,
                                ArcLength = branch ==
                                            GeneratedCampaignRelationshipBranch
                                                .SUPPORT
                                    ? inventory.OrderedQuestSourceIds.Count
                                    : 0,
                                FinalStateHash =
                                    legacy.FinalStateHash
                            }))
                .ToList();
            return legacy with
            {
                BranchQualifications = matrix,
                RelationshipBranchMatrixSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(matrix),
                SupportAvailableCount = legacy.RelationshipCount,
                SupportRequiredCount = legacy.RelationshipCount,
                SupportQualifiedCount = legacy.RelationshipCount,
                ChallengeAvailableCount = legacy.RelationshipCount,
                ChallengeRequiredCount = legacy.RelationshipCount,
                ChallengeQualifiedCount = legacy.RelationshipCount,
                RefuseAvailableCount = legacy.RelationshipCount,
                RefuseRequiredCount = legacy.RelationshipCount,
                RefuseQualifiedCount = legacy.RelationshipCount,
                UnavailableBranchRuntimeStartCount = 0,
                SaveContinuationFactsPassed = false,
                SaveContinuationFactsEvaluationStatus =
                    "NOT_EVALUATED_AT_BUILD"
            };
        }
        if (string.Equals(entry.SchemaVersion, SchemaVersionV5, StringComparison.Ordinal)
            && entry.GeneratedCampaignChoices is
                { Present: true, Passed: true, Status: "CHOICE_CURRENT" })
            return new GameProjectGeneratedCampaignRelationshipSummary
            {
                Present = true,
                Status = "RELATIONSHIPS_PENDING",
                Diagnostics = ["campaign.generated_relationships_not_current"]
            };
        return null;
    }

    private static GameProjectGeneratedCampaignRegionalEventSummary?
        ProjectGeneratedRegionalEvents(GameProjectBuildHistoryEntry entry)
    {
        if (entry.SchemaVersion == SchemaVersionV7)
            return entry.GeneratedCampaignRegionalEvents;
        if (entry.SchemaVersion == SchemaVersionV6
            && entry.GeneratedCampaignRelationships is
            {
                Passed: true,
                Status: "RELATIONSHIPS_CURRENT"
            } relationships)
            return new GameProjectGeneratedCampaignRegionalEventSummary
            {
                Present = relationships.RelationshipInventory.Any(item =>
                    item.BranchKinds.Count > 0),
                Status = "REGIONAL_EVENTS_PENDING",
                RelationshipBranchMatrixSha256 =
                    relationships.RelationshipBranchMatrixSha256,
                Diagnostics =
                [
                    "campaign.generated_regional_events_not_current"
                ]
            };
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
