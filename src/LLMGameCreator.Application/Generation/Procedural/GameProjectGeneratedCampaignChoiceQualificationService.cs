using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectGeneratedCampaignChoiceQualificationService
{
    public GameProjectGeneratedCampaignChoiceSummary Qualify(
        GamePackageDefinition finalPackage,
        GeneratedCampaignChoiceOverlayDocument overlay,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(finalPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(runtime);
        if (!overlay.Passed)
            return Invalid(overlay, overlay.Diagnostics.Count == 0
                ? ["generated_choice.overlay_invalid"]
                : overlay.Diagnostics);

        var packageBefore = PackageSha256(finalPackage, overlay);
        var frames = new List<GeneratedCampaignChoiceRuntimeFrame>();
        var diagnostics = new List<string>();
        var branchable = overlay.Bindings.Where(item => item.Branches.Count > 0)
            .OrderBy(item => item.DialogueId, StringComparer.Ordinal).ToList();
        var supportPassed = true;
        var challengeFleePassed = true;
        var challengeVictoryPassed = true;
        var refusePassed = true;

        foreach (var binding in branchable)
        {
            ValidateInitialChoices(finalPackage, runtime, binding, diagnostics);
            foreach (var branch in binding.Branches.OrderBy(item => item.Kind))
            {
                var first = ExecuteInitial(finalPackage, runtime, binding, branch, replayIndex: 1);
                var second = ExecuteInitial(finalPackage, runtime, binding, branch, replayIndex: 2);
                frames.Add(first.Frame);
                frames.Add(second.Frame);
                if (!first.Passed || !second.Passed)
                    diagnostics.Add("generated_choice.branch_runtime_failed:" + branch.Kind + ":"
                                    + first.Result.Success + ":" + first.Frame.FlagValue + ":"
                                    + first.Frame.ReputationDelta.ToString(System.Globalization.CultureInfo.InvariantCulture)
                                    + ":" + first.Frame.QuestState + ":" + first.Frame.EncounterState);
                if (!ReplayEqual(first.Frame, second.Frame))
                    diagnostics.Add("generated_choice.replay_mismatch:" + branch.Kind);

                switch (branch.Kind)
                {
                    case GeneratedCampaignBranchKind.SUPPORT:
                    {
                        var result = QualifySupport(finalPackage, runtime, binding, branch, diagnostics);
                        supportPassed &= result;
                        break;
                    }
                    case GeneratedCampaignBranchKind.CHALLENGE:
                    {
                        var flee = QualifyChallengeFlee(finalPackage, runtime, binding, branch, diagnostics);
                        var victory = QualifyChallengeVictory(finalPackage, runtime, binding, branch, diagnostics);
                        challengeFleePassed &= flee;
                        challengeVictoryPassed &= victory;
                        break;
                    }
                    case GeneratedCampaignBranchKind.REFUSE:
                    {
                        var result = QualifyRefuse(finalPackage, runtime, binding, branch, diagnostics);
                        refusePassed &= result;
                        break;
                    }
                }
            }
        }

        var rollback = QualifyAtomicRollback(finalPackage, runtime, branchable, diagnostics);
        var rollbackPassed = rollback.Passed;
        var packageUnchanged = packageBefore == PackageSha256(finalPackage, overlay);
        if (!packageUnchanged) diagnostics.Add("generated_choice.package_mutated_during_runtime");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();

        var expectedFrameCount = branchable.Sum(item => item.Branches.Count) * 2;
        var replayPassed = frames.GroupBy(item => (item.DialogueId, item.BranchKind))
            .All(group => group.Count() == 2 && ReplayEqual(group.ElementAt(0), group.ElementAt(1)));
        var exclusivePassed = frames.All(item => item.Passed && item.AlternativesLocked);
        var followUpPassed = supportPassed && challengeFleePassed && challengeVictoryPassed && refusePassed;
        var passedAll = diagnostics.Count == 0
                        && frames.Count == expectedFrameCount
                        && replayPassed
                        && exclusivePassed
                        && followUpPassed
                        && rollbackPassed
                        && packageUnchanged;

        return new GameProjectGeneratedCampaignChoiceSummary
        {
            Present = true,
            Passed = passedAll,
            Status = passedAll ? "CHOICE_CURRENT" : "INVALID",
            OverlaySchemaVersion = overlay.SchemaVersion,
            SourcePackageSha256 = overlay.SourcePackageSha256,
            ChoiceOverlayPackageSha256 = overlay.OutputPackageSha256,
            FinalPackageSha256 = packageBefore,
            GeneratedDialogueCount = overlay.GeneratedDialogueCount,
            BranchableDialogueCount = overlay.BranchableDialogueCount,
            QualifiedDialogueCount = frames.Where(item => item.Passed).Select(item => item.DialogueId)
                .Distinct(StringComparer.Ordinal).Count(),
            SupportBranchCount = Count(overlay, GeneratedCampaignBranchKind.SUPPORT),
            ChallengeBranchCount = Count(overlay, GeneratedCampaignBranchKind.CHALLENGE),
            RefuseBranchCount = Count(overlay, GeneratedCampaignBranchKind.REFUSE),
            BranchFlagIds = branchable.Select(item => item.DialogueId).ToList(),
            BranchFlagInventorySha256 = GeneratedCampaignChoiceCanonical.Hash(overlay.FlagInventory),
            ChoiceOverlaySha256 = GeneratedCampaignChoiceCanonical.Hash(overlay),
            RuntimeQualificationPassed = passedAll,
            ExclusiveBranchingPassed = exclusivePassed,
            FollowUpPassed = followUpPassed,
            ChallengeFleeFollowUpPassed = challengeFleePassed,
            ChallengeVictoryFollowUpPassed = challengeVictoryPassed,
            AtomicRollbackPassed = rollbackPassed,
            RollbackBeforeStateHash = rollback.BeforeStateHash,
            RollbackAfterStateHash = rollback.AfterStateHash,
            RollbackPackageBeforeSha256 = rollback.PackageBeforeSha256,
            RollbackPackageAfterSha256 = rollback.PackageAfterSha256,
            RollbackEventTypes = rollback.EventTypes,
            ReplayPassed = replayPassed,
            FinalStateHash = GeneratedCampaignChoiceCanonical.Hash(frames),
            RuntimeFrames = frames,
            HumanReviewFacts =
            [
                new GeneratedCampaignChoiceHumanFact
                {
                    Label = "Сюжетные решения",
                    Value = branchable.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
                },
                new GeneratedCampaignChoiceHumanFact
                {
                    Label = "Взаимоисключающие ветви",
                    Value = exclusivePassed ? "подтверждены Runtime" : "не подтверждены"
                },
                new GeneratedCampaignChoiceHumanFact
                {
                    Label = "Откат ошибочного выбора",
                    Value = rollbackPassed ? "подтверждён Runtime" : "не подтверждён"
                },
                new GeneratedCampaignChoiceHumanFact
                {
                    Label = "Постоянные флаги решений",
                    Value = overlay.FlagInventory.Count.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                }
            ],
            TechnicalDetails = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["runtimePackageSha256"] = packageBefore,
                ["branchRuntimeFrameCount"] = frames.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["replayCountPerBranch"] = "2",
                ["supportRuntimePassed"] = supportPassed.ToString(),
                ["challengeFleeRuntimePassed"] = challengeFleePassed.ToString(),
                ["challengeVictoryRuntimePassed"] = challengeVictoryPassed.ToString(),
                ["refuseRuntimePassed"] = refusePassed.ToString(),
                ["atomicRollbackPassed"] = rollbackPassed.ToString()
            },
            Overlay = overlay,
            Diagnostics = diagnostics
        };
    }

    private static void ValidateInitialChoices(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        ICollection<string> diagnostics)
    {
        var started = runtime.Start(package);
        if (!started.Success)
        {
            diagnostics.Add("generated_choice.runtime_start_failed");
            return;
        }
        var opened = runtime.ExecuteGameplayCommand(package, started.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        if (!opened.Success)
        {
            diagnostics.Add("generated_choice.dialogue_open_failed");
            return;
        }
        var available = AvailableChoiceIds(opened);
        var expected = binding.Branches.Select(item => item.ChoiceId).OrderBy(item => item, StringComparer.Ordinal).ToList();
        if (!available.SequenceEqual(expected))
            diagnostics.Add("generated_choice.initial_choice_ids_mismatch");
    }

    private static BranchExecution ExecuteInitial(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        GeneratedCampaignChoiceBranch branch,
        int replayIndex)
    {
        var started = runtime.Start(package);
        if (!started.Success) return BranchExecution.Failed(binding, branch, replayIndex);
        var opened = runtime.ExecuteGameplayCommand(package, started.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        if (!opened.Success) return BranchExecution.Failed(binding, branch, replayIndex);

        var before = GeneratedCampaignChoiceCanonical.Hash(opened.Session.GameplayState);
        var reputationBefore = Reputation(opened.Session, branch.FactionId);
        var questBefore = QuestState(opened.Session, branch.QuestId);
        var encounterBefore = EncounterState(opened.Session);
        var chosen = runtime.ExecuteGameplayCommand(package, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(branch.ChoiceId));
        var flag = Flag(chosen.Session, binding.DialogueId);
        var reputationDelta = Reputation(chosen.Session, branch.FactionId) - reputationBefore;
        var questState = QuestState(chosen.Session, branch.QuestId);
        var encounterState = EncounterState(chosen.Session);
        var locked = InitialAlternativesRequireEmptyFlag(package, binding) && flag == branch.FlagValue;
        var passed = chosen.Success
                     && flag == branch.FlagValue
                     && locked
                     && chosen.Session.GameplayState.ActiveDialogue is { Open: false };
        passed &= branch.Kind switch
        {
            GeneratedCampaignBranchKind.SUPPORT => Exact(reputationDelta, branch.ReputationAmount)
                                                    && branch.ReputationAmount > 0
                                                    && questState == "active"
                                                    && encounterState == encounterBefore,
            GeneratedCampaignBranchKind.CHALLENGE => chosen.Session.GameplayState.ActiveEncounter is
                { Active: true } active && active.EncounterId == branch.EncounterId,
            GeneratedCampaignBranchKind.REFUSE => Exact(reputationDelta, branch.ReputationAmount)
                                                   && branch.ReputationAmount < 0
                                                   && questState == questBefore
                                                   && encounterState == encounterBefore,
            _ => false
        };
        var eventNames = chosen.GameplayEvents.Select(item => item.Type.ToString()).ToList();
        var commands = new[]
        {
            GameRuntimeCommandType.OpenDialogue.ToString(),
            GameRuntimeCommandType.ChooseDialogueOption.ToString()
        };
        return new BranchExecution(chosen, new GeneratedCampaignChoiceRuntimeFrame
        {
            ReplayIndex = replayIndex,
            DialogueId = binding.DialogueId,
            BranchKind = branch.Kind,
            BeforeStateHash = before,
            StateHash = GeneratedCampaignChoiceCanonical.Hash(chosen.Session.GameplayState),
            CommandSha256 = GeneratedCampaignChoiceCanonical.Hash(commands),
            EventSha256 = GeneratedCampaignChoiceCanonical.Hash(chosen.GameplayEvents),
            Commands = commands,
            Events = eventNames,
            FlagValue = flag,
            ReputationBefore = reputationBefore,
            ReputationAfter = Reputation(chosen.Session, branch.FactionId),
            ReputationDelta = reputationDelta,
            QuestStateBefore = questBefore,
            QuestState = questState,
            EncounterStateBefore = encounterBefore,
            EncounterState = encounterState,
            AlternativesLocked = locked,
            FollowUpPassed = false,
            Passed = passed
        }, passed);
    }

    private static bool QualifySupport(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        GeneratedCampaignChoiceBranch branch,
        ICollection<string> diagnostics)
    {
        var initial = ExecuteInitial(package, runtime, binding, branch, 0);
        if (!initial.Passed || branch.QuestId is not { Length: > 0 } questId)
        {
            diagnostics.Add("generated_choice.support_initial_invalid:" + initial.Result.Success + ":"
                            + initial.Frame.FlagValue + ":"
                            + initial.Frame.ReputationDelta.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + ":" + initial.Frame.QuestState + ":" + initial.Frame.EncounterState);
            return false;
        }
        var active = Reopen(package, runtime, initial.Result.Session, binding.DialogueId);
        if (!OnlyFollowUp(active, branch.ChoiceId + "/followup/active"))
        {
            diagnostics.Add("generated_choice.support_active_followup_missing");
            return false;
        }
        var closed = runtime.ExecuteGameplayCommand(package, active.Session,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.CloseDialogue });
        if (!closed.Success)
        {
            diagnostics.Add("generated_choice.support_followup_close_failed");
            return false;
        }
        var quest = package.Game.Quests.SingleOrDefault(item => item.Id == questId);
        var encounterId = quest?.Objectives.SingleOrDefault(item => item.Kind == "complete_encounter")?.TargetId;
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            diagnostics.Add("generated_choice.support_quest_encounter_missing");
            return false;
        }
        var started = runtime.ExecuteGameplayCommand(package, closed.Session,
            GameRuntimeCommand.StartEncounter(encounterId));
        UnifiedRuntimeSession? wonSession = null;
        var won = started.Success && WinEncounter(package, runtime, started.Session, out wonSession);
        if (!won)
        {
            diagnostics.Add("generated_choice.support_combat_failed");
            return false;
        }
        var completed = runtime.ExecuteGameplayCommand(package, wonSession!,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = questId });
        if (!completed.Success || QuestState(completed.Session, questId) != "completed")
        {
            diagnostics.Add("generated_choice.support_manual_turn_in_failed");
            return false;
        }
        var followUp = Reopen(package, runtime, completed.Session, binding.DialogueId);
        if (!OnlyFollowUp(followUp, branch.ChoiceId + "/followup/completed"))
        {
            diagnostics.Add("generated_choice.support_completed_followup_missing");
            return false;
        }
        return true;
    }

    private static bool QualifyChallengeFlee(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        GeneratedCampaignChoiceBranch branch,
        ICollection<string> diagnostics)
    {
        var initial = ExecuteInitial(package, runtime, binding, branch, 0);
        if (!initial.Passed || initial.Result.Session.GameplayState.ActiveEncounter is not { Active: true })
        {
            diagnostics.Add("generated_choice.challenge_initial_invalid");
            return false;
        }
        var fled = runtime.ExecuteGameplayCommand(package, initial.Result.Session,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
        var followUp = fled.Success ? Reopen(package, runtime, fled.Session, binding.DialogueId) : fled;
        if (!OnlyFollowUp(followUp, branch.ChoiceId + "/followup/chosen"))
        {
            diagnostics.Add("generated_choice.challenge_flee_followup_missing");
            return false;
        }
        return true;
    }

    private static bool QualifyChallengeVictory(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        GeneratedCampaignChoiceBranch branch,
        ICollection<string> diagnostics)
    {
        var initial = ExecuteInitial(package, runtime, binding, branch, 0);
        if (!initial.Passed || !WinEncounter(package, runtime, initial.Result.Session, out var wonSession))
        {
            diagnostics.Add("generated_choice.challenge_victory_failed:"
                            + (initial.Result.Session.GameplayState.ActiveEncounter?.Active.ToString() ?? "null") + ":"
                            + (initial.Result.Session.GameplayState.ActiveEncounter?.ActionHistory.Count.ToString(
                                System.Globalization.CultureInfo.InvariantCulture) ?? "0"));
            return false;
        }
        var followUp = Reopen(package, runtime, wonSession!, binding.DialogueId);
        if (!OnlyFollowUp(followUp, branch.ChoiceId + "/followup/chosen"))
        {
            diagnostics.Add("generated_choice.challenge_victory_followup_missing");
            return false;
        }
        return true;
    }

    private static bool QualifyRefuse(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        GeneratedCampaignChoiceBinding binding,
        GeneratedCampaignChoiceBranch branch,
        ICollection<string> diagnostics)
    {
        var started = runtime.Start(package);
        if (!started.Success) return false;
        var stateBefore = GeneratedCampaignChoiceCanonical.Clone(started.Session.GameplayState);
        var opened = runtime.ExecuteGameplayCommand(package, started.Session,
            GameRuntimeCommand.OpenDialogue(binding.DialogueId));
        var chosen = opened.Success
            ? runtime.ExecuteGameplayCommand(package, opened.Session,
                GameRuntimeCommand.ChooseDialogueOption(branch.ChoiceId))
            : opened;
        var expectedQuest = QuestState(new UnifiedRuntimeSession { GameplayState = stateBefore }, branch.QuestId);
        var expectedEncounter = EncounterState(new UnifiedRuntimeSession { GameplayState = stateBefore });
        var exact = chosen.Success
                    && Exact(Reputation(chosen.Session, branch.FactionId)
                             - Reputation(new UnifiedRuntimeSession { GameplayState = stateBefore }, branch.FactionId),
                        branch.ReputationAmount)
                    && branch.ReputationAmount < 0
                    && QuestState(chosen.Session, branch.QuestId) == expectedQuest
                    && EncounterState(chosen.Session) == expectedEncounter;
        var followUp = chosen.Success ? Reopen(package, runtime, chosen.Session, binding.DialogueId) : chosen;
        if (!exact || !OnlyFollowUp(followUp, branch.ChoiceId + "/followup/chosen"))
        {
            diagnostics.Add("generated_choice.refuse_state_or_followup_invalid");
            return false;
        }
        return true;
    }

    private static RollbackProof QualifyAtomicRollback(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        IReadOnlyList<GeneratedCampaignChoiceBinding> bindings,
        ICollection<string> diagnostics)
    {
        var binding = bindings.FirstOrDefault(item => item.Branches.Count > 0);
        var branch = binding?.Branches.FirstOrDefault();
        if (binding is null || branch is null) return new RollbackProof(true, string.Empty,
            string.Empty, string.Empty, string.Empty, []);
        var invalid = GeneratedCampaignChoiceCanonical.Clone(package);
        var choice = invalid.Game.Dialogues.Single(item => item.Id == binding.DialogueId).Nodes
            .Single(item => item.Id == invalid.Game.Dialogues.Single(dialogue => dialogue.Id == binding.DialogueId).StartNodeId)
            .Choices.Single(item => item.Id == branch.ChoiceId);
        choice.StartEncounterId = "generated/missing/rollback-encounter";
        var invalidPackageBefore = GeneratedCampaignChoiceCanonical.Hash(invalid);
        var started = runtime.Start(invalid);
        var opened = started.Success
            ? runtime.ExecuteGameplayCommand(invalid, started.Session, GameRuntimeCommand.OpenDialogue(binding.DialogueId))
            : started;
        if (!opened.Success)
        {
            diagnostics.Add("generated_choice.rollback_setup_failed");
            return new RollbackProof(false, string.Empty, string.Empty,
                invalidPackageBefore, GeneratedCampaignChoiceCanonical.Hash(invalid), []);
        }
        var stateBefore = GeneratedCampaignChoiceCanonical.Hash(opened.Session.GameplayState);
        var failed = runtime.ExecuteGameplayCommand(invalid, opened.Session,
            GameRuntimeCommand.ChooseDialogueOption(branch.ChoiceId));
        var passed = !failed.Success
                     && stateBefore == GeneratedCampaignChoiceCanonical.Hash(failed.Session.GameplayState)
                     && invalidPackageBefore == GeneratedCampaignChoiceCanonical.Hash(invalid)
                     && failed.GameplayEvents.Count == 1
                     && failed.GameplayEvents[0].Type == GameRuntimeEventType.ValidationFailed;
        if (!passed) diagnostics.Add("generated_choice.atomic_rollback_failed");
        return new RollbackProof(
            passed,
            stateBefore,
            GeneratedCampaignChoiceCanonical.Hash(failed.Session.GameplayState),
            invalidPackageBefore,
            GeneratedCampaignChoiceCanonical.Hash(invalid),
            failed.GameplayEvents.Select(item => item.Type.ToString()).ToList());
    }

    private static bool WinEncounter(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession initial,
        out UnifiedRuntimeSession? wonSession)
    {
        var session = initial;
        var commandBound = Math.Max(128, session.GameplayState.ActiveEncounter?.Participants
            .SelectMany(item => item.Resources).Sum(item => Math.Max(0, item.Amount)) * 8 ?? 128);
        for (var index = 0; index < commandBound
             && session.GameplayState.ActiveEncounter is { Active: true } encounter; index++)
        {
            var current = encounter.Participants[Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1)];
            UnifiedRuntimeResult action;
            if (!string.Equals(current.Team, "player", StringComparison.OrdinalIgnoreCase))
            {
                action = runtime.ExecuteGameplayCommand(package, session,
                    new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            }
            else
            {
                var target = encounter.Participants.FirstOrDefault(item => item.Alive
                    && !string.Equals(item.Team, current.Team, StringComparison.OrdinalIgnoreCase));
                if (target is null) break;
                var definition = package.Game.Encounters.Single(item => item.Id == encounter.EncounterId)
                    .Participants.Single(item => item.Id == current.Id);
                var basic = runtime.ExecuteGameplayCommand(package, session,
                    GameRuntimeCommand.BasicAttack(current.Id, target.Id));
                action = basic.Success
                    ? basic
                    : TryAbilities(package, runtime, session, definition, current.Id, target.Id)
                      ?? basic;
            }
            if (!action.Success)
            {
                wonSession = null;
                return false;
            }
            session = action.Session;
        }
        wonSession = session;
        var endedEncounter = session.GameplayState.ActiveEncounter;
        return endedEncounter is { Active: false } && endedEncounter.Participants.Any(item => item.Alive
            && string.Equals(item.Team, "player", StringComparison.OrdinalIgnoreCase));
    }

    private static UnifiedRuntimeResult? TryAbilities(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        EncounterParticipantDefinition participant,
        string sourceId,
        string targetId)
    {
        foreach (var ability in participant.Abilities)
        {
            var candidate = GeneratedCampaignChoiceCanonical.Clone(session);
            var result = runtime.ExecuteGameplayCommand(package, candidate,
                GameRuntimeCommand.UseAbility(ability, sourceId, targetId));
            if (result.Success) return result;
        }
        return null;
    }

    private static bool InitialAlternativesRequireEmptyFlag(
        GamePackageDefinition package,
        GeneratedCampaignChoiceBinding binding)
    {
        var dialogue = package.Game.Dialogues.Single(item => item.Id == binding.DialogueId);
        var start = dialogue.Nodes.Single(item => item.Id == dialogue.StartNodeId);
        return binding.Branches.All(branch => start.Choices.Single(item => item.Id == branch.ChoiceId)
            .Requirements.Any(item => item.Kind == "flag_equals"
                                      && item.Id == binding.DialogueId
                                      && item.Value == string.Empty));
    }

    private static UnifiedRuntimeResult Reopen(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        string dialogueId) => session.GameplayState.ActiveEncounter is { Active: true }
        ? new UnifiedRuntimeResult { Success = false, Session = session }
        : runtime.ExecuteGameplayCommand(package, session, GameRuntimeCommand.OpenDialogue(dialogueId));

    private static bool OnlyFollowUp(UnifiedRuntimeResult result, string expectedChoiceId) =>
        result.Success && AvailableChoiceIds(result).SequenceEqual([expectedChoiceId]);

    private static IReadOnlyList<string> AvailableChoiceIds(UnifiedRuntimeResult result) => result.GameplayEvents
        .Where(item => item.Type is GameRuntimeEventType.DialogueOpened or GameRuntimeEventType.DialogueNodeChanged)
        .SelectMany(item => item.Args.TryGetValue("choiceIds", out var value)
            ? value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [])
        .Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();

    private static string Flag(UnifiedRuntimeSession session, string id) => session.GameplayState.Flags
        .FirstOrDefault(item => item.Id == id)?.Value ?? string.Empty;

    private static double Reputation(UnifiedRuntimeSession session, string id) => session.GameplayState.Factions
        .FirstOrDefault(item => item.FactionId == id)?.Reputation ?? 0;

    private static string QuestState(UnifiedRuntimeSession session, string? id) => string.IsNullOrWhiteSpace(id)
        ? string.Empty
        : session.GameplayState.Quests.FirstOrDefault(item => item.QuestId == id)?.State ?? "not_started";

    private static string EncounterState(UnifiedRuntimeSession session) =>
        GeneratedCampaignChoiceCanonical.Hash(session.GameplayState.ActiveEncounter);

    private static bool Exact(double actual, double expected) => Math.Abs(actual - expected) < 0.0000001;

    private static bool ReplayEqual(GeneratedCampaignChoiceRuntimeFrame left, GeneratedCampaignChoiceRuntimeFrame right) =>
        left.DialogueId == right.DialogueId
        && left.BranchKind == right.BranchKind
        && left.BeforeStateHash == right.BeforeStateHash
        && left.StateHash == right.StateHash
        && left.CommandSha256 == right.CommandSha256
        && left.EventSha256 == right.EventSha256
        && left.FlagValue == right.FlagValue
        && Exact(left.ReputationBefore, right.ReputationBefore)
        && Exact(left.ReputationAfter, right.ReputationAfter)
        && Exact(left.ReputationDelta, right.ReputationDelta)
        && left.QuestStateBefore == right.QuestStateBefore
        && left.QuestState == right.QuestState
        && left.EncounterStateBefore == right.EncounterStateBefore
        && left.EncounterState == right.EncounterState
        && left.AlternativesLocked == right.AlternativesLocked
        && left.Passed == right.Passed;

    private static int Count(GeneratedCampaignChoiceOverlayDocument overlay, GeneratedCampaignBranchKind kind) =>
        overlay.Bindings.Sum(item => item.Branches.Count(branch => branch.Kind == kind));

    private static string PackageSha256(
        GamePackageDefinition package,
        GeneratedCampaignChoiceOverlayDocument overlay)
    {
        var choiceSha = GeneratedCampaignChoiceCanonical.HashText(
            GeneratedCampaignChoiceCanonical.Serialize(package) + Environment.NewLine);
        if (string.Equals(choiceSha, overlay.OutputPackageSha256, StringComparison.Ordinal)) return choiceSha;
        return GeneratedEncounterCombatCanonical.HashText(
            GeneratedEncounterCombatCanonical.Serialize(package) + Environment.NewLine);
    }

    private static GameProjectGeneratedCampaignChoiceSummary Invalid(
        GeneratedCampaignChoiceOverlayDocument overlay,
        IReadOnlyList<string> diagnostics) => new()
        {
            Present = true,
            Status = "INVALID",
            Overlay = overlay,
            Diagnostics = diagnostics
        };

    private sealed record BranchExecution(
        UnifiedRuntimeResult Result,
        GeneratedCampaignChoiceRuntimeFrame Frame,
        bool Passed)
    {
        public static BranchExecution Failed(
            GeneratedCampaignChoiceBinding binding,
            GeneratedCampaignChoiceBranch branch,
            int replayIndex) => new(new UnifiedRuntimeResult { Success = false }, new GeneratedCampaignChoiceRuntimeFrame
            {
                ReplayIndex = replayIndex,
                DialogueId = binding.DialogueId,
                BranchKind = branch.Kind
            }, false);
    }

    private sealed record RollbackProof(
        bool Passed,
        string BeforeStateHash,
        string AfterStateHash,
        string PackageBeforeSha256,
        string PackageAfterSha256,
        IReadOnlyList<string> EventTypes);
}
