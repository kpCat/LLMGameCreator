namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public static class DeterministicVisualMapPatchComposerQualityGateScanner
{
    public static VisualMapPatchQualityGateScan Build(
        VisualMapPatchComposerRequest request,
        VisualMapPatchValidationResult validation,
        VisualMapPatchWaterFlowProof waterFlowProof,
        VisualMapPatchReachabilityProof reachabilityProof,
        VisualMapPatchLayeringProof layeringProof,
        VisualMapPatchNegativeProof negativeProof,
        VisualMapPatchSourceLineage sourceLineage,
        IReadOnlyDictionary<string, string> svgByPatchId)
    {
        var diagnostics = new List<VisualMapPatchDiagnostic>();
        var patchCountPassed = request.Patches.Count >= 3
            && DeterministicVisualMapPatchComposerFixtures.RequiredPatchIds.All(id => request.Patches.Any(patch => patch.PatchId == id));
        var svgTextOnly = svgByPatchId.Count == request.Patches.Count
            && svgByPatchId.Values.All(DeterministicVisualMapPatchComposerValidator.IsSvgSafe);
        var allReferencesKnown = request.Patches
            .SelectMany(DeterministicVisualMapPatchComposerFixtures.ReferencedMicrotilePreviewIds)
            .All(DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds.Contains);

        AddIfFalse(patchCountPassed, "visual_map_patch.patch_count.failed", "catalog", "All three required map patches must be present.", diagnostics);
        AddIfFalse(validation.Passed, "visual_map_patch.validation.failed", "request", "Map patch request must validate.", diagnostics);
        AddIfFalse(svgTextOnly, "visual_map_patch.svg_hygiene.failed", "patch_svgs", "All map patch previews must be safe text SVG.", diagnostics);
        AddIfFalse(allReferencesKnown, "visual_map_patch.microtile_refs.failed", "catalog", "All map patch references must trace to Goal086 microtiles.", diagnostics);
        AddIfFalse(waterFlowProof.Passed, "visual_map_patch.water_flow.failed", "water_flow_proof", "Water/biome flow proof must pass.", diagnostics);
        AddIfFalse(reachabilityProof.Passed, "visual_map_patch.reachability.failed", "reachability_proof", "Road/path reachability proof must pass.", diagnostics);
        AddIfFalse(layeringProof.Passed, "visual_map_patch.layering.failed", "layering_proof", "Layering and rating fallback proof must pass.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_map_patch.negative_proof.failed", "negative_proof", "Negative proof must reject invalid cases.", diagnostics);
        AddIfFalse(sourceLineage.Passed, "visual_map_patch.source_lineage.failed", "source_lineage", "Goal084/085/086 source lineage must pass.", diagnostics);

        return new VisualMapPatchQualityGateScan
        {
            Accepted = false,
            PatchCountPassed = patchCountPassed,
            DeterministicRerunStable = true,
            SvgTextOnlyPreviews = svgTextOnly,
            AllReferencesKnownGoal086Microtiles = allReferencesKnown,
            WaterFlowProofPassed = waterFlowProof.Passed,
            ReachabilityProofPassed = reachabilityProof.Passed,
            LayeringProofPassed = layeringProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/DeterministicVisualMapPatchComposer/",
                "tests/LLMGameCreator.Tests/Application/DeterministicVisualMapPatchComposer/",
                "tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualMapPatchComposerProductSmokeTests.cs",
                ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/",
                "docs/agent-tasks/goal-087-deterministic-visual-map-patch-composer/"
            ],
            Diagnostics = DeterministicVisualMapPatchComposerValidator.SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualMapPatchDiagnostic.Error(code, target, message));
        }
    }
}
