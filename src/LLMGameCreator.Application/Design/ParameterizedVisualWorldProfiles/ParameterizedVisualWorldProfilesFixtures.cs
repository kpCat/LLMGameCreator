namespace LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

public static class ParameterizedVisualWorldProfilesFixtures
{
    public static IReadOnlyList<VisualWorldProfile> BuildProfiles() =>
    [
        AddChunkSamples(BuildBenchmarkHeroesProfile(), BenchmarkSamples()),
        AddChunkSamples(BuildFiniteCustomSizeMatrixProfile(), FiniteMatrixSamples()),
        AddChunkSamples(BuildHugeSparseProfile(), HugeSparseSamples()),
        AddChunkSamples(BuildInfiniteStreamingProfile(), InfiniteSamples())
    ];

    public static VisualWorldProfile BuildBenchmarkHeroesProfile()
    {
        const int fixtureWidth = 144;
        const int fixtureHeight = 144;
        var layers = new[]
        {
            Layer("surface", "overworld_surface", 0, "benchmark_surface_visual_metadata"),
            Layer("underground", "subterranean_region", 1, "benchmark_underground_visual_metadata")
        };

        return new VisualWorldProfile
        {
            ProfileId = "benchmark_heroes_144x144_surface_underground",
            WorldSeed = "goal090-benchmark-seed",
            GeneratorVersion = "visual-profile-seam-v1",
            Mode = VisualWorldProfileMode.Finite,
            FiniteWidth = fixtureWidth,
            FiniteHeight = fixtureHeight,
            VirtualBounds = FiniteBounds(fixtureWidth, fixtureHeight),
            IsBenchmarkProfile = true,
            BenchmarkNote = "Fixture preserves Goal088 Heroes-scale coverage; it is not an architectural size limit.",
            ChunkProfile = new VisualChunkProfile { ChunkWidth = 24, ChunkHeight = 16 },
            PatchProfile = new VisualPatchProfile { PatchWidth = 12, PatchHeight = 8 },
            LogicalCellCount = (long)fixtureWidth * fixtureHeight * layers.Length,
            RawCellDumpAllowed = false,
            Layers = layers,
            StreamWindows = [Window("benchmark_center_window", 3, 4, 1, ["surface", "underground"])],
            LayerLinks =
            [
                Link("benchmark_surface_to_underground_gate", "surface", "underground", VisualLayerLinkKind.Portal)
            ],
            RatingMetadata = [],
            SourceLineageGoalIds = ["goal_087", "goal_088"]
        };
    }

    public static VisualWorldProfile BuildFiniteCustomSizeMatrixProfile()
    {
        const int fixtureWidth = 512;
        const int fixtureHeight = 384;
        var layers = new[]
        {
            Layer("terrain", "visual_terrain", 0, "finite_matrix_terrain_metadata"),
            Layer("interior", "local_interior_overlay", 1, "finite_matrix_interior_metadata"),
            Layer("weather_overlay", "weather_effect_overlay", 2, "finite_matrix_weather_metadata")
        };

        return new VisualWorldProfile
        {
            ProfileId = "finite_custom_sizes_matrix",
            WorldSeed = "goal090-finite-matrix-seed",
            GeneratorVersion = "visual-profile-seam-v1",
            Mode = VisualWorldProfileMode.Finite,
            FiniteWidth = fixtureWidth,
            FiniteHeight = fixtureHeight,
            VirtualBounds = FiniteBounds(fixtureWidth, fixtureHeight),
            ChunkProfile = new VisualChunkProfile { ChunkWidth = 32, ChunkHeight = 32 },
            PatchProfile = new VisualPatchProfile { PatchWidth = 8, PatchHeight = 8 },
            LogicalCellCount = (long)fixtureWidth * fixtureHeight * layers.Length,
            RawCellDumpAllowed = false,
            Layers = layers,
            FiniteSizeSamples =
            [
                Size("sample_1x1", 1, 1, layers.Length),
                Size("sample_17x31", 17, 31, layers.Length),
                Size("sample_64x96", 64, 96, layers.Length),
                Size("sample_144x144", 144, 144, layers.Length),
                Size("sample_255x257", 255, 257, layers.Length),
                Size("sample_512x384", 512, 384, layers.Length)
            ],
            StreamWindows = [Window("finite_matrix_center_window", 8, 6, 1, ["terrain", "interior", "weather_overlay"])],
            LayerLinks =
            [
                Link("terrain_to_interior_portal", "terrain", "interior", VisualLayerLinkKind.Portal),
                Link("weather_overlay_to_terrain", "weather_overlay", "terrain", VisualLayerLinkKind.Overlay)
            ],
            RatingMetadata = [],
            SourceLineageGoalIds = ["goal_087", "goal_088"]
        };
    }

