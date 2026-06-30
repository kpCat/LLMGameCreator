using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;

public sealed class FullCampaignPlayableReviewPackageRcSourceLoader
{
    private const string Goal060Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal060RelativeOutputDirectory;
    private const string Goal059Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal059RelativeOutputDirectory;
    private const string Goal058Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal058RelativeOutputDirectory;
    private const string Goal057Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal057RelativeOutputDirectory;
    private const string Goal056Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal056RelativeOutputDirectory;
    private const string Goal055Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal055RelativeOutputDirectory;
    private const string Goal054Root = FullCampaignPlayableReviewPackageRcVocabulary.Goal054RelativeOutputDirectory;

    private sealed record SourcePlan(string SourceGoal, string ArtifactFamily, string RelativePath, IReadOnlyList<string> RequiredFields);

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        new("Goal060", "source_manifest", Goal060Root + "/source-campaign-matrix-manifest.json", ["schemaVersion", "goalId", "goal059AcceptedByUserHandoff"]),
        new("Goal060", "package_inventory", Goal060Root + "/materialized-package-inventory.json", ["schemaVersion", "goalId", "packageCount", "packages"]),
        new("Goal060", "package_validation_matrix", Goal060Root + "/package-validation-matrix.json", ["schemaVersion", "validPackageCount", "rows"]),
        new("Goal060", "runtime_consumption_matrix", Goal060Root + "/runtime-consumption-matrix.json", ["schemaVersion", "runtimePassedFamilyCount", "rows"]),
        new("Goal060", "preview_export_payloads", Goal060Root + "/preview-export-package-payloads.json", ["schemaVersion", "packageImmutabilityAuditPassed", "rows"]),
        new("Goal060", "unity_package_command_plan", Goal060Root + "/unity-package-consumption-command-plan.json", ["schemaVersion", "expectedPlayerMarkers"]),
        new("Goal060", "unity_package_proof", Goal060Root + "/unity-package-consumption-proof.json", ["schemaVersion", "matchedMarkers"]),
        new("Goal060", "invalid_matrix", Goal060Root + "/invalid-package-materialization-diagnostics-matrix.json", ["schemaVersion", "scenarios"]),
        new("Goal059", "seed_profile_matrix", Goal059Root + "/seed-profile-matrix.json", ["schemaVersion", "rowCount", "rows"]),
        new("Goal059", "unity_matrix_command_plan", Goal059Root + "/unity-alpha-matrix-command-plan.json", ["schemaVersion", "expectedPlayerMarkers"]),
        new("Goal058", "campaign_player_proof", Goal058Root + "/unity-alpha-campaign-player-proof.json", ["schemaVersion", "matchedMarkers"]),
        new("Goal058", "unified_review_package_manifest", Goal058Root + "/unified-review-package-manifest.json", ["schemaVersion", "requiredEvidenceFiles"]),
        new("Goal057", "family_command_plan", Goal057Root + "/family-command-plan.json", ["schemaVersion", "familyModes", "commands"]),
        new("Goal056", "unity_media_load_proof", Goal056Root + "/unity-media-load-proof.json", ["schemaVersion", "matchedMarkers"]),
        new("Goal055", "streaming_assets_media_manifest", Goal055Root + "/streaming-assets-media-manifest.json", ["schemaVersion", "bindings"]),
        new("Goal054", "materialized_media_inventory", Goal054Root + "/materialized-media-inventory.json", ["schemaVersion", "files"])
    ];

    public FullCampaignPlayableSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<FullCampaignPlayableReviewPackageRcDiagnostic>();
        var refs = new List<FullCampaignPlayableSourceArtifactReference>();
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

        var reportRelativePath = Goal060Root + "/full-campaign-gamepackage-materialization-report.md";
        var reportMarkdown = ReadOptionalText(projectRoot, reportRelativePath);
        if (string.IsNullOrWhiteSpace(reportMarkdown))
        {
            diagnostics.Add(Error("goal061.source.goal060_report_missing", reportRelativePath, "Goal 060 produced report is required before Goal 061 can consume it."));
        }
        else
        {
            refs.Add(new FullCampaignPlayableSourceArtifactReference
            {
                SourceGoal = "Goal060",
                ArtifactFamily = "goal060_report",
                ArtifactRelativePath = Normalize(reportRelativePath),
                ArtifactHash = Hash(reportMarkdown),
                Exists = true,
                HashMatches = true,
                RequiredFields = ["implementationStatus=GREEN", "accepted=false", "manualGate=full_campaign_gamepackage_materialization_matrix_verification"],
                Diagnostics = []
            });
        }

        var rows = ReadPackageRows(projectRoot, textByPath, refs, diagnostics);
        var mediaBindings = ReadMediaBindings(projectRoot, textByPath.GetValueOrDefault(Normalize(Goal055Root + "/streaming-assets-media-manifest.json"), string.Empty), diagnostics);
        var stagingFiles = ReadGoal059StagingFiles(projectRoot, diagnostics);
        var goal060UnityProof = textByPath.GetValueOrDefault(Normalize(Goal060Root + "/unity-package-consumption-proof.json"), string.Empty);
        var goal058UnityProof = textByPath.GetValueOrDefault(Normalize(Goal058Root + "/unity-alpha-campaign-player-proof.json"), string.Empty);
        var goal056UnityProof = textByPath.GetValueOrDefault(Normalize(Goal056Root + "/unity-media-load-proof.json"), string.Empty);

        return new FullCampaignPlayableSourceBundle
        {
            Goal060AcceptedByUserHandoff = true,
            Goal060ReportWasGreenProducedForReview = reportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && reportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && reportMarkdown.Contains("manualGate=full_campaign_gamepackage_materialization_matrix_verification", StringComparison.Ordinal),
            Goal060UnityProofPassed = JsonBool(goal060UnityProof, "passed")
                && JsonNullableInt(goal060UnityProof, "unityExitCode") == 0
                && JsonNullableInt(goal060UnityProof, "playerExitCode") == 0,
            Goal059MatrixConsumed = rows.Count == 9
                && refs.Any(item => item.ArtifactFamily == "seed_profile_matrix" && item.Exists && item.Diagnostics.Count == 0),
            Goal058CampaignProofConsumed = JsonBool(goal058UnityProof, "passed")
                || goal058UnityProof.Contains("campaign_review_package_proof=goal058", StringComparison.Ordinal),
            MediaProofChainConsumed = mediaBindings.Count == 15
                && mediaBindings.All(item => item.Exists && item.HashMatches)
                && (JsonBool(goal056UnityProof, "passed") || goal056UnityProof.Contains("media_bound_hash_validation=true", StringComparison.Ordinal)),
            PackageRows = rows,
            MediaBindings = mediaBindings,
            StagingFiles = stagingFiles,
            SourceArtifactRefs = refs.OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<FullCampaignPlayablePackageRowSource> ReadPackageRows(
        string projectRoot,
        IReadOnlyDictionary<string, string> textByPath,
        List<FullCampaignPlayableSourceArtifactReference> refs,
        List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var inventoryJson = textByPath.GetValueOrDefault(Normalize(Goal060Root + "/materialized-package-inventory.json"), string.Empty);
        var runtimeJson = textByPath.GetValueOrDefault(Normalize(Goal060Root + "/runtime-consumption-matrix.json"), string.Empty);
        var previewJson = textByPath.GetValueOrDefault(Normalize(Goal060Root + "/preview-export-package-payloads.json"), string.Empty);
        var seedMatrixJson = textByPath.GetValueOrDefault(Normalize(Goal059Root + "/seed-profile-matrix.json"), string.Empty);
        var runtimeByRow = ReadRuntimeRows(runtimeJson, diagnostics);
        var previewByRow = ReadPreviewRows(previewJson, diagnostics);
        var seedRows = ReadSeedRows(seedMatrixJson, diagnostics);

        if (string.IsNullOrWhiteSpace(inventoryJson))
        {
            diagnostics.Add(Error("goal061.source.goal060_inventory_missing", Goal060Root + "/materialized-package-inventory.json", "Goal 061 requires the Goal 060 materialized package inventory."));
            return [];
        }

        using var document = JsonDocument.Parse(inventoryJson);
        if (!TryGetArray(document.RootElement, "packages", out var packages))
        {
            diagnostics.Add(Error("goal061.source.goal060_inventory_packages_missing", "materialized-package-inventory.json#packages", "Goal 060 inventory must contain packages."));
            return [];
        }

        var rows = new List<FullCampaignPlayablePackageRowSource>();
        var seenRows = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in packages.EnumerateArray())
        {
            var rowId = GetString(item, "rowId");
            var familyId = GetString(item, "familyId");
            var seedId = GetString(item, "seedId");
            var packageRelativePath = GetString(item, "packageRelativePath");
            var sourcePackagePath = Normalize(Goal060Root + "/" + packageRelativePath);
            var expectedPackageHash = GetString(item, "packageHash");
            if (!seenRows.Add(rowId))
            {
                diagnostics.Add(Error("goal061.source.duplicate_row_id", rowId, "Goal 060 inventory row ids must be unique."));
            }

            if (!IsExpectedFamily(familyId) || !IsExpectedSeed(seedId))
            {
                diagnostics.Add(Error("goal061.source.fake_family_seed_package_row", rowId, "Goal 061 accepts only the Goal 060 family x seed matrix rows."));
            }

            if (!IsSafeRelativePath(packageRelativePath))
            {
                diagnostics.Add(Error("goal061.source.package_path_unsafe", packageRelativePath, "Package paths must stay repo-relative and traversal-free."));
                continue;
            }

            var packagePath = Resolve(projectRoot, sourcePackagePath);
            var packageJson = File.Exists(packagePath) ? File.ReadAllText(packagePath, Encoding.UTF8) : string.Empty;
            if (string.IsNullOrWhiteSpace(packageJson))
            {
                diagnostics.Add(Error("goal061.source.package_file_missing", sourcePackagePath, "Every Goal 060 inventory package file must exist physically."));
            }

            var actualHash = string.IsNullOrWhiteSpace(packageJson) ? string.Empty : Hash(packageJson.TrimEnd('\r', '\n'));
            if (!string.IsNullOrWhiteSpace(packageJson) && !IsValidJson(packageJson))
            {
                diagnostics.Add(Error("goal061.source.package_json_malformed", sourcePackagePath, "Every Goal 060 package file must be valid JSON."));
            }

            var hashVerified = string.Equals(expectedPackageHash, actualHash, StringComparison.Ordinal);
            if (!hashVerified)
            {
                diagnostics.Add(Error("goal061.source.package_hash_mismatch", rowId, "Goal 060 inventory hash must match the physical package file."));
            }

            if (!seedRows.TryGetValue(rowId, out var seedRowHash))
            {
                diagnostics.Add(Error("goal061.source.goal059_row_missing", rowId, "Every Goal 060 package row must trace to a Goal 059 matrix row."));
            }

            runtimeByRow.TryGetValue(rowId, out var runtime);
            previewByRow.TryGetValue(rowId, out var preview);
            var reviewPackageRelativePath = "review-package/p/" + rowId + ".json";
            var stagedUnityRelativePath = "review-package-rc/p/" + rowId + ".json";

            refs.Add(new FullCampaignPlayableSourceArtifactReference
            {
                SourceGoal = "Goal060",
                ArtifactFamily = "physical_package",
                ArtifactRelativePath = sourcePackagePath,
                ArtifactHash = string.IsNullOrWhiteSpace(packageJson) ? string.Empty : Hash(packageJson),
                Exists = !string.IsNullOrWhiteSpace(packageJson),
                HashMatches = hashVerified,
                RequiredFields = ["manifest", "game", "generatedContent"],
                Diagnostics = hashVerified ? [] : [Error("goal061.source.package_hash_mismatch", rowId, "Physical package hash did not match Goal 060 inventory.")]
            });

            rows.Add(new FullCampaignPlayablePackageRowSource
            {
                RowId = rowId,
                FamilyId = familyId,
                SeedId = seedId,
                PackageId = GetString(item, "packageId"),
                SourcePackageRelativePath = sourcePackagePath,
                ReviewPackageRelativePath = reviewPackageRelativePath,
                StagedUnityRelativePath = stagedUnityRelativePath,
                PackageHash = expectedPackageHash,
                PackageFileHash = actualHash,
                PackageJson = packageJson.TrimEnd('\r', '\n'),
                ValidationPassed = GetBool(item, "validationPassed"),
                PackageHashVerified = hashVerified,
                Goal059RowHash = string.IsNullOrWhiteSpace(seedRowHash) ? GetString(item, "goal059RowHash") : seedRowHash,
                RuntimeLoopKind = runtime?.RuntimeLoopKind ?? string.Empty,
                RuntimePassed = runtime?.RuntimePassed == true,
                SaveLoadRoundtripPassed = runtime?.SaveLoadRoundtripPassed == true,
                RuntimeChangedStateKeys = runtime?.ChangedStateKeys ?? [],
                RuntimeCommandIds = runtime?.CommandIds ?? [],
                RuntimeCommandTypes = runtime?.CommandTypes ?? [],
                PreviewPayloadRef = preview?.PreviewPayloadRef ?? string.Empty,
                ExportPayloadRef = preview?.ExportPayloadRef ?? string.Empty
            });
        }

        var ordered = rows
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();
        if (ordered.Count != 9 || ordered.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != 9)
        {
            diagnostics.Add(Error("goal061.source.package_matrix_invalid", Goal060Root, "Goal 061 requires nine unique Goal 060 package rows."));
        }

        return ordered;
    }

    private sealed record RuntimeRow(
        string RuntimeLoopKind,
        bool RuntimePassed,
        bool SaveLoadRoundtripPassed,
        IReadOnlyList<string> ChangedStateKeys,
        IReadOnlyList<string> CommandIds,
        IReadOnlyList<string> CommandTypes);

    private sealed record PreviewRow(string PreviewPayloadRef, string ExportPayloadRef);

    private static SortedDictionary<string, RuntimeRow> ReadRuntimeRows(string json, List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, RuntimeRow>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal061.source.runtime_rows_missing", "runtime-consumption-matrix.json#rows", "Runtime consumption matrix must contain rows."));
            return result;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var rowId = GetString(row, "rowId");
            var commandIds = new List<string>();
            var commandTypes = new List<string>();
            if (TryGetArray(row, "commands", out var commands))
            {
                foreach (var command in commands.EnumerateArray())
                {
                    commandIds.Add(GetString(command, "commandId"));
                    commandTypes.Add(GetString(command, "commandType"));
                }
            }

            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = new RuntimeRow(
                    GetString(row, "expectedRuntimeLoopKind"),
                    GetBool(row, "runtimePassed"),
                    GetBool(row, "saveLoadRoundtripPassed"),
                    ReadStringArray(row, "changedStateKeys"),
                    commandIds.Where(item => !string.IsNullOrWhiteSpace(item)).Order(StringComparer.Ordinal).ToList(),
                    commandTypes.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList());
            }
        }

        return result;
    }

    private static SortedDictionary<string, PreviewRow> ReadPreviewRows(string json, List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, PreviewRow>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal061.source.preview_rows_missing", "preview-export-package-payloads.json#rows", "Preview/export payloads must contain rows."));
            return result;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var rowId = GetString(row, "rowId");
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                result[rowId] = new PreviewRow(GetString(row, "previewPayloadRef"), GetString(row, "exportPayloadRef"));
            }
        }

        return result;
    }

    private static SortedDictionary<string, string> ReadSeedRows(string json, List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "rows", out var rows))
        {
            diagnostics.Add(Error("goal061.source.seed_matrix_rows_missing", "seed-profile-matrix.json#rows", "Goal 059 seed matrix must contain rows."));
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

    private static IReadOnlyList<FullCampaignPlayableMediaBindingSource> ReadMediaBindings(
        string projectRoot,
        string manifestJson,
        List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(manifestJson))
        {
            diagnostics.Add(Error("goal061.source.media_manifest_missing", Goal055Root + "/streaming-assets-media-manifest.json", "Goal 061 requires Goal 055 StreamingAssets media manifest."));
            return [];
        }

        using var document = JsonDocument.Parse(manifestJson);
        if (!TryGetArray(document.RootElement, "bindings", out var bindings))
        {
            diagnostics.Add(Error("goal061.source.media_bindings_missing", "streaming-assets-media-manifest.json#bindings", "Media manifest must contain bindings."));
            return [];
        }

        var result = new List<FullCampaignPlayableMediaBindingSource>();
        foreach (var item in bindings.EnumerateArray())
        {
            var sourceRelativePath = Normalize(GetString(item, "sourceGoal054RelativePath"));
            if (!IsSafeRelativePath(sourceRelativePath))
            {
                diagnostics.Add(Error("goal061.source.media_path_unsafe", sourceRelativePath, "Media source paths must be safe repo-relative paths."));
                continue;
            }

            var expectedHash = GetString(item, "sourceGoal054Sha256");
            var path = Resolve(projectRoot, sourceRelativePath);
            var exists = File.Exists(path);
            var actualHash = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty;
            var hashMatches = exists && string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase);
            if (!exists)
            {
                diagnostics.Add(Error("goal061.source.media_file_missing", sourceRelativePath, "Every media binding source file must exist."));
            }
            else if (!hashMatches)
            {
                diagnostics.Add(Error("goal061.source.media_hash_mismatch", sourceRelativePath, "Media binding hash must match Goal 054 physical media."));
            }

            result.Add(new FullCampaignPlayableMediaBindingSource
            {
                BindingId = GetString(item, "bindingId"),
                FamilyId = GetString(item, "familyId"),
                SlotId = GetString(item, "slotId"),
                MediaKind = GetString(item, "mediaKind"),
                SourceRelativePath = sourceRelativePath,
                StreamingAssetsRelativePath = GetString(item, "streamingAssetsRelativePath"),
                SourceSha256 = expectedHash,
                ActualSha256 = actualHash,
                SizeBytes = GetLong(item, "sizeBytes"),
                ReviewTrace = GetString(item, "reviewTrace"),
                Exists = exists,
                HashMatches = hashMatches
            });
        }

        return result
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => item.SlotId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<FullCampaignPlayableFilePayload> ReadGoal059StagingFiles(
        string projectRoot,
        List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal059Root + "/staging");
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal061.source.goal059_staging_missing", Goal059Root + "/staging", "Goal 059 staging folder is required for the Unity Alpha review package route."));
            return [];
        }

        var result = new List<FullCampaignPlayableFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal061.source.goal059_staging_unsafe_path", relative, "Goal 059 staging file paths must be safe relative paths."));
                continue;
            }

            result.Add(new FullCampaignPlayableFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(file)
            });
        }

        return result.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static FullCampaignPlayableSourceArtifactReference ReadSource(
        string projectRoot,
        SourcePlan plan,
        List<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics)
    {
        var path = Resolve(projectRoot, plan.RelativePath);
        if (!File.Exists(path))
        {
            var missing = Error("goal061.source.required_artifact_missing", plan.RelativePath, "Required source artifact is missing.");
            diagnostics.Add(missing);
            return new FullCampaignPlayableSourceArtifactReference
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
        return new FullCampaignPlayableSourceArtifactReference
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

    private static IEnumerable<FullCampaignPlayableReviewPackageRcDiagnostic> RequiredFieldDiagnostics(string text, SourcePlan plan)
    {
        foreach (var required in plan.RequiredFields)
        {
            if (!text.Contains("\"" + required + "\"", StringComparison.Ordinal) && !text.Contains(required, StringComparison.Ordinal))
            {
                yield return Error("goal061.source.required_field_missing", plan.RelativePath + "#" + required, "Required source field was not found.");
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

    private static long GetLong(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt64(out var value)
            ? value
            : 0;

    private static bool JsonBool(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        using var document = JsonDocument.Parse(json);
        return GetBool(document.RootElement, propertyName);
    }

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

    private static bool IsValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsExpectedFamily(string familyId) =>
        FullCampaignPlayableReviewPackageRcVocabulary.FamilyIds.Contains(familyId, StringComparer.Ordinal);

    private static bool IsExpectedSeed(string seedId) =>
        FullCampaignPlayableReviewPackageRcVocabulary.SeedIds.Contains(seedId, StringComparer.Ordinal);

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

    private static IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> SortDiagnostics(IEnumerable<FullCampaignPlayableReviewPackageRcDiagnostic> diagnostics) =>
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

    private static string Hash(string text) => FullCampaignPlayableReviewPackageRcHash.Hash(text);

    private static string HashBytes(byte[] bytes) => FullCampaignPlayableReviewPackageRcHash.HashBytes(bytes);

    private static FullCampaignPlayableReviewPackageRcDiagnostic Error(string code, string target, string message) =>
        FullCampaignPlayableReviewPackageRcDiagnostic.Error(code, target, message);
}
