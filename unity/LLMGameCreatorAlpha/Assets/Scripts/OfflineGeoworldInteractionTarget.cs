using System;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldInteractionTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = string.Empty;
        [SerializeField] private string targetName = string.Empty;
        [SerializeField] private string sourceObjectId = string.Empty;
        [SerializeField] private string sourceObjectName = string.Empty;
        [SerializeField] private string sourceCommandId = string.Empty;
        [SerializeField] private string commandKind = string.Empty;
        [SerializeField] private string sourceChunkKey = string.Empty;
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private bool visited;
        [SerializeField] private bool blocked;
        [SerializeField] private bool sampleCollected;
        [SerializeField] private string lastActionKind = string.Empty;
        [SerializeField] private string lastStatus = string.Empty;

        public string TargetId { get { return targetId; } }
        public string TargetName { get { return targetName; } }
        public string SourceObjectId { get { return sourceObjectId; } }
        public string SourceObjectName { get { return sourceObjectName; } }
        public string CommandKind { get { return commandKind; } }
        public float InteractionRadius { get { return interactionRadius; } }
        public bool Visited { get { return visited; } }
        public bool Blocked { get { return blocked; } }
        public bool SampleCollected { get { return sampleCollected; } }
        public string LastStatus { get { return lastStatus; } }

        public void BindMetadata(OfflineGeoworldInteractionTargetMetadata metadata)
        {
            targetId = metadata.TargetId;
            targetName = metadata.TargetName;
            sourceObjectId = metadata.SourceObjectId;
            sourceObjectName = metadata.SourceObjectName;
            sourceCommandId = metadata.SourceCommandId;
            commandKind = metadata.CommandKind;
            sourceChunkKey = metadata.SourceChunkKey;
            interactionRadius = metadata.InteractionRadius;
            lastStatus = "bound target=" + targetId + " source=" + sourceObjectId;
        }

        public bool IsAvailable(Vector3 playerPosition, float requiredRadius)
        {
            var radius = Mathf.Min(interactionRadius, requiredRadius <= 0f ? interactionRadius : requiredRadius);
            return Vector3.Distance(playerPosition, transform.position) <= radius;
        }

        public void ApplyDelta(string actionKind)
        {
            lastActionKind = actionKind;
            if (string.Equals(actionKind, "mark_visited", StringComparison.Ordinal)
                || string.Equals(actionKind, "enter_or_focus", StringComparison.Ordinal)
                || string.Equals(actionKind, "inspect", StringComparison.Ordinal))
            {
                visited = true;
            }
            else if (string.Equals(actionKind, "toggle_blocked", StringComparison.Ordinal))
            {
                blocked = !blocked;
            }
            else if (string.Equals(actionKind, "collect_sample", StringComparison.Ordinal))
            {
                sampleCollected = true;
            }

            lastStatus = "action=" + lastActionKind
                         + " visited=" + visited.ToString().ToLowerInvariant()
                         + " blocked=" + blocked.ToString().ToLowerInvariant()
                         + " sample=" + sampleCollected.ToString().ToLowerInvariant();
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldInteractionTargetMetadata
    {
        public string TargetId = string.Empty;
        public string TargetName = string.Empty;
        public string SourceObjectId = string.Empty;
        public string SourceObjectName = string.Empty;
        public string SourceCommandId = string.Empty;
        public string CommandKind = string.Empty;
        public string SourceChunkKey = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
        public float InteractionRadius = 3f;
    }
}
