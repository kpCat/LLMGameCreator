namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundPlayableReviewPackageValidator
{
    public IReadOnlyList<MediaBoundDiagnostic> ValidateSourceManifest(MediaBoundSourceManifest manifest)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal055.gate.self_pass.forbidden", "source-manifest", "Goal 055 must not mark its own gate passed."));
        }

        if (!manifest.Goal054AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "media_materialization_review_package_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal055.preflight.goal054_handoff_missing", "source-manifest", "Goal 054 acceptance by user handoff is required before Goal 055."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == MediaBoundPlayableReviewPackageVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal055.gate.required_missing", "source-manifest", "Goal 055 gate must remain required."));
        }

        if (!manifest.Goal054ReportWasGreenProducedForReview)
        {
            diagnostics.Add(Error("goal055.source.goal054_report_inconsistent", "Goal054", "Goal 054 report must be GREEN produced-for-review source evidence before user handoff acceptance."));
        }

        if (manifest.Goal047FamilyDryRunCount != 3 || manifest.Goal053BindingCount <= 0 || manifest.Goal054PhysicalMediaCount <= 0)
        {
            diagnostics.Add(Error("goal055.source.counts_invalid", "source-manifest", "Goal 047, Goal 053 and Goal 054 source counts are required."));
        }

        foreach (var familyId in MediaBoundPlayableReviewPackageVocabulary.FamilyIds)
        {
            if (!manifest.Families.Any(item => item.FamilyId == familyId && item.Goal054PngCount > 0 && item.Goal054WavCount > 0 && item.Goal054BundleJsonCount > 0))
            {
                diagnostics.Add(Error("goal055.family.required_media_missing", familyId, "Every family must have Goal 054 PNG, WAV and bundle source media."));
            }
        }

        if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal047")
            || !manifest.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal053")
            || !manifest.SourceArtifactRefs.Any(item => item.SourceGoal == "Goal054"))
        {
            diagnostics.Add(Error("goal055.source.required_refs_missing", "source-manifest", "Goal 047, Goal 053 and Goal 054 source artifact refs are required."));
        }

        if (!manifest.BoundaryClaims.AllFalse)
        {
            diagnostics.Add(Error("goal055.boundary.provider_network_llm_rag", "source-manifest", "Boundary claims must stay false."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidateReviewPackage(MediaBoundReviewPackageManifest manifest)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (!manifest.Passed || manifest.StagedFileCount == 0 || manifest.Bindings.Count != manifest.StagedFileCount || string.IsNullOrWhiteSpace(manifest.DeterministicHash))
        {
            diagnostics.Add(Error("goal055.review_package.manifest_invalid", "media-bound-review-package-manifest", "Review package manifest must pass with staged files, bindings and deterministic hash."));
        }

        if (manifest.PngFileCount < 3 || manifest.WavFileCount < 3 || manifest.BundleJsonFileCount < 3)
        {
            diagnostics.Add(Error("goal055.review_package.media_kind_coverage_missing", "media-bound-review-package-manifest", "Review package must stage PNG, WAV and bundle JSON for every family."));
        }

        foreach (var staged in manifest.StagedFiles)
        {
            if (!staged.SafeRelativePath)
            {
                diagnostics.Add(Error("goal055.path.unsafe", staged.StagedRelativePath, "Staged media path must be safe relative."));
            }

            if (!staged.SourceHashMatches || staged.StagedSha256 != staged.SourceSha256)
            {
                diagnostics.Add(Error("goal055.stage.hash_mismatch", staged.StagedRelativePath, "Staged bytes must match Goal 054 source hash."));
            }

            if (staged.StagedRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && (!staged.PngValid || staged.PngWidth <= 0 || staged.PngHeight <= 0))
            {
                diagnostics.Add(Error("goal055.media.png_malformed", staged.StagedRelativePath, "PNG staged media requires valid signature, dimensions and CRC."));
            }

            if (staged.StagedRelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && (!staged.WavValid || staged.WavSampleRate <= 0 || staged.WavSampleCount <= 0))
            {
                diagnostics.Add(Error("goal055.media.wav_malformed", staged.StagedRelativePath, "WAV staged media requires valid PCM header and samples."));
            }

            if (string.IsNullOrWhiteSpace(staged.ReviewTrace))
            {
                diagnostics.Add(Error("goal055.review.trace_missing", staged.StagedRelativePath, "Every staged file requires a Goal 054 review trace."));
            }
        }

        if (manifest.Bindings.Select(item => item.BindingId).Distinct(StringComparer.Ordinal).Count() != manifest.Bindings.Count)
        {
            diagnostics.Add(Error("goal055.binding.duplicate_id", "media-bound-review-package-manifest", "Binding ids must be unique."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidateStreamingManifest(StreamingAssetsMediaManifest manifest)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (!manifest.Passed || manifest.FamilyCount != 3 || manifest.BindingCount == 0 || string.IsNullOrWhiteSpace(manifest.DeterministicHash))
        {
            diagnostics.Add(Error("goal055.streaming_assets.manifest_invalid", "streaming-assets-media-manifest", "StreamingAssets media manifest must pass with all three families and deterministic hash."));
        }

        if (manifest.Bindings.Select(item => item.BindingId).Distinct(StringComparer.Ordinal).Count() != manifest.Bindings.Count)
        {
            diagnostics.Add(Error("goal055.binding.duplicate_id", "streaming-assets-media-manifest", "StreamingAssets binding ids must be unique."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidatePreviewPayloads(MediaBoundPreviewPayloads payloads)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (!payloads.Passed || payloads.FamilyCount != 3 || !payloads.FamilyDryRunToMediaManifestProof)
        {
            diagnostics.Add(Error("goal055.preview_payloads.invalid", "media-bound-preview-payloads", "Preview/export payloads must connect dry-run, staged media, manifest and proof records for all families."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidateUnityContractAndProofs(
        UnityMediaLoadContract contract,
        IReadOnlyList<UnityMediaLoadProof> proofs)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (!contract.Passed || contract.Files.Count == 0)
        {
            diagnostics.Add(Error("goal055.unity.contract_invalid", "unity-media-load-contract", "Unity-compatible media load contract must pass over staged manifest files."));
        }

        if (contract.UnitySourceChanged || contract.UnityBuildOrPlayerExecuted)
        {
            diagnostics.Add(Error("goal055.boundary.unity_broad_mutation", "unity-media-load-contract", "Goal 055 Application proof must not claim Unity source/build/player execution unless a narrow route actually ran."));
        }

        foreach (var familyId in MediaBoundPlayableReviewPackageVocabulary.FamilyIds)
        {
            var proof = proofs.FirstOrDefault(item => item.FamilyId == familyId);
            if (proof == null || !proof.Passed || !proof.ManifestLoaded || !proof.ImageLoaded || !proof.WavValidated || !proof.FamilyPanelReady)
            {
                diagnostics.Add(Error("goal055.unity.proof_missing", familyId, "Each family must have manifest/image/WAV/panel-ready proof records."));
                continue;
            }

            if (!proof.ProofLines.Any(item => item.StartsWith("MEDIA_BOUND_MANIFEST_LOADED family=" + familyId, StringComparison.Ordinal))
                || !proof.ProofLines.Any(item => item.StartsWith("MEDIA_BOUND_IMAGE_LOADED family=" + familyId, StringComparison.Ordinal))
                || !proof.ProofLines.Any(item => item.StartsWith("MEDIA_BOUND_WAV_VALIDATED family=" + familyId, StringComparison.Ordinal))
                || !proof.ProofLines.Any(item => item.StartsWith("MEDIA_BOUND_FAMILY_PANEL_READY family=" + familyId, StringComparison.Ordinal)))
            {
                diagnostics.Add(Error("goal055.unity.fake_proof_line", familyId, "Unity proof lines must include the required deterministic media-bound markers."));
            }
        }

        return Sort(diagnostics.Concat(contract.Diagnostics).Concat(proofs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidateFamilySmokeMatrix(MediaBoundFamilySmokeMatrix matrix)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        if (!matrix.Passed || matrix.FamilyCount != 3 || matrix.Families.Any(item => !item.Passed))
        {
            diagnostics.Add(Error("goal055.family_smoke.failed", "media-bound-family-smoke-matrix", "Family smoke matrix must pass for all three families."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<MediaBoundDiagnostic> ValidateInvalidMatrix(InvalidMediaBoundPackageDiagnosticsMatrix matrix)
    {
        var diagnostics = new List<MediaBoundDiagnostic>();
        foreach (var scenarioId in MediaBoundPlayableReviewPackageVocabulary.RequiredInvalidScenarioIds)
        {
            if (!matrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal055.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<MediaBoundDiagnostic> Sort(IEnumerable<MediaBoundDiagnostic> diagnostics) =>
        MediaBoundPlayableReviewPackageBuilder.SortDiagnostics(diagnostics);

    private static MediaBoundDiagnostic Error(string code, string target, string message) =>
        MediaBoundDiagnostic.Error(code, target, message);
}
