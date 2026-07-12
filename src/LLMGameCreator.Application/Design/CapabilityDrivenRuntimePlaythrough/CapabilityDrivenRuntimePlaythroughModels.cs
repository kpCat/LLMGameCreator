using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;

public sealed record FeatureModuleRuntimePlaythroughContract
{
    public string ContractId { get; init; } = string.Empty;
    public string CapabilityId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Phase { get; init; } = string.Empty;
    public int Order { get; init; }
    public string RuntimePrimitiveId { get; init; } = string.Empty;
    public string TargetSelector { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Args { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> DependsOnActionIds { get; init; } = [];
    public bool CheckpointBoundaryAfter { get; init; }
    public bool PresentationOnly { get; init; }
    public bool Required { get; init; } = true;
    public IReadOnlyList<string> ExpectedRuntimeEffects { get; init; } = [];
}

public sealed record CapabilityDrivenRuntimePlaythroughPlanningResult
{
    public bool Passed { get; init; }
    public CapabilityRuntimePlaythroughPlan Plan { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class CapabilityDrivenRuntimePlaythroughException : InvalidOperationException
{
    public CapabilityDrivenRuntimePlaythroughException(IReadOnlyList<string> diagnostics)
        : base("Capability-driven Runtime playthrough rejected: " + string.Join("; ", diagnostics))
    {
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<string> Diagnostics { get; }
}

public static class CapabilityRuntimePrimitiveIds
{
    public const string Start = "runtime.command.start";
    public const string Move = "runtime.command.move";
    public const string Interact = "runtime.command.interact";
    public const string OpenDialogue = "runtime.command.open_dialogue";
    public const string StartOrUpdateQuest = "runtime.command.start_or_update_quest";
    public const string ShowInventory = "runtime.command.show_inventory";
    public const string CraftRecipe = "runtime.command.craft_recipe";
    public const string HarvestResource = "runtime.command.harvest_resource";
    public const string ExecuteTransaction = "runtime.command.execute_transaction";
    public const string StartEncounter = "runtime.command.start_encounter";
    public const string BasicAttack = "runtime.command.basic_attack";
    public const string OpenContainer = "runtime.command.open_container";
    public const string TakeFromContainer = "runtime.command.take_from_container";
    public const string EquipItem = "runtime.command.equip_item";
    public const string ChangeProgression = "runtime.command.change_progression";
    public const string InspectInventory = "runtime.presentation.inspect_inventory";
    public const string InspectStatus = "runtime.presentation.inspect_status";
    public const string InspectEquipment = "runtime.presentation.inspect_equipment";
    public const string InspectAttributes = "runtime.presentation.inspect_attributes";
    public const string InspectProgression = "runtime.presentation.inspect_progression";
    public const string FinalState = "runtime.presentation.final_state";

    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Start, Move, Interact, OpenDialogue, StartOrUpdateQuest, ShowInventory,
        CraftRecipe, HarvestResource, ExecuteTransaction, StartEncounter, BasicAttack,
        OpenContainer, TakeFromContainer, EquipItem, ChangeProgression,
        InspectInventory, InspectStatus, InspectEquipment, InspectAttributes, InspectProgression, FinalState
    };
}
