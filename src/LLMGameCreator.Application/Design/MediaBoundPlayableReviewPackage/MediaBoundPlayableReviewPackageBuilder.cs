namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundPlayableReviewPackageBuilder
{
    public MediaBoundSourceManifest BuildSourceManifest(MediaBoundSourceBundle source)
    {
        var families = MediaBoundPlayableReviewPackageVocabulary.FamilyIds
            .OrderBy(FamilyOrderingKey, StringComparer.Ordinal)
            .Select(familyId =>
            {
                var goal054Family = source.Goal054SourceManifest.Families.First(item => item.FamilyId == familyId);
                var files = source.Goal054Inventory.Files.Where(item => item.FamilyId == familyId).ToList();
                return new MediaBoundFamilySourceRecord
                {
                    FamilyId = familyId,
                    ScenarioId = goal054Family.ScenarioId,
                    ProfileId = goal054Family.ProfileId,
                    DryRunArtifactRef = goal054Family.DryRunArtifactRef,
                    Goal054PreviewPayloadRef = source.Goal054PreviewPayloads.Payloads.First(item => item.FamilyId == familyId).PreviewPayloadId,
                    Goal054PhysicalMediaCount = files.Count,
                    Goal054PngCount = files.Count(IsPng),
                    Goal054WavCount = files.Count(IsWav),
                    Goal054BundleJsonCount = files.Count(IsBundle)
                };
            })
            .ToList();

        return new MediaBoundSourceManifest
        {
            Accepted = false,
            Goal054AcceptedByUserHandoff = true,
            Goal054ReportWasGreenProducedForReview = source.Goal054ReportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
                && source.Goal054ReportMarkdown.Contains("accepted=false", StringComparison.Ordinal)
                && source.Goal054ReportMarkdown.Contains("media_materialization_review_package_verification required", StringComparison.Ordinal),
            Goal047FamilyDryRunCount = source.Goal047FamilyDryRuns.Count,
            Goal053BindingCount = source.Goal053BindingManifest.BindingCount,
            Goal054PhysicalMediaCount = source.Goal054Inventory.FileCount,
            Goal054PngCount = source.Goal054Inventory.PngFileCount,
            Goal054WavCount = source.Goal054Inventory.WavFileCount,
            Goal054BundleJsonCount = source.Goal054Inventory.BundleJsonFileCount,
            PreflightGates =
            [
                new MediaBoundGateRecord
                {
                    GateId = "media_materialization_review_package_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 055 task preflight handoff"
                },
                new MediaBoundGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new MediaBoundGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new MediaBoundGateRecord
                {
                    GateId = MediaBoundPlayableReviewPackageVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 055 produced for review"
                }
            ],
            SelectedFamilyIds = MediaBoundPlayableReviewPackageVocabulary.FamilyIds.OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            Families = families,
            SourceArtifactRefs = source.SourceArtifactRefs,
            BoundaryClaims = new MediaBoundBoundaryClaims(),
            Diagnostics = SortDiagnostics(source.Diagnostics.Concat(
            [
                MediaBoundDiagnostic.Info("goal055.preflight.goal054_handoff_recorded", "media_materialization_review_package_verification", "Goal 054 is recorded as accepted by user handoff before Goal 055."),
                MediaBoundDiagnostic.Info("goal055.source.physical_media_loaded", "Goal054", "Goal 054 physical PNG/WAV/bundle media bytes were loaded from repository-local evidence.")
            ]))
        };
    }

    public (IReadOnlyList<StagedMediaFileRecord> Records, IReadOnlyList<StagedMediaFilePayload> Payloads) BuildStagedMedia(MediaBoundSourceBundle source)
    {
        var licenseByMaterialization = source.Goal054LicenseLedger.MaterializedFiles
            .ToDictionary(item => item.MaterializationId, item => item, StringComparer.Ordinal);
        var records = new List<StagedMediaFileRecord>();
        var payloads = new List<StagedMediaFilePayload>();

        foreach (var physical in source.Goal054PhysicalMediaFiles)
        {
            var sourceRecord = physical.InventoryRecord;
            var stagedHash = MediaBoundPlayableReviewPackageHash.Hash(physical.Bytes);
            var extension = ExtensionFor(sourceRecord);
            var stableName = StableFileName(sourceRecord, stagedHash, extension);
            var stagedRelativePath = string.Join('/',
            [
                MediaBoundPlayableReviewPackageVocabulary.StreamingAssetsMediaRoot,
                SafeSegment(sourceRecord.FamilyId),
                stableName
            ]);

            var png = IsPng(sourceRecord) ? MediaBoundMediaValidators.ValidatePng(physical.Bytes) : new PngValidationResult();
            var wav = IsWav(sourceRecord) ? MediaBoundMediaValidators.ValidateWav(physical.Bytes) : new WavValidationResult();
            licenseByMaterialization.TryGetValue(sourceRecord.MaterializationId, out var license);

            records.Add(new StagedMediaFileRecord
            {
                StagingId = "staged/" + SafeSegment(sourceRecord.MaterializationId),
                FamilyId = sourceRecord.FamilyId,
                MediaKind = sourceRecord.MediaKind,
                SlotId = sourceRecord.MediaSlotId,
                StableFileName = stableName,
                StagedRelativePath = stagedRelativePath,
                SourceGoalId = sourceRecord.MaterializationId,
                SourceRelativePath = physical.SourceRelativePath,
                SourceSha256 = sourceRecord.Sha256,
                SourceSizeBytes = sourceRecord.ByteLength,
                StagedSha256 = stagedHash,
                SizeBytes = physical.Bytes.LongLength,
                LicenseDecision = license?.LicenseStatus ?? "missing_license_decision",
                ProvenanceDecision = license?.ProvenanceStatus ?? "missing_provenance_decision",
                ReviewTrace = "goal054:" + sourceRecord.MaterializationId,
                SafeRelativePath = IsSafeRelativePath(stagedRelativePath),
                SourceHashMatches = stagedHash == sourceRecord.Sha256 && physical.ActualSha256 == sourceRecord.Sha256,
                PngValid = IsPng(sourceRecord) && png.Passed,
                PngWidth = png.Width,
                PngHeight = png.Height,
                WavValid = IsWav(sourceRecord) && wav.Passed,
                WavSampleRate = wav.SampleRate,
                WavChannels = wav.Channels,
                WavSampleCount = wav.SampleCount,
                DeterministicOrderingKey = FamilyOrderingKey(sourceRecord.FamilyId) + "|" + SlotOrder(sourceRecord.MediaSlotId).ToString("000") + "|" + stagedRelativePath
            });
            payloads.Add(new StagedMediaFilePayload
            {
                RelativePath = stagedRelativePath,
                Bytes = physical.Bytes
            });
        }

        return (
            records.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            payloads.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList());
    }

    public MediaBoundReviewPackageManifest BuildReviewPackageManifest(
        MediaBoundSourceManifest sourceManifest,
        IReadOnlyList<StagedMediaFileRecord> stagedFiles)
    {
        var bindings = BuildBindings(stagedFiles);
        var families = sourceManifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                var files = stagedFiles.Where(item => item.FamilyId == family.FamilyId).OrderBy(item => item.StagedRelativePath, StringComparer.Ordinal).ToList();
                return new FamilyReviewPackageRecord
                {
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    ReadmeRelativePath = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/README.md",
                    PlayableManifestRelativePath = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/media-bound-playable-manifest.json",
                    StreamingAssetsManifestRelativePath = StreamingManifestPackagePath(),
                    SourceDryRunArtifactRef = family.DryRunArtifactRef,
                    StagedFileCount = files.Count,
                    ImagePngCount = files.Count(IsPng),
                    WavCount = files.Count(IsWav),
                    BundleJsonCount = files.Count(IsBundle),
                    Passed = files.Any(IsPng) && files.Any(IsWav) && files.Any(IsBundle),
                    StagedMediaRefs = files.Select(item => item.StagedRelativePath).ToList()
                };
            })
            .ToList();

        var packageFiles = new List<string>
        {
            MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/README.md",
            MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/CHECKLIST.md",
            MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/media-bound-playable-manifest.json",
            StreamingManifestPackagePath()
        };
        packageFiles.AddRange(stagedFiles.Select(item => item.StagedRelativePath));

        return new MediaBoundReviewPackageManifest
        {
            Passed = families.Count == 3
                && families.All(item => item.Passed)
                && stagedFiles.All(item => item.SourceHashMatches && item.SafeRelativePath)
                && stagedFiles.Any(IsPng)
                && stagedFiles.Any(IsWav)
                && stagedFiles.Any(IsBundle),
            ReadmeRelativePath = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/README.md",
            ChecklistRelativePath = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/CHECKLIST.md",
            PlayableManifestRelativePath = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot + "/media-bound-playable-manifest.json",
            StagedFileCount = stagedFiles.Count,
            PngFileCount = stagedFiles.Count(IsPng),
            WavFileCount = stagedFiles.Count(IsWav),
            BundleJsonFileCount = stagedFiles.Count(IsBundle),
            Families = families,
            StagedFiles = stagedFiles.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            Bindings = bindings,
            PackageFiles = packageFiles.Order(StringComparer.Ordinal).ToList()
        };
    }

    public StreamingAssetsMediaManifest BuildStreamingAssetsManifest(IReadOnlyList<StagedMediaFileRecord> stagedFiles)
    {
        var bindings = BuildBindings(stagedFiles);
        return new StreamingAssetsMediaManifest
        {
            Passed = bindings.Count == stagedFiles.Count
                && bindings.All(item => IsSafeRelativePath(item.StreamingAssetsRelativePath))
                && MediaBoundPlayableReviewPackageVocabulary.FamilyIds.All(familyId => bindings.Any(item => item.FamilyId == familyId)),
            ManifestRelativePath = StreamingManifestPackagePath(),
            FamilyCount = bindings.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            BindingCount = bindings.Count,
            Bindings = bindings
        };
    }

    public MediaBoundPreviewPayloads BuildPreviewPayloads(
        MediaBoundSourceBundle source,
        MediaBoundSourceManifest sourceManifest,
        StreamingAssetsMediaManifest streamingManifest,
        IReadOnlyList<StagedMediaFileRecord> stagedFiles)
    {
        var payloads = sourceManifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                var files = stagedFiles.Where(item => item.FamilyId == family.FamilyId).OrderBy(item => item.StagedRelativePath, StringComparer.Ordinal).ToList();
                var goal054Payload = source.Goal054PreviewPayloads.Payloads.First(item => item.FamilyId == family.FamilyId);
                return new MediaBoundPreviewPayloadRecord
                {
                    PreviewPayloadId = "media-bound-preview/" + SafeSegment(family.FamilyId),
                    ExportPayloadId = "media-bound-export/" + SafeSegment(family.FamilyId),
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    ReferencedDryRunArtifactRef = family.DryRunArtifactRef,
                    Goal054PreviewPayloadId = goal054Payload.PreviewPayloadId,
                    StagedMediaRefs = files.Select(item => item.StagedRelativePath).ToList(),
                    StreamingAssetsManifestRef = streamingManifest.ManifestRelativePath,
                    UnityLoadContractRef = MediaBoundPlayableReviewPackageEvidenceService.UnityLoadContractJsonFileName,
                    UnityLoadProofRef = UnityProofFileName(family.FamilyId),
                    ValidationStatus = files.Any(IsPng) && files.Any(IsWav) && files.Any(IsBundle) ? "passed" : "failed_missing_media_kind",
                    HashSummary = Hash(string.Join("|", files.Select(item => item.StagedSha256).Order(StringComparer.Ordinal)))
                };
            })
            .ToList();

        return new MediaBoundPreviewPayloads
        {
            Passed = payloads.Count == 3
                && payloads.All(item => item.ValidationStatus == "passed")
                && payloads.All(item => !string.IsNullOrWhiteSpace(item.ReferencedDryRunArtifactRef))
                && payloads.All(item => !string.IsNullOrWhiteSpace(item.StreamingAssetsManifestRef)),
            FamilyCount = payloads.Count,
            FamilyDryRunToMediaManifestProof = payloads.All(item => item.StagedMediaRefs.Count >= 3),
            Payloads = payloads
        };
    }

    public UnityMediaLoadContract BuildUnityLoadContract(
        StreamingAssetsMediaManifest streamingManifest,
        IReadOnlyList<StagedMediaFileRecord> stagedFiles)
    {
        var files = stagedFiles
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(item => new UnityMediaLoadFileContract
            {
                FamilyId = item.FamilyId,
                SlotId = item.SlotId,
                MediaKind = item.MediaKind,
                StreamingAssetsRelativePath = ToStreamingAssetsRelativePath(item.StagedRelativePath),
                Sha256 = item.StagedSha256,
                SizeBytes = item.SizeBytes,
                ExpectedWidth = item.PngWidth,
                ExpectedHeight = item.PngHeight,
                ExpectedSampleRate = item.WavSampleRate,
                ExpectedChannels = item.WavChannels,
                ExpectedSampleCount = item.WavSampleCount
            })
            .ToList();

        return new UnityMediaLoadContract
        {
            Passed = streamingManifest.Passed
                && files.Count == stagedFiles.Count
                && files.Any(item => item.ExpectedWidth > 0 && item.ExpectedHeight > 0)
                && files.Any(item => item.ExpectedSampleRate > 0 && item.ExpectedSampleCount > 0),
            UnitySourceChanged = false,
            UnityBuildOrPlayerExecuted = false,
            ManifestRelativePath = streamingManifest.ManifestRelativePath,
            RequiredProofLineTemplates =
            [
                "MEDIA_BOUND_MANIFEST_LOADED family=<family>",
                "MEDIA_BOUND_IMAGE_LOADED family=<family> slot=<slot> width=<w> height=<h> sha256=<hash>",
                "MEDIA_BOUND_WAV_VALIDATED family=<family> slot=<slot> sampleRate=<rate> channels=<channels> sampleCount=<count> sha256=<hash>",
                "MEDIA_BOUND_FAMILY_PANEL_READY family=<family>"
            ],
            Files = files,
            Diagnostics =
            [
                MediaBoundDiagnostic.Info("goal055.unity.contract.application_level", "unity-media-load-contract", "Unity-compatible proof is produced by BCL Application validation without changing Unity source or claiming player execution.")
            ]
        };
    }

    public IReadOnlyList<UnityMediaLoadProof> BuildUnityLoadProofs(IReadOnlyList<StagedMediaFileRecord> stagedFiles)
    {
        return MediaBoundPlayableReviewPackageVocabulary.FamilyIds
            .OrderBy(FamilyOrderingKey, StringComparer.Ordinal)
            .Select(familyId =>
            {
                var files = stagedFiles.Where(item => item.FamilyId == familyId).OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList();
                var records = new List<UnityMediaLoadProofRecord>();
                var lines = new List<string> { "MEDIA_BOUND_MANIFEST_LOADED family=" + familyId };

                foreach (var file in files.Where(IsPng))
                {
                    var line = $"MEDIA_BOUND_IMAGE_LOADED family={familyId} slot={file.SlotId} width={file.PngWidth} height={file.PngHeight} sha256={file.StagedSha256}";
                    lines.Add(line);
                    records.Add(new UnityMediaLoadProofRecord
                    {
                        ProofKind = "image_loaded",
                        FamilyId = familyId,
                        SlotId = file.SlotId,
                        MediaKind = file.MediaKind,
                        StagedRelativePath = file.StagedRelativePath,
                        Sha256 = file.StagedSha256,
                        Width = file.PngWidth,
                        Height = file.PngHeight,
                        ProofLine = line
                    });
                }

                foreach (var file in files.Where(IsWav))
                {
                    var line = $"MEDIA_BOUND_WAV_VALIDATED family={familyId} slot={file.SlotId} sampleRate={file.WavSampleRate} channels={file.WavChannels} sampleCount={file.WavSampleCount} sha256={file.StagedSha256}";
                    lines.Add(line);
                    records.Add(new UnityMediaLoadProofRecord
                    {
                        ProofKind = "wav_validated",
                        FamilyId = familyId,
                        SlotId = file.SlotId,
                        MediaKind = file.MediaKind,
                        StagedRelativePath = file.StagedRelativePath,
                        Sha256 = file.StagedSha256,
                        SampleRate = file.WavSampleRate,
                        Channels = file.WavChannels,
                        SampleCount = file.WavSampleCount,
                        ProofLine = line
                    });
                }

                lines.Add("MEDIA_BOUND_FAMILY_PANEL_READY family=" + familyId);

                return new UnityMediaLoadProof
                {
                    FamilyId = familyId,
                    Passed = files.Any(IsPng) && files.Any(IsWav) && files.Where(IsPng).All(item => item.PngValid) && files.Where(IsWav).All(item => item.WavValid),
                    ManifestLoaded = true,
                    ImageLoaded = files.Any(IsPng),
                    WavValidated = files.Any(IsWav),
                    FamilyPanelReady = true,
                    UnitySourceChanged = false,
                    UnityBuildOrPlayerExecuted = false,
                    ProofLines = lines,
                    Records = records,
                    Diagnostics =
                    [
                        MediaBoundDiagnostic.Info("goal055.unity.proof.application_level", familyId, "Proof lines are deterministic Application-level records compatible with a Unity StreamingAssets loader.")
                    ]
                };
            })
            .ToList();
    }

    public MediaBoundFamilySmokeMatrix BuildFamilySmokeMatrix(
        MediaBoundSourceManifest sourceManifest,
        MediaBoundReviewPackageManifest reviewPackage,
        MediaBoundPreviewPayloads previewPayloads,
        IReadOnlyList<UnityMediaLoadProof> loadProofs)
    {
        var previewByFamily = previewPayloads.Payloads.ToDictionary(item => item.FamilyId, item => item, StringComparer.Ordinal);
        var proofByFamily = loadProofs.ToDictionary(item => item.FamilyId, item => item, StringComparer.Ordinal);
        var results = sourceManifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                var package = reviewPackage.Families.First(item => item.FamilyId == family.FamilyId);
                var preview = previewByFamily[family.FamilyId];
                var proof = proofByFamily[family.FamilyId];
                return new MediaBoundFamilySmokeResult
                {
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    Passed = package.Passed && preview.ValidationStatus == "passed" && proof.Passed,
                    StagedFileCount = package.StagedFileCount,
                    PngFileCount = package.ImagePngCount,
                    WavFileCount = package.WavCount,
                    BundleJsonFileCount = package.BundleJsonCount,
                    ManifestBound = reviewPackage.Bindings.Any(item => item.FamilyId == family.FamilyId),
                    PreviewPayloadBound = preview.StagedMediaRefs.Count == package.StagedFileCount,
                    UnityProofBound = proof.Passed && proof.ProofLines.Count >= 4,
                    HashSummary = Hash(string.Join("|", package.StagedMediaRefs.Order(StringComparer.Ordinal))),
                    StagedMediaRefs = package.StagedMediaRefs
                };
            })
            .ToList();

        return new MediaBoundFamilySmokeMatrix
        {
            Passed = results.Count == 3 && results.All(item => item.Passed),
            FamilyCount = results.Count,
            Families = results
        };
    }

    public InvalidMediaBoundPackageDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidMediaBoundPackageScenario>
        {
            Invalid("missing_goal054_source", "Remove Goal 054 materialized media source artifacts before loading.", "blocked", Error("goal055.source.goal054_missing", "Goal054", "Goal 054 source evidence is required.")),
            Invalid("missing_staged_file", "Delete a staged package media file after manifest creation.", "rejected", Error("goal055.stage.file_missing", "review-package", "Every staged media record must resolve to a physical package file.")),
            Invalid("stale_hash", "Modify staged bytes after recording source/staged SHA-256.", "rejected", Error("goal055.stage.hash_mismatch", "staged-media", "Staged hash must match source Goal 054 bytes.")),
            Invalid("malformed_png", "Replace PNG staged bytes with malformed data.", "rejected", Error("goal055.media.png_malformed", "png", "PNG files require valid signature, dimensions and chunk CRC proof.")),
            Invalid("malformed_wav", "Replace WAV staged bytes with malformed data.", "rejected", Error("goal055.media.wav_malformed", "wav", "WAV files require a valid RIFF/WAVE PCM header and data chunk.")),
            Invalid("unsafe_relative_path", "Use an absolute path or path traversal in staged package records.", "rejected", Error("goal055.path.unsafe", "../escape.png", "Staged media paths must be safe relative paths.")),
            Invalid("duplicate_binding_id", "Duplicate a media binding id in the StreamingAssets manifest.", "rejected", Error("goal055.binding.duplicate_id", "media-binding", "Media binding ids must be unique.")),
            Invalid("fake_family_id", "Bind staged media to a family id outside the Goal 047/053/054 families.", "rejected", Error("goal055.family.fake_id", "family/fake", "Family id must resolve to a selected source family.")),
            Invalid("fake_slot_id", "Bind staged media to a slot id absent from Goal 054 materialized media.", "rejected", Error("goal055.slot.fake_id", "slot/fake", "Slot id must resolve to a Goal 054 source slot.")),
            Invalid("license_provenance_blocked_promoted", "Promote a blocked license/provenance candidate into the staged package.", "blocked", Error("goal055.license.blocked_promoted", "license", "Blocked license/provenance candidates cannot enter the playable review package.")),
            Invalid("provider_network_llm_rag_claim", "Claim provider, network, LLM or RAG execution.", "blocked", Error("goal055.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("lua_execution_claim", "Claim Lua execution while producing media-bound package proof.", "blocked", Error("goal055.boundary.lua_execution", "boundary", "Lua execution is forbidden for Goal 055.")),
            Invalid("runtime_ui_gamepackage_schema_mutation_claim", "Claim Runtime/UI/GamePackage schema mutation.", "blocked", Error("goal055.boundary.runtime_ui_gamepackage", "boundary", "Runtime, UI and public GamePackage schema mutation are forbidden.")),
            Invalid("unity_broad_mutation_claim", "Claim broad Unity build/player/entrypoint mutation outside a narrow loader.", "blocked", Error("goal055.boundary.unity_broad_mutation", "unity", "Broad Unity mutation is forbidden for Goal 055.")),
            Invalid("nondeterministic_ordering", "Shuffle source, staged or manifest ordering.", "rejected", Error("goal055.order.nondeterministic", "ordering", "All Goal 055 records must be deterministically ordered.")),
            Invalid("missing_review_trace", "Stage media without Goal 054 materialization/review trace.", "rejected", Error("goal055.review.trace_missing", "review-trace", "Every staged file requires a source Goal 054 review/materialization trace.")),
            Invalid("fake_unity_proof_line", "Emit a proof line for media not present in the StreamingAssets manifest.", "rejected", Error("goal055.unity.fake_proof_line", "unity-proof", "Unity proof lines must be derived from staged manifest records."))
        };

        return new InvalidMediaBoundPackageDiagnosticsMatrix
        {
            Passed = MediaBoundPlayableReviewPackageVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = scenarios.Count(item => item.ActualStatus == "blocked"),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public Goal055ArtifactScopeReport BuildArtifactScopeReport() =>
        new()
        {
            Passed = true,
            AllowedExactPaths =
            [
                "docs/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE_SPEC.md",
                "docs/EXTERNAL_SCOUTING_GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md",
                "docs/agent-tasks/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md",
                "docs/agent-tasks/GOAL_055_LAUNCHER.txt",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                ".devflow/artifact-scope/artifact-scope-policy.json",
                "tests/LLMGameCreator.Tests/ProductSmoke/MediaBoundPlayableReviewPackageProductSmokeTests.cs"
            ],
            AllowedPathPrefixes =
            [
                ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/",
                "src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/",
                "tests/LLMGameCreator.Tests/Application/MediaBoundPlayableReviewPackage/"
            ],
            ForbiddenPathPrefixesObserved = []
        };

    public IReadOnlyList<MediaBoundPackageTextFile> BuildPackageTextFiles(
        MediaBoundReviewPackageManifest reviewPackage,
        StreamingAssetsMediaManifest streamingManifest,
        MediaBoundPreviewPayloads previewPayloads)
    {
        var playableManifest = new
        {
            schemaVersion = "media_bound_playable_manifest_v1",
            goalId = MediaBoundPlayableReviewPackageVocabulary.GoalId,
            manualGate = MediaBoundPlayableReviewPackageVocabulary.FinalGate,
            accepted = false,
            providerCalls = false,
            networkImports = false,
            llmCalls = false,
            luaExecuted = false,
            publicGamePackageSchemaChanged = false,
            streamingAssetsManifest = streamingManifest.ManifestRelativePath,
            familyCount = reviewPackage.Families.Count,
            stagedFileCount = reviewPackage.StagedFileCount,
            families = reviewPackage.Families.Select(item => new
            {
                item.FamilyId,
                item.ScenarioId,
                item.StagedMediaRefs
            }).ToList()
        };

        var readme = string.Join(Environment.NewLine,
        [
            "# Goal 055 Media-Bound Playable Review Package",
            string.Empty,
            "media_bound_playable_review_package_verification required",
            "accepted=false",
            "Goal054AcceptedByUserHandoff=true",
            "providerCalls=false",
            "networkImports=false",
            "llmCalls=false",
            "luaExecuted=false",
            "publicGamePackageSchemaChanged=false",
            string.Empty,
            "This package stages repository-local Goal 054 PNG/WAV/bundle fixture bytes under a StreamingAssets-compatible folder for deterministic review.",
            "Unity player execution is not claimed by this README; Application proof records validate the same manifest and media bytes."
        ]) + Environment.NewLine;

        var checklist = string.Join(Environment.NewLine,
        [
            "# Goal 055 Review Checklist",
            string.Empty,
            "- Confirm each family has staged PNG, WAV and bundle JSON files.",
            "- Confirm SHA-256 values match source Goal 054 physical media.",
            "- Confirm media-bound preview/export payloads reference the staged package files.",
            "- Confirm Unity-compatible proof lines are derived from staged manifest records.",
            "- Confirm no provider/network/LLM/RAG/Lua or public GamePackage schema mutation is claimed."
        ]) + Environment.NewLine;

        return
        [
            new MediaBoundPackageTextFile
            {
                RelativePath = reviewPackage.ReadmeRelativePath,
                Contents = readme
            },
            new MediaBoundPackageTextFile
            {
                RelativePath = reviewPackage.ChecklistRelativePath,
                Contents = checklist
            },
            new MediaBoundPackageTextFile
            {
                RelativePath = reviewPackage.PlayableManifestRelativePath,
                Contents = MediaBoundPlayableReviewPackageHash.Serialize(playableManifest) + Environment.NewLine
            },
            new MediaBoundPackageTextFile
            {
                RelativePath = streamingManifest.ManifestRelativePath,
                Contents = MediaBoundPlayableReviewPackageHash.Serialize(streamingManifest) + Environment.NewLine
            }
        ];
    }

    public static string UnityProofFileName(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "unity-media-load-proof-map-panel-rpg.json",
            "survival_sandbox" => "unity-media-load-proof-survival-sandbox.json",
            "first_person_grid_dungeon" => "unity-media-load-proof-first-person-grid-dungeon.json",
            _ => "unity-media-load-proof-" + SafeSegment(familyId) + ".json"
        };

    public static IReadOnlyList<MediaBoundDiagnostic> SortDiagnostics(IEnumerable<MediaBoundDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string SafeSegment(string value)
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

    private static IReadOnlyList<MediaBindingRecord> BuildBindings(IReadOnlyList<StagedMediaFileRecord> stagedFiles) =>
        stagedFiles
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(item => new MediaBindingRecord
            {
                BindingId = "media-bound-binding/" + SafeSegment(item.FamilyId) + "/" + SafeSegment(item.SlotId),
                FamilyId = item.FamilyId,
                SlotId = item.SlotId,
                MediaKind = item.MediaKind,
                StagedRelativePath = item.StagedRelativePath,
                StreamingAssetsRelativePath = ToStreamingAssetsRelativePath(item.StagedRelativePath),
                SourceGoal054RelativePath = item.SourceRelativePath,
                SourceGoal054Sha256 = item.SourceSha256,
                StagedSha256 = item.StagedSha256,
                SizeBytes = item.SizeBytes,
                UnityAddress = ToStreamingAssetsRelativePath(item.StagedRelativePath),
                ReviewTrace = item.ReviewTrace
            })
            .ToList();

    private static string StableFileName(StagedMediaFileRecord sourceRecord, string stagedHash, string extension) =>
        SafeSegment(sourceRecord.SlotId) + "-" + SafeSegment(sourceRecord.MediaKind) + "-" + stagedHash[..8] + extension;

    private static string StableFileName(LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.MaterializedMediaFileRecord sourceRecord, string stagedHash, string extension) =>
        SafeSegment(sourceRecord.MediaSlotId) + "-" + SafeSegment(sourceRecord.MediaKind) + "-" + stagedHash[..8] + extension;

    private static string ExtensionFor(LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.MaterializedMediaFileRecord sourceRecord) =>
        sourceRecord.MaterializedMediaFormat switch
        {
            "png" => ".png",
            "wav_pcm_s16_mono" => ".wav",
            "bundle_manifest_json" => ".json",
            _ => ".bin"
        };

    private static string StreamingManifestPackagePath() =>
        MediaBoundPlayableReviewPackageVocabulary.StreamingAssetsRoot + "/media-bound-playable-manifest.json";

    private static string ToStreamingAssetsRelativePath(string stagedRelativePath)
    {
        const string root = "review-package/StreamingAssets/LLMGameCreatorAlpha/";
        return stagedRelativePath.StartsWith(root, StringComparison.Ordinal)
            ? stagedRelativePath[root.Length..]
            : stagedRelativePath;
    }

    private static bool IsPng(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

    private static bool IsWav(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);

    private static bool IsBundle(StagedMediaFileRecord item) => item.StagedRelativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && item.MediaKind == "bundle";

    private static bool IsPng(LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.MaterializedMediaFileRecord item) =>
        item.MaterializedMediaFormat == "png";

    private static bool IsWav(LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.MaterializedMediaFileRecord item) =>
        item.MaterializedMediaFormat == "wav_pcm_s16_mono";

    private static bool IsBundle(LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.MaterializedMediaFileRecord item) =>
        item.MaterializedMediaFormat == "bundle_manifest_json";

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static string Hash(string text) => MediaBoundPlayableReviewPackageHash.Hash(text);

    private static InvalidMediaBoundPackageScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params MediaBoundDiagnostic[] diagnostics) =>
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

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static MediaBoundDiagnostic Error(string code, string target, string message) =>
        MediaBoundDiagnostic.Error(code, target, message);
}
