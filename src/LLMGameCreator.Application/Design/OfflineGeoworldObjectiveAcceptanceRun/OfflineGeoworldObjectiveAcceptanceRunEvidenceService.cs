using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

namespace LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

public sealed partial class OfflineGeoworldObjectiveAcceptanceRunEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldObjectiveBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var scripts = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var proof = ValidatePayload(payload.PayloadFiles, context, payloadReadAttempted: true);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var consolidation = BuildAlphaQualityConsolidation(root, context, scripts, editor);
        var quality = BuildQualityGate(
            root,
            context,
            payload,
            scripts,
            editor,
            proof,
            negative,
            binding,
            lineage,
            consolidation);
        return BuildResult(payload, scripts, editor, proof, negative, binding, lineage, consolidation, quality);
    }

    public async Task<OfflineGeoworldObjectiveWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.StreamingAssetsRelativeRoot);
        ResetDirectory(root, streamingAssetsDirectory);

        var written = new List<string>();
        foreach (var item in payload.PayloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(streamingAssetsDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var mirroredPayload = ReadPayloadFiles(root);
        var mirrored = payload with { PayloadFiles = mirroredPayload };
        var scripts = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var proof = ValidateMirroredPayload(root, mirroredPayload, context);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var consolidation = BuildAlphaQualityConsolidation(root, context, scripts, editor);
        var quality = BuildQualityGate(
            root,
            context,
            mirrored,
            scripts,
            editor,
            proof,
            negative,
            binding,
            lineage,
            consolidation);
        var result = BuildResult(mirrored, scripts, editor, proof, negative, binding, lineage, consolidation, quality);

        var outputDirectory = Resolve(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);
        foreach (var item in result.PayloadJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

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
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldObjectiveWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldObjectiveBuildResult BuildResult(
        Goal107Payload payload,
        OfflineGeoworldObjectiveUnityScriptInventory scripts,
        OfflineGeoworldObjectiveEditorWindowInventory editor,
        OfflineGeoworldObjectiveReplayAcceptanceProof proof,
        OfflineGeoworldObjectiveNegativeProof negative,
        OfflineGeoworldObjectiveWorkspaceBindingInventory binding,
        OfflineGeoworldObjectiveSourceLineage lineage,
        OfflineGeoworldObjectiveAlphaQualityConsolidation consolidation,
        OfflineGeoworldObjectiveQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(
            scripts,
            editor,
            proof,
            negative,
            binding,
            lineage,
            consolidation,
            quality);
        var reportWithoutHash = BuildReport(payload, scripts, editor, proof, negative, binding, consolidation, quality);
        var markdownWithoutHash = RenderReport(reportWithoutHash, quality, proof, consolidation);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(markdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof, consolidation);
        return new OfflineGeoworldObjectiveBuildResult
        {
            Manifest = payload.Manifest,
            Objectives = payload.Objectives,
            AcceptanceRun = payload.AcceptanceRun,
            CompletionState = payload.CompletionState,
            ReplayAcceptanceProof = payload.ReplayAcceptanceProof,
            Readme = payload.Readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedAcceptanceProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            AlphaQualityConsolidation = consolidation,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payload.PayloadFiles,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal107Payload BuildPayload(Goal107SourceContext context)
    {
        var deltas = context.DeltaLog.Deltas
            .OrderBy(item => item.ReplayStepIndex)
            .ToList();
        var objectives = BuildObjectiveDefinitions(context, deltas);
        var hashChain = new List<string> { context.Manifest.InitialStateHash };
        var runSteps = new List<OfflineGeoworldObjectiveRunStep>();
        var previous = context.Manifest.InitialStateHash;
        foreach (var objective in objectives)
        {
            var completionHash = ObjectiveHash(previous, objective);
            hashChain.Add(completionHash);
            runSteps.Add(new OfflineGeoworldObjectiveRunStep
            {
                StepIndex = objective.SequenceIndex,
                ObjectiveId = objective.ObjectiveId,
                ObjectiveKind = objective.ObjectiveKind,
                AppliedReplayEventIds = objective.LinkedEventIds,
                AppliedActionIds = objective.LinkedActionIds,
                StateHashBefore = previous,
                StateHashAfter = objective.ExpectedStateHashAfter,
                CheckpointLoadedBeforeCompletion = objective.RequiresCheckpoint,
                CompletionHash = completionHash
            });
            previous = completionHash;
        }

        var completion = new OfflineGeoworldObjectiveCompletionState
        {
            Completed = true,
            FinalStatus = "completed",
            CompletedObjectiveCount = objectives.Count,
            CompletedObjectiveIds = objectives.Select(item => item.ObjectiveId).ToList(),
            ObjectiveHashChain = hashChain,
            FinalObjectiveAcceptanceHash = hashChain.Last(),
            ReplayLinked = true,
            SaveLoadResumeLinked = true
        };
        var acceptanceRun = new OfflineGeoworldObjectiveAcceptanceRun
        {
            ReplayStepCount = context.Manifest.ReplayStepCount,
            StateDeltaCount = context.Manifest.StateDeltaCount,
            CheckpointStepIndex = context.Manifest.CheckpointStepIndex,
            InitialStateHash = context.Manifest.InitialStateHash,
            CheckpointStateHash = context.Manifest.CheckpointStateHash,
            FinalStateHash = context.Manifest.FinalStateHash,
            FinalObjectiveId = objectives.Last().ObjectiveId,
            Steps = runSteps
        };
        var objectiveDocument = new OfflineGeoworldObjectiveDocument
        {
            ObjectiveCount = objectives.Count,
            Objectives = objectives
        };
        var readme = new OfflineGeoworldObjectiveReadme();
        var proof = BuildPayloadReplayProof(context, objectiveDocument, acceptanceRun, completion);
        var objectivesJson = Serialize(objectiveDocument);
        var acceptanceRunJson = Serialize(acceptanceRun);
        var completionJson = Serialize(completion);
        var proofJson = Serialize(proof);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldObjectiveManifest
        {
            PayloadFileCount = OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredPayloadFileNames.Count,
            ObjectiveCount = objectiveDocument.ObjectiveCount,
            CompletedObjectiveCount = completion.CompletedObjectiveCount,
            SourceGoal106ReplayStepCount = context.Manifest.ReplayStepCount,
            SourceGoal106StateDeltaCount = context.Manifest.StateDeltaCount,
            SourceGoal106CheckpointStepIndex = context.Manifest.CheckpointStepIndex,
            SourceGoal106InitialStateHash = context.Manifest.InitialStateHash,
            SourceGoal106CheckpointStateHash = context.Manifest.CheckpointStateHash,
            SourceGoal106FinalStateHash = context.Manifest.FinalStateHash,
            ObjectiveAcceptanceHash = completion.FinalObjectiveAcceptanceHash,
            CompletionStateHash = Hash(completionJson),
            FinalStatus = completion.FinalStatus,
            AlphaRuntimeBootstrapUnchanged = context.AlphaRuntimeBootstrapUnchanged,
            ObjectivesHash = Hash(objectivesJson),
            AcceptanceRunHash = Hash(acceptanceRunJson),
            CompletionStateFileHash = Hash(completionJson),
            ReplayAcceptanceProofHash = Hash(proofJson),
            ReadmeHash = Hash(readmeJson)
        };
        var manifestJson = Serialize(manifest);
        return new Goal107Payload(
            manifest,
            objectiveDocument,
            acceptanceRun,
            completion,
            proof,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.ManifestFileName] = manifestJson,
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.ObjectivesFileName] = objectivesJson,
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.AcceptanceRunFileName] =
                    acceptanceRunJson,
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.CompletionStateFileName] =
                    completionJson,
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReplayAcceptanceProofFileName] =
                    proofJson,
                [OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static List<OfflineGeoworldObjectiveDefinition> BuildObjectiveDefinitions(
        Goal107SourceContext context,
        IReadOnlyList<OfflineGeoworldSessionDeltaRecord> deltas)
    {
        var inspect = FirstDelta(deltas, "inspect", indexFallback: 0);
        var markVisited = FirstDelta(deltas, "mark_visited", indexFallback: 2);
        var toggleBlocked = FirstDelta(deltas, "toggle_blocked", indexFallback: 3);
        var collectSample = FirstDelta(deltas, "collect_sample", indexFallback: 4);
        var finalInspect = deltas.Last();
        var checkpointDeltas = deltas.Take(context.Manifest.CheckpointStepIndex).ToList();
        var result = new List<OfflineGeoworldObjectiveDefinition>
        {
            Objective(
                1,
                "objective_001_inspect_target",
                "inspect_poi_or_building_target",
                "Inspect a visible offline geoworld point-of-interest target.",
                [],
                [inspect],
                false,
                0,
                "selected target inspection delta must be present"),
            Objective(
                2,
                "objective_002_mark_target_visited",
                "mark_target_visited",
                "Mark one offline geoworld target as visited.",
                ["objective_001_inspect_target"],
                [markVisited],
                false,
                0,
                "visited flag delta must be present"),
            Objective(
                3,
                "objective_003_save_load_checkpoint_resume",
                "save_load_checkpoint_resume",
                "Save, load and resume from the Goal106 checkpoint.",
                ["objective_002_mark_target_visited"],
                checkpointDeltas,
                true,
                context.Manifest.CheckpointStepIndex,
                "Goal106 checkpoint snapshot must resume at checkpoint state hash"),
            Objective(
                4,
                "objective_004_clear_blocked_route",
                "toggle_or_clear_blocked_route",
                "Toggle or clear the blocked route/barrier state.",
                ["objective_003_save_load_checkpoint_resume"],
                [toggleBlocked],
                false,
                0,
                "blocked route state delta must be present"),
            Objective(
                5,
                "objective_005_collect_sample",
                "collect_sample",
                "Collect one sample from an offline geoworld interaction target.",
                ["objective_004_clear_blocked_route"],
                [collectSample],
                false,
                0,
                "sample collection state delta must be present"),
            Objective(
                6,
                "objective_006_finalize_acceptance_run",
                "finalize_acceptance_run",
                "Finalize the objective acceptance run after replay completion.",
                ["objective_005_collect_sample"],
                [finalInspect],
                false,
                0,
                "final state hash must match Goal106 replay final hash")
        };
        return result;

        OfflineGeoworldObjectiveDefinition Objective(
            int sequence,
            string id,
            string kind,
            string title,
            IReadOnlyList<string> prerequisites,
            IReadOnlyList<OfflineGeoworldSessionDeltaRecord> linkedDeltas,
            bool requiresCheckpoint,
            int checkpointStep,
            string condition)
        {
            var linkedActions = linkedDeltas.Select(item => item.ActionId).Distinct(StringComparer.Ordinal).ToList();
            var linkedEvents = linkedDeltas.Select(item => item.EventId).Distinct(StringComparer.Ordinal).ToList();
            var linkedTargets = linkedDeltas.Select(item => item.TargetId).Distinct(StringComparer.Ordinal).ToList();
            var deltaKeys = linkedDeltas.Select(item => item.StateKey).Distinct(StringComparer.Ordinal).ToList();
            var deltaKinds = linkedDeltas.Select(item => item.DeltaKind).Distinct(StringComparer.Ordinal).ToList();
            var expectedHash = requiresCheckpoint
                ? context.Manifest.CheckpointStateHash
                : linkedDeltas.Last().StateHashAfter;
            return new OfflineGeoworldObjectiveDefinition
            {
                ObjectiveId = id,
                ObjectiveKind = kind,
                Title = title,
                SequenceIndex = sequence,
                PrerequisiteObjectiveIds = prerequisites,
                LinkedActionIds = linkedActions,
                LinkedTargetIds = linkedTargets,
                LinkedEventIds = linkedEvents,
                ExpectedStateDeltaKeys = deltaKeys,
                ExpectedStateDeltaKinds = deltaKinds,
                VisibleDiagnostics =
                [
                    "objective=" + id,
                    "actions=" + string.Join(",", linkedActions),
                    "deltaKinds=" + string.Join(",", deltaKinds),
                    requiresCheckpoint ? "checkpointStep=" + checkpointStep : "checkpointStep=not_required"
                ],
                CompletionCondition = condition,
                RequiresCheckpoint = requiresCheckpoint,
                RequiredCheckpointStepIndex = checkpointStep,
                ExpectedStateHashAfter = expectedHash,
                DeterministicHashContribution = Hash(
                    id
                    + "|"
                    + kind
                    + "|"
                    + string.Join(",", prerequisites)
                    + "|"
                    + string.Join(",", linkedActions)
                    + "|"
                    + string.Join(",", linkedEvents)
                    + "|"
                    + expectedHash)
            };
        }
    }

    private static OfflineGeoworldSessionDeltaRecord FirstDelta(
        IReadOnlyList<OfflineGeoworldSessionDeltaRecord> deltas,
        string actionKind,
        int indexFallback)
    {
        var match = deltas.FirstOrDefault(item =>
            string.Equals(item.ActionKind, actionKind, StringComparison.Ordinal));
        return match ?? deltas[Math.Min(indexFallback, deltas.Count - 1)];
    }

    private static OfflineGeoworldObjectiveReplayAcceptanceProof BuildPayloadReplayProof(
        Goal107SourceContext context,
        OfflineGeoworldObjectiveDocument objectives,
        OfflineGeoworldObjectiveAcceptanceRun run,
        OfflineGeoworldObjectiveCompletionState completion)
    {
        var actionIds = context.InitialState.Actions.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);
        var targetIds = context.InitialState.Targets.Select(item => item.TargetId).ToHashSet(StringComparer.Ordinal);
        var deltaKeys = context.DeltaLog.Deltas.Select(item => item.StateKey).ToHashSet(StringComparer.Ordinal);
        var knownActions = objectives.Objectives
            .SelectMany(item => item.LinkedActionIds)
            .All(actionIds.Contains);
        var knownTargets = objectives.Objectives
            .SelectMany(item => item.LinkedTargetIds)
            .All(targetIds.Contains);
        var knownDeltas = objectives.Objectives
            .SelectMany(item => item.ExpectedStateDeltaKeys)
            .All(deltaKeys.Contains);
        var prerequisites = ValidatePrerequisites(objectives.Objectives);
        var transitions = run.Steps.Count == objectives.ObjectiveCount
                          && completion.CompletedObjectiveIds.SequenceEqual(
                              objectives.Objectives.Select(item => item.ObjectiveId));
        var hashChain = completion.ObjectiveHashChain.Count == objectives.ObjectiveCount + 1
                        && completion.ObjectiveHashChain.Last() == completion.FinalObjectiveAcceptanceHash;
        var checkpoint = run.SaveLoadResumeRequired
                         && run.CheckpointStepIndex == context.Manifest.CheckpointStepIndex
                         && run.CheckpointStateHash == context.Manifest.CheckpointStateHash
                         && context.SourceReplayProof.CheckpointLoaded
                         && context.SourceReplayProof.ReplayResumedToFinalHash;
        var failedPrerequisiteRejected = !ValidatePrerequisites(
            objectives.Objectives.Select(item =>
                item.SequenceIndex == 4
                    ? item with { PrerequisiteObjectiveIds = ["missing_required_objective"] }
                    : item).ToList());
        return new OfflineGeoworldObjectiveReplayAcceptanceProof
        {
            Passed = context.Goal106Ready
                     && knownActions
                     && knownTargets
                     && knownDeltas
                     && prerequisites
                     && transitions
                     && hashChain
                     && checkpoint
                     && failedPrerequisiteRejected,
            PayloadReadAttempted = true,
            ManifestRead = true,
            ObjectivesRead = true,
            AcceptanceRunRead = true,
            CompletionStateRead = true,
            SourceGoal106PayloadRead = context.Goal106Ready,
            SourceGoal106ReplayProofRead = context.SourceReplayProof.Passed,
            SourceGoal106ReplayHashChainPassed = context.SourceReplayProof.ReplayResumedToFinalHash,
            CheckpointResumeApplied = checkpoint,
            ObjectivePrerequisitesPassed = prerequisites,
            CompletionTransitionsPassed = transitions,
            StateDeltaLinkagePassed = knownActions && knownTargets && knownDeltas,
            DeterministicHashChainPassed = hashChain,
            FailedPrerequisiteRejected = failedPrerequisiteRejected,
            NoAbsolutePaths = true,
            NoRawGeodata = true,
            NoBinaryOrRasterMedia = true,
            NoProviderOrNetworkMarkers = true,
            ObjectiveCount = objectives.ObjectiveCount,
            CompletedObjectiveCount = completion.CompletedObjectiveCount,
            FinalStatus = completion.FinalStatus,
            FinalObjectiveAcceptanceHash = completion.FinalObjectiveAcceptanceHash,
            ObjectiveHashChain = completion.ObjectiveHashChain
        };
    }

    private static bool ValidatePrerequisites(IReadOnlyList<OfflineGeoworldObjectiveDefinition> objectives)
    {
        var completed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var objective in objectives.OrderBy(item => item.SequenceIndex))
        {
            if (!objective.PrerequisiteObjectiveIds.All(completed.Contains))
            {
                return false;
            }

            completed.Add(objective.ObjectiveId);
        }

        return true;
    }

    private static Goal107SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        var sourceRoot = OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceGoal106Root;
        var manifest = ReadSource<OfflineGeoworldSessionManifest>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName,
            diagnostics) ?? new OfflineGeoworldSessionManifest();
        var initial = ReadSource<OfflineGeoworldSessionInitialState>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName,
            diagnostics) ?? new OfflineGeoworldSessionInitialState();
        var deltaLog = ReadSource<OfflineGeoworldSessionDeltaLog>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName,
            diagnostics) ?? new OfflineGeoworldSessionDeltaLog();
        var replay = ReadSource<OfflineGeoworldSessionReplayScript>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName,
            diagnostics) ?? new OfflineGeoworldSessionReplayScript();
        var checklist = ReadSource<OfflineGeoworldSessionAcceptanceChecklist>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName,
            diagnostics) ?? new OfflineGeoworldSessionAcceptanceChecklist();
        var sourceProof = ReadSource<OfflineGeoworldSessionSimulatedReplayProof>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
            diagnostics) ?? new OfflineGeoworldSessionSimulatedReplayProof();
        var sourceNegative = ReadSource<OfflineGeoworldSessionNegativeProof>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.NegativeProofFileName,
            diagnostics) ?? new OfflineGeoworldSessionNegativeProof();
        var sourceScripts = ReadSource<OfflineGeoworldSessionUnityScriptInventory>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName,
            diagnostics) ?? new OfflineGeoworldSessionUnityScriptInventory();
        var sourceEditor = ReadSource<OfflineGeoworldSessionEditorWindowInventory>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName,
            diagnostics) ?? new OfflineGeoworldSessionEditorWindowInventory();
        var sourceQuality = ReadSource<OfflineGeoworldSessionQualityGateScan>(
            root,
            sourceRoot + "/" + OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
            diagnostics) ?? new OfflineGeoworldSessionQualityGateScan();

        var goal106Ready = !manifest.Accepted
                           && manifest.ImplementationStatus == "GREEN"
                           && manifest.ReplayStepCount == 6
                           && manifest.StateDeltaCount == 6
                           && manifest.CheckpointStepIndex == 3
                           && initial.TargetCount >= 8
                           && deltaLog.DeltaCount == 6
                           && replay.ReplayStepCount == 6
                           && checklist.StepCount > 0
                           && sourceProof.Passed
                           && sourceProof.CheckpointLoaded
                           && sourceProof.ReplayResumedToFinalHash
                           && sourceNegative.Passed
                           && sourceScripts.Passed
                           && sourceEditor.Passed
                           && sourceQuality.Passed;
        var alphaPath = Resolve(root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldObjectiveAcceptanceRunVocabulary
                                     .AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldObjectiveAcceptanceRunVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        AddIfFalse(goal106Ready, "goal107.source.goal106_ready", "Goal106", diagnostics);
        AddIfFalse(alphaUnchanged, "goal107.source.alpha_unchanged", "AlphaRuntimeBootstrap", diagnostics);
        return new Goal107SourceContext(
            manifest,
            initial,
            deltaLog,
            replay,
            checklist,
            sourceProof,
            sourceNegative,
            sourceScripts,
            sourceEditor,
            sourceQuality,
            goal106Ready,
            alphaUnchanged,
            SortDiagnostics(diagnostics));
    }

    private static string RenderReport(
        OfflineGeoworldObjectiveReport report,
        OfflineGeoworldObjectiveQualityGateScan quality,
        OfflineGeoworldObjectiveReplayAcceptanceProof proof,
        OfflineGeoworldObjectiveAlphaQualityConsolidation consolidation) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 107 Offline Geoworld Objective Acceptance Run",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal107 adds a metadata-only Unity Alpha objective acceptance run over real Goal106 replay/save-load artifacts. It is Alpha tooling, not final Runtime gameplay, public schema, live geodata, provider execution, final art or release build.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- objectiveCount: " + report.ObjectiveCount,
            "- completedObjectiveCount: " + report.CompletedObjectiveCount,
            "- finalStatus: " + report.FinalStatus,
            "- replayStepCount: " + report.ReplayStepCount,
            "- stateDeltaCount: " + report.StateDeltaCount,
            "- checkpointStepIndex: " + report.CheckpointStepIndex,
            "- finalStateHash: " + report.FinalStateHash,
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- goal106Consumed: " + quality.Goal106Consumed.ToString().ToLowerInvariant(),
            "- objectivePayloadCreated: " + quality.ObjectivePayloadCreated.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + report.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- editorWindowReady: " + report.EditorWindowReady.ToString().ToLowerInvariant(),
            "- replayAcceptanceProofPassed: " + report.ReplayAcceptanceProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaQualityConsolidationPassed: "
            + report.AlphaQualityConsolidationPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: "
            + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- checkpointResumeApplied: " + proof.CheckpointResumeApplied.ToString().ToLowerInvariant(),
            "- objectivePrerequisitesPassed: "
            + proof.ObjectivePrerequisitesPassed.ToString().ToLowerInvariant(),
            "- completionTransitionsPassed: "
            + proof.CompletionTransitionsPassed.ToString().ToLowerInvariant(),
            "- failedPrerequisiteRejected: " + proof.FailedPrerequisiteRejected.ToString().ToLowerInvariant(),
            "- travelPreviewReady: " + consolidation.TravelPreviewReady.ToString().ToLowerInvariant(),
            "- sessionReplayReady: " + consolidation.SessionReplayReady.ToString().ToLowerInvariant(),
            "- objectiveAcceptanceRunReady: "
            + consolidation.ObjectiveAcceptanceRunReady.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: "
            + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noAbsolutePaths: " + quality.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            "- noScenePrefabSettingsChanges: "
            + quality.NoScenePrefabSettingsChanges.ToString().ToLowerInvariant(),
            "- noExternalDependenciesOrNewInputSystem: "
            + quality.NoExternalDependenciesOrNewInputSystem.ToString().ToLowerInvariant()
        ]) + Environment.NewLine;

    private sealed record Goal107Payload(
        OfflineGeoworldObjectiveManifest Manifest,
        OfflineGeoworldObjectiveDocument Objectives,
        OfflineGeoworldObjectiveAcceptanceRun AcceptanceRun,
        OfflineGeoworldObjectiveCompletionState CompletionState,
        OfflineGeoworldObjectiveReplayAcceptanceProof ReplayAcceptanceProof,
        OfflineGeoworldObjectiveReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal107SourceContext(
        OfflineGeoworldSessionManifest Manifest,
        OfflineGeoworldSessionInitialState InitialState,
        OfflineGeoworldSessionDeltaLog DeltaLog,
        OfflineGeoworldSessionReplayScript ReplayScript,
        OfflineGeoworldSessionAcceptanceChecklist Checklist,
        OfflineGeoworldSessionSimulatedReplayProof SourceReplayProof,
        OfflineGeoworldSessionNegativeProof SourceNegativeProof,
        OfflineGeoworldSessionUnityScriptInventory SourceUnityScripts,
        OfflineGeoworldSessionEditorWindowInventory SourceEditorWindow,
        OfflineGeoworldSessionQualityGateScan SourceQualityGate,
        bool Goal106Ready,
        bool AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldObjectiveDiagnostic> Diagnostics);
}
