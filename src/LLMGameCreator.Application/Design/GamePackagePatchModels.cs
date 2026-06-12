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
