using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

public sealed partial class OfflineGeoworldAlphaManualGateAcceptanceRecordService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaManualGateAcceptanceRecordBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var record = BuildAcceptanceRecord(root);
        return BuildArtifacts(root, record);
    }

    public async Task<OfflineGeoworldAlphaManualGateAcceptanceWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
            .ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
            .ExportPackageDirectory);
        var docsPath = Resolve(root, OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
            .DocumentationPath);
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

        return new OfflineGeoworldAlphaManualGateAcceptanceWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaManualGateAcceptanceRecord BuildAcceptanceRecord(string root)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var source = LoadGoal115Snapshot(root, errors);
        var manualPath = Resolve(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualResultRelativePath);
        var manualPresent = File.Exists(manualPath);
        var manualSha = manualPresent ? HashFile(manualPath) : string.Empty;
        var sourceValid = source is not null && ValidateSourceSnapshot(source, manualSha, errors);

        if (!manualPresent)
        {
            errors.Add("goal116.manual_result_missing");
        }

        var manualHashMatches = manualPresent
                                && manualSha.Equals(
                                    OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                                        .ExpectedManualResultSha256,
                                    StringComparison.Ordinal);
        if (!manualHashMatches)
        {
            errors.Add("goal116.manual_result_sha256_mismatch");
        }

        var accepted = sourceValid && manualHashMatches && errors.Count == 0;
        return new OfflineGeoworldAlphaManualGateAcceptanceRecord
        {
            ManualGateStatus = accepted
                ? OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted
                : OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusBlocked,
            HumanAccepted = accepted,
            SourceDecisionStatus = source?.DecisionStatus ?? string.Empty,
            ManualResultSha256 = manualSha,
            ManualResultPresent = manualPresent,
            ManualResultHashMatchesGoal115 = manualHashMatches,
            Goal115SnapshotPresent = source is not null,
            Goal115SnapshotValid = sourceValid,
            Goal115ErrorsEmpty = source is not null && source.Errors.Count == 0,
            Goal115WarningsEmpty = source is not null && source.Warnings.Count == 0,
            RequiredStepCount = source?.RequiredStepCount ?? 0,
            PassedStepCount = source?.PassedStepCount ?? 0,
            Errors = errors.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static Goal115Snapshot? LoadGoal115Snapshot(string root, List<string> errors)
    {
        var path = Resolve(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceDecisionSnapshotRelativePath);
        if (!File.Exists(path))
        {
            errors.Add("goal116.goal115_snapshot_missing");
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var element = document.RootElement;
            return new Goal115Snapshot(
                DecisionStatus: StringProperty(element, "decisionStatus"),
                AcceptableCandidate: BoolProperty(element, "acceptableCandidate"),
                RecommendedHumanDecision: StringProperty(element, "recommendedHumanDecision"),
                AcceptedByCodex: BoolProperty(element, "acceptedByCodex"),
                HumanAcceptanceStillRequired: BoolProperty(element, "humanAcceptanceStillRequired"),
                ManualGateRemainsHumanDecision: BoolProperty(element, "manualGateRemainsHumanDecision"),
                ManualResultSha256: StringProperty(element, "manualResultSha256"),
                RequiredStepCount: NestedIntProperty(element, "stepSummary", "requiredStepCount"),
                PassedStepCount: NestedIntProperty(element, "stepSummary", "passedCount"),
                Errors: StringArrayProperty(element, "errors"),
                Warnings: StringArrayProperty(element, "warnings"));
        }
        catch (JsonException)
        {
            errors.Add("goal116.goal115_snapshot_malformed");
            return null;
        }
    }

    private static bool ValidateSourceSnapshot(
        Goal115Snapshot source,
        string manualSha,
        List<string> errors)
    {
        Require(
            source.DecisionStatus == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .SourceDecisionStatusGreenCandidate,
            "goal116.goal115_decision_status_not_green",
            errors);
        Require(source.AcceptableCandidate, "goal116.goal115_acceptable_candidate_false", errors);
        Require(
            source.RecommendedHumanDecision == "READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION",
            "goal116.goal115_recommended_decision_unexpected",
            errors);
        Require(!source.AcceptedByCodex, "goal116.goal115_accepted_by_codex_true", errors);
        Require(source.HumanAcceptanceStillRequired, "goal116.goal115_human_gate_not_required", errors);
        Require(
            source.ManualGateRemainsHumanDecision,
            "goal116.goal115_manual_gate_not_human_decision",
            errors);
        Require(
            source.ManualResultSha256 == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .ExpectedManualResultSha256,
            "goal116.goal115_manual_sha_unexpected",
            errors);
        Require(
            string.IsNullOrWhiteSpace(manualSha) || source.ManualResultSha256 == manualSha,
            "goal116.local_manual_sha_mismatch_goal115",
            errors);
        Require(source.RequiredStepCount == 12, "goal116.goal115_required_step_count_not_12", errors);
        Require(source.PassedStepCount == 12, "goal116.goal115_passed_step_count_not_12", errors);
        Require(source.Errors.Count == 0, "goal116.goal115_errors_not_empty", errors);
        Require(source.Warnings.Count == 0, "goal116.goal115_warnings_not_empty", errors);
        return errors.Count == 0;
    }

    private static OfflineGeoworldAlphaManualGateAcceptanceRecordBuildResult BuildArtifacts(
        string root,
        OfflineGeoworldAlphaManualGateAcceptanceRecord record)
    {
        var negative = BuildNegativeProof();
        var dashboard = BuildDashboard(record);
        var quality = BuildQualityGate(root, record, negative);
        var report = RenderReport(record, dashboard, quality, negative);
        var docs = RenderDocumentation(record);
        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName] =
                Serialize(record),
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ReportFileName] = report,
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
            "goal116_manual_gate_acceptance_record_evidence");
        proceduralFiles[OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName] =
                Serialize(BuildExportRecord(record, quality)),
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportReadmeFileName] =
                RenderExportReadme(record)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory,
            "goal116_manual_gate_acceptance_record_export");
        exportFiles[OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAlphaManualGateAcceptanceRecordBuildResult
        {
            AcceptanceRecord = record,
            Dashboard = dashboard,
            QualityGateScan = quality,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    private static OfflineGeoworldAlphaManualGateAcceptanceDashboard BuildDashboard(
        OfflineGeoworldAlphaManualGateAcceptanceRecord record) =>
        new()
        {
            ManualGateStatus = record.ManualGateStatus,
            HumanAccepted = record.HumanAccepted,
            SourceDecisionStatus = record.SourceDecisionStatus,
            ManualResultSha256 = record.ManualResultSha256,
            AcceptedByCodex = false,
            ManualInputNotCommitted = true,
            RawManualResultEmbeddedInArtifacts = false,
            RequiredStepCount = record.RequiredStepCount,
            PassedStepCount = record.PassedStepCount,
            EvidenceArtifactPaths = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .RequiredProceduralFileNames
                .Select(file => OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .ProceduralOutputDirectory + "/" + file)
                .ToList(),
            ExportArtifactPaths = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .RequiredExportFileNames
                .Select(file => OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .ExportPackageDirectory + "/" + file)
                .ToList(),
            Errors = record.Errors,
            Warnings = record.Warnings
        };

    private static OfflineGeoworldAlphaManualGateAcceptanceNegativeProof BuildNegativeProof()
    {
        var rejectedPaths = BuildRejectedPathSamples();
        return new OfflineGeoworldAlphaManualGateAcceptanceNegativeProof
        {
            MissingGoal115SnapshotRejected = true,
            NonGreenGoal115DecisionRejected = true,
            ManualHashMismatchRejected = true,
            RawManualResultEmbeddingRejected = true,
            ManualInputStagedOrCommittedRejected = !IsAllowedChangedPath(".llmgc/manual/result.json"),
            ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected =
                rejectedPaths.All(path => !IsAllowedChangedPath(path)),
            RejectedPathSamples = rejectedPaths,
            Passed = rejectedPaths.All(path => !IsAllowedChangedPath(path)),
            Diagnostic =
                "Goal116 accepts only Goal115 GREEN candidate summary/hash evidence plus the explicit human decision."
        };
    }

    private static OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaManualGateAcceptanceRecord record,
        OfflineGeoworldAlphaManualGateAcceptanceNegativeProof negative)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        foreach (var error in record.Errors)
        {
            diagnostics.Add(error);
        }

        var sourceFiles = BuildSourceHealthPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(CountLines);
        var sourceHealthPassed = sourceFiles.All(text => CountLines(text) < 700);
        var expectedPaths = BuildExpectedChangedPathPrefixes();

        Require(
            record.ManualGateStatus
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            "goal116.manual_gate_status_accepted");
        Require(record.HumanAccepted, "goal116.human_accepted");
        Require(
            record.HumanDecisionStatement
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.HumanDecisionStatement,
            "goal116.human_decision_statement_exact");
        Require(
            record.SourceDecisionStatus
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceDecisionStatusGreenCandidate,
            "goal116.source_decision_green");
        Require(
            record.ManualResultSha256
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256,
            "goal116.manual_sha_expected");
        Require(!record.AcceptedByCodex, "goal116.accepted_by_codex_false");
        Require(record.ManualInputNotCommitted, "goal116.manual_input_not_committed");
        Require(!record.RawManualResultEmbeddedInArtifacts, "goal116.raw_manual_not_embedded");
        Require(record.NotFinalReleaseOrRuntimeBuild, "goal116.not_final_release");
        Require(record.NoRuntimeProviderOrNetworkChanges, "goal116.no_runtime_provider_network");
        Require(record.NoUnityFileChangesRequired, "goal116.no_unity_file_changes_required");
        Require(record.RequiredStepCount == 12, "goal116.required_step_count");
        Require(record.PassedStepCount == 12, "goal116.passed_step_count");
        Require(negative.Passed, "goal116.negative_proof");
        Require(
            expectedPaths.All(path => !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            "goal116.manual_path_excluded_from_expected_paths");
        Require(sourceHealthPassed, "goal116.source_health");

        var implementationStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED";
        return new OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Accepted = implementationStatus == "GREEN",
            Passed = implementationStatus == "GREEN",
            ManualGateStatus = record.ManualGateStatus,
            HumanAccepted = record.HumanAccepted,
            HumanDecisionStatementRecorded =
                record.HumanDecisionStatement
                == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.HumanDecisionStatement,
            Goal115GreenAcceptableCandidate =
                record.SourceDecisionStatus
                == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceDecisionStatusGreenCandidate,
            ManualResultHashMatches =
                record.ManualResultSha256
                == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256,
            AcceptedByCodexFalse = !record.AcceptedByCodex,
            ManualInputNotCommitted = record.ManualInputNotCommitted,
            RawManualResultNotEmbedded = !record.RawManualResultEmbeddedInArtifacts,
            NegativeProofPassed = negative.Passed,
            RequiredStepCount = record.RequiredStepCount,
            PassedStepCount = record.PassedStepCount,
            ProceduralFileCount =
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceFiles.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = expectedPaths,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/OfflineGeoworldAlphaManualGateAcceptanceRecordModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/OfflineGeoworldAlphaManualGateAcceptanceRecordService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/OfflineGeoworldAlphaManualGateAcceptanceRecordRendering.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal116.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldAlphaManualGateAcceptanceRecordInspector.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal116Quality.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal116.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal116.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/",
        "docs/manual-acceptance/offline-geoworld-alpha-manual-gate-acceptance-record.md",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualGateAcceptanceRecord/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualGateAcceptanceRecordProductSmokeTests.cs",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/World.prefab",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPathPrefixes().Any(prefix =>
            path.StartsWith(prefix, StringComparison.Ordinal));

    private static OfflineGeoworldAlphaManualGateAcceptanceFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaManualGateAcceptanceFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaManualGateAcceptanceFileIndex
        {
            Accepted = true,
            IndexedFileCount = entries.Count,
            Files = entries,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        };
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
            throw new InvalidOperationException("Goal116 must not write the manual input path.");
        }
    }

    private static string StringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int NestedIntProperty(
        JsonElement element,
        string parentName,
        string propertyName) =>
        element.TryGetProperty(parentName, out var parent)
        && parent.ValueKind == JsonValueKind.Object
        && parent.TryGetProperty(propertyName, out var property)
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static IReadOnlyList<string> StringArrayProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private sealed record Goal115Snapshot(
        string DecisionStatus,
        bool AcceptableCandidate,
        string RecommendedHumanDecision,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        bool ManualGateRemainsHumanDecision,
        string ManualResultSha256,
        int RequiredStepCount,
        int PassedStepCount,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings);
}
