using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;

public sealed class DeterministicVisualMicrotileMaterializerEvidenceService
{
    public const string ReportMarkdownFileName = "visual-microtile-materializer-report.md";
    public const string PreviewCatalogJsonFileName = "visual-microtile-preview-catalog.json";
    public const string MaterializationManifestJsonFileName = "visual-microtile-materialization-manifest.json";
    public const string FileLedgerJsonFileName = "visual-microtile-file-ledger.json";
    public const string WaterBiomeProofJsonFileName = "visual-microtile-water-biome-proof.json";
    public const string LayeringProofJsonFileName = "visual-microtile-layering-proof.json";
    public const string NegativeProofJsonFileName = "visual-microtile-negative-proof.json";
    public const string QualityGateScanJsonFileName = "visual-microtile-quality-gate-scan.json";
    public const string SourceLineageJsonFileName = "visual-microtile-source-lineage.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static DeterministicVisualMicrotileMaterializerEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualMicrotileEvidenceResult Build(string? projectRootPath = null)
    {
        var request = DeterministicVisualMicrotileMaterializerFixtures.BuildDefaultRequest();
        var svgByPreviewId = request.Previews
            .OrderBy(item => item.PreviewId, StringComparer.Ordinal)
            .ToDictionary(item => item.PreviewId, RenderSvg, StringComparer.Ordinal);
        var validation = DeterministicVisualMicrotileMaterializerValidator.Validate(request, svgByPreviewId);
        var previewCatalog = BuildPreviewCatalog(request);
        var materializationManifest = BuildMaterializationManifest(request, svgByPreviewId);
        var waterProof = BuildWaterBiomeProof(request);
        var layeringProof = BuildLayeringProof(request);
        var negativeProof = BuildNegativeProof(request);
        var sourceLineage = BuildSourceLineage(projectRootPath);
        var qualityGate = DeterministicVisualMicrotileMaterializerQualityGateScanner.Build(
            request,
            validation,
            waterProof,
            layeringProof,
            negativeProof,
            sourceLineage,
            svgByPreviewId);

        var previewCatalogJson = Serialize(previewCatalog);
        var materializationManifestJson = Serialize(materializationManifest);
        var waterProofJson = Serialize(waterProof);
        var layeringProofJson = Serialize(layeringProof);
        var negativeProofJson = Serialize(negativeProof);
        var qualityGateJson = Serialize(qualityGate);
        var sourceLineageJson = Serialize(sourceLineage);
        var fileLedger = BuildFileLedger(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [PreviewCatalogJsonFileName] = previewCatalogJson,
                [MaterializationManifestJsonFileName] = materializationManifestJson,
                [WaterBiomeProofJsonFileName] = waterProofJson,
                [LayeringProofJsonFileName] = layeringProofJson,
                [NegativeProofJsonFileName] = negativeProofJson,
                [QualityGateScanJsonFileName] = qualityGateJson,
                [SourceLineageJsonFileName] = sourceLineageJson
            },
            request,
            svgByPreviewId);
        var fileLedgerJson = Serialize(fileLedger);

