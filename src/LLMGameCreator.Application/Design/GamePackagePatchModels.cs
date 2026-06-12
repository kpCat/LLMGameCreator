using LLMGameCreator.Domain.Validation;

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

