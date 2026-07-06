using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    public sealed class GenericGamePackageProjectionState
    {
        public string selectedEntityId = string.Empty;
        public string selectedInteractionId = string.Empty;
        public string selectedDialogueId = string.Empty;
        public string selectedQuestId = string.Empty;
        public string inventorySummary = string.Empty;
        public string resourceSummary = string.Empty;
        public string questObjectiveSummary = string.Empty;
        public string interactionEffectPreview = string.Empty;
        public string projectionEventLog = string.Empty;
        public int appliedInteractionCount;
        public int startedQuestCount;

        public readonly Dictionary<string, string> projectionFlags =
            new Dictionary<string, string>(System.StringComparer.Ordinal);
        public readonly List<string> events = new List<string>();

        public bool SamplePackageLoaded;
        public bool GenericProjectionBuilt;
        public bool InteractionPreviewPresent;
        public bool InteractionApplyPassed;
        public bool DialogueSummaryPresent;
        public bool QuestObjectiveSummaryPresent;
        public bool InventorySummaryPresent;
        public bool ResourceSummaryPresent;
        public bool EventLogPresent;

        public void AppendEvent(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            events.Add(value);
            projectionEventLog = string.Join("\n", events.ToArray());
            EventLogPresent = events.Count > 0;
        }
    }
}
