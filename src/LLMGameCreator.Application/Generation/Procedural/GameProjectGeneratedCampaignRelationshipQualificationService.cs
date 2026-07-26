using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class
    GameProjectGeneratedCampaignRelationshipQualificationService
{
    private readonly GeneratedCampaignExactCombatRouteService _combatRoute;

    public GameProjectGeneratedCampaignRelationshipQualificationService(
        GeneratedCampaignExactCombatRouteService? combatRoute = null)
    {
        _combatRoute = combatRoute ??
                       new GeneratedCampaignExactCombatRouteService();
    }

    public GameProjectGeneratedCampaignRelationshipSummary Qualify(
        GamePackageDefinition finalPackage,
        GeneratedCampaignRelationshipOverlayDocument overlay,
        GameProjectGeneratedEncounterCombatSummary? combatSummary,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(finalPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(runtime);
        if (!overlay.Passed)
            return Invalid(overlay,
                overlay.Diagnostics.Count == 0
                    ? ["generated_relationship.overlay_invalid"]
                    : overlay.Diagnostics);
        if (overlay.RelationshipCount == 0)
            return new GameProjectGeneratedCampaignRelationshipSummary
            {
                Passed = true,
                Status = "ABSENT",
                Overlay = overlay,
                AssignmentUnique = overlay.AssignmentUnique,
                ArcOrderingDeterministic =
                    overlay.ArcOrderingDeterministic,
                OverlayControlledDeltaPassed =
                    overlay.ControlledDeltaPassed,
                RuntimeQualificationPassed = true,
                ExclusiveBranchingPassed = true,
                ArcProgressionPassed = true,
                ExactCombatCatalogPassed = true,
                SupportPassed = true,
                SupportReplayEquivalent = true,
                ChallengeFleePassed = true,
                ChallengeVictoryPassed = true,
                ChallengeRecoveryPassed = true,
                RefusePassed = true,
                AtomicRollbackPassed = true,
                SaveContinuationFactsPassed = false,
                SaveContinuationFactsEvaluationStatus =
                    "NOT_EVALUATED_AT_BUILD",
                RelationshipBranchMatrixSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(
                        Array.Empty<
                            GeneratedCampaignRelationshipBranchQualification>())
            };
        var needsCombat = overlay.Bindings.Any(item =>
            item.Branches.Contains(
                GeneratedCampaignRelationshipBranch.CHALLENGE)
            || item.Branches.Contains(
                GeneratedCampaignRelationshipBranch.SUPPORT)
            && item.QuestArc.Any(step =>
                !string.IsNullOrWhiteSpace(step.TargetEncounterId)));
        if (needsCombat && combatSummary is null)
            return Invalid(overlay,
                ["generated_relationship.qualified_combat_catalog_missing"]);

        var packageBefore = PackageSha256(finalPackage);
        var diagnostics = new List<string>();
        var frames = new List<GeneratedCampaignRelationshipRuntimeFrame>();
        var branchQualifications = new List<
            GeneratedCampaignRelationshipBranchQualification>();
        var relationshipPassed = 0;
        var arcQuestPassed = 0;
        var challengeFleePassed = true;
        var challengeVictoryPassed = true;
        var challengeRecoveryPassed = true;
        var exclusivePassed = true;
        string primaryFinalStateHash = string.Empty;

        foreach (var relationship in overlay.Bindings
                     .OrderBy(item => item.RelationshipId,
                         StringComparer.Ordinal))
        {
            foreach (var branch in Enum.GetValues<
                         GeneratedCampaignRelationshipBranch>())
            {
                var available = relationship.Branches.Contains(branch);
                if (!available)
                {
                    branchQualifications.Add(new
                        GeneratedCampaignRelationshipBranchQualification
                        {
                            RelationshipId =
                                relationship.RelationshipId,
                            Branch = branch,
                            Available = false,
                            Required = false,
                            Passed = true,
                            ReplayEquivalent = true,
                            RuntimeStartCount = 0,
                            RuntimeCommandCount = 0,
                            ArcLength = branch ==
                                GeneratedCampaignRelationshipBranch.SUPPORT
                                ? relationship.QuestArc.Count
                                : 0
                        });
                    continue;
                }

                if (branch == GeneratedCampaignRelationshipBranch.SUPPORT)
                {
                    if (relationship.QuestArc.Count == 0)
                    {
                        const string diagnostic =
                            "generated_relationship.support_requires_arc";
                        diagnostics.Add(diagnostic);
                        branchQualifications.Add(new
                            GeneratedCampaignRelationshipBranchQualification
                            {
                                RelationshipId =
                                    relationship.RelationshipId,
                                Branch = branch,
                                Available = true,
                                Required = true,
                                Passed = false,
                                ReplayEquivalent = false,
                                ArcLength = 0,
                                Diagnostics = [diagnostic]
                            });
                        continue;
                    }
                    var first = ExecuteSupport(finalPackage, relationship,
                        combatSummary!, runtime, 1);
                    var second = ExecuteSupport(finalPackage, relationship,
                        combatSummary!, runtime, 2);
                    frames.AddRange(first.Frames);
                    frames.AddRange(second.Frames);
                    var replay = Equivalent(first, second);
                    var branchPassed =
                        first.Passed && second.Passed && replay;
                    exclusivePassed &= first.AlternativesLocked
                                       && second.AlternativesLocked;
                    var branchDiagnostics = first.Diagnostics.Concat(
                            second.Diagnostics)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList();
                    diagnostics.AddRange(branchDiagnostics);
                    if (!branchPassed)
                        diagnostics.Add(
                            "generated_relationship.required_branch_failed");
                    if (string.IsNullOrWhiteSpace(primaryFinalStateHash)
                        && first.Passed)
                        primaryFinalStateHash = first.FinalStateHash;
                    branchQualifications.Add(new
                        GeneratedCampaignRelationshipBranchQualification
                        {
                            RelationshipId =
                                relationship.RelationshipId,
                            Branch = branch,
                            Available = true,
                            Required = true,
                            Passed = branchPassed,
                            ReplayEquivalent = replay,
                            RuntimeStartCount = 2,
                            RuntimeCommandCount =
                                first.Frames.Count + second.Frames.Count,
                            ArcLength = relationship.QuestArc.Count,
                            FinalStateHash = first.FinalStateHash,
                            Diagnostics = branchDiagnostics
                        });
                    if (branchPassed)
                        arcQuestPassed += relationship.QuestArc.Count;
                    continue;
                }

                if (branch == GeneratedCampaignRelationshipBranch.CHALLENGE)
                {
                    var flee = ExecuteChallenge(finalPackage, relationship,
                        combatSummary!, runtime,
                        GeneratedCampaignExactCombatRouteGoal.FLEE);
                    var victory = ExecuteChallenge(finalPackage,
                        relationship, combatSummary!, runtime,
                        GeneratedCampaignExactCombatRouteGoal.VICTORY);
                    challengeFleePassed &= flee.Passed;
                    challengeVictoryPassed &= victory.Passed;
                    challengeRecoveryPassed &=
                        victory.RecoveryCompatible;
                    frames.AddRange(flee.Frames);
                    frames.AddRange(victory.Frames);
                    var branchDiagnostics = flee.Diagnostics.Concat(
                            victory.Diagnostics)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList();
                    diagnostics.AddRange(branchDiagnostics);
                    var branchPassed = flee.Passed && victory.Passed
                                                   && victory
                                                       .RecoveryCompatible;
                    if (!branchPassed)
                        diagnostics.Add(
                            "generated_relationship.required_branch_failed");
                    if (string.IsNullOrWhiteSpace(primaryFinalStateHash)
                        && victory.Passed)
                        primaryFinalStateHash = victory.FinalStateHash;
                    branchQualifications.Add(new
                        GeneratedCampaignRelationshipBranchQualification
                        {
                            RelationshipId =
                                relationship.RelationshipId,
                            Branch = branch,
                            Available = true,
                            Required = true,
                            Passed = branchPassed,
                            ReplayEquivalent = branchPassed,
                            RuntimeStartCount = 2,
                            RuntimeCommandCount =
                                flee.Frames.Count + victory.Frames.Count,
                            FinalStateHash = victory.FinalStateHash,
                            Diagnostics = branchDiagnostics
                        });
                    continue;
                }

                var refuse = ExecuteRefuse(finalPackage, relationship,
                    runtime);
                frames.AddRange(refuse.Frames);
                diagnostics.AddRange(refuse.Diagnostics);
                exclusivePassed &= refuse.AlternativesLocked;
                if (!refuse.Passed)
                    diagnostics.Add(
                        "generated_relationship.required_branch_failed");
                if (string.IsNullOrWhiteSpace(primaryFinalStateHash)
                    && refuse.Passed)
                    primaryFinalStateHash = refuse.FinalStateHash;
                branchQualifications.Add(new
                    GeneratedCampaignRelationshipBranchQualification
                    {
                        RelationshipId = relationship.RelationshipId,
                        Branch = branch,
                        Available = true,
                        Required = true,
                        Passed = refuse.Passed,
                        ReplayEquivalent = refuse.Passed,
                        RuntimeStartCount = 1,
                        RuntimeCommandCount = refuse.Frames.Count,
                        FinalStateHash = refuse.FinalStateHash,
                        Diagnostics = refuse.Diagnostics
                    });
            }

            if (branchQualifications.Where(item =>
                        item.RelationshipId ==
                        relationship.RelationshipId
                        && item.Required)
                    .All(item => item.Passed))
                relationshipPassed++;
        }

        var atomicTarget = overlay.Bindings
            .OrderBy(item => item.RelationshipId, StringComparer.Ordinal)
            .Select(item => new
            {
                Relationship = item,
                EncounterId = item.Branches.Contains(
                                  GeneratedCampaignRelationshipBranch
                                      .CHALLENGE)
                              && !string.IsNullOrWhiteSpace(
                                  item.ChallengeEncounterId)
                    ? item.ChallengeEncounterId
                    : item.Branches.Contains(
                        GeneratedCampaignRelationshipBranch.SUPPORT)
                        ? item.QuestArc.Select(step =>
                                step.TargetEncounterId)
                            .FirstOrDefault(id =>
                                !string.IsNullOrWhiteSpace(id))
                          ?? string.Empty
                        : string.Empty
            })
            .FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item.EncounterId));
        var atomic = atomicTarget is null || combatSummary is null
            || QualifyAtomicFailure(finalPackage,
                atomicTarget.Relationship, atomicTarget.EncounterId,
                combatSummary, runtime);
        if (!atomic) diagnostics.Add(
            "generated_relationship.atomic_rollback_failed");
        var packageAfter = PackageSha256(finalPackage);
        var packageUnchanged = string.Equals(packageBefore, packageAfter,
            StringComparison.Ordinal)
            && (!needsCombat || string.Equals(packageBefore,
                combatSummary!.ExactPackageSha256,
                StringComparison.Ordinal));
        if (!packageUnchanged)
            diagnostics.Add(
                "generated_relationship.qualified_action_definition_changed");
        var exactCatalog = !needsCombat ||
                           combatSummary is not null
                           && CatalogExact(combatSummary);
        if (!exactCatalog)
            diagnostics.Add(
                "generated_relationship.qualified_combat_catalog_missing");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var supportFacts = branchQualifications.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.SUPPORT)
            .ToList();
        var challengeFacts = branchQualifications.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.CHALLENGE)
            .ToList();
        var refuseFacts = branchQualifications.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.REFUSE)
            .ToList();
        var supportPassed = supportFacts.Where(item => item.Required)
            .All(item => item.Passed);
        var supportReplay = supportFacts.Where(item => item.Required)
            .All(item => item.ReplayEquivalent);
        var refusePassed = refuseFacts.Where(item => item.Required)
            .All(item => item.Passed);
        var requiredArcQuestCount = overlay.Bindings
            .Where(item => item.Branches.Contains(
                GeneratedCampaignRelationshipBranch.SUPPORT))
            .Sum(item => item.QuestArc.Count);
        var arcProgression = supportPassed
                             && arcQuestPassed == requiredArcQuestCount;
        var branchMatrixSha256 =
            GeneratedCampaignChoiceCanonical.Hash(branchQualifications);
        if (string.IsNullOrWhiteSpace(primaryFinalStateHash))
            primaryFinalStateHash = branchMatrixSha256;
        var passed = diagnostics.Count == 0
                     && relationshipPassed == overlay.RelationshipCount
                     && arcProgression
                     && supportReplay
                     && challengeFleePassed
                     && challengeVictoryPassed
                     && challengeRecoveryPassed
                     && refusePassed
                     && exclusivePassed
                     && atomic
                     && exactCatalog
                     && packageUnchanged;
        return new GameProjectGeneratedCampaignRelationshipSummary
        {
            Present = true,
            Passed = passed,
            Status = passed ? "RELATIONSHIPS_CURRENT" : "INVALID",
            RelationshipCount = overlay.RelationshipCount,
            QualifiedRelationshipCount = relationshipPassed,
            ArcQuestCount = overlay.ArcQuestCount,
            QualifiedArcQuestCount = arcQuestPassed,
            MaximumObservedArcLength = overlay.Bindings.Count == 0
                ? 0
                : overlay.Bindings.Max(item => item.QuestArc.Count),
            AssignmentUnique = overlay.AssignmentUnique,
            ArcOrderingDeterministic =
                overlay.ArcOrderingDeterministic,
            OverlayControlledDeltaPassed =
                overlay.ControlledDeltaPassed,
            RuntimeQualificationPassed = passed,
            ExclusiveBranchingPassed = exclusivePassed,
            ArcProgressionPassed = arcProgression,
            ExactCombatCatalogPassed = exactCatalog,
            SupportPassed = supportPassed,
            SupportReplayEquivalent = supportReplay,
            ChallengeFleePassed = challengeFleePassed,
            ChallengeVictoryPassed = challengeVictoryPassed,
            ChallengeRecoveryPassed = challengeRecoveryPassed,
            RefusePassed = refusePassed,
            AtomicRollbackPassed = atomic,
            SaveContinuationFactsPassed = false,
            SaveContinuationFactsEvaluationStatus =
                "NOT_EVALUATED_AT_BUILD",
            SupportAvailableCount = supportFacts.Count(item =>
                item.Available),
            SupportRequiredCount = supportFacts.Count(item =>
                item.Required),
            SupportQualifiedCount = supportFacts.Count(item =>
                item.Required && item.Passed),
            ChallengeAvailableCount = challengeFacts.Count(item =>
                item.Available),
            ChallengeRequiredCount = challengeFacts.Count(item =>
                item.Required),
            ChallengeQualifiedCount = challengeFacts.Count(item =>
                item.Required && item.Passed),
            RefuseAvailableCount = refuseFacts.Count(item =>
                item.Available),
            RefuseRequiredCount = refuseFacts.Count(item =>
                item.Required),
            RefuseQualifiedCount = refuseFacts.Count(item =>
                item.Required && item.Passed),
            UnavailableBranchRuntimeStartCount =
                branchQualifications.Where(item => !item.Available)
                    .Sum(item => item.RuntimeStartCount),
            ExactPackageSha256 = packageBefore,
            RelationshipOverlaySha256 =
                GeneratedCampaignChoiceCanonical.Hash(overlay),
            RelationshipInventorySha256 = overlay.InventorySha256,
            RelationshipBranchMatrixSha256 = branchMatrixSha256,
            QualifiedActionsSha256 =
                combatSummary?.QualifiedActionsSha256 ?? string.Empty,
            FinalStateHash = primaryFinalStateHash,
            RelationshipInventory = overlay.Inventory,
            BranchQualifications = branchQualifications,
            RuntimeFrames = frames,
            HumanReviewFacts =
            [
                new GeneratedCampaignRelationshipHumanFact
                {
                    Label = "Отношения",
                    Value = overlay.RelationshipCount.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRelationshipHumanFact
                {
                    Label = "Заданий в арках",
                    Value = overlay.ArcQuestCount.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRelationshipHumanFact
                {
                    Label = "Максимальная длина арки",
                    Value = overlay.Bindings.Max(item =>
                            item.QuestArc.Count)
                        .ToString(
                            System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignRelationshipHumanFact
                {
                    Label = "Боевые действия",
                    Value = exactCatalog
                        ? "точный каталог подтверждён"
                        : "каталог не подтверждён"
                }
            ],
            TechnicalDetails =
                new SortedDictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["relationshipOverlayPackageSha256"] =
                        overlay.OutputPackageSha256,
                    ["finalPackageSha256"] = packageBefore,
                    ["qualifiedActionsSha256"] =
                        combatSummary?.QualifiedActionsSha256
                        ?? string.Empty,
                    ["relationshipBranchMatrixSha256"] =
                        branchMatrixSha256,
                    ["supportReplayEquivalent"] =
                        supportReplay.ToString(),
                    ["atomicRollbackPassed"] = atomic.ToString(),
                    ["saveContinuationFactsEvaluationStatus"] =
                        "NOT_EVALUATED_AT_BUILD"
                },
            Overlay = overlay,
            Diagnostics = diagnostics
        };
    }

    private SupportRoute ExecuteSupport(
        GamePackageDefinition package,
        GeneratedCampaignRelationshipBinding relationship,
        GameProjectGeneratedEncounterCombatSummary combatSummary,
        IUnifiedGameRuntimeService runtime,
        int replayIndex)
    {
        var frames = new List<GeneratedCampaignRelationshipRuntimeFrame>();
        var diagnostics = new List<string>();
        var start = runtime.Start(package);
        if (!start.Success)
            return SupportRoute.Failed(
                "generated_relationship.runtime_start_failed");
        var opened = runtime.ExecuteGameplayCommand(package, start.Session,
            GameRuntimeCommand.OpenDialogue(relationship.DialogueId));
        if (!opened.Success)
            return SupportRoute.Failed(
                "generated_relationship.dialogue_open_failed");
        var initial = FindChoice(package, relationship.DialogueId,
            GeneratedCampaignRelationshipBranch.SUPPORT, "initial");
        if (initial is null)
            return SupportRoute.Failed(
                "generated_relationship.support_choice_missing");
        var initialAvailable = AvailableChoiceIds(opened);
        var alternativesLocked = initialAvailable.Contains(initial.Id);
        var reputationBefore = Reputation(opened.Session,
            relationship.FactionId);
        var before = StateHash(opened.Session);
        var chosen = runtime.ExecuteGameplayCommand(package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(initial.Id));
        AddFrame(frames, replayIndex, relationship,
            GeneratedCampaignRelationshipBranch.SUPPORT, 0,
            relationship.QuestArc[0].QuestId, string.Empty,
            GameRuntimeCommandType.ChooseDialogueOption, before, chosen);
        if (!chosen.Success
            || Flag(chosen.Session, relationship.DecisionFlagId) !=
            "SUPPORT"
            || !Exact(Reputation(chosen.Session, relationship.FactionId)
                     - reputationBefore,
                relationship.SupportReputationAmount)
            || QuestState(chosen.Session,
                relationship.QuestArc[0].QuestId) != "active"
            || relationship.QuestArc.Skip(1).Any(item =>
                QuestState(chosen.Session, item.QuestId) != "not_started"))
            return SupportRoute.Failed(
                "generated_relationship.support_initial_invalid",
                frames, alternativesLocked);

        var session = chosen.Session;
        foreach (var step in relationship.QuestArc)
        {
            if (QuestState(session, step.QuestId) != "active")
            {
                diagnostics.Add(
                    "generated_relationship.arc_quest_not_active");
                break;
            }
            if (!string.IsNullOrWhiteSpace(step.TargetEncounterId))
            {
                var combat = _combatRoute.Execute(
                    new GeneratedCampaignExactCombatRouteRequest
                    {
                        FinalPackage = package,
                        EncounterId = step.TargetEncounterId,
                        CombatSummary = combatSummary,
                        Runtime = runtime,
                        InitialSession = session,
                        Goal =
                            GeneratedCampaignExactCombatRouteGoal.VICTORY
                    });
                foreach (var command in combat.Commands)
                    frames.Add(new GeneratedCampaignRelationshipRuntimeFrame
                    {
                        ReplayIndex = replayIndex,
                        RelationshipId = relationship.RelationshipId,
                        Branch =
                            GeneratedCampaignRelationshipBranch.SUPPORT,
                        ArcStep = step.Order,
                        QuestId = step.QuestId,
                        EncounterId = step.TargetEncounterId,
                        CommandType = command.ToString(),
                        BeforeStateHash = string.Empty,
                        AfterStateHash = StateHash(combat.Session),
                        CommandSha256 =
                            GeneratedCampaignChoiceCanonical.Hash(command),
                        EventSha256 =
                            GeneratedCampaignChoiceCanonical.Hash(
                                combat.Events),
                        Passed = combat.Passed
                    });
                if (!combat.Passed)
                {
                    diagnostics.AddRange(combat.Diagnostics);
                    break;
                }
                session = combat.Session;
            }

            var completeBefore = StateHash(session);
            var completed = runtime.ExecuteGameplayCommand(package, session,
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.CompleteQuest,
                    Id = step.QuestId
                });
            AddFrame(frames, replayIndex, relationship,
                GeneratedCampaignRelationshipBranch.SUPPORT, step.Order,
                step.QuestId, step.TargetEncounterId,
                GameRuntimeCommandType.CompleteQuest, completeBefore,
                completed);
            if (!completed.Success
                || QuestState(completed.Session, step.QuestId) !=
                "completed")
            {
                diagnostics.Add(
                    "generated_relationship.arc_manual_turn_in_failed");
                break;
            }
            session = completed.Session;
            var reopened = runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.OpenDialogue(relationship.DialogueId));
            if (!reopened.Success)
            {
                diagnostics.Add(
                    "generated_relationship.dialogue_reopen_failed");
                break;
            }
            var available = AvailableChoiceIds(reopened);
            if (step.Order + 1 < relationship.QuestArc.Count)
            {
                var next = relationship.QuestArc[step.Order + 1];
                var followUp = FindNextChoice(package,
                    relationship.DialogueId, next.QuestId);
                if (followUp is null || !available.Contains(followUp.Id))
                {
                    diagnostics.Add(
                        "generated_relationship.next_quest_followup_missing");
                    break;
                }
                var nextBefore = StateHash(reopened.Session);
                var started = runtime.ExecuteGameplayCommand(package,
                    reopened.Session,
                    GameRuntimeCommand.ChooseDialogueOption(followUp.Id));
                AddFrame(frames, replayIndex, relationship,
                    GeneratedCampaignRelationshipBranch.SUPPORT,
                    next.Order, next.QuestId, string.Empty,
                    GameRuntimeCommandType.ChooseDialogueOption,
                    nextBefore, started);
                if (!started.Success
                    || QuestState(started.Session, next.QuestId) !=
                    "active")
                {
                    diagnostics.Add(
                        "generated_relationship.next_quest_start_failed");
                    break;
                }
                session = started.Session;
            }
            else
            {
                var final = FindCompletedChoice(package,
                    relationship.DialogueId);
                if (final is null || !available.Contains(final.Id))
                {
                    diagnostics.Add(
                        "generated_relationship.completed_followup_missing");
                    break;
                }
                var finalBefore = StateHash(reopened.Session);
                var observed = runtime.ExecuteGameplayCommand(package,
                    reopened.Session,
                    GameRuntimeCommand.ChooseDialogueOption(final.Id));
                AddFrame(frames, replayIndex, relationship,
                    GeneratedCampaignRelationshipBranch.SUPPORT,
                    step.Order, step.QuestId, string.Empty,
                    GameRuntimeCommandType.ChooseDialogueOption,
                    finalBefore, observed);
                if (!observed.Success)
                {
                    diagnostics.Add(
                        "generated_relationship.completed_followup_failed");
                    break;
                }
                session = observed.Session;
            }
        }

        var allCompleted = relationship.QuestArc.All(item =>
            QuestState(session, item.QuestId) == "completed");
        if (!allCompleted)
            diagnostics.Add(
                "generated_relationship.arc_progression_incomplete");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new SupportRoute(
            diagnostics.Count == 0,
            alternativesLocked,
            StateHash(session),
            frames,
            diagnostics);
    }

    private ChallengeRoute ExecuteChallenge(
        GamePackageDefinition package,
        GeneratedCampaignRelationshipBinding relationship,
        GameProjectGeneratedEncounterCombatSummary combatSummary,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignExactCombatRouteGoal goal)
    {
        if (string.IsNullOrWhiteSpace(
                relationship.ChallengeEncounterId))
            return ChallengeRoute.Failed(
                "generated_relationship.challenge_encounter_missing");
        var started = runtime.Start(package);
        var opened = started.Success
            ? runtime.ExecuteGameplayCommand(package, started.Session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId))
            : started;
        var choice = FindChoice(package, relationship.DialogueId,
            GeneratedCampaignRelationshipBranch.CHALLENGE, "initial");
        if (!opened.Success || choice is null)
            return ChallengeRoute.Failed(
                "generated_relationship.challenge_choice_missing");
        var before = StateHash(opened.Session);
        var chosen = runtime.ExecuteGameplayCommand(package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(choice.Id));
        var frames = new List<GeneratedCampaignRelationshipRuntimeFrame>();
        AddFrame(frames, 1, relationship,
            GeneratedCampaignRelationshipBranch.CHALLENGE, -1,
            string.Empty, relationship.ChallengeEncounterId,
            GameRuntimeCommandType.ChooseDialogueOption, before, chosen);
        if (!chosen.Success
            || Flag(chosen.Session, relationship.DecisionFlagId) !=
            "CHALLENGE"
            || chosen.Session.GameplayState.ActiveEncounter is not
            { Active: true } encounter
            || encounter.EncounterId !=
            relationship.ChallengeEncounterId)
            return ChallengeRoute.Failed(
                "generated_relationship.challenge_initial_invalid",
                frames);
        var route = _combatRoute.Execute(
            new GeneratedCampaignExactCombatRouteRequest
            {
                FinalPackage = package,
                EncounterId = relationship.ChallengeEncounterId,
                CombatSummary = combatSummary,
                Runtime = runtime,
                InitialSession = chosen.Session,
                Goal = goal
            });
        frames.AddRange(route.Commands.Select((command, index) =>
            new GeneratedCampaignRelationshipRuntimeFrame
            {
                ReplayIndex = 1,
                RelationshipId = relationship.RelationshipId,
                Branch =
                    GeneratedCampaignRelationshipBranch.CHALLENGE,
                ArcStep = -1,
                EncounterId = relationship.ChallengeEncounterId,
                CommandType = command.ToString(),
                AfterStateHash = StateHash(route.Session),
                CommandSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(command),
                EventSha256 =
                    GeneratedCampaignChoiceCanonical.Hash(route.Events),
                Passed = route.Passed
            }));
        var reopened = route.Passed
            ? runtime.ExecuteGameplayCommand(package, route.Session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId))
            : new UnifiedRuntimeResult
            {
                Success = false,
                Session = route.Session
            };
        var followUp = FindChoice(package, relationship.DialogueId,
            GeneratedCampaignRelationshipBranch.CHALLENGE, "followup");
        var passed = route.Passed && reopened.Success
                                  && followUp is not null
                                  && AvailableChoiceIds(reopened)
                                      .Contains(followUp.Id);
        var finalSession = route.Session;
        if (passed
            && goal == GeneratedCampaignExactCombatRouteGoal.VICTORY)
        {
            var followUpBefore = StateHash(reopened.Session);
            var chosenFollowUp = runtime.ExecuteGameplayCommand(package,
                reopened.Session,
                GameRuntimeCommand.ChooseDialogueOption(followUp!.Id));
            AddFrame(frames, 1, relationship,
                GeneratedCampaignRelationshipBranch.CHALLENGE, -1,
                string.Empty, relationship.ChallengeEncounterId,
                GameRuntimeCommandType.ChooseDialogueOption,
                followUpBefore, chosenFollowUp);
            passed = chosenFollowUp.Success
                     && Flag(chosenFollowUp.Session,
                         relationship.RelationshipId
                         + "/challenge-victory") == "VICTORY";
            finalSession = chosenFollowUp.Session;
        }
        var diagnostics = route.Diagnostics.ToList();
        if (!passed)
            diagnostics.Add(goal ==
                            GeneratedCampaignExactCombatRouteGoal.FLEE
                ? "generated_relationship.challenge_flee_failed"
                : "generated_relationship.challenge_victory_failed");
        return new ChallengeRoute(
            passed,
            goal == GeneratedCampaignExactCombatRouteGoal.VICTORY
            && combatSummary.QualifiedActions.Count > 0,
            StateHash(finalSession),
            frames,
            diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal).ToList());
    }

    private static RefuseRoute ExecuteRefuse(
        GamePackageDefinition package,
        GeneratedCampaignRelationshipBinding relationship,
        IUnifiedGameRuntimeService runtime)
    {
        var start = runtime.Start(package);
        var opened = start.Success
            ? runtime.ExecuteGameplayCommand(package, start.Session,
                GameRuntimeCommand.OpenDialogue(
                    relationship.DialogueId))
            : start;
        var choice = FindChoice(package, relationship.DialogueId,
            GeneratedCampaignRelationshipBranch.REFUSE, "initial");
        if (!opened.Success || choice is null)
            return RefuseRoute.Failed(
                "generated_relationship.refuse_choice_missing");
        var alternativesLocked = AvailableChoiceIds(opened)
            .Contains(choice.Id);
        var reputationBefore = Reputation(opened.Session,
            relationship.FactionId);
        var questBefore =
            GeneratedCampaignChoiceCanonical.Hash(new
            {
                opened.Session.GameplayState.QuestStates,
                opened.Session.GameplayState.Quests
            });
        var encounterBefore =
            GeneratedCampaignChoiceCanonical.Hash(
                opened.Session.GameplayState.ActiveEncounter);
        var before = StateHash(opened.Session);
        var chosen = runtime.ExecuteGameplayCommand(package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(choice.Id));
        var frames = new List<GeneratedCampaignRelationshipRuntimeFrame>();
        AddFrame(frames, 1, relationship,
            GeneratedCampaignRelationshipBranch.REFUSE, -1,
            string.Empty, string.Empty,
            GameRuntimeCommandType.ChooseDialogueOption, before, chosen);
        var passed = chosen.Success
                     && Flag(chosen.Session,
                         relationship.DecisionFlagId) == "REFUSE"
                     && Exact(
                         Reputation(chosen.Session,
                             relationship.FactionId)
                         - reputationBefore,
                         relationship.RefuseReputationAmount)
                     && questBefore ==
                     GeneratedCampaignChoiceCanonical.Hash(new
                     {
                         chosen.Session.GameplayState.QuestStates,
                         chosen.Session.GameplayState.Quests
                     })
                     && encounterBefore ==
                     GeneratedCampaignChoiceCanonical.Hash(
                         chosen.Session.GameplayState.ActiveEncounter);
        return passed
            ? new RefuseRoute(true, alternativesLocked,
                StateHash(chosen.Session), frames, [])
            : RefuseRoute.Failed(
                "generated_relationship.refuse_state_invalid", frames,
                alternativesLocked);
    }

    private bool QualifyAtomicFailure(
        GamePackageDefinition package,
        GeneratedCampaignRelationshipBinding relationship,
        string encounterId,
        GameProjectGeneratedEncounterCombatSummary combatSummary,
        IUnifiedGameRuntimeService runtime)
    {
        var start = runtime.Start(package);
        if (!start.Success) return false;
        var before = StateHash(start.Session);
        var invalid = combatSummary with
        {
            QualifiedActions = combatSummary.QualifiedActions.Select(
                    (item, index) => index == 0
                        ? item with
                        {
                            AbilityDefinitionSha256 =
                            item.ActionKind ==
                            GeneratedEncounterCombatQualifiedActionKind
                                .PACKAGE_ABILITY
                                ? "changed"
                                : item.AbilityDefinitionSha256,
                            ObservedEffect = item.ObservedEffect with
                            {
                                Fingerprint = "changed"
                            }
                        }
                        : item)
                .ToList()
        };
        invalid = invalid with
        {
            QualifiedActionsSha256 =
                GeneratedEncounterCombatCanonical.Hash(
                    invalid.QualifiedActions)
        };
        var result = _combatRoute.Execute(
            new GeneratedCampaignExactCombatRouteRequest
            {
                FinalPackage = package,
                EncounterId = encounterId,
                CombatSummary = invalid,
                Runtime = runtime,
                InitialSession = start.Session,
                Goal =
                    GeneratedCampaignExactCombatRouteGoal.VICTORY
            });
        return !result.Passed
               && before == StateHash(start.Session)
               && result.Diagnostics.Contains(
                   "generated_relationship.qualified_action_definition_changed",
                   StringComparer.Ordinal)
               && result.PackageReferenceUnchanged;
    }

    private static bool CatalogExact(
        GameProjectGeneratedEncounterCombatSummary summary)
    {
        var actions = summary.QualifiedActions
            .OrderBy(item => item.ActionKind)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256,
                StringComparer.Ordinal)
            .ThenBy(item => item.ObservedEffect.Fingerprint,
                StringComparer.Ordinal)
            .ToList();
        return summary.Passed && summary.Status == "CAMPAIGN_CURRENT"
                              && actions.Count > 0
                              && summary.QualifiedActionCount ==
                              actions.Count
                              && summary.QualifiedActionsSha256 ==
                              GeneratedEncounterCombatCanonical.Hash(
                                  actions);
    }

    private static DialogueChoiceDefinition? FindChoice(
        GamePackageDefinition package,
        string dialogueId,
        GeneratedCampaignRelationshipBranch branch,
        string phase)
    {
        var dialogue = package.Game.Dialogues.SingleOrDefault(item =>
            item.Id == dialogueId);
        var choices = dialogue?.Nodes.SelectMany(item => item.Choices)
            ?? [];
        return choices.SingleOrDefault(item =>
            item.Metadata.GetValueOrDefault("generatedChoiceKind")
            == branch.ToString()
            && (phase == "initial"
                ? item.Metadata.GetValueOrDefault("generatedChoicePhase")
                  == "initial"
                : (item.Metadata.GetValueOrDefault(
                        "generatedChoicePhase") ?? string.Empty)
                    .StartsWith("followup",
                        StringComparison.Ordinal)));
    }

    private static DialogueChoiceDefinition? FindNextChoice(
        GamePackageDefinition package,
        string dialogueId,
        string nextQuestId) =>
        package.Game.Dialogues.Single(item => item.Id == dialogueId)
            .Nodes.SelectMany(item => item.Choices)
            .SingleOrDefault(item => item.StartQuestId == nextQuestId
                                     && (item.Metadata.GetValueOrDefault(
                                             "generatedRelationshipPhase")
                                         ?? string.Empty)
                                     .StartsWith("followup/next/",
                                         StringComparison.Ordinal));

    private static DialogueChoiceDefinition? FindCompletedChoice(
        GamePackageDefinition package,
        string dialogueId) =>
        package.Game.Dialogues.Single(item => item.Id == dialogueId)
            .Nodes.SelectMany(item => item.Choices)
            .SingleOrDefault(item =>
                item.Metadata.GetValueOrDefault(
                    "generatedRelationshipPhase")
                == "followup/completed");

    private static HashSet<string> AvailableChoiceIds(
        UnifiedRuntimeResult result)
    {
        var value = result.GameplayEvents.LastOrDefault(item =>
                item.Type is GameRuntimeEventType.DialogueOpened
                    or GameRuntimeEventType.DialogueNodeChanged)
            ?.Args.GetValueOrDefault("choiceIds");
        return value?.Split(',',
                   StringSplitOptions.RemoveEmptyEntries
                   | StringSplitOptions.TrimEntries)
                   .ToHashSet(StringComparer.Ordinal)
               ?? [];
    }

    private static void AddFrame(
        ICollection<GeneratedCampaignRelationshipRuntimeFrame> frames,
        int replayIndex,
        GeneratedCampaignRelationshipBinding relationship,
        GeneratedCampaignRelationshipBranch branch,
        int step,
        string questId,
        string encounterId,
        GameRuntimeCommandType command,
        string before,
        UnifiedRuntimeResult result)
    {
        frames.Add(new GeneratedCampaignRelationshipRuntimeFrame
        {
            ReplayIndex = replayIndex,
            RelationshipId = relationship.RelationshipId,
            Branch = branch,
            ArcStep = step,
            QuestId = questId,
            EncounterId = encounterId,
            CommandType = command.ToString(),
            BeforeStateHash = before,
            AfterStateHash = StateHash(result.Session),
            CommandSha256 =
                GeneratedCampaignChoiceCanonical.Hash(command),
            EventSha256 =
                GeneratedCampaignChoiceCanonical.Hash(
                    result.GameplayEvents),
            Passed = result.Success
        });
    }

    private static bool Equivalent(
        SupportRoute left,
        SupportRoute right) =>
        left.Passed && right.Passed
                    && left.FinalStateHash == right.FinalStateHash
                    && left.Frames.Select(FrameSignature)
                        .SequenceEqual(
                            right.Frames.Select(FrameSignature),
                            StringComparer.Ordinal);

    private static string FrameSignature(
        GeneratedCampaignRelationshipRuntimeFrame frame) =>
        string.Join("|", frame.RelationshipId, frame.Branch,
            frame.ArcStep, frame.QuestId, frame.EncounterId,
            frame.CommandType, frame.BeforeStateHash,
            frame.AfterStateHash, frame.CommandSha256,
            frame.EventSha256, frame.Passed);

    private static string Flag(UnifiedRuntimeSession session, string id) =>
        session.GameplayState.Flags.SingleOrDefault(item =>
            item.Id == id)?.Value ?? string.Empty;

    private static double Reputation(
        UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Factions.SingleOrDefault(item =>
            item.FactionId == id)?.Reputation ?? 0;

    private static string QuestState(
        UnifiedRuntimeSession session,
        string id) =>
        session.GameplayState.Quests.SingleOrDefault(item =>
            item.QuestId == id)?.State ?? "not_started";

    private static string StateHash(UnifiedRuntimeSession session) =>
        GeneratedCampaignChoiceCanonical.Hash(
            session.GameplayState);

    private static string PackageSha256(GamePackageDefinition package) =>
        GeneratedEncounterCombatCanonical.HashText(
            GeneratedEncounterCombatCanonical.Serialize(package)
            + Environment.NewLine);

    private static bool Exact(double left, double right) =>
        Math.Abs(left - right) < 0.0000001;

    private static GameProjectGeneratedCampaignRelationshipSummary Invalid(
        GeneratedCampaignRelationshipOverlayDocument overlay,
        IReadOnlyList<string> diagnostics) => new()
    {
        Present = overlay.RelationshipCount > 0,
        Status = overlay.RelationshipCount > 0 ? "INVALID" : "ABSENT",
        RelationshipCount = overlay.RelationshipCount,
        ArcQuestCount = overlay.ArcQuestCount,
        Overlay = overlay,
        Diagnostics = diagnostics
    };

    private sealed record SupportRoute(
        bool Passed,
        bool AlternativesLocked,
        string FinalStateHash,
        IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame> Frames,
        IReadOnlyList<string> Diagnostics)
    {
        public static SupportRoute Failed(
            string diagnostic,
            IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame>? frames =
                null,
            bool alternativesLocked = false) =>
            new(false, alternativesLocked, string.Empty,
                frames ?? [], [diagnostic]);
    }

    private sealed record ChallengeRoute(
        bool Passed,
        bool RecoveryCompatible,
        string FinalStateHash,
        IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame> Frames,
        IReadOnlyList<string> Diagnostics)
    {
        public static ChallengeRoute Failed(
            string diagnostic,
                IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame>? frames =
                null) =>
            new(false, false, string.Empty, frames ?? [], [diagnostic]);
    }

    private sealed record RefuseRoute(
        bool Passed,
        bool AlternativesLocked,
        string FinalStateHash,
        IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame> Frames,
        IReadOnlyList<string> Diagnostics)
    {
        public static RefuseRoute Failed(
            string diagnostic,
            IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame>? frames =
                null,
            bool alternativesLocked = false) =>
            new(false, alternativesLocked, string.Empty, frames ?? [],
                [diagnostic]);
    }
}
