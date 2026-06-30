using System.Text;

namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationReviewPackageEvidenceService
{
    public const string RelativeOutputDirectory = MediaMaterializationReviewPackageVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string QueueJsonFileName = "media-materialization-queue.json";
    public const string InventoryJsonFileName = "materialized-media-inventory.json";
    public const string LicenseLedgerJsonFileName = "media-provenance-license-ledger.json";
    public const string BindingValidationJsonFileName = "media-binding-validation.json";
    public const string ReviewPackageManifestJsonFileName = "media-review-package-manifest.json";
    public const string PreviewExportPayloadsJsonFileName = "preview-export-media-payloads.json";
    public const string InvalidMatrixJsonFileName = "invalid-media-materialization-matrix.json";
    public const string ReportMarkdownFileName = "media-materialization-review-package-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly MediaMaterializationReviewPackageSourceLoader _sourceLoader;

    public MediaMaterializationReviewPackageEvidenceService(MediaMaterializationReviewPackageSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new MediaMaterializationReviewPackageSourceLoader();
    }

    public MediaMaterializationReviewPackageEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new MediaMaterializationReviewPackageBuilder();
        var validator = new MediaMaterializationReviewPackageValidator();

        var manifest = builder.BuildSourceManifest(source);
        var queueBuild = builder.BuildMaterializationQueue(source);
        var inventory = builder.BuildInventory(queueBuild.Queue, queueBuild.Payloads);
        var ledger = builder.BuildProvenanceLicenseLedger(source, inventory, queueBuild.Queue);
        var bindingValidation = builder.BuildBindingValidation(source, queueBuild.Queue, inventory, ledger);
        var payloads = builder.BuildPreviewExportPayloads(manifest, queueBuild.Queue, inventory);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var reviewPackageWithoutHash = builder.BuildReviewPackageManifest(queueBuild.Queue, inventory, ledger, bindingValidation, payloads, invalidMatrix);
        var reviewPackage = reviewPackageWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reviewPackageWithoutHash))
        };
        var familySmokes = builder.BuildFamilySmokeProofs(queueBuild.Queue, inventory, payloads);

        var diagnostics = MediaMaterializationReviewPackageValidator.Sort(
            validator.ValidateSourceManifest(manifest)
                .Concat(validator.ValidateQueue(queueBuild.Queue))
                .Concat(validator.ValidateInventory(inventory, queueBuild.Queue))
                .Concat(validator.ValidateLedger(ledger))
                .Concat(validator.ValidateBindingValidation(bindingValidation))
                .Concat(validator.ValidatePayloads(payloads))
                .Concat(validator.ValidateReviewPackage(reviewPackage))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(manifest),
            [QueueJsonFileName] = Serialize(queueBuild.Queue),
            [InventoryJsonFileName] = Serialize(inventory),
            [LicenseLedgerJsonFileName] = Serialize(ledger),
            [BindingValidationJsonFileName] = Serialize(bindingValidation),
            [ReviewPackageManifestJsonFileName] = Serialize(reviewPackage),
            [PreviewExportPayloadsJsonFileName] = Serialize(payloads),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var smoke in familySmokes)
        {
            artifactJson[MediaMaterializationReviewPackageBuilder.FamilySmokeFileName(smoke.FamilyId)] = Serialize(smoke);
        }

        var allRequiredProofPassed = diagnostics.All(item => item.Severity is not "error" and not "critical")
            && manifest.Goal053AcceptedByUserHandoff
            && manifest.Goal053ProducedForReviewReportGreen
            && manifest.Goal053ReportKeptRequired
            && queueBuild.Queue.Passed
            && inventory.Passed
            && ledger.Passed
            && bindingValidation.Passed
            && payloads.Passed
            && reviewPackage.Passed
            && familySmokes.All(item => item.Passed)
            && invalidMatrix.Passed;
        var blocked = diagnostics.Any(item =>
            item.Code.Contains(".source.", StringComparison.Ordinal)
            || item.Code.Contains(".boundary.", StringComparison.Ordinal)
            || item.Code.Contains(".provider", StringComparison.Ordinal));

        var reportWithoutHash = new MediaMaterializationReviewPackageReport
        {
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            Accepted = false,
            FamilyCount = manifest.Families.Count,
            QueueItemCount = queueBuild.Queue.QueueItemCount,
            MaterializedFileCount = inventory.FileCount,
            PngFileCount = inventory.PngFileCount,
            WavFileCount = inventory.WavFileCount,
            BundleJsonFileCount = inventory.BundleJsonFileCount,
            Goal053AcceptedByUserHandoff = manifest.Goal053AcceptedByUserHandoff,
            Goal053SourceReportGreenRequired = manifest.Goal053ProducedForReviewReportGreen && manifest.Goal053ReportKeptRequired,
            PhysicalMediaProduced = inventory.FileCount > 0,
            PngProofPassed = inventory.Files.Where(item => item.MaterializedMediaFormat == "png").All(item => item.PngSignatureValid && item.PngChunkCrcsValid),
            WavProofPassed = inventory.Files.Where(item => item.MaterializedMediaFormat == "wav_pcm_s16_mono").All(item => item.WavHeaderValid),
            ProvenanceLicenseLedgerPassed = ledger.Passed,
            BindingValidationPassed = bindingValidation.Passed,
            PreviewExportPayloadsPassed = payloads.Passed,
            ReviewPackageManifestPassed = reviewPackage.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            ProviderNetworkLlmRagCalled = false,
            GamePackageSchemaChanged = false,
            RuntimeUiUnityChanged = false,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            QueueHash = Hash(artifactJson[QueueJsonFileName]),
            InventoryHash = Hash(artifactJson[InventoryJsonFileName]),
            LicenseLedgerHash = Hash(artifactJson[LicenseLedgerJsonFileName]),
            BindingValidationHash = Hash(artifactJson[BindingValidationJsonFileName]),
            ReviewPackageManifestHash = Hash(artifactJson[ReviewPackageManifestJsonFileName]),
            PreviewExportPayloadsHash = Hash(artifactJson[PreviewExportPayloadsJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new MediaMaterializationReviewPackageEvidenceResult
        {
            SourceManifest = manifest,
            MaterializationQueue = queueBuild.Queue,
            MaterializedMediaInventory = inventory,
            ProvenanceLicenseLedger = ledger,
            BindingValidation = bindingValidation,
            ReviewPackageManifest = reviewPackage,
            PreviewExportMediaPayloads = payloads,
            FamilySmokeProofs = familySmokes,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            MediaFiles = queueBuild.Payloads,
            ReportMarkdown = RenderReport(report, manifest, queueBuild.Queue, inventory, ledger, bindingValidation, reviewPackage, payloads, familySmokes, invalidMatrix)
        };
    }

    public async Task<MediaMaterializationReviewPackageWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MediaMaterializationReviewPackageWriteResult> WriteAsync(
        string projectRootPath,
        MediaMaterializationReviewPackageEvidenceResult result,
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

        foreach (var media in result.MediaFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, media.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
            await File.WriteAllBytesAsync(path, media.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new MediaMaterializationReviewPackageWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        MediaMaterializationReviewPackageReport report,
        MediaMaterializationSourceManifest sourceManifest,
        MediaMaterializationQueue queue,
        MaterializedMediaInventory inventory,
        MediaProvenanceLicenseLedger ledger,
        MediaBindingValidation bindingValidation,
        MediaReviewPackageManifest reviewPackage,
        PreviewExportMediaPayloads payloads,
        IReadOnlyList<FamilyMediaSmokeProof> familySmokes,
        InvalidMediaMaterializationMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Media Materialization Review Package Report",
            string.Empty,
            "media_materialization_review_package_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={report.ManualGate}",
            $"goal053AcceptedByUserHandoff={report.Goal053AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"goal053SourceReportGreenRequired={report.Goal053SourceReportGreenRequired.ToString().ToLowerInvariant()}",
            $"physicalMediaProduced={report.PhysicalMediaProduced.ToString().ToLowerInvariant()}",
            $"familyCount={report.FamilyCount}",
            $"queueItemCount={report.QueueItemCount}",
            $"materializedFileCount={report.MaterializedFileCount}",
            $"pngFileCount={report.PngFileCount}",
            $"wavFileCount={report.WavFileCount}",
            $"bundleJsonFileCount={report.BundleJsonFileCount}",
            $"pngProofPassed={report.PngProofPassed.ToString().ToLowerInvariant()}",
            $"wavProofPassed={report.WavProofPassed.ToString().ToLowerInvariant()}",
            $"bindingValidationPassed={report.BindingValidationPassed.ToString().ToLowerInvariant()}",
            $"provenanceLicenseLedgerPassed={report.ProvenanceLicenseLedgerPassed.ToString().ToLowerInvariant()}",
            $"previewExportPayloadsPassed={report.PreviewExportPayloadsPassed.ToString().ToLowerInvariant()}",
            $"reviewPackageManifestPassed={report.ReviewPackageManifestPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"providerNetworkLlmRagCalled={report.ProviderNetworkLlmRagCalled.ToString().ToLowerInvariant()}",
            $"gamePackageSchemaChanged={report.GamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"runtimeUiUnityChanged={report.RuntimeUiUnityChanged.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"queueHash={report.QueueHash}",
            $"inventoryHash={report.InventoryHash}",
            $"reviewPackageManifestHash={report.ReviewPackageManifestHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Preflight",
            string.Empty
        };
        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source Facts");
        lines.Add(string.Empty);
        lines.Add($"- sourceArtifactRefCount: {sourceManifest.SourceArtifactRefs.Count}");
        lines.Add($"- goal053RequestCount: {sourceManifest.Goal053RequestCount}");
        lines.Add($"- goal053BindingCount: {sourceManifest.Goal053BindingCount}");
        lines.AddRange(sourceManifest.Families.Select(item => $"- {item.FamilyId}: scenario={item.ScenarioId}, profile={item.ProfileId}, requests={item.SourceMediaRequestCount}, bindings={item.SourceBindingCount}, dryRun={item.DryRunArtifactRef}, preview={item.RuntimePreviewPayloadRef}, exportProfile={item.ExportProfileId}"));
        lines.Add(string.Empty);
        lines.Add("## Materialization Queue");
        lines.Add(string.Empty);
        lines.Add($"- passed: {queue.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(queue.Items.Select(item => $"- {item.MaterializationId}: family={item.FamilyId}, slot={item.MediaSlotId}, kind={item.MediaKind}, format={item.MaterializedMediaFormat}, path={item.OutputRelativePath}, sha256={item.ExpectedSha256}, role={item.ConsumerPayloadRole}"));
        lines.Add(string.Empty);
        lines.Add("## Physical Media Files");
        lines.Add(string.Empty);
        lines.Add($"- passed: {inventory.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(inventory.Files.Select(item => $"- {item.RelativePath}: format={item.MaterializedMediaFormat}, bytes={item.ByteLength}, sha256={item.Sha256}, pngSignature={item.PngSignatureValid.ToString().ToLowerInvariant()}, pngCrc={item.PngChunkCrcsValid.ToString().ToLowerInvariant()}, wavHeader={item.WavHeaderValid.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Provenance And License");
        lines.Add(string.Empty);
        lines.Add($"- passed: {ledger.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(ledger.LicenseDecisions.Select(item => $"- {item.SourceKind}: decision={item.Goal054Decision}, promoted={item.PromotedInGoal054.ToString().ToLowerInvariant()}, attributionRequired={item.RequiresAttributionPayload.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Binding Validation And Payloads");
        lines.Add(string.Empty);
        lines.Add($"- bindingValidationPassed: {bindingValidation.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- everyFamilyHasImageAndAudioFixture: {bindingValidation.EveryFamilyHasImageAndAudioFixture.ToString().ToLowerInvariant()}");
        lines.AddRange(payloads.Payloads.Select(item => $"- {item.FamilyId}: preview={item.PreviewPayloadId}, export={item.ExportPayloadId}, mediaRefs={item.PhysicalMediaFileRefs.Count}, validation={item.ValidationStatus}, included={item.IncludedInReviewPackage.ToString().ToLowerInvariant()}, hashSummary={item.HashSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Review Package");
        lines.Add(string.Empty);
        lines.Add($"- passed: {reviewPackage.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- deterministicHash: {reviewPackage.DeterministicHash}");
        lines.AddRange(reviewPackage.FamilyCoverageSummary.Select(item => $"- {item.Key}: mediaFiles={item.Value}"));
        lines.Add(string.Empty);
        lines.Add("## Family Smoke");
        lines.Add(string.Empty);
        lines.AddRange(familySmokes.Select(item => $"- {item.FamilyId}: passed={item.Passed.ToString().ToLowerInvariant()}, files={item.MaterializedBindingCount}, png={item.ImagePngCount}, wav={item.AudioWavCount}, hashSummary={item.HashSummary}"));
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
        lines.Add("No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity, provider path, generator-library, solution or project file change is required by this Goal 054 proof.");
        lines.Add(string.Empty);
        lines.Add("media_materialization_review_package_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => MediaMaterializationReviewPackageHash.Serialize(value);

    private static string Hash(string text) => MediaMaterializationReviewPackageHash.Hash(text);

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
