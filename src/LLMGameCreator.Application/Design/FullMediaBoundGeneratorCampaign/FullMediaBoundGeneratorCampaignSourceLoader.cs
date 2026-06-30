using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignSourceLoader
{
    private sealed record SourcePlan(string SourceGoal, string ArtifactFamily, string RelativePath, IReadOnlyList<string> RequiredFields);

    private static readonly IReadOnlyList<SourcePlan> SourcePlans =
    [
        new("Goal034", "strict_draft", ".llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/draft-loop-contract-summary.json", ["schemaVersion"]),
        new("Goal034", "strict_draft", ".llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/candidate-quarantine-matrix.json", ["schemaVersion"]),
        new("Goal034", "strict_draft", ".llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/promotion-decision-matrix.json", ["schemaVersion"]),
        new("Goal035", "lua_manifest", ".llmgc/procedural/goal-035-lua-module-manifest-registry/lua-module-registry-summary.json", ["schemaVersion"]),
        new("Goal035", "lua_manifest", ".llmgc/procedural/goal-035-lua-module-manifest-registry/lua-module-dependency-plan.json", ["schemaVersion"]),
        new("Goal036", "lua_sandbox", ".llmgc/procedural/goal-036-lua-sandbox-execution-gate/lua-sandbox-policy-summary.json", ["schemaVersion"]),
        new("Goal036", "lua_sandbox", ".llmgc/procedural/goal-036-lua-sandbox-execution-gate/lua-sandbox-dry-run-trace-matrix.json", ["schemaVersion"]),
        new("Goal037", "hybrid_expansion", ".llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/hybrid-pipeline-summary.json", ["schemaVersion"]),
        new("Goal037", "hybrid_expansion", ".llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/sandbox-approved-expansion-matrix.json", ["schemaVersion"]),
        new("Goal038", "world_region", ".llmgc/procedural/goal-038-world-scale-region-map-foundation/region-graph-summary.json", ["schemaVersion"]),
        new("Goal038", "world_region", ".llmgc/procedural/goal-038-world-scale-region-map-foundation/chunked-world-config-prelude.json", ["schemaVersion"]),
        new("Goal039", "runtime_delta", ".llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/runtime-save-load-roundtrip-proof.json", ["schemaVersion"]),
        new("Goal039", "runtime_delta", ".llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/chunk-replay-determinism-proof.json", ["schemaVersion"]),
        new("Goal040", "chunked_preview_export", ".llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-export-manifest.json", ["schemaVersion"]),
        new("Goal040", "chunked_preview_export", ".llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/runtime-preview-consumption-proof.json", ["schemaVersion"]),
        new("Goal043", "family_loop", ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/shared-lifecycle-contract.json", ["schemaVersion"]),
        new("Goal043", "family_loop", ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/family-template-catalog.json", ["schemaVersion"]),
        new("Goal047", "full_generator_without_media", ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/dry-run-source-manifest.json", ["schemaVersion"]),
        new("Goal047", "full_generator_without_media", ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/package-compatibility-or-materialization-summary.json", ["schemaVersion"]),
        new("Goal053", "media_campaign", ".llmgc/procedural/goal-053-media-asset-campaign-orchestration/media-campaign-source-manifest.json", ["schemaVersion"]),
        new("Goal053", "media_campaign", ".llmgc/procedural/goal-053-media-asset-campaign-orchestration/media-binding-manifest.json", ["schemaVersion"]),
        new("Goal054", "media_materialization", ".llmgc/procedural/goal-054-media-materialization-review-package/materialized-media-inventory.json", ["schemaVersion"]),
        new("Goal054", "media_materialization", ".llmgc/procedural/goal-054-media-materialization-review-package/media-review-package-manifest.json", ["schemaVersion"]),
        new("Goal055", "media_bound_review_package", ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/media-bound-review-package-manifest.json", ["schemaVersion"]),
        new("Goal055", "media_bound_review_package", ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/streaming-assets-media-manifest.json", ["schemaVersion"]),
        new("Goal056", "unity_media_bound", ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/source-evidence-manifest.json", ["schemaVersion"]),
        new("Goal056", "unity_media_bound", ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/unity-media-load-proof.json", ["schemaVersion"]),
        new("Goal056", "unity_media_bound", ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/unity-alpha-media-bound-smoke-log-summary.json", ["schemaVersion"]),
        new("Goal057", "unity_multifamily_loop", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/source-manifest.json", ["schemaVersion"]),
        new("Goal057", "unity_multifamily_loop", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/family-command-plan.json", ["schemaVersion", "familyModes", "commands"]),
        new("Goal057", "unity_multifamily_loop", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/player-log-summary.json", ["schemaVersion"]),
        new("Goal057", "unity_multifamily_loop", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/review-package-manifest.json", ["schemaVersion"]),
        new("Goal057", "unity_multifamily_loop", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/media-binding-validation.json", ["schemaVersion"])
    ];

    public FullMediaBoundCampaignSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        var refs = new List<FullMediaBoundCampaignSourceArtifactReference>();
        var artifactTextByPath = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var plan in SourcePlans)
        {
            var path = Resolve(projectRoot, plan.RelativePath);
            if (!File.Exists(path))
            {
                refs.Add(new FullMediaBoundCampaignSourceArtifactReference
                {
                    SourceGoal = plan.SourceGoal,
                    ArtifactFamily = plan.ArtifactFamily,
                    ArtifactRelativePath = Normalize(plan.RelativePath),
                    Exists = false,
                    HashMatches = false,
                    RequiredFields = plan.RequiredFields,
                    Diagnostics =
                    [
                        Error("goal058.source.missing", plan.RelativePath, "Required campaign source artifact is missing.")
                    ]
                });
                continue;
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            var fieldDiagnostics = RequiredFieldDiagnostics(text, plan).ToList();
            artifactTextByPath[Normalize(plan.RelativePath)] = text;
            refs.Add(new FullMediaBoundCampaignSourceArtifactReference
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

        var goal057ReportPath = ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/unity-alpha-multifamily-playable-loop-report.md";
        var goal057Report = ReadOptionalText(projectRoot, goal057ReportPath);
        if (!string.IsNullOrWhiteSpace(goal057Report))
        {
            refs.Add(new FullMediaBoundCampaignSourceArtifactReference
            {
                SourceGoal = "Goal057",
                ArtifactFamily = "unity_multifamily_loop_report",
                ArtifactRelativePath = Normalize(goal057ReportPath),
                ArtifactHash = Hash(goal057Report),
                Exists = true,
                HashMatches = true,
                RequiredFields = ["implementationStatus=GREEN", "accepted=false", "unityExitCode=0", "playerExitCode=0"]
            });
        }
        else
        {
            diagnostics.Add(Error("goal058.source.goal057_report_missing", goal057ReportPath, "Goal 057 report is required for preflight acceptance."));
        }

        var familySources = ReadFamilySources(projectRoot, artifactTextByPath, diagnostics);
        var stagingFiles = ReadGoal057StagingFiles(projectRoot, diagnostics);

        return new FullMediaBoundCampaignSourceBundle
        {
            Goal057ReportWasGreenProducedForReview = goal057Report.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && goal057Report.Contains("accepted=false", StringComparison.Ordinal)
                && goal057Report.Contains("manualGate=unity_alpha_multifamily_playable_loop_verification", StringComparison.Ordinal),
            Goal057UnityProofPassed = goal057Report.Contains("unityExitCode=0", StringComparison.Ordinal)
                && goal057Report.Contains("playerExitCode=0", StringComparison.Ordinal)
                && goal057Report.Contains("allFamilyLoopsVerified=true", StringComparison.Ordinal),
            Goal057ReportMarkdown = goal057Report,
            Families = familySources.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ToList(),
            Goal057StagingFiles = stagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<FullMediaBoundCampaignFamilySource> ReadFamilySources(
        string projectRoot,
        IReadOnlyDictionary<string, string> artifactTextByPath,
        List<FullMediaBoundCampaignDiagnostic> diagnostics)
    {
        var commandPlanPath = ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/family-command-plan.json";
        if (!artifactTextByPath.TryGetValue(commandPlanPath, out var commandPlanText))
        {
            diagnostics.Add(Error("goal058.source.goal057_command_plan_missing", commandPlanPath, "Goal 057 family command plan is required."));
            return [];
        }

        using var commandPlan = JsonDocument.Parse(commandPlanText);
        var families = ReadFamilyModes(commandPlan.RootElement);
        var commands = ReadCommands(commandPlan.RootElement);
        var result = new List<FullMediaBoundCampaignFamilySource>();
        foreach (var family in families.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal))
        {
            var segment = family.FamilyId.Replace('_', '-');
            var dryRunRef = ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-" + segment + "-dry-run.json";
            var loopProofRef = ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/family-loop-proof-" + segment + ".json";
            var dryRunText = ReadOptionalText(projectRoot, dryRunRef);
            var mediaFileCount = CountStagedMediaFiles(projectRoot, family.FamilyId);
            result.Add(new FullMediaBoundCampaignFamilySource
            {
                FamilyId = family.FamilyId,
                ScenarioId = family.ScenarioId,
                ProfileId = family.ProfileId,
                Goal047DryRunRef = dryRunRef,
                Goal057LoopProofRef = loopProofRef,
                RuntimePreviewPayloadRef = ExtractNestedString(dryRunText, "runtimePreviewPayloadSummary", "payloadRelativePath"),
                ExportMode = ExtractNestedString(dryRunText, "exportCandidatePayloadSummary", "exportMode"),
                MediaFileCount = mediaFileCount,
                LoopCommands = commands
                    .Where(item => item.FamilyId == family.FamilyId)
                    .OrderBy(item => item.Order)
                    .ToList()
            });
        }

        foreach (var familyId in FullMediaBoundGeneratorCampaignVocabulary.FamilyIds)
        {
            if (!result.Any(item => item.FamilyId == familyId))
            {
                diagnostics.Add(Error("goal058.source.family_missing", familyId, "Goal 058 must consume all three Goal 057 family modes."));
            }
        }

        return result;
    }

    private static IReadOnlyList<FullMediaBoundCampaignFilePayload> ReadGoal057StagingFiles(
        string projectRoot,
        List<FullMediaBoundCampaignDiagnostic> diagnostics)
    {
        var stagingRoot = Resolve(projectRoot, ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/staging");
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal058.source.goal057_staging_missing", ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/staging", "Goal 057 staging folder is required for campaign Unity staging."));
            return [];
        }

        var result = new List<FullMediaBoundCampaignFilePayload>();
        foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
            if (!IsSafeRelativePath(relative))
            {
                diagnostics.Add(Error("goal058.source.goal057_staging_unsafe_path", relative, "Goal 057 staging files must stay under safe relative paths."));
                continue;
            }

            result.Add(new FullMediaBoundCampaignFilePayload
            {
                RelativePath = relative,
                Bytes = File.ReadAllBytes(file)
            });
        }

        return result;
    }

    private static IEnumerable<FullMediaBoundCampaignDiagnostic> RequiredFieldDiagnostics(string text, SourcePlan plan)
    {
        foreach (var required in plan.RequiredFields)
        {
            if (!text.Contains("\"" + required + "\"", StringComparison.Ordinal) && !text.Contains(required, StringComparison.Ordinal))
            {
                yield return Error("goal058.source.required_field_missing", plan.RelativePath + "#" + required, "Required source field was not found.");
            }
        }
    }

    private static IReadOnlyList<(string FamilyId, string ScenarioId, string ProfileId)> ReadFamilyModes(JsonElement root)
    {
        if (!TryGetArray(root, "familyModes", out var familyModes))
        {
            return [];
        }

        return familyModes.EnumerateArray()
            .Select(item => (
                FamilyId: GetString(item, "familyId"),
                ScenarioId: GetString(item, "scenarioId"),
                ProfileId: GetString(item, "profileId")))
            .Where(item => !string.IsNullOrWhiteSpace(item.FamilyId))
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<FullMediaBoundCampaignLoopCommand> ReadCommands(JsonElement root)
    {
        if (!TryGetArray(root, "commands", out var commands))
        {
            return [];
        }

        return commands.EnumerateArray()
            .Select(item =>
            {
                var familyId = GetString(item, "familyId");
                var order = GetInt(item, "order");
                var commandType = GetString(item, "commandType");
                var familyMarker = GetString(item, "familyMarker");
                var expectedStatus = GetString(item, "expectedStatus");
                return new FullMediaBoundCampaignLoopCommand
                {
                    FamilyId = familyId,
                    ScenarioId = GetString(item, "scenarioId"),
                    Order = order,
                    CommandType = commandType,
                    FamilyMarker = familyMarker,
                    ExpectedStatus = expectedStatus,
                    ExpectedPlayerMarker = "family_loop_step=" + familyId + ":" + order + ":" + commandType + ":" + familyMarker + ":" + expectedStatus
                };
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.FamilyId))
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => item.Order)
            .ToList();
    }

    private static string ExtractNestedString(string json, string objectProperty, string nestedProperty)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.TryGetProperty(objectProperty, out var nested)
            && nested.ValueKind == JsonValueKind.Object
            && nested.TryGetProperty(nestedProperty, out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static int CountStagedMediaFiles(string projectRoot, string familyId)
    {
        var familySegment = familyId.Replace('_', '-');
        var path = Resolve(projectRoot, ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/staging/media-bound/media/" + familySegment);
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).Count()
            : 0;
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

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal034" => 34,
            "Goal035" => 35,
            "Goal036" => 36,
            "Goal037" => 37,
            "Goal038" => 38,
            "Goal039" => 39,
            "Goal040" => 40,
            "Goal043" => 43,
            "Goal047" => 47,
            "Goal053" => 53,
            "Goal054" => 54,
            "Goal055" => 55,
            "Goal056" => 56,
            "Goal057" => 57,
            _ => 999
        };

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

    private static IReadOnlyList<FullMediaBoundCampaignDiagnostic> SortDiagnostics(IEnumerable<FullMediaBoundCampaignDiagnostic> diagnostics) =>
        FullMediaBoundGeneratorCampaignBuilder.SortDiagnostics(diagnostics);

    private static string Hash(string text) => FullMediaBoundGeneratorCampaignHash.Hash(text);

    private static FullMediaBoundCampaignDiagnostic Error(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Error(code, target, message);
}
