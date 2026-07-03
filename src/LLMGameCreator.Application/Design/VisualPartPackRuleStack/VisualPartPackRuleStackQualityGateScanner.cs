namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public static class VisualPartPackRuleStackQualityGateScanner
{
    public static VisualPartPackQualityGateScan Build(
        VisualPartPackManifest manifest,
        VisualPartPackValidationMatrix validationMatrix,
        VisualPartPackNegativeProof negativeProof,
        DeepsearchLineageInventory deepsearchLineage,
        Goal084ContractBindingMatrix goal084Binding,
        WaterBiomeCoverageMatrix waterCoverage)
    {
        var diagnostics = new List<VisualRuleStackDiagnostic>();
        AddIfFalse(deepsearchLineage.Passed, "visual_part_pack.deepsearch_lineage.failed", "deepsearch_lineage", "All eight deepsearch docs must exist and be routed.", diagnostics);
        AddIfFalse(goal084Binding.Passed, "visual_part_pack.goal084_binding.failed", "goal084_binding", "Goal084 contract lineage and slot bindings must pass.", diagnostics);
        AddIfFalse(validationMatrix.Passed, "visual_part_pack.valid_fixtures.failed", "validation_matrix", "All required fixture packs must validate.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_part_pack.negative_proof.failed", "negative_proof", "Invalid unsafe/fake rule-stack cases must be rejected.", diagnostics);
        AddIfFalse(waterCoverage.Passed, "visual_part_pack.water_coverage.failed", "water_biome_coverage", "Water/coast/river/lake/marsh coverage must pass.", diagnostics);

        var creaturePack = manifest.PartPacks.First(item => item.PackId == "creature_bodyplan_equipment_part_pack");
        var uiPack = manifest.PartPacks.First(item => item.PackId == "ui_theme_icon_effect_part_pack");
        var adultPack = manifest.PartPacks.First(item => item.PackId == "adult_rating_gated_extension_metadata_only");
        var creatureCoverage = creaturePack.CreatureBodyPlanProfiles.Count >= 6
            && creaturePack.EquipmentOverlayProfiles.Count >= 4
            && creaturePack.BodyPlanGrammarCapacity >= 100
            && creaturePack.HandAuthoredSpeciesAssetCount == 0;
        var uiCoverage = uiPack.UiThemeProfiles.Any()
            && uiPack.EffectProfiles.Any(item => item.EffectKind == "weather")
            && uiPack.EffectProfiles.Any(item => item.EffectKind == "day_night")
            && uiPack.EffectProfiles.All(item => item.HasSafeFallback);
        var adultBoundary = adultPack.IsAdultRatingExtension
            && adultPack.ProviderState == VisualPartProviderState.CandidateQuarantine
            && adultPack.ReviewStatus == VisualPartReviewStatus.CandidateQuarantined
            && !string.IsNullOrWhiteSpace(adultPack.SafeFallbackPackId)
            && adultPack.Parts.All(item => item.RelativePath.EndsWith(".metadata.json", StringComparison.Ordinal));

        AddIfFalse(creatureCoverage, "visual_part_pack.creature_coverage.failed", creaturePack.PackId, "Creature body-plan/equipment grammar coverage must pass.", diagnostics);
        AddIfFalse(uiCoverage, "visual_part_pack.ui_effect_coverage.failed", uiPack.PackId, "UI/effect/weather/day-night coverage must pass.", diagnostics);
        AddIfFalse(adultBoundary, "visual_part_pack.adult_boundary.failed", adultPack.PackId, "Adult/rating extension must remain metadata-only, quarantined and fallback-bound.", diagnostics);

        return new VisualPartPackQualityGateScan
        {
            Accepted = false,
            AllDeepsearchDocsConsumed = deepsearchLineage.Passed,
            Goal084ContractLineagePassed = goal084Binding.Passed,
            ValidFixturesPassed = validationMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            WaterBiomeCoveragePassed = waterCoverage.Passed,
            CreatureBodyPlanEquipmentCoveragePassed = creatureCoverage,
            UiEffectWeatherDayNightCoveragePassed = uiCoverage,
            AdultMetadataOnlyFallbackBound = adultBoundary,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualPartPackRuleStack/",
                "tests/LLMGameCreator.Tests/Application/VisualPartPackRuleStack/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualPartPackRuleStackProductSmokeTests.cs",
                ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/",
                "docs/agent-tasks/goal-085-deepsearch-backed-visual-part-pack-rule-stack/"
            ],
            Diagnostics = VisualPartPackRuleStackValidator.SortDiagnostics(diagnostics)
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualRuleStackDiagnostic.Error(code, target, message));
        }
    }
}
