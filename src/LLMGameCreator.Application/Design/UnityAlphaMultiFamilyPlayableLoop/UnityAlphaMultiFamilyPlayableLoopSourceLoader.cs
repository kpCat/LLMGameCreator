using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyPlayableLoopSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public UnityAlphaMultiFamilySourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        var refs = new List<UnityAlphaMultiFamilySourceArtifactReference>();

        string ReadRequired(string relativePath, string sourceGoal, string artifactFamily)
        {
            var normalized = NormalizeRelativePath(relativePath);
            var path = Path.GetFullPath(Path.Combine(projectRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 057 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            refs.Add(new UnityAlphaMultiFamilySourceArtifactReference
            {
                SourceGoal = sourceGoal,
                ArtifactFamily = artifactFamily,
                ArtifactRelativePath = normalized,
                ArtifactHash = Hash(text),
                Exists = true,
                HashMatches = true
            });
            return text;
        }

        T ReadJson<T>(string relativePath, string sourceGoal, string artifactFamily) =>
            JsonSerializer.Deserialize<T>(ReadRequired(relativePath, sourceGoal, artifactFamily), JsonOptions)
            ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

        JsonDocument ReadDocument(string relativePath, string sourceGoal, string artifactFamily) =>
            JsonDocument.Parse(ReadRequired(relativePath, sourceGoal, artifactFamily));

        var goal056Root = UnityAlphaMediaBoundPlayablePackageVocabulary.RelativeOutputDirectory;
        var goal056SourceManifest = ReadJson<UnityAlphaMediaBoundSourceManifest>(
            goal056Root + "/" + UnityAlphaMediaBoundPlayablePackageEvidenceService.SourceManifestJsonFileName,
            "Goal056",
            "source_manifest");
        var goal056StagingManifest = ReadJson<UnityAlphaMediaBoundStagingManifest>(
            goal056Root + "/" + UnityAlphaMediaBoundPlayablePackageEvidenceService.StagingManifestJsonFileName,
            "Goal056",
            "unity_staging_manifest");
        var goal056LoadProof = ReadJson<UnityAlphaMediaBoundLoadProof>(
            goal056Root + "/" + UnityAlphaMediaBoundPlayablePackageEvidenceService.UnityLoadProofJsonFileName,
            "Goal056",
            "unity_media_load_proof");
        var goal056SmokeSummary = ReadJson<UnityAlphaMediaBoundSmokeLogSummary>(
            goal056Root + "/" + UnityAlphaMediaBoundPlayablePackageEvidenceService.SmokeLogSummaryJsonFileName,
            "Goal056",
            "unity_smoke_log_summary");
        var goal056Report = ReadRequired(
            goal056Root + "/" + UnityAlphaMediaBoundPlayablePackageEvidenceService.ReportMarkdownFileName,
            "Goal056",
            "report");

        _ = ReadRequired(
            ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/media-bound-review-package-manifest.json",
            "Goal055",
            "review_package_manifest");

        var families = new List<UnityAlphaMultiFamilySourceFamily>();
        foreach (var familyId in UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds.OrderBy(FamilyOrderingKey, StringComparer.Ordinal))
        {
            var segment = familyId.Replace('_', '-');
            using var plan = ReadDocument(
                ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/family-loop-plan-" + segment + ".json",
                "Goal043",
                "family_loop_plan");
            using var proof = ReadDocument(
                ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/family-simulatable-loop-proof-" + segment + ".json",
                "Goal043",
                "family_simulatable_loop_proof");
            using var dryRun = ReadDocument(
                ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-" + segment + "-dry-run.json",
                "Goal047",
                "family_dry_run");

            families.Add(ReadFamily(familyId, segment, plan.RootElement, proof.RootElement, dryRun.RootElement));
        }

        var stagingRoot = Path.Combine(projectRoot, goal056Root.Replace('/', Path.DirectorySeparatorChar), UnityAlphaMediaBoundPlayablePackageVocabulary.StagingRoot);
        var stagingFiles = new List<UnityAlphaMultiFamilyFilePayload>();
        if (!Directory.Exists(stagingRoot))
        {
            diagnostics.Add(Error("goal057.source.goal056_staging_missing", goal056Root + "/staging", "Goal 056 staging folder is required for Goal 057 Unity staging."));
        }
        else
        {
            foreach (var file in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var relative = Path.GetRelativePath(stagingRoot, file).Replace('\\', '/');
                if (!IsSafeRelativePath(relative))
                {
                    diagnostics.Add(Error("goal057.source.goal056_staging_path_unsafe", relative, "Goal 056 staging file paths must be safe relative paths."));
                    continue;
                }

                stagingFiles.Add(new UnityAlphaMultiFamilyFilePayload
                {
                    RelativePath = relative,
                    Bytes = File.ReadAllBytes(file)
                });
            }
        }

        foreach (var sourceRef in goal056SourceManifest.SourceArtifactRefs)
        {
            var sourcePath = Path.GetFullPath(Path.Combine(projectRoot, sourceRef.ArtifactRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(projectRoot, sourcePath);
            var exists = File.Exists(sourcePath);
            var hashMatches = exists && string.Equals(Hash(File.ReadAllText(sourcePath, Encoding.UTF8)), sourceRef.ArtifactHash, StringComparison.Ordinal);
            refs.Add(new UnityAlphaMultiFamilySourceArtifactReference
            {
                SourceGoal = sourceRef.SourceGoal,
                ArtifactFamily = sourceRef.ArtifactFamily,
                ArtifactRelativePath = sourceRef.ArtifactRelativePath,
                ArtifactHash = sourceRef.ArtifactHash,
                Exists = exists,
                HashMatches = hashMatches
            });
        }

        return new UnityAlphaMultiFamilySourceBundle
        {
            Goal056SourceManifest = goal056SourceManifest,
            Goal056StagingManifest = goal056StagingManifest,
            Goal056LoadProof = goal056LoadProof,
            Goal056SmokeLogSummary = goal056SmokeSummary,
            Goal056ReportMarkdown = goal056Report,
            Families = families.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ToList(),
            Goal056StagingFiles = stagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            SourceArtifactRefs = refs
                .GroupBy(item => item.SourceGoal + "|" + item.ArtifactRelativePath, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityAlphaMultiFamilySourceFamily ReadFamily(
        string familyId,
        string segment,
        JsonElement plan,
        JsonElement proof,
        JsonElement dryRun)
    {
        var scenarioId = GetString(plan, "scenarioId");
        var profileId = GetString(plan, "profileId");
        var commands = new List<UnityAlphaMultiFamilyLoopCommand>();
        foreach (var item in EnumerateArray(plan, "loopCommands"))
        {
            var order = GetInt(item, "order");
            commands.Add(new UnityAlphaMultiFamilyLoopCommand
            {
                FamilyId = familyId,
                ScenarioId = scenarioId,
                Order = order,
                CommandId = GetString(item, "commandId"),
                CommandType = GetString(item, "commandType"),
                TargetId = GetString(item, "targetId"),
                SecondaryTargetId = GetString(item, "secondaryTargetId"),
                Value = GetString(item, "value"),
                FamilyMarker = GetString(item, "familyMarker"),
                ExpectedStatus = GetString(item, "expectedStatus"),
                ExpectedPlayerMarker = BuildStepMarker(familyId, order, GetString(item, "commandType"), GetString(item, "familyMarker"), GetString(item, "expectedStatus"))
            });
        }

        return new UnityAlphaMultiFamilySourceFamily
        {
            FamilyId = familyId,
            ScenarioId = scenarioId,
            ProfileId = profileId,
            DeterministicOrderingKey = FamilyOrderingKey(familyId),
            Goal043PlanRelativePath = ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/family-loop-plan-" + segment + ".json",
            Goal043ProofRelativePath = ".llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/family-simulatable-loop-proof-" + segment + ".json",
            Goal047DryRunRelativePath = ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-" + segment + "-dry-run.json",
            RuntimePreviewPayloadRef = TryGetProperty(dryRun, "runtimePreviewPayloadSummary", out var runtimePreview)
                ? GetString(runtimePreview, "payloadRelativePath")
                : string.Empty,
            ExportMode = TryGetProperty(dryRun, "exportCandidatePayloadSummary", out var exportSummary)
                ? GetString(exportSummary, "exportMode")
                : string.Empty,
            StateChangingLoopProof = GetBool(dryRun, "stateChangingLoopProof"),
            FamilySpecificMinimumsPassed = GetBool(proof, "familySpecificMinimumsPassed"),
            SourceChangedMarkers = EnumerateStringArray(proof, "changedMarkers").Order(StringComparer.Ordinal).ToList(),
            LoopCommands = commands.OrderBy(item => item.Order).ToList()
        };
    }

    private static string BuildStepMarker(string familyId, int order, string commandType, string familyMarker, string expectedStatus) =>
        "family_loop_step=" + familyId + ":" + order + ":" + commandType + ":" + familyMarker + ":" + expectedStatus;

    private static IEnumerable<JsonElement> EnumerateArray(JsonElement element, string propertyName) =>
        TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
            : [];

    private static IEnumerable<string> EnumerateStringArray(JsonElement element, string propertyName) =>
        EnumerateArray(element, propertyName)
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item));

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out property))
        {
            return true;
        }

        property = default;
        return false;
    }

    private static string GetString(JsonElement element, string propertyName) =>
        TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int GetInt(JsonElement element, string propertyName) =>
        TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static bool GetBool(JsonElement element, string propertyName) =>
        TryGetProperty(element, propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False && property.GetBoolean();

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal043" => 43,
            "Goal047" => 47,
            "Goal055" => 55,
            "Goal056" => 56,
            _ => 999
        };

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static string NormalizeRelativePath(string relativePath) => relativePath.Replace('\\', '/').TrimStart('/');

    private static string Hash(string text) => UnityAlphaMultiFamilyPlayableLoopHash.Hash(text);

    private static UnityAlphaMultiFamilyDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMultiFamilyDiagnostic.Error(code, target, message);

    private static IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> SortDiagnostics(IEnumerable<UnityAlphaMultiFamilyDiagnostic> diagnostics) =>
        UnityAlphaMultiFamilyPlayableLoopBuilder.SortDiagnostics(diagnostics);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
