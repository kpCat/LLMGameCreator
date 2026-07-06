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
        public string recipePreview = string.Empty;
        public string recipeApplyResult = string.Empty;
        public string harvestPreview = string.Empty;
        public string harvestApplyResult = string.Empty;
        public string transactionPreview = string.Empty;
        public string encounterPreview = string.Empty;
        public string combatRoundPreview = string.Empty;
        public string systemsEventLog = string.Empty;
        public string fullPlaythroughStatus = string.Empty;
        public string movementPathSummary = string.Empty;
        public string signInteractionResult = string.Empty;
        public string dialogueSummary = string.Empty;
        public string questObjectiveStatus = string.Empty;
        public string inventoryResourceFinalSummary = string.Empty;
        public string systemsSummary = string.Empty;
        public string combatSummary = string.Empty;
        public string eventTranscriptSummary = string.Empty;
        public string finalStateSummary = string.Empty;
        public int appliedInteractionCount;
        public int startedQuestCount;

        public readonly Dictionary<string, string> projectionFlags =
            new Dictionary<string, string>(System.StringComparer.Ordinal);
        public readonly Dictionary<string, int> playerInventory =
            new Dictionary<string, int>(System.StringComparer.Ordinal);
        public readonly Dictionary<string, int> resourceLedger =
            new Dictionary<string, int>(System.StringComparer.Ordinal);
        public readonly Dictionary<string, int> itemDurability =
            new Dictionary<string, int>(System.StringComparer.Ordinal);
        public readonly List<string> events = new List<string>();
        public readonly List<string> systemsEvents = new List<string>();

        public bool SamplePackageLoaded;
        public bool GenericProjectionBuilt;
        public bool InteractionPreviewPresent;
        public bool InteractionApplyPassed;
        public bool DialogueSummaryPresent;
        public bool QuestObjectiveSummaryPresent;
        public bool InventorySummaryPresent;
        public bool ResourceSummaryPresent;
        public bool EventLogPresent;
        public bool InventoryInitialized;
        public bool ResourcesInitialized;
        public bool RecipePreviewPresent;
        public bool RecipeApplyPassed;
        public bool HarvestPreviewPresent;
        public bool HarvestApplyPassed;
        public bool TransactionPreviewPresent;
        public bool EncounterPreviewPresent;
        public bool CombatRoundPreviewPresent;
        public bool SystemsEventLogPresent;
        public bool GenericSystemsPassed;
        public bool FullPlaythroughPassed;
        public bool MapPathPreviewPresent;
        public bool SignInteractionApplied;
        public bool QuestObjectiveStatusPresent;
        public bool SystemsSummaryPresent;
        public bool EventTranscriptPresent;

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

        public void AppendSystemsEvent(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            systemsEvents.Add(value);
            systemsEventLog = string.Join("\n", systemsEvents.ToArray());
            SystemsEventLogPresent = systemsEvents.Count > 0;
        }
    }
}
