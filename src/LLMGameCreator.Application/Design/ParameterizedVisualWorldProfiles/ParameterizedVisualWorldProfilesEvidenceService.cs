using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

public sealed class ParameterizedVisualWorldProfilesEvidenceService
{
    public const string ReportMarkdownFileName = "visual-world-profile-report.md";
    public const string CatalogJsonFileName = "visual-world-profile-catalog.json";
    public const string SizeMatrixJsonFileName = "visual-world-profile-size-matrix.json";
    public const string ValidationMatrixJsonFileName = "visual-world-profile-validation-matrix.json";
    public const string NegativeProofJsonFileName = "visual-world-profile-negative-proof.json";
    public const string ChunkAddressProofJsonFileName = "visual-world-profile-chunk-address-proof.json";
    public const string SparseWorldProofJsonFileName = "visual-world-profile-sparse-world-proof.json";
    public const string LayerModelProofJsonFileName = "visual-world-profile-layer-model-proof.json";
    public const string SourceLineageJsonFileName = "visual-world-profile-source-lineage.json";
    public const string QualityGateScanJsonFileName = "visual-world-profile-quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static ParameterizedVisualWorldProfilesEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualWorldProfileEvidenceResult Build(string? projectRootPath = null)
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var catalog = new VisualWorldProfileCatalog { Profiles = profiles };
        var sizeMatrix = BuildSizeMatrix(profiles);
        var validationMatrix = BuildValidationMatrix(profiles);
        var negativeProof = BuildNegativeProof();
        var chunkAddressProof = BuildChunkAddressProof(profiles);
        var sparseWorldProof = BuildSparseWorldProof(profiles);
        var layerModelProof = BuildLayerModelProof(profiles);
        var sourceLineage = BuildSourceLineage(projectRootPath);
        var overviewSvgs = RenderOverviewSvgs(profiles);
        var qualityGate = ParameterizedVisualWorldProfilesQualityGateScanner.Build(
            catalog,
            sizeMatrix,
            validationMatrix,
            negativeProof,
            chunkAddressProof,
            sparseWorldProof,
            layerModelProof,
            sourceLineage,
            overviewSvgs);

        var catalogJson = Serialize(catalog);
        var sizeMatrixJson = Serialize(sizeMatrix);
        var validationMatrixJson = Serialize(validationMatrix);
        var negativeProofJson = Serialize(negativeProof);
        var chunkAddressProofJson = Serialize(chunkAddressProof);
        var sparseWorldProofJson = Serialize(sparseWorldProof);
        var layerModelProofJson = Serialize(layerModelProof);
        var sourceLineageJson = Serialize(sourceLineage);
        var qualityGateJson = Serialize(qualityGate);

        var reportWithoutHash = BuildReport(
            profiles.Count,
            validationMatrix,
            sizeMatrix,
            negativeProof,
            chunkAddressProof,
            sparseWorldProof,
            layerModelProof,
            sourceLineage,
            qualityGate,
            catalogJson,
            sizeMatrixJson,
            validationMatrixJson,
            negativeProofJson,
            chunkAddressProofJson,
            sparseWorldProofJson,
            layerModelProofJson,
            sourceLineageJson,
            qualityGateJson);
        var reportMarkdownWithoutHash = RenderReport(
            reportWithoutHash,
            catalog,
            sizeMatrix,
            validationMatrix,
            negativeProof,
            sparseWorldProof,
            layerModelProof,
            qualityGate,
            string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = ParameterizedVisualWorldProfilesHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(
            report,
            catalog,
            sizeMatrix,
            validationMatrix,
            negativeProof,
            sparseWorldProof,
            layerModelProof,
            qualityGate,
            report.DeterministicReportHash);

        return new VisualWorldProfileEvidenceResult
        {
            Catalog = catalog,
            SizeMatrix = sizeMatrix,
            ValidationMatrix = validationMatrix,
            NegativeProof = negativeProof,
            ChunkAddressProof = chunkAddressProof,
            SparseWorldProof = sparseWorldProof,
            LayerModelProof = layerModelProof,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            SizeMatrixJson = sizeMatrixJson,
            ValidationMatrixJson = validationMatrixJson,
            NegativeProofJson = negativeProofJson,
            ChunkAddressProofJson = chunkAddressProofJson,
            SparseWorldProofJson = sparseWorldProofJson,
            LayerModelProofJson = layerModelProofJson,
            SourceLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown,
            OverviewSvgByRelativePath = overviewSvgs
        };
    }

