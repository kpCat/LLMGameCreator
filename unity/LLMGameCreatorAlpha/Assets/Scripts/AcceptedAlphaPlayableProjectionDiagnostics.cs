using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public static class AcceptedAlphaPlayableProjectionDiagnostics
    {
        public static string ResolveRepositoryRoot(List<string> diagnostics)
        {
            var current = new DirectoryInfo(Application.dataPath);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            diagnostics.Add("repository_root_not_found");
            return string.Empty;
        }

        public static string ReadRequiredFile(string path, string diagnosticCode, List<string> diagnostics)
        {
            if (!File.Exists(path))
            {
                diagnostics.Add(diagnosticCode + ":missing:" + path);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        public static string Combine(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            var path = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                path = Path.Combine(path, parts[i]);
            }

            return path;
        }

        public static List<string> Blocks(string json, string field)
        {
            var blocks = new List<string>();
            var pattern = "\\{[^\\{\\}]*\"" + Regex.Escape(field) + "\"[\\s\\S]*?\\}";
            foreach (Match match in Regex.Matches(json ?? string.Empty, pattern))
            {
                blocks.Add(match.Value);
            }

            return blocks;
        }

        public static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        public static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            bool value;
            return match.Success && bool.TryParse(match.Groups[1].Value, out value) && value;
        }

        public static float FloatField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");
            float value;
            return match.Success && float.TryParse(match.Groups[1].Value, out value) ? value : 0f;
        }

        public static string Compact(string value)
        {
            var compact = Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_]+", "_");
            return string.IsNullOrWhiteSpace(compact) ? "unknown" : compact;
        }
    }
}
