using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.Semantics;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticRuntimeCompositionAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedSemanticRuntimeCompositionArtifacts()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("semantic_selected_runtime_composition_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal005GateRecorded);
        Assert.True(first.Report.PackageValidationPassed);
        Assert.True(first.Report.SemanticSelectedIdsExecutedInRuntime);
        Assert.True(first.Report.InvalidScenarioRejected);
        Assert.True(first.Report.DeterministicReplayPassed);
        Assert.True(first.Report.SaveLoadRoundtripPassed);
        Assert.True(first.Report.CrossVariantIsolationPassed);
        Assert.True(first.Report.MultiSeedNoDanglingReferences);
        Assert.False(first.Report.CandidateConflictLeakageDetected);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);

        var valid = first.Report.Scenarios.Where(item => item.ExpectedValid).ToList();
        Assert.Equal(4, valid.Count);
        Assert.All(valid, scenario =>
        {
            Assert.True(scenario.ActualValid);
            Assert.True(scenario.PackageValidationPassed);
            Assert.True(scenario.SemanticSelectedIdsExecutedInRuntime);
            Assert.Equal(scenario.SelectedQuestPatternId, scenario.RuntimeEvidence.ExecutedQuestPatternId);
            Assert.Equal(scenario.SelectedDialogueIntentId, scenario.RuntimeEvidence.ExecutedDialogueIntentId);
            Assert.Equal(scenario.SelectedInteractionPatternId, scenario.RuntimeEvidence.ExecutedInteractionPatternId);
            Assert.Equal(scenario.GeneratedPackageHash, scenario.RuntimeEvidence.PackageHash);
            Assert.NotEmpty(scenario.TraceChain);
            Assert.Equal(scenario.SelectedQuestPatternId, scenario.CompositionPlan.Provenance["selectedQuestPatternId"]);
            Assert.Equal(scenario.SelectedDialogueIntentId, scenario.CompositionPlan.Provenance["selectedDialogueIntentId"]);
            Assert.Equal(scenario.SelectedInteractionPatternId, scenario.CompositionPlan.Provenance["selectedInteractionPatternId"]);
            Assert.All(scenario.CompositionPlan.SelectedQuestObjectives, objective =>
            {
                Assert.DoesNotContain("dialogue/semantic_selected", objective.PackageTargetId);
                Assert.All(objective.RequiredInteractionPatternIds, requiredInteractionId =>
                    Assert.Contains(scenario.CompositionPlan.MaterializedInteractions, interaction => interaction.InteractionPatternId == requiredInteractionId));
                Assert.Contains(scenario.RuntimeEvidence.ObjectiveEvidence, evidence =>
                    evidence.PackageObjectiveId == objective.PackageObjectiveId &&
                    evidence.Completed &&
                    evidence.RuntimeOwnedProgressEvidence);
            });
            Assert.All(scenario.CompositionPlan.MaterializedInteractions, interaction =>
                Assert.Contains(scenario.RuntimeEvidence.Commands, command =>
                    command.CommandType == "gameplay/execute_interaction" &&
                    command.TargetId == interaction.PackageInteractionId &&
                    command.Succeeded));
            Assert.True(scenario.RuntimeEvidence.StateDelta.RewardAmountAfter > scenario.RuntimeEvidence.StateDelta.RewardAmountBefore);
            Assert.Equal("completed", scenario.RuntimeEvidence.StateDelta.CompletionFlagAfter);
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
        });

        var overlay = first.Report.Scenarios.Single(item => item.ScenarioId == "core_genre_project_overlay");
        var wildland = first.Report.Scenarios.Single(item => item.ScenarioId == "core_plus_wildland_frontier");
        Assert.NotEqual(wildland.GeneratedPackageHash, overlay.GeneratedPackageHash);
        Assert.Equal("quest_pattern/two_step_sequence", overlay.SelectedQuestPatternId);
        Assert.Equal("dialogue/completion_response/default", overlay.SelectedDialogueIntentId);
        Assert.Equal("interaction/use_reward_on_contact", overlay.SelectedInteractionPatternId);
        Assert.Equal(
            ["interaction/take_cache_item", "interaction/talk_contact", "interaction/use_reward_on_contact"],
            overlay.CompositionPlan.MaterializedInteractions.Select(item => item.InteractionPatternId).OrderBy(item => item, StringComparer.Ordinal).ToList());
        Assert.All(overlay.CompositionPlan.SelectedQuestObjectives, objective =>
            Assert.True(objective.PackageTargetId is "entity/generated_contact" or "item/generated_cache"));

        var invalid = first.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        Assert.False(invalid.ActualValid);
        Assert.False(invalid.SemanticSelectedIdsExecutedInRuntime);
        Assert.Equal(string.Empty, invalid.GeneratedPackageHash);
        Assert.Contains(invalid.Diagnostics, item => item.Severity == "error" && item.Code == "semantic_guided.excludes_conflict");

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        Assert.Contains("\"accepted\": true", json);
        Assert.Contains("\"semanticSelectedIdsExecutedInRuntime\": true", json);
    }

    [Fact]
    public void RuntimeAdapterUnavailablePreventsFalseRuntimeExecutionClaim()
    {
        using var temp = new TempDirectory();
        var service = new SemanticSelectedRuntimeCompositionAcceptanceService(
            semanticGuidedService: CreateSemanticGuidedService());

        var result = service.Build(temp.Path);

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.SemanticSelectedIdsExecutedInRuntime);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.runtime_adapter_unavailable");
    }

    [Fact]
    public void MaterializedBindingAuditRejectsSuffixlessDialoguePlaceholderBeforeRuntime()
    {
        var service = CreateService(
            adapter: new CountingRuntimeAdapter(new DefaultSemanticRuntimeAdapter()),
            packageMutator: (package, _) =>
            {
                package.Game.Quests[0].Objectives[0].TargetId = "dialogue/semantic_selected";
                return package;
            });

        var result = service.Build();

        Assert.False(result.Report.Accepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.audit.suffixless_dialogue_placeholder");
        Assert.All(result.Report.Scenarios.Where(item => item.ExpectedValid), scenario => Assert.False(scenario.RuntimeEvidence.RuntimeAttempted));
    }

    [Fact]
    public void MissingObjectiveRequiredInteractionPreventsRuntimeExecution()
    {
        var adapter = new CountingRuntimeAdapter(new DefaultSemanticRuntimeAdapter());
        var service = CreateService(
            adapter: adapter,
            packageMutator: (package, _) =>
            {
                package.Game.Interactions.RemoveAll(item => item.Id == "interaction/take_cache_item");
                return package;
            });

        var result = service.Build();

        Assert.False(result.Report.Accepted);
        Assert.All(
            result.Report.Scenarios.Where(item => item.CompositionPlan.SelectedQuestObjectives.Any(objective => objective.RequiredInteractionPatternIds.Contains("interaction/take_cache_item", StringComparer.Ordinal))),
            scenario => Assert.False(scenario.RuntimeEvidence.RuntimeAttempted));
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.audit.required_interaction_missing");
    }

    [Fact]
    public void FakeAdapterSuccessWithoutCommandAndStateProofIsRejected()
    {
        var result = CreateService(adapter: new FakeSuccessRuntimeAdapter()).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.SemanticSelectedIdsExecutedInRuntime);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.required_command_missing");
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.objective_missing");
    }

    [Fact]
    public void FailedRequiredCommandMakesRuntimeAcceptanceFalse()
    {
        var result = CreateService(adapter: new MutatingRuntimeAdapter(evidence =>
            evidence with
            {
                Commands = evidence.Commands.Select((command, index) => index == 0 ? command with { Succeeded = false } : command).ToList()
            })).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.SemanticSelectedIdsExecutedInRuntime);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.required_command_failed");
    }

    [Fact]
    public void AllObjectivesNeedRuntimeOwnedEvidenceAndRewardSaveLoadDeltas()
    {
        var missingObjective = CreateService(adapter: new MutatingRuntimeAdapter(evidence =>
            evidence with { ObjectiveEvidence = evidence.ObjectiveEvidence.Take(1).ToList() })).Build();
        Assert.False(missingObjective.Report.Accepted);
        Assert.Contains(missingObjective.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.objective_missing");

        var noRewardDelta = CreateService(adapter: new MutatingRuntimeAdapter(evidence =>
            evidence with
            {
                StateDelta = evidence.StateDelta with { RewardAmountAfter = evidence.StateDelta.RewardAmountBefore }
            })).Build();
        Assert.False(noRewardDelta.Report.Accepted);
        Assert.Contains(noRewardDelta.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.reward_completion_delta_missing");

        var saveLoadMismatch = CreateService(adapter: new MutatingRuntimeAdapter(evidence =>
            evidence with
            {
                RestoredStateEvidence = Set(evidence.RestoredStateEvidence, "questState", "stale")
            })).Build();
        Assert.False(saveLoadMismatch.Report.Accepted);
        Assert.Contains(saveLoadMismatch.Report.Diagnostics, item => item.Code == "semantic_runtime.evidence.save_load_mismatch");
    }

    [Fact]
    public void InjectedPreviousVariantStateMakesIsolationFail()
    {
        var result = CreateService(adapter: new MutatingRuntimeAdapter((request, evidence) =>
            request.ScenarioId == "core_plus_gothic_mystery"
                ? evidence with { IsolationKeys = evidence.IsolationKeys.Concat(["scenario:core_plus_wildland_frontier"]).OrderBy(item => item, StringComparer.Ordinal).ToList() }
                : evidence)).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.CrossVariantIsolationPassed);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "semantic_runtime.isolation.foreign_state_leak");
    }

    public static SemanticSelectedRuntimeCompositionAcceptanceService CreateService(
        ISemanticSelectedRuntimeCompositionRuntimeAdapter? adapter = null,
        Func<GamePackageDefinition, SemanticSelectedCompositionPlan, GamePackageDefinition>? packageMutator = null)
    {
        return new SemanticSelectedRuntimeCompositionAcceptanceService(
            semanticGuidedService: CreateSemanticGuidedService(),
            runtimeAdapter: adapter ?? new DefaultSemanticRuntimeAdapter(),
            packageMutator: packageMutator);
    }

    private static SemanticGuidedCompositionAcceptanceService CreateSemanticGuidedService()
    {
        var serializer = new RuntimeStateSerializer();
        var goal004 = new QuestDialogInteractionFamilyAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultVisibleRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));

        return new SemanticGuidedCompositionAcceptanceService(goal004Service: goal004);
    }

    private static IReadOnlyDictionary<string, string> Set(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            copy[pair.Key] = pair.Value;
        }

        copy[key] = value;
        return copy;
    }

    private sealed class CountingRuntimeAdapter(ISemanticSelectedRuntimeCompositionRuntimeAdapter inner) : ISemanticSelectedRuntimeCompositionRuntimeAdapter
    {
        public int CallCount { get; private set; }

        public SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request)
        {
            CallCount++;
            return inner.Run(request);
        }
    }

    private sealed class FakeSuccessRuntimeAdapter : ISemanticSelectedRuntimeCompositionRuntimeAdapter
    {
        public SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request) => new()
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            SemanticSelectedIdsExecutedInRuntime = true,
            PackageHash = request.PackageHash,
            ExecutedQuestPatternId = request.Plan.SelectedQuestPatternId,
            ExecutedDialogueIntentId = request.Plan.SelectedDialogueIntentId,
            ExecutedInteractionPatternId = request.Plan.SelectedInteractionPatternId,
            ExecutedPackageQuestId = request.Plan.PackageQuestId,
            ExecutedPackageDialogueId = request.Plan.PackageDialogueId,
            ExecutedPackageInteractionId = request.Plan.PackageInteractionId,
            SaveLoadRoundtripPassed = true
        };
    }

    private sealed class MutatingRuntimeAdapter : ISemanticSelectedRuntimeCompositionRuntimeAdapter
    {
        private readonly Func<SemanticSelectedRuntimeCompositionRuntimeRequest, SemanticSelectedRuntimeCompositionRuntimeEvidence, SemanticSelectedRuntimeCompositionRuntimeEvidence> _mutate;
        private readonly DefaultSemanticRuntimeAdapter _inner = new();

        public MutatingRuntimeAdapter(Func<SemanticSelectedRuntimeCompositionRuntimeEvidence, SemanticSelectedRuntimeCompositionRuntimeEvidence> mutate)
            : this((_, evidence) => mutate(evidence))
        {
        }

        public MutatingRuntimeAdapter(Func<SemanticSelectedRuntimeCompositionRuntimeRequest, SemanticSelectedRuntimeCompositionRuntimeEvidence, SemanticSelectedRuntimeCompositionRuntimeEvidence> mutate)
        {
            _mutate = mutate;
        }

        public SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request)
        {
            return _mutate(request, _inner.Run(request));
        }
    }

    private sealed class DefaultSemanticRuntimeAdapter : ISemanticSelectedRuntimeCompositionRuntimeAdapter
    {
        public SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request)
        {
            var bridge = CreateBridge();
            var serializer = new RuntimeStateSerializer();
            var commands = new List<SemanticSelectedRuntimeCommandEvidence>();
            var eventTypes = new SortedSet<string>(StringComparer.Ordinal);
            var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>();
            var start = bridge.Start(request.Package);
            foreach (var eventType in start.MapEvents.Select(item => "map:" + item.Type).Concat(start.GameplayEvents.Select(item => "gameplay:" + item.Type)))
            {
                eventTypes.Add(eventType);
            }

            var session = start.Session;
            var rewardItemId = "item/generated_reward";
            var completionFlagId = request.Package.Game.Quests
                .First(item => item.Id == request.Plan.PackageQuestId)
                .Rewards.First(item => item.Kind == "flag").Id;
            var rewardBefore = ItemAmount(session.GameplayState, rewardItemId);
            var completionFlagBefore = FlagValue(session.GameplayState, completionFlagId);
            var questStateBefore = QuestState(session.GameplayState, request.Plan.PackageQuestId);
            var mapIdBefore = session.GameplayState.CurrentMapId;
            var encounterIdBefore = session.GameplayState.ActiveEncounter?.EncounterId ?? string.Empty;
            var move = bridge.ExecutePlayerCommand(request.Package, session, PlayerCommand.Move(Direction2D.Right));
            commands.Add(ToCommand("01_move_right", "move/right", request.Plan.PackageMapId, move));
            AddEvents(eventTypes, move);
            var playerInteract = bridge.ExecutePlayerCommand(request.Package, session, PlayerCommand.Interact());
            commands.Add(ToCommand("02_player_interact", "player/interact", request.Plan.PackageInteractionId, playerInteract));
            AddEvents(eventTypes, playerInteract);
            var startQuest = bridge.ExecuteGameplayCommand(request.Package, session, GameRuntimeCommand.StartQuest(request.Plan.PackageQuestId));
            commands.Add(ToCommand("03_start_quest", "gameplay/start_quest", request.Plan.PackageQuestId, startQuest));
            AddEvents(eventTypes, startQuest);

            var correlations = new List<SemanticSelectedRuntimeInteractionObjectiveCorrelation>();
            var commandOrdinal = 4;
            foreach (var objective in request.Plan.SelectedQuestObjectives)
            {
                foreach (var requiredInteractionId in objective.RequiredInteractionPatternIds)
                {
                    var binding = request.Plan.MaterializedInteractions.First(item => item.InteractionPatternId == requiredInteractionId);
                    var commandId = $"{commandOrdinal:00}_execute_{SafeId(binding.PackageInteractionId)}";
                    var executeInteraction = bridge.ExecuteGameplayCommand(
                        request.Package,
                        session,
                        GameRuntimeCommand.ExecuteInteraction(binding.PackageInteractionId, binding.PackageTargetId));
                    commands.Add(ToCommand(commandId, "gameplay/execute_interaction", binding.PackageInteractionId, executeInteraction, binding.InteractionPatternId, objective.PackageObjectiveId));
                    AddEvents(eventTypes, executeInteraction);
                    commandOrdinal++;

                    var advanceCommandId = $"{commandOrdinal:00}_advance_{SafeId(objective.PackageObjectiveId)}";
                    var advance = executeInteraction.Success
                        ? bridge.ExecuteGameplayCommand(
                            request.Package,
                            session,
                            GameRuntimeCommand.AdvanceQuestObjective(request.Plan.PackageQuestId, objective.PackageObjectiveId))
                        : new UnifiedRuntimeResult { Success = false, Session = session, Message = "Required interaction failed before objective advancement." };
                    commands.Add(ToCommand(advanceCommandId, "gameplay/advance_quest_objective", objective.PackageObjectiveId, advance, binding.InteractionPatternId, objective.PackageObjectiveId));
                    AddEvents(eventTypes, advance);
                    commandOrdinal++;

                    correlations.Add(new SemanticSelectedRuntimeInteractionObjectiveCorrelation
                    {
                        PackageObjectiveId = objective.PackageObjectiveId,
                        InteractionPatternId = binding.InteractionPatternId,
                        PackageInteractionId = binding.PackageInteractionId,
                        InteractionCommandId = commandId,
                        InteractionSucceeded = executeInteraction.Success,
                        ObjectiveAdvanceCommandId = advanceCommandId,
                        ObjectiveAdvanceSucceeded = advance.Success
                    });
                }
            }

            var selectedBinding = request.Plan.MaterializedInteractions.First(item => item.InteractionPatternId == request.Plan.SelectedInteractionPatternId);
            if (!request.Plan.SelectedQuestObjectives.Any(objective => objective.RequiredInteractionPatternIds.Contains(selectedBinding.InteractionPatternId, StringComparer.Ordinal)))
            {
                var commandId = $"{commandOrdinal:00}_execute_selected_{SafeId(selectedBinding.PackageInteractionId)}";
                var selectedExecute = bridge.ExecuteGameplayCommand(
                    request.Package,
                    session,
                    GameRuntimeCommand.ExecuteInteraction(selectedBinding.PackageInteractionId, selectedBinding.PackageTargetId));
                commands.Add(ToCommand(commandId, "gameplay/execute_interaction", selectedBinding.PackageInteractionId, selectedExecute, selectedBinding.InteractionPatternId));
                AddEvents(eventTypes, selectedExecute);
                commandOrdinal++;
            }

            var activeDialogueIdAfterOpen = string.Empty;
            var openDialogue = bridge.ExecuteGameplayCommand(request.Package, session, GameRuntimeCommand.OpenDialogue(request.Plan.PackageDialogueId));
            commands.Add(ToCommand($"{commandOrdinal:00}_open_selected_dialogue", "gameplay/open_dialogue", request.Plan.PackageDialogueId, openDialogue));
            AddEvents(eventTypes, openDialogue);
            commandOrdinal++;
            activeDialogueIdAfterOpen = session.GameplayState.ActiveDialogue?.DialogueId ?? string.Empty;
            var choose = bridge.ExecuteGameplayCommand(request.Package, session, GameRuntimeCommand.ChooseDialogueOption("advance"));
            commands.Add(ToCommand($"{commandOrdinal:00}_choose_dialogue_advance", "gameplay/choose_dialogue_option", "advance", choose));
            AddEvents(eventTypes, choose);

            var stateJson = serializer.Serialize(session);
            var restored = serializer.DeserializeUnifiedSession(stateJson);
            var restoredJson = serializer.Serialize(restored);
            var stateHash = ComputeHash(stateJson);
            var restoredHash = ComputeHash(restoredJson);
            var quest = session.GameplayState.Quests.FirstOrDefault(item => item.QuestId == request.Plan.PackageQuestId);
            var restoredQuest = restored.GameplayState.Quests.FirstOrDefault(item => item.QuestId == request.Plan.PackageQuestId);
            var objectiveStates = quest?.Objectives
                .Select(item => item.ObjectiveId + "=" + item.CurrentAmount.ToString("0.####") + "/" + item.RequiredAmount.ToString("0.####") + "/" + item.Completed.ToString().ToLowerInvariant())
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList() ?? [];
            var objectiveEvidence = request.Plan.SelectedQuestObjectives
                .Select(objective =>
                {
                    var state = quest?.Objectives.FirstOrDefault(item => item.ObjectiveId == objective.PackageObjectiveId);
                    return new SemanticSelectedRuntimeObjectiveEvidence
                    {
                        PackageObjectiveId = objective.PackageObjectiveId,
                        TargetId = state?.TargetId ?? string.Empty,
                        BeforeAmount = 0,
                        AfterAmount = state?.CurrentAmount ?? 0,
                        RequiredAmount = state?.RequiredAmount ?? 1,
                        Completed = state?.Completed == true,
                        RuntimeOwnedProgressEvidence = commands.Any(command =>
                            command.CommandType == "gameplay/advance_quest_objective" &&
                            command.TargetId == objective.PackageObjectiveId &&
                            command.Succeeded),
                        RequiredInteractionPatternIds = objective.RequiredInteractionPatternIds,
                        CorrelatedInteractionPatternIds = correlations
                            .Where(correlation => correlation.PackageObjectiveId == objective.PackageObjectiveId && correlation.InteractionSucceeded && correlation.ObjectiveAdvanceSucceeded)
                            .Select(correlation => correlation.InteractionPatternId)
                            .OrderBy(item => item, StringComparer.Ordinal)
                            .ToList()
                    };
                })
                .ToList();
            var rewardAfter = ItemAmount(session.GameplayState, rewardItemId);
            var completionFlagAfter = FlagValue(session.GameplayState, completionFlagId);
            var stateDelta = new SemanticSelectedRuntimeStateDelta
            {
                PackageId = session.GameplayState.PackageId,
                PackageHash = request.PackageHash,
                MapIdBefore = mapIdBefore,
                MapIdAfter = session.GameplayState.CurrentMapId,
                QuestStateBefore = questStateBefore,
                QuestStateAfter = quest?.State ?? string.Empty,
                RewardItemId = rewardItemId,
                RewardAmountBefore = rewardBefore,
                RewardAmountAfter = rewardAfter,
                CompletionFlagId = completionFlagId,
                CompletionFlagBefore = completionFlagBefore,
                CompletionFlagAfter = completionFlagAfter,
                ActiveDialogueIdAfterOpen = activeDialogueIdAfterOpen,
                DialogueOpened = openDialogue.Success,
                DialogueClosedAfterChoice = choose.Success && session.GameplayState.ActiveDialogue?.Open == false,
                EncounterIdBefore = encounterIdBefore,
                EncounterIdAfter = session.GameplayState.ActiveEncounter?.EncounterId ?? string.Empty
            };
            var stateEvidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["scenarioId"] = request.ScenarioId,
                ["packageId"] = session.GameplayState.PackageId,
                ["currentMapId"] = session.GameplayState.CurrentMapId,
                ["questId"] = quest?.QuestId ?? string.Empty,
                ["questState"] = quest?.State ?? string.Empty,
                ["questObjectiveStates"] = string.Join(",", objectiveStates),
                ["activeDialogueId"] = session.GameplayState.ActiveDialogue?.DialogueId ?? string.Empty,
                ["selectedQuestPatternId"] = quest?.Metadata.GetValueOrDefault("selectedQuestPatternId") ?? string.Empty,
                ["selectedDialogueIntentId"] = quest?.Metadata.GetValueOrDefault("selectedDialogueIntentId") ?? string.Empty,
                ["selectedInteractionPatternId"] = quest?.Metadata.GetValueOrDefault("selectedInteractionPatternId") ?? string.Empty
            };
            var restoredObjectiveStates = restoredQuest?.Objectives
                .Select(item => item.ObjectiveId + "=" + item.CurrentAmount.ToString("0.####") + "/" + item.RequiredAmount.ToString("0.####") + "/" + item.Completed.ToString().ToLowerInvariant())
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList() ?? [];
            var restoredEvidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["scenarioId"] = request.ScenarioId,
                ["packageId"] = restored.GameplayState.PackageId,
                ["currentMapId"] = restored.GameplayState.CurrentMapId,
                ["questId"] = restoredQuest?.QuestId ?? string.Empty,
                ["questState"] = restoredQuest?.State ?? string.Empty,
                ["questObjectiveStates"] = string.Join(",", restoredObjectiveStates),
                ["activeDialogueId"] = restored.GameplayState.ActiveDialogue?.DialogueId ?? string.Empty,
                ["selectedQuestPatternId"] = restoredQuest?.Metadata.GetValueOrDefault("selectedQuestPatternId") ?? string.Empty,
                ["selectedDialogueIntentId"] = restoredQuest?.Metadata.GetValueOrDefault("selectedDialogueIntentId") ?? string.Empty,
                ["selectedInteractionPatternId"] = restoredQuest?.Metadata.GetValueOrDefault("selectedInteractionPatternId") ?? string.Empty
            };
            var isolationKeys = new SortedSet<string>(StringComparer.Ordinal)
            {
                "scenario:" + request.ScenarioId,
                "packageId:" + request.Package.Manifest.PackageId,
                "packageHash:" + request.PackageHash,
                "quest:" + request.Plan.PackageQuestId,
                "dialogue:" + request.Plan.PackageDialogueId,
                "selectedQuest:" + request.Plan.SelectedQuestPatternId,
                "selectedDialogue:" + request.Plan.SelectedDialogueIntentId,
                "selectedInteraction:" + request.Plan.SelectedInteractionPatternId
            };
            foreach (var objective in request.Plan.SelectedQuestObjectives)
            {
                isolationKeys.Add("objective:" + objective.PackageObjectiveId);
            }

            foreach (var binding in request.Plan.MaterializedInteractions)
            {
                isolationKeys.Add("interaction:" + binding.PackageInteractionId);
            }

            var selectedExecuted = start.Success &&
                                   commands.Any(item => item.TargetId == request.Plan.PackageInteractionId && item.Succeeded) &&
                                   request.Plan.MaterializedInteractions.All(binding => commands.Any(item => item.TargetId == binding.PackageInteractionId && item.Succeeded)) &&
                                   stateEvidence["selectedQuestPatternId"] == request.Plan.SelectedQuestPatternId &&
                                   stateEvidence["selectedDialogueIntentId"] == request.Plan.SelectedDialogueIntentId &&
                                   stateEvidence["selectedInteractionPatternId"] == request.Plan.SelectedInteractionPatternId &&
                                   quest != null &&
                                   request.Plan.SelectedQuestObjectives.All(objective => quest.Objectives.Any(item => item.ObjectiveId == objective.PackageObjectiveId && item.Completed)) &&
                                   rewardAfter > rewardBefore &&
                                   completionFlagAfter == "completed";

            if (!selectedExecuted)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.selected_ids_not_observed", request.ScenarioId, "Runtime state did not preserve the selected semantic/rule declaration ids."));
            }

            var evidenceWithoutHash = new SemanticSelectedRuntimeCompositionRuntimeEvidence
            {
                RuntimeAttempted = true,
                RuntimeStartSucceeded = start.Success,
                SemanticSelectedIdsExecutedInRuntime = selectedExecuted,
                PackageHash = request.PackageHash,
                ExecutedQuestPatternId = request.Plan.SelectedQuestPatternId,
                ExecutedDialogueIntentId = request.Plan.SelectedDialogueIntentId,
                ExecutedInteractionPatternId = request.Plan.SelectedInteractionPatternId,
                ExecutedPackageQuestId = request.Plan.PackageQuestId,
                ExecutedPackageDialogueId = request.Plan.PackageDialogueId,
                ExecutedPackageInteractionId = request.Plan.PackageInteractionId,
                Commands = commands,
                RuntimeEventTypes = eventTypes.ToList(),
                RuntimeStateHash = stateHash,
                RestoredRuntimeStateHash = restoredHash,
                SaveLoadRoundtripPassed = stateHash == restoredHash && DictionaryEquals(stateEvidence, restoredEvidence),
                StateEvidence = stateEvidence,
                RestoredStateEvidence = restoredEvidence,
                ObjectiveEvidence = objectiveEvidence,
                ObjectiveInteractionCorrelations = correlations,
                StateDelta = stateDelta,
                IsolationKeys = isolationKeys.ToList(),
                Diagnostics = diagnostics
            };

            return evidenceWithoutHash with
            {
                RuntimeEvidenceHash = ComputeHash(JsonSerializer.Serialize(evidenceWithoutHash, JsonOptions))
            };
        }

        private static void AddEvents(ISet<string> eventTypes, UnifiedRuntimeResult result)
        {
            foreach (var eventType in result.MapEvents.Select(item => "map:" + item.Type).Concat(result.GameplayEvents.Select(item => "gameplay:" + item.Type)))
            {
                eventTypes.Add(eventType);
            }
        }

        private static SemanticSelectedRuntimeCommandEvidence ToCommand(
            string commandId,
            string commandType,
            string targetId,
            UnifiedRuntimeResult result,
            string ruleInteractionPatternId = "",
            string correlatedObjectiveId = "") => new()
        {
            CommandId = commandId,
            CommandType = commandType,
            TargetId = targetId,
            Succeeded = result.Success,
            RuleInteractionPatternId = ruleInteractionPatternId,
            CorrelatedObjectiveId = correlatedObjectiveId,
            EventTypes = result.MapEvents.Select(item => "map:" + item.Type)
                .Concat(result.GameplayEvents.Select(item => "gameplay:" + item.Type))
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };

        private static string SafeId(string value) =>
            value.Replace('/', '_').Replace('-', '_');

        private static double ItemAmount(GameRuntimeState state, string itemId) =>
            state.Inventories
                .SelectMany(item => item.Stacks)
                .Where(item => item.ItemId == itemId)
                .Sum(item => item.Amount);

        private static string FlagValue(GameRuntimeState state, string flagId) =>
            state.Flags.FirstOrDefault(item => item.Id == flagId)?.Value ?? string.Empty;

        private static string QuestState(GameRuntimeState state, string questId) =>
            state.Quests.FirstOrDefault(item => item.QuestId == questId)?.State ?? "not_started";

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out var value) || value != pair.Value)
                {
                    return false;
                }
            }

        return true;
    }

    private static IReadOnlyDictionary<string, string> Set(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            copy[pair.Key] = pair.Value;
        }

        copy[key] = value;
        return copy;
    }

        private static IUnifiedGameRuntimeService CreateBridge()
        {
            return new UnifiedGameRuntimeService(new DefaultGameRuntime(), CreateGameplayRuntime());
        }

        private static IGameRuntimeService CreateGameplayRuntime()
        {
            var requirementEvaluator = new RequirementEvaluator();
            var costConsumer = new CostConsumer();
            var outputApplier = new OutputApplier();
            var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
            var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
            var questRuntimeService = new QuestRuntimeService(requirementEvaluator, outputApplier);
            var encounterRuntimeService = new EncounterRuntimeService(requirementEvaluator, outputApplier);
            var dialogueRuntimeService = new DialogueRuntimeService(
                requirementEvaluator,
                costConsumer,
                outputApplier,
                questRuntimeService,
                transactionRuntimeService,
                encounterRuntimeService);
            var useItemRuntimeService = new UseItemRuntimeService(requirementEvaluator, outputApplier);
            var interactionRuntimeService = new InteractionRuntimeService(
                requirementEvaluator,
                outputApplier,
                recipeRuntimeService,
                transactionRuntimeService,
                useItemRuntimeService: useItemRuntimeService,
                dialogueRuntimeService: dialogueRuntimeService,
                questRuntimeService: questRuntimeService,
                encounterRuntimeService: encounterRuntimeService);

            return new GameRuntimeService(
                new GameRuntimeStateFactory(),
                recipeRuntimeService,
                new LootRuntimeService(requirementEvaluator, outputApplier),
                transactionRuntimeService,
                new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
                useItemRuntimeService,
                interactionRuntimeService,
                encounterRuntimeService: encounterRuntimeService,
                questRuntimeService: questRuntimeService,
                dialogueRuntimeService: dialogueRuntimeService);
        }

        private static SemanticSelectedRuntimeCompositionDiagnostic Diagnostic(
            string severity,
            string code,
            string target,
            string message) => new()
            {
                Severity = severity,
                Code = code,
                Target = target,
                Message = message
            };
    }

    private sealed class DefaultVisibleRuntimeAdapter : IVisibleGeneratedPlayableRuntimeAdapter
    {
        public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package)
        {
            var runtime = new DefaultGameRuntime();
            var start = runtime.Start(package);
            var eventTypes = new SortedSet<string>(start.Events.Select(item => item.Type.ToString()), StringComparer.Ordinal);
            var commandAttempts = new List<VisibleGeneratedPlayableRuntimeCommandAttempt>();
            var currentState = start.State;

            if (start.Success)
            {
                var move = runtime.Execute(package, currentState, PlayerCommand.Move(Direction2D.Right));
                currentState = move.State;
                commandAttempts.Add(ToAttempt("01_move_right", "move/right", move));
                foreach (var eventType in move.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }

                var interact = runtime.Execute(package, currentState, PlayerCommand.Interact());
                currentState = interact.State;
                commandAttempts.Add(ToAttempt("02_interact", "interact", interact));
                foreach (var eventType in interact.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }
            }

            return new VisibleGeneratedPlayableRuntimeAttempt
            {
                RuntimeStartAttempted = true,
                RuntimeStartSucceeded = start.Success,
                StartMapId = package.Manifest.StartMapId,
                CurrentMapId = currentState.CurrentMapId,
                PlayerStartPosition = new VisibleGeneratedPlayablePosition
                {
                    X = start.State.PlayerPosition.X,
                    Y = start.State.PlayerPosition.Y
                },
                PlayerCurrentPosition = new VisibleGeneratedPlayablePosition
                {
                    X = currentState.PlayerPosition.X,
                    Y = currentState.PlayerPosition.Y
                },
                CommandAttempts = commandAttempts,
                EventTypes = eventTypes.ToList()
            };
        }

        private static VisibleGeneratedPlayableRuntimeCommandAttempt ToAttempt(
            string commandId,
            string commandType,
            CommandResult result) => new()
        {
            CommandId = commandId,
            CommandType = commandType,
            Succeeded = result.Success,
            CurrentMapId = result.State.CurrentMapId,
            PlayerPosition = new VisibleGeneratedPlayablePosition
            {
                X = result.State.PlayerPosition.X,
                Y = result.State.PlayerPosition.Y
            },
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventTargets = result.Events.Select(item => item.TargetId ?? string.Empty).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventMessages = result.Events.Select(item => item.Message).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
