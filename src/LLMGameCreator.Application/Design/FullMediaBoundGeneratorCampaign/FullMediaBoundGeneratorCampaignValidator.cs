namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignValidator
{
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> ValidateSourceManifest(FullMediaBoundCampaignSourceManifest manifest)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal058.gate.self_pass.forbidden", "campaign-source-manifest", "Goal 058 must not mark its own gate passed."));
        }

        if (!manifest.Goal057AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "unity_alpha_multifamily_playable_loop_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal058.preflight.goal057_handoff_missing", "campaign-source-manifest", "Goal 057 acceptance by user handoff is required before Goal 058."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == FullMediaBoundGeneratorCampaignVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal058.gate.required_missing", "campaign-source-manifest", "Goal 058 gate must remain required."));
        }

        foreach (var sourceGoal in new[] { "Goal043", "Goal047", "Goal053", "Goal054", "Goal055", "Goal056", "Goal057" })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == sourceGoal && item.Exists && item.HashMatches))
            {
                diagnostics.Add(Error("goal058.source.required_ref_missing", sourceGoal, "Goal 058 source refs must include matching Goal 043, 047, 053, 054, 055, 056 and 057 artifacts."));
            }
        }

        if (!manifest.Goal057ReportWasGreenProducedForReview || !manifest.Goal057UnityProofPassed)
        {
            diagnostics.Add(Error("goal058.source.goal057_not_green", "Goal057", "Goal 057 must be GREEN produced-for-review source evidence before Goal 058 handoff acceptance."));
        }

        if (manifest.FamilyCount != 3 || FullMediaBoundGeneratorCampaignVocabulary.FamilyIds.Any(familyId => !manifest.SelectedFamilyIds.Contains(familyId, StringComparer.Ordinal)))
        {
            diagnostics.Add(Error("goal058.source.family_coverage_missing", "campaign-source-manifest", "Goal 058 must select all three required family ids."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> ValidateCampaignPlan(FullMediaBoundCampaignPlan plan)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        if (!plan.Passed || plan.Accepted)
        {
            diagnostics.Add(Error("goal058.campaign_plan.invalid", "campaign-plan", "Campaign plan must pass and remain accepted=false."));
        }

        foreach (var stageId in FullMediaBoundGeneratorCampaignVocabulary.StageIds)
        {
            if (!plan.Stages.Any(item => item.StageId == stageId && item.Passed))
            {
                diagnostics.Add(Error("goal058.stage.missing_or_failed", stageId, "Every required campaign stage must pass."));
            }
        }

        if (plan.SeedProfileFamilySet.Count != 3)
        {
            diagnostics.Add(Error("goal058.campaign_plan.family_set_missing", "seed/profile/family", "Campaign plan must cover three seed/profile/family records."));
        }

        return Sort(diagnostics.Concat(plan.Stages.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> ValidateFamilyRuns(IReadOnlyDictionary<string, FullMediaBoundCampaignFamilyRun> familyRuns)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        foreach (var familyId in FullMediaBoundGeneratorCampaignVocabulary.FamilyIds)
        {
            if (!familyRuns.TryGetValue(familyId, out var run) || !run.Passed || run.CommandCount < 5 || run.MediaFileCount < 5)
            {
                diagnostics.Add(Error("goal058.family_run.invalid", familyId, "Each family run must include commands, media and preview/export refs."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> ValidateReviewAndPayloads(
        FullMediaBoundReviewPackageManifest review,
        FullMediaBoundUnityCampaignCommandPlan commandPlan,
        PreviewExportCampaignPayload previewExport,
        CampaignPackageCompatibilityProof packageProof,
        InvalidFullMediaBoundCampaignMatrix invalidMatrix)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        if (!review.Passed || review.Accepted || review.StreamingAssetsFiles.Count < 3)
        {
            diagnostics.Add(Error("goal058.review.invalid", "unified-review-package-manifest", "Review package manifest must pass, remain accepted=false and stage required StreamingAssets payloads."));
        }

        if (!commandPlan.Passed || commandPlan.Accepted)
        {
            diagnostics.Add(Error("goal058.command_plan.invalid", "unity-alpha-campaign-command-plan", "Unity command plan must pass and remain accepted=false."));
        }

        foreach (var marker in FullMediaBoundGeneratorCampaignBuilder.ExpectedCampaignMarkers(FullMediaBoundGeneratorCampaignVocabulary.FamilyIds))
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal058.command_plan.marker_missing", marker, "Unity command plan must include all required campaign markers."));
            }
        }

        if (!previewExport.Passed || previewExport.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal058.preview_export.invalid", "preview-export-campaign-payload", "Preview/export campaign payload must cover all three families."));
        }

        if (!packageProof.Passed || packageProof.PublicGamePackageSchemaChanged || packageProof.RuntimeSourceChanged || packageProof.WinFormsUiChanged)
        {
            diagnostics.Add(Error("goal058.package_compat.invalid", "campaign-package-compatibility-proof", "Package compatibility proof must pass without schema/runtime/UI mutation."));
        }

        foreach (var scenarioId in FullMediaBoundGeneratorCampaignVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal058.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> ValidateUnityProof(
        FullMediaBoundUnityCampaignCommandPlan commandPlan,
        FullMediaBoundCampaignUnityProof proof)
    {
        var diagnostics = new List<FullMediaBoundCampaignDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerProof.PlayerExecuted && !proof.PlayerProof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal058.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 058 marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorOrPlayerExecuted || proof.PlayerProof.UnityExitCode != 0 || proof.PlayerProof.PlayerExitCode != 0)
            {
                diagnostics.Add(Error("goal058.unity.proof_inconsistent", "unity-alpha-campaign-player-proof", "Passed Unity proof must have Unity/player execution and zero exit codes."));
            }
        }
        else if (string.IsNullOrWhiteSpace(proof.BlockerCode))
        {
            diagnostics.Add(Error("goal058.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry an exact blocker code."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics).Concat(proof.PlayerProof.Diagnostics));
    }

    public static IReadOnlyList<FullMediaBoundCampaignDiagnostic> Sort(IEnumerable<FullMediaBoundCampaignDiagnostic> diagnostics) =>
        FullMediaBoundGeneratorCampaignBuilder.SortDiagnostics(diagnostics);

    private static FullMediaBoundCampaignDiagnostic Error(string code, string target, string message) =>
        FullMediaBoundCampaignDiagnostic.Error(code, target, message);
}
