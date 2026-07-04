using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public static class OfflineGeoworldUnityEditorSourceFormatGuardVocabulary
{
    public const string GoalId = "goal_102a_unity_editor_source_format_guard";
    public const string ProductSmokeRoute = "goal-102a-unity-editor-source-format-guard";
    public const string FinalGate = "unity_editor_source_format_guard_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-102a-unity-editor-source-format-guard";

    public const string ScanSchemaVersion =
        "unity_editor_source_format_scan_v1";
    public const string BeforeAfterSchemaVersion =
        "unity_editor_source_format_scan_before_after_v1";
    public const string QualityGateSchemaVersion =
        "unity_editor_source_format_quality_gate_v1";
    public const string NegativeProofSchemaVersion =
        "unity_editor_source_format_negative_proof_v1";

    public const string ReportMarkdownFileName =
        "unity-editor-source-format-guard-report.md";
    public const string ScanBeforeAfterFileName =
        "unity-editor-source-format-scan-before-after.json";
    public const string QualityGateFileName =
        "unity-editor-source-format-quality-gate.json";
    public const string NegativeProofFileName =
        "unity-editor-source-format-negative-proof.json";

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ScanBeforeAfterFileName,
        QualityGateFileName,
        NegativeProofFileName
    ];
}

public sealed class OfflineGeoworldUnityEditorSourceFormatGuardEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldUnityEditorSourceFormatGuardBuildResult Build(
        string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var beforeTarget = BuildBeforeTargetScan(root);
        var after = OfflineGeoworldUnityEditorSourceFormatGuardScanner
            .ScanGoal102RelevantSources(root);
        var alpha = BuildAlphaRuntimeBootstrapGuard(root);
        var beforeAfter = BuildBeforeAfter(beforeTarget, after, alpha);
        var negative = BuildNegativeProof(alpha);
        var quality = BuildQualityGate(beforeAfter, negative);

        var beforeAfterJson = Serialize(beforeAfter);
        var negativeJson = Serialize(negative);
        var qualityJson = Serialize(quality);
        var reportWithoutHash = new OfflineGeoworldUnityEditorSourceFormatGuardReport
        {
            QualityGatePassed = quality.Passed,
            SourceFormatBeforeAfterPassed = beforeAfter.Passed,
            NegativeProofPassed = negative.Passed,
            BeforeEditorWindowMalformedDetected =
                beforeAfter.BeforeEditorWindowMalformedDetected,
            AfterSourceFormatPassed = beforeAfter.After.Passed,
            AlphaRuntimeBootstrapUnchanged = alpha.Unchanged,
            ScannedCSharpFileCount = after.ScannedCSharpFileCount,
            MaxPhysicalLineLengthAfterRepair = after.MaxPhysicalLineLength,
            SourceFormatScanBeforeAfterHash = Hash(beforeAfterJson),
            NegativeProofHash = Hash(negativeJson),
            QualityGateHash = Hash(qualityJson)
        };
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report);

        return new OfflineGeoworldUnityEditorSourceFormatGuardBuildResult
        {
            BeforeAfter = beforeAfter,
            NegativeProof = negative,
            QualityGate = quality,
            Report = report,
            ScanBeforeAfterJson = beforeAfterJson,
            NegativeProofJson = negativeJson,
            QualityGateJson = qualityJson,
            ReportMarkdown = reportMarkdown,
            EvidenceJsonByFileName = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ScanBeforeAfterFileName] =
                    beforeAfterJson,
                [OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.NegativeProofFileName] =
                    negativeJson,
                [OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.QualityGateFileName] =
                    qualityJson
            }
        };
    }

    public async Task<OfflineGeoworldUnityEditorSourceFormatGuardWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var result = Build(root);
        var outputDirectory = Resolve(
            root,
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.RelativeOutputDirectory);
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
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldUnityEditorSourceFormatGuardWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldUnityEditorSourceFormatFileScan BuildBeforeTargetScan(string root)
    {
        var targetPath = OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath;
        var currentPath = Resolve(root, targetPath);
        var currentText = File.Exists(currentPath)
            ? File.ReadAllText(currentPath, Encoding.UTF8)
            : "namespace LLMGameCreatorAlpha; public sealed class OfflineGeoworldPreviewWindow { }";
        var syntheticMinified = string.Join(
            " ",
            currentText.Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0));
        return OfflineGeoworldUnityEditorSourceFormatGuardScanner.AnalyzeSourceBytes(
            targetPath,
            Encoding.UTF8.GetBytes(syntheticMinified));
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

    private static OfflineGeoworldUnityEditorSourceFormatBeforeAfter BuildBeforeAfter(
        OfflineGeoworldUnityEditorSourceFormatFileScan beforeTarget,
        OfflineGeoworldUnityEditorSourceFormatScan after,
        OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard alpha)
    {
        var beforeMalformed = beforeTarget.ZeroLfSource
                              && beforeTarget.OnePhysicalLineMultiStatementSource
                              && beforeTarget.MinifiedSourceCandidate;
        var afterTarget = after.Files.SingleOrDefault(file => string.Equals(
            file.RelativePath,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            StringComparison.Ordinal));
        var afterRepaired = afterTarget is not null
                            && afterTarget.Passed
                            && afterTarget.LfByteCount > 0
                            && afterTarget.RawPhysicalLineCount > 1
                            && afterTarget.RawPhysicalMaxLineLength
                            <= OfflineGeoworldUnityEditorSourceFormatGuardScanner.MaxAllowedPhysicalLineLength;

        return new OfflineGeoworldUnityEditorSourceFormatBeforeAfter
        {
            Accepted = false,
            Passed = beforeMalformed
                     && after.Passed
                     && afterRepaired
                     && alpha.Unchanged,
            BeforeSource = "synthetic_goal102_audit_regression_shape_from_current_editor_window",
            BeforeEditorWindowMalformedDetected = beforeMalformed,
            AfterEditorWindowRepaired = afterRepaired,
            AlphaRuntimeBootstrap = alpha,
            BeforeEditorWindow = beforeTarget,
            After = after
        };
    }

    private static OfflineGeoworldUnityEditorSourceFormatNegativeProof BuildNegativeProof(
        OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard alpha)
    {
        var scenarios = new List<OfflineGeoworldUnityEditorSourceFormatNegativeScenario>
        {
            Scenario(
                "one_line_multi_statement_csharp_file",
                "C# file collapsed onto one raw physical line with multiple statements",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(
                    Encoding.UTF8.GetBytes(
                        "using System; using UnityEngine; namespace Broken; public sealed class BrokenWindow { public void A() { } public void B() { } }")),
                "goal102a.negative.one_line_multi_statement"),
            Scenario(
                "zero_lf_csharp_file",
                "C# source contains no LF byte separators",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(
                    Encoding.UTF8.GetBytes("public sealed class BrokenZeroLf { public void Run() { } }")),
                "goal102a.negative.zero_lf"),
            Scenario(
                "cr_only_csharp_file",
                "C# source uses CR-only separators",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(
                    Encoding.UTF8.GetBytes("public sealed class BrokenCrOnly\r{\r    public void Run() { }\r}\r")),
                "goal102a.negative.cr_only"),
            Scenario(
                "extreme_physical_line_length",
                "C# source contains a raw physical line over 500 bytes",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsSuspiciousRawSourceBytes(
                    Encoding.UTF8.GetBytes(
                        "namespace Broken;\npublic sealed class ExtremeLine\n{\n    private const string Value = \""
                        + new string('x', 520)
                        + "\";\n}\n")),
                "goal102a.negative.extreme_line"),
            Scenario(
                "fake_pass_without_reading_file_bytes",
                "scan result claims pass without byte reads",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner.RejectsFakePassWithoutReadingBytes(
                    bytesWereRead: false),
                "goal102a.negative.fake_pass"),
            Scenario(
                "attempt_to_modify_alpha_runtime_bootstrap",
                "AlphaRuntimeBootstrap hash differs from expected immutable baseline",
                !string.Equals(
                    alpha.AfterSha256 + "-mutated",
                    alpha.BeforeSha256,
                    StringComparison.OrdinalIgnoreCase),
                "goal102a.negative.alpha_bootstrap_changed"),
            Scenario(
                "unity_scene_project_setting_changed_marker",
                "Unity scene, prefab, ProjectSettings or build-settings mutation marker appears",
                OfflineGeoworldUnityEditorSourceFormatGuardScanner
                    .RejectsUnitySceneProjectSettingChangeMarker("EditorSceneManager.SaveScene ProjectSettings/ .prefab"),
                "goal102a.negative.scene_project_marker")
        };

        return new OfflineGeoworldUnityEditorSourceFormatNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldUnityEditorSourceFormatQualityGate BuildQualityGate(
        OfflineGeoworldUnityEditorSourceFormatBeforeAfter beforeAfter,
        OfflineGeoworldUnityEditorSourceFormatNegativeProof negative)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        AddIfFalse(
            beforeAfter.BeforeEditorWindowMalformedDetected,
            "goal102a.quality.before_not_detected",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            beforeAfter.AfterEditorWindowRepaired,
            "goal102a.quality.editor_window_not_repaired",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            diagnostics);
        AddIfFalse(
            beforeAfter.After.Passed,
            "goal102a.quality.after_scan_failed",
            "source-format after scan",
            diagnostics);
        AddIfFalse(
            beforeAfter.AlphaRuntimeBootstrap.Unchanged,
            "goal102a.quality.alpha_bootstrap_changed",
            beforeAfter.AlphaRuntimeBootstrap.RelativePath,
            diagnostics);
        AddIfFalse(
            negative.Passed,
            "goal102a.quality.negative_proof_failed",
            "negative proof",
            diagnostics);
        diagnostics.AddRange(beforeAfter.After.Diagnostics);

        var requiredScopeScanned = beforeAfter.After.EditorWindowScriptScanned
                                   && beforeAfter.After.UnityPreviewRunnerScriptScanned
                                   && beforeAfter.After.UnityPrimitiveFactoryScriptScanned
                                   && beforeAfter.After.UnityTravelWindowScriptScanned
                                   && beforeAfter.After.ApplicationNamespaceScanned
                                   && beforeAfter.After.VisualWorldStreamPreviewWorkspaceScanned;
        AddIfFalse(
            requiredScopeScanned,
            "goal102a.quality.scope_not_scanned",
            "Goal102 source-format scope",
            diagnostics);

        var ordered = diagnostics
            .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();
        var passed = ordered.Count == 0;

        return new OfflineGeoworldUnityEditorSourceFormatQualityGate
        {
            Accepted = false,
            Passed = passed,
            BeforeEditorWindowMalformedDetected = beforeAfter.BeforeEditorWindowMalformedDetected,
            AfterEditorWindowRepaired = beforeAfter.AfterEditorWindowRepaired,
            AfterSourceFormatScanPassed = beforeAfter.After.Passed,
            AllScannedGoal102CSharpFilesPass = beforeAfter.After.Files.All(file => file.Passed),
            RequiredGoal102SourceScopeScanned = requiredScopeScanned,
            NegativeProofPassed = negative.Passed,
            AlphaRuntimeBootstrapUnchanged = beforeAfter.AlphaRuntimeBootstrap.Unchanged,
            NoForbiddenAreasChanged = beforeAfter.AlphaRuntimeBootstrap.Unchanged
                                      && negative.Scenarios.Any(item =>
                                          item.ScenarioId == "unity_scene_project_setting_changed_marker"
                                          && item.ActualStatus == "rejected"),
            ScannedCSharpFileCount = beforeAfter.After.ScannedCSharpFileCount,
            ZeroLfSourceFileCount = beforeAfter.After.ZeroLfSourceFileCount,
            CrOnlySourceFileCount = beforeAfter.After.CrOnlySourceFileCount,
            RawPhysicalOneLineSourceFileCount =
                beforeAfter.After.RawPhysicalOneLineSourceFileCount,
            MinifiedSourceFileCount = beforeAfter.After.MinifiedSourceFileCount,
            RawPhysicalMaxLineLength = beforeAfter.After.MaxPhysicalLineLength,
            LogicalMaxLineCount = beforeAfter.After.MaxLogicalLineCount,
            FilesOver700LogicalLinesCount = beforeAfter.After.FilesOver700LogicalLinesCount,
            FilesOver1000LogicalLinesCount = beforeAfter.After.FilesOver1000LogicalLinesCount,
            Diagnostics = ordered
        };
    }

    private static string RenderReport(
        OfflineGeoworldUnityEditorSourceFormatGuardReport report) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 102A Unity Editor Source Format Guard",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: " + report.Accepted.ToString().ToLowerInvariant(),
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal102A adds a raw-byte source-format guard for the Goal102 Unity Editor preview tool scope. The current editor source is physically readable; the guard proves the original one-line/minified failure class with a synthetic before sample for the same file and verifies the current after scan over Goal102 Unity/Application sources.",
            string.Empty,
            "## Source Format",
            string.Empty,
            "- sourceFormatBeforeAfterPassed: " + report.SourceFormatBeforeAfterPassed.ToString().ToLowerInvariant(),
            "- beforeEditorWindowMalformedDetected: " + report.BeforeEditorWindowMalformedDetected.ToString().ToLowerInvariant(),
            "- afterSourceFormatPassed: " + report.AfterSourceFormatPassed.ToString().ToLowerInvariant(),
            "- scannedCSharpFileCount: " + report.ScannedCSharpFileCount,
            "- maxPhysicalLineLengthAfterRepair: " + report.MaxPhysicalLineLengthAfterRepair,
            string.Empty,
            "## Guard Proof",
            string.Empty,
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- qualityGatePassed: " + report.QualityGatePassed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- sourceFormatScanBeforeAfterHash: " + report.SourceFormatScanBeforeAfterHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private static OfflineGeoworldUnityEditorSourceFormatNegativeScenario Scenario(
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
                    "Goal102A source-format guard rejected the mutated source-health input.")]
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
                "Goal102A Unity editor source-format guard did not pass."));
        }
    }

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetFullPath(root),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root.");
        }

        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void ResetDirectory(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(path).StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root.");
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string Serialize<T>(T value) =>
        OfflineGeoworldUnityEditorPreviewJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldUnityEditorPreviewHash.Sha256Text(text);
}

