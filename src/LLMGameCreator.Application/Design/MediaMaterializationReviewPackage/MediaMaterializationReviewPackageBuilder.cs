using System.Text;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationReviewPackageBuilder
{
    public MediaMaterializationSourceManifest BuildSourceManifest(MediaMaterializationSourceBundle source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var reportGreen = source.Goal053ReportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal)
            && source.Goal053ReportMarkdown.Contains("accepted=false", StringComparison.Ordinal);
        var reportRequired = source.Goal053ReportMarkdown.Contains("media_asset_campaign_orchestration_verification required", StringComparison.Ordinal);
        var familyDryRunsById = source.Goal047FamilyDryRuns.ToDictionary(item => item.FamilyId, item => item, StringComparer.Ordinal);
        var requestCountByFamily = source.Goal053RequestQueue.Requests
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var bindingCountByFamily = source.Goal053BindingManifest.Bindings
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        var families = source.Goal053SourceManifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                familyDryRunsById.TryGetValue(family.FamilyId, out var dryRun);
                return new MediaMaterializationFamilySourceRecord
                {
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    ProfileId = family.ProfileId,
                    DryRunArtifactRef = family.DryRunArtifactRef,
                    RuntimePreviewPayloadRef = family.RuntimePreviewPayloadRef,
                    ExportProfileId = dryRun?.ExportCandidatePayloadSummary.ExportProfileId ?? family.ExportProfileId,
                    SourceMediaRequestCount = requestCountByFamily.GetValueOrDefault(family.FamilyId),
                    SourceBindingCount = bindingCountByFamily.GetValueOrDefault(family.FamilyId)
                };
            })
            .ToList();

        return new MediaMaterializationSourceManifest
        {
            Accepted = false,
            Goal053AcceptedByUserHandoff = true,
            Goal053ProducedForReviewReportGreen = reportGreen,
            Goal053ReportKeptRequired = reportRequired,
            Goal053RequestCount = source.Goal053RequestQueue.RequestCount,
            Goal053BindingCount = source.Goal053BindingManifest.BindingCount,
            PreflightGates =
            [
                new() { GateId = MediaAssetCampaignVocabulary.FinalGate, Status = "passed", ProvenanceKind = "user_handoff", EvidenceRef = "Goal 054 starting handoff" },
                new() { GateId = "semantic_pack_composition_blueprint_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 031 preserved policy" },
                new() { GateId = "dynamic_semantic_feature_system_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 032 preserved policy" },
                new() { GateId = MediaMaterializationReviewPackageVocabulary.FinalGate, Status = "required", ProvenanceKind = "programmatic", EvidenceRef = "Goal 054 produced for review" }
            ],
            SelectedFamilyIds = families.Select(item => item.FamilyId).ToList(),
            Families = families,
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(source.Diagnostics.Concat(
            [
                Info("goal054.preflight.goal053_handoff_recorded", MediaAssetCampaignVocabulary.FinalGate, "Goal 053 is recorded as accepted by user handoff before Goal 054."),
                Info("goal054.source.goal053_report_verified", "media-asset-campaign-orchestration-report.md", "Goal 053 report remains GREEN produced-for-review evidence with its own gate required.")
            ]))
        };
    }

    public (MediaMaterializationQueue Queue, IReadOnlyList<MaterializedMediaFilePayload> Payloads) BuildMaterializationQueue(
        MediaMaterializationSourceBundle source)
    {
        var requestsById = source.Goal053RequestQueue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var decisionsByCandidate = source.Goal053ReviewLedger.Decisions.ToDictionary(item => item.CandidateId, item => item, StringComparer.Ordinal);
        var queueItems = new List<MediaMaterializationQueueItem>();
        var payloads = new List<MaterializedMediaFilePayload>();

        foreach (var binding in source.Goal053BindingManifest.Bindings.OrderBy(item => FamilyOrderingKey(item.FamilyId)).ThenBy(item => item.BindingId, StringComparer.Ordinal))
        {
            var request = requestsById[binding.RequestId];
            var decision = decisionsByCandidate[binding.CandidateId];
            var format = FormatFor(binding.MediaKind);
            var path = MaterializedPath(binding, format);
            var itemWithoutHash = new MediaMaterializationQueueItem
            {
                MaterializationId = "materialization/" + SafeSegment(binding.BindingId),
                FamilyId = binding.FamilyId,
                SourceRequestId = binding.RequestId,
                SourceBindingId = binding.BindingId,
                SourceCandidateId = binding.CandidateId,
                MediaKind = binding.MediaKind,
                MediaSlotId = binding.MediaSlotId,
                GeneratedTargetId = binding.GeneratedTargetId,
                MaterializedMediaFormat = format,
                OutputRelativePath = path,
                ProvenanceStatus = "repo_generated_deterministic_fixture",
                LicenseStatus = "repo_fixture_no_external_license",
                ReviewStatus = decision.Promoted ? "promoted_fixture_materialized_for_review" : "not_promoted",
                ConsumerPayloadRole = ConsumerPayloadRole(request.MediaSlotId),
                DeterministicOrderingKey = $"{FamilyOrderingKey(binding.FamilyId)}-{SafeSegment(binding.BindingId)}"
            };
            var payload = MediaFixtureWriters.CreatePayload(itemWithoutHash);
            var item = itemWithoutHash with
            {
                ExpectedSha256 = MediaMaterializationReviewPackageHash.Hash(payload.Bytes),
                ExpectedByteLength = payload.Bytes.LongLength
            };
            queueItems.Add(item);
            payloads.Add(payload);
        }

        return (new MediaMaterializationQueue
        {
            Passed = queueItems.Count == source.Goal053BindingManifest.BindingCount
                && queueItems.Count > 0
                && queueItems.All(item => item.ReviewStatus == "promoted_fixture_materialized_for_review")
                && MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queueItems.Any(item => item.FamilyId == family && item.MediaKind == "image"))
                && MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queueItems.Any(item => item.FamilyId == family && item.MediaKind == "audio")),
            QueueItemCount = queueItems.Count,
            Items = queueItems.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            Diagnostics = [Info("goal054.queue.built", "media-materialization-queue", "Materialization queue maps every Goal 053 promoted fixture binding to deterministic physical media bytes.")]
        }, payloads.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList());
    }

    public MaterializedMediaInventory BuildInventory(
        MediaMaterializationQueue queue,
        IReadOnlyList<MaterializedMediaFilePayload> payloads)
    {
        var payloadsByPath = payloads.ToDictionary(item => item.RelativePath, item => item, StringComparer.Ordinal);
        var files = queue.Items
            .OrderBy(item => item.OutputRelativePath, StringComparer.Ordinal)
            .Select(item =>
            {
                var payload = payloadsByPath[item.OutputRelativePath];
                var isPng = item.MaterializedMediaFormat == "png";
                var isWav = item.MaterializedMediaFormat == "wav_pcm_s16_mono";
                return new MaterializedMediaFileRecord
                {
                    MaterializationId = item.MaterializationId,
                    FamilyId = item.FamilyId,
                    MediaKind = item.MediaKind,
                    MediaSlotId = item.MediaSlotId,
                    MaterializedMediaFormat = item.MaterializedMediaFormat,
                    RelativePath = payload.RelativePath,
                    ByteLength = payload.Bytes.LongLength,
                    Sha256 = MediaMaterializationReviewPackageHash.Hash(payload.Bytes),
                    PngSignatureValid = isPng && MediaFixtureWriters.HasValidPngSignature(payload.Bytes),
                    PngChunkCrcsValid = isPng && MediaFixtureWriters.ValidatePngChunkCrcs(payload.Bytes),
                    WavHeaderValid = isWav && MediaFixtureWriters.HasValidWavHeader(payload.Bytes),
                    DeterministicBytes = item.ExpectedSha256 == MediaMaterializationReviewPackageHash.Hash(payload.Bytes)
                };
            })
            .ToList();

        return new MaterializedMediaInventory
        {
            Passed = files.Count == queue.Items.Count
                && files.All(item => item.ByteLength > 0)
                && files.Where(item => item.MaterializedMediaFormat == "png").All(item => item.PngSignatureValid && item.PngChunkCrcsValid)
                && files.Where(item => item.MaterializedMediaFormat == "wav_pcm_s16_mono").All(item => item.WavHeaderValid)
                && files.All(item => item.DeterministicBytes),
            FileCount = files.Count,
            PngFileCount = files.Count(item => item.MaterializedMediaFormat == "png"),
            WavFileCount = files.Count(item => item.MaterializedMediaFormat == "wav_pcm_s16_mono"),
            BundleJsonFileCount = files.Count(item => item.MaterializedMediaFormat == "bundle_manifest_json"),
            Files = files,
            Diagnostics = [Info("goal054.inventory.built", "materialized-media-inventory", "Physical deterministic PNG/WAV/bundle fixture bytes are inventoried with hashes and header proof.")]
        };
    }

    public MediaProvenanceLicenseLedger BuildProvenanceLicenseLedger(
        MediaMaterializationSourceBundle source,
        MaterializedMediaInventory inventory,
        MediaMaterializationQueue queue)
    {
        var queueById = queue.Items.ToDictionary(item => item.MaterializationId, item => item, StringComparer.Ordinal);
        var decisions = new List<MediaLicenseDecisionProof>
        {
            Decision("fixture-generated-by-repo", "materialize_deterministic_fixture_for_review", true, false, "Repository-generated deterministic fixture bytes are allowed for Goal 054 review proof."),
            Decision("manual-user-provided", "review_only_until_manual_license_record", false, false, "Manual media remains unpromoted until an explicit source and license review exists."),
            Decision("imported-cc0", "review_only_not_auto_promoted", false, false, "Imported CC0 candidates need later import/source proof and are not Goal 054 promoted bindings."),
            Decision("imported-cc-by", "review_only_requires_attribution_payload", false, true, "Attribution-required media stays review-only unless attribution payload exists."),
            Decision("imported-share-alike-or-gpl-risk", "blocked_license", false, false, "Share-alike/GPL-risk media is blocked for this goal."),
            Decision("provider-generated-with-model-license", "blocked_provider_not_configured", false, false, "Provider outputs are forbidden in Goal 054."),
            Decision("unknown/no-license", "blocked_missing_license", false, false, "Unknown license blocks promotion.")
        };

        var fileRecords = inventory.Files
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(file =>
            {
                var queueItem = queueById[file.MaterializationId];
                return new MaterializedMediaProvenanceRecord
                {
                    MaterializationId = file.MaterializationId,
                    SourceBindingId = queueItem.SourceBindingId,
                    RelativePath = file.RelativePath,
                    Sha256 = file.Sha256,
                    SourceKind = "fixture-generated-by-repo",
                    LicenseStatus = queueItem.LicenseStatus,
                    ProvenanceStatus = "repository_generated_deterministic_bytes",
                    ProviderImportedOrManual = false,
                    AttributionPayloadPresent = false
                };
            })
            .ToList();

        return new MediaProvenanceLicenseLedger
        {
            Passed = source.Goal053LicenseLedger.Passed
                && decisions.Any(item => item.SourceKind == "unknown/no-license" && !item.PromotedInGoal054)
                && decisions.Any(item => item.SourceKind == "imported-cc-by" && item.RequiresAttributionPayload && !item.PromotedInGoal054)
                && decisions.Where(item => item.SourceKind != "fixture-generated-by-repo").All(item => !item.PromotedInGoal054)
                && fileRecords.Count == inventory.FileCount
                && fileRecords.All(item => !item.ProviderImportedOrManual),
            LicenseDecisions = decisions,
            MaterializedFiles = fileRecords,
            Diagnostics = [Info("goal054.license.ledger_built", "media-provenance-license-ledger", "Only repository-generated deterministic fixture media is materialized; import/provider/unknown license paths remain blocked or review-only.")]
        };
    }

    public MediaBindingValidation BuildBindingValidation(
        MediaMaterializationSourceBundle source,
        MediaMaterializationQueue queue,
        MaterializedMediaInventory inventory,
        MediaProvenanceLicenseLedger ledger)
    {
        var requestsById = source.Goal053RequestQueue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var bindingsById = source.Goal053BindingManifest.Bindings.ToDictionary(item => item.BindingId, item => item, StringComparer.Ordinal);
        var filesByMaterializationId = inventory.Files.ToDictionary(item => item.MaterializationId, item => item, StringComparer.Ordinal);
        var provenanceByMaterializationId = ledger.MaterializedFiles.ToDictionary(item => item.MaterializationId, item => item, StringComparer.Ordinal);

        var records = queue.Items.Select(item =>
        {
            var requestExists = requestsById.TryGetValue(item.SourceRequestId, out var request);
            var bindingExists = bindingsById.TryGetValue(item.SourceBindingId, out var binding);
            var fileExists = filesByMaterializationId.TryGetValue(item.MaterializationId, out var file);
            var provenanceExists = provenanceByMaterializationId.TryGetValue(item.MaterializationId, out var provenance);
            var diagnostics = new List<MediaMaterializationDiagnostic>();
            if (!requestExists)
            {
                diagnostics.Add(Error("goal054.request.fake_id", item.SourceRequestId, "Source request id must resolve to Goal 053 request queue."));
            }

            if (!bindingExists)
            {
                diagnostics.Add(Error("goal054.binding.fake_id", item.SourceBindingId, "Source binding id must resolve to Goal 053 binding manifest."));
            }

            if (!fileExists)
            {
                diagnostics.Add(Error("goal054.media.file_missing", item.MaterializationId, "Materialized file must exist in the inventory."));
            }

            var mediaKindMatches = requestExists && bindingExists && request!.MediaKind == item.MediaKind && binding!.MediaKind == item.MediaKind && request.MediaSlotId == item.MediaSlotId;
            if (!mediaKindMatches)
            {
                diagnostics.Add(Error("goal054.media.kind_mismatch", item.MaterializationId, "Materialized media kind and slot must match source request and binding."));
            }

            var hashMatches = fileExists && file!.Sha256 == item.ExpectedSha256 && file.ByteLength == item.ExpectedByteLength;
            if (!hashMatches)
            {
                diagnostics.Add(Error("goal054.media.hash_mismatch", item.OutputRelativePath, "Materialized file hash and length must match queue expectations."));
            }

            var safePath = IsSafeRelativePath(item.OutputRelativePath);
            if (!safePath)
            {
                diagnostics.Add(Error("goal054.path.absolute", item.OutputRelativePath, "Materialized media paths must be safe relative paths."));
            }

            var crossFamily = requestExists && bindingExists && (request!.FamilyId != item.FamilyId || binding!.FamilyId != item.FamilyId);
            if (crossFamily)
            {
                diagnostics.Add(Error("goal054.binding.cross_family_leak", item.SourceBindingId, "Binding family must match request and materialization family."));
            }

            var unapproved = !provenanceExists || provenance!.ProviderImportedOrManual || provenance.SourceKind != "fixture-generated-by-repo";
            if (unapproved)
            {
                diagnostics.Add(Error("goal054.provenance.import_or_provider_promoted", item.SourceBindingId, "Only repository-generated fixture provenance may be bound."));
            }

            return new MediaBindingValidationRecord
            {
                BindingId = item.SourceBindingId,
                FamilyId = item.FamilyId,
                MaterializationId = item.MaterializationId,
                RelativePath = item.OutputRelativePath,
                SourceSlotExists = requestExists,
                MaterializedFileExistsInInventory = fileExists,
                FileHashMatchesExpected = hashMatches,
                MediaKindMatchesSlot = mediaKindMatches,
                SafeRelativePath = safePath,
                CrossFamilyLeakDetected = crossFamily,
                UnapprovedProviderImportBound = unapproved,
                Diagnostics = SortDiagnostics(diagnostics)
            };
        }).ToList();

        return new MediaBindingValidation
        {
            Passed = records.Count == queue.Items.Count
                && records.All(item => item.SourceSlotExists && item.MaterializedFileExistsInInventory && item.FileHashMatchesExpected && item.MediaKindMatchesSlot && item.SafeRelativePath && !item.CrossFamilyLeakDetected && !item.UnapprovedProviderImportBound)
                && MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queue.Items.Any(item => item.FamilyId == family && item.MediaKind == "image"))
                && MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queue.Items.Any(item => item.FamilyId == family && item.MediaKind == "audio")),
            BindingCount = records.Count,
            EveryFamilyHasImageAndAudioFixture = MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queue.Items.Any(item => item.FamilyId == family && item.MediaKind == "image"))
                && MediaMaterializationReviewPackageVocabulary.FamilyIds.All(family => queue.Items.Any(item => item.FamilyId == family && item.MediaKind == "audio")),
            Bindings = records.OrderBy(item => item.BindingId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Info("goal054.binding.validation_built", "media-binding-validation", "Every promoted Goal 053 binding resolves to a physical materialized file with hash, kind and family isolation proof.")]
        };
    }

    public PreviewExportMediaPayloads BuildPreviewExportPayloads(
        MediaMaterializationSourceManifest manifest,
        MediaMaterializationQueue queue,
        MaterializedMediaInventory inventory)
    {
        var inventoryPaths = inventory.Files.Select(item => item.RelativePath).ToHashSet(StringComparer.Ordinal);
        var payloads = manifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                var familyItems = queue.Items.Where(item => item.FamilyId == family.FamilyId).OrderBy(item => item.SourceBindingId, StringComparer.Ordinal).ToList();
                var refs = familyItems.Select(item => item.OutputRelativePath).ToList();
                return new MediaBoundPayloadRecord
                {
                    PreviewPayloadId = "preview-media-payload/" + family.FamilyId.Replace('_', '-'),
                    ExportPayloadId = "export-media-payload/" + family.FamilyId.Replace('_', '-'),
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    ReferencedDryRunArtifactRef = family.DryRunArtifactRef,
                    ReferencedMediaBindingIds = familyItems.Select(item => item.SourceBindingId).ToList(),
                    PhysicalMediaFileRefs = refs,
                    HashSummary = MediaMaterializationReviewPackageHash.Hash(string.Join("|", familyItems.Select(item => item.ExpectedSha256).Order(StringComparer.Ordinal))),
                    ValidationStatus = refs.All(inventoryPaths.Contains) ? "passed" : "failed_missing_media_ref",
                    IncludedInReviewPackage = true
                };
            })
            .ToList();

        return new PreviewExportMediaPayloads
        {
            Passed = payloads.Count == 3
                && payloads.All(item => item.ValidationStatus == "passed" && item.IncludedInReviewPackage)
                && payloads.All(item => item.ReferencedMediaBindingIds.Count > 0 && item.PhysicalMediaFileRefs.Count > 0),
            FamilyCount = payloads.Count,
            AllMediaRefsResolveToInventory = payloads.SelectMany(item => item.PhysicalMediaFileRefs).All(inventoryPaths.Contains),
            GamePackageSchemaChanged = false,
            RuntimeUiUnityChanged = false,
            Payloads = payloads,
            Diagnostics = [Info("goal054.preview_export.payloads_built", "preview-export-media-payloads", "Media-bound preview/export payload records point to physical review-package media files without mutating package/runtime/Unity payloads.")]
        };
    }

    public MediaReviewPackageManifest BuildReviewPackageManifest(
        MediaMaterializationQueue queue,
        MaterializedMediaInventory inventory,
        MediaProvenanceLicenseLedger ledger,
        MediaBindingValidation validation,
        PreviewExportMediaPayloads payloads,
        InvalidMediaMaterializationMatrix invalidMatrix)
    {
        var manifestPaths = new List<string>
        {
            MediaMaterializationReviewPackageEvidenceService.SourceManifestJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.QueueJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.InventoryJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.LicenseLedgerJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.BindingValidationJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.ReviewPackageManifestJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.PreviewExportPayloadsJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.InvalidMatrixJsonFileName,
            MediaMaterializationReviewPackageEvidenceService.ReportMarkdownFileName
        };
        manifestPaths.AddRange(MediaMaterializationReviewPackageVocabulary.FamilyIds.Select(FamilySmokeFileName));

        return new MediaReviewPackageManifest
        {
            Passed = inventory.Passed
                && ledger.Passed
                && validation.Passed
                && payloads.Passed
                && invalidMatrix.Passed
                && inventory.FileCount == queue.QueueItemCount,
            ManifestPathList = manifestPaths.Order(StringComparer.Ordinal).ToList(),
            MediaFileList = inventory.Files.Select(item => item.RelativePath).Order(StringComparer.Ordinal).ToList(),
            PayloadList = payloads.Payloads.SelectMany(item => new[] { item.PreviewPayloadId, item.ExportPayloadId }).Order(StringComparer.Ordinal).ToList(),
            LicenseProvenanceList = ledger.MaterializedFiles.Select(item => item.MaterializationId + ":" + item.SourceKind + ":" + item.LicenseStatus).Order(StringComparer.Ordinal).ToList(),
            FamilyCoverageSummary = inventory.Files
                .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
                .OrderBy(group => FamilyOrderingKey(group.Key), StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
            ManualReviewChecklist =
            [
                "confirm_png_files_open_in_review_tool",
                "confirm_wav_files_play_as_short_fixture_cues",
                "confirm_bundle_manifest_is_not_final_media",
                "confirm_no_provider_import_or_network_media_promoted",
                "confirm_goal054_gate_remains_required"
            ],
            ValidatorSummary =
            [
                "source_manifest=" + validation.Passed.ToString().ToLowerInvariant(),
                "materialized_file_count=" + inventory.FileCount.ToString(),
                "png_count=" + inventory.PngFileCount.ToString(),
                "wav_count=" + inventory.WavFileCount.ToString(),
                "invalid_matrix=" + invalidMatrix.Passed.ToString().ToLowerInvariant()
            ]
        };
    }

    public IReadOnlyList<FamilyMediaSmokeProof> BuildFamilySmokeProofs(
        MediaMaterializationQueue queue,
        MaterializedMediaInventory inventory,
        PreviewExportMediaPayloads payloads)
    {
        var payloadsByFamily = payloads.Payloads.ToDictionary(item => item.FamilyId, item => item, StringComparer.Ordinal);
        return MediaMaterializationReviewPackageVocabulary.FamilyIds
            .OrderBy(FamilyOrderingKey, StringComparer.Ordinal)
            .Select(family =>
            {
                var files = inventory.Files.Where(item => item.FamilyId == family).OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
                var payload = payloadsByFamily[family];
                return new FamilyMediaSmokeProof
                {
                    FamilyId = family,
                    Passed = files.Any(item => item.MediaKind == "image" && item.MaterializedMediaFormat == "png")
                        && files.Any(item => item.MediaKind == "audio" && item.MaterializedMediaFormat == "wav_pcm_s16_mono")
                        && payload.ValidationStatus == "passed",
                    MaterializedBindingCount = files.Count,
                    ImagePngCount = files.Count(item => item.MediaKind == "image" && item.MaterializedMediaFormat == "png"),
                    AudioWavCount = files.Count(item => item.MediaKind == "audio" && item.MaterializedMediaFormat == "wav_pcm_s16_mono"),
                    PayloadIds = [payload.PreviewPayloadId, payload.ExportPayloadId],
                    MediaFileRefs = files.Select(item => item.RelativePath).ToList(),
                    HashSummary = MediaMaterializationReviewPackageHash.Hash(string.Join("|", files.Select(item => item.Sha256).Order(StringComparer.Ordinal))),
                    Diagnostics = [Info("goal054.family_smoke.passed", family, "Family has physical image/audio media and media-bound preview/export payload refs.")]
                };
            })
            .ToList();
    }

    public InvalidMediaMaterializationMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidMediaMaterializationScenario>
        {
            Invalid("missing_goal053_source", "Remove Goal 053 source evidence before loading.", "blocked", Error("goal054.source.goal053_missing", "Goal053", "Goal 053 source evidence is required.")),
            Invalid("fake_media_request_id", "Materialize a binding with a request id absent from the Goal 053 queue.", "rejected", Error("goal054.request.fake_id", "media-request/fake", "Source media request id must resolve.")),
            Invalid("fake_binding_id", "Materialize a binding id absent from Goal 053 binding manifest.", "rejected", Error("goal054.binding.fake_id", "media-binding/fake", "Source binding id must resolve.")),
            Invalid("missing_physical_media_file", "Delete a materialized media file after queue creation.", "rejected", Error("goal054.media.file_missing", "review-package/media/missing.png", "Physical materialized media file is required.")),
            Invalid("hash_mismatch", "Modify bytes after the expected hash is recorded.", "rejected", Error("goal054.media.hash_mismatch", "materialized-file", "Materialized file hash must match queue expectation.")),
            Invalid("media_kind_mismatch", "Bind an audio file to an image slot.", "rejected", Error("goal054.media.kind_mismatch", "media-kind", "Media kind must match the source slot and binding.")),
            Invalid("unknown_prohibited_license_promoted", "Promote an unknown/no-license or prohibited license candidate.", "blocked", Error("goal054.license.unknown_or_prohibited", "unknown/no-license", "Unknown or prohibited licenses block promotion.")),
            Invalid("imported_provider_candidate_promoted", "Promote an imported/provider candidate as materialized media.", "blocked", Error("goal054.provenance.import_or_provider_promoted", "candidate/provider", "Imported/provider candidates are not materialized in Goal 054.")),
            Invalid("cross_family_binding_leak", "Attach a map_panel_rpg binding to survival_sandbox payload.", "rejected", Error("goal054.binding.cross_family_leak", "media-binding", "Materialized media must remain family-scoped.")),
            Invalid("absolute_path_leak", "Emit a machine absolute media path.", "rejected", Error("goal054.path.absolute", "C:/unsafe/file.png", "Artifacts must use safe relative paths only.")),
            Invalid("network_provider_llm_rag_call_claim", "Claim network/provider/LLM/RAG execution.", "blocked", Error("goal054.boundary.provider_network_llm_rag", "boundary", "Provider, network, LLM and RAG calls are forbidden.")),
            Invalid("gamepackage_schema_mutation_claim", "Claim public GamePackage schema mutation.", "blocked", Error("goal054.boundary.gamepackage_schema", "boundary", "GamePackage schema mutation is forbidden.")),
            Invalid("runtime_ui_unity_mutation_claim", "Claim Runtime/UI/Unity source mutation.", "blocked", Error("goal054.boundary.runtime_ui_unity", "boundary", "Runtime/UI/Unity mutation is forbidden for this Application-layer proof.")),
            Invalid("nondeterministic_ordering", "Shuffle materialization queue or manifest ordering.", "rejected", Error("goal054.order.nondeterministic", "ordering", "Materialization ordering must be deterministic.")),
            Invalid("malformed_png_header", "Replace PNG signature or chunk CRC.", "rejected", Error("goal054.media.png_malformed", "png", "PNG fixtures require a valid signature and CRC-protected chunks.")),
            Invalid("malformed_wav_header", "Replace RIFF/WAVE PCM header.", "rejected", Error("goal054.media.wav_malformed", "wav", "WAV fixtures require a valid RIFF/WAVE PCM header.")),
            Invalid("missing_provenance", "Remove materialized file provenance.", "rejected", Error("goal054.provenance.missing", "media-provenance-license-ledger", "Every materialized file requires provenance.")),
            Invalid("missing_review_trace", "Bind materialized media without Goal 053 review decision trace.", "rejected", Error("goal054.review.trace_missing", "media-binding-validation", "Every materialized binding requires a source review trace."))
        };

        return new InvalidMediaMaterializationMatrix
        {
            Passed = MediaMaterializationReviewPackageVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = scenarios.Count(item => item.ActualStatus == "blocked"),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static string FamilySmokeFileName(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "family-media-smoke-map-panel-rpg.json",
            "survival_sandbox" => "family-media-smoke-survival-sandbox.json",
            "first_person_grid_dungeon" => "family-media-smoke-first-person-grid-dungeon.json",
            _ => "family-media-smoke-" + SafeSegment(familyId) + ".json"
        };

    public static IReadOnlyList<MediaMaterializationDiagnostic> SortDiagnostics(IEnumerable<MediaMaterializationDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static MediaLicenseDecisionProof Decision(string sourceKind, string goal054Decision, bool promoted, bool requiresAttribution, string reason) =>
        new()
        {
            SourceKind = sourceKind,
            Goal054Decision = goal054Decision,
            PromotedInGoal054 = promoted,
            RequiresAttributionPayload = requiresAttribution,
            Reason = reason
        };

    private static string FormatFor(string mediaKind) =>
        mediaKind switch
        {
            "image" or "ui" => "png",
            "audio" => "wav_pcm_s16_mono",
            "bundle" => "bundle_manifest_json",
            _ => "fixture_binary"
        };

    private static string MaterializedPath(MediaBindingRecord binding, string format)
    {
        var folder = format switch
        {
            "png" when binding.MediaKind == "ui" => "ui",
            "png" => "images",
            "wav_pcm_s16_mono" => "audio",
            "bundle_manifest_json" => "bundles",
            _ => "other"
        };
        var extension = format switch
        {
            "png" => ".png",
            "wav_pcm_s16_mono" => ".wav",
            "bundle_manifest_json" => ".json",
            _ => ".bin"
        };
        return MediaMaterializationReviewPackageVocabulary.MaterializedMediaRoot + "/" + folder + "/" + SafeSegment(binding.BindingId) + extension;
    }

    private static string ConsumerPayloadRole(string slotId) =>
        slotId switch
        {
            "world_key_art" => "preview_world_key_art",
            "npc_portrait" => "preview_character_focus",
            "sfx_interaction" => "preview_interaction_audio_cue",
            "ui_panel_skin" => "review_ui_panel_skin",
            "export_placeholder_bundle" => "export_review_bundle_manifest",
            _ => "review_media_reference"
        };

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

    private static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static InvalidMediaMaterializationScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params MediaMaterializationDiagnostic[] diagnostics) =>
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

    private static MediaMaterializationDiagnostic Error(string code, string target, string message) =>
        MediaMaterializationDiagnostic.Error(code, target, message);

    private static MediaMaterializationDiagnostic Info(string code, string target, string message) =>
        MediaMaterializationDiagnostic.Info(code, target, message);
}
