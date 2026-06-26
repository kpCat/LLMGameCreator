using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnityRuntimeExport;

public sealed class UnityRuntimeExportAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-runtime-export";
    public const string ExportDirectoryName = "export";
    public const string ReportJsonFileName = "unity-runtime-export-report.json";
    public const string ReportMarkdownFileName = "unity-runtime-export-report.md";
    public const string VerificationMarkdownFileName = "unity-runtime-export-verification.md";
    public const string ManualGate = "unity_runtime_export_vertical_slice_artifact_verification";

    private const string RuntimeContractSchemaVersion = "unity_runtime_export_contract_v1";
    private const int MaxExportFileCount = 32;
    private const long MaxExportByteCount = 2_000_000;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly string[] RequiredAssetCategories =
    [
        "tile_region_graphic",
        "npc_portrait",
        "item_icon_ui_graphic",
        "sound_effect",
        "music_ambience"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IGamePackageValidator _packageValidator;

    static UnityRuntimeExportAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityRuntimeExportAcceptanceService(IGamePackageValidator? packageValidator = null)
    {
        _packageValidator = packageValidator ?? new GamePackageValidator();
    }

    public UnityRuntimeExportAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityRuntimeExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityRuntimeExportOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var relativeOutputDirectory = string.IsNullOrWhiteSpace(settings.RelativeOutputDirectoryOverride)
            ? RelativeOutputDirectory
            : settings.RelativeOutputDirectoryOverride;
        if (!IsSafeRelativePath(relativeOutputDirectory))
        {
            throw new ArgumentException("Relative output directory override must be a safe relative path.", nameof(options));
        }

        var artifactRoot = Path.GetFullPath(Path.Combine(projectRoot, relativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        var exportRoot = Path.GetFullPath(Path.Combine(artifactRoot, ExportDirectoryName));
        EnsureContained(projectRoot, artifactRoot);
        EnsureContained(artifactRoot, exportRoot);
        ResetDirectory(exportRoot);

        var diagnostics = new List<UnityRuntimeExportDiagnostic>
        {
            Diagnostic("info", "unity_runtime_export.goal011_gate_recorded", "minimum_asset_pipeline_artifact_verification", "User-confirmed Goal 011 artifact verification is recorded as passed."),
            Diagnostic("info", "unity_runtime_export.no_external_execution", "harness", "No Unity Editor, Unity build, Windows executable, LLM, RAG, provider, Lua or media execution was invoked.")
        };

        var selection = SelectInput(contentGenerationResult.Report, minimumAssetResult.Report, settings.SelectionOrdinal);
        diagnostics.AddRange(selection.Diagnostics);

        UnityRuntimeExportMaterialization? primaryExport = null;
        UnityRuntimeContractValidationResult primaryValidation = new();
        if (selection.Accepted)
        {
            primaryExport = MaterializeExport(projectRoot, exportRoot, selection);
            primaryValidation = ValidateExport(projectRoot, exportRoot, selection, primaryExport.RuntimeConfig, primaryExport.LaunchMetadata, primaryExport.Manifest);
            diagnostics.AddRange(primaryValidation.Diagnostics);
        }

        var replay = selection.Accepted
            ? BuildReplayEvidence(projectRoot, artifactRoot, selection)
            : new UnityRuntimeExportReplayEvidence();
        var variation = BuildVariationEvidence(projectRoot, artifactRoot, contentGenerationResult.Report, minimumAssetResult.Report);
        var invalidMatrix = BuildInvalidMatrix(projectRoot, exportRoot, selection, primaryExport, settings);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var validMatrixPassed =
            selection.Accepted &&
            primaryExport != null &&
            primaryValidation.Passed &&
            primaryExport.Manifest.Files.Count > 0 &&
            primaryExport.Manifest.Files.Count <= MaxExportFileCount &&
            primaryExport.Manifest.TotalByteCount > 0 &&
            primaryExport.Manifest.TotalByteCount <= MaxExportByteCount &&
            primaryExport.Manifest.Files.All(file => File.Exists(Path.Combine(exportRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar)))) &&
            replay.Passed &&
            variation.Passed;
        var invalidMatrixPassed = invalidMatrix.Passed;
        var packageValidationPassed = primaryValidation.PackageValidatorClean;
        var assetManifestValidationPassed = primaryValidation.AssetManifestValid;
        var selectedLoopResolutionPassed = primaryValidation.SelectedLoopResolutionPassed;
        var exportManifestValidationPassed = primaryValidation.ExportManifestValid;

        diagnostics.Add(Diagnostic(validMatrixPassed ? "info" : "error", validMatrixPassed ? "unity_runtime_export.valid_matrix_passed" : "unity_runtime_export.valid_matrix_failed", "valid_matrix", "A deterministic export, replay and second valid input hash difference are required."));
        diagnostics.Add(Diagnostic(invalidMatrixPassed ? "info" : "error", invalidMatrixPassed ? "unity_runtime_export.invalid_matrix_rejected" : "unity_runtime_export.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must fail through the export validation path."));
        diagnostics.Add(Diagnostic(packageValidationPassed ? "info" : "error", packageValidationPassed ? "unity_runtime_export.package_validation_passed" : "unity_runtime_export.package_validation_failed", "package", "The selected package must remain validator-clean."));
        diagnostics.Add(Diagnostic(assetManifestValidationPassed ? "info" : "error", assetManifestValidationPassed ? "unity_runtime_export.asset_manifest_validation_passed" : "unity_runtime_export.asset_manifest_validation_failed", "asset_manifest", "Selected asset refs must resolve to exported files with matching hashes."));

        var reportWithoutHash = new UnityRuntimeExportReport
        {
            Accepted = validMatrixPassed &&
                       invalidMatrixPassed &&
                       packageValidationPassed &&
                       assetManifestValidationPassed &&
                       selectedLoopResolutionPassed &&
                       exportManifestValidationPassed,
            ManualGate = ManualGate,
            Goal011GateRecorded = true,
            CompletedSlices = ["S099", "S100", "S101", "S102", "S103", "S104", "S105"],
            ProductSmokeRoute = "unity-runtime-export",
            SelectedInput = selection,
            ExportFolderRelativePath = RelativePath(projectRoot, exportRoot),
            ExportFileCount = primaryExport?.Manifest.Files.Count ?? 0,
            ExportByteCount = primaryExport?.Manifest.TotalByteCount ?? 0,
            ExportManifestHash = primaryExport?.Manifest.ManifestHash ?? string.Empty,
            RuntimeConfigHash = primaryExport?.RuntimeConfig.ConfigHash ?? string.Empty,
            ValidMatrixPassed = validMatrixPassed,
            InvalidMatrixPassed = invalidMatrixPassed,
            PackageValidationPassed = packageValidationPassed,
            AssetManifestValidationPassed = assetManifestValidationPassed,
            ExportManifestValidationPassed = exportManifestValidationPassed,
            SelectedLoopResolutionPassed = selectedLoopResolutionPassed,
            ReplayEvidence = replay,
            VariationEvidence = variation,
            ContractValidation = primaryValidation,
            InvalidMatrix = invalidMatrix,
            ExternalExecution = new UnityRuntimeExportExternalExecutionFlags(),
            WindowsExecutableProduced = false,
            UnityEditorExecuted = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            RuntimePreviewDependency = false,
            GeneratorLibraryChanged = false,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityRuntimeExportAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<UnityRuntimeExportWriteResult> WriteAsync(
        string projectRootPath,
        UnityRuntimeExportAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "unity-runtime-export"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityRuntimeExportWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ExportDirectoryPath = Path.Combine(outputDirectory, ExportDirectoryName),
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityRuntimeExportWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private UnityRuntimeExportInputSelection SelectInput(
        ContentGenerationScaleReport contentReport,
        MinimumAssetPipelineReport assetReport,
        int selectionOrdinal)
    {
        var diagnostics = new List<UnityRuntimeExportDiagnostic>();
        if (!contentReport.Accepted)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.missing_prior_package_evidence", "content_generation", "Accepted Goal 010 content/package evidence is required."));
        }

        if (!assetReport.Accepted)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.missing_prior_asset_manifest_evidence", "minimum_asset_pipeline", "Accepted Goal 011 asset manifest evidence is required."));
        }

        var contentPacks = contentReport.Packs
            .Where(pack => pack.Accepted && pack.PackageAudit.ValidatorClean)
            .OrderBy(pack => pack.PackId, StringComparer.Ordinal)
            .ToList();
        var assetRuns = assetReport.Runs
            .Where(run => run.Accepted && run.AssetValidation.Passed)
            .OrderBy(run => run.PackId, StringComparer.Ordinal)
            .ToList();
        if (contentPacks.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.no_valid_package", "content_generation", "No accepted generated package input was available."));
        }

        if (assetRuns.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.no_valid_asset_manifest", "minimum_asset_pipeline", "No accepted asset manifest input was available."));
        }

        var matches = contentPacks
            .Join(assetRuns, pack => pack.PackId, run => run.PackId, (pack, run) => (Pack: pack, Run: run), StringComparer.Ordinal)
            .OrderBy(item => item.Pack.PackId, StringComparer.Ordinal)
            .ToList();
        if (matches.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.package_asset_mismatch", "input_selection", "No generated package id has a matching resolved asset manifest."));
            return new UnityRuntimeExportInputSelection { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var selected = matches[Math.Clamp(selectionOrdinal, 0, matches.Count - 1)];
        var thread = selected.Pack.RuntimeThreads
            .Where(item => item.ActualValid && item.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService && item.RuntimeEvidence.SaveLoadRoundtripPassed)
            .OrderBy(item => item.ThreadId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (thread == null)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.no_headless_runtime_thread", selected.Pack.PackId, "A selected package needs a real headless runtime thread from Goal 010 evidence."));
            return new UnityRuntimeExportInputSelection { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var selectedAssets = new List<UnityRuntimeExportAssetRef>();
        foreach (var category in RequiredAssetCategories)
        {
            var asset = selected.Run.ResolvedAssets
                .Where(item => item.Category == category)
                .OrderBy(item => item.ContentId, StringComparer.Ordinal)
                .ThenBy(item => item.AssetId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (asset == null)
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.missing_required_asset_category", category, "The selected asset manifest does not contain a required Unity runtime asset category."));
                continue;
            }

            selectedAssets.Add(AssetRef(asset));
        }

        var package = selected.Pack.PackageAudit.Package;
        var startMapId = string.IsNullOrWhiteSpace(package.Manifest.StartMapId)
            ? package.Game.Maps.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty
            : package.Manifest.StartMapId;
        if (string.IsNullOrWhiteSpace(startMapId))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.missing_start_map", selected.Pack.PackId, "The selected package must expose a start map for Unity runtime bootstrap."));
        }

        var catalogIds = selected.Pack.Catalog.AllGeneratedIds
            .Concat(PackageIds(package))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        foreach (var selectedId in thread.SelectedGeneratedIds)
        {
            if (!catalogIds.Contains(selectedId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.unresolved_selected_loop_id", selectedId, "Selected runtime loop id must resolve in generated/package evidence."));
            }
        }

        var assetManifestHashMatches = string.Equals(selected.Run.PackageHash, selected.Run.PackageBindingAudit.PreAssetPackageHash, StringComparison.Ordinal) &&
                                       string.Equals(selected.Run.PackageContentHash, selected.Run.PackageBindingAudit.GeneratedContentHash, StringComparison.Ordinal);
        if (!assetManifestHashMatches)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.input.asset_manifest_hash_mismatch", selected.Run.PackId, "Resolved asset manifest hashes must match the package/content bytes used by Goal 011."));
        }

        return new UnityRuntimeExportInputSelection
        {
            Accepted = diagnostics.All(item => item.Severity != "error"),
            SelectionPolicy = "first_matching_accepted_pack_by_pack_id",
            PackageId = selected.Pack.PackageAudit.PackageId,
            PackId = selected.Pack.PackId,
            SourcePackageHash = selected.Pack.PackageAudit.PackageHash,
            AssetManifestHash = selected.Run.Manifest.ManifestHash,
            AssetManifestPackageHash = selected.Run.PackageHash,
            AssetManifestContentHash = selected.Run.PackageContentHash,
            ContentCatalogHash = selected.Pack.Catalog.CatalogHash,
            StartMapId = startMapId,
            SelectedThreadId = thread.ThreadId,
            SelectedGeneratedIds = thread.SelectedGeneratedIds.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SelectedRuntimeCommands = thread.Commands.Select(command => new UnityRuntimeExportCommandHint
            {
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                TargetId = command.TargetId,
                SecondaryTargetId = command.SecondaryTargetId,
                Value = command.Value,
                InventoryId = command.InventoryId,
                Amount = command.Amount
            }).OrderBy(item => item.CommandId, StringComparer.Ordinal).ToList(),
            SelectedAssetRefs = selectedAssets.OrderBy(item => item.Category, StringComparer.Ordinal).ThenBy(item => item.AssetId, StringComparer.Ordinal).ToList(),
            RuntimeStateHash = thread.RuntimeEvidence.RuntimeStateHash,
            RestoredRuntimeStateHash = thread.RuntimeEvidence.RestoredRuntimeStateHash,
            SaveLoadRoundtripPassed = thread.RuntimeEvidence.SaveLoadRoundtripPassed,
            PackageValidatorClean = selected.Pack.PackageAudit.ValidatorClean,
            PackageContentIds = catalogIds,
            Diagnostics = SortDiagnostics(diagnostics),
            Package = package,
            AssetManifest = selected.Run.Manifest
        };
    }

    private UnityRuntimeExportMaterialization MaterializeExport(
        string projectRoot,
        string exportRoot,
        UnityRuntimeExportInputSelection selection)
    {
        ResetDirectory(exportRoot);

        var runtimeConfig = BuildRuntimeConfig(selection);
        var launchMetadata = new UnityRuntimeLaunchMetadata
        {
            SchemaVersion = "unity_runtime_launch_metadata_v1",
            RuntimeHostKind = "generic_unity_runtime",
            LaunchMode = "headless_contract_validation_only",
            WindowsExecutableProduced = false,
            UnityEditorExecuted = false,
            UnityBuildProduced = false,
            RequiresUnityEditorLaunch = false,
            ExternalExecutionFlags = new UnityRuntimeExportExternalExecutionFlags()
        };

        var files = new List<UnityRuntimeExportFileManifestEntry>();
        WriteJson(exportRoot, "game-data/game-package.json", selection.Package, files);
        WriteJson(exportRoot, "game-data/generated-content-provenance.json", BuildGeneratedContentPayload(selection), files);
        WriteJson(exportRoot, "assets/asset-manifest.json", BuildAssetPayload(selection), files);
        WriteJson(exportRoot, "runtime/unity-runtime-config.json", runtimeConfig, files);
        WriteJson(exportRoot, "runtime/launch-metadata.json", launchMetadata, files);
        foreach (var asset in selection.SelectedAssetRefs.OrderBy(item => item.ExportRelativePath, StringComparer.Ordinal))
        {
            var sourcePath = Path.GetFullPath(Path.Combine(projectRoot, asset.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            var relativePath = asset.ExportRelativePath;
            if (!IsSafeRelativePath(relativePath))
            {
                continue;
            }

            var destinationPath = OutputPath(exportRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
            files.Add(FileEntry(exportRoot, relativePath, "asset_payload", asset.AssetId));
        }

        var manifestWithoutHash = new UnityRuntimeExportFileManifest
        {
            SchemaVersion = "unity_runtime_export_manifest_v1",
            PackageId = selection.PackageId,
            PackageHash = selection.SourcePackageHash,
            AssetManifestHash = selection.AssetManifestHash,
            Files = files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList()
        };
        var manifest = manifestWithoutHash with
        {
            TotalByteCount = manifestWithoutHash.Files.Sum(item => item.ByteCount),
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };
        WriteJson(exportRoot, "export-manifest.json", manifest, files: null);

        return new UnityRuntimeExportMaterialization
        {
            RuntimeConfig = runtimeConfig with { ConfigHash = ComputeHash(JsonSerializer.Serialize(runtimeConfig, JsonOptions)) },
            LaunchMetadata = launchMetadata,
            Manifest = manifest
        };
    }

    private UnityRuntimeConfig BuildRuntimeConfig(UnityRuntimeExportInputSelection selection)
    {
        var startPosition = selection.Package.Game.Maps
            .FirstOrDefault(map => string.Equals(map.Id, selection.StartMapId, StringComparison.Ordinal))?.StartPosition;
        var configWithoutHash = new UnityRuntimeConfig
        {
            SchemaVersion = RuntimeContractSchemaVersion,
            RuntimeHostKind = "generic_unity_runtime",
            PackageId = selection.PackageId,
            PackageVersion = selection.Package.Manifest.Version,
            PackageHash = selection.SourcePackageHash,
            AssetManifestHash = selection.AssetManifestHash,
            StartMapId = selection.StartMapId,
            StartSceneId = selection.StartMapId,
            PlayerSpawn = new UnityRuntimePlayerSpawn
            {
                StateRef = "runtime_state/bootstrap/player",
                MapId = selection.StartMapId,
                X = startPosition?.X ?? 0,
                Y = startPosition?.Y ?? 0
            },
            SelectedThreadId = selection.SelectedThreadId,
            SelectedGeneratedIds = selection.SelectedGeneratedIds,
            CommandHints = selection.SelectedRuntimeCommands,
            AssetRefs = selection.SelectedAssetRefs,
            SaveLoad = new UnityRuntimeSaveLoadBootstrap
            {
                RuntimeStateOwner = "GameRuntimeState",
                RuntimeStateHash = selection.RuntimeStateHash,
                RestoredRuntimeStateHash = selection.RestoredRuntimeStateHash,
                SaveLoadRoundtripPassed = selection.SaveLoadRoundtripPassed,
                BootstrapMetadataPath = "game-data/generated-content-provenance.json"
            },
            ExternalExecution = new UnityRuntimeExportExternalExecutionFlags()
        };

        return configWithoutHash with { ConfigHash = string.Empty };
    }

    private UnityRuntimeContractValidationResult ValidateExport(
        string projectRoot,
        string exportRoot,
        UnityRuntimeExportInputSelection selection,
        UnityRuntimeConfig config,
        UnityRuntimeLaunchMetadata launch,
        UnityRuntimeExportFileManifest manifest)
    {
        var diagnostics = new List<UnityRuntimeExportDiagnostic>();
        if (!string.Equals(config.SchemaVersion, RuntimeContractSchemaVersion, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.schema_version", "runtime_config", "Unity runtime config schema version is not supported."));
        }

        if (string.Equals(config.RuntimeHostKind, "winforms_runtime_preview", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.runtime_preview_dependency", "runtime_config", "Unity runtime export must not depend on WinForms Runtime Preview."));
        }

        if (launch.RequiresUnityEditorLaunch || launch.UnityEditorExecuted || launch.UnityBuildProduced)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.unity_editor_claim_without_artifact", "launch_metadata", "Unity Editor/build execution is not accepted without a real reported artifact."));
        }

        if (launch.WindowsExecutableProduced)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.windows_executable_claim_without_artifact", "launch_metadata", "Goal 012 must not claim a Windows executable without a real verified executable."));
        }

        if (launch.ExternalExecutionFlags.AnyExecuted() || config.ExternalExecution.AnyExecuted())
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.external_execution", "external_execution", "External provider, LLM, Lua, media, Unity or executable execution flags must remain false."));
        }

        if (!string.Equals(config.PackageId, selection.PackageId, StringComparison.Ordinal) ||
            !string.Equals(config.PackageHash, selection.SourcePackageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.package_hash_mismatch", config.PackageId, "Runtime config package id/hash must match the selected package evidence."));
        }

        if (!string.Equals(config.AssetManifestHash, selection.AssetManifestHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.asset_manifest_hash_mismatch", config.AssetManifestHash, "Runtime config asset manifest hash must match the selected Goal 011 manifest."));
        }

        var packageIds = selection.PackageContentIds.ToHashSet(StringComparer.Ordinal);
        if (!packageIds.Contains(config.StartMapId))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.start_map_unresolved", config.StartMapId, "Start map must resolve in selected package data."));
        }

        foreach (var selectedId in config.SelectedGeneratedIds)
        {
            if (!packageIds.Contains(selectedId))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.selected_loop_ref_unresolved", selectedId, "Selected loop id must resolve in selected generated/package data."));
            }
        }

        foreach (var command in config.CommandHints)
        {
            if (!string.IsNullOrWhiteSpace(command.TargetId) && !packageIds.Contains(command.TargetId) && !command.CommandType.Contains("set_flag", StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.command_target_unresolved", command.TargetId, "Runtime command hint target must resolve in selected package data or be an existing runtime flag primitive."));
            }
        }

        var selectedAssetIds = selection.AssetManifest.ResolvedAssets.Select(asset => asset.AssetId).ToHashSet(StringComparer.Ordinal);
        var selectedAssetContentIds = selection.AssetManifest.ResolvedAssets.Select(asset => asset.ContentId).ToHashSet(StringComparer.Ordinal);
        foreach (var assetRef in config.AssetRefs)
        {
            if (!RequiredAssetCategories.Contains(assetRef.Category, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.asset_category_unknown", assetRef.Category, "Runtime asset ref category must be one of the Goal 012 required categories."));
            }

            if (!selectedAssetIds.Contains(assetRef.AssetId) || !selectedAssetContentIds.Contains(assetRef.ContentId))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.asset_ref_unresolved", assetRef.AssetId, "Runtime asset ref must be a strict subset of the selected Goal 011 asset manifest."));
            }

            if (!IsSafeRelativePath(assetRef.ExportRelativePath))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.unsafe_export_path", assetRef.ExportRelativePath, "Export asset path must be relative and contained under the export root."));
                continue;
            }

            if (IsExecutableOrScriptPath(assetRef.ExportRelativePath))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.executable_payload_injection", assetRef.ExportRelativePath, "Executable, script or provider payloads are not valid Unity runtime export assets."));
            }

            var path = Path.GetFullPath(Path.Combine(exportRoot, assetRef.ExportRelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsContained(exportRoot, path) || !File.Exists(path))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.exported_asset_file_missing", assetRef.ExportRelativePath, "Runtime asset ref must resolve to a real exported file."));
                continue;
            }

            var bytes = File.ReadAllBytes(path);
            if (!string.Equals(ComputeHash(bytes), assetRef.Hash, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.exported_asset_hash_mismatch", assetRef.ExportRelativePath, "Runtime asset ref hash must match the exported file bytes."));
            }
        }

        var manifestFilesValid = ValidateManifestFiles(exportRoot, manifest, diagnostics);
        var validation = _packageValidator.Validate(selection.Package);
        foreach (var error in validation.Issues.Where(issue => issue.Severity.ToString().Equals("Error", StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.package_validator_error", error.TargetId ?? error.TargetPath ?? error.FilePath ?? "package", error.Message));
        }

        var categoriesPresent = RequiredAssetCategories.All(category => config.AssetRefs.Any(asset => asset.Category == category));
        if (!categoriesPresent)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.contract.asset_category_coverage", "asset_refs", "Runtime export must include tile, portrait, icon, sound and music asset refs."));
        }

        if (manifest.Files.Count > MaxExportFileCount)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.file_count_budget", "export_manifest", "Export file manifest exceeds the bounded file-count budget."));
        }

        if (manifest.TotalByteCount > MaxExportByteCount)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.byte_budget", "export_manifest", "Export file manifest exceeds the bounded byte budget."));
        }

        var packageValidatorClean = !diagnostics.Any(item => item.Code == "unity_runtime_export.package_validator_error");
        var assetManifestValid = !diagnostics.Any(item => item.Code.Contains("asset", StringComparison.Ordinal));
        var selectedLoopResolutionPassed = !diagnostics.Any(item => item.Code.Contains("selected_loop", StringComparison.Ordinal) || item.Code.Contains("command_target", StringComparison.Ordinal));
        var exportManifestValid = manifestFilesValid && !diagnostics.Any(item => item.Code.Contains("manifest", StringComparison.Ordinal) || item.Code.Contains("exported_", StringComparison.Ordinal) || item.Code.Contains("unsafe_export_path", StringComparison.Ordinal));

        return new UnityRuntimeContractValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            PackageValidatorClean = packageValidatorClean,
            AssetManifestValid = assetManifestValid,
            ExportManifestValid = exportManifestValid,
            SelectedLoopResolutionPassed = selectedLoopResolutionPassed,
            RuntimePreviewDependencyFree = !diagnostics.Any(item => item.Code == "unity_runtime_export.contract.runtime_preview_dependency"),
            ExternalExecutionFlagsFalse = !launch.ExternalExecutionFlags.AnyExecuted() && !config.ExternalExecution.AnyExecuted(),
            WindowsExecutableProduced = launch.WindowsExecutableProduced,
            UnityEditorExecuted = launch.UnityEditorExecuted,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private UnityRuntimeExportReplayEvidence BuildReplayEvidence(
        string projectRoot,
        string artifactRoot,
        UnityRuntimeExportInputSelection selection)
    {
        var firstRoot = Path.Combine(artifactRoot, "replay-a");
        var secondRoot = Path.Combine(artifactRoot, "replay-b");
        var first = MaterializeExport(projectRoot, firstRoot, selection);
        var second = MaterializeExport(projectRoot, secondRoot, selection);
        return new UnityRuntimeExportReplayEvidence
        {
            Passed = string.Equals(first.Manifest.ManifestHash, second.Manifest.ManifestHash, StringComparison.Ordinal) &&
                     string.Equals(first.RuntimeConfig.ConfigHash, second.RuntimeConfig.ConfigHash, StringComparison.Ordinal),
            FirstExportManifestHash = first.Manifest.ManifestHash,
            ReplayedExportManifestHash = second.Manifest.ManifestHash,
            FirstRuntimeConfigHash = first.RuntimeConfig.ConfigHash,
            ReplayedRuntimeConfigHash = second.RuntimeConfig.ConfigHash
        };
    }

    private UnityRuntimeExportVariationEvidence BuildVariationEvidence(
        string projectRoot,
        string artifactRoot,
        ContentGenerationScaleReport contentReport,
        MinimumAssetPipelineReport assetReport)
    {
        var primary = SelectInput(contentReport, assetReport, 0);
        var variation = SelectInput(contentReport, assetReport, 1);
        if (!primary.Accepted || !variation.Accepted || string.Equals(primary.PackId, variation.PackId, StringComparison.Ordinal))
        {
            return new UnityRuntimeExportVariationEvidence();
        }

        var first = MaterializeExport(projectRoot, Path.Combine(artifactRoot, "variation-a"), primary);
        var second = MaterializeExport(projectRoot, Path.Combine(artifactRoot, "variation-b"), variation);
        return new UnityRuntimeExportVariationEvidence
        {
            Passed = !string.Equals(first.Manifest.ManifestHash, second.Manifest.ManifestHash, StringComparison.Ordinal) &&
                     !string.Equals(primary.PackageId, variation.PackageId, StringComparison.Ordinal),
            PrimaryPackId = primary.PackId,
            VariationPackId = variation.PackId,
            PrimaryExportManifestHash = first.Manifest.ManifestHash,
            VariationExportManifestHash = second.Manifest.ManifestHash
        };
    }

    private UnityRuntimeExportInvalidMatrix BuildInvalidMatrix(
        string projectRoot,
        string exportRoot,
        UnityRuntimeExportInputSelection selection,
        UnityRuntimeExportMaterialization? export,
        UnityRuntimeExportOptions settings)
    {
        var scenarios = new List<UnityRuntimeExportInvalidScenario>();
        if (!selection.Accepted || export == null)
        {
            scenarios.Add(InvalidScenario("missing_valid_export_baseline", [Diagnostic("error", "unity_runtime_export.invalid.no_valid_baseline", "invalid_matrix", "Invalid matrix requires one valid export baseline.")]));
            return FinishInvalidMatrix(scenarios);
        }

        scenarios.Add(Scenario("missing_prior_package_evidence", selection with { SourcePackageHash = string.Empty }, export.RuntimeConfig, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.package_hash_mismatch"));
        scenarios.Add(Scenario("missing_prior_asset_manifest_evidence", selection with { AssetManifestHash = string.Empty }, export.RuntimeConfig, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.asset_manifest_hash_mismatch"));
        scenarios.Add(Scenario("package_hash_mismatch", selection, export.RuntimeConfig with { PackageHash = "sha256/not-the-package" }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.package_hash_mismatch"));
        scenarios.Add(Scenario("asset_manifest_hash_mismatch", selection, export.RuntimeConfig with { AssetManifestHash = "sha256/not-the-asset-manifest" }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.asset_manifest_hash_mismatch"));
        scenarios.Add(Scenario("unresolved_package_id", selection, export.RuntimeConfig with { StartMapId = "map/missing" }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.start_map_unresolved"));
        scenarios.Add(Scenario("unresolved_asset_id", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { AssetId = "asset/missing" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.asset_ref_unresolved"));
        scenarios.Add(Scenario("missing_exported_file", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ExportRelativePath = "assets/missing.fixture" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.exported_asset_file_missing"));
        scenarios.Add(Scenario("mismatched_exported_file_hash", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { Hash = "0" + asset.Hash.Skip(1).Aggregate(new StringBuilder(), (b, c) => b.Append(c)).ToString() }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.exported_asset_hash_mismatch"));
        scenarios.Add(Scenario("path_traversal_export_path", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ExportRelativePath = "../escape.fixture" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.unsafe_export_path"));
        scenarios.Add(Scenario("absolute_export_path", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ExportRelativePath = "C:/escape.fixture" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.unsafe_export_path"));
        scenarios.Add(Scenario("executable_script_provider_payload_injection", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ExportRelativePath = "assets/payload.exe" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.executable_payload_injection"));
        scenarios.Add(Scenario("copied_expectation_report_without_files", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ExportRelativePath = settings.IncludeExpectationOnlyInvalidMutation ? "assets/copied-report-only.fixture" : asset.ExportRelativePath }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.exported_asset_file_missing"));
        scenarios.Add(Scenario("runtime_preview_only_dependency", selection, export.RuntimeConfig with { RuntimeHostKind = "winforms_runtime_preview" }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.runtime_preview_dependency"));
        scenarios.Add(Scenario("unity_editor_build_claim_without_artifact", selection, export.RuntimeConfig, export.LaunchMetadata with { UnityEditorExecuted = true, UnityBuildProduced = true, RequiresUnityEditorLaunch = true }, export.Manifest, "unity_runtime_export.contract.unity_editor_claim_without_artifact"));
        scenarios.Add(Scenario("cross_pack_or_cross_asset_leakage", selection, export.RuntimeConfig with { AssetRefs = ReplaceFirstAsset(export.RuntimeConfig.AssetRefs, asset => asset with { ContentId = "content/from/other-pack" }) }, export.LaunchMetadata, export.Manifest, "unity_runtime_export.contract.asset_ref_unresolved"));

        return FinishInvalidMatrix(scenarios);

        UnityRuntimeExportInvalidScenario Scenario(
            string id,
            UnityRuntimeExportInputSelection mutatedSelection,
            UnityRuntimeConfig mutatedConfig,
            UnityRuntimeLaunchMetadata mutatedLaunch,
            UnityRuntimeExportFileManifest mutatedManifest,
            string expectedCode)
        {
            var validation = ValidateExport(projectRoot, exportRoot, mutatedSelection, mutatedConfig, mutatedLaunch, mutatedManifest);
            var diagnostics = validation.Diagnostics
                .Where(item => item.Severity == "error")
                .ToList();
            if (!diagnostics.Any(item => item.Code == expectedCode) && id == "copied_expectation_report_without_files" && !settings.IncludeExpectationOnlyInvalidMutation)
            {
                diagnostics = [];
            }

            return InvalidScenario(id, diagnostics);
        }
    }

    private UnityRuntimeExportInvalidMatrix FinishInvalidMatrix(IReadOnlyList<UnityRuntimeExportInvalidScenario> scenarios)
    {
        var diagnostics = scenarios.SelectMany(item => item.Diagnostics).ToList();
        return new UnityRuntimeExportInvalidMatrix
        {
            Passed = scenarios.Count >= 14 && scenarios.All(item => !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityRuntimeExportInvalidScenario InvalidScenario(string scenarioId, IReadOnlyList<UnityRuntimeExportDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static bool ValidateManifestFiles(
        string exportRoot,
        UnityRuntimeExportFileManifest manifest,
        ICollection<UnityRuntimeExportDiagnostic> diagnostics)
    {
        var passed = true;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in manifest.Files.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            if (!seen.Add(file.RelativePath))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.duplicate_file", file.RelativePath, "Export manifest cannot contain duplicate file paths."));
                passed = false;
            }

            if (!IsSafeRelativePath(file.RelativePath))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.unsafe_path", file.RelativePath, "Export manifest paths must be relative and contained."));
                passed = false;
                continue;
            }

            if (IsExecutableOrScriptPath(file.RelativePath))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.executable_payload_injection", file.RelativePath, "Export manifest must not include executable/script/provider payloads."));
                passed = false;
            }

            var path = Path.GetFullPath(Path.Combine(exportRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsContained(exportRoot, path) || !File.Exists(path))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.file_missing", file.RelativePath, "Every export manifest entry must point to an existing file."));
                passed = false;
                continue;
            }

            var bytes = File.ReadAllBytes(path);
            if (bytes.LongLength != file.ByteCount)
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.byte_count_mismatch", file.RelativePath, "Export manifest byte count must match actual file bytes."));
                passed = false;
            }

            if (!string.Equals(ComputeHash(bytes), file.Hash, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_export.manifest.hash_mismatch", file.RelativePath, "Export manifest hash must match actual file bytes."));
                passed = false;
            }
        }

        return passed;
    }

    private static IReadOnlyList<UnityRuntimeExportAssetRef> ReplaceFirstAsset(
        IReadOnlyList<UnityRuntimeExportAssetRef> assets,
        Func<UnityRuntimeExportAssetRef, UnityRuntimeExportAssetRef> mutate)
    {
        if (assets.Count == 0)
        {
            return assets;
        }

        return assets.Select((asset, index) => index == 0 ? mutate(asset) : asset).ToList();
    }

    private static object BuildGeneratedContentPayload(UnityRuntimeExportInputSelection selection) => new
    {
        schemaVersion = "unity_runtime_generated_content_payload_v1",
        selection.PackageId,
        selection.PackId,
        selection.ContentCatalogHash,
        selection.SelectedThreadId,
        selectedGeneratedIds = selection.SelectedGeneratedIds,
        commandHints = selection.SelectedRuntimeCommands,
        saveLoad = new
        {
            selection.RuntimeStateHash,
            selection.RestoredRuntimeStateHash,
            selection.SaveLoadRoundtripPassed
        }
    };

    private static object BuildAssetPayload(UnityRuntimeExportInputSelection selection) => new
    {
        schemaVersion = "unity_runtime_asset_payload_v1",
        selection.PackageId,
        selection.AssetManifestHash,
        selection.AssetManifestPackageHash,
        selection.AssetManifestContentHash,
        assets = selection.SelectedAssetRefs.OrderBy(item => item.Category, StringComparer.Ordinal).ThenBy(item => item.AssetId, StringComparer.Ordinal)
    };

    private static UnityRuntimeExportAssetRef AssetRef(ResolvedMinimumAsset asset) =>
        new()
        {
            AssetId = asset.AssetId,
            SlotId = asset.SlotId,
            Category = asset.Category,
            MediaType = asset.MediaType,
            ContentId = asset.ContentId,
            SourceRelativePath = asset.RelativePath,
            ExportRelativePath = Path.Combine("assets", SafeSegment(asset.Category), SafeSegment(asset.AssetId) + ".fixture").Replace('\\', '/'),
            Hash = asset.Hash,
            ByteCount = asset.ByteCount,
            ResolutionKind = asset.ResolutionKind
        };

    private static IReadOnlyList<string> PackageIds(GamePackageDefinition package)
    {
        var ids = new SortedSet<string>(StringComparer.Ordinal)
        {
            package.Manifest.PackageId
        };
        if (!string.IsNullOrWhiteSpace(package.Manifest.StartMapId))
        {
            ids.Add(package.Manifest.StartMapId);
        }

        foreach (var item in package.Game.Maps) ids.Add(item.Id);
        foreach (var item in package.Game.TilePrototypes) ids.Add(item.Id);
        foreach (var item in package.Game.EntityPrototypes) ids.Add(item.Id);
        foreach (var item in package.Game.Items) ids.Add(item.Id);
        foreach (var item in package.Game.Quests) ids.Add(item.Id);
        foreach (var objective in package.Game.Quests.SelectMany(item => item.Objectives)) ids.Add(objective.Id);
        foreach (var item in package.Game.Dialogues) ids.Add(item.Id);
        foreach (var choice in package.Game.Dialogues.SelectMany(item => item.Nodes).SelectMany(node => node.Choices)) ids.Add(choice.Id);
        foreach (var item in package.Game.Interactions) ids.Add(item.Id);
        foreach (var item in package.Game.Encounters) ids.Add(item.Id);
        foreach (var item in package.Game.Factions) ids.Add(item.Id);
        foreach (var item in package.GeneratedContent.Regions) ids.Add(item.SourceId);
        foreach (var item in package.GeneratedContent.Scenes)
        {
            ids.Add(item.SourceId);
            ids.Add(item.PackageMapId);
        }

        foreach (var item in package.GeneratedContent.Npcs) ids.Add(item.SourceId);
        foreach (var item in package.GeneratedContent.Items) ids.Add(item.SourceId);
        foreach (var item in package.GeneratedContent.Dialogues) ids.Add(item.SourceId);
        foreach (var item in package.GeneratedContent.Encounters) ids.Add(item.SourceId);
        foreach (var item in package.GeneratedContent.Quests)
        {
            ids.Add(item.SourceId);
            ids.Add(item.PackageQuestId);
        }

        return ids.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
    }

    private static void WriteJson(string exportRoot, string relativePath, object payload, ICollection<UnityRuntimeExportFileManifestEntry>? files)
    {
        var path = OutputPath(exportRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions), Utf8WithoutBom);
        files?.Add(FileEntry(exportRoot, relativePath, "json_payload", relativePath));
    }

    private static UnityRuntimeExportFileManifestEntry FileEntry(string exportRoot, string relativePath, string kind, string logicalId)
    {
        var path = OutputPath(exportRoot, relativePath);
        var bytes = File.ReadAllBytes(path);
        return new UnityRuntimeExportFileManifestEntry
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Kind = kind,
            LogicalId = logicalId,
            Hash = ComputeHash(bytes),
            ByteCount = bytes.LongLength
        };
    }

    private static string RenderReport(UnityRuntimeExportReport report)
    {
        var lines = new List<string>
        {
            "# Unity Runtime Export Vertical Slice Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Selected package: {report.SelectedInput.PackageId}",
            $"- Selected pack: {report.SelectedInput.PackId}",
            $"- Package hash: {report.SelectedInput.SourcePackageHash}",
            $"- Asset manifest hash: {report.SelectedInput.AssetManifestHash}",
            $"- Export folder: {report.ExportFolderRelativePath}",
            $"- Export files: {report.ExportFileCount}",
            $"- Export bytes: {report.ExportByteCount}",
            $"- Export manifest hash: {report.ExportManifestHash}",
            $"- Runtime config hash: {report.RuntimeConfigHash}",
            $"- Windows executable produced: {report.WindowsExecutableProduced.ToString().ToLowerInvariant()}",
            $"- Unity Editor executed: {report.UnityEditorExecuted.ToString().ToLowerInvariant()}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            string.Empty,
            "## Selected Loop",
            string.Empty,
            $"- Thread: {report.SelectedInput.SelectedThreadId}",
            $"- Generated ids: {string.Join(", ", report.SelectedInput.SelectedGeneratedIds.Take(12))}",
            $"- Command hints: {report.SelectedInput.SelectedRuntimeCommands.Count}",
            string.Empty,
            "## Asset Refs"
        };
        lines.AddRange(report.SelectedInput.SelectedAssetRefs.Select(asset => $"- {asset.Category}: {asset.AssetId} -> {asset.ExportRelativePath}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid Matrix");
        lines.Add(string.Empty);
        foreach (var scenario in report.InvalidMatrix.Scenarios)
        {
            lines.Add($"- {scenario.ScenarioId}: actualValid={scenario.ActualValid.ToString().ToLowerInvariant()} diagnostics={string.Join(", ", scenario.Diagnostics.Select(item => item.Code).Distinct(StringComparer.Ordinal))}");
        }

        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(diagnostic => $"- {diagnostic.Severity}: {diagnostic.Code} [{diagnostic.Target}] {diagnostic.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(UnityRuntimeExportReport report)
    {
        var lines = new List<string>
        {
            "# Unity Runtime Export Verification",
            string.Empty,
            "Final gate remains:",
            string.Empty,
            "```text",
            ManualGate,
            "```",
            string.Empty,
            "- Do not mark this gate passed in Goal 012 output.",
            "- Later-slice and post-Goal-012 work remain unstarted.",
            $"- Export artifact hash: {report.DeterministicHash}",
            $"- Export manifest hash: {report.ExportManifestHash}",
            $"- Valid matrix passed: {report.ValidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- Invalid matrix passed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- Unity Editor executed: {report.UnityEditorExecuted.ToString().ToLowerInvariant()}",
            $"- Windows executable produced: {report.WindowsExecutableProduced.ToString().ToLowerInvariant()}",
            $"- External execution flags all false: {(!report.ExternalExecution.AnyExecuted()).ToString().ToLowerInvariant()}"
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<UnityRuntimeExportDiagnostic> SortDiagnostics(IEnumerable<UnityRuntimeExportDiagnostic> diagnostics) =>
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

    private static UnityRuntimeExportDiagnostic Diagnostic(string severity, string code, string target, string message) =>
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

    private static string OutputPath(string root, string relativePath)
    {
        if (!IsSafeRelativePath(relativePath))
        {
            throw new InvalidOperationException("Unsafe output path: " + relativePath);
        }

        var path = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, path);
        return path;
    }

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains('\\') &&
        !path.Contains(':', StringComparison.Ordinal) &&
        !path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(part => part == "..");

    private static bool IsExecutableOrScriptPath(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".exe" or ".dll" or ".bat" or ".cmd" or ".ps1" or ".sh" or ".js" or ".lua";
    }

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

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                builder.Append(ch);
            }
            else if (ch is '/' or '_' or '-' or '.')
            {
                builder.Append('-');
            }
        }

        var safe = builder.ToString().Trim('-');
        while (safe.Contains("--", StringComparison.Ordinal))
        {
            safe = safe.Replace("--", "-", StringComparison.Ordinal);
        }

        return safe.Length == 0 ? "id" : safe;
    }

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record UnityRuntimeExportOptions
{
    public int SelectionOrdinal { get; init; }
    public bool IncludeExpectationOnlyInvalidMutation { get; init; } = true;
    public string RelativeOutputDirectoryOverride { get; init; } = string.Empty;
}

public sealed record UnityRuntimeExportAcceptanceResult
{
    public UnityRuntimeExportReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityRuntimeExportWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ExportDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityRuntimeExportReport
{
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal011GateRecorded { get; init; }
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public UnityRuntimeExportInputSelection SelectedInput { get; init; } = new();
    public string ExportFolderRelativePath { get; init; } = string.Empty;
    public int ExportFileCount { get; init; }
    public long ExportByteCount { get; init; }
    public string ExportManifestHash { get; init; } = string.Empty;
    public string RuntimeConfigHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool ValidMatrixPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PackageValidationPassed { get; init; }
    public bool AssetManifestValidationPassed { get; init; }
    public bool ExportManifestValidationPassed { get; init; }
    public bool SelectedLoopResolutionPassed { get; init; }
    public UnityRuntimeExportReplayEvidence ReplayEvidence { get; init; } = new();
    public UnityRuntimeExportVariationEvidence VariationEvidence { get; init; } = new();
    public UnityRuntimeContractValidationResult ContractValidation { get; init; } = new();
    public UnityRuntimeExportInvalidMatrix InvalidMatrix { get; init; } = new();
    public UnityRuntimeExportExternalExecutionFlags ExternalExecution { get; init; } = new();
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public IReadOnlyList<UnityRuntimeExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeExportInputSelection
{
    [JsonIgnore]
    public GamePackageDefinition Package { get; init; } = new();
    [JsonIgnore]
    public MinimumAssetManifest AssetManifest { get; init; } = new();
    [JsonIgnore]
    public IReadOnlyList<string> PackageContentIds { get; init; } = [];
    public bool Accepted { get; init; }
    public string SelectionPolicy { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackId { get; init; } = string.Empty;
    public string SourcePackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string AssetManifestPackageHash { get; init; } = string.Empty;
    public string AssetManifestContentHash { get; init; } = string.Empty;
    public string ContentCatalogHash { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedGeneratedIds { get; init; } = [];
    public IReadOnlyList<UnityRuntimeExportCommandHint> SelectedRuntimeCommands { get; init; } = [];
    public IReadOnlyList<UnityRuntimeExportAssetRef> SelectedAssetRefs { get; init; } = [];
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool PackageValidatorClean { get; init; }
    public IReadOnlyList<UnityRuntimeExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeExportCommandHint
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string InventoryId { get; init; } = string.Empty;
    public double Amount { get; init; }
}

public sealed record UnityRuntimeExportAssetRef
{
    public string AssetId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string ExportRelativePath { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
    public string ResolutionKind { get; init; } = string.Empty;
}

public sealed record UnityRuntimeConfig
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string RuntimeHostKind { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageVersion { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public string StartSceneId { get; init; } = string.Empty;
    public UnityRuntimePlayerSpawn PlayerSpawn { get; init; } = new();
    public string SelectedThreadId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedGeneratedIds { get; init; } = [];
    public IReadOnlyList<UnityRuntimeExportCommandHint> CommandHints { get; init; } = [];
    public IReadOnlyList<UnityRuntimeExportAssetRef> AssetRefs { get; init; } = [];
    public UnityRuntimeSaveLoadBootstrap SaveLoad { get; init; } = new();
    public UnityRuntimeExportExternalExecutionFlags ExternalExecution { get; init; } = new();
    public string ConfigHash { get; init; } = string.Empty;
}

public sealed record UnityRuntimePlayerSpawn
{
    public string StateRef { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record UnityRuntimeSaveLoadBootstrap
{
    public string RuntimeStateOwner { get; init; } = string.Empty;
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public string BootstrapMetadataPath { get; init; } = string.Empty;
}

public sealed record UnityRuntimeLaunchMetadata
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string RuntimeHostKind { get; init; } = string.Empty;
    public string LaunchMode { get; init; } = string.Empty;
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool UnityBuildProduced { get; init; }
    public bool RequiresUnityEditorLaunch { get; init; }
    public UnityRuntimeExportExternalExecutionFlags ExternalExecutionFlags { get; init; } = new();
}

public sealed record UnityRuntimeExportFileManifest
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public int FileCount => Files.Count;
    public long TotalByteCount { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<UnityRuntimeExportFileManifestEntry> Files { get; init; } = [];
}

public sealed record UnityRuntimeExportFileManifestEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string LogicalId { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record UnityRuntimeContractValidationResult
{
    public bool Passed { get; init; }
    public bool PackageValidatorClean { get; init; }
    public bool AssetManifestValid { get; init; }
    public bool ExportManifestValid { get; init; }
    public bool SelectedLoopResolutionPassed { get; init; }
    public bool RuntimePreviewDependencyFree { get; init; }
    public bool ExternalExecutionFlagsFalse { get; init; }
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public IReadOnlyList<UnityRuntimeExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeExportReplayEvidence
{
    public bool Passed { get; init; }
    public string FirstExportManifestHash { get; init; } = string.Empty;
    public string ReplayedExportManifestHash { get; init; } = string.Empty;
    public string FirstRuntimeConfigHash { get; init; } = string.Empty;
    public string ReplayedRuntimeConfigHash { get; init; } = string.Empty;
}

public sealed record UnityRuntimeExportVariationEvidence
{
    public bool Passed { get; init; }
    public string PrimaryPackId { get; init; } = string.Empty;
    public string VariationPackId { get; init; } = string.Empty;
    public string PrimaryExportManifestHash { get; init; } = string.Empty;
    public string VariationExportManifestHash { get; init; } = string.Empty;
}

public sealed record UnityRuntimeExportInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<UnityRuntimeExportInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityRuntimeExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeExportInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<UnityRuntimeExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeExportExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool MediaExecuted { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool WindowsExecutableExecuted { get; init; }

    public bool AnyExecuted() =>
        LlmExecuted ||
        RagExecuted ||
        ProviderExecuted ||
        LuaExecuted ||
        MediaExecuted ||
        UnityEditorExecuted ||
        UnityBuildExecuted ||
        WindowsExecutableExecuted;
}

public sealed record UnityRuntimeExportDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record UnityRuntimeExportMaterialization
{
    public UnityRuntimeConfig RuntimeConfig { get; init; } = new();
    public UnityRuntimeLaunchMetadata LaunchMetadata { get; init; } = new();
    public UnityRuntimeExportFileManifest Manifest { get; init; } = new();
}
