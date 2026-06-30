using System.Text;

namespace LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;

public sealed class FullCampaignPlayableReviewPackageRcEvidenceService
{
    public const string RelativeOutputDirectory = FullCampaignPlayableReviewPackageRcVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string ReviewPackageManifestJsonFileName = "review-package-rc-manifest.json";
    public const string FileInventoryJsonFileName = "review-package-file-inventory.json";
    public const string PackageRowSelectionMatrixJsonFileName = "package-row-selection-matrix.json";
    public const string UnityCommandPlanJsonFileName = "unity-player-command-plan.json";
    public const string UnityProofMatrixJsonFileName = "unity-player-proof-matrix.json";
    public const string PackageMediaBindingAuditJsonFileName = "package-media-binding-audit.json";
    public const string SaveLoadReplayAuditJsonFileName = "save-load-replay-package-row-audit.json";
    public const string ManualReviewChecklistMarkdownFileName = "manual-review-checklist.md";
    public const string ScriptManifestJsonFileName = "automated-smoke-script-manifest.json";
    public const string InvalidMatrixJsonFileName = "invalid-review-package-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "full-campaign-playable-review-package-rc-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly FullCampaignPlayableReviewPackageRcSourceLoader _sourceLoader;
    private readonly FullCampaignPlayableReviewPackageRcUnityProofRunner _unityProofRunner;

    public FullCampaignPlayableReviewPackageRcEvidenceService(
        FullCampaignPlayableReviewPackageRcSourceLoader? sourceLoader = null,
        FullCampaignPlayableReviewPackageRcUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new FullCampaignPlayableReviewPackageRcSourceLoader();
        _unityProofRunner = unityProofRunner ?? new FullCampaignPlayableReviewPackageRcUnityProofRunner();
    }

