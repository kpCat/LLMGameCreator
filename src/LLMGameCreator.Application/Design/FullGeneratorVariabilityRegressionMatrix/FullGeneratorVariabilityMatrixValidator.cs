namespace LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityMatrixValidator
{
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> ValidateSourceManifest(FullGeneratorVariabilitySourceManifest manifest)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal059.gate.self_pass.forbidden", "matrix-source-manifest", "Goal 059 must not mark its own gate passed."));
        }

        if (!manifest.Goal058AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "full_media_bound_generator_campaign_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal059.preflight.goal058_handoff_missing", "matrix-source-manifest", "Goal 058 acceptance by user handoff is required before Goal 059."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == FullGeneratorVariabilityMatrixVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal059.gate.required_missing", "matrix-source-manifest", "Goal 059 gate must remain required."));
        }

        if (!manifest.Goal058ReportWasGreenProducedForReview || !manifest.Goal058UnityProofPassed)
        {
            diagnostics.Add(Error("goal059.source.goal058_not_green", "Goal058", "Goal 058 must be GREEN produced-for-review source evidence before Goal 059 handoff acceptance."));
        }

        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            if (!manifest.SelectedFamilyIds.Contains(familyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal059.source.family_missing", familyId, "Goal 059 must consume all required Goal 058 family ids."));
            }
        }

        foreach (var artifactFamily in new[]
        {
            "campaign_source_manifest",
            "campaign_plan",
            "family_run",
            "review_package_manifest",
            "preview_export_payload",
            "unity_command_plan",
            "unity_player_proof",
            "staging_family_command_plan",
            "staging_media_manifest"
        })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.ArtifactFamily == artifactFamily && item.Exists && item.HashMatches))
            {
                diagnostics.Add(Error("goal059.source.required_ref_missing", artifactFamily, "Required Goal 058 source reference is missing."));
            }
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> ValidateRows(
        FullGeneratorVariabilitySeedProfileMatrix matrix,
        IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        if (!matrix.Passed || matrix.Accepted || matrix.RowCount != 9 || rows.Count != 9)
        {
            diagnostics.Add(Error("goal059.matrix.invalid_count", "seed-profile-matrix", "Goal 059 requires a produced-for-review 3 family x 3 seed matrix."));
        }

        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in FullGeneratorVariabilityMatrixVocabulary.SeedIds)
            {
                if (!rows.Values.Any(item => item.FamilyId == familyId && item.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal059.matrix.row_missing", familyId + "/" + seedId, "Required family x seed row is missing."));
                }
            }
        }

        if (rows.Values.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() != rows.Count)
        {
            diagnostics.Add(Error("goal059.matrix.duplicate_row_id", "seed-profile-matrix", "Matrix row ids must be unique."));
        }

        foreach (var row in rows.Values)
        {
            if (string.IsNullOrWhiteSpace(row.DerivedCampaignHash)
                || row.SelectedMediaRefs.Count == 0
                || row.SelectedFamilyLoopRefs.Count == 0
                || row.SelectedPreviewExportRefs.Count == 0
                || row.DeterministicMarkerPlan.Count < 5
                || row.VariationDimensions.Count < 4)
            {
                diagnostics.Add(Error("goal059.matrix.row_incomplete", row.RowId, "Every row needs derived hash, source refs, media refs, loop refs, preview/export refs and marker plan."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> ValidateVarianceAndReplay(
        FullGeneratorVariabilityVarianceMetrics variance,
        FullGeneratorVariabilityReplayDeterminismProof replay)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        if (!variance.Passed
            || variance.RowCount != 9
            || variance.DistinctRowIdCount != 9
            || variance.DistinctDerivedCampaignHashCount != 9
            || variance.FamilyCount != 3
            || variance.SeedCount != 3
            || !variance.MediaBindingCoveragePassed
            || !variance.FamilyLoopMarkerCoveragePassed
            || variance.MinimumMeaningfulVariationDimensionsPerFamily < 2
            || variance.OverfitWarningCount != 0)
        {
            diagnostics.Add(Error("goal059.variance.invalid", "variance-metrics", "Variance metrics must prove non-overfit family x seed differences."));
        }

        if (!replay.Passed || replay.RowCount != 9 || replay.MatchedRowCount != 9)
        {
            diagnostics.Add(Error("goal059.replay.invalid", "replay-determinism-proof", "Every matrix row must replay with byte-stable JSON/hash."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> ValidateReviewAndPayloads(
        FullGeneratorVariabilityReviewPackageMatrixManifest review,
        FullGeneratorVariabilityPreviewExportMatrixPayload previewExport,
        FullGeneratorVariabilityUnityMatrixCommandPlan commandPlan,
        InvalidFullGeneratorVariabilityMatrix invalidMatrix)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        if (!review.Passed || review.Accepted || review.RowCount != 9 || review.MatrixRowRefs.Count != 9)
        {
            diagnostics.Add(Error("goal059.review.invalid", "review-package-matrix-manifest", "Review package matrix manifest must reference all matrix rows and remain accepted=false."));
        }

        if (!previewExport.Passed || previewExport.RowCount != 9)
        {
            diagnostics.Add(Error("goal059.preview_export.invalid", "preview-export-matrix-payload", "Preview/export matrix payload must cover all rows."));
        }

        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal059.unity.command_plan.invalid", "unity-alpha-matrix-command-plan", "Unity matrix command plan must cover all rows and remain accepted=false."));
        }

        foreach (var marker in RequiredMatrixGlobalMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal059.unity.command_plan.marker_missing", marker, "Unity command plan is missing a required global marker."));
            }
        }

        foreach (var scenarioId in FullGeneratorVariabilityMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal059.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal059.invalid.matrix_failed", "invalid-matrix-diagnostics", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> ValidateUnityProof(
        FullGeneratorVariabilityUnityMatrixCommandPlan commandPlan,
        FullGeneratorVariabilityUnityProof proof)
    {
        var diagnostics = new List<FullGeneratorVariabilityDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerProof.PlayerExecuted && !proof.PlayerProof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal059.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 059 matrix marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorOrPlayerExecuted || proof.PlayerProof.UnityExitCode != 0 || proof.PlayerProof.PlayerExitCode != 0)
            {
                diagnostics.Add(Error("goal059.unity.proof_inconsistent", "unity-alpha-matrix-player-proof", "Passed Unity proof must have Unity/player execution and zero exit codes."));
            }
        }
        else if (string.IsNullOrWhiteSpace(proof.BlockerCode))
        {
            diagnostics.Add(Error("goal059.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry an exact blocker code."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics).Concat(proof.PlayerProof.Diagnostics));
    }

    public static IReadOnlyList<FullGeneratorVariabilityDiagnostic> Sort(IEnumerable<FullGeneratorVariabilityDiagnostic> diagnostics) =>
        FullGeneratorVariabilityMatrixBuilder.SortDiagnostics(diagnostics);

    private static IReadOnlyList<string> RequiredMatrixGlobalMarkers() =>
    [
        "full_generator_matrix_loaded=true",
        "full_generator_matrix_completed=true"
    ];

    private static FullGeneratorVariabilityDiagnostic Error(string code, string target, string message) =>
        FullGeneratorVariabilityDiagnostic.Error(code, target, message);
}
