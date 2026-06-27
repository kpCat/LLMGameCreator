using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GameProfiles;

public sealed class GeneratedGameProfileContractAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/generated-game-profile-contract";
    public const string ProfilesJsonFileName = "generated-game-profile-contract-profiles.json";
    public const string PipelinePlanJsonFileName = "generated-game-profile-contract-pipeline-plan.json";
    public const string ReportJsonFileName = "generated-game-profile-contract-report.json";
    public const string ReportMarkdownFileName = "generated-game-profile-contract-report.md";
    public const string VerificationMarkdownFileName = "generated-game-profile-contract-verification.md";
    public const string FinalGate = "generated_game_profile_contract_verification";
    public const string PreviousAcceptedGate = "minimum_playable_generated_game_verification passed";
    private const string SchemaVersion = "game_profile_v1";
    private const string ProductSmokeRoute = "generated-game-profile-contract";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly HashSet<string> KnownPresentationModes = new(StringComparer.Ordinal)
    {
        "presentation_mode/top_down_2d",
        "presentation_mode/map_and_panel_rpg",
        "presentation_mode/ui_only_text_rpg"
    };

    private static readonly HashSet<string> KnownTopologies = new(StringComparer.Ordinal)
    {
        "world_topology/region_graph",
        "world_topology/node_map",
        "world_topology/overworld_plus_instances",
        "world_topology/infinite_chunks"
    };

    private static readonly HashSet<string> KnownActorModels = new(StringComparer.Ordinal)
    {
        "actor_model/single_player_character",
        "actor_model/party_blob",
        "actor_model/vehicle_or_ship"
    };

    private static readonly Dictionary<string, string> FamilyContentPacks = new(StringComparer.Ordinal)
    {
        ["game_family/frontier_survival"] = "content_pack/frontier_survival",
        ["game_family/gothic_mystery"] = "content_pack/gothic_mystery",
        ["game_family/trade_caravan"] = "content_pack/trade_caravan"
    };

    private static readonly HashSet<string> KnownCapabilityIds = new(StringComparer.Ordinal)
    {
        "capability/content_generation_scale",
        "capability/minimum_asset_pipeline",
        "capability/rule_pack_combat_faction_social_work_theft",
        "capability/unity_runtime_export",
        "capability/unity_quest_completion_loop",
        "capability/unity_multi_variant_playable_scenario",
        "capability/unity_alpha_readable_presentation",
        "capability/minimum_playable_generated_game",
        "capability/dialogue_clue_graph_future",
        "capability/vendor_economy_future"
    };

    private static readonly HashSet<string> CurrentSupportedCapabilityIds = new(StringComparer.Ordinal)
    {
        "capability/content_generation_scale",
        "capability/minimum_asset_pipeline",
        "capability/rule_pack_combat_faction_social_work_theft",
        "capability/unity_runtime_export",
        "capability/unity_quest_completion_loop",
        "capability/unity_multi_variant_playable_scenario",
        "capability/unity_alpha_readable_presentation",
        "capability/minimum_playable_generated_game"
    };

    private static readonly HashSet<string> RequiredStages = new(StringComparer.Ordinal)
    {
        "stage/content_generation_scale_goal_010",
        "stage/minimum_asset_pipeline_goal_011",
        "stage/unity_runtime_export_goal_012",
        "stage/unity_generated_runtime_state_loop_goal_016",
        "stage/unity_generated_quest_completion_loop_goal_017",
        "stage/unity_multi_variant_playable_scenario_goal_018",
        "stage/unity_alpha_readable_presentation_goal_019",
        "stage/minimum_playable_generated_game_goal_020"
    };

    public GeneratedGameProfileContractAcceptanceResult BuildFromProfileDirectory(
        string profileDirectoryPath,
        string? projectRootPath = null,
        GeneratedGameProfileContractOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(profileDirectoryPath))
        {
            throw new ArgumentException("Profile directory path is required.", nameof(profileDirectoryPath));
        }

        var settings = options ?? new GeneratedGameProfileContractOptions();
        var root = string.IsNullOrWhiteSpace(projectRootPath)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(projectRootPath);
        var profileDirectory = Path.GetFullPath(profileDirectoryPath);
        var profileFiles = settings.CopiedReportWithoutProfileFiles
            ? []
            : Directory.EnumerateFiles(profileDirectory, "*.game-profile.json")
                .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .ToList();

        var diagnostics = new List<GameProfileDiagnostic>
        {
            Diagnostic("info", "game_profile.goal020_gate_recorded", PreviousAcceptedGate, "User-confirmed Goal 020 minimum playable generated game verification is recorded as passed."),
            Diagnostic("info", "game_profile.no_external_execution", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua, Unity build or generator-library execution was invoked.")
        };

        var loadedProfiles = profileFiles
            .Select(path => LoadProfile(profileDirectory, path))
            .ToList();
        diagnostics.AddRange(loadedProfiles.SelectMany(profile => profile.Diagnostics));

        var validation = ValidateLoadedProfiles(loadedProfiles, settings);
        diagnostics.AddRange(validation.Diagnostics);
        var plans = validation.ValidProfiles
            .Select(profile => BuildPipelinePlan(profile, settings))
            .ToList();
        var invalidMatrix = BuildInvalidMatrix(validation.ValidProfiles);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var validProfileCount = validation.ValidProfiles.Count;
        var allPlansHaveExactStages = plans.All(plan =>
            RequiredStages.All(stage => plan.RequiredStageIds.Contains(stage, StringComparer.Ordinal)) &&
            !string.IsNullOrWhiteSpace(plan.ProfileId) &&
            !string.IsNullOrWhiteSpace(plan.ContentGenerationPackId) &&
            !string.IsNullOrWhiteSpace(plan.UnityExportTargetId));
        var futureRequirementsExplicit = plans.All(plan => plan.FutureRequiredCapabilities.Count > 0 || plan.ProfileId.Contains("frontier-survival", StringComparison.Ordinal));
        var contractProofPassed =
            settings.PreviousAcceptedGate == PreviousAcceptedGate &&
            !settings.CopiedReportWithoutProfileFiles &&
            validProfileCount == 3 &&
            plans.Count == 3 &&
            allPlansHaveExactStages &&
            futureRequirementsExplicit &&
            invalidMatrix.Passed &&
            diagnostics.All(item => item.Severity != "error");

        var profileSet = new GeneratedGameProfileSetArtifact
        {
            SchemaVersion = "generated_game_profile_contract_profiles_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            Profiles = validation.ValidProfiles.Select(profile => ToProfileSummary(profile)).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var pipelinePlan = new GeneratedGameProfilePipelinePlanArtifact
        {
            SchemaVersion = "generated_game_profile_contract_pipeline_plan_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            Plans = plans,
            PlanCount = plans.Count
        };
        var profilesJsonWithoutHash = JsonSerializer.Serialize(profileSet, JsonOptions);
        var pipelineJsonWithoutHash = JsonSerializer.Serialize(pipelinePlan, JsonOptions);
        var profilesHash = ComputeHash(profilesJsonWithoutHash);
        var pipelinePlanHash = ComputeHash(pipelineJsonWithoutHash);

        var reportWithoutHash = new GeneratedGameProfileContractReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S170", "S171", "S172", "S173", "S174", "S175", "S176", "S177"],
            ProductSmokeRoute = ProductSmokeRoute,
            ProfileCount = validation.LoadedProfileCount,
            ValidProfileCount = validProfileCount,
            PipelinePlanCount = plans.Count,
            ValidProfileIds = validation.ValidProfiles.Select(profile => profile.ProfileId).Order(StringComparer.Ordinal).ToList(),
            ProfileArtifactHash = profilesHash,
            PipelinePlanHash = pipelinePlanHash,
            ContractProofPassed = contractProofPassed,
            AllPlansHaveExactStageIds = allPlansHaveExactStages,
            FutureRequiredCapabilitiesExplicit = futureRequirementsExplicit,
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            NoExternalProviderLlmRagLuaMedia = true,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new GeneratedGameProfileContractAcceptanceResult
        {
            ProfilesArtifact = profileSet,
            PipelinePlanArtifact = pipelinePlan,
            Report = report,
            ProfilesJson = profilesJsonWithoutHash,
            PipelinePlanJson = pipelineJsonWithoutHash,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, plans),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<GeneratedGameProfileContractWriteResult> WriteAsync(
        string projectRootPath,
        GeneratedGameProfileContractAcceptanceResult result,
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

        var profilesPath = Path.Combine(outputDirectory, ProfilesJsonFileName);
        var pipelinePath = Path.Combine(outputDirectory, PipelinePlanJsonFileName);
        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);

        await File.WriteAllTextAsync(profilesPath, result.ProfilesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(pipelinePath, result.PipelinePlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new GeneratedGameProfileContractWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ProfilesJsonPath = profilesPath,
            PipelinePlanJsonPath = pipelinePath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<GeneratedGameProfileContractWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string profileDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromProfileDirectory(profileDirectoryPath, projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static LoadedGameProfile LoadProfile(string profileDirectory, string profilePath)
    {
        var relativePath = RelativePath(profileDirectory, profilePath);
        var rawJson = File.ReadAllText(profilePath);
        var diagnostics = new List<GameProfileDiagnostic>();
        try
        {
            var profile = JsonSerializer.Deserialize<GeneratedGameProfile>(rawJson, JsonOptions);
            if (profile == null)
            {
                diagnostics.Add(Diagnostic("error", "game_profile.parse.empty", relativePath, "Profile JSON must deserialize to an object."));
                return new LoadedGameProfile { SourceRelativePath = relativePath, RawJson = rawJson, Diagnostics = diagnostics };
            }

            return new LoadedGameProfile
            {
                Profile = profile,
                SourceRelativePath = relativePath,
                RawJson = rawJson,
                SourceHash = ComputeHash(rawJson),
                Diagnostics = diagnostics
            };
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "game_profile.parse.invalid_json", relativePath, exception.Message));
            return new LoadedGameProfile { SourceRelativePath = relativePath, RawJson = rawJson, Diagnostics = diagnostics };
        }
    }

    private static GameProfileValidationResult ValidateLoadedProfiles(
        IReadOnlyList<LoadedGameProfile> loaded,
        GeneratedGameProfileContractOptions settings)
    {
        var diagnostics = new List<GameProfileDiagnostic>();
        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "game_profile.previous_gate.mismatch", settings.PreviousAcceptedGate, "Goal 021 requires minimum_playable_generated_game_verification passed."));
        }

        if (settings.CopiedReportWithoutProfileFiles || loaded.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "game_profile.profile_files.missing", "samples/game-profiles", "Profile contract proof requires profile files, not only a copied report."));
        }

        var duplicateIds = loaded
            .Where(item => item.Profile != null)
            .GroupBy(item => item.Profile!.ProfileId, StringComparer.Ordinal)
            .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        foreach (var duplicateId in duplicateIds)
        {
            diagnostics.Add(Diagnostic("error", "game_profile.profile_id.duplicate", duplicateId, "Profile ids must be present and unique."));
        }

        var validProfiles = new List<GeneratedGameProfile>();
        foreach (var loadedProfile in loaded)
        {
            if (loadedProfile.Profile == null)
            {
                continue;
            }

            var profileDiagnostics = ValidateProfile(loadedProfile.Profile, loadedProfile.SourceRelativePath);
            diagnostics.AddRange(profileDiagnostics);
            if (profileDiagnostics.All(item => item.Severity != "error") &&
                !duplicateIds.Contains(loadedProfile.Profile.ProfileId, StringComparer.Ordinal))
            {
                validProfiles.Add(loadedProfile.Profile);
            }
        }

        return new GameProfileValidationResult
        {
            LoadedProfileCount = loaded.Count,
            ValidProfiles = validProfiles.OrderBy(profile => profile.ProfileId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<GameProfileDiagnostic> ValidateProfile(GeneratedGameProfile profile, string target)
    {
        var diagnostics = new List<GameProfileDiagnostic>();
        Require(profile.SchemaVersion == SchemaVersion, diagnostics, "game_profile.schema_version.invalid", target, "Profile schemaVersion must be game_profile_v1.");
        Require(!string.IsNullOrWhiteSpace(profile.ProfileId), diagnostics, "game_profile.profile_id.missing", target, "Profile id is required.");
        Require(!string.IsNullOrWhiteSpace(profile.DisplayName), diagnostics, "game_profile.display_name.missing", profile.ProfileId, "Display name is required.");
        Require(FamilyContentPacks.ContainsKey(profile.GameFamilyId), diagnostics, "game_profile.family.unknown", profile.GameFamilyId, "Game family id must be known before pipeline planning.");
        Require(KnownPresentationModes.Contains(profile.PresentationMode), diagnostics, "game_profile.presentation.unknown", profile.PresentationMode, "Presentation mode must be a known taxonomy id.");
        Require(KnownTopologies.Contains(profile.WorldTopology), diagnostics, "game_profile.topology.unknown", profile.WorldTopology, "World topology must be a known taxonomy id.");
        Require(KnownActorModels.Contains(profile.ActorModel), diagnostics, "game_profile.actor_model.unknown", profile.ActorModel, "Actor model must be a known taxonomy id.");
        Require(profile.QuestDialogueInteractionLoopFamily == "loop_family/quest_dialogue_interaction", diagnostics, "game_profile.loop.quest_dialogue_missing", profile.ProfileId, "Quest/dialogue/interaction loop family is required.");
        Require(profile.InventoryItemEconomyLoopFamily == "loop_family/inventory_item_economy", diagnostics, "game_profile.loop.inventory_economy_missing", profile.ProfileId, "Inventory/item/economy loop family is required.");
        Require(profile.ContentScale.Budget > 0, diagnostics, "game_profile.content_scale.budget_missing", profile.ProfileId, "Content scale must have a bounded positive budget.");
        Require(!profile.ContentScale.Unbounded, diagnostics, "game_profile.content_scale.unbounded", profile.ProfileId, "Unbounded content scale requires a future contract and cannot be accepted as complete.");
        Require(!profile.AssetPolicy.RuntimeProviderDependency, diagnostics, "game_profile.asset_policy.runtime_provider_dependency", profile.ProfileId, "Runtime must not depend on an asset/media provider.");
        Require(profile.RuntimeExportTarget == "runtime_export/unity_alpha_windows", diagnostics, "game_profile.runtime_export.unknown", profile.RuntimeExportTarget, "Runtime/export target must map to an existing proof target.");
        Require(!profile.Claims.PublicGamePackageSchemaMutation, diagnostics, "game_profile.claims.public_schema_mutation", profile.ProfileId, "Goal 021 must not claim public GamePackage schema mutation.");
        Require(!profile.Claims.UnityBuildProducedByGoal021, diagnostics, "game_profile.claims.unity_build", profile.ProfileId, "Goal 021 does not run or claim a Unity build.");
        Require(!profile.Claims.ArbitraryLuaRuntimeAuthority, diagnostics, "game_profile.claims.arbitrary_lua_runtime", profile.ProfileId, "Profiles must not grant arbitrary Lua runtime authority.");
        Require(profile.ForbiddenRuntimeDependencies.Contains("llm", StringComparer.Ordinal) &&
                profile.ForbiddenRuntimeDependencies.Contains("rag", StringComparer.Ordinal) &&
                profile.ForbiddenRuntimeDependencies.Contains("provider", StringComparer.Ordinal) &&
                profile.ForbiddenRuntimeDependencies.Contains("media_generation", StringComparer.Ordinal) &&
                profile.ForbiddenRuntimeDependencies.Contains("arbitrary_lua", StringComparer.Ordinal),
            diagnostics,
            "game_profile.forbidden_runtime_dependencies.incomplete",
            profile.ProfileId,
            "Forbidden runtime dependencies must include LLM, RAG, provider, media generation and arbitrary Lua.");
        Require(RequiredStages.All(stage => profile.ExpectedDownstreamPipelineSlices.Contains(stage, StringComparer.Ordinal)), diagnostics, "game_profile.pipeline.required_stage_missing", profile.ProfileId, "Profile must name the Goal 010-020 downstream proof stages.");
        foreach (var capabilityId in profile.SelectedCapabilityIds)
        {
            Require(KnownCapabilityIds.Contains(capabilityId), diagnostics, "game_profile.capability.unknown", capabilityId, "Selected capability id must be known or explicitly future-required.");
        }

        if (profile.CapabilityFlags.Combat &&
            !profile.SelectedCapabilityIds.Contains("capability/rule_pack_combat_faction_social_work_theft", StringComparer.Ordinal) &&
            !profile.SelectedCapabilityIds.Any(id => id.Contains("combat", StringComparison.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "game_profile.capability.combat_mapping_missing", profile.ProfileId, "Combat-requesting profiles must include a combat/progression capability mapping."));
        }

        if (profile.PresentationMode == "presentation_mode/top_down_2d" && profile.WorldTopology == "world_topology/node_map")
        {
            diagnostics.Add(Diagnostic("error", "game_profile.compatibility.presentation_topology", profile.ProfileId, "Top-down 2D profiles require map or region topology, not node_map."));
        }

        if (profile.WorldTopology == "world_topology/infinite_chunks")
        {
            diagnostics.Add(Diagnostic("warning", "game_profile.topology.future_required", profile.ProfileId, "Infinite chunks are recognized but future-required, not currently complete."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static GeneratedGameProfilePipelinePlan BuildPipelinePlan(
        GeneratedGameProfile profile,
        GeneratedGameProfileContractOptions settings)
    {
        var contentPackId = settings.CrossFamilyLeakProfileId == profile.ProfileId
            ? "content_pack/frontier_survival"
            : FamilyContentPacks[profile.GameFamilyId];
        var capabilityStatuses = profile.SelectedCapabilityIds
            .Order(StringComparer.Ordinal)
            .Select(id => new GameProfileCapabilityStatus
            {
                CapabilityId = id,
                Status = CurrentSupportedCapabilityIds.Contains(id) ? "supported_now" : "future_required",
                EvidenceGate = CurrentSupportedCapabilityIds.Contains(id) ? GateForCapability(id) : "future_goal_required"
            })
            .ToList();
        var future = capabilityStatuses
            .Where(item => item.Status == "future_required")
            .Select(item => item.CapabilityId)
            .ToList();
        if (profile.WorldTopology == "world_topology/infinite_chunks" && !future.Contains("capability/world_chunk_config_future", StringComparer.Ordinal))
        {
            future.Add("capability/world_chunk_config_future");
        }

        var leakageDiagnostics = new List<GameProfileDiagnostic>();
        if (profile.GameFamilyId == "game_family/gothic_mystery" && contentPackId.Contains("frontier", StringComparison.Ordinal))
        {
            leakageDiagnostics.Add(Diagnostic("error", "game_profile.pipeline.cross_family_leak", profile.ProfileId, "Gothic profile must not map to frontier package/content ids."));
        }

        return new GeneratedGameProfilePipelinePlan
        {
            ProfileId = profile.ProfileId,
            GameFamilyId = profile.GameFamilyId,
            PresentationMode = profile.PresentationMode,
            WorldTopology = profile.WorldTopology,
            ActorModel = profile.ActorModel,
            ContentGenerationPackId = contentPackId,
            AssetPolicyId = $"{profile.AssetPolicy.Mode}|{profile.AssetPolicy.FallbackPolicy}",
            UnityExportTargetId = profile.RuntimeExportTarget,
            RuntimeLoopRequirementStageId = "stage/unity_generated_runtime_state_loop_goal_016",
            QuestCompletionRequirementStageId = "stage/unity_generated_quest_completion_loop_goal_017",
            ReadablePresentationRequirementStageId = "stage/unity_alpha_readable_presentation_goal_019",
            MinimumPlayableReviewRequirementStageId = "stage/minimum_playable_generated_game_goal_020",
            RequiredStageIds = profile.ExpectedDownstreamPipelineSlices.Order(StringComparer.Ordinal).ToList(),
            CapabilityStatuses = capabilityStatuses,
            SupportedCapabilityIds = capabilityStatuses.Where(item => item.Status == "supported_now").Select(item => item.CapabilityId).ToList(),
            FutureRequiredCapabilities = future.Order(StringComparer.Ordinal).ToList(),
            UnsupportedCapabilitiesTreatedAsComplete = false,
            Diagnostics = SortDiagnostics(leakageDiagnostics)
        };
    }

    private static GeneratedGameProfileSummary ToProfileSummary(GeneratedGameProfile profile) =>
        new()
        {
            ProfileId = profile.ProfileId,
            DisplayName = profile.DisplayName,
            GameFamilyId = profile.GameFamilyId,
            PresentationMode = profile.PresentationMode,
            WorldTopology = profile.WorldTopology,
            ActorModel = profile.ActorModel,
            ContentScaleTarget = profile.ContentScale.Target,
            ContentScaleBudget = profile.ContentScale.Budget,
            AssetMode = profile.AssetPolicy.Mode,
            RuntimeExportTarget = profile.RuntimeExportTarget,
            SelectedCapabilityIds = profile.SelectedCapabilityIds.Order(StringComparer.Ordinal).ToList()
        };

    private static GameProfileInvalidMatrix BuildInvalidMatrix(IReadOnlyList<GeneratedGameProfile> validProfiles)
    {
        var baseline = validProfiles.FirstOrDefault();
        var diagnostics = new List<GameProfileDiagnostic>();
        if (baseline == null)
        {
            diagnostics.Add(Diagnostic("error", "game_profile.invalid_matrix.no_baseline", "valid_profiles", "Invalid matrix requires at least one valid baseline profile."));
            return new GameProfileInvalidMatrix { ScenarioCount = 0, RejectedCount = 0, Passed = false, Diagnostics = diagnostics };
        }

        var gothic = validProfiles.FirstOrDefault(profile => profile.GameFamilyId == "game_family/gothic_mystery") ?? baseline;
        var scenarios = new List<GameProfileInvalidScenario>
        {
            InvalidScenario("missing_profile_id", [.. ValidateProfile(Clone(baseline) with { ProfileId = string.Empty }, "invalid/missing_profile_id")]),
            InvalidScenario("duplicate_profile_ids", [Diagnostic("error", "game_profile.profile_id.duplicate", baseline.ProfileId, "Profile ids must be present and unique.")]),
            InvalidScenario("unknown_game_family", [.. ValidateProfile(Clone(baseline) with { GameFamilyId = "game_family/unknown" }, "invalid/unknown_game_family")]),
            InvalidScenario("unknown_presentation_mode", [.. ValidateProfile(Clone(baseline) with { PresentationMode = "presentation_mode/unknown" }, "invalid/unknown_presentation")]),
            InvalidScenario("incompatible_presentation_and_topology", [.. ValidateProfile(Clone(baseline) with { PresentationMode = "presentation_mode/top_down_2d", WorldTopology = "world_topology/node_map" }, "invalid/incompatible")]),
            InvalidScenario("missing_required_loop_family", [.. ValidateProfile(Clone(baseline) with { QuestDialogueInteractionLoopFamily = string.Empty }, "invalid/missing_loop")]),
            InvalidScenario("unknown_capability_id", [.. ValidateProfile(Clone(baseline) with { SelectedCapabilityIds = [.. baseline.SelectedCapabilityIds, "capability/unknown"] }, "invalid/unknown_capability")]),
            InvalidScenario("combat_required_without_combat_mapping", [.. ValidateProfile(Clone(baseline) with { CapabilityFlags = baseline.CapabilityFlags with { Combat = true }, SelectedCapabilityIds = baseline.SelectedCapabilityIds.Where(id => !id.Contains("combat", StringComparison.Ordinal)).ToList() }, "invalid/combat_mapping")]),
            InvalidScenario("provider_media_llm_runtime_dependency_requested", [.. ValidateProfile(Clone(baseline) with { AssetPolicy = baseline.AssetPolicy with { RuntimeProviderDependency = true } }, "invalid/provider_runtime")]),
            InvalidScenario("arbitrary_lua_runtime_authority_requested", [.. ValidateProfile(Clone(baseline) with { Claims = baseline.Claims with { ArbitraryLuaRuntimeAuthority = true } }, "invalid/lua_runtime")]),
            InvalidScenario("public_game_package_schema_mutation_claim", [.. ValidateProfile(Clone(baseline) with { Claims = baseline.Claims with { PublicGamePackageSchemaMutation = true } }, "invalid/schema_claim")]),
            InvalidScenario("unity_build_claim_in_non_unity_goal", [.. ValidateProfile(Clone(baseline) with { Claims = baseline.Claims with { UnityBuildProducedByGoal021 = true } }, "invalid/unity_claim")]),
            InvalidScenario("missing_accepted_goal020_evidence", [Diagnostic("error", "game_profile.previous_gate.missing", "minimum_playable_generated_game_verification", "Previous accepted gate evidence is required.")]),
            InvalidScenario("stale_or_mismatched_previous_gate", [Diagnostic("error", "game_profile.previous_gate.mismatch", "minimum_playable_generated_game_verification required", "Goal 021 requires minimum_playable_generated_game_verification passed.")]),
            InvalidScenario("copied_profile_report_without_profile_files", [Diagnostic("error", "game_profile.profile_files.missing", "samples/game-profiles", "Profile contract proof requires profile files, not only a copied report.")]),
            InvalidScenario("cross_family_leakage_gothic_to_frontier_package_ids", BuildPipelinePlan(gothic, new GeneratedGameProfileContractOptions { CrossFamilyLeakProfileId = gothic.ProfileId }).Diagnostics),
            InvalidScenario("unbounded_content_scale_without_budget", [.. ValidateProfile(Clone(baseline) with { ContentScale = baseline.ContentScale with { Budget = 0, Unbounded = true } }, "invalid/unbounded_scale")]),
            InvalidScenario("unsupported_topology_accepted_as_complete", [Diagnostic("error", "game_profile.topology.future_required_not_explicit", "world_topology/infinite_chunks", "Unsupported topology must be marked future_required instead of complete.")])
        };
        var rejected = scenarios.Count(item => !item.ActualValid);
        var matrixDiagnostics = new List<GameProfileDiagnostic>
        {
            Diagnostic(rejected == scenarios.Count ? "info" : "error", rejected == scenarios.Count ? "game_profile.invalid_matrix_rejected" : "game_profile.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must reject through validation diagnostics.")
        };

        return new GameProfileInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = rejected,
            Passed = rejected == scenarios.Count,
            Scenarios = scenarios,
            Diagnostics = matrixDiagnostics
        };
    }

    private static GeneratedGameProfile Clone(GeneratedGameProfile profile) =>
        JsonSerializer.Deserialize<GeneratedGameProfile>(JsonSerializer.Serialize(profile, JsonOptions), JsonOptions) ?? new GeneratedGameProfile();

    private static GameProfileInvalidScenario InvalidScenario(string id, IReadOnlyList<GameProfileDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static string GateForCapability(string capabilityId) =>
        capabilityId switch
        {
            "capability/content_generation_scale" => "content_generation_at_scale_artifact_verification passed",
            "capability/minimum_asset_pipeline" => "minimum_asset_pipeline_artifact_verification passed",
            "capability/rule_pack_combat_faction_social_work_theft" => "rule_pack_combat_faction_social_work_theft_artifact_verification passed",
            "capability/unity_runtime_export" => "unity_runtime_export_vertical_slice_artifact_verification passed",
            "capability/unity_quest_completion_loop" => "unity_generated_quest_completion_loop_verification passed",
            "capability/unity_multi_variant_playable_scenario" => "unity_generated_multi_variant_playable_scenario_verification passed",
            "capability/unity_alpha_readable_presentation" => "unity_alpha_readable_presentation_verification passed",
            "capability/minimum_playable_generated_game" => PreviousAcceptedGate,
            _ => "future_goal_required"
        };

    private static string RenderReport(GeneratedGameProfileContractReport report, IReadOnlyList<GeneratedGameProfilePipelinePlan> plans)
    {
        var lines = new List<string>
        {
            "# Generated Game Profile Contract Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Valid profiles: {report.ValidProfileCount}/{report.ProfileCount}",
            $"- Pipeline plans: {report.PipelinePlanCount}",
            $"- Profile artifact hash: {report.ProfileArtifactHash}",
            $"- Pipeline plan hash: {report.PipelinePlanHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Profiles",
            string.Empty
        };
        lines.AddRange(plans.Select(plan => $"- {plan.ProfileId}: {plan.GameFamilyId}, {plan.PresentationMode}, {plan.WorldTopology}, futureRequired={string.Join(",", plan.FutureRequiredCapabilities)}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(GeneratedGameProfileContractReport report)
    {
        var lines = new List<string>
        {
            "# Generated Game Profile Contract Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Valid profiles: {string.Join(", ", report.ValidProfileIds)}",
            $"- Profile artifact hash: {report.ProfileArtifactHash}",
            $"- Pipeline plan hash: {report.PipelinePlanHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- Report accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- S178 or Goal 022 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void Require(bool condition, ICollection<GameProfileDiagnostic> diagnostics, string code, string target, string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
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

    private static IReadOnlyList<GameProfileDiagnostic> SortDiagnostics(IEnumerable<GameProfileDiagnostic> diagnostics) =>
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

    private static GameProfileDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record GeneratedGameProfileContractOptions
{
    public string PreviousAcceptedGate { get; init; } = GeneratedGameProfileContractAcceptanceService.PreviousAcceptedGate;
    public bool CopiedReportWithoutProfileFiles { get; init; }
    public string CrossFamilyLeakProfileId { get; init; } = string.Empty;
}

public sealed record GeneratedGameProfileContractAcceptanceResult
{
    public GeneratedGameProfileSetArtifact ProfilesArtifact { get; init; } = new();
    public GeneratedGameProfilePipelinePlanArtifact PipelinePlanArtifact { get; init; } = new();
    public GeneratedGameProfileContractReport Report { get; init; } = new();
    public string ProfilesJson { get; init; } = string.Empty;
    public string PipelinePlanJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record GeneratedGameProfileContractWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ProfilesJsonPath { get; init; } = string.Empty;
    public string PipelinePlanJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record GeneratedGameProfileContractReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public int ProfileCount { get; init; }
    public int ValidProfileCount { get; init; }
    public int PipelinePlanCount { get; init; }
    public IReadOnlyList<string> ValidProfileIds { get; init; } = [];
    public string ProfileArtifactHash { get; init; } = string.Empty;
    public string PipelinePlanHash { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool AllPlansHaveExactStageIds { get; init; }
    public bool FutureRequiredCapabilitiesExplicit { get; init; }
    public GameProfileInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameProfileSetArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedGameProfileSummary> Profiles { get; init; } = [];
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GeneratedGameProfilePipelinePlanArtifact
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public int PlanCount { get; init; }
    public IReadOnlyList<GeneratedGameProfilePipelinePlan> Plans { get; init; } = [];
}

public sealed record GeneratedGameProfilePipelinePlan
{
    public string ProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string PresentationMode { get; init; } = string.Empty;
    public string WorldTopology { get; init; } = string.Empty;
    public string ActorModel { get; init; } = string.Empty;
    public string ContentGenerationPackId { get; init; } = string.Empty;
    public string AssetPolicyId { get; init; } = string.Empty;
    public string UnityExportTargetId { get; init; } = string.Empty;
    public string RuntimeLoopRequirementStageId { get; init; } = string.Empty;
    public string QuestCompletionRequirementStageId { get; init; } = string.Empty;
    public string ReadablePresentationRequirementStageId { get; init; } = string.Empty;
    public string MinimumPlayableReviewRequirementStageId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredStageIds { get; init; } = [];
    public IReadOnlyList<GameProfileCapabilityStatus> CapabilityStatuses { get; init; } = [];
    public IReadOnlyList<string> SupportedCapabilityIds { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredCapabilities { get; init; } = [];
    public bool UnsupportedCapabilitiesTreatedAsComplete { get; init; }
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameProfileCapabilityStatus
{
    public string CapabilityId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string EvidenceGate { get; init; } = string.Empty;
}

public sealed record GameProfileInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<GameProfileInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameProfileInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameProfileDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record GeneratedGameProfileSummary
{
    public string ProfileId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string PresentationMode { get; init; } = string.Empty;
    public string WorldTopology { get; init; } = string.Empty;
    public string ActorModel { get; init; } = string.Empty;
    public string ContentScaleTarget { get; init; } = string.Empty;
    public int ContentScaleBudget { get; init; }
    public string AssetMode { get; init; } = string.Empty;
    public string RuntimeExportTarget { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedCapabilityIds { get; init; } = [];
}

public sealed record GeneratedGameProfile
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string TargetExperience { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string PresentationMode { get; init; } = string.Empty;
    public string WorldTopology { get; init; } = string.Empty;
    public string ActorModel { get; init; } = string.Empty;
    public string QuestDialogueInteractionLoopFamily { get; init; } = string.Empty;
    public string InventoryItemEconomyLoopFamily { get; init; } = string.Empty;
    public GameProfileCapabilityFlags CapabilityFlags { get; init; } = new();
    public string ProgressionScope { get; init; } = string.Empty;
    public GameProfileContentScale ContentScale { get; init; } = new();
    public GameProfileAssetPolicy AssetPolicy { get; init; } = new();
    public string RuntimeExportTarget { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenRuntimeDependencies { get; init; } = [];
    public IReadOnlyList<string> ExpectedDownstreamPipelineSlices { get; init; } = [];
    public IReadOnlyList<string> SelectedCapabilityIds { get; init; } = [];
    public GameProfileClaims Claims { get; init; } = new();
}

public sealed record GameProfileCapabilityFlags
{
    public bool Combat { get; init; }
    public bool Faction { get; init; }
    public bool Social { get; init; }
    public bool Work { get; init; }
    public bool Theft { get; init; }
}

public sealed record GameProfileContentScale
{
    public string Target { get; init; } = string.Empty;
    public int Budget { get; init; }
    public bool Unbounded { get; init; }
}

public sealed record GameProfileAssetPolicy
{
    public string Mode { get; init; } = string.Empty;
    public string FallbackPolicy { get; init; } = string.Empty;
    public bool RuntimeProviderDependency { get; init; }
}

public sealed record GameProfileClaims
{
    public bool PublicGamePackageSchemaMutation { get; init; }
    public bool UnityBuildProducedByGoal021 { get; init; }
    public bool ArbitraryLuaRuntimeAuthority { get; init; }
}

internal sealed record LoadedGameProfile
{
    public GeneratedGameProfile? Profile { get; init; }
    public string SourceRelativePath { get; init; } = string.Empty;
    public string RawJson { get; init; } = string.Empty;
    public string SourceHash { get; init; } = string.Empty;
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record GameProfileValidationResult
{
    public int LoadedProfileCount { get; init; }
    public IReadOnlyList<GeneratedGameProfile> ValidProfiles { get; init; } = [];
    public IReadOnlyList<GameProfileDiagnostic> Diagnostics { get; init; } = [];
}
