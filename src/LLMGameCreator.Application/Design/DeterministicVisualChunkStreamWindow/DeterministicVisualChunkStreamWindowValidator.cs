using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public static class DeterministicVisualChunkStreamWindowValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.Compiled);

    public static VisualChunkStreamValidationResult ValidateRequest(
        VisualChunkStreamRequest request,
        IReadOnlyList<VisualWorldProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(profiles);
        var diagnostics = new List<VisualChunkStreamDiagnostic>();

        ValidateStableId(request.FixtureId, "visual_chunk_stream.fixture_id.invalid", request.FixtureId, "Fixture id must be stable lowercase metadata.", diagnostics);
        ValidateStableId(request.WindowId, "visual_chunk_stream.window_id.invalid", request.WindowId, "Window id must be stable lowercase metadata.", diagnostics);
        ValidateRequired(request.ProfileId, "visual_chunk_stream.profile.missing", request.FixtureId, "Profile id is required.", diagnostics);
        ValidateRequired(request.LayerId, "visual_chunk_stream.layer.missing", request.FixtureId, "Primary layer id is required.", diagnostics);
        ValidateRequired(request.WorldSeed, "visual_chunk_stream.seed.missing", request.FixtureId, "World seed is required.", diagnostics);
        ValidateRequired(request.GeneratorVersion, "visual_chunk_stream.generator_version.missing", request.FixtureId, "Generator version is required.", diagnostics);

        if (request.RadiusChunks < 0 || request.RadiusChunks > DeterministicVisualChunkStreamWindowVocabulary.MaximumRadiusChunks)
        {
            diagnostics.Add(Error("visual_chunk_stream.radius.invalid", request.WindowId, "Radius must be in the supported deterministic proof range."));
        }

        if (request.AttemptsRawFullWorldDump)
        {
            diagnostics.Add(Error("visual_chunk_stream.raw_full_world_dump.forbidden", request.WindowId, "Stream windows must not request raw full-world cell dumps."));
        }

        if (request.ContainsAbsolutePath)
        {
            diagnostics.Add(Error("visual_chunk_stream.path.absolute", request.WindowId, "Stream evidence must not use absolute paths as data."));
        }

        if (request.PromptTextIsSourceOfTruth
            || string.Equals(request.SourceOfTruthKind, "prompt", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_chunk_stream.prompt.source_of_truth", request.WindowId, "Prompt text must not be stream-window source of truth."));
        }

        if (request.ContainsAdultRatingMetadata && string.IsNullOrWhiteSpace(request.SafeFallbackRefId))
        {
            diagnostics.Add(Error("visual_chunk_stream.rating.safe_fallback_missing", request.WindowId, "Adult/rating metadata requires a safe fallback ref."));
        }

        var profile = profiles.SingleOrDefault(item => item.ProfileId == request.ProfileId);
        if (profile == null)
        {
            diagnostics.Add(Error("visual_chunk_stream.profile.unknown", request.ProfileId, "Requested profile must exist in Goal090 profile fixtures."));
            return Result(diagnostics);
        }

        if (MapMode(profile) != request.Mode)
        {
            diagnostics.Add(Error("visual_chunk_stream.profile_mode.mismatch", request.WindowId, "Request mode must match the Goal090 profile mode."));
        }

        var requestedLayers = request.LayerIds.Count == 0 ? [request.LayerId] : request.LayerIds;
        var profileLayerIds = profile.Layers.Select(item => item.LayerId).ToHashSet(StringComparer.Ordinal);
        if (!requestedLayers.Contains(request.LayerId, StringComparer.Ordinal))
        {
            diagnostics.Add(Error("visual_chunk_stream.layer.primary_missing", request.WindowId, "Primary layer must be included in requested layers."));
        }

        foreach (var layerId in requestedLayers)
        {
            if (!profileLayerIds.Contains(layerId))
            {
                diagnostics.Add(Error("visual_chunk_stream.layer.unknown", layerId, "Requested layer must exist in the Goal090 profile."));
            }
        }

        var duplicateLayers = requestedLayers.GroupBy(item => item, StringComparer.Ordinal).Where(item => item.Count() > 1);
        foreach (var duplicate in duplicateLayers)
        {
            diagnostics.Add(Error("visual_chunk_stream.layer.duplicate", duplicate.Key, "Requested layers must be unique."));
        }

        ValidateFiniteWindowBounds(request, profile, diagnostics);
        ValidateDeltaOverlay(request, diagnostics);

        return Result(diagnostics);
    }

    public static VisualChunkStreamValidationResult ValidateWindow(VisualChunkStreamWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var diagnostics = new List<VisualChunkStreamDiagnostic>();

        if (window.ChunkCount != window.Chunks.Count || window.ChunkCount == 0)
        {
            diagnostics.Add(Error("visual_chunk_stream.chunk_count.invalid", window.WindowId, "Materialized chunk count must match chunk refs and be non-zero."));
        }

        if (window.AttemptsRawFullWorldDump || !window.NoRawFullWorldDump || window.Chunks.Count > DeterministicVisualChunkStreamWindowVocabulary.RawDumpChunkThreshold)
        {
            diagnostics.Add(Error("visual_chunk_stream.raw_full_world_dump.forbidden", window.WindowId, "Materialized stream windows must stay compact and not dump raw world cells."));
        }

        if (window.ContainsAbsolutePath)
        {
            diagnostics.Add(Error("visual_chunk_stream.path.absolute", window.WindowId, "Materialized evidence must not carry absolute paths."));
        }

        if (window.PromptTextIsSourceOfTruth
            || string.Equals(window.SourceOfTruthKind, "prompt", StringComparison.OrdinalIgnoreCase)
            || string.Equals(window.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_chunk_stream.prompt.source_of_truth", window.WindowId, "Prompt text must not be materialized source of truth."));
        }

        if (window.ContainsAdultRatingMetadata && string.IsNullOrWhiteSpace(window.SafeFallbackRefId))
        {
            diagnostics.Add(Error("visual_chunk_stream.rating.safe_fallback_missing", window.WindowId, "Adult/rating metadata requires safe fallback evidence."));
        }

        foreach (var duplicate in window.Chunks.GroupBy(item => item.ChunkKey, StringComparer.Ordinal).Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error("visual_chunk_stream.chunk_key.duplicate", duplicate.Key, "Chunk keys must be unique inside one materialized window."));
        }

        foreach (var chunk in window.Chunks)
        {
            var expected = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(
                window.ProfileId,
                window.WorldSeed,
                window.GeneratorVersion,
                chunk.LayerId,
                chunk.ChunkX,
                chunk.ChunkY);
            if (!string.Equals(chunk.ChunkKey, expected.Key, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.chunk_key.mismatch", chunk.ChunkKey, "Chunk key must match deterministic profile/seed/version/layer/chunk formula."));
            }

            if (!string.Equals(chunk.NeighborSeamKeys.North, ExpectedSeamKey(window, chunk, "y", chunk.ChunkY, chunk.ChunkX), StringComparison.Ordinal)
                || !string.Equals(chunk.NeighborSeamKeys.South, ExpectedSeamKey(window, chunk, "y", chunk.ChunkY + 1, chunk.ChunkX), StringComparison.Ordinal)
                || !string.Equals(chunk.NeighborSeamKeys.West, ExpectedSeamKey(window, chunk, "x", chunk.ChunkX, chunk.ChunkY), StringComparison.Ordinal)
                || !string.Equals(chunk.NeighborSeamKeys.East, ExpectedSeamKey(window, chunk, "x", chunk.ChunkX + 1, chunk.ChunkY), StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.seam_key.mismatch", chunk.ChunkKey, "Neighbor seam keys must be deterministic from profile/seed/version/layer/edge."));
            }
        }

        foreach (var seam in window.Seams)
        {
            var from = window.Chunks.FirstOrDefault(item => item.ChunkKey == seam.FromChunkKey);
            var to = window.Chunks.FirstOrDefault(item => item.ChunkKey == seam.ToChunkKey);
            if (from == null || to == null)
            {
                diagnostics.Add(Error("visual_chunk_stream.seam.chunk_missing", seam.SeamKey, "Seams must reference materialized chunks."));
                continue;
            }

            var expectedKey = seam.Direction == "east" ? from.NeighborSeamKeys.East : from.NeighborSeamKeys.South;
            var reverseKey = seam.Direction == "east" ? to.NeighborSeamKeys.West : to.NeighborSeamKeys.North;
            if (!string.Equals(seam.SeamKey, expectedKey, StringComparison.Ordinal)
                || !string.Equals(seam.SeamKey, reverseKey, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.seam_key.mismatch", seam.SeamKey, "Adjacent chunks must share the same seam key."));
            }

            if (!seam.WaterContinuityPassed || !seam.RoadContinuityPassed || !seam.BiomeContinuityPassed)
            {
                diagnostics.Add(Error("visual_chunk_stream.seam_continuity.mismatch", seam.SeamKey, "Water, road and biome seam continuity must pass."));
            }

            if (!string.Equals(seam.WaterConnector, DeterministicVisualChunkStreamWindowMaterializer.ConnectorFromSeam(seam.SeamKey, "water"), StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.water_connector.mismatch", seam.SeamKey, "Water connector must be derived from the seam key."));
            }

            if (!string.Equals(seam.RoadConnector, DeterministicVisualChunkStreamWindowMaterializer.ConnectorFromSeam(seam.SeamKey, "road"), StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.road_connector.mismatch", seam.SeamKey, "Road connector must be derived from the seam key."));
            }

            if (!string.Equals(seam.BiomeBand, DeterministicVisualChunkStreamWindowMaterializer.ConnectorFromSeam(seam.SeamKey, "biome"), StringComparison.Ordinal))
            {
                diagnostics.Add(Error("visual_chunk_stream.biome_band.mismatch", seam.SeamKey, "Biome continuity band must be derived from the seam key."));
            }
        }

        foreach (var overlay in window.DeltaOverlays)
        {
            if (overlay.ContainsRawCellPayload)
            {
                diagnostics.Add(Error("visual_chunk_stream.delta_overlay.raw_payload", overlay.OverlayId, "Delta overlays must be compact summaries, not raw cell payloads."));
            }
        }

        return Result(diagnostics);
    }

    public static bool IsSvgSafe(string svg) =>
        !string.IsNullOrWhiteSpace(svg)
        && svg.Contains("<svg", StringComparison.Ordinal)
        && svg.Contains("viewBox=", StringComparison.Ordinal)
        && !svg.Contains("<script", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("https://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("xlink:href", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains(" href=", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("data:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("base64", StringComparison.OrdinalIgnoreCase);

    public static int CountSvgRects(string svg) => Count(svg, "<rect ");

    public static IReadOnlyList<VisualChunkStreamDiagnostic> SortDiagnostics(IEnumerable<VisualChunkStreamDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void ValidateFiniteWindowBounds(
        VisualChunkStreamRequest request,
        VisualWorldProfile profile,
        List<VisualChunkStreamDiagnostic> diagnostics)
    {
        if (request.Mode == VisualChunkStreamWorldMode.Infinite)
        {
            return;
        }

        var finiteWidth = request.FiniteWidthOverride ?? profile.FiniteWidth;
        var finiteHeight = request.FiniteHeightOverride ?? profile.FiniteHeight;
        if (!finiteWidth.HasValue || !finiteHeight.HasValue)
        {
            diagnostics.Add(Error("visual_chunk_stream.finite_bounds.missing", request.WindowId, "Finite stream windows require finite profile bounds."));
            return;
        }

        if (profile.ChunkProfile.ChunkWidth <= 0 || profile.ChunkProfile.ChunkHeight <= 0)
        {
            diagnostics.Add(Error("visual_chunk_stream.chunk_size.invalid", request.ProfileId, "Profile chunk dimensions must be positive."));
            return;
        }

        var chunkColumns = CeilingDivide(finiteWidth.Value, profile.ChunkProfile.ChunkWidth);
        var chunkRows = CeilingDivide(finiteHeight.Value, profile.ChunkProfile.ChunkHeight);
        var minX = request.CenterChunkX - request.RadiusChunks;
        var minY = request.CenterChunkY - request.RadiusChunks;
        var maxX = request.CenterChunkX + request.RadiusChunks;
        var maxY = request.CenterChunkY + request.RadiusChunks;
        var outOfBounds = minX < 0 || minY < 0 || maxX >= chunkColumns || maxY >= chunkRows;
        if (outOfBounds && request.BoundaryPolicy != VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds)
        {
            diagnostics.Add(Error("visual_chunk_stream.finite_clipping.required", request.WindowId, "Finite out-of-bounds windows require explicit clipping policy."));
        }
    }

    private static void ValidateDeltaOverlay(
        VisualChunkStreamRequest request,
        List<VisualChunkStreamDiagnostic> diagnostics)
    {
        if (request.DeltaOverlay == null)
        {
            return;
        }

        ValidateStableId(request.DeltaOverlay.OverlayId, "visual_chunk_stream.delta_overlay_id.invalid", request.DeltaOverlay.OverlayId, "Delta overlay id must be stable lowercase metadata.", diagnostics);
        if (request.DeltaOverlay.ContainsRawCellPayload)
        {
            diagnostics.Add(Error("visual_chunk_stream.delta_overlay.raw_payload", request.DeltaOverlay.OverlayId, "Delta overlays must be compact summaries."));
        }
    }

    private static string ExpectedSeamKey(VisualChunkStreamWindow window, VisualChunkStreamChunkRef chunk, string axis, long boundary, long lane) =>
        DeterministicVisualChunkStreamWindowMaterializer.EdgeSeamKey(
            window.ProfileId,
            window.WorldSeed,
            window.GeneratorVersion,
            chunk.LayerId,
            axis,
            boundary,
            lane);

    private static VisualChunkStreamWorldMode MapMode(VisualWorldProfile profile) =>
        profile.Mode switch
        {
            VisualWorldProfileMode.HugeSparseFinite => VisualChunkStreamWorldMode.HugeSparseFinite,
            VisualWorldProfileMode.Infinite => VisualChunkStreamWorldMode.Infinite,
            _ => VisualChunkStreamWorldMode.Finite
        };

    private static void ValidateStableId(
        string id,
        string code,
        string target,
        string message,
        List<VisualChunkStreamDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !StableIdRegex.IsMatch(id))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRequired(
        string value,
        string code,
        string target,
        string message,
        List<VisualChunkStreamDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static VisualChunkStreamValidationResult Result(List<VisualChunkStreamDiagnostic> diagnostics) =>
        new()
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static long CeilingDivide(long value, long divisor) => (value + divisor - 1) / divisor;

    private static int Count(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static VisualChunkStreamDiagnostic Error(string code, string target, string message) =>
        VisualChunkStreamDiagnostic.Error(code, target, message);
}
