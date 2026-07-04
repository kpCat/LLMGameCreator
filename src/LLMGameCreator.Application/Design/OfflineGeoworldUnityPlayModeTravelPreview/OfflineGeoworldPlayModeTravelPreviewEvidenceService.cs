using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

public sealed partial class OfflineGeoworldPlayModeTravelPreviewEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldPlayModeBuildResult Build(string repositoryRootPath)
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
        var closure = BuildGoal102BClosure(context);
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
            closure);
        return BuildResult(payload, scripts, editor, proof, negative, binding, lineage, closure, quality);
    }

    public async Task<OfflineGeoworldPlayModeWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.StreamingAssetsRelativeRoot);
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
        var closure = BuildGoal102BClosure(context);
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
            closure);
        var result = BuildResult(mirrored, scripts, editor, proof, negative, binding, lineage, closure, quality);

        var outputDirectory = Resolve(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.RelativeOutputDirectory);
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
            OfflineGeoworldPlayModeTravelPreviewVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldPlayModeWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldPlayModeBuildResult BuildResult(
        Goal103Payload payload,
        OfflineGeoworldPlayModeUnityScriptInventory scripts,
        OfflineGeoworldPlayModeEditorWindowInventory editor,
        OfflineGeoworldPlayModeSimulatedExecutionProof proof,
        OfflineGeoworldPlayModeNegativeProof negative,
        OfflineGeoworldPlayModeWorkspaceBindingInventory binding,
        OfflineGeoworldPlayModeSourceLineage lineage,
        Goal102BFalsePositiveClosure closure,
        OfflineGeoworldPlayModeQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(scripts, editor, proof, negative, binding, lineage, closure, quality);
        var reportWithoutHash = BuildReport(payload, scripts, editor, proof, negative, binding, closure, quality, evidence);
        var markdownWithoutHash = RenderReport(reportWithoutHash, quality, proof, closure);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(markdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof, closure);
        return new OfflineGeoworldPlayModeBuildResult
        {
            Manifest = payload.Manifest,
            Steps = payload.Steps,
            ChunkVisibility = payload.ChunkVisibility,
            ObjectStateIndex = payload.ObjectStateIndex,
            Readme = payload.Readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedExecutionProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            Goal102BClosure = closure,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payload.PayloadFiles,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal103Payload BuildPayload(Goal103SourceContext context)
    {
        var objectStates = BuildObjectStates(context);
        var steps = BuildTravelSteps(context, objectStates);
        var chunkVisibility = BuildChunkVisibility(steps, objectStates);
        var stepsDocument = new OfflineGeoworldPlayModeTravelStepsDocument
        {
            StepCount = steps.Count,
            Steps = steps
        };
        var objectIndex = new OfflineGeoworldPlayModeObjectStateIndex
        {
            ObjectCount = objectStates.Count,
            Objects = objectStates
        };
        var chunkDocument = new OfflineGeoworldPlayModeChunkVisibilityDocument
        {
            StepCount = chunkVisibility.Count,
            Steps = chunkVisibility
        };
        var readme = new OfflineGeoworldPlayModeReadme();
        var stepsJson = Serialize(stepsDocument);
        var chunkJson = Serialize(chunkDocument);
        var objectJson = Serialize(objectIndex);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldPlayModeTravelManifest
        {
            PayloadFileCount = OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames.Count,
            StepCount = steps.Count,
            ObjectCount = objectStates.Count,
            SourceCommandCount = context.Commands.CommandCount,
            SourceTravelWindowStepCount = context.Travel.StepCount,
            MaxActiveChunkCount = steps.Count == 0 ? 0 : steps.Max(item => item.ActiveChunkKeys.Count),
            MaxBoundaryPrefetchChunkCount = steps.Count == 0 ? 0 : steps.Max(item => item.BoundaryPrefetchChunkKeys.Count),
            Goal102BFalsePositiveClosureRecorded = context.Goal102BClosureInputsPresent
                                                    && context.Goal102BActualHeadBeforeMalformedDetected == false
                                                    && context.Goal102BWorkingTreeSourceReadable,
            AlphaRuntimeBootstrapUnchanged = context.Goal102BAlphaRuntimeBootstrapUnchanged,
            StepsHash = Hash(stepsJson),
            ChunkVisibilityHash = Hash(chunkJson),
            ObjectStateIndexHash = Hash(objectJson),
            ReadmeHash = Hash(readmeJson)
        };
        var manifestJson = Serialize(manifest);
        return new Goal103Payload(
            manifest,
            stepsDocument,
            chunkDocument,
            objectIndex,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName] = manifestJson,
                [OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName] = stepsJson,
                [OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName] = chunkJson,
                [OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName] = objectJson,
                [OfflineGeoworldPlayModeTravelPreviewVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static IReadOnlyList<OfflineGeoworldPlayModeObjectState> BuildObjectStates(
        Goal103SourceContext context)
    {
        var visibleByCommand = context.Travel.Steps
            .SelectMany(step => step.VisibleCommandIds.Select(commandId => (commandId, step.StepIndex)))
            .GroupBy(item => item.commandId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.StepIndex).Distinct().Order().ToList(),
                StringComparer.Ordinal);

        return context.Commands.Commands
            .OrderBy(command => command.CommandId, StringComparer.Ordinal)
            .Select(command =>
            {
                visibleByCommand.TryGetValue(command.CommandId, out var visibleSteps);
                var shortHash = Hash(command.CommandId)[..12];
                return new OfflineGeoworldPlayModeObjectState
                {
                    ObjectId = "playmode_object/" + shortHash,
                    ObjectName = "__LLMGC_OfflineGeoworldPlayMode_"
                                 + Compact(command.CommandKind)
                                 + "_"
                                 + shortHash,
                    SourceCommandId = command.CommandId,
                    CommandKind = command.CommandKind,
                    SourceChunkKey = command.SourceChunkKey,
                    GridX = command.GridX,
                    GridZ = command.GridZ,
                    Elevation = command.Elevation,
                    VisibleStepIndexes = visibleSteps ?? [],
                    MetadataOnly = command.MetadataOnly,
                    RawGeodataIncluded = command.RawGeodataIncluded
                };
            })
            .ToList();
    }

    private static IReadOnlyList<OfflineGeoworldPlayModeTravelStep> BuildTravelSteps(
        Goal103SourceContext context,
        IReadOnlyList<OfflineGeoworldPlayModeObjectState> objectStates)
    {
        var objectByCommand = objectStates.ToDictionary(
            item => item.SourceCommandId,
            item => item,
            StringComparer.Ordinal);
        var allObjectIds = objectStates.Select(item => item.ObjectId)
            .Order(StringComparer.Ordinal)
            .ToList();
        var previousVisible = new SortedSet<string>(StringComparer.Ordinal);
        var previousHash = string.Empty;
        var result = new List<OfflineGeoworldPlayModeTravelStep>();

        foreach (var sourceStep in context.Travel.Steps.OrderBy(item => item.StepIndex))
        {
            var visible = sourceStep.VisibleCommandIds
                .Where(objectByCommand.ContainsKey)
                .Select(commandId => objectByCommand[commandId].ObjectId)
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
            var activeChunks = sourceStep.VisibleCommandIds
                .Where(objectByCommand.ContainsKey)
                .Select(commandId => objectByCommand[commandId].SourceChunkKey)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            if (activeChunks.Count == 0 && !string.IsNullOrWhiteSpace(sourceStep.CenterChunkKey))
            {
                activeChunks.Add(sourceStep.CenterChunkKey);
            }

            var prefetch = BuildBoundaryPrefetchChunks(sourceStep.CenterChunkKey, activeChunks);
            var stepSeed = string.Join(
                "|",
                previousHash,
                sourceStep.StepIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
                sourceStep.StepId,
                sourceStep.Action,
                sourceStep.CenterChunkKey,
                string.Join(",", activeChunks),
                string.Join(",", prefetch),
                string.Join(",", visible),
                string.Join(",", newlyVisible),
                string.Join(",", newlyHidden));
            var stateHash = Hash(stepSeed);
            result.Add(new OfflineGeoworldPlayModeTravelStep
            {
                StepIndex = sourceStep.StepIndex,
                StepId = "goal103_" + sourceStep.StepId,
                SourceGoal101StepId = sourceStep.StepId,
                Action = sourceStep.Action,
                CenterChunkKey = sourceStep.CenterChunkKey,
                ActiveChunkKeys = activeChunks,
                BoundaryPrefetchChunkKeys = prefetch,
                VisibleObjectIds = visible,
                HiddenObjectIds = hidden,
                NewlyVisibleObjectIds = newlyVisible,
                NewlyHiddenObjectIds = newlyHidden,
                ExpectedVisibleObjectCount = visible.Count,
                PreviousStateHash = previousHash,
                DeterministicStateHash = stateHash
            });
            previousVisible = visibleSet;
            previousHash = stateHash;
        }

        return result;
    }

    private static IReadOnlyList<OfflineGeoworldPlayModeChunkVisibilityStep> BuildChunkVisibility(
        IReadOnlyList<OfflineGeoworldPlayModeTravelStep> steps,
        IReadOnlyList<OfflineGeoworldPlayModeObjectState> objects)
    {
        var objectsById = objects.ToDictionary(item => item.ObjectId, item => item, StringComparer.Ordinal);
        return steps.Select(step =>
        {
            var byChunk = new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            foreach (var chunk in step.ActiveChunkKeys)
            {
                byChunk[chunk] = step.VisibleObjectIds
                    .Where(id => objectsById.TryGetValue(id, out var obj)
                                 && string.Equals(obj.SourceChunkKey, chunk, StringComparison.Ordinal))
                    .Order(StringComparer.Ordinal)
                    .ToList();
            }

            return new OfflineGeoworldPlayModeChunkVisibilityStep
            {
                StepIndex = step.StepIndex,
                StepId = step.StepId,
                ActiveChunkKeys = step.ActiveChunkKeys,
                BoundaryPrefetchChunkKeys = step.BoundaryPrefetchChunkKeys,
                ActiveChunkCount = step.ActiveChunkKeys.Count,
                BoundaryPrefetchChunkCount = step.BoundaryPrefetchChunkKeys.Count,
                VisibleObjectIdsByChunk = byChunk
            };
        }).ToList();
    }

    private static Goal103SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        using var commandDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName,
            diagnostics);
        using var travelDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
            diagnostics);
        using var manifestDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName,
            diagnostics);
        using var qualityDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
            diagnostics);
        using var goal102Report = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102SourceRoot
            + "/"
            + OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
            diagnostics);
        using var beforeAfterDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BSourceRoot
            + "/"
            + OfflineGeoworldActualUnityEditorSourceReformatVocabulary.BeforeAfterFileName,
            diagnostics);
        using var trustAuditDoc = ReadJson(
            root,
            OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BSourceRoot
            + "/"
            + OfflineGeoworldActualUnityEditorSourceReformatVocabulary.TrustAuditFileName,
            diagnostics);

        var commands = commandDoc is null
            ? new OfflineGeoworldPreviewFeatureCommandCatalog()
            : Deserialize<OfflineGeoworldPreviewFeatureCommandCatalog>(commandDoc.RootElement.GetRawText())
              ?? new OfflineGeoworldPreviewFeatureCommandCatalog();
        var travel = travelDoc is null
            ? new OfflineGeoworldPreviewTravelWindowScript()
            : Deserialize<OfflineGeoworldPreviewTravelWindowScript>(travelDoc.RootElement.GetRawText())
              ?? new OfflineGeoworldPreviewTravelWindowScript();

        var goal101AcceptedFalse = manifestDoc is not null
                                   && !TryGetBool(manifestDoc.RootElement, "accepted");
        var goal101Counts = commands.CommandCount == 18
                            && commands.CommandKindCount == 10
                            && travel.StepCount >= 4;
        var goal101Quality = qualityDoc is not null && TryGetBool(qualityDoc.RootElement, "passed");
        var goal102Quality = goal102Report is not null && TryGetBool(goal102Report.RootElement, "passed");
        var goal102BInputsPresent = beforeAfterDoc is not null && trustAuditDoc is not null;
        var actualHeadRead = beforeAfterDoc is not null
                             && TryGetBool(beforeAfterDoc.RootElement, "actualHeadBeforeBlobRead");
        var actualHeadMalformed = beforeAfterDoc is not null
                                  && TryGetBool(beforeAfterDoc.RootElement, "actualHeadBeforeMalformedDetected");
        var workingTreeReadable = beforeAfterDoc is not null
                                  && TryGetBool(beforeAfterDoc.RootElement, "workingTreeSourceReadable");
        var rawPhysicalLines = beforeAfterDoc is null
            ? 0
            : TryGetNestedInt(beforeAfterDoc.RootElement, "actualHeadBefore", "rawPhysicalLineCount");
        var maxPhysicalLine = beforeAfterDoc is null
            ? 0
            : TryGetNestedInt(beforeAfterDoc.RootElement, "actualHeadBefore", "rawPhysicalMaxLineLength");
        var goal102BAlpha = beforeAfterDoc is not null
                            && TryGetNestedBool(beforeAfterDoc.RootElement, "alphaRuntimeBootstrap", "unchanged");
        var goal102BTrust = trustAuditDoc is not null
                            && TryGetBool(trustAuditDoc.RootElement, "goal102AEvidenceTrustDefectRecorded")
                            && TryGetBool(trustAuditDoc.RootElement, "goal102AEvidenceConflictsWithActualHead");

        AddIfFalse(goal101AcceptedFalse, "goal103.source.goal101_accepted", "Goal101", diagnostics);
        AddIfFalse(goal101Counts, "goal103.source.goal101_counts", "Goal101", diagnostics);
        AddIfFalse(goal101Quality, "goal103.source.goal101_quality", "Goal101", diagnostics);
        AddIfFalse(goal102Quality, "goal103.source.goal102_quality", "Goal102", diagnostics);
        AddIfFalse(goal102BInputsPresent, "goal103.source.goal102b_inputs", "Goal102B", diagnostics);
        AddIfFalse(actualHeadRead, "goal103.source.goal102b_actual_read", "Goal102B", diagnostics);
        AddIfFalse(!actualHeadMalformed, "goal103.source.goal102b_false_positive", "Goal102B", diagnostics);
        AddIfFalse(workingTreeReadable, "goal103.source.goal102b_worktree", "Goal102B", diagnostics);
        AddIfFalse(goal102BTrust, "goal103.source.goal102b_trust", "Goal102B", diagnostics);
        AddIfFalse(goal102BAlpha, "goal103.source.goal102b_alpha", "Goal102B", diagnostics);

        return new Goal103SourceContext(
            Commands: commands,
            Travel: travel,
            Goal101AcceptedFalse: goal101AcceptedFalse,
            Goal101CountsProven: goal101Counts,
            Goal101QualityGatePassed: goal101Quality,
            Goal102QualityGatePassed: goal102Quality,
            Goal102BClosureInputsPresent: goal102BInputsPresent,
            Goal102BActualHeadBeforeBlobRead: actualHeadRead,
            Goal102BActualHeadBeforeMalformedDetected: actualHeadMalformed,
            Goal102BWorkingTreeSourceReadable: workingTreeReadable,
            Goal102BActualHeadRawPhysicalLineCount: rawPhysicalLines,
            Goal102BActualHeadMaxPhysicalLineLength: maxPhysicalLine,
            Goal102BAlphaRuntimeBootstrapUnchanged: goal102BAlpha,
            Goal102BTrustDefectRecorded: goal102BTrust,
            Diagnostics: SortDiagnostics(diagnostics));
    }

    private static string RenderReport(
        OfflineGeoworldPlayModeTravelReport report,
        OfflineGeoworldPlayModeQualityGateScan quality,
        OfflineGeoworldPlayModeSimulatedExecutionProof proof,
        Goal102BFalsePositiveClosure closure) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 103 Offline Geoworld Play Mode Travel Preview",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal103 adds Unity Alpha play-mode travel preview tooling over real Goal101 metadata and closes the Goal102B product/source blocker as a false-positive proceed decision without marking Goal102B GREEN. It does not implement final Runtime gameplay, final art, real geodata fetching or release build behavior.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- stepCount: " + report.StepCount,
            "- objectCount: " + report.ObjectCount,
            "- maxActiveChunkCount: " + report.MaxActiveChunkCount,
            "- maxBoundaryPrefetchChunkCount: " + report.MaxBoundaryPrefetchChunkCount,
            "- expectedVisibleObjectCountsByStep: " + string.Join(",", proof.ExpectedVisibleObjectCountsByStep),
            string.Empty,
            "## Goal102B Closure",
            string.Empty,
            "- goal102bRemainsBlocked: " + closure.Goal102BRemainsBlocked.ToString().ToLowerInvariant(),
            "- productSourceBlockerClosed: " + closure.ProductSourceBlockerClosed.ToString().ToLowerInvariant(),
            "- actualHeadBeforeMalformedDetected: " + closure.ActualHeadBeforeMalformedDetected.ToString().ToLowerInvariant(),
            "- workingTreeSourceReadable: " + closure.WorkingTreeSourceReadable.ToString().ToLowerInvariant(),
            "- futureGatesRequireActualTargetBytes: " + closure.FutureGatesRequireActualTargetBytes.ToString().ToLowerInvariant(),
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- playModePayloadCreated: " + quality.PlayModePayloadCreated.ToString().ToLowerInvariant(),
            "- travelStepPlanBuilt: " + quality.TravelStepPlanBuilt.ToString().ToLowerInvariant(),
            "- boundaryPrefetchRepresented: " + quality.BoundaryPrefetchRepresented.ToString().ToLowerInvariant(),
            "- objectVisibilityDiffsBuilt: " + quality.ObjectVisibilityDiffsBuilt.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + report.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- editorWindowReady: " + report.EditorWindowReady.ToString().ToLowerInvariant(),
            "- simulatedExecutionProofPassed: " + report.SimulatedExecutionProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- goal102bClosureRecorded: " + report.Goal102BClosureRecorded.ToString().ToLowerInvariant(),
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
            "- stepsHash: " + report.StepsHash,
            "- chunkVisibilityHash: " + report.ChunkVisibilityHash,
            "- objectStateIndexHash: " + report.ObjectStateIndexHash,
            "- unityScriptInventoryHash: " + report.UnityScriptInventoryHash,
            "- editorWindowInventoryHash: " + report.EditorWindowInventoryHash,
            "- simulatedExecutionProofHash: " + report.SimulatedExecutionProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash,
            "- goal102bClosureHash: " + report.Goal102BClosureHash
        ]) + Environment.NewLine;

    private sealed record Goal103Payload(
        OfflineGeoworldPlayModeTravelManifest Manifest,
        OfflineGeoworldPlayModeTravelStepsDocument Steps,
        OfflineGeoworldPlayModeChunkVisibilityDocument ChunkVisibility,
        OfflineGeoworldPlayModeObjectStateIndex ObjectStateIndex,
        OfflineGeoworldPlayModeReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal103SourceContext(
        OfflineGeoworldPreviewFeatureCommandCatalog Commands,
        OfflineGeoworldPreviewTravelWindowScript Travel,
        bool Goal101AcceptedFalse,
        bool Goal101CountsProven,
        bool Goal101QualityGatePassed,
        bool Goal102QualityGatePassed,
        bool Goal102BClosureInputsPresent,
        bool Goal102BActualHeadBeforeBlobRead,
        bool Goal102BActualHeadBeforeMalformedDetected,
        bool Goal102BWorkingTreeSourceReadable,
        int Goal102BActualHeadRawPhysicalLineCount,
        int Goal102BActualHeadMaxPhysicalLineLength,
        bool Goal102BAlphaRuntimeBootstrapUnchanged,
        bool Goal102BTrustDefectRecorded,
        IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics);
}
