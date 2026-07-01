using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;

public sealed class ProgrammaticNarrativeSourceLoader
{
    private const string Goal060Root = ProgrammaticNarrativeVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal061Root = ProgrammaticNarrativeVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal062Root = ProgrammaticNarrativeVocabulary.Goal062RelativeOutputDirectory;
    private const string Goal063Root = ProgrammaticNarrativeVocabulary.Goal063RelativeOutputDirectory;
    private const string Goal064Root = ProgrammaticNarrativeVocabulary.Goal064RelativeOutputDirectory;
    private const string Goal065Root = ProgrammaticNarrativeVocabulary.Goal065RelativeOutputDirectory;
    private const string Goal066Root = ProgrammaticNarrativeVocabulary.Goal066RelativeOutputDirectory;

    public ProgrammaticNarrativeSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        var sourceRefs = new List<ProgrammaticNarrativeSourceArtifactReference>();

        var goal060Packages = ReadRows(projectRoot, Goal060Root + "/materialized-package-inventory.json", "packages", "Goal060", "materialized_package_inventory", ["packageCount", "packages"], sourceRefs, diagnostics);
        var goal060Validation = ReadRows(projectRoot, Goal060Root + "/package-validation-matrix.json", "rows", "Goal060", "package_validation_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal061Review = ReadRows(projectRoot, Goal061Root + "/review-package-rc-manifest.json", "rows", "Goal061", "review_package_rc_manifest", ["passed", "reviewPackageRcId", "rows"], sourceRefs, diagnostics);
        var goal062Spatial = ReadRows(projectRoot, Goal062Root + "/spatial-detail-matrix.json", "rows", "Goal062", "spatial_detail_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal063Runtime = ReadRows(projectRoot, Goal063Root + "/runtime-state-delta-matrix.json", "rows", "Goal063", "runtime_state_delta_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal064Plan = ReadRows(projectRoot, Goal064Root + "/simulation-matrix-plan.json", "rows", "Goal064", "living_world_simulation_matrix_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Rows = ReadRows(projectRoot, Goal065Root + "/row-plan-matrix.json", "rows", "Goal065", "interlocked_row_plan_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal066Rows = ReadRows(projectRoot, Goal066Root + "/settlement-construction-row-matrix.json", "rows", "Goal066", "settlement_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal066Replay = ReadRows(projectRoot, Goal066Root + "/settlement-save-load-replay-proof.json", "rows", "Goal066", "settlement_save_load_replay", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRows(projectRoot, Goal066Root + "/settlement-unity-player-proof-summary.json", "matchedMarkers", "Goal066", "settlement_unity_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);

        var handoffText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal066Accepted = handoffText.Contains("settlement_construction_destruction_production_matrix_verification passed before Goal 067", StringComparison.Ordinal);
        if (!goal066Accepted)
        {
            diagnostics.Add(Error("goal067.preflight.goal066_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal 066 user-handoff acceptance must be recorded before Goal 067."));
        }

        var validationByRow = goal060Validation.ToDictionary(RowId, StringComparer.Ordinal);
        var reviewByRow = goal061Review.ToDictionary(RowId, StringComparer.Ordinal);
        var spatialByRow = goal062Spatial.ToDictionary(RowId, StringComparer.Ordinal);
        var gameplayByRow = goal063Runtime.ToDictionary(RowId, StringComparer.Ordinal);
        var livingByRow = goal064Plan.ToDictionary(RowId, StringComparer.Ordinal);
        var interlockedByRow = goal065Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var settlementByRow = goal066Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var settlementReplayByRow = goal066Replay.ToDictionary(RowId, StringComparer.Ordinal);

        var rows = new List<ProgrammaticNarrativeSourceRow>();
        foreach (var package in goal060Packages)
        {
            var rowId = RowId(package);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal067.source.goal060_row_id_missing", Goal060Root + "/materialized-package-inventory.json", "Goal 060 package inventory row id is required."));
                continue;
            }

            var familyId = Text(package, "familyId");
            var seedId = Text(package, "seedId");
            if (!ProgrammaticNarrativeVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal067.source.fake_family_id", familyId, "Goal 067 accepts only the three proven family ids."));
            }

            if (!ProgrammaticNarrativeVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal067.source.fake_seed_id", seedId, "Goal 067 accepts only seed_alpha, seed_beta and seed_gamma."));
            }

            if (!reviewByRow.TryGetValue(rowId, out var review))
            {
                diagnostics.Add(Error("goal067.source.goal061_row_missing", rowId, "Goal 061 review package RC row is required."));
                continue;
            }

            if (!spatialByRow.TryGetValue(rowId, out var spatial))
            {
                diagnostics.Add(Error("goal067.source.goal062_row_missing", rowId, "Goal 062 spatial detail row is required."));
                continue;
            }

            if (!gameplayByRow.TryGetValue(rowId, out var gameplay))
            {
                diagnostics.Add(Error("goal067.source.goal063_row_missing", rowId, "Goal 063 gameplay consequence row is required."));
                continue;
            }

            if (!livingByRow.TryGetValue(rowId, out var living))
            {
                diagnostics.Add(Error("goal067.source.goal064_row_missing", rowId, "Goal 064 living-world row is required."));
                continue;
            }

            if (!interlockedByRow.TryGetValue(rowId, out var interlocked))
            {
                diagnostics.Add(Error("goal067.source.goal065_row_missing", rowId, "Goal 065 interlocked gameplay row is required."));
                continue;
            }

            if (!settlementByRow.TryGetValue(rowId, out var settlement))
            {
                diagnostics.Add(Error("goal067.source.goal066_row_missing", rowId, "Goal 066 settlement row is required."));
                continue;
            }

            validationByRow.TryGetValue(rowId, out var validation);
            settlementReplayByRow.TryGetValue(rowId, out var settlementReplay);

            sourceRefs.Add(FileRef(projectRoot, "Goal060", "materialized_package_row", Goal060Root + "/" + Text(package, "packageRelativePath"), ["id", "metadata"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal062", "spatial_detail_row", Goal062Root + "/spatial-detail-row-" + familyId + "-" + seedId + ".json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal063", "gameplay_consequence_row", Goal063Root + "/rows/" + familyId + "-" + seedId + "-gameplay-proof.json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal064", "living_world_row", Goal064Root + "/rows/" + familyId + "-" + seedId + "-living-world-row.json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal065", "interlocked_gameplay_row", Goal065Root + "/row-" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + ".json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal066", "settlement_row", Goal066Root + "/rows/" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + "-settlement-row.json", ["rowId", "familyId", "seedId", "rowHash", "settlementId"]));

            rows.Add(new ProgrammaticNarrativeSourceRow
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
                SourceSettlementRowRef = "Goal066:" + rowId,
                PackageId = Text(package, "packageId"),
                PackageHash = Text(package, "packageHash"),
                PackageRelativePath = Text(package, "packageRelativePath"),
                ReviewPackageRelativePath = Text(review, "packageRelativePath"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                SpatialVarianceMarker = Text(spatial, "varianceMarker"),
                GameplayAfterStateHash = NestedText(gameplay, "afterState", "stateHash"),
                LivingWorldRowHash = Text(living, "rowHash"),
                LivingWorldAfterStateHash = NestedText(living, "afterState", "stateHash"),
                InterlockedRowHash = Text(interlocked, "rowHash"),
                InterlockedAfterStateHash = NestedText(interlocked, "afterState", "stateHash"),
                SettlementRowHash = Text(settlement, "rowHash"),
                SettlementAfterStateHash = NestedText(settlement, "afterState", "stateHash"),
                SettlementId = Text(settlement, "settlementId"),
                BuildingId = Text(settlement, "buildingId"),
                LivingWorldLinkageId = NestedText(settlement, "livingWorldConsequence", "linkageId"),
                InterlockedDependencyId = NestedText(settlement, "interlockedGameplayDependency", "dependencyId"),
                Goal060PackageValid = Bool(package, "validationPassed") && (validation.ValueKind == JsonValueKind.Undefined || Bool(validation, "validationPassed")),
                Goal061ReviewPackageRcExists = Bool(review, "packageHashVerified") && Bool(review, "saveLoadReplayVerified"),
                Goal062SpatialRowValid = Bool(spatial, "reachable") && Bool(spatial, "routeVerified") && !string.IsNullOrWhiteSpace(Text(spatial, "rowHash")),
                Goal063GameplayRowValid = Int(gameplay, "stateChangingStepCount") >= 3 && !string.IsNullOrWhiteSpace(NestedText(gameplay, "afterState", "stateHash")),
                Goal064LivingWorldRowValid = !string.IsNullOrWhiteSpace(Text(living, "rowHash"))
                    && !string.Equals(NestedText(living, "beforeState", "stateHash"), NestedText(living, "afterState", "stateHash"), StringComparison.Ordinal)
                    && ReadNestedStringArray(living, "actorRecords", "actorId").Count > 0
                    && ReadNestedStringArray(living, "factionRecords", "factionId").Count > 0,
                Goal065InterlockedRowValid = Bool(interlocked, "stateChanging") && !string.IsNullOrWhiteSpace(Text(interlocked, "rowHash")),
                Goal066SettlementRowValid = Bool(settlement, "stateChanging")
                    && !string.IsNullOrWhiteSpace(Text(settlement, "settlementId"))
                    && !string.IsNullOrWhiteSpace(Text(settlement, "buildingId")),
                Goal066SaveLoadReplayPassed = settlementReplay.ValueKind != JsonValueKind.Undefined
                    && Bool(settlementReplay, "beforeAfterStateChanged")
                    && Bool(settlementReplay, "saveLoadRoundtripPassed")
                    && Bool(settlementReplay, "replayDeterminismPassed"),
                GameplayDeltaIds = ReadGameplayDeltaIds(gameplay),
                LivingWorldActorIds = ReadNestedStringArray(living, "actorRecords", "actorId"),
                LivingWorldFactionIds = ReadNestedStringArray(living, "factionRecords", "factionId"),
                LivingWorldEventIds = ReadNestedStringArray(living, "worldEventRecords", "eventId"),
                LivingWorldMemoryRumorIds = ReadMemoryRumorIds(living),
                InterlockedDeltaIds = ReadDeltaIds(interlocked),
                SettlementLedgerEntryIds = ReadSettlementLedgerIds(settlement)
            });
        }

        var orderedRows = rows
            .OrderBy(item => ProgrammaticNarrativeVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => ProgrammaticNarrativeVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new ProgrammaticNarrativeSourceBundle
        {
            Goal066AcceptedByUserHandoff = goal066Accepted,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal060PackageValid && !string.IsNullOrWhiteSpace(item.PackageHash)),
            Goal061ReviewPackageRcConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061ReviewPackageRcExists),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062SpatialRowValid),
            Goal063GameplayRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal063GameplayRowValid && item.GameplayDeltaIds.Count >= 2),
            Goal064LivingWorldRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal064LivingWorldRowValid && item.LivingWorldActorIds.Count > 0 && item.LivingWorldFactionIds.Count > 0),
            Goal065InterlockedRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal065InterlockedRowValid && item.InterlockedDeltaIds.Count >= 5),
            Goal066SettlementRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal066SettlementRowValid && item.Goal066SaveLoadReplayPassed && item.SettlementLedgerEntryIds.Count >= 4),
            Goal066UnityProofConsumed = sourceRefs.Any(item => item.ArtifactFamily == "settlement_unity_proof_summary" && item.Exists && item.HashMatches),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(ProgrammaticNarrativeVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(ProgrammaticNarrativeVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadGoal066StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<ProgrammaticNarrativeDiagnostic> SortDiagnostics(IEnumerable<ProgrammaticNarrativeDiagnostic> diagnostics) =>
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
        List<ProgrammaticNarrativeSourceArtifactReference> refs,
        List<ProgrammaticNarrativeDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal067.source.required_artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal067.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray().Select(item => item.Clone()).ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal067.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static ProgrammaticNarrativeSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<ProgrammaticNarrativeDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = true;
        if (exists)
        {
            var bytes = File.ReadAllBytes(path);
            hash = ProgrammaticNarrativeHash.HashBytes(bytes);
            var text = Encoding.UTF8.GetString(bytes);
            foreach (var field in requiredFields)
            {
                if (!text.Contains("\"" + field + "\"", StringComparison.Ordinal) && !text.Contains(field, StringComparison.Ordinal))
                {
                    fieldsPresent = false;
                    diagnostics.Add(Error("goal067.source.required_field_missing", normalized + "#" + field, "Required source field is missing."));
                }
            }
        }
        else
        {
            fieldsPresent = false;
            diagnostics.Add(Error("goal067.source.file_missing", normalized, "Required source file is missing."));
        }

        return new ProgrammaticNarrativeSourceArtifactReference
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

    private static IReadOnlyList<ProgrammaticNarrativeFilePayload> LoadGoal066StagingFiles(
        string projectRoot,
        List<ProgrammaticNarrativeDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal066Root + "/" + ProgrammaticNarrativeVocabulary.StagingRoot);
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal067.source.goal066_staging_missing", Goal066Root + "/staging", "Goal 067 Unity proof requires Goal 066 Alpha staging payload files."));
            return [];
        }

        var result = new List<ProgrammaticNarrativeFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal067.source.goal066_staging_unsafe_path", relative, "Goal 066 staging file path is not safe for reuse."));
                continue;
            }

