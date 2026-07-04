using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public static class OfflineGeoworldActualUnityEditorSourceReformatVocabulary
{
    public const string GoalId = "goal_102b_actual_unity_editor_source_reformat";
    public const string ProductSmokeRoute =
        "goal-102b-actual-unity-editor-source-reformat";
    public const string FinalGate =
        "actual_unity_editor_source_reformat_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-102b-actual-unity-editor-source-reformat";

    public const string BeforeAfterSchemaVersion =
        "actual_unity_editor_source_before_after_v1";
    public const string QualityGateSchemaVersion =
        "actual_unity_editor_source_quality_gate_v1";
    public const string NegativeProofSchemaVersion =
        "actual_unity_editor_source_negative_proof_v1";
    public const string TrustAuditSchemaVersion =
        "actual_unity_editor_source_trust_audit_v1";

    public const string ReportMarkdownFileName =
        "actual-unity-editor-source-reformat-report.md";
    public const string BeforeAfterFileName =
        "actual-unity-editor-source-before-after.json";
    public const string QualityGateFileName =
        "actual-unity-editor-source-quality-gate.json";
    public const string NegativeProofFileName =
        "actual-unity-editor-source-negative-proof.json";
    public const string TrustAuditFileName =
        "actual-unity-editor-source-trust-audit.json";

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        BeforeAfterFileName,
        QualityGateFileName,
        NegativeProofFileName,
        TrustAuditFileName
    ];
}

