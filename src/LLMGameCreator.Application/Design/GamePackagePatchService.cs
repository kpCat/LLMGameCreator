using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design;

public sealed class GamePackagePatchService : IGamePackagePatchService
{
    private const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions PatchJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions PackageJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static GamePackagePatchService()
    {
        PackageJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private readonly IGeneratedArtifactRepository _artifactRepository;
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGamePackageValidator _validator;
    private readonly GamePackagePatchOperationValidator _operationValidator;

    public GamePackagePatchService(
        IGeneratedArtifactRepository artifactRepository,
        ICurrentGamePackageService currentGamePackageService,
        IGamePackageValidator validator,
        GamePackagePatchOperationValidator operationValidator)
    {
        _artifactRepository = artifactRepository;
        _currentGamePackageService = currentGamePackageService;
        _validator = validator;
        _operationValidator = operationValidator;
    }

    public async Task<GamePackagePatchCreateResult> CreatePatchArtifactFromPreviewAsync(string previewArtifactId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(previewArtifactId))
        {
            return CreateFailure(null, "Preview artifact id is required.", "patch.preview.id.empty", "artifact");
        }

        var previewArtifact = await _artifactRepository.GetGeneratedArtifactByIdAsync(previewArtifactId, cancellationToken).ConfigureAwait(false);
        if (previewArtifact == null)
        {
            return CreateFailure(null, $"Preview artifact was not found: {previewArtifactId}", "patch.preview.not_found", previewArtifactId);
        }

        if (!previewArtifact.Kind.Equals("generator_plan_preview", StringComparison.OrdinalIgnoreCase))
        {
            return CreateFailure(previewArtifact, "Only generator_plan_preview artifacts can be converted to package patches.", "patch.preview.kind.invalid", previewArtifact.Id);
        }

        var extraction = ExtractPatchOperationsFromPreview(previewArtifact);
        if (extraction.ValidationResults.Any(IsError))
        {
            return new GamePackagePatchCreateResult(previewArtifact, null, extraction.ValidationResults, false, extraction.Message);
        }

        var patchArtifactId = BuildPatchArtifactId(previewArtifact.Id);
        var document = new GamePackagePatchDocument(
            GamePackagePatchArtifactKinds.PatchV1,
            SchemaVersion,
            new GamePackagePatchSource(extraction.PlanId, previewArtifact.Id),
            extraction.Operations);
        var json = SerializePatchDocument(document);
        var validationResults = _operationValidator.ValidatePatchJson(patchArtifactId, json);
        if (validationResults.Any(IsError))
        {
            return new GamePackagePatchCreateResult(previewArtifact, null, validationResults, false, "Patch operations failed validation; patch artifact was not saved.");
        }

        validationResults = validationResults.Concat(new[]
        {
            new GeneratedArtifactValidationResultRecord(
                BuildValidationResultId(patchArtifactId, "patch.policy.data_only", validationResults.Count),
                patchArtifactId,
                "warning",
                "patch.policy.data_only",
                "Patch artifact stores strict data-only operations; Lua, modules, LLM, codegen and Unity execution are disabled.",
                patchArtifactId,
                "{}")
        }).ToList();

        var artifact = new GeneratedArtifactRecord(
            patchArtifactId,
            GamePackagePatchArtifactKinds.PatchV1,
            $"design-db://generated-artifacts/{patchArtifactId}",
            json,
            extraction.PlanId,
            ValidationState(validationResults),
            JsonSerializer.Serialize(new
            {
                created_utc = DateTimeOffset.UtcNow.ToString("O"),
                source_preview_artifact_id = previewArtifact.Id,
                operation_count = extraction.Operations.Count
            }, PatchJsonOptions));

        await _artifactRepository.SaveGeneratedArtifactAsync(artifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(artifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        return new GamePackagePatchCreateResult(previewArtifact, artifact, validationResults, true, $"Patch artifact saved: {artifact.Path}");
    }

    public async Task<GamePackagePatchDryRunResult> DryRunPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken)
    {
        var load = await LoadAndValidatePatchArtifactAsync(patchArtifactId, cancellationToken).ConfigureAwait(false);
        if (load.Artifact == null || load.Document == null || load.ValidationResults.Any(IsError))
        {
            return new GamePackagePatchDryRunResult(load.Artifact, false, Array.Empty<GamePackagePatchDiffLine>(), ToValidationIssues(load.ValidationResults), load.ValidationResults, load.Message);
        }

        var currentPackage = _currentGamePackageService.CurrentPackage;
        if (currentPackage == null)
        {
            return new GamePackagePatchDryRunResult(load.Artifact, false, Array.Empty<GamePackagePatchDiffLine>(), new[]
            {
                Issue("patch.package.not_loaded", "No current game package is loaded.", patchArtifactId)
            }, load.ValidationResults, "No current game package is loaded.");
        }

        var clone = ClonePackage(currentPackage);
        var diffLines = new List<GamePackagePatchDiffLine>();
        var localIssues = new List<ValidationIssue>();
        ApplyPatch(load.Document, clone, diffLines, localIssues);

        var report = _validator.Validate(clone, _currentGamePackageService.CurrentFolder);
        localIssues.AddRange(report.Issues);
        var canApply = localIssues.All(issue => issue.Severity != ValidationSeverity.Error && issue.Severity != ValidationSeverity.Critical)
            && diffLines.All(line => !line.ChangeKind.Equals("error", StringComparison.OrdinalIgnoreCase));

        return new GamePackagePatchDryRunResult(
            load.Artifact,
            canApply,
            diffLines,
            localIssues,
            load.ValidationResults,
            canApply ? "Patch dry-run can be applied." : "Patch dry-run found errors; apply is blocked.");
    }

    public async Task<GamePackagePatchApplyResult> ApplyPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken)
    {
        var dryRun = await DryRunPatchArtifactAsync(patchArtifactId, cancellationToken).ConfigureAwait(false);
        if (!dryRun.CanApply || dryRun.PatchArtifact == null)
        {
            return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, null, dryRun.DiffLines, dryRun.ValidationIssues, null, dryRun.Message);
        }

