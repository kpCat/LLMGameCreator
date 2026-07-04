using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

namespace LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

public sealed partial class OfflineGeoworldSessionPersistenceReplayEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldSessionBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var scripts = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var proof = ValidatePayload(payload.PayloadFiles, payloadReadAttempted: true);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(root, context, payload, scripts, editor, proof, negative, binding, lineage);
        return BuildResult(payload, scripts, editor, proof, negative, binding, lineage, quality);
    }

    public async Task<OfflineGeoworldSessionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldSessionPersistenceReplayVocabulary.StreamingAssetsRelativeRoot);
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
        var proof = ValidateMirroredPayload(root, mirroredPayload);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(root, context, mirrored, scripts, editor, proof, negative, binding, lineage);
        var result = BuildResult(mirrored, scripts, editor, proof, negative, binding, lineage, quality);

        var outputDirectory = Resolve(
            root,
            OfflineGeoworldSessionPersistenceReplayVocabulary.RelativeOutputDirectory);
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
            OfflineGeoworldSessionPersistenceReplayVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldSessionWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldSessionBuildResult BuildResult(
        Goal106Payload payload,
        OfflineGeoworldSessionUnityScriptInventory scripts,
        OfflineGeoworldSessionEditorWindowInventory editor,
        OfflineGeoworldSessionSimulatedReplayProof proof,
        OfflineGeoworldSessionNegativeProof negative,
        OfflineGeoworldSessionWorkspaceBindingInventory binding,
        OfflineGeoworldSessionSourceLineage lineage,
        OfflineGeoworldSessionQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(scripts, editor, proof, negative, binding, lineage, quality);
        var reportWithoutHash = BuildReport(payload, scripts, editor, proof, negative, binding, quality);
        var markdownWithoutHash = RenderReport(reportWithoutHash, quality, proof);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(markdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof);
        return new OfflineGeoworldSessionBuildResult
        {
            Manifest = payload.Manifest,
            InitialState = payload.InitialState,
            DeltaLog = payload.DeltaLog,
            ReplayScript = payload.ReplayScript,
            AcceptanceChecklist = payload.AcceptanceChecklist,
            Readme = payload.Readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedReplayProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payload.PayloadFiles,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal106Payload BuildPayload(Goal106SourceContext context)
    {
        var sourceTargets = context.Targets.Targets;
        var sourceActions = context.Actions.Actions;
        var sourceEvents = context.Session.Events;
        var sourceDeltas = context.StateDeltaPlan.Deltas
            .OrderBy(item => item.DeltaIndex)
            .ToList();
        var checkpointStep = Math.Max(3, sourceDeltas.Count / 2);
        checkpointStep = Math.Min(checkpointStep, sourceDeltas.Count);
        var checkpointHash = context.StateDeltaPlan.StateHashChain.Count > checkpointStep
            ? context.StateDeltaPlan.StateHashChain[checkpointStep]
            : string.Empty;

        var initial = new OfflineGeoworldSessionInitialState
        {
            TargetCount = sourceTargets.Count,
            ActionCount = sourceActions.Count,
            ScriptedEventCount = sourceEvents.Count,
            StateDeltaCount = sourceDeltas.Count,
            InitialStateHash = context.StateDeltaPlan.InitialStateHash,
            SourceFinalStateHash = context.StateDeltaPlan.FinalStateHash,
            Targets = sourceTargets.Select(target => new OfflineGeoworldSessionTargetLineage
            {
                TargetId = target.TargetId,
                TargetName = target.TargetName,
                SourceObjectId = target.SourceObjectId,
                SourceObjectName = target.SourceObjectName,
                SourceChunkKey = target.SourceChunkKey,
                RawGeodataIncluded = target.RawGeodataIncluded
            }).ToList(),
            Actions = sourceActions.Select(action => new OfflineGeoworldSessionActionLineage
            {
                ActionId = action.ActionId,
                TargetId = action.TargetId,
                ActionKind = action.ActionKind,
                StateDeltaKind = action.StateDeltaKind
            }).ToList(),
            SessionEvents = sourceEvents.Select(item => new OfflineGeoworldSessionEventLineage
            {
                EventIndex = item.EventIndex,
                EventId = item.EventId,
                TargetId = item.TargetId,
                ActionId = item.ActionId,
                ExpectedStateHashBefore = item.ExpectedStateHashBefore,
                ExpectedStateHashAfter = item.ExpectedStateHashAfter
            }).ToList()
        };
        var deltas = sourceDeltas.Select(delta => new OfflineGeoworldSessionDeltaRecord
        {
            DeltaIndex = delta.DeltaIndex,
            ReplayStepIndex = delta.DeltaIndex + 1,
            EventId = delta.EventId,
            TargetId = delta.TargetId,
            ActionId = delta.ActionId,
            ActionKind = delta.ActionKind,
            DeltaKind = delta.DeltaKind,
            StateKey = delta.StateKey,
            StateValue = delta.StateValue,
            StateHashBefore = delta.PreviousStateHash,
            StateHashAfter = delta.DeterministicStateHash,
            MutatesBaseDataDirectly = delta.MutatesBaseDataDirectly
        }).ToList();
        var deltaLog = new OfflineGeoworldSessionDeltaLog
        {
            DeltaCount = deltas.Count,
            InitialStateHash = context.StateDeltaPlan.InitialStateHash,
            FinalStateHash = context.StateDeltaPlan.FinalStateHash,
            StateHashChain = context.StateDeltaPlan.StateHashChain,
            Deltas = deltas
        };
        var snapshotHash = BuildSnapshotHash(
            context.StateDeltaPlan.InitialStateHash,
            checkpointStep,
            checkpointHash,
            deltas.Take(checkpointStep).Select(item => item.EventId));
        var replay = new OfflineGeoworldSessionReplayScript
        {
            ReplayStepCount = deltas.Count,
            InitialStateHash = context.StateDeltaPlan.InitialStateHash,
            FinalStateHash = context.StateDeltaPlan.FinalStateHash,
            Checkpoint = new OfflineGeoworldSessionCheckpoint
            {
                AfterEventCount = checkpointStep,
                StepIndex = checkpointStep,
                StateHash = checkpointHash,
                SnapshotHash = snapshotHash
            },
            Steps = deltas.Select(delta => new OfflineGeoworldSessionReplayStep
            {
                StepIndex = delta.ReplayStepIndex,
                EventId = delta.EventId,
                DeltaId = "goal106_delta_" + delta.DeltaIndex.ToString("00"),
                StateHashBefore = delta.StateHashBefore,
                StateHashAfter = delta.StateHashAfter
            }).ToList()
        };
        var checklist = BuildAcceptanceChecklist(deltas.Count, checkpointStep);
        var readme = new OfflineGeoworldSessionReadme();
        var initialJson = Serialize(initial);
        var deltaJson = Serialize(deltaLog);
        var replayJson = Serialize(replay);
        var checklistJson = Serialize(checklist);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldSessionManifest
        {
            PayloadFileCount = OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames.Count,
            SourceGoal105TargetCount = context.Manifest.TargetCount,
            SourceGoal105ActionCount = context.Manifest.ActionCount,
            SourceGoal105ActionKindCount = context.Manifest.ActionKindCount,
            ReplayStepCount = replay.ReplayStepCount,
            StateDeltaCount = deltaLog.DeltaCount,
            CheckpointAfterEventCount = checkpointStep,
            CheckpointStepIndex = checkpointStep,
            InitialStateHash = deltaLog.InitialStateHash,
            CheckpointStateHash = checkpointHash,
            FinalStateHash = deltaLog.FinalStateHash,
            AlphaRuntimeBootstrapUnchanged = context.AlphaRuntimeBootstrapUnchanged,
            InitialStateHashFile = Hash(initialJson),
            DeltaLogHash = Hash(deltaJson),
            ReplayScriptHash = Hash(replayJson),
            AcceptanceChecklistHash = Hash(checklistJson),
            ReadmeHash = Hash(readmeJson)
        };
        var manifestJson = Serialize(manifest);
        return new Goal106Payload(
            manifest,
            initial,
            deltaLog,
            replay,
            checklist,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName] = manifestJson,
                [OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName] = initialJson,
                [OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName] = deltaJson,
                [OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName] = replayJson,
                [OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName] = checklistJson,
                [OfflineGeoworldSessionPersistenceReplayVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static OfflineGeoworldSessionAcceptanceChecklist BuildAcceptanceChecklist(
        int replayStepCount,
        int checkpointStep)
    {
        var steps = new[]
        {
            ("Open the Unity Alpha project and load the existing offline geoworld review scene.",
                "Unity opens without changing project settings, packages, scenes or prefabs."),
            ("Confirm the Goal106 StreamingAssets payload is present under LLMGameCreator/OfflineGeoworldGoal106.",
                "Manifest, initial state, delta log, replay script, checklist and readme are readable."),
            ("Open LLMGameCreator/Offline Geoworld Session Replay and press Refresh.",
                "The editor helper shows payload ready, replay steps and checkpoint state hash."),
            ("Press Create Rig.",
                "A manual save-load/replay rig is created with save/load and replay controllers."),
            ("Run or inspect the first " + checkpointStep + " replay steps and save a session snapshot.",
                "Snapshot hash matches the checkpoint state hash from the replay script."),
            ("Clear the in-memory rig state, then load the saved snapshot.",
                "Loaded state resumes from checkpoint step " + checkpointStep + "."),
            ("Continue replay through " + replayStepCount + " total steps.",
                "Final state hash matches the manifest final state hash."),
            ("Attempt duplicate/corrupted snapshot handling from the helper controls.",
                "Duplicate replay is rejected or deterministic no-op, and corrupted snapshot is rejected."),
            ("Press Clear Rig after review.",
                "Only the manual rig objects are removed; no scene or project settings are saved.")
        };
        return new OfflineGeoworldSessionAcceptanceChecklist
        {
            StepCount = steps.Length,
            Steps = steps.Select((step, index) => new OfflineGeoworldSessionAcceptanceStep
            {
                StepIndex = index + 1,
                Instruction = step.Item1,
                ExpectedResult = step.Item2
            }).ToList()
        };
    }

    private static Goal106SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
        var sourceRoot = OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root;
        var manifest = ReadSource<OfflineGeoworldInteractionManifest>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName,
            diagnostics) ?? new OfflineGeoworldInteractionManifest();
        var targets = ReadSource<OfflineGeoworldInteractionTargetsDocument>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName,
            diagnostics) ?? new OfflineGeoworldInteractionTargetsDocument();
        var actions = ReadSource<OfflineGeoworldInteractionActionsDocument>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName,
            diagnostics) ?? new OfflineGeoworldInteractionActionsDocument();
        var session = ReadSource<OfflineGeoworldInteractionSessionScript>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName,
            diagnostics) ?? new OfflineGeoworldInteractionSessionScript();
        var deltas = ReadSource<OfflineGeoworldInteractionStateDeltaPlan>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName,
            diagnostics) ?? new OfflineGeoworldInteractionStateDeltaPlan();
        var proof = ReadSource<OfflineGeoworldInteractionSimulatedSessionProof>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName,
            diagnostics) ?? new OfflineGeoworldInteractionSimulatedSessionProof();
        var quality = ReadSource<OfflineGeoworldInteractionQualityGateScan>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
            diagnostics) ?? new OfflineGeoworldInteractionQualityGateScan();
        var scripts = ReadSource<OfflineGeoworldInteractionUnityScriptInventory>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName,
            diagnostics) ?? new OfflineGeoworldInteractionUnityScriptInventory();

        var goal105Ready = !manifest.Accepted
                           && manifest.ImplementationStatus == "GREEN"
                           && manifest.TargetCount >= 8
                           && manifest.ActionKindCount >= 5
                           && manifest.ScriptedEventCount >= 6
                           && manifest.StateDeltaCount >= 6
                           && proof.Passed
                           && proof.DeterministicStateHashChainPassed
                           && quality.Passed
                           && scripts.Passed;
        var alphaPath = Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.AlphaRuntimeBootstrapPath);
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = File.Exists(alphaPath)
            ? CountLines(File.ReadAllText(alphaPath, Encoding.UTF8))
            : 0;
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldSessionPersistenceReplayVocabulary
                                     .AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldSessionPersistenceReplayVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        AddIfFalse(goal105Ready, "goal106.source.goal105_ready", "Goal105", diagnostics);
        AddIfFalse(alphaUnchanged, "goal106.source.alpha_unchanged", "AlphaRuntimeBootstrap", diagnostics);
        return new Goal106SourceContext(
            manifest,
            targets,
            actions,
            session,
            deltas,
            proof,
            quality,
            scripts,
            goal105Ready,
            alphaUnchanged,
            SortDiagnostics(diagnostics));
    }

    private static string RenderReport(
        OfflineGeoworldSessionReport report,
        OfflineGeoworldSessionQualityGateScan quality,
        OfflineGeoworldSessionSimulatedReplayProof proof) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 106 Offline Geoworld Session Persistence Replay",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal106 adds Alpha-only save/load/replay metadata over real Goal105 interaction deltas. It does not add final Runtime save systems, schema changes, live geodata, provider calls, final gameplay or binary media.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- replayStepCount: " + report.ReplayStepCount,
            "- stateDeltaCount: " + report.StateDeltaCount,
            "- checkpointStepIndex: " + report.CheckpointStepIndex,
            "- checkpointStateHash: " + report.CheckpointStateHash,
            "- finalStateHash: " + report.FinalStateHash,
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- goal105Consumed: " + quality.Goal105Consumed.ToString().ToLowerInvariant(),
            "- sessionPayloadCreated: " + quality.SessionPayloadCreated.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + report.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- editorWindowReady: " + report.EditorWindowReady.ToString().ToLowerInvariant(),
            "- simulatedSaveLoadReplayProofPassed: "
            + report.SimulatedSaveLoadReplayProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: "
            + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- checkpointSaved: " + proof.CheckpointSaved.ToString().ToLowerInvariant(),
            "- checkpointLoaded: " + proof.CheckpointLoaded.ToString().ToLowerInvariant(),
            "- replayResumedToFinalHash: " + proof.ReplayResumedToFinalHash.ToString().ToLowerInvariant(),
            "- duplicateReplayRejected: " + proof.DuplicateReplayRejected.ToString().ToLowerInvariant(),
            "- corruptedSnapshotRejected: " + proof.CorruptedSnapshotRejected.ToString().ToLowerInvariant(),
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

    private sealed record Goal106Payload(
        OfflineGeoworldSessionManifest Manifest,
        OfflineGeoworldSessionInitialState InitialState,
        OfflineGeoworldSessionDeltaLog DeltaLog,
        OfflineGeoworldSessionReplayScript ReplayScript,
        OfflineGeoworldSessionAcceptanceChecklist AcceptanceChecklist,
        OfflineGeoworldSessionReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal106SourceContext(
        OfflineGeoworldInteractionManifest Manifest,
        OfflineGeoworldInteractionTargetsDocument Targets,
        OfflineGeoworldInteractionActionsDocument Actions,
        OfflineGeoworldInteractionSessionScript Session,
        OfflineGeoworldInteractionStateDeltaPlan StateDeltaPlan,
        OfflineGeoworldInteractionSimulatedSessionProof Proof,
        OfflineGeoworldInteractionQualityGateScan Quality,
        OfflineGeoworldInteractionUnityScriptInventory UnityScripts,
        bool Goal105Ready,
        bool AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics);
}
