using System;
using System.Collections.Generic;
using UnityEngine;

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
        public int ActionCount;
        public string FirstActionSummary = string.Empty;
        public string ExpectedStateDeltaSummary = string.Empty;
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
        public bool RootPresent;
        public bool BaselineLoaded;
        public bool PlayerProxyPresent;
        public bool ChunkWindowMarkerPresent;
        public bool InteractionOrObjectiveMarkerPresent;
        public bool DiagnosticsStatusPresent;
        public bool LegendPresent;
        public bool MarkerDescriptorPresent;
        public bool SelectableInteractionTargetPresent;
        public bool SelectedMarkerDetailsPresent;
        public bool InteractionPreviewPresent;
        public bool SelectableObjectivePresent;
        public bool ObjectiveReplayDetailsPresent;
        public bool VerificationEventLogPresent;
        public bool ProjectionActionPreviewPresent;
        public bool ProjectionActionApplyPassed;
        public bool ProjectionStateResetPassed;
        public bool WindowLayoutPolishPresent;
        public bool MaterialWarningGuardPresent;
        public bool ZeroFatalErrors;
        public bool FullVerificationPassed;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return RootPresent
                       && BaselineLoaded
                       && PlayerProxyPresent
                       && ChunkWindowMarkerPresent
                       && InteractionOrObjectiveMarkerPresent
                       && DiagnosticsStatusPresent
                       && LegendPresent
                       && MarkerDescriptorPresent
                       && SelectableInteractionTargetPresent
                       && SelectableObjectivePresent
                       && MaterialWarningGuardPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "passed=" + Passed
                   + "\nrootPresent=" + RootPresent
                   + "\nbaselineLoaded=" + BaselineLoaded
                   + "\nplayerProxyPresent=" + PlayerProxyPresent
                   + "\nchunkWindowMarkerPresent=" + ChunkWindowMarkerPresent
                   + "\ninteractionOrObjectiveMarkerPresent=" + InteractionOrObjectiveMarkerPresent
                   + "\ndiagnosticsStatusPresent=" + DiagnosticsStatusPresent
                   + "\nlegendPresent=" + LegendPresent
                   + "\nmarkerDescriptorPresent=" + MarkerDescriptorPresent
                   + "\nselectableInteractionTargetPresent=" + SelectableInteractionTargetPresent
                   + "\nselectedMarkerDetailsPresent=" + SelectedMarkerDetailsPresent
                   + "\ninteractionPreviewPresent=" + InteractionPreviewPresent
                   + "\nselectableObjectivePresent=" + SelectableObjectivePresent
                   + "\nobjectiveReplayDetailsPresent=" + ObjectiveReplayDetailsPresent
                   + "\nverificationEventLogPresent=" + VerificationEventLogPresent
                   + "\nprojectionActionPreviewPresent=" + ProjectionActionPreviewPresent
                   + "\nprojectionActionApplyPassed=" + ProjectionActionApplyPassed
                   + "\nprojectionStateResetPassed=" + ProjectionStateResetPassed
                   + "\nwindowLayoutPolishPresent=" + WindowLayoutPolishPresent
                   + "\nmaterialWarningGuardPresent=" + MaterialWarningGuardPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\nfullVerificationPassed=" + FullVerificationPassed
                   + "\nstatusLine=" + StatusLine;
        }
    }

    public sealed class AcceptedAlphaPlayableProjectionMarkerDescriptor : MonoBehaviour
    {
        public string MarkerId = string.Empty;
        public string MarkerName = string.Empty;
        public string MarkerKind = string.Empty;
        public string SourceGoal = string.Empty;
        public string SourceFile = string.Empty;
        public string DisplayLabel = string.Empty;
        public string Status = string.Empty;
        public string Details = string.Empty;

        public void Configure(
            string markerId,
            string markerName,
            string markerKind,
            string sourceGoal,
            string sourceFile,
            string displayLabel,
            string status,
            string details)
        {
            MarkerId = markerId ?? string.Empty;
            MarkerName = markerName ?? string.Empty;
            MarkerKind = markerKind ?? string.Empty;
            SourceGoal = sourceGoal ?? string.Empty;
            SourceFile = sourceFile ?? string.Empty;
            DisplayLabel = displayLabel ?? string.Empty;
            Status = status ?? string.Empty;
            Details = details ?? string.Empty;
        }
    }

    public sealed class AcceptedAlphaPlayableProjectionLegend : MonoBehaviour
    {
        public string[] Entries = Array.Empty<string>();

        public void Configure(params string[] entries)
        {
            Entries = entries ?? Array.Empty<string>();
        }
    }
}
