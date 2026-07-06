using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public static class AcceptedAlphaPlayableProjectionDrilldown
    {
        public static string DescribeMarker(GameObject marker)
        {
            if (marker == null)
            {
                return "markerSelected=false";
            }

            var descriptor = marker.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor == null)
            {
                return "markerSelected=true"
                       + "\nmarkerName=" + marker.name
                       + "\nmarkerKind=unknown"
                       + "\nstatus=descriptor_missing";
            }

            return "markerSelected=true"
                   + "\nmarkerId=" + descriptor.MarkerId
                   + "\nmarkerName=" + descriptor.MarkerName
                   + "\nmarkerKind=" + descriptor.MarkerKind
                   + "\nsourceGoal=" + descriptor.SourceGoal
                   + "\nsourceFile=" + descriptor.SourceFile
                   + "\ndisplayLabel=" + descriptor.DisplayLabel
                   + "\nstatus=" + descriptor.Status
                   + "\ndetails=" + descriptor.Details;
        }

        public static string BuildObjectiveReplayDetails(
            string objectiveId,
            string objectivesJson,
            string replayJson)
        {
            var objectiveBlock = string.Empty;
            foreach (var block in AcceptedAlphaPlayableProjectionDiagnostics.Blocks(objectivesJson, "objectiveId"))
            {
                if (AcceptedAlphaPlayableProjectionDiagnostics.StringField(block, "objectiveId") == objectiveId)
                {
                    objectiveBlock = block;
                    break;
                }
            }

            var title = AcceptedAlphaPlayableProjectionDiagnostics.StringField(objectiveBlock, "title");
            var completionState =
                AcceptedAlphaPlayableProjectionDiagnostics.StringField(objectiveBlock, "completionState");
            var expectedStateHash =
                AcceptedAlphaPlayableProjectionDiagnostics.StringField(objectiveBlock, "expectedStateHashAfter");
            var replayStepCount = AcceptedAlphaPlayableProjectionDiagnostics.IntField(replayJson, "replayStepCount");
            var checkpointStepIndex = AcceptedAlphaPlayableProjectionDiagnostics.IntField(replayJson, "stepIndex");
            var finalStateHash = AcceptedAlphaPlayableProjectionDiagnostics.StringField(replayJson, "finalStateHash");

            return "objectiveId=" + objectiveId
                   + "\ntitle=" + title
                   + "\ncompletionState=" + completionState
                   + "\nexpectedStateHashAfter=" + expectedStateHash
                   + "\nreplayStepCount=" + replayStepCount
                   + "\ncheckpointStepIndex=" + checkpointStepIndex
                   + "\nreplayFinalStateHash=" + finalStateHash;
        }
    }
}
