using LLMGameCreator.Domain.Validation;
using LLMGameCreator.Domain.Definitions;

namespace LLMGameCreator.Application.Design;

public static class GamePackagePatchArtifactKinds
{
    public const string PatchV1 = "game_package_patch_v1";
    public const string ApplyResultV1 = "game_package_patch_apply_result_v1";
}

public sealed record GamePackagePatchSource(
    string PlanId,
    string PreviewArtifactId);

public sealed record GamePackagePatchDocument(
    string Kind,
    int SchemaVersion,
    GamePackagePatchSource Source,
    IReadOnlyList<GamePackagePatchOperation> Operations);

public abstract record GamePackagePatchOperation(string Op)
{
    public abstract string Target { get; }
}

public sealed record UpsertTilePrototypePatchOperation(
    string Id,
    string Name,
    bool Walkable,
    double MovementCost,
    string? AssetId) : GamePackagePatchOperation("upsert_tile_prototype")
{
    public override string Target => Id;
}

public sealed record UpsertMapPatchOperation(
    string Id,
    string Name,
    int Width,
    int Height,
    string DefaultTileId,
    int StartX,
    int StartY) : GamePackagePatchOperation("upsert_map")
{
    public override string Target => Id;
}

public sealed record UpsertEntityPrototypePatchOperation(
    string Id,
    string Name,
    string? AssetId) : GamePackagePatchOperation("upsert_entity_prototype")
{
    public override string Target => Id;
}

public sealed record UpdateManifestPatchOperation(
    string? Title,
    string? Description,
    string? Version,
    string? StartMapId) : GamePackagePatchOperation("update_manifest")
{
    public override string Target => "manifest";
}

public sealed record UpsertItemPrototypePatchOperation(ItemDefinition Item) : GamePackagePatchOperation("upsert_item_prototype")
{
    public override string Target => Item.Id;
}

public sealed record UpsertResourcePatchOperation(ResourceDefinition Resource) : GamePackagePatchOperation("upsert_resource")
{
    public override string Target => Resource.Id;
}

public sealed record UpsertStatusPatchOperation(StatusDefinition Status) : GamePackagePatchOperation("upsert_status")
{
    public override string Target => Status.Id;
}

public sealed record UpsertRecipePatchOperation(RecipeDefinition Recipe) : GamePackagePatchOperation("upsert_recipe")
{
    public override string Target => Recipe.Id;
}

public sealed record UpsertLootTablePatchOperation(LootTableDefinition LootTable) : GamePackagePatchOperation("upsert_loot_table")
{
    public override string Target => LootTable.Id;
}

public sealed record UpsertTransactionPatchOperation(TransactionDefinition Transaction) : GamePackagePatchOperation("upsert_transaction")
{
    public override string Target => Transaction.Id;
}

public sealed record UpsertResourceNetworkPatchOperation(ResourceNetworkDefinition ResourceNetwork) : GamePackagePatchOperation("upsert_resource_network")
{
    public override string Target => ResourceNetwork.Id;
}

public sealed record UpsertResourceNodePatchOperation(ResourceNodeDefinition ResourceNode) : GamePackagePatchOperation("upsert_resource_node")
{
    public override string Target => ResourceNode.Id;
}

public sealed record UpsertInventoryPatchOperation(InventoryDefinition Inventory) : GamePackagePatchOperation("upsert_inventory")
{
    public override string Target => Inventory.Id;
}

public sealed record UpsertEquipmentSlotPatchOperation(EquipmentSlotDefinition EquipmentSlot) : GamePackagePatchOperation("upsert_equipment_slot")
{
    public override string Target => EquipmentSlot.Id;
}

public sealed record UpsertStatPatchOperation(StatDefinition Stat) : GamePackagePatchOperation("upsert_stat")
{
    public override string Target => Stat.Id;
}

public sealed record UpsertProgressionPatchOperation(ProgressionDefinition Progression) : GamePackagePatchOperation("upsert_progression")
{
    public override string Target => Progression.Id;
}

public sealed record UpsertEncounterPatchOperation(EncounterDefinition Encounter) : GamePackagePatchOperation("upsert_encounter")
{
    public override string Target => Encounter.Id;
}

public sealed record UpsertAbilityPatchOperation(AbilityDefinition Ability) : GamePackagePatchOperation("upsert_ability")
{
    public override string Target => Ability.Id;
}

public sealed record UpsertQuestPatchOperation(QuestDefinition Quest) : GamePackagePatchOperation("upsert_quest")
{
    public override string Target => Quest.Id;
}

public sealed record UpsertDialoguePatchOperation(DialogueDefinition Dialogue) : GamePackagePatchOperation("upsert_dialogue")
{
    public override string Target => Dialogue.Id;
}

public sealed record UpsertFactionPatchOperation(FactionDefinition Faction) : GamePackagePatchOperation("upsert_faction")
{
    public override string Target => Faction.Id;
}

public sealed record GamePackagePatchValidationIssue(
    string Severity,
    string Code,
    string Message,
    string Target);

public sealed record GamePackagePatchCreateResult(
    GeneratedArtifactRecord? PreviewArtifact,
    GeneratedArtifactRecord? PatchArtifact,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults,
    bool Saved,
    string Message);

public sealed record GamePackagePatchDryRunResult(
    GeneratedArtifactRecord? PatchArtifact,
    bool CanApply,
    IReadOnlyList<GamePackagePatchDiffLine> DiffLines,
    IReadOnlyList<ValidationIssue> ValidationIssues,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> PatchValidationResults,
    string Message);

public sealed record GamePackagePatchDiffLine(
    string Operation,
    string Target,
    string ChangeKind,
    string BeforeJson,
    string AfterJson,
    string Message);

public sealed record GamePackagePatchApplyResult(
    GeneratedArtifactRecord? PatchArtifact,
    bool Applied,
    string? BackupPath,
    IReadOnlyList<GamePackagePatchDiffLine> DiffLines,
    IReadOnlyList<ValidationIssue> ValidationIssues,
    GeneratedArtifactRecord? AuditArtifact,
    string Message);
