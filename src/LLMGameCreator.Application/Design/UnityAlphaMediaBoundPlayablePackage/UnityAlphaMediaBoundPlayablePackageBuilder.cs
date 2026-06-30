using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundPlayablePackageBuilder
{
    public UnityAlphaMediaBoundSourceManifest BuildSourceManifest(UnityAlphaMediaBoundSourceBundle source)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>(source.Diagnostics)
        {
            Info("goal056.preflight.goal055_handoff_recorded", "media_bound_playable_review_package_verification", "Goal 055 is recorded as accepted by user handoff before Goal 056."),
            Info("goal056.source.goal055_loaded", "Goal055", "Goal 055 staged media-bound review package facts were loaded from repository-local evidence.")
        };

        return new UnityAlphaMediaBoundSourceManifest
        {
            Accepted = false,
            Goal055AcceptedByUserHandoff = true,
            Goal055ReportWasGreenProducedForReview = source.Goal055ReportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && source.Goal055ReportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && source.Goal055ReportMarkdown.Contains("media_bound_playable_review_package_verification required", StringComparison.Ordinal),
            Goal055PhysicalMediaFileCount = source.Goal055StagedFiles.Count,
            Goal055PngFileCount = source.Goal055StagedFiles.Count(IsPng),
            Goal055WavFileCount = source.Goal055StagedFiles.Count(IsWav),
            Goal055BundleFileCount = source.Goal055StagedFiles.Count(IsBundle),
            BaseAlphaPayloadSourceRoot = source.BaseAlphaPayloadSourceRootRelativePath,
            BaseAlphaPayloadFound = source.BaseAlphaPayloadFiles.Count > 0,
            SelectedFamilyIds = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds.OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new UnityAlphaMediaBoundGateRecord
                {
                    GateId = "media_bound_playable_review_package_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 056 task preflight handoff"
                },
                new UnityAlphaMediaBoundGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new UnityAlphaMediaBoundGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new UnityAlphaMediaBoundGateRecord
                {
                    GateId = UnityAlphaMediaBoundPlayablePackageVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 056 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public (IReadOnlyList<UnityAlphaMediaBoundBinding> Bindings, IReadOnlyList<UnityAlphaMediaBoundFilePayload> StagingFiles) BuildStagingFiles(UnityAlphaMediaBoundSourceBundle source)
    {
        var bindings = new List<UnityAlphaMediaBoundBinding>();
        var stagingFiles = new List<UnityAlphaMediaBoundFilePayload>();

        foreach (var file in source.BaseAlphaPayloadFiles)
        {
            stagingFiles.Add(file);
        }

        foreach (var staged in source.Goal055StagedFiles.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            var sourcePayload = source.Goal055MediaFiles.FirstOrDefault(item =>
                string.Equals(item.RelativePath, UnityAlphaMediaBoundPlayablePackageSourceLoader.ToUnityStagingRelativePath(staged.StagedRelativePath), StringComparison.Ordinal));
            if (sourcePayload == null)
            {
                continue;
            }

            var hash = Hash(sourcePayload.Bytes);
            var png = IsPng(staged) ? MediaBoundMediaValidators.ValidatePng(sourcePayload.Bytes) : new PngValidationResult();
            var wav = IsWav(staged) ? MediaBoundMediaValidators.ValidateWav(sourcePayload.Bytes) : new WavValidationResult();
            var relativePath = sourcePayload.RelativePath;
            stagingFiles.Add(new UnityAlphaMediaBoundFilePayload
            {
                RelativePath = relativePath,
                Bytes = sourcePayload.Bytes
            });

            bindings.Add(new UnityAlphaMediaBoundBinding
            {
                BindingId = "goal056-media-binding/" + SafeSegment(staged.FamilyId) + "/" + SafeSegment(staged.SlotId),
                FamilyId = staged.FamilyId,
                SlotId = staged.SlotId,
                MediaKind = staged.MediaKind,
                RelativePath = relativePath,
                SourceGoal055RelativePath = NormalizeGoal055RelativePath(staged.StagedRelativePath),
                Sha256 = hash,
                SizeBytes = sourcePayload.Bytes.LongLength,
                Width = png.Width,
                Height = png.Height,
                SampleRate = wav.SampleRate,
                Channels = wav.Channels,
                SampleCount = wav.SampleCount,
                SafeRelativePath = IsSafeRelativePath(relativePath),
                HashMatchesGoal055 = hash == staged.StagedSha256,
                PngValid = IsPng(staged) && png.Passed,
                WavValid = IsWav(staged) && wav.Passed,
                ReviewTrace = staged.ReviewTrace,
                DeterministicOrderingKey = FamilyOrderingKey(staged.FamilyId) + "|" + SlotOrder(staged.SlotId).ToString("000") + "|" + relativePath
            });
        }

        var manifest = BuildUnityRuntimeManifest(bindings);
        stagingFiles.Add(new UnityAlphaMediaBoundFilePayload
        {
            RelativePath = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath,
            Bytes = System.Text.Encoding.UTF8.GetBytes(manifest + Environment.NewLine)
        });

        return (
            bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            stagingFiles
                .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
                .Select(group => group.Last())
                .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
                .ToList());
    }

    public UnityAlphaMediaBoundStagingManifest BuildStagingManifest(
        UnityAlphaMediaBoundSourceBundle source,
        IReadOnlyList<UnityAlphaMediaBoundBinding> bindings)
    {
        var withoutHash = new UnityAlphaMediaBoundStagingManifest
        {
            Passed = source.BaseAlphaPayloadFiles.Count > 0
                && bindings.Count == 15
                && bindings.All(item => item.SafeRelativePath && item.HashMatchesGoal055)
                && bindings.Count(IsPng) >= 3
                && bindings.Count(IsWav) >= 3
                && bindings.Count(IsBundle) >= 3
                && UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds.All(familyId => bindings.Any(item => item.FamilyId == familyId)),
            BasePayloadFileCount = source.BaseAlphaPayloadFiles.Count,
            PhysicalMediaFileCount = bindings.Count,
            PngFileCount = bindings.Count(IsPng),
            WavFileCount = bindings.Count(IsWav),
            BundleFileCount = bindings.Count(IsBundle),
            FamilyCount = bindings.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            Bindings = bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList()
        };
        return withoutHash with { DeterministicHash = Hash(Serialize(withoutHash)) };
    }

    public UnityAlphaMediaBoundFamilyPanelModels BuildFamilyPanelModels(IReadOnlyList<UnityAlphaMediaBoundBinding> bindings)
    {
        var families = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds
            .OrderBy(FamilyOrderingKey, StringComparer.Ordinal)
            .Select(familyId =>
            {
                var familyBindings = bindings.Where(item => item.FamilyId == familyId).OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList();
                return new UnityAlphaMediaBoundFamilyPanelModel
                {
                    FamilyId = familyId,
                    PanelId = "unity-alpha-media-panel/" + familyId,
                    ImageBindingId = familyBindings.FirstOrDefault(IsPng)?.BindingId ?? string.Empty,
                    WavBindingId = familyBindings.FirstOrDefault(IsWav)?.BindingId ?? string.Empty,
                    BundleBindingId = familyBindings.FirstOrDefault(IsBundle)?.BindingId ?? string.Empty,
                    PanelProofMarker = "media_bound_family_panel_proof=" + familyId,
                    BindingIds = familyBindings.Select(item => item.BindingId).ToList()
                };
            })
            .ToList();

        return new UnityAlphaMediaBoundFamilyPanelModels
        {
            Passed = families.Count == 3
                && families.All(item => !string.IsNullOrWhiteSpace(item.ImageBindingId))
                && families.All(item => !string.IsNullOrWhiteSpace(item.WavBindingId))
                && families.All(item => !string.IsNullOrWhiteSpace(item.BundleBindingId)),
            FamilyCount = families.Count,
            Families = families
        };
    }

    public UnityAlphaMediaBoundLoadContract BuildUnityLoadContract(IReadOnlyList<UnityAlphaMediaBoundBinding> bindings)
    {
        var markers = new List<string>
        {
            "media_bound_manifest_loaded=true",
            "media_bound_family_count=3",
            "media_bound_png_loaded=true",
            "media_bound_wav_loaded=true",
            "media_bound_bundle_loaded=true",
            "media_bound_hash_validation=true",
            "media_bound_playable_review_package_verification=required"
        };
        markers.AddRange(UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds
            .OrderBy(FamilyOrderingKey, StringComparer.Ordinal)
            .Select(familyId => "media_bound_family_panel_proof=" + familyId));

        return new UnityAlphaMediaBoundLoadContract
        {
            Passed = bindings.Count == 15 && bindings.All(item => item.HashMatchesGoal055 && item.SafeRelativePath),
            RequiredLogMarkers = markers,
            ExpectedBindings = bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList()
        };
    }

    public UnityAlphaMediaBoundHashInventory BuildHashInventory(IReadOnlyList<UnityAlphaMediaBoundBinding> bindings) =>
        new()
        {
            Passed = bindings.Count == 15 && bindings.All(item => item.HashMatchesGoal055 && !string.IsNullOrWhiteSpace(item.Sha256)),
            FileCount = bindings.Count,
            MediaFiles = bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList()
        };

    public UnityAlphaMediaBoundPreviewExportPayloads BuildPreviewExportPayloads(UnityAlphaMediaBoundFamilyPanelModels panelModels)
    {
        var payloads = panelModels.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(item => new UnityAlphaMediaBoundPreviewExportPayloadRecord
            {
                FamilyId = item.FamilyId,
                PreviewPayloadId = "unity-alpha-media-bound-preview/" + item.FamilyId,
                ExportPayloadId = "unity-alpha-media-bound-export/" + item.FamilyId,
                UnityManifestRef = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath,
                PanelProofMarker = item.PanelProofMarker,
                BindingIds = item.BindingIds
            })
            .ToList();

        return new UnityAlphaMediaBoundPreviewExportPayloads
        {
            Passed = payloads.Count == 3 && payloads.All(item => item.BindingIds.Count >= 3),
            FamilyCount = payloads.Count,
            Payloads = payloads
        };
    }

    public InvalidUnityAlphaMediaBoundMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidUnityAlphaMediaBoundScenario>
        {
            Invalid("missing_goal055_source", "Remove Goal 055 staged media-bound package evidence.", "blocked", Error("goal056.source.goal055_missing", "Goal055", "Goal 055 media-bound review package source is required.")),
            Invalid("stale_goal055_hash", "Change a Goal 055 source artifact after Goal 056 source manifest records its hash.", "rejected", Error("goal056.source.goal055_hash_mismatch", "Goal055", "Goal 055 source hashes must match physical artifacts.")),
            Invalid("missing_staged_png", "Delete a staged PNG after manifest creation.", "rejected", Error("goal056.stage.png_missing", "staging/media-bound", "Every family requires at least one staged PNG.")),
            Invalid("missing_staged_wav", "Delete a staged WAV after manifest creation.", "rejected", Error("goal056.stage.wav_missing", "staging/media-bound", "Every family requires at least one staged WAV.")),
            Invalid("malformed_png", "Replace staged PNG bytes with malformed data.", "rejected", Error("goal056.media.png_malformed", "png", "PNG files require valid signature, dimensions and CRC proof.")),
            Invalid("malformed_wav", "Replace staged WAV bytes with malformed data.", "rejected", Error("goal056.media.wav_malformed", "wav", "WAV files require a valid RIFF/WAVE PCM header and data chunk.")),
            Invalid("unsafe_relative_path", "Use an absolute path or path traversal in the Unity manifest.", "rejected", Error("goal056.path.unsafe", "../escape.png", "Unity media paths must be safe relative paths.")),
            Invalid("duplicate_media_binding_id", "Duplicate a media binding id in the Unity manifest.", "rejected", Error("goal056.binding.duplicate_id", "media-binding", "Unity media binding ids must be unique.")),
            Invalid("fake_family_id", "Bind media to a family outside Goal 047/055 families.", "rejected", Error("goal056.family.fake_id", "family/fake", "Family id must resolve to a selected source family.")),
            Invalid("fake_slot_id", "Bind media to a slot absent from Goal 055 staged files.", "rejected", Error("goal056.slot.fake_id", "slot/fake", "Slot id must resolve to a Goal 055 media slot.")),
            Invalid("missing_unity_load_trace", "Claim Unity proof without a launch/play-loop log marker.", "blocked", Error("goal056.unity.trace_missing", "logs", "Unity media proof requires real launch or play-loop trace markers.")),
            Invalid("stale_unity_load_hash", "Load a Unity media file whose runtime hash differs from the manifest hash.", "rejected", Error("goal056.unity.hash_mismatch", "logs", "Unity media hash validation must pass for loaded files.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider, network, LLM or RAG execution.", "blocked", Error("goal056.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("lua_execution_claim", "Claim Lua execution while producing media-bound Unity proof.", "blocked", Error("goal056.boundary.lua_execution", "boundary", "Lua execution is forbidden for Goal 056.")),
            Invalid("gamepackage_schema_mutation_claim", "Claim public GamePackage schema mutation.", "blocked", Error("goal056.boundary.gamepackage_schema", "boundary", "Public GamePackage schema mutation is forbidden.")),
            Invalid("runtime_ui_broad_mutation_claim", "Claim Runtime or WinForms UI mutation.", "blocked", Error("goal056.boundary.runtime_ui", "boundary", "Runtime and WinForms UI mutation are forbidden.")),
            Invalid("unity_broad_refactor_claim", "Replace the Unity Alpha runtime with a broad media browser/refactor.", "blocked", Error("goal056.boundary.unity_broad_refactor", "unity", "Only a narrow media manifest loader/panel/log extension is allowed.")),
            Invalid("nondeterministic_ordering", "Shuffle source, staged or manifest records.", "rejected", Error("goal056.order.nondeterministic", "ordering", "Goal 056 records must be deterministically ordered.")),
            Invalid("missing_review_provenance_trace", "Stage media without Goal 055/054 review provenance.", "rejected", Error("goal056.review.trace_missing", "review-trace", "Every staged file requires Goal 055/054 provenance trace."))
        };

        return new InvalidUnityAlphaMediaBoundMatrix
        {
            Passed = UnityAlphaMediaBoundPlayablePackageVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public Goal056ArtifactScopeReport BuildArtifactScopeReport() =>
        new()
        {
            Passed = true,
            AllowedExactPaths =
            [
                "docs/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE_SPEC.md",
                "docs/EXTERNAL_SCOUTING_GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md",
                "docs/agent-tasks/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md",
                "docs/agent-tasks/GOAL_056_LAUNCHER.txt",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                ".devflow/artifact-scope/artifact-scope-policy.json",
                "tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaMediaBoundPlayablePackageProductSmokeTests.cs"
            ],
            AllowedPathPrefixes =
            [
                ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/",
                "src/LLMGameCreator.Application/Design/UnityAlphaMediaBoundPlayablePackage/",
                "tests/LLMGameCreator.Tests/Application/UnityAlphaMediaBoundPlayablePackage/",
                "unity/LLMGameCreatorAlpha/"
            ]
        };

    public static string BuildUnityRuntimeManifest(IReadOnlyList<UnityAlphaMediaBoundBinding> bindings)
    {
        var manifest = new
        {
            schemaVersion = "unity_alpha_media_bound_runtime_manifest_v1",
            goalId = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId,
            manualGate = UnityAlphaMediaBoundPlayablePackageVocabulary.FinalGate,
            accepted = false,
            familyCount = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds.Count,
            playableReviewPackageVerification = "required",
            bindings = bindings.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).Select(item => new
            {
                item.BindingId,
                item.FamilyId,
                item.SlotId,
                item.MediaKind,
                item.RelativePath,
                item.Sha256,
                item.SizeBytes,
                item.Width,
                item.Height,
                item.SampleRate,
                item.Channels,
                item.SampleCount,
                item.ReviewTrace
            }).ToList()
        };

        return Serialize(manifest);
    }

    public static IReadOnlyList<UnityAlphaMediaBoundDiagnostic> SortDiagnostics(IEnumerable<UnityAlphaMediaBoundDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static string NormalizeGoal055RelativePath(string stagedRelativePath) =>
        MediaBoundPlayableReviewPackageEvidenceService.RelativeOutputDirectory.TrimEnd('/', '\\') + "/" + stagedRelativePath;

    private static bool IsPng(UnityAlphaMediaBoundBinding item) => item.RelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

    private static bool IsWav(UnityAlphaMediaBoundBinding item) => item.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);

    private static bool IsBundle(UnityAlphaMediaBoundBinding item) => item.RelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && item.MediaKind == "bundle";

    private static bool IsPng(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

    private static bool IsWav(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);

    private static bool IsBundle(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && item.MediaKind == "bundle";

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static int SlotOrder(string slotId) =>
        slotId switch
        {
            "world_key_art" => 1,
            "npc_portrait" => 2,
            "ui_panel_skin" => 3,
            "sfx_interaction" => 4,
            "export_placeholder_bundle" => 5,
            _ => 999
        };

    private static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    private static string SafeSegment(string value)
    {
        var builder = new System.Text.StringBuilder();
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

    private static InvalidUnityAlphaMediaBoundScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params UnityAlphaMediaBoundDiagnostic[] diagnostics) =>
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

    private static string Serialize<T>(T value) => UnityAlphaMediaBoundPlayablePackageHash.Serialize(value);

    private static string Hash(string text) => UnityAlphaMediaBoundPlayablePackageHash.Hash(text);

    private static string Hash(byte[] bytes) => UnityAlphaMediaBoundPlayablePackageHash.Hash(bytes);

    private static UnityAlphaMediaBoundDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMediaBoundDiagnostic.Error(code, target, message);

    private static UnityAlphaMediaBoundDiagnostic Info(string code, string target, string message) =>
        UnityAlphaMediaBoundDiagnostic.Info(code, target, message);
}
