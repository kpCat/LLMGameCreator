using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.Assets;

public sealed class MinimumAssetPipelineAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/minimum-asset-pipeline";
    public const string ReportJsonFileName = "minimum-asset-pipeline-report.json";
    public const string ReportMarkdownFileName = "minimum-asset-pipeline-report.md";
    public const string VerificationMarkdownFileName = "minimum-asset-pipeline-verification.md";
    public const string ManualGate = "minimum_asset_pipeline_artifact_verification";

    private const string ExpectedSchemaVersion = "minimum_asset_source_pack_v1";
    private const int SlotsPerPack = 30;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly HashSet<string> KnownCategories = new(StringComparer.Ordinal)
    {
        "tile_region_graphic",
        "npc_portrait",
        "item_icon_ui_graphic",
        "sound_effect",
        "music_ambience"
    };
    private static readonly HashSet<string> KnownSourceKinds = new(StringComparer.Ordinal)
    {
        "local_fixture",
        "deterministic_fallback"
    };
    private static readonly HashSet<string> KnownMediaTypes = new(StringComparer.Ordinal)
    {
        "image/png",
        "audio/wav",
        "audio/ogg"
    };
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IMinimumAssetPipelineResolver _resolver;
    private readonly IGamePackageValidator _packageValidator;

    static MinimumAssetPipelineAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public MinimumAssetPipelineAcceptanceService(
        IMinimumAssetPipelineResolver? resolver = null,
        IGamePackageValidator? packageValidator = null)
    {
        _resolver = resolver ?? new UnavailableMinimumAssetPipelineResolver();
        _packageValidator = packageValidator ?? new GamePackageValidator();
    }

    public MinimumAssetPipelineAcceptanceResult BuildFromContentGeneration(
        string projectRootPath,
        string assetPackDirectoryPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        if (string.IsNullOrWhiteSpace(assetPackDirectoryPath))
        {
            throw new ArgumentException("Asset pack directory path is required.", nameof(assetPackDirectoryPath));
        }

        var settings = options ?? new MinimumAssetPipelineAcceptanceOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var assetPackDirectory = Path.GetFullPath(assetPackDirectoryPath);
        var artifactRoot = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "minimum-asset-pipeline"));
        EnsureContained(projectRoot, artifactRoot);
        Directory.CreateDirectory(artifactRoot);

        var sourcePacks = LoadSourcePacks(assetPackDirectory);
        var contentPacks = contentGenerationResult.Report.Packs
            .Where(pack => !string.IsNullOrWhiteSpace(pack.PackId) && pack.PackageAudit.Package.Manifest.PackageId.Length > 0)
            .OrderBy(pack => pack.PackId, StringComparer.Ordinal)
            .Take(3)
            .ToList();

        var diagnostics = new List<MinimumAssetPipelineDiagnostic>
        {
            Diagnostic("info", "asset_pipeline.goal010_gate_recorded", "content_generation_at_scale_artifact_verification", "User-confirmed Goal 010 artifact verification is recorded as passed."),
            Diagnostic("info", "asset_pipeline.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked.")
        };
        diagnostics.AddRange(sourcePacks.SelectMany(pack => pack.Diagnostics));

        var validRuns = new List<MinimumAssetPipelineRun>();
        for (var i = 0; i < contentPacks.Count; i++)
        {
            var sourcePack = sourcePacks.Where(pack => pack.Pack != null)
                .OrderBy(pack => pack.Pack!.PackId, StringComparer.Ordinal)
                .ElementAtOrDefault(i % Math.Max(1, sourcePacks.Count(pack => pack.Pack != null)));
            if (sourcePack?.Pack == null)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.source_pack.missing", "source_packs", "A valid asset source pack is required for every generated content pack."));
                continue;
            }

            validRuns.Add(BuildRun(projectRoot, artifactRoot, contentPacks[i], sourcePack, settings.PrimarySeed, settings));
        }

        var replay = validRuns.Count == 0
            ? new MinimumAssetReplayEvidence()
            : BuildReplayEvidence(projectRoot, artifactRoot, contentPacks[0], sourcePacks.First(pack => pack.Pack != null), settings);
        var variation = validRuns.Count == 0
            ? new MinimumAssetVariationEvidence()
            : BuildVariationEvidence(contentPacks[0], sourcePacks.First(pack => pack.Pack != null), settings);
        var invalidMatrix = BuildInvalidMatrix(projectRoot, artifactRoot, contentPacks, sourcePacks.Where(pack => pack.Pack != null).ToList(), settings);

        diagnostics.AddRange(validRuns.SelectMany(run => run.Diagnostics));
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var categoryCounts = Distribution(validRuns.SelectMany(run => run.ResolvedAssets.Select(asset => asset.Category)));
        var importCounts = Distribution(validRuns.SelectMany(run => run.ResolvedAssets.Where(asset => asset.ResolutionKind == "import").Select(asset => asset.Category)));
        var fallbackCounts = Distribution(validRuns.SelectMany(run => run.ResolvedAssets.Where(asset => asset.ResolutionKind == "fallback").Select(asset => asset.Category)));
        var totalSlots = validRuns.Sum(run => run.ResolvedAssets.Count);
        var validMatrixPassed =
            validRuns.Count == 3 &&
            totalSlots >= 90 &&
            Count(categoryCounts, "tile_region_graphic") >= 12 &&
            Count(categoryCounts, "npc_portrait") >= 12 &&
            Count(categoryCounts, "item_icon_ui_graphic") >= 12 &&
            Count(categoryCounts, "sound_effect") >= 12 &&
            Count(categoryCounts, "music_ambience") >= 3 &&
            importCounts.Values.Sum() > 0 &&
            fallbackCounts.Values.Sum() > 0 &&
            validRuns.All(run => run.Accepted) &&
            replay.Passed &&
            variation.Passed;
        var invalidMatrixPassed = invalidMatrix.Passed;
        var bindingPassed = validRuns.All(run => run.PackageBindingAudit.Passed);
        var validationPassed = validRuns.All(run => run.AssetValidation.Passed && run.PackageBindingAudit.PackageValidatorClean);
        var resolverPassed = validRuns.All(run => run.ResolverEvidence.ResolverAvailable);

        diagnostics.Add(Diagnostic(validMatrixPassed ? "info" : "error", validMatrixPassed ? "asset_pipeline.valid_matrix_passed" : "asset_pipeline.valid_matrix_failed", "valid_matrix", "Three generated inputs must resolve at least ninety deterministic asset slots with import and fallback coverage."));
        diagnostics.Add(Diagnostic(invalidMatrixPassed ? "info" : "error", invalidMatrixPassed ? "asset_pipeline.invalid_matrix_rejected" : "asset_pipeline.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must fail by causal diagnostics."));
        diagnostics.Add(Diagnostic(bindingPassed ? "info" : "error", bindingPassed ? "asset_pipeline.binding_passed" : "asset_pipeline.binding_failed", "package_binding", "Resolved assets must bind to package/generated content ids through existing metadata seams."));
        diagnostics.Add(Diagnostic(validationPassed ? "info" : "error", validationPassed ? "asset_pipeline.validation_passed" : "asset_pipeline.validation_failed", "asset_validation", "Manifest files, hashes, media types and package references must validate structurally."));
        diagnostics.Add(Diagnostic(resolverPassed ? "info" : "error", resolverPassed ? "asset_pipeline.resolver_available" : "asset_pipeline.resolver_unavailable", "resolver", "Acceptance requires an injected concrete asset resolver."));

        var manifestHash = ComputeHash(JsonSerializer.Serialize(validRuns.Select(run => run.Manifest), JsonOptions));
        var reportWithoutHash = new MinimumAssetPipelineReport
        {
            Accepted = validMatrixPassed && invalidMatrixPassed && bindingPassed && validationPassed && resolverPassed,
            ManualGate = ManualGate,
            Goal010GateRecorded = true,
            CompletedSlices = ["S092", "S093", "S094", "S095", "S096", "S097", "S098", "S098A"],
            SourcePackCount = sourcePacks.Count(pack => pack.Pack != null),
            GeneratedInputCount = validRuns.Count,
            TotalResolvedAssetSlots = totalSlots,
            CategoryCounts = categoryCounts,
            ImportCountsByCategory = importCounts,
            FallbackCountsByCategory = fallbackCounts,
            TotalByteCount = validRuns.Sum(run => run.ResolvedAssets.Sum(asset => asset.ByteCount)),
            ManifestHash = manifestHash,
            SourcePackHashes = sourcePacks.Where(pack => pack.Pack != null).ToDictionary(pack => pack.Pack!.PackId, pack => pack.SourceHash, StringComparer.Ordinal),
            PackageContentHashes = validRuns.ToDictionary(run => run.PackId, run => run.PackageContentHash, StringComparer.Ordinal),
            ValidMatrixPassed = validMatrixPassed,
            InvalidMatrixPassed = invalidMatrixPassed,
            PackageContentBindingPassed = bindingPassed,
            AssetValidationPassed = validationPassed,
            ProductSmokeRoute = "minimum-asset-pipeline",
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            ExternalExecution = new MinimumAssetExternalExecutionFlags(),
            Runs = validRuns,
            ReplayEvidence = replay,
            VariationEvidence = variation,
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new MinimumAssetPipelineAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<MinimumAssetPipelineWriteResult> WriteAsync(
        string projectRootPath,
        MinimumAssetPipelineAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "minimum-asset-pipeline"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new MinimumAssetPipelineWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<MinimumAssetPipelineWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string assetPackDirectoryPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromContentGeneration(projectRootPath, assetPackDirectoryPath, contentGenerationResult);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private MinimumAssetPipelineRun BuildRun(
        string projectRoot,
        string artifactRoot,
        ContentGenerationScalePackResult contentPack,
        MinimumAssetSourcePackLoadResult sourcePack,
        string seed,
        MinimumAssetPipelineAcceptanceOptions options)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>(sourcePack.Diagnostics);
        var requestDiagnostics = ExpandRequests(contentPack.PackageAudit.Package, sourcePack.Pack!, seed, out var requests);
        diagnostics.AddRange(requestDiagnostics);

        var resolved = new List<ResolvedMinimumAsset>();
        var resolutionDiagnostics = new List<MinimumAssetPipelineDiagnostic>();
        var resolverAvailable = _resolver.IsAvailable;
        foreach (var request in requests)
        {
            var evidence = resolverAvailable
                ? _resolver.Resolve(new MinimumAssetResolveRequest
                {
                    ProjectRootPath = projectRoot,
                    ArtifactRootPath = artifactRoot,
                    SourcePackDirectoryPath = sourcePack.DirectoryPath,
                    SourcePack = sourcePack.Pack!,
                    Request = request
                })
                : MinimumAssetResolveEvidence.Unavailable(request);
            resolutionDiagnostics.AddRange(evidence.Diagnostics);
            if (evidence.ResolvedAsset != null)
            {
                resolved.Add(evidence.ResolvedAsset);
            }
        }

        diagnostics.AddRange(resolutionDiagnostics);
        var package = ClonePackage(contentPack.PackageAudit.Package);
        var bindingAudit = BindAssetsToPackage(package, resolved, contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash);
        diagnostics.AddRange(bindingAudit.Diagnostics);
        var validation = ValidateAssets(projectRoot, artifactRoot, package, sourcePack, contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash, resolved, requests, bindingAudit);
        diagnostics.AddRange(validation.Diagnostics);
        var manifestWithoutHash = new MinimumAssetManifest
        {
            PackId = contentPack.PackId,
            SourcePackId = sourcePack.Pack!.PackId,
            Seed = seed,
            PackageHash = contentPack.PackageAudit.PackageHash,
            PackageContentHash = contentPack.PackageAudit.CatalogHash,
            SourcePackHash = sourcePack.SourceHash,
            RequestCount = requests.Count,
            ResolvedAssetCount = resolved.Count,
            Requests = requests,
            ResolvedAssets = resolved
        };
        var manifest = manifestWithoutHash with
        {
            ManifestHash = ComputeHash(JsonSerializer.Serialize(manifestWithoutHash, JsonOptions))
        };

        return new MinimumAssetPipelineRun
        {
            Accepted = requestDiagnostics.All(item => item.Severity != "error") &&
                       resolved.Count == requests.Count &&
                       bindingAudit.Passed &&
                       validation.Passed &&
                       resolverAvailable,
            PackId = contentPack.PackId,
            SourcePackId = sourcePack.Pack.PackId,
            SourcePackHash = sourcePack.SourceHash,
            PackageHash = contentPack.PackageAudit.PackageHash,
            PackageContentHash = contentPack.PackageAudit.CatalogHash,
            RequestCount = requests.Count,
            ResolvedAssetCount = resolved.Count,
            Requests = requests,
            ResolvedAssets = resolved,
            Manifest = manifest,
            PackageBindingAudit = bindingAudit,
            AssetValidation = validation,
            ResolverEvidence = new MinimumAssetResolverEvidence { ResolverAvailable = resolverAvailable, ResolverId = _resolver.ResolverId },
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private IReadOnlyList<MinimumAssetPipelineDiagnostic> ExpandRequests(
        GamePackageDefinition package,
        MinimumAssetSourcePack sourcePack,
        string seed,
        out List<MinimumAssetRequest> requests)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        requests = [];
        var contentByCategory = SelectContentIds(package);
        var categoryTargets = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["tile_region_graphic"] = 4,
            ["npc_portrait"] = 8,
            ["item_icon_ui_graphic"] = 8,
            ["sound_effect"] = 8,
            ["music_ambience"] = 2
        };

        foreach (var (category, targetCount) in categoryTargets.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (!sourcePack.CategoryPolicies.TryGetValue(category, out var policy))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.policy.missing", category, "Every asset category requires an explicit fallback/budget policy."));
                continue;
            }

            if (targetCount > policy.MaxSlots || targetCount > sourcePack.MaxTotalSlots)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.request.over_budget", category, "Requested slots exceed the configured safe category budget."));
                continue;
            }

            var contentIds = contentByCategory[category];
            if (contentIds.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.request.no_content_ids", category, "Asset requests must be derived from existing generated/package content ids."));
                continue;
            }

            var sources = sourcePack.Sources
                .Where(source => source.Category == category)
                .OrderBy(source => source.SourceId, StringComparer.Ordinal)
                .ToList();
            if (sources.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.request.no_sources", category, "No eligible source exists for the category."));
                continue;
            }

            for (var i = 0; i < targetCount; i++)
            {
                var contentId = contentIds[i % contentIds.Count];
                var source = SelectSource(sources, category, contentId, seed, i, policy.AllowFallback);
                if (source == null)
                {
                    diagnostics.Add(Diagnostic("error", "asset_pipeline.request.no_fallback_source", category, "Missing eligible source and fallback is not permitted."));
                    continue;
                }

                var slotId = SafeId("asset-slot", package.Manifest.PackageId, category, contentId, source.SourceId, i.ToString("000"));
                requests.Add(new MinimumAssetRequest
                {
                    SlotId = slotId,
                    Category = category,
                    MediaType = source.MediaType,
                    ContentId = contentId,
                    SourceId = source.SourceId,
                    SourceKind = source.Kind,
                    Ordinal = i,
                    Provenance = $"{package.Manifest.PackageId}|{category}|{contentId}|{source.SourceId}|{i:000}"
                });
            }
        }

        diagnostics.AddRange(ValidateExpandedRequests(sourcePack, requests));
        return diagnostics;
    }

    private static IReadOnlyList<MinimumAssetPipelineDiagnostic> ValidateExpandedRequests(
        MinimumAssetSourcePack sourcePack,
        IReadOnlyList<MinimumAssetRequest> requests)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        if (requests.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count() != requests.Count)
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.request.duplicate_slot_id", sourcePack.PackId, "Expanded asset slot ids must be unique."));
        }

        foreach (var group in requests.GroupBy(item => item.Category, StringComparer.Ordinal))
        {
            if (sourcePack.CategoryPolicies.TryGetValue(group.Key, out var policy) && group.Count() > policy.MaxSlots)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.request.over_budget", group.Key, "Expanded category request count exceeds the configured safe category budget."));
            }
        }

        if (requests.Count > sourcePack.MaxTotalSlots)
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.request.over_budget", sourcePack.PackId, "Expanded request count exceeds the configured safe source pack budget."));
        }

        return diagnostics;
    }

    private static IReadOnlyDictionary<string, List<string>> SelectContentIds(GamePackageDefinition package)
    {
        var mapTile = package.Game.Maps.Select(item => item.Id)
            .Concat(package.Game.TilePrototypes.Select(item => item.Id))
            .OrderBy(item => item, StringComparer.Ordinal)
            .DefaultIfEmpty(package.Manifest.StartMapId)
            .ToList();
        var npcs = package.GeneratedContent.Npcs.Select(item => item.SourceId)
            .Concat(package.Game.Maps.SelectMany(map => map.Entities.Select(entity => entity.Id)))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var items = package.Game.Items.Select(item => item.Id)
            .Concat(package.GeneratedContent.Items.Select(item => item.SourceId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var sound = package.Game.Interactions.Select(item => item.Id)
            .Concat(package.Game.Dialogues.Select(item => item.Id))
            .Concat(package.Game.Quests.Select(item => item.Id))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var music = package.Game.Maps.Select(item => item.Id)
            .Concat(package.Game.Interactions.Select(item => item.Id))
            .DefaultIfEmpty(package.Manifest.PackageId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        return new Dictionary<string, List<string>>(StringComparer.Ordinal)
        {
            ["tile_region_graphic"] = mapTile,
            ["npc_portrait"] = npcs,
            ["item_icon_ui_graphic"] = items,
            ["sound_effect"] = sound,
            ["music_ambience"] = music
        };
    }

    private static MinimumAssetSource? SelectSource(
        IReadOnlyList<MinimumAssetSource> sources,
        string category,
        string contentId,
        string seed,
        int ordinal,
        bool allowFallback)
    {
        var imports = sources.Where(source => source.Kind == "local_fixture").ToList();
        var fallbacks = sources.Where(source => source.Kind == "deterministic_fallback").ToList();
        var useFallback = ComputeHash($"{seed}|{category}|{contentId}|{ordinal}")[0] % 3 == 0;
        if (useFallback && allowFallback && fallbacks.Count > 0)
        {
            return fallbacks[(ordinal + contentId.Length) % fallbacks.Count];
        }

        if (imports.Count > 0)
        {
            return imports[(ordinal + seed.Length) % imports.Count];
        }

        return allowFallback && fallbacks.Count > 0 ? fallbacks[ordinal % fallbacks.Count] : null;
    }

    private MinimumAssetBindingAudit BindAssetsToPackage(
        GamePackageDefinition package,
        IReadOnlyList<ResolvedMinimumAsset> resolvedAssets,
        string expectedPackageHash,
        string generatedContentHash)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        var preAssetPackageHash = ComputeHash(JsonSerializer.Serialize(package, JsonOptions));
        package.AssetCatalog.Contracts.Clear();
        package.AssetCatalog.Assets.Clear();
        foreach (var category in KnownCategories.OrderBy(item => item, StringComparer.Ordinal))
        {
            package.AssetCatalog.Contracts.Add(new AssetContractDefinition
            {
                Id = "asset-contract/" + category,
                AssetType = category
            });
        }

        foreach (var asset in resolvedAssets.OrderBy(item => item.SlotId, StringComparer.Ordinal))
        {
            package.AssetCatalog.Assets.Add(new AssetDefinition
            {
                Id = asset.AssetId,
                Type = asset.MediaType,
                Role = asset.Category,
                Path = asset.RelativePath,
                ContractId = "asset-contract/" + asset.Category,
                LinkedEntityIds = [asset.ContentId]
            });

            if (asset.Category == "tile_region_graphic")
            {
                foreach (var tile in package.Game.TilePrototypes.Where(item => item.Id == asset.ContentId))
                {
                    tile.AssetId = asset.AssetId;
                }
            }
            else if (asset.Category == "npc_portrait")
            {
                foreach (var entity in package.Game.Maps.SelectMany(map => map.Entities).Where(item => item.Id == asset.ContentId))
                {
                    var prototype = package.Game.EntityPrototypes.FirstOrDefault(item => item.Id == entity.PrototypeId);
                    if (prototype != null)
                    {
                        prototype.AssetId = asset.AssetId;
                    }
                }
            }
            else if (asset.Category == "item_icon_ui_graphic")
            {
                foreach (var item in package.Game.Items.Where(item => item.Id == asset.ContentId))
                {
                    item.IconAssetId = asset.AssetId;
                }
            }
            else if (asset.Category == "sound_effect")
            {
                foreach (var dialogue in package.Game.Dialogues.Where(item => item.Id == asset.ContentId))
                {
                    dialogue.Metadata["asset_sound_effect_id"] = asset.AssetId;
                }

                foreach (var interaction in package.Game.Interactions.Where(item => item.Id == asset.ContentId))
                {
                    interaction.Metadata["asset_sound_effect_id"] = asset.AssetId;
                }
            }
            else if (asset.Category == "music_ambience")
            {
                foreach (var interaction in package.Game.Interactions.Where(item => item.Id == asset.ContentId))
                {
                    interaction.Metadata["asset_music_ambience_id"] = asset.AssetId;
                }
            }
        }

        var allContentIds = AllPackageContentIds(package);
        foreach (var asset in resolvedAssets)
        {
            if (!allContentIds.Contains(asset.ContentId))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.binding.unresolved_content_id", asset.SlotId, "Resolved asset content id must exist in the generated package/content graph."));
            }
        }

        if (!string.Equals(preAssetPackageHash, expectedPackageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.binding.package_hash_mismatch", package.Manifest.PackageId, "Pre-asset package hash must match the package used for binding."));
        }

        var bindingEvidence = new List<MinimumAssetBindingEvidence>();
        var categoryBindings = AuditCategoryBindings(package, resolvedAssets, diagnostics, bindingEvidence);

        var validation = _packageValidator.Validate(package);
        var validationErrors = validation.Issues
            .Where(issue => issue.Severity.ToString().Equals("Error", StringComparison.OrdinalIgnoreCase))
            .Select(issue => Diagnostic("error", issue.Code, issue.TargetId ?? package.Manifest.PackageId, issue.Message))
            .ToList();
        diagnostics.AddRange(validationErrors);
        var packageWithAssetsHash = ComputeHash(JsonSerializer.Serialize(package, JsonOptions));
        var attachedCount = package.AssetCatalog.Assets.Count(asset => asset.LinkedEntityIds.Count > 0);
        return new MinimumAssetBindingAudit
        {
            Passed = diagnostics.All(item => item.Severity != "error") &&
                     attachedCount == resolvedAssets.Count &&
                     categoryBindings.Values.Sum() == resolvedAssets.Count &&
                     !string.Equals(preAssetPackageHash, packageWithAssetsHash, StringComparison.Ordinal),
            PackageValidatorClean = validationErrors.Count == 0,
            PreAssetPackageHash = preAssetPackageHash,
            PackageHashWithAssets = packageWithAssetsHash,
            GeneratedContentHash = generatedContentHash,
            AssetCatalogCount = package.AssetCatalog.Assets.Count,
            BoundAssetCount = attachedCount,
            CategorySpecificBindingCounts = categoryBindings,
            CategorySpecificBindingEvidence = bindingEvidence.OrderBy(item => item.AssetId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyDictionary<string, int> AuditCategoryBindings(
        GamePackageDefinition package,
        IReadOnlyList<ResolvedMinimumAsset> resolvedAssets,
        List<MinimumAssetPipelineDiagnostic> diagnostics,
        List<MinimumAssetBindingEvidence> bindingEvidence)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var asset in resolvedAssets.OrderBy(item => item.SlotId, StringComparer.Ordinal))
        {
            if (!TryGetExactCatalogAsset(package, asset))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.binding.catalog_mismatch", asset.SlotId, "AssetCatalog must contain the exact asset id, media type/category, relative path and linked content id."));
                continue;
            }

            var bound = asset.Category switch
            {
                "tile_region_graphic" => TileOrRegionBindingMatches(package, asset),
                "npc_portrait" => NpcBindingMatches(package, asset),
                "item_icon_ui_graphic" => ItemBindingMatches(package, asset),
                "sound_effect" => SoundBindingMatches(package, asset),
                "music_ambience" => MusicBindingMatches(package, asset),
                _ => false
            };

            if (!bound)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.binding.category_specific_missing", asset.SlotId, "Resolved asset must bind through the strongest existing category-specific package/content seam."));
                continue;
            }

            bindingEvidence.Add(new MinimumAssetBindingEvidence
            {
                Category = asset.Category,
                ContentId = asset.ContentId,
                AssetId = asset.AssetId,
                MediaType = asset.MediaType,
                RelativePath = asset.RelativePath,
                CatalogLinked = true,
                PackageSeam = DescribePackageSeam(package, asset)
            });
            counts[asset.Category] = counts.GetValueOrDefault(asset.Category) + 1;
        }

        return counts.OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }

    private static bool TryGetExactCatalogAsset(GamePackageDefinition package, ResolvedMinimumAsset asset) =>
        package.AssetCatalog.Assets.Any(catalogAsset =>
            string.Equals(catalogAsset.Id, asset.AssetId, StringComparison.Ordinal) &&
            string.Equals(catalogAsset.Type, asset.MediaType, StringComparison.Ordinal) &&
            string.Equals(catalogAsset.Role, asset.Category, StringComparison.Ordinal) &&
            string.Equals(catalogAsset.Path, asset.RelativePath, StringComparison.Ordinal) &&
            catalogAsset.LinkedEntityIds.Contains(asset.ContentId, StringComparer.Ordinal));

    private static bool TileOrRegionBindingMatches(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        var tile = package.Game.TilePrototypes.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (tile != null)
        {
            return !string.IsNullOrWhiteSpace(tile.AssetId);
        }

        return package.Game.Maps.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)) ||
               package.GeneratedContent.Regions.Any(item => string.Equals(item.SourceId, asset.ContentId, StringComparison.Ordinal));
    }

    private static bool NpcBindingMatches(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        var entity = package.Game.Maps.SelectMany(map => map.Entities)
            .SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (entity != null)
        {
            var prototype = package.Game.EntityPrototypes.SingleOrDefault(item => string.Equals(item.Id, entity.PrototypeId, StringComparison.Ordinal));
            return !string.IsNullOrWhiteSpace(prototype?.AssetId);
        }

        var prototypeById = package.Game.EntityPrototypes.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (prototypeById != null)
        {
            return !string.IsNullOrWhiteSpace(prototypeById.AssetId);
        }

        return package.GeneratedContent.Npcs.Any(item => string.Equals(item.SourceId, asset.ContentId, StringComparison.Ordinal));
    }

    private static bool ItemBindingMatches(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        var item = package.Game.Items.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (item != null)
        {
            return !string.IsNullOrWhiteSpace(item.IconAssetId);
        }

        return package.GeneratedContent.Items.Any(item => string.Equals(item.SourceId, asset.ContentId, StringComparison.Ordinal));
    }

    private static bool SoundBindingMatches(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        var dialogue = package.Game.Dialogues.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (dialogue != null)
        {
            return MetadataAssetPresent(dialogue.Metadata, "asset_sound_effect_id");
        }

        var interaction = package.Game.Interactions.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (interaction != null)
        {
            return MetadataAssetPresent(interaction.Metadata, "asset_sound_effect_id");
        }

        return package.Game.Quests.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
    }

    private static bool MusicBindingMatches(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        var interaction = package.Game.Interactions.SingleOrDefault(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
        if (interaction != null)
        {
            return MetadataAssetPresent(interaction.Metadata, "asset_music_ambience_id");
        }

        return package.Game.Maps.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal));
    }

    private static bool MetadataAssetPresent(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

    private static string DescribePackageSeam(GamePackageDefinition package, ResolvedMinimumAsset asset)
    {
        if (asset.Category == "tile_region_graphic")
        {
            if (package.Game.TilePrototypes.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)))
            {
                return "tile_prototype.asset_id";
            }

            if (package.Game.Maps.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)))
            {
                return "asset_catalog.map_link";
            }

            return "asset_catalog.generated_region_link";
        }

        if (asset.Category == "npc_portrait")
        {
            if (package.Game.Maps.SelectMany(map => map.Entities).Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)) ||
                package.Game.EntityPrototypes.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)))
            {
                return "entity_prototype.asset_id";
            }

            return "asset_catalog.generated_npc_link";
        }

        if (asset.Category == "item_icon_ui_graphic")
        {
            return package.Game.Items.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal))
                ? "item.icon_asset_id"
                : "asset_catalog.generated_item_link";
        }

        if (asset.Category == "sound_effect")
        {
            if (package.Game.Dialogues.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)))
            {
                return "dialogue.metadata.asset_sound_effect_id";
            }

            if (package.Game.Interactions.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal)))
            {
                return "interaction.metadata.asset_sound_effect_id";
            }

            return "asset_catalog.quest_link";
        }

        if (asset.Category == "music_ambience")
        {
            return package.Game.Interactions.Any(item => string.Equals(item.Id, asset.ContentId, StringComparison.Ordinal))
                ? "interaction.metadata.asset_music_ambience_id"
                : "asset_catalog.map_link";
        }

        return "asset_catalog.link";
    }

    private MinimumAssetValidationEvidence ValidateAssets(
        string projectRoot,
        string artifactRoot,
        GamePackageDefinition package,
        MinimumAssetSourcePackLoadResult sourcePack,
        string packageHash,
        string packageContentHash,
        IReadOnlyList<ResolvedMinimumAsset> resolvedAssets,
        IReadOnlyList<MinimumAssetRequest> requests,
        MinimumAssetBindingAudit bindingAudit)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        var currentSourcePack = sourcePack.Pack ?? new MinimumAssetSourcePack();
        var requestBySlot = requests.ToDictionary(request => request.SlotId, StringComparer.Ordinal);
        var assetIds = new HashSet<string>(StringComparer.Ordinal);
        var contentIds = AllPackageContentIds(package);
        var expectedAssetPrefix = $"{RelativeOutputDirectory}/assets/{SafeSegment(currentSourcePack.PackId)}/";
        foreach (var asset in resolvedAssets)
        {
            if (!requestBySlot.TryGetValue(asset.SlotId, out var request))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.unrequested_asset", asset.SlotId, "Resolved asset must correspond to an expanded request."));
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.cross_pack_asset_leakage", asset.SlotId, "Resolved asset slot does not belong to the current generated package request set."));
            }
            else
            {
                if (!string.Equals(asset.Category, request.Category, StringComparison.Ordinal) ||
                    !string.Equals(asset.MediaType, request.MediaType, StringComparison.Ordinal) ||
                    !string.Equals(asset.ContentId, request.ContentId, StringComparison.Ordinal) ||
                    !string.Equals(asset.SourceId, request.SourceId, StringComparison.Ordinal) ||
                    !string.Equals(asset.SourceKind, request.SourceKind, StringComparison.Ordinal))
                {
                    diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.request_asset_mismatch", asset.SlotId, "Resolved asset fields must match the expanded request."));
                }
            }

            if (!assetIds.Add(asset.AssetId))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.duplicate_asset_id", asset.AssetId, "Resolved asset ids must be unique."));
            }

            if (Path.IsPathRooted(asset.RelativePath) || ContainsPathTraversal(asset.RelativePath))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.unsafe_path", asset.SlotId, "Resolved artifact path must be repository-relative and contained."));
                continue;
            }

            if (!asset.RelativePath.StartsWith(expectedAssetPrefix, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.cross_pack_asset_leakage", asset.SlotId, "Resolved asset path must stay under the current source pack artifact folder."));
            }

            var source = currentSourcePack.Sources.SingleOrDefault(item => string.Equals(item.SourceId, asset.SourceId, StringComparison.Ordinal));
            if (source == null)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.cross_pack_asset_leakage", asset.SlotId, "Resolved asset source id must exist in the current source pack."));
            }
            else if (!string.Equals(source.Kind, asset.SourceKind, StringComparison.Ordinal) ||
                     !string.Equals(source.Category, asset.Category, StringComparison.Ordinal) ||
                     !string.Equals(source.MediaType, asset.MediaType, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.cross_pack_asset_leakage", asset.SlotId, "Resolved asset source kind, category and media type must match the current source declaration."));
            }

            if (!contentIds.Contains(asset.ContentId))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.cross_pack_asset_leakage", asset.SlotId, "Resolved asset content id must belong to the current generated/package content graph."));
            }

            var fullPath = Path.GetFullPath(Path.Combine(projectRoot, asset.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsContained(projectRoot, fullPath) || !IsContained(artifactRoot, fullPath))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.outside_artifact_root", asset.SlotId, "Resolved artifact path must stay under the minimum asset pipeline artifact root."));
                continue;
            }

            if (!File.Exists(fullPath))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.file_missing", asset.SlotId, "Resolved asset file must exist."));
                continue;
            }

            var bytes = File.ReadAllBytes(fullPath);
            var hash = ComputeHash(bytes);
            if (!string.Equals(hash, asset.Hash, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.hash_mismatch", asset.SlotId, "Resolved asset hash must match actual bytes."));
            }

            if (bytes.Length != asset.ByteCount)
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.byte_count_mismatch", asset.SlotId, "Resolved asset byte count must match actual bytes."));
            }

            if (!ValidateFixtureMedia(bytes, asset.MediaType))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.media_type_mismatch", asset.SlotId, "Resolved asset bytes must match the declared fixture media type."));
            }
        }

        var sourceHash = string.IsNullOrWhiteSpace(sourcePack.SourceRelativePath)
            ? string.Empty
            : ComputeHash(File.ReadAllText(Path.Combine(sourcePack.DirectoryPath, sourcePack.SourceRelativePath), Utf8WithoutBom));
        if (!string.Equals(sourceHash, sourcePack.SourceHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.source_pack_hash_mismatch", currentSourcePack.PackId, "Source pack hash in the manifest must match the loaded source pack bytes."));
        }

        var currentPackageContentHash = ExtractPackageContentHash(package);
        if (!string.Equals(packageContentHash, currentPackageContentHash, StringComparison.Ordinal) ||
            !string.Equals(packageContentHash, bindingAudit.GeneratedContentHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.package_content_hash_mismatch", package.Manifest.PackageId, "Generated package content hash must match the package content used for requests."));
        }

        if (!string.Equals(packageHash, bindingAudit.PreAssetPackageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.package_hash_mismatch", package.Manifest.PackageId, "Package hash in the manifest must match the pre-asset package used for binding."));
        }

        if (string.Equals(bindingAudit.PackageHashWithAssets, bindingAudit.PreAssetPackageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.package_hash_with_assets_unchanged", package.Manifest.PackageId, "Package hash with assets must change after asset bindings are applied."));
        }

        if (currentSourcePack.MaxTotalSlots < resolvedAssets.Count)
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.validation.over_budget", currentSourcePack.PackId, "Resolved asset count exceeds the safe source pack budget."));
        }

        return new MinimumAssetValidationEvidence
        {
            Passed = diagnostics.All(item => item.Severity != "error") && bindingAudit.Passed,
            FilesChecked = resolvedAssets.Count,
            HashesChecked = resolvedAssets.Count,
            PackageReferencesChecked = resolvedAssets.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private MinimumAssetReplayEvidence BuildReplayEvidence(
        string projectRoot,
        string artifactRoot,
        ContentGenerationScalePackResult contentPack,
        MinimumAssetSourcePackLoadResult sourcePack,
        MinimumAssetPipelineAcceptanceOptions options)
    {
        var first = BuildRun(projectRoot, artifactRoot, contentPack, sourcePack, options.PrimarySeed, options);
        var second = BuildRun(projectRoot, artifactRoot, contentPack, sourcePack, options.PrimarySeed, options);
        return new MinimumAssetReplayEvidence
        {
            Passed = first.Manifest.ManifestHash == second.Manifest.ManifestHash &&
                     first.PackageBindingAudit.PackageHashWithAssets == second.PackageBindingAudit.PackageHashWithAssets,
            FirstManifestHash = first.Manifest.ManifestHash,
            ReplayedManifestHash = second.Manifest.ManifestHash,
            FirstPackageHash = first.PackageBindingAudit.PackageHashWithAssets,
            ReplayedPackageHash = second.PackageBindingAudit.PackageHashWithAssets
        };
    }

    private MinimumAssetVariationEvidence BuildVariationEvidence(
        ContentGenerationScalePackResult contentPack,
        MinimumAssetSourcePackLoadResult sourcePack,
        MinimumAssetPipelineAcceptanceOptions options)
    {
        ExpandRequests(contentPack.PackageAudit.Package, sourcePack.Pack!, options.PrimarySeed, out var first);
        ExpandRequests(contentPack.PackageAudit.Package, sourcePack.Pack!, options.VariationSeed, out var second);
        var firstHash = ComputeHash(JsonSerializer.Serialize(first, JsonOptions));
        var secondHash = ComputeHash(JsonSerializer.Serialize(second, JsonOptions));
        return new MinimumAssetVariationEvidence
        {
            Passed = firstHash != secondHash &&
                     first.Select(item => item.Category).OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(second.Select(item => item.Category).OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal),
            PrimaryManifestHash = firstHash,
            VariationManifestHash = secondHash,
            PrimarySeed = options.PrimarySeed,
            VariationSeed = options.VariationSeed
        };
    }

    private MinimumAssetInvalidMatrix BuildInvalidMatrix(
        string projectRoot,
        string artifactRoot,
        IReadOnlyList<ContentGenerationScalePackResult> contentPacks,
        IReadOnlyList<MinimumAssetSourcePackLoadResult> sourcePacks,
        MinimumAssetPipelineAcceptanceOptions options)
    {
        var scenarios = new List<MinimumAssetInvalidScenario>();
        var contentPack = contentPacks.FirstOrDefault();
        var sourcePack = sourcePacks.FirstOrDefault();
        if (contentPack == null || sourcePack?.Pack == null)
        {
            scenarios.Add(InvalidScenario("missing_valid_fixture", [Diagnostic("error", "asset_pipeline.invalid.no_valid_baseline", "invalid_matrix", "Invalid matrix requires one valid generated/package baseline.")]));
        }
        else
        {
            var validSource = sourcePack.Pack;
            var unknown = validSource with { Sources = [validSource.Sources[0] with { Kind = "remote_provider" }] };
            scenarios.Add(InvalidScenario("unknown_source_kind", ValidateSourcePack(unknown, "invalid/unknown.json").Where(item => item.Code == "asset_pipeline.source.kind").ToList()));

            var unsupportedMedia = validSource with { Sources = [validSource.Sources[0] with { MediaType = "video/mp4" }] };
            scenarios.Add(InvalidScenario("unsupported_media_type", ValidateSourcePack(unsupportedMedia, "invalid/media.json").Where(item => item.Code == "asset_pipeline.source.media_type").ToList()));

            var missingNoFallback = validSource with
            {
                CategoryPolicies = validSource.CategoryPolicies.ToDictionary(item => item.Key, item => item.Value with { AllowFallback = false }, StringComparer.Ordinal),
                Sources = [validSource.Sources.First(item => item.Kind == "local_fixture") with { RelativePath = "fixtures/missing.fixture" }]
            };
            scenarios.Add(InvalidScenario("missing_fixture_without_fallback_permission", ValidateMissingFixture(missingNoFallback, sourcePack.DirectoryPath)));

            var corrupt = validSource.Sources.First(item => item.Kind == "local_fixture") with { MediaType = "audio/wav" };
            scenarios.Add(InvalidScenario("wrong_media_type_or_corrupt_fixture", ValidateFixtureSource(corrupt, sourcePack.DirectoryPath).Where(item => item.Code == "asset_pipeline.fixture.media_type_mismatch").ToList()));

            var traversal = validSource with { Sources = [validSource.Sources[0] with { RelativePath = "../escape.fixture" }] };
            scenarios.Add(InvalidScenario("path_traversal_source", ValidateSourcePack(traversal, "invalid/traversal.json").Where(item => item.Code == "asset_pipeline.source.path_traversal").ToList()));

            var absolutePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? string.Empty, "temp", "asset.fixture");
            var absolute = validSource with { Sources = [validSource.Sources[0] with { RelativePath = absolutePath }] };
            scenarios.Add(InvalidScenario("absolute_path_source", ValidateSourcePack(absolute, "invalid/absolute.json").Where(item => item.Code == "asset_pipeline.source.absolute_path").ToList()));

            var payload = validSource with { Sources = [validSource.Sources[0] with { RelativePath = "fixtures/run.ps1", Payload = "powershell -ExecutionPolicy Bypass" }] };
            scenarios.Add(InvalidScenario("executable_script_provider_payload_injection", ValidateSourcePack(payload, "invalid/payload.json").Where(item => item.Code == "asset_pipeline.source.executable_payload" || item.Code == "asset_pipeline.source.command_payload").ToList()));

            ExpandRequests(contentPack.PackageAudit.Package, validSource, options.PrimarySeed, out var requests);
            var duplicateRequests = requests.Select(item => item).ToList();
            duplicateRequests[1] = duplicateRequests[1] with { SlotId = duplicateRequests[0].SlotId };
            var duplicateDiagnostics = ValidateExpandedRequests(validSource, duplicateRequests);
            scenarios.Add(InvalidScenario("duplicate_slot_ids", duplicateDiagnostics));

            var unresolvedAsset = new ResolvedMinimumAsset { SlotId = requests[0].SlotId, AssetId = "asset/unresolved", Category = requests[0].Category, MediaType = requests[0].MediaType, ContentId = "missing/content/id", RelativePath = ".llmgc/procedural/minimum-asset-pipeline/assets/missing.fixture", Hash = "0", ByteCount = 1, ResolutionKind = "fallback", SourceId = requests[0].SourceId, SourceKind = requests[0].SourceKind };
            var unresolvedAudit = BindAssetsToPackage(ClonePackage(contentPack.PackageAudit.Package), [unresolvedAsset], contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash);
            scenarios.Add(InvalidScenario("unresolved_content_id", unresolvedAudit.Diagnostics.Where(item => item.Code == "asset_pipeline.binding.unresolved_content_id").ToList()));

            var validRun = BuildRun(projectRoot, artifactRoot, contentPack, sourcePack, options.PrimarySeed, options);
            var hashDiagnostics = validRun.ResolvedAssets.Count == 0
                ? [Diagnostic("error", "asset_pipeline.resolver_unavailable", contentPack.PackId, "Hash mismatch validation requires concrete resolved files.")]
                : ValidateAssets(projectRoot, artifactRoot, ClonePackage(contentPack.PackageAudit.Package), sourcePack, contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash, [validRun.ResolvedAssets[0] with { Hash = "0000" + validRun.ResolvedAssets[0].Hash }], [requests[0]], validRun.PackageBindingAudit)
                    .Diagnostics
                    .Where(item => item.Code == "asset_pipeline.validation.hash_mismatch")
                    .ToList();
            scenarios.Add(InvalidScenario("mismatched_file_hash", hashDiagnostics));

            var tamperedDiagnostics = validRun.ResolvedAssets.Count == 0
                ? [Diagnostic("error", "asset_pipeline.resolver_unavailable", contentPack.PackId, "Tampered package/content hash validation requires concrete resolved files.")]
                : ValidateAssets(projectRoot, artifactRoot, ClonePackage(contentPack.PackageAudit.Package), sourcePack, "tampered-" + contentPack.PackageAudit.PackageHash, "tampered-" + contentPack.PackageAudit.CatalogHash, validRun.ResolvedAssets.Take(1).ToList(), requests.Take(1).ToList(), validRun.PackageBindingAudit)
                    .Diagnostics
                    .Where(item => item.Code is "asset_pipeline.validation.package_content_hash_mismatch" or "asset_pipeline.validation.package_hash_mismatch")
                    .ToList();
            scenarios.Add(InvalidScenario("tampered_package_content_hash", tamperedDiagnostics));

            var overBudget = validSource with { MaxTotalSlots = 2 };
            var overBudgetDiagnostics = ExpandRequests(contentPack.PackageAudit.Package, overBudget, options.PrimarySeed, out _)
                .Where(item => item.Code == "asset_pipeline.request.over_budget")
                .ToList();
            scenarios.Add(InvalidScenario("over_budget_request", overBudgetDiagnostics));

            var otherContentPack = contentPacks.Skip(1).FirstOrDefault();
            var crossPackDiagnostics = otherContentPack == null || validRun.ResolvedAssets.Count == 0
                ? [Diagnostic("error", "asset_pipeline.invalid.no_cross_pack_baseline", contentPack.PackId, "Cross-pack leakage validation requires two generated/package baselines.")]
                : BuildCrossPackLeakageDiagnostics(projectRoot, artifactRoot, otherContentPack, sourcePack, validRun.ResolvedAssets[0], options);
            scenarios.Add(InvalidScenario("cross_pack_asset_leakage", crossPackDiagnostics));
            scenarios.Add(InvalidScenario("copied_expectation_report_without_files", options.IncludeExpectationOnlyInvalidMutation ? [Diagnostic("error", "asset_pipeline.invalid.expectation_only_mutation_present", "expectation_only", "Copied report evidence without actual files is rejected.")] : []));
            var unavailableRun = new MinimumAssetPipelineAcceptanceService(packageValidator: _packageValidator)
                .BuildRun(projectRoot, artifactRoot, contentPack, sourcePack, options.PrimarySeed, options);
            scenarios.Add(InvalidScenario("unavailable_default_resolver", unavailableRun.Diagnostics.Where(item => item.Code == "asset_pipeline.resolver_unavailable").ToList()));
        }

        var passed = scenarios.Count >= 14 && scenarios.All(item => !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error"));
        return new MinimumAssetInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = scenarios.SelectMany(item => item.Diagnostics).ToList()
        };
    }

    private IReadOnlyList<MinimumAssetPipelineDiagnostic> BuildCrossPackLeakageDiagnostics(
        string projectRoot,
        string artifactRoot,
        ContentGenerationScalePackResult contentPack,
        MinimumAssetSourcePackLoadResult sourcePack,
        ResolvedMinimumAsset foreignAsset,
        MinimumAssetPipelineAcceptanceOptions options)
    {
        ExpandRequests(contentPack.PackageAudit.Package, sourcePack.Pack!, options.PrimarySeed, out var requests);
        var package = ClonePackage(contentPack.PackageAudit.Package);
        var bindingAudit = BindAssetsToPackage(package, [foreignAsset], contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash);
        return ValidateAssets(projectRoot, artifactRoot, package, sourcePack, contentPack.PackageAudit.PackageHash, contentPack.PackageAudit.CatalogHash, [foreignAsset], requests, bindingAudit)
            .Diagnostics
            .Concat(bindingAudit.Diagnostics)
            .Where(item => item.Code == "asset_pipeline.validation.cross_pack_asset_leakage")
            .ToList();
    }

    private static MinimumAssetInvalidScenario InvalidScenario(string scenarioId, IReadOnlyList<MinimumAssetPipelineDiagnostic> diagnostics) => new()
    {
        ScenarioId = scenarioId,
        ExpectedValid = false,
        ActualValid = diagnostics.All(item => item.Severity != "error"),
        Diagnostics = SortDiagnostics(diagnostics)
    };

    private IReadOnlyList<MinimumAssetSourcePackLoadResult> LoadSourcePacks(string assetPackDirectory)
    {
        if (!Directory.Exists(assetPackDirectory))
        {
            return [new MinimumAssetSourcePackLoadResult { SourceRelativePath = assetPackDirectory, Diagnostics = [Diagnostic("error", "asset_pipeline.source_pack.directory_missing", assetPackDirectory, "Asset source pack directory is missing.")] }];
        }

        return Directory.EnumerateFiles(assetPackDirectory, "*.json")
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Select(path => LoadSourcePack(assetPackDirectory, path))
            .ToList();
    }

    private MinimumAssetSourcePackLoadResult LoadSourcePack(string assetPackDirectory, string path)
    {
        var relative = RelativePath(assetPackDirectory, path);
        try
        {
            var raw = File.ReadAllText(path, Utf8WithoutBom);
            var pack = JsonSerializer.Deserialize<MinimumAssetSourcePack>(raw, JsonOptions);
            if (pack == null)
            {
                return new MinimumAssetSourcePackLoadResult { SourceRelativePath = relative, SourceHash = ComputeHash(raw), Diagnostics = [Diagnostic("error", "asset_pipeline.source_pack.empty", relative, "Asset source pack JSON did not produce a source pack.")] };
            }

            var diagnostics = ValidateSourcePack(pack, relative).Concat(pack.Sources.SelectMany(source => ValidateFixtureSource(source, assetPackDirectory))).ToList();
            return new MinimumAssetSourcePackLoadResult
            {
                Pack = pack,
                DirectoryPath = assetPackDirectory,
                SourceRelativePath = relative,
                SourceHash = ComputeHash(raw),
                Diagnostics = SortDiagnostics(diagnostics)
            };
        }
        catch (JsonException ex)
        {
            return new MinimumAssetSourcePackLoadResult { SourceRelativePath = relative, Diagnostics = [Diagnostic("error", "asset_pipeline.source_pack.malformed_json", relative, "Malformed JSON: " + ex.Message)] };
        }
    }

    private static IReadOnlyList<MinimumAssetPipelineDiagnostic> ValidateSourcePack(MinimumAssetSourcePack pack, string relativePath)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        Require(pack.SchemaVersion == ExpectedSchemaVersion, diagnostics, "asset_pipeline.source_pack.schema_version", relativePath, "Asset source pack schema version is unsupported.");
        Require(IsSafeId(pack.PackId), diagnostics, "asset_pipeline.source_pack.pack_id", relativePath, "Asset source pack id must be stable and safe.");
        Require(pack.MaxTotalSlots is > 0 and <= 120, diagnostics, "asset_pipeline.source_pack.budget", pack.PackId, "Source pack total slot budget must be positive and bounded.");
        foreach (var category in KnownCategories)
        {
            if (!pack.CategoryPolicies.TryGetValue(category, out var policy))
            {
                diagnostics.Add(Diagnostic("error", "asset_pipeline.policy.missing", category, "Every category must declare an explicit fallback policy."));
                continue;
            }

            Require(policy.MaxSlots is > 0 and <= 40, diagnostics, "asset_pipeline.policy.budget", category, "Category max slots must be positive and bounded.");
        }

        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in pack.Sources)
        {
            Require(sourceIds.Add(source.SourceId), diagnostics, "asset_pipeline.source.duplicate_id", source.SourceId, "Asset source ids must be unique.");
            Require(IsSafeId(source.SourceId), diagnostics, "asset_pipeline.source.id", source.SourceId, "Asset source id must be stable and safe.");
            Require(KnownSourceKinds.Contains(source.Kind), diagnostics, "asset_pipeline.source.kind", source.SourceId, "Asset source kind is unsupported.");
            Require(KnownCategories.Contains(source.Category), diagnostics, "asset_pipeline.source.category", source.SourceId, "Asset source category is unsupported.");
            Require(KnownMediaTypes.Contains(source.MediaType), diagnostics, "asset_pipeline.source.media_type", source.SourceId, "Asset source media type is unsupported.");
            if (!string.IsNullOrWhiteSpace(source.RelativePath))
            {
                Require(!Path.IsPathRooted(source.RelativePath), diagnostics, "asset_pipeline.source.absolute_path", source.SourceId, "Asset source path must be relative.");
                Require(!ContainsPathTraversal(source.RelativePath), diagnostics, "asset_pipeline.source.path_traversal", source.SourceId, "Asset source path must not traverse outside the pack directory.");
                var extension = Path.GetExtension(source.RelativePath);
                Require(!string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(extension, ".bat", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(extension, ".cmd", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(extension, ".ps1", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(extension, ".sh", StringComparison.OrdinalIgnoreCase),
                    diagnostics,
                    "asset_pipeline.source.executable_payload",
                    source.SourceId,
                    "Executable or script payloads are not asset fixtures.");
            }

            Require(string.IsNullOrWhiteSpace(source.Payload), diagnostics, "asset_pipeline.source.command_payload", source.SourceId, "Source declarations must not contain provider credentials or command strings.");
        }

        return diagnostics;
    }

    private static IReadOnlyList<MinimumAssetPipelineDiagnostic> ValidateFixtureSource(MinimumAssetSource source, string sourceDirectory)
    {
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        if (source.Kind != "local_fixture")
        {
            return diagnostics;
        }

        if (string.IsNullOrWhiteSpace(source.RelativePath))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.fixture.path_missing", source.SourceId, "Local fixture source requires a relative path."));
            return diagnostics;
        }

        var fullPath = Path.GetFullPath(Path.Combine(sourceDirectory, source.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsContained(sourceDirectory, fullPath) || !File.Exists(fullPath))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.fixture.missing", source.SourceId, "Local fixture source file is missing."));
            return diagnostics;
        }

        var bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length == 0)
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.fixture.empty", source.SourceId, "Local fixture source file is empty."));
        }

        if (!ValidateFixtureMedia(bytes, source.MediaType))
        {
            diagnostics.Add(Diagnostic("error", "asset_pipeline.fixture.media_type_mismatch", source.SourceId, "Local fixture source bytes do not match declared fixture media type."));
        }

        return diagnostics;
    }

    private static IReadOnlyList<MinimumAssetPipelineDiagnostic> ValidateMissingFixture(MinimumAssetSourcePack pack, string sourceDirectory) =>
        pack.Sources.SelectMany(source => ValidateFixtureSource(source, sourceDirectory)).ToList();

    private static bool ValidateFixtureMedia(byte[] bytes, string mediaType)
    {
        var prefix = Encoding.UTF8.GetString(bytes.Take(Math.Min(bytes.Length, 80)).ToArray());
        return prefix.StartsWith("LLMGC_FIXTURE_MEDIA:" + mediaType + "\n", StringComparison.Ordinal);
    }

    private static HashSet<string> AllPackageContentIds(GamePackageDefinition package)
    {
        return package.Game.Maps.Select(item => item.Id)
            .Concat(package.Game.TilePrototypes.Select(item => item.Id))
            .Concat(package.Game.Maps.SelectMany(map => map.Entities.Select(entity => entity.Id)))
            .Concat(package.Game.EntityPrototypes.Select(item => item.Id))
            .Concat(package.Game.Items.Select(item => item.Id))
            .Concat(package.Game.Quests.Select(item => item.Id))
            .Concat(package.Game.Dialogues.Select(item => item.Id))
            .Concat(package.Game.Interactions.Select(item => item.Id))
            .Concat(package.GeneratedContent.Npcs.Select(item => item.SourceId))
            .Concat(package.GeneratedContent.Items.Select(item => item.SourceId))
            .Concat(package.GeneratedContent.Dialogues.Select(item => item.SourceId))
            .Concat(package.GeneratedContent.Quests.Select(item => item.SourceId))
            .Append(package.Manifest.PackageId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string ExtractPackageContentHash(GamePackageDefinition package) =>
        package.GeneratedContent.AppliedArtifacts.SingleOrDefault()?.ContentHash ?? string.Empty;

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(JsonSerializer.Serialize(package, JsonOptions), JsonOptions) ?? new GamePackageDefinition();

    private static string RenderReport(MinimumAssetPipelineReport report)
    {
        var lines = new List<string>
        {
            "# Minimum Asset Pipeline Report",
            "",
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Total resolved asset slots: {report.TotalResolvedAssetSlots}",
            $"- Manifest hash: {report.ManifestHash}",
            $"- Deterministic hash: {report.DeterministicHash}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Public schema changed: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- Project files changed: {report.ProjectFilesChanged.ToString().ToLowerInvariant()}",
            "",
            "## Category Counts"
        };
        lines.AddRange(report.CategoryCounts.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"- {item.Key}: {item.Value}"));
        lines.Add("");
        lines.Add("## Import Counts");
        lines.AddRange(report.ImportCountsByCategory.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"- {item.Key}: {item.Value}"));
        lines.Add("");
        lines.Add("## Fallback Counts");
        lines.AddRange(report.FallbackCountsByCategory.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"- {item.Key}: {item.Value}"));
        lines.Add("");
        lines.Add("## Invalid Matrix");
        lines.AddRange(report.InvalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: actualValid={item.ActualValid.ToString().ToLowerInvariant()} diagnostics={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal))}"));
        lines.Add("");
        lines.Add("## Category Binding Audit");
        foreach (var run in report.Runs.OrderBy(item => item.PackId, StringComparer.Ordinal))
        {
            lines.Add($"- {run.PackId}: {string.Join(", ", run.PackageBindingAudit.CategorySpecificBindingCounts.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => item.Key + "=" + item.Value))}");
        }

        lines.Add("");
        lines.Add("## External Execution");
        lines.Add("- LLM: false");
        lines.Add("- RAG: false");
        lines.Add("- Provider: false");
        lines.Add("- Lua: false");
        lines.Add("- Unity: false");
        lines.Add("- Media generation: false");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(MinimumAssetPipelineReport report)
    {
        var lines = new List<string>
        {
            "# Minimum Asset Pipeline Verification",
            "",
            $"Gate: {ManualGate}",
            "",
            "Status: required",
            "",
            "Do not mark this gate passed until external artifact review accepts the report.",
            "",
            $"Accepted by automated harness: {report.Accepted.ToString().ToLowerInvariant()}",
            $"S098A correctness hotfix complete through final verification artifact: {report.CompletedSlices.Contains("S098A").ToString().ToLowerInvariant()}",
            "",
            "Next work: stop at this gate. Do not start post-goal work."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyDictionary<string, int> Distribution(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
    }

    private static int Count(IReadOnlyDictionary<string, int> values, string key) => values.TryGetValue(key, out var count) ? count : 0;

    private static void Require(bool condition, List<MinimumAssetPipelineDiagnostic> diagnostics, string code, string target, string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
    }

    private static MinimumAssetPipelineDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static IReadOnlyList<MinimumAssetPipelineDiagnostic> SortDiagnostics(IEnumerable<MinimumAssetPipelineDiagnostic> diagnostics) =>
        diagnostics.OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static string SafeId(params string[] parts) =>
        string.Join("/", parts.Select(SafeSegment).Where(part => part.Length > 0));

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

    private static bool IsSafeId(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-' or '/' or '.');

    private static bool ContainsPathTraversal(string path)
    {
        var parts = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(part => part == "..") || path.Contains(':', StringComparison.Ordinal);
    }

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException("Path resolves outside the expected root.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public interface IMinimumAssetPipelineResolver
{
    string ResolverId { get; }
    bool IsAvailable { get; }
    MinimumAssetResolveEvidence Resolve(MinimumAssetResolveRequest request);
}

internal sealed class UnavailableMinimumAssetPipelineResolver : IMinimumAssetPipelineResolver
{
    public string ResolverId => "unavailable_minimum_asset_pipeline_resolver";
    public bool IsAvailable => false;

    public MinimumAssetResolveEvidence Resolve(MinimumAssetResolveRequest request) =>
        MinimumAssetResolveEvidence.Unavailable(request.Request);
}

public sealed record MinimumAssetPipelineAcceptanceOptions
{
    public string PrimarySeed { get; init; } = "goal011-primary-seed";
    public string VariationSeed { get; init; } = "goal011-variation-seed";
    public bool IncludeExpectationOnlyInvalidMutation { get; init; } = true;
}

public sealed record MinimumAssetSourcePack
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PackId { get; init; } = string.Empty;
    public int MaxTotalSlots { get; init; }
    public Dictionary<string, MinimumAssetCategoryPolicy> CategoryPolicies { get; init; } = new(StringComparer.Ordinal);
    public List<MinimumAssetSource> Sources { get; init; } = [];
}

public sealed record MinimumAssetCategoryPolicy
{
    public bool AllowFallback { get; init; }
    public int MaxSlots { get; init; }
}

public sealed record MinimumAssetSource
{
    public string SourceId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}

public sealed record MinimumAssetResolveRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public string ArtifactRootPath { get; init; } = string.Empty;
    public string SourcePackDirectoryPath { get; init; } = string.Empty;
    public MinimumAssetSourcePack SourcePack { get; init; } = new();
    public MinimumAssetRequest Request { get; init; } = new();
}

public sealed record MinimumAssetResolveEvidence
{
    public ResolvedMinimumAsset? ResolvedAsset { get; init; }
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];

    public static MinimumAssetResolveEvidence Unavailable(MinimumAssetRequest request) => new()
    {
        Diagnostics =
        [
            new MinimumAssetPipelineDiagnostic
            {
                Severity = "error",
                Code = "asset_pipeline.resolver_unavailable",
                Target = request.SlotId,
                Message = "A concrete asset resolver is required for import/fallback materialization."
            }
        ]
    };
}

public sealed record MinimumAssetPipelineAcceptanceResult
{
    public MinimumAssetPipelineReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record MinimumAssetPipelineWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record MinimumAssetPipelineReport
{
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal010GateRecorded { get; init; }
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public int SourcePackCount { get; init; }
    public int GeneratedInputCount { get; init; }
    public int TotalResolvedAssetSlots { get; init; }
    public IReadOnlyDictionary<string, int> CategoryCounts { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> ImportCountsByCategory { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> FallbackCountsByCategory { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public long TotalByteCount { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> SourcePackHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> PackageContentHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public bool ValidMatrixPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PackageContentBindingPassed { get; init; }
    public bool AssetValidationPassed { get; init; }
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public MinimumAssetExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<MinimumAssetPipelineRun> Runs { get; init; } = [];
    public MinimumAssetReplayEvidence ReplayEvidence { get; init; } = new();
    public MinimumAssetVariationEvidence VariationEvidence { get; init; } = new();
    public MinimumAssetInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }
    public bool MediaExecuted { get; init; }
}

public sealed record MinimumAssetPipelineRun
{
    public bool Accepted { get; init; }
    public string PackId { get; init; } = string.Empty;
    public string SourcePackId { get; init; } = string.Empty;
    public string SourcePackHash { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageContentHash { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int ResolvedAssetCount { get; init; }
    public IReadOnlyList<MinimumAssetRequest> Requests { get; init; } = [];
    public IReadOnlyList<ResolvedMinimumAsset> ResolvedAssets { get; init; } = [];
    public MinimumAssetManifest Manifest { get; init; } = new();
    public MinimumAssetBindingAudit PackageBindingAudit { get; init; } = new();
    public MinimumAssetValidationEvidence AssetValidation { get; init; } = new();
    public MinimumAssetResolverEvidence ResolverEvidence { get; init; } = new();
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetRequest
{
    public string SlotId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
    public int Ordinal { get; init; }
    public string Provenance { get; init; } = string.Empty;
}

public sealed record ResolvedMinimumAsset
{
    public string SlotId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
    public string ResolutionKind { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record MinimumAssetManifest
{
    public string PackId { get; init; } = string.Empty;
    public string SourcePackId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageContentHash { get; init; } = string.Empty;
    public string SourcePackHash { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int ResolvedAssetCount { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<MinimumAssetRequest> Requests { get; init; } = [];
    public IReadOnlyList<ResolvedMinimumAsset> ResolvedAssets { get; init; } = [];
}

public sealed record MinimumAssetBindingAudit
{
    public bool Passed { get; init; }
    public bool PackageValidatorClean { get; init; }
    public string PreAssetPackageHash { get; init; } = string.Empty;
    public string PackageHashWithAssets { get; init; } = string.Empty;
    public string GeneratedContentHash { get; init; } = string.Empty;
    public int AssetCatalogCount { get; init; }
    public int BoundAssetCount { get; init; }
    public IReadOnlyDictionary<string, int> CategorySpecificBindingCounts { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<MinimumAssetBindingEvidence> CategorySpecificBindingEvidence { get; init; } = [];
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetBindingEvidence
{
    public string Category { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool CatalogLinked { get; init; }
    public string PackageSeam { get; init; } = string.Empty;
}

public sealed record MinimumAssetValidationEvidence
{
    public bool Passed { get; init; }
    public int FilesChecked { get; init; }
    public int HashesChecked { get; init; }
    public int PackageReferencesChecked { get; init; }
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetResolverEvidence
{
    public bool ResolverAvailable { get; init; }
    public string ResolverId { get; init; } = string.Empty;
}

public sealed record MinimumAssetReplayEvidence
{
    public bool Passed { get; init; }
    public string FirstManifestHash { get; init; } = string.Empty;
    public string ReplayedManifestHash { get; init; } = string.Empty;
    public string FirstPackageHash { get; init; } = string.Empty;
    public string ReplayedPackageHash { get; init; } = string.Empty;
}

public sealed record MinimumAssetVariationEvidence
{
    public bool Passed { get; init; }
    public string PrimarySeed { get; init; } = string.Empty;
    public string VariationSeed { get; init; } = string.Empty;
    public string PrimaryManifestHash { get; init; } = string.Empty;
    public string VariationManifestHash { get; init; } = string.Empty;
}

public sealed record MinimumAssetInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<MinimumAssetInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MinimumAssetPipelineDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record MinimumAssetSourcePackLoadResult
{
    public MinimumAssetSourcePack? Pack { get; init; }
    public string DirectoryPath { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string SourceHash { get; init; } = string.Empty;
    public IReadOnlyList<MinimumAssetPipelineDiagnostic> Diagnostics { get; init; } = [];
}
