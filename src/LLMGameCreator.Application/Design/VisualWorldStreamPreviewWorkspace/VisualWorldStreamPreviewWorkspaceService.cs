using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceService
{
    public const string ReportMarkdownFileName = "visual-world-stream-preview-workspace-report.md";
    public const string CatalogJsonFileName = "visual-world-stream-preview-catalog.json";
    public const string ProofStatusJsonFileName = "visual-world-stream-preview-proof-status.json";
    public const string WinFormsBindingInventoryJsonFileName =
        "visual-world-stream-preview-winforms-binding-inventory.json";
    public const string QualityGateScanJsonFileName =
        "visual-world-stream-preview-quality-gate-scan.json";

    private const int MaxPreviewCharacters = 32000;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualWorldStreamPreviewWorkspaceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualWorldStreamPreviewWorkspaceResult Build(string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var svgEntries = new List<VisualWorldPreviewSvgEntry>();
        var groups = new List<VisualWorldPreviewArtifactGroup>
        {
            BuildMicrotileGroup(projectRoot, diagnostics, svgEntries),
            BuildMapPatchGroup(projectRoot, diagnostics, svgEntries),
            BuildRegionGroup(projectRoot, diagnostics, svgEntries),
            BuildWorldProfileGroup(projectRoot, diagnostics, svgEntries),
            BuildChunkStreamGroup(projectRoot, diagnostics, svgEntries)
        };

        var proofStatus = BuildProofStatus(projectRoot, diagnostics);
        var bindingInventory = BuildWinFormsBindingInventory(projectRoot);
        var qualityGate = BuildQualityGate(groups, svgEntries, proofStatus, bindingInventory, diagnostics);
        var catalog = new VisualWorldStreamPreviewCatalog
        {
            Accepted = false,
            GroupCount = groups.Count,
            EntryCount = groups.Sum(group => group.EntryCount),
            SvgTextPreviewCount = svgEntries.Count,
            Groups = groups,
            SvgEntries = svgEntries.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList()
        };

        var proofDocument = new VisualWorldStreamPreviewProofStatusDocument
        {
            Passed = proofStatus.All(item => item.Passed),
            ProofCount = proofStatus.Count,
            Proofs = proofStatus
        };

        var catalogJson = Serialize(catalog);
        var proofStatusJson = Serialize(proofDocument);
        var bindingJson = Serialize(bindingInventory);
        var qualityJson = Serialize(qualityGate);
        var reportWithoutHash = BuildReport(
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            catalogJson,
            proofStatusJson,
            bindingJson,
            qualityJson);
        var reportMarkdownWithoutHash = RenderReport(
            reportWithoutHash,
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            deterministicReportHash: string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Sha256Text(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(
            report,
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            report.DeterministicReportHash);

        return new VisualWorldStreamPreviewWorkspaceResult
        {
            Catalog = catalog,
            ProofStatus = proofDocument,
            WinFormsBindingInventory = bindingInventory,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            ProofStatusJson = proofStatusJson,
            WinFormsBindingInventoryJson = bindingJson,
            QualityGateScanJson = qualityJson,
            ReportMarkdown = reportMarkdown,
            Diagnostics = diagnostics
                .Concat(bindingInventory.Diagnostics)
                .Concat(qualityGate.Diagnostics)
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> BuildAndWriteAsync(
        string sourceRootPath,
        string outputRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(sourceRootPath);
        return await WriteAsync(outputRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldStreamPreviewWorkspaceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualWorldStreamPreviewWorkspaceVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualWorldStreamPreviewWorkspaceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            ProofStatusJsonPath = Path.Combine(outputDirectory, ProofStatusJsonFileName),
            WinFormsBindingInventoryJsonPath =
                Path.Combine(outputDirectory, WinFormsBindingInventoryJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            Result = result
        };

        await File.WriteAllTextAsync(
            write.ReportMarkdownPath,
            result.ReportMarkdown,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.CatalogJsonPath,
            result.CatalogJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.ProofStatusJsonPath,
            result.ProofStatusJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.WinFormsBindingInventoryJsonPath,
            result.WinFormsBindingInventoryJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.QualityGateScanJsonPath,
            result.QualityGateScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return write;
    }

    public VisualWorldPreviewWinFormsBindingInventory BuildWinFormsBindingInventory(
        string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs";
        var designerRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Designer.cs";
        var compositionRelativePath = "src/LLMGameCreator.WinForms/CompositionRoot.cs";
        var pageText = ReadOptionalText(projectRoot, pageRelativePath);
        var designerText = ReadOptionalText(projectRoot, designerRelativePath);
        var compositionText = ReadOptionalText(projectRoot, compositionRelativePath);
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();

        var pageExists = pageText.Length > 0;
        var designerExists = designerText.Length > 0;
        var serviceRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspaceService",
            StringComparison.Ordinal);
        var pageRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspacePageControl",
            StringComparison.Ordinal);
        var registryIncludesPage = compositionText.Contains(
            "resolver.Resolve<VisualWorldStreamPreviewWorkspacePageControl>()",
            StringComparison.Ordinal);
        var activationLoads = pageText.Contains("BuildAndWriteAsync(root)", StringComparison.Ordinal)
            && pageText.Contains("Bind(write.Result)", StringComparison.Ordinal);
        var bindDisplays = pageText.Contains("_groupsListBox", StringComparison.Ordinal)
            && pageText.Contains("_entriesListView", StringComparison.Ordinal)
            && pageText.Contains("_proofsListView", StringComparison.Ordinal)
            && pageText.Contains("_svgPreviewTextBox", StringComparison.Ordinal);

        AddIfFalse(pageExists, "goal092.winforms.page_missing", pageRelativePath, diagnostics);
        AddIfFalse(designerExists, "goal092.winforms.designer_missing", designerRelativePath, diagnostics);
        AddIfFalse(serviceRegistered, "goal092.winforms.service_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(pageRegistered, "goal092.winforms.page_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(registryIncludesPage, "goal092.winforms.registry_missing", compositionRelativePath, diagnostics);
        AddIfFalse(activationLoads, "goal092.winforms.activation_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplays, "goal092.winforms.bind_missing", pageRelativePath, diagnostics);

        return new VisualWorldPreviewWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            PageControlExists = pageExists,
            DesignerExists = designerExists,
            CompositionRootRegistersService = serviceRegistered,
            CompositionRootRegistersPage = pageRegistered,
            EditorRegistryIncludesPage = registryIncludesPage,
            PageActivationLoadsApplicationResult = activationLoads,
            PageBindDisplaysGroupsEntriesProofs = bindDisplays,
            Diagnostics = diagnostics
        };
    }

    private static VisualWorldPreviewArtifactGroup BuildMicrotileGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_086_deterministic_visual_microtile_materializer";
        const string sourceRoot =
            ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-microtile-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-microtile-materializer-report.md", "report"),
                ("visual-microtile-preview-catalog.json", "catalog"),
                ("visual-microtile-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-microtile-preview-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "previews", out var previews))
        {
            foreach (var preview in previews.OrderBy(
                item => TryGetString(item, "previewId"),
                StringComparer.Ordinal))
            {
                var previewId = TryGetString(preview, "previewId");
                var relativePath = sourceRoot + "/" + TryGetString(preview, "previewRelativePath");
                var metadata = "category=" + TryGetString(preview, "category")
                    + "; adultMetadataOnly=" + TryGetBool(preview, "adultMetadataOnly")
                    + "; safeFallback=" + TryGetString(preview, "safeFallbackPreviewId");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    previewId,
                    relativePath,
                    "text_svg_microtile_preview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.microtiles.catalog_missing_previews",
                sourceRoot + "/visual-microtile-preview-catalog.json",
                "Goal 086 preview catalog must expose preview entries."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("microtiles", "Goal 086 Microtiles", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildMapPatchGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_087_deterministic_visual_map_patch_composer";
        const string sourceRoot =
            ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-map-patch-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-map-patch-composer-report.md", "report"),
                ("visual-map-patch-catalog.json", "catalog"),
                ("visual-map-patch-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        foreach (var path in EnumerateExistingFiles(projectRoot, sourceRoot + "/patches", "*.svg"))
        {
            var relativePath = Relative(projectRoot, path);
            AddSvgEntry(
                projectRoot,
                entries,
                svgEntries,
                sourceGoalId,
                Path.GetFileNameWithoutExtension(path),
                relativePath,
                "text_svg_map_patch_preview",
                "textSvg=true; raster=false; providerOutput=false",
                ledger,
                groupDiagnostics);
        }

        if (!entries.Any(item => item.ArtifactKind == "text_svg_map_patch_preview"))
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.map_patches.svg_missing",
                sourceRoot + "/patches",
                "Goal 087 must expose text SVG patch previews."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("map_patches", "Goal 087 Map Patches", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildRegionGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_088_deterministic_visual_region_composer";
        const string sourceRoot =
            ".llmgc/procedural/goal-088-deterministic-visual-region-composer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-region-composer-report.md", "report"),
                ("visual-region-definition.json", "definition"),
                ("visual-region-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        foreach (var path in EnumerateExistingFiles(projectRoot, sourceRoot, "region-overview-*.svg"))
        {
            var relativePath = Relative(projectRoot, path);
            AddSvgEntry(
                projectRoot,
                entries,
                svgEntries,
                sourceGoalId,
                Path.GetFileNameWithoutExtension(path),
                relativePath,
                "text_svg_region_overview",
                "safeSvgOverview=true; compactRegionSummary=true",
                ledger,
                groupDiagnostics);
        }

        if (!entries.Any(item => item.ArtifactKind == "text_svg_region_overview"))
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.region.svg_missing",
                sourceRoot,
                "Goal 088 must expose text SVG region overview files."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("region_composer", "Goal 088 Region Composer", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildWorldProfileGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_090_parameterized_visual_world_profiles";
        const string sourceRoot =
            ".llmgc/procedural/goal-090-parameterized-visual-world-profiles";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-world-profile-report.md", "report"),
                ("visual-world-profile-catalog.json", "catalog"),
                ("visual-world-profile-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-world-profile-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "profiles", out var profiles))
        {
            foreach (var profile in profiles.OrderBy(
                item => TryGetString(item, "profileId"),
                StringComparer.Ordinal))
            {
                var profileId = TryGetString(profile, "profileId");
                var relativePath = sourceRoot + "/profile-overviews/" + profileId + ".svg";
                var metadata = "mode=" + TryGetString(profile, "mode")
                    + "; infinite=" + TryGetBool(profile, "isInfinite")
                    + "; rawCellDumpAllowed=" + TryGetBool(profile, "rawCellDumpAllowed");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    profileId,
                    relativePath,
                    "text_svg_world_profile_overview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.world_profiles.catalog_missing_profiles",
                sourceRoot + "/visual-world-profile-catalog.json",
                "Goal 090 profile catalog must expose profile entries."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("world_profiles", "Goal 090 World Profiles", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildChunkStreamGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_091_deterministic_visual_chunk_stream_window";
        const string sourceRoot =
            ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-chunk-stream-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-chunk-stream-window-report.md", "report"),
                ("visual-chunk-stream-window-catalog.json", "catalog"),
                ("visual-chunk-stream-quality-gate-scan.json", "quality_gate"),
                ("visual-chunk-stream-materialization-manifest.json", "materialization_manifest")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-chunk-stream-window-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "fixtures", out var fixtures))
        {
            foreach (var fixture in fixtures.OrderBy(
                item => TryGetString(item, "fixtureId"),
                StringComparer.Ordinal))
            {
                var fixtureId = TryGetString(fixture, "fixtureId");
                var relativePath = sourceRoot + "/" + TryGetString(fixture, "overviewSvgRelativePath");
                var metadata = "profile=" + TryGetString(fixture, "profileId")
                    + "; mode=" + TryGetString(fixture, "mode")
                    + "; windows=" + TryGetInt(fixture, "windowCount")
                    + "; chunks=" + TryGetInt(fixture, "totalMaterializedChunks");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    fixtureId,
                    relativePath,
                    "text_svg_chunk_stream_window_overview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.chunk_stream.catalog_missing_fixtures",
                sourceRoot + "/visual-chunk-stream-window-catalog.json",
                "Goal 091 stream catalog must expose fixtures."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "chunk_stream_windows",
            "Goal 091 Chunk Stream Windows",
            sourceGoalId,
            sourceRoot,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        const string sourceGoalId = "goal_091_deterministic_visual_chunk_stream_window";
        const string sourceRoot =
            ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-chunk-stream-file-ledger.json");
        var proofDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var proofs = new List<VisualWorldPreviewProofStatus>
        {
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.seam",
                "visual-chunk-stream-seam-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.cache_reuse",
                "visual-chunk-stream-cache-reuse-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.layer_transition",
                "visual-chunk-stream-layer-transition-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.negative",
                "visual-chunk-stream-negative-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.finite_boundary_clipping",
                "visual-chunk-stream-quality-gate-scan.json",
                "boundaryClippingExplicit",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.huge_sparse_no_raw_dump",
                "visual-chunk-stream-quality-gate-scan.json",
                "hugeSparseNoRawDump",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.infinite_overlap_reuse",
                "visual-chunk-stream-quality-gate-scan.json",
                "infiniteOverlapReuseProven",
                ledger,
                proofDiagnostics)
        };

        diagnostics.AddRange(proofDiagnostics);
        return proofs.OrderBy(item => item.ProofId, StringComparer.Ordinal).ToList();
    }

    private static VisualWorldPreviewArtifactGroup Group(
        string groupId,
        string displayName,
        string sourceGoalId,
        string sourceRoot,
        List<VisualWorldPreviewArtifactEntry> entries,
        IReadOnlyList<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var hasError = diagnostics.Any(item => item.Severity == "error");
        return new VisualWorldPreviewArtifactGroup
        {
            GroupId = groupId,
            DisplayName = displayName,
            SourceGoalId = sourceGoalId,
            SourceRootRelativePath = sourceRoot,
            Status = hasError || entries.Count == 0
                ? VisualWorldPreviewArtifactStatus.Failed
                : VisualWorldPreviewArtifactStatus.Passed,
            EntryCount = entries.Count,
            SvgEntryCount = entries.Count(item => !string.IsNullOrWhiteSpace(item.TextSvgPreviewPath)),
            Entries = entries.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal).ToList()
        };
    }

    private static List<VisualWorldPreviewArtifactEntry> BuildCoreEntries(
        string projectRoot,
        string sourceRoot,
        string sourceGoalId,
        IReadOnlyList<(string FileName, string Kind)> files,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var entries = new List<VisualWorldPreviewArtifactEntry>();
        foreach (var file in files)
        {
            var relativePath = sourceRoot + "/" + file.FileName;
            var fullPath = Resolve(projectRoot, relativePath);
            var exists = File.Exists(fullPath);
            if (!exists)
            {
                diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                    "goal092.artifact.missing",
                    relativePath,
                    "Required visual world source artifact was not found."));
            }

            entries.Add(new VisualWorldPreviewArtifactEntry
            {
                Id = sourceGoalId + "." + Path.GetFileNameWithoutExtension(file.FileName),
                RelativePath = relativePath,
                ArtifactKind = file.Kind,
                SourceGoalId = sourceGoalId,
                Sha256 = exists ? HashFor(projectRoot, relativePath, ledger) : string.Empty,
                Status = exists
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = exists ? "artifact exists" : "artifact missing",
                SafeRatingMetadataSummary = "sourceArtifact=true"
            });
        }

        return entries;
    }

    private static void AddSvgEntry(
        string projectRoot,
        List<VisualWorldPreviewArtifactEntry> entries,
        List<VisualWorldPreviewSvgEntry> svgEntries,
        string sourceGoalId,
        string id,
        string relativePath,
        string artifactKind,
        string metadataSummary,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(relativePath))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.invalid_catalog_entry",
                sourceGoalId,
                "SVG catalog entry must have an id and relative path."));
            return;
        }

        var fullPath = Resolve(projectRoot, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var safe = exists && IsSafeSvg(text);
        if (!exists)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.missing",
                relativePath,
                "Text SVG preview declared by source catalog is missing."));
        }
        else if (!safe)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.unsafe",
                relativePath,
                "Text SVG preview contains unsafe content for text display."));
        }

        var sha = exists ? HashFor(projectRoot, relativePath, ledger) : string.Empty;
        var entryId = sourceGoalId + "." + id;
        var preview = exists ? TruncatePreview(text) : string.Empty;
        entries.Add(new VisualWorldPreviewArtifactEntry
        {
            Id = entryId,
            RelativePath = relativePath,
            ArtifactKind = artifactKind,
            SourceGoalId = sourceGoalId,
            Sha256 = sha,
            Status = safe
                ? VisualWorldPreviewArtifactStatus.Passed
                : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = safe ? "safe text SVG preview" : "missing or unsafe text SVG",
            TextSvgPreviewPath = relativePath,
            SafeRatingMetadataSummary = metadataSummary,
            TextPreview = preview
        });
        svgEntries.Add(new VisualWorldPreviewSvgEntry
        {
            EntryId = entryId,
            SourceGoalId = sourceGoalId,
            RelativePath = relativePath,
            Sha256 = sha,
            ByteLength = exists ? Encoding.UTF8.GetByteCount(text) : 0,
            SafeToDisplayAsText = safe,
            SafetySummary = safe
                ? "text SVG contains no script, external URL or base64 payload"
                : "missing or unsafe SVG",
            PreviewText = preview
        });
    }

    private static VisualWorldPreviewProofStatus BuildProof(
        string projectRoot,
        string sourceRoot,
        string sourceGoalId,
        string proofId,
        string fileName,
        string booleanProperty,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var relativePath = sourceRoot + "/" + fileName;
        var passed = false;
        var summary = "proof missing";
        using var doc = TryReadJson(projectRoot, relativePath, diagnostics);
        if (doc is not null)
        {
            passed = TryGetBool(doc.RootElement, booleanProperty);
            summary = BuildProofSummary(doc.RootElement, booleanProperty, passed);
        }

        if (!passed)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.proof.failed",
                proofId,
                "Required Goal 091 proof is missing or did not pass."));
        }

        return new VisualWorldPreviewProofStatus
        {
            ProofId = proofId,
            SourceGoalId = sourceGoalId,
            RelativePath = relativePath,
            Status = passed
                ? VisualWorldPreviewArtifactStatus.Passed
                : VisualWorldPreviewArtifactStatus.Failed,
            Passed = passed,
            Sha256 = File.Exists(Resolve(projectRoot, relativePath))
                ? HashFor(projectRoot, relativePath, ledger)
                : string.Empty,
            DiagnosticSummary = summary
        };
    }

    private static VisualWorldPreviewWorkspaceQualityGate BuildQualityGate(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewSvgEntry> svgEntries,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        VisualWorldPreviewWinFormsBindingInventory binding,
        IReadOnlyList<VisualWorldPreviewDiagnostic> sourceDiagnostics)
    {
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var groupCount = groups.Count;
        var entryCount = groups.Sum(group => group.EntryCount);
        var goal091StreamEntries = groups
            .Where(group => group.GroupId == "chunk_stream_windows")
            .SelectMany(group => group.Entries)
            .Count(entry => entry.ArtifactKind == "text_svg_chunk_stream_window_overview");
        var requiredGroups = new[]
        {
            "microtiles",
            "map_patches",
            "region_composer",
            "world_profiles",
            "chunk_stream_windows"
        };
        var requiredArtifactGroupsPresent = requiredGroups.All(required =>
            groups.Any(group => group.GroupId == required && group.EntryCount > 0));
        var noAbsolutePaths = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(IsSafeRelativePath);
        var noBinaryMedia = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(path => !IsBinaryOrRasterMedia(path));
        var proofStatusPassed = proofs.Count >= 7 && proofs.All(item => item.Passed);

        AddIfFalse(requiredArtifactGroupsPresent, "goal092.quality.groups_missing", "catalog", diagnostics);
        AddIfFalse(svgEntries.Count >= 4, "goal092.quality.svg_count", "catalog.svgEntries", diagnostics);
        AddIfFalse(goal091StreamEntries >= 4, "goal092.quality.goal091_missing", "chunk_stream_windows", diagnostics);
        AddIfFalse(proofStatusPassed, "goal092.quality.proofs_failed", "proofStatus", diagnostics);
        AddIfFalse(noAbsolutePaths, "goal092.quality.absolute_path", "catalog", diagnostics);
        AddIfFalse(noBinaryMedia, "goal092.quality.binary_media", "catalog", diagnostics);
        AddIfFalse(binding.Passed, "goal092.quality.winforms_binding", "winformsBinding", diagnostics);
        foreach (var diagnostic in sourceDiagnostics.Where(item => item.Severity == "error"))
        {
            diagnostics.Add(diagnostic);
        }

        var passed = diagnostics.All(item => item.Severity != "error");
        return new VisualWorldPreviewWorkspaceQualityGate
        {
            Accepted = false,
            Passed = passed,
            GroupCount = groupCount,
            EntryCount = entryCount,
            SvgTextPreviewCount = svgEntries.Count,
            Goal091StreamWindowEntryCount = goal091StreamEntries,
            RequiredArtifactGroupsPresent = requiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = goal091StreamEntries >= 4,
            ProofStatusPassed = proofStatusPassed,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMediaAdded = noBinaryMedia,
            WinFormsBindingReal = binding.Passed,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                ".llmgc/procedural/goal-092-visual-world-stream-preview-workspace/",
                "docs/agent-tasks/goal-092-visual-world-stream-preview-workspace/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static VisualWorldStreamPreviewWorkspaceReport BuildReport(
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string catalogJson,
        string proofStatusJson,
        string bindingJson,
        string qualityJson) =>
        new()
        {
            Accepted = false,
            GroupCount = catalog.GroupCount,
            EntryCount = catalog.EntryCount,
            SvgTextPreviewCount = catalog.SvgTextPreviewCount,
            Goal091StreamWindowEntryCount = qualityGate.Goal091StreamWindowEntryCount,
            ProofStatusPassed = proofStatus.Passed,
            WinFormsBindingPassed = binding.Passed,
            QualityGatePassed = qualityGate.Passed,
            CatalogHash = Sha256Text(catalogJson),
            ProofStatusHash = Sha256Text(proofStatusJson),
            WinFormsBindingInventoryHash = Sha256Text(bindingJson),
            QualityGateHash = Sha256Text(qualityJson)
        };

    private static string RenderReport(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 092 Visual World Stream Preview Workspace Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 092 adds a BCL-only Application review seam and WinForms workspace over the deterministic visual world artifacts from Goals 086-091. It loads existing JSON/text-SVG evidence by repository-relative path and does not add runtime, Unity, provider, schema, project-file, dependency, binary media or raster media changes.",
            string.Empty,
            "## Catalog",
            string.Empty,
            $"- groupCount: {catalog.GroupCount}",
            $"- entryCount: {catalog.EntryCount}",
            $"- svgTextPreviewCount: {catalog.SvgTextPreviewCount}",
            $"- goal091StreamWindowEntryCount: {qualityGate.Goal091StreamWindowEntryCount}",
            string.Empty
        };
        lines.AddRange(catalog.Groups.Select(group =>
            "- " + group.GroupId + ": entries=" + group.EntryCount
            + ", svgEntries=" + group.SvgEntryCount
            + ", sourceGoal=" + group.SourceGoalId
            + ", status=" + group.Status));
        lines.AddRange(
        [
            string.Empty,
            "## Proof Status",
            string.Empty,
            $"- proofStatusPassed: {proofStatus.Passed.ToString().ToLowerInvariant()}",
            $"- proofCount: {proofStatus.ProofCount}"
        ]);
        lines.AddRange(proofStatus.Proofs.Select(proof =>
            "- " + proof.ProofId + ": passed=" + proof.Passed.ToString().ToLowerInvariant()
            + ", path=" + proof.RelativePath));
        lines.AddRange(
        [
            string.Empty,
            "## WinForms Binding",
            string.Empty,
            $"- bindingPassed: {binding.Passed.ToString().ToLowerInvariant()}",
            $"- pageControlExists: {binding.PageControlExists.ToString().ToLowerInvariant()}",
            $"- designerExists: {binding.DesignerExists.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersService: {binding.CompositionRootRegistersService.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersPage: {binding.CompositionRootRegistersPage.ToString().ToLowerInvariant()}",
            $"- editorRegistryIncludesPage: {binding.EditorRegistryIncludesPage.ToString().ToLowerInvariant()}",
            $"- pageActivationLoadsApplicationResult: {binding.PageActivationLoadsApplicationResult.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysGroupsEntriesProofs: {binding.PageBindDisplaysGroupsEntriesProofs.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Quality Gate",
            string.Empty,
            $"- qualityGatePassed: {qualityGate.Passed.ToString().ToLowerInvariant()}",
            $"- requiredArtifactGroupsPresent: {qualityGate.RequiredArtifactGroupsPresent.ToString().ToLowerInvariant()}",
            $"- goal091StreamWindowsVisible: {qualityGate.Goal091StreamWindowsVisible.ToString().ToLowerInvariant()}",
            $"- noAbsolutePaths: {qualityGate.NoAbsolutePaths.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- proofStatusHash: {report.ProofStatusHash}",
            $"- winFormsBindingInventoryHash: {report.WinFormsBindingInventoryHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyDictionary<string, string> LoadLedger(
        string projectRoot,
        string sourceRoot,
        string ledgerFileName)
    {
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        using var doc = TryReadJson(
            projectRoot,
            sourceRoot + "/" + ledgerFileName,
            []);
        if (doc is null || !TryGetArray(doc.RootElement, "files", out var files))
        {
            return ledger;
        }

        foreach (var file in files)
        {
            var relativePath = NormalizeLedgerPath(sourceRoot, TryGetString(file, "relativePath"));
            var sha = TryGetString(file, "sha256");
            if (!string.IsNullOrWhiteSpace(relativePath) && !string.IsNullOrWhiteSpace(sha))
            {
                ledger[relativePath] = sha;
            }
        }

        return ledger;
    }

    private static JsonDocument? TryReadJson(
        string projectRoot,
        string relativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.json.missing",
                relativePath,
                "Required JSON artifact was not found."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static IEnumerable<string> EnumerateExistingFiles(
        string projectRoot,
        string relativeDirectory,
        string pattern)
    {
        var fullDirectory = Resolve(projectRoot, relativeDirectory);
        if (!Directory.Exists(fullDirectory))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullDirectory, pattern, SearchOption.TopDirectoryOnly);
    }

    private static string BuildProofSummary(JsonElement root, string booleanProperty, bool passed)
    {
        var fragments = new List<string>
        {
            booleanProperty + "=" + passed.ToString().ToLowerInvariant()
        };
        foreach (var property in new[]
        {
            "seamCount",
            "cacheRecordCount",
            "reusedChunkKeyCount",
            "infiniteOverlapReusedChunkKeyCount",
            "portalOrTransitionLinkCount",
            "scenarioCount",
            "rejectedCount"
        })
        {
            if (TryGetInt(root, property, out var value))
            {
                fragments.Add(property + "=" + value);
            }
        }

        return string.Join("; ", fragments);
    }

    private static string HashFor(
        string projectRoot,
        string relativePath,
        IReadOnlyDictionary<string, string> ledger)
    {
        var normalized = NormalizePath(relativePath);
        if (ledger.TryGetValue(normalized, out var declaredHash))
        {
            return declaredHash;
        }

        var fullPath = Resolve(projectRoot, normalized);
        return File.Exists(fullPath) ? Sha256File(fullPath) : string.Empty;
    }

    private static string NormalizeLedgerPath(string sourceRoot, string relativePath)
    {
        var normalized = NormalizePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.StartsWith(".llmgc/", StringComparison.Ordinal))
        {
            return normalized;
        }

        return NormalizePath(sourceRoot + "/" + normalized);
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out List<JsonElement> values)
    {
        values = [];
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        values = property.EnumerateArray().ToList();
        return true;
    }

    private static string TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static bool TryGetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(property.GetString(), out var value) && value,
            _ => false
        };
    }

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static string TryGetInt(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value.ToString() : string.Empty;

    private static bool IsSafeSvg(string text) =>
        text.Contains("<svg", StringComparison.OrdinalIgnoreCase)
        && text.Contains("viewBox=", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("<script", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("https://", StringComparison.OrdinalIgnoreCase)
        && !text.Contains("base64", StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathFullyQualified(path)
        && !path.StartsWith("/", StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Contains(":\\", StringComparison.Ordinal);

    private static bool IsBinaryOrRasterMedia(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".asset", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bytes", StringComparison.OrdinalIgnoreCase);
    }

    private static string TruncatePreview(string text) =>
        text.Length <= MaxPreviewCharacters
            ? text
            : text[..MaxPreviewCharacters] + Environment.NewLine + "... truncated ...";

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                code,
                target,
                "Visual world stream preview workspace quality gate did not pass."));
        }
    }

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        return File.Exists(fullPath) ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(
            Path.GetFullPath(projectRoot),
            NormalizePath(relativePath).Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string projectRoot, string path) =>
        Path.GetRelativePath(projectRoot, path).Replace('\\', '/');

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim().TrimStart('/');

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path must stay under the project root.");
        }
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Sha256Text(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
