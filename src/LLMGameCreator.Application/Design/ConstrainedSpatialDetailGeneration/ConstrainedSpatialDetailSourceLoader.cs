using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialDetailSourceLoader
{
    private const string Goal061Root = ConstrainedSpatialDetailVocabulary.Goal061RelativeOutputDirectory;
    private const string Goal060Root = ConstrainedSpatialDetailVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal059Root = ConstrainedSpatialDetailVocabulary.Goal059RelativeOutputDirectory;

    private sealed record SourcePlan(string SourceGoal, string ArtifactFamily, string RelativePath, IReadOnlyList<string> RequiredFields);

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        new("Goal061", "source_manifest", Goal061Root + "/source-manifest.json", ["schemaVersion", "packageRowCount", "familyIds", "seedIds"]),
        new("Goal061", "review_package_rc_manifest", Goal061Root + "/review-package-rc-manifest.json", ["schemaVersion", "rows", "packageRowCount"]),
        new("Goal061", "package_row_selection_matrix", Goal061Root + "/package-row-selection-matrix.json", ["schemaVersion", "rows", "rowCount"]),
        new("Goal061", "unity_review_package_command_plan", Goal061Root + "/unity-player-command-plan.json", ["schemaVersion", "expectedPlayerMarkers", "rows"]),
        new("Goal061", "unity_review_package_proof", Goal061Root + "/unity-player-proof-matrix.json", ["schemaVersion", "matchedMarkers", "provenRowCount"]),
        new("Goal060", "package_inventory", Goal060Root + "/materialized-package-inventory.json", ["schemaVersion", "packageCount", "packages"]),
        new("Goal060", "package_validation_matrix", Goal060Root + "/package-validation-matrix.json", ["schemaVersion", "rows"]),
        new("Goal059", "seed_profile_matrix", Goal059Root + "/seed-profile-matrix.json", ["schemaVersion", "rowCount", "rows"]),
        new("Goal059", "variance_metrics", Goal059Root + "/variance-metrics.json", ["schemaVersion", "distinctDerivedCampaignHashCount", "familySummaries"])
    ];

    public ConstrainedSpatialSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        var refs = new List<ConstrainedSpatialSourceArtifactReference>();
        var textByPath = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var plan in SourcePlans)
        {
            var reference = ReadSource(projectRoot, plan, diagnostics);
            refs.Add(reference);
            if (reference.Exists)
            {
                textByPath[Normalize(plan.RelativePath)] = File.ReadAllText(Resolve(projectRoot, plan.RelativePath), Encoding.UTF8);
            }
        }

        var stateMarkdown = ReadOptionalText(projectRoot, "docs/CURRENT_GENERATOR_STATE.md");
        var goal061Accepted = stateMarkdown.Contains("full_campaign_playable_review_package_rc_verification passed before Goal 062", StringComparison.Ordinal);
        if (!goal061Accepted)
        {
            diagnostics.Add(Error("goal062.preflight.goal061_handoff_missing", "docs/CURRENT_GENERATOR_STATE.md", "Goal 061 user-handoff acceptance must be recorded before Goal 062 source loading is accepted."));
        }

        var reviewManifest = textByPath.GetValueOrDefault(Normalize(Goal061Root + "/review-package-rc-manifest.json"), string.Empty);
        var goal061Proof = textByPath.GetValueOrDefault(Normalize(Goal061Root + "/unity-player-proof-matrix.json"), string.Empty);
        var inventory = textByPath.GetValueOrDefault(Normalize(Goal060Root + "/materialized-package-inventory.json"), string.Empty);
        var seedMatrix = textByPath.GetValueOrDefault(Normalize(Goal059Root + "/seed-profile-matrix.json"), string.Empty);
        var varianceMetrics = textByPath.GetValueOrDefault(Normalize(Goal059Root + "/variance-metrics.json"), string.Empty);
        var commandPlan = textByPath.GetValueOrDefault(Normalize(Goal061Root + "/unity-player-command-plan.json"), string.Empty);

        var inventoryByRow = ReadGoal060InventoryRows(inventory, diagnostics);
        var derivedHashes = ReadGoal059DerivedHashes(seedMatrix, diagnostics);
        var commandSteps = ReadGoal061CommandSteps(commandPlan, diagnostics);
        var rows = ReadGoal061Rows(reviewManifest, inventoryByRow, derivedHashes, commandSteps, diagnostics);
        var baseStagingFiles = ReadGoal061StagingFiles(projectRoot, diagnostics);

        return new ConstrainedSpatialSourceBundle
        {
            Goal061AcceptedByUserHandoff = goal061Accepted,
            Goal061ReviewPackageRcManifestPassed = JsonBool(reviewManifest, "passed")
                && JsonInt(reviewManifest, "packageRowCount") == 9
                && rows.Count == 9,
            Goal061UnityProofPassed = JsonBool(goal061Proof, "passed")
                && JsonBool(goal061Proof, "playerExecuted")
                && JsonNullableInt(goal061Proof, "unityExitCode") == 0
                && JsonNullableInt(goal061Proof, "playerExitCode") == 0
                && JsonInt(goal061Proof, "provenRowCount") == 9
                && JsonArrayCount(goal061Proof, "missingMarkers") == 0,
            Goal060PackageInventoryConsumed = inventoryByRow.Count == 9,
            Goal059VarianceConsumed = JsonBool(varianceMetrics, "passed")
                && JsonInt(varianceMetrics, "distinctDerivedCampaignHashCount") == 9
                && derivedHashes.Count == 9,
            FamilyIds = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(ConstrainedSpatialDetailVocabulary.FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(ConstrainedSpatialDetailVocabulary.SeedOrderingKey, StringComparer.Ordinal).ToList(),
            PackageRows = rows,
            BaseStagingFiles = baseStagingFiles,
            SourceArtifactRefs = refs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<ConstrainedSpatialPackageRowSource> ReadGoal061Rows(
        string reviewManifest,
        IReadOnlyDictionary<string, InventoryRow> inventoryByRow,
        IReadOnlyDictionary<string, string> derivedHashes,
        IReadOnlyDictionary<string, IReadOnlyList<string>> commandSteps,
        List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(reviewManifest))
        {
            diagnostics.Add(Error("goal062.source.goal061_manifest_missing", Goal061Root + "/review-package-rc-manifest.json", "Goal 061 review package RC manifest is required."));
            return [];
        }

        using var document = JsonDocument.Parse(reviewManifest);
        if (!TryGetArray(document.RootElement, "rows", out var rowsElement))
        {
            diagnostics.Add(Error("goal062.source.goal061_rows_missing", "review-package-rc-manifest.json#rows", "Goal 061 review manifest must contain rows."));
            return [];
        }

        var rows = new List<ConstrainedSpatialPackageRowSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rowsElement.EnumerateArray())
        {
            var rowId = GetString(row, "rowId");
            var familyId = GetString(row, "familyId");
            var seedId = GetString(row, "seedId");
            if (!seen.Add(rowId))
            {
                diagnostics.Add(Error("goal062.source.duplicate_row_id", rowId, "Goal 061 package rows must be unique."));
            }

            if (!ConstrainedSpatialDetailVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal062.source.fake_family", familyId, "Goal 062 only accepts the three Goal 061 family ids."));
            }

            if (!ConstrainedSpatialDetailVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal062.source.fake_seed", seedId, "Goal 062 only accepts the three Goal 061 seed ids."));
            }

            inventoryByRow.TryGetValue(rowId, out var inventory);
            if (inventory is null)
            {
                diagnostics.Add(Error("goal062.source.goal060_inventory_row_missing", rowId, "Every Goal 061 review row must map to the Goal 060 package inventory."));
            }

            derivedHashes.TryGetValue(rowId, out var derivedHash);
            if (string.IsNullOrWhiteSpace(derivedHash))
            {
                diagnostics.Add(Error("goal062.source.goal059_row_hash_missing", rowId, "Every Goal 061 review row must trace to a Goal 059 seed profile row."));
            }

            commandSteps.TryGetValue(rowId, out var steps);
            var reviewPackageRelativePath = Normalize(Goal061Root + "/review-package/" + GetString(row, "packageRelativePath").Replace("review-package-rc/", string.Empty, StringComparison.Ordinal));
            rows.Add(new ConstrainedSpatialPackageRowSource
            {
                RowId = rowId,
                FamilyId = familyId,
                SeedId = seedId,
                PackageId = GetString(row, "packageId"),
                PackageHash = GetString(row, "packageHash"),
                ReviewPackageRelativePath = reviewPackageRelativePath,
                Goal060PackageRelativePath = inventory?.PackageRelativePath ?? string.Empty,
                Goal059DerivedCampaignHash = derivedHash ?? string.Empty,
                ReviewPackageCommandSteps = steps ?? []
            });
        }

        var ordered = rows
            .OrderBy(item => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();
        if (ordered.Count != 9
            || ordered.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != 9)
        {
            diagnostics.Add(Error("goal062.source.row_matrix_invalid", Goal061Root, "Goal 062 requires nine unique Goal 061 package rows."));
        }

        return ordered;
    }

    private static SortedDictionary<string, InventoryRow> ReadGoal060InventoryRows(string json, List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, InventoryRow>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error("goal062.source.goal060_inventory_missing", Goal060Root + "/materialized-package-inventory.json", "Goal 060 package inventory is required."));
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "packages", out var packages))
        {
            diagnostics.Add(Error("goal062.source.goal060_inventory_packages_missing", "materialized-package-inventory.json#packages", "Goal 060 inventory must contain packages."));
            return result;
        }

        foreach (var package in packages.EnumerateArray())
        {
            var rowId = GetString(package, "rowId");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = new InventoryRow(Normalize(Goal060Root + "/" + GetString(package, "packageRelativePath")), GetString(package, "packageHash"));
            }
        }

        return result;
    }

    private static SortedDictionary<string, string> ReadGoal059DerivedHashes(string json, List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error("goal062.source.goal059_seed_matrix_missing", Goal059Root + "/seed-profile-matrix.json", "Goal 059 seed matrix is required."));
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal062.source.goal059_rows_missing", "seed-profile-matrix.json#rows", "Goal 059 seed matrix must contain rows."));
            return result;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var rowId = GetString(row, "rowId");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = GetString(row, "derivedCampaignHash");
            }
        }

        return result;
    }

    private static IReadOnlyList<ConstrainedSpatialFilePayload> ReadGoal061StagingFiles(
        string projectRoot,
        List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal061Root + "/staging");
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal062.source.goal061_staging_missing", Goal061Root + "/staging", "Goal 062 Unity proof requires the accepted Goal 061 staging payload."));
            return [];
        }

        var result = new List<ConstrainedSpatialFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal062.source.goal061_staging_unsafe_path", relative, "Goal 061 staging file paths must be safe relative paths before reuse."));
                continue;
            }

            result.Add(new ConstrainedSpatialFilePayload
            {
                RelativePath = Normalize(relative),
                Bytes = File.ReadAllBytes(file)
            });
        }

        if (!result.Any(item => item.RelativePath == "runtime/unity-runtime-config.json"))
        {
            diagnostics.Add(Error("goal062.source.goal061_staging_runtime_config_missing", Goal061Root + "/staging", "Goal 061 staging must include the Alpha runtime config."));
        }

        return result.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static SortedDictionary<string, IReadOnlyList<string>> ReadGoal061CommandSteps(string json, List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error("goal062.source.goal061_unity_command_plan_missing", Goal061Root + "/unity-player-command-plan.json", "Goal 061 Unity command plan is required as family command-plan equivalent."));
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal062.source.goal061_command_rows_missing", "unity-player-command-plan.json#rows", "Goal 061 command plan must contain rows."));
            return result;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var rowId = GetString(row, "rowId");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = ReadStringArray(row, "orderedStepIds");
            }
        }

        return result;
    }

    private static ConstrainedSpatialSourceArtifactReference ReadSource(
        string projectRoot,
        SourcePlan plan,
        List<ConstrainedSpatialDiagnostic> diagnostics)
    {
        var path = Resolve(projectRoot, plan.RelativePath);
        if (!File.Exists(path))
        {
            var missing = Error("goal062.source.required_artifact_missing", plan.RelativePath, "Required source artifact is missing.");
            diagnostics.Add(missing);
            return new ConstrainedSpatialSourceArtifactReference
            {
                SourceGoal = plan.SourceGoal,
                ArtifactFamily = plan.ArtifactFamily,
                ArtifactRelativePath = Normalize(plan.RelativePath),
                Exists = false,
                HashMatches = false,
                RequiredFields = plan.RequiredFields,
                Diagnostics = [missing]
            };
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        var fieldDiagnostics = RequiredFieldDiagnostics(text, plan).ToList();
        diagnostics.AddRange(fieldDiagnostics);
        return new ConstrainedSpatialSourceArtifactReference
        {
            SourceGoal = plan.SourceGoal,
            ArtifactFamily = plan.ArtifactFamily,
            ArtifactRelativePath = Normalize(plan.RelativePath),
            ArtifactHash = Hash(text),
            Exists = true,
            HashMatches = true,
            RequiredFields = plan.RequiredFields,
            Diagnostics = fieldDiagnostics
        };
    }

    private static IEnumerable<ConstrainedSpatialDiagnostic> RequiredFieldDiagnostics(string text, SourcePlan plan)
    {
        foreach (var required in plan.RequiredFields)
        {
            if (!text.Contains("\"" + required + "\"", StringComparison.Ordinal) && !text.Contains(required, StringComparison.Ordinal))
            {
                yield return Error("goal062.source.required_field_missing", plan.RelativePath + "#" + required, "Required source field was not found.");
            }
        }
    }

    public static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!TryGetArray(root, propertyName, out var array))
        {
            return [];
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static bool TryGetArray(JsonElement root, string propertyName, out JsonElement property)
    {
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty(propertyName, out property)
            && property.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        property = default;
        return false;
    }

    private static string GetString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool GetBool(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static bool JsonBool(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        using var document = JsonDocument.Parse(json);
        return GetBool(document.RootElement, propertyName);
    }

    private static int JsonInt(string json, string propertyName) => JsonNullableInt(json, propertyName) ?? 0;

    private static int? JsonNullableInt(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetInt32(out var value))
        {
            return value;
        }

        return null;
    }

    private static int JsonArrayCount(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return 0;
        }

        using var document = JsonDocument.Parse(json);
        return TryGetArray(document.RootElement, propertyName, out var array) ? array.GetArrayLength() : 0;
    }

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

    public static IReadOnlyList<ConstrainedSpatialDiagnostic> SortDiagnostics(IEnumerable<ConstrainedSpatialDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string Hash(string text) => ConstrainedSpatialDetailHash.Hash(text);

    private static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Error(code, target, message);

    private sealed record InventoryRow(string PackageRelativePath, string PackageHash);
}
