using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class SeededGeneratedProjectSourceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly IReadOnlySet<string> ExactV1SourceProperties = new HashSet<string>(StringComparer.Ordinal)
    {
        "schemaVersion", "creationKind", "seed", "mode", "presetId", "styleHintIds", "variantIds",
        "mechanicsProfileId", "planId", "planSha256", "rulePackId", "rulePackSha256",
        "tinyLoopStateSha256", "generatedMvpPackageSha256", "generatedOverlaySha256",
        "generatedBasePackageSha256", "goal142BaselinePackageSha256", "generatedStartMapId",
        "counts", "tinyLoop", "sidecarSha256"
    };

    private static readonly IReadOnlySet<string> ExactV2SourceProperties = new HashSet<string>(StringComparer.Ordinal)
    {
        "schemaVersion", "creationKind", "generationRequest", "resolvedGenerationOptions",
        "mechanicsProfileId", "planId", "planSha256", "rulePackId", "rulePackSha256",
        "tinyLoopStateSha256", "generatedMvpPackageSha256", "generatedOverlaySha256",
        "generatedBasePackageSha256", "goal142BaselinePackageSha256", "generatedStartMapId",
        "counts", "tinyLoop", "sidecarSha256"
    };

    private static readonly IReadOnlySet<string> ExactRequestProperties = new HashSet<string>(StringComparer.Ordinal)
    {
        "seed", "mode", "presetId", "compactStyleHintIds", "selectedVariantIds"
    };

    private static readonly IReadOnlySet<string> ExactResolvedProperties = new HashSet<string>(StringComparer.Ordinal)
    {
        "seed", "mode", "presetId", "compactStyleHintIds", "selectedVariantIds", "stableSummary",
        "presetDefinitionSha256", "styleOverridesApplied", "variantOverridesApplied"
    };

    private readonly IGamePackageValidator _validator;
    private readonly GenerationPresetOptionsService _presetOptions;
    private readonly GeneratedProjectOverlayService _overlayService;
    private readonly ProceduralGameKernelService _kernelService;
    private readonly IGeneratedProjectBaselineProvider? _baselineProvider;

    public SeededGeneratedProjectSourceService(
        IGamePackageValidator? validator = null,
        GenerationPresetOptionsService? presetOptions = null,
        GeneratedProjectOverlayService? overlayService = null,
        ProceduralGameKernelService? kernelService = null,
        IGeneratedProjectBaselineProvider? baselineProvider = null)
    {
        _validator = validator ?? new GamePackageValidator();
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
        _overlayService = overlayService ?? new GeneratedProjectOverlayService(_validator);
        _kernelService = kernelService ?? new ProceduralGameKernelService();
        _baselineProvider = baselineProvider;
    }

    public SeededGeneratedProjectSourceValidationResult Validate(string projectFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        var root = Path.GetFullPath(projectFolder);
        var sourcePath = Resolve(root, SeededGeneratedProjectVocabulary.SourceRelativePath);
        if (!File.Exists(sourcePath)) return new SeededGeneratedProjectSourceValidationResult
        {
            Present = false,
            Passed = true,
            Status = "ABSENT",
            SourcePath = sourcePath
        };

        var diagnostics = new List<string>();
        try
        {
            var sourceJson = File.ReadAllText(sourcePath, Encoding.UTF8);
            using var sourceDocument = JsonDocument.Parse(sourceJson);
            var schemaVersion = sourceDocument.RootElement.TryGetProperty("schemaVersion", out var schemaElement)
                ? schemaElement.GetString() ?? string.Empty
                : string.Empty;
            ValidateExactProperties(sourceDocument.RootElement, schemaVersion, diagnostics);
            var deserialized = JsonSerializer.Deserialize<SeededGeneratedProjectSourceRecord>(sourceJson, JsonOptions);
            if (deserialized is null)
                return Failed(sourcePath, diagnostics.Append("generated_source.invalid_json"));
            if (schemaVersion == SeededGeneratedProjectVocabulary.SourceV2SchemaVersion)
            {
                var request = sourceDocument.RootElement.TryGetProperty("generationRequest", out var requestElement)
                    ? JsonSerializer.Deserialize<SeededGeneratedProjectGenerationRequest>(
                        requestElement.GetRawText(), JsonOptions)
                    : null;
                var resolvedOptions = sourceDocument.RootElement.TryGetProperty(
                    "resolvedGenerationOptions", out var resolvedElement)
                    ? JsonSerializer.Deserialize<SeededGeneratedProjectResolvedOptions>(
                        resolvedElement.GetRawText(), JsonOptions)
                    : null;
                deserialized = deserialized with
                {
                    GenerationRequest = request ?? new SeededGeneratedProjectGenerationRequest(),
                    ResolvedGenerationOptions = resolvedOptions ?? new SeededGeneratedProjectResolvedOptions()
                };
            }
            var source = NormalizeSource(deserialized, schemaVersion, diagnostics);
            ValidateVocabulary(source, diagnostics);
            var resolved = _presetOptions.Resolve(source.GenerationRequest);
            ValidateResolvedRequest(source, resolved, diagnostics);
            var generationRoot = Resolve(root, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            ValidateSidecars(generationRoot, source, diagnostics);
            if (diagnostics.Count > 0) return Failed(sourcePath, diagnostics, source);

            var planJson = Read(generationRoot, SeededGeneratedProjectVocabulary.PlanJsonFileName);
            var rulePackJson = Read(generationRoot, SeededGeneratedProjectVocabulary.RulePackJsonFileName);
            var tinyStateJson = Read(generationRoot, SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName);
            var tinyReportMarkdown = Read(generationRoot, SeededGeneratedProjectVocabulary.TinyLoopReportMarkdownFileName);
            var generatedMvpJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName);
            var overlayJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName);
            var generatedBaseJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);

            var plan = Deserialize<ProceduralGeneratedGamePlan>(planJson, "generated_source.plan_invalid_json");
            var rulePack = Deserialize<FormulaEffectActionRulePack>(rulePackJson, "generated_source.rule_pack_invalid_json");
            var tinyState = Deserialize<TinyGeneratedRuntimeState>(tinyStateJson, "generated_source.tiny_state_invalid_json");
            var generatedMvp = Deserialize<GamePackageDefinition>(generatedMvpJson, "generated_source.mvp_invalid_json");
            var overlay = Deserialize<GeneratedProjectOverlayDocument>(overlayJson, "generated_source.overlay_invalid_json");
            var generatedBase = Deserialize<GamePackageDefinition>(generatedBaseJson, "generated_source.base_invalid_json");

            var regeneratedPlan = _kernelService.Generate(new ProceduralGameKernelRequest
            {
                Seed = resolved.Seed,
                Mode = resolved.Mode,
                CompactStyleHintIds = resolved.CompactStyleHintIds,
                SelectedVariantIds = resolved.SelectedVariantIds
            });
            if (!string.Equals(regeneratedPlan.Json, planJson, StringComparison.Ordinal)
                || !string.Equals(regeneratedPlan.Markdown, Read(
                    generationRoot,
                    SeededGeneratedProjectVocabulary.PlanMarkdownFileName), StringComparison.Ordinal))
                diagnostics.Add("generated_source.plan_regeneration_mismatch");
            if (!string.Equals(source.PlanId, regeneratedPlan.Plan.PlanId, StringComparison.Ordinal)
                || !string.Equals(source.Seed, plan.Metadata.Seed, StringComparison.Ordinal)
                || !string.Equals(source.Mode, plan.Metadata.Mode, StringComparison.Ordinal))
                diagnostics.Add("generated_source.plan_metadata_mismatch");
            if (!source.StyleHintIds.All(plan.Profile.StyleHintIds.Contains)
                || !source.VariantIds.All(plan.Profile.VariantIds.Contains)
                || !OrdinalSetEquals(regeneratedPlan.Plan.Profile.StyleHintIds, plan.Profile.StyleHintIds)
                || !OrdinalSetEquals(regeneratedPlan.Plan.Profile.VariantIds, plan.Profile.VariantIds))
                diagnostics.Add("generated_source.plan_profile_mismatch");

            ValidateCounts(source.Counts, regeneratedPlan.Plan, diagnostics);
            if (!string.Equals(source.RulePackId, rulePack.Metadata.RulePackId, StringComparison.Ordinal))
                diagnostics.Add("generated_source.identity_mismatch");
            if (!string.Equals(source.GeneratedStartMapId, overlay.GeneratedStartMapId, StringComparison.Ordinal)
                || !generatedBase.Game.Maps.Any(map => map.Id == source.GeneratedStartMapId))
                diagnostics.Add("generated_source.generated_start_map_missing");

            var regeneratedRulePack = new FormulaEffectActionRegistryService().Generate(
                new FormulaEffectActionRegistryRequest { SourcePlan = regeneratedPlan.Plan });
            if (!string.Equals(regeneratedRulePack.Json, rulePackJson, StringComparison.Ordinal))
                diagnostics.Add("generated_source.rule_pack_regeneration_mismatch");
            var regeneratedTinyLoop = new TinyGeneratedRuntimeLoopService().Run(new TinyGeneratedRuntimeLoopRequest
            {
                SourcePlan = regeneratedPlan.Plan,
                RulePack = regeneratedRulePack.RulePack,
                RulePackValidationReport = regeneratedRulePack.ValidationReport
            });
            if (!string.Equals(regeneratedTinyLoop.StateJson, tinyStateJson, StringComparison.Ordinal)
                || !string.Equals(regeneratedTinyLoop.ReportMarkdown, tinyReportMarkdown, StringComparison.Ordinal))
                diagnostics.Add("generated_source.tiny_loop_regeneration_mismatch");
            var expectedTiny = BuildTinyLoopFacts(regeneratedPlan.Plan, regeneratedRulePack.RulePack, regeneratedTinyLoop);
            if (expectedTiny != source.TinyLoop
                || !string.Equals(tinyState.DeterministicHash, source.TinyLoop.FinalStateHash, StringComparison.Ordinal)
                || !source.TinyLoop.Passed)
                diagnostics.Add("generated_source.tiny_loop_failed");

            var regeneratedMvp = new GeneratedPackageMvpService(_validator).Generate(new GeneratedPackageMvpRequest
            {
                SourcePlan = regeneratedPlan.Plan,
                RulePack = regeneratedRulePack.RulePack,
                RulePackValidationReport = regeneratedRulePack.ValidationReport,
                TinyLoopResult = regeneratedTinyLoop
            });
            var regeneratedNamespacedMvp = _overlayService.NamespaceGeneratedPackage(regeneratedMvp.PackageJson);
            if (!string.Equals(regeneratedNamespacedMvp, generatedMvpJson, StringComparison.Ordinal))
                diagnostics.Add("generated_source.mvp_regeneration_mismatch");

            GeneratedProjectBaseline? baseline = null;
            try
            {
                baseline = _baselineProvider?.Resolve();
                if (baseline is null) diagnostics.Add("generated_source.baseline_unavailable");
            }
            catch (InvalidOperationException exception) when (
                exception.Message is "generated_source.baseline_unavailable" or "generated_source.baseline_hash_mismatch")
            {
                diagnostics.Add(exception.Message);
            }
            if (baseline is not null)
            {
                if (!string.Equals(source.Goal142BaselinePackageSha256, baseline.PackageSha256, StringComparison.Ordinal)
                    || !string.Equals(overlay.Goal142BaselinePackageSha256, baseline.PackageSha256, StringComparison.Ordinal))
                    diagnostics.Add("generated_source.baseline_hash_mismatch");
                var rebuilt = _overlayService.Build(
                    baseline.PackageJson,
                    baseline.PackageSha256,
                    regeneratedNamespacedMvp,
                    regeneratedPlan.Plan);
                if (!string.Equals(rebuilt.OverlayJson, overlayJson, StringComparison.Ordinal))
                    diagnostics.Add("generated_source.overlay_regeneration_mismatch");
                if (!string.Equals(rebuilt.GeneratedBasePackageJson, generatedBaseJson, StringComparison.Ordinal))
                    diagnostics.Add("generated_source.base_regeneration_mismatch");
                if (!string.Equals(source.GeneratedBasePackageSha256, rebuilt.Document.GeneratedBasePackageSha256,
                        StringComparison.Ordinal)
                    || !string.Equals(source.GeneratedMvpPackageSha256, rebuilt.Document.GeneratedMvpPackageSha256,
                        StringComparison.Ordinal)
                    || !string.Equals(source.GeneratedBasePackageSha256, overlay.GeneratedBasePackageSha256,
                        StringComparison.Ordinal))
                    diagnostics.Add("generated_source.overlay_hash_chain_mismatch");
            }

            diagnostics.AddRange(_overlayService.ValidatePackageRecords(generatedBaseJson, overlay, includeBaseline: true));
            var validation = _validator.Validate(generatedBase);
            diagnostics.AddRange(validation.Issues
                .Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error
                    or Domain.Validation.ValidationSeverity.Critical)
                .Select(issue => "generated_source.package_invalid:" + issue.Code));
            if (!string.Equals(HashText(generatedMvpJson), source.GeneratedMvpPackageSha256, StringComparison.Ordinal)
                || !string.Equals(HashText(generatedBaseJson), source.GeneratedBasePackageSha256, StringComparison.Ordinal)
                || !string.Equals(HashText(overlayJson), source.GeneratedOverlaySha256, StringComparison.Ordinal))
                diagnostics.Add("generated_source.sidecar_hash_mismatch");

            diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
            var passed = diagnostics.Count == 0;
            return new SeededGeneratedProjectSourceValidationResult
            {
                Present = true,
                Passed = passed,
                Status = passed ? "CURRENT" : "INVALID",
                SourcePath = sourcePath,
                Source = source,
                Overlay = overlay,
                GeneratedBasePackage = generatedBase,
                GeneratedMvpPackage = generatedMvp,
                RegeneratedPlan = passed ? regeneratedPlan.Plan : null,
                RegeneratedPlanJson = passed ? regeneratedPlan.Json : string.Empty,
                GenerationRequest = passed ? source.GenerationRequest : null,
                ResolvedGenerationOptions = passed ? resolved : null,
                RequestOrigin = passed ? source.RequestOrigin : string.Empty,
                Diagnostics = diagnostics
            };
        }
        catch (JsonException)
        {
            return Failed(sourcePath, diagnostics.Append("generated_source.invalid_json"));
        }
        catch (IOException exception)
        {
            return Failed(sourcePath, diagnostics.Append("generated_source.unreadable:" + exception.GetType().Name));
        }
        catch (InvalidOperationException exception)
        {
            return Failed(sourcePath, diagnostics.Append(exception.Message));
        }
    }

    public static GeneratedProjectCounts Counts(ProceduralGeneratedGamePlan plan) => new()
    {
        Regions = plan.World.Regions.Count,
        Factions = plan.Factions.Count,
        Actors = plan.ActorSeeds.Count,
        ItemsAndResources = plan.ItemResourceSeeds.Count,
        Encounters = plan.EncounterSeeds.Count,
        QuestEvents = plan.QuestEventSeeds.Count
    };

    public static string SerializeV2(SeededGeneratedProjectSourceRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!string.Equals(source.SchemaVersion, SeededGeneratedProjectVocabulary.SourceV2SchemaVersion,
                StringComparison.Ordinal))
            throw new InvalidOperationException("generated_source.v2_schema_required");
        return JsonSerializer.Serialize(new
        {
            source.SchemaVersion,
            source.CreationKind,
            source.GenerationRequest,
            source.ResolvedGenerationOptions,
            source.MechanicsProfileId,
            source.PlanId,
            source.PlanSha256,
            source.RulePackId,
            source.RulePackSha256,
            source.TinyLoopStateSha256,
            source.GeneratedMvpPackageSha256,
            source.GeneratedOverlaySha256,
            source.GeneratedBasePackageSha256,
            source.Goal142BaselinePackageSha256,
            source.GeneratedStartMapId,
            source.Counts,
            source.TinyLoop,
            source.SidecarSha256
        }, JsonOptions);
    }

    public static GeneratedProjectTinyLoopFacts BuildTinyLoopFacts(
        ProceduralGeneratedGamePlan plan,
        FormulaEffectActionRulePack rulePack,
        TinyGeneratedRuntimeLoopResult tinyLoop)
    {
        var initial = HashText("seeded_generated_project_initial_state_v1\n"
                               + plan.Metadata.DeterministicHash + "\n" + rulePack.Metadata.DeterministicHash);
        var rewardOrCost = tinyLoop.State.InventoryItemCounts.Values.Any(value => value != 0)
                           || tinyLoop.State.FactionReputationDeltas.Values.Any(value => value != 0)
                           || tinyLoop.State.QuestEventStates.Count > 0;
        return new GeneratedProjectTinyLoopFacts
        {
            Passed = !tinyLoop.Report.HasErrors && tinyLoop.Report.Steps.Count > 0,
            InitialStateHash = initial,
            FinalStateHash = tinyLoop.State.DeterministicHash,
            StepCount = tinyLoop.Report.Steps.Count,
            RewardOrCostObserved = rewardOrCost,
            StateChangeObserved = tinyLoop.Report.Steps.Count > 0
                                  && !string.Equals(initial, tinyLoop.State.DeterministicHash, StringComparison.Ordinal)
        };
    }

    private void ValidateVocabulary(SeededGeneratedProjectSourceRecord source, ICollection<string> diagnostics)
    {
        if (source.SchemaVersion is not SeededGeneratedProjectVocabulary.SourceSchemaVersion
                and not SeededGeneratedProjectVocabulary.SourceV2SchemaVersion
            || !string.Equals(source.CreationKind, GameProjectCreationKinds.SeededGenerated, StringComparison.Ordinal))
            diagnostics.Add("generated_source.unsupported_schema");
        if (string.IsNullOrWhiteSpace(source.Seed)) diagnostics.Add("generated_source.seed_missing");
        if (!ProceduralGameGenerationModes.Supported.Contains(source.Mode)) diagnostics.Add("generated_source.mode_unsupported");
        if (!_presetOptions.GetPresets().Any(preset => preset.PresetId == source.PresetId)) diagnostics.Add("generated_source.preset_unknown");
        if (!GeneratedProjectMechanicsProfiles.Supported.Contains(source.MechanicsProfileId, StringComparer.Ordinal))
            diagnostics.Add("generated_source.profile_unknown");
    }

    private static void ValidateResolvedRequest(
        SeededGeneratedProjectSourceRecord source,
        SeededGeneratedProjectResolvedOptions resolved,
        ICollection<string> diagnostics)
    {
        if (source.SchemaVersion == SeededGeneratedProjectVocabulary.SourceV2SchemaVersion)
        {
            var normalizedRequest = NormalizeRequest(source.GenerationRequest);
            if (!RequestEquals(source.GenerationRequest, normalizedRequest))
                diagnostics.Add("generated_source.request_options_mismatch");
            if (!ResolvedEquals(source.ResolvedGenerationOptions, resolved))
                diagnostics.Add("generated_source.v2_request_resolution_mismatch");
            if (!string.Equals(source.ResolvedGenerationOptions.PresetDefinitionSha256,
                    resolved.PresetDefinitionSha256, StringComparison.Ordinal))
                diagnostics.Add("generated_source.preset_definition_mismatch");
            if (!RequestMatchesResolved(source.GenerationRequest, source.ResolvedGenerationOptions))
                diagnostics.Add("generated_source.request_options_mismatch");
            if (!ResolvedEquals(source.ResolvedGenerationOptions, resolved))
                diagnostics.Add("generated_source.resolved_options_mismatch");
            return;
        }

        var mismatch = false;
        if (!string.Equals(source.Seed, resolved.Seed, StringComparison.Ordinal))
        {
            diagnostics.Add("generated_source.seed_mismatch");
            mismatch = true;
        }
        if (!string.Equals(source.Mode, resolved.Mode, StringComparison.Ordinal))
        {
            diagnostics.Add("generated_source.mode_mismatch");
            mismatch = true;
        }
        if (!string.Equals(source.PresetId, resolved.PresetId, StringComparison.Ordinal))
            mismatch = true;
        if (!OrdinalSetEquals(source.StyleHintIds, resolved.CompactStyleHintIds))
        {
            diagnostics.Add("generated_source.style_hints_mismatch");
            mismatch = true;
        }
        if (!OrdinalSetEquals(source.VariantIds, resolved.SelectedVariantIds))
        {
            diagnostics.Add("generated_source.variant_ids_mismatch");
            mismatch = true;
        }
        if (mismatch) diagnostics.Add("generated_source.request_resolution_mismatch");
    }

    private SeededGeneratedProjectSourceRecord NormalizeSource(
        SeededGeneratedProjectSourceRecord source,
        string schemaVersion,
        ICollection<string> diagnostics)
    {
        if (schemaVersion == SeededGeneratedProjectVocabulary.SourceSchemaVersion)
        {
            var request = new SeededGeneratedProjectGenerationRequest
            {
                Seed = source.Seed,
                Mode = source.Mode,
                PresetId = source.PresetId,
                CompactStyleHintIds = source.StyleHintIds,
                SelectedVariantIds = source.VariantIds
            };
            SeededGeneratedProjectResolvedOptions resolved;
            try { resolved = _presetOptions.Resolve(request); }
            catch (InvalidOperationException exception)
            {
                diagnostics.Add(exception.Message);
                resolved = new SeededGeneratedProjectResolvedOptions();
            }
            return source with
            {
                GenerationRequest = request,
                ResolvedGenerationOptions = resolved,
                RequestOrigin = SeededGeneratedProjectRequestOrigins.LegacyV1EffectiveOptions
            };
        }
        if (schemaVersion != SeededGeneratedProjectVocabulary.SourceV2SchemaVersion) return source;
        var effective = source.ResolvedGenerationOptions;
        return source with
        {
            Seed = effective.Seed,
            Mode = effective.Mode,
            PresetId = effective.PresetId,
            StyleHintIds = effective.CompactStyleHintIds,
            VariantIds = effective.SelectedVariantIds,
            RequestOrigin = SeededGeneratedProjectRequestOrigins.ExplicitV2Request
        };
    }

    private static void ValidateExactProperties(
        JsonElement root,
        string schemaVersion,
        ICollection<string> diagnostics)
    {
        var properties = root.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        var expected = schemaVersion == SeededGeneratedProjectVocabulary.SourceSchemaVersion
            ? ExactV1SourceProperties
            : schemaVersion == SeededGeneratedProjectVocabulary.SourceV2SchemaVersion
                ? ExactV2SourceProperties
                : null;
        if (expected is null || !properties.SetEquals(expected)) diagnostics.Add("generated_source.unsupported_schema");
        if (schemaVersion != SeededGeneratedProjectVocabulary.SourceV2SchemaVersion) return;
        if (!root.TryGetProperty("generationRequest", out var request)
            || request.ValueKind != JsonValueKind.Object
            || !request.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal)
                .SetEquals(ExactRequestProperties))
            diagnostics.Add("generated_source.request_options_mismatch");
        if (!root.TryGetProperty("resolvedGenerationOptions", out var resolved)
            || resolved.ValueKind != JsonValueKind.Object
            || !resolved.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal)
                .SetEquals(ExactResolvedProperties))
            diagnostics.Add("generated_source.resolved_options_mismatch");
    }

    internal static SeededGeneratedProjectGenerationRequest NormalizeRequest(
        SeededGeneratedProjectGenerationRequest request) => new()
    {
        Seed = request.Seed.Trim(),
        Mode = request.Mode.Trim(),
        PresetId = request.PresetId.Trim(),
        CompactStyleHintIds = NormalizeIds(request.CompactStyleHintIds),
        SelectedVariantIds = NormalizeIds(request.SelectedVariantIds)
    };

    private static IReadOnlyList<string> NormalizeIds(IReadOnlyList<string> values) => values
        .Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim())
        .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();

    internal static bool RequestEquals(
        SeededGeneratedProjectGenerationRequest left,
        SeededGeneratedProjectGenerationRequest right) =>
        string.Equals(left.Seed, right.Seed, StringComparison.Ordinal)
        && string.Equals(left.Mode, right.Mode, StringComparison.Ordinal)
        && string.Equals(left.PresetId, right.PresetId, StringComparison.Ordinal)
        && OrdinalSetEquals(left.CompactStyleHintIds, right.CompactStyleHintIds)
        && OrdinalSetEquals(left.SelectedVariantIds, right.SelectedVariantIds);

    internal static bool ResolvedEquals(
        SeededGeneratedProjectResolvedOptions left,
        SeededGeneratedProjectResolvedOptions right) =>
        string.Equals(left.Seed, right.Seed, StringComparison.Ordinal)
        && string.Equals(left.Mode, right.Mode, StringComparison.Ordinal)
        && string.Equals(left.PresetId, right.PresetId, StringComparison.Ordinal)
        && OrdinalSetEquals(left.CompactStyleHintIds, right.CompactStyleHintIds)
        && OrdinalSetEquals(left.SelectedVariantIds, right.SelectedVariantIds)
        && string.Equals(left.StableSummary, right.StableSummary, StringComparison.Ordinal)
        && string.Equals(left.PresetDefinitionSha256, right.PresetDefinitionSha256, StringComparison.Ordinal)
        && left.StyleOverridesApplied == right.StyleOverridesApplied
        && left.VariantOverridesApplied == right.VariantOverridesApplied;

    private static bool RequestMatchesResolved(
        SeededGeneratedProjectGenerationRequest request,
        SeededGeneratedProjectResolvedOptions resolved) =>
        string.Equals(request.Seed, resolved.Seed, StringComparison.Ordinal)
        && string.Equals(request.Mode, resolved.Mode, StringComparison.Ordinal)
        && string.Equals(request.PresetId, resolved.PresetId, StringComparison.Ordinal)
        && (request.CompactStyleHintIds.Count == 0
            || OrdinalSetEquals(request.CompactStyleHintIds, resolved.CompactStyleHintIds))
        && (request.SelectedVariantIds.Count == 0
            || OrdinalSetEquals(request.SelectedVariantIds, resolved.SelectedVariantIds));

    private static bool OrdinalSetEquals(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        left.OrderBy(value => value, StringComparer.Ordinal)
            .SequenceEqual(right.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal);

    private static void ValidateSidecars(
        string generationRoot,
        SeededGeneratedProjectSourceRecord source,
        ICollection<string> diagnostics)
    {
        foreach (var fileName in SeededGeneratedProjectVocabulary.RequiredSidecarFileNames)
        {
            var path = Resolve(generationRoot, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("generated_source.sidecar_missing:" + fileName);
                continue;
            }
            if (!source.SidecarSha256.TryGetValue(fileName, out var expected)
                || !string.Equals(HashFile(path), expected, StringComparison.Ordinal))
                diagnostics.Add("generated_source.sidecar_hash_mismatch:" + fileName);
        }
        if (source.SidecarSha256.Count != SeededGeneratedProjectVocabulary.RequiredSidecarFileNames.Count)
            diagnostics.Add("generated_source.sidecar_inventory_mismatch");
        Match(source.PlanSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.PlanJsonFileName, diagnostics);
        Match(source.RulePackSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.RulePackJsonFileName, diagnostics);
        Match(source.TinyLoopStateSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName, diagnostics);
        Match(source.GeneratedMvpPackageSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName, diagnostics);
        Match(source.GeneratedOverlaySha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName, diagnostics);
        Match(source.GeneratedBasePackageSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName, diagnostics);
    }

    private static void Match(
        string directHash,
        IReadOnlyDictionary<string, string> sidecars,
        string fileName,
        ICollection<string> diagnostics)
    {
        if (!sidecars.TryGetValue(fileName, out var value)
            || !string.Equals(directHash, value, StringComparison.Ordinal))
            diagnostics.Add("generated_source.sidecar_hash_mismatch:" + fileName);
    }

    private static void ValidateCounts(
        GeneratedProjectCounts expected,
        ProceduralGeneratedGamePlan plan,
        ICollection<string> diagnostics)
    {
        if (expected != Counts(plan)) diagnostics.Add("generated_source.count_mismatch");
    }

    private static T Deserialize<T>(string json, string code) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException(code);

    private static string Read(string root, string fileName) => File.ReadAllText(Resolve(root, fileName), Encoding.UTF8);

    private static string Resolve(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("generated_source.path_escape");
        return path;
    }

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static SeededGeneratedProjectSourceValidationResult Failed(
        string sourcePath,
        IEnumerable<string> diagnostics,
        SeededGeneratedProjectSourceRecord? source = null) => new()
    {
        Present = true,
        Passed = false,
        Status = "INVALID",
        SourcePath = sourcePath,
        Source = source,
        Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
    };
}
