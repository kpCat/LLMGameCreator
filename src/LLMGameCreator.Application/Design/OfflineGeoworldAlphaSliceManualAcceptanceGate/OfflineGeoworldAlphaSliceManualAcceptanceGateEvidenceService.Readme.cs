namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

public sealed partial class OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService
{
    private static string RenderReadme(OfflineGeoworldAlphaAcceptanceManifest manifest)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Manual Acceptance Runner",
            string.Empty,
            "This package is an Alpha-only manual acceptance runner and release-gate dashboard over the Goal109 export package.",
            "It is not a final release package, installer, Runtime consumer, real geodata import, provider path or final-art path.",
            string.Empty,
            "- accepted: false",
            "- manualGate: " + manifest.ManualGate + " required",
            "- manualAcceptancePending: true",
            "- automatedGatePassed: " + manifest.AutomatedGatePassed.ToString().ToLowerInvariant(),
            "- checklistStepCount: " + manifest.ChecklistStepCount,
            "- resultTemplateRelativePath: " + manifest.ResultTemplateRelativePath,
            "- alphaRuntimeBootstrapUnchanged: "
            + manifest.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
