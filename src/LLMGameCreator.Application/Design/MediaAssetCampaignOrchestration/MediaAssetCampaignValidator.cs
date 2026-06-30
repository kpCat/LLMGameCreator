namespace LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignValidator
{
    private static readonly HashSet<string> ValidMediaKinds = new(StringComparer.Ordinal) { "image", "audio", "ui", "bundle" };

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateSourceManifest(MediaCampaignSourceManifest manifest)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal053.gate.self_pass.forbidden", "media-campaign-source-manifest", "Goal 053 must not mark its own gate passed."));
        }

        if (!manifest.PreflightGates.Any(item =>
                item.GateId == "full_generator_without_media_verification"
                && item.Status == "passed"
                && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal053.preflight.goal047_handoff_missing", "media-campaign-source-manifest", "Goal 047 must be accepted by user handoff before Goal 053."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == "semantic_pack_composition_blueprint_verification" && item.Status == "produced_for_review_not_passed")
            || !manifest.PreflightGates.Any(item => item.GateId == "dynamic_semantic_feature_system_verification" && item.Status == "produced_for_review_not_passed"))
        {
            diagnostics.Add(Error("goal053.preflight.goal031_032_policy", "media-campaign-source-manifest", "Goal 031 and Goal 032 must remain produced-for-review/not passed."));
        }

        if (manifest.SelectedFamilyIds.Count != 3
            || !MediaAssetCampaignVocabulary.FamilyIds.All(item => manifest.SelectedFamilyIds.Contains(item, StringComparer.Ordinal)))
        {
            diagnostics.Add(Error("goal053.family.required_missing", "media-campaign-source-manifest", "Source manifest must cover the three Goal 047 families."));
        }

        foreach (var sourceGoal in new[] { "Goal040", "Goal043", "Goal047" })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == sourceGoal))
            {
                diagnostics.Add(Error("goal053.source." + sourceGoal.ToLowerInvariant() + "_missing", "media-campaign-source-manifest", sourceGoal + " source artifact refs are required."));
            }
        }

        if (manifest.SourceArtifactRefs.Any(item => string.IsNullOrWhiteSpace(item.ArtifactHash) || !IsSafeRelativePath(item.ArtifactRelativePath)))
        {
            diagnostics.Add(Error("goal053.source.ref_invalid", "media-campaign-source-manifest", "Every source artifact ref must carry a hash and a safe relative path."));
        }

        if (manifest.MetamoduleStressSummary.OneRequestPerSpeciesArchetypeSlotGenerated)
        {
            diagnostics.Add(Error("goal053.metamodule.expanded_files_forbidden", "media-campaign-source-manifest", "Metamodule stress proof must stay compact and must not generate one media file per species/archetype slot."));
        }

        diagnostics.AddRange(ValidateBoundary(manifest.BoundaryClaims, "media-campaign-source-manifest"));
        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateSlotCatalog(MediaSlotCatalog catalog)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        foreach (var required in MediaAssetCampaignVocabulary.RequiredSlotIds)
        {
            if (!catalog.Slots.Any(item => item.SlotId == required))
            {
                diagnostics.Add(Error("goal053.catalog.slot_missing", required, "Required media slot is missing."));
            }
        }

        if (catalog.Slots.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count() != catalog.Slots.Count)
        {
            diagnostics.Add(Error("goal053.catalog.duplicate_slot", "media-slot-catalog", "Media slot ids must be unique."));
        }

        foreach (var slot in catalog.Slots)
        {
            if (!ValidMediaKinds.Contains(slot.MediaKind))
            {
                diagnostics.Add(Error("goal053.media_kind.invalid", slot.SlotId, "Slot media kind must be image, audio, ui or bundle."));
            }

            if (slot.AllowedSourceTypes.Count == 0 || slot.ReviewRequirements.Count == 0 || string.IsNullOrWhiteSpace(slot.LicensePolicyRequirement))
            {
                diagnostics.Add(Error("goal053.catalog.policy_missing", slot.SlotId, "Slot definitions require source, review and license policy requirements."));
            }
        }

        return Sort(diagnostics.Concat(catalog.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateRequestQueue(
        MediaRequestQueue queue,
        MediaCampaignSourceManifest manifest,
        MediaSlotCatalog catalog)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        var familyIds = manifest.Families.Select(item => item.FamilyId).ToHashSet(StringComparer.Ordinal);
        var slotIds = catalog.Slots.Select(item => item.SlotId).ToHashSet(StringComparer.Ordinal);
        var targetIds = manifest.Families.SelectMany(item => item.GeneratedRuntimeTargetIds.Append(item.ExportProfileId)).ToHashSet(StringComparer.Ordinal);

        if (queue.Requests.Select(item => item.RequestId).Distinct(StringComparer.Ordinal).Count() != queue.Requests.Count)
        {
            diagnostics.Add(Error("goal053.request.duplicate_id", "media-request-queue", "Media request ids must be unique."));
        }

        if (queue.Requests.Count < 30)
        {
            diagnostics.Add(Error("goal053.request.count_low", "media-request-queue", "Goal 053 requires at least thirty media requests."));
        }

        foreach (var familyId in MediaAssetCampaignVocabulary.FamilyIds)
        {
            if (queue.Requests.Count(item => item.FamilyId == familyId) < 8)
            {
                diagnostics.Add(Error("goal053.request.family_count_low", familyId, "Each family requires at least eight media requests."));
            }
        }

        foreach (var request in queue.Requests)
        {
            if (!familyIds.Contains(request.FamilyId))
            {
                diagnostics.Add(Error("goal053.family.unknown", request.FamilyId, "Media request family id must come from the source manifest."));
            }

            if (!slotIds.Contains(request.MediaSlotId))
            {
                diagnostics.Add(Error("goal053.slot.unknown", request.MediaSlotId, "Media request slot id must exist in the slot catalog."));
            }

            if (!ValidMediaKinds.Contains(request.MediaKind))
            {
                diagnostics.Add(Error("goal053.media_kind.invalid", request.RequestId, "Request media kind must be image, audio, ui or bundle."));
            }

            if (!targetIds.Contains(request.TargetGeneratedId))
            {
                diagnostics.Add(Error("goal053.target.unknown", request.TargetGeneratedId, "Request target id must come from Goal 047 family dry-run facts."));
            }

            if (request.PromptInputSkeleton.FinalProviderPromptText)
            {
                diagnostics.Add(Error("goal053.prompt.final_text", request.RequestId, "Request queue must carry prompt/input skeleton fields, not final provider prompt text."));
            }
        }

        if (!queue.Requests.Any(item => item.MediaKind == "image")
            || !queue.Requests.Any(item => item.MediaKind == "audio")
            || !queue.Requests.Any(item => item.MediaKind is "ui" or "bundle"))
        {
            diagnostics.Add(Error("goal053.request.media_kind_coverage", "media-request-queue", "Request queue must include image, audio and UI/bundle categories."));
        }

        if (queue.MetamoduleStressSummary.OneRequestPerSpeciesArchetypeSlotGenerated)
        {
            diagnostics.Add(Error("goal053.metamodule.expanded_files_forbidden", "media-request-queue", "Metamodule stress must be compacted instead of generating one request/file per 112 slots."));
        }

        return Sort(diagnostics.Concat(queue.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateLicenseLedger(MediaLicenseProvenanceLedger ledger)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        foreach (var sourceKind in MediaAssetCampaignVocabulary.LicenseSourceKinds)
        {
            if (!ledger.Policies.Any(item => item.SourceKind == sourceKind))
            {
                diagnostics.Add(Error("goal053.license.policy_missing", sourceKind, "Required license/provenance policy is missing."));
            }
        }

        if (ledger.Policies.Any(item => item.SourceKind != "fixture-generated-by-repo" && item.CanAutoPromoteInGoal053))
        {
            diagnostics.Add(Error("goal053.license.auto_promotion_forbidden", "media-license-provenance-ledger", "Only repo-generated fixture assets can be auto-promoted in Goal 053."));
        }

        return Sort(diagnostics.Concat(ledger.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateReviewPromotionLedger(
        MediaReviewPromotionLedger ledger,
        MediaCandidateQuarantine quarantine)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        foreach (var decision in MediaAssetCampaignVocabulary.RequiredReviewDecisions)
        {
            if (!ledger.Decisions.Any(item => item.Decision == decision))
            {
                diagnostics.Add(Error("goal053.review.decision_missing", decision, "Required review/promotion decision is missing."));
            }
        }

        var candidatesById = quarantine.Candidates.ToDictionary(item => item.CandidateId, item => item, StringComparer.Ordinal);
        foreach (var decision in ledger.Decisions)
        {
            if (!candidatesById.TryGetValue(decision.CandidateId, out var candidate))
            {
                diagnostics.Add(Error("goal053.review.trace_missing", decision.CandidateId, "Review decision must reference a quarantined candidate."));
                continue;
            }

            if (decision.Promoted
                && (decision.Decision != "promote_fixture"
                    || candidate.SourceKind != "fixture-generated-by-repo"
                    || candidate.ClaimsFinalArtworkOrProse))
            {
                diagnostics.Add(Error("goal053.review.invalid_promotion", decision.CandidateId, "Only repo-generated fixtures can be promoted as fixture assets."));
            }
        }

        return Sort(diagnostics.Concat(ledger.Diagnostics).Concat(ledger.Decisions.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateFixtureInventory(MediaFixtureFileInventory inventory)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        if (inventory.Files.Count == 0)
        {
            diagnostics.Add(Error("goal053.fixture.none", "media-fixture-file-inventory", "Goal 053 requires deterministic fixture media files."));
        }

        if (inventory.Files.Select(item => item.RelativePath).Distinct(StringComparer.Ordinal).Count() != inventory.Files.Count)
        {
            diagnostics.Add(Error("goal053.fixture.duplicate_path", "media-fixture-file-inventory", "Fixture paths must be unique."));
        }

        foreach (var file in inventory.Files)
        {
            if (!IsSafeRelativePath(file.RelativePath))
            {
                diagnostics.Add(Error("goal053.fixture.path_traversal", file.RelativePath, "Fixture paths must be safe relative paths."));
            }

            if (file.ByteLength <= 0 || string.IsNullOrWhiteSpace(file.Sha256))
            {
                diagnostics.Add(Error("goal053.fixture.hash_missing", file.RelativePath, "Fixture inventory requires byte length and hash."));
            }
        }

        return Sort(diagnostics.Concat(inventory.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateBindingManifest(MediaBindingManifest manifest)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        if (manifest.Bindings.Count == 0)
        {
            diagnostics.Add(Error("goal053.binding.none", "media-binding-manifest", "At least one promoted fixture binding is required."));
        }

        foreach (var familyId in MediaAssetCampaignVocabulary.FamilyIds)
        {
            if (!manifest.Bindings.Any(item => item.FamilyId == familyId && item.MediaKind == "image")
                || !manifest.Bindings.Any(item => item.FamilyId == familyId && item.MediaKind == "audio"))
            {
                diagnostics.Add(Error("goal053.binding.family_image_audio_missing", familyId, "Every family requires at least one image-like and one audio-like fixture binding."));
            }
        }

        if (manifest.Bindings.Any(item => !item.FixtureOnlyNotFinalMedia))
        {
            diagnostics.Add(Error("goal053.binding.final_media_claim", "media-binding-manifest", "Bindings must identify fixture media as fixture-only, not final media."));
        }

        if (manifest.Fallbacks.Count == 0)
        {
            diagnostics.Add(Error("goal053.binding.fallback_missing", "media-binding-manifest", "Unfilled media slots require explicit fallback records."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidatePreviewExportPayloads(PreviewExportMediaPayloads payloads)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        if (!payloads.EveryFamilyHasMediaBindings || !payloads.EveryFamilyHasImageAndAudioFixtureBindings)
        {
            diagnostics.Add(Error("goal053.preview_export.family_media_missing", "preview-export-media-payloads", "Each family must have media bindings and image/audio fixture coverage."));
        }

        if (payloads.PackageRuntimeExportPayloadsMutated || payloads.GamePackageSchemaChanged || payloads.UnityExportModified)
        {
            diagnostics.Add(Error("goal053.boundary.runtime_ui_unity_gamepackage", "preview-export-media-payloads", "Preview/export media proof must not mutate GamePackage, Runtime or Unity/export payloads."));
        }

        if (payloads.Families.Any(item => !item.ExplicitFallbackForUnfilledSlots))
        {
            diagnostics.Add(Error("goal053.preview_export.fallback_missing", "preview-export-media-payloads", "Each family must explicitly record fallback behavior for unfilled slots."));
        }

        return Sort(diagnostics.Concat(payloads.Diagnostics));
    }

    public IReadOnlyList<MediaCampaignDiagnostic> ValidateInvalidMatrix(InvalidMediaDiagnosticsMatrix matrix)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        var requiredScenarioIds = new[]
        {
            "duplicate_media_request_id",
            "unknown_family_id",
            "unknown_generated_target_id",
            "unknown_media_slot_id",
            "invalid_media_kind",
            "missing_required_provenance",
            "unknown_no_license_candidate_accepted_attempt",
            "cc_by_without_attribution",
            "share_alike_gpl_risk_auto_promotion",
            "provider_candidate_without_model_license_run_metadata",
            "final_prose_or_final_artwork_claim",
            "path_traversal_in_fixture_path",
            "external_absolute_path_in_artifact",
            "network_url_treated_as_downloaded_asset",
            "provider_llm_rag_call_claim",
            "runtime_ui_unity_gamepackage_mutation_claim",
            "nondeterministic_ordering",
            "fake_source_artifact_hash_or_path",
            "self_promotion_without_review_trace"
        };

        foreach (var scenarioId in requiredScenarioIds)
        {
            if (!matrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.Diagnostics.Count > 0 && item.ExpectedStatus == item.ActualStatus))
            {
                diagnostics.Add(Error("goal053.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or does not match expectation."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<MediaCampaignDiagnostic> ValidateBoundary(
        MediaCampaignBoundaryClaims claims,
        string target)
    {
        var diagnostics = new List<MediaCampaignDiagnostic>();
        if (claims.FinalProseOrArtworkClaim)
        {
            diagnostics.Add(Error("goal053.boundary.final_claim", target, "Final prose/artwork claims are forbidden."));
        }

        if (claims.ProviderLlmRagCalled || claims.RealMediaGenerationCalled)
        {
            diagnostics.Add(Error("goal053.boundary.provider_llm_rag", target, "Provider, LLM, RAG and real media generation calls are forbidden."));
        }

        if (claims.NetworkOrImportCalled)
        {
            diagnostics.Add(Error("goal053.artifact.network_url", target, "Network/import execution is forbidden."));
        }

        if (claims.GamePackageSchemaChanged || claims.RuntimeSourceChanged || claims.RuntimeAbstractionsChanged || claims.WinFormsUiChanged || claims.UnitySourceOrExportChanged)
        {
            diagnostics.Add(Error("goal053.boundary.runtime_ui_unity_gamepackage", target, "Runtime/UI/Unity/GamePackage changes are forbidden."));
        }

        if (claims.ProviderPathChanged || claims.LuaOrGeneratorLibraryChanged || claims.ExternalDependencyAdded)
        {
            diagnostics.Add(Error("goal053.boundary.external_or_provider_path", target, "Provider paths, Lua/generator-library changes and external dependencies are forbidden."));
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<MediaCampaignDiagnostic> Sort(IEnumerable<MediaCampaignDiagnostic> diagnostics) =>
        MediaAssetCampaignBuilder.SortDiagnostics(diagnostics);

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static MediaCampaignDiagnostic Error(string code, string target, string message) =>
        MediaCampaignDiagnostic.Error(code, target, message);
}
