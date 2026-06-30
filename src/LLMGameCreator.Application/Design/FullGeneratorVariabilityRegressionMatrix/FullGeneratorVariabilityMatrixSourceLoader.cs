using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityMatrixSourceLoader
{
    private const string Goal058Root = ".llmgc/procedural/goal-058-full-media-bound-generator-campaign";

    private sealed record SourcePlan(string SourceGoal, string ArtifactFamily, string RelativePath, IReadOnlyList<string> RequiredFields);

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        new("Goal058", "campaign_source_manifest", Goal058Root + "/campaign-source-manifest.json", ["schemaVersion", "sourceArtifactRefs"]),
        new("Goal058", "campaign_plan", Goal058Root + "/campaign-plan.json", ["schemaVersion", "stages"]),
        new("Goal058", "family_run", Goal058Root + "/family-run-map-panel-rpg.json", ["schemaVersion", "familyId", "runtimePreviewPayloadRef"]),
        new("Goal058", "family_run", Goal058Root + "/family-run-survival-sandbox.json", ["schemaVersion", "familyId", "runtimePreviewPayloadRef"]),
        new("Goal058", "family_run", Goal058Root + "/family-run-first-person-grid-dungeon.json", ["schemaVersion", "familyId", "runtimePreviewPayloadRef"]),
        new("Goal058", "review_package_manifest", Goal058Root + "/unified-review-package-manifest.json", ["schemaVersion", "streamingAssetsFiles"]),
        new("Goal058", "preview_export_payload", Goal058Root + "/preview-export-campaign-payload.json", ["schemaVersion", "previewRefs", "exportModes"]),
        new("Goal058", "unity_command_plan", Goal058Root + "/unity-alpha-campaign-command-plan.json", ["schemaVersion", "expectedPlayerMarkers"]),
        new("Goal058", "unity_player_proof", Goal058Root + "/unity-alpha-campaign-player-proof.json", ["schemaVersion", "matchedMarkers"]),
        new("Goal058", "package_compatibility", Goal058Root + "/campaign-package-compatibility-proof.json", ["schemaVersion", "publicGamePackageSchemaChanged"]),
        new("Goal058", "invalid_matrix", Goal058Root + "/invalid-campaign-diagnostics-matrix.json", ["schemaVersion", "scenarios"]),
        new("Goal058", "staging_campaign_manifest", Goal058Root + "/staging/campaign/full-media-bound-campaign-manifest.json", ["schemaVersion", "families"]),
        new("Goal058", "staging_family_command_plan", Goal058Root + "/staging/family-loop/family-command-plan.json", ["schemaVersion", "familyModes", "commands"]),
        new("Goal058", "staging_media_manifest", Goal058Root + "/staging/media-bound/unity-alpha-media-bound-manifest.json", ["schemaVersion", "bindings"])
    ];

    public FullGeneratorVariabilitySourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        var refs = new List<FullGeneratorVariabilitySourceArtifactReference>();
        var textByPath = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var plan in SourcePlans)
        {
            var path = Resolve(projectRoot, plan.RelativePath);
            if (!File.Exists(path))
            {
                var missing = Error("goal059.source.missing", plan.RelativePath, "Required Goal 058 source artifact is missing.");
                refs.Add(new FullGeneratorVariabilitySourceArtifactReference
                {
                    SourceGoal = plan.SourceGoal,
                    ArtifactFamily = plan.ArtifactFamily,
                    ArtifactRelativePath = Normalize(plan.RelativePath),
                    Exists = false,
                    HashMatches = false,
                    RequiredFields = plan.RequiredFields,
                    Diagnostics = [missing]
                });
                diagnostics.Add(missing);
                continue;
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            var fieldDiagnostics = RequiredFieldDiagnostics(text, plan).ToList();
            textByPath[Normalize(plan.RelativePath)] = text;
            refs.Add(new FullGeneratorVariabilitySourceArtifactReference
            {
                SourceGoal = plan.SourceGoal,
                ArtifactFamily = plan.ArtifactFamily,
                ArtifactRelativePath = Normalize(plan.RelativePath),
                ArtifactHash = Hash(text),
                Exists = true,
                HashMatches = true,
                RequiredFields = plan.RequiredFields,
                Diagnostics = fieldDiagnostics
            });
            diagnostics.AddRange(fieldDiagnostics);
        }

        var reportRelativePath = Goal058Root + "/full-media-bound-generator-campaign-report.md";
        var reportMarkdown = ReadOptionalText(projectRoot, reportRelativePath);
        if (string.IsNullOrWhiteSpace(reportMarkdown))
        {
            diagnostics.Add(Error("goal059.source.goal058_report_missing", reportRelativePath, "Goal 058 report is required for handoff acceptance."));
        }
        else
        {
            refs.Add(new FullGeneratorVariabilitySourceArtifactReference
            {
                SourceGoal = "Goal058",
                ArtifactFamily = "campaign_report",
                ArtifactRelativePath = Normalize(reportRelativePath),
                ArtifactHash = Hash(reportMarkdown),
                Exists = true,
                HashMatches = true,
                RequiredFields = ["implementationStatus=GREEN", "accepted=false", "unityExitCode=0", "playerExitCode=0"]
            });
        }

        var sourceHash = Hash(string.Join("|", refs
            .Where(item => item.Exists)
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .Select(item => item.ArtifactRelativePath + ":" + item.ArtifactHash)));
        var mediaRefs = ReadMediaRefs(textByPath, diagnostics);
        var loopCommands = ReadLoopCommands(textByPath, diagnostics);
        var familyRuns = ReadFamilyRuns(textByPath, mediaRefs, loopCommands, diagnostics);
        var stagingFiles = ReadGoal058StagingFiles(projectRoot, diagnostics);

        return new FullGeneratorVariabilitySourceBundle
        {
            Goal058CampaignId = ExtractString(textByPath, Goal058Root + "/campaign-plan.json", "campaignId"),
            Goal058ReportWasGreenProducedForReview = reportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && reportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && reportMarkdown.Contains("manualGate=full_media_bound_generator_campaign_verification", StringComparison.Ordinal),
            Goal058UnityProofPassed = reportMarkdown.Contains("unityExitCode=0", StringComparison.Ordinal)
                && reportMarkdown.Contains("playerExitCode=0", StringComparison.Ordinal)
                && reportMarkdown.Contains("allCampaignMarkersMatched=true", StringComparison.Ordinal),
            Goal058ReportMarkdown = reportMarkdown,
            SourceCampaignHash = sourceHash,
            Families = familyRuns.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ToList(),
            Goal058UnityMatchedMarkers = ReadStringArray(textByPath, Goal058Root + "/unity-alpha-campaign-player-proof.json", "matchedMarkers"),
            Goal058StagingFiles = stagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<FullGeneratorVariabilityFamilySource> ReadFamilyRuns(
        IReadOnlyDictionary<string, string> textByPath,
        IReadOnlyList<FullGeneratorVariabilityMediaRef> mediaRefs,
        IReadOnlyList<FullGeneratorVariabilityLoopCommandRef> loopCommands,
        List<FullGeneratorVariabilityDiagnostic> diagnostics)
    {
        var result = new List<FullGeneratorVariabilityFamilySource>();
        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            var path = Goal058Root + "/family-run-" + SafeSegment(familyId) + ".json";
            if (!textByPath.TryGetValue(path, out var json))
            {
                diagnostics.Add(Error("goal059.source.family_run_missing", familyId, "Goal 058 family run artifact is required."));
                continue;
            }

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var familyMedia = mediaRefs.Where(item => item.FamilyId == familyId).OrderBy(item => SlotOrder(item.SlotId)).ThenBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
            var commands = loopCommands.Where(item => item.FamilyId == familyId).OrderBy(item => item.Order).ToList();
            result.Add(new FullGeneratorVariabilityFamilySource
            {
                FamilyId = GetString(root, "familyId"),
                ScenarioId = GetString(root, "scenarioId"),
                ProfileId = GetString(root, "profileId"),
                FamilyRunRef = path,
                Goal057LoopProofRef = ReadStringArray(root, "sourceRefs").FirstOrDefault(item => item.Contains("goal-057-unity-alpha-multifamily-playable-loop", StringComparison.Ordinal)) ?? string.Empty,
                RuntimePreviewPayloadRef = GetString(root, "runtimePreviewPayloadRef"),
                ExportMode = GetString(root, "exportMode"),
                CommandCount = GetInt(root, "commandCount"),
                MediaFileCount = GetInt(root, "mediaFileCount"),
                ExpectedCampaignMarkers = ReadStringArray(root, "expectedCampaignMarkers"),
                MediaRefs = familyMedia,
                LoopCommands = commands
            });
        }

        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            if (!result.Any(item => item.FamilyId == familyId))
            {
                diagnostics.Add(Error("goal059.source.family_missing", familyId, "Goal 059 matrix requires all three Goal 058 families."));
            }
        }

        return result;
    }

    private static IReadOnlyList<FullGeneratorVariabilityMediaRef> ReadMediaRefs(
        IReadOnlyDictionary<string, string> textByPath,
        List<FullGeneratorVariabilityDiagnostic> diagnostics)
    {
        var path = Goal058Root + "/staging/media-bound/unity-alpha-media-bound-manifest.json";
        if (!textByPath.TryGetValue(path, out var json))
        {
            diagnostics.Add(Error("goal059.source.media_manifest_missing", path, "Goal 058 staged media manifest is required."));
            return [];
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "bindings", out var bindings))
        {
            diagnostics.Add(Error("goal059.source.media_bindings_missing", path, "Goal 058 media manifest must contain bindings."));
            return [];
        }

        return bindings.EnumerateArray()
            .Select(item => new FullGeneratorVariabilityMediaRef
            {
                BindingId = GetString(item, "bindingId"),
                FamilyId = GetString(item, "familyId"),
                SlotId = GetString(item, "slotId"),
                MediaKind = GetString(item, "mediaKind"),
                RelativePath = GetString(item, "relativePath"),
                Sha256 = GetString(item, "sha256"),
                ReviewTrace = GetString(item, "reviewTrace")
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.FamilyId))
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SlotOrder(item.SlotId))
            .ThenBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<FullGeneratorVariabilityLoopCommandRef> ReadLoopCommands(
        IReadOnlyDictionary<string, string> textByPath,
        List<FullGeneratorVariabilityDiagnostic> diagnostics)
    {
        var path = Goal058Root + "/staging/family-loop/family-command-plan.json";
        if (!textByPath.TryGetValue(path, out var json))
        {
            diagnostics.Add(Error("goal059.source.family_command_plan_missing", path, "Goal 058 staged family command plan is required."));
            return [];
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetArray(document.RootElement, "commands", out var commands))
        {
            diagnostics.Add(Error("goal059.source.family_commands_missing", path, "Goal 058 family command plan must contain commands."));
            return [];
        }

        return commands.EnumerateArray()
            .Select(item => new FullGeneratorVariabilityLoopCommandRef
            {
                FamilyId = GetString(item, "familyId"),
                ScenarioId = GetString(item, "scenarioId"),
                Order = GetInt(item, "order"),
                CommandId = GetString(item, "commandId"),
                CommandType = GetString(item, "commandType"),
                TargetId = GetString(item, "targetId"),
                SecondaryTargetId = GetString(item, "secondaryTargetId"),
                FamilyMarker = GetString(item, "familyMarker"),
                ExpectedStatus = GetString(item, "expectedStatus"),
                ExpectedPlayerMarker = GetString(item, "expectedPlayerMarker")
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.FamilyId))
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => item.Order)
            .ToList();
    }

    private static IReadOnlyList<FullGeneratorVariabilityFilePayload> ReadGoal058StagingFiles(
        string projectRoot,
        List<FullGeneratorVariabilityDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, Goal058Root + "/staging");
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal059.source.goal058_staging_missing", Goal058Root + "/staging", "Goal 058 staging folder is required for Unity matrix proof."));
            return [];
        }

        var result = new List<FullGeneratorVariabilityFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal059.source.goal058_staging_unsafe_path", relative, "Goal 058 staging file paths must be safe relative paths."));
                continue;
            }

            result.Add(new FullGeneratorVariabilityFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(file)
            });
        }

        return result;
    }

    private static IEnumerable<FullGeneratorVariabilityDiagnostic> RequiredFieldDiagnostics(string text, SourcePlan plan)
    {
        foreach (var required in plan.RequiredFields)
        {
            if (!text.Contains("\"" + required + "\"", StringComparison.Ordinal) && !text.Contains(required, StringComparison.Ordinal))
            {
                yield return Error("goal059.source.required_field_missing", plan.RelativePath + "#" + required, "Required source field was not found.");
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

    public static int SlotOrder(string slotId) =>
        slotId switch
        {
            "world_key_art" => 1,
            "npc_portrait" => 2,
            "ui_panel_skin" => 3,
            "sfx_interaction" => 4,
            "export_placeholder_bundle" => 5,
            _ => 999
        };

    private static string ExtractString(IReadOnlyDictionary<string, string> textByPath, string relativePath, string propertyName)
    {
        if (!textByPath.TryGetValue(Normalize(relativePath), out var text) || string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(text);
        return GetString(document.RootElement, propertyName);
    }

    private static IReadOnlyList<string> ReadStringArray(IReadOnlyDictionary<string, string> textByPath, string relativePath, string propertyName)
    {
        if (!textByPath.TryGetValue(Normalize(relativePath), out var text) || string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        using var document = JsonDocument.Parse(text);
        return ReadStringArray(document.RootElement, propertyName);
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

    private static int GetInt(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
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

    private static IReadOnlyList<FullGeneratorVariabilityDiagnostic> SortDiagnostics(IEnumerable<FullGeneratorVariabilityDiagnostic> diagnostics) =>
        FullGeneratorVariabilityMatrixBuilder.SortDiagnostics(diagnostics);

    private static string Hash(string text) => FullGeneratorVariabilityMatrixHash.Hash(text);

    private static FullGeneratorVariabilityDiagnostic Error(string code, string target, string message) =>
        FullGeneratorVariabilityDiagnostic.Error(code, target, message);
}
