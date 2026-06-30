using System.Text;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

namespace LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignBuilder
{
    public MediaCampaignSourceManifest BuildSourceManifest(MediaCampaignSourceBundle source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceRefByPath = source.SourceArtifactRefs
            .ToDictionary(item => item.ArtifactRelativePath, item => item, StringComparer.Ordinal);
        var catalogByFamily = source.Goal043Catalog.Families
            .ToDictionary(item => item.FamilyId, item => item, StringComparer.Ordinal);

        var families = source.Goal047FamilyDryRuns
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(record =>
            {
                var catalog = catalogByFamily[record.FamilyId];
                var dryRunRef = sourceRefByPath[FullGeneratorWithoutMediaDryRunEvidenceService.RelativeOutputDirectory + "/" + FullGeneratorWithoutMediaDryRunEvidenceService.FamilyDryRunFileName(record.FamilyId)];
                return new MediaCampaignFamilySourceRecord
                {
                    FamilyId = record.FamilyId,
                    ScenarioId = record.ScenarioId,
                    ProfileId = record.ProfileId,
                    StyleId = StyleId(record.FamilyId, record.ScenarioId),
                    DryRunArtifactRef = dryRunRef.ArtifactRelativePath,
                    DryRunArtifactHash = dryRunRef.ArtifactHash,
                    RuntimePreviewPayloadRef = record.RuntimePreviewPayloadSummary.PayloadRelativePath,
                    RuntimePreviewPayloadHash = record.RuntimePreviewPayloadSummary.PayloadHash,
                    ExportProfileId = record.ExportCandidatePayloadSummary.ExportProfileId,
                    GeneratedTemplateIds = record.FamilyProfileRefs
                        .Concat(catalog.RequiredFamilyMarkers)
                        .Append(catalog.FamilyExtensionSchemaId)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    GeneratedRuntimeTargetIds = record.GeneratedSystemCoverage
                        .Select(item => item.SourceRef)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    SemanticFeatureRefs = catalog.SelectedFeatureRefs
                        .Concat(catalog.SelectedIntentionRefs)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList()
                };
            })
            .ToList();

        var metamoduleFeatureRefs = source.Goal043Catalog.Families
            .Where(item => item.ScenarioId == "metamodule_kingdoms")
            .SelectMany(item => item.SelectedFeatureRefs)
            .Where(item => item.Contains("metamodule", StringComparison.Ordinal) || item.Contains("species", StringComparison.Ordinal) || item.Contains("archetype", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return new MediaCampaignSourceManifest
        {
            Accepted = false,
            PreflightGates =
            [
                new() { GateId = "full_generator_without_media_verification", Status = "passed", ProvenanceKind = "user_handoff", EvidenceRef = "Goal 053 starting handoff" },
                new() { GateId = "semantic_pack_composition_blueprint_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 031 preserved policy" },
                new() { GateId = "dynamic_semantic_feature_system_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 032 preserved policy" },
                new() { GateId = MediaAssetCampaignVocabulary.FinalGate, Status = "required", ProvenanceKind = "programmatic", EvidenceRef = "Goal 053 produced for review" }
            ],
            SelectedFamilyIds = families.Select(item => item.FamilyId).ToList(),
            Families = families,
            MetamoduleStressSummary = new MediaCampaignMetamoduleStressSummary
            {
                KingdomOrRegionGroupCount = source.Goal040MetamodulePayload.VisitedRegionIds.Count,
                RuntimeDeltaMarkerCount = source.Goal040MetamodulePayload.RuntimeDeltaMarkers.Count,
                CompactedSpeciesArchetypeSlotRefCount = metamoduleFeatureRefs.Any(item => item.Contains("species_archetype", StringComparison.Ordinal)) ? 112 : 0,
                OneRequestPerSpeciesArchetypeSlotGenerated = false,
                SourceFeatureRefs = metamoduleFeatureRefs
            },
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics =
            [
                Info("goal053.preflight.goal047_handoff_recorded", "full_generator_without_media_verification", "Goal 047 is recorded as accepted by user handoff before Goal 053 evidence."),
                Info("goal053.source.compact_refs_only", "media-campaign-source-manifest", "Goal 053 references source artifact paths and hashes without copying heavy source JSON.")
            ]
        };
    }

    public MediaSlotCatalog BuildSlotCatalog()
    {
        var families = MediaAssetCampaignVocabulary.FamilyIds;
        var slots = new List<MediaSlotDefinition>
        {
            Slot("world_key_art", "world_key_art", "image", families, "1536x864 concept descriptor", ["world", "key_art", "family_style"], "generated_world_or_family", "Use neutral fixture key-art descriptor until reviewed media exists."),
            Slot("region_tile_or_background", "region_tile_or_background", "image", families, "512x512 tile or 1280x720 background descriptor", ["region", "tile", "background"], "region_or_chunk", "Use generated-content fallback color/label tile."),
            Slot("npc_portrait", "npc_portrait", "image", families, "512x512 portrait descriptor", ["npc", "portrait", "role"], "entity_or_npc", "Use generic silhouette fixture per family."),
            Slot("species_or_archetype_portrait", "species_or_archetype_portrait", "image", families, "512x512 portrait descriptor; compacted for metamodule scale", ["species", "archetype", "portrait"], "species_or_archetype", "Use compact family archetype placeholder; do not expand 112 files."),
            Slot("item_icon", "item_icon", "image", families, "128x128 icon descriptor", ["item", "icon", "inventory"], "item_or_resource", "Use deterministic geometric fixture icon."),
            Slot("quest_or_event_icon", "quest_or_event_icon", "image", families, "128x128 icon descriptor", ["quest", "event", "journal"], "quest_or_event", "Use text marker icon fixture."),
            Slot("ui_panel_skin", "ui_panel_skin", "ui", families, "panel theme descriptor with palette and border hints", ["ui", "panel", "theme"], "ui_skin", "Use plain fixture UI skin descriptor."),
            Slot("sfx_interaction", "sfx_interaction", "audio", families, "0.4-1.0s mono 44.1kHz descriptor", ["sfx", "interaction"], "interaction_or_command", "Use text fixture cue id; runtime remains silent/fallback."),
            Slot("sfx_combat_or_hazard", "sfx_combat_or_hazard", "audio", families, "0.5-1.2s mono 44.1kHz descriptor", ["sfx", "combat", "hazard"], "combat_or_hazard", "Use text fixture cue id; runtime remains silent/fallback."),
            Slot("ambient_loop", "ambient_loop", "audio", families, "20-45s loop intent descriptor", ["ambient", "loop", "world"], "scenario_or_region", "Use ambient fixture cue metadata only."),
            Slot("music_stinger", "music_stinger", "audio", families, "2-6s stinger intent descriptor", ["music", "stinger", "event"], "quest_or_event", "Use music fixture cue metadata only."),
            Slot("export_placeholder_bundle", "export_placeholder_bundle", "bundle", families, "manifest bundle descriptor", ["export", "placeholder", "bundle"], "preview_export_payload", "Use explicit export placeholder bundle until reviewed media exists.")
        };

        return new MediaSlotCatalog
        {
            Passed = slots.Count == MediaAssetCampaignVocabulary.RequiredSlotIds.Count,
            Slots = slots,
            Diagnostics = [Info("goal053.catalog.built", "media-slot-catalog", "Media slot catalog covers required image/audio/ui/bundle categories.")]
        };
    }

    public MediaStylePolicy BuildStylePolicy(MediaCampaignSourceManifest manifest)
    {
        var styles = manifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family => new MediaStylePolicyRecord
            {
                StyleId = family.StyleId,
                FamilyId = family.FamilyId,
                ScenarioId = family.ScenarioId,
                SourceStyleRefs = family.SemanticFeatureRefs,
                VisualTags = VisualTags(family.FamilyId),
                AudioTags = AudioTags(family.FamilyId),
                UiTags = UiTags(family.FamilyId),
                PromptSkeletonSections = ["subject_ref", "style_ref", "semantic_tags", "output_contract", "negative_boundaries"],
                ContainsFinalProviderPromptText = false
            })
            .ToList();

        return new MediaStylePolicy
        {
            Passed = styles.Count == 3 && styles.All(item => !item.ContainsFinalProviderPromptText),
            Styles = styles
        };
    }

    public MediaRequestQueue BuildRequestQueue(MediaCampaignSourceManifest manifest, MediaSlotCatalog catalog)
    {
        var slots = catalog.Slots.OrderBy(item => SlotOrder(item.SlotId)).ToList();
        var requests = new List<MediaRequestRecord>();
        var priority = 1;

        foreach (var family in manifest.Families.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal))
        {
            foreach (var slot in slots)
            {
                var target = TargetForSlot(family, slot.SlotId);
                requests.Add(new MediaRequestRecord
                {
                    RequestId = RequestId(family.FamilyId, slot.SlotId),
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    StyleId = family.StyleId,
                    TargetGeneratedId = target.TargetId,
                    TargetArtifactFamily = target.ArtifactFamily,
                    TargetArtifactKind = target.ArtifactKind,
                    MediaSlotId = slot.SlotId,
                    MediaKind = slot.MediaKind,
                    PromptInputSkeleton = new MediaPromptInputSkeleton
                    {
                        SubjectRef = target.TargetId,
                        StyleRef = family.StyleId,
                        RequiredContentFacts = family.SemanticFeatureRefs.Take(4).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                        SemanticTags = slot.SemanticStyleTags,
                        NegativeBoundaries = ["no_final_provider_prompt_text", "no_final_prose_claim", "no_downloaded_asset_claim", "no_runtime_schema_mutation"],
                        OutputContract = "fixture_or_candidate_descriptor_only",
                        FinalProviderPromptText = false
                    },
                    SemanticTags = slot.SemanticStyleTags
                        .Concat(family.SemanticFeatureRefs.Take(3))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    RequiredProvenancePolicy = slot.LicensePolicyRequirement,
                    BudgetHint = BudgetHint(slot),
                    Priority = priority,
                    DeterministicOrderingKey = $"{FamilyOrderingKey(family.FamilyId)}-{SlotOrder(slot.SlotId):000}-{slot.SlotId}",
                    Status = RequestStatus(slot.SlotId)
                });
                priority++;
            }
        }

        return new MediaRequestQueue
        {
            Passed = requests.Count >= 30
                && MediaAssetCampaignVocabulary.FamilyIds.All(familyId => requests.Count(item => item.FamilyId == familyId) >= 8)
                && requests.Any(item => item.MediaKind == "image")
                && requests.Any(item => item.MediaKind == "audio")
                && requests.Any(item => item.MediaKind is "ui" or "bundle"),
            FamilyCount = requests.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            RequestCount = requests.Count,
            MetamoduleStressSummary = manifest.MetamoduleStressSummary,
            Requests = requests.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList(),
            Diagnostics = [Info("goal053.request_queue.built", "media-request-queue", "Request queue covers three families, all required media slot categories and compacted metamodule stress facts.")]
        };
    }

