using System.Text;

namespace LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyPlayableLoopEvidenceService
{
    public const string RelativeOutputDirectory = UnityAlphaMultiFamilyPlayableLoopVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string FamilyModeManifestJsonFileName = "family-mode-manifest.json";
    public const string UnityStagingManifestJsonFileName = "unity-staging-manifest.json";
    public const string FamilyCommandPlanJsonFileName = "family-command-plan.json";
    public const string PlayerLogSummaryJsonFileName = "player-log-summary.json";
    public const string MediaBindingValidationJsonFileName = "media-binding-validation.json";
    public const string PreviewExportPayloadJsonFileName = "preview-export-payload.json";
    public const string ReviewPackageManifestJsonFileName = "review-package-manifest.json";
    public const string InvalidMatrixJsonFileName = "invalid-matrix.json";
    public const string ReportMarkdownFileName = "unity-alpha-multifamily-playable-loop-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly UnityAlphaMultiFamilyPlayableLoopSourceLoader _sourceLoader;
    private readonly UnityAlphaMultiFamilyUnityProofRunner _unityProofRunner;

    public UnityAlphaMultiFamilyPlayableLoopEvidenceService(
        UnityAlphaMultiFamilyPlayableLoopSourceLoader? sourceLoader = null,
        UnityAlphaMultiFamilyUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new UnityAlphaMultiFamilyPlayableLoopSourceLoader();
        _unityProofRunner = unityProofRunner ?? new UnityAlphaMultiFamilyUnityProofRunner();
    }

    public static string FamilyLoopProofFileName(string familyId) =>
        "family-loop-proof-" + familyId.Replace('_', '-') + ".json";

