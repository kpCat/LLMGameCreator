using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class AcceptedAlphaPlayableProjectionState
    {
        private readonly List<string> eventLog = new List<string>();

        public string SelectedInteractionTargetId { get; private set; } = string.Empty;
        public string SelectedInteractionTargetKind { get; private set; } = string.Empty;
        public string PreviewActionSummary { get; private set; } = string.Empty;
        public string LastAppliedActionSummary { get; private set; } = string.Empty;
        public int AppliedActionCount { get; private set; }
        public bool LastApplyPassed { get; private set; }
        public bool LastResetPassed { get; private set; }

        public string StatusLine
        {
            get
            {
                return "selectedTargetId=" + EmptyAsNone(SelectedInteractionTargetId)
                       + "; selectedTargetKind=" + EmptyAsNone(SelectedInteractionTargetKind)
                       + "; preview=" + (string.IsNullOrWhiteSpace(PreviewActionSummary) ? "empty" : "present")
                       + "; appliedActionCount=" + AppliedActionCount
                       + "; lastApplyPassed=" + LastApplyPassed
                       + "; lastResetPassed=" + LastResetPassed;
            }
        }

        public string EventLogText
        {
            get
            {
                return eventLog.Count == 0
                    ? "projectionActionLoopEventLog=empty"
                    : string.Join("\n", eventLog.ToArray());
            }
        }

        public void SelectTarget(string markerId, string markerKind)
        {
            SelectedInteractionTargetId = markerId ?? string.Empty;
            SelectedInteractionTargetKind = markerKind ?? string.Empty;
            AddEvent("selectTarget id=" + EmptyAsNone(SelectedInteractionTargetId)
                     + " kind=" + EmptyAsNone(SelectedInteractionTargetKind));
        }

        public void SetPreview(string previewActionSummary)
        {
            PreviewActionSummary = previewActionSummary ?? string.Empty;
            LastApplyPassed = false;
            LastResetPassed = false;
            AddEvent("previewSelectedAction present=" + !string.IsNullOrWhiteSpace(PreviewActionSummary));
        }

        public bool ApplyPreview()
        {
            LastApplyPassed = !string.IsNullOrWhiteSpace(PreviewActionSummary);
            LastResetPassed = false;
            if (LastApplyPassed)
            {
                AppliedActionCount++;
                LastAppliedActionSummary = PreviewActionSummary;
            }

            AddEvent("applyPreviewAction passed=" + LastApplyPassed
                     + " appliedActionCount=" + AppliedActionCount);
            return LastApplyPassed;
        }

        public bool Reset()
        {
            PreviewActionSummary = string.Empty;
            LastAppliedActionSummary = string.Empty;
            AppliedActionCount = 0;
            LastApplyPassed = false;
            LastResetPassed = true;
            AddEvent("resetProjectionState passed=True");
            return LastResetPassed;
        }

        private void AddEvent(string message)
        {
            eventLog.Add(message);
        }

        private static string EmptyAsNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }

    public sealed partial class AcceptedAlphaPlayableProjectionController
    {
        public void SelectProjectionActionTarget(GameObject marker)
        {
            var descriptor = marker == null
                ? null
                : marker.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor == null)
            {
                selectedMarkerId = string.Empty;
                selectedMarkerKind = string.Empty;
                selectedMarkerDetails = string.Empty;
                projectionState.SelectTarget(string.Empty, string.Empty);
                statusLine = "Goal122 projection action target unavailable";
                UpdateProjectionStateMarker();
                return;
            }

            selectedMarkerId = descriptor.MarkerId;
            selectedMarkerKind = descriptor.MarkerKind;
            selectedMarkerDetails = "markerId=" + descriptor.MarkerId
                                    + "\nmarkerKind=" + descriptor.MarkerKind
                                    + "\ndisplayLabel=" + descriptor.DisplayLabel
                                    + "\nstatus=" + descriptor.Status
                                    + "\ndetails=" + descriptor.Details;
            projectionState.SelectTarget(descriptor.MarkerId, descriptor.MarkerKind);
            statusLine = "Goal122 projection action target selected: " + descriptor.MarkerId;
            UpdateProjectionStateMarker();
        }

        public string PreviewSelectedAction()
        {
            if (string.IsNullOrWhiteSpace(selectedMarkerId))
            {
                SelectProjectionActionTarget(FindNextMarkerByKind("interaction", 0));
            }

            if (string.IsNullOrWhiteSpace(selectedMarkerId))
            {
                projectionActionPreviewPresent = false;
                projectionState.SetPreview(string.Empty);
                statusLine = "Goal122 projection action preview unavailable";
                UpdateProjectionStateMarker();
                return string.Empty;
            }

            var marker = FindMarkerById(selectedMarkerId);
            var descriptor = marker == null
                ? null
                : marker.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            var displayLabel = descriptor == null ? selectedMarkerId : descriptor.DisplayLabel;
            var preview = AcceptedAlphaPlayableProjectionActionPreview.BuildProjectionActionSummary(
                selectedMarkerId,
                displayLabel,
                ReadAcceptedStreamingPayload("OfflineGeoworldGoal105",
                    "offline-geoworld-interaction-actions.json"),
                ReadAcceptedStreamingPayload("OfflineGeoworldGoal105",
                    "offline-geoworld-interaction-state-delta-plan.json"));
            projectionState.SetPreview(preview);
            projectionActionPreviewPresent = !string.IsNullOrWhiteSpace(preview);
            interactionPreview = interactionPreview
                                 + (string.IsNullOrWhiteSpace(interactionPreview) ? string.Empty : "\n\n")
                                 + "Projection Action Preview\n" + preview
                                 + "\nprojectionStateStatus=" + projectionState.StatusLine;
            verificationEventLog = AppendProjectionEvent(verificationEventLog, projectionState.EventLogText);
            statusLine = projectionActionPreviewPresent
                ? "Goal122 projection action preview ready"
                : "Goal122 projection action preview missing";
            UpdateProjectionStateMarker();
            return preview;
        }

        public bool ApplyPreviewActionToProjectionState()
        {
            projectionActionApplyPassed = projectionState.ApplyPreview();
            interactionPreview = interactionPreview
                                 + "\n\nProjection State After Apply\n"
                                 + projectionState.StatusLine;
            verificationEventLog = AppendProjectionEvent(verificationEventLog, projectionState.EventLogText);
            statusLine = projectionActionApplyPassed
                ? "Goal122 projection action applied to projection state"
                : "Goal122 projection action apply skipped";
            UpdateProjectionStateMarker();
            return projectionActionApplyPassed;
        }

        public bool ResetProjectionState()
        {
            projectionStateResetPassed = projectionState.Reset();
            verificationEventLog = AppendProjectionEvent(verificationEventLog, projectionState.EventLogText);
            statusLine = projectionStateResetPassed
                ? "Goal122 projection state reset"
                : "Goal122 projection state reset failed";
            UpdateProjectionStateMarker();
            return projectionStateResetPassed;
        }

        private void UpdateProjectionStateMarker()
        {
            var marker = FindDescendantObjectWithPrefix(transform, "goal122_projection_state_marker");
            if (marker == null)
            {
                var section = FindDescendantObjectWithPrefix(transform, "goal120_legend_diagnostics")
                              ?? AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateSection(
                                  transform,
                                  "goal120_legend_diagnostics",
                                  new Vector3(-7f, 0f, 0f));
                marker = AcceptedAlphaPlayableProjectionPrimitiveFactory.CreateText(
                    section.transform,
                    "goal122_projection_state_marker",
                    "Goal122 projection state: " + projectionState.StatusLine,
                    new Vector3(4f, 1.2f, 5f),
                    Color.magenta,
                    0.24f);
                AttachDescriptor(marker, "goal122_projection_state_marker",
                    "goal122_projection_state_marker", "diagnostics", "goal122",
                    ControllerSourceFile, "Projection action loop state", "ready",
                    "Projection-only action loop state marker. No files, scenes, prefabs, StreamingAssets, Runtime, providers or schema are mutated.");
                return;
            }

            var text = marker.GetComponent<TextMesh>();
            if (text != null)
            {
                text.text = "Goal122 projection state: " + projectionState.StatusLine;
            }

            var descriptor = marker.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor != null)
            {
                descriptor.Status = projectionState.LastApplyPassed || projectionState.LastResetPassed
                    ? "ready"
                    : "pending";
                descriptor.Details = projectionState.EventLogText.Replace("\n", "; ");
            }
        }

        private static GameObject FindDescendantObjectWithDescriptorId(Transform root, string markerId)
        {
            var descriptor = root.GetComponent<AcceptedAlphaPlayableProjectionMarkerDescriptor>();
            if (descriptor != null
                && string.Equals(descriptor.MarkerId, markerId, StringComparison.Ordinal))
            {
                return root.gameObject;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var match = FindDescendantObjectWithDescriptorId(root.GetChild(i), markerId);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static string AppendProjectionEvent(string existingLog, string projectionLog)
        {
            if (string.IsNullOrWhiteSpace(existingLog))
            {
                return projectionLog ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(projectionLog)
                ? existingLog
                : existingLog + "\n" + projectionLog;
        }
    }
}