        var reportWithoutHash = BuildReport(
            previewCatalog,
            validation,
            negativeProof,
            qualityGate,
            previewCatalogJson,
            materializationManifestJson,
            fileLedgerJson,
            waterProofJson,
            layeringProofJson,
            negativeProofJson,
            qualityGateJson,
            sourceLineageJson);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, previewCatalog, waterProof, negativeProof, qualityGate, deterministicReportHash: string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = DeterministicVisualMicrotileMaterializerHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, previewCatalog, waterProof, negativeProof, qualityGate, report.DeterministicReportHash);

        return new VisualMicrotileEvidenceResult
        {
            PreviewCatalog = previewCatalog,
            MaterializationManifest = materializationManifest,
            FileLedger = fileLedger,
            WaterBiomeProof = waterProof,
            LayeringProof = layeringProof,
            NegativeProof = negativeProof,
            QualityGateScan = qualityGate,
            SourceLineage = sourceLineage,
            Report = report,
            PreviewCatalogJson = previewCatalogJson,
            MaterializationManifestJson = materializationManifestJson,
            FileLedgerJson = fileLedgerJson,
            WaterBiomeProofJson = waterProofJson,
            LayeringProofJson = layeringProofJson,
            NegativeProofJson = negativeProofJson,
            QualityGateScanJson = qualityGateJson,
            SourceLineageJson = sourceLineageJson,
            ReportMarkdown = reportMarkdown,
            SvgByPreviewId = svgByPreviewId
        };
    }

    public async Task<VisualMicrotileWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualMicrotileWriteResult> WriteAsync(
        string projectRootPath,
        VisualMicrotileEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var previewDirectory = Path.GetFullPath(Path.Combine(outputDirectory, DeterministicVisualMicrotileMaterializerVocabulary.PreviewRelativeDirectory));
        EnsureContained(outputDirectory, previewDirectory);
        Directory.CreateDirectory(previewDirectory);

        var write = new VisualMicrotileWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            PreviewDirectoryPath = previewDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            PreviewCatalogJsonPath = Path.Combine(outputDirectory, PreviewCatalogJsonFileName),
            MaterializationManifestJsonPath = Path.Combine(outputDirectory, MaterializationManifestJsonFileName),
            FileLedgerJsonPath = Path.Combine(outputDirectory, FileLedgerJsonFileName),
            WaterBiomeProofJsonPath = Path.Combine(outputDirectory, WaterBiomeProofJsonFileName),
            LayeringProofJsonPath = Path.Combine(outputDirectory, LayeringProofJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            PreviewSvgPaths = result.PreviewCatalog.Previews
                .OrderBy(item => item.PreviewId, StringComparer.Ordinal)
                .Select(item => Path.Combine(outputDirectory, item.PreviewRelativePath.Replace('/', Path.DirectorySeparatorChar)))
                .ToList()
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.PreviewCatalogJsonPath, result.PreviewCatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MaterializationManifestJsonPath, result.MaterializationManifestJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FileLedgerJsonPath, result.FileLedgerJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WaterBiomeProofJsonPath, result.WaterBiomeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LayeringProofJsonPath, result.LayeringProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        foreach (var preview in result.PreviewCatalog.Previews.OrderBy(item => item.PreviewId, StringComparer.Ordinal))
        {
            var previewPath = Path.Combine(outputDirectory, preview.PreviewRelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, previewPath);
            await File.WriteAllTextAsync(previewPath, result.SvgByPreviewId[preview.PreviewId], Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    public static VisualMicrotileNegativeProof BuildNegativeProof(VisualMicrotileMaterializationRequest baseline)
    {
        var first = baseline.Previews[0];
        var coast = baseline.Previews.Single(item => item.PreviewId == "water_coast_transition");
        var river = baseline.Previews.Single(item => item.PreviewId == "water_river_segment");
        var adult = baseline.Previews.Single(item => item.PreviewId == "adult_metadata_only_safe_fallback_slot");
        var baselineSvgs = baseline.Previews.ToDictionary(item => item.PreviewId, RenderSvg, StringComparer.Ordinal);
        var unsafeSvgs = new Dictionary<string, string>(baselineSvgs, StringComparer.Ordinal)
        {
            [first.PreviewId] = "<svg viewBox=\"0 0 64 64\"><script></script><image href=\"https://example.invalid/a.png\" /></svg>"
        };

        var scenarios = new List<VisualMicrotileNegativeScenario>
        {
            Invalid("absolute_output_path", "absolute output path", baseline with { OutputRelativeDirectory = "C:/unsafe/out" }),
            Invalid("prompt_text_as_source_of_truth", "prompt text as source of truth", baseline with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }),
            Invalid("missing_palette", "missing palette profile and swatches", ReplacePreview(baseline, first with { PaletteProfileId = "", Palette = [] })),
            Invalid("missing_layer_stack", "missing layer stack", ReplacePreview(baseline, first with { LayerStack = [] })),
            Invalid("coast_without_water_land_adjacency", "coast tile without water/land adjacency", ReplacePreview(baseline, coast with { WaterLandAdjacency = null })),
            Invalid("river_without_flow_connectors", "river tile without flow connectors", ReplacePreview(baseline, river with { FlowConnectors = [] })),
            Invalid("adult_capable_without_safe_fallback", "adult-capable slot without safe fallback", ReplacePreview(baseline, adult with { SafeFallbackPreviewId = "" })),
            Invalid("provider_candidate_treated_as_approved_output", "provider candidate treated as approved output", ReplacePreview(baseline, adult with { TreatProviderCandidateAsApprovedOutput = true })),
            Invalid("missing_seed", "non-deterministic or missing seed", ReplacePreview(baseline, first with { Seed = 0 })),
            Invalid("svg_with_script_external_resource_base64", "unsafe SVG content", baseline, unsafeSvgs),
            Invalid("duplicate_preview_id", "duplicate preview id", baseline with { Previews = [.. baseline.Previews, first] }),
            Invalid("missing_goal084_085_lineage", "missing source Goal084/085 lineage", ReplacePreview(baseline, first with { SourceGoal084SlotId = "", SourceGoal085PackId = "" }))
        };

        return new VisualMicrotileNegativeProof
        {
            Passed = scenarios.Count >= 12 && scenarios.All(item => item.ExpectedValid == item.ActualValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualMicrotilePreviewCatalog BuildPreviewCatalog(VisualMicrotileMaterializationRequest request)
    {
        var entries = request.Previews
            .OrderBy(item => item.PreviewId, StringComparer.Ordinal)
            .Select(item => new VisualMicrotilePreviewCatalogEntry
            {
                PreviewId = item.PreviewId,
                Category = item.Category,
                PreviewRelativePath = item.PreviewRelativePath,
                PartPackId = item.PartPackId,
                AssetSlotId = item.AssetSlotId,
                PaletteProfileId = item.PaletteProfileId,
                Seed = item.Seed,
                BiomeRuleId = item.BiomeRuleId,
                WaterRuleId = item.WaterRuleId,
                AdultMetadataOnly = item.AdultMetadataOnly,
                SafeFallbackPreviewId = item.SafeFallbackPreviewId
            })
            .ToList();

        return new VisualMicrotilePreviewCatalog
        {
            Accepted = false,
            PreviewCount = entries.Count,
            CategoryCoverage = entries
                .GroupBy(item => item.Category)
                .OrderBy(item => item.Key)
                .Select(item => new VisualMicrotileCategoryCoverage { Category = item.Key, Count = item.Count() })
                .ToList(),
            Previews = entries
        };
    }

    private static VisualMicrotileMaterializationManifest BuildMaterializationManifest(
        VisualMicrotileMaterializationRequest request,
        IReadOnlyDictionary<string, string> svgByPreviewId) =>
        new()
        {
            GeneratorVersion = request.GeneratorVersion,
            OutputRelativeDirectory = request.OutputRelativeDirectory,
            PreviewCount = request.Previews.Count,
            Previews = request.Previews
                .OrderBy(item => item.PreviewId, StringComparer.Ordinal)
                .Select(item =>
                {
                    var svg = svgByPreviewId[item.PreviewId];
                    return new VisualMicrotileMaterializedPreview
                    {
                        PreviewId = item.PreviewId,
                        PreviewRelativePath = item.PreviewRelativePath,
                        SvgSha256 = DeterministicVisualMicrotileMaterializerHash.Compute(svg),
                        ByteLength = Encoding.UTF8.GetByteCount(svg),
                        ContainsViewBox = svg.Contains("viewBox=", StringComparison.Ordinal),
                        ExternalResourceFree = DeterministicVisualMicrotileMaterializerValidator.IsSvgSafe(svg),
                        ScriptFree = !svg.Contains("<script", StringComparison.OrdinalIgnoreCase),
                        LayerCount = item.LayerStack.Count,
                        GeneratedShapeCount = DeterministicVisualMicrotileMaterializerValidator.CountGeneratedShapes(svg) - 1
                    };
                })
                .ToList()
        };

    private static VisualMicrotileWaterBiomeProof BuildWaterBiomeProof(VisualMicrotileMaterializationRequest request)
    {
        bool Has(string id) => request.Previews.Any(item => item.PreviewId == id);
        var proof = new VisualMicrotileWaterBiomeProof
        {
            GrassOverworldCovered = Has("terrain_grass_overworld"),
            SnowCovered = Has("terrain_snow_tundra"),
            DesertDryCovered = Has("terrain_desert_dry"),
            LavaAshCovered = Has("terrain_lava_ash"),
            ForestOverlayCovered = Has("terrain_forest_overlay"),
            MountainRockCovered = Has("terrain_mountain_rock"),
            WaterBaseCovered = Has("water_base"),
            CoastTransitionCovered = Has("water_coast_transition"),
            RiverSegmentCovered = Has("water_river_segment"),
            LakeEdgeCovered = Has("water_lake_edge"),
            MarshSwampCovered = Has("water_marsh_swamp"),
            BridgeDockAnchorMetadataCovered = Has("water_bridge_dock_anchor")
        };

        return proof with
        {
            Passed = proof.GrassOverworldCovered
                && proof.SnowCovered
                && proof.DesertDryCovered
                && proof.LavaAshCovered
                && proof.ForestOverlayCovered
                && proof.MountainRockCovered
                && proof.WaterBaseCovered
                && proof.CoastTransitionCovered
                && proof.RiverSegmentCovered
                && proof.LakeEdgeCovered
                && proof.MarshSwampCovered
                && proof.BridgeDockAnchorMetadataCovered
        };
    }

    private static VisualMicrotileLayeringProof BuildLayeringProof(VisualMicrotileMaterializationRequest request)
    {
        var rows = request.Previews
            .OrderBy(item => item.PreviewId, StringComparer.Ordinal)
            .Select(item => new VisualMicrotileLayeringProofRow
            {
                PreviewId = item.PreviewId,
                LayerCount = item.LayerStack.Count,
                LayerOrders = item.LayerStack.OrderBy(layer => layer.Order).Select(layer => layer.Order).ToList(),
                PaletteProfileId = item.PaletteProfileId,
                MaskCount = item.MaskIds.Count,
                SocketCount = item.SocketIds.Count,
                AnchorCount = item.AnchorIds.Count
            })
            .ToList();
        var ordersStable = request.Previews.All(item =>
            item.LayerStack.Count > 0
            && item.LayerStack.Select(layer => layer.Order).Distinct().Count() == item.LayerStack.Count);
        var bindingsPresent = rows.All(item =>
            !string.IsNullOrWhiteSpace(item.PaletteProfileId)
            && item.MaskCount > 0
            && item.SocketCount > 0
            && item.AnchorCount > 0);

        return new VisualMicrotileLayeringProof
        {
            Passed = ordersStable && bindingsPresent && rows.Count == request.Previews.Count,
            PreviewCount = rows.Count,
            AllPreviewLayerOrderingStable = ordersStable,
            AllPreviewsUsePaletteMasksSocketsAndAnchors = bindingsPresent,
            Rows = rows
        };
    }

    private static VisualMicrotileSourceLineage BuildSourceLineage(string? projectRootPath)
    {
        var records = SourceInputs()
            .Select(item => BuildSourceRecord(projectRootPath, item.Path, item.Tags))
            .ToList();
        var goal084Report = ReadText(projectRootPath, ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md");
        var goal084Catalog = ReadText(projectRootPath, ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json");
        var goal085Report = ReadText(projectRootPath, ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md");
        var goal085Catalog = ReadText(projectRootPath, ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-catalog.json");
        var goal084Green = goal084Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal);
        var goal084AcceptedFalse = goal084Report.Contains("accepted: false", StringComparison.Ordinal)
            && goal084Catalog.Contains("\"accepted\": false", StringComparison.Ordinal);
        var goal085Green = goal085Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal);
        var goal085AcceptedFalse = goal085Report.Contains("accepted: false", StringComparison.Ordinal)
            && goal085Catalog.Contains("\"accepted\": false", StringComparison.Ordinal);
        var deepsearchDocsExist = records
            .Where(item => item.RelativePath.StartsWith("docs/deepsearch/", StringComparison.Ordinal))
            .Count(item => item.Exists) == 8;
        var synthesisExists = records.Any(item => item.RelativePath == "docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md" && item.Exists);

        return new VisualMicrotileSourceLineage
        {
            Passed = records.All(item => item.Exists)
                && goal084Green
                && goal084AcceptedFalse
                && goal085Green
                && goal085AcceptedFalse
                && deepsearchDocsExist
                && synthesisExists,
            Goal084ArtifactsGreen = goal084Green,
            Goal084AcceptedFalse = goal084AcceptedFalse,
            Goal085ArtifactsGreen = goal085Green,
            Goal085AcceptedFalse = goal085AcceptedFalse,
            DeepsearchDocsExist = deepsearchDocsExist,
            SynthesisExists = synthesisExists,
            SourceRecordCount = records.Count,
            Records = records
        };
    }

    private static VisualMicrotileFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> jsonFiles,
        VisualMicrotileMaterializationRequest request,
        IReadOnlyDictionary<string, string> svgByPreviewId)
    {
        var entries = new List<VisualMicrotileFileLedgerEntry>();
        foreach (var file in jsonFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            entries.Add(LedgerEntry(file.Key, "json_manifest", file.Value));
        }

        foreach (var preview in request.Previews.OrderBy(item => item.PreviewId, StringComparer.Ordinal))
        {
            entries.Add(LedgerEntry(preview.PreviewRelativePath, "preview_svg", svgByPreviewId[preview.PreviewId]));
        }

        return new VisualMicrotileFileLedger
        {
            Passed = entries.Count == jsonFiles.Count + request.Previews.Count
                && entries.All(item => item.Sha256.Length == 64)
                && entries.All(item => item.ByteLength > 0),
            FileCount = entries.Count,
            Files = entries
        };
    }

    private static VisualMicrotileFileLedgerEntry LedgerEntry(string relativePathUnderOutput, string role, string text)
    {
        var repoRelativePath = DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory + "/" + relativePathUnderOutput;
        return new VisualMicrotileFileLedgerEntry
        {
            RelativePath = repoRelativePath,
            Role = role,
            Sha256 = DeterministicVisualMicrotileMaterializerHash.Compute(text),
            ByteLength = Encoding.UTF8.GetByteCount(text)
        };
    }

    private static VisualMicrotileReport BuildReport(
        VisualMicrotilePreviewCatalog catalog,
        VisualMicrotileValidationResult validation,
        VisualMicrotileNegativeProof negativeProof,
        VisualMicrotileQualityGateScan qualityGate,
        string previewCatalogJson,
        string materializationManifestJson,
        string fileLedgerJson,
        string waterProofJson,
        string layeringProofJson,
        string negativeProofJson,
        string qualityGateJson,
        string sourceLineageJson) =>
        new()
        {
            Accepted = false,
            PreviewCount = catalog.PreviewCount,
            TerrainBiomePreviewCount = CountCategory(catalog, VisualMicrotileCategory.TerrainBiome),
            WaterPreviewCount = CountCategory(catalog, VisualMicrotileCategory.Water),
            SettlementPreviewCount = CountCategory(catalog, VisualMicrotileCategory.SettlementStructure),
            CreaturePreviewCount = CountCategory(catalog, VisualMicrotileCategory.CreatureNpc),
            UiEffectPreviewCount = CountCategory(catalog, VisualMicrotileCategory.UiEffect),
            AdultMetadataOnlyPreviewCount = CountCategory(catalog, VisualMicrotileCategory.AdultRating),
            ValidationPassed = validation.Passed,
            NegativeProofPassed = negativeProof.Passed,
            QualityGatePassed = qualityGate.Diagnostics.All(item => item.Severity != "error"),
            PreviewCatalogHash = DeterministicVisualMicrotileMaterializerHash.Compute(previewCatalogJson),
            MaterializationManifestHash = DeterministicVisualMicrotileMaterializerHash.Compute(materializationManifestJson),
            FileLedgerHash = DeterministicVisualMicrotileMaterializerHash.Compute(fileLedgerJson),
            WaterBiomeProofHash = DeterministicVisualMicrotileMaterializerHash.Compute(waterProofJson),
            LayeringProofHash = DeterministicVisualMicrotileMaterializerHash.Compute(layeringProofJson),
            NegativeProofHash = DeterministicVisualMicrotileMaterializerHash.Compute(negativeProofJson),
            QualityGateHash = DeterministicVisualMicrotileMaterializerHash.Compute(qualityGateJson),
            SourceLineageHash = DeterministicVisualMicrotileMaterializerHash.Compute(sourceLineageJson)
        };

    private static int CountCategory(VisualMicrotilePreviewCatalog catalog, VisualMicrotileCategory category) =>
        catalog.CategoryCoverage.FirstOrDefault(item => item.Category == category)?.Count ?? 0;

    private static string RenderReport(
        VisualMicrotileReport report,
        VisualMicrotilePreviewCatalog catalog,
        VisualMicrotileWaterBiomeProof waterProof,
        VisualMicrotileNegativeProof negativeProof,
        VisualMicrotileQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 086 Visual Microtile Materializer Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 086 adds a BCL-only Application-side deterministic visual microtile materializer. It consumes Goal 084 visual asset slots and Goal 085 part-pack rule stack lineage, then writes text SVG previews plus compact JSON manifests. It does not add dependencies, provider calls, Runtime behavior, Unity behavior, public GamePackage schema changes, binary media, real adult content or prompt dumps.",
            string.Empty,
            "## Preview Coverage",
            string.Empty,
            $"- previewCount: {report.PreviewCount}",
            $"- terrainBiomePreviewCount: {report.TerrainBiomePreviewCount}",
            $"- waterPreviewCount: {report.WaterPreviewCount}",
            $"- settlementPreviewCount: {report.SettlementPreviewCount}",
            $"- creaturePreviewCount: {report.CreaturePreviewCount}",
            $"- uiEffectPreviewCount: {report.UiEffectPreviewCount}",
            $"- adultMetadataOnlyPreviewCount: {report.AdultMetadataOnlyPreviewCount}",
            string.Empty
        };
        lines.AddRange(catalog.Previews.Select(item => $"- {item.PreviewId}: {item.PreviewRelativePath}"));
        lines.AddRange(
        [
            string.Empty,
            "## Water And Biome Proof",
            string.Empty,
            $"- passed: {waterProof.Passed.ToString().ToLowerInvariant()}",
            $"- grassOverworld: {waterProof.GrassOverworldCovered.ToString().ToLowerInvariant()}",
            $"- snow: {waterProof.SnowCovered.ToString().ToLowerInvariant()}",
            $"- desertDry: {waterProof.DesertDryCovered.ToString().ToLowerInvariant()}",
            $"- lavaAsh: {waterProof.LavaAshCovered.ToString().ToLowerInvariant()}",
            $"- forestOverlay: {waterProof.ForestOverlayCovered.ToString().ToLowerInvariant()}",
            $"- mountainRock: {waterProof.MountainRockCovered.ToString().ToLowerInvariant()}",
            $"- waterBase: {waterProof.WaterBaseCovered.ToString().ToLowerInvariant()}",
            $"- coastTransition: {waterProof.CoastTransitionCovered.ToString().ToLowerInvariant()}",
            $"- riverSegment: {waterProof.RiverSegmentCovered.ToString().ToLowerInvariant()}",
            $"- lakeEdge: {waterProof.LakeEdgeCovered.ToString().ToLowerInvariant()}",
            $"- marshSwamp: {waterProof.MarshSwampCovered.ToString().ToLowerInvariant()}",
            $"- bridgeDockAnchorMetadata: {waterProof.BridgeDockAnchorMetadataCovered.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validationPassed: {report.ValidationPassed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- svgTextOnlyPreviews: {qualityGate.SvgTextOnlyPreviews.ToString().ToLowerInvariant()}",
            $"- noExternalDependenciesAdded: {qualityGate.NoExternalDependenciesAdded.ToString().ToLowerInvariant()}",
            $"- noBinaryMediaAdded: {qualityGate.NoBinaryMediaAdded.ToString().ToLowerInvariant()}",
            $"- noProviderCalls: {qualityGate.NoProviderCalls.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noExplicitAdultContent: {qualityGate.NoExplicitAdultContent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- previewCatalogHash: {report.PreviewCatalogHash}",
            $"- materializationManifestHash: {report.MaterializationManifestHash}",
            $"- fileLedgerHash: {report.FileLedgerHash}",
            $"- waterBiomeProofHash: {report.WaterBiomeProofHash}",
            $"- layeringProofHash: {report.LayeringProofHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- qualityGateHash: {report.QualityGateHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static VisualMicrotileNegativeScenario Invalid(
        string id,
        string mutation,
        VisualMicrotileMaterializationRequest request,
        IReadOnlyDictionary<string, string>? svgByPreviewId = null)
    {
        var validation = DeterministicVisualMicrotileMaterializerValidator.Validate(request, svgByPreviewId);
        return new VisualMicrotileNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static VisualMicrotileMaterializationRequest ReplacePreview(
        VisualMicrotileMaterializationRequest request,
        VisualMicrotilePreviewSpec replacement) =>
        request with
        {
            Previews = request.Previews.Select(item => item.PreviewId == replacement.PreviewId ? replacement : item).ToList()
        };

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> SourceInputs() =>
    [
        (".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md", ["goal084", "report"]),
        (".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json", ["goal084", "catalog"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md", ["goal085", "report"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-catalog.json", ["goal085", "catalog"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/water-biome-coverage-matrix.json", ["goal085", "water"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-validation-matrix.json", ["goal085", "validation"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-negative-proof.json", ["goal085", "negative_proof"]),
        ("docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md", ["deepsearch", "synthesis"]),
        ("docs/deepsearch/01_PROCEDURAL_VISUAL_SYNTHESIS_CORE_AND_PART_PACKS.md", ["deepsearch", "part_pack_core"]),
        ("docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md", ["deepsearch", "water_biome"]),
        ("docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md", ["deepsearch", "pseudo3d"]),
        ("docs/deepsearch/04_CREATURE_NPC_APPEARANCE_BODYPLAN_PAPERDOLL.md", ["deepsearch", "creature"]),
        ("docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md", ["deepsearch", "settlement"]),
        ("docs/deepsearch/06_UI_THEMES_EFFECTS_WEATHER_DAYNIGHT_VFX.md", ["deepsearch", "ui_effect"]),
        ("docs/deepsearch/07_MEDIA_PIPELINE_PROVIDER_QUARANTINE_PROVENANCE_RATING_ADULT.md", ["deepsearch", "adult_rating"]),
        ("docs/deepsearch/08_EXISTING_LIBRARIES_AND_TOOLS_SCOUTING.md", ["deepsearch", "libraries"])
    ];

    private static VisualMicrotileSourceLineageRecord BuildSourceRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new VisualMicrotileSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : DeterministicVisualMicrotileMaterializerHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static string RenderSvg(VisualMicrotilePreviewSpec preview)
    {
        var background = Color(preview, "background");
        var primary = Color(preview, "primary");
        var secondary = Color(preview, "secondary");
        var accent = Color(preview, "accent");
        var jitterA = Jitter(preview, "a", -4, 4);
        var jitterB = Jitter(preview, "b", -5, 5);
        var jitterC = Jitter(preview, "c", -3, 3);
        var shapeOne = preview.Category switch
        {
            VisualMicrotileCategory.SettlementStructure => $"<rect x=\"{12 + jitterA}\" y=\"{22 + jitterC}\" width=\"{40 + jitterB}\" height=\"{28 - jitterC}\" rx=\"2\" fill=\"{primary}\" />",
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating => $"<ellipse cx=\"{32 + jitterA}\" cy=\"{34 + jitterC}\" rx=\"{13 + Math.Abs(jitterB)}\" ry=\"{20 + Math.Abs(jitterC)}\" fill=\"{primary}\" />",
            VisualMicrotileCategory.UiEffect => $"<rect x=\"{10 + jitterA}\" y=\"{12 + jitterC}\" width=\"{44 - jitterA}\" height=\"{38 + jitterC}\" rx=\"4\" fill=\"{primary}\" />",
            VisualMicrotileCategory.Water => $"<path d=\"M 0 {30 + jitterA} C 14 {20 + jitterB}, 28 {42 + jitterC}, 64 {28 - jitterB} L 64 64 L 0 64 Z\" fill=\"{primary}\" />",
            _ => $"<polygon points=\"0,{64 - Math.Abs(jitterA)} 18,{34 + jitterB} 34,{48 + jitterC} 64,{28 - jitterA} 64,64\" fill=\"{primary}\" />"
        };
        var shapeTwo = preview.Category switch
        {
            VisualMicrotileCategory.SettlementStructure => $"<polygon points=\"{10 + jitterA},{23 + jitterC} 32,{8 + jitterB} {55 - jitterA},{23 + jitterC}\" fill=\"{secondary}\" />",
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating => $"<circle cx=\"{32 + jitterB}\" cy=\"{16 + jitterC}\" r=\"{8 + Math.Abs(jitterA)}\" fill=\"{secondary}\" />",
            VisualMicrotileCategory.UiEffect => $"<rect x=\"{17 + jitterC}\" y=\"{19 + jitterA}\" width=\"{30 + jitterB}\" height=\"{24 - jitterC}\" rx=\"2\" fill=\"{secondary}\" />",
            VisualMicrotileCategory.Water => $"<path d=\"M 2 {42 + jitterC} C 20 {34 - jitterB}, 34 {48 + jitterA}, 62 {39 - jitterC}\" fill=\"none\" stroke=\"{secondary}\" stroke-width=\"4\" />",
            _ => $"<circle cx=\"{24 + jitterA}\" cy=\"{26 + jitterB}\" r=\"{9 + Math.Abs(jitterC)}\" fill=\"{secondary}\" />"
        };
        var shapeThree = preview.Category switch
        {
            VisualMicrotileCategory.SettlementStructure => $"<rect x=\"{28 + jitterC}\" y=\"{36 + jitterA}\" width=\"9\" height=\"14\" fill=\"{accent}\" />",
            VisualMicrotileCategory.CreatureNpc => $"<path d=\"M {20 + jitterA} {48 + jitterC} L {32 + jitterB} {37 + jitterA} L {45 - jitterA} {48 + jitterC}\" fill=\"none\" stroke=\"{accent}\" stroke-width=\"4\" />",
            VisualMicrotileCategory.AdultRating => $"<path d=\"M {18 + jitterA} {33 + jitterC} L {32 + jitterB} {20 + jitterA} L {46 - jitterA} {33 + jitterC} L {32 + jitterB} {50 - jitterC} Z\" fill=\"none\" stroke=\"{accent}\" stroke-width=\"4\" />",
            VisualMicrotileCategory.UiEffect => $"<circle cx=\"{48 + jitterA}\" cy=\"{16 + jitterB}\" r=\"{6 + Math.Abs(jitterC)}\" fill=\"{accent}\" />",
            VisualMicrotileCategory.Water => $"<polyline points=\"{12 + jitterA},{22 + jitterC} {25 + jitterB},{26 - jitterA} {38 - jitterC},{22 + jitterB} {52 - jitterA},{25 + jitterC}\" fill=\"none\" stroke=\"{accent}\" stroke-width=\"3\" />",
            _ => $"<path d=\"M {8 + jitterA} {18 + jitterC} Q {31 + jitterB} {8 + jitterA} {56 - jitterC} {20 + jitterB}\" fill=\"none\" stroke=\"{accent}\" stroke-width=\"3\" />"
        };

        var layers = preview.LayerStack.OrderBy(item => item.Order).ToList();
        return string.Join(Environment.NewLine,
        [
            $"<svg viewBox=\"{preview.ViewBox}\" data-preview-id=\"{Escape(preview.PreviewId)}\" data-part-pack-id=\"{Escape(preview.PartPackId)}\" data-asset-slot-id=\"{Escape(preview.AssetSlotId)}\" data-palette-profile-id=\"{Escape(preview.PaletteProfileId)}\" data-seed=\"{preview.Seed}\">",
            $"  <rect x=\"0\" y=\"0\" width=\"64\" height=\"64\" fill=\"{background}\" />",
            $"  <g id=\"{SvgId(layers[0].LayerId)}\" data-order=\"{layers[0].Order}\" data-role=\"{Escape(layers[0].Role)}\">",
            $"    {shapeOne}",
            "  </g>",
            $"  <g id=\"{SvgId(layers[1].LayerId)}\" data-order=\"{layers[1].Order}\" data-role=\"{Escape(layers[1].Role)}\">",
            $"    {shapeTwo}",
            "  </g>",
            $"  <g id=\"{SvgId(layers[2].LayerId)}\" data-order=\"{layers[2].Order}\" data-role=\"{Escape(layers[2].Role)}\">",
            $"    {shapeThree}",
            "  </g>",
            "</svg>",
            string.Empty
        ]);
    }

    private static string Color(VisualMicrotilePreviewSpec preview, string slot) =>
        preview.Palette.FirstOrDefault(item => item.SlotId == slot)?.HexColor ?? "#808080";

    private static int Jitter(VisualMicrotilePreviewSpec preview, string salt, int minInclusive, int maxInclusive) =>
        DeterministicVisualMicrotileMaterializerHash.StableInt(
            preview.PreviewId + "|" + preview.PartPackId + "|" + preview.AssetSlotId + "|" + preview.Seed + "|" + salt,
            minInclusive,
            maxInclusive);

    private static string SvgId(string id) =>
        id.Replace('/', '_').Replace('.', '_');

    private static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

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
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
