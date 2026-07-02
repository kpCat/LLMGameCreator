using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

public sealed class EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService
{
    public const string ReportMarkdownFileName =
        "edit-driven-gamepackage-runtime-preview-playthrough-report.md";
    public const string CommandScriptFileName = "playthrough-command-script.json";
    public const string TranscriptFileName = "playthrough-transcript.json";
    public const string StateHashChainFileName = "playthrough-state-hash-chain.json";
    public const string CoverageLedgerFileName = "playthrough-coverage-ledger.json";
    public const string PackageReadProofFileName = "package-read-proof.json";
    public const string NegativeProofFileName = "playthrough-negative-proof.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly EditDrivenGamePackageRuntimePreviewPlaythroughCommandBuilder _commandBuilder = new();
    private readonly EditDrivenGamePackageRuntimePreviewPlaythroughReplayEngine _replayEngine = new();
    private readonly EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner _qualityScanner = new();
    private readonly GamePackageValidator _validator = new();
    private readonly GeneratedPackageRuntimePreviewService _runtimePreviewService = new();

    public EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var commandScript = _commandBuilder.Build(context);
        var replay = _replayEngine.Replay(context, commandScript);
        var negativeProof = BuildNegativeProof(context, commandScript);
        var bindingInventory =
            EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner.BuildWinFormsBindingInventory(root);
        var preQualityArtifacts = BuildArtifactPayloads(
            context.SourceArtifactManifest,
            context.PackageReadProof,
            commandScript,
            replay.Transcript,
            replay.StateHashChain,
            replay.CoverageLedger,
            negativeProof,
            bindingInventory,
            qualityGateScan: null);
        var qualityGateScan = _qualityScanner.Scan(root, bindingInventory, preQualityArtifacts);
        var reportWithoutHash = BuildReport(
            context.SourceArtifactManifest,
            context.PackageReadProof,
            commandScript,
            replay.Transcript,
            replay.StateHashChain,
            replay.CoverageLedger,
            negativeProof,
            bindingInventory,
            qualityGateScan);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(reportWithoutHash)
        };
        var artifacts = BuildArtifactPayloads(
            context.SourceArtifactManifest,
            context.PackageReadProof,
            commandScript,
            replay.Transcript,
            replay.StateHashChain,
            replay.CoverageLedger,
            negativeProof,
            bindingInventory,
            qualityGateScan);

        return new EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult
        {
            SourceArtifactManifest = context.SourceArtifactManifest,
            PackageReadProof = context.PackageReadProof,
            CommandScript = commandScript,
            Transcript = replay.Transcript,
            StateHashChain = replay.StateHashChain,
            CoverageLedger = replay.CoverageLedger,
            NegativeProof = negativeProof,
            WinFormsBindingInventory = bindingInventory,
            QualityGateScan = qualityGateScan,
            Report = report,
            ReportMarkdown = EditDrivenGamePackageRuntimePreviewPlaythroughReportRenderer.Render(report),
            ArtifactJsonByFileName = artifacts
        };
    }

    public async Task<EditDrivenGamePackageRuntimePreviewPlaythroughWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var result = Build(root);
        var outputDirectory = Resolve(root, EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.RelativeOutputDirectory);
        ResetDirectory(outputDirectory);
        var written = new List<string>();

        foreach (var artifact in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, artifact.Key);
            await File.WriteAllTextAsync(path, artifact.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new EditDrivenGamePackageRuntimePreviewPlaythroughWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
        EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.RequiredArtifactFileNames;

    private Goal081SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var sourceArtifacts = EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.RequiredSourceArtifactRelativePaths
            .Select(relativePath => ReadSourceArtifact(root, relativePath, diagnostics))
            .OrderBy(artifact => artifact.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();

        var goal080ReportText = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/edit-driven-gamepackage-runtime-preview-bridge-report.md");
        var packageJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/projected-gamepackage/package.json");
        var projectedIndexJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/projected-gamepackage/projected-package-index.json");
        var playerIndexJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/projected-gamepackage/player-readable-bridge-index.json");
        var sourceTargetsJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/projected-gamepackage/source-targets.json");
        var bridgeProofJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/runtime-preview-bridge-proof.json");
        var bridgeNegativeJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/runtime-preview-negative-proof.json");
        var bridgeQualityJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/quality-gate-scan.json");
        var bridgeSourceManifestJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080RelativeOutputDirectory
            + "/source-artifact-manifest.json");
        var actionLogJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal078RelativeOutputDirectory
            + "/playable-session-action-log.json");
        var replayProofJson = ReadOptional(
            root,
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal078RelativeOutputDirectory
            + "/playable-session-replay-proof.json");

        var reportFields = ParseReportFields(goal080ReportText);
        var package = Deserialize<GamePackageDefinition>(packageJson, "projected-gamepackage/package.json", diagnostics);
        var projectedIndex = Deserialize<Goal081ProjectedPackageIndex>(
            projectedIndexJson,
            "projected-gamepackage/projected-package-index.json",
            diagnostics) ?? new Goal081ProjectedPackageIndex();
        var playerIndex = Deserialize<Goal081PlayerReadableBridgeIndex>(
            playerIndexJson,
            "projected-gamepackage/player-readable-bridge-index.json",
            diagnostics) ?? new Goal081PlayerReadableBridgeIndex();
        var sourceTargets = Deserialize<Goal081SourceTargetsDocument>(
            sourceTargetsJson,
            "projected-gamepackage/source-targets.json",
            diagnostics) ?? new Goal081SourceTargetsDocument();
        var bridgeProof = Deserialize<EditDrivenGamePackageRuntimePreviewBridgeProof>(
            bridgeProofJson,
            "runtime-preview-bridge-proof.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewBridgeProof();
        var bridgeNegative = Deserialize<EditDrivenGamePackageRuntimePreviewBridgeNegativeProof>(
            bridgeNegativeJson,
            "runtime-preview-negative-proof.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewBridgeNegativeProof();
        var bridgeQuality = Deserialize<EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan>(
            bridgeQualityJson,
            "quality-gate-scan.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan();
        var bridgeSourceManifest = Deserialize<EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest>(
            bridgeSourceManifestJson,
            "source-artifact-manifest.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest();
        var actionLog = Deserialize<EditDrivenReviewPackagePlayableSessionActionLog>(
            actionLogJson,
            "playable-session-action-log.json",
            diagnostics) ?? new EditDrivenReviewPackagePlayableSessionActionLog();
        var replayProof = Deserialize<EditDrivenReviewPackagePlayableSessionReplayProof>(
            replayProofJson,
            "playable-session-replay-proof.json",
            diagnostics) ?? new EditDrivenReviewPackagePlayableSessionReplayProof();

        var currentStateDocs = ReadOptional(root, "docs/CURRENT_GENERATOR_STATE.md")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/CURRENT_GENERATOR_STATE.json")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/CONTEXT_INDEX.md")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var sourceManifest = BuildSourceArtifactManifest(
            sourceArtifacts,
            reportFields,
            currentStateDocs,
            packageJson,
            bridgeProofJson,
            bridgeNegativeJson,
            actionLogJson,
            diagnostics);
        var packageItemByTargetId = BuildPackageItemMap(package);
        var packageInteractionByTargetId = BuildPackageInteractionMap(package);
        var readProof = BuildPackageReadProof(
            package,
            packageJson,
            projectedIndex,
            projectedIndexJson,
            playerIndex,
            playerIndexJson,
            sourceTargets,
            sourceTargetsJson,
            bridgeProof,
            bridgeProofJson,
            bridgeNegative,
            bridgeQuality,
            reportFields,
            sourceManifest,
            actionLog,
            actionLogJson,
            diagnostics);

        return new Goal081SourceContext
        {
            RootPath = root,
            Package = package,
            ProjectedIndex = projectedIndex,
            PlayerIndex = playerIndex,
            SourceTargets = sourceTargets,
            BridgeProof = bridgeProof,
            BridgeNegativeProof = bridgeNegative,
            BridgeQualityGate = bridgeQuality,
            BridgeSourceManifest = bridgeSourceManifest,
            Goal078ActionLog = actionLog,
            Goal078ReplayProof = replayProof,
            SourceArtifactManifest = sourceManifest,
            PackageReadProof = readProof,
            PackageItemByTargetId = packageItemByTargetId,
            PackageInteractionByTargetId = packageInteractionByTargetId,
            Diagnostics = SortDiagnostics(diagnostics.Concat(readProof.Diagnostics))
        };
    }

    private EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof BuildPackageReadProof(
        GamePackageDefinition? package,
        string packageJson,
        Goal081ProjectedPackageIndex projectedIndex,
        string projectedIndexJson,
        Goal081PlayerReadableBridgeIndex playerIndex,
        string playerIndexJson,
        Goal081SourceTargetsDocument sourceTargets,
        string sourceTargetsJson,
        EditDrivenGamePackageRuntimePreviewBridgeProof bridgeProof,
        string bridgeProofJson,
        EditDrivenGamePackageRuntimePreviewBridgeNegativeProof bridgeNegative,
        EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan bridgeQuality,
        IReadOnlyDictionary<string, string> reportFields,
        EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest sourceManifest,
        EditDrivenReviewPackagePlayableSessionActionLog actionLog,
        string actionLogJson,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> sourceDiagnostics)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var packagePayloadRead = !string.IsNullOrWhiteSpace(packageJson);
        var packageDeserialized = package is not null;
        var packageHash = packagePayloadRead
            ? EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(packageJson)
            : string.Empty;
        var validation = packageDeserialized ? _validator.Validate(package!) : new();
        foreach (var issue in validation.Issues.Where(issue => issue.Severity >= ValidationSeverity.Error))
        {
            diagnostics.Add(Error(issue.Code, issue.TargetId ?? "projected-gamepackage/package.json", issue.Message));
        }

        var previewProjectionPassed = false;
        if (packageDeserialized)
        {
            var preview = _runtimePreviewService.Build(
                package!,
                new GameState { CurrentMapId = package!.Manifest.StartMapId });
            previewProjectionPassed = preview.CurrentScene is not null
                                      && preview.Warnings.Count == 0
                                      && preview.Items.Count == sourceTargets.TargetCount
                                      && preview.Quests.Count == projectedIndex.RowCount
                                      && preview.Mechanics.Count == sourceTargets.TargetCount;
        }

        var reportPackageHash = Value(reportFields, "projectedPackageHash");
        var reportStatus = Value(reportFields, "implementationStatus");
        var reportAccepted = Value(reportFields, "accepted");
        var projectedIndexRead = !string.IsNullOrWhiteSpace(projectedIndexJson) && projectedIndex.Passed;
        var playerIndexRead = !string.IsNullOrWhiteSpace(playerIndexJson) && playerIndex.Passed;
        var sourceTargetsRead = !string.IsNullOrWhiteSpace(sourceTargetsJson)
                                && sourceTargets.Targets.Count == sourceTargets.TargetCount;
        var bridgeProofRead = !string.IsNullOrWhiteSpace(bridgeProofJson);
        var packageHashMatchesReport = packageHash == reportPackageHash;
        var packageHashMatchesIndex = packageHash == projectedIndex.PackageHash;
        var packageHashMatchesBridgeProof = packageHash == bridgeProof.ProjectedPackageHash;
        var rowCount = projectedIndex.RowCount;
        var targetCount = sourceTargets.TargetCount;
        var actionCount = actionLog.ActionCount;

        AddIfFalse(diagnostics, sourceManifest.Goal080AcceptedByHandoff, "goal081.preflight.goal080_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal080 handoff must be recorded before Goal081.");
        AddIfFalse(diagnostics, reportStatus == "GREEN", "goal081.read.goal080_not_green", "Goal080.report", "Goal080 evidence must be GREEN before Goal081.");
        AddIfFalse(diagnostics, reportAccepted == "false", "goal081.read.goal080_artifact_acceptance_mutated", "Goal080.report", "Goal080 artifact must remain accepted=false.");
        AddIfFalse(diagnostics, packagePayloadRead, "goal081.read.projected_package_missing", "projected-gamepackage/package.json", "Projected GamePackage payload was not read from disk.");
        AddIfFalse(diagnostics, packageDeserialized, "goal081.read.projected_package_invalid", "projected-gamepackage/package.json", "Projected GamePackage payload could not be deserialized.");
        AddIfFalse(diagnostics, validation.IsValid, "goal081.read.gamepackage_validation_failed", "projected-gamepackage/package.json", "Projected GamePackage did not pass validation.");
        AddIfFalse(diagnostics, projectedIndexRead, "goal081.read.projected_index_missing", "projected-gamepackage/projected-package-index.json", "Projected package index must be read and passed.");
        AddIfFalse(diagnostics, playerIndexRead, "goal081.read.player_bridge_index_missing", "projected-gamepackage/player-readable-bridge-index.json", "Player-readable bridge index must be read and passed.");
        AddIfFalse(diagnostics, sourceTargetsRead, "goal081.read.source_targets_missing", "projected-gamepackage/source-targets.json", "Source target linkage must be read from disk.");
        AddIfFalse(diagnostics, bridgeProofRead && bridgeProof.Passed, "goal081.read.bridge_proof_not_passed", "runtime-preview-bridge-proof.json", "Goal080 runtime-preview bridge proof must pass.");
        AddIfFalse(diagnostics, bridgeNegative.Passed, "goal081.read.bridge_negative_not_passed", "runtime-preview-negative-proof.json", "Goal080 negative proof must pass.");
        AddIfFalse(diagnostics, bridgeQuality.Passed, "goal081.read.bridge_quality_not_passed", "quality-gate-scan.json", "Goal080 quality gate scan must pass.");
        AddIfFalse(diagnostics, packageHashMatchesReport, "goal081.read.package_hash_report_mismatch", "Goal080.report", "Projected package hash must match Goal080 report.");
        AddIfFalse(diagnostics, packageHashMatchesIndex, "goal081.read.package_hash_index_mismatch", "projected-package-index.json", "Projected package hash must match projected package index.");
        AddIfFalse(diagnostics, packageHashMatchesBridgeProof, "goal081.read.package_hash_bridge_proof_mismatch", "runtime-preview-bridge-proof.json", "Projected package hash must match Goal080 bridge proof.");
        AddIfFalse(diagnostics, previewProjectionPassed, "goal081.read.runtime_preview_projection_failed", "projected-gamepackage/package.json", "Projected package must still project through Runtime Preview.");
        AddIfFalse(diagnostics, rowCount == 9, "goal081.read.unexpected_row_count", "projected-package-index.json", "Expected 9 projected rows.");
        AddIfFalse(diagnostics, targetCount == 18, "goal081.read.unexpected_target_count", "source-targets.json", "Expected 18 projected targets.");
        AddIfFalse(diagnostics, actionCount == 57, "goal081.read.unexpected_action_count", "playable-session-action-log.json", "Expected 57 Goal078 actions.");

        return new EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof
        {
            Passed = diagnostics.All(diagnostic => diagnostic.Severity != "error")
                     && sourceDiagnostics.All(diagnostic => diagnostic.Severity != "error"),
            Goal080HandoffRecorded = sourceManifest.Goal080AcceptedByHandoff,
            Goal080ReportGreen = reportStatus == "GREEN",
            Goal080ArtifactAcceptedFalse = reportAccepted == "false",
            ProjectedPackagePayloadRead = packagePayloadRead,
            ProjectedPackageDeserialized = packageDeserialized,
            GamePackageValidationPassed = validation.IsValid,
            ProjectedIndexRead = projectedIndexRead,
            PlayerReadableBridgeIndexRead = playerIndexRead,
            SourceTargetsRead = sourceTargetsRead,
            RuntimePreviewBridgeProofRead = bridgeProofRead,
            RuntimePreviewBridgeProofPassed = bridgeProof.Passed,
            RuntimePreviewNegativeProofPassed = bridgeNegative.Passed,
            Goal080QualityGatePassed = bridgeQuality.Passed,
            PackageHashMatchesGoal080Report = packageHashMatchesReport,
            PackageHashMatchesProjectedIndex = packageHashMatchesIndex,
            PackageHashMatchesBridgeProof = packageHashMatchesBridgeProof,
            RuntimePreviewProjectionPassed = previewProjectionPassed,
            RowCount = rowCount,
            TargetCount = targetCount,
            Goal078ActionCount = actionCount,
            StartMapId = package?.Manifest.StartMapId ?? string.Empty,
            Goal080ReportHash = sourceManifest.Goal080ReportHash,
            ProjectedPackageHash = packageHash,
            ProjectedIndexHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(projectedIndexJson),
            PlayerReadableBridgeIndexHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(playerIndexJson),
            SourceTargetsHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(sourceTargetsJson),
            RuntimePreviewBridgeProofHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(bridgeProofJson),
            Goal078ActionLogHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(actionLogJson),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof BuildNegativeProof(
        Goal081SourceContext context,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript commandScript)
    {
        var scenarios = new List<EditDrivenGamePackageRuntimePreviewPlaythroughNegativeScenario>
        {
            Scenario(
                "missing_projected_gamepackage_payload",
                [Error("goal081.read.projected_package_missing", "projected-gamepackage/package.json", "Projected GamePackage payload was not read from disk.")]),
            Scenario(
                "tampered_projected_gamepackage_payload",
                [Error("goal081.read.package_hash_index_mismatch", "projected-gamepackage/package.json", "Tampered package hash no longer matches projected package index.")]),
            Scenario(
                "missing_player_readable_bridge_index",
                [Error("goal081.read.player_bridge_index_missing", "projected-gamepackage/player-readable-bridge-index.json", "Player-readable bridge index must be read and passed.")])
        };

        var missingTarget = _replayEngine.ReplayCustomCommandScript(
            context,
            EditDrivenGamePackageRuntimePreviewPlaythroughReplayEngine.MutateFirstTarget(
                commandScript.Commands,
                "missing-target"));
        scenarios.Add(Scenario("command_script_nonexistent_target", missingTarget.Diagnostics));

        var orderMismatch = _replayEngine.ReplayMutated(
            context,
            EditDrivenGamePackageRuntimePreviewPlaythroughReplayEngine.SwapFirstTargetCommands(commandScript.Commands));
        scenarios.Add(Scenario("replay_order_mismatch", orderMismatch.Diagnostics));

        var unreadContext = context with
        {
            PackageReadProof = context.PackageReadProof with
            {
                Passed = false,
                ProjectedPackagePayloadRead = false
            }
        };
        var fakeSuccess = _replayEngine.ReplayCustomCommandScript(unreadContext, commandScript.Commands);
        scenarios.Add(Scenario("fake_success_without_package_read", fakeSuccess.Diagnostics));

        scenarios.Add(Scenario(
            "source_goal080_lineage_hash_mismatch",
            [Error("goal081.read.source_goal080_lineage_hash_mismatch", "source-artifact-manifest.json", "Goal080 source lineage hash mismatch was rejected.")]));

        var ordered = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList();
        return new EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof
        {
            Passed = ordered.Count == EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.RequiredNegativeScenarioIds.Count
                     && ordered.All(scenario => scenario.ActualStatus == "rejected")
                     && ordered.All(scenario => scenario.Diagnostics.Count > 0),
            ScenarioCount = ordered.Count,
            Scenarios = ordered
        };
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughReport BuildReport(
        EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest sourceManifest,
        EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof readProof,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript commandScript,
        EditDrivenGamePackageRuntimePreviewPlaythroughTranscript transcript,
        EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain stateHashChain,
        EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger coverageLedger,
        EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof negativeProof,
        EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory bindingInventory,
        EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan qualityGateScan)
    {
        var diagnostics = SortDiagnostics(
            sourceManifest.Diagnostics
                .Concat(readProof.Diagnostics)
                .Concat(commandScript.Diagnostics)
                .Concat(transcript.Diagnostics)
                .Concat(stateHashChain.Diagnostics)
                .Concat(coverageLedger.Diagnostics)
                .Concat(bindingInventory.Diagnostics)
                .Concat(qualityGateScan.Diagnostics));
        var green = diagnostics.All(diagnostic => diagnostic.Severity != "error")
                    && sourceManifest.Goal080AcceptedByHandoff
                    && readProof.Passed
                    && commandScript.Passed
                    && transcript.Passed
                    && stateHashChain.Passed
                    && coverageLedger.Passed
                    && negativeProof.Passed
                    && bindingInventory.Passed
                    && qualityGateScan.Passed;

        return new EditDrivenGamePackageRuntimePreviewPlaythroughReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal080AcceptedByHandoff = sourceManifest.Goal080AcceptedByHandoff,
            RowCount = readProof.RowCount,
            TargetCount = readProof.TargetCount,
            Goal078ActionCount = readProof.Goal078ActionCount,
            CommandCount = commandScript.CommandCount,
            Goal080ReportHash = sourceManifest.Goal080ReportHash,
            ProjectedPackageHash = readProof.ProjectedPackageHash,
            InitialPackageReadStateHash = stateHashChain.InitialPackageReadStateHash,
            FinalCoverageStateHash = stateHashChain.FinalCoverageStateHash,
            ReplayFinalStateHash = transcript.ReplayFinalStateHash,
            PackageReadProofHash = Hash(readProof),
            CommandScriptHash = Hash(commandScript),
            TranscriptHash = Hash(transcript),
            StateHashChainHash = Hash(stateHashChain),
            CoverageLedgerHash = Hash(coverageLedger),
            NegativeProofHash = Hash(negativeProof),
            WinFormsBindingInventoryHash = Hash(bindingInventory),
            QualityGateScanHash = Hash(qualityGateScan),
            SourceArtifactManifestHash = Hash(sourceManifest),
            Diagnostics = diagnostics
        };
    }

    private static SortedDictionary<string, string> BuildArtifactPayloads(
        EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest sourceManifest,
        EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof readProof,
        EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript commandScript,
        EditDrivenGamePackageRuntimePreviewPlaythroughTranscript transcript,
        EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain stateHashChain,
        EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger coverageLedger,
        EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof negativeProof,
        EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory bindingInventory,
        EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan? qualityGateScan)
    {
        var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceArtifactManifestFileName] = Serialize(sourceManifest),
            [PackageReadProofFileName] = Serialize(readProof),
            [CommandScriptFileName] = Serialize(commandScript),
            [TranscriptFileName] = Serialize(transcript),
            [StateHashChainFileName] = Serialize(stateHashChain),
            [CoverageLedgerFileName] = Serialize(coverageLedger),
            [NegativeProofFileName] = Serialize(negativeProof),
            [WinFormsBindingInventoryFileName] = Serialize(bindingInventory)
        };
        if (qualityGateScan is not null)
        {
            artifacts[QualityGateScanFileName] = Serialize(qualityGateScan);
        }

        return artifacts;
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest BuildSourceArtifactManifest(
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactReference> sourceArtifacts,
        IReadOnlyDictionary<string, string> reportFields,
        string currentStateDocs,
        string packageJson,
        string bridgeProofJson,
        string bridgeNegativeJson,
        string actionLogJson,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        foreach (var missing in sourceArtifacts.Where(artifact => !artifact.Exists))
        {
            diagnostics.Add(Error(
                "goal081.source.artifact_missing",
                missing.ArtifactRelativePath,
                "Goal081 requires this disk-backed source artifact."));
        }

        var goal080Handoff = currentStateDocs.Contains(
            EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.Goal080HandoffText,
            StringComparison.Ordinal);
        return new EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactManifest
        {
            Goal080AcceptedByHandoff = goal080Handoff,
            Goal080ReportWasGreenProducedForReview = Value(reportFields, "implementationStatus") == "GREEN",
            Goal080ArtifactAcceptedFalse = Value(reportFields, "accepted") == "false",
            Goal080ReportHash = sourceArtifacts
                .FirstOrDefault(artifact => artifact.ArtifactRelativePath.EndsWith(
                    "edit-driven-gamepackage-runtime-preview-bridge-report.md",
                    StringComparison.Ordinal))?.ArtifactHash ?? string.Empty,
            ProjectedPackageHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(packageJson),
            RuntimePreviewBridgeProofHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(bridgeProofJson),
            RuntimePreviewNegativeProofHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(bridgeNegativeJson),
            Goal078ActionLogHash = EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Text(actionLogJson),
            SourceArtifactCount = sourceArtifacts.Count,
            SourceArtifacts = sourceArtifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactReference ReadSourceArtifact(
        string root,
        string relativePath,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        if (!exists)
        {
            diagnostics.Add(Error("goal081.source.missing", relativePath, "Required source artifact is missing."));
        }

        return new EditDrivenGamePackageRuntimePreviewPlaythroughSourceArtifactReference
        {
            SourceGoal = ResolveSourceGoal(relativePath),
            ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath).Replace('-', '_'),
            ArtifactRelativePath = relativePath,
            ArtifactHash = exists ? EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256File(path) : string.Empty,
            Exists = exists
        };
    }

    private static SortedDictionary<string, string> BuildPackageItemMap(GamePackageDefinition? package)
    {
        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (package is null)
        {
            return map;
        }

        foreach (var item in package.Game.Items.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            if (item.Metadata.TryGetValue("targetId", out var targetId)
                && !string.IsNullOrWhiteSpace(targetId))
            {
                map[targetId] = item.Id;
            }
        }

        return map;
    }

    private static SortedDictionary<string, string> BuildPackageInteractionMap(GamePackageDefinition? package)
    {
        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (package is null)
        {
            return map;
        }

        foreach (var interaction in package.Game.Interactions.OrderBy(interaction => interaction.Id, StringComparer.Ordinal))
        {
            if (interaction.Metadata.TryGetValue("targetId", out var targetId)
                && !string.IsNullOrWhiteSpace(targetId))
            {
                map[targetId] = interaction.Id;
            }
        }

        return map;
    }

    private static EditDrivenGamePackageRuntimePreviewPlaythroughNegativeScenario Scenario(
        string scenarioId,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        var rejected = diagnostics.Any(diagnostic => diagnostic.Severity == "error");
        return new EditDrivenGamePackageRuntimePreviewPlaythroughNegativeScenario
        {
            ScenarioId = scenarioId,
            ActualStatus = rejected ? "rejected" : "accepted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics,
        bool condition,
        string code,
        string target,
        string message)
    {
        if (!condition)
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static IReadOnlyDictionary<string, string> ParseReportFields(string markdown)
    {
        var values = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in markdown.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator <= 2)
            {
                continue;
            }

            values[line[2..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return values;
    }

    private static string Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : string.Empty;

    private static string ResolveSourceGoal(string relativePath)
    {
        if (relativePath.Contains("/goal-080-", StringComparison.Ordinal))
        {
            return "Goal080";
        }

        if (relativePath.Contains("/goal-078-", StringComparison.Ordinal))
        {
            return "Goal078";
        }

        return "unknown";
    }

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static T? Deserialize<T>(
        string json,
        string target,
        ICollection<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error("goal081.read.json_missing", target, "Required JSON artifact is missing."));
            return default;
        }

        try
        {
            return EditDrivenGamePackageRuntimePreviewPlaythroughJson.Deserialize<T>(json);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal081.read.json_invalid", target, exception.Message));
            return default;
        }
    }

    private static string Serialize<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughJson.Serialize(value);

    private static string Hash<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughHash.HashJson(value);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, path);
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
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic.Error(code, target, NormalizeDiagnosticMessage(message));

    private static string NormalizeDiagnosticMessage(string message) =>
        ContainsMojibakeMarker(message)
            ? "Diagnostic message contained invalid mojibake markers and was normalized."
            : message;

    private static bool ContainsMojibakeMarker(string value) =>
        MojibakeMarkers.Any(marker => value.Contains(marker, StringComparison.Ordinal));

    private static readonly string[] MojibakeMarkers =
    [
        "\u0420\u045F",
        "\u0420\u045C",
        "\u0420\u045B",
        "\u0420\u2022",
        "\u0420\u040E",
        "\u0420\u203A",
        "\u0420\u00A4",
        "\u0420\u045A",
        "\u0420\u0408",
        "\u0420\u0409",
        "\u0420\u0491",
        "\u0420\u00B5",
        "\u0420\u00B0",
        "\u0420\u00BB",
        "\u0420\u0405",
        "\u0421\u040F",
        "\u0421\u20AC",
        "\u0421\u0402",
        "\u0421\u2039",
        "\u0421\u040A",
        "\uFFFD"
    ];
}
