using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldBoundaryPrefetchState : MonoBehaviour
    {
        [SerializeField] private string currentBoundaryCrossingId = string.Empty;
        [SerializeField] private string currentSyntheticChunkKey = string.Empty;
        [SerializeField] private int activeChunkCount;
        [SerializeField] private int prefetchChunkCount;
        [SerializeField] private bool inBoundaryBand;
        [SerializeField] private string statusLine = string.Empty;

        public string CurrentBoundaryCrossingId { get { return currentBoundaryCrossingId; } }
        public int ActiveChunkCount { get { return activeChunkCount; } }
        public int PrefetchChunkCount { get { return prefetchChunkCount; } }
        public bool InBoundaryBand { get { return inBoundaryBand; } }
        public string StatusLine { get { return statusLine; } }

        public void Apply(
            string boundaryCrossingId,
            string syntheticChunkKey,
            bool boundaryBand,
            IReadOnlyCollection<string> activeChunks,
            IReadOnlyCollection<string> prefetchChunks)
        {
            currentBoundaryCrossingId = boundaryCrossingId;
            currentSyntheticChunkKey = syntheticChunkKey;
            inBoundaryBand = boundaryBand;
            activeChunkCount = activeChunks.Count;
            prefetchChunkCount = prefetchChunks.Count;
            statusLine = "chunk=" + currentSyntheticChunkKey
                         + " crossing=" + currentBoundaryCrossingId
                         + " active=" + activeChunkCount
                         + " prefetch=" + prefetchChunkCount
                         + " boundaryBand=" + inBoundaryBand.ToString().ToLowerInvariant();
        }
    }
}
