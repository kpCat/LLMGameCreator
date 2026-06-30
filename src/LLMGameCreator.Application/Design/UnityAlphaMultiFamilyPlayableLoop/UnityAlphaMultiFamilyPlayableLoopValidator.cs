namespace LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyPlayableLoopValidator
{
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateSourceManifest(UnityAlphaMultiFamilySourceManifest manifest)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal057.gate.self_pass.forbidden", "source-manifest", "Goal 057 must not mark its own gate passed."));
        }

        if (!manifest.Goal056AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "unity_alpha_media_bound_playable_package_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal057.preflight.goal056_handoff_missing", "source-manifest", "Goal 056 acceptance by user handoff is required before Goal 057."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal057.gate.required_missing", "source-manifest", "Goal 057 gate must remain required."));
        }

        foreach (var sourceGoal in new[] { "Goal043", "Goal047", "Goal055", "Goal056" })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == sourceGoal && item.Exists && item.HashMatches))
            {
                diagnostics.Add(Error("goal057.source.required_ref_missing", sourceGoal, "Goal 057 source refs must include matching Goal 043, Goal 047, Goal 055 and Goal 056 artifacts."));
            }
        }

        if (!manifest.Goal056ReportWasGreenProducedForReview || !manifest.Goal056UnityProofPassed)
        {
            diagnostics.Add(Error("goal057.source.goal056_not_green", "Goal056", "Goal 056 must be GREEN produced-for-review source evidence before Goal 057 handoff acceptance."));
        }

        if (manifest.FamilyCount != 3 || UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds.Any(familyId => !manifest.SelectedFamilyIds.Contains(familyId, StringComparer.Ordinal)))
        {
            diagnostics.Add(Error("goal057.source.family_coverage_missing", "source-manifest", "Goal 057 must select all three required family ids."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateFamilyModeManifest(UnityAlphaFamilyModeManifest manifest)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        if (!manifest.Passed || manifest.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal057.family_mode.manifest_invalid", "family-mode-manifest", "Family mode manifest must cover all three families."));
        }

        if (manifest.Families.Select(item => item.ModeId).Distinct(StringComparer.Ordinal).Count() != manifest.Families.Count)
        {
            diagnostics.Add(Error("goal057.family.duplicate_mode_id", "family-mode-manifest", "Family mode ids must be unique."));
        }

        foreach (var familyId in UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds)
        {
            if (!manifest.Families.Any(item => item.FamilyId == familyId && item.ExpectedMarkers.Contains("family_loop_completed=" + familyId, StringComparer.Ordinal)))
            {
                diagnostics.Add(Error("goal057.family.mode_missing", familyId, "Each required family needs a completed-loop marker."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateStagingAndPlan(UnityAlphaMultiFamilyStagingManifest staging, UnityAlphaFamilyCommandPlan plan)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        if (!staging.Passed || string.IsNullOrWhiteSpace(staging.DeterministicHash))
        {
            diagnostics.Add(Error("goal057.staging.manifest_invalid", "unity-staging-manifest", "Unity staging manifest must pass and carry a deterministic hash."));
        }

        if (!plan.Passed || plan.Accepted)
        {
            diagnostics.Add(Error("goal057.command_plan.invalid", "family-command-plan", "Family command plan must pass and remain accepted=false."));
        }

        if (plan.FamilyModes.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() != 3)
        {
            diagnostics.Add(Error("goal057.command_plan.family_coverage_missing", "family-command-plan", "Family command plan must cover all three families."));
        }

        if (plan.Commands.Select(item => item.FamilyId + "|" + item.Order).Distinct(StringComparer.Ordinal).Count() != plan.Commands.Count)
        {
            diagnostics.Add(Error("goal057.command_plan.duplicate_order", "family-command-plan", "Command order must be unique per family."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateUnityProof(
        UnityAlphaFamilyCommandPlan commandPlan,
        UnityAlphaMultiFamilyUnityProof proof,
        IReadOnlyDictionary<string, UnityAlphaFamilyLoopProof> familyProofs)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerLogSummary.PlayerExecuted && !proof.PlayerLogSummary.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal057.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 057 marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorOrPlayerExecuted || proof.PlayerLogSummary.UnityExitCode != 0 || proof.PlayerLogSummary.PlayerExitCode != 0)
            {
                diagnostics.Add(Error("goal057.unity.proof_inconsistent", "player-log-summary", "Passed Unity proof must have Unity/player execution and zero exit codes."));
            }

            foreach (var familyId in UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds)
            {
                if (!familyProofs.TryGetValue(familyId, out var familyProof) || !familyProof.Passed || familyProof.LoopStepCount < 3)
                {
                    diagnostics.Add(Error("goal057.unity.family_loop_missing", familyId, "Passed Unity proof must prove at least three family loop steps for every family."));
                }
            }
        }
        else if (string.IsNullOrWhiteSpace(proof.BlockerCode))
        {
            diagnostics.Add(Error("goal057.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry an exact blocker code."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics).Concat(proof.PlayerLogSummary.Diagnostics));
    }

    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateMediaAndReview(
        UnityAlphaMultiFamilyMediaBindingValidation media,
        UnityAlphaMultiFamilyPreviewExportPayload previewExport,
        UnityAlphaMultiFamilyReviewPackageManifest review)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        if (!media.Passed || media.MediaBindingCount != 15)
        {
            diagnostics.Add(Error("goal057.media.validation_failed", "media-binding-validation", "Goal 057 must retain all Goal 056 media bindings with matching hashes."));
        }

        if (!previewExport.Passed || previewExport.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal057.preview_export.invalid", "preview-export-payload", "Preview/export payload must cover all three families."));
        }

        if (!review.Passed || review.Accepted)
        {
            diagnostics.Add(Error("goal057.review.invalid", "review-package-manifest", "Review package manifest must pass and remain accepted=false."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> ValidateInvalidMatrix(InvalidUnityAlphaMultiFamilyMatrix matrix)
    {
        var diagnostics = new List<UnityAlphaMultiFamilyDiagnostic>();
        foreach (var scenarioId in UnityAlphaMultiFamilyPlayableLoopVocabulary.RequiredInvalidScenarioIds)
        {
            if (!matrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal057.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        return Sort(diagnostics);
    }

    public static IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Sort(IEnumerable<UnityAlphaMultiFamilyDiagnostic> diagnostics) =>
        UnityAlphaMultiFamilyPlayableLoopBuilder.SortDiagnostics(diagnostics);

    private static UnityAlphaMultiFamilyDiagnostic Error(string code, string target, string message) =>
        UnityAlphaMultiFamilyDiagnostic.Error(code, target, message);
}
