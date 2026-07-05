using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public static class AcceptedAlphaPlayableProjectionPrimitiveFactory
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

        public static bool MaterialWarningGuardPresent { get { return true; } }

        public static GameObject CreateSection(Transform parent, string name, Vector3 localPosition)
        {
            var section = new GameObject(name);
            section.transform.SetParent(parent, false);
            section.transform.localPosition = localPosition;
            return section;
        }

        public static GameObject CreateMarker(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Color color,
            Vector3 localPosition,
            Vector3 localScale)
        {
            var marker = GameObject.CreatePrimitive(primitiveType);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
            marker.transform.localScale = localScale;
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor(ColorPropertyId, color);
                block.SetColor(BaseColorPropertyId, color);
                renderer.SetPropertyBlock(block);
            }

            return marker;
        }

        public static AcceptedAlphaPlayableProjectionMarkerDescriptor AttachDescriptor(
            GameObject target,
            string markerId,
            string markerName,
            string markerKind,
            string sourceGoal,
            string sourceFile,
            string displayLabel,
            string status,
            string details)
        {
            var descriptor = target.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor == null)
            {
                descriptor = target.AddComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            }

            descriptor.Configure(
                markerId,
                markerName,
                markerKind,
                sourceGoal,
                sourceFile,
                displayLabel,
                status,
                details);
            return descriptor;
        }

        public static GameObject CreateText(
            Transform parent,
            string name,
            string text,
            Vector3 localPosition,
            Color color,
            float characterSize)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            var mesh = obj.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.color = color;
            mesh.characterSize = characterSize;
            mesh.anchor = TextAnchor.MiddleLeft;
            return obj;
        }

        public static Color ColorForKind(string commandKind)
        {
            switch (commandKind)
            {
                case "water_body_plane":
                    return new Color(0.25f, 0.55f, 0.95f);
                case "road_segment_line":
                case "bridge_marker":
                    return new Color(0.75f, 0.75f, 0.75f);
                case "vegetation_area_marker":
                    return new Color(0.2f, 0.7f, 0.35f);
                case "building_footprint_marker":
                    return new Color(0.8f, 0.65f, 0.45f);
                case "terrain_hint_marker":
                    return new Color(0.65f, 0.55f, 0.35f);
                case "poi_marker":
                    return new Color(0.95f, 0.75f, 0.2f);
                case "barrier_line":
                    return new Color(0.85f, 0.25f, 0.25f);
                default:
                    return new Color(0.55f, 0.65f, 0.85f);
            }
        }

        public static PrimitiveType PrimitiveForKind(string commandKind)
        {
            switch (commandKind)
            {
                case "poi_marker":
                case "bridge_marker":
                case "terrain_hint_marker":
                case "administrative_hint_marker":
                    return PrimitiveType.Sphere;
                case "road_segment_line":
                case "barrier_line":
                    return PrimitiveType.Cube;
                case "water_body_plane":
                case "land_use_area_plane":
                    return PrimitiveType.Cube;
                default:
                    return PrimitiveType.Cube;
            }
        }

        public static Vector3 ScaleForKind(string commandKind)
        {
            switch (commandKind)
            {
                case "road_segment_line":
                case "barrier_line":
                    return new Vector3(1.6f, 0.08f, 0.18f);
                case "water_body_plane":
                case "land_use_area_plane":
                    return new Vector3(1.5f, 0.05f, 1.5f);
                case "building_footprint_marker":
                    return new Vector3(0.9f, 0.65f, 0.9f);
                default:
                    return Vector3.one * 0.65f;
            }
        }
    }
}