    public FullCampaignPlayableReviewPackageRcEvidenceResult Build(string projectRootPath, FullCampaignPlayableReviewPackageRcOptions? options = null)
    {
        var proof = new FullCampaignPlayableUnityProof
        {
            Passed = false,
            BlockerCode = "goal061.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerProof = new FullCampaignPlayableUnityPlayerProof
            {
                Diagnostics =
                [
                    FullCampaignPlayableReviewPackageRcDiagnostic.Warning("goal061.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<FullCampaignPlayableReviewPackageRcWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        FullCampaignPlayableReviewPackageRcOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new FullCampaignPlayableReviewPackageRcOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new FullCampaignPlayableUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal061.unity.pending" : "goal061.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerProof = new FullCampaignPlayableUnityPlayerProof()
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

    public async Task<FullCampaignPlayableReviewPackageRcWriteResult> WriteAsync(
        string projectRootPath,
        FullCampaignPlayableReviewPackageRcEvidenceResult result,
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
            var path = Path.GetFullPath(Path.Combine(outputDirectory, FullCampaignPlayableReviewPackageRcVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
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

        var checklistPath = Path.Combine(outputDirectory, ManualReviewChecklistMarkdownFileName);
        await File.WriteAllTextAsync(checklistPath, result.ManualReviewChecklistMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(checklistPath);

        var artifactScopePath = Path.Combine(outputDirectory, ArtifactScopeReportJsonFileName);
        await File.WriteAllTextAsync(artifactScopePath, RenderArtifactScopeReportJson() + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(artifactScopePath);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new FullCampaignPlayableReviewPackageRcWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, FullCampaignPlayableReviewPackageRcVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private FullCampaignPlayableReviewPackageRcEvidenceResult BuildCore(string projectRootPath, FullCampaignPlayableUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new FullCampaignPlayableReviewPackageRcBuilder();

        var sourceManifest = builder.BuildSourceManifest(source);
        var selectionMatrix = builder.BuildPackageRowSelectionMatrix(source);
        var mediaAudit = builder.BuildPackageMediaBindingAudit(source);
        var saveLoadReplayAudit = builder.BuildSaveLoadReplayAudit(source);
        var unityCommandPlan = builder.BuildUnityCommandPlan(selectionMatrix, mediaAudit, saveLoadReplayAudit);
        var reviewPackageFiles = builder.BuildReviewPackageFiles(source, selectionMatrix, unityCommandPlan, mediaAudit, saveLoadReplayAudit);
        var fileInventory = builder.BuildFileInventory(reviewPackageFiles);
        var scriptManifest = builder.BuildScriptManifest(reviewPackageFiles);
        var reviewPackageManifest = builder.BuildReviewPackageManifest(sourceManifest, selectionMatrix, fileInventory, mediaAudit, saveLoadReplayAudit, scriptManifest);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var stagingFiles = builder.BuildStagingFiles(source, unityCommandPlan);

        var stagingDiagnostics = FullCampaignPlayableReviewPackageRcBuilder.SortDiagnostics(
            sourceManifest.Diagnostics
                .Concat(selectionMatrix.Rows.Where(item => !item.PackageHashVerified).Select(item => Error("goal061.matrix.package_hash_unverified", item.RowId, "Package hash was not verified.")))
                .Concat(mediaAudit.Rows.Where(item => !item.PackageMediaBindingsVerified).Select(item => Error("goal061.media.package_binding_unverified", item.RowId, "Package row media bindings were not verified.")))
                .Concat(saveLoadReplayAudit.Rows.Where(item => !item.SaveLoadRoundtripPassed || !item.ReplayDeterminismPassed).Select(item => Error("goal061.save_load_replay.row_unverified", item.RowId, "Save/load/replay audit failed for package row."))));
        var diagnostics = FullCampaignPlayableReviewPackageRcBuilder.SortDiagnostics(
            stagingDiagnostics
                .Concat(unityProof.Diagnostics)
                .Concat(unityProof.PlayerProof.Diagnostics));

        var stagingPassed = sourceManifest.Goal060AcceptedByUserHandoff
            && sourceManifest.Goal060ReportWasGreenProducedForReview
            && sourceManifest.Goal060UnityProofPassed
            && sourceManifest.Goal059MatrixConsumed
            && sourceManifest.Goal058CampaignProofConsumed
            && sourceManifest.MediaProofChainConsumed
            && reviewPackageManifest.Passed
            && fileInventory.Passed
            && selectionMatrix.Passed
            && mediaAudit.Passed
            && saveLoadReplayAudit.Passed
            && scriptManifest.Passed
            && unityCommandPlan.Passed
            && invalidMatrix.Passed
            && stagingDiagnostics.All(item => item.Severity is not "error" and not "critical");
        var allUnityMarkersMatched = unityProof.Passed && unityProof.PlayerProof.MissingMarkers.Count == 0 && unityProof.PlayerProof.ProvenRowCount == 9;
        var implementationStatus = stagingPassed && allUnityMarkersMatched
            ? "GREEN"
            : stagingPassed && !allUnityMarkersMatched
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [ReviewPackageManifestJsonFileName] = Serialize(reviewPackageManifest),
            [FileInventoryJsonFileName] = Serialize(fileInventory),
            [PackageRowSelectionMatrixJsonFileName] = Serialize(selectionMatrix),
            [UnityCommandPlanJsonFileName] = Serialize(unityCommandPlan),
            [UnityProofMatrixJsonFileName] = Serialize(unityProof.PlayerProof),
            [PackageMediaBindingAuditJsonFileName] = Serialize(mediaAudit),
            [SaveLoadReplayAuditJsonFileName] = Serialize(saveLoadReplayAudit),
            [ScriptManifestJsonFileName] = Serialize(scriptManifest),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };

        var reportWithoutHash = new FullCampaignPlayableReviewPackageRcReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal060AcceptedByUserHandoff = sourceManifest.Goal060AcceptedByUserHandoff,
            SourceFactsConsumed = reviewPackageManifest.SourceChainConsumed,
            ReviewPackageManifestPassed = reviewPackageManifest.Passed,
            FileInventoryPassed = fileInventory.Passed,
            PackageRowSelectionMatrixPassed = selectionMatrix.Passed,
            PackageMediaBindingAuditPassed = mediaAudit.Passed,
            SaveLoadReplayAuditPassed = saveLoadReplayAudit.Passed,
            ScriptManifestPassed = scriptManifest.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerProof.PlayerExitCode,
            AllUnityReviewPackageMarkersMatched = allUnityMarkersMatched,
            InvalidMatrixPassed = invalidMatrix.Passed,
            PackageRowCount = selectionMatrix.RowCount,
            PhysicalPackageCount = reviewPackageManifest.PhysicalPackageCount,
            UnityProvenRowCount = unityProof.PlayerProof.ProvenRowCount,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            ReviewPackageManifestHash = Hash(artifactJson[ReviewPackageManifestJsonFileName]),
            FileInventoryHash = Hash(artifactJson[FileInventoryJsonFileName]),
            PackageRowSelectionMatrixHash = Hash(artifactJson[PackageRowSelectionMatrixJsonFileName]),
            UnityCommandPlanHash = Hash(artifactJson[UnityCommandPlanJsonFileName]),
            UnityProofMatrixHash = Hash(artifactJson[UnityProofMatrixJsonFileName]),
            PackageMediaBindingAuditHash = Hash(artifactJson[PackageMediaBindingAuditJsonFileName]),
            SaveLoadReplayAuditHash = Hash(artifactJson[SaveLoadReplayAuditJsonFileName]),
            ScriptManifestHash = Hash(artifactJson[ScriptManifestJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new FullCampaignPlayableReviewPackageRcEvidenceResult
        {
            SourceManifest = sourceManifest,
            ReviewPackageManifest = reviewPackageManifest,
            FileInventory = fileInventory,
            PackageRowSelectionMatrix = selectionMatrix,
            UnityCommandPlan = unityCommandPlan,
            UnityPlayerProof = unityProof.PlayerProof,
            PackageMediaBindingAudit = mediaAudit,
            SaveLoadReplayAudit = saveLoadReplayAudit,
            ScriptManifest = scriptManifest,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReviewPackageFiles = reviewPackageFiles,
            StagingFiles = stagingFiles,
            ManualReviewChecklistMarkdown = RenderRootManualChecklist(selectionMatrix, mediaAudit, saveLoadReplayAudit, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, reviewPackageManifest, fileInventory, selectionMatrix, unityCommandPlan, unityProof, mediaAudit, saveLoadReplayAudit, scriptManifest, invalidMatrix)
        };
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal061_artifact_scope_report_v1",
            scenario = FullCampaignPlayableReviewPackageRcVocabulary.ProductSmokeRoute,
            gate = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate + " required",
            allowedArtifactRoot = FullCampaignPlayableReviewPackageRcVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/FullCampaignPlayableReviewPackageRc/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignPlayableReviewPackageRcProductSmokeTests.cs",
            narrowUnityAllowance = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            forbiddenChanges = new[]
            {
                "public GamePackage schema/model definitions",
                "Runtime/Runtime.Abstractions",
                "WinForms UI",
                "Infrastructure provider/LLM/RAG paths",
                "generator-library",
                "solution/project files",
                "external dependencies"
            }
        });

    private static string RenderRootManualChecklist(
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit,
        FullCampaignPlayableUnityCommandPlan unityCommandPlan)
    {
        var lines = new List<string>
        {
            "# Goal 061 Manual Review Checklist",
            string.Empty,
            "full_campaign_playable_review_package_rc_verification required",
            "accepted=false",
            string.Empty,
            "- Review package root: review-package/",
            "- Package rows: " + selectionMatrix.RowCount,
            "- Unity command rows: " + unityCommandPlan.Rows.Count,
            "- Media audit passed: " + mediaAudit.Passed.ToString().ToLowerInvariant(),
            "- Save/load/replay audit passed: " + saveLoadReplayAudit.Passed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Row Checks"
        };
        lines.AddRange(selectionMatrix.Rows.Select(row => "- " + row.RowId + ": package=" + row.PackageId + ", hash=" + row.PackageHash));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderReport(
        FullCampaignPlayableReviewPackageRcReport report,
        FullCampaignPlayableSourceManifest sourceManifest,
        FullCampaignPlayableReviewPackageRcManifest reviewManifest,
        FullCampaignPlayableReviewPackageFileInventory fileInventory,
        FullCampaignPlayablePackageRowSelectionMatrix selectionMatrix,
        FullCampaignPlayableUnityCommandPlan commandPlan,
        FullCampaignPlayableUnityProof unityProof,
        FullCampaignPlayablePackageMediaBindingAudit mediaAudit,
        FullCampaignPlayableSaveLoadReplayPackageRowAudit saveLoadReplayAudit,
        FullCampaignPlayableSmokeScriptManifest scriptManifest,
        InvalidFullCampaignPlayableReviewPackageMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Full Campaign Playable Review Package RC Report",
            string.Empty,
            "full_campaign_playable_review_package_rc_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=full_campaign_playable_review_package_rc_verification",
            $"goal060AcceptedByUserHandoff={report.Goal060AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"reviewPackageManifestPassed={report.ReviewPackageManifestPassed.ToString().ToLowerInvariant()}",
            $"fileInventoryPassed={report.FileInventoryPassed.ToString().ToLowerInvariant()}",
            $"packageRowSelectionMatrixPassed={report.PackageRowSelectionMatrixPassed.ToString().ToLowerInvariant()}",
            $"packageMediaBindingAuditPassed={report.PackageMediaBindingAuditPassed.ToString().ToLowerInvariant()}",
            $"saveLoadReplayAuditPassed={report.SaveLoadReplayAuditPassed.ToString().ToLowerInvariant()}",
            $"scriptManifestPassed={report.ScriptManifestPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allUnityReviewPackageMarkersMatched={report.AllUnityReviewPackageMarkersMatched.ToString().ToLowerInvariant()}",
            $"unityProvenRowCount={report.UnityProvenRowCount}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"packageRowCount={report.PackageRowCount}",
            $"physicalPackageCount={report.PhysicalPackageCount}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"reviewPackageManifestHash={report.ReviewPackageManifestHash}",
            $"fileInventoryHash={report.FileInventoryHash}",
            $"packageRowSelectionMatrixHash={report.PackageRowSelectionMatrixHash}",
            $"unityCommandPlanHash={report.UnityCommandPlanHash}",
            $"unityProofMatrixHash={report.UnityProofMatrixHash}",
            $"packageMediaBindingAuditHash={report.PackageMediaBindingAuditHash}",
            $"saveLoadReplayAuditHash={report.SaveLoadReplayAuditHash}",
            $"scriptManifestHash={report.ScriptManifestHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Preflight",
            string.Empty
        };
        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source Chain");
        lines.Add(string.Empty);
        lines.Add($"- goal060ReportWasGreenProducedForReview: {sourceManifest.Goal060ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal060UnityProofPassed: {sourceManifest.Goal060UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- goal059MatrixConsumed: {sourceManifest.Goal059MatrixConsumed.ToString().ToLowerInvariant()}");
        lines.Add($"- goal058CampaignProofConsumed: {sourceManifest.Goal058CampaignProofConsumed.ToString().ToLowerInvariant()}");
        lines.Add($"- mediaProofChainConsumed: {sourceManifest.MediaProofChainConsumed.ToString().ToLowerInvariant()}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Review Package RC");
        lines.Add(string.Empty);
        lines.Add($"- passed: {reviewManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- packageRows: {reviewManifest.PackageRowCount}");
        lines.Add($"- physicalPackages: {reviewManifest.PhysicalPackageCount}");
        lines.Add($"- scripts: {reviewManifest.ScriptCount}");
        lines.Add($"- scenarioSummaries: {reviewManifest.ScenarioSummaryCount}");
        lines.Add($"- fileInventoryCount: {fileInventory.FileCount}");
        foreach (var row in selectionMatrix.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, packageId={row.PackageId}, packageHashVerified={row.PackageHashVerified.ToString().ToLowerInvariant()}, media={row.PackageMediaBindingsVerified.ToString().ToLowerInvariant()}, saveLoadReplay={row.SaveLoadReplayVerified.ToString().ToLowerInvariant()}");
        }

        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.PlayerProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerProof.PlayerExitCode?.ToString())}");
        lines.Add($"- provenRowCount: {unityProof.PlayerProof.ProvenRowCount}");
        lines.Add($"- blockerCode: {TextOrNone(unityProof.BlockerCode)}");
        lines.Add($"- blockerMessage: {TextOrNone(unityProof.BlockerMessage)}");
        lines.Add($"- launchLog: {unityProof.PlayerProof.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityProof.PlayerProof.PlayLoopLogRelativePath}");
        lines.Add($"- expectedMarkerCount: {commandPlan.ExpectedPlayerMarkers.Count}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Audits");
        lines.Add(string.Empty);
        lines.Add($"- packageMediaBindingAuditPassed: {mediaAudit.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- saveLoadReplayAuditPassed: {saveLoadReplayAudit.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scriptManifestPassed: {scriptManifest.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(saveLoadReplayAudit.Rows.Select(item => $"- {item.RowId}: packageHash={item.PackageHash}, saveLoad={item.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, previewExport={item.PreviewExportPayloadConsistent.ToString().ToLowerInvariant()}"));
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
        lines.Add("No provider/media generation, network/import/download, LLM/RAG call, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 061 proof. Unity changes are limited to review-package RC marker support in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("full_campaign_playable_review_package_rc_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => FullCampaignPlayableReviewPackageRcHash.Serialize(value);

    private static string Hash(string text) => FullCampaignPlayableReviewPackageRcHash.Hash(text);

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

    private static FullCampaignPlayableReviewPackageRcDiagnostic Error(string code, string target, string message) =>
        FullCampaignPlayableReviewPackageRcDiagnostic.Error(code, target, message);
}