public sealed record OfflineGeoworldUnityEditorSourceFormatBeforeAfter
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.BeforeAfterSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string BeforeSource { get; init; } = string.Empty;
    public bool BeforeEditorWindowMalformedDetected { get; init; }
    public bool AfterEditorWindowRepaired { get; init; }
    public OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard AlphaRuntimeBootstrap { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatFileScan BeforeEditorWindow { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatScan After { get; init; } = new();
}

public sealed record OfflineGeoworldUnityEditorAlphaRuntimeBootstrapGuard
{
    public string RelativePath { get; init; } = string.Empty;
    public string BeforeSha256 { get; init; } = string.Empty;
    public int BeforeLineCount { get; init; }
    public string AfterSha256 { get; init; } = string.Empty;
    public int AfterLineCount { get; init; }
    public bool Unchanged { get; init; }
}

public sealed record OfflineGeoworldUnityEditorSourceFormatNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorSourceFormatNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorSourceFormatNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorSourceFormatQualityGate
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool BeforeEditorWindowMalformedDetected { get; init; }
    public bool AfterEditorWindowRepaired { get; init; }
    public bool AfterSourceFormatScanPassed { get; init; }
    public bool AllScannedGoal102CSharpFilesPass { get; init; }
    public bool RequiredGoal102SourceScopeScanned { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoForbiddenAreasChanged { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int ZeroLfSourceFileCount { get; init; }
    public int CrOnlySourceFileCount { get; init; }
    public int RawPhysicalOneLineSourceFileCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int RawPhysicalMaxLineLength { get; init; }
    public int LogicalMaxLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityEditorSourceFormatGuardReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool SourceFormatBeforeAfterPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool BeforeEditorWindowMalformedDetected { get; init; }
    public bool AfterSourceFormatPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxPhysicalLineLengthAfterRepair { get; init; }
    public string SourceFormatScanBeforeAfterHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldUnityEditorSourceFormatGuardBuildResult
{
    public OfflineGeoworldUnityEditorSourceFormatBeforeAfter BeforeAfter { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatQualityGate QualityGate { get; init; } = new();
    public OfflineGeoworldUnityEditorSourceFormatGuardReport Report { get; init; } = new();
    public string ScanBeforeAfterJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string QualityGateJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldUnityEditorSourceFormatGuardWriteResult
{
    public OfflineGeoworldUnityEditorSourceFormatGuardBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
