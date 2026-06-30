using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;

public sealed class GameplayConsequenceDepthMatrixSourceLoader
{
    private static readonly IReadOnlyList<string> RequiredStagingFiles =
    [
        "runtime/unity-runtime-config.json",
        "game-data/game-package.json",
        "assets/asset-manifest.json",
        "export-manifest.json"
    ];

    public GameplayConsequenceSourceBundle Load(string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        var sourceRefs = new List<GameplayConsequenceSourceArtifactReference>();

        var goal060Packages = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal060RelativeOutputDirectory + "/materialized-package-inventory.json",
            "packages",
            "Goal060",
            "materialized_package_inventory",
            ["packageCount", "packages"],
            sourceRefs,
            diagnostics);
        var goal060Runtime = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal060RelativeOutputDirectory + "/runtime-consumption-matrix.json",
            "rows",
            "Goal060",
            "runtime_consumption_matrix",
            ["passed", "rows"],
            sourceRefs,
            diagnostics);
        var goal061Rows = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal061RelativeOutputDirectory + "/package-row-selection-matrix.json",
            "rows",
            "Goal061",
            "package_row_selection_matrix",
            ["passed", "rows"],
            sourceRefs,
            diagnostics);
        var goal061SaveLoad = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal061RelativeOutputDirectory + "/save-load-replay-package-row-audit.json",
            "rows",
            "Goal061",
            "save_load_replay_audit",
            ["passed", "rows"],
            sourceRefs,
            diagnostics);
        var goal062Spatial = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal062RelativeOutputDirectory + "/spatial-detail-matrix.json",
            "rows",
            "Goal062",
            "spatial_detail_matrix",
            ["passed", "rows"],
            sourceRefs,
            diagnostics);
        _ = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal062RelativeOutputDirectory + "/unity-spatial-detail-command-plan.json",
            "rows",
            "Goal062",
            "unity_spatial_detail_command_plan",
            ["passed", "rows"],
            sourceRefs,
            diagnostics);
        _ = ReadObjectRows(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal062RelativeOutputDirectory + "/unity-spatial-detail-proof-summary.json",
            "matchedMarkers",
            "Goal062",
            "unity_spatial_detail_proof_summary",
            ["passed", "provenRowCount", "matchedMarkers"],
            sourceRefs,
            diagnostics);

        var runtimeByRowId = goal060Runtime.ToDictionary(RowId, StringComparer.Ordinal);
        var reviewByRowId = goal061Rows.ToDictionary(RowId, StringComparer.Ordinal);
        var saveByRowId = goal061SaveLoad.ToDictionary(RowId, StringComparer.Ordinal);
        var spatialByRowId = goal062Spatial.ToDictionary(RowId, StringComparer.Ordinal);

        var rows = new List<GameplayConsequenceSourceRow>();
        foreach (var package in goal060Packages)
        {
            var rowId = RowId(package);
            if (string.IsNullOrWhiteSpace(rowId))
            {
                continue;
            }

            var familyId = Text(package, "familyId");
            var seedId = Text(package, "seedId");
            if (!reviewByRowId.TryGetValue(rowId, out var review))
            {
                diagnostics.Add(Error("goal063.source.goal061_row_missing", rowId, "Goal 061 review package row is required."));
                continue;
            }

            if (!spatialByRowId.TryGetValue(rowId, out var spatial))
            {
                diagnostics.Add(Error("goal063.source.goal062_row_missing", rowId, "Goal 062 spatial detail row is required."));
                continue;
            }

            var hasRuntime = runtimeByRowId.TryGetValue(rowId, out var runtime);
            var hasSave = saveByRowId.TryGetValue(rowId, out var save);
            var spatialFile = GameplayConsequenceDepthMatrixVocabulary.Goal062RelativeOutputDirectory
                + "/spatial-detail-row-" + familyId + "-" + seedId + ".json";
            sourceRefs.Add(FileRef(
                projectRoot,
                "Goal062",
                "spatial_detail_row",
                spatialFile,
                ["rowId", "familyId", "seedId", "rowHash"]));

            rows.Add(new GameplayConsequenceSourceRow
            {
                RowId = rowId,
                FamilyId = familyId,
                SeedId = seedId,
                SourcePackageRowRef = "Goal060:" + rowId,
                SourceReviewPackageRowRef = "Goal061:" + rowId,
                SourceSpatialDetailRowRef = "Goal062:" + rowId,
                PackageId = Text(package, "packageId"),
                PackageHash = Text(package, "packageHash"),
                Goal060PackageRelativePath = Text(package, "packageRelativePath"),
                ReviewPackageRelativePath = Text(review, "packageRelativePath"),
                SpatialDetailRowHash = Text(spatial, "rowHash"),
                SpatialVarianceMarker = Text(spatial, "varianceMarker"),
                Goal060RuntimeStateChanged = hasRuntime && Bool(runtime, "stateChanged"),
                Goal060SaveLoadRoundtripPassed = hasRuntime && Bool(runtime, "saveLoadRoundtripPassed"),
                Goal061SaveLoadReplayVerified = Bool(review, "saveLoadReplayVerified")
                    && (!hasSave || (Bool(save, "saveLoadRoundtripPassed") && Bool(save, "replayDeterminismPassed"))),
                Goal062Reachable = Bool(spatial, "reachable"),
                Goal062RouteVerified = Bool(spatial, "routeVerified"),
                Goal060ChangedStateKeys = hasRuntime ? StringArray(runtime, "changedStateKeys") : [],
                Goal061RuntimeCommandIds = hasSave ? StringArray(save, "runtimeCommandIds") : [],
                Goal061ReviewCommandSteps = StringArray(review, "commandPlanSteps")
            });
        }

        var orderedRows = rows
            .OrderBy(item => GameplayConsequenceDepthMatrixVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => GameplayConsequenceDepthMatrixVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();

        return new GameplayConsequenceSourceBundle
        {
            Goal060AcceptedByUserHandoff = true,
            Goal061AcceptedByUserHandoff = true,
            Goal062AcceptedByUserHandoff = true,
            Goal060PackageRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => !string.IsNullOrWhiteSpace(item.PackageHash)),
            Goal061ReviewRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061SaveLoadReplayVerified),
            Goal062SpatialRowsConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal062Reachable && item.Goal062RouteVerified),
            Goal060RuntimeProofConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal060RuntimeStateChanged && item.Goal060SaveLoadRoundtripPassed),
            Goal061SaveLoadReplayConsumed = orderedRows.Count == 9 && orderedRows.All(item => item.Goal061SaveLoadReplayVerified),
            Goal062UnityProofConsumed = sourceRefs.Any(item => item.ArtifactFamily == "unity_spatial_detail_proof_summary" && item.Exists && item.HashMatches),
            FamilyIds = orderedRows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(GameplayConsequenceDepthMatrixVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = orderedRows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(GameplayConsequenceDepthMatrixVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            Rows = orderedRows,
            BaseStagingFiles = LoadBaseStagingFiles(projectRoot, diagnostics),
            SourceArtifactRefs = sourceRefs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<GameplayConsequenceDiagnostic> SortDiagnostics(IEnumerable<GameplayConsequenceDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<JsonElement> ReadObjectRows(
        string projectRoot,
        string relativePath,
        string arrayProperty,
        string sourceGoal,
        string artifactFamily,
        IReadOnlyList<string> requiredFields,
        List<GameplayConsequenceSourceArtifactReference> refs,
        List<GameplayConsequenceDiagnostic> diagnostics)
    {
        var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        refs.Add(FileRef(projectRoot, sourceGoal, artifactFamily, relativePath, requiredFields));
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal063.source.artifact_missing", relativePath, "Required source artifact is missing."));
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            if (!doc.RootElement.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                diagnostics.Add(Error("goal063.source.array_missing", relativePath + "#" + arrayProperty, "Required source artifact array is missing."));
                return [];
            }

            return array.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.String)
                .Select(item => item.Clone())
                .ToList();
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal063.source.json_invalid", relativePath, exception.Message));
            return [];
        }
    }

    private static GameplayConsequenceSourceArtifactReference FileRef(
        string projectRoot,
        string sourceGoal,
        string artifactFamily,
        string relativePath,
        IReadOnlyList<string> requiredFields)
    {
        var normalized = relativePath.Replace('\\', '/');
        var path = Path.Combine(projectRoot, normalized.Replace('/', Path.DirectorySeparatorChar));
        var exists = File.Exists(path);
        var diagnostics = new List<GameplayConsequenceDiagnostic>();
        var hash = string.Empty;
        var fieldsPresent = true;
        if (exists)
        {
            var bytes = File.ReadAllBytes(path);
            hash = GameplayConsequenceDepthMatrixHash.HashBytes(bytes);
            if (requiredFields.Count > 0)
            {
                var text = Encoding.UTF8.GetString(bytes);
                foreach (var field in requiredFields)
                {
                    if (!text.Contains("\"" + field + "\"", StringComparison.Ordinal))
                    {
                        fieldsPresent = false;
                        diagnostics.Add(Error("goal063.source.required_field_missing", normalized + "#" + field, "Required source field is missing."));
                    }
                }
            }
        }
        else
        {
            fieldsPresent = false;
            diagnostics.Add(Error("goal063.source.file_missing", normalized, "Required source file is missing."));
        }

        return new GameplayConsequenceSourceArtifactReference
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

    private static IReadOnlyList<GameplayConsequenceFilePayload> LoadBaseStagingFiles(
        string projectRoot,
        List<GameplayConsequenceDiagnostic> diagnostics)
    {
        var files = new List<GameplayConsequenceFilePayload>();
        var stagingRoot = Path.Combine(
            projectRoot,
            GameplayConsequenceDepthMatrixVocabulary.Goal062RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar),
            GameplayConsequenceDepthMatrixVocabulary.StagingRoot);

        foreach (var relative in RequiredStagingFiles)
        {
            var path = Path.Combine(stagingRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                if (relative != "export-manifest.json")
                {
                    diagnostics.Add(Error("goal063.staging.source_file_missing", relative, "Goal 063 Unity proof needs the existing Alpha staging payload file."));
                }

                continue;
            }

            files.Add(new GameplayConsequenceFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(path)
            });
        }

        return files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
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

    private static bool Bool(JsonElement item, string propertyName)
    {
        if (item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty(propertyName, out var property)
            || property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        return property.GetBoolean();
    }

    private static IReadOnlyList<string> StringArray(JsonElement item, string propertyName)
    {
        if (item.ValueKind != JsonValueKind.Object
            || !item.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property.EnumerateArray()
            .Where(value => value.ValueKind == JsonValueKind.String)
            .Select(value => value.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
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

    private static GameplayConsequenceDiagnostic Error(string code, string target, string message) =>
        GameplayConsequenceDiagnostic.Error(code, target, message);
}
