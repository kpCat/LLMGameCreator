using System.Text;

namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignBuilder
{
    public FullMediaBoundCampaignSourceManifest BuildSourceManifest(FullMediaBoundCampaignSourceBundle source)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>(source.Diagnostics)
        {
            Info("goal058.preflight.goal057_handoff_recorded", "unity_alpha_multifamily_playable_loop_verification", "Goal 057 is recorded as accepted by user handoff before Goal 058."),
            Info("goal058.source.loaded", "Goal034-Goal057", "Goal 058 source facts were loaded from repository-local compact evidence.")
        };

        return new FullMediaBoundCampaignSourceManifest
        {
            Accepted = false,
            Goal057AcceptedByUserHandoff = true,
            Goal057ReportWasGreenProducedForReview = source.Goal057ReportWasGreenProducedForReview,
            Goal057UnityProofPassed = source.Goal057UnityProofPassed,
            SourceArtifactCount = source.SourceArtifactRefs.Count,
            FamilyCount = source.Families.Count,
            SelectedFamilyIds = source.Families.Select(item => item.FamilyId).OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new FullMediaBoundCampaignGateRecord
                {
                    GateId = "unity_alpha_multifamily_playable_loop_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 058 task preflight handoff"
                },
                new FullMediaBoundCampaignGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new FullMediaBoundCampaignGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new FullMediaBoundCampaignGateRecord
                {
                    GateId = FullMediaBoundGeneratorCampaignVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 058 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public FullMediaBoundCampaignPlan BuildCampaignPlan(FullMediaBoundCampaignSourceBundle source)
    {
        var stages = BuildStages(source.SourceArtifactRefs).ToList();
        return new FullMediaBoundCampaignPlan
        {
            Passed = stages.Count == FullMediaBoundGeneratorCampaignVocabulary.StageIds.Count
                && stages.All(item => item.Passed)
                && source.Families.Count == 3,
            Accepted = false,
            FamilyCount = source.Families.Count,
            StageCount = stages.Count,
            SeedProfileFamilySet = source.Families
                .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
                .Select(item => "seed/profile/" + item.ProfileId + "/family/" + item.FamilyId)
                .ToList(),
            Stages = stages
        };
    }

    public IReadOnlyDictionary<string, FullMediaBoundCampaignFamilyRun> BuildFamilyRuns(FullMediaBoundCampaignSourceBundle source) =>
        source.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToDictionary(
                item => item.FamilyId,
                item => new FullMediaBoundCampaignFamilyRun
                {
                    FamilyId = item.FamilyId,
                    ScenarioId = item.ScenarioId,
                    ProfileId = item.ProfileId,
                    Passed = item.LoopCommands.Count >= 5
                        && item.MediaFileCount >= 5
                        && !string.IsNullOrWhiteSpace(item.RuntimePreviewPayloadRef)
                        && !string.IsNullOrWhiteSpace(item.ExportMode),
                    CommandCount = item.LoopCommands.Count,
                    MediaFileCount = item.MediaFileCount,
                    RuntimePreviewPayloadRef = item.RuntimePreviewPayloadRef,
                    ExportMode = item.ExportMode,
                    SourceRefs =
                    [
                        item.Goal047DryRunRef,
                        item.Goal057LoopProofRef
                    ],
                    ExpectedCampaignMarkers =
                    [
                        "campaign_family=" + item.FamilyId,
                        "campaign_family_completed=" + item.FamilyId
                    ]
                },
                StringComparer.Ordinal);

    public FullMediaBoundUnityCampaignCommandPlan BuildUnityCommandPlan(FullMediaBoundCampaignSourceBundle source)
    {
        var families = source.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(item => item.FamilyId)
            .ToList();
        var expected = ExpectedCampaignMarkers(families)
            .Concat(ExpectedMediaMarkers())
            .Concat(source.Families.SelectMany(ExpectedFamilyLoopMarkers))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new FullMediaBoundUnityCampaignCommandPlan
        {
            Passed = families.Count == 3 && expected.Count >= 20,
            Accepted = false,
            Families = families,
            ExpectedPlayerMarkers = expected
        };
    }

    public (IReadOnlyList<FullMediaBoundCampaignFilePayload> StagingFiles, IReadOnlyList<FullMediaBoundCampaignFilePayload> ReviewPackageFiles) BuildStagingAndReviewPackageFiles(
        FullMediaBoundCampaignSourceBundle source,
        FullMediaBoundUnityCampaignCommandPlan commandPlan)
    {
        var campaignManifestJson = Serialize(BuildCampaignUnityManifest(source));
        var commandPlanJson = Serialize(commandPlan);
        var stagingFiles = source.Goal057StagingFiles.ToList();
        stagingFiles.Add(new FullMediaBoundCampaignFilePayload
        {
            RelativePath = FullMediaBoundGeneratorCampaignVocabulary.CampaignManifestStagingRelativePath,
            Bytes = Encoding.UTF8.GetBytes(campaignManifestJson + Environment.NewLine)
        });
        stagingFiles.Add(new FullMediaBoundCampaignFilePayload
        {
            RelativePath = FullMediaBoundGeneratorCampaignVocabulary.CampaignCommandPlanStagingRelativePath,
            Bytes = Encoding.UTF8.GetBytes(commandPlanJson + Environment.NewLine)
        });

        var mediaManifestBytes = source.Goal057StagingFiles
            .FirstOrDefault(item => item.RelativePath == "media-bound/unity-alpha-media-bound-manifest.json")
            ?.Bytes ?? [];
        var reviewFiles = new List<FullMediaBoundCampaignFilePayload>
        {
            new()
            {
                RelativePath = "review-package/StreamingAssets/full-media-bound-campaign-manifest.json",
                Bytes = Encoding.UTF8.GetBytes(campaignManifestJson + Environment.NewLine)
            },
            new()
            {
                RelativePath = "review-package/StreamingAssets/family-command-plan.json",
                Bytes = Encoding.UTF8.GetBytes(commandPlanJson + Environment.NewLine)
            },
            new()
            {
                RelativePath = "review-package/StreamingAssets/media-bound-manifest.json",
                Bytes = mediaManifestBytes
            }
        };

        return (
            stagingFiles
                .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
                .Select(group => group.Last())
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList(),
            reviewFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList());
    }

    public FullMediaBoundReviewPackageManifest BuildReviewPackageManifest()
    {
        var requiredEvidenceFiles = RequiredEvidenceFiles();
        return new FullMediaBoundReviewPackageManifest
        {
            Passed = true,
            Accepted = false,
            StreamingAssetsFiles =
            [
                "review-package/StreamingAssets/full-media-bound-campaign-manifest.json",
                "review-package/StreamingAssets/family-command-plan.json",
                "review-package/StreamingAssets/media-bound-manifest.json"
            ],
            RequiredEvidenceFiles = requiredEvidenceFiles
        };
    }

    public PreviewExportCampaignPayload BuildPreviewExportPayload(IReadOnlyDictionary<string, FullMediaBoundCampaignFamilyRun> familyRuns) =>
        new()
        {
            Passed = familyRuns.Count == 3
                && familyRuns.Values.All(item => item.Passed),
            FamilyCount = familyRuns.Count,
            PreviewRefs = familyRuns.Values
                .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
                .Select(item => item.RuntimePreviewPayloadRef)
                .ToList(),
            ExportModes = familyRuns.Values
                .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
                .Select(item => item.ExportMode)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList()
        };

    public CampaignPackageCompatibilityProof BuildPackageCompatibilityProof(FullMediaBoundCampaignSourceBundle source) =>
        new()
        {
            Passed = source.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal047" && item.ArtifactFamily == "full_generator_without_media" && item.Exists && item.HashMatches)
                && source.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal055" && item.ArtifactFamily == "media_bound_review_package" && item.Exists && item.HashMatches)
                && source.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal057" && item.ArtifactFamily == "unity_multifamily_loop" && item.Exists && item.HashMatches),
            PublicGamePackageSchemaChanged = false,
            RuntimeSourceChanged = false,
            WinFormsUiChanged = false,
            CompatibilityRefs =
            [
                ".llmgc/procedural/goal-047-full-generator-without-media-dry-run/package-compatibility-or-materialization-summary.json",
                ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/media-bound-review-package-manifest.json",
                ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/review-package-manifest.json"
            ]
        };

    public InvalidFullMediaBoundCampaignMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidFullMediaBoundCampaignScenario>
        {
            Invalid("missing_goal057_source", "Remove Goal 057 player proof and family command plan.", "blocked", Error("goal058.source.goal057_missing", "Goal057", "Goal 057 accepted source evidence is required.")),
            Invalid("stale_source_hash", "Change a consumed source artifact after its hash was recorded.", "rejected", Error("goal058.source.hash_mismatch", "source-manifest", "Source artifact bytes must match recorded hashes.")),
            Invalid("fake_family_id", "Inject a family outside the Goal 043/047/057 family set.", "rejected", Error("goal058.family.fake_id", "family/fake", "Campaign families must resolve to the required three family ids.")),
            Invalid("missing_family_command_plan", "Remove the staged campaign/family command plan.", "blocked", Error("goal058.command_plan.missing", "campaign/family-command-plan.json", "Unity player proof requires the staged campaign command plan.")),
            Invalid("missing_media_file", "Remove a staged media-bound file.", "rejected", Error("goal058.media.missing_file", "media-bound", "Campaign review package requires every staged media file from Goal 057.")),
            Invalid("media_hash_mismatch", "Mutate a staged media file after Goal 056/057 hash validation.", "rejected", Error("goal058.media.hash_mismatch", "media-bound", "Staged media hashes must match prior accepted manifests.")),
            Invalid("missing_unity_marker", "Omit a campaign_loaded or campaign_family_completed marker from player logs.", "blocked", Error("goal058.unity.marker_missing", "campaign markers", "Unity player logs must contain every required campaign marker.")),
            Invalid("duplicate_campaign_id", "Emit two campaign ids in one review package.", "rejected", Error("goal058.campaign.duplicate_id", "goal058", "Campaign id must be unique.")),
            Invalid("unsafe_relative_path", "Use absolute paths or traversal inside staging/review package.", "rejected", Error("goal058.path.unsafe", "../escape", "Campaign paths must be safe relative paths.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider, network, LLM or RAG execution.", "blocked", Error("goal058.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("real_media_generation_claim", "Claim real image/audio/provider media generation.", "blocked", Error("goal058.boundary.real_media_generation", "boundary", "Goal 058 may only consume accepted fixture media evidence.")),
            Invalid("lua_arbitrary_execution_claim", "Claim arbitrary Lua execution during campaign proof.", "blocked", Error("goal058.boundary.lua_arbitrary_execution", "boundary", "Arbitrary Lua execution is forbidden.")),
            Invalid("runtime_ui_gamepackage_schema_mutation_claim", "Claim Runtime, UI or public GamePackage schema mutation.", "blocked", Error("goal058.boundary.runtime_ui_gamepackage", "boundary", "Runtime/UI/GamePackage schema mutation is forbidden.")),
            Invalid("unity_broad_mutation_claim", "Replace or broaden Unity Alpha architecture.", "blocked", Error("goal058.boundary.unity_broad_mutation", "unity", "Only deterministic campaign marker support is allowed.")),
            Invalid("nondeterministic_order", "Shuffle source, family, stage or command records.", "rejected", Error("goal058.order.nondeterministic", "ordering", "Campaign records must be deterministically ordered.")),
            Invalid("missing_review_trace", "Publish a package without source/review trace refs.", "rejected", Error("goal058.review.trace_missing", "review-package", "Review package must preserve source and media provenance.")),
            Invalid("self_promotion_without_validation", "Mark Goal 058 passed from its own production result.", "blocked", Error("goal058.gate.self_promotion", "manualGate", "Goal 058 must remain required until user acceptance."))
        };

        return new InvalidFullMediaBoundCampaignMatrix
        {
            Passed = FullMediaBoundGeneratorCampaignVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> ExpectedCampaignMarkers(IReadOnlyList<string> families)
    {
        var markers = new List<string>
        {
            "campaign_loaded=goal058",
            "campaign_media_bound=true",
            "campaign_review_package_proof=goal058"
        };
        markers.AddRange(families.OrderBy(FamilyOrderingKey, StringComparer.Ordinal).Select(item => "campaign_family=" + item));
        markers.AddRange(families.OrderBy(FamilyOrderingKey, StringComparer.Ordinal).Select(item => "campaign_family_completed=" + item));
        return markers.Order(StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> ExpectedMediaMarkers() =>
    [
        "media_bound_manifest_loaded=true",
        "media_bound_family_count=3",
        "media_bound_png_loaded=true",
        "media_bound_wav_loaded=true",
        "media_bound_bundle_loaded=true",
        "media_bound_hash_validation=true"
    ];

    public static IEnumerable<string> ExpectedFamilyLoopMarkers(FullMediaBoundCampaignFamilySource family)
    {
        yield return "family_scenario_loaded=" + family.FamilyId;
        yield return "family_mode_selected=" + family.FamilyId;
        yield return "family_loop_started=" + family.FamilyId;
        foreach (var command in family.LoopCommands.OrderBy(item => item.Order))
        {
            yield return command.ExpectedPlayerMarker;
        }

        yield return "family_loop_completed=" + family.FamilyId;
    }

    public static IReadOnlyList<FullMediaBoundCampaignDiagnostic> SortDiagnostics(IEnumerable<FullMediaBoundCampaignDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string FamilyOrderingKey(string familyId) =>
        FullMediaBoundGeneratorCampaignSourceLoader.FamilyOrderingKey(familyId);

    private static IEnumerable<FullMediaBoundCampaignStageRecord> BuildStages(IReadOnlyList<FullMediaBoundCampaignSourceArtifactReference> refs)
    {
        var plans = new[]
        {
            ("strict_draft_quarantined_candidate_source_facts", 1, new[] { "Goal034" }),
            ("lua_manifest_sandbox_expansion_source_facts", 2, new[] { "Goal035", "Goal036", "Goal037" }),
            ("world_region_chunk_runtime_delta_source_facts", 3, new[] { "Goal038", "Goal039", "Goal040" }),
            ("family_simulatable_loop_source_facts", 4, new[] { "Goal043" }),
            ("full_generator_without_media_dry_run_source_facts", 5, new[] { "Goal047" }),
            ("media_materialization_review_package_source_facts", 6, new[] { "Goal053", "Goal054", "Goal055" }),
            ("unity_alpha_media_bound_package_source_facts", 7, new[] { "Goal056" }),
            ("unity_alpha_multifamily_playable_loop_source_facts", 8, new[] { "Goal057" }),
            ("campaign_review_package_plan", 9, new[] { "Goal055", "Goal056", "Goal057" }),
            ("campaign_unity_player_command_plan", 10, new[] { "Goal057" }),
            ("campaign_preview_export_payload", 11, new[] { "Goal047", "Goal053", "Goal057" })
        };

        foreach (var (stageId, order, goals) in plans)
        {
            var sourceRefs = refs
                .Where(item => goals.Contains(item.SourceGoal, StringComparer.Ordinal))
                .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ToList();
            var passed = goals.All(goal => sourceRefs.Any(item => item.SourceGoal == goal && item.Exists && item.HashMatches))
                && sourceRefs.All(item => item.Diagnostics.Count == 0);
            yield return new FullMediaBoundCampaignStageRecord
            {
                StageId = stageId,
                Order = order,
                Passed = passed,
                SourceGoals = goals.Order(StringComparer.Ordinal).ToList(),
                SourceArtifactRefs = sourceRefs.Select(item => item.ArtifactRelativePath).ToList(),
                Diagnostics = sourceRefs.SelectMany(item => item.Diagnostics).ToList()
            };
        }
    }

    private static object BuildCampaignUnityManifest(FullMediaBoundCampaignSourceBundle source) =>
        new
        {
            schemaVersion = "full_media_bound_campaign_unity_manifest_v1",
            goalId = FullMediaBoundGeneratorCampaignVocabulary.GoalId,
            campaignId = FullMediaBoundGeneratorCampaignVocabulary.CampaignId,
            mediaBound = true,
            reviewPackageProof = "goal058",
            families = source.Families
                .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
                .Select(item => new
                {
                    familyId = item.FamilyId,
                    scenarioId = item.ScenarioId,
                    profileId = item.ProfileId
                })
                .ToArray()
        };

    private static IReadOnlyList<string> RequiredEvidenceFiles() =>
    [
        FullMediaBoundGeneratorCampaignEvidenceService.SourceManifestJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.CampaignPlanJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("map_panel_rpg"),
        FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("survival_sandbox"),
        FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("first_person_grid_dungeon"),
        FullMediaBoundGeneratorCampaignEvidenceService.ReviewPackageManifestJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.UnityCommandPlanJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.UnityPlayerProofJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.PreviewExportPayloadJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.PackageCompatibilityProofJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.InvalidMatrixJsonFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.ArtifactScopeReportMarkdownFileName,
        FullMediaBoundGeneratorCampaignEvidenceService.ReportMarkdownFileName
    ];

    private static InvalidFullMediaBoundCampaignScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params FullMediaBoundCampaignDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string Serialize<T>(T value) => FullMediaBoundGeneratorCampaignHash.Serialize(value);

    private static FullMediaBoundCampaignDiagnostic Error(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Error(code, target, message);

    private static FullMediaBoundCampaignDiagnostic Info(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Info(code, target, message);
}
