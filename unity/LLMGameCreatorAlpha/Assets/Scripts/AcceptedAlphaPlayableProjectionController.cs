using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class AcceptedAlphaPlayableProjectionController : MonoBehaviour
    {
        public const string GeneratedRootName = "__LLMGC_AcceptedAlphaPlayableProjection__";
        public const string UnityMenuPath = "LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection";
        public const string LegendObjectName = "goal120_legend";
        public const string DiagnosticsMarkerName = "goal120_diagnostics_marker";

        private const string BaselineId = "offline_geoworld_alpha_accepted_baseline_v1";
        private const string ManualGateAccepted = "ACCEPTED_BY_HUMAN";
        private const string ControllerSourceFile =
            "unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs";

        [SerializeField] private string statusLine = "Not loaded";
        [SerializeField] private string baselineId = string.Empty;
        [SerializeField] private string manualGateStatus = string.Empty;
        [SerializeField] private bool acceptedBaselineReady;
        [SerializeField] private bool goal116Accepted;
        [SerializeField] private int previewCommandCount;
        [SerializeField] private int chunkWindowStepCount;
        [SerializeField] private int boundaryCrossingCount;
        [SerializeField] private int interactionTargetCount;
        [SerializeField] private int objectiveCount;
        [SerializeField] private int completedObjectiveCount;
        [SerializeField] private int replayStepCount;
        [SerializeField] private int diagnosticsCount;
        [SerializeField] private int fatalErrorCount;
        [SerializeField] private string lastDiagnostics = string.Empty;
        [SerializeField] private string lastSmokeDiagnostics = string.Empty;

        public string StatusLine { get { return statusLine; } }
        public string LastDiagnostics { get { return lastDiagnostics; } }
        public string LastSmokeDiagnostics { get { return lastSmokeDiagnostics; } }

        private void Start()
        {
            RefreshAcceptedBaseline();
        }

        [ContextMenu("Refresh Accepted Baseline")]
        public void RefreshAcceptedBaseline()
        {
            var summary = LoadSummary();
            ApplySummary(summary);
            statusLine = acceptedBaselineReady
                ? "Goal119 accepted baseline ready: " + baselineId
                : "Goal119 accepted baseline incomplete: diagnostics=" + diagnosticsCount;
        }

        [ContextMenu("Build/Refresh Playable Projection")]
        public void BuildOrRefreshProjection()
        {
            fatalErrorCount = 0;
            try
            {
                var summary = LoadSummary();
                ApplySummary(summary);
                ClearChildren(transform);

                var map = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    "goal120_map_markers",
                    Vector3.zero);
                var systems = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    "goal120_system_markers",
                    new Vector3(18f, 0f, 0f));
                var interactions = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    "goal120_interaction_markers",
                    new Vector3(0f, 0f, 14f));
                var objectives = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    "goal120_objective_markers",
                    new Vector3(18f, 0f, 8f));
                var legend = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    "goal120_legend_diagnostics",
                    new Vector3(-7f, 0f, 0f));

                var player = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    map.transform,
                    "goal119_player_proxy",
                    PrimitiveType.Capsule,
                    new Color(0.95f, 0.95f, 0.95f),
                    new Vector3(6f, 0.8f, 6f),
                    new Vector3(0.7f, 1.4f, 0.7f));
                AttachDescriptor(player, "goal119_player_proxy", "goal119_player_proxy", "player", "goal119",
                    ControllerSourceFile, "Player proxy", acceptedBaselineReady ? "ready" : "blocked",
                    "Accepted Alpha player proxy for camera focus and scene selection.");

                RenderPreviewCommands(map.transform);
                RenderChunkAndBoundaryMarkers(systems.transform);
                RenderInteractionTargets(interactions.transform);
                RenderObjectives(objectives.transform);
                RenderReplayAndDiagnostics(legend.transform);
                BuildOrRefreshLegend(legend.transform);

                var smoke = RunLocalProjectionSmoke();
                statusLine = smoke
                    ? "Goal119 playable projection built and smoke passed"
                    : "Goal119 playable projection built; smoke incomplete";
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal119 playable projection fatal error: " + ex.GetType().Name;
                lastDiagnostics = statusLine + "\n" + ex.Message;
            }
        }

        [ContextMenu("Run Local Projection Smoke")]
        public bool RunLocalProjectionSmoke()
        {
            var result = new AcceptedAlphaProjectionSmokeResult
            {
                RootPresent = string.Equals(name, GeneratedRootName, System.StringComparison.Ordinal),
                BaselineLoaded = acceptedBaselineReady,
                PlayerProxyPresent = HasDescendantWithPrefix(transform, "goal119_player_proxy"),
                ChunkWindowMarkerPresent = HasDescendantWithPrefix(transform, "goal119_chunk_window"),
                InteractionOrObjectiveMarkerPresent =
                    HasDescendantWithPrefix(transform, "goal119_interaction_target")
                    || HasDescendantWithPrefix(transform, "goal119_objective"),
                DiagnosticsStatusPresent = HasDescendantWithPrefix(transform, DiagnosticsMarkerName),
                LegendPresent = HasDescendantWithPrefix(transform, LegendObjectName),
                MarkerDescriptorPresent = HasDescendantWithDescriptor(transform),
                SelectableInteractionTargetPresent = FindNextMarkerByKind("interaction", 0) != null,
                SelectableObjectivePresent = FindNextMarkerByKind("objective", 0) != null,
                MaterialWarningGuardPresent =
                    AcceptedAlphaPlayableProjectionPrimitiveFactory.MaterialWarningGuardPresent,
                ZeroFatalErrors = fatalErrorCount == 0,
                StatusLine = statusLine
            };
            lastSmokeDiagnostics = result.ToDiagnosticText();
            statusLine = result.Passed ? "Goal119 local projection smoke passed" : "Goal119 local projection smoke failed";
            return result.Passed;
        }

        public void ClearProjectionChildren()
        {
            ClearChildren(transform);
            statusLine = "Goal119 projection children cleared";
        }

        public GameObject FindPlayerProxy()
        {
            return FindNextMarkerByKind("player", 0);
        }

        public GameObject FindDiagnosticsMarker()
        {
            return FindDescendantObjectWithPrefix(transform, DiagnosticsMarkerName);
        }

        public GameObject FindNextMarkerByKind(string markerKind, int startIndex)
        {
            var matches = new List<GameObject>();
            CollectDescendantsWithDescriptorKind(transform, markerKind, matches);
            if (matches.Count == 0)
            {
                return null;
            }

            var index = Mathf.Abs(startIndex) % matches.Count;
            return matches[index];
        }

        public void ToggleOrRefreshLegend()
        {
            var legend = FindDescendantObjectWithPrefix(transform, LegendObjectName);
            if (legend != null)
            {
                legend.SetActive(!legend.activeSelf);
                statusLine = legend.activeSelf ? "Goal120 legend visible" : "Goal120 legend hidden";
                return;
            }

            var section = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                transform,
                "goal120_legend_diagnostics",
                new Vector3(-7f, 0f, 0f));
            BuildOrRefreshLegend(section.transform);
            statusLine = "Goal120 legend refreshed";
        }

        private AcceptedAlphaProjectionSummary LoadSummary()
        {
            var diagnostics = new List<string>();
            var summary = new AcceptedAlphaProjectionSummary();
            var repoRoot = AcceptedAlphaPlayableProjectionDiagnostics.ResolveRepositoryRoot(diagnostics);
            var streamingRoot = AcceptedAlphaPlayableProjectionDiagnostics.Combine(
                Application.streamingAssetsPath,
                "LLMGameCreator");

            var goal118Dashboard = ReadRepoFile(
                repoRoot,
                ".llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-dashboard.json",
                diagnostics);

            var goal116Record = ReadRepoFile(
                repoRoot,
                ".llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-record.json",
                diagnostics);
            summary.BaselineId = AcceptedAlphaPlayableProjectionDiagnostics.StringField(goal118Dashboard, "baselineId");
            summary.ManualGateStatus =
                AcceptedAlphaPlayableProjectionDiagnostics.StringField(goal118Dashboard, "manualGateStatus");
            summary.Goal116Accepted =
                AcceptedAlphaPlayableProjectionDiagnostics.StringField(goal116Record, "manualGateStatus")
                == ManualGateAccepted
                && AcceptedAlphaPlayableProjectionDiagnostics.BoolField(goal116Record, "humanAccepted")
                && !AcceptedAlphaPlayableProjectionDiagnostics.BoolField(goal116Record, "acceptedByCodex");
            summary.AcceptedBaselineReady =
                summary.BaselineId == BaselineId
                && summary.ManualGateStatus == ManualGateAccepted
                && AcceptedAlphaPlayableProjectionDiagnostics.BoolField(goal118Dashboard, "acceptedBaselineReady")
                && summary.Goal116Accepted;

            var commands = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal101",
                "offline-geoworld-preview-feature-commands.json",
                diagnostics);
            var steps = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal103",
                "offline-geoworld-playmode-steps.json",
                diagnostics);
            var boundaries = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal104",
                "offline-geoworld-interactive-boundary-zones.json",
                diagnostics);
            var targets = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal105",
                "offline-geoworld-interaction-targets.json",
                diagnostics);
            var session = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal106",
                "offline-geoworld-session-manifest.json",
                diagnostics);
            var objectives = ReadStreamingFile(
                streamingRoot,
                "OfflineGeoworldGoal107",
                "offline-geoworld-objectives.json",
                diagnostics);

            summary.PreviewCommandCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(commands, "commandCount");
            summary.ChunkWindowStepCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(steps, "stepCount");
            summary.BoundaryCrossingCount =
                AcceptedAlphaPlayableProjectionDiagnostics.IntField(boundaries, "boundaryCrossingCount");
            summary.InteractionTargetCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(targets, "targetCount");
            summary.ObjectiveCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(objectives, "objectiveCount");
            summary.CompletedObjectiveCount = CountCompletedObjectives(objectives);
            summary.ReplayStepCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(session, "replayStepCount");
            summary.Diagnostics.AddRange(diagnostics);
            return summary;
        }

        private void ApplySummary(AcceptedAlphaProjectionSummary summary)
        {
            baselineId = summary.BaselineId;
            manualGateStatus = summary.ManualGateStatus;
            acceptedBaselineReady = summary.AcceptedBaselineReady;
            goal116Accepted = summary.Goal116Accepted;
            previewCommandCount = summary.PreviewCommandCount;
            chunkWindowStepCount = summary.ChunkWindowStepCount;
            boundaryCrossingCount = summary.BoundaryCrossingCount;
            interactionTargetCount = summary.InteractionTargetCount;
            objectiveCount = summary.ObjectiveCount;
            completedObjectiveCount = summary.CompletedObjectiveCount;
            replayStepCount = summary.ReplayStepCount;
            diagnosticsCount = summary.Diagnostics.Count;
            lastDiagnostics = summary.Diagnostics.Count == 0
                ? "No diagnostics."
                : string.Join("\n", summary.Diagnostics.ToArray());
        }

        private void RenderPreviewCommands(Transform parent)
        {
            var json = ReadAcceptedStreamingPayload(
                "OfflineGeoworldGoal101",
                "offline-geoworld-preview-feature-commands.json");
            var index = 0;
            foreach (var command in LoadCommands(json))
            {
                var markerName =
                    "goal119_preview_" + AcceptedAlphaPlayableProjectionDiagnostics.Compact(command.CommandKind);
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    parent,
                    markerName,
                    AcceptedAlphaPlayableProjectionPrimitiveFactory.PrimitiveForKind(command.CommandKind),
                    AcceptedAlphaPlayableProjectionPrimitiveFactory.ColorForKind(command.CommandKind),
                    new Vector3(command.GridX, 0.1f + command.Elevation * 0.2f, command.GridZ),
                    AcceptedAlphaPlayableProjectionPrimitiveFactory.ScaleForKind(command.CommandKind));
                AttachDescriptor(marker, string.IsNullOrWhiteSpace(command.CommandId) ? markerName : command.CommandId,
                    markerName, "map", "goal101",
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/offline-geoworld-preview-feature-commands.json",
                    command.CommandKind, "ready", "sourceChunkKey=" + command.SourceChunkKey + "; styleKey=" + command.StyleKey);
                if (index < 8)
                {
                    var label = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                        parent,
                        "goal120_label_preview_" + index,
                        command.CommandKind,
                        new Vector3(command.GridX + 0.6f, 0.85f + command.Elevation * 0.2f, command.GridZ),
                        Color.white,
                        0.22f);
                    AttachGoal120Descriptor(label, "goal120_label_preview_" + index, "label",
                        command.CommandKind, "ready", "Readable label for accepted alpha map marker.");
                }

                index++;
            }
        }

        private void RenderChunkAndBoundaryMarkers(Transform parent)
        {
            var count = Mathf.Max(1, chunkWindowStepCount);
            for (var i = 0; i < count; i++)
            {
                var markerName = "goal119_chunk_window_step_" + i;
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    parent,
                    markerName,
                    PrimitiveType.Cube,
                    new Color(0.3f, 0.65f, 0.9f),
                    new Vector3(0f, 0.1f, i * 1.2f),
                    new Vector3(1.5f, 0.08f, 0.85f));
                AttachDescriptor(marker, markerName, markerName, "system", "goal103",
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/offline-geoworld-playmode-steps.json",
                    "Chunk window step " + i, "ready", "Goal103 play mode travel chunk/window marker.");
            }

            for (var i = 0; i < boundaryCrossingCount; i++)
            {
                var markerName = "goal119_boundary_prefetch_" + i;
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    parent,
                    markerName,
                    PrimitiveType.Cube,
                    new Color(0.95f, 0.45f, 0.2f),
                    new Vector3(2.5f, 0.1f, i * 1.2f),
                    new Vector3(1f, 0.08f, 0.85f));
                AttachDescriptor(marker, markerName, markerName, "system", "goal104",
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/offline-geoworld-interactive-boundary-zones.json",
                    "Boundary prefetch " + i, "ready", "Goal104 boundary crossing marker.");
            }
        }

        private void RenderInteractionTargets(Transform parent)
        {
            var json = ReadAcceptedStreamingPayload(
                "OfflineGeoworldGoal105",
                "offline-geoworld-interaction-targets.json");
            var index = 0;
            foreach (var target in LoadTargets(json))
            {
                var markerName =
                    "goal119_interaction_target_" + AcceptedAlphaPlayableProjectionDiagnostics.Compact(target.TargetId);
                var labelText = string.IsNullOrWhiteSpace(target.TargetName)
                    ? target.TargetId
                    : target.TargetName;
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    parent,
                    markerName,
                    PrimitiveType.Sphere,
                    new Color(1f, 0.86f, 0.24f),
                    new Vector3(target.GridX, 0.5f + target.Elevation * 0.2f, target.GridZ),
                    Vector3.one * Mathf.Max(0.45f, target.InteractionRadius * 0.18f));
                AttachDescriptor(marker, target.TargetId, markerName, "interaction", "goal105",
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/offline-geoworld-interaction-targets.json",
                    labelText, "selectable", "commandKind=" + target.CommandKind + "; radius=" + target.InteractionRadius);
                var label = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    parent,
                    "goal120_interaction_label_" + index,
                    labelText,
                    new Vector3(target.GridX + 0.5f, 1f + target.Elevation * 0.2f, target.GridZ),
                    Color.yellow,
                    0.24f);
                AttachGoal120Descriptor(label, "goal120_interaction_label_" + index, "label",
                    labelText, "ready", "Readable label for selectable interaction target.");
                index++;
            }
        }

        private void RenderObjectives(Transform parent)
        {
            var json = ReadAcceptedStreamingPayload(
                "OfflineGeoworldGoal107",
                "offline-geoworld-objectives.json");
            var index = 0;
            foreach (var objective in LoadObjectives(json))
            {
                var markerName =
                    "goal119_objective_" + AcceptedAlphaPlayableProjectionDiagnostics.Compact(objective.ObjectiveId);
                var label = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    parent,
                    markerName,
                    objective.ObjectiveId + " [" + objective.CompletionState + "]",
                    new Vector3(4f, 1.2f + index * 0.55f, 0f),
                    objective.CompletionState == "completed" ? new Color(0.4f, 1f, 0.55f) : Color.yellow,
                    0.28f);
                AttachDescriptor(label, objective.ObjectiveId, markerName, "objective", "goal107",
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/offline-geoworld-objectives.json",
                    string.IsNullOrWhiteSpace(objective.Title) ? objective.ObjectiveId : objective.Title,
                    objective.CompletionState, "Accepted alpha objective checklist entry.");
                index++;
            }
        }

        private void RenderReplayAndDiagnostics(Transform parent)
        {
            var replay = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                "goal119_replay_checkpoint_status",
                "Goal106 replay steps=" + replayStepCount,
                new Vector3(4f, 0.4f, 5f),
                Color.cyan,
                0.3f);
            AttachDescriptor(replay, "goal119_replay_checkpoint_status", "goal119_replay_checkpoint_status",
                "diagnostics", "goal106",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/offline-geoworld-session-manifest.json",
                "Replay checkpoint status", "ready", "Goal106 replay step count.");
            var diagnostics = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                DiagnosticsMarkerName,
                "Goal119 baseline=" + acceptedBaselineReady
                + " commands=" + previewCommandCount
                + " objectives=" + completedObjectiveCount + "/" + objectiveCount
                + " diagnostics=" + diagnosticsCount
                + " fatalErrors=" + fatalErrorCount,
                new Vector3(4f, 0.8f, 5f),
                fatalErrorCount == 0 ? Color.white : Color.red,
                0.28f);
            AttachGoal120Descriptor(diagnostics, DiagnosticsMarkerName, "diagnostics",
                "Projection diagnostics", fatalErrorCount == 0 ? "ready" : "blocked",
                "Goal120 selectable diagnostics marker.");
        }

        private void BuildOrRefreshLegend(Transform parent)
        {
            var legend = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                LegendObjectName,
                new Vector3(0f, 0f, 0f));
            var legendComponent = legend.AddComponent<AcceptedAlphaPlayableProjectionLegend>();
            legendComponent.Configure(
                "White capsule: player proxy",
                "Blue/orange cubes: chunk windows and boundary prefetch",
                "Yellow spheres: selectable interaction targets",
                "Green/yellow text: accepted objectives",
                "Cyan/white text: replay and diagnostics");
            AttachGoal120Descriptor(legend, LegendObjectName, "legend", "Projection legend", "ready",
                "Readable Goal120 legend under the generated projection root.");

            for (var i = 0; i < legendComponent.Entries.Length; i++)
            {
                var text = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    legend.transform,
                    "goal120_legend_line_" + i,
                    legendComponent.Entries[i],
                    new Vector3(0f, 1.4f - i * 0.42f, 0f),
                    Color.white,
                    0.24f);
                AttachGoal120Descriptor(text, "goal120_legend_line_" + i, "label",
                    legendComponent.Entries[i], "ready", "Legend line.");
            }
        }

        private static void AttachGoal120Descriptor(
            GameObject target, string markerId, string markerKind, string displayLabel, string status, string details) =>
            AttachDescriptor(target, markerId, target.name, markerKind, "goal120", ControllerSourceFile,
                displayLabel, status, details);

        private static void AttachDescriptor(
            GameObject target,
            string markerId,
            string markerName,
            string markerKind,
            string sourceGoal,
            string sourceFile,
            string displayLabel,
            string status,
            string details)
        {
            AcceptedAlphaPlayableProjectionPrimitiveFactory.AttachDescriptor(
                target,
                markerId,
                markerName,
                markerKind,
                sourceGoal,
                sourceFile,
                displayLabel,
                status,
                details);
        }

        private List<AcceptedAlphaProjectionCommand> LoadCommands(string json)
        {
            var commands = new List<AcceptedAlphaProjectionCommand>();
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(json, "commandId"))
            {
                commands.Add(new AcceptedAlphaProjectionCommand
                {
                    CommandId = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "commandId"),
                    CommandKind = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "commandKind"),
                    StyleKey = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "styleKey"),
                    SourceChunkKey = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "sourceChunkKey"),
                    GridX = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "gridX"),
                    GridZ = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "gridZ"),
                    Elevation = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "elevation")
                });
            }

            return commands;
        }

        private List<AcceptedAlphaProjectionTarget> LoadTargets(string json)
        {
            var targets = new List<AcceptedAlphaProjectionTarget>();
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(json, "targetId"))
            {
                targets.Add(new AcceptedAlphaProjectionTarget
                {
                    TargetId = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "targetId"),
                    TargetName = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "targetName"),
                    CommandKind = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "commandKind"),
                    GridX = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "gridX"),
                    GridZ = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "gridZ"),
                    Elevation = AcceptedAlphaPlayableProjectionDiagnostics.IntField(block, "elevation"),
                    InteractionRadius = AcceptedAlphaPlayableProjectionDiagnostics.FloatField(block, "interactionRadius")
                });
            }

            return targets;
        }

        private List<AcceptedAlphaProjectionObjective> LoadObjectives(string json)
        {
            var objectives = new List<AcceptedAlphaProjectionObjective>();
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(json, "objectiveId"))
            {
                objectives.Add(new AcceptedAlphaProjectionObjective
                {
                    ObjectiveId = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "objectiveId"),
                    Title = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "title"),
                    CompletionState = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "completionState")
                });
            }

            return objectives;
        }

        private static int CountCompletedObjectives(string json)
        {
            var count = 0;
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(json, "objectiveId"))
            {
                if (AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "completionState") == "completed")
                {
                    count++;
                }
            }

            return count;
        }

        private static string ReadRepoFile(string repoRoot, string relativePath, List<string> diagnostics)
        {
            if (string.IsNullOrWhiteSpace(repoRoot))
            {
                return string.Empty;
            }

            return AcceptedAlphaPlayableProjectionDiagnostics.ReadRequiredFile(
                Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)),
                "goal119_repo_file",
                diagnostics);
        }

        private static string ReadStreamingFile(
            string streamingRoot,
            string goalFolder,
            string fileName,
            List<string> diagnostics) =>
            AcceptedAlphaPlayableProjectionDiagnostics.ReadRequiredFile(
                AcceptedAlphaPlayableProjectionDiagnostics.Combine(streamingRoot, goalFolder, fileName),
                "goal119_streaming_file",
                diagnostics);

        private static string ReadAcceptedStreamingPayload(string goalFolder, string fileName)
        {
            var path = AcceptedAlphaPlayableProjectionDiagnostics.Combine(
                Application.streamingAssetsPath,
                "LLMGameCreator",
                goalFolder,
                fileName);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        private static void ClearChildren(Transform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static bool HasDescendantWithPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return true;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (HasDescendantWithPrefix(root.GetChild(i), prefix))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDescendantWithDescriptor(Transform root)
        {
            if (root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>() != null)
            {
                return true;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (HasDescendantWithDescriptor(root.GetChild(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject FindDescendantObjectWithPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var match = FindDescendantObjectWithPrefix(root.GetChild(i), prefix);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void CollectDescendantsWithDescriptorKind(
            Transform root,
            string markerKind,
            List<GameObject> matches)
        {
            var descriptor = root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor != null
                && string.Equals(descriptor.MarkerKind, markerKind, System.StringComparison.Ordinal))
            {
                matches.Add(root.gameObject);
            }

            for (var i = 0; i < root.childCount; i++)
            {
                CollectDescendantsWithDescriptorKind(root.GetChild(i), markerKind, matches);
            }
        }
    }
}
