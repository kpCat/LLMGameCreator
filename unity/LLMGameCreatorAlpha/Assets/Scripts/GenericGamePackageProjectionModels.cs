using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class GenericGamePackageProjectionDocument
    {
        public GenericGamePackageManifest manifest = new GenericGamePackageManifest();
        public GenericGamePackageGame game = new GenericGamePackageGame();
    }

    [Serializable]
    public sealed class GenericGamePackageManifest
    {
        public string packageId = string.Empty;
        public string title = string.Empty;
        public string startMapId = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageGame
    {
        public GenericGamePackageTilePrototype[] tilePrototypes = Array.Empty<GenericGamePackageTilePrototype>();
        public GenericGamePackageEntityPrototype[] entityPrototypes = Array.Empty<GenericGamePackageEntityPrototype>();
        public GenericGamePackageMap[] maps = Array.Empty<GenericGamePackageMap>();
        public GenericGamePackageItem[] items = Array.Empty<GenericGamePackageItem>();
        public GenericGamePackageResource[] resources = Array.Empty<GenericGamePackageResource>();
        public GenericGamePackageInventory[] inventories = Array.Empty<GenericGamePackageInventory>();
        public GenericGamePackageQuest[] quests = Array.Empty<GenericGamePackageQuest>();
        public GenericGamePackageDialogue[] dialogues = Array.Empty<GenericGamePackageDialogue>();
        public GenericGamePackageInteraction[] interactions = Array.Empty<GenericGamePackageInteraction>();
        public GenericGamePackageRecipe[] recipes = Array.Empty<GenericGamePackageRecipe>();
        public GenericGamePackageLootTable[] lootTables = Array.Empty<GenericGamePackageLootTable>();
        public GenericGamePackageTransaction[] transactions =
            Array.Empty<GenericGamePackageTransaction>();
        public GenericGamePackageResourceNode[] resourceNodes =
            Array.Empty<GenericGamePackageResourceNode>();
        public GenericGamePackageAbility[] abilities = Array.Empty<GenericGamePackageAbility>();
        public GenericGamePackageEncounter[] encounters = Array.Empty<GenericGamePackageEncounter>();
    }

    [Serializable]
    public sealed class GenericGamePackageTilePrototype
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public bool walkable = true;
    }

    [Serializable]
    public sealed class GenericGamePackageEntityPrototype
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string assetId = string.Empty;
        public GenericGamePackageComponent[] components = Array.Empty<GenericGamePackageComponent>();
    }

    [Serializable]
    public sealed class GenericGamePackageComponent
    {
        public string type = string.Empty;
        public GenericGamePackageComponentArgs args = new GenericGamePackageComponentArgs();
    }

    [Serializable]
    public sealed class GenericGamePackageComponentArgs
    {
        public string dialogueId = string.Empty;
        public string text = string.Empty;
        public string interactionId = string.Empty;
        public string blocksMovement = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageMap
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public int width;
        public int height;
        public string defaultTileId = string.Empty;
        public GenericGamePackagePosition startPosition = new GenericGamePackagePosition();
        public GenericGamePackageTile[] tiles = Array.Empty<GenericGamePackageTile>();
        public GenericGamePackageEntity[] entities = Array.Empty<GenericGamePackageEntity>();
    }

    [Serializable]
    public sealed class GenericGamePackagePosition
    {
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class GenericGamePackageTile
    {
        public int x;
        public int y;
        public string tileId = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageEntity
    {
        public string id = string.Empty;
        public string prototypeId = string.Empty;
        public GenericGamePackagePosition position = new GenericGamePackagePosition();
        public GenericGamePackageComponent[] components = Array.Empty<GenericGamePackageComponent>();
    }

    [Serializable]
    public sealed class GenericGamePackageItem
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
        public int maxDurability;
        public string[] tags = Array.Empty<string>();
        public GenericGamePackageMetadataEntry[] metadata =
            Array.Empty<GenericGamePackageMetadataEntry>();
    }

    [Serializable]
    public sealed class GenericGamePackageResource
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
        public int defaultValue;
        public int minValue;
        public int maxValue;
    }

    [Serializable]
    public sealed class GenericGamePackageInventory
    {
        public string id = string.Empty;
        public string ownerKind = string.Empty;
        public string ownerId = string.Empty;
        public int slots;
        public GenericGamePackageInventoryStack[] stacks = Array.Empty<GenericGamePackageInventoryStack>();
    }

    [Serializable]
    public sealed class GenericGamePackageInventoryStack
    {
        public string itemId = string.Empty;
        public int amount;
        public int durability;
    }

    [Serializable]
    public sealed class GenericGamePackageQuest
    {
        public string id = string.Empty;
        public string title = string.Empty;
        public string description = string.Empty;
        public GenericGamePackageQuestObjective[] objectives =
            Array.Empty<GenericGamePackageQuestObjective>();
    }

    [Serializable]
    public sealed class GenericGamePackageQuestObjective
    {
        public string id = string.Empty;
        public string kind = string.Empty;
        public string targetId = string.Empty;
        public int requiredAmount;
    }

    [Serializable]
    public sealed class GenericGamePackageDialogue
    {
        public string id = string.Empty;
        public string title = string.Empty;
        public string startNodeId = string.Empty;
        public GenericGamePackageDialogueNode[] nodes = Array.Empty<GenericGamePackageDialogueNode>();
    }

    [Serializable]
    public sealed class GenericGamePackageDialogueNode
    {
        public string id = string.Empty;
        public string speakerId = string.Empty;
        public string text = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageInteraction
    {
        public string id = string.Empty;
        public string kind = string.Empty;
        public GenericGamePackageEffect[] effects = Array.Empty<GenericGamePackageEffect>();
    }

    [Serializable]
    public sealed class GenericGamePackageEffect
    {
        public string type = string.Empty;
        public GenericGamePackageEffectArgs args = new GenericGamePackageEffectArgs();
    }

    [Serializable]
    public sealed class GenericGamePackageEffectArgs
    {
        public string id = string.Empty;
        public string value = string.Empty;
        public string message = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageAmount
    {
        public string kind = string.Empty;
        public string id = string.Empty;
        public int amount;
    }

    [Serializable]
    public sealed class GenericGamePackageRecipe
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public GenericGamePackageAmount[] inputs = Array.Empty<GenericGamePackageAmount>();
        public GenericGamePackageAmount[] costs = Array.Empty<GenericGamePackageAmount>();
        public GenericGamePackageAmount[] outputs = Array.Empty<GenericGamePackageAmount>();
    }

    [Serializable]
    public sealed class GenericGamePackageLootTable
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public GenericGamePackageLootEntry[] entries = Array.Empty<GenericGamePackageLootEntry>();
    }

    [Serializable]
    public sealed class GenericGamePackageLootEntry
    {
        public string id = string.Empty;
        public int minCount;
        public int maxCount;
        public GenericGamePackageAmount output = new GenericGamePackageAmount();
    }

    [Serializable]
    public sealed class GenericGamePackageTransaction
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public GenericGamePackageAmount[] costs = Array.Empty<GenericGamePackageAmount>();
        public GenericGamePackageAmount[] outputs = Array.Empty<GenericGamePackageAmount>();
    }

    [Serializable]
    public sealed class GenericGamePackageResourceNode
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public GenericGamePackageAmount[] production = Array.Empty<GenericGamePackageAmount>();
        public GenericGamePackageMetadataEntry[] metadata =
            Array.Empty<GenericGamePackageMetadataEntry>();
    }

    [Serializable]
    public sealed class GenericGamePackageAbility
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
        public int power;
        public string resourceId = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageEncounter
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
        public GenericGamePackageEncounterParticipant[] participants =
            Array.Empty<GenericGamePackageEncounterParticipant>();
        public GenericGamePackageMetadataEntry[] metadata =
            Array.Empty<GenericGamePackageMetadataEntry>();
    }

    [Serializable]
    public sealed class GenericGamePackageEncounterParticipant
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
        public string team = string.Empty;
        public GenericGamePackageAmount[] resources = Array.Empty<GenericGamePackageAmount>();
        public string[] abilities = Array.Empty<string>();
    }

    [Serializable]
    public sealed class GenericGamePackageMetadataEntry
    {
        public string key = string.Empty;
        public string value = string.Empty;
    }

    public sealed class GenericGamePackageProjectionModel
    {
        public string SamplePackagePath = GenericGamePackageProjectionAdapter.SamplePackageRelativePath;
        public string PackageId = string.Empty;
        public string PackageTitle = string.Empty;
        public string StartMapId = string.Empty;
        public string MapId = string.Empty;
        public string MapName = string.Empty;
        public int MapWidth;
        public int MapHeight;
        public int StartX;
        public int StartY;
        public readonly List<GenericGamePackageProjectionTile> Tiles = new List<GenericGamePackageProjectionTile>();
        public readonly List<GenericGamePackageProjectionEntity> Entities = new List<GenericGamePackageProjectionEntity>();
        public readonly List<GenericGamePackageProjectionItem> Items = new List<GenericGamePackageProjectionItem>();
        public readonly List<GenericGamePackageProjectionResource> Resources =
            new List<GenericGamePackageProjectionResource>();
        public readonly List<GenericGamePackageProjectionInventory> Inventories =
            new List<GenericGamePackageProjectionInventory>();
        public readonly List<GenericGamePackageProjectionQuest> Quests =
            new List<GenericGamePackageProjectionQuest>();
        public readonly List<GenericGamePackageProjectionDialogue> Dialogues =
            new List<GenericGamePackageProjectionDialogue>();
        public readonly List<GenericGamePackageProjectionInteraction> Interactions =
            new List<GenericGamePackageProjectionInteraction>();
        public readonly List<GenericGamePackageProjectionRecipe> Recipes =
            new List<GenericGamePackageProjectionRecipe>();
        public readonly List<GenericGamePackageProjectionLootTable> LootTables =
            new List<GenericGamePackageProjectionLootTable>();
        public readonly List<GenericGamePackageProjectionTransaction> Transactions =
            new List<GenericGamePackageProjectionTransaction>();
        public readonly List<GenericGamePackageProjectionResourceNode> ResourceNodes =
            new List<GenericGamePackageProjectionResourceNode>();
        public readonly List<GenericGamePackageProjectionAbility> Abilities =
            new List<GenericGamePackageProjectionAbility>();
        public readonly List<GenericGamePackageProjectionEncounter> Encounters =
            new List<GenericGamePackageProjectionEncounter>();
        public readonly List<string> Diagnostics = new List<string>();
    }

    public sealed class GenericGamePackageProjectionTile
    {
        public int X;
        public int Y;
        public string TileId = string.Empty;
        public string TileName = string.Empty;
        public string TileKind = string.Empty;
        public bool Explicit;
        public bool Walkable;
    }

    public sealed class GenericGamePackageProjectionEntity
    {
        public string EntityId = string.Empty;
        public string PrototypeId = string.Empty;
        public string PrototypeName = string.Empty;
        public int X;
        public int Y;
        public bool Interactable;
        public string InteractionId = string.Empty;
        public string DialogueId = string.Empty;
        public string InteractionText = string.Empty;
    }

    public sealed class GenericGamePackageProjectionItem
    {
        public string ItemId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
        public int MaxDurability;
        public readonly List<string> Tags = new List<string>();
        public readonly Dictionary<string, string> Metadata =
            new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public sealed class GenericGamePackageProjectionResource
    {
        public string ResourceId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
    }

    public sealed class GenericGamePackageProjectionInventory
    {
        public string InventoryId = string.Empty;
        public string OwnerKind = string.Empty;
        public string OwnerId = string.Empty;
        public int Slots;
        public readonly List<GenericGamePackageProjectionInventoryStack> Stacks =
            new List<GenericGamePackageProjectionInventoryStack>();
    }

    public sealed class GenericGamePackageProjectionInventoryStack
    {
        public string ItemId = string.Empty;
        public string ItemName = string.Empty;
        public int Amount;
        public int Durability;
    }

    public sealed class GenericGamePackageProjectionQuest
    {
        public string QuestId = string.Empty;
        public string Title = string.Empty;
        public string Description = string.Empty;
        public readonly List<GenericGamePackageProjectionQuestObjective> Objectives =
            new List<GenericGamePackageProjectionQuestObjective>();
    }

    public sealed class GenericGamePackageProjectionQuestObjective
    {
        public string ObjectiveId = string.Empty;
        public string Kind = string.Empty;
        public string TargetId = string.Empty;
        public string TargetName = string.Empty;
        public int RequiredAmount;
    }

    public sealed class GenericGamePackageProjectionDialogue
    {
        public string DialogueId = string.Empty;
        public string Title = string.Empty;
        public string StartNodeId = string.Empty;
        public string StartSpeakerId = string.Empty;
        public string StartText = string.Empty;
    }

    public sealed class GenericGamePackageProjectionInteraction
    {
        public string InteractionId = string.Empty;
        public string Kind = string.Empty;
        public readonly List<GenericGamePackageProjectionEffect> Effects =
            new List<GenericGamePackageProjectionEffect>();
    }

    public sealed class GenericGamePackageProjectionEffect
    {
        public string Type = string.Empty;
        public string Id = string.Empty;
        public string Value = string.Empty;
        public string Message = string.Empty;
    }

    public sealed class GenericGamePackageProjectionAmount
    {
        public string Kind = string.Empty;
        public string Id = string.Empty;
        public int Amount;
    }

    public sealed class GenericGamePackageProjectionRecipe
    {
        public string RecipeId = string.Empty;
        public string Name = string.Empty;
        public readonly List<GenericGamePackageProjectionAmount> Inputs =
            new List<GenericGamePackageProjectionAmount>();
        public readonly List<GenericGamePackageProjectionAmount> Costs =
            new List<GenericGamePackageProjectionAmount>();
        public readonly List<GenericGamePackageProjectionAmount> Outputs =
            new List<GenericGamePackageProjectionAmount>();
    }

    public sealed class GenericGamePackageProjectionLootTable
    {
        public string LootTableId = string.Empty;
        public string Name = string.Empty;
        public readonly List<GenericGamePackageProjectionLootEntry> Entries =
            new List<GenericGamePackageProjectionLootEntry>();
    }

    public sealed class GenericGamePackageProjectionLootEntry
    {
        public string EntryId = string.Empty;
        public int MinCount;
        public int MaxCount;
        public GenericGamePackageProjectionAmount Output =
            new GenericGamePackageProjectionAmount();
    }

    public sealed class GenericGamePackageProjectionTransaction
    {
        public string TransactionId = string.Empty;
        public string Name = string.Empty;
        public readonly List<GenericGamePackageProjectionAmount> Costs =
            new List<GenericGamePackageProjectionAmount>();
        public readonly List<GenericGamePackageProjectionAmount> Outputs =
            new List<GenericGamePackageProjectionAmount>();
    }

    public sealed class GenericGamePackageProjectionResourceNode
    {
        public string ResourceNodeId = string.Empty;
        public string Name = string.Empty;
        public readonly List<GenericGamePackageProjectionAmount> Production =
            new List<GenericGamePackageProjectionAmount>();
        public readonly Dictionary<string, string> Metadata =
            new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public sealed class GenericGamePackageProjectionAbility
    {
        public string AbilityId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
        public int Power;
        public string ResourceId = string.Empty;
    }

    public sealed class GenericGamePackageProjectionEncounter
    {
        public string EncounterId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
        public readonly List<GenericGamePackageProjectionEncounterParticipant> Participants =
            new List<GenericGamePackageProjectionEncounterParticipant>();
        public readonly Dictionary<string, string> Metadata =
            new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public sealed class GenericGamePackageProjectionEncounterParticipant
    {
        public string ParticipantId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
        public string Team = string.Empty;
        public readonly List<GenericGamePackageProjectionAmount> Resources =
            new List<GenericGamePackageProjectionAmount>();
        public readonly List<string> Abilities = new List<string>();
    }

    public sealed class GenericGamePackageProjectionSmokeResult
    {
        public bool SectionPresent;
        public bool PackageIdentityPresent;
        public bool MapDimensionsPresent;
        public bool StartPlayerMarkerPresent;
        public bool TileMarkerPresent;
        public bool EntityMarkerPresent;
        public bool InteractionMarkerPresent;
        public bool ItemSummaryEntryPresent;
        public bool DescriptorPresent;
        public bool EventLogPresent;
        public bool ZeroFatalErrors;
        public string PackageId = string.Empty;
        public string PackageTitle = string.Empty;
        public string MapId = string.Empty;
        public int MapWidth;
        public int MapHeight;
        public int EntityCount;
        public int ItemCount;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return SectionPresent
                       && PackageIdentityPresent
                       && MapDimensionsPresent
                       && StartPlayerMarkerPresent
                       && TileMarkerPresent
                       && EntityMarkerPresent
                       && InteractionMarkerPresent
                       && ItemSummaryEntryPresent
                       && DescriptorPresent
                       && EventLogPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "passed=" + Passed
                   + "\nsectionPresent=" + SectionPresent
                   + "\npackageIdentityPresent=" + PackageIdentityPresent
                   + "\nmapDimensionsPresent=" + MapDimensionsPresent
                   + "\nstartPlayerMarkerPresent=" + StartPlayerMarkerPresent
                   + "\ntileMarkerPresent=" + TileMarkerPresent
                   + "\nentityMarkerPresent=" + EntityMarkerPresent
                   + "\ninteractionMarkerPresent=" + InteractionMarkerPresent
                   + "\nitemSummaryEntryPresent=" + ItemSummaryEntryPresent
                   + "\ndescriptorPresent=" + DescriptorPresent
                   + "\neventLogPresent=" + EventLogPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\npackageId=" + PackageId
                   + "\npackageTitle=" + PackageTitle
                   + "\nmapId=" + MapId
                   + "\nmapWidth=" + MapWidth
                   + "\nmapHeight=" + MapHeight
                   + "\nentityCount=" + EntityCount
                   + "\nitemCount=" + ItemCount
                   + "\nstatusLine=" + StatusLine;
        }
    }

    public sealed class GenericGamePackageProjectionLoopSmokeResult
    {
        public bool GenericLoopPassed;
        public bool SamplePackageLoaded;
        public bool GenericProjectionBuilt;
        public bool InteractionPreviewPresent;
        public bool InteractionApplyPassed;
        public bool DialogueSummaryPresent;
        public bool QuestObjectiveSummaryPresent;
        public bool InventorySummaryPresent;
        public bool ResourceSummaryPresent;
        public bool EventLogPresent;
        public bool ZeroFatalErrors;
        public string SelectedEntityId = string.Empty;
        public string SelectedInteractionId = string.Empty;
        public string SelectedDialogueId = string.Empty;
        public string SelectedQuestId = string.Empty;
        public int AppliedInteractionCount;
        public int StartedQuestCount;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return GenericLoopPassed
                       && SamplePackageLoaded
                       && GenericProjectionBuilt
                       && InteractionPreviewPresent
                       && InteractionApplyPassed
                       && DialogueSummaryPresent
                       && QuestObjectiveSummaryPresent
                       && InventorySummaryPresent
                       && ResourceSummaryPresent
                       && EventLogPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "genericLoopPassed=" + GenericLoopPassed
                   + "\nsamplePackageLoaded=" + SamplePackageLoaded
                   + "\ngenericProjectionBuilt=" + GenericProjectionBuilt
                   + "\ninteractionPreviewPresent=" + InteractionPreviewPresent
                   + "\ninteractionApplyPassed=" + InteractionApplyPassed
                   + "\ndialogueSummaryPresent=" + DialogueSummaryPresent
                   + "\nquestObjectiveSummaryPresent=" + QuestObjectiveSummaryPresent
                   + "\ninventorySummaryPresent=" + InventorySummaryPresent
                   + "\nresourceSummaryPresent=" + ResourceSummaryPresent
                   + "\neventLogPresent=" + EventLogPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\nselectedEntityId=" + SelectedEntityId
                   + "\nselectedInteractionId=" + SelectedInteractionId
                   + "\nselectedDialogueId=" + SelectedDialogueId
                   + "\nselectedQuestId=" + SelectedQuestId
                   + "\nappliedInteractionCount=" + AppliedInteractionCount
                   + "\nstartedQuestCount=" + StartedQuestCount
                   + "\nstatusLine=" + StatusLine;
        }
    }

    public sealed class GenericGamePackageProjectionSystemsSmokeResult
    {
        public bool GenericSystemsPassed;
        public bool SamplePackageLoaded;
        public bool GenericProjectionBuilt;
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
        public bool ZeroFatalErrors;
        public string RecipeId = string.Empty;
        public string ResourceNodeId = string.Empty;
        public string TransactionId = string.Empty;
        public string EncounterId = string.Empty;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return GenericSystemsPassed
                       && SamplePackageLoaded
                       && GenericProjectionBuilt
                       && InventoryInitialized
                       && ResourcesInitialized
                       && RecipePreviewPresent
                       && RecipeApplyPassed
                       && HarvestPreviewPresent
                       && HarvestApplyPassed
                       && TransactionPreviewPresent
                       && EncounterPreviewPresent
                       && CombatRoundPreviewPresent
                       && SystemsEventLogPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "genericSystemsPassed=" + GenericSystemsPassed
                   + "\nsamplePackageLoaded=" + SamplePackageLoaded
                   + "\ngenericProjectionBuilt=" + GenericProjectionBuilt
                   + "\ninventoryInitialized=" + InventoryInitialized
                   + "\nresourcesInitialized=" + ResourcesInitialized
                   + "\nrecipePreviewPresent=" + RecipePreviewPresent
                   + "\nrecipeApplyPassed=" + RecipeApplyPassed
                   + "\nharvestPreviewPresent=" + HarvestPreviewPresent
                   + "\nharvestApplyPassed=" + HarvestApplyPassed
                   + "\ntransactionPreviewPresent=" + TransactionPreviewPresent
                   + "\nencounterPreviewPresent=" + EncounterPreviewPresent
                   + "\ncombatRoundPreviewPresent=" + CombatRoundPreviewPresent
                   + "\nsystemsEventLogPresent=" + SystemsEventLogPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\nrecipeId=" + RecipeId
                   + "\nresourceNodeId=" + ResourceNodeId
                   + "\ntransactionId=" + TransactionId
                   + "\nencounterId=" + EncounterId
                   + "\nstatusLine=" + StatusLine;
        }
    }
}
