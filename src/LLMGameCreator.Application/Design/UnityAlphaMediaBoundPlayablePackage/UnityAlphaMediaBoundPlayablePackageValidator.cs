namespace LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundPlayablePackageValidator
{
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> ValidateSourceManifest(UnityAlphaMediaBoundSourceManifest manifest)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal056.gate.self_pass.forbidden", "source-manifest", "Goal 056 must not mark its own gate passed."));
        }

        if (!manifest.Goal055AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "media_bound_playable_review_package_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal056.preflight.goal055_handoff_missing", "source-manifest", "Goal 055 acceptance by user handoff is required before Goal 056."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == UnityAlphaMediaBoundPlayablePackageVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal056.gate.required_missing", "source-manifest", "Goal 056 gate must remain required."));
        }

        if (!manifest.Goal055ReportWasGreenProducedForReview)
        {
            diagnostics.Add(Error("goal056.source.goal055_report_inconsistent", "Goal055", "Goal 055 report must be GREEN produced-for-review source evidence before user handoff acceptance."));
        }

        if (manifest.Goal055PhysicalMediaFileCount != 15 || manifest.Goal055PngFileCount < 3 || manifest.Goal055WavFileCount < 3 || manifest.Goal055BundleFileCount < 3)
        {
            diagnostics.Add(Error("goal056.source.goal055_media_counts_invalid", "Goal055", "Goal 056 requires the complete Goal 055 staged physical media set."));
        }

        if (!manifest.BaseAlphaPayloadFound)
        {
            diagnostics.Add(Error("goal056.base_payload.missing", "Alpha base payload", "Goal 056 requires an existing Alpha base payload for Unity launch proof."));
        }

        foreach (var sourceGoal in new[] { "Goal047", "Goal054", "Goal055" })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == sourceGoal && item.Exists && item.HashMatches))
            {
                diagnostics.Add(Error("goal056.source.required_ref_missing", sourceGoal, "Goal 056 source refs must include matching Goal 047, Goal 054 and Goal 055 artifacts."));
            }
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> ValidateStagingManifest(UnityAlphaMediaBoundStagingManifest manifest)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        if (!manifest.Passed || string.IsNullOrWhiteSpace(manifest.DeterministicHash))
        {
            diagnostics.Add(Error("goal056.staging.manifest_invalid", "unity-streamingassets-staging-manifest", "Unity StreamingAssets staging manifest must pass and carry a deterministic hash."));
        }

        if (manifest.PhysicalMediaFileCount != 15 || manifest.PngFileCount < 3 || manifest.WavFileCount < 3 || manifest.BundleFileCount < 3 || manifest.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal056.staging.media_coverage_missing", "staging", "Staging must include PNG, WAV and bundle media for all three families."));
        }

        if (manifest.Bindings.Select(item => item.BindingId).Distinct(StringComparer.Ordinal).Count() != manifest.Bindings.Count)
        {
            diagnostics.Add(Error("goal056.binding.duplicate_id", "unity-streamingassets-staging-manifest", "Media binding ids must be unique."));
        }

        foreach (var binding in manifest.Bindings)
        {
            if (!binding.SafeRelativePath)
            {
                diagnostics.Add(Error("goal056.path.unsafe", binding.RelativePath, "Media path must be safe relative."));
            }

            if (!binding.HashMatchesGoal055)
            {
                diagnostics.Add(Error("goal056.stage.hash_mismatch", binding.RelativePath, "Staged media hash must match Goal 055 source bytes."));
            }

            if (binding.RelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && (!binding.PngValid || binding.Width <= 0 || binding.Height <= 0))
            {
                diagnostics.Add(Error("goal056.media.png_malformed", binding.RelativePath, "PNG staged media requires valid signature, dimensions and CRC."));
            }

            if (binding.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && (!binding.WavValid || binding.SampleRate <= 0 || binding.SampleCount <= 0))
            {
                diagnostics.Add(Error("goal056.media.wav_malformed", binding.RelativePath, "WAV staged media requires valid PCM header and samples."));
            }

            if (string.IsNullOrWhiteSpace(binding.ReviewTrace))
            {
                diagnostics.Add(Error("goal056.review.trace_missing", binding.RelativePath, "Every staged file requires a Goal 055/054 review trace."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> ValidatePanelModels(UnityAlphaMediaBoundFamilyPanelModels models)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        if (!models.Passed || models.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal056.panel.models_invalid", "media-bound-family-panel-models", "Panel proof models must cover all three families."));
        }

        foreach (var familyId in UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds)
        {
            if (!models.Families.Any(item => item.FamilyId == familyId && item.PanelProofMarker == "media_bound_family_panel_proof=" + familyId))
            {
                diagnostics.Add(Error("goal056.panel.family_missing", familyId, "Each family must have a deterministic panel proof marker."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> ValidateUnityContractAndProof(
        UnityAlphaMediaBoundLoadContract contract,
        UnityAlphaMediaBoundLoadProof proof)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        if (!contract.Passed || contract.ExpectedBindings.Count != 15)
        {
            diagnostics.Add(Error("goal056.unity.contract_invalid", "unity-media-load-contract", "Unity media load contract must cover all staged bindings."));
        }

        foreach (var marker in contract.RequiredLogMarkers)
        {
            if (proof.SmokeLogSummary.PlayerExecuted && !proof.SmokeLogSummary.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal056.unity.marker_missing", marker, "Executed Unity proof must include every required media-bound marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorOrPlayerExecuted || !proof.ManifestLoadedByUnityProof || !proof.PngLoadProofPassed || !proof.WavLoadProofPassed || !proof.BundleProofPassed || !proof.HashValidationPassed || !proof.FamilyMediaPanelProofPassed)
            {
                diagnostics.Add(Error("goal056.unity.proof_inconsistent", "unity-media-load-proof", "Passed Unity proof must prove manifest, PNG, WAV, bundle, hashes and family panel markers."));
            }
        }
        else if (string.IsNullOrWhiteSpace(proof.BlockerCode))
        {
            diagnostics.Add(Error("goal056.unity.blocker_missing", "unity-media-load-proof", "Non-passing Unity proof must carry an exact blocker code."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics).Concat(proof.SmokeLogSummary.Diagnostics));
    }

    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> ValidateInvalidMatrix(InvalidUnityAlphaMediaBoundMatrix matrix)
    {
        var diagnostics = new List<UnityAlphaMediaBoundDiagnostic>();
        foreach (var scenarioId in UnityAlphaMediaBoundPlayablePackageVocabulary.RequiredInvalidScenarioIds)
        {
            if (!matrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal056.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Sort(IEnumerable<UnityAlphaMediaBoundDiagnostic> diagnostics) =>
        UnityAlphaMediaBoundPlayablePackageBuilder.SortDiagnostics(diagnostics);

    private static UnityAlphaMediaBoundDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMediaBoundDiagnostic.Error(code, target, message);
}
