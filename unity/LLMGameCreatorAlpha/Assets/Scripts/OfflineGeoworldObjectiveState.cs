using System;
using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldObjectiveState
    {
        public string ObjectiveId = string.Empty;
        public string ObjectiveKind = string.Empty;
        public string DisplayName = string.Empty;
        public int StepIndex;
        public bool Completed;
        public string CompletionStateKey = string.Empty;
        public string ExpectedStateDeltaKey = string.Empty;
        public string DeterministicHashContribution = string.Empty;
        public readonly List<string> RequiredPredecessorIds = new List<string>();

        public bool CanComplete(ICollection<string> completedObjectiveIds)
        {
            if (completedObjectiveIds == null)
            {
                return false;
            }

            foreach (var predecessorId in RequiredPredecessorIds)
            {
                if (!completedObjectiveIds.Contains(predecessorId))
                {
                    return false;
                }
            }

            return !Completed;
        }

        public void MarkCompleted()
        {
            Completed = true;
        }
    }
}
