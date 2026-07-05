using System;
using System.Collections.Generic;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class AcceptedAlphaProjectionCommand
    {
        public string CommandId = string.Empty;
        public string CommandKind = string.Empty;
        public string StyleKey = string.Empty;
        public string SourceChunkKey = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
    }

    [Serializable]
    public sealed class AcceptedAlphaProjectionTarget
    {
        public string TargetId = string.Empty;
        public string TargetName = string.Empty;
        public string CommandKind = string.Empty;
        public int GridX;
        public int GridZ;
        public int Elevation;
        public float InteractionRadius;
    }

    [Serializable]
    public sealed class AcceptedAlphaProjectionObjective
    {
        public string ObjectiveId = string.Empty;
        public string Title = string.Empty;
        public string CompletionState = string.Empty;
    }

    [Serializable]
    public sealed class AcceptedAlphaProjectionSummary
    {
        public string BaselineId = string.Empty;
        public string ManualGateStatus = string.Empty;
        public bool AcceptedBaselineReady;
        public bool Goal116Accepted;
        public int PreviewCommandCount;
        public int ChunkWindowStepCount;
        public int BoundaryCrossingCount;
        public int InteractionTargetCount;
        public int ObjectiveCount;
        public int CompletedObjectiveCount;
        public int ReplayStepCount;
        public readonly List<string> Diagnostics = new List<string>();
    }

    [Serializable]
    public sealed class AcceptedAlphaProjectionSmokeResult
    {
        public bool BaselineLoaded;
        public bool PlayerProxyPresent;
        public bool ChunkWindowMarkerPresent;
        public bool InteractionOrObjectiveMarkerPresent;
        public bool DiagnosticsStatusPresent;
        public bool ZeroFatalErrors;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return BaselineLoaded
                       && PlayerProxyPresent
                       && ChunkWindowMarkerPresent
                       && InteractionOrObjectiveMarkerPresent
                       && DiagnosticsStatusPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "passed=" + Passed
                   + "\nbaselineLoaded=" + BaselineLoaded
                   + "\nplayerProxyPresent=" + PlayerProxyPresent
                   + "\nchunkWindowMarkerPresent=" + ChunkWindowMarkerPresent
                   + "\ninteractionOrObjectiveMarkerPresent=" + InteractionOrObjectiveMarkerPresent
                   + "\ndiagnosticsStatusPresent=" + DiagnosticsStatusPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\nstatusLine=" + StatusLine;
        }
    }
}
