using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService
{
    public const string ReportMarkdownFileName =
        "edit-driven-unity-alpha-streamingassets-handoff-report.md";
    public const string HandoffManifestFileName = "unity-streamingassets-handoff-manifest.json";
    public const string FileLedgerFileName = "unity-streamingassets-file-ledger.json";
    public const string ProbeReadProofFileName = "unity-probe-read-proof.json";
    public const string NegativeProofFileName = "unity-probe-negative-proof.json";
    public const string CommandTranscriptProofFileName = "unity-probe-command-transcript-proof.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly EditDrivenUnityAlphaStreamingAssetsHandoffPayloadBuilder _payloadBuilder = new();
    private readonly EditDrivenUnityAlphaStreamingAssetsHandoffReadValidator _readValidator = new();
    private readonly EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner _qualityScanner = new();

    public EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payloadFiles = ReadPayloadFiles(root);
        if (payloadFiles.Count == 0)
        {
            payloadFiles = _payloadBuilder.BuildPayloadFiles(context, "GREEN");
        }

        return BuildResult(root, context, payloadFiles);
    }

    public async Task<EditDrivenUnityAlphaStreamingAssetsHandoffWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payloadFiles = _payloadBuilder.BuildPayloadFiles(context, "GREEN");
        var streamingAssetsDirectoryPath = Resolve(
            root,
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot);
        ResetDirectory(root, streamingAssetsDirectoryPath);

        var written = new List<string>();
        foreach (var payload in payloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(streamingAssetsDirectoryPath, Normalize(payload.Key));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await File.WriteAllTextAsync(targetPath, payload.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, targetPath));
        }

        var result = BuildResult(root, context, ReadPayloadFiles(root));
        var outputDirectoryPath = Resolve(
            root,
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectoryPath);

        foreach (var artifact in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(outputDirectoryPath, artifact.Key);
            await File.WriteAllTextAsync(targetPath, artifact.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, targetPath));
        }

        var reportPath = Path.Combine(outputDirectoryPath, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new EditDrivenUnityAlphaStreamingAssetsHandoffWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectoryPath,
            StreamingAssetsDirectoryPath = streamingAssetsDirectoryPath,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
        EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredArtifactFileNames;

    private EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult BuildResult(
        string root,
        Goal082SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var fileLedger = EditDrivenUnityAlphaStreamingAssetsHandoffPayloadBuilder.BuildFileLedger(payloadFiles);
        var readProof = _readValidator.ValidateMirroredPayload(root, context);
        if (readProof.PayloadFileCount == 0)
        {
            readProof = _readValidator.ValidatePayloadFiles(root, context, payloadFiles, payloadReadAttempted: true);
        }

        var negativeProof = _readValidator.BuildNegativeProof(root, context, payloadFiles);
        var commandTranscriptProof = BuildCommandTranscriptProof(context);
        var bindingInventory = EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.BuildWinFormsBindingInventory(root);
        var preQualityArtifacts = BuildArtifactPayloads(
            context.SourceArtifactManifest,
            ReadManifest(payloadFiles),
            fileLedger,
            readProof,
            negativeProof,
            commandTranscriptProof,
            bindingInventory,
            qualityGateScan: null);
        var qualityGate = _qualityScanner.Scan(root, bindingInventory, preQualityArtifacts, payloadFiles);
        var reportWithoutHash = BuildReport(
            context,
            ReadManifest(payloadFiles),
            fileLedger,
            readProof,
            negativeProof,
            commandTranscriptProof,
            bindingInventory,
            qualityGate);
        var report = reportWithoutHash with { DeterministicHash = Hash(reportWithoutHash) };
        var artifacts = BuildArtifactPayloads(
            context.SourceArtifactManifest,
            ReadManifest(payloadFiles),
            fileLedger,
            readProof,
            negativeProof,
            commandTranscriptProof,
            bindingInventory,
            qualityGate);

        return new EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult
        {
            SourceArtifactManifest = context.SourceArtifactManifest,
            HandoffManifest = ReadManifest(payloadFiles),
            FileLedger = fileLedger,
            ProbeReadProof = readProof,
            NegativeProof = negativeProof,
            CommandTranscriptProof = commandTranscriptProof,
            WinFormsBindingInventory = bindingInventory,
            QualityGateScan = qualityGate,
            Report = report,
            ReportMarkdown = EditDrivenUnityAlphaStreamingAssetsHandoffReportRenderer.Render(report),
            PayloadJsonByFileName = new SortedDictionary<string, string>(
                payloadFiles.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
                StringComparer.Ordinal),
            ArtifactJsonByFileName = artifacts
        };
    }

    private Goal082SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        var sourceArtifacts = EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredSourceArtifactRelativePaths
            .Select(path => ReadSourceArtifact(root, path, diagnostics))
            .OrderBy(artifact => artifact.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        var goal080Report = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal080RelativeOutputDirectory
                                               + "/edit-driven-gamepackage-runtime-preview-bridge-report.md");
        var goal081Report = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                               + "/edit-driven-gamepackage-runtime-preview-playthrough-report.md");
        var goal080PackageJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal080RelativeOutputDirectory
                                                    + "/projected-gamepackage/package.json");
        var goal080ProjectedIndexJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal080RelativeOutputDirectory
                                                           + "/projected-gamepackage/projected-package-index.json");
        var goal080ValidationReportJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal080RelativeOutputDirectory
                                                             + "/projected-gamepackage/validation-report.json");
        var goal080BridgeProofJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal080RelativeOutputDirectory
                                                        + "/runtime-preview-bridge-proof.json");
        var goal081PackageReadProofJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                             + "/package-read-proof.json");
        var goal081CommandScriptJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                          + "/playthrough-command-script.json");
        var goal081TranscriptJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                       + "/playthrough-transcript.json");
        var goal081StateHashChainJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                           + "/playthrough-state-hash-chain.json");
        var goal081CoverageLedgerJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                           + "/playthrough-coverage-ledger.json");
        var goal081NegativeProofJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                          + "/playthrough-negative-proof.json");
        var goal081QualityGateJson = ReadOptional(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081RelativeOutputDirectory
                                                        + "/quality-gate-scan.json");

        var goal080Fields = ParseReportFields(goal080Report);
        var goal081Fields = ParseReportFields(goal081Report);
        var currentStateDocs = ReadOptional(root, "docs/CURRENT_GENERATOR_STATE.md")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/CURRENT_GENERATOR_STATE.json")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/CONTEXT_INDEX.md")
                               + Environment.NewLine
                               + ReadOptional(root, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var goal080BridgeProof = Deserialize<EditDrivenGamePackageRuntimePreviewBridgeProof>(
            goal080BridgeProofJson,
            "runtime-preview-bridge-proof.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewBridgeProof();
        var packageReadProof = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof>(
            goal081PackageReadProofJson,
            "package-read-proof.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof();
        var commandScript = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript>(
            goal081CommandScriptJson,
            "playthrough-command-script.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript();
        var transcript = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughTranscript>(
            goal081TranscriptJson,
            "playthrough-transcript.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughTranscript();
        var stateHashChain = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain>(
            goal081StateHashChainJson,
            "playthrough-state-hash-chain.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain();
        var coverageLedger = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger>(
            goal081CoverageLedgerJson,
            "playthrough-coverage-ledger.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger();
        var negativeProof = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof>(
            goal081NegativeProofJson,
            "playthrough-negative-proof.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof();
        var qualityGate = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan>(
            goal081QualityGateJson,
            "quality-gate-scan.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan();

        var goal081AcceptedByHandoff = currentStateDocs.Contains(
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.Goal081HandoffText,
            StringComparison.Ordinal);
        var goal080Green = Value(goal080Fields, "implementationStatus") == "GREEN";
        var goal080AcceptedFalse = Value(goal080Fields, "accepted") == "false";
        var goal081Green = Value(goal081Fields, "implementationStatus") == "GREEN";
        var goal081AcceptedFalse = Value(goal081Fields, "accepted") == "false";
        var projectedPackageHash = HashText(goal080PackageJson);
        var commandScriptHash = HashText(goal081CommandScriptJson);
        var transcriptHash = HashText(goal081TranscriptJson);
        var stateHashChainHash = HashText(goal081StateHashChainJson);
        var coverageLedgerHash = HashText(goal081CoverageLedgerJson);
        var finalCoverageStateHash = stateHashChain.FinalCoverageStateHash;
        var replayFinalStateHash = transcript.ReplayFinalStateHash;

        AddIfFalse(diagnostics, goal081AcceptedByHandoff, "goal082.preflight.goal081_handoff_missing", "docs/CURRENT_GENERATOR_STATE.*", "Goal081 handoff must be recorded before Goal082.");
        AddIfFalse(diagnostics, goal080Green, "goal082.source.goal080_not_green", "Goal080.report", "Goal080 must be GREEN produced-for-review source evidence.");
        AddIfFalse(diagnostics, goal080AcceptedFalse, "goal082.source.goal080_accepted_mutated", "Goal080.report", "Goal080 artifact must remain accepted=false.");
        AddIfFalse(diagnostics, goal081Green, "goal082.source.goal081_not_green", "Goal081.report", "Goal081 must be GREEN produced-for-review source evidence.");
        AddIfFalse(diagnostics, goal081AcceptedFalse, "goal082.source.goal081_accepted_mutated", "Goal081.report", "Goal081 artifact must remain accepted=false.");
        AddIfFalse(diagnostics, goal080BridgeProof.Passed, "goal082.source.goal080_bridge_proof_not_passed", "runtime-preview-bridge-proof.json", "Goal080 runtime-preview bridge proof must pass.");
        AddIfFalse(diagnostics, packageReadProof.Passed, "goal082.source.goal081_package_read_not_passed", "package-read-proof.json", "Goal081 package read proof must pass.");
        AddIfFalse(diagnostics, commandScript.Passed, "goal082.source.goal081_command_script_not_passed", "playthrough-command-script.json", "Goal081 command script must pass.");
        AddIfFalse(diagnostics, transcript.Passed, "goal082.source.goal081_transcript_not_passed", "playthrough-transcript.json", "Goal081 transcript must pass.");
        AddIfFalse(diagnostics, stateHashChain.Passed, "goal082.source.goal081_state_chain_not_passed", "playthrough-state-hash-chain.json", "Goal081 state hash chain must pass.");
        AddIfFalse(diagnostics, coverageLedger.Passed, "goal082.source.goal081_coverage_not_passed", "playthrough-coverage-ledger.json", "Goal081 coverage ledger must pass.");
        AddIfFalse(diagnostics, negativeProof.Passed, "goal082.source.goal081_negative_not_passed", "playthrough-negative-proof.json", "Goal081 negative proof must pass.");
        AddIfFalse(diagnostics, qualityGate.Passed, "goal082.source.goal081_quality_not_passed", "quality-gate-scan.json", "Goal081 quality gate must pass.");
        AddIfFalse(diagnostics, projectedPackageHash == packageReadProof.ProjectedPackageHash, "goal082.source.projected_package_hash_mismatch", "projected-gamepackage/package.json", "Goal080 package hash must match Goal081 package read proof.");
        AddIfFalse(diagnostics, commandScriptHash == Value(goal081Fields, "commandScriptHash"), "goal082.source.command_hash_mismatch", "Goal081.report", "Goal081 command script hash must match report.");
        AddIfFalse(diagnostics, transcriptHash == Value(goal081Fields, "transcriptHash"), "goal082.source.transcript_hash_mismatch", "Goal081.report", "Goal081 transcript hash must match report.");
        AddIfFalse(diagnostics, stateHashChainHash == Value(goal081Fields, "stateHashChainHash"), "goal082.source.state_chain_hash_mismatch", "Goal081.report", "Goal081 state hash chain hash must match report.");
        AddIfFalse(diagnostics, packageReadProof.RowCount == 9 && packageReadProof.TargetCount == 18, "goal082.source.package_counts_unexpected", "package-read-proof.json", "Goal082 expects Goal080/081 rows=9 and targets=18.");
        AddIfFalse(diagnostics, commandScript.CommandCount == 124 && commandScript.Goal078ActionCount == 57, "goal082.source.command_counts_unexpected", "playthrough-command-script.json", "Goal082 expects Goal081 commandCount=124 and actionCount=57.");

        var sourceManifest = new EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactManifest
        {
            Goal081AcceptedByHandoff = goal081AcceptedByHandoff,
            Goal080ReportWasGreenProducedForReview = goal080Green,
            Goal080ArtifactAcceptedFalse = goal080AcceptedFalse,
            Goal081ReportWasGreenProducedForReview = goal081Green,
            Goal081ArtifactAcceptedFalse = goal081AcceptedFalse,
            Goal080ReportHash = ArtifactHash(sourceArtifacts, "edit-driven-gamepackage-runtime-preview-bridge-report.md"),
            Goal080ProjectedPackageHash = projectedPackageHash,
            Goal080ProjectedPackageIndexHash = HashText(goal080ProjectedIndexJson),
            Goal080ValidationReportHash = HashText(goal080ValidationReportJson),
            Goal080RuntimePreviewBridgeProofHash = HashText(goal080BridgeProofJson),
            Goal081ReportHash = ArtifactHash(sourceArtifacts, "edit-driven-gamepackage-runtime-preview-playthrough-report.md"),
            Goal081PackageReadProofHash = HashText(goal081PackageReadProofJson),
            Goal081CommandScriptHash = commandScriptHash,
            Goal081TranscriptHash = transcriptHash,
            Goal081StateHashChainHash = stateHashChainHash,
            Goal081CoverageLedgerHash = coverageLedgerHash,
            Goal081NegativeProofHash = HashText(goal081NegativeProofJson),
            Goal081QualityGateScanHash = HashText(goal081QualityGateJson),
            SourceArtifactCount = sourceArtifacts.Count,
            SourceArtifacts = sourceArtifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return new Goal082SourceContext
        {
            RootPath = root,
            SourceArtifactManifest = sourceManifest,
            Goal080ReportMarkdown = goal080Report,
            Goal081ReportMarkdown = goal081Report,
            Goal080PackageJson = goal080PackageJson,
            Goal080ProjectedIndexJson = goal080ProjectedIndexJson,
            Goal080ValidationReportJson = goal080ValidationReportJson,
            Goal080RuntimePreviewBridgeProofJson = goal080BridgeProofJson,
            Goal081PackageReadProofJson = goal081PackageReadProofJson,
            Goal081CommandScriptJson = goal081CommandScriptJson,
            Goal081TranscriptJson = goal081TranscriptJson,
            Goal081StateHashChainJson = goal081StateHashChainJson,
            Goal081CoverageLedgerJson = goal081CoverageLedgerJson,
            Goal081NegativeProofJson = goal081NegativeProofJson,
            Goal081QualityGateJson = goal081QualityGateJson,
            RowCount = packageReadProof.RowCount,
            TargetCount = packageReadProof.TargetCount,
            Goal078ActionCount = commandScript.Goal078ActionCount,
            CommandCount = commandScript.CommandCount,
            ProjectedPackageHash = projectedPackageHash,
            CommandScriptHash = commandScriptHash,
            TranscriptHash = transcriptHash,
            StateHashChainHash = stateHashChainHash,
            CoverageLedgerHash = coverageLedgerHash,
            FinalCoverageStateHash = finalCoverageStateHash,
            ReplayFinalStateHash = replayFinalStateHash,
            CommandTypeCounts = commandScript.Commands
                .GroupBy(command => command.CommandType, StringComparer.Ordinal)
                .Select(group => new EditDrivenUnityAlphaStreamingAssetsHandoffCommandTypeCount
                {
                    CommandType = group.Key,
                    Count = group.Count()
                })
                .OrderBy(item => item.CommandType, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof BuildCommandTranscriptProof(
        Goal082SourceContext context)
    {
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        var commandScript = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript>(
            context.Goal081CommandScriptJson,
            "playthrough-command-script.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript();
        var transcript = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughTranscript>(
            context.Goal081TranscriptJson,
            "playthrough-transcript.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughTranscript();
        var stateHashChain = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain>(
            context.Goal081StateHashChainJson,
            "playthrough-state-hash-chain.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughStateHashChain();
        var coverage = Deserialize<EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger>(
            context.Goal081CoverageLedgerJson,
            "playthrough-coverage-ledger.json",
            diagnostics) ?? new EditDrivenGamePackageRuntimePreviewPlaythroughCoverageLedger();
        var commandCountMatches = commandScript.CommandCount == transcript.CommandCount
                                  && commandScript.CommandCount == context.CommandCount;
        var coverageCountsMatch = coverage.CoveredRowCount == context.RowCount
                                  && coverage.CoveredTargetCount == context.TargetCount
                                  && coverage.CoveredGoal078ActionCount == context.Goal078ActionCount;

        AddIfFalse(diagnostics, commandCountMatches, "goal082.command_transcript.command_count_mismatch", "playthrough-transcript.json", "Command count must match the transcript.");
        AddIfFalse(diagnostics, coverageCountsMatch, "goal082.command_transcript.coverage_mismatch", "playthrough-coverage-ledger.json", "Coverage counts must match Goal081 expectations.");
        AddIfFalse(diagnostics, stateHashChain.FinalCoverageStateHash == context.FinalCoverageStateHash, "goal082.command_transcript.state_hash_mismatch", "playthrough-state-hash-chain.json", "Final coverage state hash must match source context.");

        return new EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof
        {
            Passed = diagnostics.Count == 0
                     && commandScript.Passed
                     && transcript.Passed
                     && stateHashChain.Passed
                     && coverage.Passed
                     && commandCountMatches
                     && coverageCountsMatch,
            CommandScriptRead = commandScript.Passed,
            TranscriptRead = transcript.Passed,
            StateHashChainRead = stateHashChain.Passed,
            CoverageLedgerRead = coverage.Passed,
            CommandCountMatchesTranscript = commandCountMatches,
            CoverageCountsMatch = coverageCountsMatch,
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            Goal078ActionCount = context.Goal078ActionCount,
            CommandCount = context.CommandCount,
            CommandScriptHash = context.CommandScriptHash,
            TranscriptHash = context.TranscriptHash,
            StateHashChainHash = context.StateHashChainHash,
            CoverageLedgerHash = context.CoverageLedgerHash,
            FinalCoverageStateHash = context.FinalCoverageStateHash,
            ReplayFinalStateHash = context.ReplayFinalStateHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffReport BuildReport(
        Goal082SourceContext context,
        EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest handoffManifest,
        EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger fileLedger,
        EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof readProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof negativeProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof commandTranscriptProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory bindingInventory,
        EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan qualityGate)
    {
        var diagnostics = SortDiagnostics(
            context.Diagnostics
                .Concat(context.SourceArtifactManifest.Diagnostics)
                .Concat(fileLedger.Diagnostics)
                .Concat(readProof.Diagnostics)
                .Concat(commandTranscriptProof.Diagnostics)
                .Concat(bindingInventory.Diagnostics)
                .Concat(qualityGate.Diagnostics));
        var green = diagnostics.All(diagnostic => diagnostic.Severity != "error")
                    && context.SourceArtifactManifest.Goal081AcceptedByHandoff
                    && fileLedger.Passed
                    && readProof.Passed
                    && negativeProof.Passed
                    && commandTranscriptProof.Passed
                    && bindingInventory.Passed
                    && qualityGate.Passed;

        return new EditDrivenUnityAlphaStreamingAssetsHandoffReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal081AcceptedByHandoff = context.SourceArtifactManifest.Goal081AcceptedByHandoff,
            PayloadFileCount = fileLedger.FileCount,
            RowCount = context.RowCount,
            TargetCount = context.TargetCount,
            Goal078ActionCount = context.Goal078ActionCount,
            CommandCount = context.CommandCount,
            ProjectedPackageHash = context.ProjectedPackageHash,
            CommandScriptHash = context.CommandScriptHash,
            TranscriptHash = context.TranscriptHash,
            StateHashChainHash = context.StateHashChainHash,
            FinalCoverageStateHash = context.FinalCoverageStateHash,
            ReplayFinalStateHash = context.ReplayFinalStateHash,
            HandoffManifestHash = Hash(handoffManifest),
            FileLedgerHash = Hash(fileLedger),
            ProbeReadProofHash = Hash(readProof),
            NegativeProofHash = Hash(negativeProof),
            CommandTranscriptProofHash = Hash(commandTranscriptProof),
            WinFormsBindingInventoryHash = Hash(bindingInventory),
            QualityGateScanHash = Hash(qualityGate),
            SourceArtifactManifestHash = Hash(context.SourceArtifactManifest),
            Diagnostics = diagnostics
        };
    }

    private static SortedDictionary<string, string> BuildArtifactPayloads(
        EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactManifest sourceManifest,
        EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest handoffManifest,
        EditDrivenUnityAlphaStreamingAssetsHandoffFileLedger fileLedger,
        EditDrivenUnityAlphaStreamingAssetsHandoffProbeReadProof readProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffNegativeProof negativeProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffCommandTranscriptProof commandTranscriptProof,
        EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory bindingInventory,
        EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan? qualityGateScan)
    {
        var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceArtifactManifestFileName] = Serialize(sourceManifest),
            [HandoffManifestFileName] = Serialize(handoffManifest),
            [FileLedgerFileName] = Serialize(fileLedger),
            [ProbeReadProofFileName] = Serialize(readProof),
            [NegativeProofFileName] = Serialize(negativeProof),
            [CommandTranscriptProofFileName] = Serialize(commandTranscriptProof),
            [WinFormsBindingInventoryFileName] = Serialize(bindingInventory)
        };
        if (qualityGateScan is not null)
        {
            artifacts[QualityGateScanFileName] = Serialize(qualityGateScan);
        }

        return artifacts;
    }

    private static SortedDictionary<string, string> ReadPayloadFiles(string root)
    {
        var payloadRoot = Resolve(root, EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot);
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(payloadRoot))
        {
            return payloads;
        }

        foreach (var fileName in EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.RequiredUnityPayloadFileNames)
        {
            var path = Path.Combine(payloadRoot, Normalize(fileName));
            if (File.Exists(path))
            {
                payloads[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return payloads;
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest ReadManifest(
        IReadOnlyDictionary<string, string> payloadFiles) =>
        payloadFiles.TryGetValue("handoff-manifest.json", out var json)
            ? EditDrivenUnityAlphaStreamingAssetsHandoffJson.Deserialize<EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest>(json)
              ?? new EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest()
            : new EditDrivenUnityAlphaStreamingAssetsHandoffPayloadManifest();

    private static EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactReference ReadSourceArtifact(
        string root,
        string relativePath,
        ICollection<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        if (!exists)
        {
            diagnostics.Add(Error("goal082.source.artifact_missing", relativePath, "Required source artifact is missing."));
        }

        var file = exists ? new FileInfo(path) : null;
        return new EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactReference
        {
            SourceGoal = ResolveSourceGoal(relativePath),
            ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath).Replace('-', '_'),
            ArtifactRelativePath = relativePath,
            ArtifactHash = exists ? EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256File(path) : string.Empty,
            ByteCount = file?.Length ?? 0,
            Exists = exists
        };
    }

    private static T? Deserialize<T>(
        string json,
        string target,
        ICollection<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(Error("goal082.source.json_missing", target, "Required JSON artifact is missing."));
            return default;
        }

        try
        {
            return EditDrivenUnityAlphaStreamingAssetsHandoffJson.Deserialize<T>(json);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal082.source.json_invalid", target, exception.Message));
            return default;
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

    private static string ArtifactHash(
        IEnumerable<EditDrivenUnityAlphaStreamingAssetsHandoffSourceArtifactReference> artifacts,
        string fileName) =>
        artifacts.FirstOrDefault(artifact => artifact.ArtifactRelativePath.EndsWith(fileName, StringComparison.Ordinal))?.ArtifactHash
        ?? string.Empty;

    private static string Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : string.Empty;

    private static string ResolveSourceGoal(string relativePath)
    {
        if (relativePath.Contains("/goal-080-", StringComparison.Ordinal))
        {
            return "Goal080";
        }

        if (relativePath.Contains("/goal-081-", StringComparison.Ordinal))
        {
            return "Goal081";
        }

        return "unknown";
    }

    private static void AddIfFalse(
        ICollection<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics,
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

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, Normalize(relativePath)));
        EnsureContained(root, path);
        return path;
    }

    private static void ResetDirectory(string root, string path)
    {
        EnsureContained(root, path);
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

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string Serialize<T>(T value) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffJson.Serialize(value);

    private static string HashText(string text) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256Text(text);

    private static string Hash<T>(T value) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffHash.HashJson(value);

    private static IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.SortDiagnostics(diagnostics);

    private static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic.Error(code, target, NormalizeDiagnosticMessage(message));

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
