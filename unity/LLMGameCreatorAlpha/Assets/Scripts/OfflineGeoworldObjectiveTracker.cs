using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldObjectiveTracker : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal107";
        private const string ManifestFileName = "offline-geoworld-objective-manifest.json";
        private const string ObjectivesFileName = "offline-geoworld-objectives.json";
        private const string AcceptanceRunFileName = "offline-geoworld-objective-acceptance-run.json";
        private const string CompletionStateFileName = "offline-geoworld-objective-completion-state.json";
        private const string ReplayProofFileName = "offline-geoworld-objective-replay-acceptance-proof.json";

        [SerializeField] private int objectiveCount;
        [SerializeField] private int completedObjectiveCount;
        [SerializeField] private int currentObjectiveIndex;
        [SerializeField] private int replayStepCount;
        [SerializeField] private int stateDeltaCount;
        [SerializeField] private int checkpointStepIndex;
        [SerializeField] private string finalStatus = string.Empty;
        [SerializeField] private string finalStateHash = string.Empty;
        [SerializeField] private string finalObjectiveAcceptanceHash = string.Empty;
        [SerializeField] private string payloadRoot = RelativeRoot;
        [SerializeField] private string statusLine = string.Empty;

        private readonly List<OfflineGeoworldObjectiveState> objectives =
            new List<OfflineGeoworldObjectiveState>();
        private readonly HashSet<string> completedObjectiveIds =
            new HashSet<string>(StringComparer.Ordinal);
        private OfflineGeoworldInteractionController interactionController;
        private OfflineGeoworldSessionReplayController replayController;
        private OfflineGeoworldSessionSaveLoadController saveLoadController;

        public int ObjectiveCount { get { return objectiveCount; } }
        public int CompletedObjectiveCount { get { return completedObjectiveCount; } }
        public int CurrentObjectiveIndex { get { return currentObjectiveIndex; } }
        public string FinalStatus { get { return finalStatus; } }
        public string FinalObjectiveAcceptanceHash { get { return finalObjectiveAcceptanceHash; } }
        public string LastStatus { get { return statusLine; } }

        private void Awake()
        {
            BindOptionalControllers();
        }

        private void Start()
        {
            RefreshPayloadStatus();
        }

        [ContextMenu("Refresh Goal107 Objective Payload")]
        public void RefreshPayloadStatus()
        {
            BindOptionalControllers();
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifestJson = ReadFile(root, ManifestFileName, diagnostics);
            var objectivesJson = ReadFile(root, ObjectivesFileName, diagnostics);
            var runJson = ReadFile(root, AcceptanceRunFileName, diagnostics);
            var completionJson = ReadFile(root, CompletionStateFileName, diagnostics);
            var proofJson = ReadFile(root, ReplayProofFileName, diagnostics);

            objectiveCount = IntField(manifestJson, "objectiveCount");
            replayStepCount = IntField(manifestJson, "replayStepCount");
            stateDeltaCount = IntField(manifestJson, "stateDeltaCount");
            checkpointStepIndex = IntField(manifestJson, "checkpointStepIndex");
            finalStateHash = StringField(manifestJson, "finalStateHash");
            finalObjectiveAcceptanceHash = StringField(manifestJson, "objectiveAcceptanceHash");
            completedObjectiveCount = IntField(completionJson, "completedObjectiveCount");
            finalStatus = StringField(completionJson, "finalStatus");
            objectives.Clear();
            completedObjectiveIds.Clear();

            foreach (var block in Blocks(objectivesJson, "objectiveId"))
            {
                var objective = new OfflineGeoworldObjectiveState
                {
                    ObjectiveId = StringField(block, "objectiveId"),
                    ObjectiveKind = StringField(block, "objectiveKind"),
                    DisplayName = StringField(block, "displayName"),
                    StepIndex = IntField(block, "stepIndex"),
                    CompletionStateKey = StringField(block, "completionStateKey"),
                    ExpectedStateDeltaKey = FirstStringArrayValue(block, "expectedStateDeltaKeys"),
                    DeterministicHashContribution = StringField(block, "deterministicHashContribution")
                };
                objective.RequiredPredecessorIds.AddRange(StringArray(block, "requiredPredecessorObjectiveIds"));
                if (!string.IsNullOrWhiteSpace(objective.ObjectiveId))
                {
                    objectives.Add(objective);
                }
            }

            foreach (var objectiveId in StringArray(completionJson, "completedObjectiveIds"))
            {
                completedObjectiveIds.Add(objectiveId);
            }

            for (var index = 0; index < objectives.Count; index++)
            {
                var objective = objectives[index];
                if (completedObjectiveIds.Contains(objective.ObjectiveId))
                {
                    objective.MarkCompleted();
                }
            }

            currentObjectiveIndex = Math.Min(completedObjectiveIds.Count, Math.Max(0, objectives.Count - 1));
            var runSteps = CountBlocks(runJson, "stepIndex");
            var replayLink = BoolField(proofJson, "checkpointResumeApplied")
                             && BoolField(proofJson, "stateDeltaLinkagePassed");
            statusLine = "goal107_objectives count=" + objectives.Count
                         + " completed=" + completedObjectiveIds.Count
                         + " runSteps=" + runSteps
                         + " replayLink=" + replayLink
                         + " diagnostics=" + diagnostics.Count;
        }

        [ContextMenu("Manual Advance Current Goal107 Objective")]
        public bool ManualAdvanceCurrentObjective()
        {
            BindOptionalControllers();
            if (objectives.Count == 0)
            {
                RefreshPayloadStatus();
            }

            if (currentObjectiveIndex < 0 || currentObjectiveIndex >= objectives.Count)
            {
                statusLine = "objective advance rejected index=" + currentObjectiveIndex;
                return false;
            }

            var objective = objectives[currentObjectiveIndex];
            if (!objective.CanComplete(completedObjectiveIds))
            {
                statusLine = "objective prerequisites rejected id=" + objective.ObjectiveId;
                return false;
            }

            if (interactionController != null && objective.ObjectiveKind.Contains("inspect"))
            {
                interactionController.ExecuteManualAction("inspect");
            }

            objective.MarkCompleted();
            completedObjectiveIds.Add(objective.ObjectiveId);
            completedObjectiveCount = completedObjectiveIds.Count;
            currentObjectiveIndex = Math.Min(completedObjectiveCount, Math.Max(0, objectives.Count - 1));
            statusLine = "objective completed id=" + objective.ObjectiveId
                         + " completed=" + completedObjectiveCount
                         + "/" + objectives.Count;
            return true;
        }

        [ContextMenu("Check Goal107 Replay Linkage")]
        public bool CheckReplayLinkage()
        {
            BindOptionalControllers();
            var replayReady = replayController != null
                              && replayController.ReplayStepCount == replayStepCount
                              && string.Equals(replayController.FinalStateHash, finalStateHash, StringComparison.Ordinal);
            var saveLoadReady = saveLoadController != null
                                && saveLoadController.ReplayStepCount == replayStepCount
                                && saveLoadController.StateDeltaCount == stateDeltaCount
                                && saveLoadController.CheckpointStepIndex == checkpointStepIndex;
            statusLine = "goal107_replay_linkage replay=" + replayReady + " saveLoad=" + saveLoadReady;
            return replayReady && saveLoadReady;
        }

        [ContextMenu("Replay Goal107 From Metadata")]
        public bool ReplayFromMetadata()
        {
            BindOptionalControllers();
            if (replayController == null || saveLoadController == null)
            {
                statusLine = "goal107 replay rejected missing Goal106 controllers";
                return false;
            }

            replayController.ResetReplay();
            replayController.SaveCheckpointSnapshot();
            if (!replayController.LoadCheckpointSnapshot())
            {
                statusLine = "goal107 checkpoint replay rejected";
                return false;
            }

            replayController.ReplayAllRemaining();
            var matched = CheckReplayLinkage()
                          && string.Equals(replayController.CurrentStateHash, finalStateHash, StringComparison.Ordinal);
            statusLine = matched ? "goal107 replay matched final hash" : "goal107 replay final hash mismatch";
            return matched;
        }

        private void BindOptionalControllers()
        {
            interactionController = GetComponent<OfflineGeoworldInteractionController>();
            replayController = GetComponent<OfflineGeoworldSessionReplayController>();
            saveLoadController = GetComponent<OfflineGeoworldSessionSaveLoadController>();
        }

        private static string ReadFile(string root, string fileName, List<string> diagnostics)
        {
            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing:" + fileName);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static List<string> Blocks(string json, string anchorField)
        {
            var result = new List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\""
                                                                   + Regex.Escape(anchorField)
                                                                   + "\"[\\s\\S]*?\\}"))
            {
                result.Add(match.Value);
            }

            return result;
        }

        private static int CountBlocks(string json, string anchorField)
        {
            return Blocks(json, anchorField).Count;
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        private static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            bool value;
            return match.Success && bool.TryParse(match.Groups[1].Value, out value) && value;
        }

        private static List<string> StringArray(string json, string field)
        {
            var result = new List<string>();
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\\[([^\\]]*)\\]");
            if (!match.Success)
            {
                return result;
            }

            foreach (Match item in Regex.Matches(match.Groups[1].Value, "\"([^\"]*)\""))
            {
                result.Add(item.Groups[1].Value);
            }

            return result;
        }

        private static string FirstStringArrayValue(string json, string field)
        {
            var values = StringArray(json, field);
            return values.Count == 0 ? string.Empty : values[0];
        }
    }
}
