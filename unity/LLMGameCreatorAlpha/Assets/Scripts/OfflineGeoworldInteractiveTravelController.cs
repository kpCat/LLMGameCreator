using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldInteractiveTravelController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal104";
        private const string ManifestFileName = "offline-geoworld-interactive-travel-manifest.json";
        private const string MovementPathFileName = "offline-geoworld-interactive-movement-path.json";
        private const string BoundaryZonesFileName = "offline-geoworld-interactive-boundary-zones.json";
        private const string PrefetchPlanFileName = "offline-geoworld-interactive-prefetch-plan.json";

        [SerializeField] private bool autoAdvance;
        [SerializeField] private float secondsBetweenSamples = 2.0f;
        [SerializeField] private int currentSampleIndex;
        [SerializeField] private int movementSampleCount;
        [SerializeField] private int boundaryCrossingCount;
        [SerializeField] private int objectCount;
        [SerializeField] private int visibleObjectCount;
        [SerializeField] private int missingObjectCount;
        [SerializeField] private int activeChunkCount;
        [SerializeField] private int prefetchChunkCount;
        [SerializeField] private string currentSyntheticChunkKey = string.Empty;
        [SerializeField] private string currentBoundaryCrossingId = string.Empty;
        [SerializeField] private string lastStatus = string.Empty;
        [SerializeField] private string payloadRoot = RelativeRoot;

        private readonly Dictionary<string, OfflineGeoworldInteractiveObjectState> statesById =
            new Dictionary<string, OfflineGeoworldInteractiveObjectState>(StringComparer.Ordinal);
        private readonly Dictionary<string, GameObject> objectsById =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly List<OfflineGeoworldInteractiveMovementSample> samples =
            new List<OfflineGeoworldInteractiveMovementSample>();
        private OfflineGeoworldPreviewPlayerMotor motor;
        private OfflineGeoworldBoundaryPrefetchState prefetchState;
        private float timer;

        public int CurrentSampleIndex { get { return currentSampleIndex; } }
        public int MovementSampleCount { get { return movementSampleCount; } }
        public int BoundaryCrossingCount { get { return boundaryCrossingCount; } }
        public int VisibleObjectCount { get { return visibleObjectCount; } }
        public int MissingObjectCount { get { return missingObjectCount; } }
        public string LastStatus { get { return lastStatus; } }

        private void Awake()
        {
            motor = GetComponent<OfflineGeoworldPreviewPlayerMotor>();
            if (motor == null)
            {
                motor = gameObject.AddComponent<OfflineGeoworldPreviewPlayerMotor>();
            }

            prefetchState = GetComponent<OfflineGeoworldBoundaryPrefetchState>();
            if (prefetchState == null)
            {
                prefetchState = gameObject.AddComponent<OfflineGeoworldBoundaryPrefetchState>();
            }
        }

        private void Start()
        {
            RefreshPayload();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyManualMovement(1f, 0f);
            }

            if (!autoAdvance || movementSampleCount <= 1)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= Mathf.Max(0.1f, secondsBetweenSamples))
            {
                timer = 0f;
                NextSample();
            }
        }

        [ContextMenu("Refresh Goal104 Interactive Travel")]
        public void RefreshPayload()
        {
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifestJson = ReadFile(root, ManifestFileName, diagnostics);
            var movementJson = ReadFile(root, MovementPathFileName, diagnostics);
            var boundaryJson = ReadFile(root, BoundaryZonesFileName, diagnostics);
            var prefetchJson = ReadFile(root, PrefetchPlanFileName, diagnostics);

            movementSampleCount = IntField(manifestJson, "movementSampleCount");
            boundaryCrossingCount = IntField(manifestJson, "boundaryCrossingCount");
            objectCount = IntField(manifestJson, "objectCount");
            samples.Clear();
            statesById.Clear();
            foreach (var block in Blocks(movementJson, "stepIndex"))
            {
                var sample = new OfflineGeoworldInteractiveMovementSample
                {
                    StepIndex = IntField(block, "stepIndex"),
                    StepId = StringField(block, "stepId"),
                    Action = StringField(block, "action"),
                    CenterChunkKey = StringField(block, "centerChunkKey"),
                    BoundaryBand = BoolField(block, "boundaryBand"),
                    ActiveChunkKeys = StringArrayField(block, "activeChunkKeys"),
                    BoundaryPrefetchChunkKeys = StringArrayField(block, "boundaryPrefetchChunkKeys"),
                    VisibleObjectIds = StringArrayField(block, "visibleObjectIds"),
                    HiddenObjectIds = StringArrayField(block, "hiddenObjectIds")
                };
                if (samples.Exists(item => item.StepIndex == sample.StepIndex) == false)
                {
                    samples.Add(sample);
                }
            }

            foreach (var block in Blocks(movementJson, "objectId"))
            {
                var item = new OfflineGeoworldInteractiveObjectState
                {
                    ObjectId = StringField(block, "objectId"),
                    ObjectName = StringField(block, "objectName"),
                    SourceCommandId = StringField(block, "sourceCommandId"),
                    CommandKind = StringField(block, "commandKind"),
                    SourceChunkKey = StringField(block, "sourceChunkKey"),
                    GridX = IntField(block, "gridX"),
                    GridZ = IntField(block, "gridZ"),
                    Elevation = IntField(block, "elevation")
                };
                if (!string.IsNullOrWhiteSpace(item.ObjectId))
                {
                    statesById[item.ObjectId] = item;
                }
            }

            EnsureObjects();
            currentSampleIndex = 0;
            ApplySample(currentSampleIndex);
            AddIfFalse(diagnostics, CountBlocks(boundaryJson, "crossingId") >= 2, "boundary-zones");
            AddIfFalse(diagnostics, CountBlocks(prefetchJson, "crossingId") >= 2, "prefetch-plan");
            lastStatus = "goal104_interactive_travel samples=" + movementSampleCount
                         + " crossings=" + boundaryCrossingCount
                         + " objects=" + objectCount
                         + " visible=" + visibleObjectCount
                         + " missing=" + missingObjectCount
                         + " diagnostics=" + diagnostics.Count;
        }

        public void RefreshPayloadStatus()
        {
            RefreshPayload();
        }

        public void ApplyManualMovement(float deltaX, float deltaZ)
        {
            if (motor != null)
            {
                motor.ApplyManualMovement(new Vector2(deltaX, deltaZ));
            }

            if (movementSampleCount <= 0)
            {
                RefreshPayload();
                return;
            }

            NextSample();
        }

        [ContextMenu("Next Goal104 Movement Sample")]
        public void NextSample()
        {
            if (movementSampleCount <= 0)
            {
                RefreshPayload();
                return;
            }

            ApplySample((currentSampleIndex + 1) % movementSampleCount);
        }

        public void ApplySample(int sampleIndex)
        {
            movementSampleCount = samples.Count;
            currentSampleIndex = movementSampleCount <= 0 ? 0 : Mathf.Clamp(sampleIndex, 0, movementSampleCount - 1);
            var sample = movementSampleCount == 0
                ? new OfflineGeoworldInteractiveMovementSample()
                : samples[currentSampleIndex];
            var visible = new HashSet<string>(sample.VisibleObjectIds, StringComparer.Ordinal);
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

            activeChunkCount = sample.ActiveChunkKeys.Count;
            prefetchChunkCount = sample.BoundaryPrefetchChunkKeys.Count;
            currentSyntheticChunkKey = sample.CenterChunkKey;
            currentBoundaryCrossingId = sample.Action == "boundary_crossing"
                ? "goal104_boundary_crossing_" + (sample.StepIndex == 2 ? "00" : "01")
                : string.Empty;
            if (motor != null)
            {
                motor.SnapToSample(currentSampleIndex, currentSyntheticChunkKey);
            }

            if (prefetchState != null)
            {
                prefetchState.Apply(
                    currentBoundaryCrossingId,
                    currentSyntheticChunkKey,
                    sample.BoundaryBand,
                    sample.ActiveChunkKeys,
                    sample.BoundaryPrefetchChunkKeys);
            }

            lastStatus = "goal104_interactive_travel sample=" + currentSampleIndex
                         + " visible=" + visibleObjectCount
                         + " missing=" + missingObjectCount
                         + " activeChunks=" + activeChunkCount
                         + " prefetch=" + prefetchChunkCount;
        }

        private void EnsureObjects()
        {
            foreach (var item in statesById.Values)
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
                gameObject.transform.localScale = Vector3.one * 0.6f;
                objectsById[item.ObjectId] = gameObject;
            }
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

        private static bool BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            return match.Success && string.Equals(match.Groups[1].Value, "true", StringComparison.Ordinal);
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

    [Serializable]
    public sealed class OfflineGeoworldInteractiveMovementSample
    {
        public int StepIndex;
        public string StepId = string.Empty;
        public string Action = string.Empty;
        public string CenterChunkKey = string.Empty;
        public bool BoundaryBand;
        public List<string> ActiveChunkKeys = new List<string>();
        public List<string> BoundaryPrefetchChunkKeys = new List<string>();
        public List<string> VisibleObjectIds = new List<string>();
        public List<string> HiddenObjectIds = new List<string>();
    }

    [Serializable]
    public sealed class OfflineGeoworldInteractiveObjectState
    {
        public string ObjectId = string.Empty;
        public string ObjectName = string.Empty;
        public string SourceCommandId = string.Empty;
        public string CommandKind = string.Empty;
        public string SourceChunkKey = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
    }
}
