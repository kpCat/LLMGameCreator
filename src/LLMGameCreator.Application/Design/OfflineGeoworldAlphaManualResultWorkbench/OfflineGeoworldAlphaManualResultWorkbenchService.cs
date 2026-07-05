using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

public sealed partial class OfflineGeoworldAlphaManualResultWorkbenchService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaManualResultWorkbenchBuildResult Build(string repositoryRootPath) =>
        Build(repositoryRootPath, candidateResultRelativePaths: []);

    public OfflineGeoworldAlphaManualResultWorkbenchBuildResult Build(
        string repositoryRootPath,
        IReadOnlyList<string> candidateResultRelativePaths)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var source = LoadSource(root);
        var candidatePaths = BuildCandidatePaths(source, candidateResultRelativePaths);
        var validation = ValidateCandidates(root, source.Goal110, candidatePaths);
        var dashboard = BuildDashboard(source, candidatePaths, validation);
        return BuildArtifacts(root, source, dashboard);
    }

    public async Task<OfflineGeoworldAlphaManualResultWorkbenchWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, OfflineGeoworldAlphaManualResultWorkbenchVocabulary
            .ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaManualResultWorkbenchVocabulary
            .ExportPackageDirectory);
        var docsPath = Resolve(root, OfflineGeoworldAlphaManualResultWorkbenchVocabulary
            .DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualResultPath(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualResultPath(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualResultPath(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new OfflineGeoworldAlphaManualResultWorkbenchWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchBuildResult BuildArtifacts(
        string root,
        WorkbenchSource source,
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard)
    {
        var draft = BuildDraftTemplate(source.Goal110, dashboard);
        var fieldMap = BuildFieldMap(source.Goal110);
        var noResultProof = new OfflineGeoworldAlphaManualResultWorkbenchNegativeProof
        {
            ScenarioId = "missing_manual_result_does_not_accept_alpha",
            Passed = !dashboard.ManualResultPresent
                     && !dashboard.AcceptedByCodex
                     && dashboard.HumanAcceptanceStillRequired,
            ManualResultPresent = dashboard.ManualResultPresent,
            AcceptedByCodex = false,
            WorkbenchStatus = dashboard.WorkbenchStatus,
            Diagnostic = "Missing real manual result keeps the workbench pending and cannot accept Alpha."
        };
        var invalidProof = BuildInvalidResultProof(source.Goal110);
        var quality = BuildQualityGate(root, source, dashboard, noResultProof, invalidProof);
        var report = RenderReport(dashboard, quality);
        var runbook = RenderRunbook(dashboard);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ReportFileName] = report,
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RunbookFileName] = runbook,
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName] = draft,
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.FieldMapFileName] =
                Serialize(fieldMap),
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeNoResultFileName] =
                Serialize(noResultProof),
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeInvalidResultFileName] =
                Serialize(invalidProof)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory,
            "goal113_manual_result_workbench_evidence");
        proceduralFiles[OfflineGeoworldAlphaManualResultWorkbenchVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportDashboard = BuildExportDashboard(dashboard, quality);
        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName] =
                Serialize(exportDashboard),
            [OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportReadmeFileName] =
                RenderExportReadme(dashboard)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory,
            "goal113_manual_result_workbench_export");
        exportFiles[OfflineGeoworldAlphaManualResultWorkbenchVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAlphaManualResultWorkbenchBuildResult
        {
            Dashboard = dashboard,
            FieldMap = fieldMap,
            QualityGateScan = quality,
            NegativeNoResultProof = noResultProof,
            NegativeInvalidResultProof = invalidProof,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchDashboard BuildDashboard(
        WorkbenchSource source,
        IReadOnlyList<string> candidatePaths,
        OfflineGeoworldAlphaManualResultWorkbenchValidation validation)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        if (!source.Goal110.PackagePresent)
        {
            errors.Add("Goal110 acceptance package metadata is missing or incomplete.");
        }

        if (!source.Goal111.DecisionPresent || !source.Goal111.DecisionValid)
        {
            errors.Add("Goal111 manual-result intake decision is missing or invalid.");
        }

        if (!source.Goal112.ArtifactsPresent)
        {
            errors.Add("Goal112 operator dashboard/path-map/runbook artifacts are missing.");
        }

        if (!validation.ManualResultPresent)
        {
            warnings.Add("No real manual result JSON is present in deterministic candidate paths.");
        }

        var workbenchStatus = DeriveWorkbenchStatus(source, validation);
        errors.AddRange(validation.Errors);
        warnings.AddRange(validation.Warnings);

        return new OfflineGeoworldAlphaManualResultWorkbenchDashboard
        {
            WorkbenchStatus = workbenchStatus,
            Goal111DecisionStatus = source.Goal111.DecisionStatus,
            Goal112OperatorStatus = source.Goal112.OperatorStatus,
            ManualResultPresent = validation.ManualResultPresent,
            CandidateManualResultPaths = candidatePaths,
            RealManualResultPath = validation.ResultFilePath,
            ChecklistHash = source.Goal110.ChecklistHash,
            ChecklistStepCount = source.Goal110.RequiredSteps.Count,
            RequiredSteps = source.Goal110.RequiredSteps,
            Validation = validation with { ValidationStatus = workbenchStatus },
            SourceLineage = BuildSourceLineage(source),
            NextHumanActions = BuildNextHumanActions(),
            DoNotStartYet = BuildDoNotStartYet(),
            ProceduralArtifactPaths = OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                .RequiredProceduralFileNames
                .Select(file => OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                    .ProceduralOutputDirectory + "/" + file)
                .ToList(),
            ExportArtifactPaths = OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                .RequiredExportFileNames
                .Select(file => OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                    .ExportPackageDirectory + "/" + file)
                .ToList(),
            Errors = errors.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static string DeriveWorkbenchStatus(
        WorkbenchSource source,
        OfflineGeoworldAlphaManualResultWorkbenchValidation validation)
    {
        if (!source.Goal110.PackagePresent)
        {
            return OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal110;
        }

        if (!source.Goal111.DecisionPresent || !source.Goal111.DecisionValid)
        {
            return OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal111;
        }

        if (!source.Goal112.ArtifactsPresent)
        {
            return OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusMissingGoal112;
        }

        if (!validation.ManualResultPresent)
        {
            return OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                .WorkbenchStatusReadyPendingHumanResult;
        }

        return validation.ReadyForHumanReview
            ? OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultReadyForHumanReview
            : OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusResultInvalid;
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan BuildQualityGate(
        string root,
        WorkbenchSource source,
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard,
        OfflineGeoworldAlphaManualResultWorkbenchNegativeProof noResultProof,
        OfflineGeoworldAlphaManualResultWorkbenchNegativeProof invalidProof)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        var sourceFiles = BuildSourceHealthPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(CountLines);
        var sourceHealthPassed = sourceFiles.All(text => CountLines(text) < 700);
        var blocked = dashboard.WorkbenchStatus.StartsWith("WORKBENCH_BLOCKED_", StringComparison.Ordinal);

        Require(source.Goal110.PackagePresent, "goal113.goal110_package_present");
        Require(source.Goal111.DecisionPresent && source.Goal111.DecisionValid,
            "goal113.goal111_decision_valid");
        Require(source.Goal112.ArtifactsPresent, "goal113.goal112_artifacts_present");
        Require(!dashboard.AcceptedByCodex, "goal113.accepted_by_codex_false");
        Require(dashboard.HumanAcceptanceStillRequired, "goal113.human_gate_still_required");
        Require(dashboard.DoesNotWritePreferredManualResultPath,
            "goal113.does_not_write_preferred_manual_result_path");
        Require(dashboard.DraftTemplateOnly, "goal113.draft_template_only");
        Require(dashboard.NotFinalReleaseOrRuntimeBuild, "goal113.not_final_release");
        Require(dashboard.NoRuntimeProviderOrNetworkChanges, "goal113.no_runtime_provider_network");
        Require(dashboard.NoUnityFileChangesRequired, "goal113.no_unity_file_changes_required");
        Require(noResultProof.Passed || dashboard.ManualResultPresent,
            "goal113.negative_no_result_no_acceptance");
        Require(invalidProof.Passed, "goal113.negative_invalid_result");
        Require(sourceHealthPassed, "goal113.source_health");

        return new OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan
        {
            ImplementationStatus = blocked ? "BLOCKED" : diagnostics.Count == 0 ? "GREEN" : "FAILED",
            Passed = diagnostics.Count == 0,
            WorkbenchStatus = dashboard.WorkbenchStatus,
            ManualResultPresent = dashboard.ManualResultPresent,
            Goal110PackagePresent = source.Goal110.PackagePresent,
            Goal111DecisionPresent = source.Goal111.DecisionPresent,
            Goal112ArtifactsPresent = source.Goal112.ArtifactsPresent,
            NegativeNoResultNoAcceptancePassed = noResultProof.Passed || dashboard.ManualResultPresent,
            NegativeInvalidResultPassed = invalidProof.Passed,
            ChecklistStepCount = dashboard.ChecklistStepCount,
            ProceduralFileCount =
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceFiles.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = BuildExpectedChangedPathPrefixes(),
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/"
        + "OfflineGeoworldAlphaManualResultWorkbenchModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/"
        + "OfflineGeoworldAlphaManualResultWorkbenchService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/"
        + "OfflineGeoworldAlphaManualResultWorkbenchService.Read.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/"
        + "OfflineGeoworldAlphaManualResultWorkbenchService.Validation.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/"
        + "OfflineGeoworldAlphaManualResultWorkbenchService.Render.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewWorkspaceModels.Goal113.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewOfflineGeoworldAlphaManualResultWorkbenchInspector.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewWorkspacePageControl.Goal113.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-113-offline-geoworld-alpha-manual-result-workbench/",
        "docs/manual-acceptance/",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultWorkbench/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];

    private static IReadOnlyList<string> BuildNextHumanActions() =>
    [
        "Open unity/LLMGameCreatorAlpha in Unity.",
        "Open LLMGameCreator/Offline Geoworld Alpha Acceptance Runner.",
        "Use the Goal113 draft template only as a copy/edit starting point.",
        "Run every Goal110 checklist step manually and record real evidence.",
        "Write the real result JSON to "
        + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath,
        "Re-run Goal111, Goal112 and Goal113 validation before deciding the manual gate."
    ];

    private static IReadOnlyList<string> BuildDoNotStartYet() =>
    [
        "live geodata",
        "providers",
        "Runtime consumer",
        "public schema",
        "Lua",
        "generator-library",
        "final art",
        "atlas",
        "scene/prefab/project settings",
        "release packaging"
    ];

    private static OfflineGeoworldAlphaManualResultWorkbenchFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaManualResultWorkbenchFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaManualResultWorkbenchFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries
        };
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

    private static void GuardNotManualResultPath(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal113 must not write a real manual result path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
}
