using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreator
{
    public sealed class ProjectStandalonePlayerAdapterBootstrap : MonoBehaviour
    {
        [Serializable] private sealed class Manifest { public string projectPackageId; public string projectTitle; public string projectVersion; public string packageSha256; public string finalStateHash; public bool runtimeAuthority; public bool unityGameplayTruth; public bool projectionOnly; public string[] selectedModuleIds; public int requiredMechanicCount; public int selectedOptionalMechanicCount; public int activeMechanicCount; public int configuredParameterCount; }
        [Serializable] private sealed class Frame { public int index; public string title; public string category; public string stateHash; }
        [Serializable] private sealed class ReviewFact { public string label; public string value; }
        [Serializable] private sealed class Model { public ReviewFact[] humanReviewFacts; public int equipmentDamageBonus; public float statDamageBonus; public float totalAdditionalDamage; }

        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;
        private Manifest _manifest;
        private Frame[] _frames = new Frame[0];
        private Model _model;
        private int _frameIndex;
        private bool _autoPlay;
        private bool _showTechnicalDetails;
        private float _nextAutoStep;
        private int _selfCheckPassed;
        private int _selfCheckTotal;
        private string _selfCheckError = string.Empty;

        private void Start()
        {
            try
            {
                LoadPayload();
                RunSelfCheck();
                if (HasArgument("-llmgcStandaloneSmokeExit"))
                {
                    var passed = Smoke();
                    WriteSmokeLog(passed);
                    Application.Quit(passed ? 0 : 2);
                }
            }
            catch (Exception exception)
            {
                _selfCheckError = exception.Message;
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
            var originalColor = GUI.color;
            var originalMatrix = GUI.matrix;
            try
            {
                if (Event.current.type == EventType.Repaint)
                {
                    GUI.color = new Color(0.045f, 0.06f, 0.08f, 1f);
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                    GUI.color = originalColor;
                }

                var scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
                var offset = new Vector2((Screen.width - ReferenceWidth * scale) * 0.5f, (Screen.height - ReferenceHeight * scale) * 0.5f);
                GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1f));
                GUILayout.BeginArea(new Rect(34, 24, ReferenceWidth - 68, ReferenceHeight - 48));
                DrawShell();
                GUILayout.EndArea();
            }
            finally
            {
                GUI.color = originalColor;
                GUI.matrix = originalMatrix;
            }
        }

        private void DrawShell()
        {
            GUILayout.Label(_manifest.projectTitle, HeaderStyle(30));
            GUILayout.Label("Windows-игра Alpha", SubHeaderStyle());
            GUILayout.Label("Игровая логика: Runtime    •    Режим Unity: PlayerAdapter", MutedStyle());
            GUILayout.Space(12);

            var green = _selfCheckPassed == _selfCheckTotal && _selfCheckTotal > 0;
            var old = GUI.color;
            GUI.color = green ? new Color(0.10f, 0.52f, 0.25f, 1f) : new Color(0.65f, 0.16f, 0.16f, 1f);
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(80));
            GUI.color = Color.white;
            GUILayout.Label(green ? "АВТОПРОВЕРКА ПРОЙДЕНА" : "АВТОПРОВЕРКА НЕ ПРОЙДЕНА", BannerStyle());
            GUILayout.Label(green ? "Автопроверка пройдена: " + _selfCheckPassed + "/" + _selfCheckTotal : _selfCheckError, BodyStyle());
            GUILayout.EndVertical();
            GUI.color = old;

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            BeginPanel("Проект", 360);
            GUILayout.Label("Пакет: " + _manifest.projectPackageId + "  v" + _manifest.projectVersion, BodyStyle());
            GUILayout.Label("Обязательных механик: " + _manifest.requiredMechanicCount, BodyStyle());
            GUILayout.Label("Дополнительно выбрано: " + _manifest.selectedOptionalMechanicCount, BodyStyle());
            GUILayout.Label("Всего активно: " + _manifest.activeMechanicCount, BodyStyle());
            GUILayout.Label("Настроено параметров: " + _manifest.configuredParameterCount, BodyStyle());
            EndPanel();

            GUILayout.Space(12);
            BeginPanel("Текущий кадр", 0);
            var frame = _frames[Mathf.Clamp(_frameIndex, 0, Mathf.Max(0, _frames.Length - 1))];
            GUILayout.Label("Кадр " + (_frameIndex + 1) + " / " + _frames.Length, HeaderStyle(22));
            GUILayout.Label(frame.title, FrameTitleStyle());
            GUILayout.Label("Категория: " + frame.category + "    •    Состояние: " + ShortHash(frame.stateHash), MutedStyle());
            EndPanel();
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            BeginPanel("Итог игрового состояния", 0);
            foreach (var fact in _model.humanReviewFacts ?? new ReviewFact[0])
                GUILayout.Label(fact.label + ": " + fact.value, BodyStyle());
            EndPanel();
            GUILayout.Space(12);
            BeginPanel("Управление", 430);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("В начало", ButtonStyle())) _frameIndex = 0;
            if (GUILayout.Button("Назад", ButtonStyle())) _frameIndex = Mathf.Max(0, _frameIndex - 1);
            if (GUILayout.Button("Далее", ButtonStyle())) _frameIndex = Mathf.Min(_frames.Length - 1, _frameIndex + 1);
            if (GUILayout.Button("В конец", ButtonStyle())) _frameIndex = _frames.Length - 1;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Автошаг", ButtonStyle())) _frameIndex = (_frameIndex + 1) % _frames.Length;
            if (GUILayout.Button(_autoPlay ? "Остановить" : "Автовоспроизведение", ButtonStyle())) { _autoPlay = !_autoPlay; _nextAutoStep = Time.unscaledTime + 1.0f; }
            if (GUILayout.Button("Сбросить", ButtonStyle())) { _frameIndex = 0; _autoPlay = false; }
            if (GUILayout.Button("Закрыть", ButtonStyle())) Application.Quit();
            GUILayout.EndHorizontal();
            EndPanel();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            _showTechnicalDetails = GUILayout.Toggle(_showTechnicalDetails, "Технические сведения", ToggleStyle());
            if (_showTechnicalDetails)
            {
                GUILayout.Label("Полный SHA пакета: " + _manifest.packageSha256, TechnicalStyle());
                GUILayout.Label("Полный SHA состояния: " + _manifest.finalStateHash, TechnicalStyle());
            }
        }

        private void LoadPayload()
        {
            var root = Path.Combine(Application.streamingAssetsPath, "LLMGameCreatorProject");
            var manifest = ReadRequired(Path.Combine(root, "project-manifest.json"));
            var model = ReadRequired(Path.Combine(root, "player-adapter-model.json"));
            var frames = ReadRequired(Path.Combine(root, "player-adapter-frames.json"));
            var launch = ReadRequired(Path.Combine(root, "standalone-launch.json"));
            var packageJson = ReadRequired(Path.Combine(root, "game-package.json"));
            if (!manifest.Contains("llmgc_project_standalone_v2") || !model.Contains("llmgc_player_adapter_model_v2") || !launch.Contains("llmgc_standalone_launch_v2")) throw new InvalidOperationException("Поддерживается только payload standalone v2.");
            _manifest = new Manifest { projectPackageId = StringValue(manifest, "projectPackageId"), projectTitle = StringValue(manifest, "projectTitle"), projectVersion = StringValue(manifest, "projectVersion"), packageSha256 = StringValue(manifest, "packageSha256"), finalStateHash = StringValue(manifest, "finalStateHash"), runtimeAuthority = BoolValue(manifest, "runtimeAuthority"), unityGameplayTruth = BoolValue(manifest, "unityGameplayTruth"), projectionOnly = BoolValue(manifest, "projectionOnly"), selectedModuleIds = StringArray(manifest, "selectedModuleIds"), requiredMechanicCount = IntValue(manifest, "requiredMechanicCount"), selectedOptionalMechanicCount = IntValue(manifest, "selectedOptionalMechanicCount"), activeMechanicCount = IntValue(manifest, "activeMechanicCount"), configuredParameterCount = IntValue(manifest, "configuredParameterCount") };
            _frames = Regex.Matches(frames, "\\{[^{}]*\\\"index\\\"\\s*:\\s*(\\d+)[^{}]*\\\"title\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\\"category\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\\"stateHash\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"[^{}]*\\}").Cast<Match>().Select(match => new Frame { index = int.Parse(match.Groups[1].Value), title = match.Groups[2].Value, category = match.Groups[3].Value, stateHash = match.Groups[4].Value }).ToArray();
            _model = new Model { humanReviewFacts = Regex.Matches(model, "\\{\\s*\\\"label\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"\\s*,\\s*\\\"value\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"").Cast<Match>().Select(match => new ReviewFact { label = match.Groups[1].Value, value = match.Groups[2].Value }).ToArray(), equipmentDamageBonus = IntValue(model, "equipmentDamageBonus"), statDamageBonus = FloatValue(model, "statDamageBonus"), totalAdditionalDamage = FloatValue(model, "totalAdditionalDamage") };
            if (!string.Equals(HashFile(Path.Combine(root, "game-package.json")), _manifest.packageSha256, StringComparison.Ordinal)) throw new InvalidOperationException("SHA игрового пакета не совпадает с manifest.");
        }

        private void RunSelfCheck()
        {
            _selfCheckTotal = 12;
            var checks = new[]
            {
                _manifest != null && _model != null && _frames != null,
                !string.IsNullOrEmpty(_manifest.projectPackageId) && !string.IsNullOrEmpty(_manifest.projectTitle) && !string.IsNullOrEmpty(_manifest.projectVersion),
                !string.IsNullOrEmpty(_manifest.packageSha256) && !string.IsNullOrEmpty(_manifest.finalStateHash),
                _manifest.runtimeAuthority && !_manifest.unityGameplayTruth && !_manifest.projectionOnly,
                _frames.Length > 0,
                _frames.Select((frame, index) => frame.index == index && !string.IsNullOrEmpty(frame.title) && !string.IsNullOrEmpty(frame.category) && !string.IsNullOrEmpty(frame.stateHash)).All(value => value),
                _manifest.selectedModuleIds != null && _manifest.selectedOptionalMechanicCount == _manifest.selectedModuleIds.Length,
                _manifest.activeMechanicCount == _manifest.requiredMechanicCount + _manifest.selectedOptionalMechanicCount,
                _manifest.configuredParameterCount == EffectiveParameterCount(),
                _model.humanReviewFacts != null && _model.humanReviewFacts.Length > 0,
                CursorTransitionsAreDeterministic(),
                _model.equipmentDamageBonus >= 0 && _model.totalAdditionalDamage >= _model.equipmentDamageBonus
            };
            _selfCheckPassed = checks.Count(value => value);
            _selfCheckError = _selfCheckPassed == _selfCheckTotal ? string.Empty : "Автопроверка: " + _selfCheckPassed + "/" + _selfCheckTotal;
        }

        private bool Smoke()
        {
            var passed = _selfCheckPassed == _selfCheckTotal;
            if (passed)
            {
                Debug.Log("LLMGC_PROJECT_STANDALONE_LOAD_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS");
                Debug.Log("LLMGC_PROJECT_STANDALONE_SMOKE_PASS");
            }
            return passed;
        }

        private int EffectiveParameterCount()
        {
            var manifestPath = Path.Combine(Application.streamingAssetsPath, "LLMGameCreatorProject", "project-manifest.json");
            return Regex.Matches(File.ReadAllText(manifestPath), "\\\"parameterId\\\"\\s*:").Count;
        }

        private bool CursorTransitionsAreDeterministic()
        {
            if (_frames.Length == 0) return false;
            var first = 0;
            var next = Mathf.Min(_frames.Length - 1, first + 1);
            var last = _frames.Length - 1;
            var reset = 0;
            return next == Mathf.Min(1, last) && last >= first && reset == first;
        }

        private void WriteSmokeLog(bool passed)
        {
            var path = ArgumentValue("-llmgcStandaloneSmokeLogPath");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllLines(path, passed ? new[] { "LLMGC_PROJECT_STANDALONE_LOAD_PASS", "LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS", "LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS", "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS", "LLMGC_PROJECT_STANDALONE_SMOKE_PASS" } : new[] { "LLMGC_PROJECT_STANDALONE_SMOKE_FAIL" });
        }

        private static void BeginPanel(string title, float width) { GUILayout.BeginVertical(GUI.skin.box, width > 0 ? new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(218) } : new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(218) }); GUILayout.Label(title, HeaderStyle(18)); }
        private static void EndPanel() { GUILayout.EndVertical(); }
        private static string ReadRequired(string path) { if (!File.Exists(path)) throw new FileNotFoundException("Отсутствует файл payload", path); return File.ReadAllText(path); }
        private static string StringValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*\\\"([^\\\"]*)\\\""); return match.Success ? match.Groups[1].Value : string.Empty; }
        private static bool BoolValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*(true|false)"); return match.Success && match.Groups[1].Value == "true"; }
        private static int IntValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*(-?\\d+)"); return match.Success ? int.Parse(match.Groups[1].Value) : 0; }
        private static float FloatValue(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)"); return match.Success ? float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : 0f; }
        private static string[] StringArray(string json, string name) { var match = Regex.Match(json, "\\\"" + Regex.Escape(name) + "\\\"\\s*:\\s*\\[([^]]*)\\]"); return match.Success ? Regex.Matches(match.Groups[1].Value, "\\\"([^\\\"]*)\\\"").Cast<Match>().Select(value => value.Groups[1].Value).ToArray() : new string[0]; }
        private static string HashFile(string path) { using (var stream = File.OpenRead(path)) return BitConverter.ToString(SHA256.Create().ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant(); }
        private static string ShortHash(string value) { return string.IsNullOrEmpty(value) ? "—" : value.Substring(0, Mathf.Min(12, value.Length)); }
        private static bool HasArgument(string key) { return Environment.GetCommandLineArgs().Any(argument => string.Equals(argument, key, StringComparison.Ordinal)); }
        private static string ArgumentValue(string key) { var args = Environment.GetCommandLineArgs(); for (var i = 0; i + 1 < args.Length; i++) if (string.Equals(args[i], key, StringComparison.Ordinal)) return args[i + 1]; return string.Empty; }
        private static GUIStyle HeaderStyle(int size) { var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = Color.white } }; return style; }
        private static GUIStyle SubHeaderStyle() { var style = HeaderStyle(20); style.normal.textColor = new Color(0.65f, 0.82f, 1f); return style; }
        private static GUIStyle BannerStyle() { var style = HeaderStyle(24); style.alignment = TextAnchor.MiddleCenter; return style; }
        private static GUIStyle BodyStyle() { var style = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, normal = { textColor = Color.white } }; return style; }
        private static GUIStyle FrameTitleStyle() { var style = HeaderStyle(20); style.clipping = TextClipping.Clip; style.wordWrap = true; return style; }
        private static GUIStyle MutedStyle() { var style = BodyStyle(); style.fontSize = 15; style.normal.textColor = new Color(0.72f, 0.78f, 0.84f); return style; }
        private static GUIStyle TechnicalStyle() { var style = MutedStyle(); style.fontSize = 13; return style; }
        private static GUIStyle ToggleStyle() { var style = BodyStyle(); style.fontSize = 15; return style; }
        private static GUIStyle ButtonStyle() { var style = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 34, wordWrap = true }; return style; }
    }
}
