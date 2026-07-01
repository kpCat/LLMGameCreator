using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;

public sealed class SettlementConstructionDestructionProductionSourceLoader
{
    private const string Goal060Root = SettlementConstructionDestructionProductionVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal061Root = SettlementConstructionDestructionProductionVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal062Root = SettlementConstructionDestructionProductionVocabulary.Goal062RelativeOutputDirectory;
    private const string Goal063Root = SettlementConstructionDestructionProductionVocabulary.Goal063RelativeOutputDirectory;
    private const string Goal064Root = SettlementConstructionDestructionProductionVocabulary.Goal064RelativeOutputDirectory;
    private const string Goal065Root = SettlementConstructionDestructionProductionVocabulary.Goal065RelativeOutputDirectory;

    public SettlementSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<SettlementDiagnostic>();
        var sourceRefs = new List<SettlementSourceArtifactReference>();

        var goal060Packages = ReadRows(projectRoot, Goal060Root + "/materialized-package-inventory.json", "packages", "Goal060", "materialized_package_inventory", ["packageCount", "packages"], sourceRefs, diagnostics);
        var goal060Runtime = ReadRows(projectRoot, Goal060Root + "/runtime-consumption-matrix.json", "rows", "Goal060", "runtime_consumption_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal061Rows = ReadRows(projectRoot, Goal061Root + "/package-row-selection-matrix.json", "rows", "Goal061", "package_row_selection_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal061SaveLoad = ReadRows(projectRoot, Goal061Root + "/save-load-replay-package-row-audit.json", "rows", "Goal061", "save_load_replay_audit", ["passed", "rows"], sourceRefs, diagnostics);
        var goal062Spatial = ReadRows(projectRoot, Goal062Root + "/spatial-detail-matrix.json", "rows", "Goal062", "spatial_detail_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal063Runtime = ReadRows(projectRoot, Goal063Root + "/runtime-state-delta-matrix.json", "rows", "Goal063", "runtime_state_delta_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal064Plan = ReadRows(projectRoot, Goal064Root + "/simulation-matrix-plan.json", "rows", "Goal064", "living_world_simulation_matrix_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Rows = ReadRows(projectRoot, Goal065Root + "/row-plan-matrix.json", "rows", "Goal065", "interlocked_row_plan_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Replay = ReadRows(projectRoot, Goal065Root + "/save-load-replay-proof.json", "rows", "Goal065", "interlocked_save_load_replay_proof", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRows(projectRoot, Goal065Root + "/unity-player-proof.json", "matchedMarkers", "Goal065", "interlocked_unity_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);

        var goal065Accepted = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            .Contains("interlocked_gameplay_systems_depth_matrix_verification passed before Goal 066", StringComparison.Ordinal);
        if (!goal065Accepted)
        {
            diagnostics.Add(Error("goal066.preflight.goal065_handoff_missing", "docs/CURRENT_GENERATOR_STATE.md", "Goal 065 user-handoff acceptance must be recorded before Goal 066."));
        }

        var runtime060ByRow = goal060Runtime.ToDictionary(RowId, StringComparer.Ordinal);
        var reviewByRow = goal061Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var save061ByRow = goal061SaveLoad.ToDictionary(RowId, StringComparer.Ordinal);
        var spatialByRow = goal062Spatial.ToDictionary(RowId, StringComparer.Ordinal);
        var gameplayByRow = goal063Runtime.ToDictionary(RowId, StringComparer.Ordinal);
        var livingByRow = goal064Plan.ToDictionary(RowId, StringComparer.Ordinal);
        var interlockedByRow = goal065Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var replay065ByRow = goal065Replay.ToDictionary(RowId, StringComparer.Ordinal);

        var rows = new List<SettlementSourceRow>();
        foreach (var package in goal060Packages)
        {
            var rowId = RowId(package);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal066.source.goal060_row_id_missing", Goal060Root + "/materialized-package-inventory.json", "Goal 060 package inventory row id is required."));
                continue;
            }

            var familyId = Text(package, "familyId");
            var seedId = Text(package, "seedId");
            if (!SettlementConstructionDestructionProductionVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal066.source.fake_family_id", familyId, "Goal 066 accepts only the three proven family ids."));
            }

            if (!SettlementConstructionDestructionProductionVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal066.source.fake_seed_id", seedId, "Goal 066 accepts only seed_alpha, seed_beta and seed_gamma."));
            }

            if (!reviewByRow.TryGetValue(rowId, out var review))
            {
                diagnostics.Add(Error("goal066.source.goal061_row_missing", rowId, "Goal 061 review package row is required."));
                continue;
            }

            if (!spatialByRow.TryGetValue(rowId, out var spatial))
            {
                diagnostics.Add(Error("goal066.source.goal062_row_missing", rowId, "Goal 062 spatial detail row is required."));
                continue;
            }

            if (!gameplayByRow.TryGetValue(rowId, out var gameplay))
            {
                diagnostics.Add(Error("goal066.source.goal063_row_missing", rowId, "Goal 063 gameplay consequence row is required."));
                continue;
            }

            if (!livingByRow.TryGetValue(rowId, out var living))
            {
                diagnostics.Add(Error("goal066.source.goal064_row_missing", rowId, "Goal 064 living-world row is required."));
                continue;
            }

            if (!interlockedByRow.TryGetValue(rowId, out var interlocked))
            {
                diagnostics.Add(Error("goal066.source.goal065_row_missing", rowId, "Goal 065 interlocked gameplay row is required."));
                continue;
            }

            runtime060ByRow.TryGetValue(rowId, out var runtime060);
            save061ByRow.TryGetValue(rowId, out var save061);
            replay065ByRow.TryGetValue(rowId, out var replay065);

            sourceRefs.Add(FileRef(projectRoot, "Goal062", "spatial_detail_row", Goal062Root + "/spatial-detail-row-" + familyId + "-" + seedId + ".json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal064", "living_world_row", Goal064Root + "/rows/" + familyId + "-" + seedId + "-living-world-row.json", ["rowId", "familyId", "seedId", "rowHash", "afterState"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal065", "interlocked_row", Goal065Root + "/row-" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + ".json", ["rowId", "familyId", "seedId", "rowHash", "afterState"]));

            rows.Add(new SettlementSourceRow
            {
                RowId = rowId,
                FamilyId = familyId,
                SeedId = seedId,
                SourcePackageRowRef = "Goal060:" + rowId,
                SourceReviewPackageRowRef = "Goal061:" + rowId,
                SourceSpatialDetailRowRef = "Goal062:" + rowId,
                SourceGameplayConsequenceRowRef = "Goal063:" + rowId,
                SourceLivingWorldRowRef = "Goal064:" + rowId,
                SourceInterlockedGameplayRowRef = "Goal065:" + rowId,
                PackageId = Text(package, "packageId"),
                PackageHash = Text(package, "packageHash"),
                PackageRelativePath = Text(package, "packageRelativePath"),
                ReviewPackageRelativePath = Text(review, "packageRelativePath"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                SpatialVarianceMarker = Text(spatial, "varianceMarker"),
                LivingWorldRowHash = Text(living, "rowHash"),
                LivingWorldAfterStateHash = NestedText(living, "afterState", "stateHash"),
                InterlockedRowHash = Text(interlocked, "rowHash"),
                InterlockedAfterStateHash = NestedText(interlocked, "afterState", "stateHash"),
                Goal060RuntimeStateChanged = Bool(runtime060, "stateChanged"),
                Goal061SaveLoadReplayVerified = Bool(review, "saveLoadReplayVerified")
                    && (save061.ValueKind == JsonValueKind.Undefined || (Bool(save061, "saveLoadRoundtripPassed") && Bool(save061, "replayDeterminismPassed"))),
                Goal062Reachable = Bool(spatial, "reachable"),
                Goal062RouteVerified = Bool(spatial, "routeVerified"),
                Goal063StateChanging = Int(gameplay, "stateChangingStepCount") >= 3,
                Goal064StateChanging = !string.Equals(NestedText(living, "beforeState", "stateHash"), NestedText(living, "afterState", "stateHash"), StringComparison.Ordinal)
                    && ArrayCount(living, "stateDeltaSummary") >= 8,
                Goal065StateChanging = Bool(interlocked, "stateChanging")
                    && !string.Equals(NestedText(interlocked, "beforeState", "stateHash"), NestedText(interlocked, "afterState", "stateHash"), StringComparison.Ordinal),
                Goal065SaveLoadReplayPassed = replay065.ValueKind != JsonValueKind.Undefined
                    && Bool(replay065, "beforeAfterStateChanged")
                    && Bool(replay065, "saveLoadRoundtripPassed")
                    && Bool(replay065, "replayDeterminismPassed"),
                Goal064ActorIds = ReadNestedStringArray(living, "actorRecords", "actorId"),
                Goal064FactionIds = ReadNestedStringArray(living, "factionRecords", "factionId"),
                Goal064EventIds = ReadNestedStringArray(living, "worldEventRecords", "eventId"),
                Goal065EconomyDeltaIds = ReadDeltaIds(interlocked, "economy"),
                Goal065CraftingDeltaIds = ReadDeltaIds(interlocked, "crafting"),
                Goal065CombatDeltaIds = ReadDeltaIds(interlocked, "combat"),
                Goal065ProgressionDeltaIds = ReadDeltaIds(interlocked, "progression"),
                Goal065StatusDeltaIds = ReadDeltaIds(interlocked, "status")
            });
        }

        var orderedRows = rows
            .OrderBy(item => SettlementConstructionDestructionProductionVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SettlementConstructionDestructionProductionVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new SettlementSourceBundle
        {
            Goal065AcceptedByUserHandoff = goal065Accepted,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => !string.IsNullOrWhiteSpace(item.PackageHash) && item.Goal060RuntimeStateChanged),
            Goal061ReviewRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061SaveLoadReplayVerified),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062Reachable && item.Goal062RouteVerified && !string.IsNullOrWhiteSpace(item.SpatialDetailRowHash)),
            Goal063GameplayRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal063StateChanging),
            Goal064LivingWorldRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal064StateChanging && !string.IsNullOrWhiteSpace(item.LivingWorldRowHash)),
            Goal065InterlockedRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal065StateChanging && item.Goal065SaveLoadReplayPassed && !string.IsNullOrWhiteSpace(item.InterlockedRowHash)),
            Goal065UnityProofConsumed = sourceRefs.Any(item => item.ArtifactFamily == "interlocked_unity_proof_summary" && item.Exists && item.HashMatches),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(SettlementConstructionDestructionProductionVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(SettlementConstructionDestructionProductionVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadGoal065StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<SettlementDiagnostic> SortDiagnostics(IEnumerable<SettlementDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    public static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static IReadOnlyList<JsonElement> ReadRows(
        string projectRoot,
        string relativePath,
        string arrayProperty,
        string sourceGoal,
        string artifactFamily,
        IReadOnlyList<string> requiredFields,
        List<SettlementSourceArtifactReference> refs,
        List<SettlementDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal066.source.required_artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal066.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray().Select(item => item.Clone()).ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal066.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static SettlementSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<SettlementDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = true;
        if (exists)
        {
            var bytes = File.ReadAllBytes(path);
            hash = SettlementConstructionDestructionProductionHash.HashBytes(bytes);
            var text = Encoding.UTF8.GetString(bytes);
            foreach (var field in requiredFields)
            {
                if (!text.Contains("\"" + field + "\"", StringComparison.Ordinal) && !text.Contains(field, StringComparison.Ordinal))
                {
                    fieldsPresent = false;
                    diagnostics.Add(Error("goal066.source.required_field_missing", normalized + "#" + field, "Required source field is missing."));
                }
            }
        }
        else
        {
            fieldsPresent = false;
            diagnostics.Add(Error("goal066.source.file_missing", normalized, "Required source file is missing."));
        }

        return new SettlementSourceArtifactReference
        {
            SourceGoal = sourceGoal,
            ArtifactFamily = artifactFamily,
            ArtifactRelativePath = normalized,
            ArtifactHash = hash,
            Exists = exists,
            HashMatches = exists && fieldsPresent,
            RequiredFields = requiredFields,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<SettlementFilePayload> LoadGoal065StagingFiles(
        string projectRoot,
        List<SettlementDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal065Root + "/" + SettlementConstructionDestructionProductionVocabulary.StagingRoot);
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal066.source.goal065_staging_missing", Goal065Root + "/staging", "Goal 066 Unity proof requires Goal 065 Alpha staging payload files."));
            return [];
        }

        var result = new List<SettlementFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal066.source.goal065_staging_unsafe_path", relative, "Goal 065 staging file path is not safe for reuse."));
                continue;
            }

            result.Add(new SettlementFilePayload
            {
                RelativePath = Normalize(relative),
                Bytes = File.ReadAllBytes(file)
            });
        }

        if (!result.Any(item => item.RelativePath == "runtime/unity-runtime-config.json"))
        {
            diagnostics.Add(Error("goal066.source.goal065_staging_runtime_config_missing", Goal065Root + "/staging", "Goal 065 staging must include Alpha runtime config."));
        }

        return result.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> ReadDeltaIds(JsonElement row, string category)
    {
        if (row.ValueKind != JsonValueKind.Object || !row.TryGetProperty("deltas", out var deltas) || deltas.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return deltas.EnumerateArray()
            .Where(item => Text(item, "category") == category)
            .Select(item => Text(item, "deltaId"))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> ReadNestedStringArray(JsonElement row, string arrayProperty, string nestedProperty)
    {
        if (row.ValueKind != JsonValueKind.Object || !row.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return array.EnumerateArray()
            .Select(item => Text(item, nestedProperty))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static int ArrayCount(JsonElement row, string arrayProperty)
    {
        if (row.ValueKind != JsonValueKind.Object || !row.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return array.GetArrayLength();
    }

    private static string RowId(JsonElement item) => Text(item, "rowId");

    private static string Text(JsonElement item, string propertyName)
    {
        if (item.ValueKind == JsonValueKind.String && propertyName == "rowId")
        {
            return item.GetString() ?? string.Empty;
        }

        return item.ValueKind == JsonValueKind.Object
            && item.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string NestedText(JsonElement item, string objectProperty, string nestedProperty)
    {
        if (item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty(objectProperty, out var nested)
            || nested.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return Text(nested, nestedProperty);
    }

    private static bool Bool(JsonElement item, string propertyName) =>
        item.ValueKind == JsonValueKind.Object
        && item.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int Int(JsonElement item, string propertyName) =>
        item.ValueKind == JsonValueKind.Object
        && item.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var normalized = Normalize(relativePath);
        var path = Path.GetFullPath(Path.Combine(projectRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static string Normalize(string relativePath) => relativePath.Replace('\\', '/').TrimStart('/');

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static SettlementDiagnostic Error(string code, string target, string message) =>
        SettlementDiagnostic.Error(code, target, message);
}
