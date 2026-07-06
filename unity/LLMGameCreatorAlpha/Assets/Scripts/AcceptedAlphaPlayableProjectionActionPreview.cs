using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    public static class AcceptedAlphaPlayableProjectionActionPreview
    {
        public static string BuildInteractionPreview(
            string targetId,
            string targetName,
            string actionsJson,
            string stateDeltaJson)
        {
            var actionCount = CountActionsForTarget(actionsJson, targetId);
            var firstAction = FirstActionSummaryForTarget(actionsJson, targetId);
            var delta = StateDeltaSummaryForTarget(stateDeltaJson, targetId);
            return "targetId=" + targetId
                   + "\ntargetName=" + targetName
                   + "\nactionCount=" + actionCount
                   + "\nfirstAction=" + firstAction
                   + "\nexpectedStateDelta=" + delta;
        }

        public static int CountActionsForTarget(string actionsJson, string targetId)
        {
            var count = 0;
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(actionsJson, "actionId"))
            {
                if (AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "targetId") == targetId)
                {
                    count++;
                }
            }

            return count;
        }

        public static string FirstActionSummaryForTarget(string actionsJson, string targetId)
        {
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(actionsJson, "actionId"))
            {
                if (AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "targetId") != targetId)
                {
                    continue;
                }

                var actionKind = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "actionKind");
                var displayLabel = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "displayLabel");
                var deltaKind = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "stateDeltaKind");
                return actionKind + ": " + displayLabel + "; deltaKind=" + deltaKind;
            }

            return "none";
        }

        public static string StateDeltaSummaryForTarget(string stateDeltaJson, string targetId)
        {
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(stateDeltaJson, "deltaIndex"))
            {
                if (AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "targetId") != targetId)
                {
                    continue;
                }

                var deltaKind = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "deltaKind");
                var stateKey = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "stateKey");
                var stateValue = AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "stateValue");
                return deltaKind + ": " + stateKey + "=" + stateValue;
            }

            return "none";
        }
    }
}
