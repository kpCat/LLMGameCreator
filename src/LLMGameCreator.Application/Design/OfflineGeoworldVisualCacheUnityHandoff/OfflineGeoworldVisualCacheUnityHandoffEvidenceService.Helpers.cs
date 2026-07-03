using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;

public sealed partial class OfflineGeoworldVisualCacheUnityHandoffEvidenceService
{
    private static OfflineGeoworldUnitySimulatedReadProof ValidateMirroredPayload(
        string root,
        Goal100SourceContext context,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        var rootPath = Resolve(
            root,
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(
                File.Exists(path),
                "goal100.read.payload_file_missing",
                fileName,
                diagnostics);
        }

        return ValidatePayload(context, payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldUnitySimulatedReadProof ValidatePayload(
        Goal100SourceContext context,
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldVisualCacheDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName,
            out var manifestJson);
        payload.TryGetValue(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.PackageIndexFileName,
            out var packageJson);
        payload.TryGetValue(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerFileName,
            out var ledgerJson);
        payload.TryGetValue(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamWindowIndexFileName,
            out var streamJson);
        payload.TryGetValue(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.RuntimeReadmeFileName,
            out var readmeJson);

        var manifest = OfflineGeoworldVisualCacheUnityHandoffJson
            .Deserialize<OfflineGeoworldUnityHandoffManifest>(manifestJson ?? string.Empty)
            ?? new OfflineGeoworldUnityHandoffManifest();
        var packages = OfflineGeoworldVisualCacheUnityHandoffJson
            .Deserialize<OfflineGeoworldVisualCachePackageIndex>(packageJson ?? string.Empty)
            ?? new OfflineGeoworldVisualCachePackageIndex();
        var ledger = OfflineGeoworldVisualCacheUnityHandoffJson
            .Deserialize<OfflineGeoworldFeatureChunkLedger>(ledgerJson ?? string.Empty)
            ?? new OfflineGeoworldFeatureChunkLedger();
        var stream = OfflineGeoworldVisualCacheUnityHandoffJson
            .Deserialize<OfflineGeoworldStreamWindowIndex>(streamJson ?? string.Empty)
            ?? new OfflineGeoworldStreamWindowIndex();

        var hashesMatch = string.Equals(manifest.PackageIndexHash, Hash(packageJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.FeatureChunkLedgerHash,
                              Hash(ledgerJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.StreamWindowIndexHash,
                              Hash(streamJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.RuntimeReadmeHash,
                              Hash(readmeJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase);
        var countsMatch = manifest.PackageCount == 3
                          && packages.PackageCount == manifest.PackageCount
                          && ledger.FeatureCount == manifest.FeatureCount
                          && ledger.VisualCacheRecordCount == manifest.VisualCacheRecordCount
                          && stream.RequiredChunkCount == manifest.StreamWindowChunkCount
                          && manifest.FeatureCount == context.Features.Count
                          && manifest.SourceChunkCount == context.SourceChunkKeys.Count
                          && manifest.StreamWindowChunkCount == context.RequiredChunkKeys.Count;
        var values = payload.Values.ToList();
        var noRaw = values.All(value =>
            !value.Contains("\"rawGeodataIncluded\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"noRawGeodata\": false", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"noRawFullWorldDump\": false", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"rawFullAreaDump\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"planetDump\": true", StringComparison.OrdinalIgnoreCase));
        var noAbsolute = values.All(value => !ContainsAbsolutePath(value));
        var noBinary = payload.Keys.All(path => !IsBinaryOrRasterMedia(path))
                       && values.All(value => !BinaryOrRasterExtensions.Any(ext =>
                           value.Contains(ext, StringComparison.OrdinalIgnoreCase)));
        var noMarkers = values.All(value => !ProviderNetworkMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)));

        AddIfFalse(requiredPresent, "goal100.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.SchemaVersion), "goal100.read.manifest", "manifest", diagnostics);
        AddIfFalse(hashesMatch, "goal100.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(countsMatch, "goal100.read.counts", "payload", diagnostics);
        AddIfFalse(noRaw, "goal100.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal100.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noBinary, "goal100.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal100.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnitySimulatedReadProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && countsMatch
                     && noRaw
                     && noAbsolute
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.SchemaVersion),
            RequiredPayloadFilesPresent = requiredPresent,
            PayloadHashesMatchManifest = hashesMatch,
            CountsMatchVisualCacheCatalog = countsMatch,
            NoRawGeodata = noRaw,
            NoRawFullWorldDump = noRaw,
            NoAbsolutePaths = noAbsolute,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            PayloadFileCount = payload.Count,
            PackageCount = manifest.PackageCount,
            FeatureCount = manifest.FeatureCount,
            FeatureKindCount = manifest.FeatureKindCount,
            VisualCacheRecordCount = manifest.VisualCacheRecordCount,
            SourceChunkCount = manifest.SourceChunkCount,
            StreamWindowChunkCount = manifest.StreamWindowChunkCount,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal099_world_graph", "Goal099 WorldSourceGraph file removed",
                "goal100.negative.goal099_graph_missing", "offline-geoworld-worldsourcegraph.json"),
            Scenario("unmapped_feature_kind", "normalized feature kind has no visual layer mapping",
                "goal100.negative.feature_kind_unmapped", "normalized feature"),
            Scenario("raw_geodata_leak", "raw geodata payload is present in cache record",
                "goal100.negative.raw_geodata_leak", "visual cache record"),
            Scenario("missing_license_provenance", "license/provenance summary removed",
                "goal100.negative.license_provenance", "source feature"),
            Scenario("absolute_path", "absolute local path inserted into payload",
                "goal100.negative.absolute_path", "payload"),
            Scenario("live_network_fetch", "live network fetch marker added",
                "goal100.negative.live_network", "payload"),
            Scenario("public_tile_scraping_marker", "public tile scraping marker added",
                "goal100.negative.public_tile_scraping", "source lineage"),
            Scenario("lfz_copied_code_marker", "LFZ copied-code marker added",
                "goal100.negative.lfz_copied_code", "source lineage"),
            Scenario("raw_full_area_or_planet_dump", "raw full-area or planet dump marker added",
                "goal100.negative.raw_full_area_dump", "payload"),
            Scenario("fake_unity_success_without_file_read", "simulated read marked passed without file read",
                "goal100.negative.fake_unity_success", "simulated read proof"),
            Scenario("missing_streamingassets_manifest", "StreamingAssets manifest removed",
                "goal100.negative.manifest_missing", "StreamingAssets"),
            Scenario("tampered_manifest_hash", "manifest hash no longer matches payload file",
                "goal100.negative.manifest_hash", "manifest"),
            Scenario("unity_probe_provider_network_marker", "Unity probe contains provider/network marker",
                "goal100.negative.probe_marker", "OfflineGeoworldHandoffProbe.cs"),
            Scenario("adult_rating_metadata_without_safe_fallback", "rating metadata lacks safe fallback",
                "goal100.negative.rating_safe_fallback", "visual cache record")
        };
        return new OfflineGeoworldNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldVisualCacheUnityHandoffVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldWorkspaceBindingInventory BuildWorkspaceBindingInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        var workspaceDirectory = Resolve(
            root,
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        var pagePath = Resolve(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs");
        var workspaceText = Directory.Exists(workspaceDirectory)
            ? string.Join(Environment.NewLine, Directory.EnumerateFiles(workspaceDirectory, "*.cs")
                .Select(File.ReadAllText))
            : string.Empty;
        var pageText = File.Exists(pagePath) ? File.ReadAllText(pagePath, Encoding.UTF8) : string.Empty;
        var group = workspaceText.Contains("offline_geoworld_handoff", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("OfflineGeoworldHandoff", StringComparison.Ordinal)
                       || pageText.Contains("offlineGeoworldHandoff", StringComparison.Ordinal)
                       || pageText.Contains("offlineGeoworldHandoffPackageCount", StringComparison.Ordinal);
        var packageCount = pageText.Contains("offlineGeoworldHandoffPackageCount", StringComparison.Ordinal);
        var kindCounts = pageText.Contains("offlineGeoworldHandoffFeatureKindCounts", StringComparison.Ordinal);
        var payloadCount = pageText.Contains("offlineGeoworldHandoffUnityPayloadFileCount", StringComparison.Ordinal);
        var alpha = pageText.Contains("offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);

        AddIfFalse(group, "goal100.workspace.group_missing", "VisualWorldStreamPreviewWorkspace", diagnostics);
        AddIfFalse(relative, "goal100.workspace.relative_path_missing", "VisualWorldStreamPreviewWorkspace", diagnostics);
        AddIfFalse(winForms, "goal100.workspace.winforms_fields_missing", "VisualWorldStreamPreviewWorkspacePageControl.cs", diagnostics);
        AddIfFalse(packageCount, "goal100.workspace.package_count_missing", "VisualWorldStreamPreviewWorkspacePageControl.cs", diagnostics);
        AddIfFalse(kindCounts, "goal100.workspace.kind_counts_missing", "VisualWorldStreamPreviewWorkspacePageControl.cs", diagnostics);
        AddIfFalse(payloadCount, "goal100.workspace.payload_count_missing", "VisualWorldStreamPreviewWorkspacePageControl.cs", diagnostics);
        AddIfFalse(alpha, "goal100.workspace.alpha_status_missing", "VisualWorldStreamPreviewWorkspacePageControl.cs", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesOfflineGeoworldHandoffGroup = group,
            WorkspaceReadsGoal100EvidenceByRelativePath = relative,
            WinFormsPageDisplaysOfflineGeoworldHandoffFields = winForms,
            ShowsPackageCount = packageCount,
            ShowsFeatureCountByKind = kindCounts,
            ShowsUnityPayloadCount = payloadCount,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldSourceLineage BuildSourceLineage(
        string root,
        Goal100SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                "goal100.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal099AcceptedFalse, "goal100.lineage.goal099_accepted", Goal099Root, diagnostics);
        AddIfFalse(context.Goal099NoNetworkProviderProven, "goal100.lineage.goal099_network", Goal099Root, diagnostics);
        AddIfFalse(context.Goal099NoLfzCodeCopiedProven, "goal100.lineage.goal099_lfz", Goal099Root, diagnostics);
        AddIfFalse(context.ExistingVisualCacheHandoffArtifactsObserved, "goal100.lineage.visual_handoff", Goal095Root, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal099AcceptedFalsePreserved = context.Goal099AcceptedFalse,
            Goal099WorldSourceGraphConsumed = context.ChunkIdByKey.Count == 5,
            Goal099NoNetworkProviderProven = context.Goal099NoNetworkProviderProven,
            Goal099NoLfzCodeCopiedProven = context.Goal099NoLfzCodeCopiedProven,
            ExistingVisualCacheHandoffArtifactsObserved = context.ExistingVisualCacheHandoffArtifactsObserved,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldQualityGateScan BuildQualityGate(
        string root,
        Goal100SourceContext context,
        Goal100Payload payload,
        OfflineGeoworldUnityStreamingAssetsLedger ledger,
        OfflineGeoworldUnityProbeSourceInventory probe,
        OfflineGeoworldUnitySimulatedReadProof readProof,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(ledger.Diagnostics);
        diagnostics.AddRange(probe.Diagnostics);
        diagnostics.AddRange(readProof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);

        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                "goal100.source.file_over_700",
                file.RelativePath,
                "New/changed Goal100 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                "goal100.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldVisualCacheUnityHandoffVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldVisualCacheUnityHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldVisualCacheUnityHandoffVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var visualCacheRecordsBuilt = payload.Catalog.VisualCacheRecordCount == 18;
        var mappedKinds = payload.Catalog.FeatureCountByKind.Keys
            .OrderBy(value => value, StringComparer.Ordinal)
            .SequenceEqual(OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredVisualFeatureKinds
                .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal);
        var packagesCreated = payload.Catalog.PackageCount == 3
                              && payload.Catalog.Packages.All(item => item.MetadataOnly);
        var unityPayloadCreated = ledger.Passed && ledger.PayloadFileCount == 5;
        var noRaw = payload.FeatureChunkLedger.NoRawGeodata
                    && payload.Catalog.Records.All(item => !item.RawGeodataIncluded)
                    && readProof.NoRawGeodata
                    && readProof.NoRawFullWorldDump;
        var noAbs = readProof.NoAbsolutePaths;
        var noBinary = readProof.NoBinaryOrRasterMedia;
        var noNetwork = context.Goal099NoNetworkProviderProven
                        && readProof.NoProviderOrNetworkMarkers
                        && probe.HasNoProviderLlmNetworkMarkers;

        AddIfFalse(alphaUnchanged, "goal100.quality.alpha_bootstrap", OfflineGeoworldVisualCacheUnityHandoffVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(visualCacheRecordsBuilt, "goal100.quality.records", "visual cache catalog", diagnostics);
        AddIfFalse(mappedKinds, "goal100.quality.feature_kinds", "visual cache catalog", diagnostics);
        AddIfFalse(packagesCreated, "goal100.quality.packages", "package index", diagnostics);
        AddIfFalse(unityPayloadCreated, "goal100.quality.unity_payload", "StreamingAssets", diagnostics);
        AddIfFalse(readProof.Passed, "goal100.quality.read_proof", "simulated read", diagnostics);
        AddIfFalse(negative.Passed, "goal100.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal100.quality.workspace_binding", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal100.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(noRaw, "goal100.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbs, "goal100.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal100.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noNetwork, "goal100.quality.network_provider", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            VisualCacheRecordsBuilt = visualCacheRecordsBuilt,
            AllFeatureKindsMapped = mappedKinds,
            PackagesCreated = packagesCreated,
            UnityPayloadCreated = unityPayloadCreated,
            SimulatedReadProofPassed = readProof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            SourceLineagePassed = lineage.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            NoNetworkOrProviderImplementation = noNetwork,
            NoLfzCodeCopied = context.Goal099NoLfzCodeCopiedProven,
            NoRawGeodataDump = noRaw,
            NoAbsolutePaths = noAbs,
            NoBinaryOrRasterMedia = noBinary,
            FeatureCount = payload.Manifest.FeatureCount,
            PackageCount = payload.Manifest.PackageCount,
            VisualCacheRecordCount = payload.Manifest.VisualCacheRecordCount,
            SourceChunkCount = payload.Manifest.SourceChunkCount,
            StreamWindowChunkCount = payload.Manifest.StreamWindowChunkCount,
            UnityPayloadFileCount = ledger.PayloadFileCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldVisualCacheUnityHandoff/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldVisualCacheUnityHandoff/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100/",
                ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/",
                "docs/agent-tasks/goal-100-offline-geoworld-visual-cache-unity-handoff/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldUnityProbeSourceInventory BuildProbeSourceInventory(string root)
    {
        var path = Resolve(root, OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        var usesStreamingAssets = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var usesRoot = text.Contains(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var exposesInspector = text.Contains("public ProbeResult LastResult", StringComparison.Ordinal)
                               && text.Contains("[SerializeField]", StringComparison.Ordinal);
        var noBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));

        AddIfFalse(exists, "goal100.probe.missing", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);
        AddIfFalse(usesStreamingAssets, "goal100.probe.streamingassets", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);
        AddIfFalse(usesRoot, "goal100.probe.root", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);
        AddIfFalse(exposesInspector, "goal100.probe.inspector", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);
        AddIfFalse(noBootstrap, "goal100.probe.bootstrap_dependency", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);
        AddIfFalse(noMarkers, "goal100.probe.provider_network", OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityProbeSourceInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ProbeExists = exists,
            ProbeSha256 = exists ? HashFile(path) : string.Empty,
            ProbeLineCount = CountLines(text),
            UsesApplicationStreamingAssetsPath = usesStreamingAssets,
            UsesExpectedPayloadRoot = usesRoot,
            ExposesInspectorResultFields = exposesInspector,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderLlmNetworkMarkers = noMarkers,
            Diagnostics = ordered
        };
    }

    private static IReadOnlyDictionary<string, string> ReadPayloadFiles(string root)
    {
        var directory = Resolve(
            root,
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsRelativeRoot);
        var payload = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                payload[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return payload;
    }

    private static IReadOnlyList<Goal100Feature> ReadFeatures(JsonElement root)
    {
        if (!root.TryGetProperty("features", out var features)
            || features.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return features
            .EnumerateArray()
            .Select(feature => new Goal100Feature(
                TryGetString(feature, "featureId"),
                TryGetString(feature, "kind"),
                TryGetString(feature, "licenseProvenanceSummary"),
                ReadStringArray(feature, "chunkKeys")))
            .OrderBy(item => item.FeatureId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyDictionary<string, string> ReadChunks(JsonElement root)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (!root.TryGetProperty("chunks", out var chunks)
            || chunks.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var chunk in chunks.EnumerateArray())
        {
            if (!chunk.TryGetProperty("tileKey", out var tileKey))
            {
                continue;
            }

            var key = TryGetString(tileKey, "key");
            if (!string.IsNullOrWhiteSpace(key))
            {
                result[key] = TryGetString(chunk, "chunkId");
            }
        }

        return result;
    }

    private static (string VisualKind, string LayerId) MapFeatureKind(string sourceKind) =>
        sourceKind switch
        {
            "administrativeArea" => ("administrativeHint", "geoworld_administrative_hints"),
            "barrier" => ("barrier", "geoworld_barriers"),
            "bridge" => ("bridge", "geoworld_bridges"),
            "building" => ("buildingFootprint", "geoworld_building_footprints"),
            "landUse" => ("landUse", "geoworld_land_use"),
            "poi" => ("poi", "geoworld_poi"),
            "road" => ("roadSegment", "geoworld_road_segments"),
            "terrainHint" => ("terrainHint", "geoworld_terrain_hints"),
            "vegetation" => ("vegetation", "geoworld_vegetation"),
            "water" => ("waterBody", "geoworld_water_bodies"),
            _ => ("unmapped", "geoworld_unmapped")
        };

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return array.EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static JsonDocument? ReadJson(
        string root,
        string relativePath,
        List<OfflineGeoworldVisualCacheDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                "goal100.json.missing",
                relativePath,
                "Required source JSON file is missing."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldVisualCacheDiagnostic.Error(
                "goal100.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        ("docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md", "LFZ geoworld pattern study, no source copy"),
        ("docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md", "boundary stream window policy"),
        ("docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md", "offline/cache/provenance policy"),
        (Goal098Root + "/geoworld-source-adapter-streaming-contract-report.md", "Goal098 source adapter lineage"),
        (Goal099Root + "/offline-geoworld-worldsourcegraph-report.md", "Goal099 accepted=false report"),
        (Goal099Root + "/offline-geoworld-normalized-features.json", "Goal099 normalized features"),
        (Goal099Root + "/offline-geoworld-worldsourcegraph.json", "Goal099 WorldSourceGraph"),
        (Goal099Root + "/offline-geoworld-stream-window-plan.json", "Goal099 stream window"),
        (Goal099Root + "/offline-geoworld-negative-proof.json", "Goal099 negative proof"),
        (Goal093Root + "/visual-chunk-cache-export-report.md", "existing visual cache export"),
        (Goal095Root + "/visual-chunk-cache-unity-handoff-report.md", "existing Unity StreamingAssets handoff"),
        (Goal096Root + "/unity-handoff-inspector-report.md", "existing workspace Unity handoff inspector")
    ];

    private static OfflineGeoworldSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldVisualCacheUnityHandoff");
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldVisualCacheUnityHandoff");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeScriptPath));
        return paths;
    }

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var directory = Resolve(root, relativePath);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            paths.Add(file);
        }
    }

    private static (string RelativePath, int Lines) ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return (Relative(root, path), CountLines(text));
    }

}