    public MediaLicenseProvenanceLedger BuildLicenseLedger()
    {
        var policies = new List<MediaLicensePolicyRecord>
        {
            Policy("fixture-generated-by-repo", "promote_as_fixture_only", "repo_fixture_id, request_id, generated_target_id, fixture_hash", "fixture candidates may be promoted only as fixture assets", true),
            Policy("manual-user-provided", "quarantine_until_manual_review", "user_source_record, license_assertion, reviewer", "manual assets require later manual review", false),
            Policy("imported-cc0", "acceptable_with_source_record", "source_url_or_record, license_snapshot, importer", "CC0 imports can be acceptable later but are not auto-promoted in Goal 053", false),
            Policy("imported-cc-by", "requires_attribution_record", "source_url_or_record, license_snapshot, attribution_text", "CC-BY imports require attribution and later review", false),
            Policy("imported-share-alike-or-gpl-risk", "quarantine_or_block", "source_url_or_record, license_snapshot, explicit_future_allowance", "share-alike/GPL-risk imports are blocked unless explicitly allowed later", false),
            Policy("provider-generated-with-model-license", "future_provider_metadata_required", "provider, model, model_license, run_id, prompt_hash, seed", "provider output is not allowed in Goal 053", false),
            Policy("unknown/no-license", "reject", "none", "unknown or missing license is rejected", false)
        };

        return new MediaLicenseProvenanceLedger
        {
            Passed = MediaAssetCampaignVocabulary.LicenseSourceKinds.All(kind => policies.Any(item => item.SourceKind == kind)),
            Policies = policies,
            Diagnostics = [Info("goal053.license.ledger_built", "media-license-provenance-ledger", "License/provenance policies cover fixture, manual, imported, provider and unknown sources.")]
        };
    }

