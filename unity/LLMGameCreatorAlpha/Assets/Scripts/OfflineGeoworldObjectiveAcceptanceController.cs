using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldObjectiveAcceptanceController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal107";

        [SerializeField] private OfflineGeoworldObjectiveTracker tracker;
        [SerializeField] private OfflineGeoworldInteractionController interactionController;
        [SerializeField] private OfflineGeoworldSessionSaveLoadController saveLoadController;
        [SerializeField] private OfflineGeoworldSessionReplayController replayController;
        [SerializeField] private string payloadRoot = RelativeRoot;
        [SerializeField] private string statusLine = string.Empty;

        public string LastStatus { get { return statusLine; } }

        private void Awake()
        {
            BindComponents();
        }

        private void Start()
        {
            RefreshPayloadStatus();
        }

        [ContextMenu("Refresh Goal107 Objective Acceptance")]
        public void RefreshPayloadStatus()
        {
            BindComponents();
            tracker.RefreshPayloadStatus();
            statusLine = "goal107 acceptance refreshed root=" + payloadRoot
                         + " tracker=" + tracker.LastStatus;
        }

        [ContextMenu("Manual Advance Goal107 Objective")]
        public bool ManualAdvanceCurrentObjective()
        {
            BindComponents();
            var advanced = tracker.ManualAdvanceCurrentObjective();
            statusLine = advanced ? "goal107 objective advanced" : "goal107 objective advance rejected";
            return advanced;
        }

        [ContextMenu("Check Goal107 Replay Linkage")]
        public bool CheckReplayLinkage()
        {
            BindComponents();
            var linked = tracker.CheckReplayLinkage();
            statusLine = linked ? "goal107 replay linkage ready" : "goal107 replay linkage incomplete";
            return linked;
        }

        [ContextMenu("Replay Goal107 Acceptance Metadata")]
        public bool ReplayFromMetadata()
        {
            BindComponents();
            var replayed = tracker.ReplayFromMetadata();
            statusLine = replayed ? "goal107 replay accepted" : "goal107 replay rejected";
            return replayed;
        }

        private void BindComponents()
        {
            tracker = GetComponent<OfflineGeoworldObjectiveTracker>();
            if (tracker == null)
            {
                tracker = gameObject.AddComponent<OfflineGeoworldObjectiveTracker>();
            }

            interactionController = GetComponent<OfflineGeoworldInteractionController>();
            if (interactionController == null)
            {
                interactionController = gameObject.AddComponent<OfflineGeoworldInteractionController>();
            }

            saveLoadController = GetComponent<OfflineGeoworldSessionSaveLoadController>();
            if (saveLoadController == null)
            {
                saveLoadController = gameObject.AddComponent<OfflineGeoworldSessionSaveLoadController>();
            }

            replayController = GetComponent<OfflineGeoworldSessionReplayController>();
            if (replayController == null)
            {
                replayController = gameObject.AddComponent<OfflineGeoworldSessionReplayController>();
            }
        }
    }
}