            result.Add(new ProgrammaticNarrativeFilePayload
            {
                RelativePath = Normalize(relative),
                Bytes = File.ReadAllBytes(file)
            });
        }

        if (!result.Any(item => item.RelativePath == "runtime/unity-runtime-config.json"))
        {
            diagnostics.Add(Error("goal067.source.goal066_staging_runtime_config_missing", Goal066Root + "/staging", "Goal 066 staging must include Alpha runtime config."));
        }

        return result.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> ReadGameplayDeltaIds(JsonElement row)
    {
        if (row.ValueKind != JsonValueKind.Object || !row.TryGetProperty("transitions", out var transitions) || transitions.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<string>();
        foreach (var transition in transitions.EnumerateArray())
        {
            if (transition.TryGetProperty("deltas", out var deltas) && deltas.ValueKind == JsonValueKind.Array)
            {
                result.AddRange(deltas.EnumerateArray().Select(delta => Text(delta, "deltaId")));
            }
        }

        return result.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> ReadDeltaIds(JsonElement row)
    {
        if (row.ValueKind != JsonValueKind.Object || !row.TryGetProperty("deltas", out var deltas) || deltas.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return deltas.EnumerateArray()
            .Select(item => Text(item, "deltaId"))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> ReadSettlementLedgerIds(JsonElement row)
    {
        var result = new List<string>();
        result.Add(Text(row, "settlementId"));
        result.Add(Text(row, "buildingId"));
        result.Add(NestedText(row, "constructionAction", "actionId"));
        result.Add(NestedText(row, "productionAction", "actionId"));
        result.Add(NestedText(row, "damageDestructionThreatEvent", "actionId"));
        result.Add(NestedText(row, "repairUpgradeDefenseResponse", "actionId"));
        result.Add(NestedText(row, "livingWorldConsequence", "linkageId"));
        result.Add(NestedText(row, "interlockedGameplayDependency", "dependencyId"));
        return result.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> ReadMemoryRumorIds(JsonElement row)
    {
        var primary = ReadNestedStringArray(row, "memoryRumorTraceRecords", "traceId");
        if (primary.Count > 0)
        {
            return primary;
        }

        return ReadNestedStringArray(row, "memoryRumorPropagation", "recordId");
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
            .Order(StringComparer.Ordinal)
            .ToList();
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

    private static ProgrammaticNarrativeDiagnostic Error(string code, string target, string message) =>
        ProgrammaticNarrativeDiagnostic.Error(code, target, message);
}
