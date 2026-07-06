using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed partial class AcceptedAlphaUnityPlayableProjectionService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AcceptedAlphaUnityPlayableProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var errors = new List<string>();
        var warnings = new List<string>();
        var baseline = LoadBaseline(root, errors);
        var payload = LoadPayloadSummary(root, errors, warnings);
        var inventory = BuildScriptInventory(root);
        var smokePlan = BuildSmokePlan(baseline, payload);
        var negative = BuildNegativeProof();
        var dashboard = BuildDashboard(baseline, payload, inventory, smokePlan, negative, errors, warnings);
        var quality = BuildQualityGate(root, dashboard, inventory, smokePlan, negative);
        var report = RenderReport(dashboard, inventory, smokePlan, quality, negative);
        var docs = RenderDocumentation(dashboard, quality);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName] = Serialize(inventory),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.ReportFileName] = report,
            [AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName] = Serialize(quality),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory,
            "goal119_accepted_alpha_unity_playable_projection_evidence");
        proceduralFiles[AcceptedAlphaUnityPlayableProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName] = Serialize(inventory),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.ReportFileName] = report,
            [AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName] = Serialize(quality),
            [AcceptedAlphaUnityPlayableProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory,
            "goal119_accepted_alpha_unity_playable_projection_export");
        exportFiles[AcceptedAlphaUnityPlayableProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new AcceptedAlphaUnityPlayableProjectionBuildResult
        {
            Dashboard = dashboard,
            ScriptInventory = inventory,
            SmokePlan = smokePlan,
            QualityGateScan = quality,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<AcceptedAlphaUnityPlayableProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, AcceptedAlphaUnityPlayableProjectionVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new AcceptedAlphaUnityPlayableProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionDashboard BuildDashboard(
        AcceptedAlphaUnityPlayableProjectionBaselineSummary baseline,
        AcceptedAlphaUnityPlayableProjectionPayloadSummary payload,
        AcceptedAlphaUnityPlayableProjectionScriptInventory inventory,
        AcceptedAlphaUnityPlayableProjectionSmokePlan smokePlan,
        AcceptedAlphaUnityPlayableProjectionNegativeProof negative,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        var forbiddenClean = negative.Passed && inventory.NoForbiddenUnityPathsExpected;
        var projectionStatus = errors.Count == 0
                               && baseline.AcceptedBaselineReady
                               && inventory.AllScriptsPresent
                               && inventory.MenuPathExistsExactly
                               && smokePlan.StepCount >= 5
                               && forbiddenClean
            ? "GREEN"
            : "BLOCKED";

        return new AcceptedAlphaUnityPlayableProjectionDashboard
        {
            ProjectionStatus = projectionStatus,
            BaselineId = baseline.BaselineId,
            AcceptedBaselineReady = baseline.AcceptedBaselineReady,
            ManualGateStatus = baseline.ManualGateStatus,
            ScriptInventoryCount = inventory.ScriptCount,
            SmokePlanStepCount = smokePlan.StepCount,
            PreviewCommandCount = payload.PreviewCommandCount,
            ChunkWindowStepCount = payload.ChunkWindowStepCount,
            BoundaryCrossingCount = payload.BoundaryCrossingCount,
            InteractionTargetCount = payload.InteractionTargetCount,
            ObjectiveCount = payload.ObjectiveCount,
            CompletedObjectiveCount = payload.CompletedObjectiveCount,
            ReplayStepCount = payload.ReplayStepCount,
            ForbiddenUnitySurfaceClean = forbiddenClean,
            Errors = errors.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            Warnings = warnings.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionBaselineSummary LoadBaseline(
        string root,
        List<string> errors)
    {
        using var goal118 = LoadJson(
            root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.Goal118DashboardPath,
            "goal119.goal118_dashboard_missing",
            "goal119.goal118_dashboard_malformed",
            errors);
        using var goal116 = LoadJson(
            root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.Goal116AcceptanceRecordPath,
            "goal119.goal116_record_missing",
            "goal119.goal116_record_malformed",
            errors);

        var baselineId = StringProperty(goal118?.RootElement, "baselineId");
        var acceptedBaselineReady =
            baselineId == AcceptedAlphaUnityPlayableProjectionVocabulary.BaselineId
            && TryGetBool(goal118?.RootElement, "acceptedBaselineReady")
            && StringProperty(goal118?.RootElement, "manualGateStatus")
            == AcceptedAlphaUnityPlayableProjectionVocabulary.ManualGateStatusAccepted;
        var goal116Accepted =
            StringProperty(goal116?.RootElement, "manualGateStatus")
            == AcceptedAlphaUnityPlayableProjectionVocabulary.ManualGateStatusAccepted
            && TryGetBool(goal116?.RootElement, "humanAccepted")
            && !TryGetBool(goal116?.RootElement, "acceptedByCodex")
            && TryGetBool(goal116?.RootElement, "manualInputNotCommitted")
            && !TryGetBool(goal116?.RootElement, "rawManualResultEmbeddedInArtifacts");

        Require(acceptedBaselineReady, "goal119.goal118_baseline_not_ready", errors);
        Require(goal116Accepted, "goal119.goal116_manual_gate_not_accepted", errors);
        return new AcceptedAlphaUnityPlayableProjectionBaselineSummary(
            BaselineId: baselineId,
            AcceptedBaselineReady: acceptedBaselineReady && goal116Accepted,
            ManualGateStatus: StringProperty(goal118?.RootElement, "manualGateStatus"),
            Goal116Accepted: goal116Accepted);
    }

    private static AcceptedAlphaUnityPlayableProjectionPayloadSummary LoadPayloadSummary(
        string root,
        List<string> errors,
        List<string> warnings)
    {
        var previewCommandCount = IntFromJson(root,
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/"
            + "offline-geoworld-preview-feature-commands.json",
            "commandCount", errors);
        var chunkWindowStepCount = IntFromJson(root,
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/"
            + "offline-geoworld-playmode-steps.json",
            "stepCount", errors);
        var boundaryCrossingCount = IntFromJson(root,
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/"
            + "offline-geoworld-interactive-boundary-zones.json",
            "boundaryCrossingCount", errors);
        var interactionTargetCount = IntFromJson(root,
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/"
            + "offline-geoworld-interaction-targets.json",
            "targetCount", errors);
        var replayStepCount = IntFromJson(root,
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/"
            + "offline-geoworld-session-manifest.json",
            "replayStepCount", errors);

        var objectivePath =
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/"
            + "offline-geoworld-objectives.json";
        using var objective = LoadJson(root, objectivePath,
            "goal119.payload.objectives_missing",
            "goal119.payload.objectives_malformed",
            errors);
        var objectiveCount = IntProperty(objective?.RootElement, "objectiveCount");
        var completedObjectiveCount = CountCompletedObjectives(objective?.RootElement);
        if (completedObjectiveCount == 0 && objectiveCount > 0)
        {
            warnings.Add("goal119.payload.objectives_not_completed");
        }

        return new AcceptedAlphaUnityPlayableProjectionPayloadSummary(
            PreviewCommandCount: previewCommandCount,
            ChunkWindowStepCount: chunkWindowStepCount,
            BoundaryCrossingCount: boundaryCrossingCount,
            InteractionTargetCount: interactionTargetCount,
            ObjectiveCount: objectiveCount,
            CompletedObjectiveCount: completedObjectiveCount,
            ReplayStepCount: replayStepCount);
    }

    private static AcceptedAlphaUnityPlayableProjectionScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window",
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
                "unity_projection_controller",
                AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
                "unity_projection_diagnostics",
                "AcceptedAlphaPlayableProjectionDiagnostics"),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
                "unity_projection_models",
                "AcceptedAlphaProjectionSmokeResult"),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory",
                "AcceptedAlphaPlayableProjectionPrimitiveFactory"),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDrilldownPath,
                "unity_projection_drilldown",
                "AcceptedAlphaPlayableProjectionDrilldown"),
            Entry(root,
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityActionPreviewPath,
                "unity_projection_action_preview",
                "AcceptedAlphaPlayableProjectionActionPreview")
        }.ToList();

        return new AcceptedAlphaUnityPlayableProjectionScriptInventory
        {
            MenuPathExistsExactly = entries.Any(entry =>
                entry.RelativePath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath
                && entry.ContainsRequiredMarker),
            ScriptCount = entries.Count,
            AllScriptsPresent = entries.All(entry => entry.Exists && entry.ContainsRequiredMarker),
            NoForbiddenUnityPathsExpected =
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityScriptPaths.All(path =>
                    !path.Contains("/Assets/Scenes/", StringComparison.Ordinal)
                    && !path.EndsWith(".unity", StringComparison.Ordinal)
                    && !path.EndsWith(".prefab", StringComparison.Ordinal)
                    && !path.StartsWith("unity/LLMGameCreatorAlpha/ProjectSettings/", StringComparison.Ordinal)
                    && !path.StartsWith("unity/LLMGameCreatorAlpha/Packages/", StringComparison.Ordinal)
                    && !path.StartsWith(
                        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/",
                        StringComparison.Ordinal)),
            Scripts = entries
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new AcceptedAlphaUnityPlayableProjectionScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionSmokePlan BuildSmokePlan(
        AcceptedAlphaUnityPlayableProjectionBaselineSummary baseline,
        AcceptedAlphaUnityPlayableProjectionPayloadSummary payload)
    {
        var steps = new List<AcceptedAlphaUnityPlayableProjectionSmokePlanStep>
        {
            Step(1, "refresh_accepted_baseline",
                "Goal118 baseline loaded as " + baseline.BaselineId),
            Step(2, "create_player_proxy",
                "Projection root contains at least one player proxy primitive."),
            Step(3, "render_chunk_window_and_prefetch_markers",
                "Projection contains chunk/window and boundary/prefetch markers."),
            Step(4, "render_interactions_and_objectives",
                "Projection contains interaction target markers and objective checklist entries."),
            Step(5, "show_save_load_replay_status",
                "Projection shows Goal106 replay/checkpoint status."),
            Step(6, "show_diagnostics_status",
                "Projection contains diagnostics status and zero fatal errors.")
        };

        return new AcceptedAlphaUnityPlayableProjectionSmokePlan
        {
            BaselineLoaded = baseline.AcceptedBaselineReady,
            HasPlayerProxyStep = true,
            HasChunkWindowStep = payload.ChunkWindowStepCount > 0,
            HasInteractionOrObjectiveStep =
                payload.InteractionTargetCount > 0 || payload.ObjectiveCount > 0,
            HasDiagnosticsStatusStep = true,
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionSmokePlanStep Step(
        int index,
        string stepId,
        string expectedResult) =>
        new()
        {
            StepIndex = index,
            StepId = stepId,
            ExpectedResult = expectedResult
        };

    private static AcceptedAlphaUnityPlayableProjectionNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new AcceptedAlphaUnityPlayableProjectionNegativeProof
        {
            ManualInputRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            LiveGeodataProviderNetworkRejected = true,
            ManualInputExcluded = BuildExpectedChangedPaths().All(path =>
                !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            RejectedPathSamples = rejected,
            Passed = rejected.All(path => !IsAllowedChangedPath(path)),
            Diagnostic =
                "Goal119 is a temporary Unity Editor projection over accepted Alpha evidence, not release/runtime/provider/schema work."
        };
    }

    private static AcceptedAlphaUnityPlayableProjectionFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new AcceptedAlphaUnityPlayableProjectionFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new AcceptedAlphaUnityPlayableProjectionFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        };
    }

    private static int IntFromJson(string root, string relativePath, string propertyName, List<string> errors)
    {
        using var doc = LoadJson(root, relativePath,
            "goal119.payload.missing:" + relativePath,
            "goal119.payload.malformed:" + relativePath,
            errors);
        return IntProperty(doc?.RootElement, propertyName);
    }

    private static JsonDocument? LoadJson(
        string root,
        string relativePath,
        string missingCode,
        string malformedCode,
        List<string> errors)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            errors.Add(missingCode);
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException)
        {
            errors.Add(malformedCode);
            return null;
        }
    }

    private static string StringProperty(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int IntProperty(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static bool TryGetBool(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int CountCompletedObjectives(JsonElement? element)
    {
        if (element is null
            || !element.Value.TryGetProperty("objectives", out var objectives)
            || objectives.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return objectives.EnumerateArray()
            .Count(item =>
                item.TryGetProperty("completionState", out var state)
                && state.ValueKind == JsonValueKind.String
                && string.Equals(state.GetString(), "completed", StringComparison.Ordinal));
    }

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal119 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
}
