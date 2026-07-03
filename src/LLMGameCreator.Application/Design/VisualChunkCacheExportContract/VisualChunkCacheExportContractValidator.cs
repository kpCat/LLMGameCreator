namespace LLMGameCreator.Application.Design.VisualChunkCacheExportContract;

public static class VisualChunkCacheExportContractValidator
{
    private static readonly HashSet<string> KnownInvalidationKeys = new(StringComparer.Ordinal)
    {
        "goal090_source_hash",
        "goal091_manifest_hash",
        "goal091_cache_reuse_hash",
        "generator_version",
        "profile_id",
        "world_seed",
        "layer_id",
        "chunk_hash",
        "stream_window_membership",
        "rating_safe_fallback",
        "delta_overlay_hash"
    };

    private static readonly HashSet<string> BinaryOrRasterMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    public static VisualChunkCacheValidationResult ValidateManifest(
        VisualChunkCacheExportManifest manifest,
        IReadOnlySet<string>? knownSourceChunkKeys = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var diagnostics = new List<VisualChunkCacheDiagnostic>();

        if (manifest.Accepted)
        {
            diagnostics.Add(Error("visual_chunk_cache.accepted.must_be_false", manifest.GoalId, "Goal 093 evidence must remain produced for review with accepted=false."));
        }

        if (manifest.PackageCount != manifest.Packages.Count || manifest.PackageCount < 4)
        {
            diagnostics.Add(Error("visual_chunk_cache.package_count.invalid", manifest.GoalId, "Manifest must contain all four required export packages."));
        }

        var requiredPackageIds = new[]
        {
            VisualChunkCacheExportContractVocabulary.FinitePackageId,
            VisualChunkCacheExportContractVocabulary.HugeSparsePackageId,
            VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId,
            VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId
        };
        foreach (var packageId in requiredPackageIds)
        {
            if (!manifest.Packages.Any(item => item.PackageId == packageId))
            {
                diagnostics.Add(Error("visual_chunk_cache.package.missing", packageId, "Required cache export package is missing."));
            }
        }

        var records = manifest.Packages.SelectMany(item => item.Records).ToList();
        if (manifest.ExportRecordCount != records.Count || records.Count == 0)
        {
            diagnostics.Add(Error("visual_chunk_cache.record_count.invalid", manifest.GoalId, "Export record count must match package records and be non-zero."));
        }

        foreach (var package in manifest.Packages)
        {
            ValidatePackage(package, knownSourceChunkKeys, diagnostics);
        }

        foreach (var group in records.GroupBy(item => item.CacheKey.ChunkKey, StringComparer.Ordinal))
        {
            var chunkHashes = group.Select(item => item.ChunkHash).Distinct(StringComparer.Ordinal).ToList();
            if (chunkHashes.Count > 1)
            {
                diagnostics.Add(Error("visual_chunk_cache.chunk_key.conflicting_hash", group.Key, "Duplicate chunk keys must not carry conflicting chunk hashes."));
            }
        }

        return Result(diagnostics);
    }

    public static VisualChunkCacheValidationResult ValidateRuntimeHandoffSidecar(VisualChunkCacheRuntimeHandoffSidecar sidecar)
    {
        ArgumentNullException.ThrowIfNull(sidecar);
        var diagnostics = new List<VisualChunkCacheDiagnostic>();

        if (sidecar.Accepted)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.accepted.must_be_false", sidecar.SidecarId, "Runtime handoff sidecar must remain accepted=false."));
        }

