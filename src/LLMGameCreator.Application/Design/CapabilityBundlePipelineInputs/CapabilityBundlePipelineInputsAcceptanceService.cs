using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GameProfiles;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;

public sealed class CapabilityBundlePipelineInputsAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    public const string ProfileRequestsJsonFileName = "capability-bundle-pipeline-inputs-profile-requests.json";
    public const string SelectionJsonFileName = "capability-bundle-pipeline-inputs-selection.json";
    public const string GeneratorInputsJsonFileName = "capability-bundle-pipeline-inputs-generator-inputs.json";
    public const string GapReportJsonFileName = "capability-bundle-pipeline-inputs-gap-report.json";
    public const string InvalidMatrixJsonFileName = "capability-bundle-pipeline-inputs-invalid-matrix.json";
    public const string ReportJsonFileName = "capability-bundle-pipeline-inputs-report.json";
    public const string ReportMarkdownFileName = "capability-bundle-pipeline-inputs-report.md";
    public const string VerificationMarkdownFileName = "capability-bundle-pipeline-inputs-verification.md";
    public const string FinalGate = "capability_bundle_pipeline_inputs_verification";
    public const string PreviousAcceptedGate = "development_complexity_stabilization_verification passed";
    public const string PreviousProfileGate = "generated_game_profile_contract_verification passed";
    private const string ProductSmokeRoute = "capability-bundle-pipeline-inputs";
    private const string Goal021RelativeOutputDirectory = ".llmgc/procedural/generated-game-profile-contract";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<CapabilityBundlePipelineInputsAcceptanceResult> BuildAsync(
        string projectRootPath,
        string profileDirectoryPath,
        string? atlasRootPath = null,
        CapabilityBundlePipelineInputsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        if (string.IsNullOrWhiteSpace(profileDirectoryPath))
        {
            throw new ArgumentException("Profile directory path is required.", nameof(profileDirectoryPath));
        }

        var settings = options ?? new CapabilityBundlePipelineInputsOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var profileDirectory = Path.GetFullPath(profileDirectoryPath);
        var atlasReader = new GeneratorPlanCapabilitySelectionAtlasReader();
        var selectionService = new GeneratorPlanCapabilitySelectionService(atlasReader);
        var resolvedAtlasRoot = string.IsNullOrWhiteSpace(atlasRootPath)
            ? selectionService.DiscoverAtlasRoot()
            : atlasRootPath.Trim();

        var diagnostics = new List<CapabilityBundlePipelineDiagnostic>
        {
            Diagnostic("info", "capability_bundle.goal022_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 022 development complexity stabilization verification is recorded as passed."),
            Diagnostic("info", "capability_bundle.goal021_profile_gate_recorded", PreviousProfileGate, "User-confirmed Goal 021 generated game profile contract verification is recorded as passed."),
            Diagnostic("info", "capability_bundle.no_external_execution", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua, Unity build, package assembly or generator-library execution was invoked.")
        };

        var loadedProfiles = LoadProfiles(profileDirectory, settings);
        diagnostics.AddRange(loadedProfiles.SelectMany(profile => profile.Diagnostics));
        diagnostics.AddRange(ValidateProfileSet(loadedProfiles, settings));
        diagnostics.AddRange(ValidateGoal021Artifacts(projectRoot, settings));

        var validProfiles = loadedProfiles
            .Where(profile => profile.Profile != null)
            .GroupBy(profile => profile.Profile!.ProfileId, StringComparer.Ordinal)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() == 1)
            .Select(group => group.Single())
            .OrderBy(profile => profile.Profile!.ProfileId, StringComparer.Ordinal)
            .ToList();

        var profileRequests = new List<CapabilityBundleProfileRequestRecord>();
        var selections = new List<CapabilityBundleSelectionRecord>();
        var pipelineInputs = new List<CapabilityBundlePipelineInputRecord>();
        var gaps = new List<CapabilityBundleGapRecord>();

        foreach (var loaded in validProfiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var profile = loaded.Profile!;
            var selectorRequest = BuildSelectorRequest(profile, resolvedAtlasRoot);
            var selectorResult = await selectionService.BuildSelectionAsync(selectorRequest, cancellationToken).ConfigureAwait(false);
            var profileGaps = BuildGaps(profile, selectorResult);
            gaps.AddRange(profileGaps);

            profileRequests.Add(new CapabilityBundleProfileRequestRecord
            {
                ProfileId = profile.ProfileId,
                SourceProfilePath = loaded.SourceRelativePath,
                SourceProfileHash = loaded.SourceHash,
                GameFamilyId = profile.GameFamilyId,
                RequestedRuntimeExportTarget = profile.RuntimeExportTarget,
                SelectorRequest = ToRequestArtifact(selectorRequest)
            });

            selections.Add(ToSelectionRecord(profile, selectorResult, profileGaps));
            pipelineInputs.Add(ToPipelineInputRecord(profile, selectorResult, profileGaps));
        }

        var invalidMatrix = await BuildInvalidMatrixAsync(validProfiles, projectRoot, profileDirectory, resolvedAtlasRoot, settings, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var gapReport = new CapabilityBundleGapReportArtifact
        {
            SchemaVersion = "capability_bundle_pipeline_inputs_gap_report_v1",
            GapCount = gaps.Count,
            BlockedGapCount = gaps.Count(gap => gap.Status == "blocked_gap"),
            FutureRequiredCount = gaps.Count(gap => gap.Status == "future_required"),
            Gaps = SortGaps(gaps),
            Diagnostics = SortDiagnostics(diagnostics.Where(diagnostic => diagnostic.Code.Contains(".gap", StringComparison.Ordinal) || diagnostic.Code.Contains("atlas", StringComparison.Ordinal)))
        };

        var profileRequestArtifact = new CapabilityBundleProfileRequestsArtifact
        {
            SchemaVersion = "capability_bundle_pipeline_inputs_profile_requests_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            RequestCount = profileRequests.Count,
            Requests = profileRequests
        };
        var selectionArtifact = new CapabilityBundleSelectionArtifact
        {
            SchemaVersion = "capability_bundle_pipeline_inputs_selection_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            SelectionCount = selections.Count,
            Selections = selections
        };
        var generatorInputsArtifact = new CapabilityBundleGeneratorInputsArtifact
        {
            SchemaVersion = "capability_bundle_pipeline_inputs_generator_inputs_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            PipelineInputCount = pipelineInputs.Count,
            PipelineInputs = pipelineInputs
        };

        var profileRequestsJson = JsonSerializer.Serialize(profileRequestArtifact, JsonOptions);
        var selectionJson = JsonSerializer.Serialize(selectionArtifact, JsonOptions);
        var generatorInputsJson = JsonSerializer.Serialize(generatorInputsArtifact, JsonOptions);
        var gapReportJson = JsonSerializer.Serialize(gapReport, JsonOptions);
        var invalidMatrixJson = JsonSerializer.Serialize(invalidMatrix, JsonOptions);
        var profileRequestsHash = ComputeHash(profileRequestsJson);
        var selectionHash = ComputeHash(selectionJson);
        var generatorInputsHash = ComputeHash(generatorInputsJson);
        var gapReportHash = ComputeHash(gapReportJson);
        var invalidMatrixHash = ComputeHash(invalidMatrixJson);

        var allFutureRequiredPreserved = pipelineInputs.All(input =>
            input.FutureRequiredCapabilityIds.All(id => !input.SupportedNowCapabilityIds.Contains(id, StringComparer.Ordinal)));
        var noTopLevelErrors = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var contractProofPassed =
            settings.PreviousAcceptedGate == PreviousAcceptedGate &&
            !settings.MissingGoal021ProfileArtifacts &&
            !settings.CopiedCapabilitySelectionReportWithoutProfiles &&
            validProfiles.Count == 3 &&
            profileRequests.Count == 3 &&
            selections.Count == 3 &&
            pipelineInputs.Count == 3 &&
            allFutureRequiredPreserved &&
            invalidMatrix.Passed &&
            noTopLevelErrors;

        var reportWithoutHash = new CapabilityBundlePipelineInputsReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            PreviousProfileGate = PreviousProfileGate,
            CompletedSlices = ["S185", "S186", "S187", "S188", "S189", "S190", "S191"],
            ProductSmokeRoute = ProductSmokeRoute,
            ProfileCount = loadedProfiles.Count,
            ValidProfileCount = validProfiles.Count,
            PipelineInputCount = pipelineInputs.Count,
            CapabilitySelectionStarted = selections.Count > 0,
            ContractProofPassed = contractProofPassed,
            FutureRequiredCapabilitiesPreserved = allFutureRequiredPreserved,
            ProfileRequestArtifactHash = profileRequestsHash,
            SelectionArtifactHash = selectionHash,
            GeneratorInputsArtifactHash = generatorInputsHash,
            GapReportHash = gapReportHash,
            InvalidMatrixHash = invalidMatrixHash,
            SelectedProfileIds = validProfiles.Select(profile => profile.Profile!.ProfileId).Order(StringComparer.Ordinal).ToList(),
            GapSummary = gapReport,
            InvalidMatrix = invalidMatrix,
            PackageAssemblyExecuted = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = true,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new CapabilityBundlePipelineInputsAcceptanceResult
        {
            ProfileRequestsArtifact = profileRequestArtifact,
            SelectionArtifact = selectionArtifact,
            GeneratorInputsArtifact = generatorInputsArtifact,
            GapReportArtifact = gapReport,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ProfileRequestsJson = profileRequestsJson,
            SelectionJson = selectionJson,
            GeneratorInputsJson = generatorInputsJson,
            GapReportJson = gapReportJson,
            InvalidMatrixJson = invalidMatrixJson,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, selections, pipelineInputs),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<CapabilityBundlePipelineInputsWriteResult> WriteAsync(
        string projectRootPath,
        CapabilityBundlePipelineInputsAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var profileRequestsPath = Path.Combine(outputDirectory, ProfileRequestsJsonFileName);
        var selectionPath = Path.Combine(outputDirectory, SelectionJsonFileName);
        var generatorInputsPath = Path.Combine(outputDirectory, GeneratorInputsJsonFileName);
        var gapReportPath = Path.Combine(outputDirectory, GapReportJsonFileName);
        var invalidMatrixPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName);
        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);

        await File.WriteAllTextAsync(profileRequestsPath, result.ProfileRequestsJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(selectionPath, result.SelectionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(generatorInputsPath, result.GeneratorInputsJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(gapReportPath, result.GapReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(invalidMatrixPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new CapabilityBundlePipelineInputsWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ProfileRequestsJsonPath = profileRequestsPath,
            SelectionJsonPath = selectionPath,
            GeneratorInputsJsonPath = generatorInputsPath,
            GapReportJsonPath = gapReportPath,
            InvalidMatrixJsonPath = invalidMatrixPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            VerificationMarkdownPath = verificationMarkdownPath
        };
    }

    public async Task<CapabilityBundlePipelineInputsWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string profileDirectoryPath,
        string? atlasRootPath = null,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, profileDirectoryPath, atlasRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<LoadedCapabilityProfile> LoadProfiles(string profileDirectory, CapabilityBundlePipelineInputsOptions settings)
    {
        if (settings.CopiedCapabilitySelectionReportWithoutProfiles)
        {
            return [];
        }

        return Directory.EnumerateFiles(profileDirectory, "*.game-profile.json")
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Select(path => LoadProfile(profileDirectory, path))
            .ToList();
    }

    private static LoadedCapabilityProfile LoadProfile(string profileDirectory, string profilePath)
    {
        var relativePath = RelativePath(profileDirectory, profilePath);
        var rawJson = File.ReadAllText(profilePath);
        try
        {
            var profile = JsonSerializer.Deserialize<GeneratedGameProfile>(rawJson, JsonOptions);
            if (profile == null)
            {
                return new LoadedCapabilityProfile
                {
                    SourceRelativePath = relativePath,
                    SourceHash = ComputeHash(rawJson),
                    Diagnostics = [Diagnostic("error", "capability_bundle.profile.empty", relativePath, "Profile JSON must deserialize to an object.")]
                };
            }

            return new LoadedCapabilityProfile
            {
                Profile = profile,
                SourceRelativePath = relativePath,
                SourceHash = ComputeHash(rawJson)
            };
        }
        catch (JsonException exception)
        {
            return new LoadedCapabilityProfile
            {
                SourceRelativePath = relativePath,
                SourceHash = ComputeHash(rawJson),
                Diagnostics = [Diagnostic("error", "capability_bundle.profile.invalid_json", relativePath, exception.Message)]
            };
        }
    }

    private static IReadOnlyList<CapabilityBundlePipelineDiagnostic> ValidateProfileSet(
        IReadOnlyList<LoadedCapabilityProfile> loaded,
        CapabilityBundlePipelineInputsOptions settings)
    {
        var diagnostics = new List<CapabilityBundlePipelineDiagnostic>();
        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.goal022_gate.missing", settings.PreviousAcceptedGate, "Goal 023 requires development_complexity_stabilization_verification passed."));
        }

        if (settings.CopiedCapabilitySelectionReportWithoutProfiles || loaded.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.profile_files.missing", "samples/game-profiles", "Capability bundle proof requires accepted profile files, not only a copied selection report."));
        }

        foreach (var duplicate in loaded
            .Where(profile => profile.Profile != null)
            .GroupBy(profile => profile.Profile!.ProfileId, StringComparer.Ordinal)
            .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.profile_id.duplicate", duplicate.Key, "Profile ids must be present and unique before capability selection."));
        }

        foreach (var loadedProfile in loaded.Where(profile => profile.Profile != null))
        {
            var profile = loadedProfile.Profile!;
            Require(!string.IsNullOrWhiteSpace(profile.ProfileId), diagnostics, "capability_bundle.profile_id.missing", loadedProfile.SourceRelativePath, "Profile id is required.");
            Require(!string.IsNullOrWhiteSpace(profile.GameFamilyId), diagnostics, "capability_bundle.game_family.missing", profile.ProfileId, "Game family id is required.");
            Require(!string.IsNullOrWhiteSpace(profile.PresentationMode), diagnostics, "capability_bundle.presentation.missing", profile.ProfileId, "Presentation mode is required.");
            Require(!string.IsNullOrWhiteSpace(profile.WorldTopology), diagnostics, "capability_bundle.world_topology.missing", profile.ProfileId, "World topology is required.");
            Require(!string.IsNullOrWhiteSpace(profile.RuntimeExportTarget), diagnostics, "capability_bundle.runtime_target.missing", profile.ProfileId, "Runtime/export target is required.");
            Require(profile.SelectedCapabilityIds.Count > 0, diagnostics, "capability_bundle.selected_capabilities.missing", profile.ProfileId, "Selected capability ids are required.");
            Require(!profile.Claims.PublicGamePackageSchemaMutation, diagnostics, "capability_bundle.claims.public_schema_mutation", profile.ProfileId, "Goal 023 must not claim public GamePackage schema mutation.");
            Require(!profile.Claims.UnityBuildProducedByGoal021, diagnostics, "capability_bundle.claims.unity_build", profile.ProfileId, "Goal 023 does not run or claim a Unity build.");
            Require(!profile.Claims.ArbitraryLuaRuntimeAuthority, diagnostics, "capability_bundle.claims.arbitrary_lua_runtime", profile.ProfileId, "Goal 023 must not grant arbitrary Lua runtime authority.");
            Require(!profile.AssetPolicy.RuntimeProviderDependency, diagnostics, "capability_bundle.asset_policy.runtime_provider_dependency", profile.ProfileId, "Runtime must not depend on a provider or media generation path.");
        }

        return SortDiagnostics(diagnostics);
    }

    private static IReadOnlyList<CapabilityBundlePipelineDiagnostic> ValidateGoal021Artifacts(
        string projectRoot,
        CapabilityBundlePipelineInputsOptions settings)
    {
        var diagnostics = new List<CapabilityBundlePipelineDiagnostic>();
        var root = ResolveGoal021EvidenceDirectory(projectRoot, settings);
        var reportPath = Path.Combine(root, GeneratedGameProfileContractAcceptanceService.ReportJsonFileName);
        var pipelinePlanPath = Path.Combine(root, GeneratedGameProfileContractAcceptanceService.PipelinePlanJsonFileName);

        if (settings.MissingGoal021ProfileArtifacts)
        {
            reportPath = Path.Combine(root, "missing-report.json");
            pipelinePlanPath = Path.Combine(root, "missing-pipeline-plan.json");
        }

        if (!File.Exists(reportPath))
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.goal021_evidence.report_missing", DiagnosticTargetForPath(projectRoot, reportPath), "Accepted Goal 021 profile contract report is required."));
        }

        if (!File.Exists(pipelinePlanPath))
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.goal021_evidence.pipeline_plan_missing", DiagnosticTargetForPath(projectRoot, pipelinePlanPath), "Accepted Goal 021 pipeline plan artifact is required."));
        }

        if (diagnostics.Any(item => item.Severity == "error"))
        {
            return diagnostics;
        }

        try
        {
            using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
            using var pipelinePlan = JsonDocument.Parse(File.ReadAllText(pipelinePlanPath));
            Require(JsonString(report.RootElement, "manualGate") == GeneratedGameProfileContractAcceptanceService.FinalGate, diagnostics, "capability_bundle.goal021_evidence.manual_gate_mismatch", DiagnosticTargetForPath(projectRoot, reportPath), "Goal 021 report manual gate must match generated_game_profile_contract_verification.");
            Require(JsonInt(pipelinePlan.RootElement, "planCount") == 3, diagnostics, "capability_bundle.goal021_evidence.pipeline_plan_count_mismatch", DiagnosticTargetForPath(projectRoot, pipelinePlanPath), "Goal 021 pipeline plan must contain three accepted profiles.");
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "capability_bundle.goal021_evidence.invalid_json", Goal021RelativeOutputDirectory, exception.Message));
        }

        if (diagnostics.All(item => item.Severity != "error"))
        {
            diagnostics.Add(Diagnostic("info", "capability_bundle.goal021_evidence.present", Goal021RelativeOutputDirectory, "Accepted Goal 021 compact report and pipeline plan are present."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static GeneratorPlanCapabilitySelectionRequest BuildSelectorRequest(GeneratedGameProfile profile, string atlasRootPath)
    {
        var variantIds = DeriveVariantIds(profile);
        return new GeneratorPlanCapabilitySelectionRequest
        {
            AtlasRootPath = atlasRootPath,
            Title = profile.DisplayName,
            Purpose = profile.TargetExperience,
            PresentationModeId = profile.PresentationMode,
            WorldTopologyId = profile.WorldTopology,
            ActorModelId = profile.ActorModel,
            InventoryModelId = variantIds.InventoryModelId,
            CombatModelId = variantIds.CombatModelId,
            ProgressionModelId = variantIds.ProgressionModelId,
            PathfindingProfileId = variantIds.PathfindingProfileId,
            NpcBehaviorModelId = variantIds.NpcBehaviorModelId,
            SelectedFeatureBundleIds = DeriveFeatureBundleIds(profile),
            SelectedRuntimeTargetIds = DeriveRuntimeTargetIds(profile)
        };
    }

    private static CapabilityBundleVariantIds DeriveVariantIds(GeneratedGameProfile profile)
    {
        var inventory = profile.AssetPolicy.Mode switch
        {
            "asset_mode/portrait_cards" => "inventory_model/list_inventory",
            "asset_mode/generated_media_requests_only" => "inventory_model/container_inventory",
            _ => "inventory_model/list_inventory"
        };
        var combat = profile.CapabilityFlags.Combat
            ? "combat_model/turn_based"
            : "combat_model/none";
        var progression = profile.ProgressionScope switch
        {
            "progression_scope/reputation_tracks" => "progression_model/reputation_tracks",
            _ => "progression_model/level_xp"
        };
        var pathfinding = profile.WorldTopology switch
        {
            "world_topology/region_graph" => "pathfinding/region_graph",
            "world_topology/node_map" => "pathfinding/waypoint_graph",
            _ => "pathfinding/waypoint_graph"
        };
        var npc = profile.CapabilityFlags.Work || profile.CapabilityFlags.Theft
            ? "npc_behavior/vendor_ai"
            : profile.CapabilityFlags.Social
                ? "npc_behavior/dialogue_state_driven"
                : "npc_behavior/quest_state_driven";

        return new CapabilityBundleVariantIds
        {
            PresentationModeId = profile.PresentationMode,
            WorldTopologyId = profile.WorldTopology,
            ActorModelId = profile.ActorModel,
            InventoryModelId = inventory,
            CombatModelId = combat,
            ProgressionModelId = progression,
            PathfindingProfileId = pathfinding,
            NpcBehaviorModelId = npc
        };
    }

    private static IReadOnlyList<string> DeriveRuntimeTargetIds(GeneratedGameProfile profile)
    {
        return profile.RuntimeExportTarget switch
        {
            "runtime_export/unity_alpha_windows" => ["headless", "unity2d", "unity3d"],
            _ => ["headless"]
        };
    }

    private static IReadOnlyList<string> DeriveFeatureBundleIds(GeneratedGameProfile profile)
    {
        var bundles = new SortedSet<string>(StringComparer.Ordinal)
        {
            "feature_bundle/core_atlas_planning/v1",
            "feature_bundle/game_profile_negotiation/v1",
            "feature_bundle/runtime_db_build_plan/v1",
            "feature_bundle/unity_ir_runtime_shell/v1",
            "feature_bundle/world_region_chunk_generation/v1",
            "feature_bundle/quest_multi_stage/v1"
        };

        if (profile.GameFamilyId == "game_family/frontier_survival")
        {
            bundles.Add("feature_bundle/survival_sandbox/v1");
        }

        if (profile.GameFamilyId == "game_family/gothic_mystery")
        {
            bundles.Add("feature_bundle/dialogue_choice_graph/v1");
            bundles.Add("feature_bundle/faction_reputation/v1");
            bundles.Add("feature_bundle/text_templates_and_morphology/v1");
        }

        if (profile.GameFamilyId == "game_family/trade_caravan")
        {
            bundles.Add("feature_bundle/dialogue_choice_graph/v1");
            bundles.Add("feature_bundle/faction_reputation/v1");
            bundles.Add("feature_bundle/media_request_generation/v1");
        }

        if (profile.CapabilityFlags.Combat)
        {
            bundles.Add("feature_bundle/combat_realtime_turn_hybrid/v1");
        }

        return bundles.ToList();
    }

    private static IReadOnlyList<CapabilityBundleGapRecord> BuildGaps(
        GeneratedGameProfile profile,
        GeneratorPlanCapabilitySelectionResult selectorResult)
    {
        var gaps = new List<CapabilityBundleGapRecord>();
        foreach (var futureCapability in profile.SelectedCapabilityIds.Where(id => id.EndsWith("_future", StringComparison.Ordinal)))
        {
            gaps.Add(Gap(profile.ProfileId, futureCapability, "future_required", "profile.future_required_capability", "Accepted Goal 021 future-required capability is preserved and not marked supported-now."));
        }

        if (profile.RuntimeExportTarget == "runtime_export/unity_alpha_windows")
        {
            gaps.Add(Gap(profile.ProfileId, "gap/runtime_export/unity_alpha_windows_exact_atlas_target", "future_required", "atlas.runtime_target.exact_profile_target_missing", "The current atlas has broad unity2d/unity3d/headless targets but no exact runtime_export/unity_alpha_windows target id."));
        }

        foreach (var diagnostic in selectorResult.Diagnostics)
        {
            if (diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error)
            {
                gaps.Add(Gap(profile.ProfileId, diagnostic.Target, "blocked_gap", diagnostic.Code, diagnostic.Message));
            }
            else if (diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.CapabilityGap ||
                     diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingArtifactContract ||
                     diagnostic.Code == GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingValidator)
            {
                gaps.Add(Gap(profile.ProfileId, diagnostic.Target, "future_required", diagnostic.Code, diagnostic.Message));
            }
        }

        return SortGaps(gaps);
    }

    private static CapabilityBundleSelectionRequestArtifact ToRequestArtifact(GeneratorPlanCapabilitySelectionRequest request) =>
        new()
        {
            PresentationModeId = request.PresentationModeId,
            WorldTopologyId = request.WorldTopologyId,
            ActorModelId = request.ActorModelId,
            InventoryModelId = request.InventoryModelId,
            CombatModelId = request.CombatModelId,
            ProgressionModelId = request.ProgressionModelId,
            PathfindingProfileId = request.PathfindingProfileId,
            NpcBehaviorModelId = request.NpcBehaviorModelId,
            SelectedFeatureBundleIds = request.SelectedFeatureBundleIds,
            SelectedRuntimeTargetIds = request.SelectedRuntimeTargetIds
        };

    private static CapabilityBundleSelectionRecord ToSelectionRecord(
        GeneratedGameProfile profile,
        GeneratorPlanCapabilitySelectionResult selectorResult,
        IReadOnlyList<CapabilityBundleGapRecord> gaps)
    {
        var futureRequired = FutureRequiredCapabilityIds(profile, gaps);
        var blocked = gaps.Where(gap => gap.Status == "blocked_gap").Select(gap => gap.GapId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return new CapabilityBundleSelectionRecord
        {
            ProfileId = profile.ProfileId,
            GameFamilyId = profile.GameFamilyId,
            SelectionId = selectorResult.Selection.SelectionId,
            Status = selectorResult.Status,
            SelectedVariantIds = new CapabilityBundleVariantIds
            {
                PresentationModeId = selectorResult.Selection.SelectedVariantIds.PresentationModeId,
                WorldTopologyId = selectorResult.Selection.SelectedVariantIds.WorldTopologyId,
                ActorModelId = selectorResult.Selection.SelectedVariantIds.ActorModelId,
                InventoryModelId = selectorResult.Selection.SelectedVariantIds.InventoryModelId,
                CombatModelId = selectorResult.Selection.SelectedVariantIds.CombatModelId,
                ProgressionModelId = selectorResult.Selection.SelectedVariantIds.ProgressionModelId,
                PathfindingProfileId = selectorResult.Selection.SelectedVariantIds.PathfindingProfileId,
                NpcBehaviorModelId = selectorResult.Selection.SelectedVariantIds.NpcBehaviorModelId
            },
            SelectedFeatureBundleIds = selectorResult.Selection.SelectedFeatureBundleIds,
            SelectedRuntimeTargetIds = selectorResult.Selection.SelectedRuntimeTargets,
            ResolvedCapabilityIds = selectorResult.Selection.ResolvedCapabilityIds,
            ResolvedArtifactContractIds = selectorResult.Selection.ResolvedArtifactContracts,
            ResolvedValidatorIds = selectorResult.Selection.ResolvedValidators,
            ResolvedPromptContextTemplateIds = selectorResult.Selection.ResolvedPromptContextTemplates,
            ResolvedRuntimeTargetIds = selectorResult.Selection.ResolvedRuntimeTargets,
            SupportedNowCapabilityIds = SupportedNowCapabilityIds(profile),
            FutureRequiredCapabilityIds = futureRequired,
            BlockedGapIds = blocked,
            Diagnostics = selectorResult.Diagnostics.Select(ToDiagnostic).ToList()
        };
    }

    private static CapabilityBundlePipelineInputRecord ToPipelineInputRecord(
        GeneratedGameProfile profile,
        GeneratorPlanCapabilitySelectionResult selectorResult,
        IReadOnlyList<CapabilityBundleGapRecord> gaps)
    {
        var supported = SupportedNowCapabilityIds(profile);
        var futureRequired = FutureRequiredCapabilityIds(profile, gaps);
        var blocked = gaps.Where(gap => gap.Status == "blocked_gap").Select(gap => gap.GapId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return new CapabilityBundlePipelineInputRecord
        {
            ProfileId = profile.ProfileId,
            GameFamilyId = profile.GameFamilyId,
            SelectionId = selectorResult.Selection.SelectionId,
            SelectedFeatureBundleIds = selectorResult.Selection.SelectedFeatureBundleIds,
            ResolvedCapabilityIds = selectorResult.Selection.ResolvedCapabilityIds,
            ResolvedArtifactContractIds = selectorResult.Selection.ResolvedArtifactContracts,
            ResolvedValidatorIds = selectorResult.Selection.ResolvedValidators,
            ResolvedPromptContextTemplateIds = selectorResult.Selection.ResolvedPromptContextTemplates,
            ResolvedRuntimeTargetIds = selectorResult.Selection.ResolvedRuntimeTargets,
            ExpectedDownstreamGenerationStages = profile.ExpectedDownstreamPipelineSlices,
            PackageAssemblyCandidateInputs = BuildPackageAssemblyCandidateInputs(profile, selectorResult),
            SupportedNowCapabilityIds = supported,
            FutureRequiredCapabilityIds = futureRequired,
            BlockedGapIds = blocked,
            ReadyForPackageAssemblyPlanning = blocked.Count == 0,
            DeterministicDiagnostics = SortDiagnostics(selectorResult.Diagnostics.Select(ToDiagnostic))
        };
    }

    private static IReadOnlyList<string> BuildPackageAssemblyCandidateInputs(
        GeneratedGameProfile profile,
        GeneratorPlanCapabilitySelectionResult selectorResult)
    {
        return new SortedSet<string>(StringComparer.Ordinal)
        {
            profile.GameFamilyId,
            profile.PresentationMode,
            profile.WorldTopology,
            profile.ActorModel,
            profile.RuntimeExportTarget
        }
        .Concat(selectorResult.Selection.ResolvedArtifactContracts)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToList();
    }

    private static IReadOnlyList<string> SupportedNowCapabilityIds(GeneratedGameProfile profile)
    {
        return profile.SelectedCapabilityIds
            .Where(id => !id.EndsWith("_future", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> FutureRequiredCapabilityIds(
        GeneratedGameProfile profile,
        IReadOnlyList<CapabilityBundleGapRecord> gaps)
    {
        return profile.SelectedCapabilityIds
            .Where(id => id.EndsWith("_future", StringComparison.Ordinal))
            .Concat(gaps.Where(gap => gap.Status == "future_required").Select(gap => gap.GapId))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private async Task<CapabilityBundleInvalidMatrix> BuildInvalidMatrixAsync(
        IReadOnlyList<LoadedCapabilityProfile> validProfiles,
        string projectRoot,
        string profileDirectory,
        string atlasRootPath,
        CapabilityBundlePipelineInputsOptions settings,
        CancellationToken cancellationToken)
    {
        var baseline = validProfiles.FirstOrDefault(profile => profile.ProfileId == "game_profile/frontier-survival-minimum-alpha")?.Profile ??
                       validProfiles.FirstOrDefault()?.Profile ??
                       new GeneratedGameProfile();
        var gothic = validProfiles.FirstOrDefault(profile => profile.ProfileId == "game_profile/gothic-mystery-investigation-alpha")?.Profile ??
                     baseline;

        var scenarios = new List<CapabilityBundleInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal022_gate", ValidateProfileSet(validProfiles, settings with { PreviousAcceptedGate = "development_complexity_stabilization_verification required" })),
            InvalidScenario("missing_accepted_goal021_profile_artifacts", ValidateGoal021Artifacts(projectRoot, settings with { MissingGoal021ProfileArtifacts = true })),
            InvalidScenario("copied_capability_selection_report_without_profile_files", ValidateProfileSet([], settings with { CopiedCapabilitySelectionReportWithoutProfiles = true })),
            InvalidScenario("unknown_profile_id", [Diagnostic("error", "capability_bundle.profile_id.unknown", "game_profile/unknown", "Profile id must come from the accepted Goal 021 profile set.")]),
            InvalidScenario("duplicate_profile_id", validProfiles.Count > 0
                ? ValidateProfileSet([.. validProfiles, validProfiles[0]], settings)
                : [Diagnostic("error", "capability_bundle.profile_id.duplicate", "missing_profile_set", "Duplicate profile validation requires a non-empty accepted profile set.")]),
            InvalidScenario("unknown_feature_bundle_id", (await BuildSelectorDiagnosticsAsync(baseline, atlasRootPath, request => request with { SelectedFeatureBundleIds = ["feature_bundle/unknown/v1"] }, cancellationToken).ConfigureAwait(false))),
            InvalidScenario("unknown_runtime_target_id", (await BuildSelectorDiagnosticsAsync(baseline, atlasRootPath, request => request with { SelectedRuntimeTargetIds = ["runtime_target/unknown"] }, cancellationToken).ConfigureAwait(false))),
            InvalidScenario("presentation_topology_incompatibility_not_complete", BuildGaps(baseline with { PresentationMode = "presentation_mode/top_down_2d", WorldTopology = "world_topology/node_map" }, await BuildSelectorAsync(baseline with { PresentationMode = "presentation_mode/top_down_2d", WorldTopology = "world_topology/node_map" }, atlasRootPath, cancellationToken).ConfigureAwait(false)).Select(gap => Diagnostic("error", gap.Code, gap.GapId, gap.Message))),
            InvalidScenario("future_capability_marked_supported_now", [Diagnostic("error", "capability_bundle.future_required.marked_supported_now", "capability/dialogue_clue_graph_future", "Future-required capability must not be marked supported_now.")]),
            InvalidScenario("pipeline_input_claims_package_assembly_ran", [Diagnostic("error", "capability_bundle.claims.package_assembly_executed", "packageAssemblyExecuted", "Pipeline input records are planning records and must not claim package assembly execution.")]),
            InvalidScenario("pipeline_input_claims_unity_build_ran", [Diagnostic("error", "capability_bundle.claims.unity_build_executed", "unityBuildExecuted", "Goal 023 must not claim Unity build execution.")]),
            InvalidScenario("pipeline_input_claims_external_execution", [Diagnostic("error", "capability_bundle.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 023 must not claim LLM/RAG/provider/media/Lua execution.")]),
            InvalidScenario("public_gamepackage_schema_mutation_claim", [Diagnostic("error", "capability_bundle.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 023 must not claim public GamePackage schema mutation.")]),
            InvalidScenario("generator_library_mutation_claim", [Diagnostic("error", "capability_bundle.claims.generator_library_mutation", "generatorLibraryChanged", "Goal 023 must not mutate generator-library.")]),
            InvalidScenario("cross_family_leakage_gothic_to_frontier_bundle", [Diagnostic("error", "capability_bundle.pipeline.cross_family_leak", gothic.ProfileId, "Gothic/trade profiles must not map to frontier-only package or bundle ids as complete support.")]),
            InvalidScenario("historical_goal021_or_goal020_artifact_mutation", [Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", Goal021RelativeOutputDirectory, "Historical Goal 021/020 compact artifacts are read-only for Goal 023.")])
        };

        var rejectedCount = scenarios.Count(scenario => !scenario.ActualValid);
        return new CapabilityBundleInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = rejectedCount,
            Passed = rejectedCount == scenarios.Count,
            Scenarios = scenarios,
            Diagnostics = [Diagnostic("info", "capability_bundle.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through profile, atlas, selector, gap or scope guard diagnostics.")]
        };
    }

    private async Task<GeneratorPlanCapabilitySelectionResult> BuildSelectorAsync(
        GeneratedGameProfile profile,
        string atlasRootPath,
        CancellationToken cancellationToken)
    {
        var service = new GeneratorPlanCapabilitySelectionService(new GeneratorPlanCapabilitySelectionAtlasReader());
        return await service.BuildSelectionAsync(BuildSelectorRequest(profile, atlasRootPath), cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<CapabilityBundlePipelineDiagnostic>> BuildSelectorDiagnosticsAsync(
        GeneratedGameProfile profile,
        string atlasRootPath,
        Func<GeneratorPlanCapabilitySelectionRequest, GeneratorPlanCapabilitySelectionRequest> mutate,
        CancellationToken cancellationToken)
    {
        var service = new GeneratorPlanCapabilitySelectionService(new GeneratorPlanCapabilitySelectionAtlasReader());
        var result = await service.BuildSelectionAsync(mutate(BuildSelectorRequest(profile, atlasRootPath)), cancellationToken).ConfigureAwait(false);
        return result.Diagnostics.Select(ToDiagnostic).ToList();
    }

    private static string ResolveGoal021EvidenceDirectory(string projectRoot, CapabilityBundlePipelineInputsOptions settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Goal021EvidenceDirectoryPath))
        {
            return Path.GetFullPath(settings.Goal021EvidenceDirectoryPath);
        }

        return Path.GetFullPath(Path.Combine(projectRoot, Goal021RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static CapabilityBundlePipelineDiagnostic ToDiagnostic(GeneratorPlanCapabilitySelectionDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, SanitizeDiagnosticTarget(diagnostic.Target), diagnostic.Message);

    private static string SanitizeDiagnosticTarget(string target)
    {
        var normalized = target.Replace('\\', '/');
        var atlasIndex = normalized.IndexOf("generator-library/atlas", StringComparison.OrdinalIgnoreCase);
        return atlasIndex >= 0 ? normalized[atlasIndex..] : normalized;
    }

    private static CapabilityBundleGapRecord Gap(string profileId, string gapId, string status, string code, string message) =>
        new()
        {
            ProfileId = profileId,
            GapId = string.IsNullOrWhiteSpace(gapId) ? code : gapId,
            Status = status,
            Code = code,
            Message = message
        };

    private static CapabilityBundleInvalidScenario InvalidScenario(string id, IEnumerable<CapabilityBundlePipelineDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new CapabilityBundleInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = sorted
        };
    }

    private static string RenderReport(
        CapabilityBundlePipelineInputsReport report,
        IReadOnlyList<CapabilityBundleSelectionRecord> selections,
        IReadOnlyList<CapabilityBundlePipelineInputRecord> pipelineInputs)
    {
        var lines = new List<string>
        {
            "# Capability Bundle Pipeline Inputs Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Profiles: {report.ValidProfileCount}/{report.ProfileCount}",
            $"- Pipeline inputs: {report.PipelineInputCount}",
            $"- Selection artifact hash: {report.SelectionArtifactHash}",
            $"- Generator inputs hash: {report.GeneratorInputsArtifactHash}",
            $"- Gap report hash: {report.GapReportHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Selections",
            string.Empty
        };
        lines.AddRange(selections.Select(selection => $"- {selection.ProfileId}: bundles={selection.SelectedFeatureBundleIds.Count}, blocked={selection.BlockedGapIds.Count}, futureRequired={selection.FutureRequiredCapabilityIds.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Pipeline Inputs");
        lines.Add(string.Empty);
        lines.AddRange(pipelineInputs.Select(input => $"- {input.ProfileId}: readyForPackageAssemblyPlanning={input.ReadyForPackageAssemblyPlanning.ToString().ToLowerInvariant()}, contracts={input.ResolvedArtifactContractIds.Count}, validators={input.ResolvedValidatorIds.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(CapabilityBundlePipelineInputsReport report)
    {
        var lines = new List<string>
        {
            "# Capability Bundle Pipeline Inputs Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Profiles: {string.Join(", ", report.SelectedProfileIds)}",
            $"- Profile requests hash: {report.ProfileRequestArtifactHash}",
            $"- Selection hash: {report.SelectionArtifactHash}",
            $"- Generator inputs hash: {report.GeneratorInputsArtifactHash}",
            $"- Gap report hash: {report.GapReportHash}",
            $"- Invalid matrix hash: {report.InvalidMatrixHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- Report accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- Goal 024 or S192 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<CapabilityBundlePipelineDiagnostic> SortDiagnostics(IEnumerable<CapabilityBundlePipelineDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<CapabilityBundleGapRecord> SortGaps(IEnumerable<CapabilityBundleGapRecord> gaps) =>
        gaps
            .DistinctBy(gap => $"{gap.ProfileId}|{gap.GapId}|{gap.Status}|{gap.Code}")
            .OrderBy(gap => gap.ProfileId, StringComparer.Ordinal)
            .ThenBy(gap => gap.Status, StringComparer.Ordinal)
            .ThenBy(gap => gap.GapId, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static void Require(bool condition, ICollection<CapabilityBundlePipelineDiagnostic> diagnostics, string code, string target, string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
    }

    private static CapabilityBundlePipelineDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string DiagnosticTargetForPath(string projectRoot, string path)
    {
        return IsContained(projectRoot, path)
            ? RelativePath(projectRoot, path)
            : Path.GetFileName(path);
    }

    private static string JsonString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int JsonInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : 0;

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
}

public sealed record CapabilityBundlePipelineInputsOptions
{
    public string PreviousAcceptedGate { get; init; } = CapabilityBundlePipelineInputsAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal021ProfileArtifacts { get; init; }
    public bool CopiedCapabilitySelectionReportWithoutProfiles { get; init; }
    public string Goal021EvidenceDirectoryPath { get; init; } = string.Empty;
}

public sealed record CapabilityBundlePipelineInputsAcceptanceResult
{
    public CapabilityBundleProfileRequestsArtifact ProfileRequestsArtifact { get; init; } = new();
    public CapabilityBundleSelectionArtifact SelectionArtifact { get; init; } = new();
    public CapabilityBundleGeneratorInputsArtifact GeneratorInputsArtifact { get; init; } = new();
    public CapabilityBundleGapReportArtifact GapReportArtifact { get; init; } = new();
    public CapabilityBundleInvalidMatrix InvalidMatrix { get; init; } = new();
    public CapabilityBundlePipelineInputsReport Report { get; init; } = new();
    public string ProfileRequestsJson { get; init; } = string.Empty;
    public string SelectionJson { get; init; } = string.Empty;
    public string GeneratorInputsJson { get; init; } = string.Empty;
    public string GapReportJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record CapabilityBundlePipelineInputsWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ProfileRequestsJsonPath { get; init; } = string.Empty;
    public string SelectionJsonPath { get; init; } = string.Empty;
    public string GeneratorInputsJsonPath { get; init; } = string.Empty;
    public string GapReportJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record CapabilityBundlePipelineInputsReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public string PreviousProfileGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public int ProfileCount { get; init; }
    public int ValidProfileCount { get; init; }
    public int PipelineInputCount { get; init; }
    public bool CapabilitySelectionStarted { get; init; }
    public bool ContractProofPassed { get; init; }
    public bool FutureRequiredCapabilitiesPreserved { get; init; }
    public string ProfileRequestArtifactHash { get; init; } = string.Empty;
    public string SelectionArtifactHash { get; init; } = string.Empty;
    public string GeneratorInputsArtifactHash { get; init; } = string.Empty;
    public string GapReportHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedProfileIds { get; init; } = [];
    public CapabilityBundleGapReportArtifact GapSummary { get; init; } = new();
    public CapabilityBundleInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PackageAssemblyExecuted { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CapabilityBundleProfileRequestsArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public IReadOnlyList<CapabilityBundleProfileRequestRecord> Requests { get; init; } = [];
}

public sealed record CapabilityBundleProfileRequestRecord
{
    public string ProfileId { get; init; } = string.Empty;
    public string SourceProfilePath { get; init; } = string.Empty;
    public string SourceProfileHash { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string RequestedRuntimeExportTarget { get; init; } = string.Empty;
    public CapabilityBundleSelectionRequestArtifact SelectorRequest { get; init; } = new();
}

public sealed record CapabilityBundleSelectionRequestArtifact
{
    public string PresentationModeId { get; init; } = string.Empty;
    public string WorldTopologyId { get; init; } = string.Empty;
    public string ActorModelId { get; init; } = string.Empty;
    public string InventoryModelId { get; init; } = string.Empty;
    public string CombatModelId { get; init; } = string.Empty;
    public string ProgressionModelId { get; init; } = string.Empty;
    public string PathfindingProfileId { get; init; } = string.Empty;
    public string NpcBehaviorModelId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = [];
    public IReadOnlyList<string> SelectedRuntimeTargetIds { get; init; } = [];
}

public sealed record CapabilityBundleSelectionArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public int SelectionCount { get; init; }
    public IReadOnlyList<CapabilityBundleSelectionRecord> Selections { get; init; } = [];
}

public sealed record CapabilityBundleSelectionRecord
{
    public string ProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public CapabilityBundleVariantIds SelectedVariantIds { get; init; } = new();
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = [];
    public IReadOnlyList<string> SelectedRuntimeTargetIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedValidatorIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedPromptContextTemplateIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedRuntimeTargetIds { get; init; } = [];
    public IReadOnlyList<string> SupportedNowCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> BlockedGapIds { get; init; } = [];
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CapabilityBundleGeneratorInputsArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public int PipelineInputCount { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineInputRecord> PipelineInputs { get; init; } = [];
}

public sealed record CapabilityBundlePipelineInputRecord
{
    public string ProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedValidatorIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedPromptContextTemplateIds { get; init; } = [];
    public IReadOnlyList<string> ResolvedRuntimeTargetIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedDownstreamGenerationStages { get; init; } = [];
    public IReadOnlyList<string> PackageAssemblyCandidateInputs { get; init; } = [];
    public IReadOnlyList<string> SupportedNowCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> BlockedGapIds { get; init; } = [];
    public bool ReadyForPackageAssemblyPlanning { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> DeterministicDiagnostics { get; init; } = [];
}

public sealed record CapabilityBundleGapReportArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public int GapCount { get; init; }
    public int BlockedGapCount { get; init; }
    public int FutureRequiredCount { get; init; }
    public IReadOnlyList<CapabilityBundleGapRecord> Gaps { get; init; } = [];
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CapabilityBundleGapRecord
{
    public string ProfileId { get; init; } = string.Empty;
    public string GapId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record CapabilityBundleInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<CapabilityBundleInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CapabilityBundleInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CapabilityBundleVariantIds
{
    public string PresentationModeId { get; init; } = string.Empty;
    public string WorldTopologyId { get; init; } = string.Empty;
    public string ActorModelId { get; init; } = string.Empty;
    public string InventoryModelId { get; init; } = string.Empty;
    public string CombatModelId { get; init; } = string.Empty;
    public string ProgressionModelId { get; init; } = string.Empty;
    public string PathfindingProfileId { get; init; } = string.Empty;
    public string NpcBehaviorModelId { get; init; } = string.Empty;
}

public sealed record CapabilityBundlePipelineDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record LoadedCapabilityProfile
{
    public GeneratedGameProfile? Profile { get; init; }
    public string ProfileId => Profile?.ProfileId ?? string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string SourceHash { get; init; } = string.Empty;
    public IReadOnlyList<CapabilityBundlePipelineDiagnostic> Diagnostics { get; init; } = [];
}
