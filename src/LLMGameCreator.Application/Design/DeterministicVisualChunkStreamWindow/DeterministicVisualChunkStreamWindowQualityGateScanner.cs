namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public static class DeterministicVisualChunkStreamWindowQualityGateScanner
{
    public static VisualChunkStreamQualityGateScan Build(
        VisualChunkStreamCatalog catalog,
        VisualChunkStreamMaterializationManifest manifest,
        VisualChunkStreamDeterminismProof determinismProof,
        VisualChunkStreamSeamProof seamProof,
        VisualChunkStreamCacheReuseProof cacheReuseProof,
        VisualChunkStreamLayerTransitionProof layerTransitionProof,
        VisualChunkStreamNegativeProof negativeProof,
        VisualChunkStreamSourceLineage sourceLineage,
        IReadOnlyDictionary<string, string> overviewSvgByFixtureId)
    {
        var diagnostics = new List<VisualChunkStreamDiagnostic>();
        var allFixtures = new[]
        {
            DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId,
            DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId,
            DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId,
            DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId
        };
        var fixtureIds = catalog.Fixtures.Select(item => item.FixtureId).ToHashSet(StringComparer.Ordinal);
        var allFixtureWindowsMaterialized = allFixtures.All(fixtureIds.Contains)
            && catalog.Fixtures.All(item => item.WindowCount > 0 && item.TotalMaterializedChunks > 0);
        var boundaryClippingExplicit = catalog.Fixtures.Any(item =>
            item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId
            && item.BoundaryClippingExplicit);
        var hugeSparseNoRawDump = catalog.Fixtures.Any(item =>
            item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId
            && item.NoRawFullWorldDump)
            && manifest.Windows.Any(item =>
                item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId
                && item.EstimatedFullWorldChunkCapacity > item.ChunkCount);
        var infiniteOverlapReuse = cacheReuseProof.InfiniteOverlapReusedChunkKeyCount > 0;
        var safeSvgs = overviewSvgByFixtureId.Count == 4
            && overviewSvgByFixtureId.Values.All(DeterministicVisualChunkStreamWindowValidator.IsSvgSafe);

        AddIfFalse(allFixtureWindowsMaterialized, "visual_chunk_stream.fixtures.missing", "catalog", "All four required stream fixtures must materialize at least one window.", diagnostics);
        AddIfFalse(determinismProof.Passed, "visual_chunk_stream.determinism.failed", "determinism_proof", "Window materialization must be deterministic.", diagnostics);
        AddIfFalse(seamProof.Passed, "visual_chunk_stream.seam.failed", "seam_proof", "Water, road and biome seam continuity must pass.", diagnostics);
        AddIfFalse(cacheReuseProof.Passed, "visual_chunk_stream.cache.failed", "cache_reuse_proof", "Overlapping infinite windows must reuse chunk keys.", diagnostics);
        AddIfFalse(layerTransitionProof.Passed, "visual_chunk_stream.layer_transition.failed", "layer_transition_proof", "Layer transition proof must be data-driven.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_chunk_stream.negative.failed", "negative_proof", "Invalid matrix must reject expected cases.", diagnostics);
        AddIfFalse(sourceLineage.Passed, "visual_chunk_stream.source_lineage.failed", "source_lineage", "Goal090 and visual-stack lineage must exist.", diagnostics);
        AddIfFalse(boundaryClippingExplicit, "visual_chunk_stream.clipping.failed", "finite_fixture", "Finite 255x257 fixture must prove explicit boundary clipping.", diagnostics);
        AddIfFalse(hugeSparseNoRawDump, "visual_chunk_stream.huge_sparse.failed", "huge_sparse_fixture", "Huge sparse fixture must stay compact and avoid raw full-world dumps.", diagnostics);
        AddIfFalse(infiniteOverlapReuse, "visual_chunk_stream.infinite_overlap.failed", "cache_reuse_proof", "Infinite overlapping stream windows must prove reused chunk keys.", diagnostics);
        AddIfFalse(safeSvgs, "visual_chunk_stream.svg.failed", "stream_overviews", "Stream overviews must be compact safe text SVGs.", diagnostics);

        return new VisualChunkStreamQualityGateScan
        {
            Accepted = false,
            AllFixtureWindowsMaterialized = allFixtureWindowsMaterialized,
            DeterminismProofPassed = determinismProof.Passed,
            SeamProofPassed = seamProof.Passed,
            CacheReuseProofPassed = cacheReuseProof.Passed,
            LayerTransitionProofPassed = layerTransitionProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            BoundaryClippingExplicit = boundaryClippingExplicit,
            HugeSparseNoRawDump = hugeSparseNoRawDump,
            InfiniteOverlapReuseProven = infiniteOverlapReuse,
            SvgTextOnlyPreviews = safeSvgs,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/DeterministicVisualChunkStreamWindow/",
                "tests/LLMGameCreator.Tests/Application/DeterministicVisualChunkStreamWindow/",
                "tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualChunkStreamWindowProductSmokeTests.cs",
                ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/",
                "docs/agent-tasks/goal-091-deterministic-visual-chunk-stream-window/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = DeterministicVisualChunkStreamWindowValidator.SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualChunkStreamDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualChunkStreamDiagnostic.Error(code, target, message));
        }
    }
}
