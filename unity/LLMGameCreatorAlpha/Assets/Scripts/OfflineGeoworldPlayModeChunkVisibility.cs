using System;
using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldPlayModeChunkVisibility
    {
        public int StepIndex;
        public string StepId = string.Empty;
        public List<string> ActiveChunkKeys = new List<string>();
        public List<string> BoundaryPrefetchChunkKeys = new List<string>();
        public int ActiveChunkCount;
        public int BoundaryPrefetchChunkCount;

        public string ToStatusLine()
        {
            return "step=" + StepIndex
                   + " activeChunks=" + ActiveChunkCount
                   + " boundaryPrefetch=" + BoundaryPrefetchChunkCount;
        }
    }
}
