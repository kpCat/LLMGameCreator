using System.Text;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

namespace LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

public sealed class EditDrivenReviewPackagePlayableSessionEvidenceService
{
    public const string ReportMarkdownFileName = "edit-driven-review-package-playable-session-report.md";
    public const string ManifestFileName = "playable-session-manifest.json";
    public const string ActionLogFileName = "playable-session-action-log.json";
    public const string StateChainFileName = "playable-session-state-chain.json";
    public const string ReplayProofFileName = "playable-session-replay-proof.json";
    public const string PackageReadProofFileName = "package-read-proof.json";
    public const string TamperNegativeProofFileName = "tamper-negative-proof.json";
    public const string PlayerCommandIndexFileName = "player-command-index.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly EditDrivenReviewPackagePlayableSessionQualityGateScanner _qualityScanner;

    public EditDrivenReviewPackagePlayableSessionEvidenceService(
        EditDrivenReviewPackagePlayableSessionQualityGateScanner? qualityScanner = null)
    {
        _qualityScanner = qualityScanner ?? new EditDrivenReviewPackagePlayableSessionQualityGateScanner();
    }

    public EditDrivenReviewPackagePlayableSessionBuildResult Build(string projectRootPath)
    {
        var root = Path.GetFullPath(projectRootPath);
        var sourceContext = EditDrivenReviewPackagePlayableSessionReadValidator.LoadFromDisk(root);
        var sourceManifest = BuildSourceArtifactManifest(root, sourceContext);
        var replay = EditDrivenReviewPackagePlayableSessionReplayEngine.Build(sourceContext);
        var negative = EditDrivenReviewPackagePlayableSessionReplayEngine.BuildNegativeProof(
            sourceContext,
            replay.ActionLog);
        var commandIndex = EditDrivenReviewPackagePlayableSessionReplayEngine.BuildPlayerCommandIndex(replay.ActionLog);
        var binding = _qualityScanner.BuildWinFormsBindingInventory(root);
        var preQualityArtifacts = BuildArtifactPayloads(
            sourceManifest,
            manifest: null,
            sourceContext.PackageReadProof,
            replay.ActionLog,
            replay.StateChain,
            replay.ReplayProof,
            negative,
            commandIndex,
            binding,
            quality: null);
        var quality = _qualityScanner.Scan(
            root,
            ReadGoal077AlphaHash(root),
            preQualityArtifacts);
        var reportWithoutHash = BuildReport(
            sourceManifest,
            sourceContext.PackageReadProof,
            replay.ActionLog,
            replay.StateChain,
            replay.ReplayProof,
            negative,
            commandIndex,
            binding,
            quality);
        var report = reportWithoutHash with { DeterministicHash = Hash(reportWithoutHash) };
        var manifest = BuildManifest(
            report,
            sourceContext.PackageReadProof,
            replay.StateChain,
            replay.ReplayProof,
            negative);
        var artifacts = BuildArtifactPayloads(
            sourceManifest,
            manifest,
            sourceContext.PackageReadProof,
            replay.ActionLog,
            replay.StateChain,
            replay.ReplayProof,
            negative,
            commandIndex,
            binding,
            quality);
        var reportMarkdown = RenderReport(report, sourceContext.PackageReadProof, replay.ReplayProof, negative, quality);

        return new EditDrivenReviewPackagePlayableSessionBuildResult
        {
            SourceArtifactManifest = sourceManifest,
            PackageReadProof = sourceContext.PackageReadProof,
            Manifest = manifest,
            ActionLog = replay.ActionLog,
            StateChain = replay.StateChain,
            ReplayProof = replay.ReplayProof,
            TamperNegativeProof = negative,
            PlayerCommandIndex = commandIndex,
            WinFormsBindingInventory = binding,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            ArtifactJsonByFileName = artifacts
        };
    }

