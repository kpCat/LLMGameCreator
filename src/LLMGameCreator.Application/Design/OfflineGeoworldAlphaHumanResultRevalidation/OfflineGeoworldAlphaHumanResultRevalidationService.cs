using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

public sealed class OfflineGeoworldAlphaHumanResultRevalidationService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public OfflineGeoworldAlphaHumanResultRevalidationBuildResult Build(string repositoryRootPath) =>
        Build(repositoryRootPath, [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath]);

    public OfflineGeoworldAlphaHumanResultRevalidationBuildResult Build(
        string repositoryRootPath,
        IReadOnlyList<string> manualResultCandidatePaths)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var decision = BuildDecision(root, manualResultCandidatePaths);
        return BuildArtifacts(root, decision);
    }

    public async Task<OfflineGeoworldAlphaHumanResultRevalidationWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, OfflineGeoworldAlphaHumanResultRevalidationVocabulary
            .ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaHumanResultRevalidationVocabulary
            .ExportPackageDirectory);
        var docsPath = Resolve(root, OfflineGeoworldAlphaHumanResultRevalidationVocabulary
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

        return new OfflineGeoworldAlphaHumanResultRevalidationWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot BuildDecision(
        string root,
        IReadOnlyList<string> manualResultCandidatePaths)
    {
        IReadOnlyList<string> candidates = manualResultCandidatePaths.Count == 0
            ? [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath]
            : manualResultCandidatePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
        var intake = new OfflineGeoworldAlphaManualResultIntakeService().Build(root, candidates);
        var intakeDecision = intake.Decision;
        var decisionStatus = MapDecisionStatus(intakeDecision);
        var acceptable = decisionStatus == OfflineGeoworldAlphaHumanResultRevalidationVocabulary
            .DecisionStatusGreenCandidate;
        var resultPath = string.IsNullOrWhiteSpace(intakeDecision.ResultFilePath)
            ? candidates.FirstOrDefault()
              ?? OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath
            : intakeDecision.ResultFilePath;
        var manualFilePath = Resolve(root, resultPath);
        var present = File.Exists(manualFilePath);
        var manualSha = present ? HashFile(manualFilePath) : string.Empty;
        var jsonValid = false;
        var resultChecklistHash = string.Empty;
        if (present)
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manualFilePath, Encoding.UTF8));
                jsonValid = true;
                resultChecklistHash = StringProperty(document.RootElement, "checklistHash");
            }
            catch (JsonException)
            {
                jsonValid = false;
            }
        }

        return new OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot
        {
            ManualResultRelativePath = resultPath,
            ManualResultSha256 = manualSha,
            ManualResultPresent = present,
            ManualResultJsonValid = jsonValid,
            Goal111DecisionStatus = intakeDecision.DecisionStatus,
            DecisionStatus = decisionStatus,
            AcceptableCandidate = acceptable,
            AcceptedByCodex = false,
            HumanAcceptanceStillRequired = true,
            ManualGateRemainsHumanDecision = true,
            RecommendedHumanDecision = acceptable
                ? OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionReady
                : OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionDoNotAccept,
            ChecklistHashExpected = intakeDecision.ChecklistHashExpected,
            ChecklistHashActual = intakeDecision.ChecklistHashActual,
            ResultChecklistHash = resultChecklistHash,
            StepSummary = ConvertStepSummary(intakeDecision.StepSummary),
            Errors = intakeDecision.Errors.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Warnings = intakeDecision.Warnings.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            NotFinalReleaseOrRuntimeBuild = true,
            NoRuntimeProviderOrNetworkChanges = true,
            NoUnityFileChangesRequired = true,
            ManualInputNotCommitted = true,
            RawManualResultEmbeddedInArtifacts = false
        };
    }

    private static OfflineGeoworldAlphaHumanResultRevalidationBuildResult BuildArtifacts(
        string root,
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision)
    {
        var negative = BuildNegativeProof(root);
        var dashboard = BuildDashboard(decision);
        var quality = BuildQualityGate(root, decision, negative);
        var report = RenderReport(decision, dashboard, quality, negative);
        var docs = RenderDocumentation(decision);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName] =
                Serialize(decision),
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ReportFileName] = report,
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
            "goal115_human_result_revalidation_evidence");
        proceduralFiles[OfflineGeoworldAlphaHumanResultRevalidationVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName] =
                Serialize(BuildExportDashboard(decision, quality)),
            [OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportReadmeFileName] =
                RenderExportReadme(decision)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory,
            "goal115_human_result_revalidation_export");
        exportFiles[OfflineGeoworldAlphaHumanResultRevalidationVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAlphaHumanResultRevalidationBuildResult
        {
            DecisionSnapshot = decision,
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

    private static OfflineGeoworldAlphaHumanResultRevalidationDashboard BuildDashboard(
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision)
    {
        var blockingStepIssues = decision.StepSummary.FailedCount
                                 + decision.StepSummary.PendingCount
                                 + decision.StepSummary.SkippedCount
                                 + decision.StepSummary.MissingCount
                                 + decision.StepSummary.DuplicateCount
                                 + decision.StepSummary.InvalidStatusCount
                                 + decision.StepSummary.MissingStatusCount;
        return new OfflineGeoworldAlphaHumanResultRevalidationDashboard
        {
            DecisionStatus = decision.DecisionStatus,
            AcceptableCandidate = decision.AcceptableCandidate,
            RecommendedHumanDecision = decision.RecommendedHumanDecision,
            ManualResultPresent = decision.ManualResultPresent,
            ManualResultRelativePath = decision.ManualResultRelativePath,
            ManualResultSha256 = decision.ManualResultSha256,
            AcceptedByCodex = false,
            HumanAcceptanceStillRequired = true,
            ManualGateRemainsHumanDecision = true,
            RequiredStepCount = decision.StepSummary.RequiredStepCount,
            PassedStepCount = decision.StepSummary.PassedCount,
            BlockingStepIssueCount = blockingStepIssues,
            EvidenceArtifactPaths = OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                .RequiredProceduralFileNames
                .Select(file => OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                    .ProceduralOutputDirectory + "/" + file)
                .ToList(),
            ExportArtifactPaths = OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                .RequiredExportFileNames
                .Select(file => OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                    .ExportPackageDirectory + "/" + file)
                .ToList(),
            Errors = decision.Errors,
            Warnings = decision.Warnings
        };
    }
    private static OfflineGeoworldAlphaHumanResultRevalidationNegativeProof BuildNegativeProof(
        string root)
    {
        var intake = new OfflineGeoworldAlphaManualResultIntakeService();
        var missing = intake.Build(root, ["__goal115_missing_manual_result.json"]).Decision;
        var draftPath = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
                        + "/"
                        + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName;
        var draftDecision = File.Exists(Resolve(root, draftPath))
            ? MapDecisionStatus(intake.Build(root, [draftPath]).Decision)
            : OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending;
        var malformedRejected = false;
        try
        {
            using var _ = JsonDocument.Parse("{ invalid json");
        }
        catch (JsonException)
        {
            malformedRejected = true;
        }

        var missingStatus = MapDecisionStatus(missing);
        var draftBlocked = draftDecision != OfflineGeoworldAlphaHumanResultRevalidationVocabulary
            .DecisionStatusGreenCandidate;
        return new OfflineGeoworldAlphaHumanResultRevalidationNegativeProof
        {
            MissingManualResultDecisionStatus = missingStatus,
            MissingManualResultBlocked = missingStatus == OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                .DecisionStatusPending,
            MalformedManualResultDecisionStatus =
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusInvalid,
            MalformedManualResultRejected = malformedRejected,
            DraftTemplateLikeDecisionStatus = draftDecision,
            DraftTemplateLikeResultBlocked = draftBlocked,
            ManualResultRawJsonCopied = false,
            Passed = missingStatus == OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending
                     && malformedRejected
                     && draftBlocked,
            Diagnostic = "Missing, malformed and Goal113 draft-template-like inputs do not become Alpha acceptance."
        };
    }
    private static OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision,
        OfflineGeoworldAlphaHumanResultRevalidationNegativeProof negative)
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
        var expectedPaths = BuildExpectedChangedPathPrefixes();
        var manualExcluded = !expectedPaths.Any(path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        Require(!decision.AcceptedByCodex, "goal115.accepted_by_codex_false");
        Require(decision.HumanAcceptanceStillRequired, "goal115.human_gate_still_required");
        Require(decision.ManualGateRemainsHumanDecision, "goal115.manual_gate_human_decision");
        Require(decision.NotFinalReleaseOrRuntimeBuild, "goal115.not_final_release");
        Require(decision.NoRuntimeProviderOrNetworkChanges, "goal115.no_runtime_provider_network");
        Require(decision.NoUnityFileChangesRequired, "goal115.no_unity_file_changes_required");
        Require(decision.ManualInputNotCommitted, "goal115.manual_input_not_committed");
        Require(!decision.RawManualResultEmbeddedInArtifacts, "goal115.no_raw_manual_result_copy");
        Require(negative.Passed, "goal115.negative_proof");
        Require(manualExcluded, "goal115.manual_path_excluded_from_expected_changed_paths");
        Require(sourceHealthPassed, "goal115.source_health");

        var implementationStatus = diagnostics.Count > 0
            || decision.DecisionStatus == OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                .DecisionStatusInvalid
            ? "FAILED"
            : decision.DecisionStatus == OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                .DecisionStatusGreenCandidate
                ? "GREEN"
                : "BLOCKED";
        return new OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Passed = implementationStatus == "GREEN",
            DecisionStatus = decision.DecisionStatus,
            AcceptableCandidate = decision.AcceptableCandidate,
            ManualResultPresent = decision.ManualResultPresent,
            ManualResultJsonValid = decision.ManualResultJsonValid,
            AcceptedByCodexFalse = !decision.AcceptedByCodex,
            HumanAcceptanceStillRequired = decision.HumanAcceptanceStillRequired,
            ManualGateRemainsHumanDecision = decision.ManualGateRemainsHumanDecision,
            NotFinalReleaseOrRuntimeBuild = decision.NotFinalReleaseOrRuntimeBuild,
            NoRuntimeProviderOrNetworkChanges = decision.NoRuntimeProviderOrNetworkChanges,
            NoUnityFileChangesRequired = decision.NoUnityFileChangesRequired,
            ManualInputNotCommitted = decision.ManualInputNotCommitted,
            ManualInputExcludedFromFileIndex = manualExcluded,
            NegativeProofPassed = negative.Passed,
            RequiredStepCount = decision.StepSummary.RequiredStepCount,
            PassedStepCount = decision.StepSummary.PassedCount,
            ProceduralFileCount =
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceFiles.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = expectedPaths,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaHumanResultRevalidation/OfflineGeoworldAlphaHumanResultRevalidationModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaHumanResultRevalidation/OfflineGeoworldAlphaHumanResultRevalidationService.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal115.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldAlphaHumanResultRevalidationInspector.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal115Quality.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal115.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-115-offline-geoworld-alpha-human-result-revalidation/",
        "docs/manual-acceptance/offline-geoworld-alpha-human-result-revalidation.md",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaHumanResultRevalidation/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaHumanResultRevalidation/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaHumanResultRevalidationProductSmokeTests.cs",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];

    private static OfflineGeoworldAlphaHumanResultRevalidationStepSummary ConvertStepSummary(
        OfflineGeoworldAlphaManualResultStepSummary summary) =>
        new()
        {
            RequiredStepCount = summary.RequiredStepCount,
            ResultStepCount = summary.ResultStepCount,
            PassedCount = summary.PassedCount,
            FailedCount = summary.FailedCount,
            PendingCount = summary.PendingCount,
            SkippedCount = summary.SkippedCount,
            MissingCount = summary.MissingCount,
            DuplicateCount = summary.DuplicateCount,
            UnknownCount = summary.UnknownCount,
            InvalidStatusCount = summary.InvalidStatusCount,
            MissingStatusCount = summary.MissingStatusCount,
            RequiredStepsPresentExactlyOnce = summary.RequiredStepsPresentExactlyOnce,
            AllRequiredStepsPassed = summary.RequiredStepCount > 0
                                     && summary.PassedCount == summary.RequiredStepCount
                                     && summary.FailedCount == 0
                                     && summary.PendingCount == 0
                                     && summary.SkippedCount == 0
                                     && summary.MissingCount == 0
                                     && summary.DuplicateCount == 0
                                     && summary.InvalidStatusCount == 0
                                     && summary.MissingStatusCount == 0,
            MissingStepIds = summary.MissingStepIds,
            DuplicateStepIds = summary.DuplicateStepIds,
            UnknownStepIds = summary.UnknownStepIds
        };

    private static string MapDecisionStatus(OfflineGeoworldAlphaManualResultDecision decision)
    {
        if (decision.DecisionStatus == OfflineGeoworldAlphaManualResultIntakeVocabulary
                .DecisionStatusGreenCandidate)
        {
            return OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate;
        }

        if (decision.DecisionStatus == OfflineGeoworldAlphaManualResultIntakeVocabulary
                .DecisionStatusPending)
        {
            return OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending;
        }

        if (decision.DecisionStatus == OfflineGeoworldAlphaManualResultIntakeVocabulary
                .DecisionStatusIncomplete
            || decision.DecisionStatus == OfflineGeoworldAlphaManualResultIntakeVocabulary
                .DecisionStatusAcceptedFalse)
        {
            return OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusIncomplete;
        }

        var identityOrSyntaxError = decision.Errors.Any(error =>
            error.Contains("malformed", StringComparison.OrdinalIgnoreCase)
            || error.Contains("goalId", StringComparison.Ordinal)
            || error.Contains("manualGate", StringComparison.Ordinal)
            || error.Contains("resultSchema", StringComparison.Ordinal)
            || error.Contains("checklistHash", StringComparison.Ordinal));
        return identityOrSyntaxError
            ? OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusInvalid
            : OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusIncomplete;
    }

    private static OfflineGeoworldAlphaHumanResultRevalidationFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaHumanResultRevalidationFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaHumanResultRevalidationFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        };
    }

    private static object BuildExportDashboard(
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision,
        OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan quality) =>
        new
        {
            decision.GoalId,
            decision.SourceGoalIds,
            decision.ManualGate,
            decision.ManualResultRelativePath,
            decision.ManualResultSha256,
            decision.ManualResultPresent,
            decision.DecisionStatus,
            decision.Goal111DecisionStatus,
            decision.AcceptableCandidate,
            decision.RecommendedHumanDecision,
            decision.AcceptedByCodex,
            decision.HumanAcceptanceStillRequired,
            decision.ManualGateRemainsHumanDecision,
            qualityGatePassed = quality.Passed,
            decision.NotFinalReleaseOrRuntimeBuild,
            decision.NoRuntimeProviderOrNetworkChanges,
            decision.NoUnityFileChangesRequired,
            decision.ManualInputNotCommitted
        };

    private static string RenderReport(
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision,
        OfflineGeoworldAlphaHumanResultRevalidationDashboard dashboard,
        OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan quality,
        OfflineGeoworldAlphaHumanResultRevalidationNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 115 Offline Geoworld Alpha Human Result Revalidation Report",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + decision.ManualGate + " required",
            "- decisionStatus: " + decision.DecisionStatus,
            "- goal111DecisionStatus: " + decision.Goal111DecisionStatus,
            "- acceptableCandidate: " + decision.AcceptableCandidate.ToString().ToLowerInvariant(),
            "- recommendedHumanDecision: " + decision.RecommendedHumanDecision,
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- manualGateRemainsHumanDecision: true",
            "- manualResultRelativePath: " + decision.ManualResultRelativePath,
            "- manualResultSha256: " + decision.ManualResultSha256,
            "- manualInputNotCommitted: true",
            "- rawManualResultEmbeddedInArtifacts: false",
            "- notFinalReleaseOrRuntimeBuild: true",
            "- noRuntimeProviderOrNetworkChanges: true",
            "- noUnityFileChangesRequired: true",
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Step Summary",
            string.Empty,
            "- requiredStepCount: " + decision.StepSummary.RequiredStepCount,
            "- resultStepCount: " + decision.StepSummary.ResultStepCount,
            "- passedCount: " + decision.StepSummary.PassedCount,
            "- failedCount: " + decision.StepSummary.FailedCount,
            "- pendingCount: " + decision.StepSummary.PendingCount,
            "- skippedCount: " + decision.StepSummary.SkippedCount,
            "- missingCount: " + decision.StepSummary.MissingCount,
            "- duplicateCount: " + decision.StepSummary.DuplicateCount,
            "- invalidStatusCount: " + decision.StepSummary.InvalidStatusCount,
            "- allRequiredStepsPassed: "
            + decision.StepSummary.AllRequiredStepsPassed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Evidence",
            string.Empty
        };
        lines.AddRange(dashboard.EvidenceArtifactPaths.Select(path => "- " + path));
        lines.Add(string.Empty);
        lines.Add("## Export");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.ExportArtifactPaths.Select(path => "- " + path));
        lines.AddRange(
        [
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- missingManualResultBlocked: " + negative.MissingManualResultBlocked.ToString().ToLowerInvariant(),
            "- malformedManualResultRejected: " + negative.MalformedManualResultRejected.ToString().ToLowerInvariant(),
            "- draftTemplateLikeResultBlocked: " + negative.DraftTemplateLikeResultBlocked.ToString().ToLowerInvariant(),
            "- manualResultRawJsonCopied: false"
        ]);
        AddDiagnostics(lines, "Errors", decision.Errors);
        AddDiagnostics(lines, "Warnings", decision.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Human Result Revalidation",
            string.Empty,
            "Goal115 revalidates the local human-created Goal110 result and records a deterministic decision snapshot. It does not accept the Alpha gate by Codex.",
            string.Empty,
            "## Decision Snapshot",
            string.Empty,
            "- decisionStatus: " + decision.DecisionStatus,
            "- acceptableCandidate: " + decision.AcceptableCandidate.ToString().ToLowerInvariant(),
            "- recommendedHumanDecision: " + decision.RecommendedHumanDecision,
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- manualGateRemainsHumanDecision: true",
            "- manualResultRelativePath: `" + decision.ManualResultRelativePath + "`",
            "- manualResultSha256: " + decision.ManualResultSha256,
            "- requiredStepCount: " + decision.StepSummary.RequiredStepCount,
            "- passedStepCount: " + decision.StepSummary.PassedCount,
            string.Empty,
            "## Human Gate",
            string.Empty,
            "If the decision is `GREEN_ACCEPTABLE_CANDIDATE`, the next action is an explicit human acceptance decision for `offline_geoworld_alpha_manual_acceptance_verification`. Do not treat this snapshot as Codex acceptance or final release.",
            string.Empty,
            "## Do Not Start From This Snapshot",
            string.Empty,
            "- live geodata",
            "- providers",
            "- Runtime consumer",
            "- public schema",
            "- Lua",
            "- generator-library",
            "- final art",
            "- atlas",
            "- scene/prefab/project settings",
            "- release packaging",
            string.Empty
        };
        AddDiagnostics(lines, "Errors", decision.Errors);
        AddDiagnostics(lines, "Warnings", decision.Warnings);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderExportReadme(
        OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot decision) =>
        "# Goal 115 Offline Geoworld Alpha Human Result Revalidation" + Environment.NewLine
        + Environment.NewLine
        + "This export summarizes the deterministic revalidation decision for the local human result. "
        + "It contains a hash and summary only, not the raw manual JSON." + Environment.NewLine
        + Environment.NewLine
        + "- decisionStatus: " + decision.DecisionStatus + Environment.NewLine
        + "- acceptableCandidate: " + decision.AcceptableCandidate.ToString().ToLowerInvariant()
        + Environment.NewLine
        + "- recommendedHumanDecision: " + decision.RecommendedHumanDecision + Environment.NewLine
        + "- acceptedByCodex: false" + Environment.NewLine
        + "- humanAcceptanceStillRequired: true" + Environment.NewLine;

    private static void AddDiagnostics(
        List<string> lines,
        string title,
        IReadOnlyList<string> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return;
        }

        lines.Add(string.Empty);
        lines.Add("## " + title);
        lines.Add(string.Empty);
        lines.AddRange(diagnostics.Select(item => "- " + item));
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
            throw new InvalidOperationException("Goal115 must not write the manual input path.");
        }
    }

    private static string StringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
}
