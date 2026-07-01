using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;

public sealed class IntegratedCampaignTimelineSourceLoader
{
    private const string Goal060Root = IntegratedCampaignTimelineVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal061Root = IntegratedCampaignTimelineVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal062Root = IntegratedCampaignTimelineVocabulary.Goal062RelativeOutputDirectory;
    private const string Goal063Root = IntegratedCampaignTimelineVocabulary.Goal063RelativeOutputDirectory;
    private const string Goal064Root = IntegratedCampaignTimelineVocabulary.Goal064RelativeOutputDirectory;
    private const string Goal065Root = IntegratedCampaignTimelineVocabulary.Goal065RelativeOutputDirectory;
    private const string Goal066Root = IntegratedCampaignTimelineVocabulary.Goal066RelativeOutputDirectory;
    private const string Goal067Root = IntegratedCampaignTimelineVocabulary.Goal067RelativeOutputDirectory;
    private const string Goal068Root = IntegratedCampaignTimelineVocabulary.Goal068RelativeOutputDirectory;
    private const string Goal069Root = IntegratedCampaignTimelineVocabulary.Goal069RelativeOutputDirectory;

    public TimelineSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<TimelineDiagnostic>();
        var sourceRefs = new List<TimelineSourceArtifactReference>();

        RequireFolder(projectRoot, Goal060Root, "Goal060", diagnostics);
        RequireFolder(projectRoot, Goal061Root, "Goal061", diagnostics);
        RequireFolder(projectRoot, Goal062Root, "Goal062", diagnostics);
        RequireFolder(projectRoot, Goal063Root, "Goal063", diagnostics);
        RequireFolder(projectRoot, Goal064Root, "Goal064", diagnostics);
        RequireFolder(projectRoot, Goal065Root, "Goal065", diagnostics);
        RequireFolder(projectRoot, Goal066Root, "Goal066", diagnostics);
        RequireFolder(projectRoot, Goal067Root, "Goal067", diagnostics);
        RequireFolder(projectRoot, Goal068Root, "Goal068", diagnostics);
        RequireFolder(projectRoot, Goal069Root, "Goal069", diagnostics);

