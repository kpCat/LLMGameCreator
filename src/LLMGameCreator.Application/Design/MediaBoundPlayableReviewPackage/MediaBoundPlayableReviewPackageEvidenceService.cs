using System.Text;

namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundPlayableReviewPackageEvidenceService
{
    public const string RelativeOutputDirectory = MediaBoundPlayableReviewPackageVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string ReviewPackageManifestJsonFileName = "media-bound-review-package-manifest.json";
    public const string StreamingAssetsManifestJsonFileName = "streaming-assets-media-manifest.json";
    public const string PreviewPayloadsJsonFileName = "media-bound-preview-payloads.json";
    public const string UnityLoadContractJsonFileName = "unity-media-load-contract.json";
    public const string FamilySmokeMatrixJsonFileName = "media-bound-family-smoke-matrix.json";
    public const string InvalidMatrixJsonFileName = "invalid-media-bound-package-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "media-bound-playable-review-package-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly MediaBoundPlayableReviewPackageSourceLoader _sourceLoader;

    public MediaBoundPlayableReviewPackageEvidenceService(MediaBoundPlayableReviewPackageSourceLoader? sourceLoader = null)
    {
        _sourceLoader = sourceLoader ?? new MediaBoundPlayableReviewPackageSourceLoader();
    }

    public MediaBoundPlayableReviewPackageEvidenceResult Build(string projectRootPath)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new MediaBoundPlayableReviewPackageBuilder();
        var validator = new MediaBoundPlayableReviewPackageValidator();

        var sourceManifest = builder.BuildSourceManifest(source);
        var staged = builder.BuildStagedMedia(source);
        var reviewPackageWithoutHash = builder.BuildReviewPackageManifest(sourceManifest, staged.Records);
        var reviewPackage = reviewPackageWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reviewPackageWithoutHash))
        };
        var streamingManifestWithoutHash = builder.BuildStreamingAssetsManifest(staged.Records);
        var streamingManifest = streamingManifestWithoutHash with
        {
            DeterministicHash = Hash(Serialize(streamingManifestWithoutHash))
        };
        var previewPayloads = builder.BuildPreviewPayloads(source, sourceManifest, streamingManifest, staged.Records);
        var unityLoadContract = builder.BuildUnityLoadContract(streamingManifest, staged.Records);
        var unityLoadProofs = builder.BuildUnityLoadProofs(staged.Records);
        var familySmokeMatrix = builder.BuildFamilySmokeMatrix(sourceManifest, reviewPackage, previewPayloads, unityLoadProofs);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var artifactScopeReport = builder.BuildArtifactScopeReport();

        var diagnostics = MediaBoundPlayableReviewPackageValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateReviewPackage(reviewPackage))
                .Concat(validator.ValidateStreamingManifest(streamingManifest))
                .Concat(validator.ValidatePreviewPayloads(previewPayloads))
                .Concat(validator.ValidateUnityContractAndProofs(unityLoadContract, unityLoadProofs))
                .Concat(validator.ValidateFamilySmokeMatrix(familySmokeMatrix))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [ReviewPackageManifestJsonFileName] = Serialize(reviewPackage),
            [StreamingAssetsManifestJsonFileName] = Serialize(streamingManifest),
            [PreviewPayloadsJsonFileName] = Serialize(previewPayloads),
            [UnityLoadContractJsonFileName] = Serialize(unityLoadContract),
            [FamilySmokeMatrixJsonFileName] = Serialize(familySmokeMatrix),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix),
            [ArtifactScopeReportJsonFileName] = Serialize(artifactScopeReport)
        };
        foreach (var proof in unityLoadProofs.OrderBy(item => item.FamilyId, StringComparer.Ordinal))
        {
            artifactJson[MediaBoundPlayableReviewPackageBuilder.UnityProofFileName(proof.FamilyId)] = Serialize(proof);
        }

        var allRequiredProofPassed = diagnostics.All(item => item.Severity is not "error" and not "critical")
            && sourceManifest.Goal054AcceptedByUserHandoff
            && sourceManifest.Goal054ReportWasGreenProducedForReview
            && reviewPackage.Passed
            && streamingManifest.Passed
            && previewPayloads.Passed
            && unityLoadContract.Passed
            && unityLoadProofs.All(item => item.Passed)
            && familySmokeMatrix.Passed
            && invalidMatrix.Passed
            && artifactScopeReport.Passed;
        var blocked = diagnostics.Any(item =>
            item.Code.Contains(".source.", StringComparison.Ordinal)
            || item.Code.Contains(".boundary.", StringComparison.Ordinal)
            || item.Code.Contains(".license.", StringComparison.Ordinal));

        var reportWithoutHash = new MediaBoundPlayableReviewPackageReport
        {
            ImplementationStatus = allRequiredProofPassed ? "GREEN" : blocked ? "BLOCKED" : "FAILED",
            Accepted = false,
            Goal054AcceptedByUserHandoff = sourceManifest.Goal054AcceptedByUserHandoff,
            FamilyCount = sourceManifest.Families.Count,
            StagedFileCount = reviewPackage.StagedFileCount,
            PngFileCount = reviewPackage.PngFileCount,
            WavFileCount = reviewPackage.WavFileCount,
            BundleJsonFileCount = reviewPackage.BundleJsonFileCount,
            PhysicalMediaStaged = reviewPackage.StagedFileCount > 0,
            PngProofPassed = reviewPackage.StagedFiles.Where(item => item.StagedRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).All(item => item.PngValid),
            WavProofPassed = reviewPackage.StagedFiles.Where(item => item.StagedRelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)).All(item => item.WavValid),
            BundleProofPassed = reviewPackage.BundleJsonFileCount >= 3,
            ReviewPackageManifestPassed = reviewPackage.Passed,
            StreamingAssetsManifestPassed = streamingManifest.Passed,
            PreviewPayloadsPassed = previewPayloads.Passed,
            UnityMediaLoadContractPassed = unityLoadContract.Passed && unityLoadProofs.All(item => item.Passed),
            FamilySmokeMatrixPassed = familySmokeMatrix.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            ProviderCalls = false,
            NetworkImports = false,
            LlmCalls = false,
            LuaExecuted = false,
            PublicGamePackageSchemaChanged = false,
            UnitySourceChanged = unityLoadContract.UnitySourceChanged,
            UnityBuildOrPlayerExecuted = unityLoadContract.UnityBuildOrPlayerExecuted,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            ReviewPackageManifestHash = Hash(artifactJson[ReviewPackageManifestJsonFileName]),
            StreamingAssetsManifestHash = Hash(artifactJson[StreamingAssetsManifestJsonFileName]),
            PreviewPayloadsHash = Hash(artifactJson[PreviewPayloadsJsonFileName]),
            UnityLoadContractHash = Hash(artifactJson[UnityLoadContractJsonFileName]),
            FamilySmokeMatrixHash = Hash(artifactJson[FamilySmokeMatrixJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };
        var packageTextFiles = builder.BuildPackageTextFiles(reviewPackage, streamingManifest, previewPayloads);

        return new MediaBoundPlayableReviewPackageEvidenceResult
        {
            SourceManifest = sourceManifest,
            ReviewPackageManifest = reviewPackage,
            StreamingAssetsManifest = streamingManifest,
            PreviewPayloads = previewPayloads,
            UnityLoadContract = unityLoadContract,
            UnityLoadProofs = unityLoadProofs,
            FamilySmokeMatrix = familySmokeMatrix,
            InvalidMatrix = invalidMatrix,
            ArtifactScopeReport = artifactScopeReport,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            StagedMediaFiles = staged.Payloads,
            PackageTextFiles = packageTextFiles,
            ReportMarkdown = RenderReport(report, sourceManifest, reviewPackage, streamingManifest, previewPayloads, unityLoadContract, unityLoadProofs, familySmokeMatrix, invalidMatrix)
        };
    }

    public async Task<MediaBoundPlayableReviewPackageWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MediaBoundPlayableReviewPackageWriteResult> WriteAsync(
        string projectRootPath,
        MediaBoundPlayableReviewPackageEvidenceResult result,
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
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var media in result.StagedMediaFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, media.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
            await File.WriteAllBytesAsync(path, media.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var textFile in result.PackageTextFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, textFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
            await File.WriteAllTextAsync(path, textFile.Contents, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new MediaBoundPlayableReviewPackageWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        MediaBoundPlayableReviewPackageReport report,
        MediaBoundSourceManifest sourceManifest,
        MediaBoundReviewPackageManifest reviewPackage,
        StreamingAssetsMediaManifest streamingManifest,
        MediaBoundPreviewPayloads previewPayloads,
        UnityMediaLoadContract unityLoadContract,
        IReadOnlyList<UnityMediaLoadProof> unityLoadProofs,
        MediaBoundFamilySmokeMatrix familySmokeMatrix,
        InvalidMediaBoundPackageDiagnosticsMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Media-Bound Playable Review Package Report",
            string.Empty,
            "media_bound_playable_review_package_verification required",
            $"implementationStatus: {report.ImplementationStatus}",
            $"implementationStatus={report.ImplementationStatus}",
            "manualGate=media_bound_playable_review_package_verification",
            "accepted=false",
            "Goal054AcceptedByUserHandoff: true",
            $"goal054AcceptedByUserHandoff={report.Goal054AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"providerCalls={report.ProviderCalls.ToString().ToLowerInvariant()}",
            $"networkImports={report.NetworkImports.ToString().ToLowerInvariant()}",
            $"llmCalls={report.LlmCalls.ToString().ToLowerInvariant()}",
            $"luaExecuted={report.LuaExecuted.ToString().ToLowerInvariant()}",
            $"publicGamePackageSchemaChanged={report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"unitySourceChanged={report.UnitySourceChanged.ToString().ToLowerInvariant()}",
            $"unityBuildOrPlayerExecuted={report.UnityBuildOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"familyCount={report.FamilyCount}",
            $"stagedFileCount={report.StagedFileCount}",
            $"pngFileCount={report.PngFileCount}",
            $"wavFileCount={report.WavFileCount}",
            $"bundleJsonFileCount={report.BundleJsonFileCount}",
            $"physicalMediaStaged={report.PhysicalMediaStaged.ToString().ToLowerInvariant()}",
            $"pngProofPassed={report.PngProofPassed.ToString().ToLowerInvariant()}",
            $"wavProofPassed={report.WavProofPassed.ToString().ToLowerInvariant()}",
            $"bundleProofPassed={report.BundleProofPassed.ToString().ToLowerInvariant()}",
            $"reviewPackageManifestPassed={report.ReviewPackageManifestPassed.ToString().ToLowerInvariant()}",
            $"streamingAssetsManifestPassed={report.StreamingAssetsManifestPassed.ToString().ToLowerInvariant()}",
            $"previewPayloadsPassed={report.PreviewPayloadsPassed.ToString().ToLowerInvariant()}",
            $"unityMediaLoadContractPassed={report.UnityMediaLoadContractPassed.ToString().ToLowerInvariant()}",
            $"familySmokeMatrixPassed={report.FamilySmokeMatrixPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"reviewPackageManifestHash={report.ReviewPackageManifestHash}",
            $"streamingAssetsManifestHash={report.StreamingAssetsManifestHash}",
            $"previewPayloadsHash={report.PreviewPayloadsHash}",
            $"unityLoadContractHash={report.UnityLoadContractHash}",
            $"familySmokeMatrixHash={report.FamilySmokeMatrixHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
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
        lines.Add($"- goal047FamilyDryRunCount: {sourceManifest.Goal047FamilyDryRunCount}");
        lines.Add($"- goal053BindingCount: {sourceManifest.Goal053BindingCount}");
        lines.Add($"- goal054PhysicalMediaCount: {sourceManifest.Goal054PhysicalMediaCount}");
        lines.AddRange(sourceManifest.Families.Select(item => $"- {item.FamilyId}: scenario={item.ScenarioId}, png={item.Goal054PngCount}, wav={item.Goal054WavCount}, bundle={item.Goal054BundleJsonCount}, dryRun={item.DryRunArtifactRef}, goal054Payload={item.Goal054PreviewPayloadRef}"));
        lines.Add(string.Empty);
        lines.Add("## Physical Staging");
        lines.Add(string.Empty);
        lines.Add($"- reviewPackagePassed: {reviewPackage.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(reviewPackage.StagedFiles.Select(item => $"- {item.FamilyId}/{item.SlotId}: kind={item.MediaKind}, staged={item.StagedRelativePath}, source={item.SourceRelativePath}, sha256={item.StagedSha256}, bytes={item.SizeBytes}, license={item.LicenseDecision}, provenance={item.ProvenanceDecision}"));
        lines.Add(string.Empty);
        lines.Add("## StreamingAssets Manifest");
        lines.Add(string.Empty);
        lines.Add($"- passed: {streamingManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- manifest: {streamingManifest.ManifestRelativePath}");
        lines.Add($"- bindingCount: {streamingManifest.BindingCount}");
        lines.Add(string.Empty);
        lines.Add("## Preview/Export Payloads");
        lines.Add(string.Empty);
        lines.Add($"- passed: {previewPayloads.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(previewPayloads.Payloads.Select(item => $"- {item.FamilyId}: preview={item.PreviewPayloadId}, export={item.ExportPayloadId}, dryRun={item.ReferencedDryRunArtifactRef}, stagedRefs={item.StagedMediaRefs.Count}, proof={item.UnityLoadProofRef}, hash={item.HashSummary}"));
        lines.Add(string.Empty);
        lines.Add("## Unity-Compatible Media Load Proof");
        lines.Add(string.Empty);
        lines.Add($"- contractPassed: {unityLoadContract.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- readSurface: {unityLoadContract.ReadSurface}");
        lines.Add($"- imageLoadApi: {unityLoadContract.ImageLoadApi}");
        lines.Add($"- wavValidationMode: {unityLoadContract.WavValidationMode}");
        lines.Add($"- unitySourceChanged: {unityLoadContract.UnitySourceChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- unityBuildOrPlayerExecuted: {unityLoadContract.UnityBuildOrPlayerExecuted.ToString().ToLowerInvariant()}");
        foreach (var proof in unityLoadProofs.OrderBy(item => item.FamilyId, StringComparer.Ordinal))
        {
            lines.Add($"- {proof.FamilyId}: passed={proof.Passed.ToString().ToLowerInvariant()}, lines={proof.ProofLines.Count}");
            lines.AddRange(proof.ProofLines.Select(line => "  - " + line));
        }
        lines.Add(string.Empty);
        lines.Add("## Family Smoke");
        lines.Add(string.Empty);
        lines.Add($"- passed: {familySmokeMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(familySmokeMatrix.Families.Select(item => $"- {item.FamilyId}: passed={item.Passed.ToString().ToLowerInvariant()}, files={item.StagedFileCount}, png={item.PngFileCount}, wav={item.WavFileCount}, bundle={item.BundleJsonFileCount}, manifest={item.ManifestBound.ToString().ToLowerInvariant()}, preview={item.PreviewPayloadBound.ToString().ToLowerInvariant()}, unityProof={item.UnityProofBound.ToString().ToLowerInvariant()}"));
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
        lines.Add("No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, provider path, generator-library, solution or project file change is required by this Goal 055 proof. Unity source/build/player execution is not claimed; the proof is a deterministic Application-level StreamingAssets-compatible contract over staged physical Goal 054 bytes.");
        lines.Add(string.Empty);
        lines.Add("media_bound_playable_review_package_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => MediaBoundPlayableReviewPackageHash.Serialize(value);

    private static string Hash(string text) => MediaBoundPlayableReviewPackageHash.Hash(text);

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
