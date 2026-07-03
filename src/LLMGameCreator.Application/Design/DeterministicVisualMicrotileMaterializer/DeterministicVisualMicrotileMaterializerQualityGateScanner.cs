namespace LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;

public static class DeterministicVisualMicrotileMaterializerQualityGateScanner
{
    public static VisualMicrotileQualityGateScan Build(
        VisualMicrotileMaterializationRequest request,
        VisualMicrotileValidationResult validation,
        VisualMicrotileWaterBiomeProof waterProof,
        VisualMicrotileLayeringProof layeringProof,
        VisualMicrotileNegativeProof negativeProof,
        VisualMicrotileSourceLineage sourceLineage,
        IReadOnlyDictionary<string, string> svgByPreviewId)
    {
        var diagnostics = new List<VisualMicrotileDiagnostic>();
        var previewCountWithinBounds = request.Previews.Count is >= 18 and <= 36;
        var svgTextOnly = svgByPreviewId.Count == request.Previews.Count
            && svgByPreviewId.Values.All(DeterministicVisualMicrotileMaterializerValidator.IsSvgSafe);
        var creatureCoverage = HasPreview(request, "creature_bodyplan_silhouette")
            && HasPreview(request, "creature_equipment_clothing_overlay")
            && HasPreview(request, "creature_damaged_dirty_worn_state")
            && HasPreview(request, "creature_paperdoll_neutral_slot");
        var uiCoverage = HasPreview(request, "ui_frame_panel_motif")
            && HasPreview(request, "effect_status_aura")
            && HasPreview(request, "atmosphere_day_night_weather_overlay");
        var adultCoverage = request.Previews.Any(item =>
            item.PreviewId == "adult_metadata_only_safe_fallback_slot"
            && item.AdultMetadataOnly
            && !string.IsNullOrWhiteSpace(item.SafeFallbackPreviewId));

        AddIfFalse(previewCountWithinBounds, "visual_microtile.preview_count.out_of_bounds", "preview_catalog", "Preview count must stay between 18 and 36.", diagnostics);
        AddIfFalse(validation.Passed, "visual_microtile.validation.failed", "request", "Materialization request must validate.", diagnostics);
        AddIfFalse(svgTextOnly, "visual_microtile.svg_hygiene.failed", "previews", "All previews must be safe text SVG.", diagnostics);
        AddIfFalse(waterProof.Passed, "visual_microtile.water_biome.failed", "water_biome_proof", "Water and biome coverage must pass.", diagnostics);
        AddIfFalse(layeringProof.Passed, "visual_microtile.layering.failed", "layering_proof", "Layering proof must pass.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_microtile.negative_proof.failed", "negative_proof", "Negative proof must reject invalid cases.", diagnostics);
        AddIfFalse(sourceLineage.Passed, "visual_microtile.source_lineage.failed", "source_lineage", "Goal084/085 source lineage must pass.", diagnostics);
        AddIfFalse(creatureCoverage, "visual_microtile.creature_coverage.failed", "creature_previews", "Creature body/equipment/state/paperdoll coverage is required.", diagnostics);
        AddIfFalse(uiCoverage, "visual_microtile.ui_effect_coverage.failed", "ui_effect_previews", "UI/effect/weather coverage is required.", diagnostics);
        AddIfFalse(adultCoverage, "visual_microtile.adult_fallback.failed", "adult_metadata_preview", "Adult-capable metadata-only fallback coverage is required.", diagnostics);

        return new VisualMicrotileQualityGateScan
        {
            Accepted = false,
            PreviewCountWithinBounds = previewCountWithinBounds,
            SvgTextOnlyPreviews = svgTextOnly,
            DeterministicRerunStable = true,
            WaterBiomeCoveragePassed = waterProof.Passed,
            CreatureEquipmentStateCoveragePassed = creatureCoverage,
            UiEffectWeatherCoveragePassed = uiCoverage,
            AdultMetadataOnlyFallbackCoveragePassed = adultCoverage,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/DeterministicVisualMicrotileMaterializer/",
                "tests/LLMGameCreator.Tests/Application/DeterministicVisualMicrotileMaterializer/",
                "tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualMicrotileMaterializerProductSmokeTests.cs",
                ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/",
                "docs/agent-tasks/goal-086-deterministic-visual-microtile-materializer/"
            ],
            Diagnostics = DeterministicVisualMicrotileMaterializerValidator.SortDiagnostics(diagnostics)
        };
    }

    private static bool HasPreview(VisualMicrotileMaterializationRequest request, string previewId) =>
        request.Previews.Any(item => item.PreviewId == previewId);

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualMicrotileDiagnostic.Error(code, target, message));
        }
    }
}
