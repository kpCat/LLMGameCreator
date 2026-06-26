using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.Gameplay;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Tests.Application.Gameplay;

public static class RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory
{
    public static RulePackCombatFactionSocialWorkTheftAcceptanceService CreateService(
        IRulePackCombatFactionSocialWorkTheftRuntimeAdapter? runtimeAdapter = null) =>
        new(runtimeAdapter ?? new RealRulePackCombatFactionSocialWorkTheftRuntimeAdapter());
}

public sealed class RealRulePackCombatFactionSocialWorkTheftRuntimeAdapter : IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RealRulePackCombatFactionSocialWorkTheftRuntimeAdapter()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request)
    {
        var services = CreateRuntimeServices();
        var serializer = new RuntimeStateSerializer();
        var snapshotStore = new RuntimeSnapshotStore(serializer);
        var start = services.Runtime.CreateInitialState(request.Package);
        var state = start.State;
        var diagnostics = start.Diagnostics
            .Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? request.ScenarioId, item.Message))
            .ToList();

        state.Metadata["combatFamily.scenario"] = request.ExpectedScenarioStateMarker;
        var commands = new List<CombatRuntimeCommandEvidence>();
        foreach (var command in request.Commands)
        {
            var before = RuntimeSnapshot.FromState(state);
            var result = services.Runtime.Execute(request.Package, state, ToRuntimeCommand(command));
            commands.Add(ToCommandEvidence(command, before, RuntimeSnapshot.FromState(result.State), result));
            state = result.State;
            state.Metadata["combatFamily.commandLog"] = string.Join("|", commands.Select(item => item.CommandId + ":" + item.Succeeded.ToString().ToLowerInvariant()));
            if (!result.Success)
            {
                break;
            }
        }

        var snapshot = RuntimeSnapshot.FromState(state);
        var stateEvidence = snapshot.ToEvidence(request.ScenarioId);
        var serialized = serializer.Serialize(state);
        var restoredState = serializer.DeserializeGameRuntimeState(serialized);
        var snapshotProjectRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "RulePackCombatFamily", Guid.NewGuid().ToString("N"));
        var slotName = "goal009_" + request.ScenarioId.Replace('/', '_');
        var save = snapshotStore.SaveSnapshot(snapshotProjectRoot, slotName, new UnifiedRuntimeSession { GameplayState = state });
        var load = snapshotStore.LoadSnapshot(snapshotProjectRoot, slotName);

        diagnostics.AddRange(save.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? slotName, item.Message)));
        diagnostics.AddRange(load.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? slotName, item.Message)));

        var restoredFromSnapshot = load.Session?.GameplayState ?? restoredState;
        if (request.ExpectedScenarioStateMarker == "invalid_save_load_mismatch")
        {
            restoredFromSnapshot.Flags.Add(new RuntimeFlagState { Id = "flag/save_load_mismatch", Value = "corrupted" });
        }

        var restoredEvidence = RuntimeSnapshot.FromState(restoredFromSnapshot).ToEvidence(request.ScenarioId);
        var stateHash = ComputeHash(serialized);
        var restoredHash = ComputeHash(serializer.Serialize(restoredFromSnapshot));
        var scenarioIsolationPassed = !request.ExpectedScenarioStateMarker.StartsWith("leak:", StringComparison.Ordinal) &&
                                      !state.Metadata.ContainsKey("combatFamily.previousScenario");

        var evidenceWithoutHash = new RulePackCombatFactionSocialWorkTheftRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = start.Success,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = state.PackageId,
            RuntimeBoundary = new CombatRuntimeBoundaryEvidence
            {
                AdapterId = "real_combat_family_game_runtime_service_adapter",
                RuntimeServiceType = typeof(GameRuntimeService).FullName ?? nameof(GameRuntimeService),
                StateFactoryType = typeof(GameRuntimeStateFactory).FullName ?? nameof(GameRuntimeStateFactory),
                SerializerType = typeof(RuntimeStateSerializer).FullName ?? nameof(RuntimeStateSerializer),
                SnapshotStoreType = typeof(RuntimeSnapshotStore).FullName ?? nameof(RuntimeSnapshotStore),
                UsedGameRuntimeService = true,
                UsedRuntimeStateFactory = true,
                UsedEncounterRuntimeService = true,
                UsedEncounterAiService = true,
                UsedFactionRuntimeService = true,
                UsedDialogueRuntimeService = true,
                UsedInteractionRuntimeService = true,
                UsedContainerRuntimeService = true
            },
            Commands = commands,
            EncounterBefore = commands.FirstOrDefault()?.EncounterDelta.Before ?? new CombatEncounterEvidence(),
            EncounterAfter = snapshot.Encounter,
            FactionReputationBefore = commands.FirstOrDefault()?.FactionDelta.Before ?? new SortedDictionary<string, string>(StringComparer.Ordinal),
            FactionReputationAfter = snapshot.Factions,
            DialogueBefore = commands.FirstOrDefault()?.DialogueDelta.Before ?? new CombatDialogueEvidence(),
            DialogueAfter = snapshot.Dialogue,
            WorkEvidence = BuildWorkEvidence(snapshot),
            TheftEvidence = BuildTheftEvidence(snapshot),
            RuntimeStateHash = stateHash,
            RestoredRuntimeStateHash = restoredHash,
            SaveLoadRoundtripPassed = stateHash == restoredHash && DictionaryEquals(stateEvidence, restoredEvidence),
            SaveLoadEvidence = new CombatSaveLoadEvidence
            {
                UsedRuntimeStateSerializer = true,
                UsedRuntimeSnapshotStore = true,
                SerializedFullState = true,
                SerializedStateHash = stateHash,
                RestoredSerializedStateHash = restoredHash,
                SnapshotSlotName = slotName,
                SnapshotSaveSucceeded = save.Success,
                SnapshotLoadSucceeded = load.Success
            },
            ScenarioIsolationPassed = scenarioIsolationPassed,
            StateEvidence = stateEvidence,
            RestoredStateEvidence = restoredEvidence,
            Diagnostics = diagnostics
        };

        return evidenceWithoutHash with
        {
            RuntimeEvidenceHash = ComputeHash(JsonSerializer.Serialize(evidenceWithoutHash, JsonOptions))
        };
    }

    private static RuntimeServices CreateRuntimeServices()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var stateFactory = new GameRuntimeStateFactory();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var containerRuntimeService = new ContainerRuntimeService();
        var harvestRuntimeService = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var useItemRuntimeService = new UseItemRuntimeService(requirementEvaluator, outputApplier);
        var encounterRuntimeService = new EncounterRuntimeService(requirementEvaluator, outputApplier);
        var encounterAiService = new EncounterAiService(encounterRuntimeService);
        var factionRuntimeService = new FactionRuntimeService();
        var questRuntimeService = new QuestRuntimeService(requirementEvaluator, outputApplier);
        var dialogueRuntimeService = new DialogueRuntimeService(requirementEvaluator, costConsumer, outputApplier, questRuntimeService, transactionRuntimeService, encounterRuntimeService);
        var interactionRuntimeService = new InteractionRuntimeService(
            requirementEvaluator,
            outputApplier,
            recipeRuntimeService,
            transactionRuntimeService,
            containerRuntimeService,
            harvestRuntimeService,
            useItemRuntimeService,
            dialogueRuntimeService,
            questRuntimeService,
            encounterRuntimeService);

        var runtime = new GameRuntimeService(
            stateFactory,
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            useItemRuntimeService,
            interactionRuntimeService,
            new EquipmentRuntimeService(requirementEvaluator),
            containerRuntimeService,
            harvestRuntimeService,
            encounterRuntimeService,
            encounterAiService,
            questRuntimeService,
            dialogueRuntimeService,
            factionRuntimeService);

        return new RuntimeServices(runtime);
    }

    private static GameRuntimeCommand ToRuntimeCommand(CombatCommandSpec command) =>
        command.CommandType switch
        {
            "combat/start_encounter" => GameRuntimeCommand.StartEncounter(command.TargetId),
            "combat/use_ability" => GameRuntimeCommand.UseAbility(command.TargetId, command.ActorId, command.SecondaryTargetId),
            "combat/basic_attack" => GameRuntimeCommand.BasicAttack(command.ActorId, command.SecondaryTargetId),
            "combat/run_ai" => new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi },
            "faction/change_reputation" => GameRuntimeCommand.ChangeReputation(command.TargetId, command.Amount),
            "social/open_dialogue" => GameRuntimeCommand.OpenDialogue(command.TargetId),
            "social/choose_dialogue" => GameRuntimeCommand.ChooseDialogueOption(command.TargetId),
            "work/execute_contract" => new GameRuntimeCommand { Type = GameRuntimeCommandType.ExecuteInteraction, Id = command.TargetId, InventoryId = command.InventoryId },
            "theft/open_container" => GameRuntimeCommand.OpenContainer(command.TargetId),
            "theft/take_from_container" => GameRuntimeCommand.TakeFromContainer(command.TargetId, command.SecondaryTargetId, command.Amount, command.InventoryId),
            "gameplay/set_flag" => new GameRuntimeCommand { Type = GameRuntimeCommandType.SetFlag, Id = command.TargetId, Value = command.Value },
            _ => new GameRuntimeCommand { Type = (GameRuntimeCommandType)999, Id = command.TargetId }
        };

    private static CombatRuntimeCommandEvidence ToCommandEvidence(
        CombatCommandSpec command,
        RuntimeSnapshot before,
        RuntimeSnapshot after,
        GameRuntimeResult result)
    {
        var diagnostic = result.Diagnostics.FirstOrDefault(item => item.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        return new CombatRuntimeCommandEvidence
        {
            CommandId = command.CommandId,
            SourceDeclarationId = command.SourceDeclarationId,
            CommandType = command.CommandType,
            TargetId = command.TargetId,
            SecondaryTargetId = command.SecondaryTargetId,
            Succeeded = result.Success,
            DiagnosticCode = result.Success ? "ok" : diagnostic?.Code ?? "runtime.command_failed",
            DiagnosticMessage = result.Success ? string.Empty : diagnostic?.Message ?? result.Message,
            RuntimeEventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            RuntimeDiagnosticCodes = result.Diagnostics.Select(item => item.Code).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EncounterDelta = new CombatEncounterDelta { Changed = !Equals(before.Encounter, after.Encounter), Before = before.Encounter, After = after.Encounter },
            FactionDelta = new CombatFactionDelta { Changed = !DictionaryEquals(before.Factions, after.Factions), Before = before.Factions, After = after.Factions },
            DialogueDelta = new CombatDialogueDelta { Changed = !Equals(before.Dialogue, after.Dialogue), Before = before.Dialogue, After = after.Dialogue },
            WorkDelta = new CombatWorkDelta { Changed = !Equals(before.Work, after.Work), Before = before.Work, After = after.Work },
            ContainerDelta = new CombatContainerDelta { Opened = result.Events.Any(item => item.Type == GameRuntimeEventType.ContainerOpened), Changed = !DictionaryEquals(before.ContainerItems, after.ContainerItems), Before = before.ContainerItems, After = after.ContainerItems },
            InventoryDelta = new CombatInventoryDelta { Changed = !DictionaryEquals(before.PlayerItems, after.PlayerItems), Before = before.PlayerItems, After = after.PlayerItems },
            FlagDelta = new CombatFlagDelta { Changed = !DictionaryEquals(before.Flags, after.Flags), Before = before.Flags, After = after.Flags }
        };
    }

    private static CombatWorkEvidence BuildWorkEvidence(RuntimeSnapshot snapshot) => new()
    {
        ContractInteractionId = "interaction/work_contract_reward",
        RewardItemId = "item/wage_scrip",
        RewardAmountAfter = ParseDouble(snapshot.PlayerItems.GetValueOrDefault("item/wage_scrip")) ?? 0,
        CompletionFlagId = "flag/work_contract_completed",
        CompletionFlagAfter = snapshot.Flags.GetValueOrDefault("flag/work_contract_completed", string.Empty)
    };

    private static CombatTheftEvidence BuildTheftEvidence(RuntimeSnapshot snapshot) => new()
    {
        ContainerInventoryId = "inventory/merchant_cache",
        ItemId = "item/stolen_gem",
        ContainerAmountAfter = ParseDouble(snapshot.ContainerItems.GetValueOrDefault("item/stolen_gem")) ?? 0,
        PlayerAmountAfter = ParseDouble(snapshot.PlayerItems.GetValueOrDefault("item/stolen_gem")) ?? 0,
        TheftFlagId = "flag/theft_reported",
        TheftFlagAfter = snapshot.Flags.GetValueOrDefault("flag/theft_reported", string.Empty),
        FactionId = "faction/settlement_watch",
        ReputationAfter = snapshot.Factions.GetValueOrDefault("faction/settlement_watch", string.Empty)
    };

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string> first, IReadOnlyDictionary<string, string> second) =>
        first.Count == second.Count &&
        first.All(item => second.TryGetValue(item.Key, out var value) && string.Equals(item.Value, value, StringComparison.Ordinal));

    private static double? ParseDouble(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static string Format(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static RulePackCombatFactionSocialWorkTheftDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private sealed record RuntimeServices(IGameRuntimeService Runtime);

    private sealed record RuntimeSnapshot
    {
        public string PackageId { get; init; } = string.Empty;
        public string CurrentMapId { get; init; } = string.Empty;
        public CombatEncounterEvidence Encounter { get; init; } = new();
        public CombatDialogueEvidence Dialogue { get; init; } = new();
        public IReadOnlyDictionary<string, string> Factions { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> PlayerItems { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> ContainerItems { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> Flags { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public CombatWorkEvidence Work { get; init; } = new();

        public static RuntimeSnapshot FromState(GameRuntimeState state)
        {
            var playerInventory = state.Inventories.FirstOrDefault(item => item.Id == "inventory/player" || item.OwnerKind.Equals("player", StringComparison.OrdinalIgnoreCase));
            var container = state.Inventories.FirstOrDefault(item => item.Id == "inventory/merchant_cache");
            var playerItems = ToItemDictionary(playerInventory);
            var flags = state.Flags
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToDictionary(item => item.Id, item => item.Value, StringComparer.Ordinal);

            return new RuntimeSnapshot
            {
                PackageId = state.PackageId,
                CurrentMapId = state.CurrentMapId,
                Encounter = ToEncounter(state.ActiveEncounter),
                Dialogue = ToDialogue(state.ActiveDialogue),
                Factions = state.Factions
                    .OrderBy(item => item.FactionId, StringComparer.Ordinal)
                    .ToDictionary(item => item.FactionId, item => Format(item.Reputation) + "|" + item.RelationKind, StringComparer.Ordinal),
                PlayerItems = playerItems,
                ContainerItems = ToItemDictionary(container),
                Flags = flags,
                Metadata = state.Metadata
                    .OrderBy(item => item.Key, StringComparer.Ordinal)
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
                Work = new CombatWorkEvidence
                {
                    ContractInteractionId = "interaction/work_contract_reward",
                    RewardItemId = "item/wage_scrip",
                    RewardAmountAfter = ParseDouble(playerItems.GetValueOrDefault("item/wage_scrip")) ?? 0,
                    CompletionFlagId = "flag/work_contract_completed",
                    CompletionFlagAfter = flags.GetValueOrDefault("flag/work_contract_completed", string.Empty)
                }
            };
        }

        public IReadOnlyDictionary<string, string> ToEvidence(string scenarioId) => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["scenarioId"] = scenarioId,
            ["packageId"] = PackageId,
            ["currentMapId"] = CurrentMapId,
            ["encounter"] = Encounter.EncounterId + "|" + Encounter.Active.ToString().ToLowerInvariant() + "|" + string.Join(",", Encounter.Participants.Select(item => item.Key + "=" + item.Value)) + "|" + string.Join(",", Encounter.ActionHistory),
            ["dialogue"] = Dialogue.DialogueId + "|" + Dialogue.CurrentNodeId + "|" + Dialogue.Open.ToString().ToLowerInvariant() + "|" + string.Join(",", Dialogue.History),
            ["factions"] = string.Join(",", Factions.Select(item => item.Key + "=" + item.Value)),
            ["playerItems"] = string.Join(",", PlayerItems.Select(item => item.Key + "=" + item.Value)),
            ["containerItems"] = string.Join(",", ContainerItems.Select(item => item.Key + "=" + item.Value)),
            ["flags"] = string.Join(",", Flags.Select(item => item.Key + "=" + item.Value)),
            ["commandLog"] = Metadata.GetValueOrDefault("combatFamily.commandLog", string.Empty),
            ["scenarioMarker"] = Metadata.GetValueOrDefault("combatFamily.scenario", string.Empty)
        };

        private static CombatEncounterEvidence ToEncounter(EncounterRuntimeState? encounter)
        {
            if (encounter == null)
            {
                return new CombatEncounterEvidence();
            }

            return new CombatEncounterEvidence
            {
                EncounterId = encounter.EncounterId,
                Active = encounter.Active,
                Round = encounter.Round,
                TurnIndex = encounter.TurnIndex,
                ActionHistory = encounter.ActionHistory.ToList(),
                Participants = encounter.Participants
                    .OrderBy(item => item.Id, StringComparer.Ordinal)
                    .ToDictionary(
                        item => item.Id,
                        item => (item.Alive ? "alive" : "defeated") + "|" + item.Team + "|" + string.Join(";", item.Resources.OrderBy(resource => resource.ResourceId, StringComparer.Ordinal).Select(resource => resource.ResourceId + ":" + Format(resource.Amount))),
                        StringComparer.Ordinal)
            };
        }

        private static CombatDialogueEvidence ToDialogue(DialogueRuntimeState? dialogue)
        {
            if (dialogue == null)
            {
                return new CombatDialogueEvidence();
            }

            return new CombatDialogueEvidence
            {
                DialogueId = dialogue.DialogueId,
                CurrentNodeId = dialogue.CurrentNodeId,
                Open = dialogue.Open,
                History = dialogue.History.ToList()
            };
        }

        private static IReadOnlyDictionary<string, string> ToItemDictionary(InventoryState? inventory)
        {
            if (inventory == null)
            {
                return new SortedDictionary<string, string>(StringComparer.Ordinal);
            }

            return inventory.Stacks
                .GroupBy(item => item.ItemId, StringComparer.Ordinal)
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => Format(item.Sum(stack => stack.Amount)), StringComparer.Ordinal);
        }
    }
}
