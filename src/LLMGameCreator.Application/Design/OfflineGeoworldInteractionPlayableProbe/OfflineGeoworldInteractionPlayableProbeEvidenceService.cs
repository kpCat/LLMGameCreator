using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

public sealed partial class OfflineGeoworldInteractionPlayableProbeEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldInteractionBuildResult Build(string repositoryRootPath)
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

    public async Task<OfflineGeoworldInteractionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldInteractionPlayableProbeVocabulary.StreamingAssetsRelativeRoot);
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
            OfflineGeoworldInteractionPlayableProbeVocabulary.RelativeOutputDirectory);
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
            OfflineGeoworldInteractionPlayableProbeVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldInteractionWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldInteractionBuildResult BuildResult(
        Goal105Payload payload,
        OfflineGeoworldInteractionUnityScriptInventory scripts,
        OfflineGeoworldInteractionEditorWindowInventory editor,
        OfflineGeoworldInteractionSimulatedSessionProof proof,
        OfflineGeoworldInteractionNegativeProof negative,
        OfflineGeoworldInteractionWorkspaceBindingInventory binding,
        OfflineGeoworldInteractionSourceLineage lineage,
        OfflineGeoworldInteractionQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(scripts, editor, proof, negative, binding, lineage, quality);
        var reportWithoutHash = BuildReport(payload, scripts, editor, proof, negative, binding, quality, evidence);
        var markdownWithoutHash = RenderReport(reportWithoutHash, quality, proof);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(markdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof);
        return new OfflineGeoworldInteractionBuildResult
        {
            Manifest = payload.Manifest,
            Targets = payload.Targets,
            Actions = payload.Actions,
            SessionScript = payload.SessionScript,
            StateDeltaPlan = payload.StateDeltaPlan,
            Readme = payload.Readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedSessionProof = proof,
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

    private static Goal105Payload BuildPayload(Goal105SourceContext context)
    {
        var baseTargets = BuildTargets(context);
        var actions = BuildActions(baseTargets);
        var actionsByTarget = actions
            .GroupBy(item => item.TargetId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.ActionId).Order(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);
        var targets = baseTargets
            .Select(target => target with
            {
                ActionIds = actionsByTarget.TryGetValue(target.TargetId, out var ids) ? ids : []
            })
            .ToList();
        var state = BuildSessionAndState(targets, actions);

        var targetsDocument = new OfflineGeoworldInteractionTargetsDocument
        {
            TargetCount = targets.Count,
            SourceGoal104VisibleObjectCount = context.SourceObjectIndex.Objects.Count(item =>
                item.VisibleStepIndexes.Count > 0),
            Targets = targets
        };
        var actionsDocument = new OfflineGeoworldInteractionActionsDocument
        {
            ActionCount = actions.Count,
            ActionKindCount = actions.Select(item => item.ActionKind).Distinct(StringComparer.Ordinal).Count(),
            ActionKinds = actions.Select(item => item.ActionKind).Distinct(StringComparer.Ordinal).Order().ToList(),
            Actions = actions
        };
        var readme = new OfflineGeoworldInteractionReadme();
        var targetsJson = Serialize(targetsDocument);
        var actionsJson = Serialize(actionsDocument);
        var sessionJson = Serialize(state.SessionScript);
        var deltaJson = Serialize(state.StateDeltaPlan);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldInteractionManifest
        {
            PayloadFileCount = OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames.Count,
            TargetCount = targets.Count,
            ActionCount = actions.Count,
            ActionKindCount = actionsDocument.ActionKindCount,
            ScriptedEventCount = state.SessionScript.EventCount,
            StateDeltaCount = state.StateDeltaPlan.StateDeltaCount,
            SourceGoal104ObjectCount = context.SourceObjectIndex.ObjectCount,
            SourceGoal104MovementSampleCount = context.SourceManifest.MovementSampleCount,
            SourceGoal104BoundaryCrossingCount = context.SourceManifest.BoundaryCrossingCount,
            AlphaRuntimeBootstrapUnchanged = context.AlphaRuntimeBootstrapUnchanged,
            InitialStateHash = state.StateDeltaPlan.InitialStateHash,
            FinalStateHash = state.StateDeltaPlan.FinalStateHash,
            TargetsHash = Hash(targetsJson),
            ActionsHash = Hash(actionsJson),
            SessionScriptHash = Hash(sessionJson),
            StateDeltaPlanHash = Hash(deltaJson),
            ReadmeHash = Hash(readmeJson)
        };
        var manifestJson = Serialize(manifest);
        return new Goal105Payload(
            manifest,
            targetsDocument,
            actionsDocument,
            state.SessionScript,
            state.StateDeltaPlan,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName] = manifestJson,
                [OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName] = targetsJson,
                [OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName] = actionsJson,
                [OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName] = sessionJson,
                [OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName] = deltaJson,
                [OfflineGeoworldInteractionPlayableProbeVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static List<OfflineGeoworldInteractionTargetRecord> BuildTargets(Goal105SourceContext context) =>
        context.SourceObjectIndex.Objects
            .Where(item => item.VisibleStepIndexes.Count > 0)
            .OrderBy(item => item.ObjectId, StringComparer.Ordinal)
            .Take(8)
            .Select((source, index) =>
            {
                var shortHash = Hash(source.ObjectId)[..12];
                return new OfflineGeoworldInteractionTargetRecord
                {
                    TargetId = "interaction_target/" + shortHash,
                    TargetName = "__LLMGC_OfflineGeoworldInteraction_"
                                 + Compact(source.CommandKind)
                                 + "_"
                                 + shortHash,
                    SourceObjectId = source.ObjectId,
                    SourceObjectName = source.ObjectName,
                    SourceCommandId = source.SourceCommandId,
                    CommandKind = source.CommandKind,
                    SourceChunkKey = source.SourceChunkKey,
                    GridX = source.GridX,
                    GridZ = source.GridZ,
                    Elevation = source.Elevation,
                    InteractionRadius = 2.5 + index % 3,
                    VisibleStepIndexes = source.VisibleStepIndexes,
                    MetadataOnly = source.MetadataOnly,
                    RawGeodataIncluded = source.RawGeodataIncluded
                };
            })
            .ToList();

    private static List<OfflineGeoworldInteractionActionRecord> BuildActions(
        IReadOnlyList<OfflineGeoworldInteractionTargetRecord> targets)
    {
        var kinds = OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredActionKinds;
        return targets.Select((target, index) =>
        {
            var kind = index < kinds.Count ? kinds[index] : "inspect";
            return new OfflineGeoworldInteractionActionRecord
            {
                ActionId = ActionId(target.TargetId, kind),
                TargetId = target.TargetId,
                ActionKind = kind,
                DisplayLabel = kind.Replace('_', ' ') + " " + target.CommandKind,
                RequiredRadius = Math.Min(target.InteractionRadius, kind == "collect_sample" ? 2.5 : 3.5),
                StateDeltaKind = kind switch
                {
                    "inspect" => "inspection_log",
                    "enter_or_focus" => "focus_target",
                    "mark_visited" => "visited_flag",
                    "toggle_blocked" => "blocked_flag",
                    "collect_sample" => "sample_collected",
                    _ => "interaction_event"
                }
            };
        }).ToList();
    }

    private static Goal105SessionState BuildSessionAndState(
        IReadOnlyList<OfflineGeoworldInteractionTargetRecord> targets,
        IReadOnlyList<OfflineGeoworldInteractionActionRecord> actions)
    {
        var actionsByTarget = actions.ToDictionary(item => item.TargetId, item => item, StringComparer.Ordinal);
        var previousHash = BuildInitialStateHash(targets, actions);
        var chain = new List<string> { previousHash };
        var events = new List<OfflineGeoworldInteractionScriptedEvent>();
        var deltas = new List<OfflineGeoworldInteractionStateDelta>();

        foreach (var target in targets.Take(6))
        {
            var action = actionsByTarget[target.TargetId];
            var eventIndex = events.Count;
            var playerX = target.GridX + eventIndex % 2;
            var playerZ = target.GridZ;
            var distance = Distance(playerX, playerZ, target);
            var eventId = "goal105_scripted_interaction_" + eventIndex.ToString("00");
            var deltaWithoutHash = new OfflineGeoworldInteractionStateDelta
            {
                DeltaIndex = eventIndex,
                EventId = eventId,
                TargetId = target.TargetId,
                ActionId = action.ActionId,
                ActionKind = action.ActionKind,
                DeltaKind = action.StateDeltaKind,
                StateKey = target.TargetId + "/" + action.ActionKind,
                StateValue = action.ActionKind == "collect_sample"
                    ? "sample:" + Hash(target.SourceObjectId)[..10]
                    : "true",
                PreviousStateHash = previousHash
            };
            var delta = deltaWithoutHash with
            {
                DeterministicStateHash = Hash(BuildDeltaHashSeed(deltaWithoutHash))
            };
            var scripted = new OfflineGeoworldInteractionScriptedEvent
            {
                EventIndex = eventIndex,
                EventId = eventId,
                TargetId = target.TargetId,
                ActionId = action.ActionId,
                ActionKind = action.ActionKind,
                PlayerGridX = playerX,
                PlayerGridZ = playerZ,
                DistanceToTarget = distance,
                RequiredRadius = action.RequiredRadius,
                AvailableByDistance = distance <= action.RequiredRadius,
                ExpectedStateHashBefore = previousHash,
                ExpectedStateHashAfter = delta.DeterministicStateHash
            };
            events.Add(scripted);
            deltas.Add(delta);
            previousHash = delta.DeterministicStateHash;
            chain.Add(previousHash);
        }

        var session = new OfflineGeoworldInteractionSessionScript
        {
            EventCount = events.Count,
            Events = events
        };
        var plan = new OfflineGeoworldInteractionStateDeltaPlan
        {
            StateDeltaCount = deltas.Count,
            InitialStateHash = chain.First(),
            FinalStateHash = chain.Last(),
            StateHashChain = chain,
            Deltas = deltas
        };
        return new Goal105SessionState(session, plan);
    }

    private static Goal105SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
        var sourceRoot = OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot;
        var manifest = ReadSource<OfflineGeoworldInteractiveTravelManifest>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveTravelManifest();
        var movement = ReadSource<OfflineGeoworldInteractiveTravelStepsDocument>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveTravelStepsDocument();
        var objects = ReadSource<OfflineGeoworldInteractiveObjectStateIndex>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveObjectStateIndex();
        var proof = ReadSource<OfflineGeoworldInteractiveSimulatedExecutionProof>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveSimulatedExecutionProof();
        var quality = ReadSource<OfflineGeoworldInteractiveQualityGateScan>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveQualityGateScan();
        var scripts = ReadSource<OfflineGeoworldInteractiveUnityScriptInventory>(
            root,
            sourceRoot + "/" + OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
            diagnostics) ?? new OfflineGeoworldInteractiveUnityScriptInventory();

        var goal104Ready = !manifest.Accepted
                           && manifest.ImplementationStatus == "GREEN"
                           && manifest.MovementSampleCount >= 6
                           && manifest.BoundaryCrossingCount >= 2
                           && manifest.ObjectCount >= 18
                           && objects.ObjectCount >= 18
                           && movement.MovementSampleCount >= 6
                           && proof.Passed
                           && quality.Passed;
        var alphaPath = Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.AlphaRuntimeBootstrapPath);
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = File.Exists(alphaPath)
            ? CountLines(File.ReadAllText(alphaPath, Encoding.UTF8))
            : 0;
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldInteractionPlayableProbeVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldInteractionPlayableProbeVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        AddIfFalse(goal104Ready, "goal105.source.goal104_ready", "Goal104", diagnostics);
        AddIfFalse(alphaUnchanged, "goal105.source.alpha_unchanged", "AlphaRuntimeBootstrap", diagnostics);
        return new Goal105SourceContext(
            manifest,
            movement,
            objects,
            proof,
            quality,
            scripts,
            goal104Ready,
            alphaUnchanged,
            SortDiagnostics(diagnostics));
    }

    private static string RenderReport(
        OfflineGeoworldInteractionReport report,
        OfflineGeoworldInteractionQualityGateScan quality,
        OfflineGeoworldInteractionSimulatedSessionProof proof) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 105 Offline Geoworld Interaction Playable Probe",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal105 adds a metadata-only Unity Alpha interaction probe over real Goal104 visible objects. It builds interaction targets, action availability, scripted interaction events and separate state-delta payloads without final Runtime gameplay, live geodata fetching, provider calls or final art.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- targetCount: " + report.TargetCount,
            "- actionKindCount: " + report.ActionKindCount,
            "- actionCount: " + report.ActionCount,
            "- scriptedEventCount: " + report.ScriptedEventCount,
            "- stateDeltaCount: " + report.StateDeltaCount,
            "- finalStateHash: " + report.FinalStateHash,
            "- stateHashChainLength: " + proof.StateHashChain.Count,
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- targetGraphBuilt: " + quality.TargetGraphBuilt.ToString().ToLowerInvariant(),
            "- actionGraphBuilt: " + quality.ActionGraphBuilt.ToString().ToLowerInvariant(),
            "- sessionScriptBuilt: " + quality.SessionScriptBuilt.ToString().ToLowerInvariant(),
            "- stateDeltaPlanBuilt: " + quality.StateDeltaPlanBuilt.ToString().ToLowerInvariant(),
            "- stateHashChainPassed: " + quality.StateHashChainPassed.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + report.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- editorWindowReady: " + report.EditorWindowReady.ToString().ToLowerInvariant(),
            "- unityScriptInventorySafetyPassed: " + report.UnityScriptInventorySafetyPassed.ToString().ToLowerInvariant(),
            "- simulatedSessionProofPassed: " + report.SimulatedSessionProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: " + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noAbsolutePaths: " + quality.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            "- noScenePrefabSettingsChanges: " + quality.NoScenePrefabSettingsChanges.ToString().ToLowerInvariant(),
            "- noExternalDependenciesOrNewInputSystem: " + quality.NoExternalDependenciesOrNewInputSystem.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- manifestHash: " + report.ManifestHash,
            "- targetsHash: " + report.TargetsHash,
            "- actionsHash: " + report.ActionsHash,
            "- sessionScriptHash: " + report.SessionScriptHash,
            "- stateDeltaPlanHash: " + report.StateDeltaPlanHash,
            "- unityScriptInventoryHash: " + report.UnityScriptInventoryHash,
            "- editorWindowInventoryHash: " + report.EditorWindowInventoryHash,
            "- simulatedSessionProofHash: " + report.SimulatedSessionProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private sealed record Goal105Payload(
        OfflineGeoworldInteractionManifest Manifest,
        OfflineGeoworldInteractionTargetsDocument Targets,
        OfflineGeoworldInteractionActionsDocument Actions,
        OfflineGeoworldInteractionSessionScript SessionScript,
        OfflineGeoworldInteractionStateDeltaPlan StateDeltaPlan,
        OfflineGeoworldInteractionReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal105SessionState(
        OfflineGeoworldInteractionSessionScript SessionScript,
        OfflineGeoworldInteractionStateDeltaPlan StateDeltaPlan);

    private sealed record Goal105SourceContext(
        OfflineGeoworldInteractiveTravelManifest SourceManifest,
        OfflineGeoworldInteractiveTravelStepsDocument SourceMovement,
        OfflineGeoworldInteractiveObjectStateIndex SourceObjectIndex,
        OfflineGeoworldInteractiveSimulatedExecutionProof SourceProof,
        OfflineGeoworldInteractiveQualityGateScan SourceQuality,
        OfflineGeoworldInteractiveUnityScriptInventory SourceUnityScripts,
        bool Goal104Ready,
        bool AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics);
}