    public static VisualWorldProfile BuildHugeSparseProfile()
    {
        const int fixtureWidth = 100000;
        const int fixtureHeight = 100000;
        var layers = new[]
        {
            Layer("surface", "overworld_surface", 0, "huge_sparse_surface_metadata"),
            Layer("underground", "subterranean_region", 1, "huge_sparse_underground_metadata"),
            Layer("underwater", "water_depth_region", 2, "huge_sparse_underwater_metadata")
        };

        return new VisualWorldProfile
        {
            ProfileId = "huge_sparse_100000x100000_multilayer",
            WorldSeed = "goal090-huge-sparse-seed",
            GeneratorVersion = "visual-profile-seam-v1",
            Mode = VisualWorldProfileMode.HugeSparseFinite,
            FiniteWidth = fixtureWidth,
            FiniteHeight = fixtureHeight,
            VirtualBounds = FiniteBounds(fixtureWidth, fixtureHeight),
            ChunkProfile = new VisualChunkProfile { ChunkWidth = 64, ChunkHeight = 64 },
            PatchProfile = new VisualPatchProfile { PatchWidth = 16, PatchHeight = 16 },
            LogicalCellCount = (long)fixtureWidth * fixtureHeight * layers.Length,
            RawCellDumpAllowed = false,
            Layers = layers,
            SparseRegionIndex = new VisualSparseRegionIndex
            {
                SparseOnly = true,
                AttemptsRawCellDump = false,
                FiniteOnlyMaterialization = false,
                AnchorIds = ["origin_anchor", "western_coast_anchor", "deep_gate_anchor", "underwater_sample_anchor"]
            },
            StreamWindows = [Window("huge_sparse_review_window", 0, 0, 1, ["surface", "underground", "underwater"])],
            LayerLinks =
            [
                Link("surface_to_underground_gate", "surface", "underground", VisualLayerLinkKind.Portal),
                Link("surface_to_underwater_port", "surface", "underwater", VisualLayerLinkKind.Transition)
            ],
            RatingMetadata =
            [
                new VisualRatingMetadata
                {
                    MetadataId = "huge_sparse_rating_safe_fallback",
                    RatingKind = "safe_public",
                    SafeFallbackRefId = "visual_safe_fallback/huge_sparse_public"
                }
            ],
            SourceLineageGoalIds = ["goal_087", "goal_088"]
        };
    }

    public static VisualWorldProfile BuildInfiniteStreamingProfile()
    {
        var layers = new[]
        {
            Layer("surface", "infinite_surface", 0, "infinite_surface_stream_metadata"),
            Layer("underground", "infinite_underground", 1, "infinite_underground_stream_metadata"),
            Layer("interior", "streamed_interior", 2, "infinite_interior_stream_metadata"),
            Layer("sky_overlay", "weather_sky_overlay", 3, "infinite_sky_overlay_metadata")
        };

        return new VisualWorldProfile
        {
            ProfileId = "infinite_streaming_world_multilayer",
            WorldSeed = "goal090-infinite-stream-seed",
            GeneratorVersion = "visual-profile-seam-v1",
            Mode = VisualWorldProfileMode.Infinite,
            VirtualBounds = new VisualVirtualWorldBounds { IsInfinite = true },
            IsInfinite = true,
            ChunkProfile = new VisualChunkProfile { ChunkWidth = 64, ChunkHeight = 64 },
            PatchProfile = new VisualPatchProfile { PatchWidth = 16, PatchHeight = 16 },
            LogicalCellCount = null,
            RawCellDumpAllowed = false,
            Layers = layers,
            SparseRegionIndex = new VisualSparseRegionIndex
            {
                SparseOnly = true,
                AttemptsRawCellDump = false,
                FiniteOnlyMaterialization = false,
                AnchorIds = ["player_spawn_window", "far_expedition_window"]
            },
            StreamWindows =
            [
                Window("player_spawn_window", 0, 0, 1, ["surface", "underground", "interior", "sky_overlay"]),
                Window("far_expedition_window", -320, 511, 1, ["surface", "underground", "interior", "sky_overlay"])
            ],
            LayerLinks =
            [
                Link("surface_to_underground_portal", "surface", "underground", VisualLayerLinkKind.Portal),
                Link("surface_to_interior_door", "surface", "interior", VisualLayerLinkKind.Portal),
                Link("sky_overlay_to_surface", "sky_overlay", "surface", VisualLayerLinkKind.Overlay)
            ],
            RatingMetadata =
            [
                new VisualRatingMetadata
                {
                    MetadataId = "infinite_rating_safe_fallback",
                    RatingKind = "safe_public",
                    SafeFallbackRefId = "visual_safe_fallback/infinite_public"
                }
            ],
            SourceLineageGoalIds = ["goal_087", "goal_088"]
        };
    }

