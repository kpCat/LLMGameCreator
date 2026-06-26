using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class AlphaRuntimeBootstrap : MonoBehaviour
    {
        private const string PayloadFolderName = "LLMGameCreatorAlpha";

        private void Start()
        {
            var arguments = Environment.GetCommandLineArgs();
            var logPath = GetArgumentValue(arguments, "-alphaLogPath");
            if (string.IsNullOrWhiteSpace(logPath))
            {
                logPath = Path.Combine(Application.persistentDataPath, "alpha-launch-diagnostic.log");
            }

            var lines = new List<string>
            {
                "alpha_runtime.launch_started=true",
                "alpha_runtime.streaming_assets_path=" + Application.streamingAssetsPath
            };

            try
            {
                var payloadRoot = Path.Combine(Application.streamingAssetsPath, PayloadFolderName);
                var configPath = Path.Combine(payloadRoot, "runtime", "unity-runtime-config.json");
                var packagePath = Path.Combine(payloadRoot, "game-data", "game-package.json");
                var assetManifestPath = Path.Combine(payloadRoot, "assets", "asset-manifest.json");
                var configJson = File.ReadAllText(configPath);
                var packageJson = File.ReadAllText(packagePath);
                var assetManifestJson = File.ReadAllText(assetManifestPath);

                lines.Add("alpha_runtime.payload_root_exists=" + Directory.Exists(payloadRoot).ToString().ToLowerInvariant());
                lines.Add("alpha_runtime.config_loaded=true");
                lines.Add("alpha_runtime.package_loaded=true");
                lines.Add("alpha_runtime.asset_manifest_loaded=true");
                lines.Add("alpha_runtime.package_id=" + ExtractJsonString(configJson, "packageId"));
                lines.Add("alpha_runtime.package_hash=" + ExtractJsonString(configJson, "packageHash"));
                lines.Add("alpha_runtime.asset_manifest_hash=" + ExtractJsonString(configJson, "assetManifestHash"));
                lines.Add("alpha_runtime.start_map_id=" + ExtractJsonString(configJson, "startMapId"));
                lines.Add("alpha_runtime.selected_thread_id=" + ExtractJsonString(configJson, "selectedThreadId"));
                lines.Add("alpha_runtime.command_hint_count=" + CountJsonObjectsInArray(configJson, "commandHints"));
                lines.Add("alpha_runtime.asset_ref_count=" + CountJsonObjectsInArray(configJson, "assetRefs"));
                lines.Add("alpha_runtime.package_bytes=" + packageJson.Length.ToString());
                lines.Add("alpha_runtime.asset_manifest_bytes=" + assetManifestJson.Length.ToString());
                lines.Add("alpha_runtime.launch_completed=true");
            }
            catch (Exception ex)
            {
                lines.Add("alpha_runtime.launch_completed=false");
                lines.Add("alpha_runtime.error_type=" + ex.GetType().Name);
                lines.Add("alpha_runtime.error_message=" + ex.Message.Replace(Environment.NewLine, " "));
            }
            finally
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? Application.persistentDataPath);
                File.WriteAllLines(logPath, lines);
                Debug.Log(string.Join(Environment.NewLine, lines));

                if (HasArgument(arguments, "-alphaSmokeExit"))
                {
                    Application.Quit(lines.Contains("alpha_runtime.launch_completed=true") ? 0 : 1);
                }
            }
        }

        private static string ExtractJsonString(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"");
            return match.Success ? Regex.Unescape(match.Groups["value"].Value) : string.Empty;
        }

        private static int CountJsonObjectsInArray(string json, string propertyName)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\\[(?<value>.*?)\\]", RegexOptions.Singleline);
            if (!match.Success)
            {
                return 0;
            }

            var value = match.Groups["value"].Value;
            var count = 0;
            foreach (var ch in value)
            {
                if (ch == '{')
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasArgument(IReadOnlyList<string> arguments, string name)
        {
            for (var index = 0; index < arguments.Count; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetArgumentValue(IReadOnlyList<string> arguments, string name)
        {
            for (var index = 0; index < arguments.Count - 1; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return string.Empty;
        }
    }
}
