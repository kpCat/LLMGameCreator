namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public static class DeterministicVisualRegionComposerQualityGateScanner
{
    public static VisualRegionQualityGateScan Build(
        VisualRegionDefinition definition,
        VisualRegionValidationResult validation,
        VisualRegionPatchPlacementIndex patchPlacementIndex,
        VisualRegionChunkIndex chunkIndex,
        VisualRegionWaterNetworkProof waterNetworkProof,
        VisualRegionRoadReachabilityProof roadReachabilityProof,
        VisualRegionLayerTransitionProof layerTransitionProof,
        VisualRegionObjectPlacementProof objectPlacementProof,
        VisualRegionNegativeProof negativeProof,
        VisualRegionSourceLineage sourceLineage,
        IReadOnlyDictionary<string, string> overviewSvgByFileName)
    {
        var diagnostics = new List<VisualRegionDiagnostic>();
        var dimensionsPassed = definition.Width == DeterministicVisualRegionComposerVocabulary.RegionWidth
            && definition.Height == DeterministicVisualRegionComposerVocabulary.RegionHeight
            && definition.LayerCount == DeterministicVisualRegionComposerVocabulary.LayerCount
            && definition.DerivedLogicalCellCount == DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount;
        var compactArtifactsPassed = !definition.HeavyRawCellMode
            && definition.ExplicitRawCellRecordCount == 0
            && chunkIndex.Passed
            && patchPlacementIndex.DerivedLogicalCellCount == DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount;
        var safeSvg = overviewSvgByFileName.Count == 3
            && overviewSvgByFileName.Values.All(DeterministicVisualRegionComposerValidator.IsSvgSafe);

        AddIfFalse(dimensionsPassed, "visual_region.dimensions.failed", "definition", "144x144x2 dimensions must pass.", diagnostics);
        AddIfFalse(validation.Passed, "visual_region.validation.failed", "definition", "Region definition must validate.", diagnostics);
        AddIfFalse(patchPlacementIndex.Passed, "visual_region.patch_placement_index.failed", "patch_placement_index", "Patch placement index must pass.", diagnostics);
        AddIfFalse(compactArtifactsPassed, "visual_region.compact_artifacts.failed", "definition", "Region evidence must remain compact and avoid raw cell dumps.", diagnostics);
        AddIfFalse(patchPlacementIndex.AllPatchIdsKnownGoal087, "visual_region.goal087_refs.failed", "patch_placement_index", "All placements must reference known Goal087 patch ids.", diagnostics);
        AddIfFalse(waterNetworkProof.Passed, "visual_region.water_network.failed", "water_network_proof", "Water network proof must pass.", diagnostics);
        AddIfFalse(roadReachabilityProof.Passed, "visual_region.road_reachability.failed", "road_reachability_proof", "Road reachability proof must pass.", diagnostics);
        AddIfFalse(layerTransitionProof.Passed, "visual_region.layer_transition.failed", "layer_transition_proof", "Layer transition proof must pass.", diagnostics);
        AddIfFalse(objectPlacementProof.Passed, "visual_region.object_placement.failed", "object_placement_proof", "Object placement proof must pass.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_region.negative_proof.failed", "negative_proof", "Negative proof must reject invalid cases.", diagnostics);
        AddIfFalse(sourceLineage.Passed, "visual_region.source_lineage.failed", "source_lineage", "Goal084/085/086/087 source lineage must pass.", diagnostics);
        AddIfFalse(safeSvg, "visual_region.svg_overviews.failed", "overview_svgs", "Overview SVGs must be safe text SVG.", diagnostics);

        return new VisualRegionQualityGateScan
        {
            Accepted = false,
            DimensionsPassed = dimensionsPassed,
            PatchPlacementCountPassed = patchPlacementIndex.Passed,
            CompactArtifactsPassed = compactArtifactsPassed,
            Goal087ReferencesPassed = patchPlacementIndex.AllPatchIdsKnownGoal087,
            WaterNetworkProofPassed = waterNetworkProof.Passed,
            RoadReachabilityProofPassed = roadReachabilityProof.Passed,
            LayerTransitionProofPassed = layerTransitionProof.Passed,
            ObjectPlacementProofPassed = objectPlacementProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            SafeSvgOverviewsPassed = safeSvg,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/DeterministicVisualRegionComposer/",
                "tests/LLMGameCreator.Tests/Application/DeterministicVisualRegionComposer/",
                "tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualRegionComposerProductSmokeTests.cs",
                ".llmgc/procedural/goal-088-deterministic-visual-region-composer/",
                "docs/agent-tasks/goal-088-deterministic-visual-region-composer/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = DeterministicVisualRegionComposerValidator.SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualRegionDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualRegionDiagnostic.Error(code, target, message));
        }
    }
}
