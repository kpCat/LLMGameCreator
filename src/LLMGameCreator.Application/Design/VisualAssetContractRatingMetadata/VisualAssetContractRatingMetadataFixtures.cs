using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;

public static class VisualAssetContractRatingMetadataFixtures
{
    public static readonly IReadOnlyList<string> RequiredFixtureIds =
    [
        "fantasy_overworld_tile_safe",
        "water_coast_biome_safe",
        "settlement_building_safe",
        "creature_bodyplan_safe",
        "humanoid_paperdoll_adult_capable_metadata_only",
        "tech_future_ui_panel_safe"
    ];

    public static VisualAssetContract BuildDefaultContract()
    {
        var recipeRefs = RequiredFixtureIds
            .Select(id => new VisualAssetRecipeRef
            {
                RecipeId = $"recipe/{id}/v1",
                ProvenanceRef = "docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md",
                GeneratorVersion = "goal084-metadata-v1"
            })
            .ToList();
        var partPackRefs = RequiredFixtureIds
            .Select(id => new VisualPartPackRef
            {
                PartPackId = $"part_pack/{id}/v1",
                ProvenanceRef = "docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md",
                GeneratorVersion = "goal084-metadata-v1"
            })
            .ToList();
        var slots = new List<VisualAssetSlot>
        {
            SafeSlot("fantasy_overworld_tile_safe", recipeRefs[0], partPackRefs[0], "terrain_tile"),
            SafeSlot("water_coast_biome_safe", recipeRefs[1], partPackRefs[1], "water_coast"),
            SafeSlot("settlement_building_safe", recipeRefs[2], partPackRefs[2], "settlement_building"),
            SafeSlot("creature_bodyplan_safe", recipeRefs[3], partPackRefs[3], "creature_bodyplan"),
            AdultCapableMetadataOnlySlot(recipeRefs[4], partPackRefs[4]),
            SafeSlot("tech_future_ui_panel_safe", recipeRefs[5], partPackRefs[5], "ui_panel")
        };

        return new VisualAssetContract
        {
            ContractId = "visual_asset_contract_rating_metadata_goal084",
            GeneratorVersion = "goal084-metadata-v1",
            StrictReferenceValidation = true,
            RecipeRefs = recipeRefs,
            PartPackRefs = partPackRefs,
            Slots = slots,
            CandidateRecords =
            [
                new VisualCandidateRecord
                {
                    CandidateId = "candidate/humanoid_paperdoll_adult_capable_metadata_only/metadata_only",
                    AssetSlot = "humanoid_paperdoll_adult_capable_metadata_only",
                    ProviderState = VisualProviderState.CandidateQuarantine,
                    CandidateQuarantine = true,
                    ReviewStatus = VisualReviewStatus.CandidateQuarantined,
                    PromotionRequested = false,
                    Rating = VisualRating.Suggestive,
                    ExportPolicy = VisualExportPolicy.MatureOptional,
                    ProvenanceRef = "docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md",
                    RelativePath = "visual/candidates/humanoid_paperdoll_adult_capable_metadata_only.metadata.json",
                    Sha256 = StableHash("candidate/humanoid_paperdoll_adult_capable_metadata_only/metadata_only"),
                    Seed = "goal084-seed-humanoid-paperdoll-adult-capable-metadata-only",
                    GeneratorVersion = "goal084-metadata-v1",
                    SourceOfTruthKind = "metadata_contract",
                    PromptTextIsSourceOfTruth = false
                }
            ]
        };
    }

    public static VisualRatingPolicyMatrix BuildRatingPolicyMatrix() =>
        new()
        {
            Rows =
            [
                Row(VisualRating.Safe, [VisualExportPolicy.PublicSafe, VisualExportPolicy.MatureOptional, VisualExportPolicy.AdultBuildOnly], false, true, false, "Safe-rated refs may be public when approved or backed by deterministic fallback."),
                Row(VisualRating.Suggestive, [VisualExportPolicy.MatureOptional, VisualExportPolicy.AdultBuildOnly], true, false, true, "Suggestive metadata is opt-in and requires fallback when adult-enabled."),
                Row(VisualRating.AdultNudeReference, [VisualExportPolicy.AdultBuildOnly, VisualExportPolicy.PrivateLocalOnly], true, false, true, "Adult reference metadata is not public export material."),
                Row(VisualRating.AdultEroticScene, [VisualExportPolicy.AdultBuildOnly, VisualExportPolicy.PrivateLocalOnly], true, false, true, "Adult scene metadata remains review-gated and fallback-backed."),
                Row(VisualRating.AdultPrivateExplicit, [VisualExportPolicy.PrivateLocalOnly], true, false, true, "Private explicit metadata never ships by default.")
            ]
        };