    public static IReadOnlyList<VisualChunkAddress> BenchmarkSamples() =>
    [
        Address("surface", 0, 0),
        Address("surface", 3, 4),
        Address("underground", 2, 3)
    ];

    public static IReadOnlyList<VisualChunkAddress> FiniteMatrixSamples() =>
    [
        Address("terrain", 0, 0),
        Address("terrain", 7, 5),
        Address("interior", 2, 1),
        Address("weather_overlay", 7, 5)
    ];

    public static IReadOnlyList<VisualChunkAddress> HugeSparseSamples() =>
    [
        Address("surface", 0, 0),
        Address("surface", 1562, 1562),
        Address("underground", 48, 1024),
        Address("underwater", 1400, 12)
    ];

    public static IReadOnlyList<VisualChunkAddress> InfiniteSamples() =>
    [
        Address("surface", 0, 0),
        Address("surface", -320, 511),
        Address("underground", -320, 511),
        Address("interior", 1, -1),
        Address("sky_overlay", 0, 0)
    ];

    private static VisualWorldProfile AddChunkSamples(
        VisualWorldProfile profile,
        IReadOnlyList<VisualChunkAddress> addresses)
    {
        var samples = addresses
            .Select((address, index) => new VisualChunkSample
            {
                SampleId = $"sample_{index:00}_{address.LayerId}_{NormalizeCoordinate(address.ChunkX)}_{NormalizeCoordinate(address.ChunkY)}",
                Address = address,
                ChunkKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(profile, address),
                SampleRole = index == 0 ? "origin_or_primary_anchor" : "sparse_review_anchor"
            })
            .ToList();

        return profile with
        {
            SparseRegionIndex = profile.SparseRegionIndex with
            {
                MaterializedChunks = samples,
                SparseOnly = profile.SparseRegionIndex.SparseOnly || profile.Mode != VisualWorldProfileMode.Finite
            }
        };
    }

    private static VisualWorldLayerProfile Layer(string id, string kind, int order, string role) =>
        new()
        {
            LayerId = id,
            LayerKind = kind,
            Order = order,
            MaterializationRole = role,
            SafeFallbackRefId = $"visual_safe_fallback/{id}"
        };

    private static VisualRegionSize Size(string id, int width, int height, int layerCount) =>
        new()
        {
            SizeId = id,
            Width = width,
            Height = height,
            LayerCount = layerCount
        };

    private static VisualLayerLink Link(string id, string from, string to, VisualLayerLinkKind kind) =>
        new()
        {
            LinkId = id,
            FromLayerId = from,
            ToLayerId = to,
            LinkKind = kind
        };

    private static VisualChunkAddress Address(string layerId, long chunkX, long chunkY) =>
        new()
        {
            LayerId = layerId,
            ChunkX = chunkX,
            ChunkY = chunkY
        };

    private static VisualStreamWindow Window(
        string id,
        long centerX,
        long centerY,
        int radius,
        IReadOnlyList<string> layerIds)
    {
        var addresses = new List<VisualChunkAddress>();
        for (var y = centerY - radius; y <= centerY + radius; y++)
        {
            for (var x = centerX - radius; x <= centerX + radius; x++)
            {
                foreach (var layerId in layerIds.OrderBy(item => item, StringComparer.Ordinal))
                {
                    addresses.Add(Address(layerId, x, y));
                }
            }
        }

        return new VisualStreamWindow
        {
            WindowId = id,
            CenterChunkX = centerX,
            CenterChunkY = centerY,
            RadiusChunks = radius,
            WindowChunkCount = addresses.Count,
            SampledAddresses = addresses
        };
    }

    private static VisualVirtualWorldBounds FiniteBounds(int width, int height) =>
        new()
        {
            IsInfinite = false,
            MinimumX = 0,
            MinimumY = 0,
            MaximumX = width - 1L,
            MaximumY = height - 1L
        };

    private static string NormalizeCoordinate(long value) =>
        value < 0
            ? $"neg{Math.Abs(value)}"
            : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
