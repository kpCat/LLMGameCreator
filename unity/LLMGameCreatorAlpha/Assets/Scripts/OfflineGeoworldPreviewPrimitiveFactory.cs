using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldPreviewCommand
    {
        public string CommandId = string.Empty;
        public string CommandKind = string.Empty;
        public string StyleKey = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
    }

    public static class OfflineGeoworldPreviewPrimitiveFactory
    {
        public static GameObject CreatePlaceholder(
            Transform parent,
            OfflineGeoworldPreviewCommand command,
            string styleLegendJson)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var primitiveHint = PrimitiveHint(styleLegendJson, command.CommandKind);
            var color = ParseColor(ColorHex(styleLegendJson, command.CommandKind));
            GameObject obj;
            if (primitiveHint == "line")
            {
                obj = new GameObject("preview_line_" + Compact(command.CommandKind));
                var line = obj.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.startWidth = 0.08f;
                line.endWidth = 0.08f;
                line.useWorldSpace = false;
                line.SetPosition(0, new Vector3(-0.8f, 0f, 0f));
                line.SetPosition(1, new Vector3(0.8f, 0f, 0f));
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = color;
                line.endColor = color;
            }
            else
            {
                obj = GameObject.CreatePrimitive(ToPrimitiveType(primitiveHint));
                obj.name = "preview_" + Compact(command.CommandKind);
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = color;
                }
            }

            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = new Vector3(command.GridX, command.Elevation, command.GridZ);
            obj.transform.localScale = ScaleFor(primitiveHint, command.CommandKind);
            return obj;
        }

        private static PrimitiveType ToPrimitiveType(string primitiveHint)
        {
            switch (primitiveHint)
            {
                case "sphere":
                    return PrimitiveType.Sphere;
                case "capsule":
                    return PrimitiveType.Capsule;
                case "plane":
                    return PrimitiveType.Cube;
                default:
                    return PrimitiveType.Cube;
            }
        }

        private static Vector3 ScaleFor(string primitiveHint, string commandKind)
        {
            if (primitiveHint == "line")
            {
                return new Vector3(1.4f, 1f, 1f);
            }

            if (primitiveHint == "plane")
            {
                return new Vector3(1.8f, 0.05f, 1.8f);
            }

            if (commandKind == "building_footprint_marker")
            {
                return new Vector3(1.1f, 0.8f, 1.1f);
            }

            return Vector3.one * 0.75f;
        }

        private static string PrimitiveHint(string json, string commandKind)
        {
            var block = StyleBlock(json, commandKind);
            var match = Regex.Match(block, "\"primitiveHint\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : "cube";
        }

        private static string ColorHex(string json, string commandKind)
        {
            var block = StyleBlock(json, commandKind);
            var match = Regex.Match(block, "\"colorHex\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : "#808080";
        }

        private static string StyleBlock(string json, string commandKind)
        {
            var pattern = "\\{[^\\{\\}]*\"commandKind\"\\s*:\\s*\""
                          + Regex.Escape(commandKind)
                          + "\"[\\s\\S]*?\\}";
            var match = Regex.Match(json, pattern);
            return match.Success ? match.Value : string.Empty;
        }

        private static Color ParseColor(string hex)
        {
            Color color;
            return ColorUtility.TryParseHtmlString(hex, out color) ? color : Color.gray;
        }

        private static string Compact(string value)
        {
            return Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_]+", "_");
        }
    }
}
