using System.Text;

namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignEvidenceService
{
    public const string RelativeOutputDirectory = FullMediaBoundGeneratorCampaignVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "campaign-source-manifest.json";
    public const string CampaignPlanJsonFileName = "campaign-plan.json";
    public const string ReviewPackageManifestJsonFileName = "unified-review-package-manifest.json";
    public const string UnityCommandPlanJsonFileName = "unity-alpha-campaign-command-plan.json";
    public const string UnityPlayerProofJsonFileName = "unity-alpha-campaign-player-proof.json";
    public const string PreviewExportPayloadJsonFileName = "preview-export-campaign-payload.json";
    public const string PackageCompatibilityProofJsonFileName = "campaign-package-compatibility-proof.json";
    public const string InvalidMatrixJsonFileName = "invalid-campaign-diagnostics-matrix.json";
    public const string ArtifactScopeReportMarkdownFileName = "artifact-scope-report.md";
    public const string ReportMarkdownFileName = "full-media-bound-generator-campaign-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly FullMediaBoundGeneratorCampaignSourceLoader _sourceLoader;
    private readonly FullMediaBoundUnityProofRunner _unityProofRunner;

    public FullMediaBoundGeneratorCampaignEvidenceService(
        FullMediaBoundGeneratorCampaignSourceLoader? sourceLoader = null,
        FullMediaBoundUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new FullMediaBoundGeneratorCampaignSourceLoader();
        _unityProofRunner = unityProofRunner ?? new FullMediaBoundUnityProofRunner();
    }

    public static string FamilyRunFileName(string familyId) =>
        "family-run-" + familyId.Replace('_', '-') + ".json";

    public FullMediaBoundCampaignEvidenceResult Build(string projectRootPath, FullMediaBoundGeneratorCampaignOptions? options = null)
    {
        var proof = new FullMediaBoundCampaignUnityProof
        {
            Passed = false,
            BlockerCode = "goal058.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerProof = new FullMediaBoundCampaignPlayerProof
            {
                Diagnostics =
                [
                    FullMediaBoundCampaignDiagnostic.Warning("goal058.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<FullMediaBoundCampaignWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        FullMediaBoundGeneratorCampaignOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new FullMediaBoundGeneratorCampaignOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new FullMediaBoundCampaignUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal058.unity.pending" : "goal058.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerProof = new FullMediaBoundCampaignPlayerProof()
        });
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutputDirectory: true, cancellationToken).ConfigureAwait(false);

        var proof = _unityProofRunner.Run(
            sourceRoot,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            settings);
        var final = BuildCore(sourceRoot, proof);
        return await WriteAsync(projectRootPath, final, resetOutputDirectory: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FullMediaBoundCampaignWriteResult> WriteAsync(
        string projectRootPath,
        FullMediaBoundCampaignEvidenceResult result,
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
            var path = Path.GetFullPath(Path.Combine(outputDirectory, FullMediaBoundGeneratorCampaignVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, stagingFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var reviewFile in result.ReviewPackageFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, reviewFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, reviewFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var artifactScopePath = Path.Combine(outputDirectory, ArtifactScopeReportMarkdownFileName);
        await File.WriteAllTextAsync(artifactScopePath, result.ArtifactScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(artifactScopePath);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new FullMediaBoundCampaignWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, FullMediaBoundGeneratorCampaignVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private FullMediaBoundCampaignEvidenceResult BuildCore(string projectRootPath, FullMediaBoundCampaignUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new FullMediaBoundGeneratorCampaignBuilder();
        var validator = new FullMediaBoundGeneratorCampaignValidator();

        var sourceManifest = builder.BuildSourceManifest(source);
        var campaignPlan = builder.BuildCampaignPlan(source);
        var familyRuns = builder.BuildFamilyRuns(source);
        var unityCommandPlan = builder.BuildUnityCommandPlan(source);
        var staging = builder.BuildStagingAndReviewPackageFiles(source, unityCommandPlan);
        var reviewPackageManifest = builder.BuildReviewPackageManifest();
        var previewExportPayload = builder.BuildPreviewExportPayload(familyRuns);
        var packageCompatibilityProof = builder.BuildPackageCompatibilityProof(source);
        var invalidMatrix = builder.BuildInvalidMatrix();

        var stagingDiagnostics = FullMediaBoundGeneratorCampaignValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateCampaignPlan(campaignPlan))
                .Concat(validator.ValidateFamilyRuns(familyRuns))
                .Concat(validator.ValidateReviewAndPayloads(reviewPackageManifest, unityCommandPlan, previewExportPayload, packageCompatibilityProof, invalidMatrix)));
        var proofDiagnostics = validator.ValidateUnityProof(unityCommandPlan, unityProof);
        var diagnostics = FullMediaBoundGeneratorCampaignValidator.Sort(stagingDiagnostics.Concat(proofDiagnostics));

        var stagingPassed = stagingDiagnostics.All(item => item.Severity is not "error" and not "critical")
            && sourceManifest.Goal057AcceptedByUserHandoff
            && sourceManifest.Goal057ReportWasGreenProducedForReview
            && sourceManifest.Goal057UnityProofPassed
            && campaignPlan.Passed
            && familyRuns.Count == 3
            && familyRuns.Values.All(item => item.Passed)
            && reviewPackageManifest.Passed
            && unityCommandPlan.Passed
            && previewExportPayload.Passed
            && packageCompatibilityProof.Passed
            && invalidMatrix.Passed;
        var allCampaignMarkersMatched = unityProof.Passed && unityProof.PlayerProof.MissingMarkers.Count == 0;
        var implementationStatus = stagingPassed && allCampaignMarkersMatched
            ? "GREEN"
            : stagingPassed && !allCampaignMarkersMatched
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [CampaignPlanJsonFileName] = Serialize(campaignPlan),
            [ReviewPackageManifestJsonFileName] = Serialize(reviewPackageManifest),
            [UnityCommandPlanJsonFileName] = Serialize(unityCommandPlan),
            [UnityPlayerProofJsonFileName] = Serialize(unityProof.PlayerProof),
            [PreviewExportPayloadJsonFileName] = Serialize(previewExportPayload),
            [PackageCompatibilityProofJsonFileName] = Serialize(packageCompatibilityProof),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var pair in familyRuns.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            artifactJson[FamilyRunFileName(pair.Key)] = Serialize(pair.Value);
        }

        var reportWithoutHash = new FullMediaBoundGeneratorCampaignReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal057AcceptedByUserHandoff = sourceManifest.Goal057AcceptedByUserHandoff,
            SourceFactsConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches && item.Diagnostics.Count == 0),
            AllFamiliesIncluded = familyRuns.Count == 3 && FullMediaBoundGeneratorCampaignVocabulary.FamilyIds.All(familyRuns.ContainsKey),
            CampaignRunnerExecuted = stagingPassed,
            ReviewPackageManifestPassed = reviewPackageManifest.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerProof.PlayerExitCode,
            AllCampaignMarkersMatched = allCampaignMarkersMatched,
            InvalidMatrixPassed = invalidMatrix.Passed,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            CampaignPlanHash = Hash(artifactJson[CampaignPlanJsonFileName]),
            UnityPlayerProofHash = Hash(artifactJson[UnityPlayerProofJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new FullMediaBoundCampaignEvidenceResult
        {
            SourceManifest = sourceManifest,
            CampaignPlan = campaignPlan,
            FamilyRunsByFamilyId = familyRuns,
            ReviewPackageManifest = reviewPackageManifest,
            UnityCommandPlan = unityCommandPlan,
            UnityPlayerProof = unityProof.PlayerProof,
            PreviewExportPayload = previewExportPayload,
            PackageCompatibilityProof = packageCompatibilityProof,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            StagingFiles = staging.StagingFiles,
            ReviewPackageFiles = staging.ReviewPackageFiles,
            ArtifactScopeReportMarkdown = RenderArtifactScopeReport(),
            ReportMarkdown = RenderReport(report, sourceManifest, campaignPlan, familyRuns, reviewPackageManifest, unityCommandPlan, unityProof, previewExportPayload, packageCompatibilityProof, invalidMatrix)
        };
    }

    private static string RenderArtifactScopeReport()
    {
        var lines = new[]
        {
            "# Goal 058 Artifact Scope Report",
            "",
            "- Scenario: goal-058-full-media-bound-generator-campaign",
            "- Declared gate: full_media_bound_generator_campaign_verification required",
            "- Allowed code root: src/LLMGameCreator.Application/Design/FullMediaBoundGeneratorCampaign/",
            "- Allowed tests root: tests/LLMGameCreator.Tests/Application/FullMediaBoundGeneratorCampaign/",
            "- Allowed product smoke: tests/LLMGameCreator.Tests/ProductSmoke/FullMediaBoundGeneratorCampaignProductSmokeTests.cs",
            "- Allowed artifact root: .llmgc/procedural/goal-058-full-media-bound-generator-campaign/",
            "- Narrow Unity allowance: unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            "- Forbidden provider/network/LLM/RAG/media generation/runtime/schema/UI/generator-library changes: enforced by task scope and final artifact guard"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderReport(
        FullMediaBoundGeneratorCampaignReport report,
        FullMediaBoundCampaignSourceManifest sourceManifest,
        FullMediaBoundCampaignPlan campaignPlan,
        IReadOnlyDictionary<string, FullMediaBoundCampaignFamilyRun> familyRuns,
        FullMediaBoundReviewPackageManifest reviewPackageManifest,
        FullMediaBoundUnityCampaignCommandPlan commandPlan,
        FullMediaBoundCampaignUnityProof unityProof,
        PreviewExportCampaignPayload previewExport,
        CampaignPackageCompatibilityProof packageProof,
        InvalidFullMediaBoundCampaignMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Full Media-Bound Generator Campaign Report",
            string.Empty,
            "full_media_bound_generator_campaign_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=full_media_bound_generator_campaign_verification",
            $"goal057AcceptedByUserHandoff={report.Goal057AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"allFamiliesIncluded={report.AllFamiliesIncluded.ToString().ToLowerInvariant()}",
            $"campaignRunnerExecuted={report.CampaignRunnerExecuted.ToString().ToLowerInvariant()}",
            $"reviewPackageManifestPassed={report.ReviewPackageManifestPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allCampaignMarkersMatched={report.AllCampaignMarkersMatched.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"campaignPlanHash={report.CampaignPlanHash}",
            $"unityPlayerProofHash={report.UnityPlayerProofHash}",
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
        lines.Add($"- goal057ReportWasGreenProducedForReview: {sourceManifest.Goal057ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal057UnityProofPassed: {sourceManifest.Goal057UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- sourceArtifactCount: {sourceManifest.SourceArtifactCount}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.SourceGoal}: family={item.ArtifactFamily}, artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Campaign Plan");
        lines.Add(string.Empty);
        lines.Add($"- passed: {campaignPlan.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- familyCount: {campaignPlan.FamilyCount}");
        lines.Add($"- stageCount: {campaignPlan.StageCount}");
        lines.AddRange(campaignPlan.Stages.Select(item => $"- stage={item.Order}:{item.StageId}, passed={item.Passed.ToString().ToLowerInvariant()}, goals={string.Join(",", item.SourceGoals)}"));
        lines.Add(string.Empty);
        lines.Add("## Family Runs");
        lines.Add(string.Empty);
        foreach (var run in familyRuns.Values.OrderBy(item => FullMediaBoundGeneratorCampaignBuilder.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal))
        {
            lines.Add($"- {run.FamilyId}: passed={run.Passed.ToString().ToLowerInvariant()}, scenario={run.ScenarioId}, profile={run.ProfileId}, commands={run.CommandCount}, mediaFiles={run.MediaFileCount}, preview={run.RuntimePreviewPayloadRef}, exportMode={run.ExportMode}");
        }

        lines.Add(string.Empty);
        lines.Add("## Review Package");
        lines.Add(string.Empty);
        lines.Add($"- passed: {reviewPackageManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- accepted: {reviewPackageManifest.Accepted.ToString().ToLowerInvariant()}");
        lines.AddRange(reviewPackageManifest.StreamingAssetsFiles.Select(item => $"- streamingAssetsFile: {item}"));
        lines.AddRange(reviewPackageManifest.RequiredEvidenceFiles.Select(item => $"- requiredEvidenceFile: {item}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.PlayerProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerProof.PlayerExitCode?.ToString())}");
        lines.Add($"- blockerCode: {TextOrNone(unityProof.BlockerCode)}");
        lines.Add($"- blockerMessage: {TextOrNone(unityProof.BlockerMessage)}");
        lines.Add($"- launchLog: {unityProof.PlayerProof.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityProof.PlayerProof.PlayLoopLogRelativePath}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Preview/Export And Package Compatibility");
        lines.Add(string.Empty);
        lines.Add($"- previewExportPassed: {previewExport.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- packageCompatibilityPassed: {packageProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- publicGamePackageSchemaChanged: {packageProof.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- runtimeSourceChanged: {packageProof.RuntimeSourceChanged.ToString().ToLowerInvariant()}");
        lines.Add($"- winFormsUiChanged: {packageProof.WinFormsUiChanged.ToString().ToLowerInvariant()}");
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
        lines.Add("No provider/media generation, network/import/download, LLM/RAG call, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 058 proof. Unity changes are limited to deterministic campaign marker support in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("full_media_bound_generator_campaign_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => FullMediaBoundGeneratorCampaignHash.Serialize(value);

    private static string Hash(string text) => FullMediaBoundGeneratorCampaignHash.Hash(text);

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
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
