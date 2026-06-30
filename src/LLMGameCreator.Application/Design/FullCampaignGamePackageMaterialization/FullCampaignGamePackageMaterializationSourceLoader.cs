using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;

public sealed class FullCampaignGamePackageMaterializationSourceLoader
{
    private const string Goal059Root = FullCampaignGamePackageMaterializationVocabulary.Goal059RelativeOutputDirectory;

    private sealed record SourcePlan(string SourceGoal, string ArtifactFamily, string RelativePath, IReadOnlyList<string> RequiredFields);

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        new("Goal059", "source_manifest", Goal059Root + "/matrix-source-manifest.json", ["schemaVersion", "goalId", "sourceCampaignHash", "sourceArtifactRefs"]),
        new("Goal059", "seed_profile_matrix", Goal059Root + "/seed-profile-matrix.json", ["schemaVersion", "goalId", "rowCount", "rows"]),
        new("Goal059", "review_package_matrix_manifest", Goal059Root + "/review-package-matrix-manifest.json", ["schemaVersion", "requiredEvidenceFiles"]),
        new("Goal059", "preview_export_matrix_payload", Goal059Root + "/preview-export-matrix-payload.json", ["schemaVersion", "rows"]),
        new("Goal059", "unity_command_plan", Goal059Root + "/unity-alpha-matrix-command-plan.json", ["schemaVersion", "expectedPlayerMarkers"]),
        new("Goal059", "unity_player_proof", Goal059Root + "/unity-alpha-matrix-player-proof.json", ["schemaVersion", "matchedMarkers"]),
        new("Goal059", "invalid_matrix", Goal059Root + "/invalid-matrix-diagnostics.json", ["schemaVersion", "scenarios"])
    ];

    public FullCampaignSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<FullCampaignGamePackageMaterializationDiagnostic>();
        var refs = new List<FullCampaignSourceArtifactReference>();
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

        var rowRefs = ReadRows(projectRoot, textByPath, refs, diagnostics);
        var reportRelativePath = Goal059Root + "/full-generator-variability-regression-matrix-report.md";
        var reportMarkdown = ReadOptionalText(projectRoot, reportRelativePath);
        if (string.IsNullOrWhiteSpace(reportMarkdown))
        {
            diagnostics.Add(Error("goal060.source.goal059_report_missing", reportRelativePath, "Goal 059 report is required for Goal 060 handoff acceptance."));
        }
        else
        {
            refs.Add(new FullCampaignSourceArtifactReference
            {
                SourceGoal = "Goal059",
                ArtifactFamily = "goal059_report",
                ArtifactRelativePath = Normalize(reportRelativePath),
                ArtifactHash = Hash(reportMarkdown),
                Exists = true,
                HashMatches = true,
                RequiredFields = ["implementationStatus=GREEN", "accepted=false", "manualGate=full_generator_variability_regression_matrix_verification"],
                Diagnostics = []
            });
        }

        var stagingFiles = ReadGoal059StagingFiles(projectRoot, diagnostics);
        var seedProfileText = textByPath.GetValueOrDefault(Goal059Root + "/seed-profile-matrix.json", string.Empty);
        var sourceManifestText = textByPath.GetValueOrDefault(Goal059Root + "/matrix-source-manifest.json", string.Empty);

        return new FullCampaignSourceBundle
        {
            Goal059AcceptedByUserHandoff = true,
            Goal059ReportWasGreenProducedForReview = reportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && reportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && reportMarkdown.Contains("manualGate=full_generator_variability_regression_matrix_verification", StringComparison.Ordinal),
            Goal059UnityProofPassed = reportMarkdown.Contains("unityExitCode=0", StringComparison.Ordinal)
                && reportMarkdown.Contains("playerExitCode=0", StringComparison.Ordinal)
                && reportMarkdown.Contains("allMatrixMarkersMatched=true", StringComparison.Ordinal),
            Goal059SourceCampaignHash = ExtractString(sourceManifestText, "sourceCampaignHash"),
            Goal059SeedProfileMatrixHash = string.IsNullOrWhiteSpace(seedProfileText) ? string.Empty : Hash(seedProfileText),
            Goal059ReportMarkdown = reportMarkdown,
            Rows = rowRefs.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList(),
            Goal059StagingFiles = stagingFiles,
            SourceArtifactRefs = refs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static FullCampaignSourceArtifactReference ReadSource(
        string projectRoot,
        SourcePlan plan,
        List<FullCampaignGamePackageMaterializationDiagnostic> diagnostics)
    {
        var path = Resolve(projectRoot, plan.RelativePath);
        if (!File.Exists(path))
        {
            var missing = Error("goal060.source.goal059_missing", plan.RelativePath, "Required Goal 059 source artifact is missing.");
            diagnostics.Add(missing);
            return new FullCampaignSourceArtifactReference
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
        return new FullCampaignSourceArtifactReference
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

    private static IReadOnlyList<Goal059MatrixRowSource> ReadRows(
        string projectRoot,
        IReadOnlyDictionary<string, string> textByPath,
        List<FullCampaignSourceArtifactReference> refs,
        List<FullCampaignGamePackageMaterializationDiagnostic> diagnostics)
    {
        var rows = new List<Goal059MatrixRowSource>();
        var expectedFromSeedMatrix = ReadSeedMatrixRows(textByPath.GetValueOrDefault(Goal059Root + "/seed-profile-matrix.json", string.Empty), diagnostics);
        foreach (var familyId in FullCampaignGamePackageMaterializationVocabulary.FamilyIds)
        {
            foreach (var seedId in FullCampaignGamePackageMaterializationVocabulary.SeedIds)
            {
                var rowId = "matrix-row-" + SafeSegment(familyId) + "-" + SafeSegment(seedId);
                var relativePath = Goal059Root + "/" + rowId + ".json";
                var path = Resolve(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    diagnostics.Add(Error("goal060.source.goal059_row_missing", rowId, "Every Goal 059 family x seed row is required."));
                    continue;
                }

                var text = File.ReadAllText(path, Encoding.UTF8);
                using var document = JsonDocument.Parse(text);
                var root = document.RootElement;
                var row = new Goal059MatrixRowSource
                {
                    RowId = GetString(root, "rowId"),
                    FamilyId = GetString(root, "familyId"),
                    SeedId = GetString(root, "seedId"),
                    SourceCampaignHash = GetString(root, "sourceCampaignHash"),
                    DerivedCampaignHash = GetString(root, "derivedCampaignHash"),
                    RowRelativePath = Normalize(relativePath),
                    RowHash = Hash(text),
                    SourceManifestRefs = ReadStringArray(root, "sourceManifestRefs"),
                    SelectedWorldMapChunkRefs = ReadStringArray(root, "selectedWorldMapChunkRefs"),
                    SelectedMediaRefs = ReadStringArray(root, "selectedMediaRefs"),
                    SelectedFamilyLoopRefs = ReadStringArray(root, "selectedFamilyLoopRefs"),
                    SelectedPreviewExportRefs = ReadStringArray(root, "selectedPreviewExportRefs"),
                    DeterministicMarkerPlan = ReadStringArray(root, "deterministicMarkerPlan")
                };

                if (!string.Equals(row.RowId, rowId, StringComparison.Ordinal))
                {
                    diagnostics.Add(Error("goal060.source.row_id_mismatch", relativePath, "Goal 059 row id must match the deterministic family/seed file name."));
                }

                if (!string.Equals(row.FamilyId, familyId, StringComparison.Ordinal))
                {
                    diagnostics.Add(Error("goal060.source.family_id_mismatch", relativePath, "Goal 059 row family id must match the expected family."));
                }

                if (!string.Equals(row.SeedId, seedId, StringComparison.Ordinal))
                {
                    diagnostics.Add(Error("goal060.source.seed_id_mismatch", relativePath, "Goal 059 row seed id must match the expected seed."));
                }

                if (expectedFromSeedMatrix.TryGetValue(row.RowId, out var expectedHash)
                    && !string.Equals(expectedHash, row.DerivedCampaignHash, StringComparison.Ordinal))
                {
                    diagnostics.Add(Error("goal060.source.seed_matrix_hash_mismatch", row.RowId, "Goal 059 seed-profile matrix derived hash must match the row file."));
                }

                refs.Add(new FullCampaignSourceArtifactReference
                {
                    SourceGoal = "Goal059",
                    ArtifactFamily = "matrix_row",
                    ArtifactRelativePath = Normalize(relativePath),
                    ArtifactHash = row.RowHash,
                    Exists = true,
                    HashMatches = true,
                    RequiredFields = ["schemaVersion", "goalId", "rowId", "familyId", "seedId", "sourceCampaignHash", "derivedCampaignHash"],
                    Diagnostics = []
                });
                rows.Add(row);
            }
        }

        var ordered = rows.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
        if (ordered.Count != 9 || ordered.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != ordered.Count)
        {
            diagnostics.Add(Error("goal060.source.row_matrix_invalid", Goal059Root, "Goal 060 requires nine unique Goal 059 matrix rows."));
        }

        return ordered;
    }

    private static SortedDictionary<string, string> ReadSeedMatrixRows(string json, List<FullCampaignGamePackageMaterializationDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal060.source.seed_matrix_rows_missing", "seed-profile-matrix", "Goal 059 seed-profile matrix must contain rows."));
            return result;
        }

        foreach (var item in rows.EnumerateArray())
        {
            var rowId = GetString(item, "rowId");
            var hash = GetString(item, "derivedCampaignHash");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = hash;
            }
        }

        return result;
    }

    private static IReadOnlyList<FullCampaignFilePayload> ReadGoal059StagingFiles(
        string projectRoot,
        List<FullCampaignGamePackageMaterializationDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal059Root + "/staging");
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal060.source.goal059_staging_missing", Goal059Root + "/staging", "Goal 059 staging folder is required for Unity package proof."));
            return [];
        }

        var result = new List<FullCampaignFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal060.source.goal059_staging_unsafe_path", relative, "Goal 059 staging file paths must be safe relative paths."));
                continue;
            }

            result.Add(new FullCampaignFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(file)
            });
        }

        return result.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static IEnumerable<FullCampaignGamePackageMaterializationDiagnostic> RequiredFieldDiagnostics(string text, SourcePlan plan)
    {
        foreach (var required in plan.RequiredFields)
        {
            if (!text.Contains("\"" + required + "\"", StringComparison.Ordinal) && !text.Contains(required, StringComparison.Ordinal))
            {
                yield return Error("goal060.source.required_field_missing", plan.RelativePath + "#" + required, "Required source field was not found.");
            }
        }
    }

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string SeedOrderingKey(string seedId) =>
        seedId switch
        {
            "seed_alpha" => "001-seed-alpha",
            "seed_beta" => "002-seed-beta",
            "seed_gamma" => "003-seed-gamma",
            _ => "999-" + seedId
        };

    public static string SafeSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                builder.Append(ch);
            }
            else if (ch is '/' or '_' or '-' or '.')
            {
                builder.Append('-');
            }
        }

        var safe = builder.ToString().Trim('-');
        while (safe.Contains("--", StringComparison.Ordinal))
        {
            safe = safe.Replace("--", "-", StringComparison.Ordinal);
        }

        return safe.Length == 0 ? "id" : safe;
    }

    private static string ExtractString(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(json);
        return GetString(document.RootElement, propertyName);
    }

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

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> SortDiagnostics(IEnumerable<FullCampaignGamePackageMaterializationDiagnostic> diagnostics) =>
        FullCampaignGamePackageMaterializationBuilder.SortDiagnostics(diagnostics);

    private static string Hash(string text) => FullCampaignGamePackageMaterializationHash.Hash(text);

    private static FullCampaignGamePackageMaterializationDiagnostic Error(string code, string target, string message) =>
        FullCampaignGamePackageMaterializationDiagnostic.Error(code, target, message);
}
