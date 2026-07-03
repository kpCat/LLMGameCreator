using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

public static class ParameterizedVisualWorldProfilesValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.Compiled);

    public static VisualWorldProfileValidationResult Validate(VisualWorldProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var diagnostics = new List<VisualWorldProfileDiagnostic>();

        ValidateId(profile.ProfileId, "visual_world.profile_id.invalid", profile.ProfileId, "Profile id must be stable lowercase metadata.", diagnostics);
        ValidateRequired(profile.WorldSeed, "visual_world.seed.missing", profile.ProfileId, "World seed is required.", diagnostics);
        ValidateRequired(profile.GeneratorVersion, "visual_world.generator_version.missing", profile.ProfileId, "Generator version is required.", diagnostics);
        ValidateRequired(profile.CoordinateOrigin, "visual_world.coordinate_origin.missing", profile.ProfileId, "Coordinate origin is required.", diagnostics);
        ValidateRelativePath(profile.OutputRelativeDirectory, "visual_world.path.absolute", profile.ProfileId, "Output path must be relative and safe.", diagnostics);

        if (profile.PromptTextIsSourceOfTruth
            || string.Equals(profile.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase)
            || string.Equals(profile.SourceOfTruthKind, "prompt", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_world.prompt.source_of_truth", profile.ProfileId, "Prompt text must not be profile source of truth."));
        }

        if (profile.ClaimsGenericButUsesFixedSizeAllowlist || profile.FixedSizeAllowlist.Count > 0)
        {
            diagnostics.Add(Error("visual_world.fixed_size_only.forbidden", profile.ProfileId, "Generic profiles must not be backed by a fixed-size allowlist."));
        }

        if (profile.RequiresSurfaceUndergroundOnly)
        {
            diagnostics.Add(Error("visual_world.surface_underground_only.forbidden", profile.ProfileId, "Layer validation must be data-driven and not require only surface plus underground."));
        }

        ValidateFiniteAndInfiniteMode(profile, diagnostics);
        ValidateLayers(profile, diagnostics);
        ValidateChunkAndPatchProfiles(profile, diagnostics);
        ValidateSparseAndStreamWindows(profile, diagnostics);
        ValidateChunkSamples(profile, diagnostics);
        ValidateLayerLinks(profile, diagnostics);
        ValidateRatingMetadata(profile, diagnostics);
        ValidateSourceLineage(profile, diagnostics);

        return new VisualWorldProfileValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static VisualWorldProfileValidationResult ValidateFiniteSizeSample(
        VisualWorldProfile profile,
        VisualRegionSize size)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(size);
        var diagnostics = new List<VisualWorldProfileDiagnostic>();

        ValidateId(size.SizeId, "visual_world.size_id.invalid", size.SizeId, "Size sample id must be stable lowercase metadata.", diagnostics);
        ValidateDimension(size.Width, profile.ValidationBounds.MinimumWidth, profile.ValidationBounds.MaximumWidth, "visual_world.dimension.invalid", size.SizeId, "Finite width is outside validation bounds.", diagnostics);
        ValidateDimension(size.Height, profile.ValidationBounds.MinimumHeight, profile.ValidationBounds.MaximumHeight, "visual_world.dimension.invalid", size.SizeId, "Finite height is outside validation bounds.", diagnostics);
        if (size.LayerCount != profile.Layers.Count)
        {
            diagnostics.Add(Error("visual_world.size.layer_count_mismatch", size.SizeId, "Size sample layer count must match the data-driven layer set."));
        }

        if (profile.ChunkProfile.ChunkWidth <= 0 || profile.ChunkProfile.ChunkHeight <= 0)
        {
            diagnostics.Add(Error("visual_world.chunk_size.invalid", profile.ProfileId, "Chunk dimensions must be positive before validating finite sizes."));
        }

        return new VisualWorldProfileValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static VisualChunkKey CreateChunkKey(VisualWorldProfile profile, VisualChunkAddress address) =>
        CreateChunkKey(profile.ProfileId, profile.WorldSeed, profile.GeneratorVersion, address.LayerId, address.ChunkX, address.ChunkY);

    public static VisualChunkKey CreateChunkKey(
        string profileId,
        string worldSeed,
        string generatorVersion,
        string layerId,
        long chunkX,
        long chunkY)
    {
        var payload = string.Join("|", profileId, worldSeed, generatorVersion, layerId, chunkX.ToString(System.Globalization.CultureInfo.InvariantCulture), chunkY.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return new VisualChunkKey
        {
            ProfileId = profileId,
            WorldSeed = worldSeed,
            GeneratorVersion = generatorVersion,
            LayerId = layerId,
            ChunkX = chunkX,
            ChunkY = chunkY,
            Key = ParameterizedVisualWorldProfilesHash.Compute(payload)
        };
    }

    public static IReadOnlyList<VisualWorldProfileDiagnostic> SortDiagnostics(IEnumerable<VisualWorldProfileDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

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

    public static long? EstimateFiniteChunkCapacity(VisualWorldProfile profile)
    {
        if (profile.IsInfinite || profile.FiniteWidth is not { } width || profile.FiniteHeight is not { } height)
        {
            return null;
        }

        if (profile.ChunkProfile.ChunkWidth <= 0 || profile.ChunkProfile.ChunkHeight <= 0)
        {
            return null;
        }

        var columns = CeilingDivide(width, profile.ChunkProfile.ChunkWidth);
        var rows = CeilingDivide(height, profile.ChunkProfile.ChunkHeight);
        return columns * rows * Math.Max(1, profile.Layers.Count);
    }

    private static void ValidateFiniteAndInfiniteMode(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (profile.IsInfinite)
        {
            if (!profile.VirtualBounds.IsInfinite)
            {
                diagnostics.Add(Error("visual_world.infinite.bounds_invalid", profile.ProfileId, "Infinite profiles must declare infinite virtual bounds."));
            }

            if (profile.FiniteWidth.HasValue || profile.FiniteHeight.HasValue || profile.LogicalCellCount.HasValue)
            {
                diagnostics.Add(Error("visual_world.infinite.finite_materialization", profile.ProfileId, "Infinite profiles must not declare finite dimensions or finite raw logical cell counts."));
            }

            if (profile.RawCellDumpAllowed)
            {
                diagnostics.Add(Error("visual_world.raw_cell_dump.forbidden", profile.ProfileId, "Infinite profiles must not allow raw cell dumps."));
            }

            return;
        }

        if (!profile.FiniteWidth.HasValue || !profile.FiniteHeight.HasValue)
        {
            diagnostics.Add(Error("visual_world.dimension.missing", profile.ProfileId, "Finite profiles require finite width and height."));
            return;
        }

        ValidateDimension(profile.FiniteWidth.Value, profile.ValidationBounds.MinimumWidth, profile.ValidationBounds.MaximumWidth, "visual_world.dimension.invalid", profile.ProfileId, "Finite width is outside validation bounds.", diagnostics);
        ValidateDimension(profile.FiniteHeight.Value, profile.ValidationBounds.MinimumHeight, profile.ValidationBounds.MaximumHeight, "visual_world.dimension.invalid", profile.ProfileId, "Finite height is outside validation bounds.", diagnostics);

        var expectedLogicalCells = (long)profile.FiniteWidth.Value * profile.FiniteHeight.Value * Math.Max(1, profile.Layers.Count);
        if (profile.LogicalCellCount != expectedLogicalCells)
        {
            diagnostics.Add(Error("visual_world.logical_cell_count.invalid", profile.ProfileId, "Logical cell count must be a computed summary from dimensions and layers."));
        }

        if (expectedLogicalCells > ParameterizedVisualWorldProfilesVocabulary.RawDumpLogicalCellThreshold && profile.RawCellDumpAllowed)
        {
            diagnostics.Add(Error("visual_world.raw_cell_dump.forbidden", profile.ProfileId, "Huge finite profiles must not allow raw cell dumps."));
        }
    }

    private static void ValidateLayers(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (profile.Layers.Count == 0)
        {
            diagnostics.Add(Error("visual_world.layers.missing", profile.ProfileId, "At least one data-driven layer is required."));
            return;
        }

        foreach (var duplicate in profile.Layers.GroupBy(item => item.LayerId, StringComparer.Ordinal).Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error("visual_world.layer_id.duplicate", duplicate.Key, "Layer ids must be unique."));
        }

        foreach (var layer in profile.Layers.OrderBy(item => item.Order).ThenBy(item => item.LayerId, StringComparer.Ordinal))
        {
            ValidateId(layer.LayerId, "visual_world.layer_id.invalid", layer.LayerId, "Layer id must be stable lowercase metadata.", diagnostics);
            ValidateRequired(layer.LayerKind, "visual_world.layer_kind.missing", layer.LayerId, "Layer kind is required.", diagnostics);
            ValidateRequired(layer.MaterializationRole, "visual_world.layer_role.missing", layer.LayerId, "Layer materialization role is required.", diagnostics);
        }
    }

    private static void ValidateChunkAndPatchProfiles(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (profile.ChunkProfile.ChunkWidth <= 0 || profile.ChunkProfile.ChunkHeight <= 0)
        {
            diagnostics.Add(Error("visual_world.chunk_size.invalid", profile.ProfileId, "Chunk width and height must be positive."));
        }

        if (profile.PatchProfile.PatchWidth <= 0 || profile.PatchProfile.PatchHeight <= 0)
        {
            diagnostics.Add(Error("visual_world.patch_size.invalid", profile.ProfileId, "Patch width and height must be positive."));
        }

        if (!profile.ChunkProfile.UsesDeterministicChunkKeys
            || !string.Equals(profile.ChunkProfile.DeterministicKeyFormula, ParameterizedVisualWorldProfilesVocabulary.DeterministicChunkKeyFormula, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("visual_world.chunk_key.nondeterministic", profile.ProfileId, "Chunk keys must use the deterministic profile/seed/version/layer/chunk formula."));
        }

        if (profile.ChunkProfile.RequiresPatchAlignment
            && profile.ChunkProfile.ChunkWidth > 0
            && profile.ChunkProfile.ChunkHeight > 0
            && profile.PatchProfile.PatchWidth > 0
            && profile.PatchProfile.PatchHeight > 0
            && (profile.ChunkProfile.ChunkWidth % profile.PatchProfile.PatchWidth != 0
                || profile.ChunkProfile.ChunkHeight % profile.PatchProfile.PatchHeight != 0))
        {
            diagnostics.Add(Error("visual_world.patch_chunk.incompatible", profile.ProfileId, "Patch dimensions must divide chunk dimensions when alignment is required."));
        }
    }

    private static void ValidateSparseAndStreamWindows(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        var hugeOrInfinite = profile.IsInfinite
            || profile.Mode == VisualWorldProfileMode.HugeSparseFinite
            || (profile.LogicalCellCount.HasValue
                && profile.LogicalCellCount.Value > ParameterizedVisualWorldProfilesVocabulary.RawDumpLogicalCellThreshold);

        if (hugeOrInfinite)
        {
            if (!profile.SparseRegionIndex.SparseOnly)
            {
                diagnostics.Add(Error("visual_world.sparse_index.required", profile.ProfileId, "Huge and infinite profiles require sparse chunk indexes."));
            }

            if (profile.SparseRegionIndex.AttemptsRawCellDump || profile.RawCellDumpAllowed)
            {
                diagnostics.Add(Error("visual_world.raw_cell_dump.forbidden", profile.ProfileId, "Huge and infinite profiles must not attempt raw cell dumps."));
            }
        }

        if (profile.IsInfinite && profile.SparseRegionIndex.FiniteOnlyMaterialization)
        {
            diagnostics.Add(Error("visual_world.infinite.finite_materialization", profile.ProfileId, "Infinite profiles must not declare finite-only materialization."));
        }

        foreach (var window in profile.StreamWindows)
        {
            ValidateId(window.WindowId, "visual_world.stream_window_id.invalid", window.WindowId, "Stream window id must be stable lowercase metadata.", diagnostics);
            if (!window.CenterChunkX.HasValue || !window.CenterChunkY.HasValue || window.RadiusChunks < 0 || window.WindowChunkCount <= 0)
            {
                diagnostics.Add(Error("visual_world.stream_window.invalid", window.WindowId, "Stream windows require a center, non-negative radius and positive window size."));
            }
        }

        if (profile.IsInfinite && profile.StreamWindows.Count == 0)
        {
            diagnostics.Add(Error("visual_world.stream_window.missing", profile.ProfileId, "Infinite profiles require at least one stream window."));
        }
    }

    private static void ValidateChunkSamples(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        var layerIds = profile.Layers.Select(item => item.LayerId).ToHashSet(StringComparer.Ordinal);
        foreach (var sample in profile.SparseRegionIndex.MaterializedChunks)
        {
            ValidateId(sample.SampleId, "visual_world.chunk_sample_id.invalid", sample.SampleId, "Chunk sample id must be stable lowercase metadata.", diagnostics);
            if (!layerIds.Contains(sample.Address.LayerId))
            {
                diagnostics.Add(Error("visual_world.chunk_address.layer_unknown", sample.SampleId, "Chunk sample layer must exist in profile layers."));
                continue;
            }

            var expected = CreateChunkKey(profile, sample.Address);
            if (!string.Equals(sample.ChunkKey.Key, expected.Key, StringComparison.Ordinal)
                || !string.Equals(sample.ChunkKey.Formula, expected.Formula, StringComparison.Ordinal)
                || !string.Equals(sample.ChunkKey.LayerId, expected.LayerId, StringComparison.Ordinal)
                || sample.ChunkKey.ChunkX != expected.ChunkX
                || sample.ChunkKey.ChunkY != expected.ChunkY)
            {
                diagnostics.Add(Error("visual_world.chunk_key.nondeterministic", sample.SampleId, "Materialized chunk keys must match the deterministic chunk key formula."));
            }
        }
    }

    private static void ValidateLayerLinks(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        var layerIds = profile.Layers.Select(item => item.LayerId).ToHashSet(StringComparer.Ordinal);
        foreach (var link in profile.LayerLinks)
        {
            ValidateId(link.LinkId, "visual_world.layer_link_id.invalid", link.LinkId, "Layer link id must be stable lowercase metadata.", diagnostics);
            if (!layerIds.Contains(link.FromLayerId) || !layerIds.Contains(link.ToLayerId))
            {
                diagnostics.Add(Error("visual_world.layer_link.unknown_layer", link.LinkId, "Layer links must reference known layers."));
            }
        }
    }

    private static void ValidateRatingMetadata(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        foreach (var metadata in profile.RatingMetadata)
        {
            ValidateId(metadata.MetadataId, "visual_world.rating_metadata_id.invalid", metadata.MetadataId, "Rating metadata id must be stable lowercase metadata.", diagnostics);
            if (string.IsNullOrWhiteSpace(metadata.SafeFallbackRefId))
            {
                diagnostics.Add(Error("visual_world.rating.safe_fallback_missing", metadata.MetadataId, "Adult/rating metadata requires a safe fallback ref."));
            }
        }
    }

    private static void ValidateSourceLineage(
        VisualWorldProfile profile,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (!profile.SourceLineageGoalIds.Contains("goal_087", StringComparer.Ordinal)
            || !profile.SourceLineageGoalIds.Contains("goal_088", StringComparer.Ordinal))
        {
            diagnostics.Add(Error("visual_world.source_lineage.missing", profile.ProfileId, "Profile must trace to Goal087 and Goal088 visual lineage."));
        }
    }

    private static void ValidateDimension(
        int value,
        int minimum,
        int maximum,
        string code,
        string target,
        string message,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (value < minimum || value > maximum)
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateId(
        string id,
        string code,
        string target,
        string message,
        List<VisualWorldProfileDiagnostic> diagnostics)
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
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateRelativePath(
        string relativePath,
        string code,
        string target,
        string message,
        List<VisualWorldProfileDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || Path.IsPathRooted(relativePath)
            || relativePath.Contains(':', StringComparison.Ordinal)
            || relativePath.Contains('\\', StringComparison.Ordinal)
            || relativePath.Split('/').Any(segment => segment == ".." || string.IsNullOrWhiteSpace(segment)))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

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

    private static VisualWorldProfileDiagnostic Error(string code, string target, string message) =>
        VisualWorldProfileDiagnostic.Error(code, target, message);
}
