using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaSliceCoordinator : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal108";
        private const string ManifestFileName = "offline-geoworld-alpha-slice-manifest.json";
        private const string ComponentsFileName = "offline-geoworld-alpha-slice-components.json";
        private const string ReadinessMatrixFileName = "offline-geoworld-alpha-slice-readiness-matrix.json";

        [SerializeField] private bool previewReady;
        [SerializeField] private bool travelReady;
        [SerializeField] private bool interactionsReady;
        [SerializeField] private bool sessionReplayReady;
        [SerializeField] private bool objectivesReady;
        [SerializeField] private bool finalAcceptanceReady;
        [SerializeField] private int componentCount;
        [SerializeField] private int readyComponentCount;
        [SerializeField] private int objectiveCount;
        [SerializeField] private int completedObjectiveCount;
        [SerializeField] private string finalStatus = string.Empty;
        [SerializeField] private string finalAcceptanceHash = string.Empty;
        [SerializeField] private string statusLine = string.Empty;

        private OfflineGeoworldPreviewRunner previewRunner;
        private OfflineGeoworldPlayModeTravelController playModeTravelController;
        private OfflineGeoworldInteractiveTravelController interactiveTravelController;
        private OfflineGeoworldInteractionController interactionController;
        private OfflineGeoworldSessionSaveLoadController saveLoadController;
        private OfflineGeoworldSessionReplayController replayController;
        private OfflineGeoworldObjectiveAcceptanceController objectiveAcceptanceController;

        public bool PreviewReady { get { return previewReady; } }
        public bool TravelReady { get { return travelReady; } }
        public bool InteractionsReady { get { return interactionsReady; } }
        public bool SessionReplayReady { get { return sessionReplayReady; } }
        public bool ObjectivesReady { get { return objectivesReady; } }
        public bool FinalAcceptanceReady { get { return finalAcceptanceReady; } }
        public string LastStatus { get { return statusLine; } }

        private void Awake()
        {
            EnsureGoalControllers();
        }

        private void Start()
        {
            RefreshStatus();
        }

        [ContextMenu("Refresh Goal108 Alpha Slice Status")]
        public void RefreshStatus()
        {
            EnsureGoalControllers();
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifest = ReadPayloadFile(root, ManifestFileName, diagnostics);
            var components = ReadPayloadFile(root, ComponentsFileName, diagnostics);
            var matrix = ReadPayloadFile(root, ReadinessMatrixFileName, diagnostics);

            componentCount = IntField(manifest, "componentCount");
            readyComponentCount = IntField(manifest, "readyComponentCount");
            objectiveCount = IntField(manifest, "objectiveCount");
            completedObjectiveCount = IntField(manifest, "completedObjectiveCount");
            finalStatus = StringField(manifest, "finalStatus");
            finalAcceptanceHash = StringField(manifest, "finalAcceptanceHash");

            previewReady = ComponentReady(components, "preview");
            travelReady = ComponentReady(components, "play_mode_travel")
                          && ComponentReady(components, "interactive_travel");
            interactionsReady = ComponentReady(components, "interactions");
            sessionReplayReady = ComponentReady(components, "session_replay");
            objectivesReady = ComponentReady(components, "objective_acceptance");
            finalAcceptanceReady = BoolField(matrix, "passed")
                                   && previewReady
                                   && travelReady
                                   && interactionsReady
                                   && sessionReplayReady
                                   && objectivesReady
                                   && objectiveCount >= 5
                                   && completedObjectiveCount == objectiveCount
                                   && finalStatus == "completed";
            statusLine = "goal108 components=" + readyComponentCount + "/" + componentCount
                         + " objectives=" + completedObjectiveCount + "/" + objectiveCount
                         + " final=" + finalAcceptanceReady
                         + " diagnostics=" + diagnostics.Count;
        }

        [ContextMenu("Verify Goal108 Alpha Slice")]
        public bool VerifySlice()
        {
            RefreshStatus();
            if (previewRunner != null)
            {
                previewRunner.RefreshPayloadStatus();
            }

            if (playModeTravelController != null)
            {
                playModeTravelController.RefreshPayloadStatus();
            }

            if (interactiveTravelController != null)
            {
                interactiveTravelController.RefreshPayloadStatus();
            }

            if (interactionController != null)
            {
                interactionController.RefreshPayloadStatus();
            }

            if (saveLoadController != null)
            {
                saveLoadController.RefreshPayload();
            }

            if (replayController != null)
            {
                replayController.RefreshReplayPayload();
            }

            if (objectiveAcceptanceController != null)
            {
                objectiveAcceptanceController.RefreshPayloadStatus();
            }

            statusLine = finalAcceptanceReady
                ? "goal108 alpha slice verified hash=" + finalAcceptanceHash
                : "goal108 alpha slice incomplete";
            return finalAcceptanceReady;
        }

        public void EnsureGoalControllers()
        {
            previewRunner = GetOrCreate<OfflineGeoworldPreviewRunner>();
            playModeTravelController = GetOrCreate<OfflineGeoworldPlayModeTravelController>();
            interactiveTravelController = GetOrCreate<OfflineGeoworldInteractiveTravelController>();
            interactionController = GetOrCreate<OfflineGeoworldInteractionController>();
            saveLoadController = GetOrCreate<OfflineGeoworldSessionSaveLoadController>();
            replayController = GetOrCreate<OfflineGeoworldSessionReplayController>();
            objectiveAcceptanceController = GetOrCreate<OfflineGeoworldObjectiveAcceptanceController>();
        }

        private T GetOrCreate<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static string ReadPayloadFile(string root, string fileName, List<string> diagnostics)
        {
            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing:" + fileName);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static bool ComponentReady(string json, string componentId)
        {
            foreach (Match block in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\"componentId\"[\\s\\S]*?\\}"))
            {
                var text = block.Value;
                if (StringField(text, "componentId") == componentId)
                {
                    return BoolField(text, "ready");
                }
            }

            return false;
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
    }
}
