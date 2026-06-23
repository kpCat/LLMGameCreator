using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveMaterializationService
{
    public const string RelativeOutputDirectory = ".llmgc/unity-archive";
    public const string OptionalZipRelativePath = ".llmgc/unity-archive.zip";
    public const string ValidationFilePath = "export-validation.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

private readonly UnityArchiveExportDryRunService _dryRunService;
    private readonly UnityArchiveGameDataPayloadService _gameDataPayloadService;
    private readonly UnityArchiveAssetAudioLuaRequestService _requestPipelineService;
    private readonly UnityArchiveProviderJobPlanService _providerJobPlanService;
    private readonly UnityArchiveFulfillmentStateService _fulfillmentStateService;

    public UnityArchiveMaterializationService(
        UnityArchiveExportDryRunService dryRunService,
        UnityArchiveGameDataPayloadService? gameDataPayloadService = null,
        UnityArchiveAssetAudioLuaRequestService? requestPipelineService = null,
        UnityArchiveProviderJobPlanService? providerJobPlanService = null,
        UnityArchiveFulfillmentStateService? fulfillmentStateService = null)
    {
        _dryRunService = dryRunService ?? throw new ArgumentNullException(nameof(dryRunService));
        _gameDataPayloadService = gameDataPayloadService ?? new UnityArchiveGameDataPayloadService();
        _requestPipelineService = requestPipelineService ?? new UnityArchiveAssetAudioLuaRequestService();
        _providerJobPlanService = providerJobPlanService ?? new UnityArchiveProviderJobPlanService();
        _fulfillmentStateService = fulfillmentStateService ?? new UnityArchiveFulfillmentStateService();
    }

    public async Task<UnityArchiveMaterializationResult> MaterializeAsync(
        UnityArchiveMaterializationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DesignBrief);
        ArgumentNullException.ThrowIfNull(request.TargetProfile);
        ArgumentNullException.ThrowIfNull(request.ArchiveManifest);
        ArgumentNullException.ThrowIfNull(request.RuntimeModules);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var projectRoot = Path.GetFullPath(request.ProjectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "unity-archive"));
        EnsureContained(projectRoot, outputDirectory, "Unity archive output directory");

        var dryRun = await _dryRunService.ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = request.DesignBrief,
            TargetProfile = request.TargetProfile,
            ArchiveManifest = request.ArchiveManifest,
            RuntimeModules = request.RuntimeModules
        }, cancellationToken).ConfigureAwait(false);

        var pipelineResult = _requestPipelineService.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = request.ProjectRootPath,
            DesignBrief = request.DesignBrief,
            TargetProfile = request.TargetProfile,
            ArchiveManifest = request.ArchiveManifest,
            RuntimeModules = request.RuntimeModules,
            Package = request.GamePackage
        });
        var providerJobPlan = _providerJobPlanService.BuildPlan(new UnityArchiveProviderJobPlanRequest
        {
            ProjectRootPath = request.ProjectRootPath,
            RequestPipeline = pipelineResult,
            ArchiveManifest = request.ArchiveManifest,
DesignBrief = request.DesignBrief,
            TargetProfile = request.TargetProfile
        });

        var readiness = CombineMaterializationReadiness(
            dryRun.Plan.Readiness,
            pipelineResult.Readiness,
            providerJobPlan.Readiness);
        var diagnostics = CreateDiagnostics(dryRun, request.CreateZip, readiness, pipelineResult, providerJobPlan);

        ResetOutputDirectory(outputDirectory);
        var files = new List<UnityArchiveMaterializedFile>();
        if (dryRun.Plan.Readiness is not (UnityArchiveExportReadiness.Invalid or UnityArchiveExportReadiness.MissingRequirements))
        {
            await WriteArchiveFilesAsync(outputDirectory, request, files, pipelineResult, providerJobPlan, cancellationToken).ConfigureAwait(false);
        }

        var orderedFiles = files
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        var validationPath = OutputPath(outputDirectory, ValidationFilePath);
        orderedFiles.Add(MaterializedFile(ValidationFilePath, "validation_report"));
        orderedFiles = orderedFiles
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        await WriteJsonAsync(validationPath, new UnityArchiveMaterializationValidationReport
        {
            Readiness = readiness,
            DryRunReadiness = dryRun.Plan.Readiness,
            Diagnostics = diagnostics,
            MaterializedFiles = orderedFiles
        }, cancellationToken).ConfigureAwait(false);

        return new UnityArchiveMaterializationResult
        {
            OutputDirectoryPath = outputDirectory,
            ValidationReportPath = validationPath,
            ZipFilePath = null,
            Readiness = readiness,
            MaterializedFiles = orderedFiles,
            Diagnostics = diagnostics,
            DryRunResult = dryRun
        };
    }

    private async Task WriteArchiveFilesAsync(
        string outputDirectory,
        UnityArchiveMaterializationRequest request,
        ICollection<UnityArchiveMaterializedFile> files,
        UnityArchiveRequestPipelineResult pipelineResult,
        UnityArchiveProviderJobPlanResult providerJobPlan,
        CancellationToken cancellationToken)
    {
        await WriteJsonFileAsync(outputDirectory, "manifest/unity-game-archive.json", "archive_manifest", request.ArchiveManifest, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "composition/game-design-brief.json", "design_brief", request.DesignBrief, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "composition/unity-target-profile.json", "target_profile", request.TargetProfile, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "composition/runtime-modules-index.json", "runtime_modules", new UnityArchiveRuntimeModulesIndex
        {
            Modules = request.RuntimeModules
                .OrderBy(module => module.ModuleId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(module => module.ModuleId, StringComparer.Ordinal)
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "ui/layouts-index.json", "ui_layouts", new UnityArchiveUiLayoutsIndex
        {
            Layouts = request.ArchiveManifest.UiLayouts
                .OrderBy(layout => layout.LayoutId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(layout => layout.LayoutId, StringComparer.Ordinal)
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);

        await WriteJsonFileAsync(outputDirectory, "assets/asset-requests.json", "asset_requests", new UnityArchiveAssetRequestsIndex
        {
            Requests = pipelineResult.AssetRequests
                .OrderBy(item => item.RequestId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RequestId, StringComparer.Ordinal)
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "assets/asset-request-index.json", "asset_request_index", new UnityArchiveAssetRequestIndex
        {
            Requests = pipelineResult.AssetRequests
                .OrderBy(item => item.RequestId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RequestId, StringComparer.Ordinal)
                .Select(item => new UnityArchiveAssetRequestIndexEntry
                {
                    RequestId = item.RequestId,
                    AssetId = item.AssetId,
                    AssetKind = item.AssetKind,
                    ProviderKind = item.ProviderKind,
                    SourceRef = item.SourceRef
                })
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "audio/audio-requests.json", "audio_requests", new UnityArchiveAudioRequestsIndex
        {
            Requests = pipelineResult.AudioRequests
                .OrderBy(item => item.RequestId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RequestId, StringComparer.Ordinal)
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "audio/audio-request-index.json", "audio_request_index", new UnityArchiveAudioRequestIndex
        {
            Requests = pipelineResult.AudioRequests
                .OrderBy(item => item.RequestId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RequestId, StringComparer.Ordinal)
                .Select(item => new UnityArchiveAudioRequestIndexEntry
                {
                    RequestId = item.RequestId,
                    AudioId = item.AudioId,
                    AudioKind = item.AudioKind,
                    ProviderKind = item.ProviderKind,
                    SourceRef = item.SourceRef
                })
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "lua/module-requests.json", "lua_module_requests", new UnityArchiveLuaModuleRequests
        {
            Requests = pipelineResult.LuaModuleRequests
                .OrderBy(item => item.ModuleId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.ModuleId, StringComparer.Ordinal)
                .ToList()
        }, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "lua/modules-index.json", "lua_modules", new UnityArchiveLuaModulesIndex
        {
            ModuleIds = Normalize(pipelineResult.LuaModuleRequests.Select(r => r.ModuleId))
        }, files, cancellationToken).ConfigureAwait(false);
await WriteJsonFileAsync(outputDirectory, "production/fulfillment-plan.json", "fulfillment_plan", providerJobPlan.FulfillmentPlan, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "production/readiness-report.json", "provider_readiness_report", providerJobPlan.ReadinessReport, files, cancellationToken).ConfigureAwait(false);
        
        var fulfillmentState = _fulfillmentStateService.Scan(new UnityArchiveFulfillmentStateRequest
        {
            OutputDirectoryPath = outputDirectory,
            ProviderJobPlan = providerJobPlan
        });
        await WriteJsonFileAsync(outputDirectory, "production/fulfillment-state.json", "fulfillment_state", fulfillmentState.FulfillmentState, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "production/fulfilled-assets-index.json", "fulfilled_assets", fulfillmentState.FulfilledAssets, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "production/fulfilled-audio-index.json", "fulfilled_audio", fulfillmentState.FulfilledAudio, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "production/fulfilled-lua-index.json", "fulfilled_lua", fulfillmentState.FulfilledLua, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "production/invalid-outputs.json", "invalid_outputs", fulfillmentState.InvalidOutputs, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "assets/asset-slots.json", "asset_slots", providerJobPlan.AssetSlots, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "audio/audio-slots.json", "audio_slots", providerJobPlan.AudioSlots, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "lua/module-slots.json", "lua_module_slots", providerJobPlan.LuaModuleSlots, files, cancellationToken).ConfigureAwait(false);
        await WriteProviderJobFilesAsync(outputDirectory, providerJobPlan.ProviderJobs, files, cancellationToken).ConfigureAwait(false);
        await WriteJsonFileAsync(outputDirectory, "localization/index.json", "localization", new UnityArchiveLocalizationIndex
        {
            ContentLanguage = request.ArchiveManifest.ContentLanguage.Trim(),
            Files = Normalize(request.ArchiveManifest.LocalizationFiles)
        }, files, cancellationToken).ConfigureAwait(false);

        if (request.GamePackage != null)
        {
            var payload = await _gameDataPayloadService.WriteAsync(new UnityArchiveGameDataPayloadRequest
            {
                ProjectRootPath = request.ProjectRootPath,
                Package = request.GamePackage
            }, cancellationToken).ConfigureAwait(false);
            foreach (var payloadFile in payload.WrittenFiles)
            {
                files.Add(MaterializedFile(payloadFile.RelativePath, payloadFile.Kind));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CompositionReportMarkdown))
        {
            await WriteTextFileAsync(outputDirectory, "composition/composition-report.md", "composition_report", request.CompositionReportMarkdown, files, cancellationToken).ConfigureAwait(false);
        }

        await WriteTextFileAsync(outputDirectory, "export-report.md", "export_report", RenderReport(request, files), files, cancellationToken).ConfigureAwait(false);
    }

    private static string RenderReport(
        UnityArchiveMaterializationRequest request,
        IEnumerable<UnityArchiveMaterializedFile> files)
    {
        var lines = new List<string>
        {
            "# Unity Archive Materialization v1",
            string.Empty,
            $"- Game: `{request.ArchiveManifest.GameId.Trim()}`",
            $"- Design brief: `{request.DesignBrief.BriefId.Trim()}`",
            $"- Target profile: `{request.TargetProfile.TargetProfileId.Trim()}`",
            "- Unity implementation: not included",
            "- Archive purpose: deterministic contract and metadata for a future Unity player",
            string.Empty,
            "## Materialized contract files",
            string.Empty
        };
        lines.AddRange(files
            .Select(file => file.RelativePath)
            .Append("export-report.md")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => path, StringComparer.Ordinal)
            .Select(path => $"- `{path}`"));
        return string.Join("\n", lines) + "\n";
    }

    private static IReadOnlyList<UnityArchiveMaterializationDiagnostic> CreateDiagnostics(
        UnityArchiveExportDryRunResult dryRun,
        bool createZip,
        UnityArchiveMaterializationReadiness readiness,
        UnityArchiveRequestPipelineResult pipelineResult,
        UnityArchiveProviderJobPlanResult providerJobPlan)
    {
        var diagnostics = dryRun.Plan.Diagnostics.Select(diagnostic => new UnityArchiveMaterializationDiagnostic
        {
            Severity = diagnostic.Severity,
            Code = UnityArchiveMaterializationDiagnosticCodes.DryRunDiagnostic,
            Message = $"[{diagnostic.Code}] {diagnostic.Message}",
            TargetId = diagnostic.TargetId,
            RelatedId = diagnostic.RelatedId
        }).ToList();

foreach (var pipelineDiagnostic in pipelineResult.Diagnostics)
        {
            diagnostics.Add(new UnityArchiveMaterializationDiagnostic
            {
                Severity = pipelineDiagnostic.Severity,
                Code = pipelineDiagnostic.Code.StartsWith("request.", StringComparison.Ordinal)
                    ? pipelineDiagnostic.Code
                    : $"request.{pipelineDiagnostic.Code}",
                Message = pipelineDiagnostic.Message,
                TargetId = pipelineDiagnostic.TargetId
            });
        }

        foreach (var planDiagnostic in providerJobPlan.Diagnostics)
        {
            diagnostics.Add(new UnityArchiveMaterializationDiagnostic
            {
                Severity = planDiagnostic.Severity,
                Code = planDiagnostic.Code,
                Message = planDiagnostic.Message,
                TargetId = planDiagnostic.TargetId
            });
        }

        if (readiness == UnityArchiveMaterializationReadiness.MaterializedMetadataOnly)
        {
            diagnostics.Add(Diagnostic(
                UnityArchiveExportDiagnosticSeverity.Warning,
                UnityArchiveMaterializationDiagnosticCodes.FutureModulesMetadataOnly,
                "Planned future runtime modules allow metadata-only materialization; the archive is not a playable contract.",
                dryRun.Plan.ArchiveGameId));
        }

        if (readiness is UnityArchiveMaterializationReadiness.Blocked or UnityArchiveMaterializationReadiness.Invalid)
        {
            diagnostics.Add(Diagnostic(
                UnityArchiveExportDiagnosticSeverity.Error,
                UnityArchiveMaterializationDiagnosticCodes.MaterializationBlocked,
                "Dry-run validation blocked archive contract materialization; only validation output was written.",
                dryRun.Plan.ArchiveGameId));
        }

        if (createZip)
        {
            diagnostics.Add(Diagnostic(
                UnityArchiveExportDiagnosticSeverity.Info,
                UnityArchiveMaterializationDiagnosticCodes.ZipNotImplemented,
                "Deterministic zip output is optional and is not implemented in materialization v1.",
                dryRun.Plan.ArchiveGameId));
        }

        return diagnostics
            .Distinct()
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.RelatedId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static UnityArchiveMaterializationReadiness CombineMaterializationReadiness(
         UnityArchiveExportReadiness dryRunReadiness,
         UnityArchiveRequestReadiness pipelineReadiness,
         UnityArchiveProviderPlanReadiness planReadiness)
     {
         if (dryRunReadiness is UnityArchiveExportReadiness.Invalid or UnityArchiveExportReadiness.MissingRequirements)
         {
             return MapReadiness(dryRunReadiness);
         }

         if (pipelineReadiness is UnityArchiveRequestReadiness.BlockedByErrors ||
             planReadiness is UnityArchiveProviderPlanReadiness.BlockedByErrors)
         {
             return UnityArchiveMaterializationReadiness.Blocked;
         }

         if (pipelineReadiness is UnityArchiveRequestReadiness.ReadyWithWarnings ||
             planReadiness is UnityArchiveProviderPlanReadiness.ReadyWithWarnings)
         {
             if (dryRunReadiness is UnityArchiveExportReadiness.BlockedByFutureModules)
             {
                 return UnityArchiveMaterializationReadiness.MaterializedMetadataOnly;
             }
             return UnityArchiveMaterializationReadiness.MaterializedWithWarnings;
         }

         return MapReadiness(dryRunReadiness);
     }

     private static UnityArchiveMaterializationReadiness MapReadiness(UnityArchiveExportReadiness readiness)
    {
        return readiness switch
        {
            UnityArchiveExportReadiness.ExportableNow => UnityArchiveMaterializationReadiness.MaterializedPlayableContract,
            UnityArchiveExportReadiness.ExportableWithWarnings => UnityArchiveMaterializationReadiness.MaterializedWithWarnings,
            UnityArchiveExportReadiness.BlockedByFutureModules => UnityArchiveMaterializationReadiness.MaterializedMetadataOnly,
            UnityArchiveExportReadiness.MissingRequirements => UnityArchiveMaterializationReadiness.Blocked,
            _ => UnityArchiveMaterializationReadiness.Invalid
        };
    }

    private static async Task WriteJsonFileAsync<T>(
        string outputDirectory,
        string relativePath,
        string kind,
        T value,
        ICollection<UnityArchiveMaterializedFile> files,
        CancellationToken cancellationToken)
    {
        var path = OutputPath(outputDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await WriteJsonAsync(path, value, cancellationToken).ConfigureAwait(false);
        files.Add(MaterializedFile(relativePath, kind));
    }

    private static async Task WriteProviderJobFilesAsync(
        string outputDirectory,
        UnityArchiveProviderJobIndex providerJobs,
        ICollection<UnityArchiveMaterializedFile> files,
        CancellationToken cancellationToken)
    {
        var paths = new Dictionary<UnityArchiveRequestProviderKind, string>
        {
            [UnityArchiveRequestProviderKind.manual_import] = "providers/manual-import/jobs.json",
            [UnityArchiveRequestProviderKind.comfyui_future] = "providers/comfyui/jobs.json",
            [UnityArchiveRequestProviderKind.suno_future] = "providers/suno/jobs.json",
            [UnityArchiveRequestProviderKind.local_audio_future] = "providers/local-audio/jobs.json",
            [UnityArchiveRequestProviderKind.procedural_future] = "providers/procedural/jobs.json"
        };

        foreach (var batch in providerJobs.Batches)
        {
            if (!paths.TryGetValue(batch.ProviderKind, out var relativePath))
            {
                continue;
            }

            await WriteJsonFileAsync(outputDirectory, relativePath, "provider_jobs", batch, files, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteTextFileAsync(
        string outputDirectory,
        string relativePath,
        string kind,
        string content,
        ICollection<UnityArchiveMaterializedFile> files,
        CancellationToken cancellationToken)
    {
        var path = OutputPath(outputDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        files.Add(MaterializedFile(relativePath, kind));
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await File.WriteAllTextAsync(path, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static string OutputPath(string outputDirectory, string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(outputDirectory, normalized));
        EnsureContained(outputDirectory, path, "Unity archive materialized file");
        return path;
    }

    private static void ResetOutputDirectory(string outputDirectory)
    {
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, true);
        }

        Directory.CreateDirectory(outputDirectory);
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityArchiveMaterializedFile MaterializedFile(string relativePath, string kind)
    {
        return new UnityArchiveMaterializedFile { RelativePath = relativePath, Kind = kind };
    }

    private static UnityArchiveMaterializationDiagnostic Diagnostic(
        UnityArchiveExportDiagnosticSeverity severity,
        string code,
        string message,
        string targetId)
    {
        return new UnityArchiveMaterializationDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            TargetId = targetId
        };
    }

    private static void EnsureContained(string rootPath, string candidatePath, string pathLabel)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{pathLabel} must stay under '{root}'.");
        }
    }

    private static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityArchiveExportDiagnosticSeverity.Error => 0,
            UnityArchiveExportDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
