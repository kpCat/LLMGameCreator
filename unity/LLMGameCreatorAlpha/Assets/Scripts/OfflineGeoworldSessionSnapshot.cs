using System;
using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldSessionSnapshot
    {
        public string GoalId = "goal_106_offline_geoworld_session_persistence_replay";
        public string InitialStateHash = string.Empty;
        public string CurrentStateHash = string.Empty;
        public string FinalStateHash = string.Empty;
        public int AppliedReplayStepCount;
        public int CheckpointStepIndex;
        public string CheckpointStateHash = string.Empty;
        public string SnapshotHash = string.Empty;
        public List<OfflineGeoworldSessionSnapshotDelta> Deltas =
            new List<OfflineGeoworldSessionSnapshotDelta>();
    }

    [Serializable]
    public sealed class OfflineGeoworldSessionSnapshotDelta
    {
        public int ReplayStepIndex;
        public string EventId = string.Empty;
        public string TargetId = string.Empty;
        public string ActionId = string.Empty;
        public string ActionKind = string.Empty;
        public string StateHashBefore = string.Empty;
        public string StateHashAfter = string.Empty;
    }
}
