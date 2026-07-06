using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class GenericGamePackageProjectionController : MonoBehaviour
    {
        public const string GenericSectionName = "goal123_generic_gamepackage_projection";

        private GenericGamePackageProjectionModel model = new GenericGamePackageProjectionModel();
        private string statusLine = "Generic package projection not loaded";
        private string lastDiagnostics = string.Empty;
        private string lastSmokeDiagnostics = string.Empty;
        private string selectedMarkerDetails = string.Empty;
        private string verificationEventLog = string.Empty;
        private string selectedMarkerId = string.Empty;
        private string selectedMarkerKind = string.Empty;
        private bool lastVerificationPassed;
        private bool lastGenericLoopVerificationPassed;
        private bool lastGenericSystemsVerificationPassed;
        private bool lastGenericFullPlaythroughVerificationPassed;
        private int fatalErrorCount;
        private GenericGamePackageProjectionState loopState = new GenericGamePackageProjectionState();

        public string StatusLine { get { return statusLine; } }
        public string LastDiagnostics { get { return lastDiagnostics; } }
        public string LastSmokeDiagnostics { get { return lastSmokeDiagnostics; } }
        public string SelectedMarkerDetails { get { return selectedMarkerDetails; } }
        public string VerificationEventLog { get { return verificationEventLog; } }
        public string SelectedMarkerId { get { return selectedMarkerId; } }
        public string SelectedMarkerKind { get { return selectedMarkerKind; } }
        public bool LastVerificationPassed { get { return lastVerificationPassed; } }
        public bool LastGenericLoopVerificationPassed { get { return lastGenericLoopVerificationPassed; } }
        public bool LastGenericSystemsVerificationPassed { get { return lastGenericSystemsVerificationPassed; } }
        public bool LastGenericFullPlaythroughVerificationPassed
        {
            get { return lastGenericFullPlaythroughVerificationPassed; }
        }
        public string PackageId { get { return model.PackageId; } }
        public string PackageTitle { get { return model.PackageTitle; } }
        public string MapId { get { return model.MapId; } }
        public int MapWidth { get { return model.MapWidth; } }
        public int MapHeight { get { return model.MapHeight; } }
        public int EntityCount { get { return model.Entities.Count; } }
        public int ItemCount { get { return model.Items.Count; } }
        public string InventorySummary { get { return loopState.inventorySummary; } }
        public string ResourceSummary { get { return loopState.resourceSummary; } }
        public string QuestObjectiveSummary { get { return loopState.questObjectiveSummary; } }
        public string InteractionEffectPreview { get { return loopState.interactionEffectPreview; } }
        public string RecipePreview { get { return loopState.recipePreview; } }
        public string RecipeApplyResult { get { return loopState.recipeApplyResult; } }
        public string HarvestPreview { get { return loopState.harvestPreview; } }
        public string HarvestApplyResult { get { return loopState.harvestApplyResult; } }
        public string TransactionPreview { get { return loopState.transactionPreview; } }
        public string EncounterPreview { get { return loopState.encounterPreview; } }
        public string CombatRoundPreview { get { return loopState.combatRoundPreview; } }
        public string SystemsEventLog { get { return loopState.systemsEventLog; } }
        public string FullPlaythroughStatus { get { return loopState.fullPlaythroughStatus; } }
        public string MovementPathSummary { get { return loopState.movementPathSummary; } }
        public string SignInteractionResult { get { return loopState.signInteractionResult; } }
        public string DialogueSummary { get { return loopState.dialogueSummary; } }
        public string QuestObjectiveStatus { get { return loopState.questObjectiveStatus; } }
        public string InventoryResourceFinalSummary
        {
            get { return loopState.inventoryResourceFinalSummary; }
        }
        public string SystemsSummary { get { return loopState.systemsSummary; } }
        public string CombatSummary { get { return loopState.combatSummary; } }
        public string EventTranscriptSummary { get { return loopState.eventTranscriptSummary; } }
        public string FinalStateSummary { get { return loopState.finalStateSummary; } }
        public string SelectedDialogueId { get { return loopState.selectedDialogueId; } }
        public string SelectedQuestId { get { return loopState.selectedQuestId; } }
        public int AppliedInteractionCount { get { return loopState.appliedInteractionCount; } }
        public int StartedQuestCount { get { return loopState.startedQuestCount; } }

        public void BuildOrRefreshGenericPackageProjection()
        {
            fatalErrorCount = 0;
            selectedMarkerDetails = string.Empty;
            verificationEventLog = string.Empty;
            selectedMarkerId = string.Empty;
            selectedMarkerKind = string.Empty;
            lastVerificationPassed = false;
            lastGenericLoopVerificationPassed = false;
            lastGenericSystemsVerificationPassed = false;
            lastGenericFullPlaythroughVerificationPassed = false;
            loopState = new GenericGamePackageProjectionState();

            try
            {
                var diagnostics = new List<string>();
                model = GenericGamePackageProjectionAdapter.LoadSamplePackageProjection(diagnostics);
                ClearGenericSection();

                var section = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                    transform,
                    GenericSectionName,
                    new Vector3(-2f, 0f, -10f));
                AttachDescriptor(section, "goal123_generic_projection_root", "generic_package_root",
                    "Generic GamePackage Projection", "ready",
                    "Projection-only preview for " + GenericGamePackageProjectionAdapter.SamplePackageRelativePath);

                RenderPackageHeader(section.transform);
                RenderMapGrid(section.transform);
                RenderPlayerStart(section.transform);
                RenderEntities(section.transform);
                RenderItemSummary(section.transform);
                RenderStatusPanel(section.transform);

                lastDiagnostics = model.Diagnostics.Count == 0
                    ? "No diagnostics."
                    : string.Join("\n", model.Diagnostics.ToArray());
                statusLine = "Goal123 generic package projection built for "
                             + EmptyAsNone(model.PackageTitle)
                             + " (" + EmptyAsNone(model.PackageId) + ")";
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal123 generic package projection fatal error: " + ex.GetType().Name;
                lastDiagnostics = statusLine + "\n" + ex.Message;
            }
        }

        public bool RunGenericPackageProjectionVerification()
        {
            var events = new List<string>();
            try
            {
                events.Add("loadSamplePackage=" + GenericGamePackageProjectionAdapter.SamplePackageRelativePath);
                BuildOrRefreshGenericPackageProjection();
                events.Add("sectionPresent=" + (FindGenericProjectionRoot() != null));
                events.Add("packageId=" + model.PackageId);
                events.Add("packageTitle=" + model.PackageTitle);
                events.Add("mapId=" + model.MapId);
                events.Add("mapSize=" + model.MapWidth + "x" + model.MapHeight);
                events.Add("entityCount=" + model.Entities.Count);
                events.Add("itemCount=" + model.Items.Count);

                var firstEntity = FindFirstGenericEntityMarker();
                SelectMarker(firstEntity ?? FindGenericProjectionRoot());
                events.Add("selectedMarkerId=" + EmptyAsNone(selectedMarkerId));
                events.Add("selectedMarkerKind=" + EmptyAsNone(selectedMarkerKind));

                var smoke = RunGenericPackageProjectionSmoke(events);
                lastVerificationPassed = smoke;
                events.Add(smoke
                    ? "Goal123 generic package projection verification passed"
                    : "Goal123 generic package projection verification failed");
                verificationEventLog = string.Join("\n", events.ToArray());
                statusLine = smoke
                    ? "Goal123 generic package projection verification passed"
                    : "Goal123 generic package projection verification failed";
                return smoke;
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal123 generic package projection verification fatal error: "
                             + ex.GetType().Name;
                events.Add(statusLine);
                events.Add(ex.Message);
                verificationEventLog = string.Join("\n", events.ToArray());
                RunGenericPackageProjectionSmoke(events);
                return false;
            }
        }

        public bool RunGenericPackageGameplayLoopVerification()
        {
            var events = new List<string>();
            try
            {
                events.Add("loadSamplePackage=" + GenericGamePackageProjectionAdapter.SamplePackageRelativePath);
                BuildOrRefreshGenericPackageProjection();
                var section = FindGenericProjectionRoot();
                events.Add("sectionPresent=" + (section != null));
                loopState = new GenericGamePackageProjectionLoop().Run(model, events);
                RenderLoopPanels(section == null ? transform : section.transform);
                SelectMarker(FindSignEntityMarker() ?? FindFirstGenericEntityMarker() ?? section);

                var smoke = RunGenericPackageLoopSmoke(events);
                lastGenericLoopVerificationPassed = smoke;
                lastVerificationPassed = smoke;
                events.Add(smoke
                    ? "Goal124 generic package loop verification passed"
                    : "Goal124 generic package loop verification failed");
                verificationEventLog = string.Join("\n", events.ToArray());
                statusLine = smoke
                    ? "Goal124 generic package loop verification passed"
                    : "Goal124 generic package loop verification failed";
                return smoke;
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal124 generic package loop verification fatal error: "
                             + ex.GetType().Name;
                events.Add(statusLine);
                events.Add(ex.Message);
                verificationEventLog = string.Join("\n", events.ToArray());
                RunGenericPackageLoopSmoke(events);
                return false;
            }
        }

        public bool RunGenericPackageSystemsLoopVerification()
        {
            var events = new List<string>();
            try
            {
                events.Add("loadSamplePackage=" + GenericGamePackageProjectionAdapter.SamplePackageRelativePath);
                BuildOrRefreshGenericPackageProjection();
                var section = FindGenericProjectionRoot();
                events.Add("sectionPresent=" + (section != null));
                loopState = new GenericGamePackageProjectionSystems().Run(model, events);
                RenderSystemsPanels(section == null ? transform : section.transform);
                SelectMarker(
                    FindDescendantObjectWithPrefix(transform, "goal125_systems_loop_status")
                    ?? FindGenericProjectionRoot());

                var smoke = RunGenericPackageSystemsSmoke(events);
                lastGenericSystemsVerificationPassed = smoke;
                lastVerificationPassed = smoke;
                events.Add(smoke
                    ? "Goal125 generic package systems loop verification passed"
                    : "Goal125 generic package systems loop verification failed");
                verificationEventLog = string.Join("\n", events.ToArray());
                statusLine = smoke
                    ? "Goal125 generic package systems loop verification passed"
                    : "Goal125 generic package systems loop verification failed";
                return smoke;
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal125 generic package systems loop verification fatal error: "
                             + ex.GetType().Name;
                events.Add(statusLine);
                events.Add(ex.Message);
                verificationEventLog = string.Join("\n", events.ToArray());
                RunGenericPackageSystemsSmoke(events);
                return false;
            }
        }

        public bool RunGenericPackageFullPlaythroughVerification()
        {
            var events = new List<string>();
            try
            {
                events.Add("loadSamplePackage=" + GenericGamePackageProjectionAdapter.SamplePackageRelativePath);
                BuildOrRefreshGenericPackageProjection();
                var section = FindGenericProjectionRoot();
                events.Add("sectionPresent=" + (section != null));
                loopState = new GenericGamePackageProjectionPlaythrough().Run(model, events);
                RenderFullPlaythroughPanels(section == null ? transform : section.transform);
                SelectMarker(
                    FindDescendantObjectWithPrefix(transform, "goal126_full_playthrough_status")
                    ?? FindGenericProjectionRoot());

                var smoke = RunGenericPackageFullPlaythroughSmoke(events);
                lastGenericFullPlaythroughVerificationPassed = smoke;
                lastGenericLoopVerificationPassed = smoke;
                lastGenericSystemsVerificationPassed = smoke;
                lastVerificationPassed = smoke;
                events.Add(smoke
                    ? "Goal126 generic package full playthrough verification passed"
                    : "Goal126 generic package full playthrough verification failed");
                verificationEventLog = string.Join("\n", events.ToArray());
                statusLine = smoke
                    ? "Goal126 generic package full playthrough verification passed"
                    : "Goal126 generic package full playthrough verification failed";
                return smoke;
            }
            catch (System.Exception ex)
            {
                fatalErrorCount++;
                statusLine = "Goal126 generic package full playthrough verification fatal error: "
                             + ex.GetType().Name;
                events.Add(statusLine);
                events.Add(ex.Message);
                verificationEventLog = string.Join("\n", events.ToArray());
                RunGenericPackageFullPlaythroughSmoke(events);
                return false;
            }
        }

        public GameObject FindGenericProjectionRoot()
        {
            return FindDescendantObjectWithPrefix(transform, GenericSectionName);
        }

        public GameObject FindFirstGenericEntityMarker()
        {
            return FindNextMarkerByKind("entity", 0);
        }

        public GameObject FindSignEntityMarker()
        {
            return FindDescendantObjectWithPrefix(
                transform,
                "goal123_entity_marker_entity_village_sign");
        }

        private bool RunGenericPackageProjectionSmoke(List<string> events)
        {
            var result = new GenericGamePackageProjectionSmokeResult
            {
                SectionPresent = FindGenericProjectionRoot() != null,
                PackageIdentityPresent =
                    !string.IsNullOrWhiteSpace(model.PackageId)
                    && !string.IsNullOrWhiteSpace(model.PackageTitle),
                MapDimensionsPresent =
                    !string.IsNullOrWhiteSpace(model.MapId)
                    && model.MapWidth > 0
                    && model.MapHeight > 0,
                StartPlayerMarkerPresent = HasDescendantWithPrefix(transform, "goal123_start_player_proxy"),
                TileMarkerPresent = HasDescendantWithPrefix(transform, "goal123_tile_"),
                EntityMarkerPresent = HasDescendantWithDescriptorKind(transform, "entity"),
                InteractionMarkerPresent = HasDescendantWithDescriptorKind(transform, "interaction"),
                ItemSummaryEntryPresent = HasDescendantWithPrefix(transform, "goal123_item_summary_entry_"),
                DescriptorPresent = HasDescendantWithDescriptor(transform),
                EventLogPresent = events != null && events.Count > 0,
                ZeroFatalErrors = fatalErrorCount == 0,
                PackageId = model.PackageId,
                PackageTitle = model.PackageTitle,
                MapId = model.MapId,
                MapWidth = model.MapWidth,
                MapHeight = model.MapHeight,
                EntityCount = model.Entities.Count,
                ItemCount = model.Items.Count,
                StatusLine = statusLine
            };
            lastSmokeDiagnostics = result.ToDiagnosticText();
            return result.Passed;
        }

        private bool RunGenericPackageLoopSmoke(List<string> events)
        {
            var result = new GenericGamePackageProjectionLoopSmokeResult
            {
                GenericLoopPassed =
                    loopState.InteractionApplyPassed
                    && loopState.DialogueSummaryPresent
                    && loopState.QuestObjectiveSummaryPresent
                    && loopState.InventorySummaryPresent
                    && loopState.ResourceSummaryPresent,
                SamplePackageLoaded = loopState.SamplePackageLoaded,
                GenericProjectionBuilt =
                    FindGenericProjectionRoot() != null
                    && HasDescendantWithPrefix(transform, "goal124_generic_loop_status"),
                InteractionPreviewPresent =
                    loopState.InteractionPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal124_interaction_preview"),
                InteractionApplyPassed =
                    loopState.InteractionApplyPassed
                    && HasDescendantWithPrefix(transform, "goal124_interaction_effect"),
                DialogueSummaryPresent =
                    loopState.DialogueSummaryPresent
                    && HasDescendantWithPrefix(transform, "goal124_dialogue_summary"),
                QuestObjectiveSummaryPresent =
                    loopState.QuestObjectiveSummaryPresent
                    && HasDescendantWithPrefix(transform, "goal124_quest_objective_status"),
                InventorySummaryPresent =
                    loopState.InventorySummaryPresent
                    && HasDescendantWithPrefix(transform, "goal124_inventory_summary"),
                ResourceSummaryPresent =
                    loopState.ResourceSummaryPresent
                    && HasDescendantWithPrefix(transform, "goal124_resource_summary"),
                EventLogPresent =
                    loopState.EventLogPresent
                    && HasDescendantWithPrefix(transform, "goal124_event_log_summary"),
                ZeroFatalErrors = fatalErrorCount == 0,
                SelectedEntityId = loopState.selectedEntityId,
                SelectedInteractionId = loopState.selectedInteractionId,
                SelectedDialogueId = loopState.selectedDialogueId,
                SelectedQuestId = loopState.selectedQuestId,
                AppliedInteractionCount = loopState.appliedInteractionCount,
                StartedQuestCount = loopState.startedQuestCount,
                StatusLine = statusLine
            };
            lastSmokeDiagnostics = result.ToDiagnosticText();
            if (events != null)
            {
                events.Add("genericLoopPassed=" + result.Passed);
            }

            return result.Passed;
        }

        private bool RunGenericPackageSystemsSmoke(List<string> events)
        {
            var result = new GenericGamePackageProjectionSystemsSmokeResult
            {
                GenericSystemsPassed = loopState.GenericSystemsPassed,
                SamplePackageLoaded = loopState.SamplePackageLoaded,
                GenericProjectionBuilt =
                    FindGenericProjectionRoot() != null
                    && HasDescendantWithPrefix(transform, "goal125_systems_loop_status"),
                InventoryInitialized = loopState.InventoryInitialized,
                ResourcesInitialized = loopState.ResourcesInitialized,
                RecipePreviewPresent =
                    loopState.RecipePreviewPresent
                    && HasDescendantWithPrefix(transform, "goal125_recipe_craft_result"),
                RecipeApplyPassed = loopState.RecipeApplyPassed,
                HarvestPreviewPresent =
                    loopState.HarvestPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal125_harvest_result"),
                HarvestApplyPassed = loopState.HarvestApplyPassed,
                TransactionPreviewPresent =
                    loopState.TransactionPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal125_transaction_preview"),
                EncounterPreviewPresent =
                    loopState.EncounterPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal125_encounter_combat_preview"),
                CombatRoundPreviewPresent = loopState.CombatRoundPreviewPresent,
                SystemsEventLogPresent =
                    loopState.SystemsEventLogPresent
                    && HasDescendantWithPrefix(transform, "goal125_systems_event_log_summary"),
                ZeroFatalErrors = fatalErrorCount == 0,
                RecipeId = "recipe/healing_potion",
                ResourceNodeId = "node/apple_tree",
                TransactionId = "transaction/buy_healing_potion",
                EncounterId = "encounter/goblin_duel",
                StatusLine = statusLine
            };
            lastSmokeDiagnostics = result.ToDiagnosticText();
            if (events != null)
            {
                events.Add("genericSystemsPassed=" + result.Passed);
            }

            return result.Passed;
        }

        private bool RunGenericPackageFullPlaythroughSmoke(List<string> events)
        {
            var result = new GenericGamePackageProjectionFullPlaythroughSmokeResult
            {
                FullPlaythroughPassed = loopState.FullPlaythroughPassed,
                SamplePackageLoaded = loopState.SamplePackageLoaded,
                GenericProjectionBuilt =
                    FindGenericProjectionRoot() != null
                    && HasDescendantWithPrefix(transform, "goal126_full_playthrough_status"),
                MapPathPreviewPresent =
                    loopState.MapPathPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal126_movement_path_summary"),
                SignInteractionApplied =
                    loopState.SignInteractionApplied
                    && HasDescendantWithPrefix(transform, "goal126_sign_interaction_result"),
                DialogueSummaryPresent =
                    loopState.DialogueSummaryPresent
                    && HasDescendantWithPrefix(transform, "goal126_dialogue_summary"),
                QuestObjectiveStatusPresent =
                    loopState.QuestObjectiveStatusPresent
                    && HasDescendantWithPrefix(transform, "goal126_quest_objective_status"),
                InventorySummaryPresent =
                    loopState.InventorySummaryPresent
                    && HasDescendantWithPrefix(transform, "goal126_inventory_resource_final_summary"),
                ResourceSummaryPresent =
                    loopState.ResourceSummaryPresent
                    && HasDescendantWithPrefix(transform, "goal126_inventory_resource_final_summary"),
                RecipeApplyPassed = loopState.RecipeApplyPassed,
                HarvestApplyPassed = loopState.HarvestApplyPassed,
                TransactionPreviewPresent =
                    loopState.TransactionPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal126_systems_summary"),
                CombatRoundPreviewPresent =
                    loopState.CombatRoundPreviewPresent
                    && HasDescendantWithPrefix(transform, "goal126_combat_round_summary"),
                EventTranscriptPresent =
                    loopState.EventTranscriptPresent
                    && HasDescendantWithPrefix(transform, "goal126_event_transcript_summary"),
                ZeroFatalErrors = fatalErrorCount == 0,
                PackageId = model.PackageId,
                PackageTitle = model.PackageTitle,
                MapId = model.MapId,
                StatusLine = statusLine
            };
            lastSmokeDiagnostics = result.ToDiagnosticText();
            if (events != null)
            {
                events.Add("fullPlaythroughPassed=" + result.Passed);
            }

            return result.Passed;
        }

        private void RenderPackageHeader(Transform parent)
        {
            var header = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                "goal123_package_header",
                model.PackageTitle + " | " + model.PackageId,
                new Vector3(0f, 1.2f, -1.4f),
                Color.white,
                0.28f);
            AttachDescriptor(header, "goal123_package_header", "label",
                "Package title/id", "ready",
                "packageId=" + model.PackageId + "; title=" + model.PackageTitle);

            var map = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                "goal123_map_header",
                "Map " + model.MapId + " " + model.MapWidth + "x" + model.MapHeight
                + " start=(" + model.StartX + "," + model.StartY + ")",
                new Vector3(0f, 0.8f, -1.4f),
                Color.cyan,
                0.22f);
            AttachDescriptor(map, "goal123_map_header", "label",
                "Map dimensions", "ready",
                "mapId=" + model.MapId + "; mapName=" + model.MapName);
        }

        private void RenderMapGrid(Transform parent)
        {
            var mapSection = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                "goal123_map_grid",
                Vector3.zero);
            foreach (var tile in model.Tiles)
            {
                var markerName = "goal123_tile_"
                                 + tile.X
                                 + "_"
                                 + tile.Y
                                 + "_"
                                 + AcceptedAlphaPlayableProjectionDiagnostics.Compact(tile.TileId);
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    mapSection.transform,
                    markerName,
                    PrimitiveType.Cube,
                    ColorForTile(tile),
                    GridPosition(tile.X, tile.Y, 0.02f),
                    tile.TileKind == "wall"
                        ? new Vector3(0.82f, 0.42f, 0.82f)
                        : new Vector3(0.82f, 0.05f, 0.82f));
                AttachDescriptor(marker, markerName, "tile",
                    tile.TileId, tile.Explicit ? "explicit" : "default",
                    "tileId=" + tile.TileId
                    + "; tileName=" + tile.TileName
                    + "; tileKind=" + tile.TileKind
                    + "; walkable=" + tile.Walkable);
            }
        }

        private void RenderPlayerStart(Transform parent)
        {
            var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                parent,
                "goal123_start_player_proxy",
                PrimitiveType.Capsule,
                Color.white,
                GridPosition(model.StartX, model.StartY, 0.75f),
                new Vector3(0.48f, 1.2f, 0.48f));
            AttachDescriptor(marker, "goal123_start_player_proxy", "player",
                "Start/player proxy", "ready",
                "startPosition=(" + model.StartX + "," + model.StartY + ")");
        }

        private void RenderEntities(Transform parent)
        {
            var index = 0;
            foreach (var entity in model.Entities)
            {
                var compact = AcceptedAlphaPlayableProjectionDiagnostics.Compact(entity.EntityId);
                var marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                    parent,
                    "goal123_entity_marker_" + compact,
                    PrimitiveType.Sphere,
                    entity.Interactable ? Color.yellow : new Color(0.65f, 0.75f, 1f),
                    GridPosition(entity.X, entity.Y, 0.62f),
                    Vector3.one * 0.48f);
                AttachDescriptor(marker, entity.EntityId, "entity",
                    EntityLabel(entity), "ready",
                    EntityDetails(entity));

                var label = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    parent,
                    "goal123_entity_label_" + index,
                    EntityLabel(entity),
                    GridPosition(entity.X, entity.Y, 1.15f) + new Vector3(0.35f, 0f, 0.25f),
                    Color.white,
                    0.18f);
                AttachDescriptor(label, entity.EntityId + ".label", "label",
                    EntityLabel(entity), "ready", EntityDetails(entity));

                if (entity.Interactable)
                {
                    var interaction = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateMarker(
                        parent,
                        "goal123_interaction_marker_" + compact,
                        PrimitiveType.Cylinder,
                        new Color(1f, 0.55f, 0.15f),
                        GridPosition(entity.X, entity.Y, 1.0f) + new Vector3(0.28f, 0f, 0.28f),
                        new Vector3(0.24f, 0.16f, 0.24f));
                    AttachDescriptor(interaction, entity.EntityId + ".interaction", "interaction",
                        EntityLabel(entity), "selectable", EntityDetails(entity));
                }

                index++;
            }
        }

        private void RenderItemSummary(Transform parent)
        {
            var itemSection = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                "goal123_item_summary_panel",
                new Vector3(model.MapWidth * 0.95f + 1.5f, 0f, 0f));
            var header = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                itemSection.transform,
                "goal123_item_summary_header",
                "Items: " + model.Items.Count,
                new Vector3(0f, 1f, -0.6f),
                Color.green,
                0.24f);
            AttachDescriptor(header, "goal123_item_summary_header", "item_summary",
                "Item summary", "ready", "itemCount=" + model.Items.Count);

            var index = 0;
            foreach (var item in model.Items)
            {
                var text = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    itemSection.transform,
                    "goal123_item_summary_entry_" + index,
                    item.ItemId + " | " + item.Name + " | " + item.Kind,
                    new Vector3(0f, 0.62f - index * 0.32f, -0.6f),
                    Color.white,
                    0.16f);
                AttachDescriptor(text, item.ItemId, "item_summary",
                    item.Name, "ready",
                    "itemId=" + item.ItemId + "; name=" + item.Name + "; kind=" + item.Kind);
                index++;
            }
        }

        private void RenderStatusPanel(Transform parent)
        {
            var status = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                "goal123_package_event_log_status",
                "Goal123 package verification: package="
                + EmptyAsNone(model.PackageId)
                + " map="
                + EmptyAsNone(model.MapId)
                + " entities="
                + model.Entities.Count
                + " items="
                + model.Items.Count,
                new Vector3(0f, 1.55f, model.MapHeight * 0.92f + 0.8f),
                Color.magenta,
                0.22f);
            AttachDescriptor(status, "goal123_package_event_log_status", "diagnostics",
                "Package verification event log", "ready",
                "samplePackagePath=" + GenericGamePackageProjectionAdapter.SamplePackageRelativePath
                + "; diagnostics=" + model.Diagnostics.Count);
        }

        private void RenderLoopPanels(Transform parent)
        {
            var panel = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                "goal124_generic_loop_panel",
                new Vector3(0f, 0f, model.MapHeight * 0.92f + 1.75f));
            RenderLoopLine(panel.transform, "goal124_generic_loop_status",
                "Generic loop: "
                + (loopState.GenericProjectionBuilt ? "passed" : "not passed")
                + " appliedInteractions="
                + loopState.appliedInteractionCount
                + " startedQuests="
                + loopState.startedQuestCount,
                0f,
                Color.white,
                "generic_package_loop_status",
                "genericLoopPassed=" + loopState.GenericProjectionBuilt);
            RenderLoopLine(panel.transform, "goal124_selected_entity",
                "Selected entity: " + EmptyAsNone(loopState.selectedEntityId),
                -0.35f,
                Color.cyan,
                "selected_entity",
                "selectedEntityId=" + loopState.selectedEntityId);
            RenderLoopLine(panel.transform, "goal124_interaction_preview",
                "Interaction preview: " + EmptyAsNone(loopState.selectedInteractionId),
                -0.7f,
                new Color(1f, 0.75f, 0.25f),
                "interaction_preview",
                loopState.interactionEffectPreview);
            RenderLoopLine(panel.transform, "goal124_interaction_effect",
                "Applied effect: " + EmptyAsNone(loopState.interactionEffectPreview),
                -1.05f,
                new Color(1f, 0.55f, 0.15f),
                "interaction_effect",
                "appliedInteractionCount=" + loopState.appliedInteractionCount);
            RenderLoopLine(panel.transform, "goal124_dialogue_summary",
                "Dialogue: " + EmptyAsNone(loopState.selectedDialogueId),
                -1.4f,
                new Color(0.7f, 0.85f, 1f),
                "dialogue_summary",
                "dialogueSummaryPresent=" + loopState.DialogueSummaryPresent);
            RenderLoopLine(panel.transform, "goal124_quest_objective_status",
                "Quest objective: " + EmptyAsNone(loopState.questObjectiveSummary),
                -1.75f,
                Color.green,
                "quest_objective_status",
                loopState.questObjectiveSummary);
            RenderLoopLine(panel.transform, "goal124_inventory_summary",
                "Inventory: " + EmptyAsNone(loopState.inventorySummary),
                -2.1f,
                new Color(0.75f, 1f, 0.75f),
                "inventory_summary",
                loopState.inventorySummary);
            RenderLoopLine(panel.transform, "goal124_resource_summary",
                "Resources: " + EmptyAsNone(loopState.resourceSummary),
                -2.45f,
                new Color(0.85f, 0.75f, 1f),
                "resource_summary",
                loopState.resourceSummary);
            RenderLoopLine(panel.transform, "goal124_event_log_summary",
                "Event log: " + loopState.events.Count + " entries",
                -2.8f,
                Color.magenta,
                "event_log_summary",
                loopState.projectionEventLog);
        }

        private void RenderSystemsPanels(Transform parent)
        {
            var panel = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                "goal125_generic_systems_loop_panel",
                new Vector3(model.MapWidth * 0.95f + 1.8f, 0f, model.MapHeight * 0.92f + 1.75f));
            RenderLoopLine(panel.transform, "goal125_systems_loop_status",
                "Systems loop: " + (loopState.GenericSystemsPassed ? "passed" : "not passed"),
                0f,
                Color.white,
                "systems_loop_status",
                "genericSystemsPassed=" + loopState.GenericSystemsPassed);
            RenderLoopLine(panel.transform, "goal125_inventory_summary",
                "Inventory: " + EmptyAsNone(loopState.inventorySummary),
                -0.35f,
                new Color(0.75f, 1f, 0.75f),
                "inventory_summary",
                loopState.inventorySummary);
            RenderLoopLine(panel.transform, "goal125_resource_ledger_summary",
                "Resources: " + EmptyAsNone(loopState.resourceSummary),
                -0.7f,
                new Color(0.85f, 0.75f, 1f),
                "resource_ledger_summary",
                loopState.resourceSummary);
            RenderLoopLine(panel.transform, "goal125_recipe_craft_result",
                "Recipe: " + EmptyAsNone(loopState.recipeApplyResult),
                -1.05f,
                new Color(1f, 0.75f, 0.25f),
                "recipe_craft_preview",
                loopState.recipePreview + "\n" + loopState.recipeApplyResult);
            RenderLoopLine(panel.transform, "goal125_harvest_result",
                "Harvest: " + EmptyAsNone(loopState.harvestApplyResult),
                -1.4f,
                Color.green,
                "harvest_preview",
                loopState.harvestPreview + "\n" + loopState.harvestApplyResult);
            RenderLoopLine(panel.transform, "goal125_transaction_preview",
                "Transaction: " + EmptyAsNone(loopState.transactionPreview),
                -1.75f,
                new Color(1f, 0.55f, 0.15f),
                "transaction_preview",
                loopState.transactionPreview);
            RenderLoopLine(panel.transform, "goal125_encounter_combat_preview",
                "Combat: " + EmptyAsNone(loopState.combatRoundPreview),
                -2.1f,
                new Color(0.7f, 0.85f, 1f),
                "encounter_combat_preview",
                loopState.encounterPreview + "\n" + loopState.combatRoundPreview);
            RenderLoopLine(panel.transform, "goal125_systems_event_log_summary",
                "Systems event log: " + loopState.systemsEvents.Count + " entries",
                -2.45f,
                Color.magenta,
                "systems_event_log_summary",
                loopState.systemsEventLog);
        }

        private void RenderFullPlaythroughPanels(Transform parent)
        {
            var panel = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                parent,
                "goal126_generic_full_playthrough_panel",
                new Vector3(0f, 0f, model.MapHeight * 0.92f + 5.05f));
            RenderLoopLine(panel.transform, "goal126_full_playthrough_status",
                "Full playthrough: " + EmptyAsNone(loopState.fullPlaythroughStatus),
                0f,
                Color.white,
                "full_playthrough_status",
                "fullPlaythroughPassed=" + loopState.FullPlaythroughPassed);
            RenderLoopLine(panel.transform, "goal126_movement_path_summary",
                "Movement/path: " + EmptyAsNone(loopState.movementPathSummary),
                -0.35f,
                Color.cyan,
                "movement_path_summary",
                loopState.movementPathSummary);
            RenderLoopLine(panel.transform, "goal126_sign_interaction_result",
                "Sign interaction: " + EmptyAsNone(loopState.signInteractionResult),
                -0.7f,
                new Color(1f, 0.75f, 0.25f),
                "sign_interaction_result",
                loopState.signInteractionResult);
            RenderLoopLine(panel.transform, "goal126_dialogue_summary",
                "Dialogue: " + EmptyAsNone(loopState.dialogueSummary),
                -1.05f,
                new Color(0.7f, 0.85f, 1f),
                "dialogue_summary",
                loopState.dialogueSummary);
            RenderLoopLine(panel.transform, "goal126_quest_objective_status",
                "Quest objective: " + EmptyAsNone(loopState.questObjectiveStatus),
                -1.4f,
                Color.green,
                "quest_objective_status",
                loopState.questObjectiveStatus);
            RenderLoopLine(panel.transform, "goal126_inventory_resource_final_summary",
                "Inventory/resources: " + EmptyAsNone(loopState.inventoryResourceFinalSummary),
                -1.75f,
                new Color(0.75f, 1f, 0.75f),
                "inventory_resource_final_summary",
                loopState.inventoryResourceFinalSummary);
            RenderLoopLine(panel.transform, "goal126_systems_summary",
                "Systems: " + EmptyAsNone(loopState.systemsSummary),
                -2.1f,
                new Color(1f, 0.55f, 0.15f),
                "craft_harvest_transaction_summary",
                loopState.systemsSummary);
            RenderLoopLine(panel.transform, "goal126_combat_round_summary",
                "Combat: " + EmptyAsNone(loopState.combatSummary),
                -2.45f,
                new Color(0.7f, 0.85f, 1f),
                "combat_round_summary",
                loopState.combatSummary);
            RenderLoopLine(panel.transform, "goal126_event_transcript_summary",
                "Event transcript: " + EmptyAsNone(loopState.eventTranscriptSummary),
                -2.8f,
                Color.magenta,
                "event_transcript_summary",
                loopState.eventTranscriptSummary + "\n" + loopState.projectionEventLog);
        }

        private static void RenderLoopLine(
            Transform parent,
            string name,
            string text,
            float y,
            Color color,
            string markerKind,
            string details)
        {
            var obj = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                parent,
                name,
                text,
                new Vector3(0f, y, 0f),
                color,
                0.18f);
            AttachDescriptor(obj, name, markerKind, text, "ready", details);
        }

        private void SelectMarker(GameObject marker)
        {
            var descriptor = marker == null
                ? null
                : marker.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor == null)
            {
                selectedMarkerId = string.Empty;
                selectedMarkerKind = string.Empty;
                selectedMarkerDetails = "markerSelected=false";
                return;
            }

            selectedMarkerId = descriptor.MarkerId;
            selectedMarkerKind = descriptor.MarkerKind;
            selectedMarkerDetails = AcceptedAlphaPlayableProjectionDrilldown.DescribeMarker(marker);
        }

        private static void AttachDescriptor(
            GameObject target,
            string markerId,
            string markerKind,
            string displayLabel,
            string status,
            string details)
        {
            AcceptedAlphaPlayableProjectionPrimitiveFactory.AttachDescriptor(
                target,
                markerId,
                target.name,
                markerKind,
                "goal123",
                GenericGamePackageProjectionAdapter.SamplePackageRelativePath,
                displayLabel,
                status,
                details);
        }

        private void ClearGenericSection()
        {
            var section = FindGenericProjectionRoot();
            if (section == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(section);
            }
            else
            {
                DestroyImmediate(section);
            }
        }

        private static Vector3 GridPosition(int x, int y, float height)
        {
            return new Vector3(x * 0.9f, height, y * 0.9f);
        }

        private static Color ColorForTile(GenericGamePackageProjectionTile tile)
        {
            if (tile.TileKind == "wall")
            {
                return new Color(0.38f, 0.38f, 0.42f);
            }

            if (tile.TileKind == "road")
            {
                return new Color(0.64f, 0.52f, 0.36f);
            }

            return tile.Explicit ? new Color(0.28f, 0.62f, 0.3f) : new Color(0.18f, 0.46f, 0.2f);
        }

        private static string EntityLabel(GenericGamePackageProjectionEntity entity)
        {
            return string.IsNullOrWhiteSpace(entity.PrototypeName)
                ? entity.EntityId
                : entity.PrototypeName;
        }

        private static string EntityDetails(GenericGamePackageProjectionEntity entity)
        {
            return "entityId=" + entity.EntityId
                   + "; prototypeId=" + entity.PrototypeId
                   + "; position=(" + entity.X + "," + entity.Y + ")"
                   + "; interactable=" + entity.Interactable
                   + "; interactionId=" + EmptyAsNone(entity.InteractionId)
                   + "; dialogueId=" + EmptyAsNone(entity.DialogueId)
                   + "; text=" + EmptyAsNone(entity.InteractionText);
        }

        private static bool HasDescendantWithPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return true;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (HasDescendantWithPrefix(root.GetChild(i), prefix))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDescendantWithDescriptor(Transform root)
        {
            if (root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>() != null)
            {
                return true;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (HasDescendantWithDescriptor(root.GetChild(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDescendantWithDescriptorKind(Transform root, string markerKind)
        {
            var descriptor = root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor != null
                && string.Equals(descriptor.MarkerKind, markerKind, System.StringComparison.Ordinal))
            {
                return true;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (HasDescendantWithDescriptorKind(root.GetChild(i), markerKind))
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject FindNextMarkerByKind(string markerKind, int startIndex)
        {
            var matches = new List<GameObject>();
            CollectDescendantsWithDescriptorKind(transform, markerKind, matches);
            if (matches.Count == 0)
            {
                return null;
            }

            var index = Mathf.Abs(startIndex) % matches.Count;
            return matches[index];
        }

        private static GameObject FindDescendantObjectWithPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var match = FindDescendantObjectWithPrefix(root.GetChild(i), prefix);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void CollectDescendantsWithDescriptorKind(
            Transform root,
            string markerKind,
            List<GameObject> matches)
        {
            var descriptor = root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor != null
                && string.Equals(descriptor.MarkerKind, markerKind, System.StringComparison.Ordinal))
            {
                matches.Add(root.gameObject);
            }

            for (var i = 0; i < root.childCount; i++)
            {
                CollectDescendantsWithDescriptorKind(root.GetChild(i), markerKind, matches);
            }
        }

        private static string EmptyAsNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
