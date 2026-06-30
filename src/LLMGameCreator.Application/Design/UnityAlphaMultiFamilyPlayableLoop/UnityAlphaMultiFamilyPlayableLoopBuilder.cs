using System.Text;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyPlayableLoopBuilder
{
    public UnityAlphaMultiFamilySourceManifest BuildSourceManifest(UnityAlphaMultiFamilySourceBundle source)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>(source.Diagnostics)
        {
            Info("goal057.preflight.goal056_handoff_recorded", "unity_alpha_media_bound_playable_package_verification", "Goal 056 is recorded as accepted by user handoff before Goal 057."),
            Info("goal057.source.loaded", "Goal043/Goal047/Goal055/Goal056", "Goal 057 source facts were loaded from repository-local evidence.")
        };

        return new UnityAlphaMultiFamilySourceManifest
        {
            Accepted = false,
            Goal056AcceptedByUserHandoff = true,
            Goal056ReportWasGreenProducedForReview = source.Goal056ReportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && source.Goal056ReportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && source.Goal056ReportMarkdown.Contains("manualGate=unity_alpha_media_bound_playable_package_verification", StringComparison.Ordinal),
            Goal056UnityProofPassed = source.Goal056LoadProof.Passed
                && source.Goal056SmokeLogSummary.Passed
                && source.Goal056SmokeLogSummary.UnityExitCode == 0
                && source.Goal056SmokeLogSummary.PlayerExitCode == 0,
            FamilyCount = source.Families.Count,
            SelectedFamilyIds = source.Families.Select(item => item.FamilyId).OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new UnityAlphaMultiFamilyGateRecord
                {
                    GateId = "unity_alpha_media_bound_playable_package_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 057 task preflight handoff"
                },
                new UnityAlphaMultiFamilyGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new UnityAlphaMultiFamilyGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new UnityAlphaMultiFamilyGateRecord
                {
                    GateId = UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 057 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public UnityAlphaFamilyModeManifest BuildFamilyModeManifest(
        IReadOnlyList<UnityAlphaMultiFamilySourceFamily> families,
        IReadOnlyList<UnityAlphaMediaBoundBinding> mediaBindings)
    {
        var records = families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                var bindingIds = mediaBindings
                    .Where(binding => binding.FamilyId == family.FamilyId)
                    .OrderBy(binding => binding.DeterministicOrderingKey, StringComparer.Ordinal)
                    .Select(binding => binding.BindingId)
                    .ToList();
                var expectedMarkers = ExpectedMarkersForFamily(family).ToList();
                return new UnityAlphaFamilyModeRecord
                {
                    FamilyId = family.FamilyId,
                    ModeId = "unity-alpha-family-mode/" + family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    ProfileId = family.ProfileId,
                    SelectionArgument = family.FamilyId,
                    RuntimePreviewPayloadRef = family.RuntimePreviewPayloadRef,
                    ExportMode = family.ExportMode,
                    SourceLoopRefs =
                    [
                        family.Goal043PlanRelativePath,
                        family.Goal043ProofRelativePath,
                        family.Goal047DryRunRelativePath
                    ],
                    StagedMediaBindingIds = bindingIds,
                    VisiblePanelRecords =
                    [
                        "family_panel/header/" + family.FamilyId,
                        "family_panel/loop_steps/" + family.FamilyId,
                        "family_panel/media_bound/" + family.FamilyId
                    ],
                    ExpectedMarkers = expectedMarkers
                };
            })
            .ToList();

        return new UnityAlphaFamilyModeManifest
        {
            Passed = records.Count == 3
                && records.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() == 3
                && records.All(item => item.StagedMediaBindingIds.Count >= 3)
                && records.All(item => item.ExpectedMarkers.Count >= 6),
            FamilyCount = records.Count,
            Families = records
        };
    }

    public UnityAlphaFamilyCommandPlan BuildFamilyCommandPlan(IReadOnlyList<UnityAlphaMultiFamilySourceFamily> families)
    {
        var modes = families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(item => new UnityAlphaFamilyCommandPlanMode
            {
                FamilyId = item.FamilyId,
                ModeId = "unity-alpha-family-mode/" + item.FamilyId,
                ScenarioId = item.ScenarioId,
                ProfileId = item.ProfileId
            })
            .ToList();
        var commands = families
            .SelectMany(item => item.LoopCommands)
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => item.Order)
            .ToList();
        var expected = ExpectedMediaMarkers()
            .Concat(modes.SelectMany(mode =>
            {
                var family = families.Single(item => item.FamilyId == mode.FamilyId);
                return ExpectedMarkersForFamily(family);
            }))
            .Append("review_package_proof=goal057")
            .Append("unity_alpha_multifamily_playable_loop_verification=required")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new UnityAlphaFamilyCommandPlan
        {
            Passed = modes.Count == 3
                && commands.Count >= 15
                && UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds.All(familyId => commands.Count(command => command.FamilyId == familyId) >= 5),
            Accepted = false,
            FamilyModes = modes,
            Commands = commands,
            ExpectedPlayerMarkers = expected
        };
    }

    public (UnityAlphaMultiFamilyStagingManifest Manifest, IReadOnlyList<UnityAlphaMultiFamilyFilePayload> StagingFiles) BuildUnityStaging(
        UnityAlphaMultiFamilySourceBundle source,
        UnityAlphaFamilyCommandPlan commandPlan)
    {
        var stagingFiles = source.Goal056StagingFiles.ToList();
        var commandPlanJson = Serialize(commandPlan);
        stagingFiles.Add(new UnityAlphaMultiFamilyFilePayload
        {
            RelativePath = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath,
            Bytes = Encoding.UTF8.GetBytes(commandPlanJson + Environment.NewLine)
        });

        var withoutHash = new UnityAlphaMultiFamilyStagingManifest
        {
            Passed = source.Goal056StagingFiles.Count > 0
                && source.Goal056StagingFiles.Any(item => item.RelativePath == UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath)
                && commandPlan.Passed,
            CopiedGoal056StagingFileCount = source.Goal056StagingFiles.Count,
            PhysicalMediaFileCount = source.Goal056StagingManifest.PhysicalMediaFileCount,
            PngFileCount = source.Goal056StagingManifest.PngFileCount,
            WavFileCount = source.Goal056StagingManifest.WavFileCount,
            BundleFileCount = source.Goal056StagingManifest.BundleFileCount,
            FamilyCount = source.Goal056StagingManifest.FamilyCount
        };

        return (
            withoutHash with { DeterministicHash = Hash(Serialize(withoutHash)) },
            stagingFiles
                .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
                .Select(group => group.Last())
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList());
    }

    public UnityAlphaMultiFamilyMediaBindingValidation BuildMediaBindingValidation(UnityAlphaMediaBoundStagingManifest goal056StagingManifest) =>
        new()
        {
            Passed = goal056StagingManifest.Passed
                && goal056StagingManifest.PhysicalMediaFileCount == 15
                && goal056StagingManifest.PngFileCount == 9
                && goal056StagingManifest.WavFileCount == 3
                && goal056StagingManifest.BundleFileCount == 3
                && goal056StagingManifest.FamilyCount == 3
                && goal056StagingManifest.Bindings.All(item => item.SafeRelativePath && item.HashMatchesGoal055),
            FamilyCount = goal056StagingManifest.FamilyCount,
            MediaBindingCount = goal056StagingManifest.Bindings.Count,
            PngFileCount = goal056StagingManifest.PngFileCount,
            WavFileCount = goal056StagingManifest.WavFileCount,
            BundleFileCount = goal056StagingManifest.BundleFileCount,
            HashValidationPassed = goal056StagingManifest.Bindings.All(item => item.HashMatchesGoal055),
            Bindings = goal056StagingManifest.Bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList()
        };

    public UnityAlphaMultiFamilyPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<UnityAlphaMultiFamilySourceFamily> families)
    {
        var payloads = families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(item => new UnityAlphaMultiFamilyPreviewExportRecord
            {
                FamilyId = item.FamilyId,
                PreviewPayloadId = "unity-alpha-multifamily-preview/" + item.FamilyId,
                ExportPayloadId = "unity-alpha-multifamily-export/" + item.FamilyId,
                RuntimePreviewPayloadRef = item.RuntimePreviewPayloadRef,
                ExportMode = item.ExportMode
            })
            .ToList();

        return new UnityAlphaMultiFamilyPreviewExportPayload
        {
            Passed = payloads.Count == 3
                && payloads.All(item => !string.IsNullOrWhiteSpace(item.RuntimePreviewPayloadRef))
                && payloads.All(item => !string.IsNullOrWhiteSpace(item.ExportMode)),
            FamilyCount = payloads.Count,
            Payloads = payloads
        };
    }

    public UnityAlphaMultiFamilyReviewPackageManifest BuildReviewPackageManifest()
    {
        var requiredFiles = new[]
        {
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.SourceManifestJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyModeManifestJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.UnityStagingManifestJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyCommandPlanJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("map_panel_rpg"),
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("survival_sandbox"),
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("first_person_grid_dungeon"),
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.PlayerLogSummaryJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.MediaBindingValidationJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.PreviewExportPayloadJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.ReviewPackageManifestJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.InvalidMatrixJsonFileName,
            UnityAlphaMultiFamilyPlayableLoopEvidenceService.ReportMarkdownFileName
        };

        return new UnityAlphaMultiFamilyReviewPackageManifest
        {
            Passed = true,
            Accepted = false,
            FamilyCount = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds.Count,
            RequiredEvidenceFiles = requiredFiles.Order(StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyDictionary<string, UnityAlphaFamilyLoopProof> BuildFamilyLoopProofs(
        IReadOnlyList<UnityAlphaMultiFamilySourceFamily> families,
        UnityAlphaMultiFamilyUnityProof unityProof)
    {
        var matchedSet = unityProof.PlayerLogSummary.MatchedMarkers.ToHashSet(StringComparer.Ordinal);
        return families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToDictionary(
                item => item.FamilyId,
                item =>
                {
                    var expected = ExpectedMarkersForFamily(item).ToList();
                    var matched = expected.Where(marker => matchedSet.Contains(marker)).Order(StringComparer.Ordinal).ToList();
                    var missing = expected.Where(marker => !matchedSet.Contains(marker)).Order(StringComparer.Ordinal).ToList();
                    var passed = unityProof.Passed
                        && missing.Count == 0
                        && item.LoopCommands.Count(command => matchedSet.Contains(command.ExpectedPlayerMarker)) >= 3
                        && matchedSet.Contains("media_bound_hash_validation=true")
                        && matchedSet.Contains("review_package_proof=goal057");
                    return new UnityAlphaFamilyLoopProof
                    {
                        FamilyId = item.FamilyId,
                        ScenarioId = item.ScenarioId,
                        Passed = passed,
                        UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
                        ScenarioLoaded = matchedSet.Contains("family_scenario_loaded=" + item.FamilyId),
                        MediaManifestHashValidationPassed = matchedSet.Contains("media_bound_hash_validation=true"),
                        ReviewPackageProofPassed = matchedSet.Contains("review_package_proof=goal057"),
                        LoopStepCount = item.LoopCommands.Count(command => matchedSet.Contains(command.ExpectedPlayerMarker)),
                        ExpectedMarkers = expected,
                        MatchedMarkers = matched,
                        MissingMarkers = missing,
                        Commands = item.LoopCommands,
                        SourceChangedMarkers = item.SourceChangedMarkers,
                        Diagnostics = missing.Select(marker => Error("goal057.unity.family_marker_missing", marker, "Unity player log did not contain the required family marker.")).ToList()
                    };
                },
                StringComparer.Ordinal);
    }

    public InvalidUnityAlphaMultiFamilyMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidUnityAlphaMultiFamilyScenario>
        {
            Invalid("missing_goal056_source", "Remove Goal 056 media-bound Unity proof artifacts.", "blocked", Error("goal057.source.goal056_missing", "Goal056", "Goal 056 accepted media-bound source evidence is required.")),
            Invalid("missing_media_manifest", "Remove media-bound/unity-alpha-media-bound-manifest.json from staging.", "rejected", Error("goal057.media.manifest_missing", "staging/media-bound", "Goal 057 requires the Goal 056 media manifest in StreamingAssets.")),
            Invalid("stale_hash_mismatched_media_file", "Change a staged media file after the manifest hash is recorded.", "rejected", Error("goal057.media.hash_mismatch", "media-bound", "Staged media bytes must match the Goal 056 manifest hash.")),
            Invalid("fake_family_id", "Add a family mode outside Goal 043/047/056 families.", "rejected", Error("goal057.family.fake_id", "family/fake", "Family id must resolve to selected source families.")),
            Invalid("duplicate_family_mode_id", "Duplicate a family mode id in the family manifest.", "rejected", Error("goal057.family.duplicate_mode_id", "family-mode-manifest", "Family mode ids must be unique.")),
            Invalid("missing_family_command_plan", "Remove the staged family-loop command plan.", "blocked", Error("goal057.command_plan.missing", "family-loop/family-command-plan.json", "Unity player proof requires the staged family command plan.")),
            Invalid("missing_player_marker", "Omit a required family_loop_completed marker from the player log.", "blocked", Error("goal057.unity.marker_missing", "family_loop_completed", "Unity player logs must contain every required family marker.")),
            Invalid("fake_player_log", "Copy a previous player log without Goal 057 family markers.", "rejected", Error("goal057.unity.fake_log", "logs/alpha-player-play-loop.log", "Player log must be freshly produced by the Goal 057 Unity route.")),
            Invalid("malformed_png_wav_bundle_ref", "Point a media binding to malformed PNG, WAV or bundle bytes.", "rejected", Error("goal057.media.malformed_ref", "media-bound", "PNG, WAV and bundle refs must remain valid Goal 056 media refs.")),
            Invalid("unsafe_relative_path", "Use an absolute path or path traversal in staging.", "rejected", Error("goal057.path.unsafe", "../escape", "Staging paths must be safe relative paths.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider, network, LLM or RAG execution.", "blocked", Error("goal057.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("lua_execution_claim", "Claim Lua execution during Unity family proof.", "blocked", Error("goal057.boundary.lua_execution", "boundary", "Lua execution is forbidden for Goal 057.")),
            Invalid("runtime_gamepackage_schema_mutation_claim", "Claim Runtime or public GamePackage schema mutation.", "blocked", Error("goal057.boundary.runtime_gamepackage_schema", "boundary", "Runtime and public GamePackage schema mutation are forbidden.")),
            Invalid("broad_unity_mutation_claim", "Replace the Unity Alpha architecture or scene route.", "blocked", Error("goal057.boundary.broad_unity_mutation", "unity", "Only a narrow family-mode diagnostic marker extension is allowed.")),
            Invalid("nondeterministic_ordering", "Shuffle source, family or command records.", "rejected", Error("goal057.order.nondeterministic", "ordering", "Goal 057 records must be deterministically ordered.")),
            Invalid("missing_review_trace", "Publish a review package manifest without Goal 056/057 trace refs.", "rejected", Error("goal057.review.trace_missing", "review-package", "Every family proof requires source and review trace refs."))
        };

        return new InvalidUnityAlphaMultiFamilyMatrix
        {
            Passed = UnityAlphaMultiFamilyPlayableLoopVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
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

    public static IEnumerable<string> ExpectedMarkersForFamily(UnityAlphaMultiFamilySourceFamily family)
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

    public static IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> SortDiagnostics(IEnumerable<UnityAlphaMultiFamilyDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string FamilyOrderingKey(string familyId) =>
        UnityAlphaMultiFamilyPlayableLoopSourceLoader.FamilyOrderingKey(familyId);

    private static InvalidUnityAlphaMultiFamilyScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params UnityAlphaMultiFamilyDiagnostic[] diagnostics) =>
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

    private static string Serialize<T>(T value) => UnityAlphaMultiFamilyPlayableLoopHash.Serialize(value);

    private static string Hash(string text) => UnityAlphaMultiFamilyPlayableLoopHash.Hash(text);

    private static UnityAlphaMultiFamilyDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMultiFamilyDiagnostic.Error(code, target, message);

    private static UnityAlphaMultiFamilyDiagnostic Info(string code, string target, string message) =>
        UnityAlphaMultiFamilyDiagnostic.Info(code, target, message);
}