        var goal060Packages = ReadRows(projectRoot, Goal060Root + "/materialized-package-inventory.json", "packages", "Goal060", "materialized_package_inventory", ["packageCount", "packages"], sourceRefs, diagnostics);
        var goal061Review = ReadRows(projectRoot, Goal061Root + "/review-package-rc-manifest.json", "rows", "Goal061", "review_package_rc_manifest", ["passed", "reviewPackageRcId", "rows"], sourceRefs, diagnostics);
        var goal062Spatial = ReadRows(projectRoot, Goal062Root + "/spatial-detail-matrix.json", "rows", "Goal062", "spatial_detail_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal063Runtime = ReadRows(projectRoot, Goal063Root + "/runtime-state-delta-matrix.json", "rows", "Goal063", "runtime_state_delta_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal064Plan = ReadRows(projectRoot, Goal064Root + "/simulation-matrix-plan.json", "rows", "Goal064", "living_world_simulation_matrix_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal065Rows = ReadRows(projectRoot, Goal065Root + "/row-plan-matrix.json", "rows", "Goal065", "interlocked_row_plan_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal066Rows = ReadRows(projectRoot, Goal066Root + "/settlement-construction-row-matrix.json", "rows", "Goal066", "settlement_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal067Rows = ReadRows(projectRoot, Goal067Root + "/narrative-row-matrix.json", "rows", "Goal067", "narrative_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal068Rows = ReadRows(projectRoot, Goal068Root + "/combat-magic-row-matrix.json", "rows", "Goal068", "combat_magic_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal069Rows = ReadRows(projectRoot, Goal069Root + "/world-event-weather-daynight-row-matrix.json", "rows", "Goal069", "world_event_row_matrix", ["passed", "rows"], sourceRefs, diagnostics);
        var goal069Replay = ReadRows(projectRoot, Goal069Root + "/save-load-replay-proof.json", "rows", "Goal069", "world_event_save_load_replay", ["passed", "rows"], sourceRefs, diagnostics);
        _ = ReadRows(projectRoot, Goal069Root + "/unity-command-plan.json", "rows", "Goal069", "world_event_unity_command_plan", ["passed", "rows"], sourceRefs, diagnostics);
        var goal069UnityMarkers = ReadRows(projectRoot, Goal069Root + "/unity-proof-summary.json", "matchedMarkers", "Goal069", "world_event_unity_proof_summary", ["passed", "playerExecuted", "provenRowCount", "matchedMarkers"], sourceRefs, diagnostics);

        AddReportRefs(projectRoot, sourceRefs, diagnostics);
        AddGoal069RowFileRefs(projectRoot, goal069Rows, sourceRefs, diagnostics);

        var packageByRow = ToRowMap(goal060Packages);
        var reviewByRow = ToRowMap(goal061Review);
        var spatialByRow = ToRowMap(goal062Spatial);
        var gameplayByRow = ToRowMap(goal063Runtime);
        var livingByRow = ToRowMap(goal064Plan);
        var interlockedByRow = ToRowMap(goal065Rows);
        var settlementByRow = ToRowMap(goal066Rows);
        var narrativeByRow = ToRowMap(goal067Rows);
        var combatByRow = ToRowMap(goal068Rows);
        var worldReplayByRow = ToRowMap(goal069Replay);

        var handoffText = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptionalText(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal069Accepted = handoffText.Contains("world_event_weather_daynight_crisis_matrix_verification passed before Goal 070", StringComparison.Ordinal);
        if (!goal069Accepted)
        {
            diagnostics.Add(Error("goal070.preflight.goal069_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal 069 user-handoff acceptance must be recorded before Goal 070."));
        }

        var rows = new List<TimelineSourceRow>();
        foreach (var worldEvent in goal069Rows)
        {
            var rowId = RowId(worldEvent);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                diagnostics.Add(Error("goal070.source.goal069_row_id_missing", Goal069Root + "/world-event-weather-daynight-row-matrix.json", "Goal 069 row id is required."));
                continue;
            }

            var familyId = Text(worldEvent, "familyId");
            var seedId = Text(worldEvent, "seedId");
            packageByRow.TryGetValue(rowId, out var package);
            reviewByRow.TryGetValue(rowId, out var review);
            spatialByRow.TryGetValue(rowId, out var spatial);
            gameplayByRow.TryGetValue(rowId, out var gameplay);
            livingByRow.TryGetValue(rowId, out var living);
            interlockedByRow.TryGetValue(rowId, out var interlocked);
            settlementByRow.TryGetValue(rowId, out var settlement);
            narrativeByRow.TryGetValue(rowId, out var narrative);
            combatByRow.TryGetValue(rowId, out var combat);
            worldReplayByRow.TryGetValue(rowId, out var replay);

            ValidateIdentity("Goal060", rowId, familyId, seedId, package, diagnostics);
            ValidateIdentity("Goal061", rowId, familyId, seedId, review, diagnostics);
            ValidateIdentity("Goal062", rowId, familyId, seedId, spatial, diagnostics);
            ValidateIdentity("Goal063", rowId, familyId, seedId, gameplay, diagnostics);
            ValidateIdentity("Goal064", rowId, familyId, seedId, living, diagnostics);
            ValidateIdentity("Goal065", rowId, familyId, seedId, interlocked, diagnostics);
            ValidateIdentity("Goal066", rowId, familyId, seedId, settlement, diagnostics);
            ValidateIdentity("Goal067", rowId, familyId, seedId, narrative, diagnostics);
            ValidateIdentity("Goal068", rowId, familyId, seedId, combat, diagnostics);

            if (!IntegratedCampaignTimelineVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal070.source.fake_family_id", familyId, "Goal 070 accepts only the three proven family ids."));
            }

            if (!IntegratedCampaignTimelineVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal070.source.fake_seed_id", seedId, "Goal 070 accepts only seed_alpha, seed_beta and seed_gamma."));
            }

            rows.Add(new TimelineSourceRow
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
                SourceCombatMagicRowRef = "Goal068:" + rowId,
                SourceWorldEventRowRef = "Goal069:" + rowId,
                PackageHash = Text(package, "packageHash"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                GameplayAfterStateHash = NestedText(gameplay, "afterState", "stateHash"),
                LivingWorldAfterStateHash = NestedText(living, "afterState", "stateHash"),
                InterlockedAfterStateHash = NestedText(interlocked, "afterState", "stateHash"),
                SettlementAfterStateHash = NestedText(settlement, "afterState", "stateHash"),
                NarrativeAfterStateHash = NestedText(narrative, "afterState", "stateHash"),
                CombatMagicAfterStateHash = NestedText(combat, "afterState", "stateHash"),
                WorldEventRowHash = Text(worldEvent, "rowHash"),
                WorldEventAfterStateHash = NestedText(worldEvent, "afterState", "stateHash"),
                WorldClockPhase = NestedText(worldEvent, "worldClockAfter", "phase"),
                WeatherId = NestedText(worldEvent, "weatherHazard", "weatherId"),
                CrisisId = NestedText(worldEvent, "crisisEvent", "crisisId"),
                WorldEventChangedCategories = ReadStringArray(worldEvent, "changedCategories"),
                UpstreamRefs = ReadStringArray(worldEvent, "upstreamRefs").Concat(["Goal069:" + rowId]).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                UpstreamHashes = UpstreamHashes(package, spatial, gameplay, living, interlocked, settlement, narrative, combat, worldEvent),
                Goal060PackageValid = !string.IsNullOrWhiteSpace(Text(package, "packageHash")),
                Goal061ReviewPackageRcExists = !string.IsNullOrWhiteSpace(RowId(review)),
                Goal062SpatialRowValid = !string.IsNullOrWhiteSpace(Text(spatial, "rowHash")),
                Goal063GameplayRowValid = !string.IsNullOrWhiteSpace(NestedText(gameplay, "afterState", "stateHash")),
                Goal064LivingWorldRowValid = !string.IsNullOrWhiteSpace(NestedText(living, "afterState", "stateHash")),
                Goal065InterlockedRowValid = Bool(interlocked, "stateChanging") || !string.IsNullOrWhiteSpace(Text(interlocked, "rowHash")),
                Goal066SettlementRowValid = Bool(settlement, "stateChanging") || !string.IsNullOrWhiteSpace(Text(settlement, "rowHash")),
                Goal067NarrativeRowValid = Bool(narrative, "stateChanging") && !string.IsNullOrWhiteSpace(Text(narrative, "rowHash")),
                Goal068CombatMagicRowValid = Bool(combat, "stateChanging") && !string.IsNullOrWhiteSpace(NestedText(combat, "afterState", "stateHash")),
                Goal069WorldEventRowValid = Bool(worldEvent, "stateChanging")
                    && !string.IsNullOrWhiteSpace(Text(worldEvent, "rowHash"))
                    && !string.IsNullOrWhiteSpace(NestedText(worldEvent, "afterState", "stateHash"))
                    && ReadStringArray(worldEvent, "changedCategories").Count >= 4,
                Goal069SaveLoadReplayPassed = Bool(replay, "beforeAfterStateChanged")
                    && Bool(replay, "saveLoadRoundtripPassed")
                    && Bool(replay, "replayDeterminismPassed")
            });
        }

        var orderedRows = rows
            .OrderBy(item => IntegratedCampaignTimelineVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => IntegratedCampaignTimelineVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new TimelineSourceBundle
        {
            Goal069AcceptedByUserHandoff = goal069Accepted,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal060PackageValid),
            Goal061ReviewPackageRcConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061ReviewPackageRcExists),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062SpatialRowValid),
            Goal063GameplayRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal063GameplayRowValid),
            Goal064LivingWorldRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal064LivingWorldRowValid),
            Goal065InterlockedRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal065InterlockedRowValid),
            Goal066SettlementRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal066SettlementRowValid),
            Goal067NarrativeRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal067NarrativeRowValid),
            Goal068CombatMagicRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal068CombatMagicRowValid),
            Goal069WorldEventRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal069WorldEventRowValid && item.Goal069SaveLoadReplayPassed),
            Goal069UnityProofConsumed = goal069UnityMarkers.Count > 0 && SourceArtifactExists(sourceRefs, "world_event_unity_proof_summary"),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(IntegratedCampaignTimelineVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(IntegratedCampaignTimelineVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadGoal069StagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<TimelineDiagnostic> SortDiagnostics(IEnumerable<TimelineDiagnostic> diagnostics) =>
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

    private static void RequireFolder(string projectRoot, string relativeRoot, string sourceGoal, List<TimelineDiagnostic> diagnostics)
    {
        if (!Directory.Exists(Resolve(projectRoot, relativeRoot)))
        {
            diagnostics.Add(Error("goal070.source.required_folder_missing", sourceGoal + ":" + relativeRoot, "Required source artifact folder is missing."));
        }
    }

    private static IReadOnlyList<JsonElement> ReadRows(
        string projectRoot,
        string relativePath,
        string arrayProperty,
        string sourceGoal,
        string artifactFamily,
        IReadOnlyList<string> requiredFields,
        List<TimelineSourceArtifactReference> refs,
        List<TimelineDiagnostic> diagnostics)
    {
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal070.source.required_artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal070.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray().Select(item => item.Clone()).ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal070.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static TimelineSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = Normalize(relativePath);
        var path = Resolve(projectRoot, normalized);
        var exists = File.Exists(path);
        var diagnostics = new List<TimelineDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = requiredFields.Count == 0;
        if (!exists)
        {
            diagnostics.Add(Error("goal070.source.required_artifact_missing", normalized, "Required source artifact is missing."));
        }
        else
        {
            var bytes = File.ReadAllBytes(path);
            hash = IntegratedCampaignTimelineHash.Sha256(bytes);
            if (requiredFields.Count > 0)
            {
                try
                {
                    using var document = JsonDocument.Parse(bytes);
                    fieldsPresent = requiredFields.All(field => document.RootElement.TryGetProperty(field, out _));
                    if (!fieldsPresent)
                    {
                        diagnostics.Add(Error("goal070.source.required_field_missing", normalized, "Required top-level source fields are missing."));
                    }
                }
                catch (JsonException exception)
                {
                    diagnostics.Add(Error("goal070.source.json_invalid", normalized, exception.Message));
                }
            }
        }

        return new TimelineSourceArtifactReference
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

    private static void AddReportRefs(
        string projectRoot,
        List<TimelineSourceArtifactReference> sourceRefs,
        List<TimelineDiagnostic> diagnostics)
    {
        AddFirstMatchingFileRef(projectRoot, Goal060Root, "*report.md", "Goal060", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal061Root, "*report.md", "Goal061", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal062Root, "*report.md", "Goal062", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal063Root, "*report.md", "Goal063", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal064Root, "*report.md", "Goal064", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal065Root, "*report.md", "Goal065", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal066Root, "*report.md", "Goal066", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal067Root, "*report.md", "Goal067", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal068Root, "*report.md", "Goal068", sourceRefs, diagnostics);
        AddFirstMatchingFileRef(projectRoot, Goal069Root, "*report.md", "Goal069", sourceRefs, diagnostics);
    }

    private static void AddFirstMatchingFileRef(
        string projectRoot,
        string relativeRoot,
        string pattern,
        string sourceGoal,
        List<TimelineSourceArtifactReference> sourceRefs,
        List<TimelineDiagnostic> diagnostics)
    {
        var root = Resolve(projectRoot, relativeRoot);
        var match = Directory.Exists(root)
            ? Directory.EnumerateFiles(root, pattern, SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .FirstOrDefault()
            : null;

        if (string.IsNullOrWhiteSpace(match))
        {
            diagnostics.Add(Error("goal070.source.required_report_missing", sourceGoal + ":" + pattern, "Required source report file is missing."));
            sourceRefs.Add(FileRef(projectRoot, sourceGoal, "report", relativeRoot + "/" + pattern, []));
            return;
        }

        sourceRefs.Add(FileRef(projectRoot, sourceGoal, "report", Normalize(Path.GetRelativePath(projectRoot, match)), []));
    }

    private static void AddGoal069RowFileRefs(
        string projectRoot,
        IReadOnlyList<JsonElement> goal069Rows,
        List<TimelineSourceArtifactReference> sourceRefs,
        List<TimelineDiagnostic> diagnostics)
    {
        foreach (var row in goal069Rows)
        {
            var familyId = Text(row, "familyId");
            var seedId = Text(row, "seedId");
            var rowPath = Goal069Root + "/rows/" + familyId.Replace('_', '-') + "-" + seedId.Replace('_', '-') + "-world-event-row.json";
            sourceRefs.Add(FileRef(projectRoot, "Goal069", "world_event_row_file", rowPath, ["rowId", "familyId", "seedId", "rowHash"]));
            if (!File.Exists(Resolve(projectRoot, rowPath)))
            {
                diagnostics.Add(Error("goal070.source.required_row_file_missing", rowPath, "Required Goal 069 physical row file is missing."));
            }
        }
    }

    private static IReadOnlyList<TimelineFilePayload> LoadGoal069StagingFiles(string projectRoot, List<TimelineDiagnostic> diagnostics)
    {
        var root = Resolve(projectRoot, Goal069Root + "/" + IntegratedCampaignTimelineVocabulary.StagingRoot);
        if (!Directory.Exists(root))
        {
            diagnostics.Add(Error("goal070.source.goal069_staging_missing", Goal069Root + "/staging", "Goal 069 staging files are required for Goal 070 Unity Alpha proof."));
            return [];
        }

        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => new TimelineFilePayload
            {
                RelativePath = Normalize(Path.GetRelativePath(root, path)),
                Bytes = File.ReadAllBytes(path)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static bool SourceArtifactExists(IReadOnlyList<TimelineSourceArtifactReference> refs, string artifactFamily) =>
        refs.Any(item => item.ArtifactFamily == artifactFamily && item.Exists && item.HashMatches);

    private static IReadOnlyDictionary<string, JsonElement> ToRowMap(IEnumerable<JsonElement> rows) =>
        rows
            .Select(item => new { RowId = RowId(item), Row = item })
            .Where(item => !string.IsNullOrWhiteSpace(item.RowId))
            .GroupBy(item => item.RowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Row, StringComparer.Ordinal);

    private static void ValidateIdentity(string sourceGoal, string expectedRowId, string expectedFamilyId, string expectedSeedId, JsonElement element, List<TimelineDiagnostic> diagnostics)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            diagnostics.Add(Error("goal070.source.missing_family_row", sourceGoal + ":" + expectedRowId, "Required source row is missing for a Goal 070 family/seed row."));
            return;
        }

        var rowId = RowId(element);
        if (!string.Equals(rowId, expectedRowId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal070.source.row_identity_mismatch", sourceGoal + ":" + expectedRowId, "Source row id does not align with Goal 069 row id."));
        }

        var familyId = Text(element, "familyId");
        if (!string.IsNullOrWhiteSpace(familyId) && !string.Equals(familyId, expectedFamilyId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal070.source.family_identity_mismatch", sourceGoal + ":" + expectedRowId, "Source family id does not align with Goal 069 row family."));
        }

        var seedId = Text(element, "seedId");
        if (!string.IsNullOrWhiteSpace(seedId) && !string.Equals(seedId, expectedSeedId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal070.source.seed_identity_mismatch", sourceGoal + ":" + expectedRowId, "Source seed id does not align with Goal 069 row seed."));
        }
    }

    private static IReadOnlyList<TimelineSourceHash> UpstreamHashes(params JsonElement[] elements)
    {
        var result = new List<TimelineSourceHash>();
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

    private static void AddHash(List<TimelineSourceHash> hashes, string hashKind, string sourceRef, string hash, string sourceGoal)
    {
        if (!string.IsNullOrWhiteSpace(hash))
        {
            hashes.Add(new TimelineSourceHash
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
        if (!string.IsNullOrWhiteSpace(NestedText(element, "weatherHazard", "weatherId")))
        {
            return "Goal069";
        }

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

    private static TimelineDiagnostic Error(string code, string target, string message) =>
        TimelineDiagnostic.Error(code, target, message);
}
