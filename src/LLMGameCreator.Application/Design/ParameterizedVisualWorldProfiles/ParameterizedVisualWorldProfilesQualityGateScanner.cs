namespace LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

public static class ParameterizedVisualWorldProfilesQualityGateScanner
{
    public static VisualWorldProfileQualityGateScan Build(
        VisualWorldProfileCatalog catalog,
        VisualWorldProfileSizeMatrix sizeMatrix,
        VisualWorldProfileValidationMatrix validationMatrix,
        VisualWorldProfileNegativeProof negativeProof,
        VisualWorldProfileChunkAddressProof chunkAddressProof,
        VisualWorldProfileSparseWorldProof sparseWorldProof,
        VisualWorldProfileLayerModelProof layerModelProof,
        VisualWorldProfileSourceLineage sourceLineage,
        IReadOnlyDictionary<string, string> overviewSvgByRelativePath)
    {
        var diagnostics = new List<VisualWorldProfileDiagnostic>();
        var benchmark = catalog.Profiles.SingleOrDefault(item => item.ProfileId == "benchmark_heroes_144x144_surface_underground");
        var benchmark144OnlyFixture = benchmark is { IsBenchmarkProfile: true, FiniteWidth: 144, FiniteHeight: 144 }
            && catalog.Profiles.Count(item => item.FiniteWidth == 144 && item.FiniteHeight == 144) >= 1
            && catalog.Profiles.Any(item => item.ProfileId == "finite_custom_sizes_matrix"
                && item.FiniteSizeSamples.Any(size => size.Width == 255 && size.Height == 257));
        var noRawHeavyDump = catalog.Profiles.All(profile =>
            !profile.RawCellDumpAllowed
            && !profile.SparseRegionIndex.AttemptsRawCellDump
            && profile.SparseRegionIndex.MaterializedChunks.Count < 100);
        var safeSvgs = overviewSvgByRelativePath.Count == 4
            && overviewSvgByRelativePath.Values.All(ParameterizedVisualWorldProfilesValidator.IsSvgSafe);

        AddIfFalse(validationMatrix.Passed, "visual_world.validation_matrix.failed", "validation_matrix", "All valid fixtures must pass the generic validator.", diagnostics);
        AddIfFalse(sizeMatrix.Passed, "visual_world.size_matrix.failed", "size_matrix", "Finite size matrix must validate through the generic path.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_world.negative_proof.failed", "negative_proof", "Negative proof must reject invalid cases.", diagnostics);
        AddIfFalse(chunkAddressProof.Passed, "visual_world.chunk_address_proof.failed", "chunk_address_proof", "Chunk key proof must be deterministic and variant-sensitive.", diagnostics);
        AddIfFalse(sparseWorldProof.Passed, "visual_world.sparse_world_proof.failed", "sparse_world_proof", "Huge and infinite worlds must remain sparse.", diagnostics);
        AddIfFalse(layerModelProof.Passed, "visual_world.layer_model_proof.failed", "layer_model_proof", "Layer model proof must be data-driven.", diagnostics);
        AddIfFalse(sourceLineage.Passed, "visual_world.source_lineage.failed", "source_lineage", "Goal087/088 lineage must exist.", diagnostics);
        AddIfFalse(benchmark144OnlyFixture, "visual_world.benchmark_144.failed", "catalog", "144x144 must be present only as benchmark fixture coverage.", diagnostics);
        AddIfFalse(noRawHeavyDump, "visual_world.raw_cell_dump.failed", "catalog", "No huge or infinite profile may emit raw heavy cell dumps.", diagnostics);
        AddIfFalse(safeSvgs, "visual_world.svg_overviews.failed", "profile_overviews", "Profile overview SVGs must be compact safe text diagrams.", diagnostics);

        return new VisualWorldProfileQualityGateScan
        {
            Accepted = false,
            ValidationMatrixPassed = validationMatrix.Passed,
            SizeMatrixPassed = sizeMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            ChunkAddressProofPassed = chunkAddressProof.Passed,
            SparseWorldProofPassed = sparseWorldProof.Passed,
            LayerModelProofPassed = layerModelProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            Benchmark144OnlyFixturePassed = benchmark144OnlyFixture,
            NoRawHeavyCellDump = noRawHeavyDump,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/ParameterizedVisualWorldProfiles/",
                "tests/LLMGameCreator.Tests/Application/ParameterizedVisualWorldProfiles/",
                "tests/LLMGameCreator.Tests/ProductSmoke/ParameterizedVisualWorldProfilesProductSmokeTests.cs",
                ".llmgc/procedural/goal-090-parameterized-visual-world-profiles/",
                "docs/agent-tasks/goal-090-parameterized-visual-world-profiles/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = ParameterizedVisualWorldProfilesValidator.SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualWorldProfileDiagnostic.Error(code, target, message));
        }
    }
}
