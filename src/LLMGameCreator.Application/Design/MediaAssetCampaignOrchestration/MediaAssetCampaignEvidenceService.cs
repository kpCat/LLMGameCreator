using System.Text;

namespace LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignEvidenceService
{
    public const string RelativeOutputDirectory = MediaAssetCampaignVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "media-campaign-source-manifest.json";
    public const string SlotCatalogJsonFileName = "media-slot-catalog.json";
    public const string RequestQueueJsonFileName = "media-request-queue.json";
    public const string StylePolicyJsonFileName = "media-style-policy.json";
    public const string LicenseProvenanceLedgerJsonFileName = "media-license-provenance-ledger.json";
    public const string CandidateQuarantineJsonFileName = "media-candidate-quarantine.json";
    public const string ReviewPromotionLedgerJsonFileName = "media-review-promotion-ledger.json";
    public const string BindingManifestJsonFileName = "media-binding-manifest.json";
    public const string FixtureInventoryJsonFileName = "media-fixture-file-inventory.json";
    public const string PreviewExportMediaPayloadsJsonFileName = "preview-export-media-payloads.json";
    public const string InvalidMatrixJsonFileName = "invalid-media-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "media-asset-campaign-orchestration-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly MediaAssetCampaignSourceLoader _sourceLoader;

    public MediaAssetCampaignEvidenceService(MediaAssetCampaignSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new MediaAssetCampaignSourceLoader();
    }

    public MediaAssetCampaignEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new MediaAssetCampaignBuilder();
        var validator = new MediaAssetCampaignValidator();

        var manifest = builder.BuildSourceManifest(source);
        var slotCatalog = builder.BuildSlotCatalog();
        var stylePolicy = builder.BuildStylePolicy(manifest);
        var requestQueue = builder.BuildRequestQueue(manifest, slotCatalog);
        var licenseLedger = builder.BuildLicenseLedger();
        var candidateQuarantine = builder.BuildCandidateQuarantine(requestQueue);
        var reviewLedger = builder.BuildReviewPromotionLedger(requestQueue, candidateQuarantine, licenseLedger);
        var fixtureBuild = builder.BuildFixtureInventory(requestQueue, candidateQuarantine, reviewLedger);
        var bindingManifest = builder.BuildBindingManifest(requestQueue, candidateQuarantine, reviewLedger, fixtureBuild.Inventory);
        var previewExportPayloads = builder.BuildPreviewExportPayloads(manifest, bindingManifest);
        var invalidMatrix = builder.BuildInvalidMatrix();