    private static VisualRatingPolicyRow Row(
        VisualRating rating,
        IReadOnlyList<VisualExportPolicy> policies,
        bool adultEnabledAllowed,
        bool publicExportAllowed,
        bool safeFallbackRequiredWhenAdultEnabled,
        string boundary) =>
        new()
        {
            Rating = rating,
            AllowedExportPolicies = policies,
            AdultEnabledAllowed = adultEnabledAllowed,
            PublicExportAllowed = publicExportAllowed,
            SafeFallbackRequiredWhenAdultEnabled = safeFallbackRequiredWhenAdultEnabled,
            Boundary = boundary
        };

    private static VisualAssetSlot SafeSlot(
        string id,
        VisualAssetRecipeRef recipe,
        VisualPartPackRef partPack,
        string provenanceKind) =>
        new()
        {
            AssetSlot = id,
            Rating = VisualRating.Safe,
            AdultEnabled = false,
            SafeFallbackRequired = false,
            CandidateQuarantine = false,
            ReviewStatus = VisualReviewStatus.ApprovedSafe,
            ExportPolicy = VisualExportPolicy.PublicSafe,
            ApprovedAssetRef = Approved(id),
            RecipeRef = recipe,
            PartPackRef = partPack,
            SafeFallbackRef = Fallback(id),
            ProvenanceRef = $"goal083/{provenanceKind}",
            RelativePath = $"visual/approved/{id}.metadata.json",
            Sha256 = StableHash($"slot/{id}"),
            Seed = $"goal084-seed-{id}",
            GeneratorVersion = "goal084-metadata-v1",
            BodyPlanEligibility = VisualBodyPlanEligibility.SafeOnly,
            BodyPlanEligibilityFacts = new VisualBodyPlanEligibilityFacts()
        };

    private static VisualAssetSlot AdultCapableMetadataOnlySlot(
        VisualAssetRecipeRef recipe,
        VisualPartPackRef partPack) =>
        new()
        {
            AssetSlot = "humanoid_paperdoll_adult_capable_metadata_only",
            Rating = VisualRating.Suggestive,
            AdultEnabled = true,
            SafeFallbackRequired = true,
            CandidateQuarantine = true,
            ReviewStatus = VisualReviewStatus.CandidateQuarantined,
            ExportPolicy = VisualExportPolicy.MatureOptional,
            ApprovedAssetRef = null,
            RecipeRef = recipe,
            PartPackRef = partPack,
            SafeFallbackRef = Fallback("humanoid_paperdoll_adult_capable_metadata_only"),
            ProvenanceRef = "docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md",
            RelativePath = "visual/metadata/humanoid_paperdoll_adult_capable_metadata_only.json",
            Sha256 = StableHash("slot/humanoid_paperdoll_adult_capable_metadata_only"),
            Seed = "goal084-seed-humanoid-paperdoll-adult-capable-metadata-only",
            GeneratorVersion = "goal084-metadata-v1",
            BodyPlanEligibility = VisualBodyPlanEligibility.AdultEligibleHumanoidSapient,
            BodyPlanEligibilityFacts = new VisualBodyPlanEligibilityFacts
            {
                AdultCharacter = true,
                AgeKnownAdult = true,
                Sapient = true,
                HumanoidCompatible = true
            }
        };

    private static VisualApprovedAssetRef Approved(string id) =>
        new()
        {
            AssetId = $"approved/{id}",
            RelativePath = $"visual/approved/{id}.metadata.json",
            Sha256 = StableHash($"approved/{id}"),
            ProvenanceRef = "goal084_fixture_metadata",
            Rating = VisualRating.Safe,
            ReviewStatus = VisualReviewStatus.ApprovedSafe,
            ExportPolicy = VisualExportPolicy.PublicSafe
        };

    private static VisualSafeFallbackRef Fallback(string id) =>
        new()
        {
            FallbackId = $"fallback/{id}/safe",
            RelativePath = $"visual/fallbacks/{id}.metadata.json",
            Sha256 = StableHash($"fallback/{id}/safe"),
            ProvenanceRef = "goal084_deterministic_fallback_metadata",
            Deterministic = true,
            Rating = VisualRating.Safe
        };

    internal static string StableHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
