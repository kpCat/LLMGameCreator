namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialDetailValidator
{
    public IReadOnlyList<ConstrainedSpatialDiagnostic> ValidateSourceManifest(ConstrainedSpatialSourceManifest manifest)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal062.gate.self_pass.forbidden", "source-manifest", "Goal 062 must not mark its own gate passed."));
        }

        if (!manifest.Goal061AcceptedByUserHandoff
            || !manifest.PreflightGates.Any(item => item.GateId == "full_campaign_playable_review_package_rc_verification" && item.Status == "passed" && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal062.preflight.goal061_handoff_missing", "source-manifest", "Goal 061 acceptance by user handoff is required before Goal 062."));
        }

        if (!manifest.PreflightGates.Any(item => item.GateId == ConstrainedSpatialDetailVocabulary.FinalGate && item.Status == "required"))
        {
            diagnostics.Add(Error("goal062.gate.required_missing", "source-manifest", "Goal 062 gate must remain required."));
        }

        if (!manifest.Goal061ReviewPackageRcManifestPassed || !manifest.Goal061UnityProofPassed)
        {
            diagnostics.Add(Error("goal062.source.goal061_not_green", "Goal061", "Goal 061 must be GREEN and Unity-proven before spatial detail consumption."));
        }

        if (manifest.PackageRowCount != 9 || manifest.FamilyCount != 3 || manifest.SeedCount != 3)
        {
            diagnostics.Add(Error("goal062.source.matrix_counts_invalid", "source-manifest", "Goal 062 requires 9 rows across 3 families and 3 seeds."));
        }

        return Sort(diagnostics.Concat(manifest.Diagnostics).Concat(manifest.SourceArtifactRefs.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<ConstrainedSpatialDiagnostic> ValidateCatalogs(
        ConstrainedSpatialPaletteCatalog palette,
        ConstrainedSpatialRewriteRuleCatalog rewrite,
        ConstrainedSpatialConstraintRuleCatalog constraints)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        if (!palette.Passed || palette.Tiles.Any(tile => tile.Provenance != "in_house_fixture"))
        {
            diagnostics.Add(Error("goal062.palette.invalid", "spatial-palette-catalog", "Palette catalog must be in-house and cover required family semantics."));
        }

        if (!rewrite.Passed)
        {
            diagnostics.Add(Error("goal062.rules.rewrite_catalog_invalid", "rewrite-rule-catalog", "Rewrite rule records must cover entry, exit, objective, route connection and repair behaviors."));
        }

        if (!constraints.Passed)
        {
            diagnostics.Add(Error("goal062.constraints.catalog_invalid", "constraint-rule-catalog", "Constraint rule catalog must cover every palette tile with bounded retry/fallback budgets."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ConstrainedSpatialDiagnostic> ValidateSpatialRows(
        ConstrainedSpatialDetailMatrix matrix,
        IReadOnlyList<ConstrainedSpatialDetailRow> rows,
        ConstrainedSpatialPaletteCatalog palette)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        var tileIds = palette.Tiles.Select(item => item.TileId).ToHashSet(StringComparer.Ordinal);
        if (!matrix.Passed || matrix.Accepted || rows.Count != 9 || matrix.RowCount != 9)
        {
            diagnostics.Add(Error("goal062.matrix.invalid", "spatial-detail-matrix", "Spatial detail matrix must contain 9 produced-for-review rows."));
        }

        foreach (var familyId in ConstrainedSpatialDetailVocabulary.FamilyIds)
        {
            foreach (var seedId in ConstrainedSpatialDetailVocabulary.SeedIds)
            {
                if (!rows.Any(row => row.FamilyId == familyId && row.SeedId == seedId))
                {
                    diagnostics.Add(Error("goal062.matrix.row_missing", familyId + "/" + seedId, "Required family x seed spatial detail row is missing."));
                }
            }
        }

        foreach (var row in rows)
        {
            if (row.Cells.Count != row.Width * row.Height
                || row.Anchors.All(anchor => anchor.AnchorId != "entry")
                || row.Anchors.All(anchor => anchor.AnchorId != "exit")
                || row.Anchors.All(anchor => anchor.AnchorId != "objective"))
            {
                diagnostics.Add(Error("goal062.row.required_anchor_or_cell_missing", row.RowId, "Every row requires cells plus entry, objective and exit anchors."));
            }

            if (row.Cells.Any(cell => !tileIds.Contains(cell.TileId)))
            {
                diagnostics.Add(Error("goal062.row.invalid_tile_id", row.RowId, "Every cell tile id must be present in the spatial palette catalog."));
            }

            if (!row.ReachabilityProof.Reachable || !row.ReachabilityProof.RouteVerified)
            {
                diagnostics.Add(Error("goal062.row.unreachable", row.RowId, "Every row must prove entry/objective/exit and family-specific reachability."));
            }

            if (string.IsNullOrWhiteSpace(row.RowHash)
                || string.IsNullOrWhiteSpace(row.VarianceMetrics.VarianceMarker)
                || row.VarianceMetrics.MeaningfulMetricKeys.Count < 5)
            {
                diagnostics.Add(Error("goal062.row.variance_incomplete", row.RowId, "Every row requires hash and meaningful variance metrics."));
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ConstrainedSpatialDiagnostic> ValidateProofsAndPayloads(
        ConstrainedSpatialReachabilityProofMatrix reachability,
        ConstrainedSpatialRepairFallbackMatrix repairs,
        ConstrainedSpatialPreviewExportPayload preview,
        ConstrainedSpatialUnityCommandPlan commandPlan,
        InvalidConstrainedSpatialDetailDiagnosticsMatrix invalidMatrix)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        if (!reachability.Passed || reachability.RowCount != 9 || reachability.ReachableRowCount != 9)
        {
            diagnostics.Add(Error("goal062.reachability.matrix_invalid", "reachability-proof-matrix", "Reachability matrix must prove all 9 rows."));
        }

        if (!repairs.Passed || repairs.RowCount != 9 || repairs.ContradictionScenarioCount < 1)
        {
            diagnostics.Add(Error("goal062.repair.matrix_invalid", "spatial-repair-fallback-matrix", "Repair/fallback matrix must include all rows and contradiction diagnostics."));
        }

        if (!preview.Passed || preview.RowCount != 9)
        {
            diagnostics.Add(Error("goal062.preview.payload_invalid", "preview-export-spatial-payload", "Preview/export payload must cover all spatial detail rows."));
        }

        if (!commandPlan.Passed || commandPlan.Accepted || commandPlan.Rows.Count != 9)
        {
            diagnostics.Add(Error("goal062.unity.command_plan_invalid", "unity-spatial-detail-command-plan", "Unity command plan must cover all 9 rows and remain accepted=false."));
        }

        foreach (var marker in RequiredUnityMarkers())
        {
            if (!commandPlan.ExpectedPlayerMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal062.unity.marker_missing", marker, "Unity command plan is missing a required global marker."));
            }
        }

        foreach (var scenarioId in ConstrainedSpatialDetailVocabulary.RequiredInvalidScenarioIds)
        {
            if (!invalidMatrix.Scenarios.Any(item => item.ScenarioId == scenarioId && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0))
            {
                diagnostics.Add(Error("goal062.invalid.scenario_missing", scenarioId, "Required invalid/fake/leak scenario is missing or mismatched."));
            }
        }

        if (!invalidMatrix.Passed)
        {
            diagnostics.Add(Error("goal062.invalid.matrix_failed", "invalid-spatial-detail-diagnostics-matrix", "Invalid/fake/leak matrix must pass expected causal diagnostics."));
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<ConstrainedSpatialDiagnostic> ValidateUnityProof(
        ConstrainedSpatialUnityCommandPlan commandPlan,
        ConstrainedSpatialUnityProof proof)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        foreach (var marker in commandPlan.ExpectedPlayerMarkers)
        {
            if (proof.PlayerProof.PlayerExecuted && !proof.PlayerProof.MatchedMarkers.Contains(marker, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("goal062.unity.marker_missing", marker, "Executed Unity proof must include every required Goal 062 spatial-detail marker."));
            }
        }

        if (proof.Passed)
        {
            if (!proof.UnityEditorOrPlayerExecuted
                || proof.PlayerProof.UnityExitCode != 0
                || proof.PlayerProof.PlayerExitCode != 0
                || proof.PlayerProof.ProvenRowCount != 9)
            {
                diagnostics.Add(Error("goal062.unity.proof_inconsistent", "unity-spatial-detail-proof-summary", "Passed Unity proof must have Unity/player execution, zero exit codes and all 9 rows."));
            }
        }
        else if (string.IsNullOrWhiteSpace(proof.BlockerCode))
        {
            diagnostics.Add(Error("goal062.unity.blocker_missing", "unity-proof", "Non-passing Unity proof must carry an exact blocker code."));
        }

        return Sort(diagnostics.Concat(proof.Diagnostics).Concat(proof.PlayerProof.Diagnostics));
    }

    public static IReadOnlyList<ConstrainedSpatialDiagnostic> Sort(IEnumerable<ConstrainedSpatialDiagnostic> diagnostics) =>
        ConstrainedSpatialDetailSourceLoader.SortDiagnostics(diagnostics);

    private static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "spatial_detail_loaded=true",
        "review_package_proof=goal062",
        "constrained_spatial_detail_generation_verification=required"
    ];

    private static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Error(code, target, message);
}
