using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldPlayModeTravelState
    {
        public string PayloadRoot = "LLMGameCreator/OfflineGeoworldGoal103";
        public int StepCount;
        public int ObjectCount;
        public List<OfflineGeoworldPlayModeTravelStepState> Steps =
            new List<OfflineGeoworldPlayModeTravelStepState>();
        public List<OfflineGeoworldPlayModeObjectState> Objects =
            new List<OfflineGeoworldPlayModeObjectState>();

        public OfflineGeoworldPlayModeTravelStepState StepAt(int index)
        {
            if (Steps.Count == 0)
            {
                return new OfflineGeoworldPlayModeTravelStepState();
            }

            return Steps[Mathf.Clamp(index, 0, Steps.Count - 1)];
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldPlayModeTravelStepState
    {
        public int StepIndex;
        public string StepId = string.Empty;
        public string Action = string.Empty;
        public string CenterChunkKey = string.Empty;
        public List<string> ActiveChunkKeys = new List<string>();
        public List<string> BoundaryPrefetchChunkKeys = new List<string>();
        public List<string> VisibleObjectIds = new List<string>();
        public List<string> HiddenObjectIds = new List<string>();
        public List<string> NewlyVisibleObjectIds = new List<string>();
        public List<string> NewlyHiddenObjectIds = new List<string>();
        public int ExpectedVisibleObjectCount;
        public string DeterministicStateHash = string.Empty;
    }

    [Serializable]
    public sealed class OfflineGeoworldPlayModeObjectState
    {
        public string ObjectId = string.Empty;
        public string ObjectName = string.Empty;
        public string SourceCommandId = string.Empty;
        public string CommandKind = string.Empty;
        public string SourceChunkKey = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
    }
}
