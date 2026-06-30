namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationReviewPackageValidator
{
    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateSourceManifest(MediaMaterializationSourceManifest manifest)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal054.gate.self_pass.forbidden", "source-manifest", "Goal 054 must not mark its own gate passed."));
        }

        if (!manifest.Goal053AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "media_asset_campaign_orchestration_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal054.preflight.goal053_handoff_missing", "source-manifest", "Goal 053 must be accepted by user handoff before Goal 054."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == MediaMaterializationReviewPackageVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal054.gate.required_missing", "source-manifest", "Goal 054 gate must remain required."));
        }

        if (!manifest.Goal053ProducedForReviewReportGreen || !manifest.Goal053ReportKeptRequired)
        {
            diagnostics.Add(Error("goal054.source.goal053_report_inconsistent", "Goal053", "Goal 053 report must be GREEN produced-for-review evidence with its own gate required."));
        }

        if (manifest.Goal053RequestCount <= 0 || manifest.Goal053BindingCount <= 0)
        {
            diagnostics.Add(Error("goal054.source.goal053_counts_missing", "Goal053", "Goal 053 media request and binding counts are required."));
        }

        foreach (var familyId in MediaMaterializationReviewPackageVocabulary.FamilyIds)
        {
            if (!manifest.Families.Any(item => item.FamilyId == familyId))
            {
                diagnostics.Add(Error("goal054.family.required_missing", familyId, "Goal 054 must cover every current generated family."));
            }
        }

        if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal053")
            || !manifest.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal047"))
        {
            diagnostics.Add(Error("goal054.source.required_refs_missing", "source-manifest", "Goal 053 and Goal 047 source artifact refs are required."));
        }

        if (!manifest.BoundaryClaims.AllFalse)
        {
            diagnostics.Add(Error("goal054.boundary.provider_network_llm_rag", "source-manifest", "Goal 054 source manifest boundary claims must all remain false."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateQueue(MediaMaterializationQueue queue)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (queue.QueueItemCount == 0 || queue.Items.Count != queue.QueueItemCount)
        {
            diagnostics.Add(Error("goal054.queue.count_invalid", "media-materialization-queue", "Queue item count must match materialization items."));
        }

        if (queue.Items.Select(item => item.MaterializationId).Distinct(StringComparer.Ordinal).Count() != queue.Items.Count)
        {
            diagnostics.Add(Error("goal054.queue.duplicate_materialization_id", "media-materialization-queue", "Materialization ids must be unique."));
        }

        if (!queue.Items.SequenceEqual(queue.Items.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)))
        {
            diagnostics.Add(Error("goal054.order.nondeterministic", "media-materialization-queue", "Materialization queue must be deterministic."));
        }

        foreach (var item in queue.Items)
        {
            if (!IsSafeRelativePath(item.OutputRelativePath))
            {
                diagnostics.Add(Error("goal054.path.absolute", item.OutputRelativePath, "Materialized output path must be a safe relative path."));
            }

            if (string.IsNullOrWhiteSpace(item.ExpectedSha256) || item.ExpectedByteLength <= 0)
            {
                diagnostics.Add(Error("goal054.media.hash_mismatch", item.MaterializationId, "Queue item must include expected hash and byte length."));
            }
        }

        return Sort(diagnostics.Concat(queue.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateInventory(
        MaterializedMediaInventory inventory,
        MediaMaterializationQueue queue)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        var queueById = queue.Items.ToDictionary(item => item.MaterializationId, item => item, StringComparer.Ordinal);
        if (inventory.FileCount != queue.QueueItemCount || inventory.Files.Count != inventory.FileCount)
        {
            diagnostics.Add(Error("goal054.media.file_missing", "materialized-media-inventory", "Every queue item must have one materialized file."));
        }

        foreach (var file in inventory.Files)
        {
            if (!queueById.TryGetValue(file.MaterializationId, out var queueItem))
            {
                diagnostics.Add(Error("goal054.binding.fake_id", file.MaterializationId, "Inventory materialization id must resolve to queue."));
                continue;
            }

            if (file.Sha256 != queueItem.ExpectedSha256 || file.ByteLength != queueItem.ExpectedByteLength)
            {
                diagnostics.Add(Error("goal054.media.hash_mismatch", file.RelativePath, "Inventory hash and length must match queue expectation."));
            }

            if (file.MaterializedMediaFormat == "png" && (!file.PngSignatureValid || !file.PngChunkCrcsValid))
            {
                diagnostics.Add(Error("goal054.media.png_malformed", file.RelativePath, "PNG fixtures require valid signature and chunk CRC proof."));
            }

            if (file.MaterializedMediaFormat == "wav_pcm_s16_mono" && !file.WavHeaderValid)
            {
                diagnostics.Add(Error("goal054.media.wav_malformed", file.RelativePath, "WAV fixtures require valid RIFF/WAVE PCM header proof."));
            }

            if (!IsSafeRelativePath(file.RelativePath))
            {
                diagnostics.Add(Error("goal054.path.absolute", file.RelativePath, "Inventory paths must be safe relative paths."));
            }
        }

        return Sort(diagnostics.Concat(inventory.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateLedger(MediaProvenanceLicenseLedger ledger)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (!ledger.LicenseDecisions.Any(item => item.SourceKind == "unknown/no-license" && !item.PromotedInGoal054)
            || !ledger.LicenseDecisions.Any(item => item.SourceKind == "imported-share-alike-or-gpl-risk" && !item.PromotedInGoal054))
        {
            diagnostics.Add(Error("goal054.license.unknown_or_prohibited", "media-provenance-license-ledger", "Unknown/prohibited license paths must block promotion."));
        }

        if (ledger.LicenseDecisions.Any(item => item.SourceKind != "fixture-generated-by-repo" && item.PromotedInGoal054))
        {
            diagnostics.Add(Error("goal054.provenance.import_or_provider_promoted", "media-provenance-license-ledger", "Only repository-generated fixtures may be promoted in Goal 054."));
        }

        if (ledger.MaterializedFiles.Any(item => item.ProviderImportedOrManual || item.SourceKind != "fixture-generated-by-repo"))
        {
            diagnostics.Add(Error("goal054.provenance.import_or_provider_promoted", "media-provenance-license-ledger", "Materialized media provenance must not be provider/import/manual."));
        }

        if (ledger.MaterializedFiles.Any(item => string.IsNullOrWhiteSpace(item.Sha256)))
        {
            diagnostics.Add(Error("goal054.provenance.missing", "media-provenance-license-ledger", "Every materialized file requires hash provenance."));
        }

        return Sort(diagnostics.Concat(ledger.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateBindingValidation(MediaBindingValidation validation)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (!validation.Passed || !validation.EveryFamilyHasImageAndAudioFixture)
        {
            diagnostics.Add(Error("goal054.binding.validation_failed", "media-binding-validation", "Binding validation must pass with image/audio fixture coverage for every family."));
        }

        diagnostics.AddRange(validation.Bindings.SelectMany(item => item.Diagnostics));
        return Sort(diagnostics.Concat(validation.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidatePayloads(PreviewExportMediaPayloads payloads)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (!payloads.Passed || payloads.FamilyCount != 3 || !payloads.AllMediaRefsResolveToInventory)
        {
            diagnostics.Add(Error("goal054.preview_export.refs_invalid", "preview-export-media-payloads", "Preview/export media payload refs must resolve for all three families."));
        }

        if (payloads.GamePackageSchemaChanged)
        {
            diagnostics.Add(Error("goal054.boundary.gamepackage_schema", "preview-export-media-payloads", "GamePackage schema mutation is forbidden."));
        }

        if (payloads.RuntimeUiUnityChanged)
        {
            diagnostics.Add(Error("goal054.boundary.runtime_ui_unity", "preview-export-media-payloads", "Runtime/UI/Unity mutation is forbidden."));
        }

        return Sort(diagnostics.Concat(payloads.Diagnostics));
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateReviewPackage(MediaReviewPackageManifest manifest)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        if (!manifest.Passed || manifest.MediaFileList.Count == 0 || manifest.PayloadList.Count == 0 || string.IsNullOrWhiteSpace(manifest.DeterministicHash))
        {
            diagnostics.Add(Error("goal054.review_package.manifest_invalid", "media-review-package-manifest", "Review package manifest must include files, payloads and deterministic hash."));
        }

        foreach (var familyId in MediaMaterializationReviewPackageVocabulary.FamilyIds)
        {
            if (!manifest.FamilyCoverageSummary.ContainsKey(familyId) || manifest.FamilyCoverageSummary[familyId] == 0)
            {
                diagnostics.Add(Error("goal054.review_package.family_missing", familyId, "Review package must include media for every family."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<MediaMaterializationDiagnostic> ValidateInvalidMatrix(InvalidMediaMaterializationMatrix matrix)
    {
        var diagnostics = new List<MediaMaterializationDiagnostic>();
        foreach (var scenarioId in MediaMaterializationReviewPackageVocabulary.RequiredInvalidScenarioIds)
        {
            if (!matrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal054.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or does not match expectation."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<MediaMaterializationDiagnostic> Sort(IEnumerable<MediaMaterializationDiagnostic> diagnostics) =>
        MediaMaterializationReviewPackageBuilder.SortDiagnostics(diagnostics);

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static MediaMaterializationDiagnostic Error(string code, string target, string message) =>
        MediaMaterializationDiagnostic.Error(code, target, message);
}
