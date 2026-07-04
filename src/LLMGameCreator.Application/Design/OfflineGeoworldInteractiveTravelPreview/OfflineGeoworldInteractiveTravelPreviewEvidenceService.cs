using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

public sealed partial class OfflineGeoworldInteractiveTravelPreviewEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldInteractiveBuildResult Build(string repositoryRootPath)
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

    public async Task<OfflineGeoworldInteractiveWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldInteractiveTravelPreviewVocabulary.StreamingAssetsRelativeRoot);
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
            OfflineGeoworldInteractiveTravelPreviewVocabulary.RelativeOutputDirectory);
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
            OfflineGeoworldInteractiveTravelPreviewVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldInteractiveWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldInteractiveBuildResult BuildResult(
        Goal104Payload payload,
        OfflineGeoworldInteractiveUnityScriptInventory scripts,
        OfflineGeoworldInteractiveEditorWindowInventory editor,
        OfflineGeoworldInteractiveSimulatedExecutionProof proof,
        OfflineGeoworldInteractiveNegativeProof negative,
        OfflineGeoworldInteractiveWorkspaceBindingInventory binding,
        OfflineGeoworldInteractiveSourceLineage lineage,
        OfflineGeoworldInteractiveQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(scripts, editor, proof, negative, binding, lineage, quality);
        var reportWithoutHash = BuildReport(payload, scripts, editor, proof, negative, binding, quality, evidence);
        var markdownWithoutHash = RenderReport(reportWithoutHash, quality, proof);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(markdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof);
        return new OfflineGeoworldInteractiveBuildResult
        {
            Manifest = payload.Manifest,
            Steps = payload.MovementPath,
            ChunkVisibility = payload.BoundaryZones,
            ObjectStateIndex = payload.PrefetchPlan,
            Readme = payload.Readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedExecutionProof = proof,
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

    private static Goal104Payload BuildPayload(Goal104SourceContext context)
    {
        var objectStates = BuildObjectStates(context);
        var samples = BuildMovementSamples(context, objectStates);
        var boundaryZones = BuildBoundaryZones(samples);
        var prefetchPlans = BuildPrefetchPlans(boundaryZones);
        var movementPath = new OfflineGeoworldInteractiveTravelStepsDocument
        {
            StepCount = samples.Count,
            MovementSampleCount = samples.Count,
            ObjectCount = objectStates.Count,
            Steps = samples,
            MovementSamples = samples,
            Objects = objectStates
        };
        var boundaryDocument = new OfflineGeoworldInteractiveChunkVisibilityDocument
        {
            StepCount = samples.Count,
            Steps = BuildChunkVisibility(samples, objectStates),
            BoundaryCrossingCount = boundaryZones.Count,
            BoundaryZones = boundaryZones
        };
        var prefetchDocument = new OfflineGeoworldInteractiveObjectStateIndex
        {
            ObjectCount = objectStates.Count,
            Objects = objectStates,
            PrefetchPlanCount = prefetchPlans.Count,
            Plans = prefetchPlans
        };
        var readme = new OfflineGeoworldInteractiveReadme();
        var movementJson = Serialize(movementPath);
        var boundaryJson = Serialize(boundaryDocument);
        var prefetchJson = Serialize(prefetchDocument);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldInteractiveTravelManifest
        {
            PayloadFileCount = OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredPayloadFileNames.Count,
            StepCount = samples.Count,
            MovementSampleCount = samples.Count,
            BoundaryCrossingCount = boundaryZones.Count,
            PrefetchPlanCount = prefetchPlans.Count,
            ObjectCount = objectStates.Count,
            SourceCommandCount = context.SourceObjectIndex.ObjectCount,
            SourceTravelWindowStepCount = context.SourceSteps.StepCount,
            SourceGoal103StepCount = context.SourceSteps.StepCount,
            SourceGoal103ObjectCount = context.SourceObjectIndex.ObjectCount,
            MaxActiveChunkCount = samples.Count == 0 ? 0 : samples.Max(item => item.ActiveChunkKeys.Count),
            MaxBoundaryPrefetchChunkCount = samples.Count == 0
                ? 0
                : samples.Max(item => item.BoundaryPrefetchChunkKeys.Count),
            AlphaRuntimeBootstrapUnchanged = context.AlphaRuntimeBootstrapUnchanged,
            StepsHash = Hash(movementJson),
            ChunkVisibilityHash = Hash(boundaryJson),
            ObjectStateIndexHash = Hash(prefetchJson),
            MovementPathHash = Hash(movementJson),
            BoundaryZonesHash = Hash(boundaryJson),
            PrefetchPlanHash = Hash(prefetchJson),
            ReadmeHash = Hash(readmeJson)
        };
        var manifestJson = Serialize(manifest);
        return new Goal104Payload(
            manifest,
            movementPath,
            boundaryDocument,
            prefetchDocument,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName] = manifestJson,
                [OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName] = movementJson,
                [OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilityFileName] = boundaryJson,
                [OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName] = prefetchJson,
                [OfflineGeoworldInteractiveTravelPreviewVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static IReadOnlyList<OfflineGeoworldInteractiveObjectState> BuildObjectStates(
        Goal104SourceContext context)
    {
        var visibleBySourceObject = context.SourceSteps.Steps
            .SelectMany(step => step.VisibleObjectIds.Select(objectId => (objectId, step.StepIndex)))
            .GroupBy(item => item.objectId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.StepIndex).Distinct().Order().ToList(),
                StringComparer.Ordinal);

        return context.SourceObjectIndex.Objects
            .OrderBy(item => item.ObjectId, StringComparer.Ordinal)
            .Select(source =>
            {
                visibleBySourceObject.TryGetValue(source.ObjectId, out var visibleSteps);
                var shortHash = Hash(source.ObjectId)[..12];
                return new OfflineGeoworldInteractiveObjectState
                {
                    ObjectId = "interactive_object/" + shortHash,
                    ObjectName = "__LLMGC_OfflineGeoworldInteractive_"
                                 + Compact(source.CommandKind)
                                 + "_"
                                 + shortHash,
                    SourceCommandId = source.SourceCommandId,
                    CommandKind = source.CommandKind,
                    SourceChunkKey = source.SourceChunkKey,
                    GridX = source.GridX,
                    GridZ = source.GridZ,
                    Elevation = source.Elevation,
                    VisibleStepIndexes = visibleSteps ?? [],
                    MetadataOnly = source.MetadataOnly,
                    RawGeodataIncluded = source.RawGeodataIncluded
                };
            })
            .ToList();
    }

    private static IReadOnlyList<OfflineGeoworldInteractiveTravelStep> BuildMovementSamples(
        Goal104SourceContext context,
        IReadOnlyList<OfflineGeoworldInteractiveObjectState> objectStates)
    {
        var sourceObjectsByCommand = context.SourceObjectIndex.Objects
            .ToDictionary(item => item.SourceCommandId, item => item.ObjectId, StringComparer.Ordinal);
        var objectBySourceObjectId = objectStates.ToDictionary(
            item => context.SourceObjectIndex.Objects.Single(source => source.SourceCommandId == item.SourceCommandId).ObjectId,
            item => item,
            StringComparer.Ordinal);
        var sourceSteps = context.SourceSteps.Steps.OrderBy(item => item.StepIndex).ToList();
        var plan = new[]
        {
            (SourceIndex: 0, SampleId: "goal104_load_center", Kind: "load_manifest", PlayerX: 0, PlayerZ: 0, Band: false),
            (SourceIndex: 1, SampleId: "goal104_manual_move_east_band", Kind: "manual_move", PlayerX: 3, PlayerZ: 1, Band: true),
            (SourceIndex: 2, SampleId: "goal104_cross_east_boundary", Kind: "boundary_crossing", PlayerX: 6, PlayerZ: 1, Band: true),
            (SourceIndex: 2, SampleId: "goal104_prefetch_settle_east", Kind: "manual_move", PlayerX: 8, PlayerZ: 2, Band: false),
            (SourceIndex: 3, SampleId: "goal104_cross_west_return", Kind: "boundary_crossing", PlayerX: 4, PlayerZ: 2, Band: true),
            (SourceIndex: 0, SampleId: "goal104_manual_reset_center", Kind: "manual_move", PlayerX: 1, PlayerZ: 0, Band: false)
        };
        var allObjectIds = objectStates.Select(item => item.ObjectId).Order(StringComparer.Ordinal).ToList();
        var previousVisible = new SortedSet<string>(StringComparer.Ordinal);
        var previousHash = string.Empty;
        var result = new List<OfflineGeoworldInteractiveTravelStep>();

        for (var index = 0; index < plan.Length; index++)
        {
            var spec = plan[index];
            var sourceStep = sourceSteps[Math.Clamp(spec.SourceIndex, 0, sourceSteps.Count - 1)];
            var visible = sourceStep.VisibleObjectIds
                .Where(objectBySourceObjectId.ContainsKey)
                .Select(sourceId => objectBySourceObjectId[sourceId].ObjectId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var visibleSet = new SortedSet<string>(visible, StringComparer.Ordinal);
            var hidden = allObjectIds.Except(visibleSet, StringComparer.Ordinal).ToList();
            var newlyVisible = visibleSet.Except(previousVisible, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var newlyHidden = previousVisible.Except(visibleSet, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var activeChunks = sourceStep.ActiveChunkKeys.Count == 0
                ? sourceStep.VisibleObjectIds
                    .Where(objectBySourceObjectId.ContainsKey)
                    .Select(sourceId => objectBySourceObjectId[sourceId].SourceChunkKey)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToList()
                : sourceStep.ActiveChunkKeys.Order(StringComparer.Ordinal).ToList();
            if (activeChunks.Count == 0 && !string.IsNullOrWhiteSpace(sourceStep.CenterChunkKey))
            {
                activeChunks.Add(sourceStep.CenterChunkKey);
            }

            var prefetch = sourceStep.BoundaryPrefetchChunkKeys.Count == 0
                ? BuildBoundaryPrefetchChunks(sourceStep.CenterChunkKey, activeChunks)
                : sourceStep.BoundaryPrefetchChunkKeys.Order(StringComparer.Ordinal).ToList();
            var sample = new OfflineGeoworldInteractiveTravelStep
            {
                StepIndex = index,
                StepId = spec.SampleId,
                SourceGoal101StepId = sourceStep.StepId,
                SourceGoal103StepId = sourceStep.StepId,
                Action = spec.Kind,
                CenterChunkKey = sourceStep.CenterChunkKey,
                BoundaryBand = spec.Band,
                ActiveChunkKeys = activeChunks,
                BoundaryPrefetchChunkKeys = prefetch,
                VisibleObjectIds = visible,
                HiddenObjectIds = hidden,
                NewlyVisibleObjectIds = newlyVisible,
                NewlyHiddenObjectIds = newlyHidden,
                ExpectedVisibleObjectCount = visible.Count,
                PreviousStateHash = previousHash
            };
            var withHash = sample with
            {
                DeterministicStateHash = Hash(BuildStepHashSeed(sample))
            };
            result.Add(withHash);
            previousVisible = visibleSet;
            previousHash = withHash.DeterministicStateHash;
        }

        return result;
    }

    private static IReadOnlyList<OfflineGeoworldInteractiveChunkVisibilityStep> BuildChunkVisibility(
        IReadOnlyList<OfflineGeoworldInteractiveTravelStep> samples,
        IReadOnlyList<OfflineGeoworldInteractiveObjectState> objects)
    {
        var objectsById = objects.ToDictionary(item => item.ObjectId, item => item, StringComparer.Ordinal);
        return samples.Select(sample =>
        {
            var byChunk = new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            foreach (var chunk in sample.ActiveChunkKeys)
            {
                byChunk[chunk] = sample.VisibleObjectIds
                    .Where(id => objectsById.TryGetValue(id, out var obj)
                                 && string.Equals(obj.SourceChunkKey, chunk, StringComparison.Ordinal))
                    .Order(StringComparer.Ordinal)
                    .ToList();
            }

            return new OfflineGeoworldInteractiveChunkVisibilityStep
            {
                StepIndex = sample.StepIndex,
                StepId = sample.StepId,
                ActiveChunkKeys = sample.ActiveChunkKeys,
                BoundaryPrefetchChunkKeys = sample.BoundaryPrefetchChunkKeys,
                ActiveChunkCount = sample.ActiveChunkKeys.Count,
                BoundaryPrefetchChunkCount = sample.BoundaryPrefetchChunkKeys.Count,
                VisibleObjectIdsByChunk = byChunk
            };
        }).ToList();
    }

    private static IReadOnlyList<OfflineGeoworldInteractiveBoundaryZone> BuildBoundaryZones(
        IReadOnlyList<OfflineGeoworldInteractiveTravelStep> samples)
    {
        var result = new List<OfflineGeoworldInteractiveBoundaryZone>();
        foreach (var sample in samples.Where(item => item.Action == "boundary_crossing"))
        {
            var before = samples.Single(item => item.StepIndex == sample.StepIndex - 1);
            var axis = sample.StepIndex == 2 ? "x+" : "x-";
            result.Add(new OfflineGeoworldInteractiveBoundaryZone
            {
                CrossingIndex = result.Count,
                CrossingId = "goal104_boundary_crossing_" + result.Count.ToString("00"),
                FromMovementSampleIndex = before.StepIndex,
                ToMovementSampleIndex = sample.StepIndex,
                FromChunkKey = before.CenterChunkKey,
                ToChunkKey = sample.CenterChunkKey,
                BoundaryAxis = axis,
                ActiveChunkKeysBefore = before.ActiveChunkKeys,
                ActiveChunkKeysAfter = sample.ActiveChunkKeys,
                PrefetchChunkKeys = sample.BoundaryPrefetchChunkKeys,
                VisibleObjectIdsBefore = before.VisibleObjectIds,
                VisibleObjectIdsAfter = sample.VisibleObjectIds,
                NewlyVisibleObjectIds = sample.NewlyVisibleObjectIds,
                NewlyHiddenObjectIds = sample.NewlyHiddenObjectIds
            });
        }

        return result;
    }

    private static IReadOnlyList<OfflineGeoworldInteractivePrefetchPlan> BuildPrefetchPlans(
        IReadOnlyList<OfflineGeoworldInteractiveBoundaryZone> zones) =>
        zones.Select(zone =>
        {
            var beforePrefetch = BuildBoundaryPrefetchChunks(zone.FromChunkKey, zone.ActiveChunkKeysBefore);
            return new OfflineGeoworldInteractivePrefetchPlan
            {
                CrossingIndex = zone.CrossingIndex,
                CrossingId = zone.CrossingId,
                ActiveChunkKeysBefore = zone.ActiveChunkKeysBefore,
                ActiveChunkKeysAfter = zone.ActiveChunkKeysAfter,
                PrefetchChunkKeys = zone.PrefetchChunkKeys,
                AddedPrefetchChunkKeys = zone.PrefetchChunkKeys.Except(beforePrefetch, StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToList(),
                RemovedPrefetchChunkKeys = beforePrefetch.Except(zone.PrefetchChunkKeys, StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToList(),
                NewlyVisibleObjectIds = zone.NewlyVisibleObjectIds,
                NewlyHiddenObjectIds = zone.NewlyHiddenObjectIds
            };
        }).ToList();

    private static Goal104SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        var sourceRoot = OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot;
        var manifest = ReadSource<OfflineGeoworldPlayModeTravelManifest>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeTravelManifest();
        var sourceSteps = ReadSource<OfflineGeoworldPlayModeTravelStepsDocument>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeTravelStepsDocument();
        var sourceChunks = ReadSource<OfflineGeoworldPlayModeChunkVisibilityDocument>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeChunkVisibilityDocument();
        var sourceObjects = ReadSource<OfflineGeoworldPlayModeObjectStateIndex>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeObjectStateIndex();
        var sourceProof = ReadSource<OfflineGeoworldPlayModeSimulatedExecutionProof>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeSimulatedExecutionProof();
        var sourceQuality = ReadSource<OfflineGeoworldPlayModeQualityGateScan>(
            root,
            sourceRoot + "/" + OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
            diagnostics) ?? new OfflineGeoworldPlayModeQualityGateScan();

        var goal103Ready = manifest.Accepted == false
                           && manifest.ImplementationStatus == "GREEN"
                           && manifest.StepCount == 4
                           && manifest.ObjectCount == 18
                           && sourceSteps.StepCount == 4
                           && sourceObjects.ObjectCount == 18
                           && sourceProof.Passed
                           && sourceQuality.Passed;
        var alphaPath = Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.AlphaRuntimeBootstrapPath);
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = File.Exists(alphaPath)
            ? CountLines(File.ReadAllText(alphaPath, Encoding.UTF8))
            : 0;
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldInteractiveTravelPreviewVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldInteractiveTravelPreviewVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        AddIfFalse(goal103Ready, "goal104.source.goal103_ready", "Goal103", diagnostics);
        AddIfFalse(alphaUnchanged, "goal104.source.alpha_unchanged", "AlphaRuntimeBootstrap", diagnostics);
        return new Goal104SourceContext(
            manifest,
            sourceSteps,
            sourceChunks,
            sourceObjects,
            sourceProof,
            sourceQuality,
            goal103Ready,
            alphaUnchanged,
            SortDiagnostics(diagnostics));
    }

    private static string RenderReport(
        OfflineGeoworldInteractiveTravelReport report,
        OfflineGeoworldInteractiveQualityGateScan quality,
        OfflineGeoworldInteractiveSimulatedExecutionProof proof) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 104 Offline Geoworld Interactive Travel Preview",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal104 adds Unity Alpha interactive offline geoworld travel preview tooling over real Goal103 metadata. It remains metadata-only Alpha tooling and does not implement final Runtime gameplay, final art, live geodata fetching or release build behavior.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- movementSampleCount: " + report.MovementSampleCount,
            "- boundaryCrossingCount: " + report.BoundaryCrossingCount,
            "- prefetchPlanCount: " + report.PrefetchPlanCount,
            "- objectCount: " + report.ObjectCount,
            "- maxActiveChunkCount: " + report.MaxActiveChunkCount,
            "- maxBoundaryPrefetchChunkCount: " + report.MaxBoundaryPrefetchChunkCount,
            "- expectedVisibleObjectCountsBySample: " + string.Join(",", proof.ExpectedVisibleObjectCountsByStep),
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- interactivePayloadCreated: " + quality.InteractivePayloadCreated.ToString().ToLowerInvariant(),
            "- movementPathBuilt: " + quality.MovementPathBuilt.ToString().ToLowerInvariant(),
            "- boundaryZonesBuilt: " + quality.BoundaryZonesBuilt.ToString().ToLowerInvariant(),
            "- prefetchPlanBuilt: " + quality.PrefetchPlanBuilt.ToString().ToLowerInvariant(),
            "- boundaryPrefetchRepresented: " + quality.BoundaryPrefetchRepresented.ToString().ToLowerInvariant(),
            "- objectVisibilityDiffsBuilt: " + quality.ObjectVisibilityDiffsBuilt.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + report.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- editorWindowReady: " + report.EditorWindowReady.ToString().ToLowerInvariant(),
            "- simulatedExecutionProofPassed: " + report.SimulatedExecutionProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: " + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noAbsolutePaths: " + quality.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            "- noScenePrefabSettingsChanges: " + quality.NoScenePrefabSettingsChanges.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- manifestHash: " + report.ManifestHash,
            "- movementPathHash: " + report.MovementPathHash,
            "- boundaryZonesHash: " + report.BoundaryZonesHash,
            "- prefetchPlanHash: " + report.PrefetchPlanHash,
            "- unityScriptInventoryHash: " + report.UnityScriptInventoryHash,
            "- editorWindowInventoryHash: " + report.EditorWindowInventoryHash,
            "- simulatedExecutionProofHash: " + report.SimulatedExecutionProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private sealed record Goal104Payload(
        OfflineGeoworldInteractiveTravelManifest Manifest,
        OfflineGeoworldInteractiveTravelStepsDocument MovementPath,
        OfflineGeoworldInteractiveChunkVisibilityDocument BoundaryZones,
        OfflineGeoworldInteractiveObjectStateIndex PrefetchPlan,
        OfflineGeoworldInteractiveReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal104SourceContext(
        OfflineGeoworldPlayModeTravelManifest SourceManifest,
        OfflineGeoworldPlayModeTravelStepsDocument SourceSteps,
        OfflineGeoworldPlayModeChunkVisibilityDocument SourceChunks,
        OfflineGeoworldPlayModeObjectStateIndex SourceObjectIndex,
        OfflineGeoworldPlayModeSimulatedExecutionProof SourceProof,
        OfflineGeoworldPlayModeQualityGateScan SourceQuality,
        bool Goal103Ready,
        bool AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics);
}
