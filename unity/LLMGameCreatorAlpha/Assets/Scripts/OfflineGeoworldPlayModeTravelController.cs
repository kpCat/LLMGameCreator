using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPlayModeTravelController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal103";
        private const string ManifestFileName = "offline-geoworld-playmode-travel-manifest.json";
        private const string StepsFileName = "offline-geoworld-playmode-steps.json";
        private const string ObjectStateIndexFileName =
            "offline-geoworld-playmode-object-state-index.json";
        private const string ChunkVisibilityFileName =
            "offline-geoworld-playmode-chunk-visibility.json";

        [SerializeField] private bool autoAdvance;
        [SerializeField] private float secondsBetweenSteps = 2.5f;
        [SerializeField] private int currentStepIndex;
        [SerializeField] private int stepCount;
        [SerializeField] private int objectCount;
        [SerializeField] private int visibleObjectCount;
        [SerializeField] private int missingObjectCount;
        [SerializeField] private int activeChunkCount;
        [SerializeField] private int boundaryPrefetchChunkCount;
        [SerializeField] private string lastStatus = string.Empty;
        [SerializeField] private string payloadRoot = RelativeRoot;

        private readonly Dictionary<string, OfflineGeoworldPlayModeObjectState> statesById =
            new Dictionary<string, OfflineGeoworldPlayModeObjectState>(StringComparer.Ordinal);
        private readonly Dictionary<string, GameObject> objectsById =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private OfflineGeoworldPlayModeTravelState state =
            new OfflineGeoworldPlayModeTravelState();
        private float timer;

        public int CurrentStepIndex { get { return currentStepIndex; } }
        public int StepCount { get { return stepCount; } }
        public int VisibleObjectCount { get { return visibleObjectCount; } }
        public int MissingObjectCount { get { return missingObjectCount; } }
        public string LastStatus { get { return lastStatus; } }

        private void Start()
        {
            RefreshPayload();
        }

        private void Update()
        {
            if (!autoAdvance || stepCount <= 1)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= Mathf.Max(0.1f, secondsBetweenSteps))
            {
                timer = 0f;
                NextStep();
            }
        }

        [ContextMenu("Refresh Goal103 Play Mode Travel")]
        public void RefreshPayload()
        {
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifestJson = ReadFile(root, ManifestFileName, diagnostics);
            var stepsJson = ReadFile(root, StepsFileName, diagnostics);
            var objectsJson = ReadFile(root, ObjectStateIndexFileName, diagnostics);
            var chunkJson = ReadFile(root, ChunkVisibilityFileName, diagnostics);

            state = ReadState(manifestJson, stepsJson, objectsJson);
            statesById.Clear();
            foreach (var item in state.Objects)
            {
                statesById[item.ObjectId] = item;
            }

            EnsureObjects();
            currentStepIndex = 0;
            ApplyStep(currentStepIndex);
            var chunkCount = CountBlocks(chunkJson, "stepIndex");
            AddIfFalse(diagnostics, chunkCount == state.StepCount, "chunk-visibility");
            lastStatus = "goal103_playmode_travel steps=" + stepCount
                         + " objects=" + objectCount
                         + " visible=" + visibleObjectCount
                         + " missing=" + missingObjectCount
                         + " diagnostics=" + diagnostics.Count;
        }

        [ContextMenu("Next Goal103 Travel Step")]
        public void NextStep()
        {
            if (stepCount <= 0)
            {
                RefreshPayload();
                return;
            }

            ApplyStep((currentStepIndex + 1) % stepCount);
        }

        public void ApplyStep(int stepIndex)
        {
            stepCount = state.StepCount;
            objectCount = state.ObjectCount;
            currentStepIndex = stepCount <= 0 ? 0 : Mathf.Clamp(stepIndex, 0, stepCount - 1);
            var step = state.StepAt(currentStepIndex);
            var visible = new HashSet<string>(step.VisibleObjectIds, StringComparer.Ordinal);
            visibleObjectCount = 0;
            missingObjectCount = 0;
            foreach (var pair in objectsById)
            {
                var shouldShow = visible.Contains(pair.Key);
                pair.Value.SetActive(shouldShow);
                if (shouldShow)
                {
                    visibleObjectCount++;
                }
            }

            foreach (var visibleId in visible)
            {
                if (!objectsById.ContainsKey(visibleId))
                {
                    missingObjectCount++;
                }
            }

            activeChunkCount = step.ActiveChunkKeys.Count;
            boundaryPrefetchChunkCount = step.BoundaryPrefetchChunkKeys.Count;
            lastStatus = "goal103_playmode_travel step=" + currentStepIndex
                         + " visible=" + visibleObjectCount
                         + " missing=" + missingObjectCount
                         + " activeChunks=" + activeChunkCount
                         + " boundaryPrefetch=" + boundaryPrefetchChunkCount;
        }

        private void EnsureObjects()
        {
            foreach (var item in state.Objects)
            {
                if (objectsById.ContainsKey(item.ObjectId))
                {
                    continue;
                }

                var existing = transform.Find(item.ObjectName);
                var gameObject = existing == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                    : existing.gameObject;
                gameObject.name = item.ObjectName;
                gameObject.transform.SetParent(transform, false);
                gameObject.transform.localPosition = new Vector3(item.GridX, item.Elevation * 0.05f, item.GridZ);
                gameObject.transform.localScale = Vector3.one * 0.75f;
                objectsById[item.ObjectId] = gameObject;
            }
        }

        private static OfflineGeoworldPlayModeTravelState ReadState(
            string manifestJson,
            string stepsJson,
            string objectJson)
        {
            var result = new OfflineGeoworldPlayModeTravelState
            {
                StepCount = IntField(manifestJson, "stepCount"),
                ObjectCount = IntField(manifestJson, "objectCount")
            };

            foreach (var block in Blocks(stepsJson, "stepIndex"))
            {
                result.Steps.Add(new OfflineGeoworldPlayModeTravelStepState
                {
                    StepIndex = IntField(block, "stepIndex"),
                    StepId = StringField(block, "stepId"),
                    Action = StringField(block, "action"),
                    CenterChunkKey = StringField(block, "centerChunkKey"),
                    ActiveChunkKeys = StringArrayField(block, "activeChunkKeys"),
                    BoundaryPrefetchChunkKeys = StringArrayField(block, "boundaryPrefetchChunkKeys"),
                    VisibleObjectIds = StringArrayField(block, "visibleObjectIds"),
                    HiddenObjectIds = StringArrayField(block, "hiddenObjectIds"),
                    NewlyVisibleObjectIds = StringArrayField(block, "newlyVisibleObjectIds"),
                    NewlyHiddenObjectIds = StringArrayField(block, "newlyHiddenObjectIds"),
                    ExpectedVisibleObjectCount = IntField(block, "expectedVisibleObjectCount"),
                    DeterministicStateHash = StringField(block, "deterministicStateHash")
                });
            }

            foreach (var block in Blocks(objectJson, "objectId"))
            {
                result.Objects.Add(new OfflineGeoworldPlayModeObjectState
                {
                    ObjectId = StringField(block, "objectId"),
                    ObjectName = StringField(block, "objectName"),
                    SourceCommandId = StringField(block, "sourceCommandId"),
                    CommandKind = StringField(block, "commandKind"),
                    SourceChunkKey = StringField(block, "sourceChunkKey"),
                    GridX = IntField(block, "gridX"),
                    GridZ = IntField(block, "gridZ"),
                    Elevation = IntField(block, "elevation")
                });
            }

            return result;
        }

        private static string ReadFile(string root, string fileName, List<string> diagnostics)
        {
            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing:" + fileName);
                return string.Empty;
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static List<string> Blocks(string json, string anchorField)
        {
            var result = new List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\""
                                                                   + Regex.Escape(anchorField)
                                                                   + "\"[\\s\\S]*?\\}"))
            {
                result.Add(match.Value);
            }

            return result;
        }

        private static int CountBlocks(string json, string anchorField)
        {
            return Blocks(json, anchorField).Count;
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }

        private static List<string> StringArrayField(string json, string field)
        {
            var result = new List<string>();
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\\[(.*?)\\]");
            if (!match.Success)
            {
                return result;
            }

            foreach (Match item in Regex.Matches(match.Groups[1].Value, "\"([^\"]*)\""))
            {
                result.Add(item.Groups[1].Value);
            }

            return result;
        }

        private static void AddIfFalse(List<string> diagnostics, bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add("mismatch:" + code);
            }
        }
    }
}
