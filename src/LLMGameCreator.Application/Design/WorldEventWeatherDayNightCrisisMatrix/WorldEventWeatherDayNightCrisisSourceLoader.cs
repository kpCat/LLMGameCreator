using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;

public sealed class WorldEventWeatherDayNightCrisisSourceLoader
{
    private const string Goal060Root = WorldEventWeatherDayNightCrisisVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal061Root = WorldEventWeatherDayNightCrisisVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal062Root = WorldEventWeatherDayNightCrisisVocabulary.Goal062RelativeOutputDirectory;
    private const string Goal063Root = WorldEventWeatherDayNightCrisisVocabulary.Goal063RelativeOutputDirectory;
    private const string Goal064Root = WorldEventWeatherDayNightCrisisVocabulary.Goal064RelativeOutputDirectory;
    private const string Goal065Root = WorldEventWeatherDayNightCrisisVocabulary.Goal065RelativeOutputDirectory;
    private const string Goal066Root = WorldEventWeatherDayNightCrisisVocabulary.Goal066RelativeOutputDirectory;
    private const string Goal067Root = WorldEventWeatherDayNightCrisisVocabulary.Goal067RelativeOutputDirectory;
    private const string Goal068Root = WorldEventWeatherDayNightCrisisVocabulary.Goal068RelativeOutputDirectory;

    public WorldEventSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<WorldEventDiagnostic>();
        var sourceRefs = new List<WorldEventSourceArtifactReference>();

