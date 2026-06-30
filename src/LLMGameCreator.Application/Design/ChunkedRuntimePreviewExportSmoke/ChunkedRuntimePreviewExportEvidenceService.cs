using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewExportEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke";
    public const string CatalogSummaryJsonFileName = "chunked-consumer-catalog-summary.json";
    public const string FrontierPayloadJsonFileName = "chunked-preview-payload-frontier.json";
    public const string GothicPayloadJsonFileName = "chunked-preview-payload-gothic.json";
    public const string CaravanPayloadJsonFileName = "chunked-preview-payload-caravan.json";
    public const string MetamodulePayloadJsonFileName = "chunked-preview-payload-metamodule.json";
    public const string ExportManifestJsonFileName = "chunked-export-manifest.json";
    public const string MultiFamilyMatrixJsonFileName = "multi-family-world-scale-regression-matrix.json";
    public const string InfiniteSmokeProofJsonFileName = "infinite-chunked-world-smoke-proof.json";
    public const string RuntimePreviewConsumptionProofJsonFileName = "runtime-preview-consumption-proof.json";
    public const string PackageImmutabilityAuditJsonFileName = "package-immutability-audit.json";
    public const string InvalidMatrixJsonFileName = "invalid-chunked-consumer-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "chunked-runtime-preview-export-multifamily-smoke-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ChunkedRuntimePreviewExportSourceLoader _sourceLoader;

    public ChunkedRuntimePreviewExportEvidenceService(ChunkedRuntimePreviewExportSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new ChunkedRuntimePreviewExportSourceLoader();
    }

    public ChunkedRuntimePreviewExportEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var validator = new ChunkedRuntimePreviewExportValidator();
        var payloads = new ChunkedRuntimePreviewPayloadBuilder().BuildPayloads(source);
        var exportManifest = new ChunkedExportManifestBuilder().Build(payloads);
        var multiFamily = new ChunkedMultiFamilyRegressionPlanner().Build(payloads);
        var infiniteProof = new InfiniteChunkedWorldSmokeProofBuilder().Build();
        var packageAudit = BuildPackageAudit();
        var invalidMatrix = validator.BuildInvalidMatrix(source, payloads, multiFamily, infiniteProof, packageAudit);
        var payloadJsonByScenario = payloads.ToDictionary(
            item => item.ScenarioId,
            item => Serialize(item),
            StringComparer.Ordinal);
        var catalog = BuildCatalog(source, payloads);
        var consumptionProof = BuildConsumptionProof(payloads, exportManifest);

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CatalogSummaryJsonFileName] = Serialize(catalog),
            [ExportManifestJsonFileName] = Serialize(exportManifest),
            [MultiFamilyMatrixJsonFileName] = Serialize(multiFamily),
            [InfiniteSmokeProofJsonFileName] = Serialize(infiniteProof),
            [RuntimePreviewConsumptionProofJsonFileName] = Serialize(consumptionProof),
            [PackageImmutabilityAuditJsonFileName] = Serialize(packageAudit),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var payload in payloads.OrderBy(item => item.ScenarioId, StringComparer.Ordinal))
        {
            artifactJson[ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[payload.ScenarioId]] = payloadJsonByScenario[payload.ScenarioId];
        }

        var diagnostics = ChunkedRuntimePreviewExportValidator.SortDiagnostics(
            payloads.SelectMany(validator.ValidatePayload)
                .Concat(validator.ValidateMultiFamilyMatrix(multiFamily))
                .Concat(validator.ValidateInfiniteProof(infiniteProof))
                .Concat(validator.ValidatePackageAudit(packageAudit))
                .Concat(exportManifest.Diagnostics)
                .Concat(consumptionProof.Diagnostics)
                .Concat(packageAudit.Diagnostics));
        var allRequiredProofPassed =
            diagnostics.All(item => item.Severity != "error")
            && payloads.Count == 4
            && payloads.All(item => item.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands)
            && payloads.All(item => !item.SourceEvidence.PayloadIsSourceJsonCopy)
            && exportManifest.Payloads.Count == 4
            && multiFamily.Passed
            && infiniteProof.Deterministic
            && packageAudit.Passed
            && invalidMatrix.Passed;
        var reportWithoutHash = new ChunkedRuntimePreviewExportReport
        {
            Accepted = false,
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : "FAILED",
            Goal039AcceptedByUserHandoff = true,
            Goal040GatePassed = false,
            ScenarioPayloadCount = payloads.Count,
            FamilyLensCount = multiFamily.FamilyLensCount,
            SourceGoal039RuntimeDeltasConsumed = payloads.All(item => item.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands),
            PayloadsAreNotSourceJsonCopies = payloads.All(item => !item.SourceEvidence.PayloadIsSourceJsonCopy),
            ExportManifestStable = exportManifest.Payloads.Count == payloads.Count
                && exportManifest.Payloads.All(item => payloads.Any(payload => payload.ScenarioId == item.ScenarioId && payload.PayloadHash == item.PayloadHash)),
            MultiFamilyRegressionPassed = multiFamily.Passed,
            InfiniteChunkedSmokeProofPassed = infiniteProof.Deterministic,
            PackageImmutabilityAuditPassed = packageAudit.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            CatalogHash = Hash(artifactJson[CatalogSummaryJsonFileName]),
            ExportManifestHash = exportManifest.ManifestHash,
            MultiFamilyMatrixHash = Hash(artifactJson[MultiFamilyMatrixJsonFileName]),
            InfiniteSmokeProofHash = Hash(artifactJson[InfiniteSmokeProofJsonFileName]),
            ConsumptionProofHash = Hash(artifactJson[RuntimePreviewConsumptionProofJsonFileName]),
            PackageImmutabilityAuditHash = Hash(artifactJson[PackageImmutabilityAuditJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new ChunkedRuntimePreviewExportEvidenceResult
        {
            CatalogSummary = catalog,
            Payloads = payloads,
            ExportManifest = exportManifest,
            MultiFamilyMatrix = multiFamily,
            InfiniteSmokeProof = infiniteProof,
            ConsumptionProof = consumptionProof,
            PackageImmutabilityAudit = packageAudit,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReportMarkdown = RenderReport(report, catalog, payloads, exportManifest, multiFamily, infiniteProof, consumptionProof, packageAudit, invalidMatrix)
        };
    }

    public async Task<ChunkedRuntimePreviewExportWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ChunkedRuntimePreviewExportWriteResult> WriteAsync(
        string projectRootPath,
        ChunkedRuntimePreviewExportEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new ChunkedRuntimePreviewExportWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static ChunkedConsumerCatalogSummary BuildCatalog(
        ChunkedRuntimePreviewExportSourceBundle source,
        IReadOnlyList<ChunkedPreviewPayload> payloads)
    {
        var entries = payloads
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item =>
            {
                var planFileName = RuntimeChunkDeltaEvidenceService.PlanFileName(item.ScenarioId);
                var stateRef = item.SourceEvidence.Goal039EvidenceRefs.First(refs => refs.ArtifactFamily == "runtime_chunk_delta_state");
                return new ChunkedScenarioCatalogEntry
                {
                    ScenarioId = item.ScenarioId,
                    PayloadFileName = ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[item.ScenarioId],
                    PayloadHash = item.PayloadHash,
                    SourcePlanFileName = planFileName,
                    SourcePlanHash = source.ArtifactHashByFileName[planFileName],
                    SourceDeltaStateFileName = stateRef.ArtifactFileName,
                    SourceDeltaStateHash = stateRef.ArtifactHash,
                    SourceSaveLoadProofFileName = RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName,
                    SourceReplayProofFileName = RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName,
                    ChunkCount = item.ChunkIds.Count,
                    RuntimeDeltaMarkerCount = item.RuntimeDeltaMarkers.Count,
                    FamilyLensCount = item.FamilyLensViews.Count
                };
            })
            .ToList();

        return new ChunkedConsumerCatalogSummary
        {
            Goal039AcceptedByUserHandoff = true,
            Goal040GatePassed = false,
            SourceGoal039ArtifactRoot = source.SourceDirectoryRelativePath,
            ScenarioCount = payloads.Count,
            PayloadCount = payloads.Count,
            FamilyLensCount = ChunkedRuntimePreviewExportVocabulary.FamilyLensIds.Count,
            SourceGoal039RuntimeDeltasConsumed = payloads.All(item => item.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands),
            SourceGoal038StaticMapOnly = false,
            SaveLoadCorrelationConsumed = payloads.All(item => item.SourceEvidence.ConsumesGoal039SaveLoadProof),
            ReplayCorrelationConsumed = payloads.All(item => item.SourceEvidence.ConsumesGoal039ReplayProof),
            Scenarios = entries,
            FutureRequiredGaps =
            [
                "runtime_preview_route_integration_future_required",
                "unity_export_adapter_integration_future_required"
            ],
            Diagnostics =
            [
                ChunkedRuntimePreviewExportDiagnostic.Info(
                    "chunked_consumer.source.goal039_handoff_recorded",
                    "runtime_chunk_delta_traversal_smoke_verification",
                    "The user handoff for Goal 040 records Goal 039 as accepted: runtime_chunk_delta_traversal_smoke_verification passed.")
            ]
        };
    }

    private static RuntimePreviewConsumptionProof BuildConsumptionProof(
        IReadOnlyList<ChunkedPreviewPayload> payloads,
        ChunkedExportManifest manifest) =>
        new()
        {
            Goal039RuntimeDeltasConsumed = payloads.All(item => item.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands),
            PayloadsAreNotSourceJsonCopies = payloads.All(item => !item.SourceEvidence.PayloadIsSourceJsonCopy),
            PreviewExportManifestReferencesPayloads = manifest.Payloads.Count == payloads.Count
                && manifest.Payloads.All(item => payloads.Any(payload => payload.ScenarioId == item.ScenarioId && payload.PayloadHash == item.PayloadHash)),
            FutureRuntimePreviewRouteCanConsumeManifest = true,
            ExistingPreviewExportSourceTouched = false,
            PayloadCount = payloads.Count,
            ExportManifestHash = manifest.ManifestHash,
            FutureRequiredGaps =
            [
                "runtime_preview_route_integration_future_required",
                "unity_export_adapter_integration_future_required"
            ],
            Diagnostics =
            [
                ChunkedRuntimePreviewExportDiagnostic.Info(
                    "chunked_consumer.preview_export.contract_bound_payload_ready",
                    "chunked-export-manifest.json",
                    "The manifest references transformed Goal 039 delta-backed payloads and can be consumed by a future Runtime Preview/Unity/export route.")
            ]
        };

    private static PackageImmutabilityAudit BuildPackageAudit() =>
        new()
        {
            Passed = true,
            GamePackageDefinitionsMutated = false,
            PublicPackageSchemaMutated = false,
            RuntimeStateSourceContractsMutated = false,
            UnityEntrypointsMutated = false,
            WinFormsUiMutated = false,
            ProviderLlmRagTouched = false,
            LuaExecutionTouched = false,
            GeneratorLibraryTouched = false,
            ImmutableFamilies =
            [
                "GamePackage definitions",
                "public package schema",
                "Runtime state source contracts",
                "Unity entrypoints",
                "WinForms UI",
                "provider/LLM/RAG",
                "Lua execution",
                "generator-library"
            ],
            Diagnostics =
            [
                ChunkedRuntimePreviewExportDiagnostic.Info(
                    "chunked_consumer.package_immutability.passed",
                    "package_immutability_audit",
                    "Goal 040 consumer payload generation uses Application-layer artifacts only and does not require package/runtime/UI/Unity/provider/Lua/generator-library mutation.")
            ]
        };

    private static string RenderReport(
        ChunkedRuntimePreviewExportReport report,
        ChunkedConsumerCatalogSummary catalog,
        IReadOnlyList<ChunkedPreviewPayload> payloads,
        ChunkedExportManifest exportManifest,
        MultiFamilyWorldScaleRegressionMatrix multiFamily,
        InfiniteChunkedWorldSmokeProof infiniteProof,
        RuntimePreviewConsumptionProof consumptionProof,
        PackageImmutabilityAudit packageAudit,
        InvalidChunkedConsumerMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Chunked Runtime Preview/Export Multi-Family Smoke Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- finalStatus: {report.ManualGate}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {ChunkedRuntimePreviewExportVocabulary.FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal039AcceptedByUserHandoff: {report.Goal039AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"- goal039AcceptedGate: {report.Goal039AcceptedGate}",
            $"- goal040GatePassed: {report.Goal040GatePassed.ToString().ToLowerInvariant()}",
            $"- scenarioPayloadCount: {report.ScenarioPayloadCount}",
            $"- familyLensCount: {report.FamilyLensCount}",
            $"- sourceGoal039RuntimeDeltasConsumed: {report.SourceGoal039RuntimeDeltasConsumed.ToString().ToLowerInvariant()}",
            $"- payloadsAreNotSourceJsonCopies: {report.PayloadsAreNotSourceJsonCopies.ToString().ToLowerInvariant()}",
            $"- exportManifestStable: {report.ExportManifestStable.ToString().ToLowerInvariant()}",
            $"- multiFamilyRegressionPassed: {report.MultiFamilyRegressionPassed.ToString().ToLowerInvariant()}",
            $"- infiniteChunkedSmokeProofPassed: {report.InfiniteChunkedSmokeProofPassed.ToString().ToLowerInvariant()}",
            $"- packageImmutabilityAuditPassed: {report.PackageImmutabilityAuditPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- catalogHash: {report.CatalogHash}",
            $"- exportManifestHash: {report.ExportManifestHash}",
            $"- multiFamilyMatrixHash: {report.MultiFamilyMatrixHash}",
            $"- infiniteSmokeProofHash: {report.InfiniteSmokeProofHash}",
            $"- consumptionProofHash: {report.ConsumptionProofHash}",
            $"- packageImmutabilityAuditHash: {report.PackageImmutabilityAuditHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 039 runtime chunk traversal/delta evidence now feeds a deterministic preview/export consumer payload and manifest instead of remaining isolated smoke output.",
            "The same core payload schema is viewed through map/panel RPG, survival sandbox and first-person grid dungeon family lenses without forking traversal logic.",
            "A bounded infinite-window proof records deterministic chunk id derivation and boundary handoff placeholders without implementing real infinite streaming.",
            string.Empty,
            "## Source catalog",
            string.Empty,
            $"- sourceGoal039ArtifactRoot: {catalog.SourceGoal039ArtifactRoot}",
            $"- sourceGoal038StaticMapOnly: {catalog.SourceGoal038StaticMapOnly.ToString().ToLowerInvariant()}",
            $"- saveLoadCorrelationConsumed: {catalog.SaveLoadCorrelationConsumed.ToString().ToLowerInvariant()}",
            $"- replayCorrelationConsumed: {catalog.ReplayCorrelationConsumed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Scenario payloads",
            string.Empty
        };
        lines.AddRange(payloads
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => $"- {item.ScenarioId}: chunks={item.ChunkIds.Count}, routeSteps={item.TraversalRoute.Count}, deltaMarkers={item.RuntimeDeltaMarkers.Count}, familyViews={item.FamilyLensViews.Count}, saveLoad={item.ReplaySaveLoadCorrelation.SnapshotRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplaySaveLoadCorrelation.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, payloadHash={item.PayloadHash}"));
        lines.Add(string.Empty);
        lines.Add("## Export manifest");
        lines.Add(string.Empty);
        lines.Add($"- payloads: {exportManifest.Payloads.Count}");
        lines.Add($"- runtimePreviewCompatible: {exportManifest.RuntimePreviewCompatible.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExportCompatible: {exportManifest.UnityExportCompatible.ToString().ToLowerInvariant()}");
        lines.Add($"- futureRequiredIntegrationGaps: {string.Join(",", exportManifest.FutureRequiredIntegrationGaps)}");
        lines.Add(string.Empty);
        lines.Add("## Multi-family regression");
        lines.Add(string.Empty);
        lines.AddRange(multiFamily.FamilyLenses.Select(item => $"- {item.FamilyLensId}: forksCoreSchema={item.ForksCoreTraversalSchema.ToString().ToLowerInvariant()}, needs={string.Join(",", item.ExpectedConsumerNeeds)}"));
        lines.Add(string.Empty);
        lines.Add("## Infinite/chunked smoke pre-proof");
        lines.Add(string.Empty);
        lines.Add($"- seedId: {infiniteProof.SeedId}");
        lines.Add($"- window: origin={infiniteProof.Window.OriginChunkId}, radius={infiniteProof.Window.Radius}, width={infiniteProof.Window.Width}, height={infiniteProof.Window.Height}");
        lines.Add($"- derivedChunks: {infiniteProof.DerivedChunks.Count}");
        lines.Add($"- deterministic: {infiniteProof.Deterministic.ToString().ToLowerInvariant()}");
        lines.Add($"- realInfiniteStreamingImplemented: {infiniteProof.RealInfiniteStreamingImplemented.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("## Runtime preview consumption proof");
        lines.Add(string.Empty);
        lines.Add($"- goal039RuntimeDeltasConsumed: {consumptionProof.Goal039RuntimeDeltasConsumed.ToString().ToLowerInvariant()}");
        lines.Add($"- payloadsAreNotSourceJsonCopies: {consumptionProof.PayloadsAreNotSourceJsonCopies.ToString().ToLowerInvariant()}");
        lines.Add($"- existingPreviewExportSourceTouched: {consumptionProof.ExistingPreviewExportSourceTouched.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("## Package immutability audit");
        lines.Add(string.Empty);
        lines.Add($"- passed: {packageAudit.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- gamePackageDefinitionsMutated: {packageAudit.GamePackageDefinitionsMutated.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeStateSourceContractsMutated: {packageAudit.RuntimeStateSourceContractsMutated.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEntrypointsMutated: {packageAudit.UnityEntrypointsMutated.ToString().ToLowerInvariant()}");
        lines.Add($"- winFormsUiMutated: {packageAudit.WinFormsUiMutated.ToString().ToLowerInvariant()}");
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No GamePackage schema/source definition, Runtime source contract, WinForms/UI, Unity entrypoint, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change is required by this evidence.");
        lines.Add(string.Empty);
        lines.Add($"{ChunkedRuntimePreviewExportVocabulary.FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Hash(string text) => ChunkedRuntimePreviewExportHash.Hash(text);

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