        if (!sidecar.MetadataOnly)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.metadata_only.required", sidecar.SidecarId, "Runtime handoff sidecar must be metadata-only."));
        }

        if (sidecar.ContainsRuntimeExecution)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.runtime_execution.forbidden", sidecar.SidecarId, "Runtime execution instructions are forbidden in Goal 093."));
        }

        if (sidecar.ContainsProviderCalls)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.provider_call.forbidden", sidecar.SidecarId, "Provider call instructions are forbidden in Goal 093."));
        }

        if (sidecar.ContainsUnityImplementation)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.unity_implementation.forbidden", sidecar.SidecarId, "Unity implementation instructions are forbidden in Goal 093."));
        }

        if (sidecar.ContainsPromptText)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.prompt_text.forbidden", sidecar.SidecarId, "Prompt text must not be source of truth for runtime handoff."));
        }

        if (sidecar.RecordCount <= 0)
        {
            diagnostics.Add(Error("visual_chunk_cache.sidecar.records.missing", sidecar.SidecarId, "Runtime handoff sidecar must reference exported records."));
        }

        ValidateArtifactRefs(sidecar.ArtifactRefs, sidecar.SidecarId, diagnostics);
        return Result(diagnostics);
    }

    public static VisualChunkCacheValidationResult ValidateSourceLineage(VisualChunkCacheSourceLineage sourceLineage)
    {
        ArgumentNullException.ThrowIfNull(sourceLineage);
        var diagnostics = new List<VisualChunkCacheDiagnostic>();

        if (!sourceLineage.Goal090LineagePresent)
        {
            diagnostics.Add(Error("visual_chunk_cache.source_lineage.goal090.missing", sourceLineage.GoalId, "Goal090 profile lineage is required."));
        }

        if (!sourceLineage.Goal091LineagePresent)
        {
            diagnostics.Add(Error("visual_chunk_cache.source_lineage.goal091.missing", sourceLineage.GoalId, "Goal091 stream-window lineage is required."));
        }

        if (sourceLineage.Records.Any(item => string.IsNullOrWhiteSpace(item.Sha256)))
        {
            diagnostics.Add(Error("visual_chunk_cache.source_lineage.hash.missing", sourceLineage.GoalId, "All source lineage records must have hashes."));
        }

        return Result(diagnostics);
    }

    public static VisualChunkCacheValidationResult ValidateInvalidationMatrix(VisualChunkCacheInvalidationMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        var diagnostics = new List<VisualChunkCacheDiagnostic>();

        foreach (var row in matrix.Rows)
        {
            if (!KnownInvalidationKeys.Contains(row.InvalidationKey) || !row.KnownKey)
            {
                diagnostics.Add(Error("visual_chunk_cache.invalidation_key.unknown", row.InvalidationKey, "Cache invalidation rules must use known keys."));
            }
        }

        if (matrix.RuleCount != matrix.Rows.Count || matrix.Rows.Count == 0)
        {
            diagnostics.Add(Error("visual_chunk_cache.invalidation_matrix.invalid", matrix.GoalId, "Invalidation matrix must contain the declared rule rows."));
        }

        return Result(diagnostics);
    }

    public static bool IsSafeRelativePath(string relativePath) =>
        !string.IsNullOrWhiteSpace(relativePath)
        && !Path.IsPathFullyQualified(relativePath)
        && !relativePath.StartsWith("/", StringComparison.Ordinal)
        && !relativePath.StartsWith("\\", StringComparison.Ordinal)
        && !relativePath.Contains("..", StringComparison.Ordinal);

    public static bool IsBinaryOrRasterPath(string relativePath) =>
        BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(relativePath));

    public static IReadOnlyList<VisualChunkCacheDiagnostic> SortDiagnostics(IEnumerable<VisualChunkCacheDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidatePackage(
        VisualChunkCacheExportPackage package,
        IReadOnlySet<string>? knownSourceChunkKeys,
        List<VisualChunkCacheDiagnostic> diagnostics)
    {
        if (package.Records.Count == 0)
        {
            diagnostics.Add(Error("visual_chunk_cache.package.records.missing", package.PackageId, "Cache export package must contain records."));
        }

        if (package.ExportedRecordCount != package.Records.Count)
        {
            diagnostics.Add(Error("visual_chunk_cache.package.record_count.invalid", package.PackageId, "Exported record count must match package records."));
        }

        if (string.IsNullOrWhiteSpace(package.ProfileId)
            || string.IsNullOrWhiteSpace(package.WorldSeed)
            || string.IsNullOrWhiteSpace(package.GeneratorVersion))
        {
            diagnostics.Add(Error("visual_chunk_cache.package.lineage.missing", package.PackageId, "Profile id, world seed and generator version are required."));
        }

        if (!string.Equals(package.GeneratorVersion, VisualChunkCacheExportContractVocabulary.ExpectedGeneratorVersion, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("visual_chunk_cache.generator_version.stale", package.PackageId, "Export package generator version must match the Goal091 source generator version."));
        }

        if (!package.NoRawFullWorldDump || !package.OnlyMaterializedChunksExported)
        {
            diagnostics.Add(Error("visual_chunk_cache.raw_full_world_dump.forbidden", package.PackageId, "Cache export must not include raw full-world cell dumps."));
        }

        if (package.ExportTargetKind == VisualChunkCacheExportTargetKind.RuntimeHandoff && !package.MetadataOnly)
        {
            diagnostics.Add(Error("visual_chunk_cache.runtime_handoff.metadata_only.required", package.PackageId, "Runtime handoff export package must stay metadata-only."));
        }

        var streamWindowIds = package.StreamWindows.Select(item => item.WindowId).ToHashSet(StringComparer.Ordinal);
        foreach (var record in package.Records)
        {
            ValidateRecord(record, package, streamWindowIds, knownSourceChunkKeys, diagnostics);
        }

        ValidateArtifactRefs(package.ArtifactRefs, package.PackageId, diagnostics);
        ValidateInvalidationRules(package.InvalidationRules, package.PackageId, diagnostics);
    }

    private static void ValidateRecord(
        VisualChunkCacheRecord record,
        VisualChunkCacheExportPackage package,
        IReadOnlySet<string> streamWindowIds,
        IReadOnlySet<string>? knownSourceChunkKeys,
        List<VisualChunkCacheDiagnostic> diagnostics)
    {
        if (!string.Equals(record.PackageId, package.PackageId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("visual_chunk_cache.record.package_mismatch", record.CacheKey.ChunkKey, "Record package id must match its containing package."));
        }

        if (string.IsNullOrWhiteSpace(record.CacheKey.ChunkKey))
        {
            diagnostics.Add(Error("visual_chunk_cache.chunk_key.missing", package.PackageId, "Cache record chunk key is required."));
        }
        else if (knownSourceChunkKeys != null && knownSourceChunkKeys.Count > 0 && !knownSourceChunkKeys.Contains(record.CacheKey.ChunkKey))
        {
            diagnostics.Add(Error("visual_chunk_cache.chunk_key.unknown_source", record.CacheKey.ChunkKey, "Cache record must reference a known Goal091 source chunk key."));
        }

        if (string.IsNullOrWhiteSpace(record.ChunkHash))
        {
            diagnostics.Add(Error("visual_chunk_cache.chunk_hash.missing", record.CacheKey.ChunkKey, "Cache record chunk hash is required."));
        }

        if (!string.Equals(record.GeneratorVersion, VisualChunkCacheExportContractVocabulary.ExpectedGeneratorVersion, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("visual_chunk_cache.generator_version.stale", record.CacheKey.ChunkKey, "Cache record generator version must match source."));
        }

        if (record.StreamWindowIds.Count == 0 || record.StreamWindowIds.Any(item => !streamWindowIds.Contains(item)))
        {
            diagnostics.Add(Error("visual_chunk_cache.stream_window_membership.mismatch", record.CacheKey.ChunkKey, "Cache record stream-window membership must match source windows."));
        }

        if (record.ContainsRawFullWorldCellDump || !record.NoRawFullWorldDump)
        {
            diagnostics.Add(Error("visual_chunk_cache.raw_full_world_dump.forbidden", record.CacheKey.ChunkKey, "Cache records must not contain raw full-world cell dumps."));
        }

        if (record.PromptTextIsSourceOfTruth)
        {
            diagnostics.Add(Error("visual_chunk_cache.prompt.source_of_truth", record.CacheKey.ChunkKey, "Prompt text must not be cache export source of truth."));
        }

        if (record.RatingMetadata.ContainsAdultRatingMetadata && !record.RatingMetadata.SafeFallbackPresent)
        {
            diagnostics.Add(Error("visual_chunk_cache.rating.safe_fallback_missing", record.CacheKey.ChunkKey, "Adult/rating metadata requires safe fallback metadata."));
        }

        foreach (var overlay in record.DeltaOverlays.Where(item => item.ContainsRawCellPayload))
        {
            diagnostics.Add(Error("visual_chunk_cache.delta_overlay.raw_payload", overlay.OverlayId, "Delta overlays must be compact metadata only."));
        }

        ValidateArtifactRefs(record.ArtifactRefs, record.CacheKey.ChunkKey, diagnostics);
        ValidateInvalidationRules(record.InvalidationRules, record.CacheKey.ChunkKey, diagnostics);
    }

    private static void ValidateArtifactRefs(
        IEnumerable<VisualChunkCacheArtifactRef> artifactRefs,
        string target,
        List<VisualChunkCacheDiagnostic> diagnostics)
    {
        foreach (var artifact in artifactRefs)
        {
            if (!IsSafeRelativePath(artifact.RelativePath))
            {
                diagnostics.Add(Error("visual_chunk_cache.artifact_ref.absolute_path", target, "Artifact refs must use safe repository-relative paths."));
            }

            if (artifact.IsBinaryOrRaster || IsBinaryOrRasterPath(artifact.RelativePath))
            {
                diagnostics.Add(Error("visual_chunk_cache.artifact_ref.binary_raster", artifact.RelativePath, "Binary or raster artifact refs are forbidden in Goal 093."));
            }

            if (artifact.IsPromptDump
                || artifact.ArtifactKind.Contains("prompt", StringComparison.OrdinalIgnoreCase)
                || artifact.RelativePath.Contains("prompt", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("visual_chunk_cache.artifact_ref.prompt_dump", artifact.RelativePath, "Prompt dumps are forbidden in Goal 093."));
            }

            if (string.IsNullOrWhiteSpace(artifact.Sha256) || artifact.Sha256.Length != 64)
            {
                diagnostics.Add(Error("visual_chunk_cache.artifact_ref.hash.missing", artifact.RelativePath, "Artifact refs must carry source hashes."));
            }
        }
    }

    private static void ValidateInvalidationRules(
        IEnumerable<VisualChunkCacheInvalidationRule> rules,
        string target,
        List<VisualChunkCacheDiagnostic> diagnostics)
    {
        foreach (var rule in rules)
        {
            if (!KnownInvalidationKeys.Contains(rule.InvalidationKey))
            {
                diagnostics.Add(Error("visual_chunk_cache.invalidation_key.unknown", rule.InvalidationKey, "Cache invalidation rules must use known keys."));
            }

            if (string.IsNullOrWhiteSpace(rule.SourceValueHash) || rule.SourceValueHash.Length != 64)
            {
                diagnostics.Add(Error("visual_chunk_cache.invalidation_hash.missing", target, "Cache invalidation rules must carry source value hashes."));
            }
        }
    }

    private static VisualChunkCacheValidationResult Result(List<VisualChunkCacheDiagnostic> diagnostics) =>
        new()
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static VisualChunkCacheDiagnostic Error(string code, string target, string message) =>
        VisualChunkCacheDiagnostic.Error(code, target, message);
}
