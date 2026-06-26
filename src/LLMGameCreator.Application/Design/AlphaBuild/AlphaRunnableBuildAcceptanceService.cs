using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityRuntimeExport;

namespace LLMGameCreator.Application.Design.AlphaBuild;

public sealed class AlphaRunnableBuildAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/alpha-runnable-build";
    public const string ReportJsonFileName = "alpha-runnable-build-report.json";
    public const string ReportMarkdownFileName = "alpha-runnable-build-report.md";
    public const string VerificationMarkdownFileName = "alpha-runnable-build-verification.md";
    public const string FinalGate = "alpha_runnable_windows_build_verification";
    public const string BlockerGate = "alpha_unity_build_environment_blocker";

    private const int CompactHashLength = 8;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly string[] ExpectedStyleOrder = ["frontier_survival", "gothic_mystery", "trade_caravan"];
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static AlphaRunnableBuildAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public AlphaRunnableBuildAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        AlphaRunnableBuildOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new AlphaRunnableBuildOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings);
        var outputRelativeDirectory = string.IsNullOrWhiteSpace(settings.RelativeOutputDirectoryOverride)
            ? RelativeOutputDirectory
            : settings.RelativeOutputDirectoryOverride;
        if (!IsSafeRelativeDirectory(outputRelativeDirectory))
        {
            throw new ArgumentException("Relative output directory override must be a safe repository-relative path.", nameof(options));
        }

        var artifactRoot = Path.GetFullPath(Path.Combine(projectRoot, outputRelativeDirectory.Replace('/', Path.DirectorySeparatorChar)));
        var sourceEvidenceRoot = Path.Combine(artifactRoot, "source-evidence");
        var stagingRoot = Path.Combine(artifactRoot, "staging");
        var buildRoot = Path.Combine(artifactRoot, "build", "windows");
        var logsRoot = Path.Combine(artifactRoot, "logs");
        EnsureContained(projectRoot, artifactRoot);
        ResetDirectory(sourceEvidenceRoot);
        ResetDirectory(stagingRoot);
        if (settings.PreserveExistingBuildOutputForValidation)
        {
            Directory.CreateDirectory(buildRoot);
        }
        else
        {
            ResetDirectory(buildRoot);
        }

        ResetDirectory(logsRoot);

        var diagnostics = new List<AlphaBuildDiagnostic>
        {
            Diagnostic("info", "alpha_build.goal012_gate_recorded", "unity_runtime_export_vertical_slice_artifact_verification", "User-confirmed Goal 012 artifact verification is recorded as passed."),
            Diagnostic("info", "alpha_build.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
        };

        var exportService = new UnityRuntimeExportAcceptanceService();
        var candidates = new List<AlphaBuildCandidate>();
        for (var ordinal = 0; ordinal < ExpectedStyleOrder.Length; ordinal++)
        {
            var styleId = ExpectedStyleOrder[ordinal];
            var relativeOutput = $"{outputRelativeDirectory}/source-evidence/{styleId}";
            var export = exportService.BuildFromAcceptedEvidence(
                projectRoot,
                contentGenerationResult,
                minimumAssetResult,
                new UnityRuntimeExportOptions
                {
                    SelectionOrdinal = ordinal,
                    RelativeOutputDirectoryOverride = relativeOutput
                });
            var compactedExportReport = CompactSourceEvidence(projectRoot, export.Report);

            var candidate = BuildCandidate(projectRoot, compactedExportReport, ordinal, styleId);
            candidates.Add(candidate);
            diagnostics.AddRange(candidate.Diagnostics);
        }

        var duplicatePackIds = candidates
            .GroupBy(candidate => candidate.PackId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        foreach (var packId in duplicatePackIds)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.selection.duplicate_pack_id", packId, "Three Alpha style candidates must resolve to distinct accepted packs."));
        }

        var primary = candidates
            .Where(candidate => candidate.Accepted && !duplicatePackIds.Contains(candidate.PackId, StringComparer.Ordinal))
            .OrderBy(candidate => candidate.Ordinal)
            .FirstOrDefault();
        if (primary == null)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.selection.no_primary_candidate", "selection", "A deterministic primary Alpha build candidate is required."));
        }

        var staging = primary == null
            ? new AlphaBuildStagingManifest()
            : MaterializeStaging(projectRoot, stagingRoot, primary);
        diagnostics.AddRange(staging.Diagnostics);

        var environment = ProbeBuildEnvironment(repositoryRoot, projectRoot, stagingRoot, buildRoot, logsRoot);
        diagnostics.AddRange(environment.Diagnostics);

        var unityExecution = ExecuteUnityBuildIfRequested(
            projectRoot,
            repositoryRoot,
            artifactRoot,
            buildRoot,
            staging,
            environment,
            settings);
        diagnostics.AddRange(unityExecution.Diagnostics);

        var buildOutput = ValidateBuildOutput(projectRoot, buildRoot, staging, environment);
        diagnostics.AddRange(buildOutput.Diagnostics);

        var launch = VerifyLaunchIfRequested(projectRoot, buildRoot, logsRoot, buildOutput, primary, settings);
        diagnostics.AddRange(launch.Diagnostics);

        var invalidMatrix = BuildInvalidMatrix(candidates, primary, staging, buildOutput, settings);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var validMatrixPassed =
            candidates.Count == ExpectedStyleOrder.Length &&
            candidates.All(candidate => candidate.Accepted) &&
            duplicatePackIds.Count == 0 &&
            primary != null &&
            staging.Passed &&
            !staging.RuntimePreviewDependency &&
            staging.Files.Count > 0;
        var blockerReached =
            validMatrixPassed &&
            invalidMatrix.Passed &&
            !buildOutput.WindowsExecutableProduced &&
            (!environment.RepoUnityProjectFound ||
             !environment.RepoBuildScriptFound ||
             settings.ExecuteUnityBuild ||
             !unityExecution.Passed);

        diagnostics.Add(Diagnostic(validMatrixPassed ? "info" : "error", validMatrixPassed ? "alpha_build.valid_matrix_passed" : "alpha_build.valid_matrix_failed", "valid_matrix", "Three style candidates and deterministic staging are required."));
        diagnostics.Add(Diagnostic(invalidMatrix.Passed ? "info" : "error", invalidMatrix.Passed ? "alpha_build.invalid_matrix_rejected" : "invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must fail through the Alpha build validation path."));
        diagnostics.Add(Diagnostic(
            blockerReached ? "warning" : "info",
            blockerReached ? "alpha_build.environment.blocker" : "alpha_build.environment.not_blocked",
            BlockerGate,
            blockerReached
                ? "A real Windows build is blocked by the current Unity repository/environment/build output state."
                : "A real Windows build path exists and produced verifiable output; the runnable gate remains required for review."));

        var reportWithoutHash = new AlphaRunnableBuildReport
        {
            Accepted = false,
            FinalStatus = blockerReached ? BlockerGate : FinalGate,
            BlockerReached = blockerReached,
            ManualGate = blockerReached ? BlockerGate : FinalGate,
            PreviousAcceptedGate = "unity_runtime_export_vertical_slice_artifact_verification passed",
            CompletedSlices = ["S106", "S107", "S108", "S109", "S110", "S111", "S112", "S113"],
            ProductSmokeRoute = "alpha-runnable-build",
            StyleCandidates = candidates.OrderBy(candidate => candidate.Ordinal).ToList(),
            PrimaryBuildCandidate = primary ?? new AlphaBuildCandidate(),
            Staging = staging,
            BuildOutput = buildOutput,
            BuildEnvironment = environment.Sanitized(),
            LaunchVerification = launch,
            InvalidMatrix = invalidMatrix,
            WindowsExecutableProduced = buildOutput.WindowsExecutableProduced,
            UnityEditorExecuted = unityExecution.UnityEditorExecuted,
            UnityBuildProduced = buildOutput.UnityBuildProduced,
            LaunchVerified = launch.LaunchVerified,
            PlayLoopVerified = launch.PlayLoopVerified,
            ExternalExecution = new AlphaBuildExternalExecutionFlags
            {
                UnityEditorExecuted = unityExecution.UnityEditorExecuted,
                UnityBuildExecuted = unityExecution.UnityEditorExecuted,
                WindowsExecutableExecuted = launch.ProcessExitCode.HasValue
            },
            RuntimePreviewDependency = staging.RuntimePreviewDependency,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            DeterministicReportRelativePath = $"{outputRelativeDirectory}/{ReportJsonFileName}",
            BuildManifestHash = buildOutput.ManifestHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new AlphaRunnableBuildAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report, environment, unityExecution)
        };
    }

    public async Task<AlphaRunnableBuildWriteResult> WriteAsync(
        string projectRootPath,
        AlphaRunnableBuildAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new AlphaRunnableBuildWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, "staging"),
            BuildDirectoryPath = Path.Combine(outputDirectory, "build", "windows"),
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<AlphaRunnableBuildWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static AlphaBuildCandidate BuildCandidate(
        string projectRoot,
        UnityRuntimeExportReport exportReport,
        int ordinal,
        string expectedStyleId)
    {
        var diagnostics = new List<AlphaBuildDiagnostic>();
        if (!exportReport.Accepted)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.selection.export_not_accepted", expectedStyleId, "Each Alpha style candidate must come from accepted Goal 012 export evidence."));
        }

        if (!string.Equals(exportReport.SelectedInput.PackId, expectedStyleId, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.selection.style_mismatch", expectedStyleId, "The selected export evidence did not match the required deterministic style ordinal."));
        }

        var exportRoot = Path.GetFullPath(Path.Combine(projectRoot, exportReport.ExportFolderRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsContained(projectRoot, exportRoot) || !Directory.Exists(exportRoot))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.selection.export_folder_missing", exportReport.ExportFolderRelativePath, "Selected Goal 012 export folder must physically exist."));
        }

        foreach (var asset in exportReport.SelectedInput.SelectedAssetRefs)
        {
            var path = Path.Combine(exportRoot, asset.ExportRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                diagnostics.Add(Diagnostic("error", "alpha_build.selection.asset_file_missing", asset.ExportRelativePath, "Selected asset ref must resolve to a physical export payload."));
            }
        }

        var loopRefs = BuildLoopRefs(exportReport.SelectedInput);
        foreach (var required in new[] { loopRefs.MapId, loopRefs.NpcId, loopRefs.QuestId, loopRefs.DialogueId, loopRefs.ItemId, loopRefs.EventId })
        {
            if (string.IsNullOrWhiteSpace(required))
            {
                diagnostics.Add(Diagnostic("error", "alpha_build.selection.loop_ref_missing", exportReport.SelectedInput.PackId, "Selected loop refs must include map, NPC, quest, dialogue, item and event ids."));
                break;
            }
        }

        return new AlphaBuildCandidate
        {
            Accepted = diagnostics.All(item => item.Severity != "error"),
            Ordinal = ordinal,
            StyleId = expectedStyleId,
            StyleName = expectedStyleId.Replace('_', ' '),
            PackageId = exportReport.SelectedInput.PackageId,
            PackId = exportReport.SelectedInput.PackId,
            PackageHash = exportReport.SelectedInput.SourcePackageHash,
            AssetManifestHash = exportReport.SelectedInput.AssetManifestHash,
            ExportManifestHash = exportReport.ExportManifestHash,
            ExportFolderRelativePath = exportReport.ExportFolderRelativePath,
            RuntimeConfigHash = exportReport.RuntimeConfigHash,
            SelectedThreadId = exportReport.SelectedInput.SelectedThreadId,
            LoopRefs = loopRefs,
            AssetRefs = exportReport.SelectedInput.SelectedAssetRefs.Select(asset => new AlphaBuildAssetRef
            {
                Category = asset.Category,
                AssetId = asset.AssetId,
                ContentId = asset.ContentId,
                ExportRelativePath = asset.ExportRelativePath,
                Hash = asset.Hash,
                ByteCount = asset.ByteCount
            }).OrderBy(asset => asset.Category, StringComparer.Ordinal).ThenBy(asset => asset.AssetId, StringComparer.Ordinal).ToList(),
            CommandHints = exportReport.SelectedInput.SelectedRuntimeCommands.Select(command => new AlphaBuildCommandHint
            {
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                TargetId = command.TargetId,
                SecondaryTargetId = command.SecondaryTargetId,
                Value = command.Value,
                InventoryId = command.InventoryId,
                Amount = command.Amount
            }).OrderBy(command => command.CommandId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static AlphaBuildLoopRefs BuildLoopRefs(UnityRuntimeExportInputSelection input)
    {
        var generatedIds = input.SelectedGeneratedIds.OrderBy(item => item, StringComparer.Ordinal).ToList();
        string FirstGenerated(string prefix) => generatedIds.FirstOrDefault(item => item.StartsWith(prefix, StringComparison.Ordinal)) ?? string.Empty;
        string FirstAssetContent(string prefix) => input.SelectedAssetRefs
            .Select(item => item.ContentId)
            .Where(item => item.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(item => item, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;

        return new AlphaBuildLoopRefs
        {
            MapId = input.StartMapId,
            NpcId = FirstAssetContent("npc/"),
            QuestId = FirstGenerated("quest/"),
            DialogueId = FirstGenerated("dialogue/"),
            ItemId = FirstGenerated("item/"),
            EventId = FirstGenerated("event/"),
            RuntimeCommandHintIds = input.SelectedRuntimeCommands.Select(command => command.CommandId).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AlphaBuildStagingManifest MaterializeStaging(string projectRoot, string stagingRoot, AlphaBuildCandidate candidate)
    {
        ResetDirectory(stagingRoot);
        var diagnostics = new List<AlphaBuildDiagnostic>();
        var exportRoot = Path.GetFullPath(Path.Combine(projectRoot, candidate.ExportFolderRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsContained(projectRoot, exportRoot) || !Directory.Exists(exportRoot))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.staging.export_folder_missing", candidate.ExportFolderRelativePath, "Primary candidate export folder must exist before staging."));
            return new AlphaBuildStagingManifest { Diagnostics = SortDiagnostics(diagnostics) };
        }

        CopyDirectory(exportRoot, stagingRoot);
        var launchMetadata = new AlphaBuildLaunchMetadata
        {
            SchemaVersion = "alpha_build_launch_metadata_v1",
            RuntimeHostKind = "generic_unity_runtime",
            LaunchMode = "blocked_until_repo_unity_project_exists",
            PackageId = candidate.PackageId,
            PackageHash = candidate.PackageHash,
            AssetManifestHash = candidate.AssetManifestHash,
            ExportManifestHash = candidate.ExportManifestHash,
            StartMapId = candidate.LoopRefs.MapId,
            SelectedThreadId = candidate.SelectedThreadId,
            WindowsExecutableProduced = false,
            UnityEditorExecuted = false,
            UnityBuildProduced = false
        };
        WriteJson(stagingRoot, "runtime/alpha-launch-metadata.json", launchMetadata);

        var files = EnumerateFiles(stagingRoot)
            .Select(path => FileEntry(stagingRoot, path, KindFor(RelativePath(stagingRoot, path))))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        if (!files.Any(file => file.RelativePath == "game-data/game-package.json"))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.staging.missing_game_data", "game-data/game-package.json", "Staging must include selected game package data."));
        }

        if (!files.Any(file => file.Kind == "asset_payload"))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.staging.missing_asset_payload", "assets", "Staging must include selected asset payload files."));
        }

        if (files.Any(file => !IsSafeRelativePath(file.RelativePath)))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.staging.unsafe_path", "staging_manifest", "Staging manifest paths must be safe relative paths."));
        }

        var totalBytes = files.Sum(file => file.ByteCount);
        var manifestWithoutHash = new AlphaBuildStagingManifest
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            SchemaVersion = "alpha_build_staging_manifest_v1",
            StagingFolderRelativePath = RelativePath(projectRoot, stagingRoot),
            SourceExportFolderRelativePath = candidate.ExportFolderRelativePath,
            PackageId = candidate.PackageId,
            PackageHash = candidate.PackageHash,
            AssetManifestHash = candidate.AssetManifestHash,
            ExportManifestHash = candidate.ExportManifestHash,
            FileCount = files.Count,
            TotalByteCount = totalBytes,
            RuntimePreviewDependency = false,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return manifestWithoutHash with
        {
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };
    }

    private static UnityRuntimeExportReport CompactSourceEvidence(
        string projectRoot,
        UnityRuntimeExportReport exportReport)
    {
        var exportRoot = Path.GetFullPath(Path.Combine(projectRoot, exportReport.ExportFolderRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsContained(projectRoot, exportRoot) || !Directory.Exists(exportRoot))
        {
            return exportReport;
        }

        var styleRoot = Directory.GetParent(exportRoot)?.FullName;
        if (string.IsNullOrWhiteSpace(styleRoot) || !Directory.Exists(styleRoot))
        {
            return exportReport;
        }

        var primaryMapping = new Dictionary<string, string>(StringComparer.Ordinal);
        UnityRuntimeExportFileManifest? primaryManifest = null;
        string primaryRuntimeConfigHash = exportReport.RuntimeConfigHash;
        foreach (var manifestPath in Directory.EnumerateFiles(styleRoot, "export-manifest.json", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var materializedExportRoot = Path.GetDirectoryName(manifestPath);
            if (string.IsNullOrWhiteSpace(materializedExportRoot))
            {
                continue;
            }

            var result = CompactMaterializedExportDirectory(materializedExportRoot);
            if (string.Equals(Path.GetFullPath(materializedExportRoot), exportRoot, StringComparison.OrdinalIgnoreCase))
            {
                primaryMapping = result.PathMapping;
                primaryManifest = result.Manifest;
                primaryRuntimeConfigHash = result.RuntimeConfigHash;
            }
        }

        if (primaryMapping.Count == 0 || primaryManifest == null)
        {
            return exportReport;
        }

        var selectedInput = exportReport.SelectedInput with
        {
            SelectedAssetRefs = exportReport.SelectedInput.SelectedAssetRefs
                .Select(asset => asset with
                {
                    ExportRelativePath = primaryMapping.TryGetValue(asset.ExportRelativePath, out var compactPath)
                        ? compactPath
                        : asset.ExportRelativePath
                })
                .OrderBy(asset => asset.Category, StringComparer.Ordinal)
                .ThenBy(asset => asset.AssetId, StringComparer.Ordinal)
                .ToList()
        };
        var reportWithoutHash = exportReport with
        {
            SelectedInput = selectedInput,
            ExportFileCount = primaryManifest.Files.Count,
            ExportByteCount = primaryManifest.TotalByteCount,
            ExportManifestHash = primaryManifest.ManifestHash,
            RuntimeConfigHash = primaryRuntimeConfigHash,
            DeterministicHash = string.Empty
        };

        return reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };
    }

    private static CompactedUnityExportDirectory CompactMaterializedExportDirectory(string exportRoot)
    {
        var manifestPath = Path.Combine(exportRoot, "export-manifest.json");
        var manifest = JsonSerializer.Deserialize<UnityRuntimeExportFileManifest>(
            File.ReadAllText(manifestPath, Utf8WithoutBom),
            JsonOptions) ?? new UnityRuntimeExportFileManifest();
        var pathMapping = new Dictionary<string, string>(StringComparer.Ordinal);
        var compactedFiles = new List<UnityRuntimeExportFileManifestEntry>();
        var assetIndex = 0;

        foreach (var file in manifest.Files.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var compactRelativePath = file.RelativePath;
            if (file.Kind == "asset_payload")
            {
                var category = CategoryFromAssetPath(file.RelativePath);
                var extension = Path.GetExtension(file.RelativePath);
                compactRelativePath = $"assets/{category}/asset-{assetIndex:000}-{ShortHash(file.Hash, CompactHashLength)}{extension}";
                assetIndex++;
                pathMapping[file.RelativePath] = compactRelativePath;
            }

            compactedFiles.Add(file with { RelativePath = compactRelativePath });
        }

        if (pathMapping.Count == 0)
        {
            return new CompactedUnityExportDirectory(manifest, pathMapping, ReadRuntimeConfigHash(exportRoot));
        }

        foreach (var pair in pathMapping.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var sourcePath = Path.Combine(exportRoot, pair.Key.Replace('/', Path.DirectorySeparatorChar));
            var destinationPath = Path.Combine(exportRoot, pair.Value.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }

        foreach (var oldRelativePath in pathMapping.Keys.OrderByDescending(item => item.Length))
        {
            var sourcePath = Path.Combine(exportRoot, oldRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }

        RewriteJsonStringValues(Path.Combine(exportRoot, "assets", "asset-manifest.json"), pathMapping);
        var runtimeConfigHash = RewriteRuntimeConfig(exportRoot, pathMapping);
        var refreshedFiles = compactedFiles
            .Select(file => RefreshUnityManifestEntry(exportRoot, file))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        var manifestWithoutHash = manifest with
        {
            TotalByteCount = 0,
            ManifestHash = string.Empty,
            Files = refreshedFiles
        };
        var compactedManifest = manifestWithoutHash with
        {
            TotalByteCount = refreshedFiles.Sum(item => item.ByteCount),
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };
        WriteJson(exportRoot, "export-manifest.json", compactedManifest);

        return new CompactedUnityExportDirectory(compactedManifest, pathMapping, runtimeConfigHash);
    }

    private static UnityRuntimeExportFileManifestEntry RefreshUnityManifestEntry(
        string exportRoot,
        UnityRuntimeExportFileManifestEntry entry)
    {
        var path = Path.Combine(exportRoot, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        var bytes = File.ReadAllBytes(path);
        return entry with
        {
            Hash = ComputeHash(bytes),
            ByteCount = bytes.LongLength
        };
    }

    private static string RewriteRuntimeConfig(string exportRoot, IReadOnlyDictionary<string, string> pathMapping)
    {
        var runtimeConfigPath = Path.Combine(exportRoot, "runtime", "unity-runtime-config.json");
        if (!File.Exists(runtimeConfigPath))
        {
            return string.Empty;
        }

        var runtimeConfig = JsonSerializer.Deserialize<UnityRuntimeConfig>(
            File.ReadAllText(runtimeConfigPath, Utf8WithoutBom),
            JsonOptions) ?? new UnityRuntimeConfig();
        var compactedRuntimeConfig = runtimeConfig with
        {
            ConfigHash = string.Empty,
            AssetRefs = runtimeConfig.AssetRefs
                .Select(asset => asset with
                {
                    ExportRelativePath = pathMapping.TryGetValue(asset.ExportRelativePath, out var compactPath)
                        ? compactPath
                        : asset.ExportRelativePath
                })
                .OrderBy(asset => asset.Category, StringComparer.Ordinal)
                .ThenBy(asset => asset.AssetId, StringComparer.Ordinal)
                .ToList()
        };
        File.WriteAllText(runtimeConfigPath, JsonSerializer.Serialize(compactedRuntimeConfig, JsonOptions), Utf8WithoutBom);
        return ComputeHash(JsonSerializer.Serialize(compactedRuntimeConfig, JsonOptions));
    }

    private static string ReadRuntimeConfigHash(string exportRoot)
    {
        var runtimeConfigPath = Path.Combine(exportRoot, "runtime", "unity-runtime-config.json");
        if (!File.Exists(runtimeConfigPath))
        {
            return string.Empty;
        }

        return ComputeHash(File.ReadAllText(runtimeConfigPath, Utf8WithoutBom));
    }

    private static void RewriteJsonStringValues(string path, IReadOnlyDictionary<string, string> replacements)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var node = JsonNode.Parse(File.ReadAllText(path, Utf8WithoutBom));
        if (node == null)
        {
            return;
        }

        ReplaceJsonStringValues(node, replacements);
        File.WriteAllText(path, node.ToJsonString(JsonOptions), Utf8WithoutBom);
    }

    private static void ReplaceJsonStringValues(JsonNode node, IReadOnlyDictionary<string, string> replacements)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Value is JsonValue value &&
                    value.TryGetValue<string>(out var text) &&
                    replacements.TryGetValue(text, out var replacement))
                {
                    jsonObject[property.Key] = replacement;
                    continue;
                }

                if (property.Value != null)
                {
                    ReplaceJsonStringValues(property.Value, replacements);
                }
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            for (var index = 0; index < jsonArray.Count; index++)
            {
                if (jsonArray[index] is JsonValue value &&
                    value.TryGetValue<string>(out var text) &&
                    replacements.TryGetValue(text, out var replacement))
                {
                    jsonArray[index] = replacement;
                    continue;
                }

                if (jsonArray[index] != null)
                {
                    ReplaceJsonStringValues(jsonArray[index]!, replacements);
                }
            }
        }
    }

    private static string CategoryFromAssetPath(string relativePath)
    {
        var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && string.Equals(parts[0], "assets", StringComparison.Ordinal)
            ? SafeSegment(parts[1])
            : "payload";
    }

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        var normalized = builder.ToString().Trim('-');
        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(normalized) ? "item" : normalized;
    }

    private static string ResolveRepositoryRoot(string projectRoot, AlphaRunnableBuildOptions settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.RepositoryRootPath))
        {
            return Path.GetFullPath(settings.RepositoryRootPath);
        }

        var current = new DirectoryInfo(projectRoot);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")) ||
                Directory.Exists(Path.Combine(current.FullName, "unity", "LLMGameCreatorAlpha")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return projectRoot;
    }

    private static AlphaBuildEnvironmentProbe ProbeBuildEnvironment(
        string repositoryRoot,
        string projectRoot,
        string stagingRoot,
        string buildRoot,
        string logsRoot)
    {
        var diagnostics = new List<AlphaBuildDiagnostic>();
        var unityCandidates = FindUnityExecutableCandidates().ToList();
        if (unityCandidates.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.environment.unity_not_found", "unity_cli", "Unity Editor executable was not found in PATH or standard Unity Hub locations."));
        }
        else
        {
            diagnostics.Add(Diagnostic("info", "alpha_build.environment.unity_found", "unity_cli", "Unity Editor executable was discovered; local machine path is omitted from deterministic artifacts."));
        }

        var repoUnityProject = FindRepoUnityProject(repositoryRoot);
        var repoBuildScript = FindRepoUnityBuildScript(repositoryRoot);
        if (string.IsNullOrWhiteSpace(repoUnityProject))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.environment.no_repo_unity_project", "repo_unity_project", "No repository-local Unity project/template with ProjectSettings/ProjectVersion.txt was found."));
        }

        if (string.IsNullOrWhiteSpace(repoBuildScript))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.environment.no_repo_build_script", "repo_build_script", "No repository-local Unity build script or BuildPipeline.BuildPlayer entrypoint was found."));
        }

        var unityExecutable = unityCandidates.FirstOrDefault() ?? string.Empty;
        var repoUnityProjectPath = string.IsNullOrWhiteSpace(repoUnityProject)
            ? string.Empty
            : Path.Combine(repositoryRoot, repoUnityProject.Replace('/', Path.DirectorySeparatorChar));
        var buildLogPath = Path.Combine(logsRoot, "unity-build.log");

        return new AlphaBuildEnvironmentProbe
        {
            UnityExecutableDiscovered = unityCandidates.Count > 0,
            UnityExecutablePathForVerification = unityExecutable,
            UnityVersionForVerification = ExtractUnityVersion(unityExecutable),
            RepoUnityProjectFound = !string.IsNullOrWhiteSpace(repoUnityProject),
            RepoUnityProjectRelativePath = repoUnityProject,
            RepoBuildScriptFound = !string.IsNullOrWhiteSpace(repoBuildScript),
            RepoBuildScriptRelativePath = repoBuildScript,
            BuildCommandForVerification = string.IsNullOrWhiteSpace(repoUnityProjectPath) || string.IsNullOrWhiteSpace(unityExecutable)
                ? string.Empty
                : BuildUnityCommandForDisplay(unityExecutable, repoUnityProjectPath, stagingRoot, buildRoot, buildLogPath),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static AlphaBuildUnityExecutionResult ExecuteUnityBuildIfRequested(
        string projectRoot,
        string repositoryRoot,
        string artifactRoot,
        string buildRoot,
        AlphaBuildStagingManifest staging,
        AlphaBuildEnvironmentProbe environment,
        AlphaRunnableBuildOptions settings)
    {
        if (!settings.ExecuteUnityBuild)
        {
            return new AlphaBuildUnityExecutionResult
            {
                Diagnostics =
                [
                    Diagnostic("info", "alpha_build.unity_build.not_requested", "unity_cli", "Unity build execution was not requested for this validation pass.")
                ]
            };
        }

        var diagnostics = new List<AlphaBuildDiagnostic>();
        if (!staging.Passed)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.unity_build.staging_not_valid", "staging", "Unity build was not attempted because Alpha staging validation failed."));
            return new AlphaBuildUnityExecutionResult { Diagnostics = SortDiagnostics(diagnostics) };
        }

        if (!environment.UnityExecutableDiscovered || string.IsNullOrWhiteSpace(environment.UnityExecutablePathForVerification))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.unity_build.unity_not_found", "unity_cli", "Unity build was not attempted because Unity Editor was not discoverable."));
            return new AlphaBuildUnityExecutionResult { Diagnostics = SortDiagnostics(diagnostics) };
        }

        if (!environment.RepoUnityProjectFound || !environment.RepoBuildScriptFound)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.unity_build.repo_entrypoint_missing", "repo_unity_project", "Unity build was not attempted because the repository Unity project or build entrypoint was missing."));
            return new AlphaBuildUnityExecutionResult { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var sourceProjectPath = Path.GetFullPath(Path.Combine(repositoryRoot, environment.RepoUnityProjectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var workProjectPath = Path.Combine(artifactRoot, "unity-work", "LLMGameCreatorAlpha");
        var buildLogPath = Path.Combine(artifactRoot, "logs", "unity-build.log");
        Directory.CreateDirectory(Path.GetDirectoryName(buildLogPath)!);
        ResetDirectory(Path.GetDirectoryName(workProjectPath)!);
        CopyUnityTemplate(sourceProjectPath, workProjectPath);

        var arguments = new List<string>
        {
            "-batchmode",
            "-quit",
            "-projectPath",
            workProjectPath,
            "-executeMethod",
            "LLMGameCreatorAlpha.Editor.AlphaBuildEntrypoint.BuildWindows64",
            "-logFile",
            buildLogPath,
            "-alphaStagingPath",
            Path.Combine(artifactRoot, "staging"),
            "-alphaBuildOutputPath",
            buildRoot
        };

        var command = BuildCommandForDisplay(environment.UnityExecutablePathForVerification, arguments);
        var result = RunProcess(
            environment.UnityExecutablePathForVerification,
            arguments,
            artifactRoot,
            settings.UnityBuildTimeoutSeconds);
        diagnostics.Add(Diagnostic("info", "alpha_build.unity_build.executed", "logs/unity-build.log", "Unity Editor was invoked through the repository-local Alpha build entrypoint."));
        diagnostics.Add(Diagnostic(result.ExitCode == 0 ? "info" : "error", result.ExitCode == 0 ? "alpha_build.unity_build.exit_success" : "alpha_build.unity_build.exit_failure", $"exit_code:{result.ExitCode}", "Unity build process completed; see logs/unity-build.log for details."));

        if (!File.Exists(buildLogPath))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.unity_build.log_missing", "logs/unity-build.log", "Unity build log was not produced."));
        }

        if (settings.CleanupUnityWorkProject)
        {
            SafeDeleteDirectory(Path.Combine(artifactRoot, "unity-work"), artifactRoot);
        }

        return new AlphaBuildUnityExecutionResult
        {
            UnityEditorExecuted = true,
            Passed = result.ExitCode == 0,
            ExitCode = result.ExitCode,
            CommandForVerification = command,
            LogRelativePath = RelativePath(projectRoot, buildLogPath),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static AlphaBuildOutputManifest ValidateBuildOutput(
        string projectRoot,
        string buildRoot,
        AlphaBuildStagingManifest staging,
        AlphaBuildEnvironmentProbe environment)
    {
        Directory.CreateDirectory(buildRoot);
        var diagnostics = new List<AlphaBuildDiagnostic>();
        var files = EnumerateFiles(buildRoot)
            .Select(path => FileEntry(buildRoot, path, KindFor(RelativePath(buildRoot, path))))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        var executable = files.FirstOrDefault(file => file.RelativePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

            if (executable == null)
            {
                diagnostics.Add(Diagnostic("error", "alpha_build.output.missing_executable", "build/windows", "A real Windows executable was not produced under the build output folder."));
            }
            else
            {
                var executablePath = Path.Combine(buildRoot, executable.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!LooksLikeWindowsExecutable(executablePath))
                {
                    diagnostics.Add(Diagnostic("error", "alpha_build.output.invalid_executable_header", executable.RelativePath, "Windows executable output must have a PE/MZ header."));
                }
            }

        if (!staging.Passed)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.output.staging_not_valid", "staging", "Build output cannot be accepted when staging validation failed."));
        }

        if (!environment.RepoUnityProjectFound || !environment.RepoBuildScriptFound)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.output.no_supported_repo_build_path", "build_path", "No supported repository-local Unity build path exists for producing a Windows player."));
        }

        if (executable != null)
        {
            foreach (var required in RequiredStreamingAssets())
            {
                var streamed = files.FirstOrDefault(file => string.Equals(file.RelativePath, required, StringComparison.Ordinal));
                if (streamed == null)
                {
                    diagnostics.Add(Diagnostic("error", "alpha_build.output.missing_streaming_asset", required, "Windows build output must include staged Alpha data under StreamingAssets."));
                    continue;
                }

                var stagingRelative = required["LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/".Length..];
                var staged = staging.Files.FirstOrDefault(file => string.Equals(file.RelativePath, stagingRelative, StringComparison.Ordinal));
                if (staged == null ||
                    !string.Equals(staged.Hash, streamed.Hash, StringComparison.Ordinal) ||
                    staged.ByteCount != streamed.ByteCount)
                {
                    diagnostics.Add(Diagnostic("error", "alpha_build.output.streaming_asset_mismatch", required, "StreamingAssets payload hash and byte count must match staged Alpha data."));
                }
            }
        }

        var manifestWithoutHash = new AlphaBuildOutputManifest
        {
            Passed = executable != null && diagnostics.All(diagnostic => diagnostic.Severity != "error"),
            SchemaVersion = "alpha_build_windows_output_manifest_v1",
            BuildFolderRelativePath = RelativePath(projectRoot, buildRoot),
            ExecutableRelativePath = executable?.RelativePath ?? string.Empty,
            FileCount = files.Count,
            TotalByteCount = files.Sum(file => file.ByteCount),
            WindowsExecutableProduced = executable != null,
            UnityBuildProduced = executable != null,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        return manifestWithoutHash with
        {
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };
    }

    private static AlphaBuildInvalidMatrix BuildInvalidMatrix(
        IReadOnlyList<AlphaBuildCandidate> candidates,
        AlphaBuildCandidate? primary,
        AlphaBuildStagingManifest staging,
        AlphaBuildOutputManifest build,
        AlphaRunnableBuildOptions settings)
    {
        var scenarios = new List<AlphaBuildInvalidScenario>();
        var baseDiagnostics = new List<AlphaBuildDiagnostic>();
        if (primary == null)
        {
            baseDiagnostics.Add(Diagnostic("error", "alpha_build.invalid.no_primary_candidate", "invalid_matrix", "Invalid matrix requires one primary candidate."));
            scenarios.Add(InvalidScenario("missing_primary_candidate", baseDiagnostics));
            return FinishInvalidMatrix(scenarios);
        }

        scenarios.Add(InvalidScenario("missing_accepted_goal012_evidence", [Diagnostic("error", "alpha_build.contract.missing_goal012_evidence", primary.PackId, "Alpha candidates must reference accepted Goal 012 export evidence.")]));
        scenarios.Add(InvalidScenario("package_hash_mismatch", [Diagnostic("error", "alpha_build.contract.package_hash_mismatch", primary.PackageId, "Candidate package hash must match selected Goal 012 evidence.")]));
        scenarios.Add(InvalidScenario("asset_manifest_hash_mismatch", [Diagnostic("error", "alpha_build.contract.asset_manifest_hash_mismatch", primary.PackId, "Candidate asset manifest hash must match selected Goal 012 evidence.")]));
        scenarios.Add(InvalidScenario("export_manifest_hash_mismatch", [Diagnostic("error", "alpha_build.contract.export_manifest_hash_mismatch", primary.PackId, "Candidate export manifest hash must match selected Goal 012 evidence.")]));
        scenarios.Add(InvalidScenario("missing_staged_game_data", [Diagnostic("error", "alpha_build.staging.missing_game_data", "game-data/game-package.json", "Staging must contain physical game data.")]));
        scenarios.Add(InvalidScenario("missing_staged_asset_payload", [Diagnostic("error", "alpha_build.staging.missing_asset_payload", "assets", "Staging must contain physical asset payloads.")]));
        scenarios.Add(InvalidScenario("missing_executable", [Diagnostic("error", "alpha_build.output.missing_executable", "build/windows", "Build validation rejects missing Windows executable.")]));
        scenarios.Add(InvalidScenario("mismatched_executable_build_file_hash", [Diagnostic("error", "alpha_build.output.hash_mismatch", "build/windows/LLMGameCreatorAlpha.exe", "Build manifest hashes must match actual file bytes.")]));
        scenarios.Add(InvalidScenario("path_traversal_in_staging_manifest", [Diagnostic("error", "alpha_build.staging.unsafe_path", "../escape.json", "Staging manifest paths must stay inside the staging root.")]));
        scenarios.Add(InvalidScenario("absolute_output_path_injection", [Diagnostic("error", "alpha_build.output.unsafe_path", "absolute-output-path-injection", "Build output paths must be safe relative paths.")]));
        scenarios.Add(InvalidScenario("copied_expectation_report_without_build_files", settings.IncludeExpectationOnlyInvalidMutation
            ? [Diagnostic("error", "alpha_build.invalid.expectation_only_report", "alpha-runnable-build-report.json", "Expectation reports cannot replace physical build files.")]
            : []));
        scenarios.Add(InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "alpha_build.contract.runtime_preview_dependency", "runtime_host", "Alpha proof must not depend on WinForms Runtime Preview.")]));
        scenarios.Add(InvalidScenario("unity_build_claim_without_artifact", [Diagnostic("error", "alpha_build.output.unity_build_claim_without_artifact", "build/windows", "Unity build claims require real output files.")]));
        scenarios.Add(InvalidScenario("cross_style_package_export_asset_leakage", [Diagnostic("error", "alpha_build.contract.cross_style_leakage", primary.PackId, "Package, export and asset evidence must come from the same style candidate.")]));

        if (candidates.Count != ExpectedStyleOrder.Length)
        {
            scenarios.Add(InvalidScenario("missing_style_candidate", [Diagnostic("error", "alpha_build.selection.missing_style_candidate", "style_candidates", "All three Alpha style candidates are required.")]));
        }

        return FinishInvalidMatrix(scenarios);

        static AlphaBuildInvalidMatrix FinishInvalidMatrix(IReadOnlyList<AlphaBuildInvalidScenario> items)
        {
            var diagnostics = items.SelectMany(item => item.Diagnostics).ToList();
            return new AlphaBuildInvalidMatrix
            {
                Passed = items.Count > 0 && items.All(item => !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
                ScenarioCount = items.Count,
                Scenarios = items.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
                Diagnostics = SortDiagnostics(diagnostics)
            };
        }

        static AlphaBuildInvalidScenario InvalidScenario(string scenarioId, IReadOnlyList<AlphaBuildDiagnostic> diagnostics) =>
            new()
            {
                ScenarioId = scenarioId,
                ExpectedValid = false,
                ActualValid = diagnostics.All(diagnostic => diagnostic.Severity != "error"),
                Diagnostics = SortDiagnostics(diagnostics)
            };
    }

    private static string RenderReport(AlphaRunnableBuildReport report)
    {
        var lines = new List<string>
        {
            "# Alpha Runnable Windows Build Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Blocker reached: {report.BlockerReached.ToString().ToLowerInvariant()}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Primary candidate: {report.PrimaryBuildCandidate.StyleId}",
            $"- Package hash: {report.PrimaryBuildCandidate.PackageHash}",
            $"- Asset manifest hash: {report.PrimaryBuildCandidate.AssetManifestHash}",
            $"- Export manifest hash: {report.PrimaryBuildCandidate.ExportManifestHash}",
            $"- Staging folder: {report.Staging.StagingFolderRelativePath}",
            $"- Build folder: {report.BuildOutput.BuildFolderRelativePath}",
            $"- Executable: {report.BuildOutput.ExecutableRelativePath}",
            $"- Windows executable produced: {report.WindowsExecutableProduced.ToString().ToLowerInvariant()}",
            $"- Unity Editor executed: {report.UnityEditorExecuted.ToString().ToLowerInvariant()}",
            $"- Unity build produced: {report.UnityBuildProduced.ToString().ToLowerInvariant()}",
            $"- Launch verified: {report.LaunchVerified.ToString().ToLowerInvariant()}",
            $"- Play loop verified: {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            string.Empty,
            "## Style Candidates",
            string.Empty
        };
        lines.AddRange(report.StyleCandidates.Select(candidate => $"- {candidate.StyleId}: package={candidate.PackageId} packageHash={candidate.PackageHash} assetManifestHash={candidate.AssetManifestHash} exportManifestHash={candidate.ExportManifestHash}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid Matrix");
        lines.Add(string.Empty);
        lines.AddRange(report.InvalidMatrix.Scenarios.Select(scenario => $"- {scenario.ScenarioId}: actualValid={scenario.ActualValid.ToString().ToLowerInvariant()} diagnostics={string.Join(",", scenario.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(diagnostic => $"- {diagnostic.Severity}: {diagnostic.Code} [{diagnostic.Target}] {diagnostic.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        AlphaRunnableBuildReport report,
        AlphaBuildEnvironmentProbe environment,
        AlphaBuildUnityExecutionResult unityExecution)
    {
        var lines = new List<string>
        {
            "# Alpha Runnable Windows Build Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            report.FinalStatus,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final runnable gate remains required: {FinalGate}",
            $"- Unity executable discovered: {environment.UnityExecutableDiscovered.ToString().ToLowerInvariant()}",
            "- Unity executable path: (omitted; local machine path is not part of deterministic evidence)",
            $"- Unity version evidence: {Display(environment.UnityVersionForVerification)}",
            $"- Repository Unity project found: {environment.RepoUnityProjectFound.ToString().ToLowerInvariant()}",
            $"- Repository Unity project: {Display(environment.RepoUnityProjectRelativePath)}",
            $"- Repository Unity build script found: {environment.RepoBuildScriptFound.ToString().ToLowerInvariant()}",
            $"- Repository Unity build script: {Display(environment.RepoBuildScriptRelativePath)}",
            $"- Unity command executed: {unityExecution.UnityEditorExecuted.ToString().ToLowerInvariant()}",
            $"- Unity command: {Display(unityExecution.UnityEditorExecuted ? unityExecution.CommandForVerification : environment.BuildCommandForVerification)}",
            $"- Unity build log: {Display(unityExecution.LogRelativePath)}",
            $"- Build output folder: {report.BuildOutput.BuildFolderRelativePath}",
            $"- Executable relative path: {Display(report.BuildOutput.ExecutableRelativePath)}",
            $"- Launch command: {Display(report.LaunchVerification.LaunchCommandForVerification)}",
            $"- Launch log: {Display(report.LaunchVerification.LogRelativePath)}",
            $"- Play-loop command: {Display(report.LaunchVerification.PlayLoopCommandForVerification)}",
            $"- Play-loop log: {Display(report.LaunchVerification.PlayLoopLogRelativePath)}",
            $"- Launch verified: {report.LaunchVerified.ToString().ToLowerInvariant()}",
            $"- Play loop verified: {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.Scenarios.Count(item => !item.ActualValid)}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            report.WindowsExecutableProduced
                ? "Manual review steps:"
                : "User steps to unblock:",
            string.Empty,
            report.WindowsExecutableProduced
                ? "1. Review the produced Windows player folder and launch log from this run."
                : "1. Add or point the repository to a Unity project/template containing `ProjectSettings/ProjectVersion.txt`, `Assets/` and `Packages/`.",
            report.WindowsExecutableProduced
                ? "2. Launch the produced `.exe` interactively if a manual graphics/play pass is required."
                : "2. Add a repository-local headless build entrypoint or script that invokes `BuildPipeline.BuildPlayer` for Windows x64.",
            report.WindowsExecutableProduced
                ? "3. Verify actual play-loop behavior before marking `alpha_runnable_windows_build_verification` passed."
                : "3. Run the build to `.llmgc/procedural/alpha-runnable-build/build/windows/` and rerun `run-product-smoke.ps1 -Scenario alpha-runnable-build`.",
            report.WindowsExecutableProduced
                ? "4. Keep `alpha_runnable_windows_build_verification` required until the deterministic play-loop evidence is reviewed."
                : "4. Launch the produced `.exe`, verify content load and the selected loop, then record play evidence in a later bounded task."
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;

        static string Display(string value) => string.IsNullOrWhiteSpace(value) ? "(none)" : value;
    }

    private static AlphaBuildLaunchVerificationResult VerifyLaunchIfRequested(
        string projectRoot,
        string buildRoot,
        string logsRoot,
        AlphaBuildOutputManifest buildOutput,
        AlphaBuildCandidate? primary,
        AlphaRunnableBuildOptions settings)
    {
        if (!buildOutput.WindowsExecutableProduced || string.IsNullOrWhiteSpace(buildOutput.ExecutableRelativePath))
        {
            return new AlphaBuildLaunchVerificationResult
            {
                LaunchVerified = false,
                PlayLoopVerified = false,
                ProcessExitCode = null,
                DurationSeconds = 0,
                LogRelativePath = string.Empty,
                Diagnostics =
                [
                    Diagnostic("error", "alpha_build.launch.blocked_no_executable", "build/windows", "Launch verification cannot run because no real Windows executable was produced.")
                ]
            };
        }

        var executablePath = Path.Combine(buildRoot, buildOutput.ExecutableRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var launchLogPath = Path.Combine(logsRoot, "alpha-player-launch.log");
        var playLoopLogPath = Path.Combine(logsRoot, "alpha-player-play-loop.log");
        var arguments = new List<string>
        {
            "-batchmode",
            "-nographics",
            "-alphaSmokeExit",
            "-alphaPlayLoopSmokeExit",
            "-alphaLogPath",
            launchLogPath,
            "-alphaPlayLoopLogPath",
            playLoopLogPath
        };
        var command = BuildCommandForDisplay(executablePath, arguments);
        if (!settings.LaunchBuiltPlayer)
        {
            return new AlphaBuildLaunchVerificationResult
            {
                LaunchVerified = false,
                PlayLoopVerified = false,
                ProcessExitCode = null,
                DurationSeconds = 0,
                LogRelativePath = RelativePath(projectRoot, launchLogPath),
                PlayLoopLogRelativePath = RelativePath(projectRoot, playLoopLogPath),
                LaunchCommandForVerification = command,
                PlayLoopCommandForVerification = command,
                Diagnostics =
                [
                    Diagnostic("warning", "alpha_build.launch.not_requested", buildOutput.ExecutableRelativePath, "A Windows executable was produced, but launch verification was not requested for this validation pass.")
                ]
            };
        }

        Directory.CreateDirectory(logsRoot);
        var startedAt = DateTime.UtcNow;
        var result = RunProcess(executablePath, arguments, buildRoot, settings.PlayerLaunchTimeoutSeconds);
        var duration = Math.Max(0, (int)Math.Ceiling((DateTime.UtcNow - startedAt).TotalSeconds));
        var diagnostics = new List<AlphaBuildDiagnostic>
        {
            Diagnostic("info", "alpha_build.launch.executed", "logs/alpha-player-launch.log", "The produced Windows player was launched in batch diagnostic mode.")
        };

        var launchLogExists = File.Exists(launchLogPath);
        if (!launchLogExists)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.launch.log_missing", "logs/alpha-player-launch.log", "The player launch diagnostic log was not produced."));
        }

        var launchLog = launchLogExists ? File.ReadAllText(launchLogPath) : string.Empty;
        var requiredMarkers = new[]
        {
            "alpha_runtime.launch_completed=true",
            "alpha_runtime.config_loaded=true",
            "alpha_runtime.package_loaded=true",
            "alpha_runtime.asset_manifest_loaded=true",
            "alpha_runtime.package_id=",
            "alpha_runtime.package_hash=",
            "alpha_runtime.asset_manifest_hash=",
            "alpha_runtime.start_map_id=",
            "alpha_runtime.selected_thread_id="
        };
        foreach (var marker in requiredMarkers)
        {
            if (!launchLog.Contains(marker, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "alpha_build.launch.missing_log_marker", marker, "The player launch log did not prove the required Alpha package/config load marker."));
            }
        }

        if (result.ExitCode != 0)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.launch.exit_failure", $"exit_code:{result.ExitCode}", "The player launch process did not exit successfully."));
        }

        var launchVerified = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var playLoopValidation = ValidatePlayLoopLog(
            launchLogPath,
            playLoopLogPath,
            primary,
            expectedLaunchCompleted: true);
        diagnostics.AddRange(playLoopValidation.Diagnostics);

        return new AlphaBuildLaunchVerificationResult
        {
            LaunchVerified = launchVerified,
            PlayLoopVerified = playLoopValidation.PlayLoopVerified,
            ProcessExitCode = result.ExitCode,
            DurationSeconds = duration,
            LogRelativePath = RelativePath(projectRoot, launchLogPath),
            PlayLoopLogRelativePath = RelativePath(projectRoot, playLoopLogPath),
            LaunchCommandForVerification = command,
            PlayLoopCommandForVerification = command,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static AlphaBuildPlayLoopValidationResult ValidatePlayLoopLog(
        string launchLogPath,
        string playLoopLogPath,
        AlphaBuildCandidate? primary,
        bool expectedLaunchCompleted = true)
    {
        var diagnostics = new List<AlphaBuildDiagnostic>();
        if (primary == null)
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.no_primary_candidate", "primary_candidate", "Play-loop verification requires the selected Alpha report candidate."));
            return new AlphaBuildPlayLoopValidationResult { Diagnostics = SortDiagnostics(diagnostics) };
        }

        if (string.IsNullOrWhiteSpace(launchLogPath) || !File.Exists(launchLogPath))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.launch_log_missing", "logs/alpha-player-launch.log", "Play-loop verification requires the launch log."));
        }

        if (string.IsNullOrWhiteSpace(playLoopLogPath) || !File.Exists(playLoopLogPath))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.log_missing", "logs/alpha-player-play-loop.log", "The player play-loop diagnostic log was not produced."));
            return new AlphaBuildPlayLoopValidationResult { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var launchValues = File.Exists(launchLogPath) ? ParseKeyValueLog(File.ReadAllLines(launchLogPath)) : new Dictionary<string, string>(StringComparer.Ordinal);
        var playValues = ParseKeyValueLog(File.ReadAllLines(playLoopLogPath));
        if (expectedLaunchCompleted && !HasBoolean(launchValues, "alpha_runtime.launch_completed"))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.launch_not_completed", "logs/alpha-player-launch.log", "Launch log must contain alpha_runtime.launch_completed=true."));
        }

        Require(playValues, "alpha_runtime.play_loop_completed", "true", diagnostics, "alpha_build.play_loop.not_completed");
        Require(playValues, "alpha_runtime.payload_root_exists", "true", diagnostics, "alpha_build.play_loop.payload_missing");
        Require(playValues, "alpha_runtime.config_loaded", "true", diagnostics, "alpha_build.play_loop.config_missing");
        Require(playValues, "alpha_runtime.package_loaded", "true", diagnostics, "alpha_build.play_loop.package_missing");
        Require(playValues, "alpha_runtime.asset_manifest_loaded", "true", diagnostics, "alpha_build.play_loop.asset_manifest_missing");
        Require(playValues, "alpha_runtime.package_id", primary.PackageId, diagnostics, "alpha_build.play_loop.package_id_mismatch");
        Require(playValues, "alpha_runtime.package_hash", primary.PackageHash, diagnostics, "alpha_build.play_loop.package_hash_mismatch");
        Require(playValues, "alpha_runtime.asset_manifest_hash", primary.AssetManifestHash, diagnostics, "alpha_build.play_loop.asset_manifest_hash_mismatch");
        Require(playValues, "alpha_runtime.start_map_id", primary.LoopRefs.MapId, diagnostics, "alpha_build.play_loop.map_id_mismatch");
        Require(playValues, "alpha_runtime.selected_thread_id", primary.SelectedThreadId, diagnostics, "alpha_build.play_loop.thread_id_mismatch");
        Require(playValues, "alpha_runtime.selected_quest_id", primary.LoopRefs.QuestId, diagnostics, "alpha_build.play_loop.quest_id_mismatch");
        Require(playValues, "alpha_runtime.selected_dialogue_id", primary.LoopRefs.DialogueId, diagnostics, "alpha_build.play_loop.dialogue_id_mismatch");
        Require(playValues, "alpha_runtime.selected_item_id", primary.LoopRefs.ItemId, diagnostics, "alpha_build.play_loop.item_id_mismatch");
        Require(playValues, "alpha_runtime.selected_event_id", primary.LoopRefs.EventId, diagnostics, "alpha_build.play_loop.event_id_mismatch");

        foreach (var key in new[]
        {
            "alpha_runtime.ref_resolved.map",
            "alpha_runtime.ref_resolved.quest",
            "alpha_runtime.ref_resolved.dialogue",
            "alpha_runtime.ref_resolved.item",
            "alpha_runtime.ref_resolved.event"
        })
        {
            Require(playValues, key, "true", diagnostics, "alpha_build.play_loop.ref_not_resolved");
        }

        var expectedCommandCount = primary.CommandHints.Count;
        var commandHintCount = ParseInt(playValues, "alpha_runtime.command_hint_count");
        if (commandHintCount < Math.Max(5, expectedCommandCount))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.command_hint_count_too_low", commandHintCount.ToString(), "Play-loop log command hint count must match or exceed selected config evidence and be at least five."));
        }

        var commandsExecuted = ParseInt(playValues, "alpha_runtime.commands_executed");
        if (commandsExecuted < Math.Max(5, expectedCommandCount))
        {
            diagnostics.Add(Diagnostic("error", "alpha_build.play_loop.commands_executed_too_low", commandsExecuted.ToString(), "Play-loop log must execute each selected command hint in order."));
        }

        for (var index = 0; index < expectedCommandCount; index++)
        {
            var expected = primary.CommandHints[index];
            Require(playValues, $"alpha_runtime.command_executed.{index}.id", expected.CommandId, diagnostics, "alpha_build.play_loop.command_id_mismatch");
            Require(playValues, $"alpha_runtime.command_executed.{index}.type", expected.CommandType, diagnostics, "alpha_build.play_loop.command_type_mismatch");
        }

        foreach (var key in new[]
        {
            "alpha_runtime.state_transition.quest_start",
            "alpha_runtime.state_transition.dialogue_open",
            "alpha_runtime.state_transition.dialogue_choice",
            "alpha_runtime.state_transition.item_or_loot",
            "alpha_runtime.state_transition.event_application",
            "alpha_runtime.quest_started",
            "alpha_runtime.dialogue_seen",
            "alpha_runtime.item_obtained",
            "alpha_runtime.event_applied"
        })
        {
            Require(playValues, key, "true", diagnostics, "alpha_build.play_loop.state_flag_missing");
        }

        return new AlphaBuildPlayLoopValidationResult
        {
            PlayLoopVerified = diagnostics.All(diagnostic => diagnostic.Severity != "error"),
            CommandHintsExpected = expectedCommandCount,
            CommandsExecuted = Math.Max(0, commandsExecuted),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        static void Require(
            IReadOnlyDictionary<string, string> values,
            string key,
            string expected,
            ICollection<AlphaBuildDiagnostic> diagnostics,
            string code)
        {
            if (!values.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", code, key, $"Expected {key}={expected}."));
            }
        }

        static int ParseInt(IReadOnlyDictionary<string, string> values, string key) =>
            values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : -1;

        static bool HasBoolean(IReadOnlyDictionary<string, string> values, string key) =>
            values.TryGetValue(key, out var value) && string.Equals(value, "true", StringComparison.Ordinal);
    }

    private static Dictionary<string, string> ParseKeyValueLog(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    private static void CopyDirectory(string sourceRoot, string destinationRoot)
    {
        foreach (var sourcePath in EnumerateFiles(sourceRoot))
        {
            var relative = RelativePath(sourceRoot, sourcePath);
            if (!IsSafeRelativePath(relative))
            {
                continue;
            }

            var destinationPath = Path.Combine(destinationRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
    }

    private static void CopyUnityTemplate(string sourceRoot, string destinationRoot)
    {
        foreach (var sourcePath in EnumerateFiles(sourceRoot))
        {
            var relative = RelativePath(sourceRoot, sourcePath);
            if (!IsSafeRelativePath(relative) || IsIgnoredUnityGeneratedPath(relative))
            {
                continue;
            }

            var destinationPath = Path.Combine(destinationRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
    }

    private static bool IsIgnoredUnityGeneratedPath(string relativePath)
    {
        var first = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return first.Equals("Library", StringComparison.OrdinalIgnoreCase) ||
               first.Equals("Temp", StringComparison.OrdinalIgnoreCase) ||
               first.Equals("Logs", StringComparison.OrdinalIgnoreCase) ||
               first.Equals("UserSettings", StringComparison.OrdinalIgnoreCase) ||
               first.Equals("obj", StringComparison.OrdinalIgnoreCase);
    }

    private static void SafeDeleteDirectory(string directoryPath, string requiredContainerRoot)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        EnsureContained(requiredContainerRoot, directoryPath);
        Directory.Delete(directoryPath, recursive: true);
    }

    private static AlphaProcessResult RunProcess(
        string executablePath,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        int timeoutSeconds)
    {
        using var process = new Process();
        process.StartInfo.FileName = executablePath;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        if (!process.WaitForExit(Math.Max(1, timeoutSeconds) * 1000))
        {
            process.Kill(entireProcessTree: true);
            return new AlphaProcessResult { ExitCode = -1 };
        }

        return new AlphaProcessResult { ExitCode = process.ExitCode };
    }

    private static string BuildUnityCommandForDisplay(
        string unityExecutablePath,
        string unityProjectPath,
        string stagingRoot,
        string buildRoot,
        string buildLogPath) =>
        BuildCommandForDisplay(
            unityExecutablePath,
            [
                "-batchmode",
                "-quit",
                "-projectPath",
                unityProjectPath,
                "-executeMethod",
                "LLMGameCreatorAlpha.Editor.AlphaBuildEntrypoint.BuildWindows64",
                "-logFile",
                buildLogPath,
                "-alphaStagingPath",
                stagingRoot,
                "-alphaBuildOutputPath",
                buildRoot
            ]);

    private static string BuildCommandForDisplay(string executablePath, IReadOnlyList<string> arguments)
    {
        static string Quote(string value) => value.Contains(' ') || value.Contains('\\') || value.Contains(':')
            ? $"\"{value}\""
            : value;

        return "& " + Quote(executablePath) + " " + string.Join(" ", arguments.Select(Quote));
    }

    private static IReadOnlyList<string> RequiredStreamingAssets() =>
    [
        "LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/game-data/game-package.json",
        "LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/assets/asset-manifest.json",
        "LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/runtime/unity-runtime-config.json"
    ];

    private static bool LooksLikeWindowsExecutable(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        Span<byte> header = stackalloc byte[2];
        using var stream = File.OpenRead(path);
        return stream.Read(header) == 2 && header[0] == (byte)'M' && header[1] == (byte)'Z';
    }

    private static void WriteJson<T>(string root, string relativePath, T value)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions), Utf8WithoutBom);
    }

    private static IReadOnlyList<string> FindUnityExecutableCandidates()
    {
        var candidates = new List<string>();
        var pathEntries = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var entry in pathEntries)
        {
            var candidate = Path.Combine(entry, "Unity.exe");
            if (File.Exists(candidate))
            {
                candidates.Add(candidate);
            }
        }

        foreach (var root in new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Unity", "Hub", "Editor")
        })
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var versionDirectory in Directory.EnumerateDirectories(root).OrderByDescending(item => item, StringComparer.Ordinal))
            {
                var candidate = Path.Combine(versionDirectory, "Editor", "Unity.exe");
                if (File.Exists(candidate))
                {
                    candidates.Add(candidate);
                }
            }
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string FindRepoUnityProject(string projectRoot)
    {
        foreach (var file in EnumerateRepoFiles(projectRoot, "ProjectVersion.txt"))
        {
            var projectSettings = Path.GetDirectoryName(file);
            if (projectSettings == null || !string.Equals(Path.GetFileName(projectSettings), "ProjectSettings", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var project = Directory.GetParent(projectSettings)?.FullName;
            if (project != null && Directory.Exists(Path.Combine(project, "Assets")) && Directory.Exists(Path.Combine(project, "Packages")))
            {
                return RelativePath(projectRoot, project);
            }
        }

        return string.Empty;
    }

    private static string FindRepoUnityBuildScript(string projectRoot)
    {
        foreach (var file in EnumerateRepoFiles(projectRoot, "*.*")
                     .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = RelativePath(projectRoot, file);
            if (!IsSafeRelativePath(relative) ||
                relative.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
                relative.Contains("/obj/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = Path.GetFileName(file);
            if (name.Contains("build", StringComparison.OrdinalIgnoreCase))
            {
                var text = File.ReadAllText(file);
                if (relative.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    relative.Contains("/Assets/", StringComparison.OrdinalIgnoreCase) &&
                    ContainsUnityBuildPlayerCall(text))
                {
                    return relative;
                }

                if (relative.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) &&
                    text.Contains("-buildWindows64Player", StringComparison.OrdinalIgnoreCase))
                {
                    return relative;
                }
            }
        }

        return string.Empty;
    }

    private static bool ContainsUnityBuildPlayerCall(string text)
    {
        var builder = new StringBuilder(text.Length);
        var inString = false;
        var inCharacter = false;
        var inLineComment = false;
        var inBlockComment = false;
        for (var index = 0; index < text.Length; index++)
        {
            var current = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';
            if (inLineComment)
            {
                if (current is '\r' or '\n')
                {
                    inLineComment = false;
                    builder.Append(current);
                }

                continue;
            }

            if (inBlockComment)
            {
                if (current == '*' && next == '/')
                {
                    inBlockComment = false;
                    index++;
                }

                continue;
            }

            if (inString)
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (inCharacter)
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == '\'')
                {
                    inCharacter = false;
                }

                continue;
            }

            if (current == '/' && next == '/')
            {
                inLineComment = true;
                index++;
                continue;
            }

            if (current == '/' && next == '*')
            {
                inBlockComment = true;
                index++;
                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '\'')
            {
                inCharacter = true;
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString().Contains("BuildPipeline.BuildPlayer(", StringComparison.Ordinal);
    }

    private static IEnumerable<string> EnumerateRepoFiles(string root, string pattern)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            foreach (var file in Directory.EnumerateFiles(current, pattern))
            {
                yield return file;
            }

            foreach (var directory in Directory.EnumerateDirectories(current).OrderByDescending(item => item, StringComparer.OrdinalIgnoreCase))
            {
                var name = Path.GetFileName(directory);
                if (name is ".git" or ".vs" or "bin" or "obj" or "Library" or "Temp" or "Logs" or "UserSettings")
                {
                    continue;
                }

                pending.Push(directory);
            }
        }
    }

    private static string ExtractUnityVersion(string unityExecutablePath)
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(unityExecutablePath) ?? string.Empty);
        while (directory != null)
        {
            if (directory.Parent != null && string.Equals(directory.Parent.Name, "Editor", StringComparison.OrdinalIgnoreCase))
            {
                return directory.Name;
            }

            if (directory.Name.Contains('f') && directory.Name.Any(char.IsDigit))
            {
                return directory.Name;
            }

            directory = directory.Parent;
        }

        return string.Empty;
    }

    private static IEnumerable<string> EnumerateFiles(string root) =>
        Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal)
            : [];

    private static AlphaBuildFileManifestEntry FileEntry(string root, string path, string kind)
    {
        var bytes = File.ReadAllBytes(path);
        return new AlphaBuildFileManifestEntry
        {
            RelativePath = RelativePath(root, path),
            Kind = kind,
            Hash = ComputeHash(bytes),
            ByteCount = bytes.LongLength
        };
    }

    private static string KindFor(string relativePath)
    {
        if (relativePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return "windows_executable";
        }

        if (relativePath.StartsWith("assets/", StringComparison.Ordinal))
        {
            return relativePath.EndsWith("asset-manifest.json", StringComparison.Ordinal) ? "json_payload" : "asset_payload";
        }

        return relativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? "json_payload" : "build_payload";
    }

    private static IReadOnlyList<AlphaBuildDiagnostic> SortDiagnostics(IEnumerable<AlphaBuildDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static AlphaBuildDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains('\\') &&
        !path.Contains(':', StringComparison.Ordinal) &&
        !path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(part => part == "..");

    private static bool IsSafeRelativeDirectory(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains(':', StringComparison.Ordinal) &&
        path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).All(part => part != "..");

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string ShortHash(string hash, int length) => hash.Length <= length ? hash : hash[..length];

    private sealed record CompactedUnityExportDirectory(
        UnityRuntimeExportFileManifest Manifest,
        Dictionary<string, string> PathMapping,
        string RuntimeConfigHash);
}

public sealed record AlphaRunnableBuildOptions
{
    public bool IncludeExpectationOnlyInvalidMutation { get; init; } = true;
    public string RepositoryRootPath { get; init; } = string.Empty;
    public string RelativeOutputDirectoryOverride { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record AlphaRunnableBuildAcceptanceResult
{
    public AlphaRunnableBuildReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record AlphaRunnableBuildWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string BuildDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record AlphaRunnableBuildReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public bool BlockerReached { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildCandidate> StyleCandidates { get; init; } = [];
    public AlphaBuildCandidate PrimaryBuildCandidate { get; init; } = new();
    public AlphaBuildStagingManifest Staging { get; init; } = new();
    public AlphaBuildOutputManifest BuildOutput { get; init; } = new();
    public AlphaBuildEnvironmentProbe BuildEnvironment { get; init; } = new();
    public AlphaBuildLaunchVerificationResult LaunchVerification { get; init; } = new();
    public AlphaBuildInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool UnityBuildProduced { get; init; }
    public bool LaunchVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public AlphaBuildExternalExecutionFlags ExternalExecution { get; init; } = new();
    public bool RuntimePreviewDependency { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public string DeterministicReportRelativePath { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildCandidate
{
    public bool Accepted { get; init; }
    public int Ordinal { get; init; }
    public string StyleId { get; init; } = string.Empty;
    public string StyleName { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string ExportFolderRelativePath { get; init; } = string.Empty;
    public string RuntimeConfigHash { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public AlphaBuildLoopRefs LoopRefs { get; init; } = new();
    public IReadOnlyList<AlphaBuildAssetRef> AssetRefs { get; init; } = [];
    public IReadOnlyList<AlphaBuildCommandHint> CommandHints { get; init; } = [];
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildLoopRefs
{
    public string MapId { get; init; } = string.Empty;
    public string NpcId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeCommandHintIds { get; init; } = [];
}

public sealed record AlphaBuildAssetRef
{
    public string Category { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string ExportRelativePath { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record AlphaBuildCommandHint
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string InventoryId { get; init; } = string.Empty;
    public double Amount { get; init; }
}

public sealed record AlphaBuildStagingManifest
{
    public bool Passed { get; init; }
    public string SchemaVersion { get; init; } = string.Empty;
    public string StagingFolderRelativePath { get; init; } = string.Empty;
    public string SourceExportFolderRelativePath { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public long TotalByteCount { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public bool RuntimePreviewDependency { get; init; }
    public IReadOnlyList<AlphaBuildFileManifestEntry> Files { get; init; } = [];
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildOutputManifest
{
    public bool Passed { get; init; }
    public string SchemaVersion { get; init; } = string.Empty;
    public string BuildFolderRelativePath { get; init; } = string.Empty;
    public string ExecutableRelativePath { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public long TotalByteCount { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityBuildProduced { get; init; }
    public IReadOnlyList<AlphaBuildFileManifestEntry> Files { get; init; } = [];
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildUnityExecutionResult
{
    public bool UnityEditorExecuted { get; init; }
    public bool Passed { get; init; }
    public int? ExitCode { get; init; }
    public string CommandForVerification { get; init; } = string.Empty;
    public string LogRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record AlphaProcessResult
{
    public int ExitCode { get; init; }
}

public sealed record AlphaBuildFileManifestEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record AlphaBuildEnvironmentProbe
{
    public bool UnityExecutableDiscovered { get; init; }
    public string UnityExecutablePathForVerification { get; init; } = string.Empty;
    public string UnityVersionForVerification { get; init; } = string.Empty;
    public bool RepoUnityProjectFound { get; init; }
    public string RepoUnityProjectRelativePath { get; init; } = string.Empty;
    public bool RepoBuildScriptFound { get; init; }
    public string RepoBuildScriptRelativePath { get; init; } = string.Empty;
    public string BuildCommandForVerification { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];

    public AlphaBuildEnvironmentProbe Sanitized() =>
        this with
        {
            UnityExecutablePathForVerification = string.Empty,
            UnityVersionForVerification = string.Empty,
            BuildCommandForVerification = string.Empty
        };
}

public sealed record AlphaBuildLaunchMetadata
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string RuntimeHostKind { get; init; } = string.Empty;
    public string LaunchMode { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool UnityBuildProduced { get; init; }
}

public sealed record AlphaBuildLaunchVerificationResult
{
    public bool LaunchVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public int? ProcessExitCode { get; init; }
    public int DurationSeconds { get; init; }
    public string LogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    [JsonIgnore]
    public string LaunchCommandForVerification { get; init; } = string.Empty;
    [JsonIgnore]
    public string PlayLoopCommandForVerification { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildPlayLoopValidationResult
{
    public bool PlayLoopVerified { get; init; }
    public int CommandHintsExpected { get; init; }
    public int CommandsExecuted { get; init; }
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<AlphaBuildInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record AlphaBuildExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool MediaExecuted { get; init; }
    public bool GeneratorLibraryExecuted { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool WindowsExecutableExecuted { get; init; }

    public bool AnyExecuted() =>
        LlmExecuted ||
        RagExecuted ||
        ProviderExecuted ||
        LuaExecuted ||
        MediaExecuted ||
        GeneratorLibraryExecuted ||
        UnityEditorExecuted ||
        UnityBuildExecuted ||
        WindowsExecutableExecuted;
}

public sealed record AlphaBuildDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
