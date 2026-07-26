using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public enum GeneratedCampaignExactCombatRouteGoal
{
    FLEE,
    VICTORY
}

public sealed record GeneratedCampaignExactCombatRouteRequest
{
    public GamePackageDefinition FinalPackage { get; init; } = new();
    public string EncounterId { get; init; } = string.Empty;
    public GameProjectGeneratedEncounterCombatSummary CombatSummary { get; init; } = new();
    public IUnifiedGameRuntimeService Runtime { get; init; } = null!;
    public UnifiedRuntimeSession InitialSession { get; init; } = new();
    public GeneratedCampaignExactCombatRouteGoal Goal { get; init; }
}

public sealed record GeneratedCampaignExactCombatRouteResult
{
    public bool Passed { get; init; }
    public UnifiedRuntimeSession Session { get; init; } = new();
    public IReadOnlyList<GameRuntimeCommandType> Commands { get; init; } = [];
    public IReadOnlyList<GameRuntimeEventType> Events { get; init; } = [];
    public IReadOnlyList<string> UsedQualifiedActionFingerprints { get; init; } = [];
    public string PackageSha256Before { get; init; } = string.Empty;
    public string PackageSha256After { get; init; } = string.Empty;
    public string QualifiedActionsSha256 { get; init; } = string.Empty;
    public bool PackageReferenceUnchanged { get; init; }
    public bool EncounterProgressObserved { get; init; }
    public bool RewardObserved { get; init; }
    public bool QuestProgressObserved { get; init; }
    public bool ReputationChanged { get; init; }
    public int CommandBound { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedCampaignExactCombatRouteService
{
    public GeneratedCampaignExactCombatRouteResult Execute(
        GeneratedCampaignExactCombatRouteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FinalPackage);
        ArgumentNullException.ThrowIfNull(request.CombatSummary);
        ArgumentNullException.ThrowIfNull(request.Runtime);
        ArgumentNullException.ThrowIfNull(request.InitialSession);

        var packageBefore = PackageSha256(request.FinalPackage);
        var diagnostics = ValidateCatalog(request.FinalPackage, request.EncounterId,
            request.CombatSummary, packageBefore);
        if (diagnostics.Count > 0)
            return Failed(request.InitialSession, packageBefore, request.CombatSummary,
                diagnostics);

        var session = GeneratedEncounterCombatCanonical.Clone(request.InitialSession);
        var commands = new List<GameRuntimeCommandType>();
        var events = new List<GameRuntimeEventType>();
        var used = new List<string>();
        var inventoryBefore = GeneratedEncounterCombatCanonical.Hash(session.GameplayState.Inventories);
        var questsBefore = GeneratedEncounterCombatCanonical.Hash(new
        {
            session.GameplayState.QuestStates,
            session.GameplayState.Quests
        });
        var reputationBefore = GeneratedEncounterCombatCanonical.Hash(session.GameplayState.Factions);
        var rewardObserved = false;
        var progressObserved = false;

        if (session.GameplayState.ActiveEncounter is not
            {
                Active: true
            } active || !string.Equals(active.EncounterId, request.EncounterId,
                StringComparison.Ordinal))
        {
            var started = request.Runtime.ExecuteGameplayCommand(
                request.FinalPackage, session,
                GameRuntimeCommand.StartEncounter(request.EncounterId));
            commands.Add(GameRuntimeCommandType.StartEncounter);
            events.AddRange(started.GameplayEvents.Select(item => item.Type));
            if (!started.Success)
                return Failed(started.Session, packageBefore, request.CombatSummary,
                    ["generated_relationship.arc_combat_failed",
                     "generated_relationship.encounter_start_failed"], commands, events);
            session = started.Session;
        }

        if (request.Goal == GeneratedCampaignExactCombatRouteGoal.FLEE)
        {
            var fled = request.Runtime.ExecuteGameplayCommand(
                request.FinalPackage, session,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
            commands.Add(GameRuntimeCommandType.FleeEncounter);
            events.AddRange(fled.GameplayEvents.Select(item => item.Type));
            session = fled.Session;
            rewardObserved = fled.GameplayEvents.Any(IsReward);
            var questChanged = questsBefore != GeneratedEncounterCombatCanonical.Hash(new
            {
                session.GameplayState.QuestStates,
                session.GameplayState.Quests
            });
            var reputationChanged = reputationBefore !=
                                    GeneratedEncounterCombatCanonical.Hash(
                                        session.GameplayState.Factions);
            var inventoryChanged = inventoryBefore !=
                                   GeneratedEncounterCombatCanonical.Hash(
                                       session.GameplayState.Inventories);
            var passed = fled.Success
                         && session.GameplayState.ActiveEncounter is { Active: false }
                         && !rewardObserved
                         && !questChanged
                         && !reputationChanged
                         && !inventoryChanged;
            if (!passed) diagnostics.Add("generated_relationship.arc_combat_failed");
            return Complete(request, session, packageBefore, commands, events, used,
                progressObserved, rewardObserved, questChanged, reputationChanged, 1,
                diagnostics);
        }

        var commandBound = DynamicCommandBound(
            session.GameplayState.ActiveEncounter!,
            request.CombatSummary.QualifiedActions.Count);
        var seenTurnStates = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < commandBound
             && session.GameplayState.ActiveEncounter is { Active: true } encounter;
             index++)
        {
            if (encounter.Participants.Count == 0)
            {
                diagnostics.Add("generated_relationship.arc_combat_failed");
                break;
            }

            var current = encounter.Participants[
                Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1)];
            UnifiedRuntimeResult result;
            if (!IsPlayer(current.Team))
            {
                result = request.Runtime.ExecuteGameplayCommand(
                    request.FinalPackage, session,
                    new GameRuntimeCommand
                    {
                        Type = GameRuntimeCommandType.RunCurrentTurnAi
                    });
                commands.Add(GameRuntimeCommandType.RunCurrentTurnAi);
                events.AddRange(result.GameplayEvents.Select(item => item.Type));
                if (!result.Success)
                {
                    diagnostics.Add("generated_relationship.arc_combat_failed");
                    break;
                }
            }
            else
            {
                var target = encounter.Participants
                    .Where(item => item.Alive && !string.Equals(item.Team,
                        current.Team, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (target is null)
                {
                    diagnostics.Add("generated_relationship.arc_combat_failed");
                    break;
                }

                var action = ExecuteQualifiedPlayerAction(request, session, current.Id,
                    target.Id);
                if (action is null)
                {
                    diagnostics.Add("generated_relationship.qualified_action_no_progress");
                    diagnostics.Add("generated_relationship.arc_combat_failed");
                    break;
                }
                result = action.Result;
                commands.Add(action.CommandType);
                events.AddRange(result.GameplayEvents.Select(item => item.Type));
                used.Add(action.DescriptorFingerprint);
                progressObserved = true;
            }

            rewardObserved |= result.GameplayEvents.Any(IsReward);
            session = result.Session;
            var turnState = GeneratedEncounterCombatCanonical.Hash(
                session.GameplayState.ActiveEncounter);
            if (session.GameplayState.ActiveEncounter is { Active: true }
                && !seenTurnStates.Add(turnState))
            {
                diagnostics.Add("generated_relationship.qualified_action_no_progress");
                diagnostics.Add("generated_relationship.arc_combat_failed");
                break;
            }
        }

        var ended = session.GameplayState.ActiveEncounter;
        var victory = ended is { Active: false }
                      && ended.Participants.Any(item => item.Alive
                          && IsPlayer(item.Team))
                      && ended.Participants.Where(item => !IsPlayer(item.Team))
                          .All(item => !item.Alive)
                      && !ended.ActionHistory.Any(item => string.Equals(item, "flee",
                          StringComparison.OrdinalIgnoreCase));
        if (!victory) diagnostics.Add("generated_relationship.arc_combat_failed");
        var questProgress = questsBefore != GeneratedEncounterCombatCanonical.Hash(new
        {
            session.GameplayState.QuestStates,
            session.GameplayState.Quests
        });
        var reputationChangedAfter = reputationBefore !=
                                     GeneratedEncounterCombatCanonical.Hash(
                                         session.GameplayState.Factions);
        return Complete(request, session, packageBefore, commands, events, used,
            progressObserved, rewardObserved, questProgress, reputationChangedAfter,
            commandBound, diagnostics);
    }

    private static QualifiedActionExecution? ExecuteQualifiedPlayerAction(
        GeneratedCampaignExactCombatRouteRequest request,
        UnifiedRuntimeSession session,
        string sourceId,
        string targetId)
    {
        foreach (var descriptor in CanonicalActions(
                     request.CombatSummary.QualifiedActions))
        {
            if (!DescriptorDefinitionCurrent(request.FinalPackage, descriptor))
                continue;
            var attempt = GeneratedEncounterCombatCanonical.Clone(session);
            var before = GeneratedEncounterCombatCanonical.Clone(attempt);
            var command = descriptor.ActionKind ==
                          GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? GameRuntimeCommand.BasicAttack(sourceId, targetId)
                : GameRuntimeCommand.UseAbility(descriptor.AbilityId, sourceId, targetId);
            var result = request.Runtime.ExecuteGameplayCommand(
                request.FinalPackage, attempt, command);
            if (!result.Success
                || !GeneratedEncounterCombatContractService.TryObserveSupportedEffect(
                    request.FinalPackage, before, result.Session, targetId,
                    descriptor.ActionKind ==
                    GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY,
                    out var observed)
                || !ExactObservedEffect(descriptor, observed)
                || !string.Equals(observed.EffectClass,
                    "TARGET_HEALTH_DECREASE", StringComparison.Ordinal)
                || GeneratedEncounterCombatCanonical.Hash(
                    before.GameplayState.ActiveEncounter)
                == GeneratedEncounterCombatCanonical.Hash(
                    result.Session.GameplayState.ActiveEncounter))
                continue;
            return new QualifiedActionExecution(
                result,
                descriptor.RuntimeCommandType,
                DescriptorFingerprint(descriptor));
        }
        return null;
    }

    private static List<string> ValidateCatalog(
        GamePackageDefinition package,
        string encounterId,
        GameProjectGeneratedEncounterCombatSummary summary,
        string actualPackageSha256)
    {
        var diagnostics = new List<string>();
        if (!summary.Passed || summary.Status != "CAMPAIGN_CURRENT"
                            || summary.QualifiedActions.Count == 0)
            diagnostics.Add("generated_relationship.qualified_combat_catalog_missing");
        var actions = CanonicalActions(summary.QualifiedActions);
        var basic = actions.Count(item => item.ActionKind ==
                                          GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK);
        var ability = actions.Count(item => item.ActionKind ==
                                            GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
        if (summary.QualifiedActionCount != actions.Count
            || summary.QualifiedBasicAttackCount != basic
            || summary.QualifiedPackageAbilityCount != ability
            || !string.Equals(summary.QualifiedActionsSha256,
                GeneratedEncounterCombatCanonical.Hash(actions), StringComparison.Ordinal))
            diagnostics.Add("generated_relationship.qualified_combat_catalog_missing");
        if (!string.Equals(summary.ExactPackageSha256, actualPackageSha256,
                StringComparison.Ordinal))
            diagnostics.Add("generated_relationship.qualified_action_definition_changed");
        if (package.Game.Encounters.Count(item => string.Equals(item.Id, encounterId,
                StringComparison.Ordinal)) != 1)
            diagnostics.Add("generated_relationship.arc_combat_failed");
        if (actions.Any(item => !DescriptorDefinitionCurrent(package, item)
                                || !DescriptorShapeCurrent(item)))
            diagnostics.Add("generated_relationship.qualified_action_definition_changed");
        return diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static bool DescriptorShapeCurrent(
        GeneratedEncounterCombatQualifiedAction descriptor)
    {
        if (!descriptor.RuntimeQualificationPassed
            || string.IsNullOrWhiteSpace(descriptor.ObservedEffect.EffectClass)
            || string.IsNullOrWhiteSpace(descriptor.ObservedEffect.Fingerprint)
            || descriptor.RuntimeCommandType !=
            (descriptor.ActionKind ==
             GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? GameRuntimeCommandType.BasicAttack
                : GameRuntimeCommandType.UseAbility))
            return false;
        if (descriptor.ActionKind ==
            GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
            && !string.IsNullOrWhiteSpace(descriptor.AbilityId))
            return false;
        if (descriptor.ActionKind ==
            GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY
            && (string.IsNullOrWhiteSpace(descriptor.AbilityId)
                || string.IsNullOrWhiteSpace(descriptor.AbilityDefinitionSha256)))
            return false;
        var expectedFingerprint = GeneratedEncounterCombatCanonical.Hash(new
        {
            effectClass = descriptor.ObservedEffect.EffectClass,
            damagedResources = descriptor.ObservedEffect.TargetResourceIds
                .OrderBy(item => item, StringComparer.Ordinal).ToList(),
            changedStats = descriptor.ObservedEffect.TargetStatIds
                .OrderBy(item => item, StringComparer.Ordinal).ToList(),
            changedStatuses = descriptor.ObservedEffect.TargetStatusIds
                .OrderBy(item => item, StringComparer.Ordinal).ToList()
        });
        return string.Equals(expectedFingerprint,
                   descriptor.ObservedEffect.Fingerprint, StringComparison.Ordinal)
               && descriptor.TargetResourceIds.SequenceEqual(
                   descriptor.ObservedEffect.TargetResourceIds, StringComparer.Ordinal)
               && descriptor.TargetStatIds.SequenceEqual(
                   descriptor.ObservedEffect.TargetStatIds, StringComparer.Ordinal)
               && descriptor.TargetStatusIds.SequenceEqual(
                   descriptor.ObservedEffect.TargetStatusIds, StringComparer.Ordinal);
    }

    private static bool DescriptorDefinitionCurrent(
        GamePackageDefinition package,
        GeneratedEncounterCombatQualifiedAction descriptor) =>
        descriptor.RuntimeQualificationPassed
        && (descriptor.ActionKind ==
            GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
            || package.Game.Abilities.SingleOrDefault(item =>
                string.Equals(item.Id, descriptor.AbilityId,
                    StringComparison.Ordinal)) is { } ability
            && string.Equals(GeneratedEncounterCombatCanonical.Hash(ability),
                descriptor.AbilityDefinitionSha256, StringComparison.Ordinal));

    private static bool ExactObservedEffect(
        GeneratedEncounterCombatQualifiedAction descriptor,
        GeneratedEncounterCombatObservedEffect observed) =>
        GeneratedEncounterCombatContractService.MatchesObservedEffect(
            descriptor, observed)
        && string.Equals(descriptor.ObservedEffect.Fingerprint,
            observed.Fingerprint, StringComparison.Ordinal)
        && descriptor.TargetResourceIds.SequenceEqual(
            observed.TargetResourceIds, StringComparer.Ordinal)
        && descriptor.TargetStatIds.SequenceEqual(
            observed.TargetStatIds, StringComparer.Ordinal)
        && descriptor.TargetStatusIds.SequenceEqual(
            observed.TargetStatusIds, StringComparer.Ordinal);

    private static List<GeneratedEncounterCombatQualifiedAction> CanonicalActions(
        IReadOnlyList<GeneratedEncounterCombatQualifiedAction> actions) =>
        actions.OrderBy(item => item.ActionKind ==
                                GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? 0
                : 1)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal)
            .ThenBy(item => item.ObservedEffect.Fingerprint, StringComparer.Ordinal)
            .ToList();

    private static int DynamicCommandBound(
        EncounterRuntimeState encounter,
        int qualifiedActionCount)
    {
        var resourceUnits = encounter.Participants.SelectMany(item => item.Resources)
            .Sum(item => Math.Max(0, item.Amount));
        var statUnits = encounter.Participants.SelectMany(item => item.Stats)
            .Sum(item => Math.Max(0, Math.Abs(item.Value)));
        var statusUnits = encounter.Participants.SelectMany(item => item.Statuses)
            .Sum(item => Math.Max(1, item.RemainingTicks ?? 1));
        var liveUnits = Math.Max(1, (int)Math.Ceiling(
            resourceUnits + statUnits + statusUnits));
        var turnCycleWidth = Math.Max(1, encounter.Participants.Count);
        return checked(liveUnits
                       * turnCycleWidth
                       * turnCycleWidth
                       * Math.Max(1, qualifiedActionCount));
    }

    private static GeneratedCampaignExactCombatRouteResult Complete(
        GeneratedCampaignExactCombatRouteRequest request,
        UnifiedRuntimeSession session,
        string packageBefore,
        IReadOnlyList<GameRuntimeCommandType> commands,
        IReadOnlyList<GameRuntimeEventType> events,
        IReadOnlyList<string> used,
        bool progress,
        bool reward,
        bool questProgress,
        bool reputationChanged,
        int commandBound,
        IReadOnlyList<string> diagnostics)
    {
        var after = PackageSha256(request.FinalPackage);
        var packageUnchanged = string.Equals(packageBefore, after,
            StringComparison.Ordinal)
                               && string.Equals(packageBefore,
                                   request.CombatSummary.ExactPackageSha256,
                                   StringComparison.Ordinal);
        var finalDiagnostics = diagnostics.ToList();
        if (!packageUnchanged)
            finalDiagnostics.Add(
                "generated_relationship.qualified_action_definition_changed");
        finalDiagnostics = finalDiagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignExactCombatRouteResult
        {
            Passed = finalDiagnostics.Count == 0,
            Session = session,
            Commands = commands,
            Events = events,
            UsedQualifiedActionFingerprints = used,
            PackageSha256Before = packageBefore,
            PackageSha256After = after,
            QualifiedActionsSha256 = request.CombatSummary.QualifiedActionsSha256,
            PackageReferenceUnchanged = packageUnchanged,
            EncounterProgressObserved = progress,
            RewardObserved = reward,
            QuestProgressObserved = questProgress,
            ReputationChanged = reputationChanged,
            CommandBound = commandBound,
            Diagnostics = finalDiagnostics
        };
    }

    private static GeneratedCampaignExactCombatRouteResult Failed(
        UnifiedRuntimeSession session,
        string packageBefore,
        GameProjectGeneratedEncounterCombatSummary summary,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<GameRuntimeCommandType>? commands = null,
        IReadOnlyList<GameRuntimeEventType>? events = null) => new()
    {
        Session = session,
        Commands = commands ?? [],
        Events = events ?? [],
        PackageSha256Before = packageBefore,
        PackageSha256After = packageBefore,
        QualifiedActionsSha256 = summary.QualifiedActionsSha256,
        PackageReferenceUnchanged = true,
        Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList()
    };

    private static bool IsReward(GameRuntimeEvent item) =>
        item.Type is GameRuntimeEventType.RewardGranted
            or GameRuntimeEventType.QuestRewardGranted;

    private static bool IsPlayer(string? team) =>
        string.Equals(team, "player", StringComparison.OrdinalIgnoreCase);

    private static string PackageSha256(GamePackageDefinition package) =>
        GeneratedEncounterCombatCanonical.HashText(
            GeneratedEncounterCombatCanonical.Serialize(package)
            + Environment.NewLine);

    private static string DescriptorFingerprint(
        GeneratedEncounterCombatQualifiedAction descriptor) =>
        GeneratedEncounterCombatCanonical.Hash(descriptor);

    private sealed record QualifiedActionExecution(
        UnifiedRuntimeResult Result,
        GameRuntimeCommandType CommandType,
        string DescriptorFingerprint);
}
