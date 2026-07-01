using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;

public sealed class CombatMagicAbilityBossEncounterSourceLoader
{
    private const string Goal060Root = CombatMagicAbilityBossEncounterVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal061Root = CombatMagicAbilityBossEncounterVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal062Root = CombatMagicAbilityBossEncounterVocabulary.Goal062RelativeOutputDirectory;
    private const string Goal063Root = CombatMagicAbilityBossEncounterVocabulary.Goal063RelativeOutputDirectory;
    private const string Goal064Root = CombatMagicAbilityBossEncounterVocabulary.Goal064RelativeOutputDirectory;
    private const string Goal065Root = CombatMagicAbilityBossEncounterVocabulary.Goal065RelativeOutputDirectory;
    private const string Goal066Root = CombatMagicAbilityBossEncounterVocabulary.Goal066RelativeOutputDirectory;
    private const string Goal067Root = CombatMagicAbilityBossEncounterVocabulary.Goal067RelativeOutputDirectory;

    public CombatMagicSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<CombatMagicDiagnostic>();
        var sourceRefs = new List<CombatMagicSourceArtifactReference>();

        var goal060Packages = ReadRows(projectRoot, Goal060Root + "/materialized-package-inventory.json", "packages", "Goal060", "materialized_package_inventory", ["packageCount", "packages"], sourceRefs, diagnostics);
        var goal060Validation = ReadRows(projectRoot, Goal060Root + "/package-validation-matrix.json", "rows", "Goal060", "package_validation_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal061Review = ReadRows(projectRoot, Goal061Root + "/review-package-rc-manifest.json", "rows", "Goal061", "review_package_rc_manifest", ["passed", "reviewPackageRcId", "rows"], sourceRefs, diagnostics);
        var goal062Spatial = ReadRows(projectRoot, Goal062Root + "/spatial-detail-matrix.json", "rows", "Goal062", "spatial_detail_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal063Runtime = ReadRows(projectRoot, Goal063Root + "/runtime-state-delta-matrix.json", "rows", "Goal063", "runtime_state_delta_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal064Plan = ReadRows(projectRoot, Goal064Root + "/simulation-matrix-plan.json", "rows", "Goal064", "living_world_simulation_matrix_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Rows = ReadRows(projectRoot, Goal065Root + "/row-plan-matrix.json", "rows", "Goal065", "interlocked_row_plan_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065CombatLedger = ReadRows(projectRoot, Goal065Root + "/combat-progression-ledger.json", "entries", "Goal065", "combat_progression_ledger", ["passed", "entries"], sourceRefs, diagnostics);
        var goal065StatusLedger = ReadRows(projectRoot, Goal065Root + "/status-effect-ledger.json", "entries", "Goal065", "status_effect_ledger", ["passed", "entries"], sourceRefs, diagnostics);
        var goal066Rows = ReadRows(projectRoot, Goal066Root + "/settlement-construction-row-matrix.json", "rows", "Goal066", "settlement_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal067Rows = ReadRows(projectRoot, Goal067Root + "/narrative-row-matrix.json", "rows", "Goal067", "narrative_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal067Replay = ReadRows(projectRoot, Goal067Root + "/narrative-save-load-replay-proof.json", "rows", "Goal067", "narrative_save_load_replay", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRows(projectRoot, Goal067Root + "/narrative-unity-player-proof-summary.json", "matchedMarkers", "Goal067", "narrative_unity_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);

        var handoffText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal067Accepted = handoffText.Contains("programmatic_narrative_quest_dialogue_event_matrix_verification passed before Goal 068", StringComparison.Ordinal);
        if (!goal067Accepted)
        {
            diagnostics.Add(Error("goal068.preflight.goal067_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal 067 user-handoff acceptance must be recorded before Goal 068."));
        }

        var validationByRow = goal060Validation.ToDictionary(RowId, StringComparer.Ordinal);
        var reviewByRow = goal061Review.ToDictionary(RowId, StringComparer.Ordinal);
        var spatialByRow = goal062Spatial.ToDictionary(RowId, StringComparer.Ordinal);
        var gameplayByRow = goal063Runtime.ToDictionary(RowId, StringComparer.Ordinal);
        var livingByRow = goal064Plan.ToDictionary(RowId, StringComparer.Ordinal);
        var interlockedByRow = goal065Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var settlementByRow = goal066Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var narrativeByRow = goal067Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var narrativeReplayByRow = goal067Replay.ToDictionary(RowId, StringComparer.Ordinal);
        var combatLedgerByRow = goal065CombatLedger.GroupBy(RowId, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var statusLedgerByRow = goal065StatusLedger.GroupBy(RowId, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var rows = new List<CombatMagicSourceRow>();
        foreach (var package in goal060Packages)
        {
            var rowId = RowId(package);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal068.source.goal060_row_id_missing", Goal060Root + "/materialized-package-inventory.json", "Goal 060 package inventory row id is required."));
                continue;
            }

            var familyId = Text(package, "familyId");
            var seedId = Text(package, "seedId");
            if (!CombatMagicAbilityBossEncounterVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal068.source.fake_family_id", familyId, "Goal 068 accepts only the three proven family ids."));
            }

            if (!CombatMagicAbilityBossEncounterVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal068.source.fake_seed_id", seedId, "Goal 068 accepts only seed_alpha, seed_beta and seed_gamma."));
            }

            if (!reviewByRow.TryGetValue(rowId, out var review)
                || !spatialByRow.TryGetValue(rowId, out var spatial)
                || !gameplayByRow.TryGetValue(rowId, out var gameplay)
                || !livingByRow.TryGetValue(rowId, out var living)
                || !interlockedByRow.TryGetValue(rowId, out var interlocked)
                || !settlementByRow.TryGetValue(rowId, out var settlement)
                || !narrativeByRow.TryGetValue(rowId, out var narrative))
            {
                diagnostics.Add(Error("goal068.source.row_chain_missing", rowId, "Goal 068 requires matching Goal 060/061/062/063/064/065/066/067 rows."));
                continue;
            }

            validationByRow.TryGetValue(rowId, out var validation);
            narrativeReplayByRow.TryGetValue(rowId, out var narrativeReplay);
            combatLedgerByRow.TryGetValue(rowId, out var combatLedger);
            statusLedgerByRow.TryGetValue(rowId, out var statusLedger);
            combatLedger ??= [];
            statusLedger ??= [];

            sourceRefs.Add(FileRef(projectRoot, "Goal060", "materialized_package_row", Goal060Root + "/" + Text(package, "packageRelativePath"), ["id", "metadata"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal062", "spatial_detail_row", Goal062Root + "/spatial-detail-row-" + familyId + "-" + seedId + ".json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal063", "gameplay_consequence_row", Goal063Root + "/rows/" + familyId + "-" + seedId + "-gameplay-proof.json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal064", "living_world_row", Goal064Root + "/rows/" + familyId + "-" + seedId + "-living-world-row.json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal065", "interlocked_gameplay_row", Goal065Root + "/row-" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + ".json", ["rowId", "familyId", "seedId", "rowHash"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal066", "settlement_row", Goal066Root + "/rows/" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + "-settlement-row.json", ["rowId", "familyId", "seedId", "rowHash", "settlementId"]));
            sourceRefs.Add(FileRef(projectRoot, "Goal067", "narrative_row", Goal067Root + "/rows/" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + "-narrative-row.json", ["rowId", "familyId", "seedId", "rowHash"]));

            rows.Add(new CombatMagicSourceRow
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
                SourceNarrativeRowRef = "Goal067:" + rowId,
                PackageId = Text(package, "packageId"),
                PackageHash = Text(package, "packageHash"),
                PackageRelativePath = Text(package, "packageRelativePath"),
                ReviewPackageRelativePath = Text(review, "packageRelativePath"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                GameplayAfterStateHash = NestedText(gameplay, "afterState", "stateHash"),
                LivingWorldAfterStateHash = NestedText(living, "afterState", "stateHash"),
                InterlockedAfterStateHash = NestedText(interlocked, "afterState", "stateHash"),
                SettlementAfterStateHash = NestedText(settlement, "afterState", "stateHash"),
                NarrativeAfterStateHash = NestedText(narrative, "afterState", "stateHash"),
                QuestArcId = Text(narrative, "questArcId"),
                DialogueGraphId = Text(narrative, "dialogueGraphId"),
                EventChainId = Text(narrative, "eventChainId"),
                SettlementId = Text(settlement, "settlementId"),
                BuildingId = Text(settlement, "buildingId"),
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
                Goal067NarrativeRowValid = Bool(narrative, "stateChanging")
                    && Bool(narrative, "noFinalProse")
                    && !string.IsNullOrWhiteSpace(Text(narrative, "rowHash"))
                    && !string.IsNullOrWhiteSpace(NestedText(narrative, "afterState", "stateHash")),
                Goal067SaveLoadReplayPassed = narrativeReplay.ValueKind != JsonValueKind.Undefined
                    && Bool(narrativeReplay, "beforeAfterStateChanged")
                    && Bool(narrativeReplay, "saveLoadRoundtripPassed")
                    && Bool(narrativeReplay, "replayDeterminismPassed"),
                GameplayDeltaIds = ReadGameplayDeltaIds(gameplay),
                LivingWorldActorIds = ReadNestedStringArray(living, "actorRecords", "actorId"),
                LivingWorldFactionIds = ReadNestedStringArray(living, "factionRecords", "factionId"),
                InterlockedDeltaIds = ReadDeltaIds(interlocked),
                InterlockedCombatProgressionLedgerEntryIds = combatLedger.Select(item => Text(item, "entryId")).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                InterlockedStatusLedgerEntryIds = statusLedger.Select(item => Text(item, "entryId")).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                SettlementLedgerEntryIds = ReadSettlementLedgerIds(settlement),
                NarrativeDeltaIds = ReadNestedStringArray(narrative, "stateDeltas", "deltaId")
            });
        }

        var orderedRows = rows
            .OrderBy(item => CombatMagicAbilityBossEncounterVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => CombatMagicAbilityBossEncounterVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new CombatMagicSourceBundle
        {
            Goal067AcceptedByUserHandoff = goal067Accepted,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal060PackageValid && !string.IsNullOrWhiteSpace(item.PackageHash)),
            Goal061ReviewPackageRcConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061ReviewPackageRcExists),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062SpatialRowValid),
            Goal063GameplayRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal063GameplayRowValid && item.GameplayDeltaIds.Count >= 2),
            Goal064LivingWorldRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal064LivingWorldRowValid && item.LivingWorldActorIds.Count > 0 && item.LivingWorldFactionIds.Count > 0),
            Goal065InterlockedRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal065InterlockedRowValid && item.InterlockedDeltaIds.Count >= 5 && item.InterlockedCombatProgressionLedgerEntryIds.Count >= 3 && item.InterlockedStatusLedgerEntryIds.Count >= 2),
            Goal066SettlementRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal066SettlementRowValid && item.SettlementLedgerEntryIds.Count >= 4),
            Goal067NarrativeRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal067NarrativeRowValid && item.Goal067SaveLoadReplayPassed && item.NarrativeDeltaIds.Count >= 5),
            Goal067UnityProofConsumed = sourceRefs.Any(item => item.ArtifactFamily == "narrative_unity_proof_summary" && item.Exists && item.HashMatches),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(CombatMagicAbilityBossEncounterVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(CombatMagicAbilityBossEncounterVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadGoal067StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<CombatMagicDiagnostic> SortDiagnostics(IEnumerable<CombatMagicDiagnostic> diagnostics) =>
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
        List<CombatMagicSourceArtifactReference> refs,
        List<CombatMagicDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal068.source.required_artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal068.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray().Select(item => item.Clone()).ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal068.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static CombatMagicSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<CombatMagicDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = true;
        if (exists)
        {
            var bytes = File.ReadAllBytes(path);
            hash = CombatMagicAbilityBossEncounterHash.HashBytes(bytes);
            var text = Encoding.UTF8.GetString(bytes);
            foreach (var field in requiredFields)
            {
                if (!text.Contains("\"" + field + "\"", StringComparison.Ordinal) && !text.Contains(field, StringComparison.Ordinal))
                {
                    fieldsPresent = false;
                    diagnostics.Add(Error("goal068.source.required_field_missing", normalized + "#" + field, "Required source field is missing."));
                }
            }
        }
        else
        {
            fieldsPresent = false;
            diagnostics.Add(Error("goal068.source.file_missing", normalized, "Required source file is missing."));
        }

        return new CombatMagicSourceArtifactReference
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

    private static IReadOnlyList<CombatMagicFilePayload> LoadGoal067StagingFiles(
        string projectRoot,
        List<CombatMagicDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal067Root + "/" + CombatMagicAbilityBossEncounterVocabulary.StagingRoot);
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal068.source.goal067_staging_missing", Goal067Root + "/staging", "Goal 068 Unity proof requires Goal 067 Alpha staging payload files."));
            return [];
        }

        var result = new List<CombatMagicFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal068.source.goal067_staging_unsafe_path", relative, "Goal 067 staging file path is not safe for reuse."));
                continue;
            }

            result.Add(new CombatMagicFilePayload
            {
                RelativePath = Normalize(relative),
                Bytes = File.ReadAllBytes(file)
            });
        }

        if (!result.Any(item => item.RelativePath == "runtime/unity-runtime-config.json"))
        {
            diagnostics.Add(Error("goal068.source.goal067_staging_runtime_config_missing", Goal067Root + "/staging", "Goal 067 staging must include Alpha runtime config."));
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

    private static CombatMagicDiagnostic Error(string code, string target, string message) =>
        CombatMagicDiagnostic.Error(code, target, message);
}
