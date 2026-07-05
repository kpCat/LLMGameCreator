using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

public sealed partial class OfflineGeoworldAlphaAcceptanceOperatorPackService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaAcceptanceOperatorPackBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var goal110 = LoadGoal110Metadata(root);
        var goal111 = LoadGoal111Decision(root);
        var dashboard = BuildDashboard(goal110, goal111);
        return BuildArtifacts(root, goal110, goal111, dashboard);
    }

    public async Task<OfflineGeoworldAlphaAcceptanceOperatorPackWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
            .ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
            .ExportPackageDirectory);
        var docsPath = Resolve(root, OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
            .DocumentationRunbookPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        await WriteTextAsync(docsPath, result.DocumentationRunbookMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new OfflineGeoworldAlphaAcceptanceOperatorPackWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationRunbookPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaAcceptanceOperatorPackBuildResult BuildArtifacts(
        string root,
        Goal110Metadata goal110,
        Goal111DecisionEvidence goal111,
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard)
    {
        var pathMap = new OfflineGeoworldAlphaAcceptanceResultPathMap
        {
            CandidateManualResultPaths = dashboard.CandidateManualResultPaths
        };
        var notary = new OfflineGeoworldAlphaAcceptanceNotaryBoundary
        {
            ForbiddenStarts = BuildDoNotDoYet()
        };
        var pendingTemplate = BuildPendingTemplateCopy(goal110, dashboard);
        var runbook = RenderRunbook(dashboard, pathMap);
        var docsRunbook = RenderDocumentationRunbook(dashboard);
        var negativeProof = new OfflineGeoworldAlphaAcceptanceNegativeProofNoResultNoAcceptance
        {
            Passed = !dashboard.AcceptedByCodex
                     && dashboard.HumanAcceptanceStillRequired,
            ManualResultPresent = dashboard.ManualResultPresent,
            AcceptedByCodex = false,
            OperatorStatus = dashboard.OperatorStatus
        };
        var quality = BuildQualityGate(root, goal110, goal111, dashboard, pendingTemplate, runbook, negativeProof);
        var preflightReport = RenderPreflightReport(dashboard, quality);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName] = runbook,
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ResultPathMapFileName] =
                Serialize(pathMap),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreflightReportFileName] =
                preflightReport,
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NotaryBoundaryFileName] =
                Serialize(notary),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NegativeProofFileName] =
                Serialize(negativeProof),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PendingResultTemplateCopyFileName] =
                pendingTemplate
        };
        var proceduralIndex = BuildFileIndex(proceduralFiles, "goal112_operator_pack_evidence");
        proceduralFiles[OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName] =
                Serialize(BuildExportDashboard(dashboard, quality)),
            [OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportReadmeFileName] =
                RenderExportReadme(dashboard)
        };
        var exportIndex = BuildFileIndex(exportFiles, "goal112_operator_pack_export");
        exportFiles[OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAlphaAcceptanceOperatorPackBuildResult
        {
            Dashboard = dashboard,
            ResultPathMap = pathMap,
            NotaryBoundary = notary,
            QualityGateScan = quality,
            NegativeProof = negativeProof,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationRunbookMarkdown = docsRunbook
        };
    }

    private static OfflineGeoworldAlphaAcceptanceOperatorDashboard BuildDashboard(
        Goal110Metadata goal110,
        Goal111DecisionEvidence goal111)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        if (!goal110.PackagePresent)
        {
            errors.Add("Goal110 acceptance package metadata is missing or incomplete.");
        }

        if (!goal111.DecisionPresent)
        {
            errors.Add("Goal111 manual-result intake decision file is missing.");
        }
        else if (!goal111.DecisionValid)
        {
            errors.Add("Goal111 manual-result intake decision is invalid.");
        }

        if (!goal111.ManualResultPresent)
        {
            warnings.Add("No real manual result JSON is present in deterministic candidate paths.");
        }

        var operatorStatus = DeriveOperatorStatus(goal110, goal111);
        return new OfflineGeoworldAlphaAcceptanceOperatorDashboard
        {
            OperatorStatus = operatorStatus,
            DecisionStatusFromGoal111 = goal111.DecisionStatus,
            CandidateManualResultPaths = goal111.CandidateResultPaths.Count == 0
                ? OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths
                : goal111.CandidateResultPaths,
            ChecklistStepCount = goal110.ChecklistStepCount,
            ChecklistHash = goal110.ChecklistHash,
            ResultTemplateHash = goal110.ResultTemplateHash,
            ManualResultPresent = goal111.ManualResultPresent,
            ManualResultAvailableForHumanReview = operatorStatus
                == OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
                    .OperatorStatusGreenManualResultAvailable,
            AcceptedByCodex = false,
            HumanAcceptanceStillRequired = true,
            Errors = errors.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            NextHumanActions = BuildNextHumanActions(),
            DoNotDoYet = BuildDoNotDoYet()
        };
    }

    private static string DeriveOperatorStatus(Goal110Metadata goal110, Goal111DecisionEvidence goal111)
    {
        if (!goal110.PackagePresent)
        {
            return OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal110Missing;
        }

        if (!goal111.DecisionPresent)
        {
            return OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal111DecisionMissing;
        }

        if (!goal111.DecisionValid
            || string.Equals(goal111.DecisionStatus,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
                StringComparison.Ordinal))
        {
            return OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusGoal111Invalid;
        }

        if (string.Equals(goal111.DecisionStatus,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
                StringComparison.Ordinal))
        {
            return OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
                .OperatorStatusGreenManualResultAvailable;
        }

        return OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
            .OperatorStatusReadyPendingHumanRun;
    }

    private static IReadOnlyList<string> BuildNextHumanActions() =>
    [
        "Open unity/LLMGameCreatorAlpha in Unity.",
        "Open LLMGameCreator/Offline Geoworld Alpha Acceptance Runner.",
        "Run the Goal110 checklist manually and record every required step.",
        "Write the real result JSON to "
        + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath,
        "Re-run the Goal111 intake verifier and review the decision before deciding the gate."
    ];

    private static IReadOnlyList<string> BuildDoNotDoYet() =>
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

    private static OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan BuildQualityGate(
        string root,
        Goal110Metadata goal110,
        Goal111DecisionEvidence goal111,
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard,
        string pendingTemplate,
        string runbook,
        OfflineGeoworldAlphaAcceptanceNegativeProofNoResultNoAcceptance negativeProof)
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
        var pendingTemplateSafe = IsPendingTemplateSafe(pendingTemplate);
        var runbookScanPassed = RunbookBoundaryScanPassed(runbook);
        var implementationStatus = diagnostics.Count == 0
                                   && goal110.PackagePresent
                                   && goal111.DecisionPresent
                                   && goal111.DecisionValid
                                   && sourceHealthPassed
            ? "GREEN"
            : dashboard.OperatorStatus.StartsWith("BLOCKED_", StringComparison.Ordinal)
                ? "BLOCKED"
                : "FAILED";

        Require(goal110.PackagePresent, "goal112.goal110_package_present");
        Require(goal111.DecisionPresent, "goal112.goal111_decision_present");
        Require(goal111.DecisionValid, "goal112.goal111_decision_valid");
        Require(!string.IsNullOrWhiteSpace(goal110.ChecklistHash), "goal112.checklist_hash");
        Require(!string.IsNullOrWhiteSpace(goal110.ResultTemplateHash), "goal112.result_template_hash");
        Require(pendingTemplateSafe, "goal112.pending_template_copy_safe");
        Require(negativeProof.Passed, "goal112.negative_no_result_no_acceptance");
        Require(runbookScanPassed, "goal112.runbook_boundary_scan");
        Require(!dashboard.AcceptedByCodex, "goal112.accepted_by_codex_false");
        Require(dashboard.HumanAcceptanceStillRequired, "goal112.human_gate_still_required");
        Require(dashboard.NoUnityFileChangesRequired, "goal112.no_unity_file_changes_required");
        Require(dashboard.NoRuntimeProviderOrNetworkChanges, "goal112.no_runtime_provider_network");
        Require(dashboard.NotFinalReleaseOrRuntimeBuild, "goal112.not_final_release");
        Require(sourceHealthPassed, "goal112.source_health");

        implementationStatus = diagnostics.Count == 0 ? "GREEN" : implementationStatus;
        return new OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Passed = diagnostics.Count == 0,
            OperatorStatus = dashboard.OperatorStatus,
            DecisionStatusFromGoal111 = dashboard.DecisionStatusFromGoal111,
            Goal110PackagePresent = goal110.PackagePresent,
            Goal111DecisionPresent = goal111.DecisionPresent,
            ChecklistHashResolved = !string.IsNullOrWhiteSpace(goal110.ChecklistHash),
            ResultTemplateHashResolved = !string.IsNullOrWhiteSpace(goal110.ResultTemplateHash),
            PendingTemplateCopySafe = pendingTemplateSafe,
            NegativeNoResultNoAcceptancePassed = negativeProof.Passed,
            RunbookBoundaryScanPassed = runbookScanPassed,
            ChecklistStepCount = dashboard.ChecklistStepCount,
            ProceduralFileCount =
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceFiles.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = BuildExpectedChangedPathPrefixes(),
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/"
        + "OfflineGeoworldAlphaAcceptanceOperatorPackModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/"
        + "OfflineGeoworldAlphaAcceptanceOperatorPackService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/"
        + "OfflineGeoworldAlphaAcceptanceOperatorPackService.Read.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/"
        + "OfflineGeoworldAlphaAcceptanceOperatorPackService.Render.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewWorkspaceModels.Goal112.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewOfflineGeoworldAlphaAcceptanceOperatorPackInspector.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewWorkspacePageControl.Goal112.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-112-offline-geoworld-alpha-acceptance-operator-pack/",
        "docs/manual-acceptance/",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaAcceptanceOperatorPack/",
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

    private static bool IsPendingTemplateSafe(string text)
    {
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        return TryGetBool(root, "templateCopyOnly")
               && TryGetBool(root, "pendingOnly")
               && TryGetBool(root, "notRealHumanResult")
               && TryGetBool(root, "humanMustFillRealResultSeparately")
               && root.TryGetProperty("accepted", out var accepted)
               && accepted.ValueKind == JsonValueKind.False;
    }

    private static bool RunbookBoundaryScanPassed(string text)
    {
        var forbidden = new[]
        {
            "/mnt",
            "/home/oai",
            "sandbox:/",
            "Infection Free Zone"
        };
        return forbidden.All(item => !text.Contains(item, StringComparison.OrdinalIgnoreCase));
    }

    private static OfflineGeoworldAlphaAcceptanceOperatorFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaAcceptanceOperatorFileIndexEntry
            {
                RelativePath = item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaAcceptanceOperatorFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries
        };
    }

    private static object BuildExportDashboard(
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard,
        OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan quality) =>
        new
        {
            dashboard.GoalId,
            dashboard.SourceGoalIds,
            dashboard.ManualGate,
            dashboard.OperatorStatus,
            dashboard.DecisionStatusFromGoal111,
            dashboard.PreferredManualResultPath,
            dashboard.ManualResultPresent,
            dashboard.ManualResultAvailableForHumanReview,
            dashboard.AcceptedByCodex,
            dashboard.HumanAcceptanceStillRequired,
            qualityGatePassed = quality.Passed,
            dashboard.NotFinalReleaseOrRuntimeBuild,
            dashboard.NoRuntimeProviderOrNetworkChanges,
            dashboard.NoUnityFileChangesRequired
        };

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

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
}
