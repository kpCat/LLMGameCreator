using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldStateDeltaLog : MonoBehaviour
    {
        [SerializeField] private int deltaCount;
        [SerializeField] private string lastEventId = string.Empty;
        [SerializeField] private string lastTargetId = string.Empty;
        [SerializeField] private string lastActionKind = string.Empty;
        [SerializeField] private string currentStateHash = string.Empty;
        [SerializeField] private string statusLine = string.Empty;

        private readonly List<OfflineGeoworldStateDeltaLogEntry> entries =
            new List<OfflineGeoworldStateDeltaLogEntry>();

        public int DeltaCount { get { return deltaCount; } }
        public string CurrentStateHash { get { return currentStateHash; } }
        public IReadOnlyList<OfflineGeoworldStateDeltaLogEntry> Entries { get { return entries; } }

        public void ClearLog(string initialStateHash)
        {
            entries.Clear();
            deltaCount = 0;
            lastEventId = string.Empty;
            lastTargetId = string.Empty;
            lastActionKind = string.Empty;
            currentStateHash = initialStateHash ?? string.Empty;
            statusLine = "cleared stateHash=" + currentStateHash;
        }

        public void AppendDelta(OfflineGeoworldStateDeltaLogEntry entry)
        {
            entries.Add(entry);
            deltaCount = entries.Count;
            lastEventId = entry.EventId;
            lastTargetId = entry.TargetId;
            lastActionKind = entry.ActionKind;
            currentStateHash = entry.DeterministicStateHash;
            statusLine = "delta=" + deltaCount
                         + " event=" + lastEventId
                         + " action=" + lastActionKind
                         + " stateHash=" + currentStateHash;
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldStateDeltaLogEntry
    {
        public string EventId = string.Empty;
        public string TargetId = string.Empty;
        public string ActionId = string.Empty;
        public string ActionKind = string.Empty;
        public string DeltaKind = string.Empty;
        public string PreviousStateHash = string.Empty;
        public string DeterministicStateHash = string.Empty;
    }
}