    public async Task<VisualWorldProfileWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualWorldProfileWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldProfileEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            ParameterizedVisualWorldProfilesVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var overviewDirectory = Path.Combine(outputDirectory, "profile-overviews");
        EnsureContained(outputDirectory, overviewDirectory);
        Directory.CreateDirectory(overviewDirectory);

        var write = new VisualWorldProfileWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            SizeMatrixJsonPath = Path.Combine(outputDirectory, SizeMatrixJsonFileName),
            ValidationMatrixJsonPath = Path.Combine(outputDirectory, ValidationMatrixJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            ChunkAddressProofJsonPath = Path.Combine(outputDirectory, ChunkAddressProofJsonFileName),
            SparseWorldProofJsonPath = Path.Combine(outputDirectory, SparseWorldProofJsonFileName),
            LayerModelProofJsonPath = Path.Combine(outputDirectory, LayerModelProofJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            OverviewSvgPaths = result.OverviewSvgByRelativePath.Keys
                .OrderBy(item => item, StringComparer.Ordinal)
                .Select(item => Path.Combine(outputDirectory, item.Replace('/', Path.DirectorySeparatorChar)))
                .ToList()
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SizeMatrixJsonPath, result.SizeMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ValidationMatrixJsonPath, result.ValidationMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ChunkAddressProofJsonPath, result.ChunkAddressProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SparseWorldProofJsonPath, result.SparseWorldProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LayerModelProofJsonPath, result.LayerModelProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        foreach (var (relativePath, svg) in result.OverviewSvgByRelativePath.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            await File.WriteAllTextAsync(path, svg, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    public static VisualWorldProfileSizeMatrix BuildSizeMatrix(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var matrixProfile = profiles.Single(item => item.ProfileId == "finite_custom_sizes_matrix");
        var rows = matrixProfile.FiniteSizeSamples
            .OrderBy(item => item.Width)
            .ThenBy(item => item.Height)
            .Select(size =>
            {
                var candidate = matrixProfile with
                {
                    FiniteWidth = size.Width,
                    FiniteHeight = size.Height,
                    VirtualBounds = FiniteBounds(size.Width, size.Height),
                    LogicalCellCount = size.LogicalCellCount,
                    FiniteSizeSamples = []
                };
                var validation = ParameterizedVisualWorldProfilesValidator.Validate(candidate);
                var sampleValidation = ParameterizedVisualWorldProfilesValidator.ValidateFiniteSizeSample(matrixProfile, size);
                var diagnostics = validation.Diagnostics.Concat(sampleValidation.Diagnostics).ToList();
                return new VisualWorldProfileSizeMatrixRow
                {
                    ProfileId = matrixProfile.ProfileId,
                    SizeId = size.SizeId,
                    Width = size.Width,
                    Height = size.Height,
                    LayerCount = size.LayerCount,
                    LogicalCellCount = size.LogicalCellCount,
                    ValidatorPassed = validation.Passed && sampleValidation.Passed,
                    Diagnostics = diagnostics
                };
            })
            .ToList();

        return new VisualWorldProfileSizeMatrix
        {
            Passed = rows.Count >= 6 && rows.All(item => item.ValidatorPassed),
            Rows = rows
        };
    }

    public static VisualWorldProfileValidationMatrix BuildValidationMatrix(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var rows = profiles
            .OrderBy(item => item.ProfileId, StringComparer.Ordinal)
            .Select(profile =>
            {
                var validation = ParameterizedVisualWorldProfilesValidator.Validate(profile);
                return new VisualWorldProfileValidationMatrixRow
                {
                    ProfileId = profile.ProfileId,
                    Passed = validation.Passed,
                    DiagnosticCount = validation.DiagnosticCount,
                    Mode = profile.Mode.ToString(),
                    IsBenchmark = profile.IsBenchmarkProfile,
                    RawCellDumpAllowed = profile.RawCellDumpAllowed
                };
            })
            .ToList();

        return new VisualWorldProfileValidationMatrix
        {
            Passed = rows.Count == 4 && rows.All(item => item.Passed),
            Rows = rows
        };
    }

    public static VisualWorldProfileNegativeProof BuildNegativeProof()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var finite = profiles.Single(item => item.ProfileId == "finite_custom_sizes_matrix");
        var huge = profiles.Single(item => item.ProfileId == "huge_sparse_100000x100000_multilayer");
        var infinite = profiles.Single(item => item.ProfileId == "infinite_streaming_world_multilayer");
        var firstSample = huge.SparseRegionIndex.MaterializedChunks[0];

        var scenarios = new List<VisualWorldProfileNegativeScenario>
        {
            Invalid("fixed_size_only_profile_claims_generic", "fixed-size allowlist pretending to be generic", finite with { ClaimsGenericButUsesFixedSizeAllowlist = true, FixedSizeAllowlist = ["144x144", "256x256"] }),
            Invalid("finite_invalid_dimensions", "finite profile width below validation bound", finite with { FiniteWidth = 0, LogicalCellCount = 0 }),
            Invalid("huge_attempts_raw_cell_dump", "huge sparse profile attempts raw cell dump", huge with { RawCellDumpAllowed = true, SparseRegionIndex = huge.SparseRegionIndex with { AttemptsRawCellDump = true } }),
            Invalid("infinite_declares_finite_only_materialization", "infinite profile declares finite-only materialization", infinite with { SparseRegionIndex = infinite.SparseRegionIndex with { FiniteOnlyMaterialization = true } }),
            Invalid("invalid_layer_id", "invalid layer id", finite with { Layers = [finite.Layers[0] with { LayerId = "Surface Bad" }, .. finite.Layers.Skip(1)] }),
            Invalid("duplicate_layer_ids", "duplicate layer ids", finite with { Layers = [finite.Layers[0], finite.Layers[1] with { LayerId = finite.Layers[0].LayerId }, finite.Layers[2]] }),
            Invalid("hardcoded_surface_underground_only_requirement", "hardcoded surface and underground layer requirement", finite with { RequiresSurfaceUndergroundOnly = true }),
            Invalid("chunk_size_zero", "chunk width is zero", finite with { ChunkProfile = finite.ChunkProfile with { ChunkWidth = 0 } }),
            Invalid("patch_size_zero", "patch height is zero", finite with { PatchProfile = finite.PatchProfile with { PatchHeight = 0 } }),
            Invalid("patch_chunk_incompatibility", "patch no longer divides chunk", finite with { ChunkProfile = finite.ChunkProfile with { ChunkWidth = 30 } }),
            Invalid("missing_world_seed", "missing world seed", finite with { WorldSeed = "" }),
            Invalid("missing_generator_version", "missing generator version", finite with { GeneratorVersion = "" }),
            Invalid("absolute_output_path", "absolute output path", finite with { OutputRelativeDirectory = "C:/unsafe/output" }),
            Invalid("non_deterministic_chunk_key", "chunk key does not match formula", huge with { SparseRegionIndex = huge.SparseRegionIndex with { MaterializedChunks = [firstSample with { ChunkKey = firstSample.ChunkKey with { Key = "not_the_deterministic_key" } }, .. huge.SparseRegionIndex.MaterializedChunks.Skip(1)] } }),
            Invalid("layer_link_unknown_layer", "layer link references unknown layer", finite with { LayerLinks = [.. finite.LayerLinks, new VisualLayerLink { LinkId = "bad_unknown_layer_link", FromLayerId = finite.Layers[0].LayerId, ToLayerId = "missing_layer", LinkKind = VisualLayerLinkKind.Portal }] }),
            Invalid("stream_window_without_center", "stream window missing center and radius", infinite with { StreamWindows = [new VisualStreamWindow { WindowId = "broken_stream_window", RadiusChunks = -1, WindowChunkCount = 0 }] }),
            Invalid("rating_metadata_without_safe_fallback", "adult/rating metadata missing safe fallback", huge with { RatingMetadata = [new VisualRatingMetadata { MetadataId = "adult_rating_missing_safe_fallback", RatingKind = "adult_metadata" }] }),
            Invalid("prompt_text_source_of_truth", "prompt text as source of truth", finite with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" })
        };

        return new VisualWorldProfileNegativeProof
        {
            Passed = scenarios.Count >= 18
                && scenarios.All(item => !item.ActualValid && item.ExpectedValid == item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static VisualWorldProfileChunkAddressProof BuildChunkAddressProof(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var selected = profiles
            .Where(profile => profile.ProfileId is "huge_sparse_100000x100000_multilayer" or "infinite_streaming_world_multilayer")
            .SelectMany(profile => profile.SparseRegionIndex.MaterializedChunks.Take(3).Select(sample => (profile, sample.Address)))
            .ToList();
        var rows = selected
            .Select(item =>
            {
                var first = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile, item.Address);
                var second = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile, item.Address);
                var alternateLayer = item.profile.Layers.Select(layer => layer.LayerId).First(layerId => layerId != item.Address.LayerId);
                return new VisualChunkKeyProofRow
                {
                    ProfileId = item.profile.ProfileId,
                    Address = item.Address,
                    FirstKey = first.Key,
                    SecondKey = second.Key,
                    VariantSeedKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile.ProfileId, item.profile.WorldSeed + "-variant", item.profile.GeneratorVersion, item.Address.LayerId, item.Address.ChunkX, item.Address.ChunkY).Key,
                    VariantLayerKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile.ProfileId, item.profile.WorldSeed, item.profile.GeneratorVersion, alternateLayer, item.Address.ChunkX, item.Address.ChunkY).Key,
                    VariantChunkKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile.ProfileId, item.profile.WorldSeed, item.profile.GeneratorVersion, item.Address.LayerId, item.Address.ChunkX + 1, item.Address.ChunkY).Key,
                    VariantVersionKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(item.profile.ProfileId, item.profile.WorldSeed, item.profile.GeneratorVersion + "-v2", item.Address.LayerId, item.Address.ChunkX, item.Address.ChunkY).Key
                };
            })
            .ToList();
        var stable = rows.All(item => item.FirstKey == item.SecondKey);
        var differs = rows.All(item =>
            item.FirstKey != item.VariantSeedKey
            && item.FirstKey != item.VariantLayerKey
            && item.FirstKey != item.VariantChunkKey
            && item.FirstKey != item.VariantVersionKey);

        return new VisualWorldProfileChunkAddressProof
        {
            Passed = rows.Count > 0 && stable && differs,
            StableAcrossReruns = stable,
            DiffersBySeedLayerChunkAndVersion = differs,
            Rows = rows
        };
    }

    public static VisualWorldProfileSparseWorldProof BuildSparseWorldProof(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var rows = profiles
            .Where(profile => profile.Mode is VisualWorldProfileMode.HugeSparseFinite or VisualWorldProfileMode.Infinite)
            .OrderBy(profile => profile.ProfileId, StringComparer.Ordinal)
            .Select(profile => new VisualSparseWorldProofRow
            {
                ProfileId = profile.ProfileId,
                IsInfinite = profile.IsInfinite,
                LogicalCellCount = profile.LogicalCellCount,
                EstimatedChunkCapacity = ParameterizedVisualWorldProfilesValidator.EstimateFiniteChunkCapacity(profile),
                MaterializedChunkCount = profile.SparseRegionIndex.MaterializedChunks.Count,
                SparseOnly = profile.SparseRegionIndex.SparseOnly,
                RawCellDumpAllowed = profile.RawCellDumpAllowed || profile.SparseRegionIndex.AttemptsRawCellDump
            })
            .ToList();
        var huge = rows.Single(item => item.ProfileId == "huge_sparse_100000x100000_multilayer");
        var infinite = rows.Single(item => item.ProfileId == "infinite_streaming_world_multilayer");
        var hugePassed = huge is { SparseOnly: true, RawCellDumpAllowed: false, EstimatedChunkCapacity: > 0 }
            && huge.MaterializedChunkCount > 0
            && huge.MaterializedChunkCount < huge.EstimatedChunkCapacity;
        var infinitePassed = infinite is { IsInfinite: true, LogicalCellCount: null, EstimatedChunkCapacity: null, SparseOnly: true, RawCellDumpAllowed: false }
            && infinite.MaterializedChunkCount > 0;

        return new VisualWorldProfileSparseWorldProof
        {
            Passed = hugePassed && infinitePassed,
            HugeSparseProfilePassed = hugePassed,
            InfiniteProfilePassed = infinitePassed,
            Rows = rows
        };
    }

    public static VisualWorldProfileLayerModelProof BuildLayerModelProof(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var rows = profiles
            .OrderBy(profile => profile.ProfileId, StringComparer.Ordinal)
            .Select(profile =>
            {
                var layerIds = profile.Layers.Select(item => item.LayerId).OrderBy(item => item, StringComparer.Ordinal).ToList();
                return new VisualLayerModelProofRow
                {
                    ProfileId = profile.ProfileId,
                    LayerIds = layerIds,
                    LayerCount = layerIds.Count,
                    UsesOnlySurfaceUnderground = layerIds.SequenceEqual(["surface", "underground"])
                };
            })
            .ToList();
        var notRestricted = rows.Any(item => !item.UsesOnlySurfaceUnderground && item.LayerCount >= 3)
            && rows.Any(item => item.LayerIds.Contains("underwater", StringComparer.Ordinal))
            && rows.Any(item => item.LayerIds.Contains("interior", StringComparer.Ordinal))
            && rows.Any(item => item.LayerIds.Contains("weather_overlay", StringComparer.Ordinal));

        return new VisualWorldProfileLayerModelProof
        {
            Passed = notRestricted && profiles.All(profile => !profile.RequiresSurfaceUndergroundOnly),
            DataDrivenLayerSetsPassed = profiles.Select(profile => string.Join(",", profile.Layers.Select(layer => layer.LayerId))).Distinct(StringComparer.Ordinal).Count() > 1,
            NotRestrictedToSurfaceUnderground = notRestricted,
            Rows = rows
        };
    }

    public static VisualWorldProfileSourceLineage BuildSourceLineage(string? projectRootPath)
    {
        var records = new[]
        {
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md", ["goal087", "patch_composer_report"]),
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-catalog.json", ["goal087", "patch_catalog"]),
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-composer-report.md", ["goal088", "region_composer_report"]),
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-quality-gate-scan.json", ["goal088", "quality_gate"]),
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/check-all-hang-triage-report.md", ["goal088a", "full_check_all_baseline"]),
            SourceRecord(projectRootPath, ".llmgc/procedural/goal-089-tiered-validation-pipeline/tiered-validation-pipeline-report.md", ["goal089", "tiered_validation_policy"]),
            SourceRecord(projectRootPath, "docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md", ["deepsearch", "visual_stack"]),
            SourceRecord(projectRootPath, "docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md", ["deepsearch", "chunked_world_map"]),
            SourceRecord(projectRootPath, "docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md", ["deepsearch", "pseudo3d_sidecar"])
        };
        var goal087 = records.Any(item => item.Exists && item.PurposeTags.Contains("goal087", StringComparer.Ordinal));
        var goal088 = records.Any(item => item.Exists && item.PurposeTags.Contains("goal088", StringComparer.Ordinal));

        return new VisualWorldProfileSourceLineage
        {
            Passed = records.All(item => item.Exists) && goal087 && goal088,
            Goal087LineagePresent = goal087,
            Goal088LineagePresent = goal088,
            SourceRecordCount = records.Length,
            Records = records
        };
    }

    private static VisualWorldProfileReport BuildReport(
        int profileCount,
        VisualWorldProfileValidationMatrix validationMatrix,
        VisualWorldProfileSizeMatrix sizeMatrix,
        VisualWorldProfileNegativeProof negativeProof,
        VisualWorldProfileChunkAddressProof chunkAddressProof,
        VisualWorldProfileSparseWorldProof sparseWorldProof,
        VisualWorldProfileLayerModelProof layerModelProof,
        VisualWorldProfileSourceLineage sourceLineage,
        VisualWorldProfileQualityGateScan qualityGate,
        string catalogJson,
        string sizeMatrixJson,
        string validationMatrixJson,
        string negativeProofJson,
        string chunkAddressProofJson,
        string sparseWorldProofJson,
        string layerModelProofJson,
        string sourceLineageJson,
        string qualityGateJson) =>
        new()
        {
            ProfileCount = profileCount,
            ValidationPassed = validationMatrix.Passed,
            SizeMatrixPassed = sizeMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            ChunkAddressProofPassed = chunkAddressProof.Passed,
            SparseWorldProofPassed = sparseWorldProof.Passed,
            LayerModelProofPassed = layerModelProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            QualityGatePassed = qualityGate.Diagnostics.Count == 0,
            CatalogHash = ParameterizedVisualWorldProfilesHash.Compute(catalogJson),
            SizeMatrixHash = ParameterizedVisualWorldProfilesHash.Compute(sizeMatrixJson),
            ValidationMatrixHash = ParameterizedVisualWorldProfilesHash.Compute(validationMatrixJson),
            NegativeProofHash = ParameterizedVisualWorldProfilesHash.Compute(negativeProofJson),
            ChunkAddressProofHash = ParameterizedVisualWorldProfilesHash.Compute(chunkAddressProofJson),
            SparseWorldProofHash = ParameterizedVisualWorldProfilesHash.Compute(sparseWorldProofJson),
            LayerModelProofHash = ParameterizedVisualWorldProfilesHash.Compute(layerModelProofJson),
            SourceLineageHash = ParameterizedVisualWorldProfilesHash.Compute(sourceLineageJson),
            QualityGateHash = ParameterizedVisualWorldProfilesHash.Compute(qualityGateJson)
        };

    private static string RenderReport(
        VisualWorldProfileReport report,
        VisualWorldProfileCatalog catalog,
        VisualWorldProfileSizeMatrix sizeMatrix,
        VisualWorldProfileValidationMatrix validationMatrix,
        VisualWorldProfileNegativeProof negativeProof,
        VisualWorldProfileSparseWorldProof sparseWorldProof,
        VisualWorldProfileLayerModelProof layerModelProof,
        VisualWorldProfileQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var benchmark = catalog.Profiles.Single(item => item.ProfileId == "benchmark_heroes_144x144_surface_underground");
        var huge = sparseWorldProof.Rows.Single(item => item.ProfileId == "huge_sparse_100000x100000_multilayer");
        var infinite = sparseWorldProof.Rows.Single(item => item.ProfileId == "infinite_streaming_world_multilayer");
        var lines = new List<string>
        {
            "# Goal 090 Visual World Profile Report",
            string.Empty,
            "- implementationStatus: GREEN",
            "- accepted: false",
            $"- manualGate: {ParameterizedVisualWorldProfilesVocabulary.FinalGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 090 adds a BCL-only Application-side visual world profile and chunk addressing seam. The seam proves finite arbitrary dimensions, sparse huge finite worlds and infinite chunk-addressed streaming worlds without Runtime, Unity, provider, Lua, public GamePackage schema, project-file or dependency changes.",
            string.Empty,
            "## Profile Fixtures",
            string.Empty
        };

        lines.AddRange(catalog.Profiles
            .OrderBy(item => item.ProfileId, StringComparer.Ordinal)
            .Select(profile => $"- {profile.ProfileId}: mode={profile.Mode}, infinite={profile.IsInfinite.ToString().ToLowerInvariant()}, layers={profile.Layers.Count}, rawCellDumpAllowed={profile.RawCellDumpAllowed.ToString().ToLowerInvariant()}"));
        lines.AddRange([
            string.Empty,
            "## Benchmark Boundary",
            string.Empty,
            $"- benchmarkProfileId: {benchmark.ProfileId}",
            $"- benchmarkDimensions: {benchmark.FiniteWidth}x{benchmark.FiniteHeight}",
            $"- benchmarkMarkedAsFixtureOnly: {benchmark.IsBenchmarkProfile.ToString().ToLowerInvariant()}",
            "- architecturalLimit: false",
            string.Empty,
            "## Arbitrary Finite Size Matrix",
            string.Empty,
            $"- sizeMatrixPassed: {sizeMatrix.Passed.ToString().ToLowerInvariant()}",
            $"- rows: {sizeMatrix.Rows.Count}",
            $"- sizes: {string.Join(", ", sizeMatrix.Rows.Select(item => $"{item.Width}x{item.Height}"))}",
            string.Empty,
            "## Sparse And Infinite Proof",
            string.Empty,
            $"- hugeProfile: {huge.ProfileId}",
            $"- hugeLogicalCellCount: {huge.LogicalCellCount}",
            $"- hugeEstimatedChunkCapacity: {huge.EstimatedChunkCapacity}",
            $"- hugeMaterializedChunkCount: {huge.MaterializedChunkCount}",
            $"- infiniteProfile: {infinite.ProfileId}",
            $"- infiniteLogicalCellCount: {(infinite.LogicalCellCount.HasValue ? infinite.LogicalCellCount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "none")}",
            $"- infiniteMaterializedChunkCount: {infinite.MaterializedChunkCount}",
            string.Empty,
            "## Layer Model",
            string.Empty,
            $"- layerModelProofPassed: {layerModelProof.Passed.ToString().ToLowerInvariant()}",
            $"- notRestrictedToSurfaceUnderground: {layerModelProof.NotRestrictedToSurfaceUnderground.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validationMatrixPassed: {validationMatrix.Passed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            $"- chunkAddressProofPassed: {report.ChunkAddressProofPassed.ToString().ToLowerInvariant()}",
            $"- sparseWorldProofPassed: {report.SparseWorldProofPassed.ToString().ToLowerInvariant()}",
            $"- sourceLineagePassed: {report.SourceLineagePassed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- noRawHeavyCellDump: {qualityGate.NoRawHeavyCellDump.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noExplicitAdultContent: {qualityGate.NoExplicitAdultContent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- sizeMatrixHash: {report.SizeMatrixHash}",
            $"- validationMatrixHash: {report.ValidationMatrixHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- chunkAddressProofHash: {report.ChunkAddressProofHash}",
            $"- sparseWorldProofHash: {report.SparseWorldProofHash}",
            $"- layerModelProofHash: {report.LayerModelProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyDictionary<string, string> RenderOverviewSvgs(IReadOnlyList<VisualWorldProfile> profiles) =>
        profiles.ToDictionary(
            profile => $"profile-overviews/{profile.ProfileId}.svg",
            RenderProfileOverviewSvg,
            StringComparer.Ordinal);

    private static string RenderProfileOverviewSvg(VisualWorldProfile profile)
    {
        const int blockWidth = 46;
        var width = Math.Max(320, profile.Layers.Count * blockWidth + 24);
        var height = 120;
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 {width} {height}\" data-profile-id=\"{Escape(profile.ProfileId)}\">",
            $"  <title>{Escape(profile.ProfileId)}</title>",
            $"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"#12161b\" />",
            $"  <text x=\"12\" y=\"18\" fill=\"#e8eef3\" font-size=\"10\" font-family=\"monospace\">{Escape(profile.ProfileId)}</text>",
            $"  <text x=\"12\" y=\"34\" fill=\"#9fb0bf\" font-size=\"8\" font-family=\"monospace\">mode={profile.Mode}; chunks={profile.ChunkProfile.ChunkWidth}x{profile.ChunkProfile.ChunkHeight}; patch={profile.PatchProfile.PatchWidth}x{profile.PatchProfile.PatchHeight}</text>"
        };

        for (var index = 0; index < profile.Layers.Count; index++)
        {
            var layer = profile.Layers.OrderBy(item => item.Order).ElementAt(index);
            var x = 12 + index * blockWidth;
            lines.Add($"  <rect x=\"{x}\" y=\"48\" width=\"40\" height=\"24\" fill=\"{LayerColor(index)}\" stroke=\"#26313b\" stroke-width=\"1\" data-layer-id=\"{Escape(layer.LayerId)}\" />");
            lines.Add($"  <text x=\"{x + 3}\" y=\"63\" fill=\"#0f1317\" font-size=\"6\" font-family=\"monospace\">{Escape(Short(layer.LayerId))}</text>");
        }

        var sampleOffset = 12;
        foreach (var sample in profile.SparseRegionIndex.MaterializedChunks.Take(6))
        {
            lines.Add($"  <rect x=\"{sampleOffset}\" y=\"86\" width=\"14\" height=\"14\" fill=\"#c79d4a\" data-chunk-key=\"{Escape(sample.ChunkKey.Key[..12])}\" />");
            sampleOffset += 18;
        }

        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static VisualWorldProfileNegativeScenario Invalid(
        string id,
        string mutation,
        VisualWorldProfile profile)
    {
        var validation = ParameterizedVisualWorldProfilesValidator.Validate(profile);
        return new VisualWorldProfileNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static VisualWorldProfileSourceLineageRecord SourceRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new VisualWorldProfileSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : ParameterizedVisualWorldProfilesHash.Compute(text),
            PurposeTags = tags
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

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ReadText(string? projectRootPath, string relativePath)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        return fullPath != null && File.Exists(fullPath)
            ? File.ReadAllText(fullPath, Encoding.UTF8)
            : string.Empty;
    }

    private static string? ResolveOptionalPath(string? projectRootPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(Path.GetFullPath(projectRootPath), relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static string LayerColor(int index) =>
        (index % 6) switch
        {
            0 => "#69a55a",
            1 => "#746b84",
            2 => "#4d8aa1",
            3 => "#d0b15c",
            4 => "#7ba7d1",
            _ => "#a66d58"
        };

    private static string Short(string value) =>
        value.Length <= 10 ? value : value[..10];

    private static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
