using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class CanonicalRuntimePlayerLoopReadinessService :
    ICanonicalRuntimePlayerLoopReadinessService
{
    public static IReadOnlyList<string> RequiredStepCategories =>
    [
        "load_package",
        "show_start_state",
        "show_map_position",
        "show_interaction_result",
        "show_dialogue",
        "show_quest_state",
        "show_inventory_state",
        "show_crafting_result",
        "show_harvest_result",
        "show_transaction_result",
        "show_encounter_state",
        "show_combat_round",
        "show_final_state"
    ];

    public CanonicalRuntimePlayerLoopReadinessResult Build(
        IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> transcript,
        CanonicalRuntimeSelectedCandidateStateSummary stateSummary,
        CanonicalRuntimePlayerLoopReadinessRequest request,
        bool saveLoadReplayStillReferenced,
        bool selectedCandidateExecutedByRuntime)
    {
        var steps = new List<CanonicalRuntimePlayerLoopStep>();
        AddEventStep(
            steps,
            "load_package",
            "Load selected package",
            "Player adapter loads canonical package identity from runtime state summary.",
            "feature.package.load",
            "runtime.command.start",
            FirstEvent(transcript, "GameStarted") ?? FirstEvent(transcript, "Message"),
            fallbackTargetId: stateSummary.PackageId);
        AddEventStep(
            steps,
            "show_start_state",
            "Show start state",
            "Present the initial runtime-owned game state.",
            "feature.runtime.start_state",
            "runtime.command.start",
            FirstEvent(transcript, "GameStarted"),
            fallbackTargetId: stateSummary.PackageId);
        AddEventStep(
            steps,
            "show_map_position",
            "Show map position",
            "Present canonical map position "
            + stateSummary.CurrentMapId
            + " @ "
            + stateSummary.PlayerX
            + ","
            + stateSummary.PlayerY
            + ".",
            "feature.world.grid_position",
            "runtime.command.move",
            FirstEvent(transcript, "PlayerMoved"),
            fallbackTargetId: stateSummary.CurrentMapId);
        AddEventStep(
            steps,
            "show_interaction_result",
            "Show interaction result",
            "Present interaction outcome from canonical runtime events.",
            "feature.interaction.inspect",
            "runtime.command.interact",
            FirstEvent(transcript, "InteractionTriggered"));
        AddEventStep(
            steps,
            "show_dialogue",
            "Show dialogue",
            "Present opened dialogue from runtime state.",
            "feature.dialogue.intent_templates",
            "runtime.command.open_dialogue",
            FirstEvent(transcript, "DialogueOpened"));
        AddEventStep(
            steps,
            "show_quest_state",
            "Show quest state",
            "Present quest/journal/objective state from runtime events.",
            "feature.quest.objective_chain",
            "runtime.command.start_quest",
            FirstEvent(transcript, "QuestObjectiveUpdated")
            ?? FirstEvent(transcript, "QuestStarted")
            ?? FirstEvent(transcript, "JournalUpdated"));
        AddEventStep(
            steps,
            "show_inventory_state",
            "Show inventory state",
            "Present inventory/resource summary from canonical final state.",
            "feature.inventory.basic",
            "runtime.command.add_item",
            FirstEvent(transcript, "InventoryChanged"),
            fallbackTargetId: "inventory/player_start");
        AddEventStep(
            steps,
            "show_crafting_result",
            "Show crafting result",
            "Present crafting result from runtime recipe service.",
            "feature.crafting.recipes",
            "runtime.command.craft_recipe",
            FirstEvent(transcript, "RecipeCrafted"));
        AddEventStep(
            steps,
            "show_harvest_result",
            "Show harvest result",
            "Present harvest result from runtime harvest service.",
            "feature.harvest.resource_node",
            "runtime.command.harvest_resource",
            FirstEvent(transcript, "ResourceHarvested"));
        AddEventStep(
            steps,
            "show_transaction_result",
            "Show transaction result",
            "Present transaction result from runtime transaction service.",
            "feature.economy.transaction",
            "runtime.command.execute_transaction",
            FirstEvent(transcript, "TransactionExecuted"));
        AddEventStep(
            steps,
            "show_encounter_state",
            "Show encounter state",
            "Present encounter state from runtime encounter service.",
            "feature.combat.turn_based_encounter",
            "runtime.command.start_encounter",
            FirstEvent(transcript, "EncounterStarted"));
        AddEventStep(
            steps,
            "show_combat_round",
            "Show combat round",
            "Present turn-based combat round from runtime events.",
            "feature.combat.round",
            "runtime.command.use_ability",
            FirstEvent(transcript, "DamageApplied") ?? FirstEvent(transcript, "AbilityUsed"));
        AddFinalStateStep(steps, stateSummary);

        var presentCategories = steps
            .Where(step => !string.IsNullOrWhiteSpace(step.Category))
            .Select(step => step.Category)
            .ToHashSet(StringComparer.Ordinal);
        var missing = RequiredStepCategories
            .Where(category => !presentCategories.Contains(category))
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToList();
        var featureHints = steps
            .Select(step => step.FeatureModuleHint)
            .Where(hint => !string.IsNullOrWhiteSpace(hint))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(hint => hint, StringComparer.Ordinal)
            .ToList();
        var contract = new CanonicalRuntimePlayerAdapterContract
        {
            CandidateId = stateSummary.CandidateId,
            PackageId = stateSummary.PackageId,
            PackageTitle = stateSummary.PackageTitle,
            TranscriptPath = request.TranscriptPath,
            StateSummaryPath = request.StateSummaryPath,
            CanonicalRuntimeSource = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            GameplayExecutedByPlayerAdapter = false,
            RequiredStepCategories = RequiredStepCategories,
            FeatureModuleCoverageHints = featureHints
        };
        var diagnostics = new List<string>();
        if (transcript.Count == 0)
        {
            diagnostics.Add("goal135.transcript_empty");
        }

        if (missing.Count > 0)
        {
            diagnostics.Add("goal135.missing_step_categories:" + string.Join(",", missing));
        }

        if (!saveLoadReplayStillReferenced)
        {
            diagnostics.Add("goal135.save_load_replay_not_referenced");
        }

        if (!selectedCandidateExecutedByRuntime)
        {
            diagnostics.Add("goal135.selected_candidate_not_executed_by_runtime");
        }

        return new CanonicalRuntimePlayerLoopReadinessResult
        {
            CandidateId = stateSummary.CandidateId,
            Passed = transcript.Count > 0
                     && missing.Count == 0
                     && saveLoadReplayStillReferenced
                     && selectedCandidateExecutedByRuntime,
            CanonicalRuntimeSource = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            PlayerAdapterContractPresent = true,
            PlayerLoopPlanPresent = steps.Count > 0,
            RequiredStepCategoriesPresent = missing.Count == 0,
            SaveLoadReplayStillReferenced = saveLoadReplayStillReferenced,
            SelectedCandidateExecutedByRuntime = selectedCandidateExecutedByRuntime,
            PlayerLoopStepCount = steps.Count,
            RequiredStepCategories = RequiredStepCategories,
            MissingStepCategories = missing,
            Steps = steps,
            PlayerAdapterContract = contract,
            FeatureModuleCoverageHints = featureHints,
            Diagnostics = diagnostics
        };
    }

    private static CanonicalRuntimeSelectedCandidateEvent? FirstEvent(
        IEnumerable<CanonicalRuntimeSelectedCandidateEvent> transcript,
        string eventType) =>
        transcript.FirstOrDefault(item => string.Equals(
            item.EventType,
            eventType,
            StringComparison.Ordinal));

    private static void AddEventStep(
        ICollection<CanonicalRuntimePlayerLoopStep> steps,
        string category,
        string title,
        string detail,
        string featureModuleHint,
        string runtimePrimitiveHint,
        CanonicalRuntimeSelectedCandidateEvent? runtimeEvent,
        string fallbackTargetId = "")
    {
        if (runtimeEvent is null && string.IsNullOrWhiteSpace(fallbackTargetId))
        {
            return;
        }

        steps.Add(new CanonicalRuntimePlayerLoopStep
        {
            Index = steps.Count,
            Category = category,
            Title = title,
            Detail = runtimeEvent is null
                ? detail
                : detail + " Event: " + runtimeEvent.Message,
            SourceEventIndex = runtimeEvent?.EventIndex,
            SourceCommandIndex = runtimeEvent?.CommandIndex,
            SourceStepId = runtimeEvent?.StepId ?? string.Empty,
            SourceEventType = runtimeEvent?.EventType ?? "stateSummary",
            SourceTargetId = string.IsNullOrWhiteSpace(runtimeEvent?.TargetId)
                ? fallbackTargetId
                : runtimeEvent.TargetId,
            FeatureModuleHint = featureModuleHint,
            RuntimePrimitiveHint = runtimePrimitiveHint,
            StateHashBefore = runtimeEvent?.StateHashBefore ?? string.Empty,
            StateHashAfter = runtimeEvent?.StateHashAfter ?? string.Empty,
            CanonicalRuntimeAuthority = true
        });
    }

    private static void AddFinalStateStep(
        ICollection<CanonicalRuntimePlayerLoopStep> steps,
        CanonicalRuntimeSelectedCandidateStateSummary stateSummary)
    {
        if (string.IsNullOrWhiteSpace(stateSummary.FinalStateHash))
        {
            return;
        }

        steps.Add(new CanonicalRuntimePlayerLoopStep
        {
            Index = steps.Count,
            Category = "show_final_state",
            Title = "Show final state",
            Detail =
                "Present final canonical runtime state. inventory="
                + stateSummary.InventorySummary
                + "; resources="
                + stateSummary.ResourceSummary
                + "; quest="
                + stateSummary.QuestSummary
                + "; encounter="
                + stateSummary.ActiveEncounterSummary,
            SourceEventIndex = null,
            SourceCommandIndex = null,
            SourceStepId = "canonical_runtime_state_summary",
            SourceEventType = "stateSummary",
            SourceTargetId = stateSummary.PackageId,
            FeatureModuleHint = "feature.runtime.final_state",
            RuntimePrimitiveHint = "runtime.state.summary",
            StateHashBefore = stateSummary.StateHashChain.LastOrDefault() ?? string.Empty,
            StateHashAfter = stateSummary.FinalStateHash,
            CanonicalRuntimeAuthority = true
        });
    }
}