        var currentPackage = _currentGamePackageService.CurrentPackage;
        var currentFolder = _currentGamePackageService.CurrentFolder;
        if (currentPackage == null)
        {
            return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, null, dryRun.DiffLines, dryRun.ValidationIssues, null, "No current game package is loaded.");
        }

        if (string.IsNullOrWhiteSpace(currentFolder))
        {
            return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, null, dryRun.DiffLines, dryRun.ValidationIssues, null, "Current game package folder is not set.");
        }

        var original = ClonePackage(currentPackage);
        var backupPath = CreateRollbackSnapshot(currentFolder);
        try
        {
            var load = await LoadAndValidatePatchArtifactAsync(patchArtifactId, cancellationToken).ConfigureAwait(false);
            if (load.Document == null || load.ValidationResults.Any(IsError))
            {
                return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, backupPath, dryRun.DiffLines, ToValidationIssues(load.ValidationResults), null, load.Message);
            }

            var applyIssues = new List<ValidationIssue>();
            ApplyPatch(load.Document, currentPackage, new List<GamePackagePatchDiffLine>(), applyIssues);
            var report = _validator.Validate(currentPackage, currentFolder);
            applyIssues.AddRange(report.Issues);
            if (applyIssues.Any(issue => issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical))
            {
                _currentGamePackageService.ReplaceCurrent(original);
                return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, backupPath, dryRun.DiffLines, applyIssues, null, "Post-apply validation failed; in-memory package was restored and package.json was not saved.");
            }

            await _currentGamePackageService.SaveAsync(cancellationToken).ConfigureAwait(false);
            var auditArtifact = BuildApplyAuditArtifact(dryRun.PatchArtifact, backupPath, dryRun.DiffLines, applyIssues);
            var auditValidationResults = ToArtifactValidationResults(auditArtifact.Id, applyIssues)
                .Concat(new[]
                {
                    new GeneratedArtifactValidationResultRecord(
                        BuildValidationResultId(auditArtifact.Id, "patch.apply.backup_created", applyIssues.Count),
                        auditArtifact.Id,
                        "info",
                        "patch.apply.backup_created",
                        "Rollback snapshot was created before package save.",
                        backupPath,
                        "{}")
                })
                .ToList();

            await _artifactRepository.SaveGeneratedArtifactAsync(auditArtifact, cancellationToken).ConfigureAwait(false);
            await _artifactRepository.SaveValidationResultsAsync(auditArtifact.Id, auditValidationResults, cancellationToken).ConfigureAwait(false);

            return new GamePackagePatchApplyResult(dryRun.PatchArtifact, true, backupPath, dryRun.DiffLines, applyIssues, auditArtifact, "Patch applied and package.json saved.");
        }
        catch (Exception ex)
        {
            _currentGamePackageService.ReplaceCurrent(original);
            return new GamePackagePatchApplyResult(dryRun.PatchArtifact, false, backupPath, dryRun.DiffLines, new[]
            {
                Issue("patch.apply.failed", ex.Message, patchArtifactId)
            }, null, $"Patch apply failed; rollback snapshot is available at {backupPath}");
        }
    }

    private async Task<PatchLoadResult> LoadAndValidatePatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(patchArtifactId))
        {
            var results = new[] { ValidationResult("artifact/missing", "error", "patch.artifact.id.empty", "Patch artifact id is required.", "artifact", 0) };
            return new PatchLoadResult(null, null, results, "Patch artifact id is required.");
        }

        var artifact = await _artifactRepository.GetGeneratedArtifactByIdAsync(patchArtifactId, cancellationToken).ConfigureAwait(false);
        if (artifact == null)
        {
            var results = new[] { ValidationResult(patchArtifactId, "error", "patch.artifact.not_found", $"Patch artifact was not found: {patchArtifactId}", patchArtifactId, 0) };
            return new PatchLoadResult(null, null, results, $"Patch artifact was not found: {patchArtifactId}");
        }

        if (!artifact.Kind.Equals(GamePackagePatchArtifactKinds.PatchV1, StringComparison.OrdinalIgnoreCase))
        {
            var results = new[] { ValidationResult(artifact.Id, "error", "patch.artifact.kind.invalid", "Artifact kind must be game_package_patch_v1.", artifact.Id, 0) };
            return new PatchLoadResult(artifact, null, results, "Artifact kind must be game_package_patch_v1.");
        }

        var validationResults = _operationValidator.ValidatePatchJson(artifact.Id, artifact.Json);
        var document = validationResults.Any(IsError) ? null : _operationValidator.ParsePatchDocument(artifact.Json, artifact.Id).Document;
        return new PatchLoadResult(artifact, document, validationResults, validationResults.Any(IsError) ? "Patch artifact validation failed." : "Patch artifact loaded.");
    }

    private PatchExtractionResult ExtractPatchOperationsFromPreview(GeneratedArtifactRecord previewArtifact)
    {
        try
        {
            var root = JsonNode.Parse(previewArtifact.Json)?.AsObject();
            if (root == null)
            {
                return ExtractionFailure(previewArtifact.Id, "Preview JSON root must be an object.", "patch.preview.json.invalid", previewArtifact.Id);
            }

            var kind = root["kind"]?.GetValue<string>() ?? string.Empty;
            if (!kind.Equals("generator_plan_preview", StringComparison.OrdinalIgnoreCase))
            {
                return ExtractionFailure(previewArtifact.Id, "Preview JSON kind must be generator_plan_preview.", "patch.preview.json.kind.invalid", previewArtifact.Id);
            }

            var planId = root["plan"]?["id"]?.GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(planId))
            {
                planId = previewArtifact.GeneratedBy;
            }

            var operations = new JsonArray();
            if (root["steps"] is JsonArray steps)
            {
                foreach (var step in steps)
                {
                    if (step?["config"]?["package_operations"] is not JsonArray packageOperations)
                    {
                        continue;
                    }

                    foreach (var operation in packageOperations)
                    {
                        operations.Add(operation?.DeepClone());
                    }
                }
            }

            if (operations.Count == 0)
            {
                return ExtractionFailure(previewArtifact.Id, "Preview artifact does not contain explicit package_operations in step configs.", "patch.preview.package_operations.empty", previewArtifact.Id);
            }

            var candidate = new JsonObject
            {
                ["kind"] = GamePackagePatchArtifactKinds.PatchV1,
                ["schema_version"] = SchemaVersion,
                ["source"] = new JsonObject
                {
                    ["plan_id"] = planId,
                    ["preview_artifact_id"] = previewArtifact.Id
                },
                ["operations"] = operations
            }.ToJsonString(PatchJsonOptions);

            var parse = _operationValidator.ParsePatchDocument(candidate, BuildPatchArtifactId(previewArtifact.Id));
            if (parse.ValidationResults.Any(IsError) || parse.Document == null)
            {
                return new PatchExtractionResult(planId ?? previewArtifact.GeneratedBy, Array.Empty<GamePackagePatchOperation>(), parse.ValidationResults, "Explicit package_operations failed patch validation.");
            }

            return new PatchExtractionResult(planId ?? previewArtifact.GeneratedBy, parse.Document.Operations, Array.Empty<GeneratedArtifactValidationResultRecord>(), "Package operations extracted.");
        }
        catch (JsonException ex)
        {
            return ExtractionFailure(previewArtifact.Id, ex.Message, "patch.preview.json.invalid", previewArtifact.Id);
        }
        catch (InvalidOperationException ex)
        {
            return ExtractionFailure(previewArtifact.Id, ex.Message, "patch.preview.json.invalid", previewArtifact.Id);
        }
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidatePatchJson(string artifactId, string json)
    {
        var parse = ParsePatchDocument(json, artifactId);
        if (parse.ValidationResults.Any(IsError) || parse.Document == null)
        {
            return parse.ValidationResults;
        }

        return new[]
        {
            ValidationResult(artifactId, "info", "patch.schema.valid", "Patch artifact matches game_package_patch_v1 schema.", artifactId, 0)
        };
    }

    private static PatchParseResult ParsePatchDocument(string json, string artifactId)
    {
        var results = new List<GeneratedArtifactValidationResultRecord>();
        try
        {
            var root = JsonNode.Parse(json)?.AsObject();
            if (root == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.json.root.invalid", "Patch JSON root must be an object.", artifactId, results.Count));
                return new PatchParseResult(null, results);
            }

            var kind = root["kind"]?.GetValue<string>() ?? string.Empty;
            if (!kind.Equals(GamePackagePatchArtifactKinds.PatchV1, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.kind.invalid", "Patch kind must be game_package_patch_v1.", artifactId, results.Count));
            }

            var schemaVersion = root["schema_version"]?.GetValue<int>() ?? 0;
            if (schemaVersion != SchemaVersion)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.schema_version.invalid", "Patch schema_version must be 1.", artifactId, results.Count));
            }

            var source = root["source"] as JsonObject;
            var planId = source?["plan_id"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var previewArtifactId = source?["preview_artifact_id"]?.GetValue<string>()?.Trim() ?? string.Empty;
            if (source == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.missing", "Patch source is required.", artifactId, results.Count));
            }

            if (string.IsNullOrWhiteSpace(planId))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.plan_id.empty", "Patch source.plan_id is required.", "source.plan_id", results.Count));
            }

            if (string.IsNullOrWhiteSpace(previewArtifactId))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.preview_artifact_id.empty", "Patch source.preview_artifact_id is required.", "source.preview_artifact_id", results.Count));
            }

            if (root["operations"] is not JsonArray operationNodes)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operations.missing", "Patch operations array is required.", "operations", results.Count));
                return new PatchParseResult(null, results);
            }

            if (operationNodes.Count == 0)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operations.empty", "Patch operations array must not be empty.", "operations", results.Count));
            }

            var operations = new List<GamePackagePatchOperation>();
            var seenTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < operationNodes.Count; index++)
            {
                if (operationNodes[index] is not JsonObject operationNode)
                {
                    results.Add(ValidationResult(artifactId, "error", "patch.operation.invalid", "Patch operation must be an object.", $"operations[{index}]", results.Count));
                    continue;
                }

                var operation = ParseOperation(operationNode, artifactId, index, results);
                if (operation == null)
                {
                    continue;
                }

                var key = $"{operation.Op}:{operation.Target}";
                if (!seenTargets.Add(key))
                {
                    results.Add(ValidationResult(artifactId, "error", "patch.operation.duplicate_target", $"Duplicate operation target: {key}", operation.Target, results.Count));
                    continue;
                }

                operations.Add(operation);
            }

            if (results.Any(IsError))
            {
                return new PatchParseResult(null, results);
            }

            return new PatchParseResult(new GamePackagePatchDocument(
                GamePackagePatchArtifactKinds.PatchV1,
                SchemaVersion,
                new GamePackagePatchSource(planId, previewArtifactId),
                operations), results);
        }
        catch (JsonException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new PatchParseResult(null, results);
        }
        catch (InvalidOperationException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new PatchParseResult(null, results);
        }
    }

    private static GamePackagePatchOperation? ParseOperation(JsonObject operationNode, string artifactId, int index, List<GeneratedArtifactValidationResultRecord> results)
    {
        var op = operationNode["op"]?.GetValue<string>()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(op))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.op.empty", "Operation op is required.", $"operations[{index}].op", results.Count));
            return null;
        }

        if (op.Contains("delete", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.delete.unsupported", "Delete operations are not supported in this goal.", $"operations[{index}]", results.Count));
            return null;
        }

        switch (op)
        {
            case "upsert_tile_prototype":
                return ParseTileOperation(operationNode, artifactId, index, results);
            case "upsert_map":
                return ParseMapOperation(operationNode, artifactId, index, results);
            case "upsert_entity_prototype":
                return ParseEntityOperation(operationNode, artifactId, index, results);
            case "update_manifest":
                return ParseManifestOperation(operationNode, artifactId, index, results);
            default:
                results.Add(ValidationResult(artifactId, "error", "patch.operation.op.unknown", $"Unsupported patch operation: {op}", $"operations[{index}].op", results.Count));
                return null;
        }
    }

    private static GamePackagePatchOperation? ParseTileOperation(JsonObject node, string artifactId, int index, List<GeneratedArtifactValidationResultRecord> results)
    {
        var id = RequiredString(node, "id", artifactId, index, results);
        var name = RequiredString(node, "name", artifactId, index, results);
        var walkable = RequiredBool(node, "walkable", artifactId, index, results);
        var movementCost = RequiredDouble(node, "movement_cost", artifactId, index, results);
        var assetId = OptionalString(node, "asset_id");
        if (movementCost <= 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.tile.movement_cost.invalid", "Tile movement_cost must be positive.", id ?? $"operations[{index}]", results.Count));
        }

        return id == null || name == null || walkable == null || movementCost == null
            ? null
            : new UpsertTilePrototypePatchOperation(id, name, walkable.Value, movementCost.Value, assetId);
    }

    private static GamePackagePatchOperation? ParseMapOperation(JsonObject node, string artifactId, int index, List<GeneratedArtifactValidationResultRecord> results)
    {
        var id = RequiredString(node, "id", artifactId, index, results);
        var name = RequiredString(node, "name", artifactId, index, results);
        var width = RequiredInt(node, "width", artifactId, index, results);
        var height = RequiredInt(node, "height", artifactId, index, results);
        var defaultTileId = RequiredString(node, "default_tile_id", artifactId, index, results);
        var startX = RequiredInt(node, "start_x", artifactId, index, results);
        var startY = RequiredInt(node, "start_y", artifactId, index, results);
        if (width <= 0 || height <= 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.map.size.invalid", "Map width and height must be positive.", id ?? $"operations[{index}]", results.Count));
        }

        if (width != null && height != null && startX != null && startY != null && (startX < 0 || startY < 0 || startX >= width || startY >= height))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.map.start_position.out_of_bounds", "Map start position must be inside map bounds.", id ?? $"operations[{index}]", results.Count));
        }

        return id == null || name == null || width == null || height == null || defaultTileId == null || startX == null || startY == null
            ? null
            : new UpsertMapPatchOperation(id, name, width.Value, height.Value, defaultTileId, startX.Value, startY.Value);
    }

    private static GamePackagePatchOperation? ParseEntityOperation(JsonObject node, string artifactId, int index, List<GeneratedArtifactValidationResultRecord> results)
    {
        var id = RequiredString(node, "id", artifactId, index, results);
        var name = RequiredString(node, "name", artifactId, index, results);
        var assetId = OptionalString(node, "asset_id");
        return id == null || name == null ? null : new UpsertEntityPrototypePatchOperation(id, name, assetId);
    }

    private static GamePackagePatchOperation? ParseManifestOperation(JsonObject node, string artifactId, int index, List<GeneratedArtifactValidationResultRecord> results)
    {
        var title = OptionalString(node, "title");
        var description = OptionalString(node, "description");
        var version = OptionalString(node, "version");
        var startMapId = OptionalString(node, "start_map_id");
        if (title == null && description == null && version == null && startMapId == null)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.manifest.empty", "update_manifest must set at least one supported field.", $"operations[{index}]", results.Count));
            return null;
        }

        return new UpdateManifestPatchOperation(title, description, version, startMapId);
    }

    private static void ApplyPatch(GamePackagePatchDocument document, GamePackageDefinition package, List<GamePackagePatchDiffLine> diffLines, List<ValidationIssue> issues)
    {
        foreach (var operation in document.Operations)
        {
            switch (operation)
            {
                case UpsertTilePrototypePatchOperation tile:
                    UpsertTile(package, tile, diffLines);
                    break;
                case UpsertMapPatchOperation map:
                    if (!package.Game.TilePrototypes.Any(tile => IdEquals(tile.Id, map.DefaultTileId)))
                    {
                        issues.Add(Issue("patch.map.default_tile.missing", $"Default tile does not exist: {map.DefaultTileId}", map.Id));
                        diffLines.Add(ErrorDiff(map.Op, map.Id, $"Default tile does not exist: {map.DefaultTileId}"));
                    }
                    else if (map.StartX < 0 || map.StartY < 0 || map.StartX >= map.Width || map.StartY >= map.Height)
                    {
                        issues.Add(Issue("patch.map.start_position.out_of_bounds", "Map start position must be inside map bounds.", map.Id));
                        diffLines.Add(ErrorDiff(map.Op, map.Id, "Map start position must be inside map bounds."));
                    }
                    else
                    {
                        UpsertMap(package, map, diffLines);
                    }
                    break;
                case UpsertEntityPrototypePatchOperation entity:
                    UpsertEntity(package, entity, diffLines);
                    break;
                case UpsertItemPrototypePatchOperation item:
                    UpsertDefinition(package.Game.Items, item.Item, item.Op, item.Target, "item prototype", diffLines);
                    break;
                case UpsertResourcePatchOperation resource:
                    UpsertDefinition(package.Game.Resources, resource.Resource, resource.Op, resource.Target, "resource", diffLines);
                    break;
                case UpsertStatusPatchOperation status:
                    UpsertDefinition(package.Game.Statuses, status.Status, status.Op, status.Target, "status", diffLines);
                    break;
                case UpsertRecipePatchOperation recipe:
                    UpsertDefinition(package.Game.Recipes, recipe.Recipe, recipe.Op, recipe.Target, "recipe", diffLines);
                    break;
                case UpsertLootTablePatchOperation lootTable:
                    UpsertDefinition(package.Game.LootTables, lootTable.LootTable, lootTable.Op, lootTable.Target, "loot table", diffLines);
                    break;
                case UpsertTransactionPatchOperation transaction:
                    UpsertDefinition(package.Game.Transactions, transaction.Transaction, transaction.Op, transaction.Target, "transaction", diffLines);
                    break;
                case UpsertResourceNetworkPatchOperation resourceNetwork:
                    UpsertDefinition(package.Game.ResourceNetworks, resourceNetwork.ResourceNetwork, resourceNetwork.Op, resourceNetwork.Target, "resource network", diffLines);
                    break;
                case UpsertResourceNodePatchOperation resourceNode:
                    UpsertDefinition(package.Game.ResourceNodes, resourceNode.ResourceNode, resourceNode.Op, resourceNode.Target, "resource node", diffLines);
                    break;
                case UpsertInventoryPatchOperation inventory:
                    UpsertDefinition(package.Game.Inventories, inventory.Inventory, inventory.Op, inventory.Target, "inventory", diffLines);
                    break;
                case UpsertEquipmentSlotPatchOperation equipmentSlot:
                    UpsertDefinition(package.Game.EquipmentSlots, equipmentSlot.EquipmentSlot, equipmentSlot.Op, equipmentSlot.Target, "equipment slot", diffLines);
                    break;
                case UpdateManifestPatchOperation manifest:
                    if (!string.IsNullOrWhiteSpace(manifest.StartMapId) && !package.Game.Maps.Any(map => IdEquals(map.Id, manifest.StartMapId)))
                    {
                        issues.Add(Issue("patch.manifest.start_map.missing", $"Start map does not exist: {manifest.StartMapId}", manifest.StartMapId));
                        diffLines.Add(ErrorDiff(manifest.Op, manifest.Target, $"Start map does not exist: {manifest.StartMapId}"));
                    }
                    else
                    {
                        UpdateManifest(package, manifest, diffLines);
                    }
                    break;
            }
        }
    }

    private static void UpsertTile(GamePackageDefinition package, UpsertTilePrototypePatchOperation operation, List<GamePackagePatchDiffLine> diffLines)
    {
        var existing = package.Game.TilePrototypes.FirstOrDefault(tile => IdEquals(tile.Id, operation.Id));
        var before = existing == null ? string.Empty : ToJson(existing);
        var replacement = new TilePrototypeDefinition
        {
            Id = operation.Id,
            Name = operation.Name,
            Walkable = operation.Walkable,
            MovementCost = operation.MovementCost,
            AssetId = NormalizeNullable(operation.AssetId)
        };
        var after = ToJson(replacement);
        if (existing == null)
        {
            package.Game.TilePrototypes.Add(replacement);
            diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, "add", before, after, $"Add tile prototype {operation.Id}."));
            return;
        }

        var changeKind = before == after ? "no_change" : "update";
        existing.Name = replacement.Name;
        existing.Walkable = replacement.Walkable;
        existing.MovementCost = replacement.MovementCost;
        existing.AssetId = replacement.AssetId;
        diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, changeKind, before, after, $"{ChangeVerb(changeKind)} tile prototype {operation.Id}."));
    }

    private static void UpsertMap(GamePackageDefinition package, UpsertMapPatchOperation operation, List<GamePackagePatchDiffLine> diffLines)
    {
        var existing = package.Game.Maps.FirstOrDefault(map => IdEquals(map.Id, operation.Id));
        var before = existing == null ? string.Empty : ToJson(existing);
        var replacement = new MapDefinition
        {
            Id = operation.Id,
            Name = operation.Name,
            Width = operation.Width,
            Height = operation.Height,
            DefaultTileId = operation.DefaultTileId,
            StartPosition = new Position2D(operation.StartX, operation.StartY)
        };
        var after = ToJson(replacement);
        if (existing == null)
        {
            package.Game.Maps.Add(replacement);
            diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, "add", before, after, $"Add map {operation.Id}."));
            return;
        }

        var changeKind = before == after ? "no_change" : "update";
        existing.Name = replacement.Name;
        existing.Width = replacement.Width;
        existing.Height = replacement.Height;
        existing.DefaultTileId = replacement.DefaultTileId;
        existing.StartPosition = replacement.StartPosition;
        diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, changeKind, before, after, $"{ChangeVerb(changeKind)} map {operation.Id}."));
    }

    private static void UpsertEntity(GamePackageDefinition package, UpsertEntityPrototypePatchOperation operation, List<GamePackagePatchDiffLine> diffLines)
    {
        var existing = package.Game.EntityPrototypes.FirstOrDefault(entity => IdEquals(entity.Id, operation.Id));
        var before = existing == null ? string.Empty : ToJson(existing);
        var replacement = new EntityPrototypeDefinition
        {
            Id = operation.Id,
            Name = operation.Name,
            AssetId = NormalizeNullable(operation.AssetId)
        };
        var after = ToJson(replacement);
        if (existing == null)
        {
            package.Game.EntityPrototypes.Add(replacement);
            diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, "add", before, after, $"Add entity prototype {operation.Id}."));
            return;
        }

        var changeKind = before == after ? "no_change" : "update";
        existing.Name = replacement.Name;
        existing.AssetId = replacement.AssetId;
        diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Id, changeKind, before, after, $"{ChangeVerb(changeKind)} entity prototype {operation.Id}."));
    }

    private static void UpsertDefinition<T>(List<T> definitions, T replacement, string operation, string target, string label, List<GamePackagePatchDiffLine> diffLines)
    {
        var existingIndex = definitions.FindIndex(definition => IdEquals(GetId(definition), target));
        var before = existingIndex < 0 ? string.Empty : ToJson(definitions[existingIndex]);
        var after = ToJson(replacement);
        if (existingIndex < 0)
        {
            definitions.Add(replacement);
            diffLines.Add(new GamePackagePatchDiffLine(operation, target, "add", before, after, $"Add {label} {target}."));
            return;
        }

        var changeKind = before == after ? "no_change" : "update";
        definitions[existingIndex] = replacement;
        diffLines.Add(new GamePackagePatchDiffLine(operation, target, changeKind, before, after, $"{ChangeVerb(changeKind)} {label} {target}."));
    }

    private static void UpdateManifest(GamePackageDefinition package, UpdateManifestPatchOperation operation, List<GamePackagePatchDiffLine> diffLines)
    {
        var before = ToJson(package.Manifest);
        if (operation.Title != null)
        {
            package.Manifest.Title = operation.Title;
        }

        if (operation.Description != null)
        {
            package.Manifest.Description = operation.Description;
        }

        if (operation.Version != null)
        {
            package.Manifest.Version = operation.Version;
        }

        if (operation.StartMapId != null)
        {
            package.Manifest.StartMapId = operation.StartMapId;
        }

        var after = ToJson(package.Manifest);
        var changeKind = before == after ? "no_change" : "update";
        diffLines.Add(new GamePackagePatchDiffLine(operation.Op, operation.Target, changeKind, before, after, $"{ChangeVerb(changeKind)} manifest."));
    }

    private static string SerializePatchDocument(GamePackagePatchDocument document)
    {
        var operations = new JsonArray();
        foreach (var operation in document.Operations)
        {
            operations.Add(OperationToJson(operation));
        }

        return new JsonObject
        {
            ["kind"] = document.Kind,
            ["schema_version"] = document.SchemaVersion,
            ["source"] = new JsonObject
            {
                ["plan_id"] = document.Source.PlanId,
                ["preview_artifact_id"] = document.Source.PreviewArtifactId
            },
            ["operations"] = operations
        }.ToJsonString(PatchJsonOptions);
    }

    private static JsonObject OperationToJson(GamePackagePatchOperation operation)
    {
        switch (operation)
        {
            case UpsertTilePrototypePatchOperation tile:
                var tileJson = new JsonObject
                {
                    ["op"] = tile.Op,
                    ["id"] = tile.Id,
                    ["name"] = tile.Name,
                    ["walkable"] = tile.Walkable,
                    ["movement_cost"] = tile.MovementCost
                };
                if (!string.IsNullOrWhiteSpace(tile.AssetId))
                {
                    tileJson["asset_id"] = tile.AssetId;
                }

                return tileJson;
            case UpsertMapPatchOperation map:
                return new JsonObject
                {
                    ["op"] = map.Op,
                    ["id"] = map.Id,
                    ["name"] = map.Name,
                    ["width"] = map.Width,
                    ["height"] = map.Height,
                    ["default_tile_id"] = map.DefaultTileId,
                    ["start_x"] = map.StartX,
                    ["start_y"] = map.StartY
                };
            case UpsertEntityPrototypePatchOperation entity:
                var entityJson = new JsonObject
                {
                    ["op"] = entity.Op,
                    ["id"] = entity.Id,
                    ["name"] = entity.Name
                };
                if (!string.IsNullOrWhiteSpace(entity.AssetId))
                {
                    entityJson["asset_id"] = entity.AssetId;
                }

                return entityJson;
            case UpsertItemPrototypePatchOperation item:
                return DefinitionOperationToJson(item.Op, item.Item);
            case UpsertResourcePatchOperation resource:
                return DefinitionOperationToJson(resource.Op, resource.Resource);
            case UpsertStatusPatchOperation status:
                return DefinitionOperationToJson(status.Op, status.Status);
            case UpsertRecipePatchOperation recipe:
                return DefinitionOperationToJson(recipe.Op, recipe.Recipe);
            case UpsertLootTablePatchOperation lootTable:
                return DefinitionOperationToJson(lootTable.Op, lootTable.LootTable);
            case UpsertTransactionPatchOperation transaction:
                return DefinitionOperationToJson(transaction.Op, transaction.Transaction);
            case UpsertResourceNetworkPatchOperation resourceNetwork:
                return DefinitionOperationToJson(resourceNetwork.Op, resourceNetwork.ResourceNetwork);
            case UpsertResourceNodePatchOperation resourceNode:
                return DefinitionOperationToJson(resourceNode.Op, resourceNode.ResourceNode);
            case UpsertInventoryPatchOperation inventory:
                return DefinitionOperationToJson(inventory.Op, inventory.Inventory);
            case UpsertEquipmentSlotPatchOperation equipmentSlot:
                return DefinitionOperationToJson(equipmentSlot.Op, equipmentSlot.EquipmentSlot);
            case UpdateManifestPatchOperation manifest:
                var manifestJson = new JsonObject
                {
                    ["op"] = manifest.Op
                };
                if (manifest.Title != null)
                {
                    manifestJson["title"] = manifest.Title;
                }

                if (manifest.Description != null)
                {
                    manifestJson["description"] = manifest.Description;
                }

                if (manifest.Version != null)
                {
                    manifestJson["version"] = manifest.Version;
                }

                if (manifest.StartMapId != null)
                {
                    manifestJson["start_map_id"] = manifest.StartMapId;
                }

                return manifestJson;
            default:
                throw new InvalidOperationException($"Unsupported patch operation type: {operation.GetType().Name}");
        }
    }

    private static JsonObject DefinitionOperationToJson<T>(string op, T definition)
    {
        var json = JsonSerializer.SerializeToNode(definition, PatchJsonOptions)?.AsObject()
            ?? throw new InvalidOperationException($"Failed to serialize {typeof(T).Name}.");
        json["op"] = op;
        return json;
    }

    private static string? GetId<T>(T definition)
    {
        return definition switch
        {
            ItemDefinition item => item.Id,
            ResourceDefinition resource => resource.Id,
            StatusDefinition status => status.Id,
            RecipeDefinition recipe => recipe.Id,
            LootTableDefinition lootTable => lootTable.Id,
            TransactionDefinition transaction => transaction.Id,
            ResourceNetworkDefinition resourceNetwork => resourceNetwork.Id,
            ResourceNodeDefinition resourceNode => resourceNode.Id,
            InventoryDefinition inventory => inventory.Id,
            EquipmentSlotDefinition equipmentSlot => equipmentSlot.Id,
            _ => null
        };
    }

    private static GeneratedArtifactRecord BuildApplyAuditArtifact(
        GeneratedArtifactRecord patchArtifact,
        string backupPath,
        IReadOnlyList<GamePackagePatchDiffLine> diffLines,
        IReadOnlyList<ValidationIssue> validationIssues)
    {
        var artifactId = $"{patchArtifact.Id}/apply/{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        var errorCount = validationIssues.Count(issue => issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical);
        var warningCount = validationIssues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        var json = JsonSerializer.Serialize(new
        {
            kind = GamePackagePatchArtifactKinds.ApplyResultV1,
            schema_version = SchemaVersion,
            patch_artifact_id = patchArtifact.Id,
            applied_utc = DateTimeOffset.UtcNow.ToString("O"),
            backup_path = backupPath,
            diff_lines = diffLines,
            validation_issue_count = validationIssues.Count
        }, PatchJsonOptions);

        return new GeneratedArtifactRecord(
            artifactId,
            GamePackagePatchArtifactKinds.ApplyResultV1,
            $"design-db://generated-artifacts/{artifactId}",
            json,
            patchArtifact.Id,
            errorCount > 0 ? "invalid" : warningCount > 0 ? "warning" : "valid",
            JsonSerializer.Serialize(new
            {
                created_utc = DateTimeOffset.UtcNow.ToString("O"),
                patch_artifact_id = patchArtifact.Id,
                backup_path = backupPath,
                diff_count = diffLines.Count,
                warning_count = warningCount,
                error_count = errorCount
            }, PatchJsonOptions));
    }

    private static string CreateRollbackSnapshot(string projectFolder)
    {
        var projectRoot = Path.GetFullPath(projectFolder);
        var packagePath = Path.GetFullPath(Path.Combine(projectRoot, "package.json"));
        if (!packagePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved package path is outside the current project folder.");
        }

        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException("package.json was not found for rollback snapshot.", packagePath);
        }

        var backupDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "backups"));
        if (!backupDirectory.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved backup path is outside the current project folder.");
        }

        Directory.CreateDirectory(backupDirectory);
        var backupPath = Path.Combine(backupDirectory, $"package-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}.json");
        File.Copy(packagePath, backupPath, overwrite: false);
        return backupPath;
    }

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package)
    {
        var json = JsonSerializer.Serialize(package, PackageJsonOptions);
        return JsonSerializer.Deserialize<GamePackageDefinition>(json, PackageJsonOptions)
            ?? throw new InvalidOperationException("Failed to clone current GamePackage.");
    }

    private static string? RequiredString(JsonObject node, string propertyName, string artifactId, int operationIndex, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = OptionalString(node, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
            return null;
        }

        return value;
    }

    private static string? OptionalString(JsonObject node, string propertyName)
    {
        var value = node[propertyName]?.GetValue<string>()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool? RequiredBool(JsonObject node, string propertyName, string artifactId, int operationIndex, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<bool>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be boolean: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
            return null;
        }
    }

    private static int? RequiredInt(JsonObject node, string propertyName, string artifactId, int operationIndex, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<int>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be integer: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
            return null;
        }
    }

    private static double? RequiredDouble(JsonObject node, string propertyName, string artifactId, int operationIndex, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<double>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be numeric: {propertyName}", $"operations[{operationIndex}].{propertyName}", results.Count));
            return null;
        }
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToArtifactValidationResults(string artifactId, IReadOnlyList<ValidationIssue> issues)
    {
        return issues
            .Select((issue, index) => ValidationResult(
                artifactId,
                SeverityToString(issue.Severity),
                issue.Code,
                issue.Message,
                issue.TargetId ?? artifactId,
                index))
            .ToList();
    }

    private static IReadOnlyList<ValidationIssue> ToValidationIssues(IReadOnlyList<GeneratedArtifactValidationResultRecord> results)
    {
        return results
            .Select(result => Issue(result.Code, result.Message, result.Target, StringToSeverity(result.Severity)))
            .ToList();
    }

    private static ValidationIssue Issue(string code, string message, string? target, ValidationSeverity severity = ValidationSeverity.Error)
    {
        return new ValidationIssue
        {
            Code = code,
            Message = message,
            TargetId = target,
            Category = "GamePackagePatch",
            Severity = severity
        };
    }

    private static GeneratedArtifactValidationResultRecord ValidationResult(string artifactId, string severity, string code, string message, string target, int index)
    {
        return new GeneratedArtifactValidationResultRecord(
            BuildValidationResultId(artifactId, code, index),
            artifactId,
            severity,
            code,
            message,
            target,
            "{}");
    }

    private static GamePackagePatchDiffLine ErrorDiff(string operation, string target, string message)
    {
        return new GamePackagePatchDiffLine(operation, target, "error", string.Empty, string.Empty, message);
    }

    private static string ToJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, PackageJsonOptions);
    }

    private static bool IsError(GeneratedArtifactValidationResultRecord result)
    {
        return result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            || result.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase);
    }

    private static string SeverityToString(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Critical => "critical",
            ValidationSeverity.Error => "error",
            ValidationSeverity.Warning => "warning",
            _ => "info"
        };
    }

    private static ValidationSeverity StringToSeverity(string severity)
    {
        return severity.Equals("critical", StringComparison.OrdinalIgnoreCase)
            ? ValidationSeverity.Critical
            : severity.Equals("warning", StringComparison.OrdinalIgnoreCase)
                ? ValidationSeverity.Warning
                : severity.Equals("info", StringComparison.OrdinalIgnoreCase)
                    ? ValidationSeverity.Info
                    : ValidationSeverity.Error;
    }

    private static string ValidationState(IReadOnlyList<GeneratedArtifactValidationResultRecord> validationResults)
    {
        if (validationResults.Any(result => result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            return "invalid";
        }

        return validationResults.Any(result => result.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "valid";
    }

    private static string BuildPatchArtifactId(string previewArtifactId)
    {
        return $"artifact/game-package-patch/{previewArtifactId.Replace("artifact/", string.Empty, StringComparison.OrdinalIgnoreCase)}";
    }

    private static string BuildValidationResultId(string artifactId, string code, int index)
    {
        return $"{artifactId}/validation/{index.ToString("D3", CultureInfo.InvariantCulture)}/{code}";
    }

    private static string ChangeVerb(string changeKind)
    {
        return changeKind.Equals("no_change", StringComparison.OrdinalIgnoreCase) ? "No change for" : "Update";
    }

    private static string? NormalizeNullable(string? value)
    {
        var text = value?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }

    private static GamePackagePatchCreateResult CreateFailure(GeneratedArtifactRecord? previewArtifact, string message, string code, string target)
    {
        var artifactId = previewArtifact?.Id ?? "artifact/game-package-patch/missing";
        return new GamePackagePatchCreateResult(
            previewArtifact,
            null,
            new[] { ValidationResult(artifactId, "error", code, message, target, 0) },
            false,
            message);
    }

    private static PatchExtractionResult ExtractionFailure(string artifactId, string message, string code, string target)
    {
        return new PatchExtractionResult(
            string.Empty,
            Array.Empty<GamePackagePatchOperation>(),
            new[] { ValidationResult(artifactId, "error", code, message, target, 0) },
            message);
    }

    private sealed record PatchParseResult(GamePackagePatchDocument? Document, IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults);

    private sealed record PatchLoadResult(
        GeneratedArtifactRecord? Artifact,
        GamePackagePatchDocument? Document,
        IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults,
        string Message);

    private sealed record PatchExtractionResult(
        string PlanId,
        IReadOnlyList<GamePackagePatchOperation> Operations,
        IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults,
        string Message);
}
