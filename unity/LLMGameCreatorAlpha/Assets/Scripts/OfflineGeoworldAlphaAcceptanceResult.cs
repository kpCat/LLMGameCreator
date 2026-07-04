using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldAlphaAcceptanceResult
    {
        public string goalId = "goal_110_offline_geoworld_alpha_manual_acceptance_gate";
        public string manualGate = "offline_geoworld_alpha_manual_acceptance_verification";
        public bool accepted;
        public bool manualAcceptancePending = true;
        public bool automatedGatePassed = true;
        public string resultStatus = "manual_result_required";
        public string checklistHash = string.Empty;
        public string resultTemplateHash = string.Empty;
        public string packagePath = string.Empty;
        public string diagnostics = string.Empty;
        public string resultHash = string.Empty;
        public List<OfflineGeoworldAlphaAcceptanceStepResult> steps =
            new List<OfflineGeoworldAlphaAcceptanceStepResult>();

        public static OfflineGeoworldAlphaAcceptanceResult CreatePending(
            IEnumerable<string> stepIds,
            string packagePathValue,
            string checklistHashValue,
            string resultTemplateHashValue)
        {
            var result = new OfflineGeoworldAlphaAcceptanceResult
            {
                accepted = false,
                manualAcceptancePending = true,
                automatedGatePassed = true,
                resultStatus = "manual_result_pending",
                checklistHash = checklistHashValue ?? string.Empty,
                resultTemplateHash = resultTemplateHashValue ?? string.Empty,
                packagePath = packagePathValue ?? string.Empty
            };
            foreach (var stepId in stepIds)
            {
                result.steps.Add(new OfflineGeoworldAlphaAcceptanceStepResult
                {
                    stepId = stepId,
                    status = "pending",
                    notes = string.Empty,
                    evidenceRef = stepId + "Evidence"
                });
            }

            result.resultHash = result.ComputeStableHash();
            return result;
        }

        public string ToJson()
        {
            resultHash = ComputeStableHash();
            return JsonUtility.ToJson(this, true);
        }

        public string ComputeStableHash()
        {
            var count = steps == null ? 0 : steps.Count;
            return goalId + "|" + manualGate + "|" + accepted + "|" + manualAcceptancePending
                   + "|" + automatedGatePassed + "|" + resultStatus + "|" + checklistHash
                   + "|" + resultTemplateHash + "|" + count;
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldAlphaAcceptanceStepResult
    {
        public string stepId = string.Empty;
        public string status = "pending";
        public string notes = string.Empty;
        public string evidenceRef = string.Empty;
    }
}
