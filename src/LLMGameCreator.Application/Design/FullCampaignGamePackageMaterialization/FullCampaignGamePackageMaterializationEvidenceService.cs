using System.Text;

namespace LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;

public sealed class FullCampaignGamePackageMaterializationEvidenceService
{
    public const string RelativeOutputDirectory = FullCampaignGamePackageMaterializationVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-campaign-matrix-manifest.json";
    public const string PackageMaterializationPlanJsonFileName = "package-materialization-plan.json";
    public const string MaterializedPackageInventoryJsonFileName = "materialized-package-inventory.json";
    public const string PackageValidationMatrixJsonFileName = "package-validation-matrix.json";
    public const string RuntimeConsumptionMatrixJsonFileName = "runtime-consumption-matrix.json";
    public const string PreviewExportPackagePayloadsJsonFileName = "preview-export-package-payloads.json";
    public const string UnityCommandPlanJsonFileName = "unity-package-consumption-command-plan.json";
    public const string UnityProofJsonFileName = "unity-package-consumption-proof.json";
    public const string InvalidMatrixJsonFileName = "invalid-package-materialization-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "full-campaign-gamepackage-materialization-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly FullCampaignGamePackageMaterializationSourceLoader _sourceLoader;
    private readonly FullCampaignGamePackageMaterializationUnityProofRunner _unityProofRunner;
    private readonly IFullCampaignGamePackageMaterializationRuntimeAdapter _runtimeAdapter;

    public FullCampaignGamePackageMaterializationEvidenceService(
        FullCampaignGamePackageMaterializationSourceLoader? sourceLoader = null,
        FullCampaignGamePackageMaterializationUnityProofRunner? unityProofRunner = null,
        IFullCampaignGamePackageMaterializationRuntimeAdapter? runtimeAdapter = null)
    {
        _sourceLoader = sourceLoader ?? new FullCampaignGamePackageMaterializationSourceLoader();
        _unityProofRunner = unityProofRunner ?? new FullCampaignGamePackageMaterializationUnityProofRunner();
        _runtimeAdapter = runtimeAdapter ?? new MissingFullCampaignRuntimeAdapter();
    }

