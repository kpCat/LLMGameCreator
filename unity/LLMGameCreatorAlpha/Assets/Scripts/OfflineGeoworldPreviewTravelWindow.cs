using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPreviewTravelWindow : MonoBehaviour
    {
        [SerializeField] private int currentStepIndex;
        [SerializeField] private int stepCount;
        [SerializeField] private string lastStepId = string.Empty;

        public int CurrentStepIndex { get { return currentStepIndex; } }
        public int StepCount { get { return stepCount; } }
        public string LastStepId { get { return lastStepId; } }

        public void LoadScript(string travelWindowJson)
        {
            stepCount = IntField(travelWindowJson, "stepCount");
            currentStepIndex = 0;
            lastStepId = StepIdAt(travelWindowJson, currentStepIndex);
        }

        public void ApplyStep(
            int stepIndex,
            string travelWindowJson,
            IReadOnlyList<GameObject> spawnedObjects)
        {
            stepCount = IntField(travelWindowJson, "stepCount");
            if (stepCount <= 0)
            {
                currentStepIndex = 0;
                lastStepId = string.Empty;
                SetAll(spawnedObjects, true);
                return;
            }

            currentStepIndex = Mathf.Clamp(stepIndex, 0, stepCount - 1);
            lastStepId = StepIdAt(travelWindowJson, currentStepIndex);
            SetAll(spawnedObjects, true);
        }

        public void Next(string travelWindowJson, IReadOnlyList<GameObject> spawnedObjects)
        {
            var next = stepCount <= 0 ? 0 : (currentStepIndex + 1) % stepCount;
            ApplyStep(next, travelWindowJson, spawnedObjects);
        }

        private static void SetAll(IReadOnlyList<GameObject> objects, bool active)
        {
            if (objects == null)
            {
                return;
            }

            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].SetActive(active);
                }
            }
        }

        private static string StepIdAt(string json, int index)
        {
            var pattern = "\\{[^\\{\\}]*\"stepIndex\"\\s*:\\s*" + index + "[\\s\\S]*?\\}";
            var block = Regex.Match(json ?? string.Empty, pattern).Value;
            var match = Regex.Match(block, "\"stepId\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int IntField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(\\d+)");
            int value;
            return match.Success && int.TryParse(match.Groups[1].Value, out value) ? value : 0;
        }
    }
}