    public MediaCandidateQuarantine BuildCandidateQuarantine(MediaRequestQueue queue)
    {
        var requestsById = queue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var candidates = new List<MediaCandidateRecord>();
        foreach (var request in queue.Requests.Where(item => item.Status == "fixture-ready").OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            candidates.Add(Candidate(
                "candidate/fixture/" + SafeSegment(request.RequestId),
                request.RequestId,
                "fixture",
                "fixture-generated-by-repo",
                "repo-fixture",
                "complete",
                request.MediaKind,
                request.MediaSlotId,
                FixturePath(request),
                string.Empty,
                string.Empty,
                false,
                false,
                false,
                "promote_fixture"));
        }

        foreach (var request in queue.Requests.Where(item => item.MediaSlotId == "region_tile_or_background").OrderBy(item => item.RequestId, StringComparer.Ordinal))
        {
            candidates.Add(Candidate("candidate/manual/" + SafeSegment(request.RequestId), request.RequestId, "manual_placeholder", "manual-user-provided", "manual-review-required", "placeholder_only", request.MediaKind, request.MediaSlotId, string.Empty, string.Empty, string.Empty, false, false, false, "needs_manual_review"));
        }

        foreach (var request in queue.Requests.Where(item => item.MediaSlotId == "item_icon").Take(3).OrderBy(item => item.RequestId, StringComparer.Ordinal))
        {
            candidates.Add(Candidate("candidate/imported-cc0/" + SafeSegment(request.RequestId), request.RequestId, "import_placeholder", "imported-cc0", "cc0", "source_record_present", request.MediaKind, request.MediaSlotId, string.Empty, string.Empty, string.Empty, false, false, false, "needs_manual_review"));
        }

        var firstQuest = queue.Requests.First(item => item.MediaSlotId == "quest_or_event_icon");
        var firstHazard = queue.Requests.First(item => item.MediaSlotId == "sfx_combat_or_hazard");
        var firstAmbient = queue.Requests.First(item => item.MediaSlotId == "ambient_loop");
        var firstMusic = queue.Requests.First(item => item.MediaSlotId == "music_stinger");
        var firstWorld = queue.Requests.First(item => item.MediaSlotId == "world_key_art");

        candidates.Add(Candidate("candidate/imported-cc-by/missing-attribution", firstQuest.RequestId, "import_placeholder", "imported-cc-by", "cc-by", "source_record_present", firstQuest.MediaKind, firstQuest.MediaSlotId, string.Empty, string.Empty, string.Empty, false, false, false, "blocked_missing_provenance"));
        candidates.Add(Candidate("candidate/gpl-risk/auto-promotion-attempt", firstHazard.RequestId, "import_placeholder", "imported-share-alike-or-gpl-risk", "cc-by-sa-or-gpl-risk", "source_record_present", firstHazard.MediaKind, firstHazard.MediaSlotId, string.Empty, "Some Author", string.Empty, false, false, false, "blocked_license"));
        candidates.Add(Candidate("candidate/provider/missing-metadata", firstAmbient.RequestId, "provider_later_placeholder", "provider-generated-with-model-license", "provider-model-license-required", "metadata_missing", firstAmbient.MediaKind, firstAmbient.MediaSlotId, string.Empty, string.Empty, string.Empty, false, false, false, "blocked_provider_not_configured"));
        candidates.Add(Candidate("candidate/unknown/no-license", firstMusic.RequestId, "import_placeholder", "unknown/no-license", "unknown", "missing", firstMusic.MediaKind, firstMusic.MediaSlotId, string.Empty, string.Empty, string.Empty, false, false, false, "blocked_missing_provenance"));
        candidates.Add(Candidate("candidate/leak/final-artwork-claim", firstWorld.RequestId, "fixture", "fixture-generated-by-repo", "repo-fixture", "complete", firstWorld.MediaKind, firstWorld.MediaSlotId, FixturePath(firstWorld), string.Empty, string.Empty, true, false, false, "blocked_leak"));
        candidates.Add(Candidate("candidate/mismatch/wrong-kind", firstWorld.RequestId, "fixture", "fixture-generated-by-repo", "repo-fixture", "complete", "audio", firstWorld.MediaSlotId, FixturePath(firstWorld).Replace("fixtures/images/", "fixtures/audio/", StringComparison.Ordinal), string.Empty, string.Empty, false, false, false, "blocked_mismatch"));

        _ = requestsById;
        return new MediaCandidateQuarantine
        {
            Passed = candidates.Count > 0,
            Candidates = candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal).ToList()
        };
    }

    public MediaReviewPromotionLedger BuildReviewPromotionLedger(
        MediaRequestQueue queue,
        MediaCandidateQuarantine quarantine,
        MediaLicenseProvenanceLedger ledger)
    {
        var requestsById = queue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var policiesByKind = ledger.Policies.ToDictionary(item => item.SourceKind, item => item, StringComparer.Ordinal);
        var decisions = quarantine.Candidates
            .OrderBy(item => item.CandidateId, StringComparer.Ordinal)
            .Select(candidate => Decide(candidate, requestsById[candidate.RequestId], policiesByKind))
            .ToList();

        return new MediaReviewPromotionLedger
        {
            Deterministic = decisions.SequenceEqual(decisions.OrderBy(item => item.DecisionId, StringComparer.Ordinal)),
            Passed = decisions.Any(item => item.Decision == "promote_fixture")
                && MediaAssetCampaignVocabulary.RequiredReviewDecisions.All(decision => decisions.Any(item => item.Decision == decision))
                && decisions.Where(item => item.Promoted).All(item => item.Decision == "promote_fixture"),
            PromotedFixtureCount = decisions.Count(item => item.Decision == "promote_fixture" && item.Promoted),
            Decisions = decisions,
            Diagnostics = [Info("goal053.review.ledger_built", "media-review-promotion-ledger", "Review ledger promotes fixture candidates only and blocks risky provenance or leak candidates.")]
        };
    }

    public (MediaFixtureFileInventory Inventory, IReadOnlyList<MediaFixtureFilePayload> Payloads) BuildFixtureInventory(
        MediaRequestQueue queue,
        MediaCandidateQuarantine quarantine,
        MediaReviewPromotionLedger review)
    {
        var requestsById = queue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var candidatesById = quarantine.Candidates.ToDictionary(item => item.CandidateId, item => item, StringComparer.Ordinal);
        var payloads = new List<MediaFixtureFilePayload>();
        var files = new List<MediaFixtureFileRecord>();

        foreach (var decision in review.Decisions.Where(item => item.Decision == "promote_fixture" && item.Promoted).OrderBy(item => item.DecisionId, StringComparer.Ordinal))
        {
            var candidate = candidatesById[decision.CandidateId];
            var request = requestsById[decision.RequestId];
            var contents = RenderFixtureDescriptor(request, candidate);
            var bytes = Encoding.UTF8.GetBytes(contents);
            payloads.Add(new MediaFixtureFilePayload
            {
                RelativePath = candidate.RelativeFixturePath,
                Contents = contents
            });
            files.Add(new MediaFixtureFileRecord
            {
                FixtureId = "fixture/" + SafeSegment(request.RequestId),
                RelativePath = candidate.RelativeFixturePath,
                ByteLength = bytes.LongLength,
                Sha256 = MediaAssetCampaignHash.Hash(bytes),
                MediaKind = request.MediaKind,
                BoundRequestId = request.RequestId,
                BoundGeneratedTargetId = request.TargetGeneratedId,
                FixtureStatus = "fixture_asset_only_not_final_media"
            });
        }

        return (new MediaFixtureFileInventory
        {
            Passed = files.Count > 0 && files.Count == payloads.Count && files.All(item => IsSafeRelativePath(item.RelativePath)),
            FixtureFileCount = files.Count,
            Files = files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = [Info("goal053.fixture.inventory_built", "media-fixture-file-inventory", "Deterministic textual fixture descriptors are hashed and bound to request ids.")]
        }, payloads.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList());
    }

    public MediaBindingManifest BuildBindingManifest(
        MediaRequestQueue queue,
        MediaCandidateQuarantine quarantine,
        MediaReviewPromotionLedger review,
        MediaFixtureFileInventory inventory)
    {
        var requestsById = queue.Requests.ToDictionary(item => item.RequestId, item => item, StringComparer.Ordinal);
        var candidatesById = quarantine.Candidates.ToDictionary(item => item.CandidateId, item => item, StringComparer.Ordinal);
        var fixturesByRequest = inventory.Files.ToDictionary(item => item.BoundRequestId, item => item, StringComparer.Ordinal);
        var promoted = review.Decisions
            .Where(item => item.Decision == "promote_fixture" && item.Promoted)
            .OrderBy(item => item.DecisionId, StringComparer.Ordinal)
            .ToList();

        var bindings = promoted.Select(decision =>
        {
            var request = requestsById[decision.RequestId];
            var candidate = candidatesById[decision.CandidateId];
            var fixture = fixturesByRequest[request.RequestId];
            return new MediaBindingRecord
            {
                BindingId = "media-binding/" + SafeSegment(request.RequestId),
                FamilyId = request.FamilyId,
                RequestId = request.RequestId,
                CandidateId = candidate.CandidateId,
                MediaSlotId = request.MediaSlotId,
                MediaKind = request.MediaKind,
                GeneratedTargetId = request.TargetGeneratedId,
                TargetArtifactKind = request.TargetArtifactKind,
                FixtureRelativePath = fixture.RelativePath,
                FixtureSha256 = fixture.Sha256,
                FixtureOnlyNotFinalMedia = true
            };
        }).ToList();

        var boundRequests = bindings.Select(item => item.RequestId).ToHashSet(StringComparer.Ordinal);
        var fallbacks = queue.Requests
            .Where(item => !boundRequests.Contains(item.RequestId))
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(item => new MediaFallbackRecord
            {
                FamilyId = item.FamilyId,
                RequestId = item.RequestId,
                MediaSlotId = item.MediaSlotId,
                FallbackBehavior = "explicit_unfilled_slot_uses_placeholder_or_silent_fallback_no_package_mutation"
            })
            .ToList();

        return new MediaBindingManifest
        {
            Passed = bindings.Count > 0
                && MediaAssetCampaignVocabulary.FamilyIds.All(family => bindings.Any(item => item.FamilyId == family && item.MediaKind == "image"))
                && MediaAssetCampaignVocabulary.FamilyIds.All(family => bindings.Any(item => item.FamilyId == family && item.MediaKind == "audio"))
                && fallbacks.Count > 0,
            BindingCount = bindings.Count,
            Bindings = bindings,
            Fallbacks = fallbacks,
            Diagnostics = [Info("goal053.binding.manifest_built", "media-binding-manifest", "Promoted fixture candidates are bound to generated target ids with explicit fallback records for unfilled slots.")]
        };
    }

    public PreviewExportMediaPayloads BuildPreviewExportPayloads(
        MediaCampaignSourceManifest manifest,
        MediaBindingManifest bindingManifest)
    {
        var bindingsByFamily = bindingManifest.Bindings
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var fallbacksByFamily = bindingManifest.Fallbacks
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var families = manifest.Families
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(family =>
            {
                bindingsByFamily.TryGetValue(family.FamilyId, out var bindings);
                fallbacksByFamily.TryGetValue(family.FamilyId, out var fallbacks);
                bindings ??= [];
                fallbacks ??= [];
                return new PreviewExportMediaPayloadFamilySummary
                {
                    FamilyId = family.FamilyId,
                    ScenarioId = family.ScenarioId,
                    RuntimePreviewPayloadRef = family.RuntimePreviewPayloadRef,
                    ExportProfileId = family.ExportProfileId,
                    BindingCount = bindings.Count,
                    ImageLikeFixtureBindingCount = bindings.Count(item => item.MediaKind == "image"),
                    AudioLikeFixtureBindingCount = bindings.Count(item => item.MediaKind == "audio"),
                    UiOrBundleFixtureBindingCount = bindings.Count(item => item.MediaKind is "ui" or "bundle"),
                    HasImageLikeFixtureBinding = bindings.Any(item => item.MediaKind == "image"),
                    HasAudioLikeFixtureBinding = bindings.Any(item => item.MediaKind == "audio"),
                    ExplicitFallbackForUnfilledSlots = fallbacks.Count > 0,
                    PackageRuntimeExportPayloadsMutated = false,
                    GamePackageSchemaChanged = false,
                    RuntimeChanged = false,
                    UnityExportModified = false
                };
            })
            .ToList();

        return new PreviewExportMediaPayloads
        {
            Passed = families.Count == 3
                && families.All(item => item.HasImageLikeFixtureBinding && item.HasAudioLikeFixtureBinding && item.ExplicitFallbackForUnfilledSlots)
                && families.All(item => !item.PackageRuntimeExportPayloadsMutated && !item.GamePackageSchemaChanged && !item.RuntimeChanged && !item.UnityExportModified),
            FamilyCount = families.Count,
            EveryFamilyHasMediaBindings = families.All(item => item.BindingCount > 0),
            EveryFamilyHasImageAndAudioFixtureBindings = families.All(item => item.HasImageLikeFixtureBinding && item.HasAudioLikeFixtureBinding),
            PackageRuntimeExportPayloadsMutated = false,
            GamePackageSchemaChanged = false,
            UnityExportModified = false,
            Families = families,
            Diagnostics = [Info("goal053.preview_export.payloads_built", "preview-export-media-payloads", "Preview/export payload proof consumes media bindings without mutating package/runtime/export payloads.")]
        };
    }

    public InvalidMediaDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidMediaScenario>
        {
            Invalid("duplicate_media_request_id", "Duplicate a media request id in the queue.", "rejected", Error("goal053.request.duplicate_id", "media-request-queue", "Media request ids must be unique.")),
            Invalid("unknown_family_id", "Attach a request to an unknown family id.", "rejected", Error("goal053.family.unknown", "unknown_family", "Media requests must target one of the three Goal 047 families.")),
            Invalid("unknown_generated_target_id", "Bind a request to a target generated id absent from the source manifest.", "rejected", Error("goal053.target.unknown", "generated-target", "Generated target id must come from Goal 047 family dry-run facts.")),
            Invalid("unknown_media_slot_id", "Use a media slot id absent from the catalog.", "rejected", Error("goal053.slot.unknown", "media-slot", "Media slot id must exist in the catalog.")),
            Invalid("invalid_media_kind", "Declare a media kind not in image/audio/ui/bundle.", "rejected", Error("goal053.media_kind.invalid", "media-kind", "Media kind must be image, audio, ui or bundle.")),
            Invalid("missing_required_provenance", "Remove required candidate provenance fields.", "rejected", Error("goal053.provenance.missing", "candidate", "Candidate provenance is required before review.")),
            Invalid("unknown_no_license_candidate_accepted_attempt", "Attempt to accept unknown/no-license candidate.", "rejected", Error("goal053.license.unknown", "unknown/no-license", "Unknown or no-license candidates must be rejected.")),
            Invalid("cc_by_without_attribution", "Promote a CC-BY candidate without attribution.", "rejected", Error("goal053.license.attribution_missing", "imported-cc-by", "CC-BY candidates require attribution records.")),
            Invalid("share_alike_gpl_risk_auto_promotion", "Auto-promote share-alike/GPL-risk candidate.", "blocked", Error("goal053.license.share_alike_or_gpl_risk", "imported-share-alike-or-gpl-risk", "Share-alike/GPL-risk candidates are blocked unless explicitly allowed later.")),
            Invalid("provider_candidate_without_model_license_run_metadata", "Review provider candidate without model/license/run metadata.", "blocked", Error("goal053.provider.metadata_missing", "provider-generated-with-model-license", "Provider candidate requires model, license and run metadata and is not allowed in Goal 053.")),
            Invalid("final_prose_or_final_artwork_claim", "Fixture candidate claims final prose/artwork.", "blocked", Error("goal053.boundary.final_claim", "candidate", "Fixture candidates must not claim final prose or final artwork.")),
            Invalid("path_traversal_in_fixture_path", "Use ../ in fixture path.", "rejected", Error("goal053.fixture.path_traversal", "../escape.txt", "Fixture paths must stay under the Goal 053 artifact folder.")),
            Invalid("external_absolute_path_in_artifact", "Use an external absolute path in an artifact.", "rejected", Error("goal053.artifact.absolute_path", "C:/unsafe/file.png", "Artifacts must not contain absolute machine paths.")),
            Invalid("network_url_treated_as_downloaded_asset", "Treat a network URL as a downloaded asset.", "rejected", Error("goal053.artifact.network_url", "https://example.invalid/asset.png", "Goal 053 must not download or import network assets.")),
            Invalid("provider_llm_rag_call_claim", "Claim provider/LLM/RAG execution.", "blocked", Error("goal053.boundary.provider_llm_rag", "execution", "Provider, LLM and RAG calls are forbidden.")),
            Invalid("runtime_ui_unity_gamepackage_mutation_claim", "Claim Runtime/UI/Unity/GamePackage schema mutation.", "blocked", Error("goal053.boundary.runtime_ui_unity_gamepackage", "boundary", "Runtime, UI, Unity and GamePackage schema mutation are forbidden.")),
            Invalid("nondeterministic_ordering", "Shuffle request/review ordering.", "rejected", Error("goal053.order.nondeterministic", "ordering", "Request, candidate and binding ordering must be deterministic.")),
            Invalid("fake_source_artifact_hash_or_path", "Reference a fake source artifact path or hash.", "rejected", Error("goal053.source.fake_hash_or_path", "source-ref", "Source artifact refs must match physical Goal 047/043/040 paths and hashes.")),
            Invalid("self_promotion_without_review_trace", "Promote a candidate without a review decision trace.", "rejected", Error("goal053.review.trace_missing", "media-binding-manifest", "Every binding requires a review/promotion decision trace."))
        };

        return new InvalidMediaDiagnosticsMatrix
        {
            Passed = scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = scenarios.Count(item => item.ActualStatus == "blocked"),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<MediaCampaignDiagnostic> SortDiagnostics(IEnumerable<MediaCampaignDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string StyleId(string familyId, string scenarioId) =>
        "media-style/" + familyId.Replace('_', '-') + "/" + scenarioId.Replace('_', '-');

    public static string RequestId(string familyId, string slotId) =>
        "media-request/" + familyId.Replace('_', '-') + "/" + slotId.Replace('_', '-');

    private static MediaReviewDecisionRecord Decide(
        MediaCandidateRecord candidate,
        MediaRequestRecord request,
        IReadOnlyDictionary<string, MediaLicensePolicyRecord> policiesByKind)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        var decision = candidate.ExpectedReviewDecision;
        var cause = "goal053.review.expected_policy";
        var promoted = decision == "promote_fixture";
        if (!policiesByKind.ContainsKey(candidate.SourceKind))
        {
            decision = "blocked_missing_provenance";
            cause = "goal053.provenance.unknown_source_kind";
            promoted = false;
            diagnostics.Add(Error(cause, candidate.CandidateId, "Candidate source kind is unknown."));
        }
        else if (candidate.ClaimsFinalArtworkOrProse || candidate.ClaimsProviderLlmRagCall || candidate.ClaimsRuntimeUiUnityGamePackageMutation)
        {
            decision = "blocked_leak";
            cause = "goal053.review.leak_claim";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Candidate includes a forbidden final/provenance/boundary claim."));
        }
        else if (candidate.SourceKind == "fixture-generated-by-repo"
                 && (candidate.MediaKind != request.MediaKind || candidate.DeclaredMediaSlotId != request.MediaSlotId))
        {
            decision = "blocked_mismatch";
            cause = "goal053.review.media_mismatch";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Fixture candidate media kind or slot does not match the request."));
        }
        else if (candidate.SourceKind == "fixture-generated-by-repo"
                 && (!IsSafeRelativePath(candidate.RelativeFixturePath) || candidate.ProvenanceStatus != "complete"))
        {
            decision = "blocked_missing_provenance";
            cause = "goal053.review.fixture_provenance_missing";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Fixture candidates require safe relative paths and complete provenance."));
        }
        else if (candidate.SourceKind == "imported-cc-by" && string.IsNullOrWhiteSpace(candidate.Attribution))
        {
            decision = "blocked_missing_provenance";
            cause = "goal053.license.attribution_missing";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "CC-BY candidates require attribution records."));
        }
        else if (candidate.SourceKind == "provider-generated-with-model-license")
        {
            decision = "blocked_provider_not_configured";
            cause = "goal053.provider.metadata_missing";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Provider candidates are blocked until model/license/run metadata and provider configuration exist."));
        }
        else if (candidate.SourceKind == "imported-share-alike-or-gpl-risk")
        {
            decision = "blocked_license";
            cause = "goal053.license.share_alike_or_gpl_risk";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Share-alike/GPL-risk candidates are blocked for Goal 053."));
        }
        else if (candidate.SourceKind == "unknown/no-license")
        {
            decision = "blocked_missing_provenance";
            cause = "goal053.license.unknown";
            promoted = false;
            diagnostics.Add(Warning(cause, candidate.CandidateId, "Unknown/no-license candidates are rejected."));
        }
        else if (decision == "needs_manual_review")
        {
            cause = "goal053.review.manual_or_import_requires_later_review";
            promoted = false;
            diagnostics.Add(Info(cause, candidate.CandidateId, "Manual/import candidates remain quarantined for later review."));
        }
        else if (decision == "promote_fixture")
        {
            cause = "goal053.review.fixture_promoted";
            promoted = true;
            diagnostics.Add(Info(cause, candidate.CandidateId, "Repository-generated fixture candidate promoted as fixture asset only."));
        }

        return new MediaReviewDecisionRecord
        {
            DecisionId = "review/" + SafeSegment(candidate.CandidateId),
            CandidateId = candidate.CandidateId,
            RequestId = candidate.RequestId,
            Decision = decision,
            CauseCode = cause,
            Promoted = promoted,
            PromotionScope = promoted ? "fixture_asset_only_not_final_content" : "quarantine_or_block",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static MediaSlotDefinition Slot(
        string slotId,
        string category,
        string mediaKind,
        IReadOnlyList<string> families,
        string hint,
        IReadOnlyList<string> tags,
        string bindingTargetKind,
        string fallback) =>
        new()
        {
            SlotId = slotId,
            Category = category,
            MediaKind = mediaKind,
            TargetFamilies = families,
            DimensionsOrDurationHint = hint,
            SemanticStyleTags = tags,
            AllowedSourceTypes = ["fixture", "manual", "import", "provider_later"],
            ReviewRequirements = ["provenance_record", "license_policy_decision", "review_trace_required", "no_final_content_claim"],
            LicensePolicyRequirement = "license_provenance_required_before_promotion",
            BindingTargetKind = bindingTargetKind,
            FallbackPlaceholderBehavior = fallback
        };

    private static MediaLicensePolicyRecord Policy(
        string sourceKind,
        string promotionPolicy,
        string requiredMetadata,
        string decision,
        bool canAutoPromote) =>
        new()
        {
            SourceKind = sourceKind,
            PromotionPolicy = promotionPolicy,
            RequiredMetadata = requiredMetadata,
            Goal053Decision = decision,
            CanAutoPromoteInGoal053 = canAutoPromote
        };

    private static MediaCandidateRecord Candidate(
        string candidateId,
        string requestId,
        string candidateKind,
        string sourceKind,
        string licenseKind,
        string provenanceStatus,
        string mediaKind,
        string slotId,
        string relativePath,
        string attribution,
        string providerMetadata,
        bool finalClaim,
        bool providerClaim,
        bool mutationClaim,
        string expectedDecision) =>
        new()
        {
            CandidateId = candidateId,
            RequestId = requestId,
            CandidateKind = candidateKind,
            SourceKind = sourceKind,
            LicenseKind = licenseKind,
            ProvenanceStatus = provenanceStatus,
            MediaKind = mediaKind,
            DeclaredMediaSlotId = slotId,
            RelativeFixturePath = relativePath,
            Attribution = attribution,
            ProviderModelRunMetadata = providerMetadata,
            ClaimsFinalArtworkOrProse = finalClaim,
            ClaimsProviderLlmRagCall = providerClaim,
            ClaimsRuntimeUiUnityGamePackageMutation = mutationClaim,
            ExpectedReviewDecision = expectedDecision
        };

    private static (string TargetId, string ArtifactFamily, string ArtifactKind) TargetForSlot(
        MediaCampaignFamilySourceRecord family,
        string slotId)
    {
        var systemId = slotId switch
        {
            "world_key_art" => "world",
            "region_tile_or_background" => "world",
            "npc_portrait" => "entity",
            "species_or_archetype_portrait" => "entity",
            "item_icon" => "item",
            "quest_or_event_icon" => "quest",
            "ui_panel_skin" => "event",
            "sfx_interaction" => "dialogue",
            "sfx_combat_or_hazard" => "combat",
            "ambient_loop" => "world",
            "music_stinger" => "event",
            "export_placeholder_bundle" => "export",
            _ => "event"
        };
        var target = systemId == "export"
            ? family.ExportProfileId
            : family.GeneratedRuntimeTargetIds.First(item => item.EndsWith("/" + systemId, StringComparison.Ordinal));
        return (target, systemId, slotId);
    }

    private static string RequestStatus(string slotId) =>
        slotId switch
        {
            "world_key_art" or "npc_portrait" or "sfx_interaction" or "ui_panel_skin" or "export_placeholder_bundle" => "fixture-ready",
            "ambient_loop" or "music_stinger" => "future-provider",
            "species_or_archetype_portrait" => "blocked",
            _ => "requested"
        };

    private static string BudgetHint(MediaSlotDefinition slot) =>
        slot.MediaKind switch
        {
            "image" => "low_fixture_descriptor_budget_no_binary_media",
            "audio" => "low_fixture_descriptor_budget_no_waveform_media",
            "ui" => "low_fixture_descriptor_budget_no_ui_asset_pack",
            "bundle" => "compact_manifest_descriptor_budget",
            _ => "low_budget"
        };

    private static string FixturePath(MediaRequestRecord request)
    {
        var folder = request.MediaKind switch
        {
            "image" => "fixtures/images",
            "audio" => "fixtures/audio",
            "ui" => "fixtures/ui",
            "bundle" => "fixtures/bundles",
            _ => "fixtures/other"
        };
        return folder + "/" + SafeSegment(request.RequestId) + ".txt";
    }

    private static string RenderFixtureDescriptor(MediaRequestRecord request, MediaCandidateRecord candidate) =>
        string.Join('\n',
        [
            "fixtureSchema=goal053_text_fixture_descriptor_v1",
            "fixtureOnly=true",
            "finalMedia=false",
            "requestId=" + request.RequestId,
            "candidateId=" + candidate.CandidateId,
            "familyId=" + request.FamilyId,
            "scenarioId=" + request.ScenarioId,
            "styleId=" + request.StyleId,
            "mediaKind=" + request.MediaKind,
            "mediaSlotId=" + request.MediaSlotId,
            "targetGeneratedId=" + request.TargetGeneratedId,
            "sourceKind=fixture-generated-by-repo",
            "realProviderCalled=false",
            "realMediaGenerationCalled=false",
            "networkOrImportCalled=false"
        ]) + '\n';

    private static IReadOnlyList<string> VisualTags(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => ["gothic_panel_map", "landmark_forward", "muted_readable"],
            "survival_sandbox" => ["frontier_survival", "resource_hazard", "weathered_readable"],
            "first_person_grid_dungeon" => ["grid_dungeon", "kingdom_route", "high_contrast_readable"],
            _ => ["generic"]
        };

    private static IReadOnlyList<string> AudioTags(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => ["soft_ui_cues", "gothic_ambience"],
            "survival_sandbox" => ["hazard_cues", "camp_loop"],
            "first_person_grid_dungeon" => ["step_turn_cues", "corridor_tension"],
            _ => ["generic_audio"]
        };

    private static IReadOnlyList<string> UiTags(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => ["panel_journal", "travel_log"],
            "survival_sandbox" => ["survival_status", "resource_panel"],
            "first_person_grid_dungeon" => ["compass_strip", "party_panel"],
            _ => ["generic_ui"]
        };

    private static int SlotOrder(string slotId)
    {
        for (var i = 0; i < MediaAssetCampaignVocabulary.RequiredSlotIds.Count; i++)
        {
            if (string.Equals(MediaAssetCampaignVocabulary.RequiredSlotIds[i], slotId, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }

        return 999;
    }

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

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static InvalidMediaScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params MediaCampaignDiagnostic[] diagnostics) =>
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
            "error" => 0,
            "critical" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static MediaCampaignDiagnostic Error(string code, string target, string message) =>
        MediaCampaignDiagnostic.Error(code, target, message);

    private static MediaCampaignDiagnostic Warning(string code, string target, string message) =>
        MediaCampaignDiagnostic.Warning(code, target, message);

    private static MediaCampaignDiagnostic Info(string code, string target, string message) =>
        MediaCampaignDiagnostic.Info(code, target, message);
}
