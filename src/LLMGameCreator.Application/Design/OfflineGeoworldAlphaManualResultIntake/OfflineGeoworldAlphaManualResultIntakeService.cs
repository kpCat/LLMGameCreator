using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

public sealed partial class OfflineGeoworldAlphaManualResultIntakeService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaManualResultIntakeBuildResult Build(string repositoryRootPath) =>
        Build(repositoryRootPath, OfflineGeoworldAlphaManualResultIntakeVocabulary
            .DefaultCandidateResultRelativePaths);

    public OfflineGeoworldAlphaManualResultIntakeBuildResult Build(
        string repositoryRootPath,
        IReadOnlyList<string> candidateResultRelativePaths)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var metadata = LoadGoal110Metadata(root);
        var decision = BuildDecision(root, metadata, candidateResultRelativePaths);
        return BuildArtifacts(root, metadata, decision);
    }

    public async Task<OfflineGeoworldAlphaManualResultIntakeWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, OfflineGeoworldAlphaManualResultIntakeVocabulary
            .ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaManualResultIntakeVocabulary
            .ExportPackageDirectory);
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

        return new OfflineGeoworldAlphaManualResultIntakeWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            WrittenFiles = written
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static OfflineGeoworldAlphaManualResultIntakeBuildResult BuildArtifacts(
        string root,
        Goal110Metadata metadata,
        OfflineGeoworldAlphaManualResultDecision decision)
    {
        var missingProof = new OfflineGeoworldAlphaManualResultIntakeNegativeProof
        {
            ScenarioId = "missing_manual_result",
            Passed = true,
            DecisionStatus = OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            Diagnostic = "Missing or empty manual result is blocked as pending, not accepted."
        };
        var invalidProof = BuildInvalidResultProof(root, metadata);
        var quality = BuildQualityGate(root, metadata, decision, missingProof, invalidProof);
        var report = new OfflineGeoworldAlphaManualResultIntakeReport
        {
            ImplementationStatus = quality.ImplementationStatus,
            DecisionStatus = decision.DecisionStatus,
            AcceptableCandidate = decision.AcceptableCandidate,
            AcceptedByCodex = false,
            HumanAcceptanceStillRequired = true,
            ResultFilePath = decision.ResultFilePath
        };
        var reportWithoutHash = RenderReport(report, decision, quality, deterministicReportHash: string.Empty);
        report = report with { DeterministicReportHash = HashText(reportWithoutHash) };

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName] =
                Serialize(decision),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.ReportFileName] =
                RenderReport(report, decision, quality, report.DeterministicReportHash),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.MissingResultProofFileName] =
                Serialize(missingProof),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.InvalidResultProofFileName] =
                Serialize(invalidProof),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.ValidSampleResultFileName] =
                BuildValidSampleResult(metadata)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            "goal111_manual_result_intake_evidence");
        proceduralFiles[OfflineGeoworldAlphaManualResultIntakeVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportDashboard = BuildExportDashboard(decision, quality);
        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportDashboardFileName] =
                Serialize(exportDashboard),
            [OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportReadmeFileName] =
                RenderExportReadme(decision)
        };
        var exportIndex = BuildFileIndex(exportFiles, "goal111_manual_result_intake_export");
        exportFiles[OfflineGeoworldAlphaManualResultIntakeVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAlphaManualResultIntakeBuildResult
        {
            Decision = decision,
            QualityGateScan = quality,
            Report = report,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            MissingResultProof = missingProof,
            InvalidResultProof = invalidProof,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles
        };
    }

    private static OfflineGeoworldAlphaManualResultIntakeNegativeProof BuildInvalidResultProof(
        string root,
        Goal110Metadata metadata)
    {
        var scratchRoot = Path.Combine(Path.GetTempPath(), "llmgc-goal111-invalid-proof");
        var decision = ValidateResultText(
            root,
            metadata,
            "offline-geoworld-alpha-acceptance-result.json",
            "{ invalid json");
        return new OfflineGeoworldAlphaManualResultIntakeNegativeProof
        {
            ScenarioId = "malformed_manual_result_json",
            Passed = decision.DecisionStatus
                     == OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            DecisionStatus = decision.DecisionStatus,
            Diagnostic = scratchRoot.Length > 0
                ? "Malformed manual result JSON is rejected deterministically."
                : "Malformed manual result JSON is rejected deterministically."
        };
    }

    private static OfflineGeoworldAlphaManualResultIntakeQualityGateScan BuildQualityGate(
        string root,
        Goal110Metadata metadata,
        OfflineGeoworldAlphaManualResultDecision decision,
        OfflineGeoworldAlphaManualResultIntakeNegativeProof missingProof,
        OfflineGeoworldAlphaManualResultIntakeNegativeProof invalidProof)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        var sourceFiles = new[]
            {
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/"
                + "OfflineGeoworldAlphaManualResultIntakeModels.cs",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/"
                + "OfflineGeoworldAlphaManualResultIntakeService.cs",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/"
                + "OfflineGeoworldAlphaManualResultIntakeService.Validation.cs",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
                + "VisualWorldStreamPreviewWorkspaceModels.Goal111.cs",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
                + "VisualWorldStreamPreviewOfflineGeoworldAlphaManualResultIntakeInspector.cs",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
                + "VisualWorldStreamPreviewWorkspacePageControl.Goal111.cs"
            }
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(CountLines);
        var sourceHealthPassed = sourceFiles.All(text => CountLines(text) < 700);
        var implementationStatus = metadata.PackagePresent && sourceHealthPassed ? "GREEN" : "FAILED";

        Require(metadata.PackagePresent, "goal111.goal110_package_present");
        Require(!string.IsNullOrWhiteSpace(decision.ChecklistHashExpected),
            "goal111.checklist_hash_expected");
        Require(decision.ChecklistHashActual == decision.ChecklistHashExpected,
            "goal111.checklist_hash_match");
        Require(missingProof.Passed, "goal111.negative_missing_result");
        Require(invalidProof.Passed, "goal111.negative_invalid_result");
        Require(!decision.AcceptedByCodex, "goal111.accepted_by_codex_false");
        Require(decision.HumanAcceptanceStillRequired, "goal111.human_gate_still_required");
        Require(decision.NotFinalReleaseOrRuntimeBuild, "goal111.not_final_release");
        Require(decision.NoRuntimeProviderOrNetworkChanges, "goal111.no_runtime_provider_network");
        Require(sourceHealthPassed, "goal111.source_health");

        return new OfflineGeoworldAlphaManualResultIntakeQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Passed = diagnostics.Count == 0,
            DecisionStatus = decision.DecisionStatus,
            Goal110PackagePresent = metadata.PackagePresent,
            ChecklistHashResolved = !string.IsNullOrWhiteSpace(decision.ChecklistHashExpected)
                                    && decision.ChecklistHashActual == decision.ChecklistHashExpected,
            MissingResultProofPassed = missingProof.Passed,
            InvalidResultProofPassed = invalidProof.Passed,
            RequiredStepCount = metadata.RequiredSteps.Count,
            ProceduralFileCount =
                OfflineGeoworldAlphaManualResultIntakeVocabulary.RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaManualResultIntakeVocabulary.RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceFiles.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes =
            [
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory + "/",
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory + "/",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/"
                + "OfflineGeoworldAlphaManualResultIntakeProductSmokeTests.cs",
                "docs/agent-tasks/goal-111-offline-geoworld-alpha-manual-result-intake/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/MILESTONE_GATES.md",
                "docs/RELEASE_RISK_REGISTER.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaManualResultIntakeFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaManualResultIntakeFileIndexEntry
            {
                RelativePath = item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaManualResultIntakeFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries
        };
    }

    private static object BuildExportDashboard(
        OfflineGeoworldAlphaManualResultDecision decision,
        OfflineGeoworldAlphaManualResultIntakeQualityGateScan quality) =>
        new
        {
            goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
            sourceGoalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
            manualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
            decisionStatus = decision.DecisionStatus,
            acceptableCandidate = decision.AcceptableCandidate,
            acceptedByCodex = decision.AcceptedByCodex,
            humanAcceptanceStillRequired = decision.HumanAcceptanceStillRequired,
            resultFilePath = decision.ResultFilePath,
            qualityGatePassed = quality.Passed,
            validManualResultAvailableForHumanGateDecision = decision.AcceptableCandidate,
            notFinalReleaseOrRuntimeBuild = true,
            noRuntimeProviderOrNetworkChanges = true
        };

    private static string BuildValidSampleResult(Goal110Metadata metadata)
    {
        var steps = metadata.RequiredSteps
            .OrderBy(step => step.StepId, StringComparer.Ordinal)
            .Select(step => new
            {
                stepId = step.StepId,
                status = "passed",
                notes = "sample fixture only; not real human acceptance",
                evidenceRef = step.StepId + "Evidence"
            })
            .ToArray();
        return Serialize(new
        {
            sampleOnly = true,
            notRealHumanAcceptance = true,
            goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId,
            manualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
            resultSchema = metadata.ResultSchema,
            accepted = true,
            checklistHash = metadata.ChecklistHashExpected,
            steps
        });
    }

    private static string RenderReport(
        OfflineGeoworldAlphaManualResultIntakeReport report,
        OfflineGeoworldAlphaManualResultDecision decision,
        OfflineGeoworldAlphaManualResultIntakeQualityGateScan quality,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 111 Offline Geoworld Alpha Manual Result Intake Report",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- decisionStatus: " + report.DecisionStatus,
            "- acceptableCandidate: " + report.AcceptableCandidate.ToString().ToLowerInvariant(),
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- deterministicReportHash: " + deterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 111 reads Goal110 manual acceptance metadata and any deterministic manual result JSON. It only produces a decision bridge; it is not final release packaging and does not mark the Alpha accepted by Codex.",
            string.Empty,
            "## Decision",
            string.Empty,
            "- resultFilePath: "
            + (string.IsNullOrWhiteSpace(decision.ResultFilePath) ? "(none)" : decision.ResultFilePath),
            "- checklistHashExpected: " + decision.ChecklistHashExpected,
            "- checklistHashActual: " + decision.ChecklistHashActual,
            "- passedSteps: " + decision.StepSummary.PassedCount,
            "- failedSteps: " + decision.StepSummary.FailedCount,
            "- pendingSteps: " + decision.StepSummary.PendingCount,
            "- skippedSteps: " + decision.StepSummary.SkippedCount,
            "- missingSteps: " + decision.StepSummary.MissingCount,
            "- duplicateSteps: " + decision.StepSummary.DuplicateCount,
            "- invalidStatusSteps: " + decision.StepSummary.InvalidStatusCount,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- validManualResultAvailableForHumanGateDecision: "
            + decision.AcceptableCandidate.ToString().ToLowerInvariant()
        };
        if (decision.Errors.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Errors");
            lines.AddRange(decision.Errors.Select(error => "- " + error));
        }

        if (decision.Warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Warnings");
            lines.AddRange(decision.Warnings.Select(warning => "- " + warning));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderExportReadme(OfflineGeoworldAlphaManualResultDecision decision) =>
        "# Goal 111 Offline Geoworld Alpha Manual Result Intake" + Environment.NewLine
        + Environment.NewLine
        + "This export summarizes the deterministic manual result intake decision. "
        + "It does not mark the Alpha accepted by Codex and is not final release packaging."
        + Environment.NewLine
        + Environment.NewLine
        + "- decisionStatus: " + decision.DecisionStatus + Environment.NewLine
        + "- acceptableCandidate: " + decision.AcceptableCandidate.ToString().ToLowerInvariant()
        + Environment.NewLine
        + "- humanAcceptanceStillRequired: true" + Environment.NewLine;

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