    public async Task<EditDrivenReviewPackagePlayableSessionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EditDrivenReviewPackagePlayableSessionWriteResult> WriteAsync(
        string projectRootPath,
        EditDrivenReviewPackagePlayableSessionBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRootPath);
        var outputDirectory = Resolve(root, EditDrivenReviewPackagePlayableSessionVocabulary.RelativeOutputDirectory);
        ResetDirectory(outputDirectory);
        var written = new List<string>();

        foreach (var artifact in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, artifact.Key);
            await File.WriteAllTextAsync(path, artifact.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(reportPath);

        return new EditDrivenReviewPackagePlayableSessionWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        ReportMarkdownFileName,
        ManifestFileName,
        ActionLogFileName,
        StateChainFileName,
        ReplayProofFileName,
        PackageReadProofFileName,
        TamperNegativeProofFileName,
        PlayerCommandIndexFileName,
        WinFormsBindingInventoryFileName,
        QualityGateScanFileName,
        SourceArtifactManifestFileName
    ];

    private static EditDrivenReviewPackagePlayableSessionSourceArtifactManifest BuildSourceArtifactManifest(
        string projectRoot,
        EditDrivenReviewPackagePlayableSessionReadContext sourceContext)
    {
        var artifacts = EditDrivenReviewPackagePlayableSessionVocabulary.RequiredSourceArtifactRelativePaths
            .Select(relativePath => ReadSourceArtifact(projectRoot, relativePath))
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();
        foreach (var missing in artifacts.Where(item => !item.Exists))
        {
            diagnostics.Add(Error(
                "goal078.source.goal077_artifact_missing",
                missing.ArtifactRelativePath,
                "Goal 078 consumes the real disk-backed Goal 077 review package and requires this artifact."));
        }

        var stateDocs = ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var handoff = stateDocs.Contains(
            EditDrivenReviewPackagePlayableSessionVocabulary.Goal077AcceptedHandoffText,
            StringComparison.Ordinal);
        if (!handoff)
        {
            diagnostics.Add(Error(
                "goal078.preflight.goal077_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 077 user handoff must be recorded before Goal 078."));
        }

        if (sourceContext.ReportFields.ImplementationStatus != "GREEN")
        {
            diagnostics.Add(Error(
                "goal078.preflight.goal077_not_green",
                "Goal077.report",
                "Goal 077 evidence must be GREEN before Goal 078 can build a playable session."));
        }

        if (sourceContext.ReportFields.Accepted != "false")
        {
            diagnostics.Add(Error(
                "goal078.preflight.goal077_artifact_acceptance_mutated",
                "Goal077.report",
                "Goal 077 artifact must remain accepted=false; only current-state handoff records acceptance."));
        }

        return new EditDrivenReviewPackagePlayableSessionSourceArtifactManifest
        {
            Goal077AcceptedByUserHandoff = handoff,
            Goal077ReportWasGreenProducedForReview = sourceContext.ReportFields.ImplementationStatus == "GREEN",
            Goal077ArtifactAcceptedFalse = sourceContext.ReportFields.Accepted == "false",
            SourceGoal077ReportHash = sourceContext.PackageReadProof.SourceGoal077ReportHash,
            SourceGoal077ReportDeclaredHash = sourceContext.ReportFields.ReportHash,
            ReviewPackageManifestHash = sourceContext.PackageReadProof.ReviewPackageManifestHash,
            PackageFileLedgerHash = sourceContext.PackageReadProof.PackageFileLedgerHash,
            PackageIndexHash = sourceContext.PackageReadProof.PackageIndexHash,
            PlayerReadableIndexHash = sourceContext.PackageReadProof.PlayerReadableIndexHash,
            SourceArtifactCount = artifacts.Count,
            SourceArtifacts = artifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenReviewPackagePlayableSessionReport BuildReport(
        EditDrivenReviewPackagePlayableSessionSourceArtifactManifest source,
        EditDrivenReviewPackagePlayableSessionPackageReadProof readProof,
        EditDrivenReviewPackagePlayableSessionActionLog actionLog,
        EditDrivenReviewPackagePlayableSessionStateChain stateChain,
        EditDrivenReviewPackagePlayableSessionReplayProof replayProof,
        EditDrivenReviewPackagePlayableSessionNegativeProof negative,
        EditDrivenReviewPackagePlayableSessionPlayerCommandIndex commandIndex,
        EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory binding,
        EditDrivenReviewPackagePlayableSessionQualityGateScan quality)
    {
        var replayDiagnostics = replayProof.Passed
            ? Array.Empty<EditDrivenReviewPackagePlayableSessionDiagnostic>()
            : replayProof.Diagnostics;
        var diagnostics = SortDiagnostics(
            source.Diagnostics
                .Concat(readProof.Diagnostics)
                .Concat(actionLog.Diagnostics)
                .Concat(stateChain.Diagnostics)
                .Concat(replayDiagnostics)
                .Concat(binding.Diagnostics)
                .Concat(quality.Diagnostics));
        var green = diagnostics.All(item => item.Severity != "error")
            && source.Goal077AcceptedByUserHandoff
            && source.Goal077ReportWasGreenProducedForReview
            && source.Goal077ArtifactAcceptedFalse
            && readProof.Passed
            && actionLog.Passed
            && stateChain.Passed
            && replayProof.Passed
            && negative.Passed
            && commandIndex.Passed
            && binding.Passed
            && quality.Passed;

        return new EditDrivenReviewPackagePlayableSessionReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal077AcceptedByUserHandoff = source.Goal077AcceptedByUserHandoff,
            Goal077ImplementationGreen = source.Goal077ReportWasGreenProducedForReview,
            RowCount = readProof.RowCount,
            TargetCount = readProof.TargetCount,
            ActionCount = actionLog.ActionCount,
            SourceGoal077ReportHash = readProof.SourceGoal077ReportHash,
            PackageManifestHash = readProof.ReviewPackageManifestHash,
            PackageFileLedgerHash = readProof.PackageFileLedgerHash,
            PackageIndexHash = readProof.PackageIndexHash,
            PlayerReadableIndexHash = readProof.PlayerReadableIndexHash,
            InitialStateHash = stateChain.InitialStateHash,
            SavedSessionHash = stateChain.SavedSessionHash,
            FinalStateHash = stateChain.FinalStateHash,
            ReplayFinalStateHash = replayProof.ReplayFinalStateHash,
            PackageReadProofHash = Hash(readProof),
            ActionLogHash = Hash(actionLog),
            StateChainHash = Hash(stateChain),
            ReplayProofHash = Hash(replayProof),
            TamperNegativeProofHash = Hash(negative),
            PlayerCommandIndexHash = Hash(commandIndex),
            WinFormsBindingInventoryHash = Hash(binding),
            QualityGateScanHash = Hash(quality),
            Diagnostics = diagnostics
        };
    }

    private static EditDrivenReviewPackagePlayableSessionManifest BuildManifest(
        EditDrivenReviewPackagePlayableSessionReport report,
        EditDrivenReviewPackagePlayableSessionPackageReadProof readProof,
        EditDrivenReviewPackagePlayableSessionStateChain stateChain,
        EditDrivenReviewPackagePlayableSessionReplayProof replayProof,
        EditDrivenReviewPackagePlayableSessionNegativeProof negative) =>
        new()
        {
            ImplementationStatus = report.ImplementationStatus,
            Accepted = false,
            RowCount = report.RowCount,
            TargetCount = report.TargetCount,
            ActionCount = report.ActionCount,
            SourceGoal077ReportHash = report.SourceGoal077ReportHash,
            PackageManifestHash = report.PackageManifestHash,
            PackageFileLedgerHash = report.PackageFileLedgerHash,
            PackageIndexHash = report.PackageIndexHash,
            PlayerReadableIndexHash = report.PlayerReadableIndexHash,
            InitialStateHash = report.InitialStateHash,
            SavedSessionHash = stateChain.SavedSessionHash,
            FinalStateHash = report.FinalStateHash,
            ReplayFinalStateHash = report.ReplayFinalStateHash,
            PackageReadProofPassed = readProof.Passed,
            ReplayProofPassed = replayProof.Passed,
            NegativeProofPassed = negative.Passed
        };

    private static string RenderReport(
        EditDrivenReviewPackagePlayableSessionReport report,
        EditDrivenReviewPackagePlayableSessionPackageReadProof readProof,
        EditDrivenReviewPackagePlayableSessionReplayProof replayProof,
        EditDrivenReviewPackagePlayableSessionNegativeProof negative,
        EditDrivenReviewPackagePlayableSessionQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Goal 078 Edit-Driven Review Package Playable Session",
            string.Empty,
            "- gate: " + EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate + " required",
            "- accepted: false",
            "- implementationStatus: " + report.ImplementationStatus,
            "- goal077Handoff: " + report.Goal077AcceptedByUserHandoff,
            "- goal077ImplementationGreen: " + report.Goal077ImplementationGreen,
            "- rowCount: " + report.RowCount,
            "- targetCount: " + report.TargetCount,
            "- actionCount: " + report.ActionCount,
            "- sourceGoal077ReportHash: " + report.SourceGoal077ReportHash,
            "- packageManifestHash: " + report.PackageManifestHash,
            "- packageFileLedgerHash: " + report.PackageFileLedgerHash,
            "- packageIndexHash: " + report.PackageIndexHash,
            "- playerReadableIndexHash: " + report.PlayerReadableIndexHash,
            "- initialStateHash: " + report.InitialStateHash,
            "- savedSessionHash: " + report.SavedSessionHash,
            "- finalStateHash: " + report.FinalStateHash,
            "- replayFinalStateHash: " + report.ReplayFinalStateHash,
            "- packageReadProofHash: " + report.PackageReadProofHash,
            "- actionLogHash: " + report.ActionLogHash,
            "- stateChainHash: " + report.StateChainHash,
            "- replayProofHash: " + report.ReplayProofHash,
            "- tamperNegativeProofHash: " + report.TamperNegativeProofHash,
            "- qualityGateScanHash: " + report.QualityGateScanHash,
            "- reportHash: " + report.DeterministicHash,
            string.Empty,
            "## Package Read Proof",
            "- packageReadProofPassed: " + readProof.Passed,
            "- allLedgerFilesExist: " + readProof.AllLedgerFilesExist,
            "- allLedgerFileHashesMatch: " + readProof.AllLedgerFileHashesMatch,
            "- allPackageIndexTargetsInLedger: " + readProof.AllPackageIndexTargetsInLedger,
            "- allPlayerIndexTargetsInLedger: " + readProof.AllPlayerIndexTargetsInLedger,
            string.Empty,
            "## Replay Proof",
            "- initialDiffersFromFinal: " + replayProof.InitialDiffersFromFinal,
            "- replayFinalHashMatchesOriginal: " + replayProof.ReplayFinalHashMatchesOriginal,
            "- replayOrderMismatchRejected: " + replayProof.ReplayOrderMismatchRejected,
            "- illegalActionTargetRejected: " + replayProof.IllegalActionTargetRejected,
            "- fakeSuccessWithoutPayloadReadRejected: " + replayProof.FakeSuccessWithoutPayloadReadRejected,
            string.Empty,
            "## Negative Proof"
        };
        lines.AddRange(negative.Scenarios.Select(item => "- " + item.ScenarioId + ": " + item.ActualStatus));
        lines.AddRange(
        [
            string.Empty,
            "## Quality",
            "- maxLineLength: " + quality.MaxLineLength,
            "- minifiedSourceFileCount: " + quality.MinifiedSourceFileCount,
            "- filesOver1000LinesCount: " + quality.FilesOver1000LinesCount,
            "- alphaRuntimeBootstrapLineCount: " + quality.AlphaRuntimeBootstrapLineCount,
            "- alphaRuntimeBootstrapHash: " + quality.AlphaRuntimeBootstrapHash,
            "- alphaRuntimeBootstrapUnchanged: " + quality.AlphaRuntimeBootstrapUnchanged,
            "- absoluteLocalPaths: " + quality.EvidenceContainsAbsoluteLocalPaths,
            "- timestampLikeValues: " + quality.EvidenceContainsTimestampLikeValues,
            "- heavyLogs: " + quality.EvidenceContainsHeavyLogs,
            "- scratchTamperFiles: " + quality.EvidenceContainsScratchTamperFiles,
            "- forbiddenAreaEvidenceDetected: " + quality.ForbiddenAreaEvidenceDetected,
            string.Empty,
            "## Diagnostics"
        ]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- none"]
            : report.Diagnostics.Select(item => "- " + item.Severity + ": " + item.Code + " [" + item.Target + "]"));
        lines.Add(string.Empty);
        lines.Add(EditDrivenReviewPackagePlayableSessionVocabulary.FinalGate + " required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static SortedDictionary<string, string> BuildArtifactPayloads(
        EditDrivenReviewPackagePlayableSessionSourceArtifactManifest source,
        EditDrivenReviewPackagePlayableSessionManifest? manifest,
        EditDrivenReviewPackagePlayableSessionPackageReadProof readProof,
        EditDrivenReviewPackagePlayableSessionActionLog actionLog,
        EditDrivenReviewPackagePlayableSessionStateChain stateChain,
        EditDrivenReviewPackagePlayableSessionReplayProof replayProof,
        EditDrivenReviewPackagePlayableSessionNegativeProof negative,
        EditDrivenReviewPackagePlayableSessionPlayerCommandIndex commandIndex,
        EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory binding,
        EditDrivenReviewPackagePlayableSessionQualityGateScan? quality)
    {
        var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceArtifactManifestFileName] = Serialize(source),
            [PackageReadProofFileName] = Serialize(readProof),
            [ActionLogFileName] = Serialize(actionLog),
            [StateChainFileName] = Serialize(stateChain),
            [ReplayProofFileName] = Serialize(replayProof),
            [TamperNegativeProofFileName] = Serialize(negative),
            [PlayerCommandIndexFileName] = Serialize(commandIndex),
            [WinFormsBindingInventoryFileName] = Serialize(binding)
        };
        if (manifest is not null)
        {
            artifacts[ManifestFileName] = Serialize(manifest);
        }