    public FullCampaignGamePackageMaterializationEvidenceResult Build(string projectRootPath, FullCampaignGamePackageMaterializationOptions? options = null)
    {
        var proof = new FullCampaignUnityProof
        {
            Passed = false,
            BlockerCode = "goal060.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerProof = new FullCampaignUnityPlayerProof
            {
                Diagnostics =
                [
                    FullCampaignGamePackageMaterializationDiagnostic.Warning("goal060.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<FullCampaignGamePackageMaterializationWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        FullCampaignGamePackageMaterializationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new FullCampaignGamePackageMaterializationOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new FullCampaignUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal060.unity.pending" : "goal060.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerProof = new FullCampaignUnityPlayerProof()
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

    public async Task<FullCampaignGamePackageMaterializationWriteResult> WriteAsync(
        string projectRootPath,
        FullCampaignGamePackageMaterializationEvidenceResult result,
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
            var path = Path.GetFullPath(Path.Combine(outputDirectory, FullCampaignGamePackageMaterializationVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, stagingFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var package in result.Packages.OrderBy(item => item.PackageRelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, package.PackageRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, package.PackageJson + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var artifactScopePath = Path.Combine(outputDirectory, ArtifactScopeReportJsonFileName);
        await File.WriteAllTextAsync(artifactScopePath, result.ArtifactScopeReportJson + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(artifactScopePath);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new FullCampaignGamePackageMaterializationWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, FullCampaignGamePackageMaterializationVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private FullCampaignGamePackageMaterializationEvidenceResult BuildCore(string projectRootPath, FullCampaignUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new FullCampaignGamePackageMaterializationBuilder();

        var sourceManifest = builder.BuildSourceManifest(source);
        var plan = builder.BuildPackageMaterializationPlan(source);
        var packages = builder.MaterializePackages(plan);
        var inventory = builder.BuildPackageInventory(packages);
        var validation = builder.BuildPackageValidationMatrix(packages);
        var runtime = builder.BuildRuntimeConsumptionMatrix(plan, packages, _runtimeAdapter);
        var previewExport = builder.BuildPreviewExportPackagePayloads(packages);
        var unityCommandPlan = builder.BuildUnityCommandPlan(packages, runtime);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var stagingFiles = builder.BuildStagingFiles(source, unityCommandPlan, packages);

        var stagingDiagnostics = FullCampaignGamePackageMaterializationBuilder.SortDiagnostics(
            sourceManifest.Diagnostics
                .Concat(runtime.Rows.SelectMany(item => item.Diagnostics)));
        var diagnostics = FullCampaignGamePackageMaterializationBuilder.SortDiagnostics(
            stagingDiagnostics
                .Concat(unityProof.Diagnostics)
                .Concat(unityProof.PlayerProof.Diagnostics));

        var stagingPassed = sourceManifest.Goal059AcceptedByUserHandoff
            && sourceManifest.Goal059ReportWasGreenProducedForReview
            && sourceManifest.Goal059UnityProofPassed
            && sourceManifest.RowCount == 9
            && plan.Passed
            && inventory.Passed
            && validation.Passed
            && runtime.Passed
            && previewExport.Passed
            && unityCommandPlan.Passed
            && invalidMatrix.Passed
            && stagingDiagnostics.All(item => item.Severity is not "error" and not "critical");
        var allUnityMarkersMatched = unityProof.Passed && unityProof.PlayerProof.MissingMarkers.Count == 0;
        var implementationStatus = stagingPassed && allUnityMarkersMatched
            ? "GREEN"
            : stagingPassed && !allUnityMarkersMatched
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [PackageMaterializationPlanJsonFileName] = Serialize(plan),
            [MaterializedPackageInventoryJsonFileName] = Serialize(inventory),
            [PackageValidationMatrixJsonFileName] = Serialize(validation),
            [RuntimeConsumptionMatrixJsonFileName] = Serialize(runtime),
            [PreviewExportPackagePayloadsJsonFileName] = Serialize(previewExport),
            [UnityCommandPlanJsonFileName] = Serialize(unityCommandPlan),
            [UnityProofJsonFileName] = Serialize(unityProof.PlayerProof),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };

        var reportWithoutHash = new FullCampaignGamePackageMaterializationReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal059AcceptedByUserHandoff = sourceManifest.Goal059AcceptedByUserHandoff,
            SourceFactsConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches && item.Diagnostics.Count == 0),
            PackageMaterializationPlanPassed = plan.Passed,
            PackageInventoryPassed = inventory.Passed,
            PackageValidationMatrixPassed = validation.Passed,
            RuntimeConsumptionMatrixPassed = runtime.Passed,
            PreviewExportPackagePayloadsPassed = previewExport.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerProof.PlayerExitCode,
            AllUnityPackageMarkersMatched = allUnityMarkersMatched,
            InvalidMatrixPassed = invalidMatrix.Passed,
            MaterializedPackageCount = inventory.PackageCount,
            ValidatorCleanPackageCount = validation.ValidPackageCount,
            RuntimePassedFamilyCount = runtime.RuntimePassedFamilyCount,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            PackagePlanHash = Hash(artifactJson[PackageMaterializationPlanJsonFileName]),
            PackageInventoryHash = Hash(artifactJson[MaterializedPackageInventoryJsonFileName]),
            PackageValidationMatrixHash = Hash(artifactJson[PackageValidationMatrixJsonFileName]),
            RuntimeConsumptionMatrixHash = Hash(artifactJson[RuntimeConsumptionMatrixJsonFileName]),
            PreviewExportPackagePayloadsHash = Hash(artifactJson[PreviewExportPackagePayloadsJsonFileName]),
            UnityProofHash = Hash(artifactJson[UnityProofJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new FullCampaignGamePackageMaterializationEvidenceResult
        {
            SourceManifest = sourceManifest,
            PackageMaterializationPlan = plan,
            PackageInventory = inventory,
            PackageValidationMatrix = validation,
            RuntimeConsumptionMatrix = runtime,
            PreviewExportPackagePayloads = previewExport,
            UnityCommandPlan = unityCommandPlan,
            UnityPlayerProof = unityProof.PlayerProof,
            InvalidMatrix = invalidMatrix,
            Report = report,
            Packages = packages,
            ArtifactJsonByFileName = artifactJson,
            StagingFiles = stagingFiles,
            ArtifactScopeReportJson = RenderArtifactScopeReportJson(),
            ReportMarkdown = RenderReport(report, sourceManifest, plan, inventory, validation, runtime, previewExport, unityCommandPlan, unityProof, invalidMatrix)
        };
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal060_artifact_scope_report_v1",
            scenario = FullCampaignGamePackageMaterializationVocabulary.ProductSmokeRoute,
            gate = FullCampaignGamePackageMaterializationVocabulary.FinalGate + " required",
            allowedArtifactRoot = FullCampaignGamePackageMaterializationVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/FullCampaignGamePackageMaterialization/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignGamePackageMaterializationProductSmokeTests.cs",
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

    private static string RenderReport(
        FullCampaignGamePackageMaterializationReport report,
        FullCampaignSourceManifest sourceManifest,
        FullCampaignPackageMaterializationPlan plan,
        FullCampaignMaterializedPackageInventory inventory,
        FullCampaignPackageValidationMatrix validation,
        FullCampaignRuntimeConsumptionMatrix runtime,
        FullCampaignPreviewExportPackagePayloads previewExport,
        FullCampaignUnityPackageCommandPlan commandPlan,
        FullCampaignUnityProof unityProof,
        InvalidFullCampaignMaterializationMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Full Campaign GamePackage Materialization Report",
            string.Empty,
            "full_campaign_gamepackage_materialization_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=full_campaign_gamepackage_materialization_matrix_verification",
            $"goal059AcceptedByUserHandoff={report.Goal059AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"packageMaterializationPlanPassed={report.PackageMaterializationPlanPassed.ToString().ToLowerInvariant()}",
            $"packageInventoryPassed={report.PackageInventoryPassed.ToString().ToLowerInvariant()}",
            $"packageValidationMatrixPassed={report.PackageValidationMatrixPassed.ToString().ToLowerInvariant()}",
            $"runtimeConsumptionMatrixPassed={report.RuntimeConsumptionMatrixPassed.ToString().ToLowerInvariant()}",
            $"previewExportPackagePayloadsPassed={report.PreviewExportPackagePayloadsPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allUnityPackageMarkersMatched={report.AllUnityPackageMarkersMatched.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"materializedPackageCount={report.MaterializedPackageCount}",
            $"validatorCleanPackageCount={report.ValidatorCleanPackageCount}",
            $"runtimePassedFamilyCount={report.RuntimePassedFamilyCount}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"packagePlanHash={report.PackagePlanHash}",
            $"packageInventoryHash={report.PackageInventoryHash}",
            $"packageValidationMatrixHash={report.PackageValidationMatrixHash}",
            $"runtimeConsumptionMatrixHash={report.RuntimeConsumptionMatrixHash}",
            $"previewExportPackagePayloadsHash={report.PreviewExportPackagePayloadsHash}",
            $"unityProofHash={report.UnityProofHash}",
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
        lines.Add($"- goal059ReportWasGreenProducedForReview: {sourceManifest.Goal059ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal059UnityProofPassed: {sourceManifest.Goal059UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- sourceCampaignHash: {sourceManifest.SourceCampaignHash}");
        lines.Add($"- seedProfileMatrixHash: {sourceManifest.SeedProfileMatrixHash}");
        lines.Add($"- rowCount: {sourceManifest.RowCount}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Materialized Packages");
        lines.Add(string.Empty);
        lines.Add($"- planPassed: {plan.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- packageCount: {inventory.PackageCount}");
        lines.Add($"- validatorCleanPackageCount: {validation.ValidPackageCount}");
        foreach (var package in inventory.Packages)
        {
            lines.Add($"- {package.RowId}: family={package.FamilyId}, seed={package.SeedId}, packageId={package.PackageId}, validation={package.ValidationPassed.ToString().ToLowerInvariant()}, hash={package.PackageHash}, path={package.PackageRelativePath}");
        }

        lines.Add(string.Empty);
        lines.Add("## Runtime Consumption");
        lines.Add(string.Empty);
        lines.Add($"- passed: {runtime.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- families: {runtime.RuntimePassedFamilyCount}/{runtime.MaterializedFamilyCount}");
        foreach (var row in runtime.Rows)
        {
            lines.Add($"- {row.RowId}: loop={row.ExpectedRuntimeLoopKind}, runtimePassed={row.RuntimePassed.ToString().ToLowerInvariant()}, stateChanged={row.StateChanged.ToString().ToLowerInvariant()}, saveLoad={row.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, changed={string.Join(",", row.ChangedStateKeys)}");
        }

        lines.Add(string.Empty);
        lines.Add("## Preview/Export");
        lines.Add(string.Empty);
        lines.Add($"- passed: {previewExport.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- immutability: {previewExport.PackageImmutabilityAuditPassed.ToString().ToLowerInvariant()}");
        lines.AddRange(previewExport.Rows.Select(item => $"- {item.RowId}: preview={item.PreviewPayloadRef}, export={item.ExportPayloadRef}, immutable={item.PackageImmutable.ToString().ToLowerInvariant()}"));
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
        lines.Add($"- expectedMarkerCount: {commandPlan.ExpectedPlayerMarkers.Count}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
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
        lines.Add("No provider/media generation, network/import/download, LLM/RAG call, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 060 proof. Unity changes are limited to package-consumption marker support in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("full_campaign_gamepackage_materialization_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => FullCampaignGamePackageMaterializationHash.Serialize(value);

    private static string Hash(string text) => FullCampaignGamePackageMaterializationHash.Hash(text);

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
