using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

namespace LLMGameCreator
{
    public sealed class ProjectStandalonePlayerAdapterBootstrap : MonoBehaviour
    {
        [Serializable] private sealed class Manifest { public string projectPackageId; public string projectTitle; public string projectVersion; public string packageSha256; public string finalStateHash; public bool runtimeAuthority; public bool unityGameplayTruth; public bool projectionOnly; public string[] selectedModuleIds; }
        [Serializable] private sealed class Frame { public int index; public string title; public string category; public string stateHash; }
        [Serializable] private sealed class Model { public string equipmentSummary; public string attributesSummary; public string progressionSummary; public string mapSummary; public string inventorySummary; public string questSummary; public string combatSummary; }

        private Manifest _manifest;
        private Frame[] _frames = new Frame[0];
        private Model _model;
        private int _frameIndex;
        private bool _autoPlay;
        private float _nextAutoStep;

        private void Start()
        {
            try
            {
                LoadPayload();
                if (HasArgument("-llmgcStandaloneSmokeExit"))
                {
                    var passed = Smoke();
                    WriteSmokeLog(passed);
                    Application.Quit(passed ? 0 : 2);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("LLMGC_PROJECT_STANDALONE_SMOKE_FAIL " + exception.Message);
                WriteSmokeLog(false);
                if (HasArgument("-llmgcStandaloneSmokeExit")) Application.Quit(2);
            }
        }

        private void Update()
        {
            if (!_autoPlay || _frames.Length == 0 || Time.unscaledTime < _nextAutoStep) return;
            _frameIndex = (_frameIndex + 1) % _frames.Length;
            _nextAutoStep = Time.unscaledTime + 1.0f;
        }

        private void OnGUI()
        {
            if (_manifest == null) return;
            GUILayout.BeginArea(new Rect(18, 18, Mathf.Min(Screen.width - 36, 980), Screen.height - 36));
            GUILayout.Label("Windows standalone Alpha — Runtime-backed PlayerAdapter", HeaderStyle());
            GUILayout.Label("Gameplay truth: Runtime   Unity mode: PlayerAdapter");
            GUILayout.Label(_manifest.projectTitle + "  |  " + _manifest.projectPackageId + " v" + _manifest.projectVersion);
            GUILayout.Label("Selected mechanics: " + (_manifest.selectedModuleIds == null ? 0 : _manifest.selectedModuleIds.Length) + "   Configured parameters: payload-backed");
            var frame = _frames[Mathf.Clamp(_frameIndex, 0, Mathf.Max(0, _frames.Length - 1))];
            GUILayout.Space(10);
            GUILayout.Label("Frame " + (_frameIndex + 1) + " / " + _frames.Length + ": " + frame.title + " (" + frame.category + ")");
            GUILayout.Label("Canonical state hash: " + _manifest.finalStateHash);
            GUILayout.Label("Map: " + _model.mapSummary);
            GUILayout.Label("Inventory: " + _model.inventorySummary);
            GUILayout.Label("Quest: " + _model.questSummary);
            GUILayout.Label("Combat: " + _model.combatSummary);
            GUILayout.Label("Equipment: " + _model.equipmentSummary);
            GUILayout.Label("Attributes: " + _model.attributesSummary);
            GUILayout.Label("Progression: " + _model.progressionSummary);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("First")) _frameIndex = 0;
            if (GUILayout.Button("Previous")) _frameIndex = Mathf.Max(0, _frameIndex - 1);
            if (GUILayout.Button("Next")) _frameIndex = Mathf.Min(_frames.Length - 1, _frameIndex + 1);
            if (GUILayout.Button("Last")) _frameIndex = _frames.Length - 1;
            if (GUILayout.Button("Auto Step")) _frameIndex = (_frameIndex + 1) % _frames.Length;
            if (GUILayout.Button(_autoPlay ? "Stop Auto Play" : "Auto Play")) { _autoPlay = !_autoPlay; _nextAutoStep = Time.unscaledTime + 1.0f; }
            if (GUILayout.Button("Reset")) { _frameIndex = 0; _autoPlay = false; }
            if (GUILayout.Button("Quit")) Application.Quit();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void LoadPayload()
        {
            var root = Path.Combine(Application.streamingAssetsPath, "LLMGameCreatorProject");
            var manifest = File.ReadAllText(Path.Combine(root, "project-manifest.json"));
            _manifest = new Manifest { projectPackageId = StringValue(manifest, "projectPackageId"), projectTitle = StringValue(manifest, "projectTitle"), projectVersion = StringValue(manifest, "projectVersion"), packageSha256 = StringValue(manifest, "packageSha256"), finalStateHash = StringValue(manifest, "finalStateHash"), runtimeAuthority = BoolValue(manifest, "runtimeAuthority"), unityGameplayTruth = BoolValue(manifest, "unityGameplayTruth"), projectionOnly = BoolValue(manifest, "projectionOnly"), selectedModuleIds = StringArray(manifest, "selectedModuleIds") };
            var frames = File.ReadAllText(Path.Combine(root, "player-adapter-frames.json"));
            var titles = Regex.Matches(frames, "\\\"title\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"");
            var categories = Regex.Matches(frames, "\\\"category\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"");
            _frames = new Frame[titles.Count];
            for (var index = 0; index < titles.Count; index++) _frames[index] = new Frame { index = index, title = titles[index].Groups[1].Value, category = index < categories.Count ? categories[index].Groups[1].Value : "runtime", stateHash = _manifest.finalStateHash };
            var model = File.ReadAllText(Path.Combine(root, "player-adapter-model.json"));
            _model = new Model { equipmentSummary = StringValue(model, "equipmentSummary"), attributesSummary = StringValue(model, "attributesSummary"), progressionSummary = StringValue(model, "progressionSummary"), mapSummary = StringValue(model, "mapSummary"), inventorySummary = StringValue(model, "inventorySummary"), questSummary = StringValue(model, "questSummary"), combatSummary = StringValue(model, "combatSummary") };
            if (_manifest == null || _frames == null || _model == null) throw new InvalidOperationException("payload schema is unsupported");
        }

        private static string StringValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*\\\"([^\\\"]*)\\\""); return match.Success ? match.Groups[1].Value : string.Empty; }
        private static bool BoolValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*(true|false)"); return match.Success && match.Groups[1].Value == "true"; }
        private static string[] StringArray(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*\\[([^]]*)\\]"); return match.Success ? Regex.Matches(match.Groups[1].Value, "\\\"([^\\\"]*)\\\"").Cast<Match>().Select(value => value.Groups[1].Value).ToArray() : new string[0]; }
        private bool Smoke()
        {
            var passed = _frames.Length > 0 && !string.IsNullOrEmpty(_manifest.packageSha256) && !string.IsNullOrEmpty(_manifest.finalStateHash) && _manifest.runtimeAuthority && !_manifest.unityGameplayTruth && !_manifest.projectionOnly && _manifest.selectedModuleIds != null;
            if (passed)
            {
                Debug.Log("LLMGC_PROJECT_STANDALONE_LOAD_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_FRAME_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_SMOKE_PASS");
            }
            return passed;
        }

        private void WriteSmokeLog(bool passed)
        {
            var path = ArgumentValue("-llmgcStandaloneSmokeLogPath");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllLines(path, passed ? new[] { "LLMGC_PROJECT_STANDALONE_LOAD_PASS", "LLMGC_PROJECT_STANDALONE_FRAME_PASS", "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS", "LLMGC_PROJECT_STANDALONE_SMOKE_PASS" } : new[] { "LLMGC_PROJECT_STANDALONE_SMOKE_FAIL" });
        }

        private static bool HasArgument(string key) { foreach (var argument in Environment.GetCommandLineArgs()) if (string.Equals(argument, key, StringComparison.Ordinal)) return true; return false; }
        private static string ArgumentValue(string key) { var args = Environment.GetCommandLineArgs(); for (var i = 0; i + 1 < args.Length; i++) if (string.Equals(args[i], key, StringComparison.Ordinal)) return args[i + 1]; return string.Empty; }
        private static GUIStyle HeaderStyle() { var style = new GUIStyle(GUI.skin.label); style.fontSize = 20; style.fontStyle = FontStyle.Bold; return style; }
    }
}
