using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public static class DeterministicVisualChunkStreamWindowFixtures
{
    public const string FiniteFixtureId = "finite_custom_255x257_surface_window";
    public const string HugeSparseFixtureId = "huge_sparse_100000x100000_surface_window";
    public const string InfiniteFixtureId = "infinite_streaming_multilayer_window";
    public const string LayerTransitionFixtureId = "layer_transition_window_surface_underground_water";

    public static IReadOnlyList<VisualChunkStreamRequest> BuildRequests()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var finite = profiles.Single(item => item.ProfileId == "finite_custom_sizes_matrix");
        var huge = profiles.Single(item => item.ProfileId == "huge_sparse_100000x100000_multilayer");
        var infinite = profiles.Single(item => item.ProfileId == "infinite_streaming_world_multilayer");

        return
        [
            new VisualChunkStreamRequest
            {
                FixtureId = FiniteFixtureId,
                WindowId = "finite_255x257_clipped_origin_radius2",
                ProfileId = finite.ProfileId,
                LayerId = "terrain",
                LayerIds = ["terrain"],
                WorldSeed = finite.WorldSeed,
                GeneratorVersion = finite.GeneratorVersion,
                Mode = VisualChunkStreamWorldMode.Finite,
                CenterChunkX = 0,
                CenterChunkY = 0,
                RadiusChunks = 2,
                BoundaryPolicy = VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds,
                FiniteWidthOverride = 255,
                FiniteHeightOverride = 257,
                FiniteSizeId = "sample_255x257",
                SafeFallbackRefId = "visual_safe_fallback/terrain"
            },
            new VisualChunkStreamRequest
            {
                FixtureId = HugeSparseFixtureId,
                WindowId = "huge_sparse_far_surface_radius1",
                ProfileId = huge.ProfileId,
                LayerId = "surface",
                LayerIds = ["surface"],
                WorldSeed = huge.WorldSeed,
                GeneratorVersion = huge.GeneratorVersion,
                Mode = VisualChunkStreamWorldMode.HugeSparseFinite,
                CenterChunkX = 1561,
                CenterChunkY = 1561,
                RadiusChunks = 1,
                BoundaryPolicy = VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds,
                SafeFallbackRefId = "visual_safe_fallback/huge_sparse_public"
            },
            new VisualChunkStreamRequest
            {
                FixtureId = InfiniteFixtureId,
                WindowId = "infinite_player_spawn_radius1",
                ProfileId = infinite.ProfileId,
                LayerId = "surface",
                LayerIds = ["surface", "underground", "interior", "sky_overlay"],
                WorldSeed = infinite.WorldSeed,
                GeneratorVersion = infinite.GeneratorVersion,
                Mode = VisualChunkStreamWorldMode.Infinite,
                CenterChunkX = 0,
                CenterChunkY = 0,
                RadiusChunks = 1,
                BoundaryPolicy = VisualChunkStreamBoundaryPolicy.UnboundedInfinite,
                ContainsAdultRatingMetadata = true,
                SafeFallbackRefId = "visual_safe_fallback/infinite_public",
                DeltaOverlay = Overlay("infinite_spawn_weather_overlay", "metadata-only weather overlay for spawn stream window", 4)
            },
            new VisualChunkStreamRequest
            {
                FixtureId = InfiniteFixtureId,
                WindowId = "infinite_east_camera_radius1",
                ProfileId = infinite.ProfileId,
                LayerId = "surface",
                LayerIds = ["surface", "underground", "interior", "sky_overlay"],
                WorldSeed = infinite.WorldSeed,
                GeneratorVersion = infinite.GeneratorVersion,
                Mode = VisualChunkStreamWorldMode.Infinite,
                CenterChunkX = 1,
                CenterChunkY = 0,
                RadiusChunks = 1,
                BoundaryPolicy = VisualChunkStreamBoundaryPolicy.UnboundedInfinite,
                ContainsAdultRatingMetadata = true,
                SafeFallbackRefId = "visual_safe_fallback/infinite_public",
                DeltaOverlay = Overlay("infinite_camera_delta_overlay", "metadata-only camera delta overlay; no raw cells", 8)
            },
            new VisualChunkStreamRequest
            {
                FixtureId = LayerTransitionFixtureId,
                WindowId = "surface_underground_underwater_transition_radius1",
                ProfileId = huge.ProfileId,
                LayerId = "surface",
                LayerIds = ["surface", "underground", "underwater"],
                WorldSeed = huge.WorldSeed,
                GeneratorVersion = huge.GeneratorVersion,
                Mode = VisualChunkStreamWorldMode.HugeSparseFinite,
                CenterChunkX = 24,
                CenterChunkY = 32,
                RadiusChunks = 1,
                BoundaryPolicy = VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds,
                ContainsAdultRatingMetadata = true,
                SafeFallbackRefId = "visual_safe_fallback/huge_sparse_public",
                DeltaOverlay = Overlay("surface_underwater_portal_overlay", "portal/link summary only; no hardcoded layer pair", 6)
            }
        ];
    }

    private static VisualChunkStreamDeltaOverlay Overlay(string id, string summary, int changedChunkCount) =>
        new()
        {
            OverlayId = id,
            Summary = summary,
            ChangedChunkCount = changedChunkCount,
            ContainsRawCellPayload = false,
            StableHash = DeterministicVisualChunkStreamWindowHash.Compute($"{id}|{summary}|{changedChunkCount}")
        };
}