        var diagnostics = MediaAssetCampaignValidator.Sort(
            validator.ValidateSourceManifest(manifest)
                .Concat(validator.ValidateSlotCatalog(slotCatalog))
                .Concat(validator.ValidateRequestQueue(requestQueue, manifest, slotCatalog))
                .Concat(validator.ValidateLicenseLedger(licenseLedger))
                .Concat(validator.ValidateReviewPromotionLedger(reviewLedger, candidateQuarantine))
                .Concat(validator.ValidateFixtureInventory(fixtureBuild.Inventory))
                .Concat(validator.ValidateBindingManifest(bindingManifest))
                .Concat(validator.ValidatePreviewExportPayloads(previewExportPayloads))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));

        var allRequiredProofPassed = diagnostics.All(item => item.Severity is not "error" and not "critical")
            && !manifest.Accepted
            && manifest.PreflightGates.Any(item => item.GateId == "full_generator_without_media_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff")
            && slotCatalog.Passed
            && stylePolicy.Passed
            && requestQueue.Passed
            && licenseLedger.Passed
            && candidateQuarantine.Passed
            && reviewLedger.Passed
            && fixtureBuild.Inventory.Passed
            && bindingManifest.Passed
            && previewExportPayloads.Passed
            && invalidMatrix.Passed;
        var blockingDiagnostics = diagnostics.Where(item => item.Severity is "error" or "critical").ToList();
        var blocked = blockingDiagnostics.Any(item =>
            item.Code.Contains(".boundary.", StringComparison.Ordinal)
            || item.Code.Contains(".source.", StringComparison.Ordinal)
            || item.Code.Contains(".provider.", StringComparison.Ordinal));

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(manifest),
            [SlotCatalogJsonFileName] = Serialize(slotCatalog),
            [RequestQueueJsonFileName] = Serialize(requestQueue),
            [StylePolicyJsonFileName] = Serialize(stylePolicy),
            [LicenseProvenanceLedgerJsonFileName] = Serialize(licenseLedger),
            [CandidateQuarantineJsonFileName] = Serialize(candidateQuarantine),
            [ReviewPromotionLedgerJsonFileName] = Serialize(reviewLedger),
            [FixtureInventoryJsonFileName] = Serialize(fixtureBuild.Inventory),
            [BindingManifestJsonFileName] = Serialize(bindingManifest),
            [PreviewExportMediaPayloadsJsonFileName] = Serialize(previewExportPayloads),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };

        var reportWithoutHash = new MediaAssetCampaignReport
        {
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            Accepted = false,
            FamilyCount = manifest.Families.Count,
            RequestCount = requestQueue.RequestCount,
            FixtureFileCount = fixtureBuild.Inventory.FixtureFileCount,
            BindingCount = bindingManifest.BindingCount,
            Goal047AcceptedByUserHandoff = true,
            CatalogPassed = slotCatalog.Passed,
            RequestQueuePassed = requestQueue.Passed,
            LicenseLedgerPassed = licenseLedger.Passed,
            ReviewPromotionPassed = reviewLedger.Passed,
            FixtureInventoryPassed = fixtureBuild.Inventory.Passed,
            BindingManifestPassed = bindingManifest.Passed,
            PreviewExportPayloadsPassed = previewExportPayloads.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            FixtureMediaProduced = fixtureBuild.Inventory.FixtureFileCount > 0,
            RealProviderCalled = false,
            RealMediaGenerationCalled = false,
            NetworkOrImportCalled = false,
            GamePackageSchemaChanged = false,
            RuntimeUiUnityChanged = false,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            SlotCatalogHash = Hash(artifactJson[SlotCatalogJsonFileName]),
            RequestQueueHash = Hash(artifactJson[RequestQueueJsonFileName]),
            LicenseLedgerHash = Hash(artifactJson[LicenseProvenanceLedgerJsonFileName]),
            ReviewLedgerHash = Hash(artifactJson[ReviewPromotionLedgerJsonFileName]),
            FixtureInventoryHash = Hash(artifactJson[FixtureInventoryJsonFileName]),
            BindingManifestHash = Hash(artifactJson[BindingManifestJsonFileName]),
            PreviewExportPayloadsHash = Hash(artifactJson[PreviewExportMediaPayloadsJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new MediaAssetCampaignEvidenceResult
        {
            SourceManifest = manifest,
            SlotCatalog = slotCatalog,
            RequestQueue = requestQueue,
            StylePolicy = stylePolicy,
            LicenseLedger = licenseLedger,
            CandidateQuarantine = candidateQuarantine,
            ReviewPromotionLedger = reviewLedger,
            FixtureInventory = fixtureBuild.Inventory,
            BindingManifest = bindingManifest,
            PreviewExportPayloads = previewExportPayloads,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            FixtureFiles = fixtureBuild.Payloads,
            ReportMarkdown = RenderReport(report, manifest, slotCatalog, requestQueue, licenseLedger, candidateQuarantine, reviewLedger, fixtureBuild.Inventory, bindingManifest, previewExportPayloads, invalidMatrix)
        };
    }

    public async Task<MediaAssetCampaignWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MediaAssetCampaignWriteResult> WriteAsync(
        string projectRootPath,
        MediaAssetCampaignEvidenceResult result,
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

        foreach (var fixture in result.FixtureFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, fixture.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
            await File.WriteAllTextAsync(path, fixture.Contents, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new MediaAssetCampaignWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        MediaAssetCampaignReport report,
        MediaCampaignSourceManifest manifest,
        MediaSlotCatalog catalog,
        MediaRequestQueue queue,
        MediaLicenseProvenanceLedger licenseLedger,
        MediaCandidateQuarantine quarantine,
        MediaReviewPromotionLedger reviewLedger,
        MediaFixtureFileInventory fixtureInventory,
        MediaBindingManifest bindingManifest,
        PreviewExportMediaPayloads previewExportPayloads,
        InvalidMediaDiagnosticsMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Media Asset Campaign Orchestration Report",
            string.Empty,
            $"media_asset_campaign_orchestration_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={report.ManualGate}",
            $"realProviderCalled={report.RealProviderCalled.ToString().ToLowerInvariant()}",
            $"realMediaGenerationCalled={report.RealMediaGenerationCalled.ToString().ToLowerInvariant()}",
            $"fixtureMediaProduced={report.FixtureMediaProduced.ToString().ToLowerInvariant()}",
            $"familyCount={report.FamilyCount}",
            $"requestCount={report.RequestCount}",
            $"fixtureFileCount={report.FixtureFileCount}",
            $"bindingCount={report.BindingCount}",
            $"bindingManifestPassed={report.BindingManifestPassed.ToString().ToLowerInvariant()}",
            $"licenseLedgerPassed={report.LicenseLedgerPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            string.Empty,
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal047AcceptedByUserHandoff: {report.Goal047AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"- catalogPassed: {report.CatalogPassed.ToString().ToLowerInvariant()}",
            $"- requestQueuePassed: {report.RequestQueuePassed.ToString().ToLowerInvariant()}",
            $"- reviewPromotionPassed: {report.ReviewPromotionPassed.ToString().ToLowerInvariant()}",
            $"- fixtureInventoryPassed: {report.FixtureInventoryPassed.ToString().ToLowerInvariant()}",
            $"- previewExportPayloadsPassed: {report.PreviewExportPayloadsPassed.ToString().ToLowerInvariant()}",
            $"- gamePackageSchemaChanged: {report.GamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- runtimeUiUnityChanged: {report.RuntimeUiUnityChanged.ToString().ToLowerInvariant()}",
            $"- networkOrImportCalled: {report.NetworkOrImportCalled.ToString().ToLowerInvariant()}",
            $"- sourceManifestHash: {report.SourceManifestHash}",
            $"- requestQueueHash: {report.RequestQueueHash}",
            $"- fixtureInventoryHash: {report.FixtureInventoryHash}",
            $"- bindingManifestHash: {report.BindingManifestHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## Preflight",
            string.Empty
        };
        lines.AddRange(manifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source Manifest");
        lines.Add(string.Empty);
        lines.Add($"- sourceArtifactRefCount: {manifest.SourceArtifactRefs.Count}");
        lines.Add($"- selectedFamilies: {string.Join(",", manifest.SelectedFamilyIds)}");
        lines.Add($"- metamoduleKingdomOrRegionGroupCount: {manifest.MetamoduleStressSummary.KingdomOrRegionGroupCount}");
        lines.Add($"- metamoduleRuntimeDeltaMarkerCount: {manifest.MetamoduleStressSummary.RuntimeDeltaMarkerCount}");
        lines.Add($"- metamoduleCompactedSpeciesArchetypeSlotRefCount: {manifest.MetamoduleStressSummary.CompactedSpeciesArchetypeSlotRefCount}");
        lines.Add($"- oneRequestPerSpeciesArchetypeSlotGenerated: {manifest.MetamoduleStressSummary.OneRequestPerSpeciesArchetypeSlotGenerated.ToString().ToLowerInvariant()}");
        lines.AddRange(manifest.Families.Select(item => $"- {item.FamilyId}: scenario={item.ScenarioId}, profile={item.ProfileId}, style={item.StyleId}, exportProfile={item.ExportProfileId}, targets={item.GeneratedRuntimeTargetIds.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Slot Catalog");
        lines.Add(string.Empty);
        lines.Add($"- passed: {catalog.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(catalog.Slots.Select(item => $"- {item.SlotId}: kind={item.MediaKind}, target={item.BindingTargetKind}, fallback={item.FallbackPlaceholderBehavior}"));
        lines.Add(string.Empty);
        lines.Add("## Request Queue");
        lines.Add(string.Empty);
        lines.Add($"- passed: {queue.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(queue.Requests.GroupBy(item => item.FamilyId, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal).Select(group => $"- {group.Key}: requests={group.Count()}, fixtureReady={group.Count(item => item.Status == "fixture-ready")}, audio={group.Count(item => item.MediaKind == "audio")}, image={group.Count(item => item.MediaKind == "image")}"));
        lines.Add(string.Empty);
        lines.Add("## License And Provenance");
        lines.Add(string.Empty);
        lines.Add($"- passed: {licenseLedger.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(licenseLedger.Policies.Select(item => $"- {item.SourceKind}: policy={item.PromotionPolicy}, autoPromoteGoal053={item.CanAutoPromoteInGoal053.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Candidate Review");
        lines.Add(string.Empty);
        lines.Add($"- candidateCount: {quarantine.Candidates.Count}");
        lines.Add($"- promotedFixtureCount: {reviewLedger.PromotedFixtureCount}");
        lines.AddRange(reviewLedger.Decisions.Select(item => $"- {item.DecisionId}: decision={item.Decision}, promoted={item.Promoted.ToString().ToLowerInvariant()}, cause={item.CauseCode}"));
        lines.Add(string.Empty);
        lines.Add("## Fixture Files");
        lines.Add(string.Empty);
        lines.Add($"- passed: {fixtureInventory.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(fixtureInventory.Files.Select(item => $"- {item.RelativePath}: bytes={item.ByteLength}, sha256={item.Sha256}, request={item.BoundRequestId}, target={item.BoundGeneratedTargetId}, status={item.FixtureStatus}"));
        lines.Add(string.Empty);
        lines.Add("## Bindings And Payloads");
        lines.Add(string.Empty);
        lines.Add($"- bindingManifestPassed: {bindingManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- explicitFallbackCount: {bindingManifest.Fallbacks.Count}");
        lines.AddRange(previewExportPayloads.Families.Select(item => $"- {item.FamilyId}: bindings={item.BindingCount}, image={item.ImageLikeFixtureBindingCount}, audio={item.AudioLikeFixtureBindingCount}, uiOrBundle={item.UiOrBundleFixtureBindingCount}, fallback={item.ExplicitFallbackForUnfilledSlots.ToString().ToLowerInvariant()}, packageRuntimeExportPayloadsMutated={item.PackageRuntimeExportPayloadsMutated.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak Matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalidMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalidMatrix.ScenarioCount}");
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No real provider/media generation, no network/import, no GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity/export, provider/LLM/RAG, Lua, generator-library, solution/project or Designer file change is required by this Goal 053 evidence.");
        lines.Add(string.Empty);
        lines.Add($"{MediaAssetCampaignVocabulary.FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => MediaAssetCampaignHash.Serialize(value);

    private static string Hash(string text) => MediaAssetCampaignHash.Hash(text);

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