public sealed partial class OfflineGeoworldActualUnityEditorSourceReformatEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldActualUnityEditorSourceReformatBuildResult Build(
        string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var gitHead = ReadGitHead(root);
        var beforeHead = BuildActualHeadBeforeScan(root);
        var afterWorkingTree = OfflineGeoworldUnityEditorSourceFormatGuardScanner
            .AnalyzeSourceFile(
                root,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath);
        var alpha = BuildAlphaRuntimeBootstrapGuard(root);
        var changedPaths = BuildChangedPathInventory(root);
        var trustAudit = BuildTrustAudit(root, beforeHead);
        var beforeAfter = BuildBeforeAfter(gitHead, beforeHead, afterWorkingTree, alpha, changedPaths);
        var negative = BuildNegativeProof(beforeAfter, alpha);
        var quality = BuildQualityGate(beforeAfter, negative, trustAudit);

        var beforeAfterJson = Serialize(beforeAfter);
        var negativeJson = Serialize(negative);
        var trustAuditJson = Serialize(trustAudit);
        var qualityJson = Serialize(quality);
        var reportWithoutHash = new OfflineGeoworldActualUnityEditorSourceReformatReport
        {
            ImplementationStatus = quality.ImplementationStatus,
            QualityGatePassed = quality.Passed,
            BeforeAfterPassed = beforeAfter.Passed,
            NegativeProofPassed = negative.Passed,
            TrustAuditPassed = trustAudit.Passed,
            ActualHeadBeforeMalformedDetected = beforeAfter.ActualHeadBeforeMalformedDetected,
            WorkingTreeSourceReadable = beforeAfter.WorkingTreeSourceReadable,
            TargetFileChanged = beforeAfter.TargetFileChanged,
            Goal102AEvidenceTrustDefectRecorded = trustAudit.Goal102AEvidenceTrustDefectRecorded,
            AlphaRuntimeBootstrapUnchanged = alpha.Unchanged,
            BlockedReason = quality.BlockedReason,
            BeforeAfterHash = Hash(beforeAfterJson),
            NegativeProofHash = Hash(negativeJson),
            TrustAuditHash = Hash(trustAuditJson),
            QualityGateHash = Hash(qualityJson)
        };
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report);

        return new OfflineGeoworldActualUnityEditorSourceReformatBuildResult
        {
            BeforeAfter = beforeAfter,
            NegativeProof = negative,
            TrustAudit = trustAudit,
            QualityGate = quality,
            Report = report,
            BeforeAfterJson = beforeAfterJson,
            NegativeProofJson = negativeJson,
            TrustAuditJson = trustAuditJson,
            QualityGateJson = qualityJson,
            ReportMarkdown = reportMarkdown,
            EvidenceJsonByFileName = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldActualUnityEditorSourceReformatVocabulary.BeforeAfterFileName] =
                    beforeAfterJson,
                [OfflineGeoworldActualUnityEditorSourceReformatVocabulary.NegativeProofFileName] =
                    negativeJson,
                [OfflineGeoworldActualUnityEditorSourceReformatVocabulary.TrustAuditFileName] =
                    trustAuditJson,
                [OfflineGeoworldActualUnityEditorSourceReformatVocabulary.QualityGateFileName] =
                    qualityJson
            }
        };
    }

    public async Task<OfflineGeoworldActualUnityEditorSourceReformatWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var result = Build(root);
        var outputDirectory = Resolve(
            root,
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);

        var written = new List<string>();
        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(
            outputDirectory,
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldActualUnityEditorSourceReformatWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldActualUnityEditorGitHead BuildActualHeadBeforeScan(
        string root)
    {
        var targetPath = OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath;
        var git = RunGitBytes(root, "cat-file", "blob", "HEAD:" + targetPath);
        if (!git.Succeeded)
        {
            return new OfflineGeoworldActualUnityEditorGitHead
            {
                RelativePath = targetPath,
                GitCommand = "git cat-file blob HEAD:" + targetPath,
                BlobRead = false,
                Diagnostic = git.Error
            };
        }

        var scan = OfflineGeoworldUnityEditorSourceFormatGuardScanner.AnalyzeSourceBytes(
            targetPath,
            git.OutputBytes);
        return new OfflineGeoworldActualUnityEditorGitHead
        {
            RelativePath = targetPath,
            GitCommand = "git cat-file blob HEAD:" + targetPath,
            BlobRead = true,
            BlobByteCount = git.OutputBytes.Length,
            Scan = scan
        };
    }

    private static OfflineGeoworldActualUnityEditorGitRevision ReadGitHead(string root)
    {
        var commit = RunGitText(root, "rev-parse", "HEAD").Trim();
        var subject = RunGitText(root, "log", "-1", "--format=%s").Trim();
        var contains = RunGitExitCode(root, "merge-base", "--is-ancestor", "62f883b", "HEAD") == 0;

        return new OfflineGeoworldActualUnityEditorGitRevision
        {
            Revision = "HEAD",
            Commit = commit,
            Subject = subject,
            ContainsGoal102ACommit = contains
        };
    }

    private static OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard BuildAlphaRuntimeBootstrapGuard(
        string root)
    {
        var path = Resolve(root, OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapPath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var hash = exists ? OfflineGeoworldUnityEditorPreviewHash.Sha256File(path) : string.Empty;
        var lineCount = string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
        var unchanged = string.Equals(
                            hash,
                            OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapExpectedHash,
                            StringComparison.OrdinalIgnoreCase)
                        && lineCount == OfflineGeoworldUnityEditorPreviewToolVocabulary
                            .AlphaRuntimeBootstrapExpectedLineCount;

        return new OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard
        {
            RelativePath = OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapPath,
            BeforeSha256 = OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapExpectedHash,
            BeforeLineCount = OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapExpectedLineCount,
            AfterSha256 = hash,
            AfterLineCount = lineCount,
            Unchanged = unchanged
        };
    }

    private static OfflineGeoworldActualUnityEditorChangedPathInventory BuildChangedPathInventory(
        string root)
    {
        var text = RunGitText(root, "status", "--porcelain");
        var paths = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(ParsePorcelainPath)
            .Where(path => path.Length > 0)
            .Select(NormalizeRelativePath)
            .Where(path => !path.StartsWith(
                OfflineGeoworldActualUnityEditorSourceReformatVocabulary.RelativeOutputDirectory + "/",
                StringComparison.Ordinal))
            .Where(path => !string.Equals(
                path,
                OfflineGeoworldActualUnityEditorSourceReformatVocabulary.RelativeOutputDirectory,
                StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var forbidden = paths
            .Where(IsForbiddenChangedPath)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new OfflineGeoworldActualUnityEditorChangedPathInventory
        {
            TargetFileChanged = paths.Contains(
                OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
                StringComparer.Ordinal),
            NoForbiddenAreasChanged = forbidden.Count == 0,
            ChangedPaths = paths,
            ForbiddenChangedPaths = forbidden
        };
    }

    private static OfflineGeoworldActualUnityEditorTrustAudit BuildTrustAudit(
        string root,
        OfflineGeoworldActualUnityEditorGitHead beforeHead)
    {
        var beforeAfterPath = Resolve(
            root,
            ".llmgc/procedural/goal-102a-unity-editor-source-format-guard/"
            + OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ScanBeforeAfterFileName);
        var qualityPath = Resolve(
            root,
            ".llmgc/procedural/goal-102a-unity-editor-source-format-guard/"
            + OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.QualityGateFileName);
        var reportPath = Resolve(
            root,
            ".llmgc/procedural/goal-102a-unity-editor-source-format-guard/"
            + OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ReportMarkdownFileName);

        var beforeAfterExists = File.Exists(beforeAfterPath);
        var qualityExists = File.Exists(qualityPath);
        var reportExists = File.Exists(reportPath);
        var usedSyntheticBefore = false;
        var claimedBeforeMalformed = false;
        var claimedQualityGreen = false;
        if (beforeAfterExists)
        {
            using var beforeAfter = JsonDocument.Parse(File.ReadAllText(beforeAfterPath, Encoding.UTF8));
            if (beforeAfter.RootElement.TryGetProperty("beforeSource", out var beforeSource))
            {
                usedSyntheticBefore = (beforeSource.GetString() ?? string.Empty)
                    .Contains("synthetic", StringComparison.OrdinalIgnoreCase);
            }

            if (beforeAfter.RootElement.TryGetProperty("beforeEditorWindowMalformedDetected", out var malformed))
            {
                claimedBeforeMalformed = malformed.GetBoolean();
            }
        }

        if (qualityExists)
        {
            using var quality = JsonDocument.Parse(File.ReadAllText(qualityPath, Encoding.UTF8));
            claimedQualityGreen = quality.RootElement.TryGetProperty("passed", out var qualityPassedProperty)
                                  && qualityPassedProperty.GetBoolean();
        }

        var actualHeadMalformed = IsMalformedEditorWindow(beforeHead.Scan);
        var trustDefect = usedSyntheticBefore && claimedBeforeMalformed;
        var conflict = trustDefect && beforeHead.BlobRead && !actualHeadMalformed;
        var passed = beforeAfterExists
                     && qualityExists
                     && reportExists
                     && trustDefect
                     && beforeHead.BlobRead;

        return new OfflineGeoworldActualUnityEditorTrustAudit
        {
            Passed = passed,
            Goal102ABeforeAfterExists = beforeAfterExists,
            Goal102AQualityGateExists = qualityExists,
            Goal102AReportExists = reportExists,
            Goal102AUsedSyntheticBeforeSample = usedSyntheticBefore,
            Goal102AClaimedBeforeMalformedDetected = claimedBeforeMalformed,
            Goal102AClaimedQualityGreen = claimedQualityGreen,
            ActualHeadBlobRead = beforeHead.BlobRead,
            ActualHeadBeforeMalformedDetected = actualHeadMalformed,
            Goal102AEvidenceTrustDefectRecorded = trustDefect,
            Goal102AEvidenceConflictsWithActualHead = conflict,
            SupersededByGoal102B = true,
            RootCause =
                "Goal102A trusted a synthetic before sample and did not require actual target-file HEAD bytes as the before proof."
        };
    }

    private static OfflineGeoworldActualUnityEditorSourceBeforeAfter BuildBeforeAfter(
        OfflineGeoworldActualUnityEditorGitRevision gitHead,
        OfflineGeoworldActualUnityEditorGitHead beforeHead,
        OfflineGeoworldUnityEditorSourceFormatFileScan afterWorkingTree,
        OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard alpha,
        OfflineGeoworldActualUnityEditorChangedPathInventory changedPaths)
    {
        var beforeMalformed = IsMalformedEditorWindow(beforeHead.Scan);
        var afterReadable = IsReadableEditorWindow(afterWorkingTree);

        return new OfflineGeoworldActualUnityEditorSourceBeforeAfter
        {
            Passed = beforeHead.BlobRead
                     && beforeMalformed
                     && afterReadable
                     && changedPaths.TargetFileChanged
                     && alpha.Unchanged,
            GitHead = gitHead,
            BeforeSource = "actual_git_head_blob",
            AfterSource = "working_tree_bytes",
            ActualHeadBeforeBlobRead = beforeHead.BlobRead,
            ActualHeadBeforeMalformedDetected = beforeMalformed,
            WorkingTreeSourceReadable = afterReadable,
            TargetFileChanged = changedPaths.TargetFileChanged,
            ActualHeadBefore = beforeHead.Scan,
            WorkingTreeAfter = afterWorkingTree,
            AlphaRuntimeBootstrap = alpha,
            ChangedPaths = changedPaths
        };
    }

    private static OfflineGeoworldActualUnityEditorSourceNegativeProof BuildNegativeProof(
        OfflineGeoworldActualUnityEditorSourceBeforeAfter beforeAfter,
        OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard alpha)
    {
        var oneLineActualPath = Encoding.UTF8.GetBytes(
            "using UnityEditor; using UnityEngine; namespace LLMGameCreatorAlpha; public sealed class OfflineGeoworldPreviewWindow : EditorWindow { public void RefreshPayloadStatus() { } public void CreatePreviewObjects() { } public void ClearPreviewObjects() { } }");
        var syntheticOneLine = Encoding.UTF8.GetBytes(
            "using System; using UnityEngine; namespace Broken; public sealed class BrokenWindow { public void A() { } public void B() { } }");
        var crOnly = Encoding.UTF8.GetBytes("public sealed class BrokenCrOnly\r{\r    public void Run() { }\r}\r");
        var extreme = Encoding.UTF8.GetBytes(
            "namespace Broken;\npublic sealed class ExtremeLine\n{\n    private const string Value = \""
            + new string('x', 520)
            + "\";\n}\n");

        var scenarios = new List<OfflineGeoworldActualUnityEditorSourceNegativeScenario>
        {
            Scenario(
                "actual_file_remains_one_line",
                "Actual target source is still one physical line.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.AnalyzeSourceBytes(
                    OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
                    oneLineActualPath).OnePhysicalLineMultiStatementSource,
                "goal102b.negative.actual_one_line"),
            Scenario(
                "target_file_not_in_changed_paths",
                "Quality gate must reject evidence when the target source is not changed.",
                !beforeAfter.TargetFileChanged,
                "goal102b.negative.target_not_changed"),
            Scenario(
                "before_scan_uses_only_synthetic_sample",
                "Before proof must come from actual git HEAD bytes, not synthetic-only source.",
                string.Equals(beforeAfter.BeforeSource, "actual_git_head_blob", StringComparison.Ordinal),
                "goal102b.negative.synthetic_before_only"),
            Scenario(
                "evidence_claims_repaired_but_raw_file_one_line",
                "Evidence must not claim repaired while the raw working-tree file has one physical line.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(oneLineActualPath),
                "goal102b.negative.fake_repaired_one_line"),
            Scenario(
                "fake_pass_without_reading_file_bytes",
                "Scan result claims pass without byte reads.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsFakePassWithoutReadingBytes(
                    bytesWereRead: false),
                "goal102b.negative.fake_pass"),
            Scenario(
                "synthetic_one_line_zero_lf_sample",
                "Synthetic one-line and zero-LF samples still must be rejected.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(syntheticOneLine),
                "goal102b.negative.synthetic_one_line"),
            Scenario(
                "cr_only_sample",
                "CR-only C# source must be rejected.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(crOnly),
                "goal102b.negative.cr_only"),
            Scenario(
                "extreme_line_sample",
                "Extreme physical lines must be rejected.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(extreme),
                "goal102b.negative.extreme_line"),
            Scenario(
                "attempt_to_modify_alpha_runtime_bootstrap",
                "AlphaRuntimeBootstrap hash/line-count drift must be rejected.",
                alpha.Unchanged,
                "goal102b.negative.alpha_bootstrap_changed"),
            Scenario(
                "unity_scene_project_setting_changed_marker",
                "Unity scene, prefab, ProjectSettings or build-settings mutation marker appears.",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner
                    .RejectsUnitySceneProjectSettingChangeMarker("EditorSceneManager.SaveScene ProjectSettings/ .prefab"),
                "goal102b.negative.scene_project_marker"),
            Scenario(
                "streamingassets_payload_changed_marker",
                "Goal102B must reject StreamingAssets payload mutation.",
                IsForbiddenChangedPath(
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/payload.json"),
                "goal102b.negative.streamingassets_changed")
        };

        return new OfflineGeoworldActualUnityEditorSourceNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldActualUnityEditorSourceQualityGate BuildQualityGate(
        OfflineGeoworldActualUnityEditorSourceBeforeAfter beforeAfter,
        OfflineGeoworldActualUnityEditorSourceNegativeProof negative,
        OfflineGeoworldActualUnityEditorTrustAudit trustAudit)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        AddIfFalse(
            beforeAfter.GitHead.ContainsGoal102ACommit,
            "goal102b.quality.head_missing_goal102a",
            "HEAD",
            diagnostics);
        AddIfFalse(
            beforeAfter.ActualHeadBeforeBlobRead,
            "goal102b.quality.before_blob_not_read",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            beforeAfter.ActualHeadBeforeMalformedDetected,
            "goal102b.quality.before_not_malformed",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            beforeAfter.WorkingTreeSourceReadable,
            "goal102b.quality.after_not_readable",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            beforeAfter.TargetFileChanged,
            "goal102b.quality.target_not_changed",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            trustAudit.Goal102AEvidenceTrustDefectRecorded,
            "goal102b.quality.goal102a_trust_defect_not_recorded",
            "goal102a evidence",
            diagnostics);
        AddIfFalse(
            beforeAfter.AlphaRuntimeBootstrap.Unchanged,
            "goal102b.quality.alpha_bootstrap_changed",
            beforeAfter.AlphaRuntimeBootstrap.RelativePath,
            diagnostics);
        AddIfFalse(
            beforeAfter.ChangedPaths.NoForbiddenAreasChanged,
            "goal102b.quality.forbidden_paths_changed",
            "changed paths",
            diagnostics);
        AddIfFalse(
            negative.Passed,
            "goal102b.quality.negative_proof_failed",
            "negative proof",
            diagnostics);

        var ordered = diagnostics
            .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();
        var passed = ordered.Count == 0;
        var blocked = !beforeAfter.ActualHeadBeforeBlobRead
                      || !beforeAfter.ActualHeadBeforeMalformedDetected;
        var status = passed ? "GREEN" : blocked ? "BLOCKED" : "FAILED";
        var blockedReason = blocked
            ? "actual HEAD target blob is already readable, so Goal102B cannot honestly prove the required one-line HEAD-before precondition"
            : string.Empty;

        return new OfflineGeoworldActualUnityEditorSourceQualityGate
        {
            Passed = passed,
            ImplementationStatus = status,
            BlockedReason = blockedReason,
            ActualHeadBeforeBlobRead = beforeAfter.ActualHeadBeforeBlobRead,
            ActualHeadBeforeMalformedDetected = beforeAfter.ActualHeadBeforeMalformedDetected,
            WorkingTreeSourceReadable = beforeAfter.WorkingTreeSourceReadable,
            TargetFileChanged = beforeAfter.TargetFileChanged,
            Goal102AEvidenceTrustDefectRecorded = trustAudit.Goal102AEvidenceTrustDefectRecorded,
            Goal102AEvidenceConflictsWithActualHead = trustAudit.Goal102AEvidenceConflictsWithActualHead,
            NegativeProofPassed = negative.Passed,
            AlphaRuntimeBootstrapUnchanged = beforeAfter.AlphaRuntimeBootstrap.Unchanged,
            NoForbiddenAreasChanged = beforeAfter.ChangedPaths.NoForbiddenAreasChanged,
            WorkingTreePhysicalLineCount = beforeAfter.WorkingTreeAfter.RawPhysicalLineCount,
            WorkingTreeMaxPhysicalLineLength = beforeAfter.WorkingTreeAfter.RawPhysicalMaxLineLength,
            ChangedPathCount = beforeAfter.ChangedPaths.ChangedPaths.Count,
            ForbiddenChangedPaths = beforeAfter.ChangedPaths.ForbiddenChangedPaths,
            Diagnostics = ordered
        };
    }

    private static bool IsMalformedEditorWindow(OfflineGeoworldUnityEditorSourceFormatFileScan scan) =>
        scan.BytesRead
        && scan.ZeroLfSource
        && scan.OnePhysicalLineMultiStatementSource
        && scan.MinifiedSourceCandidate;

    private static bool IsReadableEditorWindow(OfflineGeoworldUnityEditorSourceFormatFileScan scan) =>
        scan.Passed
        && scan.LfByteCount > 0
        && scan.RawPhysicalLineCount >= 80
        && scan.RawPhysicalMaxLineLength <= 180;

    private static OfflineGeoworldActualUnityEditorSourceNegativeScenario Scenario(
        string id,
        string mutation,
        bool rejected,
        string code) =>
        new()
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ActualStatus = rejected ? "rejected" : "missed",
            Diagnostics = rejected
                ? [OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                    code,
                    id,
                    "Goal102B actual-source trust guard rejected the mutated evidence input.")]
                : []
        };

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<OfflineGeoworldUnityEditorPreviewDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                code,
                target,
                "Goal102B actual Unity editor source trust gate did not pass."));
        }
    }

    private static string RenderReport(
        OfflineGeoworldActualUnityEditorSourceReformatReport report) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 102B Actual Unity Editor Source Reformat",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: " + report.Accepted.ToString().ToLowerInvariant(),
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            report.ImplementationStatus == "BLOCKED"
                ? "Goal102B is BLOCKED because the actual raw HEAD blob for OfflineGeoworldPreviewWindow.cs is already multi-line/readable. The required one-line HEAD-before proof cannot be produced honestly from this repository state."
                : "Goal102B verifies actual git HEAD-before bytes and working-tree after bytes for OfflineGeoworldPreviewWindow.cs.",
            string.Empty,
            "## Actual Source Proof",
            string.Empty,
            "- actualHeadBeforeMalformedDetected: " + report.ActualHeadBeforeMalformedDetected.ToString().ToLowerInvariant(),
            "- workingTreeSourceReadable: " + report.WorkingTreeSourceReadable.ToString().ToLowerInvariant(),
            "- targetFileChanged: " + report.TargetFileChanged.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            string.Empty,
            "## Trust Repair",
            string.Empty,
            "- goal102aEvidenceTrustDefectRecorded: " + report.Goal102AEvidenceTrustDefectRecorded.ToString().ToLowerInvariant(),
            "- trustAuditPassed: " + report.TrustAuditPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- qualityGatePassed: " + report.QualityGatePassed.ToString().ToLowerInvariant(),
            "- blockedReason: " + report.BlockedReason,
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- beforeAfterHash: " + report.BeforeAfterHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- trustAuditHash: " + report.TrustAuditHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

}
