using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldInteractionController : MonoBehaviour
    {
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal105";
        private const string ManifestFileName = "offline-geoworld-interaction-manifest.json";
        private const string TargetsFileName = "offline-geoworld-interaction-targets.json";
        private const string ActionsFileName = "offline-geoworld-interaction-actions.json";
        private const string SessionScriptFileName = "offline-geoworld-interaction-session-script.json";
        private const string StateDeltaPlanFileName = "offline-geoworld-interaction-state-delta-plan.json";

        [SerializeField] private Transform playerProxy;
        [SerializeField] private bool autoRunScriptedSession;
        [SerializeField] private int targetCount;
        [SerializeField] private int actionKindCount;
        [SerializeField] private int scriptedEventCount;
        [SerializeField] private int stateDeltaCount;
        [SerializeField] private int boundTargetCount;
        [SerializeField] private int missingBindingCount;
        [SerializeField] private string nearestTargetId = string.Empty;
        [SerializeField] private string nearestTargetName = string.Empty;
        [SerializeField] private float nearestTargetDistance;
        [SerializeField] private int availableActionCount;
        [SerializeField] private string lastActionStatus = string.Empty;
        [SerializeField] private string lastStatus = string.Empty;
        [SerializeField] private string payloadRoot = RelativeRoot;

        private readonly Dictionary<string, OfflineGeoworldInteractionTargetMetadata> targetMetadataById =
            new Dictionary<string, OfflineGeoworldInteractionTargetMetadata>(StringComparer.Ordinal);
        private readonly Dictionary<string, OfflineGeoworldInteractionActionMetadata> actionById =
            new Dictionary<string, OfflineGeoworldInteractionActionMetadata>(StringComparer.Ordinal);
        private readonly Dictionary<string, OfflineGeoworldInteractionTarget> boundTargetsById =
            new Dictionary<string, OfflineGeoworldInteractionTarget>(StringComparer.Ordinal);
        private readonly List<OfflineGeoworldInteractionScriptedEventMetadata> scriptedEvents =
            new List<OfflineGeoworldInteractionScriptedEventMetadata>();
        private readonly Dictionary<string, OfflineGeoworldStateDeltaLogEntry> deltasByEventId =
            new Dictionary<string, OfflineGeoworldStateDeltaLogEntry>(StringComparer.Ordinal);
        private OfflineGeoworldStateDeltaLog stateDeltaLog;
        private string initialStateHash = string.Empty;

        public string NearestTargetId { get { return nearestTargetId; } }
        public int AvailableActionCount { get { return availableActionCount; } }
        public int StateDeltaCount { get { return stateDeltaCount; } }
        public string LastStatus { get { return lastStatus; } }

        private void Awake()
        {
            stateDeltaLog = GetComponent<OfflineGeoworldStateDeltaLog>();
            if (stateDeltaLog == null)
            {
                stateDeltaLog = gameObject.AddComponent<OfflineGeoworldStateDeltaLog>();
            }
        }

        private void Start()
        {
            RefreshPayload();
            if (autoRunScriptedSession)
            {
                ExecuteScriptedSession();
            }
        }

        private void Update()
        {
            FindNearestTarget();
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExecuteManualAction("inspect");
            }
        }

        [ContextMenu("Refresh Goal105 Interaction Payload")]
        public void RefreshPayload()
        {
            var root = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            var diagnostics = new List<string>();
            var manifestJson = ReadFile(root, ManifestFileName, diagnostics);
            var targetsJson = ReadFile(root, TargetsFileName, diagnostics);
            var actionsJson = ReadFile(root, ActionsFileName, diagnostics);
            var sessionJson = ReadFile(root, SessionScriptFileName, diagnostics);
            var deltasJson = ReadFile(root, StateDeltaPlanFileName, diagnostics);

            targetCount = IntField(manifestJson, "targetCount");
            actionKindCount = IntField(manifestJson, "actionKindCount");
            scriptedEventCount = IntField(manifestJson, "scriptedEventCount");
            stateDeltaCount = IntField(manifestJson, "stateDeltaCount");
            initialStateHash = StringField(deltasJson, "initialStateHash");
            targetMetadataById.Clear();
            actionById.Clear();
            scriptedEvents.Clear();
            deltasByEventId.Clear();

            foreach (var block in Blocks(targetsJson, "targetId"))
            {
                var target = new OfflineGeoworldInteractionTargetMetadata
                {
                    TargetId = StringField(block, "targetId"),
                    TargetName = StringField(block, "targetName"),
                    SourceObjectId = StringField(block, "sourceObjectId"),
                    SourceObjectName = StringField(block, "sourceObjectName"),
                    SourceCommandId = StringField(block, "sourceCommandId"),
                    CommandKind = StringField(block, "commandKind"),
                    SourceChunkKey = StringField(block, "sourceChunkKey"),
                    GridX = IntField(block, "gridX"),
                    GridZ = IntField(block, "gridZ"),
                    Elevation = IntField(block, "elevation"),
                    InteractionRadius = FloatField(block, "interactionRadius")
                };
                if (!string.IsNullOrWhiteSpace(target.TargetId))
                {
                    targetMetadataById[target.TargetId] = target;
                }
            }

            foreach (var block in Blocks(actionsJson, "actionId"))
            {
                var action = new OfflineGeoworldInteractionActionMetadata
                {
                    ActionId = StringField(block, "actionId"),
                    TargetId = StringField(block, "targetId"),
                    ActionKind = StringField(block, "actionKind"),
                    RequiredRadius = FloatField(block, "requiredRadius"),
                    StateDeltaKind = StringField(block, "stateDeltaKind")
                };
                if (!string.IsNullOrWhiteSpace(action.ActionId))
                {
                    actionById[action.ActionId] = action;
                }
            }

            foreach (var block in Blocks(sessionJson, "eventId"))
            {
                scriptedEvents.Add(new OfflineGeoworldInteractionScriptedEventMetadata
                {
                    EventIndex = IntField(block, "eventIndex"),
                    EventId = StringField(block, "eventId"),
                    TargetId = StringField(block, "targetId"),
                    ActionId = StringField(block, "actionId"),
                    ActionKind = StringField(block, "actionKind"),
                    RequiredRadius = FloatField(block, "requiredRadius"),
                    ExpectedStateHashBefore = StringField(block, "expectedStateHashBefore"),
                    ExpectedStateHashAfter = StringField(block, "expectedStateHashAfter")
                });
            }

            foreach (var block in Blocks(deltasJson, "deltaIndex"))
            {
                var delta = new OfflineGeoworldStateDeltaLogEntry
                {
                    EventId = StringField(block, "eventId"),
                    TargetId = StringField(block, "targetId"),
                    ActionId = StringField(block, "actionId"),
                    ActionKind = StringField(block, "actionKind"),
                    DeltaKind = StringField(block, "deltaKind"),
                    PreviousStateHash = StringField(block, "previousStateHash"),
                    DeterministicStateHash = StringField(block, "deterministicStateHash")
                };
                if (!string.IsNullOrWhiteSpace(delta.EventId))
                {
                    deltasByEventId[delta.EventId] = delta;
                }
            }

            stateDeltaLog.ClearLog(initialStateHash);
            BindTargetsByIdOrName();
            FindNearestTarget();
            lastStatus = "goal105_interactions targets=" + targetCount
                         + " actions=" + actionById.Count
                         + " events=" + scriptedEvents.Count
                         + " deltas=" + deltasByEventId.Count
                         + " bound=" + boundTargetCount
                         + " missing=" + missingBindingCount
                         + " diagnostics=" + diagnostics.Count;
        }

        public void BindTargetsByIdOrName()
        {
            boundTargetsById.Clear();
            boundTargetCount = 0;
            missingBindingCount = 0;
            foreach (var metadata in targetMetadataById.Values)
            {
                var existingObject = GameObject.Find(metadata.SourceObjectName);
                if (existingObject == null)
                {
                    var child = transform.Find(metadata.SourceObjectName);
                    existingObject = child == null ? null : child.gameObject;
                }

                if (existingObject == null)
                {
                    existingObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    existingObject.name = metadata.SourceObjectName;
                    existingObject.transform.SetParent(transform, false);
                    existingObject.transform.localPosition =
                        new Vector3(metadata.GridX, metadata.Elevation * 0.05f, metadata.GridZ);
                    existingObject.transform.localScale = Vector3.one * 0.65f;
                    missingBindingCount++;
                }

                var target = existingObject.GetComponent<OfflineGeoworldInteractionTarget>();
                if (target == null)
                {
                    target = existingObject.AddComponent<OfflineGeoworldInteractionTarget>();
                }

                target.BindMetadata(metadata);
                boundTargetsById[metadata.TargetId] = target;
                boundTargetCount++;
            }
        }

        public void FindNearestTarget()
        {
            var origin = playerProxy == null ? transform.position : playerProxy.position;
            nearestTargetId = string.Empty;
            nearestTargetName = string.Empty;
            nearestTargetDistance = 0f;
            availableActionCount = 0;
            foreach (var pair in boundTargetsById)
            {
                var distance = Vector3.Distance(origin, pair.Value.transform.position);
                if (string.IsNullOrWhiteSpace(nearestTargetId) || distance < nearestTargetDistance)
                {
                    nearestTargetId = pair.Key;
                    nearestTargetName = pair.Value.TargetName;
                    nearestTargetDistance = distance;
                }
            }

            foreach (var action in actionById.Values)
            {
                if (boundTargetsById.TryGetValue(action.TargetId, out var target)
                    && target.IsAvailable(origin, action.RequiredRadius))
                {
                    availableActionCount++;
                }
            }
        }

        [ContextMenu("Execute Goal105 Scripted Session")]
        public void ExecuteScriptedSession()
        {
            foreach (var item in scriptedEvents)
            {
                ExecuteEvent(item);
            }
        }

        public bool ExecuteManualAction(string actionKind)
        {
            FindNearestTarget();
            foreach (var action in actionById.Values)
            {
                if (string.Equals(action.TargetId, nearestTargetId, StringComparison.Ordinal)
                    && string.Equals(action.ActionKind, actionKind, StringComparison.Ordinal))
                {
                    return ExecuteAction(action, "manual:" + actionKind);
                }
            }

            lastActionStatus = "manual action unavailable kind=" + actionKind;
            return false;
        }

        private bool ExecuteEvent(OfflineGeoworldInteractionScriptedEventMetadata item)
        {
            if (!actionById.TryGetValue(item.ActionId, out var action))
            {
                lastActionStatus = "missing action " + item.ActionId;
                return false;
            }

            return ExecuteAction(action, item.EventId);
        }

        private bool ExecuteAction(OfflineGeoworldInteractionActionMetadata action, string eventId)
        {
            if (!boundTargetsById.TryGetValue(action.TargetId, out var target))
            {
                lastActionStatus = "missing target " + action.TargetId;
                return false;
            }

            var origin = playerProxy == null ? transform.position : playerProxy.position;
            if (!target.IsAvailable(origin, action.RequiredRadius))
            {
                lastActionStatus = "unavailable action=" + action.ActionKind + " target=" + action.TargetId;
                return false;
            }

            target.ApplyDelta(action.ActionKind);
            if (deltasByEventId.TryGetValue(eventId, out var delta))
            {
                stateDeltaLog.AppendDelta(delta);
            }
            else
            {
                stateDeltaLog.AppendDelta(new OfflineGeoworldStateDeltaLogEntry
                {
                    EventId = eventId,
                    TargetId = action.TargetId,
                    ActionId = action.ActionId,
                    ActionKind = action.ActionKind,
                    DeltaKind = action.StateDeltaKind,
                    PreviousStateHash = stateDeltaLog.CurrentStateHash,
                    DeterministicStateHash = stateDeltaLog.CurrentStateHash
                });
            }

            lastActionStatus = "executed action=" + action.ActionKind
                               + " target=" + action.TargetId
                               + " deltas=" + stateDeltaLog.DeltaCount;
            return true;
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

        private static float FloatField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(-?\\d+(\\.\\d+)?)");
            float value;
            return match.Success && float.TryParse(match.Groups[1].Value, out value) ? value : 0f;
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldInteractionActionMetadata
    {
        public string ActionId = string.Empty;
        public string TargetId = string.Empty;
        public string ActionKind = string.Empty;
        public float RequiredRadius;
        public string StateDeltaKind = string.Empty;
    }

    [Serializable]
    public sealed class OfflineGeoworldInteractionScriptedEventMetadata
    {
        public int EventIndex;
        public string EventId = string.Empty;
        public string TargetId = string.Empty;
        public string ActionId = string.Empty;
        public string ActionKind = string.Empty;
        public float RequiredRadius;
        public string ExpectedStateHashBefore = string.Empty;
        public string ExpectedStateHashAfter = string.Empty;
    }
}