    public UnityAlphaMultiFamilyEvidenceResult Build(
        string projectRootPath,
        UnityAlphaMultiFamilyOptions? options = null)
    {
        var proof = new UnityAlphaMultiFamilyUnityProof
        {
            Passed = false,
            BlockerCode = "goal057.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerLogSummary = new UnityAlphaMultiFamilyPlayerLogSummary
            {
                Diagnostics =
                [
                    UnityAlphaMultiFamilyDiagnostic.Warning("goal057.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<UnityAlphaMultiFamilyWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        UnityAlphaMultiFamilyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new UnityAlphaMultiFamilyOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new UnityAlphaMultiFamilyUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal057.unity.pending" : "goal057.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerLogSummary = new UnityAlphaMultiFamilyPlayerLogSummary()
        });
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutputDirectory: true, cancellationToken).ConfigureAwait(false);

        var proof = _unityProofRunner.Run(
            sourceRoot,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.FamilyCommandPlan,
            settings);
        var final = BuildCore(sourceRoot, proof);
        return await WriteAsync(projectRootPath, final, resetOutputDirectory: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UnityAlphaMultiFamilyWriteResult> WriteAsync(
        string projectRootPath,
        UnityAlphaMultiFamilyEvidenceResult result,
        bool resetOutputDirectory = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        if (resetOutputDirectory)
        {
            ResetDirectory(outputDirectory);
        }
        else
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var written = new List<string>();
        foreach (var stagingFile in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, UnityAlphaMultiFamilyPlayableLoopVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, stagingFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new UnityAlphaMultiFamilyWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, UnityAlphaMultiFamilyPlayableLoopVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private UnityAlphaMultiFamilyEvidenceResult BuildCore(string projectRootPath, UnityAlphaMultiFamilyUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new UnityAlphaMultiFamilyPlayableLoopBuilder();
        var validator = new UnityAlphaMultiFamilyPlayableLoopValidator();

        var sourceManifest = builder.BuildSourceManifest(source);
        var familyModeManifest = builder.BuildFamilyModeManifest(source.Families, source.Goal056StagingManifest.Bindings);
        var commandPlan = builder.BuildFamilyCommandPlan(source.Families);
        var staging = builder.BuildUnityStaging(source, commandPlan);
        var mediaValidation = builder.BuildMediaBindingValidation(source.Goal056StagingManifest);
        var previewExportPayload = builder.BuildPreviewExportPayload(source.Families);
        var reviewPackageManifest = builder.BuildReviewPackageManifest();
        var invalidMatrix = builder.BuildInvalidMatrix();
        var familyProofs = builder.BuildFamilyLoopProofs(source.Families, unityProof);

        var stagingDiagnostics = UnityAlphaMultiFamilyPlayableLoopValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateFamilyModeManifest(familyModeManifest))
                .Concat(validator.ValidateStagingAndPlan(staging.Manifest, commandPlan))
                .Concat(validator.ValidateMediaAndReview(mediaValidation, previewExportPayload, reviewPackageManifest))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));
        var proofDiagnostics = validator.ValidateUnityProof(commandPlan, unityProof, familyProofs);
        var diagnostics = UnityAlphaMultiFamilyPlayableLoopValidator.Sort(stagingDiagnostics.Concat(proofDiagnostics));

        var stagingPassed = stagingDiagnostics.All(item => item.Severity is not "error" and not "critical")
            && sourceManifest.Goal056AcceptedByUserHandoff
            && sourceManifest.Goal056ReportWasGreenProducedForReview
            && sourceManifest.Goal056UnityProofPassed
            && familyModeManifest.Passed
            && staging.Manifest.Passed
            && commandPlan.Passed
            && mediaValidation.Passed
            && previewExportPayload.Passed
            && reviewPackageManifest.Passed
            && invalidMatrix.Passed;
        var allFamilyLoopsVerified = familyProofs.Count == 3 && familyProofs.Values.All(item => item.Passed);
        var implementationStatus = stagingPassed && unityProof.Passed && allFamilyLoopsVerified
            ? "GREEN"
            : stagingPassed && !unityProof.Passed
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [FamilyModeManifestJsonFileName] = Serialize(familyModeManifest),
            [UnityStagingManifestJsonFileName] = Serialize(staging.Manifest),
            [FamilyCommandPlanJsonFileName] = Serialize(commandPlan),
            [PlayerLogSummaryJsonFileName] = Serialize(unityProof.PlayerLogSummary),
            [MediaBindingValidationJsonFileName] = Serialize(mediaValidation),
            [PreviewExportPayloadJsonFileName] = Serialize(previewExportPayload),
            [ReviewPackageManifestJsonFileName] = Serialize(reviewPackageManifest),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };

        foreach (var pair in familyProofs.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            artifactJson[FamilyLoopProofFileName(pair.Key)] = Serialize(pair.Value);
        }

        var reportWithoutHash = new UnityAlphaMultiFamilyPlayableLoopReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal056AcceptedByUserHandoff = sourceManifest.Goal056AcceptedByUserHandoff,
            SourceFactsConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches),
            UnityStagingExists = staging.Manifest.Passed,
            AllFamilyModesPresent = familyModeManifest.Passed,
            AllFamilyLoopsVerified = allFamilyLoopsVerified,
            MediaBindingValidationPassed = mediaValidation.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerLogSummary.UnityExitCode,
            PlayerExitCode = unityProof.PlayerLogSummary.PlayerExitCode,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            FamilyModeManifestHash = Hash(artifactJson[FamilyModeManifestJsonFileName]),
            UnityStagingManifestHash = Hash(artifactJson[UnityStagingManifestJsonFileName]),
            FamilyCommandPlanHash = Hash(artifactJson[FamilyCommandPlanJsonFileName]),
            PlayerLogSummaryHash = Hash(artifactJson[PlayerLogSummaryJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new UnityAlphaMultiFamilyEvidenceResult
        {
            SourceManifest = sourceManifest,
            FamilyModeManifest = familyModeManifest,
            UnityStagingManifest = staging.Manifest,
            FamilyCommandPlan = commandPlan,
            FamilyLoopProofsByFamilyId = familyProofs
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            PlayerLogSummary = unityProof.PlayerLogSummary,
            MediaBindingValidation = mediaValidation,
            PreviewExportPayload = previewExportPayload,
            ReviewPackageManifest = reviewPackageManifest,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            StagingFiles = staging.StagingFiles,
            ReportMarkdown = RenderReport(report, sourceManifest, familyModeManifest, staging.Manifest, commandPlan, familyProofs, unityProof, mediaValidation, previewExportPayload, reviewPackageManifest, invalidMatrix)
        };
    }

    private static string RenderReport(
        UnityAlphaMultiFamilyPlayableLoopReport report,
        UnityAlphaMultiFamilySourceManifest sourceManifest,
        UnityAlphaFamilyModeManifest familyModeManifest,
        UnityAlphaMultiFamilyStagingManifest stagingManifest,
        UnityAlphaFamilyCommandPlan commandPlan,
        IReadOnlyDictionary<string, UnityAlphaFamilyLoopProof> familyProofs,
        UnityAlphaMultiFamilyUnityProof unityProof,
        UnityAlphaMultiFamilyMediaBindingValidation mediaValidation,
        UnityAlphaMultiFamilyPreviewExportPayload previewExportPayload,
        UnityAlphaMultiFamilyReviewPackageManifest reviewPackageManifest,
        InvalidUnityAlphaMultiFamilyMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Unity Alpha Multi-Family Playable Loop Report",
            string.Empty,
            "unity_alpha_multifamily_playable_loop_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=unity_alpha_multifamily_playable_loop_verification",
            $"goal056AcceptedByUserHandoff={report.Goal056AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"unityStagingExists={report.UnityStagingExists.ToString().ToLowerInvariant()}",
            $"allFamilyModesPresent={report.AllFamilyModesPresent.ToString().ToLowerInvariant()}",
            $"allFamilyLoopsVerified={report.AllFamilyLoopsVerified.ToString().ToLowerInvariant()}",
            $"mediaBindingValidationPassed={report.MediaBindingValidationPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"blockerCode={TextOrNone(unityProof.BlockerCode)}",
            $"blockerMessage={TextOrNone(unityProof.BlockerMessage)}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"familyModeManifestHash={report.FamilyModeManifestHash}",
            $"unityStagingManifestHash={report.UnityStagingManifestHash}",
            $"familyCommandPlanHash={report.FamilyCommandPlanHash}",
            $"playerLogSummaryHash={report.PlayerLogSummaryHash}",
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
        lines.Add($"- goal056ReportWasGreenProducedForReview: {sourceManifest.Goal056ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal056UnityProofPassed: {sourceManifest.Goal056UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- selectedFamilyCount: {sourceManifest.FamilyCount}");
        lines.AddRange(sourceManifest.SelectedFamilyIds.Select(item => $"- selectedFamily: {item}"));
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.SourceGoal}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Family Modes");
        lines.Add(string.Empty);
        lines.Add($"- passed: {familyModeManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- familyCount: {familyModeManifest.FamilyCount}");
        lines.AddRange(familyModeManifest.Families.Select(item => $"- {item.FamilyId}: mode={item.ModeId}, scenario={item.ScenarioId}, profile={item.ProfileId}, mediaBindings={item.StagedMediaBindingIds.Count}, expectedMarkers={item.ExpectedMarkers.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Staging");
        lines.Add(string.Empty);
        lines.Add($"- passed: {stagingManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- stagingRoot: {stagingManifest.StagingRoot}");
        lines.Add($"- familyCommandPlan: {stagingManifest.FamilyCommandPlanRelativePath}");
        lines.Add($"- goal056MediaManifest: {stagingManifest.Goal056MediaManifestRelativePath}");
        lines.Add($"- copiedGoal056StagingFileCount: {stagingManifest.CopiedGoal056StagingFileCount}");
        lines.Add($"- physicalMediaFileCount: {stagingManifest.PhysicalMediaFileCount}");
        lines.Add($"- pngFileCount: {stagingManifest.PngFileCount}");
        lines.Add($"- wavFileCount: {stagingManifest.WavFileCount}");
        lines.Add($"- bundleFileCount: {stagingManifest.BundleFileCount}");
        lines.Add(string.Empty);
        lines.Add("## Family Command Plan");
        lines.Add(string.Empty);
        lines.Add($"- passed: {commandPlan.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- accepted: {commandPlan.Accepted.ToString().ToLowerInvariant()}");
        lines.Add($"- commandCount: {commandPlan.Commands.Count}");
        lines.Add($"- expectedPlayerMarkerCount: {commandPlan.ExpectedPlayerMarkers.Count}");
        lines.AddRange(commandPlan.Commands.Select(item => $"- {item.FamilyId}: order={item.Order}, commandType={item.CommandType}, marker={item.ExpectedPlayerMarker}, expectedStatus={item.ExpectedStatus}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.PlayerLogSummary.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerLogSummary.PlayerExitCode?.ToString())}");
        lines.Add($"- unityBuildLog: {unityProof.PlayerLogSummary.UnityBuildLogRelativePath}");
        lines.Add($"- launchLog: {unityProof.PlayerLogSummary.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityProof.PlayerLogSummary.PlayLoopLogRelativePath}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerLogSummary.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerLogSummary.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Family Loop Proofs");
        lines.Add(string.Empty);
        foreach (var proof in familyProofs.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            lines.Add($"- {proof.Key}: passed={proof.Value.Passed.ToString().ToLowerInvariant()}, scenarioLoaded={proof.Value.ScenarioLoaded.ToString().ToLowerInvariant()}, loopStepCount={proof.Value.LoopStepCount}, missingMarkers={proof.Value.MissingMarkers.Count}");
            lines.AddRange(proof.Value.MatchedMarkers.Select(marker => $"  - matchedMarker: {marker}"));
            lines.AddRange(proof.Value.MissingMarkers.Select(marker => $"  - missingMarker: {marker}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Media Binding Validation");
        lines.Add(string.Empty);
        lines.Add($"- passed: {mediaValidation.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- mediaBindingCount: {mediaValidation.MediaBindingCount}");
        lines.Add($"- hashValidationPassed: {mediaValidation.HashValidationPassed.ToString().ToLowerInvariant()}");
        lines.AddRange(mediaValidation.Bindings.Select(item => $"- {item.FamilyId}/{item.SlotId}: kind={item.MediaKind}, path={item.RelativePath}, sha256={item.Sha256}"));
        lines.Add(string.Empty);
        lines.Add("## Preview Export Payload");
        lines.Add(string.Empty);
        lines.Add($"- passed: {previewExportPayload.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(previewExportPayload.Payloads.Select(item => $"- {item.FamilyId}: preview={item.PreviewPayloadId}, export={item.ExportPayloadId}, mode={item.ExportMode}, source={item.RuntimePreviewPayloadRef}"));
        lines.Add(string.Empty);
        lines.Add("## Review Package");
        lines.Add(string.Empty);
        lines.Add($"- passed: {reviewPackageManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- accepted: {reviewPackageManifest.Accepted.ToString().ToLowerInvariant()}");
        lines.AddRange(reviewPackageManifest.RequiredEvidenceFiles.Select(item => $"- requiredEvidenceFile: {item}"));
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
        lines.Add("No provider/media generation, network/import/download, LLM/RAG call, Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, provider path change, generator-library change, solution or project file change is part of this Goal 057 proof. Unity changes are limited to the repo-local Alpha family-mode diagnostic marker extension.");
        lines.Add(string.Empty);
        lines.Add("unity_alpha_multifamily_playable_loop_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => UnityAlphaMultiFamilyPlayableLoopHash.Serialize(value);

    private static string Hash(string text) => UnityAlphaMultiFamilyPlayableLoopHash.Hash(text);

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static void ResetDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out var exception))
        {
            throw new IOException($"Directory could not be reset: {path}", exception);
        }
    }

    private static bool TryResetDirectory(string path, int maxAttempts, out Exception? lastException)
    {
        lastException = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                lastException = exception;
                if (attempt < maxAttempts)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        return false;
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