        var goal060Packages = ReadRows(projectRoot, Goal060Root + "/materialized-package-inventory.json", "packages", "Goal060", "materialized_package_inventory", ["packageCount", "packages"], sourceRefs, diagnostics);
        var goal061Review = ReadRows(projectRoot, Goal061Root + "/review-package-rc-manifest.json", "rows", "Goal061", "review_package_rc_manifest", ["passed", "reviewPackageRcId", "rows"], sourceRefs, diagnostics);
        var goal062Spatial = ReadRows(projectRoot, Goal062Root + "/spatial-detail-matrix.json", "rows", "Goal062", "spatial_detail_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal063Runtime = ReadRows(projectRoot, Goal063Root + "/runtime-state-delta-matrix.json", "rows", "Goal063", "runtime_state_delta_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal064Plan = ReadRows(projectRoot, Goal064Root + "/simulation-matrix-plan.json", "rows", "Goal064", "living_world_simulation_matrix_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Rows = ReadRows(projectRoot, Goal065Root + "/row-plan-matrix.json", "rows", "Goal065", "interlocked_row_plan_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal066Rows = ReadRows(projectRoot, Goal066Root + "/settlement-construction-row-matrix.json", "rows", "Goal066", "settlement_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal067Rows = ReadRows(projectRoot, Goal067Root + "/narrative-row-matrix.json", "rows", "Goal067", "narrative_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal068Rows = ReadRows(projectRoot, Goal068Root + "/combat-magic-row-matrix.json", "rows", "Goal068", "combat_magic_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal068Replay = ReadRows(projectRoot, Goal068Root + "/combat-magic-save-load-replay-proof.json", "rows", "Goal068", "combat_magic_save_load_replay", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRows(projectRoot, Goal068Root + "/combat-magic-unity-command-plan.json", "rows", "Goal068", "combat_magic_unity_command_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal068UnityMarkers = ReadRows(projectRoot, Goal068Root + "/combat-magic-unity-player-proof-summary.json", "matchedMarkers", "Goal068", "combat_magic_unity_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);

        var packageByRow = ToRowMap(goal060Packages);
        var reviewByRow = ToRowMap(goal061Review);
        var spatialByRow = ToRowMap(goal062Spatial);
        var gameplayByRow = ToRowMap(goal063Runtime);
        var livingByRow = ToRowMap(goal064Plan);
        var interlockedByRow = ToRowMap(goal065Rows);
        var settlementByRow = ToRowMap(goal066Rows);
        var narrativeByRow = ToRowMap(goal067Rows);
        var combatReplayByRow = ToRowMap(goal068Replay);

        var handoffText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal068Accepted = handoffText.Contains("combat_magic_ability_boss_encounter_matrix_verification passed before Goal 069", StringComparison.Ordinal);
        if (!goal068Accepted)
        {
            diagnostics.Add(Error("goal069.preflight.goal068_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal 068 user-handoff acceptance must be recorded before Goal 069."));
        }

        var rows = new List<WorldEventSourceRow>();
        foreach (var combat in goal068Rows)
        {
            var rowId = RowId(combat);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal069.source.goal068_row_id_missing", Goal068Root + "/combat-magic-row-matrix.json", "Goal 068 row id is required."));
                continue;
            }

            var familyId = Text(combat, "familyId");
            var seedId = Text(combat, "seedId");
            packageByRow.TryGetValue(rowId, out var package);
            reviewByRow.TryGetValue(rowId, out var review);
            spatialByRow.TryGetValue(rowId, out var spatial);
            gameplayByRow.TryGetValue(rowId, out var gameplay);
            livingByRow.TryGetValue(rowId, out var living);
            interlockedByRow.TryGetValue(rowId, out var interlocked);
            settlementByRow.TryGetValue(rowId, out var settlement);
            narrativeByRow.TryGetValue(rowId, out var narrative);
            combatReplayByRow.TryGetValue(rowId, out var replay);

            if (!WorldEventWeatherDayNightCrisisVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal069.source.fake_family_id", familyId, "Goal 069 accepts only the three proven family ids."));
            }

            if (!WorldEventWeatherDayNightCrisisVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal069.source.fake_seed_id", seedId, "Goal 069 accepts only seed_alpha, seed_beta and seed_gamma."));
            }

            rows.Add(new WorldEventSourceRow
            {
                RowId = rowId,
                FamilyId = familyId,
                SeedId = seedId,
                SourcePackageRowRef = Text(combat, "sourcePackageRowRef"),
                SourceReviewPackageRowRef = Text(combat, "sourceReviewPackageRowRef"),
                SourceSpatialDetailRowRef = Text(combat, "sourceSpatialDetailRowRef"),
                SourceGameplayConsequenceRowRef = Text(combat, "sourceGameplayConsequenceRowRef"),
                SourceLivingWorldRowRef = Text(combat, "sourceLivingWorldRowRef"),
                SourceInterlockedGameplayRowRef = Text(combat, "sourceInterlockedGameplayRowRef"),
                SourceSettlementRowRef = Text(combat, "sourceSettlementRowRef"),
                SourceNarrativeRowRef = Text(combat, "sourceNarrativeRowRef"),
                SourceCombatMagicRowRef = "Goal068:" + rowId,
                PackageHash = Text(package, "packageHash"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                GameplayAfterStateHash = NestedText(gameplay, "afterState", "stateHash"),
                LivingWorldAfterStateHash = NestedText(living, "afterState", "stateHash"),
                InterlockedAfterStateHash = NestedText(interlocked, "afterState", "stateHash"),
                SettlementAfterStateHash = NestedText(settlement, "afterState", "stateHash"),
                NarrativeAfterStateHash = NestedText(narrative, "afterState", "stateHash"),
                CombatMagicRowHash = Text(combat, "rowHash"),
                CombatMagicAfterStateHash = NestedText(combat, "afterState", "stateHash"),
                QuestArcId = Text(narrative, "questArcId"),
                DialogueGraphId = Text(narrative, "dialogueGraphId"),
                EventChainId = Text(narrative, "eventChainId"),
                SettlementId = Text(settlement, "settlementId"),
                BuildingId = Text(settlement, "buildingId"),
                Goal060PackageValid = !string.IsNullOrWhiteSpace(Text(package, "packageHash")),
                Goal061ReviewPackageRcExists = !string.IsNullOrWhiteSpace(RowId(review)),
                Goal062SpatialRowValid = !string.IsNullOrWhiteSpace(Text(spatial, "rowHash")),
                Goal063GameplayRowValid = !string.IsNullOrWhiteSpace(NestedText(gameplay, "afterState", "stateHash")),
                Goal064LivingWorldRowValid = !string.IsNullOrWhiteSpace(NestedText(living, "afterState", "stateHash")),
                Goal065InterlockedRowValid = Bool(interlocked, "stateChanging") || !string.IsNullOrWhiteSpace(Text(interlocked, "rowHash")),
                Goal066SettlementRowValid = Bool(settlement, "stateChanging") || !string.IsNullOrWhiteSpace(Text(settlement, "rowHash")),
                Goal067NarrativeRowValid = Bool(narrative, "stateChanging") && !string.IsNullOrWhiteSpace(Text(narrative, "rowHash")),
                Goal068CombatMagicRowValid = Bool(combat, "stateChanging")
                    && !string.IsNullOrWhiteSpace(Text(combat, "rowHash"))
                    && !string.IsNullOrWhiteSpace(NestedText(combat, "afterState", "stateHash")),
                Goal068SaveLoadReplayPassed = Bool(replay, "beforeAfterStateChanged")
                    && Bool(replay, "saveLoadRoundtripPassed")
                    && Bool(replay, "replayDeterminismPassed"),
                CombatMagicChangedCategories = ReadStringArray(combat, "changedCategories"),
                UpstreamHashes = UpstreamHashes(package, spatial, gameplay, living, interlocked, settlement, narrative, combat)
            });
        }

        var orderedRows = rows
            .OrderBy(item => WorldEventWeatherDayNightCrisisVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => WorldEventWeatherDayNightCrisisVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new WorldEventSourceBundle
        {
            Goal068AcceptedByUserHandoff = goal068Accepted,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal060PackageValid),
            Goal061ReviewPackageRcConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061ReviewPackageRcExists),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062SpatialRowValid),
            Goal063GameplayRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal063GameplayRowValid),
            Goal064LivingWorldRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal064LivingWorldRowValid),
            Goal065InterlockedRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal065InterlockedRowValid),
            Goal066SettlementRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal066SettlementRowValid),
            Goal067NarrativeRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal067NarrativeRowValid),
            Goal068CombatMagicRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal068CombatMagicRowValid && item.Goal068SaveLoadReplayPassed),
            Goal068UnityProofConsumed = goal068UnityMarkers.Count > 0 && SourceArtifactExists(sourceRefs, "combat_magic_unity_proof_summary"),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(WorldEventWeatherDayNightCrisisVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(WorldEventWeatherDayNightCrisisVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadGoal068StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<WorldEventDiagnostic> SortDiagnostics(IEnumerable<WorldEventDiagnostic> diagnostics) =>
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
        List<WorldEventSourceArtifactReference> refs,
        List<WorldEventDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal069.source.required_artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal069.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray().Select(item => item.Clone()).ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal069.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static WorldEventSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<WorldEventDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = false;
        if (!exists)
        {
            diagnostics.Add(Error("goal069.source.required_artifact_missing", normalized, "Required source artifact is missing."));
        }
        else
        {
            var bytes = File.ReadAllBytes(path);
            hash = WorldEventWeatherDayNightCrisisHash.Sha256(bytes);
            try
            {
                using var document = JsonDocument.Parse(bytes);
                fieldsPresent = requiredFields.All(field => document.RootElement.TryGetProperty(field, out _));
                if (!fieldsPresent)
                {
                    diagnostics.Add(Error("goal069.source.required_field_missing", normalized, "Required top-level source fields are missing."));
                }
            }
            catch (JsonException exception)
            {
                diagnostics.Add(Error("goal069.source.json_invalid", normalized, exception.Message));
            }
        }

        return new WorldEventSourceArtifactReference
        {
            SourceGoal = sourceGoal,
            ArtifactFamily = artifactFamily,
            ArtifactRelativePath = normalized,
            ArtifactHash = hash,
            Exists = exists,
            HashMatches = exists && fieldsPresent && !string.IsNullOrWhiteSpace(hash),
            RequiredFields = requiredFields,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<WorldEventFilePayload> LoadGoal068StagingFiles(string projectRoot, List<WorldEventDiagnostic> diagnostics)
    {
        var root = Resolve(projectRoot, Goal068Root + "/" + WorldEventWeatherDayNightCrisisVocabulary.StagingRoot);
        if (!Directory.Exists(root))
        {
            diagnostics.Add(Error("goal069.source.goal068_staging_missing", Goal068Root + "/staging", "Goal 068 staging files are required for Goal 069 Unity Alpha proof."));
            return [];
        }

        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => new WorldEventFilePayload
            {
                RelativePath = Normalize(Path.GetRelativePath(root, path)),
                Bytes = File.ReadAllBytes(path)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static bool SourceArtifactExists(IReadOnlyList<WorldEventSourceArtifactReference> refs, string artifactFamily) =>
        refs.Any(item => item.ArtifactFamily == artifactFamily && item.Exists && item.HashMatches);

    private static IReadOnlyDictionary<string, JsonElement> ToRowMap(IEnumerable<JsonElement> rows) =>
        rows
            .Select(item => new { RowId = RowId(item), Row = item })
            .Where(item => !string.IsNullOrWhiteSpace(item.RowId))
            .GroupBy(item => item.RowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Row, StringComparer.Ordinal);

    private static IReadOnlyList<WorldEventUpstreamHash> UpstreamHashes(params JsonElement[] elements)
    {
        var result = new List<WorldEventUpstreamHash>();
        foreach (var element in elements)
        {
            if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                continue;
            }

            var rowId = RowId(element);
            var familyId = Text(element, "familyId");
            var seedId = Text(element, "seedId");
            var sourceRef = string.IsNullOrWhiteSpace(rowId) ? familyId + "/" + seedId : rowId;
            AddHash(result, "row", sourceRef, Text(element, "rowHash"), GuessSourceGoal(element));
            AddHash(result, "after_state", sourceRef, NestedText(element, "afterState", "stateHash"), GuessSourceGoal(element));
            AddHash(result, "package", sourceRef, Text(element, "packageHash"), GuessSourceGoal(element));
        }

        return result
            .GroupBy(item => item.SourceGoal + "|" + item.SourceRef + "|" + item.HashKind + "|" + item.Hash, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.SourceGoal, StringComparer.Ordinal)
            .ThenBy(item => item.SourceRef, StringComparer.Ordinal)
            .ThenBy(item => item.HashKind, StringComparer.Ordinal)
            .ToList();
    }

    private static void AddHash(List<WorldEventUpstreamHash> hashes, string hashKind, string sourceRef, string hash, string sourceGoal)
    {
        if (!string.IsNullOrWhiteSpace(hash))
        {
            hashes.Add(new WorldEventUpstreamHash
            {
                SourceGoal = sourceGoal,
                SourceRef = sourceRef,
                HashKind = hashKind,
                Hash = hash
            });
        }
    }

    private static string GuessSourceGoal(JsonElement element)
    {
        if (!string.IsNullOrWhiteSpace(Text(element, "encounterId")))
        {
            return "Goal068";
        }

        if (!string.IsNullOrWhiteSpace(Text(element, "questArcId")))
        {
            return "Goal067";
        }

        if (!string.IsNullOrWhiteSpace(Text(element, "settlementId")))
        {
            return "Goal066";
        }

        if (!string.IsNullOrWhiteSpace(Text(element, "packageHash")))
        {
            return "Goal060";
        }

        return "Goal062-065";
    }

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Normalize(string path) =>
        path.Replace('\\', '/');

    private static string RowId(JsonElement element)
    {
        var rowId = Text(element, "rowId");
        if (!string.IsNullOrWhiteSpace(rowId))
        {
            return rowId;
        }

        rowId = Text(element, "matrixRowId");
        if (!string.IsNullOrWhiteSpace(rowId))
        {
            return rowId;
        }

        return Text(element, "id");
    }

    private static string Text(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string NestedText(JsonElement element, string objectProperty, string valueProperty)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        return element.TryGetProperty(objectProperty, out var nested) ? Text(nested, valueProperty) : string.Empty;
    }

    private static bool Bool(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return false;
        }

        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.True;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            || !element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static WorldEventDiagnostic Error(string code, string target, string message) =>
        WorldEventDiagnostic.Error(code, target, message);
}
