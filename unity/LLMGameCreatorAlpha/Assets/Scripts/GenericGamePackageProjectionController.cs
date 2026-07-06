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
        private int fatalErrorCount;

        public string StatusLine { get { return statusLine; } }
        public string LastDiagnostics { get { return lastDiagnostics; } }
        public string LastSmokeDiagnostics { get { return lastSmokeDiagnostics; } }
        public string SelectedMarkerDetails { get { return selectedMarkerDetails; } }
        public string VerificationEventLog { get { return verificationEventLog; } }
        public string SelectedMarkerId { get { return selectedMarkerId; } }
        public string SelectedMarkerKind { get { return selectedMarkerKind; } }
        public bool LastVerificationPassed { get { return lastVerificationPassed; } }
        public string PackageId { get { return model.PackageId; } }
        public string PackageTitle { get { return model.PackageTitle; } }
        public string MapId { get { return model.MapId; } }
        public int MapWidth { get { return model.MapWidth; } }
        public int MapHeight { get { return model.MapHeight; } }
        public int EntityCount { get { return model.Entities.Count; } }
        public int ItemCount { get { return model.Items.Count; } }

        public void BuildOrRefreshGenericPackageProjection()
        {
            fatalErrorCount = 0;
            selectedMarkerDetails = string.Empty;
            verificationEventLog = string.Empty;
            selectedMarkerId = string.Empty;
            selectedMarkerKind = string.Empty;
            lastVerificationPassed = false;

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

        public GameObject FindGenericProjectionRoot()
        {
            return FindDescendantObjectWithPrefix(transform, GenericSectionName);
        }

        public GameObject FindFirstGenericEntityMarker()
        {
            return FindNextMarkerByKind("entity", 0);
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