        if (quality is not null)
        {
            artifacts[QualityGateScanFileName] = Serialize(quality);
        }

        return artifacts;
    }

    private static string ReadGoal077AlphaHash(string projectRoot)
    {
        var json = ReadOptional(
            projectRoot,
            EditDrivenReviewPackagePlayableSessionVocabulary.Goal077RelativeOutputDirectory + "/quality-gate-scan.json");
        var scan = EditDrivenReviewPackagePlayableSessionHash
            .Deserialize<EditDrivenReviewPackageQualityGateScan>(json);
        return scan?.AlphaRuntimeBootstrapHash ?? string.Empty;
    }

    private static EditDrivenReviewPackagePlayableSessionSourceArtifactReference ReadSourceArtifact(
        string projectRoot,
        string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            return new EditDrivenReviewPackagePlayableSessionSourceArtifactReference
            {
                ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath),
                ArtifactRelativePath = relativePath,
                Exists = false
            };
        }

        return new EditDrivenReviewPackagePlayableSessionSourceArtifactReference
        {
            ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath),
            ArtifactRelativePath = relativePath,
            ArtifactHash = EditDrivenReviewPackagePlayableSessionHash.Sha256(File.ReadAllBytes(path)),
            Exists = true
        };
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n') : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static string Serialize<T>(T value) =>
        EditDrivenReviewPackagePlayableSessionHash.Serialize(value);

    private static string Hash<T>(T value) =>
        EditDrivenReviewPackagePlayableSessionHash.Sha256(Serialize(value));

    private static IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics) =>
        EditDrivenReviewPackagePlayableSessionQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenReviewPackagePlayableSessionDiagnostic Error(string code, string target, string message) =>
        EditDrivenReviewPackagePlayableSessionDiagnostic.Error(code, target, message);
}
